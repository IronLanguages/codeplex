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

namespace IronPython.Runtime {

    public enum Conversion {
        Identity,       // Identical conversion
        Implicit,       // Implicit conversion available
        NonStandard,    // Non-standard (Python specific) conversion available
        Eval,           // Takes a method call to convert
        Truncation,     // Conversion that looses precision
        None,           // No conversion possible
    }

    public static partial class Converter {

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
        private static readonly Type IListOfTType = typeof(System.Collections.Generic.IList<int>).GetGenericTypeDefinition();
        private static readonly Type ListOfTType = typeof(System.Collections.Generic.List<int>).GetGenericTypeDefinition();
        private static readonly Type IDictOfTType = typeof(System.Collections.Generic.IDictionary<int, int>).GetGenericTypeDefinition();
        private static readonly Type HashtableType = typeof(Hashtable);
        private static readonly Type ListWrapperForIListType = typeof(ListWrapperForIList<int>).GetGenericTypeDefinition();
        private static readonly Type IEnumeratorOfT = typeof(System.Collections.Generic.IEnumerator<int>).GetGenericTypeDefinition();
        private static readonly Type IEnumeratorOfTWrapper = typeof(IEnumeratorOfTWrapper<int>).GetGenericTypeDefinition();
        private static readonly Type DictWrapperForIDictType = typeof(DictWrapperForIDict<int, int>).GetGenericTypeDefinition();

        public static object TryConvert(object value, Type to, out Conversion conversion) {
            if (value == null) return TryConvertFromNull(to, out conversion);

            return TryConvertWorker(value, to, out conversion);
        }

        private static object TryConvertToArray(object value, Type to, out Conversion conversion) {
            int rank = to.GetArrayRank();

            if (rank == 1) {
                Tuple tupleVal = value as Tuple;
                List listVal;
                IEnumerator ie;
                if (tupleVal != null) {
                    Array res = Activator.CreateInstance(to, tupleVal.Count) as Array;
                    try {
                        tupleVal.CopyTo(res, 0);

                        conversion = Conversion.NonStandard;
                        return res;
                    } catch (InvalidCastException) {
                        // invalid conversion
                    }
                } else if ((listVal = value as List) != null) {
                    Array res = Activator.CreateInstance(to, listVal.Count) as Array;
                    try {

                        listVal.CopyTo(res, 0);

                        conversion = Conversion.NonStandard;
                        return res;
                    } catch (InvalidCastException) {
                        // invalid conversion
                    }
                } else if (Ops.TryGetEnumerator(value, out ie)) {
                    List vals = new List();
                    while (ie.MoveNext()) {
                        vals.AddNoLock(ie.Current);
                    }

                    // recurse back to the List version.
                    return TryConvertToArray(vals, to, out conversion);
                }
            }
            conversion = Conversion.None;
            return (object[])null;
        }
        private static object TryConvertToIListT(object value, Type[] listOf, out Conversion conversion) {
            List l;
            if (value is Tuple) {
                return TryConvertToListOfT(value, listOf, out conversion);
            } else if ((l = value as List) != null) {
                Type t = ListWrapperForIListType.MakeGenericType(listOf);
                object res = Activator.CreateInstance(t, l);
                // Implicit so we always take precedence over List<T> which isn't as good (and reported as NonStandard)
                conversion = Conversion.Implicit;
                return res;
            }

            conversion = Conversion.None;
            return null;
        }

        private static object TryConvertToIDictOfT(object value, Type[] dictOf, out Conversion conversion) {
            Dict dv = value as Dict;
            if (dv != null) {
                if (((dictOf[0] != typeof(object) || dictOf[1] != typeof(object)) &&
                  (dictOf[0] != typeof(SymbolId) && dictOf[1] != typeof(object)))) {
                    Type t = DictWrapperForIDictType.MakeGenericType(dictOf);
                    object res = Activator.CreateInstance(t, dv);
                    conversion = Conversion.Implicit;
                    return res;
                } else {
                    // our dicts can be both of these, don't do a wrapping...
                    conversion = Conversion.Identity;
                    return dv;
                }
            }
            conversion = Conversion.None;
            return null;
        }

        private static object TryConvertFromNull(Type to, out Conversion conversion) {
            if (!to.IsValueType) {
                conversion = Conversion.Implicit;
                return null;
            }

            // value type, if it's nullable, we convert, if not
            // we have no conversion.
            if (to.IsGenericType && to.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                conversion = Conversion.Implicit;
                return Activator.CreateInstance(to, new object[0]);
            }
            conversion = Conversion.None;
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

