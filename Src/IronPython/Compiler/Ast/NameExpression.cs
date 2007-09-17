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
using MSAst = Microsoft.Scripting.Ast;

namespace IronPython.Compiler.Ast {
    using Ast = Microsoft.Scripting.Ast.Ast;

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
                return Ast.Read(Span, variable);
            } else {
                return Ast.Read(Span, _name);
            }
        }

        internal override MSAst.Statement TransformSet(AstGenerator ag, MSAst.Expression right, Operators op) {
            MSAst.Variable variable;
            MSAst.Expression assignment;

            if ((variable = _reference.Variable) != null) {
                assignment = Ast.Assign(variable, right, op);
            } else {
                assignment = Ast.Assign(_name, right, op);
            }

            return Ast.Statement(
                right.Span.IsValid ? new SourceSpan(Span.Start, right.End) : SourceSpan.None,
                assignment
            );
        }

        internal override MSAst.Statement TransformDelete(AstGenerator ag) {
            MSAst.Variable variable;
            if ((variable = _reference.Variable) != null) {
                return Ast.Delete(Span, variable);
            } else {
                return Ast.Statement(Span, Ast.Delete(_name));
            }
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
            }
            walker.PostWalk(this);
        }
    }
}
