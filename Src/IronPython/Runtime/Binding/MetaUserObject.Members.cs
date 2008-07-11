/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Diagnostics;
using System.Scripting;
using System.Scripting.Actions;
using System.Linq.Expressions;
using System.Scripting.Runtime;

using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

using Utils = Microsoft.Scripting.Ast.Utils;

namespace IronPython.Runtime.Binding {
    using Ast = System.Linq.Expressions.Expression;
    using Microsoft.Scripting.Generation;

    partial class MetaUserObject : MetaPythonObject {
        
        #region MetaObject Overrides

        public override MetaObject/*!*/ GetMember(GetMemberAction/*!*/ action, MetaObject/*!*/[]/*!*/ args) {
            MetaObject self = Restrict(Value.GetType());
            CodeContext context = BinderState.GetBinderState(action).Context;
            IPythonObject sdo = (IPythonObject)args[0].Value;
            GetBindingInfo bindingInfo = new GetBindingInfo(
                action,
                args,
                Ast.Variable(Expression.Type, "self"),
                Ast.Variable(typeof(object), "lookupRes"),
                new ConditionalBuilder(action),
                BindingHelpers.GetValidationInfo(self.Expression, sdo.PythonType)
            );

            PythonTypeSlot dts;
            if (TryGetGetAttribute(context, sdo.PythonType, out dts)) {
                return MakeGetAttributeRule(bindingInfo, sdo, dts);
            }

            // otherwise look the object according to Python rules:
            //  1. 1st search the MRO of the type, and if it's there, and it's a get/set descriptor,
            //      return that value.
            //  2. Look in the instance dictionary.  If it's there return that value, otherwise return
            //      a value found during the MRO search.  If no value was found during the MRO search then
            //      raise an exception.      
            //  3. fall back to __getattr__ if defined.
            //
            // Ultimately we cache the result of the MRO search based upon the type version.  If we have
            // a get/set descriptor we'll only ever use that directly.  Otherwise if we have a get descriptor
            // we'll first check the dictionary and then invoke the get descriptor.  If we have no descriptor
            // at all we'll just check the dictionary.  If both lookups fail we'll raise an exception.

            bool isOldStyle = false;                // if we're mixed new-style/old-style we have to do a slower check
            bool systemTypeResolution = false;      // if we pick up the property from a System type we fallback
            foreach (PythonType pt in sdo.PythonType.ResolutionOrder) {
                if (pt.IsOldClass) {
                    isOldStyle = true;
                }

                if (pt.TryLookupSlot(context, SymbolTable.StringToId(action.Name), out dts)) {
                    // use our built-in binding for ClassMethodDescriptors rather than falling back
                    if (!(dts is ClassMethodDescriptor)) {
                        systemTypeResolution = pt.IsSystemType;
                    }
                    break;
                }
            }

            if (!isOldStyle || dts is ReflectedSlotProperty) {
                if (sdo.HasDictionary && (dts == null || !dts.IsSetDescriptor(context, sdo.PythonType))) {
                    MakeDictionaryAccess(bindingInfo);
                }

                if (dts != null) {
                    if (systemTypeResolution) {
                        bindingInfo.Body.FinishCondition(Fallback(action, args).Expression);
                    } else {
                        MakeSlotAccess(bindingInfo, dts);
                    }
                }
            } else {
                MakeOldStyleAccess(bindingInfo);
            }

            if (!bindingInfo.Body.IsFinal) {
                // fall back to __getattr__ if it's defined.
                PythonTypeSlot getattr;
                if (sdo.PythonType.TryResolveSlot(context, Symbols.GetBoundAttr, out getattr)) {
                    MakeGetAttrRule(bindingInfo, GetWeakSlot(getattr));
                }

                bindingInfo.Body.FinishCondition(FallbackGetError(action, args).Expression);
            }

            MetaObject res = bindingInfo.Body.GetMetaObject(args);
            res = new MetaObject(
                Ast.Scope(
                    Ast.Comma(
                        Ast.Assign(bindingInfo.Self, self.Expression),
                        res.Expression
                    ),
                    bindingInfo.Self,
                    bindingInfo.Result
                ),
                self.Restrictions.Merge(res.Restrictions)
            );

            return BindingHelpers.AddDynamicTestAndDefer(
                action,
                res,
                args,
                bindingInfo.Validation
            );
        }

