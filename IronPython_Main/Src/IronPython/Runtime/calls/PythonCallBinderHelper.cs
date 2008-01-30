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
using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

namespace IronPython.Runtime.Calls {
    using Ast = Microsoft.Scripting.Ast.Ast;

    class PythonCallBinderHelper<T> : CallBinderHelper<T, CallAction> {
        private List<Type[]> _testTypes = new List<Type[]>();
        private bool _canTemplate, _altVersion;

        private static Dictionary<ShareableTemplateKey, TemplatedRuleBuilder<T>> PythonCallTemplateBuilders = new Dictionary<ShareableTemplateKey, TemplatedRuleBuilder<T>>();

        public PythonCallBinderHelper(CodeContext context, CallAction action, object[] args)
            : base(context, action, args) {
        }

        public new StandardRule<T> MakeRule() {            
            PythonType dt = Arguments[0] as PythonType;
            if (dt != null) {
                if (IsStandardDotNetType(dt)) {
                    return ((IDynamicObject)dt).GetRule<T>(MakeCreateInstanceAction(Action), Context, Arguments);
                }

                // TODO: this should move into PythonType's IDynamicObject implementation when that exists
                return MakePythonTypeCallRule(dt);
            }

            // fall back to default implementation
            return null;
        }

        private StandardRule<T> MakePythonTypeCallRule(PythonType creating) {
            ArgumentValues ai = MakeArgumentInfo();
            NewAdapter newAdapter;
            InitAdapter initAdapter;

            if (TooManyArgsForDefaultNew(creating, ai.Expressions.Length > 0)) {
                return MakeIncorrectArgumentsRule(ai, creating);
            }

            GetAdapters(creating, ai, out newAdapter, out initAdapter);

            // get the expression for calling __new__
            Expression createExpr = newAdapter.GetExpression(Binder, Rule);
            if (createExpr == null) {
                Rule.Target = newAdapter.GetError(Binder, Rule);
                MakeErrorTests(ai);
                return Rule;
            }

            // then get the statement for calling __init__
            Variable allocatedInst = Rule.GetTemporary(createExpr.Type, "newInst");
            Expression tmpRead = Ast.Read(allocatedInst);
            Expression initCall = initAdapter.MakeInitCall(Binder, Rule, tmpRead);

            List<Expression> body = new List<Expression>();

            // then get the call to __del__ if we need one
            if (HasFinalizer(creating)) {
                body.Add(
                    Ast.Statement(
                        Ast.Assign(allocatedInst, createExpr)
                    )
                );
                body.Add(
                    Ast.Statement(
                        GetFinalizerInitialization(allocatedInst)
                    )
                );
            }

            // add the call to init if we need to
            if (initCall != tmpRead) {
                // init can fail but if __new__ returns a different type
                // no exception is raised.
                Expression initStmt = initCall != null ?
                    Ast.Statement(initCall) :
                    initAdapter.GetError(Binder, Rule);

                if (body.Count == 0) {
                    body.Add(
                        Ast.Statement(
                            Ast.Assign(allocatedInst, createExpr)
                        )
                    );
                }

                if (!creating.UnderlyingSystemType.IsAssignableFrom(createExpr.Type)) {
                    // return type of object, we need to check the return type before calling __init__.
                    body.Add(
                        Ast.IfThen(
                            Ast.TypeIs(Ast.ReadDefined(allocatedInst), creating.UnderlyingSystemType),
                            initStmt
                        )
                    );
                } else {
                    // just call the __init__ method, no type check necessary (TODO: need null check?)
                    body.Add(initStmt);
                }
            }

            // and build the target from everything we have
            if (body.Count == 0) {
                // no init or del
                Rule.Target = Rule.MakeReturn(Binder, createExpr);
            } else {
                body.Add(
                    Rule.MakeReturn(Binder, Ast.Read(allocatedInst))
                );

                Rule.Target = Ast.Block(body.ToArray());
            }

            MakeTests(ai, newAdapter, initAdapter);

            if (_canTemplate) {
                CopyTemplateToRule(Context, creating, newAdapter.TemplateKey, initAdapter.TemplateKey);
            }

            return Rule;
        }

        #region Adapter support

