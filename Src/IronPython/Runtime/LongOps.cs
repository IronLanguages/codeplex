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
using System.Threading;

using IronMath;

namespace IronPython.Runtime {
    /// <summary>
    /// BigInteger doesn't have the right ctor's on it for us to call 
    /// the derived class from a base class, so we define ExtensibeLong
    /// and give it a new ctor.
    /// </summary>
    public partial class ExtensibleLong : IRichComparable, ICodeFormattable, INumber {
        private BigInteger value;

        public ExtensibleLong() {
            value = BigInteger.Zero;
        }

        public ExtensibleLong(BigInteger val) {
            value = val;
        }

        public BigInteger Value {
            get {
                return value;
            }
        }

        #region IRichComparable Members

        public object CompareTo(object other) {
            return LongOps.Compare(this.Value, other);
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
        public virtual object RichGetHashCode() {
            return Ops.Int2Object(GetHashCode());
        }

        [PythonName("__eq__")]
        public virtual object RichEquals(object other) {
            if (other == null) return Ops.FALSE;

            BigInteger bi = other as BigInteger;
            if (!object.ReferenceEquals(bi, null)) return value == bi;

            return Ops.NotImplemented;
        }

        [PythonName("__ne__")]
        public virtual object RichNotEquals(object other) {
            object res = RichEquals(other);
            if (res != Ops.NotImplemented) return Ops.Not(res);

            return Ops.NotImplemented;
        }

        #endregion

        [PythonName("__str__")]
        public override string ToString() {
            return value.ToString();
        }

        public override bool Equals(object obj) {
            return value.Equals(obj);
        }

        public override int GetHashCode() {
            return value.GetHashCode();
        }

        #region ICodeFormattable Members

        [PythonName("__repr__")]
        public string ToCodeString() {            
            return (string)Ops.Repr(value);
        }

        #endregion
    }

    public static partial class LongOps {
        static ReflectedType LongType;
        public static ReflectedType MakeDynamicType() {
            if (LongType == null) {
                ReflectedType res = new OpsReflectedType("long",
                    typeof(BigInteger), 
                    typeof(LongOps), 
                    typeof(ExtensibleLong));

                if (Interlocked.CompareExchange<ReflectedType>(ref LongType, res, null) == null)
                    return res;
            }
            return LongType;
        }

        [PythonName("__new__")]
        public static object Make(PythonType cls, string s, int radix) {
            if (cls == LongType) {
                return ParseBigIntegerSign(s, radix);
            } else {
                BigInteger res = ParseBigIntegerSign(s, radix);
                return cls.ctor.Call(cls, res);
            }
        }

        private static BigInteger ParseBigIntegerSign(string s, int radix) {
            try {
                return LiteralParser.ParseBigIntegerSign(s, radix);
            } catch (ArgumentException e) {
                throw Ops.ValueError(e.Message);
            }
        }

        [PythonName("__new__")]
        public static object Make(PythonType cls, object x) {
            ExtensibleLong el;

            if (cls == LongType) {
                if (x is string) return ParseBigIntegerSign((string)x, 10);
                if (x is IronMath.BigInteger) return (IronMath.BigInteger)x;
                else if ((el = x as ExtensibleLong) != null) return el.Value;
                else if (x is int) return (long)(int)x;
                else if (x is double) return IronMath.BigInteger.Create((double)x);
                else if (x is long) return x;
                else {
                    Conversion conv;
                    BigInteger intVal;
                    intVal = Converter.TryConvertToBigInteger(x, out conv);
                    if (conv < Conversion.Truncation) return intVal;
                }
            } else {
                BigInteger intVal = null;

                if (x is string) intVal = ParseBigIntegerSign((string)x, 10);
                else if (x is IronMath.BigInteger) intVal = (IronMath.BigInteger)x;
                else if ((el = x as ExtensibleLong) != null) intVal = el.Value;
                else if (x is int) intVal = (long)(int)x;
                else if (x is double) intVal = IronMath.BigInteger.Create((double)x);
                else if (x is long) intVal = (long)x;
                else {
                    Conversion conv;
                    intVal = Converter.TryConvertToBigInteger(x, out conv);
                    if (conv >= Conversion.Truncation) intVal = null;
                }

                if (!Object.Equals(intVal, null)) {
                    return cls.ctor.Call(cls, intVal);
                }
            }

