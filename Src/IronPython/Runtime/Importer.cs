/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Text;
using System.Diagnostics;

using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Compiler;
using IronPython.Compiler.Generation;
using IronPython.Runtime.Types;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Calls;
using IronPython.Hosting;
using System.Threading;
using IronPython.Runtime.Exceptions;

namespace IronPython.Runtime {
    
    /// <summary>
    /// Importer class - used for importing modules.  Used by Ops and __builtin__
    /// Singleton living on Python engine.
    /// </summary>
    public static class Importer {
        private static readonly DynamicSite<object, string, IAttributesCollection, IAttributesCollection, PythonTuple, int, object> _importSite = MakeImportSite();
        private static readonly DynamicSite<object, string, IAttributesCollection, IAttributesCollection, PythonTuple, object> _oldImportSite = MakeOldImportSite();
        internal const string ModuleReloadMethod = "PerformModuleReload";

        #region Internal API Surface

        /// <summary>
        /// Gateway into importing ... called from Ops.  Performs the initial import of
        /// a module and returns the module.
        /// </summary>
        public static object Import(CodeContext/*!*/ context, string fullName, PythonTuple from, int level) {
            Exception exLast = PythonOps.SaveCurrentException();
            try {
                if (level == -1) {
                    // no specific level provided, call the 4 param version so legacy code continues to work
                    return _oldImportSite.Invoke(context, FindImportFunction(context), fullName, Builtin.globals(context), Builtin.LocalsAsAttributesCollection(context), from);
                }

                // relative import or absolute import, in other words:
                //
                // from . import xyz
                // or 
                // from __future__ import absolute_import
                return _importSite.Invoke(context, FindImportFunction(context), fullName, Builtin.globals(context), Builtin.LocalsAsAttributesCollection(context), from, level);
            } finally {
                PythonOps.RestoreCurrentException(exLast);
            }

        }

        private static DynamicSite<object, string, IAttributesCollection, IAttributesCollection, PythonTuple, int, object> MakeImportSite() {
            // cant be FastDynamicSite because we need to flow our caller's true context because import is a meta-programming feature.
            return RuntimeHelpers.CreateSimpleCallSite<object, string, IAttributesCollection, IAttributesCollection, PythonTuple, int, object>();
        }

        private static DynamicSite<object, string, IAttributesCollection, IAttributesCollection, PythonTuple, object> MakeOldImportSite() {
            // cant be FastDynamicSite because we need to flow our caller's true context because import is a meta-programming feature.
            return RuntimeHelpers.CreateSimpleCallSite<object, string, IAttributesCollection, IAttributesCollection, PythonTuple, object>();
        }

        /// <summary>
        /// Gateway into importing ... called from Ops.  This is called after
        /// importing the module and is used to return individual items from
        /// the module.  The outer modules dictionary is then updated with the
        /// result.
        /// </summary>
        public static object ImportFrom(CodeContext/*!*/ context, object from, string name) {
            Exception exLast = PythonOps.SaveCurrentException();
            try {
                Scope scope = from as Scope;
                if (scope != null) {
                    object ret;
                    if (scope.TryGetName(context.LanguageContext, SymbolTable.StringToId(name), out ret)) {
                        return ret;
                    }

                    object path;
                    List listPath;
                    if (scope.TryGetName(context.LanguageContext, Symbols.Path, out path) && (listPath = path as List) != null) {
                        return ImportNestedModule(context, scope, name, listPath);
                    }
                } else {
                    // This is too lax, for example it allows from module.class import member
                    object ret;
                    if (PythonOps.TryGetBoundAttr(context, from, SymbolTable.StringToId(name), out ret)) {
                        return ret;
                    }
                }
            } finally {
                PythonOps.RestoreCurrentException(exLast);
            }
            throw PythonOps.ImportError("Cannot import name {0}", name);
        }


        private static object ImportModuleFrom(CodeContext/*!*/ context, object from, string name) {
            Scope scope = from as Scope;
            if (scope != null) {
                object path;
                List listPath;
                if (scope.TryGetName(context.LanguageContext, Symbols.Path, out path) && (listPath = path as List) != null) {
                    return ImportNestedModule(context, scope, name, listPath);
                }
            }

