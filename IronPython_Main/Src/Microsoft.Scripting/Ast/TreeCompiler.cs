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

using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Ast {
    public static class TreeCompiler {
        public static T CompileExpression<T>(Expression expression) {
            Contract.Requires(typeof(Delegate).IsAssignableFrom(typeof(T)), "T");
            Contract.RequiresNotNull(expression, "expression");

            Expression body;
            if (expression.Type != typeof(void)) {
                body = Ast.Return(
                    expression
                );
            } else {
                body = Ast.Block(
                    expression,
                    Ast.Return()
                );
            }

            CodeBlock cb = Ast.CodeBlock("<expression>", expression.Type, body, new Variable[0], new Variable[0]);
            return CompileBlock<T>(cb);
        }

        public static T CompileStatement<T>(Expression expression) {
            Contract.Requires(typeof(Delegate).IsAssignableFrom(typeof(T)), "T");
            Contract.RequiresNotNull(expression, "expression");

            Expression body = Ast.Block(
                expression,
                Ast.Return()
            );

            CodeBlock cb = Ast.CodeBlock("<statement>", typeof(void), body, new Variable[0], new Variable[0]);
            return CompileBlock<T>(cb);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "block")]
        public static T CompileBlock<T>(CodeBlock block) {
            Contract.Requires(typeof(Delegate).IsAssignableFrom(typeof(T)), "T");
            Contract.RequiresNotNull(block, "block");

            // Call compiler to create the delegate.
            return LambdaCompiler.CompileCodeBlock<T>(block);
        }
    }
}
