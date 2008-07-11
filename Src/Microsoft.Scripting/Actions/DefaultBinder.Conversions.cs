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
using System.Reflection;
using System.Linq.Expressions;
using System.Scripting;
using System.Scripting.Actions;
using System.Scripting.Generation;
using System.Scripting.Runtime;
using System.Scripting.Utils;

using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Actions {
    using Ast = System.Linq.Expressions.Expression;

    public partial class DefaultBinder : ActionBinder {
        public MetaObject/*!*/ ConvertTo(Type/*!*/ toType, ConversionResultKind kind, MetaObject/*!*/ arg) {
            ContractUtils.RequiresNotNull(toType, "toType");
            ContractUtils.RequiresNotNull(arg, "arg");

            Type knownType = arg.RuntimeType;

            // try all the conversions - first look for conversions against the expression type,
            // these can be done w/o any additional tests.  Then look for conversions against the 
            // restricted type.
            Restrictions typeRestrictions = arg.Restrictions.Merge(Restrictions.TypeRestriction(arg.Expression, arg.RuntimeType));

            return
                TryConvertToObject(toType, arg.Expression.Type, arg) ??
                TryAllConversions(toType, kind, arg.Expression.Type, arg.Restrictions, arg) ??
                TryAllConversions(toType, kind, arg.RuntimeType, typeRestrictions, arg) ??
                TryExtraConversions(toType, typeRestrictions, arg) ??
                MakeErrorTarget(toType, kind, typeRestrictions, arg);
        }

        #region Conversion attempt helpers

        /// <summary>
        /// Checks if the conversion is to object and produces a target if it is.
        /// </summary>
        private static MetaObject TryConvertToObject(Type/*!*/ toType, Type/*!*/ knownType, MetaObject/*!*/ arg) {
            if (toType == typeof(object)) {
                if (knownType.IsValueType) {
                    return MakeBoxingTarget(arg);
                } else {
                    return arg;
                }
            }
            return null;
        }

        /// <summary>
        /// Checks if any conversions are available and if so builds the target for that conversion.
        /// </summary>
        private MetaObject TryAllConversions(Type/*!*/ toType, ConversionResultKind kind, Type/*!*/ knownType, Restrictions/*!*/ restrictions, MetaObject/*!*/ arg) {
            return
                TryAssignableConversion(toType, knownType, restrictions, arg) ??           // known type -> known type
                TryExtensibleConversion(toType, knownType, restrictions, arg) ??           // Extensible<T> -> Extensible<T>.Value
                TryUserDefinedConversion(kind, toType, knownType, restrictions, arg) ?? // op_Implicit
                TryImplicitNumericConversion(toType, knownType, restrictions, arg) ??      // op_Implicit
                TryNullableConversion(toType, kind, knownType, restrictions, arg) ??         // null -> Nullable<T> or T -> Nullable<T>
                TryEnumerableConversion(toType, knownType, restrictions, arg) ??           // IEnumerator -> IEnumerable / IEnumerator<T> -> IEnumerable<T>
                TryNullConversion(toType, knownType, restrictions);                        // null -> reference type
        }

        /// <summary>
        /// Checks if the conversion can be handled by a simple cast.
        /// </summary>
        private static MetaObject TryAssignableConversion(Type/*!*/ toType, Type/*!*/ type, Restrictions/*!*/ restrictions, MetaObject/*!*/ arg) {
            if (toType.IsAssignableFrom(type) ||
                (type == typeof(None) && (toType.IsClass || toType.IsInterface))) {
                if (toType == typeof(IEnumerator) && typeof(IEnumerable).IsAssignableFrom(type)) {
                    // Special case to handle C#-defined enumerators that implement both IEnumerable and IEnumerator
                    return null;
                }
                // MakeSimpleConversionTarget handles the ConversionResultKind check
                return MakeSimpleConversionTarget(toType, restrictions, arg);
            }

            return null;
        }

        /// <summary>
        /// Checks if the conversion can be handled by calling a user-defined conversion method.
        /// </summary>
        private MetaObject TryUserDefinedConversion(ConversionResultKind kind, Type/*!*/ toType, Type/*!*/ type, Restrictions/*!*/ restrictions, MetaObject/*!*/ arg) {
            Type fromType = GetUnderlyingType(type);

            MetaObject res = 
                   TryOneConversion(kind, toType, type, fromType, "op_Implicit", true, restrictions, arg) ??
                   TryOneConversion(kind, toType, type, fromType, "ConvertTo" + toType.Name, true, restrictions, arg);

            if (kind == ConversionResultKind.ExplicitCast || 
                kind == ConversionResultKind.ExplicitTry) {
                // finally try explicit conversions
                res = res ??
                    TryOneConversion(kind, toType, type, fromType, "op_Explicit", false, restrictions, arg) ??
                    TryOneConversion(kind, toType, type, fromType, "ConvertTo" + toType.Name, false, restrictions, arg);
            }
            
            return res;
        }

        /// <summary>
        /// Helper that checkes both types to see if either one defines the specified conversion
        /// method.
        /// </summary>
        private MetaObject TryOneConversion(ConversionResultKind kind, Type toType, Type type, Type fromType, string methodName, bool isImplicit, Restrictions/*!*/ restrictions, MetaObject/*!*/ arg) {
            OldConvertToAction action = OldConvertToAction.Make(this, toType, kind);

            MemberGroup conversions = GetMember(action, fromType, methodName);
            MetaObject res = TryUserDefinedConversion(kind, toType, type, conversions, isImplicit, restrictions, arg);
            if (res != null) {
                return res;
            }

            // then on the type we're trying to convert to
            conversions = GetMember(action, toType, methodName);
            return TryUserDefinedConversion(kind, toType, type, conversions, isImplicit, restrictions, arg);
        }

        /// <summary>
        /// Checks if any of the members of the MemberGroup provide the applicable conversion and 
        /// if so uses it to build a conversion rule.
        /// </summary>
        private static MetaObject TryUserDefinedConversion(ConversionResultKind kind, Type toType, Type type, MemberGroup conversions, bool isImplicit, Restrictions/*!*/ restrictions, MetaObject/*!*/ arg) {
            Type checkType = GetUnderlyingType(type);

            foreach (MemberTracker mt in conversions) {
                if (mt.MemberType != TrackerTypes.Method) continue;                

                MethodTracker method = (MethodTracker)mt;

                if (isImplicit && method.Method.IsDefined(typeof(ExplicitConversionMethodAttribute), true)) {
                    continue;
                }

                if (method.Method.ReturnType == toType) {   // TODO: IsAssignableFrom?  IsSubclass?
                    ParameterInfo[] pis = method.Method.GetParameters();

                    if (pis.Length == 1 && pis[0].ParameterType.IsAssignableFrom(checkType)) {
                        // we can use this method
                        if (type == checkType) {
                            return MakeConversionTarget(kind, method, type, isImplicit, restrictions, arg);
                        } else {
                            return MakeExtensibleConversionTarget(kind, method, type, isImplicit, restrictions, arg);
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Checks if the conversion is to applicable by extracting the value from Extensible of T.
        /// </summary>
        private static MetaObject TryExtensibleConversion(Type/*!*/ toType, Type/*!*/ type, Restrictions/*!*/ restrictions, MetaObject/*!*/ arg) {
            Type extensibleType = typeof(Extensible<>).MakeGenericType(toType);
            if (extensibleType.IsAssignableFrom(type)) {
                return MakeExtensibleTarget(extensibleType, restrictions, arg);
            }
            return null;
        }

        /// <summary>
        /// Checks if there's an implicit numeric conversion for primitive data types.
        /// </summary>
        private static MetaObject TryImplicitNumericConversion(Type/*!*/ toType, Type/*!*/ type, Restrictions/*!*/ restrictions, MetaObject/*!*/ arg) {
            Type checkType = type;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Extensible<>)) {
                checkType = type.GetGenericArguments()[0];
            }

            if (TypeUtils.IsNumeric(toType) && TypeUtils.IsNumeric(checkType)) {
                // check for an explicit conversion
                int tx, ty, fx, fy;
                if (TypeUtils.GetNumericConversionOrder(Type.GetTypeCode(toType), out tx, out ty) &&
                    TypeUtils.GetNumericConversionOrder(Type.GetTypeCode(checkType), out fx, out fy)) {
                    if (TypeUtils.IsImplicitlyConvertible(fx, fy, tx, ty)) {
                        // MakeSimpleConversionTarget handles the ConversionResultKind check
                        if (type == checkType) {
                            return MakeSimpleConversionTarget(toType, restrictions, arg);
                        } else {
                            return MakeSimpleExtensibleConversionTarget(toType, restrictions, arg);
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Checks if there's a conversion to/from Nullable of T.
        /// </summary>
        private MetaObject TryNullableConversion(Type/*!*/ toType, ConversionResultKind kind, Type/*!*/ knownType, Restrictions/*!*/ restrictions, MetaObject/*!*/ arg) {
            if (toType.IsGenericType && toType.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                if (knownType == typeof(None)) {
                    // null -> Nullable<T>
                    return MakeNullToNullableOfTTarget(toType, restrictions);
                } else if (knownType == toType.GetGenericArguments()[0]) {
                    return MakeTToNullableOfTTarget(toType, knownType, restrictions, arg);
                } else if (kind == ConversionResultKind.ExplicitCast || kind == ConversionResultKind.ExplicitTry) {
                    if (knownType != typeof(object)) {
                        // when doing an explicit cast we'll do things like int -> Nullable<float>
                        return MakeConvertingToTToNullableOfTTarget(toType, kind, restrictions, arg);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Checks if there's a conversion to IEnumerator or IEnumerator of T via calling GetEnumerator
        /// </summary>
        private static MetaObject TryEnumerableConversion(Type/*!*/ toType, Type/*!*/ knownType, Restrictions/*!*/ restrictions, MetaObject/*!*/ arg) {
            if (toType == typeof(IEnumerator)) {
                return MakeIEnumerableTarget(knownType, restrictions, arg);
            } else if (toType.IsInterface && toType.IsGenericType && toType.GetGenericTypeDefinition() == typeof(IEnumerator<>)) {
                return MakeIEnumeratorOfTTarget(toType, knownType, restrictions, arg);
            }
            return null;
        }

        /// <summary>
        /// Checks to see if there's a conversion of null to a reference type
        /// </summary>
        private static MetaObject TryNullConversion(Type/*!*/ toType, Type/*!*/ knownType, Restrictions/*!*/ restrictions) {
            if (knownType == typeof(None) && !toType.IsValueType) {
                return MakeNullTarget(toType, restrictions);
            }
            return null;
        }

        /// <summary>
        /// Checks for any extra conversions which aren't based upon the incoming type of the object.
        /// </summary>
        private static MetaObject TryExtraConversions(Type/*!*/ toType, Restrictions/*!*/ restrictions, MetaObject/*!*/ arg) {
            if (typeof(Delegate).IsAssignableFrom(toType) && toType != typeof(Delegate)) {
                // generate a conversion to delegate
                return MakeDelegateTarget(toType, restrictions, arg);
            }
            return null;
        }

        #endregion

        #region Rule production helpers

        /// <summary>
        /// Helper to produce an error when a conversion cannot occur
        /// </summary>
        private MetaObject/*!*/ MakeErrorTarget(Type/*!*/ toType, ConversionResultKind kind, Restrictions/*!*/ restrictions, MetaObject/*!*/ arg) {
            MetaObject target;

            switch (kind) {
                case ConversionResultKind.ImplicitCast:
                case ConversionResultKind.ExplicitCast:
                    target = MakeError(
                        MakeConversionError(toType, arg.Expression),
                        restrictions
                    );
                    break;
                case ConversionResultKind.ImplicitTry:
                case ConversionResultKind.ExplicitTry:
                    target = new MetaObject(
                        CompilerHelpers.GetTryConvertReturnValue(toType),
                        restrictions
                    );
                    break;
                default:
                    throw new InvalidOperationException(kind.ToString());
            }

            return target;
        }

        /// <summary>
        /// Helper to produce a rule which just boxes a value type
        /// </summary>
        private static MetaObject/*!*/ MakeBoxingTarget(MetaObject arg) {
            // MakeSimpleConversionTarget handles the ConversionResultKind check
            return MakeSimpleConversionTarget(typeof(object), arg.Restrictions, arg);
        }

        /// <summary>
        /// Helper to produce a conversion rule by calling the helper method to do the convert
        /// </summary>
        private static MetaObject/*!*/ MakeConversionTarget(ConversionResultKind kind, MethodTracker/*!*/ method, Type/*!*/ fromType, bool isImplicit, Restrictions/*!*/ restrictions, MetaObject/*!*/ arg) {            
            Expression param = Ast.ConvertHelper(arg.Expression, fromType);

            return MakeConversionTargetWorker(kind, method, isImplicit, restrictions, param);
        }

        /// <summary>
        /// Helper to produce a conversion rule by calling the helper method to do the convert
        /// </summary>
        private static MetaObject/*!*/ MakeExtensibleConversionTarget(ConversionResultKind kind, MethodTracker/*!*/ method, Type/*!*/ fromType, bool isImplicit, Restrictions/*!*/ restrictions, MetaObject/*!*/ arg) {
            return MakeConversionTargetWorker(kind, method, isImplicit, restrictions, GetExtensibleValue(fromType, arg));
        }

        /// <summary>
        /// Helper to produce a conversion rule by calling the method to do the convert.  This version takes the parameter
        /// to be passed to the conversion function and we call it w/ our own value or w/ our Extensible.Value.
        /// </summary>
        private static MetaObject/*!*/ MakeConversionTargetWorker(ConversionResultKind kind, MethodTracker/*!*/ method, bool isImplicit, Restrictions/*!*/ restrictions, Expression/*!*/ param) {
            return new MetaObject(
                WrapForThrowingTry(
                    kind,
                    isImplicit,
                    Ast.SimpleCallHelper(
                        method.Method,
                        param
                    ),
                    method.Method.ReturnType
                ),
                restrictions
            );
        }

        /// <summary>
        /// Helper to wrap explicit conversion call into try/catch incase it throws an exception.  If
        /// it throws the default value is returned.
        /// </summary>
        private static Expression/*!*/ WrapForThrowingTry(ConversionResultKind kind, bool isImplicit, Expression/*!*/ ret, Type retType) {
            if (!isImplicit && kind == ConversionResultKind.ExplicitTry) {
                Expression convFailed = GetTryConvertReturnValue(retType);
                VariableExpression tmp = Ast.Variable(convFailed.Type == typeof(object) ? typeof(object) : ret.Type, "tmp");
                ret = Ast.Scope(
                    Ast.Comma(
                        Ast.Try(
                            Ast.Assign(tmp, Ast.ConvertHelper(ret, tmp.Type))
                        ).Catch(
                            typeof(Exception),
                            Ast.Assign(tmp, convFailed)
                        ),
                        tmp
                    ),
                    tmp
                );
            }
            return ret;
        }

        /// <summary>
        /// Helper to produce a rule when no conversion is required (the strong type of the expression
        /// input matches the type we're converting to or has an implicit conversion at the IL level)
        /// </summary>
        private static MetaObject/*!*/ MakeSimpleConversionTarget(Type toType, Restrictions/*!*/ restrictions, MetaObject/*!*/ arg) {
            return new MetaObject(
                Ast.ConvertHelper(arg.Expression, CompilerHelpers.GetVisibleType(toType)),
                restrictions);

            /*
            if (toType.IsValueType && _rule.ReturnType == typeof(object) && Expression.Type == typeof(object)) {
                // boxed value type is being converted back to object.  We've done 
                // the type check, there's no need to unbox & rebox the value.  infact 
                // it breaks calls on instance methods so we need to avoid it.
                _rule.Target =
                    _rule.MakeReturn(
                        Binder,
                        Expression
                    );
            } 
             * */
        }

        /// <summary>
        /// Helper to produce a rule when no conversion is required from an extensible type's
        /// underlying storage to the type we're converting to.  The type of extensible type
        /// matches the type we're converting to or has an implicit conversion at the IL level.
        /// </summary>
        private static MetaObject/*!*/ MakeSimpleExtensibleConversionTarget(Type/*!*/ toType, Restrictions/*!*/ restrictions, MetaObject/*!*/ arg) {
            Type extType = typeof(Extensible<>).MakeGenericType(toType);

            return new MetaObject(
                Ast.ConvertHelper(
                    GetExtensibleValue(extType, arg),
                    toType
                ),
                restrictions
            );
        }       

        /// <summary>
        /// Helper to extract the value from an Extensible of T
        /// </summary>
        private static MetaObject/*!*/ MakeExtensibleTarget(Type/*!*/ extensibleType, Restrictions/*!*/ restrictions, MetaObject/*!*/ arg) {
            return new MetaObject(
                Ast.Property(Ast.Convert(arg.Expression, extensibleType), extensibleType.GetProperty("Value")),
                restrictions
            );
        }

        /// <summary>
        /// Helper to convert a null value to nullable of T
        /// </summary>
        private static MetaObject/*!*/ MakeNullToNullableOfTTarget(Type/*!*/ toType, Restrictions/*!*/ restrictions) {
            return new MetaObject(
                Ast.Call(typeof(RuntimeHelpers).GetMethod("CreateInstance").MakeGenericMethod(toType)),
                restrictions
            );
        }

        /// <summary>
        /// Helper to produce the rule for converting T to Nullable of T
        /// </summary>
        private static MetaObject/*!*/ MakeTToNullableOfTTarget(Type/*!*/ toType, Type/*!*/ knownType, Restrictions/*!*/ restrictions, MetaObject/*!*/ arg) {
            // T -> Nullable<T>
            return new MetaObject(
                Ast.New(
                    toType.GetConstructor(new Type[] { knownType }),
                    Ast.ConvertHelper(arg.Expression, knownType)
                ),
                restrictions
            );
        }

        /// <summary>
        /// Helper to produce the rule for converting T to Nullable of T
        /// </summary>
        private MetaObject/*!*/ MakeConvertingToTToNullableOfTTarget(Type/*!*/ toType, ConversionResultKind kind, Restrictions/*!*/ restrictions, MetaObject/*!*/ arg) {
            Type valueType = toType.GetGenericArguments()[0];
            
            // ConvertSelfToT -> Nullable<T>
            if (kind == ConversionResultKind.ExplicitCast) {
                // if the conversion to T fails we just throw
                Expression conversion = ConvertExpression(arg.Expression, valueType, kind, Ast.Null(typeof(CodeContext)));
                
                return new MetaObject(
                    Ast.New(
                        toType.GetConstructor(new Type[] { valueType }),
                        conversion
                    ),
                    restrictions
                );
            } else {
                Expression conversion = ConvertExpression(arg.Expression, valueType, kind, Ast.Null(typeof(CodeContext)));

                // if the conversion to T succeeds then produce the nullable<T>, otherwise return default(retType)
                VariableExpression tmp = Ast.Variable(typeof(object), "tmp");
                return new MetaObject(
                    Ast.Scope(
                        Ast.Condition(
                            Ast.NotEqual(
                                Ast.Assign(tmp, conversion),
                                Ast.Constant(null)
                            ),
                            Ast.New(
                                toType.GetConstructor(new Type[] { valueType }),
                                Ast.Convert(
                                    tmp,
                                    valueType
                                )
                            ),
                            CompilerHelpers.GetTryConvertReturnValue(toType)
                        ),
                        tmp
                    ),
                    restrictions
                );
            }
        }

        /// <summary>
        /// Returns a value which indicates failure when a OldConvertToAction of ImplicitTry or
        /// ExplicitTry.
        /// </summary>
        public static Expression GetTryConvertReturnValue(Type type) {
            Expression res;
            if (type.IsInterface || type.IsClass) {
                res = Ast.Null(type);
            } else {
                res = Ast.Null();
            }

            return res;
        }

        /// <summary>
        /// Helper to extract the Value of an Extensible of T from the
        /// expression being converted.
        /// </summary>
        private static Expression GetExtensibleValue(Type/*!*/ extType, MetaObject/*!*/ arg) {
            return Ast.Property(
                Ast.ConvertHelper(
                    arg.Expression,
                    extType
                ),
                extType.GetProperty("Value")
            );
        }

        /// <summary>
        /// Helper that checks if fromType is an Extensible of T or a subtype of 
        /// Extensible of T and if so returns the T.  Otherwise it returns fromType.
        /// 
        /// This is used to treat extensible types the same as their underlying types.
        /// </summary>
        private static Type GetUnderlyingType(Type fromType) {
            Type curType = fromType;
            do {
                if (curType.IsGenericType && curType.GetGenericTypeDefinition() == typeof(Extensible<>)) {
                    fromType = curType.GetGenericArguments()[0];
                }
                curType = curType.BaseType;
            } while (curType != null);
            return fromType;
        }

        /// <summary>
        /// Makes a conversion target which converts IEnumerable -> IEnumerator
        /// </summary>
        private static MetaObject MakeIEnumerableTarget(Type/*!*/ knownType, Restrictions/*!*/ restrictions, MetaObject/*!*/ arg) {
            bool canConvert = typeof(IEnumerable).IsAssignableFrom(knownType);

            if (canConvert) {
                return new MetaObject(
                    Ast.Call(
                        Ast.ConvertHelper(arg.Expression, typeof(IEnumerable)),
                        typeof(IEnumerable).GetMethod("GetEnumerator")
                    ),
                    restrictions
                );
            }
            return null;
        }

        /// <summary>
        /// Makes a conversion target which converts IEnumerable of T to IEnumerator of T
        /// </summary>
        private static MetaObject MakeIEnumeratorOfTTarget(Type/*!*/ toType, Type/*!*/ knownType, Restrictions/*!*/ restrictions, MetaObject/*!*/ arg) {
            Type enumType = typeof(IEnumerable<>).MakeGenericType(toType.GetGenericArguments()[0]);
            if (enumType.IsAssignableFrom(knownType)) {
                return new MetaObject(
                    Ast.Call(
                        Ast.ConvertHelper(arg.Expression, enumType),
                        toType.GetMethod("GetEnumerator")
                    ),
                    restrictions
                );
            }
            return null;
        }

        /// <summary>
        /// Creates a target which returns null for a reference type.
        /// </summary>
        private static MetaObject/*!*/ MakeNullTarget(Type toType, Restrictions/*!*/ restrictions) {
            return new MetaObject(
                Ast.Convert(Ast.Null(), toType),
                restrictions
            );
        }

        /// <summary>
        /// Creates a target which creates a new dynamic method which contains a single
        /// dynamic site that invokes the callable object.
        /// </summary>
        private static MetaObject/*!*/ MakeDelegateTarget(Type/*!*/ toType, Restrictions/*!*/ restrictions, MetaObject/*!*/ arg) {
            return new MetaObject(
                Ast.Call(
                    typeof(BinderOps).GetMethod("GetDelegate"),
                    Ast.Null(typeof(CodeContext)),
                    arg.Expression,
                    Ast.Constant(toType)
                ),
                restrictions
            );
        }

        #endregion
    }
}
