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

namespace Microsoft.Scripting.Ast {
    /// <summary>
    /// The LambdaBinder resolves variable references across lambdas.
    /// </summary>
    internal sealed class LambdaBinder : VariableBinder {
        /// <summary>
        /// LambdaBinder entry point.
        /// </summary>
        internal static AnalyzedTree Bind(LambdaExpression ast) {
            LambdaBinder cb = new LambdaBinder();
            // Collect the lambdas
            cb.WalkNode(ast);
            cb.BindTheScopes();

            return new AnalyzedTree(cb.Lambdas, cb.Infos, cb.Generators);
        }

        /// <summary>
        /// Private constructor so that only the class can create self.
        /// </summary>
        private LambdaBinder() {
        }

        #region AstWalker overrides

        // This may not belong here because it is checking for the
        // AST type consistency. However, since it is the only check
        // it seems unwarranted to make an extra walk of the AST just
        // to verify this condition.
        protected override bool Walk(ReturnStatement node) {
            if (Stack.Count == 0) {
                throw InvalidReturnStatement("Return outside of a lambda", node);
            }

            Type returnType = Stack.Peek().Lambda.ReturnType;

            if (node.Expression != null) {
                if (!returnType.IsAssignableFrom(node.Expression.Type)) {
                    throw InvalidReturnStatement("Invalid type of return expression value", node);
                }
            } else {
                // return without expression can be only from lambda with void return type
                if (returnType != typeof(void)) {
                    throw InvalidReturnStatement("Missing return expression", node);
                }
            }

            return true;
        }

        private static ArgumentException InvalidReturnStatement(string message, ReturnStatement node) {
            SourceSpan span = node.Annotations.Get<SourceSpan>();
            return new ArgumentException(
                String.Format(
                    "{0} at {1}:{2}-{3}:{4}", message,
                    span.Start.Line, span.Start.Column, span.End.Line, span.End.Column
                )
            );
        }

        #endregion
    }
}
