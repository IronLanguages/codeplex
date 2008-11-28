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
using Microsoft.Linq.Expressions;
using System.Reflection;
using Microsoft.Scripting;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronPython.Runtime.Binding {
    using Ast = Microsoft.Linq.Expressions.Expression;

    partial class MetaPythonType : MetaPythonObject, IPythonInvokable {

        #region IPythonInvokable Members

        public MetaObject/*!*/ Invoke(PythonInvokeBinder/*!*/ pythonInvoke, Expression/*!*/ codeContext, MetaObject/*!*/ target, MetaObject/*!*/[]/*!*/ args) {
            return InvokeWorker(pythonInvoke, args, codeContext);
        }

        #endregion

        #region MetaObject Overrides

        public override MetaObject/*!*/ BindInvokeMember(InvokeMemberBinder/*!*/ action, MetaObject/*!*/[]/*!*/ args) {
            return BindingHelpers.GenericCall(action, this, args);
        }

        public override MetaObject/*!*/ BindInvoke(InvokeBinder/*!*/ call, params MetaObject/*!*/[]/*!*/ args) {
            return InvokeWorker(call, args, BinderState.GetCodeContext(call));
        }

        #endregion

        #region Invoke Implementation

        private MetaObject/*!*/ InvokeWorker(MetaObjectBinder/*!*/ call, MetaObject/*!*/[]/*!*/ args, Expression/*!*/ codeContext) {
            if (this.NeedsDeferral()) {
                return call.Defer(ArrayUtils.Insert(this, args));
            }

            for (int i = 0; i < args.Length; i++) {
                if (args[i].NeedsDeferral()) {
                    return call.Defer(ArrayUtils.Insert(this, args));
                }
            }

            if (IsStandardDotNetType(call)) {
                return MakeStandardDotNetTypeCall(call, codeContext, args);
            }

            return MakePythonTypeCall(call, codeContext, args);
        }

        /// <summary>
        /// Creating a standard .NET type is easy - we just call it's constructor with the provided
        /// arguments.
        /// </summary>
        private MetaObject/*!*/ MakeStandardDotNetTypeCall(MetaObjectBinder/*!*/ call, Expression/*!*/ codeContext, MetaObject/*!*/[]/*!*/ args) {
            CallSignature signature = BindingHelpers.GetCallSignature(call);
            BinderState state = BinderState.GetBinderState(call);
            MethodBase[] ctors = CompilerHelpers.GetConstructors(Value.UnderlyingSystemType, state.Binder.PrivateBinding);

            if (ctors.Length > 0) {
                return state.Binder.CallMethod(
                    new ParameterBinderWithCodeContext(state.Binder, codeContext),
                    ctors,
                    args,
                    signature,
                    Restrictions.Merge(Restrictions.GetInstanceRestriction(Expression, Value))
                );
            } else {
                return new MetaObject(
                   Ast.Throw(
                       Ast.New(
                           typeof(ArgumentTypeException).GetConstructor(new Type[] { typeof(string) }),
                           Ast.Constant("Cannot create instances of " + Value.Name)
                       )
                   ),
                   Restrictions.Merge(Restrictions.GetInstanceRestriction(Expression, Value))
                );
            }
        }

        /// <summary>
        /// Creating a Python type involves calling __new__ and __init__.  We resolve them
        /// and generate calls to either the builtin funcions directly or embed sites which
        /// call the slots at runtime.
        /// </summary>
        private MetaObject/*!*/ MakePythonTypeCall(MetaObjectBinder/*!*/ call, Expression/*!*/ codeContext, MetaObject/*!*/[]/*!*/ args) {
            ValidationInfo valInfo = MakeVersionCheck();

            MetaObject self = new RestrictedMetaObject(
                AstUtils.Convert(Expression, LimitType),
                Restrictions.GetTypeRestriction(Expression, LimitType),
                Value
            );
            ArgumentValues ai = new ArgumentValues(BindingHelpers.GetCallSignature(call), self, args);
            NewAdapter newAdapter;
            InitAdapter initAdapter;

            if (TooManyArgsForDefaultNew(call, args)) {
                return MakeIncorrectArgumentsForCallError(call, ai, valInfo);
            }

            GetAdapters(ai, call, codeContext, out newAdapter, out initAdapter);
            BinderState state = BinderState.GetBinderState(call);
            
            // get the expression for calling __new__
            MetaObject createExpr = newAdapter.GetExpression(state.Binder);
            if (createExpr.Expression.Type == typeof(void)) {
                return BindingHelpers.AddDynamicTestAndDefer(
                    call,
                    createExpr,
                    args,                        
                    valInfo
                );                    
            }

            // then get the statement for calling __init__
            ParameterExpression allocatedInst = Ast.Variable(createExpr.LimitType, "newInst");
            Expression tmpRead = allocatedInst;
            MetaObject initCall = initAdapter.MakeInitCall(
                state.Binder,
                new RestrictedMetaObject(
                    AstUtils.Convert(allocatedInst, Value.UnderlyingSystemType),
                    createExpr.Restrictions
                )
            );

            List<Expression> body = new List<Expression>();
            // then get the call to __del__ if we need one
            if (HasFinalizer(call)) {
                body.Add(
                    Ast.Assign(allocatedInst, createExpr.Expression)
                );
                body.Add(
                    GetFinalizerInitialization(call, allocatedInst)
                );
            }

            // add the call to init if we need to
            if (initCall.Expression != tmpRead) {
                // init can fail but if __new__ returns a different type
                // no exception is raised.
                MetaObject initStmt = initCall;

                if (body.Count == 0) {
                    body.Add(
                        Ast.Assign(allocatedInst, createExpr.Expression)
                    );
                }

                if (!Value.UnderlyingSystemType.IsAssignableFrom(createExpr.Expression.Type)) {
                    // return type of object, we need to check the return type before calling __init__.
                    body.Add(
                        AstUtils.IfThen(
                            Ast.TypeIs(allocatedInst, Value.UnderlyingSystemType),
                            initStmt.Expression
                        )
                    );
                } else {
                    // just call the __init__ method, no type check necessary (TODO: need null check?)
                    body.Add(initStmt.Expression);
                }
            }

            Expression res;
            // and build the target from everything we have
            if (body.Count == 0) {
                res = createExpr.Expression;
            } else {
                body.Add(allocatedInst);
                res = Ast.Block(body);
            }
            res = Ast.Block(new ParameterExpression[] { allocatedInst }, res);

            return BindingHelpers.AddDynamicTestAndDefer(
                call,
                new MetaObject(
                    res,
                    self.Restrictions.Merge(initCall.Restrictions)
                ),
                ArrayUtils.Insert(this, args),
                valInfo
            );
        }


        #endregion

        #region Adapter support

        private void GetAdapters(ArgumentValues/*!*/ ai, MetaObjectBinder/*!*/ call, Expression/*!*/ codeContext, out NewAdapter/*!*/ newAdapter, out InitAdapter/*!*/ initAdapter) {
            PythonTypeSlot newInst, init;

            Value.TryResolveSlot(BinderState.GetBinderState(call).Context, Symbols.NewInst, out newInst);
            Value.TryResolveSlot(BinderState.GetBinderState(call).Context, Symbols.Init, out init);

            // these are never null because we always resolve to __new__ or __init__ somewhere.
            Assert.NotNull(newInst, init);

            newAdapter = GetNewAdapter(ai, newInst, call, codeContext);
            initAdapter = GetInitAdapter(ai, init, call, codeContext);
        }

        private InitAdapter/*!*/ GetInitAdapter(ArgumentValues/*!*/ ai, PythonTypeSlot/*!*/ init, MetaObjectBinder/*!*/ call, Expression/*!*/ codeContext) {
            BinderState state = BinderState.GetBinderState(call);
            if (IsMixedNewStyleOldStyle()) {
                return new MixedInitAdapter(ai, state, codeContext);
            } else if ((init == InstanceOps.Init && !HasFinalizer(call)) || (Value == TypeCache.PythonType && ai.Arguments.Length == 2)) {
                return new DefaultInitAdapter(ai, state, codeContext);
            } else if (init is BuiltinMethodDescriptor) {
                return new BuiltinInitAdapter(ai, ((BuiltinMethodDescriptor)init).Template, state, codeContext);
            } else if (init is BuiltinFunction) {
                return new BuiltinInitAdapter(ai, (BuiltinFunction)init, state, codeContext);
            } else {
                return new SlotInitAdapter(init, ai, state, codeContext);
            }
        }

        private NewAdapter/*!*/ GetNewAdapter(ArgumentValues/*!*/ ai, PythonTypeSlot/*!*/ newInst, MetaObjectBinder/*!*/ call, Expression/*!*/ codeContext) {
            BinderState state = BinderState.GetBinderState(call);

            if (IsMixedNewStyleOldStyle()) {
                return new MixedNewAdapter(ai, state, codeContext);
            } else if (newInst == InstanceOps.New) {
                return new DefaultNewAdapter(ai, Value, state, codeContext);
            } else if (newInst is ConstructorFunction) {
                return new ConstructorNewAdapter(ai, Value, state, codeContext);
            } else if (newInst is BuiltinFunction) {
                return new BuiltinNewAdapter(ai, Value, ((BuiltinFunction)newInst), state, codeContext);
            }

            return new NewAdapter(ai, state, codeContext);
        }

        private class CallAdapter {
            private readonly ArgumentValues/*!*/ _argInfo;
            private readonly BinderState/*!*/ _state;
            private readonly Expression/*!*/ _context;
            private Restrictions/*!*/ _restrictions;            

            public CallAdapter(ArgumentValues/*!*/ ai, BinderState/*!*/ state, Expression/*!*/ codeContext) {
                _argInfo = ai;
                _state = state;
                _restrictions = Restrictions.Empty;
                _context = codeContext;
            }

            public Restrictions/*!*/ Restrictions {
                get {
                    return _restrictions;
                }
                set {
                    _restrictions = value;
                }
            }

            protected BinderState BinderState {
                get {
                    return _state;
                }
            }

            protected Expression CodeContext {
                get {
                    return _context;
                }
            }

            protected ArgumentValues/*!*/ Arguments {
                get { return _argInfo; }
            }
        }

        private class ArgumentValues {
            public readonly MetaObject/*!*/ Self;
            public readonly MetaObject/*!*/[]/*!*/ Arguments;
            public readonly CallSignature Signature;

            public ArgumentValues(CallSignature signature, MetaObject/*!*/ self, MetaObject/*!*/[]/*!*/ args) {
                Self = self;
                Signature = signature;
                Arguments = args;
            }
        }

        #endregion

        #region __new__ adapters

        private class NewAdapter : CallAdapter {
            public NewAdapter(ArgumentValues/*!*/ ai, BinderState/*!*/ state, Expression/*!*/ codeContext)
                : base(ai, state, codeContext) {
            }

            public virtual MetaObject/*!*/ GetExpression(DefaultBinder/*!*/ binder) {
                return MakeDefaultNew(
                    binder,
                    Ast.Call(
                        typeof(PythonOps).GetMethod("PythonTypeGetMember"),
                        CodeContext,
                        AstUtils.Convert(Arguments.Self.Expression, typeof(PythonType)),
                        Ast.Constant(null),
                        AstUtils.Constant(Symbols.NewInst)
                    )
                );
            }

            protected MetaObject/*!*/ MakeDefaultNew(DefaultBinder/*!*/ binder, Expression/*!*/ function) {
                // calling theType.__new__(theType, args)
                List<Expression> args = new List<Expression>();
                args.Add(CodeContext);
                args.Add(function);

                AppendNewArgs(args);

                return new MetaObject(
                    Ast.Dynamic(
                        new PythonInvokeBinder(
                            BinderState,
                            GetDynamicNewSignature()
                        ),
                        typeof(object),
                        args.ToArray()
                    ),
                    Arguments.Self.Restrictions
                );
            }

            private void AppendNewArgs(List<Expression> args) {
                // theType
                args.Add(Arguments.Self.Expression);

                // args
                foreach (MetaObject mo in Arguments.Arguments) {
                    args.Add(mo.Expression);
                }
            }

            protected CallSignature GetDynamicNewSignature() {
                return Arguments.Signature.InsertArgument(Argument.Simple);
            }
        }

        private class DefaultNewAdapter : NewAdapter {
            private readonly PythonType/*!*/ _creating;

            public DefaultNewAdapter(ArgumentValues/*!*/ ai, PythonType/*!*/ creating, BinderState/*!*/ state, Expression/*!*/ codeContext)
                : base(ai, state, codeContext) {
                _creating = creating;
            }

            public override MetaObject/*!*/ GetExpression(DefaultBinder/*!*/ binder) {
                var mc = new ParameterBinderWithCodeContext(binder, CodeContext);
                
                if (_creating.IsSystemType) {
                    return binder.CallMethod(
                        mc,
                        _creating.UnderlyingSystemType.GetConstructors(),
                        new MetaObject[0],
                        new CallSignature(0),
                        Restrictions.Empty,
                        _creating.Name
                    );
                }

                return binder.CallMethod(
                    mc,
                    _creating.UnderlyingSystemType.GetConstructors(),
                    new MetaObject[] { Arguments.Self },
                    new CallSignature(1),
                    Restrictions.Empty,                    
                    _creating.Name
                );
            }
        }

        private class ConstructorNewAdapter : NewAdapter {
            private readonly PythonType/*!*/ _creating;

            public ConstructorNewAdapter(ArgumentValues/*!*/ ai, PythonType/*!*/ creating, BinderState/*!*/ state, Expression/*!*/ codeContext)
                : base(ai, state, codeContext) {
                _creating = creating;
            }

            public override MetaObject/*!*/ GetExpression(DefaultBinder/*!*/ binder) {
                var mc = new ParameterBinderWithCodeContext(binder, CodeContext);

                if (_creating.IsSystemType) {
                    return binder.CallMethod(
                        mc,
                        _creating.UnderlyingSystemType.GetConstructors(),
                        Arguments.Arguments,
                        Arguments.Signature,
                        Arguments.Self.Restrictions,
                        _creating.Name
                    );
                }

                return binder.CallMethod(
                    mc,
                    _creating.UnderlyingSystemType.GetConstructors(),
                    ArrayUtils.Insert(Arguments.Self, Arguments.Arguments),
                    GetDynamicNewSignature(),
                    Arguments.Self.Restrictions,
                    _creating.Name
                );
            }
        }

        private class BuiltinNewAdapter : NewAdapter {
            private readonly PythonType/*!*/ _creating;
            private readonly BuiltinFunction/*!*/ _ctor;

            public BuiltinNewAdapter(ArgumentValues/*!*/ ai, PythonType/*!*/ creating, BuiltinFunction/*!*/ ctor, BinderState/*!*/ state, Expression/*!*/ codeContext)
                : base(ai, state, codeContext) {
                _creating = creating;
                _ctor = ctor;
            }

            public override MetaObject/*!*/ GetExpression(DefaultBinder/*!*/ binder) {
                var mc = new ParameterBinderWithCodeContext(binder, CodeContext);

                CallSignature sig = Arguments.Signature.InsertArgument(new Argument(ArgumentType.Simple));
                return binder.CallMethod(
                    mc,
                    _ctor.Targets,
                    ArrayUtils.Insert(Arguments.Self, Arguments.Arguments),
                    sig,
                    _creating.Name
                );
            }
        }

        private class MixedNewAdapter : NewAdapter {
            public MixedNewAdapter(ArgumentValues/*!*/ ai, BinderState/*!*/ state, Expression/*!*/ codeContext)
                : base(ai, state, codeContext) {
            }

            public override MetaObject/*!*/ GetExpression(DefaultBinder/*!*/ binder) {
                return MakeDefaultNew(
                    binder,
                    Ast.Call(
                        typeof(PythonOps).GetMethod("GetMixedMember"),
                        CodeContext,
                        Arguments.Self.Expression,
                        Ast.Constant(null),
                        AstUtils.Constant(Symbols.NewInst)
                    )
                );
            }
        }

        #endregion

        #region __init__ adapters

        private abstract class InitAdapter : CallAdapter {
            protected InitAdapter(ArgumentValues/*!*/ ai, BinderState/*!*/ state, Expression/*!*/ codeContext)
                : base(ai, state, codeContext) {
            }

            public abstract MetaObject/*!*/ MakeInitCall(DefaultBinder/*!*/ binder, MetaObject/*!*/ createExpr);

            protected MetaObject/*!*/ MakeDefaultInit(DefaultBinder/*!*/ binder, MetaObject/*!*/ createExpr, Expression/*!*/ init) {
                List<Expression> args = new List<Expression>();
                args.Add(CodeContext);
                args.Add(init);
                foreach (MetaObject mo in Arguments.Arguments) {
                    args.Add(mo.Expression);
                }

                return new MetaObject(
                    Ast.Dynamic(
                        new PythonInvokeBinder(
                            BinderState,
                            Arguments.Signature
                        ),
                        typeof(object),
                        args
                    ),
                    Arguments.Self.Restrictions.Merge(createExpr.Restrictions)
                );
            }
        }

        private class SlotInitAdapter : InitAdapter {
            private readonly PythonTypeSlot/*!*/ _slot;
            
            public SlotInitAdapter(PythonTypeSlot/*!*/ slot, ArgumentValues/*!*/ ai, BinderState/*!*/ state, Expression/*!*/ codeContext)
                : base(ai, state, codeContext) {
                _slot = slot;
            }

            public override MetaObject/*!*/ MakeInitCall(DefaultBinder/*!*/ binder, MetaObject/*!*/ createExpr) {
                Expression init = Ast.Call(
                    typeof(PythonOps).GetMethod("GetInitSlotMember"),
                    CodeContext,
                    Ast.Convert(Arguments.Self.Expression, typeof(PythonType)),
                    Ast.Convert(AstUtils.WeakConstant(_slot), typeof(PythonTypeSlot)),
                    AstUtils.Convert(createExpr.Expression, typeof(object))
                );

                return MakeDefaultInit(binder, createExpr, init);
            }
        }

        private class DefaultInitAdapter : InitAdapter {
            public DefaultInitAdapter(ArgumentValues/*!*/ ai, BinderState/*!*/ state, Expression/*!*/ codeContext)
                : base(ai, state, codeContext) {
            }

            public override MetaObject/*!*/ MakeInitCall(DefaultBinder/*!*/ binder, MetaObject/*!*/ createExpr) {
                // default init, we can just return the value from __new__
                return createExpr;
            }
        }

        private class BuiltinInitAdapter : InitAdapter {
            private readonly BuiltinFunction/*!*/ _method;

            public BuiltinInitAdapter(ArgumentValues/*!*/ ai, BuiltinFunction/*!*/ method, BinderState/*!*/ state, Expression/*!*/ codeContext)
                : base(ai, state, codeContext) {
                _method = method;
            }

            public override MetaObject/*!*/ MakeInitCall(DefaultBinder/*!*/ binder, MetaObject/*!*/ createExpr) {
                if (_method == InstanceOps.Init.Template) {
                    // we have a default __init__, don't call it.
                    return createExpr;
                }

                return binder.CallInstanceMethod(
                    new ParameterBinderWithCodeContext(binder, CodeContext),
                    _method.Targets,
                    createExpr,
                    Arguments.Arguments,
                    Arguments.Signature,
                    Arguments.Self.Restrictions
                );
            }
        }

        private class MixedInitAdapter : InitAdapter {
            public MixedInitAdapter(ArgumentValues/*!*/ ai, BinderState/*!*/ state, Expression/*!*/ codeContext)
                : base(ai, state, codeContext) {
            }

            public override MetaObject/*!*/ MakeInitCall(DefaultBinder/*!*/ binder, MetaObject/*!*/ createExpr) {
                Expression init = Ast.Call(
                    typeof(PythonOps).GetMethod("GetMixedMember"),
                    CodeContext,
                    Ast.Convert(Arguments.Self.Expression, typeof(PythonType)),
                    AstUtils.Convert(createExpr.Expression, typeof(object)),
                    AstUtils.Constant(Symbols.Init)
                );

                return MakeDefaultInit(binder, createExpr, init);
            }
        }

        #endregion

        #region Helpers

        private MetaObject/*!*/ MakeIncorrectArgumentsForCallError(MetaObjectBinder/*!*/ call, ArgumentValues/*!*/ ai, ValidationInfo/*!*/ valInfo) {
            string message;

            if (Value.IsSystemType) {
                if (Value.UnderlyingSystemType.GetConstructors().Length == 0) {
                    // this is a type we can't create ANY instances of, give the user a half-way decent error message
                    message = "cannot create instances of " + Value.Name;
                } else {
                    message = "default __new__ does not take parameters";
                }
            } else {
                message = "default __new__ does not take parameters";
            }

            return BindingHelpers.AddDynamicTestAndDefer(
                call,
                new MetaObject(
                    Ast.Throw(
                        Ast.New(
                            typeof(ArgumentTypeException).GetConstructor(new Type[] { typeof(string) }),
                            Ast.Constant(message)
                        )
                    ),
                    GetErrorRestrictions(ai)
                ), 
                ai.Arguments,                
                valInfo
            );
        }

        private Restrictions/*!*/ GetErrorRestrictions(ArgumentValues/*!*/ ai) {
            Restrictions res = Restrict(RuntimeType).Restrictions;
            res = res.Merge(GetInstanceRestriction(ai));

            foreach (MetaObject mo in ai.Arguments) {
                if (mo.HasValue) {                    
                    res = res.Merge(mo.Restrict(mo.RuntimeType).Restrictions);
                }
            }

            return res;
        }

        private static Restrictions GetInstanceRestriction(ArgumentValues ai) {
            return Restrictions.GetInstanceRestriction(ai.Self.Expression, ai.Self.Value);
        }

        private Expression/*!*/ GetFinalizerInitialization(MetaObjectBinder/*!*/ action, ParameterExpression/*!*/ variable) {
            return Ast.Call(
                typeof(PythonOps).GetMethod("InitializeForFinalization"),
                Ast.Constant(BinderState.GetBinderState(action).Context),
                AstUtils.Convert(variable, typeof(object))
            );
        }

        private bool HasFinalizer(MetaObjectBinder/*!*/ action) {
            // only user types have finalizers...
            if (Value.IsSystemType) return false;

            PythonTypeSlot del;
            bool hasDel = Value.TryResolveSlot(BinderState.GetBinderState(action).Context, Symbols.Unassign, out del);
            return hasDel;
        }

        private bool IsMixedNewStyleOldStyle() {
            if (!Value.IsOldClass) {
                foreach (PythonType baseType in Value.ResolutionOrder) {
                    if (baseType.IsOldClass) {
                        // mixed new-style/old-style class, we can't handle
                        // __init__ in an old-style class yet (it doesn't show
                        // up in a slot).
                        return true;
                    }
                }
            }
            return false;
        }

        private bool HasDefaultNew(MetaObjectBinder/*!*/ action) {
            PythonTypeSlot newInst;
            Value.TryResolveSlot(BinderState.GetBinderState(action).Context, Symbols.NewInst, out newInst);
            return newInst == InstanceOps.New;
        }

        private bool HasDefaultInit(MetaObjectBinder/*!*/ action) {
            PythonTypeSlot init;
            Value.TryResolveSlot(BinderState.GetBinderState(action).Context, Symbols.Init, out init);
            return init == InstanceOps.Init;
        }

        private bool HasDefaultNewAndInit(MetaObjectBinder/*!*/ action) {
            return HasDefaultNew(action) && HasDefaultInit(action);
        }

        /// <summary>
        /// Checks if we have a default new and init - in this case if we have any
        /// arguments we don't allow the call.
        /// </summary>
        private bool TooManyArgsForDefaultNew(MetaObjectBinder/*!*/ action, MetaObject/*!*/[]/*!*/ args) {
            if (args.Length > 0 && HasDefaultNewAndInit(action)) {
                Argument[] infos = BindingHelpers.GetCallSignature(action).GetArgumentInfos();
                for (int i = 0; i < infos.Length; i++) {
                    Argument curArg = infos[i];

                    switch(curArg.Kind) {
                        case ArgumentType.List:
                            // Deferral?
                            if (((IList<object>)args[i].Value).Count > 0) {
                                return true;
                            }
                            break;
                        case ArgumentType.Dictionary:
                            // Deferral?
                            if (((IDictionary<object, object>)args[i].Value).Count > 0) {
                                return true;
                            }
                            break;
                        default:
                            return true;
                    }                    
                }
            }
            return false;
        }

        /// <summary>
        /// Creates a test which tests the specific version of the type.
        /// </summary>
        private ValidationInfo/*!*/ MakeVersionCheck() {
            int version = Value.Version;
            return new ValidationInfo(
                Ast.Equal(
                    Ast.Call(
                        typeof(PythonOps).GetMethod("GetTypeVersion"),
                        Ast.Convert(Expression, typeof(PythonType))
                    ),
                    Ast.Constant(version)
                ),
                new BindingHelpers.PythonTypeValidator(Value, version).Validate
            );
        }

        private bool IsStandardDotNetType(MetaObjectBinder/*!*/ action) {
            BinderState bState = BinderState.GetBinderState(action);

            return
                Value.IsSystemType &&
                !Value.IsPythonType &&
                !bState.Binder.HasExtensionTypes(Value.UnderlyingSystemType) &&
                !typeof(Delegate).IsAssignableFrom(Value.UnderlyingSystemType) &&
                !Value.UnderlyingSystemType.IsArray;                                
        }

        #endregion        
    }
}
