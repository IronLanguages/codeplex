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

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;

using IronPython.Runtime.Calls;
using IronPython.Runtime.Types;

using SpecialNameAttribute = System.Runtime.CompilerServices.SpecialNameAttribute;

namespace IronPython.Runtime.Operations {

    public static partial class Int32Ops {
        private static readonly DynamicSite<object, object> _intSite =
            new DynamicSite<object, object>(ConvertToAction.Make(DefaultContext.DefaultPythonBinder, typeof(int)));

        private static object FastNew(object o) {
            Extensible<BigInteger> el;

            if (o is string) return __new__(null, (string)o, 10);
            if (o is double) return DoubleOps.__int__((double)o);
            if (o is int) return o;
            if (o is BigInteger) {
                BigInteger bi = o as BigInteger;
                int res;
                if (bi.AsInt32(out res)) {
                    return RuntimeHelpers.Int32ToObject(res);
                }
                return o;
            }

            if ((el = o as Extensible<BigInteger>) != null) {
                int res;
                if (el.Value.AsInt32(out res)) {
                    return RuntimeHelpers.Int32ToObject(res);
                }
                return el.Value;
            }

            if (o is float) return DoubleOps.__int__((double)(float)o);

            if (o is Complex64) throw PythonOps.TypeError("can't convert complex to int; use int(abs(z))");

            if (o is Int64) {
                Int64 val = (Int64)o;
                if (Int32.MinValue <= val && val <= Int32.MaxValue) {
                    return (Int32)val;
                } else {
                    return BigInteger.Create(val);
                }
            }
            if (o is UInt32) {
                UInt32 val = (UInt32)o;
                if (val <= Int32.MaxValue) {
                    return (Int32)val;
                } else {
                    return BigInteger.Create(val);
                }
            }
            if (o is UInt64) {
                UInt64 val = (UInt64)o;
                if (val <= Int32.MaxValue) {
                    return (Int32)val;
                } else {
                    return BigInteger.Create(val);
                }
            }

            if (o is Decimal) {
                Decimal val = (Decimal)o;
                if (Int32.MinValue <= val && val <= Int32.MaxValue) {
                    return (Int32)val;
                } else {
                    return BigInteger.Create(val);
                }
            }

            if (o is Enum) {
                return ((IConvertible)o).ToInt32(null);
            }

            Extensible<string> es = o as Extensible<string>;
            if (es != null) {
                return __new__(null, es.Value, 10);
            }

            return _intSite.Invoke(DefaultContext.Default, o);
        }

        [StaticExtensionMethod]
        public static object __new__(CodeContext context, object o) {
            return __new__(context, TypeCache.Int32, o);
        }

        private static void ValidateType(PythonType cls) {
            if (cls == TypeCache.Boolean)
                throw PythonOps.TypeError("int.__new__(bool) is not safe, use bool.__new__()");
        }

        [StaticExtensionMethod]
        public static object __new__(PythonType cls, string s, int radix) {
            ValidateType(cls);

            // radix 16 allows a 0x preceding it... We either need a whole new
            // integer parser, or special case it here.
            if (radix == 16) {
                s = TrimRadix(s);
            }

            return LiteralParser.ParseIntegerSign(s, radix);
        }

        internal static string TrimRadix(string s) {
            for (int i = 0; i < s.Length; i++) {
                if (Char.IsWhiteSpace(s[i])) continue;

                if (s[i] == '0' && i < s.Length - 1 && (s[i + 1] == 'x' || s[i + 1] == 'X')) {
                    s = s.Substring(i + 2);
                }
                break;
            }
            return s;
        }

        [StaticExtensionMethod]
        public static object __new__(CodeContext context, PythonType cls, object x) {
            if (cls == TypeCache.Int32) return FastNew(x); // TODO: Call site?

            ValidateType(cls);

            // derived int creation...
            return cls.CreateInstance(context, x);
        }

        // "int()" calls ReflectedType.Call(), which calls "Activator.CreateInstance" and return directly.
        // this is for derived int creation or direct calls to __new__...
        [StaticExtensionMethod]
        public static object __new__(CodeContext context, PythonType cls) {
            if (cls == TypeCache.Int32) return 0;

            return cls.CreateInstance(context);
        }

        #region Binary Operators
        
        [SpecialName]
        public static object FloorDivide(int x, int y) {
            if (y == -1 && x == Int32.MinValue) {
                return -BigInteger.Create(Int32.MinValue);
            }
            return RuntimeHelpers.Int32ToObject(FloorDivideImpl(x, y));
        }

