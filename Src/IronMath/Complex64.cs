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
using System.Diagnostics;
using System.Text;
using System.Collections;

namespace IronMath {
    /// <summary>
    /// arbitrary precision integers
    /// </summary>
    public struct Complex64 {
        public readonly double real, imag;

        public static Complex64 MakeImaginary(double imag) {
            return new Complex64(0.0, imag);
        }

        public static Complex64 MakeReal(double real) {
            return new Complex64(real, 0.0);
        }

        public Complex64(double real)
            : this(real, 0.0) {
        }

        public Complex64(double real, double imag) {
            this.real = real;
            this.imag = imag;
        }

        public bool IsZero {
            get {
                return real == 0.0 && imag == 0.0;
            }
        }

        public double Real {
            get {
                return real;
            }
        }

        public double Imag {
            get {
                return imag;
            }
        }

        public Complex64 Conjugate() {
            return new Complex64(real, -imag);
        }


        public override string ToString() {
            if (real == 0.0) return imag.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + "j";
            else if (imag < 0.0) return string.Format(System.Globalization.CultureInfo.InvariantCulture.NumberFormat, "({0}{1}j)", real, imag);
            else return string.Format(System.Globalization.CultureInfo.InvariantCulture.NumberFormat, "({0}+{1}j)", real, imag);
        }

        public static implicit operator Complex64(double d) {
            return MakeReal(d);
        }

        public static implicit operator Complex64(BigInteger i) {
            if (object.ReferenceEquals(i, null)) {
                throw new ArgumentException(IronMath.ResourceManager.GetString("InvalidArgument"));
            }

            // throws an overflow exception if we can't handle the value.
            return MakeReal(i.ToFloat64());
        }

        public static bool operator ==(Complex64 x, Complex64 y) {
            return x.real == y.real && x.imag == y.imag;
        }

        public static bool operator !=(Complex64 x, Complex64 y) {
            return x.real != y.real || x.imag != y.imag;
        }

        public static Complex64 Add(Complex64 x, Complex64 y) {
            return x + y;
        }

        public static Complex64 operator +(Complex64 x, Complex64 y) {
            return new Complex64(x.real + y.real, x.imag + y.imag);
        }

        public static Complex64 Subtract(Complex64 x, Complex64 y) {
            return x - y;
        }

        public static Complex64 operator -(Complex64 x, Complex64 y) {
            return new Complex64(x.real - y.real, x.imag - y.imag);
        }

        public static Complex64 Multiply(Complex64 x, Complex64 y) {
            return x * y;
        }

        public static Complex64 operator *(Complex64 x, Complex64 y) {
            return new Complex64(x.real * y.real - x.imag * y.imag, x.real * y.imag + x.imag * y.real);
        }

        public static Complex64 Divide(Complex64 x, Complex64 y) {
            return x / y;
        }

        public static Complex64 operator /(Complex64 a, Complex64 b) {
            if (b.IsZero) throw new DivideByZeroException("complex division");

            double real, imag, den, r;

            if (Math.Abs(b.real) >= Math.Abs(b.imag)) {
                r = b.imag / b.real;
                den = b.real + r * b.imag;
                real = (a.real + a.imag * r) / den;
                imag = (a.imag - a.real * r) / den;
            } else {
                r = b.real / b.imag;
                den = b.imag + r * b.real;
                real = (a.real * r + a.imag) / den;
                imag = (a.imag * r - a.real) / den;
            }

            return new Complex64(real, imag);
        }

        public static Complex64 Modulus(Complex64 x, Complex64 y) {
            return x % y;
        }

        public static Complex64 operator %(Complex64 x, Complex64 y) {
            if (object.ReferenceEquals(x, null)) {
                throw new ArgumentException(IronMath.ResourceManager.GetString("InvalidArgument"), "x");
            }
            if (object.ReferenceEquals(y, null)) {
                throw new ArgumentException(IronMath.ResourceManager.GetString("InvalidArgument"), "y");
            }

            if (y == 0) throw new DivideByZeroException();

            throw new NotImplementedException();
        }

        public static Complex64 Negate(Complex64 x) {
            return -x;
        }

        public static Complex64 operator -(Complex64 x) {
            return new Complex64(-x.real, -x.imag);
        }

        public static double Hypot(double x, double y) {
            //
            // sqrt(x*x + y*y) == sqrt(x*x * (1 + (y*y)/(x*x))) ==
            // sqrt(x*x) * sqrt(1 + (y/x)*(y/x)) ==
            // abs(x) * sqrt(1 + (y/x)*(y/x))
            //

            //  First, get abs
            if (x < 0.0) x = -x;
            if (y < 0.0) y = -y;

            // Obvious cases
            if (x == 0.0) return y;
            if (y == 0.0) return x;

            // Divide smaller number by bigger number to safeguard the (y/x)*(y/x)
            if (x < y) { double temp = y; y = x; x = temp; }

            y /= x;

            // calculate abs(x) * sqrt(1 + (y/x)*(y/x))
            return x * Math.Sqrt(1 + y * y);
        }

        public double Abs() {
            return Hypot(real, imag);
        }

        public Complex64 Power(Complex64 y) {
            double c = y.real;
            double d = y.imag;
            int power = (int)c;

            if (power == c && power >= 0 && d == .0) {
                Complex64 result = new Complex64(1.0);
                if (power == 0) return result;
                Complex64 factor = this;
                while (power != 0) {
                    if ((power & 1) != 0) {
                        result = result * factor;
                    }
                    factor = factor * factor;
                    power >>= 1;
                }
                return result;
            } else if (IsZero) {
                return y.IsZero ? Complex64.MakeReal(1.0) : Complex64.MakeReal(0.0);
            } else {
                double a = real;
                double b = imag;
                double powers = a * a + b * b;
                double arg = Math.Atan2(b, a);
                double mul = Math.Pow(powers, c / 2) * Math.Pow(Math.E, -d * arg);
                double common = c * arg + .5 * d * Math.Log(powers, Math.E);

                return new Complex64(mul * Math.Cos(common), mul * Math.Sin(common));
            }
        }

        public override int GetHashCode() {
            return (int)real + (int)imag * 1000003;
        }

        public override bool Equals(object obj) {
            if (!(obj is Complex64)) return false;
            return this == ((Complex64)obj);
        }
    }
}