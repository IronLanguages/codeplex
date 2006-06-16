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
using IronMath;
using IronPython.Runtime;
using IronPython.Runtime.Types;

namespace IronPython.Runtime.Operations {
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
                            return Int64Ops.Add(leftInt16, ((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.AddImpl(leftInt16, ((UInt64)right));
                        }
                    case TypeCode.Single: {
                            return (Single)(((Single)leftInt16) + ((Single)((Single)right)));
                        }
                    case TypeCode.Double: {
                            return (Double)(((Double)leftInt16) + ((Double)((Double)right)));
                        }
                }
            }
            if (right is BigInteger) {
                return LongOps.Add(leftInt16, (BigInteger)right);
            } else if (right is Complex64) {
                return ComplexOps.Add(leftInt16, (Complex64)right);
            } else if (right is ExtensibleInt) {
                Int64 result = (Int64)(((Int64)leftInt16) + ((Int64)((ExtensibleInt)right).value));
                if (Int16.MinValue <= result && result <= Int16.MaxValue) {
                    return (Int16)result;
                } else return result;
            } else if (right is ExtensibleLong) {
                return LongOps.Add(leftInt16, ((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return (Double)(((Double)leftInt16) + ((Double)((ExtensibleFloat)right).value));
            } else if (right is ExtensibleComplex) {
                return ComplexOps.Add(leftInt16, ((ExtensibleComplex)right).value);
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
                            return UInt64Ops.DivideImpl(leftInt16, ((UInt64)right));
                        }
                    case TypeCode.Single: {
                            return SingleOps.DivideImpl((Single)leftInt16, (Single)((Single)right));
                        }
                    case TypeCode.Double: {
                            return FloatOps.Divide((Double)leftInt16, (Double)((Double)right));
                        }
                }
            }
            if (right is BigInteger) {
                return LongOps.Divide(leftInt16, (BigInteger)right);
            } else if (right is Complex64) {
                return FloatOps.Divide(leftInt16, (Complex64)right);
            } else if (right is ExtensibleInt) {
                return IntOps.Divide((Int32)leftInt16, (Int32)((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.Divide(leftInt16, ((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return FloatOps.Divide((Double)leftInt16, (Double)((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return FloatOps.Divide(leftInt16, ((ExtensibleComplex)right).value);
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
                            return UInt64Ops.FloorDivideImpl(leftInt16, ((UInt64)right));
                        }
                    case TypeCode.Single: {
                            return SingleOps.FloorDivideImpl((Single)leftInt16, (Single)((Single)right));
                        }
                    case TypeCode.Double: {
                            return FloatOps.FloorDivide((Double)leftInt16, (Double)((Double)right));
                        }
                }
            }
            if (right is BigInteger) {
                return LongOps.FloorDivide(leftInt16, (BigInteger)right);
            } else if (right is Complex64) {
                return FloatOps.FloorDivide(leftInt16, (Complex64)right);
            } else if (right is ExtensibleInt) {
                return IntOps.FloorDivide((Int32)leftInt16, (Int32)((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.FloorDivide(leftInt16, ((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return FloatOps.FloorDivide((Double)leftInt16, (Double)((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return FloatOps.FloorDivide(leftInt16, ((ExtensibleComplex)right).value);
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
                            return UInt64Ops.ModImpl(leftInt16, ((UInt64)right));
                        }
                    case TypeCode.Single: {
                            return SingleOps.ModImpl((Single)leftInt16, (Single)((Single)right));
                        }
                    case TypeCode.Double: {
                            return FloatOps.Mod((Double)leftInt16, (Double)((Double)right));
                        }
                }
            }
            if (right is BigInteger) {
                return LongOps.Mod(leftInt16, (BigInteger)right);
            } else if (right is Complex64) {
                return FloatOps.Mod(leftInt16, (Complex64)right);
            } else if (right is ExtensibleInt) {
                return IntOps.Mod((Int32)leftInt16, (Int32)((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.Mod(leftInt16, ((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return FloatOps.Mod((Double)leftInt16, (Double)((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return FloatOps.Mod(leftInt16, ((ExtensibleComplex)right).value);
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
                            return Int64Ops.Multiply(leftInt16, ((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.MultiplyImpl(leftInt16, ((UInt64)right));
                        }
                    case TypeCode.Single: {
                            return (Double)(((Double)leftInt16) * ((Double)((Single)right)));
                        }
                    case TypeCode.Double: {
                            return FloatOps.Multiply(leftInt16, ((Double)right));
                        }
                }
            }
            if (right is BigInteger) {
                return LongOps.Multiply(leftInt16, (BigInteger)right);
            } else if (right is Complex64) {
                return ComplexOps.Multiply(leftInt16, (Complex64)right);
            } else if (right is ExtensibleInt) {
                Int64 result = (Int64)(((Int64)leftInt16) * ((Int64)((ExtensibleInt)right).value));
                if (Int16.MinValue <= result && result <= Int16.MaxValue) {
                    return (Int16)result;
                } else return result;
            } else if (right is ExtensibleLong) {
                return LongOps.Multiply(leftInt16, ((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return FloatOps.Multiply(leftInt16, ((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return ComplexOps.Multiply(leftInt16, ((ExtensibleComplex)right).value);
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
                            return Int64Ops.Subtract(leftInt16, ((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.SubtractImpl(leftInt16, ((UInt64)right));
                        }
                    case TypeCode.Single: {
                            return (Single)(((Single)leftInt16) - ((Single)((Single)right)));
                        }
                    case TypeCode.Double: {
                            return (Double)(((Double)leftInt16) - ((Double)((Double)right)));
                        }
                }
            }
            if (right is BigInteger) {
                return LongOps.Subtract(leftInt16, (BigInteger)right);
            } else if (right is Complex64) {
                return ComplexOps.Subtract(leftInt16, (Complex64)right);
            } else if (right is ExtensibleInt) {
                Int64 result = (Int64)(((Int64)leftInt16) - ((Int64)((ExtensibleInt)right).value));
                if (Int16.MinValue <= result && result <= Int16.MaxValue) {
                    return (Int16)result;
                } else return result;
            } else if (right is ExtensibleLong) {
                return LongOps.Subtract(leftInt16, ((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return (Double)(((Double)leftInt16) - ((Double)((ExtensibleFloat)right).value));
            } else if (right is ExtensibleComplex) {
                return ComplexOps.Subtract(leftInt16, ((ExtensibleComplex)right).value);
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
                            return Int64Ops.ReverseAdd(leftInt16, ((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ReverseAddImpl(leftInt16, ((UInt64)right));
                        }
                    case TypeCode.Single: {
                            return (Single)(((Single)((Single)right)) + ((Single)leftInt16));
                        }
                    case TypeCode.Double: {
                            return (Double)(((Double)((Double)right)) + ((Double)leftInt16));
                        }
                }
            }
            if (right is BigInteger) {
                return LongOps.ReverseAdd(leftInt16, (BigInteger)right);
            } else if (right is Complex64) {
                return ComplexOps.ReverseAdd(leftInt16, (Complex64)right);
            } else if (right is ExtensibleInt) {
                Int64 result = (Int64)(((Int64)((ExtensibleInt)right).value) + ((Int64)leftInt16));
                if (Int16.MinValue <= result && result <= Int16.MaxValue) {
                    return (Int16)result;
                } else return result;
            } else if (right is ExtensibleLong) {
                return LongOps.ReverseAdd(leftInt16, ((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return (Double)(((Double)((ExtensibleFloat)right).value) + ((Double)leftInt16));
            } else if (right is ExtensibleComplex) {
                return ComplexOps.ReverseAdd(leftInt16, ((ExtensibleComplex)right).value);
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
                            return UInt64Ops.ReverseDivideImpl(leftInt16, ((UInt64)right));
                        }
                    case TypeCode.Single: {
                            return SingleOps.ReverseDivideImpl((Single)leftInt16, (Single)((Single)right));
                        }
                    case TypeCode.Double: {
                            return FloatOps.ReverseDivide((Double)leftInt16, (Double)((Double)right));
                        }
                }
            }
            if (right is BigInteger) {
                return LongOps.ReverseDivide(leftInt16, (BigInteger)right);
            } else if (right is Complex64) {
                return FloatOps.ReverseDivide(leftInt16, (Complex64)right);
            } else if (right is ExtensibleInt) {
                return IntOps.ReverseDivide((Int32)leftInt16, (Int32)((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.ReverseDivide(leftInt16, ((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return FloatOps.ReverseDivide((Double)leftInt16, (Double)((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return FloatOps.ReverseDivide(leftInt16, ((ExtensibleComplex)right).value);
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
                            return UInt64Ops.ReverseFloorDivideImpl(leftInt16, ((UInt64)right));
                        }
                    case TypeCode.Single: {
                            return SingleOps.ReverseFloorDivideImpl((Single)leftInt16, (Single)((Single)right));
                        }
                    case TypeCode.Double: {
                            return FloatOps.ReverseFloorDivide((Double)leftInt16, (Double)((Double)right));
                        }
                }
            }
            if (right is BigInteger) {
                return LongOps.ReverseFloorDivide(leftInt16, (BigInteger)right);
            } else if (right is Complex64) {
                return FloatOps.ReverseFloorDivide(leftInt16, (Complex64)right);
            } else if (right is ExtensibleInt) {
                return IntOps.ReverseFloorDivide((Int32)leftInt16, (Int32)((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.ReverseFloorDivide(leftInt16, ((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return FloatOps.ReverseFloorDivide((Double)leftInt16, (Double)((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return FloatOps.ReverseFloorDivide(leftInt16, ((ExtensibleComplex)right).value);
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
                            return UInt64Ops.ReverseModImpl(leftInt16, ((UInt64)right));
                        }
                    case TypeCode.Single: {
                            return SingleOps.ReverseModImpl((Single)leftInt16, (Single)((Single)right));
                        }
                    case TypeCode.Double: {
                            return FloatOps.ReverseMod((Double)leftInt16, (Double)((Double)right));
                        }
                }
            }
            if (right is BigInteger) {
                return LongOps.ReverseMod(leftInt16, (BigInteger)right);
            } else if (right is Complex64) {
                return FloatOps.ReverseMod(leftInt16, (Complex64)right);
            } else if (right is ExtensibleInt) {
                return IntOps.ReverseMod((Int32)leftInt16, (Int32)((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.ReverseMod(leftInt16, ((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return FloatOps.ReverseMod((Double)leftInt16, (Double)((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return FloatOps.ReverseMod(leftInt16, ((ExtensibleComplex)right).value);
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
                            return Int64Ops.ReverseMultiply(leftInt16, ((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ReverseMultiplyImpl(leftInt16, ((UInt64)right));
                        }
                    case TypeCode.Single: {
                            return (Double)(((Double)((Single)right)) * ((Double)leftInt16));
                        }
                    case TypeCode.Double: {
                            return FloatOps.ReverseMultiply(leftInt16, ((Double)right));
                        }
                }
            }
            if (right is BigInteger) {
                return LongOps.ReverseMultiply(leftInt16, (BigInteger)right);
            } else if (right is Complex64) {
                return ComplexOps.ReverseMultiply(leftInt16, (Complex64)right);
            } else if (right is ExtensibleInt) {
                Int64 result = (Int64)(((Int64)((ExtensibleInt)right).value) * ((Int64)leftInt16));
                if (Int16.MinValue <= result && result <= Int16.MaxValue) {
                    return (Int16)result;
                } else return result;
            } else if (right is ExtensibleLong) {
                return LongOps.ReverseMultiply(leftInt16, ((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return FloatOps.ReverseMultiply(leftInt16, ((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return ComplexOps.ReverseMultiply(leftInt16, ((ExtensibleComplex)right).value);
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
                            return Int64Ops.ReverseSubtract(leftInt16, ((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ReverseSubtractImpl(leftInt16, ((UInt64)right));
                        }
                    case TypeCode.Single: {
                            return (Single)(((Single)((Single)right)) - ((Single)leftInt16));
                        }
                    case TypeCode.Double: {
                            return (Double)(((Double)((Double)right)) - ((Double)leftInt16));
                        }
                }
            }
            if (right is BigInteger) {
                return LongOps.ReverseSubtract(leftInt16, (BigInteger)right);
            } else if (right is Complex64) {
                return ComplexOps.ReverseSubtract(leftInt16, (Complex64)right);
            } else if (right is ExtensibleInt) {
                Int64 result = (Int64)(((Int64)((ExtensibleInt)right).value) - ((Int64)leftInt16));
                if (Int16.MinValue <= result && result <= Int16.MaxValue) {
                    return (Int16)result;
                } else return result;
            } else if (right is ExtensibleLong) {
                return LongOps.ReverseSubtract(leftInt16, ((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return (Double)(((Double)((ExtensibleFloat)right).value) - ((Double)leftInt16));
            } else if (right is ExtensibleComplex) {
                return ComplexOps.ReverseSubtract(leftInt16, ((ExtensibleComplex)right).value);
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
            if (right is BigInteger) {
                BigInteger leftBigInteger = (BigInteger)leftInt16;
                return leftBigInteger & (BigInteger)right;
            } else if (right is ExtensibleInt) {
                Int32 leftInt32 = (Int32)leftInt16;
                return leftInt32 & (Int32)((ExtensibleInt)right).value;
            } else if (right is ExtensibleLong) {
                BigInteger leftBigInteger = (BigInteger)leftInt16;
                return leftBigInteger & (BigInteger)((ExtensibleLong)right).Value;
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
            if (right is BigInteger) {
                BigInteger leftBigInteger = (BigInteger)leftInt16;
                return leftBigInteger & (BigInteger)right;
            } else if (right is ExtensibleInt) {
                Int32 leftInt32 = (Int32)leftInt16;
                return leftInt32 & (Int32)((ExtensibleInt)right).value;
            } else if (right is ExtensibleLong) {
                BigInteger leftBigInteger = (BigInteger)leftInt16;
                return leftBigInteger & (BigInteger)((ExtensibleLong)right).Value;
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
            if (right is BigInteger) {
                BigInteger leftBigInteger = (BigInteger)leftInt16;
                return leftBigInteger | (BigInteger)right;
            } else if (right is ExtensibleInt) {
                Int32 leftInt32 = (Int32)leftInt16;
                return leftInt32 | (Int32)((ExtensibleInt)right).value;
            } else if (right is ExtensibleLong) {
                BigInteger leftBigInteger = (BigInteger)leftInt16;
                return leftBigInteger | (BigInteger)((ExtensibleLong)right).Value;
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
            if (right is BigInteger) {
                BigInteger leftBigInteger = (BigInteger)leftInt16;
                return leftBigInteger | (BigInteger)right;
            } else if (right is ExtensibleInt) {
                Int32 leftInt32 = (Int32)leftInt16;
                return leftInt32 | (Int32)((ExtensibleInt)right).value;
            } else if (right is ExtensibleLong) {
                BigInteger leftBigInteger = (BigInteger)leftInt16;
                return leftBigInteger | (BigInteger)((ExtensibleLong)right).Value;
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
            if (right is BigInteger) {
                BigInteger leftBigInteger = (BigInteger)leftInt16;
                return leftBigInteger ^ (BigInteger)right;
            } else if (right is ExtensibleInt) {
                Int32 leftInt32 = (Int32)leftInt16;
                return leftInt32 ^ (Int32)((ExtensibleInt)right).value;
            } else if (right is ExtensibleLong) {
                BigInteger leftBigInteger = (BigInteger)leftInt16;
                return leftBigInteger ^ (BigInteger)((ExtensibleLong)right).Value;
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
            if (right is BigInteger) {
                BigInteger leftBigInteger = (BigInteger)leftInt16;
                return leftBigInteger ^ (BigInteger)right;
            } else if (right is ExtensibleInt) {
                Int32 leftInt32 = (Int32)leftInt16;
                return leftInt32 ^ (Int32)((ExtensibleInt)right).value;
            } else if (right is ExtensibleLong) {
                BigInteger leftBigInteger = (BigInteger)leftInt16;
                return leftBigInteger ^ (BigInteger)((ExtensibleLong)right).Value;
            }
            return Ops.NotImplemented;
        }
        [PythonName("__divmod__")]
        public static object DivMod(object left, object right) {
            Debug.Assert(left is Int16);
            Int16 leftInt16 = (Int16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte:
                        return Int16Ops.DivModImpl(leftInt16, (Byte)right);
                    case TypeCode.SByte:
                        return Int16Ops.DivModImpl(leftInt16, (SByte)right);
                    case TypeCode.Int16:
                        return Int16Ops.DivModImpl(leftInt16, (Int16)right);
                    case TypeCode.UInt16:
                        return IntOps.DivMod(leftInt16, (UInt16)right);
                    case TypeCode.Int32:
                        return IntOps.DivMod(leftInt16, (Int32)right);
                    case TypeCode.UInt32:
                        return Int64Ops.DivMod(leftInt16, (UInt32)right);
                    case TypeCode.Int64:
                        return Int64Ops.DivMod(leftInt16, (Int64)right);
                    case TypeCode.UInt64:
                        return UInt64Ops.DivModImpl(leftInt16, (UInt64)right);
                    case TypeCode.Single:
                        return SingleOps.DivModImpl(leftInt16, (Single)right);
                    case TypeCode.Double:
                        return FloatOps.DivMod(leftInt16, (Double)right);
                }
            }
            if (right is BigInteger) {
                return LongOps.DivMod(leftInt16, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return IntOps.DivMod(leftInt16, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.DivMod(leftInt16, ((ExtensibleLong)right).Value);
            } else if (right is Complex64) {
                return ComplexOps.DivMod(leftInt16, (Complex64)right);
            } else if (right is ExtensibleFloat) {
                return FloatOps.DivMod(leftInt16, ((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return ComplexOps.DivMod(leftInt16, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rdivmod__")]
        public static object ReverseDivMod(object left, object right) {
            Debug.Assert(left is Int16);
            Int16 leftInt16 = (Int16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte:
                        return Int16Ops.ReverseDivModImpl(leftInt16, (Byte)right);
                    case TypeCode.SByte:
                        return Int16Ops.ReverseDivModImpl(leftInt16, (SByte)right);
                    case TypeCode.Int16:
                        return Int16Ops.ReverseDivModImpl(leftInt16, (Int16)right);
                    case TypeCode.UInt16:
                        return IntOps.ReverseDivMod(leftInt16, (UInt16)right);
                    case TypeCode.Int32:
                        return IntOps.ReverseDivMod(leftInt16, (Int32)right);
                    case TypeCode.UInt32:
                        return Int64Ops.ReverseDivMod(leftInt16, (UInt32)right);
                    case TypeCode.Int64:
                        return Int64Ops.ReverseDivMod(leftInt16, (Int64)right);
                    case TypeCode.UInt64:
                        return UInt64Ops.ReverseDivModImpl(leftInt16, (UInt64)right);
                    case TypeCode.Single:
                        return SingleOps.ReverseDivModImpl(leftInt16, (Single)right);
                    case TypeCode.Double:
                        return FloatOps.ReverseDivMod(leftInt16, (Double)right);
                }
            }
            if (right is BigInteger) {
                return LongOps.ReverseDivMod(leftInt16, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return IntOps.ReverseDivMod(leftInt16, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.ReverseDivMod(leftInt16, ((ExtensibleLong)right).Value);
            } else if (right is Complex64) {
                return ComplexOps.ReverseDivMod(leftInt16, (Complex64)right);
            } else if (right is ExtensibleFloat) {
                return FloatOps.ReverseDivMod(leftInt16, ((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return ComplexOps.ReverseDivMod(leftInt16, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__lshift__")]
        public static object LeftShift(object left, object right) {
            Debug.Assert(left is Int16);
            Int16 leftInt16 = (Int16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte:
                        return Int16Ops.LeftShiftImpl(leftInt16, (Byte)right);
                    case TypeCode.SByte:
                        return Int16Ops.LeftShiftImpl(leftInt16, (SByte)right);
                    case TypeCode.Int16:
                        return Int16Ops.LeftShiftImpl(leftInt16, (Int16)right);
                    case TypeCode.UInt16:
                        return IntOps.LeftShift(leftInt16, (UInt16)right);
                    case TypeCode.Int32:
                        return IntOps.LeftShift(leftInt16, (Int32)right);
                    case TypeCode.UInt32:
                        return Int64Ops.LeftShift(leftInt16, (UInt32)right);
                    case TypeCode.Int64:
                        return Int64Ops.LeftShift(leftInt16, (Int64)right);
                    case TypeCode.UInt64:
                        return UInt64Ops.LeftShiftImpl(leftInt16, (UInt64)right);
                }
            }
            if (right is BigInteger) {
                return LongOps.LeftShift(leftInt16, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return IntOps.LeftShift(leftInt16, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.LeftShift(leftInt16, ((ExtensibleLong)right).Value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rlshift__")]
        public static object ReverseLeftShift(object left, object right) {
            Debug.Assert(left is Int16);
            Int16 leftInt16 = (Int16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte:
                        return Int16Ops.ReverseLeftShiftImpl(leftInt16, (Byte)right);
                    case TypeCode.SByte:
                        return Int16Ops.ReverseLeftShiftImpl(leftInt16, (SByte)right);
                    case TypeCode.Int16:
                        return Int16Ops.ReverseLeftShiftImpl(leftInt16, (Int16)right);
                    case TypeCode.UInt16:
                        return IntOps.ReverseLeftShift(leftInt16, (UInt16)right);
                    case TypeCode.Int32:
                        return IntOps.ReverseLeftShift(leftInt16, (Int32)right);
                    case TypeCode.UInt32:
                        return Int64Ops.ReverseLeftShift(leftInt16, (UInt32)right);
                    case TypeCode.Int64:
                        return Int64Ops.ReverseLeftShift(leftInt16, (Int64)right);
                    case TypeCode.UInt64:
                        return UInt64Ops.ReverseLeftShiftImpl(leftInt16, (UInt64)right);
                }
            }
            if (right is BigInteger) {
                return LongOps.ReverseLeftShift(leftInt16, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return IntOps.ReverseLeftShift(leftInt16, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.ReverseLeftShift(leftInt16, ((ExtensibleLong)right).Value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__pow__")]
        public static object Power(object left, object right) {
            Debug.Assert(left is Int16);
            Int16 leftInt16 = (Int16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte:
                        return Int16Ops.PowerImpl(leftInt16, (Byte)right);
                    case TypeCode.SByte:
                        return Int16Ops.PowerImpl(leftInt16, (SByte)right);
                    case TypeCode.Int16:
                        return Int16Ops.PowerImpl(leftInt16, (Int16)right);
                    case TypeCode.UInt16:
                        return IntOps.Power(leftInt16, (UInt16)right);
                    case TypeCode.Int32:
                        return IntOps.Power(leftInt16, (Int32)right);
                    case TypeCode.UInt32:
                        return Int64Ops.Power(leftInt16, (UInt32)right);
                    case TypeCode.Int64:
                        return Int64Ops.Power(leftInt16, (Int64)right);
                    case TypeCode.UInt64:
                        return UInt64Ops.PowerImpl(leftInt16, (UInt64)right);
                    case TypeCode.Single:
                        return SingleOps.PowerImpl(leftInt16, (Single)right);
                    case TypeCode.Double:
                        return FloatOps.Power(leftInt16, (Double)right);
                }
            }
            if (right is BigInteger) {
                return LongOps.Power(leftInt16, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return IntOps.Power(leftInt16, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.Power(leftInt16, ((ExtensibleLong)right).Value);
            } else if (right is Complex64) {
                return ComplexOps.Power(leftInt16, (Complex64)right);
            } else if (right is ExtensibleFloat) {
                return FloatOps.Power(leftInt16, ((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return ComplexOps.Power(leftInt16, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rpow__")]
        public static object ReversePower(object left, object right) {
            Debug.Assert(left is Int16);
            Int16 leftInt16 = (Int16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte:
                        return Int16Ops.ReversePowerImpl(leftInt16, (Byte)right);
                    case TypeCode.SByte:
                        return Int16Ops.ReversePowerImpl(leftInt16, (SByte)right);
                    case TypeCode.Int16:
                        return Int16Ops.ReversePowerImpl(leftInt16, (Int16)right);
                    case TypeCode.UInt16:
                        return IntOps.ReversePower(leftInt16, (UInt16)right);
                    case TypeCode.Int32:
                        return IntOps.ReversePower(leftInt16, (Int32)right);
                    case TypeCode.UInt32:
                        return Int64Ops.ReversePower(leftInt16, (UInt32)right);
                    case TypeCode.Int64:
                        return Int64Ops.ReversePower(leftInt16, (Int64)right);
                    case TypeCode.UInt64:
                        return UInt64Ops.ReversePowerImpl(leftInt16, (UInt64)right);
                    case TypeCode.Single:
                        return SingleOps.ReversePowerImpl(leftInt16, (Single)right);
                    case TypeCode.Double:
                        return FloatOps.ReversePower(leftInt16, (Double)right);
                }
            }
            if (right is BigInteger) {
                return LongOps.ReversePower(leftInt16, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return IntOps.ReversePower(leftInt16, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.ReversePower(leftInt16, ((ExtensibleLong)right).Value);
            } else if (right is Complex64) {
                return ComplexOps.ReversePower(leftInt16, (Complex64)right);
            } else if (right is ExtensibleFloat) {
                return FloatOps.ReversePower(leftInt16, ((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return ComplexOps.ReversePower(leftInt16, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rshift__")]
        public static object RightShift(object left, object right) {
            Debug.Assert(left is Int16);
            Int16 leftInt16 = (Int16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte:
                        return Int16Ops.RightShiftImpl(leftInt16, (Byte)right);
                    case TypeCode.SByte:
                        return Int16Ops.RightShiftImpl(leftInt16, (SByte)right);
                    case TypeCode.Int16:
                        return Int16Ops.RightShiftImpl(leftInt16, (Int16)right);
                    case TypeCode.UInt16:
                        return IntOps.RightShift(leftInt16, (UInt16)right);
                    case TypeCode.Int32:
                        return IntOps.RightShift(leftInt16, (Int32)right);
                    case TypeCode.UInt32:
                        return Int64Ops.RightShift(leftInt16, (UInt32)right);
                    case TypeCode.Int64:
                        return Int64Ops.RightShift(leftInt16, (Int64)right);
                    case TypeCode.UInt64:
                        return UInt64Ops.RightShiftImpl(leftInt16, (UInt64)right);
                }
            }
            if (right is BigInteger) {
                return LongOps.RightShift(leftInt16, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return IntOps.RightShift(leftInt16, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.RightShift(leftInt16, ((ExtensibleLong)right).Value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rrshift__")]
        public static object ReverseRightShift(object left, object right) {
            Debug.Assert(left is Int16);
            Int16 leftInt16 = (Int16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte:
                        return Int16Ops.ReverseRightShiftImpl(leftInt16, (Byte)right);
                    case TypeCode.SByte:
                        return Int16Ops.ReverseRightShiftImpl(leftInt16, (SByte)right);
                    case TypeCode.Int16:
                        return Int16Ops.ReverseRightShiftImpl(leftInt16, (Int16)right);
                    case TypeCode.UInt16:
                        return IntOps.ReverseRightShift(leftInt16, (UInt16)right);
                    case TypeCode.Int32:
                        return IntOps.ReverseRightShift(leftInt16, (Int32)right);
                    case TypeCode.UInt32:
                        return Int64Ops.ReverseRightShift(leftInt16, (UInt32)right);
                    case TypeCode.Int64:
                        return Int64Ops.ReverseRightShift(leftInt16, (Int64)right);
                    case TypeCode.UInt64:
                        return UInt64Ops.ReverseRightShiftImpl(leftInt16, (UInt64)right);
                }
            }
            if (right is BigInteger) {
                return LongOps.ReverseRightShift(leftInt16, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return IntOps.ReverseRightShift(leftInt16, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.ReverseRightShift(leftInt16, ((ExtensibleLong)right).Value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__truediv__")]
        public static object TrueDivide(object left, object right) {
            Debug.Assert(left is Int16);
            Int16 leftInt16 = (Int16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte:
                        return FloatOps.TrueDivide(leftInt16, (Byte)right);
                    case TypeCode.SByte:
                        return FloatOps.TrueDivide(leftInt16, (SByte)right);
                    case TypeCode.Int16:
                        return FloatOps.TrueDivide(leftInt16, (Int16)right);
                    case TypeCode.UInt16:
                        return FloatOps.TrueDivide(leftInt16, (UInt16)right);
                    case TypeCode.Int32:
                        return FloatOps.TrueDivide(leftInt16, (Int32)right);
                    case TypeCode.UInt32:
                        return FloatOps.TrueDivide(leftInt16, (UInt32)right);
                    case TypeCode.Int64:
                        return FloatOps.TrueDivide(leftInt16, (Int64)right);
                    case TypeCode.UInt64:
                        return FloatOps.TrueDivide(leftInt16, (UInt64)right);
                    case TypeCode.Single:
                        return FloatOps.TrueDivide(leftInt16, (Single)right);
                    case TypeCode.Double:
                        return FloatOps.TrueDivide(leftInt16, (Double)right);
                }
            }
            if (right is BigInteger) {
                return FloatOps.TrueDivide(leftInt16, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return FloatOps.TrueDivide(leftInt16, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return FloatOps.TrueDivide(leftInt16, ((ExtensibleLong)right).Value);
            } else if (right is Complex64) {
                return FloatOps.TrueDivide(leftInt16, (Complex64)right);
            } else if (right is ExtensibleFloat) {
                return FloatOps.TrueDivide(leftInt16, ((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return FloatOps.TrueDivide(leftInt16, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rtruediv__")]
        public static object ReverseTrueDivide(object left, object right) {
            Debug.Assert(left is Int16);
            Int16 leftInt16 = (Int16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte:
                        return FloatOps.ReverseTrueDivide(leftInt16, (Byte)right);
                    case TypeCode.SByte:
                        return FloatOps.ReverseTrueDivide(leftInt16, (SByte)right);
                    case TypeCode.Int16:
                        return FloatOps.ReverseTrueDivide(leftInt16, (Int16)right);
                    case TypeCode.UInt16:
                        return FloatOps.ReverseTrueDivide(leftInt16, (UInt16)right);
                    case TypeCode.Int32:
                        return FloatOps.ReverseTrueDivide(leftInt16, (Int32)right);
                    case TypeCode.UInt32:
                        return FloatOps.ReverseTrueDivide(leftInt16, (UInt32)right);
                    case TypeCode.Int64:
                        return FloatOps.ReverseTrueDivide(leftInt16, (Int64)right);
                    case TypeCode.UInt64:
                        return FloatOps.ReverseTrueDivide(leftInt16, (UInt64)right);
                    case TypeCode.Single:
                        return FloatOps.ReverseTrueDivide(leftInt16, (Single)right);
                    case TypeCode.Double:
                        return FloatOps.ReverseTrueDivide(leftInt16, (Double)right);
                }
            }
            if (right is BigInteger) {
                return FloatOps.ReverseTrueDivide(leftInt16, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return FloatOps.ReverseTrueDivide(leftInt16, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return FloatOps.ReverseTrueDivide(leftInt16, ((ExtensibleLong)right).Value);
            } else if (right is Complex64) {
                return FloatOps.ReverseTrueDivide(leftInt16, (Complex64)right);
            } else if (right is ExtensibleFloat) {
                return FloatOps.ReverseTrueDivide(leftInt16, ((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return FloatOps.ReverseTrueDivide(leftInt16, ((ExtensibleComplex)right).value);
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


        internal static object DivModImpl(Int16 x, Int16 y) {
            object div = DivideImpl(x, y);
            if (div == Ops.NotImplemented) return div;
            object mod = ModImpl(x, y);
            if (mod == Ops.NotImplemented) return mod;
            return Tuple.MakeTuple(div, mod);
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
        internal static object ReverseDivModImpl(Int16 x, Int16 y) {
            return DivModImpl(y, x);
        }
        internal static object FloorDivideImpl(Int16 x, Int16 y) {
            return DivideImpl(x, y);
        }
        internal static object ReverseFloorDivideImpl(Int16 x, Int16 y) {
            return DivideImpl(y, x);
        }
        internal static object ReverseLeftShiftImpl(Int16 x, Int16 y) {
            return LeftShiftImpl(y, x);
        }
        internal static object ReversePowerImpl(Int16 x, Int16 y) {
            return PowerImpl(y, x);
        }
        internal static object ReverseRightShiftImpl(Int16 x, Int16 y) {
            return RightShiftImpl(y, x);
        }


        // *** END GENERATED CODE ***

        #endregion
    }
}
