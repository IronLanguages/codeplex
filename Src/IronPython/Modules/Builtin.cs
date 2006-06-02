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
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Diagnostics;

using IronPython.Runtime;
using IronPython.Compiler;

using IronMath;

[assembly: PythonModule("__builtin__", typeof(IronPython.Modules.Builtin))]
namespace IronPython.Modules {
    public static partial class Builtin {
        public static object True = Ops.Bool2Object(true);
        public static object False = Ops.Bool2Object(false);

        public static object None;
        public static object Ellipsis = Ops.Ellipsis;
        public static object NotImplemented = Ops.NotImplemented;

        public static object exit = "Use Ctrl-Z plus Return to exit";
        public static object quit = "Use Ctrl-Z plus Return to exit";

        public static bool __debug__ {
            [PythonName("__debug__")]
            get {
                return IronPython.Hosting.PythonEngine.options.DebugMode;
            }
        }

        [PythonName("__import__")]
        [Documentation("__import__(name) -> module\n\nImport a module.")]
        public static object __import__(ICallerContext context, string name) {
            return __import__(context, name, null, null, null);
        }

        [PythonName("__import__")]
        [Documentation("__import__(name, globals) -> module\n\nImport a module.")]
        public static object __import__(ICallerContext context, string name, object globals) {
            return __import__(context, name, null, null, null);
        }

        [PythonName("__import__")]
        [Documentation("__import__(name, globals, locals) -> module\n\nImport a module.")]
        public static object __import__(ICallerContext context, string name, object globals, object locals) {
            return __import__(context, name, null, null, null);
        }

        [PythonName("__import__")]
        [Documentation("__import__(name, globals, locals, fromlist) -> module\n\nImport a module.")]
        public static object __import__(ICallerContext context, string name, object globals, object locals, object fromList) {
            List from = fromList as List;

            object ret = Importer.ImportModule(context, name, from != null && from.GetLength() > 0);
            if (ret == null) {
                throw Ops.ImportError("No module named {0}", name);
            }
            return ret;
        }

        [PythonName("abs")]
        [Documentation("abs(number) -> number\n\nReturn the absolute value of the argument.")]
        public static object Abs(object o) {
            if (o is int) return IntOps.Abs((int)o);
            if (o is long) return Int64Ops.Abs((long)o);
            if (o is double) return FloatOps.Abs((double)o);
            if (o is bool) return (((bool)o) ? 1 : 0);
            if (o is string) throw Ops.TypeError("bad operand type for abs()");
            BigInteger bi = o as BigInteger;
            if (!Object.Equals(bi, null)) return LongOps.Abs(bi);
            if (o is Complex64) return ComplexOps.Abs((Complex64)o);

            object ret;
            if (Ops.TryToInvoke(o, SymbolTable.AbsoluteValue, out ret)) {
                return ret;
            } else {
                throw Ops.TypeError("bad operand type for abs()");
            }
        }

        [PythonName("apply")]
        [Documentation("apply(object[, args[, kwargs]]) -> value\n\nDeprecated.\nInstead, use:\n    function(*args, **keywords).")]
        public static object Apply(ICallerContext context, object func, object args) {
            return Ops.CallWithArgsTupleAndContext(context, func, new object[0], args);
        }

        [PythonName("apply")]
        [Documentation("apply(object[, args[, kwargs]]) -> value\n\nDeprecated.\nInstead, use:\n    function(*args, **keywords).")]
        public static object Apply(ICallerContext context, object func, object args, object kws) {
            return Ops.CallWithArgsTupleAndKeywordDictAndContext(context, func, new object[0], new string[0], args, kws);
        }

        public static object basestring = Ops.GetDynamicTypeFromType(typeof(string)); //!!! more unicode "unification"

        public static object @bool = Ops.GetDynamicTypeFromType(typeof(bool));


        public static object buffer = Ops.GetDynamicTypeFromType(typeof(PythonBuffer));

        [PythonName("callable")]
        [Documentation("callable(object) -> bool\n\nReturn whether the object is callable (i.e., some kind of function).")]
        public static object Callable(object o) {
            object dummy;
            if (Ops.TryGetAttr(DefaultContext.Default, o, SymbolTable.Call, out dummy))
                return Ops.TRUE;

            if (o is ICallable) {
                return Ops.TRUE;
            }
            return Ops.FALSE;
        }

        [PythonName("chr")]
        [Documentation("chr(i) -> character\n\nReturn a string of one character with ordinal i; 0 <= i< 256.")]
        public static string Chr(int value) {
            if (value < 0 || value > 0xFF) {
                throw Ops.ValueError("{0} is not in required range", value);
            }
            return Ops.Char2String((char)value);
        }

        private static object TryCoerce(object x, object y) {
            object res;
            if (Ops.TryToInvoke(x, SymbolTable.Coerce, out res, y)) {
                return res;
            }
            return null;
        }

        [PythonName("coerce")]
        [Documentation("coerce(x, y) -> (x1, y1)\n\nReturn a tuple consisting of the two numeric arguments converted to\na common type. If coercion is not possible, raise TypeError.")]
        public static object Coerce(object x, object y) {
            Conversion conversion;
            object converted;

