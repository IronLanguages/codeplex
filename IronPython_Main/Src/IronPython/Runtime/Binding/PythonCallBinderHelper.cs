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
using System.Linq.Expressions;
using System.Scripting;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime {
    using Ast = System.Linq.Expressions.Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    class PythonCallBinderHelper<T> : CallBinderHelper<T, OldCallAction> where T : class {
        public PythonCallBinderHelper(CodeContext context, OldCallAction action, object[] args)
            : base(context, action, args) {
        }

        private ActionBinder PythonBinder {
            get {
                return PythonContext.GetContext(Context).Binder;
            }
        }

        public new RuleBuilder<T> MakeRule() {            
            PythonType dt = Arguments[0] as PythonType;
            if (dt != null) {
                if (IsStandardDotNetType(dt)) {
                    return ((IOldDynamicObject)dt).GetRule<T>(MakeCreateInstanceAction(Action), Context, Arguments);
                }

                // TODO: this should move into PythonType's IDynamicObject implementation when that exists
                return MakePythonTypeCallRule(dt);
            }

            // fall back to default implementation
            return null;
        }

        private RuleBuilder<T> MakePythonTypeCallRule(PythonType creating) {
            ArgumentValues ai = MakeArgumentInfo();
            NewAdapter newAdapter;
            InitAdapter initAdapter;

            if (TooManyArgsForDefaultNew(creating, ai.Expressions.Length > 0)) {
                return MakeIncorrectArgumentsRule(ai, creating);
            }

            GetAdapters(creating, ai, out newAdapter, out initAdapter);

            // get the expression for calling __new__
            Expression createExpr = newAdapter.GetExpression(Binder, PythonBinder, Rule);
            if (createExpr == null) {
                Rule.Target = newAdapter.GetError(Binder, Rule);
                MakeErrorTests(ai);
                return Rule;
            }

            // then get the statement for calling __init__
            VariableExpression allocatedInst = Rule.GetTemporary(createExpr.Type, "newInst");
            Expression tmpRead = allocatedInst;
            Expression initCall = initAdapter.MakeInitCall(Binder, Rule, tmpRead);

            List<Expression> body = new List<Expression>();

            // then get the call to __del__ if we need one
            if (HasFinalizer(creating)) {
                body.Add(
                    Ast.Assign(allocatedInst, createExpr)
                );
                body.Add(
                    GetFinalizerInitialization(allocatedInst)
                );
            }

            // add the call to init if we need to
            if (initCall != tmpRead) {
                // init can fail but if __new__ returns a different type
                // no exception is raised.
                Expression initStmt = initCall ?? initAdapter.GetError(Binder, Rule);

                if (body.Count == 0) {
                    body.Add(
                        Ast.Assign(allocatedInst, createExpr)
                    );
                }

                if (!creating.UnderlyingSystemType.IsAssignableFrom(createExpr.Type)) {
                    // return type of object, we need to check the return type before calling __init__.
                    body.Add(
                        AstUtils.IfThen(
                            Ast.TypeIs(allocatedInst, creating.UnderlyingSystemType),
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
                    Rule.MakeReturn(Binder, allocatedInst)
                );

                Rule.Target = Ast.Block(body.ToArray());
            }

            MakeTests(ai, newAdapter, initAdapter);

            return Rule;
        }

        #region Adapter support

        private void GetAdapters(PythonType creating, ArgumentValues ai, out NewAdapter newAdapter, out InitAdapter initAdapter) {
            PythonTypeSlot newInst, init;
            creating.TryResolveSlot(Context, Symbols.NewInst, out newInst);
            creating.TryResolveSlot(Context, Symbols.Init, out init);

            newAdapter = GetNewAdapter(ai, creating, newInst);
            initAdapter = GetInitAdapter(ai, creating, init);
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
                return new ConstructorNewAdapter(ai, Action);
            } else if (newInst is BuiltinFunction) {
                return new BuiltinNewAdapter(ai, Action, ((BuiltinFunction)newInst));
            }

            return new NewAdapter(ai, Action);
        }

        private class CallAdapter {
            private Type[] _testTypes;
            private OldCallAction _action;
            private ArgumentValues _argInfo;

            public CallAdapter(ArgumentValues ai, OldCallAction action) {
                _action = action;
                _argInfo = ai;
            }

            public Type[] TestTypes {
                get { return _testTypes; }
                set { _testTypes = value; }
            }

            protected OldCallAction Action {
                get { return _action; }
            }

            protected ArgumentValues Arguments {
                get { return _argInfo; }
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
            public NewAdapter(ArgumentValues ai, OldCallAction action)
                : base(ai, action) {
            }

            public virtual Expression GetExpression(ActionBinder binder, ActionBinder python, RuleBuilder<T> rule) {
                return AstUtils.Call(
                        GetDynamicNewAction(python),
                        typeof(object),
                        ArrayUtils.Insert<Expression>(
                            rule.Context,
                            Ast.Call(
                                typeof(PythonOps).GetMethod("PythonTypeGetMember"),
                                rule.Context,
                                Ast.Convert(rule.Parameters[0], typeof(PythonType)),
                                Ast.Null(),
                                AstUtils.Constant(Symbols.NewInst)
                            ),                        
                            rule.Parameters
                        )
                    );
            }

            public virtual Expression GetError(DefaultBinder binder, RuleBuilder<T> rule) {
                throw new InvalidOperationException();
            }

            protected OldCallAction GetDynamicNewAction(ActionBinder python) {
                return OldCallAction.Make(python, Action.Signature.InsertArgument(ArgumentInfo.Simple));
            }
        }

        private class DefaultNewAdapter : NewAdapter {
            private PythonType _creating;

            public DefaultNewAdapter(ArgumentValues ai, OldCallAction action, PythonType creating)
                : base(ai, action) {
                _creating = creating;
            }

            public override Expression GetExpression(ActionBinder binder, ActionBinder python, RuleBuilder<T> rule) {
                BindingTarget target = _creating.IsSystemType ?
                    GetTypeConstructor(binder, _creating, Type.EmptyTypes) :
                    GetTypeConstructor(binder, _creating, new Type[] { typeof(PythonType) });

                Debug.Assert(target.Success);
                return target.MakeExpression(
                    rule,
                    _creating.IsSystemType ? new Expression[0] : new Expression[] { rule.Parameters[0] }
                );
            }
        }

        private class ConstructorNewAdapter : NewAdapter {
            private PythonType _creating;
            private BindingTarget _target;

            public ConstructorNewAdapter(ArgumentValues ai, OldCallAction action)
                : base(ai, action) {
                _creating = (PythonType)ai.Arguments[0];
            }

            public override Expression GetExpression(ActionBinder binder, ActionBinder python, RuleBuilder<T> rule) {
                Type[] types = _creating.IsSystemType ? Arguments.Types : ArrayUtils.Insert(typeof(PythonType), Arguments.Types);

                MethodBinder mb = GetMethodBinder(binder);
                _target = mb.MakeBindingTarget(CallTypes.None, types);

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
                return MethodBinder.MakeBinder(binder, _creating.Name, _creating.UnderlyingSystemType.GetConstructors(), Arguments.Names);
            }

            public override Expression GetError(DefaultBinder binder, RuleBuilder<T> rule) {
                return binder.MakeInvalidParametersError(_target).MakeErrorForRule(rule, binder);
            }
        }

        private class BuiltinNewAdapter : NewAdapter {
            private PythonType _creating;
            private BuiltinFunction _ctor;
            private BindingTarget _target;

            public BuiltinNewAdapter(ArgumentValues ai, OldCallAction action, BuiltinFunction ctor)
                : base(ai, action) {
                _creating = (PythonType)ai.Arguments[0];
                _ctor = ctor;
            }

            public override Expression GetExpression(ActionBinder binder, ActionBinder python, RuleBuilder<T> rule) {
                Type[] types = ArrayUtils.Insert(typeof(PythonType), Arguments.Types);

                MethodBinder mb = GetMethodBinder(binder);

                _target = mb.MakeBindingTarget(CallTypes.None, types);
                if (_target.Success) {
                    Expression[] parameters = ArrayUtils.Insert(rule.Parameters[0], Arguments.Expressions);
                    if (_target.ArgumentTests != null) {
                        TestTypes = ArrayUtils.RemoveFirst(ArrayUtils.ToArray(_target.ArgumentTests));
                    }
                    return _target.MakeExpression(rule, parameters);
                }
                return null;

            }

            public override Expression GetError(DefaultBinder binder, RuleBuilder<T> rule) {
                return binder.MakeInvalidParametersError(_target).MakeErrorForRule(rule, binder);
            }

            private MethodBinder GetMethodBinder(ActionBinder binder) {
                return MethodBinder.MakeBinder(binder, _creating.Name, _ctor.Targets, Arguments.Names);
            }
        }

        private class MixedNewAdapter : NewAdapter {
            public MixedNewAdapter(ArgumentValues ai, OldCallAction action)
                : base(ai, action) {
            }


            public override Expression GetExpression(ActionBinder binder, ActionBinder python, RuleBuilder<T> rule) {
                return AstUtils.Call(
                        GetDynamicNewAction(python),
                        typeof(object),
                        ArrayUtils.Insert<Expression>(
                            rule.Context,
                            Ast.Call(
                                typeof(PythonOps).GetMethod("GetMixedMember"),
                                rule.Context,
                                Ast.Convert(rule.Parameters[0], typeof(PythonType)),
                                Ast.Constant(null),
                                AstUtils.Constant(Symbols.NewInst)
                            ),
                            rule.Parameters
                        )
                    );
            }
        }

        #endregion

        #region __init__ adapters

        private class InitAdapter : CallAdapter {
            public InitAdapter(ArgumentValues ai, OldCallAction action)
                : base(ai, action) {
            }

            public virtual Expression MakeInitCall(ActionBinder binder, RuleBuilder<T> rule, Expression createExpr) {
                return
                    AstUtils.Call(
                        Action,
                        typeof(object),
                        ArrayUtils.Insert<Expression>(
                            rule.Context,
                            Ast.Call(
                                typeof(PythonOps).GetMethod("GetInitMember"),
                                rule.Context,
                                Ast.Convert(rule.Parameters[0], typeof(PythonType)),
                                Ast.ConvertHelper(createExpr, typeof(object))
                            ),
                            ArrayUtils.RemoveFirst(rule.Parameters)
                        )
                    );
            }

            public virtual Expression GetError(DefaultBinder binder, RuleBuilder<T> rule) {
                throw new InvalidOperationException();
            }
        }

        private class DefaultInitAdapter : InitAdapter {
            public DefaultInitAdapter(ArgumentValues ai, OldCallAction action)
                : base(ai, action) {
            }

            public override Expression MakeInitCall(ActionBinder binder, RuleBuilder<T> rule, Expression createExpr) {
                // default init, we can just return the value from __new__
                return createExpr;
            }
        }

        private class BuiltinInitAdapter : InitAdapter {
            private BuiltinFunction _method;
            private PythonType _creating;
            private BindingTarget _target;

            public BuiltinInitAdapter(ArgumentValues ai, OldCallAction action, BuiltinFunction method)
                : base(ai, action) {
                _method = method;
                _creating = (PythonType)ai.Arguments[0];
            }

            public override Expression MakeInitCall(ActionBinder binder, RuleBuilder<T> rule, Expression createExpr) {
                if (_method == InstanceOps.Init.Template) {
                    // we have a default __init__, don't call it.
                    return createExpr;
                }

                MethodBinder mb = GetMethodBinder(binder);
                _target = mb.MakeBindingTarget(CallTypes.None, ArrayUtils.Insert(_creating.UnderlyingSystemType, Arguments.Types));

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

            public override Expression GetError(DefaultBinder binder, RuleBuilder<T> rule) {
                return binder.MakeInvalidParametersError(_target).MakeErrorForRule(rule, binder);
            }

            private MethodBinder GetMethodBinder(ActionBinder binder) {
                return MethodBinder.MakeBinder(binder, "__init__", _method.Targets, Arguments.Names);
            }
        }

        private class MixedInitAdapter : InitAdapter {
            public MixedInitAdapter(ArgumentValues ai, OldCallAction action)
                : base(ai, action) {
            }

            public override Expression MakeInitCall(ActionBinder binder, RuleBuilder<T> rule, Expression createExpr) {
                return
                    AstUtils.Call(
                        Action,
                        typeof(object),
                        ArrayUtils.Insert<Expression>(
                            rule.Context,
                            Ast.Call(
                                typeof(PythonOps).GetMethod("GetMixedMember"),
                                rule.Context,
                                Ast.Convert(rule.Parameters[0], typeof(PythonType)),
                                Ast.ConvertHelper(createExpr, typeof(object)),
                                AstUtils.Constant(Symbols.Init)
                            ),
                            ArrayUtils.RemoveFirst(rule.Parameters)
                        )
                    );
            }
        }

        #endregion

        #region Misc. Helpers

        private RuleBuilder<T> MakeIncorrectArgumentsRule(ArgumentValues ai, PythonType creating) {
            if (creating.IsSystemType && creating.UnderlyingSystemType.GetConstructors().Length == 0) {
                // this is a type we can't create ANY instances of, give the user a half-way decent error message
                Rule.Target = 
                    Rule.MakeError(
                        Ast.New(
                            typeof(ArgumentTypeException).GetConstructor(new Type[] { typeof(string) }),
                            Ast.Constant("cannot create instances of " + creating.Name)
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

        internal static OldCreateInstanceAction MakeCreateInstanceAction(OldCallAction action) {
            return OldCreateInstanceAction.Make(action.Binder, new CallSignature(action.Signature));
        }


        private bool IsStandardDotNetType(PythonType type) {
            return type.IsSystemType &&
                !type.IsPythonType &&            
                !typeof(Delegate).IsAssignableFrom(type.UnderlyingSystemType) &&
                !type.UnderlyingSystemType.IsArray;
        }

        private Expression GetFinalizerInitialization(VariableExpression variable) {
            return Ast.Call(
                typeof(PythonOps).GetMethod("InitializeForFinalization"),
                Rule.Context,
                Ast.ConvertHelper(variable, typeof(object))
            );
        }

        private bool HasFinalizer(PythonType creating) {
            // only user types have finalizers...
            if (creating.IsSystemType) return false;

            PythonTypeSlot del;
            bool hasDel = creating.TryResolveSlot(Context, Symbols.Unassign, out del);
            return hasDel;
        }

        private static bool IsMixedNewStyleOldStyle(PythonType dt) {
            if (!dt.IsOldClass) {
                foreach (PythonType baseType in dt.ResolutionOrder) {
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
                creating.Name,
                creating.UnderlyingSystemType.GetConstructors());

            BindingTarget target = mb.MakeBindingTarget(CallTypes.None, argTypes);
            if (target.Success && target.Method.IsPublic) {
                return target;
            }
            return null;
        }

        /// <summary>
        /// Creates a test which tests the specific version of the type.
        /// </summary>
        public Expression MakeTypeTestForCreateInstance(PythonType creating, RuleBuilder<T> rule) {
            int version = creating.Version;

            Expression versionExpr = Ast.Constant(version);            

            return Ast.Equal(
                Ast.Call(
                    typeof(PythonOps).GetMethod("GetTypeVersion"),
                    Ast.Convert(rule.Parameters[0], typeof(PythonType))
                ),
                versionExpr
            );
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
