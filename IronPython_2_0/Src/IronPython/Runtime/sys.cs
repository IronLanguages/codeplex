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

using System; using Microsoft;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;

using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using System.Text;
using IronPython.Runtime;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using System.Security;
using IronPython.Runtime.Types;

[assembly: PythonModule("sys", typeof(IronPython.Runtime.SysModule))]
namespace IronPython.Runtime {
    public static class SysModule {
        public const int api_version = 0;
        // argv is set by PythonContext and only on the initial load
        public static readonly string byteorder = BitConverter.IsLittleEndian ? "little" : "big";
        // builtin_module_names is set by PythonContext and updated on reload
        public const string copyright = "Copyright (c) Microsoft Corporation. All rights reserved.";

        static SysModule() {
#if SILVERLIGHT
            prefix = String.Empty;
#else
            try {
                prefix = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            } catch (SecurityException) {
                prefix = String.Empty;
            }
#endif
        }

        /// <summary>
        /// Handles output of the expression statement.
        /// Prints the value and sets the __builtin__._
        /// </summary>
        public static void displayhook(CodeContext/*!*/ context, object value) {
            if (value != null) {
                PythonOps.Print(context, PythonOps.Repr(context, value));
                ScopeOps.SetMember(context, PythonContext.GetContext(context).BuiltinModuleInstance, "_", value);
            }
        }

        public static void excepthook(CodeContext/*!*/ context, object exctype, object value, object traceback) {
            PythonContext pc = PythonContext.GetContext(context);

            PythonOps.PrintWithDest(
                context,
                pc.SystemStandardError,
                pc.FormatException(PythonExceptions.ToClr(value))
            );
        }

        public static int getcheckinterval() {
            throw PythonOps.NotImplementedError("IronPython does not support sys.getcheckinterval");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
        public static void setcheckinterval(int value) {
            throw PythonOps.NotImplementedError("IronPython does not support sys.setcheckinterval");
        }

        // warnoptions is set by PythonContext and updated on each reload        

        public static void exc_clear() {
            PythonOps.ClearCurrentException();
        }

        public static PythonTuple exc_info(CodeContext/*!*/ context) {
            return PythonOps.GetExceptionInfo(context);
        }

        // exec_prefix and executable are set by PythonContext and updated on each reload

        public static void exit() {
            exit(null);
        }

        public static void exit(object code) {
            if (code == null) {
                throw new PythonExceptions._SystemExit().InitAndGetClrException();
            } else {
                PythonTuple pt = code as PythonTuple;
                if (pt != null && pt.__len__() == 1) {
                    code = pt[0];
                }

                // throw as a python exception here to get the args set.
                throw new PythonExceptions._SystemExit().InitAndGetClrException(code);
            }
        }

        public static string getdefaultencoding(CodeContext/*!*/ context) {
            return PythonContext.GetContext(context).GetDefaultEncodingName();
        }

        public static object getfilesystemencoding() {
            return null;
        }

        public static TraceBackFrame/*!*/ _getframe(CodeContext/*!*/ context) {
            return new TraceBackFrame(Builtin.globals(context), Builtin.locals(context), null);
        }

        public static TraceBackFrame/*!*/ _getframe(CodeContext/*!*/ context, int depth) {
            if (depth == 0) {
                return _getframe(context);
            }

            throw PythonOps.ValueError("_getframe is not implemented for non-zero depth");
        }

        // hex_version is set by PythonContext
        public const int maxint = Int32.MaxValue;
        public const int maxunicode = (int)ushort.MaxValue;

        // modules is set by PythonContext and only on the initial load

        // path is set by PythonContext and only on the initial load

#if SILVERLIGHT
        public const string platform = "silverlight";
#else
        public const string platform = "cli";
#endif

        public static readonly string prefix;

        // ps1 and ps2 are set by PythonContext and only on the initial load

        public static void setdefaultencoding(CodeContext context, object name) {
            if (name == null) throw PythonOps.TypeError("name cannot be None");
            string strName = name as string;
            if (strName == null) throw PythonOps.TypeError("name must be a string");

            PythonContext pc = PythonContext.GetContext(context);
            Encoding enc;
            if (!StringOps.TryGetEncoding(strName, out enc)) {
                throw PythonOps.LookupError("'{0}' does not match any available encodings", strName);
            }

            pc.DefaultEncoding = enc;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "o")]
        public static void settrace(object o) {
            throw PythonOps.NotImplementedError("sys.settrace is not yet supported by IronPython");
        }

        public static void setrecursionlimit(int limit) {
            PythonFunction.SetRecursionLimit(limit);
        }

        public static int getrecursionlimit() {
            return PythonFunction._MaximumDepth;
        }

        // stdin, stdout, stderr, __stdin__, __stdout__, and __stderr__ added by PythonContext

        // version and version_info are set by PythonContext

        public const string winver = "2.5";

        [SpecialName]
        public static void PerformModuleReload(PythonContext/*!*/ context, IAttributesCollection/*!*/ dict) {
            dict[SymbolTable.StringToId("stdin")] = dict[SymbolTable.StringToId("__stdin__")];
            dict[SymbolTable.StringToId("stdout")] = dict[SymbolTable.StringToId("__stdout__")];
            dict[SymbolTable.StringToId("stderr")] = dict[SymbolTable.StringToId("__stderr__")];

            // !!! These fields do need to be reset on "reload(sys)". However, the initial value is specified by the 
            // engine elsewhere. For now, we initialize them just once to some default value
            dict[SymbolTable.StringToId("warnoptions")] = new List(0);

            PublishBuiltinModuleNames(context, dict);
            context.SetHostVariables(dict);

            dict[SymbolTable.StringToId("meta_path")] = new List(0);
            dict[SymbolTable.StringToId("path_hooks")] = new List(0);
            dict[SymbolTable.StringToId("path_importer_cache")] = new PythonDictionary();
        }

        private static void PublishBuiltinModuleNames(PythonContext/*!*/ context, IAttributesCollection/*!*/ dict) {
            object[] keys = new object[context.Builtins.Keys.Count];
            int index = 0;
            foreach (object key in context.Builtins.Keys) {
                keys[index++] = key;
            }
            dict[SymbolTable.StringToId("builtin_module_names")] = PythonTuple.MakeTuple(keys);
        }

    }
}
