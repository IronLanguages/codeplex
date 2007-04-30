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
using System.Reflection;
using System.Diagnostics;

using IronPython.Compiler;
using IronPython.Runtime.Types;
using IronPython.Runtime.Operations;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Internal.Ast;
using Microsoft.Scripting.Internal.Generation;

namespace IronPython.Runtime.Calls {
    public class GetMemberBinderHelper<T> {
        private ActionBinder _binder;
        private GetMemberAction _action;
        public GetMemberBinderHelper(ActionBinder binder, GetMemberAction action) {
            this._binder = binder;
            this._action = action;
        }

        public StandardRule<T> MakeNewRule(object[] args) {
            Debug.Assert(args != null && args.Length == 1);

            object target = args[0];

            IActionable ido = target as IActionable;
            if (ido != null) {
                StandardRule<T> rule = ido.GetRule<T>(_action, _binder.Context, args);
                if (rule != null) return rule;
            }

            DynamicType targetType = Ops.GetDynamicType(target);

            // Disable caching for the dynamic cases
            if (target is ICustomMembers ||
                !targetType.IsImmutable ||
                !targetType.IsSystemType ||
                targetType.Version == DynamicMixin.DynamicVersion ||
                targetType.HasDynamicMembers(_binder.Context)) {
                return MakeDynamicRule(targetType);
            }

            DynamicTypeSlot slot;
            if (targetType.TryResolveSlot(_binder.Context, _action.Name, out slot)) {
                ReflectedField rf;
                ReflectedProperty rp;

                if ((rf = slot as ReflectedField) != null) {
                    return MakeFieldRule(targetType, rf);
                } else if ((rp = slot as ReflectedProperty) != null) {
                    return MakePropertyRule(targetType, rp);
                } else {
                    // TODO handle builtin methods specially here
                    return MakeDynamicRule(targetType);
                }
            }
            return MakeRuleForNoMatch(targetType);
        }

        private StandardRule<T> MakePropertyRule(DynamicType targetType, ReflectedProperty reflectedProperty) {
            MethodInfo getter = reflectedProperty.Getter;
            if (getter != null && !getter.IsStatic && CompilerHelpers.CanOptimizeMethod(getter)) {
                StandardRule<T> rule = new StandardRule<T>();
                rule.MakeTest(new DynamicType[] { targetType });
                rule.SetTarget(MakeCallExpression(getter, rule.Parameters));
                return rule;
            } else {
                return MakeDynamicRule(targetType);
            }
        }

        private Statement MakeCallExpression(MethodInfo method, VariableReference[] parameters) {
            ParameterInfo[] infos = method.GetParameters();
            Expression callInst = null;
            int parameter = 0;
            Expression[] callArgs = new Expression[infos.Length];

            if (!method.IsStatic) {
                callInst = BoundExpression.Defined(parameters[0]);
                parameter = 1;
            }
            for (int arg = 0; arg < infos.Length; arg++) {
                if (parameter < parameters.Length) {
                    callArgs[arg] = _binder.ConvertExpression(
                        BoundExpression.Defined(parameters[parameter++]),
                        infos[arg].ParameterType);
                } else {
                    return InvalidArgumentCount(method, infos.Length, parameters.Length);
                }
            }

            // check that we used all parameters
            if (parameter != parameters.Length) {
                return InvalidArgumentCount(method, infos.Length, parameters.Length);
            }
            return new ReturnStatement(MethodCallExpression.Call(callInst, method, callArgs));
        }

        private static Statement InvalidArgumentCount(MethodInfo method, int expected, int provided) {
            return new ExpressionStatement(
                new ThrowExpression(
                    MethodCallExpression.Call(
                        null,
                        typeof(RuntimeHelpers).GetMethod("SimpleTypeError"),
                        new ConstantExpression(
                            String.Format("{0}() takes exactly {1} arguments ({1} given)", method.Name, expected, provided)
                        )
                    )
                )
            );
        }

        private StandardRule<T> MakeFieldRule(DynamicType targetType, ReflectedField reflectedField) {
            FieldInfo field = reflectedField.info;
            if (!field.IsStatic && CompilerHelpers.CanOptimizeField(field)) {
                StandardRule<T> rule = new StandardRule<T>();
                rule.MakeTest(new DynamicType[] { targetType });
                rule.SetTarget(
                    rule.MakeReturn(
                        _binder,
                        MemberExpression.Field(
                            field.IsStatic ?
                                null :
                                rule.GetParameterExpression(0),
                            reflectedField.info
                        )
                    )
                );
                return rule;
            } else {
                return MakeDynamicRule(targetType);
            }
        }

        private StandardRule<T> MakeDynamicRule(DynamicType targetType) {
            StandardRule<T> rule = new StandardRule<T>();
            rule.MakeTest(new DynamicType[] { targetType });
            Expression expr = MethodCallExpression.Call(null, typeof(Ops).GetMethod("GetBoundAttr"),
                    new CodeContextExpression(),
                    rule.GetParameterExpression(0),
                    ConstantExpression.Constant(this._action.Name));
            rule.SetTarget(rule.MakeReturn(_binder, expr));
            return rule;
        }

        private StandardRule<T> MakeRuleForNoMatch(DynamicType targetType) {
            return StandardRule<T>.AttributeError(
                "couldn't find member " + SymbolTable.IdToString(_action.Name),
                targetType
            );
        }
    }
}
