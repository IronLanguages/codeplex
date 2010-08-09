/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

#if CLR2
using Microsoft.Scripting.Math;
#else
using System.Numerics;
#endif

[assembly: PythonModule("sys", typeof(IronPython.Modules.SysModule))]
namespace IronPython.Modules {
    public static class SysModule {
        public const string __doc__ = "Provides access to functions which query or manipulate the Python runtime.";

        public const int api_version = 0;
        // argv is set by PythonContext and only on the initial load
        public static readonly string byteorder = BitConverter.IsLittleEndian ? "little" : "big";
        // builtin_module_names is set by PythonContext and updated on reload
        public const string copyright = "Copyright (c) Microsoft Corporation. All rights reserved.";

        private static string GetPrefix() {
            string prefix;
#if SILVERLIGHT
            prefix = String.Empty;
#else
            try {
                prefix = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            } catch (SecurityException) {
                prefix = String.Empty;
            }
#endif
            return prefix;
        }

        /// <summary>
        /// Returns detailed call statistics.  Not implemented in IronPython and always returns None.
        /// </summary>
        public static object callstats() {
            return null;
        }

        /// <summary>
        /// Handles output of the expression statement.
        /// Prints the value and sets the __builtin__._
        /// </summary>
        public static void displayhook(CodeContext/*!*/ context, object value) {
            if (value != null) {
                PythonOps.Print(context, PythonOps.Repr(context, value));
                PythonContext.GetContext(context).BuiltinModuleDict["_"] = value;
            }
        }

        public const int dllhandle = 0;

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

        [Python3Warning("'sys.exc_clear() not supported in 3.x; use except clauses'")]
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
            return "mbcs";
        }

        [PythonHidden]
        public static TraceBackFrame/*!*/ _getframeImpl(CodeContext/*!*/ context) {
            return _getframeImpl(context, 0);
        }

        [PythonHidden]
        public static TraceBackFrame/*!*/ _getframeImpl(CodeContext/*!*/ context, int depth) {
            return _getframeImpl(context, depth, PythonOps.GetFunctionStack());
        }

        internal static TraceBackFrame/*!*/ _getframeImpl(CodeContext/*!*/ context, int depth, List<FunctionStack> stack) {
            if (depth < stack.Count) {
                TraceBackFrame cur = null;
                int curTraceFrame = -1;

                for (int i = 0; i < stack.Count - depth; i++) {
                    var elem = stack[i];

                    if (elem.Frame != null) {
                        // we previously handed out a frame here, hand out the same one now
                        cur = elem.Frame;
                    } else {
                        // create a new frame and save it for future calls
                        cur = new TraceBackFrame(
                            context,
                            Builtin.globals(elem.Context),
                            Builtin.locals(elem.Context),
                            elem.Code,
                            cur
                        );

                        stack[i] = new FunctionStack(elem.Context, elem.Code, cur);
                    }

                    curTraceFrame++;
                }
                return cur;
            } 

            throw PythonOps.ValueError("call stack is not deep enough");
        }

        public static int getsizeof(object o) {
            return ObjectOps.__sizeof__(o);
        }

#if !SILVERLIGHT
        public static PythonTuple getwindowsversion() {
            var osVer = Environment.OSVersion;
            return PythonTuple.MakeTuple(
                osVer.Version.Major,
                osVer.Version.Minor,
                osVer.Version.Build,
                (int)osVer.Platform,
                osVer.ServicePack
            );
        }
#endif

        // hex_version is set by PythonContext
        public const int maxint = Int32.MaxValue;
        public const int maxsize = Int32.MaxValue;
        public const int maxunicode = (int)ushort.MaxValue;

        // modules is set by PythonContext and only on the initial load

        // path is set by PythonContext and only on the initial load

#if SILVERLIGHT
        public const string platform = "silverlight";
#else
        public const string platform = "cli";
#endif

