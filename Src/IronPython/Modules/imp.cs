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
using System.IO;

using IronPython.Runtime;
using IronPython.Compiler;

[assembly: PythonModule("imp", typeof(IronPython.Modules.PythonImport))]
namespace IronPython.Modules {
    [PythonType("imp")]
    public static class PythonImport {

        [PythonName("get_magic")]
        public static string GetMagic() {
            return "";
        }

        [PythonName("get_suffixes")]
        public static List GetSuffixes() {
            return new List();
        }

        [PythonName("find_module")]
        public static Tuple FindModule(string name) {
            return Tuple.MakeTuple();
        }

        [PythonName("find_module")]
        public static Tuple FindModule(string name, string path) {
            return FindModule(name);
        }

        [PythonName("load_module")]
        public static object LoadModule(string name, object file, string filename, string description) {
            return null;
        }

        [Documentation("new_module(name) -> module\nCreates a new module without adding it to sys.modules.")]
        [PythonName("new_module")]
        public static object NewModule(ICallerContext context, string name) {
            return new PythonModule(name, new FieldIdDict(), context.SystemState);
        }

        [PythonName("lock_held")]
        public static bool IsLockHeld() {
            return false;
        }

        [PythonName("acquire_lock")]
        public static void AcquireLock() {
        }

        [PythonName("release_lock")]
        public static void ReleaseLock() {
        }

        public static object PY_SOURCE = 1;
        public static object PY_COMPILED = 2;
        public static object C_EXTENSION = 3;
        public static object PY_RESOURCE = 4;
        public static object PKG_DIRECTORY = 5;
        public static object C_BUILTIN = 6;
        public static object PY_FROZEN = 7;
        public static object SEARCH_ERROR = 0;

        [PythonName("init_builtin")]
        public static object InitBuiltin(string name) {
            return null;
        }

        [PythonName("init_frozen")]
        public static object InitFrozen(string name) {
            return null;
        }

        [PythonName("is_builtin")]
        public static int IsBuiltin(string name) {
            return 0;
        }

        [PythonName("is_frozen")]
        public static bool IsFrozen(string name) {
            return false;
        }

        [PythonName("load_compiled")]
        public static object LoadCompiled(string name, string pathname) {
            return null;
        }

        [PythonName("load_compiled")]
        public static object LoadCompiled(string name, string pathname, object file) {
            return null;
        }

        [PythonName("load_dynamic")]
        public static object LoadDynamic(string name, string pathname) {
            return null;
        }

        [PythonName("load_dynamic")]
        public static object LoadDynamic(string name, string pathname, object file) {
            return null;
        }

        [PythonName("load_source")]
        public static object LoadSource(string name, string pathname) {
            return null;
        }

        [PythonName("load_source")]
        public static object LoadSource(string name, string pathname, object file) {
            return null;
        }
    }
}