        private void GetAdapters(PythonType creating, ArgumentValues ai, out NewAdapter newAdapter, out InitAdapter initAdapter) {
            PythonTypeSlot newInst, init;
            creating.TryResolveSlot(Context, Symbols.NewInst, out newInst);
            creating.TryResolveSlot(Context, Symbols.Init, out init);

            newAdapter = GetNewAdapter(ai, creating, newInst);
            initAdapter = GetInitAdapter(ai, creating, init);
            _canTemplate = newAdapter.TemplateKey != null && initAdapter.TemplateKey != null;
        }

        private InitAdapter GetInitAdapter(ArgumentValues ai, PythonType creating, PythonTypeSlot init) {
            if (IsMixedNewStyleOldStyle(creating)) {
                return new MixedInitAdapter(ai, Action);
            } else if ((init == InstanceOps.Init && !HasFinalizer(creating)) || (creating == TypeCache.PythonType && Arguments.Length == 2)) {
                return new DefaultInitAdapter(ai, Action);
            } else if (init is BuiltinMethodDescriptor) {
                return new BuiltinInitAdapter(ai, Action, ((BuiltinMethodDescriptor)init).Template);
            } else if (init is BuiltinFunction) {
                return new BuiltinInitAdapter(ai, Action, (BuiltinFunction)init);
            } else {
                return new InitAdapter(ai, Action);
            }
        }

        private NewAdapter GetNewAdapter(ArgumentValues ai, PythonType creating, PythonTypeSlot newInst) {
            if (IsMixedNewStyleOldStyle(creating)) {
                return new MixedNewAdapter(ai, Action);
            } else if (newInst == InstanceOps.New) {
                return new DefaultNewAdapter(ai, Action, creating);
            } else if (newInst is ConstructorFunction) {
                return new ConstructorNewAdapter(ai, Action, ((ConstructorFunction)newInst));
            } else if (newInst is BuiltinFunction) {
                return new BuiltinNewAdapter(ai, Action, ((BuiltinFunction)newInst));
            }

            return new NewAdapter(ai, Action);
        }

        private class CallAdapter {
            private Type[] _testTypes;
            private CallAction _action;
            private ArgumentValues _argInfo;

            public CallAdapter(ArgumentValues ai, CallAction action) {
                _action = action;
                _argInfo = ai;
            }

            public Type[] TestTypes {
                get { return _testTypes; }
                set { _testTypes = value; }
            }

            protected CallAction Action {
                get { return _action; }
            }

            protected ArgumentValues Arguments {
                get { return _argInfo; }
            }

            /// <summary>
            /// Returns the key to use for templating or null if templating isn't support.
            /// 
            /// Typical return value is a value type or reference type which has value equality
            /// semantics to indicate whether or not this is compatible with another templated rule.
            /// </summary>
            public virtual object TemplateKey {
                get {
                    return null;
                }
            }
        }

        private class ArgumentValues {
            /// <summary>
            /// The actual arguments passed for the call, including the callable object.
            /// </summary>
            public object[] Arguments;
            /// <summary>
            /// The expression arguments for the call, expanded to include params / dict params arguments.  Excludes the callable object.
            /// </summary>
            public Expression[] Expressions;
            /// <summary>
            /// The names of named arguments, expanded to include dictionary params arguments.
            /// </summary>
            public SymbolId[] Names;
            /// <summary>
            /// The types of the arguments excluding the callable object.  This is expanded to include the types in a
            /// params array and params dictionary as well.
            /// </summary>
            public Type[] Types;

            public ArgumentValues(object[] args, Expression[] argExprs, SymbolId[] names, Type[] types) {
                Arguments = args;
                Expressions = argExprs;
                Names = names;
                Types = types;
            }
        }

        #endregion

        #region __new__ adapters

        private class NewAdapter : CallAdapter {
            public NewAdapter(ArgumentValues ai, CallAction action)
                : base(ai, action) {
            }

            public virtual Expression GetExpression(ActionBinder binder, StandardRule<T> rule) {
                return Ast.Action.Call(
                        GetDynamicNewAction(),
                        typeof(object),
                        ArrayUtils.Insert<Expression>(
                            Ast.Call(
                                Ast.Convert(rule.Parameters[0], typeof(PythonType)),
                                typeof(PythonType).GetMethod("GetMember"),
                                Ast.CodeContext(),
                                Ast.Null(),
                                Ast.Constant(Symbols.NewInst)
                            ),                        
                            rule.Parameters
                        )
                    );
            }

            public virtual Expression GetError(ActionBinder binder, StandardRule<T> rule) {
                throw new InvalidOperationException();
            }

