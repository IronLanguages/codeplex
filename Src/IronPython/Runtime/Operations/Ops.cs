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
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

using System.Reflection;
using System.Reflection.Emit;

using IronPython.Runtime;
using IronPython.Runtime.Types;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Exceptions;
using IronPython.Modules;
using IronPython.Compiler;
using IronPython.Compiler.Generation;
using IronPython.Compiler.AST;

using IronMath;

namespace IronPython.Runtime.Operations {

    /// <summary>
    /// Contains functions that are called directly from
    /// generated code to perform low-level runtime functionality.
    /// </summary>
    public static partial class Ops {
        private const int MIN_CACHE = -100;
        private const int MAX_CACHE = 1000;

        [ThreadStatic]
        private static ArrayList InfiniteCmp;
        [ThreadStatic]
        private static ArrayList InfiniteRepr;

        public static readonly object NotImplemented = "<NotImplemented>";
        public static readonly object Ellipsis = "...";
        public static readonly object TRUE = true;
        public static readonly object FALSE = false;

        private static readonly object[] EMPTY = new object[0];
        private static readonly object[] cache = new object[MAX_CACHE - MIN_CACHE];
        private static readonly string[] chars = new string[255];
        private static ReflectedType StringType;
        private static Dictionary<Type, DynamicType> dynamicTypes = MakeDynamicTypesTable();
        // The cache for dynamically generated delegates.
        private static Publisher<DelegateSignatureInfo, MethodInfo> dynamicDelegates = new Publisher<DelegateSignatureInfo, MethodInfo>();


        public static object[] MakeArray(object o1) { return new object[] { o1 }; }
        public static object[] MakeArray(object o1, object o2) { return new object[] { o1, o2 }; }


        public static List MakeList() { return List.MakeEmptyList(10); }

        public static List MakeList(params object[] items) {
            return new List(items);
        }

        public static Tuple MakeTuple(params object[] items) {
            return Tuple.MakeTuple(items);
        }

        public static Tuple MakeExpandableTuple(params object[] items) {
            return Tuple.MakeExpandableTuple(items);
        }

        public static object MakeSlice(object start, object stop, object step) {
            return new Slice(start, stop, step);
        }

        public static IronMath.BigInteger MakeIntegerFromHex(string s) {
            return LiteralParser.ParseBigInteger(s, 16);
        }

        public static Dict MakeDict(int size) {
            return new Dict(size);
        }

        public static bool IsTrue(object o) {
            if (o == null) return false;

            return Converter.ConvertToBoolean(o);
        }

        //        public static IList GetList(object o) {
        //            return (IList)o;
        //        }

        public static ArrayList GetReprInfinite() {
            if (InfiniteRepr == null) {
                InfiniteRepr = new ArrayList();
            }
            return InfiniteRepr;
        }

        public static ArrayList GetCmpInfinite() {
            if (InfiniteCmp == null) {
                InfiniteCmp = new ArrayList();
            }
            return InfiniteCmp;
        }

        internal static object GetClassName(object obj) {
            return (obj is OldInstance) ? ((OldInstance)obj).__class__.__name__ : Ops.GetDynamicType(obj).__name__;
        }

        public static string StringRepr(object o) {
            if (o == null) return "None";

            string s = o as string;
            if (s != null) return StringOps.Quote(s);
            if (o is int) return o.ToString();
            if (o is long) return ((long)o).ToString() + "L";
            if (o is BigInteger) return ((BigInteger)o).ToString() + "L";
            if (o is double) return FloatOps.ToString((double)o);
            if (o is float) return FloatOps.ToString((float)o);

            PerfTrack.NoteEvent(PerfTrack.Categories.Temporary, "Repr " + o.GetType().FullName);

            // could be a container object, we need to detect recursion, but only
            // for our own built-in types that we're aware of.  The user can setup
            // infinite recursion in their own class if they want.
            ICodeFormattable f = o as ICodeFormattable;
            if (f != null) {
                ArrayList infinite = GetAndCheckInfinite(o);
                if (infinite == null) return GetInfiniteRepr(o);
                int index = infinite.Add(o);
                try {
                    return f.ToCodeString();
                } finally {
                    System.Diagnostics.Debug.Assert(index == infinite.Count - 1);
                    infinite.RemoveAt(index);
                }
            }

            Array a = o as Array;
            if (a != null) {
                ArrayList infinite = GetAndCheckInfinite(o);
                if (infinite == null) return GetInfiniteRepr(o);
                int index = infinite.Add(o);
                try {
                    return ArrayOps.CodeRepresentation(a);
                } finally {
                    System.Diagnostics.Debug.Assert(index == infinite.Count - 1);
                    infinite.RemoveAt(index);
                }
            }

            return GetDynamicType(o).Repr(o);
        }

        private static ArrayList GetAndCheckInfinite(object o) {
            ArrayList infinite = GetReprInfinite();
            foreach (object o2 in infinite) {
                if (o == o2) {
                    return null;
                }
            }
            return infinite;
        }

        private static string GetInfiniteRepr(object o) {
            object keys;
            return o is List ? "[...]" :
                o is Dict ? "{...}" :
                Ops.TryGetAttr(o, SymbolTable.Keys, out keys) ? "{...}" : // user dictionary
                "...";
        }

        public static string ToString(object o) {
            if (o == null) return "None";
            else if (o is double) return FloatOps.ToString((double)o);
            else if (o is float) return FloatOps.ToString((float)o);
            return o.ToString();
        }


        public static object Repr(object o) {
            return StringRepr(o);
        }

        static Ops() {
            for (int i = 0; i < (MAX_CACHE - MIN_CACHE); i++) {
                cache[i] = (object)(i + MIN_CACHE);
            }

            for (char ch = (char)0; ch < 255; ch++) {
                chars[ch] = new string(ch, 1);
            }
        }

        public static string Char2String(char ch) {
            if (ch < 255) return chars[ch];
            return new string(ch, 1);
        }

        public static object Bool2Object(bool value) {
            return value ? TRUE : FALSE;
        }

        public static object Int2ByteOrInt(object val) {
            if (val is int) {
                int ival = (int)val;
                if (ival < 256 && ival >= 0) return ((byte)ival);
            }
            return val;
        }

        public static object Int2Object(int value) {
            // caches improves pystone by ~5-10% on MS .Net 1.1, this is a very integer intense app
            if (value < MAX_CACHE && value >= MIN_CACHE) {
                return cache[value - MIN_CACHE];
            }
            return (object)value;
        }

        public static object Long2Object(long value) {
            return value;  // just use standard boxing conversion here
        }

        public static Delegate GetDelegate(object o, Type delegateType) {
            Delegate handler = o as Delegate;

            if (handler != null) return handler;

            // if we have a python function make sure it's compatible...
            PythonFunction fo = o as PythonFunction;

            Method m = o as Method;
            if (m != null) {
                fo = m.Function as PythonFunction;
            }

            MethodInfo invoke = delegateType.GetMethod("Invoke");
            ParameterInfo[] pis = invoke.GetParameters();
            if (fo != null) {
                if (fo is FunctionN == false) {
                    int expArgCnt = pis.Length;
                    int maxArgCnt = fo.ArgCount;
                    int minArgCnt = fo.ArgCount - fo.FunctionDefaults.Count;

                    // take into account unbound methods / bound methods
                    if (m != null) {
                        if (m.Self != null) {
                            maxArgCnt--;
                            minArgCnt--;
                        }
                    }

                    // the target is no good for this delegate - we don't have enough
                    // parameters.
                    if (expArgCnt < minArgCnt || expArgCnt > maxArgCnt)
                        return null;
                }
            }

            DelegateSignatureInfo dsi = new DelegateSignatureInfo(invoke.ReturnType, pis);
            MethodInfo methodInfo = dynamicDelegates.GetOrCreateValue(dsi,
                delegate() {
                    // creation code
                    return dsi.CreateNewDelegate();
                });

            return CodeGen.CreateDelegate(methodInfo, delegateType, o);
        }        

        public static object ConvertTo(object o, Type toType) {
            return Converter.Convert(o, toType);
        }

        /// <summary>
        /// ToPython() wraps a CLI object with a PythonEngine object for cases where the PythonEngine does not want
        /// to deal with the CLI object directly. However, the general philosophy is to avoid using wrappers as
        /// that interferes with interoperability with the CLI world. Hence, there should be *very few* cases where
        /// wrappers are required. Try *really hard* to avoid wrappers.
        /// </summary>
        public static object ToPython(Type type, object o) {
            if (type == typeof(bool)) return Bool2Object((bool)o);  // preserve object identity o

            return o;
        }

        public static object GetLength(object o) {
            return Ops.NotImplemented;
        }

        public static object IsNonZero(object o) {
            return Ops.NotImplemented;
        }

        public static object Plus(object o) {
            object ret;

            if (o is int) return o;
            else if (o is double) return o;
            else if (o is BigInteger) return o;
            else if (o is Complex64) return o;
            else if (o is long) return o;
            else if (o is float) return o;
            else if (o is bool) return Int2Object((bool)o ? 1 : 0);
            else if(Ops.TryToInvoke(o, SymbolTable.Positive, out ret)) return ret;

            ret = GetDynamicType(o).Positive(o);
            if (ret != Ops.NotImplemented) return ret;

            throw Ops.TypeError("bad operand type for unary +");
        }

        public static object Negate(object o) {
            if (o is int) return IntOps.Negate((int)o);
            else if (o is double) return FloatOps.Negate((double)o);
            else if (o is long) return Int64Ops.Negate((long)o);
            else if (o is BigInteger) return LongOps.Negate((BigInteger)o);
            else if (o is Complex64) return ComplexOps.Negate((Complex64)o);
            else if (o is float) return FloatOps.Negate((float)o);
            else if (o is bool) return Int2Object((bool)o ? -1 : 0);

            object ret = GetDynamicType(o).Negate(o);
            if (ret != Ops.NotImplemented) return ret;

            throw Ops.TypeError("bad operand type for unary -");
        }

        public static object OnesComplement(object o) {
            if (o is int) return ~(int)o;
            if (o is long) return ~(long)o;
            if (o is BigInteger) return ~((BigInteger)o);
            if (o is bool) return Int2Object((bool)o ? -2 : -1);

