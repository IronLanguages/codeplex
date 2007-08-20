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
using Microsoft.Scripting.Types;

namespace Microsoft.Scripting.Actions {
    using Ast = Microsoft.Scripting.Ast.Ast;
    using Microsoft.Scripting.Utils;

    public class SetMemberBinderHelper<T> :
        MemberBinderHelper<T, SetMemberAction> {

        public SetMemberBinderHelper(CodeContext context, SetMemberAction action, object[] args)
            : base(context, action, args) {
        }

        public StandardRule<T> MakeNewRule() {
            Type targetType = CompilerHelpers.GetType(Target);

            Rule.MakeTest(StrongBoxType ?? targetType);

            if (Target is ICustomMembers) {
                return MakeSetCustomMemberRule(targetType);
            }

            return MakeSetMemberRule(targetType);
        }

        private StandardRule<T> MakeSetMemberRule(Type type) {
            if (MakeOperatorSetMemberBody(type)) {
                return Rule;
            }

            MemberInfo[] members = Binder.GetMember(type, StringName);

            // if lookup failed try the strong-box type if available.
            if (members.Length == 0 && StrongBoxType != null) {
                type = StrongBoxType;
                StrongBoxType = null;

                members = Binder.GetMember(type, StringName);
            }

            Expression error;
            MemberTypes memberTypes = GetMemberType(members, out error);
            if (error != null) {
                Rule.MakeError(Binder, error);
                return Rule;
            }

            switch (memberTypes) {
                case MemberTypes.Method:
                case MemberTypes.NestedType:
                case MemberTypes.TypeInfo:
                case MemberTypes.Constructor: return MakeReadOnlyMemberError(type);
                case MemberTypes.Event: return MakeEventValidation(type, members);
                case MemberTypes.Field: return MakeFieldRule(type, members);
                case MemberTypes.Property: return MakePropertyRule(type, members);
                case MemberTypes.All:
                    // no match
                    return MakeMissingMemberError(type);
                default:
                    throw new InvalidOperationException();
            }
        }

        private StandardRule<T> MakeEventValidation(Type type, MemberInfo[] members) {
            ReflectedEvent ev = ReflectionCache.GetReflectedEvent((EventInfo)members[0]);

            // handles in place addition of events - this validates the user did the right thing, probably too Python specific.
            Rule.SetTarget(
                Rule.MakeReturn(Binder,
                    Ast.Call(
                        Ast.RuntimeConstant(ev),
                        typeof(ReflectedEvent).GetMethod("TrySetValue"),
                        Ast.CodeContext(),
                        Rule.Parameters[0],
                        Ast.Null(),
                        Rule.Parameters[1]
                    )
                )
            );

            return Rule;
        }

        private StandardRule<T> MakePropertyRule(Type targetType, MemberInfo[] properties) {
            PropertyInfo info = (PropertyInfo)properties[0];

            MethodInfo setter = info.GetSetMethod(true);

            // Allow access to protected getters TODO: this should go, it supports IronPython semantics.
            if (setter != null && !setter.IsPublic && !(setter.IsFamily || setter.IsFamilyOrAssembly)) {
                if (!ScriptDomainManager.Options.PrivateBinding) {
                    setter = null;
                }
            }

            if (setter != null) {
                if (setter.IsStatic) {
                    // TODO: Too python specific
                    Rule.SetTarget(Binder.MakeReadOnlyMemberError(Rule, targetType, StringName));
                } else if (setter.ContainsGenericParameters) {
                    Rule.SetTarget(Rule.MakeError(Binder, MakeGenericPropertyExpression()));
                } else if (setter.IsPublic && !setter.DeclaringType.IsValueType) {
                    Rule.SetTarget(Rule.MakeReturn(Binder, MakeReturnValue(MakeCallExpression(setter, Rule.Parameters))));
                } else {
                    // TODO: Should be able to do better w/ value types.
                    Rule.SetTarget(Rule.MakeReturn(
                            Binder,
                            MakeReturnValue(
                                Ast.Call(
                                    Ast.RuntimeConstant(info),
                                    typeof(PropertyInfo).GetMethod("SetValue", new Type[] { typeof(object), typeof(object), typeof(object[]) }),
                                    Instance,
                                    Rule.Parameters[1],
                                    Ast.NewArray(typeof(object[]))
                                )
                            )
                        )
                    );
                }
            } else {
                Rule.SetTarget(Binder.MakeMissingMemberError(Rule, targetType, StringName));
            }
            return Rule;
        }

        private StandardRule<T> MakeFieldRule(Type targetType, MemberInfo[] fields) {
            FieldInfo field = (FieldInfo)fields[0];

            if (field.DeclaringType.IsGenericType && field.DeclaringType.GetGenericTypeDefinition() == typeof(StrongBox<>)) {
                // work around a CLR bug where we can't access generic fields from dynamic methods.
                Rule.SetTarget(
                    Rule.MakeReturn(Binder,
                        MakeReturnValue(
                            Ast.Call(
                                null,
                                typeof(RuntimeHelpers).GetMethod("UpdateBox").MakeGenericMethod(field.DeclaringType.GetGenericArguments()),
                                Instance,
                                Rule.Parameters[1]
                            )
                        )
                    )
                );
            } else if (field.DeclaringType.IsValueType) {
                Rule.SetTarget(Rule.MakeError(Binder, Ast.New(typeof(ArgumentException).GetConstructor(ArrayUtils.EmptyTypes))));
            } else if (field.IsInitOnly || (field.IsStatic && targetType != field.DeclaringType)) {     // TODO: Field static check too python specific
                Rule.SetTarget(Binder.MakeReadOnlyMemberError(Rule, targetType, StringName));
            } else if (field.IsPublic && field.DeclaringType.IsVisible) {
                Rule.SetTarget(
                    Rule.MakeReturn(
                        Binder,
                        MakeReturnValue(
                            Ast.AssignField(
                                field.IsStatic ?
                                    null :
                                    Ast.Cast(Rule.Parameters[0], field.DeclaringType),
                                field,
                                Binder.ConvertExpression(Rule.Parameters[1], field.FieldType)
                            )
                        )
                    )
                );
            } else {
                Rule.SetTarget(
                    Rule.MakeReturn(
                        Binder,
                        MakeReturnValue(
                            Ast.Call(
                                Ast.RuntimeConstant(field),
                                typeof(FieldInfo).GetMethod("SetValue", new Type[] { typeof(object), typeof(object) }),
                                field.IsStatic ? Ast.Constant(null) : Instance,
                                Rule.Parameters[1]
                            )
                        )
                    )
                );
            }

            return Rule;
        }

        private StandardRule<T> MakeSetCustomMemberRule(Type targetType) {
            Rule.SetTarget(
                Rule.MakeReturn(Binder,
                    MakeReturnValue(
                        Ast.Call(
                            Ast.Cast(Rule.Parameters[0], typeof(ICustomMembers)),
                            typeof(ICustomMembers).GetMethod("SetCustomMember"),
                            Ast.CodeContext(),
                            Ast.Constant(Action.Name),
                            Rule.Parameters[1]
                        )
                    )
                )
            );
            return Rule;
        }

        private Expression MakeReturnValue(Expression expression) {
            return Ast.Comma(0, Rule.Parameters[1], expression);
        }

        /// <summary> if a member-injector is defined-on or registered-for this type call it </summary>
        private bool MakeOperatorSetMemberBody(Type type) {
            MethodInfo setMem = GetMethod(type, "SetMember");
            if (setMem != null && setMem.IsSpecialName) {
                Expression[] args = setMem.IsStatic ? 
                    new Expression[] { Rule.Parameters[0], Ast.Constant(StringName), Rule.Parameters[1] } :
                    new Expression[] { Ast.Constant(StringName), Rule.Parameters[1] };

                Rule.SetTarget(Rule.MakeReturn(Binder,
                    Ast.Call(
                    setMem.IsStatic ? null : Instance,
                    setMem,
                    args)
                ));
                return true;
            }
            return false;

        }
        private StandardRule<T> MakeMissingMemberError(Type type) {
            Rule.SetTarget(Binder.MakeMissingMemberError(Rule, type, StringName));
            return Rule;
        }

        private StandardRule<T> MakeReadOnlyMemberError(Type type) {
            Rule.SetTarget(Binder.MakeReadOnlyMemberError(Rule, type, StringName));
            return Rule;
        }
    }
}
