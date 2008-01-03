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
using System.IO;
using System.Diagnostics;

using IronPython.Compiler;
using IronPython.Compiler.Generation;
using IronPython.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Types;
using IronPython.Runtime.Operations;
using IronPython.Hosting;

using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Hosting;

[assembly: PythonModule("imp", typeof(IronPython.Modules.PythonImport))]
namespace IronPython.Modules {
    public static class PythonImport {
        internal const int PythonSource = 1;
        internal const int PythonCompiled = 2;
        internal const int CExtension = 3;
        internal const int PythonResource = 4;
        internal const int PackageDirectory = 5;
        internal const int CBuiltin = 6;
        internal const int PythonFrozen = 7;
        internal const int PythonCodeResource = 8;
        internal const int SearchError = 0;

        [PythonName("get_magic")]
        public static string GetMagic() {
            return "";
        }

        [PythonName("get_suffixes")]
        public static List GetSuffixes() {
            return List.MakeList(PythonTuple.MakeTuple(".py", "U", PythonSource));
        }

        [PythonName("find_module")]
        public static PythonTuple FindModule(CodeContext context, string name) {
            return FindBuiltinOrSysPath(context, name);
        }

        [PythonName("find_module")]
        public static PythonTuple FindModule(CodeContext context, string name, List path) {
            if (path == null) {
                return FindBuiltinOrSysPath(context, name);
            } else {
                return FindModulePath(context, name, path);
            }
        }

        [PythonName("load_module")]
        public static object LoadModule(CodeContext context, string name, PythonFile file, string filename, PythonTuple description) {
            if (description == null) {
                throw PythonOps.TypeError("load_module() argument 4 must be 3-item sequence, not None");
            }
            if (description.Count != 3) {
                throw PythonOps.TypeError("load_module() argument 4 must be sequence of length 3, not {0}", description.Count);
            }

            // already loaded? do reload()
            object mod;
            if (SystemState.Instance.modules.TryGetValue(name, out mod)) {
                ScriptScope module = mod as ScriptScope;
                if (module != null) {
                    return Builtin.reload(context, module);
                }
            }
            int type = Converter.ConvertToInt32(description[2]);
            switch (type) {
                case PythonSource:
                    return LoadPythonSource(context, name, file, filename);
                case CBuiltin:
                    return LoadBuiltinModule(context, name);
                case PackageDirectory:
#if !SILVERLIGHT // file-system
                    return LoadPackageDirectory(context, name, filename);
#endif
                default:
                    throw PythonOps.TypeError("don't know how to import {0}, (type code {1}", name, type);
            }
        }

        [Documentation("new_module(name) -> module\nCreates a new module without adding it to sys.modules.")]
        [PythonName("new_module")]
        public static object NewModule(CodeContext context, string name) { // TODO: remove context?
            ScriptScope res = PythonContext.MakePythonModule(name, null, ModuleOptions.None);            

            PythonModuleOps.SetPythonCreated(res);
            return res;
        }

        private static long lock_count;

        [PythonName("lock_held")]
        public static bool IsLockHeld() {
            return lock_count != 0;
        }

        [PythonName("acquire_lock")]
        public static void AcquireLock() {
            lock_count++;
        }

        [PythonName("release_lock")]
        public static void ReleaseLock() {
            if (lock_count <= 0) {
                throw PythonOps.RuntimeError("not holding the import lock");
            }
            lock_count--;
        }

        public static object PY_SOURCE = PythonSource;
        public static object PY_COMPILED = PythonCompiled;
        public static object C_EXTENSION = CExtension;
        public static object PY_RESOURCE = PythonResource;
        public static object PKG_DIRECTORY = PackageDirectory;
        public static object C_BUILTIN = CBuiltin;
        public static object PY_FROZEN = PythonFrozen;
        public static object PY_CODERESOURCE = PythonCodeResource;
        public static object SEARCH_ERROR = SearchError;

        [PythonName("init_builtin")]
        public static object InitBuiltin(CodeContext context, string name) {
            return LoadBuiltinModule(context, name);
        }

        [PythonName("init_frozen")]
        public static object InitFrozen(string name) {
            return null;
        }

        [PythonName("is_builtin")]
        public static int IsBuiltin(CodeContext context, string name) {
            Type ty;
            if (SystemState.Instance.Builtins.TryGetValue(name, out ty)) {
                return 1;
            }
            return 0;
        }

        [PythonName("is_frozen")]
        public static bool IsFrozen(string name) {
            return false;
        }

#if !SILVERLIGHT

        [PythonName("load_compiled")]
        public static object LoadCompiled(string name, string pathname) {
            return null;
        }

        [PythonName("load_compiled")]
        public static object LoadCompiled(string name, string pathname, PythonFile file) {
            return null;
        }

