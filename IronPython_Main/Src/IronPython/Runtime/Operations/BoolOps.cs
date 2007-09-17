/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Text;
using System.Collections;
using System.Threading;
using SpecialNameAttribute = System.Runtime.CompilerServices.SpecialNameAttribute;

using IronPython.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Types;
using IronPython.Runtime.Operations;

using Microsoft.Scripting;
using Microsoft.Scripting.Math;

[assembly: PythonExtensionType(typeof(bool), typeof(BoolOps))]

namespace IronPython.Runtime.Operations {

    public static class BoolOps {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "cls"), StaticExtensionMethod("__new__")]
        public static object Make(object cls) {
            return RuntimeHelpers.False;
        }

        [StaticExtensionMethod("__new__")]
        public static bool Make(object cls, object o) {
            return PythonOps.IsTrue(o);
        }

        [SpecialName]
        public static bool BitwiseAnd(bool x, bool y) {
            return (bool)(x & y);
        }
        
        [SpecialName]
        public static bool BitwiseOr(bool x, bool y) {
            return (bool)(x | y);
        }

        [SpecialName]
        public static bool ExclusiveOr(bool x, bool y) {
            return (bool)(x ^ y);
        }

        [SpecialName]
        public static int BitwiseAnd(int x, bool y) {
            return Int32Ops.BitwiseAnd(y ? 1 : 0, x);
        }

        [SpecialName]
        public static int BitwiseAnd(bool x, int y) {
            return Int32Ops.BitwiseAnd(x ? 1 : 0, y);
        }

        [SpecialName]
        public static int BitwiseOr(int x, bool y) {
            return Int32Ops.BitwiseOr(y ? 1 : 0, x);
        }

        [SpecialName]
        public static int BitwiseOr(bool x, int y) {
            return Int32Ops.BitwiseOr(x ? 1 : 0, y);
        }

        [SpecialName]
        public static int ExclusiveOr(int x, bool y) {
            return Int32Ops.ExclusiveOr(y ? 1 : 0, x);
        }

        [SpecialName]
        public static int ExclusiveOr(bool x, int y) {
            return Int32Ops.ExclusiveOr(x ? 1 : 0, y);
        }

        [SpecialName, PythonName("__repr__")]
        public static string CodeRepresentation(bool self) {
            return self ? "True" : "False";
        }

        // Binary Operations - Comparisons
        [SpecialName]
        public static bool Equals(bool x, bool y) {
            return x == y;
        }
        [SpecialName]
        public static bool NotEquals(bool x, bool y) {
            return x != y;
        }
        [SpecialName]
        public static bool Equals(bool x, int y) {
            return (x ? 1 : 0) == y;
        }
        [SpecialName]
        public static bool NotEquals(bool x, int y) {
            return (x ? 1 : 0) != y;
        }
        [SpecialName]
        public static bool Equals(int x, bool y) {
            return Equals(y, x);
        }
        [SpecialName]
        public static bool NotEquals(int x, bool y) {
            return NotEquals(y, x);
        }

        // Conversion operators
        [ImplicitConversionMethod]
        public static SByte ConvertToSByte(Boolean x) {
            return (SByte)(x ? 1 : 0);
        }
        [ImplicitConversionMethod]
        public static Byte ConvertToByte(Boolean x) {
            return (Byte)(x ? 1 : 0);
        }
        [ImplicitConversionMethod]
        public static Int16 ConvertToInt16(Boolean x) {
            return (Int16)(x ? 1 : 0);
        }
        [ImplicitConversionMethod]
        public static UInt16 ConvertToUInt16(Boolean x) {
            return (UInt16)(x ? 1 : 0);
        }

        [SpecialName, PythonName("__int__")]
        public static Int32 ConvertToInt(Boolean x) {
            return (Int32)(x ? 1 : 0);
        }

        [ImplicitConversionMethod]
        public static Int32 ConvertToInt32(Boolean x) {
            return (Int32)(x ? 1 : 0);
        }
        [ImplicitConversionMethod]
        public static UInt32 ConvertToUInt32(Boolean x) {
            return (UInt32)(x ? 1 : 0);
        }
        [ImplicitConversionMethod]
        public static Int64 ConvertToInt64(Boolean x) {
            return (Int64)(x ? 1 : 0);
        }
        [ImplicitConversionMethod]
        public static UInt64 ConvertToUInt64(Boolean x) {
            return (UInt64)(x ? 1 : 0);
        }
        [ImplicitConversionMethod]
        public static Single ConvertToSingle(Boolean x) {
            return (Single)(x ? 1 : 0);
        }
        [ImplicitConversionMethod]
        public static Double ConvertToDouble(Boolean x) {
            return (Double)(x ? 1 : 0);
        }
    }
}
