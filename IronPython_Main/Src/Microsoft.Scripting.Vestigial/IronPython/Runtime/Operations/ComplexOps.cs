/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
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

using Microsoft.Scripting;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Internal;

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
        [PythonName("real")]
        public static DynamicTypeSlot Real = AddComplexProperty(Symbols.RealPart, "Real");
        [PythonName("imag")]
        public static DynamicTypeSlot Imag = AddComplexProperty(Symbols.ImaginaryPart, "Imag");

        private static DynamicTypeSlot AddComplexProperty(SymbolId symbol, string name) {
            System.Reflection.PropertyInfo pi = typeof(Complex64).GetProperty(name);

            return new OpsReflectedProperty<Complex64, Extensible<Complex64>>(pi, NameType.PythonProperty);
        }

        [StaticOpsMethod("__new__")]
        public static object Make(CodeContext context, DynamicType cls) {
            if (cls == TypeCache.Complex64) return new Complex64();
            return cls.CreateInstance(context);
        }

        [StaticOpsMethod("__new__")]
        public static object Make(
            CodeContext context, 
            DynamicType cls,
            [DefaultParameterValueAttribute(null)]object real,
            [DefaultParameterValueAttribute(null)]object imag
           ) {
            Complex64 real2, imag2;
            real2 = imag2 = new Complex64();

            if (real == null && imag == null && cls == TypeCache.Complex64) throw Ops.TypeError("argument must be a string or a number");

            if (imag != null) {
                if (real is string) throw Ops.TypeError("complex() can't take second arg if first is a string");
                if (imag is string) throw Ops.TypeError("complex() second arg can't be a string");
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

        #region Binary operators
        [OperatorMethod]
        public static Complex64 TrueDivide(Complex64 x, Complex64 y) {
            return x / y;
        }

        [OperatorMethod]
        public static Complex64 Power(Complex64 x, Complex64 y) {
            if (x.IsZero && (y.Real < 0.0 || y.Imag != 0.0))
                throw Ops.ZeroDivisionError("0.0 to a negative or complex power");
            return x.Power(y);
        }

        // floordiv for complex numbers is deprecated in the Python 2.4
        // specification; this function implements the observable
        // functionality in CPython 2.4: 
        //   Let x, y be complex.
        //   Re(x//y) := floor(Re(x/y))
        //   Im(x//y) := 0
        [OperatorMethod]
        public static Complex64 FloorDivide(Complex64 x, Complex64 y) {
            Complex64 quotient = x / y;
            return Complex64.MakeReal(Ops.CheckMath(Math.Floor(quotient.Real)));
        }

        // mod for complex numbers is also deprecated. IronPython
        // implements the CPython semantics, that is:
        // x % y = x - (y * (x//y)).
        [OperatorMethod]
        public static Complex64 Mod(Complex64 x, Complex64 y) {
            Complex64 quotient = FloorDivide(x, y);
            return x - (quotient * y);
        }

        #endregion

        internal static object DivMod(Complex64 x, Complex64 y) {
            return Tuple.MakeTuple(x / y, Mod(x, y));
        }


        #region Unary operators

        [OperatorMethod, PythonName("__hash__")]
        public static int GetHashCode(Complex64 x) {
            return x.GetHashCode();
        }

        [OperatorMethod, PythonName("__nonzero__")]
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
                return Tuple.MakeTuple(
                    ComplexOps.Make(context,
                        TypeCache.Complex64,
                        Ops.GetBoundAttr(context, self, Symbols.RealPart),
                        Ops.GetBoundAttr(context, self, Symbols.ImaginaryPart)
                    )
                );
            }
            throw Ops.TypeErrorForBadInstance("__getnewargs__ requires a 'complex' object but received a '{0}'", self);
        }

        #endregion

        [OperatorMethod, PythonName("__coerce__")]
        public static object Coerce(object x, object y) {
            if (!(x is Complex64)) throw Ops.TypeError("__coerce__ requires a complex object, but got {0}", Ops.StringRepr(Ops.GetDynamicType(x)));
            Complex64 right;
            if (Converter.TryConvertToComplex64(y, out right)) return Tuple.MakeTuple(x, right);

            if (y is BigInteger || y is Extensible<BigInteger>) throw Ops.OverflowError("long too large to convert");

            return Ops.NotImplemented;
        }

        [OperatorMethod, PythonName("__repr__")]
        public static string ToCodeRepresentation(Complex64 x) {
            if (x.Real != 0) {
                return "(" + x.Real.ToString("G") + "+" + x.Imag.ToString("G") + "j)";
            }

            return x.Imag.ToString("G") + "j";
        }


        /// <summary>
        /// Used when user calls cmp(x,y) versus x > y, if the values are the same we return 0.
        /// </summary>
        public static int TrueCompare(CodeContext context, object x, object y) {
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
                } else if (y is Extensible<Complex64>) {
                    them = ((Extensible<Complex64>)y).Value;
                    haveOther = true;
                } else {
                    object res;

                    if (Ops.GetDynamicType(y).TryInvokeBinaryOperator(context, Operators.Coerce, y, x, out res)) {
                        if (res != Ops.NotImplemented && !(res is OldInstance)) {
                            return Ops.Compare(((Tuple)res)[1], ((Tuple)res)[0]);
                        }
                    }
                }


                if (haveOther) {
                    if (us.Imag == them.Imag && us.Real == them.Real) return 0;
                    throw Ops.TypeError("complex is not an ordered type");
                }

                // Complex vs user type, check what the user type says
                object ret;
                if (Ops.GetDynamicType(y).TryInvokeBinaryOperator(context,
                    Operators.Compare,
                    y,
                    x,
                    out ret) && ret != Ops.NotImplemented) {
                    return ((int)ret) * -1;
                }

                // Otherwise all types are less than complex
                return -1;
            } else {
                System.Diagnostics.Debug.Assert(y is Complex64);
                return -1 * TrueCompare(context, y, x);
            }
        }

        // Unary Operations
        [OperatorMethod]
        public static double Abs(Complex64 x) {
            return x.Abs();
        }

        // Binary Operations - Comparisons (eq & ne defined on Complex64 type as operators)

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "y"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "x"), OperatorMethod]
        public static bool LessThan(Complex64 x, Complex64 y) {
            throw Ops.TypeError("complex is not an ordered type");
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "y"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "x"), OperatorMethod]
        public static bool LessThanOrEqual(Complex64 x, Complex64 y) {
            throw Ops.TypeError("complex is not an ordered type");
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "x"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "y"), OperatorMethod]
        public static bool GreaterThan(Complex64 x, Complex64 y) {
            throw Ops.TypeError("complex is not an ordered type");
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "y"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "x"), OperatorMethod]
        public static bool GreaterThanOrEqual(Complex64 x, Complex64 y) {
            throw Ops.TypeError("complex is not an ordered type");
        }

    }
}