        public override MetaObject/*!*/ SetMember(SetMemberAction/*!*/ action, MetaObject/*!*/[]/*!*/ args) {
            MetaObject self = Restrict(Value.GetType());
            CodeContext context = BinderState.GetBinderState(action).Context;
            IPythonObject sdo = args[0].Value as IPythonObject;
            SetBindingInfo bindingInfo = new SetBindingInfo(
                action,
                args,
                new ConditionalBuilder(action),
                BindingHelpers.GetValidationInfo(self.Expression, sdo.PythonType)
            );

            MetaObject res = null;
            // call __setattr__ if it exists
            PythonTypeSlot dts;
            if (sdo.PythonType.TryResolveSlot(context, Symbols.SetAttr, out dts) && !IsStandardObjectMethod(dts)) {
                // skip the fake __setattr__ on mixed new-style/old-style types
                if (dts != null) {
                    MakeSetAttrTarget(bindingInfo, sdo, dts);
                    res = bindingInfo.Body.GetMetaObject(args);
                }
            }

            if (res == null) {
                // then see if we have a set descriptor
                bool systemTypeResolution = false;      // if we pick up the property from a System type we fallback
                foreach (PythonType pt in sdo.PythonType.ResolutionOrder) {
                    if (pt.TryLookupSlot(context, SymbolTable.StringToId(action.Name), out dts)) {
                        systemTypeResolution = pt.IsSystemType;
                        break;
                    }
                }

                sdo.PythonType.TryResolveSlot(context, SymbolTable.StringToId(action.Name), out dts);
                ReflectedSlotProperty rsp = dts as ReflectedSlotProperty;
                if (rsp != null) {
                    MakeSlotsSetTarget(bindingInfo, rsp, args[1].Expression);
                    res = bindingInfo.Body.GetMetaObject(args);
                } else if (dts != null && dts.IsSetDescriptor(context, sdo.PythonType)) {
                    if (systemTypeResolution) {
                        res = Fallback(action, args);
                    } else {
                        MakeSlotSet(bindingInfo, dts);
                    }
                }
            }

            if (res == null) {
                // finally if we have a dictionary set the value there.
                if (sdo.HasDictionary) {
                    MakeDictionarySetTarget(bindingInfo);
                } else {
                    bindingInfo.Body.FinishCondition(
                        FallbackSetError(action, args).Expression
                    );
                }

                res = bindingInfo.Body.GetMetaObject(args);
            }

            res = new MetaObject(
                res.Expression,
                self.Restrictions.Merge(res.Restrictions)
            );

            return BindingHelpers.AddDynamicTestAndDefer(
                action,
                res,
                args,
                bindingInfo.Validation
            );
        }

        public override MetaObject/*!*/ DeleteMember(DeleteMemberAction/*!*/ action, MetaObject/*!*/[]/*!*/ args) {
            IPythonObject sdo = args[0].Value as IPythonObject;

            return MakeDeleteMemberRule(
                new DeleteBindingInfo(
                    action,
                    args,
                    new ConditionalBuilder(action),
                    BindingHelpers.GetValidationInfo(Expression, sdo.PythonType)
                )
            );
        }

        #endregion

        #region Get Member Helpers

        /// <summary>
        /// Checks to see if this type has __getattribute__ that overrides all other attribute lookup.
        /// 
        /// This is more complex then it needs to be.  The problem is that when we have a 
        /// mixed new-style/old-style class we have a weird __getattribute__ defined.  When
        /// we always dispatch through rules instead of PythonTypes it should be easy to remove
        /// this.
        /// </summary>
        private static bool TryGetGetAttribute(CodeContext/*!*/ context, PythonType/*!*/ type, out PythonTypeSlot dts) {
            if (type.TryResolveSlot(context, Symbols.GetAttribute, out dts)) {
                BuiltinMethodDescriptor bmd = dts as BuiltinMethodDescriptor;

                if (bmd == null || bmd.DeclaringType != typeof(object) ||
                    bmd.Template.Targets.Count != 1 ||
                    bmd.Template.Targets[0].DeclaringType != typeof(ObjectOps) ||
                    bmd.Template.Targets[0].Name != "__getattribute__") {
                    return dts != null;
                }
            }
            return false;
        }

