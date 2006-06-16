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
using System.Collections.Generic;
using System.Text;

using IronPython.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;

namespace IronPython.Compiler.AST {
    public abstract partial class Operator {
        #region Generated Operators

        // *** BEGIN GENERATED CODE ***

        public static readonly BinaryOperator Add = new BinaryOperator("+", new CallTarget2(Ops.Add), new CallTarget2(Ops.InPlaceAdd), 4);
        public static readonly BinaryOperator Subtract = new BinaryOperator("-", new CallTarget2(Ops.Subtract), new CallTarget2(Ops.InPlaceSubtract), 4);
        public static readonly BinaryOperator Power = new BinaryOperator("**", new CallTarget2(Ops.Power), new CallTarget2(Ops.InPlacePower), 6);
        public static readonly BinaryOperator Multiply = new BinaryOperator("*", new CallTarget2(Ops.Multiply), new CallTarget2(Ops.InPlaceMultiply), 5);
        public static readonly BinaryOperator FloorDivide = new BinaryOperator("//", new CallTarget2(Ops.FloorDivide), new CallTarget2(Ops.InPlaceFloorDivide), 5);
        public static readonly BinaryOperator Divide = new DivisionOperator("/", new CallTarget2(Ops.Divide), new CallTarget2(Ops.InPlaceDivide), new CallTarget2(Ops.TrueDivide), new CallTarget2(Ops.InPlaceTrueDivide), 5);
        public static readonly BinaryOperator Mod = new BinaryOperator("%", new CallTarget2(Ops.Mod), new CallTarget2(Ops.InPlaceMod), 5);
        public static readonly BinaryOperator LeftShift = new BinaryOperator("<<", new CallTarget2(Ops.LeftShift), new CallTarget2(Ops.InPlaceLeftShift), 3);
        public static readonly BinaryOperator RightShift = new BinaryOperator(">>", new CallTarget2(Ops.RightShift), new CallTarget2(Ops.InPlaceRightShift), 3);
        public static readonly BinaryOperator BitwiseAnd = new BinaryOperator("&", new CallTarget2(Ops.BitwiseAnd), new CallTarget2(Ops.InPlaceBitwiseAnd), 2);
        public static readonly BinaryOperator BitwiseOr = new BinaryOperator("|", new CallTarget2(Ops.BitwiseOr), new CallTarget2(Ops.InPlaceBitwiseOr), 0);
        public static readonly BinaryOperator Xor = new BinaryOperator("^", new CallTarget2(Ops.Xor), new CallTarget2(Ops.InPlaceXor), 1);
        public static readonly BinaryOperator LessThan = new BinaryOperator("<", new CallTarget2(Ops.LessThan), null, -1);
        public static readonly BinaryOperator GreaterThan = new BinaryOperator(">", new CallTarget2(Ops.GreaterThan), null, -1);
        public static readonly BinaryOperator LessThanOrEqual = new BinaryOperator("<=", new CallTarget2(Ops.LessThanOrEqual), null, -1);
        public static readonly BinaryOperator GreaterThanOrEqual = new BinaryOperator(">=", new CallTarget2(Ops.GreaterThanOrEqual), null, -1);
        public static readonly BinaryOperator Equal = new BinaryOperator("==", new CallTarget2(Ops.Equal), null, -1);
        public static readonly BinaryOperator NotEqual = new BinaryOperator("!=", new CallTarget2(Ops.NotEqual), null, -1);

        // *** END GENERATED CODE ***

        #endregion
    }
}
