/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if !SILVERLIGHT3
#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System;
using System.Collections;
using TreeUtils = Microsoft.Scripting.Ast.Utils;
using EU = ETUtils.ExpressionUtils;

namespace AstTest {
    public partial class Scenarios {
        public static void Positive_TestReducible(EU.IValidator V) {
            var result = Expression.Parameter(typeof(string), "$result");
            var str = Expression.Parameter(typeof(string), "$str");
            var forEach = new TestReducibleNode(
                Expression.NewArrayInit(typeof(string), Expression.Constant("foo"), Expression.Constant("bar"), Expression.Constant("red")),
                str,
                Expression.Assign(
                    result,
                    Expression.Call(
                        typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) }),
                        result,
                        str
                    )
                )
            );

            var e = Expression.Lambda<Func<string>>(
                Expression.Block(
                    new[] { result },
                    Expression.Assign(result, Expression.Constant("")),
                    forEach,
                    result
                )
            );

            // Node is preserved as is
            EU.Equal(((BlockExpression)e.Body).Expressions[1], forEach);

            V.Validate(e, f =>
            {
                EU.Equal(f(), "foobarred");
            });
        }

        public static void Positive_TestExtensionToString(EU.IValidator V) {
            string s = new TestReducibleNode(
                Expression.Empty(),
                Expression.Parameter(typeof(int), "x"),
                Expression.Empty()
            ).ToString();
            EU.Equal(s, "[AstTest.TestReducibleNode]");
            
            s = new TestLegacyExtensionNode().ToString();
            EU.Equal(s, "[AddChecked]");

            // create a non-interned test string instance.
            string test = new string("Hello, world.".ToCharArray());
            s = new TestExtensionToString(test).ToString();
            EU.ReferenceEqual(s, test);

            // now put it in another expression, not necessarily ref equal anymore
            s = Expression.TypeAs(new TestExtensionToString(test), typeof(object)).ToString();
            EU.Equal(s, "(Hello, world. As Object)");
        }

        // doesn't override NodeType or Type
        private class BadExtension : Expression {
        }

        public static void Negative_BadExtension(EU.IValidator V) {
            var bad = new BadExtension();
            EU.Throws<InvalidOperationException>(() => { var x = bad.NodeType; });
            EU.Throws<InvalidOperationException>(() => { var x = bad.Type; });
        }
    }

    public class TestLegacyExtensionNode : Expression {
        public sealed override Type Type {
            get { return typeof(object); }
        }

        public sealed override ExpressionType NodeType {
            get { return ExpressionType.AddChecked; }
        }
    }

    public class TestExtensionToString : TestLegacyExtensionNode {
        private readonly string _toString;

        public TestExtensionToString(string str) {
            _toString = str;
        }

        public override string ToString() {
            return _toString;
        }
    }

    // Simple reducible node implementing a foreach node
    public class TestReducibleNode : Expression {
        private readonly Expression _enumerable;
        private readonly ParameterExpression _variable;
        private readonly Expression _body;

        public TestReducibleNode(Expression enumerable, ParameterExpression variable, Expression body) {
            _enumerable = enumerable;
            _variable = variable;
            _body = body;
        }

        public override bool CanReduce {
            get { return true; }
        }

        public sealed override Type Type {
            get { return typeof(void); }
        }

        public sealed override ExpressionType NodeType {
            get { return ExpressionType.Extension; }
        }

        public override Expression Reduce() {
            ParameterExpression temp = Expression.Variable(typeof(IEnumerator), "$enumerator");
            return Expression.Block(
                new[] { temp, _variable },
                Expression.Assign(temp, Expression.Call(_enumerable, typeof(IEnumerable).GetMethod("GetEnumerator"))),
                TreeUtils.Loop(
                    Expression.Call(temp, typeof(IEnumerator).GetMethod("MoveNext")),
                    null,
                    Expression.Block(
                        Expression.Assign(
                            _variable,
                            Expression.Convert(
                                Expression.Property(temp, typeof(IEnumerator).GetProperty("Current")),
                                _variable.Type
                            )
                        ),
                        _body
                    ),
                    null,
                    null,
                    null
                )
            );
        }
    }
}
#endif
