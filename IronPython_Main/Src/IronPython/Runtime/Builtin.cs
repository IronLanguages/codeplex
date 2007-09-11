/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Diagnostics;

using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Types;
using Microsoft.Scripting.Utils;

using IronPython.Runtime;
using IronPython.Runtime.Types;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Calls;
using IronPython.Compiler;
using IronPython.Compiler.Generation;
using IronPython.Runtime.Operations;
using IronPython.Hosting;


[assembly: PythonModule("__builtin__", typeof(Builtin))]
namespace IronPython.Runtime {
    [PythonType("__builtin__"), Documentation("")]  // Documentation suppresses XML Doc on startup.
    public static partial class Builtin {
        
        public static object True = RuntimeHelpers.True;
        public static object False = RuntimeHelpers.False;

        // This will always stay null
        public static readonly object None;

        public static object Ellipsis = PythonOps.Ellipsis;
        public static object NotImplemented = PythonOps.NotImplemented;

        public static object exit = "Use Ctrl-Z plus Return to exit";
        public static object quit = "Use Ctrl-Z plus Return to exit";

        public static bool __debug__ {
            get {
                return ScriptDomainManager.Options.DebugMode;
            }
        }

        [Documentation("__import__(name) -> module\n\nImport a module.")]
        public static object __import__(CodeContext context, string name) {
            return __import__(context, name, null, null, null);
        }

        [Documentation("__import__(name, globals) -> module\n\nImport a module.")]
        public static object __import__(CodeContext context, string name, object globals) {
            return __import__(context, name, globals, null, null);

        }

        [Documentation("__import__(name, globals, locals) -> module\n\nImport a module.")]
        public static object __import__(CodeContext context, string name, object globals, object locals) {
            return __import__(context, name, globals, locals, null);
        }

        [Documentation("__import__(name, globals, locals, fromlist) -> module\n\nImport a module.")]
        public static object __import__(CodeContext context, string name, object globals, object locals, object fromList) {
            //!!! remove suppress in GlobalSuppressions.cs when CodePlex 2704 is fixed.
            List from = fromList as List;

            object ret = PythonEngine.CurrentEngine.Importer.ImportModule(context, name, from != null && from.GetLength() > 0);
            if (ret == null) {
                throw PythonOps.ImportError("No module named {0}", name);
            }

            ScriptModule mod = ret as ScriptModule;
            if (mod != null && from != null) {
                string strAttrName;
                object attrValue;
                foreach (object attrName in from) {
                    if (Converter.TryConvertToString(attrName, out strAttrName) && strAttrName != null && strAttrName != "*") {
                        try {
                            attrValue = PythonEngine.CurrentEngine.Importer.ImportFrom(context, mod, strAttrName);
                        } catch (PythonImportErrorException) {
                            continue;
                        }
                    }
                }
            }

            return ret;
        }

        [Documentation("abs(number) -> number\n\nReturn the absolute value of the argument.")]
        public static object abs(CodeContext context, object o) {
            if (o is int) return Int32Ops.Abs((int)o);
            if (o is long) return Int64Ops.Abs((long)o);
            if (o is double) return DoubleOps.Abs((double)o);
            if (o is bool) return (((bool)o) ? 1 : 0);
            if (o is string) throw PythonOps.TypeError("bad operand type for abs()");
            BigInteger bi = o as BigInteger;
            if (!Object.ReferenceEquals(bi, null)) return BigIntegerOps.Abs(bi);
            if (o is Complex64) return ComplexOps.Abs((Complex64)o);

            object ret;
            if (PythonOps.TryInvokeOperator(context,
                Operators.AbsoluteValue,
                o,
                out ret)) {
                return ret;
            } else {
                throw PythonOps.TypeError("bad operand type for abs()");
            }
        }

        [PythonVersion(2, 5)]
        public static bool all(object x) {
            IEnumerator i = PythonOps.GetEnumerator(x);
            while (i.MoveNext()) {
                if (!PythonOps.IsTrue(i.Current)) return false;
            }
            return true;
        }

