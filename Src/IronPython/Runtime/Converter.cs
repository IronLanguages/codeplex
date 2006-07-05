/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Shared Source License
 * for IronPython. A copy of the license can be found in the License.html file
 * at the root of this distribution. If you can not locate the Shared Source License
 * for IronPython, please send an email to ironpy@microsoft.com.
 * By using this source code in any fashion, you are agreeing to be bound by
 * the terms of the Shared Source License for IronPython.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

using System;
using IronMath;
using System.Reflection;
using System.Collections;
using System.Diagnostics;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

namespace IronPython.Runtime {

    public enum NarrowingLevel {
        None,
        Preferred,
        All
    }

    public static partial class Converter {
        #region Conversion entry points

        //
        // ConvertToByte - fast paths and custom logic
        //
        public static Byte ConvertToByte(object value) {
            // Fast Paths
            if (value is Int32) return checked((Byte)(Int32)value);
            if (value is Boolean) return (Boolean)value ? (Byte)1 : (Byte)0;
            if (value is Byte) return (Byte)value;

            // Fall back to comprehensive conversion
            Byte result;
            if (ConvertToByteImpl(value, out result)) return result;

            //// Fall back to __xxx__ method call
            //object newValue;
            //if (Ops.TryToInvoke(value, SymbolTable.ConvertToInt, out newValue)) {
            //    // Convert resulting object to the desired type
            //    if (ConvertToByteImpl(newValue, out result)) return result;
            //}

            throw CannotConvertTo("Byte", value);
        }

        //
        // ConvertToSByte - fast paths and custom logic
        //
        public static SByte ConvertToSByte(object value) {
            // Fast Paths
            if (value is Int32) return checked((SByte)(Int32)value);
            if (value is Boolean) return (Boolean)value ? (SByte)1 : (SByte)0;
            if (value is SByte) return (SByte)value;

            // Fall back to comprehensive conversion
            SByte result;
            if (ConvertToSByteImpl(value, out result)) return result;

            //// Fall back to __xxx__ method call
            //object newValue;
            //if (Ops.TryToInvoke(value, SymbolTable.ConvertToInt, out newValue)) {
            //    // Convert resulting object to the desired type
            //    if (ConvertToSByteImpl(newValue, out result)) return result;
            //}

            throw CannotConvertTo("SByte", value);
        }

        //
        // ConvertToInt16 - fast paths and custom logic
        //
        public static Int16 ConvertToInt16(object value) {
            // Fast Paths
            if (value is Int32) return checked((Int16)(Int32)value);
            if (value is Boolean) return (Boolean)value ? (Int16)1 : (Int16)0;
            if (value is Int16) return (Int16)value;

            Int16 result;
            // Fall back to comprehensive conversion
            if (ConvertToInt16Impl(value, out result)) return result;

            // Fall back to __xxx__ method call
            //object newValue;
            //if (Ops.TryToInvoke(value, SymbolTable.ConvertToInt, out newValue)) {
            //    // Convert resulting object to the desired type
            //    if (ConvertToInt16Impl(newValue, out result)) return result;
            //}

            throw CannotConvertTo("Int16", value);
        }

        //
        // ConvertToInt32 - fast paths and custom logic
        //
        public static Int32 ConvertToInt32(object value) {
            // Fast Paths
            ExtensibleInt ei;
            BigInteger bi;
            if (value is Int32) return (Int32)value;
            if ((ei = value as ExtensibleInt) != null) return ei.value;
            if (value is Boolean) return ((Boolean)value) ? 1 : 0;
            if ((Object)(bi = value as BigInteger) != null) return bi.ToInt32();

            // Fall back to comprehensive conversion
            Int32 result;
            if (ConvertToInt32Impl(value, out result)) return result;

            // Fall back to __xxx__ method call
            object newValue;
            if (Ops.TryToInvoke(value, SymbolTable.ConvertToInt, out newValue)) {
                // Convert resulting object to the desired type
                if (ConvertToInt32Impl(newValue, out result)) return result;
            }

            throw CannotConvertTo("Int32", value);
        }

        //
        // ConvertToInt64 - fast paths and custom logic
        //
        public static Int64 ConvertToInt64(object value) {
            // Fast Paths
            ExtensibleInt ei;
            BigInteger bi;

            if (value is Int32) return (Int64)(Int32)value;
            if ((ei = value as ExtensibleInt) != null) return (Int64)ei.value;
            if (value is Boolean) return (Boolean)value ? (Int64)1 : (Int64)0;
            if ((Object)(bi = value as BigInteger) != null) return ((BigInteger)value).ToInt64();

            // Fall back to comprehensive conversion
            Int64 result;
            if (ConvertToInt64Impl(value, out result)) return result;

            // Fall back to __xxx__ method call
            //object newValue;
            //if (Ops.TryToInvoke(value, SymbolTable.ConvertToInt, out newValue)) {
            //    // Convert resulting object to the desired type
            //    if (ConvertToInt64Impl(newValue, out result)) return result;
            //}
            //if (Ops.TryToInvoke(value, SymbolTable.ConvertToLong, out newValue)) {
            //    // Convert resulting object to the desired type
            //    if (ConvertToInt64Impl(newValue, out result)) return result;
            //}

            throw CannotConvertTo("Int64", value);
        }

