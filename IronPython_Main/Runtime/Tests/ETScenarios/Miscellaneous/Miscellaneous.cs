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
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;

namespace ETScenarios.Miscellaneous {
    using EU = ETUtils.ExpressionUtils;
    using Expr = Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public class Miscellaneous {

        //arg[i]++
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Miscellaneous 3", new string[] { "positive", "arguments", "miscellaneous", "Pri1" })]
        public static Expr Miscellaneous3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Var = Expr.Parameter(typeof(int[]), "");
            Expressions.Add(Expr.Assign(Var, Expr.Constant(new int[] { 1, 2, 3, 4 })));
            Expr Arr = Expr.ArrayAccess(Var, Expr.Constant(2));

            Expressions.Add(Expr.Increment(Arr));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(3), Arr));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(4), Expr.PreIncrementAssign(Arr)));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(4), Expr.PostIncrementAssign(Arr)));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(5), Arr));

            var tree = Expr.Block(new[] { Var }, Expressions);
            V.Validate(tree);
            return tree;

        }

        public class MyBinder : CallSiteBinder {
            public override Expression Bind(object[] args, ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel) {
                string Result = "";
                foreach (var arg in args) {
                    Result += arg.ToString();
                }

                return Expr.Return(returnLabel, Expr.Constant("Success" + Result));
            }
        }

        // TypedBinaryExpression test
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TypedBinaryExpression 1", new string[] { "positive", "typedbinaryexpression", "miscellaneous", "Pri2" }, Priority = 2)]
        public static Expr TypedBinaryExpression1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            Expressions.Add(Expr.Assign(Result, Expr.Constant("Test")));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(true), Expr.TypeEqual(Result, typeof(string)), "TypeEqual 1"));

            var tree = Expr.Block(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // UnaryExpressions via MakeUnary factory
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "UnaryExpression 2", new string[] { "positive", "unaryexpression", "miscellaneous", "Pri2" }, Priority = 2)]
        public static Expr UnaryExpression2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expr op = Expr.MakeUnary(ExpressionType.OnesComplement, Expr.Constant(1), typeof(int));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(-2), op, "UnaryExpression 1"));

            Expr op2 = Expr.MakeUnary(ExpressionType.UnaryPlus, Expr.Constant(1), typeof(int));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(1), op2, "UnaryExpression 2"));

            Expr op3 = Expr.MakeUnary(ExpressionType.Unbox, Expr.Constant(1, typeof(object)), typeof(int));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(1), op3, "UnaryExpression 3"));

            Expr op4 = Expr.MakeUnary(ExpressionType.Increment, Expr.Constant(1), typeof(int));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(2), op4, "UnaryExpression 4"));

            Expr op5 = Expr.MakeUnary(ExpressionType.Decrement, Expr.Constant(1), typeof(int));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(0), op5, "UnaryExpression 5"));

            ParameterExpression Value = Expr.Variable(typeof(int), "Value");
            Expressions.Add(Expr.Assign(Value, Expr.Constant(1)));

            Expr op6 = Expr.MakeUnary(ExpressionType.PreIncrementAssign, Value, typeof(int));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(2), op6, "UnaryExpression 6"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(2), Value, "UnaryExpression 6_1"));

            Expr op7 = Expr.MakeUnary(ExpressionType.PreDecrementAssign, Value, typeof(int));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(1), op7, "UnaryExpression 7"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(1), Value, "UnaryExpression 7_1"));

            Expr op8 = Expr.MakeUnary(ExpressionType.PostIncrementAssign, Value, typeof(int));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(1), op8, "UnaryExpression 8"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(2), Value, "UnaryExpression 8_1"));

            Expr op9 = Expr.MakeUnary(ExpressionType.PostDecrementAssign, Value, typeof(int));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(2), op9, "UnaryExpression 9"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(1), Value, "UnaryExpression 9_1"));

            var tree = Expr.Block(new[] { Value }, Expressions);
            V.Validate(tree);
            return tree;
        }


        public static int IncrementMethod(int x) {
            return x + 2;
        }

        // UnaryExpression with MethodInfo based coercion operator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "UnaryExpression 3", new string[] { "positive", "unaryexpression", "miscellaneous", "Pri2" }, Priority = 2)]
        public static Expr UnaryExpression3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Miscellaneous).GetMethod("IncrementMethod");
            Expr Result = Expr.MakeUnary(ExpressionType.Increment, Expr.Constant(1), typeof(int), mi);
            Expressions.Add(EU.GenAreEqual(Expr.Constant(3), Result, "UnaryExpression 1"));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }

        public static int BadMethod() {
            return -1;
        }

        // UnaryExpression with MethodInfo based coercion operator with wrong number of args
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "UnaryExpression 4", new string[] { "negative", "unaryexpression", "miscellaneous", "Pri2" }, Exception = typeof(ArgumentException), Priority = 2)]
        public static Expr UnaryExpression4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Miscellaneous).GetMethod("BadMethod");
            Expr Result = EU.Throws<ArgumentException>(() => { Expr.MakeUnary(ExpressionType.Increment, Expr.Constant(1), typeof(int), mi); });

            return Expr.Empty();
        }

        public static double BadMethod2(int x) {
            return -1.1;
        }

        // UnaryExpression with MethodInfo based coercion operator with mistmatched arg and expression types
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "UnaryExpression 5", new string[] { "positive", "unaryexpression", "miscellaneous", "Pri2" }, Priority = 2)]
        public static Expr UnaryExpression5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Miscellaneous).GetMethod("BadMethod2");
            Expr Result = Expr.MakeUnary(ExpressionType.Increment, Expr.Constant(1), typeof(int), mi);
            Expressions.Add(EU.GenAreEqual(Expr.Constant(-1.1), Result, "UnaryExpression 1"));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }

        public static double BadIncrementMethod(int x) {
            return x + 1.1;
        }

        // MakeOpAssignUnary with return type of operator/MethodInfo != unary expression type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MakeOpAssignUnary 1", new string[] { "negative", "unaryexpression", "miscellaneous", "Pri2" }, Exception = typeof(ArgumentException), Priority = 2)]
        public static Expr MakeOpAssignUnary1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Miscellaneous).GetMethod("BadIncrementMethod");
            ParameterExpression Result = Expr.Variable(typeof(int), "Result");
            Expressions.Add(Expr.Assign(Result, Expr.Constant(1)));

            Expressions.Add(EU.Throws<ArgumentException>(() => { EU.GenAreEqual(Expr.Constant(2), Expr.PreIncrementAssign(Result, mi), "MakeOpAssignUnary 1"); }));

            return Expr.Empty();
        }

        public class TestClass {
            public int Value { get; set; }
            public int[] data;
            public TestClass() {
                Value = -1;
                data = new int[3] { 1, 2, 3 };
            }
            public int this[int index] {
                get {
                    return -1;
                }
                set {
                    data[index] = value;
                }
            }
        }

        public static string OpAssignMethod(double x, string y) {
            return (x + double.Parse(y)).ToString();
        }

        // OpAssignWithConversion test
        // Use Lambda IEnumerable factory for code coverage.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OpAssign 1", new string[] { "positive", "opassign", "miscellaneous", "Pri2" }, Priority = 2)]
        public static Expr OpAssign1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Variable(typeof(double), "Left");
            ParameterExpression Right = Expr.Variable(typeof(string), "Right");

            Expressions.Add(Expr.Assign(Left, Expr.Constant(2.1)));
            Expressions.Add(Expr.Assign(Right, Expr.Constant("1")));

            MethodInfo mi = typeof(Miscellaneous).GetMethod("OpAssignMethod");

            LambdaExpression converter =
                Expr.Lambda<Func<string, double>>(
                    Expr.Call(typeof(double).GetMethod("Parse", new Type[] { typeof(string) }), Right),
                    new List<ParameterExpression>() { Right }
                );

            Expr Result =
                Expr.AddAssign(
                    Left,
                    Right,
                    mi,
                    converter
                );

            Expressions.Add(EU.GenAreEqual(Expr.Constant(3.1), Result, "OpAssign 1"));

            var tree = Expr.Block(new[] { Left, Right }, Expressions);
            V.Validate(tree);
            return tree;
        }

        public static double OpAssignMethod2(double x, string y) {
            return 1.1;
        }

        // OpAssignWithConversion test
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OpAssign 2", new string[] { "negative", "opassign", "miscellaneous", "Pri2" }, Exception = typeof(InvalidOperationException), Priority = 2)]
        public static Expr OpAssign2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Left = Expr.Variable(typeof(double), "Left");
            ParameterExpression Right = Expr.Variable(typeof(string), "Right");

            Expressions.Add(Expr.Assign(Left, Expr.Constant(2.1)));
            Expressions.Add(Expr.Assign(Right, Expr.Constant("1")));

            MethodInfo mi = typeof(Miscellaneous).GetMethod("OpAssignMethod2");

            LambdaExpression converter =
                Expr.Lambda<Func<string, double>>(
                    Expr.Call(typeof(double).GetMethod("Parse", new Type[] { typeof(string) }), Right),
                    new ParameterExpression[] { Right }
                );

            Expr Result =
                EU.Throws<InvalidOperationException>(() =>
                {
                    Expr.AddAssign(
                        Left,
                        Right,
                        mi,
                        converter
                    );
                });

            return Expr.Empty();
        }

        public class BindingTestClass {
            public int _x;
            public string _y;
            public Data _d;
            public BindingTestClass() {
                _x = -1;
                _y = "Default";
            }
            public BindingTestClass(int a, string b) {
                _x = a;
                _y = b;
            }
        }

        public struct Data {
            public int a;
            public double b;
            public string c;
            Data(int x, double y, string z) {
                a = x;
                b = y;
                c = z;
            }
        }

        // MemberMemberBinding tests
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MemberMemberBind 1", new string[] { "positive", "membermemberbind", "miscellaneous", "Pri1" }, Priority = 2)]
        public static Expr MemberMemberBind1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Val3 = Expr.Variable(typeof(BindingTestClass), "Val3");

            MemberInfo memi = typeof(BindingTestClass).GetMember("_x")[0];
            var b1 = Expr.Bind(memi, Expr.Constant(3));
            MemberInfo memi2 = typeof(BindingTestClass).GetMember("_y")[0];
            var b2 = Expr.Bind(memi2, Expr.Constant("Test"));

            LabelTarget label = Expr.Label(typeof(int));

            Expr Tree =
                Expr.Block(
                    new[] { Val3 },
                    Expr.TryCatch(
                        Expr.Block(
                            AstUtils.YieldReturn(label, Expr.Constant(1)),
                            Expr.Assign(
                                Val3,
                                Expr.MemberInit(
                                    Expr.New(typeof(BindingTestClass)),
                                    new MemberBinding[] { 
                                        Expr.Bind(memi, Expr.Constant(-1)), // _x
                                        b2, // _y
                                        Expr.MemberBind( // _d
                                            typeof(BindingTestClass).GetField("_d"),
                                            new List<MemberBinding>() { // binding fields of _d member of type Data
                                                Expr.Bind(typeof(Data).GetField("a"), Expr.Constant(5)),
                                                Expr.Bind(typeof(Data).GetField("b"), Expr.Constant(1.1)),
                                                Expr.Bind(typeof(Data).GetField("c"), Expr.Constant("Testing"))
                                            }
                                        )
                                    }
                                )
                            ),
                            AstUtils.YieldReturn(label, Expr.Constant(2)),
                            Expr.Throw(Expr.Constant(new DivideByZeroException()))
                        ),
                        Expr.Catch(
                            typeof(DivideByZeroException),
                            Expr.Block(
                                AstUtils.YieldReturn(label, Expr.Field(Expr.Field(Val3, "_d"), "a")),
                                AstUtils.YieldReturn(label, Expr.Field(Val3, "_x"))
                            )
                        )
                    )
                );

            ParameterExpression Result = Expr.Variable(typeof(bool), "Result");
            ParameterExpression Value = Expr.Variable(typeof(object), "Value");

            var Gen =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    label,
                    Tree,
                    new ParameterExpression[] { }
                );

            IEnumerator enumerator = (IEnumerator)Gen.Compile().DynamicInvoke();
            Expr e = Expr.Constant(enumerator, typeof(IEnumerator));

            EU.Enumerate(ref Expressions, new int[] { 1, 2, 5, -1 }, 4, e, ref Result, ref Value);

            var tree = Expr.Block(new[] { Result, Value, Val3 }, Expressions);
            V.Validate(tree);
            return tree;
        }

        private static Expr TE(Type t, string Message) {
            ConstructorInfo ci = t.GetConstructor(new Type[] { typeof(String) });
            Expression Ex = Expr.New(ci, Expr.Constant(Message));
            return Ex;
        }

