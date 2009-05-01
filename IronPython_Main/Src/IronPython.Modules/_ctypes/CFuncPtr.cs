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
using System.Collections.Generic;
using Microsoft.Scripting;
using Microsoft.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;

using System.Runtime.InteropServices;
using System.Threading;

using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;

using IronPython.Runtime;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

#if !SILVERLIGHT
namespace IronPython.Modules {
    /// <summary>
    /// Provides support for interop with native code from Python code.
    /// </summary>
    public static partial class CTypes {

        [PythonType("CFuncPtr")]
        public abstract class _CFuncPtr : CData, IDynamicMetaObjectProvider {
            private readonly IntPtr _addr;
            private readonly Delegate _delegate;
            private object _errcheck, _restype = _noResType, _argtypes;
            private int _id;
            private static int _curId = 0;
            internal static object _noResType = new object();
            // __nonzero__ 

            /// <summary>
            /// Creates a new CFuncPtr object from a tuple.  The 1st element of the
            /// tuple is the ordinal or function name.  The second is an object with
            /// a _handle property.  The _handle property is the handle of the module
            /// from which the function will be loaded.
            /// </summary>
            public _CFuncPtr(PythonTuple args) {
                if (args == null) {
                    throw PythonOps.TypeError("expected sequence, got None");
                } else if (args.Count != 2) {
                    throw PythonOps.TypeError("argument 1 must be a sequence of length 2, not {0}", args.Count);
                }

                object nameOrOrdinal = args[0];
                object dll = args[1];
                IntPtr intPtrHandle = GetHandleFromObject(dll, "the _handle attribute of the second element must be an integer");

                string funcName = args[0] as string;
                if (funcName != null) {
                    _addr = NativeFunctions.GetProcAddress(intPtrHandle, funcName);
                } else {
                    _addr = NativeFunctions.GetProcAddress(intPtrHandle, (int)nameOrOrdinal);
                }

                if (_addr == IntPtr.Zero) {
                    if (CallingConvention == CallingConvention.StdCall && funcName != null) {
                        // apply std call name mangling - prepend a _, append @bytes where 
                        // bytes is the number of bytes of the argument list.
                        string mangled = "_" + funcName + "@";
                        for (int i = 0; i < 128 && _addr == IntPtr.Zero; i += 4) {
                            _addr = NativeFunctions.GetProcAddress(intPtrHandle, mangled + i);
                        }
                    }

                    if (_addr == IntPtr.Zero) {
                        throw PythonOps.AttributeError("function {0} is not defined", args[0]);
                    }
                }

                _id = Interlocked.Increment(ref _curId);
            }

            public _CFuncPtr(CodeContext context, object function) {
                if (function != null) {
                    if (!PythonOps.IsCallable(context, function)) {
                        throw PythonOps.TypeError("argument must be called or address of function");
                    }

                    _delegate = ((CFuncPtrType)DynamicHelpers.GetPythonType(this)).MakeReverseDelegate(context, function);
                    _addr = Marshal.GetFunctionPointerForDelegate(_delegate);
                }
                _id = Interlocked.Increment(ref _curId);
            }


            /// <summary>
            /// Creates a new CFuncPtr with the specfied address.
            /// </summary>
            public _CFuncPtr(int handle) {
                _addr = new IntPtr(handle);
                _id = Interlocked.Increment(ref _curId);
            }

            /// <summary>
            /// Creates a new CFuncPtr with the specfied address.
            /// </summary>
            public _CFuncPtr([NotNull]BigInteger handle) {
                _addr = new IntPtr(handle.ToInt64());
                _id = Interlocked.Increment(ref _curId);
            }

            public _CFuncPtr(IntPtr handle) {
                _addr = handle;
                _id = Interlocked.Increment(ref _curId);
            }

            public bool __nonzero__() {
                return _addr != IntPtr.Zero;
            }

            #region Public APIs

            public object errcheck {
                get { return _errcheck; }
                set { _errcheck = value; }
            }

            public object restype {
                get {
                    if (_restype == _noResType) {
                        return ((CFuncPtrType)NativeType)._restype;
                    }

                    return _restype;
                }
                set {
                    INativeType nt = value as INativeType;
                    if (nt != null || value == null) {
                        _restype = nt;
                        _id = Interlocked.Increment(ref _curId);
                    } else {
                        // TODO: handle callables
                        throw PythonOps.TypeError("restype must be a type, a callable, or None");
                    }
                }
            }

            [SpecialName, PropertyMethod]
            public void Deleterestype() {
                _restype = _noResType;
                _id = Interlocked.Increment(ref _curId);
            }

