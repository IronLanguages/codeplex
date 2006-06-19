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
using IronPython.Runtime;

namespace IronPython.Runtime.Operations {
    public partial class ExtensibleLong {
        #region Generated Extensible LongOps

        // *** BEGIN GENERATED CODE ***

        [PythonName("__add__")]
        public virtual object Add(object other) {
            return LongOps.Add(value, other);
        }

        [PythonName("__radd__")]
        public virtual object ReverseAdd(object other) {
            return LongOps.ReverseAdd(value, other);
        }
        [PythonName("__sub__")]
        public virtual object Subtract(object other) {
            return LongOps.Subtract(value, other);
        }

        [PythonName("__rsub__")]
        public virtual object ReverseSubtract(object other) {
            return LongOps.ReverseSubtract(value, other);
        }
        [PythonName("__pow__")]
        public virtual object Power(object other) {
            return LongOps.Power(value, other);
        }

        [PythonName("__rpow__")]
        public virtual object ReversePower(object other) {
            return LongOps.ReversePower(value, other);
        }
        [PythonName("__mul__")]
        public virtual object Multiply(object other) {
            return LongOps.Multiply(value, other);
        }

        [PythonName("__rmul__")]
        public virtual object ReverseMultiply(object other) {
            return LongOps.ReverseMultiply(value, other);
        }
        [PythonName("__div__")]
        public virtual object Divide(object other) {
            return LongOps.Divide(value, other);
        }

        [PythonName("__rdiv__")]
        public virtual object ReverseDivide(object other) {
            return LongOps.ReverseDivide(value, other);
        }
        [PythonName("__floordiv__")]
        public virtual object FloorDivide(object other) {
            return LongOps.FloorDivide(value, other);
        }

        [PythonName("__rfloordiv__")]
        public virtual object ReverseFloorDivide(object other) {
            return LongOps.ReverseFloorDivide(value, other);
        }
        [PythonName("__truediv__")]
        public virtual object TrueDivide(object other) {
            return LongOps.TrueDivide(value, other);
        }

        [PythonName("__rtruediv__")]
        public virtual object ReverseTrueDivide(object other) {
            return LongOps.ReverseTrueDivide(value, other);
        }
        [PythonName("__mod__")]
        public virtual object Mod(object other) {
            return LongOps.Mod(value, other);
        }

        [PythonName("__rmod__")]
        public virtual object ReverseMod(object other) {
            return LongOps.ReverseMod(value, other);
        }
        [PythonName("__lshift__")]
        public virtual object LeftShift(object other) {
            return LongOps.LeftShift(value, other);
        }

        [PythonName("__rlshift__")]
        public virtual object ReverseLeftShift(object other) {
            return LongOps.ReverseLeftShift(value, other);
        }
        [PythonName("__rshift__")]
        public virtual object RightShift(object other) {
            return LongOps.RightShift(value, other);
        }

        [PythonName("__rrshift__")]
        public virtual object ReverseRightShift(object other) {
            return LongOps.ReverseRightShift(value, other);
        }
        [PythonName("__and__")]
        public virtual object BitwiseAnd(object other) {
            return LongOps.BitwiseAnd(value, other);
        }

        [PythonName("__rand__")]
        public virtual object ReverseBitwiseAnd(object other) {
            return LongOps.ReverseBitwiseAnd(value, other);
        }
        [PythonName("__or__")]
        public virtual object BitwiseOr(object other) {
            return LongOps.BitwiseOr(value, other);
        }

        [PythonName("__ror__")]
        public virtual object ReverseBitwiseOr(object other) {
            return LongOps.ReverseBitwiseOr(value, other);
        }
        [PythonName("__xor__")]
        public virtual object Xor(object other) {
            return LongOps.Xor(value, other);
        }

        [PythonName("__rxor__")]
        public virtual object ReverseXor(object other) {
            return LongOps.ReverseXor(value, other);
        }

        // *** END GENERATED CODE ***

