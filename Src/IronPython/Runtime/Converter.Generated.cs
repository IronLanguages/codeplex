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
using System.Collections.Generic;
using System.Text;

using IronMath;

namespace IronPython.Runtime {
    public static partial class Converter {



        #region Generated conversion routines

        // *** BEGIN GENERATED CODE ***

        //
        // "Try" conversion methods
        //
        public static int TryConvertToInt32(object value, out Conversion conversion) {
            ExtensibleInt ei;
            BigInteger bi;
            Enum e;
            ExtensibleFloat ef;
            ExtensibleLong el;
            if (value is int) {
                conversion = Conversion.Identity;
                return (int)value;
            } else if (!Object.Equals((ei = value as ExtensibleInt), null)) {
                conversion = Conversion.Implicit;
                return ei.value;
            } else if (value is double) {
                double DoubleVal = (double)value;
                if (DoubleVal >= Int32.MinValue && DoubleVal <= Int32.MaxValue) {
                    conversion = Conversion.Truncation;
                    return (int)DoubleVal;
                }
            } else if (!Object.Equals((bi = value as BigInteger), null)) {
                int res;
                if (bi.AsInt32(out res)) {
                    conversion = Conversion.Implicit;
                    return res;
                }
            } else if (value is bool) {
                conversion = Conversion.NonStandard;
                return ((bool)value) ? 1 : 0;
            } else if (value is char) {
                conversion = Conversion.Implicit;
                return (int)(char)value;
            } else if (value is byte) {
                conversion = Conversion.Implicit;
                return (int)(byte)value;
            } else if (value is sbyte) {
                conversion = Conversion.Implicit;
                return (int)(sbyte)value;
            } else if (value is short) {
                conversion = Conversion.Implicit;
                return (int)(short)value;
            } else if (value is uint) {
                uint UInt32Val = (uint)value;
                if (UInt32Val <= Int32.MaxValue) {
                    conversion = Conversion.Implicit;
                    return (int)UInt32Val;
                }
            } else if (value is ulong) {
                ulong UInt64Val = (ulong)value;
                if (UInt64Val <= Int32.MaxValue) {
                    conversion = Conversion.Implicit;
                    return (int)UInt64Val;
                }
            } else if (value is ushort) {
                conversion = Conversion.Implicit;
                return (int)(ushort)value;
            } else if (!Object.Equals((e = value as Enum), null)) {
                return TryConvertEnumToInt32(e, out conversion);
            } else if (!Object.Equals((ef = value as ExtensibleFloat), null)) {
                if (/*ExtensibleFloatVal*/ ef.value >= Int32.MinValue &&
                    /*ExtensibleFloatVal*/ef.value <= Int32.MaxValue) {
                    conversion = Conversion.Truncation;
                    return (int)ef.value;
                }
            } else if (!Object.Equals((el = value as ExtensibleLong), null)) {
                int res;
                if (el.Value.AsInt32(out res)) {
                    conversion = Conversion.Implicit;
                    return res;
                }
            } else if (value is decimal) {
                decimal DecimalVal = (decimal)value;
                if (DecimalVal <= Int32.MaxValue) {
                    conversion = Conversion.Implicit;
                    return (int)DecimalVal;
                }
            } else if (value is long) {
                long Int64Val = (long)value;
                if (Int64Val >= Int32.MinValue && Int64Val <= Int32.MaxValue) {
                    conversion = Conversion.Implicit;
                    return (int)Int64Val;
                }
            } else {
                object ObjectVal;
                if (Ops.TryToInvoke(value, SymbolTable.ConvertToInt, out ObjectVal)) {
                    conversion = Conversion.Eval;
                    if (ObjectVal is int)
                        return (int)ObjectVal;
                    if (ObjectVal is double)
                        return (int)(double)ObjectVal;
                    if (ObjectVal is long)
                        return (int)(long)ObjectVal;
                    if (ObjectVal is BigInteger)
                        return (int)(BigInteger)ObjectVal;
                    throw Ops.TypeError("__int__ returned non-int");
                }
            }
            conversion = Conversion.None;
            return (int)0;
        }

        public static string TryConvertToString(object value, out Conversion conversion) {
            string str;
            ExtensibleString es;
            if (!Object.Equals((str = value as string), null)) {
                conversion = Conversion.Identity;
                return str;
            } else if (value == null) {
                conversion = Conversion.Identity;
                return null;
            } else if (value is char) {
                conversion = Conversion.Identity;
                return Ops.Char2String((char)value);
            } else if (!Object.Equals((es = value as ExtensibleString), null)) {
                conversion = Conversion.Implicit;
                return es.Value;
            }
            conversion = Conversion.None;
            return (string)String.Empty;
        }

        public static double TryConvertToDouble(object value, out Conversion conversion) {
            ExtensibleInt ei;
            BigInteger bi;
            ExtensibleFloat ef;
            ExtensibleLong el;
            if (value is double) {
                conversion = Conversion.Identity;
                return (double)value;
            } else if (value is int) {
                conversion = Conversion.Implicit;
                return (double)(int)value;
            } else if (!Object.Equals((ei = value as ExtensibleInt), null)) {
                conversion = Conversion.Implicit;
                return (double)ei.value;
            } else if (!Object.Equals((bi = value as BigInteger), null)) {
                conversion = Conversion.Implicit;
                double res;
                if (bi.TryToFloat64(out res)) return res;
            } else if (value is bool) {
                conversion = Conversion.NonStandard;
                return ((bool)value) ? 1.0 : 0.0;
            } else if (value is char) {
                conversion = Conversion.Implicit;
                return (double)(char)value;
            } else if (value is byte) {
                conversion = Conversion.Implicit;
                return (double)(byte)value;
            } else if (value is sbyte) {
                conversion = Conversion.Implicit;
                return (double)(sbyte)value;
            } else if (value is short) {
                conversion = Conversion.Implicit;
                return (double)(short)value;
            } else if (value is uint) {
                conversion = Conversion.Implicit;
                return (double)(uint)value;
            } else if (value is ulong) {
                conversion = Conversion.Implicit;
                return (double)(ulong)value;
            } else if (value is ushort) {
                conversion = Conversion.Implicit;
                return (double)(ushort)value;
            } else if (value is float) {
                conversion = Conversion.Implicit;
                return (double)(float)value;
            } else if (!Object.Equals((ef = value as ExtensibleFloat), null)) {
                conversion = Conversion.Implicit;
                return (double)ef.value;
            } else if (!Object.Equals((el = value as ExtensibleLong), null)) {
                double res;
                if (el.Value.TryToFloat64(out res)) {
                    conversion = Conversion.Implicit;
                    return res;
                }
            } else if (value is long) {
                conversion = Conversion.Implicit;
                return (double)(long)value;
            } else if (value is decimal) {
                conversion = Conversion.Truncation;
                return (double)(decimal)value;
            } else {
                object ObjectVal;
                if (Ops.TryToInvoke(value, SymbolTable.ConvertToFloat, out ObjectVal)) {
                    conversion = Conversion.Eval;
                    if (ObjectVal is double)
                        return (double)ObjectVal;
                    if (ObjectVal is int)
                        return (double)(int)ObjectVal;
                    if (ObjectVal is long)
                        return (double)(long)ObjectVal;
                    if (ObjectVal is BigInteger)
                        return (double)(BigInteger)ObjectVal;
                    throw Ops.TypeError("__float__ returned non-float");
                }
            }
            conversion = Conversion.None;
            return (double)0.0;
        }

