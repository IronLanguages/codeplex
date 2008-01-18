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
using Microsoft.Scripting.Utils;

using IronPython.Compiler;
using IronPython.Compiler.Generation;
using IronPython.Runtime.Types;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Calls;
using IronPython.Hosting;
using System.Threading;

namespace IronPython.Runtime {
    
    /// <summary>
    /// Importer class - used for importing modules.  Used by Ops and __builtin__
    /// Singleton living on Python engine.
    /// </summary>
    public sealed class Importer {
        private readonly PythonContext/*!*/ _context;
        private PythonModule _clrModule;

        private static DynamicSite<object, string, IAttributesCollection, IAttributesCollection, PythonTuple, object> _importSite = MakeImportSite();

        internal Importer(PythonContext/*!*/ context) {
            Assert.NotNull(context);
            _context = context;
        }

        #region Internal API Surface

        /// <summary>
        /// Gateway into importing ... called from Ops.  Performs the initial import of
        /// a module and returns the module.
        /// </summary>
        public object Import(CodeContext/*!*/ context, string fullName, PythonTuple from) {
            Exception exLast = PythonOps.SaveCurrentException();
            try {
                return _importSite.Invoke(context, FindImportFunction(context), fullName, Builtin.globals(context), Builtin.locals(context), from);
            } finally {
                PythonOps.RestoreCurrentException(exLast);
            }

        }

        private static DynamicSite<object, string, IAttributesCollection, IAttributesCollection, PythonTuple, object> MakeImportSite() {
            // cant be FastDynamicSite because we need to flow our caller's true context because import is a meta-programming feature.
            return RuntimeHelpers.CreateSimpleCallSite<object, string, IAttributesCollection, IAttributesCollection, PythonTuple, object>();
        }

