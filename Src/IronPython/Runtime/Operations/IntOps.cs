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
using System.Text;
using System.Collections;
using System.Reflection;
using System.Threading;

using IronPython.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Types;
using IronPython.Compiler;
using IronMath;

namespace IronPython.Runtime.Operations {

    public partial class ExtensibleInt : IRichComparable, IComparable, INumber {
        public int value;
        public ExtensibleInt() { this.value = 0; }
        public ExtensibleInt(int _value) { this.value = _value; }

        public override string ToString() {
            return value.ToString();
        }

        [PythonName("__cmp__")]
        public virtual object Compare(object other) {
            return IntOps.Compare(value, other);
        }

        #region IComparable Members

        public int CompareTo(object obj) {
            object res = CompareToWorker(obj);
            Conversion conv;

            int iRes = Converter.TryConvertToInt32(res, out conv);
            if (conv != Conversion.None) return iRes;

            throw Ops.TypeErrorForBadInstance("cannot compare {0} to int", obj);
        }

        #endregion

        #region IRichComparable Members

        object IRichEquality.RichEquals(object other) {
            return IntOps.Equals(value, other);
        }

        object IRichEquality.RichNotEquals(object other) {
            object res = IntOps.Equals(value, other);
            if (res != Ops.NotImplemented) return Ops.Not(res);

            return Ops.NotImplemented;
        }

        object IRichEquality.RichGetHashCode() {
            return Ops.Int2Object(GetHashCode());
        }

        object IRichComparable.CompareTo(object other) {
            return CompareToWorker(other);
        }

        private object CompareToWorker(object other) {
            return IntOps.Compare(value, other);
        }

        public object GreaterThan(object other) {
            object res = CompareToWorker(other);
            if (res is int) {
                return ((int)res) > 0;
            }
            return Ops.NotImplemented;
        }

        public object LessThan(object other) {
            object res = CompareToWorker(other);
            if (res is int) {
                return ((int)res) < 0;
            }
            return Ops.NotImplemented;
        }

        public object GreaterThanOrEqual(object other) {
            object res = CompareToWorker(other);
            if (res is int) {
                return ((int)res) >= 0;
            }
            return Ops.NotImplemented;
        }

        public object LessThanOrEqual(object other) {
            object res = CompareToWorker(other);
            if (res is int) {
                return ((int)res) <= 0;
            }
            return Ops.NotImplemented;
        }

        #endregion

        [PythonName("__hash__")]
        public override int GetHashCode() {
            return value.GetHashCode();
        }
    }

    public static partial class IntOps {
        static ReflectedType IntType;
        public static ReflectedType MakeDynamicType() {
            if (IntType == null) {
                OpsReflectedType ret = new OpsReflectedType("int", typeof(int), typeof(IntOps), typeof(ExtensibleInt), new CallTarget1(FastNew));
                if (Interlocked.CompareExchange<ReflectedType>(ref IntType, ret, null) == null)
                    return ret;
            }
            return IntType;
        }

        private static object FastNew(object o) {
            ExtensibleLong el;

            if (o is string) return Make(null, (string)o, 10);
            if (o is double) return FloatOps.ToInteger((double)o);
            if (o is int) return o;
            if (o is BigInteger) return o;
            if ((el = o as ExtensibleLong)!=null) return el.Value;
            if (o is float) return FloatOps.ToInteger((double)(float)o);

            if (o is Complex64) throw Ops.TypeError("can't convert complex to int; use int(abs(z))");

            if (o is Byte) return (Int32)(Byte)o;
            if (o is SByte) return (Int32)(SByte)o;
            if (o is Int16) return (Int32)(Int16)o;
            if (o is Int64) {
                Int64 val = (Int64)o;
                if (Int32.MinValue<= val && val <= Int32.MaxValue) {
                    return (Int32)val;
                } else {
                    return BigInteger.Create(val);
                }
            }
            if (o is UInt16) return (Int32)(UInt16)o;

            if (o is UInt32) {
                UInt32 val = (UInt32)o;
                if (val < Int32.MaxValue) {
                    return (Int32)val;
                } else {
                    return BigInteger.Create(val);
                }
            }
            if (o is UInt64) {
                UInt64 val = (UInt64)o;
                if (val < Int32.MaxValue) {
                    return (Int32)val;
                } else {
                    return BigInteger.Create(val);
                }
            }