            NamespaceTracker ns = from as NamespaceTracker;
            if (ns != null) {
                object val;
                if (ns.TryGetValue(SymbolTable.StringToId(name), out val)) {
                    return val;
                }
            }

            throw PythonOps.ImportError("No module named {0}", name);
        }

        /// <summary>
        /// Called by the __builtin__.__import__ functions (general importing) and ScriptEngine (for site.py)
        /// 
        /// level indiciates whether to perform absolute or relative imports.
        ///     -1 indicates both should be performed
        ///     0 indicates only absolute imports should be performed
        ///     Positive numbers indicate the # of parent directories to search relative to the calling module
        /// </summary>        
        public static object ImportModule(CodeContext/*!*/ context, object globals, string/*!*/ modName, bool bottom, int level) {
            object newmod = null;
            string[] parts = modName.Split('.');

            if (level != 0) {
                // if importing a.b.c, import "a" first and then import b.c from a
                string name;    // name of the module we are to import in relation to the current module
                List path;      // path to search
                Scope parentScope;
                if (TryGetNameAndPath(context, globals, parts[0], level, out name, out path, out parentScope)) {
                    // import relative
                    if (!TryGetExistingOrMetaPathModule(context, name, path, out newmod)) {
                        newmod = ImportTopRelative(context, parts[0], name, path);
                        if (newmod != null && parentScope != null) {
                            parentScope.SetName(SymbolTable.StringToId(modName), newmod);
                        }
                    } else if (parts.Length == 1) {
                        // if we imported before having the assembly
                        // loaded and then loaded the assembly we want
                        // to make the assembly available now.

                        if (newmod is NamespaceTracker) {
                            context.ModuleContext.ShowCls = true;
                        }
                    }
                }
            }
            
            if (level <= 0) {
                if (newmod == null) {
                    newmod = ImportTopAbsolute(context, parts[0]);

                    if (newmod == null) {
                        return null;
                    }
                }
            }
            
            // now import the b.c etc.
            object next = newmod;
            string curName = parts[0];
            for (int i = 1; i < parts.Length; i++) {
                curName = curName + "." + parts[i];
                object tmpNext;
                if (TryGetExistingModule(context, curName, out tmpNext)) {
                    next = tmpNext;
                    continue;
                }
                next = ImportModuleFrom(context, next, parts[i]);
            }

            return bottom ? next : newmod;
        }

        private static object ImportTopRelative(CodeContext/*!*/ context, string/*!*/ name, string/*!*/ full, List/*!*/ path) {
            object importedScope = ImportFromPath(context, name, full, path);
            if (importedScope != null) {
                context.Scope.SetName(SymbolTable.StringToId(name), importedScope);
            }
            return importedScope;
        }

        /// <summary>
        /// Interrogates the importing module for __name__ and __path__, which determine
        /// whether the imported module (whose name is 'name') is being imported as nested
        /// module (__path__ is present) or as sibling.
        /// 
        /// For sibling import, the full name of the imported module is parent.sibling
        /// For nested import, the full name of the imported module is parent.module.nested
        /// where parent.module is the mod.__name__
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name">Name of the module to be imported</param>
        /// <param name="full">Output - full name of the module being imported</param>
        /// <param name="path">Path to use to search for "full"</param>
        /// <returns></returns>
         private static bool TryGetNameAndPath(CodeContext/*!*/ context, object globals, string name, int level, out string full, out List path, out Scope parentScope) {
           Debug.Assert(level != 0);   // shouldn't be here for absolute imports

            // Unless we can find enough information to perform relative import,
            // we are going to import the module whose name we got
            full = name;
            path = null;
            parentScope = null;

            // We need to get __name__ to find the name of the imported module.
            // If absent, fall back to absolute import
            object attribute;
            if (!TryGetGlobalValue(globals, Symbols.Name, out attribute)) {
                return false;
            }

            // And the __name__ needs to be string
            string modName = attribute as string;
            if (modName == null) {
                return false;
            }

            // If the module has __path__ (and __path__ is list), nested module is being imported
            // otherwise, importing sibling to the importing module

            if (TryGetGlobalValue(globals, Symbols.Path, out attribute) && (path = attribute as List) != null) {
                // found __path__, importing nested module. The actual name of the nested module
                // is the name of the mod plus the name of the imported module
                full = modName + "." + name;
                return true;
            }

            // importing sibling. The name of the imported module replaces
            // the last element in the importing module name
            string[] names = modName.Split('.');
            if (names.Length == 1) {
                // name doesn't include dot, only absolute import possible
                return false;
            }

            StringBuilder parentName = new StringBuilder(names[0]);

            if (level == -1) level = 1;
            for (int i = 1; i < names.Length - level; i++) {
                parentName.Append('.');
                parentName.Append(names[i]);
            }
            
            string pn = parentName.ToString();

            path = GetParentPathAndScope(context, pn, out parentScope);
            if (path != null) {
                if (String.IsNullOrEmpty(name)) {
                    full = pn;
                } else {
                    full = pn + "." + name;
                }
                return true;
            }

            // not enough information - absolute import
            return false;
        }