        /// <summary>
        /// Gateway into importing ... called from Ops.  This is called after
        /// importing the module and is used to return individual items from
        /// the module.  The outer modules dictionary is then updated with the
        /// result.
        /// </summary>
        public object ImportFrom(CodeContext/*!*/ context, object from, string name) {
            Exception exLast = PythonOps.SaveCurrentException();
            try {
                Scope scope = from as Scope;
                if (scope != null) {
                    object ret;
                    if (scope.TryGetName(context.LanguageContext, SymbolTable.StringToId(name), out ret)) {
                        return ret;
                    }

                    object path;
                    if (scope.TryGetName(context.LanguageContext, Symbols.Path, out path)) {
                        return ImportNestedModule(context, scope, name);
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


        private object ImportModuleFrom(CodeContext/*!*/ context, object from, string name) {
            Scope scope = from as Scope;
            if (scope != null) {
                object path;
                if (scope.TryGetName(context.LanguageContext, Symbols.Path, out path)) {
                    return ImportNestedModule(context, scope, name);
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
        /// </summary>        
        public object ImportModule(CodeContext/*!*/ context, string/*!*/ modName, bool bottom) {
            object newmod = null;
            string[] parts = modName.Split('.');

            // if importing a.b.c, import "a" first and then import b.c from a
            string name;    // name of the module we are to import in relation to the current module
            List path;      // path to search
            if (TryGetNameAndPath(context, parts[0], out name, out path)) {
                // import relative
                if (!TryGetExistingModule(name, out newmod)) {
                    newmod = ImportTopRelative(context, parts[0], name, path);
                } else if (parts.Length == 1) {
                    // if we imported before having the assembly
                    // loaded and then loaded the assembly we want
                    // to make the assembly available now.

                    if (newmod is NamespaceTracker) {
                        context.ModuleContext.ShowCls = true;
                    }
                }
            }

            if (newmod == null) {
                newmod = ImportTopAbsolute(context, parts[0]);
                
                if (newmod == null) {
                    return null;
                }
            }

            // now import the b.c etc.
            object next = newmod;
            string curName = parts[0];
            for (int i = 1; i < parts.Length; i++) {
                curName = curName + "." + parts[i];
                object tmpNext;
                if (TryGetExistingModule(curName, out tmpNext)) {
                    next = tmpNext;
                    continue;
                }
                next = ImportModuleFrom(context, next, parts[i]);
            }

            return bottom ? next : newmod;
        }

        private Scope ImportTopRelative(CodeContext/*!*/ context, string/*!*/ name, string/*!*/ full, List/*!*/ path) {
            Scope importedScope = ImportFromPath(context, name, full, path);
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
        private bool TryGetNameAndPath(CodeContext/*!*/ context, string name, out string full, out List path) {
            // Unless we can find enough information to perform relative import,
            // we are going to import the module whose name we got
            full = name;
            path = null;

            // We need to get __name__ to find the name of the imported module.
            // If absent, fall back to absolute import
            object attribute;
            if (!context.Scope.ModuleScope.TryLookupName(context.LanguageContext, Symbols.Name, out attribute)) {
                return false;
            }

            // And the __name__ needs to be string
            string modName = attribute as string;
            if (modName == null) {
                return false;
            }

            // If the module has __path__ (and __path__ is list), nested module is being imported
            // otherwise, importing sibling to the importing module

            if (context.Scope.ModuleScope.TryLookupName(context.LanguageContext, Symbols.Path, out attribute) && (path = attribute as List) != null) {
                // found __path__, importing nested module. The actual name of the nested module
                // is the name of the mod plus the name of the imported module
                full = modName + "." + name;
                return true;
            }

            // importing sibling. The name of the imported module replaces
            // the last element in the importing module name
            int lastDot = modName.LastIndexOf('.');
            if (lastDot < 0) {
                // name doesn't include dot, only absolute import possible
                return false;
            }

            string parentName = modName.Substring(0, lastDot);
            object parentObject;
            // Try lookup parent module in the sys.modules
            if (!_context.SystemState.modules.TryGetValue(parentName, out parentObject)) {
                // parent module not found in sys.modules, fallback to absolute import
                return false;
            }

            Scope parentScope = parentObject as Scope;
            if (parentScope == null) {
                // the sys.module entry is not module, fallback to absolute import
                return false;
            }

            // The parentModule now needs to have __path__ - list
            if (parentScope.TryGetName(context.LanguageContext, Symbols.Path, out attribute) && (path = attribute as List) != null) {
                // combine the module names
                full = parentName + "." + name;
                return true;
            }

            // not enough information - absolute import
            return false;
        }

        public void ReloadModule(Scope/*!*/ scope) {
            PythonModule module = DefaultContext.DefaultPythonContext.GetReloadableModule(scope);

            // We created the module and it only contains Python code. If the user changes
            // __file__ we'll reload from that file. 
            string fileName = module.GetFile() as string;

            // built-in module:
            if (fileName == null) {
                ReloadBuiltinModule(module);
                return;
            }

            SourceUnit sourceUnit = _context.TryGetSourceFileUnit(fileName, _context.SystemState.DefaultEncoding, SourceCodeKind.File);

            if (sourceUnit == null) {
                throw PythonOps.SystemError("module source file not found");
            }

            ScriptCode compiledCode = _context.CompileSourceCode(sourceUnit);
            compiledCode.Run(scope);
        }

        private void ReloadBuiltinModule(PythonModule/*!*/ module) {
            Assert.NotNull(module);
            Debug.Assert(module.GetName() is string, "Module is reloadable only if its name is a non-null string");
            Type type;

            string name = (string)module.GetName();

            if (!_context.SystemState.Builtins.TryGetValue(name, out type)) {
                throw new NotImplementedException();
            }

            // TODO: is this correct?
            module.Scope.Clear();
            module.SetName(name);
            PythonModuleOps.PopulateModuleDictionary(module.Scope.Dict, type);
        }

        internal bool TryGetExistingModule(string/*!*/ fullName, out object ret) {
            // Python uses None/null as a key here to indicate a missing module
            if (_context.SystemState.modules.TryGetValue(fullName, out ret)) {
                return ret != null;
            }
            return false;
        }

        #endregion

        #region Private Implementation Details

        private object ImportTopAbsolute(CodeContext/*!*/ context, string/*!*/ name) {
            object ret;
            if (TryGetExistingModule(name, out ret)) {
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

            ret = ImportFromPath(context, name, name, _context.SystemState.path);
            if (ret != null) return ret;

            ret = ImportReflected(context, name);
            if (ret != null) return ret;

            return null;
        }

        private bool TryGetNestedModule(CodeContext/*!*/ context, Scope/*!*/ scope, string/*!*/ name, out object nested) {
            Assert.NotNull(context, scope, name);

            if (scope.TryGetName(context.LanguageContext, SymbolTable.StringToId(name), out nested)) {
                if (nested is Scope) return true;

                // This allows from System.Math import *
                PythonType dt = nested as PythonType;
                if (dt != null && dt.IsSystemType) return true;
            }
            return false;
        }

        private object ImportNestedModule(CodeContext/*!*/ context, Scope/*!*/ scope, string name) {
            object ret;
            if (TryGetNestedModule(context, scope, name, out ret)) { return ret; }

            PythonModule module = _context.EnsurePythonModule(scope);

            string baseName;
            List path = ResolveSearchPath(context, module, out baseName);
            string fullName = CreateFullName(baseName, name);

            if (TryGetExistingModule(fullName, out ret)) {
                return ret;
            }

            if (path != null) {
                Scope importedScope = ImportFromPath(context, name, fullName, path);
                if (importedScope != null) {
                    module.Scope.SetName(SymbolTable.StringToId(name), importedScope);
                    return importedScope;
                }
            }

            throw PythonOps.ImportError("cannot import {0} from {1}", name, module.GetName());
        }

        private object FindImportFunction(CodeContext/*!*/ context) {
            object builtin, import;
            if (!context.Scope.ModuleScope.TryGetName(context.LanguageContext, Symbols.Builtins, out builtin)) {
                builtin = SystemState.Instance.BuiltinModuleInstance;
            }

            Scope scope = builtin as Scope;
            if (scope != null && scope.TryGetName(context.LanguageContext, Symbols.Import, out import)) {
                return import;
            }

            throw PythonOps.ImportError("cannot find __import__");
        }

        internal object ImportBuiltin(CodeContext/*!*/ context, string/*!*/ name) {
            Assert.NotNull(context, name);

            if (name == "sys") {
                return _context.SystemState;
            } else if (name == "clr") {
                if (_clrModule == null) {
                    Interlocked.CompareExchange<PythonModule>(
                        ref _clrModule,
                        CreateBuiltinModule(name),
                        null);
                }

                context.ModuleContext.ShowCls = true;
                return _clrModule.Scope;
            } else {
                PythonModule mod = CreateBuiltinModule(name);
                if (mod != null) {
                    _context.PublishModule(name, mod);
                    return mod.Scope;
                }
            }
                                 
            return null;
        }

        private PythonModule CreateBuiltinModule(string name) {
            Type type;
            if (_context.SystemState.Builtins.TryGetValue(name, out type)) {
                // RuntimeHelpers.RunClassConstructor
                // run the type's .cctor before doing any custom reflection on the type.
                // This allows modules to lazily initialize PythonType's to custom values
                // rather than having them get populated w/ the ReflectedType.  W/o this the
                // cctor runs after we've done a bunch of reflection over the type that doesn't
                // force the cctor to run.
                System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(type.TypeHandle);
                return _context.CreateBuiltinModule(name, type);
            }

            return null;
        }

        private object ImportReflected(CodeContext/*!*/ context, string/*!*/ name) {
            object ret;
            if (!context.LanguageContext.DomainManager.Globals.TryGetName(SymbolTable.StringToId(name), out ret)) {
                ret = ScriptDomainManager.CurrentManager.UseModule(name);
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
        internal void InitializeModule(string/*!*/ fullName, PythonModule/*!*/ module, ScriptCode/*!*/ code, bool executeModule) {
            Assert.NotNull(fullName, module, code);

            //Put this in modules dict so we won't reload with circular imports
            _context.PublishModule(fullName, module);
            bool success = false;
            try {
                if (executeModule) {
                    code.Run(module.Scope, module);
                }

                success = true;
            } finally {
                if (!success) {
                    _context.SystemState.modules.Remove(fullName);
                }
            }
        }

        private List ResolveSearchPath(CodeContext/*!*/ context, PythonModule/*!*/ module, out string baseName) {
            baseName = module.GetName() as string;

            // TODO: is this precise?
            if (baseName == null) {
                return null;
            }

            object path;            
            if (!module.Scope.TryGetName(context.LanguageContext, Symbols.Path, out path)) {
                List basePath = path as List;
                for (; ; ) {
                    int lastDot = baseName.LastIndexOf('.');
                    if (lastDot < 0) {
                        baseName = null;
                        break;
                    }

                    baseName = baseName.Substring(0, lastDot);
                    object package = _context.SystemState.modules[baseName];
                    if (PythonOps.TryGetBoundAttr(package, Symbols.Path, out path)) {
                        if (path is List) {
                            basePath = (List)path;
                        }
                        break;
                    }
                }
                return basePath;
            }

            return path as List; // trouble if __path__ is not a List
        }

        private string CreateFullName(string/*!*/ baseName, string name) {
            if (baseName == null || baseName.Length == 0 || baseName == "__main__") {
                return name;
            }
            return baseName + "." + name;
        }

        #endregion

        private Scope ImportFromPath(CodeContext/*!*/ context, string/*!*/ name, string/*!*/ fullName, List/*!*/ path) {
            Assert.NotNull(context, name, fullName, path);

            foreach (object dirname in path) {
                string str;
                if (Converter.TryConvertToString(dirname, out str) && str != null) {  // ignore non-string
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

            return null;
        }

        private PythonModule LoadModuleFromSource(CodeContext/*!*/ context, string/*!*/ name, string/*!*/ path) {
            Assert.NotNull(context, name, path);

            SourceUnit sourceUnit = _context.TryGetSourceFileUnit(path, _context.SystemState.DefaultEncoding, SourceCodeKind.File);
            if (sourceUnit == null) {
                return null;
            }
            return LoadFromSourceUnit(sourceUnit, name, path);
        }

        private PythonModule LoadPackageFromSource(CodeContext/*!*/ context, string/*!*/ name, string/*!*/ path) {
            Assert.NotNull(context, name, path);
            return LoadModuleFromSource(context, name, Path.Combine(path, "__init__.py"));
        }

        private PythonModule/*!*/ LoadFromSourceUnit(SourceUnit/*!*/ sourceCode, string/*!*/ name, string/*!*/ path) {
            Assert.NotNull(sourceCode, name, path);
            return _context.CompileModule(path, name, sourceCode, ModuleOptions.Initialize | ModuleOptions.Optimized | ModuleOptions.PublishModule, false);
        }
    }
}
