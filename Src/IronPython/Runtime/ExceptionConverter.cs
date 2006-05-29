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
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Reflection;

using System.Threading;

using Builtin = IronPython.Modules.Builtin;

namespace IronPython.Runtime {

    /// <summary>
    /// Converts CLR exceptions to Python exceptions and vice-versa.
    /// </summary>
    public static class ExceptionConverter {
        static Dictionary<Type, DynamicType> clrToPython = new Dictionary<Type, DynamicType>();
        static Dictionary<DynamicType, Type> pythonToClr = new Dictionary<DynamicType, Type>();
        static Dictionary<QualifiedExceptionName, DynamicType> nameToPython = new Dictionary<QualifiedExceptionName, DynamicType>();

        // common methods on exception class
        static PythonFunction exceptionInitMethod;
        static PythonFunction exceptionGetItemMethod;
        static PythonFunction exceptionStrMethod;
        static PythonFunction syntaxErrorStrMethod;
        static PythonFunction unicodeErrorInit;
        static PythonFunction systemExitInitMethod;

        const string pythonExceptionKey = "PythonExceptionInfo";
        const string prevStackTraces = "PreviousStackTraces";
        const string defaultExceptionModule = "exceptions";

        /*********************************************************
         * Exception mapping hierarchy - this defines how we
         * map all Python exceptions onto CLR exceptions. 
         */
        static readonly ExceptionMapping[] exceptionMappings = new ExceptionMapping[]{
            new ExceptionMapping("Exception", typeof(System.Exception), new ExceptionMapping[]{
                new ExceptionMapping("SystemExit", typeof(PythonSystemExit), SystemExitExceptionCreator),
                new ExceptionMapping("StopIteration", typeof(StopIterationException)),
                new ExceptionMapping("StandardError", typeof(System.ApplicationException), new ExceptionMapping[]{
                    new ExceptionMapping("KeyboardInterrupt", typeof(PythonKeyboardInterrupt)),
                    new ExceptionMapping("ImportError", typeof(PythonImportError)),
                    new ExceptionMapping("EnvironmentError", typeof(System.Runtime.InteropServices.ExternalException), new ExceptionMapping[]{
                        new ExceptionMapping("IOError",typeof(System.IO.IOException)),
                        new ExceptionMapping("OSError", typeof(PythonOSError), new ExceptionMapping[]{
                            new ExceptionMapping("WindowsError", typeof(System.ComponentModel.Win32Exception))
                        }),
                    }),
                    new ExceptionMapping("EOFError", typeof(System.IO.EndOfStreamException)),
                    new ExceptionMapping("RuntimeError", typeof(PythonRuntimeError), new ExceptionMapping[]{
                        new ExceptionMapping("NotImplementedError", typeof(System.NotImplementedException)),
                    }),
                    new ExceptionMapping("NameError", typeof(PythonNameError), new ExceptionMapping[]{
                        new ExceptionMapping("UnboundLocalError", typeof(PythonUnboundLocalError)),
                    }),
                    new ExceptionMapping("AttributeError", typeof(System.MissingMemberException)),
                    new ExceptionMapping("SyntaxError", typeof(PythonSyntaxError), SyntaxErrorExceptionCreator, new ExceptionMapping[]{
                        new ExceptionMapping("IndentationError", typeof(PythonIndentationError), SyntaxErrorExceptionCreator, new ExceptionMapping[]{
                            new ExceptionMapping("TabError", typeof(PythonTabError), SyntaxErrorExceptionCreator)
                        }),
                    }),
                    new ExceptionMapping("TypeError", typeof(IronPython.Runtime.ArgumentTypeException)),
                    new ExceptionMapping("AssertionError", typeof(PythonAssertionError)),
                    new ExceptionMapping("LookupError", typeof(PythonLookupError), new ExceptionMapping[]{
                        new ExceptionMapping("IndexError", typeof(System.IndexOutOfRangeException)),
                        new ExceptionMapping("KeyError", typeof(System.Collections.Generic.KeyNotFoundException)),
                    }),

                    new ExceptionMapping("ArithmeticError", typeof(System.ArithmeticException), new ExceptionMapping[]{
                        new ExceptionMapping("OverflowError", typeof(System.OverflowException)),
                        new ExceptionMapping("ZeroDivisionError", typeof(System.DivideByZeroException)),
                        new ExceptionMapping("FloatingPointError", typeof(PythonFloatingPointError)),
                    }),
                    
                    new ExceptionMapping("ValueError", typeof(System.ArgumentException), new ExceptionMapping[]{
                        new ExceptionMapping("UnicodeError", typeof(PythonUnicodeError), new ExceptionMapping[]{
                            new ExceptionMapping("UnicodeEncodeError", typeof(System.Text.EncoderFallbackException), UnicodeErrorExceptionCreator),
                            new ExceptionMapping("UnicodeDecodeError", typeof(System.Text.DecoderFallbackException), UnicodeErrorExceptionCreator),
                            new ExceptionMapping("UnicodeTranslateError", typeof(PythonUnicodeTranslateError)),
                        }),
                    }),
                    new ExceptionMapping("ReferenceError", typeof(PythonReferenceError)),
                    new ExceptionMapping("SystemError", typeof(SystemException)),
                    new ExceptionMapping("MemoryError", typeof(System.OutOfMemoryException)),

                }),
                new ExceptionMapping("Warning",typeof(System.ComponentModel.WarningException), new ExceptionMapping[]{
                    new ExceptionMapping("UserWarning",typeof(PythonUserWarning)),
                    new ExceptionMapping("DeprecationWarning",typeof(PythonDeprecationWarning)),
                    new ExceptionMapping("PendingDeprecationWarning",typeof(PythonPendingDeprecationWarning)),
                    new ExceptionMapping("SyntaxWarning",typeof(PythonSyntaxWarning)),
                    new ExceptionMapping("OverflowWarning",typeof(PythonOverflowWarning)),
                    new ExceptionMapping("RuntimeWarning",typeof(PythonRuntimeWarning)),
                    new ExceptionMapping("FutureWarning",typeof(PythonFutureWarning)),
                }),
            })            
        };

