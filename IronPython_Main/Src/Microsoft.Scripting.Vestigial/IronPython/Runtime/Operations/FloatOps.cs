/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Text;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Threading;

using Microsoft.Scripting;
using Microsoft.Scripting.Internal;
using Microsoft.Scripting.Math;
using IronPython.Runtime;
using IronPython.Runtime.Types;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Operations {

    public static partial class DoubleOps {
        [StaticOpsMethod("__new__")]
        public static object Make(CodeContext context, DynamicType cls) {
            if (cls == TypeCache.Double) return 0.0;

            return cls.CreateInstance(context);
        }

        [StaticOpsMethod("__new__")]
        public static object Make(CodeContext context, DynamicType cls, object x) {
            if (cls == TypeCache.Double) {
                if (x is string) {
                    return ParseFloat((string)x);
                }
                if (x is char) {
                    return ParseFloat(RuntimeHelpers.CharToString((char)x));
                }

                double doubleVal;
                if (Converter.TryConvertToDouble(x, out doubleVal)) return doubleVal;

                if (x is Complex64) throw Ops.TypeError("can't convert complex to float; use abs(z)");

                object d = Ops.CallWithContext(context, Ops.GetBoundAttr(context, x, Symbols.ConvertToFloat));
                if (d is double) return d;
                throw Ops.TypeError("__float__ returned non-float (type %s)", Ops.GetDynamicType(d));
            } else {
                return cls.CreateInstance(context, x);
            }
        }

        private static object ParseFloat(string x) {
            try {
                return LiteralParser.ParseFloat(x);
            } catch (FormatException) {
                throw Ops.ValueError("invalid literal for float(): {0}", x);
            }
        }


        #region Binary operators

        [OperatorMethod]
        public static double Mod(double x, double y) {
            if (y == 0) throw Ops.ZeroDivisionError();

            double r = x % y;
            if (r > 0 && y < 0) {
                r = r + y;
            } else if (r < 0 && y > 0) {
                r = r + y;
            }
            return r;
        }

        [OperatorMethod]
        public static double Power(double x, double y) {
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
        #endregion

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "x"), OperatorMethod, PythonName("__coerce__")]
        public static double Coerce(CodeContext context, double x, object o) {
            // REVIEW: How is this used?
            double d = (double)Make(context, TypeCache.Double, o);

            if (Double.IsInfinity(d)) {
                throw Ops.OverflowError("number too big");
            }

            return d;
        }
        
        internal static object DivMod(double x, double y) {
            object div = FloorDivide(x, y);
            if (div == Ops.NotImplemented) return div;
            return Tuple.MakeTuple(div, Mod(x, y));
        }

        #region Unary operators
        [OperatorMethod, PythonName("__int__")]
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
        public static object GetNewArgs(CodeContext context, double self) {
            return Tuple.MakeTuple(DoubleOps.Make(context, TypeCache.Double, self));
        }
        #endregion

        #region ToString

       [OperatorMethod, PythonName("__str__")]
        public static string ToString(double x) {
            StringFormatter sf = new StringFormatter("%.12g", x);
            sf._TrailingZeroAfterWholeFloat = true;
            return sf.Format();
        }

        [OperatorMethod, PythonName("__str__")]
        public static string ToString(double x, IFormatProvider provider) {
            return x.ToString(provider);
        }
        [OperatorMethod, PythonName("__str__")]
        public static string ToString(double x, string format) {
            return x.ToString(format);
        }
        [OperatorMethod, PythonName("__str__")]
        public static string ToString(double x, string format, IFormatProvider provider) {
            return x.ToString(format, provider);
        }

        [OperatorMethod, PythonName("__hash__")]
        public static int GetHashCode(double x) {
            return (int)x;
        }

        public static string ToString(float x) {
            // Python does not natively support System.Single. However, we try to provide
            // formatting consistent with System.Double.
            StringFormatter sf = new StringFormatter("%.6g", x);
            sf._TrailingZeroAfterWholeFloat = true;
            return sf.Format();
        }

        #endregion

        [OperatorMethod]
        public static bool LessThan(double x, BigInteger y) {
            return Compare(x, y) < 0;
        }
        [OperatorMethod]
        public static bool LessThanOrEqual(double x, BigInteger y) {
            return Compare(x, y) <= 0;
        }
        [OperatorMethod]
        public static bool GreaterThan(double x, BigInteger y) {
            return Compare(x, y) > 0;
        }
        [OperatorMethod]
        public static bool GreaterThanOrEqual(double x, BigInteger y) {
            return Compare(x, y) >= 0;
        }
        [OperatorMethod]
        public static bool Equal(double x, BigInteger y) {
            return Compare(x, y) == 0;
        }
        [OperatorMethod]
        public static bool NotEqual(double x, BigInteger y) {
            return Compare(x, y) != 0;
        }

        internal static int Compare(double x, BigInteger y) {
            return -Compare(y, x);
        }

        internal static int Compare(BigInteger x, double y) {
            // BigInts can hold doubles, but doubles can't hold BigInts, so
            // if we're comparing against a BigInt then we should convert ourself
            // to a long and then compare.
            if (object.ReferenceEquals(x,null)) return -1;
            BigInteger by = BigInteger.Create(y);
            if (by == x) {
                double mod = y % 1;
                if (mod == 0) return 0;
                if (mod > 0) return -1;
                return +1;
            }
            if (by > x) return -1;
            return +1;
        }

        [OperatorMethod]
        public static bool LessThan(double x, decimal y) {
            return Compare(x, y) < 0;
        }
        [OperatorMethod]
        public static bool LessThanOrEqual(double x, decimal y) {
            return Compare(x, y) <= 0;
        }
        [OperatorMethod]
        public static bool GreaterThan(double x, decimal y) {
            return Compare(x, y) > 0;
        }
        [OperatorMethod]
        public static bool GreaterThanOrEqual(double x, decimal y) {
            return Compare(x, y) >= 0;
        }
        [OperatorMethod]
        public static bool Equal(double x, decimal y) {
            return Compare(x, y) == 0;
        }
        [OperatorMethod]
        public static bool NotEqual(double x, decimal y) {
            return Compare(x, y) != 0;
        }


        internal static int Compare(double x, decimal y) {
            if (x > (double)decimal.MaxValue) return +1;
            if (x < (double)decimal.MinValue) return -1;
            return ((decimal)x).CompareTo(y);
        }

        [OperatorMethod]
        public static bool LessThan(Double x, int y) {
            return x < y;
        }
        [OperatorMethod]
        public static bool LessThanOrEqual(Double x, int y) {
            return x <= y;
        }
        [OperatorMethod]
        public static bool GreaterThan(Double x, int y) {
            return x > y;
        }
        [OperatorMethod]
        public static bool GreaterThanOrEqual(Double x, int y) {
            return x >= y;
        }
        [OperatorMethod]
        public static bool Equal(Double x, int y) {
            return x == y;
        }
        [OperatorMethod]
        public static bool NotEqual(Double x, int y) {
            return x != y;
        }

        [PythonName("__cmp__")]
        [return: MaybeNotImplemented]
        public static object Compare(CodeContext context, double self, object other) {
            if (other == null) return 1;

            BigInteger bi = other as BigInteger;
            if (!Object.ReferenceEquals(bi, null)) {
                return Compare(self, bi);
            }

            // everything else can be held by a double.
            double val;
            if (Converter.TryConvertToDouble(other, out val)) {
                if (val == self) return 0;
                if (self < val) return -1;
                return 1;
            } else {
                object res;
                if (Ops.GetDynamicType(other).TryInvokeBinaryOperator(context, Operators.Coerce, other, self, out res)) {
                    if (res != Ops.NotImplemented && !(res is OldInstance)) {
                        return Ops.Compare(context, ((Tuple)res)[1], ((Tuple)res)[0]);
                    }
                }

                Complex64 c64;
                if (Converter.TryConvertToComplex64(other, out c64)) {
                    return ComplexOps.TrueCompare(context, c64, new Complex64(self)) * -1;
                }

                return Ops.NotImplemented;
            }
        }

    }

    public partial class SingleOps {
        [OperatorMethod]
        public static float Mod(float x, float y) {
            return (float)DoubleOps.Mod(x, y);
        }

        [OperatorMethod]
        public static float Power(float x, float y) {
            return (float)DoubleOps.Power(x, y);
        }
    }
}