            object ret = GetDynamicType(o).OnesComplement(o);
            if (ret != Ops.NotImplemented) return ret;

            throw Ops.TypeError("bad operand type for unary ~");
        }

        public static object Not(object o) {
            return IsTrue(o) ? FALSE : TRUE;
        }

        public static object Is(object x, object y) {
            return x == y ? TRUE: FALSE;
        }

        public static bool IsRetBool(object x, object y) {
            return x == y;
        }

        public static object IsNot(object x, object y) {
            return x != y ? TRUE : FALSE;
        }

        public static bool IsNotRetBool(object x, object y) {
            return x != y;
        }


        public static object In(object x, object y) {
            if (y is IDictionary) {
                return Bool2Object(((IDictionary)y).Contains(x));
            }

            if (y is IList) {
                return Bool2Object(((IList)y).Contains(x));
            }

            if (y is string) {
                string s = x as string;
                if (s == null) {
                    throw TypeError("'in <string>' requires string as left operand");
                }
                return ((string)y).Contains(s) ? TRUE : FALSE;
            }

            object contains;
            if(Ops.TryToInvoke(y, SymbolTable.Contains, out contains, x)) {
                return Ops.IsTrue(contains) ? TRUE : FALSE;
            }

            IEnumerator e = GetEnumerator(y);
            while (e.MoveNext()) {
                if (Ops.EqualRetBool(e.Current, x)) return TRUE;
            }

            return FALSE;
        }

        public static bool InRetBool(object x, object y) {
            if (y is IDictionary) {
                return ((IDictionary)y).Contains(x);
            }

            if (y is IList) {
                return ((IList)y).Contains(x);
            }

            if (y is string) {
                string s = x as string;
                if (s == null) {
                    throw TypeError("'in <string>' requires string as left operand");
                }
                return ((string)y).Contains(s);
            }

            object contains;
            if (Ops.TryToInvoke(y, SymbolTable.Contains, out contains, x)) {
                return Ops.IsTrue(contains);
            }

            IEnumerator e = GetEnumerator(y);
            while (e.MoveNext()) {
                if (Ops.EqualRetBool(e.Current, x)) return true;
            }

            return false;
        }

        public static object NotIn(object x, object y) {
            return Not(In(x, y));  //???
        }

        public static bool NotInRetBool(object x, object y) {
            return !InRetBool(x, y);  //???
        }

        //        public static object GetDynamicType1(object o) {
        //            IConvertible ic = o as IConvertible;
        //            if (ic != null) {
        //                switch (ic.GetTypeCode()) {
        //                    case TypeCode.Int32: return "int";
        //                    case TypeCode.Double: return "double";
        //                    default: throw new NotImplementedException();
        //                }
        //            } else {
        //                throw new NotImplementedException();
        //            }
        //        }
        //
        //        private static object[] oas = new object[] { "int", "double" };
        //
        //        public static object GetDynamicType2(object o) {
        //            Type ty = o.GetType();
        //            int hc = ty.GetHashCode();
        //
        //            return oas[hc%1];
        //        }

        private static Dictionary<Type, DynamicType> MakeDynamicTypesTable() {
            Dictionary<Type, DynamicType> ret = new Dictionary<Type, DynamicType>();

            ret[typeof(ValueType)] = ReflectedType.FromClsOnlyType(typeof(ValueType));
            StringType = new StringDynamicType();
            ret[typeof(string)] = StringType;
            ret[typeof(ExtensibleString)] = StringType;

            ret[typeof(object)] = ReflectedType.FromType(typeof(object), true);
            ret[typeof(int)] = IntOps.MakeDynamicType();
            ret[typeof(ExtensibleInt)] = ret[typeof(int)];

            ret[typeof(double)] = FloatOps.MakeDynamicType();
            ret[typeof(Single)] = SingleOps.MakeDynamicType();
            ret[typeof(ExtensibleFloat)] = ret[typeof(double)];

            ret[typeof(BigInteger)] = LongOps.MakeDynamicType();

            ret[typeof(long)] = Int64Ops.MakeDynamicType();
            ret[typeof(bool)] = BoolOps.MakeDynamicType((ReflectedType)ret[typeof(int)]);
            ret[typeof(OldInstance)] = OldInstanceType.Instance;
            ret[typeof(Array)] = ArrayOps.MakeDynamicType();
            ret[typeof(Assembly)] = ReflectedAssemblyType.MakeDynamicType();
            ret[typeof(Complex64)] = ComplexOps.MakeDynamicType();
            ret[typeof(Decimal)] = DecimalOps.MakeDynamicType();

            ret[typeof(byte)] = ByteOps.MakeDynamicType();
            ret[typeof(sbyte)] = SByteOps.MakeDynamicType();
            ret[typeof(short)] = Int16Ops.MakeDynamicType();
            ret[typeof(ushort)] = UInt16Ops.MakeDynamicType();
            ret[typeof(uint)] = UInt32Ops.MakeDynamicType();
            ret[typeof(ulong)] = UInt64Ops.MakeDynamicType();
            

            return ret;
        }

        public static DynamicType GetDynamicTypeFromClsOnlyType(Type ty) {
            DynamicType ret;
            if (dynamicTypes.TryGetValue(ty, out ret)) return ret;

            ret = ReflectedType.FromClsOnlyType(ty);
            dynamicTypes[ty] = ret;
            return ret;
        }

        public static DynamicType GetDynamicTypeFromType(Type ty) {
            PerfTrack.NoteEvent(PerfTrack.Categories.DictInvoke, "TypeLookup " + ty.FullName);

            DynamicType ret;
            if (dynamicTypes.TryGetValue(ty, out ret)) return ret;

            lock (dynamicTypes) {
                if (dynamicTypes.TryGetValue(ty, out ret)) return ret;

                ret = ReflectedType.FromType(ty);
                dynamicTypes[ty] = ret;
                return ret;
            }
        }

        public static void RegisterAttributesInjectorForType(Type ty, IAttributesInjector attrInjector, bool prepend) {
            ReflectedType rty = GetDynamicTypeFromType(ty) as ReflectedType;

            if (rty == null) {
                throw new Exception("failed to obtain ReflectedType from Type");
            }

            rty.RegisterAttributesInjector(attrInjector, prepend);
        }

        public static DynamicType GetDynamicType(object o) {
            IDynamicObject dt = o as IDynamicObject;
            if (dt != null) return dt.GetDynamicType();

            if (o == null) return NoneType.InstanceOfNoneType;

            if (o is String) return StringType;

            return GetDynamicTypeFromType(o.GetType());
        }

        public static bool EqualIsTrue(object x, int y) {
            if (x is int) return ((int)x) == y;

            return EqualRetBool(x, y);
        }

        internal delegate T MultiplySequenceWorker<T>(T self, int count);

        /// <summary>
        /// Wraps up all the semantics of multiplying sequences so that all of our sequences
        /// don't duplicate the same logic.  When multiplying sequences we need to deal with
        /// only multiplying by valid sequence types (ints, not floats), support coercion
        /// to integers if the type supports it, not multiplying by None, and getting the
        /// right semantics for multiplying by negative numbers and 1 (w/ and w/o subclasses).
        /// </summary>
        internal static object MultiplySequence<T>(MultiplySequenceWorker<T> multiplier, T sequence, object count) {
            Conversion conv;
            int value = Converter.TryConvertToInt32(count, out conv);
            if(conv == Conversion.None) {
                // check __coerce__...
                Tuple coerced = Ops.GetDynamicType(count).Coerce(count, 1) as Tuple;
                if (coerced != null) {
                    value = Converter.TryConvertToInt32(coerced[0], out conv);
                    if (conv <= Conversion.Implicit) {
                        if (value < 0) value = 0;
                        if (value == 1 && sequence.GetType() == typeof(T)) return sequence;

                        return multiplier(sequence, value);
                    }
                }

                if (count != null && !(count is Complex64 || count is ExtensibleComplex)) {
                    // try __rmul__
                    object ret;
                    if (Ops.TryToInvoke(count, SymbolTable.OpReverseMultiply, out ret, sequence)) {
                        return ret;
                    }
                }
            } else if (conv <= Conversion.Implicit) {
                if (value < 0) value = 0;
                if (value == 1 && sequence.GetType() == typeof(T)) return sequence;

                return multiplier(sequence, value);
            }

            throw Ops.TypeError("can't multiply sequence by non-int");
        }        

        public static object Equal(object x, object y) {
            ExtensibleString es;

            object ret = Ops.NotImplemented;
            if (x is int) {
                ret = IntOps.Equals((int)x, y);
            } else if (x is string) {
                ret = StringOps.Equals((string)x, y);
            } else if (x is double) {
                ret = FloatOps.Equals((double)x, y);
            } else if (x is BigInteger) {
                ret = LongOps.Equals((BigInteger)x, y);
            } else if (x is Complex64) {
                ret = ComplexOps.Equals((Complex64)x, y);
            } else if (x == null) {
                if (y == null) ret = TRUE;
                else if (y.GetType().IsPrimitive || y is BigInteger) ret = FALSE;
            } else if (x is bool) {
                ret = BoolOps.Equals((bool)x, y);
            } else if (x is ExtensibleInt) {
                ret = IntOps.Equals(((ExtensibleInt)x).value, y);
            } else if ((es = x as ExtensibleString)!=null) {
                ret = es.RichEquals(y);
            } else if (x is ExtensibleFloat) {
                ret = FloatOps.Equals(((ExtensibleFloat)x).value, y);
            } else if (x is byte) {
                ret = IntOps.Equals((int)(byte)x, y);
            } else if (x is long) {
                ret = Int64Ops.Equals((long)x, y);
            } else if (x is float) {
                ret = FloatOps.Equals((float)x, y);
            } else if (x is Enum) {
                ret = EnumOps.Equal(x, y);
            } else if (x is sbyte) {
                ret = IntOps.Equals((int)(sbyte)x, y);
            } else if (x is short) {
                ret = IntOps.Equals((int)(short)x, y);
            } else if (x is ushort) {
                ret = IntOps.Equals((int)(ushort)x, y);
            } else if (x is uint) {
                ret = LongOps.Equals(BigInteger.Create((uint)x), y);
            } else if (x is ulong) {
                ret = LongOps.Equals(BigInteger.Create((ulong)x), y);
            } else if (x is decimal) {
                ret = FloatOps.Equals((double)(decimal)x, y);
            }

