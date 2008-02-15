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
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Compiler;
using IronPython.Hosting;
using IronPython.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

[assembly: PythonModule("__builtin__", typeof(Builtin))]
namespace IronPython.Runtime {
    [Documentation("")]  // Documentation suppresses XML Doc on startup.
    public static partial class Builtin {

        public static readonly object True = RuntimeHelpers.True;
        public static readonly object False = RuntimeHelpers.False;

        // This will always stay null
        public static readonly object None;

        public static readonly object Ellipsis = PythonOps.Ellipsis;
        public static readonly object NotImplemented = PythonOps.NotImplemented;

        public static readonly object exit = "Use Ctrl-Z plus Return to exit";
        public static readonly object quit = "Use Ctrl-Z plus Return to exit";

        [Documentation("__import__(name) -> module\n\nImport a module.")]
        public static object __import__(CodeContext/*!*/ context, string name) {
            return __import__(context, name, null, null, null);
        }

        [Documentation("__import__(name, globals) -> module\n\nImport a module.")]
        public static object __import__(CodeContext/*!*/ context, string name, object globals) {
            return __import__(context, name, globals, null, null);

        }

        [Documentation("__import__(name, globals, locals) -> module\n\nImport a module.")]
        public static object __import__(CodeContext/*!*/ context, string name, object globals, object locals) {
            return __import__(context, name, globals, locals, null);
        }

        [Documentation("__import__(name, globals, locals, fromlist) -> module\n\nImport a module.")]
        public static object __import__(CodeContext/*!*/ context, string name, object globals, object locals, object fromList) {
            //!!! remove suppress in GlobalSuppressions.cs when CodePlex 2704 is fixed.
            ISequence from = fromList as ISequence;

            object ret = Importer.ImportModule(context, name, from != null && from.__len__() > 0);
            if (ret == null) {
                throw PythonOps.ImportError("No module named {0}", name);
            }

            Scope mod = ret as Scope;
            if (mod != null && from != null) {
                string strAttrName;
                object attrValue;
                for (int i = 0; i < from.__len__(); i++) {
                    object attrName = from[i];

                    if (Converter.TryConvertToString(attrName, out strAttrName) && strAttrName != null && strAttrName != "*") {
                        try {
                            attrValue = Importer.ImportFrom(context, mod, strAttrName);
                        } catch (ImportException) {
                            continue;
                        }
                    }
                }
            }

            return ret;
        }

        [Documentation("abs(number) -> number\n\nReturn the absolute value of the argument.")]
        public static object abs(CodeContext/*!*/ context, object o) {
            if (o is int) return Int32Ops.Abs((int)o);
            if (o is long) return Int64Ops.Abs((long)o);
            if (o is double) return DoubleOps.Abs((double)o);
            if (o is bool) return (((bool)o) ? 1 : 0);
            if (o is string) throw PythonOps.TypeError("bad operand type for abs()");
            BigInteger bi = o as BigInteger;
            if (!Object.ReferenceEquals(bi, null)) return BigIntegerOps.Abs(bi);
            if (o is Complex64) return ComplexOps.Abs((Complex64)o);

            object value;
            if (PythonTypeOps.TryInvokeUnaryOperator(context, o, Symbols.AbsoluteValue, out value)) {
                return value;
            }

            throw PythonOps.TypeError("bad operand type for abs()");            
        }

        public static bool all(object x) {
            IEnumerator i = PythonOps.GetEnumerator(x);
            while (i.MoveNext()) {
                if (!PythonOps.IsTrue(i.Current)) return false;
            }
            return true;
        }

        public static bool any(object x) {
            IEnumerator i = PythonOps.GetEnumerator(x);
            while (i.MoveNext()) {
                if (PythonOps.IsTrue(i.Current)) return true;
            }
            return false;
        }

        [Documentation("apply(object[, args[, kwargs]]) -> value\n\nDeprecated.\nInstead, use:\n    function(*args, **keywords).")]
        public static object apply(CodeContext/*!*/ context, object func) {
            return PythonOps.CallWithContext(context, func);
        }

        public static object apply(CodeContext/*!*/ context, object func, object args) {
            return PythonOps.CallWithArgsTupleAndContext(context, func, ArrayUtils.EmptyObjects, args);
        }

        public static object apply(CodeContext/*!*/ context, object func, object args, object kws) {
            return PythonOps.CallWithArgsTupleAndKeywordDictAndContext(context, func, ArrayUtils.EmptyObjects, ArrayUtils.EmptyStrings, args, kws);
        }

        public static readonly PythonType basestring = DynamicHelpers.GetPythonTypeFromType(typeof(string));

        public static readonly PythonType @bool = DynamicHelpers.GetPythonTypeFromType(typeof(bool));


        public static readonly PythonType buffer = DynamicHelpers.GetPythonTypeFromType(typeof(PythonBuffer));

        [Documentation("callable(object) -> bool\n\nReturn whether the object is callable (i.e., some kind of function).")]
        public static bool callable(object o) {
            return PythonOps.IsCallable(o);
        }

        [Documentation("chr(i) -> character\n\nReturn a string of one character with ordinal i; 0 <= i< 256.")]
        public static string chr(int value) {
            if (value < 0 || value > 0xFF) {
                throw PythonOps.ValueError("{0} is not in required range", value);
            }
            return RuntimeHelpers.CharToString((char)value);
        }

        internal static object TryCoerce(CodeContext/*!*/ context, object x, object y) {
            PythonTypeSlot pts;
            PythonType xType = DynamicHelpers.GetPythonType(x);

            if (xType.TryResolveSlot(context, Symbols.Coerce, out pts)) {
                object callable;
                if (pts.TryGetBoundValue(context, x, xType, out callable)) {
                    return PythonCalls.Call(callable, y);
                }
            }
            return PythonOps.NotImplemented;
        }

        [Documentation("coerce(x, y) -> (x1, y1)\n\nReturn a tuple consisting of the two numeric arguments converted to\na common type. If coercion is not possible, raise TypeError.")]
        public static object coerce(CodeContext/*!*/ context, object x, object y) {
            object converted;

            if (x == null && y == null) {
                return PythonTuple.MakeTuple(null, null);
            }

            converted = TryCoerce(context, x, y);
            if (converted != null && converted != PythonOps.NotImplemented) {
                return converted;
            }

            converted = TryCoerce(context, y, x);
            if (converted != null && converted != PythonOps.NotImplemented) {
                return PythonTuple.Make(reversed(converted));
            }