        #endregion
    }

    public static partial class LongOps {
        #region Generated LongOps

        // *** BEGIN GENERATED CODE ***


        [PythonName("__add__")]
        public static object Add(BigInteger x, object other) {
            BigInteger bi;
            INumber num;
            ExtensibleComplex xc;

            if (other is int) return x + ((int)other);
            if (other is Complex64) return x + ((Complex64)other);
            if (other is double) return x + ((double)other);
            if ((object)(bi = other as BigInteger) != null) return x + bi;
            if ((num = other as INumber) != null) return num.ReverseAdd(x);
            if (other is bool) return x + ((bool) other ? 1 : 0);
            if (other is long) return x + ((long)other);
            if ((object)(xc = other as ExtensibleComplex) != null) return x + xc.value;
            if (other is byte) return x + (int)((byte)other);
            return Ops.NotImplemented;
        }


        [PythonName("__sub__")]
        public static object Subtract(BigInteger x, object other) {
            BigInteger bi;
            INumber num;
            ExtensibleComplex xc;

            if (other is int) return x - ((int)other);
            if (other is Complex64) return x - ((Complex64)other);
            if (other is double) return x - ((double)other);
            if ((object)(bi = other as BigInteger) != null) return x - bi;
            if ((num = other as INumber) != null) return num.ReverseSubtract(x);
            if (other is bool) return x - ((bool) other ? 1 : 0);
            if (other is long) return x - ((long)other);
            if ((object)(xc = other as ExtensibleComplex) != null) return x - xc.value;
            if (other is byte) return x - (int)((byte)other);
            return Ops.NotImplemented;
        }


        [PythonName("__pow__")]
        public static object Power(BigInteger x, object other) {
            BigInteger bi;
            ExtensibleComplex xc;
            INumber num;

            if (other is int) return Power(x, (int)other);
            if ((object)(bi = other as BigInteger) != null) return Power(x, bi);
            if ((num = other as INumber) != null) return num.ReversePower(x);
            if (other is double) return Power(x, (double)other);
            if (other is Complex64) return ComplexOps.Power(x, (Complex64)other);
            if (other is bool) return Power(x, (bool)other ? 1 : 0);
            if (other is long) return Power(x, (long)other);
            if ((object)(xc = other as ExtensibleComplex) != null) return Power(x, xc.value);
            if (other is byte) return Power(x, (int)((byte)other));
            return Ops.NotImplemented;
        }

        [PythonName("__rpow__")]
        public static object ReversePower(BigInteger x, object other) {
            BigInteger bi;
            ExtensibleComplex xc;
            INumber num;

            if (other is int) return IntOps.Power((int)other, x);
            if ((object)(bi = other as BigInteger) != null) return Power(bi, x);
            if ((num = other as INumber) != null) return num.Power(x);
            if (other is double) return FloatOps.Power((double)other, x);
            if (other is Complex64) return ComplexOps.Power((Complex64)other, x);
            if (other is bool) return IntOps.Power((bool)other ? 1 : 0, x);
            if (other is long) return Int64Ops.Power((long)other, x);
            if ((object)(xc = other as ExtensibleComplex) != null) return ComplexOps.Power(xc.value, x);
            if (other is byte) return IntOps.Power((int)((byte)other), x);
            return Ops.NotImplemented;
        }


        [PythonName("__mul__")]
        public static object Multiply(BigInteger x, object other) {
            BigInteger bi;
            INumber num;
            ExtensibleComplex xc;

            if (other is int) return x * ((int)other);
            if (other is Complex64) return x * ((Complex64)other);
            if (other is double) return x * ((double)other);
            if ((object)(bi = other as BigInteger) != null) return x * bi;
            if ((num = other as INumber) != null) return num.ReverseMultiply(x);
            if (other is bool) return x * ((bool) other ? 1 : 0);
            if (other is long) return x * ((long)other);
            if ((object)(xc = other as ExtensibleComplex) != null) return x * xc.value;
            if (other is byte) return x * (int)((byte)other);
            return Ops.NotImplemented;
        }


