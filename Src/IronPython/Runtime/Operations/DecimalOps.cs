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
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;

using IronPython.Runtime;
using IronPython.Runtime.Types;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Runtime;

using Microsoft.Scripting;
using Microsoft.Scripting.Math;

[assembly: PythonExtensionType(typeof(Decimal), typeof(DecimalOps))]
namespace IronPython.Runtime.Operations {
    public static class DecimalOps {

        [PythonName("__cmp__")]
        [return: MaybeNotImplemented]
        public static object Compare(CodeContext context, decimal x, object other) {
            return DoubleOps.Compare(context, (double)x, other);
        }

        [SpecialName]
        public static bool LessThan(decimal x, decimal y) {
            return x < y;
        }
        [SpecialName]
        public static bool LessThanOrEqual(decimal x, decimal y) {
            return x <= y;
        }
        [SpecialName]
        public static bool GreaterThan(decimal x, decimal y) {
            return x > y;
        }
        [SpecialName]
        public static bool GreaterThanOrEqual(decimal x, decimal y) {
            return x >= y;
        }
        [SpecialName]
        public static bool Equals(decimal x, decimal y) {
            return x == y;
        }
        [SpecialName]
        public static bool NotEquals(decimal x, decimal y) {
            return x != y;
        }

        internal static int Compare(BigInteger x, decimal y) {
            return -Compare(y, x);
        }

        internal static int Compare(decimal x, BigInteger y) {
            if (object.ReferenceEquals(y, null)) return +1;
            BigInteger bx = BigInteger.Create(x);
            if (bx == y) {
                decimal mod = x % 1;
                if (mod == 0) return 0;
                if (mod > 0) return +1;
                else return -1;
            }
            return bx > y ? +1 : -1;
        }
    }
}
