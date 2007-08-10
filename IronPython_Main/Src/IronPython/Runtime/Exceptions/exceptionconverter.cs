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
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Microsoft.Scripting;

using IronPython.Runtime;
using IronPython.Runtime.Types;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;

using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Shell;
using Microsoft.Scripting.Utils;

[module: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Scope = "member", Target = "IronPython.Runtime.Exceptions.ExceptionConverter.CreateThrowable(System.Object):System.Exception", MessageId = "Throwable")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Scope = "member", Target = "IronPython.Runtime.Exceptions.ExceptionConverter.CreateThrowable(System.Object,System.Object):System.Exception", MessageId = "Throwable")]

namespace IronPython.Runtime.Exceptions {

    /// <summary>
    /// Converts CLR exceptions to Python exceptions and vice-versa.
    /// </summary>
    public static class ExceptionConverter {
        static Dictionary<Type, object> clrToPython = new Dictionary<Type, object>();
        static Dictionary<object, Type> pythonToClr = new Dictionary<object, Type>();
        static Dictionary<QualifiedExceptionName, OldClass> nameToPython = new Dictionary<QualifiedExceptionName, OldClass>();

        // common methods on exception class
        static PythonFunction exceptionInitMethod;
        static PythonFunction exceptionGetItemMethod;
        static PythonFunction exceptionStrMethod;
        static PythonFunction exceptionGetStateMethod;
        static PythonFunction syntaxErrorStrMethod;
        static PythonFunction unicodeErrorInit;
        static PythonFunction systemExitInitMethod;

        const string pythonExceptionKey = "PythonExceptionInfo";
        public const string defaultExceptionModule = "exceptions";
        static readonly OldClass defaultExceptionBaseType; // assigned in static constructor

