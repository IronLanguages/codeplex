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
    /// <summary>
    /// Rule binder resolves variables in the rule and nested lambdas.
    /// </summary>
    class RuleBinder : VariableBinder {
        private readonly LambdaInfo _top = new LambdaInfo(null, null);
        private readonly Type _result;

        /// <summary>
        /// RuleBinder entry point
        /// </summary>
        internal static AnalyzedRule Bind(StandardRule rule, Type result, int paramStartIndex) {
            RuleBinder rb = new RuleBinder(result);

            // Add variables defined in the rule
            foreach (VariableExpression v in rule.ParamVariables) {
                rb._top.Variables.Add(v, new VariableInfo(v, null, paramStartIndex++));
            }
            foreach (VariableExpression v in rule.TemporaryVariables) {
                rb._top.Variables.Add(v, new VariableInfo(v, null));
            }

            rb.WalkNode(rule.Test);
            rb.WalkNode(rule.Target);

            rb.BindTheScopes();

            return new AnalyzedRule(rb._top, rb.Lambdas, rb.Infos);
        }

        private RuleBinder(Type result) {
            _result = result;
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

        protected override void Reference(VariableExpression variable) {
            Debug.Assert(variable != null);
            if (Stack == null || Stack.Count == 0) {
                // Top level reference inside the rule.
                _top.AddVariableReference(variable);
            } else {
                // Call base class for reference within a lambda.
                base.Reference(variable);
            }
        }
    }
}
