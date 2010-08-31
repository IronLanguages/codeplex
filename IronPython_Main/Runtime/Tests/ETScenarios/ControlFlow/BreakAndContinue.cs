#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using MsSc = System.Dynamic;

namespace ETScenarios.ControlFlow {
    using EU = ETUtils.ExpressionUtils;
    using Expr = Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public class Break {
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Break 1", new string[] { "negative", "break", "controlflow", "Pri1" }, Exception=typeof(ArgumentNullException))]
		public static Expr Break1(EU.IValidator V) {
            return EU.Throws<ArgumentException>(() => { Expr.Break(null); });
        }

        // NOTE: This negative test is obsolete. This is intentionally working
        // now. Running it causes an infinite loop, because the "break" will
        // jump back to loop's break label.
        //
        //[ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Break 2", new string[] { "negative", "break", "controlflow", "Pri1" }, Exception = typeof(InvalidOperationException ))]
		//public static Expr Break2(EU.Validator V) {
        //    List<Expression> Expressions = new List<Expression>();
        //    LabelTarget Label = Expr.Label();
        //    Expressions.Add(Expr.Block(Expr.DoWhile(Expr.Empty(), Expr.Constant(false), Label, null)));
        //    Expressions.Add(Expr.Break(Label));
            //var tree = Expr.Block(Expressions);
            //V.ValidateException<InvalidOperationException >(tree, false);
            //return tree;
        //}

        // Use a break outside of a "breakable" element, such as an if statement, with a valid target label
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Break 3", new string[] { "positive", "break", "controlflow", "Pri2" })]
		public static Expr Break3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression TestValue = Expr.Variable(typeof(Int32), "");

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(3)));

            LabelTarget target = Expr.Label(typeof(void), "MyLabel");

            Expr IfBlock =
                Expr.Block(
                    EU.ConcatEquals(Result, Expr.Constant("1")),
                    Expr.Condition(
                        Expr.Equal(TestValue, Expr.Constant(3)),
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("BeginTrue")),
                            Expr.Break(target),
                            EU.ConcatEquals(Result, Expr.Constant("EndTrue"))
                        ),
                        EU.ConcatEquals(Result, Expr.Constant("False"))
                    ),
                    EU.ConcatEquals(Result, Expr.Constant("2")),
                    Expr.Label(target),
                    EU.ConcatEquals(Result, Expr.Constant("3"))
                );

            Expressions.Add(IfBlock);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("1BeginTrue3"), Result, "Break 1"));

			var tree = EU.BlockVoid(new [] { Result, TestValue }, Expressions);
			V.Validate(tree);
			return tree;
        }
    }

    public class Continue {
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Continue 1", new string[] { "negative", "continue", "controlflow", "Pri1" }, Exception = typeof(ArgumentNullException))]
		public static Expr Continue1(EU.IValidator V) {
            return EU.Throws<ArgumentException>(() => { Expr.Continue(null); });
        }

        // NOTE: This negative test is obsolete. This is intentionally working
        // now. Running it causes an infinite loop, because the "continue" will
        // jump back to loop's continue label.
        //
        //[ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Continue 2", new string[] { "negative", "continue", "controlflow", "Pri1" }, Exception = typeof(InvalidOperationException))]
		//public static Expr Continue2(EU.Validator V) {
        //    List<Expression> Expressions = new List<Expression>();
        //    LabelTarget Label = Expr.Label();

        //    Expressions.Add(Expr.Block(Expr.DoWhile(Expr.Empty(), Expr.Constant(false), null, Label)));
        //    Expressions.Add(Expr.Continue(Label));  
            //var tree = Expr.Block(Expressions);
            //V.ValidateException<InvalidOperationException>(tree, false);
            //return tree;
        //}
    }
}
