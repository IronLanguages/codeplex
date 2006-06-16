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
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Diagnostics;

using IronPython.Runtime;
using IronPython.Compiler;
using IronPython.Runtime.Operations;

using IronMath;

namespace IronPython.Modules {
    public static partial class PythonMath {
        #region Generated math functions

        // *** BEGIN GENERATED CODE ***

        [PythonName("acos")]
        public static double Acos(double v0) {
            return Check(Math.Acos(v0));
        }

        [PythonName("acos")]
        public static double Acos(BigInteger v0) {
            double v0d;
            if (v0.TryToFloat64(out v0d)) {
                return Acos(v0d);
            }

            throw Ops.OverflowError("long too large to convert to float");
        }

        [PythonName("asin")]
        public static double Asin(double v0) {
            return Check(Math.Asin(v0));
        }

        [PythonName("asin")]
        public static double Asin(BigInteger v0) {
            double v0d;
            if (v0.TryToFloat64(out v0d)) {
                return Asin(v0d);
            }

            throw Ops.OverflowError("long too large to convert to float");
        }

        [PythonName("atan")]
        public static double Atan(double v0) {
            return Check(Math.Atan(v0));
        }

        [PythonName("atan")]
        public static double Atan(BigInteger v0) {
            double v0d;
            if (v0.TryToFloat64(out v0d)) {
                return Atan(v0d);
            }

            throw Ops.OverflowError("long too large to convert to float");
        }

        [PythonName("atan2")]
        public static double Atan2(double v0, double v1) {
            return Check(Math.Atan2(v0, v1));
        }

        [PythonName("atan2")]
        public static double Atan2(BigInteger v0, BigInteger v1) {
            double v0d, v1d;
            if (v0.TryToFloat64(out v0d) && v1.TryToFloat64(out v1d)) {
                return Atan2(v0d, v1d);
            }

            throw Ops.OverflowError("long too large to convert to float");
        }

        [PythonName("ceil")]
        public static double Ceil(double v0) {
            return Check(Math.Ceiling(v0));
        }

        [PythonName("ceil")]
        public static double Ceil(BigInteger v0) {
            double v0d;
            if (v0.TryToFloat64(out v0d)) {
                return Ceil(v0d);
            }

            throw Ops.OverflowError("long too large to convert to float");
        }

        [PythonName("cos")]
        public static double Cos(double v0) {
            return Check(Math.Cos(v0));
        }

        [PythonName("cos")]
        public static double Cos(BigInteger v0) {
            double v0d;
            if (v0.TryToFloat64(out v0d)) {
                return Cos(v0d);
            }

            throw Ops.OverflowError("long too large to convert to float");
        }

        [PythonName("cosh")]
        public static double Cosh(double v0) {
            return Check(Math.Cosh(v0));
        }

        [PythonName("cosh")]
        public static double Cosh(BigInteger v0) {
            double v0d;
            if (v0.TryToFloat64(out v0d)) {
                return Cosh(v0d);
            }

            throw Ops.OverflowError("long too large to convert to float");
        }

        [PythonName("exp")]
        public static double Exp(double v0) {
            return Check(Math.Exp(v0));
        }

        [PythonName("exp")]
        public static double Exp(BigInteger v0) {
            double v0d;
            if (v0.TryToFloat64(out v0d)) {
                return Exp(v0d);
            }

            throw Ops.OverflowError("long too large to convert to float");
        }

        [PythonName("fabs")]
        public static double Fabs(double v0) {
            return Check(Math.Abs(v0));
        }

        [PythonName("fabs")]
        public static double Fabs(BigInteger v0) {
            double v0d;
            if (v0.TryToFloat64(out v0d)) {
                return Fabs(v0d);
            }

            throw Ops.OverflowError("long too large to convert to float");
        }

        [PythonName("floor")]
        public static double Floor(double v0) {
            return Check(Math.Floor(v0));
        }

        [PythonName("floor")]
        public static double Floor(BigInteger v0) {
            double v0d;
            if (v0.TryToFloat64(out v0d)) {
                return Floor(v0d);
            }

            throw Ops.OverflowError("long too large to convert to float");
        }

        [PythonName("log")]
        public static double Log(double v0) {
            return Check(Math.Log(v0));
        }

        [PythonName("log")]
        public static double Log(BigInteger v0) {
            double v0d;
            if (v0.TryToFloat64(out v0d)) {
                return Log(v0d);
            }

            throw Ops.OverflowError("long too large to convert to float");
        }

        [PythonName("log")]
        public static double Log(double v0, double v1) {
            return Check(Math.Log(v0, v1));
        }

        [PythonName("log")]
        public static double Log(BigInteger v0, BigInteger v1) {
            double v0d, v1d;
            if (v0.TryToFloat64(out v0d) && v1.TryToFloat64(out v1d)) {
                return Log(v0d, v1d);
            }

            throw Ops.OverflowError("long too large to convert to float");
        }

        [PythonName("log10")]
        public static double Log10(double v0) {
            return Check(Math.Log10(v0));
        }

        [PythonName("log10")]
        public static double Log10(BigInteger v0) {
            double v0d;
            if (v0.TryToFloat64(out v0d)) {
                return Log10(v0d);
            }

            throw Ops.OverflowError("long too large to convert to float");
        }

        [PythonName("pow")]
        public static double Pow(double v0, double v1) {
            return Check(Math.Pow(v0, v1));
        }

        [PythonName("pow")]
        public static double Pow(BigInteger v0, BigInteger v1) {
            double v0d, v1d;
            if (v0.TryToFloat64(out v0d) && v1.TryToFloat64(out v1d)) {
                return Pow(v0d, v1d);
            }

            throw Ops.OverflowError("long too large to convert to float");
        }

        [PythonName("sin")]
        public static double Sin(double v0) {
            return Check(Math.Sin(v0));
        }

        [PythonName("sin")]
        public static double Sin(BigInteger v0) {
            double v0d;
            if (v0.TryToFloat64(out v0d)) {
                return Sin(v0d);
            }

            throw Ops.OverflowError("long too large to convert to float");
        }

        [PythonName("sinh")]
        public static double Sinh(double v0) {
            return Check(Math.Sinh(v0));
        }

        [PythonName("sinh")]
        public static double Sinh(BigInteger v0) {
            double v0d;
            if (v0.TryToFloat64(out v0d)) {
                return Sinh(v0d);
            }

            throw Ops.OverflowError("long too large to convert to float");
        }

        [PythonName("sqrt")]
        public static double Sqrt(double v0) {
            return Check(Math.Sqrt(v0));
        }

        [PythonName("sqrt")]
        public static double Sqrt(BigInteger v0) {
            double v0d;
            if (v0.TryToFloat64(out v0d)) {
                return Sqrt(v0d);
            }

            throw Ops.OverflowError("long too large to convert to float");
        }

        [PythonName("tan")]
        public static double Tan(double v0) {
            return Check(Math.Tan(v0));
        }

        [PythonName("tan")]
        public static double Tan(BigInteger v0) {
            double v0d;
            if (v0.TryToFloat64(out v0d)) {
                return Tan(v0d);
            }

            throw Ops.OverflowError("long too large to convert to float");
        }

        [PythonName("tanh")]
        public static double Tanh(double v0) {
            return Check(Math.Tanh(v0));
        }

        [PythonName("tanh")]
        public static double Tanh(BigInteger v0) {
            double v0d;
            if (v0.TryToFloat64(out v0d)) {
                return Tanh(v0d);
            }

            throw Ops.OverflowError("long too large to convert to float");
        }


        // *** END GENERATED CODE ***

        #endregion
    }
}
