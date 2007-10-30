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
using System.Reflection;
using System.Collections;
using System.Diagnostics;
using System.ComponentModel;

using Microsoft.Scripting;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Actions;

using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

namespace IronPython.Runtime {

    public static partial class Converter {
        private static FastDynamicSite<object, int> _intSite = MakeExplicitConvertSite<int>();
        private static FastDynamicSite<object, double> _doubleSite = MakeExplicitConvertSite<double>();
        private static FastDynamicSite<object, Complex64> _complexSite = MakeExplicitConvertSite<Complex64>();
        private static FastDynamicSite<object, BigInteger> _bigIntSite = MakeExplicitConvertSite<BigInteger>();
        private static FastDynamicSite<object, string> _stringSite = MakeExplicitConvertSite<string>();
        private static FastDynamicSite<object, bool> _boolSite = MakeExplicitConvertSite<bool>();
        private static FastDynamicSite<object, char> _charSite = MakeImplicitConvertSite<char>();
        private static FastDynamicSite<object, char> _explicitCharSite = MakeExplicitConvertSite<char>();

        private static FastDynamicSite<object, T> MakeImplicitConvertSite<T>() {
            return MakeConvertSite<T>(ConversionResultKind.ImplicitCast);
        }

        private static FastDynamicSite<object, T> MakeExplicitConvertSite<T>() {
            return MakeConvertSite<T>(ConversionResultKind.ExplicitCast);
        }

        private static FastDynamicSite<object, T> MakeConvertSite<T>(ConversionResultKind kind) {
            return FastDynamicSite<object, T>.Create(DefaultContext.Default, ConvertToAction.Make(typeof(T), kind));
        }
        
        #region Conversion entry points

        public static Int32 ConvertToInt32(object value) { return _intSite.Invoke(value);             }
        public static Double ConvertToDouble(object value) { return _doubleSite.Invoke(value); }
        public static BigInteger ConvertToBigInteger(object value) { return _bigIntSite.Invoke(value); }
        public static Complex64 ConvertToComplex64(object value) { return _complexSite.Invoke(value); }
        public static String ConvertToString(object value) { return _stringSite.Invoke(value); }
        public static Char ConvertToChar(object value) { return _charSite.Invoke(value); }
        public static Boolean ConvertToBoolean(object value) { return _boolSite.Invoke(value); }

        #endregion

        internal static Char ExplicitConvertToChar(object value) {
            return _explicitCharSite.Invoke(value);
        }

        public static T Convert<T>(object value) {
            return (T)Convert(value, typeof(T));
        }

        internal static bool CanConvert(object value, Type to) {
            object dummy;
            return TryConvert(value, to, out dummy);
        }

        /// <summary>
        /// General conversion routine TryConvert - tries to convert the object to the desired type.
        /// Try to avoid using this method, the goal is to ultimately remove it!
        /// </summary>
        internal static bool TryConvert(object value, Type to, out object result) {
            try {
                result = Convert(value, to);
                return true;
            } catch {
                result = default(object);
                return false;
            }
        }

        internal static object Convert(object value, Type to) {
            if (value == null) {
                if (to == BooleanType) return RuntimeHelpers.False;

                if (to.IsValueType &&                    
                    (!to.IsGenericType || to.GetGenericTypeDefinition() != typeof(Nullable<>))) {

                    throw MakeTypeError(to, value);
                }
                return null;
            }

            Type from = value.GetType();
            if (from == to || to == ObjectType) return value;
            if (to.IsInstanceOfType(value)) return value;

            if (to == TypeType) return ConvertToType(value);
            if (to == Int32Type) return ConvertToInt32(value);
            if (to == DoubleType) return ConvertToDouble(value);
            if (to == BooleanType) return ConvertToBoolean(value);

            if (to == CharType) return ConvertToChar(value);
            if (to == StringType) return ConvertToString(value);

            if (to == BigIntegerType) return ConvertToBigInteger(value);
            if (to == Complex64Type) return ConvertToComplex64(value);

