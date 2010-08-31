#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace ETScenarios.ControlFlow {
    using Expr = Expression;
    using MsSc = System.Dynamic;
    using EU = ETUtils.ExpressionUtils;

    public class CReturn {

        // This test is obsolete with the new Return design
        //[ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Return 1", new string[] { "negative", "throw", "controlflow", "Pri1" }, Exception = typeof(InvalidOperationException))]
		//public static Expr Return1(EU.Validator V) {
        //    List<Expression> Expressions = new List<Expression>();
        //    ParameterExpression Result = Expr.Variable(typeof(string), "");
        //
        //    Expr Lmb = Expr.Lambda<Action>(Expr.Return(Expr.Constant(5)));
        //
        //    Expressions.Add(Lmb);
        //
            //var tree = EU.BlockVoid(new [] { Result }, Expressions);
            //V.ValidateException<InvalidOperationException>(tree, false);
            //return tree;
        //}

        // Return with typeof() for code coverage of that factory
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Return 2", new string[] { "positive", "throw", "controlflow", "Pri1" })]
		public static Expr Return2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

            var label = Expr.Label(typeof(int));
            Expr Body = Expr.Return(label, Expr.Block(EU.GenThrow("testing"), Expr.Constant(5)), typeof(int));
            Expr LM = Expr.Lambda<Func<int>>(Expr.Label(label, Expr.Block(Body, Expr.Default(typeof(int)))));
            Expr InvokeLM = Expr.Invoke(LM);
                        
            CatchBlock Catch = Expr.Catch(typeof(Exception), Expression.Block(EU.ConcatEquals(Result, "Caught"), Expression.Default(typeof(int))));

            Expressions.Add(Expr.TryCatch(InvokeLM, Catch));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Caught"), Result, "Return 1"));

			var tree = EU.BlockVoid(new [] { Result }, Expressions);
			V.Validate(tree);
			return tree;
        }


        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Return 3", new string[] { "positive", "throw", "controlflow", "Pri1" })]
		public static Expr Return3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

            var label = Expr.Label(typeof(int));
            Expr Body = Expr.Return(label, Expr.Constant(5));
            CatchBlock Catch = Expr.Catch(typeof(Exception), EU.BlockVoid(EU.ConcatEquals(Result, "Caught")));

            Expr LM = Expr.Lambda<Func<int>>(
                Expr.Label(
                    label,
                    Expr.Block(
                        Expr.TryCatchFinally(Body, EU.ConcatEquals(Result, "Finally"), Catch),
                        Expr.Default(typeof(int))
                    )
                )
            );
            Expr InvokeLM = Expr.Invoke(LM);


            Expressions.Add(InvokeLM);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Finally"), Result, "Return 1"));

			var tree = EU.BlockVoid(new [] { Result }, Expressions);
			V.Validate(tree);
			return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Return 4", new string[] { "negative", "try", "controlflow", "Pri1" }, Exception = typeof(InvalidOperationException))]
		public static Expr Return4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expr Body = Expr.Empty();
            CatchBlock Catch = Expr.Catch(typeof(Exception), EU.BlockVoid(EU.ConcatEquals(Result, "Caught")));

            var label = Expr.Label(typeof(int));
            Expr LM = Expr.Lambda<Func<int>>(
                Expr.Label(
                    label,
                    Expr.Block(
                        Expr.TryCatchFinally(
                            Body,
                            EU.BlockVoid(
                                EU.ConcatEquals(Result, "Finally1"),
                                Expr.Return(label, Expr.Constant(5)),
                                EU.ConcatEquals(Result, "Finally2")
                            ),
                            Catch
                        ),
                        Expr.Default(typeof(int))
                    )
                )
            );
            Expr InvokeLM = Expr.Invoke(LM);

            Expressions.Add(InvokeLM);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Finally1"), Result, "Return 1"));

			var tree = EU.BlockVoid(new [] { Result }, Expressions);
			V.ValidateException<InvalidOperationException>(tree, true);
			return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Return 5", new string[] { "positive", "controlflow", "Pri2" })]
		public static Expr Return5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Value = Expr.Variable(typeof(int), "");

            var label = Expr.Label(typeof(int));
            Expr Body = Expr.Return(label, Expr.Field(null,typeof(int).GetField("MaxValue", BindingFlags.Static | BindingFlags.Public )));
            Expr LM = Expr.Lambda<Func<int>>(Expr.Label(label, Expr.Block(Body, Expr.Default(typeof(int)))));
            Expr InvokeLM = Expr.Invoke(LM);

            Expressions.Add(Expr.Assign(Value, InvokeLM));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(int.MaxValue), Value, "Return 1"));

			var tree = EU.BlockVoid(new [] { Value }, Expressions);
			V.Validate(tree);
			return tree;
        }

        // generate a simple return statement
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Return 6", new string[] { "positive", "controlflow", "Pri2" })]
		public static Expr Return6(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

            var label = Expr.Label();
            Expr Body = EU.BlockVoid(EU.ConcatEquals(Result, "Return"), Expr.Return(label), EU.ConcatEquals(Result, "Bad"));
            Expr LM = Expr.Lambda(Expr.Label(label, Body));
            Expr InvokeLM = Expr.Invoke(LM);

            Expressions.Add(InvokeLM);

            Expressions.Add(EU.GenAreEqual(Expr.Constant("Return"), Result));

			var tree = EU.BlockVoid(new [] { Result }, Expressions);
			V.Validate(tree);
			return tree;
        }

        // pass null to the expression
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Return 7", new string[] { "positive", "controlflow", "Pri2" })]
		public static Expr Return7(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

            var label = Expr.Label();
            Expr Body = EU.BlockVoid(EU.ConcatEquals(Result, "Return"), Expr.Return(label, (Expr)null));
            Expr LM = Expr.Lambda(Expr.Label(label, Body));
            Expr InvokeLM = Expr.Invoke(LM);

            Expressions.Add(InvokeLM);

            Expressions.Add(EU.GenAreEqual(Expr.Constant("Return"), Result));

			var tree = EU.BlockVoid(new [] { Result }, Expressions);
			V.Validate(tree);
			return tree;
        }

        // pass a non value returning expression to the expression
        // Regression for Dev10 bug 498702
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Return 8", new string[] { "positive", "controlflow", "Pri3" })]
		public static Expr Return8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "");

            var label = Expr.Label();
            Expr Val = EU.BlockVoid(EU.ConcatEquals(Result, "Val"));
            Expr Body = EU.BlockVoid(EU.ConcatEquals(Result, "Return"), Expr.Return(label, Val));
            Expr LM = Expr.Lambda(Expr.Label(label, Body));
            Expr InvokeLM = Expr.Invoke(LM);

            Expressions.Add(InvokeLM);

            Expressions.Add(EU.GenAreEqual(Expr.Constant("ReturnVal"), Result));

			var tree = EU.BlockVoid(new [] { Result }, Expressions);
			V.Validate(tree);
			return tree;
        }

        // pass null to the annotations
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Return 9", new string[] { "positive", "controlflow", "Pri3" })]
		public static Expr Return9(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

			LabelTarget returnTarget = Expr.Label();
            Expr Body = EU.BlockVoid(EU.ConcatEquals(Result, "Return"), Expr.Return(returnTarget));
            Expr LM = Expr.Lambda(Expr.Label(returnTarget, Body));
            Expr InvokeLM = Expr.Invoke(LM);

            Expressions.Add(InvokeLM);

            Expressions.Add(EU.GenAreEqual(Expr.Constant("Return"), Result));

			var tree = EU.BlockVoid(new [] { Result }, Expressions);
			V.Validate(tree);
			return tree;
        }

        // pass a scope wrapped expression to expression
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Return 10", new string[] { "positive", "controlflow", "Pri2" })]
		public static Expr Return10(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression ScopedVal = Expr.Variable(typeof(Int32), "");

            Expr Val =
                EU.BlockVoid(
                    new [] { ScopedVal },
                    Expr.Block(
                        EU.ConcatEquals(Result, "Scoped"),
                        Expr.Assign(ScopedVal, Expr.Constant(4)),
                        Expr.Assign(ScopedVal, Expr.Add(ScopedVal, Expr.Constant(2))),
                        EU.GenAreEqual(ScopedVal, Expr.Constant(6))
                    )
                );

            var label = Expr.Label();
            Expr Body = EU.BlockVoid(EU.ConcatEquals(Result, "Return"), Expr.Return(label, Val));
            Expr LM = Expr.Lambda(Expr.Label(label, Body));
            Expr InvokeLM = Expr.Invoke(LM);

            Expressions.Add(InvokeLM);

            Expressions.Add(EU.GenAreEqual(Expr.Constant("ReturnScoped"), Result));

			var tree = EU.BlockVoid(new [] { Result }, Expressions);
			V.Validate(tree);
			return tree;
        }

        // return without a value in a lambda that returns a value
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Return 11", new string[] { "negative", "controlflow", "Pri2" }, Exception = typeof(ArgumentException))]
		public static Expr Return11(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expr Lmb = EU.Throws<ArgumentException>(() => { Expr.Lambda<Func<int>>(Expr.Return(Expr.Label(), typeof(void))); });

            return Expr.Empty();
        }

        // return a scope
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Return 12", new string[] { "positive", "controlflow", "Pri2" })]
		public static Expr Return12(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression ScopedVal = Expr.Variable(typeof(Int32), "");

            Expr Scope = 
                Expr.Block(
                    new ParameterExpression[] { ScopedVal },
                    Expr.Block(
                        EU.ConcatEquals(Result, "Scoped"),
                        Expr.Assign(ScopedVal, Expr.Constant(4)),
                        Expr.Assign(ScopedVal, Expr.Multiply(ScopedVal, Expr.Constant(2))),
                        EU.GenAreEqual(ScopedVal, Expr.Constant(8)),
                        ScopedVal
                    )
                );

            var label = Expr.Label(Scope.Type);
            Expr Body = Expr.Block(EU.ConcatEquals(Result, "Return"), Expr.Return(label, Scope), EU.ConcatEquals(Result, "Bad"), Expr.Default(label.Type));
            Expr LM = Expr.Lambda<Func<int>>(Expr.Label(label, Body));
            Expr InvokeLM = Expr.Invoke(LM);

            Expressions.Add(InvokeLM);

            Expressions.Add(EU.GenAreEqual(Expr.Constant("ReturnScoped"), Result));

			var tree = EU.BlockVoid(new [] { Result }, Expressions);
			V.Validate(tree);
			return tree;
        }
    }
}