        public static BigInteger TryConvertToBigInteger(object value, out Conversion conversion) {
            BigInteger bi;
            ExtensibleLong el;
            if (!Object.Equals((bi = value as BigInteger), null)) {
                conversion = Conversion.Identity;
                return bi;
            } else if (value is int) {
                conversion = Conversion.Identity;
                return (BigInteger)(int)value;
            } else if (!Object.Equals((el = value as ExtensibleLong), null)) {
                conversion = Conversion.Implicit;
                return el.Value;
            } else if (value is long) {
                conversion = Conversion.Identity;
                return (BigInteger)(long)value;
            } else {
                object ObjectVal;
                if (Ops.TryToInvoke(value, SymbolTable.ConvertToLong, out ObjectVal)) {
                    conversion = Conversion.Eval;
                    if (ObjectVal is BigInteger)
                        return (BigInteger)ObjectVal;
                    if (ObjectVal is int)
                        return BigInteger.Create((int)ObjectVal);
                    if (ObjectVal is double)
                        return BigInteger.Create((double)ObjectVal);
                    if (ObjectVal is long)
                        return BigInteger.Create((long)ObjectVal);
                    throw Ops.TypeError("__long__ returned non-long");
                }
            }
            int Int32Val = TryConvertToInt32(value, out conversion);
            if (conversion != Conversion.None) {
                return BigInteger.Create(Int32Val);
            }
            conversion = Conversion.None;
            return (BigInteger)BigInteger.Zero;
        }

        public static bool TryConvertToBoolean(object value, out Conversion conversion) {
            BigInteger bi;
            Enum e;
            string str;
            ExtensibleInt ei;
            ExtensibleLong el;

            if (value == null) {
                conversion = Conversion.None;
                return false;
            } else if (value is bool) {
                conversion = Conversion.Identity;
                return (bool)value;
            } else if (value is int) {
                conversion = Conversion.NonStandard;
                return (int)value != 0;
            } else if (!Object.Equals((str = value as string), null)) {
                conversion = Conversion.NonStandard;
                return str.Length != 0;
            } else if (value is double) {
                conversion = Conversion.NonStandard;
                return (double)value != 0.0;
            } else if (!Object.Equals((bi = value as BigInteger), null)) {
                conversion = Conversion.Implicit;
                return bi != BigInteger.Zero;
            } else if (value is Complex64) {
                conversion = Conversion.NonStandard;
                return !((Complex64)value).IsZero;
            } else if (value is char) {
                conversion = Conversion.NonStandard;
                return (char)value != 0;
            } else if (value is byte) {
                conversion = Conversion.NonStandard;
                return (byte)value != 0;
            } else if (value is sbyte) {
                conversion = Conversion.NonStandard;
                return (sbyte)value != 0;
            } else if (value is short) {
                conversion = Conversion.NonStandard;
                return (short)value != 0;
            } else if (value is uint) {
                conversion = Conversion.NonStandard;
                return (uint)value != 0;
            } else if (value is ulong) {
                conversion = Conversion.NonStandard;
                return (ulong)value != 0;
            } else if (value is ushort) {
                conversion = Conversion.NonStandard;
                return (ushort)value != 0;
            } else if (!Object.Equals((e = value as Enum), null)) {
                return TryConvertEnumToBoolean(e, out conversion);
            } else if (value is float) {
                conversion = Conversion.NonStandard;
                return (float)value != 0.0;
            } else if (value is decimal) {
                conversion = Conversion.NonStandard;
                return (decimal)value != 0;
            } else if (value is long) {
                conversion = Conversion.NonStandard;
                return (long)value != 0;
            } else if (value is IPythonContainer) {
                conversion = Conversion.Eval;
                return ((IPythonContainer)value).GetLength() != 0;
            } else if (value is ICollection) {
                conversion = Conversion.Eval;
                return ((ICollection)value).Count != 0;
            }
            object ret;
            // try __nonzero__ first before __len__
            if (Ops.TryToInvoke(value, SymbolTable.NonZero, out ret)) {
                conversion = Conversion.Eval;
                Type retType = ret.GetType();
                if (retType == typeof(bool) || retType == typeof(int)) {
                    Conversion dummy;
                    return TryConvertToBoolean(ret, out dummy);
                }
                else throw Ops.TypeError("__nonzero__ should return bool or int, returned {0}", Ops.GetClassName(ret));
            } else if (Ops.TryToInvoke(value, SymbolTable.Length, out ret)) {
                conversion = Conversion.Eval;
                Type retType = ret.GetType();
                if (retType == typeof(bool) || retType == typeof(int)) {
                    Conversion dummy;
                    return TryConvertToBoolean(ret, out dummy);
                }
                else throw Ops.TypeError("an integer is required");
            } else if (!Object.Equals((ei = value as ExtensibleInt), null)) {
                conversion = Conversion.NonStandard;
                return ei.value != 0;
            } else if (!Object.Equals((el = value as ExtensibleLong), null)) {
                conversion = Conversion.NonStandard;
                return el.Value != 0;
            }
            conversion = Conversion.NonStandard;
            return (bool)true;
        }

        public static Complex64 TryConvertToComplex64(object value, out Conversion conversion) {
            ExtensibleComplex ec;
            if (value is Complex64) {
                conversion = Conversion.Identity;
                return (Complex64)value;
            } else if (value is double) {
                conversion = Conversion.Implicit;
                return (Complex64)Complex64.MakeReal((double)value);
            } else if (!Object.Equals((ec = value as ExtensibleComplex), null)) {
                conversion = Conversion.Implicit;
                return ec.value;
            } else {
                object ObjectVal;
                if (Ops.TryToInvoke(value, SymbolTable.ConvertToComplex, out ObjectVal)) {
                    conversion = Conversion.Eval;
                    if (ObjectVal is Complex64)
                        return (Complex64)ObjectVal;
                    if (ObjectVal is int)
                        return (Complex64)(int)ObjectVal;
                    if (ObjectVal is double)
                        return (Complex64)(double)ObjectVal;
                    if (ObjectVal is long)
                        return (Complex64)(long)ObjectVal;
                    if (ObjectVal is BigInteger)
                        return (Complex64)(BigInteger)ObjectVal;
                    throw Ops.TypeError("__complex__ returned non-complex");
                }
            }
            double DoubleVal = TryConvertToDouble(value, out conversion);
            if (conversion != Conversion.None) {
                return Complex64.MakeReal(DoubleVal);
            }
            conversion = Conversion.None;
            return (Complex64)new Complex64(0.0);
        }

        public static char TryConvertToChar(object value, out Conversion conversion) {
            string str;
            if (value is char) {
                conversion = Conversion.Identity;
                return (char)value;
            } else if (value is int) {
                int Int32Val = (int)value;
                if (Int32Val >= 0 && Int32Val <= 0xFFFF) {
                    conversion = Conversion.Implicit;
                    return (char)Int32Val;
                }
            } else if (!Object.Equals((str = value as string), null)) {
                if (/*StringVal*/str.Length == 1) {
                    conversion = Conversion.Implicit;
                    return (char)str[0];
                }
            } else if (value is sbyte) {
                sbyte SByteVal = (sbyte)value;
                if (SByteVal >= 0) {
                    conversion = Conversion.Implicit;
                    return (char)SByteVal;
                }
            } else if (value is short) {
                short Int16Val = (short)value;
                if (Int16Val >= 0) {
                    conversion = Conversion.Implicit;
                    return (char)Int16Val;
                }
            } else if (value is uint) {
                uint UInt32Val = (uint)value;
                if (UInt32Val <= 0xFFFF) {
                    conversion = Conversion.Implicit;
                    return (char)UInt32Val;
                }
            } else if (value is ulong) {
                ulong UInt64Val = (ulong)value;
                if (UInt64Val <= 0xFFFF) {
                    conversion = Conversion.Implicit;
                    return (char)UInt64Val;
                }
            } else if (value is decimal) {
                decimal DecimalVal = (decimal)value;
                if (DecimalVal >= 0 && DecimalVal <= 0xFFFF) {
                    conversion = Conversion.Implicit;
                    return (char)DecimalVal;
                }
            } else if (value is long) {
                long Int64Val = (long)value;
                if (Int64Val >= 0 && Int64Val <= 0xFFFF) {
                    conversion = Conversion.Implicit;
                    return (char)Int64Val;
                }
            } else if (value is byte) {
                conversion = Conversion.Truncation;
                return (char)(byte)value;
            } else if (value is ushort) {
                conversion = Conversion.Truncation;
                return (char)(ushort)value;
            }
            conversion = Conversion.None;
            return (char)'\0';
        }

        public static IEnumerator TryConvertToIEnumerator(object value, out Conversion conversion) {
            IEnumerator ie = value as IEnumerator;
            if (ie != null || value == null) {
                conversion = Conversion.Identity;
                return ie;
            }
            conversion = Conversion.Eval;
            return Ops.GetEnumerator(value);
        }