            if (ret != Ops.NotImplemented) return ret;

            IRichEquality ipe = x as IRichEquality;
            if (ipe != null) ret = ipe.RichEquals(y);
            if (ret != Ops.NotImplemented) return ret;

            ipe = y as IRichEquality;
            if (ipe != null) ret = ipe.RichEquals(x);
            if (ret != Ops.NotImplemented) return ret;

            ret = GetDynamicType(x).Equal(x, y);
            if (ret != Ops.NotImplemented) return ret;

            ret = GetDynamicType(y).Equal(y, x);
            if (ret != Ops.NotImplemented) return ret;

            if (x == null) {
                return y != null ? Ops.FALSE : Ops.TRUE;
            }
            
            return Bool2Object(x.Equals(y));
        }

        public static bool EqualRetBool(object x, object y) {
            ExtensibleString es;

            if (x is int) {
                return IntOps.EqualsRetBool((int)x, y);
            } else if (x is string) {
                return StringOps.EqualsRetBool((string)x, y);
            } else if (x is double) {
                return FloatOps.EqualsRetBool((double)x, y);
            } else if (x is BigInteger) {
                return LongOps.EqualsRetBool((BigInteger)x, y);
            } else if (x is Complex64) {
                return ComplexOps.EqualsRetBool((Complex64)x, y);
            } else if (x == null) {
                if (y == null) return true;
                else if (y.GetType().IsPrimitive || y is BigInteger) return false;
            } else if (x is bool) {
                return BoolOps.EqualsRetBool((bool)x, y);
            } else if (x is ExtensibleInt) {
                return IntOps.EqualsRetBool(((ExtensibleInt)x).value, y);
            } else if ((es = x as ExtensibleString) != null) {
                object res = es.RichEquals(y);
                if(res != Ops.NotImplemented) return (bool)res;
            } else if (x is ExtensibleFloat) {
                return FloatOps.EqualsRetBool(((ExtensibleFloat)x).value, y);
            } else if (x is byte) {
                return IntOps.EqualsRetBool((int)(byte)x, y);
            } else if (x is long) {
                return Int64Ops.EqualsRetBool((long)x, y);
            } else if (x is float) {
                return FloatOps.EqualsRetBool((float)x, y);
            } else if (x is Enum) {
                return EnumOps.EqualRetBool(x, y);
            } else if (x is sbyte) {
                return IntOps.EqualsRetBool((int)(sbyte)x, y);
            } else if (x is short) {
                return IntOps.EqualsRetBool((int)(short)x, y);
            } else if (x is ushort) {
                return IntOps.EqualsRetBool((int)(ushort)x, y);
            } else if (x is uint) {
                return LongOps.EqualsRetBool(BigInteger.Create((uint)x), y);
            } else if (x is ulong) {
                return LongOps.EqualsRetBool(BigInteger.Create((ulong)x), y);
            } else if (x is decimal) {
                return FloatOps.EqualsRetBool((double)(decimal)x, y);
            }

            return DynamicEqualRetBool(x, y);
        }

        public static bool DynamicEqualRetBool(object x, object y) {
            object ret = Ops.NotImplemented;

            IRichEquality ipe = x as IRichEquality;
            if (ipe != null) ret = ipe.RichEquals(y);
            if (ret != Ops.NotImplemented) return Ops.IsTrue(ret);

            ipe = y as IRichEquality;
            if (ipe != null) ret = ipe.RichEquals(x);
            if (ret != Ops.NotImplemented) return Ops.IsTrue(ret);

            ret = GetDynamicType(x).Equal(x, y);
            if (ret != Ops.NotImplemented) return Ops.IsTrue(ret);

            ret = GetDynamicType(y).Equal(y, x);
            if (ret != Ops.NotImplemented) return Ops.IsTrue(ret);

            // we've tried all the possible comparisons... if x is null
            // then y doesn't know how to compare against it
            if (x == null) return false;
            return x.Equals(y);
        }

        public static object NotEqual(object x, object y) {
            ExtensibleString es;

            object ret = Ops.NotImplemented;
            if (x is int) {
                ret = IntOps.Equals((int)x, y);
            } else if (x is string) {
                ret = StringOps.Equals((string)x, y);
            } else if (x is double) {
                ret = FloatOps.Equals((double)x, y);
            } else if (x is BigInteger) {
                ret = LongOps.Equals((BigInteger)x, y);
            } else if (x is Complex64) {
                ret = ComplexOps.Equals((Complex64)x, y);
            } else if (x == null) {
                if (y == null) ret = TRUE;
                else if (y.GetType().IsPrimitive || y is BigInteger) ret = FALSE;
            } else if (x is bool) {
                ret = BoolOps.Equals((bool)x, y);
            } else if (x is ExtensibleInt) {
                ret = IntOps.Equals(((ExtensibleInt)x).value, y);
            } else if ((es = x as ExtensibleString)!=null) {
                ret = es.RichEquals(y);
            } else if (x is ExtensibleFloat) {
                ret = FloatOps.Equals(((ExtensibleFloat)x).value, y);
            } else if (x is byte) {
                ret = IntOps.Equals((int)(byte)x, y);
            } else if (x is long) {
                ret = Int64Ops.Equals((long)x, y);
            } else if (x is float) {
                ret = FloatOps.Equals((float)x, y);
            } else if (x is Enum) {
                ret = EnumOps.Equal(x, y);
            } else if (x is sbyte) {
                ret = IntOps.Equals((int)(sbyte)x, y);
            } else if (x is short) {
                ret = IntOps.Equals((int)(short)x, y);
            } else if (x is ushort) {
                ret = IntOps.Equals((int)(ushort)x, y);
            } else if (x is uint) {
                ret = LongOps.Equals(BigInteger.Create((uint)x), y);
            } else if (x is ulong) {
                ret = LongOps.Equals(BigInteger.Create((ulong)x), y);
            } else if (x is decimal) {
                ret = FloatOps.Equals((double)(decimal)x, y);
            }

            if (ret != Ops.NotImplemented) return Not(ret);

            IRichEquality ipe = x as IRichEquality;
            if (ipe != null) ret = ipe.RichNotEquals(y);
            if (ret != Ops.NotImplemented) return ret;

            ipe = y as IRichEquality;
            if (ipe != null) ret = ipe.RichNotEquals(x);
            if (ret != Ops.NotImplemented) return ret;

            ret = GetDynamicType(x).NotEqual(x, y);
            if (ret != Ops.NotImplemented) return ret;

            ret = GetDynamicType(y).NotEqual(y, x);
            if (ret != Ops.NotImplemented) return ret;

            if (x != null) {
                return Bool2Object(!x.Equals(y));
            } else {
                return TRUE;
            }
        }

        public static bool NotEqualRetBool(object x, object y) {
            ExtensibleString es;

            if (x is int) {
                return !IntOps.EqualsRetBool((int)x, y);
            } else if (x is string) {
                return !StringOps.EqualsRetBool((string)x, y);
            } else if (x is double) {
                return !FloatOps.EqualsRetBool((double)x, y);
            } else if (x is BigInteger) {
                return !LongOps.EqualsRetBool((BigInteger)x, y);
            } else if (x is Complex64) {
                return !ComplexOps.EqualsRetBool((Complex64)x, y);
            } else if (x == null) {
                if (y == null) return false;
                else if (y.GetType().IsPrimitive || y is BigInteger) return true;
            } else if (x is bool) {
                return !BoolOps.EqualsRetBool((bool)x, y);
            } else if (x is ExtensibleInt) {
                return !IntOps.EqualsRetBool(((ExtensibleInt)x).value, y);
            } else if ((es = x as ExtensibleString)!=null) {
                object ret = es.RichEquals(y);
                if (ret != Ops.NotImplemented) return (bool)Not(ret);
            } else if (x is ExtensibleFloat) {
                return !FloatOps.EqualsRetBool(((ExtensibleFloat)x).value, y);
            } else if (x is byte) {
                return !IntOps.EqualsRetBool((int)(byte)x, y);
            } else if (x is long) {
                return !Int64Ops.EqualsRetBool((long)x, y);
            } else if (x is float) {
                return !FloatOps.EqualsRetBool((float)x, y);
            } else if (x is Enum) {
                return !EnumOps.EqualRetBool(x, y);
            } else if (x is sbyte) {
                return !IntOps.EqualsRetBool((int)(sbyte)x, y);
            } else if (x is short) {
                return !IntOps.EqualsRetBool((int)(short)x, y);
            } else if (x is ushort) {
                return !IntOps.EqualsRetBool((int)(ushort)x, y);
            } else if (x is uint) {
                return !LongOps.EqualsRetBool(BigInteger.Create((uint)x), y);
            } else if (x is ulong) {
                return !LongOps.EqualsRetBool(BigInteger.Create((ulong)x), y);
            } else if (x is decimal) {
                return !FloatOps.EqualsRetBool((double)(decimal)x, y);
            }

            return DynamicNotEqualRetBool(x, y);
        }

        public static bool DynamicNotEqualRetBool(object x, object y) {
            object ret = Ops.NotImplemented;

            IRichEquality ipe = x as IRichEquality;
            if (ipe != null) ret = ipe.RichEquals(y);
            if (ret != Ops.NotImplemented) return !Ops.IsTrue(ret);

            ipe = y as IRichEquality;
            if (ipe != null) ret = ipe.RichEquals(x);
            if (ret != Ops.NotImplemented) return !Ops.IsTrue(ret);

            ret = GetDynamicType(x).NotEqual(x, y);
            if (ret != Ops.NotImplemented) return Ops.IsTrue(ret);

            ret = GetDynamicType(y).NotEqual(y, x);
            if (ret != Ops.NotImplemented) return Ops.IsTrue(ret);

            // we've tried all the possible comparisons... if x is null
            // then y doesn't know how to compare against it
            if (x == null) return true;
            return !x.Equals(y);
        }