        [PythonVersion(2, 5)]
        public static bool any(object x) {
            IEnumerator i = PythonOps.GetEnumerator(x);
            while (i.MoveNext()) {
                if (PythonOps.IsTrue(i.Current)) return true;
            }
            return false;
        }

        [Documentation("apply(object[, args[, kwargs]]) -> value\n\nDeprecated.\nInstead, use:\n    function(*args, **keywords).")]
        public static object apply(CodeContext context, object func) {
            return PythonOps.CallWithContext(context, func);
        }

        public static object apply(CodeContext context, object func, object args) {
            return PythonOps.CallWithArgsTupleAndContext(context, func, ArrayUtils.EmptyObjects, args);
        }

        public static object apply(CodeContext context, object func, object args, object kws) {
            return PythonOps.CallWithArgsTupleAndKeywordDictAndContext(context, func, ArrayUtils.EmptyObjects, ArrayUtils.EmptyStrings, args, kws);
        }

        public static object basestring = DynamicHelpers.GetDynamicTypeFromType(typeof(string));

        public static object @bool = DynamicHelpers.GetDynamicTypeFromType(typeof(bool));


        public static object buffer = DynamicHelpers.GetDynamicTypeFromType(typeof(PythonBuffer));

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

        private static object TryCoerce(CodeContext context, object x, object y) {
            object res;
            if (DynamicHelpers.GetDynamicType(x).TryInvokeBinaryOperator(context,
                Operators.Coerce,
                x,
                y,
                out res) && res != PythonOps.NotImplemented) {
                return res;
            }
            return null;
        }

        [Documentation("coerce(x, y) -> (x1, y1)\n\nReturn a tuple consisting of the two numeric arguments converted to\na common type. If coercion is not possible, raise TypeError.")]
        public static object coerce(CodeContext context, object x, object y) {
            object converted;

            if (x == null && y == null) {
                return Tuple.MakeTuple(null, null);
            }

            if (x != null) {
                if (Converter.TryConvert(y, x.GetType(), out converted)) {
                    return Tuple.MakeTuple(x, converted);
                }
            }
            if (y != null) {
                if (Converter.TryConvert(x, y.GetType(), out converted)) {
                    return Tuple.MakeTuple(converted, y);
                }
            }

            converted = TryCoerce(context, x, y);
            if (converted != null) {
                return converted;
            }
            converted = TryCoerce(context, y, x);
            if (converted != null) {
                return Tuple.Make(reversed(converted));
            }

            throw PythonOps.TypeError("coercion failed");
        }

        [Documentation("compile a unit of source code.\n\nThe source can be compiled either as exec, eval, or single.\nexec compiles the code as if it were a file\neval compiles the code as if were an expression\nsingle compiles a single statement\n\n")]
        public static object compile(CodeContext context, string source, string filename, string kind, object flags, object dontInherit) {
            bool inheritContext = GetCompilerInheritance(dontInherit);
            CompileFlags cflags = GetCompilerFlags(flags);
            PythonContext lc = GetCompilerLanguageContext(context, inheritContext, cflags);
            ScriptEngine engine = context.LanguageContext.Engine;
            SourceUnit sourceUnit;
            
            switch (kind) {
                case "exec": sourceUnit = SourceUnit.CreateSnippet(engine, source, filename, SourceCodeKind.Default); break;
                case "eval": sourceUnit = SourceUnit.CreateSnippet(engine, source, filename, SourceCodeKind.Expression); break;
                case "single": sourceUnit = SourceUnit.CreateSnippet(engine, source, filename, SourceCodeKind.SingleStatement); break;
                default: 
                    throw PythonOps.ValueError("compile() arg 3 must be 'exec' or 'eval' or 'single'");
            }

            ScriptCode compiledCode = PythonModuleOps.CompileFlowTrueDivision(sourceUnit, lc);
            compiledCode.EnsureCompiled();

            return new FunctionCode(compiledCode, cflags);
        }

