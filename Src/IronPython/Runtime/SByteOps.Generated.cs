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
    static partial class SByteOps {
        #region Generated SByteOps

        // *** BEGIN GENERATED CODE ***

        public static DynamicType MakeDynamicType() {
            return new OpsReflectedType("SByte", typeof(SByte), typeof(SByteOps), null);
        }

        [PythonName("__add__")]
        public static object Add(object left, object right) {
            Debug.Assert(left is SByte);
            SByte leftSByte = (SByte)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            Int16 result = (Int16)(((Int16)leftSByte) + ((Int16)((Byte)right)));
                            if (SByte.MinValue <= result && result <= SByte.MaxValue) {
                                return (SByte)result;
                            } else return result;
                        }
                    case TypeCode.SByte: {
                            Int16 result = (Int16)(((Int16)leftSByte) + ((Int16)((SByte)right)));
                            if (SByte.MinValue <= result && result <= SByte.MaxValue) {
                                return (SByte)result;
                            } else return result;
                        }
                    case TypeCode.Int16: {
                            Int32 result = (Int32)(((Int32)leftSByte) + ((Int32)((Int16)right)));
                            if (SByte.MinValue <= result && result <= SByte.MaxValue) {
                                return (SByte)result;
                            } else return result;
                        }
                    case TypeCode.UInt16: {
                            Int32 result = (Int32)(((Int32)leftSByte) + ((Int32)((UInt16)right)));
                            if (SByte.MinValue <= result && result <= SByte.MaxValue) {
                                return (SByte)result;
                            } else return result;
                        }
                    case TypeCode.Int32: {
                            Int64 result = (Int64)(((Int64)leftSByte) + ((Int64)((Int32)right)));
                            if (SByte.MinValue <= result && result <= SByte.MaxValue) {
                                return (SByte)result;
                            } else return result;
                        }
                    case TypeCode.UInt32: {
                            Int64 result = (Int64)(((Int64)leftSByte) + ((Int64)((UInt32)right)));
                            if (SByte.MinValue <= result && result <= SByte.MaxValue) {
                                return (SByte)result;
                            } else return result;
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.Add(leftSByte, (Int64)right);
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.AddImpl(leftSByte, (UInt64)right);
                        }
                    case TypeCode.Single: {
                            return (Single)(((Single)leftSByte) + ((Single)((Single)right)));
                        }
                    case TypeCode.Double: {
                            return (Double)(((Double)leftSByte) + ((Double)((Double)right)));
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__div__")]
        public static object Divide(object left, object right) {
            Debug.Assert(left is SByte);
            SByte leftSByte = (SByte)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            return Int16Ops.DivideImpl((Int16)leftSByte, (Int16)((Byte)right));
                        }
                    case TypeCode.SByte: {
                            return SByteOps.DivideImpl((SByte)leftSByte, (SByte)((SByte)right));
                        }
                    case TypeCode.Int16: {
                            return Int16Ops.DivideImpl((Int16)leftSByte, (Int16)((Int16)right));
                        }
                    case TypeCode.UInt16: {
                            return IntOps.Divide((Int32)leftSByte, (Int32)((UInt16)right));
                        }
                    case TypeCode.Int32: {
                            return IntOps.Divide((Int32)leftSByte, (Int32)((Int32)right));
                        }
                    case TypeCode.UInt32: {
                            return Int64Ops.Divide((Int64)leftSByte, (Int64)((UInt32)right));
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.Divide((Int64)leftSByte, (Int64)((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.DivideImpl(leftSByte, (UInt64)right);
                        }
                    case TypeCode.Single: {
                            return SingleOps.DivideImpl((Single)leftSByte, (Single)((Single)right));
                        }
                    case TypeCode.Double: {
                            return FloatOps.Divide((Double)leftSByte, (Double)((Double)right));
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__floordiv__")]
        public static object FloorDivide(object left, object right) {
            Debug.Assert(left is SByte);
            SByte leftSByte = (SByte)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            return Int16Ops.FloorDivideImpl((Int16)leftSByte, (Int16)((Byte)right));
                        }
                    case TypeCode.SByte: {
                            return SByteOps.FloorDivideImpl((SByte)leftSByte, (SByte)((SByte)right));
                        }
                    case TypeCode.Int16: {
                            return Int16Ops.FloorDivideImpl((Int16)leftSByte, (Int16)((Int16)right));
                        }
                    case TypeCode.UInt16: {
                            return IntOps.FloorDivide((Int32)leftSByte, (Int32)((UInt16)right));
                        }
                    case TypeCode.Int32: {
                            return IntOps.FloorDivide((Int32)leftSByte, (Int32)((Int32)right));
                        }
                    case TypeCode.UInt32: {
                            return Int64Ops.FloorDivide((Int64)leftSByte, (Int64)((UInt32)right));
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.FloorDivide((Int64)leftSByte, (Int64)((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.FloorDivideImpl(leftSByte, (UInt64)right);
                        }
                    case TypeCode.Single: {
                            return SingleOps.FloorDivideImpl((Single)leftSByte, (Single)((Single)right));
                        }
                    case TypeCode.Double: {
                            return FloatOps.FloorDivide((Double)leftSByte, (Double)((Double)right));
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__mod__")]
        public static object Mod(object left, object right) {
            Debug.Assert(left is SByte);
            SByte leftSByte = (SByte)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            return Int16Ops.ModImpl((Int16)leftSByte, (Int16)((Byte)right));
                        }
                    case TypeCode.SByte: {
                            return SByteOps.ModImpl((SByte)leftSByte, (SByte)((SByte)right));
                        }
                    case TypeCode.Int16: {
                            return Int16Ops.ModImpl((Int16)leftSByte, (Int16)((Int16)right));
                        }
                    case TypeCode.UInt16: {
                            return IntOps.Mod((Int32)leftSByte, (Int32)((UInt16)right));
                        }
                    case TypeCode.Int32: {
                            return IntOps.Mod((Int32)leftSByte, (Int32)((Int32)right));
                        }
                    case TypeCode.UInt32: {
                            return Int64Ops.Mod((Int64)leftSByte, (Int64)((UInt32)right));
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.Mod((Int64)leftSByte, (Int64)((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ModImpl(leftSByte, (UInt64)right);
                        }
                    case TypeCode.Single: {
                            return SingleOps.ModImpl((Single)leftSByte, (Single)((Single)right));
                        }
                    case TypeCode.Double: {
                            return FloatOps.Mod((Double)leftSByte, (Double)((Double)right));
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__mul__")]
        public static object Multiply(object left, object right) {
            Debug.Assert(left is SByte);
            SByte leftSByte = (SByte)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            Int16 result = (Int16)(((Int16)leftSByte) * ((Int16)((Byte)right)));
                            if (SByte.MinValue <= result && result <= SByte.MaxValue) {
                                return (SByte)result;
                            } else return result;
                        }
                    case TypeCode.SByte: {
                            Int16 result = (Int16)(((Int16)leftSByte) * ((Int16)((SByte)right)));
                            if (SByte.MinValue <= result && result <= SByte.MaxValue) {
                                return (SByte)result;
                            } else return result;
                        }
                    case TypeCode.Int16: {
                            Int32 result = (Int32)(((Int32)leftSByte) * ((Int32)((Int16)right)));
                            if (SByte.MinValue <= result && result <= SByte.MaxValue) {
                                return (SByte)result;
                            } else return result;
                        }
                    case TypeCode.UInt16: {
                            Int32 result = (Int32)(((Int32)leftSByte) * ((Int32)((UInt16)right)));
                            if (SByte.MinValue <= result && result <= SByte.MaxValue) {
                                return (SByte)result;
                            } else return result;
                        }
                    case TypeCode.Int32: {
                            Int64 result = (Int64)(((Int64)leftSByte) * ((Int64)((Int32)right)));
                            if (SByte.MinValue <= result && result <= SByte.MaxValue) {
                                return (SByte)result;
                            } else return result;
                        }
                    case TypeCode.UInt32: {
                            Int64 result = (Int64)(((Int64)leftSByte) * ((Int64)((UInt32)right)));
                            if (SByte.MinValue <= result && result <= SByte.MaxValue) {
                                return (SByte)result;
                            } else return result;
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.Multiply(leftSByte, (Int64)right);
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.MultiplyImpl(leftSByte, (UInt64)right);
                        }
                    case TypeCode.Single: {
                            return (Double)(((Double)leftSByte) * ((Double)((Single)right)));
                        }
                    case TypeCode.Double: {
                            return FloatOps.Multiply(leftSByte, (Double)right);
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__sub__")]
        public static object Subtract(object left, object right) {
            Debug.Assert(left is SByte);
            SByte leftSByte = (SByte)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            Int16 result = (Int16)(((Int16)leftSByte) - ((Int16)((Byte)right)));
                            if (SByte.MinValue <= result && result <= SByte.MaxValue) {
                                return (SByte)result;
                            } else return result;
                        }
                    case TypeCode.SByte: {
                            Int16 result = (Int16)(((Int16)leftSByte) - ((Int16)((SByte)right)));
                            if (SByte.MinValue <= result && result <= SByte.MaxValue) {
                                return (SByte)result;
                            } else return result;
                        }
                    case TypeCode.Int16: {
                            Int32 result = (Int32)(((Int32)leftSByte) - ((Int32)((Int16)right)));
                            if (SByte.MinValue <= result && result <= SByte.MaxValue) {
                                return (SByte)result;
                            } else return result;
                        }
                    case TypeCode.UInt16: {
                            Int32 result = (Int32)(((Int32)leftSByte) - ((Int32)((UInt16)right)));
                            if (SByte.MinValue <= result && result <= SByte.MaxValue) {
                                return (SByte)result;
                            } else return result;
                        }
                    case TypeCode.Int32: {
                            Int64 result = (Int64)(((Int64)leftSByte) - ((Int64)((Int32)right)));
                            if (SByte.MinValue <= result && result <= SByte.MaxValue) {
                                return (SByte)result;
                            } else return result;
                        }
                    case TypeCode.UInt32: {
                            Int64 result = (Int64)(((Int64)leftSByte) - ((Int64)((UInt32)right)));
                            if (SByte.MinValue <= result && result <= SByte.MaxValue) {
                                return (SByte)result;
                            } else return result;
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.Subtract(leftSByte, (Int64)right);
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.SubtractImpl(leftSByte, (UInt64)right);
                        }
                    case TypeCode.Single: {
                            return (Single)(((Single)leftSByte) - ((Single)((Single)right)));
                        }
                    case TypeCode.Double: {
                            return (Double)(((Double)leftSByte) - ((Double)((Double)right)));
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__radd__")]
        public static object ReverseAdd(object left, object right) {
            Debug.Assert(left is SByte);
            SByte leftSByte = (SByte)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            Int16 result = (Int16)(((Int16)((Byte)right)) + ((Int16)leftSByte));
                            if (SByte.MinValue <= result && result <= SByte.MaxValue) {
                                return (SByte)result;
                            } else return result;
                        }
                    case TypeCode.SByte: {
                            Int16 result = (Int16)(((Int16)((SByte)right)) + ((Int16)leftSByte));
                            if (SByte.MinValue <= result && result <= SByte.MaxValue) {
                                return (SByte)result;
                            } else return result;
                        }
                    case TypeCode.Int16: {
                            Int32 result = (Int32)(((Int32)((Int16)right)) + ((Int32)leftSByte));
                            if (SByte.MinValue <= result && result <= SByte.MaxValue) {
                                return (SByte)result;
                            } else return result;
                        }
                    case TypeCode.UInt16: {
                            Int32 result = (Int32)(((Int32)((UInt16)right)) + ((Int32)leftSByte));
                            if (SByte.MinValue <= result && result <= SByte.MaxValue) {
                                return (SByte)result;
                            } else return result;
                        }
                    case TypeCode.Int32: {
                            Int64 result = (Int64)(((Int64)((Int32)right)) + ((Int64)leftSByte));
                            if (SByte.MinValue <= result && result <= SByte.MaxValue) {
                                return (SByte)result;
                            } else return result;
                        }
                    case TypeCode.UInt32: {
                            Int64 result = (Int64)(((Int64)((UInt32)right)) + ((Int64)leftSByte));
                            if (SByte.MinValue <= result && result <= SByte.MaxValue) {
                                return (SByte)result;
                            } else return result;
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.ReverseAdd(leftSByte, (Int64)right);
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ReverseAddImpl(leftSByte, (UInt64)right);
                        }
                    case TypeCode.Single: {
                            return (Single)(((Single)((Single)right)) + ((Single)leftSByte));
                        }
                    case TypeCode.Double: {
                            return (Double)(((Double)((Double)right)) + ((Double)leftSByte));
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rdiv__")]
        public static object ReverseDivide(object left, object right) {
            Debug.Assert(left is SByte);
            SByte leftSByte = (SByte)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            return Int16Ops.ReverseDivideImpl((Int16)leftSByte, (Int16)((Byte)right));
                        }
                    case TypeCode.SByte: {
                            return SByteOps.ReverseDivideImpl((SByte)leftSByte, (SByte)((SByte)right));
                        }
                    case TypeCode.Int16: {
                            return Int16Ops.ReverseDivideImpl((Int16)leftSByte, (Int16)((Int16)right));
                        }
                    case TypeCode.UInt16: {
                            return IntOps.ReverseDivide((Int32)leftSByte, (Int32)((UInt16)right));
                        }
                    case TypeCode.Int32: {
                            return IntOps.ReverseDivide((Int32)leftSByte, (Int32)((Int32)right));
                        }
                    case TypeCode.UInt32: {
                            return Int64Ops.ReverseDivide((Int64)leftSByte, (Int64)((UInt32)right));
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.ReverseDivide((Int64)leftSByte, (Int64)((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ReverseDivideImpl(leftSByte, (UInt64)right);
                        }
                    case TypeCode.Single: {
                            return SingleOps.ReverseDivideImpl((Single)leftSByte, (Single)((Single)right));
                        }
                    case TypeCode.Double: {
                            return FloatOps.ReverseDivide((Double)leftSByte, (Double)((Double)right));
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rfloordiv__")]
        public static object ReverseFloorDivide(object left, object right) {
            Debug.Assert(left is SByte);
            SByte leftSByte = (SByte)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            return Int16Ops.ReverseFloorDivideImpl((Int16)leftSByte, (Int16)((Byte)right));
                        }
                    case TypeCode.SByte: {
                            return SByteOps.ReverseFloorDivideImpl((SByte)leftSByte, (SByte)((SByte)right));
                        }
                    case TypeCode.Int16: {
                            return Int16Ops.ReverseFloorDivideImpl((Int16)leftSByte, (Int16)((Int16)right));
                        }
                    case TypeCode.UInt16: {
                            return IntOps.ReverseFloorDivide((Int32)leftSByte, (Int32)((UInt16)right));
                        }
                    case TypeCode.Int32: {
                            return IntOps.ReverseFloorDivide((Int32)leftSByte, (Int32)((Int32)right));
                        }
                    case TypeCode.UInt32: {
                            return Int64Ops.ReverseFloorDivide((Int64)leftSByte, (Int64)((UInt32)right));
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.ReverseFloorDivide((Int64)leftSByte, (Int64)((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ReverseFloorDivideImpl(leftSByte, (UInt64)right);
                        }
                    case TypeCode.Single: {
                            return SingleOps.ReverseFloorDivideImpl((Single)leftSByte, (Single)((Single)right));
                        }
                    case TypeCode.Double: {
                            return FloatOps.ReverseFloorDivide((Double)leftSByte, (Double)((Double)right));
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rmod__")]
        public static object ReverseMod(object left, object right) {
            Debug.Assert(left is SByte);
            SByte leftSByte = (SByte)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            return Int16Ops.ReverseModImpl((Int16)leftSByte, (Int16)((Byte)right));
                        }
                    case TypeCode.SByte: {
                            return SByteOps.ReverseModImpl((SByte)leftSByte, (SByte)((SByte)right));
                        }
                    case TypeCode.Int16: {
                            return Int16Ops.ReverseModImpl((Int16)leftSByte, (Int16)((Int16)right));
                        }
                    case TypeCode.UInt16: {
                            return IntOps.ReverseMod((Int32)leftSByte, (Int32)((UInt16)right));
                        }
                    case TypeCode.Int32: {
                            return IntOps.ReverseMod((Int32)leftSByte, (Int32)((Int32)right));
                        }
                    case TypeCode.UInt32: {
                            return Int64Ops.ReverseMod((Int64)leftSByte, (Int64)((UInt32)right));
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.ReverseMod((Int64)leftSByte, (Int64)((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ReverseModImpl(leftSByte, (UInt64)right);
                        }
                    case TypeCode.Single: {
                            return SingleOps.ReverseModImpl((Single)leftSByte, (Single)((Single)right));
                        }
                    case TypeCode.Double: {
                            return FloatOps.ReverseMod((Double)leftSByte, (Double)((Double)right));
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rmul__")]
        public static object ReverseMultiply(object left, object right) {
            Debug.Assert(left is SByte);
            SByte leftSByte = (SByte)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            Int16 result = (Int16)(((Int16)((Byte)right)) * ((Int16)leftSByte));
                            if (SByte.MinValue <= result && result <= SByte.MaxValue) {
                                return (SByte)result;
                            } else return result;
                        }
                    case TypeCode.SByte: {
                            Int16 result = (Int16)(((Int16)((SByte)right)) * ((Int16)leftSByte));
                            if (SByte.MinValue <= result && result <= SByte.MaxValue) {
                                return (SByte)result;
                            } else return result;
                        }
                    case TypeCode.Int16: {
                            Int32 result = (Int32)(((Int32)((Int16)right)) * ((Int32)leftSByte));
                            if (SByte.MinValue <= result && result <= SByte.MaxValue) {
                                return (SByte)result;
                            } else return result;
                        }
                    case TypeCode.UInt16: {
                            Int32 result = (Int32)(((Int32)((UInt16)right)) * ((Int32)leftSByte));
                            if (SByte.MinValue <= result && result <= SByte.MaxValue) {
                                return (SByte)result;
                            } else return result;
                        }
                    case TypeCode.Int32: {
                            Int64 result = (Int64)(((Int64)((Int32)right)) * ((Int64)leftSByte));
                            if (SByte.MinValue <= result && result <= SByte.MaxValue) {
                                return (SByte)result;
                            } else return result;
                        }
                    case TypeCode.UInt32: {
                            Int64 result = (Int64)(((Int64)((UInt32)right)) * ((Int64)leftSByte));
                            if (SByte.MinValue <= result && result <= SByte.MaxValue) {
                                return (SByte)result;
                            } else return result;
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.ReverseMultiply(leftSByte, (Int64)right);
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ReverseMultiplyImpl(leftSByte, (UInt64)right);
                        }
                    case TypeCode.Single: {
                            return (Double)(((Double)((Single)right)) * ((Double)leftSByte));
                        }
                    case TypeCode.Double: {
                            return FloatOps.ReverseMultiply(leftSByte, (Double)right);
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rsub__")]
        public static object ReverseSubtract(object left, object right) {
            Debug.Assert(left is SByte);
            SByte leftSByte = (SByte)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            Int16 result = (Int16)(((Int16)((Byte)right)) - ((Int16)leftSByte));
                            if (SByte.MinValue <= result && result <= SByte.MaxValue) {
                                return (SByte)result;
                            } else return result;
                        }
                    case TypeCode.SByte: {
                            Int16 result = (Int16)(((Int16)((SByte)right)) - ((Int16)leftSByte));
                            if (SByte.MinValue <= result && result <= SByte.MaxValue) {
                                return (SByte)result;
                            } else return result;
                        }
                    case TypeCode.Int16: {
                            Int32 result = (Int32)(((Int32)((Int16)right)) - ((Int32)leftSByte));
                            if (SByte.MinValue <= result && result <= SByte.MaxValue) {
                                return (SByte)result;
                            } else return result;
                        }
                    case TypeCode.UInt16: {
                            Int32 result = (Int32)(((Int32)((UInt16)right)) - ((Int32)leftSByte));
                            if (SByte.MinValue <= result && result <= SByte.MaxValue) {
                                return (SByte)result;
                            } else return result;
                        }
                    case TypeCode.Int32: {
                            Int64 result = (Int64)(((Int64)((Int32)right)) - ((Int64)leftSByte));
                            if (SByte.MinValue <= result && result <= SByte.MaxValue) {
                                return (SByte)result;
                            } else return result;
                        }
                    case TypeCode.UInt32: {
                            Int64 result = (Int64)(((Int64)((UInt32)right)) - ((Int64)leftSByte));
                            if (SByte.MinValue <= result && result <= SByte.MaxValue) {
                                return (SByte)result;
                            } else return result;
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.ReverseSubtract(leftSByte, (Int64)right);
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ReverseSubtractImpl(leftSByte, (UInt64)right);
                        }
                    case TypeCode.Single: {
                            return (Single)(((Single)((Single)right)) - ((Single)leftSByte));
                        }
                    case TypeCode.Double: {
                            return (Double)(((Double)((Double)right)) - ((Double)leftSByte));
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__and__")]
        public static object BitwiseAnd(object left, object right) {
            Debug.Assert(left is SByte);
            SByte leftSByte = (SByte)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            Int16 leftInt16 = (Int16)leftSByte;
                            Int16 rightInt16 = (Int16)(Byte)right;
                            return leftInt16 & rightInt16;
                        }
                    case TypeCode.SByte: {
                            return leftSByte & (SByte)right;
                        }
                    case TypeCode.Int16: {
                            Int16 leftInt16 = (Int16)leftSByte;
                            return leftInt16 & (Int16)right;
                        }
                    case TypeCode.UInt16: {
                            Int32 leftInt32 = (Int32)leftSByte;
                            Int32 rightInt32 = (Int32)(UInt16)right;
                            return leftInt32 & rightInt32;
                        }
                    case TypeCode.Int32: {
                            Int32 leftInt32 = (Int32)leftSByte;
                            return leftInt32 & (Int32)right;
                        }
                    case TypeCode.UInt32: {
                            Int64 leftInt64 = (Int64)leftSByte;
                            Int64 rightInt64 = (Int64)(UInt32)right;
                            return leftInt64 & rightInt64;
                        }
                    case TypeCode.Int64: {
                            Int64 leftInt64 = (Int64)leftSByte;
                            return leftInt64 & (Int64)right;
                        }
                    case TypeCode.UInt64: {
                            Int64 leftInt64 = (Int64)leftSByte;
                            return leftInt64 & (Int64)(UInt64)right;
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rand__")]
        public static object ReverseBitwiseAnd(object left, object right) {
            Debug.Assert(left is SByte);
            SByte leftSByte = (SByte)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            Int16 leftInt16 = (Int16)leftSByte;
                            Int16 rightInt16 = (Int16)(Byte)right;
                            return leftInt16 & rightInt16;
                        }
                    case TypeCode.SByte: {
                            return leftSByte & (SByte)right;
                        }
                    case TypeCode.Int16: {
                            Int16 leftInt16 = (Int16)leftSByte;
                            return leftInt16 & (Int16)right;
                        }
                    case TypeCode.UInt16: {
                            Int32 leftInt32 = (Int32)leftSByte;
                            Int32 rightInt32 = (Int32)(UInt16)right;
                            return leftInt32 & rightInt32;
                        }
                    case TypeCode.Int32: {
                            Int32 leftInt32 = (Int32)leftSByte;
                            return leftInt32 & (Int32)right;
                        }
                    case TypeCode.UInt32: {
                            Int64 leftInt64 = (Int64)leftSByte;
                            Int64 rightInt64 = (Int64)(UInt32)right;
                            return leftInt64 & rightInt64;
                        }
                    case TypeCode.Int64: {
                            Int64 leftInt64 = (Int64)leftSByte;
                            return leftInt64 & (Int64)right;
                        }
                    case TypeCode.UInt64: {
                            Int64 leftInt64 = (Int64)leftSByte;
                            return leftInt64 & (Int64)(UInt64)right;
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__or__")]
        public static object BitwiseOr(object left, object right) {
            Debug.Assert(left is SByte);
            SByte leftSByte = (SByte)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            Int16 leftInt16 = (Int16)leftSByte;
                            Int16 rightInt16 = (Int16)(Byte)right;
                            return leftInt16 | rightInt16;
                        }
                    case TypeCode.SByte: {
                            return leftSByte | (SByte)right;
                        }
                    case TypeCode.Int16: {
                            Int16 leftInt16 = (Int16)leftSByte;
                            return leftInt16 | (Int16)right;
                        }
                    case TypeCode.UInt16: {
                            Int32 leftInt32 = (Int32)leftSByte;
                            Int32 rightInt32 = (Int32)(UInt16)right;
                            return leftInt32 | rightInt32;
                        }
                    case TypeCode.Int32: {
                            Int32 leftInt32 = (Int32)leftSByte;
                            return leftInt32 | (Int32)right;
                        }
                    case TypeCode.UInt32: {
                            Int64 leftInt64 = (Int64)leftSByte;
                            Int64 rightInt64 = (Int64)(UInt32)right;
                            return leftInt64 | rightInt64;
                        }
                    case TypeCode.Int64: {
                            Int64 leftInt64 = (Int64)leftSByte;
                            return leftInt64 | (Int64)right;
                        }
                    case TypeCode.UInt64: {
                            Int64 leftInt64 = (Int64)leftSByte;
                            return leftInt64 | (Int64)(UInt64)right;
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__ror__")]
        public static object ReverseBitwiseOr(object left, object right) {
            Debug.Assert(left is SByte);
            SByte leftSByte = (SByte)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            Int16 leftInt16 = (Int16)leftSByte;
                            Int16 rightInt16 = (Int16)(Byte)right;
                            return leftInt16 | rightInt16;
                        }
                    case TypeCode.SByte: {
                            return leftSByte | (SByte)right;
                        }
                    case TypeCode.Int16: {
                            Int16 leftInt16 = (Int16)leftSByte;
                            return leftInt16 | (Int16)right;
                        }
                    case TypeCode.UInt16: {
                            Int32 leftInt32 = (Int32)leftSByte;
                            Int32 rightInt32 = (Int32)(UInt16)right;
                            return leftInt32 | rightInt32;
                        }
                    case TypeCode.Int32: {
                            Int32 leftInt32 = (Int32)leftSByte;
                            return leftInt32 | (Int32)right;
                        }
                    case TypeCode.UInt32: {
                            Int64 leftInt64 = (Int64)leftSByte;
                            Int64 rightInt64 = (Int64)(UInt32)right;
                            return leftInt64 | rightInt64;
                        }
                    case TypeCode.Int64: {
                            Int64 leftInt64 = (Int64)leftSByte;
                            return leftInt64 | (Int64)right;
                        }
                    case TypeCode.UInt64: {
                            Int64 leftInt64 = (Int64)leftSByte;
                            return leftInt64 | (Int64)(UInt64)right;
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rxor__")]
        public static object BitwiseXor(object left, object right) {
            Debug.Assert(left is SByte);
            SByte leftSByte = (SByte)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            Int16 leftInt16 = (Int16)leftSByte;
                            Int16 rightInt16 = (Int16)(Byte)right;
                            return leftInt16 ^ rightInt16;
                        }
                    case TypeCode.SByte: {
                            return leftSByte ^ (SByte)right;
                        }
                    case TypeCode.Int16: {
                            Int16 leftInt16 = (Int16)leftSByte;
                            return leftInt16 ^ (Int16)right;
                        }
                    case TypeCode.UInt16: {
                            Int32 leftInt32 = (Int32)leftSByte;
                            Int32 rightInt32 = (Int32)(UInt16)right;
                            return leftInt32 ^ rightInt32;
                        }
                    case TypeCode.Int32: {
                            Int32 leftInt32 = (Int32)leftSByte;
                            return leftInt32 ^ (Int32)right;
                        }
                    case TypeCode.UInt32: {
                            Int64 leftInt64 = (Int64)leftSByte;
                            Int64 rightInt64 = (Int64)(UInt32)right;
                            return leftInt64 ^ rightInt64;
                        }
                    case TypeCode.Int64: {
                            Int64 leftInt64 = (Int64)leftSByte;
                            return leftInt64 ^ (Int64)right;
                        }
                    case TypeCode.UInt64: {
                            Int64 leftInt64 = (Int64)leftSByte;
                            return leftInt64 ^ (Int64)(UInt64)right;
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__xor__")]
        public static object ReverseBitwiseXor(object left, object right) {
            Debug.Assert(left is SByte);
            SByte leftSByte = (SByte)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            Int16 leftInt16 = (Int16)leftSByte;
                            Int16 rightInt16 = (Int16)(Byte)right;
                            return leftInt16 ^ rightInt16;
                        }
                    case TypeCode.SByte: {
                            return leftSByte ^ (SByte)right;
                        }
                    case TypeCode.Int16: {
                            Int16 leftInt16 = (Int16)leftSByte;
                            return leftInt16 ^ (Int16)right;
                        }
                    case TypeCode.UInt16: {
                            Int32 leftInt32 = (Int32)leftSByte;
                            Int32 rightInt32 = (Int32)(UInt16)right;
                            return leftInt32 ^ rightInt32;
                        }
                    case TypeCode.Int32: {
                            Int32 leftInt32 = (Int32)leftSByte;
                            return leftInt32 ^ (Int32)right;
                        }
                    case TypeCode.UInt32: {
                            Int64 leftInt64 = (Int64)leftSByte;
                            Int64 rightInt64 = (Int64)(UInt32)right;
                            return leftInt64 ^ rightInt64;
                        }
                    case TypeCode.Int64: {
                            Int64 leftInt64 = (Int64)leftSByte;
                            return leftInt64 ^ (Int64)right;
                        }
                    case TypeCode.UInt64: {
                            Int64 leftInt64 = (Int64)leftSByte;
                            return leftInt64 ^ (Int64)(UInt64)right;
                        }
                }
            }
            return Ops.NotImplemented;
        }
        internal static object DivideImpl(SByte x, SByte y) {
            // special case (MinValue / -1) doesn't fit
            if (x == SByte.MinValue && y == -1) {
                return (Int16)((Int16)SByte.MaxValue + 1);
            }
            SByte q = (SByte)(x / y);
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

        internal static object ReverseDivideImpl(SByte x, SByte y) {
            return DivideImpl(y, x);
        }

        internal static object ModImpl(SByte x, SByte y) {
            SByte r = (SByte)(x % y);
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

        internal static object ReverseModImpl(SByte x, SByte y) {
            return ModImpl(y, x);
        }

        internal static object FloorDivideImpl(SByte x, SByte y) {
            return DivideImpl(x, y);
        }

        internal static object ReverseFloorDivideImpl(SByte x, SByte y) {
            return DivideImpl(y, x);
        }


        // *** END GENERATED CODE ***

        #endregion
    }
}