        public static readonly string prefix = GetPrefix();

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

#if PROFILE_SUPPORT
        // not enabled because we don't yet support tracing built-in functions.  Doing so is a little
        // difficult because it's hard to flip tracing on/off for them w/o a perf overhead in the 
        // non-profiling case.
        public static void setprofile(CodeContext/*!*/ context, TracebackDelegate o) {
            PythonContext pyContext = PythonContext.GetContext(context);
            pyContext.EnsureDebugContext();

            if (o == null) {
                pyContext.UnregisterTracebackHandler();
            } else {
                pyContext.RegisterTracebackHandler();
            }

            // Register the trace func with the listener
            pyContext.TracebackListener.SetProfile(o);
        }
#endif

        public static void settrace(CodeContext/*!*/ context, object o) {
            PythonContext pyContext = PythonContext.GetContext(context);
            pyContext.EnsureDebugContext();

            if (o == null) {
                pyContext.UnregisterTracebackHandler();
                PythonTracebackListener.SetTrace(null, null);
            } else {
                // We're following CPython behavior here.
                // If CurrentPythonFrame is not null then we're currently inside a traceback, and
                // enabling trace while inside a traceback is only allowed through sys.call_tracing()
                var pyThread = PythonOps.GetFunctionStackNoCreate();
                if (pyThread == null || !PythonTracebackListener.InTraceBack) {
                    pyContext.PushTracebackHandler(new PythonTracebackListener((PythonContext)context.LanguageContext));
                    pyContext.RegisterTracebackHandler();
                    PythonTracebackListener.SetTrace(o, (TracebackDelegate)Converter.ConvertToDelegate(o, typeof(TracebackDelegate)));
                }
            }
        }

        public static void call_tracing(CodeContext/*!*/ context, object func, PythonTuple args) {
            PythonContext pyContext = (PythonContext)context.LanguageContext;
            pyContext.EnsureDebugContext();

            pyContext.UnregisterTracebackHandler();
            pyContext.PushTracebackHandler(new PythonTracebackListener((PythonContext)context.LanguageContext));
            pyContext.RegisterTracebackHandler();
            try {
                PythonCalls.Call(func, args.ToArray());
            } finally {
                pyContext.PopTracebackHandler();
                pyContext.UnregisterTracebackHandler();
            }
        }

        public static object gettrace(CodeContext/*!*/ context) {
            return PythonTracebackListener.GetTraceObject();
        }

        public static void setrecursionlimit(CodeContext/*!*/ context, int limit) {
            PythonContext.GetContext(context).RecursionLimit = limit;
        }

        public static int getrecursionlimit(CodeContext/*!*/ context) {
            return PythonContext.GetContext(context).RecursionLimit;
        }

        // stdin, stdout, stderr, __stdin__, __stdout__, and __stderr__ added by PythonContext

        // version and version_info are set by PythonContext
        public static PythonTuple subversion = PythonTuple.MakeTuple("IronPython", "", "");

        public const string winver = "2.7";

        #region Special types

        [PythonHidden, PythonType("flags"), DontMapIEnumerableToIter]
        public sealed class SysFlags : IList<object> {
            private const string _className = "sys.flags"; 
            
            internal SysFlags() { }

            private const int INDEX_DEBUG = 0;
            private const int INDEX_PY3K_WARNING = 1;
            private const int INDEX_DIVISION_WARNING = 2;
            private const int INDEX_DIVISION_NEW = 3;
            private const int INDEX_INSPECT = 4;
            private const int INDEX_INTERACTIVE = 5;
            private const int INDEX_OPTIMIZE = 6;
            private const int INDEX_DONT_WRITE_BYTECODE = 7;
            private const int INDEX_NO_USER_SITE = 8;
            private const int INDEX_NO_SITE = 9;
            private const int INDEX_IGNORE_ENVIRONMENT = 10;
            private const int INDEX_TABCHECK = 11;
            private const int INDEX_VERBOSE = 12;
            private const int INDEX_UNICODE = 13;
            private const int INDEX_BYTES_WARNING = 14;

            public const int n_fields = 15;
            public const int n_sequence_fields = 15;
            public const int n_unnamed_fields = 0;

            private static readonly string[] _keys = new string[] {
                "debug", "py3k_warning", "division_warning", "division_new", "inspect",
                "interactive", "optimize", "dont_write_bytecode", "no_user_site", "no_site",
                "ignore_environment", "tabcheck", "verbose", "unicode", "bytes_warning"
            };
            private object[] _values = new object[n_fields] {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            };