            if (to == ByteType) return ConvertToByte(value);
            if (to == SByteType) return ConvertToSByte(value);
            if (to == Int16Type) return ConvertToInt16(value);
            if (to == UInt32Type) return ConvertToUInt32(value);
            if (to == UInt64Type) return ConvertToUInt64(value);
            if (to == UInt16Type) return ConvertToUInt16(value);
            if (to == SingleType) return ConvertToSingle(value);
            if (to == Int64Type) return ConvertToInt64(value);
            if (to == DecimalType) return ConvertToDecimal(value);

            if (to == IEnumerableType) return ConvertToIEnumerable(value);

            if (DelegateType.IsAssignableFrom(to)) return ConvertToDelegate(value, to);

            if (to.IsArray) return ConvertToArray(value, to);

            Object result;
            if (TrySlowConvert(value, to, out result)) return result;

            throw MakeTypeError(to, value);
        }

        internal static bool TrySlowConvert(object value, Type to, out object result) {
            // check for implicit conversions 
            PythonType tt = DynamicHelpers.GetPythonTypeFromType(to);
            PythonType dt = DynamicHelpers.GetPythonType(value);

            if (tt.IsSystemType && dt.IsSystemType) {
                if (dt.TryConvertTo(value, tt, out result)) {
                    return true;
                }

                if (tt.TryConvertFrom(value, out result)) {
                    return true;
                }
            }

            if (to.IsGenericType) {
                Type genTo = to.GetGenericTypeDefinition();
                if (genTo == NullableOfTType) {
                    result = ConvertToNullableT(value, to.GetGenericArguments());
                    return true;
                }

                if (genTo == IListOfTType) {
                    result = ConvertToIListT(value, to.GetGenericArguments());
                    return true;
                }

                if (genTo == IDictOfTType) {
                    result = ConvertToIDictT(value, to.GetGenericArguments());
                    return true;
                }

                if (genTo == IEnumerableOfTType) {
                    result = ConvertToIEnumerableT(value, to.GetGenericArguments());
                    return true;
                }
            }

            if (value.GetType().IsValueType) {
                if (to == ValueTypeType) {
                    result = (System.ValueType)value;
                    return true;
                }
            }

#if !SILVERLIGHT
            if (value != null) {
                // try available type conversions...
                object[] tcas = to.GetCustomAttributes(typeof(TypeConverterAttribute), true);
                foreach (TypeConverterAttribute tca in tcas) {
                    TypeConverter tc = GetTypeConverter(tca);

                    if (tc == null) continue;

                    if (tc.CanConvertFrom(value.GetType())) {
                        result = tc.ConvertFrom(value);
                        return true;
                    }
                }
            }
#endif

            result = null;
            return false;
        }

        internal static bool TryConvertObject(object value, Type to, out object result) {
            //This is the fallback call for every fast path converter. If we land here,
            //then 'value' has to be a reference type which might have a custom converter
            //defined on its dynamic type. (Value Type conversions if any would have 
            //already taken place during the fast conversions and should not occur through
            //the dynamic types). 
            
            if (value == null || value.GetType().IsValueType) {
                result = null;
                return false;
            }

            return TrySlowConvert(value, to, out result);
        }

        /// <summary>
        /// This function tries to convert an object to IEnumerator, or wraps it into an adapter
        /// Do not use this function directly. It is only meant to be used by Ops.GetEnumerator.
        /// </summary>
        internal static bool TryConvertToIEnumerator(object o, out IEnumerator e) {
            if (o is string) {
                e = StringOps.GetEnumerator((string)o);
                return true;
            } else if (o is IEnumerable) {
                e = ((IEnumerable)o).GetEnumerator();
                return true;
            } else if (o is IEnumerator) {
                e = (IEnumerator)o;
                return true;
            }

            if (PythonEnumerator.TryCreate(o, out e)) {
                return true;
            }
            if (ItemEnumerator.TryCreate(o, out e)) {
                return true;
            }
            e = null;
            return false;
        }

