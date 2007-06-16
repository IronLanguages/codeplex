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

using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Actions {
    public class SetMemberBinderHelper<T> {
        private ActionBinder _binder;
        private SetMemberAction _action;
        public SetMemberBinderHelper(ActionBinder binder, SetMemberAction action) {
            this._binder = binder;
            this._action = action;
        }

        public StandardRule<T> MakeNewRule(object[] args) {
            Debug.Assert(args != null && args.Length == 2);

            object target = args[0];

            DynamicType targetType = DynamicHelpers.GetDynamicType(target);

            // TODO: Optimize static fields/properties, DynamicType is an ICustomMembers.
            if (!(target is ICustomMembers)) {                  
                // if it's an non-system type or it's been extended we need to initialize
                // the type to check for GetBoundMember.  Otherwise we can skip it.
                if (!targetType.IsSystemType || targetType.IsExtended) {
                    // Disable caching for the dynamic cases
                    if (IsNonSystemMutableType(target, targetType) ||
                        targetType.Version == DynamicMixin.DynamicVersion ||
                        targetType.HasDynamicMembers(_binder.Context)) {
                        return MakeDynamicRule(targetType);
                    }
                }

                return MakeSetMemberRule(targetType.UnderlyingSystemType);
            }
            
            return MakeDynamicRule(targetType);
        }

        private StandardRule<T> MakeSetMemberRule(Type type) {
            // properties take precedence over fields (per the order of init from ReflectedTypeBuilder).
            PropertyInfo pi = type.GetProperty(SymbolTable.IdToString(_action.Name));
            if (pi != null) {
                return MakePropertyRule(type, pi);
            }

            FieldInfo fi = type.GetField(SymbolTable.IdToString(_action.Name));
            if (fi != null) {
                return MakeFieldRule(type, fi);
            }

            // TODO handle builtin methods specially here
            return MakeDynamicRule(type);
        }

        private bool IsNonSystemMutableType(object target, DynamicType targetType) {
            if (!targetType.IsSystemType) {
                ISuperDynamicObject sdo = target as ISuperDynamicObject;
                if (sdo != null && !sdo.HasDictionary) {
                    // instance can't have new members...
                    return false;
                }
            }
            return !targetType.IsImmutable;
        }

        private StandardRule<T> MakePropertyRule(Type targetType, PropertyInfo info) {
            MethodInfo setter = info.GetSetMethod();
          
            if (setter != null && !setter.IsStatic && 
                !setter.DeclaringType.IsValueType && 
                CompilerHelpers.CanOptimizeMethod(setter)) {

                StandardRule<T> rule = new StandardRule<T>();
                rule.MakeTest(targetType);
                rule.SetTarget(MakeCallExpression(setter, rule.Parameters));
                return rule;
            } 

            return MakeDynamicRule(targetType);            
        }

        private Statement MakeCallExpression(MethodInfo method, Variable[] parameters) {
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

        private StandardRule<T> MakeFieldRule(Type targetType, FieldInfo field) {            
            if (targetType.UnderlyingSystemType.IsValueType) {
                // let the dynamic rule get the error message right
                return MakeDynamicRule(targetType);
            }

            if (!field.IsInitOnly && !field.IsStatic && CompilerHelpers.CanOptimizeField(field)) {
                StandardRule<T> rule = new StandardRule<T>();
                rule.MakeTest(targetType);
                rule.SetTarget(
                    rule.MakeReturn(
                        _binder,
                        MemberAssignment.Field(
                            field.IsStatic ?
                                null :
                                rule.GetParameterExpression(0),
                            field,
                            _binder.ConvertExpression(rule.GetParameterExpression(1), field.FieldType)
                        )
                    )
                );
                return rule;
            } 
            return MakeDynamicRule(targetType);            
        }

        private StandardRule<T> MakeDynamicRule(Type targetType) {
            return MakeDynamicRule(DynamicHelpers.GetDynamicTypeFromType(targetType));
        }

        private StandardRule<T> MakeDynamicRule(DynamicType targetType) {
            StandardRule<T> rule = new StandardRule<T>();
            rule.MakeTest(new DynamicType[] { targetType });
            Expression expr = MethodCallExpression.Call(null,
                    typeof(RuntimeHelpers).GetMethod("SetMember"),
                    rule.GetParameterExpression(1),
                    rule.GetParameterExpression(0),
                    ConstantExpression.Constant(this._action.Name),
                    new CodeContextExpression());
            rule.SetTarget(rule.MakeReturn(_binder, expr));
            return rule;
        }
    }
}