            if (x == null && y == null) {
                return Tuple.MakeTuple(null, null);
            }

            if (x != null) {
                converted = Converter.TryConvert(y, x.GetType(), out conversion);
                if (conversion < Conversion.Truncation) {
                    return Tuple.MakeTuple(x, converted);
                }
            }
            if (y != null) {
                converted = Converter.TryConvert(x, y.GetType(), out conversion);
                if (conversion < Conversion.Truncation) {
                    return Tuple.MakeTuple(converted, y);
                }
            }

            converted = TryCoerce(x, y);
            if (converted != null) {
                return converted;
            }
            converted = TryCoerce(y, x);
            if (converted != null) {
                return Tuple.Make(Reversed(converted));
            }

            throw Ops.TypeError("coercion failed");
        }

        [Flags]
        private enum CompileFlags {
            CO_NESTED = 0x0010,              //  nested_scopes
            CO_GENERATOR_ALLOWED = 0x1000,   //  generators
            CO_FUTURE_DIVISION = 0x2000,   //  division
        }

        [PythonName("compile")]
        public static object Compile(ICallerContext context, string source, string filename, string kind, object flags, object dontInherit) {
            CompilerContext cc;
            CompileFlags cflags = 0;
            bool inheritContext = (dontInherit == null || Converter.ConvertToInt32(dontInherit) == 0);
            
            if (inheritContext) {
                cc = context.CreateCompilerContext().CopyWithNewSourceFile(filename);
            } else {
                cc = new CompilerContext(filename);
            }

            if (flags != null) {
                cflags = (CompileFlags)Converter.ConvertToInt32(flags);
            }
            if ((cflags & ~(CompileFlags.CO_NESTED | CompileFlags.CO_GENERATOR_ALLOWED | CompileFlags.CO_FUTURE_DIVISION)) != 0) {
                throw Ops.ValueError("unrecognized flags");
            }

            cc.TrueDivision = inheritContext && context.TrueDivision || ((cflags & CompileFlags.CO_FUTURE_DIVISION) != 0);

            Parser p = Parser.FromString(context.SystemState, cc, source);
            FrameCode code;

            if (kind == "exec") {
                Stmt s = p.ParseFileInput();
                code = OutputGenerator.GenerateSnippet(cc, s, false);
            } else if (kind == "eval") {
                Expr e = p.ParseTestListAsExpression();
                code = OutputGenerator.GenerateSnippet(cc, new ReturnStmt(e), true);
            } else if (kind == "single") {
                Stmt s = p.ParseSingleStatement();
                code = OutputGenerator.GenerateSnippet(cc, s, true);
            } else {
                throw Ops.ValueError("compile() arg 3 must be 'exec' or 'eval' or 'single'");
            }
            return new FunctionCode(code);
        }

        [PythonName("compile")]
        public static object Compile(ICallerContext context, string source, string filename, string kind, object flags) {
            return Compile(context, source, filename, kind, flags, null);
        }

        [PythonName("compile")]
        public static object Compile(ICallerContext context, string source, string filename, string kind) {
            return Compile(context, source, filename, kind, null, null);
        }

        public static object classmethod = Ops.GetDynamicTypeFromType(typeof(ClassMethod));

        [PythonName("cmp")]
        public static int Compare(object x, object y) {
            return Ops.Compare(x, y);
        }

        public static object complex = Ops.GetDynamicTypeFromType(typeof(Complex64));

        [PythonName("delattr")]
        public static void DelAttr(object o, string name) {
            Ops.DelAttr(DefaultContext.Default, o, SymbolTable.StringToId(name));
        }

        public static object dict = Ops.GetDynamicTypeFromType(typeof(Dict));

        [PythonName("dir")]
        public static List Dir(ICallerContext context) {
            IDictionary dict = context.Locals as IDictionary;
            if (dict != null) {
                List list = List.Make(dict.Keys);
                list.Sort();
                return list;
            } else return Dir(context, context.Module);
        }

        [PythonName("dir")]
        public static List Dir(ICallerContext context, object o) {
            List ret = Ops.GetAttrNames(context, o);
            ret.Sort();
            return ret;
        }

        // Python has lots of optimizations for this method that we may want to implement in the future
        [PythonName("divmod")]
        public static Tuple DivMod(object x, object y) {
            return Tuple.MakeTuple(Ops.FloorDivide(x, y), Ops.Mod(x, y));
        }

        public static object enumerate = Ops.GetDynamicTypeFromType(typeof(Enumerate));

        [PythonName("eval")]
        public static object Eval(ICallerContext context, FunctionCode code) {
            return Eval(context, code, null);
        }

        [PythonName("eval")]
        public static object Eval(ICallerContext context, FunctionCode code, IDictionary<object, object> globals) {
            return Eval(context, code, globals, globals);
        }

        [PythonName("eval")]
        public static object Eval(ICallerContext context, FunctionCode code, IDictionary<object, object> globals, object locals) {
            if (globals == null) globals = Globals(context);
            if (locals == null) locals = Locals(context);
            PythonModule mod = new PythonModule(context.Module.ModuleName, globals, context.SystemState, null, context.ContextFlags);
            return code.Call(new Frame(mod, globals, locals));
        }

