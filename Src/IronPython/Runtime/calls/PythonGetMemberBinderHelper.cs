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
                // look up in the PythonType so that we can 
                // get our custom method names (e.g. string.startswith)            
                PythonType argType = DynamicHelpers.GetPythonTypeFromType(type);
                PythonTypeSlot dts;

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

        internal bool TryMakeGetMemberRule(PythonType parent, PythonTypeSlot slot, Expression arg) {
            return TryMakeGetMemberRule(parent, slot, arg, false);
        }

        /// <summary>
        /// Helper function which makes a Get Member rule for the given type.
        /// 
        /// This does not check if the object is unsuitable for producing a GetMember rule.
        /// </summary>
        internal bool TryMakeGetMemberRule(PythonType parent, PythonTypeSlot slot, Expression arg, bool clsOnly) {
            ReflectedField rf;
            ReflectedProperty rp;
            ReflectedEvent ev;
            BuiltinFunction bf;
            ReflectedExtensionProperty rep;
            PythonTypeValueSlot vs;
            BuiltinMethodDescriptor bmd;
            PythonType dtValue;
            object value;

            Instance = arg;

            MakeOperatorGetMemberBody(parent.UnderlyingSystemType, "GetCustomMember");

            if ((rf = slot as ReflectedField) != null) {
                AddToBody(AddClsCheck(parent, clsOnly, MakeMemberRuleTarget(parent.UnderlyingSystemType, rf.info)));
            } else if ((rep = slot as ReflectedExtensionProperty) != null) {
                Statement body;
                if (TryMakePropertyGet(rep.ExtInfo.Getter, arg, out body)) {
                    AddToBody(AddClsCheck(parent, clsOnly, body));
                    Rule.SetTarget(Body);
                    return true;
                }
                return false;
            } else if ((rp = slot as ReflectedProperty) != null) {
                AddToBody(AddClsCheck(parent, clsOnly, MakeMemberRuleTarget(parent.UnderlyingSystemType, rp.Info)));
            } else if ((ev = slot as ReflectedEvent) != null) {
                AddToBody(AddClsCheck(parent, clsOnly, MakeMemberRuleTarget(parent.UnderlyingSystemType, ev.Info)));
            } else if ((bf = slot as BuiltinFunction) != null) {
                AddToBody(AddClsCheck(parent, clsOnly, MakeMethodCallRule(bf, false)));
            } else if ((bmd = slot as BuiltinMethodDescriptor) != null) {
                AddToBody(AddClsCheck(parent, clsOnly, MakeMethodCallRule(bmd.Template, true)));
            } else if ((vs = slot as PythonTypeValueSlot) != null &&
                vs.TryGetValue(Context, null, null, out value) &&
                    ((dtValue = value as PythonType) != null)) {

                Debug.Assert(dtValue.IsSystemType);
                AddToBody(AddClsCheck(parent, clsOnly, MakeMemberRuleTarget(parent.UnderlyingSystemType, dtValue.UnderlyingSystemType)));
            } else {
                Variable tmp = Rule.GetTemporary(typeof(object), "res");

                AddToBody(
                        AddClsCheck(
                            parent, 
                            clsOnly, 
                            Ast.IfThenElse(
                                Ast.Call(
                                    typeof(PythonOps).GetMethod("SlotTryGetBoundValue"),
                                    Ast.CodeContext(),
                                    Ast.Convert(
                                        Ast.WeakConstant(slot),
                                        typeof(PythonTypeSlot)
                                    ),
                                    Ast.ConvertHelper(arg, typeof(object)),
                                    Ast.ConvertHelper(Ast.RuntimeConstant(parent), typeof(PythonType)),
                                    Ast.Read(tmp)
                                ),
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
                    Ast.Call(typeof(PythonOps).GetMethod("MakeBoundBuiltinFunction"),
                        Ast.RuntimeConstant(target),
                        Ast.ConvertHelper(Instance, typeof(object))
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
            BuiltinFunction cachedTarget = PythonTypeOps.GetBuiltinFunction(target.DeclaringType, target.Name, ArrayUtils.ToArray<MethodBase>(target.Targets));
            if (cachedTarget.FunctionType == (target.FunctionType & ~FunctionType.AlwaysVisible)) {
                target = cachedTarget;
            }
            return target;
        }

        private StandardRule<T> MakePythonTypeRule(PythonTypeSlot slot, PythonType argType, bool clsOnly) {
            if (Arguments[0] is ICustomMembers) {
                Rule.SetTarget(UserTypeOps.MakeCustomMembersGetBody<T>(Context, Action, PythonTypeOps.GetName(argType), Rule));
                PythonBinderHelper.MakeTest(Rule, argType);
                return Rule;
            }
                       
            if (TryMakeGetMemberRule(argType, slot, Rule.Parameters[0], clsOnly)) {
                PythonBinderHelper.MakeTest(Rule, argType);                
                return Rule;
            }
                                   
            return null;
        }

        private Statement AddClsCheck(PythonType argType, bool clsOnly, Statement body) {
            if (clsOnly) {
                return
                    Ast.Block(
                        Ast.IfThenElse(
                            Ast.Call(
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

        private Statement MakeError(PythonType argType) {
            if (Action.IsNoThrow) {
                return Rule.MakeReturn(Binder, Ast.ReadField(null, typeof(OperationFailed).GetField("Value")));
            } else {
                return Rule.MakeError(
                    Ast.Call(
                        typeof(PythonOps).GetMethod("AttributeErrorForMissingAttribute", new Type[] { typeof(string), typeof(SymbolId) }),
                        Ast.Constant(PythonTypeOps.GetName(argType), typeof(string)),
                        Ast.Constant(Action.Name)
                    )
                );
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
