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

#if !CLR2
using MSAst = System.Linq.Expressions;
#else
using MSAst = Microsoft.Scripting.Ast;
#endif

using System;

namespace IronPython.Compiler.Ast {
    public class LambdaExpression : Expression {
        private readonly FunctionDefinition _function;

        public LambdaExpression(FunctionDefinition function) {
            _function = function;
        }

        public FunctionDefinition Function {
            get { return _function; }
        }

        public override MSAst.Expression Reduce() {
            return _function.MakeFunctionExpression();
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
