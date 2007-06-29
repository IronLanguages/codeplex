/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Ast {
    public class ShortCircuitExpression : Expression {
        private readonly MethodInfo _testOp;
        private readonly MethodInfo _resultOp;
        private readonly Expression _left, _right;

        internal ShortCircuitExpression(SourceSpan span, MethodInfo testOp, MethodInfo resultOp, Expression left, Expression right)
            : base(span) {
            if (testOp == null) {
                throw new ArgumentNullException("testOp");
            }
            if (resultOp == null) {
                throw new ArgumentNullException("resultOp");
            }
            if (left == null) {
                throw new ArgumentNullException("left");
            }
            if (right == null) {
                throw new ArgumentNullException("right");
            }

            ParameterInfo[] parameters;
            // The testOp must be public static
            if ((testOp.Attributes & (MethodAttributes.Public | MethodAttributes.Static)) !=
                (MethodAttributes.Public | MethodAttributes.Static)) {
                throw new ArgumentException("testOp must be public and static");
            }
            // And take exactly one parameter
            parameters = testOp.GetParameters();
            if (parameters.Length != 1) {
                throw new ArgumentException("testOp must have exactly one parameter");
            }
            Type testType = parameters[0].ParameterType;

            // The resultOp must be public static also
            if ((resultOp.Attributes & (MethodAttributes.Public | MethodAttributes.Static)) !=
                (MethodAttributes.Public | MethodAttributes.Static)) {
                throw new ArgumentException("resultOp must be public static");
            }

            // And take exactly two parameters
            parameters = resultOp.GetParameters();
            if (parameters.Length != 2) {
                throw new ArgumentException("resultOp must have exactly two parameters");
            }

            // The first resultOp parameter must be of the same type as testOp parameter
            // This is perhaps too restrictive, but certainly correct.
            if (parameters[0].ParameterType != testType) {
                throw new ArgumentException("testOp and resultOp must have the same type of the first parameter");
            }

            // Finally, the testOp parameter type must be the same as the
            // result type of the _resultOp. This ensures that the expression
            // behaves consistently.
            if (testType != resultOp.ReturnType) {
                throw new ArgumentException("testOp parameter type must be the same as the resultOp return type");
            }

            this._testOp = testOp;
            this._resultOp = resultOp;
            this._left = left;
            this._right = right;
        }

        public MethodInfo Test {
            get { return _testOp; }
        }

        public MethodInfo Result {
            get { return _resultOp; }
        }

        public Expression Right {
            get { return _right; }
        }

        public Expression Left {
            get { return _left; }
        }

        public override Type ExpressionType {
            get {
                return _resultOp.ReturnType;
            }
        }

        public override object Evaluate(CodeContext context) {
            object left = _left.Evaluate(context);
            if (!((bool)_testOp.Invoke(null, new object[] { left }))) {
                object right = _right.Evaluate(context);
                return _resultOp.Invoke(null, new object[] { left, right });
            } else {
                return left;
            }
        }

        public override void Emit(CodeGen cg) {
            ParameterInfo[] parameters = _resultOp.GetParameters();

            // Emit the left value as the ParameterType required by the
            // resultOp (which is the same as that required by the testOp
            // (see validation in the constructor)
            _left.EmitAs(cg, parameters[0].ParameterType);

            // Call the _testOp. It takes one parameter
            cg.Emit(OpCodes.Dup);
            cg.EmitCall(_testOp);

            // Convert the result to bool
            cg.EmitConvert(_testOp.ReturnType, typeof(bool));

            Label l = cg.DefineLabel();
            cg.Emit(OpCodes.Brtrue, l);

            // Emit the right expression as the required parameter type
            _right.EmitAs(cg, parameters[1].ParameterType);

            // Call the _resultOp method. Its result type is the same
            // as parameters[0].ParameterType so the stack is in consistent
            // state.
            cg.EmitCall(_resultOp);
            cg.MarkLabel(l);
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                _left.Walk(walker);
                _right.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }

    /// <summary>
    /// Factory methods.
    /// </summary>
    public static partial class Ast {
        public static ShortCircuitExpression ShortCircuit(MethodInfo test, MethodInfo result, Expression left, Expression right) {
            return ShortCircuit(SourceSpan.None, test, result, left, right);
        }
        public static ShortCircuitExpression ShortCircuit(SourceSpan span, MethodInfo test, MethodInfo result, Expression left, Expression right) {
            return new ShortCircuitExpression(span, test, result, left, right);
        }
    }
}
