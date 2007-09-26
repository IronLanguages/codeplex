/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
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

using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Types;
using Microsoft.Scripting.Utils;

using IronPython.Runtime.Types;
using IronPython.Runtime.Operations;

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
            DynamicType dt = Arguments[0] as DynamicType;
            if (dt != null) {
                if (IsStandardDotNetType(dt)) {
                    return ((IDynamicObject)dt).GetRule<T>(MakeCreateInstanceAction(Action), Context, Arguments);
                }

                // TODO: this should move into DynamicType's IDynamicObject implementation when that exists
                return MakePythonTypeCallRule(dt);
            }

            // fall back to default implementation
            return null;
        }

        private StandardRule<T> MakePythonTypeCallRule(DynamicType creating) {
            List<Expression> body = new List<Expression>();
            ArgumentValues ai = MakeArgumentInfo();
            NewAdapter newAdapter;
            InitAdapter initAdapter;

            if (TooManyArgsForDefaultNew(creating, ai.Expressions.Length > 0)) {
                return MakeIncorrectArgumentsRule(ai);
            }

            GetAdapters(creating, ai, out newAdapter, out initAdapter);

            // get the expression for calling __new__
            Expression createExpr = newAdapter.GetExpression(Binder, Rule);
            if (createExpr == null) {
                Rule.SetTarget(newAdapter.GetError(Binder, Rule));
                MakeErrorTests(ai);
                return Rule;
            }

            // then get the statement for calling __init__
            Variable allocatedInst = Rule.GetTemporary(createExpr.Type, "newInst");
            Expression tmpRead = Ast.Read(allocatedInst);
            Expression initCall = initAdapter.MakeInitCall(Binder, Rule, tmpRead);
            if (initCall == null) {
                // init can fail but if __new__ returns a different type
                // no exception is raised.
                initCall = Ast.Void(initAdapter.GetError(Binder, Rule));
            }

            // then get the call to __del__ if we need one
            if (HasFinalizer(creating)) {
                body.Add(Ast.Assign(allocatedInst, createExpr));
                body.Add(GetFinalizerInitialization(allocatedInst));
            }

            // add the call to init if we need to
            if (initCall != tmpRead) {
                if (body.Count == 0) body.Add(Ast.Assign(allocatedInst, createExpr));

                if (!creating.UnderlyingSystemType.IsAssignableFrom(createExpr.Type)) {
                    // return type of object, we need to check the return type before calling __init__.
                    body.Add(
                        Ast.Condition(
                            Ast.TypeIs(Ast.ReadDefined(allocatedInst), creating.UnderlyingSystemType),
                            initCall,
                            Ast.Null()
                        )
                    );
                } else {
                    // just call the __init__ method, no type check necessary (TODO: need null check?)
                    body.Add(initCall);
                }
            }

            // and build the target from everything we have
            if (body.Count == 0) {
                // no init or del
                Rule.SetTarget(Rule.MakeReturn(Binder, createExpr));
            } else {
                body.Add(Ast.Read(allocatedInst));
                Rule.SetTarget(Rule.MakeReturn(Binder, Ast.Comma(body.ToArray())));
            }

            MakeTests(ai, newAdapter, initAdapter);

            if (_canTemplate) {
                CopyTemplateToRule(Context, creating, newAdapter.TemplateKey, initAdapter.TemplateKey);
            }

            return Rule;
        }

        #region Adapter support

        private void GetAdapters(DynamicType creating, ArgumentValues ai, out NewAdapter newAdapter, out InitAdapter initAdapter) {
            DynamicTypeSlot newInst, init;
            creating.TryResolveSlot(Context, Symbols.NewInst, out newInst);
            creating.TryResolveSlot(Context, Symbols.Init, out init);

            newAdapter = GetNewAdapter(ai, creating, newInst);
            initAdapter = GetInitAdapter(ai, creating, init);
            _canTemplate = newAdapter.TemplateKey != null && initAdapter.TemplateKey != null;
        }

        private InitAdapter GetInitAdapter(ArgumentValues ai, DynamicType creating, DynamicTypeSlot init) {
            if (IsMixedNewStyleOldStyle(creating)) {
                return new MixedInitAdapter(ai, Action);
            } else if ((init == InstanceOps.Init && !HasFinalizer(creating)) || (creating == TypeCache.DynamicType && Arguments.Length == 2)) {
                return new DefaultInitAdapter(ai, Action);
            } else if (init is BuiltinMethodDescriptor) {
                return new BuiltinInitAdapter(ai, Action, ((BuiltinMethodDescriptor)init).Template);
            } else if (init is BuiltinFunction) {
                return new BuiltinInitAdapter(ai, Action, (BuiltinFunction)init);
            } else {
                return new InitAdapter(ai, Action);
            }
        }

        private NewAdapter GetNewAdapter(ArgumentValues ai, DynamicType creating, DynamicTypeSlot newInst) {
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
                                Ast.Cast(rule.Parameters[0], typeof(DynamicMixin)),
                                typeof(DynamicMixin).GetMethod("GetMember"),
                                Ast.CodeContext(),
                                Ast.Null(),
                                Ast.Constant(Symbols.NewInst)
                            ),                        
                            rule.Parameters
                        )
                    );
            }

            public virtual Statement GetError(ActionBinder binder, StandardRule<T> rule) {
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
            private DynamicType _creating;

            public DefaultNewAdapter(ArgumentValues ai, CallAction action, DynamicType creating)
                : base(ai, action) {
                _creating = creating;
            }

            public override Expression GetExpression(ActionBinder binder, StandardRule<T> rule) {
                MethodCandidate cand = _creating.IsSystemType ?
                    GetTypeConstructor(binder, _creating, ArrayUtils.EmptyTypes) :
                    GetTypeConstructor(binder, _creating, new Type[] { typeof(DynamicType) });

                Debug.Assert(cand != null);
                return cand.Target.MakeExpression(binder,
                    rule,
                    _creating.IsSystemType ? new Expression[0] : new Expression[] { rule.Parameters[0] },
                    _creating.IsSystemType ? Type.EmptyTypes : new Type[] { typeof(DynamicType) });
            }

            public override object TemplateKey {
                get {
                    // we can template
                    return RuntimeHelpers.True;
                }
            }
        }

        private class ConstructorNewAdapter : NewAdapter {
            private DynamicType _creating;
            private ConstructorFunction _ctor;

            public ConstructorNewAdapter(ArgumentValues ai, CallAction action, ConstructorFunction ctor)
                : base(ai, action) {
                _creating = (DynamicType)ai.Arguments[0];
                _ctor = ctor;
            }

            public override Expression GetExpression(ActionBinder binder, StandardRule<T> rule) {
                Type[] types = _creating.IsSystemType ? Arguments.Types : ArrayUtils.Insert(typeof(DynamicType), Arguments.Types);

                MethodBinder mb = GetMethodBinder(binder);
                MethodCandidate cand = mb.MakeBindingTarget(CallType.None, types);

                if (cand != null) {
                    return cand.Target.MakeExpression(binder,
                        rule,
                        _creating.IsSystemType ? Arguments.Expressions : ArrayUtils.Insert(rule.Parameters[0], Arguments.Expressions));
                }
                return null;
            }

            private MethodBinder GetMethodBinder(ActionBinder binder) {
                return MethodBinder.MakeBinder(binder, DynamicTypeOps.GetName(_creating), _creating.UnderlyingSystemType.GetConstructors(), BinderType.Normal, Arguments.Names);
            }

            public override Statement GetError(ActionBinder binder, StandardRule<T> rule) {
                MethodBinder mb = GetMethodBinder(binder);
                return binder.MakeInvalidParametersError(mb, Action, CallType.None, _creating.UnderlyingSystemType.GetConstructors(), rule, Arguments.Arguments);
            }

            public override object TemplateKey {
                get {
                    // we can't template
                    return null;
                }
            }
        }

        private class BuiltinNewAdapter : NewAdapter {
            private DynamicType _creating;
            private BuiltinFunction _ctor;

            public BuiltinNewAdapter(ArgumentValues ai, CallAction action, BuiltinFunction ctor)
                : base(ai, action) {
                _creating = (DynamicType)ai.Arguments[0];
                _ctor = ctor;
            }

            public override Expression GetExpression(ActionBinder binder, StandardRule<T> rule) {
                Type[] types = ArrayUtils.Insert(typeof(DynamicType), Arguments.Types);

                MethodBinder mb = GetMethodBinder(binder);

                Type[] testTypes;
                MethodCandidate mc = mb.MakeBindingTarget(CallType.None, types, out testTypes);
                Expression[] parameters = ArrayUtils.Insert(rule.Parameters[0], Arguments.Expressions);
                if (mc != null) {
                    if (testTypes != null) TestTypes = ArrayUtils.RemoveFirst(testTypes);
                    return mc.Target.MakeExpression(binder, rule, parameters);
                }
                return null;

            }

            public override Statement GetError(ActionBinder binder, StandardRule<T> rule) {
                MethodBinder mb = GetMethodBinder(binder);
                return binder.MakeInvalidParametersError(mb, Action, CallType.None, _ctor.Targets, rule, Arguments.Arguments);
            }

            private MethodBinder GetMethodBinder(ActionBinder binder) {
                return MethodBinder.MakeBinder(binder, DynamicTypeOps.GetName(_creating), _ctor.Targets, BinderType.Normal, Arguments.Names);
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
                                null,
                                typeof(PythonOps).GetMethod("GetMixedMember"),
                                Ast.CodeContext(),
                                Ast.Cast(rule.Parameters[0], typeof(DynamicMixin)),
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
                                null,
                                typeof(PythonOps).GetMethod("GetInitMember"),
                                Ast.CodeContext(),
                                Ast.Cast(rule.Parameters[0], typeof(DynamicMixin)),
                                createExpr
                            ),
                            ArrayUtils.RemoveFirst(rule.Parameters)
                        )
                    );
            }

            public virtual Statement GetError(ActionBinder binder, StandardRule<T> rule) {
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
            private DynamicType _creating;

            public BuiltinInitAdapter(ArgumentValues ai, CallAction action, BuiltinFunction method)
                : base(ai, action) {
                _method = method;
                _creating = (DynamicType)ai.Arguments[0];
            }

            public override Expression MakeInitCall(ActionBinder binder, StandardRule<T> rule, Expression createExpr) {
                if (_method == InstanceOps.Init.Template) {
                    // we have a default __init__, don't call it.
                    return createExpr;
                }

                Type[] testTypes;
                MethodBinder mb = GetMethodBinder(binder);
                MethodCandidate mc = mb.MakeBindingTarget(CallType.None, ArrayUtils.Insert(_creating.UnderlyingSystemType, Arguments.Types), out testTypes);

                if (mc != null) {
                    if (testTypes != null) TestTypes = ArrayUtils.RemoveFirst(testTypes);
                    return mc.Target.MakeExpression(
                            binder,
                            rule,
                            ArrayUtils.Insert<Expression>(createExpr, Arguments.Expressions)
                        );

                } else {
                    testTypes = Arguments.Types;
                }

                return null;
            }

            public override Statement GetError(ActionBinder binder, StandardRule<T> rule) {
                MethodBinder mb = GetMethodBinder(binder);

                return binder.MakeInvalidParametersError(mb, Action, CallType.None, _method.Targets, rule, Arguments.Arguments);
            }

            private MethodBinder GetMethodBinder(ActionBinder binder) {
                return MethodBinder.MakeBinder(binder, "__init__", _method.Targets, BinderType.Normal, Arguments.Names);
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
                                null,
                                typeof(PythonOps).GetMethod("GetMixedMember"),
                                Ast.CodeContext(),
                                Ast.Cast(rule.Parameters[0], typeof(DynamicMixin)),
                                createExpr,
                                Ast.Constant(Symbols.Init)
                            ),
                            ArrayUtils.RemoveFirst(rule.Parameters)
                        )
                    );
            }
        }

        #endregion

        #region Misc. Helpers

        private StandardRule<T> MakeIncorrectArgumentsRule(ArgumentValues ai) {
            Rule.SetTarget(
                Rule.MakeError(
                    Binder,
                    Ast.New(
                        typeof(ArgumentTypeException).GetConstructor(new Type[] { typeof(string) }),
                        Ast.Constant("default __new__ does not take parameters")
                    )
                )
            );
            MakeErrorTests(ai);
            return Rule;
        }

        private void MakeTests(ArgumentValues ai, NewAdapter newAdapter, InitAdapter initAdapter) {
            MakeSplatTests();

            Rule.SetTest(
                Ast.AndAlso(
                    Ast.AndAlso(
                        Test,
                        MakeNecessaryTests(Rule, new Type[][] { newAdapter.TestTypes, initAdapter.TestTypes }, ai.Expressions)
                    ),
                    MakeTypeTestForCreateInstance((DynamicType)Arguments[0], Rule)
                )
            );
        }

        private void MakeErrorTests(ArgumentValues ai) {
            MakeSplatTests();
            Rule.SetTest(
                Ast.AndAlso(
                    Ast.AndAlso(
                        Test,
                        MakeNecessaryTests(Rule, new Type[][] { ai.Types }, ai.Expressions)
                    ),
                    MakeTypeTestForCreateInstance((DynamicType)Arguments[0], Rule)
                )
            );
        }

        internal static CreateInstanceAction MakeCreateInstanceAction(CallAction action) {
            return CreateInstanceAction.Make(new CallSignature(action.Signature));
        }


        private bool IsStandardDotNetType(DynamicType type) {
            return type.IsSystemType &&
                !PythonTypeCustomizer.IsPythonType(type.UnderlyingSystemType) &&            
                !type.UnderlyingSystemType.IsDefined(typeof(PythonSystemTypeAttribute), true) &&
                !typeof(Delegate).IsAssignableFrom(type.UnderlyingSystemType) &&
                !type.UnderlyingSystemType.IsArray;
        }

        private Expression GetFinalizerInitialization(Variable variable) {
            return Ast.Call(
                null,
                typeof(PythonOps).GetMethod("InitializeForFinalization"),
                Ast.ReadDefined(variable)
            );
        }

        private bool HasFinalizer(DynamicType creating) {
            DynamicTypeSlot del;
            bool hasDel = creating.TryResolveSlot(Context, Symbols.Unassign, out del);
            return hasDel;
        }

        private static bool IsMixedNewStyleOldStyle(DynamicType dt) {
            if (!Mro.IsOldStyle(dt)) {
                foreach (DynamicType baseType in dt.ResolutionOrder) {
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

        private bool HasDefaultNew(DynamicType creating) {
            DynamicTypeSlot newInst;
            creating.TryResolveSlot(Context, Symbols.NewInst, out newInst);
            return newInst == InstanceOps.New;
        }

        private bool HasDefaultInit(DynamicType creating) {
            DynamicTypeSlot init;
            creating.TryResolveSlot(Context, Symbols.Init, out init);
            return init == InstanceOps.Init;
        }

        private bool HasDefaultNewAndInit(DynamicType creating) {
            return HasDefaultNew(creating) && HasDefaultInit(creating);
        }

        /// <summary>
        /// Checks if we have a default new and init - in this case if we have any
        /// arguments we don't allow the call.
        /// </summary>
        private bool TooManyArgsForDefaultNew(DynamicType creating, bool hasArgs) {
            if (hasArgs) {
                return HasDefaultNewAndInit(creating);
            }
            return false;
        }

        /// <summary>
        /// Generates an expression which calls a .NET constructor directly.
        /// </summary>
        public static MethodCandidate GetTypeConstructor(ActionBinder binder, DynamicType creating, Type[] argTypes) {
            // type has no __new__ override, call .NET ctor directly
            MethodBinder mb = MethodBinder.MakeBinder(binder,
                DynamicTypeOps.GetName(creating),
                creating.UnderlyingSystemType.GetConstructors(),
                BinderType.Normal);

            MethodCandidate mc = mb.MakeBindingTarget(CallType.None, argTypes);
            if (mc != null && mc.Target.Method.IsPublic) {
                return mc;
            }
            return null;
        }

        /// <summary>
        /// Creates a test which tests the specific version of the type.
        /// </summary>
        public Expression MakeTypeTestForCreateInstance(DynamicType creating, StandardRule<T> rule) {
            _altVersion = creating.Version == DynamicType.DynamicVersion;
            string vername = _altVersion ? "AlternateVersion" : "Version";
            int version = _altVersion ? creating.AlternateVersion : creating.Version;

            Expression versionExpr;
            if (_canTemplate) {
                versionExpr = rule.AddTemplatedConstant(typeof(int), version);
            } else {
                versionExpr = Ast.Constant(version);
            }

            return Ast.Equal(
                Ast.ReadProperty(
                    Ast.Cast(rule.Parameters[0], typeof(DynamicType)),
                    typeof(DynamicType).GetProperty(vername)),
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

        private void CopyTemplateToRule(CodeContext context, DynamicType t, params object[] templateData) {
            TemplatedRuleBuilder<T> builder;

            lock (PythonCallTemplateBuilders) {
                ShareableTemplateKey key =
                    new ShareableTemplateKey(Action,
                    t.UnderlyingSystemType,
                    t.Version == DynamicMixin.DynamicVersion,
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
