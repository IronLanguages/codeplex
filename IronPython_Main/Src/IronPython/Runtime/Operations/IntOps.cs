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

using System; using Microsoft;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime.Types;

using SpecialNameAttribute = System.Runtime.CompilerServices.SpecialNameAttribute;

namespace IronPython.Runtime.Operations {

    public static partial class Int32Ops {
        private static object FastNew(CodeContext/*!*/ context, object o) {
            Extensible<BigInteger> el;

            if (o is string) return __new__(null, (string)o, 10);
            if (o is double) return DoubleOps.__int__((double)o);
            if (o is int) return o;
            if (o is BigInteger) {
                BigInteger bi = o as BigInteger;
                int res;
                if (bi.AsInt32(out res)) {
                    return ScriptingRuntimeHelpers.Int32ToObject(res);
                }
                return o;
            }

            if ((el = o as Extensible<BigInteger>) != null) {
                int res;
                if (el.Value.AsInt32(out res)) {
                    return ScriptingRuntimeHelpers.Int32ToObject(res);
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
            } else if (o is UInt32) {
                UInt32 val = (UInt32)o;
                if (val <= Int32.MaxValue) {
                    return (Int32)val;
                } else {
                    return BigInteger.Create(val);
                }
            } else if (o is UInt64) {
                UInt64 val = (UInt64)o;
                if (val <= Int32.MaxValue) {
                    return (Int32)val;
                } else {
                    return BigInteger.Create(val);
                }
            } else if (o is Decimal) {
                Decimal val = (Decimal)o;
                if (Int32.MinValue <= val && val <= Int32.MaxValue) {
                    return (Int32)val;
                } else {
                    return BigInteger.Create(val);
                }
            } else if (o is Enum) {
                return ((IConvertible)o).ToInt32(null);
            }

            Extensible<string> es = o as Extensible<string>;
            if (es != null) {
                // __int__ takes precedence, call it if it's available...
                object value;
                if (PythonTypeOps.TryInvokeUnaryOperator(DefaultContext.Default, es, Symbols.ConvertToInt, out value)) {
                    return value;
                }

                // otherwise call __new__ on the string value
                return __new__(null, es.Value, 10);
            }

            return PythonContext.GetContext(context).ImplicitConvertTo<int>(o);
        }

        [StaticExtensionMethod]
        public static object __new__(CodeContext context, object o) {
            return __new__(context, TypeCache.Int32, o);
        }

        [StaticExtensionMethod]
        public static object __new__(CodeContext context, PythonType cls, Extensible<double> o) {
            object value;
            // always succeeds as float defines __int__
            PythonTypeOps.TryInvokeUnaryOperator(context, o, Symbols.ConvertToInt, out value);
            if (cls == TypeCache.Int32) {
                return (int)value;
            } else {
                return cls.CreateInstance(context, value);
            }
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

        [StaticExtensionMethod]
        public static object __new__(CodeContext/*!*/ context, PythonType cls, IList<byte> s) {
            if (cls == TypeCache.Int32) {
                object value;
                IPythonObject po = s as IPythonObject;
                if (po != null &&
                    PythonTypeOps.TryInvokeUnaryOperator(DefaultContext.Default, po, Symbols.ConvertToInt, out value)) {
                    return value;
                }

                return FastNew(context, s.MakeString());
            }

            ValidateType(cls);

            // derived int creation...
            return cls.CreateInstance(context, FastNew(context, s.MakeString()));
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
            if (cls == TypeCache.Int32) return FastNew(context, x); // TODO: Call site?

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
            return ScriptingRuntimeHelpers.Int32ToObject(MathUtils.FloorDivideUnchecked(x, y));
        }

        [SpecialName]
        public static int Mod(int x, int y) {
            return MathUtils.FloorRemainder(x, y);
        }

        [SpecialName]
        public static object Power(int x, BigInteger power, BigInteger qmod) {
            return BigIntegerOps.Power((BigInteger)x, power, qmod);
        }

        [SpecialName]
        public static object Power(int x, double power, double qmod) {
            return NotImplementedType.Value;
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
            return ScriptingRuntimeHelpers.Int32ToObject(x << y);
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
            return NotImplementedType.Value;
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
            return NotImplementedType.Value;
        }

