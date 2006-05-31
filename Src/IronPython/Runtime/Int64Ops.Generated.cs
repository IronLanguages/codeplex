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
            ExtensibleInt xi;
            ExtensibleFloat xf;
            ExtensibleComplex xc;
            ExtensibleLong el;

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
            } else if ((object)(xi = other as ExtensibleInt) != null) {
                int y = xi.value;
                try {
                    return Ops.Long2Object(checked(x + y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) + y;
                }
            } else if ((object)(xf = other as ExtensibleFloat) != null) {
                return x + xf.value;
            } else if ((object)(xc = other as ExtensibleComplex) != null) {
                return Complex64.MakeReal(x) + xc.value;
            } else if ((object)(el = other as ExtensibleLong) != null) {
                return BigInteger.Create(x) + el.Value;
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
            ExtensibleInt xi;
            ExtensibleFloat xf;
            ExtensibleComplex xc;
            ExtensibleLong el;

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
            } else if ((object)(xi = other as ExtensibleInt) != null) {
                int y = xi.value;
                try {
                    return Ops.Long2Object(checked(y + x));
                } catch (OverflowException) {
                    return y + BigInteger.Create(x);
                }
            } else if ((object)(xf = other as ExtensibleFloat) != null) {
                return xf.value + x;
            } else if ((object)(xc = other as ExtensibleComplex) != null) {
                return xc.value + Complex64.MakeReal(x);
            } else if ((object)(el = other as ExtensibleLong) != null) {
                return el.Value + BigInteger.Create(x);
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
            ExtensibleInt xi;
            ExtensibleFloat xf;
            ExtensibleComplex xc;
            ExtensibleLong el;

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
            } else if ((object)(xi = other as ExtensibleInt) != null) {
                int y = xi.value;
                try {
                    return Ops.Long2Object(checked(x - y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) - y;
                }
            } else if ((object)(xf = other as ExtensibleFloat) != null) {
                return x - xf.value;
            } else if ((object)(xc = other as ExtensibleComplex) != null) {
                return Complex64.MakeReal(x) - xc.value;
            } else if ((object)(el = other as ExtensibleLong) != null) {
                return BigInteger.Create(x) - el.Value;
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
            ExtensibleInt xi;
            ExtensibleFloat xf;
            ExtensibleComplex xc;
            ExtensibleLong el;

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
            } else if ((object)(xi = other as ExtensibleInt) != null) {
                int y = xi.value;
                try {
                    return Ops.Long2Object(checked(y - x));
                } catch (OverflowException) {
                    return y - BigInteger.Create(x);
                }
            } else if ((object)(xf = other as ExtensibleFloat) != null) {
                return xf.value - x;
            } else if ((object)(xc = other as ExtensibleComplex) != null) {
                return xc.value - Complex64.MakeReal(x);
            } else if ((object)(el = other as ExtensibleLong) != null) {
                return el.Value - BigInteger.Create(x);
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
            ExtensibleInt xi;
            ExtensibleFloat xf;
            ExtensibleComplex xc;
            ExtensibleLong el;

            if (other is int) return Power(x, (int)other);
            if ((object)(bi = other as BigInteger) != null) return Power(x, bi);
            if (other is long) return Power(x, (long)other);
            if (other is double) return Power(x, (double)other);
            if (other is Complex64) return ComplexOps.Power(x, (Complex64)other);
            if (other is bool) return Power(x, (bool)other ? 1 : 0); 
            if (other is float) return Power(x, (float)other);
            if ((object)(xi = other as ExtensibleInt) != null) return Power(x, xi.value);
            if ((object)(xf = other as ExtensibleFloat) != null) return Power(x, xf.value);
            if ((object)(xc = other as ExtensibleComplex) != null) return Power(x, xc.value);
            if ((object)(el = other as ExtensibleLong) != null) return Power(x, el.Value);
            if (other is byte) return Power(x, (int)((byte)other));
            return Ops.NotImplemented;
        }


        [PythonName("__mul__")]
        public static object Multiply(long x, object other) {
            BigInteger bi;
            ExtensibleInt xi;
            ExtensibleFloat xf;
            ExtensibleComplex xc;
            ExtensibleLong el;

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
            } else if ((object)(xi = other as ExtensibleInt) != null) {
                int y = xi.value;
                try {
                    return Ops.Long2Object(checked(x * y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) * y;
                }
            } else if ((object)(xf = other as ExtensibleFloat) != null) {
                return x * xf.value;
            } else if ((object)(xc = other as ExtensibleComplex) != null) {
                return Complex64.MakeReal(x) * xc.value;
            } else if ((object)(el = other as ExtensibleLong) != null) {
                return BigInteger.Create(x) * el.Value;
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
            ExtensibleInt xi;
            ExtensibleFloat xf;
            ExtensibleComplex xc;
            ExtensibleLong el;

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
            } else if ((object)(xi = other as ExtensibleInt) != null) {
                int y = xi.value;
                try {
                    return Ops.Long2Object(checked(y * x));
                } catch (OverflowException) {
                    return y * BigInteger.Create(x);
                }
            } else if ((object)(xf = other as ExtensibleFloat) != null) {
                return xf.value * x;
            } else if ((object)(xc = other as ExtensibleComplex) != null) {
                return xc.value * Complex64.MakeReal(x);
            } else if ((object)(el = other as ExtensibleLong) != null) {
                return el.Value * BigInteger.Create(x);
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
            ExtensibleInt xi;
            ExtensibleFloat xf;
            ExtensibleComplex xc;
            ExtensibleLong el;

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
            } else if ((object)(xi = other as ExtensibleInt) != null) {
                int y = xi.value;
                try {
                    return Ops.Long2Object(Divide(x, y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) / y;
                }
            } else if ((object)(xf = other as ExtensibleFloat) != null) {
                return FloatOps.Divide(x, xf.value);
            } else if ((object)(xc = other as ExtensibleComplex) != null) {
                return ComplexOps.Divide(Complex64.MakeReal(x), xc.value);
            } else if ((object)(el = other as ExtensibleLong) != null) {
                return LongOps.Divide(BigInteger.Create(x), el.Value);
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
            ExtensibleInt xi;
            ExtensibleFloat xf;
            ExtensibleComplex xc;
            ExtensibleLong el;

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
            } else if ((object)(xi = other as ExtensibleInt) != null) {
                int y = xi.value;
                try {
                    return Ops.Long2Object(ReverseDivide(x, y));
                } catch (OverflowException) {
                    return y / BigInteger.Create(x);
                }
            } else if ((object)(xf = other as ExtensibleFloat) != null) {
                return FloatOps.ReverseDivide(x, xf.value);
            } else if ((object)(xc = other as ExtensibleComplex) != null) {
                return ComplexOps.ReverseDivide(Complex64.MakeReal(x), xc.value);
            } else if ((object)(el = other as ExtensibleLong) != null) {
                return LongOps.ReverseDivide(BigInteger.Create(x), el.Value);
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
            ExtensibleInt xi;
            ExtensibleFloat xf;
            ExtensibleComplex xc;
            ExtensibleLong el;

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
            } else if ((object)(xi = other as ExtensibleInt) != null) {
                int y = xi.value;
                try {
                    return Ops.Long2Object(Divide(x, y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) / y;
                }
            } else if ((object)(xf = other as ExtensibleFloat) != null) {
                return FloatOps.FloorDivide(x, xf.value);
            } else if ((object)(xc = other as ExtensibleComplex) != null) {
                return ComplexOps.FloorDivide(Complex64.MakeReal(x), xc.value);
            } else if ((object)(el = other as ExtensibleLong) != null) {
                return LongOps.FloorDivide(BigInteger.Create(x), el.Value);
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
            ExtensibleInt xi;
            ExtensibleFloat xf;
            ExtensibleComplex xc;
            ExtensibleLong el;

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
            } else if ((object)(xi = other as ExtensibleInt) != null) {
                int y = xi.value;
                try {
                    return Ops.Long2Object(ReverseDivide(x, y));
                } catch (OverflowException) {
                    return y / BigInteger.Create(x);
                }
            } else if ((object)(xf = other as ExtensibleFloat) != null) {
                return FloatOps.ReverseFloorDivide(x, xf.value);
            } else if ((object)(xc = other as ExtensibleComplex) != null) {
                return ComplexOps.ReverseFloorDivide(Complex64.MakeReal(x), xc.value);
            } else if ((object)(el = other as ExtensibleLong) != null) {
                return LongOps.ReverseFloorDivide(BigInteger.Create(x), el.Value);
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
            ExtensibleInt xi;
            ExtensibleFloat xf;
            ExtensibleComplex xc;
            ExtensibleLong el;

            if (other is int) return TrueDivide(x, (int)other);
            if ((object)(bi = other as BigInteger) != null) return TrueDivide(x, bi);
            if (other is long) return TrueDivide(x, (long)other);
            if (other is double) return TrueDivide(x, (double)other);
            if (other is Complex64) return ComplexOps.TrueDivide(x, (Complex64)other);
            if (other is bool) return TrueDivide(x, (bool)other ? 1 : 0); 
            if (other is float) return TrueDivide(x, (float)other);
            if ((object)(xi = other as ExtensibleInt) != null) return TrueDivide(x, xi.value);
            if ((object)(xf = other as ExtensibleFloat) != null) return TrueDivide(x, xf.value);
            if ((object)(xc = other as ExtensibleComplex) != null) return TrueDivide(x, xc.value);
            if ((object)(el = other as ExtensibleLong) != null) return TrueDivide(x, el.Value);
            if (other is byte) return TrueDivide(x, (int)((byte)other));
            return Ops.NotImplemented;
        }


        [PythonName("__mod__")]
        public static object Mod(long x, object other) {
            BigInteger bi;
            ExtensibleInt xi;
            ExtensibleFloat xf;
            ExtensibleComplex xc;
            ExtensibleLong el;

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
            } else if ((object)(xi = other as ExtensibleInt) != null) {
                int y = xi.value;
                try {
                    return Ops.Long2Object(Mod(x, y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) % y;
                }
            } else if ((object)(xf = other as ExtensibleFloat) != null) {
                return FloatOps.Mod(x, xf.value);
            } else if ((object)(xc = other as ExtensibleComplex) != null) {
                return ComplexOps.Mod(Complex64.MakeReal(x), xc.value);
            } else if ((object)(el = other as ExtensibleLong) != null) {
                return LongOps.Mod(BigInteger.Create(x), el.Value);
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
            ExtensibleInt xi;
            ExtensibleFloat xf;
            ExtensibleComplex xc;
            ExtensibleLong el;

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
            } else if ((object)(xi = other as ExtensibleInt) != null) {
                int y = xi.value;
                try {
                    return Ops.Long2Object(ReverseMod(x, y));
                } catch (OverflowException) {
                    return y % BigInteger.Create(x);
                }
            } else if ((object)(xf = other as ExtensibleFloat) != null) {
                return FloatOps.ReverseMod(x, xf.value);
            } else if ((object)(xc = other as ExtensibleComplex) != null) {
                return ComplexOps.ReverseMod(Complex64.MakeReal(x), xc.value);
            } else if ((object)(el = other as ExtensibleLong) != null) {
                return LongOps.ReverseMod(BigInteger.Create(x), el.Value);
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
                long y = (long)xi.value;
                return Ops.Long2Object(x & y);
            } else if ((object)(el = other as ExtensibleLong) != null) {
                return BigInteger.Create(x) & el.Value;
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
                long y = (long)xi.value;
                return Ops.Long2Object(x | y);
            } else if ((object)(el = other as ExtensibleLong) != null) {
                return BigInteger.Create(x) | el.Value;
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
                long y = (long)xi.value;
                return Ops.Long2Object(x ^ y);
            } else if ((object)(el = other as ExtensibleLong) != null) {
                return BigInteger.Create(x) ^ el.Value;
            } else if (other is byte) {
                return Ops.Long2Object(x ^ (long)((byte)other));
            }
            return Ops.NotImplemented;
        }


        // *** END GENERATED CODE ***

        #endregion

    }
}
