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
    static partial class SingleOps {
        #region Generated SingleOps

        // *** BEGIN GENERATED CODE ***

        private static ReflectedType SingleType;
        public static DynamicType MakeDynamicType() {
            if (SingleType == null) {
                OpsReflectedType ort = new OpsReflectedType("Single", typeof(Single), typeof(SingleOps), null);
                if (Interlocked.CompareExchange<ReflectedType>(ref SingleType, ort, null) == null) {
                    return ort;
                }
            }
            return SingleType;
        }

        [PythonName("__new__")]
        public static object Make(PythonType cls, object value) {
            if (cls != SingleType) {
                throw Ops.TypeError("Single.__new__: first argument must be Single type.");
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
            } else if (value is ExtensibleInt) {
                return (Single)((ExtensibleInt)value).value;
            } else if (value is ExtensibleLong) {
                return (Single)((ExtensibleLong)value).Value;
            } else if (value is ExtensibleFloat) {
                return (Single)((ExtensibleFloat)value).value;
            }
            throw Ops.ValueError("invalid value for Single.__new__");
        }

        [PythonName("__add__")]
        public static object Add(object left, object right) {
            Debug.Assert(left is Single);
            Single leftSingle = (Single)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean: {
                            return (Single)(((Single)leftSingle) + ((Single)((Boolean)right ? (Single)1 : (Single)0)));
                        }
                    case TypeCode.Byte: {
                            return (Single)(((Single)leftSingle) + ((Single)((Byte)right)));
                        }
                    case TypeCode.SByte: {
                            return (Single)(((Single)leftSingle) + ((Single)((SByte)right)));
                        }
                    case TypeCode.Int16: {
                            return (Single)(((Single)leftSingle) + ((Single)((Int16)right)));
                        }
                    case TypeCode.UInt16: {
                            return (Single)(((Single)leftSingle) + ((Single)((UInt16)right)));
                        }
                    case TypeCode.Int32: {
                            return (Single)(((Single)leftSingle) + ((Single)((Int32)right)));
                        }
                    case TypeCode.UInt32: {
                            return (Single)(((Single)leftSingle) + ((Single)((UInt32)right)));
                        }
                    case TypeCode.Int64: {
                            return (Single)(((Single)leftSingle) + ((Single)((Int64)right)));
                        }
                    case TypeCode.UInt64: {
                            return (Single)(((Single)leftSingle) + ((Single)((UInt64)right)));
                        }
                    case TypeCode.Single: {
                            return (Double)(((Double)leftSingle) + ((Double)((Single)right)));
                        }
                    case TypeCode.Double: {
                            return (Double)(((Double)leftSingle) + ((Double)((Double)right)));
                        }
                }
            }
            if (right is BigInteger) {
                return (Double)(((Double)leftSingle) + ((Double)(BigInteger)right));
            } else if (right is Complex64) {
                return ComplexOps.Add(leftSingle, (Complex64)right);
            } else if (right is ExtensibleInt) {
                return (Single)(((Single)leftSingle) + ((Single)((ExtensibleInt)right).value));
            } else if (right is ExtensibleLong) {
                return (Double)(((Double)leftSingle) + ((Double)((ExtensibleLong)right).Value));
            } else if (right is ExtensibleFloat) {
                return (Double)(((Double)leftSingle) + ((Double)((ExtensibleFloat)right).value));
            } else if (right is ExtensibleComplex) {
                return ComplexOps.Add(leftSingle, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__div__")]
        public static object Divide(object left, object right) {
            Debug.Assert(left is Single);
            Single leftSingle = (Single)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean: {
                            return SingleOps.DivideImpl((Single)leftSingle, (Single)((Boolean)right ? (Single)1 : (Single)0));
                        }
                    case TypeCode.Byte: {
                            return SingleOps.DivideImpl((Single)leftSingle, (Single)((Byte)right));
                        }
                    case TypeCode.SByte: {
                            return SingleOps.DivideImpl((Single)leftSingle, (Single)((SByte)right));
                        }
                    case TypeCode.Int16: {
                            return SingleOps.DivideImpl((Single)leftSingle, (Single)((Int16)right));
                        }
                    case TypeCode.UInt16: {
                            return SingleOps.DivideImpl((Single)leftSingle, (Single)((UInt16)right));
                        }
                    case TypeCode.Int32: {
                            return SingleOps.DivideImpl((Single)leftSingle, (Single)((Int32)right));
                        }
                    case TypeCode.UInt32: {
                            return SingleOps.DivideImpl((Single)leftSingle, (Single)((UInt32)right));
                        }
                    case TypeCode.Int64: {
                            return SingleOps.DivideImpl((Single)leftSingle, (Single)((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return SingleOps.DivideImpl((Single)leftSingle, (Single)((UInt64)right));
                        }
                    case TypeCode.Single: {
                            return SingleOps.DivideImpl((Single)leftSingle, (Single)((Single)right));
                        }
                    case TypeCode.Double: {
                            return FloatOps.Divide((Double)leftSingle, (Double)((Double)right));
                        }
                }
            }
            if (right is BigInteger) {
                return FloatOps.Divide((Double)leftSingle, (Double)(BigInteger)right);
            } else if (right is Complex64) {
                return FloatOps.Divide(leftSingle, (Complex64)right);
            } else if (right is ExtensibleInt) {
                return SingleOps.DivideImpl((Single)leftSingle, (Single)((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return FloatOps.Divide((Double)leftSingle, (Double)((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return FloatOps.Divide((Double)leftSingle, (Double)((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return FloatOps.Divide(leftSingle, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__floordiv__")]
        public static object FloorDivide(object left, object right) {
            Debug.Assert(left is Single);
            Single leftSingle = (Single)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean: {
                            return SingleOps.FloorDivideImpl((Single)leftSingle, (Single)((Boolean)right ? (Single)1 : (Single)0));
                        }
                    case TypeCode.Byte: {
                            return SingleOps.FloorDivideImpl((Single)leftSingle, (Single)((Byte)right));
                        }
                    case TypeCode.SByte: {
                            return SingleOps.FloorDivideImpl((Single)leftSingle, (Single)((SByte)right));
                        }
                    case TypeCode.Int16: {
                            return SingleOps.FloorDivideImpl((Single)leftSingle, (Single)((Int16)right));
                        }
                    case TypeCode.UInt16: {
                            return SingleOps.FloorDivideImpl((Single)leftSingle, (Single)((UInt16)right));
                        }
                    case TypeCode.Int32: {
                            return SingleOps.FloorDivideImpl((Single)leftSingle, (Single)((Int32)right));
                        }
                    case TypeCode.UInt32: {
                            return SingleOps.FloorDivideImpl((Single)leftSingle, (Single)((UInt32)right));
                        }
                    case TypeCode.Int64: {
                            return SingleOps.FloorDivideImpl((Single)leftSingle, (Single)((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return SingleOps.FloorDivideImpl((Single)leftSingle, (Single)((UInt64)right));
                        }
                    case TypeCode.Single: {
                            return SingleOps.FloorDivideImpl((Single)leftSingle, (Single)((Single)right));
                        }
                    case TypeCode.Double: {
                            return FloatOps.FloorDivide((Double)leftSingle, (Double)((Double)right));
                        }
                }
            }
            if (right is BigInteger) {
                return FloatOps.FloorDivide((Double)leftSingle, (Double)(BigInteger)right);
            } else if (right is Complex64) {
                return FloatOps.FloorDivide(leftSingle, (Complex64)right);
            } else if (right is ExtensibleInt) {
                return SingleOps.FloorDivideImpl((Single)leftSingle, (Single)((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return FloatOps.FloorDivide((Double)leftSingle, (Double)((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return FloatOps.FloorDivide((Double)leftSingle, (Double)((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return FloatOps.FloorDivide(leftSingle, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__mod__")]
        public static object Mod(object left, object right) {
            Debug.Assert(left is Single);
            Single leftSingle = (Single)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean: {
                            return SingleOps.ModImpl((Single)leftSingle, (Single)((Boolean)right ? (Single)1 : (Single)0));
                        }
                    case TypeCode.Byte: {
                            return SingleOps.ModImpl((Single)leftSingle, (Single)((Byte)right));
                        }
                    case TypeCode.SByte: {
                            return SingleOps.ModImpl((Single)leftSingle, (Single)((SByte)right));
                        }
                    case TypeCode.Int16: {
                            return SingleOps.ModImpl((Single)leftSingle, (Single)((Int16)right));
                        }
                    case TypeCode.UInt16: {
                            return SingleOps.ModImpl((Single)leftSingle, (Single)((UInt16)right));
                        }
                    case TypeCode.Int32: {
                            return SingleOps.ModImpl((Single)leftSingle, (Single)((Int32)right));
                        }
                    case TypeCode.UInt32: {
                            return SingleOps.ModImpl((Single)leftSingle, (Single)((UInt32)right));
                        }
                    case TypeCode.Int64: {
                            return SingleOps.ModImpl((Single)leftSingle, (Single)((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return SingleOps.ModImpl((Single)leftSingle, (Single)((UInt64)right));
                        }
                    case TypeCode.Single: {
                            return SingleOps.ModImpl((Single)leftSingle, (Single)((Single)right));
                        }
                    case TypeCode.Double: {
                            return FloatOps.Mod((Double)leftSingle, (Double)((Double)right));
                        }
                }
            }
            if (right is BigInteger) {
                return FloatOps.Mod((Double)leftSingle, (Double)(BigInteger)right);
            } else if (right is Complex64) {
                return FloatOps.Mod(leftSingle, (Complex64)right);
            } else if (right is ExtensibleInt) {
                return SingleOps.ModImpl((Single)leftSingle, (Single)((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return FloatOps.Mod((Double)leftSingle, (Double)((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return FloatOps.Mod((Double)leftSingle, (Double)((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return FloatOps.Mod(leftSingle, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__mul__")]
        public static object Multiply(object left, object right) {
            Debug.Assert(left is Single);
            Single leftSingle = (Single)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean: {
                            return (Single)(((Single)leftSingle) * ((Single)((Boolean)right ? (Single)1 : (Single)0)));
                        }
                    case TypeCode.Byte: {
                            return (Double)(((Double)leftSingle) * ((Double)((Byte)right)));
                        }
                    case TypeCode.SByte: {
                            return (Double)(((Double)leftSingle) * ((Double)((SByte)right)));
                        }
                    case TypeCode.Int16: {
                            return (Double)(((Double)leftSingle) * ((Double)((Int16)right)));
                        }
                    case TypeCode.UInt16: {
                            return (Double)(((Double)leftSingle) * ((Double)((UInt16)right)));
                        }
                    case TypeCode.Int32: {
                            return (Double)(((Double)leftSingle) * ((Double)((Int32)right)));
                        }
                    case TypeCode.UInt32: {
                            return (Double)(((Double)leftSingle) * ((Double)((UInt32)right)));
                        }
                    case TypeCode.Int64: {
                            return (Double)(((Double)leftSingle) * ((Double)((Int64)right)));
                        }
                    case TypeCode.UInt64: {
                            return (Double)(((Double)leftSingle) * ((Double)((UInt64)right)));
                        }
                    case TypeCode.Single: {
                            return (Double)(((Double)leftSingle) * ((Double)((Single)right)));
                        }
                    case TypeCode.Double: {
                            return FloatOps.Multiply(leftSingle, ((Double)right));
                        }
                }
            }
            if (right is BigInteger) {
                return (Double)(((Double)leftSingle) * ((Double)(BigInteger)right));
            } else if (right is Complex64) {
                return ComplexOps.Multiply(leftSingle, (Complex64)right);
            } else if (right is ExtensibleInt) {
                return (Double)(((Double)leftSingle) * ((Double)((ExtensibleInt)right).value));
            } else if (right is ExtensibleLong) {
                return (Double)(((Double)leftSingle) * ((Double)((ExtensibleLong)right).Value));
            } else if (right is ExtensibleFloat) {
                return FloatOps.Multiply(leftSingle, ((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return ComplexOps.Multiply(leftSingle, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__sub__")]
        public static object Subtract(object left, object right) {
            Debug.Assert(left is Single);
            Single leftSingle = (Single)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean: {
                            return (Single)(((Single)leftSingle) - ((Single)((Boolean)right ? (Single)1 : (Single)0)));
                        }
                    case TypeCode.Byte: {
                            return (Single)(((Single)leftSingle) - ((Single)((Byte)right)));
                        }
                    case TypeCode.SByte: {
                            return (Single)(((Single)leftSingle) - ((Single)((SByte)right)));
                        }
                    case TypeCode.Int16: {
                            return (Single)(((Single)leftSingle) - ((Single)((Int16)right)));
                        }
                    case TypeCode.UInt16: {
                            return (Single)(((Single)leftSingle) - ((Single)((UInt16)right)));
                        }
                    case TypeCode.Int32: {
                            return (Single)(((Single)leftSingle) - ((Single)((Int32)right)));
                        }
                    case TypeCode.UInt32: {
                            return (Single)(((Single)leftSingle) - ((Single)((UInt32)right)));
                        }
                    case TypeCode.Int64: {
                            return (Single)(((Single)leftSingle) - ((Single)((Int64)right)));
                        }
                    case TypeCode.UInt64: {
                            return (Single)(((Single)leftSingle) - ((Single)((UInt64)right)));
                        }
                    case TypeCode.Single: {
                            return (Double)(((Double)leftSingle) - ((Double)((Single)right)));
                        }
                    case TypeCode.Double: {
                            return (Double)(((Double)leftSingle) - ((Double)((Double)right)));
                        }
                }
            }
            if (right is BigInteger) {
                return (Double)(((Double)leftSingle) - ((Double)(BigInteger)right));
            } else if (right is Complex64) {
                return ComplexOps.Subtract(leftSingle, (Complex64)right);
            } else if (right is ExtensibleInt) {
                return (Single)(((Single)leftSingle) - ((Single)((ExtensibleInt)right).value));
            } else if (right is ExtensibleLong) {
                return (Double)(((Double)leftSingle) - ((Double)((ExtensibleLong)right).Value));
            } else if (right is ExtensibleFloat) {
                return (Double)(((Double)leftSingle) - ((Double)((ExtensibleFloat)right).value));
            } else if (right is ExtensibleComplex) {
                return ComplexOps.Subtract(leftSingle, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__radd__")]
        public static object ReverseAdd(object left, object right) {
            Debug.Assert(left is Single);
            Single leftSingle = (Single)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean: {
                            return (Single)(((Single)((Boolean)right ? (Single)1 : (Single)0)) + ((Single)leftSingle));
                        }
                    case TypeCode.Byte: {
                            return (Single)(((Single)((Byte)right)) + ((Single)leftSingle));
                        }
                    case TypeCode.SByte: {
                            return (Single)(((Single)((SByte)right)) + ((Single)leftSingle));
                        }
                    case TypeCode.Int16: {
                            return (Single)(((Single)((Int16)right)) + ((Single)leftSingle));
                        }
                    case TypeCode.UInt16: {
                            return (Single)(((Single)((UInt16)right)) + ((Single)leftSingle));
                        }
                    case TypeCode.Int32: {
                            return (Single)(((Single)((Int32)right)) + ((Single)leftSingle));
                        }
                    case TypeCode.UInt32: {
                            return (Single)(((Single)((UInt32)right)) + ((Single)leftSingle));
                        }
                    case TypeCode.Int64: {
                            return (Single)(((Single)((Int64)right)) + ((Single)leftSingle));
                        }
                    case TypeCode.UInt64: {
                            return (Single)(((Single)((UInt64)right)) + ((Single)leftSingle));
                        }
                    case TypeCode.Single: {
                            return (Double)(((Double)((Single)right)) + ((Double)leftSingle));
                        }
                    case TypeCode.Double: {
                            return (Double)(((Double)((Double)right)) + ((Double)leftSingle));
                        }
                }
            }
            if (right is BigInteger) {
                return (Double)(((Double)(BigInteger)right) + ((Double)leftSingle));
            } else if (right is Complex64) {
                return ComplexOps.ReverseAdd(leftSingle, (Complex64)right);
            } else if (right is ExtensibleInt) {
                return (Single)(((Single)((ExtensibleInt)right).value) + ((Single)leftSingle));
            } else if (right is ExtensibleLong) {
                return (Double)(((Double)((ExtensibleLong)right).Value) + ((Double)leftSingle));
            } else if (right is ExtensibleFloat) {
                return (Double)(((Double)((ExtensibleFloat)right).value) + ((Double)leftSingle));
            } else if (right is ExtensibleComplex) {
                return ComplexOps.ReverseAdd(leftSingle, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rdiv__")]
        public static object ReverseDivide(object left, object right) {
            Debug.Assert(left is Single);
            Single leftSingle = (Single)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean: {
                            return SingleOps.ReverseDivideImpl((Single)leftSingle, (Single)((Boolean)right ? (Single)1 : (Single)0));
                        }
                    case TypeCode.Byte: {
                            return SingleOps.ReverseDivideImpl((Single)leftSingle, (Single)((Byte)right));
                        }
                    case TypeCode.SByte: {
                            return SingleOps.ReverseDivideImpl((Single)leftSingle, (Single)((SByte)right));
                        }
                    case TypeCode.Int16: {
                            return SingleOps.ReverseDivideImpl((Single)leftSingle, (Single)((Int16)right));
                        }
                    case TypeCode.UInt16: {
                            return SingleOps.ReverseDivideImpl((Single)leftSingle, (Single)((UInt16)right));
                        }
                    case TypeCode.Int32: {
                            return SingleOps.ReverseDivideImpl((Single)leftSingle, (Single)((Int32)right));
                        }
                    case TypeCode.UInt32: {
                            return SingleOps.ReverseDivideImpl((Single)leftSingle, (Single)((UInt32)right));
                        }
                    case TypeCode.Int64: {
                            return SingleOps.ReverseDivideImpl((Single)leftSingle, (Single)((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return SingleOps.ReverseDivideImpl((Single)leftSingle, (Single)((UInt64)right));
                        }
                    case TypeCode.Single: {
                            return SingleOps.ReverseDivideImpl((Single)leftSingle, (Single)((Single)right));
                        }
                    case TypeCode.Double: {
                            return FloatOps.ReverseDivide((Double)leftSingle, (Double)((Double)right));
                        }
                }
            }
            if (right is BigInteger) {
                return FloatOps.ReverseDivide((Double)leftSingle, (Double)(BigInteger)right);
            } else if (right is Complex64) {
                return FloatOps.ReverseDivide(leftSingle, (Complex64)right);
            } else if (right is ExtensibleInt) {
                return SingleOps.ReverseDivideImpl((Single)leftSingle, (Single)((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return FloatOps.ReverseDivide((Double)leftSingle, (Double)((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return FloatOps.ReverseDivide((Double)leftSingle, (Double)((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return FloatOps.ReverseDivide(leftSingle, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rfloordiv__")]
        public static object ReverseFloorDivide(object left, object right) {
            Debug.Assert(left is Single);
            Single leftSingle = (Single)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean: {
                            return SingleOps.ReverseFloorDivideImpl((Single)leftSingle, (Single)((Boolean)right ? (Single)1 : (Single)0));
                        }
                    case TypeCode.Byte: {
                            return SingleOps.ReverseFloorDivideImpl((Single)leftSingle, (Single)((Byte)right));
                        }
                    case TypeCode.SByte: {
                            return SingleOps.ReverseFloorDivideImpl((Single)leftSingle, (Single)((SByte)right));
                        }
                    case TypeCode.Int16: {
                            return SingleOps.ReverseFloorDivideImpl((Single)leftSingle, (Single)((Int16)right));
                        }
                    case TypeCode.UInt16: {
                            return SingleOps.ReverseFloorDivideImpl((Single)leftSingle, (Single)((UInt16)right));
                        }
                    case TypeCode.Int32: {
                            return SingleOps.ReverseFloorDivideImpl((Single)leftSingle, (Single)((Int32)right));
                        }
                    case TypeCode.UInt32: {
                            return SingleOps.ReverseFloorDivideImpl((Single)leftSingle, (Single)((UInt32)right));
                        }
                    case TypeCode.Int64: {
                            return SingleOps.ReverseFloorDivideImpl((Single)leftSingle, (Single)((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return SingleOps.ReverseFloorDivideImpl((Single)leftSingle, (Single)((UInt64)right));
                        }
                    case TypeCode.Single: {
                            return SingleOps.ReverseFloorDivideImpl((Single)leftSingle, (Single)((Single)right));
                        }
                    case TypeCode.Double: {
                            return FloatOps.ReverseFloorDivide((Double)leftSingle, (Double)((Double)right));
                        }
                }
            }
            if (right is BigInteger) {
                return FloatOps.ReverseFloorDivide((Double)leftSingle, (Double)(BigInteger)right);
            } else if (right is Complex64) {
                return FloatOps.ReverseFloorDivide(leftSingle, (Complex64)right);
            } else if (right is ExtensibleInt) {
                return SingleOps.ReverseFloorDivideImpl((Single)leftSingle, (Single)((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return FloatOps.ReverseFloorDivide((Double)leftSingle, (Double)((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return FloatOps.ReverseFloorDivide((Double)leftSingle, (Double)((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return FloatOps.ReverseFloorDivide(leftSingle, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rmod__")]
        public static object ReverseMod(object left, object right) {
            Debug.Assert(left is Single);
            Single leftSingle = (Single)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean: {
                            return SingleOps.ReverseModImpl((Single)leftSingle, (Single)((Boolean)right ? (Single)1 : (Single)0));
                        }
                    case TypeCode.Byte: {
                            return SingleOps.ReverseModImpl((Single)leftSingle, (Single)((Byte)right));
                        }
                    case TypeCode.SByte: {
                            return SingleOps.ReverseModImpl((Single)leftSingle, (Single)((SByte)right));
                        }
                    case TypeCode.Int16: {
                            return SingleOps.ReverseModImpl((Single)leftSingle, (Single)((Int16)right));
                        }
                    case TypeCode.UInt16: {
                            return SingleOps.ReverseModImpl((Single)leftSingle, (Single)((UInt16)right));
                        }
                    case TypeCode.Int32: {
                            return SingleOps.ReverseModImpl((Single)leftSingle, (Single)((Int32)right));
                        }
                    case TypeCode.UInt32: {
                            return SingleOps.ReverseModImpl((Single)leftSingle, (Single)((UInt32)right));
                        }
                    case TypeCode.Int64: {
                            return SingleOps.ReverseModImpl((Single)leftSingle, (Single)((Int64)right));
                        }
                    case TypeCode.UInt64: {
                            return SingleOps.ReverseModImpl((Single)leftSingle, (Single)((UInt64)right));
                        }
                    case TypeCode.Single: {
                            return SingleOps.ReverseModImpl((Single)leftSingle, (Single)((Single)right));
                        }
                    case TypeCode.Double: {
                            return FloatOps.ReverseMod((Double)leftSingle, (Double)((Double)right));
                        }
                }
            }
            if (right is BigInteger) {
                return FloatOps.ReverseMod((Double)leftSingle, (Double)(BigInteger)right);
            } else if (right is Complex64) {
                return FloatOps.ReverseMod(leftSingle, (Complex64)right);
            } else if (right is ExtensibleInt) {
                return SingleOps.ReverseModImpl((Single)leftSingle, (Single)((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return FloatOps.ReverseMod((Double)leftSingle, (Double)((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return FloatOps.ReverseMod((Double)leftSingle, (Double)((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return FloatOps.ReverseMod(leftSingle, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rmul__")]
        public static object ReverseMultiply(object left, object right) {
            Debug.Assert(left is Single);
            Single leftSingle = (Single)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean: {
                            return (Single)(((Single)((Boolean)right ? (Single)1 : (Single)0)) * ((Single)leftSingle));
                        }
                    case TypeCode.Byte: {
                            return (Double)(((Double)((Byte)right)) * ((Double)leftSingle));
                        }
                    case TypeCode.SByte: {
                            return (Double)(((Double)((SByte)right)) * ((Double)leftSingle));
                        }
                    case TypeCode.Int16: {
                            return (Double)(((Double)((Int16)right)) * ((Double)leftSingle));
                        }
                    case TypeCode.UInt16: {
                            return (Double)(((Double)((UInt16)right)) * ((Double)leftSingle));
                        }
                    case TypeCode.Int32: {
                            return (Double)(((Double)((Int32)right)) * ((Double)leftSingle));
                        }
                    case TypeCode.UInt32: {
                            return (Double)(((Double)((UInt32)right)) * ((Double)leftSingle));
                        }
                    case TypeCode.Int64: {
                            return (Double)(((Double)((Int64)right)) * ((Double)leftSingle));
                        }
                    case TypeCode.UInt64: {
                            return (Double)(((Double)((UInt64)right)) * ((Double)leftSingle));
                        }
                    case TypeCode.Single: {
                            return (Double)(((Double)((Single)right)) * ((Double)leftSingle));
                        }
                    case TypeCode.Double: {
                            return FloatOps.ReverseMultiply(leftSingle, ((Double)right));
                        }
                }
            }
            if (right is BigInteger) {
                return (Double)(((Double)(BigInteger)right) * ((Double)leftSingle));
            } else if (right is Complex64) {
                return ComplexOps.ReverseMultiply(leftSingle, (Complex64)right);
            } else if (right is ExtensibleInt) {
                return (Double)(((Double)((ExtensibleInt)right).value) * ((Double)leftSingle));
            } else if (right is ExtensibleLong) {
                return (Double)(((Double)((ExtensibleLong)right).Value) * ((Double)leftSingle));
            } else if (right is ExtensibleFloat) {
                return FloatOps.ReverseMultiply(leftSingle, ((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return ComplexOps.ReverseMultiply(leftSingle, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rsub__")]
        public static object ReverseSubtract(object left, object right) {
            Debug.Assert(left is Single);
            Single leftSingle = (Single)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean: {
                            return (Single)(((Single)((Boolean)right ? (Single)1 : (Single)0)) - ((Single)leftSingle));
                        }
                    case TypeCode.Byte: {
                            return (Single)(((Single)((Byte)right)) - ((Single)leftSingle));
                        }
                    case TypeCode.SByte: {
                            return (Single)(((Single)((SByte)right)) - ((Single)leftSingle));
                        }
                    case TypeCode.Int16: {
                            return (Single)(((Single)((Int16)right)) - ((Single)leftSingle));
                        }
                    case TypeCode.UInt16: {
                            return (Single)(((Single)((UInt16)right)) - ((Single)leftSingle));
                        }
                    case TypeCode.Int32: {
                            return (Single)(((Single)((Int32)right)) - ((Single)leftSingle));
                        }
                    case TypeCode.UInt32: {
                            return (Single)(((Single)((UInt32)right)) - ((Single)leftSingle));
                        }
                    case TypeCode.Int64: {
                            return (Single)(((Single)((Int64)right)) - ((Single)leftSingle));
                        }
                    case TypeCode.UInt64: {
                            return (Single)(((Single)((UInt64)right)) - ((Single)leftSingle));
                        }
                    case TypeCode.Single: {
                            return (Double)(((Double)((Single)right)) - ((Double)leftSingle));
                        }
                    case TypeCode.Double: {
                            return (Double)(((Double)((Double)right)) - ((Double)leftSingle));
                        }
                }
            }
            if (right is BigInteger) {
                return (Double)(((Double)(BigInteger)right) - ((Double)leftSingle));
            } else if (right is Complex64) {
                return ComplexOps.ReverseSubtract(leftSingle, (Complex64)right);
            } else if (right is ExtensibleInt) {
                return (Single)(((Single)((ExtensibleInt)right).value) - ((Single)leftSingle));
            } else if (right is ExtensibleLong) {
                return (Double)(((Double)((ExtensibleLong)right).Value) - ((Double)leftSingle));
            } else if (right is ExtensibleFloat) {
                return (Double)(((Double)((ExtensibleFloat)right).value) - ((Double)leftSingle));
            } else if (right is ExtensibleComplex) {
                return ComplexOps.ReverseSubtract(leftSingle, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__divmod__")]
        public static object DivMod(object left, object right) {
            Debug.Assert(left is Single);
            Single leftSingle = (Single)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean:
                        return SingleOps.DivModImpl(leftSingle, ((Boolean)right ? (Single)1 : (Single)0));
                    case TypeCode.Byte:
                        return SingleOps.DivModImpl(leftSingle, (Byte)right);
                    case TypeCode.SByte:
                        return SingleOps.DivModImpl(leftSingle, (SByte)right);
                    case TypeCode.Int16:
                        return SingleOps.DivModImpl(leftSingle, (Int16)right);
                    case TypeCode.UInt16:
                        return SingleOps.DivModImpl(leftSingle, (UInt16)right);
                    case TypeCode.Int32:
                        return SingleOps.DivModImpl(leftSingle, (Int32)right);
                    case TypeCode.UInt32:
                        return SingleOps.DivModImpl(leftSingle, (UInt32)right);
                    case TypeCode.Int64:
                        return SingleOps.DivModImpl(leftSingle, (Int64)right);
                    case TypeCode.UInt64:
                        return SingleOps.DivModImpl(leftSingle, (UInt64)right);
                    case TypeCode.Single:
                        return SingleOps.DivModImpl(leftSingle, (Single)right);
                    case TypeCode.Double:
                        return FloatOps.DivMod(leftSingle, (Double)right);
                }
            }
            if (right is BigInteger) {
                return FloatOps.DivMod(leftSingle, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return SingleOps.DivModImpl(leftSingle, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return FloatOps.DivMod(leftSingle, ((ExtensibleLong)right).Value);
            } else if (right is Complex64) {
                return ComplexOps.DivMod(leftSingle, (Complex64)right);
            } else if (right is ExtensibleFloat) {
                return FloatOps.DivMod(leftSingle, ((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return ComplexOps.DivMod(leftSingle, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rdivmod__")]
        public static object ReverseDivMod(object left, object right) {
            Debug.Assert(left is Single);
            Single leftSingle = (Single)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean:
                        return SingleOps.ReverseDivModImpl(leftSingle, ((Boolean)right ? (Single)1 : (Single)0));
                    case TypeCode.Byte:
                        return SingleOps.ReverseDivModImpl(leftSingle, (Byte)right);
                    case TypeCode.SByte:
                        return SingleOps.ReverseDivModImpl(leftSingle, (SByte)right);
                    case TypeCode.Int16:
                        return SingleOps.ReverseDivModImpl(leftSingle, (Int16)right);
                    case TypeCode.UInt16:
                        return SingleOps.ReverseDivModImpl(leftSingle, (UInt16)right);
                    case TypeCode.Int32:
                        return SingleOps.ReverseDivModImpl(leftSingle, (Int32)right);
                    case TypeCode.UInt32:
                        return SingleOps.ReverseDivModImpl(leftSingle, (UInt32)right);
                    case TypeCode.Int64:
                        return SingleOps.ReverseDivModImpl(leftSingle, (Int64)right);
                    case TypeCode.UInt64:
                        return SingleOps.ReverseDivModImpl(leftSingle, (UInt64)right);
                    case TypeCode.Single:
                        return SingleOps.ReverseDivModImpl(leftSingle, (Single)right);
                    case TypeCode.Double:
                        return FloatOps.ReverseDivMod(leftSingle, (Double)right);
                }
            }
            if (right is BigInteger) {
                return FloatOps.ReverseDivMod(leftSingle, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return SingleOps.ReverseDivModImpl(leftSingle, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return FloatOps.ReverseDivMod(leftSingle, ((ExtensibleLong)right).Value);
            } else if (right is Complex64) {
                return ComplexOps.ReverseDivMod(leftSingle, (Complex64)right);
            } else if (right is ExtensibleFloat) {
                return FloatOps.ReverseDivMod(leftSingle, ((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return ComplexOps.ReverseDivMod(leftSingle, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__pow__")]
        public static object Power(object left, object right) {
            Debug.Assert(left is Single);
            Single leftSingle = (Single)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean:
                        return SingleOps.PowerImpl(leftSingle, ((Boolean)right ? (Single)1 : (Single)0));
                    case TypeCode.Byte:
                        return SingleOps.PowerImpl(leftSingle, (Byte)right);
                    case TypeCode.SByte:
                        return SingleOps.PowerImpl(leftSingle, (SByte)right);
                    case TypeCode.Int16:
                        return SingleOps.PowerImpl(leftSingle, (Int16)right);
                    case TypeCode.UInt16:
                        return SingleOps.PowerImpl(leftSingle, (UInt16)right);
                    case TypeCode.Int32:
                        return SingleOps.PowerImpl(leftSingle, (Int32)right);
                    case TypeCode.UInt32:
                        return SingleOps.PowerImpl(leftSingle, (UInt32)right);
                    case TypeCode.Int64:
                        return SingleOps.PowerImpl(leftSingle, (Int64)right);
                    case TypeCode.UInt64:
                        return SingleOps.PowerImpl(leftSingle, (UInt64)right);
                    case TypeCode.Single:
                        return SingleOps.PowerImpl(leftSingle, (Single)right);
                    case TypeCode.Double:
                        return FloatOps.Power(leftSingle, (Double)right);
                }
            }
            if (right is BigInteger) {
                return FloatOps.Power(leftSingle, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return SingleOps.PowerImpl(leftSingle, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return FloatOps.Power(leftSingle, ((ExtensibleLong)right).Value);
            } else if (right is Complex64) {
                return ComplexOps.Power(leftSingle, (Complex64)right);
            } else if (right is ExtensibleFloat) {
                return FloatOps.Power(leftSingle, ((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return ComplexOps.Power(leftSingle, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rpow__")]
        public static object ReversePower(object left, object right) {
            Debug.Assert(left is Single);
            Single leftSingle = (Single)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean:
                        return SingleOps.ReversePowerImpl(leftSingle, ((Boolean)right ? (Single)1 : (Single)0));
                    case TypeCode.Byte:
                        return SingleOps.ReversePowerImpl(leftSingle, (Byte)right);
                    case TypeCode.SByte:
                        return SingleOps.ReversePowerImpl(leftSingle, (SByte)right);
                    case TypeCode.Int16:
                        return SingleOps.ReversePowerImpl(leftSingle, (Int16)right);
                    case TypeCode.UInt16:
                        return SingleOps.ReversePowerImpl(leftSingle, (UInt16)right);
                    case TypeCode.Int32:
                        return SingleOps.ReversePowerImpl(leftSingle, (Int32)right);
                    case TypeCode.UInt32:
                        return SingleOps.ReversePowerImpl(leftSingle, (UInt32)right);
                    case TypeCode.Int64:
                        return SingleOps.ReversePowerImpl(leftSingle, (Int64)right);
                    case TypeCode.UInt64:
                        return SingleOps.ReversePowerImpl(leftSingle, (UInt64)right);
                    case TypeCode.Single:
                        return SingleOps.ReversePowerImpl(leftSingle, (Single)right);
                    case TypeCode.Double:
                        return FloatOps.ReversePower(leftSingle, (Double)right);
                }
            }
            if (right is BigInteger) {
                return FloatOps.ReversePower(leftSingle, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return SingleOps.ReversePowerImpl(leftSingle, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return FloatOps.ReversePower(leftSingle, ((ExtensibleLong)right).Value);
            } else if (right is Complex64) {
                return ComplexOps.ReversePower(leftSingle, (Complex64)right);
            } else if (right is ExtensibleFloat) {
                return FloatOps.ReversePower(leftSingle, ((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return ComplexOps.ReversePower(leftSingle, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__truediv__")]
        public static object TrueDivide(object left, object right) {
            Debug.Assert(left is Single);
            Single leftSingle = (Single)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean:
                        return FloatOps.TrueDivide(leftSingle, ((Boolean)right ? (Single)1 : (Single)0));
                    case TypeCode.Byte:
                        return FloatOps.TrueDivide(leftSingle, (Byte)right);
                    case TypeCode.SByte:
                        return FloatOps.TrueDivide(leftSingle, (SByte)right);
                    case TypeCode.Int16:
                        return FloatOps.TrueDivide(leftSingle, (Int16)right);
                    case TypeCode.UInt16:
                        return FloatOps.TrueDivide(leftSingle, (UInt16)right);
                    case TypeCode.Int32:
                        return FloatOps.TrueDivide(leftSingle, (Int32)right);
                    case TypeCode.UInt32:
                        return FloatOps.TrueDivide(leftSingle, (UInt32)right);
                    case TypeCode.Int64:
                        return FloatOps.TrueDivide(leftSingle, (Int64)right);
                    case TypeCode.UInt64:
                        return FloatOps.TrueDivide(leftSingle, (UInt64)right);
                    case TypeCode.Single:
                        return FloatOps.TrueDivide(leftSingle, (Single)right);
                    case TypeCode.Double:
                        return FloatOps.TrueDivide(leftSingle, (Double)right);
                }
            }
            if (right is BigInteger) {
                return FloatOps.TrueDivide(leftSingle, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return FloatOps.TrueDivide(leftSingle, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return FloatOps.TrueDivide(leftSingle, ((ExtensibleLong)right).Value);
            } else if (right is Complex64) {
                return FloatOps.TrueDivide(leftSingle, (Complex64)right);
            } else if (right is ExtensibleFloat) {
                return FloatOps.TrueDivide(leftSingle, ((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return FloatOps.TrueDivide(leftSingle, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rtruediv__")]
        public static object ReverseTrueDivide(object left, object right) {
            Debug.Assert(left is Single);
            Single leftSingle = (Single)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean:
                        return FloatOps.ReverseTrueDivide(leftSingle, ((Boolean)right ? (Single)1 : (Single)0));
                    case TypeCode.Byte:
                        return FloatOps.ReverseTrueDivide(leftSingle, (Byte)right);
                    case TypeCode.SByte:
                        return FloatOps.ReverseTrueDivide(leftSingle, (SByte)right);
                    case TypeCode.Int16:
                        return FloatOps.ReverseTrueDivide(leftSingle, (Int16)right);
                    case TypeCode.UInt16:
                        return FloatOps.ReverseTrueDivide(leftSingle, (UInt16)right);
                    case TypeCode.Int32:
                        return FloatOps.ReverseTrueDivide(leftSingle, (Int32)right);
                    case TypeCode.UInt32:
                        return FloatOps.ReverseTrueDivide(leftSingle, (UInt32)right);
                    case TypeCode.Int64:
                        return FloatOps.ReverseTrueDivide(leftSingle, (Int64)right);
                    case TypeCode.UInt64:
                        return FloatOps.ReverseTrueDivide(leftSingle, (UInt64)right);
                    case TypeCode.Single:
                        return FloatOps.ReverseTrueDivide(leftSingle, (Single)right);
                    case TypeCode.Double:
                        return FloatOps.ReverseTrueDivide(leftSingle, (Double)right);
                }
            }
            if (right is BigInteger) {
                return FloatOps.ReverseTrueDivide(leftSingle, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return FloatOps.ReverseTrueDivide(leftSingle, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return FloatOps.ReverseTrueDivide(leftSingle, ((ExtensibleLong)right).Value);
            } else if (right is Complex64) {
                return FloatOps.ReverseTrueDivide(leftSingle, (Complex64)right);
            } else if (right is ExtensibleFloat) {
                return FloatOps.ReverseTrueDivide(leftSingle, ((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return FloatOps.ReverseTrueDivide(leftSingle, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }

        // *** END GENERATED CODE ***

        #endregion
    }
}
