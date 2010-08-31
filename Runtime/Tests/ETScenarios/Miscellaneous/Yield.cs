#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;

namespace ETScenarios.Miscellaneous {
    using EU = ETUtils.ExpressionUtils;
    using Expr = Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;
    
    public class Yield {
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Yield 1", new string[] { "negative", "yield", "miscellaneous", "controlflow", "regression", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Yield1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

            ParameterExpression i = Expression.Variable(typeof(int), "i");
            Expr Test = Expr.LessThan(i, Expr.Constant(5));
            ParameterExpression LambdaReturn = Expr.Variable(typeof(IEnumerable<int>), "");
            ParameterExpression LambdaReturnList = Expr.Variable(typeof(List<int>), "");
            ParameterExpression LambdaReturnArray = Expr.Variable(typeof(int[]), "");
            LabelTarget label = Expr.Label(typeof(int));

            Expr MyLoop = AstUtils.Loop(
                        Test,
                        Expression.Assign(i, Expression.Add(i, Expression.Constant(1))),
                        AstUtils.YieldReturn(label, i),
                        Expression.Constant(null),
                        Expression.Label("YieldLoop"),
                        Expression.Label("YieldLoop")
                     );

            var lambda =
                EU.Throws<System.ArgumentException>(() =>
                {
                    Expr.Lambda(
                        typeof(Func<IEnumerable<int>>) /*typeof(TestTreeDelegate)*/,
                        EU.BlockVoid(
                            new[] { i },
                            MyLoop
                        ),
                        "LambdaYield",
                        //typeof(IEnumerable<int>),
                        new ParameterExpression[] { }
                    );
                });

            return Expr.Empty();
        }

        public interface ITest : IEnumerator {

        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Yield 2", new string[] { "negative", "generator", "miscellaneous", "controlflow", "regression", "Pri1" }, Exception = typeof(System.ArgumentException))]
        public static Expr Yield2(EU.IValidator V) {
            var label = Expr.Label(typeof(int));

            var ret = AstUtils.YieldReturn(label, Expression.Constant(1));

            return EU.Throws<ArgumentException>(() => { AstUtils.GeneratorLambda(typeof(Func<ITest>), label, ret); });
        }

        // Yield on a block
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Yield 2_1", new string[] { "negative", "yield", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Yield2_1(EU.IValidator V) {
            var tree = EU.BlockVoid(AstUtils.YieldReturn(Expr.Label(typeof(int)), Expr.Constant(1)));
            V.ValidateException<System.ArgumentException>(tree, true);
            return Expr.Empty();
        }