            private PythonTuple __tuple = null;
            private PythonTuple _tuple {
                get {
                    _Refresh();
                    return __tuple;
                }
            }

            private string __string = null;
            private string _string {
                get {
                    _Refresh();
                    return __string;
                }
            }
            public override string ToString() {
                return _string;
            }
            public string __repr__() {
                return _string;
            }

            private bool _modified = true;
            private void _Refresh() {
                if (_modified) {
                    __tuple = PythonTuple.MakeTuple(_values);

                    StringBuilder sb = new StringBuilder("sys.flags(");
                    for (int i = 0; i < n_fields; i++) {
                        if (_keys[i] == null) {
                            sb.Append(_values[i]);
                        } else {
                            sb.AppendFormat("{0}={1}", _keys[i], _values[i]);
                        }
                        if (i < n_fields - 1) {
                            sb.Append(", ");
                        } else {
                            sb.Append(')');
                        }
                    }
                    __string = sb.ToString();

                    _modified = false;
                }
            }

            private int _GetVal(int index) {
                return (int)_values[index];
            }
            private void _SetVal(int index, int value) {
                if ((int)_values[index] != value) {
                    _modified = true;
                    _values[index] = value;
                }
            }

            #region ICollection<object> Members

            void ICollection<object>.Add(object item) {
                throw new InvalidOperationException(_className + " is readonly");
            }

            void ICollection<object>.Clear() {
                throw new InvalidOperationException(_className + " is readonly");
            }

            [PythonHidden]
            public bool Contains(object item) {
                return _tuple.Contains(item);
            }

            [PythonHidden]
            public void CopyTo(object[] array, int arrayIndex) {
                _tuple.CopyTo(array, arrayIndex);
            }

            public int Count {
                [PythonHidden]
                get {
                    return n_fields;
                }
            }

            bool ICollection<object>.IsReadOnly {
                get { return true; }
            }

            bool ICollection<object>.Remove(object item) {
                throw new InvalidOperationException(_className + " is readonly");
            }

            #endregion

            #region IEnumerable Members

            [PythonHidden]
            public IEnumerator GetEnumerator() {
                return _tuple.GetEnumerator();
            }

            #endregion

            #region IEnumerable<object> Members

            IEnumerator<object> IEnumerable<object>.GetEnumerator() {
                return ((IEnumerable<object>)_tuple).GetEnumerator();
            }

            #endregion

            #region ISequence Members

            public int __len__() {
                return n_fields;
            }

            public object this[int i] {
                get {
                    return _tuple[i];
                }
            }

            public object this[BigInteger i] {
                get {
                    return this[(int)i];
                }
            }

            public object __getslice__(int start, int end) {
                return _tuple.__getslice__(start, end);
            }

            public object this[Slice s] {
                get {
                    return _tuple[s];
                }
            }

            public object this[object o] {
                get {
                    return this[Converter.ConvertToIndex(o)];
                }
            }

            #endregion

            #region IList<object> Members

            [PythonHidden]
            public int IndexOf(object item) {
                return _tuple.IndexOf(item);
            }

            void IList<object>.Insert(int index, object item) {
                throw new InvalidOperationException(_className + " is readonly");
            }

            void IList<object>.RemoveAt(int index) {
                throw new InvalidOperationException(_className + " is readonly");
            }

            object IList<object>.this[int index] {
                get {
                    return _tuple[index];
                }
                set {
                    throw new InvalidOperationException(_className + " is readonly");
                }
            }

            #endregion

            #region binary ops

            public static PythonTuple operator +([NotNull]SysFlags f, [NotNull]PythonTuple t) {
                return f._tuple + t;
            }

            public static PythonTuple operator *([NotNull]SysFlags f, int n) {
                return f._tuple * n;
            }

            public static PythonTuple operator *(int n, [NotNull]SysFlags f) {
                return f._tuple * n;
            }

            public static object operator *([NotNull]SysFlags f, [NotNull]Index n) {
                return f._tuple * n;
            }

            public static object operator *([NotNull]Index n, [NotNull]SysFlags f) {
                return f._tuple * n;
            }

            public static object operator *([NotNull]SysFlags f, object n) {
                return f._tuple * n;
            }

