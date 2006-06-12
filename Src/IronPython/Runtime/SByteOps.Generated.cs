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
using IronMath;

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
            } else if (right is BigInteger) {
                return LongOps.Add(leftSByte, (BigInteger)right);
            } else if (right is Complex64) {
                return ComplexOps.Add(leftSByte, (Complex64)right);
            } else if (right is INumber) {
                return ((INumber)right).ReverseAdd(left);
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
            } else if (right is BigInteger) {
                return LongOps.Divide(leftSByte, (BigInteger)right);
            } else if (right is Complex64) {
                return ComplexOps.Divide(leftSByte, (Complex64)right);
            } else if (right is INumber) {
                return ((INumber)right).ReverseDivide(left);
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
            } else if (right is BigInteger) {
                return LongOps.FloorDivide(leftSByte, (BigInteger)right);
            } else if (right is Complex64) {
                return ComplexOps.FloorDivide(leftSByte, (Complex64)right);
            } else if (right is INumber) {
                return ((INumber)right).ReverseFloorDivide(left);
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
            } else if (right is BigInteger) {
                return LongOps.Mod(leftSByte, (BigInteger)right);
            } else if (right is Complex64) {
                return ComplexOps.Mod(leftSByte, (Complex64)right);
            } else if (right is INumber) {
                return ((INumber)right).ReverseMod(left);
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
            } else if (right is BigInteger) {
                return LongOps.Multiply(leftSByte, (BigInteger)right);
            } else if (right is Complex64) {
                return ComplexOps.Multiply(leftSByte, (Complex64)right);
            } else if (right is INumber) {
                return ((INumber)right).ReverseMultiply(left);
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
            } else if (right is BigInteger) {
                return LongOps.Subtract(leftSByte, (BigInteger)right);
            } else if (right is Complex64) {
                return ComplexOps.Subtract(leftSByte, (Complex64)right);
            } else if (right is INumber) {
                return ((INumber)right).ReverseSubtract(left);
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
            } else if (right is BigInteger) {
                return LongOps.ReverseAdd(leftSByte, (BigInteger)right);
            } else if (right is Complex64) {
                return ComplexOps.ReverseAdd(leftSByte, (Complex64)right);
            } else if (right is INumber) {
                return ((INumber)right).Add(left);
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
            } else if (right is BigInteger) {
                return LongOps.ReverseDivide(leftSByte, (BigInteger)right);
            } else if (right is Complex64) {
                return ComplexOps.ReverseDivide(leftSByte, (Complex64)right);
            } else if (right is INumber) {
                return ((INumber)right).Divide(left);
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
            } else if (right is BigInteger) {
                return LongOps.ReverseFloorDivide(leftSByte, (BigInteger)right);
            } else if (right is Complex64) {
                return ComplexOps.ReverseFloorDivide(leftSByte, (Complex64)right);
            } else if (right is INumber) {
                return ((INumber)right).FloorDivide(left);
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
            } else if (right is BigInteger) {
                return LongOps.ReverseMod(leftSByte, (BigInteger)right);
            } else if (right is Complex64) {
                return ComplexOps.ReverseMod(leftSByte, (Complex64)right);
            } else if (right is INumber) {
                return ((INumber)right).Mod(left);
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
            } else if (right is BigInteger) {
                return LongOps.ReverseMultiply(leftSByte, (BigInteger)right);
            } else if (right is Complex64) {
                return ComplexOps.ReverseMultiply(leftSByte, (Complex64)right);
            } else if (right is INumber) {
                return ((INumber)right).Multiply(left);
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
            } else if (right is BigInteger) {
                return LongOps.ReverseSubtract(leftSByte, (BigInteger)right);
            } else if (right is Complex64) {
                return ComplexOps.ReverseSubtract(leftSByte, (Complex64)right);
            } else if (right is INumber) {
                return ((INumber)right).Subtract(left);
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
            } else if (right is BigInteger) {
                BigInteger leftBigInteger = (BigInteger)leftSByte;
                return leftBigInteger & (BigInteger)right;
            } else if (right is ExtensibleInt) {
                Int32 leftInt32 = (Int32)leftSByte;
                return leftInt32 & (Int32)((ExtensibleInt)right).value;
            } else if (right is ExtensibleLong) {
                BigInteger leftBigInteger = (BigInteger)leftSByte;
                return leftBigInteger & (BigInteger)((ExtensibleLong)right).Value;
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
            } else if (right is BigInteger) {
                BigInteger leftBigInteger = (BigInteger)leftSByte;
                return leftBigInteger & (BigInteger)right;
            } else if (right is ExtensibleInt) {
                Int32 leftInt32 = (Int32)leftSByte;
                return leftInt32 & (Int32)((ExtensibleInt)right).value;
            } else if (right is ExtensibleLong) {
                BigInteger leftBigInteger = (BigInteger)leftSByte;
                return leftBigInteger & (BigInteger)((ExtensibleLong)right).Value;
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
            } else if (right is BigInteger) {
                BigInteger leftBigInteger = (BigInteger)leftSByte;
                return leftBigInteger | (BigInteger)right;
            } else if (right is ExtensibleInt) {
                Int32 leftInt32 = (Int32)leftSByte;
                return leftInt32 | (Int32)((ExtensibleInt)right).value;
            } else if (right is ExtensibleLong) {
                BigInteger leftBigInteger = (BigInteger)leftSByte;
                return leftBigInteger | (BigInteger)((ExtensibleLong)right).Value;
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
            } else if (right is BigInteger) {
                BigInteger leftBigInteger = (BigInteger)leftSByte;
                return leftBigInteger | (BigInteger)right;
            } else if (right is ExtensibleInt) {
                Int32 leftInt32 = (Int32)leftSByte;
                return leftInt32 | (Int32)((ExtensibleInt)right).value;
            } else if (right is ExtensibleLong) {
                BigInteger leftBigInteger = (BigInteger)leftSByte;
                return leftBigInteger | (BigInteger)((ExtensibleLong)right).Value;
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
            } else if (right is BigInteger) {
                BigInteger leftBigInteger = (BigInteger)leftSByte;
                return leftBigInteger ^ (BigInteger)right;
            } else if (right is ExtensibleInt) {
                Int32 leftInt32 = (Int32)leftSByte;
                return leftInt32 ^ (Int32)((ExtensibleInt)right).value;
            } else if (right is ExtensibleLong) {
                BigInteger leftBigInteger = (BigInteger)leftSByte;
                return leftBigInteger ^ (BigInteger)((ExtensibleLong)right).Value;
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
            } else if (right is BigInteger) {
                BigInteger leftBigInteger = (BigInteger)leftSByte;
                return leftBigInteger ^ (BigInteger)right;
            } else if (right is ExtensibleInt) {
                Int32 leftInt32 = (Int32)leftSByte;
                return leftInt32 ^ (Int32)((ExtensibleInt)right).value;
            } else if (right is ExtensibleLong) {
                BigInteger leftBigInteger = (BigInteger)leftSByte;
                return leftBigInteger ^ (BigInteger)((ExtensibleLong)right).Value;
            }
            return Ops.NotImplemented;
        }
        [PythonName("__divmod__")]
        public static object DivMod(object left, object right) {
            Debug.Assert(left is SByte);
            SByte leftSByte = (SByte)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte:
                        return Int16Ops.DivModImpl(leftSByte, (Byte)right);
                    case TypeCode.SByte:
                        return SByteOps.DivModImpl(leftSByte, (SByte)right);
                    case TypeCode.Int16:
                        return Int16Ops.DivModImpl(leftSByte, (Int16)right);
                    case TypeCode.UInt16:
                        return IntOps.DivMod(leftSByte, (UInt16)right);
                    case TypeCode.Int32:
                        return IntOps.DivMod(leftSByte, (Int32)right);
                    case TypeCode.UInt32:
                        return Int64Ops.DivMod(leftSByte, (UInt32)right);
                    case TypeCode.Int64:
                        return Int64Ops.DivMod(leftSByte, (Int64)right);
                    case TypeCode.UInt64:
                        return UInt64Ops.DivModImpl(leftSByte, (UInt64)right);
                    case TypeCode.Single:
                        return SingleOps.DivModImpl(leftSByte, (Single)right);
                    case TypeCode.Double:
                        return FloatOps.DivMod(leftSByte, (Double)right);
                }
            } else if (right is BigInteger) {
                return LongOps.DivMod(leftSByte, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return IntOps.DivMod(leftSByte, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.DivMod(leftSByte, ((ExtensibleLong)right).Value);
            } else if (right is Complex64) {
                return ComplexOps.DivMod(leftSByte, (Complex64)right);
            } else if (right is ExtensibleFloat) {
                return FloatOps.DivMod(leftSByte, ((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return ComplexOps.DivMod(leftSByte, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rdivmod__")]
        public static object ReverseDivMod(object left, object right) {
            Debug.Assert(left is SByte);
            SByte leftSByte = (SByte)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte:
                        return Int16Ops.ReverseDivModImpl(leftSByte, (Byte)right);
                    case TypeCode.SByte:
                        return SByteOps.ReverseDivModImpl(leftSByte, (SByte)right);
                    case TypeCode.Int16:
                        return Int16Ops.ReverseDivModImpl(leftSByte, (Int16)right);
                    case TypeCode.UInt16:
                        return IntOps.ReverseDivMod(leftSByte, (UInt16)right);
                    case TypeCode.Int32:
                        return IntOps.ReverseDivMod(leftSByte, (Int32)right);
                    case TypeCode.UInt32:
                        return Int64Ops.ReverseDivMod(leftSByte, (UInt32)right);
                    case TypeCode.Int64:
                        return Int64Ops.ReverseDivMod(leftSByte, (Int64)right);
                    case TypeCode.UInt64:
                        return UInt64Ops.ReverseDivModImpl(leftSByte, (UInt64)right);
                    case TypeCode.Single:
                        return SingleOps.ReverseDivModImpl(leftSByte, (Single)right);
                    case TypeCode.Double:
                        return FloatOps.ReverseDivMod(leftSByte, (Double)right);
                }
            } else if (right is BigInteger) {
                return LongOps.ReverseDivMod(leftSByte, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return IntOps.ReverseDivMod(leftSByte, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.ReverseDivMod(leftSByte, ((ExtensibleLong)right).Value);
            } else if (right is Complex64) {
                return ComplexOps.ReverseDivMod(leftSByte, (Complex64)right);
            } else if (right is ExtensibleFloat) {
                return FloatOps.ReverseDivMod(leftSByte, ((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return ComplexOps.ReverseDivMod(leftSByte, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__lshift__")]
        public static object LeftShift(object left, object right) {
            Debug.Assert(left is SByte);
            SByte leftSByte = (SByte)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte:
                        return Int16Ops.LeftShiftImpl(leftSByte, (Byte)right);
                    case TypeCode.SByte:
                        return SByteOps.LeftShiftImpl(leftSByte, (SByte)right);
                    case TypeCode.Int16:
                        return Int16Ops.LeftShiftImpl(leftSByte, (Int16)right);
                    case TypeCode.UInt16:
                        return IntOps.LeftShift(leftSByte, (UInt16)right);
                    case TypeCode.Int32:
                        return IntOps.LeftShift(leftSByte, (Int32)right);
                    case TypeCode.UInt32:
                        return Int64Ops.LeftShift(leftSByte, (UInt32)right);
                    case TypeCode.Int64:
                        return Int64Ops.LeftShift(leftSByte, (Int64)right);
                    case TypeCode.UInt64:
                        return UInt64Ops.LeftShiftImpl(leftSByte, (UInt64)right);
                }
            } else if (right is BigInteger) {
                return LongOps.LeftShift(leftSByte, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return IntOps.LeftShift(leftSByte, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.LeftShift(leftSByte, ((ExtensibleLong)right).Value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rlshift__")]
        public static object ReverseLeftShift(object left, object right) {
            Debug.Assert(left is SByte);
            SByte leftSByte = (SByte)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte:
                        return Int16Ops.ReverseLeftShiftImpl(leftSByte, (Byte)right);
                    case TypeCode.SByte:
                        return SByteOps.ReverseLeftShiftImpl(leftSByte, (SByte)right);
                    case TypeCode.Int16:
                        return Int16Ops.ReverseLeftShiftImpl(leftSByte, (Int16)right);
                    case TypeCode.UInt16:
                        return IntOps.ReverseLeftShift(leftSByte, (UInt16)right);
                    case TypeCode.Int32:
                        return IntOps.ReverseLeftShift(leftSByte, (Int32)right);
                    case TypeCode.UInt32:
                        return Int64Ops.ReverseLeftShift(leftSByte, (UInt32)right);
                    case TypeCode.Int64:
                        return Int64Ops.ReverseLeftShift(leftSByte, (Int64)right);
                    case TypeCode.UInt64:
                        return UInt64Ops.ReverseLeftShiftImpl(leftSByte, (UInt64)right);
                }
            } else if (right is BigInteger) {
                return LongOps.ReverseLeftShift(leftSByte, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return IntOps.ReverseLeftShift(leftSByte, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.ReverseLeftShift(leftSByte, ((ExtensibleLong)right).Value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__pow__")]
        public static object Power(object left, object right) {
            Debug.Assert(left is SByte);
            SByte leftSByte = (SByte)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte:
                        return Int16Ops.PowerImpl(leftSByte, (Byte)right);
                    case TypeCode.SByte:
                        return SByteOps.PowerImpl(leftSByte, (SByte)right);
                    case TypeCode.Int16:
                        return Int16Ops.PowerImpl(leftSByte, (Int16)right);
                    case TypeCode.UInt16:
                        return IntOps.Power(leftSByte, (UInt16)right);
                    case TypeCode.Int32:
                        return IntOps.Power(leftSByte, (Int32)right);
                    case TypeCode.UInt32:
                        return Int64Ops.Power(leftSByte, (UInt32)right);
                    case TypeCode.Int64:
                        return Int64Ops.Power(leftSByte, (Int64)right);
                    case TypeCode.UInt64:
                        return UInt64Ops.PowerImpl(leftSByte, (UInt64)right);
                    case TypeCode.Single:
                        return SingleOps.PowerImpl(leftSByte, (Single)right);
                    case TypeCode.Double:
                        return FloatOps.Power(leftSByte, (Double)right);
                }
            } else if (right is BigInteger) {
                return LongOps.Power(leftSByte, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return IntOps.Power(leftSByte, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.Power(leftSByte, ((ExtensibleLong)right).Value);
            } else if (right is Complex64) {
                return ComplexOps.Power(leftSByte, (Complex64)right);
            } else if (right is ExtensibleFloat) {
                return FloatOps.Power(leftSByte, ((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return ComplexOps.Power(leftSByte, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rpow__")]
        public static object ReversePower(object left, object right) {
            Debug.Assert(left is SByte);
            SByte leftSByte = (SByte)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte:
                        return Int16Ops.ReversePowerImpl(leftSByte, (Byte)right);
                    case TypeCode.SByte:
                        return SByteOps.ReversePowerImpl(leftSByte, (SByte)right);
                    case TypeCode.Int16:
                        return Int16Ops.ReversePowerImpl(leftSByte, (Int16)right);
                    case TypeCode.UInt16:
                        return IntOps.ReversePower(leftSByte, (UInt16)right);
                    case TypeCode.Int32:
                        return IntOps.ReversePower(leftSByte, (Int32)right);
                    case TypeCode.UInt32:
                        return Int64Ops.ReversePower(leftSByte, (UInt32)right);
                    case TypeCode.Int64:
                        return Int64Ops.ReversePower(leftSByte, (Int64)right);
                    case TypeCode.UInt64:
                        return UInt64Ops.ReversePowerImpl(leftSByte, (UInt64)right);
                    case TypeCode.Single:
                        return SingleOps.ReversePowerImpl(leftSByte, (Single)right);
                    case TypeCode.Double:
                        return FloatOps.ReversePower(leftSByte, (Double)right);
                }
            } else if (right is BigInteger) {
                return LongOps.ReversePower(leftSByte, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return IntOps.ReversePower(leftSByte, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.ReversePower(leftSByte, ((ExtensibleLong)right).Value);
            } else if (right is Complex64) {
                return ComplexOps.ReversePower(leftSByte, (Complex64)right);
            } else if (right is ExtensibleFloat) {
                return FloatOps.ReversePower(leftSByte, ((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return ComplexOps.ReversePower(leftSByte, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rshift__")]
        public static object RightShift(object left, object right) {
            Debug.Assert(left is SByte);
            SByte leftSByte = (SByte)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte:
                        return Int16Ops.RightShiftImpl(leftSByte, (Byte)right);
                    case TypeCode.SByte:
                        return SByteOps.RightShiftImpl(leftSByte, (SByte)right);
                    case TypeCode.Int16:
                        return Int16Ops.RightShiftImpl(leftSByte, (Int16)right);
                    case TypeCode.UInt16:
                        return IntOps.RightShift(leftSByte, (UInt16)right);
                    case TypeCode.Int32:
                        return IntOps.RightShift(leftSByte, (Int32)right);
                    case TypeCode.UInt32:
                        return Int64Ops.RightShift(leftSByte, (UInt32)right);
                    case TypeCode.Int64:
                        return Int64Ops.RightShift(leftSByte, (Int64)right);
                    case TypeCode.UInt64:
                        return UInt64Ops.RightShiftImpl(leftSByte, (UInt64)right);
                }
            } else if (right is BigInteger) {
                return LongOps.RightShift(leftSByte, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return IntOps.RightShift(leftSByte, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.RightShift(leftSByte, ((ExtensibleLong)right).Value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rrshift__")]
        public static object ReverseRightShift(object left, object right) {
            Debug.Assert(left is SByte);
            SByte leftSByte = (SByte)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte:
                        return Int16Ops.ReverseRightShiftImpl(leftSByte, (Byte)right);
                    case TypeCode.SByte:
                        return SByteOps.ReverseRightShiftImpl(leftSByte, (SByte)right);
                    case TypeCode.Int16:
                        return Int16Ops.ReverseRightShiftImpl(leftSByte, (Int16)right);
                    case TypeCode.UInt16:
                        return IntOps.ReverseRightShift(leftSByte, (UInt16)right);
                    case TypeCode.Int32:
                        return IntOps.ReverseRightShift(leftSByte, (Int32)right);
                    case TypeCode.UInt32:
                        return Int64Ops.ReverseRightShift(leftSByte, (UInt32)right);
                    case TypeCode.Int64:
                        return Int64Ops.ReverseRightShift(leftSByte, (Int64)right);
                    case TypeCode.UInt64:
                        return UInt64Ops.ReverseRightShiftImpl(leftSByte, (UInt64)right);
                }
            } else if (right is BigInteger) {
                return LongOps.ReverseRightShift(leftSByte, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return IntOps.ReverseRightShift(leftSByte, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.ReverseRightShift(leftSByte, ((ExtensibleLong)right).Value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__truediv__")]
        public static object TrueDivide(object left, object right) {
            Debug.Assert(left is SByte);
            SByte leftSByte = (SByte)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte:
                        return FloatOps.TrueDivide(leftSByte, (Byte)right);
                    case TypeCode.SByte:
                        return FloatOps.TrueDivide(leftSByte, (SByte)right);
                    case TypeCode.Int16:
                        return FloatOps.TrueDivide(leftSByte, (Int16)right);
                    case TypeCode.UInt16:
                        return FloatOps.TrueDivide(leftSByte, (UInt16)right);
                    case TypeCode.Int32:
                        return FloatOps.TrueDivide(leftSByte, (Int32)right);
                    case TypeCode.UInt32:
                        return FloatOps.TrueDivide(leftSByte, (UInt32)right);
                    case TypeCode.Int64:
                        return FloatOps.TrueDivide(leftSByte, (Int64)right);
                    case TypeCode.UInt64:
                        return FloatOps.TrueDivide(leftSByte, (UInt64)right);
                    case TypeCode.Single:
                        return FloatOps.TrueDivide(leftSByte, (Single)right);
                    case TypeCode.Double:
                        return FloatOps.TrueDivide(leftSByte, (Double)right);
                }
            } else if (right is BigInteger) {
                return FloatOps.TrueDivide(leftSByte, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return FloatOps.TrueDivide(leftSByte, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return FloatOps.TrueDivide(leftSByte, ((ExtensibleLong)right).Value);
            } else if (right is Complex64) {
                return FloatOps.TrueDivide(leftSByte, (Complex64)right);
            } else if (right is ExtensibleFloat) {
                return FloatOps.TrueDivide(leftSByte, ((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return FloatOps.TrueDivide(leftSByte, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rtruediv__")]
        public static object ReverseTrueDivide(object left, object right) {
            Debug.Assert(left is SByte);
            SByte leftSByte = (SByte)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte:
                        return FloatOps.ReverseTrueDivide(leftSByte, (Byte)right);
                    case TypeCode.SByte:
                        return FloatOps.ReverseTrueDivide(leftSByte, (SByte)right);
                    case TypeCode.Int16:
                        return FloatOps.ReverseTrueDivide(leftSByte, (Int16)right);
                    case TypeCode.UInt16:
                        return FloatOps.ReverseTrueDivide(leftSByte, (UInt16)right);
                    case TypeCode.Int32:
                        return FloatOps.ReverseTrueDivide(leftSByte, (Int32)right);
                    case TypeCode.UInt32:
                        return FloatOps.ReverseTrueDivide(leftSByte, (UInt32)right);
                    case TypeCode.Int64:
                        return FloatOps.ReverseTrueDivide(leftSByte, (Int64)right);
                    case TypeCode.UInt64:
                        return FloatOps.ReverseTrueDivide(leftSByte, (UInt64)right);
                    case TypeCode.Single:
                        return FloatOps.ReverseTrueDivide(leftSByte, (Single)right);
                    case TypeCode.Double:
                        return FloatOps.ReverseTrueDivide(leftSByte, (Double)right);
                }
            } else if (right is BigInteger) {
                return FloatOps.ReverseTrueDivide(leftSByte, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return FloatOps.ReverseTrueDivide(leftSByte, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return FloatOps.ReverseTrueDivide(leftSByte, ((ExtensibleLong)right).Value);
            } else if (right is Complex64) {
                return FloatOps.ReverseTrueDivide(leftSByte, (Complex64)right);
            } else if (right is ExtensibleFloat) {
                return FloatOps.ReverseTrueDivide(leftSByte, ((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return FloatOps.ReverseTrueDivide(leftSByte, ((ExtensibleComplex)right).value);
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


        internal static object DivModImpl(SByte x, SByte y) {
            object div = DivideImpl(x, y);
            if (div == Ops.NotImplemented) return div;
            object mod = ModImpl(x, y);
            if (mod == Ops.NotImplemented) return mod;
            return Tuple.MakeTuple(div, mod);
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
        internal static object ReverseDivModImpl(SByte x, SByte y) {
            return DivModImpl(y, x);
        }
        internal static object FloorDivideImpl(SByte x, SByte y) {
            return DivideImpl(x, y);
        }
        internal static object ReverseFloorDivideImpl(SByte x, SByte y) {
            return DivideImpl(y, x);
        }
        internal static object ReverseLeftShiftImpl(SByte x, SByte y) {
            return LeftShiftImpl(y, x);
        }
        internal static object ReversePowerImpl(SByte x, SByte y) {
            return PowerImpl(y, x);
        }
        internal static object ReverseRightShiftImpl(SByte x, SByte y) {
            return RightShiftImpl(y, x);
        }


        // *** END GENERATED CODE ***

        #endregion
    }
}