        private static bool TryGetGlobalValue(object globals, SymbolId symbol, out object attribute) {
            IAttributesCollection attrGlobals = globals as IAttributesCollection;
            if (attrGlobals != null) {
                if (!attrGlobals.TryGetValue(symbol, out attribute)) {
                    return false;
                }
            } else {
                // Python doesn't allow imports from arbitrary user mappings.
                attribute = null;
                return false;
            }
            return true;
        }

        public static void ReloadModule(CodeContext/*!*/ context, Scope/*!*/ scope) {
            PythonContext pc = PythonContext.GetContext(context);

            PythonModule module = pc.GetReloadableModule(scope);

            // We created the module and it only contains Python code. If the user changes
            // __file__ we'll reload from that file. 
            string fileName = module.GetFile() as string;

            // built-in module:
            if (fileName == null) {
                ReloadBuiltinModule(context, module);
                return;
            }

            string name = module.GetName() as string;
            if (name != null) {
                List path = null;
                // find the parent module and get it's __path__ property
                int dotIndex = name.LastIndexOf('.');
                if (dotIndex != -1) {
                    Scope parentScope;
                    path = GetParentPathAndScope(context, name.Substring(0, dotIndex), out parentScope);
                }

                object reloaded;
                if (TryLoadMetaPathModule(context, module.GetName() as string, path, out reloaded) && reloaded != null) {
                    return;
                }
            }

            SourceUnit sourceUnit = pc.TryGetSourceFileUnit(fileName, pc.DefaultEncoding, SourceCodeKind.File);

            if (sourceUnit == null) {
                throw PythonOps.SystemError("module source file not found");
            }

            sourceUnit.Execute(scope);
        }

        /// <summary>
        /// Given the parent module name looks up the __path__ property.
        /// </summary>
        private static List GetParentPathAndScope(CodeContext/*!*/ context, string/*!*/ parentModuleName, out Scope parentScope) {
            List path = null;
            object parentModule;
            parentScope = null;
            
            // Try lookup parent module in the sys.modules
            if (PythonContext.GetContext(context).SystemStateModules.TryGetValue(parentModuleName, out parentModule)) {
                // see if it's a module
                parentScope = parentModule as Scope;
                if (parentScope != null) {
                    object objPath;
                    // get its path as a List if it's there
                    if (parentScope.TryGetName(context.LanguageContext, Symbols.Path, out objPath)) {
                        path = objPath as List;
                    }
                }
            }
            return path;
        }

        private static void ReloadBuiltinModule(CodeContext/*!*/ context, PythonModule/*!*/ module) {
            Assert.NotNull(module);
            Debug.Assert(module.GetName() is string, "Module is reloadable only if its name is a non-null string");
            Type type;

            string name = (string)module.GetName();
            PythonContext pc = PythonContext.GetContext(context);

            if (!pc.Builtins.TryGetValue(name, out type)) {
                throw new NotImplementedException();
            }

            // TODO: is this correct?
            module.SetName(name);
            PythonModuleOps.PopulateModuleDictionary(pc, module.Scope.Dict, type);
        }

