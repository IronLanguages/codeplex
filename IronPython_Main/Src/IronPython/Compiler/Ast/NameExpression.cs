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
using System.Diagnostics;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using MSAst = Microsoft.Scripting.Ast;

namespace IronPython.Compiler.Ast {
    using Ast = Microsoft.Scripting.Ast.Ast;

    public class NameExpression : Expression {
        private readonly SymbolId _name;
        private PythonReference _reference;
        private bool _assigned;                  // definitely assigned

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

        internal bool Assigned {
            get { return _assigned; }
            set { _assigned = value; }
        }

        public override string ToString() {
            return base.ToString() + ":" + SymbolTable.IdToString(_name);
        }

        internal override MSAst.Expression Transform(AstGenerator ag, Type type) {
            MSAst.Expression read = _reference.Variable;
            if (read == null) {
                read = Ast.Read(_name);
            }

            if (!_assigned) {
                read = Ast.Call(
                    AstGenerator.GetHelperMethod("CheckUninitialized"),
                    read,
                    Ast.Constant(_name)
                );
            }

            return read;
        }

        internal override MSAst.Expression TransformSet(AstGenerator ag, SourceSpan span, MSAst.Expression right, Operators op) {
            MSAst.Expression variable = _reference.Variable;
            MSAst.Expression assignment;

            Type vt = variable != null ? variable.Type : typeof(object);

            if (op != Operators.None) {
                right = Ast.Action.Operator(op, vt, Transform(ag, vt),  right);
            }

            if (variable != null) {
                assignment = Ast.Assign(variable, AstGenerator.ConvertIfNeeded(right, variable.Type));
            } else {
                assignment = Ast.Assign(_name, right);
            }

            return Ast.Statement(
                span.IsValid ? new SourceSpan(Span.Start, span.End) : SourceSpan.None,
                Ast.Void(assignment)
            );
        }

        internal override MSAst.Expression TransformDelete(AstGenerator ag) {
            MSAst.Expression variable;
            if ((variable = _reference.Variable) != null) {
                MSAst.Expression del = Ast.Delete(Span, variable);
                if (!_assigned) {
                    del = Ast.Block(
                        Transform(ag, variable.Type),
                        del
                    );
                }
                return del;
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
