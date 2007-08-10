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
using System.Reflection;
using System.Diagnostics;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Types;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;

using IronPython.Runtime.Types;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Calls {
    class PythonGetMemberBinderHelper<T> : 
        BinderHelper<T, GetMemberAction> {

        public PythonGetMemberBinderHelper(CodeContext context, GetMemberAction action)
            : base(context, action) {
        }

        public StandardRule<T> MakeRule(object[] args) {
            if (args[0] == null) {
                return MakeDynamicRule(typeof(None));
            }

            Type type = CompilerHelpers.GetType(args[0]);
            if (PythonTypeCustomizer.IsPythonType(type)) {
                // look up in the Dynamictype so that we can 
                // get our custom method names (e.g. string.startswith)            
                DynamicType argType = DynamicHelpers.GetDynamicTypeFromType(type);
                DynamicTypeSlot dts;

                // first try in the default context and see if it's defined
                if (argType.TryResolveSlot(DefaultContext.Default, Action.Name, out dts)) {
                    return MakePythonTypeRule(args, dts, argType, false);
                } else if (argType.TryResolveSlot(DefaultContext.DefaultCLS, Action.Name, out dts)) {
                    // if it's not there but exists in the CLS context we'll add it but hide it.
                    return MakePythonTypeRule(args, dts, argType, true);                  
                }
            }

            return null;
        }

        /// <summary>
        /// Helper function which makes a Get Member rule for the given type.
        /// 
        /// This does not check if the object is unsuitable for producing a GetMember rule.
        /// </summary>
        internal bool TryMakeGetMemberRule(StandardRule<T> rule, DynamicType parent, DynamicTypeSlot slot, Expression arg) {
            ReflectedField rf;
            ReflectedProperty rp;
            ReflectedEvent ev;
            BuiltinFunction bf;
            ReflectedExtensionProperty rep;
            DynamicTypeValueSlot vs;
            BuiltinMethodDescriptor bmd;
            DynamicType dtValue;
            object value;

            GetMemberBinderHelper<T> helper = new GetMemberBinderHelper<T>(Context, Action, rule, !(slot is BuiltinFunction), arg);

            Statement body;
            if ((rf = slot as ReflectedField) != null) {
                body = helper.MakeMemberRuleTarget(parent.UnderlyingSystemType, rf.info);
            } else if ((rep = slot as ReflectedExtensionProperty) != null) {
                return TryMakePropertyGet(rule, rep.ExtInfo.Getter, arg);
            } else if ((rp = slot as ReflectedProperty) != null) {
                body = helper.MakeMemberRuleTarget(parent.UnderlyingSystemType, rp.Info);
            } else if ((ev = slot as ReflectedEvent) != null) {
                body = helper.MakeMemberRuleTarget(parent.UnderlyingSystemType, ev.Info);
            } else if ((bf = slot as BuiltinFunction) != null) {
                body = helper.MakeMemberRuleTarget(parent.UnderlyingSystemType, ReflectionUtils.GetMethodInfos(bf.Targets));
            } else if ((bmd = slot as BuiltinMethodDescriptor) != null) {
                body = helper.MakeMemberRuleTarget(parent.UnderlyingSystemType, ReflectionUtils.GetMethodInfos(bmd.Template.Targets));
            } else if ((vs = slot as DynamicTypeValueSlot) != null &&
                vs.TryGetValue(Context, null, null, out value) &&
                    ((dtValue = value as DynamicType) != null)) {

                Debug.Assert(dtValue.IsSystemType);
                body = helper.MakeMemberRuleTarget(parent.UnderlyingSystemType, dtValue.UnderlyingSystemType);
            } else {
                Variable tmp = rule.GetTemporary(typeof(object), "res");

                body = Ast.IfThenElse(
                        Ast.Call(
                            Ast.Cast(
                                Ast.WeakConstant(slot),
                                typeof(DynamicTypeSlot)
                            ),
                            typeof(DynamicTypeSlot).GetMethod("TryGetBoundValue"),
                            Ast.CodeContext(),
                            arg,
                            Ast.RuntimeConstant(parent),
                            Ast.Read(tmp)),
                        rule.MakeReturn(Binder, Ast.Read(tmp)),
                        MakeError(parent, rule)
                       );
            }

            rule.SetTarget(body);
            return true;
        }

        private StandardRule<T> MakePythonTypeRule(object[] args, DynamicTypeSlot slot, DynamicType argType, bool clsOnly) {
            StandardRule<T> rule = new StandardRule<T>();
            if (args[0] is ICustomMembers) {
                rule.SetTarget(UserTypeOps.MakeCustomMembersBody<T>(Context, Action, DynamicTypeOps.GetName(argType), rule));
                rule.MakeTest(argType);
                return rule;
            }
            
            if (TryMakeGetMemberRule(rule, argType, slot, rule.Parameters[0])) {
                rule.MakeTest(argType);

                if (clsOnly) {
                    rule.SetTarget(
                        Ast.Block(
                            Ast.IfThenElse(
                                Ast.Call(null,
                                    typeof(PythonOps).GetMethod("IsClsVisible"),
                                    Ast.CodeContext()
                                ),
                                rule.Target,
                                MakeError(argType, rule)
                            )
                        )
                    );                   
                }
                return rule;
            }
            return null;
        }

        private Statement MakeError(DynamicType argType, StandardRule<T> rule) {
            return rule.MakeError(Binder, Ast.Call(null,
                            typeof(PythonOps).GetMethod("AttributeErrorForMissingAttribute", new Type[] { typeof(string), typeof(SymbolId) }),
                            Ast.Constant(DynamicTypeOps.GetName(argType)),
                            Ast.Constant(Action.Name)
                        ));
        }

        private StandardRule<T> MakeDynamicRule(Type targetType) {
            StandardRule<T> rule = new StandardRule<T>();
            rule.SetTarget(rule.MakeReturn(Binder, Ast.Call(null,
                    typeof(RuntimeHelpers).GetMethod("GetBoundMember"),
                    Ast.CodeContext(),
                    rule.Parameters[0],
                    Ast.Constant(Action.Name))));
            rule.MakeTest(targetType);
            return rule;
        }

        private bool TryMakePropertyGet(StandardRule<T> rule, MethodInfo getter, params Expression[] args) {
            if (getter != null && CompilerHelpers.CanOptimizeMethod(getter)) {
                Statement call = MakeCallStatement(getter, args);
                if (call != null) {
                    rule.SetTarget(MakeCallStatement(getter, args));
                    return true;
                }
            }
            return false;
        }
    }
}