        [Documentation("compile a unit of source code.\n\nThe source can be compiled either as exec, eval, or single.\nexec compiles the code as if it were a file\neval compiles the code as if were an expression\nsingle compiles a single statement\n\n")]
        public static object compile(CodeContext context, string source, string filename, string kind, object flags) {
            return compile(context, source, filename, kind, flags, null);
        }

        [Documentation("compile a unit of source code.\n\nThe source can be compiled either as exec, eval, or single.\nexec compiles the code as if it were a file\neval compiles the code as if were an expression\nsingle compiles a single statement\n\n")]
        public static object compile(CodeContext context, string source, string filename, string kind) {
            return compile(context, source, filename, kind, null, null);
        }

        public static object classmethod = DynamicHelpers.GetDynamicTypeFromType(typeof(classmethod));

        public static int cmp(CodeContext context, object x, object y) {
            return PythonOps.Compare(context, x, y);
        }

        public static object complex = DynamicHelpers.GetDynamicTypeFromType(typeof(Complex64));

        public static void delattr(CodeContext context, object o, string name) {
            context.LanguageContext.DeleteMember(context, o, SymbolTable.StringToId(name));
        }

        public static object dict = DynamicHelpers.GetDynamicTypeFromType(typeof(PythonDictionary));

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]
        public static List dir(CodeContext context) {
            List res = List.Make(locals(context).Keys);

            res.Sort();
            return res;
        }

        public static List dir(CodeContext context, object o) {
            IList<object> ret = PythonOps.GetAttrNames(context, o);
            List lret = new List(ret);
            lret.Sort();
            return lret;
        }

        // Python has lots of optimizations for this method that we may want to implement in the future
        // In particular, this should be treated like the other binary operators
        public static object divmod(CodeContext context, object x, object y) {
            Debug.Assert(PythonOps.NotImplemented != null);
            object ret;
            if (DynamicHelpers.GetDynamicType(x).TryInvokeBinaryOperator(context, Operators.DivMod, x, y, out ret)
                && ret != PythonOps.NotImplemented) return ret;

            if (DynamicHelpers.GetDynamicType(y).TryInvokeBinaryOperator(context, Operators.ReverseDivMod, y, x, out ret)
                && ret != PythonOps.NotImplemented) return ret;

            return SlowDivMod(x, y);
        }

        internal static object SlowDivMod(object x, object y) {
            return Tuple.MakeTuple(PythonSites.FloorDivide(x, y), PythonSites.Mod(x, y));
        }

        public static object enumerate = DynamicHelpers.GetDynamicTypeFromType(typeof(Enumerate));

        public static object eval(CodeContext context, FunctionCode code) {
            Debug.Assert(context != null);
            if (code == null) throw PythonOps.TypeError("eval() argument 1 must be string or code object");

            return eval(context, code, null);
        }

        public static object eval(CodeContext context, FunctionCode code, IAttributesCollection globals) {
            Debug.Assert(context != null);
            if (code == null) throw PythonOps.TypeError("eval() argument 1 must be string or code object");

            return eval(context, code, globals, globals);
        }

        public static object eval(CodeContext context, FunctionCode code, IAttributesCollection globals, object locals) {
            Debug.Assert(context != null);
            if (code == null) throw PythonOps.TypeError("eval() argument 1 must be string or code object");

            Microsoft.Scripting.Scope scope = GetExecEvalScope(context, globals, locals);

            return code.Call(context, scope, false); // Do not try evaluate mode for compiled code

        }

        public static IAttributesCollection GetAttrLocals(CodeContext context, object locals) {
            IAttributesCollection attrLocals;
            if (locals == null) {
                attrLocals = Builtin.locals(context);
            } else {
                attrLocals = locals as IAttributesCollection ?? new ObjectAttributesAdapter(locals);
            }
            return attrLocals;
        }

        public static object eval(CodeContext context, string expression) {
            Debug.Assert(context != null);
            if (expression == null) throw PythonOps.TypeError("eval() argument 1 must be string or code object");