        private static int ConvertToCompareInt(object x) {
            Conversion conv;
            int res = Converter.TryConvertToInt32(x, out conv);
            if(conv == Conversion.None){
                BigInteger bi = Converter.TryConvertToBigInteger(x, out conv);
                if (conv == Conversion.None) throw Ops.TypeError("Bad return type from comparison: {0}", Ops.GetDynamicType(x).__name__);
                
                if (bi > 0) return 1;
                else if (bi < 0) return -1;
                return 0;                                
            }
            return res;
        }
        public static int Compare(object x, object y) {
            if (x == y) return 0;

            // built-in types need to be special cased here (like for equals)
            // because they don't implement our interfaces.

            // it'd be nice to check for null ahead of time, but we can't...
            // the user could have defined __cmp__ on a class and have it
            // compare specially against null it's self.

            object ret = Ops.NotImplemented;
            if (x is int) {
                ret = IntOps.Compare((int)x, y);
            } else if (x is double) {
                ret = FloatOps.Compare((double)x, y);
            } else if (x is BigInteger) {
                ret = LongOps.Compare((BigInteger)x, y);
            } else if (x is string && y is string) {
                int temp = string.CompareOrdinal((string)x, (string)y);
                return (temp == 0) ? 0 : (temp > 0 ? 1 : -1);
            } else if (x is Complex64 || y is Complex64) {
                return ComplexOps.TrueCompare(x, y);
            } else if (x is bool) {
                ret = BoolOps.Compare((bool)x, y);
            } else if (x is long) {
                ret = Int64Ops.Compare((long)x, y);
            } else if (x is ulong) {
                ret = Int64Ops.Compare((ulong)x, y);
            } else if (x is short) {
                ret = IntOps.Compare((int)(short)x, y);
            } else if (x is ushort) {
                ret = IntOps.Compare((int)(ushort)x, y);
            } else if (x is byte) {
                ret = IntOps.Compare((int)(byte)x, y);
            } else if (x is sbyte) {
                ret = IntOps.Compare((int)(sbyte)x, y);
            } else if (x is decimal) {
                ret = FloatOps.Compare((double)(decimal)x, y);
            } else if (x is uint) {
                ret = Int64Ops.Compare((long)(uint)x, y);
            } else if (x == null) {
                if (y.GetType().IsPrimitive || y is BigInteger) {
                    // built-in type that doesn't implement our comparable
                    // interfaces, being compared against null, go ahead
                    // and skip the rest of the checks.
                    return -1;
                }
            } else if (x is ExtensibleComplex || y is ExtensibleComplex) {
                ret = ExtensibleComplex.TrueCompare(x, y);
            }

            if (ret != Ops.NotImplemented) return ConvertToCompareInt(ret);

            ret = TryRichCompare(x, y);
            if (ret != Ops.NotImplemented) return ConvertToCompareInt(ret);

            Type xType = (x == null) ? null : x.GetType(), yType = (y == null) ? null : y.GetType();
            
            IComparable c = x as IComparable;
            if (c != null) {
                if (xType != null && xType != yType) {
                    object z;
                    try {
                        Conversion conversion;
                        z = Converter.TryConvert(y, xType, out conversion);
                        if (conversion < Conversion.NonStandard) {
                            int res = c.CompareTo(z);
                            return res < 0 ? -1 : res > 0 ? 1 : 0;
                        }
                    } catch {
                    }
                } else {
                    int res = c.CompareTo(y);
                    return res < 0 ? -1 : res > 0 ? 1 : 0;
                }
            }
            c = y as IComparable;
            if (c != null) {
                if (yType != null && xType != yType) {
                    try {
                        Conversion conversion;
                        object z = Converter.TryConvert(x, yType, out conversion);
                        if (conversion < Conversion.NonStandard) {
                            int res = c.CompareTo(z);
                            return res < 0 ? 1 : res > 0 ? -1 : 0;
                        }
                    } catch {
                    }
                } else {
                    int res = c.CompareTo(x);
                    return res < 0 ? -1 : res > 0 ? 1 : 0;
                }
            }

            return CompareTypes(x, y);            
        }

        private static int CompareTypes(object x, object y) {
            if (x == null) return -1;
            if (y == null) return 1;

            string name1, name2;
            int diff;

            if (Ops.GetDynamicType(x) != Ops.GetDynamicType(y)) {
                if (x.GetType() == typeof(OldInstance)) {
                    name1 = (string)((OldInstance)x).__class__.__name__;
                    if (y.GetType() == typeof(OldInstance)) {
                        name2 = (string)((OldInstance)y).__class__.__name__;
                    } else {
                        // old instances are always less than new-style classes
                        return -1;
                    }
                } else if (y.GetType() == typeof(OldInstance)) {
                    // old instances are always less than new-style classes
                    return 1;
                } else {
                    name1 = (string)Ops.GetDynamicType(x).__name__;
                    name2 = (string)Ops.GetDynamicType(y).__name__;
                }
                diff = String.CompareOrdinal(name1, name2);
            } else {
                diff = (int)(IdDispenser.GetId(x) - IdDispenser.GetId(y));
            }
            
            if (diff < 0) return -1;
            if (diff == 0) return 0;
            return 1;
        }       

        /// <summary>
        /// Attempts a Python rich comparison (see PEP 207)
        /// </summary>
        private static object TryRichCompare(object x, object y) {
            object ret = Ops.NotImplemented;

            IRichComparable rc1 = x as IRichComparable;
            if (rc1 != null) {
                if ((ret = rc1.CompareTo(y)) != Ops.NotImplemented) {
                    return ret;
                }
            } 

            IRichComparable rc2 = y as IRichComparable;
            if (rc2 != null) {
                if ((ret = rc2.CompareTo(x)) != Ops.NotImplemented) {
                    return Ops.GetDynamicType(ret).Multiply(ret, -1);
                }
            }

            if (rc1 != null || rc2 != null) {
                // we've done our rich comparison via the fast path,
                // no need to try it via the slow types path...
                return Ops.NotImplemented;
            }
            
            // try the slow path...

            DynamicType dt1 = x != null ? null : GetDynamicType(x);
            DynamicType dt2 = y != null ? null : GetDynamicType(y);

            if (dt1 != null) {
                if ((ret = dt1.GreaterThan(x, y)) != Ops.NotImplemented) { if (IsTrue(ret)) return 1; }
                if ((ret = dt1.LessThan(x, y)) != Ops.NotImplemented) { if (IsTrue(ret)) return -1; }
                if ((ret = dt1.Equal(x, y)) != Ops.NotImplemented) { if (IsTrue(ret)) return 0; }
            }

            if (dt2 != null) {
                if ((ret = dt2.GreaterThan(y, x)) != Ops.NotImplemented) { if (IsTrue(ret)) return -1; }
                if ((ret = dt2.LessThan(y, x)) != Ops.NotImplemented) { if (IsTrue(ret)) return 1; }
                if ((ret = dt2.Equal(y, x)) != Ops.NotImplemented) { if (IsTrue(ret)) return 0; }
            } 

            if (dt1 != null && (ret = dt1.CompareTo(x, y)) != Ops.NotImplemented) return Ops.CompareToZero(ret);
            if (dt2 != null && (ret = dt2.CompareTo(y, x)) != Ops.NotImplemented) return -1 * Ops.CompareToZero(ret);

            return Ops.NotImplemented;
        }

        public static int CompareToZero(object value) {
            Conversion conversion;
            double val = Converter.TryConvertToDouble(value, out conversion);
            if (conversion == Conversion.None) {
                throw Ops.TypeError("unable to compare type {0} with 0 ", Ops.GetDynamicType(value).__name__);
            }
            if (val > 0) return 1;
            if (val < 0) return -1;
            return 0;
        }

        public static int CompareArrays(object[] data0, int size0, object[] data1, int size1) {
            int size = Math.Min(size0, size1);
            for (int i = 0; i < size; i++) {
                int c = Ops.Compare(data0[i], data1[i]);
                if (c != 0) return c;
            }
            if (size0 == size1) return 0;
            return size0 > size1 ? +1 : -1;
        }

        public static object PowerMod(object x, object y, object z) {
            object ret;
            if (x is int) {
                ret = IntOps.PowerMod((int)x, y, z);
                if (ret != NotImplemented) return ret;
            } else if (x is long) {
                ret = Int64Ops.PowerMod((long)x, y, z);
                if (ret != NotImplemented) return ret;
            } else if (x is IronMath.BigInteger) {
                ret = LongOps.PowerMod((IronMath.BigInteger)x, y, z);
                if (ret != NotImplemented) return ret;
            }

            if (x is IronMath.Complex64 || y is IronMath.Complex64 || z is IronMath.Complex64) {
                throw Ops.ValueError("complex modulo");
            }

            //            ret = GetDynamicType(x).Power(x, y);
            //            if (ret != NotImplemented) return ret;
            //            ret = GetDynamicType(y).ReversePower(y, x);
            //            if (ret != NotImplemented) return ret;
            throw Ops.TypeError("unsupported operand type(s) for power with modulus: '{0}' and '{1}'",
                GetDynamicType(x).__name__, GetDynamicType(y).__name__);
        }

        public static ICollection GetCollection(object o) {
            ICollection ret = o as ICollection;
            if (ret != null) return ret;

            ArrayList al = new ArrayList();
            IEnumerator e = GetEnumerator(o);
            while (e.MoveNext()) al.Add(e.Current);
            return al;
        }

        public static bool TryGetEnumerator(object o, out IEnumerator ie) {
            if (o is string) {
                ie = StringOps.GetEnumerator((string)o);
                return true;
            } else if (o is IEnumerable) {
                ie = ((IEnumerable)o).GetEnumerator();
                return true;
            } else if (o is IEnumerator) {
                ie = (IEnumerator)o;
                return true;
            }

            IEnumerator enumerator;
            if (PythonEnumerator.Create(o, out enumerator)) {
                ie = enumerator;
                return true;
            }
            if (ItemEnumerator.Create(o, out enumerator)) {
                ie = enumerator;
                return true;
            }
            ie = null;
            return false;
        }

        public static IEnumerator GetEnumerator(object o) {
            IEnumerator ie;
            if (!TryGetEnumerator(o, out ie)) {
                throw Ops.TypeError("{0} is not enumerable", StringRepr(o));
            }
            return ie;
        }

