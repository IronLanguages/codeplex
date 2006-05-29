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
using System.Text;

namespace IronPython.Runtime {
    static partial class Int16Ops {
        #region Generated Int16Ops

        // *** BEGIN GENERATED CODE ***

        public static DynamicType MakeDynamicType() {
            return new OpsReflectedType("Int16", typeof(Int16), typeof(Int16Ops), null);
        }

        [PythonName("__add__")]
        public static object Add(object left, object right) {
            Debug.Assert(left is Int16);
            Int16 leftInt16 = (Int16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            Int32 result = (Int32)(((Int32)leftInt16) + ((Int32)((Byte)right)));
                            if (Int16.MinValue <= result && result <= Int16.MaxValue) {
                                return (Int16)result;
                            } else return result;
                        }
                    case TypeCode.SByte: {
                            Int32 result = (Int32)(((Int32)leftInt16) + ((Int32)((SByte)right)));
                            if (Int16.MinValue <= result && result <= Int16.MaxValue) {
                                return (Int16)result;
                            } else return result;
                        }
                    case TypeCode.Int16: {
                            Int32 result = (Int32)(((Int32)leftInt16) + ((Int32)((Int16)right)));
                            if (Int16.MinValue <= result && result <= Int16.MaxValue) {
                                return (Int16)result;
                            } else return result;
                        }
                    case TypeCode.UInt16: {
                            Int32 result = (Int32)(((Int32)leftInt16) + ((Int32)((UInt16)right)));
                            if (Int16.MinValue <= result && result <= Int16.MaxValue) {
                                return (Int16)result;
                            } else return result;
                        }
                    case TypeCode.Int32: {
                            Int64 result = (Int64)(((Int64)leftInt16) + ((Int64)((Int32)right)));
                            if (Int16.MinValue <= result && result <= Int16.MaxValue) {
                                return (Int16)result;
                            } else return result;
                        }
                    case TypeCode.UInt32: {
                            Int64 result = (Int64)(((Int64)leftInt16) + ((Int64)((UInt32)right)));
                            if (Int16.MinValue <= result && result <= Int16.MaxValue) {
                                return (Int16)result;
                            } else return result;
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.Add(leftInt16, (Int64)right);
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.AddImpl(leftInt16, (UInt64)right);
                        }
                    case TypeCode.Single: {
                            return (Single)(((Single)leftInt16) + ((Single)((Single)right)));
                        }
                    case TypeCode.Double: {
                            return (Double)(((Double)leftInt16) + ((Double)((Double)right)));
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__div__")]
        public static object Divide(object left, object right) {
            Debug.Assert(left is Int16);
            Int16 leftInt16 = (Int16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            return Int16Ops.DivideImpl((Int16)leftInt16, (Int16)((Byte)right));
                        }
                    case TypeCode.SByte: {
                            return Int16Ops.DivideImpl((Int16)leftInt16, (Int16)((SByte)right));
                        }
                    case TypeCode.Int16: {
                            return Int16Ops.DivideImpl((Int16)leftInt16, (Int16)((Int16)right));
                        }
                    case TypeCode.UInt16: {
                            return IntOps.Divide((Int32)leftInt16, (Int32)((UInt16)right));
                        }
                    case TypeCode.Int32: {
                            return IntOps.Divide((Int32)leftInt16, (Int32)((Int32)right));
                        }
                    case TypeCode.UInt32: {
                            return Int64Ops.Divide((Int64)leftInt16, (Int64)((UInt32)right));
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.Divide((Int64)leftInt16, (Int64)((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.DivideImpl(leftInt16, (UInt64)right);
                        }
                    case TypeCode.Single: {
                            return SingleOps.DivideImpl((Single)leftInt16, (Single)((Single)right));
                        }
                    case TypeCode.Double: {
                            return FloatOps.Divide((Double)leftInt16, (Double)((Double)right));
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__floordiv__")]
        public static object FloorDivide(object left, object right) {
            Debug.Assert(left is Int16);
            Int16 leftInt16 = (Int16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            return Int16Ops.FloorDivideImpl((Int16)leftInt16, (Int16)((Byte)right));
                        }
                    case TypeCode.SByte: {
                            return Int16Ops.FloorDivideImpl((Int16)leftInt16, (Int16)((SByte)right));
                        }
                    case TypeCode.Int16: {
                            return Int16Ops.FloorDivideImpl((Int16)leftInt16, (Int16)((Int16)right));
                        }
                    case TypeCode.UInt16: {
                            return IntOps.FloorDivide((Int32)leftInt16, (Int32)((UInt16)right));
                        }
                    case TypeCode.Int32: {
                            return IntOps.FloorDivide((Int32)leftInt16, (Int32)((Int32)right));
                        }
                    case TypeCode.UInt32: {
                            return Int64Ops.FloorDivide((Int64)leftInt16, (Int64)((UInt32)right));
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.FloorDivide((Int64)leftInt16, (Int64)((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.FloorDivideImpl(leftInt16, (UInt64)right);
                        }
                    case TypeCode.Single: {
                            return SingleOps.FloorDivideImpl((Single)leftInt16, (Single)((Single)right));
                        }
                    case TypeCode.Double: {
                            return FloatOps.FloorDivide((Double)leftInt16, (Double)((Double)right));
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__mod__")]
        public static object Mod(object left, object right) {
            Debug.Assert(left is Int16);
            Int16 leftInt16 = (Int16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            return Int16Ops.ModImpl((Int16)leftInt16, (Int16)((Byte)right));
                        }
                    case TypeCode.SByte: {
                            return Int16Ops.ModImpl((Int16)leftInt16, (Int16)((SByte)right));
                        }
                    case TypeCode.Int16: {
                            return Int16Ops.ModImpl((Int16)leftInt16, (Int16)((Int16)right));
                        }
                    case TypeCode.UInt16: {
                            return IntOps.Mod((Int32)leftInt16, (Int32)((UInt16)right));
                        }
                    case TypeCode.Int32: {
                            return IntOps.Mod((Int32)leftInt16, (Int32)((Int32)right));
                        }
                    case TypeCode.UInt32: {
                            return Int64Ops.Mod((Int64)leftInt16, (Int64)((UInt32)right));
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.Mod((Int64)leftInt16, (Int64)((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ModImpl(leftInt16, (UInt64)right);
                        }
                    case TypeCode.Single: {
                            return SingleOps.ModImpl((Single)leftInt16, (Single)((Single)right));
                        }
                    case TypeCode.Double: {
                            return FloatOps.Mod((Double)leftInt16, (Double)((Double)right));
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__mul__")]
        public static object Multiply(object left, object right) {
            Debug.Assert(left is Int16);
            Int16 leftInt16 = (Int16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            Int32 result = (Int32)(((Int32)leftInt16) * ((Int32)((Byte)right)));
                            if (Int16.MinValue <= result && result <= Int16.MaxValue) {
                                return (Int16)result;
                            } else return result;
                        }
                    case TypeCode.SByte: {
                            Int32 result = (Int32)(((Int32)leftInt16) * ((Int32)((SByte)right)));
                            if (Int16.MinValue <= result && result <= Int16.MaxValue) {
                                return (Int16)result;
                            } else return result;
                        }
                    case TypeCode.Int16: {
                            Int32 result = (Int32)(((Int32)leftInt16) * ((Int32)((Int16)right)));
                            if (Int16.MinValue <= result && result <= Int16.MaxValue) {
                                return (Int16)result;
                            } else return result;
                        }
                    case TypeCode.UInt16: {
                            Int32 result = (Int32)(((Int32)leftInt16) * ((Int32)((UInt16)right)));
                            if (Int16.MinValue <= result && result <= Int16.MaxValue) {
                                return (Int16)result;
                            } else return result;
                        }
                    case TypeCode.Int32: {
                            Int64 result = (Int64)(((Int64)leftInt16) * ((Int64)((Int32)right)));
                            if (Int16.MinValue <= result && result <= Int16.MaxValue) {
                                return (Int16)result;
                            } else return result;
                        }
                    case TypeCode.UInt32: {
                            Int64 result = (Int64)(((Int64)leftInt16) * ((Int64)((UInt32)right)));
                            if (Int16.MinValue <= result && result <= Int16.MaxValue) {
                                return (Int16)result;
                            } else return result;
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.Multiply(leftInt16, (Int64)right);
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.MultiplyImpl(leftInt16, (UInt64)right);
                        }
                    case TypeCode.Single: {
                            return (Double)(((Double)leftInt16) * ((Double)((Single)right)));
                        }
                    case TypeCode.Double: {
                            return FloatOps.Multiply(leftInt16, (Double)right);
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__sub__")]
        public static object Subtract(object left, object right) {
            Debug.Assert(left is Int16);
            Int16 leftInt16 = (Int16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            Int32 result = (Int32)(((Int32)leftInt16) - ((Int32)((Byte)right)));
                            if (Int16.MinValue <= result && result <= Int16.MaxValue) {
                                return (Int16)result;
                            } else return result;
                        }
                    case TypeCode.SByte: {
                            Int32 result = (Int32)(((Int32)leftInt16) - ((Int32)((SByte)right)));
                            if (Int16.MinValue <= result && result <= Int16.MaxValue) {
                                return (Int16)result;
                            } else return result;
                        }
                    case TypeCode.Int16: {
                            Int32 result = (Int32)(((Int32)leftInt16) - ((Int32)((Int16)right)));
                            if (Int16.MinValue <= result && result <= Int16.MaxValue) {
                                return (Int16)result;
                            } else return result;
                        }
                    case TypeCode.UInt16: {
                            Int32 result = (Int32)(((Int32)leftInt16) - ((Int32)((UInt16)right)));
                            if (Int16.MinValue <= result && result <= Int16.MaxValue) {
                                return (Int16)result;
                            } else return result;
                        }
                    case TypeCode.Int32: {
                            Int64 result = (Int64)(((Int64)leftInt16) - ((Int64)((Int32)right)));
                            if (Int16.MinValue <= result && result <= Int16.MaxValue) {
                                return (Int16)result;
                            } else return result;
                        }
                    case TypeCode.UInt32: {
                            Int64 result = (Int64)(((Int64)leftInt16) - ((Int64)((UInt32)right)));
                            if (Int16.MinValue <= result && result <= Int16.MaxValue) {
                                return (Int16)result;
                            } else return result;
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.Subtract(leftInt16, (Int64)right);
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.SubtractImpl(leftInt16, (UInt64)right);
                        }
                    case TypeCode.Single: {
                            return (Single)(((Single)leftInt16) - ((Single)((Single)right)));
                        }
                    case TypeCode.Double: {
                            return (Double)(((Double)leftInt16) - ((Double)((Double)right)));
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__radd__")]
        public static object ReverseAdd(object left, object right) {
            Debug.Assert(left is Int16);
            Int16 leftInt16 = (Int16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            Int32 result = (Int32)(((Int32)((Byte)right)) + ((Int32)leftInt16));
                            if (Int16.MinValue <= result && result <= Int16.MaxValue) {
                                return (Int16)result;
                            } else return result;
                        }
                    case TypeCode.SByte: {
                            Int32 result = (Int32)(((Int32)((SByte)right)) + ((Int32)leftInt16));
                            if (Int16.MinValue <= result && result <= Int16.MaxValue) {
                                return (Int16)result;
                            } else return result;
                        }
                    case TypeCode.Int16: {
                            Int32 result = (Int32)(((Int32)((Int16)right)) + ((Int32)leftInt16));
                            if (Int16.MinValue <= result && result <= Int16.MaxValue) {
                                return (Int16)result;
                            } else return result;
                        }
                    case TypeCode.UInt16: {
                            Int32 result = (Int32)(((Int32)((UInt16)right)) + ((Int32)leftInt16));
                            if (Int16.MinValue <= result && result <= Int16.MaxValue) {
                                return (Int16)result;
                            } else return result;
                        }
                    case TypeCode.Int32: {
                            Int64 result = (Int64)(((Int64)((Int32)right)) + ((Int64)leftInt16));
                            if (Int16.MinValue <= result && result <= Int16.MaxValue) {
                                return (Int16)result;
                            } else return result;
                        }
                    case TypeCode.UInt32: {
                            Int64 result = (Int64)(((Int64)((UInt32)right)) + ((Int64)leftInt16));
                            if (Int16.MinValue <= result && result <= Int16.MaxValue) {
                                return (Int16)result;
                            } else return result;
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.ReverseAdd(leftInt16, (Int64)right);
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ReverseAddImpl(leftInt16, (UInt64)right);
                        }
                    case TypeCode.Single: {
                            return (Single)(((Single)((Single)right)) + ((Single)leftInt16));
                        }
                    case TypeCode.Double: {
                            return (Double)(((Double)((Double)right)) + ((Double)leftInt16));
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rdiv__")]
        public static object ReverseDivide(object left, object right) {
            Debug.Assert(left is Int16);
            Int16 leftInt16 = (Int16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            return Int16Ops.ReverseDivideImpl((Int16)leftInt16, (Int16)((Byte)right));
                        }
                    case TypeCode.SByte: {
                            return Int16Ops.ReverseDivideImpl((Int16)leftInt16, (Int16)((SByte)right));
                        }
                    case TypeCode.Int16: {
                            return Int16Ops.ReverseDivideImpl((Int16)leftInt16, (Int16)((Int16)right));
                        }
                    case TypeCode.UInt16: {
                            return IntOps.ReverseDivide((Int32)leftInt16, (Int32)((UInt16)right));
                        }
                    case TypeCode.Int32: {
                            return IntOps.ReverseDivide((Int32)leftInt16, (Int32)((Int32)right));
                        }
                    case TypeCode.UInt32: {
                            return Int64Ops.ReverseDivide((Int64)leftInt16, (Int64)((UInt32)right));
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.ReverseDivide((Int64)leftInt16, (Int64)((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ReverseDivideImpl(leftInt16, (UInt64)right);
                        }
                    case TypeCode.Single: {
                            return SingleOps.ReverseDivideImpl((Single)leftInt16, (Single)((Single)right));
                        }
                    case TypeCode.Double: {
                            return FloatOps.ReverseDivide((Double)leftInt16, (Double)((Double)right));
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rfloordiv__")]
        public static object ReverseFloorDivide(object left, object right) {
            Debug.Assert(left is Int16);
            Int16 leftInt16 = (Int16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            return Int16Ops.ReverseFloorDivideImpl((Int16)leftInt16, (Int16)((Byte)right));
                        }
                    case TypeCode.SByte: {
                            return Int16Ops.ReverseFloorDivideImpl((Int16)leftInt16, (Int16)((SByte)right));
                        }
                    case TypeCode.Int16: {
                            return Int16Ops.ReverseFloorDivideImpl((Int16)leftInt16, (Int16)((Int16)right));
                        }
                    case TypeCode.UInt16: {
                            return IntOps.ReverseFloorDivide((Int32)leftInt16, (Int32)((UInt16)right));
                        }
                    case TypeCode.Int32: {
                            return IntOps.ReverseFloorDivide((Int32)leftInt16, (Int32)((Int32)right));
                        }
                    case TypeCode.UInt32: {
                            return Int64Ops.ReverseFloorDivide((Int64)leftInt16, (Int64)((UInt32)right));
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.ReverseFloorDivide((Int64)leftInt16, (Int64)((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ReverseFloorDivideImpl(leftInt16, (UInt64)right);
                        }
                    case TypeCode.Single: {
                            return SingleOps.ReverseFloorDivideImpl((Single)leftInt16, (Single)((Single)right));
                        }
                    case TypeCode.Double: {
                            return FloatOps.ReverseFloorDivide((Double)leftInt16, (Double)((Double)right));
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rmod__")]
        public static object ReverseMod(object left, object right) {
            Debug.Assert(left is Int16);
            Int16 leftInt16 = (Int16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            return Int16Ops.ReverseModImpl((Int16)leftInt16, (Int16)((Byte)right));
                        }
                    case TypeCode.SByte: {
                            return Int16Ops.ReverseModImpl((Int16)leftInt16, (Int16)((SByte)right));
                        }
                    case TypeCode.Int16: {
                            return Int16Ops.ReverseModImpl((Int16)leftInt16, (Int16)((Int16)right));
                        }
                    case TypeCode.UInt16: {
                            return IntOps.ReverseMod((Int32)leftInt16, (Int32)((UInt16)right));
                        }
                    case TypeCode.Int32: {
                            return IntOps.ReverseMod((Int32)leftInt16, (Int32)((Int32)right));
                        }
                    case TypeCode.UInt32: {
                            return Int64Ops.ReverseMod((Int64)leftInt16, (Int64)((UInt32)right));
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.ReverseMod((Int64)leftInt16, (Int64)((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ReverseModImpl(leftInt16, (UInt64)right);
                        }
                    case TypeCode.Single: {
                            return SingleOps.ReverseModImpl((Single)leftInt16, (Single)((Single)right));
                        }
                    case TypeCode.Double: {
                            return FloatOps.ReverseMod((Double)leftInt16, (Double)((Double)right));
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rmul__")]
        public static object ReverseMultiply(object left, object right) {
            Debug.Assert(left is Int16);
            Int16 leftInt16 = (Int16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            Int32 result = (Int32)(((Int32)((Byte)right)) * ((Int32)leftInt16));
                            if (Int16.MinValue <= result && result <= Int16.MaxValue) {
                                return (Int16)result;
                            } else return result;
                        }
                    case TypeCode.SByte: {
                            Int32 result = (Int32)(((Int32)((SByte)right)) * ((Int32)leftInt16));
                            if (Int16.MinValue <= result && result <= Int16.MaxValue) {
                                return (Int16)result;
                            } else return result;
                        }
                    case TypeCode.Int16: {
                            Int32 result = (Int32)(((Int32)((Int16)right)) * ((Int32)leftInt16));
                            if (Int16.MinValue <= result && result <= Int16.MaxValue) {
                                return (Int16)result;
                            } else return result;
                        }
                    case TypeCode.UInt16: {
                            Int32 result = (Int32)(((Int32)((UInt16)right)) * ((Int32)leftInt16));
                            if (Int16.MinValue <= result && result <= Int16.MaxValue) {
                                return (Int16)result;
                            } else return result;
                        }
                    case TypeCode.Int32: {
                            Int64 result = (Int64)(((Int64)((Int32)right)) * ((Int64)leftInt16));
                            if (Int16.MinValue <= result && result <= Int16.MaxValue) {
                                return (Int16)result;
                            } else return result;
                        }
                    case TypeCode.UInt32: {
                            Int64 result = (Int64)(((Int64)((UInt32)right)) * ((Int64)leftInt16));
                            if (Int16.MinValue <= result && result <= Int16.MaxValue) {
                                return (Int16)result;
                            } else return result;
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.ReverseMultiply(leftInt16, (Int64)right);
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ReverseMultiplyImpl(leftInt16, (UInt64)right);
                        }
                    case TypeCode.Single: {
                            return (Double)(((Double)((Single)right)) * ((Double)leftInt16));
                        }
                    case TypeCode.Double: {
                            return FloatOps.ReverseMultiply(leftInt16, (Double)right);
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rsub__")]
        public static object ReverseSubtract(object left, object right) {
            Debug.Assert(left is Int16);
            Int16 leftInt16 = (Int16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            Int32 result = (Int32)(((Int32)((Byte)right)) - ((Int32)leftInt16));
                            if (Int16.MinValue <= result && result <= Int16.MaxValue) {
                                return (Int16)result;
                            } else return result;
                        }
                    case TypeCode.SByte: {
                            Int32 result = (Int32)(((Int32)((SByte)right)) - ((Int32)leftInt16));
                            if (Int16.MinValue <= result && result <= Int16.MaxValue) {
                                return (Int16)result;
                            } else return result;
                        }
                    case TypeCode.Int16: {
                            Int32 result = (Int32)(((Int32)((Int16)right)) - ((Int32)leftInt16));
                            if (Int16.MinValue <= result && result <= Int16.MaxValue) {
                                return (Int16)result;
                            } else return result;
                        }
                    case TypeCode.UInt16: {
                            Int32 result = (Int32)(((Int32)((UInt16)right)) - ((Int32)leftInt16));
                            if (Int16.MinValue <= result && result <= Int16.MaxValue) {
                                return (Int16)result;
                            } else return result;
                        }
                    case TypeCode.Int32: {
                            Int64 result = (Int64)(((Int64)((Int32)right)) - ((Int64)leftInt16));
                            if (Int16.MinValue <= result && result <= Int16.MaxValue) {
                                return (Int16)result;
                            } else return result;
                        }
                    case TypeCode.UInt32: {
                            Int64 result = (Int64)(((Int64)((UInt32)right)) - ((Int64)leftInt16));
                            if (Int16.MinValue <= result && result <= Int16.MaxValue) {
                                return (Int16)result;
                            } else return result;
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.ReverseSubtract(leftInt16, (Int64)right);
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ReverseSubtractImpl(leftInt16, (UInt64)right);
                        }
                    case TypeCode.Single: {
                            return (Single)(((Single)((Single)right)) - ((Single)leftInt16));
                        }
                    case TypeCode.Double: {
                            return (Double)(((Double)((Double)right)) - ((Double)leftInt16));
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__and__")]
        public static object BitwiseAnd(object left, object right) {
            Debug.Assert(left is Int16);
            Int16 leftInt16 = (Int16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            Int16 rightInt16 = (Int16)(Byte)right;
                            return leftInt16 & rightInt16;
                        }
                    case TypeCode.SByte: {
                            Int16 rightInt16 = (Int16)(SByte)right;
                            return leftInt16 & rightInt16;
                        }
                    case TypeCode.Int16: {
                            return leftInt16 & (Int16)right;
                        }
                    case TypeCode.UInt16: {
                            Int32 leftInt32 = (Int32)leftInt16;
                            Int32 rightInt32 = (Int32)(UInt16)right;
                            return leftInt32 & rightInt32;
                        }
                    case TypeCode.Int32: {
                            Int32 leftInt32 = (Int32)leftInt16;
                            return leftInt32 & (Int32)right;
                        }
                    case TypeCode.UInt32: {
                            Int64 leftInt64 = (Int64)leftInt16;
                            Int64 rightInt64 = (Int64)(UInt32)right;
                            return leftInt64 & rightInt64;
                        }
                    case TypeCode.Int64: {
                            Int64 leftInt64 = (Int64)leftInt16;
                            return leftInt64 & (Int64)right;
                        }
                    case TypeCode.UInt64: {
                            Int64 leftInt64 = (Int64)leftInt16;
                            return leftInt64 & (Int64)(UInt64)right;
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rand__")]
        public static object ReverseBitwiseAnd(object left, object right) {
            Debug.Assert(left is Int16);
            Int16 leftInt16 = (Int16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            Int16 rightInt16 = (Int16)(Byte)right;
                            return leftInt16 & rightInt16;
                        }
                    case TypeCode.SByte: {
                            Int16 rightInt16 = (Int16)(SByte)right;
                            return leftInt16 & rightInt16;
                        }
                    case TypeCode.Int16: {
                            return leftInt16 & (Int16)right;
                        }
                    case TypeCode.UInt16: {
                            Int32 leftInt32 = (Int32)leftInt16;
                            Int32 rightInt32 = (Int32)(UInt16)right;
                            return leftInt32 & rightInt32;
                        }
                    case TypeCode.Int32: {
                            Int32 leftInt32 = (Int32)leftInt16;
                            return leftInt32 & (Int32)right;
                        }
                    case TypeCode.UInt32: {
                            Int64 leftInt64 = (Int64)leftInt16;
                            Int64 rightInt64 = (Int64)(UInt32)right;
                            return leftInt64 & rightInt64;
                        }
                    case TypeCode.Int64: {
                            Int64 leftInt64 = (Int64)leftInt16;
                            return leftInt64 & (Int64)right;
                        }
                    case TypeCode.UInt64: {
                            Int64 leftInt64 = (Int64)leftInt16;
                            return leftInt64 & (Int64)(UInt64)right;
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__or__")]
        public static object BitwiseOr(object left, object right) {
            Debug.Assert(left is Int16);
            Int16 leftInt16 = (Int16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            Int16 rightInt16 = (Int16)(Byte)right;
                            return leftInt16 | rightInt16;
                        }
                    case TypeCode.SByte: {
                            Int16 rightInt16 = (Int16)(SByte)right;
                            return leftInt16 | rightInt16;
                        }
                    case TypeCode.Int16: {
                            return leftInt16 | (Int16)right;
                        }
                    case TypeCode.UInt16: {
                            Int32 leftInt32 = (Int32)leftInt16;
                            Int32 rightInt32 = (Int32)(UInt16)right;
                            return leftInt32 | rightInt32;
                        }
                    case TypeCode.Int32: {
                            Int32 leftInt32 = (Int32)leftInt16;
                            return leftInt32 | (Int32)right;
                        }
                    case TypeCode.UInt32: {
                            Int64 leftInt64 = (Int64)leftInt16;
                            Int64 rightInt64 = (Int64)(UInt32)right;
                            return leftInt64 | rightInt64;
                        }
                    case TypeCode.Int64: {
                            Int64 leftInt64 = (Int64)leftInt16;
                            return leftInt64 | (Int64)right;
                        }
                    case TypeCode.UInt64: {
                            Int64 leftInt64 = (Int64)leftInt16;
                            return leftInt64 | (Int64)(UInt64)right;
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__ror__")]
        public static object ReverseBitwiseOr(object left, object right) {
            Debug.Assert(left is Int16);
            Int16 leftInt16 = (Int16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            Int16 rightInt16 = (Int16)(Byte)right;
                            return leftInt16 | rightInt16;
                        }
                    case TypeCode.SByte: {
                            Int16 rightInt16 = (Int16)(SByte)right;
                            return leftInt16 | rightInt16;
                        }
                    case TypeCode.Int16: {
                            return leftInt16 | (Int16)right;
                        }
                    case TypeCode.UInt16: {
                            Int32 leftInt32 = (Int32)leftInt16;
                            Int32 rightInt32 = (Int32)(UInt16)right;
                            return leftInt32 | rightInt32;
                        }
                    case TypeCode.Int32: {
                            Int32 leftInt32 = (Int32)leftInt16;
                            return leftInt32 | (Int32)right;
                        }
                    case TypeCode.UInt32: {
                            Int64 leftInt64 = (Int64)leftInt16;
                            Int64 rightInt64 = (Int64)(UInt32)right;
                            return leftInt64 | rightInt64;
                        }
                    case TypeCode.Int64: {
                            Int64 leftInt64 = (Int64)leftInt16;
                            return leftInt64 | (Int64)right;
                        }
                    case TypeCode.UInt64: {
                            Int64 leftInt64 = (Int64)leftInt16;
                            return leftInt64 | (Int64)(UInt64)right;
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rxor__")]
        public static object BitwiseXor(object left, object right) {
            Debug.Assert(left is Int16);
            Int16 leftInt16 = (Int16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            Int16 rightInt16 = (Int16)(Byte)right;
                            return leftInt16 ^ rightInt16;
                        }
                    case TypeCode.SByte: {
                            Int16 rightInt16 = (Int16)(SByte)right;
                            return leftInt16 ^ rightInt16;
                        }
                    case TypeCode.Int16: {
                            return leftInt16 ^ (Int16)right;
                        }
                    case TypeCode.UInt16: {
                            Int32 leftInt32 = (Int32)leftInt16;
                            Int32 rightInt32 = (Int32)(UInt16)right;
                            return leftInt32 ^ rightInt32;
                        }
                    case TypeCode.Int32: {
                            Int32 leftInt32 = (Int32)leftInt16;
                            return leftInt32 ^ (Int32)right;
                        }
                    case TypeCode.UInt32: {
                            Int64 leftInt64 = (Int64)leftInt16;
                            Int64 rightInt64 = (Int64)(UInt32)right;
                            return leftInt64 ^ rightInt64;
                        }
                    case TypeCode.Int64: {
                            Int64 leftInt64 = (Int64)leftInt16;
                            return leftInt64 ^ (Int64)right;
                        }
                    case TypeCode.UInt64: {
                            Int64 leftInt64 = (Int64)leftInt16;
                            return leftInt64 ^ (Int64)(UInt64)right;
                        }
                }
            }
            return Ops.NotImplemented;
        }
        [PythonName("__xor__")]
        public static object ReverseBitwiseXor(object left, object right) {
            Debug.Assert(left is Int16);
            Int16 leftInt16 = (Int16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            Int16 rightInt16 = (Int16)(Byte)right;
                            return leftInt16 ^ rightInt16;
                        }
                    case TypeCode.SByte: {
                            Int16 rightInt16 = (Int16)(SByte)right;
                            return leftInt16 ^ rightInt16;
                        }
                    case TypeCode.Int16: {
                            return leftInt16 ^ (Int16)right;
                        }
                    case TypeCode.UInt16: {
                            Int32 leftInt32 = (Int32)leftInt16;
                            Int32 rightInt32 = (Int32)(UInt16)right;
                            return leftInt32 ^ rightInt32;
                        }
                    case TypeCode.Int32: {
                            Int32 leftInt32 = (Int32)leftInt16;
                            return leftInt32 ^ (Int32)right;
                        }
                    case TypeCode.UInt32: {
                            Int64 leftInt64 = (Int64)leftInt16;
                            Int64 rightInt64 = (Int64)(UInt32)right;
                            return leftInt64 ^ rightInt64;
                        }
                    case TypeCode.Int64: {
                            Int64 leftInt64 = (Int64)leftInt16;
                            return leftInt64 ^ (Int64)right;
                        }
                    case TypeCode.UInt64: {
                            Int64 leftInt64 = (Int64)leftInt16;
                            return leftInt64 ^ (Int64)(UInt64)right;
                        }
                }
            }
            return Ops.NotImplemented;
        }
        internal static object DivideImpl(Int16 x, Int16 y) {
            // special case (MinValue / -1) doesn't fit
            if (x == Int16.MinValue && y == -1) {
                return (Int32)((Int32)Int16.MaxValue + 1);
            }
            Int16 q = (Int16)(x / y);
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

        internal static object ReverseDivideImpl(Int16 x, Int16 y) {
            return DivideImpl(y, x);
        }

        internal static object ModImpl(Int16 x, Int16 y) {
            Int16 r = (Int16)(x % y);
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

        internal static object ReverseModImpl(Int16 x, Int16 y) {
            return ModImpl(y, x);
        }

        internal static object FloorDivideImpl(Int16 x, Int16 y) {
            return DivideImpl(x, y);
        }

        internal static object ReverseFloorDivideImpl(Int16 x, Int16 y) {
            return DivideImpl(y, x);
        }


        // *** END GENERATED CODE ***

        #endregion
    }
}
