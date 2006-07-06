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
using System.Reflection;

using IronMath;
using IronPython.Runtime;
using IronPython.Runtime.Types;

namespace IronPython.Runtime.Operations {

    public static partial class Int64Ops {
        private static ReflectedType Int64Type;
        public static DynamicType MakeDynamicType() {
            if (Int64Type == null) {
                OpsReflectedType ort = new OpsReflectedType("Int64", typeof(Int64), typeof(Int64Ops), null);
                if (Interlocked.CompareExchange<ReflectedType>(ref Int64Type, ort, null) == null) {
                    return ort;
                }
            }
            return Int64Type;
        }

        [PythonName("__new__")]
        public static object Make(DynamicType cls) {
            return Make(cls, 0);
        }


        [PythonName("__new__")]
        public static object Make(DynamicType cls, object value) {
            if (cls != Int64Type) {
                throw Ops.TypeError("Int64.__new__: first argument must be Int64 type.");
            }
            IConvertible valueConvertible;
            if ((valueConvertible = value as IConvertible) != null) {
                switch (valueConvertible.GetTypeCode()) {
                    case TypeCode.Byte: return (Int64)(Byte)value;
                    case TypeCode.SByte: return (Int64)(SByte)value;
                    case TypeCode.Int16: return (Int64)(Int16)value;
                    case TypeCode.UInt16: return (Int64)(UInt16)value;
                    case TypeCode.Int32: return (Int64)(Int32)value;
                    case TypeCode.UInt32: return (Int64)(UInt32)value;
                    case TypeCode.Int64: return (Int64)(Int64)value;
                    case TypeCode.UInt64: return (Int64)(UInt64)value;
                    case TypeCode.Single: return (Int64)(Single)value;
                    case TypeCode.Double: return (Int64)(Double)value;
                }
            }
            if (value is String) {
                return Int64.Parse((String)value);
            } else if (value is BigInteger) {
                return (Int64)(BigInteger)value;
            } else if (value is ExtensibleInt) {
                return (Int64)((ExtensibleInt)value).value;
            } else if (value is ExtensibleLong) {
                return (Int64)((ExtensibleLong)value).Value;
            } else if (value is ExtensibleFloat) {
                return (Int64)((ExtensibleFloat)value).value;
            }
            throw Ops.ValueError("invalid value for Int64.__new__");
        }


        [PythonName("__abs__")]
        public static object Abs(long x) {
            return Math.Abs(x);
        }

        private static object TrueDivide(long x, long y) {
            if (y == 0) {
                throw new DivideByZeroException();
            }
            return (double)x / (double)y;
        }

        private static object TrueDivide(long x, BigInteger y) {
            if (y == BigInteger.Zero) {
                throw new DivideByZeroException();
            }
            return (double)x / y.ToFloat64();
        }

        private static object TrueDivide(long x, double y) {
            if (y == 0.0) {
                throw new DivideByZeroException();
            }
            return (double)x / (double)y;
        }

        [PythonName("__powmod__")]
        public static long PowerMod(long x, long power, long mod) {
            if (power < 0) {
                throw Ops.TypeError("power", power, "power must be >= 0");
            }
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
                if (mod < 0) return result + mod;
            } else {
                if (mod > 0) return result + mod;
            }
            return result;
        }

        [PythonName("__powmod__")]
        public static object PowerMod(long x, object y, object z) {
            return PowerMod(x, Converter.ConvertToInt64(y), Converter.ConvertToInt64(z));
        }