            public object argtypes {
                get { return _argtypes; }
                set {
                    _argtypes = value;
                    _id = Interlocked.Increment(ref _curId);
                }
            }

            #endregion

            #region Internal APIs

            internal CallingConvention CallingConvention {
                get {
                    return ((CFuncPtrType)DynamicHelpers.GetPythonType(this)).CallingConvention;
                }
            }

            // TODO: access via PythonOps
            public IntPtr addr {
                [PythonHidden]
                get {
                    return _addr;
                }
            }

            internal int Id {
                get {
                    return _id;
                }
            }

            #endregion

            #region IDynamicObject Members

            // needs to be public so that derived base classes can call it.
            [PythonHidden]
            public DynamicMetaObject GetMetaObject(Expression parameter) {
                return new Meta(parameter, this);
            }

            #endregion

            #region MetaObject

            private class Meta : MetaPythonObject {
                public Meta(Expression parameter, _CFuncPtr func)
                    : base(parameter, BindingRestrictions.Empty, func) {
                }

                public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args) {
                    CodeContext context = BinderState.GetBinderState(binder).Context;
                    PythonContext ctx = PythonContext.GetContext(context);

                    ArgumentMarshaller[] signature = GetArgumentMarshallers(args);

                    BindingRestrictions restrictions = BindingRestrictions.GetTypeRestriction(
                        Expression,
                        Value.GetType()
                    ).Merge(
                        BindingRestrictions.GetExpressionRestriction(
                            Expression.Call(
                                typeof(ModuleOps).GetMethod("CheckFunctionId"),
                                Expression.Convert(Expression, typeof(_CFuncPtr)),
                                Expression.Constant(Value.Id)
                            )
                        )
                    );

                    foreach (var arg in signature) {
                        restrictions = restrictions.Merge(arg.GetRestrictions());
                    }

                    Expression call = MakeCall(signature, GetNativeReturnType());
                    List<Expression> block = new List<Expression>();
                    Expression res;

                    if (call.Type != typeof(void)) {
                        ParameterExpression tmp = Expression.Parameter(call.Type, "ret");
                        block.Add(Expression.Assign(tmp, call));
                        AddKeepAlives(signature, block);
                        block.Add(tmp);
                        res = Expression.Block(new[] { tmp }, block);
                    } else {
                        block.Add(call);
                        AddKeepAlives(signature, block);
                        res = Expression.Block(block);
                    }

                    return new DynamicMetaObject(Utils.Convert(res, typeof(object)), restrictions);
                }

                /// <summary>
                /// we need to keep alive any methods which have arguments for the duration of the
                /// call.  Otherwise they could be collected on the finalizer thread before we come back.
                /// </summary>
                private void AddKeepAlives(ArgumentMarshaller[] signature, List<Expression> block) {
                    foreach (ArgumentMarshaller marshaller in signature) {
                        Expression keepAlive = marshaller.GetKeepAlive();
                        if (keepAlive != null) {
                            block.Add(keepAlive);
                        }
                    }
                }

                private Expression MakeCall(ArgumentMarshaller[] signature, INativeType nativeRetType) {
                    List<object> constantPool = new List<object>();
                    MethodInfo interopInvoker = CreateInteropInvoker(
                        GetCallingConvention(),
                        signature,
                        nativeRetType,
                        constantPool
                    );

                    // build the args - IntPtr, user Args, constant pool
                    Expression[] callArgs = new Expression[signature.Length + 2];
                    callArgs[0] = Expression.Property(
                        Expression.Convert(Expression, typeof(_CFuncPtr)),
                        "addr"
                    );
                    for (int i = 0; i < signature.Length; i++) {
                        callArgs[i + 1] = signature[i].ArgumentExpression;
                    }

                    callArgs[callArgs.Length - 1] = Expression.Constant(constantPool.ToArray());

                    return Expression.Call(interopInvoker, callArgs);
                }

                private CallingConvention GetCallingConvention() {
                    return Value.CallingConvention;
                }

                private INativeType GetNativeReturnType() {
                    return Value.restype as INativeType;
                }

                private ArgumentMarshaller/*!*/[]/*!*/ GetArgumentMarshallers(DynamicMetaObject/*!*/[]/*!*/ args) {
                    CFuncPtrType funcType = ((CFuncPtrType)Value.NativeType);
                    ArgumentMarshaller[] res = new ArgumentMarshaller[args.Length];
                    for (int i = 0; i < args.Length; i++) {
                        DynamicMetaObject mo = args[i];
                        INativeType argType = null;
                        if (Value.argtypes != null) {
                            argType = (INativeType)((IList<object>)Value.argtypes)[i];
                        } else if (funcType._argtypes != null) {
                            argType = funcType._argtypes[i];
                        }

                        res[i] = GetMarshaller(mo.Expression, mo.Value, i, argType);
                    }
                    return res;
                }

