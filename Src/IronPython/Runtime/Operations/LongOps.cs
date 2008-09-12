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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;
using Microsoft.Scripting.Runtime;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Math;

namespace IronPython.Runtime.Operations {

    public static partial class BigIntegerOps {
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
            Extensible<string> es;

            if (x is string) {
                return ReturnBigInteger(context, cls, ParseBigIntegerSign((string)x, 10));
            } else if ((es = x as Extensible<string>) != null) {
                object value;
                if (PythonTypeOps.TryInvokeUnaryOperator(context, x, Symbols.ConvertToLong, out value)) {
                    return ReturnBigInteger(context, cls, (BigInteger)value);
                }

                return ReturnBigInteger(context, cls, ParseBigIntegerSign(es.Value, 10));
            }

            BigInteger intVal;
            if (Converter.TryConvertToBigInteger(x, out intVal)) {
                if (Object.Equals(intVal, null)) throw PythonOps.TypeError("can't convert {0} to long", PythonTypeOps.GetName(x));

                return ReturnBigInteger(context, cls, intVal);
            }


            if (x is Complex64) throw PythonOps.TypeError("can't convert complex to long; use long(abs(z))");

            throw PythonOps.ValueError("long argument must be convertible to long (string, number, or type that defines __long__, got {0})",
                StringOps.Quote(PythonOps.GetPythonTypeName(x)));
        }

        private static object ReturnBigInteger(CodeContext context, PythonType cls, BigInteger intVal) {
            if (cls == TypeCache.BigInteger) {
                return intVal;
            } else {
                return cls.CreateInstance(context, intVal);
            }
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
            return NotImplementedType.Value;
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
            return NotImplementedType.Value;
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
            return NotImplementedType.Value;
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
                return Microsoft.Scripting.Runtime.RuntimeHelpers.Int32ToObject(i32);
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

        [SpecialName]
        public static int Compare(BigInteger x, BigInteger y) {
            return x.CompareTo(y);
        }

        [SpecialName]
        public static int Compare(BigInteger x, int y) {
            int ix;
            if (x.AsInt32(out ix)) {                
                return ix == y ? 0 : ix > y ? 1 : -1;
            }

            return BigInteger.Compare(x, y);
        }

        [SpecialName]
        public static int Compare(BigInteger x, uint y) {
            uint ix;
            if (x.AsUInt32(out ix)) {
                return ix == y ? 0 : ix > y ? 1 : -1;
            }

            return BigInteger.Compare(x, y);
        }

        [SpecialName]
        public static int Compare(BigInteger x, double y) {
            return -((int)DoubleOps.Compare(y, x));
        }

        [SpecialName]
        public static int Compare(BigInteger x, Extensible<double> y) {
            return -((int)DoubleOps.Compare(y.Value, x));
        }

        [SpecialName]
        public static int Compare(BigInteger x, decimal y) {            
            return DecimalOps.__cmp__(x, y);
        }

        [SpecialName]
        public static int Compare(BigInteger x, bool y) {
            return Compare(x, y ? 1 : 0);
        }

        public static BigInteger __long__(BigInteger self) {
            return self;
        }

        public static BigInteger __index__(BigInteger self) {
            return self;
        }

        public static int __hash__(BigInteger self) {
            // Call the DLR's BigInteger hash function, which will return an int32 representation of
            // b if b is within the int32 range. We use that as an optimization for hashing, and 
            // assert the assumption below.
            int hash = self.GetHashCode();
#if DEBUG
            int i;
            if (self.AsInt32(out i)) {
                Debug.Assert(i == hash, "input:" + i);
            }
#endif
            return hash;
        }

        public static string __repr__([NotNull]BigInteger/*!*/ self) {
            return self.ToString() + "L";
        }

        public static object __coerce__(CodeContext context, BigInteger self, object o) {
            // called via builtin.coerce()
            BigInteger val;
            if (Converter.TryConvertToBigInteger(o, out val)) {
                return PythonTuple.MakeTuple(self, val);
            }
            return NotImplementedType.Value;
        }

        // provided for backwards compatibility...
        [PythonHidden]
        public static float ToFloat(BigInteger/*!*/ self) {
            return checked((float)self.ToFloat64());
        }
    }
}
