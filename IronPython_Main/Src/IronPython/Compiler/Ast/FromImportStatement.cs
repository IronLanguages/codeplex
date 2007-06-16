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

using System.Collections.Generic;
using Microsoft.Scripting;
using MSAst = Microsoft.Scripting.Ast;

namespace IronPython.Compiler.Ast {
    public class FromImportStatement : Statement {
        private static readonly SymbolId[] _star = new SymbolId[1];
        private readonly DottedName _root;
        private readonly SymbolId[] _names;
        private readonly SymbolId[] _asNames;
        private readonly bool _fromFuture;

        private PythonVariable[] _variables;

        public static SymbolId[] Star {
            get { return FromImportStatement._star; }
        }

        public DottedName Root {
            get { return _root; }
        } 

        public bool IsFromFuture {
            get { return _fromFuture; }
        }

        public IList<SymbolId> Names {
            get { return _names; }
        }

        public IList<SymbolId> AsNames {
            get { return _asNames; }
        }

        internal PythonVariable[] Variables {
            get { return _variables; }
            set { _variables = value; }
        }

        public FromImportStatement(DottedName root, SymbolId[] names, SymbolId[] asNames, bool fromFuture) {
            _root = root;
            _names = names;
            _asNames = asNames;
            _fromFuture = fromFuture;
        }

        internal override MSAst.Statement Transform(AstGenerator ag) {
            if (_names == _star) {
                // from a[.b] import *
                return new MSAst.ExpressionStatement(
                    MSAst.MethodCallExpression.Call(
                        Span,
                        null,
                        AstGenerator.GetHelperMethod("ImportStar"),
                        new MSAst.CodeContextExpression(),
                        new MSAst.ConstantExpression(_root.MakeString())
                        ),
                    Span
                    );
            } else {
                // from a[.b] import x [as xx], [ y [ as yy] ] [ , ... ]

                List<MSAst.Statement> statements = new List<MSAst.Statement>();
                MSAst.BoundExpression module = ag.MakeTempExpression("module", _root.Span);

                // Create initializer of the array of names being passed to ImportWithNames
                MSAst.ConstantExpression[] names = new MSAst.ConstantExpression[_names.Length];
                for (int i = 0; i < names.Length; i++) {
                    names[i] = new MSAst.ConstantExpression(SymbolTable.IdToString(_names[i]));
                }

                // module = PythonOps.ImportWithNames(<context>, _root, make_array(_names))
                statements.Add(
                    new MSAst.ExpressionStatement(
                        new MSAst.BoundAssignment(
                            module.Variable,
                            MSAst.MethodCallExpression.Call(
                                _root.Span,
                                null,
                                AstGenerator.GetHelperMethod("ImportWithNames"),
                                new MSAst.CodeContextExpression(),
                                new MSAst.ConstantExpression(_root.MakeString()),
                                MSAst.NewArrayExpression.NewArrayInit(typeof(string[]), names)
                                ),
                            Operators.None,
                            _root.Span
                            ),
                        _root.Span
                    )
                );

                // now load all the names being imported and assign the variables
                for (int i = 0; i < names.Length; i++) {
                    statements.Add(
                        new MSAst.ExpressionStatement(
                                new MSAst.BoundAssignment(
                                    _variables[i].Variable,
                                    MSAst.MethodCallExpression.Call(
                                        Span,
                                        null,
                                        AstGenerator.GetHelperMethod("ImportFrom"),
                                        new MSAst.CodeContextExpression(),
                                        module,
                                        names[i]
                                    ),
                                    Operators.None,
                                    Span
                                ),
                                Span
                            )
                        );
                }

                return new MSAst.BlockStatement(statements.ToArray(), Span);
            }
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
            }
            walker.PostWalk(this);
        }
    }
}