            public static object operator *(object n, [NotNull]SysFlags f) {
                return f._tuple * n;
            }

            #endregion

            # region comparison and hashing methods

            public static bool operator >(SysFlags f, PythonTuple t) {
                return f._tuple > t;
            }

            public static bool operator <(SysFlags f, PythonTuple t) {
                return f._tuple < t;
            }

            public static bool operator >=(SysFlags f, PythonTuple t) {
                return f._tuple >= t;
            }

            public static bool operator <=(SysFlags f, PythonTuple t) {
                return f._tuple <= t;
            }

            public override bool Equals(object obj) {
                if (obj is SysFlags) {
                    return _tuple.Equals(((SysFlags)obj)._tuple);
                }
                return _tuple.Equals(obj);
            }

            public override int GetHashCode() {
                return _tuple.GetHashCode();
            }

            # endregion

            #region sys.flags API

            public int debug {
                get { return _GetVal(INDEX_DEBUG); }
                internal set { _SetVal(INDEX_DEBUG, value); }
            }

            public int py3k_warning {
                get { return _GetVal(INDEX_PY3K_WARNING); }
                internal set { _SetVal(INDEX_PY3K_WARNING, value); }
            }

            public int division_warning {
                get { return _GetVal(INDEX_DIVISION_WARNING); }
                internal set { _SetVal(INDEX_DIVISION_WARNING, value); }
            }

            public int division_new {
                get { return _GetVal(INDEX_DIVISION_NEW); }
                internal set { _SetVal(INDEX_DIVISION_NEW, value); }
            }

            public int inspect {
                get { return _GetVal(INDEX_INSPECT); }
                internal set { _SetVal(INDEX_INSPECT, value); }
            }

            public int interactive {
                get { return _GetVal(INDEX_INTERACTIVE); }
                internal set { _SetVal(INDEX_INTERACTIVE, value); }
            }

            public int optimize {
                get { return _GetVal(INDEX_OPTIMIZE); }
                internal set { _SetVal(INDEX_OPTIMIZE, value); }
            }

            public int dont_write_bytecode {
                get { return _GetVal(INDEX_DONT_WRITE_BYTECODE); }
                internal set { _SetVal(INDEX_DONT_WRITE_BYTECODE, value); }
            }

            public int no_user_site {
                get { return _GetVal(INDEX_NO_USER_SITE); }
                internal set { _SetVal(INDEX_NO_USER_SITE, value); }
            }

            public int no_site {
                get { return _GetVal(INDEX_NO_SITE); }
                internal set { _SetVal(INDEX_NO_SITE, value); }
            }

            public int ignore_environment {
                get { return _GetVal(INDEX_IGNORE_ENVIRONMENT); }
                internal set { _SetVal(INDEX_IGNORE_ENVIRONMENT, value); }
            }

            public int tabcheck {
                get { return _GetVal(INDEX_TABCHECK); }
                internal set { _SetVal(INDEX_TABCHECK, value); }
            }

            public int verbose {
                get { return _GetVal(INDEX_VERBOSE); }
                internal set { _SetVal(INDEX_VERBOSE, value); }
            }

            public int unicode {
                get { return _GetVal(INDEX_UNICODE); }
                internal set { _SetVal(INDEX_UNICODE, value); }
            }

            public int bytes_warning {
                get { return _GetVal(INDEX_BYTES_WARNING); }
                internal set { _SetVal(INDEX_BYTES_WARNING, value); }
            }

            #endregion
        }

        #endregion

        public static floatinfo float_info = new floatinfo(PythonTuple.MakeTuple(Double.MaxValue, 1024, 308, Double.MinValue, -1021, -307, 15, 53, Double.Epsilon, 2, 1), null);

        [PythonType, PythonHidden, DontMapIEnumerableToIter]
        public class floatinfo : IList, IList<object> {
            private readonly object _max, _dig, _mant_dig, _epsilon, _rounds, _max_exp, _max_10_exp, _min, _min_exp, _min_10_exp, _radix;

            public const int n_fields = 11;
            public const int n_sequence_fields = 11;
            public const int n_unnamed_fields = 0;