            return Converter.ConvertToInt32(o);
        }

        public static object Make(object o) {
            return Make(TypeCache.Int32, o);
        }

        private static void ValidateType(PythonType cls) {
            if (cls == BoolOps.BoolType)
                throw Ops.TypeError("int.__new__(bool) is not safe, use bool.__new__()");
        }

        [PythonName("__new__")]
        public static object Make(PythonType cls, string s, int radix) {
            ValidateType(cls);

            try {
                return LiteralParser.ParseIntegerSign(s, radix);
            } catch (ArgumentException e) {
                throw Ops.ValueError(e.Message);
            }
        }

        [PythonName("__new__")]
        public static object Make(PythonType cls, object x) {
            if (cls == IntType) {
                return FastNew(x);
            } else {
                ValidateType(cls);

                // derived int creation...
                return cls.ctor.Call(cls, x);
            }
        }

        // "int()" calls ReflectedType.Call(), which calls "Activator.CreateInstance" and return directly.
        // this is for derived int creation or direct calls to __new__...
        [PythonName("__new__")]
        public static object Make(PythonType cls) {
            if (cls == TypeCache.Int32) return 0;

            return cls.ctor.Call(cls);
        }

        [PythonName("__cmp__")]
        public static object Compare(int self, object obj) {
            if (obj == null) return 1;

            int otherInt;

            if (obj is int) {
                otherInt = (int)obj;
            } else if (obj is ExtensibleInt) {
                otherInt = ((ExtensibleInt)obj).value;
            } else if (obj is bool) {
                otherInt = ((bool)obj) ? 1 : 0;
            } else if (obj is double) {
                // compare as double to avoid truncation issues
                return FloatOps.Compare((double)self, (double)obj);
            } else if (obj is ExtensibleFloat) {
                // compare as double to avoid truncation issues
                return FloatOps.Compare((double)self, ((ExtensibleFloat)obj).value);
            } else if (obj is Decimal) {
                return FloatOps.Compare((double)self, (double)(decimal)obj);
            } else {
                Conversion conv;
                otherInt = Converter.TryConvertToInt32(obj, out conv);
                if (conv == Conversion.None) {
                    object res = Ops.GetDynamicType(obj).Coerce(obj, self);
                    if (res != Ops.NotImplemented && !(res is OldInstance)) {
                        return Ops.Compare(((Tuple)res)[1], ((Tuple)res)[0]);
                    }
                    return Ops.NotImplemented;
                }
            }

            return self == otherInt ? 0 : (self < otherInt ? -1 : +1);
        }

        [PythonName("__abs__")]
        public static object Abs(int x) {
            return Math.Abs(x);
        }

        private static object TrueDivide(int x, double y) {
            if (y == 0.0) {
                throw new DivideByZeroException();
            }
            return (double)x / y;
        }

        private static object TrueDivide(int x, Complex64 y) {
            return ComplexOps.TrueDivide(Complex64.MakeReal(x), y);
        }

        [PythonName("__powmod__")]
        public static int PowerMod(int x, int power, int mod) {
            if (power < 0) throw Ops.TypeError("power", power, "power must be >= 0");

            if (mod == 0) {
                throw Ops.ZeroDivisionError();
            }

            long result = 1;
            if (power > 0) {
                long factor = x;
                while (power != 0) {
                    if ((power & 1) != 0) result = result * factor % mod;
                    factor = factor * factor % mod; //???
                    power >>= 1;
                }
            }

            if (result >= 0) {
                if (mod < 0) return (int)(result + mod);
            } else {
                if (mod > 0) return (int)(result + mod);
            }
            return (int)result;
        }

        [PythonName("__powmod__")]
        public static object PowerMod(int x, int y, object z) {
            ExtensibleLong el;
            if (z is int) {
                return PowerMod(x, y, (int)z);
            } else if (z is long) {
                return Int64Ops.PowerMod(x, y, (long)z);
            } else if (z is BigInteger) {
                return LongOps.PowerMod(BigInteger.Create(x), BigInteger.Create(y), (BigInteger)z);
            } else if ((el = z as ExtensibleLong)!=null) {
                return LongOps.PowerMod(BigInteger.Create(x), BigInteger.Create(y), el.Value);
            }
            return Ops.NotImplemented;
        }

