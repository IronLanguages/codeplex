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
using System.Diagnostics;

using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Actions {
    class RuleBinder : Walker {
        private readonly Dictionary<Variable, VariableReference> _refs = new Dictionary<Variable,VariableReference>();
        private readonly Type _result;

        public static Dictionary<Variable, VariableReference> Bind(Expression test, Expression target, Type result) {
            Assert.NotNull(test, target);

            RuleBinder rb = new RuleBinder(result);
            rb.WalkNode(test);
            rb.WalkNode(target);
            return rb._refs;
        }

        private RuleBinder(Type result) {
            _result = result;
        }

        protected internal override bool Walk(BoundAssignment node) {
            GetOrMakeRef(node.Variable);
            return true;
        }

        protected internal override bool Walk(BoundExpression node) {
            GetOrMakeRef(node.Variable);
            return true;
        }

        protected internal override bool Walk(DeleteStatement node) {
            GetOrMakeRef(node.Variable);
            return true;
        }

        // This may not belong here because it is checking for the
        // AST type consistency. However, since it is the only check
        // it seems unwarranted to make an extra walk of the AST just
        // to verify this condition.
        protected internal override bool Walk(ReturnStatement node) {
            if (node.Expression == null) {
                throw new ArgumentException("ReturnStatement in a rule must return value");
            }
            Type type = node.Expression.Type;
            if (!_result.IsAssignableFrom(type)) {
                string msg = String.Format("Cannot return {0} from a rule with return type {1}", type, _result);
                throw new ArgumentException(msg);
            }
            return true;
        }

        protected internal override bool Walk(CatchBlock node) {
            if (node.Variable != null) {
                GetOrMakeRef(node.Variable);
            }
            return true;
        }

        private void GetOrMakeRef(Variable variable) {
            Debug.Assert(variable != null);
            if (!_refs.ContainsKey(variable)) {
                _refs[variable] = new VariableReference(variable);
            }
        }
    }
}
