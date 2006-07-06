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

namespace IronPython.Compiler.Ast {
    public abstract partial class PythonOperator {
        #region Generated Operators

        // *** BEGIN GENERATED CODE ***

        private static readonly BinaryOperator add = new BinaryOperator("+", new CallTarget2(Ops.Add), new CallTarget2(Ops.InPlaceAdd), 4);
        private static readonly BinaryOperator sub = new BinaryOperator("-", new CallTarget2(Ops.Subtract), new CallTarget2(Ops.InPlaceSubtract), 4);
        private static readonly BinaryOperator pow = new BinaryOperator("**", new CallTarget2(Ops.Power), new CallTarget2(Ops.InPlacePower), 6);
        private static readonly BinaryOperator mul = new BinaryOperator("*", new CallTarget2(Ops.Multiply), new CallTarget2(Ops.InPlaceMultiply), 5);
        private static readonly BinaryOperator floordiv = new BinaryOperator("//", new CallTarget2(Ops.FloorDivide), new CallTarget2(Ops.InPlaceFloorDivide), 5);
        private static readonly BinaryOperator div = new DivisionOperator("/", new CallTarget2(Ops.Divide), new CallTarget2(Ops.InPlaceDivide), new CallTarget2(Ops.TrueDivide), new CallTarget2(Ops.InPlaceTrueDivide), 5);
        private static readonly BinaryOperator mod = new BinaryOperator("%", new CallTarget2(Ops.Mod), new CallTarget2(Ops.InPlaceMod), 5);
        private static readonly BinaryOperator lshift = new BinaryOperator("<<", new CallTarget2(Ops.LeftShift), new CallTarget2(Ops.InPlaceLeftShift), 3);
        private static readonly BinaryOperator rshift = new BinaryOperator(">>", new CallTarget2(Ops.RightShift), new CallTarget2(Ops.InPlaceRightShift), 3);
        private static readonly BinaryOperator and = new BinaryOperator("&", new CallTarget2(Ops.BitwiseAnd), new CallTarget2(Ops.InPlaceBitwiseAnd), 2);
        private static readonly BinaryOperator or = new BinaryOperator("|", new CallTarget2(Ops.BitwiseOr), new CallTarget2(Ops.InPlaceBitwiseOr), 0);
        private static readonly BinaryOperator xor = new BinaryOperator("^", new CallTarget2(Ops.Xor), new CallTarget2(Ops.InPlaceXor), 1);
        private static readonly BinaryOperator lt = new BinaryOperator("<", new CallTarget2(Ops.LessThan), null, -1);
        private static readonly BinaryOperator gt = new BinaryOperator(">", new CallTarget2(Ops.GreaterThan), null, -1);
        private static readonly BinaryOperator le = new BinaryOperator("<=", new CallTarget2(Ops.LessThanOrEqual), null, -1);
        private static readonly BinaryOperator ge = new BinaryOperator(">=", new CallTarget2(Ops.GreaterThanOrEqual), null, -1);
        private static readonly BinaryOperator eq = new BinaryOperator("==", new CallTarget2(Ops.Equal), null, -1);
        private static readonly BinaryOperator ne = new BinaryOperator("!=", new CallTarget2(Ops.NotEqual), null, -1);


        public static BinaryOperator Add {
            get { return add; }
        }

        public static BinaryOperator Subtract {
            get { return sub; }
        }

        public static BinaryOperator Power {
            get { return pow; }
        }

        public static BinaryOperator Multiply {
            get { return mul; }
        }

        public static BinaryOperator FloorDivide {
            get { return floordiv; }
        }

        public static BinaryOperator Divide {
            get { return div; }
        }

        public static BinaryOperator Mod {
            get { return mod; }
        }

        public static BinaryOperator LeftShift {
            get { return lshift; }
        }

        public static BinaryOperator RightShift {
            get { return rshift; }
        }

        public static BinaryOperator BitwiseAnd {
            get { return and; }
        }

        public static BinaryOperator BitwiseOr {
            get { return or; }
        }

        public static BinaryOperator Xor {
            get { return xor; }
        }

        public static BinaryOperator LessThan {
            get { return lt; }
        }

        public static BinaryOperator GreaterThan {
            get { return gt; }
        }

        public static BinaryOperator LessThanOrEqual {
            get { return le; }
        }

        public static BinaryOperator GreaterThanOrEqual {
            get { return ge; }
        }

        public static BinaryOperator Equal {
            get { return eq; }
        }

        public static BinaryOperator NotEqual {
            get { return ne; }
        }


        // *** END GENERATED CODE ***

        #endregion
    }
}
