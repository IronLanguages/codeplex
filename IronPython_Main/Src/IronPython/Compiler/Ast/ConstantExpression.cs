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
using MSAst = Microsoft.Scripting.Ast;
using Operators = Microsoft.Scripting.Operators;
using IronPython.Runtime;
using IronPython.Runtime.Operations;

namespace IronPython.Compiler.Ast {
    public class ConstantExpression : Expression {
        private readonly object _value;

        public ConstantExpression(object value) {
            _value = value;
        }

        public object Value {
            get { return _value; }
        }

        internal override MSAst.Expression Transform(AstGenerator ag, Type type) {
            if (_value == PythonOps.Ellipsis) {
                return MSAst.MemberExpression.Field(
                    null,
                    typeof(PythonOps).GetField("Ellipsis")
                    );
            }

            return new MSAst.ConstantExpression(_value, Span);
        }

        internal override MSAst.Statement TransformSet(AstGenerator ag, MSAst.Expression right, Operators op) {
            ag.AddError(_value == null ? "assignment to None" : "can't assign to literal", Span);
            return null;
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
            }
            walker.PostWalk(this);
        }
    }
}