            public floatinfo(IList statResult, [DefaultParameterValue(null)]PythonDictionary dict) {
                // dict is allowed by CPython's float_info, but doesn't seem to do anything, so we ignore it here.

                if (statResult.Count < 10) {
                    throw PythonOps.TypeError("float_info() takes an at least 11-sequence ({0}-sequence given)", statResult.Count);
                }

                _max = statResult[0];
                _max_exp = statResult[1];
                _max_10_exp = statResult[2];
                _min = statResult[3];
                _min_exp = statResult[4];
                _min_10_exp = statResult[5];
                _dig = statResult[6];
                _mant_dig = statResult[7];
                _epsilon = statResult[8];
                _radix = statResult[9];
                _rounds = statResult[10];
            }

            private static object TryShrinkToInt(object value) {
                if (!(value is BigInteger)) {
                    return value;
                }

                return BigIntegerOps.__int__((BigInteger)value);
            }

            public object epsilon {
                get {
                    return _epsilon;
                }
            }

            public object mant_dig {
                get {
                    return _mant_dig;
                }
            }

            public object radix {
                get {
                    return _radix;
                }
            }

            public object rounds {
                get {
                    return _rounds;
                }
            }

            public object max_10_exp {
                get {
                    return _max_10_exp;
                }
            }

            public object min_10_exp {
                get {
                    return _min_10_exp;
                }
            }

            public object max_exp {
                get {
                    return _max_exp;
                }
            }

            public object max {
                get {
                    return _max;
                }
            }

            public object min {
                get {
                    return _min;
                }
            }

            public object dig {
                get {
                    return _dig;
                }
            }

            public object min_exp {
                get {
                    return _min_exp;
                }
            }

            public static PythonTuple operator +(floatinfo stat, PythonTuple tuple) {
                return stat.MakeTuple() + tuple;
            }

            public static bool operator >(floatinfo stat, IList o) {
                return stat.MakeTuple() > PythonTuple.Make(o);
            }

            public static bool operator <(floatinfo stat, IList o) {
                return stat.MakeTuple() > PythonTuple.Make(o);
            }

            public static bool operator >=(floatinfo stat, IList o) {
                return stat.MakeTuple() >= PythonTuple.Make(o);
            }

            public static bool operator <=(floatinfo stat, IList o) {
                return stat.MakeTuple() >= PythonTuple.Make(o);
            }

            public static bool operator >(floatinfo stat, object o) {
                return true;
            }

            public static bool operator <(floatinfo stat, object o) {
                return false;
            }

            public static bool operator >=(floatinfo stat, object o) {
                return true;
            }

            public static bool operator <=(floatinfo stat, object o) {
                return false;
            }

            public static PythonTuple operator *(floatinfo stat, int size) {
                return stat.MakeTuple() * size;
            }

            public static PythonTuple operator *(int size, floatinfo stat) {
                return stat.MakeTuple() * size;
            }

            public override string ToString() {
                return MakeTuple().ToString();
            }

            public string/*!*/ __repr__() {
                return ToString();
            }

            public PythonTuple __reduce__() {
                PythonDictionary emptyDict = new PythonDictionary(0);

                return PythonTuple.MakeTuple(
                    DynamicHelpers.GetPythonTypeFromType(typeof(floatinfo)),
                    PythonTuple.MakeTuple(MakeTuple(), emptyDict)
                );
            }

            #region ISequence Members

            public object this[int index] {
                get {
                    return MakeTuple()[index];
                }
            }

            public object this[Slice slice] {
                get {
                    return MakeTuple()[slice];
                }
            }

            public object __getslice__(int start, int stop) {
                return MakeTuple().__getslice__(start, stop);
            }

            public int __len__() {
                return MakeTuple().__len__();
            }

            public bool __contains__(object item) {
                return ((ICollection<object>)MakeTuple()).Contains(item);
            }

            #endregion

            private PythonTuple MakeTuple() {
                return PythonTuple.MakeTuple(
                    max,
                    max_exp,
                    max_10_exp,
                    min,
                    min_exp,
                    min_10_exp,
                    dig,
                    _mant_dig,
                    _epsilon,
                    _radix,
                    _rounds
                );
            }

            #region Object overrides

