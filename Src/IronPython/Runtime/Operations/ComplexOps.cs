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
using System.Text;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;

using IronMath;
using IronPython.Runtime;
using IronPython.Runtime.Types;

namespace IronPython.Runtime.Operations {
    public partial class ExtensibleComplex : IRichComparable, IExtensible<Complex64> {
        public Complex64 value;
        
        public ExtensibleComplex() { this.value = new Complex64(0, 0); }
        public ExtensibleComplex(double real) { value = new Complex64(real); }
        public ExtensibleComplex(double real, double imag) {
            value = new Complex64(real, imag);
        }
        
        public override string ToString() {
            return value.ToString();
        }

        public override bool Equals(object obj) {
            return value.Equals(obj);
        }

        public override int GetHashCode() {
            return value.GetHashCode();
        }

        public virtual object ReversePower(object other) {
            return ComplexOps.ReversePower(value, other);
        }

        public virtual object Power(object other) {
            return ComplexOps.Power(value, other);
        }

        #region IExtensible<Complex64> Members

        public Complex64 Value {
            get { return value; }
        }

        #endregion

        #region IRichComparable Members

        [PythonName("__cmp__")]
        public virtual object CompareTo(object other) {
            return ComplexOps.Compare(value, other);
        }

        [PythonName("__gt__")]
        public virtual object GreaterThan(object other) {
            return ComplexOps.GreaterThan(value, other);
        }

        [PythonName("__lt__")]
        public virtual object LessThan(object other) {
            return ComplexOps.LessThan(value, other);
        }

        [PythonName("__ge__")]
        public virtual object GreaterThanOrEqual(object other) {
            return ComplexOps.GreaterThanEquals(value, other);
        }

        [PythonName("__le__")]
        public virtual object LessThanOrEqual(object other) {
            return ComplexOps.LessThanEquals(value, other);
        }

        #endregion

        #region IRichEquality Members

        [PythonName("__hash__")]
        public virtual object RichGetHashCode() {
            return ComplexOps.GetHashCode(value);
        }

        [PythonName("__eq__")]
        public virtual object RichEquals(object other) {
            return ComplexOps.Equals(value, other);
        }

        [PythonName("__ne__")]
        public virtual object RichNotEquals(object other) {
            object res = RichEquals(other);
            if (res != Ops.NotImplemented) return Ops.Not(res);

            return Ops.NotImplemented;
        }

        #endregion

        internal static object TrueCompare(object x, object y) {
            Debug.Assert(x is ExtensibleComplex || y is ExtensibleComplex);

            object res;
            if (x is ExtensibleComplex) {
                // Was __cmp__ overriden ?
                if (Ops.TryToInvoke(x, SymbolTable.Cmp, out res)) {
                    return res;
                }
                // if not, compare to the complex value
                return ComplexOps.TrueCompare(((ExtensibleComplex)x).value, y);
            } else {
                Debug.Assert(y is ExtensibleComplex);
                // Was __cmp__ overriden ?
                if (Ops.TryToInvoke(y, SymbolTable.Cmp, out res)) {
                    return res;
                }
                // if not, compare to the complex value
                return ComplexOps.TrueCompare(x, ((ExtensibleComplex)y).value);
            }
        }
    }

    public static partial class ComplexOps {
        static ReflectedType ComplexType;
        public static ReflectedType MakeDynamicType() {
            if (ComplexType == null) {
                ReflectedType res = new OpsReflectedType("complex", typeof(Complex64), typeof(ComplexOps), typeof(ExtensibleComplex));
                if (System.Threading.Interlocked.CompareExchange<ReflectedType>(ref ComplexType, res, null) == null)
                    return res;
            }
            return ComplexType;
        }

