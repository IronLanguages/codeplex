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
using Microsoft.Scripting.Internal;
using Microsoft.Scripting;

namespace IronPythonTest {
    public class Cmplx {
        private double r;
        private double i;

        public Cmplx()
            : this(0, 0) {
        }

        public Cmplx(double r)
            : this(r, 0) {
        }

        public Cmplx(double r, double i) {
            this.r = r;
            this.i = i;
        }

        public override int GetHashCode() {
            return r.GetHashCode() ^ i.GetHashCode();
        }

        public override bool Equals(object obj) {
            if (obj is Cmplx) {
                Cmplx o = (Cmplx)obj;
                return o.r == r && o.i == i;
            } else if (obj is IConvertible) {
                double o = ((IConvertible)obj).ToDouble(null);
                return o == r && i == 0;
            }
            return false;
        }

        public override string ToString() {
            return String.Format("({0} + {1}i)", r, i);
        }

        public double Real {
            get {
                return r;
            }
        }

        public double Imag {
            get {
                return i;
            }
        }

        public static Cmplx operator *(double x, Cmplx y) {
            return new Cmplx(x * y.r, x * y.i);
        }
        public static Cmplx operator *(Cmplx x, double y) {
            return new Cmplx(x.r * y, x.i * y);
        }
        public static Cmplx operator *(Cmplx x, Cmplx y) {
            return new Cmplx(x.r * y.r - x.i * y.i, x.r * y.i + x.i * y.r);
        }
        public static Cmplx operator /(double x, Cmplx y) {
            return new Cmplx(x) / y;
        }
        public static Cmplx operator /(Cmplx x, double y) {
            return new Cmplx(x.r / y, x.i / y);
        }
        public static Cmplx operator /(Cmplx x, Cmplx y) {
            double div = y.r * y.r + y.i * y.i;
            return new Cmplx((x.r * y.r + x.i * y.i) / div, (x.i * y.r - x.r * y.i) / div);
        }
        public static Cmplx operator +(double x, Cmplx y) {
            return new Cmplx(x + y.r, y.i);
        }
        public static Cmplx operator +(Cmplx x, double y) {
            return new Cmplx(x.r + y, x.i);
        }
        public static Cmplx operator +(Cmplx x, Cmplx y) {
            return new Cmplx(x.r + y.r, x.i + y.i);
        }
        public static Cmplx operator -(double x, Cmplx y) {
            return new Cmplx(x - y.r, -y.i);
        }
        public static Cmplx operator -(Cmplx x, double y) {
            return new Cmplx(x.r - y, x.i);
        }
        public static Cmplx operator -(Cmplx x, Cmplx y) {
            return new Cmplx(x.r - y.r, x.i - y.i);
        }
        public static Cmplx operator -(Cmplx x) {
            return new Cmplx(-x.r, -x.i);
        }

        [System.Runtime.CompilerServices.SpecialName, DlrSpecialNameAttribute]
        public static Cmplx op_MultiplicationAssignment(Cmplx x, double y) {
            x.r *= y;
            x.i *= y;
            return x;
        }
        [System.Runtime.CompilerServices.SpecialName, DlrSpecialNameAttribute]
        public static Cmplx op_MultiplicationAssignment(Cmplx x, Cmplx y) {
            double r = x.r * y.r - x.i * y.i;
            double i = x.r * y.i + x.i * y.r;
            x.r = r;
            x.i = i;
            return x;
        }
        [System.Runtime.CompilerServices.SpecialName, DlrSpecialNameAttribute]
        public static Cmplx op_SubtractionAssignment(Cmplx x, double y) {
            x.r -= y;
            return x;
        }
        [System.Runtime.CompilerServices.SpecialName, DlrSpecialNameAttribute]
        public static Cmplx op_SubtractionAssignment(Cmplx x, Cmplx y) {
            x.r -= y.r;
            x.i -= y.i;
            return x;
        }
        [System.Runtime.CompilerServices.SpecialName, DlrSpecialNameAttribute]
        public static Cmplx op_AdditionAssignment(Cmplx x, double y) {
            x.r += y;
            return x;
        }
        [System.Runtime.CompilerServices.SpecialName, DlrSpecialNameAttribute]
        public static Cmplx op_AdditionAssignment(Cmplx x, Cmplx y) {
            x.r += y.r;
            x.i += y.i;
            return x;
        }
        [System.Runtime.CompilerServices.SpecialName, DlrSpecialNameAttribute]
        public static Cmplx op_DivisionAssignment(Cmplx x, double y) {
            x.r /= y;
            x.i /= y;
            return x;
        }
        [System.Runtime.CompilerServices.SpecialName, DlrSpecialNameAttribute]
        public static Cmplx op_DivisionAssignment(Cmplx x, Cmplx y) {
            double div = y.r * y.r + y.i * y.i;
            double r = (x.r * y.r + x.i * y.i) / div;
            double i = (x.i * y.r - x.r * y.i) / div;
            x.r = r;
            x.i = i;
            return x;
        }
    }

    public class Cmplx2 {
        private double r;
        private double i;

        public Cmplx2()
            : this(0, 0) {
        }

        public Cmplx2(double r)
            : this(r, 0) {
        }

        public Cmplx2(double r, double i) {
            this.r = r;
            this.i = i;
        }

        public static Cmplx2 operator +(Cmplx y, Cmplx2 x) {
            return new Cmplx2(x.r + y.Real, x.i + y.Imag);
        }

        public static Cmplx2 operator +(Cmplx2 x, Cmplx y) {
            return new Cmplx2(x.r + y.Real, x.i + y.Imag);
        }
    }
}
