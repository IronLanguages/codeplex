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
using System; using Microsoft;


using System.Collections.Generic;
using Microsoft.Scripting;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using MSAst = Microsoft.Linq.Expressions;

namespace IronPython.Compiler.Ast {
    using Ast = Microsoft.Linq.Expressions.Expression;

    public class FromImportStatement : Statement {
        private static readonly SymbolId[] _star = new SymbolId[1];
        private readonly ModuleName _root;
        private readonly SymbolId[] _names;
        private readonly SymbolId[] _asNames;
        private readonly bool _fromFuture;
        private readonly bool _forceAbsolute;

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

        public FromImportStatement(ModuleName root, SymbolId[] names, SymbolId[] asNames, bool fromFuture, bool forceAbsolute) {
            _root = root;
            _names = names;
            _asNames = asNames;
            _fromFuture = fromFuture;
            _forceAbsolute = forceAbsolute;
        }

        internal override MSAst.Expression Transform(AstGenerator ag) {            
            if (_names == _star) {
                // from a[.b] import *
                return ag.AddDebugInfo(
                    Ast.Call(
                        AstGenerator.GetHelperMethod("ImportStar"), 
                        AstUtils.CodeContext(), 
                        Ast.Constant(_root.MakeString()), 
                        Ast.Constant(GetLevel())
                    ),
                    Span
                );
            } else {
                // from a[.b] import x [as xx], [ y [ as yy] ] [ , ... ]

                List<MSAst.Expression> statements = new List<MSAst.Expression>();
                MSAst.ParameterExpression module = ag.GetTemporary("module");

                // Create initializer of the array of names being passed to ImportWithNames
                MSAst.ConstantExpression[] names = new MSAst.ConstantExpression[_names.Length];
                for (int i = 0; i < names.Length; i++) {
                    names[i] = Ast.Constant(SymbolTable.IdToString(_names[i]));
                }

                // module = PythonOps.ImportWithNames(<context>, _root, make_array(_names))
                statements.Add(
                    ag.AddDebugInfo(
                        AstUtils.Assign(
                            module, 
                            Ast.Call(
                                AstGenerator.GetHelperMethod("ImportWithNames"),
                                AstUtils.CodeContext(),
                                Ast.Constant(_root.MakeString()),
                                Ast.NewArrayInit(typeof(string), names),
                                Ast.Constant(GetLevel())
                            )
                        ),
                        _root.Span
                    )
                );

                // now load all the names being imported and assign the variables
                for (int i = 0; i < names.Length; i++) {
                    statements.Add(
                        ag.AddDebugInfo(
                            AstUtils.Assign(
                                _variables[i].Variable, 
                                Ast.Call(
                                    AstGenerator.GetHelperMethod("ImportFrom"),
                                    AstUtils.CodeContext(),
                                    module,
                                    names[i]
                                )
                            ),
                            Span
                        )
                    );
                }

                statements.Add(Ast.Empty());
                return ag.AddDebugInfo(Ast.Block(statements.ToArray()), Span);
            }
        }

        private object GetLevel() {
            RelativeModuleName rmn = _root as RelativeModuleName;
            if (rmn != null) {
                return rmn.DotCount;
            }

            if (_forceAbsolute) {
                return 0;
            }

            return -1;
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
            }
            walker.PostWalk(this);
        }
    }
}
