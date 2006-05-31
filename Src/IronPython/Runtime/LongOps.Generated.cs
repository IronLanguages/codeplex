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
    public static partial class LongOps {
        #region Generated LongOps

        // *** BEGIN GENERATED CODE ***


        [PythonName("__add__")]
        public static object Add(BigInteger x, object other) {
            BigInteger bi;
            ExtensibleInt xi;
            ExtensibleFloat xf;
            ExtensibleComplex xc;
            ExtensibleLong el;

            if (other is int) return x + ((int)other);
            if (other is Complex64) return x + ((Complex64)other);
            if (other is double) return x + ((double)other);
            if ((object)(bi = other as BigInteger) != null) return x + bi;
            if ((el = other as ExtensibleLong) != null) return x + el.Value;
            if (other is bool) return x + ((bool) other ? 1 : 0);
            if (other is long) return x + ((long)other);
            if ((object)(xi = other as ExtensibleInt) != null) return x + (xi.value);
            if ((object)(xf = other as ExtensibleFloat) != null) return x + (xf.value);
            if ((object)(xc = other as ExtensibleComplex) != null) return x + xc.value;
            if (other is byte) return x + (int)((byte)other);
            return Ops.NotImplemented;
        }


        [PythonName("__sub__")]
        public static object Subtract(BigInteger x, object other) {
            BigInteger bi;
            ExtensibleInt xi;
            ExtensibleFloat xf;
            ExtensibleComplex xc;
            ExtensibleLong el;

            if (other is int) return x - ((int)other);
            if (other is Complex64) return x - ((Complex64)other);
            if (other is double) return x - ((double)other);
            if ((object)(bi = other as BigInteger) != null) return x - bi;
            if ((el = other as ExtensibleLong) != null) return x - el.Value;
            if (other is bool) return x - ((bool) other ? 1 : 0);
            if (other is long) return x - ((long)other);
            if ((object)(xi = other as ExtensibleInt) != null) return x - (xi.value);
            if ((object)(xf = other as ExtensibleFloat) != null) return x - (xf.value);
            if ((object)(xc = other as ExtensibleComplex) != null) return x - xc.value;
            if (other is byte) return x - (int)((byte)other);
            return Ops.NotImplemented;
        }


        [PythonName("__pow__")]
        public static object Power(BigInteger x, object other) {
            BigInteger bi;
            ExtensibleInt xi;
            ExtensibleFloat xf;
            ExtensibleComplex xc;
            ExtensibleLong xl;

            if (other is int) return Power(x, (int)other);
            if ((object)(bi = other as BigInteger) != null) return Power(x, bi);
            if ((xl = other as ExtensibleLong) != null) return Power(x, xl.Value);
            if (other is double) return Power(x, (double)other);
            if (other is Complex64) return ComplexOps.Power(x, (Complex64)other);
            if (other is bool) return Power(x, (bool)other ? 1 : 0);
            if (other is long) return Power(x, (long)other);
            if ((object)(xi = other as ExtensibleInt) != null) return Power(x, xi.value);
            if ((object)(xf = other as ExtensibleFloat) != null) return Power(x, xf.value);
            if ((object)(xc = other as ExtensibleComplex) != null) return Power(x, xc.value);
            if (other is byte) return Power(x, (int)((byte)other));
            return Ops.NotImplemented;
        }


        [PythonName("__mul__")]
        public static object Multiply(BigInteger x, object other) {
            BigInteger bi;
            ExtensibleInt xi;
            ExtensibleFloat xf;
            ExtensibleComplex xc;
            ExtensibleLong el;

            if (other is int) return x * ((int)other);
            if (other is Complex64) return x * ((Complex64)other);
            if (other is double) return x * ((double)other);
            if ((object)(bi = other as BigInteger) != null) return x * bi;
            if ((el = other as ExtensibleLong) != null) return x * el.Value;
            if (other is bool) return x * ((bool) other ? 1 : 0);
            if (other is long) return x * ((long)other);
            if ((object)(xi = other as ExtensibleInt) != null) return x * (xi.value);
            if ((object)(xf = other as ExtensibleFloat) != null) return x * (xf.value);
            if ((object)(xc = other as ExtensibleComplex) != null) return x * xc.value;
            if (other is byte) return x * (int)((byte)other);
            return Ops.NotImplemented;
        }


        [PythonName("__div__")]
        public static object Divide(BigInteger x, object other) {
            BigInteger bi;
            ExtensibleInt xi;
            ExtensibleFloat xf;
            ExtensibleComplex xc;
            ExtensibleLong el;

            if (other is int) return Divide(x, (int)other);
            if (other is Complex64) {
                Complex64 y = (Complex64)other;
                if(y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.Divide(Complex64.MakeReal(x), y);
            }
            if (other is double) return FloatOps.Divide(x, (double)other);
            if (other is bool) return Divide(x, (bool)other ? 1 : 0);
            if (other is long) return Divide(x, (long)other);
            if ((object)(bi = other as BigInteger) != null) return Divide(x, bi);
            if ((object)(el = other as ExtensibleLong) != null) return Divide(x, el.Value);
            if ((object)(xi = other as ExtensibleInt) != null) return Divide(x, xi.value);
            if ((object)(xc = other as ExtensibleComplex) != null) {
                Complex64 y = xc.value;
                if(y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.Divide(Complex64.MakeReal(x), y);
            }
            if (other is byte) return Divide(x, (int)((byte)other));
            if ((object)(xf = other as ExtensibleFloat) != null) return FloatOps.Divide(x, xf.value);
            return Ops.NotImplemented;
        }


        [PythonName("__rdiv__")]
        public static object ReverseDivide(BigInteger x, object other) {
            BigInteger bi;
            ExtensibleInt xi;
            ExtensibleFloat xf;
            ExtensibleComplex xc;
            ExtensibleLong el;

            if (other is int) return IntOps.Divide((int)other, x);
            if (other is Complex64) {
                Complex64 y = (Complex64)other;
                if(y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.Divide(y, Complex64.MakeReal(x));
            }
            if (other is double) return FloatOps.Divide((double)other, x);
            if (other is bool) return Divide((bool)other ? 1 : 0, x);
            if (other is long) return Divide((long)other, x);
            if ((object)(bi = other as BigInteger) != null) return Divide(bi, x);
            if ((object)(el = other as ExtensibleLong) != null) return Divide(el.Value, x);
            if ((object)(xi = other as ExtensibleInt) != null) return Divide(xi.value, x);
            if ((object)(xc = other as ExtensibleComplex) != null) {
                Complex64 y = xc.value;
                if(y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.Divide(y, Complex64.MakeReal(x));
            }
            if (other is byte) return IntOps.Divide((int)((byte)other), x);
            if ((object)(xf = other as ExtensibleFloat) != null) return FloatOps.Divide(xf.value, x);
            return Ops.NotImplemented;
        }



        [PythonName("__floordiv__")]
        public static object FloorDivide(BigInteger x, object other) {
            BigInteger bi;
            ExtensibleInt xi;
            ExtensibleFloat xf;
            ExtensibleComplex xc;
            ExtensibleLong el;

            if (other is int) return Divide(x, (int)other);
            if (other is Complex64) {
                Complex64 y = (Complex64)other;
                if(y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.FloorDivide(Complex64.MakeReal(x), y);
            }
            if (other is double) return FloatOps.FloorDivide(x, (double)other);
            if (other is bool) return Divide(x, (bool)other ? 1 : 0);
            if (other is long) return Divide(x, (long)other);
            if ((object)(bi = other as BigInteger) != null) return Divide(x, bi);
            if ((object)(el = other as ExtensibleLong) != null) return Divide(x, el.Value);
            if ((object)(xi = other as ExtensibleInt) != null) return Divide(x, xi.value);
            if ((object)(xc = other as ExtensibleComplex) != null) {
                Complex64 y = xc.value;
                if(y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.FloorDivide(Complex64.MakeReal(x), y);
            }
            if (other is byte) return Divide(x, (int)((byte)other));
            if ((object)(xf = other as ExtensibleFloat) != null) return FloatOps.FloorDivide(x, xf.value);
            return Ops.NotImplemented;
        }


        [PythonName("__rfloordiv__")]
        public static object ReverseFloorDivide(BigInteger x, object other) {
            BigInteger bi;
            ExtensibleInt xi;
            ExtensibleFloat xf;
            ExtensibleComplex xc;
            ExtensibleLong el;

            if (other is int) return IntOps.Divide((int)other, x);
            if (other is Complex64) {
                Complex64 y = (Complex64)other;
                if(y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.Divide(y, Complex64.MakeReal(x));
            }
            if (other is double) return FloatOps.FloorDivide((double)other, x);
            if (other is bool) return Divide((bool)other ? 1 : 0, x);
            if (other is long) return Divide((long)other, x);
            if ((object)(bi = other as BigInteger) != null) return Divide(bi, x);
            if ((object)(el = other as ExtensibleLong) != null) return Divide(el.Value, x);
            if ((object)(xi = other as ExtensibleInt) != null) return Divide(xi.value, x);
            if ((object)(xc = other as ExtensibleComplex) != null) {
                Complex64 y = xc.value;
                if(y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.FloorDivide(y, Complex64.MakeReal(x));
            }
            if (other is byte) return IntOps.Divide((int)((byte)other), x);
            if ((object)(xf = other as ExtensibleFloat) != null) return FloatOps.FloorDivide(xf.value, x);
            return Ops.NotImplemented;
        }



        [PythonName("__truediv__")]
        public static object TrueDivide(BigInteger x, object other) {
            BigInteger bi;
            ExtensibleInt xi;
            ExtensibleFloat xf;
            ExtensibleComplex xc;
            ExtensibleLong xl;

            if (other is int) return TrueDivide(x, (int)other);
            if ((object)(bi = other as BigInteger) != null) return TrueDivide(x, bi);
            if ((xl = other as ExtensibleLong) != null) return TrueDivide(x, xl.Value);
            if (other is double) return TrueDivide(x, (double)other);
            if (other is Complex64) return ComplexOps.TrueDivide(x, (Complex64)other);
            if (other is bool) return TrueDivide(x, (bool)other ? 1 : 0);
            if (other is long) return TrueDivide(x, (long)other);
            if ((object)(xi = other as ExtensibleInt) != null) return TrueDivide(x, xi.value);
            if ((object)(xf = other as ExtensibleFloat) != null) return TrueDivide(x, xf.value);
            if ((object)(xc = other as ExtensibleComplex) != null) return TrueDivide(x, xc.value);
            if (other is byte) return TrueDivide(x, (int)((byte)other));
            return Ops.NotImplemented;
        }


        [PythonName("__mod__")]
        public static object Mod(BigInteger x, object other) {
            BigInteger bi;
            ExtensibleInt xi;
            ExtensibleFloat xf;
            ExtensibleComplex xc;
            ExtensibleLong el;

            if (other is int) return Mod(x, (int)other);
            if (other is Complex64) {
                Complex64 y = (Complex64)other;
                if(y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.Mod(Complex64.MakeReal(x), y);
            }
            if (other is double) return FloatOps.Mod(x, (double)other);
            if (other is bool) return Mod(x, (bool)other ? 1 : 0);
            if (other is long) return Mod(x, (long)other);
            if ((object)(bi = other as BigInteger) != null) return Mod(x, bi);
            if ((object)(el = other as ExtensibleLong) != null) return Mod(x, el.Value);
            if ((object)(xi = other as ExtensibleInt) != null) return Mod(x, xi.value);
            if ((object)(xc = other as ExtensibleComplex) != null) {
                Complex64 y = xc.value;
                if(y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.Mod(Complex64.MakeReal(x), y);
            }
            if (other is byte) return Mod(x, (int)((byte)other));
            if ((object)(xf = other as ExtensibleFloat) != null) return FloatOps.Mod(x, xf.value);
            return Ops.NotImplemented;
        }


        [PythonName("__rmod__")]
        public static object ReverseMod(BigInteger x, object other) {
            BigInteger bi;
            ExtensibleInt xi;
            ExtensibleFloat xf;
            ExtensibleComplex xc;
            ExtensibleLong el;

            if (other is int) return IntOps.Mod((int)other, x);
            if (other is Complex64) {
                Complex64 y = (Complex64)other;
                if(y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.Mod(y, Complex64.MakeReal(x));
            }
            if (other is double) return FloatOps.Mod((double)other, x);
            if (other is bool) return Mod((bool)other ? 1 : 0, x);
            if (other is long) return Mod((long)other, x);
            if ((object)(bi = other as BigInteger) != null) return Mod(bi, x);
            if ((object)(el = other as ExtensibleLong) != null) return Mod(el.Value, x);
            if ((object)(xi = other as ExtensibleInt) != null) return Mod(xi.value, x);
            if ((object)(xc = other as ExtensibleComplex) != null) {
                Complex64 y = xc.value;
                if(y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.Mod(y, Complex64.MakeReal(x));
            }
            if (other is byte) return IntOps.Mod((int)((byte)other), x);
            if ((object)(xf = other as ExtensibleFloat) != null) return FloatOps.Mod(xf.value, x);
            return Ops.NotImplemented;
        }



        [PythonName("__and__")]
        public static object BitwiseAnd(BigInteger x, object other) {
            BigInteger bi;
            ExtensibleInt xi;
            ExtensibleLong xl;

            if ((object)(bi = other as BigInteger) != null) return x & bi;
            if (other is long) return x & (long)other;
            if (other is int) return x & (int)other;
            if (other is bool) return x & ((bool)other ? 1 : 0);
            if ((object)(xi = other as ExtensibleInt) != null) return x & xi.value;
            if ((object)(xl = other as ExtensibleLong) != null) return x & xl.Value;
            if (other is byte) return x & (int)((byte)other);
            return Ops.NotImplemented;
        }


        [PythonName("__or__")]
        public static object BitwiseOr(BigInteger x, object other) {
            BigInteger bi;
            ExtensibleInt xi;
            ExtensibleLong xl;

            if ((object)(bi = other as BigInteger) != null) return x | bi;
            if (other is long) return x | (long)other;
            if (other is int) return x | (int)other;
            if (other is bool) return x | ((bool)other ? 1 : 0);
            if ((object)(xi = other as ExtensibleInt) != null) return x | xi.value;
            if ((object)(xl = other as ExtensibleLong) != null) return x | xl.Value;
            if (other is byte) return x | (int)((byte)other);
            return Ops.NotImplemented;
        }


        [PythonName("__xor__")]
        public static object Xor(BigInteger x, object other) {
            BigInteger bi;
            ExtensibleInt xi;
            ExtensibleLong xl;

            if ((object)(bi = other as BigInteger) != null) return x ^ bi;
            if (other is long) return x ^ (long)other;
            if (other is int) return x ^ (int)other;
            if (other is bool) return x ^ ((bool)other ? 1 : 0);
            if ((object)(xi = other as ExtensibleInt) != null) return x ^ xi.value;
            if ((object)(xl = other as ExtensibleLong) != null) return x ^ xl.Value;
            if (other is byte) return x ^ (int)((byte)other);
            return Ops.NotImplemented;
        }


        // *** END GENERATED CODE ***

        #endregion

    }
}
