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
using System.Collections.Generic;

namespace Microsoft.Scripting.Ast {
    class AbstractInterpreter {
        private readonly AbstractContext _context;

        internal AbstractInterpreter(AbstractContext context) {
            _context = context;
        }

        internal AbstractValue Evaluate(Expression e) {
            switch (e.NodeType) {
                case AstNodeType.GlobalVariable:
                case AstNodeType.LocalVariable:
                case AstNodeType.TemporaryVariable:
                    return AbstractEvaluate((VariableExpression)e);
                default:
                    throw new NotImplementedException("Abstract interpretation for " + e.NodeType.ToString());
            }
        }

        public AbstractValue AbstractEvaluate(ActionExpression node) {
            List<AbstractValue> values = new List<AbstractValue>();
            foreach (Expression arg in node.Arguments) {
                values.Add(Evaluate(arg));
            }

            return _context.Binder.AbstractExecute(node.Action, values);
        }

        private AbstractValue AbstractEvaluate(VariableExpression node) {
            return _context.Lookup(node);
        }
    }
}