            protected CallAction GetDynamicNewAction() {
                return CallAction.Make(Action.Signature.InsertArgument(ArgumentInfo.Simple));
            }

            public override object TemplateKey {
                get {
                    // we can template
                    return 1;
                }
            }
        }

        private class DefaultNewAdapter : NewAdapter {
            private PythonType _creating;

            public DefaultNewAdapter(ArgumentValues ai, CallAction action, PythonType creating)
                : base(ai, action) {
                _creating = creating;
            }

            public override Expression GetExpression(ActionBinder binder, StandardRule<T> rule) {
                BindingTarget target = _creating.IsSystemType ?
                    GetTypeConstructor(binder, _creating, ArrayUtils.EmptyTypes) :
                    GetTypeConstructor(binder, _creating, new Type[] { typeof(PythonType) });

                Debug.Assert(target.Success);
                return target.MakeExpression(
                    rule,
                    _creating.IsSystemType ? new Expression[0] : new Expression[] { rule.Parameters[0] }
                );
            }

            public override object TemplateKey {
                get {
                    // we can template
                    return RuntimeHelpers.True;
                }
            }
        }

        private class ConstructorNewAdapter : NewAdapter {
            private PythonType _creating;
            private ConstructorFunction _ctor;
            private BindingTarget _target;

            public ConstructorNewAdapter(ArgumentValues ai, CallAction action, ConstructorFunction ctor)
                : base(ai, action) {
                _creating = (PythonType)ai.Arguments[0];
                _ctor = ctor;
            }

            public override Expression GetExpression(ActionBinder binder, StandardRule<T> rule) {
                Type[] types = _creating.IsSystemType ? Arguments.Types : ArrayUtils.Insert(typeof(PythonType), Arguments.Types);

                MethodBinder mb = GetMethodBinder(binder);
                _target = mb.MakeBindingTarget(CallType.None, types);

                if (_target.Success) {
                    if (_target.ArgumentTests != null) {
                        TestTypes = _creating.IsSystemType ? ArrayUtils.ToArray(_target.ArgumentTests) :
                            ArrayUtils.RemoveFirst(ArrayUtils.ToArray(_target.ArgumentTests));
                    }
                    return _target.MakeExpression(
                        rule,
                        _creating.IsSystemType ? Arguments.Expressions : ArrayUtils.Insert(rule.Parameters[0], Arguments.Expressions));
                }
                return null;
            }

            private MethodBinder GetMethodBinder(ActionBinder binder) {
                return MethodBinder.MakeBinder(binder, PythonTypeOps.GetName(_creating), _creating.UnderlyingSystemType.GetConstructors(), Arguments.Names);
            }

            public override Expression GetError(ActionBinder binder, StandardRule<T> rule) {
                return binder.MakeInvalidParametersError(_target).MakeErrorForRule(rule, binder);
            }

            public override object TemplateKey {
                get {
                    // we can't template
                    return null;
                }
            }
        }

        private class BuiltinNewAdapter : NewAdapter {
            private PythonType _creating;
            private BuiltinFunction _ctor;
            private BindingTarget _target;

            public BuiltinNewAdapter(ArgumentValues ai, CallAction action, BuiltinFunction ctor)
                : base(ai, action) {
                _creating = (PythonType)ai.Arguments[0];
                _ctor = ctor;
            }

            public override Expression GetExpression(ActionBinder binder, StandardRule<T> rule) {
                Type[] types = ArrayUtils.Insert(typeof(PythonType), Arguments.Types);

                MethodBinder mb = GetMethodBinder(binder);

                _target = mb.MakeBindingTarget(CallType.None, types);
                if (_target.Success) {
                    Expression[] parameters = ArrayUtils.Insert(rule.Parameters[0], Arguments.Expressions);
                    if (_target.ArgumentTests != null) {
                        TestTypes = ArrayUtils.RemoveFirst(ArrayUtils.ToArray(_target.ArgumentTests));
                    }
                    return _target.MakeExpression(rule, parameters);
                }
                return null;

            }

            public override Expression GetError(ActionBinder binder, StandardRule<T> rule) {
                return binder.MakeInvalidParametersError(_target).MakeErrorForRule(rule, binder);
            }

            private MethodBinder GetMethodBinder(ActionBinder binder) {
                return MethodBinder.MakeBinder(binder, PythonTypeOps.GetName(_creating), _ctor.Targets, Arguments.Names);
            }