#if !CLR2 // TODO: inline expression trees
        public abstract class A {
            public static void Test() {
                Expression<Func<A, Action>> ex = x => x.Foo;
                var f = ex.Compile();
                var a = f(null);
                a();
            }

            public abstract void Foo();
        }

        // Test        : Expression trees compiler allows to create a delegate to a null instance
        // Expected    : Raise TargetInvocationException
        // Notes: 
        // regression for bug # 522953
        [ETUtils.TestAttribute(ETUtils.TestState.Disabled, "RegessionTest 1", new string[] { "negative", "New", "Pri1", "regression" }, Exception = typeof(NullReferenceException))]
        public static Expr Misc4(EU.IValidator V) {
            EU.Throws<NullReferenceException>(() => { A.Test(); });
            return Expr.Empty();
        }
#endif

        private static Expr Print(String Message) {
            return Expr.Call(typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }), Expr.Constant(Message));
        }

        // Test   :  Linq: Unhandled UnaryPlus when doing UnaryPlus(Convert(Constant)) for @BUG 413822
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "RegessionTest @Bug 413822", new string[] { "positive", "New", "Pri1", "regression" })]
        public static Expr Misc7(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Value = Expr.Variable(typeof(int), "Value");
            Expression ET = Expression.MakeUnary(ExpressionType.UnaryPlus, Expression.Constant(5), typeof(int));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(5), ET));
            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;

        }

        // ArgumentMustBeArray
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ErrorMessages 1", new string[] { "negative", "errormessages", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr ErrorMessages1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression NotArr = Expr.Variable(typeof(List<int>), "NotArr");

            Expr tree =
                 EU.Throws<System.ArgumentException>(() =>
                 {
                     Expr.Block(
                          Expr.ArrayIndex(NotArr, Expr.Constant(1))
                     );
                 });

            return Expr.Empty();
        }

        public class TestMember {
            public int data;
            public static int staticField;

            public int ReadOnlyProp { get { return 1; } }
            private TestMember _a;
            public TestMember WriteOnlyProp { set { _a = value; } }

            public TestMember() { }
            public TestMember(int x) {
                data = x;
            }

            public static bool operator &(TestMember a, TestMember b) {
                return true;
            }
        }

        // ArgumentMustBeFieldInfoOrPropertInfo
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ErrorMessages 2", new string[] { "negative", "errormessages", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr ErrorMessages2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression obj = Expr.Variable(typeof(TestMember), "obj");

            MemberInfo mi = typeof(TestMember).GetConstructor(new Type[] { });

            Expr tree =
                EU.Throws<System.ArgumentException>(() =>
                {
                    Expr.Block(
                         Expr.Assign(obj, Expr.Constant(new TestMember())),
                         Expr.MemberInit(
                             Expr.New(typeof(TestMember)),
                             Expr.Bind(mi, Expr.Constant(1))
                         )
                    );
                });

            return Expr.Empty();
        }


        // ArgumentMustBeFieldInfoOrPropertInfoOrMethod
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ErrorMessages 3", new string[] { "negative", "errormessages", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr ErrorMessages3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var x = new { Name = "Dude", Age = 42 };

            var t = x.GetType();
            ConstructorInfo ci = t.GetConstructor(new Type[] { typeof(string), typeof(int) });
            List<MemberInfo> members = new List<MemberInfo>();
            members.Add(t.GetProperty("Name"));
            members.Add(ci);

            Expr tree =
                EU.Throws<System.ArgumentException>(() =>
                {
                    Expr.Block(
                         Expr.New(
                             ci,
                             new List<Expression>() { Expr.Constant("Bob"), Expr.Constant(10) },
                             members
                         )
                    );
                });

            return Expr.Empty();
        }

        // ArgumentMustBeInstanceMember
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ErrorMessages 4", new string[] { "negative", "errormessages", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr ErrorMessages4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var ci = typeof(TestMember).GetConstructor(new Type[] { typeof(int) });
            var members = typeof(TestMember).GetMembers(BindingFlags.Public | BindingFlags.Static);

            Expr tree =
                EU.Throws<System.ArgumentException>(() =>
                {
                    Expr.Block(
                         Expr.New(
                             ci,
                             new List<Expression>() { Expr.Constant(1) },
                             members
                         )
                    );
                });

            return Expr.Empty();
        }


        // ArgumentMustBeInteger
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ErrorMessages 5", new string[] { "negative", "errormessages", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr ErrorMessages5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expr tree =
                EU.Throws<System.ArgumentException>(() =>
                {
                    Expr.Block(
                         Expr.NewArrayBounds(typeof(int), new Expression[] { Expr.Constant(1.2) })
                    );
                });

            return Expr.Empty();
        }

        // ArgumentMustBeSingleDimensionalArrayType
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ErrorMessages 6", new string[] { "negative", "errormessages", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr ErrorMessages6(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression arr = Expr.Variable(typeof(int[,]), "arr");

            Expr tree =
                EU.Throws<System.ArgumentException>(() =>
                {
                    Expr.Block(
                         Expr.Assign(arr, Expr.NewArrayBounds(typeof(int), new Expression[] { Expr.Constant(2), Expr.Constant(3) })),
                         Expr.ArrayLength(arr)
                    );
                });

            return Expr.Empty();
        }


        // IncorrectTypeForTypeAs
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ErrorMessages 7", new string[] { "negative", "errormessages", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr ErrorMessages7(EU.IValidator V) {
            EU.Throws<InvalidOperationException>(() =>
            {
                Expr.NewArrayInit(typeof(DivideByZeroException), new Expression[] { Expr.Constant(new AccessViolationException()) });
            });

            return Expr.Empty();
        }

        // ExpressionTypeDoesNotMatchConstructorParameter
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ErrorMessages 8", new string[] { "negative", "errormessages", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr ErrorMessages8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expr obj =
                EU.Throws<System.ArgumentException>(() =>
                {
                    Expr.New(typeof(TestMember).GetConstructor(new Type[] { typeof(int) }), Expr.Constant(1.2));
                });

            return Expr.Empty();
        }

        // ArgumentMemberNotDeclOnType
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ErrorMessages 9", new string[] { "negative", "errormessages", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr ErrorMessages9(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var x = new { Name = "Dude", Age = 42 };
            var ci = x.GetType().GetConstructor(new Type[] { typeof(string), typeof(int) });
            List<MemberInfo> members = new List<MemberInfo>();
            members.Add(x.GetType().GetProperty("Name"));
            members.Add(typeof(TestClass).GetProperty("Value"));

            Expr obj =
                EU.Throws<System.ArgumentException>(() =>
                {
                    Expr.New(ci, new Expression[] { Expr.Constant("Bob"), Expr.Constant(1) }, members);
                });

            return Expr.Empty();
        }

        // ExpressionTypeDoesNotMatchParameter
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ErrorMessages 10", new string[] { "negative", "errormessages", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr ErrorMessages10(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression param = Expr.Variable(typeof(string), "param");
            EU.Throws<ArgumentException>(() => { Expr.Invoke(Expr.Lambda(Expr.Constant(1), param), new Expression[] { Expr.Constant(1) }); });

            return Expr.Empty();
        }

        // ExpressionTypeNotInvocable
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ErrorMessages 11", new string[] { "negative", "errormessages", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr ErrorMessages11(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expr obj = Expr.Constant(1);
            EU.Throws<System.ArgumentException>(() =>
            {
                Expr.Invoke(obj);
            });

            return Expr.Empty();
        }

        // IncorrectNumberOfLambdaArguments
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ErrorMessages 12", new string[] { "negative", "errormessages", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr ErrorMessages12(EU.IValidator V) {
            ParameterExpression param = Expr.Variable(typeof(string), "param");

            EU.Throws<InvalidOperationException>(() =>
            {
                Expr.Invoke(Expr.Lambda(Expr.Constant(1), param), new Expression[] { Expr.Constant("1"), Expr.Constant("2") });
            });

            return Expr.Empty();
        }

        // IncorrectNumberOfMembersForGivenConstructor
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ErrorMessages 13", new string[] { "negative", "errormessages", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr ErrorMessages13(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var x = new { };
            var ci = x.GetType().GetConstructor(new Type[] { });
            List<MemberInfo> members = new List<MemberInfo>();
            members.Add(x.GetType().GetProperty("Name"));
            members.Add(typeof(TestClass).GetProperty("Value"));

            Expr obj =
                EU.Throws<System.ArgumentException>(() =>
                {
                    Expr.New(ci, new Expression[] { }, members);
                });

            return Expr.Empty();
        }

        // IncorrectNumberOfArgumentsForMembers
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ErrorMessages 14", new string[] { "negative", "errormessages", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr ErrorMessages14(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var x = new { Name = "Dude", Age = 42 };
            var ci = x.GetType().GetConstructor(new Type[] { typeof(string), typeof(int) });
            List<MemberInfo> members = new List<MemberInfo>();
            members.Add(x.GetType().GetProperty("Name"));

            Expr obj =
                EU.Throws<System.ArgumentException>(() =>
                {
                    Expr.New(ci, new Expression[] { Expr.Constant("Bob"), Expr.Constant(1) }, members);
                });

            return Expr.Empty();
        }

        // MemberNotFieldOrProperty
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ErrorMessages 15", new string[] { "negative", "errormessages", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr ErrorMessages15(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var x = new { Name = "Dude", Age = 42 };
            var ci = x.GetType().GetConstructor(new Type[] { typeof(string), typeof(int) });
            EU.Throws<System.ArgumentException>(() =>
            {
                Expr.MakeMemberAccess(Expr.Constant(x), ci);
            });

            return Expr.Empty();
        }

        public static bool AndAlsoGeneric<T, V>(T x, V y) {
            return true;
        }

        // MethodContainsGenericParameters
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ErrorMessages 16", new string[] { "negative", "errormessages", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr ErrorMessages16(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            MethodInfo mi = typeof(Miscellaneous).GetMethod("AndAlsoGeneric");
            EU.Throws<System.ArgumentException>(() =>
            {
                Expr.AndAlso(Expr.Constant(true), Expr.Constant(1), mi);
            });

            return Expr.Empty();
        }

        // PropertyDoesNotHaveSetter
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ErrorMessages 17", new string[] { "negative", "errormessages", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr ErrorMessages17(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression obj = Expr.Variable(typeof(TestMember), "obj");

            MemberInfo mi = typeof(TestMember).GetProperty("ReadOnlyProp");

            Expr tree =
                 EU.Throws<System.ArgumentException>(() =>
                 {
                     Expr.Block(
                          Expr.Assign(obj, Expr.Constant(new TestMember())),
                          Expr.MemberInit(
                              Expr.New(typeof(TestMember)),
                              Expr.Bind(mi, Expr.Constant(2))
                          )
                  );
                 });

            return Expr.Empty();
        }

        // PropertyDoesNotHaveGetter
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ErrorMessages 18", new string[] { "negative", "errormessages", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr ErrorMessages18(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            EU.Throws<ArgumentException>(() =>
            {
                Expr.MemberBind(
                    typeof(TestMember).GetProperty("WriteOnlyProp"),
                    new MemberBinding[] {
                        Expr.Bind(typeof(TestMember).GetField("data"), Expr.Constant(1))
                    });
            });

            return Expr.Empty();
        }

        public struct TestStruct {
            public int data;
        }

        // OperatorNotImplementedForType
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ErrorMessages 19", new string[] { "negative", "errormessages", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr ErrorMessages19(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expr add =
            EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.Add(Expr.Constant(new TestStruct()), Expr.Constant(new TestStruct()));
            });

            return Expr.Empty();
        }

        // InstancePropertyWithSpecifiedParametersNotDefinedForType
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ErrorMessages 20", new string[] { "negative", "errormessages", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr ErrorMessages20(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            EU.Throws<ArgumentException>(() => { Expr.Property(Expr.Constant(new TestClass()), "NotHere", (Expression[])null); });

            return Expr.Empty();
        }

        public class NoCtor {
            private NoCtor() { }
        }

        // ListInitializerWithZeroMembers
        // IEnumerable ElementInit for code coverage
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ErrorMessages 21", new string[] { "negative", "errormessages", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr ErrorMessages21(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.ListInit(Expr.New(typeof(List<int>)), new List<ElementInit>() { });
            }));

            return Expr.Empty();
        }

        // UserDefinedOpMustHaveConsistentTypes
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ErrorMessages 22", new string[] { "negative", "errormessages", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr ErrorMessages22(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.AndAlso(Expr.Constant(new TestMember()), Expr.Constant(new TestMember()));
            }));

            return Expr.Empty();
        }

        // ControlCannotEnterExpression
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ErrorMessages 23", new string[] { "negative", "errormessages", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr ErrorMessages23(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(int), "Result");
            LabelTarget target = Expr.Label();

            Expr test =
                Expr.Block(
                     Expr.Goto(target),
                     Expr.Assign(Result, Expr.Block(Expr.Label(target), Expr.Constant(2)))
                );

            Expressions.Add(test);

            var tree = Expr.Block(new[] { Result }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true);
            return tree;
        }

        public int AndAlsoTest(int x, int y) {
            return 1;
        }

        // UserDefinedOperatorMustBeStatic
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ErrorMessages 24", new string[] { "negative", "errormessages", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr ErrorMessages24(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            EU.Throws<ArgumentException>(() => { Expr.AndAlso(Expr.Constant(1), Expr.Constant(2), typeof(Miscellaneous).GetMethod("AndAlsoTest")); });

            return Expr.Empty();
        }

        // UserDefinedOperatorMustBeStatic
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ErrorMessages 25", new string[] { "negative", "errormessages", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr ErrorMessages25(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.InvalidOperationException>(() =>
            {
                Expr.ReferenceNotEqual(Expr.Constant(new TestMember()), Expr.Constant(new TestClass()));
            }));

            return Expr.Empty();
        }

        // MethodWithMoreThanOneMatch
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ErrorMessages 26", new string[] { "negative", "errormessages", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr ErrorMessages26(EU.IValidator V) {
            EU.Throws<InvalidOperationException>(() =>
            {
                Expr.Call(typeof(Convert), "ToInt32", null, Expr.Constant("Test"));
            });

            return Expr.Empty();
        }

        public class Circle {
            public double Radius;
            public double Diameter { get; set; }
            public Point Center { get; set; }

            public List<Point> _mylist;
            public List<Point> MyList {
                get {
                    if (_mylist == null)
                        _mylist = new List<Point>();
                    return _mylist;
                }
                set {
                    _mylist = value;
                }
            }
        }

        public struct Point {
            public int x;
            public int y;
        }

        // CannotAutoInitializeValueTypeMemberThroughProperty
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ErrorMessages 27", new string[] { "negative", "errormessages", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr ErrorMessages27(EU.IValidator V)
        {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Variable = Expr.Variable(typeof(Circle), "Variable");

            Expr MyMemberBind =
                Expr.Block(
                    new ParameterExpression[] { Variable },
                    Expr.Assign(
                        Variable,
                        Expr.MemberInit(
                            Expr.New(typeof(Circle)),
                            new MemberBinding[] { 
                                Expr.Bind(typeof(Circle).GetMember("Radius")[0], Expr.Constant(5.2)),
                                Expr.MemberBind(
                                    typeof(Circle).GetMethod("get_Center"),
                                    new MemberBinding[] {
                                        Expr.Bind(typeof(Point).GetField("x"), Expr.Constant(1)),
                                        Expr.Bind(typeof(Point).GetField("y"), Expr.Constant(2))
                                    }
                                )
                            }
                        )
                    ),
                    Expr.Field(Expr.Property(Variable, "Center"), "y")
                );

            Expressions.Add(EU.GenAreEqual(Expr.Constant(2), MyMemberBind, "MemberBind 1"));

            ParameterExpression DoNotVisitTemp = Expression.Parameter(typeof(int), "dont_visit_node");
            var tree = Expr.Block(new[] { Variable, DoNotVisitTemp }, Expressions);
            V.ValidateException<InvalidOperationException>(tree, true);
            return tree;
        }

        // Regression for Dev10 bug: 664666
        // ListBind with empty ElementInit
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ListBind 1", new string[] { "positive", "errormessages", "miscellaneous", "Pri1" })]
        public static Expr ListBind1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Variable = Expr.Variable(typeof(Circle), "Variable");

            Expr MyMemberBind =
                Expr.Block(
                    new ParameterExpression[] { Variable },
                    Expr.Assign(
                        Variable,
                        Expr.MemberInit(
                            Expr.New(typeof(Circle)),
                            new MemberBinding[] { 
                                Expr.Bind(typeof(Circle).GetMember("Radius")[0], Expr.Constant(5.2)),
                                Expr.ListBind(
                                    typeof(Circle).GetMethod("get_MyList"),
                                    new ElementInit[] { }
                                )
                            }
                        )
                    ),
                    Expr.Field(Variable, "Radius")
                );

            Expressions.Add(EU.GenAreEqual(Expr.Constant(5.2), MyMemberBind, "MemberBind 1"));

            var tree = Expr.Block(new[] { Variable }, Expressions);
            V.Validate(tree);
            return tree;
        }
        #region ReduceVisitor
        public class ReduceVisitor : ExpressionVisitor {
            public List<string> Visited = new List<string>();

            public override Expression Visit(Expression node) {
                while (node.CanReduce) {
                    Visited.Add(node.NodeType.ToString());
                    node = node.ReduceAndCheck();
                }
                return base.Visit(node);
            }
        }
        #endregion

        // Reduce List/MemberInit
        // IEnumerable and null MethodInfo on ListInit for code coverage.
        // Bind with MethodInfo for code coverage
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Reduce 1", new string[] { "positive", "reduce", "miscellaneous", "Pri1" })]
        public static Expr Reduce1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Variable = Expr.Variable(typeof(Circle), "Variable");
            ParameterExpression TestList = Expr.Variable(typeof(List<int>), "TestList");

            var Block = Expr.Block(Expr.TryFinally(Expr.Constant(0), Expr.Empty()), Expr.Constant(1));

            Expr e =
                Expr.Block(
                    new ParameterExpression[] { Variable },
                    Expr.Assign(
                        TestList,
                        Expr.TryFinally(
                            Expr.ListInit(Expr.New(typeof(List<int>)), (MethodInfo)null, new List<Expression>() { Expr.Constant(1), Expr.Constant(2), Expr.Constant(3) }),
                            Expr.Empty()
                        )
                    ),
                    Expr.Assign(
                        Variable,
                        Expr.TryFinally(
                            Expr.MemberInit(
                                Expr.New(typeof(Circle)),
                                new MemberBinding[] { 
                                    Expr.Bind(typeof(Circle).GetMember("Radius")[0], Expr.Block(Block, Expr.Constant(5.2))),
                                    Expr.Bind(typeof(Circle).GetMethod("get_Diameter"), Expr.Block(Block, Expr.Constant(10.4))),
                                    Expr.ListBind(
                                        typeof(Circle).GetMethod("get_MyList"),
                                        new ElementInit[] { Expr.ElementInit(typeof(List<Point>).GetMethod("Add"), Expr.Block(Block, Expr.Constant(new Point()))) }
                                    )
                                }
                            ),
                            Expr.Empty()
                        )
                    )
                );

            var tree = Expr.Lambda<Func<int>>(Expr.Block(new[] { Variable, TestList }, e, Expr.Constant(1)));

            V.Validate(tree, f =>
            {
                var visitor = new EU.ReduceRewriter();
                visitor.Visit(tree);
                EU.Equal(2, visitor.Visited.Count);
                EU.Equal(true, visitor.Visited.Contains("MemberInit"));
                EU.Equal(true, visitor.Visited.Contains("ListInit"));
            });

            return tree;
        }

        // IEnumerable value type for ListInit
        public struct MyList : IEnumerable {
            public List<int> _data;
            public int Add(int x) {
                if (_data == null) {
                    _data = new List<int>();
                }
                _data.Add(x);
                return 1; // return non-void to hit unhit code
            }
            public int Get(int x) {
                return _data[x];
            }
            public IEnumerator GetEnumerator() {
                return (_data as IEnumerable).GetEnumerator();
            }
        }

        // ListInit with NewExpression of value type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ListInit 100", new string[] { "positive", "listinit", "miscellaneous", "Pri1" })]
        public static Expr ListInit100(EU.IValidator V) {
            var tree = 
                Expr.Lambda<Func<int>>(
                    Expr.Call(
                        Expr.ListInit(
                            Expr.New(typeof(MyList)),
                            new ElementInit[] { Expr.ElementInit(typeof(MyList).GetMethod("Add"), Expr.Constant(10)) }
                        ),
                        typeof(MyList).GetMethod("Get"),
                        new Expression[] { Expr.Constant(0) }
                    )
                );

            V.Validate(tree, f =>
            {
                EU.Equal(f(), 10);
            });
            return tree;
        }

        // Hit VisitBlock and VisitCatch in ExpressionQuoter
        // TODO: Third CatchBlock is commented out until filters work again.
        public static int AddAssign(ref int x, int y) {
            return x += y;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ExpressionQuoter 1", new string[] { "positive", "quote", "miscellaneous", "Pri1" })]
        public static Expr ExpressionQuoter1(EU.IValidator V) {
            //List<Expression> Expressions = new List<Expression>();
            ParameterExpression x = Expression.Parameter(typeof(int), "x");
            ParameterExpression y = Expression.Parameter(typeof(int), "y");
            ParameterExpression Ex = Expr.Variable(typeof(DivideByZeroException), "Ex");
            var e = 
                Expression.Lambda<Func<int, Expression>>(
                    Expression.Quote(
                        Expression.Lambda<Func<int, int>>(
                            Expr.Block(
                                Expr.Block( // block with variables
                                    new [] { x, y },
                                    Expr.Add(x, Expr.Constant(1))
                                ),
                                Expr.Block( // block with no variables
                                    Expr.Add(Expr.Constant(1), Expr.Constant(2))
                                ),
                                Expr.TryCatch(
                                    Expr.Throw(Expr.New(typeof(DivideByZeroException))),
                                    new CatchBlock[] {
                                        Expr.Catch(Ex, Expr.Block(Expr.TypeEqual(x, typeof(int)), Expr.Empty())),
                                        Expr.Catch(typeof(ArrayTypeMismatchException), Expr.Empty()),
                                        //Expr.Catch(typeof(Exception), Expr.Block(Expr.Empty()), Expr.TypeEqual(x, typeof(int)))
                                    }
                                ),
                                Expression.Call(typeof(Miscellaneous).GetMethod("AddAssign"), x, y)
                            ),
                            y
                        )
                    ),
                    x
                );
                
            // Trying to enable 3rd CatchBlock...
            // Need to CompileToMethod the interior Lambda
            //MethodInfo ctm = typeof(ETUtils.ExpressionUtils.TestValidator).GetMethod("TryCompileTreeAsMethod");
            //ParameterExpression LM = Expr.Variable(typeof(Func<int,int>), "LM");
            //var execExpr =
            //    Expr.Call(
            //        Expr.Constant(V),
            //        ctm,
            //        Expr.Convert(
            //            Expr.Call(
            //                e,
            //                typeof(Func<int,Expression>).GetMethod("Invoke", new Type[] { typeof(int) }),
            //                Expr.Constant(123)
            //            ),
            //            typeof(Expression<Func<int, int>>)
            //        )
            //    );
            //Expressions.Add(Expr.Assign(LM, Expr.Convert(execExpr, typeof(Func<int,int>))));
            //Expressions.Add(EU.GenAreEqual(Expr.Constant(234), Expr.Invoke(LM, Expr.Constant(111))));
            //Expressions.Add(EU.GenAreEqual(Expr.Constant(456), Expr.Invoke(LM, Expr.Constant(222))));
            //Expressions.Add(EU.GenAreEqual(Expr.Constant(789), Expr.Invoke(LM, Expr.Constant(333))));
            
            //var tree = Expr.Block(new [] { LM }, Expressions);
            //V.Validate(tree, true);

            V.Validate(e, f =>
            {
                Func<int, int> z = ((Expression<Func<int, int>>)f(123)).Compile();
                int a = z(111);
                int b = z(222);
                int c = z(333);
                EU.Equal(a, 234);
                EU.Equal(b, 456);
                EU.Equal(c, 789);
            });

            return e;
        }

        // Create a tree that causes the StackSpiller to use a Copy action rather than a full SpillStack action
        // StackSpilling is triggered by statements that require an empty IL stack when execution begins (Try/Catch is a simple case)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "SpillTest 1", new string[] { "positive", "spiller", "miscellaneous", "Pri1" })]
        public static Expr SpillTest1(EU.IValidator V) {
            // This block is a reducible expression that will trigger at least a Rewrite.Copy action.
            ParameterExpression temp = Expr.Variable(typeof(int), "temp");
            var Block = Expr.Block(new [] { temp }, Expr.AddAssign(temp, Expr.Constant(1)));

            // List/Member Init/Bind expressions which will be rewritten by code we're not hitting elsewhere
            ParameterExpression TestCircle = Expr.Variable(typeof(Circle), "TestCircle");
            ParameterExpression TestList = Expr.Variable(typeof(List<int>), "TestList");
            Expr e =
                Expr.Block(
                    new ParameterExpression[] { TestCircle, TestList },
                    Expr.Assign(
                        TestList,
                        Expr.ListInit(Expr.New(typeof(List<int>)), (MethodInfo)null, new List<Expression>() { Expr.Block(Block, Expr.Constant(1)) })
                    ),
                    Expr.Assign(
                        TestCircle,
                        Expr.MemberInit(
                            Expr.New(typeof(Circle)),
                            new MemberBinding[] { 
                                Expr.Bind(typeof(Circle).GetMember("Radius")[0], Expr.Block(Block, Expr.Constant(5.2))),
                                Expr.ListBind(
                                    typeof(Circle).GetMethod("get_MyList"),
                                    new ElementInit[] { Expr.ElementInit(typeof(List<Point>).GetMethod("Add"), Expr.Block(Block, Expr.Constant(new Point()))) }
                                )
                            }
                        )
                    ),
                    Expr.Constant(1)
                );

            // This inner lambda has reducible nodes, so the tree will be rewritten but only copied because no
            // other expressions in it require a full stackspill (ex TryExpression).
            ParameterExpression a = Expr.Variable(typeof(int), "a");

            var InnerLambda =
                Expr.Lambda(
                    Expr.Block(
                        new [] { a },
                        Expr.AddAssign(a, e)
                    )
                );

            var tree = 
                Expr.Lambda<Func<int>>(
                    Expr.Block(
                        new [] { TestCircle, TestList },
                        InnerLambda,
                        Expr.Constant(10)
                    ),
                    new ParameterExpression[] {}
                );

            V.Validate(tree, f =>
            {
                EU.Equal(f(), 10);
            });

            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Accept 1", new string[] { "positive", "Accept", "miscellaneous", "Pri1" })]
        public static Expr Accept1(EU.IValidator V)
        {
            var Ex = new Accept1Class();
            Ex.X = Expression.Add(Expression.Constant(1), Expression.Constant(2));

            var visit = new AcceptMyVisitor();
            Accept1Class res = (Accept1Class) visit.Visit(Ex);
            if (res == Ex || res.X != Ex.X)
                throw new Exception("Visitor did not create a new expression with the same child");

            return Expr.Empty();
            
        }

        class Accept1Class : Expression
        {
            protected override Expression Accept(ExpressionVisitor visitor)
            {
                if (visitor.GetType() == typeof(AcceptMyVisitor))
                {
                    return ((AcceptMyVisitor)visitor).Accept1Class(this);
                }
                return base.Accept(visitor);
            }

            public override bool CanReduce
            {
                get
                {
                    return true;
                }
            }

            public override ExpressionType NodeType
            {
                get
                {
                    return ExpressionType.Extension;
                }
            }

            public override Expression Reduce()
            {
                return Expression.Constant(2);
            }

            public override Type Type
            {
                get
                {
                    return typeof(int);
                }
            }

            public BinaryExpression X;
        }

        class AcceptMyVisitor : ExpressionVisitor
        {
            public Accept1Class Accept1Class(Accept1Class node)
            {
                var y = new Accept1Class();
                y.X = (BinaryExpression )Visit(((Accept1Class)node).X);
                return y;
            }
        }


        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Accept 2", new string[] { "positive", "Accept", "miscellaneous", "Pri1" })]
        public static Expr Accept2(EU.IValidator V)
        {
            var Ex = new Accept2Class();
            Ex.X = Expression.Add(Expression.Constant(1), Expression.Constant(2));

            var visit = new AcceptMyVisitor2();
            try
            {
                visit.Visit(Ex);
                throw new Exception("Exception expected");
            }
            catch (NullReferenceException)
            {
                return Expr.Empty();
            }
        }

        class Accept2Class : Expression
        {
            protected override Expression Accept(ExpressionVisitor visitor)
            {
                return base.Accept(null);
            }

            public override bool CanReduce
            {
                get
                {
                    return true;
                }
            }

            public override ExpressionType NodeType
            {
                get
                {
                    return ExpressionType.Extension;
                }
            }

            public override Expression Reduce()
            {
                return Expression.Constant(2);
            }

            public override Type Type
            {
                get
                {
                    return typeof(int);
                }
            }

            public BinaryExpression X;
        }

        class AcceptMyVisitor2 : ExpressionVisitor
        {
        }



    }
}