            throw PythonOps.TypeError("coercion failed");
        }

        [Documentation("compile a unit of source code.\n\nThe source can be compiled either as exec, eval, or single.\nexec compiles the code as if it were a file\neval compiles the code as if were an expression\nsingle compiles a single statement\n\n")]
        public static object compile(CodeContext/*!*/ context, string source, string filename, string kind, object flags, object dontInherit) {
            if (source.IndexOf('\0') != -1) {
                throw PythonOps.TypeError("compile() expected string without null bytes");
            }

            bool inheritContext = GetCompilerInheritance(dontInherit);
            CompileFlags cflags = GetCompilerFlags(flags);
            CompilerOptions opts = GetDefaultCompilerOptions(context, inheritContext, cflags);
            SourceUnit sourceUnit;
            string unitPath = String.IsNullOrEmpty(filename) ? null : filename;

            switch (kind) {
                case "exec": sourceUnit = context.LanguageContext.CreateSnippet(source, unitPath, SourceCodeKind.Default); break;
                case "eval": sourceUnit = context.LanguageContext.CreateSnippet(source, unitPath, SourceCodeKind.Expression); break;
                case "single": sourceUnit = context.LanguageContext.CreateSnippet(source, unitPath, SourceCodeKind.SingleStatement); break;
                default: 
                    throw PythonOps.ValueError("compile() arg 3 must be 'exec' or 'eval' or 'single'");
            }

            if ((cflags & CompileFlags.CO_DONT_IMPLY_DEDENT) != 0) {
                // re-parse in interactive code mode to see if this is valid
                ErrorSink es = new CompilerErrorSink();
                SourceUnit altSourceUnit = context.LanguageContext.CreateSnippet(source, unitPath, SourceCodeKind.InteractiveCode);
                CompilerContext compilerContext = new CompilerContext(altSourceUnit, null, es);
                Parser p = Parser.CreateParser(compilerContext, PythonContext.GetPythonOptions(context), false, false);
                SourceCodeProperties prop;
                IronPython.Compiler.Ast.PythonAst ast = p.ParseInteractiveCode(out prop);

                if (prop == SourceCodeProperties.IsIncompleteStatement) {
                    throw PythonOps.SyntaxError("invalid syntax", sourceUnit, ast != null ? ast.Body.Span : SourceSpan.Invalid, p.ErrorCode);
                }
            }

            ScriptCode compiledCode = context.LanguageContext.CompileSourceCode(sourceUnit, opts);
            compiledCode.EnsureCompiled();

            FunctionCode res = new FunctionCode(compiledCode, cflags);
            res.SetFilename(filename);
            return res;
        }

        [Documentation("compile a unit of source code.\n\nThe source can be compiled either as exec, eval, or single.\nexec compiles the code as if it were a file\neval compiles the code as if were an expression\nsingle compiles a single statement\n\n")]
        public static object compile(CodeContext/*!*/ context, string source, string filename, string kind, object flags) {
            return compile(context, source, filename, kind, flags, null);
        }

        [Documentation("compile a unit of source code.\n\nThe source can be compiled either as exec, eval, or single.\nexec compiles the code as if it were a file\neval compiles the code as if were an expression\nsingle compiles a single statement\n\n")]
        public static object compile(CodeContext/*!*/ context, string source, string filename, string kind) {
            return compile(context, source, filename, kind, null, null);
        }

        public static readonly PythonType classmethod = DynamicHelpers.GetPythonTypeFromType(typeof(classmethod));

        public static int cmp(CodeContext/*!*/ context, object x, object y) {
            return PythonOps.Compare(context, x, y);
        }

        public static readonly PythonType complex = DynamicHelpers.GetPythonTypeFromType(typeof(Complex64));

        public static void delattr(CodeContext/*!*/ context, object o, string name) {
            PythonOps.DeleteAttr(context, o, SymbolTable.StringToId(name)); 
        }

        public static readonly PythonType dict = DynamicHelpers.GetPythonTypeFromType(typeof(PythonDictionary));

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]
        public static List dir(CodeContext/*!*/ context) {
            List res = List.Make(LocalsAsAttributesCollection(context).Keys);

            res.Sort();
            return res;
        }

        public static List dir(CodeContext/*!*/ context, object o) {
            IList<object> ret = PythonOps.GetAttrNames(context, o);
            List lret = new List(ret);
            lret.Sort();
            return lret;
        }

        // Python has lots of optimizations for this method that we may want to implement in the future
        // In particular, this should be treated like the other binary operators
        private static readonly FastDynamicSite<object, object, object> _divmodSite = FastDynamicSite<object, object, object>.Create(DefaultContext.Default, DoOperationAction.Make(Operators.DivMod));
        private static readonly FastDynamicSite<object, object, object> _rdivmodSite = FastDynamicSite<object, object, object>.Create(DefaultContext.Default, DoOperationAction.Make(Operators.ReverseDivMod));

        public static object divmod(CodeContext/*!*/ context, object x, object y) {
            Debug.Assert(PythonOps.NotImplemented != null);

            object ret = _divmodSite.Invoke(x, y);
            if (ret != PythonOps.NotImplemented) {
                return ret;
            }

            ret = _rdivmodSite.Invoke(x, y);
            if (ret != PythonOps.NotImplemented) {
                return ret;
            }

            throw PythonOps.TypeErrorForBinaryOp("divmod", x, y);
        }

        public static readonly PythonType enumerate = DynamicHelpers.GetPythonTypeFromType(typeof(Enumerate));

        public static object eval(CodeContext/*!*/ context, FunctionCode code) {
            Debug.Assert(context != null);
            if (code == null) throw PythonOps.TypeError("eval() argument 1 must be string or code object");

            return eval(context, code, null);
        }

        public static object eval(CodeContext/*!*/ context, FunctionCode code, IAttributesCollection globals) {
            Debug.Assert(context != null);
            if (code == null) throw PythonOps.TypeError("eval() argument 1 must be string or code object");

            return eval(context, code, globals, globals);
        }

        public static object eval(CodeContext/*!*/ context, FunctionCode code, IAttributesCollection globals, object locals) {
            Debug.Assert(context != null);
            if (code == null) throw PythonOps.TypeError("eval() argument 1 must be string or code object");

            Scope scope = GetExecEvalScope(context, globals, locals);

            return code.Call(context, scope, false); // Do not try evaluate mode for compiled code

        }

