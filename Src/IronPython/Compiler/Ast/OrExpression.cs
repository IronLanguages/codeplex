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

using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Actions;

using IronPython.Runtime;

using MSAst = Microsoft.Scripting.Ast;

namespace IronPython.Compiler.Ast {
    using Ast = Microsoft.Scripting.Ast.Expression;

    public class OrExpression : Expression {
        private readonly Expression _left, _right;

        public OrExpression(Expression left, Expression right) {
            ContractUtils.RequiresNotNull(left, "left");
            ContractUtils.RequiresNotNull(right, "right");

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
            MSAst.Expression left = ag.Transform(_left);
            MSAst.Expression right = ag.Transform(_right);

            Type t = left.Type == right.Type ? left.Type : typeof(object);
            MSAst.VariableExpression tmp = ag.MakeTemp(Symbols.All, t);
            
            return Ast.Condition(
                Ast.Action.ConvertTo(
                    ConvertToAction.Make(ag.Binder, typeof(bool), ConversionResultKind.ExplicitCast),
                    Ast.Assign(                        
                        tmp,
                        Ast.ConvertHelper(
                            left,
                            t
                        )
                    )
                ),
                Ast.Read(tmp),
                Ast.ConvertHelper(
                    right,
                    t
                )
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
