/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Shared Source License
 * for IronPython. A copy of the license can be found in the License.html file
 * at the root of this distribution. If you can not locate the Shared Source License
 * for IronPython, please send an email to ironpy@microsoft.com.
 * By using this source code in any fashion, you are agreeing to be bound by
 * the terms of the Shared Source License for IronPython.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

using System.Diagnostics;

using IronPython.Compiler;
using IronPython.Compiler.Ast;
using IronPython.Compiler.Generation;
using IronPython.Modules;
using IronPython.Runtime.Types;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Calls;

namespace IronPython.Runtime {
    /// <summary>
    /// Importer class - used for importing modules.  Used by Ops and __builtin__
    /// </summary>
    public static class Importer {
        #region Internal API Surface

        /// <summary>
        /// Gateway into importing ... called from Ops.  Performs the initial import of
        /// a module and returns the module.
        /// </summary>
        internal static object Import(PythonModule mod, string fullName, List from) {
            object importFunction = FindImportFunction(mod);
            return Ops.CallWithContext(mod, importFunction, fullName, null, null, from);
        }

        /// <summary>
        /// Gateway into importing ... called from Ops.  This is called after
        /// importing the module and is used to return individual items from
        /// the module.  The outer modules dictionary is then updated with the
        /// result.
        /// </summary>
        internal static object ImportFrom(ICallerContext context, object mod, string name) {
            PythonModule from = mod as PythonModule;
            if (from != null) {
                object ret;
                if (from.TryGetAttr(from, SymbolTable.StringToId(name), out ret)) {
                    return ret;
                } else {
                    object path;
                    if (from.TryGetAttr(from, SymbolTable.Path, out path)) {
                        return ImportNested(from, name);
                    } else {
                        throw Ops.ImportError("Cannot import name {0}", name);
                    }
                }
            } else {
                // This is too lax, for example it allows from module.class import member
                object ret;
                if (Ops.TryGetAttr(context, mod, SymbolTable.StringToId(name), out ret)) {
                    return ret;
                } else {
                    throw Ops.ImportError("No module named {0}", name);
                }
            }
        }

        /// <summary>
        /// Called by the __builtin__.__import__ functions (general importing) & PythonEngine (for site.py)
        /// 
        /// Returns a PythonModule.
        /// </summary>        
        internal static object ImportModule(ICallerContext context, string modName, bool bottom) {
            PythonModule mod = context.Module;
            object newmod = null;
            string[] parts = modName.Split('.');

            // if importing a.b.c, import "a" first and then import b.c from a
            string name;    // name of the module we are to import in relation to the current module
            List path;      // path to search
            if (TryGetNameAndPath(context, mod, parts[0], out name, out path)) {
                // import relative
                if (!TryGetExistingModule(context.SystemState, name, out newmod)) {
                    newmod = ImportTopRelative(mod, parts[0], name, path);
                } else if (parts.Length == 1) {
                    // if we imported before having the assembly
                    // loaded and then loaded the assembly we want
                    // to make the assembly available now.

                    PythonModule pm = newmod as PythonModule;
                    if (pm != null && pm.InnerModule != null) pm.PackageImported = true;
                }

            }

            if (newmod == null) {
                newmod = ImportTopAbsolute(mod, parts[0]);
                if (newmod == null) return null;
            }

            // now import the b.c etc.
            object next = newmod;
            for (int i = 1; i < parts.Length; i++) {
                next = ImportFrom(context, next, parts[i]);
            }

            return bottom ? next : newmod;
        }

