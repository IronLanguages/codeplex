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

namespace IronPython.Runtime {
    static partial class UInt16Ops {

        #region Generated UInt16Ops

        // *** BEGIN GENERATED CODE ***

        public static DynamicType MakeDynamicType() {
            return new OpsReflectedType("UInt16", typeof(UInt16), typeof(UInt16Ops), null);
        }

        [PythonName("__add__")]
        public static object Add(object left, object right) {
            Debug.Assert(left is UInt16);
            UInt16 leftUInt16 = (UInt16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            Int32 result = (Int32)(((Int32)leftUInt16) + ((Int32)((Byte)right)));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.SByte: {
                            Int32 result = (Int32)(((Int32)leftUInt16) + ((Int32)((SByte)right)));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.Int16: {
                            Int32 result = (Int32)(((Int32)leftUInt16) + ((Int32)((Int16)right)));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.UInt16: {
                            Int32 result = (Int32)(((Int32)leftUInt16) + ((Int32)((UInt16)right)));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.Int32: {
                            Int64 result = (Int64)(((Int64)leftUInt16) + ((Int64)((Int32)right)));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.UInt32: {
                            Int64 result = (Int64)(((Int64)leftUInt16) + ((Int64)((UInt32)right)));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.Add(leftUInt16, (Int64)right);
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.AddImpl(leftUInt16, (UInt64)right);
                        }
                    case TypeCode.Single: {
                            return (Single)(((Single)leftUInt16) + ((Single)((Single)right)));
                        }
                    case TypeCode.Double: {
                            return (Double)(((Double)leftUInt16) + ((Double)((Double)right)));
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__div__")]
        public static object Divide(object left, object right) {
            Debug.Assert(left is UInt16);
            UInt16 leftUInt16 = (UInt16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            return UInt16Ops.DivideImpl((UInt16)leftUInt16, (UInt16)((Byte)right));
                        }
                    case TypeCode.SByte: {
                            return IntOps.Divide((Int32)leftUInt16, (Int32)((SByte)right));
                        }
                    case TypeCode.Int16: {
                            return IntOps.Divide((Int32)leftUInt16, (Int32)((Int16)right));
                        }
                    case TypeCode.UInt16: {
                            return UInt16Ops.DivideImpl((UInt16)leftUInt16, (UInt16)((UInt16)right));
                        }
                    case TypeCode.Int32: {
                            return IntOps.Divide((Int32)leftUInt16, (Int32)((Int32)right));
                        }
                    case TypeCode.UInt32: {
                            return UInt32Ops.DivideImpl((UInt32)leftUInt16, (UInt32)((UInt32)right));
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.Divide((Int64)leftUInt16, (Int64)((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.DivideImpl((UInt64)leftUInt16, (UInt64)((UInt64)right));
                        }
                    case TypeCode.Single: {
                            return SingleOps.DivideImpl((Single)leftUInt16, (Single)((Single)right));
                        }
                    case TypeCode.Double: {
                            return FloatOps.Divide((Double)leftUInt16, (Double)((Double)right));
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__floordiv__")]
        public static object FloorDivide(object left, object right) {
            Debug.Assert(left is UInt16);
            UInt16 leftUInt16 = (UInt16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            return UInt16Ops.FloorDivideImpl((UInt16)leftUInt16, (UInt16)((Byte)right));
                        }
                    case TypeCode.SByte: {
                            return IntOps.FloorDivide((Int32)leftUInt16, (Int32)((SByte)right));
                        }
                    case TypeCode.Int16: {
                            return IntOps.FloorDivide((Int32)leftUInt16, (Int32)((Int16)right));
                        }
                    case TypeCode.UInt16: {
                            return UInt16Ops.FloorDivideImpl((UInt16)leftUInt16, (UInt16)((UInt16)right));
                        }
                    case TypeCode.Int32: {
                            return IntOps.FloorDivide((Int32)leftUInt16, (Int32)((Int32)right));
                        }
                    case TypeCode.UInt32: {
                            return UInt32Ops.FloorDivideImpl((UInt32)leftUInt16, (UInt32)((UInt32)right));
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.FloorDivide((Int64)leftUInt16, (Int64)((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.FloorDivideImpl((UInt64)leftUInt16, (UInt64)((UInt64)right));
                        }
                    case TypeCode.Single: {
                            return SingleOps.FloorDivideImpl((Single)leftUInt16, (Single)((Single)right));
                        }
                    case TypeCode.Double: {
                            return FloatOps.FloorDivide((Double)leftUInt16, (Double)((Double)right));
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__mod__")]
        public static object Mod(object left, object right) {
            Debug.Assert(left is UInt16);
            UInt16 leftUInt16 = (UInt16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            return UInt16Ops.ModImpl((UInt16)leftUInt16, (UInt16)((Byte)right));
                        }
                    case TypeCode.SByte: {
                            return IntOps.Mod((Int32)leftUInt16, (Int32)((SByte)right));
                        }
                    case TypeCode.Int16: {
                            return IntOps.Mod((Int32)leftUInt16, (Int32)((Int16)right));
                        }
                    case TypeCode.UInt16: {
                            return UInt16Ops.ModImpl((UInt16)leftUInt16, (UInt16)((UInt16)right));
                        }
                    case TypeCode.Int32: {
                            return IntOps.Mod((Int32)leftUInt16, (Int32)((Int32)right));
                        }
                    case TypeCode.UInt32: {
                            return UInt32Ops.ModImpl((UInt32)leftUInt16, (UInt32)((UInt32)right));
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.Mod((Int64)leftUInt16, (Int64)((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ModImpl((UInt64)leftUInt16, (UInt64)((UInt64)right));
                        }
                    case TypeCode.Single: {
                            return SingleOps.ModImpl((Single)leftUInt16, (Single)((Single)right));
                        }
                    case TypeCode.Double: {
                            return FloatOps.Mod((Double)leftUInt16, (Double)((Double)right));
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__mul__")]
        public static object Multiply(object left, object right) {
            Debug.Assert(left is UInt16);
            UInt16 leftUInt16 = (UInt16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            Int32 result = (Int32)(((Int32)leftUInt16) * ((Int32)((Byte)right)));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.SByte: {
                            Int32 result = (Int32)(((Int32)leftUInt16) * ((Int32)((SByte)right)));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.Int16: {
                            Int32 result = (Int32)(((Int32)leftUInt16) * ((Int32)((Int16)right)));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.UInt16: {
                            UInt32 result = (UInt32)(((UInt32)leftUInt16) * ((UInt32)((UInt16)right)));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.Int32: {
                            Int64 result = (Int64)(((Int64)leftUInt16) * ((Int64)((Int32)right)));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.UInt32: {
                            Int64 result = (Int64)(((Int64)leftUInt16) * ((Int64)((UInt32)right)));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.Multiply(leftUInt16, (Int64)right);
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.MultiplyImpl(leftUInt16, (UInt64)right);
                        }
                    case TypeCode.Single: {
                            return (Double)(((Double)leftUInt16) * ((Double)((Single)right)));
                        }
                    case TypeCode.Double: {
                            return FloatOps.Multiply(leftUInt16, (Double)right);
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__sub__")]
        public static object Subtract(object left, object right) {
            Debug.Assert(left is UInt16);
            UInt16 leftUInt16 = (UInt16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            Int32 result = (Int32)(((Int32)leftUInt16) - ((Int32)((Byte)right)));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.SByte: {
                            Int32 result = (Int32)(((Int32)leftUInt16) - ((Int32)((SByte)right)));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.Int16: {
                            Int32 result = (Int32)(((Int32)leftUInt16) - ((Int32)((Int16)right)));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.UInt16: {
                            Int32 result = (Int32)(((Int32)leftUInt16) - ((Int32)((UInt16)right)));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.Int32: {
                            Int64 result = (Int64)(((Int64)leftUInt16) - ((Int64)((Int32)right)));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.UInt32: {
                            Int64 result = (Int64)(((Int64)leftUInt16) - ((Int64)((UInt32)right)));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.Subtract(leftUInt16, (Int64)right);
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.SubtractImpl(leftUInt16, (UInt64)right);
                        }
                    case TypeCode.Single: {
                            return (Single)(((Single)leftUInt16) - ((Single)((Single)right)));
                        }
                    case TypeCode.Double: {
                            return (Double)(((Double)leftUInt16) - ((Double)((Double)right)));
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__radd__")]
        public static object ReverseAdd(object left, object right) {
            Debug.Assert(left is UInt16);
            UInt16 leftUInt16 = (UInt16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            Int32 result = (Int32)(((Int32)((Byte)right)) + ((Int32)leftUInt16));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.SByte: {
                            Int32 result = (Int32)(((Int32)((SByte)right)) + ((Int32)leftUInt16));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.Int16: {
                            Int32 result = (Int32)(((Int32)((Int16)right)) + ((Int32)leftUInt16));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.UInt16: {
                            Int32 result = (Int32)(((Int32)((UInt16)right)) + ((Int32)leftUInt16));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.Int32: {
                            Int64 result = (Int64)(((Int64)((Int32)right)) + ((Int64)leftUInt16));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.UInt32: {
                            Int64 result = (Int64)(((Int64)((UInt32)right)) + ((Int64)leftUInt16));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.ReverseAdd(leftUInt16, (Int64)right);
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ReverseAddImpl(leftUInt16, (UInt64)right);
                        }
                    case TypeCode.Single: {
                            return (Single)(((Single)((Single)right)) + ((Single)leftUInt16));
                        }
                    case TypeCode.Double: {
                            return (Double)(((Double)((Double)right)) + ((Double)leftUInt16));
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rdiv__")]
        public static object ReverseDivide(object left, object right) {
            Debug.Assert(left is UInt16);
            UInt16 leftUInt16 = (UInt16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            return UInt16Ops.ReverseDivideImpl((UInt16)leftUInt16, (UInt16)((Byte)right));
                        }
                    case TypeCode.SByte: {
                            return IntOps.ReverseDivide((Int32)leftUInt16, (Int32)((SByte)right));
                        }
                    case TypeCode.Int16: {
                            return IntOps.ReverseDivide((Int32)leftUInt16, (Int32)((Int16)right));
                        }
                    case TypeCode.UInt16: {
                            return UInt16Ops.ReverseDivideImpl((UInt16)leftUInt16, (UInt16)((UInt16)right));
                        }
                    case TypeCode.Int32: {
                            return IntOps.ReverseDivide((Int32)leftUInt16, (Int32)((Int32)right));
                        }
                    case TypeCode.UInt32: {
                            return UInt32Ops.ReverseDivideImpl((UInt32)leftUInt16, (UInt32)((UInt32)right));
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.ReverseDivide((Int64)leftUInt16, (Int64)((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ReverseDivideImpl((UInt64)leftUInt16, (UInt64)((UInt64)right));
                        }
                    case TypeCode.Single: {
                            return SingleOps.ReverseDivideImpl((Single)leftUInt16, (Single)((Single)right));
                        }
                    case TypeCode.Double: {
                            return FloatOps.ReverseDivide((Double)leftUInt16, (Double)((Double)right));
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rfloordiv__")]
        public static object ReverseFloorDivide(object left, object right) {
            Debug.Assert(left is UInt16);
            UInt16 leftUInt16 = (UInt16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            return UInt16Ops.ReverseFloorDivideImpl((UInt16)leftUInt16, (UInt16)((Byte)right));
                        }
                    case TypeCode.SByte: {
                            return IntOps.ReverseFloorDivide((Int32)leftUInt16, (Int32)((SByte)right));
                        }
                    case TypeCode.Int16: {
                            return IntOps.ReverseFloorDivide((Int32)leftUInt16, (Int32)((Int16)right));
                        }
                    case TypeCode.UInt16: {
                            return UInt16Ops.ReverseFloorDivideImpl((UInt16)leftUInt16, (UInt16)((UInt16)right));
                        }
                    case TypeCode.Int32: {
                            return IntOps.ReverseFloorDivide((Int32)leftUInt16, (Int32)((Int32)right));
                        }
                    case TypeCode.UInt32: {
                            return UInt32Ops.ReverseFloorDivideImpl((UInt32)leftUInt16, (UInt32)((UInt32)right));
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.ReverseFloorDivide((Int64)leftUInt16, (Int64)((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ReverseFloorDivideImpl((UInt64)leftUInt16, (UInt64)((UInt64)right));
                        }
                    case TypeCode.Single: {
                            return SingleOps.ReverseFloorDivideImpl((Single)leftUInt16, (Single)((Single)right));
                        }
                    case TypeCode.Double: {
                            return FloatOps.ReverseFloorDivide((Double)leftUInt16, (Double)((Double)right));
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rmod__")]
        public static object ReverseMod(object left, object right) {
            Debug.Assert(left is UInt16);
            UInt16 leftUInt16 = (UInt16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            return UInt16Ops.ReverseModImpl((UInt16)leftUInt16, (UInt16)((Byte)right));
                        }
                    case TypeCode.SByte: {
                            return IntOps.ReverseMod((Int32)leftUInt16, (Int32)((SByte)right));
                        }
                    case TypeCode.Int16: {
                            return IntOps.ReverseMod((Int32)leftUInt16, (Int32)((Int16)right));
                        }
                    case TypeCode.UInt16: {
                            return UInt16Ops.ReverseModImpl((UInt16)leftUInt16, (UInt16)((UInt16)right));
                        }
                    case TypeCode.Int32: {
                            return IntOps.ReverseMod((Int32)leftUInt16, (Int32)((Int32)right));
                        }
                    case TypeCode.UInt32: {
                            return UInt32Ops.ReverseModImpl((UInt32)leftUInt16, (UInt32)((UInt32)right));
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.ReverseMod((Int64)leftUInt16, (Int64)((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ReverseModImpl((UInt64)leftUInt16, (UInt64)((UInt64)right));
                        }
                    case TypeCode.Single: {
                            return SingleOps.ReverseModImpl((Single)leftUInt16, (Single)((Single)right));
                        }
                    case TypeCode.Double: {
                            return FloatOps.ReverseMod((Double)leftUInt16, (Double)((Double)right));
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rmul__")]
        public static object ReverseMultiply(object left, object right) {
            Debug.Assert(left is UInt16);
            UInt16 leftUInt16 = (UInt16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            Int32 result = (Int32)(((Int32)((Byte)right)) * ((Int32)leftUInt16));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.SByte: {
                            Int32 result = (Int32)(((Int32)((SByte)right)) * ((Int32)leftUInt16));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.Int16: {
                            Int32 result = (Int32)(((Int32)((Int16)right)) * ((Int32)leftUInt16));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.UInt16: {
                            UInt32 result = (UInt32)(((UInt32)((UInt16)right)) * ((UInt32)leftUInt16));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.Int32: {
                            Int64 result = (Int64)(((Int64)((Int32)right)) * ((Int64)leftUInt16));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.UInt32: {
                            Int64 result = (Int64)(((Int64)((UInt32)right)) * ((Int64)leftUInt16));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.ReverseMultiply(leftUInt16, (Int64)right);
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ReverseMultiplyImpl(leftUInt16, (UInt64)right);
                        }
                    case TypeCode.Single: {
                            return (Double)(((Double)((Single)right)) * ((Double)leftUInt16));
                        }
                    case TypeCode.Double: {
                            return FloatOps.ReverseMultiply(leftUInt16, (Double)right);
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rsub__")]
        public static object ReverseSubtract(object left, object right) {
            Debug.Assert(left is UInt16);
            UInt16 leftUInt16 = (UInt16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            Int32 result = (Int32)(((Int32)((Byte)right)) - ((Int32)leftUInt16));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.SByte: {
                            Int32 result = (Int32)(((Int32)((SByte)right)) - ((Int32)leftUInt16));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.Int16: {
                            Int32 result = (Int32)(((Int32)((Int16)right)) - ((Int32)leftUInt16));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.UInt16: {
                            Int32 result = (Int32)(((Int32)((UInt16)right)) - ((Int32)leftUInt16));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.Int32: {
                            Int64 result = (Int64)(((Int64)((Int32)right)) - ((Int64)leftUInt16));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.UInt32: {
                            Int64 result = (Int64)(((Int64)((UInt32)right)) - ((Int64)leftUInt16));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.ReverseSubtract(leftUInt16, (Int64)right);
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ReverseSubtractImpl(leftUInt16, (UInt64)right);
                        }
                    case TypeCode.Single: {
                            return (Single)(((Single)((Single)right)) - ((Single)leftUInt16));
                        }
                    case TypeCode.Double: {
                            return (Double)(((Double)((Double)right)) - ((Double)leftUInt16));
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__and__")]
        public static object BitwiseAnd(object left, object right) {
            Debug.Assert(left is UInt16);
            UInt16 leftUInt16 = (UInt16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            UInt16 rightUInt16 = (UInt16)(Byte)right;
                            return leftUInt16 & rightUInt16;
                        }
                    case TypeCode.SByte: {
                            Int32 leftInt32 = (Int32)leftUInt16;
                            Int32 rightInt32 = (Int32)(SByte)right;
                            return leftInt32 & rightInt32;
                        }
                    case TypeCode.Int16: {
                            Int32 leftInt32 = (Int32)leftUInt16;
                            Int32 rightInt32 = (Int32)(Int16)right;
                            return leftInt32 & rightInt32;
                        }
                    case TypeCode.UInt16: {
                            return leftUInt16 & (UInt16)right;
                        }
                    case TypeCode.Int32: {
                            Int32 leftInt32 = (Int32)leftUInt16;
                            return leftInt32 & (Int32)right;
                        }
                    case TypeCode.UInt32: {
                            UInt32 leftUInt32 = (UInt32)leftUInt16;
                            return leftUInt32 & (UInt32)right;
                        }
                    case TypeCode.Int64: {
                            Int64 leftInt64 = (Int64)leftUInt16;
                            return leftInt64 & (Int64)right;
                        }
                    case TypeCode.UInt64: {
                            UInt64 leftUInt64 = (UInt64)leftUInt16;
                            return leftUInt64 & (UInt64)right;
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rand__")]
        public static object ReverseBitwiseAnd(object left, object right) {
            Debug.Assert(left is UInt16);
            UInt16 leftUInt16 = (UInt16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            UInt16 rightUInt16 = (UInt16)(Byte)right;
                            return leftUInt16 & rightUInt16;
                        }
                    case TypeCode.SByte: {
                            Int32 leftInt32 = (Int32)leftUInt16;
                            Int32 rightInt32 = (Int32)(SByte)right;
                            return leftInt32 & rightInt32;
                        }
                    case TypeCode.Int16: {
                            Int32 leftInt32 = (Int32)leftUInt16;
                            Int32 rightInt32 = (Int32)(Int16)right;
                            return leftInt32 & rightInt32;
                        }
                    case TypeCode.UInt16: {
                            return leftUInt16 & (UInt16)right;
                        }
                    case TypeCode.Int32: {
                            Int32 leftInt32 = (Int32)leftUInt16;
                            return leftInt32 & (Int32)right;
                        }
                    case TypeCode.UInt32: {
                            UInt32 leftUInt32 = (UInt32)leftUInt16;
                            return leftUInt32 & (UInt32)right;
                        }
                    case TypeCode.Int64: {
                            Int64 leftInt64 = (Int64)leftUInt16;
                            return leftInt64 & (Int64)right;
                        }
                    case TypeCode.UInt64: {
                            UInt64 leftUInt64 = (UInt64)leftUInt16;
                            return leftUInt64 & (UInt64)right;
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__or__")]
        public static object BitwiseOr(object left, object right) {
            Debug.Assert(left is UInt16);
            UInt16 leftUInt16 = (UInt16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            UInt16 rightUInt16 = (UInt16)(Byte)right;
                            return leftUInt16 | rightUInt16;
                        }
                    case TypeCode.SByte: {
                            Int32 leftInt32 = (Int32)leftUInt16;
                            Int32 rightInt32 = (Int32)(SByte)right;
                            return leftInt32 | rightInt32;
                        }
                    case TypeCode.Int16: {
                            Int32 leftInt32 = (Int32)leftUInt16;
                            Int32 rightInt32 = (Int32)(Int16)right;
                            return leftInt32 | rightInt32;
                        }
                    case TypeCode.UInt16: {
                            return leftUInt16 | (UInt16)right;
                        }
                    case TypeCode.Int32: {
                            Int32 leftInt32 = (Int32)leftUInt16;
                            return leftInt32 | (Int32)right;
                        }
                    case TypeCode.UInt32: {
                            UInt32 leftUInt32 = (UInt32)leftUInt16;
                            return leftUInt32 | (UInt32)right;
                        }
                    case TypeCode.Int64: {
                            Int64 leftInt64 = (Int64)leftUInt16;
                            return leftInt64 | (Int64)right;
                        }
                    case TypeCode.UInt64: {
                            UInt64 leftUInt64 = (UInt64)leftUInt16;
                            return leftUInt64 | (UInt64)right;
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__ror__")]
        public static object ReverseBitwiseOr(object left, object right) {
            Debug.Assert(left is UInt16);
            UInt16 leftUInt16 = (UInt16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            UInt16 rightUInt16 = (UInt16)(Byte)right;
                            return leftUInt16 | rightUInt16;
                        }
                    case TypeCode.SByte: {
                            Int32 leftInt32 = (Int32)leftUInt16;
                            Int32 rightInt32 = (Int32)(SByte)right;
                            return leftInt32 | rightInt32;
                        }
                    case TypeCode.Int16: {
                            Int32 leftInt32 = (Int32)leftUInt16;
                            Int32 rightInt32 = (Int32)(Int16)right;
                            return leftInt32 | rightInt32;
                        }
                    case TypeCode.UInt16: {
                            return leftUInt16 | (UInt16)right;
                        }
                    case TypeCode.Int32: {
                            Int32 leftInt32 = (Int32)leftUInt16;
                            return leftInt32 | (Int32)right;
                        }
                    case TypeCode.UInt32: {
                            UInt32 leftUInt32 = (UInt32)leftUInt16;
                            return leftUInt32 | (UInt32)right;
                        }
                    case TypeCode.Int64: {
                            Int64 leftInt64 = (Int64)leftUInt16;
                            return leftInt64 | (Int64)right;
                        }
                    case TypeCode.UInt64: {
                            UInt64 leftUInt64 = (UInt64)leftUInt16;
                            return leftUInt64 | (UInt64)right;
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rxor__")]
        public static object BitwiseXor(object left, object right) {
            Debug.Assert(left is UInt16);
            UInt16 leftUInt16 = (UInt16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            UInt16 rightUInt16 = (UInt16)(Byte)right;
                            return leftUInt16 ^ rightUInt16;
                        }
                    case TypeCode.SByte: {
                            Int32 leftInt32 = (Int32)leftUInt16;
                            Int32 rightInt32 = (Int32)(SByte)right;
                            return leftInt32 ^ rightInt32;
                        }
                    case TypeCode.Int16: {
                            Int32 leftInt32 = (Int32)leftUInt16;
                            Int32 rightInt32 = (Int32)(Int16)right;
                            return leftInt32 ^ rightInt32;
                        }
                    case TypeCode.UInt16: {
                            return leftUInt16 ^ (UInt16)right;
                        }
                    case TypeCode.Int32: {
                            Int32 leftInt32 = (Int32)leftUInt16;
                            return leftInt32 ^ (Int32)right;
                        }
                    case TypeCode.UInt32: {
                            UInt32 leftUInt32 = (UInt32)leftUInt16;
                            return leftUInt32 ^ (UInt32)right;
                        }
                    case TypeCode.Int64: {
                            Int64 leftInt64 = (Int64)leftUInt16;
                            return leftInt64 ^ (Int64)right;
                        }
                    case TypeCode.UInt64: {
                            UInt64 leftUInt64 = (UInt64)leftUInt16;
                            return leftUInt64 ^ (UInt64)right;
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__xor__")]
        public static object ReverseBitwiseXor(object left, object right) {
            Debug.Assert(left is UInt16);
            UInt16 leftUInt16 = (UInt16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            UInt16 rightUInt16 = (UInt16)(Byte)right;
                            return leftUInt16 ^ rightUInt16;
                        }
                    case TypeCode.SByte: {
                            Int32 leftInt32 = (Int32)leftUInt16;
                            Int32 rightInt32 = (Int32)(SByte)right;
                            return leftInt32 ^ rightInt32;
                        }
                    case TypeCode.Int16: {
                            Int32 leftInt32 = (Int32)leftUInt16;
                            Int32 rightInt32 = (Int32)(Int16)right;
                            return leftInt32 ^ rightInt32;
                        }
                    case TypeCode.UInt16: {
                            return leftUInt16 ^ (UInt16)right;
                        }
                    case TypeCode.Int32: {
                            Int32 leftInt32 = (Int32)leftUInt16;
                            return leftInt32 ^ (Int32)right;
                        }
                    case TypeCode.UInt32: {
                            UInt32 leftUInt32 = (UInt32)leftUInt16;
                            return leftUInt32 ^ (UInt32)right;
                        }
                    case TypeCode.Int64: {
                            Int64 leftInt64 = (Int64)leftUInt16;
                            return leftInt64 ^ (Int64)right;
                        }
                    case TypeCode.UInt64: {
                            UInt64 leftUInt64 = (UInt64)leftUInt16;
                            return leftUInt64 ^ (UInt64)right;
                        }
                }
            }
            return Ops.NotImplemented;
        }
        internal static object DivideImpl(UInt16 x, UInt16 y) {
            UInt16 q = (UInt16)(x / y);
            if (x >= 0) {
                if (y > 0) return q;
                else if (x % y == 0) return q;
                else return q - 1;
            } else {
                if (y > 0) {
                    if (x % y == 0) return q;
                    else return q - 1;
                } else return q;
            }
        }

        internal static object ReverseDivideImpl(UInt16 x, UInt16 y) {
            return DivideImpl(y, x);
        }

        internal static object ModImpl(UInt16 x, UInt16 y) {
            UInt16 r = (UInt16)(x % y);
            if (x >= 0) {
                if (y > 0) return r;
                else if (r == 0) return 0;
                else return r + y;
            } else {
                if (y > 0) {
                    if (r == 0) return r;
                    else return r + y;
                } else return r;
            }
        }

        internal static object ReverseModImpl(UInt16 x, UInt16 y) {
            return ModImpl(y, x);
        }

        internal static object FloorDivideImpl(UInt16 x, UInt16 y) {
            return DivideImpl(x, y);
        }

        internal static object ReverseFloorDivideImpl(UInt16 x, UInt16 y) {
            return DivideImpl(y, x);
        }


        // *** END GENERATED CODE ***

        #endregion

    }
}
