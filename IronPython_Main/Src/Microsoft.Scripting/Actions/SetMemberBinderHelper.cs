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

using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Actions {
    using Ast = Microsoft.Scripting.Ast.Ast;

    public class SetMemberBinderHelper<T> : 
        BinderHelper<T, SetMemberAction> {

        public SetMemberBinderHelper(CodeContext context, SetMemberAction action)
            : base(context, action) {
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
                        targetType.HasDynamicMembers(Context)) {
                        return MakeDynamicRule(targetType);
                    }
                }

                return MakeSetMemberRule(targetType.UnderlyingSystemType);
            }
            
            return MakeDynamicRule(targetType);
        }

        private StandardRule<T> MakeSetMemberRule(Type type) {
            // properties take precedence over fields (per the order of init from ReflectedTypeBuilder).
            PropertyInfo pi = type.GetProperty(SymbolTable.IdToString(Action.Name));
            if (pi != null) {
                return MakePropertyRule(type, pi);
            }

            FieldInfo fi = type.GetField(SymbolTable.IdToString(Action.Name));
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
                rule.SetTarget(MakeCallStatement(setter, rule.Parameters));
                return rule;
            } 

            return MakeDynamicRule(targetType);            
        }

        private static Statement InvalidArgumentCount(MethodInfo method, int expected, int provided) {
            return Ast.Statement(
                Ast.Throw(
                    Ast.Call(
                        null,
                        typeof(RuntimeHelpers).GetMethod("SimpleTypeError"),
                        Ast.Constant(
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
                        Binder,
                        Ast.AssignField(
                            field.IsStatic ?
                                null :
                                rule.Parameters[0],
                            field,
                            Binder.ConvertExpression(rule.Parameters[1], field.FieldType)
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
            Expression expr = Ast.Call(null,
                    typeof(RuntimeHelpers).GetMethod("SetMember"),
                    rule.Parameters[1],
                    rule.Parameters[0],
                    Ast.Constant(Action.Name),
                    Ast.CodeContext());
            rule.SetTarget(rule.MakeReturn(Binder, expr));
            return rule;
        }
    }
}
