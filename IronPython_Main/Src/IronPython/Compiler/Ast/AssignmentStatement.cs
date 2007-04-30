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
using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.Scripting;
using Microsoft.Scripting.Internal;

using MSAst = Microsoft.Scripting.Internal.Ast;

using IronPython.Runtime.Operations;

namespace IronPython.Compiler.Ast {
    public class AssignmentStatement : Statement {
        // _left.Length is 1 for simple assignments like "x = 1"
        // _left.Lenght will be 3 for "x = y = z = 1"
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

        internal override MSAst.Statement Transform(AstGenerator ag) {
            // Transform right
            MSAst.Expression right = _right.Transform(ag);

            if (_left.Length == 1) {
                // Do not need temps for simple assignment
                return _left[0].TransformSet(ag, right, Operators.None);
            } else {
                List<MSAst.Statement> statements = new List<MSAst.Statement>();

                // 1. Create temp variable for the right value
                MSAst.BoundExpression right_temp = ag.MakeTempExpression("assignment", right.Span);

                // 2. right_temp = right
                statements.Add(
                    AstGenerator.MakeAssignment(right_temp.Reference, right, right.Span)
                    );

                for (int index = _left.Length - 1; index >= 0; index--) {
                    Expression e = _left[index];
                    if (e == null) {
                        continue;
                    }

                    // 3. e = right_temp
                    MSAst.Statement transformed = e.TransformSet(ag, right_temp, Operators.None);
                    if (transformed != null) {
                        statements.Add(transformed);
                    }
                }

                // 4. Create and return the resulting suite
                return new MSAst.BlockStatement(
                    statements.ToArray(),
                    Span
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