        /*********************************************************
         * Exception mapping hierarchy - this defines how we
         * map all Python exceptions onto CLR exceptions. 
         */
        static readonly ExceptionMapping[] exceptionMappings = new ExceptionMapping[]{
            new ExceptionMapping("Exception", typeof(System.Exception), new ExceptionMapping[]{
                new ExceptionMapping("SystemExit", typeof(PythonSystemExitException), SystemExitExceptionCreator),
                new ExceptionMapping("StopIteration", typeof(StopIterationException)),
                new ExceptionMapping("StandardError", typeof(System.ApplicationException), new ExceptionMapping[]{
                    new ExceptionMapping("KeyboardInterrupt", typeof(KeyboardInterruptException)),
                    new ExceptionMapping("ImportError", typeof(PythonImportErrorException)),
                    new ExceptionMapping("EnvironmentError", typeof(System.Runtime.InteropServices.ExternalException), new ExceptionMapping[]{
                        new ExceptionMapping("IOError",typeof(System.IO.IOException)),
                        new ExceptionMapping("OSError", typeof(PythonOSErrorException), 
#if SILVERLIGHT // System.ComponentModel.Win32Exception
                            (ExceptionMapping [])null
#else
                            new ExceptionMapping[]{
                                new ExceptionMapping("WindowsError", typeof(System.ComponentModel.Win32Exception))
                            }
#endif
                        ),
                    }),
                    new ExceptionMapping("EOFError", typeof(System.IO.EndOfStreamException)),
                    new ExceptionMapping("RuntimeError", typeof(PythonRuntimeErrorException), new ExceptionMapping[]{
                        new ExceptionMapping("NotImplementedError", typeof(System.NotImplementedException)),
                    }),
                    new ExceptionMapping("NameError", typeof(UnboundNameException), new ExceptionMapping[]{
                        new ExceptionMapping("UnboundLocalError", typeof(UnboundLocalException)),
                    }),
                    new ExceptionMapping("AttributeError", typeof(System.MissingMemberException)),
                    new ExceptionMapping("SyntaxError", typeof(SyntaxErrorException), SyntaxErrorExceptionCreator, new ExceptionMapping[]{
                        new ExceptionMapping("IndentationError", typeof(PythonIndentationError), SyntaxErrorExceptionCreator, new ExceptionMapping[]{
                            new ExceptionMapping("TabError", typeof(PythonTabError), SyntaxErrorExceptionCreator)
                        }),
                    }),
                    new ExceptionMapping("TypeError", typeof(ArgumentTypeException)),
                    new ExceptionMapping("AssertionError", typeof(PythonAssertionErrorException)),
                    new ExceptionMapping("LookupError", typeof(PythonLookupErrorException), new ExceptionMapping[]{
                        new ExceptionMapping("IndexError", typeof(System.IndexOutOfRangeException)),
                        new ExceptionMapping("KeyError", typeof(System.Collections.Generic.KeyNotFoundException)),
                    }),

                    new ExceptionMapping("ArithmeticError", typeof(System.ArithmeticException), new ExceptionMapping[]{
                        new ExceptionMapping("OverflowError", typeof(System.OverflowException)),
                        new ExceptionMapping("ZeroDivisionError", typeof(System.DivideByZeroException)),
                        new ExceptionMapping("FloatingPointError", typeof(PythonFloatingPointErrorException)),
                    }),
                    
                    new ExceptionMapping("ValueError", typeof(System.ArgumentException), new ExceptionMapping[]{
                        new ExceptionMapping("UnicodeError", typeof(PythonUnicodeErrorException), new ExceptionMapping[]{
#if !SILVERLIGHT // System.Text.EncoderFallbackException
                            new ExceptionMapping("UnicodeEncodeError", typeof(System.Text.EncoderFallbackException), UnicodeErrorExceptionCreator),
                            new ExceptionMapping("UnicodeDecodeError", typeof(System.Text.DecoderFallbackException), UnicodeErrorExceptionCreator),
#endif
                            new ExceptionMapping("UnicodeTranslateError", typeof(PythonUnicodeTranslateErrorException)),
                        }),
                    }),
                    new ExceptionMapping("ReferenceError", typeof(PythonReferenceErrorException)),
                    new ExceptionMapping("SystemError", typeof(SystemException)),
                    new ExceptionMapping("MemoryError", typeof(System.OutOfMemoryException)),

                }),
                new ExceptionMapping("Warning",typeof(System.ComponentModel.WarningException), new ExceptionMapping[]{
                    new ExceptionMapping("UserWarning",typeof(PythonUserWarningException)),
                    new ExceptionMapping("DeprecationWarning",typeof(PythonDeprecationWarningException)),
                    new ExceptionMapping("PendingDeprecationWarning",typeof(PythonPendingDeprecationWarningException)),
                    new ExceptionMapping("SyntaxWarning",typeof(PythonSyntaxWarningException)),
                    new ExceptionMapping("OverflowWarning",typeof(PythonOverflowWarningException)),
                    new ExceptionMapping("RuntimeWarning",typeof(PythonRuntimeWarningException)),
                    new ExceptionMapping("FutureWarning",typeof(PythonFutureWarningException)),
                }),
            })            
        };

