/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
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
    using Ast = Microsoft.Scripting.Ast.Ast;

    public class ImportStatement : Statement {
        private readonly DottedName[] _names;
        private readonly SymbolId[] _asNames;

        private PythonVariable[] _variables;

        public ImportStatement(DottedName[] names, SymbolId[] asNames) {
            _names = names;
            _asNames = asNames;
        }

        internal PythonVariable[] Variables {
            get { return _variables; }
            set { _variables = value; }
        }

        public IList<DottedName> Names {
            get { return _names; }
        }

        public IList<SymbolId> AsNames {
            get { return _asNames; }
        }

        internal override MSAst.Statement Transform(AstGenerator ag) {
            List<MSAst.Statement> statements = new List<MSAst.Statement>();

            for (int i = 0; i < _names.Length; i++) {
                statements.Add(
                    // _references[i] = PythonOps.Import(<code context>, _names[i])
                    Ast.Statement(
                        _names[i].Span,
                        Ast.Assign(
                            _variables[i].Variable,
                            Ast.Call(
                                AstGenerator.GetHelperMethod(                           // helper
                                    _asNames[i] == SymbolId.Empty ? "ImportTop" : "ImportBottom"
                                ),
                                Ast.CodeContext(),                                      // 1st arg - code context
                                Ast.Constant(_names[i].MakeString())                    // 2nd arg - module name
                            )
                        )
                    )
                );
            }

            return Ast.Block(Span, statements.ToArray());
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
            }
            walker.PostWalk(this);
        }
    }
}
