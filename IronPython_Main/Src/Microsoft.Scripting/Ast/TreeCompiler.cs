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

namespace Microsoft.Scripting.Ast {
    public static class TreeCompiler {
        public static T CompileExpression<T>(Expression expression) {
            Contract.Requires(typeof(Delegate).IsAssignableFrom(typeof(T)), "T");
            Contract.RequiresNotNull(expression, "expression");

            CodeBlock cb = Ast.CodeBlock("<expression>", expression.Type);
            if (expression.Type != typeof(void)) {
                cb.Body = Ast.Return(
                    expression
                );
            } else {
                cb.Body = Ast.Block(
                    Ast.Statement(
                        expression
                    ),
                    Ast.Return()
                );
            }
            return CompileBlock<T>(cb);
        }

        public static T CompileStatement<T>(Expression expression) {
            Contract.Requires(typeof(Delegate).IsAssignableFrom(typeof(T)), "T");
            Contract.RequiresNotNull(expression, "expression");

            CodeBlock cb = Ast.CodeBlock("<statement>", typeof(void));
            cb.Body = Ast.Block(
                expression,
                Ast.Return()
            );
            return CompileBlock<T>(cb);
        }

        internal static bool DebugAssembly = true;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "block")]
        public static T CompileBlock<T>(CodeBlock block) {
            Contract.Requires(typeof(Delegate).IsAssignableFrom(typeof(T)), "T");
            Contract.RequiresNotNull(block, "block");

            // Call compiler to create the delegate.
            return Compiler.CompileCodeBlock<T>(block);
        }
    }
}