            if (x is Complex64) throw Ops.TypeError("can't convert complex to long; use long(abs(z))");

            throw Ops.ValueError("long argument must be convertible to long (string, number, or type that defines __long__, got {0})",
                Ops.StringRepr(Ops.GetDynamicType(x).__name__));
        }

        [PythonName("__new__")]
        public static object Make(PythonType cls) {
            if (cls == LongType) {
                return BigInteger.Zero;
            } else {
                return cls.ctor.Call(cls, BigInteger.Zero);
            }
        }

        private static object TrueDivide(BigInteger x, double y) {
            if (y == 0.0) {
                throw new DivideByZeroException();
            }
            return x.ToFloat64() / y;
        }

        private static object TrueDivide(BigInteger x, BigInteger y) {
            if (y == BigInteger.Zero) {
                throw new DivideByZeroException();
            }
            return x.ToFloat64() / y.ToFloat64();
        }

        [PythonName("__abs__")]
        public static object Abs(BigInteger x) {
            return x.Abs();
        }

        [PythonName("__radd__")]
        public static object ReverseAdd(BigInteger x, object other) {
            return Add(x, other);
        }

        [PythonName("__rsub__")]
        public static object ReverseSubtract(BigInteger x, object other) {
            if (other is int) return ((int)other) - x;
            if (other is Complex64) return ((Complex64)other) - x;
            if (other is double) return ((double)other) - x;
            if (other is BigInteger) return ((BigInteger)other) - x;
            if (other is bool) return ((bool)other ? 1 : 0) - x;
            if (other is long) return ((long)other) - x;
            if (other is ExtensibleInt) return (((ExtensibleInt)other).value) - x;
            if (other is ExtensibleFloat) return (((ExtensibleFloat)other).value) - x;
            if (other is ExtensibleComplex) return ((ExtensibleComplex)other).value - x;
            return Ops.NotImplemented;
        }

        [PythonName("__rmul__")]
        public static object ReverseMultiply(BigInteger x, object other) {
            return Multiply(x, other);
        }

        [PythonName("__rand__")]
        public static object ReverseBitwiseAnd(BigInteger x, object other) {
            return BitwiseAnd(x, other);
        }

        [PythonName("__ror__")]
        public static object ReverseBitwiseOr(BigInteger x, object other) {
            return BitwiseOr(x, other);
        }

        [PythonName("__rxor__")]
        public static object ReverseXor(BigInteger x, object other) {
            return Xor(x, other);
        }

        [PythonName("__powmod__")]
        public static object PowerMod(BigInteger x, object y, object z) {
            if (y is int) {
                return PowerMod(x, (int)y, z);
            } else if (y is long) {
                long longy = (long)y;
                if (longy < Int32.MinValue || longy > Int32.MaxValue) {
                    throw Ops.ValueError("value too big");
                }
                return PowerMod(x, (int)longy, z);
            } else if (y is BigInteger) {
                int inty;
                if (((BigInteger)y).AsInt32(out inty)) {
                    return PowerMod(x, inty, z);
                }
                throw Ops.ValueError("value too big");
            }
            return Ops.NotImplemented;
        }

        private static object PowerMod(BigInteger x, int y, object z) {
            if (z is int) {
                return PowerMod(x, y, BigInteger.Create((int)z));
            } else if (z is long) {
                return PowerMod(x, y, BigInteger.Create((long)z));
            } else if (z is BigInteger) {
                return PowerMod(x, y, (BigInteger)z);
            }
            return Ops.NotImplemented;
        }

        private static object PowerMod(BigInteger x, int y, BigInteger z) {
            if (y < 0) {
                throw Ops.TypeError("power", y, "power must be >= 0");
            }
            if (z == BigInteger.Zero) {
                throw Ops.ZeroDivisionError();
            }

            BigInteger result = x.ModPow(y, z);

            if (result >= 0) {
                if (z < 0) return result + z;
            } else {
                if (z > 0) return result + z;
            }
            return result;
        }

