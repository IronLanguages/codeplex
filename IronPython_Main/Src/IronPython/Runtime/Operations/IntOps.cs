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

using IronPython.Runtime.Calls;
using IronPython.Runtime.Types;

using SpecialNameAttribute = System.Runtime.CompilerServices.SpecialNameAttribute;

namespace IronPython.Runtime.Operations {

    public class ExtensibleInt : Extensible<int> {
        public ExtensibleInt() : base() { }
        public ExtensibleInt(int v) : base(v) { }

        [PythonName("__cmp__")]
        [return: MaybeNotImplemented]
        public virtual object Compare(CodeContext context, object obj) {
            return Int32Ops.Compare(context, Value, obj);
        }
    }


    public static partial class Int32Ops {
        private static FastDynamicSite<object, object> _intSite = FastDynamicSite<object, object>.Create(DefaultContext.Default, ConvertToAction.Make(typeof(int)));

        private static object FastNew(object o) {
            Extensible<BigInteger> el;

            if (o is string) return Make(null, (string)o, 10);
            if (o is double) return DoubleOps.ToInteger((double)o);
            if (o is int) return o;
            if (o is BigInteger) return o;
            if ((el = o as Extensible<BigInteger>) != null) return el.Value;
            if (o is float) return DoubleOps.ToInteger((double)(float)o);

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

            if(o is Enum) {
                return ((IConvertible)o).ToInt32(null);
            }

            return _intSite.Invoke(o);
        }

        public static object Make(CodeContext context, object o) {
            return Make(context, TypeCache.Int32, o);
        }

        private static void ValidateType(PythonType cls) {
            if (cls == TypeCache.Boolean)
                throw PythonOps.TypeError("int.__new__(bool) is not safe, use bool.__new__()");
        }

        [StaticExtensionMethod("__new__")]
        public static object Make(PythonType cls, string s, int radix) {
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

                if (s[i] == '0' && i < s.Length - 1 && s[i + 1] == 'x') {
                    s = s.Substring(i + 2);
                }
                break;
            }
            return s;
        }

        [StaticExtensionMethod("__new__")]
        public static object Make(CodeContext context, PythonType cls, object x) {
            if (cls == TypeCache.Int32)  return FastNew(x); // TODO: Call site?

            ValidateType(cls);

            // derived int creation...
            return cls.CreateInstance(context, x);
        }

        // "int()" calls ReflectedType.Call(), which calls "Activator.CreateInstance" and return directly.
        // this is for derived int creation or direct calls to __new__...
        [StaticExtensionMethod("__new__")]
        public static object Make(CodeContext context, PythonType cls) {
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

        public static int FloorDivideImpl(int x, int y) {
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

        [PythonName("__divmod__")]
        public static object DivMod(int x, int y) {
            return PythonTuple.MakeTuple(Divide(x, y), Mod(x, y));
        }

        [PythonName("__divmod__")]
        public static object DivMod(int x, object y) {
            return PythonOps.NotImplemented;
        }


        #region Unary Operators
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
                return "-0x" + (-x).ToString("x");
            } else {
                return "0x" + x.ToString("x");
            }
        }

        #endregion

        [PythonName("__getnewargs__")]
        public static object GetNewArgs(CodeContext context, int self) {
            return PythonTuple.MakeTuple(Int32Ops.Make(context, TypeCache.Int32, self));
        }

        [SpecialName, PythonName("__cmp__")]
        [return: MaybeNotImplemented]
        public static object Compare(CodeContext context, int self, object obj) {
            if (obj == null) return RuntimeHelpers.Int32ToObject(1);

            int otherInt;

            if (obj is int) {
                otherInt = (int)obj;
            } else if (obj is ExtensibleInt) {
                otherInt = ((ExtensibleInt)obj).Value;
            } else if (obj is bool) {
                otherInt = ((bool)obj) ? 1 : 0;
            } else if (obj is double) {
                // compare as double to avoid truncation issues
                return DoubleOps.Compare(context, (double)self, (double)obj);
            } else if (obj is Extensible<double>) {
                // compare as double to avoid truncation issues
                return DoubleOps.Compare(context, (double)self, ((Extensible<double>)obj).Value);
            } else if (obj is Decimal) {
                return DoubleOps.Compare(context, (double)self, (double)(decimal)obj);
            } else if (obj is string) {
                return PythonOps.NotImplemented; // just avoiding exception path here...
            } else {
                if (!Converter.TryConvertToInt32(obj, out otherInt)) {
                    object res;
                    if(DynamicHelpers.GetPythonType(obj).TryInvokeBinaryOperator(context,
                        Operators.Coerce,
                        obj,
                        self, 
                        out res)) {
                        if (res != PythonOps.NotImplemented && !(res is OldInstance)) {
                            return PythonOps.Compare(context, ((PythonTuple)res)[1], ((PythonTuple)res)[0]);
                        }
                    }
                    return PythonOps.NotImplemented;
                }
            }

            return Compare(self, otherInt);
        }

        internal static object ReverseDivMod(int x, int y) {
            return DivMod(y, x);
        }

        [PythonName("__int__")]
        public static int __int__(int self) {
            return self;
        }

        [SpecialName, PythonName("__coerce__")]
        public static object Coerce(CodeContext context, int x, object o) {
            // called via builtin.coerce()
            int val;
            if (Converter.TryConvertToInt32(o, out val)) {
                return PythonTuple.MakeTuple(x, val);
            }
            return PythonOps.NotImplemented;
        }

    }
}