        [PythonName("__new__")]
        public static object Make(
            PythonType cls,
            [DefaultParameterValueAttribute(null)]object real,
            [DefaultParameterValueAttribute(null)]object imag
           ) {
            Conversion conv;
            Complex64 real2, imag2;
            real2 = imag2 = new Complex64();

            if (real == null && imag == null && cls == ComplexType) throw Ops.TypeError("argument must be a string or a number");

            if (imag != null) {
                if (real is string) throw Ops.TypeError("complex() can't take second arg if first is a string");
                if (imag is string) throw Ops.TypeError("complex() second arg can't be a string");
                imag2 = Converter.TryConvertToComplex64(imag, out conv);
                if (conv == Conversion.None) {
                    if (imag is BigInteger || imag is ExtensibleLong)
                        throw Ops.OverflowError("long too large to convert to float");

                    throw Ops.TypeError("complex() argument must be a string or a number");
                }
            }

            if (real != null) {
                if (real is string)
                    real2 = LiteralParser.ParseComplex64((string)real);
                else if (real is Complex64) {
                    if (imag == null && cls == ComplexType) return real;
                    else real2 = (Complex64)real;                
                } else {
                    real2 = Converter.TryConvertToComplex64(real, out conv);
                    if (conv == Conversion.None) {
                        if (real is BigInteger || real is ExtensibleLong)
                            throw Ops.OverflowError("long too large to convert to float");

                        throw Ops.TypeError("complex() argument must be a string or a number");
                    }
                }
            }

            Complex64 c = real2 + imag2 * Complex64.MakeImaginary(1);
            if (cls == ComplexType) {
                return new Complex64(c.real, c.imag);
            } else {
                return cls.ctor.Call(cls, c.real, c.imag);
            }
        }

        private static object ReversePower(Complex64 x, Complex64 y){
            return Power(y, x);
        }

        private static object TrueDivide(Complex64 x, Complex64 y) {
            return x / y;
        }

        private static object ReverseTrueDivide(Complex64 x, Complex64 y) {
            return y / x;
        }

        [PythonName("__abs__")]
        public static object Abs(Complex64 x) {
            return x.Abs();
        }

        // #Power
        private static object Power(Complex64 x, Complex64 y) {
            if (x.IsZero && (y.Real < 0.0 || y.Imag != 0.0))
                throw Ops.ZeroDivisionError("0.0 to a negative or complex power");
            return x.Power(y);
        }

        [PythonName("__div__")]
        public static object Divide(Complex64 x, object other) {
            if (other is Complex64) {
                return TrueDivide(x, (Complex64)other);
            } else {
                return TrueDivide(x, other);
            }
        }

        [PythonName("__rdiv__")]
        public static object ReverseDivide(Complex64 x, object other) {
            return ReverseTrueDivide(x, other);
        }

        // floordiv for complex numbers is deprecated in the Python 2.4
        // specification; this function implements the observable
        // functionality in CPython 2.4: 
        //   Let x, y be complex.
        //   Re(x//y) := floor(Re(x/y))
        //   Im(x//y) := 0
        [PythonName("__floordiv__")]
        public static object FloorDivide(Complex64 x, object other) {
            object rawQuotient = TrueDivide(x, other);
            if (rawQuotient == Ops.NotImplemented) {
                return rawQuotient;
            }
            if (rawQuotient is Complex64) {
                Complex64 rawQComplex = (Complex64)rawQuotient;
                return new Complex64(Modules.PythonMath.Floor(rawQComplex.Real),
                    0);
            } else { // quotient was not complex
                return new Complex64(Converter.ConvertToDouble(rawQuotient), 0);
            }
        }

        [PythonName("__rfloordiv__")]
        public static object ReverseFloorDivide(Complex64 x, object other) {
            object rawQuotient = ReverseTrueDivide(x, other);
            if (rawQuotient == Ops.NotImplemented) {
                return rawQuotient;
            }
            if (rawQuotient is Complex64) {
                Complex64 rawQComplex = (Complex64)rawQuotient;
                return new Complex64(Modules.PythonMath.Floor(rawQComplex.Real),
                    0);
            } else { // quotient was not complex
                return new Complex64(Converter.ConvertToDouble(rawQuotient), 0);
            }
        }