        public static string __format__(CodeContext/*!*/ context, int self, [NotNull]string/*!*/ formatSpec) {
            StringFormatSpec spec = StringFormatSpec.FromString(formatSpec);

            if (spec.Precision != null) {
                throw PythonOps.ValueError("Precision not allowed in integer format specifier");
            }

            string digits;
            switch (spec.Type) {
                case 'n':
                    CultureInfo culture = PythonContext.GetContext(context).NumericCulture;

                    if (culture == CultureInfo.InvariantCulture) {
                        // invariant culture maps to CPython's C culture, which doesn't
                        // include any formatting info.
                        goto case 'd';
                    }

                    digits = self.ToString("N0", PythonContext.GetContext(context).NumericCulture);
                    break;
                case null:
                case 'd':
                    digits = self.ToString("D", CultureInfo.InvariantCulture);
                    break;
                case '%': digits = self.ToString("0.000000%", CultureInfo.InvariantCulture); break;
                case 'e': digits = self.ToString("0.000000e+00", CultureInfo.InvariantCulture); break;
                case 'E': digits = self.ToString("0.000000E+00", CultureInfo.InvariantCulture); break;
                case 'f': digits = self.ToString("##########.000000", CultureInfo.InvariantCulture); break;
                case 'F': digits = self.ToString("##########.000000", CultureInfo.InvariantCulture); break;
                case 'g':
                    if (self >= 1000000 || self <= -1000000) {
                        digits = self.ToString("0.#####e+00", CultureInfo.InvariantCulture);
                    } else {
                        digits = self.ToString(CultureInfo.InvariantCulture);
                    }
                    break;
                case 'G':
                    if (self >= 1000000 || self <= -1000000) {
                        digits = self.ToString("0.#####E+00", CultureInfo.InvariantCulture);
                    } else {
                        digits = self.ToString(CultureInfo.InvariantCulture);
                    }
                    break;
                case 'X':
                    if (self != Int32.MinValue) {
                        int val = self;
                        if (self < 0) {
                            val = -self;
                        }
                        digits = val.ToString("X", CultureInfo.InvariantCulture);
                    } else {
                        digits = "80000000";
                    }

                    if (spec.IncludeType) {
                        digits = "0X" + digits;
                    }
                    break;
                case 'x':
                    digits = ToHex(self, spec.IncludeType);
                    break;
                case 'b': // binary
                    digits = ToBinary(self, spec.IncludeType);
                    break;
                case 'c': // single char
                    if (spec.Sign != null) {
                        throw PythonOps.ValueError("Sign not allowed with integer format specifier 'c'");
                    }
                    
                    if (self < Char.MinValue || self > Char.MaxValue) {
                        throw PythonOps.OverflowError("%c arg not in range(0x10000)");
                    }

                    digits = Builtin.chr(self);
                    break;
                case 'o': // octal
                    if (self == 0) {
                        digits = "0";
                    } else if (self != Int32.MinValue) {
                        int val = self;
                        if (self < 0) {
                            val = -self;
                        }

                        StringBuilder sbo = new StringBuilder();
                        for (int i = 30; i >= 0; i -= 3) {
                            char value = (char)('0' + (val >> i & 0x07));
                            if (value != '0' || sbo.Length > 0) {
                                sbo.Append(value);
                            }
                        }
                        digits = sbo.ToString();
                    } else {
                        digits = "20000000000";
                    }
                    
                    if (spec.IncludeType) {
                        digits = "0o" + digits;
                    }
                    break;
                default:
                    throw PythonOps.ValueError("Unknown conversion type {0}", spec.Type.ToString());
            }

            if (self < 0 && digits[0] == '-') {
                digits = digits.Substring(1);
            }

            return spec.AlignNumericText(digits, self == 0, self > 0);
        }

        public static string/*!*/ __repr__(int self) {
            return self.ToString(CultureInfo.InvariantCulture);
        }

        internal static string ToHex(int self, bool includeType) {
            string digits;
            if (self != Int32.MinValue) {
                int val = self;
                if (self < 0) {
                    val = -self;
                }
                digits = val.ToString("x", CultureInfo.InvariantCulture);
            } else {
                digits = "80000000";
            }

            if (includeType) {
                digits = "0x" + digits;
            }
            return digits;
        }

        internal static string ToBinary(int self, bool includeType) {
            string digits;
            if (self == 0) {
                digits = "0";
            } else if (self != Int32.MinValue) {
                StringBuilder sbb = new StringBuilder();

                int val = self;
                if (self < 0) {
                    val = -self;
                }

                for (int i = 31; i >= 0; i--) {
                    if ((val & (1 << i)) != 0) {
                        sbb.Append('1');
                    } else if (sbb.Length != 0) {
                        sbb.Append('0');
                    }
                }
                digits = sbb.ToString();
            } else {
                digits = "10000000000000000000000000000000";
            }

            if (includeType) {
                digits = "0b" + digits;
            }
            return digits;
        }
    }
}