        [PythonName("eval")]
        public static object Eval(ICallerContext context, string expression) {
            return Eval(context, expression, Globals(context), Locals(context));
        }

        [PythonName("eval")]
        public static object Eval(ICallerContext context, string expression, IDictionary<object, object> globals) {
            return Eval(context, expression, globals, globals);
        }

        [PythonName("eval")]
        public static object Eval(ICallerContext context, string expression, IDictionary<object, object> globals, object locals) {
            if (locals != null && PythonOperator.IsMappingType(context, locals) == Ops.FALSE) {
                throw Ops.TypeError("locals must be mapping");
            }

            CompilerContext cc = context.CreateCompilerContext();
            Parser p = Parser.FromString(context.SystemState, cc, expression.TrimStart(' ', '\t'));
            Expr e = p.ParseTestListAsExpression();

            if (globals == null) globals = Globals(context);
            if (locals == null) locals = Locals(context);

            PythonModule mod = new PythonModule("<eval>", globals, context.SystemState, null, context.ContextFlags);
            if (Options.FastEval) {//!!! experimenting with a HUGE (>100x) performance boost to simple evals
                return e.Evaluate(new NameEnv(mod, locals));
            } else {
                Stmt s = new ReturnStmt(e);
                FrameCode fc = OutputGenerator.GenerateSnippet(cc, s, false);
                return fc.Run(new Frame(mod, globals, locals));
            }
        }

        [PythonName("execfile")]
        public static object ExecFile(ICallerContext context, object filename) {
            return ExecFile(context, filename, null, null);
        }

        [PythonName("execfile")]
        public static object ExecFile(ICallerContext context, object filename, object globals) {
            return ExecFile(context, filename, globals, null);
        }

        [PythonName("execfile")]
        public static object ExecFile(ICallerContext context, object filename, object globals, object locals) {
            PythonModule mod = context.Module;
            if (globals == null) {
                globals = mod.__dict__;
            }
            if (locals == null) {
                locals = globals;
            }

            string fname = Converter.ConvertToString(filename);
            IDictionary<object, object> g = globals as IDictionary<object, object>;
            if (g == null) {
                throw Ops.TypeError("execfile: arg 2 must be dictionary");
            }

            CompilerContext cc = context.CreateCompilerContext().CopyWithNewSourceFile(fname);
            Parser p;
            try {
                p = Parser.FromFile(context.SystemState, cc);
            } catch (UnauthorizedAccessException x) {
                throw Ops.IOError(x.Message);
            }
            Stmt s = p.ParseFileInput();

            IDictionary<object, object> l = locals as IDictionary<object, object>;
            if (l == null) {
                throw Ops.TypeError("execfile: arg 3 must be dictionary");
            }

            Frame topFrame = new Frame(mod, g, l);
            FrameCode code = OutputGenerator.GenerateSnippet(cc, s, false);
            code.Run(topFrame);
            return null;
        }

        public static object file = Ops.GetDynamicTypeFromType(typeof(PythonFile));

        [PythonName("filter")]
        public static object Filter(object function, object list) {
            string str = list as string;
            if (str != null) {
                if (function == null) return str;
                StringBuilder sb = new StringBuilder();
                foreach (char c in str) {
                    if (Ops.IsTrue(Ops.Call(function, Ops.Char2String(c)))) sb.Append(c);
                }
                return sb.ToString();
            } else if (IsSubClass(Ops.GetDynamicType(list), Ops.GetDynamicTypeFromType(typeof(string)))) {
                StringBuilder sb = new StringBuilder();
                IEnumerator e = Ops.GetEnumerator(list);
                while (e.MoveNext()) {
                    object o = e.Current;
                    object t = (function != null) ? Ops.Call(function, o) : o;

                    if (!Ops.IsTrue(t))
                        continue;

                    sb.Append(Converter.ConvertToString(o));
                }
                return sb.ToString();
            }

            List ret = new List();

            IEnumerator i = Ops.GetEnumerator(list);
            while (i.MoveNext()) {
                if (function == null) {
                    if (Ops.IsTrue(i.Current)) ret.AddNoLock(i.Current);
                } else {
                    if (Ops.IsTrue(Ops.Call(function, i.Current))) ret.AddNoLock(i.Current);
                }
            }

            if (IsInstance(list, Ops.GetDynamicTypeFromType(typeof(Tuple)))) {
                return Tuple.Make(ret);
            } else {
                return ret;
            }
        }

        public static object @float = Ops.GetDynamicTypeFromType(typeof(double));

        [PythonName("getattr")]
        public static object GetAttr(ICallerContext context, object o, string name) {
            return Ops.GetAttr(context, o, SymbolTable.StringToId(name));
        }

        [PythonName("getattr")]
        public static object GetAttr(ICallerContext context, object o, string name, object def) {
            object ret;
            if (Ops.TryGetAttr(context, o, SymbolTable.StringToId(name), out ret)) return ret;
            else return def;
        }

        [PythonName("globals")]
        public static IDictionary<object, object> Globals(ICallerContext context) {
            PythonModule mod = context.Module;
            return ((IDictionary<object,object>)mod.__dict__);
        }