        // mod for complex numbers is also deprecated. IronPython
        // implements the CPython semantics, that is:
        // x % y = x - (y * (x//y)).
        [PythonName("__mod__")]
        public static object Mod(Complex64 x, object other) {
            object rawQuotient = ComplexOps.FloorDivide(x, other);
            if (rawQuotient == Ops.NotImplemented) {
                return rawQuotient;
            }
            if (rawQuotient is Complex64) {
                Complex64 complexQuotient = (Complex64)rawQuotient;
                Complex64 product = (Complex64)ComplexOps.Multiply(complexQuotient, other);
                return ComplexOps.Subtract(x, product);
            } else {
                return Ops.NotImplemented;
            }
        }
        // other % x = other - (x * (other // y ))
        [PythonName("__rmod__")]
        public static object ReverseMod(Complex64 x, object other) {
            object rawQuotient = ComplexOps.ReverseFloorDivide(x, other);
            if (rawQuotient == Ops.NotImplemented) {
                return rawQuotient;
            }
            if (rawQuotient is Complex64) {
                Complex64 complexQuotient = (Complex64)rawQuotient;
                Complex64 product = (Complex64)ComplexOps.ReverseMultiply(complexQuotient, x);
                return ComplexOps.ReverseSubtract(product, other);
            } else {
                return Ops.NotImplemented;
            }
        }

        [PythonName("__eq__")]
        public static object Equals(Complex64 x, object other) {
            bool res;
            if (TryEquals(x, other, out res)) return Ops.Bool2Object(res);

            return Ops.NotImplemented;
        }

        [PythonName("__gt__")]
        public static object GreaterThan(Complex64 self, object other) {
            if (other == null) return Ops.NotImplemented;
            return Compare(self, other) > 0;
        }

        [PythonName("__lt__")]
        public static object LessThan(Complex64 self, object other) {
            if (other == null) return Ops.NotImplemented;
            return Compare(self, other) < 0;
        }

        [PythonName("__ge__")]
        public static object GreaterThanEquals(Complex64 self, object other) {
            if (other == null) return Ops.NotImplemented;
            return Compare(self, other) >= 0;
        }

        [PythonName("__le__")]
        public static object LessThanEquals(Complex64 self, object other) {
            if (other == null) return Ops.NotImplemented;
            return Compare(self, other) <= 0;
        }

        [PythonName("__coerce__")]
        public static object Coerce(object x, object y) {
            if (!(x is Complex64)) throw Ops.TypeError("__coerce__ requires a complex object, but got {0}", Ops.StringRepr(Ops.GetDynamicType(x)));
            Conversion conv;
            Complex64 right = Converter.TryConvertToComplex64(y, out conv);
            if(conv != Conversion.None) return Tuple.MakeTuple(x, right);

            if (y is BigInteger || y is ExtensibleLong) throw Ops.OverflowError("long too large to convert");

            return Ops.NotImplemented;
        }

        [PythonName("__hash__")]
        public static int GetHashCode(Complex64 x) {
            return x.GetHashCode();
        }

        public static bool EqualsRetBool(Complex64 x, object other) {
            bool res;
            if (TryEquals(x, other, out res)) return res;
            
            return Ops.DynamicEqualRetBool(x, other);
        }

        private static bool TryEquals(Complex64 x, object other, out bool res) {
            if (other is int) {
                res = x == (int)other;
                return true;
            } else if (other is double) {
                res = x == (double)other;
                return true;
            } else if (other is BigInteger) {
                res = x == (BigInteger)other;
                return true;
            } else if (other is Complex64) {
                res = x == (Complex64)other;
                return true;
            } else if (other is ExtensibleComplex) {
                res = x == ((ExtensibleComplex)other).value;
                return true;
            } else if (other is bool) {
                res = (bool)other ? x == 1 : x == 0;
                return true;
            }

            Conversion conversion;
            Complex64 y = Converter.TryConvertToComplex64(other, out conversion);
            if (conversion != Conversion.None) {
                res = x == y;
                return true;
            }

            object ores = Ops.GetDynamicType(other).Coerce(other, x);
            if (ores != Ops.NotImplemented && !(ores is OldInstance)) {
                res = Ops.EqualRetBool(((Tuple)ores)[1], ((Tuple)ores)[0]);
                return true;
            }

            res = false;
            return false;
        }