        public static Type TryConvertToType(object value, out Conversion conversion) {
            Type TypeVal = value as Type;
            if (TypeVal != null || value == null) {
                conversion = Conversion.Identity;
                return TypeVal;
            }
            PythonType PythonTypeVal = value as PythonType;
            if (PythonTypeVal != null) {
                conversion = Conversion.Implicit;
                return PythonTypeVal.type;
            }
            conversion = Conversion.None;
            return (Type)null;
        }

        public static byte TryConvertToByte(object value, out Conversion conversion) {
            BigInteger bi;
            Enum e;
            ExtensibleLong el;
            if (value is byte) {
                conversion = Conversion.Identity;
                return (byte)value;
            } else if (value is int) {
                int Int32Val = (int)value;
                if (Int32Val >= Byte.MinValue &&
                    Int32Val <= Byte.MaxValue) {
                    conversion = Conversion.Implicit;
                    return (byte)Int32Val;
                }
            } else if (!Object.Equals((bi = value as BigInteger), null)) {
                if (bi >= BigInteger.Create(Byte.MinValue) &&
                    bi <= BigInteger.Create(Byte.MaxValue)) {
                    conversion = Conversion.Implicit;
                    return (byte)(int)bi;
                }
            } else if (value is char) {
                char CharVal = (char)value;
                if (CharVal >= Byte.MinValue) {
                    conversion = Conversion.Implicit;
                    return (byte)CharVal;
                }
            } else if (value is sbyte) {
                sbyte SByteVal = (sbyte)value;
                if (SByteVal >= Byte.MinValue) {
                    conversion = Conversion.Implicit;
                    return (byte)SByteVal;
                }
            } else if (value is short) {
                short Int16Val = (short)value;
                if (Int16Val >= Byte.MinValue && Int16Val <= Byte.MaxValue) {
                    conversion = Conversion.Implicit;
                    return (byte)Int16Val;
                }
            } else if (value is uint) {
                uint UInt32Val = (uint)value;
                if (UInt32Val <= Byte.MaxValue) {
                    conversion = Conversion.Implicit;
                    return (byte)UInt32Val;
                }
            } else if (value is ulong) {
                ulong UInt64Val = (ulong)value;
                if (UInt64Val <= Byte.MaxValue) {
                    conversion = Conversion.Implicit;
                    return (byte)UInt64Val;
                }
            } else if (value is ushort) {
                ushort UInt16Val = (ushort)value;
                if (UInt16Val <= Byte.MaxValue) {
                    conversion = Conversion.Implicit;
                    return (byte)UInt16Val;
                }
            } else if (!Object.Equals((e = value as Enum), null)) {
                return TryConvertEnumToByte(e, out conversion);
            } else if (!Object.Equals((el = value as ExtensibleLong), null)) {
                if (el.Value >= BigInteger.Create(Byte.MinValue) &&
                    el.Value <= BigInteger.Create(Byte.MaxValue)) {
                    conversion = Conversion.Implicit;
                    return (byte)(int)el.Value;
                }
            } else if (value is decimal) {
                decimal DecimalVal = (decimal)value;
                if (DecimalVal >= Byte.MinValue && DecimalVal <= Byte.MaxValue) {
                    conversion = Conversion.Implicit;
                    return (byte)DecimalVal;
                }
            } else if (value is long) {
                long Int64Val = (long)value;
                if (Int64Val >= Byte.MinValue && Int64Val <= Byte.MaxValue) {
                    conversion = Conversion.Implicit;
                    return (byte)Int64Val;
                }
            } else if (value is bool) {
                conversion = Conversion.Truncation;
                return ((bool)value) ? (byte)1 : (byte)0;
            }
            conversion = Conversion.None;
            return (byte)0;
        }

        public static sbyte TryConvertToSByte(object value, out Conversion conversion) {
            BigInteger bi;
            Enum e;
            ExtensibleLong el;
            if (value is sbyte) {
                conversion = Conversion.Identity;
                return (sbyte)value;
            } else if (value is int) {
                int Int32Val = (int)value;
                if (Int32Val >= SByte.MinValue &&
                    Int32Val <= SByte.MaxValue) {
                    conversion = Conversion.Implicit;
                    return (sbyte)Int32Val;
                }
            } else if (!Object.Equals((bi = value as BigInteger), null)) {
                if (bi >= BigInteger.Create(SByte.MinValue) &&
                    bi <= BigInteger.Create(SByte.MaxValue)) {
                    conversion = Conversion.Implicit;
                    return (sbyte)(int)bi;
                }
            } else if (value is char) {
                char CharVal = (char)value;
                if (CharVal <= '\xFF') {
                    conversion = Conversion.Implicit;
                    return (sbyte)CharVal;
                }
            } else if (value is byte) {
                byte ByteVal = (byte)value;
                if (ByteVal <= SByte.MaxValue) {
                    conversion = Conversion.Implicit;
                    return (sbyte)ByteVal;
                }
            } else if (value is short) {
                short Int16Val = (short)value;
                if (Int16Val >= SByte.MinValue && Int16Val <= SByte.MaxValue) {
                    conversion = Conversion.Implicit;
                    return (sbyte)Int16Val;
                }
            } else if (value is uint) {
                uint UInt32Val = (uint)value;
                if (UInt32Val <= SByte.MaxValue) {
                    conversion = Conversion.Implicit;
                    return (sbyte)UInt32Val;
                }
            } else if (value is ulong) {
                ulong UInt64Val = (ulong)value;
                if (UInt64Val <= (ulong)SByte.MaxValue) {
                    conversion = Conversion.Implicit;
                    return (sbyte)UInt64Val;
                }
            } else if (value is ushort) {
                ushort UInt16Val = (ushort)value;
                if (UInt16Val <= SByte.MaxValue) {
                    conversion = Conversion.Implicit;
                    return (sbyte)UInt16Val;
                }
            } else if (!Object.Equals((e = value as Enum), null)) {
                return TryConvertEnumToSByte(e, out conversion);
            } else if (!Object.Equals((el = value as ExtensibleLong), null)) {
                if (el.Value >= BigInteger.Create(SByte.MinValue) &&
                    el.Value <= BigInteger.Create(SByte.MaxValue)) {
                    conversion = Conversion.Implicit;
                    return (sbyte)(int)el.Value;
                }
            } else if (value is decimal) {
                decimal DecimalVal = (decimal)value;
                if (DecimalVal >= SByte.MinValue && DecimalVal <= SByte.MaxValue) {
                    conversion = Conversion.Implicit;
                    return (sbyte)DecimalVal;
                }
            } else if (value is long) {
                long Int64Val = (long)value;
                if (Int64Val >= SByte.MinValue && Int64Val <= SByte.MaxValue) {
                    conversion = Conversion.Implicit;
                    return (sbyte)Int64Val;
                }
            } else if (value is bool) {
                conversion = Conversion.Truncation;
                return ((bool)value) ? (sbyte)1 : (sbyte)0;
            }
            conversion = Conversion.None;
            return (sbyte)0;
        }

