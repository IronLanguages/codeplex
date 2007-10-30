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
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.CompilerServices;

using Microsoft.Scripting;
using Microsoft.Scripting.Math;

using IronPython.Runtime;
using IronPython.Runtime.Types;
using IronPython.Runtime.Operations;

#region Generated ExtensionTypeAttributes

// *** BEGIN GENERATED CODE ***

[assembly: PythonExtensionType(typeof(SByte), typeof(SByteOps))]
[assembly: PythonExtensionType(typeof(Byte), typeof(ByteOps))]
[assembly: PythonExtensionType(typeof(Int16), typeof(Int16Ops))]
[assembly: PythonExtensionType(typeof(UInt16), typeof(UInt16Ops))]
[assembly: PythonExtensionType(typeof(Int32), typeof(Int32Ops), DerivationType=typeof(ExtensibleInt))]
[assembly: PythonExtensionType(typeof(UInt32), typeof(UInt32Ops))]
[assembly: PythonExtensionType(typeof(Int64), typeof(Int64Ops))]
[assembly: PythonExtensionType(typeof(UInt64), typeof(UInt64Ops))]
[assembly: PythonExtensionType(typeof(Single), typeof(SingleOps))]
[assembly: PythonExtensionType(typeof(Double), typeof(DoubleOps), EnableDerivation=true)]

// *** END GENERATED CODE ***

#endregion

namespace IronPython.Runtime.Operations {

    #region Generated IntOps

    // *** BEGIN GENERATED CODE ***

    public static class SByteOps {
        [StaticExtensionMethod("__new__")]
        public static object Make(PythonType cls) {
            return Make(cls, default(SByte));
        }

        [StaticExtensionMethod("__new__")]
        public static object Make(PythonType cls, object value) {
            if (cls != DynamicHelpers.GetPythonTypeFromType(typeof(SByte))) {
                throw PythonOps.TypeError("SByte.__new__: first argument must be SByte type.");
            }
            IConvertible valueConvertible;
            if ((valueConvertible = value as IConvertible) != null) {
                switch (valueConvertible.GetTypeCode()) {
                    case TypeCode.Byte: return (SByte)(Byte)value;
                    case TypeCode.SByte: return (SByte)(SByte)value;
                    case TypeCode.Int16: return (SByte)(Int16)value;
                    case TypeCode.UInt16: return (SByte)(UInt16)value;
                    case TypeCode.Int32: return (SByte)(Int32)value;
                    case TypeCode.UInt32: return (SByte)(UInt32)value;
                    case TypeCode.Int64: return (SByte)(Int64)value;
                    case TypeCode.UInt64: return (SByte)(UInt64)value;
                    case TypeCode.Single: return (SByte)(Single)value;
                    case TypeCode.Double: return (SByte)(Double)value;
                }
            }
            if (value is String) {
                return SByte.Parse((String)value);
            } else if (value is BigInteger) {
                return (SByte)(BigInteger)value;
            } else if (value is Extensible<BigInteger>) {
                return (SByte)((Extensible<BigInteger>)value).Value;
            } else if (value is Extensible<double>) {
                return (SByte)((Extensible<double>)value).Value;
            }
            throw PythonOps.ValueError("invalid value for SByte.__new__");
        }
        // Unary Operations
        [SpecialName]
        public static SByte Plus(SByte x) {
            return x;
        }
        [SpecialName]
        public static object Negate(SByte x) {
            if (x == SByte.MinValue) return -(Int16)SByte.MinValue;
            else return (SByte)(-x);
        }
        [SpecialName]
        public static object Abs(SByte x) {
            if (x < 0) {
                if (x == SByte.MinValue) return -(Int16)SByte.MinValue;
                else return (SByte)(-x);
            } else {
                return x;
            }
        }
        [SpecialName]
        public static SByte OnesComplement(SByte x) {
            return (SByte)(~(x));
        }
        [SpecialName, PythonName("__nonzero__")]
        public static bool NonZero(SByte x) {
            return (x != 0);
        }

        // Binary Operations - Arithmetic
        [SpecialName]
        public static object Add(SByte x, SByte y) {
            Int16 result = (Int16)(((Int16)x) + ((Int16)y));
            if (SByte.MinValue <= result && result <= SByte.MaxValue) {
                return (SByte)(result);
            } else {
                return result;
            }
        }
        [SpecialName]
        public static object Subtract(SByte x, SByte y) {
            Int16 result = (Int16)(((Int16)x) - ((Int16)y));
            if (SByte.MinValue <= result && result <= SByte.MaxValue) {
                return (SByte)(result);
            } else {
                return result;
            }
        }
        [SpecialName]
        public static object Multiply(SByte x, SByte y) {
            Int16 result = (Int16)(((Int16)x) * ((Int16)y));
            if (SByte.MinValue <= result && result <= SByte.MaxValue) {
                return (SByte)(result);
            } else {
                return result;
            }
        }
        [SpecialName]
        public static object Divide(SByte x, SByte y) {
            return FloorDivide(x, y);
        }
        [SpecialName]
        public static double TrueDivide(SByte x, SByte y) {
            return DoubleOps.TrueDivide((double)x, (double)y);

        }
        [SpecialName]
        public static object FloorDivide(SByte x, SByte y) {
            if (y == -1 && x == SByte.MinValue) {
                return -(Int16)SByte.MinValue;
            } else {
                return (SByte)Int32Ops.FloorDivideImpl((Int32)x, (Int32)y);
            }
        }
        [SpecialName]
        public static SByte Mod(SByte x, SByte y) {
            return (SByte)Int32Ops.Mod((Int32)x, (Int32)y);
        }
        [SpecialName]
        public static object Power(SByte x, SByte y) {
            return Int32Ops.Power((Int32)x, (Int32)y);
        }

        // Binary Operations - Bitwise
        [SpecialName]
        public static object LeftShift(SByte x, [NotNull]BigInteger y) {
            return BigIntegerOps.LeftShift((BigInteger)x, (BigInteger)y);
        }
        [SpecialName]
        public static SByte RightShift(SByte x, [NotNull]BigInteger y) {
            return (SByte)BigIntegerOps.RightShift((BigInteger)x, (BigInteger)y);
        }
        [SpecialName]
        public static object LeftShift(SByte x, Int32 y) {
            return Int32Ops.LeftShift((Int32)x, (Int32)y);
        }
        [SpecialName]
        public static SByte RightShift(SByte x, Int32 y) {
            return (SByte)Int32Ops.RightShift((Int32)x, (Int32)y);
        }
        [SpecialName]
        public static SByte BitwiseAnd(SByte x, SByte y) {
            return (SByte)(x & y);
        }
        [SpecialName]
        public static SByte BitwiseOr(SByte x, SByte y) {
            return (SByte)(x | y);
        }
        [SpecialName]
        public static SByte ExclusiveOr(SByte x, SByte y) {
            return (SByte)(x ^ y);
        }

        // Binary Operations - Comparisons
        [SpecialName]
        public static bool LessThan(SByte x, SByte y) {
            return x < y;
        }
        [SpecialName]
        public static bool LessThanOrEqual(SByte x, SByte y) {
            return x <= y;
        }
        [SpecialName]
        public static bool GreaterThan(SByte x, SByte y) {
            return x > y;
        }
        [SpecialName]
        public static bool GreaterThanOrEqual(SByte x, SByte y) {
            return x >= y;
        }
        [SpecialName]
        public static bool Equals(SByte x, SByte y) {
            return x == y;
        }
        [SpecialName]
        public static bool NotEquals(SByte x, SByte y) {
            return x != y;
        }

        // Conversion operators
        [ExplicitConversionMethod]
        public static Byte ConvertToByte(SByte x) {
            if (x >= 0) {
                return (Byte)x;
            }
            throw Converter.CannotConvertOverflow("Byte", x);
        }
        [ImplicitConversionMethod]
        public static Int16 ConvertToInt16(SByte x) {
            return (Int16)x;
        }
        [ExplicitConversionMethod]
        public static UInt16 ConvertToUInt16(SByte x) {
            if (x >= 0) {
                return (UInt16)x;
            }
            throw Converter.CannotConvertOverflow("UInt16", x);
        }
        [ImplicitConversionMethod]
        public static Int32 ConvertToInt32(SByte x) {
            return (Int32)x;
        }
        [ExplicitConversionMethod]
        public static UInt32 ConvertToUInt32(SByte x) {
            if (x >= 0) {
                return (UInt32)x;
            }
            throw Converter.CannotConvertOverflow("UInt32", x);
        }
        [ImplicitConversionMethod]
        public static Int64 ConvertToInt64(SByte x) {
            return (Int64)x;
        }
        [ExplicitConversionMethod]
        public static UInt64 ConvertToUInt64(SByte x) {
            if (x >= 0) {
                return (UInt64)x;
            }
            throw Converter.CannotConvertOverflow("UInt64", x);
        }
        [ImplicitConversionMethod]
        public static Single ConvertToSingle(SByte x) {
            return (Single)x;
        }
        [ImplicitConversionMethod]
        public static Double ConvertToDouble(SByte x) {
            return (Double)x;
        }
    }

    public static class ByteOps {
        [StaticExtensionMethod("__new__")]
        public static object Make(PythonType cls) {
            return Make(cls, default(Byte));
        }

        [StaticExtensionMethod("__new__")]
        public static object Make(PythonType cls, object value) {
            if (cls != DynamicHelpers.GetPythonTypeFromType(typeof(Byte))) {
                throw PythonOps.TypeError("Byte.__new__: first argument must be Byte type.");
            }
            IConvertible valueConvertible;
            if ((valueConvertible = value as IConvertible) != null) {
                switch (valueConvertible.GetTypeCode()) {
                    case TypeCode.Byte: return (Byte)(Byte)value;
                    case TypeCode.SByte: return (Byte)(SByte)value;
                    case TypeCode.Int16: return (Byte)(Int16)value;
                    case TypeCode.UInt16: return (Byte)(UInt16)value;
                    case TypeCode.Int32: return (Byte)(Int32)value;
                    case TypeCode.UInt32: return (Byte)(UInt32)value;
                    case TypeCode.Int64: return (Byte)(Int64)value;
                    case TypeCode.UInt64: return (Byte)(UInt64)value;
                    case TypeCode.Single: return (Byte)(Single)value;
                    case TypeCode.Double: return (Byte)(Double)value;
                }
            }
            if (value is String) {
                return Byte.Parse((String)value);
            } else if (value is BigInteger) {
                return (Byte)(BigInteger)value;
            } else if (value is Extensible<BigInteger>) {
                return (Byte)((Extensible<BigInteger>)value).Value;
            } else if (value is Extensible<double>) {
                return (Byte)((Extensible<double>)value).Value;
            }
            throw PythonOps.ValueError("invalid value for Byte.__new__");
        }
        // Unary Operations
        [SpecialName]
        public static Byte Plus(Byte x) {
            return x;
        }
        [SpecialName]
        public static object Negate(Byte x) {
            return Int16Ops.Negate((Int16)x);
        }
        [SpecialName]
        public static Byte Abs(Byte x) {
            return x;
        }
        [SpecialName]
        public static object OnesComplement(Byte x) {
            return Int16Ops.OnesComplement((Int16)x);
        }
        [SpecialName, PythonName("__nonzero__")]
        public static bool NonZero(Byte x) {
            return (x != 0);
        }