        internal static IAttributesCollection GetAttrLocals(CodeContext/*!*/ context, object locals) {
            IAttributesCollection attrLocals;
            if (locals == null) {
                attrLocals = LocalsAsAttributesCollection(context);
            } else {
                attrLocals = locals as IAttributesCollection ?? new ObjectAttributesAdapter(locals);
            }
            return attrLocals;
        }

        public static object eval(CodeContext/*!*/ context, string expression) {
            Debug.Assert(context != null);
            if (expression == null) throw PythonOps.TypeError("eval() argument 1 must be string or code object");

            return eval(context, expression, globals(context), locals(context));
        }

        public static object eval(CodeContext/*!*/ context, string expression, IAttributesCollection globals) {
            Debug.Assert(context != null);
            if (expression == null) throw PythonOps.TypeError("eval() argument 1 must be string or code object");

            return eval(context, expression, globals, globals);
        }

        public static object eval(CodeContext/*!*/ context, string expression, IAttributesCollection globals, object locals) {
            Debug.Assert(context != null);
            if (expression == null) throw PythonOps.TypeError("eval() argument 1 must be string or code object");

            if (locals != null && PythonOps.IsMappingType(context, locals) == RuntimeHelpers.False) {
                throw PythonOps.TypeError("locals must be mapping");
            }

            Scope scope = GetExecEvalScope(context, globals, locals);

            // TODO: remove TrimStart
            SourceUnit expr_code = context.LanguageContext.CreateSnippet(expression.TrimStart(' ', '\t'), SourceCodeKind.Expression);

            return context.LanguageContext.CompileSourceCode(expr_code, GetDefaultCompilerOptions(context, true, 0)).Run(scope, context.ModuleContext, true);
        }

        public static void execfile(CodeContext/*!*/ context, object filename) {
            execfile(context, filename, null, null);
        }

        public static void execfile(CodeContext/*!*/ context, object filename, object globals) {
            execfile(context, filename, globals, null);
        }

        public static void execfile(CodeContext/*!*/ context, object filename, object globals, object locals) {
            IAttributesCollection g = globals as IAttributesCollection;
            if (g == null && globals != null) {
                throw PythonOps.TypeError("execfile: arg 2 must be dictionary");
            }

            IAttributesCollection l = locals as IAttributesCollection;
            if (l == null && locals != null) {
                throw PythonOps.TypeError("execfile: arg 3 must be dictionary");
            }

            if (l == null) l = g;

            Scope execScope = GetExecEvalScope(context, g, l);
            string path = Converter.ConvertToString(filename);
            SourceUnit sourceUnit = context.LanguageContext.TryGetSourceFileUnit(path, PythonContext.GetContext(context).DefaultEncoding, SourceCodeKind.File);

            if (sourceUnit == null) {
                throw PythonOps.IOError("execfile: specified file doesn't exist");
            }

            ScriptCode code;

            try {
                code = context.LanguageContext.CompileSourceCode(sourceUnit, GetDefaultCompilerOptions(context, true, 0));
            } catch (UnauthorizedAccessException x) {
                throw PythonOps.IOError(x);
            }

            code.Run(execScope, ((PythonModule)context.ModuleContext).Clone(), false); // Do not attempt evaluation mode for execfile
        }

        public static readonly PythonType file = DynamicHelpers.GetPythonTypeFromType(typeof(PythonFile));

        public static string filter(object function, [NotNull]string list) {
            if (function == null) return list;
            if (list == null) throw PythonOps.TypeError("NoneType is not iterable");

            StringBuilder sb = new StringBuilder();
            foreach (char c in list) {
                if (PythonOps.IsTrue(PythonCalls.Call(function, RuntimeHelpers.CharToString(c)))) sb.Append(c);
            }

            return sb.ToString();
        }