        /// <summary>
        /// Used when user calls cmp(x,y) versus x > y, if the values are the same we return 0.
        /// </summary>
        public static int TrueCompare(object x, object y) {
            // Complex vs. null is 1 (when complex is on the lhs)
            // Complex vs. another type is -1 (when complex is on the lhs)
            // If two complex values are equal we return 0
            // Otherwise we throw because it's an un-ordered comparison
            if (x is Complex64) {
                Complex64 us = (Complex64)x;

                // Complex vs null, 1
                if (y == null) return 1;

                // Compex vs Complex, if they're equal we return 0, otherwize we throw
                Complex64 them = new Complex64();
                bool haveOther = false;
                if (y is Complex64) {
                    them = (Complex64)y;
                    haveOther = true;
                } else if (y is ExtensibleComplex) {
                    them = ((ExtensibleComplex)y).value;
                    haveOther = true;
                } else {
                    object res = Ops.GetDynamicType(y).Coerce(y, x);
                    if (res != Ops.NotImplemented && !(res is OldInstance)) {
                        return Ops.Compare(((Tuple)res)[1], ((Tuple)res)[0]);
                    }
                }


                if (haveOther) {
                    if (us.imag == them.imag && us.real == them.real) return 0;
                    throw Ops.TypeError("complex is not an ordered type");
                }

                // Complex vs user type, check what the user type says
                object ret = Ops.GetDynamicType(y).CompareTo(y, x);
                if (ret != Ops.NotImplemented) {
                    return ((int)ret) * -1;
                }

                // Otherwise all types are less than complex
                return -1;
            } else {
                System.Diagnostics.Debug.Assert(y is Complex64);
                return -1 * TrueCompare(y, x);
            }
        }

        /// <summary>
        /// Used for >, <, >=, <= where we don't allow ordered comparisons.
        /// </summary>
        internal static int Compare(Complex64 x, object y) {
            // Complex vs. another type is -1 (when complex is on the lhs)
            // Complex vs. null is 1 (when complex is on the lhs)
            // If two complex values are equal we return 0
            // Otherwise we throw because it's an un-ordered comparison            

            Debug.Assert(y != null);
            if (y is Complex64 || y is ExtensibleComplex) throw Ops.TypeError("complex is not an ordered type");

            object ret = Ops.GetDynamicType(y).CompareTo(y, x);
            if (ret != Ops.NotImplemented) {
                return ((int)ret) * -1;
            }

            return -1;
        }

        [PythonName("__neg__")]
        public static object Negate(Complex64 x) {
            return -x;
        }

        [PythonName("__pos__")]
        public static object Positive(Complex64 x) {
            return x;
        }

        [PythonName("conjugate")]
        public static Complex64 Conjugate(Complex64 x) {
            return x.Conjugate();
        }

        internal static object DivMod(Complex64 x, Complex64 y) {
            object div = FloorDivide(x, y);
            if (div == Ops.NotImplemented) return div;
            object mod = Mod(x, y);
            if (mod == Ops.NotImplemented) return mod;
            return Tuple.MakeTuple(div, mod);
        }

        internal static object ReverseDivMod(Complex64 x, Complex64 y) {
            return DivMod(y, x);
        }

        public static object imag = new OpsReflectedField<Complex64, ExtensibleComplex>(typeof(Complex64).GetField("imag"), NameType.PythonField);
        public static object real = new OpsReflectedField<Complex64, ExtensibleComplex>(typeof(Complex64).GetField("real"), NameType.PythonField);

    }
}