        public static IEnumerable ConvertToIEnumerable(object o) {
            if (o == null) return null;

            IEnumerable e = o as IEnumerable;
            if (e != null) return e;

            PythonEnumerable pe;
            if (PythonEnumerable.TryCreate(o, out pe)) {
                return pe;
            }

            ItemEnumerable ie;
            // only user types get converted through ItemEnumerable, otherwise we use the
            // strong typing of the system types.
            if ((o is IPythonObject || o is OldInstance) && ItemEnumerable.TryCreate(o, out ie)) {
                return ie;
            }

            throw MakeTypeError("IEnumerable", o);
        }

        public static object ConvertToIEnumerableT(object value, Type[] enumOf) {
            Type type = IEnumerableOfTType.MakeGenericType(enumOf);
            if (type.IsInstanceOfType(value)) {
                return value;
            }

            IEnumerable ie = value as IEnumerable;
            if (ie == null) {
                ie = ConvertToIEnumerable(value);
            }

            type = IEnumerableOfTWrapperType.MakeGenericType(enumOf);
            object res = Activator.CreateInstance(type, ie);
            return res;
        }

        private static object ConvertToArray(object value, Type to) {
            int rank = to.GetArrayRank();

            if (rank == 1) {
                PythonTuple tupleVal = value as PythonTuple;
                if (tupleVal != null) {
                    Type elemType = to.GetElementType();
                    Array ret = Array.CreateInstance(elemType, tupleVal.Count);
                    try {
                        tupleVal.CopyTo(ret, 0);
                        return ret;
                    } catch (InvalidCastException) {
                        // invalid conversion
                        for (int i = 0; i < tupleVal.Count; i++) {
                            ret.SetValue(Convert(tupleVal[i], elemType), i);
                        }
                        return ret;
                    }
                }
            }

            throw MakeTypeError("Array", value);
        }

        internal static int ConvertToSliceIndex(object value) {
            int val;
            if (TryConvertToInt32(value, out val))
                return val;

            BigInteger bigval;
            if (TryConvertToBigInteger(value, out bigval)) {
                return bigval > 0 ? Int32.MaxValue : Int32.MinValue;
            }

            throw PythonOps.TypeError("slice indices must be integers");
        }

        internal static Exception CannotConvertTo(string name, object value) {
            return PythonOps.TypeError("Cannot convert {0}({1}) to {2}", PythonTypeOps.GetName(value), value, name);
        }

        internal static Exception CannotConvertOverflow(string name, object value) {
            return PythonOps.OverflowError("Cannot convert {0}({1}) to {2}", PythonTypeOps.GetName(value), value, name);
        }

        private static Exception MakeTypeError(Type expectedType, object o) {
            return MakeTypeError(DynamicHelpers.GetPythonTypeFromType(expectedType).Name.ToString(), o);
        }

        private static Exception MakeTypeError(string expectedType, object o) {
            return PythonOps.TypeErrorForTypeMismatch(expectedType, o);
        }

        #region Cached Type instances