        [PythonName("__div__")]
        public static object Divide(BigInteger x, object other) {
            BigInteger bi;
            INumber num;
            ExtensibleComplex xc;

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
            if ((object)(num = other as INumber) != null) return num.ReverseDivide(x);
            if ((object)(xc = other as ExtensibleComplex) != null) {
                Complex64 y = xc.value;
                if(y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.Divide(Complex64.MakeReal(x), y);
            }
            if (other is byte) return Divide(x, (int)((byte)other));
            return Ops.NotImplemented;
        }


        [PythonName("__rdiv__")]
        public static object ReverseDivide(BigInteger x, object other) {
            BigInteger bi;
            ExtensibleComplex xc;
            INumber num;

            if (other is int) return IntOps.Divide((int)other, x);
            if (other is Complex64) {
                Complex64 y = (Complex64)other;
                if (x == 0) throw Ops.ZeroDivisionError();
                return ComplexOps.Divide(y, Complex64.MakeReal(x));
            }
            if (other is double) return FloatOps.Divide((double)other, x);
            if (other is bool) return Divide((bool)other ? 1 : 0, x);
            if (other is long) return Divide((long)other, x);
            if ((object)(bi = other as BigInteger) != null) return Divide(bi, x);
            if ((object)(num = other as INumber) != null) return num.Divide(x);
            if ((object)(xc = other as ExtensibleComplex) != null) {
                Complex64 y = xc.value;
                if (x == 0) throw Ops.ZeroDivisionError();
                return ComplexOps.Divide(y, Complex64.MakeReal(x));
            }
            if (other is byte) return IntOps.Divide((int)((byte)other), x);
            return Ops.NotImplemented;
        }



        [PythonName("__floordiv__")]
        public static object FloorDivide(BigInteger x, object other) {
            BigInteger bi;
            INumber num;
            ExtensibleComplex xc;

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
            if ((object)(num = other as INumber) != null) return num.ReverseFloorDivide(x);
            if ((object)(xc = other as ExtensibleComplex) != null) {
                Complex64 y = xc.value;
                if(y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.FloorDivide(Complex64.MakeReal(x), y);
            }
            if (other is byte) return Divide(x, (int)((byte)other));
            return Ops.NotImplemented;
        }


        [PythonName("__rfloordiv__")]
        public static object ReverseFloorDivide(BigInteger x, object other) {
            BigInteger bi;
            ExtensibleComplex xc;
            INumber num;

            if (other is int) return IntOps.Divide((int)other, x);
            if (other is Complex64) {
                Complex64 y = (Complex64)other;
                if (x == 0) throw Ops.ZeroDivisionError();
                return ComplexOps.FloorDivide(y, Complex64.MakeReal(x));
            }
            if (other is double) return FloatOps.FloorDivide((double)other, x);
            if (other is bool) return Divide((bool)other ? 1 : 0, x);
            if (other is long) return Divide((long)other, x);
            if ((object)(bi = other as BigInteger) != null) return Divide(bi, x);
            if ((object)(num = other as INumber) != null) return num.FloorDivide(x);
            if ((object)(xc = other as ExtensibleComplex) != null) {
                Complex64 y = xc.value;
                if (x == 0) throw Ops.ZeroDivisionError();
                return ComplexOps.FloorDivide(y, Complex64.MakeReal(x));
            }
            if (other is byte) return IntOps.Divide((int)((byte)other), x);
            return Ops.NotImplemented;
        }



        [PythonName("__truediv__")]
        public static object TrueDivide(BigInteger x, object other) {
            BigInteger bi;
            ExtensibleComplex xc;
            INumber num;

