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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

using Microsoft.Scripting;
using Microsoft.Scripting.Shell;
using Microsoft.Scripting.Utils;

using IronPython.Compiler.Generation;
using IronPython.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

[assembly: PythonModule("exceptions", typeof(IronPython.Runtime.Exceptions.PythonExceptions))]
namespace IronPython.Runtime.Exceptions {
    /// <summary>
    /// Implementation of the Python exceptions module and the IronPython/CLR exception mapping 
    /// mechanism.  The exception module is the parent module for all Python exception classes
    /// and therefore is built-in to IronPython.dll instead of IronPython.Modules.dll.
    /// 
    /// The exception mapping mechanism is exposed as internal surface area available to only
    /// IronPython / IronPython.Modules.dll.  The actual exceptions themselves are all public.
    /// 
    /// Because the oddity of the built-in exception types all sharing the same physical layout
    /// (see also PythonExceptions.BaseException) some classes are defined as classes w/ their
    /// proper name and some classes are defined as PythonType fields.  When a class is defined
    /// for convenience their's also an _TypeName version which is the PythonType.
    /// </summary>
    public static partial class PythonExceptions {
        private const string _pythonExceptionKey = "PythonExceptionInfo";
        internal const string DefaultExceptionModule = "exceptions";
        
        /// <summary>
        /// Base class for all Python exception objects.
        /// 
        /// When users throw exceptions they typically throw an exception which is
        /// a subtype of this.  A mapping is maintained between Python exceptions
        /// and .NET exceptions and a corresponding .NET exception is thrown which
        /// is associated with the Python exception.  This class represents the
        /// base class for the Python exception hierarchy.  
        /// 
        /// Users can catch exceptions rooted in either hierarchy.  The hierarchy
        /// determines whether the user catches the .NET exception object or the 
        /// Python exception object.
        /// 
        /// Most built-in Python exception classes are actually instances of the BaseException
        /// class here.  This is important because in CPython the exceptions do not 
        /// add new members and therefore their layouts are compatible for multiple
        /// inheritance.  The exceptions to this rule are the classes which define 
        /// their own fields within their type, therefore altering their layout:
        ///     EnvironmentError
        ///     SyntaxError
        ///         IndentationError     (same layout as SyntaxError)
        ///         TabError             (same layout as SyntaxError)
        ///     SystemExit
        ///     UnicodeDecodeError
        ///     UnicodeEncodeError
        ///     UnicodeTranslateError
        ///     
        /// These exceptions cannot be combined in multiple inheritance, e.g.:
        ///     class foo(EnvironmentError, IndentationError): pass
        ///     
        /// fails but they can be combined with anything which is just a BaseException:
        ///     class foo(UnicodeDecodeError, SystemError): pass
        ///     
        /// Therefore the majority of the classes are just BaseException instances with a 
        /// custom PythonType object.  The specialized ones have their own .NET class
        /// which inherits from BaseException.  User defined exceptions likewise inherit
        /// from this and have their own .NET class.
        /// </summary>
        [PythonSystemType("BaseException"), DynamicBaseTypeAttribute, Serializable]
        public class BaseException : ICodeFormattable, IPythonObject, IDynamicObject, IEnumerable {
            private PythonType/*!*/ _type;          // the actual Python type of the Exception object
            private object _message;                // the message object, cached at __init__ time, not updated on args assignment
            private PythonTuple _args;              // the tuple of args provided at creation time
            private IAttributesCollection _dict;    // the dictionary for extra values, created on demand
            private System.Exception _clrException; // the cached CLR exception that is thrown

            #region Public API Surface

            public BaseException(PythonType/*!*/ type) {
                Contract.RequiresNotNull(type, "type");

                _type = type;
            }

            [StaticExtensionMethod("__new__")]
            public static object __new__(PythonType/*!*/ cls, params object[] args) {
                return Activator.CreateInstance(cls.UnderlyingSystemType, cls);
            }

            /// <summary>
            /// Initializes the Exception object with an unlimited number of arguments
            /// </summary>
            public virtual void __init__(params object[] args) {
                _args = PythonTuple.MakeTuple(args ?? new object[] { null });
                if (_args.Count == 1) {
                    _message = _args[0];
                } else {
                    _message = String.Empty;
                }
            }

