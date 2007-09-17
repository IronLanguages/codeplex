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
    class PythonGetMemberBinderHelper<T> : GetMemberBinderHelper<T> {

        public PythonGetMemberBinderHelper(CodeContext context, GetMemberAction action, object []args)
            : base(context, action, args) {
        }

        public StandardRule<T> MakeRule() {
            Type type = CompilerHelpers.GetType(Arguments[0]);
            // we extend None & our standard built-in python types.  DLR doesn't support COM objects natively yet.
            if (type == typeof(None) || PythonTypeCustomizer.IsPythonType(type) || type.IsCOMObject) {
                // look up in the Dynamictype so that we can 
                // get our custom method names (e.g. string.startswith)            
                DynamicType argType = DynamicHelpers.GetDynamicTypeFromType(type);
                DynamicTypeSlot dts;

                // first try in the default context and see if it's defined
                if (argType.TryResolveSlot(DefaultContext.Default, Action.Name, out dts)) {
                    return MakePythonTypeRule(dts, argType, false);
                } else if (argType.TryResolveSlot(DefaultContext.DefaultCLS, Action.Name, out dts)) {
                    // if it's not there but exists in the CLS context we'll add it but hide it.
                    return MakePythonTypeRule(dts, argType, true);                  
                }
            }

            return null;
        }

        internal bool TryMakeGetMemberRule(DynamicType parent, DynamicTypeSlot slot, Expression arg) {
            return TryMakeGetMemberRule(parent, slot, arg, false);
        }

        /// <summary>
        /// Helper function which makes a Get Member rule for the given type.
        /// 
        /// This does not check if the object is unsuitable for producing a GetMember rule.
        /// </summary>
        internal bool TryMakeGetMemberRule(DynamicType parent, DynamicTypeSlot slot, Expression arg, bool clsOnly) {
            ReflectedField rf;
            ReflectedProperty rp;
            ReflectedEvent ev;
            BuiltinFunction bf;
            ReflectedExtensionProperty rep;
            DynamicTypeValueSlot vs;
            BuiltinMethodDescriptor bmd;
            DynamicType dtValue;
            object value;

            Instance = arg;

            MakeOperatorGetMemberBody(parent.UnderlyingSystemType, "GetCustomMember");

            if ((rf = slot as ReflectedField) != null) {
                Body = Ast.Block(Body, AddClsCheck(parent, clsOnly, MakeMemberRuleTarget(parent.UnderlyingSystemType, rf.info)));
            } else if ((rep = slot as ReflectedExtensionProperty) != null) {
                Statement body;
                if (TryMakePropertyGet(rep.ExtInfo.Getter, arg, out body)) {
                    Body = Ast.Block(Body, AddClsCheck(parent, clsOnly, body));
                    Rule.SetTarget(Body);
                    return true;
                }
                return false;
            } else if ((rp = slot as ReflectedProperty) != null) {
                Body = Ast.Block(Body, AddClsCheck(parent, clsOnly, MakeMemberRuleTarget(parent.UnderlyingSystemType, rp.Info)));
            } else if ((ev = slot as ReflectedEvent) != null) {
                Body = Ast.Block(Body, AddClsCheck(parent, clsOnly, MakeMemberRuleTarget(parent.UnderlyingSystemType, ev.Info)));
            } else if ((bf = slot as BuiltinFunction) != null) {
                Body = Ast.Block(Body, AddClsCheck(parent, clsOnly, MakeMethodCallRule(bf, false)));
            } else if ((bmd = slot as BuiltinMethodDescriptor) != null) {
                Body = Ast.Block(Body, AddClsCheck(parent, clsOnly, MakeMethodCallRule(bmd.Template, true)));
            } else if ((vs = slot as DynamicTypeValueSlot) != null &&
                vs.TryGetValue(Context, null, null, out value) &&
                    ((dtValue = value as DynamicType) != null)) {

                Debug.Assert(dtValue.IsSystemType);
                Body = Ast.Block(Body, AddClsCheck(parent, clsOnly, MakeMemberRuleTarget(parent.UnderlyingSystemType, dtValue.UnderlyingSystemType)));
            } else {
                Variable tmp = Rule.GetTemporary(typeof(object), "res");

                Body = Ast.Block(Body, 
                        AddClsCheck(
                            parent, 
                            clsOnly, 
                            Ast.IfThenElse(
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
                                Rule.MakeReturn(Binder, Ast.Read(tmp)),
                                MakeError(parent)
                            )
                        )
                    );
            }

            MakeOperatorGetMemberBody(parent.UnderlyingSystemType, "GetBoundMember");

            Rule.SetTarget(Body);
            return true;
        }

        private Statement MakeMethodCallRule(BuiltinFunction target, bool bound) {
            target = GetCachedTarget(target);

            if (((target.FunctionType & FunctionType.FunctionMethodMask) != FunctionType.Function) || bound) {
                // for strong box we need to bind the strong box, so we don't use Instance here.
                return Ast.Block(Body,
                    Rule.MakeReturn(
                    Binder,
                    Ast.New(typeof(BoundBuiltinFunction).GetConstructor(new Type[] { typeof(BuiltinFunction), typeof(object) }),
                        Ast.RuntimeConstant(target),
                        Instance
                    )
                ));
            } else {
                return Ast.Block(Body,
                    Rule.MakeReturn(
                        Binder,
                        Ast.RuntimeConstant(target)
                    )
                );
            }
        }

        /// <summary>
        /// helper to ensure we always hand out the same BuiltinFunction as the base GetMemberBinderHelper.
        /// 
        /// Because ReflectionCache can't always detect reverse methods we only use the cached version if it's
        /// the same FunctionType.
        /// </summary>
        private static BuiltinFunction GetCachedTarget(BuiltinFunction target) {
            BuiltinFunction cachedTarget = ReflectionCache.GetMethodGroup(target.DeclaringType, target.Name, target.Targets);
            if (cachedTarget.FunctionType == (target.FunctionType & ~FunctionType.AlwaysVisible)) {
                target = cachedTarget;
            }
            return target;
        }

        private StandardRule<T> MakePythonTypeRule(DynamicTypeSlot slot, DynamicType argType, bool clsOnly) {
            if (Arguments[0] is ICustomMembers) {
                Rule.SetTarget(UserTypeOps.MakeCustomMembersGetBody<T>(Context, Action, DynamicTypeOps.GetName(argType), Rule));
                PythonBinderHelper.MakeTest(Rule, argType);
                return Rule;
            }
                       
            if (TryMakeGetMemberRule(argType, slot, Rule.Parameters[0], clsOnly)) {
                PythonBinderHelper.MakeTest(Rule, argType);                
                return Rule;
            }
                                   
            return null;
        }

        private Statement AddClsCheck(DynamicType argType, bool clsOnly, Statement body) {
            if (clsOnly) {
                return
                    Ast.Block(
                        Ast.IfThenElse(
                            Ast.Call(null,
                                typeof(PythonOps).GetMethod("IsClsVisible"),
                                Ast.CodeContext()
                            ),
                            body,
                            MakeError(argType)
                        )
                    );
            }
            return body;
        }

        private Statement MakeError(DynamicType argType) {
            if (Action.IsNoThrow) {
                return Rule.MakeReturn(Binder, Ast.ReadField(null, typeof(OperationFailed).GetField("Value")));
            } else {
                return Rule.MakeError(Binder, Ast.Call(null,
                    typeof(PythonOps).GetMethod("AttributeErrorForMissingAttribute", new Type[] { typeof(string), typeof(SymbolId) }),
                    Ast.Constant(DynamicTypeOps.GetName(argType)),
                    Ast.Constant(Action.Name)
                ));
            }
        }

        private bool TryMakePropertyGet(MethodInfo getter, Expression arg, out Statement body) {
            if (getter != null && CompilerHelpers.CanOptimizeMethod(getter)) {
                body = MakeCallStatement(getter, arg);
                if (body != null) {
                    return true;
                }
            }
            body = null;
            return false;
        }

        internal StandardRule<T> InternalRule {
            get {
                return Rule;
            }
        }
    }
}