            return eval(context, expression, globals(context), locals(context));
        }

        public static object eval(CodeContext context, string expression, IAttributesCollection globals) {
            Debug.Assert(context != null);
            if (expression == null) throw PythonOps.TypeError("eval() argument 1 must be string or code object");

            return eval(context, expression, globals, globals);
        }

        public static object eval(CodeContext context, string expression, IAttributesCollection globals, object locals) {
            Debug.Assert(context != null);
            if (expression == null) throw PythonOps.TypeError("eval() argument 1 must be string or code object");

            if (locals != null && PythonOps.IsMappingType(context, locals) == RuntimeHelpers.False) {
                throw PythonOps.TypeError("locals must be mapping");
            }

            Microsoft.Scripting.Scope scope = GetExecEvalScope(context, globals, locals);

            // TODO: remove TrimStart
            SourceUnit expr_code = SourceUnit.CreateSnippet(context.LanguageContext.Engine, expression.TrimStart(' ', '\t'), SourceCodeKind.Expression);

            return PythonModuleOps.CompileFlowTrueDivision(expr_code, context.LanguageContext).Run(scope, context.ModuleContext, true);
        }


#if !SILVERLIGHT // files

        public static void execfile(CodeContext context, object filename) {
            execfile(context, filename, null, null);
        }

        public static void execfile(CodeContext context, object filename, object globals) {
            execfile(context, filename, globals, null);
        }

        public static void execfile(CodeContext context, object filename, object globals, object locals) {
            IAttributesCollection g = globals as IAttributesCollection;
            if (g == null && globals != null) {
                throw PythonOps.TypeError("execfile: arg 2 must be dictionary");
            }

            IAttributesCollection l = locals as IAttributesCollection;
            if (l == null && locals != null) {
                throw PythonOps.TypeError("execfile: arg 3 must be dictionary");
            }

            if (l == null) l = g;

            PythonEngine engine = PythonEngine.CurrentEngine;

            Scope execScope = GetExecEvalScope(context, g, l);
            string path = Converter.ConvertToString(filename);
            SourceUnit sourceUnit = ScriptDomainManager.CurrentManager.Host.TryGetSourceFileUnit(engine, path, engine.SystemState.DefaultEncoding);

            if (sourceUnit == null) {
                throw PythonOps.IOError("execfile: specified file doesn't exist");
            }

            ScriptCode code;

            try {
                code = PythonModuleOps.CompileFlowTrueDivision(sourceUnit, context.LanguageContext);
            } catch (UnauthorizedAccessException x) {
                throw PythonOps.IOError(x);
            }

            code.Run(execScope, context.ModuleContext, false); // Do not attempt evaluation mode for execfile
        }

        public static object file = DynamicHelpers.GetDynamicTypeFromType(typeof(PythonFile));
#endif

        public static object filter(object function, object list) {
            string str = list as string;
            if (str != null) {
                if (function == null) return str;
                StringBuilder sb = new StringBuilder();
                foreach (char c in str) {
                    if (PythonOps.IsTrue(PythonCalls.Call(function, RuntimeHelpers.CharToString(c)))) sb.Append(c);
                }
                return sb.ToString();
            } else if (issubclass(DynamicHelpers.GetDynamicType(list), TypeCache.String)) {
                StringBuilder sb = new StringBuilder();
                IEnumerator e = PythonOps.GetEnumerator(list);
                while (e.MoveNext()) {
                    object o = e.Current;
                    object t = (function != null) ? PythonCalls.Call(function, o) : o;

                    if (!PythonOps.IsTrue(t))
                        continue;

                    sb.Append(Converter.ConvertToString(o));
                }
                return sb.ToString();
            }

            List ret = new List();

            IEnumerator i = PythonOps.GetEnumerator(list);
            while (i.MoveNext()) {
                if (function == null) {
                    if (PythonOps.IsTrue(i.Current)) ret.AddNoLock(i.Current);
                } else {
                    if (PythonOps.IsTrue(PythonCalls.Call(function, i.Current))) ret.AddNoLock(i.Current);
                }
            }