        [PythonName("__powmod__")]
        public static object PowerMod(int x, object y, object z) {
            if (y is int) {
                return PowerMod(x, (int)y, z);
            } else if (y is long) {
                return Int64Ops.PowerMod(x, y, z);
            } else if (y is BigInteger) {
                return LongOps.PowerMod(x, y, z);
            }
            return Ops.NotImplemented;
        }

        internal static object Power(int x, int power) {
            if (power == 0) return 1;
            if (power < 0) {
                if (x == 0)
                    throw Ops.ZeroDivisionError("0.0 cannot be raised to a negative power");
                return FloatOps.Power(x, power);
            }
            int factor = x;
            int result = 1;
            int savePower = power;
            try {
                checked {
                    while (power != 0) {  //??? this loop has redundant checks for exit condition
                        if ((power & 1) != 0) result = result * factor;
                        if (power == 1) break;
                        factor = factor * factor;
                        power >>= 1;
                    }
                    return result;
                }
            } catch (OverflowException) {
                return LongOps.Power(BigInteger.Create(x), savePower);
            }
        }

        [PythonName("__div__")]
        private static int Divide(int x, int y) {
            int q = checked(x / y);

            if (x >= 0) {
                if (y > 0) return q;
                else if (x % y == 0) return q;
                else return q - 1;
            } else {
                if (y > 0) {
                    if (x % y == 0) return q;
                    else return q - 1;
                } else return q;
            }
        }

        private static int ReverseDivide(int x, int y) {
            return Divide(y, x);
        }

        private static object ReverseDivide(int x, long y) {
            return Int64Ops.Divide(y, x);
        }

        [PythonName("__div__")]
        private static long Divide(int x, long y) {
            long q = checked(x / y);

            if (x >= 0) {
                if (y > 0) return q;
                else if (x % y == 0) return q;
                else return q - 1;
            } else {
                if (y > 0) {
                    if (x % y == 0) return q;
                    else return q - 1;
                } else return q;
            }
        }

        [PythonName("__mod__")]
        private static int Mod(int x, int y) {
            int r = checked(x % y);

            if (x >= 0) {
                if (y > 0) return r;
                else if (r == 0) return 0;
                else return r + y;
            } else {
                if (y > 0) {
                    if (r == 0) return r;
                    else return r + y;
                } else return r;
            }
        }

        [PythonName("__mod__")]
        private static long Mod(int x, long y) {
            long r = checked(x % y);

            if (x >= 0) {
                if (y > 0) return r;
                else if (r == 0) return 0;
                else return r + y;
            } else {
                if (y > 0) {
                    if (r == 0) return r;
                    else return r + y;
                } else return r;
            }
        }

        private static int ReverseMod(int x, int y) {
            return Mod(y, x);
        }

        private static object ReverseMod(int x, long y) {
            return Int64Ops.Mod(y, x);
        }

        [PythonName("__eq__")]
        public static object Equals(int x, object other) {
            bool res;
            if (TryEquals(x, other, out res)) return Ops.Bool2Object(res);

            return Ops.NotImplemented;
        }

        public static bool EqualsRetBool(int x, object other) {
            bool res;
            if (TryEquals(x, other, out res)) return res;

            return Ops.DynamicEqualRetBool(x, other);
        }