        private static readonly Type Int16Type = typeof(System.Int16);
        private static readonly Type SByteType = typeof(System.SByte);
        private static readonly Type StringType = typeof(System.String);
        private static readonly Type UInt64Type = typeof(System.UInt64);
        private static readonly Type Int32Type = typeof(System.Int32);
        private static readonly Type DoubleType = typeof(System.Double);
        private static readonly Type DecimalType = typeof(System.Decimal);
        private static readonly Type ObjectType = typeof(System.Object);
        private static readonly Type Int64Type = typeof(System.Int64);
        private static readonly Type CharType = typeof(System.Char);
        private static readonly Type SingleType = typeof(System.Single);
        private static readonly Type BooleanType = typeof(System.Boolean);
        private static readonly Type UInt16Type = typeof(System.UInt16);
        private static readonly Type UInt32Type = typeof(System.UInt32);
        private static readonly Type ByteType = typeof(System.Byte);
        private static readonly Type BigIntegerType = typeof(BigInteger);
        private static readonly Type Complex64Type = typeof(Complex64);
        private static readonly Type DelegateType = typeof(Delegate);
        private static readonly Type IEnumerableType = typeof(IEnumerable);
        private static readonly Type ValueTypeType = typeof(ValueType);
        private static readonly Type TypeType = typeof(Type);
#if !SILVERLIGHT
        private static readonly Type ArrayListType = typeof(ArrayList);
        private static readonly Type HashtableType = typeof(Hashtable);
#endif
        private static readonly Type NullableOfTType = typeof(Nullable<>);
        private static readonly Type IListOfTType = typeof(System.Collections.Generic.IList<>);
        private static readonly Type ListOfTType = typeof(System.Collections.Generic.List<>);
        private static readonly Type IDictOfTType = typeof(System.Collections.Generic.IDictionary<,>);
        private static readonly Type ListWrapperForIListType = typeof(ListWrapperForIList<>);
        private static readonly Type IEnumerableOfTType = typeof(System.Collections.Generic.IEnumerable<>);
        private static readonly Type IEnumerableOfTWrapperType = typeof(IEnumerableOfTWrapper<>);
        private static readonly Type DictWrapperForIDictType = typeof(DictWrapperForIDict<,>);
        private static readonly Type IListOfObjectType = typeof(System.Collections.Generic.IList<object>);
        private static readonly Type IDictionaryOfObjectType = typeof(System.Collections.Generic.IDictionary<object, object>);
        private static readonly Type ListGenericWrapperType = typeof(ListGenericWrapper<>);
        private static readonly Type DictionaryGenericWrapperType = typeof(DictionaryGenericWrapper<,>);

        #endregion

        #region Implementation routines        

        //
        // ConvertToComplex64Impl Conversion Routine
        //
        private static bool ConvertToComplex64Impl(object value, out Complex64 result) {
            if (value is Complex64) {
                result = (Complex64)value;
                return true;
            } else if (value is Double) {
                result = Complex64.MakeReal((Double)value);
                return true;
            } else if (value is Extensible<Complex64>) {
                result = ((Extensible<Complex64>)value).Value;
                return true;
            } else {
                Double DoubleValue;
                if (ConvertToDoubleImpl(value, out DoubleValue)) {
                    result = Complex64.MakeReal(DoubleValue);
                    return true;
                }
            }
            result = default(Complex64);
            return false;
        }

        #endregion

        private static object ConvertToNullableT(object value, Type[] typeOf) {
            if (value == null) return null;
            else return Convert(value, typeOf[0]);
        }

        #region Entry points called from the generated code

        public static object ConvertToReferenceType(object fromObject, RuntimeTypeHandle typeHandle) {
            if (fromObject == null) return null;
            return Convert(fromObject, Type.GetTypeFromHandle(typeHandle));
        }

        public static object ConvertToNullableType(object fromObject, RuntimeTypeHandle typeHandle) {
            if (fromObject == null) return null;
            return Convert(fromObject, Type.GetTypeFromHandle(typeHandle));
        }

        public static object ConvertToValueType(object fromObject, RuntimeTypeHandle typeHandle) {
            if (fromObject == null) throw PythonOps.InvalidType(fromObject, typeHandle);
            return Convert(fromObject, Type.GetTypeFromHandle(typeHandle));
        }

        public static Type ConvertToType(object value) {
            if (value == null) return null;

            Type TypeVal = value as Type;
            if (TypeVal != null) return TypeVal;

            PythonType pythonTypeVal = value as PythonType;
            if (pythonTypeVal != null) return pythonTypeVal.UnderlyingSystemType;

            TypeGroup typeCollision = value as TypeGroup;
            if (typeCollision != null) {
                Type nonGenericType;
                if (typeCollision.TryGetNonGenericType(out nonGenericType)) {
                    return nonGenericType;
                }
            }

            throw MakeTypeError("Type", value);
        }