        public static short TryConvertToInt16(object value, out Conversion conversion) {
            BigInteger bi;
            Enum e;
            ExtensibleLong el;
            if (value is short) {
                conversion = Conversion.Identity;
                return (short)value;
            } else if (value is int) {
                int Int32Val = (int)value;
                if (Int32Val >= Int16.MinValue && Int32Val <= Int16.MaxValue) {
                    conversion = Conversion.Implicit;
                    return (short)Int32Val;
                }
            } else if (!Object.Equals((bi = value as BigInteger), null)) {
                if (bi >= BigInteger.Create(Int16.MinValue) &&
                    bi <= BigInteger.Create(Int16.MaxValue)) {
                    conversion = Conversion.Implicit;
                    return (short)(int)bi;
                }
            } else if (value is bool) {
                conversion = Conversion.NonStandard;
                return ((bool)value) ? (short)1 : (short)0;
            } else if (value is char) {
                char CharVal = (char)value;
                if (CharVal <= '\x7FFF') {
                    conversion = Conversion.Implicit;
                    return (short)CharVal;
                }
            } else if (value is byte) {
                conversion = Conversion.Implicit;
                return (short)(byte)value;
            } else if (value is sbyte) {
                conversion = Conversion.Implicit;
                return (short)(sbyte)value;
            } else if (value is uint) {
                uint UInt32Val = (uint)value;
                if (UInt32Val <= Int16.MaxValue) {
                    conversion = Conversion.Implicit;
                    return (short)UInt32Val;
                }
            } else if (value is ulong) {
                ulong UInt64Val = (ulong)value;
                if (UInt64Val <= (ulong)Int16.MaxValue) {
                    conversion = Conversion.Implicit;
                    return (short)UInt64Val;
                }
            } else if (value is ushort) {
                ushort UInt16Val = (ushort)value;
                if (UInt16Val <= Int16.MaxValue) {
                    conversion = Conversion.Implicit;
                    return (short)UInt16Val;
                }
            } else if (!Object.Equals((e = value as Enum), null)) {
                return TryConvertEnumToInt16(e, out conversion);
            } else if (!Object.Equals((el = value as ExtensibleLong), null)) {
                if (el.Value >= BigInteger.Create(Int16.MinValue) &&
                    el.Value <= BigInteger.Create(Int16.MaxValue)) {
                    conversion = Conversion.Implicit;
                    return (short)(int)el.Value;
                }
            } else if (value is decimal) {
                decimal DecimalVal = (decimal)value;
                if (DecimalVal >= Int16.MinValue && DecimalVal <= Int16.MaxValue) {
                    conversion = Conversion.Implicit;
                    return (short)DecimalVal;
                }
            } else if (value is long) {
                long Int64Val = (long)value;
                if (Int64Val >= Int16.MinValue && Int64Val <= Int16.MaxValue) {
                    conversion = Conversion.Implicit;
                    return (short)Int64Val;
                }
            }
            conversion = Conversion.None;
            return (short)0;
        }

        public static uint TryConvertToUInt32(object value, out Conversion conversion) {
            ExtensibleInt ei;
            BigInteger bi;
            Enum e;
            ExtensibleLong el;
            if (value is uint) {
                conversion = Conversion.Identity;
                return (uint)value;
            } else if (value is int) {
                int Int32Val = (int)value;
                if (Int32Val >= UInt32.MinValue) {
                    conversion = Conversion.Implicit;
                    return (uint)Int32Val;
                }
            } else if (!Object.Equals((ei = value as ExtensibleInt), null)) {
                conversion = Conversion.Implicit;
                return (uint)ei.value;
            } else if (!Object.Equals((bi = value as BigInteger), null)) {
                uint res;
                if (bi.AsUInt32(out res)) {
                    conversion = Conversion.Implicit;
                    return res;
                }
            } else if (value is bool) {
                conversion = Conversion.NonStandard;
                return ((bool)value) ? 1u : 0u;
            } else if (value is char) {
                conversion = Conversion.Implicit;
                return (uint)(char)value;
            } else if (value is byte) {
                conversion = Conversion.Implicit;
                return (uint)(byte)value;
            } else if (value is sbyte) {
                sbyte SByteVal = (sbyte)value;
                if (SByteVal >= 0) {
                    conversion = Conversion.Implicit;
                    return (uint)SByteVal;
                }
            } else if (value is short) {
                short Int16Val = (short)value;
                if (Int16Val >= 0) {
                    conversion = Conversion.Implicit;
                    return (uint)Int16Val;
                }
            } else if (value is ulong) {
                ulong UInt64Val = (ulong)value;
                if (UInt64Val <= UInt32.MaxValue) {
                    conversion = Conversion.Implicit;
                    return (uint)UInt64Val;
                }
            } else if (value is ushort) {
                conversion = Conversion.Implicit;
                return (uint)(ushort)value;
            } else if (!Object.Equals((e = value as Enum), null)) {
                return TryConvertEnumToUInt32(e, out conversion);
            } else if (!Object.Equals((el = value as ExtensibleLong), null)) {
                uint res;
                if (el.Value.AsUInt32(out res)) {
                    conversion = Conversion.Implicit;
                    return res;
                }
            } else if (value is decimal) {
                decimal DecimalVal = (decimal)value;
                if (DecimalVal >= UInt32.MinValue && DecimalVal <= UInt32.MaxValue) {
                    conversion = Conversion.Implicit;
                    return (uint)DecimalVal;
                }
            } else if (value is long) {
                long Int64Val = (long)value;
                if (Int64Val >= UInt32.MinValue && Int64Val <= UInt32.MaxValue) {
                    conversion = Conversion.Implicit;
                    return (uint)Int64Val;
                }
            }
            conversion = Conversion.None;
            return (uint)0;
        }

        public static ulong TryConvertToUInt64(object value, out Conversion conversion) {
            ExtensibleInt ei;
            BigInteger bi;
            Enum e;
            ExtensibleLong el;
            if (value is ulong) {
                conversion = Conversion.Identity;
                return (ulong)value;
            } else if (value is int) {
                int Int32Val = (int)value;
                if (Int32Val >= 0) {
                    conversion = Conversion.Implicit;
                    return (ulong)Int32Val;
                }
            } else if (!Object.Equals((ei = value as ExtensibleInt), null)) {
                if (ei.value >= 0) {
                    conversion = Conversion.Implicit;
                    return (ulong)ei.value;
                }
            } else if (!Object.Equals((bi = value as BigInteger), null)) {
                ulong res;
                if (bi.AsUInt64(out res)) {
                    conversion = Conversion.Implicit;
                    return res;
                }
            } else if (value is bool) {
                conversion = Conversion.NonStandard;
                return ((bool)value) ? 1ul : 0ul;
            } else if (value is char) {
                conversion = Conversion.Implicit;
                return (ulong)(char)value;
            } else if (value is byte) {
                conversion = Conversion.Implicit;
                return (ulong)(byte)value;
            } else if (value is sbyte) {
                sbyte SByteVal = (sbyte)value;
                if (SByteVal >= 0) {
                    conversion = Conversion.Implicit;
                    return (ulong)SByteVal;
                }
            } else if (value is short) {
                short Int16Val = (short)value;
                if (Int16Val >= 0) {
                    conversion = Conversion.Implicit;
                    return (ulong)Int16Val;
                }
            } else if (value is uint) {
                conversion = Conversion.Implicit;
                return (ulong)(uint)value;
            } else if (value is ushort) {
                conversion = Conversion.Implicit;
                return (ulong)(ushort)value;
            } else if (!Object.Equals((e = value as Enum), null)) {
                return TryConvertEnumToUInt64(e, out conversion);
            } else if (!Object.Equals((el = value as ExtensibleLong), null)) {
                ulong res;
                if (el.Value.AsUInt64(out res)) {
                    conversion = Conversion.Implicit;
                    return res;
                }
            } else if (value is decimal) {
                decimal DecimalVal = (decimal)value;
                if (DecimalVal >= UInt64.MinValue && DecimalVal <= UInt64.MaxValue) {
                    conversion = Conversion.Implicit;
                    return (ulong)DecimalVal;
                }
            } else if (value is long) {
                long Int64Val = (long)value;
                if (Int64Val >= 0) {
                    conversion = Conversion.Implicit;
                    return (ulong)Int64Val;
                }
            }
            conversion = Conversion.None;
            return (ulong)0;
        }