        [PythonName("__pow__")]
        public static object Power(BigInteger x, int y) {
            if (y < 0) {
                return FloatOps.Power(x.ToFloat64(), y);
            }
            return x.Power(y);
        }

        [PythonName("__float__")]
        public static object ToFloat(BigInteger self) {
            return self.ToFloat64();
        }

        internal static object Power(BigInteger x, ulong exp) {
            if (exp == 0) {
                return BigInteger.One;
            } else if (exp == 1) {
                return x;
            } else if (exp < Int32.MaxValue) {
                return Power(x, (int)exp);
            } else if (x == BigInteger.Zero) {
                return BigInteger.Zero;
            } else if (x == BigInteger.One) {
                return BigInteger.One;
            } else {
                throw Ops.ValueError("number too big");
            }
        }

        [PythonName("__pow__")]
        public static object Power(BigInteger x, long y) {
            if ((int)y == y) {
                return Power(x, (int)y);
            } else {
                if (y < 0) {
                    return FloatOps.Power(x.ToFloat64(), y);
                }
                if (x == BigInteger.Zero) {
                    return BigInteger.Zero;
                } else if (x == BigInteger.One) {
                    return BigInteger.One;
                } else {
                    throw Ops.ValueError("Number too big");
                }
            }
        }

        [PythonName("__pow__")]
        public static object Power(BigInteger x, double y) {
            return FloatOps.Power(x.ToFloat64(), y);
        }

        [PythonName("__cmp__")]
        public static object Compare(BigInteger x, object y) {
            if (y == null) return 1;

            int intVal;
            if (y is int) {
                if (x.AsInt32(out intVal)) return IntOps.Compare(intVal, y);
            } else if (y is ExtensibleInt) {
                if (x.AsInt32(out intVal)) return IntOps.Compare(intVal, ((ExtensibleInt)y).value);
            } else if (y is double) {
                return ((int)FloatOps.Compare((double)y, x)) * -1;
            } else if (y is ExtensibleFloat) {
                double dbl = x.ToFloat64();
                return FloatOps.Compare(dbl, ((ExtensibleFloat)y).value);
            } else if (y is bool) {
                if (x.AsInt32(out intVal)) return IntOps.Compare(intVal, ((bool)y) ? 1 : 0);
            } else if (y is decimal) {
                double dbl = x.ToFloat64();
                return FloatOps.Compare(dbl, y);
            }

            Conversion conv;
            BigInteger bi = Converter.TryConvertToBigInteger(y, out conv);
            if (conv == Conversion.None) {
                object res = Ops.GetDynamicType(y).Coerce(y, x);
                if (res != Ops.NotImplemented && !(res is OldInstance)) {
                    return Ops.Compare(((Tuple)res)[1], ((Tuple)res)[0]);
                }
                return Ops.NotImplemented;
            }

            BigInteger diff = x - bi;
            if (diff == 0) return 0;
            else if (diff < 0) return -1;
            else return 1;
        }

        [PythonName("__gt__")]
        public static object __gt__(BigInteger x, object y) {
            if (y == null) return true;

            Conversion conv;
            BigInteger bi = Converter.TryConvertToBigInteger(y, out conv);
            if (conv == Conversion.None) return Ops.NotImplemented;
            return x - bi > 0;
        }

        [PythonName("__lt__")]
        public static object __lt__(BigInteger x, object y) {
            if (y == null) return false;

            Conversion conv;
            BigInteger bi = Converter.TryConvertToBigInteger(y, out conv);
            if (conv == Conversion.None) return Ops.NotImplemented;
            return x - bi < 0;
        }

        [PythonName("__ge__")]
        public static object __ge__(BigInteger x, object y) {
            if (y == null) return true;

            Conversion conv;
            BigInteger bi = Converter.TryConvertToBigInteger(y, out conv);
            if (conv == Conversion.None) return Ops.NotImplemented;
            return x - bi >= 0;
        }

        [PythonName("__le__")]
        public static object __le__(BigInteger x, object y) {
            if (y == null) return false;