        public static object ConvertToDelegate(object value, Type to) {
            if (value == null) return null;
            return RuntimeHelpers.GetDelegate(value, to);
        }


        #endregion

        private static object ConvertToIListT(object value, Type[] listOf) {
            System.Collections.Generic.IList<object> lst = value as System.Collections.Generic.IList<object>;
            if (lst != null) {
                Type t = ListGenericWrapperType.MakeGenericType(listOf);
                return Activator.CreateInstance(t, lst);
            }
            throw MakeTypeError("IList<T>", value);
        }

        private static object ConvertToIDictT(object value, Type[] dictOf) {
            System.Collections.Generic.IDictionary<object, object> dict = value as System.Collections.Generic.IDictionary<object, object>;
            if (dict != null) {
                Type t = DictionaryGenericWrapperType.MakeGenericType(dictOf);
                return Activator.CreateInstance(t, dict);
            }
            throw MakeTypeError("IDictionary<K,V>", value);
        }

        public static bool CanConvertFrom(Type fromType, Type toType, NarrowingLevel allowNarrowing) {
            Contract.RequiresNotNull(fromType, "fromType");
            Contract.RequiresNotNull(toType, "toType");

            if (toType == fromType) return true;
            if (toType.IsAssignableFrom(fromType)) return true;
            if (fromType.IsCOMObject && toType.IsInterface) return true; // A COM object could be cast to any interface

            if (HasImplicitNumericConversion(fromType, toType)) return true;

            // Handling the hole that Type is the only object that we 'box'
            if (toType == TypeType && typeof(PythonType).IsAssignableFrom(fromType)) return true;

            // Support extensible types with simple implicit conversions to their base types
            if (typeof(Extensible<int>).IsAssignableFrom(fromType) && CanConvertFrom(Int32Type, toType, allowNarrowing)) {
                return true;
            }
            if (typeof(Extensible<BigInteger>).IsAssignableFrom(fromType) && CanConvertFrom(BigIntegerType, toType, allowNarrowing)) {
                return true;
            }
            if (typeof(ExtensibleString).IsAssignableFrom(fromType) && CanConvertFrom(StringType, toType, allowNarrowing)) {
                return true;
            }
            if (typeof(Extensible<double>).IsAssignableFrom(fromType) && CanConvertFrom(DoubleType, toType, allowNarrowing)) {
                return true;
            }
            if (typeof(Extensible<Complex64>).IsAssignableFrom(fromType) && CanConvertFrom(Complex64Type, toType, allowNarrowing)) {
                return true;
            }

#if !SILVERLIGHT
            // try available type conversions...
            object[] tcas = toType.GetCustomAttributes(typeof(TypeConverterAttribute), true);
            foreach (TypeConverterAttribute tca in tcas) {
                TypeConverter tc = GetTypeConverter(tca);

                if (tc == null) continue;

                if (tc.CanConvertFrom(fromType)) {
                    return true;
                }
            }
#endif

            //!!!do user-defined implicit conversions here

            if (allowNarrowing == NarrowingLevel.None) return false;

            return HasNarrowingConversion(fromType, toType, allowNarrowing);
        }

#if !SILVERLIGHT
        private static TypeConverter GetTypeConverter(TypeConverterAttribute tca) {
            try {
                ConstructorInfo ci = Type.GetType(tca.ConverterTypeName).GetConstructor(Type.EmptyTypes);
                if (ci != null) return ci.Invoke(ArrayUtils.EmptyObjects) as TypeConverter;
            } catch (TargetInvocationException) {
            }
            return null;
        }
#endif

