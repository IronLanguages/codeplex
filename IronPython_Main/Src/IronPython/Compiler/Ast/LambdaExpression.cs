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

using MSAst = Microsoft.Scripting.Internal.Ast;

namespace IronPython.Compiler.Ast {
    public class LambdaExpression : Expression {
        private readonly FunctionDefinition _function;

        public LambdaExpression(FunctionDefinition function) {
            _function = function;
        }

        public FunctionDefinition Function {
            get { return _function; }
        }

        internal override MSAst.Expression Transform(AstGenerator ag) {
            return _function.TransformToFunctionExpression(ag);
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_function!= null) {
                    _function.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }
    }
}
