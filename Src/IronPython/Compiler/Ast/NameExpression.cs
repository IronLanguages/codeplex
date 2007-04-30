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

using Microsoft.Scripting;
using MSAst = Microsoft.Scripting.Internal.Ast;

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

        public PythonReference Reference {
            get { return _reference; }
            set { _reference = value; }
        }

        public override string ToString() {
            return base.ToString() + ":" + SymbolTable.IdToString(_name);
        }

        internal override MSAst.Expression Transform(AstGenerator ag) {
            return new MSAst.BoundExpression(_reference.Reference, Span);
        }

        internal override MSAst.Statement TransformSet(AstGenerator ag, MSAst.Expression right, Operators op) {
            return new MSAst.ExpressionStatement(
                new MSAst.BoundAssignment(
                    _reference.Reference,
                    right,
                    op
                ),
                right.Span.IsValid ? new SourceSpan(Span.Start, right.End) : SourceSpan.None
            );
        }

        internal override MSAst.Statement TransformDelete(AstGenerator ag) {
            return new MSAst.DelStatement(
                _reference.Reference,
                Span
            );
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
            }
            walker.PostWalk(this);
        }
    }
}