        static ExceptionConverter() {
            exceptionInitMethod = new FunctionX(null, "__init__", new CallTargetN(ExceptionConverter.ExceptionInit), new string[] { "args" }, new object[0], IronPython.Compiler.FuncDefType.ArgList);
            exceptionGetItemMethod = new FunctionX(null, "__getitem__", new CallTargetN(ExceptionConverter.ExceptionGetItem), new string[] { "args" }, new object[0], IronPython.Compiler.FuncDefType.ArgList);
            exceptionStrMethod = new FunctionX(null, "__str__", new CallTargetN(ExceptionConverter.ExceptionToString), new string[] { "args" }, new object[0], IronPython.Compiler.FuncDefType.ArgList);
            syntaxErrorStrMethod = new FunctionX(null, "__str__",
                new CallTargetN(ExceptionConverter.SyntaxErrorToString), new string[] { "args" }, new object[0], IronPython.Compiler.FuncDefType.ArgList);
            unicodeErrorInit = new FunctionX(null,
                "__init__",
                new CallTargetN(ExceptionConverter.UnicodeErrorInit),
                new string[] { "self", "encoding", "object", "start", "end", "reason" }, new object[0], IronPython.Compiler.FuncDefType.None);
            systemExitInitMethod = new FunctionX(null, "__init__", new CallTargetN(ExceptionConverter.SystemExitExceptionInit), new string[] { "args" }, new object[0], IronPython.Compiler.FuncDefType.ArgList);

            for (int i = 0; i < exceptionMappings.Length; i++) {
                CreateExceptionMapping(null, exceptionMappings[i]);
            }

            // we also have a couple of explicit bonus mappings.
            clrToPython[typeof(InvalidCastException)] = GetPythonExceptionByName("TypeError");
            clrToPython[typeof(ArgumentNullException)] = GetPythonExceptionByName("TypeError");
        }

        #region Public API Surface
        /// <summary>
        /// Helper function for exception instances.  Initializes the
        /// exception
        /// </summary>
        public static object ExceptionInit(params object[] args) {
            Tuple t = args[0] as Tuple;
            if (t != null) {
                object self = t[0];
                object[] realArgs = new object[t.Count - 1];
                for (int i = 1; i < t.Count; i++) {
                    realArgs[i - 1] = t[i];
                }

                Ops.SetAttr(DefaultContext.Default, self, SymbolTable.Arguments, Tuple.Make(realArgs));
            }
            return null;
        }