        static ExceptionConverter() {
            Debug.Assert(DefaultContext.Default != null);
            exceptionInitMethod = new FunctionX(DefaultContext.Default, "__init__", new CallTargetWithContextN(ExceptionConverter.ExceptionInit), new string[] { "args" }, RuntimeHelpers.EmptyObjectArray, FunctionAttributes.ArgumentList);
            exceptionGetItemMethod = new FunctionX(DefaultContext.Default, "__getitem__", new CallTargetWithContextN(ExceptionConverter.ExceptionGetItem), new string[] { "args" }, RuntimeHelpers.EmptyObjectArray, FunctionAttributes.ArgumentList);
            exceptionStrMethod = new FunctionX(DefaultContext.Default, "__str__", new CallTargetWithContextN(ExceptionConverter.ExceptionToString), new string[] { "args" }, RuntimeHelpers.EmptyObjectArray, FunctionAttributes.ArgumentList);
            exceptionGetStateMethod = new FunctionX(DefaultContext.Default, "__getstate__", new CallTargetWithContextN(ExceptionConverter.ExceptionGetState), new string[] { "args" }, RuntimeHelpers.EmptyObjectArray, FunctionAttributes.ArgumentList);
            syntaxErrorStrMethod = new FunctionX(DefaultContext.Default, "__str__",
                new CallTargetWithContextN(ExceptionConverter.SyntaxErrorToString), new string[] { "args" }, RuntimeHelpers.EmptyObjectArray, FunctionAttributes.ArgumentList);
            unicodeErrorInit = new FunctionX(DefaultContext.Default,
                "__init__",
                new CallTargetWithContextN(ExceptionConverter.UnicodeErrorInit),
                new string[] { "self", "encoding", "object", "start", "end", "reason" }, RuntimeHelpers.EmptyObjectArray, FunctionAttributes.None);
            systemExitInitMethod = new FunctionX(DefaultContext.Default, "__init__", new CallTargetWithContextN(ExceptionConverter.SystemExitExceptionInit), new string[] { "args" }, RuntimeHelpers.EmptyObjectArray, FunctionAttributes.ArgumentList);

            for (int i = 0; i < exceptionMappings.Length; i++) {
                CreateExceptionMapping(null, exceptionMappings[i]);
            }

            defaultExceptionBaseType = nameToPython[new QualifiedExceptionName("Exception", defaultExceptionModule)];

            // we also have a couple of explicit bonus mappings.
            clrToPython[typeof(InvalidCastException)] = GetPythonException("TypeError");
            clrToPython[typeof(ArgumentNullException)] = GetPythonException("TypeError");
        }

        public static object ExceptionInit(CodeContext context, object[] args) {
            return ExceptionInit(args);
        }

        public static object ExceptionGetItem(CodeContext context, object[] args) {
            return ExceptionGetItem(args);
        }

        public static object ExceptionGetState(CodeContext context, object[] args) {
            return ExceptionGetState(args);
        }

        public static object ExceptionToString(CodeContext context, object[] args) {
            return ExceptionToString(args);
        }

        public static object SyntaxErrorToString(CodeContext context, object[] args) {
            return SyntaxErrorToString(args);
        }

        public static object UnicodeErrorInit(CodeContext context, object[] args) {
            return UnicodeErrorInit(args);
        }

        public static object SystemExitExceptionInit(CodeContext context, object[] args) {
            return SystemExitExceptionInit(args);
        }


        #region Public API Surface
        /// <summary>
        /// Helper function for exception instances.  Initializes the
        /// exception
        /// </summary>
        /// 
        public static object ExceptionInit(params object[] args) {
            Tuple t = args[0] as Tuple;
            if (t != null) {
                object self = t[0];
                object[] realArgs = new object[t.Count - 1];
                for (int i = 1; i < t.Count; i++) {
                    realArgs[i - 1] = t[i];
                }

                PythonOps.SetAttr(DefaultContext.Default, self, Symbols.Arguments, Tuple.Make(realArgs));
            }
            return null;
        }

        // Is this an exception object (as defined by Python)?
        private static bool IsExceptionObject(object e) {
            if (e == null) return false;
            if (e is Exception) return true;

            // It could be a DynamicType created by CreateExceptionMapping
            if (e is OldInstance) {
                OldClass oldClass = ((OldInstance)e).__class__;
                return oldClass.IsSubclassOf(clrToPython[typeof(Exception)]);
            }

            return false;
        }

        public static object UnicodeErrorInit(params object[] parameters) {
            if (parameters.Length != 6) throw PythonOps.TypeError("expected 5 arguments, got {0}", parameters.Length);

            object self = parameters[0];
            string encoding = Converter.ConvertToString(parameters[1]);
            string @object = Converter.ConvertToString(parameters[2]);
            int start = Converter.ConvertToInt32(parameters[3]);
            int end = Converter.ConvertToInt32(parameters[4]);
            string reason = Converter.ConvertToString(parameters[5]);

            ExceptionInit(new Tuple(false, new object[] { self, encoding, @object, start, end, reason }));

            PythonOps.SetAttr(DefaultContext.Default, self, SymbolTable.StringToId("encoding"), encoding);
            PythonOps.SetAttr(DefaultContext.Default, self, SymbolTable.StringToId("object"), @object);
            PythonOps.SetAttr(DefaultContext.Default, self, SymbolTable.StringToId("start"), start);
            PythonOps.SetAttr(DefaultContext.Default, self, SymbolTable.StringToId("end"), end);
            PythonOps.SetAttr(DefaultContext.Default, self, SymbolTable.StringToId("reason"), reason);

