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

using IronPython.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Generation;
using MSAst = Microsoft.Scripting.Ast;
using VariableKind = Microsoft.Scripting.Ast.Variable.VariableKind;

namespace IronPython.Compiler.Ast {
    public class PythonAst : ScopeStatement {
        private Statement _body;
        private bool _module;
        private PythonVariable _docVariable;

        /// <summary>
        /// The globals that free variables in the functions bind to,
        /// then need to be separated into their own dictionary so that
        /// free variables at module level do not bind to them
        /// </summary>
        private Dictionary<SymbolId, PythonVariable> _moduleGlobals;

        /// <summary>
        /// 
        /// </summary>
        private Dictionary<SymbolId, PythonVariable> _moduleLocals;

        private MSAst.CodeBlock _block;

        public PythonAst(Statement body, bool module) {
            _body = body;
            _module = module;
        }

        public Statement Body {
            get { return _body; }
        }

        public bool Module {
            get { return _module; }
        }

        internal PythonVariable DocVariable {
            get { return _docVariable; }
            set { _docVariable = value; }
        }

        protected override MSAst.CodeBlock Block {
            get { return _block; }
        }

        protected override void CreateVariables(MSAst.CodeBlock block) {
            PublishGlobalVariables(block, _moduleLocals);
            PublishGlobalVariables(block, _moduleGlobals);

            base.CreateVariables(block);
        }

        private void PublishGlobalVariables(MSAst.CodeBlock block, Dictionary<SymbolId, PythonVariable> variables) {
            if (variables != null) {
                foreach (KeyValuePair<SymbolId, PythonVariable> kv in variables) {
                    if (kv.Value.Scope == this) {
                        kv.Value.Transform(block);
                    }
                }
            }
        }

        internal PythonVariable EnsureGlobalVariable(SymbolId name, bool transform) {
            PythonVariable variable;
            if (TryGetVariable(name, out variable)) {
                // use the current one if it is global only
                if (variable.Kind == VariableKind.Global) {
                    return variable;
                }
            }

            return CreateModuleVariable(ref _moduleGlobals, name, VariableKind.Global, transform);
        }

        internal override PythonVariable BindName(SymbolId name) {
            PythonVariable variable;

            // First try variables local to this scope
            if (TryGetVariable(name, out variable)) {
                return variable;
            }

            // Create module level local for the unbound name
            return CreateModuleVariable(ref _moduleLocals, name, VariableKind.Local, false);
        }

        /// <summary>
        /// This method is called in name binding phase, in which case the variables don't need to be
        /// immediately transformed, and during the transformation phase, in which case the variables
        /// owned by the given scope have already been transformed so any additional variable needs
        /// to be transformed explicitly here.
        /// </summary>
        private PythonVariable CreateModuleVariable(ref Dictionary<SymbolId, PythonVariable> dict, SymbolId name, VariableKind kind, bool transform) {
            PythonVariable variable;
            if (dict == null) {
                dict = new Dictionary<SymbolId, PythonVariable>();
            }
            if (!dict.TryGetValue(name, out variable)) {
                variable = new PythonVariable(name, kind, this);
                if (transform) {
                    // If called during the transformation phase, we have the _block
                    Debug.Assert(_block != null);
                    variable.Transform(_block);
                }
                dict[name] = variable;
            }
            Debug.Assert(variable.Kind == kind);
            return variable;
        }

        internal override MSAst.Statement Transform(AstGenerator ag) {
            throw new InvalidOperationException();
        }

        internal MSAst.CodeBlock TransformToAst(AstGenerator ag, CompilerContext context) {
            string name = context.SourceUnit.Name ?? "<undefined>";
            MSAst.CodeBlock ast = MSAst.CodeBlock.MakeCodeBlock(name, _body.Span);
            ast.IsGlobal = true;

            _block = ast;

            // Create the variables
            CreateVariables(ast);

            // Use the PrintExpression value for the body (global level code)
            AstGenerator body = new AstGenerator(ast, ag.Context, ag.PrintExpressions);

            MSAst.Statement bodyStmt = body.Transform(_body);
            MSAst.Statement docStmt;

            if (_module && _body.Documentation != null) {
                docStmt = new MSAst.ExpressionStatement(
                    new MSAst.BoundAssignment(
                        _docVariable.Variable,
                        new MSAst.ConstantExpression(_body.Documentation),
                        Operators.None
                    )
                );
            } else {
                docStmt = new MSAst.EmptyStatement();
            }

            ast.Body = new MSAst.BlockStatement(
                new MSAst.Statement[] {
                    docStmt,
                    bodyStmt,
                }
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
