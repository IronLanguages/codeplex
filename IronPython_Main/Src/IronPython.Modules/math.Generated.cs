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

namespace IronPython.Modules {
    public static partial class PythonMath {
        #region Generated math functions

        // *** BEGIN GENERATED CODE ***
        // generated by function: gen_funcs from: generate_math.py

        public static double cos(double v0) {
            return Check(v0, Math.Cos(v0));
        }
        public static double sin(double v0) {
            return Check(v0, Math.Sin(v0));
        }
        public static double tan(double v0) {
            return Check(v0, Math.Tan(v0));
        }
        public static double cosh(double v0) {
            return Check(v0, Math.Cosh(v0));
        }
        public static double sinh(double v0) {
            return Check(v0, Math.Sinh(v0));
        }
        public static double tanh(double v0) {
            return Check(v0, Math.Tanh(v0));
        }
        public static double acos(double v0) {
            return Check(v0, Math.Acos(v0));
        }
        public static double asin(double v0) {
            return Check(v0, Math.Asin(v0));
        }
        public static double atan(double v0) {
            return Check(v0, Math.Atan(v0));
        }
        public static double floor(double v0) {
            return Check(v0, Math.Floor(v0));
        }
        public static double ceil(double v0) {
            return Check(v0, Math.Ceiling(v0));
        }
        public static double fabs(double v0) {
            return Check(v0, Math.Abs(v0));
        }
        public static double sqrt(double v0) {
            return Check(v0, Math.Sqrt(v0));
        }
        public static double exp(double v0) {
            return Check(v0, Math.Exp(v0));
        }

        // *** END GENERATED CODE ***

        #endregion
    }
}
