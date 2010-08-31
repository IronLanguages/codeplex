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

    public class Throw {

        private static Expr TE(Type t, string Message) {
            ConstructorInfo ci = t.GetConstructor(new Type[] { typeof(String) });
            Expression Ex = Expr.New(ci, Expr.Constant(Message));
            return Ex;
        }


        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Throw 1", new string[] { "positive", "throw", "controlflow", "Pri1" })]
        public static Expr Throw1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expr Body = Expr.Throw(TE(typeof(RankException), "Well, duh!"));

            CatchBlock Catch = Expr.Catch(typeof(RankException), Expr.Block(EU.ConcatEquals(Result, "NullThrow"), Expr.Throw(null)));

            Expr Body2 = Expr.TryCatch(Body, Catch);

            CatchBlock Catch2 = Expr.Catch(typeof(RankException), EU.BlockVoid(EU.ConcatEquals(Result, "Caught")));

            Expressions.Add(Expr.TryCatch(Body2, Catch2));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("NullThrowCaught"), Result, "Throw 1"));

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Throw 2", new string[] { "negative", "throw", "controlflow", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Throw2(EU.IValidator V) {
            return EU.Throws<ArgumentException>(() => { Expression.Throw(Expression.Constant(5)); });

        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Throw 3", new string[] { "positive", "throw", "controlflow", "Pri1" })]
        public static Expr Throw3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expr OhMy = Expr.Block(Expr.Throw(TE(typeof(FormatException), "")), TE(typeof(RankException), ""));

            Expr Body = Expr.Throw(OhMy);

            CatchBlock Catch = Expr.Catch(typeof(FormatException), EU.BlockVoid(EU.ConcatEquals(Result, "Caught")));

            Expressions.Add(Expr.TryCatch(Body, Catch));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Caught"), Result, "Throw 1"));

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Throw 4", new string[] { "positive", "throw", "controlflow", "Pri1" })]
        public static Expr Throw4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expr OhMy = Expr.Constant(null, typeof(RankException));

            Expr Body = Expr.Throw(OhMy);

            CatchBlock Catch = Expr.Catch(typeof(NullReferenceException), EU.BlockVoid(EU.ConcatEquals(Result, "Caught")));

            Expressions.Add(Expr.TryCatch(Body, Catch));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Caught"), Result, "Throw 1"));

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Throw 5", new string[] { "negative", "throw", "controlflow", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Throw5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

            Expr OhMy = EU.BlockVoid(Expr.Empty());

            Expr Body =
            EU.Throws<System.ArgumentException>(() =>
            {
                Expr.Throw(OhMy);
            });

            return Expr.Empty();
        }

        // pass a scope as the expression to throw
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Throw 6", new string[] { "positive", "throw", "controlflow", "Pri2" })]
        public static Expr Throw6(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");
            ParameterExpression ScopedVal = Expr.Variable(typeof(Int32), "");

            BlockExpression ToThrow = Expr.Block(
                        new ParameterExpression[] { ScopedVal },
                        EU.ConcatEquals(Result, "ToThrow"),
                        Expr.Assign(ScopedVal, Expr.Constant(5)),
                        Expr.Assign(ScopedVal, Expr.Add(ScopedVal, Expr.Constant(3))),
                        EU.GenAreEqual(ScopedVal, Expr.Constant(8)),
                        TE(typeof(RankException), "")
                    );

            Expr Body = Expr.Throw(ToThrow, typeof(void));

            CatchBlock Catch = Expr.Catch(typeof(RankException), EU.BlockVoid(EU.ConcatEquals(Result, "Caught")));

            Expressions.Add(Expr.TryCatch(Body, Catch));

            Expressions.Add(EU.GenAreEqual(Expr.Constant("ToThrowCaught"), Result, "Throw 1"));

            var tree = EU.BlockVoid(new ParameterExpression[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // throw and catch a non-exception object
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Throw 7", new string[] { "positive", "throw", "controlflow", "Pri2" })]
        public static Expr Throw7(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");

            Expr tree =
                Expr.TryCatch(
                    Expr.Block(
                        EU.ConcatEquals(Result, Expr.Constant("BeforeThrow")),
                        Expr.Throw(Expr.Constant("StringException")),
                        EU.ConcatEquals(Result, Expr.Constant("AfterThrow"))
                    ),
                    new CatchBlock[] {
                        Expr.Catch(typeof(DivideByZeroException), EU.ConcatEquals(Result, Expr.Constant("Bad"))),
                        Expr.Catch(typeof(Exception), EU.ConcatEquals(Result, Expr.Constant("AlsoBad"))),
                        Expr.Catch(typeof(string), EU.ConcatEquals(Result, Expr.Constant("StringCaught")))
                    }
                );

            Expressions.Add(tree);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("BeforeThrowStringCaught"), Result, "Throw 1"));

            Expressions.Add(Expr.Assign(Result, Expr.Constant("")));

            ParameterExpression Ex = Expr.Variable(typeof(string), "Result");

            // let the non-exception object get caught by an outer catch
            Expr tree2 =
                Expr.TryCatch(
                    Expr.Block(
                        EU.ConcatEquals(Result, Expr.Constant("A")),
                        Expr.TryCatch(
                            Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("B")),
                                Expr.Throw(Expr.Constant("StringException")),
                                EU.ConcatEquals(Result, Expr.Constant("C"))
                            ),
                            Expr.Catch(typeof(Exception), EU.ConcatEquals(Result, Expr.Constant("InnerCatch")))
                        ),
                        EU.ConcatEquals(Result, Expr.Constant("D"))
                    ),
                    new CatchBlock[] {
                        Expr.Catch(typeof(DivideByZeroException), EU.ConcatEquals(Result, Expr.Constant("Bad"))),
                        Expr.Catch(typeof(Exception), EU.ConcatEquals(Result, Expr.Constant("AlsoBad"))),
                        Expr.Catch(
                            Ex,
                            Expr.Block(
                                EU.ConcatEquals(Result, Expr.Constant("StringCaught")),
                                EU.ConcatEquals(Result, Ex)
                            )
                        )
                    }
                );

            Expressions.Add(tree2);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("ABStringCaughtStringException"), Result, "Throw 1"));

            var FinalTree = Expr.Block(new[] { Result, Ex }, Expressions);
            V.Validate(FinalTree);
            return FinalTree;
        }

