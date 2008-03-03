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
using System.Text;
using System.Collections;
using System.Threading;
using System.Runtime.CompilerServices;

using Microsoft.Scripting;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;

using IronPython.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Types;
using IronPython.Runtime.Operations;

[assembly: PythonExtensionType(typeof(BigInteger), typeof(BigIntegerOps), EnableDerivation=true)]
namespace IronPython.Runtime.Operations {

    public static partial class BigIntegerOps {
        private static readonly BigInteger DecimalMax = BigInteger.Create(Decimal.MaxValue);
        private static readonly BigInteger DecimalMin = BigInteger.Create(Decimal.MinValue);        

        [StaticExtensionMethod]
        public static object __new__(CodeContext context, PythonType cls, string s, int radix) {
            if (radix == 16) {
                s = Int32Ops.TrimRadix(s);
            }

            if (cls == TypeCache.BigInteger) {
                return ParseBigIntegerSign(s, radix);
            } else {
                BigInteger res = ParseBigIntegerSign(s, radix);
                return cls.CreateInstance(context, res);
            }
        }

        private static BigInteger ParseBigIntegerSign(string s, int radix) {
            try {
                return LiteralParser.ParseBigIntegerSign(s, radix);
            } catch (ArgumentException e) {
                throw PythonOps.ValueError(e.Message);
            }
        }

        [StaticExtensionMethod]
        public static object __new__(CodeContext context, PythonType cls, object x) {
            if (cls == TypeCache.BigInteger) {
                if (x is string) return ParseBigIntegerSign((string)x, 10);
                BigInteger intVal;
                if (Converter.TryConvertToBigInteger(x, out intVal)) {
                    if (Object.Equals(intVal, null)) throw PythonOps.TypeError("can't convert {0} to long", PythonTypeOps.GetName(x));
                    return intVal;
                }
            } else {
                BigInteger intVal = null;

                if (x is string) intVal = ParseBigIntegerSign((string)x, 10);
                if (Converter.TryConvertToBigInteger(x, out intVal)) {
                    if (Object.Equals(intVal, null)) throw PythonOps.TypeError("can't convert {0} to long", PythonTypeOps.GetName(x));
                }

                if (!Object.ReferenceEquals(intVal, null)) {
                    return cls.CreateInstance(context, intVal);
                }
            }

            if (x is Complex64) throw PythonOps.TypeError("can't convert complex to long; use long(abs(z))");

            throw PythonOps.ValueError("long argument must be convertible to long (string, number, or type that defines __long__, got {0})",
                StringOps.Quote(PythonOps.GetPythonTypeName(x)));
        }

        [StaticExtensionMethod]
        public static object __new__(CodeContext context, PythonType cls) {
            if (cls == TypeCache.BigInteger) {
                return BigInteger.Zero;
            } else {
                return cls.CreateInstance(context, BigInteger.Zero);
            }
        }

        #region Binary operators

        [SpecialName]
        public static object Power(BigInteger x, object y, object z) {
            if (y is int) {
                return Power(x, (int)y, z);
            } else if (y is long) {
                return Power(x, BigInteger.Create((long)y), z);
            } else if (y is BigInteger) {
                return Power(x, (BigInteger)y, z);
            }
            return PythonOps.NotImplemented;
        }

        [SpecialName]
        public static object Power(BigInteger x, int y, object z) {
            if (z is int) {
                return Power(x, y, BigInteger.Create((int)z));
            } else if (z is long) {
                return Power(x, y, BigInteger.Create((long)z));
            } else if (z is BigInteger) {
                return Power(x, y, (BigInteger)z);
            } else if (z == null) {
                return Power(x, y);
            }
            return PythonOps.NotImplemented;
        }

        [SpecialName]
        public static object Power(BigInteger x, BigInteger y, object z) {
            if (z is int) {
                return Power(x, y, BigInteger.Create((int)z));
            } else if (z is long) {
                return Power(x, y, BigInteger.Create((long)z));
            } else if (z is BigInteger) {
                return Power(x, y, (BigInteger)z);
            } else if (z == null) {
                return Power(x, y);
            }
            return PythonOps.NotImplemented;
        }

        [SpecialName]
        public static object Power(BigInteger x, int y, BigInteger z) {
            if (y < 0) {
                throw PythonOps.TypeError("power", y, "power must be >= 0");
            }
            if (z == BigInteger.Zero) {
                throw PythonOps.ZeroDivisionError();
            }

            BigInteger result = x.ModPow(y, z);

            // fix the sign for negative moduli or negative mantissas
            if ((z < BigInteger.Zero && result > BigInteger.Zero)
                || (z > BigInteger.Zero && result < BigInteger.Zero)) {
                result += z;
            }
            return result;
        }