        [PythonName("hasattr")]
        public static object HasAttr(ICallerContext context, object o, string name) {
            object tmp;
            try {
                return Ops.Bool2Object(Ops.TryGetAttr(context, o, SymbolTable.StringToId(name), out tmp));
            } catch {
                return Ops.FALSE;
            }
        }

        [PythonName("hash")]
        public static int Hash(object o) {
            return Ops.Hash(o);
        }

        [PythonName("help")]
        public static void Help(ICallerContext context, object o) {
            StringBuilder doc = new StringBuilder();
            ArrayList doced = new ArrayList();  // document things only once

            Help(context, doced, doc, 0, o);

            if (doc.Length == 0) {
                doc.Append("no documentation found for ");
                doc.Append(Ops.StringRepr(o));
            }

            string[] strings = doc.ToString().Split('\n');
            for (int i = 0; i < strings.Length; i++) {
                /* should read only a key, not a line, but we don't seem
                 * to have a way to do that...
                if ((i % Console.WindowHeight) == 0) {
                    Ops.Print(context.SystemState, "-- More --");
                    Ops.ReadLineFromSrc(context.SystemState);
                }*/
                Ops.Print(context.SystemState, strings[i]);
            }
        }

        private static void Help(ICallerContext context, ArrayList doced, StringBuilder doc, int indent, object o) {
            DynamicType dt;
            BuiltinFunction bf;
            PythonFunction pf;
            BuiltinMethodDescriptor methodDesc;
            string strVal;

            if (doced.Contains(o)) return;  // document things only once
            doced.Add(o);

            if ((strVal = o as string) != null) {
                // try and find things that string could refer to,
                // then call help on them.
                foreach (KeyValuePair<object, object> kvp in context.SystemState.modules) {
                    object pm = kvp.Value;

                    List attrs = Ops.GetAttrNames(context, pm);
                    List candidates = new List();
                    foreach (string s in attrs) {
                        if (s == strVal) {
                            object modVal;
                            if(!Ops.TryGetAttr(context, pm, SymbolTable.StringToId(strVal), out modVal))
                                continue;

                            candidates.Add(modVal);
                        }
                    }

                    // favor types, then built-in functions, then python functions,
                    // and then only display help for one.
                    dt = null;
                    bf = null;
                    pf = null;
                    for (int i = 0; i < candidates.Count; i++) {
                        if ((dt = candidates[i] as DynamicType) != null) {
                            break;
                        }

                        if (bf == null && (bf = candidates[i] as BuiltinFunction) != null)
                            continue;

                        if (pf == null && (pf = candidates[i] as PythonFunction) != null)
                            continue;
                    }

                    if (dt != null) Help(context, doced, doc, indent, dt);
                    else if (bf != null) Help(context, doced, doc, indent, bf);
                    else if (pf != null) Help(context, doced, doc, indent, pf);
                }
            } else if ((dt = o as DynamicType) != null) {
                // find all the functions, and display their 
                // documentation
                if (indent == 0) doc.AppendFormat("Help on {0} in module {1}\n\n", dt.__name__, Ops.GetAttr(context, dt, SymbolTable.Module));
                List names = dt.GetAttrNames(context, null);
                names.Sort();
                foreach (string name in names) {
                    if (name == "__class__") continue;

                    object value;
                    if (Ops.TryGetAttr(context, o, SymbolTable.StringToId(name), out value))
                        Help(context, doced, doc, indent + 1, value);
                }
            } else if ((methodDesc = o as BuiltinMethodDescriptor) != null) {
                if (indent == 0) doc.AppendFormat("Help on method-descriptor {0}\n\n", methodDesc.Name);
                AppendIndent(doc, indent);
                doc.Append(methodDesc.Name);
                doc.Append("(...)\n");

                AppendMultiLine(doc, Converter.ConvertToString(methodDesc.Documentation), indent + 1);
            } else if ((bf = o as BuiltinFunction) != null) {
                if (indent == 0) doc.AppendFormat("Help on built-in function {0}\n\n", bf.Name);
                AppendIndent(doc, indent);
                doc.Append(bf.Name);
                doc.Append("(...)\n");

                AppendMultiLine(doc, bf.Documentation, indent + 1);
            } else if ((pf = o as PythonFunction) != null) {
                if (indent == 0) doc.AppendFormat("Help on function {0} in module {1}\n\n", pf.Name, pf.Module.ModuleName);

                AppendIndent(doc, indent);
                doc.AppendFormat("{0}({1})\n", pf.Name, String.Join(", ", pf.argNames));
                string pfDoc = Converter.ConvertToString(pf.Documentation);
                if (!String.IsNullOrEmpty(pfDoc)) {
                    AppendMultiLine(doc, pfDoc, indent);
                }
            } else if (o is PythonModule) {

            }
        }

        private static void AppendMultiLine(StringBuilder doc, string multiline, int indent) {
            string[] docs = multiline.Split('\n');
            for (int i = 0; i < docs.Length; i++) {
                AppendIndent(doc, indent + 1);
                doc.Append(docs[i]);
                doc.Append('\n');
            }
        }

        private static void AppendIndent(StringBuilder doc, int indent) {
            doc.Append(" |  ");
            for (int i = 0; i < indent; i++) doc.Append("    ");
        }

