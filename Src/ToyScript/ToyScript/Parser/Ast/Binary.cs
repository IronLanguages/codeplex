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

using Microsoft.Scripting;
using MSAst = Microsoft.Scripting.Ast;

namespace ToyScript.Parser.Ast {
    using Ast = MSAst.Ast;

    class Binary : Expression {
        private readonly Operator _op;
        private readonly Expression _left;
        private readonly Expression _right;

        public Binary(SourceSpan span, Operator op, Expression left, Expression right)
            : base(span) {
            _op = op;
            _left = left;
            _right = right;
        }

        protected internal override MSAst.Expression Generate(ToyGenerator tg) {
            MSAst.Expression left = _left.Generate(tg);
            MSAst.Expression right = _right.Generate(tg);

            Operators op;
            switch (_op) {
                // Binary
                case Operator.Add: op = Operators.Add; break;
                case Operator.Subtract: op = Operators.Subtract; break;
                case Operator.Multiply: op = Operators.Multiply; break;
                case Operator.Divide: op = Operators.Divide; break;

                // Comparisons
                case Operator.LessThan: op = Operators.LessThan; break;
                case Operator.LessThanOrEqual: op = Operators.LessThanOrEqual; break;
                case Operator.GreaterThan: op = Operators.GreaterThan; break;
                case Operator.GreaterThanOrEqual: op = Operators.GreaterThanOrEqual; break;
                case Operator.Equals: op = Operators.Equals; break;
                case Operator.NotEquals: op = Operators.NotEquals; break;

                default:
                    throw new System.InvalidOperationException();
            }
            return Ast.Action.Operator(op, typeof(object), left, right);
        }
    }
}