        private static void MakeDictionaryAccess(GetBindingInfo/*!*/ info) {
            info.Body.AddCondition(
                Ast.AndAlso(
                    Ast.NotEqual(
                        Ast.Property(
                            Ast.Convert(info.Self, typeof(IPythonObject)),
                            TypeInfo._IPythonObject.Dict
                        ),
                        Ast.Constant(null)
                    ),
                    Ast.Call(
                        Ast.Property(
                            Ast.Convert(info.Self, typeof(IPythonObject)),
                            TypeInfo._IPythonObject.Dict
                        ),
                        TypeInfo._IAttributesCollection.TryGetvalue,
                        Ast.Constant(SymbolTable.StringToId(info.Action.Name)),
                        info.Result
                    )
                ),
                info.Result
            );
        }

        private static void MakeSlotAccess(GetBindingInfo/*!*/ info, PythonTypeSlot dts) {
            ReflectedSlotProperty rsp = dts as ReflectedSlotProperty;
            if (rsp != null) {
                // we need to fall back to __getattr__ if the value is not defined, so call it and check the result.
                info.Body.AddCondition(
                    Ast.NotEqual(
                        Ast.Assign(
                            info.Result,
                            Ast.SimpleCallHelper(
                                rsp.GetterMethod,
                                info.Self
                            )
                        ),
                        Ast.Field(null, typeof(Uninitialized).GetField("Instance"))
                    ),
                    info.Result
                );
                return;
            }

            PythonTypeUserDescriptorSlot slot = dts as PythonTypeUserDescriptorSlot;
            if (slot != null && !(slot.Value is PythonTypeSlot)) {
                PythonType slottype = DynamicHelpers.GetPythonType(slot.Value);
                if (slottype.IsSystemType) {
                    // this is a user slot that's known not to be a descriptor
                    // so we can just burn the value in.  For it to change the
                    // slot will need to be replaced reving the type version.
                    info.Body.FinishCondition(
                        Ast.ConvertHelper(Utils.WeakConstant(slot.Value), typeof(object))
                    );
                    return;
                }
            }

            Expression tryGet = Ast.Call(
                TypeInfo._PythonOps.SlotTryGetBoundValue,
                Ast.Constant(BinderState.GetBinderState(info.Action).Context),
                Ast.Convert(Utils.WeakConstant(dts), typeof(PythonTypeSlot)),
                Ast.ConvertHelper(info.Self, typeof(object)),
                Ast.Property(
                    Ast.Convert(
                        info.Self,
                        typeof(IPythonObject)),
                    TypeInfo._IPythonObject.PythonType
                ),
                info.Result
            );

            if (dts.GetAlwaysSucceeds) {
                info.Body.FinishCondition(
                    Ast.Comma(tryGet, info.Result)
                );
            } else {
                info.Body.AddCondition(
                    tryGet,
                    info.Result
                );
            }
        }

        /// <summary>
        /// Checks a range of the MRO to perform old-style class lookups if any old-style classes
        /// are present.  We will call this twice to produce a search before a slot and after
        /// a slot.
        /// </summary>
        private static void MakeOldStyleAccess(GetBindingInfo/*!*/ info) {
            info.Body.AddCondition(
                Ast.Call(
                    typeof(UserTypeOps).GetMethod("TryGetMixedNewStyleOldStyleSlot"),
                    Ast.Constant(BinderState.GetBinderState(info.Action).Context),
                    Ast.ConvertHelper(info.Self, typeof(object)),
                    Ast.Constant(SymbolTable.StringToId(info.Action.Name)),
                    info.Result
                ),
                info.Result
            );
        }
        
        private void MakeGetAttrRule(GetBindingInfo/*!*/ info, Expression/*!*/ getattr) {
            info.Body.AddCondition(
                MakeGetAttrTestAndGet(info, getattr),
                MakeGetAttrCall(info)
            );
        }

        private static MethodCallExpression/*!*/ MakeGetAttrTestAndGet(GetBindingInfo/*!*/ info, Expression/*!*/ getattr) {
            return Ast.Call(
                TypeInfo._PythonOps.SlotTryGetBoundValue,
                Ast.Constant(BinderState.GetBinderState(info.Action).Context),
                Ast.ConvertHelper(getattr, typeof(PythonTypeSlot)),
                Ast.ConvertHelper(info.Self, typeof(object)),
                Ast.Convert(
                    Ast.Property(
                        Ast.Convert(
                            info.Self,
                            typeof(IPythonObject)),
                        TypeInfo._IPythonObject.PythonType
                    ),
                    typeof(PythonType)
                ),
                info.Result
            );
        }

