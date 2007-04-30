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
using System.Diagnostics;
using System.Globalization;
using System.Threading;

using IronMath;
using IronPython.Runtime;
using IronPython.Runtime.Types;
using IronPython.Runtime.Calls;

namespace IronPython.Runtime.Operations {
    public partial class ExtensibleFloat : IRichComparable, IComparable, INumber {
        public double value;

        public ExtensibleFloat() { this.value = 0; }
        public ExtensibleFloat(double value) { this.value = value; }

        public override string ToString() {
            return FloatOps.ToString(value);
        }

        [PythonName("__cmp__")]
        public virtual object Compare(object other) {
            double val;
            if (Converter.TryConvertToDouble(other, out val)) {
                if (val == value) return 0;
                if (value < val) return -1;
                return 1;
            }
            return Ops.NotImplemented;
        }

        #region IComparable Members

        int IComparable.CompareTo(object obj) {
            object res = Compare(obj);
            if (res == Ops.NotImplemented) throw Ops.TypeErrorForBadInstance("cannot compare {0} to float", obj);
            return (int)res;
        }

        #endregion

        #region IRichComparable Members

        public object CompareTo(object other) {
            if (other == null) return 1;

            if (other is float) {
                return FloatOps.Compare(value, (int)other);
            } else if (other is ExtensibleFloat) {
                return FloatOps.Compare(value, ((ExtensibleFloat)other).value);
            } else if (other is bool) {
                return FloatOps.Compare(value, ((bool)other) ? 1 : 0);
            } else if (other is int) {
                return FloatOps.Compare(value, (double)((int)other));
            } else if (other is ExtensibleInt) {
                return FloatOps.Compare(value, (double)((ExtensibleInt)other).value);
            } else {
                double otherDbl;
                if (Converter.TryConvertToDouble(other, out otherDbl)) return FloatOps.Compare(value, otherDbl);
            }

            return Ops.NotImplemented;
        }

        public object GreaterThan(object other) {
            object res = CompareTo(other);
            if (res is int) {
                return ((int)res) > 0;
            }
            return Ops.NotImplemented;
        }

        public object LessThan(object other) {
            object res = CompareTo(other);
            if (res is int) {
                return ((int)res) < 0;
            }
            return Ops.NotImplemented;
        }

        public object GreaterThanOrEqual(object other) {
            object res = CompareTo(other);
            if (res is int) {
                return ((int)res) >= 0;
            }
            return Ops.NotImplemented;
        }

        public object LessThanOrEqual(object other) {
            object res = CompareTo(other);
            if (res is int) {
                return ((int)res) <= 0;
            }
            return Ops.NotImplemented;
        }

        #endregion

        #region IRichComparable

        [PythonName("__hash__")]
        public object RichGetHashCode() {
            return Ops.NotImplemented;
        }

        [PythonName("__eq__")]
        public object RichEquals(object other) {
            ExtensibleFloat ei = other as ExtensibleFloat;
            if (ei != null) return Ops.Bool2Object(value == ei.value);
            if (other is double) return Ops.Bool2Object(value == (double)other);
            if (other is float) return Ops.Bool2Object(value == (float)other);

            return Ops.NotImplemented;
        }

        [PythonName("__ne__")]
        public object RichNotEquals(object other) {
            object res = RichEquals(other);
            if (res != Ops.NotImplemented) return Ops.Not(res);

            return Ops.NotImplemented;
        }

        #endregion

        [PythonName("__hash__")]
        public override int GetHashCode() {
            return Ops.Hash(value);
        }
    }

    public static partial class FloatOps {
        static ReflectedType FloatType;
        public static ReflectedType MakeDynamicType() {
            if (FloatType == null) {
                ReflectedType res = new OpsReflectedType("float", typeof(double), typeof(FloatOps), typeof(ExtensibleFloat));
                if (Interlocked.CompareExchange<ReflectedType>(ref FloatType, res, null) == null)
                    return res;
            }
            return FloatType;
        }

        [PythonName("__new__")]
        public static object Make(DynamicType cls) {
            if (cls == FloatType) return 0.0;

            return cls.ctor.Call(cls);
        }

