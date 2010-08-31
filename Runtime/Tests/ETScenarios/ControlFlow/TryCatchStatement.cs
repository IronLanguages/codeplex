#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Utils;
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Scripting.Ast;

namespace ETScenarios.ControlFlow {
    using EU = ETUtils.ExpressionUtils;
    using Expr = Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public class TryCatchStatement {

        private static Expr TE(Type t, string Message) {
            ConstructorInfo ci = t.GetConstructor(new Type[] { typeof(String) });
            Expression Ex = Expr.New(ci, Expr.Constant(Message));
            return Ex;
        }
        //TryCatchFinally(Expression body, CatchBlock[] handlers, expression finally, Expression fault)
        //Create a simple try catch using this method
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Try 1", new string[] { "positive", "try", "controlflow", "Pri1" })]
        public static Expr Try1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            //two catches, one finally.

            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expr Body = EU.BlockVoid(EU.ConcatEquals(Result, "Try1"), Expr.Throw(TE(typeof(DivideByZeroException), "Test")), EU.ConcatEquals(Result, "Try2"));


            Expr HandlerBody0 = EU.BlockVoid(EU.ConcatEquals(Result, "Handler0"));
            CatchBlock Handler0 = Expr.Catch(typeof(AccessViolationException), HandlerBody0);

            Expr HandlerBody1 = EU.BlockVoid(EU.ConcatEquals(Result, "Handler1"));
            CatchBlock Handler1 = Expr.Catch(typeof(DivideByZeroException), HandlerBody1);

            Expr HandlerBody2 = EU.BlockVoid(EU.ConcatEquals(Result, "Handler2"));
            CatchBlock Handler2 = Expr.Catch(typeof(Exception), HandlerBody2);

            Expr FinallyBody = EU.ConcatEquals(Result, "Finally");

            TryExpression MyTry = Expr.TryCatchFinally(Body, FinallyBody, Handler0, Handler1, Handler2);

            Expressions.Add(MyTry);

            Expressions.Add(EU.GenAreEqual(Expr.Constant("Try1Handler1Finally"), Result, "Try 1"));

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //Multiple handlers, with the same type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Try 2", new string[] { "positive", "try", "controlflow", "Pri1" })]
        public static Expr Try2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            //two catches, one finally.multiple catches with the same type.

            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expr Body = EU.BlockVoid(EU.ConcatEquals(Result, "Try1"), Expr.Throw(TE(typeof(DivideByZeroException), "Test")), EU.ConcatEquals(Result, "Try2"));


            Expr HandlerBody0 = EU.BlockVoid(EU.ConcatEquals(Result, "Handler0"));
            CatchBlock Handler0 = Expr.Catch(typeof(AccessViolationException), HandlerBody0);

            Expr HandlerBody1 = EU.BlockVoid(EU.ConcatEquals(Result, "Handler1"));
            CatchBlock Handler1 = Expr.Catch(typeof(DivideByZeroException), HandlerBody1);

            Expr HandlerBody1_1 = EU.BlockVoid(EU.ConcatEquals(Result, "Handler1 1/2"));
            CatchBlock Handler1_1 = Expr.Catch(typeof(DivideByZeroException), HandlerBody1_1);

            Expr HandlerBody2 = EU.BlockVoid(EU.ConcatEquals(Result, "Handler2"));
            CatchBlock Handler2 = Expr.Catch(typeof(Exception), HandlerBody2);

            Expr FinallyBody = EU.ConcatEquals(Result, "Finally");

            TryExpression MyTry = Expr.TryCatchFinally(Body, FinallyBody, Handler0, Handler0, Handler1, Handler1_1, Handler1, Handler2, Handler2);

            Expressions.Add(MyTry);

            Expressions.Add(EU.GenAreEqual(Expr.Constant("Try1Handler1Finally"), Result, "Try 2"));

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //Multiple handlers, with a derived type after a base type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Try 3", new string[] { "positive", "try", "controlflow", "Pri1" })]
        public static Expr Try3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();


            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expr Body = EU.BlockVoid(EU.ConcatEquals(Result, "Try1"), Expr.Throw(TE(typeof(DivideByZeroException), "Test")), EU.ConcatEquals(Result, "Try2"));


            Expr HandlerBody1 = EU.BlockVoid(EU.ConcatEquals(Result, "Handler1"));
            CatchBlock Handler1 = Expr.Catch(typeof(Exception), HandlerBody1);

            Expr HandlerBody2 = EU.BlockVoid(EU.ConcatEquals(Result, "Handler2"));
            CatchBlock Handler2 = Expr.Catch(typeof(DivideByZeroException), HandlerBody2);



            Expr FinallyBody = EU.ConcatEquals(Result, "Finally");

            TryExpression MyTry = Expr.TryCatchFinally(Body, FinallyBody, Handler1, Handler2);

            Expressions.Add(MyTry);

            Expressions.Add(EU.GenAreEqual(Expr.Constant("Try1Handler1Finally"), Result, "Try 1"));

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //Handler with a non exception type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Try 4", new string[] { "positive", "try", "controlflow", "Pri1" })]
        public static Expr Try4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();


            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expr Body = EU.BlockVoid(EU.ConcatEquals(Result, "Try1"), Expr.Throw(TE(typeof(DivideByZeroException), "Test")), EU.ConcatEquals(Result, "Try2"));

            Expr HandlerBody1 = EU.BlockVoid(EU.ConcatEquals(Result, "Handler1"));
            CatchBlock Handler1 = Expr.Catch(typeof(Int32), HandlerBody1);

            Expr HandlerBody2 = EU.BlockVoid(EU.ConcatEquals(Result, "Handler2"));
            CatchBlock Handler2 = Expr.Catch(typeof(Exception), HandlerBody2);



            Expr FinallyBody = EU.ConcatEquals(Result, "Finally");

            TryExpression MyTry = Expr.TryCatchFinally(Body, FinallyBody, Handler1, Handler2);

            Expressions.Add(MyTry);

            Expressions.Add(EU.GenAreEqual(Expr.Constant("Try1Handler2Finally"), Result, "Try 1"));

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //Handler with a non exception type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Try 5", new string[] { "positive", "try", "controlflow", "Pri1" })]
        public static Expr Try5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();


            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expr Body = EU.BlockVoid(EU.ConcatEquals(Result, "Try1"), Expr.Throw(TE(typeof(DivideByZeroException), "Test")), EU.ConcatEquals(Result, "Try2"));


            Expr HandlerBody1 = EU.BlockVoid(EU.ConcatEquals(Result, "Handler1"));
            CatchBlock Handler1 = Expr.Catch(typeof(Object), HandlerBody1);

            Expr HandlerBody2 = EU.BlockVoid(EU.ConcatEquals(Result, "Handler2"));
            CatchBlock Handler2 = Expr.Catch(typeof(Exception), HandlerBody2);



            Expr FinallyBody = EU.ConcatEquals(Result, "Finally");

            TryExpression MyTry = Expr.TryCatchFinally(Body, FinallyBody, Handler1, Handler2);

            Expressions.Add(MyTry);

            Expressions.Add(EU.GenAreEqual(Expr.Constant("Try1Handler1Finally"), Result, "Try 1"));

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //Throw an exception from a fault statement
        [ETUtils.TestAttribute(ETUtils.TestState.Disabled, "Try 6", new string[] { "positive", "try", "controlflow", "Pri1" })]
        public static Expr Try6(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expr Body = EU.BlockVoid(EU.ConcatEquals(Result, "Try1"), Expr.Throw(TE(typeof(DivideByZeroException), "Test")), EU.ConcatEquals(Result, "Try2"));

            Expr HandlerBody1 = EU.ConcatEquals(Result, "Handler1");
            CatchBlock Handler1 = Expr.Catch(typeof(ArgumentException), HandlerBody1);

            Expr HandlerBody2 = EU.ConcatEquals(Result, "Handler2");
            CatchBlock Handler2 = Expr.Catch(typeof(NullReferenceException), HandlerBody2);

            Expr FaultBody = EU.ConcatEquals(Result, "Fault");

            TryExpression MyTry = Expr.TryFault(Body, FaultBody/*, Handler1, Handler2*/);

            //wrap in try catch.
            Expressions.Add(Expr.TryCatch(MyTry, Expr.Catch(typeof(DivideByZeroException), Expr.Empty())));

            Expressions.Add(EU.GenAreEqual(Expr.Constant("Try1Fault"), Result, "Try 1"));

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.Validate(tree, true);
            return tree;
        }

        //Throw an exception from a finally statement
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Try 7", new string[] { "positive", "try", "controlflow", "Pri1" })]
        public static Expr Try7(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expr Body = EU.BlockVoid(EU.ConcatEquals(Result, "Try"));

            Expr HandlerBody1 = EU.BlockVoid(EU.ConcatEquals(Result, "Handler1"));
            CatchBlock Handler1 = Expr.Catch(typeof(ArgumentException), HandlerBody1);

            Expr HandlerBody2 = EU.BlockVoid(EU.ConcatEquals(Result, "Handler2"));
            CatchBlock Handler2 = Expr.Catch(typeof(Exception), HandlerBody2);

            Expr FinallyBody = EU.BlockVoid(EU.ConcatEquals(Result, "Finally1"), Expr.Throw(TE(typeof(DivideByZeroException), "Test")), EU.ConcatEquals(Result, "Finally2"));

            TryExpression MyTry = Expr.TryCatchFinally(Body, FinallyBody, Handler1, Handler2);

            //wrap in try catch.
            Expressions.Add(Expr.TryCatch(MyTry, Expr.Catch(typeof(DivideByZeroException), Expr.Empty())));

            Expressions.Add(EU.GenAreEqual(Expr.Constant("TryFinally1"), Result, "Try 1"));

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //Throw an exception from a condition statement
        [ETUtils.TestAttribute(ETUtils.TestState.Disabled, "Try 8", new string[] { "positive", "try", "controlflow", "Pri1" })]
        public static Expr Try8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();


            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression Ex = Expr.Variable(typeof(Exception), "");

            Expr Body = EU.BlockVoid(EU.ConcatEquals(Result, "Try1"), Expr.Throw(TE(typeof(DivideByZeroException), "Test")), EU.ConcatEquals(Result, "Try2"));

            Expr HandlerBody1 = EU.ConcatEquals(Result, "Handler1");
            CatchBlock Handler1 = Expr.Catch(typeof(ArgumentException), HandlerBody1);

            Expr HandlerBody2 = EU.ConcatEquals(Result, "Handler2");
            Expr Handler2Filter = Expr.Block(
                                                EU.ConcatEquals(Result, "Filter2_1"),
                                                Expr.Throw(TE(typeof(ObjectDisposedException), "Test2")),
                                                EU.ConcatEquals(Result, "Filter2_2"),
                                                Expr.Constant(true)
                                               );
            CatchBlock Handler2 = Expr.Catch(Ex, HandlerBody2, Handler2Filter);

            Expr FinallyBody = EU.BlockVoid(EU.ConcatEquals(Result, "Finally"));

            TryExpression MyTry = Expr.TryCatchFinally(Body, FinallyBody, Handler1, Handler2);

            //wrap in try catch.
            Expressions.Add(Expr.TryCatch(MyTry, Expr.Catch(typeof(ObjectDisposedException), Expr.Empty())));

            Expressions.Add(EU.GenAreEqual(Expr.Constant("Try1Filter2_1Finally"), Result, "Try 1"));

            var tree = EU.BlockVoid(new[] { Result, Ex }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //Pass a try statement as an argument to an addition operation.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Try 9", new string[] { "negative", "try", "controlflow", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Try9(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();


            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expr Body = EU.BlockVoid(EU.ConcatEquals(Result, "Try"));

            Expr HandlerBody1 = EU.BlockVoid(EU.ConcatEquals(Result, "Handler1"));
            CatchBlock Handler1 = Expr.Catch(typeof(DivideByZeroException), HandlerBody1);

            Expr HandlerBody2 = EU.BlockVoid(EU.ConcatEquals(Result, "Handler2"));
            CatchBlock Handler2 = Expr.Catch(typeof(Exception), HandlerBody2);

            Expr FinallyBody = EU.BlockVoid(EU.ConcatEquals(Result, "Finally1"), Expr.Throw(TE(typeof(DivideByZeroException), "Test")), EU.ConcatEquals(Result, "Finally2"));

            TryExpression MyTry = Expr.TryCatchFinally(Body, FinallyBody, Handler1, Handler2);


            //use in an addition operation.
            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.Add(MyTry, Expr.Constant(5));
            }));