        public static ushort TryConvertToUInt16(object value, out Conversion conversion) {
            BigInteger bi;
            Enum e;
            ExtensibleLong el;
            if (value is ushort) {
                conversion = Conversion.Identity;
                return (ushort)value;
            } else if (value is int) {
                int Int32Val = (int)value;
                if (Int32Val >= UInt16.MinValue && Int32Val <= UInt16.MaxValue) {
                    conversion = Conversion.Implicit;
                    return (ushort)Int32Val;
                }
            } else if (!Object.Equals((bi = value as BigInteger), null)) {
                if (bi >= BigInteger.Create(UInt16.MinValue) &&
                    bi <= BigInteger.Create(UInt16.MaxValue)) {
                    conversion = Conversion.Implicit;
                    return (ushort)(int)bi;
                }
            } else if (value is bool) {
                conversion = Conversion.NonStandard;
                return ((bool)value) ? (ushort)1 : (ushort)0;
            } else if (value is char) {
                conversion = Conversion.Implicit;
                return (ushort)(char)value;
            } else if (value is byte) {
                conversion = Conversion.Implicit;
                return (ushort)(byte)value;
            } else if (value is sbyte) {
                sbyte SByteVal = (sbyte)value;
                if (SByteVal >= 0) {
                    conversion = Conversion.Implicit;
                    return (ushort)SByteVal;
                }
            } else if (value is short) {
                short Int16Val = (short)value;
                if (Int16Val >= 0) {
                    conversion = Conversion.Implicit;
                    return (ushort)Int16Val;
                }
            } else if (value is uint) {
                uint UInt32Val = (uint)value;
                if (UInt32Val <= UInt16.MaxValue) {
                    conversion = Conversion.Implicit;
                    return (ushort)UInt32Val;
                }
            } else if (value is ulong) {
                ulong UInt64Val = (ulong)value;
                if (UInt64Val <= UInt16.MaxValue) {
                    conversion = Conversion.Implicit;
                    return (ushort)UInt64Val;
                }
            } else if (!Object.Equals((e = value as Enum), null)) {
                return TryConvertEnumToUInt16(e, out conversion);
            } else if (!Object.Equals((el = value as ExtensibleLong), null)) {
                if (el.Value >= BigInteger.Create(UInt16.MinValue) &&
                    el.Value <= BigInteger.Create(UInt16.MaxValue)) {
                    conversion = Conversion.Implicit;
                    return (ushort)(int)bi;
                }
            } else if (value is decimal) {
                decimal DecimalVal = (decimal)value;
                if (DecimalVal >= UInt16.MinValue && DecimalVal <= UInt16.MaxValue) {
                    conversion = Conversion.Implicit;
                    return (ushort)DecimalVal;
                }
            } else if (value is long) {
                long Int64Val = (long)value;
                if (Int64Val >= UInt16.MinValue && Int64Val <= UInt16.MaxValue) {
                    conversion = Conversion.Implicit;
                    return (ushort)Int64Val;
                }
            }
            conversion = Conversion.None;
            return (ushort)0;
        }

        public static float TryConvertToSingle(object value, out Conversion conversion) {
            ExtensibleInt ei;
            BigInteger bi;
            ExtensibleFloat ef;
            ExtensibleLong el;
            if (value is float) {
                conversion = Conversion.Identity;
                return (float)value;
            } else if (value is int) {
                conversion = Conversion.Implicit;
                return (float)(int)value;
            } else if (!Object.Equals((ei = value as ExtensibleInt), null)) {
                conversion = Conversion.Implicit;
                return (float)ei.value;
            } else if (value is double) {
                double DoubleVal = (double)value;
                if (DoubleVal >= Single.MinValue && DoubleVal <= Single.MaxValue) {
                    conversion = Conversion.Implicit;
                    return (float)DoubleVal;
                }
            } else if (!Object.Equals((bi = value as BigInteger), null)) {
                conversion = Conversion.Implicit;
                double res;
                if (bi.TryToFloat64(out res)) return (float)res;
            } else if (value is bool) {
                conversion = Conversion.NonStandard;
                return ((bool)value) ? (float)1.0 : (float)0.0;
            } else if (value is char) {
                conversion = Conversion.Implicit;
                return (float)(char)value;
            } else if (value is byte) {
                conversion = Conversion.Implicit;
                return (float)(byte)value;
            } else if (value is sbyte) {
                conversion = Conversion.Implicit;
                return (float)(sbyte)value;
            } else if (value is short) {
                conversion = Conversion.Implicit;
                return (float)(short)value;
            } else if (value is uint) {
                conversion = Conversion.Implicit;
                return (float)(uint)value;
            } else if (value is ulong) {
                conversion = Conversion.Implicit;
                return (float)(ulong)value;
            } else if (value is ushort) {
                conversion = Conversion.Implicit;
                return (float)(ushort)value;
            } else if (!Object.Equals((ef = value as ExtensibleFloat), null)) {
                conversion = Conversion.Implicit;
                return (float)ef.value;
            } else if (!Object.Equals((el = value as ExtensibleLong), null)) {
                conversion = Conversion.Implicit;
                double res;
                if (el.Value.TryToFloat64(out res)) return (float)res;
            } else if (value is long) {
                conversion = Conversion.Implicit;
                return (float)(long)value;
            } else if (value is decimal) {
                conversion = Conversion.Truncation;
                return (float)(decimal)value;
            }
            conversion = Conversion.None;
            return (float)0.0;
        }

        public static decimal TryConvertToDecimal(object value, out Conversion conversion) {
            if (value is decimal) {
                conversion = Conversion.Identity;
                return (decimal)value;
            } else if (value is int) {
                conversion = Conversion.Implicit;
                return (decimal)(int)value;
            } else if (value is bool) {
                conversion = Conversion.NonStandard;
                return ((bool)value) ? (decimal)1 : (decimal)0;
            } else if (value is char) {
                conversion = Conversion.Implicit;
                return (decimal)(char)value;
            } else if (value is byte) {
                conversion = Conversion.Implicit;
                return (decimal)(byte)value;
            } else if (value is sbyte) {
                conversion = Conversion.Implicit;
                return (decimal)(sbyte)value;
            } else if (value is short) {
                conversion = Conversion.Implicit;
                return (decimal)(short)value;
            } else if (value is uint) {
                conversion = Conversion.Implicit;
                return (decimal)(uint)value;
            } else if (value is ulong) {
                conversion = Conversion.Implicit;
                return (decimal)(ulong)value;
            } else if (value is ushort) {
                conversion = Conversion.Implicit;
                return (decimal)(ushort)value;
            } else if (value is long) {
                conversion = Conversion.Implicit;
                return (decimal)(long)value;
            }
            conversion = Conversion.None;
            return (decimal)0;
        }

        public static long TryConvertToInt64(object value, out Conversion conversion) {
            ExtensibleInt ei;
            BigInteger bi;
            Enum e;
            if (value is long) {
                conversion = Conversion.Identity;
                return (long)value;
            } else if (value is int) {
                conversion = Conversion.Implicit;
                return (long)(int)value;
            } else if (!Object.Equals((ei = value as ExtensibleInt), null)) {
                conversion = Conversion.Implicit;
                return (long)ei.value;
            } else if (!Object.Equals((bi = value as BigInteger), null)) {
                long res;
                if (bi.AsInt64(out res)) {
                    conversion = Conversion.Implicit;
                    return res;
                }
            } else if (value is bool) {
                conversion = Conversion.NonStandard;
                return ((bool)value) ? 1 : 0;
            } else if (value is char) {
                conversion = Conversion.Implicit;
                return (long)(char)value;
            } else if (value is byte) {
                conversion = Conversion.Implicit;
                return (long)(byte)value;
            } else if (value is sbyte) {
                conversion = Conversion.Implicit;
                return (long)(sbyte)value;
            } else if (value is short) {
                conversion = Conversion.Implicit;
                return (long)(short)value;
            } else if (value is uint) {
                conversion = Conversion.Implicit;
                return (long)(uint)value;
            } else if (value is ulong) {
                ulong UInt64Val = (ulong)value;
                if (UInt64Val <= Int64.MaxValue) {
                    conversion = Conversion.Implicit;
                    return (long)UInt64Val;
                }
            } else if (value is ushort) {
                conversion = Conversion.Implicit;
                return (long)(ushort)value;
            } else if (!Object.Equals((e = value as Enum), null)) {
                return TryConvertEnumToInt64(e, out conversion);
            } else if (value is decimal) {
                decimal DecimalVal = (decimal)value;
                if (DecimalVal <= Int64.MaxValue) {
                    conversion = Conversion.Implicit;
                    return (long)DecimalVal;
                }
            }
            conversion = Conversion.None;
            return (long)0;
        }

        //
        // Entry point into "Try" conversions
        //
        public static object TryConvertWorker(object value, Type to, out Conversion conversion) {