        [PythonName("load_dynamic")]
        public static object LoadDynamic(string name, string pathname) {
            return null;
        }

        [PythonName("load_dynamic")]
        public static object LoadDynamic(string name, string pathname, PythonFile file) {
            return null;
        }

#endif

        [PythonName("load_source")]
        public static object LoadSource(CodeContext context, string name, string pathname) {
            // TODO: is this supposed to open PythonFile with Python-specific behavior?
            // we may need to insert additional layer to SourceUnit content provider if so
            SourceUnit codeUnit = context.LanguageContext.TryGetSourceFileUnit(pathname, PythonContext.GetSystemState(context).DefaultEncoding, SourceCodeKind.File);

            return GenerateAndInitializeModule(context, name, pathname, codeUnit);
        }

        [PythonName("load_source")]
        public static object LoadSource(CodeContext context, string name, string pathname, PythonFile file) {
            return LoadPythonSource(context, name, file, pathname);
        }

        #region Implementation

        private static PythonTuple FindBuiltinOrSysPath(CodeContext context, string name) {
            List sysPath = SystemState.Instance.path;
            if (sysPath == null) {
                throw PythonOps.ImportError("sys.path must be a list of directory names");
            }
            return FindModuleBuiltinOrPath(context, name, sysPath);
        }

        private static PythonTuple FindModulePath(CodeContext context, string name, List path) {
            Debug.Assert(path != null);

            if (name == null) {
                throw PythonOps.TypeError("find_module() argument 1 must be string, not None");
            }

#if !SILVERLIGHT // files
            foreach (object d in path) {
                string dir = d as string;
                if (dir == null) continue;  // skip invalid entries

                string pathName = Path.Combine(dir, name);
                if (Directory.Exists(pathName)) {
                    if (File.Exists(Path.Combine(pathName, "__init__.py"))) {
                        return PythonTuple.MakeTuple(null, pathName, PythonTuple.MakeTuple("", "", PackageDirectory));
                    }
                }

                string fileName = pathName + ".py";
                if (File.Exists(fileName)) {
                    FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    PythonFile pf = PythonFile.Create(fs, fileName, "U");
                    return PythonTuple.MakeTuple(pf, fileName, PythonTuple.MakeTuple(".py", "U", PythonSource));
                }
            }
#endif
            throw PythonOps.ImportError("No module named {0}", name);
        }

        private static PythonTuple FindModuleBuiltinOrPath(CodeContext context, string name, List path) {
            if (name.Equals("sys")) return BuiltinModuleTuple(name);
            if (name.Equals("clr")) {
                context.ModuleContext.ShowCls = true;
                return BuiltinModuleTuple(name);
            }
            Type ty;
            if (SystemState.Instance.Builtins.TryGetValue(name, out ty)) {
                return BuiltinModuleTuple(name);
            }

            return FindModulePath(context, name, path);
        }

        private static PythonTuple BuiltinModuleTuple(string name) {
            return PythonTuple.MakeTuple(null, name, PythonTuple.MakeTuple("", "", CBuiltin));
        }

        private static ScriptScope LoadPythonSource(CodeContext context, string name, PythonFile file, string filename) {
            SourceUnit sourceUnit = context.LanguageContext.CreateSnippet(file.Read(), filename, SourceCodeKind.File);
            sourceUnit.IsVisibleToDebugger = true;
            return GenerateAndInitializeModule(context, name, filename, sourceUnit);
        }

        private static ScriptScope GenerateAndInitializeModule(CodeContext context, string moduleName, string path, SourceUnit sourceUnit) {
            ScriptScope module = ScriptDomainManager.CurrentManager.CompileModule(moduleName, sourceUnit);
            
            PythonModuleOps.Set__file__(module, path);
            PythonModuleOps.Set__name__(module, moduleName);

            return PythonContext.GetImporter(context).InitializeModule(moduleName, module, true);
        }

#if !SILVERLIGHT // files
        private static ScriptScope LoadPackageDirectory(CodeContext context, string moduleName, string path) {            
            string initPath = Path.Combine(path, "__init__.py");
                        
            SourceUnit codeUnit = context.LanguageContext.CreateFileUnit(initPath, PythonContext.GetSystemState(context).DefaultEncoding);
            ScriptScope module = ScriptDomainManager.CurrentManager.CompileModule(moduleName, codeUnit);

            module.FileName = initPath;
            module.ModuleName = moduleName;

            return PythonContext.GetImporter(context).InitializeModule(moduleName, module, true);
        }
#endif

        private static object LoadBuiltinModule(CodeContext context, string name) {
            return PythonContext.GetImporter(context).ImportBuiltin(context, name);
        }

        #endregion
    }
}
