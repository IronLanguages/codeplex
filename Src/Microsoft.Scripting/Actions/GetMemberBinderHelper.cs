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
using System.Collections.Generic;
using System.Text;

using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Types;

namespace Microsoft.Scripting.Actions {
    using Ast = Microsoft.Scripting.Ast.Ast;
    using Microsoft.Scripting.Utils;

    /// <summary>
    /// Builds a rule for a GetMemberAction.  Supports all built-in .NET members, ICustomMembers, the OperatorMethod 
    /// GetBoundMember, and StrongBox instances.
    /// 
    /// The RuleMaker sets up it's initial state grabbing the incoming type and the type we should look members up
    /// from.  It then does the lookup for the members using the current context's ActionBinder and then binds to
    /// the returned members if the match is non-ambigious.  
    /// 
    /// The target of the rule is built up using a series of block statements as the body.  
    /// </summary>
    public class GetMemberBinderHelper<T> : MemberBinderHelper<T, GetMemberAction> {
        private Statement _body = Ast.Empty();      // the body of the rule as it's built up
        private Expression _instance;               // the expression the specifies the instance or null for rule.Parameters[0]
        private bool _bound;                        // true if we're forcing a bound lookup, false otherwise.  Only alters methods

        public GetMemberBinderHelper(CodeContext context, GetMemberAction action, object[] args)
            : base(context, action, args) {
        }

        public GetMemberBinderHelper(CodeContext context, GetMemberAction action, StandardRule<T> rule, bool bound, Expression self)
            : base(context, action, new object[1]) {
            Rule = rule;
            _instance = self;
            _bound = bound;
        }

        public Statement MakeMemberRuleTarget(Type instanceType, params MemberInfo[] members) {
            // This should go away w/ abstract values when it'll be easier to compose rules.
            return MakeRuleBody(instanceType, members);
        }

        public StandardRule<T> MakeNewRule() {
            Rule.MakeTest(StrongBoxType ?? CompilerHelpers.GetType(Target));
            Rule.SetTarget(MakeGetMemberTarget());

            return Rule;
        }

        public Statement MakeRuleBody(Type type, params MemberInfo[] members) {
            return MakeBodyHelper(type, members);
        }

        private Statement MakeGetMemberTarget() {
            Type type = CompilerHelpers.GetType(Target);

            // This goes away when ICustomMembers goes away.
            if (typeof(ICustomMembers).IsAssignableFrom(type)) {
                MakeCustomMembersBody(type);
                return _body;
            }

            MemberInfo[] members = Binder.GetMember(type, StringName);

            // if lookup failed try the strong-box type if available.
            if (members.Length == 0 && StrongBoxType != null) {
                StrongBoxType = null;
                type = typeof(StrongBox<>).MakeGenericType(type);

                members = Binder.GetMember(type, StringName);
            }

            return MakeBodyHelper(type, members);
        }

        private Statement MakeBodyHelper(Type type, params MemberInfo[] members) {
            Expression error;
            MemberTypes memberType = GetMemberType(members, out error);
            if (error != null) {
                _body = Ast.Block(_body, Rule.MakeError(Binder, error));
                return _body;
            }

            switch (memberType) {
                case MemberTypes.Event:      MakeEventBody(type, members);    break;
                case MemberTypes.Field:      MakeFieldBody(type, members);    break;
                case MemberTypes.Method:     MakeMethodBody(type, members);   break;
                case MemberTypes.NestedType: MakeTypeBody(type, members);     break;
                case MemberTypes.Property:   MakePropertyBody(type, members); break;
                case MemberTypes.All:     
                    // no members were found
                    MakeOperatorGetMemberBodyOrError(type);                       
                    break;
                case MemberTypes.Constructor:
                case MemberTypes.TypeInfo:
                default: throw new InvalidOperationException();
            }
            return _body;
        }

        private void MakeEventBody(Type type, MemberInfo[] members) {
            EventInfo ei = (EventInfo)members[0];

            ReflectedEvent re = ReflectionCache.GetReflectedEvent(ei);

            _body = Ast.Block(_body,
                Rule.MakeReturn(
                    Binder,
                    Ast.Call(null,
                        typeof(RuntimeHelpers).GetMethod("MakeBoundEvent"),
                        Ast.RuntimeConstant(re),
                        Instance,
                        Ast.Constant(type)
                    )
                )
            );
        }

        private void MakeFieldBody(Type type, MemberInfo[] members) {
            FieldInfo fi = (FieldInfo)members[0];

            if (fi.IsPublic && fi.DeclaringType.IsPublic) {
                _body = Ast.Block(_body,
                    Rule.MakeReturn(
                        Binder,
                        Ast.ReadField(fi.IsStatic ? null : Instance, fi)
                    )
                );
            } else {
                _body = Ast.Block(_body,
                    Rule.MakeReturn(
                        Binder,
                        Ast.Call(
                            Ast.RuntimeConstant(fi),
                            typeof(FieldInfo).GetMethod("GetValue"),
                            fi.IsStatic ? Ast.Constant(null) : Instance
                        )
                    )
                );
            } 
        }