            Conversion conv;
            BigInteger bi = Converter.TryConvertToBigInteger(y, out conv);
            if (conv == Conversion.None) return Ops.NotImplemented;
            return x - bi <= 0;
        }


        [PythonName("__pow__")]
        public static object Power(BigInteger x, BigInteger y) {
            if (Object.ReferenceEquals(x, null)) throw Ops.TypeError("unsupported operands for __pow__: NoneType and long");
            if (Object.ReferenceEquals(y, null)) throw Ops.TypeError("unsupported operands for __pow__: long and NoneType");

            long yl;
            if (y.AsInt64(out yl)) {
                return Power(x, yl);
            } else {
                if (x == BigInteger.Zero) {
                    if (y.IsNegative())
                        throw Ops.ZeroDivisionError("0.0 cannot be raised to a negative power");
                    return BigInteger.Zero;
                } else if (x == BigInteger.One) {
                    return BigInteger.One;
                } else {
                    throw Ops.ValueError("Number too big");
                }
            }
        }

        private static BigInteger DivMod(BigInteger x, BigInteger y, out BigInteger r) {
            BigInteger rr;
            BigInteger qq;

            if (Object.ReferenceEquals(x, null)) throw Ops.TypeError("unsupported operands for div/mod: NoneType and long");
            if (Object.ReferenceEquals(y, null)) throw Ops.TypeError("unsupported operands for div/mod: long and NoneType");

            qq = BigInteger.DivRem(x, y, out rr);

            if (x >= BigInteger.Zero) {
                if (y > BigInteger.Zero) {
                    r = rr;
                    return qq;
                } else {
                    if (rr == BigInteger.Zero) {
                        r = rr;
                        return qq;
                    } else {
                        r = rr + y;
                        return qq - BigInteger.One;
                    }
                }
            } else {
                if (y > BigInteger.Zero) {
                    if (rr == BigInteger.Zero) {
                        r = rr;
                        return qq;
                    } else {
                        r = rr + y;
                        return qq - BigInteger.One;
                    }
                } else {
                    r = rr;
                    return qq;
                }
            }
        }

        [PythonName("__div__")]
        public static object Divide(BigInteger x, int y) {
            BigInteger r;
            return DivMod(x, y, out r);
        }
        [PythonName("__div__")]
        public static object Divide(BigInteger x, long y) {
            BigInteger r;
            return DivMod(x, y, out r);
        }
        [PythonName("__div__")]
        public static object Divide(BigInteger x, uint y) {
            BigInteger r;
            return DivMod(x, y, out r);
        }
        [PythonName("__div__")]
        public static object Divide(BigInteger x, ulong y) {
            BigInteger r;
            return DivMod(x, y, out r);
        }
        [PythonName("__div__")]
        public static object Divide(BigInteger x, BigInteger y) {
            BigInteger r;
            return DivMod(x, y, out r);
        }
        [PythonName("__mod__")]
        public static object Mod(BigInteger x, int y) {
            BigInteger r;
            DivMod(x, y, out r);
            return r;
        }
        [PythonName("__mod__")]
        public static object Mod(BigInteger x, long y) {
            BigInteger r;
            DivMod(x, y, out r);
            return r;
        }
        [PythonName("__mod__")]
        public static object Mod(BigInteger x, uint y) {
            BigInteger r;
            DivMod(x, y, out r);
            return r;
        }
        [PythonName("__mod__")]
        public static object Mod(BigInteger x, ulong y) {
            BigInteger r;
            DivMod(x, y, out r);
            return r;
        }
        [PythonName("__mod__")]
        public static object Mod(BigInteger x, BigInteger y) {
            BigInteger r;
            DivMod(x, y, out r);
            return r;
        }
        [PythonName("__eq__")]
        public static object Equals(BigInteger x, object other) {
            if (other is int) return Ops.Bool2Object(x == (BigInteger)(int)other);
            else if (other is long) return Ops.Bool2Object(x == (BigInteger)(long)other);
            else if (other is double) return Ops.Bool2Object(x == (double)other);
            else if (other is BigInteger) return Ops.Bool2Object((BigInteger)x == (BigInteger)other);
            else if (other is Complex64) return Ops.Bool2Object(x == (Complex64)other);
            else if (other is bool) return Ops.Bool2Object((bool)other ? x == 1 : x == 0);
            else if (other is ExtensibleFloat) return Ops.Bool2Object(x == ((ExtensibleFloat)other).value);
            else if (other is decimal) return Ops.Bool2Object(x == (double)(decimal)other);
            else if (other == null) return Ops.FALSE;

