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
using IronPython.Runtime;
using IronMath;

using IronPython.Runtime.Operations;

[assembly: PythonModule("math", typeof(IronPython.Modules.PythonMath))]
namespace IronPython.Modules {
    [PythonType("math")]
    public static partial class PythonMath {
        public static double pi = Math.PI;
        public static double e = Math.E;

        private static double Check(double v) {
            if (double.IsInfinity(v)) {
                throw Ops.OverflowError("math range error");
            } else if (double.IsNaN(v)) {
                throw Ops.ValueError("math domain error");
            } else {
                return v;
            }
        }

        private const double degreesToRadians = Math.PI / 180.0;
        [PythonName("degrees")]
        public static double Degrees(double radians) {
            return Check(radians / degreesToRadians);
        }

        [PythonName("radians")]
        public static double Radians(double degrees) {
            return Check(degrees * degreesToRadians);
        }

        [PythonName("fmod")]
        public static double FloatMod(double v, double w) {
            return v % w;
        }

        private const int Bias = 0x3FE;

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

        [PythonName("frexp")]
        public static Tuple GetMantissaAndExp(double v) {
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

            return Tuple.MakeTuple(mantissa, exponent);
        }

        [PythonName("modf")]
        public static Tuple ModF(double v) {
            double w = v % 1.0;
            v -= w;
            return Tuple.MakeTuple(w, v);
        }

        [PythonName("ldexp")]
        public static double FromMantissaAndExponent(double v, int w) {
            return Check(v * Math.Pow(2.0, w));
        }

        [PythonName("hypot")]
        public static double Hypot(double v, double w) {
            return Check(IronMath.Complex64.Hypot(v, w));
        }


    }
}