        // Binary Operations - Arithmetic
        [SpecialName]
        public static object Add(Byte x, Byte y) {
            Int16 result = (Int16)(((Int16)x) + ((Int16)y));
            if (Byte.MinValue <= result && result <= Byte.MaxValue) {
                return (Byte)(result);
            } else {
                return result;
            }
        }
        [SpecialName]
        public static object Add(Byte x, SByte y) {
            return Int16Ops.Add((Int16)x, (Int16)y);
        }
        [SpecialName]
        public static object Add(SByte x, Byte y) {
            return Int16Ops.Add((Int16)x, (Int16)y);
        }
        [SpecialName]
        public static object Subtract(Byte x, Byte y) {
            Int16 result = (Int16)(((Int16)x) - ((Int16)y));
            if (Byte.MinValue <= result && result <= Byte.MaxValue) {
                return (Byte)(result);
            } else {
                return result;
            }
        }
        [SpecialName]
        public static object Subtract(Byte x, SByte y) {
            return Int16Ops.Subtract((Int16)x, (Int16)y);
        }
        [SpecialName]
        public static object Subtract(SByte x, Byte y) {
            return Int16Ops.Subtract((Int16)x, (Int16)y);
        }
        [SpecialName]
        public static object Multiply(Byte x, Byte y) {
            Int16 result = (Int16)(((Int16)x) * ((Int16)y));
            if (Byte.MinValue <= result && result <= Byte.MaxValue) {
                return (Byte)(result);
            } else {
                return result;
            }
        }
        [SpecialName]
        public static object Multiply(Byte x, SByte y) {
            return Int16Ops.Multiply((Int16)x, (Int16)y);
        }
        [SpecialName]
        public static object Multiply(SByte x, Byte y) {
            return Int16Ops.Multiply((Int16)x, (Int16)y);
        }
        [SpecialName]
        public static object Divide(Byte x, Byte y) {
            return FloorDivide(x, y);
        }
        [SpecialName]
        public static object Divide(Byte x, SByte y) {
            return Int16Ops.Divide((Int16)x, (Int16)y);
        }
        [SpecialName]
        public static object Divide(SByte x, Byte y) {
            return Int16Ops.Divide((Int16)x, (Int16)y);
        }
        [SpecialName]
        public static double TrueDivide(Byte x, Byte y) {
            return DoubleOps.TrueDivide((double)x, (double)y);

        }
        [SpecialName]
        public static double TrueDivide(Byte x, SByte y) {
            return Int16Ops.TrueDivide((Int16)x, (Int16)y);
        }
        [SpecialName]
        public static double TrueDivide(SByte x, Byte y) {
            return Int16Ops.TrueDivide((Int16)x, (Int16)y);
        }
        [SpecialName]
        public static Byte FloorDivide(Byte x, Byte y) {
            return (Byte)(x / y);
        }
        [SpecialName]
        public static object FloorDivide(Byte x, SByte y) {
            return Int16Ops.FloorDivide((Int16)x, (Int16)y);
        }
        [SpecialName]
        public static object FloorDivide(SByte x, Byte y) {
            return Int16Ops.FloorDivide((Int16)x, (Int16)y);
        }
        [SpecialName]
        public static Byte Mod(Byte x, Byte y) {
            return (Byte)(x % y);
        }
        [SpecialName]
        public static Int16 Mod(Byte x, SByte y) {
            return Int16Ops.Mod((Int16)x, (Int16)y);
        }
        [SpecialName]
        public static Int16 Mod(SByte x, Byte y) {
            return Int16Ops.Mod((Int16)x, (Int16)y);
        }
        [SpecialName]
        public static object Power(Byte x, Byte y) {
            return Int32Ops.Power((Int32)x, (Int32)y);
        }
        [SpecialName]
        public static object Power(Byte x, SByte y) {
            return Int16Ops.Power((Int16)x, (Int16)y);
        }
        [SpecialName]
        public static object Power(SByte x, Byte y) {
            return Int16Ops.Power((Int16)x, (Int16)y);
        }

        // Binary Operations - Bitwise
        [SpecialName]
        public static object LeftShift(Byte x, [NotNull]BigInteger y) {
            return BigIntegerOps.LeftShift((BigInteger)x, (BigInteger)y);
        }
        [SpecialName]
        public static Byte RightShift(Byte x, [NotNull]BigInteger y) {
            return (Byte)BigIntegerOps.RightShift((BigInteger)x, (BigInteger)y);
        }
        [SpecialName]
        public static object LeftShift(Byte x, Int32 y) {
            return Int32Ops.LeftShift((Int32)x, (Int32)y);
        }
        [SpecialName]
        public static Byte RightShift(Byte x, Int32 y) {
            return (Byte)Int32Ops.RightShift((Int32)x, (Int32)y);
        }
        [SpecialName]
        public static Byte BitwiseAnd(Byte x, Byte y) {
            return (Byte)(x & y);
        }
        [SpecialName]
        public static Int16 BitwiseAnd(Byte x, SByte y) {
            return Int16Ops.BitwiseAnd((Int16)x, (Int16)y);
        }
        [SpecialName]
        public static Int16 BitwiseAnd(SByte x, Byte y) {
            return Int16Ops.BitwiseAnd((Int16)x, (Int16)y);
        }
        [SpecialName]
        public static Byte BitwiseOr(Byte x, Byte y) {
            return (Byte)(x | y);
        }
        [SpecialName]
        public static Int16 BitwiseOr(Byte x, SByte y) {
            return Int16Ops.BitwiseOr((Int16)x, (Int16)y);
        }
        [SpecialName]
        public static Int16 BitwiseOr(SByte x, Byte y) {
            return Int16Ops.BitwiseOr((Int16)x, (Int16)y);
        }
        [SpecialName]
        public static Byte ExclusiveOr(Byte x, Byte y) {
            return (Byte)(x ^ y);
        }
        [SpecialName]
        public static Int16 ExclusiveOr(Byte x, SByte y) {
            return Int16Ops.ExclusiveOr((Int16)x, (Int16)y);
        }
        [SpecialName]
        public static Int16 ExclusiveOr(SByte x, Byte y) {
            return Int16Ops.ExclusiveOr((Int16)x, (Int16)y);
        }

        // Binary Operations - Comparisons
        [SpecialName]
        public static bool LessThan(Byte x, Byte y) {
            return x < y;
        }
        [SpecialName]
        public static bool LessThan(Byte x, SByte y) {
            return Int16Ops.LessThan((Int16)x, (Int16)y);
        }
        [SpecialName]
        public static bool LessThan(SByte x, Byte y) {
            return Int16Ops.LessThan((Int16)x, (Int16)y);
        }
        [SpecialName]
        public static bool LessThanOrEqual(Byte x, Byte y) {
            return x <= y;
        }
        [SpecialName]
        public static bool LessThanOrEqual(Byte x, SByte y) {
            return Int16Ops.LessThanOrEqual((Int16)x, (Int16)y);
        }
        [SpecialName]
        public static bool LessThanOrEqual(SByte x, Byte y) {
            return Int16Ops.LessThanOrEqual((Int16)x, (Int16)y);
        }
        [SpecialName]
        public static bool GreaterThan(Byte x, Byte y) {
            return x > y;
        }
        [SpecialName]
        public static bool GreaterThan(Byte x, SByte y) {
            return Int16Ops.GreaterThan((Int16)x, (Int16)y);
        }
        [SpecialName]
        public static bool GreaterThan(SByte x, Byte y) {
            return Int16Ops.GreaterThan((Int16)x, (Int16)y);
        }
        [SpecialName]
        public static bool GreaterThanOrEqual(Byte x, Byte y) {
            return x >= y;
        }
        [SpecialName]
        public static bool GreaterThanOrEqual(Byte x, SByte y) {
            return Int16Ops.GreaterThanOrEqual((Int16)x, (Int16)y);
        }
        [SpecialName]
        public static bool GreaterThanOrEqual(SByte x, Byte y) {
            return Int16Ops.GreaterThanOrEqual((Int16)x, (Int16)y);
        }
        [SpecialName]
        public static bool Equals(Byte x, Byte y) {
            return x == y;
        }
        [SpecialName]
        public static bool Equals(Byte x, SByte y) {
            return Int16Ops.Equals((Int16)x, (Int16)y);
        }
        [SpecialName]
        public static bool Equals(SByte x, Byte y) {
            return Int16Ops.Equals((Int16)x, (Int16)y);
        }
        [SpecialName]
        public static bool NotEquals(Byte x, Byte y) {
            return x != y;
        }
        [SpecialName]
        public static bool NotEquals(Byte x, SByte y) {
            return Int16Ops.NotEquals((Int16)x, (Int16)y);
        }
        [SpecialName]
        public static bool NotEquals(SByte x, Byte y) {
            return Int16Ops.NotEquals((Int16)x, (Int16)y);
        }

        // Conversion operators
        [ExplicitConversionMethod]
        public static SByte ConvertToSByte(Byte x) {
            if (x <= (Byte)SByte.MaxValue) {
                return (SByte)x;
            }
            throw Converter.CannotConvertOverflow("SByte", x);
        }
        [ImplicitConversionMethod]
        public static Int16 ConvertToInt16(Byte x) {
            return (Int16)x;
        }
        [ImplicitConversionMethod]
        public static UInt16 ConvertToUInt16(Byte x) {
            return (UInt16)x;
        }
        [ImplicitConversionMethod]
        public static Int32 ConvertToInt32(Byte x) {
            return (Int32)x;
        }
        [ImplicitConversionMethod]
        public static UInt32 ConvertToUInt32(Byte x) {
            return (UInt32)x;
        }
        [ImplicitConversionMethod]
        public static Int64 ConvertToInt64(Byte x) {
            return (Int64)x;
        }
        [ImplicitConversionMethod]
        public static UInt64 ConvertToUInt64(Byte x) {
            return (UInt64)x;
        }
        [ImplicitConversionMethod]
        public static Single ConvertToSingle(Byte x) {
            return (Single)x;
        }
        [ImplicitConversionMethod]
        public static Double ConvertToDouble(Byte x) {
            return (Double)x;
        }
    }

    public static class Int16Ops {
        [StaticExtensionMethod("__new__")]
        public static object Make(PythonType cls) {
            return Make(cls, default(Int16));
        }

        [StaticExtensionMethod("__new__")]
        public static object Make(PythonType cls, object value) {
            if (cls != DynamicHelpers.GetPythonTypeFromType(typeof(Int16))) {
                throw PythonOps.TypeError("Int16.__new__: first argument must be Int16 type.");
            }
            IConvertible valueConvertible;
            if ((valueConvertible = value as IConvertible) != null) {
                switch (valueConvertible.GetTypeCode()) {
                    case TypeCode.Byte: return (Int16)(Byte)value;
                    case TypeCode.SByte: return (Int16)(SByte)value;
                    case TypeCode.Int16: return (Int16)(Int16)value;
                    case TypeCode.UInt16: return (Int16)(UInt16)value;
                    case TypeCode.Int32: return (Int16)(Int32)value;
                    case TypeCode.UInt32: return (Int16)(UInt32)value;
                    case TypeCode.Int64: return (Int16)(Int64)value;
                    case TypeCode.UInt64: return (Int16)(UInt64)value;
                    case TypeCode.Single: return (Int16)(Single)value;
                    case TypeCode.Double: return (Int16)(Double)value;
                }
            }
            if (value is String) {
                return Int16.Parse((String)value);
            } else if (value is BigInteger) {
                return (Int16)(BigInteger)value;
            } else if (value is Extensible<BigInteger>) {
                return (Int16)((Extensible<BigInteger>)value).Value;
            } else if (value is Extensible<double>) {
                return (Int16)((Extensible<double>)value).Value;
            }
            throw PythonOps.ValueError("invalid value for Int16.__new__");
        }
        // Unary Operations
        [SpecialName]
        public static Int16 Plus(Int16 x) {
            return x;
        }
        [SpecialName]
        public static object Negate(Int16 x) {
            if (x == Int16.MinValue) return -(Int32)Int16.MinValue;
            else return (Int16)(-x);
        }
        [SpecialName]
        public static object Abs(Int16 x) {
            if (x < 0) {
                if (x == Int16.MinValue) return -(Int32)Int16.MinValue;
                else return (Int16)(-x);
            } else {
                return x;
            }
        }
        [SpecialName]
        public static Int16 OnesComplement(Int16 x) {
            return (Int16)(~(x));
        }
        [SpecialName, PythonName("__nonzero__")]
        public static bool NonZero(Int16 x) {
            return (x != 0);
        }