            /// <summary>
            /// Returns the exception 'message' if only a single argument was provided 
            /// during creation or an empty string.
            /// </summary>
            public object message {
                get { return _message; }
                set { _message = value; }
            }

            /// <summary>
            /// Gets or sets the arguments used for creating the exception
            /// </summary>
            public object/*!*/ args {
                get {
                    return _args ?? PythonTuple.EMPTY;
                }
                set { _args = PythonTuple.Make(value); }
            }
            
            /// <summary>
            /// Returns a tuple of (type, (arg0, ..., argN)) for implementing pickling/copying
            /// </summary>
            public object/*!*/ __reduce__() {
                return PythonTuple.MakeTuple(DynamicHelpers.GetPythonType(this), args);
            }

            /// <summary>
            /// Returns a tuple of (type, (arg0, ..., argN)) for implementing pickling/copying
            /// </summary>
            public object/*!*/ __reduce_ex__(int protocol) {
                return __reduce__();
            }

            /// <summary>
            /// Gets the nth member of the args property
            /// </summary>
            public object this[int index] {
                get {
                    return ((PythonTuple)args)[index];
                }
            }

            /// <summary>
            /// Gets the CLR exception associated w/ this Python exception.  Not visible
            /// until a .NET namespace is imported.
            /// </summary>
            [PythonHidden]
            public System.Exception/*!*/ clsException {
                get {
                    return GetClrException();
                }
                internal set {
                    _clrException = value;
                }
            }

            public override string/*!*/ ToString() {
                if (_args == null) return string.Empty;
                if (_args.Count == 0) return String.Empty;
                if (_args.Count == 1) {
                    string str;
                    Extensible<string> extStr;

                    if ((str = _args[0] as string) != null) {
                        return str;
                    } else if ((extStr = _args[0] as Extensible<string>) != null) {
                        return extStr.Value;
                    }

                    return PythonOps.ToString(_args[0]);
                }

                return _args.ToString();
            }

            #endregion

            #region Member access operators

            /// <summary>
            /// Provides custom member lookup access that fallbacks to the dictionary
            /// </summary>
            [SpecialName]
            public object GetBoundMember(string name) {
                if (_dict != null) {
                    object res;
                    if (_dict.TryGetValue(SymbolTable.StringToId(name), out res)) {
                        return res;
                    }
                }

                return OperationFailed.Value;
            }

            /// <summary>
            /// Provides custom member assignment which stores values in the dictionary
            /// </summary>
            [SpecialName]
            public void SetMemberAfter(string name, object value) {
                EnsureDict();

                _dict[SymbolTable.StringToId(name)] = value;
            }

            /// <summary>
            /// Provides custom member deletion which deletes values from the dictionary
            /// or allows clearing 'message'.
            /// </summary>
            [SpecialName]
            public bool DeleteCustomMember(string name) {
                if (name == "message") {
                    _message = null;
                    return true;
                }

                if (_dict == null) return false;

                return _dict.Remove(SymbolTable.StringToId(name));
            }

            private void EnsureDict() {
                if (_dict == null) {
                    Interlocked.CompareExchange<IAttributesCollection>(ref _dict, new SymbolDictionary(), null);
                }
            }

            #endregion

            #region ICodeFormattable Members

            /// <summary>
            /// Implements __repr__ which returns the type name + the args
            /// tuple code formatted.
            /// </summary>
            string/*!*/ ICodeFormattable.ToCodeString(CodeContext context) {
                return _type.Name + ((PythonTuple)args).ToCodeString(context);
            }

            #endregion

            #region IPythonObject Members

            IAttributesCollection IPythonObject.Dict {
                get { return _dict; }
            }

            bool IPythonObject.HasDictionary {
                get { return true; }
            }

            IAttributesCollection IPythonObject.SetDict(IAttributesCollection dict) {
                Interlocked.CompareExchange<IAttributesCollection>(ref _dict, dict, null);
                return _dict;
            }

            bool IPythonObject.ReplaceDict(IAttributesCollection dict) {
                return Interlocked.CompareExchange<IAttributesCollection>(ref _dict, dict, null) == null;
            }

            PythonType IPythonObject.PythonType {
                get { return _type; }
            }

            void IPythonObject.SetPythonType(PythonType/*!*/ newType) {
                if (_type.IsSystemType || newType.IsSystemType) {
                    throw PythonOps.TypeError("__class__ assignment can only be performed on user defined types");
                }

                _type = newType;
            }

