#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;

namespace ETScenarios.Miscellaneous {
    using EU = ETUtils.ExpressionUtils;
    using Expr = Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public class Generator {
        // Generator with yield to different LabelTarget object
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Generator 1", new string[] { "negative", "generator", "miscellaneous", "Pri2" }, Exception = typeof(InvalidOperationException), Priority = 2)]
        public static Expr Generator1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            LabelTarget target = Expr.Label(typeof(int));
            LabelTarget BadTarget = Expr.Label(typeof(int));

            var Gen =
                AstUtils.Generator(
                    target,
                    Expr.Block(
                        AstUtils.YieldReturn(target, Expr.Constant(1)),
                        AstUtils.YieldReturn(BadTarget, Expr.Constant(2)),
                        AstUtils.YieldReturn(target, Expr.Constant(3))
                    )
                );

            Expressions.Add(Gen);

            var tree = Expr.Block(Expressions);
            V.ValidateException<InvalidOperationException>(tree, true);
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

        // Use and assign to MemberAccess, Index inside Generator
        // Use Expr.New in a Generator
        // Use MemberInit and ListInit in a Generator
        // Regression for Dev10 Bug 568425
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Generator 2", new string[] { "positive", "generator", "miscellaneous", "Pri2" }, Priority = 2)]
        public static Expr Generator2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(bool), "Result");
            ParameterExpression Value = Expr.Variable(typeof(object), "Value");
            ParameterExpression TestObj = Expr.Variable(typeof(TestClass), "TestObj");
            ParameterExpression TestList = Expr.Variable(typeof(List<int>), "TestList");
            ParameterExpression TestMember = Expr.Variable(typeof(StrongBox<int>), "TestMember");