        //
        // ConvertToUInt16 - fast paths and custom logic
        //
        public static UInt16 ConvertToUInt16(object value) {
            // Fast Paths
            if (value is Int32) return checked((UInt16)(Int32)value);
            if (value is UInt16) return (UInt16)value;

            // Fall back to comprehensive conversion
            UInt16 result;
            if (ConvertToUInt16Impl(value, out result)) return result;

            //// Fall back to __xxx__ method call
            //object newValue;
            //if (Ops.TryToInvoke(value, SymbolTable.ConvertToInt, out newValue)) {
            //    // Convert resulting object to the desired type
            //    if (ConvertToUInt16Impl(newValue, out result)) return result;
            //}

            throw CannotConvertTo("UInt16", value);
        }

        //
        // ConvertToUInt32 - fast paths and custom logic
        //
        public static UInt32 ConvertToUInt32(object value) {
            // Fast Paths
            if (value is Int32) return checked((UInt32)(Int32)value);
            if (value is UInt32) return (UInt32)value;

            // Fall back to comprehensive conversion
            UInt32 result;
            if (ConvertToUInt32Impl(value, out result)) return result;

            // Fall back to __xxx__ method call
            //object newValue;
            //if (Ops.TryToInvoke(value, SymbolTable.ConvertToInt, out newValue)) {
            //    // Convert resulting object to the desired type
            //    if (ConvertToUInt32Impl(newValue, out result)) return result;
            //}

            throw CannotConvertTo("UInt32", value);
        }

        //
        // ConvertToUInt64 - fast paths and custom logic
        //
        public static UInt64 ConvertToUInt64(object value) {
            // Fast Paths
            if (value is Int32) return checked((UInt64)(Int32)value);
            if (value is UInt64) return (UInt64)value;

            // Fall back to comprehensive conversion
            UInt64 result;
            if (ConvertToUInt64Impl(value, out result)) return result;

            // Fall back to __xxx__ method call
            //object newValue;
            //if (Ops.TryToInvoke(value, SymbolTable.ConvertToInt, out newValue)) {
            //    // Convert resulting object to the desired type
            //    if (ConvertToUInt64Impl(newValue, out result)) return result;
            //}
            //if (Ops.TryToInvoke(value, SymbolTable.ConvertToLong, out newValue)) {
            //    // Convert resulting object to the desired type
            //    if (ConvertToUInt64Impl(newValue, out result)) return result;
            //}

            throw CannotConvertTo("UInt64", value);
        }

        //
        // ConvertToSingle - fast paths and custom logic
        //
        public static Single ConvertToSingle(object value) {
            // Fast Paths
            if (value is Int32) return (Single)(Int32)value;
            if (value is Single) return (Single)value;

            // Fall back to comprehensive conversion
            Single result;
            if (ConvertToSingleImpl(value, out result)) return result;

            // Fall back to __xxx__ method call
            object newValue;
            if (Ops.TryToInvoke(value, SymbolTable.ConvertToFloat, out newValue)) {
                // Convert resulting object to the desired type
                if (ConvertToSingleImpl(newValue, out result)) return result;
            }

            throw CannotConvertTo("Single", value);
        }

        //
        // ConvertToDouble - fast paths and custom logic
        //
        public static Double ConvertToDouble(object value) {
            // Fast Paths
            ExtensibleInt ei;
            ExtensibleFloat ef;
            if (value is Double) return (Double)value;
            if (value is Int32) return (Double)(Int32)value;
            if ((ef = value as ExtensibleFloat) != null) return ef.value;
            if ((ei = value as ExtensibleInt) != null) return ei.value;

            // Fall back to comprehensive conversion
            Double result;
            if (ConvertToDoubleImpl(value, out result)) return result;

            // Fall back to __xxx__ method call
            object newValue;
            if (Ops.TryToInvoke(value, SymbolTable.ConvertToFloat, out newValue)) {
                // Convert resulting object to the desired type
                if (ConvertToDoubleImpl(newValue, out result)) return result;
            }

            throw CannotConvertTo("Double", value);
        }

        //
        // ConvertToDecimal - fast paths and custom logic
        //
        public static Decimal ConvertToDecimal(object value) {
            // Fast Paths
            if (value is Decimal) return (Decimal)value;
            if (value is Int32) return (Decimal)(Int32)value;

            // Fall back to comprehensive conversion
            Decimal result;
            if (ConvertToDecimalImpl(value, out result)) return result;

            // Fall back to __xxx__ method call
            //object newValue;
            //if (Ops.TryToInvoke(value, SymbolTable.ConvertToInt, out newValue)) {
            //    // Convert resulting object to the desired type
            //    if (ConvertToDecimalImpl(newValue, out result)) return result;
            //}
            //if (Ops.TryToInvoke(value, SymbolTable.ConvertToLong, out newValue)) {
            //    // Convert resulting object to the desired type
            //    if (ConvertToDecimalImpl(newValue, out result)) return result;
            //}
            //if (Ops.TryToInvoke(value, SymbolTable.ConvertToFloat, out newValue)) {
            //    // Convert resulting object to the desired type
            //    if (ConvertToDecimalImpl(newValue, out result)) return result;
            //}

            throw CannotConvertTo("Decimal", value);
        }