        private static bool TryEquals(int x, object other, out bool res) {
            ExtensibleLong el;

            if (other is int) {
                res = x == (int)other;
                return true;
            } else if (other is long) {
                res = x == (long)other;
                return true;
            } else if (other is double) {
                res = x == (double)other;
                return true;
            } else if (other is BigInteger) {
                res = (BigInteger)x == (BigInteger)other;
                return true;
            } else if (other is Complex64) {
                res = x == (Complex64)other;
                return true;
            } else if (other is float) {
                res = x == (float)other;
                return true;
            } else if (other is byte) {
                res = x == (int)(byte)other;
                return true;
            } else if (other is bool) {
                res = (bool)other ? x == 1 : x == 0;
                return true;
            } else if (other is sbyte) {
                res = x == (int)(sbyte)other;
                return true;
            } else if (other is short) {
                res = x == (int)(short)other;
                return true;
            } else if (other is ushort) {
                res = x == (int)(ushort)other;
                return true;
            } else if (other is Decimal) { 
                res = x == (decimal)other;
                return true;
            } else if (other is ExtensibleFloat) { 
                res = x == ((ExtensibleFloat)other).value;
                return true;
            }  else if (other is ExtensibleComplex) { 
                res = x == ((ExtensibleComplex)other).value;
                return true;
            } else if ((el = other as ExtensibleLong)!=null) {
                res = (BigInteger)x == el.Value;
                return true;
            } else if (other is uint) {
                uint val = (uint)other;
                if (val < Int32.MaxValue)
                    res = x == (int)val;
                else
                    res = false;
                return true;
            } else if (other == null) {
                res = false;
                return true;
            }

            Conversion conversion;
            int y = Converter.TryConvertToInt32(other, out conversion);
            if (conversion != Conversion.None) {
                res = x == y;
                return true;
            }

            object coerce = Ops.GetDynamicType(other).Coerce(other, x);
            if (coerce != Ops.NotImplemented && !(coerce is OldInstance)) {
                res = Ops.EqualRetBool(((Tuple)coerce)[1], ((Tuple)coerce)[0]);
                return true;
            }
            res = false;
            return false;
        }

        [PythonName("__neg__")]
        public static object Negate(int x) {
            try {
                return checked(-x);
            } catch (OverflowException) {
                return LongOps.Negate(x);
            }
        }

        [PythonName("__pos__")]
        public static object Positive(int x) {
            return x;
        }

        [PythonName("__invert__")]
        public static object Invert(int x) {
            return ~x;
        }

        [PythonName("__lshift__")]
        public static object LeftShift(int x, object other) {
            ExtensibleLong el;
            ExtensibleInt ei;

            if (other is int) {
                return LeftShift(x, (int)other);
            } else if (other is long) {
                return Int64Ops.LeftShift((long)x, other);
            } else if (other is BigInteger) {
                return LongOps.LeftShift(BigInteger.Create(x), other);
            } else if (other is bool) {
                return LeftShift(x, (bool)other ? 1 : 0);
            } else if ((ei = other as ExtensibleInt) != null) {
                return ei.ReverseLeftShift(x);
            } else if (other is byte) {
                return LeftShift(x, (byte)other);
            } else if ((el = other as ExtensibleLong)!=null) {
                return el.ReverseLeftShift(BigInteger.Create(x));
            }
            return Ops.NotImplemented;
        }

        internal static object LeftShift(int x, int y) {
            if (y < 0) {
                throw Ops.ValueError("negative shift count");
            }
            if (y > 31 ||
                (x > 0 && x > (Int32.MaxValue >> y)) ||
                (x < 0 && x < (Int32.MinValue >> y))) {
                return Int64Ops.LeftShift((long)x, y);
            }
            return Ops.Int2Object(x << y);
        }


        [PythonName("__rshift__")]
        public static object RightShift(int x, object other) {
            ExtensibleLong el;
            ExtensibleInt ei;

            if (other is int) {
                return RightShift(x, (int)other);
            } else if (other is BigInteger) {
                return LongOps.RightShift(BigInteger.Create(x), other);
            } else if (other is bool) {
                return RightShift(x, (bool)other ? 1 : 0);
            } else if (other is long) {
                long y = (long)other;
                if (y < 0) {
                    throw Ops.ValueError("negative shift count");
                }
                if (y >= Int32.MaxValue) {
                    return x > 0 ? 0 : 1;
                }
                return RightShift(x, (int)y);
            } else if ((ei = other as ExtensibleInt)!= null) {
                return ei.ReverseRightShift(x);
            } else if (other is byte) {
                return RightShift(x, (byte)other);
            } else if ((el = other as ExtensibleLong)!=null) {
                return el.ReverseRightShift(BigInteger.Create(x));
            }