            return Expr.Empty();
        }

        //Try statement with no no main block, but with a catch.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Try 10", new string[] { "positive", "try", "controlflow", "Pri1" })]
        public static Expr Try10(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();


            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expr Body = Expr.Empty(); //null; // Expr.Block(EU.ConcatEquals(Result, "Try"));

            Expr HandlerBody1 = EU.BlockVoid(EU.ConcatEquals(Result, "Handler1"));
            CatchBlock Handler1 = Expr.Catch(typeof(ArgumentException), HandlerBody1);

            Expr HandlerBody2 = EU.BlockVoid(EU.ConcatEquals(Result, "Handler2"));
            CatchBlock Handler2 = Expr.Catch(typeof(Exception), HandlerBody2);

            Expr FinallyBody = EU.BlockVoid(EU.ConcatEquals(Result, "Finally"));

            TryExpression MyTry = Expr.TryCatchFinally(Body, FinallyBody, Handler1, Handler2);

            //wrap in try catch.
            Expressions.Add(Expr.TryCatch(MyTry, Expr.Catch(typeof(ObjectDisposedException), Expr.Empty())));

            Expressions.Add(EU.GenAreEqual(Expr.Constant("Finally"), Result, "Try 1"));

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }


        //Multiple Catches with filters, checking execution order
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Try 11", new string[] { "positive", "try", "controlflow", "Pri1" })]
        public static Expr Try11(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expr Body = EU.BlockVoid(EU.ConcatEquals(Result, "Try1"), Expr.Throw(TE(typeof(DivideByZeroException), "Test")), EU.ConcatEquals(Result, "Try2"));

            Expr HandlerBody1 = EU.BlockVoid(EU.ConcatEquals(Result, "Handler1"));
            Expr Handler1Filter = Expr.Block(EU.ConcatEquals(Result, "Filter1"), Expr.Constant(true));
            CatchBlock Handler1 = Expr.Catch(typeof(ArgumentException), HandlerBody1);

            Expr HandlerBody2 = EU.BlockVoid(EU.ConcatEquals(Result, "Handler2"));
            Expr Handler2Filter = Expr.Block(EU.ConcatEquals(Result, "Filter2"), Expr.Constant(true));
            CatchBlock Handler2 = Expr.Catch(typeof(DivideByZeroException), HandlerBody2, Handler2Filter);

            Expr HandlerBody3 = EU.BlockVoid(EU.ConcatEquals(Result, "Handler3"));
            Expr Handler3Filter = Expr.Block(EU.ConcatEquals(Result, "Filter3"), Expr.Constant(true));
            CatchBlock Handler3 = Expr.Catch(typeof(Exception), HandlerBody3, Handler3Filter);

            Expr FinallyBody = EU.BlockVoid(EU.ConcatEquals(Result, "Finally"));

            TryExpression MyTry = Expr.TryCatchFinally(Body, FinallyBody, Handler1, Handler2, Handler3);

            //wrap in try catch.
            Expressions.Add(Expr.TryCatch(MyTry, Expr.Catch(typeof(ObjectDisposedException), Expr.Empty())));

            Body = EU.BlockVoid(EU.ConcatEquals(Result, "Try3"), Expr.Throw(TE(typeof(DivideByZeroException), "Test")), EU.ConcatEquals(Result, "Try4"));

            HandlerBody1 = EU.BlockVoid(EU.ConcatEquals(Result, "Handler4"));
            Handler1Filter = Expr.Block(EU.ConcatEquals(Result, "Filter4"), Expr.Constant(true));
            Handler1 = Expr.Catch(typeof(ArgumentException), HandlerBody1);

            HandlerBody2 = EU.BlockVoid(EU.ConcatEquals(Result, "Handler5"));
            Handler2Filter = Expr.Block(EU.ConcatEquals(Result, "Filter5"), Expr.Constant(false));
            Handler2 = Expr.Catch(typeof(DivideByZeroException), HandlerBody2, Handler2Filter);

            HandlerBody3 = EU.BlockVoid(EU.ConcatEquals(Result, "Handler6"));
            Handler3Filter = Expr.Block(EU.ConcatEquals(Result, "Filter6"), Expr.Constant(true));
            Handler3 = Expr.Catch(typeof(Exception), HandlerBody3, Handler3Filter);

            FinallyBody = EU.BlockVoid(EU.ConcatEquals(Result, "Finally"));

            MyTry = Expr.TryCatchFinally(Body, FinallyBody, Handler1, Handler2, Handler3);

            //wrap in try catch.
            Expressions.Add(Expr.TryCatch(MyTry, Expr.Catch(typeof(ObjectDisposedException), Expr.Empty())));

            Expressions.Add(EU.GenAreEqual(Expr.Constant("Try1Filter2Handler2FinallyTry3Filter5Filter6Handler6Finally"), Result, "Try 1"));
        
            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.Validate(tree, true);
            return tree;
        }

        //Create an If with a jump condition from the finally statement.
        //Expect Result to append: "Try1Handler2Finally1" to the ResultVariable.
        private static Expr JumpFromFinally(Expression JumpExpression, ParameterExpression ResultVariable) {
            Expr Body = EU.BlockVoid(EU.ConcatEquals(ResultVariable, "Try1"), Expr.Throw(TE(typeof(DivideByZeroException), "Test")), EU.ConcatEquals(ResultVariable, "Try2"));

            Expr HandlerBody1 = EU.BlockVoid(EU.ConcatEquals(ResultVariable, "Handler1"));
            CatchBlock Handler1 = Expr.Catch(typeof(ArgumentException), HandlerBody1);

            Expr HandlerBody2 = EU.BlockVoid(EU.ConcatEquals(ResultVariable, "Handler2"));
            CatchBlock Handler2 = Expr.Catch(typeof(Exception), HandlerBody2);

            Expr FinallyBody = EU.BlockVoid(EU.ConcatEquals(ResultVariable, "Finally1"), JumpExpression, EU.ConcatEquals(ResultVariable, "Finally1"));

            TryExpression MyTry = Expr.TryCatchFinally(Body, FinallyBody, Handler1, Handler2);

            return MyTry;
        }

        //Continue from a finally statement
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Try 12", new string[] { "negative", "try", "loops", "controlflow", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Try12(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();


            ParameterExpression Result = Expr.Variable(typeof(string), "");

            ParameterExpression Test = Expr.Variable(typeof(Boolean), "");

            Expressions.Add(Expr.Assign(Test, Expr.Constant(true)));

            LabelTarget Label = Expr.Label();

            Expr Body = EU.BlockVoid(EU.ConcatEquals(Result, "Loop1"), Expr.Assign(Test, Expr.Constant(false)), JumpFromFinally(Expr.Continue(Label), Result), EU.ConcatEquals(Result, "Loop2"));

            //create a loop.
            Expressions.Add(AstUtils.Loop(Test, Expr.Empty(), Body, null, null, Label));


            Expressions.Add(EU.GenAreEqual(Expr.Constant("Loop1Try1Handler2Finally1"), Result, "Try 1"));

            var tree = EU.BlockVoid(new[] { Result, Test }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true);
            return tree;
        }

        //Break from a finally statement
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Try 13", new string[] { "negative", "try", "loop", "controlflow", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Try13(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();


            ParameterExpression Result = Expr.Variable(typeof(string), "");

            ParameterExpression Test = Expr.Variable(typeof(Boolean), "");

            Expressions.Add(Expr.Assign(Test, Expr.Constant(true)));

            LabelTarget Label = Expr.Label();

            Expr Body = EU.BlockVoid(EU.ConcatEquals(Result, "Loop1"), JumpFromFinally(Expr.Break(Label), Result), EU.ConcatEquals(Result, "Loop2"));

            //create a loop.
            Expressions.Add(AstUtils.Loop(Test, Expr.Empty(), Body, null, Label, null));


            Expressions.Add(EU.GenAreEqual(Expr.Constant("Loop1Try1Handler2Finally1"), Result, "Try 1"));

            var tree = EU.BlockVoid(new[] { Result, Test }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true);
            return tree;
        }


        //TryCatchFinally(Expression Body, CatchBlock[] Handlers, Expression Finally, Expression Fault)
        //Pass a null to fault
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Try 14", new string[] { "positive", "try", "controlflow", "Pri1" })]
        public static Expr Try14(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();


            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression Ex = Expr.Variable(typeof(Exception), "");

            Expr Body = EU.BlockVoid(EU.ConcatEquals(Result, "Try1"), Expr.Throw(TE(typeof(DivideByZeroException), "Test")), EU.ConcatEquals(Result, "Try2"));

            Expr HandlerBody1 = EU.BlockVoid(EU.ConcatEquals(Result, "Handler1"));
            CatchBlock Handler1 = Expr.Catch(typeof(ArgumentException), HandlerBody1);

            Expr HandlerBody2 = EU.BlockVoid(EU.ConcatEquals(Result, "Handler2"));
            CatchBlock Handler2 = Expr.Catch(typeof(Exception), HandlerBody2);


            TryExpression MyTry = Expr.TryCatch(Body, Handler1, Handler2);

            //wrap in try catch.
            Expressions.Add(Expr.TryCatch(MyTry, Expr.Catch(typeof(ObjectDisposedException), Expr.Empty())));

            Expressions.Add(EU.GenAreEqual(Expr.Constant("Try1Handler2"), Result, "Try 1"));

            var tree = EU.BlockVoid(new[] { Result, Ex }, Expressions);
            V.Validate(tree);
            return tree;
        }


        //TryStatementBuilder.Catch(Type type, ParameterExpression Holder, Expression Body)
        //TryStatementBuilder.Catch(Type type, ParameterExpression Holder, Expression[] Body)
        //Passing a variable that isn't defined in a containing scope
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Try 15", new string[] { "positive", "try", "controlflow", "Pri1" })]
        public static Expr Try15(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();


            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression Ex = Expr.Variable(typeof(Exception), "");

            Expr Body = EU.BlockVoid(EU.ConcatEquals(Result, "Try1"), Expr.Throw(TE(typeof(DivideByZeroException), "Test")), EU.ConcatEquals(Result, "Try2"));

            TryStatementBuilder MyTry = AstUtils.Try(Body);

            Expr HandlerBody1 = EU.BlockVoid(EU.ConcatEquals(Result, "Handler1"));
            MyTry = MyTry.Catch(typeof(ArgumentException), HandlerBody1);

            Expr HandlerBody2 = EU.BlockVoid(EU.ConcatEquals(Result, "Handler2"));
            MyTry = MyTry.Catch(Ex, HandlerBody2);

            //wrap in try catch.
            Expressions.Add(Expr.TryCatch(MyTry.Finally(Expr.Empty()), Expr.Catch(typeof(ObjectDisposedException), Expr.Empty())));

            Expressions.Add(EU.GenAreEqual(Expr.Constant("Try1Handler2"), Result, "Try 1"));

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //TryStatementBuilder.Catch(Type type, ParameterExpression Holder, Expression Body)
        //TryStatementBuilder.Catch(Type type, ParameterExpression Holder, Expression[] Body)
        //Passing a variable that isn't defined in a containing scope
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Try 16", new string[] { "negative", "try", "controlflow", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Try16(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression Ex = Expr.Variable(typeof(Exception), "");

            Expr Body = EU.BlockVoid(EU.ConcatEquals(Result, "Try1"), Expr.Throw(TE(typeof(DivideByZeroException), "Test")), EU.ConcatEquals(Result, "Try2"));

            TryStatementBuilder MyTry = AstUtils.Try(Body);

            Expr HandlerBody1 = EU.ConcatEquals(Result, "Handler1");
            MyTry = MyTry.Catch(typeof(ArgumentException), HandlerBody1, Expr.Empty());

            Expr HandlerBody2 = EU.ConcatEquals(Result, "Handler2");
            MyTry = MyTry.Catch(typeof(Exception), Ex, HandlerBody2, Expr.Empty());

            //wrap in try catch.
            Expressions.Add(Expr.TryCatch(MyTry.Finally(Expr.Empty()), Expr.Catch(typeof(ObjectDisposedException), Expr.Empty())));

            Expressions.Add(EU.GenAreEqual(Expr.Constant("Try1Handler2"), Result, "Try 1"));

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true);
            return tree;
        }

        //TryStatementBuilder.Catch(ParameterExpression Holder, Expression Body)
        //TryStatementBuilder.Catch(ParameterExpression Holder, Expression[] Body)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Try 17", new string[] { "positive", "try", "controlflow", "Pri1" })]
        public static Expr Try17(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expr Body = EU.BlockVoid(EU.ConcatEquals(Result, "Try1"), Expr.Throw(TE(typeof(DivideByZeroException), "Test")), EU.ConcatEquals(Result, "Try2"));


            TryStatementBuilder MyTry = AstUtils.Try(Body);

            Expr HandlerBody1 = EU.ConcatEquals(Result, "Handler1");
            MyTry = MyTry.Catch(typeof(ArgumentException), HandlerBody1, Expr.Empty());

            Expr HandlerBody2 = EU.ConcatEquals(Result, "Handler2");
            MyTry = MyTry.Catch(typeof(Exception), HandlerBody2, Expr.Empty());

            //wrap in try catch.
            Expressions.Add(Expr.TryCatch(MyTry.Finally(Expr.Empty()), Expr.Catch(typeof(ObjectDisposedException), Expr.Empty())));

            Expressions.Add(EU.GenAreEqual(Expr.Constant("Try1Handler2"), Result, "Try 1"));

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //TryStatementBuilder.Catch(Type type, Expression Body)
        //TryStatementBuilder.Catch(Type type, Expression[] Body)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Try 18", new string[] { "positive", "try", "controlflow", "Pri1" })]
        public static Expr Try18(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expr Body = EU.BlockVoid(EU.ConcatEquals(Result, "Try1"), Expr.Throw(TE(typeof(DivideByZeroException), "Test")), EU.ConcatEquals(Result, "Try2"));


            TryStatementBuilder MyTry = AstUtils.Try(Body);

            Expr HandlerBody1 = EU.ConcatEquals(Result, "Handler1");
            MyTry = MyTry.Catch(typeof(ArgumentException), HandlerBody1, Expr.Empty());

            Expr HandlerBody2 = EU.ConcatEquals(Result, "Handler2");
            MyTry = MyTry.Catch(typeof(Exception), HandlerBody2, Expr.Empty());

            //wrap in try catch.
            Expressions.Add(Expr.TryCatch(MyTry.Finally(Expr.Empty()), Expr.Catch(typeof(ObjectDisposedException), Expr.Empty())));

            Expressions.Add(EU.GenAreEqual(Expr.Constant("Try1Handler2"), Result, "Try 1"));

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //TryStatementBuilder.Catch(ParameterExpression Holder, Expression Body)
        //TryStatementBuilder.Catch(ParameterExpression Holder, Expression[] Body)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Try 19", new string[] { "positive", "try", "controlflow", "Pri1" })]
        public static Expr Try19(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expr Body = EU.BlockVoid(EU.ConcatEquals(Result, "Try1"), Expr.Throw(TE(typeof(DivideByZeroException), "Test")), EU.ConcatEquals(Result, "Try2"));


            TryStatementBuilder MyTry = AstUtils.Try(Body);

            Expr HandlerBody1 = EU.ConcatEquals(Result, "Handler1");
            MyTry = MyTry.Catch(typeof(ArgumentException), HandlerBody1, Expr.Empty());

            Expr HandlerBody2 = EU.ConcatEquals(Result, "Handler2");
            MyTry = MyTry.Catch(typeof(Exception), HandlerBody2, Expr.Empty());

            //wrap in try catch.
            Expressions.Add(Expr.TryCatch(MyTry.Finally(Expr.Empty()), Expr.Catch(typeof(ObjectDisposedException), Expr.Empty())));

            Expressions.Add(EU.GenAreEqual(Expr.Constant("Try1Handler2"), Result, "Try 1"));

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //TryStatementBuilder.Catch(Type type, ParameterExpression Holder, Expression Body)
        //TryStatementBuilder.Catch(Type type, ParameterExpression Holder, Expression[] Body)
        //Passing a variable that already has a value
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Try 20", new string[] { "positive", "try", "controlflow", "Pri1" })]
        public static Expr Try20(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression Ex = Expr.Variable(typeof(Exception), "");


            Expr Body = EU.BlockVoid(EU.ConcatEquals(Result, "Try1"), Expr.Throw(TE(typeof(DivideByZeroException), "Test")), EU.ConcatEquals(Result, "Try2"));

            Expressions.Add(Expr.Assign(Ex, TE(typeof(Exception), "TestMsg")));

            TryStatementBuilder MyTry = AstUtils.Try(Body);

            Expr HandlerBody1 = EU.ConcatEquals(Result, "Handler1");
            MyTry = MyTry.Catch(typeof(ArgumentException), HandlerBody1, Expr.Empty());

            Expr HandlerBody2 = EU.ConcatEquals(Result, "Handler2");
            MyTry = MyTry.Catch(Ex, HandlerBody2, Expr.Empty());

            //wrap in try catch.
            Expressions.Add(Expr.TryCatch(MyTry.Finally(Expr.Empty()), Expr.Catch(typeof(ObjectDisposedException), Expr.Empty())));

            Expressions.Add(EU.GenAreEqual(Expr.Constant("Try1Handler2"), Result, "Try 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("TestMsg"), Expr.Property(Ex, typeof(Exception).GetProperty("Message")), "Try 1"));

            var tree = EU.BlockVoid(new[] { Result, Ex }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //SkipIf(true) before each other trystatementbuilder method
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Try 21", new string[] { "positive", "try", "controlflow", "Pri1" })]
        public static Expr Try21(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression Ex = Expr.Variable(typeof(Exception), "");


            Expr Body = EU.BlockVoid(EU.ConcatEquals(Result, "Try1"), Expr.Throw(TE(typeof(DivideByZeroException), "Test")), EU.ConcatEquals(Result, "Try2"));

            Expressions.Add(Expr.Assign(Ex, TE(typeof(Exception), "TestMsg")));

            TryStatementBuilder MyTry = AstUtils.Try(Body);

            Expr HandlerBody1 = EU.ConcatEquals(Result, "Handler1");
            MyTry = MyTry.Catch(typeof(ArgumentException), HandlerBody1, Expr.Empty());

            Expr HandlerBody2 = EU.BlockVoid(EU.ConcatEquals(Result, "Handler2_1"));
            Expr HandlerBody2_2 = EU.BlockVoid(EU.ConcatEquals(Result, "Handler2_2"));
            MyTry = MyTry.Catch(Ex, HandlerBody2, Expr.Rethrow(), HandlerBody2_2);

            Expr Body2 = EU.BlockVoid(EU.ConcatEquals(Result, "Try3"), Expr.Throw(TE(typeof(DivideByZeroException), "Test")), EU.ConcatEquals(Result, "Try4"));

            //wrap in try catch.
            Expression MyTry2 = Expr.TryCatch(Body2, Expr.Catch(typeof(DivideByZeroException), MyTry.Finally(Expr.Empty())));
            Expressions.Add(Expr.TryCatch(MyTry2, Expr.Catch(typeof(DivideByZeroException), EU.BlockVoid(EU.ConcatEquals(Result, "Caught!")))));

            Expressions.Add(EU.GenAreEqual(Expr.Constant("Try3Try1Handler2_1Caught!"), Result, "Try 1"));

            var tree = EU.BlockVoid(new[] { Result, Ex }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // pass a null to the expression[] argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Try 22", new string[] { "negative", "try", "controlflow", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr Try22(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "");

            var MyTry =
                EU.Throws<System.ArgumentNullException>(() =>
                {
                    AstUtils.Try((Expression)null);
                });

            return Expr.Empty();
        }

        //Throw an exception from a fault statement
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Try 22_1", new string[] { "positive", "try", "controlflow", "Pri2" })]
        public static Expr Try22_1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expr Body = EU.BlockVoid(EU.ConcatEquals(Result, "Try1"), Expr.Throw(TE(typeof(DivideByZeroException), "Test")), EU.ConcatEquals(Result, "Try2"));

            Expr HandlerBody1 = EU.ConcatEquals(Result, "Handler1");
            CatchBlock Handler1 = Expr.Catch(typeof(ArgumentException), HandlerBody1);

            Expr HandlerBody2 = EU.ConcatEquals(Result, "Handler2");
            CatchBlock Handler2 = Expr.Catch(typeof(NullReferenceException), HandlerBody2);

            Expr FaultBody = Expr.Block(EU.ConcatEquals(Result, "Fault"), Expr.Throw(TE(typeof(MulticastNotSupportedException), "Test2")));

            TryExpression MyTry = Expr.TryFault(Body, FaultBody);

            //wrap in try catch.
            Expressions.Add(Expr.TryCatch(MyTry, Expr.Catch(typeof(MulticastNotSupportedException), EU.BlockVoid(EU.ConcatEquals(Result, "Catch1"))), Expr.Catch(typeof(DivideByZeroException), EU.BlockVoid(EU.ConcatEquals(Result, "Catch2")))));

            Expressions.Add(EU.GenAreEqual(Expr.Constant("Try1FaultCatch1"), Result, "Try 1"));

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.Validate(tree, true);
            return tree;
        }

        // pass an empty array to the expression[] argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Try 23", new string[] { "negative", "try", "controlflow", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Try23(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expressions.Add(Expr.Assign(Result, Expr.Constant("")));

            var MyTry =
                EU.Throws<System.ArgumentException>(() =>
                {
                    AstUtils.Try(new Expression[] { });
                });

            return Expr.Empty();
        }

        // pass scope wrapped expressions to the body
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Try 24", new string[] { "positive", "try", "controlflow", "Pri2" })]
        public static Expr Try24(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression TestValue = Expr.Variable(typeof(Int32), "");

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(3)));

            ParameterExpression Scoped1 = Expr.Variable(typeof(Int32), "");
            ParameterExpression Scoped2 = Expr.Variable(typeof(Int32), "");

            Expr Body1 =
                    EU.BlockVoid(
                        new[] { Scoped1 },
                        Expr.Assign(Scoped1, Expr.Constant(1)),
                        Expr.Assign(TestValue, Expr.Add(Scoped1, TestValue)),
                        EU.ConcatEquals(Result, "Body1")
                    );

            Expr Body2 =
                    EU.BlockVoid(
                        new[] { Scoped2 },
                        Expr.Assign(Scoped2, Expr.Constant(2)),
                        Expr.Assign(TestValue, Expr.Add(Scoped2, TestValue)),
                        EU.ConcatEquals(Result, "Body2")
                    );

            Expr Body3 = EU.BlockVoid(EU.ConcatEquals(Result, "Body3"), Expr.Throw(TE(typeof(DivideByZeroException), "TestException")));

            TryStatementBuilder MyTry = AstUtils.Try(Body1, Body2, Body3);

            Expr Handler1 = EU.BlockVoid(EU.ConcatEquals(Result, "Handler1"));
            MyTry = MyTry.Catch(typeof(DivideByZeroException), Handler1);

            Expressions.Add(MyTry);

            Expressions.Add(EU.GenAreEqual(Expr.Constant("Body1Body2Body3Handler1"), Result));
            Expressions.Add(EU.GenAreEqual(TestValue, Expr.Constant(6)));

            var tree = EU.BlockVoid(new[] { Result, TestValue }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // pass an empty array to the handlers
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Try 25", new string[] { "positive", "try", "controlflow", "Pri2" })]
        public static Expr Try25(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expr Body = EU.BlockVoid(EU.ConcatEquals(Result, "Body"));

            Expr FinallyBody = EU.BlockVoid(EU.ConcatEquals(Result, "Finally"));

            // case 1 - empty handlers, no exception thrown
            TryExpression MyTry = Expr.TryCatchFinally(Body, FinallyBody, new CatchBlock[] { });

            Expressions.Add(MyTry);

            Expressions.Add(EU.GenAreEqual(Expr.Constant("BodyFinally"), Result));

            // case 2 - empty handlers, exception thrown
            Expressions.Add(Expr.Assign(Result, Expr.Constant("")));

            Expr Body2 = EU.BlockVoid(EU.ConcatEquals(Result, "Body1"), Expr.Throw(TE(typeof(DivideByZeroException), "TestException")), EU.ConcatEquals(Result, "Body2"));

            Expr MyTry2 = Expr.TryCatchFinally(Body2, FinallyBody, new CatchBlock[] { });

            Expressions.Add(Expr.TryCatch(MyTry2, Expr.Catch(typeof(DivideByZeroException), EU.BlockVoid(EU.ConcatEquals(Result, "Caught")))));

            Expressions.Add(EU.GenAreEqual(Expr.Constant("Body1FinallyCaught"), Result));

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // pass an array with a null element to the handlers
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Try 26", new string[] { "negative", "try", "controlflow", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr Try26(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expr Body = EU.BlockVoid(EU.ConcatEquals(Result, "Body"), Expr.Throw(TE(typeof(DivideByZeroException), "TestException")));

            Expr Handler1 = EU.BlockVoid(EU.ConcatEquals(Result, "Handler1"));
            CatchBlock CatchBlock1 = Expr.Catch(typeof(DivideByZeroException), Handler1);

            Expr FinallyBody = EU.BlockVoid(EU.ConcatEquals(Result, "Finally"));

            var MyTry = EU.Throws<System.ArgumentNullException>(() =>
            {
                Expr.TryCatchFinally(Body, FinallyBody, new CatchBlock[] { CatchBlock1, null });
            });

            Expressions.Add(MyTry);

            return EU.BlockVoid(new[] { Result }, Expressions);
        }

        // pass a value returning expression to fault
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Try 27", new string[] { "positive", "try", "controlflow", "Pri2" })]
        public static Expr Try27(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expr Body = EU.BlockVoid(EU.ConcatEquals(Result, "Body"), Expr.Throw(TE(typeof(DivideByZeroException), "TestException")));

            Expr Handler1 = EU.BlockVoid(EU.ConcatEquals(Result, "Handler1"));
            CatchBlock CatchBlock1 = Expr.Catch(typeof(DivideByZeroException), Handler1);

            Expr FinallyBody = Expr.Block(EU.ConcatEquals(Result, "Finally"), Expr.Add(Expr.Constant(4), Expr.Constant(2)));

            TryExpression MyTry = Expr.TryCatchFinally(Body, FinallyBody, new CatchBlock[] { CatchBlock1 });

            Expressions.Add(MyTry);

            Expressions.Add(EU.GenAreEqual(Expr.Constant("BodyHandler1Finally"), Result, "Try1"));

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // pass a scope wrapped expression to finally
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Try 28", new string[] { "positive", "try", "controlflow", "Pri2" })]
        public static Expr Try28(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression TestValue = Expr.Variable(typeof(Int32), "");

            Expr Body = EU.BlockVoid(EU.ConcatEquals(Result, "Body"), Expr.Throw(TE(typeof(DivideByZeroException), "TestException")));

            Expr Handler1 = EU.BlockVoid(EU.ConcatEquals(Result, "Handler1"));
            CatchBlock CatchBlock1 = Expr.Catch(typeof(DivideByZeroException), Handler1);

            Expr FinallyBody =
                    Expr.Block(
                        new[] { TestValue },
                        Expr.Assign(TestValue, Expr.Constant(4)),
                        Expr.Assign(TestValue, Expr.Add(TestValue, Expr.Constant(3))),
                        EU.ConcatEquals(Result, "Finally"),
                        EU.GenAreEqual(Expr.Constant(7), TestValue)
                    );

            TryExpression MyTry = Expr.TryCatchFinally(Body, FinallyBody, new CatchBlock[] { CatchBlock1 });

            Expressions.Add(MyTry);

            Expressions.Add(EU.GenAreEqual(Expr.Constant("BodyHandler1Finally"), Result, "Try1"));

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // throw an exception from fault
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Try 31", new string[] { "positive", "try", "controlflow", "Pri2" })]
        public static Expr Try31(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression Ex = Expr.Variable(typeof(Exception), "");

            // case 1 - no catch blocks
            Expr Body = EU.BlockVoid(EU.ConcatEquals(Result, "Body1"), Expr.Throw(TE(typeof(DivideByZeroException), "TestException")), EU.ConcatEquals(Result, "Body2"));

            Expr FaultBlock = EU.BlockVoid(EU.ConcatEquals(Result, "FaultBlock"), Expr.Throw(TE(typeof(DivideByZeroException), "TestException")), EU.ConcatEquals(Result, "Fault2"));

            Expr MyTry = Expr.TryFault(Body, FaultBlock);

            Expressions.Add(Expr.TryCatch(MyTry, Expr.Catch(typeof(DivideByZeroException), EU.BlockVoid(EU.ConcatEquals(Result, "Caught")))));

            Expressions.Add(EU.GenAreEqual(Expr.Constant("Body1FaultBlockCaught"), Result, "Try 1"));

            var tree = EU.BlockVoid(new[] { Result, Ex }, Expressions);
            V.Validate(tree, true);
            return tree;
        }

        // Pass a value returning expression to the body
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Try 34", new string[] { "positive", "try", "controlflow", "Pri2" })]
        public static Expr Try34(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expr Body = Expr.Block(EU.ConcatEquals(Result, "Body"), Expr.Add(Expr.Constant(4), Expr.Constant(2)));

            Expr FaultBlock = EU.ConcatEquals(Result, "Fault");

            TryExpression MyTry = Expr.TryFault(Body, FaultBlock);

            Expressions.Add(MyTry);

            Expressions.Add(EU.GenAreEqual(Expr.Constant("Body"), Result, "Try1"));

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.Validate(tree, true);
            return tree;
        }

        // Pass a value returning expression to the fault
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Try 35", new string[] { "positive", "try", "controlflow", "Pri2" })]
        public static Expr Try35(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expr Body = EU.ConcatEquals(Result, "Body");

            Expr FaultBlock = Expr.Block(EU.ConcatEquals(Result, "Fault"), Expr.Add(Expr.Constant(4), Expr.Constant(2)));

            TryExpression MyTry = Expr.TryFault(Body, FaultBlock);

            Expressions.Add(MyTry);

            Expressions.Add(EU.GenAreEqual(Expr.Constant("Body"), Result, "Try1"));

            // case 2 - body throws an exception

            Expressions.Add(Expr.Assign(Result, Expr.Constant("")));

            Expr Body2 = EU.BlockVoid(EU.ConcatEquals(Result, "Body1"), Expr.Throw(TE(typeof(DivideByZeroException), "TestException")), EU.ConcatEquals(Result, "Body2"));

            TryExpression MyTry2 = Expr.TryFault(Body2, FaultBlock);

            Expressions.Add(Expr.TryCatch(MyTry2, Expr.Catch(typeof(DivideByZeroException), EU.BlockVoid(EU.ConcatEquals(Result, "Caught")))));

            Expressions.Add(EU.GenAreEqual(Expr.Constant("Body1FaultCaught"), Result, "Try2"));

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.Validate(tree, true);
            return tree;
        }

        // rethrow from a finally block
        // Regression for Dev10 bug 495240 linked to 493210 fixed on 9/16/08
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Try 36", new string[] { "negative", "try", "controlflow", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr Try36(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expr Body = EU.BlockVoid(EU.ConcatEquals(Result, "Body1"), Expr.Throw(TE(typeof(DivideByZeroException), "TestException")), EU.ConcatEquals(Result, "Body2"));

            Expr FinallyBody = Expr.Block(EU.ConcatEquals(Result, "Finally1"), Expr.Rethrow(), EU.ConcatEquals(Result, "Finally2"));

            CatchBlock CatchBlock1 = Expr.Catch(typeof(DivideByZeroException), Expr.Empty());

            TryExpression MyTry = Expr.TryCatchFinally(Body, FinallyBody, CatchBlock1);

            Expressions.Add(Expr.TryCatch(MyTry, Expr.Catch(typeof(DivideByZeroException), EU.BlockVoid(EU.ConcatEquals(Result, "Caught")))));

            Expressions.Add(EU.GenAreEqual(Expr.Constant("Body1Finally1Caught"), Result, "Try1"));

            // case 2 - no exception thrown
            Expressions.Add(Expr.Assign(Result, Expr.Constant("")));

            Expr Body2 = EU.ConcatEquals(Result, "Body1");

            TryExpression MyTry2 = Expr.TryCatchFinally(Body2, FinallyBody);

            Expressions.Add(Expr.TryCatch(MyTry2, Expr.Catch(typeof(DivideByZeroException), EU.ConcatEquals(Result, "Caught"))));

            Expressions.Add(EU.GenAreEqual(Expr.Constant("Body1Finally1Caught"), Result, "Try2"));

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true);
            return tree;
        }

        // rethrow from a filter
        // TODO: Dev10 bug 502271, open design issue
        [ETUtils.TestAttribute(ETUtils.TestState.Disabled, "Try 37", new string[] { "negative", "try", "controlflow", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Try37(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expr Body = EU.BlockVoid(EU.ConcatEquals(Result, "Body1"), Expr.Throw(TE(typeof(DivideByZeroException), "TestException")), EU.ConcatEquals(Result, "Body2"));

            Expr Handler1 = EU.BlockVoid(EU.ConcatEquals(Result, "Handler1"));
            Expr Filter1 = Expr.Block(EU.ConcatEquals(Result, "Filter1"), Expr.Rethrow(), EU.ConcatEquals(Result, "Filter1_2"), Expr.Constant(true));
            CatchBlock CatchBlock1 = Expr.Catch(typeof(DivideByZeroException), Handler1, Filter1);

            Expr FinallyBody = EU.ConcatEquals(Result, "Finally");

            TryExpression MyTry = Expr.TryCatchFinally(Body, FinallyBody, new CatchBlock[] { CatchBlock1 });

            Expressions.Add(Expr.TryCatch(MyTry, Expr.Catch(typeof(DivideByZeroException), EU.BlockVoid(EU.ConcatEquals(Result, "Caught")))));

            Expressions.Add(EU.GenAreEqual(Expr.Constant("Body1Filter1FinallyCaught"), Result, "Try1"));

            return EU.BlockVoid(new[] { Result }, Expressions);
        }

        // rethrow from a fault block
        // Regression for Dev10 bug 493210 fixed on 9/16/08
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Try 38", new string[] { "negative", "try", "controlflow", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr Try38(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expr Body = EU.BlockVoid(EU.ConcatEquals(Result, "Body1"), Expr.Throw(TE(typeof(DivideByZeroException), "TestException")), EU.ConcatEquals(Result, "Body2"));

            Expr FaultBody = Expr.Block(EU.ConcatEquals(Result, "Fault1"), Expr.Rethrow(), EU.ConcatEquals(Result, "Fault2"));

            TryExpression MyTry = Expr.TryFault(Body, FaultBody);

            Expressions.Add(Expr.TryCatch(MyTry, Expr.Catch(typeof(DivideByZeroException), EU.BlockVoid(EU.ConcatEquals(Result, "Caught")))));

            Expressions.Add(EU.GenAreEqual(Expr.Constant("Body1Fault1Caught"), Result, "Try1"));

            // case 2 - no exception thrown
            Expressions.Add(Expr.Assign(Result, Expr.Constant("")));

            Expr Body2 = EU.ConcatEquals(Result, "Body1");

            Expr FaultBody2 = EU.BlockVoid(EU.ConcatEquals(Result, "Fault1"), Expr.Rethrow(), EU.ConcatEquals(Result, "Fault2"));

            TryExpression MyTry2 = Expr.TryFault(Body2, FaultBody2);

            Expressions.Add(Expr.TryCatch(MyTry2, Expr.Catch(typeof(DivideByZeroException), EU.ConcatEquals(Result, "Caught"))));

            Expressions.Add(EU.GenAreEqual(Expr.Constant("Body1"), Result, "Try2"));

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true, true);
            return tree;
        }

        // rethrow from outside a try block
        // Regression for Dev 10 Bug 493210 fixed on 9/16/08
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Try 40", new string[] { "negative", "try", "controlflow", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr Try40(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expr Body = EU.BlockVoid(EU.ConcatEquals(Result, "Body1"), Expr.Throw(TE(typeof(DivideByZeroException), "TestException")), EU.ConcatEquals(Result, "Body2"));

            Expr FaultBody = EU.ConcatEquals(Result, "Fault1");

            TryExpression MyTry = Expr.TryFault(Body, FaultBody);
            Expr RethrowBlock = EU.BlockVoid(EU.ConcatEquals(Result, "Rethrow1"), Expr.Rethrow(), EU.ConcatEquals(Result, "Rethrow2"), MyTry);

            Expressions.Add(Expr.TryCatch(RethrowBlock, Expr.Catch(typeof(DivideByZeroException), EU.BlockVoid(EU.ConcatEquals(Result, "Caught")))));

            Expressions.Add(EU.GenAreEqual(Expr.Constant("Body1Rethrow1Fault1Caught"), Result, "Try1"));

            // case 2 - no exception thrown
            Expressions.Add(Expr.Assign(Result, Expr.Constant("")));

            Expr Body2 = EU.ConcatEquals(Result, "Body1");

            TryExpression MyTry2 = Expr.TryFault(Body2, FaultBody);
            Expr RethrowBlock2 = EU.BlockVoid(EU.ConcatEquals(Result, "Rethrow1"), Expr.Rethrow(), EU.ConcatEquals(Result, "Rethrow2"), MyTry2);

            Expressions.Add(Expr.TryCatch(RethrowBlock2, Expr.Catch(typeof(DivideByZeroException), EU.BlockVoid(EU.ConcatEquals(Result, "Caught")))));

            Expressions.Add(EU.GenAreEqual(Expr.Constant("Body1Rethrow1Fault1Caught"), Result, "Try2"));

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true);
            return tree;
        }

        // Throw a non exception type exception
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Try 42", new string[] { "negative", "try", "controlflow", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Try42(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expr Body =
            EU.Throws<System.ArgumentException>(() =>
            {
                EU.BlockVoid(EU.ConcatEquals(Result, "Body1"), Expr.Throw(Expr.Constant(1)), EU.ConcatEquals(Result, "Body2"));
            });

            CatchBlock CatchBlock1 = Expr.Catch(typeof(Int32), EU.BlockVoid(EU.ConcatEquals(Result, "Catch1")));
            CatchBlock CatchBlock2 = Expr.Catch(typeof(Exception), EU.BlockVoid(EU.ConcatEquals(Result, "Catch2")));

            Expr FinallyBody = EU.ConcatEquals(Result, "Finally");

            var MyTry = EU.Throws<System.ArgumentException>(() =>
            {
                Expr.TryCatchFinally(Body, FinallyBody, new CatchBlock[] { CatchBlock1, CatchBlock2 });
            });

            Expressions.Add(MyTry);

            Expressions.Add(EU.GenAreEqual(Expr.Constant("Body1Catch1Finally"), Result));

            return EU.BlockVoid(new[] { Result }, Expressions);
        }


        // Build a comma with a try statement: expr1, expr2, try expr
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Try 43", new string[] { "positive", "try", "controlflow", "Pri1" })]
        public static Expr Try43(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expr Body = EU.BlockVoid(EU.ConcatEquals(Result, "Body1"), Expr.Throw(TE(typeof(DivideByZeroException), "TestException")), EU.ConcatEquals(Result, "Body2"));

            Expr Handler1 = EU.BlockVoid(EU.ConcatEquals(Result, "Handler1"));
            CatchBlock CatchBlock1 = Expr.Catch(typeof(DivideByZeroException), Handler1);

            Expr FinallyBody = EU.ConcatEquals(Result, "Finally");

            TryExpression MyTry = Expr.TryCatchFinally(Body, FinallyBody, CatchBlock1);

            Expr List = Expr.Block(EU.ConcatEquals(Result, "Expr1"), EU.ConcatEquals(Result, "Expr2"), MyTry);

            Expressions.Add(List);

            Expressions.Add(EU.GenAreEqual(Expr.Constant("Expr1Expr2Body1Handler1Finally"), Result, "Try1"));

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Non defined variable expression (not in a scope) for holder on filters
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Try 44", new string[] { "positive", "try", "controlflow", "Pri2" })]
        public static Expr Try44(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression Ex = Expr.Variable(typeof(DivideByZeroException), "");

            Expr Body = EU.BlockVoid(EU.ConcatEquals(Result, "Body1"), Expr.Throw(TE(typeof(DivideByZeroException), "TestException")), EU.ConcatEquals(Result, "Body2"));

            Expr Filter1 = Expr.Block(EU.ConcatEquals(Result, "Filter1"), Expr.Constant(true));
            Expr Handler1 = EU.BlockVoid(EU.ConcatEquals(Result, "Handler1"));
            CatchBlock CatchBlock1 = Expr.Catch(Ex, Handler1, Filter1);

            Expr FinallyBody = EU.ConcatEquals(Result, "Finally");

            TryExpression MyTry = Expr.TryCatchFinally(Body, FinallyBody, CatchBlock1);

            Expressions.Add(MyTry);

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.Validate(tree, true);
            return tree;
        }

        // Holder variable is defined in scope of filter body
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Try 45", new string[] { "positive", "try", "controlflow", "Pri2" })]
        public static Expr Try45(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression Ex;

            Expr Body = EU.BlockVoid(EU.ConcatEquals(Result, "Body1"), Expr.Throw(TE(typeof(DivideByZeroException), "TestException")), EU.ConcatEquals(Result, "Body2"));

            Expr Filter1 =
                Expr.Block(
                    EU.ConcatEquals(Result, "Filter1"),
                    Ex = Expr.Variable(typeof(DivideByZeroException), ""),
                    Expr.Constant(true));
            Expr Handler1 = EU.BlockVoid(EU.ConcatEquals(Result, "Handler1"));
            CatchBlock CatchBlock1 = Expr.Catch(Ex, Handler1, Filter1);

            Expr FinallyBody = EU.ConcatEquals(Result, "Finally");

            TryExpression MyTry = Expr.TryCatchFinally(Body, FinallyBody, CatchBlock1);

            Expressions.Add(MyTry);

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.Validate(tree, true);
            return tree;
        }

        // try inside a filter
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Try 46", new string[] { "negative", "try", "controlflow", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr Try46(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "");
            Expr tree = Expr.Empty();

            Expr Body = EU.BlockVoid(EU.ConcatEquals(Result, "Body1"), Expr.Throw(TE(typeof(DivideByZeroException), "TestException")), EU.ConcatEquals(Result, "Body2"));

            Expr Handler1 = EU.BlockVoid(EU.ConcatEquals(Result, "Handler1"));
            Expr innerTry = Expr.TryCatch(EU.ConcatEquals(Result, "innerTry"), Expr.Catch(typeof(IndexOutOfRangeException), EU.ConcatEquals(Result, "innerCatch")));
            Expr Filter1 = Expr.Block(EU.ConcatEquals(Result, "Filter1"), innerTry, Expr.Constant(true));
            CatchBlock CatchBlock1 = Expr.Catch(typeof(DivideByZeroException), Handler1, Filter1);

            Expr FinallyBody = EU.ConcatEquals(Result, "Finally");

            TryExpression MyTry = Expr.TryCatchFinally(Body, FinallyBody, new CatchBlock[] { CatchBlock1 });

            Expressions.Add(Expr.TryCatch(MyTry, Expr.Catch(typeof(DivideByZeroException), EU.BlockVoid(EU.ConcatEquals(Result, "Caught")))));

            Expressions.Add(EU.GenAreEqual(Expr.Constant("Body1Filter1innerTryHandler1Finally"), Result, "Try1"));
            
            tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true, true);
            return tree;
        }


        // try return type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Try 47", new string[] { "positive", "try", "controlflow", "Pri2" })]
        public static Expr Try47(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expr Body = Expr.Constant(1);

            Expr Handler1 = Expr.Constant(2);
            CatchBlock CatchBlock1 = Expr.Catch(typeof(DivideByZeroException), Handler1);

            Expr FinallyBody = Expr.Empty();

            TryExpression MyTry = Expr.TryCatchFinally(Body, FinallyBody, new CatchBlock[] { CatchBlock1 });

            if (MyTry.Type == typeof(int)) {
                return Expr.Empty();
            } else {
                throw new Exception("Expected try type to be int");
            }
        }

        // try return type 2
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Try 48", new string[] { "negative", "try", "controlflow", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Try48(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expr Body = Expr.Constant(1);

            Expr Handler1 = Expr.Constant((short)2, typeof(short));
            CatchBlock CatchBlock1 = Expr.Catch(typeof(DivideByZeroException), Handler1);

            Expr FinallyBody = Expr.Empty();

            var MyTry = EU.Throws<System.ArgumentException>(() =>
            {
                Expr.TryCatchFinally(Body, FinallyBody, new CatchBlock[] { CatchBlock1 });
            });

            return Expr.Empty();
        }

        // try return type 3 - finally has a type.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Try 49", new string[] { "positive", "try", "controlflow", "Pri2" })]
        public static Expr Try49(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expr Body = Expr.Constant(1);

            Expr Handler1 = Expr.Constant(2);
            CatchBlock CatchBlock1 = Expr.Catch(typeof(DivideByZeroException), Handler1);

            Expr FinallyBody = Expr.Constant((double)3);

            TryExpression MyTry = Expr.TryCatchFinally(Body, FinallyBody, new CatchBlock[] { CatchBlock1 });

            Expressions.Add(EU.ExprTypeCheck(MyTry, typeof(int)));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }


        // rethrow from inside a generator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Try 50", new string[] { "positive", "try", "controlflow", "Pri1" })]
        public static Expr Try50(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(bool), "Result");
            ParameterExpression Value = Expr.Variable(typeof(object), "Value");

            LabelTarget label = Expr.Label(typeof(int));

            var Gen =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    label,
                    Expr.Block(
                        new ParameterExpression[] { },
                        Expr.TryCatchFinally(
                            Expr.Block(
                                AstUtils.YieldReturn(label, Expr.Constant(1)),
                                Expr.Throw(Expr.Constant(new DivideByZeroException())),
                                AstUtils.YieldReturn(label, Expr.Constant(2))
                            ),
                            Expr.Block(
                                AstUtils.YieldReturn(label, Expr.Constant(5))
                            ),
                            Expr.Catch(
                                typeof(DivideByZeroException),
                                Expr.Block(
                                    AstUtils.YieldReturn(label, Expr.Constant(3)),
                                    Expr.Rethrow(),
                                    AstUtils.YieldReturn(label, Expr.Constant(4))
                                )
                            )
                        )
                    ),
                    new ParameterExpression[] { }
                );

            IEnumerator enumerator = (IEnumerator)Gen.Compile().DynamicInvoke();
            Expr e = Expr.Constant(enumerator, typeof(IEnumerator));

            // can't use EU.Enumerate like normal Generator tests because Rethrow is going to blow up the last couple checks in that method
            MethodInfo MoveNext = typeof(IEnumerator).GetMethod("MoveNext");
            PropertyInfo Current = typeof(IEnumerator).GetProperty("Current");

            int[] expectedValues = { 1, 3, 5 };
            int numTimes = expectedValues.Length;

            int i = 0;
            for (; i < numTimes; i++) {
                Expressions.Add(Expression.Assign(Result, Expression.Call(e, MoveNext)));
                Expressions.Add(EU.GenAreEqual(Result, Expression.Constant(true), "Yield " + i));
                Expressions.Add(Expression.Assign(Value, Expression.Property(e, Current)));
                Expressions.Add(EU.GenAreEqual(Expression.Unbox(Value, expectedValues[0].GetType()), Expression.Constant(expectedValues[i]), "Yield " + i));
            }

            var tree = Expr.Block(new[] { Result, Value }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Expr.MakeTry with fault and finally
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Try 51", new string[] { "negative", "try", "controlflow", "Pri2" }, Priority = 2, Exception = typeof(ArgumentException))]
        public static Expr Try51(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            Expr MyTry =
                EU.Throws<System.ArgumentException>(() =>
                {
                    Expr.MakeTry(
                        null,
                        EU.ConcatEquals(Result, Expr.Constant("Body")),
                        EU.ConcatEquals(Result, Expr.Constant("Finally")),
                        EU.ConcatEquals(Result, Expr.Constant("Fault")),
                        new List<CatchBlock>() { Expr.Catch(typeof(DivideByZeroException), EU.ConcatEquals(Result, Expr.Constant("Catch"))) }
                    );
                });

            Expressions.Add(MyTry);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("BodyFinallyFault"), Result, "Try 1"));

            return Expr.Empty();
        }

        // Expr.MakeTry with no finally, fault or catch
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Try 52", new string[] { "negative", "try", "controlflow", "Pri2" }, Priority = 2, Exception = typeof(ArgumentException))]
        public static Expr Try52(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            Expr MyTry =
                EU.Throws<System.ArgumentException>(() =>
                {
                    Expr.MakeTry(
                        null,
                        EU.ConcatEquals(Result, Expr.Constant("Body")),
                        null,
                        null,
                        new List<CatchBlock>() { }
                    );
                });

            Expressions.Add(MyTry);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Body"), Result, "Try 1"));

            return Expr.Empty();
        }

        // Mismatched types for try body and catch
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Try 53", new string[] { "negative", "try", "controlflow", "Pri2" }, Priority = 2, Exception = typeof(ArgumentException))]
        public static Expr Try53(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            Expr MyTry =
                EU.Throws<System.ArgumentException>(() =>
                {
                    Expr.TryCatch(
                        Expr.Block(
                            Expr.Constant(1),
                            Expr.Empty()
                        ),
                        Expr.Catch(
                            typeof(DivideByZeroException),
                            Expr.Constant(2)
                        )
                    );
                });

            Expressions.Add(MyTry);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Body"), Result, "Try 1"));

            return Expr.Empty();
        }

        // Mismatched types for try body and catch
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Try 54", new string[] { "negative", "try", "controlflow", "Pri2" }, Priority = 2, Exception = typeof(ArgumentException))]
        public static Expr Try54(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            Expr MyTry =
                EU.Throws<System.ArgumentException>(() =>
                {
                    Expr.TryCatch(
                        Expr.Constant(1),
                        Expr.Catch(
                            typeof(DivideByZeroException),
                            Expr.Empty()
                        )
                    );
                });

            Expressions.Add(MyTry);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Body"), Result, "Try 1"));

            return Expr.Empty();
        }

        // Test         : Passing null to body of a try block
        // Expected     : ArgumentNullException 
        // Notes        : Regession for bug 461229
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Try 55", new string[] { "negative", "try", "controlflow", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr Try55(EU.IValidator V) {
            List<Expr> Expressions = new List<Expr>();

            Expr Body = null; Expr.Block(Expr.Throw(TE(typeof(Exception), "Test")));

            Expr FinallyBody = Expr.Call(typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }), Expr.Constant("Finally"));

            //try statement with both a finally and a fault.
            CatchBlock cb = Expr.Catch(typeof(InvalidOperationException), FinallyBody, null);


            var MyTry = EU.Throws<System.ArgumentNullException>(() =>
            {
                Expr.TryCatchFinally(Body, Expr.Empty(), cb);
            });

            Expressions.Add(MyTry);

            Expr.Lambda(Expr.Block(Expressions)).Compile().DynamicInvoke();

            return Expr.Empty();
        }
    }
}
