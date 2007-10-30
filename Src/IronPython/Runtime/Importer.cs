/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
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

namespace IronPython.Runtime {
    
    /// <summary>
    /// Importer class - used for importing modules.  Used by Ops and __builtin__
    /// Singleton living on Python engine.
    /// </summary>
    public sealed class Importer {
        private PythonEngine _engine;

        // TODO: for convenience now, could be removed when we have _engine : PythonEngine
        private SystemState SystemState {
            get {
                return _engine.SystemState;
            }
        }

        private static DynamicSite<object, string, IAttributesCollection, IAttributesCollection, PythonTuple, object> _importSite = MakeImportSite();

        internal Importer(PythonEngine engine) {
            Debug.Assert(engine != null);
            _engine = engine;
        }

        #region Internal API Surface

        /// <summary>
        /// Gateway into importing ... called from Ops.  Performs the initial import of
        /// a module and returns the module.
        /// </summary>
        public object Import(CodeContext context, string fullName, PythonTuple from) {
            return _importSite.Invoke(context, FindImportFunction(context), fullName, Builtin.globals(context), Builtin.locals(context), from);
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
        public object ImportFrom(CodeContext context, object mod, string name) {
            ScriptModule from = mod as ScriptModule;
            if (from != null) {
                object ret;
                if (from.Scope.TryGetName(context.LanguageContext, SymbolTable.StringToId(name), out ret)) {
                    return ret;
                }
                   
                object path;
                if (from.Scope.TryGetName(context.LanguageContext, Symbols.Path, out path)) {
                    return ImportNestedModule(context, from, name);
                }
            } else {
                // This is too lax, for example it allows from module.class import member
                object ret;
                if (PythonOps.TryGetBoundAttr(context, mod, SymbolTable.StringToId(name), out ret)) {
                    return ret;
                }
            }
            throw PythonOps.ImportError("Cannot import name {0}", name);
        }


        private object ImportModuleFrom(CodeContext context, object mod, string name) {
            ScriptModule from = mod as ScriptModule;
            if (from != null) {
                object path;
                if (from.Scope.TryGetName(context.LanguageContext, Symbols.Path, out path)) {
                    return ImportNestedModule(context, from, name);
                }
            }

            NamespaceTracker rp = mod as NamespaceTracker;
            if (rp != null) {
                object val;
                if (rp.TryGetValue(SymbolTable.StringToId(name), out val)) {
                    return val;
                }
            }

            throw PythonOps.ImportError("No module named {0}", name);
        }

        /// <summary>
        /// Called by the __builtin__.__import__ functions (general importing) and PythonEngine (for site.py)
        /// 
        /// Returns a PythonModule.
        /// </summary>        
        public object ImportModule(CodeContext context, string modName, bool bottom) {
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
                
                // fallback to DLR way of resolving source units:
                if (newmod == null) {
                    ScriptModule mod = ScriptDomainManager.CurrentManager.UseModule(modName);
                    if (mod != null) {
                        return InitializeModule(modName, mod, false);
                    }

                }

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

        private object ImportTopRelative(CodeContext context, string name, string full, List path) {
            object newmod = ImportFromPath(context, name, full, path);
            if (newmod != null) {
                context.Scope.SetName(SymbolTable.StringToId(name), newmod);
            }
            return newmod;
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
        private bool TryGetNameAndPath(CodeContext context, string name, out string full, out List path) {
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
            if (!SystemState.modules.TryGetValue(parentName, out parentObject)) {
                // parent module not found in sys.modules, fallback to absolute import
                return false;
            }

            ScriptModule parentModule;
            if ((parentModule = parentObject as ScriptModule) == null) {
                // the sys.module entry is not module, fallback to absolute import
                return false;
            }

            // The parentModule now needs to have __path__ - list
            if (parentModule.Scope.TryGetName(context.LanguageContext, Symbols.Path, out attribute) && (path = attribute as List) != null) {
                // combine the module names
                full = parentName + "." + name;
                return true;
            }

            // not enough information - absolute import
            return false;
        }

        internal void ReloadBuiltin(ScriptModule module) {
            Type ty;

            if (SystemState.Builtins.TryGetValue(module.ModuleName, out ty)) {
                if (typeof(CustomSymbolDictionary).IsAssignableFrom(ty)) {
                    CustomSymbolDictionary dict = (CustomSymbolDictionary)ty.GetConstructor(ArrayUtils.EmptyTypes).Invoke(ArrayUtils.EmptyObjects);
                    //@todo share logic to copy old values in when not already there from reload
                    module.Execute();
                } else {
                    PythonType type = DynamicHelpers.GetPythonTypeFromType(ty);

                    foreach (SymbolId attrName in type.GetMemberNames(DefaultContext.DefaultCLS)) {
                        module.Scope.SetName(attrName, type.GetBoundMember(DefaultContext.DefaultCLS, null, attrName));
                    }
                    module.Execute();
                }
                return;
            }
            throw new NotImplementedException();
        }

        internal bool TryGetExistingModule(string fullName, out object ret) {
            // Python uses None/null as a key here to indicate a missing module
            if (SystemState.modules.TryGetValue(fullName, out ret)) {
                return ret != null;
            }
            return false;
        }

        #endregion

        #region Private Implementation Details

        private object ImportTopAbsolute(CodeContext context, string name) {
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

            ret = ImportFromPath(context, name, name, SystemState.path);
            if (ret != null) return ret;

            ret = ImportReflected(context, name);
            if (ret != null) return ret;

            return null;
        }