        /// <summary>
        /// Returns the number of elements in the collection, and the element values
        /// </summary>
        /// <param name="ie">the enumerator we will go through</param>
        /// <param name="values">pre-allocated object array with expected size</param>
        /// <returns>
        ///     if the enumerater has less or equal number of objects than expected, 
        ///         return that number
        ///     otherwise, 
        ///         return the expected size + 1
        /// </returns>
        public static int GetEnumeratorValues(IEnumerator ie, ref object[] values) {
            int count = 0;
            int expected = values.Length;

            while (count < expected) {
                if (ie.MoveNext()) {
                    values[count] = ie.Current;
                    count++;
                } else {
                    return count;
                }
            }

            return count + (ie.MoveNext() ? 1 : 0);
        }

        public static long Id(object o) {
            return IdDispenser.GetId(o);
        }

        public static string HexId(object o) {
            return string.Format("0x{0:X16}", Id(o));
        }

        public static int Hash(object o) {
            if (o is int) return (int)o;
            if (o is string) return o.GetHashCode();    // avoid lookups on strings - A) We can stack overflow w/ Dict B) they don't define __hash__
            if (o is double) return (int)(double)o;
            if (o == null) return NoneType.NoneHashCode;

            IRichEquality ipe = o as IRichEquality;
            if (ipe != null) {
                object res = ipe.RichGetHashCode();
                if (res != Ops.NotImplemented) return (int)res;
            }

            return o.GetHashCode();
        }

        public static object Hex(object o) {
            if (o is int) return IntOps.Hex((int)o);
            else if (o is long) return Int64Ops.Hex((long)o);
            else if (o is BigInteger) return LongOps.Hex((BigInteger)o);

            object hex;
            if(TryToInvoke(o, SymbolTable.ConvertToHex, out hex)) {
                return hex;
            }
            throw TypeError("hex() argument cannot be converted to hex");
        }

        public static object Oct(object o) {
            if (o is int) {
                return IntOps.Oct((int)o);
            } else if (o is long) {
                return Int64Ops.Oct((long)o);
            } else if (o is BigInteger) {
                return LongOps.Oct((BigInteger)o);
            }

            object octal;
            if (TryToInvoke(o, SymbolTable.ConvertToOctal, out octal)) {
                return octal;
            }
            throw TypeError("hex() argument cannot be converted to hex");
        }

        public static object CallWithContext(ICallerContext context, object func, params object[] args) {
            ICallableWithCallerContext icc = func as ICallableWithCallerContext;
            if (icc != null) return icc.Call(context, args);

            ICallable ic = func as ICallable;
            if (ic != null) return ic.Call(args);

            return GetDynamicType(func).Call(func, args);
        }

        public static object CallWithContext(ICallerContext context, object func, object[] args, string[] names) {
            IFancyCallable ic = func as IFancyCallable;
            if (ic != null) return ic.Call(context, args, names);

            object callMeth;

            if (Ops.TryToInvoke(func, SymbolTable.Call, out callMeth)) {
                ic = callMeth as IFancyCallable;

                if (ic != null) return ic.Call(context, args, names);
            }

            throw new Exception("this object is not callable with keyword parameters");
        }

        public static object CallWithArgsTupleAndContext(ICallerContext context, object func, object[] args, object argsTuple) {
            Tuple tp = argsTuple as Tuple;
            if (tp != null) {
                object[] nargs = new object[args.Length + tp.Count];
                for (int i = 0; i < args.Length; i++) nargs[i] = args[i];
                for (int i = 0; i < tp.Count; i++) nargs[i + args.Length] = tp[i];
                return CallWithContext(context, func, nargs);
            }

            List allArgs = List.MakeEmptyList(args.Length + 10);
            IEnumerator e = Ops.GetEnumerator(argsTuple);
            while (e.MoveNext()) allArgs.AddNoLock(e.Current);

            return CallWithContext(context, func, allArgs.GetObjectArray());
        }

        public static object CallWithArgsTupleAndKeywordDictAndContext(ICallerContext context, object func, object[] args, string[] names, object argsTuple, object kwDict) {
            IDictionary kws = kwDict as IDictionary;
            if (kws == null && kwDict != null) throw Ops.TypeError("argument after ** must be a dictionary");

            if ((kws == null || kws.Count == 0) && names.Length == 0) {
                ArrayList largs = new ArrayList(args);
                if (argsTuple != null) {
                    largs.AddRange(Ops.GetCollection(argsTuple));
                }
                return CallWithContext(context, func, (object[])largs.ToArray(typeof(object)));
            } else {
                IFancyCallable ic = func as IFancyCallable;
                if (ic == null) throw new Exception("this object is not callable with keyword parameters");

                List<object> largs;

                if (argsTuple != null && args.Length == names.Length) {
                    Tuple tuple = argsTuple as Tuple;
                    if (tuple == null) tuple = new Tuple(argsTuple);                    
                    
                    largs = new List<object>(tuple);
                    largs.AddRange(args);
                } else {
                    largs = new List<object>(args);
                    if (argsTuple != null) {
                        largs.InsertRange(args.Length - names.Length, (Tuple)argsTuple);
                    }
                }

                List<string> lnames = new List<string>(names);

                if (kws != null) {
                    IDictionaryEnumerator ide = kws.GetEnumerator();
                    while (ide.MoveNext()) {
                        lnames.Add((string)ide.Key);
                        largs.Add(ide.Value);
                    }
                }

                return ic.Call(context, largs.ToArray(), lnames.ToArray());
            }
        }

        public static bool TryCall(object func, object arg0, out object ret) {
            BuiltinFunction bf = func as BuiltinFunction;
            if (bf != null) return bf.TryCall(arg0, out ret);

            BoundBuiltinFunction bbf = func as BoundBuiltinFunction;
            if (bbf != null) return bbf.TryCall(arg0, out ret);

            ret = Call(func, arg0);
            return true;
        }

        public static object Call(object func, params object[] args) {
            ICallable ic = func as ICallable;
            if (ic != null) return ic.Call(args);

            return GetDynamicType(func).Call(func, args);
        }
       
        public static object Call(ICallerContext context, object func, object[] args, string[] names) {
            IFancyCallable ic = func as IFancyCallable;
            if (ic != null) return ic.Call(context, args, names);

            object ret;
            if (TryFancyInvoke(func, SymbolTable.Call, args, names, out ret)) {
                return ret;
            }

            throw new Exception("this object is not callable with keyword parameters");
        }

        public static object CallWithArgsTuple(object func, object[] args, object argsTuple) {
            Tuple tp = argsTuple as Tuple;
            if (tp != null) {
                object[] nargs = new object[args.Length + tp.Count];
                for (int i = 0; i < args.Length; i++) nargs[i] = args[i];
                for (int i = 0; i < tp.Count; i++) nargs[i + args.Length] = tp[i];
                return Call(func, nargs);
            }

            List allArgs = List.MakeEmptyList(args.Length + 10);
            IEnumerator e = Ops.GetEnumerator(argsTuple);
            while (e.MoveNext()) allArgs.AddNoLock(e.Current);

            return Call(func, allArgs.GetObjectArray());
        }

        public static object GetIndex(object o, object index) {
            ISequence seq = o as ISequence;
            if (seq != null) {
                Slice slice;
                if (index is int) return seq[(int)index];
                else if ((slice = index as Slice)!=null) {
                    if (slice.step == null) {
                        int start, stop;
                        slice.DeprecatedFixed(o, out start, out stop);

                        return seq.GetSlice(start, stop);
                    }                    

                    return seq[slice];
                }
                //???
            }

            string s = o as string;
            if (s != null) {
                if (index is int) return StringOps.__getitem__(s, (int)index);
                else if (index is Slice) return StringOps.__getitem__(s, (Slice)index);
                //???
            }

            IMapping map = o as IMapping;
            if (map != null) {
                return map[index];
            }

            Array array = o as Array;
            if (array != null) {
                return ArrayOps.GetIndex(array, index);
            }

            IList list = o as IList;
            if (list != null) {
                Conversion conv;
                int val = Converter.TryConvertToInt32(index, out conv);
                if (conv != Conversion.None) {
                    return list[val];
                }
            }

            return GetDynamicType(o).GetIndex(o, index);
        }

        public static void SetIndexId(object o, SymbolId index, object value) {
            IAttributesDictionary ad;
            if ((ad = o as IAttributesDictionary) != null) {
                ad[index] = value;
            } else {
                SetIndex(o, index.ToString(), value);
            }
        }

        public static void SetIndex(object o, object index, object value) {
            IMutableSequence seq = o as IMutableSequence;
            if (seq != null) {
                Slice slice;

                if (index is int) {
                    seq[(int)index] = value;
                    return;
                } else if ((slice = index as Slice)!=null) {
                    if (slice.step == null) {
                        int start, stop;
                        slice.DeprecatedFixed(o, out start, out stop);

                        seq.SetSlice(start, stop, value);
                    } else seq[slice] = value;

                    return;
                }
                //???
            }

            IMapping map = o as IMapping;
            if (map != null) {
                map[index] = value;
                return;
            }

            Array array = o as Array;
            if (array != null) {
                ArrayOps.SetIndex(array, index, value);
                return;
            }

            IList list = o as IList;
            if (list != null) {
                list[Converter.ConvertToInt32(index)] = value;
                return;
            }

            GetDynamicType(o).SetIndex(o, index, value);
        }

        public static void SetIndexStackHelper(object value, object o, object index) {
            SetIndex(o, index, value);
        }

        public static void DelIndex(object o, object index) {
            if (index == null) {
                throw Ops.TypeError("index must be integer or slice");
            }

            IMutableSequence seq = o as IMutableSequence;
            if (seq != null) {
                Slice slice;
                if (index is int) {
                    seq.DeleteItem((int)index);
                    return;
                } else if ((slice = index as Slice)!=null) {
                    if (slice.step == null) {
                        int start, stop;
                        slice.DeprecatedFixed(o, out start, out stop);

                        seq.DeleteSlice(start, stop);
                    } else 
                        seq.DeleteItem((Slice)index);

                    return;
                }
                //???
            }

            IMapping map = o as IMapping;
            if (map != null) {
                map.DeleteItem(index);
                return;
            }

            GetDynamicType(o).DelIndex(o, index);
        }

