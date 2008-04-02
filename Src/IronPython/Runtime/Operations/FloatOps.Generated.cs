/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public
 * License. A  copy of the license can be found in the License.html file at the
 * root of this distribution. If  you cannot locate the  Microsoft Public
 * License, please send an email to  dlr@microsoft.com. By using this source
 * code in any fashion, you are agreeing to be bound by the terms of the 
 * Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

using IronMath;
using IronPython.Runtime;

namespace IronPython.Runtime.Operations {
    public partial class ExtensibleFloat {
        #region Generated Extensible FloatOps

        // *** BEGIN GENERATED CODE ***

        [PythonName("__add__")]
        public virtual object Add(object other) {
            return FloatOps.Add(value, other);
        }

        [PythonName("__radd__")]
        public virtual object ReverseAdd(object other) {
            return FloatOps.ReverseAdd(value, other);
        }
        [PythonName("__sub__")]
        public virtual object Subtract(object other) {
            return FloatOps.Subtract(value, other);
        }

        [PythonName("__rsub__")]
        public virtual object ReverseSubtract(object other) {
            return FloatOps.ReverseSubtract(value, other);
        }
        [PythonName("__pow__")]
        public virtual object Power(object other) {
            return FloatOps.Power(value, other);
        }

        [PythonName("__rpow__")]
        public virtual object ReversePower(object other) {
            return FloatOps.ReversePower(value, other);
        }
        [PythonName("__mul__")]
        public virtual object Multiply(object other) {
            return FloatOps.Multiply(value, other);
        }

        [PythonName("__rmul__")]
        public virtual object ReverseMultiply(object other) {
            return FloatOps.ReverseMultiply(value, other);
        }
        [PythonName("__div__")]
        public virtual object Divide(object other) {
            return FloatOps.Divide(value, other);
        }

        [PythonName("__rdiv__")]
        public virtual object ReverseDivide(object other) {
            return FloatOps.ReverseDivide(value, other);
        }
        [PythonName("__floordiv__")]
        public virtual object FloorDivide(object other) {
            return FloatOps.FloorDivide(value, other);
        }

        [PythonName("__rfloordiv__")]
        public virtual object ReverseFloorDivide(object other) {
            return FloatOps.ReverseFloorDivide(value, other);
        }
        [PythonName("__truediv__")]
        public virtual object TrueDivide(object other) {
            return FloatOps.TrueDivide(value, other);
        }

        [PythonName("__rtruediv__")]
        public virtual object ReverseTrueDivide(object other) {
            return FloatOps.ReverseTrueDivide(value, other);
        }
        [PythonName("__mod__")]
        public virtual object Mod(object other) {
            return FloatOps.Mod(value, other);
        }

        [PythonName("__rmod__")]
        public virtual object ReverseMod(object other) {
            return FloatOps.ReverseMod(value, other);
        }

        // *** END GENERATED CODE ***

        #endregion
    }

    public static partial class FloatOps {
        #region Generated FloatOps

        // *** BEGIN GENERATED CODE ***


        [PythonName("__add__")]
        public static object Add(double x, object other) {
            BigInteger bi;
            ExtensibleComplex xc;
            INumber num;

            if (other is double) return x + ((double)other);
            if (other is int) return x + ((int)other);
            if (other is Complex64) return ComplexOps.Add(Complex64.MakeReal(x), (Complex64)other);
            if ((object)(bi = other as BigInteger) != null) return x + bi;
            if (other is float) return x + ((float)other);
            if ((object)(num = other as INumber) != null) return num.ReverseAdd(x);
            if (other is string) return Ops.NotImplemented;
            if (other is IConvertible) {
                double y = ((IConvertible)other).ToDouble(null);
                return x + y;
            }
            if (other is long) return x + ((long)other);
            if ((object)(xc = other as ExtensibleComplex) != null) return ComplexOps.Add(Complex64.MakeReal(x), xc.value);
            return Ops.NotImplemented;
        }


        [PythonName("__radd__")]
        public static object ReverseAdd(double x, object other) {
            BigInteger bi;
            ExtensibleComplex xc;
            INumber num;

            if (other is double) return ((double)other) + x;
            if (other is int) return ((int)other) + x;
            if (other is Complex64) return ComplexOps.Add((Complex64)other, Complex64.MakeReal(x));
            if ((object)(bi = other as BigInteger) != null) return bi + x;
            if (other is float) return ((float)other) + x;
            if ((object)(num = other as INumber) != null) return num.Add(x);
            if (other is string) return Ops.NotImplemented;
            if (other is long) return ((long)other) + x;
            if ((object)(xc = other as ExtensibleComplex) != null) return ComplexOps.Add(xc.value, Complex64.MakeReal(x));
            if (other is IConvertible) {
                double y = ((IConvertible)other).ToDouble(null);
                return y + x;
            }
            return Ops.NotImplemented;
        }


        [PythonName("__sub__")]
        public static object Subtract(double x, object other) {
            BigInteger bi;
            ExtensibleComplex xc;
            INumber num;

            if (other is double) return x - ((double)other);
            if (other is int) return x - ((int)other);
            if (other is Complex64) return ComplexOps.Subtract(Complex64.MakeReal(x), (Complex64)other);
            if ((object)(bi = other as BigInteger) != null) return x - bi;
            if (other is float) return x - ((float)other);
            if ((object)(num = other as INumber) != null) return num.ReverseSubtract(x);
            if (other is string) return Ops.NotImplemented;
            if (other is IConvertible) {
                double y = ((IConvertible)other).ToDouble(null);
                return x - y;
            }
            if (other is long) return x - ((long)other);
            if ((object)(xc = other as ExtensibleComplex) != null) return ComplexOps.Subtract(Complex64.MakeReal(x), xc.value);
            return Ops.NotImplemented;
        }


