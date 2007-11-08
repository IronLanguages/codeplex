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
using System.Collections.Generic;

using Microsoft.Scripting;
using Microsoft.Scripting.Generation;
using MSAst = Microsoft.Scripting.Ast;

using IronPython.Runtime;
using IronPython.Runtime.Calls;
using Microsoft.Scripting.Actions;

namespace IronPython.Compiler.Ast {
    using Ast = Microsoft.Scripting.Ast.Ast;
    using Microsoft.Scripting.Hosting;

    public class FunctionDefinition : ScopeStatement {
        protected Statement _body;
        private SourceLocation _header;
        private readonly SymbolId _name;
        private readonly Parameter[] _parameters;
        private IList<Expression> _decorators;
        private SourceUnit _sourceUnit;
        private bool _generator;                        // The function is a generator

        private PythonVariable _variable;               // The variable corresponding to the function name
        private MSAst.CodeBlock _block;

        public FunctionDefinition(SymbolId name, Parameter[] parameters, SourceUnit sourceUnit)
            : this(name, parameters, null, sourceUnit) {
        }

        public FunctionDefinition(SymbolId name, Parameter[] parameters, Statement body, SourceUnit sourceUnit) {
            _name = name;
            _parameters = parameters;
            _body = body;
            _sourceUnit = sourceUnit;
        }

        public Parameter[] Parameters {
            get { return _parameters; }
        }

        public Statement Body {
            get { return _body; }
            set { _body = value; }
        }

        public SourceLocation Header {
            get { return _header; }
            set { _header = value; }
        }

        public SymbolId Name {
            get { return _name; }
        }

        public IList<Expression> Decorators {
            get { return _decorators; }
            set { _decorators = value; }
        }

        public bool IsGenerator {
            get { return _generator; }
            set { _generator = value; }
        }

        internal PythonVariable Variable {
            get { return _variable; }
            set { _variable = value; }
        }

        protected override MSAst.CodeBlock Block {
            get { return _block; }
        }

        private static FunctionAttributes ComputeFlags(Parameter[] parameters) {
            FunctionAttributes fa = FunctionAttributes.None;
            if (parameters != null) {
                int i;
                for (i = 0; i < parameters.Length; i++) {
                    Parameter p = parameters[i];
                    if (p.IsDictionary || p.IsList) break;
                }
                // Check for the list and dictionary parameters, which must be the last(two)
                if (i < parameters.Length && parameters[i].IsList) {
                    i++;
                    fa |= FunctionAttributes.ArgumentList;
                }
                if (i < parameters.Length && parameters[i].IsDictionary) {
                    i++;
                    fa |= FunctionAttributes.KeywordDictionary;
                }
                // All parameters must now be exhausted
                if (i < parameters.Length) {
                    throw new ArgumentException(IronPython.Resources.InvalidParameters, "parameters");
                }
            }
            return fa;
        }

        internal override bool TryBindOuter(SymbolId name, out PythonVariable variable) {
            // Functions expose their locals to direct access
            ContainsNestedFreeVariables = true;
            return TryGetVariable(name, out variable);
        }

        internal override PythonVariable BindName(SymbolId name) {
            PythonVariable variable;

            // First try variables local to this scope
            if (TryGetVariable(name, out variable)) {
                return variable;
            }

            // Try to bind in outer scopes
            for (ScopeStatement parent = Parent; parent != null; parent = parent.Parent) {
                if (parent.TryBindOuter(name, out variable)) {
                    IsClosure = true;
                    return variable;
                }
            }

            // Unbound variable
            if (ContainsUnqualifiedExec | ContainsImportStar | NeedsLocalsDictionary) {
                // If the context contains unqualified exec, new locals can be introduced
                // We introduce the locals for every free variable to optimize the name based
                // lookup.
                EnsureHiddenVariable(name);
                return null;
            } else {
                // Create a global variable to bind to.
                return GetGlobalScope().EnsureGlobalVariable(name);
            }
        }

        internal override void Bind(PythonNameBinder binder) {
            base.Bind(binder);
            Verify(binder);
        }

        private void Verify(PythonNameBinder binder) {
            if (ContainsImportStar && IsClosure) {
                binder.ReportSyntaxError(
                    String.Format(
                        System.Globalization.CultureInfo.InvariantCulture,
                        "import * is not allowed in function '{0}' because it is a nested function",
                        SymbolTable.IdToString(Name)),
                    this);
            }
            if (ContainsImportStar && Parent is FunctionDefinition) {
                binder.ReportSyntaxError(
                    String.Format(
                        System.Globalization.CultureInfo.InvariantCulture,
                        "import * is not allowed in function '{0}' because it is a nested function",
                        SymbolTable.IdToString(Name)),
                    this);
            }
            if (ContainsImportStar && ContainsNestedFreeVariables) {
                binder.ReportSyntaxError(
                    String.Format(
                        System.Globalization.CultureInfo.InvariantCulture,
                        "import * is not allowed in function '{0}' because it contains a nested function with free variables",
                        SymbolTable.IdToString(Name)),
                    this);
            }
            if (ContainsUnqualifiedExec && ContainsNestedFreeVariables) {
                binder.ReportSyntaxError(
                    String.Format(
                        System.Globalization.CultureInfo.InvariantCulture,
                        "unqualified exec is not allowed in function '{0}' because it contains a nested function with free variables",
                        SymbolTable.IdToString(Name)),
                    this);
            }
            if (ContainsUnqualifiedExec && IsClosure) {
                binder.ReportSyntaxError(
                    String.Format(
                        System.Globalization.CultureInfo.InvariantCulture,
                        "unqualified exec is not allowed in function '{0}' because it is a nested function",
                        SymbolTable.IdToString(Name)),
                    this);
            }
        }

