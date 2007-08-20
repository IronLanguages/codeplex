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

        public static StandardRule<T> GetRuleHelper<T>(Action action, CodeContext context, object[] args) {
            switch (action.Kind) {
                case ActionKind.GetMember: return MakeGetMemberRule<T>(context, (GetMemberAction)action, args);
                case ActionKind.SetMember: return MakeSetMemberRule<T>(context, (SetMemberAction)action, args);
                case ActionKind.DoOperation: return MakeOperationRule<T>(context, (DoOperationAction)action, args);
                case ActionKind.Call: return MakeCallRule<T>(context, (CallAction)action, args);
                    
                default: return null;
            }
        }

        private static StandardRule<T> MakeCallRule<T>(CodeContext context, CallAction callAction, object[] args) {
            StandardRule<T> rule = new StandardRule<T>();            

            ISuperDynamicObject sdo = (ISuperDynamicObject)args[0];

            rule.MakeTest(sdo.DynamicType);

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
                            Ast.Cast(Ast.WeakConstant(callSlot), typeof(DynamicTypeSlot)),
                            typeof(DynamicTypeSlot).GetMethod("TryGetValue"),
                            Ast.CodeContext(),
                            rule.Parameters[0],
                            Ast.ReadProperty(
                                Ast.Cast(rule.Parameters[0], typeof(ISuperDynamicObject)),
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
                rule.MakeTest(sdo.DynamicType);

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
                body = MakeTypeError<T>(context, sdo.DynamicType, action, rule, body);

                rule.SetTarget(body);
            } else {
                // TODO: When ICustomMembers goes away, or as it slowly gets replaced w/ IDynamicObject,
                // we'll need to call the base GetRule instead and merge that with our normal lookup
                // rules somehow.  Today ICustomMembers always takes precedence, so it does here too.
                rule.MakeTest(sdo.DynamicType);

                rule.SetTarget(MakeCustomMembersBody(context, action, DynamicTypeOps.GetName(sdo.DynamicType), rule));       
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
                                Ast.Cast(rule.Parameters[0], typeof(ISuperDynamicObject)),
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
                                    Ast.Cast(rule.Parameters[0], typeof(ISuperDynamicObject)),
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

        internal static Statement MakeCustomMembersBody<T>(CodeContext context, GetMemberAction action, string typeName, StandardRule<T> rule) {
            Variable tmp = rule.GetTemporary(typeof(object), "custmemres");

            return Ast.IfThenElse(
                        Ast.Call(
                            Ast.Cast(rule.Parameters[0], typeof(ICustomMembers)),
                            typeof(ICustomMembers).GetMethod("TryGetBoundCustomMember"),
                            Ast.CodeContext(),
                            Ast.Constant(action.Name),
                            Ast.Read(tmp)
                        ),
                        rule.MakeReturn(context.LanguageContext.Binder, Ast.Read(tmp)),
                        rule.MakeError(context.LanguageContext.Binder,
                            Ast.Call(null,
                                typeof(PythonOps).GetMethod("AttributeErrorForMissingAttribute", new Type[] { typeof(string), typeof(SymbolId) }),
                                Ast.Constant(typeName),
                                Ast.Constant(action.Name)
                            )
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
                if (bmd == null || bmd.DeclaringType != TypeCache.Object ||
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
                            Ast.Cast(
                                rule.Parameters[0],
                                typeof(ISuperDynamicObject)),
                            typeof(ISuperDynamicObject).GetProperty("DynamicType")
                        ),
                        Ast.ReadDefined(tmp)
                    ),
                    rule.MakeReturn(context.LanguageContext.Binder,
                        Ast.Action.Call(
                            CallAction.Simple,
                            typeof(object),
                            Ast.ReadDefined(tmp),
                            Ast.Constant(SymbolTable.IdToString(action.Name))
                        )
                    )

                )
            );
            return body;
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
                            Ast.Cast(
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
                            Ast.Cast(rule.Parameters[0], typeof(ISuperDynamicObject)),
                            typeof(ISuperDynamicObject).GetProperty("Dict")
                        ),
                        Ast.Constant(null)
                    ),
                    Ast.Call(
                        Ast.ReadProperty(
                            Ast.Cast(rule.Parameters[0], typeof(ISuperDynamicObject)),
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

        private static StandardRule<T> MakeDynamicGetMemberRule<T>(SymbolId name, DynamicType targetType) {
            StandardRule<T> rule = new StandardRule<T>();
            rule.MakeTest(targetType);
            Expression expr = Ast.Call(null,
                    typeof(PythonOps).GetMethod("GetBoundAttr"),
                    Ast.CodeContext(),
                    rule.Parameters[0],
                    Ast.Constant(name));
            rule.SetTarget(rule.MakeReturn(PythonEngine.CurrentEngine.DefaultBinder, expr));
            return rule;
        }

        private static StandardRule<T> MakeSetMemberRule<T>(CodeContext context, SetMemberAction action, object[] args) {
            ISuperDynamicObject sdo = args[0] as ISuperDynamicObject;

            if (sdo.DynamicType.Version != DynamicType.DynamicVersion && !(args[0] is ICustomMembers)) {
                DynamicTypeSlot dts;
                sdo.DynamicType.TryResolveSlot(context, action.Name, out dts);

                ReflectedSlotProperty rsp = dts as ReflectedSlotProperty;
                if (rsp != null) {
                    StandardRule<T> rule = new StandardRule<T>();
                    rule.MakeTest(sdo.DynamicType);
                    Variable tmp = rule.GetTemporary(typeof(object), "res");

                    // type has __slots__ defined for this member, call the setter directly
                    rule.SetTarget(
                        rule.MakeReturn(
                            context.LanguageContext.Binder,
                            Ast.Assign(tmp,
                                Ast.Call(null, rsp.SetterMethod, rule.Parameters[0], rule.Parameters[1])
                            )
                        )
                    );
                    return rule;
                }
            }

            return MakeDynamicSetMemberRule<T>(context, action, ((ISuperDynamicObject)args[0]).DynamicType);
        }

        private static StandardRule<T> MakeDynamicSetMemberRule<T>(CodeContext context, SetMemberAction action, DynamicType targetType) {
            bool altVersion = targetType.Version == DynamicType.DynamicVersion;
            string vername = altVersion ? "AlternateVersion" : "Version";
            int version = altVersion ? targetType.AlternateVersion : targetType.Version;
            
            StandardRule<T> rule = new StandardRule<T>();

            rule.SetTest(
                Ast.AndAlso(
                    rule.MakeTypeTestExpression(targetType.UnderlyingSystemType, rule.Parameters[0]),
                    Ast.Equal(
                        Ast.ReadProperty(
                            Ast.ReadProperty(
                                    Ast.Cast(rule.Parameters[0], typeof(ISuperDynamicObject)),
                                    typeof(ISuperDynamicObject).GetProperty("DynamicType")
                            ),
                            typeof(DynamicType).GetProperty(vername)
                        ),
                        rule.AddTemplatedConstant(typeof(int), version)
                    )
                )
            );

            Expression expr = Ast.Call(null,
                    typeof(PythonOps).GetMethod("SetAttr"),
                    Ast.CodeContext(),
                    rule.Parameters[0],
                    rule.AddTemplatedConstant(typeof(SymbolId), action.Name),
                    rule.Parameters[1]);
            rule.SetTarget(rule.MakeReturn(PythonEngine.CurrentEngine.DefaultBinder, expr));

            RuleBuilderCache<T>.ParameterizeSetMember(targetType.UnderlyingSystemType, altVersion, context, action, rule, version, action.Name);
            return rule;
        }

        private static class RuleBuilderCache<T> {
            public static void ParameterizeSetMember(Type type, bool versionOrAltVersion, CodeContext context, Action action, StandardRule<T> rule, params object[] args) {
                TemplatedRuleBuilder<T> builder;

                lock (SetMemberBuilders) {
                    KeyValuePair<Type, bool> kvp = new KeyValuePair<Type, bool>(type, versionOrAltVersion);
                    if (!SetMemberBuilders.TryGetValue(kvp, out builder)) {
                        SetMemberBuilders[kvp] = rule.GetTemplateBuilder();
                        return;
                    }
                }

                builder.CopyTemplateToRule(context, rule, args);                
            }

            public static Dictionary<KeyValuePair<Type, bool>, TemplatedRuleBuilder<T>> SetMemberBuilders = new Dictionary<KeyValuePair<Type, bool>, TemplatedRuleBuilder<T>>();
        }


        private static StandardRule<T> MakeOperationRule<T>(CodeContext context, DoOperationAction action, object[] args) {
            if (action.Operation == Operators.GetItem || action.Operation == Operators.SetItem) {
                ISuperDynamicObject sdo = args[0] as ISuperDynamicObject;
                
                if (sdo.DynamicType.Version != DynamicType.DynamicVersion &&
                    !HasBadSlice(context, sdo.DynamicType, action.Operation)) {
                    StandardRule<T> rule = new StandardRule<T>();
                    DynamicTypeSlot dts;

                    SymbolId item = action.Operation == Operators.GetItem ? Symbols.GetItem : Symbols.SetItem;

                    rule.MakeTest(CompilerHelpers.ObjectTypes(args));

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
                                                CallAction.Simple,
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

            return bf.DeclaringType.UnderlyingSystemType != type.UnderlyingSystemType.BaseType;            
        }

        private static MethodCallExpression MakeIndexerError<T>(StandardRule<T> rule, SymbolId item) {
            return Ast.Call(
                null,
                typeof(PythonOps).GetMethod("AttributeErrorForMissingAttribute", new Type[] { typeof(object), typeof(SymbolId) }),
                Ast.ReadProperty(
                    Ast.Cast(
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
                    Ast.Cast(
                        rule.Parameters[0],
                        typeof(ISuperDynamicObject)),
                    typeof(ISuperDynamicObject).GetProperty("DynamicType")
                ),
                Ast.ReadDefined(tmp)
            );
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

            if (DynamicHelpers.GetDynamicType(self).TryInvokeBinaryOperator(DefaultContext.Default, Operators.Equal, self, other, out res))
                return res;

            return PythonOps.NotImplemented;
        }

        public static bool ValueNotEqualsHelper(object self, object other) {
            object res;
            if (DynamicHelpers.GetDynamicType(self).TryInvokeBinaryOperator(DefaultContext.Default, Operators.NotEqual, self, other, out res) && 
                res != PythonOps.NotImplemented &&
                res != null &&
                res.GetType() == typeof(bool))
                return (bool)res;

            return false;
        }

        #endregion
    }
}
