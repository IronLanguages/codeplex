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
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Scripting;
using System.Scripting.Actions;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using Ast = System.Linq.Expressions.Expression;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronPython.Runtime.Operations {
    
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

            // TODO: dt gives us a PythonContext which we should use

            PythonTypeSlot dts = method as PythonTypeSlot;
            if (dts != null) {
                if (!dts.TryGetValue(DefaultContext.Default, instance, dt, out callable))
                    throw PythonOps.AttributeErrorForMissingAttribute(dt.Name, name);
            }

            if (!PythonOps.IsCallable(DefaultContext.Default, callable)) {
                throw PythonOps.TypeError("Expected callable value for {0}, but found {1}", name.ToString(),
                    PythonTypeOps.GetName(method));
            }

            PythonCalls.Call(callable, eventValue);
        }

        public static RuleBuilder<T> GetRuleHelper<T>(OldDynamicAction action, CodeContext context, object[] args) where T : class {
            switch (action.Kind) {
                case DynamicActionKind.GetMember: return MakeGetMemberRule<T>(context, (OldGetMemberAction)action, args);
                case DynamicActionKind.SetMember: return MakeSetMemberRule<T>(context, (OldSetMemberAction)action, args);
                case DynamicActionKind.DeleteMember: return MakeDeleteMemberRule<T>(context, (OldDeleteMemberAction)action, args);
                case DynamicActionKind.DoOperation: return MakeOperationRule<T>(context, (OldDoOperationAction)action, args);
                case DynamicActionKind.Call: return MakeCallRule<T>(context, (OldCallAction)action, args);
                case DynamicActionKind.ConvertTo: return MakeConvertRule<T>(context, (OldConvertToAction)action, args);
                default: return null;
            }
        }

        public static MetaObject/*!*/ GetMetaObjectHelper(IPythonObject self, Expression/*!*/ parameter, MetaObject baseMetaObject) {
            return new Binding.MetaUserObject(parameter, Restrictions.Empty, baseMetaObject, self);
        }

        #region Conversion support

        private static RuleBuilder<T> MakeConvertRule<T>(CodeContext context, OldConvertToAction convertToAction, object[] args) where T : class {
            ContractUtils.Requires(args.Length == 1, "args", "args must contain 1 argument for conversion");
            ContractUtils.Requires(args[0] is IPythonObject, "args[0]", "must be IPythonObject");

            Type toType = convertToAction.ToType;
            RuleBuilder<T> res = null;
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

        private static RuleBuilder<T> MakeConvertToIEnumerable<T>(CodeContext context, object[] args) where T : class {
            PythonType pt = ((IPythonObject)args[0]).PythonType;
            PythonTypeSlot pts;
            RuleBuilder<T> rule = null;
            if (pt.TryResolveSlot(context, Symbols.Iterator, out pts)) {
                rule = MakeIterRule<T>(context, "CreatePythonEnumerable");
            } else if (pt.TryResolveSlot(context, Symbols.GetItem, out pts)) {
                rule = MakeIterRule<T>(context, "CreateItemEnumerable");
            }
            return rule;
        }

        private static RuleBuilder<T> MakeConvertToIEnumerator<T>(CodeContext context, object[] args) where T : class {
            PythonType pt = ((IPythonObject)args[0]).PythonType;
            PythonTypeSlot pts;
            RuleBuilder<T> rule = null;
            if (pt.TryResolveSlot(context, Symbols.Iterator, out pts)) {
                rule = MakeIterRule<T>(context, "CreatePythonEnumerator");
            } else if (pt.TryResolveSlot(context, Symbols.GetItem, out pts)) {
                rule = MakeIterRule<T>(context, "CreateItemEnumerator");
            }
            return rule;
        }

        private static RuleBuilder<T> MakeIterRule<T>(CodeContext context, string methodName) where T : class {
            RuleBuilder<T> res = new RuleBuilder<T>();
            res.Target = res.MakeReturn(
                    context.LanguageContext.Binder,
                    Ast.Call(
                        typeof(PythonOps).GetMethod(methodName),
                        res.Parameters[0]
                    )
                );
            return res;
        }

        private static RuleBuilder<T> MakeConvertLengthToBoolRule<T>(CodeContext context, OldConvertToAction convertToAction, object self) where T : class {
            PythonType pt = ((IPythonObject)self).PythonType;
            PythonTypeSlot pts;

            if (pt.TryResolveSlot(context, Symbols.Length, out pts)) {
                RuleBuilder<T> rule = new RuleBuilder<T>();
                VariableExpression tmp = rule.GetTemporary(typeof(object), "func");

                rule.Target =
                    Ast.Block(
                        AstUtils.If(
                            PythonBinderHelper.MakeTryGetTypeMember<T>(rule, pts, tmp),
                            rule.MakeReturn(
                                context.LanguageContext.Binder,
                                PythonBinderHelper.GetConvertByLengthBody(rule, context, tmp)
                            )
                        ),
                        PythonBinderHelper.GetConversionFailedReturnValue<T>(context, convertToAction, rule)
                    );

                return rule;
            }
            return null;
        }

        private static RuleBuilder<T> MakeConvertRuleForCall<T>(CodeContext context, OldConvertToAction convertToAction, object self, Type toType, SymbolId symbolId, string returner) where T : class {
            PythonType pt = ((IPythonObject)self).PythonType;
            PythonTypeSlot pts;

            if (pt.TryResolveSlot(context, symbolId, out pts) && !IsBuiltinConversion(context, pts, symbolId, pt)) {
                RuleBuilder<T> rule = new RuleBuilder<T>();
                VariableExpression tmp = rule.GetTemporary(typeof(object), "func");

                Expression callExpr = Ast.Call(
                    PythonOps.GetConversionHelper(returner, convertToAction.ResultKind),
                    AstUtils.Call(
                        PythonContext.GetContext(context).Binder,
                        typeof(object),
                        rule.Context,
                        tmp
                    )
                );

                if (toType == rule.ReturnType && typeof(Extensible<>).MakeGenericType(toType).IsAssignableFrom(self.GetType())) {
                    // if we're doing a conversion to the underlying type and we're an 
                    // Extensible<T> of that type:

                    // if an extensible type returns it's self in a conversion, then we need 
                    // to actually return the underlying value.  If an extensible just keeps 
                    // returning more instances  of it's self a stack overflow occurs - both 
                    // behaviors match CPython.
                    callExpr = AddExtensibleSelfCheck<T>(context, self, toType, rule, tmp, callExpr);
                }

                rule.Target =
                    Ast.Block(
                        AstUtils.If(
                            PythonBinderHelper.MakeTryGetTypeMember<T>(rule, pts, tmp),
                            rule.MakeReturn(
                                context.LanguageContext.Binder,
                                callExpr
                            )
                        ),
                        PythonBinderHelper.GetConversionFailedReturnValue<T>(context, convertToAction, rule)
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
            if (ptBase.TryResolveSlot(context, name, out baseSlot) && pts == baseSlot) {
                return true;
            }

            return false;
        }

        private static Expression AddExtensibleSelfCheck<T>(CodeContext context, object self, Type toType, RuleBuilder<T> rule, VariableExpression tmp, Expression callExpr) where T : class {
            callExpr = Ast.Comma(
                Ast.Assign(tmp, callExpr),
                Ast.Condition(
                    Ast.Equal(tmp, rule.Parameters[0]),
                    Ast.Property(
                        Ast.Convert(rule.Parameters[0], self.GetType()),
                        self.GetType().GetProperty("Value")
                    ),
                    AstUtils.ConvertTo(
                        PythonContext.GetContext(context).Binder,
                        toType,
                        ConversionResultKind.ExplicitCast,
                        rule.Context,
                        tmp
                    )
                )
            );
            return callExpr;
        }

        #endregion

        private static RuleBuilder<T> MakeCallRule<T>(CodeContext context, OldCallAction callAction, object[] args) where T : class {
            RuleBuilder<T> rule = new RuleBuilder<T>();

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
                VariableExpression tmp = rule.GetTemporary(typeof(object), "callSlot");
                Expression[] callArgs = ArrayUtils.Insert(rule.Context, rule.Parameters);
                callArgs[1] = tmp;

                body = Ast.Block(
                    AstUtils.If(
                        Ast.Call(
                            typeof(PythonOps).GetMethod("SlotTryGetValue"),
                            rule.Context,
                            Ast.Convert(Utils.WeakConstant(callSlot), typeof(PythonTypeSlot)),
                            Ast.ConvertHelper(rule.Parameters[0], typeof(object)),
                            Ast.Convert(
                                Ast.Property(
                                    Ast.Convert(rule.Parameters[0], typeof(IPythonObject)),
                                    TypeInfo._IPythonObject.PythonType
                                ),
                                typeof(PythonType)
                            ),
                            tmp
                        ),
                        rule.MakeReturn(context.LanguageContext.Binder,
                            AstUtils.Call(
                                callAction,
                                typeof(object),
                                callArgs
                            )
                        )
                    ),
                    body
                );
            }

            rule.Target = body;
            return rule;
        }

        private static RuleBuilder<T> MakeGetMemberRule<T>(CodeContext context, OldGetMemberAction action, object[] args) where T : class {
            PythonTypeSlot dts;
            IPythonObject sdo = (IPythonObject)args[0];

            if (TryGetGetAttribute(context, sdo.PythonType, out dts)) {
                return MakeGetAttributeRule<T>(context, sdo, action, dts);
            }

            RuleBuilder<T> rule = new RuleBuilder<T>();
            PythonBinderHelper.MakeTest(rule, sdo.PythonType);

            VariableExpression tmp = rule.GetTemporary(typeof(object), "lookupRes");

            // fast path for accessing public properties from a derived type.
            string templateType = null;
            SymbolId memberName = SymbolId.Empty;
            Type userType = args[0].GetType();

            PropertyInfo pi = userType.GetProperty(SymbolTable.IdToString(action.Name));
            if (pi != null) {
                MethodInfo getter = pi.GetGetMethod();
                if (getter != null && getter.IsPublic) {
                    templateType = "property";
                    memberName = action.Name;
                    rule.Target =
                        rule.MakeReturn(
                            context.LanguageContext.Binder,
                            Ast.Call(
                                Ast.ConvertHelper(rule.Parameters[0], getter.DeclaringType),
                                getter
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

            sdo.PythonType.TryResolveSlot(context, action.Name, out dts);

            Expression body = Ast.Empty();
            PythonTypeSlot getattr;

            bool isOldStyle = false;
            foreach (PythonType dt in sdo.PythonType.ResolutionOrder) {
                if (dt.IsOldClass) {
                    isOldStyle = true;
                    break;
                }
            }

            bool final = false;
            if (!isOldStyle || dts is ReflectedSlotProperty) {
                if (sdo.HasDictionary && (dts == null || !dts.IsSetDescriptor(context, sdo.PythonType))) {
                    body = MakeDictionaryAccess<T>(context, action, rule, tmp);
                    templateType = "dictionary";
                }

                if (dts != null) {
                    body = MakeSlotAccess<T>(context, rule, dts, body, tmp, userType, ref templateType, ref memberName, ref final);
                    //templateType = null; // disallow templating, we don't optimize this yet...
                }
            } else {
                body = MakeOldStyleAccess(context, rule, action.Name, sdo, body, tmp);
            }

            // fall back to __getattr__ if it's defined.
            if (sdo.PythonType.TryResolveSlot(context, Symbols.GetBoundAttr, out getattr)) {
                body = MakeGetAttrRule<T>(context, action, rule, body, tmp, getattr);
                if (templateType != null) {
                    templateType += "__getattr__";
                }

                if (action.IsNoThrow) {
                    body = AstUtils.Try(body).Catch(typeof(MissingMemberException), ReturnOperationFailed<T>(context, rule));
                }
            }

            // raise an error if nothing else succeeds (TODO: need to reconcile error handling).
            if (action.IsNoThrow) {
                body = Ast.Block(body, ReturnOperationFailed<T>(context, rule));
                if (templateType != null) {
                    templateType += "nothrow";
                }
            } else {
                body = MakeTypeError<T>(context, sdo.PythonType, action, rule, body);
            }

            rule.Target = body;

            return rule;
        }


        /// <summary>
        /// Makes a rule which calls a user-defined __getattribute__ function and falls back to __getattr__ if that
        /// raises an AttributeError.
        /// 
        /// slot is the __getattribute__ method to be called.
        /// </summary>
        private static RuleBuilder<T> MakeGetAttributeRule<T>(CodeContext context, IPythonObject obj, OldGetMemberAction action, PythonTypeSlot slot) where T : class {
            RuleBuilder<T> rule = new RuleBuilder<T>();

            rule.Test = PythonBinderHelper.MakeTypeTest(rule, obj.PythonType, rule.Parameters[0]);            

            // if the type implements IOldDynamicObject and we picked up it's __getattribute__ then we want to just dispatch to the base rule logic.
            // an example of this is:
            //
            // class mc(type):
            //     def __getattr__(self, name):
            //          return 42
            //
            // class nc_ga(object):
            //     __metaclass__ = mc
            //
            // a = nc_ga.x # here we want to dispatch to the type's rule, not call __getattribute__ directly.


            Type finalType = PythonTypeOps.GetFinalSystemType(obj.PythonType.UnderlyingSystemType);
            if (typeof(IOldDynamicObject).IsAssignableFrom(finalType)) {
                PythonTypeSlot baseSlot;
                if (TryGetGetAttribute(context, DynamicHelpers.GetPythonTypeFromType(finalType), out baseSlot) && baseSlot == slot) {
                    return null;
                }
            }

            VariableExpression slotTmp = rule.GetTemporary(typeof(object), "slotTmp");

            // generate the code to call __getattribute__
            Expression getAttribute = MakeGetAttrRule<T>(context, action, rule, null, slotTmp, slot);
            string templateName = "__getattribute__";

            PythonTypeSlot getattr;
            if (obj.PythonType.TryResolveSlot(context, Symbols.GetBoundAttr, out getattr)) {
                // if the type also defines __getattr__ we need to fallback to that when __getattribute__ fails.                
                templateName += "and __getattr__";
                getAttribute =
                    AstUtils.Try(
                        getAttribute
                    ).Catch(
                        typeof(MissingMemberException),
                        MakeGetAttrRule<T>(context, action, rule, null, slotTmp, getattr)
                    );
            }

            if (action.IsNoThrow) {
                templateName += " nothrow";
                getAttribute = AstUtils.Try(
                    getAttribute
                ).Catch(
                    typeof(MissingMemberException),
                    ReturnOperationFailed<T>(context, rule)
                );
            }

            rule.Target = getAttribute;

            return rule;
        }

        private static Expression MakeMissingAttributeError<T>(CodeContext context, OldGetMemberAction action, string typeName, RuleBuilder<T> rule) where T : class {
            if (action.IsNoThrow) {
                return ReturnOperationFailed<T>(context, rule);
            } else {
                return MakeThrowingAttributeError<T>(context, action, typeName, rule);
            }
        }

        private static Expression MakeThrowingAttributeError<T>(CodeContext context, OldMemberAction action, string typeName, RuleBuilder<T> rule) where T : class {
            return rule.MakeError(
                Ast.Call(
                    typeof(PythonOps).GetMethod("AttributeErrorForMissingAttribute", new Type[] { typeof(string), typeof(SymbolId) }),
                    Ast.Constant(typeName),
                    AstUtils.Constant(action.Name)
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
        private static bool TryGetGetAttribute(CodeContext context, PythonType type, out PythonTypeSlot dts) {
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

        public static bool TryGetMixedNewStyleOldStyleSlot(CodeContext context, object instance, SymbolId name, out object value) {
            IPythonObject sdo = instance as IPythonObject;
            if (sdo != null) {
                IAttributesCollection iac = sdo.Dict;
                if (iac != null && iac.TryGetValue(name, out value)) {
                    return true;
                }
            }

            PythonType dt = DynamicHelpers.GetPythonType(instance);

            foreach (PythonType type in dt.ResolutionOrder) {
                PythonTypeSlot dts;
                if (type != TypeCache.Object && type.OldClass != null) {
                    // we're an old class, check the old-class way
                    OldClass oc = type.OldClass;

                    if (oc.TryGetBoundCustomMember(context, name, out value)) {
                        value = oc.GetOldStyleDescriptor(context, value, instance, oc);
                        return true;
                    }
                } else if (type.TryLookupSlot(context, name, out dts)) {
                    // we're a dynamic type, check the dynamic type way
                    return dts.TryGetValue(context, instance, dt, out value);
                }
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Checks a range of the MRO to perform old-style class lookups if any old-style classes
        /// are present.  We will call this twice to produce a search before a slot and after
        /// a slot.
        /// </summary>
        private static Expression MakeOldStyleAccess<T>(CodeContext context, RuleBuilder<T> rule, SymbolId name, IPythonObject sdo, Expression body, VariableExpression tmp) where T : class {
            return Ast.Block(
                body,
                AstUtils.If(
                    Ast.Call(
                        typeof(UserTypeOps).GetMethod("TryGetMixedNewStyleOldStyleSlot"),
                        rule.Context,
                        Ast.ConvertHelper(rule.Parameters[0], typeof(object)),
                        AstUtils.Constant(name),
                        tmp
                    ),
                    rule.MakeReturn(context.LanguageContext.Binder, tmp)
                )
            );
        }

        private static Expression MakeGetAttrRule<T>(CodeContext context, OldGetMemberAction action, RuleBuilder<T> rule, Expression body, VariableExpression tmp, PythonTypeSlot getattr) where T : class {
            Expression slot = Ast.ConvertHelper(Utils.WeakConstant(getattr), typeof(PythonTypeSlot));
            return MakeGetAttrRule(context, action, rule, body, tmp, slot);
        }

        private static Expression MakeGetAttrRule<T>(CodeContext context, OldGetMemberAction action, RuleBuilder<T> rule, Expression body, VariableExpression tmp, Expression getattr) where T : class {
            Expression getAttr = AstUtils.If(
                Ast.Call(
                    TypeInfo._PythonOps.SlotTryGetBoundValue,
                    rule.Context,
                    Ast.ConvertHelper(getattr, typeof(PythonTypeSlot)),
                    Ast.ConvertHelper(rule.Parameters[0], typeof(object)),
                    Ast.Convert(
                        Ast.Property(
                            Ast.Convert(
                                rule.Parameters[0],
                                typeof(IPythonObject)),
                            TypeInfo._IPythonObject.PythonType
                        ),
                        typeof(PythonType)
                    ),
                    tmp
                ),
                MakeGetAttrCall<T>(context, action, rule, tmp)
            );

            if(body != null) {
                getAttr = Ast.Block(body, getAttr);
            };

            return getAttr;
        }

        private static Expression MakeGetAttrCall<T>(CodeContext context, OldGetMemberAction action, RuleBuilder<T> rule, VariableExpression tmp) where T : class {
            Expression ret = rule.MakeReturn(context.LanguageContext.Binder,
                AstUtils.Call(
                    PythonContext.GetContext(context).Binder,
                    typeof(object),
                    rule.Context,
                    tmp,
                    Ast.Constant(SymbolTable.IdToString(action.Name))
                )
            );

            return ret;
        }

        private static Expression ReturnOperationFailed<T>(CodeContext context, RuleBuilder<T> rule) where T : class {
            return rule.MakeReturn(context.LanguageContext.Binder, Ast.Field(null, TypeInfo._OperationFailed.Value));
        }

        private static Block MakeTypeError<T>(CodeContext context, PythonType type, OldGetMemberAction action, RuleBuilder<T> rule, Expression body) where T : class {
            return Ast.Block(
                body,
                rule.MakeError(
                    Ast.Call(
                        typeof(PythonOps).GetMethod("AttributeErrorForMissingAttribute", new Type[] { typeof(string), typeof(SymbolId) }),
                        Ast.Constant(type.Name),
                        AstUtils.Constant(action.Name)
                    )
                )

            );
        }

        private static Expression MakeSlotSet<T>(CodeContext context, RuleBuilder<T> rule, PythonTypeSlot dts, Type userType, Type setType) where T : class {
            ReflectedGetterSetter rp = dts as ReflectedGetterSetter;
            if (rp != null) {
                // direct dispatch to the property...                
                MethodBinder mb = MethodBinder.MakeBinder(context.LanguageContext.Binder, rp.__name__, rp.Setter);
                BindingTarget bt = mb.MakeBindingTarget(CallTypes.ImplicitInstance, new Type[] { userType, setType });
                Expression call = bt.MakeExpression(rule, rule.Parameters);

                return
                    rule.MakeReturn(
                        context.LanguageContext.Binder,
                        Ast.Comma(
                            call,
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
                AstUtils.If(
                    Ast.Call(
                        typeof(PythonOps).GetMethod("SlotTrySetValue"),
                        rule.Context,
                        Ast.ConvertHelper(Utils.WeakConstant(dts), typeof(PythonTypeSlot)),
                        Ast.ConvertHelper(rule.Parameters[0], typeof(object)),
                        Ast.Convert(
                            Ast.Property(
                                Ast.Convert(
                                    rule.Parameters[0],
                                    typeof(IPythonObject)),
                                TypeInfo._IPythonObject.PythonType
                            ),
                            typeof(PythonType)
                        ),
                        Ast.ConvertHelper(rule.Parameters[1], typeof(object))
                    ),
                    rule.MakeReturn(context.LanguageContext.Binder, rule.Parameters[1])
                );
        }

        private static Expression MakeSlotDelete<T>(CodeContext context, RuleBuilder<T> rule, PythonTypeSlot dts) where T : class {
            return
                AstUtils.If(
                    Ast.Call(
                        typeof(PythonOps).GetMethod("SlotTryDeleteValue"),
                        rule.Context,
                        Ast.ConvertHelper(Utils.WeakConstant(dts), typeof(PythonTypeSlot)),
                        Ast.ConvertHelper(rule.Parameters[0], typeof(object)),
                        Ast.Convert(
                            Ast.Property(
                                Ast.Convert(
                                    rule.Parameters[0],
                                    typeof(IPythonObject)),
                                TypeInfo._IPythonObject.PythonType
                            ),
                            typeof(PythonType)
                        )
                    ),
                    rule.MakeReturn(context.LanguageContext.Binder, Ast.Null())
                );
        }

        private static Expression MakeSlotAccess<T>(CodeContext context, RuleBuilder<T> rule, PythonTypeSlot dts, Expression body, VariableExpression tmp, Type userType, ref string templateType, ref SymbolId memberName, ref bool final) where T : class {
            ReflectedSlotProperty rsp = dts as ReflectedSlotProperty;
            if (rsp != null) {
                // we need to fall back to __getattr__ if the value is not defined, so call it and check the result.
                templateType = null;
                return AstUtils.If(
                    Ast.NotEqual(
                        Ast.Assign(
                            tmp,
                            Ast.SimpleCallHelper(
                                rsp.GetterMethod,
                                rule.Parameters[0]
                            )
                        ),
                        Ast.Field(null, typeof(Uninitialized).GetField("Instance"))
                    ),
                    rule.MakeReturn(
                        context.LanguageContext.Binder,
                        tmp
                    )
                );

            }

            ReflectedGetterSetter rp = dts as ReflectedGetterSetter;
            if (rp != null && rp.Getter.Length > 0) {
                templateType += " getter setter";
                memberName = SymbolTable.StringToId(rp.__name__);

                // direct dispatch to the property...                
                MethodBinder mb = MethodBinder.MakeBinder(context.LanguageContext.Binder, rp.__name__, rp.Getter);
                Expression call;
                if (rp.Getter[0].IsStatic && !rp.Getter[0].IsDefined(typeof(PropertyMethodAttribute), false)) {
                    BindingTarget bt = mb.MakeBindingTarget(CallTypes.ImplicitInstance, new Type[0]);
                    call = bt.MakeExpression(rule, new Expression[0]);
                } else {
                    BindingTarget bt = mb.MakeBindingTarget(CallTypes.ImplicitInstance, new Type[] { userType });
                    call = bt.MakeExpression(rule, rule.Parameters);
                }

                final = true;
                return Ast.Block(
                    body,
                    rule.MakeReturn(
                        context.LanguageContext.Binder,
                        call
                    )
                );
            }

            ReflectedField rf = dts as ReflectedField;
            if (rf != null && rf.info.IsFamily && !rf.info.IsStatic) {
                templateType += " field";
                memberName = SymbolTable.StringToId(rf.info.Name);

                MethodInfo getter = userType.GetMethod(NewTypeMaker.FieldGetterPrefix + rf.info.Name);
                Debug.Assert(getter != null);
                final = true;

                return Ast.Block(
                    body,
                    rule.MakeReturn(
                        context.LanguageContext.Binder,
                        Ast.SimpleCallHelper(rule.Parameters[0], getter)
                    )
                );
            }

            PythonTypeUserDescriptorSlot slot = dts as PythonTypeUserDescriptorSlot;
            if (slot != null && !(slot.Value is PythonTypeSlot)) {
                PythonType slottype = DynamicHelpers.GetPythonType(slot.Value);
                if (slottype.IsSystemType) {
                    // this is a user slot that's known not to be a descriptor
                    // so we can just burn the value in.  For it to change the
                    // slot will need to be replaced reving the type version.
                    templateType += " user value slot";
                    final = true;

                    return Ast.Block(
                        body,
                        rule.MakeReturn(
                            context.LanguageContext.Binder,
                            Ast.ConvertHelper(Utils.WeakConstant(slot.Value), typeof(object))
                        )
                    );
                }
            }

            templateType = null;
            return Ast.Block(
                body,
                AstUtils.If(
                    Ast.Call(
                        TypeInfo._PythonOps.SlotTryGetBoundValue,
                        rule.Context,
                        Ast.Convert(Utils.WeakConstant(dts), typeof(PythonTypeSlot)),
                        Ast.ConvertHelper(rule.Parameters[0], typeof(object)),
                        Ast.Property(
                            Ast.Convert(
                                rule.Parameters[0],
                                typeof(IPythonObject)),
                            TypeInfo._IPythonObject.PythonType
                        ),
                        tmp
                    ),
                    rule.MakeReturn(context.LanguageContext.Binder, tmp)
                )
            );
        }

        private static IfStatementBuilder MakeDictionaryAccess<T>(CodeContext context, OldGetMemberAction action, RuleBuilder<T> rule, VariableExpression tmp) where T : class {
            return AstUtils.If(
                Ast.AndAlso(
                    Ast.NotEqual(
                        Ast.Property(
                            Ast.Convert(rule.Parameters[0], typeof(IPythonObject)),
                            TypeInfo._IPythonObject.Dict
                        ),
                        Ast.Constant(null)
                    ),
                    Ast.Call(
                        Ast.Property(
                            Ast.Convert(rule.Parameters[0], typeof(IPythonObject)),
                            TypeInfo._IPythonObject.Dict
                        ),
                        TypeInfo._IAttributesCollection.TryGetvalue,
                        AstUtils.Constant(action.Name),
                        tmp
                    )
                ),
                rule.MakeReturn(context.LanguageContext.Binder, tmp)
            );
        }

        private static RuleBuilder<T> MakeSetMemberRule<T>(CodeContext context, OldSetMemberAction action, object[] args) where T : class {
            IPythonObject sdo = args[0] as IPythonObject;
            RuleBuilder<T> rule = new RuleBuilder<T>();
            PythonBinderHelper.MakeTest(rule, sdo.PythonType);

            // call __setattr__ if it exists
            PythonTypeSlot dts;
            if (sdo.PythonType.TryResolveSlot(context, Symbols.SetAttr, out dts) && !IsStandardObjectMethod(dts)) {
                // skip the fake __setattr__ on mixed new-style/old-style types
                if (dts != null) {
                    MakeSetAttrTarget<T>(context, action, sdo, rule, dts);
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
                rule.Target = MakeSlotSet<T>(context, rule, dts, args[0].GetType(), CompilerHelpers.GetType(args[1]));
                return rule;
            }

            // see if we can do a standard .NET binding...
            RuleBuilder<T> baseRule = new SetMemberBinderHelper<T>(context, action, args).MakeNewRule();
            if (!baseRule.IsError) {
                baseRule.AddTest(PythonBinderHelper.MakeTypeTest(baseRule, sdo.PythonType, baseRule.Parameters[0]));
                return baseRule;
            }

            // finally if we have a dictionary set the value there.
            if (sdo.HasDictionary) {
                MakeDictionarySetTarget<T>(context, action, rule);
                return rule;
            }

            // otherwise it's an error
            rule.Target = MakeThrowingAttributeError(context, action, sdo.PythonType.Name, rule);
            return rule;
            // make the test for the rule depending on if we support templating or not.
        }

        private static RuleBuilder<T> MakeDeleteMemberRule<T>(CodeContext context, OldDeleteMemberAction action, object[] args) where T : class {
            IPythonObject sdo = args[0] as IPythonObject;
            RuleBuilder<T> rule = new RuleBuilder<T>();
            PythonBinderHelper.MakeTest(rule, sdo.PythonType);

            if (action.Name == Symbols.Class) {
                rule.Target =
                    rule.MakeError(
                        Ast.New(
                            typeof(ArgumentTypeException).GetConstructor(new Type[] { typeof(string) }),
                            Ast.Constant("can't delete __class__ attribute")
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

                rule.Target = body;
                return rule;
            }

            if (dts != null) {
                rule.Target = MakeSlotDelete<T>(context, rule, dts);
                return rule;
            }

            // otherwise it's an error
            rule.Target = MakeThrowingAttributeError(context, action, sdo.PythonType.Name, rule);
            return rule;
        }

        private static void MakeSlotsSetTarget<T>(CodeContext context, RuleBuilder<T> rule, ReflectedSlotProperty rsp) where T : class {
            MakeSlotsSetTarget(context, rule, rsp, rule.Parameters[1]);
        }

        private static void MakeSlotsDeleteTarget<T>(CodeContext context, RuleBuilder<T> rule, ReflectedSlotProperty rsp) where T : class {
            MakeSlotsSetTarget(context, rule, rsp, Ast.Field(null, typeof(Uninitialized).GetField("Instance")));
        }

        private static void MakeSlotsSetTarget<T>(CodeContext context, RuleBuilder<T> rule, ReflectedSlotProperty rsp, Expression value) where T : class {
            VariableExpression tmp = rule.GetTemporary(typeof(object), "res");

            // type has __slots__ defined for this member, call the setter directly
            rule.Target = rule.MakeReturn(
                context.LanguageContext.Binder,
                Ast.Assign(tmp,
                    Ast.Convert(
                        Ast.SimpleCallHelper(rsp.SetterMethod, rule.Parameters[0], value),
                        tmp.Type
                    )
                )
            );
        }

        private static Expression MakeDictionaryDeleteTarget<T>(CodeContext context, OldDeleteMemberAction action, RuleBuilder<T> rule) where T : class {
            return
                rule.MakeReturn(
                    context.LanguageContext.Binder,
                    Ast.Call(
                        typeof(UserTypeOps).GetMethod("RemoveDictionaryValue"),
                        Ast.Convert(rule.Parameters[0], typeof(IPythonObject)),
                        AstUtils.Constant(action.Name)
                    )
                );
        }

        private static void MakeDictionarySetTarget<T>(CodeContext context, OldSetMemberAction action, RuleBuilder<T> rule) where T : class {
            // return UserTypeOps.SetDictionaryValue(rule.Parameters[0], name, value);
            rule.Target = rule.MakeReturn(
                context.LanguageContext.Binder,
                Ast.Call(
                    typeof(UserTypeOps).GetMethod("SetDictionaryValue"),
                    Ast.Convert(rule.Parameters[0], typeof(IPythonObject)),
                    AstUtils.Constant(action.Name),
                    Ast.ConvertHelper(rule.Parameters[1], typeof(object))
                )
            );
        }

        private static void MakeSetAttrTarget<T>(CodeContext context, OldSetMemberAction action, IPythonObject sdo, RuleBuilder<T> rule, PythonTypeSlot dts) where T : class {
            VariableExpression tmp = rule.GetTemporary(typeof(object), "boundVal");
            // call __setattr__
            rule.Target =
                AstUtils.IfThenElse(
                    Ast.Call(
                        typeof(PythonOps).GetMethod("SlotTryGetValue"),
                        rule.Context,
                        Ast.ConvertHelper(Utils.WeakConstant(dts), typeof(PythonTypeSlot)),
                        Ast.ConvertHelper(rule.Parameters[0], typeof(object)),
                        Ast.ConvertHelper(Utils.WeakConstant(sdo.PythonType), typeof(PythonType)),
                        tmp
                    ),
                    rule.MakeReturn(
                        context.LanguageContext.Binder,
                        AstUtils.Call(
                            PythonContext.GetContext(context).Binder,
                            typeof(object),
                            rule.Context,
                            tmp,
                            Ast.Constant(SymbolTable.IdToString(action.Name)),
                            rule.Parameters[1]
                        )
                    ),
                    MakeThrowingAttributeError<T>(context, action, sdo.PythonType.Name, rule)
                );
        }

        private static void MakeDeleteAttrTarget<T>(CodeContext context, OldDeleteMemberAction action, IPythonObject sdo, RuleBuilder<T> rule, PythonTypeSlot dts) where T : class {
            VariableExpression tmp = rule.GetTemporary(typeof(object), "boundVal");
            // call __delattr__
            rule.Target =
                AstUtils.IfThenElse(
                    Ast.Call(
                        TypeInfo._PythonOps.SlotTryGetBoundValue,
                        rule.Context,
                        Ast.ConvertHelper(Utils.WeakConstant(dts), typeof(PythonTypeSlot)),
                        Ast.ConvertHelper(rule.Parameters[0], typeof(object)),
                        Ast.ConvertHelper(Utils.WeakConstant(sdo.PythonType), typeof(PythonType)),
                        tmp
                    ),
                    rule.MakeReturn(
                        context.LanguageContext.Binder,
                        AstUtils.Call(
                            PythonContext.GetContext(context).Binder,
                            typeof(object),
                            rule.Context,
                            tmp,
                            Ast.Constant(SymbolTable.IdToString(action.Name))
                        )
                    ),
                    MakeThrowingAttributeError<T>(context, action, sdo.PythonType.Name, rule)
                );
        }

        private static bool IsStandardObjectMethod(PythonTypeSlot dts) {
            BuiltinMethodDescriptor bmd = dts as BuiltinMethodDescriptor;
            if (bmd == null) return false;
            return bmd.Template.Targets[0].DeclaringType == typeof(ObjectOps);
        }

        private static RuleBuilder<T> MakeOperationRule<T>(CodeContext context, OldDoOperationAction action, object[] args) where T : class {
            switch (action.Operation) {
                case Operators.GetItem:
                case Operators.SetItem:
                case Operators.DeleteItem:
                case Operators.GetSlice:
                case Operators.SetSlice:
                case Operators.DeleteSlice:
                    // ask the default python DoOperationBinderHelper to produce the rule for the purpose
                    // of interop - it knows how to handle all things indexing.
                    return new PythonDoOperationBinderHelper<T>(context, action).MakeRule(args);
                case Operators.IsCallable:
                    return PythonBinderHelper.MakeIsCallableRule<T>(context, args[0]);
            }
            return null;
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

        internal static IAttributesCollection GetDictionary(IPythonObject self) {
            IAttributesCollection dict = self.Dict;
            if (dict == null) {
                dict = self.SetDict(PythonDictionary.MakeSymbolDictionary());
            }
            return dict;
        }

        /// <summary>
        /// Object.ToString() displays the CLI type name.  But we want to display the class name (e.g.
        /// '&lt;foo object at 0x000000000000002C&gt;' unless we've overridden __repr__ but not __str__ in 
        /// which case we'll display the result of __repr__.
        /// </summary>
        public static string ToStringHelper(IPythonObject o) {
            return ObjectOps.__str__(o);
        }

        public static bool TryGetNonInheritedMethodHelper(PythonType dt, object instance, SymbolId name, out object callTarget) {
            // search MRO for other user-types in the chain that are overriding the method
            foreach (PythonType type in dt.ResolutionOrder) {
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
            foreach (PythonType type in dt.ResolutionOrder) {
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

        public static object GetAttribute(CodeContext/*!*/ context, object self, string name, PythonTypeSlot getAttributeSlot, PythonTypeSlot getAttrSlot, SiteLocalStorage<CallSite<DynamicSiteTarget<CodeContext, object, string, object>>>/*!*/ callSite) {
            object value;
            if (callSite.Data == null) {
                callSite.Data = MakeGetAttrSite(context);
            }

            try {
                if (getAttributeSlot.TryGetBoundValue(context, self, ((IPythonObject)self).PythonType, out value)) {
                    return callSite.Data.Target(callSite.Data, context, value, name);
                } 
            } catch (MissingMemberException) {
                if (getAttrSlot != null && getAttrSlot.TryGetBoundValue(context, self, ((IPythonObject)self).PythonType, out value)) {
                    return callSite.Data.Target(callSite.Data, context, value, name);
                }

                throw;
            }

            if (getAttrSlot != null && getAttrSlot.TryGetBoundValue(context, self, ((IPythonObject)self).PythonType, out value)) {
                return callSite.Data.Target(callSite.Data, context, value, name);
            }

            throw PythonOps.AttributeError(name);
        }

        public static object GetAttributeNoThrow(CodeContext/*!*/ context, object self, string name, PythonTypeSlot getAttributeSlot, PythonTypeSlot getAttrSlot, SiteLocalStorage<CallSite<DynamicSiteTarget<CodeContext, object, string, object>>>/*!*/ callSite) {
            object value;
            if (callSite.Data == null) {
                callSite.Data = MakeGetAttrSite(context);
            }

            try {
                if (getAttributeSlot.TryGetBoundValue(context, self, ((IPythonObject)self).PythonType, out value)) {
                    return callSite.Data.Target(callSite.Data, context, value, name);
                }
            } catch (MissingMemberException) {
                try {
                    if (getAttrSlot != null && getAttrSlot.TryGetBoundValue(context, self, ((IPythonObject)self).PythonType, out value)) {
                        return callSite.Data.Target(callSite.Data, context, value, name);
                    }

                    return OperationFailed.Value;
                } catch (MissingMemberException) {
                    return OperationFailed.Value;
                }
            }

            try {
                if (getAttrSlot != null && getAttrSlot.TryGetBoundValue(context, self, ((IPythonObject)self).PythonType, out value)) {
                    return callSite.Data.Target(callSite.Data, context, value, name);
                }
            } catch (MissingMemberException) {
            }

            return OperationFailed.Value;
        }

        private static CallSite<DynamicSiteTarget<CodeContext, object, string, object>> MakeGetAttrSite(CodeContext context) {
            return CallSite<DynamicSiteTarget<CodeContext, object, string, object>>.Create(
                new InvokeBinder(
                    PythonContext.GetContext(context).DefaultBinderState,
                    new CallSignature(1)
                )
            );
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
            if (res != NotImplementedType.Value && res != null && res.GetType() == typeof(bool))
                return (bool)res;

            return false;
        }

        private static object RichEqualsHelper(object self, object other) {
            object res;

            if (PythonTypeOps.TryInvokeBinaryOperator(DefaultContext.Default, self, other, Symbols.OperatorEquals, out res))
                return res;

            return NotImplementedType.Value;
        }

        #endregion
    }
}