        [PythonName("__new__")]
        public static object Make(DynamicType cls, object x) {
            if (cls == FloatType) {
                if (x is string) {
                    return ParseFloat((string)x);
                }
                if (x is char) {
                    return ParseFloat(Ops.Char2String((char)x));
                }

                double doubleVal;
                if (Converter.TryConvertToDouble(x, out doubleVal)) return doubleVal;

                if (x is Complex64) throw Ops.TypeError("can't convert complex to float; use abs(z)");

                object d = Ops.Call(Ops.GetAttr(DefaultContext.Default, x, SymbolTable.ConvertToFloat));
                if (d is double) return d;
                throw Ops.TypeError("__float__ returned non-float (type %s)", Ops.GetDynamicType(d));
            } else {
                return cls.ctor.Call(cls, x);
            }
        }

        private static object ParseFloat(string x) {
            try {
                return LiteralParser.ParseFloat(x);
            } catch (FormatException) {
                throw Ops.ValueError("invalid literal for float(): {0}", x);
            }
        }

        private static object TrueDivide(double x, long y) {
            if (y == 0) throw Ops.ZeroDivisionError();
            return x / y;
        }

        internal static object TrueDivide(double x, double y) {
            if (y == 0) throw Ops.ZeroDivisionError();
            return x / y;
        }

        private static object TrueDivide(double x, BigInteger y) {
            if (y == BigInteger.Zero) throw Ops.ZeroDivisionError();
            return x / y;
        }

        [PythonName("__abs__")]
        public static object Abs(double x) {
            return Math.Abs(x);
        }

        [PythonName("__pow__")]
        public static object Power(double x, double y) {
            if (x == 0.0 && y < 0.0)
                throw Ops.ZeroDivisionError("0.0 cannot be raised to a negative power");
            if (x < 0 && (Math.Floor(y) != y)) {
                throw Ops.ValueError("negative number cannot be raised to fraction");
            }
            double result = Math.Pow(x, y);
            if (double.IsInfinity(result)) {
                throw Ops.OverflowError("result too large");
            }
            return result;
        }

        [PythonName("__div__")]
        public static object Divide(double x, object other) {
            return TrueDivide(x, other);
        }

        [PythonName("__rdiv__")]
        public static object ReverseDivide(double x, object other) {
            return ReverseTrueDivide(x, other);
        }

        [PythonName("__eq__")]
        public static object Equals(double x, object other) {
            ExtensibleLong el;

            if (other == null) return false;

            if (other is int) return Ops.Bool2Object(x == (int)other);
            else if (other is double) return Ops.Bool2Object(x == (double)other);
            else if (other is BigInteger) return Ops.Bool2Object(x == (BigInteger)other);
            else if (other is long) return Ops.Bool2Object(x == (long)other);
            else if (other is Complex64) return Ops.Bool2Object(x == (Complex64)other);
            else if (other is float) return Ops.Bool2Object(x == (float)other);
            else if (other is ExtensibleFloat) return Ops.Bool2Object(x == ((ExtensibleFloat)other).value);
            else if ((el = other as ExtensibleLong) != null) return Ops.Bool2Object(x == el.Value);
            else if (other is bool) return Ops.Bool2Object((bool)other ? x == 1 : x == 0);
            else if (other is decimal) return Ops.Bool2Object(x == (double)(decimal)other);

            double y;
            if (Converter.TryConvertToDouble(other, out y)) return Ops.Bool2Object(x == y);

            object res = Ops.GetDynamicType(other).Coerce(other, x);
            if (res != Ops.NotImplemented && !(res is OldInstance)) {
                return Ops.Equal(((Tuple)res)[1], ((Tuple)res)[0]);
            }

            return Ops.NotImplemented;
        }

        [PythonName("__eq__")]
        public static object Equals(float x, object other) {
            // need to not promote to double, as that may throw
            // off our number...  instead demote double to float, and
            // the compare.
            if (other is double) return x == (float)(double)other;

            return Equals((double)x, other);
        }

        public static bool EqualsRetBool(double x, object other) {
            ExtensibleLong el;

            if (other is int) return x == (int)other;
            else if (other is double) return x == (double)other;
            else if (other is BigInteger) return x == (BigInteger)other;
            else if (other is long) return x == (long)other;
            else if (other is Complex64) return x == (Complex64)other;
            else if (other is float) return x == (float)other;
            else if (other is ExtensibleFloat) return x == ((ExtensibleFloat)other).value;
            else if ((el = other as ExtensibleLong) != null) return x == el.Value;
            else if (other is bool) return (bool)other ? x == 1 : x == 0;
            else if (other is decimal) return x == (double)(decimal)other;

            double y;
            if (Converter.TryConvertToDouble(other, out y)) return x == y;

            object res = Ops.GetDynamicType(other).Coerce(other, x);
            if (res != Ops.NotImplemented && !(res is OldInstance)) {
                return Ops.EqualRetBool(((Tuple)res)[1], ((Tuple)res)[0]);
            }

            return Ops.DynamicEqualRetBool(x, other);
        }

