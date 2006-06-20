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

using IronPython.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Compiler.Generation;

using IronPython.Runtime.Operations;


namespace IronPython.Compiler.AST {
    public abstract partial class Operator {
        private static readonly BinaryOperator binIs = new BinaryOperator("is", new CallTarget2(Ops.Is), null, -1);
        private static readonly BinaryOperator binIsNot = new BinaryOperator("is not", new CallTarget2(Ops.IsNot), null, -1);
        private static readonly BinaryOperator binIn = new BinaryOperator("in", new CallTarget2(Ops.In), null, -1);
        private static readonly BinaryOperator binNotIn = new BinaryOperator("not in", new CallTarget2(Ops.NotIn), null, -1);

        private static readonly UnaryOperator unPos = new UnaryOperator("+", new CallTarget1(Ops.Plus));
        private static readonly UnaryOperator unNeg = new UnaryOperator("-", new CallTarget1(Ops.Negate));
        private static readonly UnaryOperator unInvert = new UnaryOperator("~", new CallTarget1(Ops.OnesComplement));
        private static readonly UnaryOperator unNot = new UnaryOperator("not", new CallTarget1(Ops.Not));


        public static BinaryOperator Is {
            get {
                return binIs;
            }
        }

        public static BinaryOperator IsNot {
            get {
                return binIsNot;
            }
        }

        public static BinaryOperator In {
            get {
                return binIn;
            }
        }

        public static BinaryOperator NotIn {
            get {
                return binNotIn;
            }
        }

        public static UnaryOperator Pos {
            get {
                return unPos;
            }
        }

        public static UnaryOperator Neg {
            get {
                return unNeg;
            }
        }

        public static UnaryOperator Invert {
            get {
                return unInvert;
            }
        }

        public static UnaryOperator Not {
            get {
                return unNot;   
            }
        }

        public readonly string symbol;
        public readonly int precedence;

        protected Operator(string symbol, int precedence) {
            this.symbol = symbol;
            this.precedence = precedence;
        }
    }

    public class UnaryOperator : Operator {
        private readonly CallTarget1 target;

        public UnaryOperator(string symbol, CallTarget1 target)
            : base(symbol, -1) {
            this.target = target;
        }

        public object Evaluate(object o) {
            return target(o);
        }

        public CallTarget1 Target {
            get {
                return target;
            }
        }
    }

    public class BinaryOperator : Operator {
        private readonly CallTarget2 target;
        private readonly CallTarget2 inPlaceTarget;

        public BinaryOperator(string symbol, CallTarget2 target, CallTarget2 inPlaceTarget, int precedence)
            : base(symbol, precedence) {
            this.target = target;
            this.inPlaceTarget = inPlaceTarget;
        }

        public CallTarget2 Target {
            get {
                return target;
            }
        }

        public CallTarget2 InPlaceTarget {
            get {
                return inPlaceTarget;
            }
        }

        internal virtual void Emit(CodeGen cg) {
            cg.EmitCall(target.Method);
        }

        internal virtual void EmitInPlace(CodeGen cg) {
            cg.EmitCall(inPlaceTarget.Method);
        }

        public object Evaluate(object x, object y) {
            return target(x, y);
        }

        public object EvaluateInPlace(object x, object y) {
            return inPlaceTarget(x, y);
        }

        public bool IsComparision() { return precedence == -1; }
    }

    public class DivisionOperator : BinaryOperator {
        private readonly CallTarget2 targetTrue;
        private readonly CallTarget2 inPlaceTargetTrue;

        public CallTarget2 TargetTrue {
            get { return targetTrue; }
        }

        public CallTarget2 InPlaceTargetTrue {
            get { return inPlaceTargetTrue; }
        } 


        public DivisionOperator(string symbol, CallTarget2 target, CallTarget2 inPlaceTarget, CallTarget2 targetTrue, CallTarget2 inPlaceTargetTrue, int precedence)
            : base(symbol, target, inPlaceTarget, precedence) {
            this.targetTrue = targetTrue;
            this.inPlaceTargetTrue = inPlaceTargetTrue;
        }

        internal override void Emit(CodeGen cg) {
            if (cg.Context.TrueDivision) {
                cg.EmitCall(targetTrue.Method);
            } else {
                base.Emit(cg);
            }
        }
        internal override void EmitInPlace(CodeGen cg) {
            if (cg.Context.TrueDivision) {
                cg.EmitCall(inPlaceTargetTrue.Method);
            } else {
                base.EmitInPlace(cg);
            }
        }
    }
}