        // Binary Operations - Arithmetic
        [SpecialName]
        public static object Add(Int16 x, Int16 y) {
            Int32 result = (Int32)(((Int32)x) + ((Int32)y));
            if (Int16.MinValue <= result && result <= Int16.MaxValue) {
                return (Int16)(result);
            } else {
                return result;
            }
        }
        [SpecialName]
        public static object Subtract(Int16 x, Int16 y) {
            Int32 result = (Int32)(((Int32)x) - ((Int32)y));
            if (Int16.MinValue <= result && result <= Int16.MaxValue) {
                return (Int16)(result);
            } else {
                return result;
            }
        }
        [SpecialName]
        public static object Multiply(Int16 x, Int16 y) {
            Int32 result = (Int32)(((Int32)x) * ((Int32)y));
            if (Int16.MinValue <= result && result <= Int16.MaxValue) {
                return (Int16)(result);
            } else {
                return result;
            }
        }
        [SpecialName]
        public static object Divide(Int16 x, Int16 y) {
            return FloorDivide(x, y);
        }
        [SpecialName]
        public static double TrueDivide(Int16 x, Int16 y) {
            return DoubleOps.TrueDivide((double)x, (double)y);

        }
        [SpecialName]
        public static object FloorDivide(Int16 x, Int16 y) {
            if (y == -1 && x == Int16.MinValue) {
                return -(Int32)Int16.MinValue;
            } else {
                return (Int16)Int32Ops.FloorDivideImpl((Int32)x, (Int32)y);
            }
        }
        [SpecialName]
        public static Int16 Mod(Int16 x, Int16 y) {
            return (Int16)Int32Ops.Mod((Int32)x, (Int32)y);
        }
        [SpecialName]
        public static object Power(Int16 x, Int16 y) {
            return Int32Ops.Power((Int32)x, (Int32)y);
        }

        // Binary Operations - Bitwise
        [SpecialName]
        public static object LeftShift(Int16 x, [NotNull]BigInteger y) {
            return BigIntegerOps.LeftShift((BigInteger)x, (BigInteger)y);
        }
        [SpecialName]
        public static Int16 RightShift(Int16 x, [NotNull]BigInteger y) {
            return (Int16)BigIntegerOps.RightShift((BigInteger)x, (BigInteger)y);
        }
        [SpecialName]
        public static object LeftShift(Int16 x, Int32 y) {
            return Int32Ops.LeftShift((Int32)x, (Int32)y);
        }
        [SpecialName]
        public static Int16 RightShift(Int16 x, Int32 y) {
            return (Int16)Int32Ops.RightShift((Int32)x, (Int32)y);
        }
        [SpecialName]
        public static Int16 BitwiseAnd(Int16 x, Int16 y) {
            return (Int16)(x & y);
        }
        [SpecialName]
        public static Int16 BitwiseOr(Int16 x, Int16 y) {
            return (Int16)(x | y);
        }
        [SpecialName]
        public static Int16 ExclusiveOr(Int16 x, Int16 y) {
            return (Int16)(x ^ y);
        }

        // Binary Operations - Comparisons
        [SpecialName]
        public static bool LessThan(Int16 x, Int16 y) {
            return x < y;
        }
        [SpecialName]
        public static bool LessThanOrEqual(Int16 x, Int16 y) {
            return x <= y;
        }
        [SpecialName]
        public static bool GreaterThan(Int16 x, Int16 y) {
            return x > y;
        }
        [SpecialName]
        public static bool GreaterThanOrEqual(Int16 x, Int16 y) {
            return x >= y;
        }
        [SpecialName]
        public static bool Equals(Int16 x, Int16 y) {
            return x == y;
        }
        [SpecialName]
        public static bool NotEquals(Int16 x, Int16 y) {
            return x != y;
        }

        // Conversion operators
        [ExplicitConversionMethod]
        public static SByte ConvertToSByte(Int16 x) {
            if (SByte.MinValue <= x && x <= SByte.MaxValue) {
                return (SByte)x;
            }
            throw Converter.CannotConvertOverflow("SByte", x);
        }
        [ExplicitConversionMethod]
        public static Byte ConvertToByte(Int16 x) {
            if (Byte.MinValue <= x && x <= Byte.MaxValue) {
                return (Byte)x;
            }
            throw Converter.CannotConvertOverflow("Byte", x);
        }
        [ExplicitConversionMethod]
        public static UInt16 ConvertToUInt16(Int16 x) {
            if (x >= 0) {
                return (UInt16)x;
            }
            throw Converter.CannotConvertOverflow("UInt16", x);
        }
        [ImplicitConversionMethod]
        public static Int32 ConvertToInt32(Int16 x) {
            return (Int32)x;
        }
        [ExplicitConversionMethod]
        public static UInt32 ConvertToUInt32(Int16 x) {
            if (x >= 0) {
                return (UInt32)x;
            }
            throw Converter.CannotConvertOverflow("UInt32", x);
        }
        [ImplicitConversionMethod]
        public static Int64 ConvertToInt64(Int16 x) {
            return (Int64)x;
        }
        [ExplicitConversionMethod]
        public static UInt64 ConvertToUInt64(Int16 x) {
            if (x >= 0) {
                return (UInt64)x;
            }
            throw Converter.CannotConvertOverflow("UInt64", x);
        }
        [ImplicitConversionMethod]
        public static Single ConvertToSingle(Int16 x) {
            return (Single)x;
        }
        [ImplicitConversionMethod]
        public static Double ConvertToDouble(Int16 x) {
            return (Double)x;
        }
    }

    public static class UInt16Ops {
        [StaticExtensionMethod("__new__")]
        public static object Make(PythonType cls) {
            return Make(cls, default(UInt16));
        }

        [StaticExtensionMethod("__new__")]
        public static object Make(PythonType cls, object value) {
            if (cls != DynamicHelpers.GetPythonTypeFromType(typeof(UInt16))) {
                throw PythonOps.TypeError("UInt16.__new__: first argument must be UInt16 type.");
            }
            IConvertible valueConvertible;
            if ((valueConvertible = value as IConvertible) != null) {
                switch (valueConvertible.GetTypeCode()) {
                    case TypeCode.Byte: return (UInt16)(Byte)value;
                    case TypeCode.SByte: return (UInt16)(SByte)value;
                    case TypeCode.Int16: return (UInt16)(Int16)value;
                    case TypeCode.UInt16: return (UInt16)(UInt16)value;
                    case TypeCode.Int32: return (UInt16)(Int32)value;
                    case TypeCode.UInt32: return (UInt16)(UInt32)value;
                    case TypeCode.Int64: return (UInt16)(Int64)value;
                    case TypeCode.UInt64: return (UInt16)(UInt64)value;
                    case TypeCode.Single: return (UInt16)(Single)value;
                    case TypeCode.Double: return (UInt16)(Double)value;
                }
            }
            if (value is String) {
                return UInt16.Parse((String)value);
            } else if (value is BigInteger) {
                return (UInt16)(BigInteger)value;
            } else if (value is Extensible<BigInteger>) {
                return (UInt16)((Extensible<BigInteger>)value).Value;
            } else if (value is Extensible<double>) {
                return (UInt16)((Extensible<double>)value).Value;
            }
            throw PythonOps.ValueError("invalid value for UInt16.__new__");
        }
        // Unary Operations
        [SpecialName]
        public static UInt16 Plus(UInt16 x) {
            return x;
        }
        [SpecialName]
        public static object Negate(UInt16 x) {
            return Int32Ops.Negate((Int32)x);
        }
        [SpecialName]
        public static UInt16 Abs(UInt16 x) {
            return x;
        }
        [SpecialName]
        public static object OnesComplement(UInt16 x) {
            return Int32Ops.OnesComplement((Int32)x);
        }
        [SpecialName, PythonName("__nonzero__")]
        public static bool NonZero(UInt16 x) {
            return (x != 0);
        }