        /// <summary>
        /// Trys to get an existing module and if that fails fall backs to searching 
        /// </summary>
        private static bool TryGetExistingOrMetaPathModule(CodeContext/*!*/ context, string fullName, List path, out object ret) {
            if (TryGetExistingModule(context, fullName, out ret)) {
                return true;
            }

            return TryLoadMetaPathModule(context, fullName, path, out ret);
        }

        /// <summary>
        /// Attempts to load a module from sys.meta_path as defined in PEP 302.
        /// 
        /// The meta_path provides a list of importer objects which can be used to load modules before
        /// searching sys.path but after searching built-in modules.
        /// </summary>
        private static bool TryLoadMetaPathModule(CodeContext/*!*/ context, string fullName, List path, out object ret) {
            List metaPath = PythonContext.GetContext(context).GetSystemStateValue("meta_path") as List;
            if (metaPath != null) {
                foreach (object importer in metaPath) {
                    return FindAndLoadModuleFromImporter(context, importer, fullName, path, out ret);
                }
            }

            ret = null;
            return false;
        }

        /// <summary>
        /// Given a user defined importer object as defined in PEP 302 tries to load a module.
        /// 
        /// First the find_module(fullName, path) is invoked to get a loader, then load_module(fullName) is invoked
        /// </summary>
        private static bool FindAndLoadModuleFromImporter(CodeContext/*!*/ context, object importer, string fullName, List path, out object ret) {
            object loader;            
            if (PythonTypeOps.TryInvokeTernaryOperator(context, importer, fullName, path, SymbolTable.StringToId("find_module"), out loader) && loader != null) {
                if (PythonTypeOps.TryInvokeBinaryOperator(context, loader, fullName, SymbolTable.StringToId("load_module"), out ret) && ret != null) {
                    return true;
                }
            }

            ret = null;
            return false;
        }

        internal static bool TryGetExistingModule(CodeContext/*!*/ context, string/*!*/ fullName, out object ret) {
            // Python uses None/null as a key here to indicate a missing module
            if (PythonContext.GetContext(context).SystemStateModules.TryGetValue(fullName, out ret)) {
                return ret != null;
            }
            return false;
        }

        #endregion

        #region Private Implementation Details

        private static object ImportTopAbsolute(CodeContext/*!*/ context, string/*!*/ name) {
            object ret;
            if (TryGetExistingModule(context, name, out ret)) {
                if (IsReflected(ret)) {
                    // Even though we found something in sys.modules, we need to check if a
                    // clr.AddReference has invalidated it. So try ImportReflected again.
                    ret = ImportReflected(context, name) ?? ret;
                }

                NamespaceTracker rp = ret as NamespaceTracker;
                if (rp != null) {
                    context.ModuleContext.ShowCls = true;
                }

                return ret;
            }

            ret = ImportBuiltin(context, name);
            if (ret != null) return ret;

            if (TryLoadMetaPathModule(context, name, null, out ret)) {
                return ret;
            }

            List path;
            if (PythonContext.GetContext(context).TryGetSystemPath(out path)) {
                ret = ImportFromPath(context, name, name, path);
                if (ret != null) return ret;
            }

            ret = ImportReflected(context, name);
            if (ret != null) return ret;

            return null;
        }

        private static bool TryGetNestedModule(CodeContext/*!*/ context, Scope/*!*/ scope, string/*!*/ name, out object nested) {
            Assert.NotNull(context, scope, name);

            if (scope.TryGetName(context.LanguageContext, SymbolTable.StringToId(name), out nested)) {
                if (nested is Scope) return true;

                // This allows from System.Math import *
                PythonType dt = nested as PythonType;
                if (dt != null && dt.IsSystemType) return true;
            }
            return false;
        }

        private static object ImportNestedModule(CodeContext/*!*/ context, Scope/*!*/ scope, string name, List/*!*/ path) {
            object ret;

            PythonModule module = PythonContext.GetContext(context).EnsurePythonModule(scope);

            string fullName = CreateFullName(module.GetName() as string, name);

