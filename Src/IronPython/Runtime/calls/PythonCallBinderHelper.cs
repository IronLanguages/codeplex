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

using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Types;

using IronPython.Runtime.Types;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Calls {
    using Ast = Microsoft.Scripting.Ast.Ast;
    using Microsoft.Scripting.Utils;

    class PythonCallBinderHelper<T> : BinderHelper<T, CallAction> {
        private List<Type[]> _testTypes = new List<Type []>();
        private object[] _args;
        private bool _canTemplate, _altVersion;
        private List<object> _templateData;

        private static Dictionary<ShareableTemplateKey, TemplatedRuleBuilder<T>> PythonCallTemplateBuilders = new Dictionary<ShareableTemplateKey, TemplatedRuleBuilder<T>>();

        public PythonCallBinderHelper(CodeContext context, CallAction action, object []args)
            : base(context, action) {
            _args = args;
        }

        public StandardRule<T> MakeRule() {
            DynamicType dt = _args[0] as DynamicType;
            if (dt != null) {
                if (Action.IsSimple || Action.IsParamsCall()) {
                    if (IsStandardDotNetType(dt)) {
                        // If CreateInstance can't do it then we'll fall back to a dynamic call rule.
                        // In the future CreateInstanceBinderHelper should always return a rule successfully.
                        return new CreateInstanceBinderHelper<T>(Context, 
                            CreateInstanceAction.Make(Action.ArgumentInfos)).MakeRule(_args) ?? 
                                CallBinderHelper<T>.MakeDynamicCallRule(Action, Binder, CompilerHelpers.ObjectTypes(_args));
                    } else if (IsMixedNewStyleOldStyle(dt)) {
                        return MakeDynamicTypeRule(dt);
                    } else {                                            
                        _canTemplate = IsTemplatable(dt);
                        DynamicType[] types = CompilerHelpers.ObjectTypes(_args);

                        return MakePythonTypeCallRule(dt, types, ArrayUtils.RemoveFirst(types));
                    }
                }

                return MakeDynamicTypeRule(dt);
            }

            return null;    // fall back to default implementation
        }

        private StandardRule<T> MakePythonTypeCallRule(DynamicType creating, DynamicType[] types, DynamicType[] argTypes) {
            StandardRule<T> rule = new StandardRule<T>();
            // TODO: Pass in MethodCandidate when we support kw-args
            if (!TooManyArgsForDefaultNew(creating, ArgumentCount(Action, rule) > 0)) {
                Expression createExpr = MakeCreationExpression(creating, rule);
                if (createExpr != null) {
                    Statement target = MakeInitTarget(creating, rule, createExpr);

                    if (target != null) {
                        Debug.Assert(!_canTemplate || _testTypes.Count == 0);
                        rule.SetTest(
                            Ast.AndAlso(
                                Ast.TypeIs(rule.Parameters[0], typeof(DynamicType)),
                                CreateInstanceBinderHelper<T>.MakeTypeTestForParams(Action, rule, _args, 
                                    Ast.AndAlso(
                                        MakeTypeTestForCreateInstance(creating, rule),
                                        MakeNecessaryTests(rule, 
                                            _testTypes,
                                            ArrayUtils.Insert(
                                                rule.Parameters[0], 
                                                CallBinderHelper<T>.GetArgumentExpressions(null, Action, rule, _args)))
                                    )
                                )                                    
                            )
                        );

                        rule.SetTarget(target);

                        if (_canTemplate) {
                            CopyTemplateToRule(Context, creating, rule);
                        }

                        return rule;
                    }
                }
            }
            return MakeDynamicTypeRule(creating);
        }       

        /// <summary>
        /// Calls the .NET ctor or __new__ for the creating type.    Returns null if the types aren't compatbile
        /// w/ the arguments.
        /// </summary>
        private Expression MakeCreationExpression(DynamicType creating, StandardRule<T> rule) {
            Expression createExpr = null;
            DynamicTypeSlot newInst;
            creating.TryResolveSlot(Context, Symbols.NewInst, out newInst);
            if (newInst == InstanceOps.New) {
                // parameterless ctor, call w/ no args
                MethodCandidate cand = creating.IsSystemType ?
                    CreateInstanceBinderHelper<T>.GetTypeConstructor(Binder, creating, ArrayUtils.EmptyTypes) :
                    CreateInstanceBinderHelper<T>.GetTypeConstructor(Binder, creating, new Type[] { typeof(DynamicType) });

                Debug.Assert(cand != null);
                createExpr = cand.Target.MakeExpression(Binder,
                    rule,
                    creating.IsSystemType ? new Expression[0] : new Expression[] { rule.Parameters[0] },
                    creating.IsSystemType ? Type.EmptyTypes : new Type[] { typeof(DynamicType) });
            } else if (newInst is ConstructorFunction) {
                // ctor w/ parameters, call w/ args
                Type [] types = creating.IsSystemType ?
                    CallBinderHelper<T>.GetArgumentTypes(Action, _args) :
                    ArrayUtils.Insert(typeof(DynamicType), CallBinderHelper<T>.GetArgumentTypes(Action, _args));

                MethodCandidate cand = CreateInstanceBinderHelper<T>.GetTypeConstructor(Binder,
                        creating,
                        types);

                if (cand != null) {
                    createExpr = cand.Target.MakeExpression(Binder,
                        rule,
                        creating.IsSystemType ?
                            CallBinderHelper<T>.GetArgumentExpressions(cand, Action, rule, _args) :
                            ArrayUtils.Insert(rule.Parameters[0], CallBinderHelper<T>.GetArgumentExpressions(cand, Action, rule, _args)));
                }
            } else {
                // type has __new__, call that w/ the cls parameter
                BuiltinFunction bf = newInst as BuiltinFunction;
                if (bf != null) {
                    createExpr = CallPythonNew(creating, bf, rule);
                } else {
                    createExpr = CallResolvedNew(rule);
                }
            }
            return createExpr;
        }

        private ActionExpression CallResolvedNew(StandardRule<T> rule) {
            return Ast.Action.Call(
                GetDynamicNewAction(),
                typeof(object),
                ArrayUtils.Insert<Expression>(
                    Ast.Call(
                        Ast.Cast(rule.Parameters[0], typeof(DynamicMixin)),
                        typeof(DynamicMixin).GetMethod("GetMember"),
                        new Expression[] { 
                            Ast.CodeContext(),
                            Ast.Null(),
                            Ast.Constant(Symbols.NewInst),
                        }
                    ),
                    rule.Parameters
                )
            );
        }

        private CallAction GetDynamicNewAction() {
            if (Action.IsSimple) return CallAction.Simple;

            return CallAction.Make(ArrayUtils.Insert(ArgumentInfo.Simple, Action.ArgumentInfos));
        }

        /// <summary>
        /// Generates code to call the __init__ method.  Returns null if the call is imcompatible.
        /// </summary>
        private Statement MakeInitTarget(DynamicType creating, StandardRule<T> rule, Expression createExpr) {
            DynamicTypeSlot init;
            creating.TryResolveSlot(Context, Symbols.Init, out init);
            bool hasDel = HasFinalizer(creating);

            Statement target = null;
            if ((init == InstanceOps.Init && !hasDel) || (creating == TypeCache.DynamicType && _args.Length == 2)) {
                // default init, we can just return the value from __new__
                target = rule.MakeReturn(Binder, createExpr);
            } else {
                Variable variable = rule.GetTemporary(createExpr.ExpressionType, "newInst");
                Expression assignment = Ast.Assign(variable, createExpr);
                Expression initCall = null;

                // get the target for the __init__ call, if we can
                BuiltinMethodDescriptor bmd = init as BuiltinMethodDescriptor;
                if (bmd != null) {
                    initCall = CallInitBuiltin(creating, bmd, rule, variable);
                } else {
                    // call init w/ result from new
                    initCall = CallInitDynamic(rule, variable);
                }

                // make the appropriate call to the __init__ method
                if (initCall != null) {
                    Statement body = GetTargetWithOptionalFinalization(hasDel, variable, initCall);

                    if (!creating.UnderlyingSystemType.IsAssignableFrom(createExpr.ExpressionType)) {
                        // return type of object, we need to check the return type before calling __init__.
                        target = CallInitChecked(creating, rule, variable, assignment, body);
                    } else {
                        // just call the __init__ method, no type check necessary (TODO: need null check?)
                        target = CallInitUnchecked(rule, variable, assignment, body);
                    }
                }
            }

            return target;
        }

        private Statement GetTargetWithOptionalFinalization(bool hasDel, Variable variable, Expression initCall) {
            Statement body = Ast.Statement(initCall);
            if (hasDel) {
                body = Ast.Block(
                    body,
                    Ast.Statement(
                        Ast.Call(
                            null,
                            typeof(PythonOps).GetMethod("InitializeForFinalization"),
                            Ast.ReadDefined(variable)
                        )
                    )
                );
            }

            return body;
        }

        private BlockStatement CallInitUnchecked(StandardRule<T> rule, Variable variable, Expression assignment, Statement body) {
            return Ast.Block(
                Ast.Statement(assignment),
                body,
                rule.MakeReturn(Binder, Ast.ReadDefined(variable))
            );
        }

        private BlockStatement CallInitChecked(DynamicType creating, StandardRule<T> rule, Variable variable, Expression assignment, Statement body) {
            return Ast.Block(
                Ast.Statement(assignment),
                Ast.IfThen(
                    Ast.TypeIs(Ast.ReadDefined(variable), creating.UnderlyingSystemType),
                    body
                ),
                rule.MakeReturn(Binder, Ast.ReadDefined(variable))
            );
        }

        private Expression CallInitBuiltin(DynamicType creating, BuiltinMethodDescriptor bmd, StandardRule<T> rule, Variable variable) {
            Expression initCall = null;
            if (bmd == InstanceOps.Init) {
                // we have a default __init__, don't call it.
                initCall = Ast.Void(Ast.Empty());
            } else {
                MethodBinder mb = MethodBinder.MakeBinder(Binder, "__init__", bmd.Template.Targets, BinderType.Normal);
                Type []testTypes;
                MethodCandidate mc = mb.MakeBindingTarget(CallType.None,
                    ArrayUtils.Insert(creating.UnderlyingSystemType, CallBinderHelper<T>.GetArgumentTypes(Action, _args)), out testTypes);
                if (mc != null) {
                    _testTypes.Add(testTypes);
                    initCall = mc.Target.MakeExpression(Binder,
                        rule,
                        ArrayUtils.Insert<Expression>(Ast.Read(variable), CallBinderHelper<T>.GetArgumentExpressions(mc, Action, rule, _args)));
                }
            }
            return initCall;
        }

        private ActionExpression CallInitDynamic(StandardRule<T> rule, Variable self) {
            return Ast.Action.Call(
                Action,
                typeof(object),
                ArrayUtils.Insert<Expression>(
                    Ast.Call(
                        Ast.Cast(rule.Parameters[0], typeof(DynamicMixin)),
                        typeof(DynamicMixin).GetMethod("GetMember"),
                        Ast.CodeContext(),
                        Ast.ReadDefined(self),
                        Ast.Constant(Symbols.Init)
                    ),
                    ArrayUtils.RemoveFirst(rule.Parameters)
                )
            );
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

        private bool IsTemplatable(DynamicType creating) {
            if (creating == TypeCache.DynamicType) return false;

            if (!creating.IsSystemType && !HasFinalizer(creating)) {
                DynamicTypeSlot newInst, init;
                creating.TryResolveSlot(Context, Symbols.NewInst, out newInst);
                creating.TryResolveSlot(Context, Symbols.Init, out init);

                if (init != InstanceOps.Init && init is BuiltinMethodDescriptor) return false;
                if (newInst != InstanceOps.New && (newInst is ConstructorFunction || newInst is BuiltinFunction)) return false;

                return true;
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

        private bool HasFinalizer(DynamicType creating) {
            DynamicTypeSlot del;
            bool hasDel = creating.TryResolveSlot(Context, Symbols.Unassign, out del);
            return hasDel;
        }

        /// <summary>
        /// Generates an Expression which calls the Python __new__ method w/ the class parameter
        /// </summary>
        private Expression CallPythonNew(DynamicType creating, BuiltinFunction newInst, StandardRule<T> rule) {
            Type[] types = ArrayUtils.Insert(typeof(DynamicType), CallBinderHelper<T>.GetArgumentTypes(Action, _args));

            MethodBinder mb = MethodBinder.MakeBinder(Binder,
                DynamicTypeOps.GetName(creating),
                newInst is ConstructorFunction ? ((ConstructorFunction)newInst).ConstructorTargets : newInst.Targets,
                BinderType.Normal);

            Type[] testTypes;
            MethodCandidate mc = mb.MakeBindingTarget(CallType.None, types, out testTypes);
            Expression[] parameters = ArrayUtils.Insert(rule.Parameters[0], CallBinderHelper<T>.GetArgumentExpressions(mc, Action, rule, _args));
            Expression createExpr = null;
            if (mc != null) {
                _testTypes.Add(testTypes);
                createExpr = mc.Target.MakeExpression(Binder, rule, parameters);
            }
            return createExpr;
        }

        private bool IsStandardDotNetType(DynamicType type) {
            return type.IsSystemType &&
                !PythonTypeCustomizer.IsPythonType(type.UnderlyingSystemType) &&            // TODO: Remove Python specific checks
                !typeof(Delegate).IsAssignableFrom(type.UnderlyingSystemType) &&
                !type.UnderlyingSystemType.IsArray;   
        }

        private StandardRule<T> MakeDynamicTypeRule(DynamicType dt) {
            DynamicType[] types = CompilerHelpers.ObjectTypes(_args);
            if (Action != CallAction.Simple) {
                return CallBinderHelper<T>.MakeDynamicCallRule(Action, Binder, types);
            }

            PerfTrack.NoteEvent(PerfTrack.Categories.Compiler, String.Format("dynamic type rule {0}", dt.Name));
            if (dt.GetType() != typeof(DynamicType)) {
                return CallBinderHelper<T>.MakeDynamicCallRule(Action, Binder, types);
            }

            StandardRule<T> rule = new StandardRule<T>();
            Expression[] exprs = rule.Parameters;
            Expression[] finalExprs = ArrayUtils.RemoveFirst(exprs);

            rule.SetTest(
                Ast.AndAlso(
                    Ast.TypeIs(rule.Parameters[0], typeof(DynamicType)), 
                    CreateInstanceBinderHelper<T>.MakeTypeTestForCreateInstance(dt, rule)
                )
            );

            rule.SetTarget(
                rule.MakeReturn(
                    Binder,
                    Ast.Call(
                        null,
                        typeof(DynamicTypeOps).GetMethod("CallWorker", new Type[] { typeof(CodeContext), typeof(DynamicType), typeof(object[]) }),
                        Ast.CodeContext(),
                        Ast.Cast(rule.Parameters[0], typeof(DynamicType)),
                        Ast.NewArray(typeof(object[]), finalExprs)
                    )
                )
            );

            return rule;
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

        private struct ShareableTemplateKey : IEquatable<ShareableTemplateKey> {
            private Type _type;
            private bool _altVersion, _hasDefaultInit, _hasDefaultNew;
            private Action _action;

            public ShareableTemplateKey(Action action, Type type, bool altVersion, bool hasDefaultInit, bool hasDefaultNew) {
                _type = type;
                _altVersion = altVersion;
                _hasDefaultInit = hasDefaultInit;
                _hasDefaultNew = hasDefaultNew;
                _action = action;
            }

            #region IEquatable<ShareableTemplateKey> Members

            public bool Equals(ShareableTemplateKey other) {
                return other._action == _action &&
                    other._type == _type &&
                    other._altVersion == _altVersion &&
                    other._hasDefaultInit == _hasDefaultInit &&
                    other._hasDefaultNew == _hasDefaultNew;
            }

            #endregion
        }
        private void CopyTemplateToRule(CodeContext context, DynamicType t, StandardRule<T> rule) {
            TemplatedRuleBuilder<T> builder;

            lock (PythonCallTemplateBuilders) {
                ShareableTemplateKey key = new ShareableTemplateKey(Action, t.UnderlyingSystemType, _altVersion, HasDefaultInit(t), HasDefaultNew(t));
                if (!PythonCallTemplateBuilders.TryGetValue(key, out builder)) {
                    PythonCallTemplateBuilders[key] = rule.GetTemplateBuilder();
                    return;
                }
            }

            builder.CopyTemplateToRule(context, rule, _templateData.ToArray());
        }

        public Expression MakeTypeTestForCreateInstance(DynamicType creating, StandardRule<T> rule) {
            _altVersion = creating.Version == DynamicType.DynamicVersion;
            string vername = _altVersion ? "AlternateVersion" : "Version";
            int version = _altVersion ? creating.AlternateVersion : creating.Version;

            Expression versionExpr;
            if (_canTemplate) {
                versionExpr = rule.AddTemplatedConstant(typeof(int), version);
                AddTemplateData(version);
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

        private void AddTemplateData(int version) {
            if (_templateData == null) _templateData = new List<object>();
            _templateData.Add(version);
        }
    }
}