        // Is this an exception object (as defined by Python)?
        private static bool IsExceptionObject(object e) {
            if (e == null) return false;
            if (e is Exception) return true;

            // It could be a PythonType created by CreateExceptionMapping
            if (e is OldInstance) {
                OldClass oldClass = ((OldInstance)e).__class__;
                return oldClass.IsSubclassOf(clrToPython[typeof(Exception)]);
            }

            return false;
        }

        public static object UnicodeErrorInit(params object[] prms) {
            if (prms.Length != 6) throw Ops.TypeError("expected 5 arguments, got {0}", prms.Length);

            object self = prms[0];
            string encoding = Converter.ConvertToString(prms[1]);
            string @object = Converter.ConvertToString(prms[2]);
            int start = Converter.ConvertToInt32(prms[3]);
            int end = Converter.ConvertToInt32(prms[4]);
            string reason = Converter.ConvertToString(prms[5]);

            ExceptionInit(new Tuple(false, new object[] { self, encoding, @object, start, end, reason }));

            Ops.SetAttr(DefaultContext.Default, self, SymbolTable.StringToId("encoding"), encoding);
            Ops.SetAttr(DefaultContext.Default, self, SymbolTable.StringToId("@object"), @object);
            Ops.SetAttr(DefaultContext.Default, self, SymbolTable.StringToId("start"), start);
            Ops.SetAttr(DefaultContext.Default, self, SymbolTable.StringToId("end"), end);
            Ops.SetAttr(DefaultContext.Default, self, SymbolTable.StringToId("reason"), reason);

            return null;
        }

        public static object SystemExitExceptionInit(params object[] prms) {
            if (prms.Length == 0) throw Ops.TypeError("expected at least 1 argument, got 0");

            Tuple tuple = prms[0] as Tuple;

            if (tuple != null) {
                object self = tuple[0];
                object[] argv = new object[tuple.Count - 1];

                for (int i = 1; i < tuple.Count; i++) {
                    argv[i - 1] = tuple[i];
                }
                object args = Tuple.MakeTuple(argv);
                object code = argv.Length == 0 ? null : argv.Length == 1 ? argv[0] : args;

                Ops.SetAttr(DefaultContext.Default, self, SymbolTable.StringToId("code"), code);
                Ops.SetAttr(DefaultContext.Default, self, SymbolTable.Arguments, args);
            }
    
            return null;
        }

        /// <summary>
        /// Helper function for exception instances.  Converts the exception to a string.
        // ie. Exception.__str__
        /// </summary>
        public static object ExceptionToString(params object[] args) {
            Debug.Assert(args.Length == 1);
            Tuple t = args[0] as Tuple;

            if (t == null || t.GetLength() == 0) throw Ops.TypeErrorForUnboundMethodCall("__str__", typeof(Exception), null);

            object self = t[0];

            if (!IsExceptionObject(self))
                throw Ops.TypeErrorForUnboundMethodCall("__str__", typeof(Exception), self);

            if (t.GetLength() != 1) throw Ops.TypeErrorForArgumentCountMismatch("__str__", 1, t.GetLength());

            // Get Exception.args
            object objArgs = Ops.GetAttr(DefaultContext.Default, self, SymbolTable.Arguments);
            Tuple tupArgs = objArgs as Tuple;

            // If the exception has "args", return it
            if (tupArgs != null) {
                switch (tupArgs.Count) {
                    case 0: return String.Empty;
                    case 1: return Ops.ToString(tupArgs[0]);
                    default: return Ops.ToString(tupArgs);
                }
            } else if (objArgs != null) {
                return Ops.ToString(objArgs);
            }

            // It could be a PythonType created by CreateExceptionMapping
            object cex;
            if (Ops.TryGetAttr(self, SymbolTable.ClrExceptionKey, out cex)) {
                Exception clrEx = cex as Exception;
                if (clrEx != null) {
                    return clrEx.Message;
                }
            }

            return String.Empty;
        }