        //??? type this to string
        [PythonName("hex")]
        public static object Hex(object o) {
            return Ops.Hex(o);
        }

        [PythonName("id")]
        public static long Id(object o) {
            return Ops.Id(o);
        }

        [PythonName("input")]
        public static object Input(ICallerContext context) {
            return Input(context, null);
        }

        [PythonName("input")]
        public static object Input(ICallerContext context, object prompt) {
            return Eval(context, RawInput(context, prompt));
        }

        public static object @int = Ops.GetDynamicTypeFromType(typeof(int));

        [PythonName("intern")]
        public static string Intern(object o) {
            string s = o as string;
            if (s == null) {
                throw Ops.TypeError("intern: argument must be string");
            }
            return string.Intern(s);
        }

        [PythonName("isinstance")]
        public static object IsInstanceWrapper(object o, object typeinfo) {
            return Ops.Bool2Object(IsInstance(o, typeinfo));
        }

        internal static bool IsInstance(object o, object typeinfo) {
            if (typeinfo is OldClass) {
                // old instances are strange - they all share a common type
                // of instance but they can "be subclasses" of other
                // OldClass's.  To check their types we need the actual
                // instance.
                OldInstance oi = o as OldInstance;
                if (oi != null)  return oi.__class__.IsSubclassOf(typeinfo);
            }
            return IsSubClass(Ops.GetDynamicType(o), typeinfo);
        }

        [PythonName("issubclass")]
        public static object IsSubClassWrapper(DynamicType c, object typeinfo) {
            return Ops.Bool2Object(IsSubClass(c, typeinfo));
        }

        internal static bool IsSubClass(DynamicType c, object typeinfo) {
            if (c == null) {
                throw Ops.TypeError("issubclass: arg 1 must be a class");
            }

            Tuple pt = typeinfo as Tuple;
            if (pt == null) {
                if (!(typeinfo is DynamicType)) {
                    throw Ops.TypeError("issubclass(): {0} is not a class nor a tuple of classes",
                        typeinfo == null ? "None" : typeinfo);
                }
                return c.IsSubclassOf(typeinfo);
            } else {
                foreach (object o in pt) {
                    if (!(o is DynamicType)) {
                        throw Ops.TypeError("issubclass(): tuple contains object that is not a class: {0}", o);
                    }

                    if (c.IsSubclassOf(o)) return true;
                }
                return false;
            }
        }

        [PythonName("iter")]
        public static IEnumerator Iter(object o) {
            return Ops.GetEnumerator(o);
        }

        [PythonName("iter")]
        public static object Iter(object func, object sentinel) {
            if (PythonOperator.IsCallable(func) == Ops.FALSE) {
                throw Ops.TypeError("iter(v, w): v must be callable");
            }
            return new SentinelIterator(func, sentinel);
        }

        [PythonName("len")]
        public static int Length(object o) {
            string s = o as String;
            if (s != null) return s.Length;

            ISequence seq = o as ISequence;
            if (seq != null) return seq.GetLength();

            ICollection ic = o as ICollection;
            if (ic != null) return ic.Count;

            return Converter.ConvertToInt32(Ops.Invoke(o, SymbolTable.Length));
        }


        public static object set = Ops.GetDynamicTypeFromType(typeof(SetCollection));
        public static object frozenset = Ops.GetDynamicTypeFromType(typeof(FrozenSetCollection));
        public static object list = Ops.GetDynamicTypeFromType(typeof(List));

        [PythonName("locals")]
        public static object Locals(ICallerContext context) {
            return context.Locals;
        }

        public static object @long = Ops.GetDynamicTypeFromType(typeof(BigInteger));

        [PythonName("map")]
        public static List Map(params object[] param) {
            if (param == null || param.Length < 2) {
                throw Ops.TypeError("at least 2 arguments required to map");
            }
            List ret = new List();
            object func = param[0];
            if (param.Length == 2) {
                IEnumerator i = Ops.GetEnumerator(param[1]);
                while (i.MoveNext()) {
                    if (func == null) ret.AddNoLock(i.Current);
                    else ret.AddNoLock(Ops.Call(func, i.Current));
                }
                return ret;
            } else {
                IEnumerator[] enums = new IEnumerator[param.Length - 1];
                for (int i = 0; i < enums.Length; i++) {
                    enums[i] = Ops.GetEnumerator(param[i + 1]);
                }

                object[] args = new object[enums.Length];
                while (true) {
                    bool done = true;
                    for (int i = 0; i < enums.Length; i++) {
                        if (enums[i].MoveNext()) {
                            args[i] = enums[i].Current;
                            done = false;
                        } else {
                            args[i] = null;
                        }
                    }
                    if (done) {
                        return ret;
                    }
                    if (func != null) {
                        ret.AddNoLock(Ops.Call(func, args));
                    } else {
                        ret.AddNoLock(Tuple.MakeTuple(args));
                        args = new object[enums.Length];    // Tuple does not copy the array, allocate new one.
                    }
                }
            }
        }