            if (TryGetExistingOrMetaPathModule(context, fullName, path, out ret)) {
                module.Scope.SetName(SymbolTable.StringToId(name), ret);
                return ret;
            }

            if (TryGetNestedModule(context, scope, name, out ret)) { return ret; }

            object importedScope = ImportFromPath(context, name, fullName, path);
            if (importedScope != null) {
                module.Scope.SetName(SymbolTable.StringToId(name), importedScope);
                return importedScope;
            }

            throw PythonOps.ImportError("cannot import {0} from {1}", name, module.GetName());
        }

        private static object FindImportFunction(CodeContext/*!*/ context) {
            object builtin, import;
            if (!context.Scope.ModuleScope.TryGetName(context.LanguageContext, Symbols.Builtins, out builtin)) {
                builtin = PythonContext.GetContext(context).BuiltinModuleInstance;
            }

            Scope scope = builtin as Scope;
            if (scope != null && scope.TryGetName(context.LanguageContext, Symbols.Import, out import)) {
                return import;
            }

            IAttributesCollection dict = builtin as IAttributesCollection;
            if (dict != null && dict.TryGetValue(Symbols.Import, out import)) {
                return import;
            }

            throw PythonOps.ImportError("cannot find __import__");
        }

        internal static object ImportBuiltin(CodeContext/*!*/ context, string/*!*/ name) {
            Assert.NotNull(context, name);

            PythonContext pc = PythonContext.GetContext(context);
            if (name == "sys") {
                return pc.SystemState;
            } else if (name == "clr") {
                context.ModuleContext.ShowCls = true;
                return pc.ClrModule;
            }

            PythonModule mod = pc.CreateBuiltinModule(name);
            if (mod != null) {
                pc.PublishModule(name, mod);
                return mod.Scope;
            }

            return null;
        }

        private static object ImportReflected(CodeContext/*!*/ context, string/*!*/ name) {
            object ret;
            if (!PythonContext.GetContext(context).DomainManager.Globals.TryGetName(SymbolTable.StringToId(name), out ret)) {
                ret = PythonContext.GetContext(context).DomainManager.UseModule(name);
            }

            MemberTracker res = ret as MemberTracker;
            if (res != null) {
                context.ModuleContext.ShowCls = true;
                object realRes = res;

                switch (res.MemberType) {
                    case TrackerTypes.Type: realRes = DynamicHelpers.GetPythonTypeFromType(((TypeTracker)res).Type); break;
                    case TrackerTypes.Field: realRes = PythonTypeOps.GetReflectedField(((FieldTracker)res).Field); break;
                    case TrackerTypes.Event: realRes = PythonTypeOps.GetReflectedEvent((EventTracker)res); break;
                    case TrackerTypes.Method:
                        MethodTracker mt = res as MethodTracker;
                        realRes = PythonTypeOps.GetBuiltinFunction(mt.DeclaringType, mt.Name, new MemberInfo[] { mt.Method });
                        break;
                }
             
                return realRes;
            }
            return ret;
        }

        private static bool IsReflected(object module) {
            // corresponds to the list of types that can be returned by ImportReflected
            return module is MemberTracker
                || module is PythonType
                || module is ReflectedEvent
                || module is ReflectedField
                || module is BuiltinFunction;
        }

        /// <summary>
        /// Initializes the specified module and returns the user-exposable PythonModule.
        /// </summary>
        internal static void InitializeModule(PythonContext/*!*/ context, string/*!*/ fullName, PythonModule/*!*/ module, ScriptCode/*!*/ code, bool executeModule) {
            Assert.NotNull(fullName, module, code);

            //Put this in modules dict so we won't reload with circular imports
            context.PublishModule(fullName, module);
            bool success = false;
            try {
                if (executeModule) {
                    code.Run(module.Scope, module);
                }

                success = true;
            } finally {
                if (!success) {
                    context.SystemStateModules.Remove(fullName);
                }
            }
        }

        private static string CreateFullName(string/*!*/ baseName, string name) {
            if (baseName == null || baseName.Length == 0 || baseName == "__main__") {
                return name;
            }
            return baseName + "." + name;
        }

        #endregion