            Conversion conversion;
            BigInteger y = Converter.TryConvertToBigInteger(other, out conversion);
            if (conversion != Conversion.None) return Ops.Bool2Object(x == y);

            object res = Ops.GetDynamicType(other).Coerce(other, x);
            if (res != Ops.NotImplemented && !(res is OldInstance)) {
                return Ops.Equal(((Tuple)res)[1], ((Tuple)res)[0]);
            }

            return Ops.NotImplemented;
        }

        public static bool EqualsRetBool(BigInteger x, object other) {
            if (other is int) return x == (BigInteger)(int)other;
            else if (other is long) return x == (BigInteger)(long)other;
            else if (other is double) return x == (double)other;
            else if (other is BigInteger) return (BigInteger)x == (BigInteger)other;
            else if (other is Complex64) return x == (Complex64)other;
            else if (other is bool) return (bool)other ? x == 1 : x == 0;
            else if (other is ExtensibleFloat) return x == ((ExtensibleFloat)other).value;
            else if (other is decimal) return x == (double)(decimal)other;
            else if (other == null) return false;

            Conversion conversion;
            BigInteger y = Converter.TryConvertToBigInteger(other, out conversion);
            if (conversion != Conversion.None) return x == y;

            object res = Ops.GetDynamicType(other).Coerce(other, x);
            if (res != Ops.NotImplemented && !(res is OldInstance)) {
                return Ops.EqualRetBool(((Tuple)res)[1], ((Tuple)res)[0]);
            }

            return Ops.DynamicEqualRetBool(x, other);
        }

        [PythonName("__ne__")]
        public static object NotEquals(BigInteger x, object y) {
            object res = Equals(x, y);
            if (res == Ops.NotImplemented) return res;
            else return Ops.Bool2Object(!((bool)res));
        }


        [PythonName("__neg__")]
        public static object Negate(BigInteger x) {
            return -x;
        }

        [PythonName("__pos__")]
        public static object Positive(BigInteger x) {
            return x;
        }

        [PythonName("__invert__")]
        public static object Invert(BigInteger x) {
            return ~x;
        }
        
        [PythonName("__lshift__")]
        public static object LeftShift(BigInteger x, object other) {
            ExtensibleLong el;
            ExtensibleInt ei;

            if (other is int) {
                return LeftShift(x, (int)other);
            } else if (other is BigInteger) {
                return LeftShift(x, (BigInteger)other);
            } else if (other is long) {
                long y = (long)other;
                if (Int32.MinValue <= y && y <= Int32.MaxValue) {
                    return LeftShift(x, (int)y);
                }
            } else if (other is bool) {
                return LeftShift(x, (bool)other ? 1 : 0);
            } else if ((ei = other as ExtensibleInt)!= null) {
                return ei.ReverseLeftShift(x);
            } else if ((el = other as ExtensibleLong) != null) {
                return el.ReverseLeftShift(x);
            } else if (other is byte) {
                return LeftShift(x, (int)((byte)other));
            }
            return Ops.NotImplemented;
        }

        private static object LeftShift(BigInteger x, int y) {
            if (y < 0) {
                throw Ops.ValueError("negative shift count");
            }
            return x << y;
        }
        internal static object LeftShift(BigInteger x, ulong y) {
            if (y <= Int32.MaxValue) {
                return LeftShift(x, (int)y);
            }
            throw Ops.OverflowError("number too big");
        }
        private static object LeftShift(BigInteger x, BigInteger y) {
            if (y < BigInteger.Zero) {
                throw Ops.ValueError("negative shift count");
            }
            int yint;
            if (y.AsInt32(out yint)) {
                return x << yint;
            }
            if (x == BigInteger.Zero) {
                return BigInteger.Zero;
            } else {
                throw Ops.OverflowError("number too big");
            }
        }

