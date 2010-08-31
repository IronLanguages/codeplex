#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.Reflection;

namespace ETScenarios.Miscellaneous {
    using EU = ETUtils.ExpressionUtils;
    using Expr = Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public class TypeAs {
        // Do valid cast with TypeAs
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TypeAs 1", new string[] { "positive", "typeis", "miscellaneous", "Pri1" })]
        public static Expr TypeAs1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expr test =
                Expr.Block(
                    Expr.Condition(
                        Expr.Equal(
                            Expr.TypeAs(Expr.Constant(new DivideByZeroException()), typeof(Exception)),
                            Expr.Constant(null)
                        ),
                        Expr.Constant("Null"),
                        Expr.Constant("NonNull")
                    )
                );

            Expressions.Add(EU.GenAreEqual(Expr.Constant("NonNull"), test, "TypeAs 1"));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Use TypeAs that results in null
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TypeAs 2", new string[] { "positive", "typeis", "miscellaneous", "Pri1" })]
        public static Expr TypeAs2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expr test =
                Expr.Block(
                    Expr.Condition(
                        Expr.Equal(
                            Expr.TypeAs(Expr.Constant(new DivideByZeroException()), typeof(string)),
                            Expr.Constant(null)
                        ),
                        Expr.Constant("Null"),
                        Expr.Constant("NonNull")
                    )
                );

            Expressions.Add(EU.GenAreEqual(Expr.Constant("Null"), test, "TypeAs 1"));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }

        public class Foo : DivideByZeroException {
            public Foo() {
            }
        }

        // Use TypeAs with user defined type that results in valid cast and null
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TypeAs 3", new string[] { "positive", "typeis", "miscellaneous", "Pri1" })]
        public static Expr TypeAs3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expr test =
                Expr.Block(
                    Expr.Condition(
                        Expr.Equal(
                            Expr.TypeAs(Expr.Constant(new Foo()), typeof(DivideByZeroException)),
                            Expr.Constant(null)
                        ),
                        Expr.Constant("Null"),
                        Expr.Constant("NonNull")
                    )
                );

            Expressions.Add(EU.GenAreEqual(Expr.Constant("NonNull"), test, "TypeAs 1"));

            Expr test2 =
                Expr.Block(
                    Expr.Condition(
                        Expr.Equal(
                            Expr.TypeAs(Expr.Constant(new Foo()), typeof(string)),
                            Expr.Constant(null)
                        ),
                        Expr.Constant("Null"),
                        Expr.Constant("NonNull")
                    )
                );

            Expressions.Add(EU.GenAreEqual(Expr.Constant("Null"), test2, "TypeAs 2"));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Use TypeAs with non-nullable value type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TypeAs 4", new string[] { "negative", "typeis", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr TypeAs4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expr test =
                EU.Throws<ArgumentException>(() =>
                {
                    Expr.Block(
                        Expr.Condition(
                            Expr.Equal(
                                Expr.TypeAs(Expr.Constant(new DivideByZeroException()), typeof(int)),
                                Expr.Constant(null)
                            ),
                            Expr.Constant("Null"),
                            Expr.Constant("NonNull")
                        )
                    );
                });

            return Expr.Empty();
        }

        // Use TypeAs with nullable value type
        // Regression for 660426
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TypeAs 5", new string[] { "positive", "typeis", "miscellaneous", "Pri1" })]
        public static Expr TypeAs5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expr test =
                Expr.Block(
                    Expr.Condition(
                        Expr.Equal(
                            Expr.TypeAs(Expr.Constant((short)1, typeof(short)), typeof(short?)),
                            Expr.Constant(null)
                        ),
                        Expr.Constant("Null"),
                        Expr.Constant("NonNull")
                    )
                );

            Expressions.Add(EU.GenAreEqual(Expr.Constant("NonNull"), test, "TypeAs 1"));

            Expr test2 =
                Expr.Block(
                    Expr.Condition(
                        Expr.Equal(
                            Expr.TypeAs(Expr.Constant(1.2), typeof(int?)),
                            Expr.Constant(null)
                        ),
                        Expr.Constant("Null"),
                        Expr.Constant("NonNull")
                    )
                );

            Expressions.Add(EU.GenAreEqual(Expr.Constant("Null"), test2, "TypeAs 2"));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }
    }
}