        private static object ImportFromPath(CodeContext/*!*/ context, string/*!*/ name, string/*!*/ fullName, List/*!*/ path) {
            Assert.NotNull(context, name, fullName, path);

            IDictionary<object, object> importCache = PythonContext.GetContext(context).GetSystemStateValue("path_importer_cache") as IDictionary<object, object>;

            if (importCache == null) {
                return null;
            }

            foreach (object dirname in path) {
                string str;

                if (Converter.TryConvertToString(dirname, out str) && str != null) {  // ignore non-string
                    object importer;
                    if (!importCache.TryGetValue(str, out importer)) {
                        importCache[str] = importer = FindImporterForPath(context, str);
                    }

                    if (importer != null) {
                        // user defined importer object, get the loader and use it.
                        object ret;
                        if (FindAndLoadModuleFromImporter(context, importer, fullName, null, out ret)) {
                            return ret;
                        }
                    } else {
                        // default behavior
                        PythonModule module;
                        string pathname = Path.Combine(str, name);

                        module = LoadPackageFromSource(context, fullName, pathname);
                        if (module != null) {
                            return module.Scope;
                        }

                        string filename = pathname + ".py";
                        module = LoadModuleFromSource(context, fullName, filename);
                        if (module != null) {
                            return module.Scope;
                        }
                    }
                }
            }
            
            return null;
        }

        /// <summary>
        /// Finds a user defined importer for the given path or returns null if no importer
        /// handles this path.
        /// </summary>
        private static object FindImporterForPath(CodeContext/*!*/ context, string str) {
            List pathHooks = PythonContext.GetContext(context).GetSystemStateValue("path_hooks") as List;

            foreach (object hook in pathHooks) {
                try {
                    object handler = PythonCalls.Call(hook, str);

                    if (handler != null) {
                        return handler;
                    }
                } catch (ImportException) {
                    // we can't handle the path
                }

            }
            return null;
        }

        private static PythonModule LoadModuleFromSource(CodeContext/*!*/ context, string/*!*/ name, string/*!*/ path) {
            Assert.NotNull(context, name, path);

            if (!PathMatchesFileSystemCasing(context, path)) {
                return null;
            }
            
            PythonContext pc = PythonContext.GetContext(context);
            SourceUnit sourceUnit = pc.TryGetSourceFileUnit(path, pc.DefaultEncoding, SourceCodeKind.File);
            if (sourceUnit == null) {
                return null;
            }
            return LoadFromSourceUnit(context, sourceUnit, name, sourceUnit.Path);
        }

        private static bool PathMatchesFileSystemCasing(CodeContext context, string path) {
#if !SILVERLIGHT
            // check for a match in the case of the filename, unfortunately we can't do this
            // in Silverlight becauase there's no way to get the original filename.

            PlatformAdaptationLayer pal = context.LanguageContext.DomainManager.PAL;
            string dir = Path.GetDirectoryName(path);
            if (!pal.DirectoryExists(dir)) {
                return false;
            }

            try {
                string file = Path.GetFileName(path);
                string[] files = pal.GetFiles(dir, file);
                if (files.Length != 1) {                    
                    return false;
                }

                Debug.Assert(files[0].Length > file.Length);
                if (String.Compare(files[0], files[0].Length - file.Length, file, 0, file.Length) != 0) {
                    return false;
                }
            } catch (IOException) {
                return false;
            }
#endif
            return true;
        }

        private static PythonModule LoadPackageFromSource(CodeContext/*!*/ context, string/*!*/ name, string/*!*/ path) {
            Assert.NotNull(context, name, path);
            return LoadModuleFromSource(context, name, Path.Combine(path, "__init__.py"));
        }

        private static PythonModule/*!*/ LoadFromSourceUnit(CodeContext/*!*/ context, SourceUnit/*!*/ sourceCode, string/*!*/ name, string/*!*/ path) {
            Assert.NotNull(sourceCode, name, path);
            return PythonContext.GetContext(context).CompileModule(path, name, sourceCode, ModuleOptions.Initialize | ModuleOptions.Optimized | ModuleOptions.PublishModule, false);
        }
    }
}