        private Expression/*!*/ MakeGetAttrCall(GetBindingInfo/*!*/ info) {
            Expression call = Ast.ActionExpression(
                new InvokeBinder(
                    BinderState.GetBinderState(info.Action),
                    new CallSignature(1)
                ),
                typeof(object),
                info.Result,
                Ast.Constant(info.Action.Name)
            );

            call = MaybeMakeNoThrow(info, call);

            return call;
        }

        private Expression/*!*/ MaybeMakeNoThrow(GetBindingInfo/*!*/ info, Expression/*!*/ expr) {
            if (BindingHelpers.IsNoThrow(info.Action)) {
                MetaObject fallback = FallbackGetError(info.Action, info.Args);
                Type t = BindingHelpers.GetCompatibleType(expr.Type, fallback.Expression.Type);
                VariableExpression tmp = Ast.Variable(t, "getAttrRes");

                expr = Ast.Scope(
                    Ast.Comma(
                        Ast.Try(
                            Ast.Assign(tmp, Ast.ConvertHelper(expr, t))
                        ).Catch(
                            typeof(MissingMemberException),
                            Ast.Assign(tmp, Ast.ConvertHelper(FallbackGetError(info.Action, info.Args).Expression, t))
                        ),
                        tmp
                    ),
                    tmp
                );
            }
            return expr;
        }

        /// <summary>
        /// Makes a rule which calls a user-defined __getattribute__ function and falls back to __getattr__ if that
        /// raises an AttributeError.
        /// 
        /// slot is the __getattribute__ method to be called.
        /// </summary>
        private MetaObject/*!*/ MakeGetAttributeRule(GetBindingInfo/*!*/ info, IPythonObject/*!*/ obj, PythonTypeSlot/*!*/ slot) {
            // if the type implements IDynamicObject and we picked up it's __getattribute__ then we want to just 
            // dispatch to the base meta object (or to the default binder). an example of this is:
            //
            // class mc(type):
            //     def __getattr__(self, name):
            //          return 42
            //
            // class nc_ga(object):
            //     __metaclass__ = mc
            //
            // a = nc_ga.x # here we want to dispatch to the type's rule, not call __getattribute__ directly.

            CodeContext context = BinderState.GetBinderState(info.Action).Context;
            Type finalType = PythonTypeOps.GetFinalSystemType(obj.PythonType.UnderlyingSystemType);
            if (typeof(IDynamicObject).IsAssignableFrom(finalType)) {
                PythonTypeSlot baseSlot;
                if (TryGetGetAttribute(context, DynamicHelpers.GetPythonTypeFromType(finalType), out baseSlot) && baseSlot == slot) {
                    return Fallback(info.Action, info.Args);
                }
            }
            
            // otherwise generate code into a helper function.  This will do the slot lookup and exception
            // handling for both __getattribute__ as well as __getattr__ if it exists.
            PythonTypeSlot getattr;
            obj.PythonType.TryResolveSlot(context, Symbols.GetBoundAttr, out getattr);
            MetaObject self = Restrict(Value.GetType());
            string methodName = BindingHelpers.IsNoThrow(info.Action) ? "GetAttributeNoThrow" : "GetAttribute";
            
            return BindingHelpers.AddDynamicTestAndDefer(
                info.Action,
                new MetaObject(
                    Ast.Call(
                        typeof(UserTypeOps).GetMethod(methodName),
                        Ast.Constant(BinderState.GetBinderState(info.Action).Context),
                        info.Args[0].Expression,
                        Ast.Constant(info.Action.Name),
                        Ast.Constant(slot, typeof(PythonTypeSlot)),
                        Ast.Constant(getattr, typeof(PythonTypeSlot)),
                        Ast.Constant(new SiteLocalStorage<CallSite<DynamicSiteTarget<object, string, object>>>())
                    ),
                    self.Restrictions
                ),
                info.Args,
                info.Validation
            );
        }

        private static Expression/*!*/ GetWeakSlot(PythonTypeSlot slot) {
            return Ast.ConvertHelper(Utils.WeakConstant(slot), typeof(PythonTypeSlot));
        }

        private static Expression/*!*/ MakeTypeError(string/*!*/ name, PythonType/*!*/ type) {
            return Ast.Throw(
                Ast.Call(
                    typeof(PythonOps).GetMethod("AttributeErrorForMissingAttribute", new Type[] { typeof(string), typeof(SymbolId) }),
                    Ast.Constant(type.Name),
                    Ast.Constant(SymbolTable.StringToId(name))
                )
            );
        }