        private static object ImportTopRelative(PythonModule mod, string name, string full, List path) {
            object newmod = ImportFromPath(mod.SystemState, name, full, path);
            if (newmod != null) {
                mod.SetImportedAttr(mod, SymbolTable.StringToId(name), newmod);
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
        /// <param name="mod">The module that triggered import</param>
        /// <param name="name">Name of the module to be imported</param>
        /// <param name="full">Output - full name of the module being imported</param>
        /// <param name="path">Path to use to search for "full"</param>
        /// <returns></returns>
        private static bool TryGetNameAndPath(ICallerContext context, PythonModule mod, string name, out string full, out List path) {
            // Unless we can find enough information to perform relative import,
            // we are going to import the module whose name we got
            full = name;
            path = null;

            // We need to get __name__ to find the name of the imported module.
            // If absent, fall back to absolute import
            object attribute;
            if (!mod.TryGetAttr(context, SymbolTable.Name, out attribute)) {
                return false;
            }

            // And the __name__ needs to be string
            string modName = attribute as string;
            if (modName == null) {
                return false;
            }

            // If the module has __path__ (and __path__ is list), nested module is being imported
            // otherwise, importing sibling to the importing module

            if (mod.TryGetAttr(context, SymbolTable.Path, out attribute) && (path = attribute as List) != null) {
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
            if (!mod.SystemState.modules.TryGetValue(parentName, out parentObject)) {
                // parent module not found in sys.modules, fallback to absolute import
                return false;
            }

            PythonModule parentModule;
            if ((parentModule = parentObject as PythonModule) == null) {
                // the sys.module entry is not PythonModule, fallback to absolute import
                return false;
            }

            // The parentModule now needs to have __path__ - list
            if (parentModule.TryGetAttr(context, SymbolTable.Path, out attribute) && (path = attribute as List) != null) {
                // combine the module names
                full = parentName + "." + name;
                return true;
            }

            // not enough information - absolute import
            return false;
        }

        internal static PythonModule ReloadBuiltin(PythonModule module) {
            Type ty;

            if (module.SystemState.TopPackage.Builtins.TryGetValue(module.ModuleName, out ty)) {
                if (typeof(CustomSymbolDict).IsAssignableFrom(ty)) {
                    CustomSymbolDict dict = (CustomSymbolDict)ty.GetConstructor(Type.EmptyTypes).Invoke(Ops.EMPTY);
                    //@todo share logic to copy old values in when not already there from reload
                    module.__dict__ = dict;
                    module.Initialize();
                    return module;
                } else {
                    ReflectedType type = (ReflectedType)Ops.GetDynamicTypeFromType(ty);
                    type.Initialize();
                    foreach (string attrName in type.GetAttrNames(module)) {
                        SymbolId id = SymbolTable.StringToId(attrName);

                        module.__dict__[id] = type.GetAttr(module, null, id);
                    }
                    module.Initialize();
                    return module;
                }
            }
            throw new NotImplementedException();
        }

        internal static bool TryGetExistingModule(SystemState state, string fullName, out object ret) {
            // Python uses None/null as a key here to indicate a missing module
            if (state.modules.TryGetValue(fullName, out ret)) {
                return ret != null;
            }
            return false;
        }

        #endregion

        #region Private Implementation Details

        private static object ImportTopAbsolute(PythonModule mod, string name) {
            object ret;
            if (TryGetExistingModule(mod.SystemState, name, out ret)) {
                PythonModule pm = ret as PythonModule;
                if (pm != null && pm.InnerModule != null) {
                    // if we imported before having the assembly
                    // loaded and then loaded the assembly we want
                    // to make the assembly available now.
                    pm.PackageImported = true;
                }

                return ret;
            }

            ret = ImportBuiltin(mod, name);
            if (ret != null) return ret;

            ret = ImportFromPath(mod.SystemState, name, name, mod.SystemState.path);
            if (ret != null) return ret;

            ret = ImportReflected(mod, name);
            if (ret != null) return ret;

            return null;
        }

        private static object ImportNested(PythonModule mod, string name) {
            object ret;
            if (mod.TryGetAttr(mod, SymbolTable.StringToId(name), out ret)) return ret;

            string baseName;
            List path = ResolveSearchPath(mod, out baseName);
            string fullName = CreateFullName(baseName, name);

            if (TryGetExistingModule(mod.SystemState, fullName, out ret)) {
                return ret;
            }

            if (path != null) {
                ret = ImportFromPath(mod.SystemState, name, fullName, path);
                if (ret != null) {
                    mod.SetImportedAttr(mod, SymbolTable.StringToId(name), ret);
                    return ret;
                }
            }

            throw Ops.ImportError("cannot import {0} from {1}", name, mod.ModuleName);
        }

        private static object FindImportFunction(PythonModule mod) {
            object import;
            if (mod != null) {
                if (mod.TryGetAttr(mod, SymbolTable.Import, out import)) {
                    return import;
                }

                object builtin = Ops.GetAttr(mod, mod, SymbolTable.Builtins);
                if (Ops.TryGetAttr(mod, builtin, SymbolTable.Import, out import)) {
                    return import;
                }
            }
            throw Ops.ImportError("cannot find __import__");
        }

        internal static PythonModule MakePythonModule(SystemState state, string name, ReflectedType type) {
            type.Initialize();
            FieldIdDict dict = new FieldIdDict();

            foreach (string attrName in type.GetAttrNames(DefaultContext.Default)) {

                dict[SymbolTable.StringToId(attrName)] = type.GetAttr(DefaultContext.Default, null, SymbolTable.StringToId(attrName));
            }
            PythonModule ret = new PythonModule(name, dict, state);
            state.modules[name] = ret;
            return ret;
        }

        internal static object ImportBuiltin(PythonModule mod, string name) {
            mod.SystemState.TopPackage.Initialize(mod.SystemState);

            if (name.Equals("sys")) return mod.SystemState;
            if (name.Equals("clr")) {
                ((ICallerContext)mod).ContextFlags |= CallerContextAttributes.ShowCls;
                return ((ICallerContext)mod).SystemState.ClrModule;
            }
            Type ty;
            if (mod.SystemState.TopPackage.Builtins.TryGetValue(name, out ty)) {
                // run the type's .cctor before doing any custom reflection on the type.
                // This allows modules to lazily initialize DynamicType's to custom values
                // rather than having them get populated w/ the ReflectedType.  W/o this the
                // cctor runs after we've done a bunch of reflection over the type that doesn't
                // force the cctor to run.
                System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(ty.TypeHandle);

                if (typeof(CompiledModule).IsAssignableFrom(ty)) {
                    return InitializeModule(name, CompiledModule.Load(name, ty, mod.SystemState));
                } else {
                    return MakePythonModule(mod.SystemState, name, (ReflectedType)Ops.GetDynamicTypeFromType(ty));
                }
            }
            return null;
        }

        private static object ImportReflected(PythonModule mod, string name) {
            object res = mod.SystemState.TopPackage.TryGetPackageAny(mod.SystemState, name);
            if (res != null) {
                ((ICallerContext)mod).ContextFlags |= CallerContextAttributes.ShowCls;
            }
            return res;
        }

        private static object ImportFromPath(SystemState state, string name, string fullName, List path) {
            object ret = null;
            foreach (object dirname in path) {
                string str;
                if (Converter.TryConvertToString(dirname, out str)) {  // ignore non-string
                    string pathname = Path.Combine(str, name);

                    if (Directory.Exists(pathname)) {
                        if (File.Exists(Path.Combine(pathname, "__init__.py"))) {
                            ret = LoadPackageFromSource(state, fullName, pathname);
                            break;
                        }
                    }

                    string filename = pathname + ".py";
                    if (File.Exists(filename)) {
                        ret = LoadModuleFromSource(state, fullName, filename);
                        break;
                    }
                }
            }
            return ret;
        }

        private static PythonModule LoadFromSource(SystemState state, string fullName, string fileName) {
            CompilerContext context = new CompilerContext(fileName);
            Parser parser = Parser.FromFile(state, context);
            Statement s = parser.ParseFileInput();

            PythonModule pmod = OutputGenerator.GenerateModule(state, context, s, fullName);

            pmod.Filename = fileName;
            pmod.ModuleName = fullName;

            return pmod;
        }

        /// <summary>
        /// Initializes the specified module and returns the user-exposable PythonModule.
        /// </summary>
        internal static PythonModule InitializeModule(string fullName, PythonModule pmod) {
            // if we have a collision (both a package & namespace)
            // then we could have already exposed the ReflectedPackage
            // out to the user.  Therefore the outer module will alway
            // be the reflected packge module.

            PythonModule rpMod = pmod.SystemState.TopPackage.TryGetPackage(pmod.SystemState, fullName);
            PythonModule newmod = pmod;
            if (rpMod != null) {
                pmod.InnerModule = rpMod.InnerModule;
                rpMod.InnerModule = pmod;

                pmod.PackageImported = true;

                pmod = rpMod;
            }

            //Put this in modules dict so we won't reload with circular imports
            pmod.SystemState.modules[fullName] = pmod;
            bool success = false;
            try {
                newmod.Initialize();
                success = true;
            } finally {
                if (!success) pmod.SystemState.modules.Remove(fullName);
            }


            return pmod;
        }

        private static PythonModule LoadModuleFromSource(SystemState state, string fullName, string fileName) {
            PythonModule mod = LoadFromSource(state, fullName, fileName);
            return InitializeModule(fullName, mod);
        }

        private static object LoadPackageFromSource(SystemState state, string fullName, string dirname) {
            string fullPath = Path.GetFullPath(dirname);
            List __path__ = Ops.MakeList(fullPath);
            PythonModule mod = LoadFromSource(state, fullName, Path.Combine(dirname, "__init__.py"));
            mod.SetImportedAttr(DefaultContext.Default, SymbolTable.Path, __path__);
            return InitializeModule(fullName, mod);
        }

        private static List ResolveSearchPath(PythonModule mod, out string baseName) {
            baseName = mod.ModuleName;

            object path;
            if (!mod.TryGetAttr(DefaultContext.Default, SymbolTable.Path, out path)) {
                List basePath = path as List;
                for (; ; ) {
                    int lastDot = baseName.LastIndexOf('.');
                    if (lastDot < 0) {
                        baseName = null;
                        break;
                    }

                    baseName = baseName.Substring(0, lastDot);
                    object package = mod.SystemState.modules[baseName];
                    if (Ops.TryGetAttr(package, SymbolTable.Path, out path)) {
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

        private static string CreateFullName(string baseName, string name) {
            if (baseName == null || baseName.Length == 0 || baseName == "__main__") {
                return name;
            }
            return baseName + "." + name;
        }
        #endregion
    }
}