        //
        // ConvertToBigInteger - fast paths and custom logic
        //
        public static BigInteger ConvertToBigInteger(object value) {
            // Fast Paths
            BigInteger bi;
            ExtensibleLong el;

            if ((Object)(bi = value as BigInteger) != null) return bi;
            if (value is Int32) return BigInteger.Create((Int32)value);
            if ((el = value as ExtensibleLong) != null) return el.Value;

            // Fall back to comprehensive conversion
            BigInteger result;
            if (ConvertToBigIntegerImpl(value, out result)) return result;

            // Fall back to __xxx__ method call
            object newValue;
            if (Ops.TryToInvoke(value, SymbolTable.ConvertToLong, out newValue)) {
                // Convert resulting object to the desired type
                if (ConvertToBigIntegerImpl(newValue, out result)) return result;
            }

            throw CannotConvertTo("BigInteger", value);
        }

        //
        // ConvertToComplex64 - fast paths and custom logic
        //
        public static Complex64 ConvertToComplex64(object value) {
            // Fast Paths
            if (value is Complex64) return (Complex64)value;
            if (value is Double) return Complex64.MakeReal((Double)value);

            // Fall back to comprehensive conversion
            Complex64 result;
            if (ConvertToComplex64Impl(value, out result)) return result;

            // Fall back to __xxx__ method call
            object newValue;
            if (Ops.TryToInvoke(value, SymbolTable.ConvertToComplex, out newValue)) {
                // Convert resulting object to the desired type
                if (ConvertToComplex64Impl(newValue, out result)) return result;
            }

            throw CannotConvertTo("Complex64", value);
        }

        //
        // ConvertToString - fast paths and custom logic
        //
        public static String ConvertToString(object value) {
            // Fast Paths
            String result;
            ExtensibleString es;

            if ((result = value as String) != null) return result;
            if (value == null) return null;
            if (value is Char) return Ops.Char2String((Char)value);
            if ((es = value as ExtensibleString) != null) return es.Value;

            throw CannotConvertTo("String", value);
        }

        //
        // ConvertToChar - fast paths and custom logic
        //
        public static Char ConvertToChar(object value) {
            // Fast Paths
            string str;
            ExtensibleString es;

            if (value is Char) return (Char)value;
            if (!Object.Equals((str = value as string), null) && str.Length == 1) return str[0];
            if (!Object.Equals((es = value as ExtensibleString), null) && es.Value.Length == 1) return es.Value[0];

            throw CannotConvertTo("Char", value);
        }

        //
        // ConvertToBoolean - fast paths and custom logic
        //
        public static Boolean ConvertToBoolean(object value) {
            // Fast Paths
            if (value is Int32) return (Int32)value != 0;
            if (value is Boolean) return (Boolean)value;
            if (value == null) return false;

            Boolean result;

            // Fall back to comprehensive conversion
            if (ConvertToBooleanImpl(value, out result)) return result;

            // Additional logic to convert to bool
            if (value == null) return false;
            if (value is IPythonContainer) return ((IPythonContainer)value).GetLength() != 0;
            if (value is ICollection) return ((ICollection)value).Count != 0;

            // Fall back to __xxx__ method call
            object newValue;

            // First, try __nonzero__
            if (Ops.TryToInvoke(value, SymbolTable.NonZero, out newValue)) {
                // Convert resulting object to the desired type
                if (newValue is bool || newValue is Int32) {
                    if (ConvertToBooleanImpl(newValue, out result)) return result;
                }
                throw Ops.TypeError("__nonzero__ should return bool or int, returned {0}", Ops.GetClassName(newValue));
            }

            // Then, try __len__
            if (Ops.TryToInvoke(value, SymbolTable.Length, out newValue)) {
                // Convert resulting object to the desired type
                if (newValue is Int32 || newValue is BigInteger) {
                    if (ConvertToBooleanImpl(newValue, out result)) return result;
                }
                throw Ops.TypeError("an integer is required");
            }

            // Try Extensible types as last due to possible __nonzero__ overload
            if (value is ExtensibleInt) return (Int32)((ExtensibleInt)value).value != (Int32)0;
            if (value is ExtensibleLong) return ((ExtensibleLong)value).Value != BigInteger.Zero;
            if (value is ExtensibleFloat) return ((ExtensibleFloat)value).value != (Double)0;

            // Non-null value is true
            result = true;
            return true;
        }

        #endregion

