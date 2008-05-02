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

namespace IronPython.Compiler.Ast {
    using Ast = Microsoft.Scripting.Ast.Expression;
    using Microsoft.Scripting.Utils;

    public class PythonAst : ScopeStatement {
        private readonly Statement _body;
        private readonly bool _isModule;
        private readonly bool _printExpressions;
        private readonly PythonLanguageFeatures _languageFeatures;
        private PythonVariable _docVariable;

        /// <summary>
        /// The globals that free variables in the functions bind to,
        /// then need to be separated into their own dictionary so that
        /// free variables at module level do not bind to them
        /// </summary>
        private Dictionary<SymbolId, PythonVariable> _globals;

        public PythonAst(Statement body, bool isModule, PythonLanguageFeatures languageFeatures, bool printExpressions) {
            ContractUtils.RequiresNotNull(body, "body");

            _body = body;
            _isModule = isModule;
            _printExpressions = printExpressions;
            _languageFeatures = languageFeatures;
        }

        /// <summary>
        /// True division is enabled in this AST.
        /// </summary>
        public bool TrueDivision {
            get { return (_languageFeatures & PythonLanguageFeatures.TrueDivision) != 0; }
        }

        /// <summary>
        /// True if the with statement is enabled in this AST.
        /// </summary>
        public bool AllowWithStatement {
            get {
                return (_languageFeatures & PythonLanguageFeatures.AllowWithStatement) != 0;
            }
        }

        /// <summary>
        /// True if absolute imports are enabled
        /// </summary>
        public bool AbsoluteImports {
            get {
                return (_languageFeatures & PythonLanguageFeatures.AbsoluteImports) != 0;
            }
        }

        /// <summary>
        /// Interactive code: expression statements print their value.
        /// </summary>
        public bool PrintExpressions {
            get { return _printExpressions; }
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

        internal override bool IsGlobal {
            get { return true; }
        }

        protected override bool ExposesLocalVariables {
            get { return true; }
        }

        internal override void CreateVariables(AstGenerator ag, List<MSAst.Expression> init) {
            if (_globals != null) {
                foreach (KeyValuePair<SymbolId, PythonVariable> kv in _globals) {
                    if (kv.Value.Scope == this) {
                        kv.Value.Transform(ag);
                    }
                }
            }

            base.CreateVariables(ag, init);
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
                    variable = new PythonVariable(name, typeof(object), VariableKind.Global, this);
                    _globals[name] = variable;
                }
            } else {
                variable = EnsureUnboundVariable(name);
            }
            return variable;
        }

        internal override PythonVariable BindName(PythonNameBinder binder, SymbolId name) {
            return EnsureVariable(name);
        }

        internal MSAst.LambdaExpression TransformToAst(CompilerContext context) {
            // Create the ast generator
            // Use the PrintExpression value for the body (global level code)
            PythonCompilerOptions pco = context.Options as PythonCompilerOptions;
            Debug.Assert(pco != null);

            string name;
            if (!context.SourceUnit.HasPath || (pco.Module & ModuleOptions.ExecOrEvalCode) != 0) {
                name = "<module>";
            } else {
                name = context.SourceUnit.Path;
            }

            AstGenerator ag = new AstGenerator(context, _body.Span, name, false, _printExpressions);            
            ag.Block.Global = true;

            ag.Block.Body = Ast.Block(
                Ast.Assign(ag.LineNumberExpression, Ast.Constant(0)),
                Ast.Assign(ag.LineNumberUpdated, Ast.Constant(false)),
                ag.WrapScopeStatements(Transform(ag))
            );
            return ag.Block.MakeLambda();
        }

        internal override MSAst.Expression Transform(AstGenerator ag) {
            List<MSAst.Expression> init = new List<MSAst.Expression>();
            // Create the variables
            CreateVariables(ag, init);

            MSAst.Expression bodyStmt = ag.Transform(_body);
            MSAst.Expression docStmt;

            string doc = ag.GetDocumentation(_body);

            if (_isModule && doc != null) {
                docStmt = Ast.Assign(
                    _docVariable.Variable,
                    Ast.Constant(doc)
                );
            } else {
                docStmt = Ast.Empty();
            }

            return Ast.Block(
                Ast.Block(init),
                docStmt,
                bodyStmt ?? Ast.Empty() //  bodyStmt could be null if we have an error - e.g. a top level break
            );
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
