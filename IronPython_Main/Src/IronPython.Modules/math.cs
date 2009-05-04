/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System; using Microsoft;
using System.Collections;

using Microsoft.Scripting;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;

using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

[assembly: PythonModule("math", typeof(IronPython.Modules.PythonMath))]
namespace IronPython.Modules {
    public static partial class PythonMath {
        public const double pi = Math.PI;
        public const double e = Math.E;

        private const double degreesToRadians = Math.PI / 180.0;
        private const int Bias = 0x3FE;

        public static double degrees(double radians) {
            return Check(radians, radians / degreesToRadians);
        }

        public static double radians(double degrees) {
            return Check(degrees, degrees * degreesToRadians);
        }

        public static double fmod(double v, double w) {
            return Check(v, w, v % w);
        }

        public static double fsum(IEnumerable e) {
            IEnumerator en = e.GetEnumerator();
            double value = 0.0;
            while (en.MoveNext()) {
                value += Converter.ConvertToDouble(en.Current);
            }
            return value;
        }

        public static PythonTuple frexp(double v) {
            if (Double.IsInfinity(v) || Double.IsNaN(v)) {
                return PythonTuple.MakeTuple(v, 0.0);
            }
            int exponent = 0;
            double mantissa = 0;

            if (v == 0) {
                mantissa = 0;
                exponent = 0;
            } else {
                byte[] vb = BitConverter.GetBytes(v);
                if (BitConverter.IsLittleEndian) {
                    DecomposeLe(vb, out mantissa, out exponent);
                } else {
                    throw new NotImplementedException();
                }
            }

            return PythonTuple.MakeTuple(mantissa, exponent);
        }

        public static PythonTuple modf(double v) {
            if (double.IsInfinity(v)) {
                return PythonTuple.MakeTuple(0.0, v);
            }
            double w = v % 1.0;
            v -= w;
            return PythonTuple.MakeTuple(w, v);
        }

        public static double ldexp(double v, BigInteger w) {
            if (v == 0.0 || double.IsInfinity(v)) {
                return v;
            }
            return Check(v, v * Math.Pow(2.0, w));
        }

        public static double hypot(double v, double w) {
            if (double.IsInfinity(v) || double.IsInfinity(w)) {
                return double.PositiveInfinity;
            }
            return Check(v, w, Complex64.Hypot(v, w));
        }

        public static double pow(double v, double exp) {
            if (v == 1.0 || exp == 0.0) {
                return 1.0;
            } else if (double.IsNaN(v) || double.IsNaN(exp)) {
                return double.NaN;
            } else if (v == 0.0) {
                if (exp > 0.0) {
                    return 0.0;
                }
                throw PythonOps.ValueError("math domain error");
            } else if (double.IsPositiveInfinity(exp)) {
                if (v > 1.0 || v < -1.0) {
                    return double.PositiveInfinity;
                } else if (v == -1.0) {
                    return 1.0;
                } else {
                    return 0.0;
                }
            } else if (double.IsNegativeInfinity(exp)) {
                if (v > 1.0 || v < -1.0) {
                    return 0.0;
                } else if (v == -1.0) {
                    return 1.0;
                } else {
                    return double.PositiveInfinity;
                }
            }
            return Check(v, exp, Math.Pow(v, exp));
        }

        public static double log(double v0) {
            if (v0 <= 0.0) {
                throw PythonOps.ValueError("math domain error");
            }
            return Check(v0, Math.Log(v0));
        }

        public static double log(double v0, double v1) {
            if (v0 <= 0.0 || v1 == 0.0) {
                throw PythonOps.ValueError("math domain error");
            } else if (v1 == 1.0) {
                throw PythonOps.ZeroDivisionError("float division");
            } else if (v1 == Double.PositiveInfinity) {
                return 0.0;
            }
            return Check(Math.Log(v0, v1));
        }

        public static double log(BigInteger value) {
            return Check(value.Log());
        }

        public static double log(object value) {
            // CPython tries float first, then double, so we need
            // an explicit overload which properly matches the order here
            double val;
            if (Converter.TryConvertToDouble(value, out val)) {
                return log(val);
            } else {
                return log(Converter.ConvertToBigInteger(value));
            }
        }

        public static double log(BigInteger value, double newBase) {
            if (newBase <= 0.0 || value <= 0) {
                throw PythonOps.ValueError("math domain error");
            } else if (newBase == 1.0) {
                throw PythonOps.ZeroDivisionError("float division");
            } else if (newBase == Double.PositiveInfinity) {
                return 0.0;
            }
            return Check(value.Log(newBase));
        }

        public static double log(object value, double newBase) {
            // CPython tries float first, then double, so we need
            // an explicit overload which properly matches the order here
            double val;
            if (Converter.TryConvertToDouble(value, out val)) {
                return log(val, newBase);
            } else {
                return log(Converter.ConvertToBigInteger(value), newBase);
            }
        }