        public static bool EqualsRetBool(float x, object other) {
            return EqualsRetBool((double)x, other);
        }

        [PythonName("__neg__")]
        public static object Negate(double x) {
            return -x;
        }

        [PythonName("__pos__")]
        public static object Positive(double x) {
            return x;
        }


        #region ToString

        public static string ToString(double x) {
            StringFormatter sf = new StringFormatter("%.12g", x);
            sf.TrailingZeroAfterWholeFloat = true;
            return sf.Format();
        }

        [PythonName("__str__")]
        public static string ToString(double x, IFormatProvider provider) {
            return x.ToString(provider);
        }
        [PythonName("__str__")]
        public static string ToString(double x, string format) {
            return x.ToString(format);
        }
        [PythonName("__str__")]
        public static string ToString(double x, string format, IFormatProvider provider) {
            return x.ToString(format, provider);
        }

        public static string ToString(float x) {
            // Python does not natively support System.Single. However, we try to provide
            // formatting consistent with System.Double.
            StringFormatter sf = new StringFormatter("%.6g", x);
            sf.TrailingZeroAfterWholeFloat = true;
            return sf.Format();
        }

        #endregion

        [PythonName("__coerce__")]
        public static double Coerce(double x, object o) {
            double d = (double)Make(FloatType, o);

            if (Double.IsInfinity(d)) {
                throw Ops.OverflowError("number too big");
            }

            return d;
        }

        [PythonName("__pow__")]
        public static object Power(double x, object other) {
            INumber num;
            ExtensibleComplex ec;

            if (other is int) return Power(x, ((int)other));
            if (other is long) return Power(x, ((long)other));
            if (other is Complex64) return ComplexOps.Power(x, ((Complex64)other));
            if (other is double) return Power(x, ((double)other));
            if (other is BigInteger) return Power(x, ((BigInteger)other));
            if (other is bool) return Power(x, (bool)other ? 1 : 0);
            if (other is float) return Power(x, ((float)other));
            if ((num = other as INumber) != null) return num.ReversePower(x);
            if ((ec = other as ExtensibleComplex) != null) return ComplexOps.ReversePower(ec.value, x);
            if (other is byte) return Power(x, (int)((byte)other));
            return Ops.NotImplemented;
        }

        [PythonName("__floordiv__")]
        public static object FloorDivide(double x, object other) {
            INumber num;
            ExtensibleComplex ec;

            if (other is int) {
                int y = (int)other;
                if (y == 0) throw Ops.ZeroDivisionError();
                return Math.Floor(x / y);
            }
            if (other is long) {
                long y = (long)other;
                if (y == 0) throw Ops.ZeroDivisionError();
                return Math.Floor(x / y);
            }
            if (other is Complex64) {
                Complex64 y = (Complex64)other;
                if (y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.FloorDivide(Complex64.MakeReal(x), y);
            }
            if (other is double) {
                double y = (double)other;
                if (y == 0) throw Ops.ZeroDivisionError();
                return Math.Floor(x / y);
            }
            if (other is BigInteger) {
                BigInteger y = (BigInteger)other;
                if (y == BigInteger.Zero) throw Ops.ZeroDivisionError();
                return Math.Floor(x / y);
            }
            if ((num = other as INumber) != null) {
                return num.ReverseFloorDivide(x);
            }
            if ((ec = other as ExtensibleComplex) != null) {
                ComplexOps.FloorDivide((Complex64)x, ec.value);
            }


            if (other is IConvertible) {
                double y = ((IConvertible)other).ToDouble(null);
                if (y == 0) throw Ops.ZeroDivisionError();
                return Math.Floor(x / y);
            }
            return Ops.NotImplemented;
        }

        [PythonName("__rfloordiv__")]
        public static object ReverseFloorDivide(double x, object other) {
            ExtensibleLong el;