                private ArgumentMarshaller/*!*/ GetMarshaller(Expression/*!*/ expr, object value, int index, INativeType nativeType) {
                    if (nativeType != null) {
                        return new CDataMarshaller(expr, CompilerHelpers.GetType(value), nativeType);
                    }

                    CData data = value as CData;
                    if (data != null) {
                        return new CDataMarshaller(expr, CompilerHelpers.GetType(value), data.NativeType);
                    }

                    NativeArgument arg = value as NativeArgument;
                    if (arg != null) {
                        return new NativeArgumentMarshaller(expr, value.GetType());
                    }

                    object val;
                    if (PythonOps.TryGetBoundAttr(value, SymbolTable.StringToId("_as_parameter_"), out val)) {
                        throw new NotImplementedException("_as_parameter");
                        //return new UserDefinedMarshaller(GetMarshaller(..., value, index));                    
                    }

                    // Marshalling primitive or an object
                    return new PrimitiveMarshaller(expr, CompilerHelpers.GetType(value));
                }

                public new _CFuncPtr/*!*/ Value {
                    get {
                        return (_CFuncPtr)base.Value;
                    }
                }

                /// <summary>
                /// Creates a method for calling with the specified signature.  The returned method has a signature
                /// of the form:
                /// 
                /// (IntPtr funcAddress, arg0, arg1, ..., object[] constantPool)
                /// 
                /// where IntPtr is the address of the function to be called.  The arguments types are based upon
                /// the types that the ArgumentMarshaller requires.
                /// </summary>
                private static MethodInfo/*!*/ CreateInteropInvoker(CallingConvention convention, ArgumentMarshaller/*!*/[]/*!*/ sig, INativeType nativeRetType, List<object> constantPool) {
                    Type[] sigTypes = new Type[sig.Length + 2];
                    sigTypes[0] = typeof(IntPtr);
                    for (int i = 0; i < sig.Length; i++) {
                        sigTypes[i + 1] = sig[i].ArgumentExpression.Type;
                    }
                    sigTypes[sigTypes.Length - 1] = typeof(object[]);

                    Type retType = nativeRetType != null ? nativeRetType.GetPythonType() : typeof(void);
                    Type calliRetType = nativeRetType != null ? nativeRetType.GetNativeType() : typeof(void);

#if !CTYPES_USE_SNIPPETS
                    DynamicMethod dm = new DynamicMethod("InteropInvoker", retType, sigTypes, DynamicModule);
#else
                    TypeGen tg = Snippets.Shared.DefineType("InteropInvoker", typeof(object), false, false);
                    MethodBuilder dm = tg.TypeBuilder.DefineMethod("InteropInvoker", CompilerHelpers.PublicStatic, retType, sigTypes);
#endif

                    ILGenerator method = dm.GetILGenerator();
                    LocalBuilder calliRetTmp = null, finalRetValue = null;
                    if (dm.ReturnType != typeof(void)) {
                        calliRetTmp = method.DeclareLocal(calliRetType);
                        finalRetValue = method.DeclareLocal(dm.ReturnType);
                    }

                    // try {
                    // emit all of the arguments, save their cleanups

                    method.BeginExceptionBlock();

                    List<MarshalCleanup> cleanups = null;
                    for (int i = 0; i < sig.Length; i++) {
                        MarshalCleanup cleanup = sig[i].EmitCallStubArgument(method, i + 1, constantPool, sigTypes.Length - 1);
                        if (cleanup != null) {
                            if (cleanups == null) {
                                cleanups = new List<MarshalCleanup>();
                            }

                            cleanups.Add(cleanup);
                        }
                    }

                    // emit the target function pointer and the calli
                    method.Emit(OpCodes.Ldarg_0);
                    method.Emit(OpCodes.Calli, GetCalliSignature(convention, sig, calliRetType));

                    // if we have a return value we need to store it and marshal to Python
                    // before we run any cleanup code.
                    if (retType != typeof(void)) {
                        method.Emit(OpCodes.Stloc, calliRetTmp);

                        nativeRetType.EmitReverseMarshalling(method, new Local(calliRetTmp), constantPool, sig.Length + 1);
                        method.Emit(OpCodes.Stloc, finalRetValue);
                    }

                    // } finally { 
                    // emit the cleanup code

                    method.BeginFinallyBlock();

                    if (cleanups != null) {
                        foreach (MarshalCleanup mc in cleanups) {
                            mc.Cleanup(method);
                        }
                    }

                    method.EndExceptionBlock();

                    // }
                    // load the temporary value and return it.
                    if (retType != typeof(void)) {
                        method.Emit(OpCodes.Ldloc, finalRetValue);
                    }

                    method.Emit(OpCodes.Ret);

#if CTYPES_USE_SNIPPETS
                    return tg.TypeBuilder.CreateType().GetMethod("InteropInvoker");
#else
                    return dm;
#endif
                }

