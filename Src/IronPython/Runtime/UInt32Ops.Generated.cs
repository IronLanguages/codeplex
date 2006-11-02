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

using IronPython.Runtime;
using IronPython.Runtime.Types;

namespace IronPython.Runtime.Operations {
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
                            return Int64Ops.Add(leftUInt32, ((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.AddImpl(leftUInt32, ((UInt64)right));
                        }
                    case TypeCode.Single: {
                            return (Single)(((Single)leftUInt32) + ((Single)((Single)right)));
                        }
                    case TypeCode.Double: {
                            return (Double)(((Double)leftUInt32) + ((Double)((Double)right)));
                        }
                }
            }
            if (right is BigInteger) {
                return LongOps.Add(leftUInt32, (BigInteger)right);
            } else if (right is Complex64) {
                return ComplexOps.Add(leftUInt32, (Complex64)right);
            } else if (right is ExtensibleInt) {
                Int64 result = (Int64)(((Int64)leftUInt32) + ((Int64)((ExtensibleInt)right).value));
                if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                    return (UInt32)result;
                } else return result;
            } else if (right is ExtensibleLong) {
                return LongOps.Add(leftUInt32, ((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return (Double)(((Double)leftUInt32) + ((Double)((ExtensibleFloat)right).value));
            } else if (right is ExtensibleComplex) {
                return ComplexOps.Add(leftUInt32, ((ExtensibleComplex)right).value);
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
            if (right is BigInteger) {
                return LongOps.Divide(leftUInt32, (BigInteger)right);
            } else if (right is Complex64) {
                return FloatOps.Divide(leftUInt32, (Complex64)right);
            } else if (right is ExtensibleInt) {
                return Int64Ops.Divide((Int64)leftUInt32, (Int64)((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.Divide(leftUInt32, ((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return FloatOps.Divide((Double)leftUInt32, (Double)((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return FloatOps.Divide(leftUInt32, ((ExtensibleComplex)right).value);
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
            if (right is BigInteger) {
                return LongOps.FloorDivide(leftUInt32, (BigInteger)right);
            } else if (right is Complex64) {
                return FloatOps.FloorDivide(leftUInt32, (Complex64)right);
            } else if (right is ExtensibleInt) {
                return Int64Ops.FloorDivide((Int64)leftUInt32, (Int64)((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.FloorDivide(leftUInt32, ((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return FloatOps.FloorDivide((Double)leftUInt32, (Double)((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return FloatOps.FloorDivide(leftUInt32, ((ExtensibleComplex)right).value);
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
            if (right is BigInteger) {
                return LongOps.Mod(leftUInt32, (BigInteger)right);
            } else if (right is Complex64) {
                return FloatOps.Mod(leftUInt32, (Complex64)right);
            } else if (right is ExtensibleInt) {
                return Int64Ops.Mod((Int64)leftUInt32, (Int64)((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.Mod(leftUInt32, ((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return FloatOps.Mod((Double)leftUInt32, (Double)((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return FloatOps.Mod(leftUInt32, ((ExtensibleComplex)right).value);
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
                            return Int64Ops.Multiply(leftUInt32, ((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.MultiplyImpl(leftUInt32, ((UInt64)right));
                        }
                    case TypeCode.Single: {
                            return (Double)(((Double)leftUInt32) * ((Double)((Single)right)));
                        }
                    case TypeCode.Double: {
                            return FloatOps.Multiply(leftUInt32, ((Double)right));
                        }
                }
            }
            if (right is BigInteger) {
                return LongOps.Multiply(leftUInt32, (BigInteger)right);
            } else if (right is Complex64) {
                return ComplexOps.Multiply(leftUInt32, (Complex64)right);
            } else if (right is ExtensibleInt) {
                Int64 result = (Int64)(((Int64)leftUInt32) * ((Int64)((ExtensibleInt)right).value));
                if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                    return (UInt32)result;
                } else return result;
            } else if (right is ExtensibleLong) {
                return LongOps.Multiply(leftUInt32, ((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return FloatOps.Multiply(leftUInt32, ((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return ComplexOps.Multiply(leftUInt32, ((ExtensibleComplex)right).value);
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
                            return Int64Ops.Subtract(leftUInt32, ((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.SubtractImpl(leftUInt32, ((UInt64)right));
                        }
                    case TypeCode.Single: {
                            return (Single)(((Single)leftUInt32) - ((Single)((Single)right)));
                        }
                    case TypeCode.Double: {
                            return (Double)(((Double)leftUInt32) - ((Double)((Double)right)));
                        }
                }
            }
            if (right is BigInteger) {
                return LongOps.Subtract(leftUInt32, (BigInteger)right);
            } else if (right is Complex64) {
                return ComplexOps.Subtract(leftUInt32, (Complex64)right);
            } else if (right is ExtensibleInt) {
                Int64 result = (Int64)(((Int64)leftUInt32) - ((Int64)((ExtensibleInt)right).value));
                if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                    return (UInt32)result;
                } else return result;
            } else if (right is ExtensibleLong) {
                return LongOps.Subtract(leftUInt32, ((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return (Double)(((Double)leftUInt32) - ((Double)((ExtensibleFloat)right).value));
            } else if (right is ExtensibleComplex) {
                return ComplexOps.Subtract(leftUInt32, ((ExtensibleComplex)right).value);
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
                            return Int64Ops.ReverseAdd(leftUInt32, ((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ReverseAddImpl(leftUInt32, ((UInt64)right));
                        }
                    case TypeCode.Single: {
                            return (Single)(((Single)((Single)right)) + ((Single)leftUInt32));
                        }
                    case TypeCode.Double: {
                            return (Double)(((Double)((Double)right)) + ((Double)leftUInt32));
                        }
                }
            }
            if (right is BigInteger) {
                return LongOps.ReverseAdd(leftUInt32, (BigInteger)right);
            } else if (right is Complex64) {
                return ComplexOps.ReverseAdd(leftUInt32, (Complex64)right);
            } else if (right is ExtensibleInt) {
                Int64 result = (Int64)(((Int64)((ExtensibleInt)right).value) + ((Int64)leftUInt32));
                if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                    return (UInt32)result;
                } else return result;
            } else if (right is ExtensibleLong) {
                return LongOps.ReverseAdd(leftUInt32, ((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return (Double)(((Double)((ExtensibleFloat)right).value) + ((Double)leftUInt32));
            } else if (right is ExtensibleComplex) {
                return ComplexOps.ReverseAdd(leftUInt32, ((ExtensibleComplex)right).value);
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
            if (right is BigInteger) {
                return LongOps.ReverseDivide(leftUInt32, (BigInteger)right);
            } else if (right is Complex64) {
                return FloatOps.ReverseDivide(leftUInt32, (Complex64)right);
            } else if (right is ExtensibleInt) {
                return Int64Ops.ReverseDivide((Int64)leftUInt32, (Int64)((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.ReverseDivide(leftUInt32, ((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return FloatOps.ReverseDivide((Double)leftUInt32, (Double)((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return FloatOps.ReverseDivide(leftUInt32, ((ExtensibleComplex)right).value);
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
            if (right is BigInteger) {
                return LongOps.ReverseFloorDivide(leftUInt32, (BigInteger)right);
            } else if (right is Complex64) {
                return FloatOps.ReverseFloorDivide(leftUInt32, (Complex64)right);
            } else if (right is ExtensibleInt) {
                return Int64Ops.ReverseFloorDivide((Int64)leftUInt32, (Int64)((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.ReverseFloorDivide(leftUInt32, ((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return FloatOps.ReverseFloorDivide((Double)leftUInt32, (Double)((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return FloatOps.ReverseFloorDivide(leftUInt32, ((ExtensibleComplex)right).value);
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
            if (right is BigInteger) {
                return LongOps.ReverseMod(leftUInt32, (BigInteger)right);
            } else if (right is Complex64) {
                return FloatOps.ReverseMod(leftUInt32, (Complex64)right);
            } else if (right is ExtensibleInt) {
                return Int64Ops.ReverseMod((Int64)leftUInt32, (Int64)((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.ReverseMod(leftUInt32, ((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return FloatOps.ReverseMod((Double)leftUInt32, (Double)((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return FloatOps.ReverseMod(leftUInt32, ((ExtensibleComplex)right).value);
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
                            return Int64Ops.ReverseMultiply(leftUInt32, ((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ReverseMultiplyImpl(leftUInt32, ((UInt64)right));
                        }
                    case TypeCode.Single: {
                            return (Double)(((Double)((Single)right)) * ((Double)leftUInt32));
                        }
                    case TypeCode.Double: {
                            return FloatOps.ReverseMultiply(leftUInt32, ((Double)right));
                        }
                }
            }
            if (right is BigInteger) {
                return LongOps.ReverseMultiply(leftUInt32, (BigInteger)right);
            } else if (right is Complex64) {
                return ComplexOps.ReverseMultiply(leftUInt32, (Complex64)right);
            } else if (right is ExtensibleInt) {
                Int64 result = (Int64)(((Int64)((ExtensibleInt)right).value) * ((Int64)leftUInt32));
                if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                    return (UInt32)result;
                } else return result;
            } else if (right is ExtensibleLong) {
                return LongOps.ReverseMultiply(leftUInt32, ((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return FloatOps.ReverseMultiply(leftUInt32, ((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return ComplexOps.ReverseMultiply(leftUInt32, ((ExtensibleComplex)right).value);
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
                            return Int64Ops.ReverseSubtract(leftUInt32, ((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ReverseSubtractImpl(leftUInt32, ((UInt64)right));
                        }
                    case TypeCode.Single: {
                            return (Single)(((Single)((Single)right)) - ((Single)leftUInt32));
                        }
                    case TypeCode.Double: {
                            return (Double)(((Double)((Double)right)) - ((Double)leftUInt32));
                        }
                }
            }
            if (right is BigInteger) {
                return LongOps.ReverseSubtract(leftUInt32, (BigInteger)right);
            } else if (right is Complex64) {
                return ComplexOps.ReverseSubtract(leftUInt32, (Complex64)right);
            } else if (right is ExtensibleInt) {
                Int64 result = (Int64)(((Int64)((ExtensibleInt)right).value) - ((Int64)leftUInt32));
                if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                    return (UInt32)result;
                } else return result;
            } else if (right is ExtensibleLong) {
                return LongOps.ReverseSubtract(leftUInt32, ((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return (Double)(((Double)((ExtensibleFloat)right).value) - ((Double)leftUInt32));
            } else if (right is ExtensibleComplex) {
                return ComplexOps.ReverseSubtract(leftUInt32, ((ExtensibleComplex)right).value);
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
            if (right is BigInteger) {
                BigInteger leftBigInteger = (BigInteger)leftUInt32;
                return leftBigInteger & (BigInteger)right;
            } else if (right is ExtensibleInt) {
                Int64 leftInt64 = (Int64)leftUInt32;
                Int64 rightInt64 = (Int64)((ExtensibleInt)right).value;
                return leftInt64 & rightInt64;
            } else if (right is ExtensibleLong) {
                BigInteger leftBigInteger = (BigInteger)leftUInt32;
                return leftBigInteger & (BigInteger)((ExtensibleLong)right).Value;
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
            if (right is BigInteger) {
                BigInteger leftBigInteger = (BigInteger)leftUInt32;
                return leftBigInteger & (BigInteger)right;
            } else if (right is ExtensibleInt) {
                Int64 leftInt64 = (Int64)leftUInt32;
                Int64 rightInt64 = (Int64)((ExtensibleInt)right).value;
                return leftInt64 & rightInt64;
            } else if (right is ExtensibleLong) {
                BigInteger leftBigInteger = (BigInteger)leftUInt32;
                return leftBigInteger & (BigInteger)((ExtensibleLong)right).Value;
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
            if (right is BigInteger) {
                BigInteger leftBigInteger = (BigInteger)leftUInt32;
                return leftBigInteger | (BigInteger)right;
            } else if (right is ExtensibleInt) {
                Int64 leftInt64 = (Int64)leftUInt32;
                Int64 rightInt64 = (Int64)((ExtensibleInt)right).value;
                return leftInt64 | rightInt64;
            } else if (right is ExtensibleLong) {
                BigInteger leftBigInteger = (BigInteger)leftUInt32;
                return leftBigInteger | (BigInteger)((ExtensibleLong)right).Value;
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
            if (right is BigInteger) {
                BigInteger leftBigInteger = (BigInteger)leftUInt32;
                return leftBigInteger | (BigInteger)right;
            } else if (right is ExtensibleInt) {
                Int64 leftInt64 = (Int64)leftUInt32;
                Int64 rightInt64 = (Int64)((ExtensibleInt)right).value;
                return leftInt64 | rightInt64;
            } else if (right is ExtensibleLong) {
                BigInteger leftBigInteger = (BigInteger)leftUInt32;
                return leftBigInteger | (BigInteger)((ExtensibleLong)right).Value;
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
            if (right is BigInteger) {
                BigInteger leftBigInteger = (BigInteger)leftUInt32;
                return leftBigInteger ^ (BigInteger)right;
            } else if (right is ExtensibleInt) {
                Int64 leftInt64 = (Int64)leftUInt32;
                Int64 rightInt64 = (Int64)((ExtensibleInt)right).value;
                return leftInt64 ^ rightInt64;
            } else if (right is ExtensibleLong) {
                BigInteger leftBigInteger = (BigInteger)leftUInt32;
                return leftBigInteger ^ (BigInteger)((ExtensibleLong)right).Value;
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
            if (right is BigInteger) {
                BigInteger leftBigInteger = (BigInteger)leftUInt32;
                return leftBigInteger ^ (BigInteger)right;
            } else if (right is ExtensibleInt) {
                Int64 leftInt64 = (Int64)leftUInt32;
                Int64 rightInt64 = (Int64)((ExtensibleInt)right).value;
                return leftInt64 ^ rightInt64;
            } else if (right is ExtensibleLong) {
                BigInteger leftBigInteger = (BigInteger)leftUInt32;
                return leftBigInteger ^ (BigInteger)((ExtensibleLong)right).Value;
            }
            return Ops.NotImplemented;
        }
        [PythonName("__divmod__")]
        public static object DivMod(object left, object right) {
            Debug.Assert(left is UInt32);
            UInt32 leftUInt32 = (UInt32)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte:
                        return UInt32Ops.DivModImpl(leftUInt32, (Byte)right);
                    case TypeCode.SByte:
                        return Int64Ops.DivMod(leftUInt32, (SByte)right);
                    case TypeCode.Int16:
                        return Int64Ops.DivMod(leftUInt32, (Int16)right);
                    case TypeCode.UInt16:
                        return UInt32Ops.DivModImpl(leftUInt32, (UInt16)right);
                    case TypeCode.Int32:
                        return Int64Ops.DivMod(leftUInt32, (Int32)right);
                    case TypeCode.UInt32:
                        return UInt32Ops.DivModImpl(leftUInt32, (UInt32)right);
                    case TypeCode.Int64:
                        return Int64Ops.DivMod(leftUInt32, (Int64)right);
                    case TypeCode.UInt64:
                        return UInt64Ops.DivModImpl(leftUInt32, (UInt64)right);
                    case TypeCode.Single:
                        return SingleOps.DivModImpl(leftUInt32, (Single)right);
                    case TypeCode.Double:
                        return FloatOps.DivMod(leftUInt32, (Double)right);
                }
            }
            if (right is BigInteger) {
                return LongOps.DivMod(leftUInt32, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return Int64Ops.DivMod(leftUInt32, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.DivMod(leftUInt32, ((ExtensibleLong)right).Value);
            } else if (right is Complex64) {
                return ComplexOps.DivMod(leftUInt32, (Complex64)right);
            } else if (right is ExtensibleFloat) {
                return FloatOps.DivMod(leftUInt32, ((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return ComplexOps.DivMod(leftUInt32, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rdivmod__")]
        public static object ReverseDivMod(object left, object right) {
            Debug.Assert(left is UInt32);
            UInt32 leftUInt32 = (UInt32)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte:
                        return UInt32Ops.ReverseDivModImpl(leftUInt32, (Byte)right);
                    case TypeCode.SByte:
                        return Int64Ops.ReverseDivMod(leftUInt32, (SByte)right);
                    case TypeCode.Int16:
                        return Int64Ops.ReverseDivMod(leftUInt32, (Int16)right);
                    case TypeCode.UInt16:
                        return UInt32Ops.ReverseDivModImpl(leftUInt32, (UInt16)right);
                    case TypeCode.Int32:
                        return Int64Ops.ReverseDivMod(leftUInt32, (Int32)right);
                    case TypeCode.UInt32:
                        return UInt32Ops.ReverseDivModImpl(leftUInt32, (UInt32)right);
                    case TypeCode.Int64:
                        return Int64Ops.ReverseDivMod(leftUInt32, (Int64)right);
                    case TypeCode.UInt64:
                        return UInt64Ops.ReverseDivModImpl(leftUInt32, (UInt64)right);
                    case TypeCode.Single:
                        return SingleOps.ReverseDivModImpl(leftUInt32, (Single)right);
                    case TypeCode.Double:
                        return FloatOps.ReverseDivMod(leftUInt32, (Double)right);
                }
            }
            if (right is BigInteger) {
                return LongOps.ReverseDivMod(leftUInt32, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return Int64Ops.ReverseDivMod(leftUInt32, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.ReverseDivMod(leftUInt32, ((ExtensibleLong)right).Value);
            } else if (right is Complex64) {
                return ComplexOps.ReverseDivMod(leftUInt32, (Complex64)right);
            } else if (right is ExtensibleFloat) {
                return FloatOps.ReverseDivMod(leftUInt32, ((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return ComplexOps.ReverseDivMod(leftUInt32, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__lshift__")]
        public static object LeftShift(object left, object right) {
            Debug.Assert(left is UInt32);
            UInt32 leftUInt32 = (UInt32)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte:
                        return UInt32Ops.LeftShiftImpl(leftUInt32, (Byte)right);
                    case TypeCode.SByte:
                        return Int64Ops.LeftShift(leftUInt32, (SByte)right);
                    case TypeCode.Int16:
                        return Int64Ops.LeftShift(leftUInt32, (Int16)right);
                    case TypeCode.UInt16:
                        return UInt32Ops.LeftShiftImpl(leftUInt32, (UInt16)right);
                    case TypeCode.Int32:
                        return Int64Ops.LeftShift(leftUInt32, (Int32)right);
                    case TypeCode.UInt32:
                        return UInt32Ops.LeftShiftImpl(leftUInt32, (UInt32)right);
                    case TypeCode.Int64:
                        return Int64Ops.LeftShift(leftUInt32, (Int64)right);
                    case TypeCode.UInt64:
                        return UInt64Ops.LeftShiftImpl(leftUInt32, (UInt64)right);
                }
            }
            if (right is BigInteger) {
                return LongOps.LeftShift(leftUInt32, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return Int64Ops.LeftShift(leftUInt32, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.LeftShift(leftUInt32, ((ExtensibleLong)right).Value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rlshift__")]
        public static object ReverseLeftShift(object left, object right) {
            Debug.Assert(left is UInt32);
            UInt32 leftUInt32 = (UInt32)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte:
                        return UInt32Ops.ReverseLeftShiftImpl(leftUInt32, (Byte)right);
                    case TypeCode.SByte:
                        return Int64Ops.ReverseLeftShift(leftUInt32, (SByte)right);
                    case TypeCode.Int16:
                        return Int64Ops.ReverseLeftShift(leftUInt32, (Int16)right);
                    case TypeCode.UInt16:
                        return UInt32Ops.ReverseLeftShiftImpl(leftUInt32, (UInt16)right);
                    case TypeCode.Int32:
                        return Int64Ops.ReverseLeftShift(leftUInt32, (Int32)right);
                    case TypeCode.UInt32:
                        return UInt32Ops.ReverseLeftShiftImpl(leftUInt32, (UInt32)right);
                    case TypeCode.Int64:
                        return Int64Ops.ReverseLeftShift(leftUInt32, (Int64)right);
                    case TypeCode.UInt64:
                        return UInt64Ops.ReverseLeftShiftImpl(leftUInt32, (UInt64)right);
                }
            }
            if (right is BigInteger) {
                return LongOps.ReverseLeftShift(leftUInt32, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return Int64Ops.ReverseLeftShift(leftUInt32, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.ReverseLeftShift(leftUInt32, ((ExtensibleLong)right).Value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__pow__")]
        public static object Power(object left, object right) {
            Debug.Assert(left is UInt32);
            UInt32 leftUInt32 = (UInt32)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte:
                        return UInt32Ops.PowerImpl(leftUInt32, (Byte)right);
                    case TypeCode.SByte:
                        return Int64Ops.Power(leftUInt32, (SByte)right);
                    case TypeCode.Int16:
                        return Int64Ops.Power(leftUInt32, (Int16)right);
                    case TypeCode.UInt16:
                        return UInt32Ops.PowerImpl(leftUInt32, (UInt16)right);
                    case TypeCode.Int32:
                        return Int64Ops.Power(leftUInt32, (Int32)right);
                    case TypeCode.UInt32:
                        return UInt32Ops.PowerImpl(leftUInt32, (UInt32)right);
                    case TypeCode.Int64:
                        return Int64Ops.Power(leftUInt32, (Int64)right);
                    case TypeCode.UInt64:
                        return UInt64Ops.PowerImpl(leftUInt32, (UInt64)right);
                    case TypeCode.Single:
                        return SingleOps.PowerImpl(leftUInt32, (Single)right);
                    case TypeCode.Double:
                        return FloatOps.Power(leftUInt32, (Double)right);
                }
            }
            if (right is BigInteger) {
                return LongOps.Power(leftUInt32, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return Int64Ops.Power(leftUInt32, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.Power(leftUInt32, ((ExtensibleLong)right).Value);
            } else if (right is Complex64) {
                return ComplexOps.Power(leftUInt32, (Complex64)right);
            } else if (right is ExtensibleFloat) {
                return FloatOps.Power(leftUInt32, ((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return ComplexOps.Power(leftUInt32, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rpow__")]
        public static object ReversePower(object left, object right) {
            Debug.Assert(left is UInt32);
            UInt32 leftUInt32 = (UInt32)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte:
                        return UInt32Ops.ReversePowerImpl(leftUInt32, (Byte)right);
                    case TypeCode.SByte:
                        return Int64Ops.ReversePower(leftUInt32, (SByte)right);
                    case TypeCode.Int16:
                        return Int64Ops.ReversePower(leftUInt32, (Int16)right);
                    case TypeCode.UInt16:
                        return UInt32Ops.ReversePowerImpl(leftUInt32, (UInt16)right);
                    case TypeCode.Int32:
                        return Int64Ops.ReversePower(leftUInt32, (Int32)right);
                    case TypeCode.UInt32:
                        return UInt32Ops.ReversePowerImpl(leftUInt32, (UInt32)right);
                    case TypeCode.Int64:
                        return Int64Ops.ReversePower(leftUInt32, (Int64)right);
                    case TypeCode.UInt64:
                        return UInt64Ops.ReversePowerImpl(leftUInt32, (UInt64)right);
                    case TypeCode.Single:
                        return SingleOps.ReversePowerImpl(leftUInt32, (Single)right);
                    case TypeCode.Double:
                        return FloatOps.ReversePower(leftUInt32, (Double)right);
                }
            }
            if (right is BigInteger) {
                return LongOps.ReversePower(leftUInt32, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return Int64Ops.ReversePower(leftUInt32, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.ReversePower(leftUInt32, ((ExtensibleLong)right).Value);
            } else if (right is Complex64) {
                return ComplexOps.ReversePower(leftUInt32, (Complex64)right);
            } else if (right is ExtensibleFloat) {
                return FloatOps.ReversePower(leftUInt32, ((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return ComplexOps.ReversePower(leftUInt32, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rshift__")]
        public static object RightShift(object left, object right) {
            Debug.Assert(left is UInt32);
            UInt32 leftUInt32 = (UInt32)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte:
                        return UInt32Ops.RightShiftImpl(leftUInt32, (Byte)right);
                    case TypeCode.SByte:
                        return Int64Ops.RightShift(leftUInt32, (SByte)right);
                    case TypeCode.Int16:
                        return Int64Ops.RightShift(leftUInt32, (Int16)right);
                    case TypeCode.UInt16:
                        return UInt32Ops.RightShiftImpl(leftUInt32, (UInt16)right);
                    case TypeCode.Int32:
                        return Int64Ops.RightShift(leftUInt32, (Int32)right);
                    case TypeCode.UInt32:
                        return UInt32Ops.RightShiftImpl(leftUInt32, (UInt32)right);
                    case TypeCode.Int64:
                        return Int64Ops.RightShift(leftUInt32, (Int64)right);
                    case TypeCode.UInt64:
                        return UInt64Ops.RightShiftImpl(leftUInt32, (UInt64)right);
                }
            }
            if (right is BigInteger) {
                return LongOps.RightShift(leftUInt32, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return Int64Ops.RightShift(leftUInt32, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.RightShift(leftUInt32, ((ExtensibleLong)right).Value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rrshift__")]
        public static object ReverseRightShift(object left, object right) {
            Debug.Assert(left is UInt32);
            UInt32 leftUInt32 = (UInt32)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte:
                        return UInt32Ops.ReverseRightShiftImpl(leftUInt32, (Byte)right);
                    case TypeCode.SByte:
                        return Int64Ops.ReverseRightShift(leftUInt32, (SByte)right);
                    case TypeCode.Int16:
                        return Int64Ops.ReverseRightShift(leftUInt32, (Int16)right);
                    case TypeCode.UInt16:
                        return UInt32Ops.ReverseRightShiftImpl(leftUInt32, (UInt16)right);
                    case TypeCode.Int32:
                        return Int64Ops.ReverseRightShift(leftUInt32, (Int32)right);
                    case TypeCode.UInt32:
                        return UInt32Ops.ReverseRightShiftImpl(leftUInt32, (UInt32)right);
                    case TypeCode.Int64:
                        return Int64Ops.ReverseRightShift(leftUInt32, (Int64)right);
                    case TypeCode.UInt64:
                        return UInt64Ops.ReverseRightShiftImpl(leftUInt32, (UInt64)right);
                }
            }
            if (right is BigInteger) {
                return LongOps.ReverseRightShift(leftUInt32, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return Int64Ops.ReverseRightShift(leftUInt32, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.ReverseRightShift(leftUInt32, ((ExtensibleLong)right).Value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__truediv__")]
        public static object TrueDivide(object left, object right) {
            Debug.Assert(left is UInt32);
            UInt32 leftUInt32 = (UInt32)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte:
                        return FloatOps.TrueDivide(leftUInt32, (Byte)right);
                    case TypeCode.SByte:
                        return FloatOps.TrueDivide(leftUInt32, (SByte)right);
                    case TypeCode.Int16:
                        return FloatOps.TrueDivide(leftUInt32, (Int16)right);
                    case TypeCode.UInt16:
                        return FloatOps.TrueDivide(leftUInt32, (UInt16)right);
                    case TypeCode.Int32:
                        return FloatOps.TrueDivide(leftUInt32, (Int32)right);
                    case TypeCode.UInt32:
                        return FloatOps.TrueDivide(leftUInt32, (UInt32)right);
                    case TypeCode.Int64:
                        return FloatOps.TrueDivide(leftUInt32, (Int64)right);
                    case TypeCode.UInt64:
                        return FloatOps.TrueDivide(leftUInt32, (UInt64)right);
                    case TypeCode.Single:
                        return FloatOps.TrueDivide(leftUInt32, (Single)right);
                    case TypeCode.Double:
                        return FloatOps.TrueDivide(leftUInt32, (Double)right);
                }
            }
            if (right is BigInteger) {
                return FloatOps.TrueDivide(leftUInt32, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return FloatOps.TrueDivide(leftUInt32, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return FloatOps.TrueDivide(leftUInt32, ((ExtensibleLong)right).Value);
            } else if (right is Complex64) {
                return FloatOps.TrueDivide(leftUInt32, (Complex64)right);
            } else if (right is ExtensibleFloat) {
                return FloatOps.TrueDivide(leftUInt32, ((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return FloatOps.TrueDivide(leftUInt32, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rtruediv__")]
        public static object ReverseTrueDivide(object left, object right) {
            Debug.Assert(left is UInt32);
            UInt32 leftUInt32 = (UInt32)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte:
                        return FloatOps.ReverseTrueDivide(leftUInt32, (Byte)right);
                    case TypeCode.SByte:
                        return FloatOps.ReverseTrueDivide(leftUInt32, (SByte)right);
                    case TypeCode.Int16:
                        return FloatOps.ReverseTrueDivide(leftUInt32, (Int16)right);
                    case TypeCode.UInt16:
                        return FloatOps.ReverseTrueDivide(leftUInt32, (UInt16)right);
                    case TypeCode.Int32:
                        return FloatOps.ReverseTrueDivide(leftUInt32, (Int32)right);
                    case TypeCode.UInt32:
                        return FloatOps.ReverseTrueDivide(leftUInt32, (UInt32)right);
                    case TypeCode.Int64:
                        return FloatOps.ReverseTrueDivide(leftUInt32, (Int64)right);
                    case TypeCode.UInt64:
                        return FloatOps.ReverseTrueDivide(leftUInt32, (UInt64)right);
                    case TypeCode.Single:
                        return FloatOps.ReverseTrueDivide(leftUInt32, (Single)right);
                    case TypeCode.Double:
                        return FloatOps.ReverseTrueDivide(leftUInt32, (Double)right);
                }
            }
            if (right is BigInteger) {
                return FloatOps.ReverseTrueDivide(leftUInt32, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return FloatOps.ReverseTrueDivide(leftUInt32, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return FloatOps.ReverseTrueDivide(leftUInt32, ((ExtensibleLong)right).Value);
            } else if (right is Complex64) {
                return FloatOps.ReverseTrueDivide(leftUInt32, (Complex64)right);
            } else if (right is ExtensibleFloat) {
                return FloatOps.ReverseTrueDivide(leftUInt32, ((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return FloatOps.ReverseTrueDivide(leftUInt32, ((ExtensibleComplex)right).value);
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


        internal static object DivModImpl(UInt32 x, UInt32 y) {
            object div = DivideImpl(x, y);
            if (div == Ops.NotImplemented) return div;
            object mod = ModImpl(x, y);
            if (mod == Ops.NotImplemented) return mod;
            return Tuple.MakeTuple(div, mod);
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
        internal static object ReverseDivModImpl(UInt32 x, UInt32 y) {
            return DivModImpl(y, x);
        }
        internal static object FloorDivideImpl(UInt32 x, UInt32 y) {
            return DivideImpl(x, y);
        }
        internal static object ReverseFloorDivideImpl(UInt32 x, UInt32 y) {
            return DivideImpl(y, x);
        }
        internal static object ReverseLeftShiftImpl(UInt32 x, UInt32 y) {
            return LeftShiftImpl(y, x);
        }
        internal static object ReversePowerImpl(UInt32 x, UInt32 y) {
            return PowerImpl(y, x);
        }
        internal static object ReverseRightShiftImpl(UInt32 x, UInt32 y) {
            return RightShiftImpl(y, x);
        }


        // *** END GENERATED CODE ***

        #endregion
    }
}