            public override object TemplateKey {
                get {
                    // we can't template
                    return null;
                }
            }
        }

        private class MixedNewAdapter : NewAdapter {
            public MixedNewAdapter(ArgumentValues ai, CallAction action)
                : base(ai, action) {
            }


            public override Expression GetExpression(ActionBinder binder, StandardRule<T> rule) {
                return Ast.Action.Call(
                        GetDynamicNewAction(),
                        typeof(object),
                        ArrayUtils.Insert<Expression>(
                            Ast.Call(
                                typeof(PythonOps).GetMethod("GetMixedMember"),
                                Ast.CodeContext(),
                                Ast.Convert(rule.Parameters[0], typeof(PythonType)),
                                Ast.Constant(null),
                                Ast.Constant(Symbols.NewInst)
                            ),
                            rule.Parameters
                        )
                    );
            }
        }

        #endregion

        #region __init__ adapters

        private class InitAdapter : CallAdapter {
            public InitAdapter(ArgumentValues ai, CallAction action)
                : base(ai, action) {
            }

            public virtual Expression MakeInitCall(ActionBinder binder, StandardRule<T> rule, Expression createExpr) {
                return
                    Ast.Action.Call(
                        Action,
                        typeof(object),
                        ArrayUtils.Insert<Expression>(
                            Ast.Call(
                                typeof(PythonOps).GetMethod("GetInitMember"),
                                Ast.CodeContext(),
                                Ast.Convert(rule.Parameters[0], typeof(PythonType)),
                                Ast.ConvertHelper(createExpr, typeof(object))
                            ),
                            ArrayUtils.RemoveFirst(rule.Parameters)
                        )
                    );
            }

            public virtual Expression GetError(ActionBinder binder, StandardRule<T> rule) {
                throw new InvalidOperationException();
            }

            public override object TemplateKey {
                get {
                    // we can template
                    return 1;
                }
            }
        }

        private class DefaultInitAdapter : InitAdapter {
            public DefaultInitAdapter(ArgumentValues ai, CallAction action)
                : base(ai, action) {
            }

            public override Expression MakeInitCall(ActionBinder binder, StandardRule<T> rule, Expression createExpr) {
                // default init, we can just return the value from __new__
                return createExpr;
            }

            public override object TemplateKey {
                get {
                    // we can template
                    return RuntimeHelpers.True;
                }
            }
        }

        private class BuiltinInitAdapter : InitAdapter {
            private BuiltinFunction _method;
            private PythonType _creating;
            private BindingTarget _target;

            public BuiltinInitAdapter(ArgumentValues ai, CallAction action, BuiltinFunction method)
                : base(ai, action) {
                _method = method;
                _creating = (PythonType)ai.Arguments[0];
            }

            public override Expression MakeInitCall(ActionBinder binder, StandardRule<T> rule, Expression createExpr) {
                if (_method == InstanceOps.Init.Template) {
                    // we have a default __init__, don't call it.
                    return createExpr;
                }

                MethodBinder mb = GetMethodBinder(binder);
                _target = mb.MakeBindingTarget(CallType.None, ArrayUtils.Insert(_creating.UnderlyingSystemType, Arguments.Types));

                if (_target.Success) {
                    if (_target.ArgumentTests != null) {
                        TestTypes = ArrayUtils.RemoveFirst(ArrayUtils.MakeArray(_target.ArgumentTests, 0, 0));
                    }

                    return _target.MakeExpression(
                            rule,
                            ArrayUtils.Insert<Expression>(createExpr, Arguments.Expressions)
                        );

                } 

                return null;
            }

            public override Expression GetError(ActionBinder binder, StandardRule<T> rule) {
                return binder.MakeInvalidParametersError(_target).MakeErrorForRule(rule, binder);
            }

            private MethodBinder GetMethodBinder(ActionBinder binder) {
                return MethodBinder.MakeBinder(binder, "__init__", _method.Targets, Arguments.Names);
            }

            public override object TemplateKey {
                get {
                    // we can't template
                    return null;
                }
            }
        }

        private class MixedInitAdapter : InitAdapter {
            public MixedInitAdapter(ArgumentValues ai, CallAction action)
                : base(ai, action) {
            }