        [PythonName("max")]
        static object Max(object x) {
            IEnumerator i = Ops.GetEnumerator(x);
            i.MoveNext();
            object ret = i.Current;
            while (i.MoveNext()) {
                if (Ops.GreaterThanRetBool(i.Current, ret)) ret = i.Current;
            }
            return ret;
        }

        [PythonName("max")]
        public static object Max(object x, object y) {
            return Ops.GreaterThanRetBool(x, y) ? x : y;
        }

        [PythonName("max")]
        public static object Max(params object[] args) {
            object ret = args[0];
            if (args.Length == 1) return Max(ret);
            for (int i = 1; i < args.Length; i++) {
                if (Ops.GreaterThanRetBool(args[i], ret)) ret = args[i];
            }
            return ret;
        }

        [PythonName("min")]
        static object Min(object x) {
            IEnumerator i = Ops.GetEnumerator(x);
            if (!i.MoveNext()) {
                throw Ops.ValueError("empty sequence");
            }
            object ret = i.Current;
            while (i.MoveNext()) {
                if (Ops.LessThanRetBool(i.Current, ret)) ret = i.Current;
            }
            return ret;
        }

        [PythonName("min")]
        public static object Min(object x, object y) {
            return Ops.LessThanRetBool(x, y) ? x : y;
        }

        [PythonName("min")]
        public static object Min(params object[] args) {
            if (args.Length > 0) {
                object ret = args[0];
                if (args.Length == 1) return Min(ret);
                for (int i = 1; i < args.Length; i++) {
                    if (Ops.LessThanRetBool(args[i], ret)) ret = args[i];
                }
                return ret;
            } else {
                throw Ops.TypeError("min expecting 1 arguments, got 0");
            }
        }


        public static object @object = Ops.GetDynamicTypeFromType(typeof(object));

        [PythonName("oct")]
        public static object Oct(object o) {
            return Ops.Oct(o);
        }

        public static object open = Ops.GetDynamicTypeFromType(typeof(PythonFile));

        [PythonName("ord")]
        public static int Ord(object value) {
            char ch;

            if (value is char) {
                ch = (char)value;
            } else {
                string stringValue = value as string;
                if (stringValue == null) {
                    ExtensibleString es = value as ExtensibleString;
                    if (es != null) stringValue = es.Value;
                }
                if (stringValue != null) {
                    if (stringValue.Length != 1) {
                        throw Ops.TypeError("expected a character, but string of length {0} found", stringValue.Length);
                    }
                    ch = stringValue[0];
                } else {
                    throw Ops.TypeError("expected a character, but {0} found", Ops.GetDynamicType(value));
                }
            }
            return (int)ch;
        }

        [PythonName("pow")]
        public static object Pow(object x, object y) {
            return Ops.Power(x, y);
        }

        [PythonName("pow")]
        public static object Pow(object x, object y, object z) {
            try {
                return Ops.PowerMod(x, y, z);
            } catch (DivideByZeroException) {
                throw Ops.ValueError("3rd adgument cannot be 0");
            }
        }

        public static object property = Ops.GetDynamicTypeFromType(typeof(Property));

        [PythonName("range")]
        public static object Range(object stop) {
            if (stop is int) {
                return rangeWorker((int)stop);
            }
            return rangeWorker(Converter.ConvertToBigInteger(stop));
        }

        private static object rangeWorker(int stop) {
            if (stop < 0) {
                stop = 0;
            }

            List ret = List.MakeEmptyList(stop);
            for (int i = 0; i < stop; i++) ret.AddNoLock(Ops.Int2Object(i));
            return ret;
        }

        private static object rangeWorker(BigInteger stop) {
            if (stop < BigInteger.Zero) {
                return Range(0);
            }
            int istop;
            if (stop.AsInt32(out istop)) {
                return Range(istop);
            }
            throw Ops.OverflowError("too many items in the range");
        }

        [PythonName("range")]
        public static object Range(object start, object stop) {
            if (start is int && stop is int) {
                return rangeWorker((int)start, (int)stop);
            }
            return rangeWorker(Converter.ConvertToBigInteger(start), Converter.ConvertToBigInteger(stop));
        }

        private static object rangeWorker(int start, int stop) {
            if (start > stop) {
                stop = start;
            }

            long length = (long)stop - (long)start;
            if (Int32.MinValue <= length && length <= Int32.MaxValue) {
                List ret = List.MakeEmptyList(stop - start);
                for (int i = start; i < stop; i++) ret.AddNoLock(Ops.Int2Object(i));
                return ret;
            }
            throw Ops.OverflowError("too many items in the list");
        }

        private static object rangeWorker(BigInteger start, BigInteger stop) {
            if (start > stop) {
                stop = start;
            }
            BigInteger length = stop - start;
            int ilength;
            if (length.AsInt32(out ilength)) {
                List ret = List.MakeEmptyList(ilength);
                for (int i = 0; i < ilength; i++) {
                    ret.AddNoLock(start + i);
                }
                return ret;
            }
            throw Ops.OverflowError("too many items in the range");
        }

        [PythonName("range")]
        public static object Range(object start, object stop, object step) {
            if (start is int && stop is int && step is int) {
                return rangeWorker((int)start, (int)stop, (int)step);
            }
            return rangeWorker(Converter.ConvertToBigInteger(start), Converter.ConvertToBigInteger(stop), Converter.ConvertToBigInteger(step));
        }

