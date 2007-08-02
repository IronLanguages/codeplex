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
using Microsoft.Scripting;

using IronPython.Runtime.Types;
using IronPython.Runtime.Calls;

using System.Diagnostics;
using Microsoft.Scripting.Actions;
using IronPython.Hosting;
using System.Reflection;
using Microsoft.Scripting.Ast;

namespace IronPython.Runtime.Operations {
    using Ast = Microsoft.Scripting.Ast.Ast;
    using Microsoft.Scripting.Generation;

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
                default: return null;
            }
        }

        private static StandardRule<T> MakeGetMemberRule<T>(CodeContext context, GetMemberAction action, object[] args) {
            DynamicTypeSlot dts;
            ISuperDynamicObject sdo = (ISuperDynamicObject)args[0];
            StandardRule<T> rule = new StandardRule<T>();
            Variable tmp = rule.GetTemporary(typeof(object), "lookupRes");

            rule.MakeTest(sdo.DynamicType);

            if (TryGetGetAttribute(context, sdo, out dts)) {
                // type defines __getattribute__, just call it.

                rule.SetTarget(MakeGetAttrRule<T>(context, action, rule, Ast.Empty(), tmp, dts));
            } else if (!(args[0] is ICustomMembers)) {
                // fast path for accessing properties from a derived type.
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
                body = MakeTypeError<T>(context, action, rule, body);

                rule.SetTarget(body);
            } else {
                // TODO: When ICustomMembers goes away, or as it slowly gets replaced w/ IDynamicObject,
                // we'll need to call the base GetRule instead and merge that with our normal lookup
                // rules somehow.  Today ICustomMembers always takes precedence, so it does here too.
                rule.SetTarget(MakeCustomMembersBody(context, action, DynamicTypeOps.GetName(sdo.DynamicType), rule));       
            }

            return rule;
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
            body = Ast.Block(
                body,
                Ast.If(
                    Ast.Call(
                        Ast.WeakConstant(getattr),
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

        private static BlockStatement MakeTypeError<T>(CodeContext context, GetMemberAction action, StandardRule<T> rule, Statement body) {
            return Ast.Block(
                body,
                rule.MakeError(
                    context.LanguageContext.Binder,
                    Ast.Call(
                        null,
                        typeof(PythonOps).GetMethod("AttributeErrorForMissingAttribute", new Type[] { typeof(object), typeof(SymbolId) }),
                        rule.Parameters[0],
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

            return MakeDynamicSetMemberRule<T>(action.Name, ((ISuperDynamicObject)args[0]).DynamicType);
        }
        
        private static StandardRule<T> MakeDynamicSetMemberRule<T>(SymbolId name, DynamicType targetType) {
            StandardRule<T> rule = new StandardRule<T>();
            rule.MakeTest(targetType);
            Expression expr = Ast.Call(null,
                    typeof(PythonOps).GetMethod("SetAttr"),
                    Ast.CodeContext(),
                    rule.Parameters[0],
                    Ast.Constant(name),
                    rule.Parameters[1]);
            rule.SetTarget(rule.MakeReturn(PythonEngine.CurrentEngine.DefaultBinder, expr));
            return rule;
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
                            MethodCandidate mc = mb.MakeBindingTarget(CallType.ImplicitInstance, CompilerHelpers.ObjectTypes(args));
                            if (mc != null) {
                                Expression callExpr = mc.Target.MakeExpression(context.LanguageContext.Binder, rule.Parameters);

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