            return null;
        }

        public static object SystemExitExceptionInit(params object[] parameters) {
            if (parameters.Length == 0) throw PythonOps.TypeError("expected at least 1 argument, got 0");

            Tuple tuple = parameters[0] as Tuple;

            if (tuple != null) {
                object self = tuple[0];
                object[] argv = new object[tuple.Count - 1];

                for (int i = 1; i < tuple.Count; i++) {
                    argv[i - 1] = tuple[i];
                }
                object args = Tuple.MakeTuple(argv);
                object code = argv.Length == 0 ? null : argv.Length == 1 ? argv[0] : args;

                PythonOps.SetAttr(DefaultContext.Default, self, SymbolTable.StringToId("code"), code);
                PythonOps.SetAttr(DefaultContext.Default, self, Symbols.Arguments, args);
            }

            return null;
        }

        /// <summary>
        /// Helper function for exception instances.  Converts the exception to a string.
        /// ie. Exception.__str__
        /// </summary>
        public static object ExceptionToString(params object[] args) {
            Debug.Assert(args.Length == 1);
            Tuple t = args[0] as Tuple;

            if (t == null || t.GetLength() == 0) throw PythonOps.TypeErrorForUnboundMethodCall("__str__", typeof(Exception), null);

            object self = t[0];

            if (!IsExceptionObject(self))
                throw PythonOps.TypeErrorForUnboundMethodCall("__str__", typeof(Exception), self);

            if (t.GetLength() != 1) throw PythonOps.TypeErrorForArgumentCountMismatch("__str__", 1, t.GetLength());

            // Get Exception.args
            object objArgs = PythonOps.GetBoundAttr(DefaultContext.Default, self, Symbols.Arguments);
            Tuple tupArgs = objArgs as Tuple;

            // If the exception has "args", return it
            if (tupArgs != null) {
                switch (tupArgs.Count) {
                    case 0: return String.Empty;
                    case 1: return PythonOps.ToString(tupArgs[0]);
                    default: return PythonOps.ToString(tupArgs);
                }
            } else if (objArgs != null) {
                return PythonOps.ToString(objArgs);
            }

            // It could be a DynamicType created by CreateExceptionMapping
            object cex;
            if (PythonOps.TryGetBoundAttr(self, Symbols.ClrExceptionKey, out cex)) {
                Exception clrEx = cex as Exception;
                if (clrEx != null) {
                    return clrEx.Message;
                }
            }

            return String.Empty;
        }

        /// <summary>
        /// Helper function for exception instances. Returns pickled object state.
        /// ie. Exception.__getstate__
        /// </summary>
        public static PythonDictionary ExceptionGetState(params object[] args) {
            Debug.Assert(args.Length == 1);
            Tuple t = args[0] as Tuple;

            if (t == null || t.GetLength() == 0) throw PythonOps.TypeErrorForUnboundMethodCall("__getstate__", typeof(Exception), null);

            object self = t[0];

            if (!IsExceptionObject(self))
                throw PythonOps.TypeErrorForUnboundMethodCall("__getstate__", typeof(Exception), self);

            if (t.GetLength() != 1) throw PythonOps.TypeErrorForArgumentCountMismatch("__getstate__", 1, t.GetLength());

            OldInstance selfObj = (OldInstance)self;
            PythonDictionary dictCopy = new PythonDictionary(selfObj.GetCustomMemberDictionary(DefaultContext.Default).Count - 1);
            foreach (KeyValuePair<object, object> item in selfObj.GetCustomMemberDictionary(DefaultContext.Default)) {
                if (item.Key is string && (string)item.Key == SymbolTable.IdToString(Symbols.ClrExceptionKey)) continue;
                dictCopy[item.Key] = item.Value;
            }

            return dictCopy;
        }