        #endregion

        #region Set Member Helpers

        private static bool IsStandardObjectMethod(PythonTypeSlot dts) {
            BuiltinMethodDescriptor bmd = dts as BuiltinMethodDescriptor;
            if (bmd == null) return false;
            return bmd.Template.Targets[0].DeclaringType == typeof(ObjectOps);
        }

        private void MakeSetAttrTarget(SetBindingInfo bindingInfo, IPythonObject sdo, PythonTypeSlot dts) {
            VariableExpression tmp = Ast.Variable(typeof(object), "boundVal");
            bindingInfo.Body.AddVariable(tmp);

            bindingInfo.Body.AddCondition(
                Ast.Call(
                    typeof(PythonOps).GetMethod("SlotTryGetValue"),
                    Ast.Constant(BinderState.GetBinderState(bindingInfo.Action).Context),
                    Ast.ConvertHelper(Utils.WeakConstant(dts), typeof(PythonTypeSlot)),
                    Ast.ConvertHelper(bindingInfo.Args[0].Expression, typeof(object)),
                    Ast.ConvertHelper(Utils.WeakConstant(sdo.PythonType), typeof(PythonType)),
                    tmp
                ),
                Ast.ActionExpression(
                    new InvokeBinder(
                        BinderState.GetBinderState(bindingInfo.Action),
                        new CallSignature(2)
                    ),
                    typeof(object),
                    tmp,
                    Ast.Constant(bindingInfo.Action.Name),
                    bindingInfo.Args[1].Expression
                )
            );

            bindingInfo.Body.FinishCondition(
                FallbackSetError(bindingInfo.Action, bindingInfo.Args).Expression
            );
        }

        private static void MakeSlotsDeleteTarget(MemberBindingInfo/*!*/ info, ReflectedSlotProperty/*!*/ rsp) {
            MakeSlotsSetTarget(info, rsp, Ast.Field(null, typeof(Uninitialized).GetField("Instance")));
        }

        private static void MakeSlotsSetTarget(MemberBindingInfo/*!*/ info, ReflectedSlotProperty/*!*/ rsp, Expression/*!*/ value) {
            // type has __slots__ defined for this member, call the setter directly
            VariableExpression tmp = Ast.Variable(typeof(object), "res");
            info.Body.AddVariable(tmp);

            info.Body.FinishCondition(
                Ast.Comma(
                    Ast.Assign(
                        tmp,
                        Ast.Convert(
                            Ast.SimpleCallHelper(rsp.SetterMethod, info.Args[0].Expression, value),
                            tmp.Type
                        )
                    ),
                    tmp
                )
            );
        }


        private static void MakeSlotSet(SetBindingInfo/*!*/ info, PythonTypeSlot/*!*/ dts) {
            VariableExpression tmp = Ast.Variable(info.Args[1].Expression.Type, "res");
            info.Body.AddVariable(tmp);

            CodeContext context = BinderState.GetBinderState(info.Action).Context;
            Debug.Assert(context != null);

            info.Body.AddCondition(
                Ast.Comma(
                    Ast.Assign(tmp, info.Args[1].Expression),
                    Ast.Call(
                        typeof(PythonOps).GetMethod("SlotTrySetValue"),
                        Ast.Constant(context),
                        Ast.ConvertHelper(Utils.WeakConstant(dts), typeof(PythonTypeSlot)),
                        Ast.ConvertHelper(info.Args[0].Expression, typeof(object)),
                        Ast.Convert(
                            Ast.Property(
                                Ast.Convert(
                                    info.Args[0].Expression,
                                    typeof(IPythonObject)),
                                TypeInfo._IPythonObject.PythonType
                            ),
                            typeof(PythonType)
                        ),
                        Ast.ConvertHelper(tmp, typeof(object))
                    )
                ),
                tmp
            );
        }

        private static void MakeDictionarySetTarget(SetBindingInfo/*!*/ info) {
            // return UserTypeOps.SetDictionaryValue(rule.Parameters[0], name, value);
            info.Body.FinishCondition(
                Ast.Call(
                    typeof(UserTypeOps).GetMethod("SetDictionaryValue"),
                    Ast.Convert(info.Args[0].Expression, typeof(IPythonObject)),
                    Ast.Constant(SymbolTable.StringToId(info.Action.Name)),
                    Ast.ConvertHelper(info.Args[1].Expression, typeof(object))
                )
            );
        }