            Type from = value.GetType();
            if (from == to) {
                conversion = Conversion.Identity;
                return value;
            }
            if (to == ObjectType) {
                conversion = Conversion.Implicit;
                return value;
            }
            if (to == Int32Type) {
                return TryConvertToInt32(value, out conversion);
            } else if (to == StringType) {
                return TryConvertToString(value, out conversion);
            } else if (to == DoubleType) {
                return TryConvertToDouble(value, out conversion);
            } else if (to == BigIntegerType) {
                return TryConvertToBigInteger(value, out conversion);
            } else if (to == BooleanType) {
                return TryConvertToBoolean(value, out conversion);
            } else if (to == Complex64Type) {
                return TryConvertToComplex64(value, out conversion);
            } else if (to == CharType) {
                return TryConvertToChar(value, out conversion);
            } else if (DelegateType.IsAssignableFrom(to)) {
                return TryConvertToDelegate(value, to, out conversion);
            } else if (to == IEnumeratorType) {
                return TryConvertToIEnumerator(value, out conversion);
            } else if (to == TypeType) {
                return TryConvertToType(value, out conversion);
            } else if (to == ByteType) {
                return TryConvertToByte(value, out conversion);
            } else if (to == SByteType) {
                return TryConvertToSByte(value, out conversion);
            } else if (to == Int16Type) {
                return TryConvertToInt16(value, out conversion);
            } else if (to == UInt32Type) {
                return TryConvertToUInt32(value, out conversion);
            } else if (to == UInt64Type) {
                return TryConvertToUInt64(value, out conversion);
            } else if (to == UInt16Type) {
                return TryConvertToUInt16(value, out conversion);
            } else if (to == SingleType) {
                return TryConvertToSingle(value, out conversion);
            } else if (to == DecimalType) {
                return TryConvertToDecimal(value, out conversion);
            } else if (to == Int64Type) {
                return TryConvertToInt64(value, out conversion);
            } else if (to == ArrayListType) {
                return TryConvertToArrayList(value, out conversion);
            } else if (to == HashtableType) {
                return TryConvertToHashtable(value, out conversion);
            } else if (to.IsArray) {
                return TryConvertToArray(value, to, out conversion);
            } else if (to.IsGenericType) {
                Type genTo = to.GetGenericTypeDefinition();
                if (genTo == IListOfTType) {
                    object res = TryConvertToIListT(value, to.GetGenericArguments(), out conversion);
                    if (conversion != Conversion.None) return res;
                } else if (genTo == ListOfTType) {
                    return TryConvertToListOfT(value, to.GetGenericArguments(), out conversion);
                } else if (genTo == IEnumeratorOfT) {
                    object res = TryConvertToIEnumeratorOfT(value, to.GetGenericArguments(), out conversion);
                    if (conversion != Conversion.None) return res;
                } else if (genTo == IDictOfTType) {
                    object res = TryConvertToIDictOfT(value, to.GetGenericArguments(), out conversion);
                    if (conversion != Conversion.None) return res;
                }
            }
            if (from.IsValueType) {
                if (to.IsEnum) {
                    if (value is int) {
                        int IntValue = (int)value;
                        if (IntValue == 0) {
                            conversion = Conversion.Implicit;
                            return 0;
                        }
                    }
                }
                if (to == ValueTypeType) {
                    conversion = Conversion.Implicit;
                    return (System.ValueType)value;
                }
            }
            if (to.IsInstanceOfType(value)) {
                conversion = Conversion.Identity;
                return value;
            }

            // check for implicit conversions 
            ReflectedType toType = Ops.GetDynamicTypeFromType(to) as ReflectedType;
            ReflectedType dt = Ops.GetDynamicType(value) as ReflectedType;

            if (toType != null && dt != null) {
                object res = dt.TryConvertTo(value, toType, out conversion);
                if (conversion != Conversion.None) {
                    return res;
                }

                res = toType.TryConvertFrom(value, out conversion);
                if (conversion != Conversion.None) {
                    return res;
                }
            }

            conversion = Conversion.None;
            return null;
        }

        //
        // "throw" conversion methods
        //
        public static int ConvertToInt32(object value) {
            Conversion conversion;
            int val = TryConvertToInt32(value, out conversion);
            if (conversion == Conversion.None) {
                throw Ops.TypeError("expected int, found {0}", Ops.GetDynamicType(value).__name__);
            }
            return val;
        }

        public static string ConvertToString(object value) {
            Conversion conversion;
            string val = TryConvertToString(value, out conversion);
            if (conversion == Conversion.None) {
                throw Ops.TypeError("expected string, found {0}", Ops.GetDynamicType(value).__name__);
            }
            return val;
        }

        public static double ConvertToDouble(object value) {
            Conversion conversion;
            double val = TryConvertToDouble(value, out conversion);
            if (conversion == Conversion.None) {
                throw Ops.TypeError("expected double, found {0}", Ops.GetDynamicType(value).__name__);
            }
            return val;
        }

        public static BigInteger ConvertToBigInteger(object value) {
            Conversion conversion;
            BigInteger val = TryConvertToBigInteger(value, out conversion);
            if (conversion == Conversion.None) {
                throw Ops.TypeError("expected BigInteger, found {0}", Ops.GetDynamicType(value).__name__);
            }
            return val;
        }

        public static bool ConvertToBoolean(object value) {
            Conversion conversion;
            bool val = TryConvertToBoolean(value, out conversion);
            if (conversion == Conversion.None) {
                throw Ops.TypeError("expected bool, found {0}", Ops.GetDynamicType(value).__name__);
            }
            return val;
        }

        public static Complex64 ConvertToComplex64(object value) {
            Conversion conversion;
            Complex64 val = TryConvertToComplex64(value, out conversion);
            if (conversion == Conversion.None) {
                throw Ops.TypeError("expected Complex64, found {0}", Ops.GetDynamicType(value).__name__);
            }
            return val;
        }

        public static char ConvertToChar(object value) {
            Conversion conversion;
            char val = TryConvertToChar(value, out conversion);
            if (conversion == Conversion.None) {
                throw Ops.TypeError("expected char, found {0}", Ops.GetDynamicType(value).__name__);
            }
            return val;
        }

        public static IEnumerator ConvertToIEnumerator(object value) {
            Conversion conversion;
            IEnumerator val = TryConvertToIEnumerator(value, out conversion);
            if (conversion == Conversion.None) {
                throw Ops.TypeError("expected IEnumerator, found {0}", Ops.GetDynamicType(value).__name__);
            }
            return val;
        }

        public static Type ConvertToType(object value) {
            Conversion conversion;
            Type val = TryConvertToType(value, out conversion);
            if (conversion == Conversion.None) {
                throw Ops.TypeError("expected Type, found {0}", Ops.GetDynamicType(value).__name__);
            }
            return val;
        }

        public static byte ConvertToByte(object value) {
            Conversion conversion;
            byte val = TryConvertToByte(value, out conversion);
            if (conversion == Conversion.None) {
                throw Ops.TypeError("expected byte, found {0}", Ops.GetDynamicType(value).__name__);
            }
            return val;
        }

        public static sbyte ConvertToSByte(object value) {
            Conversion conversion;
            sbyte val = TryConvertToSByte(value, out conversion);
            if (conversion == Conversion.None) {
                throw Ops.TypeError("expected sbyte, found {0}", Ops.GetDynamicType(value).__name__);
            }
            return val;
        }

        public static short ConvertToInt16(object value) {
            Conversion conversion;
            short val = TryConvertToInt16(value, out conversion);
            if (conversion == Conversion.None) {
                throw Ops.TypeError("expected short, found {0}", Ops.GetDynamicType(value).__name__);
            }
            return val;
        }

        public static uint ConvertToUInt32(object value) {
            Conversion conversion;
            uint val = TryConvertToUInt32(value, out conversion);
            if (conversion == Conversion.None) {
                throw Ops.TypeError("expected uint, found {0}", Ops.GetDynamicType(value).__name__);
            }
            return val;
        }

        public static ulong ConvertToUInt64(object value) {
            Conversion conversion;
            ulong val = TryConvertToUInt64(value, out conversion);
            if (conversion == Conversion.None) {
                throw Ops.TypeError("expected ulong, found {0}", Ops.GetDynamicType(value).__name__);
            }
            return val;
        }

