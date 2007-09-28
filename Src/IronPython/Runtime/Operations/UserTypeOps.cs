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
using System.Diagnostics;
using System.Reflection;

using Microsoft.Scripting;
using Microsoft.Scripting.Types;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;

using IronPython.Runtime.Types;
using IronPython.Runtime.Calls;
using IronPython.Hosting;

namespace IronPython.Runtime.Operations {
    using Ast = Microsoft.Scripting.Ast.Ast;
    using Microsoft.Scripting.Generation;
    using Microsoft.Scripting.Utils;

    public static class UserTypeOps {
        public static string ToStringReturnHelper(object o) {
            if (o is string && o != null) {
                return (string)o;
            }
            throw PythonOps.TypeError("__str__ returned non-string type ({0})", DynamicTypeOps.GetName(o));
        }

        public static IAttributesCollection SetDictHelper(ref IAttributesCollection iac, IAttributesCollection value) {
            if (System.Threading.Interlocked.CompareExchange<IAttributesCollection>(ref iac, value, null) == null)
                return value;
            return iac;
        }

        public static object GetPropertyHelper(object prop, object instance, SymbolId name) {
            DynamicTypeSlot desc = prop as DynamicTypeSlot;
            if (desc == null) {
                throw PythonOps.TypeError("Expected property for {0}, but found {1}",
                    name.ToString(), DynamicHelpers.GetDynamicType(prop).Name);
            }
            object value;
            desc.TryGetValue(DefaultContext.Default, instance, DynamicHelpers.GetDynamicType(instance), out value);
            return value;
        }

        public static void SetPropertyHelper(object prop, object instance, object newValue, SymbolId name) {
            DynamicTypeSlot desc = prop as DynamicTypeSlot;
            if (desc == null) {
                throw PythonOps.TypeError("Expected settable property for {0}, but found {1}",
                    name.ToString(), DynamicHelpers.GetDynamicType(prop).Name);
            }
            desc.TrySetValue(DefaultContext.Default, instance, DynamicHelpers.GetDynamicType(instance), newValue);
        }

        public static void AddRemoveEventHelper(object method, object instance, DynamicType dt, object eventValue, SymbolId name) {
            object callable = method;
            
            DynamicTypeSlot dts = method as DynamicTypeSlot;
            if(dts != null) {
                if (!dts.TryGetValue(DefaultContext.Default, instance, dt, out callable))
                    throw PythonOps.AttributeErrorForMissingAttribute(dt.Name, name);
            } 

            if (!PythonOps.IsCallable(callable)) {
                throw PythonOps.TypeError("Expected callable value for {0}, but found {1}", name.ToString(),
                    DynamicTypeOps.GetName(method));
            }

            PythonCalls.Call(callable, eventValue);
        }

        public static StandardRule<T> GetRuleHelper<T>(DynamicAction action, CodeContext context, object[] args) {
            switch (action.Kind) {
                case DynamicActionKind.GetMember: return MakeGetMemberRule<T>(context, (GetMemberAction)action, args);
                case DynamicActionKind.SetMember: return MakeSetMemberRule<T>(context, (SetMemberAction)action, args);
                case DynamicActionKind.DeleteMember: return MakeDeleteMemberRule<T>(context, (DeleteMemberAction)action, args);
                case DynamicActionKind.DoOperation: return MakeOperationRule<T>(context, (DoOperationAction)action, args);
                case DynamicActionKind.Call: return MakeCallRule<T>(context, (CallAction)action, args);
                    
                default: return null;
            }
        }

        private static StandardRule<T> MakeCallRule<T>(CodeContext context, CallAction callAction, object[] args) {
            StandardRule<T> rule = new StandardRule<T>();            

            ISuperDynamicObject sdo = (ISuperDynamicObject)args[0];

            PythonBinderHelper.MakeTest(rule, sdo.DynamicType);

            DynamicTypeSlot callSlot;
            Statement body = rule.MakeError(context.LanguageContext.Binder,
                Ast.Call(null,
                typeof(PythonOps).GetMethod("UncallableError"),
                rule.Parameters[0])
            );

            if (sdo.DynamicType.TryResolveSlot(context, Symbols.Call, out callSlot)) {
                Variable tmp = rule.GetTemporary(typeof(object), "callSlot");
                Expression[] callArgs = (Expression[])rule.Parameters.Clone();
                callArgs[0] = Ast.Read(tmp);

                body = Ast.Block(
                    Ast.If(
                        Ast.Call(
                            Ast.Convert(Ast.WeakConstant(callSlot), typeof(DynamicTypeSlot)),
                            typeof(DynamicTypeSlot).GetMethod("TryGetValue"),
                            Ast.CodeContext(),
                            rule.Parameters[0],
                            Ast.ReadProperty(
                                Ast.Convert(rule.Parameters[0], typeof(ISuperDynamicObject)),
                                typeof(ISuperDynamicObject).GetProperty("DynamicType")
                            ),
                            Ast.ReadDefined(tmp)
                        ),
                        rule.MakeReturn(context.LanguageContext.Binder,
                            Ast.Action.Call(
                                callAction,
                                typeof(object),
                                callArgs
                            )
                        )
                    ),
                    body
                );
            }

            rule.SetTarget(body);
            return rule;
        }