        #endregion

        #region Delete Member Helpers

        private MetaObject/*!*/ MakeDeleteMemberRule(DeleteBindingInfo/*!*/ info) {
            CodeContext context = BinderState.GetBinderState(info.Action).Context;
            MetaObject self = info.Args[0].Restrict(info.Args[0].RuntimeType);

            IPythonObject sdo = info.Args[0].Value as IPythonObject;
            if (info.Action.Name == "__class__") {
                return new MetaObject(
                    Ast.Throw(
                        Ast.New(
                            typeof(ArgumentTypeException).GetConstructor(new Type[] { typeof(string) }),
                            Ast.Constant("can't delete __class__ attribute")
                        )
                    ),
                    self.Restrictions
                );
            }

            // call __delattr__ if it exists
            PythonTypeSlot dts;
            if (sdo.PythonType.TryResolveSlot(context, Symbols.DelAttr, out dts) && !IsStandardObjectMethod(dts)) {
                MakeDeleteAttrTarget(info, sdo, dts);
            }

            // then see if we have a delete descriptor
            sdo.PythonType.TryResolveSlot(context, SymbolTable.StringToId(info.Action.Name), out dts);
            ReflectedSlotProperty rsp = dts as ReflectedSlotProperty;
            if (rsp != null) {
                MakeSlotsDeleteTarget(info, rsp);
            }
            
            if (!info.Body.IsFinal && dts != null) {
                MakeSlotDelete(info, dts);
            }

            if (!info.Body.IsFinal && sdo.HasDictionary) {
                // finally if we have a dictionary set the value there.
                MakeDictionaryDeleteTarget(info);
            }

            if (!info.Body.IsFinal) {
                // otherwise fallback
                info.Body.FinishCondition(
                    FallbackDeleteError(info.Action, info.Args).Expression
                );
            }

            MetaObject res = info.Body.GetMetaObject(info.Args);

            res = new MetaObject(
                res.Expression,
                self.Restrictions.Merge(res.Restrictions)
            );

            return BindingHelpers.AddDynamicTestAndDefer(
                info.Action,
                res,
                info.Args,
                info.Validation
            );

        }

        private static void MakeSlotDelete(DeleteBindingInfo/*!*/ info, PythonTypeSlot/*!*/ dts) {
            info.Body.AddCondition(
                Ast.Call(
                    typeof(PythonOps).GetMethod("SlotTryDeleteValue"),
                    Ast.Constant(BinderState.GetBinderState(info.Action).Context),
                    Ast.ConvertHelper(Utils.WeakConstant(dts), typeof(PythonTypeSlot)),
                    Ast.ConvertHelper(info.Args[0].Expression, typeof(object)),
                    Ast.Convert(
                        Ast.Property(
                            Ast.Convert(
                                info.Args[0].Expression,
                                typeof(IPythonObject)),
                            TypeInfo._IPythonObject.PythonType
                        ),
                        typeof(PythonType)
                    )
                ),
                Ast.Null()
            );
        }

        private static void MakeDeleteAttrTarget(DeleteBindingInfo/*!*/ info, IPythonObject self, PythonTypeSlot dts) {
            VariableExpression tmp = Ast.Variable(typeof(object), "boundVal");
            info.Body.AddVariable(tmp);

            // call __delattr__
            info.Body.AddCondition(
                Ast.Call(
                    TypeInfo._PythonOps.SlotTryGetBoundValue,
                    Ast.Constant(BinderState.GetBinderState(info.Action).Context),
                    Ast.ConvertHelper(Utils.WeakConstant(dts), typeof(PythonTypeSlot)),
                    Ast.ConvertHelper(info.Args[0].Expression, typeof(object)),
                    Ast.ConvertHelper(Utils.WeakConstant(self.PythonType), typeof(PythonType)),
                    tmp
                ),
                Ast.ActionExpression(
                    new InvokeBinder(
                        BinderState.GetBinderState(info.Action),
                        new CallSignature(1)
                    ),
                    typeof(object),
                    tmp,
                    Ast.Constant(info.Action.Name)
                )
            );
        }

