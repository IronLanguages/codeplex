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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Utils;

using IronPython.Compiler.Generation;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Types;

namespace IronPython.Runtime.Operations {
    using Ast = Microsoft.Scripting.Ast.Ast;

    // These operations get linked into all new-style classes. 
    public static class UserTypeOps {
        public static string ToStringReturnHelper(object o) {
            if (o is string && o != null) {
                return (string)o;
            }
            throw PythonOps.TypeError("__str__ returned non-string type ({0})", PythonTypeOps.GetName(o));
        }

        public static IAttributesCollection SetDictHelper(ref IAttributesCollection iac, IAttributesCollection value) {
            if (System.Threading.Interlocked.CompareExchange<IAttributesCollection>(ref iac, value, null) == null)
                return value;
            return iac;
        }

        public static object GetPropertyHelper(object prop, object instance, SymbolId name) {
            PythonTypeSlot desc = prop as PythonTypeSlot;
            if (desc == null) {
                throw PythonOps.TypeError("Expected property for {0}, but found {1}",
                    name.ToString(), DynamicHelpers.GetPythonType(prop).Name);
            }
            object value;
            desc.TryGetValue(DefaultContext.Default, instance, DynamicHelpers.GetPythonType(instance), out value);
            return value;
        }

        public static void SetPropertyHelper(object prop, object instance, object newValue, SymbolId name) {
            PythonTypeSlot desc = prop as PythonTypeSlot;
            if (desc == null) {
                throw PythonOps.TypeError("Expected settable property for {0}, but found {1}",
                    name.ToString(), DynamicHelpers.GetPythonType(prop).Name);
            }
            desc.TrySetValue(DefaultContext.Default, instance, DynamicHelpers.GetPythonType(instance), newValue);
        }

        public static void AddRemoveEventHelper(object method, object instance, PythonType dt, object eventValue, SymbolId name) {
            object callable = method;
            
            PythonTypeSlot dts = method as PythonTypeSlot;
            if(dts != null) {
                if (!dts.TryGetValue(DefaultContext.Default, instance, dt, out callable))
                    throw PythonOps.AttributeErrorForMissingAttribute(dt.Name, name);
            } 

            if (!PythonOps.IsCallable(callable)) {
                throw PythonOps.TypeError("Expected callable value for {0}, but found {1}", name.ToString(),
                    PythonTypeOps.GetName(method));
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
                case DynamicActionKind.ConvertTo: return MakeConvertRule<T>(context, (ConvertToAction)action, args);
                default: return null;
            }
        }

        #region Conversion support

        private static StandardRule<T> MakeConvertRule<T>(CodeContext context, ConvertToAction convertToAction, object[] args) {
            Contract.Requires(args.Length == 1, "args", "args must contain 1 argument for conversion");
            Contract.Requires(args[0] is IPythonObject, "args[0]", "must be IPythonObject");

            Type toType = convertToAction.ToType;
            StandardRule<T> res = null;
            if (toType == typeof(int)) {
                res = MakeConvertRuleForCall<T>(context, convertToAction, args[0], toType, Symbols.ConvertToInt, "ConvertToInt");
            } else if (toType == typeof(BigInteger)) {
                res = MakeConvertRuleForCall<T>(context, convertToAction, args[0], toType, Symbols.ConvertToLong, "ConvertToLong");
            } else if (toType == typeof(double)) {
                res = MakeConvertRuleForCall<T>(context, convertToAction, args[0], toType, Symbols.ConvertToFloat, "ConvertToFloat");
            } else if (toType == typeof(Complex64)) {
                res = MakeConvertRuleForCall<T>(context, convertToAction, args[0], toType, Symbols.ConvertToComplex, "ConvertToComplex");
                if (res == null) {
                    res = MakeConvertRuleForCall<T>(context, convertToAction, args[0], toType, Symbols.ConvertToFloat, "ConvertToComplex");
                }
            } else if (toType == typeof(bool)) {
                // __nonzero__
                res = MakeConvertRuleForCall<T>(context, convertToAction, args[0], toType, Symbols.NonZero, "ConvertToNonZero");
                if (res == null) {
                    // __len__
                    res = MakeConvertLengthToBoolRule<T>(context, convertToAction, args[0]);
                }
            } else if (toType == typeof(IEnumerable)) {
                res = MakeConvertToIEnumerable<T>(context, args);
            } else if (toType == typeof(IEnumerator)) {
                res = MakeConvertToIEnumerator<T>(context, args);
            }
            
            if (res == null) {
                // make the basic rule w/ but we'll add our test for the dynamic type
                res = new PythonConvertToBinderHelper<T>(context, convertToAction, args).MakeRule() ??
                      new ConvertToBinderHelper<T>(context, convertToAction, args).MakeRule();
            }
            res.AddTest(PythonBinderHelper.MakeTestForTypes(res, new PythonType[] { ((IPythonObject)args[0]).PythonType }, 0));
            return res;
        }

        private static StandardRule<T> MakeConvertToIEnumerable<T>(CodeContext context, object[] args) {
            PythonType pt = ((IPythonObject)args[0]).PythonType;
            PythonTypeSlot pts;
            StandardRule<T> rule = null;
            if (pt.TryResolveSlot(context, Symbols.Iterator, out pts)) {                
                rule = MakeIterRule<T>(context, typeof(PythonEnumerable));
            } else if (pt.TryResolveSlot(context, Symbols.GetItem, out pts)) {                
                rule = MakeIterRule<T>(context, typeof(ItemEnumerable));
            }
            return rule;
        }
        