        internal static Char ExplicitConvertToChar(object value) {
            string str;
            ExtensibleString es;
            if (value is Char) return (Char)value;
            if (value is Int32) return checked((Char)(Int32)value);
            if (!Object.Equals((str = value as string), null) && str.Length == 1) return str[0];
            if (!Object.Equals((es = value as ExtensibleString), null) && es.Value.Length == 1) return es.Value[0];
            if (value is SByte) return checked((Char)(SByte)value);
            if (value is Int16) return checked((Char)(Int16)value);
            if (value is UInt32) return checked((Char)(UInt32)value);
            if (value is UInt64) return checked((Char)(UInt64)value);
            if (value is Decimal) return checked((Char)(Decimal)value);
            if (value is Int64) return checked((Char)(Int64)value);
            if (value is Byte) return (Char)(Byte)value;
            if (value is UInt16) return checked((Char)(UInt16)value);

            throw CannotConvertTo("char", value);
        }

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
            if (IronPython.Hosting.PythonEngine.options.EngineDebug) {
                PerfTrack.NoteEvent(PerfTrack.Categories.Properties, String.Format("ConvertTo {0}, from {1}", to, Ops.GetDynamicType(value).__name__));
            }

            if (value == null) {
                if (to.IsValueType) throw MakeTypeError(to, value);
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

            if (to == IEnumeratorType) return ConvertToIEnumerator(value);
            if (DelegateType.IsAssignableFrom(to)) return ConvertToDelegate(value, to);

            if (to.IsArray) return ConvertToArray(value, to);

            // check for implicit conversions 
            ReflectedType tt = Ops.GetDynamicTypeFromType(to) as ReflectedType;
            ReflectedType dt = Ops.GetDynamicType(value) as ReflectedType;

            if (tt != null && dt != null) {
                object result;
                if (dt.TryConvertTo(value, tt, out result)) return result;
                if (tt.TryConvertFrom(value, out result)) return result;
            }

            if (to.IsGenericType) {
                Type genTo = to.GetGenericTypeDefinition();
                if (genTo == NullableOfT) return ConvertToNullableOfT(value, to.GetGenericArguments());
                if (genTo == IListOfTType) return ConvertToIListT(value, to.GetGenericArguments());
                if (genTo == IDictOfTType) return ConvertToIDictT(value, to.GetGenericArguments());
                if (genTo == IEnumeratorOfT) return ConvertToIEnumeratorT(value, to.GetGenericArguments());
            }

            if (to == BigIntegerType) return ConvertToBigInteger(value);
            if (to == Complex64Type) return ConvertToComplex64(value);
            if (to == ByteType) return ConvertToByte(value);
            if (to == SByteType) return ConvertToSByte(value);
            if (to == Int16Type) return ConvertToInt16(value);
            if (to == UInt32Type) return ConvertToUInt32(value);
            if (to == UInt64Type) return ConvertToUInt64(value);
            if (to == UInt16Type) return ConvertToUInt16(value);
            if (to == SingleType) return ConvertToSingle(value);
            if (to == DecimalType) return ConvertToDecimal(value);
            if (to == Int64Type) return ConvertToInt64(value);
            //if (to == ArrayListType) return ConvertToArrayList(value);
            //if (to == HashtableType) return ConvertToHashtable(value);

            if (from.IsValueType) {
                // !!! keep the IsEnum ???
                if (to.IsEnum) {
                    if (value is int) {
                        int IntValue = (int)value;
                        if (IntValue == 0) {
                            return 0;
                        }
                    }
                }
                if (to == ValueTypeType) {
                    return (System.ValueType)value;
                }
            }

            throw MakeTypeError(to, value);
        }


        public static Type ConvertToType(object value) {
            if (value == null) return null;

            Type TypeVal = value as Type;
            if (TypeVal != null) return TypeVal;

            DynamicType DynamicTypeVal = value as DynamicType;
            if (DynamicTypeVal != null) return DynamicTypeVal.type;

            throw MakeTypeError("Type", value);
        }

        public static IEnumerator ConvertToIEnumerator(object o) {
            IEnumerator ie = o as IEnumerator;
            if (ie != null) return ie;
            if (o == null) return null;
            return Ops.GetEnumerator(o);
        }

        private static object ConvertToArray(object value, Type to) {
            int rank = to.GetArrayRank();

            if (rank == 1) {
                Tuple tupleVal = value as Tuple;
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
                    }
                }
            }