            #endregion

            #region IEnumerable Members

            IEnumerator IEnumerable.GetEnumerator() {
                foreach (object o in ((PythonTuple)args)) {
                    yield return o;
                }
            }

            #endregion

            #region Internal .NET Exception production

            /// <summary>
            /// Creates a CLR Exception for this Python exception
            /// </summary>
            internal System.Exception/*!*/ CreateClrException(string/*!*/ message) {
                return ToClrHelper(_type, message);
            }

            /// <summary>
            /// Initializes the Python exception from a .NET exception
            /// </summary>
            /// <param name="exception"></param>
            protected internal virtual void InitializeFromClr(System.Exception/*!*/ exception) {
                if (exception.Message != null) {
                    __init__(exception.Message);
                } else {
                    __init__();
                }
            }

            /// <summary>
            /// Helper to get the CLR exception associated w/ this Python exception
            /// creating it if one has not already been created.
            /// </summary>
            internal/*!*/ System.Exception GetClrException() {
                if (_clrException != null) {
                    return _clrException;
                }

                System.Exception newExcep = CreateClrException(ToString());
                AssociateException(newExcep, this);

                Interlocked.CompareExchange<System.Exception>(ref _clrException, newExcep, null);

                return _clrException;
            }

            internal System.Exception/*!*/ InitAndGetClrException(params object[] args) {
                __init__(args);

                return GetClrException();
            }

            #endregion            
        
            #region IDynamicObject Members

            LanguageContext IDynamicObject.LanguageContext {
                get { return DefaultContext.Default.LanguageContext; }
            }

            Microsoft.Scripting.Actions.StandardRule<T> IDynamicObject.GetRule<T>(Microsoft.Scripting.Actions.DynamicAction action, CodeContext context, object[] args) {
                return UserTypeOps.GetRuleHelper<T>(action, context, args);
            }

            #endregion
        }

        #region Custom Exception Code

        public partial class SyntaxError : BaseException {
            public override string ToString() {
                PythonTuple t = ((PythonTuple)args) as PythonTuple;
                if (t != null) {
                    switch (t.Count) {
                        case 0: return PythonOps.ToString(null);
                        case 1: return PythonOps.ToString(t[0]);
                        case 2:
                            string msg = t[0] as string;
                            if (msg != null) {
                                return msg;
                            }

                            goto default;
                        default: return PythonOps.ToString(t);
                    }
                }
                return String.Empty;
            }

            public override void __init__(params object[] args) {
                base.__init__(args);

                if (args != null && args.Length != 0) {
                    msg = args[0];
                    if (args.Length >= 2) {
                        PythonTuple locationInfo = PythonTuple.Make(args[1]);
                        if (locationInfo.Count != 4) {
                            throw PythonOps.IndexError("SyntaxError expected tuple with 4 arguments, got {0}", locationInfo.Count);
                        }

                        filename = locationInfo[0];
                        lineno = locationInfo[1];
                        offset = locationInfo[2];
                        text = locationInfo[3];
                    }
                }
            }
        }

#if !SILVERLIGHT
        public partial class UnicodeDecodeError : BaseException {
            protected internal override void InitializeFromClr(System.Exception/*!*/ exception) {
                DecoderFallbackException ex = exception as DecoderFallbackException;
                if (ex != null) {
                    StringBuilder sb = new StringBuilder();
                    if (ex.BytesUnknown != null) {
                        for (int i = 0; i < ex.BytesUnknown.Length; i++) {
                            sb.Append((char)ex.BytesUnknown[i]);
                        }
                    }
                    __init__("unknown", sb.ToString(), ex.Index, ex.Index + 1, "");
                } else {
                    base.InitializeFromClr(exception);
                }
            }
        }

        public partial class UnicodeEncodeError : BaseException {
            protected internal override void InitializeFromClr(System.Exception/*!*/ exception) {
                EncoderFallbackException ex = exception as EncoderFallbackException;
                if (ex != null) {
                    __init__("unknown", new string(ex.CharUnknown, 1), ex.Index, ex.Index + 1, "");
                } else {
                    base.InitializeFromClr(exception);
                }
            }
        }
#endif

        public partial class SystemExit : BaseException {
            public override void __init__(params object[] args) {
                base.__init__(args);

                if (args != null && args.Length != 0) {
                    code = message;
                }
            }
        }