            return Ops.NotImplemented;
        }

        internal static object RightShift(int x, int y) {
            if (y < 0) {
                throw Ops.ValueError("negative shift count");
            }
            if (y > 31) {
                return x >= 0 ? 0 : -1;
            }

            int q;

            if (x >= 0) {
                q = x >> y;
            } else {
                q = (x + ((1 << y) - 1)) >> y;
                int r = x - (q << y);
                if (r != 0) q--;
            }

            return Ops.Int2Object(q);
        }

        [PythonName("__pow__")]
        public static object Power(int x, object other) {
            INumber num;
            ExtensibleComplex ec;

            if (other is int) {
                return Power(x, (int)other);
            } else if (other is BigInteger) {
                BigInteger lexp = (BigInteger)other;
                int iexp;
                if (lexp.AsInt32(out iexp)) {
                    return Power(x, iexp);
                } else {
                    if (x == 0) return 0;
                    if (x == 1) return 1;
                    throw Ops.ValueError("number too big");
                }
            } else if (other is long) {
                long lexp = (long)other;
                int iexp = (int)lexp;
                if (lexp == iexp) {
                    return Power(x, iexp);
                } else {
                    if (x == 0) return 0;
                    if (x == 1) return 1;
                    throw Ops.ValueError("Number too big");
                }
            } else if (other is double) {
                return FloatOps.Power(x, (double)other);
            } else if (other is bool) {
                return Power(x, (bool)other ? 1 : 0);
            } else if (other is float) {
                return FloatOps.Power(x, (float)other);
            } else if (other is Complex64) {
                return ComplexOps.Power(x, other);
            } else if (other is byte) {
                return Power(x, (int)(byte)other);
            } else if ((num = other as INumber)!=null) {
                return num.ReversePower(x);
            } else if ((ec = other as ExtensibleComplex) != null) {
                return ec.ReversePower(x);
            }            
            return Ops.NotImplemented;
        }

        [PythonName("__rpow__")]
        public static object ReversePower(int x, object other) {
            ExtensibleLong el;

            if (other is int) {
                return Power((int)other, x);
            } else if (other is BigInteger) {
                BigInteger lexp = (BigInteger)other;
                return LongOps.Power(lexp, x);
            } else if (other is long) {
                return Int64Ops.Power((long)(other), x);
            } else if (other is double) {
                return FloatOps.Power((double)other, x);
            } else if (other is bool) {
                return Power((bool)other ? 1 : 0, x);
            } else if (other is float) {
                return FloatOps.Power((float)other, x);
            } else if (other is Complex64) {
                return ComplexOps.Power((Complex64)other, x);
            } else if (other is byte) {
                return Power((int)(byte)other, x);
            } else if (other is ExtensibleInt) {
                return Power(((ExtensibleInt)other).value, x);
            } else if (other is ExtensibleFloat) {
                return FloatOps.Power(((ExtensibleFloat)other).value, x);
            } else if (other is ExtensibleComplex) {
                return ComplexOps.Power(((ExtensibleComplex)other).value, x);
            } else if ((el = other as ExtensibleLong)!=null) {
                return LongOps.Power(el.Value, x);
            }
            return Ops.NotImplemented;
        }

        [PythonName("__rtruediv__")]
        public static object ReverseTrueDivide(int x, object other){
            ExtensibleLong el;

            if (other is int) {
                return TrueDivide((int)other, x);
            } else if (other is BigInteger) {
                BigInteger lexp = (BigInteger)other;
                return LongOps.TrueDivide(lexp, x);
            } else if (other is long) {
                return Int64Ops.TrueDivide((long)(other), x);
            } else if (other is double) {
                return FloatOps.TrueDivide((double)other, x);
            } else if (other is bool) {
                return TrueDivide((bool)other ? 1 : 0, x);
            } else if (other is float) {
                return FloatOps.TrueDivide((float)other, x);
            } else if (other is Complex64) {
                return ComplexOps.TrueDivide((Complex64)other, x);
            } else if (other is byte) {
                return TrueDivide((int)(byte)other, x);
            } else if (other is ExtensibleInt) {
                return TrueDivide(((ExtensibleInt)other).value, x);
            } else if (other is ExtensibleFloat) {
                return FloatOps.TrueDivide(((ExtensibleFloat)other).value, x);
            } else if (other is ExtensibleComplex) {
                return ComplexOps.TrueDivide(((ExtensibleComplex)other).value, x);
            } else if ((el = other as ExtensibleLong) != null) {
                return LongOps.TrueDivide(el.Value, x);
            }
            return Ops.NotImplemented;
        }