        private bool TryGetNestedModule(CodeContext context, ScriptModule mod, string name, out object nested) {
            if (mod.Scope.TryGetName(context.LanguageContext, SymbolTable.StringToId(name), out nested)) {
                if (nested is ScriptModule) return true;

                // This allows from System.Math import *
                PythonType dt = nested as PythonType;
                if (dt != null && dt.IsSystemType) return true;
            }
            return false;
        }

        private object ImportNestedModule(CodeContext context, ScriptModule mod, string name) {
            object ret;
            if (TryGetNestedModule(context, mod, name, out ret)) { return ret; }

            string baseName;
            List path = ResolveSearchPath(context, mod, out baseName);
            string fullName = CreateFullName(baseName, name);

            if (TryGetExistingModule(fullName, out ret)) {
                return ret;
            }

            if (path != null) {
                ret = ImportFromPath(context, name, fullName, path);
                if (ret != null) {
                    mod.Scope.SetName(SymbolTable.StringToId(name), ret);
                    return ret;
                }
            }

            throw PythonOps.ImportError("cannot import {0} from {1}", name, mod.ModuleName);
        }

        private static void SetImported(CodeContext context, string name, object value) {
            context.Scope.SetName(SymbolTable.StringToId(name), value);
        }

        private object FindImportFunction(CodeContext context) {
            object builtin, import;
            if (!context.Scope.ModuleScope.TryGetName(context.LanguageContext, Symbols.Builtins, out builtin)) {
                builtin = SystemState.Instance.BuiltinModuleInstance;
            }

            ScriptModule sm = builtin as ScriptModule;            
            if (sm != null && sm.Scope.TryGetName(context.LanguageContext, Symbols.Import, out import)) {
                return import;
            }

            throw PythonOps.ImportError("cannot find __import__");
        }

        internal object ImportBuiltin(CodeContext context, string name) {
            RuntimeHelpers.TopNamespace.Initialize();

            if (name.Equals("sys")) return SystemState;
            if (name.Equals("clr")) {
                context.ModuleContext.ShowCls = true;
                return SystemState.Instance.ClrModule;
            }
            Type ty;
            if (SystemState.Builtins.TryGetValue(name, out ty)) {
                // RuntimeHelpers.RunClassConstructor
                // run the type's .cctor before doing any custom reflection on the type.
                // This allows modules to lazily initialize PythonType's to custom values
                // rather than having them get populated w/ the ReflectedType.  W/o this the
                // cctor runs after we've done a bunch of reflection over the type that doesn't
                // force the cctor to run.
                System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(ty.TypeHandle);
                ScriptModule ret = PythonModuleOps.MakePythonModule(name, ty);
                SystemState.modules[name] = ret;                
                return ret;
            }
            return null;
        }

        private object ImportReflected(CodeContext context, string name) {
            MemberTracker res = RuntimeHelpers.TopNamespace.TryGetPackageAny(name);
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
             
                SystemState.modules[name] = realRes;                
                return realRes;
            }
            return res;
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
        internal ScriptModule InitializeModule(string fullName, ScriptModule smod, bool executeModule) {
            //Put this in modules dict so we won't reload with circular imports
            SystemState.modules[fullName] = smod;            
            bool success = false;
            try {
                if(executeModule) smod.Execute();
                success = true;
            } finally {
                if (!success) SystemState.modules.Remove(fullName);
            }

            return smod;
        }

        private List ResolveSearchPath(CodeContext context, ScriptModule mod, out string baseName) {
            baseName = mod.ModuleName;

            object path;            
            if (!mod.Scope.TryGetName(context.LanguageContext, Symbols.Path, out path)) {
                List basePath = path as List;
                for (; ; ) {
                    int lastDot = baseName.LastIndexOf('.');
                    if (lastDot < 0) {
                        baseName = null;
                        break;
                    }

                    baseName = baseName.Substring(0, lastDot);
                    object package = SystemState.modules[baseName];
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

        private string CreateFullName(string baseName, string name) {
            if (baseName == null || baseName.Length == 0 || baseName == "__main__") {
                return name;
            }
            return baseName + "." + name;
        }

        #endregion

        private ScriptModule ImportFromPath(CodeContext context, string name, string fullName, List path) {
            ScriptModule ret = null;
            foreach (object dirname in path) {
                string str;
                if (Converter.TryConvertToString(dirname, out str) && str != null) {  // ignore non-string
                    string pathname = Path.Combine(str, name);

                    ret = LoadPackageFromSource(context, fullName, pathname);
                    if (ret != null) {
                        return ret;
                    }

                    string filename = pathname + ".py";
                    ret = LoadModuleFromSource(context, fullName, filename);
                    if (ret != null) {
                        return ret;
                    }
                }
            }
            return ret;
        }

        private ScriptModule LoadModuleFromSource(CodeContext context, string name, string path) {
            SourceUnit sourceUnit = ScriptDomainManager.CurrentManager.Host.TryGetSourceFileUnit(_engine, path, _engine.SystemState.DefaultEncoding);
            if (sourceUnit == null) {
                return null;
            }
            return LoadFromSourceUnit(sourceUnit, name, path);
        }

        private ScriptModule LoadPackageFromSource(CodeContext context, string name, string path) {
            return LoadModuleFromSource(context, name, Path.Combine(path, "__init__.py"));
        }

        private ScriptModule LoadFromSourceUnit(SourceUnit sourceUnit, string name, string path) {
            ScriptModule res = InitializeModule(name, ScriptDomainManager.CurrentManager.CompileModule(name, sourceUnit), true);
            ScriptDomainManager.CurrentManager.PublishModule(res, path);
            return res;
        }
    }
}