        #endregion

        #region Exception translation

        internal static System.Exception CreateThrowable(PythonType type, params object[] args) {
            BaseException be;
            if (type.UnderlyingSystemType == typeof(BaseException)) {
                be = new BaseException(type);
            } else {
                be = (BaseException)Activator.CreateInstance(type.UnderlyingSystemType, type);
            }
            be.__init__(args);

            return be.GetClrException();
        }
        
        /// <summary>
        /// Creates a new throwable exception of type type where the type is an new-style exception.
        /// 
        /// Used at runtime when creating the exception from a user provided type via the raise statement.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Throwable")]
        internal static System.Exception CreateThrowableForRaise(PythonType type, object value) {
            object pyEx;

            if (PythonOps.IsInstance(value, type)) {
                pyEx = value;
            } else if (value is PythonTuple) {
                pyEx = PythonOps.CallWithArgsTuple(type, ArrayUtils.EmptyObjects, value);
            } else {
                pyEx = PythonCalls.Call(type, value);
            }

            if (PythonOps.IsInstance(pyEx, type)) {
                // overloaded __new__ can return anything, if 
                // it's the right exception type use the normal conversion...
                // If it's wrong return an ObjectException which remembers the type.
                return ((BaseException)pyEx).GetClrException();
            }

            // user returned arbitrary object from overriden __new__, let it throw...
            return new ObjectException(type, pyEx);
        }
        
        /// <summary>
        /// Creates a throwable exception of type type where the type is an OldClass.
        /// 
        /// Used at runtime when creating the exception form a user provided type that's an old class (via the raise statement).
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Throwable")]
        internal static System.Exception CreateThrowableForRaise(OldClass type, object value) {
            object pyEx;

            if (PythonOps.IsInstance(value, type)) {
                pyEx = value;
            } else if (value is PythonTuple) {
                pyEx = PythonOps.CallWithArgsTuple(type, ArrayUtils.EmptyObjects, value);
            } else {
                pyEx = PythonCalls.Call(type, value);
            }

            return new OldInstanceException((OldInstance)pyEx);
        }

        /// <summary>
        /// Returns the CLR exception associated with a Python exception
        /// creating a new exception if necessary
        /// </summary>
        internal static System.Exception ToClr(object pythonException) {
            PythonExceptions.BaseException pyExcep = pythonException as PythonExceptions.BaseException;
            if (pyExcep != null) {
                return pyExcep.GetClrException();
            }

            System.Exception res;
            OldInstance oi = pythonException as OldInstance;
            if(oi != null) {
                res = new OldInstanceException(oi);
            } else {
                // default exception message is the exception type (from Python)
                res = new System.Exception(PythonOps.ToString(pythonException));
            }

            AssociateException(res, pythonException);

            return res;
        }

        /// <summary>
        /// Given a CLR exception returns the Python exception which most closely maps to the CLR exception.
        /// </summary>
        internal static object ToPython(System.Exception/*!*/ clrException) {
            Debug.Assert(clrException != null);

            // certain Python exceptions (StringException, OldInstanceException, ObjectException)
            // expose the underlying object they're wrapping directly.
            IPythonException ipe = clrException as IPythonException;
            if (ipe != null) {
                return ipe.ToPythonException();
            }

            object res = GetAssociatedException(clrException);
            if (res == null) {
                SyntaxErrorException syntax;

                // explicit extra conversions that need a special transformation
                if ((syntax = clrException as SyntaxErrorException) != null) {
                    return SyntaxErrorToPython(syntax);
                } 

#if !SILVERLIGHT // ThreadAbortException.ExceptionState
                ThreadAbortException ta;
                if ((ta = clrException as ThreadAbortException) != null) {
                    // transform TA w/ our reason into a KeyboardInterrupt exception.
                    KeyboardInterruptException reason = ta.ExceptionState as KeyboardInterruptException;
                    if (reason != null) return ToPython(reason);
                }
#endif
                if (res == null) {
                    res = ToPythonNewStyle(clrException);
                }

                AssociateException(clrException, res);                
            }

            return res;
        }