                private static SignatureHelper GetCalliSignature(CallingConvention convention, ArgumentMarshaller/*!*/[] sig, Type calliRetType) {
                    SignatureHelper signature = SignatureHelper.GetMethodSigHelper(convention, calliRetType);

                    foreach (ArgumentMarshaller argMarshaller in sig) {
                        signature.AddArgument(argMarshaller.NativeType);
                    }

                    return signature;
                }

                #region Argument Marshalling

                /// <summary>
                /// Base class for marshalling arguments from the user provided value to the
                /// call stub.  This class provides the logic for creating the call stub and
                /// calling it.
                /// </summary>
                abstract class ArgumentMarshaller {
                    private readonly Expression/*!*/ _argExpr;

                    public ArgumentMarshaller(Expression/*!*/ container) {
                        _argExpr = container;
                    }

                    /// <summary>
                    /// Emits the IL to get the argument for the call stub generated into
                    /// a dynamic method.
                    /// </summary>
                    public abstract MarshalCleanup EmitCallStubArgument(ILGenerator/*!*/ generator, int argIndex, List<object>/*!*/ constantPool, int constantPoolArgument);

                    public abstract Type/*!*/ NativeType {
                        get;
                    }

                    /// <summary>
                    /// Gets the expression used to provide the argument.  This is the expression
                    /// from an incoming DynamicMetaObject.
                    /// </summary>
                    public Expression/*!*/ ArgumentExpression {
                        get {
                            return _argExpr;
                        }
                    }

                    /// <summary>
                    /// Gets an expression which keeps alive the argument for the duration of the call.  
                    /// 
                    /// Returns null if a keep alive is not necessary.
                    /// </summary>
                    public virtual Expression GetKeepAlive() {
                        return null;
                    }

                    public virtual BindingRestrictions GetRestrictions() {
                        return BindingRestrictions.Empty;
                    }
                }

                /// <summary>
                /// Provides marshalling of primitive values when the function type
                /// has no type information or when the user has provided us with
                /// an explicit cdata instance.
                /// </summary>
                class PrimitiveMarshaller : ArgumentMarshaller {
                    private readonly Type/*!*/ _type;

                    public PrimitiveMarshaller(Expression/*!*/ container, Type/*!*/ type)
                        : base(container) {
                        _type = type;
                    }

                    public override MarshalCleanup EmitCallStubArgument(ILGenerator/*!*/ generator, int argIndex, List<object>/*!*/ constantPool, int constantPoolArgument) {
                        if (_type == typeof(DynamicNull)) {
                            generator.Emit(OpCodes.Ldc_I4_0);
                            generator.Emit(OpCodes.Conv_I);
                            return null;
                        }

                        generator.Emit(OpCodes.Ldarg, argIndex);
                        if (ArgumentExpression.Type != _type) {
                            generator.Emit(OpCodes.Unbox_Any, _type);
                        }

                        if (_type == typeof(string)) {
                            // pin the string and convert to a wchar*.  We could let the CLR do this
                            // but we need the string to be pinned longer than the duration of the the CLR's
                            // p/invoke.  This is because the function could return the same pointer back 
                            // to us and we need to create a new string from it.
                            LocalBuilder lb = generator.DeclareLocal(typeof(string), true);
                            generator.Emit(OpCodes.Stloc, lb);
                            generator.Emit(OpCodes.Ldloc, lb);
                            generator.Emit(OpCodes.Conv_I);
                            generator.Emit(OpCodes.Ldc_I4, RuntimeHelpers.OffsetToStringData);
                            generator.Emit(OpCodes.Add);
                        } else if (_type == typeof(BigInteger)) {
                            generator.Emit(OpCodes.Call, typeof(BigInteger).GetMethod("ToInt32", Type.EmptyTypes));
                        } else if (!_type.IsValueType) {
                            generator.Emit(OpCodes.Call, typeof(CTypes).GetMethod("PyObj_ToPtr"));
                        }

                        return null;
                    }