        public static object GetNamespace(ICallerContext context, Assembly asm, string nameSpace){
            object res;
            if (!Ops.GetDynamicTypeFromType(typeof(Assembly)).TryGetAttr(context, asm, SymbolTable.StringToId(nameSpace), out res)) {
                throw new InvalidOperationException("bad assembly");
            }
            return res;
        }

        public static bool TryGetAttr(object o, SymbolId name, out object ret) {
            return TryGetAttr(DefaultContext.Default, o, name, out ret);
        }

        public static bool TryGetAttr(ICallerContext context, object o, SymbolId name, out object ret) {
            ICustomAttributes ids = o as ICustomAttributes;
            if (ids != null) {
                return ids.TryGetAttr(context, name, out ret);
            }

            return GetDynamicType(o).TryGetAttr(context, o, name, out ret);
        }

        public static object GetAttr(ICallerContext context, object o, SymbolId name) {
            ICustomAttributes ifca = o as ICustomAttributes;
            if (ifca != null) {
                object ret;
                if (ifca.TryGetAttr(context, name, out ret)) return ret;

                if (o.GetType().IsSubclassOf(typeof(UserType))) {
                    // we have an instance of a class that is built w/
                    // a meta-class.  We need to check the metaclasses
                    // properties as well, which ICustomAttrs didn't do.
                    // we'll fall through to GetAttr (we should probably
                    // do special overrides in NewTypeMaker instead)
                } else if (o is DynamicType) {
                    throw Ops.AttributeError("type object '{0}' has no attribute '{1}'",
                        ((DynamicType)o).__name__, SymbolTable.IdToString(name));
                } else {
                    throw Ops.AttributeError("'{0}' object has no attribute '{1}'", GetDynamicType(o).__name__, SymbolTable.IdToString(name));
                }
            }

            // fall through to normal case...
            return GetDynamicType(o).GetAttr(context, o, name);
        }

        public static void SetAttr(ICallerContext context, object o, SymbolId name, object value) {
            ICustomAttributes ids = o as ICustomAttributes;

            if (ids != null) {
                ids.SetAttr(context, name, value);
                return;
            }

            GetDynamicType(o).SetAttr(context, o, name, value);
        }

        public static void SetAttrStackHelper(object value, object o, SymbolId name, ICallerContext context) {
            SetAttr(context, o, name, value);
        }

        public static void DelAttr(ICallerContext context, object o, SymbolId name) {
            ICustomAttributes ifca = o as ICustomAttributes;
            if (ifca != null) {
                ifca.DeleteAttr(context, name);
                return;
            }

            GetDynamicType(o).DelAttr(context, o, name);
        }

        public static List GetAttrNames(ICallerContext context, object o) {
            ICustomAttributes ids = o as ICustomAttributes;

            if (ids != null) {
                return ids.GetAttrNames(context);
            }

            return GetDynamicType(o).GetAttrNames(context, o);
        }

        public static IDictionary<object, object> GetAttrDict(ICallerContext context, object o) {
            ICustomAttributes ids = o as ICustomAttributes;
            if (ids != null) {
                return ids.GetAttrDict(context);
            }
            return GetDynamicType(o).GetAttrDict(context, o);
        }

        public static void CheckInitialized(object o) {
            if (o is Uninitialized) {
                throw Ops.NameError("name '{0}' not defined", ((Uninitialized)o).name);
            }
        }

        public static void CheckInitializedAttribute(object o, object self) {
            if (o is Uninitialized) {
                throw Ops.AttributeError("'{0}' object has no attribute '{1}'", 
                    Ops.GetDynamicType(self), 
                    ((Uninitialized)o).name);
            }
        }

        public static void CheckInitializedLocal(object o) {
            if (o is Uninitialized) {
                throw Ops.UnboundLocalError("local variable '{0}' referenced before assignment", ((Uninitialized)o).name);
            }
        }

        public static object GetDescriptor(object o, object instance, object context) {
            PythonFunction f = o as PythonFunction;
            if (f != null) return f.GetAttribute(instance, context);

            IDescriptor id = o as IDescriptor;
            if (id != null) return id.GetAttribute(instance, context);

            if (!(o is ISuperDynamicObject)) return o;   //???

            // slow, but only encountred for user defined descriptors.
            PerfTrack.NoteEvent(PerfTrack.Categories.DictInvoke, "__delete__");
            object ret;
            if (TryToInvoke(o, SymbolTable.GetDescriptor, out ret, instance, context)) return ret;

            return o;
        }

        public static bool SetDescriptor(object o, object instance, object value) {
            IDataDescriptor id = o as IDataDescriptor;
            if (id != null) {
                return id.SetAttribute(instance, value);
            }

            if (instance == null) return false;

            // slow, but only encountred for user defined descriptors.
            PerfTrack.NoteEvent(PerfTrack.Categories.DictInvoke, "__delete__");
            object ret;
            if (TryToInvoke(o, SymbolTable.SetDescriptor, out ret, instance, value)) return true;

            return false;
        }

        public static bool DelDescriptor(object o, object instance) {
            IDataDescriptor id = o as IDataDescriptor;
            if (id != null) {
                return id.DeleteAttribute(instance);
            }

            if (instance == null) return false;

            // slow, but only encountred for user defined descriptors.
            PerfTrack.NoteEvent(PerfTrack.Categories.DictInvoke, "__delete__");
            object ret;
            if (TryToInvoke(o, SymbolTable.DeleteDescriptor, out ret, instance)) return true;

            return false;
        }

        public static object[] ExtractArrayTail(object[] a0) {
            int N = a0.Length - 1;
            object[] ret = new object[N];
            Array.Copy(a0, 1, ret, 0, N);
            return ret;
        }

        public static object InvokeMethod(object meth, object type, object self) {
            PythonFunction f = meth as PythonFunction;
            if (f != null) return f.Call(self);
            return Ops.Call(Ops.GetDescriptor(meth, self, type));
        }
        public static object InvokeMethod(object meth, object type, object self, object arg0) {
            PythonFunction f = meth as PythonFunction;
            if (f != null) return f.Call(self, arg0);
            return Ops.Call(Ops.GetDescriptor(meth, self, type), arg0);
        }
        public static object InvokeMethod(object meth, object type, object self, object arg0, object arg1) {
            PythonFunction f = meth as PythonFunction;
            if (f != null) return f.Call(self, arg0, arg1);
            return Ops.Call(Ops.GetDescriptor(meth, self, type), arg0, arg1);
        }

        public static object InvokeMethod(object meth, object type, object[] selfAndArgs) {
            PythonFunction f = meth as PythonFunction;
            if (f != null) return f.Call(selfAndArgs);
            object self = selfAndArgs[0];
            object[] args = new object[selfAndArgs.Length - 1];
            Array.Copy(selfAndArgs, 1, args, 0, args.Length);
            return Ops.Call(Ops.GetDescriptor(meth, self, type), args);
        }

        public static Exception MissingInvokeMethodException(object self, string name) {
            return Ops.MakeAttributeError(self, name);
        }

        static Exception MakeAttributeError(object o, string name) {
            if (o is DynamicType) {
                throw Ops.AttributeError("type object '{0}' has no attribute '{1}'",
                    ((DynamicType)o).__name__, name);
            } else {
                throw Ops.AttributeError("'{0}' object has no attribute '{1}'", GetDynamicType(o).__name__, name);
            }
        }

        public static object Invoke(object target, SymbolId name, params object[] args) {
            return Ops.GetDynamicType(target).Invoke(target, name, args);
        }

        public static bool TryToInvoke(object target, SymbolId name, out object ret, params object[] args) {
            return Ops.GetDynamicType(target).TryInvoke(target, name, out ret, args);
        }

        public static bool TryFancyInvoke(object target, SymbolId name, object[] args, string[] names, out object ret) {
            return Ops.GetDynamicType(target).TryFancyInvoke(target, name, args, names, out ret);
        }

        private static void Write(SystemState state, object f, string text) {
            if (f == null) f = state.stdout;
            if (f == null || f is Uninitialized) throw Ops.RuntimeError("lost sys.std_out");
            Ops.Invoke(f, SymbolTable.ConsoleWrite, text);
        }

        private static object ReadLine(object f) {
            if (f == null || f is Uninitialized) throw Ops.RuntimeError("lost sys.std_in");
            return Ops.Invoke(f, SymbolTable.ConsoleReadLine);
        }

        private static void WriteSoftspace(SystemState state, object f) {
            if (CheckSoftspace(f)) {
                SetSoftspace(f, FALSE);
                Write(state, f, " ");
            }
        }

        private static void SetSoftspace(object f, object value) {
            Ops.SetAttr(DefaultContext.Default, f, SymbolTable.Softspace, value);
        }

        private static bool CheckSoftspace(object f) {
            object result;
            if (Ops.TryGetAttr(f, SymbolTable.Softspace, out result)) return Ops.IsTrue(result);
            return false;
        }


        public static void PrintNewline(SystemState state) {
            PrintNewlineWithDest(state, state.stdout);
        }

        public static void PrintNewlineWithDest(SystemState state, object dest) {
            Write(state, dest, "\n");
            SetSoftspace(dest, FALSE);
        }

        public static void Print(SystemState state, object o) {
            PrintWithDest(state, state.stdout, o);
        }

        public static void PrintNoNewline(SystemState state, object o) {
            PrintWithDestNoNewline(state, state.stdout, o);
        }

        public static void PrintComma(SystemState state, object o) {
            PrintCommaWithDest(state, state.stdout, o);
        }

        public static void PrintWithDest(SystemState state, object dest, object o) {
            PrintWithDestNoNewline(state, dest, o);
            Write(state, dest, "\n");
        }

        public static void PrintWithDestNoNewline(SystemState state, object dest, object o) {
            WriteSoftspace(state, dest);
            Write(state, dest, o == null ? "None" : ToString(o));
        }

        public static void PrintCommaWithDest(SystemState state, object dest, object o) {
            WriteSoftspace(state, dest);
            string s = o == null ? "None" : ToString(o);

            Write(state, dest, s);
            SetSoftspace(dest, !s.EndsWith("\n"));
        }

        public static void PrintNotNoneRepr(SystemState state, object o) {
            if (o != null) Print(state, Ops.StringRepr(o));
        }

        public static void PrintRepr(SystemState state, object o) {
            if (o != null) PrintNoNewline(state, Ops.StringRepr(o));
        }

