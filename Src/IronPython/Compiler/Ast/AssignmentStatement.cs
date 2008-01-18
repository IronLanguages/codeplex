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

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.Scripting;
using MSAst = Microsoft.Scripting.Ast;

using IronPython.Runtime.Operations;

namespace IronPython.Compiler.Ast {
    using Ast = Microsoft.Scripting.Ast.Ast;

    public class AssignmentStatement : Statement {
        // _left.Length is 1 for simple assignments like "x = 1"
        // _left.Length will be 3 for "x = y = z = 1"
        private readonly Expression[] _left;
        private readonly Expression _right;

        public AssignmentStatement(Expression[] left, Expression right) {
            _left = left;
            _right = right;
        }

        public IList<Expression> Left {
            get { return _left; }
        }

        public Expression Right {
            get { return _right; }
        }

        internal override MSAst.Expression Transform(AstGenerator ag) {
            // Transform right
            MSAst.Expression right = ag.Transform(_right);

            if (_left.Length == 1) {
                // Do not need temps for simple assignment
                return _left[0].TransformSet(ag, Span, right, Operators.None);
            } else {
                // Python assignment semantics:
                // - only evaluate RHS once. 
                // - evaluates assignment from left to right
                // - does not evaluate getters.
                // 
                // So 
                //   a=b[c]=d=f() 
                // should be:
                //   $temp = f()
                //   a = $temp
                //   b[c] = $temp
                //   d = $temp

                List<MSAst.Expression> statements = new List<MSAst.Expression>();

                // 1. Create temp variable for the right value
                MSAst.BoundExpression right_temp = ag.MakeTempExpression("assignment");

                // 2. right_temp = right
                statements.Add(
                    AstGenerator.MakeAssignment(right_temp.Variable, right)
                    );

                // Do left to right assignment
                foreach(Expression e in _left) {
                    if (e == null) {
                        continue;
                    }

                    // 3. e = right_temp
                    MSAst.Expression transformed = e.TransformSet(ag, Span, right_temp, Operators.None);
                    if (transformed != null) {
                        statements.Add(transformed);
                    }
                }

                // 4. Create and return the resulting suite
                return Ast.Block(
                    Span,
                    statements.ToArray()
                );
            }
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                foreach (Expression e in _left) {
                    e.Walk(walker);
                }
                _right.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }
}