                    public override Type NativeType {
                        get {
                            if (_type == typeof(BigInteger)) {
                                return typeof(int);
                            } else if (!_type.IsValueType) {
                                return typeof(IntPtr);
                            }

                            return _type;
                        }
                    }

                    public override BindingRestrictions GetRestrictions() {
                        if (_type == typeof(DynamicNull)) {
                            return BindingRestrictions.GetExpressionRestriction(Expression.Equal(ArgumentExpression, Expression.Constant(null)));
                        }

                        return BindingRestrictions.GetTypeRestriction(ArgumentExpression, _type);
                    }
                }

                /// <summary>
                /// Provides marshalling for when the function type provide argument information.
                /// </summary>
                class CDataMarshaller : ArgumentMarshaller {
                    private readonly Type/*!*/ _type;
                    private readonly INativeType/*!*/ _cdataType;

                    public CDataMarshaller(Expression/*!*/ container, Type/*!*/ type, INativeType/*!*/cdataType)
                        : base(container) {
                        _type = type;
                        _cdataType = cdataType;
                    }

                    public override MarshalCleanup EmitCallStubArgument(ILGenerator/*!*/ generator, int argIndex, List<object>/*!*/ constantPool, int constantPoolArgument) {
                        return _cdataType.EmitMarshalling(generator, new Arg(argIndex, ArgumentExpression.Type), constantPool, constantPoolArgument);
                    }

                    public override Type NativeType {
                        get {
                            return _cdataType.GetNativeType();
                        }
                    }

                    public override Expression GetKeepAlive() {
                        // Future possible optimization - we could just keep alive the MemoryHolder
                        if (_type.IsValueType) {
                            return null;
                        }

                        return Expression.Call(
                            typeof(GC).GetMethod("KeepAlive"),
                            ArgumentExpression
                        );
                    }

                    public override BindingRestrictions GetRestrictions() {
                        // we base this off of the type marshalling which can handle anything.
                        return BindingRestrictions.Empty;
                    }
                }

                /// <summary>
                /// Provides marshalling for when the user provides a native argument object
                /// (usually gotten by byref or pointer) and the function type has no type information.
                /// </summary>
                class NativeArgumentMarshaller : ArgumentMarshaller {
                    private readonly Type/*!*/ _type;

                    public NativeArgumentMarshaller(Expression/*!*/ container, Type/*!*/ type)
                        : base(container) {
                        _type = type;
                    }

                    public override MarshalCleanup EmitCallStubArgument(ILGenerator/*!*/ generator, int argIndex, List<object>/*!*/ constantPool, int constantPoolArgument) {
                        // We access UnsafeAddress here but ensure the object is kept 
                        // alive via the expression returned in GetKeepAlive.
                        generator.Emit(OpCodes.Ldarg, argIndex);
                        generator.Emit(OpCodes.Castclass, typeof(NativeArgument));
                        generator.Emit(OpCodes.Call, typeof(NativeArgument).GetMethod("get__obj"));
                        generator.Emit(OpCodes.Call, typeof(CData).GetMethod("get_UnsafeAddress"));
                        return null;
                    }

                    public override Type/*!*/ NativeType {
                        get {
                            return typeof(IntPtr);
                        }
                    }

                    public override Expression GetKeepAlive() {
                        // Future possible optimization - we could just keep alive the MemoryHolder
                        return Expression.Call(
                            typeof(GC).GetMethod("KeepAlive"),
                            ArgumentExpression
                        );
                    }

                    public override BindingRestrictions GetRestrictions() {
                        return BindingRestrictions.GetTypeRestriction(ArgumentExpression, typeof(NativeArgument));
                    }
                }

                /// <summary>
                /// Provides the marshalling for a user defined object which has an _as_parameter_
                /// value.
                /// </summary>
                class UserDefinedMarshaller : ArgumentMarshaller {
                    private readonly ArgumentMarshaller/*!*/ _marshaller;

                    public UserDefinedMarshaller(Expression/*!*/ container, ArgumentMarshaller/*!*/ marshaller)
                        : base(container) {
                        _marshaller = marshaller;
                    }

                    public override Type NativeType {
                        get { throw new NotImplementedException("user defined marshaller sig type"); }
                    }

                    public override MarshalCleanup EmitCallStubArgument(ILGenerator/*!*/ generator, int argIndex, List<object>/*!*/ constantPool, int constantPoolArgument) {
                        throw new NotImplementedException("user defined marshaller");
                    }
                }

                #endregion
            }

            #endregion
        }
    }
}
#endif