        /// <summary>
        /// Helper ToString function for SyntaxError instances.  Returns
        /// line information in addition to just the mesage.
        /// </summary>
        public static object SyntaxErrorToString(params object[] args) {
            Tuple t = args[0] as Tuple;
            if (t != null) {
                object objArgs = PythonOps.GetBoundAttr(DefaultContext.Default, t[0], Symbols.Arguments);
                Tuple tupArgs = objArgs as Tuple;

                if (tupArgs != null) {
                    switch (tupArgs.Count) {
                        case 0: return PythonOps.ToString(null);
                        case 1: return PythonOps.ToString(tupArgs[0]);
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
                        default: return PythonOps.ToString(tupArgs);
                    }
                } else if (objArgs != null) {
                    return PythonOps.ToString(objArgs);
                }

                object cex;
                if (PythonOps.TryGetBoundAttr(t[0], Symbols.ClrExceptionKey, out cex)) {
                    Exception clrEx = cex as Exception;
                    if (clrEx != null) {
                        return clrEx.Message;
                    }
                }
            }
            return String.Empty;
        }

        /// <summary>
        /// Helper function that shows up on exception instances, gets an
        /// index from args.
        /// </summary>
        public static object ExceptionGetItem(params object[] args) {
            Tuple t = args[0] as Tuple;
            if (t != null) {
                return PythonOps.GetIndex(PythonOps.GetBoundAttr(DefaultContext.Default, t[0], Symbols.Arguments), t[1]);
            }
            return null;
        }

        /// <summary>
        /// Returns the IPythonType for a given PythonException in the default exception module
        /// Throws KeyNotFoundException it doesn't exist.
        /// </summary>
        public static OldClass GetPythonException(string name) {
            return GetPythonException(name, defaultExceptionModule);
        }

        /// <summary>
        /// Returns the IPythonType for a given PythonException in a specified module.
        /// Throws KeyNotFoundException it doesn't exist.
        /// </summary>
        public static OldClass GetPythonException(string name, string module) {
            OldClass type;
            QualifiedExceptionName key = new QualifiedExceptionName(name, module);
            if (nameToPython.TryGetValue(key, out type)) {
                return type;
            }
            throw new KeyNotFoundException("exception " + module + "." + name + " does not exist");
        }

        /// <summary>
        /// Creates and returns the IPythonType for a given PythonException in the default exception module with the default base type.
        /// Throws InvalidOperationException if it already exists and and doesn't have the default base type.
        /// </summary>
        public static OldClass CreatePythonException(string name) {
            return CreatePythonException(name, defaultExceptionModule);
        }

        /// <summary>
        /// Creates and returns the IPythonType for a given PythonException in a given module with the default base type.
        /// Throws InvalidOperationException if it already exists and and doesn't have the default base type.
        /// 
        /// Note that specifying the module doesn't actually place the created type in that module.
        /// The type knows about the module, but the module doesn't know about the type. It's the caller's
        /// responsibility to put the returned type in the appropriate module.
        /// </summary>
        public static OldClass CreatePythonException(string name, string module) {
            return CreatePythonException(name, module, defaultExceptionBaseType);
        }

        /// <summary>
        /// Creates and returns the IPythonType for a given PythonException in a given module with a given base type.
        /// Throws InvalidOperationException if it already exists and has a different base type.
        /// 
        /// Note that specifying the module doesn't actually place the created type in that module.
        /// The type knows about the module, but the module doesn't know about the type. It's the caller's
        /// responsibility to put the returned type in the appropriate module.
        /// </summary>
        public static OldClass CreatePythonException(string name, string module, OldClass baseType) {
            return CreatePythonException(name, module, baseType, DefaultExceptionCreator);
        }