            if (other is int) return TrueDivide(x, (int)other);
            if ((object)(bi = other as BigInteger) != null) return TrueDivide(x, bi);
            if ((num = other as INumber) != null) return num.ReverseTrueDivide(x);
            if (other is double) return TrueDivide(x, (double)other);
            if (other is Complex64) return ComplexOps.TrueDivide(x, (Complex64)other);
            if (other is bool) return TrueDivide(x, (bool)other ? 1 : 0);
            if (other is long) return TrueDivide(x, (long)other);
            if ((object)(xc = other as ExtensibleComplex) != null) return TrueDivide(x, xc.value);
            if (other is byte) return TrueDivide(x, (int)((byte)other));
            return Ops.NotImplemented;
        }

        [PythonName("__rtruediv__")]
        public static object ReverseTrueDivide(BigInteger x, object other) {
            BigInteger bi;
            ExtensibleComplex xc;
            INumber num;

            if (other is int) return IntOps.TrueDivide((int)other, x);
            if ((object)(bi = other as BigInteger) != null) return TrueDivide(bi, x);
            if ((num = other as INumber) != null) return num.TrueDivide(x);
            if (other is double) return FloatOps.TrueDivide((double)other, x);
            if (other is Complex64) return ComplexOps.TrueDivide((Complex64)other, x);
            if (other is bool) return IntOps.TrueDivide((bool)other ? 1 : 0, x);
            if (other is long) return Int64Ops.TrueDivide((long)other, x);
            if ((object)(xc = other as ExtensibleComplex) != null) return ComplexOps.TrueDivide(xc.value, x);
            if (other is byte) return IntOps.TrueDivide((int)((byte)other), x);
            return Ops.NotImplemented;
        }


        [PythonName("__mod__")]
        public static object Mod(BigInteger x, object other) {
            BigInteger bi;
            INumber num;
            ExtensibleComplex xc;

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
            if ((object)(num = other as INumber) != null) return num.ReverseMod(x);
            if ((object)(xc = other as ExtensibleComplex) != null) {
                Complex64 y = xc.value;
                if(y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.Mod(Complex64.MakeReal(x), y);
            }
            if (other is byte) return Mod(x, (int)((byte)other));
            return Ops.NotImplemented;
        }


        [PythonName("__rmod__")]
        public static object ReverseMod(BigInteger x, object other) {
            BigInteger bi;
            ExtensibleComplex xc;
            INumber num;

            if (other is int) return IntOps.Mod((int)other, x);
            if (other is Complex64) {
                Complex64 y = (Complex64)other;
                if (x == 0) throw Ops.ZeroDivisionError();
                return ComplexOps.Mod(y, Complex64.MakeReal(x));
            }
            if (other is double) return FloatOps.Mod((double)other, x);
            if (other is bool) return Mod((bool)other ? 1 : 0, x);
            if (other is long) return Mod((long)other, x);
            if ((object)(bi = other as BigInteger) != null) return Mod(bi, x);
            if ((object)(num = other as INumber) != null) return num.Mod(x);
            if ((object)(xc = other as ExtensibleComplex) != null) {
                Complex64 y = xc.value;
                if (x == 0) throw Ops.ZeroDivisionError();
                return ComplexOps.Mod(y, Complex64.MakeReal(x));
            }
            if (other is byte) return IntOps.Mod((int)((byte)other), x);
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
            if ((object)(xi = other as ExtensibleInt) != null) return xi.ReverseBitwiseAnd(x);
            if ((object)(xl = other as ExtensibleLong) != null) return xl.ReverseBitwiseAnd(x);
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
            if ((object)(xi = other as ExtensibleInt) != null) return xi.ReverseBitwiseOr(x);
            if ((object)(xl = other as ExtensibleLong) != null) return xl.ReverseBitwiseOr(x);
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
            if ((object)(xi = other as ExtensibleInt) != null) return xi.ReverseXor(x);
            if ((object)(xl = other as ExtensibleLong) != null) return xl.ReverseXor(x);
            if (other is byte) return x ^ (int)((byte)other);
            return Ops.NotImplemented;
        }


        // *** END GENERATED CODE ***

        #endregion

    }
}
