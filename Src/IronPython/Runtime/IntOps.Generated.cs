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
    public static partial class IntOps {
        #region Generated IntOps

        // *** BEGIN GENERATED CODE ***


        [PythonName("__add__")]
        public static object Add(int x, object other) {
            BigInteger bi;
            ExtensibleInt xi;
            ExtensibleFloat xf;
            ExtensibleComplex xc;

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
            } else if ((object)(xi = other as ExtensibleInt) != null) {
                int y = xi.value;
                try {
                    return Ops.Int2Object(checked(x + y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) + y;
                }
            } else if ((object)(xf = other as ExtensibleFloat) != null) {
                return x + xf.value;
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
            ExtensibleInt xi;
            ExtensibleFloat xf;
            ExtensibleComplex xc;

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
            } else if ((object)(xi = other as ExtensibleInt) != null) {
                int y = xi.value;
                try {
                    return Ops.Int2Object(checked(x - y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) - y;
                }
            } else if ((object)(xf = other as ExtensibleFloat) != null) {
                return x - xf.value;
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
            ExtensibleInt xi;
            ExtensibleFloat xf;
            ExtensibleComplex xc;

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
            } else if ((object)(xi = other as ExtensibleInt) != null) {
                int y = xi.value;
                try {
                    return Ops.Int2Object(checked(x * y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) * y;
                }
            } else if ((object)(xf = other as ExtensibleFloat) != null) {
                return x * xf.value;
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
            ExtensibleInt xi;
            ExtensibleFloat xf;
            ExtensibleComplex xc;

            if (other is int) {
                int y = (int)other;
                try {
                    return Ops.Int2Object(Divide(x, y));
                } catch (OverflowException) {
                    return LongOps.Divide(BigInteger.Create(x) , y);
                }
            } else if ((object)(bi = other as BigInteger) != null) {
                return LongOps.Divide(BigInteger.Create(x), bi);
            } else if (other is double) {
                return FloatOps.Divide(x, (double)other);
            } else if (other is Complex64) {
                Complex64 y = (Complex64)other;
                if(y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.Divide(Complex64.MakeReal(x), y);
            } else if (other is bool) {
                bool b = (bool)other;
                return x / (b ? 1 : 0);
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
            } else if ((object)(xi = other as ExtensibleInt) != null) {
                int y = xi.value;
                try {
                    return Ops.Int2Object(Divide(x, y));
                } catch (OverflowException) {
                    return LongOps.Divide(BigInteger.Create(x) , y);
                }
            } else if ((object)(xf = other as ExtensibleFloat) != null) {
                return FloatOps.Divide(x, xf.value);
            } else if ((object)(xc = other as ExtensibleComplex) != null) {
                Complex64 y = xc.value;
                if(y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.Divide(Complex64.MakeReal(x), y);
            }
            return Ops.NotImplemented;
        }


        [PythonName("__rdiv__")]
        public static object ReverseDivide(int x, object other) {
            BigInteger bi;
            ExtensibleInt xi;
            ExtensibleFloat xf;
            ExtensibleComplex xc;

            if (other is int) {
                int y = (int)other;
                try {
                    return Ops.Int2Object(ReverseDivide(x, y));
                } catch (OverflowException) {
                    return LongOps.ReverseDivide(BigInteger.Create(x) , y);
                }
            } else if ((object)(bi = other as BigInteger) != null) {
                return LongOps.ReverseDivide(BigInteger.Create(x), bi);
            } else if (other is double) {
                return FloatOps.ReverseDivide(x, (double)other);
            } else if (other is Complex64) {
                Complex64 y = (Complex64)other;
                if(y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.ReverseDivide(Complex64.MakeReal(x), y);
            } else if (other is bool) {
                bool b = (bool)other;
                return (b ? 1 : 0) / x;
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
            } else if ((object)(xi = other as ExtensibleInt) != null) {
                int y = xi.value;
                try {
                    return Ops.Int2Object(ReverseDivide(x, y));
                } catch (OverflowException) {
                    return LongOps.ReverseDivide(BigInteger.Create(x) , y);
                }
            } else if ((object)(xf = other as ExtensibleFloat) != null) {
                return FloatOps.ReverseDivide(x, xf.value);
            } else if ((object)(xc = other as ExtensibleComplex) != null) {
                Complex64 y = xc.value;
                if(y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.ReverseDivide(Complex64.MakeReal(x), y);
            }
            return Ops.NotImplemented;
        }


        [PythonName("__floordiv__")]
        public static object FloorDivide(int x, object other) {
            BigInteger bi;
            ExtensibleInt xi;
            ExtensibleFloat xf;
            ExtensibleComplex xc;

            if (other is int) {
                int y = (int)other;
                try {
                    return Ops.Int2Object(Divide(x, y));
                } catch (OverflowException) {
                    return LongOps.FloorDivide(BigInteger.Create(x) , y);
                }
            } else if ((object)(bi = other as BigInteger) != null) {
                return LongOps.FloorDivide(BigInteger.Create(x), bi);
            } else if (other is double) {
                return FloatOps.FloorDivide(x, (double)other);
            } else if (other is Complex64) {
                Complex64 y = (Complex64)other;
                if(y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.FloorDivide(Complex64.MakeReal(x), y);
            } else if (other is bool) {
                bool b = (bool)other;
                return x / (b ? 1 : 0);
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
            } else if ((object)(xi = other as ExtensibleInt) != null) {
                int y = xi.value;
                try {
                    return Ops.Int2Object(Divide(x, y));
                } catch (OverflowException) {
                    return LongOps.FloorDivide(BigInteger.Create(x) , y);
                }
            } else if ((object)(xf = other as ExtensibleFloat) != null) {
                return FloatOps.FloorDivide(x, xf.value);
            } else if ((object)(xc = other as ExtensibleComplex) != null) {
                Complex64 y = xc.value;
                if(y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.FloorDivide(Complex64.MakeReal(x), y);
            }
            return Ops.NotImplemented;
        }


        [PythonName("__rfloordiv__")]
        public static object ReverseFloorDivide(int x, object other) {
            BigInteger bi;
            ExtensibleInt xi;
            ExtensibleFloat xf;
            ExtensibleComplex xc;

            if (other is int) {
                int y = (int)other;
                try {
                    return Ops.Int2Object(ReverseDivide(x, y));
                } catch (OverflowException) {
                    return LongOps.ReverseFloorDivide(BigInteger.Create(x) , y);
                }
            } else if ((object)(bi = other as BigInteger) != null) {
                return LongOps.ReverseFloorDivide(BigInteger.Create(x), bi);
            } else if (other is double) {
                return FloatOps.ReverseFloorDivide(x, (double)other);
            } else if (other is Complex64) {
                Complex64 y = (Complex64)other;
                if(y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.ReverseFloorDivide(Complex64.MakeReal(x), y);
            } else if (other is bool) {
                bool b = (bool)other;
                return (b ? 1 : 0) / x;
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
            } else if ((object)(xi = other as ExtensibleInt) != null) {
                int y = xi.value;
                try {
                    return Ops.Int2Object(ReverseDivide(x, y));
                } catch (OverflowException) {
                    return LongOps.ReverseFloorDivide(BigInteger.Create(x) , y);
                }
            } else if ((object)(xf = other as ExtensibleFloat) != null) {
                return FloatOps.ReverseFloorDivide(x, xf.value);
            } else if ((object)(xc = other as ExtensibleComplex) != null) {
                Complex64 y = xc.value;
                if(y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.ReverseFloorDivide(Complex64.MakeReal(x), y);
            }
            return Ops.NotImplemented;
        }


        [PythonName("__truediv__")]
        public static object TrueDivide(int x, object other) {
            BigInteger bi;
            ExtensibleInt xi;
            ExtensibleFloat xf;
            ExtensibleComplex xc;

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
            } else if ((object)(xi = other as ExtensibleInt) != null) {
                return TrueDivide(x, xi.value);
            } else if ((object)(xf = other as ExtensibleFloat) != null) {
                return TrueDivide(x, xf.value);
            } else if ((object)(xc = other as ExtensibleComplex) != null) {
                return TrueDivide(x, xc.value);
            }
            return Ops.NotImplemented;
        }


        [PythonName("__mod__")]
        public static object Mod(int x, object other) {
            BigInteger bi;
            ExtensibleInt xi;
            ExtensibleFloat xf;
            ExtensibleComplex xc;

            if (other is int) {
                int y = (int)other;
                try {
                    return Ops.Int2Object(Mod(x, y));
                } catch (OverflowException) {
                    return LongOps.Mod(BigInteger.Create(x) , y);
                }
            } else if ((object)(bi = other as BigInteger) != null) {
                return LongOps.Mod(BigInteger.Create(x), bi);
            } else if (other is double) {
                return FloatOps.Mod(x, (double)other);
            } else if (other is Complex64) {
                Complex64 y = (Complex64)other;
                if(y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.Mod(Complex64.MakeReal(x), y);
            } else if (other is bool) {
                bool b = (bool)other;
                return x % (b ? 1 : 0);
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
            } else if ((object)(xi = other as ExtensibleInt) != null) {
                int y = xi.value;
                try {
                    return Ops.Int2Object(Mod(x, y));
                } catch (OverflowException) {
                    return LongOps.Mod(BigInteger.Create(x) , y);
                }
            } else if ((object)(xf = other as ExtensibleFloat) != null) {
                return FloatOps.Mod(x, xf.value);
            } else if ((object)(xc = other as ExtensibleComplex) != null) {
                Complex64 y = xc.value;
                if(y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.Mod(Complex64.MakeReal(x), y);
            }
            return Ops.NotImplemented;
        }


        [PythonName("__rmod__")]
        public static object ReverseMod(int x, object other) {
            BigInteger bi;
            ExtensibleInt xi;
            ExtensibleFloat xf;
            ExtensibleComplex xc;

            if (other is int) {
                int y = (int)other;
                try {
                    return Ops.Int2Object(ReverseMod(x, y));
                } catch (OverflowException) {
                    return LongOps.ReverseMod(BigInteger.Create(x) , y);
                }
            } else if ((object)(bi = other as BigInteger) != null) {
                return LongOps.ReverseMod(BigInteger.Create(x), bi);
            } else if (other is double) {
                return FloatOps.ReverseMod(x, (double)other);
            } else if (other is Complex64) {
                Complex64 y = (Complex64)other;
                if(y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.ReverseMod(Complex64.MakeReal(x), y);
            } else if (other is bool) {
                bool b = (bool)other;
                return (b ? 1 : 0) % x;
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
            } else if ((object)(xi = other as ExtensibleInt) != null) {
                int y = xi.value;
                try {
                    return Ops.Int2Object(ReverseMod(x, y));
                } catch (OverflowException) {
                    return LongOps.ReverseMod(BigInteger.Create(x) , y);
                }
            } else if ((object)(xf = other as ExtensibleFloat) != null) {
                return FloatOps.ReverseMod(x, xf.value);
            } else if ((object)(xc = other as ExtensibleComplex) != null) {
                Complex64 y = xc.value;
                if(y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.ReverseMod(Complex64.MakeReal(x), y);
            }
            return Ops.NotImplemented;
        }


        [PythonName("__and__")]
        public static object BitwiseAnd(int x, object other) {
            BigInteger bi;
            ExtensibleInt xi;

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
                return Ops.Int2Object(x & xi.value);
            } else if (other is byte) {
                return Ops.Int2Object(x & (int)((byte)other));
            }
            return Ops.NotImplemented;
        }


        [PythonName("__or__")]
        public static object BitwiseOr(int x, object other) {
            BigInteger bi;
            ExtensibleInt xi;

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
                return Ops.Int2Object(x | xi.value);
            } else if (other is byte) {
                return Ops.Int2Object(x | (int)((byte)other));
            }
            return Ops.NotImplemented;
        }


        [PythonName("__xor__")]
        public static object Xor(int x, object other) {
            BigInteger bi;
            ExtensibleInt xi;

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
                return Ops.Int2Object(x ^ xi.value);
            } else if (other is byte) {
                return Ops.Int2Object(x ^ (int)((byte)other));
            }
            return Ops.NotImplemented;
        }


        // *** END GENERATED CODE ***

        #endregion

    }
}