        /// <summary>
        /// Creates and returns the IPythonType for a given PythonException in a given module with a given base type using a given exception creator.
        /// Throws InvalidOperationException if it already exists and has a different base type.
        /// 
        /// Note that specifying the module doesn't actually place the created type in that module.
        /// The type knows about the module, but the module doesn't know about the type. It's the caller's
        /// responsibility to put the returned type in the appropriate module.
        /// </summary>
        public static OldClass CreatePythonException(string name, string module, OldClass baseType, ExceptionClassCreator creator) {
            OldClass type;
            QualifiedExceptionName key = new QualifiedExceptionName(name, module);
            if (nameToPython.TryGetValue(key, out type)) {
                if(type.BaseClasses.Count == 1 && type.BaseClasses[0] == baseType) {
                    return type;
                } else {
                    throw new InvalidOperationException(module + "." + name + " already exists with different base type(s)");
                }
            }

            OldClass res = creator(name, module, baseType);
            nameToPython[key] = res;
            return res;
        }

        /// <summary>
        /// Creates a CLR exception for the given type
        /// </summary>
        public static Exception CreateThrowable(object type) {
            object pyEx = PythonCalls.Call(type);

            return ExceptionConverter.ToClr(pyEx);
        }


        /// <summary>
        /// Returns the CLR exception associated with a Python exception
        /// creating a new exception if necessary
        /// </summary>
        public static Exception ToClr(object pythonException) {
            object ret;
            if (PythonOps.TryGetBoundAttr(pythonException, Symbols.ClrExceptionKey, out ret)) {
                Exception exRet = ret as Exception;
                if (exRet != null) return exRet;    // maybe the user assigned some value in behind our back
            }

            Type t;
            if (pythonException is OldInstance) {
                t = GetCLRTypeFromPython(((OldInstance)pythonException).__class__);
            } else {
                t = GetCLRTypeFromPython(DynamicHelpers.GetDynamicType(pythonException));
            }

            ConstructorInfo ci = t.GetConstructor(new Type[] { typeof(string) });
            Debug.Assert(ci != null);

            // default exception message is the exception type (from Python)
            string msg = "";
            OldClass dt = PythonOps.GetBoundAttr(DefaultContext.Default, pythonException, Symbols.Class) as OldClass;
            if (dt != null) {
                msg = dt.Name;
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
            if (clrException == null) throw new ArgumentNullException("clrException");

            object result;
            if (TryGetAssociatedException(clrException, out result)) {
                Debug.Assert(result != null);
                return result;
            }

#if !SILVERLIGHT // ThreadAbortException.ExceptionState
            ThreadAbortException ta = clrException as ThreadAbortException;
            if (ta != null) {
                // transform TA w/ our reason into a KeyboardInterrupt exception.
                KeyboardInterruptException reason = ta.ExceptionState as KeyboardInterruptException;
                if (reason != null) return ToPython(reason);
            }
#endif
            object res;
            SyntaxErrorException syntax;            
            if (clrException is StringException) {
                return clrException;
            } else if ((syntax = clrException as SyntaxErrorException) != null) {
                return SyntaxErrorToPython(syntax);                
            }

            // this is an exception raised from CLR space crossing
            // into Python space.  We need to create a new Python
            // exception.
            object pythonType = GetPythonTypeFromCLR(clrException.GetType());

            // create new instance of Python type and save it (we do this directly
            // as we're calling during low-stack situations and don't want to invoke
            // a python method that would do a stack-check).
            res = new OldInstance((OldClass)pythonType);

            PythonOps.SetAttr(DefaultContext.Default, res, Symbols.ExceptionMessage, clrException.Message);
            if (clrException.Message != null) {
                PythonOps.SetAttr(DefaultContext.Default, res, Symbols.Arguments, Tuple.MakeTuple(clrException.Message));
            } else {
                PythonOps.SetAttr(DefaultContext.Default, res, Symbols.Arguments, Tuple.MakeTuple());
            }
            //'filename', 'lineno','offset', 'print_file_and_line', 'text'

            OldInstance exRes = res as OldInstance;
            if (exRes != null) AssociateExceptions(clrException, exRes);

            Debug.Assert(res != null);
            return res;
        }

        private static object SyntaxErrorToPython(SyntaxErrorException e) {
            OldClass exType = ExceptionConverter.GetPythonException(GetPythonSyntaxErrorTypeName(e));
            object inst = PythonCalls.Call(exType);

            PythonOps.SetAttr(DefaultContext.Default, inst, Symbols.ExceptionMessage, e.Message);

            PythonOps.SetAttr(DefaultContext.Default, inst, Symbols.Arguments, Tuple.MakeTuple(
                e.Message,
                Tuple.MakeTuple(
                    e.FileName,
                    e.Line,
                    e.Column == 0 ? null : (object)e.Column,
                    e.LineText
                )
                ));

            PythonOps.SetAttr(DefaultContext.Default, inst, Symbols.ExceptionFilename, e.FileName);
            PythonOps.SetAttr(DefaultContext.Default, inst, Symbols.ExceptionLineNumber, e.Line);
            if (e.Column != 0) PythonOps.SetAttr(DefaultContext.Default, inst, Symbols.ExceptionOffset, e.Column);
            else PythonOps.SetAttr(DefaultContext.Default, inst, Symbols.ExceptionOffset, null);

            PythonOps.SetAttr(DefaultContext.Default, inst, Symbols.Text, e.LineText);

            AssociateExceptions(e, inst);

            return inst;
        }

        private static string GetPythonSyntaxErrorTypeName(SyntaxErrorException e) {
            if (e is PythonTabError) return "TabError";
            if (e is PythonIndentationError) return "IndentationError";
            
            return "SyntaxError";
        }
        /// <summary>
        /// Creates a new throwable exception of type type.  
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Throwable")]
        public static Exception CreateThrowable(object type, object value) {
            object pyEx;

            if (PythonOps.IsInstance(value, type)) {
                pyEx = value;
            } else if (value is Tuple) {
                pyEx = PythonOps.CallWithArgsTuple(type, RuntimeHelpers.EmptyObjectArray, value);
            } else {
                pyEx = PythonCalls.Call(type, value);
            }

            return ExceptionConverter.ToClr(pyEx);
        }

        public static void CreateExceptionMapping(OldClass baseType, ExceptionMapping em) {
            OldClass type = CreatePythonException(em.PythonException, em.PythonModule, baseType, em.Creator);

            pythonToClr[type] = em.ClrException;
            clrToPython[em.ClrException] = type;

            if (em.Subtypes != null) {
                for (int i = 0; i < em.Subtypes.Count; i++) {
                    CreateExceptionMapping(type, em.Subtypes[i]);
                }
            }
        }

        #endregion

        #region Private implementation details

        private static void AssociateException(Exception e, object data) {
            ExceptionUtils.GetDataDictionary(e)[pythonExceptionKey] = data;
        }

        private static bool TryGetAssociatedException(Exception e, out object data) {
            if (ExceptionUtils.GetDataDictionary(e).Contains(pythonExceptionKey)) {
                data = ExceptionUtils.GetDataDictionary(e)[pythonExceptionKey];
                return true;
            } else {
                data = null;
                return false;
            }
        }


        private static void AssociateExceptions(Exception ex, object pyEx) {
            AssociateException(ex, pyEx);
            PythonOps.SetAttr(DefaultContext.Default, pyEx, Symbols.ClrExceptionKey, ex);
        }

        private static object GetPythonTypeFromCLR(Type type) {
            object pythonType;
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

            return GetPythonException("Exception");
        }

        private static Type GetCLRTypeFromPython(object type) {
            Type clrType;
            if (pythonToClr.TryGetValue(type, out clrType)) {
                return clrType;
            }
            // unknown type...  try walking the type hierarchy and 
            // throwing the closest match.
            Tuple curType = PythonOps.GetBoundAttr(DefaultContext.Default, type, Symbols.Bases) as Tuple;
            if (curType != null) {
                for (int i = 0; i < curType.Count; i++) {
                    clrType = GetCLRTypeFromPython(curType[i]);
                    if (clrType != null) return clrType;
                }
            }
            return typeof(Exception);
        }

        internal static OldClass DefaultExceptionCreator(string name, string module, object baseType) {
            IAttributesCollection dict = new SymbolDictionary();
            dict[Symbols.Module] = module;
            List<OldClass> bt = new List<OldClass>(1);
            if (baseType != null) {
                bt.Add((OldClass)baseType);
            }
            OldClass oc = new OldClass(name, bt, dict, "");
            oc.SetCustomMember(DefaultContext.Default, Symbols.Init, exceptionInitMethod);
            oc.SetCustomMember(DefaultContext.Default, Symbols.GetItem, exceptionGetItemMethod);
            oc.SetCustomMember(DefaultContext.Default, Symbols.String, exceptionStrMethod);
            oc.SetCustomMember(DefaultContext.Default, Symbols.GetState, exceptionGetStateMethod);

            return oc;
        }

        private static Tuple ObjectToTuple(object obj) {
            return (obj == null) ? Tuple.MakeTuple() : Tuple.MakeTuple(obj);
        }

        private static OldClass SyntaxErrorExceptionCreator(string name, string module, object baseType) {
            OldClass syntaxError = DefaultExceptionCreator(name, module, baseType);

            syntaxError.SetCustomMember(DefaultContext.Default, Symbols.String, syntaxErrorStrMethod);
            return syntaxError;
        }

        private static OldClass SystemExitExceptionCreator(string name, string module, object baseType) {
            OldClass systemExit = DefaultExceptionCreator(name, module, baseType);
            systemExit.SetCustomMember(DefaultContext.Default, Symbols.Init, systemExitInitMethod);
            return systemExit;
        }

        private static OldClass UnicodeErrorExceptionCreator(string name, string module, object baseType) {
            OldClass unicodeError = DefaultExceptionCreator(name, module, baseType);

            unicodeError.SetCustomMember(DefaultContext.Default, Symbols.Init, unicodeErrorInit);

            return unicodeError;
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
            public override string ToString() {
                return Module + "." + Name;
            }
            public string Name;
            public string Module;
        }
        #endregion
    }

