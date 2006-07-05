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
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;

using IronMath;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

namespace IronPython.Runtime {
    public static partial class Converter {
        #region Generated conversion helpers

        // *** BEGIN GENERATED CODE ***


        ///<summary>
        ///Conversion routine TryConvertToByte - converts object to Byte
        ///</summary>
        public static bool TryConvertToByte(object value, out Byte result) {
            try {
                result = ConvertToByte(value);
                return true;
            } catch {
                result = default(Byte);
                return false;
            }
        }

        ///<summary>
        ///Conversion routine TryConvertToSByte - converts object to SByte
        ///</summary>
        public static bool TryConvertToSByte(object value, out SByte result) {
            try {
                result = ConvertToSByte(value);
                return true;
            } catch {
                result = default(SByte);
                return false;
            }
        }

        ///<summary>
        ///Conversion routine TryConvertToInt16 - converts object to Int16
        ///</summary>
        public static bool TryConvertToInt16(object value, out Int16 result) {
            try {
                result = ConvertToInt16(value);
                return true;
            } catch {
                result = default(Int16);
                return false;
            }
        }

        ///<summary>
        ///Conversion routine TryConvertToInt32 - converts object to Int32
        ///</summary>
        public static bool TryConvertToInt32(object value, out Int32 result) {
            try {
                result = ConvertToInt32(value);
                return true;
            } catch {
                result = default(Int32);
                return false;
            }
        }

        ///<summary>
        ///Conversion routine TryConvertToInt64 - converts object to Int64
        ///</summary>
        public static bool TryConvertToInt64(object value, out Int64 result) {
            try {
                result = ConvertToInt64(value);
                return true;
            } catch {
                result = default(Int64);
                return false;
            }
        }

        ///<summary>
        ///Conversion routine TryConvertToUInt16 - converts object to UInt16
        ///</summary>
        public static bool TryConvertToUInt16(object value, out UInt16 result) {
            try {
                result = ConvertToUInt16(value);
                return true;
            } catch {
                result = default(UInt16);
                return false;
            }
        }

        ///<summary>
        ///Conversion routine TryConvertToUInt32 - converts object to UInt32
        ///</summary>
        public static bool TryConvertToUInt32(object value, out UInt32 result) {
            try {
                result = ConvertToUInt32(value);
                return true;
            } catch {
                result = default(UInt32);
                return false;
            }
        }

        ///<summary>
        ///Conversion routine TryConvertToUInt64 - converts object to UInt64
        ///</summary>
        public static bool TryConvertToUInt64(object value, out UInt64 result) {
            try {
                result = ConvertToUInt64(value);
                return true;
            } catch {
                result = default(UInt64);
                return false;
            }
        }

        ///<summary>
        ///Conversion routine TryConvertToSingle - converts object to Single
        ///</summary>
        public static bool TryConvertToSingle(object value, out Single result) {
            try {
                result = ConvertToSingle(value);
                return true;
            } catch {
                result = default(Single);
                return false;
            }
        }

        ///<summary>
        ///Conversion routine TryConvertToDouble - converts object to Double
        ///</summary>
        public static bool TryConvertToDouble(object value, out Double result) {
            try {
                result = ConvertToDouble(value);
                return true;
            } catch {
                result = default(Double);
                return false;
            }
        }

        ///<summary>
        ///Conversion routine TryConvertToDecimal - converts object to Decimal
        ///</summary>
        public static bool TryConvertToDecimal(object value, out Decimal result) {
            try {
                result = ConvertToDecimal(value);
                return true;
            } catch {
                result = default(Decimal);
                return false;
            }
        }

        ///<summary>
        ///Conversion routine TryConvertToBigInteger - converts object to BigInteger
        ///</summary>
        public static bool TryConvertToBigInteger(object value, out BigInteger result) {
            try {
                result = ConvertToBigInteger(value);
                return true;
            } catch {
                result = default(BigInteger);
                return false;
            }
        }

        ///<summary>
        ///Conversion routine TryConvertToComplex64 - converts object to Complex64
        ///</summary>
        public static bool TryConvertToComplex64(object value, out Complex64 result) {
            try {
                result = ConvertToComplex64(value);
                return true;
            } catch {
                result = default(Complex64);
                return false;
            }
        }