        [SpecialName]
        public static object Power(BigInteger x, BigInteger y, BigInteger z) {
            if (y < BigInteger.Zero) {
                throw PythonOps.TypeError("power", y, "power must be >= 0");
            }
            if (z == BigInteger.Zero) {
                throw PythonOps.ZeroDivisionError();
            }

            BigInteger result = x.ModPow(y, z);

            // fix the sign for negative moduli or negative mantissas
            if ((z < BigInteger.Zero && result > BigInteger.Zero)
                || (z > BigInteger.Zero && result < BigInteger.Zero)) {
                result += z;
            }
            return result;
        }


        [SpecialName]
        public static object Power([NotNull]BigInteger x, int y) {
            if (y < 0) {
                return DoubleOps.Power(x.ToFloat64(), y);
            }
            return x.Power(y);
        }

        [SpecialName]
        public static object Power([NotNull]BigInteger x, [NotNull]BigInteger y) {
            if (Object.ReferenceEquals(x, null)) throw PythonOps.TypeError("unsupported operands for __pow__: NoneType and long");
            if (Object.ReferenceEquals(y, null)) throw PythonOps.TypeError("unsupported operands for __pow__: long and NoneType");

            int yl;
            if (y.AsInt32(out yl)) {
                return Power(x, yl);
            } else {
                if (x == BigInteger.Zero) {
                    if (y.IsNegative())
                        throw PythonOps.ZeroDivisionError("0.0 cannot be raised to a negative power");
                    return BigInteger.Zero;
                } else if (x == BigInteger.One) {
                    return BigInteger.One;
                } else {
                    throw PythonOps.ValueError("Number too big");
                }
            }
        }

