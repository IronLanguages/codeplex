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

using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;

using IronPython.Runtime.Types;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Calls {
    class PythonCallBinderHelper<T> : BinderHelper<T, CallAction> {
        public PythonCallBinderHelper(CodeContext context, CallAction action)
            : base(context, action) {

        }

        public StandardRule<T> MakeRule(object[] args) {
            DynamicType dt = args[0] as DynamicType;
            if (dt != null) {
                if (Action.IsSimple || IsParamsCallWorker(Action)) {
                    if (IsStandardDotNetType(dt)) {
                        // If CreateInstance can't do it then we'll fall back to a dynamic call rule.
                        // In the future CreateInstanceBinderHelper should always return a rule successfully.
                        return new CreateInstanceBinderHelper<T>(Context, CreateInstanceAction.Make(Action.ArgumentKinds)).MakeRule(args) 
                            ?? CallBinderHelper<T>.MakeDynamicCallRule(Action, Binder, CompilerHelpers.ObjectTypes(args));
                    } else {
                        DynamicType[] types = CompilerHelpers.ObjectTypes(args);
                        DynamicType[] argTypes = new DynamicType[types.Length - 1];
                        Array.Copy(types, 1, argTypes, 0, types.Length - 1);

                        return MakePythonTypeCallRule(dt, types, argTypes, args);
                    }
                }
            }

            return null;    // fall back to default implementation
        }

        private StandardRule<T> MakePythonTypeCallRule(DynamicType creating, DynamicType[] types, DynamicType[] argTypes, object[] args) {
            // can't optimize types w/ metaclasses yet.
            DynamicTypeSlot mc;
            if (creating.TryResolveSlot(Context, Symbols.MetaClass, out mc)
                || creating.Version == DynamicMixin.DynamicVersion) {
                // Limiting on DynamicVersion is too agressive here - we can limit on less because all of this
                // comes from type info and instances can't customize it.  But because we can't do a good check
                // on type version ID we need to do this independently.
                return MakeDynamicTypeRule(creating, args);
            }

            StandardRule<T> rule = new StandardRule<T>();

            Expression[] parameters = CallBinderHelper<T>.GetArgumentExpressions(Action, rule, args);
            if (!TooManyArgsForDefaultNew(creating, parameters)) {
                Expression createExpr = MakeCreationExpression(creating, rule, args);
                if (createExpr != null) {
                    Statement target = MakeInitTarget(creating, rule, args, createExpr);

                    if (target != null) {
                        rule.SetTest(CreateInstanceBinderHelper<T>.MakeTestForTypeCall(Action, creating, rule, args));
                        rule.SetTarget(target);
                        return rule;
                    }
                }
            }
            return MakeDynamicTypeRule(creating, args);
        }


        /// <summary>
        /// Calls the .NET ctor or __new__ for the creating type.    Returns null if the types aren't compatbile
        /// w/ the arguments.
        /// </summary>
        private Expression MakeCreationExpression(DynamicType creating, StandardRule<T> rule, object[] args) {
            Expression createExpr = null;
            DynamicTypeSlot newInst;
            creating.TryResolveSlot(Context, Symbols.NewInst, out newInst);
            if (newInst == InstanceOps.New) {
                // parameterless ctor, call w/ no args
                createExpr = creating.IsSystemType ?
                    CreateInstanceBinderHelper<T>.CallTypeConstructor(Binder, creating, new DynamicType[0], new Expression[0]) :
                    CreateInstanceBinderHelper<T>.CallTypeConstructor(Binder, creating, new DynamicType[] { TypeCache.DynamicType }, new Expression[] { rule.GetParameterExpression(0) });
            } else if (newInst is ConstructorFunction) {
                // ctor w/ parameters, call w/ args
                createExpr = creating.IsSystemType ?
                    CreateInstanceBinderHelper<T>.CallTypeConstructor(Binder, 
                        creating, 
                        CallBinderHelper<T>.GetArgumentTypes(Action, args), 
                        CallBinderHelper<T>.GetArgumentExpressions(Action, rule, args)) :
                    CreateInstanceBinderHelper<T>.CallTypeConstructor(Binder, 
                        creating,
                        CompilerHelpers.PrependArray(TypeCache.DynamicType, 
                            CallBinderHelper<T>.GetArgumentTypes(Action, args)),
                        CompilerHelpers.PrependArray(rule.GetParameterExpression(0), CallBinderHelper<T>.GetArgumentExpressions(Action, rule, args))
                    );
            } else {
                // type has __new__, call that w/ the cls parameter
                BuiltinFunction bf = newInst as BuiltinFunction;
                if (bf != null) {
                    createExpr = CallPythonNew(creating, bf, rule, args);
                } else {
                    createExpr = ActionExpression.Call(
                        GetDynamicNewAction(),
                        typeof(object),
                        CompilerHelpers.PrependArray<Expression>(
                            MethodCallExpression.Call(rule.GetParameterExpression(0),
                                typeof(DynamicMixin).GetMethod("GetMember"),
                                new Expression[] { 
                                    new CodeContextExpression(),
                                    new ConstantExpression(null),
                                    new ConstantExpression(Symbols.NewInst),
                                }
                            ),
                            rule.GetParameterExpressions()
                        )
                    );
                }
            }
            return createExpr;
        }

        private CallAction GetDynamicNewAction() {
            if (Action.IsSimple) return CallAction.Simple;

            return CallAction.Make(CompilerHelpers.PrependArray(ArgumentKind.Simple, Action.ArgumentKinds));
        }

        /// <summary>
        /// Generates code to call the __init__ method.  Returns null if the call is imcompatible.
        /// </summary>
        private Statement MakeInitTarget(DynamicType creating, StandardRule<T> rule, object[] args, Expression createExpr) {
            DynamicTypeSlot init, del;
            creating.TryResolveSlot(Context, Symbols.Init, out init);
            bool hasDel = creating.TryResolveSlot(Context, Symbols.Unassign, out del);

            Statement target = null;
            if ((init == InstanceOps.Init && !hasDel) ||
                (creating == TypeCache.DynamicType && args.Length == 2)) {
                // default init, we can just return the value from __new__
                target = rule.MakeReturn(Binder, createExpr);
            } else {
                Variable vr = rule.GetTemporary(createExpr.ExpressionType, "newInst");
                Expression assignment = BoundAssignment.Assign(vr, createExpr);
                Expression initCall = null;

                // get the target for the __init__ call, if we can
                BuiltinMethodDescriptor bmd = init as BuiltinMethodDescriptor;
                if (bmd != null) {
                    initCall = CallInitBuiltin(creating, bmd, rule, args, vr);
                } else {
                    // call init w/ result from new
                    initCall = CallInitDynamic(rule, vr);
                }

                // make the appropriate call to the __init__ method
                if (initCall != null) {
                    Statement body = GetTargetWithOptionalFinalization(hasDel, vr, initCall);

                    if (!creating.UnderlyingSystemType.IsAssignableFrom(createExpr.ExpressionType)) {
                        // return type of object, we need to check the return type before calling __init__.
                        target = CallInitChecked(creating, rule, vr, assignment, body);
                    } else {
                        // just call the __init__ method, no type check necessary (TODO: need null check?)
                        target = CallInitUnchecked(rule, vr, assignment, body);
                    }
                }
            }

            return target;
        }

        private static Statement GetTargetWithOptionalFinalization(bool hasDel, Variable vr, Expression initCall) {
            Statement body = new ExpressionStatement(initCall);
            if (hasDel) {
                body = BlockStatement.Block(
                        body,
                        new ExpressionStatement(
                            MethodCallExpression.Call(null,
                            typeof(PythonOps).GetMethod("InitializeForFinalization"),
                            BoundExpression.Defined(vr)
                        ))
                    );
            }

            return body;
        }

        private BlockStatement CallInitUnchecked(StandardRule<T> rule, Variable vr, Expression assignment, Statement body) {
            return BlockStatement.Block(
                new ExpressionStatement(assignment),
                body,
                rule.MakeReturn(Binder, BoundExpression.Defined(vr))
            );
        }

        private BlockStatement CallInitChecked(DynamicType creating, StandardRule<T> rule, Variable vr, Expression assignment, Statement body) {
            return BlockStatement.Block(
                new ExpressionStatement(assignment),
                IfStatement.IfThen(
                    TypeBinaryExpression.TypeIs(BoundExpression.Defined(vr), creating.UnderlyingSystemType),
                    body
                ),
                rule.MakeReturn(Binder, BoundExpression.Defined(vr))
            );
        }

        private Expression CallInitBuiltin(DynamicType creating, BuiltinMethodDescriptor bmd, StandardRule<T> rule, object[] args, Variable vr) {
            Expression initCall = null;
            if (bmd == InstanceOps.Init) {
                // we have a default __init__, don't call it.
                initCall = new VoidExpression(new EmptyStatement());
            } else {
                MethodBinder mb = MethodBinder.MakeBinder(Binder, "__init__", bmd.Template.Targets, BinderType.Normal);
                MethodCandidate mc = mb.MakeBindingTarget(CallType.None, 
                    CompilerHelpers.PrependArray(creating, CallBinderHelper<T>.GetArgumentTypes(Action, args)));
                if (mc != null) {
                    initCall = mc.Target.MakeExpression(Binder, CompilerHelpers.PrependArray<Expression>(BoundExpression.Defined(vr), CallBinderHelper<T>.GetArgumentExpressions(Action, rule, args)));
                }
            }
            return initCall;
        }

        private ActionExpression CallInitDynamic(StandardRule<T> rule, Variable self) {
            return ActionExpression.Call(
                Action,
                typeof(object),
                CompilerHelpers.PrependArray<Expression>(
                    MethodCallExpression.Call(rule.GetParameterExpression(0),
                        typeof(DynamicMixin).GetMethod("GetMember"),
                        new Expression[] { 
                            new CodeContextExpression(),
                            BoundExpression.Defined(self),
                            new ConstantExpression(Symbols.Init),
                        }
                    ),
                    CompilerHelpers.RemoveFirst(rule.GetParameterExpressions())
                )
            );
        }

        /// <summary>
        /// Checks if we have a default new and init - in this case if we have any
        /// arguments we don't allow the call.
        /// </summary>
        private bool TooManyArgsForDefaultNew(DynamicType creating, Expression[] args) {
            if (args.Length > 0) {
                DynamicTypeSlot newInst, init;
                creating.TryResolveSlot(Context, Symbols.NewInst, out newInst);
                creating.TryResolveSlot(Context, Symbols.Init, out init);

                return newInst == InstanceOps.New && init == InstanceOps.Init;
            }
            return false;
        }

        /// <summary>
        /// Generates an Expression which calls the Python __new__ method w/ the class parameter
        /// </summary>
        private Expression CallPythonNew(DynamicType creating, BuiltinFunction newInst, StandardRule<T> rule, object[] args) {
            Expression[] parameters = CompilerHelpers.PrependArray(rule.GetParameterExpression(0), CallBinderHelper<T>.GetArgumentExpressions(Action, rule, args));
            DynamicType[] types = CompilerHelpers.PrependArray(TypeCache.DynamicType, CallBinderHelper<T>.GetArgumentTypes(Action, args));

            MethodBinder mb = MethodBinder.MakeBinder(Binder,
                DynamicTypeOps.GetName(creating),
                newInst is ConstructorFunction ? ((ConstructorFunction)newInst).ConstructorTargets : newInst.Targets,
                BinderType.Normal);

            MethodCandidate mc = mb.MakeBindingTarget(CallType.None, types);
            Expression createExpr = null;
            if (mc != null) {
                createExpr = mc.Target.MakeExpression(Binder, parameters);
            }
            return createExpr;
        }

        private bool IsStandardDotNetType(DynamicType type) {
            return type.IsSystemType &&
                !PythonTypeCustomizer.IsPythonType(type.UnderlyingSystemType) &&            // TODO: Remove Python specific checks
                !typeof(Delegate).IsAssignableFrom(type.UnderlyingSystemType) &&
                !type.UnderlyingSystemType.IsArray;   
        }

        private StandardRule<T> MakeDynamicTypeRule(DynamicType dt, object[] args) {
            DynamicType[] types = CompilerHelpers.ObjectTypes(args);
            if (Action != CallAction.Simple) {
                return CallBinderHelper<T>.MakeDynamicCallRule(Action, Binder, types);
            }

            PerfTrack.NoteEvent(PerfTrack.Categories.Compiler, String.Format("dynamic type rule {0}", dt.Name));
            if (dt.GetType() != typeof(DynamicType)) {
                return CallBinderHelper<T>.MakeDynamicCallRule(Action, Binder, types);
            }

            StandardRule<T> rule = new StandardRule<T>();
            Expression[] exprs = Variable.VariablesToExpressions(rule.Parameters);
            Expression[] finalExprs = new Expression[exprs.Length - 1];
            Array.Copy(exprs, 1, finalExprs, 0, exprs.Length - 1);

            rule.SetTest(CreateInstanceBinderHelper<T>.MakeTestForTypeCall(Action, dt, rule, args));

            rule.SetTarget(
                rule.MakeReturn(
                    Binder,
                    MethodCallExpression.Call(
                        null,
                        typeof(DynamicTypeOps).GetMethod("CallWorker", new Type[] { typeof(CodeContext), typeof(DynamicType), typeof(object[]) }),
                        new CodeContextExpression(),
                        StaticUnaryExpression.Convert(
                            rule.GetParameterExpression(0),
                            typeof(DynamicType)
                        ),
                        NewArrayExpression.NewArrayInit(typeof(object[]), finalExprs)
                    )
                )
            );

            return rule;


        }
    }
}