        public static object ReadLineFromSrc(object src) {
            return ReadLine(src);
        }

        // import spam.eggs
        public static object Import(PythonModule mod, string fullName) {
            return Importer.Import(mod, fullName, List.Make());
        }

        // import spam as ham
        public static object ImportAs(PythonModule mod, string fullName, string asName) {
            return Importer.Import(mod, fullName, List.MakeList(asName));
        }

        // from spam import eggs1, eggs2
        public static object ImportFrom(PythonModule mod, string fullName, string[] names) {
            return ImportFromAs(mod, fullName, names, names);
        }

        // from spam import eggs1 as ham1, eggs2 as ham2
        public static object ImportFromAs(PythonModule mod, string fullName, string[] names, string[] asNames) {
            return Importer.Import(mod, fullName, List.Make(asNames));
        }

        public static object ImportOneFrom(ICallerContext context, object fromObj, string name) {
            return Importer.ImportFrom(context, fromObj, name);
        }

        // from spam import *
        public static void ImportStar(PythonModule mod, string fullName) {
            object newmod = Importer.Import(mod, fullName, List.MakeList("*"));

            if (newmod is PythonModule) {
                object all;
                if (((PythonModule)newmod).TryGetAttr(DefaultContext.Default, SymbolTable.All, out all)) {
                    IEnumerator exports = GetEnumerator(all);

                    while (exports.MoveNext()) {
                        string name = exports.Current as string;
                        if (name == null) continue;

                        SymbolId fieldId = SymbolTable.StringToId(name);
                        mod.SetImportedAttr(mod, fieldId, GetAttr(DefaultContext.Default, newmod, fieldId));
                    }
                    return;
                }
            }

            foreach (object o in Ops.GetAttrNames(DefaultContext.Default, newmod)) {
                if (o != null) {
                    if (!(o is string)) throw Ops.TypeErrorForNonStringAttribute();
                    string name = o as string;
                    if (name.Length == 0) continue;
                    if (name[0] == '_') continue;

                    SymbolId fieldId = SymbolTable.StringToId(name);
                    mod.SetImportedAttr(mod, fieldId, GetAttr(DefaultContext.Default, newmod, fieldId));
                }
            }
        }

        #region Pre-compiled code support

        // When pre-compiled code gets loaded it always gets loaded into a single engine, not into a 
        // user-visible engine.
        internal static IronPython.Hosting.PythonEngine compiledEngine;

        public static PythonModule InitializeModule(CompiledModule compiledModule, string fullName, string[] references) {
            if (compiledEngine == null) {
                compiledEngine = new IronPython.Hosting.PythonEngine();

                compiledEngine.Sys.prefix = System.IO.Path.GetDirectoryName(fullName);
                compiledEngine.Sys.executable = fullName;
                compiledEngine.Sys.exec_prefix = compiledEngine.Sys.prefix;

                compiledEngine.AddToPath(Environment.CurrentDirectory);
            }

            if (references != null) {
                for (int i = 0; i < references.Length; i++) {
                    compiledEngine.Sys.ClrModule.AddReference(references[i]);
                }
            }

            compiledEngine.LoadAssembly(compiledModule.GetType().Assembly);
            PythonModule module = compiledModule.Load(fullName, (InitializeModule)null, compiledEngine.Sys);
            compiledEngine.Sys.modules[fullName] = module;
            return module;
        }

        public static int ExecuteCompiled(InitializeModule init) {
            // first arg is EXE 
            List args = new List();
            string[] fullArgs = Environment.GetCommandLineArgs();
            args.Add(Path.GetFullPath(fullArgs[0]));
            for (int i = 1; i < fullArgs.Length; i++)
                args.Add(fullArgs[i]);
            compiledEngine.Sys.argv = args;

            try {
                init();
            } catch (PythonSystemExit x) {
                return x.GetExitCode(compiledEngine.DefaultModuleScope);
            } catch (Exception e) {
                Ops.Write(compiledEngine.Sys, compiledEngine.Sys.stderr, compiledEngine.FormatException(e));
                return -1;
            }
            return 0;
        }

        #endregion

        public static Delegate CreateDynamicDelegate(DynamicMethod meth, Type delegateType, object target) {
            // Always close delegate around its own instance of the frame
            return meth.CreateDelegate(delegateType, target);
        }

        private static object FindMetaclass(ICallerContext context, Tuple bases, IDictionary<object, object> dict) {
            // If dict['__metaclass__'] exists, it is used. 
            object ret;
            if (dict.TryGetValue("__metaclass__", out ret) && ret != null) return ret;

            //Otherwise, if there is at least one base class, its metaclass is used
            for (int i = 0; i < bases.Count; i++) {
                if (!(bases[i] is OldClass)) return Ops.GetDynamicType(bases[i]);
            }

            //Otherwise, if there's a global variable named __metaclass__, it is used.
            if (context.Module.TryGetAttr(context, SymbolTable.MetaClass, out ret) && ret != null) {
                return ret;
            }

            //Otherwise, the classic metaclass (types.ClassType) is used.
            return OldInstanceType.Instance;
        }

        public static object MakeClass(ICallerContext mod, string modName, string name, Tuple bases, IDictionary<object, object> vars) {
            // __module__ is available when we call __metaclass__, so
            // we ensure it's in the dictionary here and flow it through
            // to our internal classes via the dictionary.
            if (!vars.ContainsKey("__module__")) {
                if (modName == null) {
                    vars["__module__"] = "__builtin__";
                } else {
                    vars["__module__"] = modName;
                }
            }

            object metaclass = FindMetaclass(mod, bases, vars);
            if (metaclass == OldInstanceType.Instance)
                return new OldClass(name, bases, vars);
            if (metaclass == TypeCache.ReflectedType || metaclass == TypeCache.UserType)
                return UserType.MakeClass(name, bases, vars);
            if (metaclass is UserType)
                return ((ICallableWithCallerContext)metaclass).Call(mod, new object[] { name, bases, vars });

            // eg:
            // def foo(*args): print args
            // __metaclass__ = foo
            // class bar: pass
            // calls our function...
            return Ops.CallWithContext(mod, metaclass, name, bases, vars);
        }


        public static void Exec(ICallerContext context, object code) {
            IAttributesDictionary locals = null;
            IAttributesDictionary globals = null;

            // if the user passes us a tuple we'll extract the 3 values out of it            
            Tuple codeTuple = code as Tuple;
            if (codeTuple != null && codeTuple.Count > 0 && codeTuple.Count <= 3) {
                code = codeTuple[0];

                if (codeTuple.Count > 1 && codeTuple[1] != null) {
                    globals = codeTuple[1] as IAttributesDictionary;
                    if (globals == null) throw Ops.TypeError("globals must be dictionary or none");
                }

                if (codeTuple.Count > 2 && codeTuple[2] != null) {
                    locals = codeTuple[2] as IAttributesDictionary;
                    if (locals == null) throw Ops.TypeError("locals must be dictionary or none");
                } else if (globals != context.Globals)
                    locals = globals;
            }

            Exec(context, code, globals, locals);
        }

        public static void Exec(ICallerContext context, object code, IAttributesDictionary globals, object locals) {
            if (code is PythonFile) {
                PythonFile pf = code as PythonFile;
                List lines = pf.ReadLines();
                StringBuilder fullCode = new StringBuilder();
                for (int i = 0; i < lines.Count; i++) {
                    fullCode.Append(lines[i]);
                }

                code = fullCode.ToString();
            } else if (code is Stream) {
                Stream cs = code as Stream;
                code = new StreamReader(cs).ReadToEnd();
            }

            if (code is String) {
                CompilerContext cc = context.CreateCompilerContext();
                Parser p = Parser.FromString(context.SystemState, cc, (string)code);
                //  could be multiple statements e.g. exec "\nprint "abc"\nprint "def"\n"
                Stmt s = p.ParseFileInput();
                code = new FunctionCode(OutputGenerator.GenerateSnippet(cc, s));
            }

            FunctionCode fc = code as FunctionCode;
            if (fc == null) {
                throw Ops.TypeError("arg 1 must be a string, file, Stream, or code object");
            }

            if (locals == null) locals = globals;
            if (globals == null) globals = (IAttributesDictionary)context.Globals;
            if (locals == null) locals = (IAttributesDictionary)context.Locals;

            if (locals != null && IronPython.Modules.PythonOperator.IsMappingType(context, locals) != Ops.TRUE) {
                throw Ops.TypeError("exec: arg 3 must be mapping or None");
            }

            fc.Call(new ModuleScope(context.Module, globals, locals));
        }

        public static void Raise() {
            if (SystemState.RawException != null) {
                PerfTrack.NoteEvent(PerfTrack.Categories.Exceptions, SystemState.RawException);
                throw SystemState.RawException;
            } else {
                throw Ops.TypeError("throwing a non-exception type, NoneType");
            }
        }

        public static void Raise(object type, object value, object traceback) {
            // non-reraise.
            // type is the type of exception to throwo or an instance.  If it 
            // is an instance then value should be null.  

            // If type is a type then value can either be an instance of type,
            // a Tuple, or a single value.  This case is handled by EC.CreateThrowable.

            // Currently we ignore the traceback.
            Exception throwable;

            if (traceback != null) {
                TraceBack tb = traceback as TraceBack;
                if (tb == null) throw Ops.TypeError("traceback argument must be a traceback object");
                tb.IsUserSupplied = true;

                SystemState.RawTraceBack = tb;
            }

            if (type is Exception) throwable = type as Exception;
            else if (Builtin.IsInstance(type, Builtin.Exception)) throwable = ExceptionConverter.ToClr(type);
            else if (type is string) throwable = new StringException(type.ToString(), value);
            else if (type is OldClass) {
                if (value == null)
                    throwable = ExceptionConverter.CreateThrowable(type);
                else
                    throwable = ExceptionConverter.CreateThrowable(type, value);
            } else if (type is OldInstance) throwable = ExceptionConverter.ToClr(type);
            else throwable = Ops.TypeError("exceptions must be classes, instances, or strings (deprecated), not {0}", Ops.GetDynamicType(type));

            PerfTrack.NoteEvent(PerfTrack.Categories.Exceptions, throwable);

            throw throwable;
        }

