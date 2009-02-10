/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System; using Microsoft;
using System.Diagnostics;
using Microsoft.Scripting.Runtime;
using MSAst = Microsoft.Linq.Expressions;

using IronPython.Runtime.Binding;

namespace IronPython.Compiler.Ast {
    using Ast = Microsoft.Linq.Expressions.Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public class UnaryExpression : Expression {
        private readonly Expression _expression;
        private readonly PythonOperator _op;

        public UnaryExpression(PythonOperator op, Expression expression) {
            _op = op;
            _expression = expression;
            End = expression.End;
        }

        public Expression Expression {
            get { return _expression; }
        }

        public PythonOperator Op {
            get { return _op; }
        }

        internal override MSAst.Expression Transform(AstGenerator ag, Type type) {
            return Binders.Operation(
                ag.BinderState,
                type,
                PythonOperatorToOperatorString(_op),
                ag.Transform(_expression)
            );
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_expression != null) {
                    _expression.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }

        private static PythonOperationKind PythonOperatorToOperatorString(PythonOperator op) {
            switch (op) {
                // Unary
                case PythonOperator.Not:
                    return PythonOperationKind.Not;
                case PythonOperator.Pos:
                    return PythonOperationKind.Positive;
                case PythonOperator.Invert:
                    return PythonOperationKind.OnesComplement;
                case PythonOperator.Negate:
                    return PythonOperationKind.Negate;
                default:
                    Debug.Assert(false, "Unexpected PythonOperator: " + op.ToString());
                    return PythonOperationKind.None;
            }
        }
    }
}