        /// <summary>
        /// Helper ToString function for SyntaxError instances.  Returns
        /// line information in addition to just the mesage.
        /// </summary>
        public static object SyntaxErrorToString(params object[] args) {
            Tuple t = args[0] as Tuple;
            if (t != null) {
                object objArgs = Ops.GetAttr(DefaultContext.Default, t[0], SymbolTable.Arguments);
                Tuple tupArgs = objArgs as Tuple;

                if (tupArgs != null) {
                    switch (tupArgs.Count) {
                        case 0: return Ops.ToString(null);
                        case 1: return Ops.ToString(tupArgs[0]);
                        case 2:
                            string msg = tupArgs[0] as string;
                            if (msg != null) {
                                Tuple innerArgs = tupArgs[1] as Tuple;
                                if (innerArgs != null && innerArgs.Count == 4) {
                                    // real SyntaxError generated by us w/ line info
                                    return String.Format("{0} ({1}, line {2})", msg, innerArgs[0], innerArgs[1]);
                                }
                            }

                            goto default;
                        default: return Ops.ToString(tupArgs);
                    }
                } else if (objArgs != null) {
                    return Ops.ToString(objArgs);
                }

                object cex;
                if (Ops.TryGetAttr(t[0], SymbolTable.ClrExceptionKey, out cex)) {
                    Exception clrEx = cex as Exception;
                    if (clrEx != null) {
                        return clrEx.Message;
                    }
                }
            }
            return String.Empty;
        }

        /// <summary>
        /// Helper function that shows up on exception instances, get's an
        /// index from args.
        /// </summary>
        public static object ExceptionGetItem(params object[] args) {
            Tuple t = args[0] as Tuple;
            if (t != null) {
                return Ops.GetIndex(Ops.GetAttr(DefaultContext.Default, t[0], SymbolTable.Arguments), t[1]);
            }
            return null;
        }

        /// <summary>
        /// Returns the DynamicType for a given PythonException in the default exception module
        /// Creates the exception type if it doesn't yet exist.
        /// </summary>
        public static DynamicType GetPythonExceptionByName(string name) {
            return GetPythonExceptionByName(name, defaultExceptionModule);
        }

        /// <summary>
        /// Returns the DynamicType for a given PythonException in a specified module
        /// Creates the exception type if it doesn't yet exist.
        /// </summary>
        public static DynamicType GetPythonExceptionByName(string name, string module) {
            DynamicType res;
            QualifiedExceptionName key = new QualifiedExceptionName(name, module);
            if (nameToPython.TryGetValue(key, out res)) {
                return res;
            }

            res = DefaultExceptionCreator(name, module, GetPythonExceptionByName("Exception"));
            nameToPython[key] = res;
            return res;
        }

        /// <summary>
        /// Creates a CLR exception for the given type
        /// </summary>
        public static Exception CreateThrowable(object type) {
            object pyEx = Ops.Call(type);

            return ExceptionConverter.ToClr(pyEx);
        }


        /// <summary>
        /// Returns the CLR exception associated with a Python exception
        /// creating a new exception if necessary
        /// </summary>
        public static Exception ToClr(object pythonException) {
            object ret;
            if (Ops.TryGetAttr(pythonException, SymbolTable.ClrExceptionKey, out ret)) {
                Exception exRet = ret as Exception;
                if (exRet != null) return exRet;    // maybe the user assigned some value in behind our back
            }

            Type t;
            if (pythonException is OldInstance) {
                t = GetCLRTypeFromPython(((OldInstance)pythonException).__class__);
            } else {
                t = GetCLRTypeFromPython(Ops.GetDynamicType(pythonException));
            }

            ConstructorInfo ci = t.GetConstructor(new Type[] { typeof(string) });
            Debug.Assert(ci != null);

            // default exception message is the exception type (from Python)
            string msg = "";
            DynamicType dt = Ops.GetAttr(DefaultContext.Default, pythonException, SymbolTable.Class) as DynamicType;
            if (dt != null) {
                msg = dt.__name__.ToString();
            }

            Exception res = ci.Invoke(new object[] { msg }) as Exception;
            Debug.Assert(res != null);

            AssociateExceptions(res, pythonException);

            return res;
        }

        /// <summary>
        /// Returns the Python exception associated with a CLR exception
        /// creating a new Python exception if necessary.
        /// </summary>
        public static object ToPython(Exception clrException) {
            if (clrException.Data.Contains(pythonExceptionKey)) {
                // this is already associated w/ a CLR exception.
                return clrException.Data[pythonExceptionKey];
            }