        private static StandardRule<T> MakeGetMemberRule<T>(CodeContext context, GetMemberAction action, object[] args) {
            DynamicTypeSlot dts;
            ISuperDynamicObject sdo = (ISuperDynamicObject)args[0];
            StandardRule<T> rule = new StandardRule<T>();
            Variable tmp = rule.GetTemporary(typeof(object), "lookupRes");

            if (TryGetGetAttribute(context, sdo, out dts)) {
                Debug.Assert(sdo.DynamicType.HasGetAttribute);

                //rule.MakeTest(sdo.DynamicType);
                //rule.SetTarget(MakeGetAttrRule<T>(context, action, rule, Ast.Empty(), tmp, dts));
                MakeGetAttributeRule<T>(context, action, rule, tmp);
            } else if (!(args[0] is ICustomMembers)) {
                // fast path for accessing properties from a derived type.
                PythonBinderHelper.MakeTest(rule, sdo.DynamicType);

                Type t = args[0].GetType();
                PropertyInfo pi = t.GetProperty(SymbolTable.IdToString(action.Name));
                if (pi != null) {
                    MethodInfo getter = pi.GetGetMethod();
                    if (getter != null && getter.IsPublic) {
                        rule.SetTarget(rule.MakeReturn(
                            context.LanguageContext.Binder, 
                            Ast.Call(rule.Parameters[0], pi.GetGetMethod())));
                        return rule;
                    }
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

                int slotLocation = -1;
                IList<DynamicMixin> mro = sdo.DynamicType.ResolutionOrder;
                for (int i = 0; i < mro.Count; i++) {
                    if (mro[i].TryLookupSlot(context, action.Name, out dts)) {
                        slotLocation = i;
                        break;
                    }
                }

                ReflectedSlotProperty rsp = dts as ReflectedSlotProperty;
                if (rsp != null) {
                    // type has __slots__ defined for this member, call the getter directly
                    rule.SetTarget(
                        rule.MakeReturn(
                            context.LanguageContext.Binder,
                            Ast.Comma(0,
                                Ast.Assign(tmp,
                                    Ast.Call(null, rsp.GetterMethod, rule.Parameters[0])
                                ),
                                Ast.Call(null, typeof(PythonOps).GetMethod("CheckInitializedAttribute"),
                                    Ast.Read(tmp),
                                    rule.Parameters[0],
                                    Ast.Constant(SymbolTable.IdToString(action.Name))
                                )
                            )
                        )
                    );
                    return rule;      
                }

                Statement body = Ast.Empty();

                bool isOldStyle = false;
                foreach (DynamicType dt in sdo.DynamicType.ResolutionOrder) {
                    if (Mro.IsOldStyle(dt)) {
                        isOldStyle = true;
                        break;
                    }
                }

                if (!isOldStyle) {
                    if (sdo.HasDictionary && (dts == null || !dts.IsSetDescriptor(context, sdo.DynamicType))) {
                        body = MakeDictionaryAccess<T>(context, action, rule, tmp);
                    }

                    if (dts != null) {
                        body = MakeSlotAccess<T>(context, rule, dts, body, tmp);
                    }
                } else {
                    body = MakeOldStyleAccess(context, rule, action.Name, sdo, body, tmp);
                }

                // fall back to __getattr__ if it's defined.
                DynamicTypeSlot getattr;
                if (sdo.DynamicType.TryResolveSlot(context, Symbols.GetBoundAttr, out getattr)) {
                    body = MakeGetAttrRule<T>(context, action, rule, body, tmp, getattr);
                }

                // raise an error if nothing else succeeds (TODO: need to reconcile error handling).
                if (action.IsNoThrow) {
                    body = Ast.Block(body, ReturnOperationFailed<T>(context, rule));
                } else {
                    body = MakeTypeError<T>(context, sdo.DynamicType, action, rule, body);
                }

                rule.SetTarget(body);
            } else {
                // TODO: When ICustomMembers goes away, or as it slowly gets replaced w/ IDynamicObject,
                // we'll need to call the base GetRule instead and merge that with our normal lookup
                // rules somehow.  Today ICustomMembers always takes precedence, so it does here too.
                PythonBinderHelper.MakeTest(rule, sdo.DynamicType);

                rule.SetTarget(MakeCustomMembersGetBody(context, action, DynamicTypeOps.GetName(sdo.DynamicType), rule));       
            }

            return rule;
        }

        private static void MakeGetAttributeRule<T>(CodeContext context, GetMemberAction action, StandardRule<T> rule, Variable tmp) {
                // type defines read it and call it (we read it so that we can
                // share this rule amongst multiple types - this trades off a little
                // perf for the __getattribute__ case while reducing the number of
                // unique rules we generate).
                rule.SetTest(
                    Ast.AndAlso(
                        Ast.TypeIs(rule.Parameters[0], typeof(ISuperDynamicObject)),
                        Ast.ReadProperty(
                            Ast.ReadProperty(
                                Ast.Convert(rule.Parameters[0], typeof(ISuperDynamicObject)),
                                typeof(ISuperDynamicObject).GetProperty("DynamicType")
                            ),
                            typeof(DynamicType).GetProperty("HasGetAttribute")
                        )
                    )
                );

                Variable slotTmp = rule.GetTemporary(typeof(DynamicTypeSlot), "slotTmp");
                Statement body = Ast.If(
                            Ast.Call(
                                Ast.ReadProperty(
                                    Ast.Convert(rule.Parameters[0], typeof(ISuperDynamicObject)),
                                    typeof(ISuperDynamicObject).GetProperty("DynamicType")
                                ),
                                typeof(DynamicType).GetMethod("TryResolveSlot"),
                                Ast.CodeContext(),
                                Ast.Constant(Symbols.GetAttribute),
                                Ast.Read(slotTmp)
                            ),
                            MakeGetAttrRule<T>(context, action, rule, Ast.Empty(), tmp, Ast.Read(slotTmp))
                        );

                rule.SetTarget(body);
        }

        internal static Statement MakeCustomMembersGetBody<T>(CodeContext context, GetMemberAction action, string typeName, StandardRule<T> rule) {
            Variable tmp = rule.GetTemporary(typeof(object), "custmemres");

            return Ast.IfThenElse(
                        Ast.Call(
                            Ast.Convert(rule.Parameters[0], typeof(ICustomMembers)),
                            typeof(ICustomMembers).GetMethod("TryGetBoundCustomMember"),
                            Ast.CodeContext(),
                            Ast.Constant(action.Name),
                            Ast.Read(tmp)
                        ),
                        rule.MakeReturn(context.LanguageContext.Binder, Ast.Read(tmp)),
                        MakeMissingAttributeError<T>(context, action, typeName, rule)
                    );
        }

        internal static Statement MakeCustomMembersSetBody<T>(CodeContext context, SetMemberAction action, string typeName, StandardRule<T> rule) {
            Variable tmp = rule.GetTemporary(typeof(object), "custmemres");

            return rule.MakeReturn(context.LanguageContext.Binder, 
                Ast.Comma(
                    1,
                    Ast.Call(
                        Ast.Convert(rule.Parameters[0], typeof(ICustomMembers)),
                        typeof(ICustomMembers).GetMethod("SetCustomMember"),
                        Ast.CodeContext(),
                        Ast.Constant(action.Name),
                        rule.Parameters[1]
                    ),
                    rule.Parameters[1]
                )
            );                                           
        }

        internal static Statement MakeCustomMembersDeleteBody<T>(CodeContext context, DeleteMemberAction action, string typeName, StandardRule<T> rule) {
            Variable tmp = rule.GetTemporary(typeof(object), "custmemres");

            return rule.MakeReturn(context.LanguageContext.Binder,
                Ast.Call(
                    Ast.Convert(rule.Parameters[0], typeof(ICustomMembers)),
                    typeof(ICustomMembers).GetMethod("DeleteCustomMember"),
                    Ast.CodeContext(),
                    Ast.Constant(action.Name)
                )
            );
        }

        private static Statement MakeMissingAttributeError<T>(CodeContext context, GetMemberAction action, string typeName, StandardRule<T> rule) {
            if (action.IsNoThrow) {
                return ReturnOperationFailed<T>(context, rule);
            } else {
                return MakeThrowingAttributeError<T>(context, action, typeName, rule);
            }
        }

        private static Statement MakeThrowingAttributeError<T>(CodeContext context, MemberAction action, string typeName, StandardRule<T> rule) {
            return rule.MakeError(context.LanguageContext.Binder,
                Ast.Call(null,
                    typeof(PythonOps).GetMethod("AttributeErrorForMissingAttribute", new Type[] { typeof(string), typeof(SymbolId) }),
                    Ast.Constant(typeName),
                    Ast.Constant(action.Name)
                )
            );
        }
        
        /// <summary>
        /// Checks to see if this type has __getattribute__ that overrides all other attribute lookup.
        /// 
        /// This is more complex then it needs to be.  The problem is that when we have a 
        /// mixed new-style/old-style class we have a weird __getattribute__ defined.  When
        /// we always dispatch through rules instead of DynamicTypes it should be easy to remove
        /// this.
        /// </summary>
        private static bool TryGetGetAttribute(CodeContext context, ISuperDynamicObject sdo, out DynamicTypeSlot dts) {
            if (sdo.DynamicType.TryResolveSlot(context, Symbols.GetAttribute, out dts)) {
                BuiltinMethodDescriptor bmd = dts as BuiltinMethodDescriptor;
                if (bmd == null || bmd.DeclaringType != typeof(object) ||
                    bmd.Template.Targets.Length != 1 ||
                    bmd.Template.Targets[0].DeclaringType != typeof(ObjectOps) ||
                    bmd.Template.Targets[0].Name != "__getattribute__") {

                    DynamicTypeGetAttributeSlot gas = dts as DynamicTypeGetAttributeSlot;
                    if (gas != null) {
                        // inherited __getattribute__ slots won't provide their value so
                        // we need to get the one that's actually going to provide a value.
                        if (gas.Inherited) {
                            for (int i = 1; i < sdo.DynamicType.ResolutionOrder.Count; i++) {
                                if (sdo.DynamicType.ResolutionOrder[i] != TypeCache.Object &&
                                    sdo.DynamicType.ResolutionOrder[i].TryLookupSlot(context, Symbols.GetAttribute, out dts)) {
                                    gas = dts as DynamicTypeGetAttributeSlot;
                                    if (gas == null || !gas.Inherited) break;

                                    dts = null;
                                }
                            }
                        }
                    }

                    return dts != null;
                }                
            }
            return false;
        }

        public static bool TryGetMixedNewStyleOldStyleSlot(CodeContext context, object instance, SymbolId name, out object value) {
            return OldInstanceTypeBuilder.TryGetMemberCustomizer(context, instance, name, out value);
        }

        /// <summary>
        /// Checks a range of the MRO to perform old-style class lookups if any old-style classes
        /// are present.  We will call this twice to produce a search before a slot and after
        /// a slot.
        /// </summary>
        private static Statement MakeOldStyleAccess<T>(CodeContext context, StandardRule<T> rule, SymbolId name, ISuperDynamicObject sdo, Statement body, Variable tmp) {
            return Ast.Block(
                body,
                Ast.If(
                    Ast.Call(
                        null,
                        typeof(UserTypeOps).GetMethod("TryGetMixedNewStyleOldStyleSlot"),
                        Ast.CodeContext(),
                        rule.Parameters[0],
                        Ast.Constant(name),
                        Ast.ReadDefined(tmp)
                    ),
                    rule.MakeReturn(context.LanguageContext.Binder, Ast.ReadDefined(tmp))
                )
            );
        }

        private static Statement MakeGetAttrRule<T>(CodeContext context, GetMemberAction action, StandardRule<T> rule, Statement body, Variable tmp, DynamicTypeSlot getattr) {
            Expression slot = Ast.WeakConstant(getattr);
            return MakeGetAttrRule(context, action, rule, body, tmp, slot);
        }

        private static Statement MakeGetAttrRule<T>(CodeContext context, GetMemberAction action, StandardRule<T> rule, Statement body, Variable tmp, Expression getattr) {
            body = Ast.Block(
                body,
                Ast.If(
                    Ast.Call(
                        getattr,
                        typeof(DynamicTypeSlot).GetMethod("TryGetBoundValue"),
                        Ast.CodeContext(),
                        rule.Parameters[0],
                        Ast.ReadProperty(
                            Ast.Convert(
                                rule.Parameters[0],
                                typeof(ISuperDynamicObject)),
                            typeof(ISuperDynamicObject).GetProperty("DynamicType")
                        ),
                        Ast.ReadDefined(tmp)
                    ),
                    MakeGetAttrCall<T>(context, action, rule, tmp)
                )
            );
            return body;
        }

        private static Statement MakeGetAttrCall<T>(CodeContext context, GetMemberAction action, StandardRule<T> rule, Variable tmp) {
            Statement ret = rule.MakeReturn(context.LanguageContext.Binder,
                Ast.Action.Call(
                    typeof(object),
                    Ast.ReadDefined(tmp),
                    Ast.Constant(SymbolTable.IdToString(action.Name))
                )
            );

            if (action.IsNoThrow) {
                ret = Ast.Try(ret).Catch(typeof(MissingMemberException), ReturnOperationFailed<T>(context, rule));
            }

            return ret;
        }

        private static Statement ReturnOperationFailed<T>(CodeContext context, StandardRule<T> rule) {
            return rule.MakeReturn(context.LanguageContext.Binder, Ast.ReadField(null, typeof(OperationFailed).GetField("Value")));
        }

        private static BlockStatement MakeTypeError<T>(CodeContext context, DynamicType type, GetMemberAction action, StandardRule<T> rule, Statement body) {
            return Ast.Block(
                body,
                rule.MakeError(
                    context.LanguageContext.Binder,
                    Ast.Call(
                        null,
                        typeof(PythonOps).GetMethod("AttributeErrorForMissingAttribute", new Type[] { typeof(string), typeof(SymbolId) }),
                        Ast.Constant(DynamicTypeOps.GetName(type)),
                        Ast.Constant(action.Name)
                    )
                )
                
            );
        }

        private static Statement MakeSlotSet<T>(CodeContext context, StandardRule<T> rule, DynamicTypeSlot dts) {
            ReflectedProperty rp = dts as ReflectedProperty;
            if (rp != null) {
                // direct dispatch to the property...                
                MethodInfo setter = rp.Setter;
                if (setter.IsPublic && setter.DeclaringType.IsVisible) {
                    Expression[] args = rule.Parameters;
                    if (setter.IsStatic && !(rp is ReflectedExtensionProperty)) {
                        args = new Expression[] { rule.Parameters[1] };
                    }

                    return
                        rule.MakeReturn(
                            context.LanguageContext.Binder,
                            Ast.Comma(
                                new BinderHelper<T, CallAction>(context, CallAction.Make(args.Length)).MakeCallExpression(setter, args),
                                rule.Parameters[1]
                            )
                        );
                }
            }

            return 
                Ast.If(
                    Ast.Call(
                        Ast.WeakConstant(dts),
                        typeof(DynamicTypeSlot).GetMethod("TrySetValue"),
                        Ast.CodeContext(),
                        rule.Parameters[0],
                        Ast.ReadProperty(
                            Ast.Convert(
                                rule.Parameters[0],
                                typeof(ISuperDynamicObject)),
                            typeof(ISuperDynamicObject).GetProperty("DynamicType")
                        ),
                        rule.Parameters[1]
                    ),
                    rule.MakeReturn(context.LanguageContext.Binder, rule.Parameters[1])                
                );
        }

        private static Statement MakeSlotDelete<T>(CodeContext context, StandardRule<T> rule, DynamicTypeSlot dts) {
            return
                Ast.If(
                    Ast.Call(
                        Ast.WeakConstant(dts),
                        typeof(DynamicTypeSlot).GetMethod("TryDeleteValue"),
                        Ast.CodeContext(),
                        rule.Parameters[0],
                        Ast.ReadProperty(
                            Ast.Convert(
                                rule.Parameters[0],
                                typeof(ISuperDynamicObject)),
                            typeof(ISuperDynamicObject).GetProperty("DynamicType")
                        )
                    ),
                    rule.MakeReturn(context.LanguageContext.Binder, Ast.Null())
                );
        }
        private static BlockStatement MakeSlotAccess<T>(CodeContext context, StandardRule<T> rule, DynamicTypeSlot dts, Statement body, Variable tmp) {
            return Ast.Block(
                body,
                Ast.If(
                    Ast.Call(
                        Ast.WeakConstant(dts),
                        typeof(DynamicTypeSlot).GetMethod("TryGetBoundValue"),
                        Ast.CodeContext(),
                        rule.Parameters[0],
                        Ast.ReadProperty(
                            Ast.Convert(
                                rule.Parameters[0],
                                typeof(ISuperDynamicObject)),
                            typeof(ISuperDynamicObject).GetProperty("DynamicType")
                        ),
                        Ast.ReadDefined(tmp)
                    ),
                    rule.MakeReturn(context.LanguageContext.Binder, Ast.ReadDefined(tmp))
                )
            );
        }

        private static IfStatementBuilder MakeDictionaryAccess<T>(CodeContext context, GetMemberAction action, StandardRule<T> rule, Variable tmp) {
            return Ast.If(
                Ast.AndAlso(
                    Ast.NotEqual(
                        Ast.ReadProperty(
                            Ast.Convert(rule.Parameters[0], typeof(ISuperDynamicObject)),
                            typeof(ISuperDynamicObject).GetProperty("Dict")
                        ),
                        Ast.Constant(null)
                    ),
                    Ast.Call(
                        Ast.ReadProperty(
                            Ast.Convert(rule.Parameters[0], typeof(ISuperDynamicObject)),
                            typeof(ISuperDynamicObject).GetProperty("Dict")
                        ),
                        typeof(IAttributesCollection).GetMethod("TryGetValue"),
                        Ast.Constant(action.Name),
                        Ast.ReadDefined(tmp)
                    )
                ),
                rule.MakeReturn(context.LanguageContext.Binder, Ast.ReadDefined(tmp))
            );
        }

        private static StandardRule<T> MakeSetMemberRule<T>(CodeContext context, SetMemberAction action, object[] args) {
            ISuperDynamicObject sdo = args[0] as ISuperDynamicObject;
            StandardRule<T> rule = new StandardRule<T>();

            if (!(args[0] is ICustomMembers)) {
                string templateType = null;
                try {
                    // call __setattr__ if it exists
                    DynamicTypeSlot dts;
                    if (sdo.DynamicType.TryResolveSlot(context, Symbols.SetAttr, out dts) && !IsStandardObjectMethod(dts)) {
                        MakeSetAttrTarget<T>(context, action, sdo, rule, dts);
                        templateType = "__setattr__";
                        return rule;
                    }

                    // then see if we have a set descriptor
                    sdo.DynamicType.TryResolveSlot(context, action.Name, out dts);
                    ReflectedSlotProperty rsp = dts as ReflectedSlotProperty;
                    if (rsp != null) {
                        MakeSlotsSetTarget<T>(context, rule, rsp);
                        return rule;
                    }

                    if (dts != null && dts.IsSetDescriptor(context, sdo.DynamicType)) {
                        rule.SetTarget(MakeSlotSet<T>(context, rule, dts));
                        return rule;
                    }

                    // see if we can do a standard .NET binding...
                    StandardRule<T> baseRule = new SetMemberBinderHelper<T>(context, action, args).MakeNewRule();
                    if (!baseRule.IsError) {
                        baseRule.AddTest(PythonBinderHelper.MakeTypeTest(baseRule, sdo.DynamicType, baseRule.Parameters[0], false));
                        return baseRule;
                    }

                    // finally if we have a dictionary set the value there.
                    if (sdo.HasDictionary) {
                        MakeDictionarySetTarget<T>(context, action, rule);
                        templateType = "dictionary";
                        return rule;
                    }

                    // otherwise it's an error
                    rule.SetTarget(MakeThrowingAttributeError(context, action, sdo.DynamicType.Name, rule));
                    return rule;
                } finally {
                    // make the test for the rule depending on if we support templating or not.
                    if (templateType == null) {
                        PythonBinderHelper.MakeTest(rule, sdo.DynamicType);
                    } else {
                        MakeTemplatedSetMemberTest(context, action, sdo.DynamicType, rule, templateType);
                    }
                }
            } else {
                // TODO: When ICustomMembers goes away, or as it slowly gets replaced w/ IDynamicObject,
                // we'll need to call the base GetRule instead and merge that with our normal lookup
                // rules somehow.  Today ICustomMembers always takes precedence, so it does here too.
                PythonBinderHelper.MakeTest(rule, sdo.DynamicType);

                rule.SetTarget(MakeCustomMembersSetBody(context, action, DynamicTypeOps.GetName(sdo.DynamicType), rule));
                return rule;
            }
        }

        private static StandardRule<T> MakeDeleteMemberRule<T>(CodeContext context, DeleteMemberAction action, object[] args) {
            ISuperDynamicObject sdo = args[0] as ISuperDynamicObject;
            StandardRule<T> rule = new StandardRule<T>();
            PythonBinderHelper.MakeTest(rule, sdo.DynamicType);

            if (!(args[0] is ICustomMembers)) {
                if (action.Name == Symbols.Class) {
                    rule.SetTarget(
                        rule.MakeError(context.LanguageContext.Binder,
                            Ast.New(
                                typeof(ArgumentTypeException).GetConstructor(new Type[] { typeof(string) }),
                                Ast.Constant("can't delete __class__ attribute")
                            )
                        )
                    );
                    return rule;
                }

                // call __delattr__ if it exists
                DynamicTypeSlot dts;
                if (sdo.DynamicType.TryResolveSlot(context, Symbols.DelAttr, out dts) && !IsStandardObjectMethod(dts)) {
                    MakeDeleteAttrTarget<T>(context, action, sdo, rule, dts);
                    return rule;
                }

                // then see if we have a delete descriptor
                sdo.DynamicType.TryResolveSlot(context, action.Name, out dts);
                ReflectedSlotProperty rsp = dts as ReflectedSlotProperty;
                if (rsp != null) {
                    MakeSlotsDeleteTarget<T>(context, rule, rsp);
                    return rule;
                }

                // finally if we have a dictionary set the value there.
                if (sdo.HasDictionary) {
                    Statement body = MakeDictionaryDeleteTarget<T>(context, action, rule);
                    if (dts != null) {
                        body = Ast.Block(MakeSlotDelete<T>(context, rule, dts), body);
                    }

                    rule.SetTarget(body);
                    return rule;
                }

                if (dts != null) {
                    rule.SetTarget(MakeSlotDelete<T>(context, rule, dts));
                    return rule;
                }

                // otherwise it's an error
                rule.SetTarget(MakeThrowingAttributeError(context, action, sdo.DynamicType.Name, rule));
                return rule;
            } else {
                // TODO: When ICustomMembers goes away, or as it slowly gets replaced w/ IDynamicObject,
                // we'll need to call the base GetRule instead and merge that with our normal lookup
                // rules somehow.  Today ICustomMembers always takes precedence, so it does here too.
                rule.SetTarget(MakeCustomMembersDeleteBody(context, action, DynamicTypeOps.GetName(sdo.DynamicType), rule));
                return rule;
            }
        }

        private static void MakeSlotsSetTarget<T>(CodeContext context, StandardRule<T> rule, ReflectedSlotProperty rsp) {
            MakeSlotsSetTarget(context, rule, rsp, rule.Parameters[1]);
        }

        private static void MakeSlotsDeleteTarget<T>(CodeContext context, StandardRule<T> rule, ReflectedSlotProperty rsp) {
            MakeSlotsSetTarget(context, rule, rsp, Ast.ReadField(null, typeof(Uninitialized).GetField("Instance")));
        }

        private static void MakeSlotsSetTarget<T>(CodeContext context, StandardRule<T> rule, ReflectedSlotProperty rsp, Expression value) {
            Variable tmp = rule.GetTemporary(typeof(object), "res");

            // type has __slots__ defined for this member, call the setter directly
            rule.SetTarget(
                rule.MakeReturn(
                    context.LanguageContext.Binder,
                    Ast.Assign(tmp,
                        Ast.Convert(
                            Ast.Call(null, rsp.SetterMethod, rule.Parameters[0], value),
                            tmp.Type
                        )
                    )
                )
            );
        }

        private static Statement MakeDictionaryDeleteTarget<T>(CodeContext context, DeleteMemberAction action, StandardRule<T> rule) {
            return 
                rule.MakeReturn(
                    context.LanguageContext.Binder,
                    Ast.Call(
                        null,
                        typeof(UserTypeOps).GetMethod("RemoveDictionaryValue"),
                        Ast.Convert(rule.Parameters[0], typeof(ISuperDynamicObject)),
                        Ast.Constant(action.Name)
                    )
                );
        }

        private static void MakeDictionarySetTarget<T>(CodeContext context, SetMemberAction action, StandardRule<T> rule) {
            // return UserTypeOps.SetDictionaryValue(rule.Parameters[0], name, value);
            rule.SetTarget(
                rule.MakeReturn(
                    context.LanguageContext.Binder,
                    Ast.Call(
                        null,
                        typeof(UserTypeOps).GetMethod("SetDictionaryValue"),
                        Ast.Convert(rule.Parameters[0], typeof(ISuperDynamicObject)),
                        rule.AddTemplatedConstant(typeof(SymbolId), action.Name),
                        rule.Parameters[1]
                    )
                )
            );
        }

        private static void MakeSetAttrTarget<T>(CodeContext context, SetMemberAction action, ISuperDynamicObject sdo, StandardRule<T> rule, DynamicTypeSlot dts) {
            Variable tmp = rule.GetTemporary(typeof(object), "boundVal");
            // call __setattr__
            rule.SetTarget(
                Ast.IfThenElse(
                    Ast.Call(
                        rule.AddTemplatedWeakConstant(typeof(DynamicTypeSlot), dts),
                        typeof(DynamicTypeSlot).GetMethod("TryGetValue"),
                        Ast.CodeContext(),
                        rule.Parameters[0],
                        rule.AddTemplatedWeakConstant(typeof(DynamicType), sdo.DynamicType),
                        Ast.Read(tmp)
                    ),
                    rule.MakeReturn(
                        context.LanguageContext.Binder,
                        Ast.Action.Call(
                            typeof(object),
                            Ast.Read(tmp),
                            rule.AddTemplatedConstant(typeof(string), SymbolTable.IdToString(action.Name)),
                            rule.Parameters[1]
                        )
                    ),
                    MakeThrowingAttributeError<T>(context, action, sdo.DynamicType.Name, rule)
                )
            );
        }

        private static void MakeDeleteAttrTarget<T>(CodeContext context, DeleteMemberAction action, ISuperDynamicObject sdo, StandardRule<T> rule, DynamicTypeSlot dts) {
            Variable tmp = rule.GetTemporary(typeof(object), "boundVal");
            // call __delattr__
            rule.SetTarget(
                Ast.IfThenElse(
                    Ast.Call(
                        Ast.WeakConstant(dts),
                        typeof(DynamicTypeSlot).GetMethod("TryGetValue"),
                        Ast.CodeContext(),
                        rule.Parameters[0],
                        Ast.WeakConstant(sdo.DynamicType),
                        Ast.Read(tmp)
                    ),
                    rule.MakeReturn(
                        context.LanguageContext.Binder,
                        Ast.Action.Call(
                            typeof(object),
                            Ast.Read(tmp),
                            Ast.Constant(SymbolTable.IdToString(action.Name))
                        )
                    ),
                    MakeThrowingAttributeError<T>(context, action, sdo.DynamicType.Name, rule)
                )
            );
        }

        private static bool IsStandardObjectMethod(DynamicTypeSlot dts) {
            BuiltinMethodDescriptor bmd = dts as BuiltinMethodDescriptor;
            if (bmd == null) return false;
            return bmd.Template.Targets[0].DeclaringType == typeof(ObjectOps);
        }

        private static void MakeTemplatedSetMemberTest<T>(CodeContext context, SetMemberAction action, DynamicType targetType, StandardRule<T> rule, string templateType) {            
            bool altVersion = targetType.Version == DynamicType.DynamicVersion;
            string vername = altVersion ? "AlternateVersion" : "Version";
            int version = altVersion ? targetType.AlternateVersion : targetType.Version;
            rule.SetTest(
                Ast.AndAlso(
                    StandardRule.MakeTypeTestExpression(targetType.UnderlyingSystemType, rule.Parameters[0]),
                    Ast.Equal(
                        Ast.ReadProperty(
                            Ast.ReadProperty(
                                    Ast.Convert(rule.Parameters[0], typeof(ISuperDynamicObject)),
                                    typeof(ISuperDynamicObject).GetProperty("DynamicType")
                            ),
                            typeof(DynamicType).GetProperty(vername)
                        ),
                        rule.AddTemplatedConstant(typeof(int), version)
                    )
                )
            );

            RuleBuilderCache<T>.ParameterizeSetMember(targetType.UnderlyingSystemType, altVersion, context, rule, templateType);
        }

        private static class RuleBuilderCache<T> {
            private struct SetMemberKey {
                public SetMemberKey(Type type, bool version, string templateType) {
                    Type = type;
                    VersionOrAltVersion = version;
                    TemplateType = templateType;
                }

                public Type Type;
                public bool VersionOrAltVersion;
                public string TemplateType;
            }

            public static void ParameterizeSetMember(Type type, bool versionOrAltVersion, CodeContext context, StandardRule<T> rule, string templateType) {
                TemplatedRuleBuilder<T> builder;

                lock (SetMemberBuilders) {
                    SetMemberKey key = new SetMemberKey(type, versionOrAltVersion, templateType);
                    if (!SetMemberBuilders.TryGetValue(key, out builder)) {
                        SetMemberBuilders[key] = rule.GetTemplateBuilder();
                        return;
                    }
                }

                builder.CopyTemplateToRule(context, rule);                
            }

            private static Dictionary<SetMemberKey, TemplatedRuleBuilder<T>> SetMemberBuilders = new Dictionary<SetMemberKey, TemplatedRuleBuilder<T>>();
        }


        private static StandardRule<T> MakeOperationRule<T>(CodeContext context, DoOperationAction action, object[] args) {
            if (action.Operation == Operators.GetItem || action.Operation == Operators.SetItem) {
                ISuperDynamicObject sdo = args[0] as ISuperDynamicObject;
                
                if (sdo.DynamicType.Version != DynamicType.DynamicVersion &&
                    !HasBadSlice(context, sdo.DynamicType, action.Operation)) {
                    StandardRule<T> rule = new StandardRule<T>();
                    DynamicTypeSlot dts;

                    SymbolId item = action.Operation == Operators.GetItem ? Symbols.GetItem : Symbols.SetItem;

                    PythonBinderHelper.MakeTest(rule, DynamicTypeOps.ObjectTypes(args));

                    if (sdo.DynamicType.TryResolveSlot(context, item, out dts)) {

                        BuiltinMethodDescriptor bmd = dts as BuiltinMethodDescriptor;
                        if (bmd == null) {
                            Variable tmp = rule.GetTemporary(typeof(object), "res");
                            Expression[] exprargs = PythonBinderHelper.GetCollapsedIndexArguments<T>(action, args, rule);
                            exprargs[0] = Ast.ReadDefined(tmp);
                            rule.SetTarget(
                                Ast.Block(
                                    Ast.If(
                                        MakeTryGetTypeMember<T>(rule, dts, tmp),
                                        rule.MakeReturn(
                                            context.LanguageContext.Binder,
                                            Ast.Action.Call(
                                                typeof(object),
                                                exprargs
                                            )
                                        )
                                    ),
                                    rule.MakeError(context.LanguageContext.Binder, MakeIndexerError<T>(rule, item))
                                )
                            );
                        } else {
                            // call to .NET function, don't collapse the arguments.
                            MethodBinder mb = MethodBinder.MakeBinder(context.LanguageContext.Binder, SymbolTable.IdToString(item), bmd.Template.Targets, BinderType.Normal);
                            MethodCandidate mc = mb.MakeBindingTarget(CallType.ImplicitInstance, CompilerHelpers.GetTypes(args));
                            if (mc != null) {
                                Expression callExpr = mc.Target.MakeExpression(context.LanguageContext.Binder, rule, rule.Parameters);

                                rule.SetTarget(rule.MakeReturn(context.LanguageContext.Binder, callExpr));
                            } else {
                                // go dynamic for error case
                                return null;
                            }
                        }
                    } else {
                        rule.SetTarget(rule.MakeError(context.LanguageContext.Binder, MakeIndexerError<T>(rule, item)));
                    }

                    return rule;
                }
            }

            return null;
        }

        private static bool HasBadSlice(CodeContext context, DynamicType type, Operators op) {
            DynamicTypeSlot dts;

            if(!type.TryResolveSlot(context, op == Operators.GetItem ? Symbols.GetSlice : Symbols.SetSlice, out dts)){
                return false;
            }

            BuiltinFunction bf = dts as BuiltinFunction;
            if(bf == null) {
                BuiltinMethodDescriptor bmd = dts as BuiltinMethodDescriptor;
                if(bmd == null) {
                    return true;
                }
                bf = bmd.Template;
            }

            return bf.DeclaringType != type.UnderlyingSystemType.BaseType;            
        }

        private static MethodCallExpression MakeIndexerError<T>(StandardRule<T> rule, SymbolId item) {
            return Ast.Call(
                null,
                typeof(PythonOps).GetMethod("AttributeErrorForMissingAttribute", new Type[] { typeof(object), typeof(SymbolId) }),
                Ast.ReadProperty(
                    Ast.Convert(
                        rule.Parameters[0],
                        typeof(ISuperDynamicObject)
                    ),
                    typeof(ISuperDynamicObject).GetProperty("DynamicType")
                ),
                Ast.Constant(item)
            );
        }

        private static MethodCallExpression MakeTryGetTypeMember<T>(StandardRule<T> rule, DynamicTypeSlot dts, Variable tmp) {
            return Ast.Call(
                Ast.WeakConstant(dts),
                typeof(DynamicTypeSlot).GetMethod("TryGetBoundValue"),
                Ast.CodeContext(),
                rule.Parameters[0],
                Ast.ReadProperty(
                    Ast.Convert(
                        rule.Parameters[0],
                        typeof(ISuperDynamicObject)),
                    typeof(ISuperDynamicObject).GetProperty("DynamicType")
                ),
                Ast.ReadDefined(tmp)
            );
        }

        public static object SetDictionaryValue(ISuperDynamicObject self, SymbolId name, object value) {
            IAttributesCollection dict = GetDictionary(self);

            return dict[name] = value;
        }

        public static void RemoveDictionaryValue(ISuperDynamicObject self, SymbolId name) {
            IAttributesCollection dict = self.Dict;
            if (dict != null) {
                if (dict.Remove(name)) {
                    return;
                }
            }

            throw PythonOps.AttributeErrorForMissingAttribute(self.DynamicType, name);
        }

        private static IAttributesCollection GetDictionary(ISuperDynamicObject self) {
            IAttributesCollection dict = self.Dict;
            if (dict == null) {
                dict = self.SetDict(new SymbolDictionary());
            }
            return dict;
        }

        /// <summary>
        /// Object.ToString() displays the CLI type name.  But we want to display the class name (e.g.
        /// '&lt;foo object at 0x000000000000002C&gt;' unless we've overridden __repr__ but not __str__ in 
        /// which case we'll display the result of __repr__.
        /// </summary>
        public static string ToStringHelper(ISuperDynamicObject o) {

            object ret;
            DynamicType ut = o.DynamicType;
            Debug.Assert(ut != null);

            DynamicTypeSlot dts;
            if (ut.TryResolveSlot(DefaultContext.Default, Symbols.Repr, out dts) &&
                dts.TryGetValue(DefaultContext.Default, o, ut, out ret)) {

                string strRet;
                if (ret != null && Converter.TryConvertToString(PythonCalls.Call(ret), out strRet)) return strRet;
                throw PythonOps.TypeError("__repr__ returned non-string type ({0})", DynamicHelpers.GetDynamicType(ret).Name);
            }

            return TypeHelpers.ReprMethod(o);
        }

        public static bool TryGetNonInheritedMethodHelper(DynamicType dt, object instance, SymbolId name, out object callTarget) {
            // search MRO for other user-types in the chain that are overriding the method
            foreach(DynamicType type in dt.ResolutionOrder) {
                if (type.IsSystemType) break;           // hit the .NET types, we're done

                if (LookupValue(type, instance, name, out callTarget)) {
                    return true;
                }
            }

            // check instance
            ISuperDynamicObject isdo = instance as ISuperDynamicObject;
            IAttributesCollection iac;
            if (isdo != null && (iac = isdo.Dict) != null) {
                if (iac.TryGetValue(name, out callTarget))
                    return true;
            }

            callTarget = null;
            return false;
        }

        private static bool LookupValue(DynamicType dt, object instance, SymbolId name, out object value) {
            DynamicTypeSlot dts;
            if (dt.TryLookupSlot(DefaultContext.Default, name, out dts) &&
                dts.TryGetValue(DefaultContext.Default, instance, dt, out value)) {
                return true;
            }
            value = null;
            return false;
        }

        public static bool TryGetNonInheritedValueHelper(DynamicType dt, object instance, SymbolId name, out object callTarget) {
            DynamicTypeSlot dts;
            // search MRO for other user-types in the chain that are overriding the method
           foreach(DynamicType type in dt.ResolutionOrder) {
                if (type.IsSystemType) break;           // hit the .NET types, we're done

                if (type.TryLookupSlot(DefaultContext.Default, name, out dts)) {
                    callTarget = dts;
                    return true;
                }
            }
            
            // check instance
            ISuperDynamicObject isdo = instance as ISuperDynamicObject;
            IAttributesCollection iac;
            if (isdo != null && (iac = isdo.Dict) != null) {
                if (iac.TryGetValue(name, out callTarget))
                    return true;
            }

            callTarget = null;
            return false;
        }

        #region IValueEquality Helpers

        public static int GetValueHashCodeHelper(object self) {
            // new-style classes only lookup in slots, not in instance
            // members
            object func;
            if (DynamicHelpers.GetDynamicType(self).TryGetBoundMember(DefaultContext.Default, self, Symbols.Hash, out func)) {
                return Converter.ConvertToInt32(PythonCalls.Call(func));
            }

            return self.GetHashCode();
        }

        public static bool ValueEqualsHelper(object self, object other) {
            object res = RichEqualsHelper(self, other);
            if (res != PythonOps.NotImplemented && res != null && res.GetType() == typeof(bool))
                return (bool)res;

            return false;
        }

        private static object RichEqualsHelper(object self, object other) {
            object res;

            if (DynamicHelpers.GetDynamicType(self).TryInvokeBinaryOperator(DefaultContext.Default, Operators.Equals, self, other, out res))
                return res;

            return PythonOps.NotImplemented;
        }

        public static bool ValueNotEqualsHelper(object self, object other) {
            object res;
            if (DynamicHelpers.GetDynamicType(self).TryInvokeBinaryOperator(DefaultContext.Default, Operators.NotEquals, self, other, out res) && 
                res != PythonOps.NotImplemented &&
                res != null &&
                res.GetType() == typeof(bool))
                return (bool)res;

            return false;
        }

        #endregion
    }
}