        private static bool HasImplicitNumericConversion(Type fromType, Type toType) {
            if (fromType.IsEnum) return false;

            if (fromType == typeof(BigInteger)) {
                if (toType == typeof(double)) return true;
                if (toType == typeof(float)) return true;
                if (toType == typeof(Complex64)) return true;
                return false;
            }

            if (fromType == typeof(bool)) {
                if (toType == typeof(int)) return true;
                return HasImplicitNumericConversion(typeof(int), toType);
            }

            switch (Type.GetTypeCode(fromType)) {
                case TypeCode.SByte:
                    switch (Type.GetTypeCode(toType)) {
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                        default:
                            if (toType == BigIntegerType) return true;
                            if (toType == Complex64Type) return true;
                            return false;
                    }
                case TypeCode.Byte:
                    switch (Type.GetTypeCode(toType)) {
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                        default:
                            if (toType == BigIntegerType) return true;
                            if (toType == Complex64Type) return true;
                            return false;
                    }
                case TypeCode.Int16:
                    switch (Type.GetTypeCode(toType)) {
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                        default:
                            if (toType == BigIntegerType) return true;
                            if (toType == Complex64Type) return true;
                            return false;
                    }
                case TypeCode.UInt16:
                    switch (Type.GetTypeCode(toType)) {
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                        default:
                            if (toType == BigIntegerType) return true;
                            if (toType == Complex64Type) return true;
                            return false;
                    }
                case TypeCode.Int32:
                    switch (Type.GetTypeCode(toType)) {
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                        default:
                            if (toType == BigIntegerType) return true;
                            if (toType == Complex64Type) return true;
                            return false;
                    }
                case TypeCode.UInt32:
                    switch (Type.GetTypeCode(toType)) {
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                        default:
                            if (toType == BigIntegerType) return true;
                            if (toType == Complex64Type) return true;
                            return false;
                    }
                case TypeCode.Int64:
                    switch (Type.GetTypeCode(toType)) {
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                        default:
                            if (toType == BigIntegerType) return true;
                            if (toType == Complex64Type) return true;
                            return false;
                    }
                case TypeCode.UInt64:
                    switch (Type.GetTypeCode(toType)) {
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                        default:
                            if (toType == BigIntegerType) return true;
                            if (toType == Complex64Type) return true;
                            return false;
                    }
                case TypeCode.Char:
                    switch (Type.GetTypeCode(toType)) {
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                        default:
                            if (toType == BigIntegerType) return true;
                            if (toType == Complex64Type) return true;
                            return false;
                    }
                case TypeCode.Single:
                    switch (Type.GetTypeCode(toType)) {
                        case TypeCode.Double:
                            return true;
                        default:
                            if (toType == Complex64Type) return true;
                            return false;
                    }
                case TypeCode.Double:
                    switch (Type.GetTypeCode(toType)) {
                        default:
                            if (toType == Complex64Type) return true;
                            return false;
                    }
                default:
                    return false;
            }
        }

        public static bool PreferConvert(Type t1, Type t2) {
            if (t1 == typeof(bool) && t2 == typeof(int)) return true;
            if (t1 == typeof(Decimal) && t2 == typeof(BigInteger)) return true;
            //if (t1 == typeof(int) && t2 == typeof(BigInteger)) return true;

            switch (Type.GetTypeCode(t1)) {
                case TypeCode.SByte:
                    switch (Type.GetTypeCode(t2)) {
                        case TypeCode.Byte:
                        case TypeCode.UInt16:
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                            return true;
                        default:
                            return false;
                    }
                case TypeCode.Int16:
                    switch (Type.GetTypeCode(t2)) {
                        case TypeCode.UInt16:
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                            return true;
                        default:
                            return false;
                    }
                case TypeCode.Int32:
                    switch (Type.GetTypeCode(t2)) {
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                            return true;
                        default:
                            return false;
                    }
                case TypeCode.Int64:
                    switch (Type.GetTypeCode(t2)) {
                        case TypeCode.UInt64:
                            return true;
                        default:
                            return false;
                    }
            }
            return false;
        }

