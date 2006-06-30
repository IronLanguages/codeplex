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
using System.Threading;
using IronMath;
using IronPython.Runtime;
using IronPython.Runtime.Types;

namespace IronPython.Runtime.Operations {
    static partial class UInt64Ops {

        #region Generated UInt64Ops

        // *** BEGIN GENERATED CODE ***

        private static ReflectedType UInt64Type;
        public static DynamicType MakeDynamicType() {
            if (UInt64Type == null) {
                OpsReflectedType ort = new OpsReflectedType("UInt64", typeof(UInt64), typeof(UInt64Ops), null);
                if (Interlocked.CompareExchange<ReflectedType>(ref UInt64Type, ort, null) == null) {
                    return ort;
                }
            }
            return UInt64Type;
        }

        [PythonName("__new__")]
        public static object Make(DynamicType cls, object value) {
            if (cls != UInt64Type) {
                throw Ops.TypeError("UInt64.__new__: first argument must be UInt64 type.");
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
            } else if (value is ExtensibleInt) {
                return (UInt64)((ExtensibleInt)value).value;
            } else if (value is ExtensibleLong) {
                return (UInt64)((ExtensibleLong)value).Value;
            } else if (value is ExtensibleFloat) {
                return (UInt64)((ExtensibleFloat)value).value;
            }
            throw Ops.ValueError("invalid value for UInt64.__new__");
        }