        ///<summary>
        ///Conversion routine TryConvertToString - converts object to String
        ///</summary>
        public static bool TryConvertToString(object value, out String result) {
            try {
                result = ConvertToString(value);
                return true;
            } catch {
                result = default(String);
                return false;
            }
        }

        ///<summary>
        ///Conversion routine TryConvertToChar - converts object to Char
        ///</summary>
        public static bool TryConvertToChar(object value, out Char result) {
            try {
                result = ConvertToChar(value);
                return true;
            } catch {
                result = default(Char);
                return false;
            }
        }

        ///<summary>
        ///Conversion routine TryConvertToBoolean - converts object to Boolean
        ///</summary>
        public static bool TryConvertToBoolean(object value, out Boolean result) {
            try {
                result = ConvertToBoolean(value);
                return true;
            } catch {
                result = default(Boolean);
                return false;
            }
        }

        ///<summary>
        ///Conversion routine TryConvertToType - converts object to Type
        ///</summary>
        public static bool TryConvertToType(object value, out Type result) {
            try {
                result = ConvertToType(value);
                return true;
            } catch {
                result = default(Type);
                return false;
            }
        }

        ///<summary>
        ///Conversion routine TryConvertToIEnumerator - converts object to IEnumerator
        ///</summary>
        public static bool TryConvertToIEnumerator(object value, out IEnumerator result) {
            try {
                result = ConvertToIEnumerator(value);
                return true;
            } catch {
                result = default(IEnumerator);
                return false;
            }
        }

        // *** END GENERATED CODE ***

        #endregion

        #region Generated explicit enum conversion

        // *** BEGIN GENERATED CODE ***