    public delegate OldClass ExceptionClassCreator(string name, string module, OldClass baseType);

    /// <summary>
    /// Represents a single exception mapping from a CLR type to python type.
    /// </summary>
    public struct ExceptionMapping {
        private string _pythonException;
        private string _pythonModule;
        private Type _clrException;
        private ExceptionMapping[] _subtypes;
        private ExceptionClassCreator _creator;

        public ExceptionMapping(string python, Type clr) {
            _pythonException = python;
            _pythonModule = ExceptionConverter.defaultExceptionModule;
            _clrException = clr;
            this._subtypes = null;
            _creator = ExceptionConverter.DefaultExceptionCreator;
        }

        public ExceptionMapping(string python, string module, Type clr) {
            _pythonException = python;
            _pythonModule = module;
            _clrException = clr;
            this._subtypes = null;
            _creator = ExceptionConverter.DefaultExceptionCreator;
        }

        public ExceptionMapping(string python, Type clr, ExceptionClassCreator creator) {
            _pythonException = python;
            _pythonModule = ExceptionConverter.defaultExceptionModule;
            _clrException = clr;
            this._subtypes = null;
            this._creator = creator;
        }

        public ExceptionMapping(string python, string module, Type clr, ExceptionMapping[] subtypes) {
            _pythonException = python;
            _pythonModule = module;
            _clrException = clr;
            this._subtypes = subtypes;
            _creator = ExceptionConverter.DefaultExceptionCreator;
        }

        public ExceptionMapping(string python, Type clr, ExceptionMapping[] subtypes) {
            _pythonException = python;
            _pythonModule = ExceptionConverter.defaultExceptionModule;
            _clrException = clr;
            this._subtypes = subtypes;
            _creator = ExceptionConverter.DefaultExceptionCreator;
        }

        public ExceptionMapping(string python, Type clr, ExceptionClassCreator creator, ExceptionMapping[] subtypes) {
            _pythonException = python;
            _pythonModule = ExceptionConverter.defaultExceptionModule;
            _clrException = clr;
            this._subtypes = subtypes;
            this._creator = creator;
        }


        public string PythonException {
            get { return _pythonException; }
        }

        public string PythonModule {
            get { return _pythonModule; }
        }

        public Type ClrException {
            get { return _clrException; }
        }

        public IList<ExceptionMapping> Subtypes {
            get { return _subtypes; }
        }

        public ExceptionClassCreator Creator {
            get { return _creator; }
        }
    }

}