            throw MakeTypeError("Array", value);
        }

        private static object ConvertToIDictOfT(object value, Type[] dictOf) {
            Dict dv = value as Dict;
            if (dv != null) {
                if (((dictOf[0] != typeof(object) || dictOf[1] != typeof(object)) &&
                  (dictOf[0] != typeof(SymbolId) && dictOf[1] != typeof(object)))) {
                    Type t = DictWrapperForIDictType.MakeGenericType(dictOf);
                    return Activator.CreateInstance(t, dv);
                } else {
                    // our dicts can be both of these, don't do a wrapping...
                    return dv;
                }
            }

            throw MakeTypeError("dict", value);
        }

        private static object TryConvertFromNull(Type to) {
            if (!to.IsValueType) {
                return null;
            }

            // value type, if it's nullable, we convert, if not
            // we have no conversion.
            if (to.IsGenericType && to.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                return Activator.CreateInstance(to, new object[0]);
            }
            return null;
        }
        private static bool ExtendList(IEnumerable ie, IList list) {
            try {
                foreach (object o in ie) {
                    list.Add(o);
                }
                return true;
            } catch (ArgumentException) {
                // conversion failed
                return false;
            }
        }

        private static object ConvertToListOfT(object value, Type[] listOf) {
            // create a new List<T> and return it            
            Tuple t;
            if ((t = value as Tuple) != null) {
                Type type = ListOfTType.MakeGenericType(listOf);
                IList list = Activator.CreateInstance(type) as IList;

                if (ExtendList(t, list)) {
                    return list;
                }
            }

            // no conversion from List to List<T> as we won't see any updates.
            throw MakeTypeError("list", value);
        }

        private static Hashtable ConvertToHashtable(object value) {

            Debug.Fail("Should I ever show up here, in ConvertToHashtable??");

            Hashtable result = value as Hashtable;
            if (result != null) {
                return result;
            }

            //Dict d = value as Dict;
            //if (d != null) {
            //    return new DictWrapperForHashtableDictionary(d);
            //}

            throw MakeTypeError("Hashtable", value);
        }

        //private static ArrayList ConvertToArrayList(object value) {
        //    ArrayList result = value as ArrayList;
        //    if (result != null) return result;

        //    Tuple t = value as Tuple;
        //    if (t != null) {
        //        result = new ArrayList(t.Count);
        //        for (int i = 0; i < t.Count; i++) {
        //            result.Add(t[i]);
        //        }
        //        return result;
        //    } else {
        //        List list = value as List;
        //        if (list != null) {
        //            return new ListWrapperForArrayListCollection(list);
        //        }
        //    }

        //    throw MakeTypeError("ArrayList", value);
        //}

        public static object ConvertToIEnumeratorT(object value, Type[] enumOf) {
            Type type = IEnumeratorOfT.MakeGenericType(enumOf);
            if (type.IsInstanceOfType(value)) {
                return value;
            }

            IEnumerator ie = value as IEnumerator;
            if (ie == null) {
                ie = Ops.GetEnumerator(value);
            }

            type = IEnumeratorOfTWrapper.MakeGenericType(enumOf);
            object res = Activator.CreateInstance(type, ie);
            return res;
        }

        public static T Convert<T>(object value) {
            return (T)Convert(value, typeof(T));
        }

        public static object ConvertToDelegate(object value, Type to) {
            Debug.Assert(typeof(Delegate).IsAssignableFrom(to));
            if (value == null) return null;

            Type inType = value.GetType();

            if (to.IsAssignableFrom(inType)) return value;

            object deleg = Ops.GetDelegate(value, to);
            if (deleg != null) return deleg;

            throw Ops.TypeErrorForBadInstance("expected compatible function, found {0}", value);
        }

        public static int ConvertToSliceIndex(object value) {
            int val;
            if (TryConvertToInt32(value, out val))
                return val;

            BigInteger bigval;
            if (TryConvertToBigInteger(value, out bigval)) {
                return bigval > 0 ? Int32.MaxValue : Int32.MinValue;
            }

            throw Ops.TypeError("slice indices must be integers");
        }

        public static int ConvertToXRangeIndex(object value) {
            int val;
            if (TryConvertToInt32(value, out val)) return val;

            IronMath.BigInteger bigval;
            if (TryConvertToBigInteger(value, out bigval)) {
                if (bigval <= Int32.MinValue)
                    throw Ops.OverflowError("xrange() result has too many items");
                if (bigval > Int32.MaxValue)
                    throw Ops.OverflowError("long int too large to convert to int");

                return bigval.ToInt32();
            }

            double dblval;
            if (TryConvertToDouble(value, out dblval))
                throw Ops.OverflowError("long int too large to convert to int");
            else
                throw Ops.TypeError("an integer is required");
        }

        private static Exception CannotConvertTo(string name, object value) {
            return Ops.TypeError("Cannot convert {0}({1}) to {2}", Ops.GetDynamicType(value).__name__, value, name);
        }

        public static Exception MakeTypeError(Type expectedType, object o) {
            return MakeTypeError(Ops.GetDynamicTypeFromType(expectedType).__name__.ToString(), o);
        }

        public static Exception MakeTypeError(string expectedType, object o) {
            return Ops.TypeErrorForTypeMismatch(expectedType, o);
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
        private static readonly Type BigIntegerType = typeof(IronMath.BigInteger);
        private static readonly Type Complex64Type = typeof(IronMath.Complex64);
        private static readonly Type DelegateType = typeof(Delegate);
        private static readonly Type IEnumeratorType = typeof(IEnumerator);
        private static readonly Type ValueTypeType = typeof(ValueType);
        private static readonly Type TypeType = typeof(Type);
        private static readonly Type ArrayListType = typeof(ArrayList);
        private static readonly Type NullableOfT = typeof(Nullable<int>).GetGenericTypeDefinition();
        private static readonly Type IListOfTType = typeof(System.Collections.Generic.IList<int>).GetGenericTypeDefinition();
        private static readonly Type ListOfTType = typeof(System.Collections.Generic.List<int>).GetGenericTypeDefinition();
        private static readonly Type IDictOfTType = typeof(System.Collections.Generic.IDictionary<int, int>).GetGenericTypeDefinition();
        private static readonly Type HashtableType = typeof(Hashtable);
        private static readonly Type ListWrapperForIListType = typeof(ListWrapperForIList<int>).GetGenericTypeDefinition();
        private static readonly Type IEnumeratorOfT = typeof(System.Collections.Generic.IEnumerator<int>).GetGenericTypeDefinition();
        private static readonly Type IEnumeratorOfTWrapper = typeof(IEnumeratorOfTWrapper<int>).GetGenericTypeDefinition();
        private static readonly Type DictWrapperForIDictType = typeof(DictWrapperForIDict<int, int>).GetGenericTypeDefinition();
        private static readonly Type IListOfObjectType = typeof(System.Collections.Generic.IList<object>);
        private static readonly Type IDictionaryOfObjectType = typeof(System.Collections.Generic.IDictionary<object, object>);
        private static readonly Type ListGenericWrapperType = typeof(ListGenericWrapper<object>).GetGenericTypeDefinition();
        private static readonly Type DictionaryGenericWrapperType = typeof(DictionaryGenericWrapper<object, object>).GetGenericTypeDefinition();

        #endregion

        #region Implementation routines

        //
        //  ConvertToBigIntegerImpl Conversion Routine
        //

        private static bool ConvertToBigIntegerImpl(object value, out BigInteger result) {
            if (value is BigInteger) {
                result = (BigInteger)value;
                return true;
            } else if (value is Int32) {
                result = (BigInteger)(Int32)value;
                return true;
            //} else if (value is Double) {
            //    // DEPRECATED IMPLICIT CONVERSION FROM FLOAT TO INT
            //    result = BigInteger.Create((Double)value);
            //    return true;
            } else if (value is Boolean) {
                result = (Boolean)value ? BigInteger.One : BigInteger.Zero;
                return true;
            } else if (value is ExtensibleInt) {
                result = (BigInteger)((ExtensibleInt)value).value;
                return true;
            } else if (value is ExtensibleLong) {
                result = ((ExtensibleLong)value).Value;
                return true;
            //} else if (value is ExtensibleFloat) {
            //    // DEPRECATED IMPLICIT CONVERSION FROM FLOAT TO INT
            //    result = BigInteger.Create(((ExtensibleFloat)value).value);
            //    return true;
            } else if (value is Int64) {
                result = (BigInteger)(Int64)value;
                return true;
            } else if (value is Byte) {
                result = (BigInteger)(Byte)value;
                return true;
            } else if (value is SByte) {
                result = (BigInteger)(SByte)value;
                return true;
            } else if (value is Int16) {
                result = (BigInteger)(Int16)value;
                return true;
            } else if (value is UInt16) {
                result = (BigInteger)(UInt16)value;
                return true;
            } else if (value is UInt32) {
                result = (BigInteger)(UInt32)value;
                return true;
            } else if (value is UInt64) {
                result = (BigInteger)(UInt64)value;
                return true;
            } else if (value is Decimal) {
                result = BigInteger.Create((Decimal)value);
                return true;
            }
            result = null;
            return false;
        }

        //
        //  ConvertToBooleanImpl Conversion Routine
        //
        private static bool ConvertToBooleanImpl(object value, out Boolean result) {
            if (value is Boolean) {
                result = (Boolean)value;
                return true;
            } else if (value is Int32) {
                result = (Int32)value != (Int32)0;
                return true;
            } else if (value is Double) {
                result = (Double)value != (Double)0;
                return true;
            } else if (value is BigInteger) {
                result = ((BigInteger)value) != BigInteger.Zero;
                return true;
            } else if (value is String) {
                result = ((String)value).Length != 0;
                return true;
            } else if (value is Complex64) {
                result = !((Complex64)value).IsZero;
                return true;
            } else if (value is Int64) {
                result = (Int64)value != (Int64)0;
                return true;
            } else if (value is Byte) {
                result = (Byte)value != (Byte)0;
                return true;
            } else if (value is SByte) {
                result = (SByte)value != (SByte)0;
                return true;
            } else if (value is Int16) {
                result = (Int16)value != (Int16)0;
                return true;
            } else if (value is UInt16) {
                result = (UInt16)value != (UInt16)0;
                return true;
            } else if (value is UInt32) {
                result = (UInt32)value != (UInt32)0;
                return true;
            } else if (value is UInt64) {
                result = (UInt64)value != (UInt64)0;
                return true;
            } else if (value is Single) {
                result = (Single)value != (Single)0;
                return true;
            } else if (value is Decimal) {
                result = (Decimal)value != (Decimal)0;
                return true;
            } else if (value is Enum) {
                return TryConvertEnumToBoolean(value, out result);
            }

            result = default(Boolean);
            return false;
        }

        private static bool TryConvertEnumToBoolean(object value, out bool result) {
            switch (((Enum)value).GetTypeCode()) {
                case TypeCode.Int32:
                    result = (int)value != 0; return true;
                case TypeCode.Int64:
                    result = (long)value != 0; return true;
                case TypeCode.Int16:
                    result = (short)value != 0; return true;
                case TypeCode.UInt32:
                    result = (uint)value != 0; return true;
                case TypeCode.UInt64:
                    result = (ulong)value != 0; return true;
                case TypeCode.SByte:
                    result = (sbyte)value != 0; return true;
                case TypeCode.UInt16:
                    result = (ushort)value != 0; return true;
                case TypeCode.Byte:
                    result = (byte)value != 0; return true;
                default:
                    result = default(Boolean); return false;
            }
        }

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
            } else if (value is ExtensibleComplex) {
                result = ((ExtensibleComplex)value).value;
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

        private static object ConvertToNullableOfT(object value, Type[] typeOf) {
            if (value == null) return null; 
            else return Convert(value, typeOf[0]);
        }

        public static object ConvertToReferenceType(object fromObject, RuntimeTypeHandle typeHandle) {
            if (fromObject == null) return null;
            return Convert(fromObject, Type.GetTypeFromHandle(typeHandle));
        }

        public static object ConvertToNullableType(object fromObject, RuntimeTypeHandle typeHandle) {
            if (fromObject == null) return null;
            return Convert(fromObject, Type.GetTypeFromHandle(typeHandle));
        }

        public static object ConvertToValueType(object fromObject, RuntimeTypeHandle typeHandle) {
            if (fromObject == null) throw Ops.InvalidType(fromObject, typeHandle);
            return Convert(fromObject, Type.GetTypeFromHandle(typeHandle));
        }

        public static bool ConvertToIEnumeratorT(object value, Type[] enumOf, out object result) {
            Type type = IEnumeratorOfT.MakeGenericType(enumOf);
            if (type.IsInstanceOfType(value)) { result = value; return true; }

            IEnumerator ie = value as IEnumerator;
            if (ie == null) {
                ie = Ops.GetEnumerator(value);
            }

            type = IEnumeratorOfTWrapper.MakeGenericType(enumOf);
            result = Activator.CreateInstance(type, ie);
            return true;
        }

        public static object ConvertToIListT(object value, Type[] listOf) {
            System.Collections.Generic.IList<object> lst = value as System.Collections.Generic.IList<object>;
            if (lst != null) {
                Type t = ListGenericWrapperType.MakeGenericType(listOf);
                return Activator.CreateInstance(t, lst);
            }
            throw MakeTypeError("IList<T>", value);
        }

        public static object ConvertToIDictT(object value, Type[] dictOf) {
            System.Collections.Generic.IDictionary<object, object> dict = value as System.Collections.Generic.IDictionary<object, object>;
            if (dict != null) {
                Type t = DictionaryGenericWrapperType.MakeGenericType(dictOf);
                return Activator.CreateInstance(t, dict);
            }
            throw MakeTypeError("IDictionary<K,V>", value);
        }

        public static bool CanConvertFrom(Type fromType, Type toType, NarrowingLevel allowNarrowing) {
            if (toType == fromType) return true;
            if (toType.IsAssignableFrom(fromType)) return true;
            if (fromType.IsCOMObject && toType.IsInterface) return true; // A COM object could be cast to any interface

            if (HasImplicitNumericConversion(fromType, toType)) return true;

            // Handling the hole that Type is the only object that we 'box'
            if (toType == typeof(Type) && typeof(DynamicType).IsAssignableFrom(fromType)) return true;

            // Support extensible types with simple implicit conversions to their base types
            if (typeof(ExtensibleInt).IsAssignableFrom(fromType) && CanConvertFrom(typeof(int), toType, allowNarrowing)) {
                return true;
            }
            if (typeof(ExtensibleLong).IsAssignableFrom(fromType) && CanConvertFrom(typeof(IronMath.BigInteger), toType, allowNarrowing)) {
                return true;
            }
            if (typeof(ExtensibleString).IsAssignableFrom(fromType) && CanConvertFrom(typeof(string), toType, allowNarrowing)) {
                return true;
            }
            if (typeof(ExtensibleFloat).IsAssignableFrom(fromType) && CanConvertFrom(typeof(float), toType, allowNarrowing)) {
                return true;
            }
            if (typeof(ExtensibleComplex).IsAssignableFrom(fromType) && CanConvertFrom(typeof(IronMath.Complex64), toType, allowNarrowing)) {
                return true;
            }

            //!!!do user-defined implicit conversions here

            if (allowNarrowing == NarrowingLevel.None) return false;

            return HasNarrowingConversion(fromType, toType, allowNarrowing);
        }

        public static bool HasImplicitNumericConversion(Type fromType, Type toType) {
            if (fromType.IsEnum) return false;

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
                            if (toType == typeof(IronMath.BigInteger)) return true;
                            if (toType == typeof(IronMath.Complex64)) return true;
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
                            if (toType == typeof(IronMath.BigInteger)) return true;
                            if (toType == typeof(IronMath.Complex64)) return true;
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
                            if (toType == typeof(IronMath.BigInteger)) return true;
                            if (toType == typeof(IronMath.Complex64)) return true;
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
                            if (toType == typeof(IronMath.BigInteger)) return true;
                            if (toType == typeof(IronMath.Complex64)) return true;
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
                            if (toType == typeof(IronMath.BigInteger)) return true;
                            if (toType == typeof(IronMath.Complex64)) return true;
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
                            if (toType == typeof(IronMath.BigInteger)) return true;
                            if (toType == typeof(IronMath.Complex64)) return true;
                            return false;
                    }
                case TypeCode.Int64:
                    switch (Type.GetTypeCode(toType)) {
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                        default:
                            if (toType == typeof(IronMath.BigInteger)) return true;
                            if (toType == typeof(IronMath.Complex64)) return true;
                            return false;
                    }
                case TypeCode.UInt64:
                    switch (Type.GetTypeCode(toType)) {
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                        default:
                            if (toType == typeof(IronMath.BigInteger)) return true;
                            if (toType == typeof(IronMath.Complex64)) return true;
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
                            if (toType == typeof(IronMath.BigInteger)) return true;
                            if (toType == typeof(IronMath.Complex64)) return true;
                            return false;
                    }
                case TypeCode.Single:
                    switch (Type.GetTypeCode(toType)) {
                        case TypeCode.Double:
                            return true;
                        default:
                            if (toType == typeof(IronMath.Complex64)) return true;
                            return false;
                    }
                case TypeCode.Double:
                    switch (Type.GetTypeCode(toType)) {
                        default:
                            if (toType == typeof(IronMath.Complex64)) return true;
                            return false;
                    }
                default:
                    return false;
            }
        }

        public static bool PreferConvert(Type t1, Type t2) {
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

        public static bool HasNarrowingConversion(Type fromType, Type toType, NarrowingLevel allowNarrowing) {
            if (allowNarrowing == NarrowingLevel.All) {
                if (IsNumeric(fromType) && IsNumeric(toType)) return true;

                if (toType.IsArray) {
                    return typeof(Tuple).IsAssignableFrom(fromType);
                }

                if (toType == typeof(char) && fromType == typeof(string)) return true;
                if (toType == typeof(int) && fromType == typeof(bool)) return true;

                if (toType == typeof(bool) && IsPythonType(fromType)) return true;
                if (typeof(Delegate).IsAssignableFrom(toType) && IsPythonType(fromType)) return true;
                if (typeof(System.Collections.IEnumerator) == toType && IsPythonType(fromType)) return true;

                //__int__, __float__, __long__
                if (toType == typeof(int) && HasPythonProtocol(fromType, SymbolTable.ConvertToInt)) return true;
                if (toType == typeof(double) && HasPythonProtocol(fromType, SymbolTable.ConvertToFloat)) return true;
                if (toType == typeof(BigInteger) && HasPythonProtocol(fromType, SymbolTable.ConvertToLong)) return true;
            }

            if (toType.IsGenericType) {
                Type genTo = toType.GetGenericTypeDefinition();
                if (genTo == typeof(System.Collections.Generic.IList<>)) {
                    return IListOfObjectType.IsAssignableFrom(fromType);
                    //return typeof(Tuple).IsAssignableFrom(fromType) || typeof(List).IsAssignableFrom(fromType);
                } else if (genTo == typeof(System.Collections.Generic.IEnumerator<>)) {
                    if (IsPythonType(fromType)) return true;
                    //return typeof(Tuple).IsAssignableFrom(fromType) || typeof(List).IsAssignableFrom(fromType);
                } else if (genTo == typeof(System.Collections.Generic.IDictionary<,>)) {
                    return IDictionaryOfObjectType.IsAssignableFrom(fromType);
                }
            }

            if (fromType == typeof(IronMath.BigInteger) && toType == typeof(long)) return true;

            return false;
        }


        public static bool IsNumeric(Type t) {
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
                    return t == typeof(IronMath.BigInteger) || t == typeof(IronMath.Complex64);
                default:
                    return true;
            }
        }

        public static bool IsPythonType(Type t) {
            return t.FullName.StartsWith("IronPython."); //!!! this and the check below are hacks
        }

        public static bool HasPythonProtocol(Type t, SymbolId name) {
            if (t.FullName.StartsWith("IronPython.NewTypes.")) return true;
            if (t == typeof(OldInstance)) return true;
            ICustomAttributes dt = Ops.GetDynamicTypeFromType(t) as ICustomAttributes;
            if (dt == null) return false;
            object tmp;
            return dt.TryGetAttr(null, name, out tmp);
        }
    }
}
