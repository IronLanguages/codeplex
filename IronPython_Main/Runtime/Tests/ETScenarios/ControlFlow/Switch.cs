#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using MsSc = System.Dynamic;

namespace ETScenarios.ControlFlow {
    using EU = ETUtils.ExpressionUtils;
    using Expr = Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public class Switch {

        // Switch 1, Switch 2 removed (they were testing SwitchBuilder, which was removed)

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 3", new string[] { "positive", "switch", "controlflow", "Pri1" })]
        public static Expr Switch3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression TestValue = Expr.Variable(typeof(Int32), "");

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), TestValue);

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(5)));


            SwitchExpression MySw = Expr.Switch(Test, AstUtils.Void(EU.ConcatEquals(Result, "Default")), EU.SwitchCase(5, EU.ConcatEquals(Result, "C5_")), EU.SwitchCase(2, EU.ConcatEquals(Result, "C2_")));
            Expressions.Add(MySw);

            Expressions.Add(EU.GenAreEqual(Expr.Constant("TestC5_"), Result, "Switch 1"));

            var tree = EU.BlockVoid(new[] { Result, TestValue }, Expressions);
            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 4", new string[] { "negative", "switch", "controlflow", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Switch4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression TestValue = Expr.Variable(typeof(Int32), "");

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), TestValue);

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(5)));


            var MySw = EU.Throws<System.ArgumentException>(() =>
            {
                Expr.Switch(Test, (Expr)null, (MethodInfo)null, null);
            });

            return Expr.Empty();
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 5", new string[] { "negative", "switch", "controlflow", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Switch5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression TestValue = Expr.Variable(typeof(Int32), "");

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), TestValue);

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(5)));


            var MySw = EU.Throws<System.ArgumentException>(() =>
            {
                Expr.Switch(Test, (Expr)null, new SwitchCase[] { });
            });
            return Expr.Empty();
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 6", new string[] { "negative", "switch", "controlflow", "Pri1" }, Exception = typeof(ArgumentNullException))]
        public static Expr Switch6(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression TestValue = Expr.Variable(typeof(Int32), "");

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), TestValue);

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(5)));


            var MySw = EU.Throws<System.ArgumentNullException>(() =>
            {
                Expr.Switch(Test, (Expr)null, new SwitchCase[] { null });
            });
            return Expr.Empty();
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 7", new string[] { "negative", "switch", "controlflow", "Pri1" }, Exception = typeof(ArgumentNullException))]
        public static Expr Switch7(EU.IValidator V) {
            EU.Throws<ArgumentException>(() =>
            {
                Expr.SwitchCase(null, Expression.Constant(1));
            });

            return Expr.Empty();
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 8", new string[] { "positive", "switch", "controlflow", "Pri1" })]
        public static Expr Switch8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression TestValue = Expr.Variable(typeof(Int32), "");

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), TestValue);

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(Int32.MaxValue)));


            SwitchExpression MySw = Expr.Switch(Test, AstUtils.Void(EU.ConcatEquals(Result, "Default")), EU.SwitchCase(Int32.MaxValue, EU.ConcatEquals(Result, "CMax_")), EU.SwitchCase(Int32.MinValue, EU.ConcatEquals(Result, "CMin_")));
            Expressions.Add(MySw);

            Expressions.Add(EU.GenAreEqual(Expr.Constant("TestCMax_"), Result, "Switch 1"));

            var tree = EU.BlockVoid(new[] { Result, TestValue }, Expressions);
            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 9", new string[] { "positive", "switch", "controlflow", "Pri1" })]
        public static Expr Switch9(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression TestValue = Expr.Variable(typeof(Int32), "");

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), TestValue);

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(5)));


            SwitchExpression MySw = Expr.Switch(Test, AstUtils.Void(EU.ConcatEquals(Result, "Default")), (MethodInfo)null, EU.SwitchCase(5, EU.ConcatEquals(Result, "CMax_")), EU.SwitchCase(0, EU.ConcatEquals(Result, "CMin_")));
            Expressions.Add(MySw);

            Expressions.Add(EU.GenAreEqual(Expr.Constant("TestCMax_"), Result, "Switch 1"));

            Expressions.Add(Expr.Assign(Result, Expr.Constant("")));

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(0)));

            Expressions.Add(MySw);

            Expressions.Add(EU.GenAreEqual(Expr.Constant("TestCMin_"), Result, "Switch 2"));


            var tree = EU.BlockVoid(new[] { Result, TestValue }, Expressions);
            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 10", new string[] { "negative", "switch", "controlflow", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Switch10(EU.IValidator V) {
            // was testing Expresison.DefaultCase
            // now tests passing a null test value
            EU.Throws<ArgumentException>(() =>
            {
                Expr.SwitchCase(Expression.Constant(1), null);
            });

            return Expr.Empty();
        }

        // Duplicate case values is no longer an error
        // Switch will match the first one
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 11", new string[] { "positive", "switch", "controlflow", "Pri1" })]
        public static Expr Switch11(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression TestValue = Expr.Variable(typeof(Int32), "");

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), TestValue);

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(5)));

            SwitchExpression MySw = Expr.Switch(
                Test,
                AstUtils.Void(EU.ConcatEquals(Result, "Default")),
                EU.SwitchCase(5, EU.ConcatEquals(Result, "C5_1_")),
                EU.SwitchCase(5, EU.ConcatEquals(Result, "C5_2_")),
                EU.SwitchCase(5, EU.ConcatEquals(Result, "C5_3_"))
            );
            Expressions.Add(MySw);

            Expressions.Add(EU.GenAreEqual(Expr.Constant("TestC5_1_"), Result, "Switch 1"));

            Expressions.Add(Expr.Assign(Result, Expr.Constant("")));

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(0)));

            Expressions.Add(MySw);

            Expressions.Add(EU.GenAreEqual(Expr.Constant("TestDefault"), Result, "Switch 2"));

            var tree = EU.BlockVoid(new[] { Result, TestValue }, Expressions);
            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 12", new string[] { "positive", "switch", "controlflow", "Pri1" })]
        public static Expr Switch12(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression TestValue = Expr.Variable(typeof(Int32), "");

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), TestValue);

            LabelTarget ThisIsTheEnd = Expr.Label();

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(6)));

            SwitchCase Case1 = EU.SwitchCase(5, EU.BlockVoid(EU.ConcatEquals(Result, "C5_"), Expr.Break(ThisIsTheEnd)));

            SwitchCase Case2 = EU.SwitchCase(6, EU.BlockVoid(EU.ConcatEquals(Result, "C6_"), Expr.Break(ThisIsTheEnd)));

            SwitchCase Case3 = EU.SwitchCase(7, EU.BlockVoid(EU.ConcatEquals(Result, "C7_"), Expr.Break(ThisIsTheEnd)));

            Expression Default = EU.BlockVoid(EU.ConcatEquals(Result, "Default"), Expr.Break(ThisIsTheEnd));

            var MySw = Expr.Block(Expr.Switch(Test, Default, Case1, Case2, Case3), Expr.Label(ThisIsTheEnd));
            Expressions.Add(MySw);

            Expressions.Add(EU.GenAreEqual(Expr.Constant("TestC6_"), Result, "Switch 1"));

            Expressions.Add(Expr.Assign(Result, Expr.Constant("")));

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(0)));

            Expressions.Add(MySw);

            Expressions.Add(EU.GenAreEqual(Expr.Constant("TestDefault"), Result, "Switch 2"));


            var tree = EU.BlockVoid(new[] { Result, TestValue }, Expressions);
            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 13", new string[] { "positive", "switch", "controlflow", "Pri1" })]
        public static Expr Switch13(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression TestValue = Expr.Variable(typeof(Int32), "");

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), TestValue);

            LabelTarget ThisIsTheEnd = Expr.Label();

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(6)));

            SwitchCase Case1 = EU.SwitchCase(5, EU.BlockVoid(EU.ConcatEquals(Result, "C5_")));

            SwitchCase Case2 = EU.SwitchCase(6, EU.BlockVoid(EU.ConcatEquals(Result, "C6_")));

            SwitchCase Case3 = EU.SwitchCase(7, EU.BlockVoid(EU.ConcatEquals(Result, "C7_")));

            Expression Default = EU.BlockVoid(EU.ConcatEquals(Result, "Default"));


            var MySw = Expr.Block(Expr.Switch(Test, Default, Case1, Case2, Case3), Expr.Label(ThisIsTheEnd));
            Expressions.Add(MySw);

            Expressions.Add(EU.GenAreEqual(Expr.Constant("TestC6_"), Result, "Switch 1"));

            Expressions.Add(Expr.Assign(Result, Expr.Constant("")));

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(0)));

            Expressions.Add(MySw);

            Expressions.Add(EU.GenAreEqual(Expr.Constant("TestDefault"), Result, "Switch 2"));


            var tree = EU.BlockVoid(new[] { Result, TestValue }, Expressions);
            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 14", new string[] { "positive", "switch", "controlflow", "Pri1" })]
        public static Expr Switch14(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression TestValue = Expr.Variable(typeof(Int32), "");

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), TestValue);

            LabelTarget ThisIsTheEnd = Expr.Label();

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(6)));

            SwitchCase Case1 = EU.SwitchCase(5, EU.BlockVoid(EU.ConcatEquals(Result, "C5_")));

            SwitchCase Case2 = EU.SwitchCase(6, EU.BlockVoid(EU.ConcatEquals(Result, "C6_")));

            SwitchCase Case3 = EU.SwitchCase(7, EU.BlockVoid(EU.ConcatEquals(Result, "C7_")));

            Expr Default = EU.BlockVoid(EU.ConcatEquals(Result, "Default"));


            var MySw = Expr.Block(Expr.Switch(Test, Default, Case1, Case2, Case3), Expr.Label(ThisIsTheEnd));
            Expressions.Add(MySw);

            Expressions.Add(EU.GenAreEqual(Expr.Constant("TestC6_"), Result, "Switch 1"));

            Expressions.Add(Expr.Assign(Result, Expr.Constant("")));

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(0)));

            Expressions.Add(MySw);

            Expressions.Add(EU.GenAreEqual(Expr.Constant("TestDefault"), Result, "Switch 2"));


            var tree = EU.BlockVoid(new[] { Result, TestValue }, Expressions);
            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 15", new string[] { "positive", "switch", "Loops", "controlflow", "Pri1" })]
        public static Expr Switch15(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression Index = Expr.Variable(typeof(Int32), "");

            LabelTarget ThisIsTheEnd = Expr.Label();

            Expressions.Add(Expr.Assign(Index, Expr.Constant(1)));

            Expr LoopTest = Expr.Block(EU.ConcatEquals(Result, "LoopTest"), Expr.LessThanOrEqual(Index, Expr.Constant(5)));

            Expr Incr = Expr.Block(EU.ConcatEquals(Result, "LoopIncr"), Expr.Assign(Index, Expr.Add(Index, Expr.Constant(1))));

            LabelTarget ThisIsTheLoopEnd = Expr.Label();

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), Index);


            SwitchCase Case1 = EU.SwitchCase(1, EU.BlockVoid(EU.ConcatEquals(Result, "C1_"), Expr.Break(ThisIsTheEnd)));

            SwitchCase Case2 = EU.SwitchCase(2, EU.BlockVoid(EU.ConcatEquals(Result, "C2_"), Expr.Break(ThisIsTheEnd)));

            SwitchCase Case3 = EU.SwitchCase(3, EU.BlockVoid(EU.ConcatEquals(Result, "C3_"), Expr.Break(ThisIsTheEnd)));

            SwitchCase Case4 = EU.SwitchCase(4, EU.BlockVoid(EU.ConcatEquals(Result, "C4_"), Expr.Continue(ThisIsTheLoopEnd)));

            Expr Default = EU.BlockVoid(EU.ConcatEquals(Result, "Default"), Expr.Break(ThisIsTheEnd));


            var MySw = Expr.Label(ThisIsTheEnd, Expr.Switch(Test, Default, Case1, Case2, Case3, Case4));

            Expressions.Add(AstUtils.Loop(LoopTest, Incr, EU.BlockVoid(MySw, EU.ConcatEquals(Result, "LoopBottom")), EU.ConcatEquals(Result, "Else"), null, ThisIsTheLoopEnd));

            Expressions.Add(EU.GenAreEqual(Expr.Constant("LoopTestTestC1_LoopBottomLoopIncrLoopTestTestC2_LoopBottomLoopIncrLoopTestTestC3_LoopBottomLoopIncrLoopTestTestC4_LoopIncrLoopTestTestDefaultLoopBottomLoopIncrLoopTestElse"), Result, "Switch 1"));


            var tree = EU.BlockVoid(new[] { Result, Index }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Switch 16 removed (negative test for multiple default cases, which cannot be constructed any more)

        // Create a simple switch statement with two cases. Go into each case.
        // First instance tests fall through from first case, second tests break, third hits other case
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 17", new string[] { "positive", "switch", "controlflow", "Pri2" })]
        public static Expr Switch17(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression TestValue = Expr.Variable(typeof(Int32), "");

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), TestValue);

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(5)));

            LabelTarget ThisIsTheEnd = Expr.Label("ThisIsTheEnd");
            LabelTarget Case2Label = Expr.Label("Case2Label");
            SwitchCase Case1 = EU.SwitchCase(5, EU.BlockVoid(EU.ConcatEquals(Result, "C5_"), Expression.Goto(Case2Label)));
            SwitchCase Case2 = EU.SwitchCase(2, EU.BlockVoid(Expression.Label(Case2Label), EU.ConcatEquals(Result, "C2_")));
            Expr MySw = Expr.Switch(Test, Case1, Case2);

            Expressions.Add(MySw);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("TestC5_C2_"), Result, "Switch 1"));

            Expressions.Add(Expr.Assign(Result, Expr.Constant("")));
            Case1 = EU.SwitchCase(5, EU.BlockVoid(EU.ConcatEquals(Result, "C5_"), Expr.Break(ThisIsTheEnd)));
            MySw = Expr.Block(Expr.Switch(Test, Case1, Case2), Expr.Label(ThisIsTheEnd));
            Expressions.Add(MySw);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("TestC5_"), Result, "Switch 2"));

            Expressions.Add(Expr.Assign(Result, Expr.Constant("")));
            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(2)));
            Expressions.Add(MySw);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("TestC2_"), Result, "Switch 3"));

            var tree = EU.BlockVoid(new[] { Result, TestValue }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass a null argument to default body
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 18", new string[] { "positive", "switch", "controlflow", "Pri2" })]
        public static Expr Switch18(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression TestValue = Expr.Variable(typeof(Int32), "");

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), TestValue);

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(5)));

            SwitchCase Case1 = EU.SwitchCase(5, EU.BlockVoid(EU.ConcatEquals(Result, "C5_")));
            SwitchExpression MySw = Expr.Switch(Test, (Expr)null, Case1);
            Expressions.Add(MySw);

            var tree = EU.BlockVoid(new[] { Result, TestValue }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Switch 20 removed (was: pass a null argument to annotations)

        // Pass a null argument to test
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 21", new string[] { "negative", "switch", "controlflow", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr Switch21(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression TestValue = Expr.Variable(typeof(Int32), "");

            Expr Test = null;

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(5)));

            SwitchCase Case1 = EU.SwitchCase(5, EU.BlockVoid(EU.ConcatEquals(Result, "C5_")));
            var MySw = EU.Throws<System.ArgumentNullException>(() =>
            {
                Expr.Switch(Test, Case1);
            });
            Expressions.Add(MySw);

            return Expr.Empty();
        }

        // Pass a scoped wrap expression to test
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 22", new string[] { "positive", "switch", "controlflow", "Pri2" })]
        public static Expr Switch22(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression TestValue = Expr.Variable(typeof(Int32), "");

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), TestValue);

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(5)));

            SwitchCase Case1 = EU.SwitchCase(5, EU.BlockVoid(EU.ConcatEquals(Result, "C5_")));
            SwitchExpression MySw = Expr.Switch(Test, Case1);

            Expressions.Add(MySw);

            Expressions.Add(EU.GenAreEqual(Expr.Constant("TestC5_"), Result, "Switch 1"));

            var tree = EU.BlockVoid(new[] { Result, TestValue }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Negative, positive values for value in SwitchCase
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 23", new string[] { "positive", "switch", "controlflow", "Pri2" })]
        public static Expr Switch23(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression TestValue = Expr.Variable(typeof(Int32), "");

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), TestValue);

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(-1)));

            SwitchCase Case1 = EU.SwitchCase(-1, EU.ConcatEquals(Result, "C-1_"));
            SwitchCase Case2 = EU.SwitchCase(3, EU.ConcatEquals(Result, "C3_"));
            SwitchExpression MySw = Expr.Switch(Test, Case1, Case2);

            Expressions.Add(MySw);

            Expressions.Add(EU.GenAreEqual(Expr.Constant("TestC-1_"), Result, "Switch 1"));

            var tree = EU.BlockVoid(new[] { Result, TestValue }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Expression that returns a value for body in SwitchCase
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 24", new string[] { "positive", "switch", "controlflow", "Pri2" })]
        public static Expr Switch24(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression TestValue = Expr.Variable(typeof(Int32), "");

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), TestValue);

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(5)));

            SwitchCase Case1 = EU.SwitchCase(5, Expr.Block(EU.ConcatEquals(Result, "C5_"), Expr.Add(Expr.Constant(2), Expr.Constant(3))));
            SwitchCase Case2 = EU.SwitchCase(3, EU.ConcatEquals(Result, "C3_"));
            SwitchExpression MySw = Expr.Switch(Test, Case1, Case2);

            Expressions.Add(MySw);

            Expressions.Add(EU.GenAreEqual(Expr.Constant("TestC5_"), Result, "Switch 1"));

            var tree = EU.BlockVoid(new[] { Result, TestValue }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass a scope wrapped expression to value in SwitchCase
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 25", new string[] { "positive", "switch", "controlflow", "Pri2" })]
        public static Expr Switch25(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression TestValue = Expr.Variable(typeof(Int32), "");

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), TestValue);

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(3)));

            SwitchCase Case1 = EU.SwitchCase(5, EU.ConcatEquals(Result, "C5_"));
            SwitchCase Case2 = EU.SwitchCase(3, Expr.Block(EU.ConcatEquals(Result, "C3_")));
            SwitchExpression MySw = Expr.Switch(Test, Case1, Case2);

            Expressions.Add(MySw);

            Expressions.Add(EU.GenAreEqual(Expr.Constant("TestC3_"), Result, "Switch 1"));

            var tree = EU.BlockVoid(new[] { Result, TestValue }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Expression that returns a value for body of DefaultCase
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 26", new string[] { "positive", "switch", "controlflow", "Pri2" })]
        public static Expr Switch26(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression TestValue = Expr.Variable(typeof(Int32), "");

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), TestValue);

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(3)));

            SwitchCase Case1 = EU.SwitchCase(5, EU.ConcatEquals(Result, "C5_"));
            Expr Default = EU.BlockVoid(EU.ConcatEquals(Result, "Default"), Expr.Add(Expr.Constant(2), Expr.Constant(3)));
            SwitchExpression MySw = Expr.Switch(Test, Default, Case1);

            Expressions.Add(MySw);

            Expressions.Add(EU.GenAreEqual(Expr.Constant("TestDefault"), Result, "Switch 1"));

            var tree = EU.BlockVoid(new[] { Result, TestValue }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass a scope wrapped expression to body of DefaultCase
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 27", new string[] { "positive", "switch", "controlflow", "Pri2" })]
        public static Expr Switch27(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression TestValue = Expr.Variable(typeof(Int32), "");

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), TestValue);

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(3)));

            SwitchCase Case1 = EU.SwitchCase(5, EU.ConcatEquals(Result, "C5_"));
            Expr Default = EU.BlockVoid(EU.ConcatEquals(Result, "Default"));
            SwitchExpression MySw = Expr.Switch(Test, Default, Case1);

            Expressions.Add(MySw);

            Expressions.Add(EU.GenAreEqual(Expr.Constant("TestDefault"), Result, "Switch 1"));

            var tree = EU.BlockVoid(new[] { Result, TestValue }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass a scope wrapped expression to body of Switch
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 28", new string[] { "positive", "switch", "controlflow", "Pri2" })]
        public static Expr Switch28(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression TestValue = Expr.Variable(typeof(Int32), "");

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), TestValue);

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(5)));

            SwitchCase Case1 = EU.SwitchCase(5, EU.ConcatEquals(Result, "C5_"));
            Expr Default = AstUtils.Void(EU.ConcatEquals(Result, "Default"));
            SwitchExpression MySw = Expr.Switch(Expr.Block(Test), Default, (MethodInfo)null, Case1);

            Expressions.Add(MySw);

            Expressions.Add(EU.GenAreEqual(Expr.Constant("TestC5_"), Result, "Switch 1"));

            var tree = EU.BlockVoid(new[] { Result, TestValue }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass a null argument to test
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 29", new string[] { "negative", "switch", "controlflow", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr Switch29(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression TestValue = Expr.Variable(typeof(Int32), "");

            Expr Test = null;

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(5)));

            SwitchCase Case1 = EU.SwitchCase(5, EU.BlockVoid(EU.ConcatEquals(Result, "C5_")));
            var MySw = EU.Throws<System.ArgumentNullException>(() =>
            {
                Expr.Switch(Test, Case1);
            });
            Expressions.Add(MySw);

            return Expr.Empty();
        }

        // Pass a scoped wrap expression to test
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 30", new string[] { "positive", "switch", "controlflow", "Pri2" })]
        public static Expr Switch30(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression TestValue = Expr.Variable(typeof(Int32), "");
            ParameterExpression ScopedValue = Expr.Variable(typeof(string), "");

            BlockExpression Test = Expr.Block(new ParameterExpression[] { ScopedValue }, EU.ConcatEquals(Result, "Test"), EU.ConcatEquals(Result, ScopedValue), TestValue);

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(5)));

            SwitchCase Case1 = EU.SwitchCase(5, EU.BlockVoid(EU.ConcatEquals(Result, "C5_")));
            SwitchExpression MySw = Expr.Switch(Test, Case1);

            Expressions.Add(MySw);

            Expressions.Add(EU.GenAreEqual(Expr.Constant("TestC5_"), Result, "Switch 1"));

            var tree = EU.BlockVoid(new[] { Result, TestValue }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Nest switch statements with gotos
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 31", new string[] { "positive", "switch", "controlflow", "Pri2" })]
        public static Expr Switch31(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

            ParameterExpression NestedTestValue = Expr.Variable(typeof(Int32), "");
            ParameterExpression TestValue = Expr.Variable(typeof(Int32), "");

            Expr NestedTest = Expr.Block(EU.ConcatEquals(Result, "NestedTestValue_"), NestedTestValue);
            Expr Test = Expr.Block(EU.ConcatEquals(Result, "TestValue_"), TestValue);

            LabelTarget NestedEnd = Expr.Label("NestedEnd");
            LabelTarget ItsTheEnd = Expr.Label("ItsTheEnd");

            Expressions.Add(Expr.Assign(NestedTestValue, Expr.Constant(2)));
            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(5)));

            // case 1 - go into nested switch back through outer switch
            SwitchCase NestedCase1 = EU.SwitchCase(5, EU.BlockVoid(EU.ConcatEquals(Result, "NestedC5_")));
            SwitchCase NestedCase2 = EU.SwitchCase(2, EU.BlockVoid(EU.ConcatEquals(Result, "NestedC2_")));
            Expr NestedDefault = AstUtils.Void(EU.ConcatEquals(Result, "NestedDefault_"));
            Expr NestedSwitch = Expr.Block(Expr.Switch(NestedTest, NestedDefault, NestedCase1, NestedCase2));

            SwitchCase Case1 = EU.SwitchCase(5, EU.BlockVoid(EU.ConcatEquals(Result, "C5_"), NestedSwitch));
            Expr Default = AstUtils.Void(EU.ConcatEquals(Result, Expr.Constant("Default_")));
            Expr MySw = Expr.Block(Expr.Switch(Test, Default, Case1));

            Expressions.Add(MySw);

            Expressions.Add(EU.GenAreEqual(Expr.Constant("TestValue_C5_NestedTestValue_NestedC2_"), Result, "Switch 1"));

            // case 2 - break in nested switch back to outer switch
            Expressions.Add(Expr.Assign(Result, Expr.Constant("")));
            Expressions.Add(Expr.Assign(NestedTestValue, Expr.Constant(5)));
            NestedCase1 = EU.SwitchCase(5, EU.BlockVoid(EU.ConcatEquals(Result, "NestedC5_"), Expr.Break(NestedEnd)));
            NestedSwitch = Expr.Block(Expr.Switch(NestedTest, NestedDefault, NestedCase1, NestedCase2), Expr.Label(NestedEnd));

            Case1 = EU.SwitchCase(5, EU.BlockVoid(EU.ConcatEquals(Result, "C5_"), NestedSwitch));
            MySw = Expr.Block(Expr.Switch(Test, Default, Case1), Expr.Label(ItsTheEnd));

            Expressions.Add(MySw);

            Expressions.Add(EU.GenAreEqual(Expr.Constant("TestValue_C5_NestedTestValue_NestedC5_"), Result, "Switch 2"));

            // case 3 - break in nested switch all the way out of outer switch
            Expressions.Add(Expr.Assign(Result, Expr.Constant("")));
            Expressions.Add(Expr.Assign(NestedTestValue, Expr.Constant(5)));
            NestedCase1 = EU.SwitchCase(5, EU.BlockVoid(EU.ConcatEquals(Result, "NestedC5_"), Expr.Break(ItsTheEnd)));
            NestedSwitch = Expr.Block(Expr.Switch(NestedTest, NestedDefault, NestedCase1, NestedCase2), Expr.Label(NestedEnd));

            Case1 = EU.SwitchCase(5, EU.BlockVoid(EU.ConcatEquals(Result, "C5_"), NestedSwitch, EU.ConcatEquals(Result, "EndC5_")));
            MySw = Expr.Block(Expr.Switch(Test, Default, Case1), Expr.Label(ItsTheEnd));

            Expressions.Add(MySw);

            Expressions.Add(EU.GenAreEqual(Expr.Constant("TestValue_C5_NestedTestValue_NestedC5_"), Result, "Switch 2"));

            // case 4 - break in nested switch back to outer switch, break in outer switch
            Expressions.Add(Expr.Assign(Result, Expr.Constant("")));
            Expressions.Add(Expr.Assign(NestedTestValue, Expr.Constant(5)));
            NestedCase1 = EU.SwitchCase(5, EU.BlockVoid(EU.ConcatEquals(Result, "NestedC5_"), Expr.Break(NestedEnd)));
            NestedSwitch = Expr.Block(Expr.Switch(NestedTest, NestedDefault, NestedCase1, NestedCase2), Expr.Label(NestedEnd));

            Case1 = EU.SwitchCase(5, EU.BlockVoid(EU.ConcatEquals(Result, "C5_"), NestedSwitch, EU.ConcatEquals(Result, "EndC5_"), Expr.Break(ItsTheEnd)));
            MySw = Expr.Block(Expr.Switch(Test, Default, Case1), Expr.Label(ItsTheEnd));

            Expressions.Add(MySw);

            Expressions.Add(EU.GenAreEqual(Expr.Constant("TestValue_C5_NestedTestValue_NestedC5_EndC5_"), Result, "Switch 3"));

            var tree = EU.BlockVoid(new[] { Result, NestedTestValue, TestValue }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // break to a label not positioned at the end of the switch statement
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 32", new string[] { "positive", "switch", "controlflow", "Pri2" })]
        public static Expr Switch32(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression TestValue = Expr.Variable(typeof(Int32), "");

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), TestValue);

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(5)));

            LabelTarget ItsTheEnd = Expr.Label();

            SwitchCase Case1 = EU.SwitchCase(5, EU.BlockVoid(EU.ConcatEquals(Result, "C5_"), Expr.Break(ItsTheEnd)));
            SwitchCase Case2 = EU.SwitchCase(2, EU.ConcatEquals(Result, "C2_"));

            Expr MySw = Expr.Label(ItsTheEnd, Expr.Switch(Test, Case1, Case2));

            Expressions.Add(MySw);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("TestC5_"), Result, "Switch 1"));

            var tree = EU.BlockVoid(new[] { Result, TestValue }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Define scopes for each block of a switch
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 33", new string[] { "positive", "switch", "controlflow", "Pri2" })]
        public static Expr Switch33(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression TestValue = Expr.Variable(typeof(Int32), "");

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), TestValue);

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(5)));

            LabelTarget ItsTheEnd = Expr.Label();

            ParameterExpression c1 = Expr.Variable(typeof(Int32), "");
            ParameterExpression c2 = Expr.Variable(typeof(Int32), "");

            SwitchCase Case1 = EU.SwitchCase(5,
                Expr.Block(
                    new[] { c1 },
                    EU.ConcatEquals(Result, "C5_"),
                    Expr.Assign(c1, Expr.Constant(1)),
                    Expr.Assign(TestValue, Expr.Add(TestValue, c1)),
                    EU.GenAreEqual(TestValue, Expr.Constant(6)),
                    TestValue
                ));
            SwitchCase Case2 = EU.SwitchCase(6,
                Expr.Block(
                    new[] { c2 },
                    EU.ConcatEquals(Result, "C6_"),
                    Expr.Assign(c2, Expr.Constant(3)),
                    Expr.Assign(TestValue, Expr.Add(TestValue, c2)),
                    EU.GenAreEqual(TestValue, Expr.Constant(9)),
                    TestValue
                ));
            SwitchCase Case3 = EU.SwitchCase(8, EU.BlockVoid(EU.ConcatEquals(Result, "C8_"), Expr.Break(ItsTheEnd)));
            Expr Default = AstUtils.Void(EU.ConcatEquals(Result, "Default"));

            var MySw = Expr.Label(ItsTheEnd, Expr.Switch(Test, Default, Case1, Case2, Case3));

            Expressions.Add(MySw);

            Expressions.Add(EU.GenAreEqual(Expr.Constant("TestC5_"), Result, "Switch 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(6), TestValue));

            var tree = EU.BlockVoid(new[] { Result, TestValue }, Expressions);
            V.Validate(tree);
            return tree;

        }

        // Pass a scope to the test
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 34", new string[] { "positive", "switch", "controlflow", "Pri2" })]
        public static Expr Switch34(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression TestValue = Expr.Variable(typeof(Int32), "");
            ParameterExpression ScopedVal = Expr.Variable(typeof(Int32), "");

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(5)));

            Expr Test =
                Expr.Block(
                    new[] { ScopedVal },
                    EU.ConcatEquals(Result, "Test"),
                    Expr.Assign(ScopedVal, Expr.Constant(3)),
                    Expr.Assign(TestValue, Expr.Add(TestValue, ScopedVal)),
                    TestValue
                );

            LabelTarget ItsTheEnd = Expr.Label();

            SwitchCase Case1 = EU.SwitchCase(5, EU.BlockVoid(EU.ConcatEquals(Result, "C5_"), Expr.Break(ItsTheEnd)));
            SwitchCase Case2 = EU.SwitchCase(8, EU.ConcatEquals(Result, "C8_"));

            var MySw = Expr.Label(ItsTheEnd, Expr.Switch(Test, Case1, Case2));

            Expressions.Add(MySw);

            Expressions.Add(EU.GenAreEqual(Expr.Constant("TestC8_"), Result, "Switch 1"));

            var tree = EU.BlockVoid(new[] { Result, TestValue }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Continue to a random external label

        // Continue to a label inside a parallel switch

        // Switch 37 removed (was: add multiple defaults, fall through the switch statement)

        // pass a type that widens to int to the test
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 38", new string[] { "negative", "switch", "controlflow", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr Switch38(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression TestValue = Expr.Variable(typeof(SByte), "");

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant((SByte)5, typeof(SByte))));

            Expr Test = Expr.Block(EU.ConcatEquals(Result, "Test"), TestValue);

            SwitchCase Case1 = EU.SwitchCase(5, EU.BlockVoid(EU.ConcatEquals(Result, "C5_")));
            Expression Default1 = EU.ConcatEquals(Result, "Default1_");

            var MySw = EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.Switch(Test, Default1, Case1);
            });

            Expressions.Add(MySw);

            return Expr.Empty();
        }

        // void case bodies
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 39", new string[] { "positive", "switch", "controlfflow", "Pri1" }, Priority = 1)]
        public static Expr Switch39(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression TestValue = Expr.Variable(typeof(char), "TestValue");

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant('C')));

            SwitchExpression MySw =
                Expr.Switch(
                    TestValue,
                    Expr.Block(EU.ConcatEquals(Result, Expr.Constant("Default")), Expr.Empty()),
                    new SwitchCase[] {
                        Expr.SwitchCase(
                            Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C1_")),
                                Expr.Empty()
                            ),
                            Expr.Constant('A')
                        ),
                        Expr.SwitchCase(
                            Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C2_")),
                                Expr.Empty()
                            ),
                            Expr.Constant('B')
                        ),
                        Expr.SwitchCase(
                            Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C3_")),
                                Expr.Empty()
                            ),
                            Expr.Constant('C')
                        )
                    }
                );

            Expressions.Add(MySw);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("C3_"), Result, "Switch 1"));

            var tree = Expr.Block(new[] { Result, TestValue }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // non-void case bodies (all of the same type)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 40", new string[] { "positive", "switch", "controlfflow", "Pri1" }, Priority = 1)]
        public static Expr Switch40(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression TestValue = Expr.Variable(typeof(int), "TestValue");

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(2)));

            SwitchExpression MySw =
                Expr.Switch(
                    TestValue,
                    Expr.Block(EU.ConcatEquals(Result, Expr.Constant("Default")), Expr.Constant(-1)),
                    new SwitchCase[] {
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C1_")),
                                Expr.Constant(10)
                            ),
                            Expr.Constant(1)
                        ),
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C2_")),
                                Expr.Constant(20)
                            ),
                            Expr.Constant(2)
                        ),
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C3_")),
                                Expr.Constant(30)
                            ),
                            Expr.Constant(3)
                        )
                    }
                );

            Expressions.Add(EU.GenAreEqual(Expr.Constant(20), MySw, "Switch 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("C2_"), Result, "Switch 2"));

            var tree = Expr.Block(new[] { Result, TestValue }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // non-void case bodies (mixed types)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 41", new string[] { "negative", "switch", "controlfflow", "Pri1" }, Exception = typeof(ArgumentException), Priority = 1)]
        public static Expr Switch41(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression TestValue = Expr.Variable(typeof(int), "TestValue");

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(4)));

            var MySw =
                EU.Throws<System.ArgumentException>(() =>
                {
                    Expr.Switch(
                        TestValue,
                        Expr.Block(EU.ConcatEquals(Result, Expr.Constant("Default")), Expr.Constant(-1)),
                        new SwitchCase[] {
	                        Expr.SwitchCase(Expr.Block(
	                                EU.ConcatEquals(Result, Expr.Constant("C1_")),
	                                Expr.Constant(10)
	                            ),
	                            Expr.Constant(1)
	                        ),
	                        Expr.SwitchCase(Expr.Block(
	                                EU.ConcatEquals(Result, Expr.Constant("C2_")),
	                                Expr.Constant("20")
	                            ),
	                            Expr.Constant(2)
	                        ),
	                        Expr.SwitchCase(Expr.Block(
	                                EU.ConcatEquals(Result, Expr.Constant("C3_")),
	                                Expr.Constant(30)
	                            ),
	                            Expr.Constant(3)
	                        )
	                    }
                    );
                });

            return Expr.Empty();
        }

        // void default, non-void cases
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 42", new string[] { "negative", "switch", "controlfflow", "Pri2" }, Exception = typeof(ArgumentException), Priority = 2)]
        public static Expr Switch42(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression TestValue = Expr.Variable(typeof(int), "TestValue");

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(4)));

            var MySw =
                EU.Throws<System.ArgumentException>(() =>
                {
                    Expr.Switch(
                        TestValue,
                        Expr.Block(EU.ConcatEquals(Result, Expr.Constant("Default"))),
                        new SwitchCase[] {
	                        Expr.SwitchCase(Expr.Block(
	                                EU.ConcatEquals(Result, Expr.Constant("C1_")),
	                                Expr.Constant(10)
	                            ),
	                            Expr.Constant(1)
	                        ),
	                        Expr.SwitchCase(Expr.Block(
	                                EU.ConcatEquals(Result, Expr.Constant("C2_")),
	                                Expr.Constant(20)
	                            ),
	                            Expr.Constant(2)
	                        ),
	                        Expr.SwitchCase(Expr.Block(
	                                EU.ConcatEquals(Result, Expr.Constant("C3_")),
	                                Expr.Constant(30)
	                            ),
	                            Expr.Constant(3)
	                        )
	                    }
                    );
                });

            Expressions.Add(EU.GenAreEqual(Expr.Constant(20), MySw, "Switch 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("C2_"), Result, "Switch 2"));

            return Expr.Empty();
        }

        // non-void default, void cases
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 43", new string[] { "negative", "switch", "controlfflow", "Pri2" }, Exception = typeof(ArgumentException), Priority = 2)]
        public static Expr Switch43(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression TestValue = Expr.Variable(typeof(int), "TestValue");

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(4)));

            var MySw =
                EU.Throws<System.ArgumentException>(() =>
                {
                    Expr.Switch(
                        TestValue,
                        Expr.Block(EU.ConcatEquals(Result, Expr.Constant("Default")), Expr.Constant(-1)),
                        new SwitchCase[] {
	                        Expr.SwitchCase(Expr.Block(
	                                EU.ConcatEquals(Result, Expr.Constant("C1_"))
	                            ),
	                            Expr.Constant(1)
	                        ),
	                        Expr.SwitchCase(Expr.Block(
	                                EU.ConcatEquals(Result, Expr.Constant("C2_"))
	                            ),
	                            Expr.Constant(2)
	                        ),
	                        Expr.SwitchCase(Expr.Block(
	                                EU.ConcatEquals(Result, Expr.Constant("C3_"))
	                            ),
	                            Expr.Constant(3)
	                        )
	                    }
                    );
                });

            Expressions.Add(EU.GenAreEqual(Expr.Constant(20), MySw, "Switch 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("C2_"), Result, "Switch 2"));

            return Expr.Empty();
        }

        // non-constant test values
        // Try/catch in test
        // Try/catch that throws in test
        // Switch in test value
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 44", new string[] { "positive", "switch", "controlfflow", "Pri1" }, Priority = 1)]
        public static Expr Switch44(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression TestValue = Expr.Variable(typeof(int), "TestValue");
            ParameterExpression Temp = Expr.Variable(typeof(int), "Temp");

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(3)));
            Expressions.Add(Expr.Assign(Temp, Expr.Constant(1)));

            SwitchExpression MySw =
                Expr.Switch(
                    TestValue,
                    Expr.Block(EU.ConcatEquals(Result, Expr.Constant("Default")), Expr.Constant(-1)),
                    new SwitchCase[] {
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C1_")),
                                Expr.Constant(10)
                            ),
                            Expr.Block(
                                Expr.Assign(Temp, Expr.Add(Temp, Expr.Constant(3))),
                                Temp
                            )
                        ),
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C2_")),
                                Expr.Constant(20)
                            ),
                            Expr.TryCatch(
                                Expr.Block(
                                    Expr.Assign(Temp, Expr.Add(Temp, Expr.Constant(1))),
                                    Expr.Throw(Expr.Constant(new DivideByZeroException())),
                                    Expr.Constant(1)
                                ),
                                Expr.Catch(typeof(DivideByZeroException), Expr.Constant(2))
                            )
                        ),
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C3_")),
                                Expr.Constant(30)
                            ),
                            Expr.Condition(
                                Expr.Equal(Temp, Expr.Constant(5)),
                                Expr.Constant(3),
                                Expr.Constant(2)
                            )
                        ),
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C4_")),
                                Expr.Constant(40)
                            ),
                            Expr.Switch(
                                Temp,
                                Expr.Block(EU.ConcatEquals(Result, Expr.Constant("NestedDefault")), Expr.Constant(-11)),
                                Expr.SwitchCase(Expr.Block(
                                    EU.ConcatEquals(Result, Expr.Constant("Nested")), 
                                    Expr.Constant(5)
                                    ),
                                    Expr.Constant(5)
                                )
                            )
                        )
                    } // end outer switch
                );

            Expressions.Add(EU.GenAreEqual(Expr.Constant(30), MySw, "Switch 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("C3_"), Result, "Switch 2"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(5), Temp, "Switch 3"));

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(5)));
            Expressions.Add(Expr.Assign(Temp, Expr.Constant(1)));
            Expressions.Add(Expr.Assign(Result, Expr.Constant("")));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(40), MySw, "Switch 4"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("NestedC4_"), Result, "Switch 5"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(5), Temp, "Switch 6"));

            var tree = Expr.Block(new[] { Result, TestValue, Temp }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Try/catch in test with filter
        [ETUtils.TestAttribute(ETUtils.TestState.Disabled, "Switch 44_1", new string[] { "positive", "switch", "controlfflow", "Pri1" }, Priority = 1)]
        public static Expr Switch44_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression TestValue = Expr.Variable(typeof(int), "TestValue");

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(2)));

            if (System.Environment.Version.Major >= 4) {
                SwitchExpression MySw =
                  Expr.Switch(
                     TestValue,
                     Expr.Block(EU.ConcatEquals(Result, Expr.Constant("Default")), Expr.Constant(-1)),
                     new SwitchCase[] {
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C1_")),
                                Expr.Constant(10)
                            ),
                            Expr.Constant(1)
                        ),
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C2_")),
                                Expr.Constant(20)
                            ),
                            Expr.TryCatch(
                                Expr.Block(
                                    Expr.Throw(Expr.Constant(new DivideByZeroException())),
                                    Expr.Constant(1)
                                ),
                                Expr.Catch(typeof(DivideByZeroException), Expr.Constant(2), Expr.Constant(true))
                            )
                        ),
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C3_")),
                                Expr.Constant(30)
                            ),
                            Expr.Constant(3)
                        )
                    }
                 );

                Expressions.Add(EU.GenAreEqual(Expr.Constant(20), MySw, "Switch 1"));
                Expressions.Add(EU.GenAreEqual(Expr.Constant("C2_"), Result, "Switch 2"));
            }

            var tree = Expr.Block(new[] { Result, TestValue }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Multiple test values on multiple cases
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 45", new string[] { "positive", "switch", "controlfflow", "Pri1" }, Priority = 1)]
        public static Expr Switch45(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression TestValue = Expr.Variable(typeof(int), "TestValue");

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(6)));

            SwitchExpression MySw =
                Expr.Switch(
                    TestValue,
                    Expr.Block(EU.ConcatEquals(Result, Expr.Constant("Default")), Expr.Constant(-1)),
                    new SwitchCase[] {
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C1_")),
                                Expr.Constant(10)
                            ),
                            Expr.Constant(1), Expr.Constant(2), Expr.Constant(1000)
                        ),
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C2_")),
                                Expr.Constant(20)
                            ),
                            Expr.Constant(3), Expr.Constant(10000000)
                        ),
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C3_")),
                                Expr.Constant(30)
                            ),
                            Expr.Constant(5), Expr.Constant(6)
                        )
                    }
                );

            Expressions.Add(EU.GenAreEqual(Expr.Constant(30), MySw, "Switch 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("C3_"), Result, "Switch 2"));

            var tree = Expr.Block(new[] { Result, TestValue }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Goto other case body (simulating fallthrough)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 46", new string[] { "negative", "switch", "controlfflow", "Pri3" }, Exception = typeof(InvalidOperationException), Priority = 3)]
        public static Expr Switch46(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression TestValue = Expr.Variable(typeof(int), "TestValue");

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(2)));

            LabelTarget target = Expr.Label(typeof(int), "target");

            SwitchExpression MySw =
                Expr.Switch(
                    TestValue,
                    Expr.Block(EU.ConcatEquals(Result, Expr.Constant("Default")), Expr.Constant(-1)),
                    new SwitchCase[] {
                        Expr.SwitchCase(Expr.Block(
                                Expr.Label(target, Expr.Constant(0)),
                                EU.ConcatEquals(Result, Expr.Constant("C1_")),
                                Expr.Constant(10)
                            ),
                            Expr.Constant(1)
                        ),
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C2_")),
                                Expr.Goto(target, Expr.Constant(-10)),
                                Expr.Constant(20)
                            ),
                            Expr.Constant(2)
                        ),
                        Expr.SwitchCase(Expr.Block(
                                Expr.Label(target, Expr.Constant(0)),
                                EU.ConcatEquals(Result, Expr.Constant("C3_")),
                                Expr.Goto(target, Expr.Constant(-20)),
                                Expr.Constant(30)
                            ),
                            Expr.Constant(3)
                        )
                    }
                );

            Expressions.Add(EU.GenAreEqual(Expr.Constant(10), MySw, "Switch 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("C2_C3_C1_"), Result, "Switch 2"));

            var tree = Expr.Block(new[] { Result, TestValue }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true);
            return tree;
        }

        // Goto other case body (simulating fallthrough)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 47", new string[] { "positive", "switch", "controlfflow", "Pri1" }, Priority = 1)]
        public static Expr Switch47(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression TestValue = Expr.Variable(typeof(int), "TestValue");

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(2)));

            LabelTarget target = Expr.Label(typeof(int), "target");
            LabelTarget target2 = Expr.Label(typeof(int), "target2");

            SwitchExpression MySw =
                Expr.Switch(
                    TestValue,
                    Expr.Block(EU.ConcatEquals(Result, Expr.Constant("Default")), Expr.Constant(-1)),
                    new SwitchCase[] {
                        Expr.SwitchCase(Expr.Block(
                                Expr.Label(target2, Expr.Constant(0)),
                                EU.ConcatEquals(Result, Expr.Constant("C1_")),
                                Expr.Constant(10)
                            ),
                            Expr.Constant(1)
                        ),
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C2_")),
                                Expr.Goto(target, Expr.Constant(-10)),
                                Expr.Constant(20)
                            ),
                            Expr.Constant(2)
                        ),
                        Expr.SwitchCase(Expr.Block(
                                Expr.Label(target, Expr.Constant(0)),
                                EU.ConcatEquals(Result, Expr.Constant("C3_")),
                                Expr.Goto(target2, Expr.Constant(-20)),
                                Expr.Constant(30)
                            ),
                            Expr.Constant(3)
                        )
                    }
                );

            Expressions.Add(EU.GenAreEqual(Expr.Constant(10), MySw, "Switch 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("C2_C3_C1_"), Result, "Switch 2"));

            var tree = Expr.Block(new[] { Result, TestValue }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Multiple test values with mismatched types
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 48", new string[] { "negative", "switch", "controlfflow", "Pri2" }, Exception = typeof(ArgumentException), Priority = 2)]
        public static Expr Switch48(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression TestValue = Expr.Variable(typeof(int), "TestValue");

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(6)));

            var MySw =
                EU.Throws<System.ArgumentException>(() =>
                {
                    Expr.Switch(
                        TestValue,
                        Expr.Block(EU.ConcatEquals(Result, Expr.Constant("Default")), Expr.Constant(-1)),
                        new SwitchCase[] {
	                        Expr.SwitchCase(Expr.Block(
	                                EU.ConcatEquals(Result, Expr.Constant("C1_")),
	                                Expr.Constant(10)
	                            ),
	                            Expr.Constant(1), Expr.Constant(2), Expr.Constant(1000)
	                        ),
	                        Expr.SwitchCase(Expr.Block(
	                                EU.ConcatEquals(Result, Expr.Constant("C2_")),
	                                Expr.Constant(20)
	                            ),
	                            Expr.Constant(3), Expr.Constant(10000000.00)
	                        ),
	                        Expr.SwitchCase(Expr.Block(
	                                EU.ConcatEquals(Result, Expr.Constant("C3_")),
	                                Expr.Constant(30)
	                            ),
	                            Expr.Constant(5), Expr.Constant(6)
	                        )
	                    }
                    );
                });

            Expressions.Add(EU.GenAreEqual(Expr.Constant(30), MySw, "Switch 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("C3_"), Result, "Switch 2"));

            return Expr.Block(new[] { Result, TestValue }, Expressions);
        }

        // non-void case bodies (all of the same type)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 49", new string[] { "positive", "switch", "controlfflow", "Pri1" }, Priority = 1)]
        public static Expr Switch49(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression TestValue = Expr.Variable(typeof(int), "TestValue");

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(2)));

            SwitchExpression MySw =
                Expr.Switch(
                    TestValue,
                    Expr.Block(EU.ConcatEquals(Result, Expr.Constant("Default")), Expr.Constant(-1)),
                    new SwitchCase[] {
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C1_")),
                                Expr.Constant(10)
                            ),
                            Expr.Constant(1)
                        ),
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C2_")),
                                Expr.Constant(20)
                            ),
                            Expr.Constant(2)
                        ),
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C3_")),
                                Expr.Constant(30)
                            ),
                            Expr.Constant(3)
                        )
                    }
                );

            Expressions.Add(EU.GenAreEqual(Expr.Constant(20), MySw, "Switch 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("C2_"), Result, "Switch 2"));

            var tree = Expr.Block(new[] { Result, TestValue }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Multiple cases with the same test value
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 50", new string[] { "positive", "switch", "controlfflow", "Pri2" }, Priority = 2)]
        public static Expr Switch50(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression TestValue = Expr.Variable(typeof(int), "TestValue");

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(1)));

            SwitchExpression MySw =
                Expr.Switch(
                    TestValue,
                    Expr.Block(EU.ConcatEquals(Result, Expr.Constant("Default")), Expr.Constant(-1)),
                    new SwitchCase[] {
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C1_")),
                                Expr.Constant(10)
                            ),
                            Expr.Constant(1), Expr.Constant(1), Expr.Constant(1000)
                        ),
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C2_")),
                                Expr.Constant(20)
                            ),
                            Expr.Constant(1), Expr.Constant(5)
                        ),
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C3_")),
                                Expr.Constant(30)
                            ),
                            Expr.Constant(5), Expr.Constant(1)
                        )
                    }
                );

            Expressions.Add(EU.GenAreEqual(Expr.Constant(10), MySw, "Switch 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("C1_"), Result, "Switch 2"));

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(5)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(20), MySw, "Switch 3"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("C1_C2_"), Result, "Switch 4"));

            var tree = Expr.Block(new[] { Result, TestValue }, Expressions);
            V.Validate(tree);
            return tree;
        }

        public class TestClass {
            public int Data { get; set; }
            public TestClass(int x) { Data = x; }
        }

        // Test values of user defined class with no comparison operators defined
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 51", new string[] { "positive", "switch", "controlfflow", "Pri2" }, Priority = 2)]
        public static Expr Switch51(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression TestValue = Expr.Variable(typeof(TestClass), "TestValue");

            Expr Val = Expr.Constant(new TestClass(1)); // no user defined comparison means switch uses reference equality
            Expressions.Add(Expr.Assign(TestValue, Val));

            ParameterExpression Case1Value = Expr.Variable(typeof(TestClass), "Case1Value");
            Expressions.Add(Expr.Assign(Case1Value, Expr.Constant(new TestClass(1))));

            ParameterExpression Case2Value = Expr.Variable(typeof(TestClass), "Case2Value");
            Expressions.Add(Expr.Assign(Case2Value, Val));

            SwitchExpression MySw =
                Expr.Switch(
                    TestValue,
                    Expr.Block(EU.ConcatEquals(Result, Expr.Constant("Default")), Expr.Constant(-1)),
                    new SwitchCase[] {
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C1_")),
                                Expr.Constant(10)
                            ),
                            Case1Value
                        ),
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C2_")),
                                Expr.Constant(20)
                            ),
                            Case2Value
                        )
                    }
                );

            Expressions.Add(EU.GenAreEqual(Expr.Constant(20), MySw, "Switch 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("C2_"), Result, "Switch 2"));

            var tree = Expr.Block(new[] { Result, TestValue, Case1Value, Case2Value }, Expressions);
            V.Validate(tree);
            return tree;
        }

        public class TestClass2 {
            public int Data { get; set; }
            public TestClass2(int x) { Data = x; }
            public static bool operator ==(TestClass2 x, TestClass2 y) {
                return x.Data == y.Data;
            }
            public static bool operator !=(TestClass2 x, TestClass2 y) {
                return x.Data != y.Data;
            }
            public override bool Equals(object obj) {
                return base.Equals(obj);
            }
            public override int GetHashCode() {
                return 1;
            }

        }

        // Test values of user defined class with comparison operators defined
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 52", new string[] { "positive", "switch", "controlfflow", "Pri2" }, Priority = 2)]
        public static Expr Switch52(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression TestValue = Expr.Variable(typeof(TestClass2), "TestValue");

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(new TestClass2(2))));

            ParameterExpression Case1Value = Expr.Variable(typeof(TestClass2), "Case1Value");
            Expressions.Add(Expr.Assign(Case1Value, Expr.Constant(new TestClass2(1))));

            ParameterExpression Case2Value = Expr.Variable(typeof(TestClass2), "Case2Value");
            Expressions.Add(Expr.Assign(Case2Value, Expr.Constant(new TestClass2(2))));

            SwitchExpression MySw =
                Expr.Switch(
                    TestValue,
                    Expr.Block(EU.ConcatEquals(Result, Expr.Constant("Default")), Expr.Constant(-1)),
                    new SwitchCase[] {
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C1_")),
                                Expr.Constant(10)
                            ),
                            Case1Value
                        ),
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C2_")),
                                Expr.Constant(20)
                            ),
                            Case2Value
                        )
                    }
                );

            Expressions.Add(EU.GenAreEqual(Expr.Constant(20), MySw, "Switch 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("C2_"), Result, "Switch 2"));

            var tree = Expr.Block(new[] { Result, TestValue, Case1Value, Case2Value }, Expressions);
            V.Validate(tree);
            return tree;
        }

        public static bool LambdaComparator(Func<string> a, Func<string> b) {
            return true;
        }

        // Execute lambda in test value
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 53", new string[] { "positive", "switch", "controlfflow", "Pri1" }, Priority = 1)]
        public static Expr Switch53(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression TestValue = Expr.Variable(typeof(DateTime), "TestValue");

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(DateTime.MaxValue)));

            LambdaExpression TestLM = Expr.Lambda(Expr.Condition(Expr.Constant(true), Expr.Constant(DateTime.MaxValue), Expr.Constant(DateTime.MinValue)));

            SwitchExpression MySw =
                Expr.Switch(
                    Expr.Constant((DateTime)TestLM.Compile().DynamicInvoke()),
                    Expr.Block(EU.ConcatEquals(Result, Expr.Constant("Default")), Expr.Constant(-1)),
                    new SwitchCase[] {
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C1_")),
                                Expr.Constant(10)
                            ),
                            Expr.Constant(DateTime.Now)
                        ),
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C2_")),
                                Expr.Constant(20)
                            ),
                            Expr.Constant(DateTime.MinValue)
                        ),
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C3_")),
                                Expr.Constant(30)
                            ),
                            Expr.Constant((DateTime)TestLM.Compile().DynamicInvoke())
                        )
                    }
                );

            Expressions.Add(EU.GenAreEqual(Expr.Constant(30), MySw, "Switch 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("C3_"), Result, "Switch 2"));

            var tree = Expr.Block(new[] { Result, TestValue }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Execute lambda in switch case
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 53_1", new string[] { "positive", "switch", "controlfflow", "Pri1" }, Priority = 1)]
        public static Expr Switch53_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression TestValue = Expr.Variable(typeof(DateTime), "TestValue");

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(DateTime.MaxValue)));

            LambdaExpression TestLM = Expr.Lambda(Expr.Condition(Expr.Constant(true), Expr.Constant(DateTime.MaxValue), Expr.Constant(DateTime.MinValue)));

            SwitchExpression MySw =
                Expr.Switch(
                    Expr.Constant((DateTime)TestLM.Compile().DynamicInvoke()),
                    Expr.Block(EU.ConcatEquals(Result, Expr.Constant("Default")), Expr.Constant(-1)),
                    new SwitchCase[] {
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C1_")),
                                Expr.Constant(10)
                            ),
                            Expr.Constant(DateTime.Now)
                        ),
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C2_")),
                                Expr.Constant(20)
                            ),
                            Expr.Constant(DateTime.MinValue)
                        ),
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C3_")),
                                EU.ConcatEquals(Result, Expr.Constant(Expr.Lambda(Expr.Constant("Lambda")).Compile().DynamicInvoke())),
                                Expr.Constant(30)
                            ),
                            Expr.Constant(DateTime.MaxValue)
                        )
                    }
                );

            Expressions.Add(EU.GenAreEqual(Expr.Constant(30), MySw, "Switch 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("C3_Lambda"), Result, "Switch 2"));

            var tree = Expr.Block(new[] { Result, TestValue }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Switch in switch case (nested switches)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 54", new string[] { "positive", "switch", "controlfflow", "Pri1" }, Priority = 1)]
        public static Expr Switch54(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression TestValue = Expr.Variable(typeof(string), "TestValue");
            ParameterExpression NestedTestValue = Expr.Variable(typeof(string), "NestedTestValue");

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant("Testing")));
            Expressions.Add(Expr.Assign(NestedTestValue, Expr.Constant("Nested")));

            LabelTarget target = Expr.Label(typeof(void));

            SwitchExpression MySw =
                Expr.Switch(
                    TestValue,
                    Expr.Block(EU.ConcatEquals(Result, Expr.Constant("Default")), Expr.Constant(-1.1)),
                    new SwitchCase[] {
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C1_")),
                                Expr.Constant(10.1)
                            ),
                            Expr.Constant("Hello"), Expr.Constant("World")
                        ),
                        Expr.SwitchCase(
                            Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C2_")),
                                Expr.Switch( // nested switch tests string and returns doubles
                                    NestedTestValue,
                                    Expr.Block(EU.ConcatEquals(Result, Expr.Constant("NDefault")), Expr.Constant(-11.1)),
                                    new SwitchCase[] {
                                        Expr.SwitchCase(Expr.Block(
                                                EU.ConcatEquals(Result, Expr.Constant("NC1_")),
                                                Expr.Constant(-10.1)
                                            ),
                                            Expr.Constant("A"), Expr.Constant("AAAAAAAAAAAAAAAAAAAAAAAAA")
                                        ),
                                        Expr.SwitchCase(Expr.Block(
                                                EU.ConcatEquals(Result, Expr.Constant("NC2_")),
                                                Expr.Constant(-20.1)
                                            ),
                                            Expr.Constant("B")
                                        ),
                                        Expr.SwitchCase(Expr.Block(
                                                EU.ConcatEquals(Result, Expr.Constant("NC3_")),
                                                Expr.Constant(-30.1)
                                            ),
                                            Expr.Constant("C")
                                        )
                                    }
                                )
                            ),
                            Expr.Constant("Testing")
                        ),
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C3_")),
                                Expr.Constant(30.1)
                            ),
                            Expr.Constant("Nothing")
                        )
                    }
                );

            Expressions.Add(EU.GenAreEqual(Expr.Constant(-11.1), MySw, "Switch 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("C2_NDefault"), Result, "Switch 2"));

            var tree = Expr.Block(new[] { Result, TestValue, NestedTestValue }, Expressions);
            V.Validate(tree);
            return tree;
        }

        public static bool SwitchComparer(DateTime x, DateTime y) {
            return (x > DateTime.Now) ? true : false;
        }

        // Switch with custom comparator MethodInfo
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 55", new string[] { "positive", "switch", "controlfflow", "Pri1" }, Priority = 1)]
        public static Expr Switch55(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression TestValue = Expr.Variable(typeof(DateTime), "TestValue");

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(DateTime.MaxValue)));

            MethodInfo mi = typeof(Switch).GetMethod("SwitchComparer");

            SwitchExpression MySw =
                Expr.Switch(
                    Expr.Constant(DateTime.MaxValue),
                    Expr.Block(EU.ConcatEquals(Result, Expr.Constant("Default")), Expr.Constant(-1)),
                    mi,
                    new SwitchCase[] {
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C1_")),
                                Expr.Constant(10)
                            ),
                            Expr.Constant(DateTime.Now)
                        ),
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C2_")),
                                Expr.Constant(20)
                            ),
                            Expr.Constant(DateTime.MinValue)
                        ),
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C3_")),
                                Expr.Constant(30)
                            ),
                            Expr.Constant(DateTime.MaxValue)
                        )
                    }
                );

            Expressions.Add(EU.GenAreEqual(Expr.Constant(10), MySw, "Switch 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("C1_"), Result, "Switch 2"));

            var tree = Expr.Block(new[] { Result, TestValue }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Cases which return complex expressions (try, switch, etc)
        // Test values that are â€˜farâ€™ from each other (Ex. for ints test values of 1 and 10000000)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 56", new string[] { "positive", "switch", "controlfflow", "Pri1" }, Priority = 1)]
        public static Expr Switch56(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression TestValue = Expr.Variable(typeof(Nullable<int>), "TestValue");

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant((Nullable<int>)null, typeof(Nullable<int>))));

            SwitchExpression MySw =
                Expr.Switch(
                    TestValue,
                    Expr.Block(EU.ConcatEquals(Result, Expr.Constant("Default")), Expr.Constant(-1.1, typeof(Nullable<double>))),
                    new SwitchCase[] {
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C1_")),
                                Expr.Constant(10.1, typeof(Nullable<double>))
                            ),
                            Expr.Constant(1, typeof(Nullable<int>)), Expr.Constant(999999999, typeof(Nullable<int>)), Expr.Constant(1000, typeof(Nullable<int>))
                        ),
                        Expr.SwitchCase(Expr.Block(
                                Expr.TryCatchFinally(
                                    Expr.Block(
                                        EU.ConcatEquals(Result, Expr.Constant("Try")),
                                        Expr.Throw(Expr.Constant(new DivideByZeroException())),
                                        Expr.Constant(20.1, typeof(Nullable<double>))
                                    ),
                                    Expr.Block(EU.ConcatEquals(Result, Expr.Constant("Finally")), Expr.Constant(1.1, typeof(Nullable<double>))),
                                    Expr.Catch(typeof(DivideByZeroException), Expr.Block(EU.ConcatEquals(Result, Expr.Constant("Catch")), Expr.Constant(2.1, typeof(Nullable<double>))))
                                )
                            ),
                            Expr.Constant(-32112212, typeof(Nullable<int>)), Expr.Constant(10000000, typeof(Nullable<int>)), Expr.Constant((Nullable<int>)null, typeof(Nullable<int>))
                        ),
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C3_")),
                                Expr.Constant(30.1, typeof(Nullable<double>))
                            ),
                            Expr.Constant(5, typeof(Nullable<int>)), Expr.Constant(6, typeof(Nullable<int>))
                        )
                    }
                );

            Expressions.Add(EU.GenAreEqual(Expr.Constant(2.1, typeof(Nullable<double>)), MySw, "Switch 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("TryCatchFinally"), Result, "Switch 2"));

            var tree = Expr.Block(new[] { Result, TestValue }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // First test value is Expr.Constant(null) comparing to nullable type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 57", new string[] { "positive", "switch", "controlfflow", "Pri1" }, Priority = 1)]
        public static Expr Switch57(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression TestValue = Expr.Variable(typeof(Nullable<int>), "TestValue");

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant((Nullable<int>)1, typeof(Nullable<int>))));

            SwitchExpression MySw =
                Expr.Switch(
                    TestValue,
                    Expr.Block(EU.ConcatEquals(Result, Expr.Constant("Default")), Expr.Constant(-1.1, typeof(Nullable<double>))),
                    new SwitchCase[] {
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C1_")),
                                Expr.Constant(10.1, typeof(Nullable<double>))
                            ),
                            Expr.Constant(null, typeof(Nullable<int>))
                        ),
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C2_")),
                                Expr.Constant(20.1, typeof(Nullable<double>))
                            ),
                            Expr.Constant(1, typeof(Nullable<int>))
                        ),
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C3_")),
                                Expr.Constant(30.1, typeof(Nullable<double>))
                            ),
                            Expr.Constant(2, typeof(Nullable<int>))
                        )
                    }
                );

            Expressions.Add(EU.GenAreEqual(Expr.Constant(20.1, typeof(Nullable<double>)), MySw, "Switch 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("C2_"), Result, "Switch 2"));

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant((Nullable<int>)null, typeof(Nullable<int>))));
            Expressions.Add(Expr.Assign(Result, Expr.Constant("")));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(10.1, typeof(Nullable<double>)), MySw, "Switch 3"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("C1_"), Result, "Switch 4"));

            var tree = Expr.Block(new[] { Result, TestValue }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // First test value is Expr.Constant(null) comparing to reference type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 58", new string[] { "positive", "switch", "controlfflow", "Pri1" }, Priority = 1)]
        public static Expr Switch58(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression TestValue = Expr.Variable(typeof(TestClass), "TestValue");

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(new TestClass(1))));

            SwitchExpression MySw =
                Expr.Switch(
                    TestValue,
                    Expr.Block(EU.ConcatEquals(Result, Expr.Constant("Default")), Expr.Constant(-1)),
                    new SwitchCase[] {
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C1_")),
                                Expr.Constant(1)
                            ),
                            Expr.Constant((TestClass)null, typeof(TestClass))
                        ),
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C2_")),
                                Expr.Constant(2)
                            ),
                            Expr.Constant(new TestClass(2))
                        ),
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C3_")),
                                Expr.Constant(3)
                            ),
                            Expr.Constant(new TestClass(3))
                        )
                    }
                );

            Expressions.Add(EU.GenAreEqual(Expr.Constant(-1), MySw, "Switch 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Default"), Result, "Switch 2"));

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant((TestClass)null, typeof(TestClass))));
            Expressions.Add(Expr.Assign(Result, Expr.Constant("")));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(1), MySw, "Switch 3"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("C1_"), Result, "Switch 4"));

            var tree = Expr.Block(new[] { Result, TestValue }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Switch with large numbers of cases
        // Note: very slow with rewriters
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 59", new string[] { "positive", "switch", "controlfflow", "Pri2" }, Priority = 2)]
        public static Expr Switch59(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression TestValue = Expr.Variable(typeof(int), "TestValue");

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(999)));

            var cases = new List<SwitchCase>();
            for (int i = 0; i < 1000; i++) {
                cases.Add(Expr.SwitchCase(Expr.Block(EU.ConcatEquals(Result, Expr.Constant("C" + i + "_")), Expr.Constant(-i)), Expr.Constant(i)));
            }

            SwitchExpression MySw =
                Expr.Switch(
                    TestValue,
                    Expr.Block(EU.ConcatEquals(Result, Expr.Constant("Default")), Expr.Constant(-1)),
                    cases.ToArray()
                );

            Expressions.Add(EU.GenAreEqual(Expr.Constant(-999), MySw, "Switch 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("C999_"), Result, "Switch 2"));

            ParameterExpression DoNotVisitTemp = Expression.Parameter(typeof(int), "dont_visit_node");
            var tree = Expr.Block(new[] { Result, TestValue, DoNotVisitTemp }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Switch with large numbers of test values for a case
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 60", new string[] { "positive", "switch", "controlfflow", "Pri2" }, Priority = 2)]
        public static Expr Switch60(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression TestValue = Expr.Variable(typeof(int), "TestValue");

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(9999)));

            var tests = new List<Expr>();
            for (int i = 0; i < 10000; i++) {
                tests.Add(Expr.Constant(i));
            }

            SwitchExpression MySw =
                Expr.Switch(
                    TestValue,
                    Expr.Block(EU.ConcatEquals(Result, Expr.Constant("Default")), Expr.Constant(-1)),
                    Expr.SwitchCase(Expr.Block(EU.ConcatEquals(Result, Expr.Constant("C1_")), Expr.Constant(1)), Expr.Constant(1)),
                    Expr.SwitchCase(Expr.Block(EU.ConcatEquals(Result, Expr.Constant("C2_")), Expr.Constant(2)), tests),
                    Expr.SwitchCase(Expr.Block(EU.ConcatEquals(Result, Expr.Constant("C3_")), Expr.Constant(3)), Expr.Constant(3))
                );

            Expressions.Add(EU.GenAreEqual(Expr.Constant(2), MySw, "Switch 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("C2_"), Result, "Switch 2"));

            ParameterExpression DoNotVisitTemp = Expression.Parameter(typeof(int), "dont_visit_node");
            var tree = Expr.Block(new[] { Result, TestValue, DoNotVisitTemp }, Expressions);
            V.Validate(tree);
            return tree;
        }

        public static bool GenComparator(Func<IEnumerator> a, Func<IEnumerator> b) {
            return true;
        }

        // Generator in test value
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 61", new string[] { "positive", "switch", "controlflow", "Pri1" }, Priority = 1)]
        public static Expr Switch61(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(bool), "Result");
            ParameterExpression Value = Expr.Variable(typeof(object), "Value");
            ParameterExpression SwitchResult = Expr.Variable(typeof(string), "SwitchResult");
            ParameterExpression TestValue = Expr.Variable(typeof(Func<IEnumerator>), "TestValue");

            LabelTarget target = Expr.Label(typeof(int), "MyLabel");
            LabelTarget target2 = Expr.Label(typeof(void), "MyOtherLabel");

            Func<IEnumerator> func = () => { return new List<int> { 1, 2, 3 }.GetEnumerator(); };
            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(func)));

            MethodInfo comparator = typeof(Switch).GetMethod("GenComparator");

            var Gen =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    target,
                    Expr.Block(
                        AstUtils.YieldReturn(target, Expr.Constant(1)),
                        Expr.Goto(target2),
                        AstUtils.YieldReturn(target, Expr.Constant(2)),
                        AstUtils.YieldReturn(target, Expr.Constant(3)),
                        Expr.Label(target2),
                        AstUtils.YieldReturn(target, Expr.Constant(5)),
                        AstUtils.YieldReturn(target, Expr.Constant(6))
                    ),
                    new ParameterExpression[] { }
                );

            Expr MySw =
                Expr.Switch(
                    TestValue,
                    Expr.Block(Gen, EU.ConcatEquals(SwitchResult, Expr.Constant("Default"))),
                    comparator,
                    Expr.SwitchCase(
                        EU.ConcatEquals(SwitchResult, Expr.Constant("C1_")),
                        Expr.Block(EU.ConcatEquals(SwitchResult, Expr.Constant("Test1")), Gen)
                    ),
                    Expr.SwitchCase(
                        EU.ConcatEquals(SwitchResult, Expr.Constant("C2_")),
                        Expr.Block(EU.ConcatEquals(SwitchResult, Expr.Constant("Test2")), Gen)
                    )
                );

            IEnumerator enumerator = (IEnumerator)Gen.Compile().DynamicInvoke();
            Expr e = Expr.Constant(enumerator, typeof(IEnumerator));

            Expressions.Add(MySw);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Test1C1_"), SwitchResult, "Goto 1"));

            EU.Enumerate(ref Expressions, new int[] { 1, 5, 6 }, 3, e, ref Result, ref Value);

            // without a custom comparator
            var Gen2 =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    target,
                    Expr.Block(
                        AstUtils.YieldReturn(target, Expr.Constant(1)),
                        AstUtils.YieldReturn(target, Expr.Constant(2)),
                        AstUtils.YieldReturn(target, Expr.Constant(3)),
                        AstUtils.YieldReturn(target, Expr.Constant(4)),
                        AstUtils.YieldReturn(target, Expr.Constant(5))
                    ),
                    new ParameterExpression[] { }
                );

            IEnumerator enumerator2 = (IEnumerator)Gen2.Compile().DynamicInvoke();
            Expr e2 = Expr.Constant(enumerator2, typeof(IEnumerator));

            Expr MySw2 =
                Expr.Switch(
                    TestValue,
                    Expr.Block(Gen, EU.ConcatEquals(SwitchResult, Expr.Constant("Default"))),
                    Expr.SwitchCase(
                        EU.ConcatEquals(SwitchResult, Expr.Constant("C1_")),
                        Expr.Block(EU.ConcatEquals(SwitchResult, Expr.Constant("Test1")), Gen)
                    ),
                    Expr.SwitchCase(
                        EU.ConcatEquals(SwitchResult, Expr.Constant("C2_")),
                        Expr.Block(EU.ConcatEquals(SwitchResult, Expr.Constant("Test2")), Gen)
                    )
                );

            Expressions.Add(Expr.Assign(SwitchResult, Expr.Constant("")));

            Expressions.Add(MySw2);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Test1Test2Default"), SwitchResult, "Goto 2"));

            EU.Enumerate(ref Expressions, new int[] { 1, 2, 3, 4, 5 }, 5, e2, ref Result, ref Value);

            var tree = Expr.Block(new[] { Result, Value, SwitchResult, TestValue }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Generator in switch case
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 62", new string[] { "positive", "switch", "controlflow", "Pri1" }, Priority = 1)]
        public static Expr Switch62(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(bool), "Result");
            ParameterExpression Value = Expr.Variable(typeof(object), "Value");
            ParameterExpression SwitchResult = Expr.Variable(typeof(string), "SwitchResult");
            ParameterExpression TestValue = Expr.Variable(typeof(int), "TestValue");

            LabelTarget target = Expr.Label(typeof(int), "MyLabel");
            LabelTarget target2 = Expr.Label(typeof(void), "MyOtherLabel");

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(2)));

            var Gen =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    target,
                    Expr.Block(
                        AstUtils.YieldReturn(target, Expr.Constant(1)),
                        AstUtils.YieldReturn(target, Expr.Constant(2)),
                        AstUtils.YieldReturn(target, Expr.Constant(3))
                    ),
                    new ParameterExpression[] { }
                );

            Expr MySw =
                Expr.Switch(
                    TestValue,
                    Expr.Block(Gen, EU.ConcatEquals(SwitchResult, Expr.Constant("Default"))),
                    Expr.SwitchCase(
                        Expr.Block(Gen, EU.ConcatEquals(SwitchResult, Expr.Constant("C1_"))),
                        Expr.Constant(1)
                    ),
                    Expr.SwitchCase(
                        Expr.Block(Gen, EU.ConcatEquals(SwitchResult, Expr.Constant("C2_"))),
                        Expr.Constant(2)
                    )
                );

            IEnumerator enumerator = (IEnumerator)Gen.Compile().DynamicInvoke();
            Expr e = Expr.Constant(enumerator, typeof(IEnumerator));

            Expressions.Add(MySw);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("C2_"), SwitchResult, "Goto 1"));

            EU.Enumerate(ref Expressions, new int[] { 1, 2, 3 }, 3, e, ref Result, ref Value);

            var tree = Expr.Block(new[] { Result, Value, SwitchResult, TestValue }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // switch in a generator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 62_1", new string[] { "positive", "switch", "controlflow", "Pri1" }, Priority = 1)]
        public static Expr Switch62_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(bool), "Result");
            ParameterExpression Value = Expr.Variable(typeof(object), "Value");
            ParameterExpression SwitchResult = Expr.Variable(typeof(string), "SwitchResult");
            ParameterExpression TestValue = Expr.Variable(typeof(int), "TestValue");

            LabelTarget target = Expr.Label(typeof(int), "MyLabel");
            LabelTarget target2 = Expr.Label(typeof(void), "MyOtherLabel");

            Expr MySw =
                Expr.Switch(
                    TestValue,
                    Expr.Block(EU.ConcatEquals(SwitchResult, Expr.Constant("Default")), Expr.Constant(-10)),
                    Expr.SwitchCase(
                        Expr.Block(EU.ConcatEquals(SwitchResult, Expr.Constant("C1_")), Expr.Constant(10)),
                        Expr.Constant(1)
                    ),
                    Expr.SwitchCase(
                        Expr.Block(EU.ConcatEquals(SwitchResult, Expr.Constant("C2_")), Expr.Constant(20)),
                        Expr.Constant(2)
                    )
                );

            var Gen =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    target,
                    Expr.Block(
                        AstUtils.YieldReturn(target, Expr.Constant(1)),
                        AstUtils.YieldReturn(target, Expr.Block(new[] { TestValue, SwitchResult }, MySw)),
                        AstUtils.YieldReturn(target, Expr.Constant(3))
                    ),
                    new ParameterExpression[] { }
                );

            IEnumerator enumerator = (IEnumerator)Gen.Compile().DynamicInvoke();
            Expr e = Expr.Constant(enumerator, typeof(IEnumerator));

            Expressions.Add(MySw);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Default"), SwitchResult, "Goto 1"));

            EU.Enumerate(ref Expressions, new int[] { 1, -10, 3 }, 3, e, ref Result, ref Value);

            var tree = Expr.Block(new[] { Result, Value, SwitchResult, TestValue }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass null to all arguments
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 63", new string[] { "negative", "switch", "controlflow", "Pri2" }, Exception = typeof(ArgumentNullException), Priority = 2)]
        public static Expr Switch63(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");

            Expr MySw =
                EU.Throws<System.ArgumentNullException>(() =>
                {
                    Expr.Switch(
                        (Expression)null,
                        (SwitchCase)null
                    );
                });

            Expressions.Add(MySw);

            return Expr.Empty();
        }

        // Fall through to default
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 64", new string[] { "positive", "switch", "controlflow", "Pri2" }, Priority = 2)]
        public static Expr Switch64(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression TestValue = Expr.Variable(typeof(int), "TestValue");

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(-1)));

            Expr MySw =
                Expr.Switch(
                    TestValue,
                    Expr.Block(EU.ConcatEquals(Result, Expr.Constant("Default"))),
                    Expr.SwitchCase(
                        Expr.Block(EU.ConcatEquals(Result, Expr.Constant("C1_"))),
                        Expr.Constant(1)
                    ),
                    Expr.SwitchCase(
                        Expr.Block(EU.ConcatEquals(Result, Expr.Constant("C2_"))),
                        Expr.Constant(2)
                    )
                );

            Expressions.Add(MySw);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Default"), Result, "Switch 1"));

            var tree = Expr.Block(new[] { Result, TestValue }, Expressions);
            V.Validate(tree);
            return tree;
        }

        public static bool ExcepComparator(Exception switchValue, Exception testValue) {
            return (switchValue.Message == testValue.Message) ? true : false;
        }

        // Custom comparator, cases with different test values which are reference assignable to comparator argument
        // Regression for Dev10 Bug 630021, 624218
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 65", new string[] { "positive", "switch 65", "controlflow", "Pri1" }, Priority = 1)]
        public static Expr Switch65(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression TestValue = Expr.Variable(typeof(Exception), "TestValue");

            MethodInfo mi = typeof(Switch).GetMethod("ExcepComparator");

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(new Exception("TestException"))));

            Expr MySw =
                Expr.Switch(
                    TestValue,
                    EU.ConcatEquals(Result, Expr.Constant("Default")),
                    mi,
                    new SwitchCase[] {
                        Expr.SwitchCase(
                            EU.ConcatEquals(Result, Expr.Constant("C1")),
                            Expr.Constant(new DivideByZeroException("NotTestException"))
                        ),
                        Expr.SwitchCase(
                            EU.ConcatEquals(Result, Expr.Constant("C2")),
                            Expr.Constant(new ArgumentOutOfRangeException("TestException"))
                        ),
                        Expr.SwitchCase(
                            EU.ConcatEquals(Result, Expr.Constant("C3")),
                            Expr.Constant(new ArrayTypeMismatchException("AlsoNotTestException"))
                        )
                    }
                );

            Expressions.Add(MySw);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Default"), Result, "Switch 1"));

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(new ArgumentOutOfRangeException("TestException"))));
            Expressions.Add(Expr.Assign(Result, Expr.Constant("")));
            Expressions.Add(MySw);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("C2"), Result, "Switch 2"));

            var tree = Expr.Block(new[] { Result, TestValue }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Custom comparator, cases with different test values which are not reference assignable to comparator argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 66", new string[] { "negative", "switch 66", "controlflow", "Pri1" }, Priority = 1, Exception = typeof(ArgumentException))]
        public static Expr Switch66(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression TestValue = Expr.Variable(typeof(Exception), "TestValue");

            MethodInfo mi = typeof(Switch).GetMethod("ExcepComparator");

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(new Exception("TestException"))));

            Expr MySw =
                EU.Throws<System.ArgumentException>(() =>
                {
                    Expr.Switch(
                        TestValue,
                        EU.ConcatEquals(Result, Expr.Constant("Default")),
                        mi,
                        new SwitchCase[] {
	                        Expr.SwitchCase(
	                            EU.ConcatEquals(Result, Expr.Constant("C1")),
	                            Expr.Constant(new DivideByZeroException("NotTestException"))
	                        ),
	                        Expr.SwitchCase(
	                            EU.ConcatEquals(Result, Expr.Constant("C2")),
	                            Expr.Constant(new ArrayTypeMismatchException("TestException"))
	                        ),
	                        Expr.SwitchCase(
	                            EU.ConcatEquals(Result, Expr.Constant("C3")),
	                            Expr.Constant("AlsoNotTestException")
	                        )
	                    }
                    );
                });

            Expressions.Add(MySw);

            return Expr.Empty();
        }

        public static bool IntComparator(int switchValue, int testValue) {
            return (switchValue % testValue == 0) ? true : false;
        }

        // SwitchExpression with explicit type specified matching case return type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 67", new string[] { "positive", "switch 67", "controlflow", "Pri1" }, Priority = 1)]
        public static Expr Switch67(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression TestValue = Expr.Variable(typeof(int), "TestValue");

            MethodInfo mi = typeof(Switch).GetMethod("IntComparator");

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(10)));

            Expr MySw =
                Expr.Switch(
                    typeof(string),
                    TestValue,
                    EU.ConcatEquals(Result, Expr.Constant("Default")),
                    mi,
                    new SwitchCase[] {
                        Expr.SwitchCase(
                            EU.ConcatEquals(Result, Expr.Constant("C1")),
                            Expr.Constant(3)
                        ),
                        Expr.SwitchCase(
                            EU.ConcatEquals(Result, Expr.Constant("C2")),
                            Expr.Constant(4)
                        ),
                        Expr.SwitchCase(
                            EU.ConcatEquals(Result, Expr.Constant("C3")),
                            Expr.Constant(5)
                        )
                    }
                );

            Expressions.Add(MySw);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("C3"), Result, "Switch 1"));

            var tree = Expr.Block(new[] { Result, TestValue }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // SwitchExpression with explicit type specified not matching case return type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 68", new string[] { "negative", "switch 68", "controlflow", "Pri1" }, Priority = 1, Exception = typeof(ArgumentException))]
        public static Expr Switch68(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression TestValue = Expr.Variable(typeof(int), "TestValue");

            MethodInfo mi = typeof(Switch).GetMethod("IntComparator");

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(10)));

            Expr MySw =
                EU.Throws<System.ArgumentException>(() =>
                {
                    Expr.Switch(
                        typeof(int),
                        TestValue,
                        EU.ConcatEquals(Result, Expr.Constant("Default")),
                        mi,
                        new SwitchCase[] {
	                        Expr.SwitchCase(
	                            EU.ConcatEquals(Result, Expr.Constant("C1")),
	                            Expr.Constant(3)
	                        ),
	                        Expr.SwitchCase(
	                            EU.ConcatEquals(Result, Expr.Constant("C2")),
	                            Expr.Constant(4)
	                        ),
	                        Expr.SwitchCase(
	                            EU.ConcatEquals(Result, Expr.Constant("C3")),
	                            Expr.Constant(5)
	                        )
	                    }
                    );
                });

            Expressions.Add(MySw);

            return Expr.Empty();
        }

        // SwitchExpression return type explicitly specified, cases with reference assignable types that match
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 69", new string[] { "positive", "switch 69", "controlflow", "Pri1" }, Priority = 1)]
        public static Expr Switch69(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression TestValue = Expr.Variable(typeof(int), "TestValue");

            MethodInfo mi = typeof(Switch).GetMethod("IntComparator");

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(10)));

            Expr MySw =
                Expr.Switch(
                    typeof(Exception),
                    TestValue,
#if SILVERLIGHT
                    Expr.Constant(new AppDomainUnloadedException()),
#else
                    Expr.Constant(new ApplicationException()),
#endif
                    mi,
                    new SwitchCase[] {
                        Expr.SwitchCase(
                            Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C1")),
                                Expr.Constant(new DivideByZeroException())
                            ),
                            Expr.Constant(3)
                        ),
                        Expr.SwitchCase(
                            Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C2")),
                                Expr.Constant(new ArrayTypeMismatchException())
                            ),
                            Expr.Constant(4)
                        ),
                        Expr.SwitchCase(
                            Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C3")),
                                Expr.Constant(new ArithmeticException())
                            ),
                            Expr.Constant(5)
                        )
                    }
                );

            Expressions.Add(MySw);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("C3"), Result, "Switch 1"));

            var tree = Expr.Block(new[] { Result, TestValue }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // SwitchExpression return type explicitly specified, cases with types that aren't reference assignable
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 70", new string[] { "negative", "switch 70", "controlflow", "Pri1" }, Priority = 1, Exception = typeof(ArgumentException))]
        public static Expr Switch70(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression TestValue = Expr.Variable(typeof(int), "TestValue");

            MethodInfo mi = typeof(Switch).GetMethod("IntComparator");

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(4)));

            Expr MySw =
                EU.Throws<System.ArgumentException>(() =>
                {
                    Expr.Switch(
                        typeof(DivideByZeroException),
                        TestValue,
#if SILVERLIGHT
                        Expr.Constant(new AppDomainUnloadedException()),
#else
                        Expr.Constant(new ApplicationException()),
#endif
                        mi,
                        new SwitchCase[] {
	                        Expr.SwitchCase(
	                            Expr.Block(
	                                EU.ConcatEquals(Result, Expr.Constant("C1")),
	                                Expr.Constant(new DivideByZeroException())
	                            ),
	                            Expr.Constant(3)
	                        ),
	                        Expr.SwitchCase(
	                            Expr.Block(
	                                EU.ConcatEquals(Result, Expr.Constant("C2")),
	                                Expr.Constant(new ArrayTypeMismatchException())
	                            ),
	                            Expr.Constant(4)
	                        ),
	                        Expr.SwitchCase(
	                            Expr.Block(
	                                EU.ConcatEquals(Result, Expr.Constant("C3")),
	                                Expr.Constant(new ArithmeticException())
	                            ),
	                            Expr.Constant(5)
	                        )
	                    }
                    );
                });

            Expressions.Add(MySw);

            return Expr.Empty();
        }

        // SwitchExpression return type explicitly specified as null
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 71", new string[] { "negative", "switch 71", "controlflow", "Pri1" }, Priority = 1, Exception = typeof(ArgumentException))]
        public static Expr Switch71(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression TestValue = Expr.Variable(typeof(int), "TestValue");

            MethodInfo mi = typeof(Switch).GetMethod("IntComparator");

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(4)));

            Expr MySw =
                EU.Throws<System.ArgumentException>(() =>
                {
                    Expr.Switch(
                        null,
                        TestValue,
#if SILVERLIGHT
                        Expr.Constant(new AppDomainUnloadedException()),
#else
                        Expr.Constant(new ApplicationException()),
#endif
                        mi,
                        new SwitchCase[] {
	                        Expr.SwitchCase(
	                            Expr.Block(
	                                EU.ConcatEquals(Result, Expr.Constant("C1")),
	                                Expr.Constant(new DivideByZeroException())
	                            ),
	                            Expr.Constant(3)
	                        ),
	                        Expr.SwitchCase(
	                            Expr.Block(
	                                EU.ConcatEquals(Result, Expr.Constant("C2")),
	                                Expr.Constant(new ArrayTypeMismatchException())
	                            ),
	                            Expr.Constant(4)
	                        )
	                    }
                    );
                });

            Expressions.Add(MySw);

            return Expr.Empty();
        }

        // SwitchExpression return type explicitly specified as void, allows cases with non-reference convertible types that match
        // Dev10 bug 634854
        [ETUtils.TestAttribute(ETUtils.TestState.Disabled, "Switch 72", new string[] { "positive", "switch 72", "controlflow", "Pri1" }, Priority = 1)]
        public static Expr Switch72(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression TestValue = Expr.Variable(typeof(int), "TestValue");

            MethodInfo mi = typeof(Switch).GetMethod("IntComparator");

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(10)));

            Expr MySw =
                Expr.Switch(
                    typeof(void),
                    TestValue,
#if SILVERLIGHT
                    Expr.Constant(new AppDomainUnloadedException()),
#else
                    Expr.Constant(new ApplicationException()),
#endif
                    mi,
                    new SwitchCase[] {
                        Expr.SwitchCase(
                            Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C1")),
                                Expr.Constant(new DivideByZeroException())
                            ),
                            Expr.Constant(3)
                        ),
                        Expr.SwitchCase(
                            Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C2")),
                                Expr.Constant(new List<int> {1,2,3} )
                            ),
                            Expr.Constant(4)
                        ),
                        Expr.SwitchCase(
                            Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C3")),
                                Expr.Constant(new Object())
                            ),
                            Expr.Constant(5)
                        )
                    }
                );

            Expressions.Add(MySw);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("C3"), Result, "Switch 1"));

            var tree = Expr.Block(new[] { Result, TestValue }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Nullable switchValue, non-nullable case testValues
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 73", new string[] { "negative", "switch", "controlfflow", "Pri1" }, Priority = 1, Exception = typeof(InvalidOperationException))]
        public static Expr Switch73(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression TestValue = Expr.Variable(typeof(int?), "TestValue");

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(2, typeof(int?))));

            var MySw =
                EU.Throws<System.InvalidOperationException>(() =>
                {
                    Expr.Switch(
                        TestValue,
                        Expr.Block(EU.ConcatEquals(Result, Expr.Constant("Default")), Expr.Constant(-1)),
                        new SwitchCase[] {
	                        Expr.SwitchCase(Expr.Block(
	                                EU.ConcatEquals(Result, Expr.Constant("C1_")),
	                                Expr.Constant(10)
	                            ),
	                            Expr.Constant(1)
	                        ),
	                        Expr.SwitchCase(Expr.Block(
	                                EU.ConcatEquals(Result, Expr.Constant("C2_")),
	                                Expr.Constant(20)
	                            ),
	                            Expr.Constant(2)
	                        ),
	                        Expr.SwitchCase(Expr.Block(
	                                EU.ConcatEquals(Result, Expr.Constant("C3_")),
	                                Expr.Constant(30)
	                            ),
	                            Expr.Constant(3)
	                        )
	                    }
                    );
                });

            Expressions.Add(EU.GenAreEqual(Expr.Constant(20), MySw, "Switch 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("C2_"), Result, "Switch 2"));

            var tree = Expr.Block(new[] { Result, TestValue }, Expressions);

            return tree;
        }

        // Nullable switchValue and case testValues
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 74", new string[] { "positive", "switch", "controlfflow", "Pri1" }, Priority = 1)]
        public static Expr Switch74(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression TestValue = Expr.Variable(typeof(int?), "TestValue");

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(2, typeof(int?))));

            SwitchExpression MySw =
                Expr.Switch(
                    TestValue,
                    Expr.Block(EU.ConcatEquals(Result, Expr.Constant("Default")), Expr.Constant(-1)),
                    new SwitchCase[] {
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C1_")),
                                Expr.Constant(10)
                            ),
                            Expr.Constant(1, typeof(int?))
                        ),
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C2_")),
                                Expr.Constant(20)
                            ),
                            Expr.Constant(2, typeof(int?))
                        ),
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C3_")),
                                Expr.Constant(30)
                            ),
                            Expr.Constant(3, typeof(int?))
                        )
                    }
                );

            Expressions.Add(EU.GenAreEqual(Expr.Constant(20), MySw, "Switch 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("C2_"), Result, "Switch 2"));

            var tree = Expr.Block(new[] { Result, TestValue }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Nullable switchValue and case testValues with custom comparator that takes non-nullables
        // Regression for Dev10 bug 631899
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 75", new string[] { "positive", "switch", "controlfflow", "Pri1" }, Priority = 1)]
        public static Expr Switch75(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression TestValue = Expr.Variable(typeof(int?), "TestValue");

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(10, typeof(int?))));

            MethodInfo mi = typeof(Switch).GetMethod("IntComparator");

            SwitchExpression MySw =
                Expr.Switch(
                    TestValue,
                    Expr.Block(EU.ConcatEquals(Result, Expr.Constant("Default")), Expr.Constant(-1)),
                    mi,
                    new SwitchCase[] {
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C1_")),
                                Expr.Constant(10)
                            ),
                            Expr.Constant(3, typeof(int?))
                        ),
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C2_")),
                                Expr.Constant(20)
                            ),
                            Expr.Constant(4, typeof(int?))
                        ),
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C3_")),
                                Expr.Constant(30)
                            ),
                            Expr.Constant(5, typeof(int?))
                        )
                    }
                );

            Expressions.Add(EU.GenAreEqual(Expr.Constant(30), MySw, "Switch 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("C3_"), Result, "Switch 2"));

            var tree = Expr.Block(new[] { Result, TestValue }, Expressions);
            V.Validate(tree);
            return tree;
        }

        public static bool NullableComparator(int? switchValue, int? testValue) {
            if (!switchValue.HasValue || !testValue.HasValue)
                return false;
            else
                return (switchValue % testValue == 0) ? true : false;
        }

        // Nullable switchValue and case testValues with custom comparator that takes nullables
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 76", new string[] { "positive", "switch", "controlfflow", "Pri1" }, Priority = 1)]
        public static Expr Switch76(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression TestValue = Expr.Variable(typeof(int?), "TestValue");

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(10, typeof(int?))));

            MethodInfo mi = typeof(Switch).GetMethod("NullableComparator");

            SwitchExpression MySw =
                Expr.Switch(
                    TestValue,
                    Expr.Block(EU.ConcatEquals(Result, Expr.Constant("Default")), Expr.Constant(-1)),
                    mi,
                    new SwitchCase[] {
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C1_")),
                                Expr.Constant(10)
                            ),
                            Expr.Constant(3, typeof(int?))
                        ),
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C2_")),
                                Expr.Constant(20)
                            ),
                            Expr.Constant(null, typeof(int?))
                        ),
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C3_")),
                                Expr.Constant(30)
                            ),
                            Expr.Constant(5, typeof(int?))
                        )
                    }
                );

            Expressions.Add(EU.GenAreEqual(Expr.Constant(30), MySw, "Switch 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("C3_"), Result, "Switch 2"));

            var tree = Expr.Block(new[] { Result, TestValue }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // non-nullable switchValue and case testValues with custom comparator that takes nullables
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 77", new string[] { "negative", "switch", "controlfflow", "Pri1" }, Priority = 1, Exception = typeof(ArgumentException))]
        public static Expr Switch77(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression TestValue = Expr.Variable(typeof(int), "TestValue");

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(10)));

            MethodInfo mi = typeof(Switch).GetMethod("NullableComparator");

            var MySw =
                EU.Throws<System.ArgumentException>(() =>
                {
                    Expr.Switch(
                        TestValue,
                        Expr.Block(EU.ConcatEquals(Result, Expr.Constant("Default")), Expr.Constant(-1)),
                        mi,
                        new SwitchCase[] {
	                        Expr.SwitchCase(Expr.Block(
	                                EU.ConcatEquals(Result, Expr.Constant("C1_")),
	                                Expr.Constant(10)
	                            ),
	                            Expr.Constant(3)
	                        ),
	                        Expr.SwitchCase(Expr.Block(
	                                EU.ConcatEquals(Result, Expr.Constant("C2_")),
	                                Expr.Constant(20)
	                            ),
	                            Expr.Constant(4)
	                        ),
	                        Expr.SwitchCase(Expr.Block(
	                                EU.ConcatEquals(Result, Expr.Constant("C3_")),
	                                Expr.Constant(30)
	                            ),
	                            Expr.Constant(5)
	                        )
	                    }
                    );
                });

            return Expr.Empty();
        }

        // Switch which returns a nullable when explicitly specifying non-nullable
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 78", new string[] { "negative", "switch", "controlfflow", "Pri1" }, Priority = 1, Exception = typeof(ArgumentException))]
        public static Expr Switch78(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression TestValue = Expr.Variable(typeof(int?), "TestValue");

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(10, typeof(int?))));

            MethodInfo mi = typeof(Switch).GetMethod("NullableComparator");

            var MySw =
                EU.Throws<System.ArgumentException>(() =>
                {
                    Expr.Switch(
                        typeof(int),
                        TestValue,
                        Expr.Block(EU.ConcatEquals(Result, Expr.Constant("Default")), Expr.Constant(-1)),
                        mi,
                        new SwitchCase[] {
	                        Expr.SwitchCase(Expr.Block(
	                                EU.ConcatEquals(Result, Expr.Constant("C1_")),
	                                Expr.Constant(10, typeof(int?))
	                            ),
	                            Expr.Constant(3, typeof(int?))
	                        ),
	                        Expr.SwitchCase(Expr.Block(
	                                EU.ConcatEquals(Result, Expr.Constant("C2_")),
	                                Expr.Constant(20, typeof(int?))
	                            ),
	                            Expr.Constant(null, typeof(int?))
	                        ),
	                        Expr.SwitchCase(Expr.Block(
	                                EU.ConcatEquals(Result, Expr.Constant("C3_")),
	                                Expr.Constant(30, typeof(int?))
	                            ),
	                            Expr.Constant(5, typeof(int?))
	                        )
	                    }
                    );
                });

            return Expr.Empty();
        }

        // Switch which returns a non-nullable when explicitly specifying a nullable
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 79", new string[] { "negative", "switch", "controlfflow", "Pri1" }, Priority = 1, Exception = typeof(ArgumentException))]
        public static Expr Switch79(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression TestValue = Expr.Variable(typeof(int?), "TestValue");

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(10, typeof(int?))));

            MethodInfo mi = typeof(Switch).GetMethod("NullableComparator");

            var MySw =
                EU.Throws<System.ArgumentException>(() =>
                {
                    Expr.Switch(
                        typeof(int?),
                        TestValue,
                        Expr.Block(EU.ConcatEquals(Result, Expr.Constant("Default")), Expr.Constant(-1)),
                        mi,
                        new SwitchCase[] {
	                        Expr.SwitchCase(Expr.Block(
	                                EU.ConcatEquals(Result, Expr.Constant("C1_")),
	                                Expr.Constant(10)
	                            ),
	                            Expr.Constant(3, typeof(int?))
	                        ),
	                        Expr.SwitchCase(Expr.Block(
	                                EU.ConcatEquals(Result, Expr.Constant("C2_")),
	                                Expr.Constant(20)
	                            ),
	                            Expr.Constant(null, typeof(int?))
	                        ),
	                        Expr.SwitchCase(Expr.Block(
	                                EU.ConcatEquals(Result, Expr.Constant("C3_")),
	                                Expr.Constant(30)
	                            ),
	                            Expr.Constant(5, typeof(int?))
	                        )
	                    }
                    );
                });

            return Expr.Empty();
        }

        // Switch that returns a nullable
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Switch 80", new string[] { "positive", "switch", "controlfflow", "Pri1" }, Priority = 1)]
        public static Expr Switch80(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression TestValue = Expr.Variable(typeof(int), "TestValue");

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(4)));

            SwitchExpression MySw =
                Expr.Switch(
                    TestValue,
                    Expr.Block(EU.ConcatEquals(Result, Expr.Constant("Default")), Expr.Constant(-1, typeof(int?))),
                    new SwitchCase[] {
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C1_")),
                                Expr.Constant(10, typeof(int?))
                            ),
                            Expr.Constant(3)
                        ),
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C2_")),
                                Expr.Constant(20, typeof(int?))
                            ),
                            Expr.Constant(4)
                        ),
                        Expr.SwitchCase(Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("C3_")),
                                Expr.Constant(30, typeof(int?))
                            ),
                            Expr.Constant(5)
                        )
                    }
                );

            Expressions.Add(EU.GenAreEqual(Expr.Constant(20, typeof(int?)), MySw, "Switch 1"));
            Expressions.Add(EU.ExprTypeCheck(MySw, typeof(int?)));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("C2_"), Result, "Switch 2"));

            var tree = Expr.Block(new[] { Result, TestValue }, Expressions);
            V.Validate(tree);
            return tree;
        }
    }
}
