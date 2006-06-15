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


    public enum NarrowingLevel {
        None,
        Preferred,
        All
    }

    public partial class NewConverter {
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

        private static readonly Type ListGenericWrapperType = typeof(ListGenericWrapper<object>).GetGenericTypeDefinition();
        private static readonly Type DictionaryGenericWrapperType = typeof(DictionaryGenericWrapper<object, object>).GetGenericTypeDefinition();
        private static readonly Type IListOfObjectType = typeof(System.Collections.Generic.IList<object>);
        private static readonly Type IDictionaryOfObjectType = typeof(System.Collections.Generic.IDictionary<object, object>);



        //!!! These don't work due to an apparent issue in Reflection.Emit
        //public static T ConvertTo<T>(object fromObject) where T : class {
        //    if (fromObject == null) return null;
        //    T ret = fromObject as T;
        //    if (ret != null) return ret;

        //    return (T)Converter.Convert(fromObject, typeof(T));
        //}

        //public static T ConvertToValue<T>(object fromObject) where T : struct {
        //    if (fromObject is T) return (T)fromObject;
        //    return (T)Converter.Convert(fromObject, typeof(T));
        //}

        //public static Nullable<T> ConvertToNullable<T>(object fromObject) where T : struct {
        //    if (fromObject == null) return new Nullable<T>();
        //    if (fromObject is Nullable<T>) return (Nullable<T>)fromObject;
        //    return (Nullable<T>)Converter.Convert(fromObject, typeof(Nullable<T>));
        //}

        public static object ConvertToReferenceType(object fromObject, RuntimeTypeHandle typeHandle) {
            if (fromObject == null) return null;
            return Convert(fromObject, Type.GetTypeFromHandle(typeHandle));
        }
        public static object ConvertToNullableType(object fromObject, RuntimeTypeHandle typeHandle) {
            //!!! currently identical to ConvertToReferenceType
            if (fromObject == null) return null;
            return Convert(fromObject, Type.GetTypeFromHandle(typeHandle));
        }
        public static object ConvertToValueType(object fromObject, RuntimeTypeHandle typeHandle) {
            if (fromObject == null) throw Ops.InvalidType(fromObject, typeHandle);
            return Convert(fromObject, Type.GetTypeFromHandle(typeHandle));
        }

        public static Int32 ConvertToInt32(object o) {
            if (o is Int32) return (Int32)o;
            ExtensibleInt ei = o as ExtensibleInt;
            if (ei != null) return ei.value;

            if (o is bool) return ((bool)o) ? 1 : 0;

            if (o == null) throw MakeTypeError("int", o);

            BigInteger bi = o as BigInteger;
            if (!object.ReferenceEquals(bi, null)) return bi.ToInt32();

            // slow path is now okay (sortof)
            return (int)CheckedOldConversion(o, Int32Type);
        }

        public static Double ConvertToDouble(object o) {
            if (o is Double) return (Double)o;
            if (o is Int32) return (Double)((Int32)o);
            
            ExtensibleFloat ef = o as ExtensibleFloat;
            if (ef != null) return ef.value;
            ExtensibleInt ei = o as ExtensibleInt;
            if (ei != null) return (Double)ei.value;

            if (o is bool) return ((bool)o) ? 1 : 0;

            if (o == null) throw MakeTypeError("float", o);
            // slow path is now okay (sortof)
            return (double)CheckedOldConversion(o, DoubleType);
        }

        public static string ConvertToString(object o) {
            string r = o as string;
            if (r != null) return r;
            if (o == null) return null;
            if (o is char) {
                return Ops.Char2String((char)o);
            }
            ExtensibleString es = o as ExtensibleString;
            if (es != null) return es.Value;

            throw MakeTypeError("str", o);
        }

        public static Type ConvertToType(object o) {
            if (o == null) return null;
            Type r = o as Type;
            if (r != null) return r;
            PythonType pt = o as PythonType;
            if (pt != null) return pt.type;

            throw MakeTypeError("type", o);
        }

        public static Char ConvertToChar(object o) {
            string s = o as string;
            if (s != null) {
                if (s.Length == 1) return s[0];
                throw Ops.TypeError("expected string of length 1, but string of length {0} found", s.Length);
            }
            if (o is char) return (char)o;

            ExtensibleString es = o as ExtensibleString;
            if (es != null) return ConvertToChar(es.Value);

            throw MakeTypeError("string of length 1", o);
        }

        public static Boolean ConvertToBoolean(object o) {
            // a few fast paths
            if (o is int) return ((int)o) != 0;
            if (o is bool) return (bool)o;
            if (o == null) return false;
            return Ops.IsTrue(o);
        }

        public static object ConvertToDelegate(object o, Type t) {
            return Converter.ConvertToDelegate(o, t);
        }

        public static IEnumerator ConvertToIEnumerator(object o) {
            IEnumerator ie = o as IEnumerator;
            if (ie != null) return ie;
            if (o == null) return null;
            return Ops.GetEnumerator(o);
        }

        public static object ConvertToArray(object value, Type to) {
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
            throw MakeTypeError(to, value);
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

        public static Exception MakeTypeError(Type expectedType, object o) {
            return MakeTypeError(Ops.GetDynamicTypeFromType(expectedType).__name__.ToString(), o);
        }

        public static Exception MakeTypeError(string expected, object o) {
            return Ops.TypeError("expected {0}, found {1}", expected, Ops.GetDynamicType(o).__name__);
        }

        public static object Convert(object value, Type toType) {
            if (IronPython.Hosting.PythonEngine.options.EngineDebug) {
                PerfTrack.NoteEvent(PerfTrack.Categories.Properties, 
                    string.Format("ConvertTo {0}, from {1}", toType, Ops.GetDynamicType(value).__name__));
            }


            if (value == null) {
                if (toType.IsValueType) throw MakeTypeError(toType, value);
                return null;
            }
            Type fromType = value.GetType();
            if (fromType == toType) return value;
            if (toType.IsInstanceOfType(value)) return value;

            if (toType == TypeType) return ConvertToType(value);

            if (toType == Int32Type) return ConvertToInt32(value);
            if (toType == DoubleType) return ConvertToDouble(value);
            if (toType == BooleanType) return ConvertToBoolean(value);

            if (toType == CharType) return ConvertToChar(value);
            if (toType == StringType) return ConvertToString(value);

            if (toType == IEnumeratorType) return ConvertToIEnumerator(value); 
            if (DelegateType.IsAssignableFrom(toType)) return ConvertToDelegate(value, toType);

            if (toType.IsArray) return ConvertToArray(value, toType);

            // check for implicit conversions 
            ReflectedType tt = Ops.GetDynamicTypeFromType(toType) as ReflectedType;
            ReflectedType dt = Ops.GetDynamicType(value) as ReflectedType;

            if (tt != null && dt != null) {
                Conversion conversion;
                object res = dt.TryConvertTo(value, tt, out conversion);
                if (conversion != Conversion.None) {
                    return res;
                }

                res = tt.TryConvertFrom(value, out conversion);
                if (conversion != Conversion.None) {
                    return res;
                }
            }

            if (toType.IsGenericType) {
                Type genTo = toType.GetGenericTypeDefinition();
                if (genTo == IListOfTType) {
                    return ConvertToIListT(value, toType.GetGenericArguments());
                } else if (genTo == IDictOfTType) {
                    return ConvertToIDictT(value, toType.GetGenericArguments());
                } else if (genTo == IEnumeratorOfT) {
                    return ConvertToIEnumeratorT(value, toType.GetGenericArguments());
                }
            }

            if (toType == ValueTypeType && fromType.IsValueType) {
                return (System.ValueType)value;
            }

            //!!! last-ditch effort gets many numeric types we'd miss otherwise
            return CheckedOldConversion(value, toType);
            
        }

        static object CheckedOldConversion(object value, Type toType) {
            Type fromType = value.GetType();
            if (CanConvertFrom(fromType, toType, NarrowingLevel.All)) return Converter.Convert(value, toType);

            throw MakeTypeError(toType, value);
        }

        public static bool CanConvertFrom(Type fromType, Type toType, NarrowingLevel allowNarrowing) {
            if (toType == fromType) return true;
            if (toType.IsAssignableFrom(fromType)) return true;
            if (fromType.IsCOMObject && toType.IsInterface) return true; // A COM object could be cast to any interface

            if (HasImplicitNumericConversion(fromType, toType)) return true;

            // Handling the hole that Type is the only object that we 'box'
            if (toType == typeof(Type) && typeof(PythonType).IsAssignableFrom(fromType)) return true;

            // Support extensible types with simple implicit conversions to their base types
            if (typeof(ExtensibleInt).IsAssignableFrom(fromType)) {
                return toType == typeof(int);
            }
            if (typeof(ExtensibleLong).IsAssignableFrom(fromType)) {
                return toType == typeof(IronMath.BigInteger);
            }
            if (typeof(ExtensibleString).IsAssignableFrom(fromType)) {
                return toType == typeof(string);
            }
            if (typeof(ExtensibleFloat).IsAssignableFrom(fromType)) {
                return toType == typeof(double);
            }
            if (typeof(ExtensibleComplex).IsAssignableFrom(fromType)) {
                return toType == typeof(IronMath.Complex64);
            }

            //!!!do user-defined implicit conversions here

            if (allowNarrowing == NarrowingLevel.None) return false;

            return HasNarrowingConversion(fromType, toType, allowNarrowing);
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
            ICustomAttributes dt = Ops.GetDynamicTypeFromType(t) as ICustomAttributes;
            if (dt == null) return false;
            object tmp;
            return dt.TryGetAttr(null, name, out tmp);
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

                if (typeof(ExtensibleInt).IsAssignableFrom(fromType)) {
                    return CanConvertFrom(typeof(int), toType, allowNarrowing);
                }
                if (typeof(ExtensibleLong).IsAssignableFrom(fromType)) {
                    return CanConvertFrom(typeof(IronMath.BigInteger), toType, allowNarrowing);
                }
                if (typeof(ExtensibleString).IsAssignableFrom(fromType)) {
                    return CanConvertFrom(typeof(string), toType, allowNarrowing);
                }
                if (typeof(ExtensibleFloat).IsAssignableFrom(fromType)) {
                    return CanConvertFrom(typeof(double), toType, allowNarrowing);
                }
                if (typeof(ExtensibleComplex).IsAssignableFrom(fromType)) {
                    return CanConvertFrom(typeof(IronMath.Complex64), toType, allowNarrowing);
                }
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
                            if (toType == typeof(IronMath.BigInteger)) return true;
                            if (toType == typeof(IronMath.Complex64)) return true;
                            return false;
                    }
                case TypeCode.Double:
                    switch (Type.GetTypeCode(toType)) {
                        default:
                            if (toType == typeof(IronMath.BigInteger)) return true;
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
    }

}