/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Text;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

using Microsoft.Scripting;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;

using IronPython.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Types;
using IronPython.Runtime.Operations;

[assembly: PythonExtensionType(typeof(Complex64), typeof(ComplexOps), DerivationType=typeof(ExtensibleComplex))]
namespace IronPython.Runtime.Operations {
    public class ExtensibleComplex : Extensible<Complex64> {
        public ExtensibleComplex() : base() { }
        public ExtensibleComplex(double real) : base(Complex64.MakeReal(real)) { }
        public ExtensibleComplex(double real, double imag) : base(new Complex64(real, imag)) { }
    }

    public static partial class ComplexOps {
        [StaticExtensionMethod("__new__")]
        public static object Make(CodeContext context, PythonType cls) {
            if (cls == TypeCache.Complex64) return new Complex64();
            return cls.CreateInstance(context);
        }

        [StaticExtensionMethod("__new__")]
        public static object Make(
            CodeContext context, 
            PythonType cls,
            [DefaultParameterValueAttribute(null)]object real,
            [DefaultParameterValueAttribute(null)]object imag
           ) {
            Complex64 real2, imag2;
            real2 = imag2 = new Complex64();

            if (real == null && imag == null && cls == TypeCache.Complex64) throw PythonOps.TypeError("argument must be a string or a number");

            if (imag != null) {
                if (real is string) throw PythonOps.TypeError("complex() can't take second arg if first is a string");
                if (imag is string) throw PythonOps.TypeError("complex() second arg can't be a string");
                imag2 = Converter.ConvertToComplex64(imag);
            }

            if (real != null) {
                if (real is string)
                    real2 = LiteralParser.ParseComplex64((string)real);
                else if (real is Complex64) {
                    if (imag == null && cls == TypeCache.Complex64) return real;
                    else real2 = (Complex64)real;
                } else {
                    real2 = Converter.ConvertToComplex64(real);
                }
            }

            Complex64 c = real2 + imag2 * Complex64.MakeImaginary(1);
            if (cls == TypeCache.Complex64) {
                return new Complex64(c.Real, c.Imag);
            } else {
                return cls.CreateInstance(context, c.Real, c.Imag);
            }
        }

        [PropertyMethod, PythonName("real")]
        public static double GetReal(Complex64 self) {
            return self.Real;
        }

        [PropertyMethod, PythonName("imag")]
        public static double GetImaginary(Complex64 self) {
            return self.Imag;
        }

        #region Binary operators
        [SpecialName]
        public static Complex64 TrueDivide(Complex64 x, Complex64 y) {
            return x / y;
        }

        [SpecialName]
        public static Complex64 Power(Complex64 x, Complex64 y) {
            if (x.IsZero && (y.Real < 0.0 || y.Imag != 0.0))
                throw PythonOps.ZeroDivisionError("0.0 to a negative or complex power");
            return x.Power(y);
        }

        // floordiv for complex numbers is deprecated in the Python 2.4
        // specification; this function implements the observable
        // functionality in CPython 2.4: 
        //   Let x, y be complex.
        //   Re(x//y) := floor(Re(x/y))
        //   Im(x//y) := 0
        [SpecialName]
        public static Complex64 FloorDivide(Complex64 x, Complex64 y) {
            Complex64 quotient = x / y;
            return Complex64.MakeReal(PythonOps.CheckMath(Math.Floor(quotient.Real)));
        }

        // mod for complex numbers is also deprecated. IronPython
        // implements the CPython semantics, that is:
        // x % y = x - (y * (x//y)).
        [SpecialName]
        public static Complex64 Mod(Complex64 x, Complex64 y) {
            Complex64 quotient = FloorDivide(x, y);
            return x - (quotient * y);
        }

        [SpecialName, PythonName("__divmod__")]
        public static PythonTuple DivMod(Complex64 x, Complex64 y) {
            return PythonTuple.MakeTuple(FloorDivide(x, y), Mod(x, y));
        }

        #endregion

        #region Unary operators

        [SpecialName, PythonName("__hash__")]
        public static int GetHashCode(Complex64 x) {
            return x.GetHashCode();
        }

        [SpecialName, PythonName("__nonzero__")]
        public static bool ConvertToBoolean(Complex64 x) {
            return !x.IsZero;
        }

        [PythonName("conjugate")]
        public static Complex64 Conjugate(Complex64 x) {
            return x.Conjugate();
        }

        [PythonName("__getnewargs__")]
        public static object GetNewArgs(CodeContext context, Complex64 self) {
            if (!Object.ReferenceEquals(self, null)) {
                return PythonTuple.MakeTuple(
                    ComplexOps.Make(context,
                        TypeCache.Complex64,
                        PythonOps.GetBoundAttr(context, self, Symbols.RealPart),
                        PythonOps.GetBoundAttr(context, self, Symbols.ImaginaryPart)
                    )
                );
            }
            throw PythonOps.TypeErrorForBadInstance("__getnewargs__ requires a 'complex' object but received a '{0}'", self);
        }

        #endregion

        [PythonName("__coerce__")]
        public static object Coerce(Complex64 x, object y) {
            Complex64 right;
            if (Converter.TryConvertToComplex64(y, out right)) return PythonTuple.MakeTuple(x, right);

            if (y is BigInteger || y is Extensible<BigInteger>) throw PythonOps.OverflowError("long too large to convert");

            return PythonOps.NotImplemented;
        }

        [SpecialName, PythonName("__repr__")]
        public static string ToCodeRepresentation(Complex64 x) {
            if (x.Real != 0) {
                if (x.Imag >= 0) {
                    return "(" + x.Real.ToString("G") + "+" + x.Imag.ToString("G") + "j)";
                } else {
                    return "(" + x.Real.ToString("G") + x.Imag.ToString("G") + "j)";
                }
            }

            return x.Imag.ToString("G") + "j";
        }
        
        // Unary Operations
        [SpecialName]
        public static double Abs(Complex64 x) {
            return x.Abs();
        }

        // Binary Operations - Comparisons (eq & ne defined on Complex64 type as operators)

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "y"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "x"), SpecialName]
        public static bool LessThan(Complex64 x, Complex64 y) {
            throw PythonOps.TypeError("complex is not an ordered type");
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "y"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "x"), SpecialName]
        public static bool LessThanOrEqual(Complex64 x, Complex64 y) {
            throw PythonOps.TypeError("complex is not an ordered type");
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "x"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "y"), SpecialName]
        public static bool GreaterThan(Complex64 x, Complex64 y) {
            throw PythonOps.TypeError("complex is not an ordered type");
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "y"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "x"), SpecialName]
        public static bool GreaterThanOrEqual(Complex64 x, Complex64 y) {
            throw PythonOps.TypeError("complex is not an ordered type");
        }

    }
}
