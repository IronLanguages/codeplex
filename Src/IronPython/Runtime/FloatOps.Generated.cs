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
using System.Collections.Generic;
using System.Text;

using IronMath;

namespace IronPython.Runtime {
    public static partial class FloatOps {
        #region Generated FloatOps

        // *** BEGIN GENERATED CODE ***


        [PythonName("__add__")]
        public static object Add(double x, object other) {
            BigInteger bi;
            ExtensibleInt xi;
            ExtensibleFloat xf;
            ExtensibleComplex xc;

            if (other is double) return x + ((double)other);
            if (other is int) return x + ((int)other);
            if (other is Complex64) return ComplexOps.Add(Complex64.MakeReal(x), (Complex64)other);
            if ((object)(bi = other as BigInteger) != null) return x + bi;
            if (other is float) return x + ((float)other);
            if ((object)(xf = other as ExtensibleFloat) != null) return x + xf.value;
            if (other is string) return Ops.NotImplemented;
            if (other is IConvertible) {
                double y = ((IConvertible)other).ToDouble(null);
                return x + y;
            }
            if (other is long) return x + ((long)other);
            if ((object)(xi = other as ExtensibleInt) != null) return x + xi.value;
            if ((object)(xc = other as ExtensibleComplex) != null) return ComplexOps.Add(Complex64.MakeReal(x), xc.value);
            return Ops.NotImplemented;
        }


        [PythonName("__radd__")]
        public static object ReverseAdd(double x, object other) {
            BigInteger bi;
            ExtensibleInt xi;
            ExtensibleFloat xf;
            ExtensibleComplex xc;

            if (other is double) return ((double)other) + x;
            if (other is int) return ((int)other) + x;
            if (other is Complex64) return ComplexOps.Add((Complex64)other, Complex64.MakeReal(x));
            if ((object)(bi = other as BigInteger) != null) return bi + x;
            if (other is float) return ((float)other) + x;
            if ((object)(xf = other as ExtensibleFloat) != null) return xf.value + x;
            if (other is string) return Ops.NotImplemented;
            if (other is long) return ((long)other) + x;
            if ((object)(xi = other as ExtensibleInt) != null) return xi.value + x;
            if ((object)(xc = other as ExtensibleComplex) != null) return ComplexOps.Add(xc.value, Complex64.MakeReal(x));
            if (other is IConvertible) {
                double y = ((IConvertible)other).ToDouble(null);
                return x + y;
            }
            return Ops.NotImplemented;
        }


        [PythonName("__sub__")]
        public static object Subtract(double x, object other) {
            BigInteger bi;
            ExtensibleInt xi;
            ExtensibleFloat xf;
            ExtensibleComplex xc;

            if (other is double) return x - ((double)other);
            if (other is int) return x - ((int)other);
            if (other is Complex64) return ComplexOps.Subtract(Complex64.MakeReal(x), (Complex64)other);
            if ((object)(bi = other as BigInteger) != null) return x - bi;
            if (other is float) return x - ((float)other);
            if ((object)(xf = other as ExtensibleFloat) != null) return x - xf.value;
            if (other is string) return Ops.NotImplemented;
            if (other is IConvertible) {
                double y = ((IConvertible)other).ToDouble(null);
                return x - y;
            }
            if (other is long) return x - ((long)other);
            if ((object)(xi = other as ExtensibleInt) != null) return x - xi.value;
            if ((object)(xc = other as ExtensibleComplex) != null) return ComplexOps.Subtract(Complex64.MakeReal(x), xc.value);
            return Ops.NotImplemented;
        }


        [PythonName("__rsub__")]
        public static object ReverseSubtract(double x, object other) {
            BigInteger bi;
            ExtensibleInt xi;
            ExtensibleFloat xf;
            ExtensibleComplex xc;

            if (other is double) return ((double)other) - x;
            if (other is int) return ((int)other) - x;
            if (other is Complex64) return ComplexOps.Subtract((Complex64)other, Complex64.MakeReal(x));
            if ((object)(bi = other as BigInteger) != null) return bi - x;
            if (other is float) return ((float)other) - x;
            if ((object)(xf = other as ExtensibleFloat) != null) return xf.value - x;
            if (other is string) return Ops.NotImplemented;
            if (other is long) return ((long)other) - x;
            if ((object)(xi = other as ExtensibleInt) != null) return xi.value - x;
            if ((object)(xc = other as ExtensibleComplex) != null) return ComplexOps.Subtract(xc.value, Complex64.MakeReal(x));
            if (other is IConvertible) {
                double y = ((IConvertible)other).ToDouble(null);
                return x - y;
            }
            return Ops.NotImplemented;
        }