            public override Expression MakeInitCall(ActionBinder binder, StandardRule<T> rule, Expression createExpr) {
                return
                    Ast.Action.Call(
                        Action,
                        typeof(object),
                        ArrayUtils.Insert<Expression>(
                            Ast.Call(
                                typeof(PythonOps).GetMethod("GetMixedMember"),
                                Ast.CodeContext(),
                                Ast.Convert(rule.Parameters[0], typeof(PythonType)),
                                Ast.ConvertHelper(createExpr, typeof(object)),
                                Ast.Constant(Symbols.Init)
                            ),
                            ArrayUtils.RemoveFirst(rule.Parameters)
                        )
                    );
            }
        }

        #endregion

        #region Misc. Helpers

        private StandardRule<T> MakeIncorrectArgumentsRule(ArgumentValues ai, PythonType creating) {
            if (creating.IsSystemType && creating.UnderlyingSystemType.GetConstructors().Length == 0) {
                // this is a type we can't create ANY instances of, give the user a half-way decent error message
                Rule.Target = 
                    Rule.MakeError(
                        Ast.New(
                            typeof(ArgumentTypeException).GetConstructor(new Type[] { typeof(string) }),
                            Ast.Constant("cannot create instances of " + PythonTypeOps.GetName(creating))
                        )
                    );
            } else {
                Rule.Target = 
                    Rule.MakeError(
                        Ast.New(
                            typeof(ArgumentTypeException).GetConstructor(new Type[] { typeof(string) }),
                            Ast.Constant("default __new__ does not take parameters")
                        )
                    );
            }
            MakeErrorTests(ai);
            return Rule;
        }

        private void MakeTests(ArgumentValues ai, NewAdapter newAdapter, InitAdapter initAdapter) {
            MakeSplatTests();

            Rule.Test = Ast.AndAlso(
                Ast.AndAlso(
                    Test,
                    MakeNecessaryTests(Rule, new Type[][] { newAdapter.TestTypes, initAdapter.TestTypes }, ai.Expressions)
                ),
                MakeTypeTestForCreateInstance((PythonType)Arguments[0], Rule)
            );
        }

        private void MakeErrorTests(ArgumentValues ai) {
            MakeSplatTests();
            Rule.Test = Ast.AndAlso(
                Ast.AndAlso(
                    Test,
                    MakeNecessaryTests(Rule, new Type[][] { ai.Types }, ai.Expressions)
                ),
                MakeTypeTestForCreateInstance((PythonType)Arguments[0], Rule)
            );
        }

        internal static CreateInstanceAction MakeCreateInstanceAction(CallAction action) {
            return CreateInstanceAction.Make(new CallSignature(action.Signature));
        }


        private bool IsStandardDotNetType(PythonType type) {
            return type.IsSystemType &&
                !PythonTypeCustomizer.IsPythonType(type.UnderlyingSystemType) &&            
                !type.UnderlyingSystemType.IsDefined(typeof(PythonSystemTypeAttribute), true) &&
                !typeof(Delegate).IsAssignableFrom(type.UnderlyingSystemType) &&
                !type.UnderlyingSystemType.IsArray;
        }

        private Expression GetFinalizerInitialization(Variable variable) {
            return Ast.Call(
                typeof(PythonOps).GetMethod("InitializeForFinalization"),
                Ast.ConvertHelper(Ast.ReadDefined(variable), typeof(object))
            );
        }

        private bool HasFinalizer(PythonType creating) {
            PythonTypeSlot del;
            bool hasDel = creating.TryResolveSlot(Context, Symbols.Unassign, out del);
            return hasDel;
        }

        private static bool IsMixedNewStyleOldStyle(PythonType dt) {
            if (!Mro.IsOldStyle(dt)) {
                foreach (PythonType baseType in dt.ResolutionOrder) {
                    if (Mro.IsOldStyle(baseType)) {
                        // mixed new-style/old-style class, we can't handle
                        // __init__ in an old-style class yet (it doesn't show
                        // up in a slot).
                        return true;
                    }
                }
            }
            return false;
        }

        private bool HasDefaultNew(PythonType creating) {
            PythonTypeSlot newInst;
            creating.TryResolveSlot(Context, Symbols.NewInst, out newInst);
            return newInst == InstanceOps.New;
        }

        private bool HasDefaultInit(PythonType creating) {
            PythonTypeSlot init;
            creating.TryResolveSlot(Context, Symbols.Init, out init);
            return init == InstanceOps.Init;
        }

