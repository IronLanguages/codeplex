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
using System.Collections.Generic;

using Microsoft.Scripting;
using Microsoft.Scripting.Internal;

using IronPython.Runtime;
using IronPython.Runtime.Calls;
using MSAst = Microsoft.Scripting.Internal.Ast;
using Microsoft.Scripting.Internal.Generation;

namespace IronPython.Compiler.Ast {
    public class FunctionDefinition : ScopeStatement {
        protected Statement _body;
        private SourceLocation _header;
        private readonly SymbolId _name;
        private readonly Parameter[] _parameters;
        private IList<Expression> _decorators;
        private SourceUnit _sourceUnit;
        private bool _generator;                        // The function is a generator

        private PythonReference _reference;             // The reference corresponding to the function name
        private MSAst.CodeBlock _block;

        public FunctionDefinition(SymbolId name, Parameter[] parameters, SourceUnit sourceUnit)
            : this(name, parameters, null, sourceUnit) {
        }

        public FunctionDefinition(SymbolId name, Parameter[] parameters, Statement body, SourceUnit sourceUnit) {
            //ValidateParameters(parameters);

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

        public PythonReference Reference {
            get { return _reference; }
            set { _reference = value; }
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

        internal override MSAst.Statement Transform(AstGenerator ag) {
            MSAst.Expression ret = TransformToFunctionExpression(ag);
            return new MSAst.ExpressionStatement(
                new MSAst.BoundAssignment(_reference.Reference, ret, Operators.None),
                new SourceSpan(Start, Header)
            );
        }

        internal MSAst.Expression TransformToFunctionExpression(AstGenerator ag) {
            Debug.Assert(_block == null);
            MSAst.CodeBlock code;
            if (IsGenerator) {
                code = new MSAst.GeneratorCodeBlock(
                    SymbolTable.IdToString(_name),
                    null,
                    null,
                    typeof(PythonGenerator),
                    typeof(PythonGenerator.NextTarget),
                    Span
                );
            } else {
                code = new MSAst.CodeBlock(SymbolTable.IdToString(_name), null, null, Span);
            }
            _block = code; //???

            SetParent(code);

            // Create AST generator to generate the body with
            AstGenerator bodyGen = new AstGenerator(code, ag);

            // Transform the parameters, should any require initialization,
            // it will be added into the list
            List<MSAst.Statement> statements = new List<MSAst.Statement>();
            code.Parameters = TransformParameters(ag, bodyGen);

            // Create variables and references. Since references refer to
            // parameters, do this after parameters have been created.
            CreateVariables(code);

            // Initialize parameters - unpack tuples.
            // Since tuples unpack into locals, this must be done after locals have been created.
            InitializeParameters(bodyGen, statements);

            // Transform the body and add the resulting statements into the list
            TransformBody(bodyGen, statements);

            code.Body = new MSAst.BlockStatement(statements.ToArray());

            FunctionAttributes flags = ComputeFlags(_parameters);

            List<MSAst.Expression> defaults = new List<MSAst.Expression>();
            List<MSAst.Expression> names = new List<MSAst.Expression>();
            // There's a weird question about where to get these
            foreach (MSAst.Parameter p in code.Parameters) {
                names.Add(new MSAst.ConstantExpression(SymbolTable.IdToString(p.Name)));
                if (p.DefaultValue != null) defaults.Add(p.DefaultValue);
            }

            FunctionCode.FuncCodeFlags codeFlags = 0;
            if (_generator) codeFlags |= FunctionCode.FuncCodeFlags.Generator;

            string filename = CompilerHelpers.GetSourceDisplayName(_sourceUnit);

            MSAst.Expression ret = MSAst.MethodCallExpression.Call(
                new SourceSpan(Start, Header),
                null,                                                                   // instance
                typeof(PythonFunction).GetMethod("MakeFunction"),                       // method
                new MSAst.CodeContextExpression(),                                      // 1. Emit CodeContext
                new MSAst.ConstantExpression(SymbolTable.IdToString(_name)),            // 2. FunctionName
                new MSAst.CodeBlockExpression(code, flags != FunctionAttributes.None),  // 3. delegate
                MSAst.NewArrayExpression.NewArrayInit(typeof(string[]), names),         // 4. parameter names
                MSAst.NewArrayExpression.NewArrayInit(typeof(object[]), defaults),      // 5. default values
                new MSAst.ConstantExpression(flags),                                    // 6. flags
                new MSAst.ConstantExpression(_body.Documentation),                      // 7. doc string or null
                new MSAst.ConstantExpression(this.Start.Line),                          // 8. line number
                new MSAst.ConstantExpression(filename),                                 // 9. filename
                new MSAst.ConstantExpression((int)codeFlags)                            // 10. code flags
                );

            // add decorators
            if (_decorators != null) {
                for (int i = _decorators.Count - 1; i >= 0; i--) {
                    Expression decorator = _decorators[i];
                    ret = new MSAst.CallExpression(
                        ag.Transform(decorator),
                        new MSAst.Arg[] {
                            MSAst.Arg.Simple(ret)
                        },
                        false, false, 0, 0);
                }
            }

            return ret;
        }

        private MSAst.Parameter[] TransformParameters(AstGenerator outer, AstGenerator inner) {
            MSAst.Parameter[] to = new MSAst.Parameter[_parameters.Length];

            for (int i = 0; i < _parameters.Length; i++) {
                to[i] = _parameters[i].Transform(outer, inner);
            }

            return to;
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
