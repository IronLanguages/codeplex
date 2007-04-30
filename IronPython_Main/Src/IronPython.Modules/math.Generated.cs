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
using IronPython.Runtime;

namespace IronPython.Modules {
    public static partial class PythonMath {
        #region Generated math functions

        // *** BEGIN GENERATED CODE ***

        [PythonName("acos")]
        public static double Acos(double v0) {
            return Check(Math.Acos(v0));
        }
        [PythonName("asin")]
        public static double Asin(double v0) {
            return Check(Math.Asin(v0));
        }
        [PythonName("atan")]
        public static double Atan(double v0) {
            return Check(Math.Atan(v0));
        }
        [PythonName("atan2")]
        public static double Atan2(double v0, double v1) {
            return Check(Math.Atan2(v0, v1));
        }
        [PythonName("ceil")]
        public static double Ceil(double v0) {
            return Check(Math.Ceiling(v0));
        }
        [PythonName("cos")]
        public static double Cos(double v0) {
            return Check(Math.Cos(v0));
        }
        [PythonName("cosh")]
        public static double Cosh(double v0) {
            return Check(Math.Cosh(v0));
        }
        [PythonName("exp")]
        public static double Exp(double v0) {
            return Check(Math.Exp(v0));
        }
        [PythonName("fabs")]
        public static double Fabs(double v0) {
            return Check(Math.Abs(v0));
        }
        [PythonName("floor")]
        public static double Floor(double v0) {
            return Check(Math.Floor(v0));
        }
        [PythonName("log")]
        public static double Log(double v0) {
            return Check(Math.Log(v0));
        }
        [PythonName("log10")]
        public static double Log10(double v0) {
            return Check(Math.Log10(v0));
        }
        [PythonName("pow")]
        public static double Pow(double v0, double v1) {
            return Check(Math.Pow(v0, v1));
        }
        [PythonName("sin")]
        public static double Sin(double v0) {
            return Check(Math.Sin(v0));
        }
        [PythonName("sinh")]
        public static double Sinh(double v0) {
            return Check(Math.Sinh(v0));
        }
        [PythonName("sqrt")]
        public static double Sqrt(double v0) {
            return Check(Math.Sqrt(v0));
        }
        [PythonName("tan")]
        public static double Tan(double v0) {
            return Check(Math.Tan(v0));
        }
        [PythonName("tanh")]
        public static double Tanh(double v0) {
            return Check(Math.Tanh(v0));
        }

        // *** END GENERATED CODE ***

        #endregion
    }
}