        public static string filter(object function, [NotNull]ExtensibleString list) {
            StringBuilder sb = new StringBuilder();
            IEnumerator e = PythonOps.GetEnumerator(list);
            while (e.MoveNext()) {
                object o = e.Current;
                object t = (function != null) ? PythonCalls.Call(function, o) : o;

                if (PythonOps.IsTrue(t)) {
                    sb.Append(Converter.ConvertToString(o));
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Specialized version because enumerating tuples by Python's definition
        /// doesn't call __getitem__, but filter does!
        /// </summary>
        public static PythonTuple filter(object function, [NotNull]PythonTuple tuple) {
            List<object> res = new List<object>(tuple.Count);
            
            for (int i = 0; i < tuple.Count; i++) {
                object obj = tuple[i];
                object t = (function != null) ? PythonCalls.Call(function, obj) : obj;

                if (PythonOps.IsTrue(t)) {
                    res.Add(obj);
                }
            }

            return PythonTuple.MakeTuple(res.ToArray());
        }

        public static List filter(object function, object list) {
            if (list == null) throw PythonOps.TypeError("NoneType is not iterable");
            List ret = new List();

            IEnumerator i = PythonOps.GetEnumerator(list);
            while (i.MoveNext()) {
                if (function == null) {
                    if (PythonOps.IsTrue(i.Current)) ret.AddNoLock(i.Current);
                } else {
                    if (PythonOps.IsTrue(PythonCalls.Call(function, i.Current))) ret.AddNoLock(i.Current);
                }
            }

            return ret;
        }

        public static readonly PythonType @float = DynamicHelpers.GetPythonTypeFromType(typeof(double));

        public static object getattr(CodeContext/*!*/ context, object o, string name) {
            return PythonOps.GetBoundAttr(context, o, SymbolTable.StringToId(name));
        }

        public static object getattr(CodeContext/*!*/ context, object o, string name, object def) {
            object ret;
            if (PythonOps.TryGetBoundAttr(context, o, SymbolTable.StringToId(name), out ret)) return ret;
            else return def;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]
        public static IAttributesCollection globals(CodeContext/*!*/ context) {
            return new GlobalsDictionary(context.Scope);
        }

        public static bool hasattr(CodeContext/*!*/ context, object o, string name) {
            return PythonOps.HasAttr(context, o, SymbolTable.StringToId(name));
        }

        public static int hash(object o) {
            return PythonOps.Hash(o);
        }

        public static void help(CodeContext/*!*/ context, object o) {
            StringBuilder doc = new StringBuilder();
            List<object> doced = new List<object>();  // document things only once

            help(context, doced, doc, 0, o);

            if (doc.Length == 0) {
                if (!(o is string)) {
                    help(context, DynamicHelpers.GetPythonType(o));
                    return;
                }
                doc.Append("no documentation found for ");
                doc.Append(PythonOps.StringRepr(o));
            }

            string[] strings = doc.ToString().Split('\n');
            for (int i = 0; i < strings.Length; i++) {
                /* should read only a key, not a line, but we don't seem
                 * to have a way to do that...
                if ((i % Console.WindowHeight) == 0) {
                    Ops.Print(context.SystemState, "-- More --");
                    Ops.ReadLineFromSrc(context.SystemState);
                }*/
                PythonOps.Print(context, strings[i]);
            }
        }

        private static void help(CodeContext/*!*/ context, List<object>/*!*/ doced, StringBuilder/*!*/ doc, int indent, object obj) {
            PythonType type;
            BuiltinFunction builtinFunction;
            BoundBuiltinFunction boundBuiltinFunction;
            PythonFunction function;
            BuiltinMethodDescriptor methodDesc;
            Method method;
            string strVal;
            Scope scope;
            OldClass oldClass;

            if (doced.Contains(obj)) return;  // document things only once
            doced.Add(obj);

            if ((strVal = obj as string) != null) {
                if (indent != 0) return;

                // try and find things that string could refer to,
                // then call help on them.
                foreach (object module in PythonContext.GetContext(context).SystemStateModules.Values) {
                    IList<object> attrs = PythonOps.GetAttrNames(context, module);
                    PythonType modType = DynamicHelpers.GetPythonType(module);
                    List candidates = new List();
                    foreach (string s in attrs) {
                        if (s == strVal) {
                            object modVal;
                            if (!PythonOps.TryGetBoundAttr(context, module, SymbolTable.StringToId(strVal), out modVal))
                                continue;

                            candidates.Add(modVal);
                        }
                    }

                    // favor types, then built-in functions, then python functions,
                    // and then only display help for one.
                    type = null;
                    builtinFunction = null;
                    function = null;
                    for (int i = 0; i < candidates.Count; i++) {
                        if ((type = candidates[i] as PythonType) != null) {
                            break;
                        }

                        if (builtinFunction == null && (builtinFunction = candidates[i] as BuiltinFunction) != null)
                            continue;

                        if (function == null && (function = candidates[i] as PythonFunction) != null)
                            continue;
                    }

                    if (type != null) help(context, doced, doc, indent, type);
                    else if (builtinFunction != null) help(context, doced, doc, indent, builtinFunction);
                    else if (function != null) help(context, doced, doc, indent, function);
                }
            } else if ((type = obj as PythonType) != null) {
                // find all the functions, and display their 
                // documentation                
                if (indent == 0) {
                    doc.AppendFormat("Help on {0} in module {1}\n\n", type.Name, PythonOps.GetBoundAttr(context, type, Symbols.Module));
                }

                PythonTypeSlot dts;
                if (type.TryResolveSlot(context, Symbols.Doc, out dts)) {
                    object docText;
                    if (dts.TryGetValue(context, null, type, out docText) && docText != null)
                        AppendMultiLine(doc, docText.ToString() + Environment.NewLine, indent);
                    AppendIndent(doc, indent);
                    doc.AppendLine("Data and other attributes defined here:");
                    AppendIndent(doc, indent);
                    doc.AppendLine();
                }

                List<SymbolId> names = new List<SymbolId>(type.GetMemberNames(context, null));
                names.Sort(delegate(SymbolId left, SymbolId right) {
                    return String.Compare(left.ToString(), right.ToString());
                });

                foreach (SymbolId name in names) {
                    if (name == Symbols.Class) continue;

                    PythonTypeSlot value;
                    object val;
                    if (type.TryLookupSlot(context, name, out value) && value.TryGetValue(context, null, type, out val)) {
                        help(context, doced, doc, indent + 1, val);
                    }
                }
            } else if ((methodDesc = obj as BuiltinMethodDescriptor) != null) {
                if (indent == 0) doc.AppendFormat("Help on method-descriptor {0}\n\n", methodDesc.__name__);
                AppendIndent(doc, indent);
                doc.Append(methodDesc.__name__);
                doc.Append("(...)\n");

                AppendMultiLine(doc, methodDesc.__doc__, indent + 1);
            } else if ((boundBuiltinFunction = obj as BoundBuiltinFunction) != null) {
                if (indent == 0) doc.AppendFormat("Help on built-in function {0}\n\n", boundBuiltinFunction.Target.__name__);

                AppendIndent(doc, indent);
                doc.Append(boundBuiltinFunction.Target.__name__);
                doc.Append("(...)\n");

                AppendMultiLine(doc, boundBuiltinFunction.Target.__doc__, indent + 1);
            } else if ((builtinFunction = obj as BuiltinFunction) != null) {
                if (indent == 0) doc.AppendFormat("Help on built-in function {0}\n\n", builtinFunction.Name);
                AppendIndent(doc, indent);
                doc.Append(builtinFunction.Name);
                doc.Append("(...)\n");

                AppendMultiLine(doc, builtinFunction.__doc__, indent + 1);
            } else if ((function = obj as PythonFunction) != null) {
                if (indent == 0) doc.AppendFormat("Help on function {0} in module {1}:\n\n", function.__name__, function.__module__);

                AppendIndent(doc, indent);
                doc.Append(function.GetSignatureString());
                string pfDoc = Converter.ConvertToString(function.__doc__);
                if (!String.IsNullOrEmpty(pfDoc)) {
                    AppendMultiLine(doc, pfDoc, indent);
                }
            } else if ((method = obj as Method) != null && ((function = method.im_func as PythonFunction) != null)) {
                if (indent == 0) doc.AppendFormat("Help on method {0} in module {1}:\n\n", function.__name__, function.__module__);

                AppendIndent(doc, indent);
                doc.Append(function.GetSignatureString());

                if (method.im_self == null) {
                    doc.AppendFormat(" unbound {0} method\n", PythonOps.ToString(method.im_class));
                } else {
                    doc.AppendFormat(" method of {0} instance\n", PythonOps.ToString(method.im_class));
                }

                string pfDoc = Converter.ConvertToString(function.__doc__);
                if (!String.IsNullOrEmpty(pfDoc)) {
                    AppendMultiLine(doc, pfDoc, indent);
                }
            } else if ((scope = obj as Scope) != null) {
                foreach (SymbolId name in scope.Keys) {
                    if (name == Symbols.Class || name == Symbols.Builtins) continue;

                    object value;
                    if (scope.TryGetName(context.LanguageContext, name, out value)) {
                        help(context, doced, doc, indent + 1, value);
                    }
                }
            } else if ((oldClass = obj as OldClass) != null) {
                if (indent == 0) {
                    doc.AppendFormat("Help on {0} in module {1}\n\n", oldClass.Name, PythonOps.GetBoundAttr(context, oldClass, Symbols.Module));
                }

                object docText;
                if (oldClass.TryLookupSlot(Symbols.Doc, out docText) && docText != null) {
                    AppendMultiLine(doc, docText.ToString() + Environment.NewLine, indent);
                    AppendIndent(doc, indent);
                    doc.AppendLine("Data and other attributes defined here:");
                    AppendIndent(doc, indent);
                    doc.AppendLine();
                }

                IList<object> names = oldClass.GetMemberNames(context);
                List sortNames = new List(names);
                sortNames.Sort();
                names = sortNames;
                foreach (string name in names) {
                    if (name == "__class__") continue;

                    object value;

                    if (oldClass.TryLookupSlot(SymbolTable.StringToId(name), out value))
                        help(context, doced, doc, indent + 1, value);
                }
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
        public static object hex(object o) {
            return PythonOps.Hex(o);
        }

        public static long id(object o) {
            return PythonOps.Id(o);
        }

        public static object input(CodeContext/*!*/ context) {
            return input(context, null);
        }

        public static object input(CodeContext/*!*/ context, object prompt) {
            return eval(context, raw_input(context, prompt));
        }

        public static readonly PythonType @int = DynamicHelpers.GetPythonTypeFromType(typeof(int));

        public static string intern(object o) {
            string s = o as string;
            if (s == null) {
                throw PythonOps.TypeError("intern: argument must be string");
            }
            return string.Intern(s);
        }

        public static bool isinstance(object o, object typeinfo) {
            return PythonOps.IsInstance(o, typeinfo);
        }

        public static bool issubclass(OldClass c, object typeinfo) {
            return PythonOps.IsSubClass(c.TypeObject, typeinfo);
        }

        public static bool issubclass(PythonType c, object typeinfo) {
            return PythonOps.IsSubClass(c, typeinfo);
        }

        public static IEnumerator iter(object o) {
            return PythonOps.GetEnumerator(o);
        }

        public static object iter(object func, object sentinel) {
            if (!PythonOps.IsCallable(func)) {
                throw PythonOps.TypeError("iter(v, w): v must be callable");
            }
            return new SentinelIterator(func, sentinel);
        }

        public static int len(object o) {
            return PythonOps.Length(o);
        }


        public static readonly PythonType set = DynamicHelpers.GetPythonTypeFromType(typeof(SetCollection));
        public static readonly PythonType frozenset = DynamicHelpers.GetPythonTypeFromType(typeof(FrozenSetCollection));
        public static readonly PythonType list = DynamicHelpers.GetPythonTypeFromType(typeof(List));

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]
        public static object locals(CodeContext/*!*/ context) {            
            ObjectAttributesAdapter adapter = context.Scope.Dict as ObjectAttributesAdapter;
            if(adapter != null) {
                // we've wrapped Locals in an IAttributesCollection, give the user back the
                // original object.
                return adapter.Backing;
            }

            return LocalsAsAttributesCollection(context);
        }

        internal static IAttributesCollection LocalsAsAttributesCollection(CodeContext/*!*/ context) {
            return LocalsDictionary.GetDictionaryFromScope(context.Scope);
        }

        public static readonly PythonType @long = DynamicHelpers.GetPythonTypeFromType(typeof(BigInteger));

        public static List map(params object[] param) {
            if (param == null || param.Length < 2) {
                throw PythonOps.TypeError("at least 2 arguments required to map");
            }
            List ret = new List();
            object func = param[0];
            
            if (param.Length == 2) {
                FastDynamicSite<object, object, object> mapSite = null;
                if (func != null) mapSite = RuntimeHelpers.CreateSimpleCallSite<object, object, object>(DefaultContext.Default);
                
                IEnumerator i = PythonOps.GetEnumerator(param[1]);
                while (i.MoveNext()) {
                    if (func == null) ret.AddNoLock(i.Current);
                    else ret.AddNoLock(mapSite.Invoke(func, i.Current));
                }
                return ret;
            } else {
                IEnumerator[] enums = new IEnumerator[param.Length - 1];
                for (int i = 0; i < enums.Length; i++) {
                    enums[i] = PythonOps.GetEnumerator(param[i + 1]);
                }

                object[] args = new object[enums.Length];
                FastDynamicSite<object, object[], object> mapSite = null;
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
                        // splat call w/ args, can't use site here yet...
                        if (mapSite == null) mapSite = FastDynamicSite<object, object[], object>.Create(DefaultContext.Default, 
                            CallAction.Make(new CallSignature(ArgumentKind.List)));

                        ret.AddNoLock(mapSite.Invoke(func, args));
                    } else {
                        ret.AddNoLock(PythonTuple.MakeTuple(args));
                        args = new object[enums.Length];    // Tuple does not copy the array, allocate new one.
                    }
                }
            }
        }

        public static object max(object x) {
            IEnumerator i = PythonOps.GetEnumerator(x);
            if (!i.MoveNext())
                throw PythonOps.ValueError("max() arg is an empty sequence");
            object ret = i.Current;
            while (i.MoveNext()) {
                if (PythonSites.GreaterThanRetBool(i.Current, ret)) ret = i.Current;
            }
            return ret;
        }

        public static object max(object x, object y) {
            return PythonSites.GreaterThanRetBool(x, y) ? x : y;
        }

        public static object max(params object[] args) {
            if (args.Length > 0) {
                object ret = args[0];
                if (args.Length == 1) return max(ret);
                for (int i = 1; i < args.Length; i++) {
                    if (PythonSites.GreaterThanRetBool(args[i], ret)) ret = args[i];
                }
                return ret;
            } else {
                throw PythonOps.TypeError("max expecting 1 arguments, got 0");
            }

        }

        public static object max(object x, [ParamDictionary] IAttributesCollection dict) {
            IEnumerator i = PythonOps.GetEnumerator(x);
            if (!i.MoveNext())
                throw PythonOps.ValueError(" max() arg is an empty sequence");
            object method = GetMaxKwArg(dict);
            object ret = i.Current;
            object retValue = PythonCalls.Call(method, i.Current);
            while (i.MoveNext()) {
                object tmpRetValue = PythonCalls.Call(method, i.Current);
                if (PythonSites.GreaterThanRetBool(tmpRetValue, retValue)) {
                    ret = i.Current;
                    retValue = tmpRetValue;
                }
            }
            return ret;
        }

        public static object max(object x, object y, [ParamDictionary] IAttributesCollection dict) {
            object method = GetMaxKwArg(dict);
            return PythonSites.GreaterThanRetBool(PythonCalls.Call(method, x), PythonCalls.Call(method, y)) ? x : y;
        }

        public static object max([ParamDictionary] IAttributesCollection dict, params object[] args) {
            if (args.Length > 0) {
                int retIndex = 0;
                if (args.Length == 1) return max(args[retIndex], dict);
                object method = GetMaxKwArg(dict);
                object retValue = PythonCalls.Call(method, args[retIndex]);
                for (int i = 1; i < args.Length; i++) {
                    object tmpRetValue = PythonCalls.Call(method, args[i]);
                    if (PythonSites.GreaterThanRetBool(tmpRetValue, retValue)) {
                        retIndex = i;
                        retValue = tmpRetValue;
                    }
                }
                return args[retIndex];
            } else {
                throw PythonOps.TypeError("max expecting 1 arguments, got 0");
            }
        }

        private static object GetMaxKwArg([ParamDictionary] IAttributesCollection dict) {
            if (dict.Count != 1)
                throw PythonOps.TypeError(" max() should have only 1 keyword argument, but got {0} keyword arguments", dict.Count);

            return VerifyKeys("max", dict);
        }

        public static object min(object x) {
            IEnumerator i = PythonOps.GetEnumerator(x);
            if (!i.MoveNext()) {
                throw PythonOps.ValueError("empty sequence");
            }
            object ret = i.Current;
            while (i.MoveNext()) {
                if (PythonSites.LessThanRetBool(i.Current, ret)) ret = i.Current;
            }
            return ret;
        }

        public static object min(object x, object y) {
            return PythonSites.LessThanRetBool(x, y) ? x : y;
        }

        public static object min(params object[] args) {
            if (args.Length > 0) {
                object ret = args[0];
                if (args.Length == 1) return min(ret);
                for (int i = 1; i < args.Length; i++) {
                    if (PythonSites.LessThanRetBool(args[i], ret)) ret = args[i];
                }
                return ret;
            } else {
                throw PythonOps.TypeError("min expecting 1 arguments, got 0");
            }
        }

        public static object min(object x, [ParamDictionary] IAttributesCollection dict) {
            IEnumerator i = PythonOps.GetEnumerator(x);
            if (!i.MoveNext())
                throw PythonOps.ValueError(" min() arg is an empty sequence");
            object method = GetMinKwArg(dict);
            object ret = i.Current;
            object retValue = PythonCalls.Call(method, i.Current);
            while (i.MoveNext()) {
                object tmpRetValue = PythonCalls.Call(method, i.Current);
                if (PythonSites.LessThanRetBool(tmpRetValue, retValue)) {
                    ret = i.Current;
                    retValue = tmpRetValue;
                }
            }
            return ret;
        }

        [PythonVersion(2, 5)]
        public static object min(object x, object y, [ParamDictionary] IAttributesCollection dict) {
            object method = GetMinKwArg(dict);
            return PythonSites.LessThanRetBool(PythonCalls.Call(method, x), PythonCalls.Call(method, y)) ? x : y;
        }

        public static object min([ParamDictionary] IAttributesCollection dict, params object[] args) {
            if (args.Length > 0) {
                int retIndex = 0;
                if (args.Length == 1) return min(args[retIndex], dict);
                object method = GetMinKwArg(dict);
                object retValue = PythonCalls.Call(method, args[retIndex]);
                for (int i = 1; i < args.Length; i++) {
                    object tmpRetValue = PythonCalls.Call(method, args[i]);
                    if (PythonSites.LessThanRetBool(tmpRetValue, retValue)) {
                        retIndex = i;
                        retValue = tmpRetValue;
                    }
                }
                return args[retIndex];
            } else {
                throw PythonOps.TypeError("min expecting 1 arguments, got 0");
            }
        }

        private static object GetMinKwArg([ParamDictionary] IAttributesCollection dict) {
            if (dict.Count != 1)
                throw PythonOps.TypeError(" min() should have only 1 keyword argument, but got {0} keyword arguments", dict.Count);

            return VerifyKeys("min", dict);
        }

        private static object VerifyKeys(string name, IAttributesCollection dict) {
            object value;
            if (!dict.TryGetValue(SymbolTable.StringToId("key"), out value)) {
                ICollection<object> keys = dict.Keys;
                IEnumerator<object> en = keys.GetEnumerator();
                if (en.MoveNext()) {
                    throw PythonOps.TypeError(" {1}() got an unexpected keyword argument ({0})", en.Current, name);
                }
            }
            return value;
        }

        public static readonly PythonType @object = DynamicHelpers.GetPythonTypeFromType(typeof(object));

        public static object oct(object o) {
            return PythonOps.Oct(o);
        }

#if !SILVERLIGHT // files
        public static readonly PythonType open = DynamicHelpers.GetPythonTypeFromType(typeof(PythonFile));
#endif

        public static int ord(object value) {
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
                        throw PythonOps.TypeError("expected a character, but string of length {0} found", stringValue.Length);
                    }
                    ch = stringValue[0];
                } else {
                    throw PythonOps.TypeError("expected a character, but {0} found", DynamicHelpers.GetPythonType(value));
                }
            }
            return (int)ch;
        }

        public static object pow(object x, object y) {
            return PythonSites.Power(x, y);
        }

        public static object pow(object x, object y, object z) {
            try {
                return PythonOps.PowerMod(x, y, z);
            } catch (DivideByZeroException) {
                throw PythonOps.ValueError("3rd adgument cannot be 0");
            }
        }

        public static readonly PythonType property = DynamicHelpers.GetPythonTypeFromType(typeof(PythonProperty));
        
        public static List range(int stop) {
            return rangeWorker(stop);
        }

        public static List range(BigInteger stop) {
            return rangeWorker(stop);
        }

        private static List rangeWorker(int stop) {
            if (stop < 0) {
                stop = 0;
            }

            List ret = List.MakeEmptyList(stop);
            for (int i = 0; i < stop; i++) ret.AddNoLock(RuntimeHelpers.Int32ToObject(i));
            return ret;
        }

        private static List rangeWorker(BigInteger stop) {
            if (stop < BigInteger.Zero) {
                return range(0);
            }
            int istop;
            if (stop.AsInt32(out istop)) {
                return range(istop);
            }
            throw PythonOps.OverflowError("too many items in the range");
        }

        public static List range(int start, int stop) {
            return rangeWorker(start, stop);
        }
        
        public static List range(BigInteger start, BigInteger stop) {
            return rangeWorker(start, stop);
        }

        private static List rangeWorker(int start, int stop) {
            if (start > stop) {
                stop = start;
            }

            long length = (long)stop - (long)start;
            if (Int32.MinValue <= length && length <= Int32.MaxValue) {
                List ret = List.MakeEmptyList(stop - start);
                for (int i = start; i < stop; i++) ret.AddNoLock(RuntimeHelpers.Int32ToObject(i));
                return ret;
            }
            throw PythonOps.OverflowError("too many items in the list");
        }

        private static List rangeWorker(BigInteger start, BigInteger stop) {
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
            throw PythonOps.OverflowError("too many items in the range");
        }

        public static List range(int start, int stop, int step) {
            return rangeWorker(start, stop, step);
        }

        public static List range(BigInteger start, BigInteger stop, BigInteger step) {
            return rangeWorker(start, stop, step);
        }

        private static List rangeWorker(int start, int stop, int step) {
            if (step == 0) {
                throw PythonOps.ValueError("step of 0");
            }

            List ret;
            if (step > 0) {
                if (start > stop) stop = start;
                ret = List.MakeEmptyList((stop - start) / step);
                for (int i = start; i < stop; i += step) {
                    ret.AddNoLock(RuntimeHelpers.Int32ToObject(i));
                }
            } else {
                if (start < stop) stop = start;
                ret = List.MakeEmptyList((stop - start) / step);
                for (int i = start; i > stop; i += step) {
                    ret.AddNoLock(RuntimeHelpers.Int32ToObject(i));
                }
            }
            return ret;
        }

        private static List rangeWorker(BigInteger start, BigInteger stop, BigInteger step) {
            if (step == BigInteger.Zero) {
                throw PythonOps.ValueError("step of 0");
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
            throw PythonOps.OverflowError("too many items for list");
        }

        /// <summary>
        /// float overload of range - reports TypeError if the float is outside the range of normal integers.
        /// 
        /// The method binder would usally report an OverflowError in this case.
        /// </summary>
        public static List range(double stop) {
            return range(GetRangeAsInt(stop, "stop"));
        }

        /// <summary>
        /// float overload of range - reports TypeError if the float is outside the range of normal integers.
        /// 
        /// The method binder would usally report an OverflowError in this case.
        /// </summary>
        public static List range(double start, double stop, [DefaultParameterValue(1.0)]double step) {
            return range(GetRangeAsInt(start, "start"), GetRangeAsInt(stop, "stop"), GetRangeAsInt(step, "step"));
        }

        private static int GetRangeAsInt(double index, string name) {
            if (index < Int32.MaxValue || index > Int32.MaxValue) {
                throw PythonOps.TypeError("expected integer for " + name + " argument, got float");
            }
            return (int)index;
        }

        public static string raw_input(CodeContext/*!*/ context) {
            return raw_input(context, null);
        }

        public static string raw_input(CodeContext/*!*/ context, object prompt) {
            if (prompt != null) {
                PythonOps.PrintNoNewline(context, prompt);
            }
            string line = PythonOps.ReadLineFromSrc(PythonContext.GetContext(context).SystemStandardIn) as string;
            if (line != null && line.EndsWith("\n")) return line.Substring(0, line.Length - 1);
            return line;
        }

        public static object reduce(object func, object seq) {
            IEnumerator i = PythonOps.GetEnumerator(seq);
            if (!i.MoveNext()) {
                throw PythonOps.TypeError("reduce() of empty sequence with no initial value");
            }
            object ret = i.Current;
            while (i.MoveNext()) {
                ret = PythonCalls.Call(func, ret, i.Current);
            }
            return ret;
        }

        public static object reduce(object func, object seq, object initializer) {
            IEnumerator i = PythonOps.GetEnumerator(seq);
            object ret = initializer;
            while (i.MoveNext()) {
                ret = PythonCalls.Call(func, ret, i.Current);
            }
            return ret;
        }

        public static object reload(CodeContext/*!*/ context, Scope/*!*/ scope) {
            if (scope == null) {
                throw PythonOps.TypeError("unexpected type: NoneType");
            }
            Importer.ReloadModule(context, scope);
            return scope;
        }

        public static object repr(object o) {
            object res = PythonOps.StringRepr(o);

            if (!(res is String) && !(res is ExtensibleString)) {
                throw PythonOps.TypeError("__repr__ returned non-string (type {0})", PythonOps.GetPythonTypeName(o));
            }

            return res;
        }

        public static object reversed(object o) {
            object reversed;
            if (PythonOps.TryGetBoundAttr(o, Symbols.Reversed, out reversed)) {
                return PythonCalls.Call(reversed);
            }

            object getitem;
            object len;

            if (!PythonOps.TryGetBoundAttr(o, Symbols.GetItem, out getitem) ||
                !PythonOps.TryGetBoundAttr(o, Symbols.Length, out len) ||
                o is PythonDictionary) {
                throw PythonOps.TypeError("argument to reversed() must be a sequence");
            }

            object length = PythonCalls.Call(len);
            if (!(length is int)) {
                throw PythonOps.ValueError("__len__ must return int");
            }
            return new ReversedEnumerator((int)length, getitem);
        }

        public static double round(double number) {
            return MathUtils.RoundAwayFromZero(number);
        }

        public static double round(double number, int ndigits) {
            return MathUtils.RoundAwayFromZero(number, ndigits);
        }

        public static void setattr(CodeContext/*!*/ context, object o, string name, object val) {
            PythonOps.SetAttr(context, o, SymbolTable.StringToId(name), val);
        }

        public static readonly PythonType slice = DynamicHelpers.GetPythonTypeFromType(typeof(Slice));

        public static List sorted(object iterable) {
            return sorted(iterable, null, null, false);
        }

        public static List sorted(object iterable, object cmp) {
            return sorted(iterable, cmp, null, false);
        }

        public static List sorted(object iterable, object cmp, object key) {
            return sorted(iterable, cmp, key, false);
        }

        public static List sorted([DefaultParameterValueAttribute(null)] object iterable,
            [DefaultParameterValueAttribute(null)]object cmp,
            [DefaultParameterValueAttribute(null)]object key,
            [DefaultParameterValueAttribute(false)]bool reverse) {

            IEnumerator iter = PythonOps.GetEnumerator(iterable);
            List l = List.MakeEmptyList(10);
            while (iter.MoveNext()) {
                l.AddNoLock(iter.Current);
            }
            l.Sort(cmp, key, reverse);
            return l;
        }

        public static readonly PythonType staticmethod = DynamicHelpers.GetPythonTypeFromType(typeof(staticmethod));

        public static object sum(object sequence) {
            return sum(sequence, 0);
        }

        private static readonly FastDynamicSite<object, object, object> _addForSum =
            FastDynamicSite<object, object, object>.Create(DefaultContext.DefaultCLS, DoOperationAction.Make(Operators.Add));

        public static object sum(object sequence, object start) {
            IEnumerator i = PythonOps.GetEnumerator(sequence);

            if (start is string) {
                throw PythonOps.TypeError("Cannot sum strings, use '{0}'.join(seq)", start);
            }

            object ret = start;
            while (i.MoveNext()) {
                ret = _addForSum.Invoke(ret, i.Current); // Ops.Add(ret, i.Current);
            }
            return ret;
        }

        public static readonly PythonType super = DynamicHelpers.GetPythonTypeFromType(typeof(Super));

        public static readonly PythonType str = DynamicHelpers.GetPythonTypeFromType(typeof(string));

        public static readonly PythonType tuple = DynamicHelpers.GetPythonTypeFromType(typeof(PythonTuple));

        public static readonly PythonType type = DynamicHelpers.GetPythonTypeFromType(typeof(PythonType));

        public static string unichr(int i) {
            if (i < Char.MinValue || i > Char.MaxValue) {
                throw PythonOps.ValueError("{0} is not in required range", i);
            }
            return RuntimeHelpers.CharToString((char)i);
        }

        public static readonly PythonType unicode = DynamicHelpers.GetPythonTypeFromType(typeof(string));

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]
        [Documentation("vars([object]) -> dictionary\n\nWithout arguments, equivalent to locals().\nWith an argument, equivalent to object.__dict__.")]
        public static object vars(CodeContext/*!*/ context) {
            return locals(context);
        }

        public static object vars(CodeContext/*!*/ context, object @object) {
            object value;
            if (!PythonOps.TryGetBoundAttr(@object, Symbols.Dict, out value)) {
                throw PythonOps.TypeError("vars() argument must have __dict__ attribute");
            }
            return value;
        }

        public static readonly PythonType xrange = DynamicHelpers.GetPythonTypeFromType(typeof(XRange)); //PyXRange.pytype;

        public static List zip(object s0, object s1) {
            IEnumerator i0 = PythonOps.GetEnumerator(s0);
            IEnumerator i1 = PythonOps.GetEnumerator(s1);
            List ret = new List();
            while (i0.MoveNext() && i1.MoveNext()) {
                ret.AddNoLock(PythonTuple.MakeTuple(i0.Current, i1.Current));
            }
            return ret;
        }

        //??? should we fastpath the 1,2,3 item cases???
        public static List zip(params object[] seqs) {
            if (seqs == null) throw PythonOps.TypeError("zip argument must support iteration, got {0}", NoneTypeOps.TypeInstance);

            int N = seqs.Length;
            if (N == 2) return zip(seqs[0], seqs[1]);
            if (N == 0) return List.Make();
            IEnumerator[] iters = new IEnumerator[N];
            for (int i = 0; i < N; i++) iters[i] = PythonOps.GetEnumerator(seqs[i]);

            List ret = new List();
            while (true) {
                object[] items = new object[N];
                for (int i = 0; i < N; i++) {
                    // first iterator which is no longer iterable ends the 
                    // loop.
                    if (!iters[i].MoveNext()) return ret;
                    items[i] = iters[i].Current;
                }
                ret.AddNoLock(PythonTuple.Make(items));
            }
        }

        public static readonly PythonType BaseException = DynamicHelpers.GetPythonTypeFromType(typeof(PythonExceptions.BaseException));

        /// <summary>
        /// Gets the appropriate LanguageContext to be used for code compiled with Python's compile built-in
        /// </summary>
        internal static CompilerOptions GetDefaultCompilerOptions(CodeContext/*!*/ context, bool inheritContext, CompileFlags cflags) {
            PythonCompilerOptions pco;
            if (inheritContext) {
                if (((PythonModule)context.ModuleContext).TrueDivision) {
                    pco = new PythonCompilerOptions(true);
                } else {
                    pco = new PythonCompilerOptions(false);
                }                
            } else if (((cflags & CompileFlags.CO_FUTURE_DIVISION) != 0)) {
                pco = new PythonCompilerOptions(true);
            } else {
                pco = DefaultContext.DefaultPythonContext.CreateDefaultCompilerOptions();
            }

            // The options created this way never creates
            // optimized module (exec, compile)
            pco.Module &= ~ModuleOptions.Optimized;
            return pco;
        }

        /// <summary> Returns true if we should inherit our callers context (true division, etc...), false otherwise </summary>
        private static bool GetCompilerInheritance(object dontInherit) {
            return dontInherit == null || Converter.ConvertToInt32(dontInherit) == 0;
        }

        /// <summary> Returns the default compiler flags or the flags the user specified. </summary>
        private static CompileFlags GetCompilerFlags(object flags) {
            CompileFlags cflags = 0;
            if (flags != null) {
                cflags = (CompileFlags)Converter.ConvertToInt32(flags);
                if ((cflags & ~(CompileFlags.CO_NESTED | CompileFlags.CO_GENERATOR_ALLOWED | CompileFlags.CO_FUTURE_DIVISION | CompileFlags.CO_DONT_IMPLY_DEDENT)) != 0) {
                    throw PythonOps.ValueError("unrecognized flags");
                }
            }

            return cflags;
        }

        /// <summary>
        /// Gets a scope used for executing new code in optionally replacing the globals and locals dictionaries.
        /// </summary>
        private static Scope GetExecEvalScope(CodeContext/*!*/ context, IAttributesCollection globals, object localsDict) {
            if (globals == null) globals = Builtin.globals(context);
            if (localsDict == null) localsDict = locals(context);

            PythonContext python = PythonContext.GetContext(context);

            Scope scope = new Scope(python, new Scope(python, globals), GetAttrLocals(context, localsDict));

            if (!globals.ContainsKey(Symbols.Builtins)) {
                globals[Symbols.Builtins] = python.BuiltinModuleInstance;
            }
            return scope;
        }

        public static void PerformModuleReload(PythonContext context, IAttributesCollection dict) {
            dict[SymbolTable.StringToId("__debug__")] = RuntimeHelpers.BooleanToObject(context.DomainManager.GlobalOptions.DebugMode);
        }
    }
}