        private static StandardRule<T> MakeConvertToIEnumerator<T>(CodeContext context, object[] args) {
            PythonType pt = ((IPythonObject)args[0]).PythonType;
            PythonTypeSlot pts;
            StandardRule<T> rule = null;
            if (pt.TryResolveSlot(context, Symbols.Iterator, out pts)) {
                rule = MakeIterRule<T>(context, typeof(PythonEnumerator));
            } else if (pt.TryResolveSlot(context, Symbols.GetItem, out pts)) {
                rule = MakeIterRule<T>(context, typeof(ItemEnumerator));
            }
            return rule;
        }

        private static StandardRule<T> MakeIterRule<T>(CodeContext context, Type t) {
            StandardRule<T> res = new StandardRule<T>();
            res.SetTarget(
                res.MakeReturn(
                    context.LanguageContext.Binder,
                    Ast.Call(
                        t.GetMethod("Create"),
                        res.Parameters[0]
                    )
                )
            );
            return res;
        }

        private static StandardRule<T> MakeConvertLengthToBoolRule<T>(CodeContext context, ConvertToAction convertToAction, object self) {
            PythonType pt = ((IPythonObject)self).PythonType;
            PythonTypeSlot pts;

            if (pt.TryResolveSlot(context, Symbols.Length, out pts)) {
                StandardRule<T> rule = new StandardRule<T>();
                Variable tmp = rule.GetTemporary(typeof(object), "func");

                rule.SetTarget(
                    Ast.Block(
                        Ast.If(
                            MakeTryGetTypeMember<T>(rule, pts, tmp),
                            rule.MakeReturn(
                                context.LanguageContext.Binder,
                                PythonBinderHelper.GetConvertByLengthBody(tmp)
                            )
                        ),
                        PythonBinderHelper.GetConversionFailedReturnValue<T>(context, convertToAction, rule)
                    )
                );

                return rule;
            }
            return null;
        }

        private static StandardRule<T> MakeConvertRuleForCall<T>(CodeContext context, ConvertToAction convertToAction, object self, Type toType, SymbolId symbolId, string returner) {
            PythonType pt = ((IPythonObject)self).PythonType;
            PythonTypeSlot pts;

            if (pt.TryResolveSlot(context, symbolId, out pts) && !IsBuiltinConversion(context, pts, symbolId, pt)) {                
                StandardRule<T> rule = new StandardRule<T>();
                Variable tmp = rule.GetTemporary(typeof(object), "func");

                Expression callExpr = Ast.Call(
                    PythonOps.GetConversionHelper(returner, convertToAction.ResultKind),
                    Ast.Action.Call(
                        typeof(object),
                        Ast.Read(tmp)
                    )
                );

                if (toType == rule.ReturnType && typeof(Extensible<>).MakeGenericType(toType).IsAssignableFrom(self.GetType())) {
                    // if we're doing a conversion to the underlying type and we're an 
                    // Extensible<T> of that type:

                    // if an extensible type returns it's self in a conversion, then we need 
                    // to actually return the underlying value.  If an extensible just keeps 
                    // returning more instances  of it's self a stack overflow occurs - both 
                    // behaviors match CPython.
                    callExpr = AddExtensibleSelfCheck<T>(self, toType, rule, tmp, callExpr);
                }

                rule.SetTarget(
                    Ast.Block(
                        Ast.If(
                            MakeTryGetTypeMember<T>(rule, pts, tmp),
                            rule.MakeReturn(
                                context.LanguageContext.Binder,
                                callExpr
                            )
                        ),
                        PythonBinderHelper.GetConversionFailedReturnValue<T>(context, convertToAction, rule)
                    )
                );

                return rule;
            }

            return null;
        }

        private static bool IsBuiltinConversion(CodeContext context, PythonTypeSlot pts, SymbolId name, PythonType selfType) {
            Type baseType = selfType.UnderlyingSystemType.BaseType;
            Type tmpType = baseType;
            do {
                if (tmpType.IsGenericType && tmpType.GetGenericTypeDefinition() == typeof(Extensible<>)) {
                    baseType = tmpType.GetGenericArguments()[0];
                    break;
                }
                tmpType = tmpType.BaseType;
            } while (tmpType != null);
            
            PythonType ptBase = DynamicHelpers.GetPythonTypeFromType(baseType);
            PythonTypeSlot baseSlot;
            if(ptBase.TryResolveSlot(context, name, out baseSlot) && pts == baseSlot) {
                return true;
            }

            return false;
        }

        private static Expression AddExtensibleSelfCheck<T>(object self, Type toType, StandardRule<T> rule, Variable tmp, Expression callExpr) {
            callExpr = Ast.Comma(
                Ast.Assign(tmp, callExpr),
                Ast.Condition(
                    Ast.Equal(Ast.Read(tmp), rule.Parameters[0]),
                    Ast.ReadProperty(
                        Ast.Convert(rule.Parameters[0], self.GetType()),
                        self.GetType().GetProperty("Value")
                    ),
                    Ast.Action.ConvertTo(
                        toType,
                        ConversionResultKind.ExplicitCast,
                        Ast.Read(tmp)
                    )
                )
            );
            return callExpr;
        }

        #endregion

