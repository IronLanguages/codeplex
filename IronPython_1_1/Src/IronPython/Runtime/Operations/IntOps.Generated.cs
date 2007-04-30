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
    public partial class ExtensibleInt {
        #region Generated Extensible IntOps

        // *** BEGIN GENERATED CODE ***

        [PythonName("__add__")]
        public virtual object Add(object other) {
            return IntOps.Add(value, other);
        }

        [PythonName("__radd__")]
        public virtual object ReverseAdd(object other) {
            return IntOps.ReverseAdd(value, other);
        }
        [PythonName("__sub__")]
        public virtual object Subtract(object other) {
            return IntOps.Subtract(value, other);
        }

        [PythonName("__rsub__")]
        public virtual object ReverseSubtract(object other) {
            return IntOps.ReverseSubtract(value, other);
        }
        [PythonName("__pow__")]
        public virtual object Power(object other) {
            return IntOps.Power(value, other);
        }

        [PythonName("__rpow__")]
        public virtual object ReversePower(object other) {
            return IntOps.ReversePower(value, other);
        }
        [PythonName("__mul__")]
        public virtual object Multiply(object other) {
            return IntOps.Multiply(value, other);
        }

        [PythonName("__rmul__")]
        public virtual object ReverseMultiply(object other) {
            return IntOps.ReverseMultiply(value, other);
        }
        [PythonName("__div__")]
        public virtual object Divide(object other) {
            return IntOps.Divide(value, other);
        }

        [PythonName("__rdiv__")]
        public virtual object ReverseDivide(object other) {
            return IntOps.ReverseDivide(value, other);
        }
        [PythonName("__floordiv__")]
        public virtual object FloorDivide(object other) {
            return IntOps.FloorDivide(value, other);
        }

        [PythonName("__rfloordiv__")]
        public virtual object ReverseFloorDivide(object other) {
            return IntOps.ReverseFloorDivide(value, other);
        }
        [PythonName("__truediv__")]
        public virtual object TrueDivide(object other) {
            return IntOps.TrueDivide(value, other);
        }

        [PythonName("__rtruediv__")]
        public virtual object ReverseTrueDivide(object other) {
            return IntOps.ReverseTrueDivide(value, other);
        }
        [PythonName("__mod__")]
        public virtual object Mod(object other) {
            return IntOps.Mod(value, other);
        }

        [PythonName("__rmod__")]
        public virtual object ReverseMod(object other) {
            return IntOps.ReverseMod(value, other);
        }
        [PythonName("__lshift__")]
        public virtual object LeftShift(object other) {
            return IntOps.LeftShift(value, other);
        }

        [PythonName("__rlshift__")]
        public virtual object ReverseLeftShift(object other) {
            return IntOps.ReverseLeftShift(value, other);
        }
        [PythonName("__rshift__")]
        public virtual object RightShift(object other) {
            return IntOps.RightShift(value, other);
        }

        [PythonName("__rrshift__")]
        public virtual object ReverseRightShift(object other) {
            return IntOps.ReverseRightShift(value, other);
        }
        [PythonName("__and__")]
        public virtual object BitwiseAnd(object other) {
            return IntOps.BitwiseAnd(value, other);
        }

        [PythonName("__rand__")]
        public virtual object ReverseBitwiseAnd(object other) {
            return IntOps.ReverseBitwiseAnd(value, other);
        }
        [PythonName("__or__")]
        public virtual object BitwiseOr(object other) {
            return IntOps.BitwiseOr(value, other);
        }

        [PythonName("__ror__")]
        public virtual object ReverseBitwiseOr(object other) {
            return IntOps.ReverseBitwiseOr(value, other);
        }
        [PythonName("__xor__")]
        public virtual object Xor(object other) {
            return IntOps.Xor(value, other);
        }

        [PythonName("__rxor__")]
        public virtual object ReverseXor(object other) {
            return IntOps.ReverseXor(value, other);
        }

        // *** END GENERATED CODE ***

        #endregion
    }

    public static partial class IntOps {
        #region Generated IntOps

        // *** BEGIN GENERATED CODE ***


        [PythonName("__add__")]
        public static object Add(int x, object other) {
            BigInteger bi;
            ExtensibleComplex xc;
            INumber num;

            if (other is int) {
                int y = (int)other;
                try {
                    return Ops.Int2Object(checked(x + y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) + y;
                }
            } else if ((object)(bi = other as BigInteger) != null) {
                return BigInteger.Create(x) + bi;
            } else if (other is double) {
                return x + (double)other;
            } else if (other is Complex64) {
                return ComplexOps.Add(Complex64.MakeReal(x), other);
            } else if (other is bool) {
                bool b = (bool)other;
                return x + (b ? 1 : 0);
            } else if (other is long) {
                long y = (long)other;
                try {
                    return checked(x + y);
                } catch (OverflowException) {
                    return BigInteger.Create(x) + y;
                }
            } else if (other is float) {
                return x + (float)other;
            } else if (other is byte) {
                return x + (byte)other;
            } else if ((object)(num = other as INumber) != null) {
                return num.ReverseAdd(x);
            } else if ((object)(xc = other as ExtensibleComplex) != null) {
                return ComplexOps.Add(Complex64.MakeReal(x), xc);
            } else if (other is byte) {
                int y = (byte)other;
                try {
                    return Ops.Int2Object(checked(x + y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) + y;
                }
            }
            return Ops.NotImplemented;
        }


        [PythonName("__sub__")]
        public static object Subtract(int x, object other) {
            BigInteger bi;
            ExtensibleComplex xc;
            INumber num;

            if (other is int) {
                int y = (int)other;
                try {
                    return Ops.Int2Object(checked(x - y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) - y;
                }
            } else if ((object)(bi = other as BigInteger) != null) {
                return BigInteger.Create(x) - bi;
            } else if (other is double) {
                return x - (double)other;
            } else if (other is Complex64) {
                return ComplexOps.Subtract(Complex64.MakeReal(x), other);
            } else if (other is bool) {
                bool b = (bool)other;
                return x - (b ? 1 : 0);
            } else if (other is long) {
                long y = (long)other;
                try {
                    return checked(x - y);
                } catch (OverflowException) {
                    return BigInteger.Create(x) - y;
                }
            } else if (other is float) {
                return x - (float)other;
            } else if (other is byte) {
                return x - (byte)other;
            } else if ((object)(num = other as INumber) != null) {
                return num.ReverseSubtract(x);
            } else if ((object)(xc = other as ExtensibleComplex) != null) {
                return ComplexOps.Subtract(Complex64.MakeReal(x), xc);
            } else if (other is byte) {
                int y = (byte)other;
                try {
                    return Ops.Int2Object(checked(x - y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) - y;
                }
            }
            return Ops.NotImplemented;
        }


        [PythonName("__mul__")]
        public static object Multiply(int x, object other) {
            BigInteger bi;
            ExtensibleComplex xc;
            INumber num;

            if (other is int) {
                int y = (int)other;
                try {
                    return Ops.Int2Object(checked(x * y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) * y;
                }
            } else if ((object)(bi = other as BigInteger) != null) {
                return BigInteger.Create(x) * bi;
            } else if (other is double) {
                return x * (double)other;
            } else if (other is Complex64) {
                return ComplexOps.Multiply(Complex64.MakeReal(x), other);
            } else if (other is bool) {
                bool b = (bool)other;
                return x * (b ? 1 : 0);
            } else if (other is long) {
                long y = (long)other;
                try {
                    return checked(x * y);
                } catch (OverflowException) {
                    return BigInteger.Create(x) * y;
                }
            } else if (other is float) {
                return x * (float)other;
            } else if (other is byte) {
                return x * (byte)other;
            } else if ((object)(num = other as INumber) != null) {
                return num.ReverseMultiply(x);
            } else if ((object)(xc = other as ExtensibleComplex) != null) {
                return ComplexOps.Multiply(Complex64.MakeReal(x), xc);
            } else if (other is byte) {
                int y = (byte)other;
                try {
                    return Ops.Int2Object(checked(x * y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) * y;
                }
            }
            return Ops.NotImplemented;
        }


        [PythonName("__div__")]
        public static object Divide(int x, object other) {
            BigInteger bi;
            ExtensibleComplex xc;
            INumber num;

            if (other is int) {
                int y = (int)other;
                try {
                    return Ops.Int2Object(Divide(x, y));
                } catch (OverflowException) {
                    return LongOps.Divide(BigInteger.Create(x), y);
                }
            } else if ((object)(bi = other as BigInteger) != null) {
                return LongOps.Divide(BigInteger.Create(x), bi);
            } else if (other is double) {
                return FloatOps.Divide(x, (double)other);
            } else if (other is Complex64) {
                Complex64 y = (Complex64)other;
                if (y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.Divide(Complex64.MakeReal(x), y);
            } else if (other is bool) {
                int y = (bool)other ? 1 : 0;
                try {
                    return Ops.Int2Object(Divide(x, y));
                } catch (OverflowException) {
                    return LongOps.Divide(BigInteger.Create(x), y);
                }
            } else if (other is long) {
                long y = (long)other;
                try {
                    return Divide(x, y);
                } catch (OverflowException) {
                    return LongOps.Divide(BigInteger.Create(x), y);
                }
            } else if (other is float) {
                return FloatOps.Divide(x, (float)other);
            } else if (other is byte) {
                return Ops.Int2Object(Divide(x, (int)((byte)other)));
            } else if ((object)(num = other as INumber) != null) {
                return num.ReverseDivide(x);
            } else if ((object)(xc = other as ExtensibleComplex) != null) {
                Complex64 y = xc.value;
                if (y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.Divide(Complex64.MakeReal(x), y);
            }

            return Ops.NotImplemented;
        }


        [PythonName("__rdiv__")]
        public static object ReverseDivide(int x, object other) {
            BigInteger bi;
            ExtensibleComplex xc;
            INumber num;

            if (other is int) {
                int y = (int)other;
                try {
                    return Ops.Int2Object(ReverseDivide(x, y));
                } catch (OverflowException) {
                    return LongOps.ReverseDivide(BigInteger.Create(x), y);
                }
            } else if ((object)(bi = other as BigInteger) != null) {
                return LongOps.ReverseDivide(BigInteger.Create(x), bi);
            } else if (other is double) {
                return FloatOps.ReverseDivide(x, (double)other);
            } else if (other is Complex64) {
                Complex64 y = (Complex64)other;
                if (x == 0) throw Ops.ZeroDivisionError();
                return ComplexOps.ReverseDivide(Complex64.MakeReal(x), y);
            } else if (other is bool) {
                int y = ((bool)other) ? 1 : 0;
                try {
                    return Ops.Int2Object(ReverseDivide(x, y));
                } catch (OverflowException) {
                    return LongOps.ReverseDivide(BigInteger.Create(x), y);
                }
            } else if (other is long) {
                long y = (long)other;
                try {
                    return ReverseDivide(x, y);
                } catch (OverflowException) {
                    return LongOps.ReverseDivide(BigInteger.Create(x), y);
                }
            } else if (other is float) {
                return FloatOps.ReverseDivide(x, (float)other);
            } else if (other is byte) {
                return Ops.Int2Object(ReverseDivide(x, (int)((byte)other)));
            } else if ((object)(num = other as INumber) != null) {
                return num.Divide(x);
            } else if ((object)(xc = other as ExtensibleComplex) != null) {
                Complex64 y = xc.value;
                if (x == 0) throw Ops.ZeroDivisionError();
                return ComplexOps.ReverseDivide(Complex64.MakeReal(x), y);
            }
            return Ops.NotImplemented;
        }


        [PythonName("__floordiv__")]
        public static object FloorDivide(int x, object other) {
            BigInteger bi;
            ExtensibleComplex xc;
            INumber num;

            if (other is int) {
                int y = (int)other;
                try {
                    return Ops.Int2Object(Divide(x, y));
                } catch (OverflowException) {
                    return LongOps.FloorDivide(BigInteger.Create(x), y);
                }
            } else if ((object)(bi = other as BigInteger) != null) {
                return LongOps.FloorDivide(BigInteger.Create(x), bi);
            } else if (other is double) {
                return FloatOps.FloorDivide(x, (double)other);
            } else if (other is Complex64) {
                Complex64 y = (Complex64)other;
                if (y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.FloorDivide(Complex64.MakeReal(x), y);
            } else if (other is bool) {
                int y = (bool)other ? 1 : 0;
                try {
                    return Ops.Int2Object(Divide(x, y));
                } catch (OverflowException) {
                    return LongOps.FloorDivide(BigInteger.Create(x), y);
                }
            } else if (other is long) {
                long y = (long)other;
                try {
                    return Divide(x, y);
                } catch (OverflowException) {
                    return LongOps.FloorDivide(BigInteger.Create(x), y);
                }
            } else if (other is float) {
                return FloatOps.FloorDivide(x, (float)other);
            } else if (other is byte) {
                return Ops.Int2Object(Divide(x, (int)((byte)other)));
            } else if ((object)(num = other as INumber) != null) {
                return num.ReverseFloorDivide(x);
            } else if ((object)(xc = other as ExtensibleComplex) != null) {
                Complex64 y = xc.value;
                if (y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.FloorDivide(Complex64.MakeReal(x), y);
            }

            return Ops.NotImplemented;
        }


        [PythonName("__rfloordiv__")]
        public static object ReverseFloorDivide(int x, object other) {
            BigInteger bi;
            ExtensibleComplex xc;
            INumber num;

            if (other is int) {
                int y = (int)other;
                try {
                    return Ops.Int2Object(ReverseDivide(x, y));
                } catch (OverflowException) {
                    return LongOps.ReverseFloorDivide(BigInteger.Create(x), y);
                }
            } else if ((object)(bi = other as BigInteger) != null) {
                return LongOps.ReverseFloorDivide(BigInteger.Create(x), bi);
            } else if (other is double) {
                return FloatOps.ReverseFloorDivide(x, (double)other);
            } else if (other is Complex64) {
                Complex64 y = (Complex64)other;
                if (x == 0) throw Ops.ZeroDivisionError();
                return ComplexOps.ReverseFloorDivide(Complex64.MakeReal(x), y);
            } else if (other is bool) {
                int y = ((bool)other) ? 1 : 0;
                try {
                    return Ops.Int2Object(ReverseDivide(x, y));
                } catch (OverflowException) {
                    return LongOps.ReverseFloorDivide(BigInteger.Create(x), y);
                }
            } else if (other is long) {
                long y = (long)other;
                try {
                    return ReverseDivide(x, y);
                } catch (OverflowException) {
                    return LongOps.ReverseFloorDivide(BigInteger.Create(x), y);
                }
            } else if (other is float) {
                return FloatOps.ReverseFloorDivide(x, (float)other);
            } else if (other is byte) {
                return Ops.Int2Object(ReverseDivide(x, (int)((byte)other)));
            } else if ((object)(num = other as INumber) != null) {
                return num.FloorDivide(x);
            } else if ((object)(xc = other as ExtensibleComplex) != null) {
                Complex64 y = xc.value;
                if (x == 0) throw Ops.ZeroDivisionError();
                return ComplexOps.ReverseFloorDivide(Complex64.MakeReal(x), y);
            }
            return Ops.NotImplemented;
        }


        [PythonName("__truediv__")]
        public static object TrueDivide(int x, object other) {
            BigInteger bi;
            ExtensibleComplex xc;
            INumber num;

            if (other is int) {
                return TrueDivide(x, (int)other);
            } else if (other is double) {
                return TrueDivide(x, (double)other);
            } else if (other is long) {
                return TrueDivide(x, (long)other);
            } else if ((object)(bi = other as BigInteger) != null) {
                return TrueDivide(x, bi);
            } else if (other is bool) {
                return TrueDivide(x, (bool)other ? 1 : 0);
            } else if (other is Complex64) {
                return TrueDivide(x, (Complex64)other);
            } else if ((object)(num = other as INumber) != null) {
                return num.ReverseTrueDivide(x);
            } else if ((object)(xc = other as ExtensibleComplex) != null) {
                return TrueDivide(x, xc.value);
            }
            return Ops.NotImplemented;
        }


        [PythonName("__mod__")]
        public static object Mod(int x, object other) {
            BigInteger bi;
            ExtensibleComplex xc;
            INumber num;

            if (other is int) {
                int y = (int)other;
                try {
                    return Ops.Int2Object(Mod(x, y));
                } catch (OverflowException) {
                    return LongOps.Mod(BigInteger.Create(x), y);
                }
            } else if ((object)(bi = other as BigInteger) != null) {
                return LongOps.Mod(BigInteger.Create(x), bi);
            } else if (other is double) {
                return FloatOps.Mod(x, (double)other);
            } else if (other is Complex64) {
                Complex64 y = (Complex64)other;
                if (y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.Mod(Complex64.MakeReal(x), y);
            } else if (other is bool) {
                int y = (bool)other ? 1 : 0;
                try {
                    return Ops.Int2Object(Mod(x, y));
                } catch (OverflowException) {
                    return LongOps.Mod(BigInteger.Create(x), y);
                }
            } else if (other is long) {
                long y = (long)other;
                try {
                    return Mod(x, y);
                } catch (OverflowException) {
                    return LongOps.Mod(BigInteger.Create(x), y);
                }
            } else if (other is float) {
                return FloatOps.Mod(x, (float)other);
            } else if (other is byte) {
                return Ops.Int2Object(Mod(x, (int)((byte)other)));
            } else if ((object)(num = other as INumber) != null) {
                return num.ReverseMod(x);
            } else if ((object)(xc = other as ExtensibleComplex) != null) {
                Complex64 y = xc.value;
                if (y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.Mod(Complex64.MakeReal(x), y);
            }

            return Ops.NotImplemented;
        }


        [PythonName("__rmod__")]
        public static object ReverseMod(int x, object other) {
            BigInteger bi;
            ExtensibleComplex xc;
            INumber num;

            if (other is int) {
                int y = (int)other;
                try {
                    return Ops.Int2Object(ReverseMod(x, y));
                } catch (OverflowException) {
                    return LongOps.ReverseMod(BigInteger.Create(x), y);
                }
            } else if ((object)(bi = other as BigInteger) != null) {
                return LongOps.ReverseMod(BigInteger.Create(x), bi);
            } else if (other is double) {
                return FloatOps.ReverseMod(x, (double)other);
            } else if (other is Complex64) {
                Complex64 y = (Complex64)other;
                if (x == 0) throw Ops.ZeroDivisionError();
                return ComplexOps.ReverseMod(Complex64.MakeReal(x), y);
            } else if (other is bool) {
                int y = ((bool)other) ? 1 : 0;
                try {
                    return Ops.Int2Object(ReverseMod(x, y));
                } catch (OverflowException) {
                    return LongOps.ReverseMod(BigInteger.Create(x), y);
                }
            } else if (other is long) {
                long y = (long)other;
                try {
                    return ReverseMod(x, y);
                } catch (OverflowException) {
                    return LongOps.ReverseMod(BigInteger.Create(x), y);
                }
            } else if (other is float) {
                return FloatOps.ReverseMod(x, (float)other);
            } else if (other is byte) {
                return Ops.Int2Object(ReverseMod(x, (int)((byte)other)));
            } else if ((object)(num = other as INumber) != null) {
                return num.Mod(x);
            } else if ((object)(xc = other as ExtensibleComplex) != null) {
                Complex64 y = xc.value;
                if (x == 0) throw Ops.ZeroDivisionError();
                return ComplexOps.ReverseMod(Complex64.MakeReal(x), y);
            }
            return Ops.NotImplemented;
        }


        [PythonName("__and__")]
        public static object BitwiseAnd(int x, object other) {
            BigInteger bi;
            ExtensibleInt xi;
            ExtensibleLong el;

            if (other is int) {
                return Ops.Int2Object(x & (int)other);
            } else if (other is long) {
                long lx = (long)x;
                return lx & (long)other;
            } else if ((object)(bi = other as BigInteger) != null) {
                return BigInteger.Create(x) & bi;
            } else if (other is bool) {
                return Ops.Int2Object(x & ((bool)other ? 1 : 0));
            } else if ((object)(xi = other as ExtensibleInt) != null) {
                return xi.ReverseBitwiseAnd(x);
            } else if (other is byte) {
                return Ops.Int2Object(x & (int)((byte)other));
            } else if ((object)(el = other as ExtensibleLong) != null) {
                return el.ReverseBitwiseAnd(x);
            }
            return Ops.NotImplemented;
        }


        [PythonName("__or__")]
        public static object BitwiseOr(int x, object other) {
            BigInteger bi;
            ExtensibleInt xi;
            ExtensibleLong el;

            if (other is int) {
                return Ops.Int2Object(x | (int)other);
            } else if (other is long) {
                long lx = (long)x;
                return lx | (long)other;
            } else if ((object)(bi = other as BigInteger) != null) {
                return BigInteger.Create(x) | bi;
            } else if (other is bool) {
                return Ops.Int2Object(x | ((bool)other ? 1 : 0));
            } else if ((object)(xi = other as ExtensibleInt) != null) {
                return xi.ReverseBitwiseOr(x);
            } else if (other is byte) {
                return Ops.Int2Object(x | (int)((byte)other));
            } else if ((object)(el = other as ExtensibleLong) != null) {
                return el.ReverseBitwiseOr(x);
            }
            return Ops.NotImplemented;
        }


        [PythonName("__xor__")]
        public static object Xor(int x, object other) {
            BigInteger bi;
            ExtensibleInt xi;
            ExtensibleLong el;

            if (other is int) {
                return Ops.Int2Object(x ^ (int)other);
            } else if (other is long) {
                long lx = (long)x;
                return lx ^ (long)other;
            } else if ((object)(bi = other as BigInteger) != null) {
                return BigInteger.Create(x) ^ bi;
            } else if (other is bool) {
                return Ops.Int2Object(x ^ ((bool)other ? 1 : 0));
            } else if ((object)(xi = other as ExtensibleInt) != null) {
                return xi.ReverseXor(x);
            } else if (other is byte) {
                return Ops.Int2Object(x ^ (int)((byte)other));
            } else if ((object)(el = other as ExtensibleLong) != null) {
                return el.ReverseXor(x);
            }
            return Ops.NotImplemented;
        }


        // *** END GENERATED CODE ***

        #endregion

    }
}