        // Yield on a comma
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Yield 3", new string[] { "negative", "yield", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Yield3(EU.IValidator V) {
            var tree = Expr.Block(AstUtils.YieldReturn(Expr.Label(typeof(int)), Expr.Constant(1)));
            V.ValidateException<System.ArgumentException>(tree, true);
            return Expr.Empty();
        }

        // Yield within a block inside a generator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Yield 4", new string[] { "positive", "yield", "miscellaneous", "Pri1" })]
        public static Expr Yield4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(bool), "");
            ParameterExpression Value = Expr.Variable(typeof(object), "");

            LabelTarget label = Expr.Label(typeof(int));

            var Gen =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    label,
                    EU.BlockVoid(
                        AstUtils.YieldReturn(label, Expr.Constant(1)),
                        AstUtils.YieldReturn(label, Expr.Constant(2)),
                        AstUtils.YieldReturn(label, Expr.Constant(3))
                    ),
                    new ParameterExpression[] { }
                );

            IEnumerator enumerator = (IEnumerator)Gen.Compile().DynamicInvoke();
            Expr e = Expr.Constant(enumerator, typeof(IEnumerator));

            EU.Enumerate(ref Expressions, new int[] { 1, 2, 3 }, 3, e, ref Result, ref Value);

            var tree = EU.BlockVoid(new[] { Result, Value }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Yield within a comma inside a generator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Yield 5", new string[] { "positive", "yield", "miscellaneous", "Pri1" })]
        public static Expr Yield5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(bool), "");
            ParameterExpression Value = Expr.Variable(typeof(object), "");

            LabelTarget label = Expr.Label(typeof(double));

            var Gen =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    label,
                    Expr.Block(
                        AstUtils.YieldReturn(label, Expr.Constant(1.0)),
                        AstUtils.YieldReturn(label, Expr.Constant(2.0)),
                        AstUtils.YieldReturn(label, Expr.Constant(3.0))
                    ),
                    new ParameterExpression[] { }
                );

            IEnumerator enumerator = (IEnumerator)Gen.Compile().DynamicInvoke();
            Expr e = Expr.Constant(enumerator, typeof(IEnumerator));

            EU.Enumerate(ref Expressions, new double[] { 1.0, 2.0, 3.0 }, 3, e, ref Result, ref Value);

            var tree = EU.BlockVoid(new[] { Result, Value }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Yield within a try inside a generator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Yield 6", new string[] { "positive", "yield", "miscellaneous", "Pri1" })]
        public static Expr Yield6(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(bool), "Result");
            ParameterExpression Value = Expr.Variable(typeof(object), "Value");
            ParameterExpression Index = Expr.Variable(typeof(Int32), "Index");

            LabelTarget label = Expr.Label(typeof(int));

            var Gen =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    label,
                    Expr.Block(
                        new ParameterExpression[] { Index },
                        Expr.TryCatch(
                            AstUtils.Loop(
                                Expr.LessThan(Index, Expr.Constant(5)),
                                Expr.Assign(Index, Expr.Add(Index, Expr.Constant(1))),
                                Expr.Block(
                                    AstUtils.YieldReturn(label, Index),
                                    Expr.Empty()
                                ),
                                null,
                                null,
                                null
                            ),
                            Expr.Catch(typeof(DivideByZeroException), Expr.Empty())
                        )
                    ),
                    new ParameterExpression[] { }
                );

            IEnumerator enumerator = (IEnumerator)Gen.Compile().DynamicInvoke();
            Expr e = Expr.Constant(enumerator, typeof(IEnumerator));

            EU.Enumerate(ref Expressions, new int[] { 0, 1, 2, 3, 4 }, 5, e, ref Result, ref Value);

            var tree = EU.BlockVoid(new[] { Result, Value, Index }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Yield within a catch inside a generator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Yield 6_1", new string[] { "positive", "yield", "miscellaneous", "Pri1" })]
        public static Expr Yield6_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(bool), "Result");
            ParameterExpression Value = Expr.Variable(typeof(object), "Value");
            ParameterExpression Index = Expr.Variable(typeof(Int32), "Index");

            LabelTarget label = Expr.Label(typeof(int));

            var Gen =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    label,
                    Expr.Block(
                        new ParameterExpression[] { Index },
                        Expr.TryCatch(
                            AstUtils.Loop(
                                Expr.LessThan(Index, Expr.Constant(5)),
                                Expr.Assign(Index, Expr.Add(Index, Expr.Constant(1))),
                                EU.BlockVoid(
                                    Expr.Condition(
                                        Expr.Equal(Index, Expr.Constant(3)),
                                        Expr.Throw(Expr.Constant(new DivideByZeroException())),
                                        Expr.Empty()
                                    )
                                ),
                                null,
                                null,
                                null
                            ),
                            Expr.Catch(typeof(DivideByZeroException), AstUtils.YieldReturn(label, Index))
                        )
                    ),
                    new ParameterExpression[] { }
                );

            IEnumerator enumerator = (IEnumerator)Gen.Compile().DynamicInvoke();
            Expr e = Expr.Constant(enumerator, typeof(IEnumerator));

            EU.Enumerate(ref Expressions, new int[] { 3 }, 1, e, ref Result, ref Value);

            var tree = EU.BlockVoid(new[] { Result, Value, Index }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Yield within a finally inside a generator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Yield 7", new string[] { "positive", "yield", "miscellaneous", "Pri1" })]
        public static Expr Yield7(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(bool), "Result");
            ParameterExpression Value = Expr.Variable(typeof(object), "Value");
            ParameterExpression Index = Expr.Variable(typeof(Int32), "Index");

            LabelTarget label = Expr.Label(typeof(int));

            var Gen =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    label,
                    Expr.Block(
                        new ParameterExpression[] { Index },
                        Expr.TryCatchFinally(
                            AstUtils.Loop(
                                Expr.LessThan(Index, Expr.Constant(5)),
                                Expr.Assign(Index, Expr.Add(Index, Expr.Constant(1))),
                                EU.BlockVoid(
                                    AstUtils.YieldReturn(label, Index),
                                    Expr.Condition(
                                        Expr.Equal(Index, Expr.Constant(3)),
                                        Expr.Throw(Expr.Constant(new DivideByZeroException())),
                                        Expr.Empty()
                                    )
                                ),
                                null,
                                null,
                                null
                            ),
                            AstUtils.YieldReturn(label, Expr.Constant(0)),
                            Expr.Catch(typeof(DivideByZeroException), AstUtils.YieldReturn(label, Index))
                        )
                    ),
                    new ParameterExpression[] { }
                );

            IEnumerator enumerator = (IEnumerator)Gen.Compile().DynamicInvoke();
            Expr e = Expr.Constant(enumerator, typeof(IEnumerator));

            EU.Enumerate(ref Expressions, new int[] { 0, 1, 2, 3, 3, 0 }, 6, e, ref Result, ref Value);

            // case 2 - no exception thrown
            ParameterExpression Index2 = Expr.Variable(typeof(Int32), "Index2");

            var Gen2 =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    label,
                    Expr.Block(
                        new ParameterExpression[] { Index2 },
                        Expr.TryCatchFinally(
                            AstUtils.Loop(
                                Expr.LessThan(Index2, Expr.Constant(5)),
                                Expr.Assign(Index2, Expr.Add(Index2, Expr.Constant(1))),
                                AstUtils.YieldReturn(label, Index2),
                                null,
                                null,
                                null
                            ),
                            AstUtils.YieldReturn(label, Expr.Constant(0)),
                            Expr.Catch(typeof(DivideByZeroException), AstUtils.YieldReturn(label, Index2))
                        )
                    ),
                    new ParameterExpression[] { }
                );

            IEnumerator enumerator2 = (IEnumerator)Gen2.Compile().DynamicInvoke();
            Expr e2 = Expr.Constant(enumerator2, typeof(IEnumerator));

            EU.Enumerate(ref Expressions, new int[] { 0, 1, 2, 3, 4, 0 }, 6, e2, ref Result, ref Value);

            var tree = EU.BlockVoid(new[] { Result, Value, Index, Index2 }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Yield within a fault inside a generator
        // Previously blocked by Dev10 Bug 491787 (fixed by new implementation of Yield though)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Yield 8", new string[] { "negative", "yield", "miscellaneous", "Pri1" }, Exception = typeof(NotSupportedException))]
        public static Expr Yield8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(bool), "Result");
            ParameterExpression Value = Expr.Variable(typeof(object), "Value");
            ParameterExpression Index = Expr.Variable(typeof(Int32), "Index");

            LabelTarget label = Expr.Label(typeof(int));

            var Gen =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    label,
                    Expr.Block(
                        new ParameterExpression[] { Index },
                        Expr.TryFault(
                            AstUtils.Loop(
                                Expr.LessThan(Index, Expr.Constant(5)),
                                Expr.Assign(Index, Expr.Add(Index, Expr.Constant(1))),
                                EU.BlockVoid(
                                    AstUtils.YieldReturn(label, Index),
                                    Expr.Condition(
                                        Expr.Equal(Index, Expr.Constant(3)),
                                        Expr.Throw(Expr.Constant(new DivideByZeroException())),
                                        Expr.Empty()
                                    )
                                ),
                                null,
                                null,
                                null
                            ),
                            AstUtils.YieldReturn(label, Expr.Constant(0))
                        )
                    ),
                    new ParameterExpression[] { }
                );

            V.ValidateException<NotSupportedException>(Gen, true);
            
            return Gen;
        }

        // Yield within a filter inside a generator
        // Regression for Dev10 Bug 509769 (fixed by new implementation of Yield though)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Yield 9", new string[] { "negative", "yield", "miscellaneous", "Pri1" }, Exception = typeof(NotSupportedException))]
        public static Expr Yield9(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(bool), "Result");
            ParameterExpression Value = Expr.Variable(typeof(object), "Value");
            ParameterExpression Index = Expr.Variable(typeof(Int32), "Index");

            LabelTarget label = Expr.Label(typeof(int));

            Expr Filter1 = Expr.Block(AstUtils.YieldReturn(label, Expr.Constant(-1)), Expr.Constant(true));
            Expr Handler1 = AstUtils.YieldReturn(label, Expr.Constant(0));
            CatchBlock CatchBlock1 = Expr.Catch(typeof(DivideByZeroException), Handler1, Filter1);

            var Gen =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    label,
                    Expr.Block(
                        new ParameterExpression[] { Index },
                        Expr.TryCatchFinally(
                            AstUtils.Loop(
                                Expr.LessThan(Index, Expr.Constant(5)),
                                Expr.Assign(Index, Expr.Add(Index, Expr.Constant(1))),
                                EU.BlockVoid(
                                    AstUtils.YieldReturn(label, Index),
                                    Expr.Condition(
                                        Expr.Equal(Index, Expr.Constant(3)),
                                        Expr.Throw(Expr.Constant(new DivideByZeroException())),
                                        Expr.Empty()
                                    )
                                ),
                                null,
                                null,
                                null
                            ),
                            AstUtils.YieldReturn(label, Expr.Constant(2)),
                            CatchBlock1
                        )
                    ),
                    new ParameterExpression[] { }
                );

            V.ValidateException<NotSupportedException>(Gen, true);

            return Gen;
        }

        // Yield within an incremement statement of a loop inside a generator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Yield 10", new string[] { "positive", "yield", "miscellaneous", "Pri1" })]
        public static Expr Yield10(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(bool), "");
            ParameterExpression Value = Expr.Variable(typeof(object), "");
            ParameterExpression Index = Expr.Variable(typeof(Int32), "");

            LabelTarget label = Expr.Label(typeof(int));

            Expr Increment = EU.BlockVoid(
                                  Expr.Assign(Index, Expr.Add(Index, Expr.Constant(1))),
                                  AstUtils.YieldReturn(label, Index)
                             );

            var Gen =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    label,
                    Expr.Block(
                        new ParameterExpression[] { Index },
                        AstUtils.Loop(
                            Expr.LessThan(Index, Expr.Constant(5)),
                            Increment,
                            AstUtils.YieldReturn(label, Index),
                            null,
                            null,
                            null
                        )
                    ),
                    new ParameterExpression[] { }
                );

            IEnumerator enumerator = (IEnumerator)Gen.Compile().DynamicInvoke();
            Expr e = Expr.Constant(enumerator, typeof(IEnumerator));

            EU.Enumerate(ref Expressions, new int[] { 0, 1, 1, 2, 2, 3, 3, 4, 4, 5 }, 10, e, ref Result, ref Value);

            var tree = EU.BlockVoid(new[] { Result, Value }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Yield within an if statement within a generator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Yield 11", new string[] { "positive", "yield", "miscellaneous", "Pri1" })]
        public static Expr Yield11(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(bool), "");
            ParameterExpression Value = Expr.Variable(typeof(object), "");

            LabelTarget label = Expr.Label(typeof(int));

            var MyIf =
                AstUtils.If(
                    Expr.Constant(true),
                    AstUtils.YieldReturn(label, Expr.Constant(-1))
                );

            var Gen =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    label,
                    EU.BlockVoid(
                            AstUtils.YieldReturn(label, Expr.Constant(1)),
                            AstUtils.YieldReturn(label, Expr.Constant(2)),
                            AstUtils.YieldReturn(label, Expr.Constant(3)),
                            MyIf.ToStatement(),
                            AstUtils.YieldReturn(label, Expr.Constant(4))
                    ),
                    new ParameterExpression[] { }
                );

            IEnumerator enumerator = (IEnumerator)Gen.Compile().DynamicInvoke();
            Expr e = Expr.Constant(enumerator, typeof(IEnumerator));

            EU.Enumerate(ref Expressions, new int[] { 1, 2, 3, -1, 4 }, 5, e, ref Result, ref Value);

            var tree = EU.BlockVoid(new[] { Result, Value }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Yield within an if statement within a generator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Yield 12", new string[] { "positive", "yield", "miscellaneous", "Pri1" })]
        public static Expr Yield12(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(bool), "");
            ParameterExpression Value = Expr.Variable(typeof(object), "");

            LabelTarget label = Expr.Label(typeof(int));

            var MyIf =
                AstUtils.If(
                    Expr.Block(AstUtils.YieldReturn(label, Expr.Constant(-1)), Expr.Constant(true)),
                    AstUtils.YieldReturn(label, Expr.Constant(-2))
                );

            var Gen =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    label,
                    EU.BlockVoid(
                            AstUtils.YieldReturn(label, Expr.Constant(1)),
                            AstUtils.YieldReturn(label, Expr.Constant(2)),
                            AstUtils.YieldReturn(label, Expr.Constant(3)),
                            MyIf.ToStatement(),
                            AstUtils.YieldReturn(label, Expr.Constant(4))

                    ),
                    new ParameterExpression[] { }
                );

            IEnumerator enumerator = (IEnumerator)Gen.Compile().DynamicInvoke();
            Expr e = Expr.Constant(enumerator, typeof(IEnumerator));

            EU.Enumerate(ref Expressions, new int[] { 1, 2, 3, -1, -2, 4 }, 6, e, ref Result, ref Value);

            var tree = EU.BlockVoid(new[] { Result, Value }, Expressions);
            V.Validate(tree);
            return tree;
        }
    }
}
