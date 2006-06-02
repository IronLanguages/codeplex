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
    public static partial class Int64Ops {
        #region Generated Int64Ops

        // *** BEGIN GENERATED CODE ***


        [PythonName("__add__")]
        public static object Add(long x, object other) {
            BigInteger bi;
            ExtensibleComplex xc;
            INumber num;

            if (other is int) {
                int y = (int)other;
                try {
                    return Ops.Long2Object(checked(x + y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) + y;
                }
            } else if ((object)(bi = other as BigInteger) != null) {
                return BigInteger.Create(x) + bi;
            } else if (other is double) {
                return x + (double)other;
            } else if (other is Complex64) {
                return Complex64.MakeReal(x) + (Complex64)other;
            } else if (other is bool) {
                int y = (bool)other ? 1 : 0;
                try {
                    return Ops.Long2Object(checked(x + y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) + y;
                }
            } else if (other is long) {
                long y = (long)other;
                try {
                    return checked(x + y);
                } catch (OverflowException) {
                    return BigInteger.Create(x) + y;
                }
            } else if (other is float) {
                return x + (float)other;
            } else if ((object)(num = other as INumber) != null) {
                return num.ReverseAdd(x);
            } else if ((object)(xc = other as ExtensibleComplex) != null) {
                return Complex64.MakeReal(x) + xc.value;
            } else if (other is byte) {
                int y = (int)((byte)other);
                try {
                    return Ops.Long2Object(checked(x + y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) + y;
                }
            }
            return Ops.NotImplemented;
        }

        [PythonName("__radd__")]
        public static object ReverseAdd(long x, object other) {
            BigInteger bi;
            ExtensibleComplex xc;
            INumber num;

            if (other is int) {
                int y = (int)other;
                try {
                    return Ops.Long2Object(checked(y + x));
                } catch (OverflowException) {
                    return y + BigInteger.Create(x);
                }
            } else if ((object)(bi = other as BigInteger) != null) {
                return bi + BigInteger.Create(x);
            } else if (other is double) {
                return (double)other + x;
            } else if (other is Complex64) {
                return (Complex64)other + Complex64.MakeReal(x);
            } else if (other is bool) {
                int y = (bool)other ? 1 : 0;
                try {
                    return Ops.Long2Object(checked(y + x));
                } catch (OverflowException) {
                    return BigInteger.Create(x) + y;
                }
            } else if (other is long) {
                long y = (long)other;
                try {
                    return checked(y + x);
                } catch (OverflowException) {
                    return y + BigInteger.Create(x);
                }
            } else if (other is float) {
                return (float)other + x;
            } else if ((object)(num = other as INumber) != null) {
                return num.Add(x);
            } else if ((object)(xc = other as ExtensibleComplex) != null) {
                return xc.value + Complex64.MakeReal(x);
            } else if (other is byte) {
                int y = (int)((byte)other);
                try {
                    return Ops.Long2Object(checked(y + x));
                } catch (OverflowException) {
                    return y + BigInteger.Create(x);
                }
            }
            return Ops.NotImplemented;
        }


        [PythonName("__sub__")]
        public static object Subtract(long x, object other) {
            BigInteger bi;
            ExtensibleComplex xc;
            INumber num;

            if (other is int) {
                int y = (int)other;
                try {
                    return Ops.Long2Object(checked(x - y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) - y;
                }
            } else if ((object)(bi = other as BigInteger) != null) {
                return BigInteger.Create(x) - bi;
            } else if (other is double) {
                return x - (double)other;
            } else if (other is Complex64) {
                return Complex64.MakeReal(x) - (Complex64)other;
            } else if (other is bool) {
                int y = (bool)other ? 1 : 0;
                try {
                    return Ops.Long2Object(checked(x - y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) - y;
                }
            } else if (other is long) {
                long y = (long)other;
                try {
                    return checked(x - y);
                } catch (OverflowException) {
                    return BigInteger.Create(x) - y;
                }
            } else if (other is float) {
                return x - (float)other;
            } else if ((object)(num = other as INumber) != null) {
                return num.ReverseSubtract(x);
            } else if ((object)(xc = other as ExtensibleComplex) != null) {
                return Complex64.MakeReal(x) - xc.value;
            } else if (other is byte) {
                int y = (int)((byte)other);
                try {
                    return Ops.Long2Object(checked(x - y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) - y;
                }
            }
            return Ops.NotImplemented;
        }

        [PythonName("__rsub__")]
        public static object ReverseSubtract(long x, object other) {
            BigInteger bi;
            ExtensibleComplex xc;
            INumber num;

            if (other is int) {
                int y = (int)other;
                try {
                    return Ops.Long2Object(checked(y - x));
                } catch (OverflowException) {
                    return y - BigInteger.Create(x);
                }
            } else if ((object)(bi = other as BigInteger) != null) {
                return bi - BigInteger.Create(x);
            } else if (other is double) {
                return (double)other - x;
            } else if (other is Complex64) {
                return (Complex64)other - Complex64.MakeReal(x);
            } else if (other is bool) {
                int y = (bool)other ? 1 : 0;
                try {
                    return Ops.Long2Object(checked(y - x));
                } catch (OverflowException) {
                    return BigInteger.Create(x) - y;
                }
            } else if (other is long) {
                long y = (long)other;
                try {
                    return checked(y - x);
                } catch (OverflowException) {
                    return y - BigInteger.Create(x);
                }
            } else if (other is float) {
                return (float)other - x;
            } else if ((object)(num = other as INumber) != null) {
                return num.Subtract(x);
            } else if ((object)(xc = other as ExtensibleComplex) != null) {
                return xc.value - Complex64.MakeReal(x);
            } else if (other is byte) {
                int y = (int)((byte)other);
                try {
                    return Ops.Long2Object(checked(y - x));
                } catch (OverflowException) {
                    return y - BigInteger.Create(x);
                }
            }
            return Ops.NotImplemented;
        }


        [PythonName("__pow__")]
        public static object Power(long x, object other) {
            BigInteger bi;
            ExtensibleComplex xc;
            INumber num;

            if (other is int) return Power(x, (int)other);
            if ((object)(bi = other as BigInteger) != null) return Power(x, bi);
            if (other is long) return Power(x, (long)other);
            if (other is double) return Power(x, (double)other);
            if (other is Complex64) return ComplexOps.Power(x, (Complex64)other);
            if (other is bool) return Power(x, (bool)other ? 1 : 0); 
            if (other is float) return Power(x, (float)other);
            if ((object)(num = other as INumber) != null) return num.ReversePower(x);
            if ((object)(xc = other as ExtensibleComplex) != null) return Power(x, xc.value);
            if (other is byte) return Power(x, (int)((byte)other));
            return Ops.NotImplemented;
        }


        [PythonName("__mul__")]
        public static object Multiply(long x, object other) {
            BigInteger bi;
            ExtensibleComplex xc;
            INumber num;

            if (other is int) {
                int y = (int)other;
                try {
                    return Ops.Long2Object(checked(x * y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) * y;
                }
            } else if ((object)(bi = other as BigInteger) != null) {
                return BigInteger.Create(x) * bi;
            } else if (other is double) {
                return x * (double)other;
            } else if (other is Complex64) {
                return Complex64.MakeReal(x) * (Complex64)other;
            } else if (other is bool) {
                int y = (bool)other ? 1 : 0;
                try {
                    return Ops.Long2Object(checked(x * y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) * y;
                }
            } else if (other is long) {
                long y = (long)other;
                try {
                    return checked(x * y);
                } catch (OverflowException) {
                    return BigInteger.Create(x) * y;
                }
            } else if (other is float) {
                return x * (float)other;
            } else if ((object)(num = other as INumber) != null) {
                return num.ReverseMultiply(x);
            } else if ((object)(xc = other as ExtensibleComplex) != null) {
                return Complex64.MakeReal(x) * xc.value;
            } else if (other is byte) {
                int y = (int)((byte)other);
                try {
                    return Ops.Long2Object(checked(x * y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) * y;
                }
            }
            return Ops.NotImplemented;
        }

        [PythonName("__rmul__")]
        public static object ReverseMultiply(long x, object other) {
            BigInteger bi;
            ExtensibleComplex xc;
            INumber num;

            if (other is int) {
                int y = (int)other;
                try {
                    return Ops.Long2Object(checked(y * x));
                } catch (OverflowException) {
                    return y * BigInteger.Create(x);
                }
            } else if ((object)(bi = other as BigInteger) != null) {
                return bi * BigInteger.Create(x);
            } else if (other is double) {
                return (double)other * x;
            } else if (other is Complex64) {
                return (Complex64)other * Complex64.MakeReal(x);
            } else if (other is bool) {
                int y = (bool)other ? 1 : 0;
                try {
                    return Ops.Long2Object(checked(y * x));
                } catch (OverflowException) {
                    return BigInteger.Create(x) * y;
                }
            } else if (other is long) {
                long y = (long)other;
                try {
                    return checked(y * x);
                } catch (OverflowException) {
                    return y * BigInteger.Create(x);
                }
            } else if (other is float) {
                return (float)other * x;
            } else if ((object)(num = other as INumber) != null) {
                return num.Multiply(x);
            } else if ((object)(xc = other as ExtensibleComplex) != null) {
                return xc.value * Complex64.MakeReal(x);
            } else if (other is byte) {
                int y = (int)((byte)other);
                try {
                    return Ops.Long2Object(checked(y * x));
                } catch (OverflowException) {
                    return y * BigInteger.Create(x);
                }
            }
            return Ops.NotImplemented;
        }


        [PythonName("__div__")]
        public static object Divide(long x, object other) {
            BigInteger bi;
            ExtensibleComplex xc;
            INumber num;

            if (other is int) {
                int y = (int)other;
                try {
                    return Ops.Long2Object(Divide(x, y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) / y;
                }
            } else if ((object)(bi = other as BigInteger) != null) {
                return LongOps.Divide(BigInteger.Create(x), bi);
            } else if (other is double) {
                return FloatOps.Divide(x, (double)other);
            } else if (other is Complex64) {
                return ComplexOps.Divide(Complex64.MakeReal(x), (Complex64)other);
            } else if (other is bool) {
                int y = (bool)other ? 1 : 0;
                try {
                    return Ops.Long2Object(Divide(x, y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) / y;
                }    
            } else if (other is long) {
                long y = (long)other;
                try {
                    return Divide(x, y);
                } catch (OverflowException) {
                    return BigInteger.Create(x) / y;
                }
            } else if (other is float) {
                return FloatOps.Divide(x, (float)other);
            } else if ((object)(num = other as INumber) != null) {
                return num.ReverseDivide(x);
            } else if ((object)(xc = other as ExtensibleComplex) != null) {
                return ComplexOps.Divide(Complex64.MakeReal(x), xc.value);
            } else if (other is byte) {
                int y = (int)((byte)other);
                try {
                    return Ops.Long2Object(Divide(x, y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) / y;
                }
            }

            return Ops.NotImplemented;
        }


        [PythonName("__rdiv__")]
        public static object ReverseDivide(long x, object other) {
            BigInteger bi;
            ExtensibleComplex xc;
            INumber num;

            if (other is int) {
                int y = (int)other;
                try {
                    return Ops.Long2Object(ReverseDivide(x, y));
                } catch (OverflowException) {
                    return y / BigInteger.Create(x);
                }
            } else if ((object)(bi = other as BigInteger) != null) {
                return LongOps.ReverseDivide(BigInteger.Create(x), bi);
            } else if (other is double) {
                return FloatOps.ReverseDivide(x, (double)other);
            } else if (other is Complex64) {
                return ComplexOps.ReverseDivide(Complex64.MakeReal(x), (Complex64)other);
            } else if (other is bool) {
                int y = (bool)other ? 1 : 0;
                try {
                    return Ops.Long2Object(ReverseDivide(x, y));
                } catch (OverflowException) {
                    return y / BigInteger.Create(x);
                }    
            } else if (other is long) {
                long y = (long)other;
                try {
                    return ReverseDivide(x, y);
                } catch (OverflowException) {
                    return y / BigInteger.Create(x);
                }
            } else if (other is float) {
                return FloatOps.ReverseDivide(x, (float)other);
            } else if ((object)(num = other as INumber) != null) {
                return num.Divide(x);
            } else if ((object)(xc = other as ExtensibleComplex) != null) {
                return ComplexOps.ReverseDivide(Complex64.MakeReal(x), xc.value);
            } else if (other is byte) {
                int y = (int)((byte)other);
                try {
                    return Ops.Long2Object(ReverseDivide(x, y));
                } catch (OverflowException) {
                    return y / BigInteger.Create(x);
                }
            }

            return Ops.NotImplemented;
        }


        [PythonName("__floordiv__")]
        public static object FloorDivide(long x, object other) {
            BigInteger bi;
            ExtensibleComplex xc;
            INumber num;

            if (other is int) {
                int y = (int)other;
                try {
                    return Ops.Long2Object(Divide(x, y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) / y;
                }
            } else if ((object)(bi = other as BigInteger) != null) {
                return LongOps.FloorDivide(BigInteger.Create(x), bi);
            } else if (other is double) {
                return FloatOps.FloorDivide(x, (double)other);
            } else if (other is Complex64) {
                return ComplexOps.FloorDivide(Complex64.MakeReal(x), (Complex64)other);
            } else if (other is bool) {
                int y = (bool)other ? 1 : 0;
                try {
                    return Ops.Long2Object(Divide(x, y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) / y;
                }    
            } else if (other is long) {
                long y = (long)other;
                try {
                    return Divide(x, y);
                } catch (OverflowException) {
                    return BigInteger.Create(x) / y;
                }
            } else if (other is float) {
                return FloatOps.FloorDivide(x, (float)other);
            } else if ((object)(num = other as INumber) != null) {
                return num.ReverseFloorDivide(x);
            } else if ((object)(xc = other as ExtensibleComplex) != null) {
                return ComplexOps.FloorDivide(Complex64.MakeReal(x), xc.value);
            } else if (other is byte) {
                int y = (int)((byte)other);
                try {
                    return Ops.Long2Object(Divide(x, y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) / y;
                }
            }

            return Ops.NotImplemented;
        }


        [PythonName("__rfloordiv__")]
        public static object ReverseFloorDivide(long x, object other) {
            BigInteger bi;
            ExtensibleComplex xc;
            INumber num;

            if (other is int) {
                int y = (int)other;
                try {
                    return Ops.Long2Object(ReverseDivide(x, y));
                } catch (OverflowException) {
                    return y / BigInteger.Create(x);
                }
            } else if ((object)(bi = other as BigInteger) != null) {
                return LongOps.ReverseFloorDivide(BigInteger.Create(x), bi);
            } else if (other is double) {
                return FloatOps.ReverseFloorDivide(x, (double)other);
            } else if (other is Complex64) {
                return ComplexOps.ReverseFloorDivide(Complex64.MakeReal(x), (Complex64)other);
            } else if (other is bool) {
                int y = (bool)other ? 1 : 0;
                try {
                    return Ops.Long2Object(ReverseDivide(x, y));
                } catch (OverflowException) {
                    return y / BigInteger.Create(x);
                }    
            } else if (other is long) {
                long y = (long)other;
                try {
                    return ReverseDivide(x, y);
                } catch (OverflowException) {
                    return y / BigInteger.Create(x);
                }
            } else if (other is float) {
                return FloatOps.ReverseFloorDivide(x, (float)other);
            } else if ((object)(num = other as INumber) != null) {
                return num.FloorDivide(x);
            } else if ((object)(xc = other as ExtensibleComplex) != null) {
                return ComplexOps.ReverseFloorDivide(Complex64.MakeReal(x), xc.value);
            } else if (other is byte) {
                int y = (int)((byte)other);
                try {
                    return Ops.Long2Object(ReverseDivide(x, y));
                } catch (OverflowException) {
                    return y / BigInteger.Create(x);
                }
            }

            return Ops.NotImplemented;
        }


        [PythonName("__truediv__")]
        public static object TrueDivide(long x, object other) {
            BigInteger bi;
            ExtensibleComplex xc;
            INumber num;

            if (other is int) return TrueDivide(x, (int)other);
            if ((object)(bi = other as BigInteger) != null) return TrueDivide(x, bi);
            if (other is long) return TrueDivide(x, (long)other);
            if (other is double) return TrueDivide(x, (double)other);
            if (other is Complex64) return ComplexOps.TrueDivide(x, (Complex64)other);
            if (other is bool) return TrueDivide(x, (bool)other ? 1 : 0); 
            if (other is float) return TrueDivide(x, (float)other);
            if ((object)(num = other as INumber) != null) return num.ReverseTrueDivide(x);
            if ((object)(xc = other as ExtensibleComplex) != null) return TrueDivide(x, xc.value);
            if (other is byte) return TrueDivide(x, (int)((byte)other));
            return Ops.NotImplemented;
        }


        [PythonName("__mod__")]
        public static object Mod(long x, object other) {
            BigInteger bi;
            ExtensibleComplex xc;
            INumber num;

            if (other is int) {
                int y = (int)other;
                try {
                    return Ops.Long2Object(Mod(x, y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) % y;
                }
            } else if ((object)(bi = other as BigInteger) != null) {
                return LongOps.Mod(BigInteger.Create(x), bi);
            } else if (other is double) {
                return FloatOps.Mod(x, (double)other);
            } else if (other is Complex64) {
                return ComplexOps.Mod(Complex64.MakeReal(x), (Complex64)other);
            } else if (other is bool) {
                int y = (bool)other ? 1 : 0;
                try {
                    return Ops.Long2Object(Mod(x, y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) % y;
                }    
            } else if (other is long) {
                long y = (long)other;
                try {
                    return Mod(x, y);
                } catch (OverflowException) {
                    return BigInteger.Create(x) % y;
                }
            } else if (other is float) {
                return FloatOps.Mod(x, (float)other);
            } else if ((object)(num = other as INumber) != null) {
                return num.ReverseMod(x);
            } else if ((object)(xc = other as ExtensibleComplex) != null) {
                return ComplexOps.Mod(Complex64.MakeReal(x), xc.value);
            } else if (other is byte) {
                int y = (int)((byte)other);
                try {
                    return Ops.Long2Object(Mod(x, y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) % y;
                }
            }

            return Ops.NotImplemented;
        }


        [PythonName("__rmod__")]
        public static object ReverseMod(long x, object other) {
            BigInteger bi;
            ExtensibleComplex xc;
            INumber num;

            if (other is int) {
                int y = (int)other;
                try {
                    return Ops.Long2Object(ReverseMod(x, y));
                } catch (OverflowException) {
                    return y % BigInteger.Create(x);
                }
            } else if ((object)(bi = other as BigInteger) != null) {
                return LongOps.ReverseMod(BigInteger.Create(x), bi);
            } else if (other is double) {
                return FloatOps.ReverseMod(x, (double)other);
            } else if (other is Complex64) {
                return ComplexOps.ReverseMod(Complex64.MakeReal(x), (Complex64)other);
            } else if (other is bool) {
                int y = (bool)other ? 1 : 0;
                try {
                    return Ops.Long2Object(ReverseMod(x, y));
                } catch (OverflowException) {
                    return y % BigInteger.Create(x);
                }    
            } else if (other is long) {
                long y = (long)other;
                try {
                    return ReverseMod(x, y);
                } catch (OverflowException) {
                    return y % BigInteger.Create(x);
                }
            } else if (other is float) {
                return FloatOps.ReverseMod(x, (float)other);
            } else if ((object)(num = other as INumber) != null) {
                return num.Mod(x);
            } else if ((object)(xc = other as ExtensibleComplex) != null) {
                return ComplexOps.ReverseMod(Complex64.MakeReal(x), xc.value);
            } else if (other is byte) {
                int y = (int)((byte)other);
                try {
                    return Ops.Long2Object(ReverseMod(x, y));
                } catch (OverflowException) {
                    return y % BigInteger.Create(x);
                }
            }

            return Ops.NotImplemented;
        }


        [PythonName("__and__")]
        public static object BitwiseAnd(long x, object other) {
            BigInteger bi;
            ExtensibleInt xi;
            ExtensibleLong el;

            if (other is int) {
                long y = (long)(int)other;
                return Ops.Long2Object(x & y);
            } else if (other is long) {
                return x & (long)other;
            } else if ((object)(bi = other as BigInteger) != null) {
                return BigInteger.Create(x) & bi;
            } else if (other is bool) {
                return Ops.Long2Object(x & ((bool)other ? 1L : 0L));
            } else if ((object)(xi = other as ExtensibleInt) != null) {
                return xi.ReverseBitwiseAnd(x);
            } else if ((object)(el = other as ExtensibleLong) != null) {
                return el.ReverseBitwiseAnd(BigInteger.Create(x));
            } else if (other is byte) {
                return Ops.Long2Object(x & (long)((byte)other));
            }
            return Ops.NotImplemented;
        }


        [PythonName("__or__")]
        public static object BitwiseOr(long x, object other) {
            BigInteger bi;
            ExtensibleInt xi;
            ExtensibleLong el;

            if (other is int) {
                long y = (long)(int)other;
                return Ops.Long2Object(x | y);
            } else if (other is long) {
                return x | (long)other;
            } else if ((object)(bi = other as BigInteger) != null) {
                return BigInteger.Create(x) | bi;
            } else if (other is bool) {
                return Ops.Long2Object(x | ((bool)other ? 1L : 0L));
            } else if ((object)(xi = other as ExtensibleInt) != null) {
                return xi.ReverseBitwiseOr(x);
            } else if ((object)(el = other as ExtensibleLong) != null) {
                return el.ReverseBitwiseOr(BigInteger.Create(x));
            } else if (other is byte) {
                return Ops.Long2Object(x | (long)((byte)other));
            }
            return Ops.NotImplemented;
        }


        [PythonName("__xor__")]
        public static object Xor(long x, object other) {
            BigInteger bi;
            ExtensibleInt xi;
            ExtensibleLong el;

            if (other is int) {
                long y = (long)(int)other;
                return Ops.Long2Object(x ^ y);
            } else if (other is long) {
                return x ^ (long)other;
            } else if ((object)(bi = other as BigInteger) != null) {
                return BigInteger.Create(x) ^ bi;
            } else if (other is bool) {
                return Ops.Long2Object(x ^ ((bool)other ? 1L : 0L));
            } else if ((object)(xi = other as ExtensibleInt) != null) {
                return xi.ReverseXor(x);
            } else if ((object)(el = other as ExtensibleLong) != null) {
                return el.ReverseXor(BigInteger.Create(x));
            } else if (other is byte) {
                return Ops.Long2Object(x ^ (long)((byte)other));
            }
            return Ops.NotImplemented;
        }


        // *** END GENERATED CODE ***

        #endregion

    }
}