        public static double log10(double v0) {
            if (v0 <= 0.0) {
                throw PythonOps.ValueError("math domain error");
            }
            return Check(v0, Math.Log10(v0));
        }

        public static double log10(BigInteger value) {
            return Check(value.Log10());
        }

        public static double log10(object value) {
            // CPython tries float first, then double, so we need
            // an explicit overload which properly matches the order here
            double val;
            if (Converter.TryConvertToDouble(value, out val)) {
                return log10(val);
            } else {
                return log10(Converter.ConvertToBigInteger(value));
            }
        }

        public static double log1p(double v0) {
            return log(v0 + 1.0);
        }

        public static double log1p(BigInteger value) {
            return log(value + (BigInteger)1);
        }

        public static double log1p(object value) {
            // CPython tries float first, then double, so we need
            // an explicit overload which properly matches the order here
            double val;
            if (Converter.TryConvertToDouble(value, out val)) {
                return log(val + 1.0);
            } else {
                return log(Converter.ConvertToBigInteger(value) + (BigInteger)1);
            }
        }

        public static double asinh(double v0) {
            if (v0 == 0.0 || double.IsInfinity(v0)) {
                return v0;
            }
            // rewrote ln(v0 + sqrt(v0**2 + 1)) for precision
            if (Math.Abs(v0) > 1.0) {
                return Math.Log(v0) + Math.Log(1.0 + Complex64.Hypot(1.0, 1.0 / v0));
            } else {
                return Math.Log(v0 + Complex64.Hypot(1.0, v0));
            }
        }

        public static double asinh(BigInteger value) {
            if (value == 0) {
                return 0;
            }
            // rewrote ln(v0 + sqrt(v0**2 + 1)) for precision
            if (value.Abs() > 1) {
                return value.Log() + Math.Log(1.0 + Complex64.Hypot(1.0, 1.0 / value));
            } else {
                return Math.Log(value + Complex64.Hypot(1.0, value));
            }
        }

        public static double asinh(object value) {
            // CPython tries float first, then double, so we need
            // an explicit overload which properly matches the order here
            double val;
            if (Converter.TryConvertToDouble(value, out val)) {
                return asinh(val);
            } else {
                return asinh(Converter.ConvertToBigInteger(value));
            }
        }

        public static double acosh(double v0) {
            if (v0 < 1.0) {
                throw PythonOps.ValueError("math domain error");
            } else if (double.IsPositiveInfinity(v0)) {
                return double.PositiveInfinity;
            }
            // rewrote ln(v0 + sqrt(v0**2 - 1)) for precision
            double c = Math.Sqrt(v0 + 1.0);
            return Math.Log(c) + Math.Log(v0 / c + Math.Sqrt(v0 - 1.0));
        }

        public static double acosh(BigInteger value) {
            if (value <= 0) {
                throw PythonOps.ValueError("math domain error");
            }
            // rewrote ln(v0 + sqrt(v0**2 - 1)) for precision
            double c = Math.Sqrt(value + (BigInteger)1);
            return Math.Log(c) + Math.Log(value / c + Math.Sqrt(value - (BigInteger)1));
        }

        public static double acosh(object value) {
            // CPython tries float first, then double, so we need
            // an explicit overload which properly matches the order here
            double val;
            if (Converter.TryConvertToDouble(value, out val)) {
                return acosh(val);
            } else {
                return acosh(Converter.ConvertToBigInteger(value));
            }
        }

        public static double atanh(double v0) {
            if (v0 >= 1.0 || v0 <= -1.0) {
                throw PythonOps.ValueError("math domain error");
            } else if (v0 == 0.0) {
                // preserve +/-0.0
                return v0;
            }

            return Math.Log((1.0 + v0) / (1.0 - v0)) * 0.5;
        }

        public static double atanh(BigInteger value) {
            if (value == 0) {
                return 0;
            } else {
                throw PythonOps.ValueError("math domain error");
            }
        }

        public static double atanh(object value) {
            // CPython tries float first, then double, so we need
            // an explicit overload which properly matches the order here
            double val;
            if (Converter.TryConvertToDouble(value, out val)) {
                return atanh(val);
            } else {
                return atanh(Converter.ConvertToBigInteger(value));
            }
        }

        public static double atan2(double v0, double v1) {
            if (double.IsNaN(v0) || double.IsNaN(v1)) {
                return double.NaN;
            } else if (double.IsInfinity(v0)) {
                if (double.IsPositiveInfinity(v1)) {
                    return pi * 0.25 * Math.Sign(v0);
                } else if (double.IsNegativeInfinity(v1)) {
                    return pi * 0.75 * Math.Sign(v0);
                } else {
                    return pi * 0.5 * Math.Sign(v0);
                }
            } else if (double.IsInfinity(v1)) {
                return v1 > 0.0 ? 0.0 : pi * DoubleOps.Sign(v0);
            }
            return Math.Atan2(v0, v1);
        }