        private static StandardRule<T> MakeCallRule<T>(CodeContext context, CallAction callAction, object[] args) {
            StandardRule<T> rule = new StandardRule<T>();            

            IPythonObject sdo = (IPythonObject)args[0];

            PythonBinderHelper.MakeTest(rule, sdo.PythonType);

            PythonTypeSlot callSlot;
            Expression body = rule.MakeError(
                Ast.Call(
                    typeof(PythonOps).GetMethod("UncallableError"),
                    Ast.ConvertHelper(rule.Parameters[0], typeof(object))
                )
            );

            if (sdo.PythonType.TryResolveSlot(context, Symbols.Call, out callSlot)) {
                Variable tmp = rule.GetTemporary(typeof(object), "callSlot");
                Expression[] callArgs = (Expression[])rule.Parameters.Clone();
                callArgs[0] = Ast.Read(tmp);

                body = Ast.Block(
                    Ast.If(
                        Ast.Call(
                            typeof(PythonOps).GetMethod("SlotTryGetValue"),
                            Ast.CodeContext(),
                            Ast.Convert(Ast.WeakConstant(callSlot), typeof(PythonTypeSlot)),
                            Ast.ConvertHelper(rule.Parameters[0], typeof(object)),
                            Ast.Convert(
                                Ast.ReadProperty(
                                    Ast.Convert(rule.Parameters[0], typeof(IPythonObject)),
                                    typeof(IPythonObject).GetProperty("PythonType")
                                ),
                                typeof(PythonType)
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
            PythonTypeSlot dts;
            IPythonObject sdo = (IPythonObject)args[0];
            StandardRule<T> rule = new StandardRule<T>();
            Variable tmp = rule.GetTemporary(typeof(object), "lookupRes");

            if (TryGetGetAttribute(context, sdo, out dts)) {
                Debug.Assert(sdo.PythonType.HasGetAttribute);

                MakeGetAttributeRule<T>(context, action, rule, tmp);
            } else if (!(args[0] is ICustomMembers)) {
                // fast path for accessing public properties from a derived type.
                PythonBinderHelper.MakeTest(rule, sdo.PythonType);

                Type userType = args[0].GetType();
                PropertyInfo pi = userType.GetProperty(SymbolTable.IdToString(action.Name));
                if (pi != null) {
                    MethodInfo getter = pi.GetGetMethod();
                    if (getter != null && getter.IsPublic) {
                        rule.SetTarget(
                            rule.MakeReturn(
                                context.LanguageContext.Binder, 
                                Ast.Call(
                                    Ast.ConvertHelper(rule.Parameters[0], getter.DeclaringType),
                                    getter
                                )
                            )
                        );
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
                IList<PythonType> mro = sdo.PythonType.ResolutionOrder;
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
                            Ast.Comma(
                                Ast.Assign(
                                    tmp,
                                    Ast.SimpleCallHelper(
                                        rsp.GetterMethod,
                                        rule.Parameters[0]
                                    )
                                ),
                                Ast.Call(
                                    typeof(PythonOps).GetMethod("CheckInitializedAttribute"),
                                    Ast.Read(tmp),
                                    Ast.ConvertHelper(rule.Parameters[0], typeof(object)),
                                    Ast.Constant(SymbolTable.IdToString(action.Name))
                                ),
                                Ast.Read(tmp)
                            )
                        )
                    );
                    return rule;      
                }

                Expression body = Ast.Empty();

                bool isOldStyle = false;
                foreach (PythonType dt in sdo.PythonType.ResolutionOrder) {
                    if (Mro.IsOldStyle(dt)) {
                        isOldStyle = true;
                        break;
                    }
                }

                if (!isOldStyle) {
                    if (sdo.HasDictionary && (dts == null || !dts.IsSetDescriptor(context, sdo.PythonType))) {
                        body = MakeDictionaryAccess<T>(context, action, rule, tmp);
                    }

                    if (dts != null) {
                        body = MakeSlotAccess<T>(context, rule, dts, body, tmp, userType);
                    }
                } else {
                    body = MakeOldStyleAccess(context, rule, action.Name, sdo, body, tmp);
                }

                // fall back to __getattr__ if it's defined.
                PythonTypeSlot getattr;
                if (sdo.PythonType.TryResolveSlot(context, Symbols.GetBoundAttr, out getattr)) {
                    body = MakeGetAttrRule<T>(context, action, rule, body, tmp, getattr);
                }

                // raise an error if nothing else succeeds (TODO: need to reconcile error handling).
                if (action.IsNoThrow) {
                    body = Ast.Block(body, ReturnOperationFailed<T>(context, rule));
                } else {
                    body = MakeTypeError<T>(context, sdo.PythonType, action, rule, body);
                }

                rule.SetTarget(body);
            } else {
                // TODO: When ICustomMembers goes away, or as it slowly gets replaced w/ IDynamicObject,
                // we'll need to call the base GetRule instead and merge that with our normal lookup
                // rules somehow.  Today ICustomMembers always takes precedence, so it does here too.
                PythonBinderHelper.MakeTest(rule, sdo.PythonType);

                rule.SetTarget(MakeCustomMembersGetBody(context, action, PythonTypeOps.GetName(sdo.PythonType), rule));       
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
                        Ast.TypeIs(rule.Parameters[0], typeof(IPythonObject)),
                        Ast.Call(
                            typeof(PythonOps).GetMethod("TypeHasGetAttribute"),
                            Ast.ReadProperty(
                                Ast.Convert(rule.Parameters[0], typeof(IPythonObject)),
                                typeof(IPythonObject).GetProperty("PythonType")
                            )                            
                        )
                    )
                );

                Variable slotTmp = rule.GetTemporary(typeof(PythonTypeSlot), "slotTmp");
                Expression body = Ast.If(
                            Ast.Call(
                                typeof(PythonOps).GetMethod("TryResolveTypeSlot"),
                                Ast.CodeContext(),
                                Ast.Convert(
                                    Ast.ReadProperty(
                                        Ast.Convert(rule.Parameters[0], typeof(IPythonObject)),
                                        typeof(IPythonObject).GetProperty("PythonType")
                                    ),
                                    typeof(PythonType)
                                ),
                                Ast.Constant(Symbols.GetAttribute),
                                Ast.Read(slotTmp)
                            ),
                            MakeGetAttrRule<T>(context, action, rule, Ast.Empty(), tmp, Ast.Read(slotTmp))
                        );

                rule.SetTarget(body);
        }

        internal static Expression MakeCustomMembersGetBody<T>(CodeContext context, GetMemberAction action, string typeName, StandardRule<T> rule) {
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

        internal static Expression MakeCustomMembersSetBody<T>(CodeContext context, SetMemberAction action, string typeName, StandardRule<T> rule) {
            Variable tmp = rule.GetTemporary(typeof(object), "custmemres");

            return rule.MakeReturn(context.LanguageContext.Binder, 
                Ast.Comma(
                    Ast.Call(
                        Ast.Convert(rule.Parameters[0], typeof(ICustomMembers)),
                        typeof(ICustomMembers).GetMethod("SetCustomMember"),
                        Ast.CodeContext(),
                        Ast.Constant(action.Name),
                        Ast.ConvertHelper(rule.Parameters[1], typeof(object))
                    ),
                    rule.Parameters[1]
                )
            );                                           
        }

        internal static Expression MakeCustomMembersDeleteBody<T>(CodeContext context, DeleteMemberAction action, string typeName, StandardRule<T> rule) {
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

        private static Expression MakeMissingAttributeError<T>(CodeContext context, GetMemberAction action, string typeName, StandardRule<T> rule) {
            if (action.IsNoThrow) {
                return ReturnOperationFailed<T>(context, rule);
            } else {
                return MakeThrowingAttributeError<T>(context, action, typeName, rule);
            }
        }

        private static Expression MakeThrowingAttributeError<T>(CodeContext context, MemberAction action, string typeName, StandardRule<T> rule) {
            return rule.MakeError(
                Ast.Call(
                    typeof(PythonOps).GetMethod("AttributeErrorForMissingAttribute", new Type[] { typeof(string), typeof(SymbolId) }),
                    Ast.Constant(typeName, typeof(string)),
                    Ast.Constant(action.Name)
                )
            );
        }
        
        /// <summary>
        /// Checks to see if this type has __getattribute__ that overrides all other attribute lookup.
        /// 
        /// This is more complex then it needs to be.  The problem is that when we have a 
        /// mixed new-style/old-style class we have a weird __getattribute__ defined.  When
        /// we always dispatch through rules instead of PythonTypes it should be easy to remove
        /// this.
        /// </summary>
        private static bool TryGetGetAttribute(CodeContext context, IPythonObject sdo, out PythonTypeSlot dts) {
            if (sdo.PythonType.TryResolveSlot(context, Symbols.GetAttribute, out dts)) {
                BuiltinMethodDescriptor bmd = dts as BuiltinMethodDescriptor;
                if (bmd == null || bmd.DeclaringType != typeof(object) ||
                    bmd.Template.Targets.Count != 1 ||
                    bmd.Template.Targets[0].DeclaringType != typeof(ObjectOps) ||
                    bmd.Template.Targets[0].Name != "__getattribute__") {

                    PythonTypeGetAttributeSlot gas = dts as PythonTypeGetAttributeSlot;
                    if (gas != null) {
                        // inherited __getattribute__ slots won't provide their value so
                        // we need to get the one that's actually going to provide a value.
                        SymbolId symbol = Symbols.GetAttribute;
                        dts = GetNonInheritedSlot(context, sdo, gas, symbol);
                    }

                    return dts != null;
                }                
            }
            return false;
        }

        private static PythonTypeSlot GetNonInheritedSlot(CodeContext context, IPythonObject sdo, PythonTypeGetAttributeSlot gas, SymbolId symbol) {
            PythonTypeSlot dts = gas;
            if (gas.Inherited) {
                for (int i = 1; i < sdo.PythonType.ResolutionOrder.Count; i++) {
                    if (sdo.PythonType.ResolutionOrder[i] != TypeCache.Object &&
                        sdo.PythonType.ResolutionOrder[i].TryLookupSlot(context, symbol, out dts)) {
                        gas = dts as PythonTypeGetAttributeSlot;
                        if (gas == null || !gas.Inherited) break;

                        dts = null;
                    }
                }
            }
            return dts;
        }

        public static bool TryGetMixedNewStyleOldStyleSlot(CodeContext context, object instance, SymbolId name, out object value) {
            return OldInstanceTypeBuilder.TryGetMemberCustomizer(context, instance, name, out value);
        }

        /// <summary>
        /// Checks a range of the MRO to perform old-style class lookups if any old-style classes
        /// are present.  We will call this twice to produce a search before a slot and after
        /// a slot.
        /// </summary>
        private static Expression MakeOldStyleAccess<T>(CodeContext context, StandardRule<T> rule, SymbolId name, IPythonObject sdo, Expression body, Variable tmp) {
            return Ast.Block(
                body,
                Ast.If(
                    Ast.Call(
                        typeof(UserTypeOps).GetMethod("TryGetMixedNewStyleOldStyleSlot"),
                        Ast.CodeContext(),
                        Ast.ConvertHelper(rule.Parameters[0], typeof(object)),
                        Ast.Constant(name),
                        Ast.ReadDefined(tmp)
                    ),
                    rule.MakeReturn(context.LanguageContext.Binder, Ast.ReadDefined(tmp))
                )
            );
        }

        private static Expression MakeGetAttrRule<T>(CodeContext context, GetMemberAction action, StandardRule<T> rule, Expression body, Variable tmp, PythonTypeSlot getattr) {
            Expression slot = Ast.WeakConstant(getattr);
            return MakeGetAttrRule(context, action, rule, body, tmp, slot);
        }

        private static Expression MakeGetAttrRule<T>(CodeContext context, GetMemberAction action, StandardRule<T> rule, Expression body, Variable tmp, Expression getattr) {
            body = Ast.Block(
                body,
                Ast.If(
                    Ast.Call(
                        typeof(PythonOps).GetMethod("SlotTryGetBoundValue"),
                        Ast.CodeContext(),
                        Ast.ConvertHelper(getattr, typeof(PythonTypeSlot)),
                        Ast.ConvertHelper(rule.Parameters[0], typeof(object)),
                        Ast.Convert(
                            Ast.ReadProperty(
                                Ast.Convert(
                                    rule.Parameters[0],
                                    typeof(IPythonObject)),
                                typeof(IPythonObject).GetProperty("PythonType")
                            ),
                            typeof(PythonType)
                        ),
                        Ast.ReadDefined(tmp)
                    ),
                    MakeGetAttrCall<T>(context, action, rule, tmp)
                )
            );
            return body;
        }

        private static Expression MakeGetAttrCall<T>(CodeContext context, GetMemberAction action, StandardRule<T> rule, Variable tmp) {
            Expression ret = rule.MakeReturn(context.LanguageContext.Binder,
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

        private static Expression ReturnOperationFailed<T>(CodeContext context, StandardRule<T> rule) {
            return rule.MakeReturn(context.LanguageContext.Binder, Ast.ReadField(null, typeof(OperationFailed).GetField("Value")));
        }

        private static Block MakeTypeError<T>(CodeContext context, PythonType type, GetMemberAction action, StandardRule<T> rule, Expression body) {
            return Ast.Block(
                body,
                rule.MakeError(
                    Ast.Call(
                        typeof(PythonOps).GetMethod("AttributeErrorForMissingAttribute", new Type[] { typeof(string), typeof(SymbolId) }),
                        Ast.Constant(PythonTypeOps.GetName(type), typeof(string)),
                        Ast.Constant(action.Name)
                    )
                )
                
            );
        }

        private static Expression MakeSlotSet<T>(CodeContext context, StandardRule<T> rule, PythonTypeSlot dts, Type userType) {
            ReflectedProperty rp = dts as ReflectedProperty;
            if (rp != null) {
                // direct dispatch to the property...                
                MethodInfo setter = rp.Setter;
                if (!setter.IsPublic || !setter.DeclaringType.IsVisible) {
                    // find the public wrapper method corresponding to this setter
                    // TODO: the ReflectedProperty should hold on to this method, instead of hardcoding the lookup
                    setter = userType.GetMethod(NewTypeMaker.BaseMethodPrefix + setter.Name, ReflectionUtils.GetParameterTypes(setter.GetParameters()));
                    Debug.Assert(setter != null);
                }

                Expression[] args = rule.Parameters;
                if (setter.IsStatic && !(rp is ReflectedExtensionProperty)) {
                    args = new Expression[] { rule.Parameters[1] };
                }

                return
                    rule.MakeReturn(
                        context.LanguageContext.Binder,
                        Ast.Comma(
                            context.LanguageContext.Binder.MakeCallExpression(setter, args),
                            rule.Parameters[1]
                        )
                    );
            }

            ReflectedField rf = dts as ReflectedField;
            if (rf != null && rf.info.IsFamily && !rf.info.IsStatic) {
                MethodInfo setter = userType.GetMethod(NewTypeMaker.FieldSetterPrefix + rf.info.Name);
                Debug.Assert(setter != null);

                return rule.MakeReturn(
                    context.LanguageContext.Binder,
                    Ast.Comma(
                        Ast.SimpleCallHelper(rule.Parameters[0], setter, rule.Parameters[1]),
                        rule.Parameters[1]
                    )
                );
            }

            return 
                Ast.If(
                    Ast.Call(
                        typeof(PythonOps).GetMethod("SlotTrySetValue"),
                        Ast.CodeContext(),
                        Ast.ConvertHelper(Ast.WeakConstant(dts), typeof(PythonTypeSlot)),
                        Ast.ConvertHelper(rule.Parameters[0], typeof(object)),
                        Ast.Convert(
                            Ast.ReadProperty(
                                Ast.Convert(
                                    rule.Parameters[0],
                                    typeof(IPythonObject)),
                                typeof(IPythonObject).GetProperty("PythonType")
                            ),
                            typeof(PythonType)
                        ),
                        Ast.ConvertHelper(rule.Parameters[1], typeof(object))
                    ),
                    rule.MakeReturn(context.LanguageContext.Binder, rule.Parameters[1])                
                );
        }

        private static Expression MakeSlotDelete<T>(CodeContext context, StandardRule<T> rule, PythonTypeSlot dts) {
            return
                Ast.If(
                    Ast.Call(
                        typeof(PythonOps).GetMethod("SlotTryDeleteValue"),
                        Ast.CodeContext(),
                        Ast.ConvertHelper(Ast.WeakConstant(dts), typeof(PythonTypeSlot)),
                        Ast.ConvertHelper(rule.Parameters[0], typeof(object)),
                        Ast.Convert(
                            Ast.ReadProperty(
                                Ast.Convert(
                                    rule.Parameters[0],
                                    typeof(IPythonObject)),
                                typeof(IPythonObject).GetProperty("PythonType")
                            ),
                            typeof(PythonType)
                        )
                    ),
                    rule.MakeReturn(context.LanguageContext.Binder, Ast.Null())
                );
        }
        private static Block MakeSlotAccess<T>(CodeContext context, StandardRule<T> rule, PythonTypeSlot dts, Expression body, Variable tmp, Type userType) {
            ReflectedProperty rp = dts as ReflectedProperty;
            if (rp != null) {
                // direct dispatch to the property...                
                MethodInfo getter = rp.Getter;
                if (!getter.IsPublic || !getter.DeclaringType.IsVisible) {
                    // find the public wrapper method corresponding to this getter
                    // TODO: the ReflectedProperty should hold on to this method, instead of hardcoding the lookup
                    getter = userType.GetMethod(NewTypeMaker.BaseMethodPrefix + getter.Name, ReflectionUtils.GetParameterTypes(getter.GetParameters()));
                    Debug.Assert(getter != null);
                }

                Expression[] args = rule.Parameters;
                if (getter.IsStatic && !(rp is ReflectedExtensionProperty)) {
                    args = new Expression[0];
                }

                return Ast.Block(
                    body,
                    rule.MakeReturn(
                        context.LanguageContext.Binder,
                        context.LanguageContext.Binder.MakeCallExpression(getter, args)
                    )
                );
            }

            ReflectedField rf = dts as ReflectedField;
            if (rf != null && rf.info.IsFamily && !rf.info.IsStatic) {
                MethodInfo getter = userType.GetMethod(NewTypeMaker.FieldGetterPrefix + rf.info.Name);
                Debug.Assert(getter != null);

                return Ast.Block(
                    body,
                    rule.MakeReturn(
                        context.LanguageContext.Binder,
                        Ast.SimpleCallHelper(rule.Parameters[0], getter)
                    )
                );
            }

            return Ast.Block(
                body,
                Ast.If(
                    Ast.Call(
                        typeof(PythonOps).GetMethod("SlotTryGetBoundValue"),
                        Ast.CodeContext(),
                        Ast.ConvertHelper(Ast.WeakConstant(dts), typeof(PythonTypeSlot)),
                        Ast.ConvertHelper(rule.Parameters[0], typeof(object)),
                        Ast.Convert(
                            Ast.ReadProperty(
                                Ast.Convert(
                                    rule.Parameters[0],
                                    typeof(IPythonObject)),
                                typeof(IPythonObject).GetProperty("PythonType")
                            ),
                            typeof(PythonType)
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
                            Ast.Convert(rule.Parameters[0], typeof(IPythonObject)),
                            typeof(IPythonObject).GetProperty("Dict")
                        ),
                        Ast.Constant(null)
                    ),
                    Ast.Call(
                        Ast.ReadProperty(
                            Ast.Convert(rule.Parameters[0], typeof(IPythonObject)),
                            typeof(IPythonObject).GetProperty("Dict")
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
            IPythonObject sdo = args[0] as IPythonObject;
            StandardRule<T> rule = new StandardRule<T>();

            if (!(args[0] is ICustomMembers)) {
                string templateType = null;
                try {
                    // call __setattr__ if it exists
                    PythonTypeSlot dts;
                    if (sdo.PythonType.TryResolveSlot(context, Symbols.SetAttr, out dts) && !IsStandardObjectMethod(dts)) {
                        // skip the fake __setattr__ on mixed new-style/old-style types
                        PythonTypeGetAttributeSlot gas = dts as PythonTypeGetAttributeSlot;
                        if (gas != null) {
                            dts = GetNonInheritedSlot(context, sdo, gas, Symbols.SetAttr);
                        }
                        if (dts != null) {
                            MakeSetAttrTarget<T>(context, action, sdo, rule, dts);
                            templateType = "__setattr__";
                            return rule;
                        }
                    }

                    // then see if we have a set descriptor
                    sdo.PythonType.TryResolveSlot(context, action.Name, out dts);
                    ReflectedSlotProperty rsp = dts as ReflectedSlotProperty;
                    if (rsp != null) {
                        MakeSlotsSetTarget<T>(context, rule, rsp);
                        return rule;
                    }

                    if (dts != null && dts.IsSetDescriptor(context, sdo.PythonType)) {
                        rule.SetTarget(MakeSlotSet<T>(context, rule, dts, args[0].GetType()));
                        return rule;
                    }

                    // see if we can do a standard .NET binding...
                    StandardRule<T> baseRule = new SetMemberBinderHelper<T>(context, action, args).MakeNewRule();
                    if (!baseRule.IsError) {
                        baseRule.AddTest(PythonBinderHelper.MakeTypeTest(baseRule, sdo.PythonType, baseRule.Parameters[0], false));
                        return baseRule;
                    }

                    // finally if we have a dictionary set the value there.
                    if (sdo.HasDictionary) {
                        MakeDictionarySetTarget<T>(context, action, rule);
                        templateType = "dictionary";
                        return rule;
                    }

                    // otherwise it's an error
                    rule.SetTarget(MakeThrowingAttributeError(context, action, sdo.PythonType.Name, rule));
                    return rule;
                } finally {
                    // make the test for the rule depending on if we support templating or not.
                    if (templateType == null) {
                        PythonBinderHelper.MakeTest(rule, sdo.PythonType);
                    } else {
                        MakeTemplatedSetMemberTest(context, action, sdo.PythonType, rule, templateType);
                    }
                }
            } else {
                // TODO: When ICustomMembers goes away, or as it slowly gets replaced w/ IDynamicObject,
                // we'll need to call the base GetRule instead and merge that with our normal lookup
                // rules somehow.  Today ICustomMembers always takes precedence, so it does here too.
                PythonBinderHelper.MakeTest(rule, sdo.PythonType);

                rule.SetTarget(MakeCustomMembersSetBody(context, action, PythonTypeOps.GetName(sdo.PythonType), rule));
                return rule;
            }
        }

        private static StandardRule<T> MakeDeleteMemberRule<T>(CodeContext context, DeleteMemberAction action, object[] args) {
            IPythonObject sdo = args[0] as IPythonObject;
            StandardRule<T> rule = new StandardRule<T>();
            PythonBinderHelper.MakeTest(rule, sdo.PythonType);

            if (!(args[0] is ICustomMembers)) {
                if (action.Name == Symbols.Class) {
                    rule.SetTarget(
                        rule.MakeError(
                            Ast.New(
                                typeof(ArgumentTypeException).GetConstructor(new Type[] { typeof(string) }),
                                Ast.Constant("can't delete __class__ attribute")
                            )
                        )
                    );
                    return rule;
                }

                // call __delattr__ if it exists
                PythonTypeSlot dts;
                if (sdo.PythonType.TryResolveSlot(context, Symbols.DelAttr, out dts) && !IsStandardObjectMethod(dts)) {
                    MakeDeleteAttrTarget<T>(context, action, sdo, rule, dts);
                    return rule;
                }

                // then see if we have a delete descriptor
                sdo.PythonType.TryResolveSlot(context, action.Name, out dts);
                ReflectedSlotProperty rsp = dts as ReflectedSlotProperty;
                if (rsp != null) {
                    MakeSlotsDeleteTarget<T>(context, rule, rsp);
                    return rule;
                }

                // finally if we have a dictionary set the value there.
                if (sdo.HasDictionary) {
                    Expression body = MakeDictionaryDeleteTarget<T>(context, action, rule);
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
                rule.SetTarget(MakeThrowingAttributeError(context, action, sdo.PythonType.Name, rule));
                return rule;
            } else {
                // TODO: When ICustomMembers goes away, or as it slowly gets replaced w/ IDynamicObject,
                // we'll need to call the base GetRule instead and merge that with our normal lookup
                // rules somehow.  Today ICustomMembers always takes precedence, so it does here too.
                rule.SetTarget(MakeCustomMembersDeleteBody(context, action, PythonTypeOps.GetName(sdo.PythonType), rule));
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
                            Ast.SimpleCallHelper(rsp.SetterMethod, rule.Parameters[0], value),
                            tmp.Type
                        )
                    )
                )
            );
        }

        private static Expression MakeDictionaryDeleteTarget<T>(CodeContext context, DeleteMemberAction action, StandardRule<T> rule) {
            return 
                rule.MakeReturn(
                    context.LanguageContext.Binder,
                    Ast.Call(
                        typeof(UserTypeOps).GetMethod("RemoveDictionaryValue"),
                        Ast.Convert(rule.Parameters[0], typeof(IPythonObject)),
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
                        typeof(UserTypeOps).GetMethod("SetDictionaryValue"),
                        Ast.Convert(rule.Parameters[0], typeof(IPythonObject)),
                        rule.AddTemplatedConstant(typeof(SymbolId), action.Name),
                        Ast.ConvertHelper(rule.Parameters[1], typeof(object))
                    )
                )
            );
        }

        private static void MakeSetAttrTarget<T>(CodeContext context, SetMemberAction action, IPythonObject sdo, StandardRule<T> rule, PythonTypeSlot dts) {
            Variable tmp = rule.GetTemporary(typeof(object), "boundVal");
            // call __setattr__
            rule.SetTarget(
                Ast.IfThenElse(
                    Ast.Call(
                        typeof(PythonOps).GetMethod("SlotTryGetValue"),
                        Ast.CodeContext(),
                        rule.AddTemplatedWeakConstant(typeof(PythonTypeSlot), dts),
                        Ast.ConvertHelper(rule.Parameters[0], typeof(object)),
                        rule.AddTemplatedWeakConstant(typeof(PythonType), sdo.PythonType),
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
                    MakeThrowingAttributeError<T>(context, action, sdo.PythonType.Name, rule)
                )
            );
        }

        private static void MakeDeleteAttrTarget<T>(CodeContext context, DeleteMemberAction action, IPythonObject sdo, StandardRule<T> rule, PythonTypeSlot dts) {
            Variable tmp = rule.GetTemporary(typeof(object), "boundVal");
            // call __delattr__
            rule.SetTarget(
                Ast.IfThenElse(
                    Ast.Call(
                        typeof(PythonOps).GetMethod("SlotTryGetValue"),
                        Ast.CodeContext(),
                        Ast.WeakConstant(dts),
                        Ast.ConvertHelper(rule.Parameters[0], typeof(object)),
                        Ast.Convert(Ast.WeakConstant(sdo.PythonType), typeof(PythonType)),
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
                    MakeThrowingAttributeError<T>(context, action, sdo.PythonType.Name, rule)
                )
            );
        }

        private static bool IsStandardObjectMethod(PythonTypeSlot dts) {
            BuiltinMethodDescriptor bmd = dts as BuiltinMethodDescriptor;
            if (bmd == null) return false;
            return bmd.Template.Targets[0].DeclaringType == typeof(ObjectOps);
        }

        private static void MakeTemplatedSetMemberTest<T>(CodeContext context, SetMemberAction action, PythonType targetType, StandardRule<T> rule, string templateType) {            
            bool altVersion = targetType.Version == PythonType.DynamicVersion;
            string vername = altVersion ? "GetAlternateTypeVersion" : "GetTypeVersion";
            int version = altVersion ? targetType.AlternateVersion : targetType.Version;
            rule.SetTest(
                Ast.AndAlso(
                    StandardRule.MakeTypeTestExpression(targetType.UnderlyingSystemType, rule.Parameters[0]),
                    Ast.Equal(
                        Ast.Call(
                            typeof(PythonOps).GetMethod(vername),
                            Ast.ReadProperty(
                                    Ast.Convert(rule.Parameters[0], typeof(IPythonObject)),
                                    typeof(IPythonObject).GetProperty("PythonType")
                            )
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
            if (action.Operation == Operators.IsCallable) {
                return PythonBinderHelper.MakeIsCallableRule<T>(context, args[0]);
            }
            else if (action.Operation == Operators.GetItem || action.Operation == Operators.SetItem) {
                IPythonObject sdo = args[0] as IPythonObject;
                
                if (sdo.PythonType.Version != PythonType.DynamicVersion &&
                    !HasBadSlice(context, sdo.PythonType, action.Operation)) {
                    StandardRule<T> rule = new StandardRule<T>();
                    PythonTypeSlot dts;

                    SymbolId item = action.Operation == Operators.GetItem ? Symbols.GetItem : Symbols.SetItem;

                    PythonBinderHelper.MakeTest(rule, PythonTypeOps.ObjectTypes(args));

                    if (sdo.PythonType.TryResolveSlot(context, item, out dts)) {

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
                                    rule.MakeError(MakeIndexerError<T>(rule, item))
                                )
                            );
                        } else {
                            // call to .NET function, don't collapse the arguments.
                            MethodBinder mb = MethodBinder.MakeBinder(context.LanguageContext.Binder, SymbolTable.IdToString(item), bmd.Template.Targets);
                            BindingTarget target = mb.MakeBindingTarget(CallType.ImplicitInstance, CompilerHelpers.GetTypes(args));
                            if (target.Success) {
                                Expression callExpr = target.MakeExpression(rule, rule.Parameters);

                                rule.SetTarget(rule.MakeReturn(context.LanguageContext.Binder, callExpr));
                            } else {
                                // go dynamic for error case
                                return null;
                            }
                        }
                    } else {
                        rule.SetTarget(rule.MakeError(MakeIndexerError<T>(rule, item)));
                    }

                    return rule;
                }
            }

            return null;
        }

        private static bool HasBadSlice(CodeContext context, PythonType type, Operators op) {
            PythonTypeSlot dts;

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
                typeof(PythonOps).GetMethod("AttributeErrorForMissingAttribute", new Type[] { typeof(object), typeof(SymbolId) }),
                Ast.Convert(
                    Ast.ReadProperty(
                        Ast.Convert(
                            rule.Parameters[0],
                            typeof(IPythonObject)
                        ),
                        typeof(IPythonObject).GetProperty("PythonType")
                    ),
                    typeof(object)
                ),
                Ast.Constant(item)
            );
        }

        private static MethodCallExpression MakeTryGetTypeMember<T>(StandardRule<T> rule, PythonTypeSlot dts, Variable tmp) {
            return Ast.Call(
                typeof(PythonOps).GetMethod("SlotTryGetBoundValue"),
                Ast.CodeContext(),
                Ast.ConvertHelper(Ast.WeakConstant(dts), typeof(PythonTypeSlot)),
                Ast.ConvertHelper(rule.Parameters[0], typeof(object)),
                Ast.Convert(
                    Ast.ReadProperty(
                        Ast.Convert(
                            rule.Parameters[0],
                            typeof(IPythonObject)),
                        typeof(IPythonObject).GetProperty("PythonType")
                    ),
                    typeof(PythonType)
                ),
                Ast.ReadDefined(tmp)
            );
        }

        public static object SetDictionaryValue(IPythonObject self, SymbolId name, object value) {
            IAttributesCollection dict = GetDictionary(self);

            return dict[name] = value;
        }

        public static void RemoveDictionaryValue(IPythonObject self, SymbolId name) {
            IAttributesCollection dict = self.Dict;
            if (dict != null) {
                if (dict.Remove(name)) {
                    return;
                }
            }

            throw PythonOps.AttributeErrorForMissingAttribute(self.PythonType, name);
        }

        private static IAttributesCollection GetDictionary(IPythonObject self) {
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
        public static string ToStringHelper(IPythonObject o) {

            object ret;
            PythonType ut = o.PythonType;
            Debug.Assert(ut != null);

            PythonTypeSlot dts;
            if (ut.TryResolveSlot(DefaultContext.Default, Symbols.Repr, out dts) &&
                dts.TryGetValue(DefaultContext.Default, o, ut, out ret)) {

                string strRet;
                if (ret != null && Converter.TryConvertToString(PythonCalls.Call(ret), out strRet)) return strRet;
                throw PythonOps.TypeError("__repr__ returned non-string type ({0})", DynamicHelpers.GetPythonType(ret).Name);
            }

            return TypeHelpers.ReprMethod(o);
        }

        public static bool TryGetNonInheritedMethodHelper(PythonType dt, object instance, SymbolId name, out object callTarget) {
            // search MRO for other user-types in the chain that are overriding the method
            foreach(PythonType type in dt.ResolutionOrder) {
                if (type.IsSystemType) break;           // hit the .NET types, we're done

                if (LookupValue(type, instance, name, out callTarget)) {
                    return true;
                }
            }

            // check instance
            IPythonObject isdo = instance as IPythonObject;
            IAttributesCollection iac;
            if (isdo != null && (iac = isdo.Dict) != null) {
                if (iac.TryGetValue(name, out callTarget))
                    return true;
            }

            callTarget = null;
            return false;
        }

        private static bool LookupValue(PythonType dt, object instance, SymbolId name, out object value) {
            PythonTypeSlot dts;
            if (dt.TryLookupSlot(DefaultContext.Default, name, out dts) &&
                dts.TryGetValue(DefaultContext.Default, instance, dt, out value)) {
                return true;
            }
            value = null;
            return false;
        }

        public static bool TryGetNonInheritedValueHelper(PythonType dt, object instance, SymbolId name, out object callTarget) {
            PythonTypeSlot dts;
            // search MRO for other user-types in the chain that are overriding the method
           foreach(PythonType type in dt.ResolutionOrder) {
                if (type.IsSystemType) break;           // hit the .NET types, we're done

                if (type.TryLookupSlot(DefaultContext.Default, name, out dts)) {
                    callTarget = dts;
                    return true;
                }
            }
            
            // check instance
            IPythonObject isdo = instance as IPythonObject;
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
            if (DynamicHelpers.GetPythonType(self).TryGetBoundMember(DefaultContext.Default, self, Symbols.Hash, out func)) {
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

            if (DynamicHelpers.GetPythonType(self).TryInvokeBinaryOperator(DefaultContext.Default, Operators.Equals, self, other, out res))
                return res;

            return PythonOps.NotImplemented;
        }

        public static bool ValueNotEqualsHelper(object self, object other) {
            object res;
            if (DynamicHelpers.GetPythonType(self).TryInvokeBinaryOperator(DefaultContext.Default, Operators.NotEquals, self, other, out res) && 
                res != PythonOps.NotImplemented &&
                res != null &&
                res.GetType() == typeof(bool))
                return (bool)res;

            return false;
        }

        #endregion
    }
}