            if (isinstance(list, DynamicHelpers.GetDynamicTypeFromType(typeof(Tuple)))) {
                return Tuple.Make(ret);
            } else {
                return ret;
            }
        }

        public static object @float = DynamicHelpers.GetDynamicTypeFromType(typeof(double));

        public static object getattr(CodeContext context, object o, string name) {
            return PythonOps.GetBoundAttr(context, o, SymbolTable.StringToId(name));
        }

        public static object getattr(CodeContext context, object o, string name, object def) {
            object ret;
            if (PythonOps.TryGetBoundAttr(context, o, SymbolTable.StringToId(name), out ret)) return ret;
            else return def;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]
        public static IAttributesCollection globals(CodeContext context) {
            return new GlobalsDictionary(context.Scope);
        }

        public static bool hasattr(CodeContext context, object o, string name) {
            return PythonOps.HasAttr(context, o, SymbolTable.StringToId(name));
        }

        public static int hash(object o) {
            return PythonOps.Hash(o);
        }

        public static void help(CodeContext context, object o) {
            StringBuilder doc = new StringBuilder();
            List<object> doced = new List<object>();  // document things only once

            help(context, doced, doc, 0, o);

            if (doc.Length == 0) {
                if (!(o is string)) {
                    help(context, DynamicHelpers.GetDynamicType(o));
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
                PythonOps.Print(strings[i]);
            }
        }

        private static void help(CodeContext context, List<object> doced, StringBuilder doc, int indent, object o) {
            DynamicType dt;
            BuiltinFunction bf;
            PythonFunction pf;
            BuiltinMethodDescriptor methodDesc;
            string strVal;
            ScriptModule sm;
            OldClass oc;

            if (doced.Contains(o)) return;  // document things only once
            doced.Add(o);

            if ((strVal = o as string) != null) {
                if (indent != 0) return;

                // try and find things that string could refer to,
                // then call help on them.
                foreach (KeyValuePair<object, object> kvp in SystemState.Instance.modules) {
                    object module = kvp.Value;

                    IList<object> attrs = PythonOps.GetAttrNames(context, module);
                    DynamicType modType = DynamicHelpers.GetDynamicType(module);
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

                    if (dt != null) help(context, doced, doc, indent, dt);
                    else if (bf != null) help(context, doced, doc, indent, bf);
                    else if (pf != null) help(context, doced, doc, indent, pf);
                }
            } else if ((dt = o as DynamicType) != null) {
                // find all the functions, and display their 
                // documentation                
                if (indent == 0) doc.AppendFormat("Help on {0} in module {1}\n\n", dt.Name, PythonOps.GetBoundAttr(context, dt, Symbols.Module));
                DynamicTypeSlot dts;
                if (dt.TryResolveSlot(context, Symbols.Doc, out dts)) {
                    object docText;
                    if (dts.TryGetValue(context, null, dt, out docText) && docText != null)
                        AppendMultiLine(doc, docText.ToString() + Environment.NewLine, indent);
                    AppendIndent(doc, indent);
                    doc.AppendLine("Data and other attributes defined here:");
                    AppendIndent(doc, indent);
                    doc.AppendLine();
                }

                List<SymbolId> names = new List<SymbolId>(dt.GetMemberNames(context, null));
                names.Sort(delegate(SymbolId left, SymbolId right) {
                    return String.Compare(left.ToString(), right.ToString());
                });

                foreach (SymbolId name in names) {
                    if (name == Symbols.Class) continue;

                    DynamicTypeSlot value;
                    object val;
                    if (dt.TryLookupSlot(context, name, out value) && value.TryGetValue(context, null, dt, out val)) {
                        help(context, doced, doc, indent + 1, val);
                    }
                }
            } else if ((methodDesc = o as BuiltinMethodDescriptor) != null) {
                if (indent == 0) doc.AppendFormat("Help on method-descriptor {0}\n\n", methodDesc.Name);
                AppendIndent(doc, indent);
                doc.Append(methodDesc.Name);
                doc.Append("(...)\n");

                AppendMultiLine(doc, BuiltinMethodDescriptorOps.GetDocumentation(methodDesc), indent + 1);
            } else if ((bf = o as BuiltinFunction) != null) {
                if (indent == 0) doc.AppendFormat("Help on built-in function {0}\n\n", bf.Name);
                AppendIndent(doc, indent);
                doc.Append(bf.Name);
                doc.Append("(...)\n");

                AppendMultiLine(doc, PythonBuiltinFunctionOps.GetDocumentation(bf), indent + 1);
            } else if ((pf = o as PythonFunction) != null) {
                if (indent == 0) doc.AppendFormat("Help on function {0} in module {1}\n\n", pf.Name, pf.Module);

                AppendIndent(doc, indent);
                doc.AppendFormat("{0}({1})\n", pf.Name, String.Join(", ", pf.ArgNames));
                string pfDoc = Converter.ConvertToString(pf.Documentation);
                if (!String.IsNullOrEmpty(pfDoc)) {
                    AppendMultiLine(doc, pfDoc, indent);
                }
            } else if ((sm = o as ScriptModule) != null) {
                foreach (SymbolId si in sm.Scope.Keys) {
                    if (si == Symbols.Class || si == Symbols.Builtins) continue;

                    object value;
                    if (sm.Scope.TryGetName(context.LanguageContext, si, out value)) {
                        help(context, doced, doc, indent + 1, value);
                    }
                }
            } else if ((oc = o as OldClass) != null) {
                if (indent == 0) doc.AppendFormat("Help on {0} in module {1}\n\n",
                                    oc.Name, PythonOps.GetBoundAttr(context, oc, Symbols.Module));
                object docText;
                if (oc.TryLookupSlot(Symbols.Doc, out docText) && docText != null) {
                    AppendMultiLine(doc, docText.ToString() + Environment.NewLine, indent);
                    AppendIndent(doc, indent);
                    doc.AppendLine("Data and other attributes defined here:");
                    AppendIndent(doc, indent);
                    doc.AppendLine();
                }

                IList<object> names = oc.GetCustomMemberNames(context);
                List sortNames = new List(names);
                sortNames.Sort();
                names = sortNames;
                foreach (string name in names) {
                    if (name == "__class__") continue;

                    object value;

                    if (oc.TryLookupSlot(SymbolTable.StringToId(name), out value))
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

        public static object input(CodeContext context) {
            return input(context, null);
        }

        public static object input(CodeContext context, object prompt) {
            return eval(context, raw_input(context, prompt));
        }

        public static object @int = DynamicHelpers.GetDynamicTypeFromType(typeof(int));

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

        public static bool issubclass(DynamicType c, object typeinfo) {
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


        public static object set = DynamicHelpers.GetDynamicTypeFromType(typeof(SetCollection));
        public static object frozenset = DynamicHelpers.GetDynamicTypeFromType(typeof(FrozenSetCollection));
        public static object list = DynamicHelpers.GetDynamicTypeFromType(typeof(List));

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]
        public static IAttributesCollection locals(CodeContext context) {
            return new LocalsDictionary(context.Scope);
        }

        public static object @long = DynamicHelpers.GetDynamicTypeFromType(typeof(BigInteger));

        public static List map(params object[] param) {
            if (param == null || param.Length < 2) {
                throw PythonOps.TypeError("at least 2 arguments required to map");
            }
            List ret = new List();
            object func = param[0];
            
            if (param.Length == 2) {
                FastDynamicSite<object, object, object> mapSite = null;
                if(func != null) mapSite = FastDynamicSite<object, object, object>.Create(DefaultContext.Default, CallAction.Simple);
                
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
                        if (mapSite == null) mapSite = FastDynamicSite<object, object[], object>.Create(DefaultContext.Default, CallAction.Make(new ArgumentInfo(ArgumentKind.List)));
                        ret.AddNoLock(mapSite.Invoke(func, args));
                    } else {
                        ret.AddNoLock(Tuple.MakeTuple(args));
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

        [PythonVersion(2, 5)]
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

        [PythonVersion(2, 5)]
        [PythonName("max")]
        public static object max(object x, object y, [ParamDictionary] IAttributesCollection dict) {
            object method = GetMaxKwArg(dict);
            return PythonSites.GreaterThanRetBool(PythonCalls.Call(method, x), PythonCalls.Call(method, y)) ? x : y;
        }

        [PythonVersion(2, 5)]
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

        [PythonVersion(2, 5)]
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
        [PythonName("min")]
        public static object min(object x, object y, [ParamDictionary] IAttributesCollection dict) {
            object method = GetMinKwArg(dict);
            return PythonSites.LessThanRetBool(PythonCalls.Call(method, x), PythonCalls.Call(method, y)) ? x : y;
        }

        [PythonVersion(2, 5)]
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

        public static object @object = DynamicHelpers.GetDynamicTypeFromType(typeof(object));

        public static object oct(object o) {
            return PythonOps.Oct(o);
        }

#if !SILVERLIGHT // files
        public static object open = DynamicHelpers.GetDynamicTypeFromType(typeof(PythonFile));
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
                    throw PythonOps.TypeError("expected a character, but {0} found", DynamicHelpers.GetDynamicType(value));
                }
            }
            return (int)ch;
        }

        [PythonName("pow")]
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

        public static object property = DynamicHelpers.GetDynamicTypeFromType(typeof(PythonProperty));

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

        public static string raw_input(CodeContext context) {
            return raw_input(context, null);
        }

        public static string raw_input(CodeContext context, object prompt) {
            if (prompt != null) {
                PythonOps.PrintNoNewline(prompt);
            }
            string line = PythonOps.ReadLineFromSrc(SystemState.Instance.stdin) as string;
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

        public static object reload(CodeContext context, ScriptModule module) {
            ((PythonContext)DefaultContext.Default.LanguageContext).CheckReloadable(module);
            module.Reload();
            return module;
        }

        public static object reload(CodeContext context, SystemState state) {
            if (state == null) throw PythonOps.TypeError("unexpected type: NoneType");

            Debug.Assert(state == SystemState.Instance, "unexpected multiple instances of SystemState");
            state.Initialize();

            return state;
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

            // OldClass check: we currently are in a strange state where we partially support
            // descriptors on old-style classes, although we're not supposed to.  We special
            // case it here until that's fixed.
            if (o is OldClass ||
                !PythonOps.TryGetBoundAttr(o, Symbols.GetItem, out getitem) ||
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

        public static double round(double x) {
#if !SILVERLIGHT
            return Math.Round(x, MidpointRounding.AwayFromZero);
#else
            return Math.Round(x); // TODO: ToEven semantics
#endif
        }

        public static double round(double x, int n) {
            // values are rounded to 10 ^ n.  For negative values
            // we need to handle this ourselves.
            if (n < 0) {
                double factor = Math.Pow(10.0, -n);
#if !SILVERLIGHT
                return factor * Math.Round(x / factor, MidpointRounding.AwayFromZero);
#else
                return factor * Math.Round(x / factor);// TODO: ToEven semantics
#endif

            }
            return Math.Round(x, n);
        }

        public static void setattr(CodeContext context, object o, string name, object val) {
            PythonOps.SetAttr(context, o, SymbolTable.StringToId(name), val);
        }

        public static object slice = DynamicHelpers.GetDynamicTypeFromType(typeof(Slice));

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

        public static object staticmethod = DynamicHelpers.GetDynamicTypeFromType(typeof(staticmethod));

        public static object sum(object sequence) {
            return sum(sequence, 0);
        }

        private static FastDynamicSite<object, object, object> addForSum =
            FastDynamicSite<object, object, object>.Create(DefaultContext.DefaultCLS, DoOperationAction.Make(Operators.Add));

        public static object sum(object sequence, object start) {
            IEnumerator i = PythonOps.GetEnumerator(sequence);

            if (start is string) {
                throw PythonOps.TypeError("Cannot sum strings, use '{0}'.join(seq)", start);
            }

            object ret = start;
            while (i.MoveNext()) {
                ret = addForSum.Invoke(ret, i.Current); // Ops.Add(ret, i.Current);
            }
            return ret;
        }

        public static object super = DynamicHelpers.GetDynamicTypeFromType(typeof(Super));

        public static object str = DynamicHelpers.GetDynamicTypeFromType(typeof(string));

        public static object tuple = DynamicHelpers.GetDynamicTypeFromType(typeof(Tuple));

        public static object type = DynamicHelpers.GetDynamicTypeFromType(typeof(DynamicType));

        public static string unichr(int i) {
            if (i < Char.MinValue || i > Char.MaxValue) {
                throw PythonOps.ValueError("{0} is not in required range", i);
            }
            return RuntimeHelpers.CharToString((char)i);
        }

        public static object unicode = DynamicHelpers.GetDynamicTypeFromType(typeof(string));

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]
        [Documentation("vars([object]) -> dictionary\n\nWithout arguments, equivalent to locals().\nWith an argument, equivalent to object.__dict__.")]
        public static object vars(CodeContext context) {
            return locals(context);
        }

        public static object vars(CodeContext context, object @object) {
            object value;
            if (!PythonOps.TryGetBoundAttr(@object, Symbols.Dict, out value)) {
                throw PythonOps.TypeError("vars() argument must have __dict__ attribute");
            }
            return value;
        }

        public static object xrange = DynamicHelpers.GetDynamicTypeFromType(typeof(XRange)); //PyXRange.pytype;

        public static List zip(object s0, object s1) {
            IEnumerator i0 = PythonOps.GetEnumerator(s0);
            IEnumerator i1 = PythonOps.GetEnumerator(s1);
            List ret = new List();
            while (i0.MoveNext() && i1.MoveNext()) {
                ret.AddNoLock(Tuple.MakeTuple(i0.Current, i1.Current));
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
                ret.AddNoLock(Tuple.Make(items));
            }
        }

        public static object Exception = ExceptionConverter.GetPythonException("Exception");

        /// <summary>
        /// Gets the appropriate LanguageContext to be used for code compiled with Python's compile built-in
        /// </summary>
        private static PythonContext GetCompilerLanguageContext(CodeContext context, bool inheritContext, CompileFlags cflags) {
            if (inheritContext) {
                return (PythonContext)context.LanguageContext;
            } else if (((cflags & CompileFlags.CO_FUTURE_DIVISION) != 0)) {
                return (PythonContext)DefaultContext.DefaultTrueDivision.LanguageContext;
            } else {
                return (PythonContext)DefaultContext.Default.LanguageContext;
            }
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
                if ((cflags & ~(CompileFlags.CO_NESTED | CompileFlags.CO_GENERATOR_ALLOWED | CompileFlags.CO_FUTURE_DIVISION)) != 0) {
                    throw PythonOps.ValueError("unrecognized flags");
                }
            }

            return cflags;
        }

        /// <summary>
        /// Gets a scope used for executing new code in optionally replacing the globals and locals dictionaries.
        /// </summary>
        private static Microsoft.Scripting.Scope GetExecEvalScope(CodeContext context, IAttributesCollection globals, object localsDict) {
            if (globals == null) globals = Builtin.globals(context);
            if (localsDict == null) localsDict = locals(context);

            Microsoft.Scripting.Scope scope = new Microsoft.Scripting.Scope(
                new Microsoft.Scripting.Scope(globals),
                GetAttrLocals(context, localsDict));

            if (!globals.ContainsKey(Symbols.Builtins)) {
                globals[Symbols.Builtins] = SystemState.Instance.BuiltinModuleInstance;
            }
            return scope;
        }

    }
}