        // Binary Operations - Arithmetic
        [SpecialName]
        public static object Add(UInt16 x, UInt16 y) {
            Int32 result = (Int32)(((Int32)x) + ((Int32)y));
            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                return (UInt16)(result);
            } else {
                return result;
            }
        }
        [SpecialName]
        public static object Add(UInt16 x, Int16 y) {
            return Int32Ops.Add((Int32)x, (Int32)y);
        }
        [SpecialName]
        public static object Add(Int16 x, UInt16 y) {
            return Int32Ops.Add((Int32)x, (Int32)y);
        }
        [SpecialName]
        public static object Subtract(UInt16 x, UInt16 y) {
            Int32 result = (Int32)(((Int32)x) - ((Int32)y));
            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                return (UInt16)(result);
            } else {
                return result;
            }
        }
        [SpecialName]
        public static object Subtract(UInt16 x, Int16 y) {
            return Int32Ops.Subtract((Int32)x, (Int32)y);
        }
        [SpecialName]
        public static object Subtract(Int16 x, UInt16 y) {
            return Int32Ops.Subtract((Int32)x, (Int32)y);
        }
        [SpecialName]
        public static object Multiply(UInt16 x, UInt16 y) {
            Int32 result = (Int32)(((Int32)x) * ((Int32)y));
            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                return (UInt16)(result);
            } else {
                return result;
            }
        }
        [SpecialName]
        public static object Multiply(UInt16 x, Int16 y) {
            return Int32Ops.Multiply((Int32)x, (Int32)y);
        }
        [SpecialName]
        public static object Multiply(Int16 x, UInt16 y) {
            return Int32Ops.Multiply((Int32)x, (Int32)y);
        }
        [SpecialName]
        public static object Divide(UInt16 x, UInt16 y) {
            return FloorDivide(x, y);
        }
        [SpecialName]
        public static object Divide(UInt16 x, Int16 y) {
            return Int32Ops.Divide((Int32)x, (Int32)y);
        }
        [SpecialName]
        public static object Divide(Int16 x, UInt16 y) {
            return Int32Ops.Divide((Int32)x, (Int32)y);
        }
        [SpecialName]
        public static double TrueDivide(UInt16 x, UInt16 y) {
            return DoubleOps.TrueDivide((double)x, (double)y);

        }
        [SpecialName]
        public static double TrueDivide(UInt16 x, Int16 y) {
            return Int32Ops.TrueDivide((Int32)x, (Int32)y);
        }
        [SpecialName]
        public static double TrueDivide(Int16 x, UInt16 y) {
            return Int32Ops.TrueDivide((Int32)x, (Int32)y);
        }
        [SpecialName]
        public static UInt16 FloorDivide(UInt16 x, UInt16 y) {
            return (UInt16)(x / y);
        }
        [SpecialName]
        public static object FloorDivide(UInt16 x, Int16 y) {
            return Int32Ops.FloorDivide((Int32)x, (Int32)y);
        }
        [SpecialName]
        public static object FloorDivide(Int16 x, UInt16 y) {
            return Int32Ops.FloorDivide((Int32)x, (Int32)y);
        }
        [SpecialName]
        public static UInt16 Mod(UInt16 x, UInt16 y) {
            return (UInt16)(x % y);
        }
        [SpecialName]
        public static Int32 Mod(UInt16 x, Int16 y) {
            return Int32Ops.Mod((Int32)x, (Int32)y);
        }
        [SpecialName]
        public static Int32 Mod(Int16 x, UInt16 y) {
            return Int32Ops.Mod((Int32)x, (Int32)y);
        }
        [SpecialName]
        public static object Power(UInt16 x, UInt16 y) {
            return Int32Ops.Power((Int32)x, (Int32)y);
        }
        [SpecialName]
        public static object Power(UInt16 x, Int16 y) {
            return Int32Ops.Power((Int32)x, (Int32)y);
        }
        [SpecialName]
        public static object Power(Int16 x, UInt16 y) {
            return Int32Ops.Power((Int32)x, (Int32)y);
        }

        // Binary Operations - Bitwise
        [SpecialName]
        public static object LeftShift(UInt16 x, [NotNull]BigInteger y) {
            return BigIntegerOps.LeftShift((BigInteger)x, (BigInteger)y);
        }
        [SpecialName]
        public static UInt16 RightShift(UInt16 x, [NotNull]BigInteger y) {
            return (UInt16)BigIntegerOps.RightShift((BigInteger)x, (BigInteger)y);
        }
        [SpecialName]
        public static object LeftShift(UInt16 x, Int32 y) {
            return Int32Ops.LeftShift((Int32)x, (Int32)y);
        }
        [SpecialName]
        public static UInt16 RightShift(UInt16 x, Int32 y) {
            return (UInt16)Int32Ops.RightShift((Int32)x, (Int32)y);
        }
        [SpecialName]
        public static UInt16 BitwiseAnd(UInt16 x, UInt16 y) {
            return (UInt16)(x & y);
        }
        [SpecialName]
        public static Int32 BitwiseAnd(UInt16 x, Int16 y) {
            return Int32Ops.BitwiseAnd((Int32)x, (Int32)y);
        }
        [SpecialName]
        public static Int32 BitwiseAnd(Int16 x, UInt16 y) {
            return Int32Ops.BitwiseAnd((Int32)x, (Int32)y);
        }
        [SpecialName]
        public static UInt16 BitwiseOr(UInt16 x, UInt16 y) {
            return (UInt16)(x | y);
        }
        [SpecialName]
        public static Int32 BitwiseOr(UInt16 x, Int16 y) {
            return Int32Ops.BitwiseOr((Int32)x, (Int32)y);
        }
        [SpecialName]
        public static Int32 BitwiseOr(Int16 x, UInt16 y) {
            return Int32Ops.BitwiseOr((Int32)x, (Int32)y);
        }
        [SpecialName]
        public static UInt16 ExclusiveOr(UInt16 x, UInt16 y) {
            return (UInt16)(x ^ y);
        }
        [SpecialName]
        public static Int32 ExclusiveOr(UInt16 x, Int16 y) {
            return Int32Ops.ExclusiveOr((Int32)x, (Int32)y);
        }
        [SpecialName]
        public static Int32 ExclusiveOr(Int16 x, UInt16 y) {
            return Int32Ops.ExclusiveOr((Int32)x, (Int32)y);
        }

        // Binary Operations - Comparisons
        [SpecialName]
        public static bool LessThan(UInt16 x, UInt16 y) {
            return x < y;
        }
        [SpecialName]
        public static bool LessThan(UInt16 x, Int16 y) {
            return Int32Ops.LessThan((Int32)x, (Int32)y);
        }
        [SpecialName]
        public static bool LessThan(Int16 x, UInt16 y) {
            return Int32Ops.LessThan((Int32)x, (Int32)y);
        }
        [SpecialName]
        public static bool LessThanOrEqual(UInt16 x, UInt16 y) {
            return x <= y;
        }
        [SpecialName]
        public static bool LessThanOrEqual(UInt16 x, Int16 y) {
            return Int32Ops.LessThanOrEqual((Int32)x, (Int32)y);
        }
        [SpecialName]
        public static bool LessThanOrEqual(Int16 x, UInt16 y) {
            return Int32Ops.LessThanOrEqual((Int32)x, (Int32)y);
        }
        [SpecialName]
        public static bool GreaterThan(UInt16 x, UInt16 y) {
            return x > y;
        }
        [SpecialName]
        public static bool GreaterThan(UInt16 x, Int16 y) {
            return Int32Ops.GreaterThan((Int32)x, (Int32)y);
        }
        [SpecialName]
        public static bool GreaterThan(Int16 x, UInt16 y) {
            return Int32Ops.GreaterThan((Int32)x, (Int32)y);
        }
        [SpecialName]
        public static bool GreaterThanOrEqual(UInt16 x, UInt16 y) {
            return x >= y;
        }
        [SpecialName]
        public static bool GreaterThanOrEqual(UInt16 x, Int16 y) {
            return Int32Ops.GreaterThanOrEqual((Int32)x, (Int32)y);
        }
        [SpecialName]
        public static bool GreaterThanOrEqual(Int16 x, UInt16 y) {
            return Int32Ops.GreaterThanOrEqual((Int32)x, (Int32)y);
        }
        [SpecialName]
        public static bool Equals(UInt16 x, UInt16 y) {
            return x == y;
        }
        [SpecialName]
        public static bool Equals(UInt16 x, Int16 y) {
            return Int32Ops.Equals((Int32)x, (Int32)y);
        }
        [SpecialName]
        public static bool Equals(Int16 x, UInt16 y) {
            return Int32Ops.Equals((Int32)x, (Int32)y);
        }
        [SpecialName]
        public static bool NotEquals(UInt16 x, UInt16 y) {
            return x != y;
        }
        [SpecialName]
        public static bool NotEquals(UInt16 x, Int16 y) {
            return Int32Ops.NotEquals((Int32)x, (Int32)y);
        }
        [SpecialName]
        public static bool NotEquals(Int16 x, UInt16 y) {
            return Int32Ops.NotEquals((Int32)x, (Int32)y);
        }

        // Conversion operators
        [ExplicitConversionMethod]
        public static SByte ConvertToSByte(UInt16 x) {
            if (x <= (UInt16)SByte.MaxValue) {
                return (SByte)x;
            }
            throw Converter.CannotConvertOverflow("SByte", x);
        }
        [ExplicitConversionMethod]
        public static Byte ConvertToByte(UInt16 x) {
            if (Byte.MinValue <= x && x <= Byte.MaxValue) {
                return (Byte)x;
            }
            throw Converter.CannotConvertOverflow("Byte", x);
        }
        [ExplicitConversionMethod]
        public static Int16 ConvertToInt16(UInt16 x) {
            if (x <= (UInt16)Int16.MaxValue) {
                return (Int16)x;
            }
            throw Converter.CannotConvertOverflow("Int16", x);
        }
        [ImplicitConversionMethod]
        public static Int32 ConvertToInt32(UInt16 x) {
            return (Int32)x;
        }
        [ImplicitConversionMethod]
        public static UInt32 ConvertToUInt32(UInt16 x) {
            return (UInt32)x;
        }
        [ImplicitConversionMethod]
        public static Int64 ConvertToInt64(UInt16 x) {
            return (Int64)x;
        }
        [ImplicitConversionMethod]
        public static UInt64 ConvertToUInt64(UInt16 x) {
            return (UInt64)x;
        }
        [ImplicitConversionMethod]
        public static Single ConvertToSingle(UInt16 x) {
            return (Single)x;
        }
        [ImplicitConversionMethod]
        public static Double ConvertToDouble(UInt16 x) {
            return (Double)x;
        }
    }

    public static partial class Int32Ops {
        // Unary Operations
        [SpecialName]
        public static Int32 Plus(Int32 x) {
            return x;
        }
        [SpecialName]
        public static object Negate(Int32 x) {
            if (x == Int32.MinValue) return -(BigInteger)Int32.MinValue;
            else return (Int32)(-x);
        }
        [SpecialName]
        public static object Abs(Int32 x) {
            if (x < 0) {
                if (x == Int32.MinValue) return -(BigInteger)Int32.MinValue;
                else return (Int32)(-x);
            } else {
                return x;
            }
        }
        [SpecialName]
        public static Int32 OnesComplement(Int32 x) {
            return (Int32)(~(x));
        }
        [SpecialName, PythonName("__nonzero__")]
        public static bool NonZero(Int32 x) {
            return (x != 0);
        }

        // Binary Operations - Arithmetic
        [SpecialName]
        public static object Add(Int32 x, Int32 y) {
            long result = (long) x + y;
            if (Int32.MinValue <= result && result <= Int32.MaxValue) {
                return (Int32)(result);
            } 
            return BigIntegerOps.Add((BigInteger)x, (BigInteger)y);

        }
        [SpecialName]
        public static object Subtract(Int32 x, Int32 y) {
            long result = (long) x - y;
            if (Int32.MinValue <= result && result <= Int32.MaxValue) {
                return (Int32)(result);
            } 
            return BigIntegerOps.Subtract((BigInteger)x, (BigInteger)y);

        }
        [SpecialName]
        public static object Multiply(Int32 x, Int32 y) {
            long result = (long) x * y;
            if (Int32.MinValue <= result && result <= Int32.MaxValue) {
                return (Int32)(result);
            } 
            return BigIntegerOps.Multiply((BigInteger)x, (BigInteger)y);

        }
        [SpecialName]
        public static object Divide(Int32 x, Int32 y) {
            return FloorDivide(x, y);
        }
        [SpecialName]
        public static double TrueDivide(Int32 x, Int32 y) {
            return DoubleOps.TrueDivide((double)x, (double)y);

        }

        // Binary Operations - Bitwise
        [SpecialName]
        public static object LeftShift(Int32 x, [NotNull]BigInteger y) {
            return BigIntegerOps.LeftShift((BigInteger)x, (BigInteger)y);
        }
        [SpecialName]
        public static Int32 RightShift(Int32 x, [NotNull]BigInteger y) {
            return (Int32)BigIntegerOps.RightShift((BigInteger)x, (BigInteger)y);
        }
        [SpecialName]
        public static Int32 BitwiseAnd(Int32 x, Int32 y) {
            return (Int32)(x & y);
        }
        [SpecialName]
        public static Int32 BitwiseOr(Int32 x, Int32 y) {
            return (Int32)(x | y);
        }
        [SpecialName]
        public static Int32 ExclusiveOr(Int32 x, Int32 y) {
            return (Int32)(x ^ y);
        }

        // Binary Operations - Comparisons
        [SpecialName]
        public static bool LessThan(Int32 x, Int32 y) {
            return x < y;
        }
        [SpecialName]
        public static bool LessThanOrEqual(Int32 x, Int32 y) {
            return x <= y;
        }
        [SpecialName]
        public static bool GreaterThan(Int32 x, Int32 y) {
            return x > y;
        }
        [SpecialName]
        public static bool GreaterThanOrEqual(Int32 x, Int32 y) {
            return x >= y;
        }
        [SpecialName]
        public static bool Equals(Int32 x, Int32 y) {
            return x == y;
        }
        [SpecialName]
        public static bool NotEquals(Int32 x, Int32 y) {
            return x != y;
        }

        // Conversion operators
        [ExplicitConversionMethod]
        public static SByte ConvertToSByte(Int32 x) {
            if (SByte.MinValue <= x && x <= SByte.MaxValue) {
                return (SByte)x;
            }
            throw Converter.CannotConvertOverflow("SByte", x);
        }
        [ExplicitConversionMethod]
        public static Byte ConvertToByte(Int32 x) {
            if (Byte.MinValue <= x && x <= Byte.MaxValue) {
                return (Byte)x;
            }
            throw Converter.CannotConvertOverflow("Byte", x);
        }
        [ExplicitConversionMethod]
        public static Int16 ConvertToInt16(Int32 x) {
            if (Int16.MinValue <= x && x <= Int16.MaxValue) {
                return (Int16)x;
            }
            throw Converter.CannotConvertOverflow("Int16", x);
        }
        [ExplicitConversionMethod]
        public static UInt16 ConvertToUInt16(Int32 x) {
            if (UInt16.MinValue <= x && x <= UInt16.MaxValue) {
                return (UInt16)x;
            }
            throw Converter.CannotConvertOverflow("UInt16", x);
        }
        [ExplicitConversionMethod]
        public static UInt32 ConvertToUInt32(Int32 x) {
            if (x >= 0) {
                return (UInt32)x;
            }
            throw Converter.CannotConvertOverflow("UInt32", x);
        }
        [ImplicitConversionMethod]
        public static Int64 ConvertToInt64(Int32 x) {
            return (Int64)x;
        }
        [ExplicitConversionMethod]
        public static UInt64 ConvertToUInt64(Int32 x) {
            if (x >= 0) {
                return (UInt64)x;
            }
            throw Converter.CannotConvertOverflow("UInt64", x);
        }
        [ImplicitConversionMethod]
        public static Single ConvertToSingle(Int32 x) {
            return (Single)x;
        }
        [ImplicitConversionMethod]
        public static Double ConvertToDouble(Int32 x) {
            return (Double)x;
        }
    }

    public static class UInt32Ops {
        [StaticExtensionMethod("__new__")]
        public static object Make(PythonType cls) {
            return Make(cls, default(UInt32));
        }

        [StaticExtensionMethod("__new__")]
        public static object Make(PythonType cls, object value) {
            if (cls != DynamicHelpers.GetPythonTypeFromType(typeof(UInt32))) {
                throw PythonOps.TypeError("UInt32.__new__: first argument must be UInt32 type.");
            }
            IConvertible valueConvertible;
            if ((valueConvertible = value as IConvertible) != null) {
                switch (valueConvertible.GetTypeCode()) {
                    case TypeCode.Byte: return (UInt32)(Byte)value;
                    case TypeCode.SByte: return (UInt32)(SByte)value;
                    case TypeCode.Int16: return (UInt32)(Int16)value;
                    case TypeCode.UInt16: return (UInt32)(UInt16)value;
                    case TypeCode.Int32: return (UInt32)(Int32)value;
                    case TypeCode.UInt32: return (UInt32)(UInt32)value;
                    case TypeCode.Int64: return (UInt32)(Int64)value;
                    case TypeCode.UInt64: return (UInt32)(UInt64)value;
                    case TypeCode.Single: return (UInt32)(Single)value;
                    case TypeCode.Double: return (UInt32)(Double)value;
                }
            }
            if (value is String) {
                return UInt32.Parse((String)value);
            } else if (value is BigInteger) {
                return (UInt32)(BigInteger)value;
            } else if (value is Extensible<BigInteger>) {
                return (UInt32)((Extensible<BigInteger>)value).Value;
            } else if (value is Extensible<double>) {
                return (UInt32)((Extensible<double>)value).Value;
            }
            throw PythonOps.ValueError("invalid value for UInt32.__new__");
        }
        // Unary Operations
        [SpecialName]
        public static UInt32 Plus(UInt32 x) {
            return x;
        }
        [SpecialName]
        public static object Negate(UInt32 x) {
            return Int64Ops.Negate((Int64)x);
        }
        [SpecialName]
        public static UInt32 Abs(UInt32 x) {
            return x;
        }
        [SpecialName]
        public static object OnesComplement(UInt32 x) {
            return Int64Ops.OnesComplement((Int64)x);
        }
        [SpecialName, PythonName("__nonzero__")]
        public static bool NonZero(UInt32 x) {
            return (x != 0);
        }

        // Binary Operations - Arithmetic
        [SpecialName]
        public static object Add(UInt32 x, UInt32 y) {
            Int64 result = (Int64)(((Int64)x) + ((Int64)y));
            if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                return (UInt32)(result);
            } else {
                return result;
            }
        }
        [SpecialName]
        public static object Add(UInt32 x, Int32 y) {
            return Int64Ops.Add((Int64)x, (Int64)y);
        }
        [SpecialName]
        public static object Add(Int32 x, UInt32 y) {
            return Int64Ops.Add((Int64)x, (Int64)y);
        }
        [SpecialName]
        public static object Subtract(UInt32 x, UInt32 y) {
            Int64 result = (Int64)(((Int64)x) - ((Int64)y));
            if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                return (UInt32)(result);
            } else {
                return result;
            }
        }
        [SpecialName]
        public static object Subtract(UInt32 x, Int32 y) {
            return Int64Ops.Subtract((Int64)x, (Int64)y);
        }
        [SpecialName]
        public static object Subtract(Int32 x, UInt32 y) {
            return Int64Ops.Subtract((Int64)x, (Int64)y);
        }
        [SpecialName]
        public static object Multiply(UInt32 x, UInt32 y) {
            Int64 result = (Int64)(((Int64)x) * ((Int64)y));
            if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                return (UInt32)(result);
            } else {
                return result;
            }
        }
        [SpecialName]
        public static object Multiply(UInt32 x, Int32 y) {
            return Int64Ops.Multiply((Int64)x, (Int64)y);
        }
        [SpecialName]
        public static object Multiply(Int32 x, UInt32 y) {
            return Int64Ops.Multiply((Int64)x, (Int64)y);
        }
        [SpecialName]
        public static object Divide(UInt32 x, UInt32 y) {
            return FloorDivide(x, y);
        }
        [SpecialName]
        public static object Divide(UInt32 x, Int32 y) {
            return Int64Ops.Divide((Int64)x, (Int64)y);
        }
        [SpecialName]
        public static object Divide(Int32 x, UInt32 y) {
            return Int64Ops.Divide((Int64)x, (Int64)y);
        }
        [SpecialName]
        public static double TrueDivide(UInt32 x, UInt32 y) {
            return DoubleOps.TrueDivide((double)x, (double)y);

        }
        [SpecialName]
        public static double TrueDivide(UInt32 x, Int32 y) {
            return Int64Ops.TrueDivide((Int64)x, (Int64)y);
        }
        [SpecialName]
        public static double TrueDivide(Int32 x, UInt32 y) {
            return Int64Ops.TrueDivide((Int64)x, (Int64)y);
        }
        [SpecialName]
        public static UInt32 FloorDivide(UInt32 x, UInt32 y) {
            return (UInt32)(x / y);
        }
        [SpecialName]
        public static object FloorDivide(UInt32 x, Int32 y) {
            return Int64Ops.FloorDivide((Int64)x, (Int64)y);
        }
        [SpecialName]
        public static object FloorDivide(Int32 x, UInt32 y) {
            return Int64Ops.FloorDivide((Int64)x, (Int64)y);
        }
        [SpecialName]
        public static UInt32 Mod(UInt32 x, UInt32 y) {
            return (UInt32)(x % y);
        }
        [SpecialName]
        public static Int64 Mod(UInt32 x, Int32 y) {
            return Int64Ops.Mod((Int64)x, (Int64)y);
        }
        [SpecialName]
        public static Int64 Mod(Int32 x, UInt32 y) {
            return Int64Ops.Mod((Int64)x, (Int64)y);
        }
        [SpecialName]
        public static object Power(UInt32 x, UInt32 y) {
            return Int32Ops.Power((Int32)x, (Int32)y);
        }
        [SpecialName]
        public static object Power(UInt32 x, Int32 y) {
            return Int64Ops.Power((Int64)x, (Int64)y);
        }
        [SpecialName]
        public static object Power(Int32 x, UInt32 y) {
            return Int64Ops.Power((Int64)x, (Int64)y);
        }

        // Binary Operations - Bitwise
        [SpecialName]
        public static object LeftShift(UInt32 x, [NotNull]BigInteger y) {
            return BigIntegerOps.LeftShift((BigInteger)x, (BigInteger)y);
        }
        [SpecialName]
        public static UInt32 RightShift(UInt32 x, [NotNull]BigInteger y) {
            return (UInt32)BigIntegerOps.RightShift((BigInteger)x, (BigInteger)y);
        }
        [SpecialName]
        public static UInt32 BitwiseAnd(UInt32 x, UInt32 y) {
            return (UInt32)(x & y);
        }
        [SpecialName]
        public static Int64 BitwiseAnd(UInt32 x, Int32 y) {
            return Int64Ops.BitwiseAnd((Int64)x, (Int64)y);
        }
        [SpecialName]
        public static Int64 BitwiseAnd(Int32 x, UInt32 y) {
            return Int64Ops.BitwiseAnd((Int64)x, (Int64)y);
        }
        [SpecialName]
        public static UInt32 BitwiseOr(UInt32 x, UInt32 y) {
            return (UInt32)(x | y);
        }
        [SpecialName]
        public static Int64 BitwiseOr(UInt32 x, Int32 y) {
            return Int64Ops.BitwiseOr((Int64)x, (Int64)y);
        }
        [SpecialName]
        public static Int64 BitwiseOr(Int32 x, UInt32 y) {
            return Int64Ops.BitwiseOr((Int64)x, (Int64)y);
        }
        [SpecialName]
        public static UInt32 ExclusiveOr(UInt32 x, UInt32 y) {
            return (UInt32)(x ^ y);
        }
        [SpecialName]
        public static Int64 ExclusiveOr(UInt32 x, Int32 y) {
            return Int64Ops.ExclusiveOr((Int64)x, (Int64)y);
        }
        [SpecialName]
        public static Int64 ExclusiveOr(Int32 x, UInt32 y) {
            return Int64Ops.ExclusiveOr((Int64)x, (Int64)y);
        }

        // Binary Operations - Comparisons
        [SpecialName]
        public static bool LessThan(UInt32 x, UInt32 y) {
            return x < y;
        }
        [SpecialName]
        public static bool LessThan(UInt32 x, Int32 y) {
            return Int64Ops.LessThan((Int64)x, (Int64)y);
        }
        [SpecialName]
        public static bool LessThan(Int32 x, UInt32 y) {
            return Int64Ops.LessThan((Int64)x, (Int64)y);
        }
        [SpecialName]
        public static bool LessThanOrEqual(UInt32 x, UInt32 y) {
            return x <= y;
        }
        [SpecialName]
        public static bool LessThanOrEqual(UInt32 x, Int32 y) {
            return Int64Ops.LessThanOrEqual((Int64)x, (Int64)y);
        }
        [SpecialName]
        public static bool LessThanOrEqual(Int32 x, UInt32 y) {
            return Int64Ops.LessThanOrEqual((Int64)x, (Int64)y);
        }
        [SpecialName]
        public static bool GreaterThan(UInt32 x, UInt32 y) {
            return x > y;
        }
        [SpecialName]
        public static bool GreaterThan(UInt32 x, Int32 y) {
            return Int64Ops.GreaterThan((Int64)x, (Int64)y);
        }
        [SpecialName]
        public static bool GreaterThan(Int32 x, UInt32 y) {
            return Int64Ops.GreaterThan((Int64)x, (Int64)y);
        }
        [SpecialName]
        public static bool GreaterThanOrEqual(UInt32 x, UInt32 y) {
            return x >= y;
        }
        [SpecialName]
        public static bool GreaterThanOrEqual(UInt32 x, Int32 y) {
            return Int64Ops.GreaterThanOrEqual((Int64)x, (Int64)y);
        }
        [SpecialName]
        public static bool GreaterThanOrEqual(Int32 x, UInt32 y) {
            return Int64Ops.GreaterThanOrEqual((Int64)x, (Int64)y);
        }
        [SpecialName]
        public static bool Equals(UInt32 x, UInt32 y) {
            return x == y;
        }
        [SpecialName]
        public static bool Equals(UInt32 x, Int32 y) {
            return Int64Ops.Equals((Int64)x, (Int64)y);
        }
        [SpecialName]
        public static bool Equals(Int32 x, UInt32 y) {
            return Int64Ops.Equals((Int64)x, (Int64)y);
        }
        [SpecialName]
        public static bool NotEquals(UInt32 x, UInt32 y) {
            return x != y;
        }
        [SpecialName]
        public static bool NotEquals(UInt32 x, Int32 y) {
            return Int64Ops.NotEquals((Int64)x, (Int64)y);
        }
        [SpecialName]
        public static bool NotEquals(Int32 x, UInt32 y) {
            return Int64Ops.NotEquals((Int64)x, (Int64)y);
        }

        // Conversion operators
        [ExplicitConversionMethod]
        public static SByte ConvertToSByte(UInt32 x) {
            if (x <= (UInt32)SByte.MaxValue) {
                return (SByte)x;
            }
            throw Converter.CannotConvertOverflow("SByte", x);
        }
        [ExplicitConversionMethod]
        public static Byte ConvertToByte(UInt32 x) {
            if (Byte.MinValue <= x && x <= Byte.MaxValue) {
                return (Byte)x;
            }
            throw Converter.CannotConvertOverflow("Byte", x);
        }
        [ExplicitConversionMethod]
        public static Int16 ConvertToInt16(UInt32 x) {
            if (x <= (UInt32)Int16.MaxValue) {
                return (Int16)x;
            }
            throw Converter.CannotConvertOverflow("Int16", x);
        }
        [ExplicitConversionMethod]
        public static UInt16 ConvertToUInt16(UInt32 x) {
            if (UInt16.MinValue <= x && x <= UInt16.MaxValue) {
                return (UInt16)x;
            }
            throw Converter.CannotConvertOverflow("UInt16", x);
        }
        [ExplicitConversionMethod]
        public static Int32 ConvertToInt32(UInt32 x) {
            if (x <= (UInt32)Int32.MaxValue) {
                return (Int32)x;
            }
            throw Converter.CannotConvertOverflow("Int32", x);
        }
        [ImplicitConversionMethod]
        public static Int64 ConvertToInt64(UInt32 x) {
            return (Int64)x;
        }
        [ImplicitConversionMethod]
        public static UInt64 ConvertToUInt64(UInt32 x) {
            return (UInt64)x;
        }
        [ImplicitConversionMethod]
        public static Single ConvertToSingle(UInt32 x) {
            return (Single)x;
        }
        [ImplicitConversionMethod]
        public static Double ConvertToDouble(UInt32 x) {
            return (Double)x;
        }
    }

    public static class Int64Ops {
        [StaticExtensionMethod("__new__")]
        public static object Make(PythonType cls) {
            return Make(cls, default(Int64));
        }

        [StaticExtensionMethod("__new__")]
        public static object Make(PythonType cls, object value) {
            if (cls != DynamicHelpers.GetPythonTypeFromType(typeof(Int64))) {
                throw PythonOps.TypeError("Int64.__new__: first argument must be Int64 type.");
            }
            IConvertible valueConvertible;
            if ((valueConvertible = value as IConvertible) != null) {
                switch (valueConvertible.GetTypeCode()) {
                    case TypeCode.Byte: return (Int64)(Byte)value;
                    case TypeCode.SByte: return (Int64)(SByte)value;
                    case TypeCode.Int16: return (Int64)(Int16)value;
                    case TypeCode.UInt16: return (Int64)(UInt16)value;
                    case TypeCode.Int32: return (Int64)(Int32)value;
                    case TypeCode.UInt32: return (Int64)(UInt32)value;
                    case TypeCode.Int64: return (Int64)(Int64)value;
                    case TypeCode.UInt64: return (Int64)(UInt64)value;
                    case TypeCode.Single: return (Int64)(Single)value;
                    case TypeCode.Double: return (Int64)(Double)value;
                }
            }
            if (value is String) {
                return Int64.Parse((String)value);
            } else if (value is BigInteger) {
                return (Int64)(BigInteger)value;
            } else if (value is Extensible<BigInteger>) {
                return (Int64)((Extensible<BigInteger>)value).Value;
            } else if (value is Extensible<double>) {
                return (Int64)((Extensible<double>)value).Value;
            }
            throw PythonOps.ValueError("invalid value for Int64.__new__");
        }
        // Unary Operations
        [SpecialName]
        public static Int64 Plus(Int64 x) {
            return x;
        }
        [SpecialName]
        public static object Negate(Int64 x) {
            if (x == Int64.MinValue) return -(BigInteger)Int64.MinValue;
            else return (Int64)(-x);
        }
        [SpecialName]
        public static object Abs(Int64 x) {
            if (x < 0) {
                if (x == Int64.MinValue) return -(BigInteger)Int64.MinValue;
                else return (Int64)(-x);
            } else {
                return x;
            }
        }
        [SpecialName]
        public static Int64 OnesComplement(Int64 x) {
            return (Int64)(~(x));
        }
        [SpecialName, PythonName("__nonzero__")]
        public static bool NonZero(Int64 x) {
            return (x != 0);
        }

        // Binary Operations - Arithmetic
        [SpecialName]
        public static object Add(Int64 x, Int64 y) {
            try {
                return (Int64)(checked(x + y));
            } catch (OverflowException) {
                return BigIntegerOps.Add((BigInteger)x, (BigInteger)y);
            }
        }
        [SpecialName]
        public static object Subtract(Int64 x, Int64 y) {
            try {
                return (Int64)(checked(x - y));
            } catch (OverflowException) {
                return BigIntegerOps.Subtract((BigInteger)x, (BigInteger)y);
            }
        }
        [SpecialName]
        public static object Multiply(Int64 x, Int64 y) {
            try {
                return (Int64)(checked(x * y));
            } catch (OverflowException) {
                return BigIntegerOps.Multiply((BigInteger)x, (BigInteger)y);
            }
        }
        [SpecialName]
        public static object Divide(Int64 x, Int64 y) {
            return FloorDivide(x, y);
        }
        [SpecialName]
        public static double TrueDivide(Int64 x, Int64 y) {
            return DoubleOps.TrueDivide((double)x, (double)y);

        }
        [SpecialName]
        public static object FloorDivide(Int64 x, Int64 y) {
            if (y == -1 && x == Int64.MinValue) {
                return -(BigInteger)Int64.MinValue;
            } else {
                return (Int64)BigIntegerOps.FloorDivideImpl((BigInteger)x, (BigInteger)y);
            }
        }
        [SpecialName]
        public static Int64 Mod(Int64 x, Int64 y) {
            return (Int64)BigIntegerOps.Mod((BigInteger)x, (BigInteger)y);
        }
        [SpecialName]
        public static object Power(Int64 x, Int64 y) {
            return BigIntegerOps.Power((BigInteger)x, (BigInteger)y);
        }

        // Binary Operations - Bitwise
        [SpecialName]
        public static object LeftShift(Int64 x, [NotNull]BigInteger y) {
            return BigIntegerOps.LeftShift((BigInteger)x, (BigInteger)y);
        }
        [SpecialName]
        public static Int64 RightShift(Int64 x, [NotNull]BigInteger y) {
            return (Int64)BigIntegerOps.RightShift((BigInteger)x, (BigInteger)y);
        }
        [SpecialName]
        public static Int64 BitwiseAnd(Int64 x, Int64 y) {
            return (Int64)(x & y);
        }
        [SpecialName]
        public static Int64 BitwiseOr(Int64 x, Int64 y) {
            return (Int64)(x | y);
        }
        [SpecialName]
        public static Int64 ExclusiveOr(Int64 x, Int64 y) {
            return (Int64)(x ^ y);
        }

        // Binary Operations - Comparisons
        [SpecialName]
        public static bool LessThan(Int64 x, Int64 y) {
            return x < y;
        }
        [SpecialName]
        public static bool LessThanOrEqual(Int64 x, Int64 y) {
            return x <= y;
        }
        [SpecialName]
        public static bool GreaterThan(Int64 x, Int64 y) {
            return x > y;
        }
        [SpecialName]
        public static bool GreaterThanOrEqual(Int64 x, Int64 y) {
            return x >= y;
        }
        [SpecialName]
        public static bool Equals(Int64 x, Int64 y) {
            return x == y;
        }
        [SpecialName]
        public static bool NotEquals(Int64 x, Int64 y) {
            return x != y;
        }

        // Conversion operators
        [ExplicitConversionMethod]
        public static SByte ConvertToSByte(Int64 x) {
            if (SByte.MinValue <= x && x <= SByte.MaxValue) {
                return (SByte)x;
            }
            throw Converter.CannotConvertOverflow("SByte", x);
        }
        [ExplicitConversionMethod]
        public static Byte ConvertToByte(Int64 x) {
            if (Byte.MinValue <= x && x <= Byte.MaxValue) {
                return (Byte)x;
            }
            throw Converter.CannotConvertOverflow("Byte", x);
        }
        [ExplicitConversionMethod]
        public static Int16 ConvertToInt16(Int64 x) {
            if (Int16.MinValue <= x && x <= Int16.MaxValue) {
                return (Int16)x;
            }
            throw Converter.CannotConvertOverflow("Int16", x);
        }
        [ExplicitConversionMethod]
        public static UInt16 ConvertToUInt16(Int64 x) {
            if (UInt16.MinValue <= x && x <= UInt16.MaxValue) {
                return (UInt16)x;
            }
            throw Converter.CannotConvertOverflow("UInt16", x);
        }
        [ExplicitConversionMethod]
        public static Int32 ConvertToInt32(Int64 x) {
            if (Int32.MinValue <= x && x <= Int32.MaxValue) {
                return (Int32)x;
            }
            throw Converter.CannotConvertOverflow("Int32", x);
        }
        [ExplicitConversionMethod]
        public static UInt32 ConvertToUInt32(Int64 x) {
            if (UInt32.MinValue <= x && x <= UInt32.MaxValue) {
                return (UInt32)x;
            }
            throw Converter.CannotConvertOverflow("UInt32", x);
        }
        [ExplicitConversionMethod]
        public static UInt64 ConvertToUInt64(Int64 x) {
            if (x >= 0) {
                return (UInt64)x;
            }
            throw Converter.CannotConvertOverflow("UInt64", x);
        }
        [ImplicitConversionMethod]
        public static Single ConvertToSingle(Int64 x) {
            return (Single)x;
        }
        [ImplicitConversionMethod]
        public static Double ConvertToDouble(Int64 x) {
            return (Double)x;
        }
    }

    public static class UInt64Ops {
        [StaticExtensionMethod("__new__")]
        public static object Make(PythonType cls) {
            return Make(cls, default(UInt64));
        }

        [StaticExtensionMethod("__new__")]
        public static object Make(PythonType cls, object value) {
            if (cls != DynamicHelpers.GetPythonTypeFromType(typeof(UInt64))) {
                throw PythonOps.TypeError("UInt64.__new__: first argument must be UInt64 type.");
            }
            IConvertible valueConvertible;
            if ((valueConvertible = value as IConvertible) != null) {
                switch (valueConvertible.GetTypeCode()) {
                    case TypeCode.Byte: return (UInt64)(Byte)value;
                    case TypeCode.SByte: return (UInt64)(SByte)value;
                    case TypeCode.Int16: return (UInt64)(Int16)value;
                    case TypeCode.UInt16: return (UInt64)(UInt16)value;
                    case TypeCode.Int32: return (UInt64)(Int32)value;
                    case TypeCode.UInt32: return (UInt64)(UInt32)value;
                    case TypeCode.Int64: return (UInt64)(Int64)value;
                    case TypeCode.UInt64: return (UInt64)(UInt64)value;
                    case TypeCode.Single: return (UInt64)(Single)value;
                    case TypeCode.Double: return (UInt64)(Double)value;
                }
            }
            if (value is String) {
                return UInt64.Parse((String)value);
            } else if (value is BigInteger) {
                return (UInt64)(BigInteger)value;
            } else if (value is Extensible<BigInteger>) {
                return (UInt64)((Extensible<BigInteger>)value).Value;
            } else if (value is Extensible<double>) {
                return (UInt64)((Extensible<double>)value).Value;
            }
            throw PythonOps.ValueError("invalid value for UInt64.__new__");
        }
        // Unary Operations
        [SpecialName]
        public static UInt64 Plus(UInt64 x) {
            return x;
        }
        [SpecialName]
        public static object Negate(UInt64 x) {
            return BigIntegerOps.Negate((BigInteger)x);
        }
        [SpecialName]
        public static UInt64 Abs(UInt64 x) {
            return x;
        }
        [SpecialName]
        public static object OnesComplement(UInt64 x) {
            return BigIntegerOps.OnesComplement((BigInteger)x);
        }
        [SpecialName, PythonName("__nonzero__")]
        public static bool NonZero(UInt64 x) {
            return (x != 0);
        }

        // Binary Operations - Arithmetic
        [SpecialName]
        public static object Add(UInt64 x, UInt64 y) {
            try {
                return (UInt64)(checked(x + y));
            } catch (OverflowException) {
                return BigIntegerOps.Add((BigInteger)x, (BigInteger)y);
            }
        }
        [SpecialName]
        public static object Add(UInt64 x, Int64 y) {
            return BigIntegerOps.Add((BigInteger)x, (BigInteger)y);
        }
        [SpecialName]
        public static object Add(Int64 x, UInt64 y) {
            return BigIntegerOps.Add((BigInteger)x, (BigInteger)y);
        }
        [SpecialName]
        public static object Subtract(UInt64 x, UInt64 y) {
            try {
                return (UInt64)(checked(x - y));
            } catch (OverflowException) {
                return BigIntegerOps.Subtract((BigInteger)x, (BigInteger)y);
            }
        }
        [SpecialName]
        public static object Subtract(UInt64 x, Int64 y) {
            return BigIntegerOps.Subtract((BigInteger)x, (BigInteger)y);
        }
        [SpecialName]
        public static object Subtract(Int64 x, UInt64 y) {
            return BigIntegerOps.Subtract((BigInteger)x, (BigInteger)y);
        }
        [SpecialName]
        public static object Multiply(UInt64 x, UInt64 y) {
            try {
                return (UInt64)(checked(x * y));
            } catch (OverflowException) {
                return BigIntegerOps.Multiply((BigInteger)x, (BigInteger)y);
            }
        }
        [SpecialName]
        public static object Multiply(UInt64 x, Int64 y) {
            return BigIntegerOps.Multiply((BigInteger)x, (BigInteger)y);
        }
        [SpecialName]
        public static object Multiply(Int64 x, UInt64 y) {
            return BigIntegerOps.Multiply((BigInteger)x, (BigInteger)y);
        }
        [SpecialName]
        public static object Divide(UInt64 x, UInt64 y) {
            return FloorDivide(x, y);
        }
        [SpecialName]
        public static object Divide(UInt64 x, Int64 y) {
            return BigIntegerOps.Divide((BigInteger)x, (BigInteger)y);
        }
        [SpecialName]
        public static object Divide(Int64 x, UInt64 y) {
            return BigIntegerOps.Divide((BigInteger)x, (BigInteger)y);
        }
        [SpecialName]
        public static double TrueDivide(UInt64 x, UInt64 y) {
            return DoubleOps.TrueDivide((double)x, (double)y);

        }
        [SpecialName]
        public static double TrueDivide(UInt64 x, Int64 y) {
            return BigIntegerOps.TrueDivide((BigInteger)x, (BigInteger)y);
        }
        [SpecialName]
        public static double TrueDivide(Int64 x, UInt64 y) {
            return BigIntegerOps.TrueDivide((BigInteger)x, (BigInteger)y);
        }
        [SpecialName]
        public static UInt64 FloorDivide(UInt64 x, UInt64 y) {
            return (UInt64)(x / y);
        }
        [SpecialName]
        public static object FloorDivide(UInt64 x, Int64 y) {
            return BigIntegerOps.FloorDivide((BigInteger)x, (BigInteger)y);
        }
        [SpecialName]
        public static object FloorDivide(Int64 x, UInt64 y) {
            return BigIntegerOps.FloorDivide((BigInteger)x, (BigInteger)y);
        }
        [SpecialName]
        public static UInt64 Mod(UInt64 x, UInt64 y) {
            return (UInt64)(x % y);
        }
        [SpecialName]
        public static BigInteger Mod(UInt64 x, Int64 y) {
            return BigIntegerOps.Mod((BigInteger)x, (BigInteger)y);
        }
        [SpecialName]
        public static BigInteger Mod(Int64 x, UInt64 y) {
            return BigIntegerOps.Mod((BigInteger)x, (BigInteger)y);
        }
        [SpecialName]
        public static object Power(UInt64 x, UInt64 y) {
            return BigIntegerOps.Power((BigInteger)x, (BigInteger)y);
        }
        [SpecialName]
        public static object Power(UInt64 x, Int64 y) {
            return BigIntegerOps.Power((BigInteger)x, (BigInteger)y);
        }
        [SpecialName]
        public static object Power(Int64 x, UInt64 y) {
            return BigIntegerOps.Power((BigInteger)x, (BigInteger)y);
        }

        // Binary Operations - Bitwise
        [SpecialName]
        public static object LeftShift(UInt64 x, [NotNull]BigInteger y) {
            return BigIntegerOps.LeftShift((BigInteger)x, (BigInteger)y);
        }
        [SpecialName]
        public static UInt64 RightShift(UInt64 x, [NotNull]BigInteger y) {
            return (UInt64)BigIntegerOps.RightShift((BigInteger)x, (BigInteger)y);
        }
        [SpecialName]
        public static UInt64 BitwiseAnd(UInt64 x, UInt64 y) {
            return (UInt64)(x & y);
        }
        [SpecialName]
        public static BigInteger BitwiseAnd(UInt64 x, Int64 y) {
            return BigIntegerOps.BitwiseAnd((BigInteger)x, (BigInteger)y);
        }
        [SpecialName]
        public static BigInteger BitwiseAnd(Int64 x, UInt64 y) {
            return BigIntegerOps.BitwiseAnd((BigInteger)x, (BigInteger)y);
        }
        [SpecialName]
        public static UInt64 BitwiseOr(UInt64 x, UInt64 y) {
            return (UInt64)(x | y);
        }
        [SpecialName]
        public static BigInteger BitwiseOr(UInt64 x, Int64 y) {
            return BigIntegerOps.BitwiseOr((BigInteger)x, (BigInteger)y);
        }
        [SpecialName]
        public static BigInteger BitwiseOr(Int64 x, UInt64 y) {
            return BigIntegerOps.BitwiseOr((BigInteger)x, (BigInteger)y);
        }
        [SpecialName]
        public static UInt64 ExclusiveOr(UInt64 x, UInt64 y) {
            return (UInt64)(x ^ y);
        }
        [SpecialName]
        public static BigInteger ExclusiveOr(UInt64 x, Int64 y) {
            return BigIntegerOps.ExclusiveOr((BigInteger)x, (BigInteger)y);
        }
        [SpecialName]
        public static BigInteger ExclusiveOr(Int64 x, UInt64 y) {
            return BigIntegerOps.ExclusiveOr((BigInteger)x, (BigInteger)y);
        }

        // Binary Operations - Comparisons
        [SpecialName]
        public static bool LessThan(UInt64 x, UInt64 y) {
            return x < y;
        }
        [SpecialName]
        public static bool LessThan(UInt64 x, Int64 y) {
            return BigIntegerOps.LessThan((BigInteger)x, (BigInteger)y);
        }
        [SpecialName]
        public static bool LessThan(Int64 x, UInt64 y) {
            return BigIntegerOps.LessThan((BigInteger)x, (BigInteger)y);
        }
        [SpecialName]
        public static bool LessThanOrEqual(UInt64 x, UInt64 y) {
            return x <= y;
        }
        [SpecialName]
        public static bool LessThanOrEqual(UInt64 x, Int64 y) {
            return BigIntegerOps.LessThanOrEqual((BigInteger)x, (BigInteger)y);
        }
        [SpecialName]
        public static bool LessThanOrEqual(Int64 x, UInt64 y) {
            return BigIntegerOps.LessThanOrEqual((BigInteger)x, (BigInteger)y);
        }
        [SpecialName]
        public static bool GreaterThan(UInt64 x, UInt64 y) {
            return x > y;
        }
        [SpecialName]
        public static bool GreaterThan(UInt64 x, Int64 y) {
            return BigIntegerOps.GreaterThan((BigInteger)x, (BigInteger)y);
        }
        [SpecialName]
        public static bool GreaterThan(Int64 x, UInt64 y) {
            return BigIntegerOps.GreaterThan((BigInteger)x, (BigInteger)y);
        }
        [SpecialName]
        public static bool GreaterThanOrEqual(UInt64 x, UInt64 y) {
            return x >= y;
        }
        [SpecialName]
        public static bool GreaterThanOrEqual(UInt64 x, Int64 y) {
            return BigIntegerOps.GreaterThanOrEqual((BigInteger)x, (BigInteger)y);
        }
        [SpecialName]
        public static bool GreaterThanOrEqual(Int64 x, UInt64 y) {
            return BigIntegerOps.GreaterThanOrEqual((BigInteger)x, (BigInteger)y);
        }
        [SpecialName]
        public static bool Equals(UInt64 x, UInt64 y) {
            return x == y;
        }
        [SpecialName]
        public static bool Equals(UInt64 x, Int64 y) {
            return BigIntegerOps.Equals((BigInteger)x, (BigInteger)y);
        }
        [SpecialName]
        public static bool Equals(Int64 x, UInt64 y) {
            return BigIntegerOps.Equals((BigInteger)x, (BigInteger)y);
        }
        [SpecialName]
        public static bool NotEquals(UInt64 x, UInt64 y) {
            return x != y;
        }
        [SpecialName]
        public static bool NotEquals(UInt64 x, Int64 y) {
            return BigIntegerOps.NotEquals((BigInteger)x, (BigInteger)y);
        }
        [SpecialName]
        public static bool NotEquals(Int64 x, UInt64 y) {
            return BigIntegerOps.NotEquals((BigInteger)x, (BigInteger)y);
        }

        // Conversion operators
        [ExplicitConversionMethod]
        public static SByte ConvertToSByte(UInt64 x) {
            if (x <= (UInt64)SByte.MaxValue) {
                return (SByte)x;
            }
            throw Converter.CannotConvertOverflow("SByte", x);
        }
        [ExplicitConversionMethod]
        public static Byte ConvertToByte(UInt64 x) {
            if (Byte.MinValue <= x && x <= Byte.MaxValue) {
                return (Byte)x;
            }
            throw Converter.CannotConvertOverflow("Byte", x);
        }
        [ExplicitConversionMethod]
        public static Int16 ConvertToInt16(UInt64 x) {
            if (x <= (UInt64)Int16.MaxValue) {
                return (Int16)x;
            }
            throw Converter.CannotConvertOverflow("Int16", x);
        }
        [ExplicitConversionMethod]
        public static UInt16 ConvertToUInt16(UInt64 x) {
            if (UInt16.MinValue <= x && x <= UInt16.MaxValue) {
                return (UInt16)x;
            }
            throw Converter.CannotConvertOverflow("UInt16", x);
        }
        [ExplicitConversionMethod]
        public static Int32 ConvertToInt32(UInt64 x) {
            if (x <= (UInt64)Int32.MaxValue) {
                return (Int32)x;
            }
            throw Converter.CannotConvertOverflow("Int32", x);
        }
        [ExplicitConversionMethod]
        public static UInt32 ConvertToUInt32(UInt64 x) {
            if (UInt32.MinValue <= x && x <= UInt32.MaxValue) {
                return (UInt32)x;
            }
            throw Converter.CannotConvertOverflow("UInt32", x);
        }
        [ExplicitConversionMethod]
        public static Int64 ConvertToInt64(UInt64 x) {
            if (x <= (UInt64)Int64.MaxValue) {
                return (Int64)x;
            }
            throw Converter.CannotConvertOverflow("Int64", x);
        }
        [ImplicitConversionMethod]
        public static Single ConvertToSingle(UInt64 x) {
            return (Single)x;
        }
        [ImplicitConversionMethod]
        public static Double ConvertToDouble(UInt64 x) {
            return (Double)x;
        }
    }

    public static partial class SingleOps {
        [StaticExtensionMethod("__new__")]
        public static object Make(PythonType cls) {
            return Make(cls, default(Single));
        }

        [StaticExtensionMethod("__new__")]
        public static object Make(PythonType cls, object value) {
            if (cls != DynamicHelpers.GetPythonTypeFromType(typeof(Single))) {
                throw PythonOps.TypeError("Single.__new__: first argument must be Single type.");
            }
            IConvertible valueConvertible;
            if ((valueConvertible = value as IConvertible) != null) {
                switch (valueConvertible.GetTypeCode()) {
                    case TypeCode.Byte: return (Single)(Byte)value;
                    case TypeCode.SByte: return (Single)(SByte)value;
                    case TypeCode.Int16: return (Single)(Int16)value;
                    case TypeCode.UInt16: return (Single)(UInt16)value;
                    case TypeCode.Int32: return (Single)(Int32)value;
                    case TypeCode.UInt32: return (Single)(UInt32)value;
                    case TypeCode.Int64: return (Single)(Int64)value;
                    case TypeCode.UInt64: return (Single)(UInt64)value;
                    case TypeCode.Single: return (Single)(Single)value;
                    case TypeCode.Double: return (Single)(Double)value;
                }
            }
            if (value is String) {
                return Single.Parse((String)value);
            } else if (value is BigInteger) {
                return (Single)(BigInteger)value;
            } else if (value is Extensible<BigInteger>) {
                return (Single)((Extensible<BigInteger>)value).Value;
            } else if (value is Extensible<double>) {
                return (Single)((Extensible<double>)value).Value;
            }
            throw PythonOps.ValueError("invalid value for Single.__new__");
        }
        // Unary Operations
        [SpecialName]
        public static Single Plus(Single x) {
            return x;
        }
        [SpecialName]
        public static Single Negate(Single x) {
            return (Single)(-(x));
        }
        [SpecialName]
        public static Single Abs(Single x) {
            return (Single)(Math.Abs(x));
        }
        [SpecialName, PythonName("__nonzero__")]
        public static bool NonZero(Single x) {
            return (x != 0);
        }

        // Binary Operations - Arithmetic
        [SpecialName]
        public static Single Add(Single x, Single y) {
            return x + y;
        }
        [SpecialName]
        public static Single Subtract(Single x, Single y) {
            return x - y;
        }
        [SpecialName]
        public static Single Multiply(Single x, Single y) {
            return x * y;
        }
        [SpecialName]
        public static Single Divide(Single x, Single y) {
            return TrueDivide(x, y);
        }
        [SpecialName]
        public static Single TrueDivide(Single x, Single y) {
            if (y == 0) throw PythonOps.ZeroDivisionError();
            return x / y;

        }
        [SpecialName]
        public static Single FloorDivide(Single x, Single y) {
            if (y == 0) throw PythonOps.ZeroDivisionError();
            return (Single)Math.Floor(x / y);

        }

        // Binary Operations - Comparisons
        [SpecialName]
        public static bool LessThan(Single x, Single y) {
            return x < y;
        }
        [SpecialName]
        public static bool LessThanOrEqual(Single x, Single y) {
            return x <= y;
        }
        [SpecialName]
        public static bool GreaterThan(Single x, Single y) {
            return x > y;
        }
        [SpecialName]
        public static bool GreaterThanOrEqual(Single x, Single y) {
            return x >= y;
        }
        [SpecialName]
        public static bool Equals(Single x, Single y) {
            return x == y;
        }
        [SpecialName]
        public static bool NotEquals(Single x, Single y) {
            return x != y;
        }

        // Conversion operators
        [ExplicitConversionMethod]
        public static SByte ConvertToSByte(Single x) {
            if (SByte.MinValue <= x && x <= SByte.MaxValue) {
                return (SByte)x;
            }
            throw Converter.CannotConvertOverflow("SByte", x);
        }
        [ExplicitConversionMethod]
        public static Byte ConvertToByte(Single x) {
            if (Byte.MinValue <= x && x <= Byte.MaxValue) {
                return (Byte)x;
            }
            throw Converter.CannotConvertOverflow("Byte", x);
        }
        [ExplicitConversionMethod]
        public static Int16 ConvertToInt16(Single x) {
            if (Int16.MinValue <= x && x <= Int16.MaxValue) {
                return (Int16)x;
            }
            throw Converter.CannotConvertOverflow("Int16", x);
        }
        [ExplicitConversionMethod]
        public static UInt16 ConvertToUInt16(Single x) {
            if (UInt16.MinValue <= x && x <= UInt16.MaxValue) {
                return (UInt16)x;
            }
            throw Converter.CannotConvertOverflow("UInt16", x);
        }
        [ExplicitConversionMethod]
        public static Int32 ConvertToInt32(Single x) {
            if (Int32.MinValue <= x && x <= Int32.MaxValue) {
                return (Int32)x;
            }
            throw Converter.CannotConvertOverflow("Int32", x);
        }
        [ExplicitConversionMethod]
        public static UInt32 ConvertToUInt32(Single x) {
            if (UInt32.MinValue <= x && x <= UInt32.MaxValue) {
                return (UInt32)x;
            }
            throw Converter.CannotConvertOverflow("UInt32", x);
        }
        [ExplicitConversionMethod]
        public static Int64 ConvertToInt64(Single x) {
            if (Int64.MinValue <= x && x <= Int64.MaxValue) {
                return (Int64)x;
            }
            throw Converter.CannotConvertOverflow("Int64", x);
        }
        [ExplicitConversionMethod]
        public static UInt64 ConvertToUInt64(Single x) {
            if (UInt64.MinValue <= x && x <= UInt64.MaxValue) {
                return (UInt64)x;
            }
            throw Converter.CannotConvertOverflow("UInt64", x);
        }
        [ImplicitConversionMethod]
        public static Double ConvertToDouble(Single x) {
            return (Double)x;
        }
    }

    public static partial class DoubleOps {
        // Unary Operations
        [SpecialName]
        public static Double Plus(Double x) {
            return x;
        }
        [SpecialName]
        public static Double Negate(Double x) {
            return (Double)(-(x));
        }
        [SpecialName]
        public static Double Abs(Double x) {
            return (Double)(Math.Abs(x));
        }
        [SpecialName, PythonName("__nonzero__")]
        public static bool NonZero(Double x) {
            return (x != 0);
        }

        // Binary Operations - Arithmetic
        [SpecialName]
        public static Double Add(Double x, Double y) {
            return x + y;
        }
        [SpecialName]
        public static Double Subtract(Double x, Double y) {
            return x - y;
        }
        [SpecialName]
        public static Double Multiply(Double x, Double y) {
            return x * y;
        }
        [SpecialName]
        public static Double Divide(Double x, Double y) {
            return TrueDivide(x, y);
        }
        [SpecialName]
        public static Double TrueDivide(Double x, Double y) {
            if (y == 0) throw PythonOps.ZeroDivisionError();
            return x / y;

        }
        [SpecialName]
        public static Double FloorDivide(Double x, Double y) {
            if (y == 0) throw PythonOps.ZeroDivisionError();
            return (Double)Math.Floor(x / y);

        }

        // Binary Operations - Comparisons
        [SpecialName]
        public static bool LessThan(Double x, Double y) {
            return x < y;
        }
        [SpecialName]
        public static bool LessThanOrEqual(Double x, Double y) {
            return x <= y;
        }
        [SpecialName]
        public static bool GreaterThan(Double x, Double y) {
            return x > y;
        }
        [SpecialName]
        public static bool GreaterThanOrEqual(Double x, Double y) {
            return x >= y;
        }
        [SpecialName]
        public static bool Equals(Double x, Double y) {
            return x == y;
        }
        [SpecialName]
        public static bool NotEquals(Double x, Double y) {
            return x != y;
        }

        // Conversion operators
        [ExplicitConversionMethod]
        public static SByte ConvertToSByte(Double x) {
            if (SByte.MinValue <= x && x <= SByte.MaxValue) {
                return (SByte)x;
            }
            throw Converter.CannotConvertOverflow("SByte", x);
        }
        [ExplicitConversionMethod]
        public static Byte ConvertToByte(Double x) {
            if (Byte.MinValue <= x && x <= Byte.MaxValue) {
                return (Byte)x;
            }
            throw Converter.CannotConvertOverflow("Byte", x);
        }
        [ExplicitConversionMethod]
        public static Int16 ConvertToInt16(Double x) {
            if (Int16.MinValue <= x && x <= Int16.MaxValue) {
                return (Int16)x;
            }
            throw Converter.CannotConvertOverflow("Int16", x);
        }
        [ExplicitConversionMethod]
        public static UInt16 ConvertToUInt16(Double x) {
            if (UInt16.MinValue <= x && x <= UInt16.MaxValue) {
                return (UInt16)x;
            }
            throw Converter.CannotConvertOverflow("UInt16", x);
        }
        [ExplicitConversionMethod]
        public static Int32 ConvertToInt32(Double x) {
            if (Int32.MinValue <= x && x <= Int32.MaxValue) {
                return (Int32)x;
            }
            throw Converter.CannotConvertOverflow("Int32", x);
        }
        [ExplicitConversionMethod]
        public static UInt32 ConvertToUInt32(Double x) {
            if (UInt32.MinValue <= x && x <= UInt32.MaxValue) {
                return (UInt32)x;
            }
            throw Converter.CannotConvertOverflow("UInt32", x);
        }
        [ExplicitConversionMethod]
        public static Int64 ConvertToInt64(Double x) {
            if (Int64.MinValue <= x && x <= Int64.MaxValue) {
                return (Int64)x;
            }
            throw Converter.CannotConvertOverflow("Int64", x);
        }
        [ExplicitConversionMethod]
        public static UInt64 ConvertToUInt64(Double x) {
            if (UInt64.MinValue <= x && x <= UInt64.MaxValue) {
                return (UInt64)x;
            }
            throw Converter.CannotConvertOverflow("UInt64", x);
        }
        [ImplicitConversionMethod]
        public static Single ConvertToSingle(Double x) {
            return (Single)x;
        }
    }


    // *** END GENERATED CODE ***

    #endregion
}