        /// <summary>
        /// Creates a new style Python exception from the .NET exception
        /// </summary>
        private static BaseException/*!*/ ToPythonNewStyle(System.Exception/*!*/ clrException) {
            BaseException pyExcep;
            if (clrException is InvalidCastException || clrException is ArgumentNullException) {
                // explicit extra conversions outside the generated hierarchy
                pyExcep = new BaseException(TypeError);
            } else {
                // conversions from generated code (in the generated hierarchy)...
                pyExcep = ToPythonHelper(clrException);
            }

            pyExcep.InitializeFromClr(clrException);

            return pyExcep;
        }

        [Serializable]
        private class ExceptionDataWrapper 
#if !SILVERLIGHT
            : MarshalByRefObject 
#endif
        {
            private readonly object _value;

            public ExceptionDataWrapper(object value) {
                _value = value;
            }

            public object Value {
                get {
                    return _value;
                }
            }
        }

        /// <summary>
        /// Internal helper to associate a .NET exception and a Python exception.
        /// </summary>
        private static void AssociateException(System.Exception e, object exception) {
            e.Data[_pythonExceptionKey] = new ExceptionDataWrapper(exception);
            BaseException be = exception as BaseException;
            if (be != null) {
                be.clsException = e;
            }            
        }

        /// <summary>
        /// Internal helper to get the associated Python exception from a .NET exception.
        /// </summary>
        private static object GetAssociatedException(System.Exception e) {
            if (e.Data.Contains(_pythonExceptionKey)) {
                return ((ExceptionDataWrapper)e.Data[_pythonExceptionKey]).Value;
            }

            return null;
        }

        /// <summary>
        /// Converts the DLR SyntaxErrorException into a Python new-style SyntaxError instance.
        /// </summary>
        private static BaseException/*!*/ SyntaxErrorToPython(SyntaxErrorException/*!*/ e) {
            PythonExceptions.SyntaxError se;
            if (e.GetType() == typeof(IndentationException)) {
                se = new SyntaxError(IndentationError);
            } else if (e.GetType() == typeof(TabException)) {
                se = new SyntaxError(TabError);
            } else {
                se = new SyntaxError();
            }

            se.message = e.Message;

            string sourceLine = e.GetCodeLine();
            string fileName = e.GetSymbolDocumentName();
            object column = e.Column == 0 ? null : (object)e.Column;

            se.args = PythonTuple.MakeTuple(e.Message, PythonTuple.MakeTuple(fileName, e.Line, column, sourceLine));

            se.filename = fileName;
            se.lineno = e.Line;
            se.offset = column;
            se.text = sourceLine;
            se.msg = e.Message;

            AssociateException(e, se);

            return se;
        }

        private static PythonType CreateSubType(PythonType baseType, string name, string documentation) {
            return CreateSubType(baseType, name, DefaultExceptionModule, documentation);
        }

        internal static PythonType CreateSubType(PythonType baseType, string name, string module, string documentation) {
            PythonType pt = new PythonType(baseType.UnderlyingSystemType);
            PythonTypeBuilder builder = PythonTypeBuilder.GetBuilder(pt);
            
            // need __new__, __init__, and __doc__
            builder.SetName(name);
            PythonTypeSlot pts;

            // CPython actually adds a new copy for each class, we share the instances, although
            // it shouldn't really matter...
            baseType.TryLookupSlot(DefaultContext.Default, Symbols.Init, out pts);
            builder.AddSlot(Symbols.Init, pts);
            baseType.TryLookupSlot(DefaultContext.Default, Symbols.NewInst, out pts);            
            builder.AddSlot(Symbols.NewInst, pts);
            builder.IsSystemType = true;
            builder.AddBaseType(baseType);
            builder.SetResolutionOrder(Mro.Calculate(pt, new PythonType[] { baseType }));

            builder.AddSlot(Symbols.Doc, new PythonTypeValueSlot(documentation));

            builder.Finish(true);

            return pt;
        }

        internal static PythonType CreateSubType(PythonType baseType, Type concreteType, string module, string documentation) {
            PythonType myType = DynamicHelpers.GetPythonTypeFromType(concreteType);            

            PythonTypeBuilder.GetBuilder(myType).AddInitializer(delegate(PythonTypeBuilder builder) {
                builder.SetResolutionOrder(Mro.Calculate(myType, new PythonType[] { baseType }));
                builder.SetBases(new PythonType[] { baseType });
            });

            return myType;
        }

        #endregion
    }
}