        public static object factorial(double v0) {
            if ((BigInteger)v0 != v0) {
                throw PythonOps.ValueError("factorial() only accepts integral values");
            }
            if (v0 < 0.0) {
                throw PythonOps.ValueError("factorial() not defined for negative values");
            }

            BigInteger val = 1;
            for (BigInteger mul = (BigInteger)v0; mul > (BigInteger)1; mul -= (BigInteger)1) {
                val *= mul;
            }

            if (val > SysModule.maxint) {
                return val;
            }
            return (int)val;
        }

        public static object factorial(BigInteger value) {
            if (value < 0) {
                throw PythonOps.ValueError("factorial() not defined for negative values");
            }

            BigInteger val = 1;
            for (BigInteger mul = value; mul > (BigInteger)1; mul -= (BigInteger)1) {
                val *= mul;
            }

            if (val > SysModule.maxint) {
                return val;
            }
            return (int)val;
        }

        public static object factorial(object value) {
            // CPython tries float first, then double, so we need
            // an explicit overload which properly matches the order here
            double val;
            if (Converter.TryConvertToDouble(value, out val)) {
                return factorial(val);
            } else {
                return factorial(Converter.ConvertToBigInteger(value));
            }
        }

        public static object trunc(CodeContext/*!*/ context, object value) {
            object func;
            if (PythonOps.TryGetBoundAttr(value, Symbols.Truncate, out func)) {
                return PythonOps.CallWithContext(context, func);
            } else {
                throw PythonOps.AttributeError("__trunc__");
            }
        }

        public static bool isinf(double v0) {
            return double.IsInfinity(v0);
        }

        public static bool isinf(BigInteger value) {
            return false;
        }

        public static bool isinf(object value) {
            // CPython tries float first, then double, so we need
            // an explicit overload which properly matches the order here
            double val;
            if (Converter.TryConvertToDouble(value, out val)) {
                return isinf(val);
            }
            return false;
        }

        public static bool isnan(double v0) {
            return double.IsNaN(v0);
        }

        public static bool isnan(BigInteger value) {
            return false;
        }

        public static bool isnan(object value) {
            // CPython tries float first, then double, so we need
            // an explicit overload which properly matches the order here
            double val;
            if (Converter.TryConvertToDouble(value, out val)) {
                return isnan(val);
            }
            return false;
        }

        public static double copysign(object x, object y) {
            double val, sign;
            if (!Converter.TryConvertToDouble(x, out val) ||
                !Converter.TryConvertToDouble(y, out sign)) {
                throw PythonOps.TypeError("TypeError: a float is required");
            }
            return DoubleOps.Sign(sign) * Math.Abs(val);
        }

        private static void SetExponentLe(byte[] v, int exp) {
            exp += Bias;
            ushort oldExp = LdExponentLe(v);
            ushort newExp = (ushort)(oldExp & 0x800f | (exp << 4));
            StExponentLe(v, newExp);
        }

        private static int IntExponentLe(byte[] v) {
            ushort exp = LdExponentLe(v);
            return ((int)((exp & 0x7FF0) >> 4) - Bias);
        }

        private static ushort LdExponentLe(byte[] v) {
            return (ushort)(v[6] | ((ushort)v[7] << 8));
        }

        private static long LdMantissaLe(byte[] v) {
            int i1 = (v[0] | (v[1] << 8) | (v[2] << 16) | (v[3] << 24));
            int i2 = (v[4] | (v[5] << 8) | ((v[6] & 0xF) << 16));

            return i1 | (i2 << 32);
        }

        private static void StExponentLe(byte[] v, ushort e) {
            v[6] = (byte)e;
            v[7] = (byte)(e >> 8);
        }

        private static bool IsDenormalizedLe(byte[] v) {
            ushort exp = LdExponentLe(v);
            long man = LdMantissaLe(v);

            return ((exp & 0x7FF0) == 0 && (man != 0));
        }

        private static void DecomposeLe(byte[] v, out double m, out int e) {
            if (IsDenormalizedLe(v)) {
                throw new NotImplementedException();
            } else {
                e = IntExponentLe(v);
                SetExponentLe(v, 0);
                m = BitConverter.ToDouble(v, 0);
            }
        }

        private static double Check(double v) {
            return PythonOps.CheckMath(v);
        }

        private static double Check(double input, double output) {
            if (double.IsInfinity(input) && double.IsInfinity(output) ||
                double.IsNaN(input) && double.IsNaN(input)) {
                return output;
            } else {
                return PythonOps.CheckMath(output);
            }
        }

        private static double Check(double in0, double in1, double output) {
            if ((double.IsInfinity(in0) || double.IsInfinity(in1)) && double.IsInfinity(output) ||
                (double.IsNaN(in0) || double.IsNaN(in1)) && double.IsNaN(output)) {
                return output;
            } else {
                return PythonOps.CheckMath(output);
            }
        }
    }
}