        ///<summary>
        /// Explicit conversion of Enum to Int32
        ///</summary>
        internal static Int32 CastEnumToInt32(object value) {
            Debug.Assert(value is Enum);
            switch (((Enum)value).GetTypeCode()) {
                case TypeCode.Int32:
                    return (Int32)(Int32)value;
                case TypeCode.Byte:
                    return (Int32)(Byte)value;
                case TypeCode.SByte:
                    return (Int32)(SByte)value;
                case TypeCode.Int16:
                    return (Int32)(Int16)value;
                case TypeCode.Int64:
                    return (Int32)(Int64)value;
                case TypeCode.UInt16:
                    return (Int32)(UInt16)value;
                case TypeCode.UInt32:
                    return (Int32)(UInt32)value;
                case TypeCode.UInt64:
                    return (Int32)(UInt64)value;
            }
            // Should never get here
            Debug.Fail("Invalid enum detected");
            return default(Int32);
        }
        ///<summary>
        /// Explicit conversion of Enum to Byte
        ///</summary>
        internal static Byte CastEnumToByte(object value) {
            Debug.Assert(value is Enum);
            switch (((Enum)value).GetTypeCode()) {
                case TypeCode.Int32:
                    return (Byte)(Int32)value;
                case TypeCode.Byte:
                    return (Byte)(Byte)value;
                case TypeCode.SByte:
                    return (Byte)(SByte)value;
                case TypeCode.Int16:
                    return (Byte)(Int16)value;
                case TypeCode.Int64:
                    return (Byte)(Int64)value;
                case TypeCode.UInt16:
                    return (Byte)(UInt16)value;
                case TypeCode.UInt32:
                    return (Byte)(UInt32)value;
                case TypeCode.UInt64:
                    return (Byte)(UInt64)value;
            }
            // Should never get here
            Debug.Fail("Invalid enum detected");
            return default(Byte);
        }
        ///<summary>
        /// Explicit conversion of Enum to SByte
        ///</summary>
        internal static SByte CastEnumToSByte(object value) {
            Debug.Assert(value is Enum);
            switch (((Enum)value).GetTypeCode()) {
                case TypeCode.Int32:
                    return (SByte)(Int32)value;
                case TypeCode.Byte:
                    return (SByte)(Byte)value;
                case TypeCode.SByte:
                    return (SByte)(SByte)value;
                case TypeCode.Int16:
                    return (SByte)(Int16)value;
                case TypeCode.Int64:
                    return (SByte)(Int64)value;
                case TypeCode.UInt16:
                    return (SByte)(UInt16)value;
                case TypeCode.UInt32:
                    return (SByte)(UInt32)value;
                case TypeCode.UInt64:
                    return (SByte)(UInt64)value;
            }
            // Should never get here
            Debug.Fail("Invalid enum detected");
            return default(SByte);
        }
        ///<summary>
        /// Explicit conversion of Enum to Int16
        ///</summary>
        internal static Int16 CastEnumToInt16(object value) {
            Debug.Assert(value is Enum);
            switch (((Enum)value).GetTypeCode()) {
                case TypeCode.Int32:
                    return (Int16)(Int32)value;
                case TypeCode.Byte:
                    return (Int16)(Byte)value;
                case TypeCode.SByte:
                    return (Int16)(SByte)value;
                case TypeCode.Int16:
                    return (Int16)(Int16)value;
                case TypeCode.Int64:
                    return (Int16)(Int64)value;
                case TypeCode.UInt16:
                    return (Int16)(UInt16)value;
                case TypeCode.UInt32:
                    return (Int16)(UInt32)value;
                case TypeCode.UInt64:
                    return (Int16)(UInt64)value;
            }
            // Should never get here
            Debug.Fail("Invalid enum detected");
            return default(Int16);
        }
        ///<summary>
        /// Explicit conversion of Enum to Int64
        ///</summary>
        internal static Int64 CastEnumToInt64(object value) {
            Debug.Assert(value is Enum);
            switch (((Enum)value).GetTypeCode()) {
                case TypeCode.Int32:
                    return (Int64)(Int32)value;
                case TypeCode.Byte:
                    return (Int64)(Byte)value;
                case TypeCode.SByte:
                    return (Int64)(SByte)value;
                case TypeCode.Int16:
                    return (Int64)(Int16)value;
                case TypeCode.Int64:
                    return (Int64)(Int64)value;
                case TypeCode.UInt16:
                    return (Int64)(UInt16)value;
                case TypeCode.UInt32:
                    return (Int64)(UInt32)value;
                case TypeCode.UInt64:
                    return (Int64)(UInt64)value;
            }
            // Should never get here
            Debug.Fail("Invalid enum detected");
            return default(Int64);
        }
        ///<summary>
        /// Explicit conversion of Enum to UInt16
        ///</summary>
        internal static UInt16 CastEnumToUInt16(object value) {
            Debug.Assert(value is Enum);
            switch (((Enum)value).GetTypeCode()) {
                case TypeCode.Int32:
                    return (UInt16)(Int32)value;
                case TypeCode.Byte:
                    return (UInt16)(Byte)value;
                case TypeCode.SByte:
                    return (UInt16)(SByte)value;
                case TypeCode.Int16:
                    return (UInt16)(Int16)value;
                case TypeCode.Int64:
                    return (UInt16)(Int64)value;
                case TypeCode.UInt16:
                    return (UInt16)(UInt16)value;
                case TypeCode.UInt32:
                    return (UInt16)(UInt32)value;
                case TypeCode.UInt64:
                    return (UInt16)(UInt64)value;
            }
            // Should never get here
            Debug.Fail("Invalid enum detected");
            return default(UInt16);
        }
        ///<summary>
        /// Explicit conversion of Enum to UInt32
        ///</summary>
        internal static UInt32 CastEnumToUInt32(object value) {
            Debug.Assert(value is Enum);
            switch (((Enum)value).GetTypeCode()) {
                case TypeCode.Int32:
                    return (UInt32)(Int32)value;
                case TypeCode.Byte:
                    return (UInt32)(Byte)value;
                case TypeCode.SByte:
                    return (UInt32)(SByte)value;
                case TypeCode.Int16:
                    return (UInt32)(Int16)value;
                case TypeCode.Int64:
                    return (UInt32)(Int64)value;
                case TypeCode.UInt16:
                    return (UInt32)(UInt16)value;
                case TypeCode.UInt32:
                    return (UInt32)(UInt32)value;
                case TypeCode.UInt64:
                    return (UInt32)(UInt64)value;
            }
            // Should never get here
            Debug.Fail("Invalid enum detected");
            return default(UInt32);
        }
        ///<summary>
        /// Explicit conversion of Enum to UInt64
        ///</summary>
        internal static UInt64 CastEnumToUInt64(object value) {
            Debug.Assert(value is Enum);
            switch (((Enum)value).GetTypeCode()) {
                case TypeCode.Int32:
                    return (UInt64)(Int32)value;
                case TypeCode.Byte:
                    return (UInt64)(Byte)value;
                case TypeCode.SByte:
                    return (UInt64)(SByte)value;
                case TypeCode.Int16:
                    return (UInt64)(Int16)value;
                case TypeCode.Int64:
                    return (UInt64)(Int64)value;
                case TypeCode.UInt16:
                    return (UInt64)(UInt16)value;
                case TypeCode.UInt32:
                    return (UInt64)(UInt32)value;
                case TypeCode.UInt64:
                    return (UInt64)(UInt64)value;
            }
            // Should never get here
            Debug.Fail("Invalid enum detected");
            return default(UInt64);
        }
        internal static Boolean CastEnumToBoolean(object value) {
            Debug.Assert(value is Enum);
            switch (((Enum)value).GetTypeCode()) {
                case TypeCode.Int32:
                    return (Int32)value != 0;
                case TypeCode.Byte:
                    return (Byte)value != 0;
                case TypeCode.SByte:
                    return (SByte)value != 0;
                case TypeCode.Int16:
                    return (Int16)value != 0;
                case TypeCode.Int64:
                    return (Int64)value != 0;
                case TypeCode.UInt16:
                    return (UInt16)value != 0;
                case TypeCode.UInt32:
                    return (UInt32)value != 0;
                case TypeCode.UInt64:
                    return (UInt64)value != 0;
            }
            // Should never get here
            Debug.Fail("Invalid enum detected");
            return default(Boolean);
        }