        [PythonName("__rsub__")]
        public static object ReverseSubtract(double x, object other) {
            BigInteger bi;
            ExtensibleComplex xc;
            INumber num;

            if (other is double) return ((double)other) - x;
            if (other is int) return ((int)other) - x;
            if (other is Complex64) return ComplexOps.Subtract((Complex64)other, Complex64.MakeReal(x));
            if ((object)(bi = other as BigInteger) != null) return bi - x;
            if (other is float) return ((float)other) - x;
            if ((object)(num = other as INumber) != null) return num.Subtract(x);
            if (other is string) return Ops.NotImplemented;
            if (other is long) return ((long)other) - x;
            if ((object)(xc = other as ExtensibleComplex) != null) return ComplexOps.Subtract(xc.value, Complex64.MakeReal(x));
            if (other is IConvertible) {
                double y = ((IConvertible)other).ToDouble(null);
                return y - x;
            }
            return Ops.NotImplemented;
        }


        [PythonName("__mul__")]
        public static object Multiply(double x, object other) {
            BigInteger bi;
            ExtensibleComplex xc;
            INumber num;

            if (other is double) return x * ((double)other);
            if (other is int) return x * ((int)other);
            if (other is Complex64) return ComplexOps.Multiply(Complex64.MakeReal(x), (Complex64)other);
            if ((object)(bi = other as BigInteger) != null) return x * bi;
            if (other is float) return x * ((float)other);
            if ((object)(num = other as INumber) != null) return num.ReverseMultiply(x);
            if (other is string) return Ops.NotImplemented;
            if (other is IConvertible) {
                double y = ((IConvertible)other).ToDouble(null);
                return x * y;
            }
            if (other is long) return x * ((long)other);
            if ((object)(xc = other as ExtensibleComplex) != null) return ComplexOps.Multiply(Complex64.MakeReal(x), xc.value);
            return Ops.NotImplemented;
        }


        [PythonName("__rmul__")]
        public static object ReverseMultiply(double x, object other) {
            BigInteger bi;
            ExtensibleComplex xc;
            INumber num;

            if (other is double) return ((double)other) * x;
            if (other is int) return ((int)other) * x;
            if (other is Complex64) return ComplexOps.Multiply((Complex64)other, Complex64.MakeReal(x));
            if ((object)(bi = other as BigInteger) != null) return bi * x;
            if (other is float) return ((float)other) * x;
            if ((object)(num = other as INumber) != null) return num.Multiply(x);
            if (other is string) return Ops.NotImplemented;
            if (other is long) return ((long)other) * x;
            if ((object)(xc = other as ExtensibleComplex) != null) return ComplexOps.Multiply(xc.value, Complex64.MakeReal(x));
            if (other is IConvertible) {
                double y = ((IConvertible)other).ToDouble(null);
                return y * x;
            }
            return Ops.NotImplemented;
        }


        [PythonName("__truediv__")]
        public static object TrueDivide(double x, object other) {
            BigInteger bi;
            ExtensibleComplex xc;
            INumber num;

            if (other is double) return TrueDivide(x, ((double)other));
            if (other is int) return TrueDivide(x, ((int)other));
            if (other is Complex64) return ComplexOps.TrueDivide(Complex64.MakeReal(x), (Complex64)other);
            if ((object)(bi = other as BigInteger) != null) return TrueDivide(x, bi);
            if (other is bool) return TrueDivide(x, (bool)other ? 1.0 : 0.0);
            if (other is float) return TrueDivide(x, ((float)other));
            if ((object)(num = other as INumber) != null) return num.ReverseTrueDivide(x);
            if (other is long) return TrueDivide(x, ((long)other));
            if ((object)(xc = other as ExtensibleComplex) != null) return ComplexOps.TrueDivide(Complex64.MakeReal(x), xc.value);
            if (other is byte) return TrueDivide(x, (int)((byte)other));
            return Ops.NotImplemented;
        }

        [PythonName("__rtruediv__")]
        public static object ReverseTrueDivide(double x, object other) {
            BigInteger bi;
            ExtensibleComplex xc;
            INumber num;

            if (other is double) return ReverseTrueDivide(x, ((double)other));
            if (other is int) return ReverseTrueDivide(x, ((int)other));
            if (other is Complex64) return ComplexOps.ReverseTrueDivide(Complex64.MakeReal(x), (Complex64)other);
            if ((object)(bi = other as BigInteger) != null) return ReverseTrueDivide(x, bi);
            if (other is bool) return ReverseTrueDivide(x, (bool)other ? 1.0 : 0.0);
            if (other is float) return ReverseTrueDivide(x, ((float)other));
            if ((object)(num = other as INumber) != null) return num.ReverseTrueDivide(x);
            if (other is long) return ReverseTrueDivide(x, ((long)other));
            if ((object)(xc = other as ExtensibleComplex) != null) return ComplexOps.ReverseTrueDivide(Complex64.MakeReal(x), xc.value);
            if (other is byte) return ReverseTrueDivide(x, (int)((byte)other));
            return Ops.NotImplemented;
        }


        // *** END GENERATED CODE ***

        #endregion

    }
}
