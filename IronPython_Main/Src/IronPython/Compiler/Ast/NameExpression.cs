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
using System.Diagnostics;
using Microsoft.Scripting;
using MSAst = Microsoft.Scripting.Ast;

namespace IronPython.Compiler.Ast {
    public class NameExpression : Expression {
        private readonly SymbolId _name;
        private PythonReference _reference;

        public NameExpression(SymbolId name) {
            _name = name;
        }

        public SymbolId Name {
            get { return _name; }
        }

        internal PythonReference Reference {
            get { return _reference; }
            set { _reference = value; }
        }

        public override string ToString() {
            return base.ToString() + ":" + SymbolTable.IdToString(_name);
        }

        internal override MSAst.Expression Transform(AstGenerator ag, Type type) {
            MSAst.Variable variable;
            if ((variable = _reference.Variable) != null) {
                return new MSAst.BoundExpression(variable, Span);
            } else {
                return new MSAst.UnboundExpression(_name, Span);
            }
        }

        internal override MSAst.Statement TransformSet(AstGenerator ag, MSAst.Expression right, Operators op) {
            MSAst.Variable variable;
            MSAst.Expression assignment;

            if ((variable = _reference.Variable) != null) {
                assignment = new MSAst.BoundAssignment(variable, right, op);
            } else {
                assignment = new MSAst.UnboundAssignment(_name, right, op);
            }

            return new MSAst.ExpressionStatement(
                assignment,
                right.Span.IsValid ? new SourceSpan(Span.Start, right.End) : SourceSpan.None
            );
        }

        internal override MSAst.Statement TransformDelete(AstGenerator ag) {
            MSAst.Variable variable;
            if ((variable = _reference.Variable) != null) {
                return new MSAst.DelStatement(variable, Span);
            } else {
                return new MSAst.ExpressionStatement(new MSAst.DeleteUnboundExpression(_name), Span);
            }
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
            }
            walker.PostWalk(this);
        }
    }
}
