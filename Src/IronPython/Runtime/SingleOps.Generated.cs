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
    static partial class SingleOps {
        #region Generated SingleOps

        // *** BEGIN GENERATED CODE ***

        public static DynamicType MakeDynamicType() {
            return new OpsReflectedType("Single", typeof(Single), typeof(SingleOps), null);
        }

        [PythonName("__add__")]
        public static object Add(object left, object right) {
            Debug.Assert(left is Single);
            Single leftSingle = (Single)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
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
            return Ops.NotImplemented;
        }
        [PythonName("__div__")]
        public static object Divide(object left, object right) {
            Debug.Assert(left is Single);
            Single leftSingle = (Single)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
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
            return Ops.NotImplemented;
        }
        [PythonName("__floordiv__")]
        public static object FloorDivide(object left, object right) {
            Debug.Assert(left is Single);
            Single leftSingle = (Single)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
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
            return Ops.NotImplemented;
        }
        [PythonName("__mod__")]
        public static object Mod(object left, object right) {
            Debug.Assert(left is Single);
            Single leftSingle = (Single)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
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
            return Ops.NotImplemented;
        }
        [PythonName("__mul__")]
        public static object Multiply(object left, object right) {
            Debug.Assert(left is Single);
            Single leftSingle = (Single)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
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
                            return FloatOps.Multiply(leftSingle, (Double)right);
                        }
                }
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
            return Ops.NotImplemented;
        }
        [PythonName("__radd__")]
        public static object ReverseAdd(object left, object right) {
            Debug.Assert(left is Single);
            Single leftSingle = (Single)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
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
            return Ops.NotImplemented;
        }
        [PythonName("__rdiv__")]
        public static object ReverseDivide(object left, object right) {
            Debug.Assert(left is Single);
            Single leftSingle = (Single)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
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
            return Ops.NotImplemented;
        }
        [PythonName("__rfloordiv__")]
        public static object ReverseFloorDivide(object left, object right) {
            Debug.Assert(left is Single);
            Single leftSingle = (Single)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
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
            return Ops.NotImplemented;
        }
        [PythonName("__rmod__")]
        public static object ReverseMod(object left, object right) {
            Debug.Assert(left is Single);
            Single leftSingle = (Single)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
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
            return Ops.NotImplemented;
        }
        [PythonName("__rmul__")]
        public static object ReverseMultiply(object left, object right) {
            Debug.Assert(left is Single);
            Single leftSingle = (Single)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
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
                            return FloatOps.ReverseMultiply(leftSingle, (Double)right);
                        }
                }
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
            return Ops.NotImplemented;
        }

        // *** END GENERATED CODE ***

        #endregion
    }
}