            if (other is int) {
                if (x == 0) throw Ops.ZeroDivisionError();
                return Math.Floor((int)other / x);
            }
            if (other is long) {
                if (x == 0) throw Ops.ZeroDivisionError();
                return Math.Floor((long)other / x);
            }
            if (other is Complex64) {
                if (x == 0) throw Ops.ZeroDivisionError();
                return ComplexOps.FloorDivide((Complex64)other, Complex64.MakeReal(x));
            }
            if (other is double) {
                if (x == 0) throw Ops.ZeroDivisionError();
                return Math.Floor((double)other / x);
            }
            if (other is BigInteger) {
                if (x == 0) throw Ops.ZeroDivisionError();
                return Math.Floor((BigInteger)other / x);
            }
            if (other is ExtensibleFloat) {
                if (x == 0) throw Ops.ZeroDivisionError();
                return Math.Floor(((ExtensibleFloat)other).value / x);
            }
            if (other is ExtensibleInt) {
                if (x == 0) throw Ops.ZeroDivisionError();
                return Math.Floor(((ExtensibleInt)other).value / x);
            }
            if (other is ExtensibleComplex) {
                if (x == 0) throw Ops.ZeroDivisionError();
                return ComplexOps.FloorDivide(((ExtensibleComplex)other).value, Complex64.MakeReal(x));
            }
            if ((el = other as ExtensibleLong) != null) {
                if (x == 0) throw Ops.ZeroDivisionError();
                return Math.Floor(el.Value / x);
            }
            if (other is IConvertible) {
                if (x == 0) throw Ops.ZeroDivisionError();
                return Math.Floor(((IConvertible)other).ToDouble(null) / x);
            }
            return Ops.NotImplemented;
        }

        private static double Modulo(double x, double y) {
            double r = x % y;
            if (r > 0 && y < 0) {
                r = r + y;
            } else if (r < 0 && y > 0) {
                r = r + y;
            }
            return r;
        }

        [PythonName("__mod__")]
        public static object Mod(double x, object other) {
            ExtensibleComplex ec;
            INumber num;

            if (other is int) {
                int y = (int)other;
                if (y == 0) throw Ops.ZeroDivisionError();
                return Modulo(x, y);
            }
            if (other is long) {
                long y = (long)other;
                if (y == 0) throw Ops.ZeroDivisionError();
                return Modulo(x, y);
            }
            if (other is Complex64) {
                Complex64 y = (Complex64)other;
                if (y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.Mod(Complex64.MakeReal(x), y);
            }
            if (other is double) {
                double y = (double)other;
                if (y == 0) throw Ops.ZeroDivisionError();
                return Modulo(x, y);
            }
            if (other is BigInteger) {
                BigInteger y = (BigInteger)other;
                if (y == BigInteger.Zero) throw Ops.ZeroDivisionError();
                return Modulo(x, y);
            }
            if ((num = other as INumber) != null) {
                return num.ReverseMod(x);
            }
            if ((ec = other as ExtensibleComplex) != null) {
                return ComplexOps.Mod((Complex64)x, ec.value);
            }
            if (other is IConvertible) {
                double y = ((IConvertible)other).ToDouble(null);
                if (y == 0) throw Ops.ZeroDivisionError();
                return Modulo(x, y);
            }

            return Ops.NotImplemented;
        }

        [PythonName("__rmod__")]
        public static object ReverseMod(double x, object other) {
            ExtensibleLong el;

            if (other is int) {
                if (x == 0) throw Ops.ZeroDivisionError();
                return Modulo((int)other, x);
            }
            if (other is long) {
                if (x == 0) throw Ops.ZeroDivisionError();
                return Modulo((long)other, x);
            }
            if (other is Complex64) {
                if (x == 0) throw Ops.ZeroDivisionError();
                return ComplexOps.Mod((Complex64)other, Complex64.MakeReal(x));
            }
            if (other is double) {
                if (x == 0) throw Ops.ZeroDivisionError();
                return Modulo((double)other, x);
            }
            if (other is BigInteger) {
                if (x == 0) throw Ops.ZeroDivisionError();
                return Modulo((BigInteger)other, x);
            }
            if (other is ExtensibleFloat) {
                if (x == 0) throw Ops.ZeroDivisionError();
                return Modulo(((ExtensibleFloat)other).value, x);
            }
            if (other is ExtensibleInt) {
                if (x == 0) throw Ops.ZeroDivisionError();
                return Modulo(((ExtensibleInt)other).value, x);
            }
            if (other is ExtensibleComplex) {
                if (x == 0) throw Ops.ZeroDivisionError();
                return ComplexOps.Mod(((ExtensibleComplex)other).value, Complex64.MakeReal(x));
            }
            if ((el = other as ExtensibleLong) != null) {
                if (x == 0) throw Ops.ZeroDivisionError();
                return Modulo(el.Value, x);
            }
            if (other is IConvertible) {
                if (x == 0) throw Ops.ZeroDivisionError();
                return Modulo(((IConvertible)other).ToDouble(null), x);
            }

