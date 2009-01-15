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

using Microsoft.Scripting.Ast;

namespace Microsoft.Scripting.Actions {
    /// <summary>
    /// Rule binder resolves variables in the rule and nested lambdas.
    /// </summary>
    internal sealed class RuleBinder : VariableBinder {
        private readonly Type _result;

        /// <summary>
        /// RuleBinder entry point
        /// </summary>
        internal static AnalyzedRule Bind(Rule rule, Type result, int paramStartIndex) {
            RuleBinder rb = new RuleBinder(result);

            // Add a virtual LambdaInfo to represent the rule
            // TODO: remove when Rule is an Expression<T> 
            LambdaInfo top = new LambdaInfo(null, null);
            rb.Stack.Push(top);
            rb.DefineParameters(top, rule.Parameters, paramStartIndex);

            rb.WalkNode(rule.Binding);
            rb.BindTheScopes();

            return new AnalyzedRule(top, rb.Lambdas, rb.Infos, rb.Generators);
        }

        private RuleBinder(Type result) {
            _result = result;
        }

        protected override void PostWalk(IntrinsicExpression node) {
            if (node.NodeType == AstNodeType.CodeContextExpression) {
                throw new InvalidOperationException("CodeContext in rule");
            }
        }

        // This may not belong here because it is checking for the
        // AST type consistency. However, since it is the only check
        // it seems unwarranted to make an extra walk of the AST just
        // to verify this condition.
        protected override bool Walk(ReturnStatement node) {
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
    }
}