        [PythonName("__div__")]
        private static long Divide(long x, int y) {
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

        [PythonName("__div__")]
        private static long Divide(long x, long y) {
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

        [PythonName("__rdiv__")]
        private static long ReverseDivide(long x, long y) {
            return Divide(y, x);
        }

        [PythonName("__mod__")]
        private static long Mod(long x, int y) {
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

        [PythonName("__mod__")]
        private static long Mod(long x, long y) {
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

        [PythonName("__rmod__")]
        private static long ReverseMod(long x, long y) {
            return Mod(y, x);
        }

        private static int CompareWorker(long x, long y) {
            return x > y ? 1 : x < y ? -1 : 0;
        }

        private static int CompareWorker(long x, double y) {
            return x > y ? 1 : x < y ? -1 : 0;
        }

        private static int CompareWorker(long x, decimal y) {
            return x > y ? 1 : x < y ? -1 : 0;
        }

        private static int CompareWorker(ulong x, ulong y) {
            return x > y ? 1 : x < y ? -1 : 0;
        }

        private static int CompareWorker(ulong x, double y) {
            return x > y ? 1 : x < y ? -1 : 0;
        }

        private static int CompareWorker(ulong x, decimal y) {
            return x > y ? 1 : x < y ? -1 : 0;
        }

        private static int CompareWorker(long x, ulong y) {
            if (x < 0) return -1;
            if (y > Int64.MaxValue) return 1;

            return CompareWorker(x, (long)y);
        }

        [PythonName("__cmp__")]
        public static object Compare(ulong x, object other) {
            if (other is int) return -CompareWorker((long)(int)other, x);
            else if (other is long) return -CompareWorker((long)other, x);
            else if (other is double) return CompareWorker(x, (double)other);
            else if (other is BigInteger) return -(int)LongOps.Compare((BigInteger)other, x);
            else if (other is Complex64) return ComplexOps.TrueCompare(new Complex64((double)x, 0), (Complex64)other);
            else if (other is float) return CompareWorker(x, (double)(float)other);
            else if (other is bool) return CompareWorker(x, ((bool)other) ? 1UL : 0UL);
            else if (other is decimal) return CompareWorker(x, (decimal)other);
            else if (other is ulong) return CompareWorker(x, (ulong)other);

            UInt64 y;
            if (Converter.TryConvertToUInt64(other, out y)) return CompareWorker(x, y);

            return Ops.NotImplemented;
        }

        [PythonName("__cmp__")]
        public static object Compare(long x, object other) {
            if (other is int) return CompareWorker(x, (long)(int)other);
            else if (other is long) return CompareWorker(x, (long)other);
            else if (other is double) return CompareWorker(x, (double)other);
            else if (other is BigInteger) return -(int)LongOps.Compare((BigInteger)other, x);
            else if (other is Complex64) return ComplexOps.TrueCompare(new Complex64((double)x, 0), (Complex64)other);
            else if (other is float) return CompareWorker(x, (double)(float)other);
            else if (other is bool) return CompareWorker(x, ((bool)other) ? 1L : 0L);
            else if (other is decimal) return CompareWorker(x, (decimal)other);
            else if (other is ulong) return CompareWorker(x, (ulong)other);

            long y;
            if (Converter.TryConvertToInt64(other, out y)) return CompareWorker(x, y);

            return Ops.NotImplemented;
        }

        [PythonName("__eq__")]
        public static object Equals(long x, object other) {
            object ret = Compare(x, other);
            if (ret == Ops.NotImplemented) return ret;

            if ((int)ret == 0) return Ops.TRUE;
            return Ops.FALSE;
        }

        public static bool EqualsRetBool(long x, object other) {
            object ret = Compare(x, other);
            if (ret == Ops.NotImplemented) {
                return Ops.DynamicEqualRetBool(x, other);
            }

            return (int)ret == 0;
        }

        [PythonName("__neg__")]
        public static object Negate(long x) {
            try {
                return checked(-x);
            } catch (OverflowException) {
                return LongOps.Negate(x);
            }
        }

        [PythonName("__rshift__")]
        public static object RightShift(long x, object other) {
            ExtensibleLong el;

            if (other is int) {
                return RightShift(x, (int)other);
            } else if (other is long) {
                return RightShift(x, (long)other);
            } else if (other is bool) {
                return RightShift(x, (bool)other ? 1 : 0);
            } else if (other is BigInteger) {
                BigInteger big = (BigInteger)other;
                int y;
                if (big.AsInt32(out y)) {
                    return RightShift(x, y);
                }
            } else if (other is ExtensibleInt) {
                return RightShift(x, ((ExtensibleInt)other).value);
            } else if ((el = other as ExtensibleLong) != null) {                
                int y;
                if (el.Value.AsInt32(out y)) {
                    return RightShift(x, y);
                }
            } else if (other is byte) {
                return RightShift(x, (int)((byte)other));
            }
            return Ops.NotImplemented;
        }

        internal static object RightShift(long x, long y) {
            if (Int32.MinValue <= y && y <= Int32.MaxValue) {
                return RightShift(x, (int)y);
            } else return Ops.NotImplemented;
        }

        private static object RightShift(long x, int y) {
            if (y < 0) {
                throw Ops.ValueError("negative shift count");
            }
            if (y > 63) {
                return x >= 0 ? 0 : -1;
            }

            long q;

            if (x >= 0) {
                q = x >> y;
            } else {
                q = (x + ((1 << y) - 1)) >> y;
                long r = x - (q << y);
                if (r != 0) q--;
            }
            return Ops.Long2Object(q);
        }

        [PythonName("__lshift__")]
        public static object LeftShift(long x, object other) {
            ExtensibleLong el;

            if (other is int) {
                return LeftShift(x, (int)other);
            } else if (other is long) {
                return LeftShift(x, (long)other);
            } else if (other is bool) {
                return LeftShift(x, (bool)other ? 1 : 0);
            } else if (other is BigInteger) {
                BigInteger big = (BigInteger)other;
                int y;
                if (big.AsInt32(out y)) {
                    return LeftShift(x, y);
                }
            } else if (other is ExtensibleInt) {
                return LeftShift(x, ((ExtensibleInt)other).value);
            } else if ((el = other as ExtensibleLong) != null) {
                int y;
                if (el.Value.AsInt32(out y)) {
                    return LeftShift(x, y);
                }
            } else if (other is byte) {
                return LeftShift(x, (int)((byte)other));
            }
            return Ops.NotImplemented;
        }

        internal static object LeftShift(long x, long y) {
            if (Int32.MinValue <= y && y <= Int32.MaxValue) {
                return LeftShift(x, (int)y);
            } else return Ops.NotImplemented;
        }

        private static object LeftShift(long x, int y) {
            if (y < 0) {
                throw Ops.ValueError("negative shift count");
            }
            if ((y > 63) ||
                (x > 0 && x > (Int64.MaxValue >> y)) ||
                (x < 0 && x < (Int64.MinValue >> y))) {
                return BigInteger.Create(x) << y;
            }

            long ret = x << y;
            return Ops.Long2Object(ret);
        }

        private static object Power(long x, BigInteger y) {
            return LongOps.Power(BigInteger.Create(x), y);
        }

        private static object Power(long x, double y) {
            return FloatOps.Power(x, y);
        }

        [PythonName("__pow__")]
        public static object Power(long x, long exp) {
            if (exp == 0) return 1;
            if (exp == 1) return x;
            if (exp < 0) {
                return FloatOps.Power(x, exp);
            }
            long saveexp = exp;
            long result = 1;
            long factor = x;
            try {
                checked {
                    while (exp != 0) {
                        if ((exp & 1) != 0) {
                            result = result * factor;
                        }
                        if (exp == 1) break;    // save possible overflow in the multiply
                        factor = factor * factor;
                        exp >>= 1;
                    }
                    return result;
                }
            } catch (OverflowException) {
                return LongOps.Power(BigInteger.Create(x), saveexp);
            }
        }

        [PythonName("__oct__")]
        public static string Oct(long x) {
            //!!! horribly inefficient
            if (x == 0) {
                return "0L";
            } else if (x > 0) {
                return "0" + BigInteger.Create(x).ToString(8) + "L";
            } else {
                return "-0" + BigInteger.Create(-x).ToString(8) + "L";
            }
        }

        [PythonName("__hex__")]
        public static string Hex(long x) {
            if (x < 0) {
                return "-0x" + (-x).ToString("X") + "L";
            } else {
                return "0x" + x.ToString("X") + "L";
            }
        }

        public static object DivMod(long x, long y) {
            try {
                return Tuple.MakeTuple(Divide(x, y), Mod(x, y));
            } catch (OverflowException) {
                return LongOps.DivMod(x, y);
            }
        }
        public static object ReverseDivMod(long x, long y) {
            return DivMod(y, x);
        }
        public static object ReverseLeftShift(long x, long y) {
            return LeftShift(y, x);
        }
        public static object ReversePower(long x, long y) {
            return Power(y, x);
        }
        public static object ReverseRightShift(long x, long y) {
            return RightShift(y, x);
        }
    }
}
