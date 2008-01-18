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

using IronPython.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Generation;
using MSAst = Microsoft.Scripting.Ast;
using VariableKind = Microsoft.Scripting.Ast.Variable.VariableKind;

namespace IronPython.Compiler.Ast {
    using Ast = Microsoft.Scripting.Ast.Ast;
    using Microsoft.Scripting.Utils;

    public class PythonAst : ScopeStatement {
        private readonly Statement _body;
        private readonly bool _isModule;
        private readonly bool _trueDivision;
        private readonly bool _printExpressions;

        private PythonVariable _docVariable;

        /// <summary>
        /// The globals that free variables in the functions bind to,
        /// then need to be separated into their own dictionary so that
        /// free variables at module level do not bind to them
        /// </summary>
        private Dictionary<SymbolId, PythonVariable> _globals;

        private MSAst.CodeBlock _block;

        /// <summary>
        /// True division is enabled in this AST.
        /// </summary>
        public bool TrueDivision {
            get { return _trueDivision; }
        }

        /// <summary>
        /// Interactive code: expression statements print their value.
        /// </summary>
        public bool PrintExpressions {
            get { return _printExpressions; }
        }

        public PythonAst(Statement body, bool isModule, bool trueDivision, bool printExpressions) {
            Contract.RequiresNotNull(body, "body");

            _body = body;
            _isModule = isModule;
            _trueDivision = trueDivision;
            _printExpressions = printExpressions;
        }

        public Statement Body {
            get { return _body; }
        }

        public bool Module {
            get { return _isModule; }
        }

        internal PythonVariable DocVariable {
            get { return _docVariable; }
            set { _docVariable = value; }
        }

        protected override MSAst.CodeBlock Block {
            get { return _block; }
        }

        internal override bool IsGlobal {
            get { return true; }
        }

        protected override void CreateVariables(MSAst.CodeBlock block) {
            if (_globals != null) {
                foreach (KeyValuePair<SymbolId, PythonVariable> kv in _globals) {
                    if (kv.Value.Scope == this) {
                        kv.Value.Transform(block);
                    }
                }
            }

            base.CreateVariables(block);
        }

        internal PythonVariable EnsureGlobalVariable(PythonNameBinder binder, SymbolId name) {
            PythonVariable variable;
            if (TryGetVariable(name, out variable)) {
                // use the current one if it is global only
                if (variable.Kind == VariableKind.Global) {
                    return variable;
                }
            }

            if ((binder.Module & ModuleOptions.Optimized) == 0) {
                // For non-optimized modules, keep globals separate
                if (_globals == null) {
                    _globals = new Dictionary<SymbolId, PythonVariable>();
                }
                if (!_globals.TryGetValue(name, out variable)) {
                    variable = new PythonVariable(name, VariableKind.Global, this);
                    _globals[name] = variable;
                }
            } else {
                variable = EnsureVariable(name);
            }
            return variable;
        }

        internal override PythonVariable BindName(PythonNameBinder binder, SymbolId name) {
            return EnsureVariable(name);
        }

        internal override MSAst.Expression Transform(AstGenerator ag) {
            throw new InvalidOperationException();
        }

        internal MSAst.CodeBlock TransformToAst(AstGenerator ag, CompilerContext context) {
            string name = context.SourceUnit.HasPath ? context.SourceUnit.Id : "<undefined>";
            MSAst.CodeBlock ast = Ast.CodeBlock(_body.Span, name);
            ast.IsGlobal = true;

            _block = ast;

            // Create the variables
            CreateVariables(ast);

            // Use the PrintExpression value for the body (global level code)
            AstGenerator body = new AstGenerator(ast, ag.Context, _printExpressions);

            MSAst.Expression bodyStmt = body.Transform(_body);            

            MSAst.Expression docStmt;

            if (_isModule && _body.Documentation != null) {
                docStmt = Ast.Statement(
                    Ast.Assign(
                        _docVariable.Variable,
                        Ast.Constant(_body.Documentation)
                    )
                );
            } else {
                docStmt = Ast.Empty();
            }

            ast.Body = Ast.Block(
                docStmt,
                bodyStmt ?? Ast.Empty() //  bodyStmt could be null if we have an error - e.g. a top level break
            );
            return ast;
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_body != null) {
                    _body.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }
    }
}