        private static object rangeWorker(int start, int stop, int step) {
            if (step == 0) {
                throw Ops.ValueError("step of 0");
            }

            List ret;
            if (step > 0) {
                if (start > stop) stop = start;
                ret = List.MakeEmptyList((stop - start) / step);
                for (int i = start; i < stop; i += step) {
                    ret.AddNoLock(Ops.Int2Object(i));
                }
            } else {
                if (start < stop) stop = start;
                ret = List.MakeEmptyList((stop - start) / step);
                for (int i = start; i > stop; i += step) {
                    ret.AddNoLock(Ops.Int2Object(i));
                }
            }
            return ret;
        }

        private static object rangeWorker(BigInteger start, BigInteger stop, BigInteger step) {
            if (step == BigInteger.Zero) {
                throw Ops.ValueError("step of 0");
            }
            BigInteger length;
            if (step > BigInteger.Zero) {
                if (start > stop) stop = start;
                length = (stop - start + step - 1) / step;
            } else {
                if (start < stop) stop = start;
                length = (stop - start + step + 1) / step;
            }

            int ilength;
            if (length.AsInt32(out ilength)) {
                List ret = List.MakeEmptyList(ilength);
                for (int i = 0; i < ilength; i++) {
                    ret.AddNoLock(start);
                    start = start + step;
                }
                return ret;
            }
            throw Ops.OverflowError("too many items for list");
        }

        [PythonName("raw_input")]
        public static string RawInput(ICallerContext context) {
            return RawInput(context, null);
        }

        [PythonName("raw_input")]
        public static string RawInput(ICallerContext context, object prompt) {
            if (prompt != null) {
                Ops.PrintNoNewline(context.SystemState, prompt);
            }
            string line = Ops.ReadLineFromSrc(context.SystemState.stdin) as string;
            if (line != null && line.EndsWith("\n")) return line.Substring(0, line.Length - 1);
            return line;
        }

        [PythonName("reduce")]
        public static object Reduce(object func, object seq) {
            IEnumerator i = Ops.GetEnumerator(seq);
            if (!i.MoveNext()) {
                throw Ops.TypeError("reduce() of empty sequence with no initial value");
            }
            object ret = i.Current;
            while (i.MoveNext()) {
                ret = Ops.Call(func, ret, i.Current);
            }
            return ret;
        }

        [PythonName("reduce")]
        public static object Reduce(object func, object seq, object initializer) {
            IEnumerator i = Ops.GetEnumerator(seq);
            object ret = initializer;
            while (i.MoveNext()) {
                ret = Ops.Call(func, ret, i.Current);
            }
            return ret;
        }

        private static int reloadCounter;

        [PythonName("reload")]
        public static object Reload(PythonModule module) {
            if (module.Filename == null) return Importer.ReloadBuiltin(module);

            CompilerContext cc = new CompilerContext(module.Filename);
            Parser parser = Parser.FromFile(module.SystemState, cc);
            Stmt s = parser.ParseFileInput();
            PythonModule pmod = OutputGenerator.GenerateModule(module.SystemState, cc, s, module.ModuleName, "__" + System.Threading.Interlocked.Increment(ref reloadCounter));

            foreach (KeyValuePair<object, object> attr in module.__dict__) {
                if (pmod.__dict__.ContainsObjectKey(attr.Key)) continue;
                pmod.__dict__.AddObjectKey(attr.Key, attr.Value);
            }

            module.UpdateForReload(pmod);

            return module;
        }

        [PythonName("reload")]
        public static object Reload(SystemState state) {
            if (state == null) throw Ops.TypeError("unexpected type: NoneType");

            state.Initialize();

            return state;
        }

        [PythonName("repr")]
        public static object Repr(object o) {
            object res = Ops.StringRepr(o);

            if (!(res is String) && !(res is ExtensibleString)) {
                throw Ops.TypeError("__repr__ returned non-string (type {0})", Ops.GetDynamicType(res).__name__);
            }

            return res;  
        }

        [PythonName("reversed")]
        public static object Reversed(object o) {
            object reversed;
            if (Ops.TryGetAttr(o, SymbolTable.Reversed, out reversed)) {
                return Ops.Call(reversed);
            }

            object getitem;
            object len;

            //!!! OldClass check: we currently are in a strange state where we partially support
            // descriptors on old-style classes, although we're not supposed to.  We special
            // case it here until that's fixed.
            if (o is OldClass ||
                !Ops.TryGetAttr(o, SymbolTable.GetItem, out getitem) ||
                !Ops.TryGetAttr(o, SymbolTable.Length, out len) ||
                o is Dict) {
                throw Ops.TypeError("argument to reversed() must be a sequence");
            }

            object length = Ops.Call(len);
            if (!(length is int)) {
                throw Ops.ValueError("__len__ must return int");
            }
            return new ReversedEnumerator((int)length, getitem);
        }

        [PythonName("round")]
        public static double Round(double x) {
            return Math.Round(x, MidpointRounding.AwayFromZero);
        }