            ThreadAbortException ta = clrException as ThreadAbortException;
            if (ta != null) {
                // transform TA w/ our reason into a KeyboardInterrupt exception.
                PythonKeyboardInterrupt reason = ta.ExceptionState as PythonKeyboardInterrupt;
                if (reason != null) return ToPython(reason);
            }

            object res;
            ICustomExceptionConversion ice = clrException as ICustomExceptionConversion;
            if (ice != null) {
                res = ice.ToPythonException();
            } else {
                // this is an exception raised from CLR space crossing
                // into Python space.  We need to create a new Python
                // exception.
                DynamicType pythonType = GetPythonTypeFromCLR(clrException.GetType());

                // create new instance of Python type and save it (we do this directly
                // as we're calling during low-stack situations and don't want to invoke
                // a python method that would do a stack-check).
                res = new OldInstance((OldClass)pythonType);

                Ops.SetAttr(DefaultContext.Default, res, SymbolTable.ExceptionMessage, clrException.Message);
                if (clrException.Message != null) {
                    Ops.SetAttr(DefaultContext.Default, res, SymbolTable.Arguments, Tuple.MakeTuple(clrException.Message));
                } else {
                    Ops.SetAttr(DefaultContext.Default, res, SymbolTable.Arguments, Tuple.MakeTuple());
                }
                //'filename', 'lineno','offset', 'print_file_and_line', 'text'
            }

            OldInstance exRes = res as OldInstance;
            if (exRes != null) AssociateExceptions(clrException, exRes);

            return res;
        }

        /// <summary>
        /// Updates an exception before it's getting re-thrown so
        /// we can present a reasonable stack trace to the user.
        /// </summary>
        public static Exception UpdateForRethrow(Exception rethrow) {
            List<StackTrace> prev = rethrow.Data[prevStackTraces] as List<StackTrace>;
            if (prev == null) {
                prev = new List<StackTrace>();
                rethrow.Data[prevStackTraces] = prev;
            }

            prev.Add(new StackTrace(rethrow, true));
            return rethrow;
        }

        /// <summary>
        /// Returns all the stack traces associates with an exception
        /// </summary>
        public static List<StackTrace> GetExceptionStackTraces(Exception rethrow) {
            return rethrow.Data[prevStackTraces] as List<StackTrace>;
        }


        /// <summary>
        /// Creates a new throwable exception of type type.  
        /// </summary>
        public static Exception CreateThrowable(object type, object value) {
            object pyEx;

            if (Builtin.IsInstance(value, type)) {
                pyEx = value;
            } else if (value is Tuple) {
                pyEx = Ops.CallWithArgsTuple(type, new object[0], value);
            } else {
                pyEx = Ops.Call(type, value);
            }

            return ExceptionConverter.ToClr(pyEx);
        }
        #endregion

        #region Private implementation details
        private static void CreateExceptionMapping(object baseType, ExceptionMapping em) {
            OldClass oc = em.Creator(em.PythonException, em.PythonModule, baseType);

            pythonToClr[oc] = em.CLRException;
            clrToPython[em.CLRException] = oc;
            nameToPython[new QualifiedExceptionName(em.PythonException, em.PythonModule)] = oc;

            if (em.SubTypes != null) {
                for (int i = 0; i < em.SubTypes.Length; i++) {

                    CreateExceptionMapping(oc, em.SubTypes[i]);
                }
            }
        }

        private static void AssociateExceptions(Exception ex, object pyEx) {
            ex.Data[pythonExceptionKey] = pyEx;
            Ops.SetAttr(DefaultContext.Default, pyEx, SymbolTable.ClrExceptionKey, ex);
        }

        private static DynamicType GetPythonTypeFromCLR(Type type) {
            DynamicType pythonType;
            if (clrToPython.TryGetValue(type, out pythonType)) {
                // direct mapping
                return pythonType;
            }

            // Find the closest parent which has a mapping
            Type curType = type.BaseType;
            while (curType != null) {
                if (clrToPython.TryGetValue(curType, out pythonType)) {
                    return pythonType;
                }
                curType = curType.BaseType;
            }

            return GetPythonExceptionByName("Exception");
        }