        private static object TryConvertToListOfT(object value, Type[] listOf, out Conversion conversion) {
            // create a new List<T> and return it            
            Tuple t;
            if ((t = value as Tuple) != null) {
                Type type = ListOfTType.MakeGenericType(listOf);
                IList list = Activator.CreateInstance(type) as IList;

                if (ExtendList(t, list)) {
                    conversion = Conversion.NonStandard;
                    return list;
                }
            }
            // no conversion from List to List<T> as we won't see any updates.

            conversion = Conversion.None;
            return null;
        }

        private static Hashtable TryConvertToHashtable(object value, out Conversion conversion) {
            Hashtable ht = value as Hashtable;
            if (ht != null) {
                conversion = Conversion.Identity;
                return ht;
            }

            Dict d = value as Dict;
            if (d != null) {
                conversion = Conversion.Implicit;
                return new DictWrapperForHashtableDictionary(d);
            }

            conversion = Conversion.None;
            return null;
        }

        private static ArrayList TryConvertToArrayList(object value, out Conversion conversion) {
            ArrayList arrayList = value as ArrayList;
            if (arrayList != null) {
                conversion = Conversion.Identity;
                return arrayList;
            }

            Tuple t = value as Tuple;
            if (t != null) {
                ArrayList al = new ArrayList(t.Count);
                for (int i = 0; i < t.Count; i++) {
                    al.Add(t[i]);
                }
                conversion = Conversion.NonStandard;
                return al;
            } else {
                List list = value as List;
                if (list != null) {
                    ArrayList res = new ListWrapperForArrayListCollection(list);
                    // Implicit instead of NonStandard so we take precedence over forms that don't update the real list
                    conversion = Conversion.Implicit;
                    return res;
                }
            }

            conversion = Conversion.None;
            return null;
        }

        private static object TryConvertToIEnumeratorOfT(object value, Type[] enumOf, out Conversion conversion) {
            Type type = IEnumeratorOfT.MakeGenericType(enumOf);
            if (type.IsInstanceOfType(value)) {
                conversion = Conversion.Identity;
                return value;
            }

            IEnumerator ie = value as IEnumerator;

            if (ie == null) {
                if (!Ops.TryGetEnumerator(value, out ie)) {
                    conversion = Conversion.None;
                    return null;
                }
                conversion = Conversion.Eval;
            } else {
                conversion = Conversion.NonStandard;
            }

            type = IEnumeratorOfTWrapper.MakeGenericType(enumOf);
            object res = Activator.CreateInstance(type, ie);
            return res;
        }

        public static T Convert<T>(object value) {
            return (T)Convert(value, typeof(T));
        }


        public static object ConvertToDelegate(object value, Type to) {
            Conversion conv;
            object res = TryConvertToDelegate(value, to, out conv);
            if (conv != Conversion.None) return res;

            throw Ops.TypeError("expected compatible function, found ", Ops.GetDynamicType(value).__name__);
        }

        public static object TryConvertToDelegate(object value, Type to, out Conversion conversion) {
            if (value == null) {
                conversion = Conversion.Implicit;
                return null;
            }

            Type inType = value.GetType();

            if (to.IsAssignableFrom(inType)) {
                conversion = Conversion.Implicit;
                return value;
            }

            object res = Ops.GetDelegate(value, to);
            if (res != null) {
                conversion = Conversion.Eval;
            } else {
                conversion = Conversion.None;
            }
            return res;
        }

        public static int ConvertToSliceIndex(object value) {
            Conversion conversion;

            int val = TryConvertToInt32(value, out conversion);
            if (conversion != Conversion.None)
                return val;

            BigInteger bigval = TryConvertToBigInteger(value, out conversion);
            if (conversion != Conversion.None) {
                return bigval > 0 ? Int32.MaxValue : Int32.MinValue;
            }

            throw Ops.TypeError("slice indices must be integers");
        }

        public static int ConvertToXRangeIndex(object value) {
            Conversion conversion;

            int val = Converter.TryConvertToInt32(value, out conversion);
            if (conversion != Conversion.None)
                return val;

            IronMath.BigInteger bigval = Converter.TryConvertToBigInteger(value, out conversion);
            if (conversion != Conversion.None) {
                if (bigval <= Int32.MinValue)
                    throw Ops.OverflowError("xrange() result has too many items");
                if (bigval > Int32.MaxValue)
                    throw Ops.OverflowError("long int too large to convert to int");

                return bigval.ToInt32();
            }

            Converter.TryConvertToDouble(value, out conversion);
            if (conversion != Conversion.None)
                throw Ops.OverflowError("long int too large to convert to int");
            else
                throw Ops.TypeError("an integer is required");
        }
    }
}