        public static object ExtractException(Exception e, ICallerContext context) {
            context.SystemState.SetRawException(e);
            object res = ExceptionConverter.ToPython(e);
            System.Diagnostics.Debug.Assert(res != null);
            return res;
        }

        public static void ClearException(ICallerContext context) {
            context.SystemState.ClearException();
        }

        public static void UpdateTraceBack(ICallerContext context, string funcName, string filename, int line) {
            TraceBack curTrace = SystemState.RawTraceBack;
            if (curTrace == null || !curTrace.IsUserSupplied) {
                PythonFunction fx = new Function0(context.Module, funcName, null, new string[0], new object[0]);

                TraceBackFrame tbf = new TraceBackFrame(context.Globals, context.Locals, fx.FunctionCode);
                ((FunctionCode)fx.FunctionCode).SetFilename(filename);
                ((FunctionCode)fx.FunctionCode).SetLineNumber(line);
                TraceBack tb = new TraceBack(curTrace, tbf);
                tb.SetLine(line);

                SystemState.RawTraceBack = tb;
            }
        }

        public static object CheckException(object exc, object test) {
            System.Diagnostics.Debug.Assert(exc != null);

            StringException strex;
            if (test is Tuple) {
                // we handle multiple exceptions, we'll check them one at a time.
                Tuple tt = test as Tuple;
                for (int i = 0; i < tt.Count; i++) {
                    object res = CheckException(exc, tt[i]);
                    if (res != null) return res;
                }
            } else if ((strex = exc as StringException) != null) {
                // catching a string
                if (test.GetType() != typeof(string)) return null;
                if (strex.Message == (string)test) {
                    if (strex.Value == null) return strex.Message;
                    return strex.Value;
                }
                return null;
            } else if (test is DynamicType) {
                if (Builtin.IsInstance(exc, test)) {
                    // catching a Python type.
                    return exc;
                } else if (Builtin.IsSubClass(test as DynamicType, Ops.GetDynamicTypeFromType(typeof(Exception)))) {
                    // catching a CLR exception type explicitly.
                    Exception clrEx = ExceptionConverter.ToClr(exc);
                    if (Builtin.IsInstance(clrEx, test)) return clrEx;
                }
            }

            return null;
        }

        public static int FixSliceIndex(int v, int len) {
            if (v < 0) v = len + v;
            if (v < 0) return 0;
            if (v > len) return len;
            return v;
        }

        public static void FixSlice(int length, object start, object stop, object step,
                                    out int ostart, out int ostop, out int ostep, out int ocount) {
            if (step == null) {
                ostep = 1;
            } else {
                ostep = Converter.ConvertToSliceIndex(step);
                if (ostep == 0) {
                    throw Ops.ValueError("step cannot be zero");
                }
            }

            if (start == null) {
                ostart = ostep > 0 ? 0 : length - 1;
            } else {
                ostart = Converter.ConvertToSliceIndex(start);
                if (ostart < 0) {
                    ostart += length;
                    if (ostart < 0) {
                        ostart = ostep > 0 ? Math.Min(length, 0) : Math.Min(length - 1, -1);
                    }
                } else if (ostart >= length) {
                    ostart = ostep > 0 ? length : length - 1;
                }
            }

            if (stop == null) {
                ostop = ostep > 0 ? length : -1;
            } else {
                ostop = Converter.ConvertToSliceIndex(stop);
                if (ostop < 0) {
                    ostop += length;
                    if (ostop < 0) {
                        ostop = Math.Min(length, -1);
                    }
                } else if (ostop > length) {
                    ostop = length;
                }
            }

            ocount = ostep > 0 ? (ostop - ostart + ostep - 1) / ostep
                               : (ostop - ostart + ostep + 1) / ostep;
        }

        public static int FixIndex(int v, int len) {
            if (v < 0) {
                v += len;
                if (v < 0) {
                    throw Ops.IndexError("index out of range: {0}", v - len);
                }
            } else if (v >= len) {
                throw Ops.IndexError("index out of range: {0}", v);
            }
            return v;
        }

        #region CLS Compatible exception factories

        public static Exception ValueError(string format, params object[] args) {
            return new ArgumentException(string.Format(format, args));
        }

        public static Exception KeyError(string format, params object[] args) {
            return new KeyNotFoundException(string.Format(format, args));
        }

        public static Exception StopIteration(string format, params object[] args) {
            return new StopIterationException(string.Format(format, args));
        }

        public static Exception UnicodeEncodeError(string format, params object[] args) {
            return new System.Text.DecoderFallbackException(string.Format(format, args));
        }

        public static Exception UnicodeDecodeError(string format, params object[] args) {
            return new System.Text.EncoderFallbackException(string.Format(format, args));
        }

        public static Exception IOError(string format, params object[] args) {
            return new System.IO.IOException(string.Format(format, args));
        }

        public static Exception EofError(string format, params object[] args) {
            return new System.IO.EndOfStreamException(string.Format(format, args));
        }

        public static Exception StandardError(string format, params object[] args) {
            return new SystemException(string.Format(format, args));
        }

        public static Exception ZeroDivisionError(string format, params object[] args) {
            return new DivideByZeroException(string.Format(format, args));
        }

        public static Exception SystemError(string format, params object[] args) {
            return new SystemException(string.Format(format, args));
        }

        public static Exception TypeError(string format, params object[] args) {
            return new ArgumentTypeException(string.Format(format, args));
        }

        public static Exception IndexError(string format, params object[] args) {
            return new System.IndexOutOfRangeException(string.Format(format, args));
        }

        public static Exception MemoryError(string format, params object[] args) {
            return new OutOfMemoryException(string.Format(format, args));
        }

        public static Exception ArithmeticError(string format, params object[] args) {
            return new ArithmeticException(string.Format(format, args));
        }

        public static Exception NotImplementedError(string format, params object[] args) {
            return new NotImplementedException(string.Format(format, args));
        }

        public static Exception AttributeError(string format, params object[] args) {
            return new MissingMemberException(string.Format(format, args));
        }

        public static Exception OverflowError(string format, params object[] args) {
            return new System.OverflowException(string.Format(format, args));
        }

        public static Exception WindowsError(string format, params object[] args) {
            return new System.ComponentModel.Win32Exception(string.Format(format, args));
        }

        public static Exception SystemExit(string format, params object[] args) {
            return new PythonSystemExit(string.Format(format, args));
        }

        public static Exception SyntaxError(string msg, string filename, int line, int column, string lineText, int errorCode, Hosting.Severity severity) {
            return new PythonSyntaxError(msg, filename, line, column, lineText, errorCode, severity);
        }

        public static Exception IndentationError(string msg, string filename, int line, int column, string lineText, int errorCode, Hosting.Severity severity) {
            return new PythonIndentationError(msg, filename, line, column, lineText, errorCode, severity);
        }

        public static Exception TabError(string msg, string filename, int line, int column, string lineText, int errorCode, Hosting.Severity severity) {
            return new PythonTabError(msg, filename, line, column, lineText, errorCode, severity);
        }


        #endregion


        public static Exception StopIteration() {
            return StopIteration("");
        }

        public static Exception AssertionError(string message) {
            return AssertionError(message != null ? message : String.Empty, new object[0]);
        }

        public static Exception InvalidType(object o, RuntimeTypeHandle handle) {
            System.Diagnostics.Debug.Assert(Options.GenerateSafeCasts);
            Type type = Type.GetTypeFromHandle(handle);
            return TypeError("Object {0} is not of type {1}", o == null ? "None" : o, type);
        }

        public static Exception ZeroDivisionError() {
            return ZeroDivisionError("Attempted to divide by zero.");
        }

        // If you do "(a, b) = (1, 2, 3, 4)"
        public static Exception ValueErrorForUnpackMismatch(int left, int right) {
            System.Diagnostics.Debug.Assert(left != right);

            if (left > right)
                return ValueError("need more than {0} values to unpack", right);
            else
                return ValueError("too many values to unpack");
        }

        public static Exception TypeErrorForBuiltinAttributeChange(string typeName) {
            throw Ops.TypeError("can't set attributes of built-in/extension type '{0}'", typeName);
        }

        // If an unbound method is called without a "self" argument, or a "self" argument of a bad type
        public static Exception TypeErrorForUnboundMethodCall(string methodName, Type methodType, object instance) {
            return TypeErrorForUnboundMethodCall(methodName, GetDynamicTypeFromType(methodType), instance);
        }

        public static Exception TypeErrorForUnboundMethodCall(string methodName, DynamicType methodType, object instance) {
            string message = string.Format("unbound method {0}() must be called with {1} instance as first argument (got {2} instead)",
                                           methodName, methodType, GetDynamicType(instance));
            return TypeError(message);
        }

        // If a method is called with an incorrect number of arguments
        // You should use TypeErrorForUnboundMethodCall() for unbound methods called with 0 arguments
        public static Exception TypeErrorForArgumentCountMismatch(string methodName, int expectedArgCount, int actualArgCount) {
            return TypeError("{0}() takes exactly {1} argument{2} ({3} given)",
                             methodName, expectedArgCount, expectedArgCount == 1 ? "" : "s", actualArgCount);
        }

        // If hash is called on an instance of an unhashable type
        // We have 2 versions to maintain compatibility with the 2 messages that CPython uses.
        public static Exception TypeErrorForUnhashableType(string typeName) {
            return TypeError(typeName + " objects are unhashable");
        }

        public static Exception TypeErrorForUnhashableType() {
            return TypeError("unhashable type");
        }

        internal static Exception TypeErrorForIncompatibleObjectLayout(string prefix, PythonType type, Type newType) {
            return TypeError("{0}: '{1}' object layout differs from '{2}'", prefix, type.__name__, newType);
        }

        public static Exception TypeErrorForNonStringAttribute() {
            return TypeError("attribute name must be string");
        }

        public static Exception AttributeErrorForReadonlyAttribute(string typeName, SymbolId attributeName) {
            return Ops.AttributeError("attribute '{0}' of '{1}' object is read-only", attributeName.ToString(), typeName);
        }

        public static Exception AttributeErrorForMissingAttribute(string typeName, SymbolId attributeName) {
            return Ops.AttributeError("'{0}' object has no attribute '{1}'", typeName, attributeName.ToString());
        }
    }
}
