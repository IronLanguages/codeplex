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

using System;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Math;

[assembly: PythonModule("math", typeof(IronPython.Modules.PythonMath))]
namespace IronPython.Modules {
    public static partial class PythonMath {
        public const double pi = Math.PI;
        public const double e = Math.E;

        private const double degreesToRadians = Math.PI / 180.0;
        private const int Bias = 0x3FE;

        public static double degrees(double radians) {
            return Check(radians / degreesToRadians);
        }

        public static double radians(double degrees) {
            return Check(degrees * degreesToRadians);
        }

        public static double fmod(double v, double w) {
            return v % w;
        }

        public static PythonTuple frexp(double v) {
            if (Double.IsInfinity(v) || Double.IsNaN(v)) {
                throw new OverflowException();
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
            double w = v % 1.0;
            v -= w;
            return PythonTuple.MakeTuple(w, v);
        }

        public static double ldexp(double v, int w) {
            return Check(v * Math.Pow(2.0, w));
        }

        public static double hypot(double v, double w) {
            return Check(Complex64.Hypot(v, w));
        }

        public static double log(double v0, double v1) {
#if !SILVERLIGHT        // Math.Log overload
            return Check(Math.Log(v0, v1));
#else
            if ((v1 != 1) && ((v0 == 1) || ((v1 != 0) && !double.IsPositiveInfinity(v1)))) {
                return Check((Math.Log(v0) / Math.Log(v1)));
            }
            return Check(double.NaN);

#endif
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
    }
}
