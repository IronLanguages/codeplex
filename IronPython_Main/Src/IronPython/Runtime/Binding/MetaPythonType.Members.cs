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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Scripting.Actions;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

using Ast = System.Linq.Expressions.Expression;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronPython.Runtime.Binding {

    partial class MetaPythonType : MetaPythonObject {

        #region MetaObject Overrides

        public override MetaObject/*!*/ GetMember(GetMemberAction/*!*/ member, MetaObject/*!*/[]/*!*/ args) {
            switch (member.Name) {
                case "__dict__":
                case "__class__":
                case "__bases__":
                case "__name__":
                    MetaObject self = Restrict(RuntimeType);
                    ValidationInfo valInfo = MakeMetaTypeTest(self.Expression);

                    return BindingHelpers.AddDynamicTestAndDefer(
                        member,
                        new MetaObject(
                            MakeMetaTypeRule(member, member.Fallback(args).Expression),
                            self.Restrictions
                        ),
                        args,
                        valInfo
                    );
                default:
                    if (!Value.IsSystemType) {
                        ValidationInfo typeTest = GetTypeTest();

                        return BindingHelpers.AddDynamicTestAndDefer(
                            member,
                            MakeTypeGetMember(member, args),
                            args,
                            typeTest
                        );
                    }

                    return MakeTypeGetMember(member, args);
            }
        }

        private ValidationInfo GetTypeTest() {
            int version = Value.Version;

            return new ValidationInfo(
                Ast.Call(
                    typeof(PythonOps).GetMethod("CheckSpecificTypeVersion"),
                    Ast.ConvertHelper(Expression, typeof(PythonType)),
                    Ast.Constant(version)
                ),
                new BindingHelpers.PythonTypeValidator(Value, version).Validate
            );
        }

        public override MetaObject/*!*/ SetMember(SetMemberAction/*!*/ member, MetaObject[]/*!*/ args) {
            BinderState state = BinderState.GetBinderState(member);

            if (Value.IsSystemType) {
                MemberTracker tt = MemberTracker.FromMemberInfo(Value.UnderlyingSystemType);
                MemberGroup mg = state.Binder.GetMember(OldSetMemberAction.Make(state.Binder, member.Name), Value.UnderlyingSystemType, member.Name);

                // filter protected member access against .NET types, these can only be accessed from derived types...
                foreach (MemberTracker mt in mg) {
                    if (IsProtectedSetter(mt)) {
                        return new MetaObject(
                            BindingHelpers.TypeErrorForProtectedMember(Value.UnderlyingSystemType, member.Name),
                            Restrictions.InstanceRestriction(Expression, Value).Merge(Restrictions).Merge(args[1].Restrictions)
                        );
                    }
                }

                // have the default binder perform it's operation against a TypeTracker and then
                // replace the test w/ our own.
                return new MetaObject(
                    state.Binder.SetMember(
                        member.Name,
                        new MetaObject(
                            Ast.Constant(tt),
                            Restrictions.Empty,
                            tt
                        ),
                        args[1],
                        Ast.Constant(state.Context)
                    ).Expression,
                    Restrictions.InstanceRestriction(Expression, Value).Merge(Restrictions).Merge(args[1].Restrictions)
                );
            }

            return MakeSetMember(member, args);
        }

        public override MetaObject/*!*/ DeleteMember(DeleteMemberAction/*!*/ member, MetaObject/*!*/[]/*!*/ args) {
            if (Value.IsSystemType) {
                BinderState state = BinderState.GetBinderState(member);

                MemberTracker tt = MemberTracker.FromMemberInfo(Value.UnderlyingSystemType);

                // have the default binder perform it's operation against a TypeTracker and then
                // replace the test w/ our own.
                return new MetaObject(
                    state.Binder.DeleteMember(
                        member.Name,
                        new MetaObject(
                            Ast.Constant(tt),
                            Restrictions.Empty,
                            tt
                        )
                    ).Expression,
                    Restrictions.InstanceRestriction(Expression, Value).Merge(Restrictions)
                );
            }

            return MakeDeleteMember(member, args);
        }

        #endregion

        #region Gets

        private ValidationInfo MakeMetaTypeTest(Expression self) {

            PythonType metaType = DynamicHelpers.GetPythonType(Value);
            if (!metaType.IsSystemType) {
                int version = metaType.Version;

                return new ValidationInfo(
                    Ast.Call(
                        typeof(PythonOps).GetMethod("CheckTypeVersion"),
                        self,
                        Ast.Constant(version)
                    ),
                    new BindingHelpers.PythonTypeValidator(metaType, version).Validate
                );
            }

            return ValidationInfo.Empty;
        }

        private MetaObject/*!*/ MakeTypeGetMember(GetMemberAction/*!*/ member, MetaObject/*!*/[]/*!*/ args) {
            // normal attribute, need to check the type version
            MetaObject self = new MetaObject(
                Ast.ConvertHelper(Expression, Value.GetType()),
                Restrictions.Merge(Restrictions.InstanceRestriction(Expression, Value)),
                Value
            );

            BinderState state = BinderState.GetBinderState(member);

            // have the default binder perform it's operation against a TypeTracker and then
            // replace the test w/ our own.
            MetaObject result = GetFallbackGet(member, state, args);

            for (int i = Value.ResolutionOrder.Count - 1; i >= 0; i--) {
                PythonType pt = Value.ResolutionOrder[i];

                PythonTypeSlot pts;

                if (pt.IsSystemType) {
                    // built-in type, see if we can bind to any .NET members and then quit the search 
                    // because this includes all subtypes.
                    result = new MetaObject(
                        MakeSystemTypeGetExpression(pt, member, result.Expression),
                        self.Restrictions // don't merge w/ result - we've already restricted to instance.
                    );
                } else if (pt.IsOldClass) {
                    // mixed new-style/old-style class, search the one slot in it's MRO for the member
                    VariableExpression tmp = Ast.Variable(typeof(object), "tmp");
                    result = new MetaObject(
                        Ast.Scope(
                            Ast.Condition(
                                Ast.Call(
                                    typeof(PythonOps).GetMethod("OldClassTryLookupOneSlot"),
                                    Ast.Constant(pt.OldClass),
                                    AstUtils.Constant(SymbolTable.StringToId(member.Name)),
                                    tmp
                                ),
                                tmp,
                                Ast.ConvertHelper(result.Expression, typeof(object))
                            ),
                            tmp
                        ),
                        self.Restrictions // don't merge w/ result - we've already restricted to instance.
                    );

                } else if (pt.TryLookupSlot(state.Context, SymbolTable.StringToId(member.Name), out pts)) {
                    // user defined new style class, see if we have a slot.
                    VariableExpression tmp = Ast.Variable(typeof(object), "tmp");

                    result = new MetaObject(
                        Ast.Scope(
                            Ast.Condition(
                                Ast.Call(
                                    TypeInfo._PythonOps.SlotTryGetBoundValue,
                                    Ast.Constant(BinderState.GetBinderState(member).Context),
                                    Ast.Constant(pts, typeof(PythonTypeSlot)),
                                    Ast.Null(),
                                    Ast.Constant(Value),
                                    tmp
                                ),
                                tmp,
                                Ast.ConvertHelper(
                                    result.Expression,
                                    typeof(object)
                                )
                            ),
                            tmp
                        ),
                        self.Restrictions   // don't merge w/ result - we've already restricted to instance.
                    );
                }
            }

            return result;
        }

        private MetaObject/*!*/ GetFallbackGet(GetMemberAction/*!*/ member, BinderState/*!*/ state, MetaObject/*!*/[]/*!*/ args) {
            MemberTracker tt = MemberTracker.FromMemberInfo(Value.UnderlyingSystemType);

            MetaObject res = new MetaObject(
                state.Binder.GetMember(
                    member.Name,
                    new MetaObject(
                        Ast.Constant(tt),
                        Restrictions.Empty,
                        tt
                    ),
                    Ast.Constant(state.Context),
                    BindingHelpers.IsNoThrow(member)

                ).Expression,
                Restrictions.InstanceRestriction(Expression, Value).Merge(Restrictions)
            );


            if (Value.IsHiddenMember(member.Name) && member is IPythonSite) {
                Debug.Assert(args.Length == 2 && args[1].Expression.Type == typeof(CodeContext));
                Expression codeContext = args[1].Expression;

                res = BindingHelpers.FilterShowCls(
                    codeContext,
                    member,
                    res,
                    Ast.Throw(
                        Ast.Call(
                            typeof(PythonOps).GetMethod("AttributeErrorForMissingAttribute", new Type[] { typeof(string), typeof(SymbolId) }),
                            Ast.Constant(Value.Name),
                            AstUtils.Constant(SymbolTable.StringToId(member.Name))
                        )
                    )
                );
            }

            return res;
        }

        private Expression MakeSystemTypeGetExpression(PythonType/*!*/ pt, GetMemberAction/*!*/ member, Expression/*!*/ error) {
            BinderState state = BinderState.GetBinderState(member);

            PythonTypeSlot pts;

            CodeContext clsContext = PythonContext.GetContext(state.Context).DefaultClsBinderState.Context;
            if (state.Binder.TryResolveSlot(clsContext, pt, Value, SymbolTable.StringToId(member.Name), out pts)) {
                Expression success = pts.MakeGetExpression(
                    state.Binder,
                    BinderState.GetCodeContext(member),
                    null,
                    Ast.ConvertHelper(AstUtils.WeakConstant(Value), typeof(PythonType)),
                    error
                );

                return AddClsCheck(member, pts, success, error);
            }

            // need to lookup on type
            return MakeMetaTypeRule(member, error);
        }

        private Expression/*!*/ AddClsCheck(GetMemberAction/*!*/ member, PythonTypeSlot/*!*/ slot, Expression/*!*/ success, Expression/*!*/ error) {
            BinderState state = BinderState.GetBinderState(member);

            if (Value.IsPythonType && !slot.IsAlwaysVisible) {
                Type resType = BindingHelpers.GetCompatibleType(success.Type, error.Type);

                success = Ast.Condition(
                    Ast.Call(
                        typeof(PythonOps).GetMethod("IsClsVisible"),
                        Ast.Constant(state.Context)
                    ),
                    Ast.ConvertHelper(success, resType),
                    Ast.ConvertHelper(error, resType)
                );
            }
            return success;
        }

        private Expression MakeMetaTypeRule(GetMemberAction/*!*/ member, Expression error) {
            BinderState state = BinderState.GetBinderState(member);

            OldGetMemberAction gma = OldGetMemberAction.Make(state.Binder, member.Name);
            MemberGroup mg = state.Binder.GetMember(gma, typeof(PythonType), member.Name);
            PythonType metaType = DynamicHelpers.GetPythonType(Value);
            PythonTypeSlot pts;

            foreach (PythonType pt in metaType.ResolutionOrder) {
                if (pt.IsSystemType) {
                    // need to lookup on type
                    mg = state.Binder.GetMember(gma, typeof(PythonType), member.Name);

                    if (mg.Count > 0) {
                        return GetBoundTrackerOrError(member, mg, error);
                    }
                } else if (pt.OldClass != null) {
                    // mixed new-style/old-style class, just call our version of __getattribute__
                    // and let it sort it out at runtime.  
                    // TODO: IfError support
                    return Ast.Call(
                        Ast.ConvertHelper(
                            Expression,
                            typeof(PythonType)
                        ),
                        typeof(PythonType).GetMethod("__getattribute__"),
                        Ast.Constant(BinderState.GetBinderState(member).Context),
                        Ast.Constant(member.Name)
                    );
                } else if (pt.TryLookupSlot(BinderState.GetBinderState(member).Context, SymbolTable.StringToId(member.Name), out pts)) {
                    // user defined new style class, see if we have a slot.
                    VariableExpression tmp = Ast.Variable(typeof(object), "slotRes");
                    return Ast.Scope(
                        Ast.Condition(
                            Ast.Call(
                                typeof(PythonOps).GetMethod("SlotTryGetBoundValue"),
                                Ast.Constant(BinderState.GetBinderState(member).Context),
                                Ast.Constant(pts, typeof(PythonTypeSlot)),
                                Expression,
                                Ast.Constant(metaType),
                                tmp
                            ),
                            tmp,
                            Ast.ConvertHelper(error, typeof(object))
                        ),
                        tmp
                    );
                }
            }

            // the member doesn't exist anywhere in the type hierarchy, see if
            // we define __getattr__ on our meta type.
            if (metaType.TryResolveSlot(BinderState.GetBinderState(member).Context, Symbols.GetBoundAttr, out pts)) {
                VariableExpression tmp = Ast.Variable(typeof(object), "res");
                return Ast.Scope(
                    Ast.Condition(
                        Ast.Call(
                            typeof(PythonOps).GetMethod("SlotTryGetBoundValue"),
                            Ast.Constant(BinderState.GetBinderState(member).Context),
                            Ast.Constant(pts, typeof(PythonTypeSlot)),
                            Expression,
                            Ast.Constant(metaType),
                            tmp
                        ),
                        Ast.ActionExpression(
                            new InvokeBinder(
                                BinderState.GetBinderState(member),
                                new CallSignature(1)
                            ),
                            typeof(object),
                            BinderState.GetCodeContext(member),
                            tmp,
                            Ast.Constant(member.Name)
                        ),
                        Ast.ConvertHelper(
                            error,
                            typeof(object)
                        )
                    ),
                    tmp
                );
            }

            return error;/* ?? Ast.Throw(
                Ast.Call(
                    typeof(PythonOps).GetMethod("AttributeErrorForMissingAttribute", new Type[] { typeof(string), typeof(SymbolId) }),
                    Ast.Constant(DynamicHelpers.GetPythonType(_type).Name),
                    Ast.Constant(SymbolTable.StringToId(_name))
                )
            );*/
        }

        private Expression/*!*/ GetBoundTrackerOrError(GetMemberAction/*!*/ member, MemberGroup/*!*/ mg, Expression error) {
            BinderState state = BinderState.GetBinderState(member);
            MemberTracker tracker = GetTracker(member, mg);
            Expression target = null;

            if (tracker != null) {
                tracker = tracker.BindToInstance(Ast.ConvertHelper(Expression, typeof(PythonType)));
                target = tracker.GetValue(Ast.Constant(state.Context), state.Binder, Value.UnderlyingSystemType);
            }

            return target ?? error /*?? Ast.Throw(MakeAmbiguousMatchError(mg))*/;
        }
#if FALSE
        private static Expression/*!*/ MakeErrorExpression(MemberGroup/*!*/ mg) {
            if (mg.Count == 1) {
                MemberTracker mt = mg[0];

                if (mt.DeclaringType.ContainsGenericParameters) {
                    return Ast.Throw(
                        Ast.New(
                            typeof(InvalidOperationException).GetConstructor(new Type[] { typeof(string) }),
                            Ast.Constant(String.Format("Cannot access member {1} declared on type {0} because the type contains generic parameters.", mt.DeclaringType.Name, mt.Name))
                        )
                    );
                }
            }

            return Ast.Throw(MakeAmbiguousMatchError(mg));
        }

        private static Expression/*!*/ MakeAmbiguousMatchError(MemberGroup/*!*/ members) {
            StringBuilder sb = new StringBuilder();
            foreach (MethodTracker mi in members) {
                if (sb.Length != 0) sb.Append(", ");
                sb.Append(mi.MemberType);
                sb.Append(" : ");
                sb.Append(mi.ToString());
            }

            return Ast.New(typeof(AmbiguousMatchException).GetConstructor(
                new Type[] { typeof(string) }),
                Ast.Constant(sb.ToString())
            );
        }
#endif

        private MemberTracker GetTracker(GetMemberAction/*!*/ member, MemberGroup/*!*/ mg) {
            TrackerTypes mt = GetMemberTypes(mg);
            MemberTracker tracker;

            switch (mt) {
                case TrackerTypes.Method:
                    tracker = ReflectionCache.GetMethodGroup(member.Name, mg);
                    break;
                case TrackerTypes.TypeGroup:
                case TrackerTypes.Type:
                    tracker = GetTypeGroup(mg);
                    break;
                case TrackerTypes.Field:
                case TrackerTypes.Property:
                    tracker = null;
                    foreach (MemberTracker curTracker in mg) {
                        if (curTracker.DeclaringType == Value.UnderlyingSystemType) {
                            tracker = curTracker;
                        }
                    }
                    if (tracker == null) {
                        tracker = mg[0];
                    }
                    break;
                case TrackerTypes.Field | TrackerTypes.Property:
                    // occurs when we have a protected field w/ public property accessors
                    List<MemberTracker> newGroup = new List<MemberTracker>();
                    foreach (MemberTracker curTracker in mg) {
                        if (curTracker.MemberType != TrackerTypes.Field) {
                            newGroup.Add(curTracker);
                        }
                    }

                    return GetTracker(member, new MemberGroup(newGroup.ToArray()));
                case TrackerTypes.Event:
                case TrackerTypes.Namespace:
                case TrackerTypes.Custom:
                case TrackerTypes.Constructor:
                    tracker = mg[0];
                    break;
                default:
                    tracker = null;
                    break;
            }

            return tracker;
        }

        internal static TrackerTypes GetMemberTypes(MemberGroup members) {
            TrackerTypes memberType = TrackerTypes.None;
            for (int i = 0; i < members.Count; i++) {
                MemberTracker mi = members[i];
                memberType |= mi.MemberType;
            }

            return memberType;
        }

        private static TypeTracker/*!*/ GetTypeGroup(MemberGroup/*!*/ members) {
            TypeTracker typeTracker = (TypeTracker)members[0];
            for (int i = 1; i < members.Count; i++) {
                typeTracker = TypeGroup.UpdateTypeEntity(typeTracker, (TypeTracker)members[i]);
            }
            return typeTracker;
        }

        #endregion

        #region Sets

        private MetaObject/*!*/ MakeSetMember(SetMemberAction/*!*/ member, MetaObject/*!*/[]/*!*/ args) {
            MetaObject self = Restrict(typeof(PythonType));

            return BindingHelpers.AddDynamicTestAndDefer(
                member,
                new MetaObject(
                    Ast.Call(
                        typeof(PythonOps).GetMethod("PythonTypeSetCustomMember"),
                        Ast.Constant(BinderState.GetBinderState(member).Context),
                        self.Expression,
                        AstUtils.Constant(SymbolTable.StringToId(member.Name)),
                        Ast.ConvertHelper(
                            args[1].Expression,
                            typeof(object)
                        )
                    ),
                    self.Restrictions.Merge(args[1].Restrictions)
                ),
                args,
                TestUserType()
            );
        }

        private bool IsProtectedSetter(MemberTracker mt) {
            PropertyTracker pt = mt as PropertyTracker;
            if (pt != null) {
                MethodInfo mi = pt.GetSetMethod(true);
                if (mi != null && (mi.IsFamily || mi.IsFamilyOrAssembly)) {
                    return true;
                }
            }

            FieldTracker ft = mt as FieldTracker;
            if (ft != null) {
                return ft.Field.IsFamily || ft.Field.IsFamilyOrAssembly;
            }

            return false;
        }

        #endregion

        #region Deletes

        private MetaObject/*!*/ MakeDeleteMember(DeleteMemberAction/*!*/ member, MetaObject/*!*/[]/*!*/ args) {
            MetaObject self = Restrict(typeof(PythonType));
            return BindingHelpers.AddDynamicTestAndDefer(
                member,
                new MetaObject(
                    Ast.Call(
                        typeof(PythonOps).GetMethod("PythonTypeDeleteCustomMember"),
                        Ast.Constant(BinderState.GetBinderState(member).Context),
                        self.Expression,
                        AstUtils.Constant(SymbolTable.StringToId(member.Name))
                    ),
                    self.Restrictions
                ),
                args,
                TestUserType()
            );
        }

        #endregion

        #region Helpers

        private ValidationInfo/*!*/ TestUserType() {
            return new ValidationInfo(
                Ast.Not(
                    Ast.Call(
                        typeof(PythonOps).GetMethod("IsPythonType"),
                        Ast.ConvertHelper(
                            Expression,
                            typeof(PythonType)
                        )
                    )
                ),
                null
            );
        }

        #endregion
    }
}