        internal static int FloorDivideImpl(int x, int y) {
            int q = x / y;

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

        [SpecialName]
        public static int Mod(int x, int y) {
            if (y == -1) return 0;
            int r = x % y;

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

        [SpecialName]
        public static object Power(int x, BigInteger power, BigInteger qmod) {
            return BigIntegerOps.Power((BigInteger)x, power, qmod);
        }

        [SpecialName]
        public static object Power(int x, double power, double qmod) {
            return PythonOps.NotImplemented;
        }

        [SpecialName]
        public static object Power(int x, int power, int? qmod) {
            if (qmod == null) return Power(x, power);
            int mod = (int)qmod;

            if (power < 0) throw PythonOps.TypeError("power", power, "power must be >= 0");

            if (mod == 0) {
                throw PythonOps.ZeroDivisionError();
            }

            // This is "exponentiation by squaring" (described in Applied Cryptography; a log-time algorithm)
            long result = 1 % mod; // Handle special case of power=0, mod=1
            long factor = x;
            while (power != 0) {
                if ((power & 1) != 0) result = (result * factor) % mod;
                factor = (factor * factor) % mod;
                power >>= 1;
            }

            // fix the sign for negative moduli or negative mantissas
            if ((mod < 0 && result > 0) || (mod > 0 && result < 0)) {
                result += mod;
            }
            return (int)result;
        }

        [SpecialName]
        public static object Power(int x, int power) {
            if (power == 0) return 1;
            if (power < 0) {
                if (x == 0)
                    throw PythonOps.ZeroDivisionError("0.0 cannot be raised to a negative power");
                return DoubleOps.Power(x, power);
            }
            int factor = x;
            int result = 1;
            int savePower = power;
            try {
                checked {
                    while (power != 0) {
                        if ((power & 1) != 0) result = result * factor;
                        if (power == 1) break; // prevent overflow
                        factor = factor * factor;
                        power >>= 1;
                    }
                    return result;
                }
            } catch (OverflowException) {
                return BigIntegerOps.Power(BigInteger.Create(x), savePower);
            }
        }


        [SpecialName]
        public static object LeftShift(int x, int y) {
            if (y < 0) {
                throw PythonOps.ValueError("negative shift count");
            }
            if (y > 31 ||
                (x > 0 && x > (Int32.MaxValue >> y)) ||
                (x < 0 && x < (Int32.MinValue >> y))) {
                return Int64Ops.LeftShift((long)x, y);
            }
            return RuntimeHelpers.Int32ToObject(x << y);
        }

        [SpecialName]
        public static int RightShift(int x, int y) {
            if (y < 0) {
                throw PythonOps.ValueError("negative shift count");
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

            return q;
        }

        #endregion

        public static PythonTuple __divmod__(int x, int y) {
            return PythonTuple.MakeTuple(Divide(x, y), Mod(x, y));
        }

        [return: MaybeNotImplemented]
        public static object __divmod__(int x, object y) {
            return PythonOps.NotImplemented;
        }


        #region Unary Operators

        public static string __oct__(int x) {
            if (x == 0) {
                return "0";
            } else if (x > 0) {
                return "0" + BigInteger.Create(x).ToString(8);
            } else {
                return "-0" + BigInteger.Create(-x).ToString(8);
            }
        }

        public static string __hex__(int x) {
            if (x < 0) {
                return "-0x" + (-x).ToString("x");
            } else {
                return "0x" + x.ToString("x");
            }
        }

        #endregion

        public static object __getnewargs__(CodeContext context, int self) {
            return PythonTuple.MakeTuple(Int32Ops.__new__(context, TypeCache.Int32, self));
        }

        public static object __rdivmod__(int x, int y) {
            return __divmod__(y, x);
        }

        public static int __int__(int self) {
            return self;
        }

        public static int __index__(int self) {
            return self;
        }

        public static BigInteger __long__(int self) {
            return (BigInteger)self;
        }

        public static double __float__(int self) {
            return (double)self;
        }

        public static int __abs__(int self) {
            return Math.Abs(self);
        }

        public static object __coerce__(CodeContext context, int x, object o) {
            // called via builtin.coerce()
            int val;
            if (Converter.TryConvertToInt32(o, out val)) {
                return PythonTuple.MakeTuple(x, val);
            }
            return PythonOps.NotImplemented;
        }
    }
}