            public override bool Equals(object obj) {
                if (obj is floatinfo) {
                    return MakeTuple().Equals(((floatinfo)obj).MakeTuple());
                } else {
                    return MakeTuple().Equals(obj);
                }

            }

            public override int GetHashCode() {
                return MakeTuple().GetHashCode();
            }

            #endregion

            #region IList<object> Members

            int IList<object>.IndexOf(object item) {
                return MakeTuple().IndexOf(item);
            }

            void IList<object>.Insert(int index, object item) {
                throw new InvalidOperationException();
            }

            void IList<object>.RemoveAt(int index) {
                throw new InvalidOperationException();
            }

            object IList<object>.this[int index] {
                get {
                    return MakeTuple()[index];
                }
                set {
                    throw new InvalidOperationException();
                }
            }

            #endregion

            #region ICollection<object> Members

            void ICollection<object>.Add(object item) {
                throw new InvalidOperationException();
            }

            void ICollection<object>.Clear() {
                throw new InvalidOperationException();
            }

            bool ICollection<object>.Contains(object item) {
                return __contains__(item);
            }

            void ICollection<object>.CopyTo(object[] array, int arrayIndex) {
                throw new NotImplementedException();
            }

            int ICollection<object>.Count {
                get { return __len__(); }
            }

            bool ICollection<object>.IsReadOnly {
                get { return true; }
            }

            bool ICollection<object>.Remove(object item) {
                throw new InvalidOperationException();
            }

            #endregion

            #region IEnumerable<object> Members

            IEnumerator<object> IEnumerable<object>.GetEnumerator() {
                foreach (object o in MakeTuple()) {
                    yield return o;
                }
            }

            #endregion

            #region IEnumerable Members

            IEnumerator IEnumerable.GetEnumerator() {
                foreach (object o in MakeTuple()) {
                    yield return o;
                }
            }

            #endregion

            #region IList Members

            int IList.Add(object value) {
                throw new InvalidOperationException();
            }

            void IList.Clear() {
                throw new InvalidOperationException();
            }

            bool IList.Contains(object value) {
                return __contains__(value);
            }

            int IList.IndexOf(object value) {
                return MakeTuple().IndexOf(value);
            }

            void IList.Insert(int index, object value) {
                throw new InvalidOperationException();
            }

            bool IList.IsFixedSize {
                get { return true; }
            }

            bool IList.IsReadOnly {
                get { return true; }
            }

            void IList.Remove(object value) {
                throw new InvalidOperationException();
            }

            void IList.RemoveAt(int index) {
                throw new InvalidOperationException();
            }

            object IList.this[int index] {
                get {
                    return MakeTuple()[index];
                }
                set {
                    throw new InvalidOperationException();
                }
            }

            #endregion

            #region ICollection Members

            void ICollection.CopyTo(Array array, int index) {
                throw new NotImplementedException();
            }

            int ICollection.Count {
                get { return __len__(); }
            }

            bool ICollection.IsSynchronized {
                get { return false; }
            }

            object ICollection.SyncRoot {
                get { return this; }
            }

            #endregion
        }

        [SpecialName]
        public static void PerformModuleReload(PythonContext/*!*/ context, PythonDictionary/*!*/ dict) {
            dict["stdin"] = dict["__stdin__"];
            dict["stdout"] = dict["__stdout__"];
            dict["stderr"] = dict["__stderr__"];

            // !!! These fields do need to be reset on "reload(sys)". However, the initial value is specified by the 
            // engine elsewhere. For now, we initialize them just once to some default value
            dict["warnoptions"] = new List(0);

            PublishBuiltinModuleNames(context, dict);
            context.SetHostVariables(dict);

            dict["meta_path"] = new List(0);
            dict["path_hooks"] = new List(0);
            dict["path_importer_cache"] = new PythonDictionary();
        }

        internal static void PublishBuiltinModuleNames(PythonContext/*!*/ context, PythonDictionary/*!*/ dict) {
            object[] keys = new object[context.BuiltinModules.Keys.Count];
            int index = 0;
            foreach (object key in context.BuiltinModules.Keys) {
                keys[index++] = key;
            }
            dict["builtin_module_names"] = PythonTuple.MakeTuple(keys);
        }

    }
}