        private static bool HasNarrowingConversion(Type fromType, Type toType, NarrowingLevel allowNarrowing) {
            if (allowNarrowing == NarrowingLevel.Operator) {
                if (toType == CharType && fromType == StringType) return true;
                //if (toType == Int32Type && fromType == BigIntegerType) return true;
                //if (IsIntegral(fromType) && IsIntegral(toType)) return true;

                //Check if there is an implicit convertor defined on fromType to toType
                if (HasImplicitConversion(fromType, toType)) {
                    return true;
                }
            }

            if (toType == DoubleType && fromType == DecimalType) return true;
            if (toType == SingleType && fromType == DecimalType) return true;

            if (allowNarrowing == NarrowingLevel.All) {
                if (IsNumeric(fromType) && IsNumeric(toType)) return true;

                if (toType.IsArray) {
                    return typeof(PythonTuple).IsAssignableFrom(fromType);
                }

                if (toType == CharType && fromType == StringType) return true;
                if (toType == Int32Type && fromType == BooleanType) return true;

                // Everything can convert to Boolean in Python
                if (toType == BooleanType) return true;

                if (DelegateType.IsAssignableFrom(toType) && IsPythonType(fromType)) return true;
                if (IEnumerableType == toType && IsPythonType(fromType)) return true;

                //__int__, __float__, __long__
                if (toType == Int32Type && HasPythonProtocol(fromType, Symbols.ConvertToInt)) return true;
                if (toType == DoubleType && HasPythonProtocol(fromType, Symbols.ConvertToFloat)) return true;
                if (toType == BigIntegerType && HasPythonProtocol(fromType, Symbols.ConvertToLong)) return true;
            }

            if (toType.IsGenericType) {
                Type genTo = toType.GetGenericTypeDefinition();
                if (genTo == IListOfTType) {
                    return IListOfObjectType.IsAssignableFrom(fromType);
                } else if (genTo == typeof(System.Collections.Generic.IEnumerator<>)) {
                    if (IsPythonType(fromType)) return true;
                } else if (genTo == IDictOfTType) {
                    return IDictionaryOfObjectType.IsAssignableFrom(fromType);
                }
            }

            if (fromType == BigIntegerType && toType == Int64Type) return true;

            return false;
        }

        private static bool HasImplicitConversion(Type fromType, Type toType) {
            foreach (MethodInfo method in fromType.GetMethods()) {
                if (method.Name == "op_Implicit" &&
                    method.GetParameters()[0].ParameterType == fromType &&
                    method.ReturnType == toType) {
                    return true;
                }
            }
            return false;
        }

        private static bool IsIntegral(Type t) {
            switch (Type.GetTypeCode(t)) {
                case TypeCode.DateTime:
                case TypeCode.DBNull:
                case TypeCode.Char:
                case TypeCode.Empty:
                case TypeCode.String:
                case TypeCode.Single:
                case TypeCode.Double:
                    return false;
                case TypeCode.Object:
                    return t == BigIntegerType;
                default:
                    return true;
            }
        }

        private static bool IsNumeric(Type t) {
            if (t.IsEnum) return false;

            switch (Type.GetTypeCode(t)) {
                case TypeCode.DateTime:
                case TypeCode.DBNull:
                case TypeCode.Char:
                case TypeCode.Empty:
                case TypeCode.String:
                case TypeCode.Boolean:
                    return false;
                case TypeCode.Object:
                    return t == BigIntegerType || t == Complex64Type;
                default:
                    return true;
            }
        }

        private static bool IsPythonType(Type t) {
            return t.FullName.StartsWith("IronPython."); //!!! this and the check below are hacks
        }

        private static bool HasPythonProtocol(Type t, SymbolId name) {
            if (t.FullName.StartsWith(Compiler.Generation.NewTypeMaker.TypePrefix)) return true;
            if (t == typeof(OldInstance)) return true;
            PythonType dt = DynamicHelpers.GetPythonTypeFromType(t);
            if (dt == null) return false;
            PythonTypeSlot tmp;
            return dt.TryResolveSlot(DefaultContext.Default, name, out tmp);
        }
    }
}