        // *** END GENERATED CODE ***

        #endregion

        #region Generated conversion implementations

        // *** BEGIN GENERATED CODE ***

        ///<summary>
        /// ConvertToByte Conversion Routine. If no conversion exists, returns false. Can throw OverflowException.
        ///</summary>
        private static bool ConvertToByteImpl(object value, out Byte result) {
            if (value is Int32) {
                result = checked((Byte)(Int32)value); return true;
            } else if (value is Boolean) {
                result = (Boolean)value ? (Byte)1 : (Byte)0; return true;
            } else if (value is BigInteger) {
                UInt32 UInt32Value = ((BigInteger)value).ToUInt32();
                result = checked((Byte)UInt32Value); return true;
            } else if (value is ExtensibleInt) {
                result = checked((Byte)(Int32)((ExtensibleInt)value).value); return true;
            } else if (value is ExtensibleLong) {
                UInt32 UInt32Value = ((BigInteger)((ExtensibleLong)value).Value).ToUInt32();
                result = checked((Byte)UInt32Value); return true;
            } else if (value is Int64) {
                result = checked((Byte)(Int64)value); return true;
            } else if (value is Byte) {
                result = (Byte)value; return true;
            } else if (value is SByte) {
                result = checked((Byte)(SByte)value); return true;
            } else if (value is Int16) {
                result = checked((Byte)(Int16)value); return true;
            } else if (value is UInt16) {
                result = checked((Byte)(UInt16)value); return true;
            } else if (value is UInt32) {
                result = checked((Byte)(UInt32)value); return true;
            } else if (value is UInt64) {
                result = checked((Byte)(UInt64)value); return true;
            } else if (value is Decimal) {
                result = checked((Byte)(Decimal)value); return true;
            }
            result = default(Byte);
            return false;
        }
        ///<summary>
        /// ConvertToSByte Conversion Routine. If no conversion exists, returns false. Can throw OverflowException.
        ///</summary>
        private static bool ConvertToSByteImpl(object value, out SByte result) {
            if (value is Int32) {
                result = checked((SByte)(Int32)value); return true;
            } else if (value is Boolean) {
                result = (Boolean)value ? (SByte)1 : (SByte)0; return true;
            } else if (value is BigInteger) {
                Int32 Int32Value = ((BigInteger)value).ToInt32();
                result = checked((SByte)Int32Value); return true;
            } else if (value is ExtensibleInt) {
                result = checked((SByte)(Int32)((ExtensibleInt)value).value); return true;
            } else if (value is ExtensibleLong) {
                Int32 Int32Value = ((BigInteger)((ExtensibleLong)value).Value).ToInt32();
                result = checked((SByte)Int32Value); return true;
            } else if (value is Int64) {
                result = checked((SByte)(Int64)value); return true;
            } else if (value is Byte) {
                result = checked((SByte)(Byte)value); return true;
            } else if (value is SByte) {
                result = (SByte)value; return true;
            } else if (value is Int16) {
                result = checked((SByte)(Int16)value); return true;
            } else if (value is UInt16) {
                result = checked((SByte)(UInt16)value); return true;
            } else if (value is UInt32) {
                result = checked((SByte)(UInt32)value); return true;
            } else if (value is UInt64) {
                result = checked((SByte)(UInt64)value); return true;
            } else if (value is Decimal) {
                result = checked((SByte)(Decimal)value); return true;
            }
            result = default(SByte);
            return false;
        }
        ///<summary>
        /// ConvertToInt16 Conversion Routine. If no conversion exists, returns false. Can throw OverflowException.
        ///</summary>
        private static bool ConvertToInt16Impl(object value, out Int16 result) {
            if (value is Int32) {
                result = checked((Int16)(Int32)value); return true;
            } else if (value is Boolean) {
                result = (Boolean)value ? (Int16)1 : (Int16)0; return true;
            } else if (value is BigInteger) {
                Int32 Int32Value = ((BigInteger)value).ToInt32();
                result = checked((Int16)Int32Value); return true;
            } else if (value is ExtensibleInt) {
                result = checked((Int16)(Int32)((ExtensibleInt)value).value); return true;
            } else if (value is ExtensibleLong) {
                Int32 Int32Value = ((BigInteger)((ExtensibleLong)value).Value).ToInt32();
                result = checked((Int16)Int32Value); return true;
            } else if (value is Int64) {
                result = checked((Int16)(Int64)value); return true;
            } else if (value is Byte) {
                result = (Int16)(Byte)value; return true;
            } else if (value is SByte) {
                result = (Int16)(SByte)value; return true;
            } else if (value is Int16) {
                result = (Int16)value; return true;
            } else if (value is UInt16) {
                result = checked((Int16)(UInt16)value); return true;
            } else if (value is UInt32) {
                result = checked((Int16)(UInt32)value); return true;
            } else if (value is UInt64) {
                result = checked((Int16)(UInt64)value); return true;
            } else if (value is Decimal) {
                result = checked((Int16)(Decimal)value); return true;
            }
            result = default(Int16);
            return false;
        }
        ///<summary>
        /// ConvertToUInt16 Conversion Routine. If no conversion exists, returns false. Can throw OverflowException.
        ///</summary>
        private static bool ConvertToUInt16Impl(object value, out UInt16 result) {
            if (value is Int32) {
                result = checked((UInt16)(Int32)value); return true;
            } else if (value is Boolean) {
                result = (Boolean)value ? (UInt16)1 : (UInt16)0; return true;
            } else if (value is BigInteger) {
                UInt32 UInt32Value = ((BigInteger)value).ToUInt32();
                result = checked((UInt16)UInt32Value); return true;
            } else if (value is ExtensibleInt) {
                result = checked((UInt16)(Int32)((ExtensibleInt)value).value); return true;
            } else if (value is ExtensibleLong) {
                UInt32 UInt32Value = ((BigInteger)((ExtensibleLong)value).Value).ToUInt32();
                result = checked((UInt16)UInt32Value); return true;
            } else if (value is Int64) {
                result = checked((UInt16)(Int64)value); return true;
            } else if (value is Byte) {
                result = (UInt16)(Byte)value; return true;
            } else if (value is SByte) {
                result = checked((UInt16)(SByte)value); return true;
            } else if (value is Int16) {
                result = checked((UInt16)(Int16)value); return true;
            } else if (value is UInt16) {
                result = (UInt16)value; return true;
            } else if (value is UInt32) {
                result = checked((UInt16)(UInt32)value); return true;
            } else if (value is UInt64) {
                result = checked((UInt16)(UInt64)value); return true;
            } else if (value is Decimal) {
                result = checked((UInt16)(Decimal)value); return true;
            }
            result = default(UInt16);
            return false;
        }
        ///<summary>
        /// ConvertToInt32 Conversion Routine. If no conversion exists, returns false. Can throw OverflowException.
        ///</summary>
        private static bool ConvertToInt32Impl(object value, out Int32 result) {
            if (value is Int32) {
                result = (Int32)value; return true;
            } else if (value is Boolean) {
                result = (Boolean)value ? (Int32)1 : (Int32)0; return true;
            } else if (value is BigInteger) {
                result = ((BigInteger)value).ToInt32(); return true;
            } else if (value is Double) {
                // DEPRECATED IMPLICIT CONVERSION FROM FLOAT TO INT
                result = checked((Int32)(Double)value); return true;
            } else if (value is ExtensibleInt) {
                result = (Int32)(Int32)((ExtensibleInt)value).value; return true;
            } else if (value is ExtensibleLong) {
                result = ((BigInteger)((ExtensibleLong)value).Value).ToInt32(); return true;
            } else if (value is ExtensibleFloat) {
                // DEPRECATED IMPLICIT CONVERSION FROM FLOAT TO INT
                result = checked((Int32)(Double)((ExtensibleFloat)value).value); return true;
            } else if (value is Int64) {
                result = checked((Int32)(Int64)value); return true;
            } else if (value is Byte) {
                result = (Int32)(Byte)value; return true;
            } else if (value is SByte) {
                result = (Int32)(SByte)value; return true;
            } else if (value is Int16) {
                result = (Int32)(Int16)value; return true;
            } else if (value is UInt16) {
                result = (Int32)(UInt16)value; return true;
            } else if (value is UInt32) {
                result = checked((Int32)(UInt32)value); return true;
            } else if (value is UInt64) {
                result = checked((Int32)(UInt64)value); return true;
            } else if (value is Decimal) {
                result = checked((Int32)(Decimal)value); return true;
            }
            result = default(Int32);
            return false;
        }
        ///<summary>
        /// ConvertToUInt32 Conversion Routine. If no conversion exists, returns false. Can throw OverflowException.
        ///</summary>
        private static bool ConvertToUInt32Impl(object value, out UInt32 result) {
            if (value is Int32) {
                result = checked((UInt32)(Int32)value); return true;
            } else if (value is Boolean) {
                result = (Boolean)value ? (UInt32)1 : (UInt32)0; return true;
            } else if (value is BigInteger) {
                result = ((BigInteger)value).ToUInt32(); return true;
            } else if (value is ExtensibleInt) {
                result = checked((UInt32)(Int32)((ExtensibleInt)value).value); return true;
            } else if (value is ExtensibleLong) {
                result = ((BigInteger)((ExtensibleLong)value).Value).ToUInt32(); return true;
            } else if (value is Int64) {
                result = checked((UInt32)(Int64)value); return true;
            } else if (value is Byte) {
                result = (UInt32)(Byte)value; return true;
            } else if (value is SByte) {
                result = checked((UInt32)(SByte)value); return true;
            } else if (value is Int16) {
                result = checked((UInt32)(Int16)value); return true;
            } else if (value is UInt16) {
                result = (UInt32)(UInt16)value; return true;
            } else if (value is UInt32) {
                result = (UInt32)value; return true;
            } else if (value is UInt64) {
                result = checked((UInt32)(UInt64)value); return true;
            } else if (value is Decimal) {
                result = checked((UInt32)(Decimal)value); return true;
            }
            result = default(UInt32);
            return false;
        }
        ///<summary>
        /// ConvertToInt64 Conversion Routine. If no conversion exists, returns false. Can throw OverflowException.
        ///</summary>
        private static bool ConvertToInt64Impl(object value, out Int64 result) {
            if (value is Int32) {
                result = (Int64)(Int32)value; return true;
            } else if (value is Boolean) {
                result = (Boolean)value ? (Int64)1 : (Int64)0; return true;
            } else if (value is BigInteger) {
                result = ((BigInteger)value).ToInt64(); return true;
            } else if (value is ExtensibleInt) {
                result = (Int64)(Int32)((ExtensibleInt)value).value; return true;
            } else if (value is ExtensibleLong) {
                result = ((BigInteger)((ExtensibleLong)value).Value).ToInt64(); return true;
            } else if (value is Int64) {
                result = (Int64)value; return true;
            } else if (value is Byte) {
                result = (Int64)(Byte)value; return true;
            } else if (value is SByte) {
                result = (Int64)(SByte)value; return true;
            } else if (value is Int16) {
                result = (Int64)(Int16)value; return true;
            } else if (value is UInt16) {
                result = (Int64)(UInt16)value; return true;
            } else if (value is UInt32) {
                result = (Int64)(UInt32)value; return true;
            } else if (value is UInt64) {
                result = checked((Int64)(UInt64)value); return true;
            } else if (value is Decimal) {
                result = checked((Int64)(Decimal)value); return true;
            }
            result = default(Int64);
            return false;
        }
        ///<summary>
        /// ConvertToUInt64 Conversion Routine. If no conversion exists, returns false. Can throw OverflowException.
        ///</summary>
        private static bool ConvertToUInt64Impl(object value, out UInt64 result) {
            if (value is Int32) {
                result = checked((UInt64)(Int32)value); return true;
            } else if (value is Boolean) {
                result = (Boolean)value ? (UInt64)1 : (UInt64)0; return true;
            } else if (value is BigInteger) {
                result = ((BigInteger)value).ToUInt64(); return true;
            } else if (value is ExtensibleInt) {
                result = checked((UInt64)(Int32)((ExtensibleInt)value).value); return true;
            } else if (value is ExtensibleLong) {
                result = ((BigInteger)((ExtensibleLong)value).Value).ToUInt64(); return true;
            } else if (value is Int64) {
                result = checked((UInt64)(Int64)value); return true;
            } else if (value is Byte) {
                result = (UInt64)(Byte)value; return true;
            } else if (value is SByte) {
                result = checked((UInt64)(SByte)value); return true;
            } else if (value is Int16) {
                result = checked((UInt64)(Int16)value); return true;
            } else if (value is UInt16) {
                result = (UInt64)(UInt16)value; return true;
            } else if (value is UInt32) {
                result = (UInt64)(UInt32)value; return true;
            } else if (value is UInt64) {
                result = (UInt64)value; return true;
            } else if (value is Decimal) {
                result = checked((UInt64)(Decimal)value); return true;
            }
            result = default(UInt64);
            return false;
        }
        ///<summary>
        /// ConvertToSingle Conversion Routine. If no conversion exists, returns false. Can throw OverflowException.
        ///</summary>
        private static bool ConvertToSingleImpl(object value, out Single result) {
            if (value is Int32) {
                result = (Single)(Int32)value; return true;
            } else if (value is Boolean) {
                result = (Boolean)value ? (Single)1 : (Single)0; return true;
            } else if (value is BigInteger) {
                Double DoubleValue = ((BigInteger)value).ToFloat64();
                result = checked((Single)DoubleValue); return true;
            } else if (value is Double) {
                result = checked((Single)(Double)value);
                if (Single.IsInfinity(result)) throw Ops.OverflowError("{0} won't fit into Single", value);
                return true;
            } else if (value is ExtensibleInt) {
                result = (Single)(Int32)((ExtensibleInt)value).value; return true;
            } else if (value is ExtensibleLong) {
                Double DoubleValue = ((BigInteger)((ExtensibleLong)value).Value).ToFloat64();
                result = checked((Single)DoubleValue); return true;
            } else if (value is ExtensibleFloat) {
                result = checked((Single)(Double)((ExtensibleFloat)value).value);
                if (Single.IsInfinity(result)) throw Ops.OverflowError("{0} won't fit into Single", ((ExtensibleFloat)value).value);
                return true;
            } else if (value is Int64) {
                result = (Single)(Int64)value; return true;
            } else if (value is Byte) {
                result = (Single)(Byte)value; return true;
            } else if (value is SByte) {
                result = (Single)(SByte)value; return true;
            } else if (value is Int16) {
                result = (Single)(Int16)value; return true;
            } else if (value is UInt16) {
                result = (Single)(UInt16)value; return true;
            } else if (value is UInt32) {
                result = (Single)(UInt32)value; return true;
            } else if (value is UInt64) {
                result = (Single)(UInt64)value; return true;
            } else if (value is Single) {
                result = (Single)value; return true;
            } else if (value is Decimal) {
                result = (Single)(Decimal)value; return true;
            }
            result = default(Single);
            return false;
        }
        ///<summary>
        /// ConvertToDouble Conversion Routine. If no conversion exists, returns false. Can throw OverflowException.
        ///</summary>
        private static bool ConvertToDoubleImpl(object value, out Double result) {
            if (value is Int32) {
                result = (Double)(Int32)value; return true;
            } else if (value is Boolean) {
                result = (Boolean)value ? (Double)1 : (Double)0; return true;
            } else if (value is BigInteger) {
                result = ((BigInteger)value).ToFloat64(); return true;
            } else if (value is Double) {
                result = (Double)value; return true;
            } else if (value is ExtensibleInt) {
                result = (Double)(Int32)((ExtensibleInt)value).value; return true;
            } else if (value is ExtensibleLong) {
                result = ((BigInteger)((ExtensibleLong)value).Value).ToFloat64(); return true;
            } else if (value is ExtensibleFloat) {
                result = (Double)(Double)((ExtensibleFloat)value).value; return true;
            } else if (value is Int64) {
                result = (Double)(Int64)value; return true;
            } else if (value is Byte) {
                result = (Double)(Byte)value; return true;
            } else if (value is SByte) {
                result = (Double)(SByte)value; return true;
            } else if (value is Int16) {
                result = (Double)(Int16)value; return true;
            } else if (value is UInt16) {
                result = (Double)(UInt16)value; return true;
            } else if (value is UInt32) {
                result = (Double)(UInt32)value; return true;
            } else if (value is UInt64) {
                result = (Double)(UInt64)value; return true;
            } else if (value is Single) {
                result = (Double)(Single)value; return true;
            } else if (value is Decimal) {
                result = (Double)(Decimal)value; return true;
            }
            result = default(Double);
            return false;
        }
        ///<summary>
        /// ConvertToDecimal Conversion Routine. If no conversion exists, returns false. Can throw OverflowException.
        ///</summary>
        private static bool ConvertToDecimalImpl(object value, out Decimal result) {
            if (value is Int32) {
                result = (Decimal)(Int32)value; return true;
            } else if (value is Boolean) {
                result = (Boolean)value ? (Decimal)1 : (Decimal)0; return true;
            } else if (value is BigInteger) {
                result = ((BigInteger)value).ToDecimal(); return true;
            } else if (value is Double) {
                result = checked((Decimal)(Double)value); return true;
            } else if (value is ExtensibleInt) {
                result = (Decimal)(Int32)((ExtensibleInt)value).value; return true;
            } else if (value is ExtensibleLong) {
                result = ((BigInteger)((ExtensibleLong)value).Value).ToDecimal(); return true;
            } else if (value is ExtensibleFloat) {
                result = checked((Decimal)(Double)((ExtensibleFloat)value).value); return true;
            } else if (value is Int64) {
                result = (Decimal)(Int64)value; return true;
            } else if (value is Byte) {
                result = (Decimal)(Byte)value; return true;
            } else if (value is SByte) {
                result = (Decimal)(SByte)value; return true;
            } else if (value is Int16) {
                result = (Decimal)(Int16)value; return true;
            } else if (value is UInt16) {
                result = (Decimal)(UInt16)value; return true;
            } else if (value is UInt32) {
                result = (Decimal)(UInt32)value; return true;
            } else if (value is UInt64) {
                result = (Decimal)(UInt64)value; return true;
            } else if (value is Single) {
                result = checked((Decimal)(Single)value); return true;
            } else if (value is Decimal) {
                result = (Decimal)value; return true;
            }
            result = default(Decimal);
            return false;
        }

        // *** END GENERATED CODE ***

        #endregion
    }
}
