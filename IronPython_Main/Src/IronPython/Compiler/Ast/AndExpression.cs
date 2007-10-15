/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Diagnostics;

using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using MSAst = Microsoft.Scripting.Ast;

namespace IronPython.Compiler.Ast {
    using Ast = Microsoft.Scripting.Ast.Ast;

    public class AndExpression : Expression {
        private readonly Expression _left, _right;

        public AndExpression(Expression left, Expression right) {
            Contract.RequiresNotNull(left, "left");
            Contract.RequiresNotNull(right, "right");

            _left = left;
            _right = right;
            Start = left.Start;
            End = right.End;
        }

        public Expression Left {
            get { return _left; }
        }

        public Expression Right {
            get { return _right; }
        } 

        internal override MSAst.Expression Transform(AstGenerator ag, Type type) {
            return Ast.CoalesceTrue(ag.Block,
                Ast.ConvertHelper(ag.Transform(_left), typeof(object)),
                Ast.ConvertHelper(ag.Transform(_right), typeof(object)),
                AstGenerator.GetHelperMethod("IsTrue")
            );
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_left != null) {
                    _left.Walk(walker);
                }
                if (_right != null) {
                    _right.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }
    }
}