        [PythonName("__mul__")]
        public static object Multiply(double x, object other) {
            BigInteger bi;
            ExtensibleInt xi;
            ExtensibleFloat xf;
            ExtensibleComplex xc;

            if (other is double) return x * ((double)other);
            if (other is int) return x * ((int)other);
            if (other is Complex64) return ComplexOps.Multiply(Complex64.MakeReal(x), (Complex64)other);
            if ((object)(bi = other as BigInteger) != null) return x * bi;
            if (other is float) return x * ((float)other);
            if ((object)(xf = other as ExtensibleFloat) != null) return x * xf.value;
            if (other is string) return Ops.NotImplemented;
            if (other is IConvertible) {
                double y = ((IConvertible)other).ToDouble(null);
                return x * y;
            }
            if (other is long) return x * ((long)other);
            if ((object)(xi = other as ExtensibleInt) != null) return x * xi.value;
            if ((object)(xc = other as ExtensibleComplex) != null) return ComplexOps.Multiply(Complex64.MakeReal(x), xc.value);
            return Ops.NotImplemented;
        }


        [PythonName("__rmul__")]
        public static object ReverseMultiply(double x, object other) {
            BigInteger bi;
            ExtensibleInt xi;
            ExtensibleFloat xf;
            ExtensibleComplex xc;

            if (other is double) return ((double)other) * x;
            if (other is int) return ((int)other) * x;
            if (other is Complex64) return ComplexOps.Multiply((Complex64)other, Complex64.MakeReal(x));
            if ((object)(bi = other as BigInteger) != null) return bi * x;
            if (other is float) return ((float)other) * x;
            if ((object)(xf = other as ExtensibleFloat) != null) return xf.value * x;
            if (other is string) return Ops.NotImplemented;
            if (other is long) return ((long)other) * x;
            if ((object)(xi = other as ExtensibleInt) != null) return xi.value * x;
            if ((object)(xc = other as ExtensibleComplex) != null) return ComplexOps.Multiply(xc.value, Complex64.MakeReal(x));
            if (other is IConvertible) {
                double y = ((IConvertible)other).ToDouble(null);
                return x * y;
            }
            return Ops.NotImplemented;
        }


        [PythonName("__truediv__")]
        public static object TrueDivide(double x, object other) {
            BigInteger bi;
            ExtensibleInt xi;
            ExtensibleFloat xf;
            ExtensibleComplex xc;

            if (other is double) return TrueDivide(x, ((double)other));
            if (other is int) return TrueDivide(x, ((int)other));
            if (other is Complex64) return ComplexOps.TrueDivide(Complex64.MakeReal(x), (Complex64)other);
            if ((object)(bi = other as BigInteger) != null) return TrueDivide(x, bi);
            if (other is bool) return TrueDivide(x, (bool)other ? 1.0 : 0.0);
            if (other is float) return TrueDivide(x, ((float)other));
            if ((object)(xf = other as ExtensibleFloat) != null) return TrueDivide(x, xf.value);
            if (other is long) return TrueDivide(x, ((long)other));
            if ((object)(xc = other as ExtensibleComplex) != null) return ComplexOps.TrueDivide(Complex64.MakeReal(x), xc.value);
            if ((object)(xi = other as ExtensibleInt) != null) return TrueDivide(x, xi.value);
            if (other is byte) return TrueDivide(x, (int)((byte)other));
           return Ops.NotImplemented;
        }

        [PythonName("__rtruediv__")]
        public static object ReverseTrueDivide(double x, object other) {
            BigInteger bi;
            ExtensibleInt xi;
            ExtensibleFloat xf;
            ExtensibleComplex xc;

            if (other is double) return ReverseTrueDivide(x, ((double)other));
            if (other is int) return ReverseTrueDivide(x, ((int)other));
            if (other is Complex64) return ComplexOps.ReverseTrueDivide(Complex64.MakeReal(x), (Complex64)other);
            if ((object)(bi = other as BigInteger) != null) return ReverseTrueDivide(x, bi);
            if (other is bool) return ReverseTrueDivide(x, (bool)other ? 1.0 : 0.0);
            if (other is float) return ReverseTrueDivide(x, ((float)other));
            if ((object)(xf = other as ExtensibleFloat) != null) return ReverseTrueDivide(x, xf.value);
            if (other is long) return ReverseTrueDivide(x, ((long)other));
            if ((object)(xc = other as ExtensibleComplex) != null) return ComplexOps.ReverseTrueDivide(Complex64.MakeReal(x), xc.value);
            if ((object)(xi = other as ExtensibleInt) != null) return ReverseTrueDivide(x, xi.value);
            if (other is byte) return ReverseTrueDivide(x, (int)((byte)other));
           return Ops.NotImplemented;
        }


        // *** END GENERATED CODE ***

        #endregion

    }
}
