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

using IronPython.Runtime;

using Microsoft.Scripting;
using MSAst = Microsoft.Scripting.Ast;

namespace IronPython.Compiler.Ast {
    using Ast = Microsoft.Scripting.Ast.Ast;
    using Microsoft.Scripting.Utils;

    public class ClassDefinition : ScopeStatement {
        private SourceLocation _header;
        private readonly SymbolId _name;
        private readonly Statement _body;
        private readonly Expression[] _bases;

        private PythonVariable _variable;           // Variable corresponding to the class name
        private PythonVariable _modVariable;        // Variable for the the __module__ (module name)
        private PythonVariable _docVariable;        // Variable for the __doc__ attribute
        private PythonVariable _modNameVariable;    // Variable for the module's __name__

        public ClassDefinition(SymbolId name, Expression[] bases, Statement body) {
            _name = name;
            _bases = bases;
            _body = body;
        }

        public SourceLocation Header {
            get { return _header; }
            set { _header = value; }
        }

        public SymbolId Name {
            get { return _name; }
        }

        public Expression[] Bases {
            get { return _bases; }
        }

        public Statement Body {
            get { return _body; }
        }

        internal PythonVariable Variable {
            get { return _variable; }
            set { _variable = value; }
        }

        internal PythonVariable ModVariable {
            get { return _modVariable; }
            set { _modVariable = value; }
        }

        internal PythonVariable DocVariable {
            get { return _docVariable; }
            set { _docVariable = value; }
        }

        internal PythonVariable ModuleNameVariable {
            get { return _modNameVariable; }
            set { _modNameVariable = value; }
        }

        internal override PythonVariable BindName(PythonNameBinder binder, SymbolId name) {
            PythonVariable variable;

            // Python semantics: The variables bound local in the class
            // scope are accessed by name - the dictionary behavior of classes
            if (TryGetVariable(name, out variable)) {
                return variable.Kind == MSAst.VariableKind.Local ? null : variable;
            }

            // Try to bind in outer scopes
            for (ScopeStatement parent = Parent; parent != null; parent = parent.Parent) {
                if (parent.TryBindOuter(name, out variable)) {
                    return variable;
                }
            }

            return null;
        }

        internal override MSAst.Expression Transform(AstGenerator ag) {
            MSAst.Expression bases = Ast.NewArray(
                typeof(object[]),
                ag.TransformAndConvert(_bases, typeof(object))
            );

            AstGenerator body = new AstGenerator(ag, SourceSpan.None, SymbolTable.IdToString(_name), false, false);

            CreateVariables(body);

            // Create the body
            MSAst.Expression bodyStmt = body.Transform(_body);
            MSAst.Expression modStmt =
                Ast.Assign(
                    _modVariable.Variable,
                    Ast.Read(_modNameVariable.Variable)
                );

            MSAst.Expression docStmt;
            string doc = ag.GetDocumentation(_body);
            if (doc != null) {
                docStmt =
                    Ast.Assign(
                        _docVariable.Variable,
                        Ast.Constant(doc)
                    );
            } else {
                docStmt = Ast.Empty();
            }

            MSAst.Expression returnStmt = Ast.Return(Ast.CodeContext());

            body.Block.Dictionary = true;
            body.Block.Visible = false;
            body.Block.Body = Ast.Block(
                modStmt,
                docStmt,
                bodyStmt,
                returnStmt
            );

            MSAst.LambdaExpression block = body.Block.MakeLambda();
            MSAst.Expression classDef = Ast.Call(
                AstGenerator.GetHelperMethod("MakeClass"),
                Ast.CodeContext(),
                Ast.Constant(SymbolTable.IdToString(_name)),
                bases,
                Ast.Constant(FindSelfNames()),
                Ast.CodeBlockExpression(block, typeof(IronPython.Runtime.Calls.CallTarget0))
            );

            return Ast.Statement(
                new SourceSpan(Start, Header),
                Ast.Assign(_variable.Variable, classDef)
            );
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_bases != null) {
                    foreach (Expression b in _bases) {
                        b.Walk(walker);
                    }
                }
                if (_body != null) {
                    _body.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }

        private string FindSelfNames() {
            SuiteStatement stmts = Body as SuiteStatement;
            if (stmts == null) return "";

            foreach (Statement stmt in stmts.Statements) {
                FunctionDefinition def = stmt as FunctionDefinition;
                if (def != null && def.Name == SymbolTable.StringToId("__init__")) {
                    return string.Join(",", SelfNameFinder.FindNames(def));
                }
            }
            return "";
        }

        private class SelfNameFinder : PythonWalker {
            private readonly FunctionDefinition _function;
            private readonly Parameter _self;

            public SelfNameFinder(FunctionDefinition function, Parameter self) {
                _function = function;
                _self = self;
            }

            public static string[] FindNames(FunctionDefinition function) {
                Parameter[] parameters = function.Parameters;

                if (parameters.Length > 0) {
                    SelfNameFinder finder = new SelfNameFinder(function, parameters[0]);
                    function.Body.Walk(finder);
                    return SymbolTable.IdsToStrings(new List<SymbolId>(finder._names.Keys));
                } else {
                    // no point analyzing function with no parameters
                    return ArrayUtils.EmptyStrings;
                }
            }

            private Dictionary<SymbolId, bool> _names = new Dictionary<SymbolId, bool>();

            private bool IsSelfReference(Expression expr) {
                NameExpression ne = expr as NameExpression;
                if (ne == null) return false;

                PythonVariable variable;
                if (_function.TryGetVariable(ne.Name, out variable) && variable == _self.Variable) {
                    return true;
                }

                return false;
            }

            // Don't recurse into class or function definitions
            public override bool Walk(ClassDefinition node) {
                return false;
            }
            public override bool Walk(FunctionDefinition node) {
                return false;
            }

            public override bool Walk(AssignmentStatement node) {
                foreach (Expression lhs in node.Left) {
                    MemberExpression me = lhs as MemberExpression;
                    if (me != null) {
                        if (IsSelfReference(me.Target)) {
                            _names[me.Name] = true;
                        }
                    }
                }
                return true;
            }
        }
    }
}
