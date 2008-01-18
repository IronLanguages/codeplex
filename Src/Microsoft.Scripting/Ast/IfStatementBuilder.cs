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

using System.Collections.Generic;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    public sealed class IfStatementBuilder {
        private readonly List<IfStatementTest> _clauses = new List<IfStatementTest>();

        internal IfStatementBuilder() {
        }

        public IfStatementBuilder ElseIf(Expression test, params Expression[] body) {
            Contract.RequiresNotNullItems(body, "body");
            return ElseIf(SourceSpan.None, test, SourceLocation.None, Ast.Block(body));
        }

        public IfStatementBuilder ElseIf(Expression test, Expression body) {
            return ElseIf(SourceSpan.None, test, SourceLocation.None, body);
        }

        public IfStatementBuilder ElseIf(SourceSpan span, Expression test, SourceLocation bodyLocation, Expression body) {
            Contract.RequiresNotNull(test, "test");
            Contract.Requires(test.Type == typeof(bool), "test");
            Contract.RequiresNotNull(body, "body");
            _clauses.Add(Ast.IfCondition(span, bodyLocation, test, body));
            return this;
        }

        public Expression Else(params Expression[] body) {
            Contract.RequiresNotNullItems(body, "body");
            return Else(Ast.Block(body));
        }

        public Expression Else(Expression body) {
            Contract.RequiresNotNull(body, "body");
            return BuildConditions(_clauses, body);
        }

        internal static Expression BuildConditions(IList<IfStatementTest> clauses, Expression @else) {
            Expression result = @else != null ? Ast.Void(@else) : Ast.Empty();

            int index = clauses.Count;
            while (index-- > 0) {
                IfStatementTest ist = clauses[index];

                Expression test = ist.Test;
                if (ist.Start.IsValid && ist.Header.IsValid) {
                    test = Ast.Statement(new SourceSpan(ist.Start, ist.Header), test);
                }

                result = Ast.Condition(
                    test,
                    Ast.Void(ist.Body),
                    result
                );
            }

            return result;
        }

        public Expression ToStatement() {
            return BuildConditions(_clauses, null);
        }

        public static implicit operator Expression(IfStatementBuilder builder) {
            Contract.RequiresNotNull(builder, "builder");
            return builder.ToStatement();
        }
    }

    public static partial class Ast {
        public static IfStatementBuilder If() {
            return new IfStatementBuilder();
        }

        public static IfStatementBuilder If(Expression test, params Expression[] body) {
            return If().ElseIf(test, body);
        }

        public static IfStatementBuilder If(Expression test, Expression body) {
            return If().ElseIf(test, body);
        }

        public static IfStatementBuilder If(SourceSpan testSpan, Expression test, SourceLocation header, Expression body) {
            return If().ElseIf(testSpan, test, header, body);
        }

        public static Expression If(IfStatementTest[] tests, Expression @else) {
            Contract.RequiresNotNullItems(tests, "tests");
            return IfStatementBuilder.BuildConditions(tests, @else);
        }

        public static Expression IfThen(Expression test, Expression body) {
            return IfThenElse(test, body, null);
        }

        public static Expression IfThen(Expression test, params Expression[] body) {
            return IfThenElse(test, Block(body), null);
        }

        public static Expression IfThenElse(Expression test, Expression body, Expression @else) {
            return If(
                new IfStatementTest[] {
                    Ast.IfCondition(SourceSpan.None, SourceLocation.None, test, body)
                },
                @else
            );
        }

        public static Expression Unless(Expression test, Expression body) {
            return IfThenElse(test, Ast.Empty(), body);
        }
    }
}