            return Ops.NotImplemented;
        }

        [PythonName("__rpow__")]
        public static object ReversePower(double x, object other) {
            INumber num;
            ExtensibleComplex ec;

            if (other is int) return IntOps.Power((int)other, x);
            if (other is long) return Int64Ops.Power(((long)other), x);
            if (other is Complex64) return ComplexOps.Power(((Complex64)other), x);
            if (other is double) return FloatOps.Power(((double)other), x);
            if (other is BigInteger) return LongOps.Power(((BigInteger)other), x);
            if (other is bool) return IntOps.Power((bool)other ? 1 : 0, x);
            if (other is float) return FloatOps.Power(((float)other), x);
            if ((num = other as INumber) != null) return num.Power(x);
            if ((ec = other as ExtensibleComplex) != null) return ComplexOps.Power(ec.value, x);
            if (other is byte) return IntOps.Power((int)((byte)other), x);
            return Ops.NotImplemented;
        }

        [PythonName("__int__")]
        public static object ToInteger(double d) {
            if (Int32.MinValue <= d && d <= Int32.MaxValue) {
                return (int)d;
            } else if (Int64.MinValue <= d && d <= Int64.MaxValue) {
                return (long)d;
            } else {
                return BigInteger.Create(d);
            }
        }

        [PythonName("__getnewargs__")]
        public static object GetNewArgs(double self) {
            return Tuple.MakeTuple(FloatOps.Make(TypeCache.Double, self));
        }

        internal static object ReverseTrueDivide(double x, double y) {
            return TrueDivide(y, x);
        }

        private static object ReverseTrueDivide(double x, float y) {
            return TrueDivide(y, x);
        }

        private static object ReverseTrueDivide(double x, int y) {
            return TrueDivide(y, x);
        }

        private static object ReverseTrueDivide(double x, BigInteger y) {
            return TrueDivide((double)y, x);
        }

        private static object ReverseTrueDivide(double x, long y) {
            return TrueDivide(y, x);
        }

        [PythonName("__cmp__")]
        public static object Compare(double self, object other) {
            if (other == null) return 1;

            // BigInts can hold doubles, but doubles can't hold BigInts, so
            // if we're comparing against a BigInt then we should convert ourself
            // to a long and then compare.
            BigInteger bi = other as BigInteger;
            if (!Object.ReferenceEquals(bi, null)) {
                BigInteger bigself = BigInteger.Create(self);
                if (bigself == bi) {
                    double mod = self % 1;
                    if (mod == 0) return 0;
                    if (mod > 0) return 1;
                    return -1;
                }
                if (bigself > bi) return 1;
                return -1;
            }

            // everything else can be held by a double.
            double val;
            if (Converter.TryConvertToDouble(other, out val)) {
                if (val == self) return 0;
                if (self < val) return -1;
                return 1;
            } else {
                object res = Ops.GetDynamicType(other).Coerce(other, self);
                if (res != Ops.NotImplemented && !(res is OldInstance)) {
                    return Ops.Compare(((Tuple)res)[1], ((Tuple)res)[0]);
                }

                Complex64 c64;
                if (Converter.TryConvertToComplex64(other, out c64)) {
                    return ComplexOps.TrueCompare(c64, new Complex64(self)) * -1;
                }

                return Ops.NotImplemented;
            }
        }

        internal static object DivMod(double x, double y) {
            object div = FloorDivide(x, y);
            if (div == Ops.NotImplemented) return div;
            return Tuple.MakeTuple(div, Modulo(x, y));
        }
        internal static object ReverseDivMod(double x, double y) {
            return DivMod(y, x);
        }
    }
}
