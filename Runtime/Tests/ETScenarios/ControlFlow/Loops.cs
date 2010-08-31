#if !CLR2
using System.Linq.Expressions;
#endif

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Scripting.Ast;

namespace ETScenarios.ControlFlow {
    using EU = ETUtils.ExpressionUtils;
    using Expr = Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public class Loops {
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Loops 1", new string[] { "positive", "loops", "controlflow", "Pri1" })]
        public static Expr Loops1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

            ParameterExpression Index = Expr.Variable(typeof(Int32), "");
            Expressions.Add(Expr.Assign(Index, Expr.Constant(1)));

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), Expr.LessThanOrEqual(Index, Expr.Constant(5)));

            Expr Incr = Expr.Block(EU.ConcatEquals(Result, "Incr"), Expr.Assign(Index, Expr.Add(Index, Expr.Constant(1))));

            Expressions.Add(AstUtils.Loop(Test, Incr, EU.ConcatEquals(Result, "Body"), EU.ConcatEquals(Result, "Else"), null, null));

            Expressions.Add(EU.GenAreEqual(Expr.Constant("TestBodyIncrTestBodyIncrTestBodyIncrTestBodyIncrTestBodyIncrTestElse"), Result, "Loop1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(6), Index, "Loop2"));

            var tree = EU.BlockVoid(new[] { Result, Index }, Expressions);
            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Loops 2", new string[] { "positive", "loops", "controlflow", "Pri1" })]
        public static Expr Loops2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

            ParameterExpression Index = Expr.Variable(typeof(Int32), "");
            Expressions.Add(Expr.Assign(Index, Expr.Constant(1)));

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), Expr.LessThanOrEqual(Index, Expr.Constant(5)));

            Expr Body = EU.BlockVoid(EU.ConcatEquals(Result, "Body"), Expr.Assign(Index, Expr.Add(Index, Expr.Constant(1))));

            LabelTarget @break = Expression.Label();

            Expressions.Add(
                Expr.Loop(
                    EU.BlockVoid(
                        Body,
                        Expression.Condition(
                            Test,
                            Expression.Empty(),
                            Expression.Break(@break)
                        )
                    ),
                    @break,
                    null
                )
            );

            Expressions.Add(EU.GenAreEqual(Expr.Constant("BodyTestBodyTestBodyTestBodyTestBodyTest"), Result, "Loop1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(6), Index, "Loop2"));

            var tree = EU.BlockVoid(new[] { Result, Index }, Expressions);
            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Loops 3", new string[] { "positive", "loops", "controlflow", "Pri1" })]
        public static Expr Loops3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

            ParameterExpression Index = Expr.Variable(typeof(Int32), "");
            ParameterExpression Jump = Expr.Variable(typeof(Int32), "");

            Expressions.Add(Expr.Assign(Index, Expr.Constant(1)));
            Expressions.Add(Expr.Assign(Jump, Expr.Constant(1)));

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), Expr.LessThanOrEqual(Index, Expr.Constant(5)));

            Expr Incr = Expr.Block(EU.ConcatEquals(Result, "Incr"), Expr.Assign(Index, Expr.Add(Index, Jump)));

            Expressions.Add(AstUtils.Loop(Test, Incr, EU.BlockVoid(EU.ConcatEquals(Result, "Body"), Expr.Assign(Jump, Expr.Constant(2))), EU.ConcatEquals(Result, "Else"), null, null));

            Expressions.Add(EU.GenAreEqual(Expr.Constant("TestBodyIncrTestBodyIncrTestBodyIncrTestElse"), Result, "Loop1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(7), Index, "Loop2"));

            var tree = EU.BlockVoid(new[] { Result, Index, Jump }, Expressions);
            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Loops 4", new string[] { "positive", "loops", "controlflow", "Pri1" })]
        public static Expr Loops4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

            ParameterExpression Index = Expr.Variable(typeof(Int32), "");
            Expressions.Add(Expr.Assign(Index, Expr.Constant(1)));

            LabelTarget ItsTheEnd = Expr.Label();

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), Expr.LessThanOrEqual(Index, Expr.Constant(5)));

            Expr Incr = Expr.Block(EU.ConcatEquals(Result, "Incr"), Expr.Assign(Index, Expr.Add(Index, Expr.Constant(1))));
            Expr Body = EU.BlockVoid(EU.ConcatEquals(Result, "Body"), AstUtils.If(new IfStatementTest[] { AstUtils.IfCondition(Expr.GreaterThan(Index, Expr.Constant(3)), Expr.Break(ItsTheEnd)) }, null));

            Expressions.Add(AstUtils.Loop(Test, Incr, Body, EU.ConcatEquals(Result, "Else"), ItsTheEnd, null));

            Expressions.Add(EU.GenAreEqual(Expr.Constant("TestBodyIncrTestBodyIncrTestBodyIncrTestBody"), Result, "Loop1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(4), Index, "Loop2"));

            var tree = EU.BlockVoid(new[] { Result, Index }, Expressions);
            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Loops 5", new string[] { "positive", "loops", "controlflow", "Pri1" })]
        public static Expr Loops5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

            ParameterExpression Index = Expr.Variable(typeof(Int32), "");
            Expressions.Add(Expr.Assign(Index, Expr.Constant(1)));

            LabelTarget ItsTheEnd = Expr.Label();

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), Expr.LessThanOrEqual(Index, Expr.Constant(5)));

            Expr Incr = Expr.Block(EU.ConcatEquals(Result, "Incr"), Expr.Assign(Index, Expr.Add(Index, Expr.Constant(1))));
            Expr Body = EU.BlockVoid(AstUtils.If(new IfStatementTest[] { AstUtils.IfCondition(Expr.GreaterThan(Index, Expr.Constant(3)), Expr.Continue(ItsTheEnd)) }, null), EU.ConcatEquals(Result, "Body"));

            Expressions.Add(AstUtils.Loop(Test, Incr, Body, EU.ConcatEquals(Result, "Else"), null, ItsTheEnd));

            Expressions.Add(EU.GenAreEqual(Expr.Constant("TestBodyIncrTestBodyIncrTestBodyIncrTestIncrTestIncrTestElse"), Result, "Loop1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(6), Index, "Loop2"));

            var tree = EU.BlockVoid(new[] { Result, Index }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Continue and Break with typeof(void) for code coverage, not using the factory with a type anywhere else.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Loops 6", new string[] { "positive", "loops", "controlflow", "Pri1" })]
        public static Expr Loops6(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

            ParameterExpression Index = Expr.Variable(typeof(Int32), "");
            Expressions.Add(Expr.Assign(Index, Expr.Constant(1)));

            LabelTarget ItsTheEnd = Expr.Label();

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), Expr.LessThanOrEqual(Index, Expr.Constant(5)));

            Expr Incr = Expr.Block(EU.ConcatEquals(Result, "Incr"), Expr.Assign(Index, Expr.Add(Index, Expr.Constant(1))));
            Expr Body = EU.BlockVoid(Incr, AstUtils.If(new IfStatementTest[] { AstUtils.IfCondition(Expr.GreaterThan(Index, Expr.Constant(3)), Expr.Continue(ItsTheEnd, typeof(void))) }, null), EU.ConcatEquals(Result, "Body"));

            LabelTarget @break = Expression.Label();

            Expressions.Add(
                Expr.Loop(
                    EU.BlockVoid(
                        Body,
                        Expression.Label(ItsTheEnd),
                        Expression.Condition(
                            Test,
                            Expression.Empty(),
                            Expression.Break(@break, typeof(void))
                        )
                    ),
                    @break,
                    null
                )
            );

            Expressions.Add(EU.GenAreEqual(Expr.Constant("IncrBodyTestIncrBodyTestIncrTestIncrTestIncrTest"), Result, "Loop1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(6), Index, "Loop2"));

            var tree = EU.BlockVoid(new[] { Result, Index }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Null for Test
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Loops 7", new string[] { "positive", "loops", "controlflow", "Pri2" })]
        public static Expr Loops7(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

            ParameterExpression Index = Expr.Variable(typeof(Int32), "");
            Expressions.Add(Expr.Assign(Index, Expr.Constant(1)));

            LabelTarget ItsTheEnd = Expr.Label();

            Expr Incr = Expr.Block(EU.ConcatEquals(Result, "Incr"), Expr.Assign(Index, Expr.Add(Index, Expr.Constant(1))));
            Expr Body = EU.BlockVoid(EU.ConcatEquals(Result, "Body"), AstUtils.If(new IfStatementTest[] { AstUtils.IfCondition(Expr.GreaterThan(Index, Expr.Constant(3)), Expr.Break(ItsTheEnd)) }, null));

            Expressions.Add(AstUtils.Loop(null, Incr, Body, EU.ConcatEquals(Result, "Else"), ItsTheEnd, null));

            var tree = EU.BlockVoid(new[] { Result, Index }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Null for body
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Loops 8", new string[] { "negative", "loops", "controlflow", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr Loops8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

            ParameterExpression Index = Expr.Variable(typeof(Int32), "");
            Expressions.Add(Expr.Assign(Index, Expr.Constant(1)));

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), Expr.LessThanOrEqual(Index, Expr.Constant(5)));

            Expr Incr = Expr.Block(EU.ConcatEquals(Result, "Incr"), Expr.Assign(Index, Expr.Add(Index, Expr.Constant(1))));

            Expressions.Add(EU.Throws<System.ArgumentNullException>(() =>
            {
                AstUtils.Loop(Test, Incr, null, EU.ConcatEquals(Result, "Else"), null, null);
            }));

            return null;
        }

        // Null for else
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Loops 9", new string[] { "positive", "loops", "controlflow", "Pri2" })]
        public static Expr Loops9(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

            ParameterExpression Index = Expr.Variable(typeof(Int32), "");
            Expressions.Add(Expr.Assign(Index, Expr.Constant(1)));

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), Expr.LessThanOrEqual(Index, Expr.Constant(5)));

            Expr Incr = Expr.Block(EU.ConcatEquals(Result, "Incr"), Expr.Assign(Index, Expr.Add(Index, Expr.Constant(1))));

            Expressions.Add(AstUtils.Loop(Test, Incr, EU.ConcatEquals(Result, "Body"), null, null, null));

            var tree = EU.BlockVoid(new[] { Result, Index }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass expression to Test that does not yield a value
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Loops 10", new string[] { "negative", "loops", "controlflow", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Loops10(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

            ParameterExpression Index = Expr.Variable(typeof(Int32), "");
            Expressions.Add(Expr.Assign(Index, Expr.Constant(1)));

            Expr Test = Expr.Empty();

            Expr Incr = Expr.Block(EU.ConcatEquals(Result, "Incr"), Expr.Assign(Index, Expr.Add(Index, Expr.Constant(1))));

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                AstUtils.Loop(Test, Incr, EU.ConcatEquals(Result, "Body"), EU.ConcatEquals(Result, "Else"), null, null);
            }));

            return null;
        }

        // Pass expression to Increment that does not yield a value
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Loops 11", new string[] { "positive", "loops", "controlflow", "Pri2" })]
        public static Expr Loops11(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

            ParameterExpression Index = Expr.Variable(typeof(Int32), "");
            Expressions.Add(Expr.Assign(Index, Expr.Constant(1)));

            LabelTarget ItsTheEnd = Expr.Label();

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), Expr.LessThanOrEqual(Index, Expr.Constant(5)));

            Expr Incr = Expr.Empty();

            Expr Body = EU.BlockVoid(
                EU.ConcatEquals(Result, "Body"),
                Expr.Assign(Index, Expr.Add(Index, Expr.Constant(1))),
                AstUtils.If(new IfStatementTest[] { AstUtils.IfCondition(Expr.GreaterThan(Index, Expr.Constant(3)), Expr.Break(ItsTheEnd)) }, null));

            Expressions.Add(AstUtils.Loop(Test, Incr, Body, EU.ConcatEquals(Result, "Else"), ItsTheEnd, null));

            var tree = EU.BlockVoid(new[] { Result, Index }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass expression to body that yields a value
        // Regression for Dev10 Bug 564162
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Loops 12", new string[] { "positive", "loops", "controlflow", "Pri2" })]
        public static Expr Loops12(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

            ParameterExpression Index = Expr.Variable(typeof(Int32), "");
            Expressions.Add(Expr.Assign(Index, Expr.Constant(0)));

            LabelTarget target = Expr.Label(typeof(int));

            Expr Loop =
                Expr.Loop(
                    Expr.Block(
                        EU.ConcatEquals(Result, Expr.Constant("Loop")),
                        Expr.Assign(Index, Expr.Add(Index, Expr.Constant(1))),
                        Expr.Condition(
                            Expr.Equal(Index, Expr.Constant(2)),
                            Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("Break")),
                                Expr.Break(target, Expr.Add(Index, Expr.Constant(1)), typeof(int))
                            ),
                            Expr.Constant(-1)
                        )
                    ),
                    target
                );

            Expressions.Add(EU.GenAreEqual(Expr.Constant(3), Loop, "Loop1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("LoopLoopBreak"), Result, "Loop2"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(2), Index, "Loop3"));

            var tree = Expr.Block(new[] { Result, Index }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass a scoped expression to Test
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Loops 13", new string[] { "positive", "loops", "controlflow", "Pri2" })]
        public static Expr Loops13(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

            ParameterExpression Index = Expr.Variable(typeof(Int32), "");
            Expressions.Add(Expression.Assign(Index, Expr.Constant(1)));

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), Expr.LessThanOrEqual(Index, Expr.Constant(5)));

            Expr Incr = Expr.Block(EU.ConcatEquals(Result, "Incr"), Expr.Assign(Index, Expr.Add(Index, Expr.Constant(1))));

            Expressions.Add(AstUtils.Loop(Test, Incr, EU.ConcatEquals(Result, "Body"), EU.ConcatEquals(Result, "Else"), null, null));

            Expressions.Add(EU.GenAreEqual(Expr.Constant("TestBodyIncrTestBodyIncrTestBodyIncrTestBodyIncrTestBodyIncrTestElse"), Result, "Loop1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(6), Index, "Loop2"));

            var tree = EU.BlockVoid(new[] { Result, Index }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass a scoped expression to Increment
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Loops 14", new string[] { "positive", "loops", "controlflow", "Pri2" })]
        public static Expr Loops14(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

            ParameterExpression Index = Expr.Variable(typeof(Int32), "");
            Expressions.Add(Expression.Assign(Index, Expr.Constant(1)));

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), Expr.LessThanOrEqual(Index, Expr.Constant(5)));

            Expr Incr = Expr.Block(EU.ConcatEquals(Result, "Incr"), Expr.Assign(Index, Expr.Add(Index, Expr.Constant(1))));

            Expressions.Add(AstUtils.Loop(Test, Incr, EU.ConcatEquals(Result, "Body"), EU.ConcatEquals(Result, "Else"), null, null));

            Expressions.Add(EU.GenAreEqual(Expr.Constant("TestBodyIncrTestBodyIncrTestBodyIncrTestBodyIncrTestBodyIncrTestElse"), Result, "Loop1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(6), Index, "Loop2"));

            var tree = EU.BlockVoid(new[] { Result, Index }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass a scoped expression to Body
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Loops 15", new string[] { "positive", "loops", "controlflow", "Pri2" })]
        public static Expr Loops15(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

            ParameterExpression Index = Expr.Variable(typeof(Int32), "");
            Expressions.Add(Expression.Assign(Index, Expr.Constant(1)));

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), Expr.LessThanOrEqual(Index, Expr.Constant(5)));

            Expr Incr = Expr.Block(EU.ConcatEquals(Result, "Incr"), Expr.Assign(Index, Expr.Add(Index, Expr.Constant(1))));

            Expr Body = Expr.Block(EU.ConcatEquals(Result, "Body"));

            Expressions.Add(AstUtils.Loop(Test, Incr, Body, EU.ConcatEquals(Result, "Else"), null, null));

            Expressions.Add(EU.GenAreEqual(Expr.Constant("TestBodyIncrTestBodyIncrTestBodyIncrTestBodyIncrTestBodyIncrTestElse"), Result, "Loop1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(6), Index, "Loop2"));

            var tree = EU.BlockVoid(new[] { Result, Index }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass a scoped expression to Else
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Loops 16", new string[] { "positive", "loops", "controlflow", "Pri2" })]
        public static Expr Loops16(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

            ParameterExpression Index = Expr.Variable(typeof(Int32), "");
            Expressions.Add(Expression.Assign(Index, Expr.Constant(1)));

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), Expr.LessThanOrEqual(Index, Expr.Constant(5)));

            Expr Incr = Expr.Block(EU.ConcatEquals(Result, "Incr"), Expr.Assign(Index, Expr.Add(Index, Expr.Constant(1))));

            Expr Else = Expr.Block(EU.ConcatEquals(Result, "Else"));

            Expressions.Add(AstUtils.Loop(Test, Incr, EU.ConcatEquals(Result, "Body"), Else, null, null));

            Expressions.Add(EU.GenAreEqual(Expr.Constant("TestBodyIncrTestBodyIncrTestBodyIncrTestBodyIncrTestBodyIncrTestElse"), Result, "Loop1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(6), Index, "Loop2"));

            var tree = EU.BlockVoid(new[] { Result, Index }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass a return expression to condition
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Loops 17", new string[] { "negative", "loops", "controlflow", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Loops17(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

            ParameterExpression Index = Expr.Variable(typeof(Int32), "");

            Expr Test = Expr.Return(Expression.Label(typeof(string)), EU.ConcatEquals(Result, "Return"));

            Expr Incr = Expr.Block(EU.ConcatEquals(Result, "Incr"), Expr.Assign(Index, Expr.Add(Index, Expr.Constant(1))));

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                AstUtils.Loop(Test, Incr, EU.ConcatEquals(Result, "Body"), EU.ConcatEquals(Result, "Else"), null, null);
            }));

            return Expr.Empty();
        }

        private static Expr TE(Type t, string Message) {
            ConstructorInfo ci = t.GetConstructor(new Type[] { typeof(String) });
            Expression Ex = Expr.New(ci, Expr.Constant(Message));
            return Ex;
        }

        // Throw exception from test expression
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Loops 18", new string[] { "positive", "loops", "controlfflow", "Pri2" })]
        public static Expr Loops18(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

            ParameterExpression Index = Expr.Variable(typeof(Int32), "");

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Try1"), Expr.Throw(TE(typeof(Exception), "Test")), EU.ConcatEquals(Result, "Try2"), Expr.LessThanOrEqual(Index, Expr.Constant(5)));

            Expr Incr = Expr.Block(EU.ConcatEquals(Result, "Incr"), Expr.Assign(Index, Expr.Add(Index, Expr.Constant(1))));

            Expr Body = AstUtils.Loop(Test, Incr, EU.ConcatEquals(Result, "Body"), EU.ConcatEquals(Result, "Else"), null, null);

            Expr HandlerBody1 = EU.BlockVoid(EU.ConcatEquals(Result, "HandlerBody"));
            CatchBlock CatchBlock1 = Expr.Catch(typeof(Exception), HandlerBody1);

            Expr FinallyBody = EU.ConcatEquals(Result, "FinallyBody");

            TryExpression Try = Expr.TryCatchFinally(Body, FinallyBody, new CatchBlock[] { CatchBlock1 });

            Expressions.Add(Try);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Try1HandlerBodyFinallyBody"), Result, "Try 1"));

            var tree = EU.BlockVoid(new[] { Result, Index }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Throw exception from increment expression
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Loops 19", new string[] { "positive", "loops", "controlfflow", "Pri2" })]
        public static Expr Loops19(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

            ParameterExpression Index = Expr.Variable(typeof(Int32), "");

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), Expr.LessThanOrEqual(Index, Expr.Constant(5)));

            Expr Incr = Expr.Block(EU.ConcatEquals(Result, "Try1"), Expr.Throw(TE(typeof(Exception), "Test")), EU.ConcatEquals(Result, "Try2"), Expr.Assign(Index, Expr.Add(Index, Expr.Constant(1))));

            Expr Body = AstUtils.Loop(Test, Incr, EU.ConcatEquals(Result, "Body"), EU.ConcatEquals(Result, "Else"), null, null);

            Expr HandlerBody1 = EU.BlockVoid(EU.ConcatEquals(Result, "HandlerBody"));
            CatchBlock CatchBlock1 = Expr.Catch(typeof(Exception), HandlerBody1);

            Expr FinallyBody = EU.ConcatEquals(Result, "FinallyBody");

            TryExpression Try = Expr.TryCatchFinally(Body, FinallyBody, new CatchBlock[] { CatchBlock1 });

            Expressions.Add(Try);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("TestBodyTry1HandlerBodyFinallyBody"), Result, "Try 1"));

            var tree = EU.BlockVoid(new[] { Result, Index }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // have a loop with a large number of iterations
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Loops 20", new string[] { "positive", "loops", "controlfflow", "Pri3" })]
        public static Expr Loops20(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

            ParameterExpression Index = Expr.Variable(typeof(Int32), "");

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), Expr.LessThanOrEqual(Index, Expr.Constant(1000)));

            Expr Incr = Expr.Block(EU.ConcatEquals(Result, "Incr"), Expr.Assign(Index, Expr.Add(Index, Expr.Constant(1))));

            Expr Body = EU.ConcatEquals(Result, "Body");

            Expressions.Add(AstUtils.Loop(Test, Incr, Body, EU.ConcatEquals(Result, "Else"), null, null));

            Expressions.Add(EU.GenAreEqual(Index, Expr.Constant(1001)));

            var tree = EU.BlockVoid(new[] { Result, Index }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass a non-value returning expression to condition
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Loops 21", new string[] { "negative", "loops", "controlfflow", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Loops21(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

            ParameterExpression Index = Expr.Variable(typeof(Int32), "");

            Expr Test = Expr.TryFinally(Expr.Empty(), Expr.Empty());

            Expr Incr = Expr.Block(EU.ConcatEquals(Result, "Incr"), Expr.Assign(Index, Expr.Add(Index, Expr.Constant(1))));

            Expr Body = EU.ConcatEquals(Result, "Body");

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                AstUtils.Loop(Test, Incr, Body, EU.ConcatEquals(Result, "Else"), null, null);
            }));

            return EU.BlockVoid(new[] { Result, Index }, Expressions);
        }

    }
}