        internal override MSAst.Statement Transform(AstGenerator ag) {
            MSAst.Expression function = TransformToFunctionExpression(ag);
            return Ast.Statement(
                new SourceSpan(Start, Header),
                Ast.Assign(_variable.Variable, function)
            );
        }

        internal MSAst.Expression TransformToFunctionExpression(AstGenerator ag) {
            Debug.Assert(_block == null);
            MSAst.CodeBlock code;
            if (IsGenerator) {
                code = Ast.Generator(
                    SourceSpan.None,
                    SymbolTable.IdToString(_name),
                    typeof(PythonGenerator),
                    typeof(PythonGenerator.NextTarget)
                );
            } else {
                code = Ast.CodeBlock(_name);
            }
            _block = code; //???

            SetParent(code);

            // Create AST generator to generate the body with
            AstGenerator bodyGen = new AstGenerator(code, ag.Context);

            // Transform the parameters.
            // Populate the list of the parameter names and defaults.
            List<MSAst.Expression> defaults = new List<MSAst.Expression>();
            List<MSAst.Expression> names = new List<MSAst.Expression>();
            TransformParameters(ag, bodyGen, defaults, names);

            // Create variables and references. Since references refer to
            // parameters, do this after parameters have been created.
            CreateVariables(code);

            // Initialize parameters - unpack tuples.
            // Since tuples unpack into locals, this must be done after locals have been created.
            List<MSAst.Statement> statements = new List<MSAst.Statement>();
            InitializeParameters(bodyGen, statements);

            // Transform the body and add the resulting statements into the list
            TransformBody(bodyGen, statements);

            if (ScriptDomainManager.Options.DebugMode) {
                // add beginning and ending break points for the function.
                if (statements.Count == 0 || statements[0].Start != Body.Start) {
                    statements.Insert(0, Ast.Empty(new SourceSpan(Body.Start, Body.Start)));
                }

                if (statements[statements.Count - 1].End != Body.End) {
                    statements.Add(Ast.Empty(new SourceSpan(Body.End, Body.End)));
                }
            }

            code.Body = Ast.Block(statements);

            FunctionAttributes flags = ComputeFlags(_parameters);

            MSAst.Expression ret = Ast.Call(
                typeof(PythonFunction).GetMethod("MakeFunction"),                               // method
                Ast.CodeContext(),                                                              // 1. Emit CodeContext
                Ast.Constant(SymbolTable.IdToString(_name)),                                    // 2. FunctionName
                Ast.CodeBlockExpression(code, flags != FunctionAttributes.None),                // 3. delegate
                Ast.NewArray(typeof(string[]), names),                                          // 4. parameter names
                Ast.NewArray(typeof(object[]), defaults),                                       // 5. default values
                Ast.Constant(flags),                                                            // 6. flags
                Ast.Constant(_body.Documentation, typeof(string)),                              // 7. doc string or null
                Ast.Constant(this.Start.Line),                                                  // 8. line number
                Ast.Constant(_sourceUnit.GetSymbolDocument(this.Start.Line), typeof(string))    // 9. filename
            );

            // add decorators
            if (_decorators != null) {
                for (int i = _decorators.Count - 1; i >= 0; i--) {
                    Expression decorator = _decorators[i];
                    ret = Ast.Action.Call(
                        typeof(object),
                        ag.Transform(decorator),
                        ret);
                }
            }

            return ret;
        }

        private void TransformParameters(AstGenerator outer, AstGenerator inner, List<MSAst.Expression> defaults, List<MSAst.Expression> names) {
            for (int i = 0; i < _parameters.Length; i++) {
                // Create the parameter in the inner code block
                Parameter p = _parameters[i];
                p.Transform(inner);

                // Transform the default value
                if (p.DefaultValue != null) {
                    defaults.Add(
                        outer.TransformAndConvert(p.DefaultValue, typeof(object))
                    );
                }

                names.Add(
                    Ast.Constant(
                        SymbolTable.IdToString(p.Name)
                    )
                );
            }
        }

        private void InitializeParameters(AstGenerator ag, List<MSAst.Statement> init) {
            foreach (Parameter p in _parameters) {
                p.Init(ag, init);
            }
        }

        private void TransformBody(AstGenerator ag, List<MSAst.Statement> statements) {
            SuiteStatement suite = _body as SuiteStatement;

            // Special case suite statement to avoid unnecessary allocation of extra node.
            if (suite != null) {
                foreach (Statement one in suite.Statements) {
                    MSAst.Statement transforned = ag.Transform(one);
                    if (transforned != null) {
                        statements.Add(transforned);
                    }
                }
            } else {
                MSAst.Statement transformed = ag.Transform(_body);
                if (transformed != null) {
                    statements.Add(transformed);
                }
            }
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_parameters != null) {
                    foreach (Parameter p in _parameters) {
                        p.Walk(walker);
                    }
                }
                if (_decorators != null) {
                    foreach (Expression decorator in _decorators) {
                        decorator.Walk(walker);
                    }
                }
                if (_body != null) {
                    _body.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }
    }
}