        private static void MakeDictionaryDeleteTarget(DeleteBindingInfo/*!*/ info) {
            info.Body.FinishCondition(
                Ast.Call(
                    typeof(UserTypeOps).GetMethod("RemoveDictionaryValue"),
                    Ast.Convert(info.Args[0].Expression, typeof(IPythonObject)),
                    Ast.Constant(SymbolTable.StringToId(info.Action.Name))
                )
            );
        }

        #endregion

        #region BindingInfo classes

        class MemberBindingInfo {
            public readonly ConditionalBuilder/*!*/ Body;
            public readonly MetaObject/*!*/[]/*!*/ Args;
            public readonly ValidationInfo/*!*/ Validation;

            public MemberBindingInfo(MetaObject/*!*/[]/*!*/ args, ConditionalBuilder/*!*/ body, ValidationInfo/*!*/ validation) {
                Body = body;
                Validation = validation;
                Args = args;
            }
        }

        class DeleteBindingInfo : MemberBindingInfo {
            public readonly DeleteMemberAction/*!*/ Action;

            public DeleteBindingInfo(DeleteMemberAction/*!*/ action, MetaObject/*!*/[]/*!*/ args, ConditionalBuilder/*!*/ body, ValidationInfo/*!*/ validation)
                : base(args, body, validation) {
                Action = action;
            }
        }

        class SetBindingInfo : MemberBindingInfo {
            public readonly SetMemberAction/*!*/ Action;

            public SetBindingInfo(SetMemberAction/*!*/ action, MetaObject/*!*/[]/*!*/ args, ConditionalBuilder/*!*/ body, ValidationInfo/*!*/ validation)
                : base(args, body, validation) {
                Action = action;
            }
        }

        class GetBindingInfo : MemberBindingInfo {
            public readonly GetMemberAction/*!*/ Action;
            public readonly VariableExpression/*!*/ Self, Result;

            public GetBindingInfo(GetMemberAction/*!*/ action, MetaObject/*!*/[]/*!*/ args, VariableExpression/*!*/ self, VariableExpression/*!*/ result, ConditionalBuilder/*!*/ body, ValidationInfo/*!*/ validationInfo)
                : base(args, body, validationInfo) {
                Action = action;
                Self = self;
                Result = result;
            }
        }

        #endregion

        #region Fallback Helpers

        /// <summary>
        /// Helper for falling back - if we have a base object fallback to it first (which can
        /// then fallback to the calling site), otherwise fallback to the calling site.
        /// </summary>
        private MetaObject/*!*/ FallbackGetError(GetMemberAction/*!*/ action, MetaObject/*!*/[] args) {
            if (_baseMetaObject != null) {
                return _baseMetaObject.GetMember(action, args);
            } else if (BindingHelpers.IsNoThrow(action)) {
                return new MetaObject(
                    Ast.Field(null, typeof(OperationFailed).GetField("Value")),
                    Restrictions.Empty
                );
            } else if (action is GetMemberBinder) {
                return new MetaObject(
                    MakeTypeError(action.Name, ((IPythonObject)args[0].Value).PythonType),
                    Restrictions.Empty
                );
            }

            return action.Fallback(args);
        }

        /// <summary>
        /// Helper for falling back - if we have a base object fallback to it first (which can
        /// then fallback to the calling site), otherwise fallback to the calling site.
        /// </summary>
        private MetaObject/*!*/ FallbackSetError(SetMemberAction/*!*/ action, MetaObject/*!*/[] args) {
            if (_baseMetaObject != null) {
                return _baseMetaObject.SetMember(action, args);
            } else if (action is SetMemberBinder) {
                return new MetaObject(
                    MakeTypeError(action.Name, ((IPythonObject)args[0].Value).PythonType),
                    Restrictions.Empty
                );
            }

            return action.Fallback(args);
        }


        /// <summary>
        /// Helper for falling back - if we have a base object fallback to it first (which can
        /// then fallback to the calling site), otherwise fallback to the calling site.
        /// </summary>
        private MetaObject/*!*/ FallbackDeleteError(DeleteMemberAction/*!*/ action, MetaObject/*!*/[] args) {
            if (_baseMetaObject != null) {
                return _baseMetaObject.DeleteMember(action, args);
            } else if (action is DeleteMemberBinder) {
                return new MetaObject(
                    MakeTypeError(action.Name, ((IPythonObject)args[0].Value).PythonType),
                    Restrictions.Empty
                );
            }

            return action.Fallback(args);
        }

        #endregion

    }
}
