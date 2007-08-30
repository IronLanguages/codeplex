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

using MSAst = Microsoft.Scripting.Ast;

using IronPython.Runtime;
using IronPython.Runtime.Operations;

namespace IronPython.Compiler.Ast {
    using Ast = Microsoft.Scripting.Ast.Ast;

    public class RaiseStatement : Statement {
        private readonly Expression _type, _value, _traceback;

        public RaiseStatement(Expression exceptionType, Expression exceptionValue, Expression traceBack) {
            _type = exceptionType;
            _value = exceptionValue;
            _traceback = traceBack;
        }

        public Expression Type {
            get { return _type; }
        }

        public Expression Value {
            get { return _value; }
        }

        public Expression Traceback {
            get { return _traceback; }
        }

        internal override MSAst.Statement Transform(AstGenerator ag) {
            MSAst.ThrowExpression expr;
            expr = Ast.Throw(
                Ast.Call(null, AstGenerator.GetHelperMethod("MakeException"),
                    ag.Transform(_type ?? new ConstantExpression(null)),
                    ag.Transform(_value ?? new ConstantExpression(null)),
                    ag.Transform(_traceback ?? new ConstantExpression(null))));

            return Ast.Statement(Span, expr);
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_type != null) {
                    _type.Walk(walker);
                }
                if (_value != null) {
                    _value.Walk(walker);
                }
                if (_traceback != null) {
                    _traceback.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }
    }
}
