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
    static partial class UInt32Ops {
        #region Generated UInt32Ops

        // *** BEGIN GENERATED CODE ***

        public static DynamicType MakeDynamicType() {
            return new OpsReflectedType("UInt32", typeof(UInt32), typeof(UInt32Ops), null);
        }

        [PythonName("__add__")]
        public static object Add(object left, object right) {
            Debug.Assert(left is UInt32);
            UInt32 leftUInt32 = (UInt32)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            Int64 result = (Int64)(((Int64)leftUInt32) + ((Int64)((Byte)right)));
                            if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                                return (UInt32)result;
                            } else return result;
                        }
                    case TypeCode.SByte: {
                            Int64 result = (Int64)(((Int64)leftUInt32) + ((Int64)((SByte)right)));
                            if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                                return (UInt32)result;
                            } else return result;
                        }
                    case TypeCode.Int16: {
                            Int64 result = (Int64)(((Int64)leftUInt32) + ((Int64)((Int16)right)));
                            if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                                return (UInt32)result;
                            } else return result;
                        }
                    case TypeCode.UInt16: {
                            Int64 result = (Int64)(((Int64)leftUInt32) + ((Int64)((UInt16)right)));
                            if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                                return (UInt32)result;
                            } else return result;
                        }
                    case TypeCode.Int32: {
                            Int64 result = (Int64)(((Int64)leftUInt32) + ((Int64)((Int32)right)));
                            if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                                return (UInt32)result;
                            } else return result;
                        }
                    case TypeCode.UInt32: {
                            Int64 result = (Int64)(((Int64)leftUInt32) + ((Int64)((UInt32)right)));
                            if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                                return (UInt32)result;
                            } else return result;
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.Add(leftUInt32, (Int64)right);
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.AddImpl(leftUInt32, (UInt64)right);
                        }
                    case TypeCode.Single: {
                            return (Single)(((Single)leftUInt32) + ((Single)((Single)right)));
                        }
                    case TypeCode.Double: {
                            return (Double)(((Double)leftUInt32) + ((Double)((Double)right)));
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__div__")]
        public static object Divide(object left, object right) {
            Debug.Assert(left is UInt32);
            UInt32 leftUInt32 = (UInt32)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            return UInt32Ops.DivideImpl((UInt32)leftUInt32, (UInt32)((Byte)right));
                        }
                    case TypeCode.SByte: {
                            return Int64Ops.Divide((Int64)leftUInt32, (Int64)((SByte)right));
                        }
                    case TypeCode.Int16: {
                            return Int64Ops.Divide((Int64)leftUInt32, (Int64)((Int16)right));
                        }
                    case TypeCode.UInt16: {
                            return UInt32Ops.DivideImpl((UInt32)leftUInt32, (UInt32)((UInt16)right));
                        }
                    case TypeCode.Int32: {
                            return Int64Ops.Divide((Int64)leftUInt32, (Int64)((Int32)right));
                        }
                    case TypeCode.UInt32: {
                            return UInt32Ops.DivideImpl((UInt32)leftUInt32, (UInt32)((UInt32)right));
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.Divide((Int64)leftUInt32, (Int64)((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.DivideImpl((UInt64)leftUInt32, (UInt64)((UInt64)right));
                        }
                    case TypeCode.Single: {
                            return SingleOps.DivideImpl((Single)leftUInt32, (Single)((Single)right));
                        }
                    case TypeCode.Double: {
                            return FloatOps.Divide((Double)leftUInt32, (Double)((Double)right));
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__floordiv__")]
        public static object FloorDivide(object left, object right) {
            Debug.Assert(left is UInt32);
            UInt32 leftUInt32 = (UInt32)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            return UInt32Ops.FloorDivideImpl((UInt32)leftUInt32, (UInt32)((Byte)right));
                        }
                    case TypeCode.SByte: {
                            return Int64Ops.FloorDivide((Int64)leftUInt32, (Int64)((SByte)right));
                        }
                    case TypeCode.Int16: {
                            return Int64Ops.FloorDivide((Int64)leftUInt32, (Int64)((Int16)right));
                        }
                    case TypeCode.UInt16: {
                            return UInt32Ops.FloorDivideImpl((UInt32)leftUInt32, (UInt32)((UInt16)right));
                        }
                    case TypeCode.Int32: {
                            return Int64Ops.FloorDivide((Int64)leftUInt32, (Int64)((Int32)right));
                        }
                    case TypeCode.UInt32: {
                            return UInt32Ops.FloorDivideImpl((UInt32)leftUInt32, (UInt32)((UInt32)right));
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.FloorDivide((Int64)leftUInt32, (Int64)((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.FloorDivideImpl((UInt64)leftUInt32, (UInt64)((UInt64)right));
                        }
                    case TypeCode.Single: {
                            return SingleOps.FloorDivideImpl((Single)leftUInt32, (Single)((Single)right));
                        }
                    case TypeCode.Double: {
                            return FloatOps.FloorDivide((Double)leftUInt32, (Double)((Double)right));
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__mod__")]
        public static object Mod(object left, object right) {
            Debug.Assert(left is UInt32);
            UInt32 leftUInt32 = (UInt32)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            return UInt32Ops.ModImpl((UInt32)leftUInt32, (UInt32)((Byte)right));
                        }
                    case TypeCode.SByte: {
                            return Int64Ops.Mod((Int64)leftUInt32, (Int64)((SByte)right));
                        }
                    case TypeCode.Int16: {
                            return Int64Ops.Mod((Int64)leftUInt32, (Int64)((Int16)right));
                        }
                    case TypeCode.UInt16: {
                            return UInt32Ops.ModImpl((UInt32)leftUInt32, (UInt32)((UInt16)right));
                        }
                    case TypeCode.Int32: {
                            return Int64Ops.Mod((Int64)leftUInt32, (Int64)((Int32)right));
                        }
                    case TypeCode.UInt32: {
                            return UInt32Ops.ModImpl((UInt32)leftUInt32, (UInt32)((UInt32)right));
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.Mod((Int64)leftUInt32, (Int64)((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ModImpl((UInt64)leftUInt32, (UInt64)((UInt64)right));
                        }
                    case TypeCode.Single: {
                            return SingleOps.ModImpl((Single)leftUInt32, (Single)((Single)right));
                        }
                    case TypeCode.Double: {
                            return FloatOps.Mod((Double)leftUInt32, (Double)((Double)right));
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__mul__")]
        public static object Multiply(object left, object right) {
            Debug.Assert(left is UInt32);
            UInt32 leftUInt32 = (UInt32)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            Int64 result = (Int64)(((Int64)leftUInt32) * ((Int64)((Byte)right)));
                            if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                                return (UInt32)result;
                            } else return result;
                        }
                    case TypeCode.SByte: {
                            Int64 result = (Int64)(((Int64)leftUInt32) * ((Int64)((SByte)right)));
                            if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                                return (UInt32)result;
                            } else return result;
                        }
                    case TypeCode.Int16: {
                            Int64 result = (Int64)(((Int64)leftUInt32) * ((Int64)((Int16)right)));
                            if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                                return (UInt32)result;
                            } else return result;
                        }
                    case TypeCode.UInt16: {
                            Int64 result = (Int64)(((Int64)leftUInt32) * ((Int64)((UInt16)right)));
                            if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                                return (UInt32)result;
                            } else return result;
                        }
                    case TypeCode.Int32: {
                            Int64 result = (Int64)(((Int64)leftUInt32) * ((Int64)((Int32)right)));
                            if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                                return (UInt32)result;
                            } else return result;
                        }
                    case TypeCode.UInt32: {
                            UInt64 result = (UInt64)(((UInt64)leftUInt32) * ((UInt64)((UInt32)right)));
                            if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                                return (UInt32)result;
                            } else return result;
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.Multiply(leftUInt32, (Int64)right);
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.MultiplyImpl(leftUInt32, (UInt64)right);
                        }
                    case TypeCode.Single: {
                            return (Double)(((Double)leftUInt32) * ((Double)((Single)right)));
                        }
                    case TypeCode.Double: {
                            return FloatOps.Multiply(leftUInt32, (Double)right);
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__sub__")]
        public static object Subtract(object left, object right) {
            Debug.Assert(left is UInt32);
            UInt32 leftUInt32 = (UInt32)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            Int64 result = (Int64)(((Int64)leftUInt32) - ((Int64)((Byte)right)));
                            if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                                return (UInt32)result;
                            } else return result;
                        }
                    case TypeCode.SByte: {
                            Int64 result = (Int64)(((Int64)leftUInt32) - ((Int64)((SByte)right)));
                            if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                                return (UInt32)result;
                            } else return result;
                        }
                    case TypeCode.Int16: {
                            Int64 result = (Int64)(((Int64)leftUInt32) - ((Int64)((Int16)right)));
                            if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                                return (UInt32)result;
                            } else return result;
                        }
                    case TypeCode.UInt16: {
                            Int64 result = (Int64)(((Int64)leftUInt32) - ((Int64)((UInt16)right)));
                            if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                                return (UInt32)result;
                            } else return result;
                        }
                    case TypeCode.Int32: {
                            Int64 result = (Int64)(((Int64)leftUInt32) - ((Int64)((Int32)right)));
                            if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                                return (UInt32)result;
                            } else return result;
                        }
                    case TypeCode.UInt32: {
                            Int64 result = (Int64)(((Int64)leftUInt32) - ((Int64)((UInt32)right)));
                            if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                                return (UInt32)result;
                            } else return result;
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.Subtract(leftUInt32, (Int64)right);
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.SubtractImpl(leftUInt32, (UInt64)right);
                        }
                    case TypeCode.Single: {
                            return (Single)(((Single)leftUInt32) - ((Single)((Single)right)));
                        }
                    case TypeCode.Double: {
                            return (Double)(((Double)leftUInt32) - ((Double)((Double)right)));
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__radd__")]
        public static object ReverseAdd(object left, object right) {
            Debug.Assert(left is UInt32);
            UInt32 leftUInt32 = (UInt32)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            Int64 result = (Int64)(((Int64)((Byte)right)) + ((Int64)leftUInt32));
                            if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                                return (UInt32)result;
                            } else return result;
                        }
                    case TypeCode.SByte: {
                            Int64 result = (Int64)(((Int64)((SByte)right)) + ((Int64)leftUInt32));
                            if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                                return (UInt32)result;
                            } else return result;
                        }
                    case TypeCode.Int16: {
                            Int64 result = (Int64)(((Int64)((Int16)right)) + ((Int64)leftUInt32));
                            if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                                return (UInt32)result;
                            } else return result;
                        }
                    case TypeCode.UInt16: {
                            Int64 result = (Int64)(((Int64)((UInt16)right)) + ((Int64)leftUInt32));
                            if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                                return (UInt32)result;
                            } else return result;
                        }
                    case TypeCode.Int32: {
                            Int64 result = (Int64)(((Int64)((Int32)right)) + ((Int64)leftUInt32));
                            if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                                return (UInt32)result;
                            } else return result;
                        }
                    case TypeCode.UInt32: {
                            Int64 result = (Int64)(((Int64)((UInt32)right)) + ((Int64)leftUInt32));
                            if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                                return (UInt32)result;
                            } else return result;
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.ReverseAdd(leftUInt32, (Int64)right);
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ReverseAddImpl(leftUInt32, (UInt64)right);
                        }
                    case TypeCode.Single: {
                            return (Single)(((Single)((Single)right)) + ((Single)leftUInt32));
                        }
                    case TypeCode.Double: {
                            return (Double)(((Double)((Double)right)) + ((Double)leftUInt32));
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rdiv__")]
        public static object ReverseDivide(object left, object right) {
            Debug.Assert(left is UInt32);
            UInt32 leftUInt32 = (UInt32)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            return UInt32Ops.ReverseDivideImpl((UInt32)leftUInt32, (UInt32)((Byte)right));
                        }
                    case TypeCode.SByte: {
                            return Int64Ops.ReverseDivide((Int64)leftUInt32, (Int64)((SByte)right));
                        }
                    case TypeCode.Int16: {
                            return Int64Ops.ReverseDivide((Int64)leftUInt32, (Int64)((Int16)right));
                        }
                    case TypeCode.UInt16: {
                            return UInt32Ops.ReverseDivideImpl((UInt32)leftUInt32, (UInt32)((UInt16)right));
                        }
                    case TypeCode.Int32: {
                            return Int64Ops.ReverseDivide((Int64)leftUInt32, (Int64)((Int32)right));
                        }
                    case TypeCode.UInt32: {
                            return UInt32Ops.ReverseDivideImpl((UInt32)leftUInt32, (UInt32)((UInt32)right));
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.ReverseDivide((Int64)leftUInt32, (Int64)((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ReverseDivideImpl((UInt64)leftUInt32, (UInt64)((UInt64)right));
                        }
                    case TypeCode.Single: {
                            return SingleOps.ReverseDivideImpl((Single)leftUInt32, (Single)((Single)right));
                        }
                    case TypeCode.Double: {
                            return FloatOps.ReverseDivide((Double)leftUInt32, (Double)((Double)right));
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rfloordiv__")]
        public static object ReverseFloorDivide(object left, object right) {
            Debug.Assert(left is UInt32);
            UInt32 leftUInt32 = (UInt32)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            return UInt32Ops.ReverseFloorDivideImpl((UInt32)leftUInt32, (UInt32)((Byte)right));
                        }
                    case TypeCode.SByte: {
                            return Int64Ops.ReverseFloorDivide((Int64)leftUInt32, (Int64)((SByte)right));
                        }
                    case TypeCode.Int16: {
                            return Int64Ops.ReverseFloorDivide((Int64)leftUInt32, (Int64)((Int16)right));
                        }
                    case TypeCode.UInt16: {
                            return UInt32Ops.ReverseFloorDivideImpl((UInt32)leftUInt32, (UInt32)((UInt16)right));
                        }
                    case TypeCode.Int32: {
                            return Int64Ops.ReverseFloorDivide((Int64)leftUInt32, (Int64)((Int32)right));
                        }
                    case TypeCode.UInt32: {
                            return UInt32Ops.ReverseFloorDivideImpl((UInt32)leftUInt32, (UInt32)((UInt32)right));
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.ReverseFloorDivide((Int64)leftUInt32, (Int64)((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ReverseFloorDivideImpl((UInt64)leftUInt32, (UInt64)((UInt64)right));
                        }
                    case TypeCode.Single: {
                            return SingleOps.ReverseFloorDivideImpl((Single)leftUInt32, (Single)((Single)right));
                        }
                    case TypeCode.Double: {
                            return FloatOps.ReverseFloorDivide((Double)leftUInt32, (Double)((Double)right));
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rmod__")]
        public static object ReverseMod(object left, object right) {
            Debug.Assert(left is UInt32);
            UInt32 leftUInt32 = (UInt32)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            return UInt32Ops.ReverseModImpl((UInt32)leftUInt32, (UInt32)((Byte)right));
                        }
                    case TypeCode.SByte: {
                            return Int64Ops.ReverseMod((Int64)leftUInt32, (Int64)((SByte)right));
                        }
                    case TypeCode.Int16: {
                            return Int64Ops.ReverseMod((Int64)leftUInt32, (Int64)((Int16)right));
                        }
                    case TypeCode.UInt16: {
                            return UInt32Ops.ReverseModImpl((UInt32)leftUInt32, (UInt32)((UInt16)right));
                        }
                    case TypeCode.Int32: {
                            return Int64Ops.ReverseMod((Int64)leftUInt32, (Int64)((Int32)right));
                        }
                    case TypeCode.UInt32: {
                            return UInt32Ops.ReverseModImpl((UInt32)leftUInt32, (UInt32)((UInt32)right));
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.ReverseMod((Int64)leftUInt32, (Int64)((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ReverseModImpl((UInt64)leftUInt32, (UInt64)((UInt64)right));
                        }
                    case TypeCode.Single: {
                            return SingleOps.ReverseModImpl((Single)leftUInt32, (Single)((Single)right));
                        }
                    case TypeCode.Double: {
                            return FloatOps.ReverseMod((Double)leftUInt32, (Double)((Double)right));
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rmul__")]
        public static object ReverseMultiply(object left, object right) {
            Debug.Assert(left is UInt32);
            UInt32 leftUInt32 = (UInt32)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            Int64 result = (Int64)(((Int64)((Byte)right)) * ((Int64)leftUInt32));
                            if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                                return (UInt32)result;
                            } else return result;
                        }
                    case TypeCode.SByte: {
                            Int64 result = (Int64)(((Int64)((SByte)right)) * ((Int64)leftUInt32));
                            if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                                return (UInt32)result;
                            } else return result;
                        }
                    case TypeCode.Int16: {
                            Int64 result = (Int64)(((Int64)((Int16)right)) * ((Int64)leftUInt32));
                            if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                                return (UInt32)result;
                            } else return result;
                        }
                    case TypeCode.UInt16: {
                            Int64 result = (Int64)(((Int64)((UInt16)right)) * ((Int64)leftUInt32));
                            if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                                return (UInt32)result;
                            } else return result;
                        }
                    case TypeCode.Int32: {
                            Int64 result = (Int64)(((Int64)((Int32)right)) * ((Int64)leftUInt32));
                            if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                                return (UInt32)result;
                            } else return result;
                        }
                    case TypeCode.UInt32: {
                            UInt64 result = (UInt64)(((UInt64)((UInt32)right)) * ((UInt64)leftUInt32));
                            if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                                return (UInt32)result;
                            } else return result;
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.ReverseMultiply(leftUInt32, (Int64)right);
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ReverseMultiplyImpl(leftUInt32, (UInt64)right);
                        }
                    case TypeCode.Single: {
                            return (Double)(((Double)((Single)right)) * ((Double)leftUInt32));
                        }
                    case TypeCode.Double: {
                            return FloatOps.ReverseMultiply(leftUInt32, (Double)right);
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rsub__")]
        public static object ReverseSubtract(object left, object right) {
            Debug.Assert(left is UInt32);
            UInt32 leftUInt32 = (UInt32)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            Int64 result = (Int64)(((Int64)((Byte)right)) - ((Int64)leftUInt32));
                            if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                                return (UInt32)result;
                            } else return result;
                        }
                    case TypeCode.SByte: {
                            Int64 result = (Int64)(((Int64)((SByte)right)) - ((Int64)leftUInt32));
                            if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                                return (UInt32)result;
                            } else return result;
                        }
                    case TypeCode.Int16: {
                            Int64 result = (Int64)(((Int64)((Int16)right)) - ((Int64)leftUInt32));
                            if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                                return (UInt32)result;
                            } else return result;
                        }
                    case TypeCode.UInt16: {
                            Int64 result = (Int64)(((Int64)((UInt16)right)) - ((Int64)leftUInt32));
                            if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                                return (UInt32)result;
                            } else return result;
                        }
                    case TypeCode.Int32: {
                            Int64 result = (Int64)(((Int64)((Int32)right)) - ((Int64)leftUInt32));
                            if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                                return (UInt32)result;
                            } else return result;
                        }
                    case TypeCode.UInt32: {
                            Int64 result = (Int64)(((Int64)((UInt32)right)) - ((Int64)leftUInt32));
                            if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                                return (UInt32)result;
                            } else return result;
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.ReverseSubtract(leftUInt32, (Int64)right);
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ReverseSubtractImpl(leftUInt32, (UInt64)right);
                        }
                    case TypeCode.Single: {
                            return (Single)(((Single)((Single)right)) - ((Single)leftUInt32));
                        }
                    case TypeCode.Double: {
                            return (Double)(((Double)((Double)right)) - ((Double)leftUInt32));
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__and__")]
        public static object BitwiseAnd(object left, object right) {
            Debug.Assert(left is UInt32);
            UInt32 leftUInt32 = (UInt32)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            UInt32 rightUInt32 = (UInt32)(Byte)right;
                            return leftUInt32 & rightUInt32;
                        }
                    case TypeCode.SByte: {
                            Int64 leftInt64 = (Int64)leftUInt32;
                            Int64 rightInt64 = (Int64)(SByte)right;
                            return leftInt64 & rightInt64;
                        }
                    case TypeCode.Int16: {
                            Int64 leftInt64 = (Int64)leftUInt32;
                            Int64 rightInt64 = (Int64)(Int16)right;
                            return leftInt64 & rightInt64;
                        }
                    case TypeCode.UInt16: {
                            UInt32 rightUInt32 = (UInt32)(UInt16)right;
                            return leftUInt32 & rightUInt32;
                        }
                    case TypeCode.Int32: {
                            Int64 leftInt64 = (Int64)leftUInt32;
                            Int64 rightInt64 = (Int64)(Int32)right;
                            return leftInt64 & rightInt64;
                        }
                    case TypeCode.UInt32: {
                            return leftUInt32 & (UInt32)right;
                        }
                    case TypeCode.Int64: {
                            Int64 leftInt64 = (Int64)leftUInt32;
                            return leftInt64 & (Int64)right;
                        }
                    case TypeCode.UInt64: {
                            UInt64 leftUInt64 = (UInt64)leftUInt32;
                            return leftUInt64 & (UInt64)right;
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rand__")]
        public static object ReverseBitwiseAnd(object left, object right) {
            Debug.Assert(left is UInt32);
            UInt32 leftUInt32 = (UInt32)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            UInt32 rightUInt32 = (UInt32)(Byte)right;
                            return leftUInt32 & rightUInt32;
                        }
                    case TypeCode.SByte: {
                            Int64 leftInt64 = (Int64)leftUInt32;
                            Int64 rightInt64 = (Int64)(SByte)right;
                            return leftInt64 & rightInt64;
                        }
                    case TypeCode.Int16: {
                            Int64 leftInt64 = (Int64)leftUInt32;
                            Int64 rightInt64 = (Int64)(Int16)right;
                            return leftInt64 & rightInt64;
                        }
                    case TypeCode.UInt16: {
                            UInt32 rightUInt32 = (UInt32)(UInt16)right;
                            return leftUInt32 & rightUInt32;
                        }
                    case TypeCode.Int32: {
                            Int64 leftInt64 = (Int64)leftUInt32;
                            Int64 rightInt64 = (Int64)(Int32)right;
                            return leftInt64 & rightInt64;
                        }
                    case TypeCode.UInt32: {
                            return leftUInt32 & (UInt32)right;
                        }
                    case TypeCode.Int64: {
                            Int64 leftInt64 = (Int64)leftUInt32;
                            return leftInt64 & (Int64)right;
                        }
                    case TypeCode.UInt64: {
                            UInt64 leftUInt64 = (UInt64)leftUInt32;
                            return leftUInt64 & (UInt64)right;
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__or__")]
        public static object BitwiseOr(object left, object right) {
            Debug.Assert(left is UInt32);
            UInt32 leftUInt32 = (UInt32)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            UInt32 rightUInt32 = (UInt32)(Byte)right;
                            return leftUInt32 | rightUInt32;
                        }
                    case TypeCode.SByte: {
                            Int64 leftInt64 = (Int64)leftUInt32;
                            Int64 rightInt64 = (Int64)(SByte)right;
                            return leftInt64 | rightInt64;
                        }
                    case TypeCode.Int16: {
                            Int64 leftInt64 = (Int64)leftUInt32;
                            Int64 rightInt64 = (Int64)(Int16)right;
                            return leftInt64 | rightInt64;
                        }
                    case TypeCode.UInt16: {
                            UInt32 rightUInt32 = (UInt32)(UInt16)right;
                            return leftUInt32 | rightUInt32;
                        }
                    case TypeCode.Int32: {
                            Int64 leftInt64 = (Int64)leftUInt32;
                            Int64 rightInt64 = (Int64)(Int32)right;
                            return leftInt64 | rightInt64;
                        }
                    case TypeCode.UInt32: {
                            return leftUInt32 | (UInt32)right;
                        }
                    case TypeCode.Int64: {
                            Int64 leftInt64 = (Int64)leftUInt32;
                            return leftInt64 | (Int64)right;
                        }
                    case TypeCode.UInt64: {
                            UInt64 leftUInt64 = (UInt64)leftUInt32;
                            return leftUInt64 | (UInt64)right;
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__ror__")]
        public static object ReverseBitwiseOr(object left, object right) {
            Debug.Assert(left is UInt32);
            UInt32 leftUInt32 = (UInt32)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            UInt32 rightUInt32 = (UInt32)(Byte)right;
                            return leftUInt32 | rightUInt32;
                        }
                    case TypeCode.SByte: {
                            Int64 leftInt64 = (Int64)leftUInt32;
                            Int64 rightInt64 = (Int64)(SByte)right;
                            return leftInt64 | rightInt64;
                        }
                    case TypeCode.Int16: {
                            Int64 leftInt64 = (Int64)leftUInt32;
                            Int64 rightInt64 = (Int64)(Int16)right;
                            return leftInt64 | rightInt64;
                        }
                    case TypeCode.UInt16: {
                            UInt32 rightUInt32 = (UInt32)(UInt16)right;
                            return leftUInt32 | rightUInt32;
                        }
                    case TypeCode.Int32: {
                            Int64 leftInt64 = (Int64)leftUInt32;
                            Int64 rightInt64 = (Int64)(Int32)right;
                            return leftInt64 | rightInt64;
                        }
                    case TypeCode.UInt32: {
                            return leftUInt32 | (UInt32)right;
                        }
                    case TypeCode.Int64: {
                            Int64 leftInt64 = (Int64)leftUInt32;
                            return leftInt64 | (Int64)right;
                        }
                    case TypeCode.UInt64: {
                            UInt64 leftUInt64 = (UInt64)leftUInt32;
                            return leftUInt64 | (UInt64)right;
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rxor__")]
        public static object BitwiseXor(object left, object right) {
            Debug.Assert(left is UInt32);
            UInt32 leftUInt32 = (UInt32)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            UInt32 rightUInt32 = (UInt32)(Byte)right;
                            return leftUInt32 ^ rightUInt32;
                        }
                    case TypeCode.SByte: {
                            Int64 leftInt64 = (Int64)leftUInt32;
                            Int64 rightInt64 = (Int64)(SByte)right;
                            return leftInt64 ^ rightInt64;
                        }
                    case TypeCode.Int16: {
                            Int64 leftInt64 = (Int64)leftUInt32;
                            Int64 rightInt64 = (Int64)(Int16)right;
                            return leftInt64 ^ rightInt64;
                        }
                    case TypeCode.UInt16: {
                            UInt32 rightUInt32 = (UInt32)(UInt16)right;
                            return leftUInt32 ^ rightUInt32;
                        }
                    case TypeCode.Int32: {
                            Int64 leftInt64 = (Int64)leftUInt32;
                            Int64 rightInt64 = (Int64)(Int32)right;
                            return leftInt64 ^ rightInt64;
                        }
                    case TypeCode.UInt32: {
                            return leftUInt32 ^ (UInt32)right;
                        }
                    case TypeCode.Int64: {
                            Int64 leftInt64 = (Int64)leftUInt32;
                            return leftInt64 ^ (Int64)right;
                        }
                    case TypeCode.UInt64: {
                            UInt64 leftUInt64 = (UInt64)leftUInt32;
                            return leftUInt64 ^ (UInt64)right;
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__xor__")]
        public static object ReverseBitwiseXor(object left, object right) {
            Debug.Assert(left is UInt32);
            UInt32 leftUInt32 = (UInt32)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            UInt32 rightUInt32 = (UInt32)(Byte)right;
                            return leftUInt32 ^ rightUInt32;
                        }
                    case TypeCode.SByte: {
                            Int64 leftInt64 = (Int64)leftUInt32;
                            Int64 rightInt64 = (Int64)(SByte)right;
                            return leftInt64 ^ rightInt64;
                        }
                    case TypeCode.Int16: {
                            Int64 leftInt64 = (Int64)leftUInt32;
                            Int64 rightInt64 = (Int64)(Int16)right;
                            return leftInt64 ^ rightInt64;
                        }
                    case TypeCode.UInt16: {
                            UInt32 rightUInt32 = (UInt32)(UInt16)right;
                            return leftUInt32 ^ rightUInt32;
                        }
                    case TypeCode.Int32: {
                            Int64 leftInt64 = (Int64)leftUInt32;
                            Int64 rightInt64 = (Int64)(Int32)right;
                            return leftInt64 ^ rightInt64;
                        }
                    case TypeCode.UInt32: {
                            return leftUInt32 ^ (UInt32)right;
                        }
                    case TypeCode.Int64: {
                            Int64 leftInt64 = (Int64)leftUInt32;
                            return leftInt64 ^ (Int64)right;
                        }
                    case TypeCode.UInt64: {
                            UInt64 leftUInt64 = (UInt64)leftUInt32;
                            return leftUInt64 ^ (UInt64)right;
                        }
                }
            }
            return Ops.NotImplemented;
        }
        internal static object DivideImpl(UInt32 x, UInt32 y) {
            UInt32 q = (UInt32)(x / y);
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

        internal static object ReverseDivideImpl(UInt32 x, UInt32 y) {
            return DivideImpl(y, x);
        }

        internal static object ModImpl(UInt32 x, UInt32 y) {
            UInt32 r = (UInt32)(x % y);
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

        internal static object ReverseModImpl(UInt32 x, UInt32 y) {
            return ModImpl(y, x);
        }

        internal static object FloorDivideImpl(UInt32 x, UInt32 y) {
            return DivideImpl(x, y);
        }

        internal static object ReverseFloorDivideImpl(UInt32 x, UInt32 y) {
            return DivideImpl(y, x);
        }


        // *** END GENERATED CODE ***

        #endregion
    }
}