        [PythonName("__rlshift__")]
        public static object ReverseLeftShift(int x, object other){
            ExtensibleLong el;

            if (other is int) {
                return LeftShift((int)other, x);
            } else if (other is BigInteger) {
                BigInteger lexp = (BigInteger)other;
                return LongOps.LeftShift(lexp, x);
            } else if (other is long) {
                return Int64Ops.LeftShift((long)(other), x);
            } else if (other is bool) {
                return LeftShift((bool)other ? 1 : 0, x);
            } else if (other is byte) {
                return LeftShift((int)(byte)other, x);
            } else if (other is ExtensibleInt) {
                return LeftShift(((ExtensibleInt)other).value, x);
            } else if ((el = other as ExtensibleLong) != null) {
                return LongOps.LeftShift(el.Value, x);
            }
            return Ops.NotImplemented;
        }

        [PythonName("__rrshift__")]
        public static object ReverseRightShift(int x, object other){
            ExtensibleLong el;

            if (other is int) {
                return RightShift((int)other, x);
            } else if (other is BigInteger) {
                BigInteger lexp = (BigInteger)other;
                return LongOps.RightShift(lexp, x);
            } else if (other is long) {
                return Int64Ops.RightShift((long)(other), x);
            } else if (other is bool) {
                return RightShift((bool)other ? 1 : 0, x);
            } else if (other is byte) {
                return RightShift((int)(byte)other, x);
            } else if (other is ExtensibleInt) {
                return RightShift(((ExtensibleInt)other).value, x);
            } else if ((el = other as ExtensibleLong) != null) {
                return LongOps.RightShift(el.Value, x);
            }
            return Ops.NotImplemented;
        }

        [PythonName("__radd__")]
        public static object ReverseAdd(int x, object y) {
            return Add(x, y);
        }

        [PythonName("__rsub__")]
        public static object ReverseSubtract(int x, object y) {
            return Ops.Add(-x, y);
        }

        [PythonName("__rmul__")]
        public static object ReverseMultiply(int x, object y) {
            return Multiply(x, y);
        }

        [PythonName("__rand__")]
        public static object ReverseBitwiseAnd(int x, object y){
            return BitwiseAnd(x, y);
        }

        [PythonName("__ror__")]
        public static object ReverseBitwiseOr(int x, object y){
            return BitwiseOr(x, y);
        }

        [PythonName("__rxor__")]
        public static object ReverseXor(int x, object y) {
            return Xor(x, y);
        }

        [PythonName("__oct__")]
        public static string Oct(int x) {
            if (x == 0) {
                return "0";
            } else if (x > 0) {
                return "0" + BigInteger.Create(x).ToString(8);
            } else {
                return "-0" + BigInteger.Create(-x).ToString(8);
            }
        }

        [PythonName("__hex__")]
        public static string Hex(int x) {
            if (x < 0) {
                return "-0x" + (-x).ToString("X");
            } else {
                return "0x" + x.ToString("X");
            }
        }

        [PythonName("__nonzero__")]
        public static bool NonZero(int x) {
            return (x != 0);
        }

        internal static object DivMod(int x, int y) {
            try {
                return Tuple.MakeTuple(Divide(x, y), Mod(x, y));
            } catch (OverflowException) {
                return Int64Ops.DivMod(x, y);
            }
        }
        internal static object ReverseDivMod(int x, int y) {
            return DivMod(y, x);
        }

        internal static object ReversePower(int x, int y) {
            return Power(y, x);
        }
        internal static object ReverseLeftShift(int x, int y) {
            return LeftShift(y, x);
        }
        internal static object ReverseRightShift(int x, int y) {
            return RightShift(y, x);
        }
    }
}