        [PythonName("__add__")]
        public static object Add(object left, object right) {
            Debug.Assert(left is UInt64);
            UInt64 leftUInt64 = (UInt64)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean: {
                            return UInt64Ops.AddImpl(leftUInt64, ((Boolean)right ? (UInt64)1 : (UInt64)0));
                        }
                    case TypeCode.Byte: {
                            return UInt64Ops.AddImpl(leftUInt64, ((Byte)right));
                        }
                    case TypeCode.SByte: {
                            return UInt64Ops.AddImpl(leftUInt64, ((SByte)right));
                        }
                    case TypeCode.Int16: {
                            return UInt64Ops.AddImpl(leftUInt64, ((Int16)right));
                        }
                    case TypeCode.UInt16: {
                            return UInt64Ops.AddImpl(leftUInt64, ((UInt16)right));
                        }
                    case TypeCode.Int32: {
                            return UInt64Ops.AddImpl(leftUInt64, ((Int32)right));
                        }
                    case TypeCode.UInt32: {
                            return UInt64Ops.AddImpl(leftUInt64, ((UInt32)right));
                        }
                    case TypeCode.Int64: {
                            return UInt64Ops.AddImpl(leftUInt64, ((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.AddImpl(leftUInt64, ((UInt64)right));
                        }
                    case TypeCode.Single: {
                            return (Single)(((Single)leftUInt64) + ((Single)((Single)right)));
                        }
                    case TypeCode.Double: {
                            return (Double)(((Double)leftUInt64) + ((Double)((Double)right)));
                        }
                }
            }
            if (right is BigInteger) {
                return LongOps.Add(leftUInt64, (BigInteger)right);
            } else if (right is Complex64) {
                return ComplexOps.Add(leftUInt64, (Complex64)right);
            } else if (right is ExtensibleInt) {
                return UInt64Ops.AddImpl(leftUInt64, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.Add(leftUInt64, ((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return (Double)(((Double)leftUInt64) + ((Double)((ExtensibleFloat)right).value));
            } else if (right is ExtensibleComplex) {
                return ComplexOps.Add(leftUInt64, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__div__")]
        public static object Divide(object left, object right) {
            Debug.Assert(left is UInt64);
            UInt64 leftUInt64 = (UInt64)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean: {
                            return UInt64Ops.DivideImpl((UInt64)leftUInt64, (UInt64)((Boolean)right ? (UInt64)1 : (UInt64)0));
                        }
                    case TypeCode.Byte: {
                            return UInt64Ops.DivideImpl((UInt64)leftUInt64, (UInt64)((Byte)right));
                        }
                    case TypeCode.SByte: {
                            return UInt64Ops.DivideImpl(leftUInt64, ((SByte)right));
                        }
                    case TypeCode.Int16: {
                            return UInt64Ops.DivideImpl(leftUInt64, ((Int16)right));
                        }
                    case TypeCode.UInt16: {
                            return UInt64Ops.DivideImpl((UInt64)leftUInt64, (UInt64)((UInt16)right));
                        }
                    case TypeCode.Int32: {
                            return UInt64Ops.DivideImpl(leftUInt64, ((Int32)right));
                        }
                    case TypeCode.UInt32: {
                            return UInt64Ops.DivideImpl((UInt64)leftUInt64, (UInt64)((UInt32)right));
                        }
                    case TypeCode.Int64: {
                            return UInt64Ops.DivideImpl(leftUInt64, ((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.DivideImpl((UInt64)leftUInt64, (UInt64)((UInt64)right));
                        }
                    case TypeCode.Single: {
                            return SingleOps.DivideImpl((Single)leftUInt64, (Single)((Single)right));
                        }
                    case TypeCode.Double: {
                            return FloatOps.Divide((Double)leftUInt64, (Double)((Double)right));
                        }
                }
            }
            if (right is BigInteger) {
                return LongOps.Divide(leftUInt64, (BigInteger)right);
            } else if (right is Complex64) {
                return ComplexOps.Divide(leftUInt64, (Complex64)right);
            } else if (right is ExtensibleInt) {
                return UInt64Ops.DivideImpl(leftUInt64, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.Divide(leftUInt64, ((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return FloatOps.Divide((Double)leftUInt64, (Double)((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return ComplexOps.Divide(leftUInt64, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__floordiv__")]
        public static object FloorDivide(object left, object right) {
            Debug.Assert(left is UInt64);
            UInt64 leftUInt64 = (UInt64)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean: {
                            return UInt64Ops.FloorDivideImpl((UInt64)leftUInt64, (UInt64)((Boolean)right ? (UInt64)1 : (UInt64)0));
                        }
                    case TypeCode.Byte: {
                            return UInt64Ops.FloorDivideImpl((UInt64)leftUInt64, (UInt64)((Byte)right));
                        }
                    case TypeCode.SByte: {
                            return UInt64Ops.FloorDivideImpl(leftUInt64, ((SByte)right));
                        }
                    case TypeCode.Int16: {
                            return UInt64Ops.FloorDivideImpl(leftUInt64, ((Int16)right));
                        }
                    case TypeCode.UInt16: {
                            return UInt64Ops.FloorDivideImpl((UInt64)leftUInt64, (UInt64)((UInt16)right));
                        }
                    case TypeCode.Int32: {
                            return UInt64Ops.FloorDivideImpl(leftUInt64, ((Int32)right));
                        }
                    case TypeCode.UInt32: {
                            return UInt64Ops.FloorDivideImpl((UInt64)leftUInt64, (UInt64)((UInt32)right));
                        }
                    case TypeCode.Int64: {
                            return UInt64Ops.FloorDivideImpl(leftUInt64, ((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.FloorDivideImpl((UInt64)leftUInt64, (UInt64)((UInt64)right));
                        }
                    case TypeCode.Single: {
                            return SingleOps.FloorDivideImpl((Single)leftUInt64, (Single)((Single)right));
                        }
                    case TypeCode.Double: {
                            return FloatOps.FloorDivide((Double)leftUInt64, (Double)((Double)right));
                        }
                }
            }
            if (right is BigInteger) {
                return LongOps.FloorDivide(leftUInt64, (BigInteger)right);
            } else if (right is Complex64) {
                return ComplexOps.FloorDivide(leftUInt64, (Complex64)right);
            } else if (right is ExtensibleInt) {
                return UInt64Ops.FloorDivideImpl(leftUInt64, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.FloorDivide(leftUInt64, ((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return FloatOps.FloorDivide((Double)leftUInt64, (Double)((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return ComplexOps.FloorDivide(leftUInt64, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__mod__")]
        public static object Mod(object left, object right) {
            Debug.Assert(left is UInt64);
            UInt64 leftUInt64 = (UInt64)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean: {
                            return UInt64Ops.ModImpl((UInt64)leftUInt64, (UInt64)((Boolean)right ? (UInt64)1 : (UInt64)0));
                        }
                    case TypeCode.Byte: {
                            return UInt64Ops.ModImpl((UInt64)leftUInt64, (UInt64)((Byte)right));
                        }
                    case TypeCode.SByte: {
                            return UInt64Ops.ModImpl(leftUInt64, ((SByte)right));
                        }
                    case TypeCode.Int16: {
                            return UInt64Ops.ModImpl(leftUInt64, ((Int16)right));
                        }
                    case TypeCode.UInt16: {
                            return UInt64Ops.ModImpl((UInt64)leftUInt64, (UInt64)((UInt16)right));
                        }
                    case TypeCode.Int32: {
                            return UInt64Ops.ModImpl(leftUInt64, ((Int32)right));
                        }
                    case TypeCode.UInt32: {
                            return UInt64Ops.ModImpl((UInt64)leftUInt64, (UInt64)((UInt32)right));
                        }
                    case TypeCode.Int64: {
                            return UInt64Ops.ModImpl(leftUInt64, ((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ModImpl((UInt64)leftUInt64, (UInt64)((UInt64)right));
                        }
                    case TypeCode.Single: {
                            return SingleOps.ModImpl((Single)leftUInt64, (Single)((Single)right));
                        }
                    case TypeCode.Double: {
                            return FloatOps.Mod((Double)leftUInt64, (Double)((Double)right));
                        }
                }
            }
            if (right is BigInteger) {
                return LongOps.Mod(leftUInt64, (BigInteger)right);
            } else if (right is Complex64) {
                return ComplexOps.Mod(leftUInt64, (Complex64)right);
            } else if (right is ExtensibleInt) {
                return UInt64Ops.ModImpl(leftUInt64, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.Mod(leftUInt64, ((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return FloatOps.Mod((Double)leftUInt64, (Double)((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return ComplexOps.Mod(leftUInt64, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__mul__")]
        public static object Multiply(object left, object right) {
            Debug.Assert(left is UInt64);
            UInt64 leftUInt64 = (UInt64)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean: {
                            UInt64 result = (UInt64)(((UInt64)leftUInt64) * ((UInt64)((Boolean)right ? (UInt64)1 : (UInt64)0)));
                            if (UInt64.MinValue <= result && result <= UInt64.MaxValue) {
                                return (UInt64)result;
                            } else return result;
                        }
                    case TypeCode.Byte: {
                            return UInt64Ops.MultiplyImpl(leftUInt64, ((Byte)right));
                        }
                    case TypeCode.SByte: {
                            return UInt64Ops.MultiplyImpl(leftUInt64, ((SByte)right));
                        }
                    case TypeCode.Int16: {
                            return UInt64Ops.MultiplyImpl(leftUInt64, ((Int16)right));
                        }
                    case TypeCode.UInt16: {
                            return UInt64Ops.MultiplyImpl(leftUInt64, ((UInt16)right));
                        }
                    case TypeCode.Int32: {
                            return UInt64Ops.MultiplyImpl(leftUInt64, ((Int32)right));
                        }
                    case TypeCode.UInt32: {
                            return UInt64Ops.MultiplyImpl(leftUInt64, ((UInt32)right));
                        }
                    case TypeCode.Int64: {
                            return UInt64Ops.MultiplyImpl(leftUInt64, ((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.MultiplyImpl(leftUInt64, ((UInt64)right));
                        }
                    case TypeCode.Single: {
                            return (Double)(((Double)leftUInt64) * ((Double)((Single)right)));
                        }
                    case TypeCode.Double: {
                            return FloatOps.Multiply(leftUInt64, ((Double)right));
                        }
                }
            }
            if (right is BigInteger) {
                return LongOps.Multiply(leftUInt64, (BigInteger)right);
            } else if (right is Complex64) {
                return ComplexOps.Multiply(leftUInt64, (Complex64)right);
            } else if (right is ExtensibleInt) {
                return UInt64Ops.MultiplyImpl(leftUInt64, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.Multiply(leftUInt64, ((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return FloatOps.Multiply(leftUInt64, ((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return ComplexOps.Multiply(leftUInt64, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__sub__")]
        public static object Subtract(object left, object right) {
            Debug.Assert(left is UInt64);
            UInt64 leftUInt64 = (UInt64)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean: {
                            return UInt64Ops.SubtractImpl(leftUInt64, ((Boolean)right ? (UInt64)1 : (UInt64)0));
                        }
                    case TypeCode.Byte: {
                            return UInt64Ops.SubtractImpl(leftUInt64, ((Byte)right));
                        }
                    case TypeCode.SByte: {
                            return UInt64Ops.SubtractImpl(leftUInt64, ((SByte)right));
                        }
                    case TypeCode.Int16: {
                            return UInt64Ops.SubtractImpl(leftUInt64, ((Int16)right));
                        }
                    case TypeCode.UInt16: {
                            return UInt64Ops.SubtractImpl(leftUInt64, ((UInt16)right));
                        }
                    case TypeCode.Int32: {
                            return UInt64Ops.SubtractImpl(leftUInt64, ((Int32)right));
                        }
                    case TypeCode.UInt32: {
                            return UInt64Ops.SubtractImpl(leftUInt64, ((UInt32)right));
                        }
                    case TypeCode.Int64: {
                            return UInt64Ops.SubtractImpl(leftUInt64, ((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.SubtractImpl(leftUInt64, ((UInt64)right));
                        }
                    case TypeCode.Single: {
                            return (Single)(((Single)leftUInt64) - ((Single)((Single)right)));
                        }
                    case TypeCode.Double: {
                            return (Double)(((Double)leftUInt64) - ((Double)((Double)right)));
                        }
                }
            }
            if (right is BigInteger) {
                return LongOps.Subtract(leftUInt64, (BigInteger)right);
            } else if (right is Complex64) {
                return ComplexOps.Subtract(leftUInt64, (Complex64)right);
            } else if (right is ExtensibleInt) {
                return UInt64Ops.SubtractImpl(leftUInt64, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.Subtract(leftUInt64, ((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return (Double)(((Double)leftUInt64) - ((Double)((ExtensibleFloat)right).value));
            } else if (right is ExtensibleComplex) {
                return ComplexOps.Subtract(leftUInt64, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__radd__")]
        public static object ReverseAdd(object left, object right) {
            Debug.Assert(left is UInt64);
            UInt64 leftUInt64 = (UInt64)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean: {
                            return UInt64Ops.ReverseAddImpl(leftUInt64, ((Boolean)right ? (UInt64)1 : (UInt64)0));
                        }
                    case TypeCode.Byte: {
                            return UInt64Ops.ReverseAddImpl(leftUInt64, ((Byte)right));
                        }
                    case TypeCode.SByte: {
                            return UInt64Ops.ReverseAddImpl(leftUInt64, ((SByte)right));
                        }
                    case TypeCode.Int16: {
                            return UInt64Ops.ReverseAddImpl(leftUInt64, ((Int16)right));
                        }
                    case TypeCode.UInt16: {
                            return UInt64Ops.ReverseAddImpl(leftUInt64, ((UInt16)right));
                        }
                    case TypeCode.Int32: {
                            return UInt64Ops.ReverseAddImpl(leftUInt64, ((Int32)right));
                        }
                    case TypeCode.UInt32: {
                            return UInt64Ops.ReverseAddImpl(leftUInt64, ((UInt32)right));
                        }
                    case TypeCode.Int64: {
                            return UInt64Ops.ReverseAddImpl(leftUInt64, ((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ReverseAddImpl(leftUInt64, ((UInt64)right));
                        }
                    case TypeCode.Single: {
                            return (Single)(((Single)((Single)right)) + ((Single)leftUInt64));
                        }
                    case TypeCode.Double: {
                            return (Double)(((Double)((Double)right)) + ((Double)leftUInt64));
                        }
                }
            }
            if (right is BigInteger) {
                return LongOps.ReverseAdd(leftUInt64, (BigInteger)right);
            } else if (right is Complex64) {
                return ComplexOps.ReverseAdd(leftUInt64, (Complex64)right);
            } else if (right is ExtensibleInt) {
                return UInt64Ops.ReverseAddImpl(leftUInt64, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.ReverseAdd(leftUInt64, ((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return (Double)(((Double)((ExtensibleFloat)right).value) + ((Double)leftUInt64));
            } else if (right is ExtensibleComplex) {
                return ComplexOps.ReverseAdd(leftUInt64, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rdiv__")]
        public static object ReverseDivide(object left, object right) {
            Debug.Assert(left is UInt64);
            UInt64 leftUInt64 = (UInt64)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean: {
                            return UInt64Ops.ReverseDivideImpl((UInt64)leftUInt64, (UInt64)((Boolean)right ? (UInt64)1 : (UInt64)0));
                        }
                    case TypeCode.Byte: {
                            return UInt64Ops.ReverseDivideImpl((UInt64)leftUInt64, (UInt64)((Byte)right));
                        }
                    case TypeCode.SByte: {
                            return UInt64Ops.ReverseDivideImpl(leftUInt64, ((SByte)right));
                        }
                    case TypeCode.Int16: {
                            return UInt64Ops.ReverseDivideImpl(leftUInt64, ((Int16)right));
                        }
                    case TypeCode.UInt16: {
                            return UInt64Ops.ReverseDivideImpl((UInt64)leftUInt64, (UInt64)((UInt16)right));
                        }
                    case TypeCode.Int32: {
                            return UInt64Ops.ReverseDivideImpl(leftUInt64, ((Int32)right));
                        }
                    case TypeCode.UInt32: {
                            return UInt64Ops.ReverseDivideImpl((UInt64)leftUInt64, (UInt64)((UInt32)right));
                        }
                    case TypeCode.Int64: {
                            return UInt64Ops.ReverseDivideImpl(leftUInt64, ((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ReverseDivideImpl((UInt64)leftUInt64, (UInt64)((UInt64)right));
                        }
                    case TypeCode.Single: {
                            return SingleOps.ReverseDivideImpl((Single)leftUInt64, (Single)((Single)right));
                        }
                    case TypeCode.Double: {
                            return FloatOps.ReverseDivide((Double)leftUInt64, (Double)((Double)right));
                        }
                }
            }
            if (right is BigInteger) {
                return LongOps.ReverseDivide(leftUInt64, (BigInteger)right);
            } else if (right is Complex64) {
                return ComplexOps.ReverseDivide(leftUInt64, (Complex64)right);
            } else if (right is ExtensibleInt) {
                return UInt64Ops.ReverseDivideImpl(leftUInt64, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.ReverseDivide(leftUInt64, ((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return FloatOps.ReverseDivide((Double)leftUInt64, (Double)((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return ComplexOps.ReverseDivide(leftUInt64, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rfloordiv__")]
        public static object ReverseFloorDivide(object left, object right) {
            Debug.Assert(left is UInt64);
            UInt64 leftUInt64 = (UInt64)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean: {
                            return UInt64Ops.ReverseFloorDivideImpl((UInt64)leftUInt64, (UInt64)((Boolean)right ? (UInt64)1 : (UInt64)0));
                        }
                    case TypeCode.Byte: {
                            return UInt64Ops.ReverseFloorDivideImpl((UInt64)leftUInt64, (UInt64)((Byte)right));
                        }
                    case TypeCode.SByte: {
                            return UInt64Ops.ReverseFloorDivideImpl(leftUInt64, ((SByte)right));
                        }
                    case TypeCode.Int16: {
                            return UInt64Ops.ReverseFloorDivideImpl(leftUInt64, ((Int16)right));
                        }
                    case TypeCode.UInt16: {
                            return UInt64Ops.ReverseFloorDivideImpl((UInt64)leftUInt64, (UInt64)((UInt16)right));
                        }
                    case TypeCode.Int32: {
                            return UInt64Ops.ReverseFloorDivideImpl(leftUInt64, ((Int32)right));
                        }
                    case TypeCode.UInt32: {
                            return UInt64Ops.ReverseFloorDivideImpl((UInt64)leftUInt64, (UInt64)((UInt32)right));
                        }
                    case TypeCode.Int64: {
                            return UInt64Ops.ReverseFloorDivideImpl(leftUInt64, ((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ReverseFloorDivideImpl((UInt64)leftUInt64, (UInt64)((UInt64)right));
                        }
                    case TypeCode.Single: {
                            return SingleOps.ReverseFloorDivideImpl((Single)leftUInt64, (Single)((Single)right));
                        }
                    case TypeCode.Double: {
                            return FloatOps.ReverseFloorDivide((Double)leftUInt64, (Double)((Double)right));
                        }
                }
            }
            if (right is BigInteger) {
                return LongOps.ReverseFloorDivide(leftUInt64, (BigInteger)right);
            } else if (right is Complex64) {
                return ComplexOps.ReverseFloorDivide(leftUInt64, (Complex64)right);
            } else if (right is ExtensibleInt) {
                return UInt64Ops.ReverseFloorDivideImpl(leftUInt64, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.ReverseFloorDivide(leftUInt64, ((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return FloatOps.ReverseFloorDivide((Double)leftUInt64, (Double)((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return ComplexOps.ReverseFloorDivide(leftUInt64, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rmod__")]
        public static object ReverseMod(object left, object right) {
            Debug.Assert(left is UInt64);
            UInt64 leftUInt64 = (UInt64)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean: {
                            return UInt64Ops.ReverseModImpl((UInt64)leftUInt64, (UInt64)((Boolean)right ? (UInt64)1 : (UInt64)0));
                        }
                    case TypeCode.Byte: {
                            return UInt64Ops.ReverseModImpl((UInt64)leftUInt64, (UInt64)((Byte)right));
                        }
                    case TypeCode.SByte: {
                            return UInt64Ops.ReverseModImpl(leftUInt64, ((SByte)right));
                        }
                    case TypeCode.Int16: {
                            return UInt64Ops.ReverseModImpl(leftUInt64, ((Int16)right));
                        }
                    case TypeCode.UInt16: {
                            return UInt64Ops.ReverseModImpl((UInt64)leftUInt64, (UInt64)((UInt16)right));
                        }
                    case TypeCode.Int32: {
                            return UInt64Ops.ReverseModImpl(leftUInt64, ((Int32)right));
                        }
                    case TypeCode.UInt32: {
                            return UInt64Ops.ReverseModImpl((UInt64)leftUInt64, (UInt64)((UInt32)right));
                        }
                    case TypeCode.Int64: {
                            return UInt64Ops.ReverseModImpl(leftUInt64, ((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ReverseModImpl((UInt64)leftUInt64, (UInt64)((UInt64)right));
                        }
                    case TypeCode.Single: {
                            return SingleOps.ReverseModImpl((Single)leftUInt64, (Single)((Single)right));
                        }
                    case TypeCode.Double: {
                            return FloatOps.ReverseMod((Double)leftUInt64, (Double)((Double)right));
                        }
                }
            }
            if (right is BigInteger) {
                return LongOps.ReverseMod(leftUInt64, (BigInteger)right);
            } else if (right is Complex64) {
                return ComplexOps.ReverseMod(leftUInt64, (Complex64)right);
            } else if (right is ExtensibleInt) {
                return UInt64Ops.ReverseModImpl(leftUInt64, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.ReverseMod(leftUInt64, ((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return FloatOps.ReverseMod((Double)leftUInt64, (Double)((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return ComplexOps.ReverseMod(leftUInt64, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rmul__")]
        public static object ReverseMultiply(object left, object right) {
            Debug.Assert(left is UInt64);
            UInt64 leftUInt64 = (UInt64)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean: {
                            UInt64 result = (UInt64)(((UInt64)((Boolean)right ? (UInt64)1 : (UInt64)0)) * ((UInt64)leftUInt64));
                            if (UInt64.MinValue <= result && result <= UInt64.MaxValue) {
                                return (UInt64)result;
                            } else return result;
                        }
                    case TypeCode.Byte: {
                            return UInt64Ops.ReverseMultiplyImpl(leftUInt64, ((Byte)right));
                        }
                    case TypeCode.SByte: {
                            return UInt64Ops.ReverseMultiplyImpl(leftUInt64, ((SByte)right));
                        }
                    case TypeCode.Int16: {
                            return UInt64Ops.ReverseMultiplyImpl(leftUInt64, ((Int16)right));
                        }
                    case TypeCode.UInt16: {
                            return UInt64Ops.ReverseMultiplyImpl(leftUInt64, ((UInt16)right));
                        }
                    case TypeCode.Int32: {
                            return UInt64Ops.ReverseMultiplyImpl(leftUInt64, ((Int32)right));
                        }
                    case TypeCode.UInt32: {
                            return UInt64Ops.ReverseMultiplyImpl(leftUInt64, ((UInt32)right));
                        }
                    case TypeCode.Int64: {
                            return UInt64Ops.ReverseMultiplyImpl(leftUInt64, ((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ReverseMultiplyImpl(leftUInt64, ((UInt64)right));
                        }
                    case TypeCode.Single: {
                            return (Double)(((Double)((Single)right)) * ((Double)leftUInt64));
                        }
                    case TypeCode.Double: {
                            return FloatOps.ReverseMultiply(leftUInt64, ((Double)right));
                        }
                }
            }
            if (right is BigInteger) {
                return LongOps.ReverseMultiply(leftUInt64, (BigInteger)right);
            } else if (right is Complex64) {
                return ComplexOps.ReverseMultiply(leftUInt64, (Complex64)right);
            } else if (right is ExtensibleInt) {
                return UInt64Ops.ReverseMultiplyImpl(leftUInt64, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.ReverseMultiply(leftUInt64, ((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return FloatOps.ReverseMultiply(leftUInt64, ((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return ComplexOps.ReverseMultiply(leftUInt64, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rsub__")]
        public static object ReverseSubtract(object left, object right) {
            Debug.Assert(left is UInt64);
            UInt64 leftUInt64 = (UInt64)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean: {
                            return UInt64Ops.ReverseSubtractImpl(leftUInt64, ((Boolean)right ? (UInt64)1 : (UInt64)0));
                        }
                    case TypeCode.Byte: {
                            return UInt64Ops.ReverseSubtractImpl(leftUInt64, ((Byte)right));
                        }
                    case TypeCode.SByte: {
                            return UInt64Ops.ReverseSubtractImpl(leftUInt64, ((SByte)right));
                        }
                    case TypeCode.Int16: {
                            return UInt64Ops.ReverseSubtractImpl(leftUInt64, ((Int16)right));
                        }
                    case TypeCode.UInt16: {
                            return UInt64Ops.ReverseSubtractImpl(leftUInt64, ((UInt16)right));
                        }
                    case TypeCode.Int32: {
                            return UInt64Ops.ReverseSubtractImpl(leftUInt64, ((Int32)right));
                        }
                    case TypeCode.UInt32: {
                            return UInt64Ops.ReverseSubtractImpl(leftUInt64, ((UInt32)right));
                        }
                    case TypeCode.Int64: {
                            return UInt64Ops.ReverseSubtractImpl(leftUInt64, ((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ReverseSubtractImpl(leftUInt64, ((UInt64)right));
                        }
                    case TypeCode.Single: {
                            return (Single)(((Single)((Single)right)) - ((Single)leftUInt64));
                        }
                    case TypeCode.Double: {
                            return (Double)(((Double)((Double)right)) - ((Double)leftUInt64));
                        }
                }
            }
            if (right is BigInteger) {
                return LongOps.ReverseSubtract(leftUInt64, (BigInteger)right);
            } else if (right is Complex64) {
                return ComplexOps.ReverseSubtract(leftUInt64, (Complex64)right);
            } else if (right is ExtensibleInt) {
                return UInt64Ops.ReverseSubtractImpl(leftUInt64, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.ReverseSubtract(leftUInt64, ((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return (Double)(((Double)((ExtensibleFloat)right).value) - ((Double)leftUInt64));
            } else if (right is ExtensibleComplex) {
                return ComplexOps.ReverseSubtract(leftUInt64, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__and__")]
        public static object BitwiseAnd(object left, object right) {
            Debug.Assert(left is UInt64);
            UInt64 leftUInt64 = (UInt64)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean: {
                            UInt64 rightUInt64 = (UInt64)((Boolean)right ? (UInt64)1 : (UInt64)0);
                            return leftUInt64 & rightUInt64;
                        }
                    case TypeCode.Byte: {
                            UInt64 rightUInt64 = (UInt64)(Byte)right;
                            return leftUInt64 & rightUInt64;
                        }
                    case TypeCode.SByte: {
                            Int64 rightInt64 = (Int64)(SByte)right;
                            return (Int64)leftUInt64 & rightInt64;
                        }
                    case TypeCode.Int16: {
                            Int64 rightInt64 = (Int64)(Int16)right;
                            return (Int64)leftUInt64 & rightInt64;
                        }
                    case TypeCode.UInt16: {
                            UInt64 rightUInt64 = (UInt64)(UInt16)right;
                            return leftUInt64 & rightUInt64;
                        }
                    case TypeCode.Int32: {
                            Int64 rightInt64 = (Int64)(Int32)right;
                            return (Int64)leftUInt64 & rightInt64;
                        }
                    case TypeCode.UInt32: {
                            UInt64 rightUInt64 = (UInt64)(UInt32)right;
                            return leftUInt64 & rightUInt64;
                        }
                    case TypeCode.Int64: {
                            return (Int64)leftUInt64 & (Int64)right;
                        }
                    case TypeCode.UInt64: {
                            return leftUInt64 & (UInt64)right;
                        }
                }
            }
            if (right is BigInteger) {
                BigInteger leftBigInteger = (BigInteger)leftUInt64;
                return leftBigInteger & (BigInteger)right;
            } else if (right is ExtensibleInt) {
                Int64 rightInt64 = (Int64)((ExtensibleInt)right).value;
                return (Int64)leftUInt64 & rightInt64;
            } else if (right is ExtensibleLong) {
                BigInteger leftBigInteger = (BigInteger)leftUInt64;
                return leftBigInteger & (BigInteger)((ExtensibleLong)right).Value;
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rand__")]
        public static object ReverseBitwiseAnd(object left, object right) {
            Debug.Assert(left is UInt64);
            UInt64 leftUInt64 = (UInt64)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean: {
                            UInt64 rightUInt64 = (UInt64)((Boolean)right ? (UInt64)1 : (UInt64)0);
                            return leftUInt64 & rightUInt64;
                        }
                    case TypeCode.Byte: {
                            UInt64 rightUInt64 = (UInt64)(Byte)right;
                            return leftUInt64 & rightUInt64;
                        }
                    case TypeCode.SByte: {
                            Int64 rightInt64 = (Int64)(SByte)right;
                            return (Int64)leftUInt64 & rightInt64;
                        }
                    case TypeCode.Int16: {
                            Int64 rightInt64 = (Int64)(Int16)right;
                            return (Int64)leftUInt64 & rightInt64;
                        }
                    case TypeCode.UInt16: {
                            UInt64 rightUInt64 = (UInt64)(UInt16)right;
                            return leftUInt64 & rightUInt64;
                        }
                    case TypeCode.Int32: {
                            Int64 rightInt64 = (Int64)(Int32)right;
                            return (Int64)leftUInt64 & rightInt64;
                        }
                    case TypeCode.UInt32: {
                            UInt64 rightUInt64 = (UInt64)(UInt32)right;
                            return leftUInt64 & rightUInt64;
                        }
                    case TypeCode.Int64: {
                            return (Int64)leftUInt64 & (Int64)right;
                        }
                    case TypeCode.UInt64: {
                            return leftUInt64 & (UInt64)right;
                        }
                }
            }
            if (right is BigInteger) {
                BigInteger leftBigInteger = (BigInteger)leftUInt64;
                return leftBigInteger & (BigInteger)right;
            } else if (right is ExtensibleInt) {
                Int64 rightInt64 = (Int64)((ExtensibleInt)right).value;
                return (Int64)leftUInt64 & rightInt64;
            } else if (right is ExtensibleLong) {
                BigInteger leftBigInteger = (BigInteger)leftUInt64;
                return leftBigInteger & (BigInteger)((ExtensibleLong)right).Value;
            }
            return Ops.NotImplemented;
        }
        [PythonName("__or__")]
        public static object BitwiseOr(object left, object right) {
            Debug.Assert(left is UInt64);
            UInt64 leftUInt64 = (UInt64)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean: {
                            UInt64 rightUInt64 = (UInt64)((Boolean)right ? (UInt64)1 : (UInt64)0);
                            return leftUInt64 | rightUInt64;
                        }
                    case TypeCode.Byte: {
                            UInt64 rightUInt64 = (UInt64)(Byte)right;
                            return leftUInt64 | rightUInt64;
                        }
                    case TypeCode.SByte: {
                            Int64 rightInt64 = (Int64)(SByte)right;
                            return (Int64)leftUInt64 | rightInt64;
                        }
                    case TypeCode.Int16: {
                            Int64 rightInt64 = (Int64)(Int16)right;
                            return (Int64)leftUInt64 | rightInt64;
                        }
                    case TypeCode.UInt16: {
                            UInt64 rightUInt64 = (UInt64)(UInt16)right;
                            return leftUInt64 | rightUInt64;
                        }
                    case TypeCode.Int32: {
                            Int64 rightInt64 = (Int64)(Int32)right;
                            return (Int64)leftUInt64 | rightInt64;
                        }
                    case TypeCode.UInt32: {
                            UInt64 rightUInt64 = (UInt64)(UInt32)right;
                            return leftUInt64 | rightUInt64;
                        }
                    case TypeCode.Int64: {
                            return (Int64)leftUInt64 | (Int64)right;
                        }
                    case TypeCode.UInt64: {
                            return leftUInt64 | (UInt64)right;
                        }
                }
            }
            if (right is BigInteger) {
                BigInteger leftBigInteger = (BigInteger)leftUInt64;
                return leftBigInteger | (BigInteger)right;
            } else if (right is ExtensibleInt) {
                Int64 rightInt64 = (Int64)((ExtensibleInt)right).value;
                return (Int64)leftUInt64 | rightInt64;
            } else if (right is ExtensibleLong) {
                BigInteger leftBigInteger = (BigInteger)leftUInt64;
                return leftBigInteger | (BigInteger)((ExtensibleLong)right).Value;
            }
            return Ops.NotImplemented;
        }
        [PythonName("__ror__")]
        public static object ReverseBitwiseOr(object left, object right) {
            Debug.Assert(left is UInt64);
            UInt64 leftUInt64 = (UInt64)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean: {
                            UInt64 rightUInt64 = (UInt64)((Boolean)right ? (UInt64)1 : (UInt64)0);
                            return leftUInt64 | rightUInt64;
                        }
                    case TypeCode.Byte: {
                            UInt64 rightUInt64 = (UInt64)(Byte)right;
                            return leftUInt64 | rightUInt64;
                        }
                    case TypeCode.SByte: {
                            Int64 rightInt64 = (Int64)(SByte)right;
                            return (Int64)leftUInt64 | rightInt64;
                        }
                    case TypeCode.Int16: {
                            Int64 rightInt64 = (Int64)(Int16)right;
                            return (Int64)leftUInt64 | rightInt64;
                        }
                    case TypeCode.UInt16: {
                            UInt64 rightUInt64 = (UInt64)(UInt16)right;
                            return leftUInt64 | rightUInt64;
                        }
                    case TypeCode.Int32: {
                            Int64 rightInt64 = (Int64)(Int32)right;
                            return (Int64)leftUInt64 | rightInt64;
                        }
                    case TypeCode.UInt32: {
                            UInt64 rightUInt64 = (UInt64)(UInt32)right;
                            return leftUInt64 | rightUInt64;
                        }
                    case TypeCode.Int64: {
                            return (Int64)leftUInt64 | (Int64)right;
                        }
                    case TypeCode.UInt64: {
                            return leftUInt64 | (UInt64)right;
                        }
                }
            }
            if (right is BigInteger) {
                BigInteger leftBigInteger = (BigInteger)leftUInt64;
                return leftBigInteger | (BigInteger)right;
            } else if (right is ExtensibleInt) {
                Int64 rightInt64 = (Int64)((ExtensibleInt)right).value;
                return (Int64)leftUInt64 | rightInt64;
            } else if (right is ExtensibleLong) {
                BigInteger leftBigInteger = (BigInteger)leftUInt64;
                return leftBigInteger | (BigInteger)((ExtensibleLong)right).Value;
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rxor__")]
        public static object BitwiseXor(object left, object right) {
            Debug.Assert(left is UInt64);
            UInt64 leftUInt64 = (UInt64)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean: {
                            UInt64 rightUInt64 = (UInt64)((Boolean)right ? (UInt64)1 : (UInt64)0);
                            return leftUInt64 ^ rightUInt64;
                        }
                    case TypeCode.Byte: {
                            UInt64 rightUInt64 = (UInt64)(Byte)right;
                            return leftUInt64 ^ rightUInt64;
                        }
                    case TypeCode.SByte: {
                            Int64 rightInt64 = (Int64)(SByte)right;
                            return (Int64)leftUInt64 ^ rightInt64;
                        }
                    case TypeCode.Int16: {
                            Int64 rightInt64 = (Int64)(Int16)right;
                            return (Int64)leftUInt64 ^ rightInt64;
                        }
                    case TypeCode.UInt16: {
                            UInt64 rightUInt64 = (UInt64)(UInt16)right;
                            return leftUInt64 ^ rightUInt64;
                        }
                    case TypeCode.Int32: {
                            Int64 rightInt64 = (Int64)(Int32)right;
                            return (Int64)leftUInt64 ^ rightInt64;
                        }
                    case TypeCode.UInt32: {
                            UInt64 rightUInt64 = (UInt64)(UInt32)right;
                            return leftUInt64 ^ rightUInt64;
                        }
                    case TypeCode.Int64: {
                            return (Int64)leftUInt64 ^ (Int64)right;
                        }
                    case TypeCode.UInt64: {
                            return leftUInt64 ^ (UInt64)right;
                        }
                }
            }
            if (right is BigInteger) {
                BigInteger leftBigInteger = (BigInteger)leftUInt64;
                return leftBigInteger ^ (BigInteger)right;
            } else if (right is ExtensibleInt) {
                Int64 rightInt64 = (Int64)((ExtensibleInt)right).value;
                return (Int64)leftUInt64 ^ rightInt64;
            } else if (right is ExtensibleLong) {
                BigInteger leftBigInteger = (BigInteger)leftUInt64;
                return leftBigInteger ^ (BigInteger)((ExtensibleLong)right).Value;
            }
            return Ops.NotImplemented;
        }
        [PythonName("__xor__")]
        public static object ReverseBitwiseXor(object left, object right) {
            Debug.Assert(left is UInt64);
            UInt64 leftUInt64 = (UInt64)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean: {
                            UInt64 rightUInt64 = (UInt64)((Boolean)right ? (UInt64)1 : (UInt64)0);
                            return leftUInt64 ^ rightUInt64;
                        }
                    case TypeCode.Byte: {
                            UInt64 rightUInt64 = (UInt64)(Byte)right;
                            return leftUInt64 ^ rightUInt64;
                        }
                    case TypeCode.SByte: {
                            Int64 rightInt64 = (Int64)(SByte)right;
                            return (Int64)leftUInt64 ^ rightInt64;
                        }
                    case TypeCode.Int16: {
                            Int64 rightInt64 = (Int64)(Int16)right;
                            return (Int64)leftUInt64 ^ rightInt64;
                        }
                    case TypeCode.UInt16: {
                            UInt64 rightUInt64 = (UInt64)(UInt16)right;
                            return leftUInt64 ^ rightUInt64;
                        }
                    case TypeCode.Int32: {
                            Int64 rightInt64 = (Int64)(Int32)right;
                            return (Int64)leftUInt64 ^ rightInt64;
                        }
                    case TypeCode.UInt32: {
                            UInt64 rightUInt64 = (UInt64)(UInt32)right;
                            return leftUInt64 ^ rightUInt64;
                        }
                    case TypeCode.Int64: {
                            return (Int64)leftUInt64 ^ (Int64)right;
                        }
                    case TypeCode.UInt64: {
                            return leftUInt64 ^ (UInt64)right;
                        }
                }
            }
            if (right is BigInteger) {
                BigInteger leftBigInteger = (BigInteger)leftUInt64;
                return leftBigInteger ^ (BigInteger)right;
            } else if (right is ExtensibleInt) {
                Int64 rightInt64 = (Int64)((ExtensibleInt)right).value;
                return (Int64)leftUInt64 ^ rightInt64;
            } else if (right is ExtensibleLong) {
                BigInteger leftBigInteger = (BigInteger)leftUInt64;
                return leftBigInteger ^ (BigInteger)((ExtensibleLong)right).Value;
            }
            return Ops.NotImplemented;
        }
        [PythonName("__divmod__")]
        public static object DivMod(object left, object right) {
            Debug.Assert(left is UInt64);
            UInt64 leftUInt64 = (UInt64)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean:
                        return UInt64Ops.DivModImpl(leftUInt64, ((Boolean)right ? (UInt64)1 : (UInt64)0));
                    case TypeCode.Byte:
                        return UInt64Ops.DivModImpl(leftUInt64, (Byte)right);
                    case TypeCode.SByte:
                        return UInt64Ops.DivModImpl(leftUInt64, (SByte)right);
                    case TypeCode.Int16:
                        return UInt64Ops.DivModImpl(leftUInt64, (Int16)right);
                    case TypeCode.UInt16:
                        return UInt64Ops.DivModImpl(leftUInt64, (UInt16)right);
                    case TypeCode.Int32:
                        return UInt64Ops.DivModImpl(leftUInt64, (Int32)right);
                    case TypeCode.UInt32:
                        return UInt64Ops.DivModImpl(leftUInt64, (UInt32)right);
                    case TypeCode.Int64:
                        return UInt64Ops.DivModImpl(leftUInt64, (Int64)right);
                    case TypeCode.UInt64:
                        return UInt64Ops.DivModImpl(leftUInt64, (UInt64)right);
                    case TypeCode.Single:
                        return SingleOps.DivModImpl(leftUInt64, (Single)right);
                    case TypeCode.Double:
                        return FloatOps.DivMod(leftUInt64, (Double)right);
                }
            }
            if (right is BigInteger) {
                return LongOps.DivMod(leftUInt64, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return UInt64Ops.DivModImpl(leftUInt64, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.DivMod(leftUInt64, ((ExtensibleLong)right).Value);
            } else if (right is Complex64) {
                return ComplexOps.DivMod(leftUInt64, (Complex64)right);
            } else if (right is ExtensibleFloat) {
                return FloatOps.DivMod(leftUInt64, ((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return ComplexOps.DivMod(leftUInt64, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rdivmod__")]
        public static object ReverseDivMod(object left, object right) {
            Debug.Assert(left is UInt64);
            UInt64 leftUInt64 = (UInt64)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean:
                        return UInt64Ops.ReverseDivModImpl(leftUInt64, ((Boolean)right ? (UInt64)1 : (UInt64)0));
                    case TypeCode.Byte:
                        return UInt64Ops.ReverseDivModImpl(leftUInt64, (Byte)right);
                    case TypeCode.SByte:
                        return UInt64Ops.ReverseDivModImpl(leftUInt64, (SByte)right);
                    case TypeCode.Int16:
                        return UInt64Ops.ReverseDivModImpl(leftUInt64, (Int16)right);
                    case TypeCode.UInt16:
                        return UInt64Ops.ReverseDivModImpl(leftUInt64, (UInt16)right);
                    case TypeCode.Int32:
                        return UInt64Ops.ReverseDivModImpl(leftUInt64, (Int32)right);
                    case TypeCode.UInt32:
                        return UInt64Ops.ReverseDivModImpl(leftUInt64, (UInt32)right);
                    case TypeCode.Int64:
                        return UInt64Ops.ReverseDivModImpl(leftUInt64, (Int64)right);
                    case TypeCode.UInt64:
                        return UInt64Ops.ReverseDivModImpl(leftUInt64, (UInt64)right);
                    case TypeCode.Single:
                        return SingleOps.ReverseDivModImpl(leftUInt64, (Single)right);
                    case TypeCode.Double:
                        return FloatOps.ReverseDivMod(leftUInt64, (Double)right);
                }
            }
            if (right is BigInteger) {
                return LongOps.ReverseDivMod(leftUInt64, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return UInt64Ops.ReverseDivModImpl(leftUInt64, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.ReverseDivMod(leftUInt64, ((ExtensibleLong)right).Value);
            } else if (right is Complex64) {
                return ComplexOps.ReverseDivMod(leftUInt64, (Complex64)right);
            } else if (right is ExtensibleFloat) {
                return FloatOps.ReverseDivMod(leftUInt64, ((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return ComplexOps.ReverseDivMod(leftUInt64, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__lshift__")]
        public static object LeftShift(object left, object right) {
            Debug.Assert(left is UInt64);
            UInt64 leftUInt64 = (UInt64)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean:
                        return UInt64Ops.LeftShiftImpl(leftUInt64, ((Boolean)right ? (UInt64)1 : (UInt64)0));
                    case TypeCode.Byte:
                        return UInt64Ops.LeftShiftImpl(leftUInt64, (Byte)right);
                    case TypeCode.SByte:
                        return UInt64Ops.LeftShiftImpl(leftUInt64, (SByte)right);
                    case TypeCode.Int16:
                        return UInt64Ops.LeftShiftImpl(leftUInt64, (Int16)right);
                    case TypeCode.UInt16:
                        return UInt64Ops.LeftShiftImpl(leftUInt64, (UInt16)right);
                    case TypeCode.Int32:
                        return UInt64Ops.LeftShiftImpl(leftUInt64, (Int32)right);
                    case TypeCode.UInt32:
                        return UInt64Ops.LeftShiftImpl(leftUInt64, (UInt32)right);
                    case TypeCode.Int64:
                        return UInt64Ops.LeftShiftImpl(leftUInt64, (Int64)right);
                    case TypeCode.UInt64:
                        return UInt64Ops.LeftShiftImpl(leftUInt64, (UInt64)right);
                }
            }
            if (right is BigInteger) {
                return LongOps.LeftShift(leftUInt64, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return UInt64Ops.LeftShiftImpl(leftUInt64, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.LeftShift(leftUInt64, ((ExtensibleLong)right).Value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rlshift__")]
        public static object ReverseLeftShift(object left, object right) {
            Debug.Assert(left is UInt64);
            UInt64 leftUInt64 = (UInt64)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean:
                        return UInt64Ops.ReverseLeftShiftImpl(leftUInt64, ((Boolean)right ? (UInt64)1 : (UInt64)0));
                    case TypeCode.Byte:
                        return UInt64Ops.ReverseLeftShiftImpl(leftUInt64, (Byte)right);
                    case TypeCode.SByte:
                        return UInt64Ops.ReverseLeftShiftImpl(leftUInt64, (SByte)right);
                    case TypeCode.Int16:
                        return UInt64Ops.ReverseLeftShiftImpl(leftUInt64, (Int16)right);
                    case TypeCode.UInt16:
                        return UInt64Ops.ReverseLeftShiftImpl(leftUInt64, (UInt16)right);
                    case TypeCode.Int32:
                        return UInt64Ops.ReverseLeftShiftImpl(leftUInt64, (Int32)right);
                    case TypeCode.UInt32:
                        return UInt64Ops.ReverseLeftShiftImpl(leftUInt64, (UInt32)right);
                    case TypeCode.Int64:
                        return UInt64Ops.ReverseLeftShiftImpl(leftUInt64, (Int64)right);
                    case TypeCode.UInt64:
                        return UInt64Ops.ReverseLeftShiftImpl(leftUInt64, (UInt64)right);
                }
            }
            if (right is BigInteger) {
                return LongOps.ReverseLeftShift(leftUInt64, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return UInt64Ops.ReverseLeftShiftImpl(leftUInt64, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.ReverseLeftShift(leftUInt64, ((ExtensibleLong)right).Value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__pow__")]
        public static object Power(object left, object right) {
            Debug.Assert(left is UInt64);
            UInt64 leftUInt64 = (UInt64)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean:
                        return UInt64Ops.PowerImpl(leftUInt64, ((Boolean)right ? (UInt64)1 : (UInt64)0));
                    case TypeCode.Byte:
                        return UInt64Ops.PowerImpl(leftUInt64, (Byte)right);
                    case TypeCode.SByte:
                        return UInt64Ops.PowerImpl(leftUInt64, (SByte)right);
                    case TypeCode.Int16:
                        return UInt64Ops.PowerImpl(leftUInt64, (Int16)right);
                    case TypeCode.UInt16:
                        return UInt64Ops.PowerImpl(leftUInt64, (UInt16)right);
                    case TypeCode.Int32:
                        return UInt64Ops.PowerImpl(leftUInt64, (Int32)right);
                    case TypeCode.UInt32:
                        return UInt64Ops.PowerImpl(leftUInt64, (UInt32)right);
                    case TypeCode.Int64:
                        return UInt64Ops.PowerImpl(leftUInt64, (Int64)right);
                    case TypeCode.UInt64:
                        return UInt64Ops.PowerImpl(leftUInt64, (UInt64)right);
                    case TypeCode.Single:
                        return SingleOps.PowerImpl(leftUInt64, (Single)right);
                    case TypeCode.Double:
                        return FloatOps.Power(leftUInt64, (Double)right);
                }
            }
            if (right is BigInteger) {
                return LongOps.Power(leftUInt64, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return UInt64Ops.PowerImpl(leftUInt64, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.Power(leftUInt64, ((ExtensibleLong)right).Value);
            } else if (right is Complex64) {
                return ComplexOps.Power(leftUInt64, (Complex64)right);
            } else if (right is ExtensibleFloat) {
                return FloatOps.Power(leftUInt64, ((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return ComplexOps.Power(leftUInt64, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rpow__")]
        public static object ReversePower(object left, object right) {
            Debug.Assert(left is UInt64);
            UInt64 leftUInt64 = (UInt64)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean:
                        return UInt64Ops.ReversePowerImpl(leftUInt64, ((Boolean)right ? (UInt64)1 : (UInt64)0));
                    case TypeCode.Byte:
                        return UInt64Ops.ReversePowerImpl(leftUInt64, (Byte)right);
                    case TypeCode.SByte:
                        return UInt64Ops.ReversePowerImpl(leftUInt64, (SByte)right);
                    case TypeCode.Int16:
                        return UInt64Ops.ReversePowerImpl(leftUInt64, (Int16)right);
                    case TypeCode.UInt16:
                        return UInt64Ops.ReversePowerImpl(leftUInt64, (UInt16)right);
                    case TypeCode.Int32:
                        return UInt64Ops.ReversePowerImpl(leftUInt64, (Int32)right);
                    case TypeCode.UInt32:
                        return UInt64Ops.ReversePowerImpl(leftUInt64, (UInt32)right);
                    case TypeCode.Int64:
                        return UInt64Ops.ReversePowerImpl(leftUInt64, (Int64)right);
                    case TypeCode.UInt64:
                        return UInt64Ops.ReversePowerImpl(leftUInt64, (UInt64)right);
                    case TypeCode.Single:
                        return SingleOps.ReversePowerImpl(leftUInt64, (Single)right);
                    case TypeCode.Double:
                        return FloatOps.ReversePower(leftUInt64, (Double)right);
                }
            }
            if (right is BigInteger) {
                return LongOps.ReversePower(leftUInt64, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return UInt64Ops.ReversePowerImpl(leftUInt64, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.ReversePower(leftUInt64, ((ExtensibleLong)right).Value);
            } else if (right is Complex64) {
                return ComplexOps.ReversePower(leftUInt64, (Complex64)right);
            } else if (right is ExtensibleFloat) {
                return FloatOps.ReversePower(leftUInt64, ((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return ComplexOps.ReversePower(leftUInt64, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rshift__")]
        public static object RightShift(object left, object right) {
            Debug.Assert(left is UInt64);
            UInt64 leftUInt64 = (UInt64)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean:
                        return UInt64Ops.RightShiftImpl(leftUInt64, ((Boolean)right ? (UInt64)1 : (UInt64)0));
                    case TypeCode.Byte:
                        return UInt64Ops.RightShiftImpl(leftUInt64, (Byte)right);
                    case TypeCode.SByte:
                        return UInt64Ops.RightShiftImpl(leftUInt64, (SByte)right);
                    case TypeCode.Int16:
                        return UInt64Ops.RightShiftImpl(leftUInt64, (Int16)right);
                    case TypeCode.UInt16:
                        return UInt64Ops.RightShiftImpl(leftUInt64, (UInt16)right);
                    case TypeCode.Int32:
                        return UInt64Ops.RightShiftImpl(leftUInt64, (Int32)right);
                    case TypeCode.UInt32:
                        return UInt64Ops.RightShiftImpl(leftUInt64, (UInt32)right);
                    case TypeCode.Int64:
                        return UInt64Ops.RightShiftImpl(leftUInt64, (Int64)right);
                    case TypeCode.UInt64:
                        return UInt64Ops.RightShiftImpl(leftUInt64, (UInt64)right);
                }
            }
            if (right is BigInteger) {
                return LongOps.RightShift(leftUInt64, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return UInt64Ops.RightShiftImpl(leftUInt64, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.RightShift(leftUInt64, ((ExtensibleLong)right).Value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rrshift__")]
        public static object ReverseRightShift(object left, object right) {
            Debug.Assert(left is UInt64);
            UInt64 leftUInt64 = (UInt64)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean:
                        return UInt64Ops.ReverseRightShiftImpl(leftUInt64, ((Boolean)right ? (UInt64)1 : (UInt64)0));
                    case TypeCode.Byte:
                        return UInt64Ops.ReverseRightShiftImpl(leftUInt64, (Byte)right);
                    case TypeCode.SByte:
                        return UInt64Ops.ReverseRightShiftImpl(leftUInt64, (SByte)right);
                    case TypeCode.Int16:
                        return UInt64Ops.ReverseRightShiftImpl(leftUInt64, (Int16)right);
                    case TypeCode.UInt16:
                        return UInt64Ops.ReverseRightShiftImpl(leftUInt64, (UInt16)right);
                    case TypeCode.Int32:
                        return UInt64Ops.ReverseRightShiftImpl(leftUInt64, (Int32)right);
                    case TypeCode.UInt32:
                        return UInt64Ops.ReverseRightShiftImpl(leftUInt64, (UInt32)right);
                    case TypeCode.Int64:
                        return UInt64Ops.ReverseRightShiftImpl(leftUInt64, (Int64)right);
                    case TypeCode.UInt64:
                        return UInt64Ops.ReverseRightShiftImpl(leftUInt64, (UInt64)right);
                }
            }
            if (right is BigInteger) {
                return LongOps.ReverseRightShift(leftUInt64, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return UInt64Ops.ReverseRightShiftImpl(leftUInt64, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.ReverseRightShift(leftUInt64, ((ExtensibleLong)right).Value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__truediv__")]
        public static object TrueDivide(object left, object right) {
            Debug.Assert(left is UInt64);
            UInt64 leftUInt64 = (UInt64)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean:
                        return FloatOps.TrueDivide(leftUInt64, ((Boolean)right ? (UInt64)1 : (UInt64)0));
                    case TypeCode.Byte:
                        return FloatOps.TrueDivide(leftUInt64, (Byte)right);
                    case TypeCode.SByte:
                        return FloatOps.TrueDivide(leftUInt64, (SByte)right);
                    case TypeCode.Int16:
                        return FloatOps.TrueDivide(leftUInt64, (Int16)right);
                    case TypeCode.UInt16:
                        return FloatOps.TrueDivide(leftUInt64, (UInt16)right);
                    case TypeCode.Int32:
                        return FloatOps.TrueDivide(leftUInt64, (Int32)right);
                    case TypeCode.UInt32:
                        return FloatOps.TrueDivide(leftUInt64, (UInt32)right);
                    case TypeCode.Int64:
                        return FloatOps.TrueDivide(leftUInt64, (Int64)right);
                    case TypeCode.UInt64:
                        return FloatOps.TrueDivide(leftUInt64, (UInt64)right);
                    case TypeCode.Single:
                        return FloatOps.TrueDivide(leftUInt64, (Single)right);
                    case TypeCode.Double:
                        return FloatOps.TrueDivide(leftUInt64, (Double)right);
                }
            }
            if (right is BigInteger) {
                return FloatOps.TrueDivide(leftUInt64, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return FloatOps.TrueDivide(leftUInt64, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return FloatOps.TrueDivide(leftUInt64, ((ExtensibleLong)right).Value);
            } else if (right is Complex64) {
                return FloatOps.TrueDivide(leftUInt64, (Complex64)right);
            } else if (right is ExtensibleFloat) {
                return FloatOps.TrueDivide(leftUInt64, ((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return FloatOps.TrueDivide(leftUInt64, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rtruediv__")]
        public static object ReverseTrueDivide(object left, object right) {
            Debug.Assert(left is UInt64);
            UInt64 leftUInt64 = (UInt64)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean:
                        return FloatOps.ReverseTrueDivide(leftUInt64, ((Boolean)right ? (UInt64)1 : (UInt64)0));
                    case TypeCode.Byte:
                        return FloatOps.ReverseTrueDivide(leftUInt64, (Byte)right);
                    case TypeCode.SByte:
                        return FloatOps.ReverseTrueDivide(leftUInt64, (SByte)right);
                    case TypeCode.Int16:
                        return FloatOps.ReverseTrueDivide(leftUInt64, (Int16)right);
                    case TypeCode.UInt16:
                        return FloatOps.ReverseTrueDivide(leftUInt64, (UInt16)right);
                    case TypeCode.Int32:
                        return FloatOps.ReverseTrueDivide(leftUInt64, (Int32)right);
                    case TypeCode.UInt32:
                        return FloatOps.ReverseTrueDivide(leftUInt64, (UInt32)right);
                    case TypeCode.Int64:
                        return FloatOps.ReverseTrueDivide(leftUInt64, (Int64)right);
                    case TypeCode.UInt64:
                        return FloatOps.ReverseTrueDivide(leftUInt64, (UInt64)right);
                    case TypeCode.Single:
                        return FloatOps.ReverseTrueDivide(leftUInt64, (Single)right);
                    case TypeCode.Double:
                        return FloatOps.ReverseTrueDivide(leftUInt64, (Double)right);
                }
            }
            if (right is BigInteger) {
                return FloatOps.ReverseTrueDivide(leftUInt64, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return FloatOps.ReverseTrueDivide(leftUInt64, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return FloatOps.ReverseTrueDivide(leftUInt64, ((ExtensibleLong)right).Value);
            } else if (right is Complex64) {
                return FloatOps.ReverseTrueDivide(leftUInt64, (Complex64)right);
            } else if (right is ExtensibleFloat) {
                return FloatOps.ReverseTrueDivide(leftUInt64, ((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return FloatOps.ReverseTrueDivide(leftUInt64, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        internal static object DivideImpl(UInt64 x, UInt64 y) {
            return (UInt64)(x / y);
        }
        internal static object ModImpl(UInt64 x, UInt64 y) {
            return (UInt64)(x % y);
        }


        internal static object DivModImpl(UInt64 x, UInt64 y) {
            object div = DivideImpl(x, y);
            if (div == Ops.NotImplemented) return div;
            object mod = ModImpl(x, y);
            if (mod == Ops.NotImplemented) return mod;
            return Tuple.MakeTuple(div, mod);
        }
        internal static object ReverseDivideImpl(UInt64 x, UInt64 y) {
            return DivideImpl(y, x);
        }
        internal static object ReverseModImpl(UInt64 x, UInt64 y) {
            return ModImpl(y, x);
        }
        internal static object ReverseDivModImpl(UInt64 x, UInt64 y) {
            return DivModImpl(y, x);
        }
        internal static object FloorDivideImpl(UInt64 x, UInt64 y) {
            return DivideImpl(x, y);
        }
        internal static object ReverseFloorDivideImpl(UInt64 x, UInt64 y) {
            return DivideImpl(y, x);
        }
        internal static object ReverseLeftShiftImpl(UInt64 x, UInt64 y) {
            return LeftShiftImpl(y, x);
        }
        internal static object ReversePowerImpl(UInt64 x, UInt64 y) {
            return PowerImpl(y, x);
        }
        internal static object ReverseRightShiftImpl(UInt64 x, UInt64 y) {
            return RightShiftImpl(y, x);
        }


        // *** END GENERATED CODE ***

        #endregion

    }
}