        [PythonName("round")]
        public static double Round(double x, int n) {
            // values are rounded to 10 ^ n.  For negative values
            // we need to handle this ourselves.
            if (n < 0) { 
                double factor = Math.Pow(10.0, -n);
                return factor * Math.Round(x / factor, MidpointRounding.AwayFromZero);
            }
            return Math.Round(x, n);
        }

        [PythonName("setattr")]
        public static void SetAttr(ICallerContext context, object o, string name, object val) {
            Ops.SetAttr(context, o, SymbolTable.StringToId(name), val);
        }

        public static object slice = Ops.GetDynamicTypeFromType(typeof(Slice));

        [PythonName("sorted")]
        public static List Sorted(object iterable) {
            return Sorted(iterable, null, null, false);
        }

        [PythonName("sorted")]
        public static List Sorted(object iterable, object cmp) {
            return Sorted(iterable, cmp, null, false);
        }

        [PythonName("sorted")]
        public static List Sorted(object iterable, object cmp, object key) {
            return Sorted(iterable, cmp, key, false);
        }

        [PythonName("sorted")]
        public static List Sorted([DefaultParameterValueAttribute(null)] object iterable,
            [DefaultParameterValueAttribute(null)]object cmp,
            [DefaultParameterValueAttribute(null)]object key,
            [DefaultParameterValueAttribute(false)]bool reverse) {

            IEnumerator iter = Ops.GetEnumerator(iterable);
            List l = List.MakeEmptyList(10);
            while (iter.MoveNext()) {
                l.AddNoLock(iter.Current);
            }
            l.Sort(cmp, key, reverse);
            return l;
        }

        public static object staticmethod = Ops.GetDynamicTypeFromType(typeof(StaticMethod));

        [PythonName("sum")]
        public static object Sum(object sequence) {
            return Sum(sequence, 0);
        }

        [PythonName("sum")]
        public static object Sum(object sequence, object start) {
            IEnumerator i = Ops.GetEnumerator(sequence);

            if (start is string) {
                throw Ops.TypeError("Cannot sum strings, use '{0}'.join(seq)", start);
            }

            object ret = start;
            while (i.MoveNext()) {
                ret = Ops.Add(ret, i.Current);
            }
            return ret;
        }

        public static object super = Ops.GetDynamicTypeFromType(typeof(Super));

        public static object str = Ops.GetDynamicTypeFromType(typeof(string));

        public static object tuple = Ops.GetDynamicTypeFromType(typeof(Tuple));

        public static object type = Ops.GetDynamicTypeFromType(typeof(PythonType));  //!!!

        //public static object utype = Ops.GetDynamicTypeFromType(typeof(UserType));  //!!!

        [PythonName("unichr")]
        public static string Unichr(int i) {
            if (i < Char.MinValue || i > Char.MaxValue) {
                throw Ops.ValueError("{0} is not in required range", i);
            }
            return Ops.Char2String((char)i);
        }

        public static object unicode = Ops.GetDynamicTypeFromType(typeof(string)); //!!! unicode str diffs

        [PythonName("vars")]
        [Documentation("vars([object]) -> dictionary\n\nWithout arguments, equivalent to locals().\nWith an argument, equivalent to object.__dict__.")]
        public static object Vars(ICallerContext context) {
            return context.Locals;
        }

        [PythonName("vars")]
        public static object Vars(ICallerContext context, object @object) {
            object result;
            try {
                result = Ops.GetAttrDict(context, @object);
            } catch (MissingMemberException e) {
                if (e.Message.Contains("has no attribute '__dict__'"))
                    throw Ops.TypeError("vars() argument must have __dict__ attribute");
                throw;
            }
            return result;
        }

        public static object xrange = Ops.GetDynamicTypeFromType(typeof(XRange)); //PyXRange.pytype;

        [PythonName("zip")]
        public static List Zip(object s0, object s1) {
            IEnumerator i0 = Ops.GetEnumerator(s0);
            IEnumerator i1 = Ops.GetEnumerator(s1);
            List ret = new List();
            while (i0.MoveNext() && i1.MoveNext()) {
                ret.AddNoLock(Tuple.MakeTuple(i0.Current, i1.Current));
            }
            return ret;
        }


        //??? should we fastpath the 1,2,3 item cases???
        [PythonName("zip")]
        public static List Zip(params object[] seqs) {
            if (seqs == null) throw Ops.TypeError("zip argument must support iteration, got {0}", NoneType.InstanceOfNoneType);

            int N = seqs.Length;
            if (N == 2) return Zip(seqs[0], seqs[1]);
            if (N == 0) return List.Make();
            IEnumerator[] iters = new IEnumerator[N];
            for (int i = 0; i < N; i++) iters[i] = Ops.GetEnumerator(seqs[i]);

            List ret = new List();
            while (true) {
                object[] items = new object[N];
                for (int i = 0; i < N; i++) {
                    // first iterator which is no longer iterable ends the 
                    // loop.
                    if (!iters[i].MoveNext()) return ret;  
                    items[i] = iters[i].Current;
                }
                ret.AddNoLock(Tuple.Make(items));
            }
        }

        public static object Exception = ExceptionConverter.GetPythonExceptionByName("Exception");
    }
}
