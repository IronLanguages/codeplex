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
    static partial class UInt64Ops {

        #region Generated UInt64Ops

        // *** BEGIN GENERATED CODE ***

        public static DynamicType MakeDynamicType() {
            return new OpsReflectedType("UInt64", typeof(UInt64), typeof(UInt64Ops), null);
        }

        [PythonName("__add__")]
        public static object Add(object left, object right) {
            Debug.Assert(left is UInt64);
            UInt64 leftUInt64 = (UInt64)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            return UInt64Ops.AddImpl(leftUInt64, (Byte)right);
                        }
                    case TypeCode.SByte: {
                            return UInt64Ops.AddImpl(leftUInt64, (SByte)right);
                        }
                    case TypeCode.Int16: {
                            return UInt64Ops.AddImpl(leftUInt64, (Int16)right);
                        }
                    case TypeCode.UInt16: {
                            return UInt64Ops.AddImpl(leftUInt64, (UInt16)right);
                        }
                    case TypeCode.Int32: {
                            return UInt64Ops.AddImpl(leftUInt64, (Int32)right);
                        }
                    case TypeCode.UInt32: {
                            return UInt64Ops.AddImpl(leftUInt64, (UInt32)right);
                        }
                    case TypeCode.Int64: {
                            return UInt64Ops.AddImpl(leftUInt64, (Int64)right);
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.AddImpl(leftUInt64, (UInt64)right);
                        }
                    case TypeCode.Single: {
                            return (Single)(((Single)leftUInt64) + ((Single)((Single)right)));
                        }
                    case TypeCode.Double: {
                            return (Double)(((Double)leftUInt64) + ((Double)((Double)right)));
                        }
                }
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
                    case TypeCode.Byte: {
                            return UInt64Ops.DivideImpl((UInt64)leftUInt64, (UInt64)((Byte)right));
                        }
                    case TypeCode.SByte: {
                            return UInt64Ops.DivideImpl(leftUInt64, (SByte)right);
                        }
                    case TypeCode.Int16: {
                            return UInt64Ops.DivideImpl(leftUInt64, (Int16)right);
                        }
                    case TypeCode.UInt16: {
                            return UInt64Ops.DivideImpl((UInt64)leftUInt64, (UInt64)((UInt16)right));
                        }
                    case TypeCode.Int32: {
                            return UInt64Ops.DivideImpl(leftUInt64, (Int32)right);
                        }
                    case TypeCode.UInt32: {
                            return UInt64Ops.DivideImpl((UInt64)leftUInt64, (UInt64)((UInt32)right));
                        }
                    case TypeCode.Int64: {
                            return UInt64Ops.DivideImpl(leftUInt64, (Int64)right);
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
            return Ops.NotImplemented;
        }
        [PythonName("__floordiv__")]
        public static object FloorDivide(object left, object right) {
            Debug.Assert(left is UInt64);
            UInt64 leftUInt64 = (UInt64)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            return UInt64Ops.FloorDivideImpl((UInt64)leftUInt64, (UInt64)((Byte)right));
                        }
                    case TypeCode.SByte: {
                            return UInt64Ops.FloorDivideImpl(leftUInt64, (SByte)right);
                        }
                    case TypeCode.Int16: {
                            return UInt64Ops.FloorDivideImpl(leftUInt64, (Int16)right);
                        }
                    case TypeCode.UInt16: {
                            return UInt64Ops.FloorDivideImpl((UInt64)leftUInt64, (UInt64)((UInt16)right));
                        }
                    case TypeCode.Int32: {
                            return UInt64Ops.FloorDivideImpl(leftUInt64, (Int32)right);
                        }
                    case TypeCode.UInt32: {
                            return UInt64Ops.FloorDivideImpl((UInt64)leftUInt64, (UInt64)((UInt32)right));
                        }
                    case TypeCode.Int64: {
                            return UInt64Ops.FloorDivideImpl(leftUInt64, (Int64)right);
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
            return Ops.NotImplemented;
        }
        [PythonName("__mod__")]
        public static object Mod(object left, object right) {
            Debug.Assert(left is UInt64);
            UInt64 leftUInt64 = (UInt64)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            return UInt64Ops.ModImpl((UInt64)leftUInt64, (UInt64)((Byte)right));
                        }
                    case TypeCode.SByte: {
                            return UInt64Ops.ModImpl(leftUInt64, (SByte)right);
                        }
                    case TypeCode.Int16: {
                            return UInt64Ops.ModImpl(leftUInt64, (Int16)right);
                        }
                    case TypeCode.UInt16: {
                            return UInt64Ops.ModImpl((UInt64)leftUInt64, (UInt64)((UInt16)right));
                        }
                    case TypeCode.Int32: {
                            return UInt64Ops.ModImpl(leftUInt64, (Int32)right);
                        }
                    case TypeCode.UInt32: {
                            return UInt64Ops.ModImpl((UInt64)leftUInt64, (UInt64)((UInt32)right));
                        }
                    case TypeCode.Int64: {
                            return UInt64Ops.ModImpl(leftUInt64, (Int64)right);
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
            return Ops.NotImplemented;
        }
        [PythonName("__mul__")]
        public static object Multiply(object left, object right) {
            Debug.Assert(left is UInt64);
            UInt64 leftUInt64 = (UInt64)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            return UInt64Ops.MultiplyImpl(leftUInt64, (Byte)right);
                        }
                    case TypeCode.SByte: {
                            return UInt64Ops.MultiplyImpl(leftUInt64, (SByte)right);
                        }
                    case TypeCode.Int16: {
                            return UInt64Ops.MultiplyImpl(leftUInt64, (Int16)right);
                        }
                    case TypeCode.UInt16: {
                            return UInt64Ops.MultiplyImpl(leftUInt64, (UInt16)right);
                        }
                    case TypeCode.Int32: {
                            return UInt64Ops.MultiplyImpl(leftUInt64, (Int32)right);
                        }
                    case TypeCode.UInt32: {
                            return UInt64Ops.MultiplyImpl(leftUInt64, (UInt32)right);
                        }
                    case TypeCode.Int64: {
                            return UInt64Ops.MultiplyImpl(leftUInt64, (Int64)right);
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.MultiplyImpl(leftUInt64, (UInt64)right);
                        }
                    case TypeCode.Single: {
                            return (Double)(((Double)leftUInt64) * ((Double)((Single)right)));
                        }
                    case TypeCode.Double: {
                            return FloatOps.Multiply(leftUInt64, (Double)right);
                        }
                }
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
                    case TypeCode.Byte: {
                            return UInt64Ops.SubtractImpl(leftUInt64, (Byte)right);
                        }
                    case TypeCode.SByte: {
                            return UInt64Ops.SubtractImpl(leftUInt64, (SByte)right);
                        }
                    case TypeCode.Int16: {
                            return UInt64Ops.SubtractImpl(leftUInt64, (Int16)right);
                        }
                    case TypeCode.UInt16: {
                            return UInt64Ops.SubtractImpl(leftUInt64, (UInt16)right);
                        }
                    case TypeCode.Int32: {
                            return UInt64Ops.SubtractImpl(leftUInt64, (Int32)right);
                        }
                    case TypeCode.UInt32: {
                            return UInt64Ops.SubtractImpl(leftUInt64, (UInt32)right);
                        }
                    case TypeCode.Int64: {
                            return UInt64Ops.SubtractImpl(leftUInt64, (Int64)right);
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.SubtractImpl(leftUInt64, (UInt64)right);
                        }
                    case TypeCode.Single: {
                            return (Single)(((Single)leftUInt64) - ((Single)((Single)right)));
                        }
                    case TypeCode.Double: {
                            return (Double)(((Double)leftUInt64) - ((Double)((Double)right)));
                        }
                }
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
                    case TypeCode.Byte: {
                            return UInt64Ops.ReverseAddImpl(leftUInt64, (Byte)right);
                        }
                    case TypeCode.SByte: {
                            return UInt64Ops.ReverseAddImpl(leftUInt64, (SByte)right);
                        }
                    case TypeCode.Int16: {
                            return UInt64Ops.ReverseAddImpl(leftUInt64, (Int16)right);
                        }
                    case TypeCode.UInt16: {
                            return UInt64Ops.ReverseAddImpl(leftUInt64, (UInt16)right);
                        }
                    case TypeCode.Int32: {
                            return UInt64Ops.ReverseAddImpl(leftUInt64, (Int32)right);
                        }
                    case TypeCode.UInt32: {
                            return UInt64Ops.ReverseAddImpl(leftUInt64, (UInt32)right);
                        }
                    case TypeCode.Int64: {
                            return UInt64Ops.ReverseAddImpl(leftUInt64, (Int64)right);
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ReverseAddImpl(leftUInt64, (UInt64)right);
                        }
                    case TypeCode.Single: {
                            return (Single)(((Single)((Single)right)) + ((Single)leftUInt64));
                        }
                    case TypeCode.Double: {
                            return (Double)(((Double)((Double)right)) + ((Double)leftUInt64));
                        }
                }
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
                    case TypeCode.Byte: {
                            return UInt64Ops.ReverseDivideImpl((UInt64)leftUInt64, (UInt64)((Byte)right));
                        }
                    case TypeCode.SByte: {
                            return UInt64Ops.ReverseDivideImpl(leftUInt64, (SByte)right);
                        }
                    case TypeCode.Int16: {
                            return UInt64Ops.ReverseDivideImpl(leftUInt64, (Int16)right);
                        }
                    case TypeCode.UInt16: {
                            return UInt64Ops.ReverseDivideImpl((UInt64)leftUInt64, (UInt64)((UInt16)right));
                        }
                    case TypeCode.Int32: {
                            return UInt64Ops.ReverseDivideImpl(leftUInt64, (Int32)right);
                        }
                    case TypeCode.UInt32: {
                            return UInt64Ops.ReverseDivideImpl((UInt64)leftUInt64, (UInt64)((UInt32)right));
                        }
                    case TypeCode.Int64: {
                            return UInt64Ops.ReverseDivideImpl(leftUInt64, (Int64)right);
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
            return Ops.NotImplemented;
        }
        [PythonName("__rfloordiv__")]
        public static object ReverseFloorDivide(object left, object right) {
            Debug.Assert(left is UInt64);
            UInt64 leftUInt64 = (UInt64)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            return UInt64Ops.ReverseFloorDivideImpl((UInt64)leftUInt64, (UInt64)((Byte)right));
                        }
                    case TypeCode.SByte: {
                            return UInt64Ops.ReverseFloorDivideImpl(leftUInt64, (SByte)right);
                        }
                    case TypeCode.Int16: {
                            return UInt64Ops.ReverseFloorDivideImpl(leftUInt64, (Int16)right);
                        }
                    case TypeCode.UInt16: {
                            return UInt64Ops.ReverseFloorDivideImpl((UInt64)leftUInt64, (UInt64)((UInt16)right));
                        }
                    case TypeCode.Int32: {
                            return UInt64Ops.ReverseFloorDivideImpl(leftUInt64, (Int32)right);
                        }
                    case TypeCode.UInt32: {
                            return UInt64Ops.ReverseFloorDivideImpl((UInt64)leftUInt64, (UInt64)((UInt32)right));
                        }
                    case TypeCode.Int64: {
                            return UInt64Ops.ReverseFloorDivideImpl(leftUInt64, (Int64)right);
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
            return Ops.NotImplemented;
        }
        [PythonName("__rmod__")]
        public static object ReverseMod(object left, object right) {
            Debug.Assert(left is UInt64);
            UInt64 leftUInt64 = (UInt64)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            return UInt64Ops.ReverseModImpl((UInt64)leftUInt64, (UInt64)((Byte)right));
                        }
                    case TypeCode.SByte: {
                            return UInt64Ops.ReverseModImpl(leftUInt64, (SByte)right);
                        }
                    case TypeCode.Int16: {
                            return UInt64Ops.ReverseModImpl(leftUInt64, (Int16)right);
                        }
                    case TypeCode.UInt16: {
                            return UInt64Ops.ReverseModImpl((UInt64)leftUInt64, (UInt64)((UInt16)right));
                        }
                    case TypeCode.Int32: {
                            return UInt64Ops.ReverseModImpl(leftUInt64, (Int32)right);
                        }
                    case TypeCode.UInt32: {
                            return UInt64Ops.ReverseModImpl((UInt64)leftUInt64, (UInt64)((UInt32)right));
                        }
                    case TypeCode.Int64: {
                            return UInt64Ops.ReverseModImpl(leftUInt64, (Int64)right);
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
            return Ops.NotImplemented;
        }
        [PythonName("__rmul__")]
        public static object ReverseMultiply(object left, object right) {
            Debug.Assert(left is UInt64);
            UInt64 leftUInt64 = (UInt64)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Byte: {
                            return UInt64Ops.ReverseMultiplyImpl(leftUInt64, (Byte)right);
                        }
                    case TypeCode.SByte: {
                            return UInt64Ops.ReverseMultiplyImpl(leftUInt64, (SByte)right);
                        }
                    case TypeCode.Int16: {
                            return UInt64Ops.ReverseMultiplyImpl(leftUInt64, (Int16)right);
                        }
                    case TypeCode.UInt16: {
                            return UInt64Ops.ReverseMultiplyImpl(leftUInt64, (UInt16)right);
                        }
                    case TypeCode.Int32: {
                            return UInt64Ops.ReverseMultiplyImpl(leftUInt64, (Int32)right);
                        }
                    case TypeCode.UInt32: {
                            return UInt64Ops.ReverseMultiplyImpl(leftUInt64, (UInt32)right);
                        }
                    case TypeCode.Int64: {
                            return UInt64Ops.ReverseMultiplyImpl(leftUInt64, (Int64)right);
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ReverseMultiplyImpl(leftUInt64, (UInt64)right);
                        }
                    case TypeCode.Single: {
                            return (Double)(((Double)((Single)right)) * ((Double)leftUInt64));
                        }
                    case TypeCode.Double: {
                            return FloatOps.ReverseMultiply(leftUInt64, (Double)right);
                        }
                }
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
                    case TypeCode.Byte: {
                            return UInt64Ops.ReverseSubtractImpl(leftUInt64, (Byte)right);
                        }
                    case TypeCode.SByte: {
                            return UInt64Ops.ReverseSubtractImpl(leftUInt64, (SByte)right);
                        }
                    case TypeCode.Int16: {
                            return UInt64Ops.ReverseSubtractImpl(leftUInt64, (Int16)right);
                        }
                    case TypeCode.UInt16: {
                            return UInt64Ops.ReverseSubtractImpl(leftUInt64, (UInt16)right);
                        }
                    case TypeCode.Int32: {
                            return UInt64Ops.ReverseSubtractImpl(leftUInt64, (Int32)right);
                        }
                    case TypeCode.UInt32: {
                            return UInt64Ops.ReverseSubtractImpl(leftUInt64, (UInt32)right);
                        }
                    case TypeCode.Int64: {
                            return UInt64Ops.ReverseSubtractImpl(leftUInt64, (Int64)right);
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ReverseSubtractImpl(leftUInt64, (UInt64)right);
                        }
                    case TypeCode.Single: {
                            return (Single)(((Single)((Single)right)) - ((Single)leftUInt64));
                        }
                    case TypeCode.Double: {
                            return (Double)(((Double)((Double)right)) - ((Double)leftUInt64));
                        }
                }
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
            return Ops.NotImplemented;
        }
        [PythonName("__rand__")]
        public static object ReverseBitwiseAnd(object left, object right) {
            Debug.Assert(left is UInt64);
            UInt64 leftUInt64 = (UInt64)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
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
            return Ops.NotImplemented;
        }
        [PythonName("__or__")]
        public static object BitwiseOr(object left, object right) {
            Debug.Assert(left is UInt64);
            UInt64 leftUInt64 = (UInt64)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
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
            return Ops.NotImplemented;
        }
        [PythonName("__ror__")]
        public static object ReverseBitwiseOr(object left, object right) {
            Debug.Assert(left is UInt64);
            UInt64 leftUInt64 = (UInt64)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
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
            return Ops.NotImplemented;
        }
        [PythonName("__rxor__")]
        public static object BitwiseXor(object left, object right) {
            Debug.Assert(left is UInt64);
            UInt64 leftUInt64 = (UInt64)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
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
            return Ops.NotImplemented;
        }
        [PythonName("__xor__")]
        public static object ReverseBitwiseXor(object left, object right) {
            Debug.Assert(left is UInt64);
            UInt64 leftUInt64 = (UInt64)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
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
            return Ops.NotImplemented;
        }
        internal static object DivideImpl(UInt64 x, UInt64 y) {
            UInt64 q = (UInt64)(x / y);
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

        internal static object ReverseDivideImpl(UInt64 x, UInt64 y) {
            return DivideImpl(y, x);
        }

        internal static object ModImpl(UInt64 x, UInt64 y) {
            UInt64 r = (UInt64)(x % y);
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

        internal static object ReverseModImpl(UInt64 x, UInt64 y) {
            return ModImpl(y, x);
        }

        internal static object FloorDivideImpl(UInt64 x, UInt64 y) {
            return DivideImpl(x, y);
        }

        internal static object ReverseFloorDivideImpl(UInt64 x, UInt64 y) {
            return DivideImpl(y, x);
        }


        // *** END GENERATED CODE ***

        #endregion

    }
}