        public static ushort ConvertToUInt16(object value) {
            Conversion conversion;
            ushort val = TryConvertToUInt16(value, out conversion);
            if (conversion == Conversion.None) {
                throw Ops.TypeError("expected ushort, found {0}", Ops.GetDynamicType(value).__name__);
            }
            return val;
        }

        public static float ConvertToSingle(object value) {
            Conversion conversion;
            float val = TryConvertToSingle(value, out conversion);
            if (conversion == Conversion.None) {
                throw Ops.TypeError("expected float, found {0}", Ops.GetDynamicType(value).__name__);
            }
            return val;
        }

        public static decimal ConvertToDecimal(object value) {
            Conversion conversion;
            decimal val = TryConvertToDecimal(value, out conversion);
            if (conversion == Conversion.None) {
                throw Ops.TypeError("expected decimal, found {0}", Ops.GetDynamicType(value).__name__);
            }
            return val;
        }

        public static long ConvertToInt64(object value) {
            Conversion conversion;
            long val = TryConvertToInt64(value, out conversion);
            if (conversion == Conversion.None) {
                throw Ops.TypeError("expected long, found {0}", Ops.GetDynamicType(value).__name__);
            }
            return val;
        }

        //
        // Entry point into "throw" conversion
        //
        public static object Convert(object value, Type to) {
            Conversion conversion;
            object val = TryConvert(value, to, out conversion);
            if (conversion == Conversion.None) {
                throw Ops.TypeError("No conversion from {0} to {1}", value, to);
            }
            return val;
        }

        // *** END GENERATED CODE ***

        #endregion

        #region Generated enum conversions

        // *** BEGIN GENERATED CODE ***

        private static short TryConvertEnumToInt16(object value, out Conversion conversion) {
            conversion = Conversion.NonStandard;
            switch (((Enum)value).GetTypeCode()) {
                case TypeCode.Int16:
                    return (short)(value);
                case TypeCode.Int32:
                    int Int32Val = (int)value;
                    if (Int32Val >= Int16.MinValue && Int32Val <= Int16.MaxValue) {
                        return (short)Int32Val;
                    }
                    break;
                case TypeCode.Int64:
                    long Int64Val = (long)value;
                    if (Int64Val >= Int16.MinValue && Int64Val <= Int16.MaxValue) {
                        return (short)Int64Val;
                    }
                    break;
                case TypeCode.UInt32:
                    uint UInt32Val = (uint)value;
                    if (UInt32Val <= Int16.MaxValue) {
                        return (short)UInt32Val;
                    }
                    break;
                case TypeCode.UInt64:
                    ulong UInt64Val = (ulong)value;
                    if (UInt64Val <= (ulong)Int16.MaxValue) {
                        return (short)UInt64Val;
                    }
                    break;
                case TypeCode.SByte:
                    return (short)(sbyte)(value);
                case TypeCode.UInt16:
                    ushort UInt16Val = (ushort)value;
                    if (UInt16Val <= Int16.MaxValue) {
                        return (short)UInt16Val;
                    }
                    break;
                case TypeCode.Byte:
                    return (short)(byte)(value);
            }
            conversion = Conversion.None;
            return 0;
        }

        private static sbyte TryConvertEnumToSByte(object value, out Conversion conversion) {
            conversion = Conversion.NonStandard;
            switch (((Enum)value).GetTypeCode()) {
                case TypeCode.SByte:
                    return (sbyte)(value);
                case TypeCode.Int32:
                    int Int32Val = (int)value;
                    if (Int32Val >= SByte.MinValue && Int32Val <= SByte.MaxValue) {
                        return (sbyte)Int32Val;
                    }
                    break;
                case TypeCode.Int64:
                    long Int64Val = (long)value;
                    if (Int64Val >= SByte.MinValue && Int64Val <= SByte.MaxValue) {
                        return (sbyte)Int64Val;
                    }
                    break;
                case TypeCode.Int16:
                    short Int16Val = (short)value;
                    if (Int16Val >= SByte.MinValue && Int16Val <= SByte.MaxValue) {
                        return (sbyte)Int16Val;
                    }
                    break;
                case TypeCode.UInt32:
                    uint UInt32Val = (uint)value;
                    if (UInt32Val <= SByte.MaxValue) {
                        return (sbyte)UInt32Val;
                    }
                    break;
                case TypeCode.UInt64:
                    ulong UInt64Val = (ulong)value;
                    if (UInt64Val <= (ulong)SByte.MaxValue) {
                        return (sbyte)UInt64Val;
                    }
                    break;
                case TypeCode.UInt16:
                    ushort UInt16Val = (ushort)value;
                    if (UInt16Val <= SByte.MaxValue) {
                        return (sbyte)UInt16Val;
                    }
                    break;
                case TypeCode.Byte:
                    byte ByteVal = (byte)value;
                    if (ByteVal <= SByte.MaxValue) {
                        return (sbyte)ByteVal;
                    }
                    break;
            }
            conversion = Conversion.None;
            return 0;
        }

        private static ulong TryConvertEnumToUInt64(object value, out Conversion conversion) {
            conversion = Conversion.NonStandard;
            switch (((Enum)value).GetTypeCode()) {
                case TypeCode.UInt64:
                    return (ulong)(value);
                case TypeCode.Int32:
                    int Int32Val = (int)value;
                    if (Int32Val >= 0) {
                        return (ulong)Int32Val;
                    }
                    break;
                case TypeCode.Int64:
                    long Int64Val = (long)value;
                    if (Int64Val >= 0) {
                        return (ulong)Int64Val;
                    }
                    break;
                case TypeCode.Int16:
                    short Int16Val = (short)value;
                    if (Int16Val >= 0) {
                        return (ulong)Int16Val;
                    }
                    break;
                case TypeCode.UInt32:
                    return (ulong)(uint)(value);
                case TypeCode.SByte:
                    sbyte SByteVal = (sbyte)value;
                    if (SByteVal >= 0) {
                        return (ulong)SByteVal;
                    }
                    break;
                case TypeCode.UInt16:
                    return (ulong)(ushort)(value);
                case TypeCode.Byte:
                    return (ulong)(byte)(value);
            }
            conversion = Conversion.None;
            return 0;
        }

        private static int TryConvertEnumToInt32(object value, out Conversion conversion) {
            conversion = Conversion.NonStandard;
            switch (((Enum)value).GetTypeCode()) {
                case TypeCode.Int32:
                    return (int)(value);
                case TypeCode.Int64:
                    long Int64Val = (long)value;
                    if (Int64Val >= Int32.MinValue && Int64Val <= Int32.MaxValue) {
                        return (int)Int64Val;
                    }
                    break;
                case TypeCode.Int16:
                    return (int)(short)(value);
                case TypeCode.UInt32:
                    uint UInt32Val = (uint)value;
                    if (UInt32Val <= Int32.MaxValue) {
                        return (int)UInt32Val;
                    }
                    break;
                case TypeCode.UInt64:
                    ulong UInt64Val = (ulong)value;
                    if (UInt64Val <= Int32.MaxValue) {
                        return (int)UInt64Val;
                    }
                    break;
                case TypeCode.SByte:
                    return (int)(sbyte)(value);
                case TypeCode.UInt16:
                    return (int)(ushort)(value);
                case TypeCode.Byte:
                    return (int)(byte)(value);
            }
            conversion = Conversion.None;
            return 0;
        }

        private static ushort TryConvertEnumToUInt16(object value, out Conversion conversion) {
            conversion = Conversion.NonStandard;
            switch (((Enum)value).GetTypeCode()) {
                case TypeCode.UInt16:
                    return (ushort)(value);
                case TypeCode.Int32:
                    int Int32Val = (int)value;
                    if (Int32Val >= 0 && Int32Val <= UInt16.MaxValue) {
                        return (ushort)Int32Val;
                    }
                    break;
                case TypeCode.Int64:
                    long Int64Val = (long)value;
                    if (Int64Val >= 0 && Int64Val <= UInt16.MaxValue) {
                        return (ushort)Int64Val;
                    }
                    break;
                case TypeCode.Int16:
                    short Int16Val = (short)value;
                    if (Int16Val >= 0) {
                        return (ushort)Int16Val;
                    }
                    break;
                case TypeCode.UInt32:
                    uint UInt32Val = (uint)value;
                    if (UInt32Val <= UInt16.MaxValue) {
                        return (ushort)UInt32Val;
                    }
                    break;
                case TypeCode.UInt64:
                    ulong UInt64Val = (ulong)value;
                    if (UInt64Val <= UInt16.MaxValue) {
                        return (ushort)UInt64Val;
                    }
                    break;
                case TypeCode.SByte:
                    sbyte SByteVal = (sbyte)value;
                    if (SByteVal >= 0) {
                        return (ushort)SByteVal;
                    }
                    break;
                case TypeCode.Byte:
                    return (ushort)(byte)(value);
            }
            conversion = Conversion.None;
            return 0;
        }