        private bool HasDefaultNewAndInit(PythonType creating) {
            return HasDefaultNew(creating) && HasDefaultInit(creating);
        }

        /// <summary>
        /// Checks if we have a default new and init - in this case if we have any
        /// arguments we don't allow the call.
        /// </summary>
        private bool TooManyArgsForDefaultNew(PythonType creating, bool hasArgs) {
            if (hasArgs) {
                return HasDefaultNewAndInit(creating);
            }
            return false;
        }

        /// <summary>
        /// Generates an expression which calls a .NET constructor directly.
        /// </summary>
        public static BindingTarget GetTypeConstructor(ActionBinder binder, PythonType creating, Type[] argTypes) {
            // type has no __new__ override, call .NET ctor directly
            MethodBinder mb = MethodBinder.MakeBinder(binder,
                PythonTypeOps.GetName(creating),
                creating.UnderlyingSystemType.GetConstructors());

            BindingTarget target = mb.MakeBindingTarget(CallType.None, argTypes);
            if (target.Success && target.Method.IsPublic) {
                return target;
            }
            return null;
        }

        /// <summary>
        /// Creates a test which tests the specific version of the type.
        /// </summary>
        public Expression MakeTypeTestForCreateInstance(PythonType creating, StandardRule<T> rule) {
            _altVersion = creating.Version == PythonType.DynamicVersion;
            string vername = _altVersion ? "GetAlternateTypeVersion" : "GetTypeVersion";
            int version = _altVersion ? creating.AlternateVersion : creating.Version;

            Expression versionExpr;
            if (_canTemplate) {
                versionExpr = rule.AddTemplatedConstant(typeof(int), version);
            } else {
                versionExpr = Ast.Constant(version);
            }

            return Ast.Equal(
                Ast.Call(
                    typeof(PythonOps).GetMethod(vername),
                    Ast.Convert(rule.Parameters[0], typeof(PythonType))
                ),
                versionExpr
            );
        }

        private class ShareableTemplateKey : IEquatable<ShareableTemplateKey> {
            private Type _type;
            private bool _altVersion;
            private DynamicAction _action;
            private object[] _args;

            public ShareableTemplateKey(DynamicAction action, Type type, bool altVersion, params object[] args) {
                _type = type;
                _altVersion = altVersion;
                _args = args;
                _action = action;
            }

            public override bool Equals(object obj) {
                ShareableTemplateKey other = obj as ShareableTemplateKey;
                if (other == null) return false;

                return Equals(other);
            }

            public override int GetHashCode() {
                int res = _type.GetHashCode() ^ _altVersion.GetHashCode() ^ _action.GetHashCode();
                foreach (object o in _args) {
                    res ^= o.GetHashCode();
                }
                return res;
            }

            #region IEquatable<ShareableTemplateKey> Members

            public bool Equals(ShareableTemplateKey other) {
                if (other._action == _action &&
                    other._type == _type &&
                    other._altVersion == _altVersion) {
                    if (_args.Length == other._args.Length) {
                        for (int i = 0; i < _args.Length; i++) {
                            if (!_args[i].Equals(other._args[i])) {
                                return false;
                            }
                        }
                    }
                    return true;
                }

                return false;
            }

            #endregion
        }

        private void CopyTemplateToRule(CodeContext context, PythonType t, params object[] templateData) {
            TemplatedRuleBuilder<T> builder;

            lock (PythonCallTemplateBuilders) {
                ShareableTemplateKey key =
                    new ShareableTemplateKey(Action,
                    t.UnderlyingSystemType,
                    t.Version == PythonType.DynamicVersion,
                    ArrayUtils.Insert<object>(HasFinalizer(t), templateData));
                if (!PythonCallTemplateBuilders.TryGetValue(key, out builder)) {
                    PythonCallTemplateBuilders[key] = Rule.GetTemplateBuilder();
                    return;
                }
            }

            builder.CopyTemplateToRule(context, Rule);
        }

        private ArgumentValues MakeArgumentInfo() {
            SymbolId[] names;
            Type[] argumentTypes;
            Expression[] argExpr = MakeArgumentExpressions();
            GetArgumentNamesAndTypes(out names, out argumentTypes);
            ArgumentValues ai = new ArgumentValues(Arguments, argExpr, names, argumentTypes);
            return ai;
        }

        #endregion
    }
}