            LabelTarget target = Expr.Label(typeof(int));

            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { });
            PropertyInfo pi = typeof(TestClass).GetProperty("Item");

            var Gen =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    target,
                    Expr.Block(
                        new[] { TestObj, TestList, TestMember },
                        Expr.Assign(TestObj, Expr.New(ci)),
                        AstUtils.YieldReturn(target, Expr.Constant(1)),
                        // assign and yield a property value
                        Expr.Assign(Expr.Property(TestObj, "Value"), Expr.Constant(10)),
                        AstUtils.YieldReturn(target, Expr.Property(TestObj, "Value")),
                        // assign and yield an indexer value
                        Expr.Assign(Expr.Property(TestObj, pi, new Expression[] { Expr.Constant(1) }), Expr.Constant(-1)),
                        AstUtils.YieldReturn(target, Expr.Property(TestObj, pi, new Expression[] { Expr.Constant(1) })),
                        // use ListInit
                        Expr.Assign(TestList, Expr.ListInit(Expr.New(typeof(List<int>)), new Expression[] { Expr.Constant(1), Expr.Constant(2), Expr.Constant(3) })),
                        AstUtils.YieldReturn(target, Expr.Property(TestList, "Count")),
                        // use MemberInit
                        Expr.Assign(TestMember, Expr.MemberInit(Expr.New(typeof(StrongBox<int>)), new MemberBinding[] { Expr.Bind(typeof(StrongBox<int>).GetField("Value"), Expr.Constant(42)) })),
                        AstUtils.YieldReturn(target, Expr.Field(TestMember, "Value")),
                        AstUtils.YieldReturn(target, Expr.Property(TestObj, "Value"))
                    ),
                    new ParameterExpression[] { }
                );

            Expressions.Add(Gen);

            IEnumerator enumerator = (IEnumerator)Gen.Compile().DynamicInvoke();
            Expr e = Expr.Constant(enumerator, typeof(IEnumerator));

            EU.Enumerate(ref Expressions, new int[] { 1, 10, -1, 3, 42, 10 }, 6, e, ref Result, ref Value);

            var tree = Expr.Block(new[] { Result, Value, TestObj, TestList, TestMember }, Expressions);
            V.Validate(tree);
            return tree;
        }

        public static Expr TestMethod(int x) {
            return Expr.Constant(x * x);
        }

        // Use Expr.Dynamic and TypeEqual inside a Generator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Generator 3", new string[] { "positive", "generator", "miscellaneous", "Pri2" }, Priority = 2)]
        public static Expr Generator3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(bool), "Result");
            ParameterExpression Value = Expr.Variable(typeof(object), "Value");

            LabelTarget label = Expr.Label(typeof(int));

            ParameterExpression DynamicResult = Expr.Variable(typeof(string), "DynamicResult");
            CallSiteBinder myBinder = new MyBinder();

            MethodInfo mi = typeof(Generator).GetMethod("TestMethod");

            LambdaExpression LM = Expr.Lambda((Expression)mi.Invoke(null, new object[] { 3 }));

            var Gen =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    label,
                    Expr.Block(
                        new[] { DynamicResult },
                        AstUtils.YieldReturn(label, Expr.Constant(1)),
                        Expr.Assign(DynamicResult, Expr.MakeDynamic(typeof(Func<CallSite, int, string>), myBinder, new Expression[] { Expr.Constant(1) })),
                        Expr.Condition(
                            Expr.Equal(DynamicResult, Expr.Constant("Success1")),
                            AstUtils.YieldReturn(label, Expr.Constant(2)),
                            AstUtils.YieldReturn(label, Expr.Constant(-1))
                        ),
                        Expr.Condition(
                            Expr.Equal(Expr.Constant(true), Expr.TypeEqual(DynamicResult, typeof(string))),
                            AstUtils.YieldReturn(label, Expr.Constant(3)),
                            AstUtils.YieldReturn(label, Expr.Constant(-2))
                        ),
                        AstUtils.YieldReturn(label, Expr.Invoke(LM, new Expression[] { }))
                    ),
                    new ParameterExpression[] { }
                );

            IEnumerator enumerator = (IEnumerator)Gen.Compile().DynamicInvoke();
            Expr e = Expr.Constant(enumerator, typeof(IEnumerator));

            EU.Enumerate(ref Expressions, new int[] { 1, 2, 3, 9 }, 4, e, ref Result, ref Value);

            var tree = Expr.Block(new[] { Result, Value, DynamicResult }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Rethrow in a lambda in a generator
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Generator 4", new string[] { "positive", "generator", "miscellaneous", "Pri2" }, Priority = 2)]
        public static Expr Generator4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(bool), "Result");
            ParameterExpression Value = Expr.Variable(typeof(object), "Value");

            LabelTarget label = Expr.Label(typeof(int));

            LambdaExpression LM =
                Expr.Lambda(
                    Expr.TryCatch(
                        Expr.Block(
                            Expr.Throw(Expr.Constant(new DivideByZeroException()))
                        ),
                        Expr.Catch(
                            typeof(DivideByZeroException),
                            Expr.Block(
                                Expr.Rethrow()
                            )
                        )
                    )
                );

            var Gen =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    label,
                    Expr.Block(
                        AstUtils.YieldReturn(label, Expr.Constant(1)),
                        Expr.TryCatch(
                            Expr.Block(Expr.Invoke(LM, new Expression[] { }), Expr.Empty()),
                            Expr.Catch(typeof(DivideByZeroException), Expr.Block(AstUtils.YieldReturn(label, Expr.Constant(-1)), Expr.Empty()))
                        ),
                        AstUtils.YieldReturn(label, Expr.Constant(2))
                    ),
                    new ParameterExpression[] { }
                );

            IEnumerator enumerator = (IEnumerator)Gen.Compile().DynamicInvoke();
            Expr e = Expr.Constant(enumerator, typeof(IEnumerator));

            EU.Enumerate(ref Expressions, new int[] { 1, -1, 2 }, 3, e, ref Result, ref Value);

            var tree = Expr.Block(new[] { Result, Value }, Expressions);
            V.Validate(tree);
            return tree;
        }
    }
}