        private static long TryConvertEnumToInt64(object value, out Conversion conversion) {
            conversion = Conversion.NonStandard;
            switch (((Enum)value).GetTypeCode()) {
                case TypeCode.Int64:
                    return (long)(value);
                case TypeCode.Int32:
                    return (long)(int)(value);
                case TypeCode.Int16:
                    return (long)(short)(value);
                case TypeCode.UInt32:
                    return (long)(uint)(value);
                case TypeCode.UInt64:
                    ulong UInt64Val = (ulong)value;
                    if (UInt64Val <= Int64.MaxValue) {
                        return (long)UInt64Val;
                    }
                    break;
                case TypeCode.SByte:
                    return (long)(sbyte)(value);
                case TypeCode.UInt16:
                    return (long)(ushort)(value);
                case TypeCode.Byte:
                    return (long)(byte)(value);
            }
            conversion = Conversion.None;
            return 0;
        }

        private static uint TryConvertEnumToUInt32(object value, out Conversion conversion) {
            conversion = Conversion.NonStandard;
            switch (((Enum)value).GetTypeCode()) {
                case TypeCode.UInt32:
                    return (uint)(value);
                case TypeCode.Int32:
                    int Int32Val = (int)value;
                    if (Int32Val >= 0) {
                        return (uint)Int32Val;
                    }
                    break;
                case TypeCode.Int64:
                    long Int64Val = (long)value;
                    if (Int64Val >= 0 && Int64Val <= UInt32.MaxValue) {
                        return (uint)Int64Val;
                    }
                    break;
                case TypeCode.Int16:
                    short Int16Val = (short)value;
                    if (Int16Val >= 0) {
                        return (uint)Int16Val;
                    }
                    break;
                case TypeCode.UInt64:
                    ulong UInt64Val = (ulong)value;
                    if (UInt64Val <= UInt32.MaxValue) {
                        return (uint)UInt64Val;
                    }
                    break;
                case TypeCode.SByte:
                    sbyte SByteVal = (sbyte)value;
                    if (SByteVal >= 0) {
                        return (uint)SByteVal;
                    }
                    break;
                case TypeCode.UInt16:
                    return (uint)(ushort)(value);
                case TypeCode.Byte:
                    return (uint)(byte)(value);
            }
            conversion = Conversion.None;
            return 0;
        }

        private static byte TryConvertEnumToByte(object value, out Conversion conversion) {
            conversion = Conversion.NonStandard;
            switch (((Enum)value).GetTypeCode()) {
                case TypeCode.Byte:
                    return (byte)(value);
                case TypeCode.Int32:
                    int Int32Val = (int)value;
                    if (Int32Val >= 0 && Int32Val <= Byte.MaxValue) {
                        return (byte)Int32Val;
                    }
                    break;
                case TypeCode.Int64:
                    long Int64Val = (long)value;
                    if (Int64Val >= 0 && Int64Val <= Byte.MaxValue) {
                        return (byte)Int64Val;
                    }
                    break;
                case TypeCode.Int16:
                    short Int16Val = (short)value;
                    if (Int16Val >= 0 && Int16Val <= Byte.MaxValue) {
                        return (byte)Int16Val;
                    }
                    break;
                case TypeCode.UInt32:
                    uint UInt32Val = (uint)value;
                    if (UInt32Val <= Byte.MaxValue) {
                        return (byte)UInt32Val;
                    }
                    break;
                case TypeCode.UInt64:
                    ulong UInt64Val = (ulong)value;
                    if (UInt64Val <= Byte.MaxValue) {
                        return (byte)UInt64Val;
                    }
                    break;
                case TypeCode.SByte:
                    sbyte SByteVal = (sbyte)value;
                    if (SByteVal >= 0) {
                        return (byte)SByteVal;
                    }
                    break;
                case TypeCode.UInt16:
                    ushort UInt16Val = (ushort)value;
                    if (UInt16Val <= Byte.MaxValue) {
                        return (byte)UInt16Val;
                    }
                    break;
            }
            conversion = Conversion.None;
            return 0;
        }

        private static bool TryConvertEnumToBoolean(object value, out Conversion conversion) {
            switch (((Enum)value).GetTypeCode()) {
                case TypeCode.Int32:
                    conversion = Conversion.NonStandard;
                    return (int)value != 0;
                case TypeCode.Int64:
                    conversion = Conversion.NonStandard;
                    return (long)value != 0;
                case TypeCode.Int16:
                    conversion = Conversion.NonStandard;
                    return (short)value != 0;
                case TypeCode.UInt32:
                    conversion = Conversion.NonStandard;
                    return (uint)value != 0;
                case TypeCode.UInt64:
                    conversion = Conversion.NonStandard;
                    return (ulong)value != 0;
                case TypeCode.SByte:
                    conversion = Conversion.NonStandard;
                    return (sbyte)value != 0;
                case TypeCode.UInt16:
                    conversion = Conversion.NonStandard;
                    return (ushort)value != 0;
                case TypeCode.Byte:
                    conversion = Conversion.NonStandard;
                    return (byte)value != 0;
                default:
                    conversion = Conversion.None;
                    return false;
            }
        }

        // *** END GENERATED CODE ***

        #endregion

        #region Generated Conversion Helpers

        // *** BEGIN GENERATED CODE ***

        public static bool HasConversion(Type t) {
            if (t.IsArray) return true;
            if (t == typeof(ArrayList) || t == typeof(Hashtable) || t.IsGenericType) return true;
            if (t == typeof(bool)) return true;
            if (t == typeof(char)) return true;
            if (t == typeof(sbyte)) return true;
            if (t == typeof(byte)) return true;
            if (t == typeof(short)) return true;
            if (t == typeof(ushort)) return true;
            if (t == typeof(int)) return true;
            if (t == typeof(uint)) return true;
            if (t == typeof(long)) return true;
            if (t == typeof(ulong)) return true;
            if (t == typeof(float)) return true;
            if (t == typeof(double)) return true;
            if (t == typeof(string)) return true;
            if (t == typeof(decimal)) return true;
            if (t == typeof(BigInteger) || t.IsSubclassOf(typeof(BigInteger))) return true;
            if (t == typeof(ExtensibleInt) || t.IsSubclassOf(typeof(ExtensibleInt))) return true;
            if (t == typeof(ExtensibleComplex) || t.IsSubclassOf(typeof(ExtensibleComplex))) return true;
            if (t == typeof(ExtensibleString) || t.IsSubclassOf(typeof(ExtensibleString))) return true;
            if (t == typeof(ExtensibleFloat) || t.IsSubclassOf(typeof(ExtensibleFloat))) return true;
            if (t == typeof(ExtensibleLong) || t.IsSubclassOf(typeof(ExtensibleLong))) return true;
            if (t == typeof(Complex64) || t.IsSubclassOf(typeof(Complex64))) return true;
            if (t == typeof(Delegate) || t.IsSubclassOf(typeof(Delegate))) return true;
            if (t == typeof(IEnumerator) || t.IsSubclassOf(typeof(IEnumerator))) return true;
            if (t == typeof(Type) || t.IsSubclassOf(typeof(Type))) return true;
            if (t == typeof(Tuple) || t.IsSubclassOf(typeof(Tuple))) return true;
            if (t == typeof(Enum) || t.IsSubclassOf(typeof(Enum))) return true;
            if (t.IsSubclassOf(typeof(ArrayList)) || t.IsSubclassOf(typeof(Hashtable))) return true;
            return false;
        }

        // *** END GENERATED CODE ***

        #endregion
    }
}