//RuntimeWrappedException is inaccessible on Silverlight
#if !SILVERLIGHT
        // throw a non-exception object
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Throw 8", new string[] { "negative", "throw", "controlflow", "Pri2" }, Exception = typeof(System.Runtime.CompilerServices.RuntimeWrappedException))]
        public static Expr Throw8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");

            Expr tree =
                Expr.TryCatch(
                    Expr.Block(
                        EU.ConcatEquals(Result, Expr.Constant("BeforeThrow")),
                        Expr.Throw(Expr.Constant("StringException"), typeof(string)),
                        EU.ConcatEquals(Result, Expr.Constant("AfterThrow"))
                    ),
                    new CatchBlock[] {
                        Expr.Catch(typeof(DivideByZeroException), EU.ConcatEquals(Result, Expr.Constant("Bad"))),
                        Expr.Catch(typeof(Exception), EU.ConcatEquals(Result, Expr.Constant("AlsoBad")))
                    }
                );

            Expressions.Add(tree);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("BeforeThrowStringCaught"), Result, "Throw 1"));

            var FinalTree = Expr.Block(new[] { Result }, Expressions);
            V.ValidateException<System.Runtime.CompilerServices.RuntimeWrappedException>(FinalTree, false);
            return FinalTree;
        }
#endif


        // throw a value type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Throw 9", new string[] { "negative", "throw 9", "controlflow", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Throw9(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Ex = Expr.Variable(typeof(int), "Result");

            Expr tree =
                EU.Throws<System.ArgumentException>(() =>
                {
                    Expr.TryCatch(
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("A")),
                            Expr.Throw(Expr.Constant(1)),
                            EU.ConcatEquals(Result, Expr.Constant("B"))
                        ),
                        new CatchBlock[] {
	                        Expr.Catch(typeof(DivideByZeroException), EU.ConcatEquals(Result, Expr.Constant("Bad"))),
	                        Expr.Catch(typeof(Exception), EU.ConcatEquals(Result, Expr.Constant("AlsoBad"))),
	                        Expr.Catch(
	                            Ex,
	                            Expr.Block(
	                                EU.ConcatEquals(Result, Expr.Constant("IntCaught")),
	                                EU.ConcatEquals(Result, Ex)
	                            )
	                        )
	                    }
                    );
                });

            Expressions.Add(tree);

            return Expr.Empty();
        }

        public class MyClass {
            public string Data;
            public MyClass(string x) { Data = x; }
        }

        // throw and catch a user defined non-exception object
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Throw 10", new string[] { "positive", "throw", "controlflow", "Pri2" })]
        public static Expr Throw10(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Ex = Expr.Variable(typeof(MyClass), "Ex");

            Expr tree =
                Expr.TryCatch(
                    Expr.Block(
                        EU.ConcatEquals(Result, Expr.Constant("BeforeThrow")),
                        Expr.Throw(Expr.Constant(new MyClass("SomeValue"))),
                        EU.ConcatEquals(Result, Expr.Constant("AfterThrow"))
                    ),
                    new CatchBlock[] {
                        Expr.Catch(typeof(Exception), EU.ConcatEquals(Result, Expr.Constant("AlsoBad"))),
                        Expr.Catch(typeof(string), EU.ConcatEquals(Result, Expr.Constant("StringCaught"))),
                        Expr.Catch(Ex, EU.ConcatEquals(Result, Expr.Constant("MyClassCaught")))
                    }
                );

            Expressions.Add(tree);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("BeforeThrowMyClassCaught"), Result, "Throw 1"));

            var FinalTree = Expr.Block(new[] { Result, Ex }, Expressions);
            V.Validate(FinalTree);
            return FinalTree;
        }

        public struct MyStruct {
            public string Data;
            public MyStruct(string x) { Data = x; }
        }

        // throw and catch a user defined non-exception object
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Throw 11", new string[] { "negative", "throw", "controlflow", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Throw11(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            ParameterExpression Ex = Expr.Variable(typeof(MyStruct), "Ex");

            Expr tree =
                EU.Throws<System.ArgumentException>(() =>
                {
                    Expr.TryCatch(
                        Expr.Block(
                            EU.ConcatEquals(Result, Expr.Constant("BeforeThrow")),
                            Expr.Throw(Expr.Constant(new MyStruct("SomeValue"))),
                            EU.ConcatEquals(Result, Expr.Constant("AfterThrow"))
                        ),
                        new CatchBlock[] {
	                        Expr.Catch(typeof(Exception), EU.ConcatEquals(Result, Expr.Constant("AlsoBad"))),
	                        Expr.Catch(typeof(string), EU.ConcatEquals(Result, Expr.Constant("StringCaught"))),
	                        Expr.Catch(Ex, EU.ConcatEquals(Result, Expr.Constant("MyStructCaught")))
	                    }
                    );
                });

            Expressions.Add(tree);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("BeforeThrowMyClassCaught"), Result, "Throw 1"));

            return Expr.Block(new[] { Result, Ex }, Expressions);
        }

        // throw a derived class of object and catch it as typeof(object)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Throw 12", new string[] { "positive", "throw 12", "controlflow", "Pri1" })]
        public static Expr Throw12(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");

            Expr tree =
                Expr.TryCatch(
                    Expr.Block(
                        EU.ConcatEquals(Result, Expr.Constant("A")),
                        Expr.Throw(Expr.Constant(new List<int> { 1, 2, 3 })),
                        EU.ConcatEquals(Result, Expr.Constant("B"))
                    ),
                    new CatchBlock[] {
                        Expr.Catch(typeof(DivideByZeroException), EU.ConcatEquals(Result, Expr.Constant("Bad"))),
                        Expr.Catch(typeof(Exception), EU.ConcatEquals(Result, Expr.Constant("AlsoBad"))),
                        Expr.Catch(typeof(Object), EU.ConcatEquals(Result, Expr.Constant("ObjectCaught")))
                    }
                );

            Expressions.Add(tree);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("AObjectCaught"), Result, "Throw 1"));

            // throw an int cast to object
            ParameterExpression Ex = Expr.Variable(typeof(object), "Ex");
            ParameterExpression TestValue = Expr.Variable(typeof(int), "TestValue");

            Expressions.Add(Expr.Assign(TestValue, Expr.Constant(2)));
            Expressions.Add(Expr.Assign(Result, Expr.Constant("")));

            Expr tree2 =
                Expr.TryCatch(
                    Expr.Block(
                        EU.ConcatEquals(Result, Expr.Constant("A")),
                        Expr.Throw(Expr.Convert(Expr.Constant(3), typeof(object))),
                        EU.ConcatEquals(Result, Expr.Constant("B"))
                    ),
                    new CatchBlock[] {
                        Expr.Catch(typeof(DivideByZeroException), EU.ConcatEquals(Result, Expr.Constant("Bad"))),
                        Expr.Catch(typeof(Exception), EU.ConcatEquals(Result, Expr.Constant("AlsoBad"))),
                        Expr.Catch(
                            Ex,
                            Expr.Block(
                                Expr.Assign(TestValue, Expr.Add(TestValue, Expr.Unbox(Ex, typeof(int)))),
                                EU.ConcatEquals(Result, Expr.Constant("ObjectCaught"))
                            )
                        )
                    }
                );

            Expressions.Add(tree2);
            Expressions.Add(EU.GenAreEqual(Expr.Constant("AObjectCaught"), Result, "Throw 2"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(5), TestValue, "Throw 3"));

            var FinalTree = Expr.Block(new[] { Result, TestValue }, Expressions);
            V.Validate(FinalTree);
            return FinalTree;
        }
    }
}
