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
using Microsoft.Scripting.Internal.Generation;
using MSAst = Microsoft.Scripting.Internal.Ast;
using VariableKind = Microsoft.Scripting.Internal.Ast.Variable.VariableKind;

namespace IronPython.Compiler.Ast {
    public class PythonAst : ScopeStatement {
        private Statement _body;
        private bool _module;

        private PythonReference _doc;
        private PythonReference _name;

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

        public PythonReference DocReference {
            get { return _doc; }
            set { _doc = value; }
        }

        public PythonReference NameReference {
            get { return _name; }
            set { _name = value; }
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
                        block.AddVariable(kv.Value.Transform(block));
                    }
                }
            }
        }

        // TODO: Rename !!! (BindNonGlobalFreeVariable) or something
        internal PythonVariable EnsureGlobalVariable(SymbolId name) {
            PythonVariable variable;
            if (TryGetDefinition(name, out variable)) {
                // use the current one if it is global only
                if (variable.Kind == VariableKind.Global) {
                    return variable;
                }
            }

            // TODO: Following code is almost the same as DefineModuleLocal, unify
            if (_moduleGlobals == null) {
                _moduleGlobals = new Dictionary<SymbolId, PythonVariable>();
            }
            if (!_moduleGlobals.TryGetValue(name, out variable)) {
                variable = new PythonVariable(name, this);
                variable.Kind = VariableKind.Global;
                _moduleGlobals[name] = variable;
            }
            Debug.Assert(variable.Kind == VariableKind.Global);
            return variable;
        }

        internal PythonVariable DefineModuleLocal(SymbolId name) {
            if (_moduleLocals == null) {
                _moduleLocals = new Dictionary<SymbolId, PythonVariable>();
            }
            PythonVariable variable;
            if (!_moduleLocals.TryGetValue(name, out variable)) {
                variable = new PythonVariable(name, this);
                variable.Kind = VariableKind.Local;
                _moduleLocals[name] = variable;
            }
            Debug.Assert(variable.Kind == VariableKind.Local);
            return variable;
        }

        internal override MSAst.Statement Transform(AstGenerator ag) {
            throw new InvalidOperationException();
        }

        internal MSAst.CodeBlock TransformToAst(AstGenerator ag, CompilerContext context) {
            if (_block == null) {
                string name;
                if (_name == null) {
                    name = context.SourceUnit.Name ?? "<undefined>";
                } else {
                    name = SymbolTable.IdToString(_name.Name);
                }

                MSAst.CodeBlock ast = MSAst.CodeBlock.MakeCodeBlock(name, null, _body.Span);
                ast.IsGlobal = true;

                _block = ast;

                // Publish the variables and references
                CreateVariables(ast);

                // Use the PrintExpression value for the body (global level code)
                AstGenerator body = new AstGenerator(ast, ag, ag.PrintExpressions);

                MSAst.Statement bodyStmt = body.Transform(_body);

                MSAst.Statement docStmt;
                if (_module && _body.Documentation != null) {
                    docStmt = new MSAst.ExpressionStatement(
                        new MSAst.BoundAssignment(
                            _doc.Reference,
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
            }
            return _block;
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
