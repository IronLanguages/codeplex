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
using MSAst = Microsoft.Scripting.Internal.Ast;

namespace IronPython.Compiler.Ast {
    public class ClassDefinition : ScopeStatement {
        private SourceLocation _header;
        private readonly SymbolId _name;
        private readonly Statement _body;
        private readonly Expression[] _bases;

        private PythonReference _reference;   // reference corresponding to the class name

        private PythonReference _nameReference;       // reference to the __name__ in the global context (module name)
        private PythonReference _docReference;        // reference for the __doc__ attribute
        private PythonReference _modReference;        // reference for the __module__ attribute

        private MSAst.CodeBlock _block;

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

        public PythonReference Reference {
            set { _reference = value; }
        }

        public PythonReference NameReference {
            get { return _nameReference; }
            set { _nameReference = value; }
        }

        public PythonReference DocReference {
            set { _docReference = value; }
        }

        public PythonReference ModReference {
            set { _modReference = value; }
        }

        protected override MSAst.CodeBlock Block {
            get { return _block; }
        }

        internal override MSAst.Statement Transform(AstGenerator ag) {
            Debug.Assert(_block == null);

            MSAst.CodeBlock block = new MSAst.CodeBlock(SymbolTable.IdToString(_name), new MSAst.Parameter[0], null);
            block.IsVisible = false;

            _block = block;
            block.EmitLocalDictionary = true;

            MSAst.Expression bases = MSAst.NewArrayExpression.NewArrayInit(
                typeof(object[]),
                ag.Transform(_bases));

            SetParent(block);
            CreateVariables(block);

            // Create the body
            AstGenerator body = new AstGenerator(block, ag);
            MSAst.Statement bodyStmt = body.Transform(_body);
            MSAst.Statement modStmt = new MSAst.ExpressionStatement(
                new MSAst.BoundAssignment(
                    _modReference.Reference,
                    new MSAst.BoundExpression(_nameReference.Reference),
                    Operators.None
                )
            );

            MSAst.Statement docStmt;
            if (_body.Documentation != null) {
                docStmt = new MSAst.ExpressionStatement(
                    new MSAst.BoundAssignment(
                        _docReference.Reference,
                        new MSAst.ConstantExpression(_body.Documentation),
                        Operators.None
                    )
                );
            } else {
                docStmt = new MSAst.EmptyStatement();
            }
             
            MSAst.Statement returnStmt = new MSAst.ReturnStatement(new MSAst.CodeContextExpression());
            block.Body = new MSAst.BlockStatement(
                new MSAst.Statement[] {
                    modStmt,
                    docStmt,
                    bodyStmt,
                    returnStmt
                }
            );

            MSAst.Expression classDef = new MSAst.MethodCallExpression(
                AstGenerator.GetHelperMethod("MakeClass"),
                null,
                new MSAst.Expression[] {
                    new MSAst.CodeContextExpression(), 
                    new MSAst.ConstantExpression(SymbolTable.IdToString(_name)),
                    bases,
                    new MSAst.ConstantExpression(FindSelfNames()),
                    new MSAst.CodeBlockExpression(block, false), //TODO typing is messed up
                });

            return new MSAst.ExpressionStatement(
                new MSAst.BoundAssignment(_reference.Reference, classDef, Operators.None),
                new SourceSpan(Start, Header)
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
                    return string.Join(",", SelfNameFinder.FindNames(def.Body));
                }
            }
            return "";
        }

        private class SelfNameFinder : PythonWalker {
            public static string[] FindNames(Statement body) {
                SelfNameFinder finder = new SelfNameFinder();
                body.Walk(finder);
                return SymbolTable.IdsToStrings(new List<SymbolId>(finder._names.Keys));
            }

            private Dictionary<SymbolId, bool> _names = new Dictionary<SymbolId,bool>();

            private bool IsSelfReference(Expression expr) {
                NameExpression ne = expr as NameExpression;
                if (ne == null) return false;

                MSAst.Variable var = ne.Reference.Reference.Variable;
                if (var == null) return false;

                return var.Kind == MSAst.Variable.VariableKind.Parameter && var.Parameter == 0;
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