        private void MakeMethodBody(Type type, MemberInfo[] members) {
            BuiltinFunction target = ReflectionCache.GetMethodGroup(type, StringName, GetCallableMethods(members));

            if ((target.FunctionType & FunctionType.FunctionMethodMask) != FunctionType.Function || _bound) {
                // for strong box we need to bind the strong box, so we don't use Instance here.
                _body = Ast.Block(_body,
                    Rule.MakeReturn(
                    Binder,                        
                    Ast.New(typeof(BoundBuiltinFunction).GetConstructor(new Type[] { typeof(BuiltinFunction), typeof(object) }),
                        Ast.RuntimeConstant(target),
                        _instance ?? Rule.Parameters[0]    
                    )
                ));
            } else {
                _body = Ast.Block(_body,
                    Rule.MakeReturn(
                        Binder,
                        Ast.RuntimeConstant(target)
                    )
                );
            }
        }

        private void MakeTypeBody(Type type, MemberInfo[] members) {
            _body = Ast.Block(_body,
                Rule.MakeReturn(Binder,
                Ast.Call(null,
                    typeof(DynamicHelpers).GetMethod("GetDynamicTypeFromType"),
                    Ast.Constant(type)
                )
            ));
        }

        private void MakePropertyBody(Type type, MemberInfo[] members) {
            PropertyInfo pi = members[0] as PropertyInfo;
            MethodInfo getter = pi.GetGetMethod(true);

            // Allow access to protected getters TODO: this should go, it supports IronPython semantics.
            if (getter != null && !getter.IsPublic && !(getter.IsFamily || getter.IsFamilyOrAssembly)) {
                if (!ScriptDomainManager.Options.PrivateBinding) {
                    getter = null;
                }
            }

            if (getter == null) {
                MakeMissingMemberError(type);
                return;
            }

            getter = CompilerHelpers.GetCallableMethod(getter);

            if (pi.GetIndexParameters().Length > 0) {
                _body = Ast.Block(_body,
                    Rule.MakeReturn(Binder,
                        Ast.New(typeof(ReflectedIndexer).GetConstructor(new Type[] { typeof(ReflectedIndexer), typeof(object) }),
                            Ast.RuntimeConstant(ReflectionCache.GetReflectedIndexer(pi)),
                            Instance
                        )
                    )
                );
            } else if (getter.ContainsGenericParameters) {
                MakeGenericPropertyError();
            } else if (getter.IsStatic) {
                MakeIncorrectArgumentCountError();
            } else if (getter.IsPublic) {
                // MakeCallStatement instead of Ast.ReadProperty because getter might
                // be a method on a base type after GetCallableMethod
                _body = Ast.Block(_body, MakeCallStatement(getter, Instance));
            } else {
                _body = Ast.Block(_body,
                    Rule.MakeReturn(
                        Binder,
                        Ast.Call(
                            Ast.RuntimeConstant(pi),
                            typeof(PropertyInfo).GetMethod("GetValue", new Type[] { typeof(object), typeof(object[]) }),
                            Instance,
                            Ast.NewArray(typeof(object[]))
                        )
                    )
                );
            }                
        }

        /// <summary> if a member-injector is defined-on or registered-for this type call it </summary>
        private void MakeOperatorGetMemberBodyOrError(Type type) {
            MethodInfo getMem = GetMethod(type, "GetBoundMember");
            if (getMem != null && getMem.IsSpecialName) {
                Variable tmp = Rule.GetTemporary(typeof(object), "getVal");
                _body = Ast.Block(_body,
                    Ast.If(
                        Ast.NotEqual(
                            Ast.Assign(
                                tmp,
                                MakeCallExpression(getMem, Instance, Ast.Constant(StringName))
                            ),
                            Ast.ReadField(null, typeof(DBNull).GetField("Value"))
                        ),
                        Rule.MakeReturn(Binder, Ast.Read(tmp))
                    )
                );
            }
            MakeMissingMemberError(type);

        }

        private void MakeCustomMembersBody(Type type) {
            Variable tmp = Rule.GetTemporary(typeof(object), "lookupRes");
            _body = Ast.Block(_body,
                        Ast.If(
                            Ast.Call(
                                Ast.Cast(Instance, typeof(ICustomMembers)),
                                typeof(ICustomMembers).GetMethod("TryGetBoundCustomMember"),
                                Ast.CodeContext(),
                                Ast.Constant(Action.Name),
                                Ast.Read(tmp)
                            ),
                            Rule.MakeReturn(Binder, Ast.Read(tmp))
                        )
                    );
            // if the lookup fails throw an exception
            MakeMissingMemberError(type);
        }

        /// <summary> Gets the Expression that represents the instance we're looking up </summary>
        private new Expression Instance {
            get {
                if (_instance != null) return _instance;

                return base.Instance;
            }
        }

        private static MemberInfo[] GetCallableMethods(MemberInfo[] members) {
            for (int i = 0; i < members.Length; i++) {
                members[i] = CompilerHelpers.GetCallableMethod((MethodInfo)members[i]);
            }
            return members;
        }

        #region Error rules

        private void MakeIncorrectArgumentCountError() {
            _body = Ast.Block(_body,
                Rule.MakeError(Binder,
                    MakeIncorrectArgumentExpression(0, 0)
                )
            );
        }

        private void MakeGenericPropertyError() {
            // TODO: Better exception
            _body = Ast.Block(_body, 
                Rule.MakeError(Binder,
                    MakeGenericPropertyExpression()
                )
            );
        }

        private void MakeMissingMemberError(Type type) {                
            _body = Ast.Block(_body,
                Rule.MakeError(
                    Binder,
                    Binder.MakeMissingMemberError(type, StringName)
                )
            );
        }
        
        #endregion

    }
}