        [PythonName("__rshift__")]
        public static object RightShift(BigInteger x, object other) {
            ExtensibleLong el;
            ExtensibleInt ei;

            if (other is int) {
                return RightShift(x, (int)other);
            } else if (other is BigInteger) {
                BigInteger y = (BigInteger)other;
                if (y < BigInteger.Zero) {
                    throw Ops.ValueError("negative shift count");
                }
                int yint;
                if (y.AsInt32(out yint)) {
                    return RightShift(x, yint);
                }
            } else if (other is long) {
                long y = (long)other;
                if (y < 0) {
                    throw Ops.ValueError("negative shift count");
                }
                if (y <= Int32.MaxValue) {
                    return RightShift(x, (int)y);
                }
            } else if (other is bool) {
                return RightShift(x, (bool)other ? 1 : 0);
            } else if ((ei = other as ExtensibleInt)!=null) {
                return ei.ReverseRightShift(x);
            } else if ((el = other as ExtensibleLong) != null) {
                return el.ReverseRightShift(x);
            } else if (other is byte) {
                return RightShift(x, (int)((byte)other));
            }
            return Ops.NotImplemented;
        }

        [PythonName("__rlshift__")]
        public static object ReverseLeftShift(BigInteger x, object other) {
            ExtensibleLong el;
            ExtensibleInt ei;

            if (other is int) {
                return IntOps.LeftShift((int)other, x);
            } else if (other is BigInteger) {
                return LongOps.LeftShift((BigInteger)other, x);
            } else if (other is long) {
                return Int64Ops.LeftShift((long)other, x);
            } else if (other is bool) {
                return IntOps.LeftShift((bool)other ? 1 : 0, x);
            } else if ((ei = other as ExtensibleInt) != null) {
                return ei.LeftShift(x);
            } else if ((el = other as ExtensibleLong) != null) {
                return el.LeftShift(x);
            } else if (other is byte) {
                return IntOps.LeftShift((bool)other ? 1 : 0, x);
            }
            return Ops.NotImplemented;
        }

        [PythonName("__rrshift__")]
        public static object ReverseRightShift(BigInteger x, object other) {
            ExtensibleLong el;
            ExtensibleInt ei;

            if (other is int) {
                return IntOps.RightShift((int)other, x);
            } else if (other is BigInteger) {
                return LongOps.RightShift((BigInteger)other, x);
            } else if (other is long) {
                return Int64Ops.RightShift((long)other, x);
            } else if (other is bool) {
                return IntOps.RightShift((bool)other ? 1 : 0, x);
            } else if ((ei = other as ExtensibleInt) != null) {
                return ei.RightShift(x);
            } else if ((el = other as ExtensibleLong) != null) {
                return el.RightShift(x);
            } else if (other is byte) {
                return IntOps.RightShift((bool)other ? 1 : 0, x);
            }
            return Ops.NotImplemented;
        }

        private static object RightShift(BigInteger x, int y) {
            BigInteger q;
            if (y < 0) {
                throw Ops.ValueError("negative shift count");
            }
            if (x < BigInteger.Zero) {
                q = x >> y;
                BigInteger r = x - (q << y);
                if (r != BigInteger.Zero) q -= BigInteger.One; ;
            } else {
                q = x >> y;
            }
            return q;
        }

        [PythonName("__oct__")]
        public static string Oct(BigInteger x) {
            if (x == BigInteger.Zero) {
                return "0L";
            } else if (x > 0) {
                return "0" + x.ToString(8) + "L";
            } else {
                return "-0" + (-x).ToString(8) + "L";
            }
        }

        [PythonName("__hex__")]
        public static string Hex(BigInteger x) {
            if (x < 0) {
                return "-0x" + (-x).ToString(16) + "L";
            } else {
                return "0x" + x.ToString(16) + "L";
            }
        }

        internal static object DivMod(BigInteger x, BigInteger y) {
            BigInteger div, mod;
            div = DivMod(x, y, out mod);
            return Tuple.MakeTuple(div, mod);
        }

        internal static object ReverseDivMod(BigInteger x, BigInteger y) {
            return DivMod(y, x);
        }
    }
}
