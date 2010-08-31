#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;

namespace ETScenarios.ControlFlow {
    using EU = ETUtils.ExpressionUtils;
    using Expr = Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public class Condition {
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Condition 1", new string[] { "positive", "condition", "controlflow", "Pri1" })]
        public static Expr Condition1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

            ParameterExpression Res = Expr.Variable(typeof(int), "");

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), Expr.Constant(true));
            Expr IfTrue = Expr.Block(EU.ConcatEquals(Result, "If True"), Expr.Constant(1));
            Expr IfFalse = Expr.Block(EU.ConcatEquals(Result, "If False"), Expr.Constant(2));

            Expressions.Add(Expr.Assign(Res, Expr.Condition(Test, IfTrue, IfFalse)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(1), Res, "Condition1"));

            Expressions.Add(EU.GenAreEqual(Expr.Constant("TestIf True"), Result, "Condition2"));

            Expressions.Add(Expr.Assign(Result, Expr.Constant("")));

            Test = Expr.Block(EU.ConcatEquals(Result, "Test"), Expr.Constant(false));
            IfTrue = Expr.Block(EU.ConcatEquals(Result, "If True"), Expr.Constant(1));
            IfFalse = Expr.Block(EU.ConcatEquals(Result, "If False"), Expr.Constant(2));

            Expressions.Add(Expr.Assign(Res, Expr.Condition(Test, IfTrue, IfFalse)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(2), Res, "Condition3"));

            Expressions.Add(EU.GenAreEqual(Expr.Constant("TestIf False"), Result, "Condition4"));

            var tree = EU.BlockVoid(new[] { Result, Res }, Expressions);
            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Condition 2", new string[] { "positive", "condition", "controlflow", "Pri1" })]
        public static Expr Condition2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

            ParameterExpression Res = Expr.Variable(typeof(int), "");

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), Expr.Constant(true));
            Expr IfTrue = Expr.Block(EU.ConcatEquals(Result, "If True"), Expr.Constant(1));
            Expr IfFalse = Expr.Block(EU.ConcatEquals(Result, "If False"), Expr.Constant(2));

            Expressions.Add(EU.GenAreEqual(EU.GetExprType(Expr.Constant(2)), EU.GetExprType(Expr.Condition(Test, IfTrue, IfFalse)), "Condition1"));

            var tree = EU.BlockVoid(new[] { Result, Res }, Expressions);
            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Condition 3", new string[] { "negative", "condition", "controlflow", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Condition3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

            ParameterExpression Res = Expr.Variable(typeof(int), "");

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), Expr.Constant(true));
            Expr IfTrue = Expr.Block(EU.ConcatEquals(Result, "If True"), Expr.Constant(1));
            Expr IfFalse = Expr.Block(EU.ConcatEquals(Result, "If False"), Expr.Constant((short)2, typeof(short)));

            EU.Throws<ArgumentException>(() => { Expr.Condition(Test, IfTrue, IfFalse); });

            return Expr.Empty();
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Condition 4", new string[] { "negative", "condition", "controlflow", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Condition4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

            ParameterExpression Res = Expr.Variable(typeof(int), "");

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), Expr.Constant(true));
            Expr IfTrue = Expr.Block(EU.ConcatEquals(Result, "If True"), Expr.Constant("test"));
            Expr IfFalse = Expr.Block(EU.ConcatEquals(Result, "If False"), Expr.Constant((short)2, typeof(short)));

            EU.Throws<ArgumentException>(() => { Expr.Condition(Test, IfTrue, IfFalse); });

            return Expr.Empty();
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Condition 5", new string[] { "negative", "condition", "controlflow", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Condition5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

            ParameterExpression Res = Expr.Variable(typeof(int), "");

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), Expr.Constant(true));
            Expr IfTrue = Expr.Block(EU.ConcatEquals(Result, "If True"), AstUtils.Void(Expr.Constant(1)));
            Expr IfFalse = Expr.Block(EU.ConcatEquals(Result, "If False"), Expr.Constant(2));

            EU.Throws<ArgumentException>(() => { Expr.Condition(Test, IfTrue, IfFalse); });

            return Expr.Empty();
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Condition 6", new string[] { "positive", "condition", "controlflow", "Pri1" })]
        public static Expr Condition6(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

            ParameterExpression Res = Expr.Variable(typeof(object), "");

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), Expr.Constant(true));
            Expr IfTrue = Expr.Block(EU.ConcatEquals(Result, "If True"), AstUtils.Void(Expr.Constant(1)));
            Expr IfFalse = Expr.Block(EU.ConcatEquals(Result, "If False"), AstUtils.Void(Expr.Constant(2)));

            Expressions.Add(EU.GenAreEqual(EU.GetExprType(AstUtils.Void(Expr.Constant(3))), EU.GetExprType(Expr.Condition(Test, IfTrue, IfFalse)), "Condition1"));

            Expressions.Add(EU.GenAreEqual(Expr.Constant("TestIf True"), Result, "Condition2"));

            var tree = Expr.Empty();
            V.Validate(tree);
            return tree;

        }

        // pass null to test
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Condition 7", new string[] { "negative", "condition", "controlflow", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr Condition7(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression Res = Expr.Variable(typeof(object), "");

            Expr Test = null;
            Expr IfTrue = Expr.Block(EU.ConcatEquals(Result, "If True"), Expr.Constant(1));
            Expr IfFalse = Expr.Block(EU.ConcatEquals(Result, "If False"), Expr.Constant(2));

            Expressions.Add(EU.Throws<System.ArgumentNullException>(() =>
            {
                Expr.Assign(Res, Expr.Condition(Test, IfTrue, IfFalse));
            }));

            return Expr.Empty();
        }

        // pass null to ifTrue
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Condition 8", new string[] { "negative", "condition", "controlflow", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr Condition8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression Res = Expr.Variable(typeof(object), "");

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), Expr.Constant(true));
            Expr IfTrue = null;
            Expr IfFalse = Expr.Block(EU.ConcatEquals(Result, "If False"), Expr.Constant(2));

            Expressions.Add(EU.Throws<System.ArgumentNullException>(() =>
            {
                Expr.Assign(Res, Expr.Condition(Test, IfTrue, IfFalse));
            }));

            return Expr.Empty();
        }

        // pass null to ifFalse
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Condition 9", new string[] { "negative", "condition", "controlflow", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr Condition9(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression Res = Expr.Variable(typeof(object), "");

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), Expr.Constant(true));
            Expr IfTrue = Expr.Block(EU.ConcatEquals(Result, "If True"), Expr.Constant(1));
            Expr IfFalse = null;

            Expressions.Add(EU.Throws<System.ArgumentNullException>(() =>
            {
                Expr.Assign(Res, Expr.Condition(Test, IfTrue, IfFalse));
            }));

            return Expr.Empty();
        }

        // pass null to annotations
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Condition 10", new string[] { "positive", "condition", "controlflow", "Pri3" })]
        public static Expr Condition10(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression Res = Expr.Variable(typeof(Int32), "");

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), Expr.Constant(true));
            Expr IfTrue = Expr.Block(EU.ConcatEquals(Result, "If True"), Expr.Constant(1));
            Expr IfFalse = Expr.Block(EU.ConcatEquals(Result, "If False"), Expr.Constant(2));

            Expressions.Add(Expr.Assign(Res, Expr.Condition(Test, IfTrue, IfFalse)));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("TestIf True"), Result, "Condition1"));
            Expressions.Add(EU.GenAreEqual(EU.GetExprType(Expr.Constant(3)), EU.GetExprType(Res), "Condition2"));

            var tree = EU.BlockVoid(new[] { Result, Res }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // pass expression not returning a value to test
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Condition 11", new string[] { "negative", "condition", "controlflow", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Condition11(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression Res = Expr.Variable(typeof(object), "");

            Expr Test = EU.BlockVoid(EU.ConcatEquals(Result, "Test"));
            Expr IfTrue = Expr.Block(EU.ConcatEquals(Result, "If True"), Expr.Constant(1));
            Expr IfFalse = Expr.Block(EU.ConcatEquals(Result, "If False"), Expr.Constant(2));

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.Assign(Res, Expr.Condition(Test, IfTrue, IfFalse));
            }));

            return Expr.Empty();
        }

        // pass expression not returning a value to ifTrue
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Condition 12", new string[] { "negative", "condition", "controlflow", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Condition12(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression Res = Expr.Variable(typeof(object), "");

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), Expr.Constant(true));
            Expr IfTrue = EU.BlockVoid(EU.ConcatEquals(Result, "If True"));
            Expr IfFalse = Expr.Block(EU.ConcatEquals(Result, "If False"), Expr.Constant(2));

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.Assign(Res, Expr.Condition(Test, IfTrue, IfFalse));
            }));

            return Expr.Empty();
        }

        // pass expression not returning a value to ifFalse
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Condition 13", new string[] { "negative", "condition", "controlflow", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Condition13(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression Res = Expr.Variable(typeof(object), "");

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), Expr.Constant(true));
            Expr IfTrue = Expr.Block(EU.ConcatEquals(Result, "If True"), Expr.Constant(1));
            Expr IfFalse = EU.BlockVoid(EU.ConcatEquals(Result, "If False"));

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.Assign(Res, Expr.Condition(Test, IfTrue, IfFalse));
            }));

            return Expr.Empty();
        }

        // pass non Boolean expression to test
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Condition 14", new string[] { "negative", "condition", "controlflow", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Condition14(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression Res = Expr.Variable(typeof(object), "");

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), Expr.Constant("NonBool"));
            Expr IfTrue = Expr.Block(EU.ConcatEquals(Result, "If True"), Expr.Constant(1));
            Expr IfFalse = Expr.Block(EU.ConcatEquals(Result, "If False"), Expr.Constant(2));

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.Assign(Res, Expr.Condition(Test, IfTrue, IfFalse));
            }));

            return Expr.Empty();
        }

        // pass scope wrapped expression to test
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Condition 15", new string[] { "positive", "condition", "controlflow", "Pri2" })]
        public static Expr Condition15(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression Res = Expr.Variable(typeof(Int32), "");
            ParameterExpression ScopedVal = Expr.Variable(typeof(bool), "");

            Expr Test = Expr.Block(
                new ParameterExpression[] { ScopedVal },
                EU.ConcatEquals(Result, "Test"),
                Expr.Assign(ScopedVal, Expr.Constant(false)),
                ScopedVal
            );


            Expr IfTrue = Expr.Block(EU.ConcatEquals(Result, "If True"), Expr.Constant(1));
            Expr IfFalse = Expr.Block(EU.ConcatEquals(Result, "If False"), Expr.Constant(2));

            Expressions.Add(Expr.Assign(Res, Expr.Condition(Test, IfTrue, IfFalse)));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("TestIf False"), Result, "Condition1"));
            Expressions.Add(EU.GenAreEqual(EU.GetExprType(Expr.Constant(3)), EU.GetExprType(Res), "Condition2"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(2), Res));

            var tree = EU.BlockVoid(new[] { Result, Res }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // pass scope wrapped expression to ifTrue
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Condition 16", new string[] { "positive", "condition", "controlflow", "Pri2" })]
        public static Expr Condition16(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression Res = Expr.Variable(typeof(Int32), "");
            ParameterExpression ScopedVal = Expr.Variable(typeof(Int32), "");

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), Expr.Constant(true));
            Expr IfTrue =
                Expr.Block(
                    new ParameterExpression[] { ScopedVal },
                    EU.ConcatEquals(Result, "If True"),
                    Expr.Assign(ScopedVal, Expr.Constant(1)),
                    ScopedVal
                );

            Expr IfFalse = Expr.Block(EU.ConcatEquals(Result, "If False"), Expr.Constant(2));

            Expressions.Add(Expr.Assign(Res, Expr.Condition(Test, IfTrue, IfFalse)));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("TestIf True"), Result, "Condition1"));
            Expressions.Add(EU.GenAreEqual(EU.GetExprType(Expr.Constant(3)), EU.GetExprType(Res), "Condition2"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(1), Res));

            var tree = EU.BlockVoid(new[] { Result, Res }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // pass scope wrapped expression to ifFalse
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Condition 17", new string[] { "positive", "condition", "controlflow", "Pri2" })]
        public static Expr Condition17(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression Res = Expr.Variable(typeof(Int32), "");
            ParameterExpression ScopedVal = Expr.Variable(typeof(Int32), "");

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), Expr.Constant(false));
            Expr IfTrue = Expr.Block(EU.ConcatEquals(Result, "If True"), Expr.Constant(1));
            Expr IfFalse =
                Expr.Block(
                    new ParameterExpression[] { ScopedVal },
                    EU.ConcatEquals(Result, "If False"),
                    Expr.Assign(ScopedVal, Expr.Constant(2)),
                    ScopedVal
                );


            Expressions.Add(Expr.Assign(Res, Expr.Condition(Test, IfTrue, IfFalse)));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("TestIf False"), Result, "Condition1"));
            Expressions.Add(EU.GenAreEqual(EU.GetExprType(Expr.Constant(3)), EU.GetExprType(Res), "Condition2"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(2), Res));

            var tree = EU.BlockVoid(new[] { Result, Res }, Expressions);
            V.Validate(tree);
            return tree;
        }

        internal class TestClass {
            internal TestClass() { }
            bool IsTrue() { return true; }
            bool IsFalse() { return false; }
        }

        // Test is not Boolean but defines istrue, isfalse
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Condition 18", new string[] { "negative", "condition", "controlflow", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Condition18(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression Res = Expr.Variable(typeof(Int32), "");
            ParameterExpression TestValue = Expr.Variable(typeof(TestClass), "");

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(new TestClass())));

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), TestValue);
            Expr IfTrue = Expr.Block(EU.ConcatEquals(Result, "If True"), Expr.Constant(1));
            Expr IfFalse = Expr.Block(EU.ConcatEquals(Result, "If False"), Expr.Constant(2));

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.Assign(Res, Expr.Condition(Test, IfTrue, IfFalse));
            }));

            return Expr.Empty();
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Condition 19", new string[] { "negative", "condition", "controlflow", "Pri1" }, Exception = typeof(ArgumentNullException))]
        public static Expr Condition19(EU.IValidator V) {
            return EU.Throws<ArgumentNullException>(() => { Expr.IfThen(null, Expr.Constant(1)); });
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Condition 20", new string[] { "negative", "condition", "controlflow", "Pri1" }, Exception = typeof(ArgumentNullException))]
        public static Expr Condition20(EU.IValidator V) {
            return EU.Throws<ArgumentNullException>(() => { Expr.IfThen(Expr.Constant(true), null); });
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Condition 21", new string[] { "positive", "condition", "controlflow", "Pri1" })]
        public static Expr Condition21(EU.IValidator V) {
            var tree = EU.GenAreEqual(Expr.Constant(true), Expr.Constant(Expr.IfThen(Expr.Constant(true), Expr.Constant(1)).Type == typeof(void)), "condition 1");
            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Condition 22", new string[] { "positive", "condition", "controlflow", "Pri1" })]
        public static Expr Condition22(EU.IValidator V) {
            var Res = Expr.Parameter(typeof(int), "");
            var tree = Expr.Block(
                new[] { Res },
                Expr.IfThen(Expr.Constant(true), Expr.Assign(Res, Expr.Constant(1))),
                EU.GenAreEqual(Expr.Constant(1), Res, "ifthen 1")
            );

            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Condition 23", new string[] { "positive", "condition", "controlflow", "Pri1" })]
        public static Expr Condition23(EU.IValidator V) {
            var Res = Expr.Parameter(typeof(int), "");
            var tree = Expr.Block(
                new[] { Res },
                Expr.IfThen(Expr.Constant(false), Expr.Assign(Res, Expr.Constant(1))),
                EU.GenAreEqual(Expr.Constant(0), Res, "ifthen 1")
            );

            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Condition 24", new string[] { "negative", "condition", "controlflow", "Pri1" }, Exception = typeof(ArgumentNullException))]
        public static Expr Condition24(EU.IValidator V) {
            return EU.Throws<ArgumentNullException>(() => { Expr.IfThenElse(null, Expr.Constant(1), Expr.Constant(1)); });
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Condition 25", new string[] { "negative", "condition", "controlflow", "Pri1" }, Exception = typeof(ArgumentNullException))]
        public static Expr Condition25(EU.IValidator V) {
            return EU.Throws<ArgumentNullException>(() => { Expr.IfThenElse(Expr.Constant(true), null, Expr.Constant(1)); });
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Condition 26", new string[] { "negative", "condition", "controlflow", "Pri1" }, Exception = typeof(ArgumentNullException))]
        public static Expr Condition26(EU.IValidator V) {
            return EU.Throws<ArgumentNullException>(() => { Expr.IfThenElse(Expr.Constant(true), Expr.Constant(1), null); });
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Condition 27", new string[] { "positive", "condition", "controlflow", "Pri1" })]
        public static Expr Condition27(EU.IValidator V) {
            var tree = EU.GenAreEqual(Expr.Constant(true), Expr.Constant(Expr.IfThenElse(Expr.Constant(true), Expr.Constant(1), Expr.Constant(1)).Type == typeof(void)), "condition 1");
            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Condition 28", new string[] { "positive", "condition", "controlflow", "Pri1" })]
        public static Expr Condition28(EU.IValidator V) {
            var tree = EU.GenAreEqual(Expr.Constant(true), Expr.Constant(Expr.IfThenElse(Expr.Constant(true), Expr.Constant(1), Expr.Constant("1")).Type == typeof(void)), "condition 1");
            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Condition 29", new string[] { "positive", "condition", "controlflow", "Pri1" })]
        public static Expr Condition29(EU.IValidator V) {
            var Res = Expr.Parameter(typeof(int), "");
            var tree = Expr.Block(
                new[] { Res },
                Expr.IfThenElse(Expr.Constant(true), Expr.Assign(Res, Expr.Constant(1)), Expr.Assign(Res, Expr.Constant(2))),
                EU.GenAreEqual(Expr.Constant(1), Res, "ifthen 1")
            );

            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Condition 30", new string[] { "positive", "condition", "controlflow", "Pri1" })]
        public static Expr Condition30(EU.IValidator V) {
            var Res = Expr.Parameter(typeof(int), "");
            var tree = Expr.Block(
                new[] { Res },
                Expr.IfThenElse(Expr.Constant(false), Expr.Assign(Res, Expr.Constant(1)), Expr.Assign(Res, Expr.Constant(2))),
                EU.GenAreEqual(Expr.Constant(2), Res, "ifthen 1")
            );

            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Condition 31", new string[] { "negative", "condition", "controlflow", "Pri1" }, Exception = typeof(ArgumentNullException))]
        public static Expr Condition31(EU.IValidator V) {
            var tree =
                EU.Throws<System.ArgumentNullException>(() =>
                {
                    Expr.Block(
                    Expr.Condition(
                        Expr.Constant(true),
                        Expr.Constant(1),
                        Expr.Constant(2),
                        null
                    ));
                });

            return Expr.Empty();
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Condition 32", new string[] { "negative", "condition", "controlflow", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Condition32(EU.IValidator V) {
            var tree =
                EU.Throws<ArgumentException>(() =>
                {
                    Expr.Block(
                    Expr.Condition(
                        Expr.Constant(true),
                        Expr.Constant(1),
                        Expr.Constant(2),
                        typeof(long)
                    )
                );
                });

            return Expr.Empty();
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Condition 33", new string[] { "positive", "condition", "controlflow", "Pri1" })]
        public static Expr Condition33(EU.IValidator V) {
            var tree = Expr.Block(
                EU.GenAreEqual(
                    Expr.Constant(2),
                    Expr.Condition(
                        Expr.Constant(false),
                        Expr.Constant(1),
                        Expr.Constant(2),
                        typeof(int)
                    ),
                    "Condition 1"
                )
            );

            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Condition 34", new string[] { "positive", "condition", "controlflow", "Pri1" })]
        public static Expr Condition34(EU.IValidator V) {
            var tree = Expr.Block(
                EU.GenAreEqual(
                    Expr.Constant(typeof(ArgumentException), typeof(Type)),
                    Expr.Call(
                        Expr.Condition(
                            Expr.Constant(false),
                            Expr.Constant(new InvalidCastException()),
                            Expr.Constant(new ArgumentException()),
                            typeof(Exception)
                        ),
                        typeof(object).GetMethod("GetType")
                    ),
                    "Condition 1"
                )
            );

            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Condition 35", new string[] { "positive", "condition", "controlflow", "Pri1" })]
        public static Expr Condition35(EU.IValidator V) {
            var tree = Expr.Block(
                EU.GenAreEqual(
                    Expr.Constant(typeof(void), typeof(Type)),
                    Expr.Constant(
                        Expr.Condition(
                            Expr.Constant(false),
                            Expr.Constant(new InvalidCastException()),
                            Expr.Constant(1),
                            typeof(void)
                        ).Type,
                        typeof(Type)
                    ),
                    "Condition 1"
                )
            );

            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Condition 36", new string[] { "negative", "condition", "controlflow", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Condition36(EU.IValidator V) {
            var tree = EU.Throws<ArgumentException>(() =>
	            {
	                Expr.Block(
	                    Expr.Condition(
	                        Expr.Constant(1),
	                        Expr.Constant(1),
	                        Expr.Constant(2),
	                        typeof(void)
	                    )
                	);
				});

            return Expr.Empty();
        }
    }
}
