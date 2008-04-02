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
using Microsoft.Scripting.Math;

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "Microsoft.Scripting.Ast")]

namespace Microsoft.Scripting.Ast {
    public static partial class Utils {
        public static Expression Constant(object value) {
            BigInteger bi = value as BigInteger;

            if ((object)bi != null) {
                return BigIntegerConstant(bi);
            } else if (value is Complex64) {
                return ComplexConstant((Complex64)value);
            } else {
                return Ast.Constant(value);
            }
        }

        private static Expression BigIntegerConstant(BigInteger value) {
            int ival;
            if (value.AsInt32(out ival)) {
                return Ast.Call(
                    typeof(BigInteger).GetMethod("Create", new Type[] { typeof(int) }),
                    Ast.Constant(ival)
                );
            }

            long lval;
            if (value.AsInt64(out lval)) {
                return Ast.Call(
                    typeof(BigInteger).GetMethod("Create", new Type[] { typeof(long) }),
                    Ast.Constant(lval)
                );
            }

            return Ast.New(
                typeof(BigInteger).GetConstructor(new Type[] { typeof(int), typeof(uint[]) }),
                Ast.Constant((int)value.Sign),
                CreateUIntArray(value.GetBits())
            );
        }

        private static Expression CreateUIntArray(uint[] array) {
            Expression[] init = new Expression[array.Length];
            for (int i = 0; i < init.Length; i++) {
                init[i] = Ast.Constant(array[i]);
            }
            return Ast.NewArray(typeof(uint[]), init);
        }

        private static Expression ComplexConstant(Complex64 value) {
            if (value.Real != 0.0) {
                if (value.Imag != 0.0) {
                    return Ast.Call(
                        typeof(Complex64).GetMethod("Make"),
                        Ast.Constant(value.Real),
                        Ast.Constant(value.Imag)
                    );
                } else {
                    return Ast.Call(
                        typeof(Complex64).GetMethod("MakeReal"),
                        Ast.Constant(value.Real)
                    );
                }
            } else {
                return Ast.Call(
                    typeof(Complex64).GetMethod("MakeImaginary"),
                    Ast.Constant(value.Imag)
                );
            }
        }
    }
}