        private static Type GetCLRTypeFromPython(DynamicType type) {
            Type clrType;
            if (pythonToClr.TryGetValue(type, out clrType)) {
                return clrType;
            }
            // unknown type...  try walking the type hierarchy and 
            // throwing the closest match.
            Tuple curType = Ops.GetAttr(DefaultContext.Default, type, SymbolTable.Bases) as Tuple;
            if (curType != null) {
                for (int i = 0; i < curType.Count; i++) {
                    clrType = GetCLRTypeFromPython(curType[i] as DynamicType);
                    if (clrType != null) return clrType;
                }
            }
            return typeof(Exception);
        }

        private static OldClass DefaultExceptionCreator(string name, string module, object baseType) {
            Tuple bases = (baseType == null) ? Tuple.MakeTuple() : Tuple.MakeTuple(baseType);

            FieldIdDict dict = new FieldIdDict();
            dict[SymbolTable.Module] = module;
            OldClass oc = new OldClass(name, bases, dict);
            oc.SetAttr(DefaultContext.Default, SymbolTable.Init, exceptionInitMethod);
            oc.SetAttr(DefaultContext.Default, SymbolTable.GetItem, exceptionGetItemMethod);
            oc.SetAttr(DefaultContext.Default, SymbolTable.String, exceptionStrMethod);

            return oc;
        }

        private static OldClass SyntaxErrorExceptionCreator(string name, string module, object baseType) {
            OldClass syntaxError = DefaultExceptionCreator(name, module, baseType);

            syntaxError.SetAttr(DefaultContext.Default, SymbolTable.String, syntaxErrorStrMethod);
            return syntaxError;
        }

        private static OldClass SystemExitExceptionCreator(string name, string module, object baseType) {
            OldClass systemExit = DefaultExceptionCreator(name, module, baseType);
            systemExit.SetAttr(DefaultContext.Default, SymbolTable.Init, systemExitInitMethod);
            return systemExit;
        }

        private static OldClass UnicodeErrorExceptionCreator(string name, string module, object baseType) {
            OldClass unicodeError = DefaultExceptionCreator(name, module, baseType);

            unicodeError.SetAttr(DefaultContext.Default, SymbolTable.Init, unicodeErrorInit);

            return unicodeError;
        }

        /// <summary>
        /// Represents a single exception mapping from a CLR type to python type.
        /// </summary>
        private struct ExceptionMapping {
            public ExceptionMapping(string python, Type clr) {
                PythonException = python;
                PythonModule = ExceptionConverter.defaultExceptionModule;
                CLRException = clr;
                SubTypes = null;
                Creator = DefaultExceptionCreator;
            }

            public ExceptionMapping(string python, string module, Type clr) {
                PythonException = python;
                PythonModule = module;
                CLRException = clr;
                SubTypes = null;
                Creator = DefaultExceptionCreator;
            }

            public ExceptionMapping(string python, Type clr, CreateExceptionClass creator) {
                PythonException = python;
                PythonModule = ExceptionConverter.defaultExceptionModule;
                CLRException = clr;
                SubTypes = null;
                Creator = creator;
            }

            public ExceptionMapping(string python, string module, Type clr, ExceptionMapping[] subTypes) {
                PythonException = python;
                PythonModule = module;
                CLRException = clr;
                SubTypes = subTypes;
                Creator = DefaultExceptionCreator;
            }

            public ExceptionMapping(string python, Type clr, ExceptionMapping[] subTypes) {
                PythonException = python;
                PythonModule = ExceptionConverter.defaultExceptionModule;
                CLRException = clr;
                SubTypes = subTypes;
                Creator = DefaultExceptionCreator;
            }

            public ExceptionMapping(string python, Type clr, CreateExceptionClass creator, ExceptionMapping[] subTypes) {
                PythonException = python;
                PythonModule = ExceptionConverter.defaultExceptionModule;
                CLRException = clr;
                SubTypes = subTypes;
                Creator = creator;
            }

            public string PythonException;
            public string PythonModule;
            public Type CLRException;
            public ExceptionMapping[] SubTypes;
            public CreateExceptionClass Creator;

            public delegate OldClass CreateExceptionClass(string name, string module, object baseType);
        }

        /// <summary>
        /// Python exception name, qualified with the module name, e.g. exceptions.TypeError
        /// </summary>
        private struct QualifiedExceptionName {
            public QualifiedExceptionName(string name, string module) {
                Name = name;
                Module = module;
            }
            public override int GetHashCode() {
                return Name.GetHashCode() ^ Module.GetHashCode();
            }
            public string Name;
            public string Module;
        }
        #endregion
    }
}