        private static BigInteger DivMod(BigInteger x, BigInteger y, out BigInteger r) {
            BigInteger rr;
            BigInteger qq;

            if (Object.ReferenceEquals(x, null)) throw PythonOps.TypeError("unsupported operands for div/mod: NoneType and long");
            if (Object.ReferenceEquals(y, null)) throw PythonOps.TypeError("unsupported operands for div/mod: long and NoneType");

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

        [SpecialName]
        public static BigInteger Add([NotNull]BigInteger x, [NotNull]BigInteger y) {
            return x + y;
        }
        [SpecialName]
        public static BigInteger Subtract([NotNull]BigInteger x, [NotNull]BigInteger y) {
            return x - y;
        }
        [SpecialName]
        public static BigInteger Multiply([NotNull]BigInteger x, [NotNull]BigInteger y) {
            return x * y;
        }

        [SpecialName]
        public static BigInteger FloorDivide([NotNull]BigInteger x, [NotNull]BigInteger y) {
            return Divide(x, y);
        }

        [SpecialName]
        public static double TrueDivide([NotNull]BigInteger x, [NotNull]BigInteger y) {
            if (y == BigInteger.Zero) {
                throw new DivideByZeroException();
            }

            // first see if we can keep the two inputs as floats to give a precise result
            double fRes, fDiv;
            if (x.TryToFloat64(out fRes) && y.TryToFloat64(out fDiv)) {
                return fRes / fDiv;
            }

            // otherwise give the user the truncated result if the result fits in a float
            BigInteger rem;
            BigInteger res = BigInteger.DivRem(x, y, out rem);
            if (res.TryToFloat64(out fRes)) {                
                if(rem != BigInteger.Zero) {
                    // try and figure out the fractional portion
                    BigInteger fraction = y / rem;
                    if (fraction.TryToFloat64(out fDiv)) {
                        if (fDiv != 0) {
                            fRes += 1 / fDiv;
                        }
                    }
                }

                return fRes;
            }            

            // otherwise report an error
            throw PythonOps.OverflowError("long/long too large for a float");
        }

        [SpecialName]
        public static BigInteger Divide([NotNull]BigInteger x, [NotNull]BigInteger y) {
            BigInteger r;
            return DivMod(x, y, out r);
        }

        [SpecialName]
        public static BigInteger Mod([NotNull]BigInteger x, [NotNull]BigInteger y) {
            BigInteger r;
            DivMod(x, y, out r);
            return r;
        }


        [SpecialName]
        public static BigInteger LeftShift([NotNull]BigInteger x, int y) {
            if (y < 0) {
                throw PythonOps.ValueError("negative shift count");
            }
            return x << y;
        }

        [SpecialName]
        public static BigInteger RightShift([NotNull]BigInteger x, int y) {
            BigInteger q;
            if (y < 0) {
                throw PythonOps.ValueError("negative shift count");
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

        [SpecialName]
        public static BigInteger LeftShift([NotNull]BigInteger x, [NotNull]BigInteger y) {
            return LeftShift(x, y.ToInt32());
        }

        [SpecialName]
        public static BigInteger RightShift([NotNull]BigInteger x, [NotNull]BigInteger y) {
            return RightShift(x, y.ToInt32());
        }
        #endregion

        [SpecialName]
        public static PythonTuple DivMod(BigInteger x, BigInteger y) {
            BigInteger div, mod;
            div = DivMod(x, y, out mod);
            return PythonTuple.MakeTuple(div, mod);
        }

        #region Unary operators

        public static object __abs__(BigInteger x) {
            return x.Abs();
        }

        public static bool __nonzero__(BigInteger x) {
            return !x.IsZero();
        }

        [SpecialName]
        public static object Negate(BigInteger x) {
            return -x;
        }

        public static object __pos__(BigInteger x) {
            return x;
        }

        public static object __int__(BigInteger x) {
            // The python spec says __int__  should return a long if needed, rather than overflow.
            int i32;
            if (x.AsInt32(out i32)) {
                return i32;
            }

            return x;
        }

        public static object __float__(BigInteger self) {
            return self.ToFloat64();
        }

        public static string __oct__(BigInteger x) {
            if (x == BigInteger.Zero) {
                return "0L";
            } else if (x > 0) {
                return "0" + x.ToString(8) + "L";
            } else {
                return "-0" + (-x).ToString(8) + "L";
            }
        }

        public static string __hex__(BigInteger x) {
            // CPython 2.5 prints letters in lowercase, with a capital L. 
            if (x < 0) {
                return "-0x" + (-x).ToString(16).ToLower() + "L";
            } else {
                return "0x" + x.ToString(16).ToLower() + "L";
            }
        }

        public static object __getnewargs__(CodeContext context, BigInteger self) {
            if (!Object.ReferenceEquals(self, null)) {
                return PythonTuple.MakeTuple(BigIntegerOps.__new__(context, TypeCache.BigInteger, self));
            }
            throw PythonOps.TypeErrorForBadInstance("__getnewargs__ requires a 'long' object but received a '{0}'", self);
        }

        #endregion

        // These functions make the code generation of other types more regular
        internal static BigInteger OnesComplement(BigInteger x) {
            return ~x;
        }

        internal static BigInteger FloorDivideImpl(BigInteger x, BigInteger y) {
            return FloorDivide(x, y);
        }
        [SpecialName]
        public static BigInteger BitwiseAnd([NotNull]BigInteger x, [NotNull]BigInteger y) {
            return x & y;
        }
        [SpecialName]
        public static BigInteger BitwiseOr([NotNull]BigInteger x, [NotNull]BigInteger y) {
            return x | y;
        }
        [SpecialName]
        public static BigInteger ExclusiveOr([NotNull]BigInteger x, [NotNull]BigInteger y) {
            return x ^ y;
        }

        [SpecialName, ExplicitConversionMethod]
        public static int ConvertToInt32(BigInteger self) {
            int res;
            if (self.AsInt32(out res)) return res;

            throw Converter.CannotConvertOverflow("int", self);
        }

        [SpecialName, ImplicitConversionMethod]
        public static BigInteger ConvertToBigInteger(bool self) {
            return self ? BigInteger.One : BigInteger.Zero;
        }

        public static int __cmp__(BigInteger x, BigInteger y) {
            return x.CompareTo(y);
        }

        public static int __cmp__(CodeContext context, BigInteger x, int y) {
            int ix;
            if (x.AsInt32(out ix)) {                
                return ix == y ? 0 : ix > y ? 1 : -1;
            }

            return BigInteger.Compare(x, y);
        }

        public static int __cmp__(CodeContext context, BigInteger x, uint y) {
            uint ix;
            if (x.AsUInt32(out ix)) {
                return ix == y ? 0 : ix > y ? 1 : -1;
            }

            return BigInteger.Compare(x, y);
        }

        public static int __cmp__(CodeContext context, BigInteger x, double y) {
            return -((int)DoubleOps.Compare(y, x));
        }

        public static int __cmp__(CodeContext context, BigInteger x, Extensible<double> y) {
            return -((int)DoubleOps.Compare(y.Value, x));
        }

        public static int __cmp__(CodeContext context, BigInteger x, decimal y) {            
            return DecimalOps.__cmp__(x, y);
        }

        public static int __cmp__(CodeContext context, BigInteger x, bool y) {
            return __cmp__(x, y ? 1 : 0);
        }

        public static BigInteger __long__(BigInteger self) {
            return self;
        }

        public static BigInteger __index__(BigInteger self) {
            return self;
        }
    }
}
