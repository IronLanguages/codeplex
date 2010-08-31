#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ETScenarios.Miscellaneous {
    using EU = ETUtils.ExpressionUtils;
    using Expr = Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;
    using ETUtils;

    public class DebugView {
        private static string sep = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator;

        // Binary Ops
        // Regression for Dev10 672090
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DebugView 1", new string[] { "positive", "debugview", "miscellaneous", "Pri2", "FullTrustOnly" })]
        public static Expr DebugView1(EU.IValidator V) {
            ParameterExpression IntValue = Expr.Variable(typeof(int), "IntValue");
            ParameterExpression BoolValue = Expr.Variable(typeof(bool), "BoolValue");

            Expr tree =
                Expr.Block(
                    new[] { IntValue, BoolValue },
                    Expr.Add(Expr.Constant(1), Expr.Constant(2)),
                    Expr.AddAssign(IntValue, Expr.Add(Expr.Constant(2), Expr.Constant(3))),
                    Expr.AddAssignChecked(IntValue, Expr.AddAssign(IntValue, Expr.Constant(4))),
                    Expr.AddChecked(Expr.Constant(5), Expr.Constant(6), ((Func<int, int, int>)((x, y) => { return x; })).Method),

                    Expr.Subtract(Expr.Constant(1), Expr.Constant(2)),
                    Expr.SubtractAssign(IntValue, Expr.Subtract(Expr.Constant(2), Expr.Constant(3))),
                    Expr.SubtractAssignChecked(IntValue, Expr.SubtractAssign(IntValue, Expr.Constant(4))),
                    Expr.SubtractChecked(Expr.Constant(5), Expr.Constant(6), ((Func<int, int, int>)((x, y) => { return x; })).Method),

                    Expr.Multiply(Expr.Constant(1), Expr.Constant(2)),
                    Expr.MultiplyAssign(IntValue, Expr.Multiply(Expr.Constant(2), Expr.Constant(3))),
                    Expr.MultiplyAssignChecked(IntValue, Expr.MultiplyAssign(IntValue, Expr.Constant(4))),
                    Expr.MultiplyChecked(Expr.Constant(5), Expr.Constant(6), ((Func<int, int, int>)((x, y) => { return x; })).Method),

                    Expr.Divide(Expr.Constant(1), Expr.Constant(2)),
                    Expr.DivideAssign(IntValue, Expr.Divide(Expr.Constant(2), Expr.Constant(3))),

                    Expr.Modulo(Expr.Constant(1), Expr.Constant(2)),
                    Expr.ModuloAssign(IntValue, Expr.Modulo(Expr.Constant(2), Expr.Constant(3))),

                    Expr.AndAssign(BoolValue, Expr.AndAlso(Expr.Constant(true), Expr.And(Expr.Constant(true), Expr.Constant(false)))),
                    Expr.OrAssign(BoolValue, Expr.OrElse(Expr.Constant(true), Expr.Or(Expr.Constant(true), Expr.Constant(false)))),
                    Expr.ExclusiveOrAssign(BoolValue, Expr.ExclusiveOr(Expr.Constant(true), Expr.OrElse(Expr.Constant(true), Expr.Constant(false)))),

                    Expr.LessThan(Expr.Constant(1), Expr.Constant(2)),
                    Expr.GreaterThan(IntValue, Expr.AddAssign(IntValue, Expr.Constant(3))),
                    Expr.Equal(Expr.Constant((int?)1, typeof(int?)), Expr.Constant((int?)2, typeof(int?)), true, ((Func<int?, int?, bool>)((x, y) => { return true; })).Method),
                    Expr.GreaterThanOrEqual(Expr.Convert(IntValue, typeof(Double)), Expr.Constant(2.1)),
                    Expr.NotEqual(Expr.Constant(true), Expr.LessThan(Expr.Constant(1), IntValue)),

                    Expr.Coalesce(Expr.Constant("a"), Expr.Constant("b")),

                    Expr.LeftShift(Expr.Constant(1), IntValue),
                    Expr.RightShift(Expr.LeftShift(Expr.Constant(2), Expr.Constant(3)), Expr.Constant((int?)1, typeof(int?))),
                    Expr.LeftShiftAssign(IntValue, Expr.RightShiftAssign(IntValue, Expr.Constant(-1)))
                );

            #region str
            string str =
@".Block(
    System.Int32 $IntValue,
    System.Boolean $BoolValue) {
    1 + 2;
    $IntValue += 2 + 3;
    $IntValue #+= ($IntValue += 4);
    5 #+ 6;
    1 - 2;
    $IntValue -= 2 - 3;
    $IntValue #-= ($IntValue -= 4);
    5 #- 6;
    1 * 2;
    $IntValue *= 2 * 3;
    $IntValue #*= ($IntValue *= 4);
    5 #* 6;
    1 / 2;
    $IntValue /= 2 / 3;
    1 % 2;
    $IntValue %= 2 % 3;
    $BoolValue &= True && True & False;
    $BoolValue |= True || True | False;
    $BoolValue ^= True ^ (True || False);
    1 < 2;
    $IntValue > ($IntValue += 3);
    .Constant<System.Nullable`1[System.Int32]>(1) == .Constant<System.Nullable`1[System.Int32]>(2);
    (System.Double)$IntValue >= 2" + sep + @"1D;
    True != 1 < $IntValue;
    ""a"" ?? ""b"";
    1 << $IntValue;
    (2 << 3) >> .Constant<System.Nullable`1[System.Int32]>(1);
    $IntValue <<= ($IntValue >>= -1)
}";
            #endregion str

            EU.Equal(str.Trim(), tree.DebugView().Trim());

            return Expr.Empty();
        }

        // Unary Ops
        // Regression for Dev10 672090
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DebugView 2", new string[] { "positive", "debugview", "miscellaneous", "Pri2", "FullTrustOnly" })]
        public static Expr DebugView2(EU.IValidator V) {
            ParameterExpression DoubleValue = Expr.Variable(typeof(double), "DoubleValue");

            Expr tree =
                Expr.Block(
                    new[] { DoubleValue },
                    Expr.Increment(Expr.Constant(1)),
                    Expr.PreIncrementAssign(DoubleValue),
                    Expr.PostDecrementAssign(DoubleValue),
                    Expr.Decrement(DoubleValue, ((Func<double, double>)(x => { return x + 2; })).Method),
                    Expr.ConvertChecked(DoubleValue, typeof(float)),
                    Expr.Convert(Expr.Convert(Expr.Constant(1), typeof(double)), typeof(long)),
                    Expr.IsTrue(Expr.Constant(false)),
                    Expr.Negate(Expr.Constant(-1)),
                    Expr.NegateChecked(Expr.Constant((float)-9.1, typeof(float))),
                    Expr.Not(Expr.Constant(false), ((Func<bool, bool>)(x => { return !x; })).Method),
                    Expr.OnesComplement(Expr.Add(Expr.Constant(2), Expr.Constant(4))),
                    Expr.TypeAs(Expr.Convert(DoubleValue, typeof(double?)), typeof(decimal?)),
                    Expr.TypeEqual(Expr.Add(DoubleValue, Expr.Constant(1.1)), typeof(float)),
                    Expr.TypeIs(Expr.TypeEqual(Expr.Constant(2), typeof(int)), typeof(bool))
                );

            #region str
            string str =
@".Block(System.Double $DoubleValue) {
    .Increment(1);
    ++$DoubleValue;
    $DoubleValue--;
    .Decrement($DoubleValue);
    #(System.Single)$DoubleValue;
    (System.Int64)((System.Double)1);
    .IsTrue(False);
    -(-1);
    #-(-9" + sep + @"1F);
    !False;
    ~(2 + 4);
    (System.Nullable`1[System.Double])$DoubleValue .As System.Nullable`1[System.Decimal];
    $DoubleValue + 1" + sep + @"1D .TypeEqual System.Single;
    (2 .TypeEqual System.Int32) .Is System.Boolean
}";
            #endregion str

            EU.Equal(str.Trim(), tree.DebugView().Trim());

            return Expr.Empty();
        }

        // Control Flow
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DebugView 3", new string[] { "positive", "debugview", "miscellaneous", "Pri2", "FullTrustOnly" })]
        public static Expr DebugView3(EU.IValidator V) {
            ParameterExpression Ex = Expr.Variable(typeof(Exception), "Ex");
            ParameterExpression StringValue = Expr.Variable(typeof(string), "StringValue");
            ParameterExpression IntValue = Expr.Variable(typeof(int), "IntValue");

            LabelTarget breakTarget = Expr.Label();

            #region tree
            Expr tree =
                Expr.Block(
                    new[] { IntValue },
                    Expr.TryCatchFinally(
                        Expr.Block(
                            Expr.Condition(
                                Expr.Equal(StringValue, Expr.Constant("Hi")),
                                Expr.IfThen(
                                    Expr.LessThan(Expr.Constant(1), Expr.Constant(2)),
                                    Expr.Constant("5")
                                ),
                                Expr.Constant("Fail"),
                                typeof(void)
                            ),
                            Expr.Loop(
                                Expr.Block(
                                    Expr.Call(typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }), Expr.Constant("A")),
                                    Expr.Break(breakTarget)
                                ),
                                breakTarget
                            ),
                            Expr.Constant(false)
                        ),
                        Expr.Constant("Finally"),
                        new CatchBlock[] { 
                            Expr.Catch(typeof(DivideByZeroException), Expr.Equal(Expr.Constant(3), Expr.Add(Expr.Constant(1), Expr.Constant(2)))),
                            Expr.Catch(typeof(ArrayTypeMismatchException), Expr.TypeEqual(Expr.Constant(5), typeof(int?)), Expr.Block(Expr.TypeIs(Ex, typeof(Exception)))),
                            Expr.Catch(Ex, Expr.Block(Expr.Rethrow(), Expr.Constant(true)))
                        }
                    ),
                    Expr.TryFault(
                        Expr.Switch(
                            typeof(void),
                            IntValue,
                            Expr.Block(
                                typeof(void),
                                Expr.Constant("DefaultCase")
                            ),
                            null,
                            new SwitchCase[] {
                                Expr.SwitchCase(
                                    Expr.TryFinally(
                                        Expr.Constant("TryThis"),
                                        Expr.Constant("AndFinally")
                                    ),
                                    Expr.Constant(1), Expr.Constant(2)
                                ),
                                Expr.SwitchCase(
                                    Expr.GreaterThan(Expr.Constant(1), Expr.Constant(2)),
                                    Expr.Constant(5)
                                )
                            }
                        ),
                        Expr.Throw(Expr.New(typeof(Exception).GetConstructor(new Type[] { })))
                    )
                );
            #endregion tree

            #region str
            string str =
@".Block(System.Int32 $IntValue) {
    .Try {
        .Block() {
            .If ($StringValue == ""Hi"") {
                .If (1 < 2) {
                    ""5""
                } .Else {
                    .Default(System.Void)
                }
            } .Else {
                ""Fail""
            };
            .Loop  {
                .Block() {
                    .Call System.Console.WriteLine(""A"");
                    .Break #Label1 { }
                }
            }
            .LabelTarget #Label1:;
            False
        }
    } .Catch (System.DivideByZeroException) {
        3 == 1 + 2
    } .Catch (System.ArrayTypeMismatchException) .If (.Block() {
        $Ex .Is System.Exception
    }) {
        5 .TypeEqual System.Nullable`1[System.Int32]
    } .Catch (System.Exception $Ex) {
        .Block() {
            .Rethrow;
            True
        }
    } .Finally {
        ""Finally""
    };
    .Try {
        .Switch ($IntValue) {
        .Case (1):
        .Case (2):
                .Try {
                    ""TryThis""
                } .Finally {
                    ""AndFinally""
                }
        .Case (5):
                1 > 2
        .Default:
                .Block<System.Void>() {
                    ""DefaultCase""
                }
        }
    } .Fault {
        .Throw .New System.Exception()
    }
}";
            #endregion str

            EU.Equal(str.Trim(), tree.DebugView().Trim());

            return Expr.Empty();
        }

        #region types
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

        public class Circle {
            public double Radius;
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

        #endregion types

        // Object Initialization
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DebugView 4", new string[] { "positive", "debugview", "miscellaneous", "Pri2", "FullTrustOnly" })]
        public static Expr DebugView4(EU.IValidator V) {
            ParameterExpression Val3 = Expr.Variable(typeof(BindingTestClass), "Val3");
            ParameterExpression Variable = Expr.Variable(typeof(Circle), "Variable");
            ParameterExpression Arr = Expr.Parameter(typeof(int[,]), "Arr");
            ParameterExpression JaggedArr = Expr.Parameter(typeof(int?[]), "");
            ParameterExpression TestList = Expr.Variable(typeof(List<int>), "TestList");

            MemberInfo memi = typeof(BindingTestClass).GetMember("_x")[0];
            var b1 = Expr.Bind(memi, Expr.Constant(3));
            MemberInfo memi2 = typeof(BindingTestClass).GetMember("_y")[0];
            var b2 = Expr.Bind(memi2, Expr.Constant("Test"));

            LabelTarget label = Expr.Label(typeof(int));

            #region tree
            Expr Tree =
                Expr.Block(
                    new[] { Arr, Val3, TestList },
                    Expr.TryCatch(
                        Expr.Block(
                            Expr.Assign(Arr, Expr.NewArrayBounds(typeof(int), new[] { Expr.Constant(3), Expr.Constant(2) })),
                            Expr.Block(
                                new [] { JaggedArr },
                                Expr.Assign(
                                    JaggedArr,
                                    Expr.NewArrayInit(typeof(int?), new Expression[] { Expr.Constant(-1, typeof(int?)), Expr.Constant(-2, typeof(int?)), Expr.Constant(-3, typeof(int?)) })
                                ),
                                Expr.Assign(Expr.ArrayAccess(JaggedArr, Expr.Constant(1)), Expr.Constant(1, typeof(int?))),
                                Expr.Assign(Expr.ArrayAccess(JaggedArr, Expr.Constant(2)), Expr.Constant(2, typeof(int?)))
                            ),
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
                                            new MemberBinding[] { // binding fields of _d member of type Data
                                                Expr.Bind(typeof(Data).GetField("a"), Expr.Constant(5)),
                                                Expr.Bind(typeof(Data).GetField("b"), Expr.Constant(1.1)),
                                                Expr.Bind(typeof(Data).GetField("c"), Expr.Constant("Testing"))
                                            }
                                        )
                                    }
                                )
                            ),
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
                                                new ElementInit[] {
                                                    Expr.ElementInit(typeof(List<Point>).GetMethod("Add"), Expr.New(typeof(Point)))
                                                }
                                            )
                                        }
                                   )
                               ),
                               Expr.Field(Variable, "Radius")
                            ),
                            AstUtils.YieldReturn(label, Expr.Constant(2)),
                            Expr.Assign(TestList, Expr.ListInit(Expr.New(typeof(List<int>)), new Expression[] { Expr.Constant(1), Expr.Constant(2), Expr.Constant(3) })),
                            Expr.Throw(Expr.Constant(Expr.New(typeof(DivideByZeroException).GetConstructor(new Type[] {}))))
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

            var Gen =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    label,
                    Tree,
                    new ParameterExpression[] { }
                );
            #endregion tree

            #region str
            string str =
@".Lambda #Lambda1<" + typeof(Func<>).FullName + @"[System.Collections.IEnumerator]>() {
    .Extension<Microsoft.Scripting.Ast.GeneratorExpression> {
        .Call Microsoft.Scripting.Runtime.ScriptingRuntimeHelpers.MakeGenerator(.Block(
                System.DivideByZeroException $var1,
                System.Int32[,] $Arr,
                ETScenarios.Miscellaneous.DebugView+BindingTestClass $Val3,
                System.Collections.Generic.List`1[System.Int32] $TestList) {
                .Lambda #Lambda2<Microsoft.Scripting.Runtime.GeneratorNext`1[System.Int32]>
            })
    }
}

.Lambda #Lambda2<Microsoft.Scripting.Runtime.GeneratorNext`1[System.Int32]>(
    System.Int32& $state,
    System.Int32& $current) {
    .Block(System.Int32 $$gotoRouter) {
        .Switch ($$gotoRouter = $state) {
        .Case (1):
                .Goto #Label1 { }
        .Case (2):
                .Goto #Label1 { }
        .Case (3):
                .Goto #Label1 { }
        .Case (4):
                .Goto #Label1 { }
        .Case (0):
                .Goto #Label2 { }
        };
        .Block() {
            .Block() {
                .Label
                .LabelTarget #Label1:;
                .Block() {
                    .Switch ($$gotoRouter) {
                    .Case (3):
                            .Goto #Label3 { }
                    .Case (4):
                            .Goto #Label4 { }
                    .Default:
                            .Default(System.Void)
                    };
                    .Try {
                        .Block() {
                            .Switch ($$gotoRouter) {
                            .Case (1):
                                    .Goto #Label5 { }
                            .Case (2):
                                    .Goto #Label6 { }
                            .Default:
                                    .Default(System.Void)
                            };
                            .Block() {
                                $Arr = .NewArray System.Int32[
                                    3,
                                    2];
                                .Block(System.Nullable`1[System.Int32][] $var2) {
                                    $var2 = .NewArray System.Nullable`1[System.Int32][] {
                                        .Constant<System.Nullable`1[System.Int32]>(-1),
                                        .Constant<System.Nullable`1[System.Int32]>(-2),
                                        .Constant<System.Nullable`1[System.Int32]>(-3)
                                    };
                                    $var2[1] = .Constant<System.Nullable`1[System.Int32]>(1);
                                    $var2[2] = .Constant<System.Nullable`1[System.Int32]>(2)
                                };
                                .Block() {
                                    $current = 1;
                                    $state = 1;
                                    .Goto #Label2 { };
                                    .Label
                                    .LabelTarget #Label5:;
                                    $$gotoRouter = -1;
                                    .Default(System.Void)
                                };
                                $Val3 = .New ETScenarios.Miscellaneous.DebugView+BindingTestClass(){
                                    _x = -1,
                                    _y = ""Test"",
                                    _d = {
                                        a = 5,
                                        b = 1" + sep + @"1D,
                                        c = ""Testing""
                                    }
                                };
                                .Block(ETScenarios.Miscellaneous.DebugView+Circle $Variable) {
                                    $Variable = .New ETScenarios.Miscellaneous.DebugView+Circle(){
                                        Radius = 5" + sep + @"2D,
                                        MyList = {
                                            .New ETScenarios.Miscellaneous.DebugView+Point()
                                        }
                                    };
                                    $Variable.Radius
                                };
                                .Block() {
                                    $current = 2;
                                    $state = 2;
                                    .Goto #Label2 { };
                                    .Label
                                    .LabelTarget #Label6:;
                                    $$gotoRouter = -1;
                                    .Default(System.Void)
                                };
                                $TestList = .New System.Collections.Generic.List`1[System.Int32](){
                                    1,
                                    2,
                                    3
                                };
                                .Throw .Constant<%NewExpression%>(new DivideByZeroException())
                            }
                        }
                    } .Catch (System.DivideByZeroException $var3) {
                        .Block() {
                            $var1 = $var3;
                            .Default(System.Void)
                        }
                    };
                    .If ($var1 != null) {
                        .Block() {
                            .Block() {
                                $current = ($Val3._d).a;
                                $state = 3;
                                .Goto #Label2 { };
                                .Label
                                .LabelTarget #Label3:;
                                $$gotoRouter = -1;
                                .Default(System.Void)
                            };
                            .Block() {
                                $current = $Val3._x;
                                $state = 4;
                                .Goto #Label2 { };
                                .Label
                                .LabelTarget #Label4:;
                                $$gotoRouter = -1;
                                .Default(System.Void)
                            }
                        }
                    } .Else {
                        .Default(System.Void)
                    }
                }
            }
        };
        $state = 0;
        .Label
        .LabelTarget #Label2:
    }
}".Replace("%NewExpression%", typeof(NewExpression).FullName);

            #endregion str

            EU.Equal(str.Trim(), Gen.DebugView().Trim());

            return Expr.Empty();
        }

        // Extension nodes
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DebugView 5", new string[] { "positive", "debugview", "miscellaneous", "Pri2", "FullTrustOnly" })]
        public static Expr DebugView5(EU.IValidator V) {
            ParameterExpression Result = Expr.Variable(typeof(bool), "Result");
            ParameterExpression Value = Expr.Variable(typeof(object), "Value");

            LabelTarget target = Expr.Label(typeof(int), "MyLabel");
            LabelTarget target2 = Expr.Label(typeof(void), "MyOtherLabel");
            var target2Value = Expr.Variable(typeof(int), "MyOtherLabelValue");

            var Gen =
                AstUtils.GeneratorLambda(
                    typeof(Func<IEnumerator>),
                    target,
                    Expr.Block(
                        new[] { target2Value },
                        AstUtils.YieldReturn(target, Expr.Constant(1)),
                        AstUtils.FinallyFlowControl(
                            Expr.Block(
                                Expr.Block(
                                        AstUtils.YieldReturn(target, Expr.Constant(2)),
                                        EU.Goto(target2Value, target2, Expr.Constant(10)),
                                        AstUtils.YieldReturn(target, Expr.Constant(3))
                                    ),
                                Expr.Block(
                                    Expr.TryCatchFinally(
                                        AstUtils.YieldReturn(target, Expr.Constant(4)),
                                        AstUtils.YieldReturn(target, Expr.Constant(5)),
                                        Expr.Catch(typeof(IndexOutOfRangeException), AstUtils.YieldReturn(target, Expr.Constant(-1)))
                                    )
                                ),
                                Expr.Block(
                                    AstUtils.YieldReturn(target, Expr.Constant(6)),
                                    AstUtils.YieldReturn(target, EU.Label(target2Value, target2, Expr.Constant(0))),
                                    AstUtils.YieldReturn(target, Expr.Constant(7))
                                )
                            )
                        ),
                        AstUtils.YieldReturn(target, Expr.Constant(8))
                    ),
                    new ParameterExpression[] { }
                );

            #region str
            string str =
@".Lambda #Lambda1<" + typeof(Func<>).FullName + @"[System.Collections.IEnumerator]>() {
    .Extension<Microsoft.Scripting.Ast.GeneratorExpression> {
        .Call Microsoft.Scripting.Runtime.ScriptingRuntimeHelpers.MakeGenerator(.Block(
                System.IndexOutOfRangeException $var1,
                System.Int32 $MyOtherLabelValue,
                System.Exception $$saved$0) {
                .Lambda #Lambda2<Microsoft.Scripting.Runtime.GeneratorNext`1[System.Int32]>
            })
    }
}

.Lambda #Lambda2<Microsoft.Scripting.Runtime.GeneratorNext`1[System.Int32]>(
    System.Int32& $state,
    System.Int32& $current) {
    .Block(System.Int32 $$gotoRouter) {
        .Switch ($$gotoRouter = $state) {
        .Case (1):
                .Goto #Label1 { }
        .Case (2):
                .Goto #Label2 { }
        .Case (3):
                .Goto #Label3 { }
        .Case (4):
                .Goto #Label4 { }
        .Case (5):
                .Goto #Label4 { }
        .Case (6):
                .Goto #Label4 { }
        .Case (7):
                .Goto #Label5 { }
        .Case (8):
                .Goto #Label6 { }
        .Case (9):
                .Goto #Label7 { }
        .Case (10):
                .Goto #Label8 { }
        .Case (0):
                .Goto #Label9 { }
        };
        .Block() {
            .Block() {
                $current = 1;
                $state = 1;
                .Goto #Label9 { };
                .Label
                .LabelTarget #Label1:;
                $$gotoRouter = -1;
                .Default(System.Void)
            };
            .Block() {
                .Block() {
                    .Block() {
                        $current = 2;
                        $state = 2;
                        .Goto #Label9 { };
                        .Label
                        .LabelTarget #Label2:;
                        $$gotoRouter = -1;
                        .Default(System.Void)
                    };
                    .Goto MyOtherLabel { $MyOtherLabelValue = 10 };
                    .Block() {
                        $current = 3;
                        $state = 3;
                        .Goto #Label9 { };
                        .Label
                        .LabelTarget #Label3:;
                        $$gotoRouter = -1;
                        .Default(System.Void)
                    }
                };
                .Block() {
                    .Block() {
                        .Label
                        .LabelTarget #Label4:;
                        .Block() {
                            .Try {
                                .Block() {
                                    .Switch ($$gotoRouter) {
                                    .Case (6):
                                            .Goto #Label10 { }
                                    .Default:
                                            .Default(System.Void)
                                    };
                                    .Block() {
                                        .Switch ($$gotoRouter) {
                                        .Case (5):
                                                .Goto #Label11 { }
                                        .Default:
                                                .Default(System.Void)
                                        };
                                        .Try {
                                            .Block() {
                                                .Switch ($$gotoRouter) {
                                                .Case (4):
                                                        .Goto #Label12 { }
                                                .Default:
                                                        .Default(System.Void)
                                                };
                                                .Block() {
                                                    $current = 4;
                                                    $state = 4;
                                                    $$gotoRouter = 0;
                                                    .Goto #Label9 { };
                                                    .Label
                                                    .LabelTarget #Label12:;
                                                    $$gotoRouter = -1;
                                                    .Default(System.Void)
                                                }
                                            }
                                        } .Catch (System.IndexOutOfRangeException $var2) {
                                            .Block() {
                                                $var1 = $var2;
                                                .Default(System.Void)
                                            }
                                        };
                                        .If ($var1 != null) {
                                            .Block() {
                                                $current = -1;
                                                $state = 5;
                                                $$gotoRouter = 0;
                                                .Goto #Label9 { };
                                                .Label
                                                .LabelTarget #Label11:;
                                                $$gotoRouter = -1;
                                                .Default(System.Void)
                                            }
                                        } .Else {
                                            .Default(System.Void)
                                        }
                                    };
                                    $$saved$0 = null;
                                    .Label
                                    .LabelTarget #Label10:
                                }
                            } .Catch (System.Exception $e) {
                                .Block() {
                                    $$saved$0 = $e;
                                    .Default(System.Void)
                                }
                            } .Finally {
                                .Block() {
                                    .If (
                                        $$gotoRouter == 0 && $state != 0
                                    ) {
                                        .Goto #Label13 { }
                                    } .Else {
                                        .Default(System.Void)
                                    };
                                    .Switch ($$gotoRouter) {
                                    .Case (6):
                                            .Goto #Label14 { }
                                    .Default:
                                            .Default(System.Void)
                                    };
                                    .Block() {
                                        $current = 5;
                                        $state = 6;
                                        $$gotoRouter = 0;
                                        .Goto #Label13 { };
                                        .Label
                                        .LabelTarget #Label14:;
                                        $$gotoRouter = -1;
                                        .Default(System.Void)
                                    };
                                    .If ($$saved$0 != null) {
                                        .Throw $$saved$0
                                    } .Else {
                                        .Default(System.Void)
                                    };
                                    .Label
                                    .LabelTarget #Label13:
                                }
                            };
                            .If ($$gotoRouter == 0) {
                                .Goto #Label9 { }
                            } .Else {
                                .Default(System.Void)
                            }
                        }
                    }
                };
                .Block() {
                    .Block() {
                        $current = 6;
                        $state = 7;
                        .Goto #Label9 { };
                        .Label
                        .LabelTarget #Label5:;
                        $$gotoRouter = -1;
                        .Default(System.Void)
                    };
                    .Block() {
                        .Block() {
                            .Label
                                $MyOtherLabelValue = 0
                            .LabelTarget MyOtherLabel:;
                            $current = $MyOtherLabelValue
                        };
                        $state = 8;
                        .Goto #Label9 { };
                        .Label
                        .LabelTarget #Label6:;
                        $$gotoRouter = -1;
                        .Default(System.Void)
                    };
                    .Block() {
                        $current = 7;
                        $state = 9;
                        .Goto #Label9 { };
                        .Label
                        .LabelTarget #Label7:;
                        $$gotoRouter = -1;
                        .Default(System.Void)
                    }
                }
            };
            .Block() {
                $current = 8;
                $state = 10;
                .Goto #Label9 { };
                .Label
                .LabelTarget #Label8:;
                $$gotoRouter = -1;
                .Default(System.Void)
            }
        };
        $state = 0;
        .Label
        .LabelTarget #Label9:
    }
}";
            #endregion str

            EU.Equal(str.Trim(), Gen.DebugView().Trim());

            return Expr.Empty();
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

        // Dynamic
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DebugView 6", new string[] { "positive", "debugview", "miscellaneous", "Pri2", "FullTrustOnly" })]
        public static Expr DebugView6(EU.IValidator V) {
            MethodInfo mi = ((Func<string, int>)((string s) => { return Convert.ToInt32(s); })).Method;

            MyBinder binder = new MyBinder();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            Expr tree =
                Expr.Block(
                    new[] { Result },
                    Expr.TryCatch(
                        Expr.Block(
                            Expr.Assign(
                                Result,
                                Expr.Dynamic(binder, typeof(string), new List<Expression>() { Expr.Constant(1)})
                            ),
                            Expr.Assign(
                                Result,
                                Expr.Dynamic(new MyBinder(), typeof(string), new List<Expression>() { Expr.Constant(1), Expr.Constant("Test"), Expr.Constant(2.1), Expr.Constant(3) })
                            ),
                            Expr.Add(Expr.Convert(Result, typeof(int), mi), Expr.Convert(Expr.Dynamic(new MyBinder(), typeof(string), new List<Expression>() { Expr.Constant(2.1) }), typeof(int), mi)),
                            Expr.Constant("EndTry")
                        ),
                        Expr.Catch(typeof(string), Expr.Constant("CaughtString")),
                        Expr.Catch(typeof(Exception), Expr.Constant("CaughtException"))
                    )
                );

            #region str
            string str =
@".Block(System.String $Result) {
    .Try {
        .Block() {
            $Result = .Dynamic ETScenarios.Miscellaneous.DebugView+MyBinder(1);
            $Result = .Dynamic ETScenarios.Miscellaneous.DebugView+MyBinder(
                1,
                ""Test"",
                2" + sep + @"1D,
                3);
            (System.Int32)$Result + (System.Int32).Dynamic ETScenarios.Miscellaneous.DebugView+MyBinder(2" + sep + @"1D);
            ""EndTry""
        }
    } .Catch (System.String) {
        ""CaughtString""
    } .Catch (System.Exception) {
        ""CaughtException""
    }
}";
            #endregion

            EU.Equal(str.Trim(), tree.DebugView().Trim());

            return Expr.Empty();
        }

        #region ToString miscellaneous nodes
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ToString 1", new string[] { "positive", "tostring", "miscellaneous", "Pri2", })]
        public static Expr ToString1(EU.IValidator V) {
            List<object> Expressions = new List<object>();

            var label1 = Expr.Label("label1");
            var label2 = Expr.Label(typeof(int), "label2");

            var MyGoto1 = Expr.Goto(label1);
            var MyGoto2 = Expr.Goto(label2, Expr.Constant(1));

            Expressions.Add(MyGoto1);
            Expressions.Add(MyGoto2);

            var MySwitchCase1 = Expr.SwitchCase(Expr.Constant(1), new Expression[] { Expr.Constant(10) });
            var MySwitchCase2 = Expr.SwitchCase(Expr.TryCatch(Expr.Constant(1), Expr.Catch(typeof(DivideByZeroException), Expr.Constant(-1))), new Expression[] { Expr.Constant(DateTime.Now), Expr.Constant(DateTime.MaxValue) });

            Expressions.Add(MySwitchCase1);
            Expressions.Add(MySwitchCase2);

            ParameterExpression Ex = Expr.Variable(typeof(Exception), "Ex");

            var MyCatchBlock1 = Expr.Catch(typeof(Exception), Expr.Constant(1));
            var MyCatchBlock2 = Expr.Catch(Ex, Expr.TryCatch(Expr.Constant(1), MyCatchBlock1), Expr.GreaterThan(Expr.Constant(1), Expr.Constant(0)));

            Expressions.Add(MyCatchBlock1);
            Expressions.Add(MyCatchBlock2);

            var sd = Expr.SymbolDocument("");
            Expressions.Add(Expr.DebugInfo(sd, 1, 1, 1, 1));

            var MyElementInit1 = Expr.ElementInit(typeof(List<int>).GetMethod("Add"), new Expression[] { Expr.Constant(1) });
            var MyListInit2 = Expr.ListInit(Expr.New(typeof(List<int>).GetConstructor(new Type[] { })), new ElementInit[] { MyElementInit1, MyElementInit1, MyElementInit1 });

            Expressions.Add(MyElementInit1);
            Expressions.Add(MyListInit2);

            MemberInfo memi = typeof(BindingTestClass).GetMember("_x")[0];
            var b1 = Expr.Bind(memi, Expr.Constant(3));
            MemberInfo memi2 = typeof(BindingTestClass).GetMember("_y")[0];
            var b2 = Expr.Bind(memi2, Expr.Constant("Test"));

            Expressions.Add(
                Expr.MemberInit(
                    Expr.New(typeof(BindingTestClass)),
                    new MemberBinding[] { 
                        Expr.Bind(memi, Expr.Constant(-1)), // _x
                        b2, // _y
                        Expr.MemberBind( // _d
                            typeof(BindingTestClass).GetField("_d"),
                            new MemberBinding[] { // binding fields of _d member of type Data
                                Expr.Bind(typeof(Data).GetField("a"), Expr.Constant(5)),
                                Expr.Bind(typeof(Data).GetField("b"), Expr.Constant(1.1)),
                                Expr.Bind(typeof(Data).GetField("c"), Expr.Constant("Testing"))
                            }
                        )
                    }
                )
            );

            Expressions.Add(
                Expr.MemberBind( // _d
                            typeof(BindingTestClass).GetField("_d"),
                            new MemberBinding[] { // binding fields of _d member of type Data
                                Expr.Bind(typeof(Data).GetField("a"), Expr.Constant(5)),
                                Expr.Bind(typeof(Data).GetField("b"), Expr.Constant(1.1)),
                                Expr.Bind(typeof(Data).GetField("c"), Expr.Constant("Testing"))
                            }
                        )
            );

            var MyNewArray1 = Expr.NewArrayInit(typeof(int), new Expression[] { Expr.Constant(1), Expr.Constant(2), Expr.Constant(3) });
            var MyNewArray2 = Expr.NewArrayBounds(typeof(int[]), new Expression[] { Expr.Constant(2), Expr.Constant(3) });

            Expressions.Add(MyNewArray1);
            Expressions.Add(MyNewArray2);

            ParameterExpression TestResult = Expr.Variable(typeof(string), "TestResult");
            ParameterExpression Value = Expr.Variable(typeof(int), "Value");
            Expressions.Add(Expr.Assign(TestResult, Expr.Constant("hello")));
            Expressions.Add(Expr.Assign(Value, Expr.Constant(-1)));
            var MyRTV = Expr.RuntimeVariables(TestResult, Value);
            Expressions.Add(MyRTV);

            var MyArrayIndex = Expr.ArrayIndex(MyNewArray1, Expr.Constant(0));

            Expressions.Add(MyArrayIndex);

            var MyOrElse = Expr.OrElse(Expr.Constant(true), Expr.Constant(false));
            var MyGreaterThan = Expr.GreaterThan(Expr.Constant(1), Expr.Constant(0));
            var MyGreaterThanOrEqual = Expr.GreaterThanOrEqual(Expr.Constant(1), Expr.Constant(0));
            var MyLessThan = Expr.LessThan(Expr.Constant(1), Expr.Constant(0));
            var MyLessThanOrEqual = Expr.LessThanOrEqual(Expr.Constant(1), Expr.Constant(0));
            var MyAddAssign = Expr.AddAssign(Value, Expr.Constant(1));
            var MyAddAssignChecked = Expr.AddAssignChecked(Value, Expr.Constant(1));
            var MySubtractAssign = Expr.SubtractAssign(Value, Expr.Constant(1));
            var MySubtractAssignChecked = Expr.SubtractAssignChecked(Value, Expr.Constant(1));
            var MyMultiplyAssign = Expr.MultiplyAssign(Value, Expr.Constant(1));
            var MyMultiplyAssignChecked = Expr.MultiplyAssignChecked(Value, Expr.Constant(1));
            ParameterExpression PowerValue = Expr.Variable(typeof(double), "PowerValue");
            var MyPower = Expr.Power(PowerValue, Expr.Constant(2.1, typeof(double)));
            var MyPowerAssign = Expr.PowerAssign(PowerValue, Expr.Constant(2.1, typeof(double)));
            var MyModuloAssign = Expr.ModuloAssign(Value, Expr.Constant(1));
            var MyModulo = Expr.Modulo(Value, Expr.Constant(1));
            var MyRightShiftAssign = Expr.RightShiftAssign(Value, Expr.Constant(1));
            var MyLeftShiftAssign = Expr.LeftShiftAssign(Value, Expr.Constant(1));
            var MyAnd = Expr.And(Expr.Constant(true), Expr.Constant(false));
            var MyAnd2 = Expr.And(Expr.Constant(1), Expr.Constant(2));
            ParameterExpression ValueBool = Expr.Variable(typeof(bool), "ValueBool");
            ParameterExpression ValueInt = Expr.Variable(typeof(int), "ValueInt");
            var MyAndAssign = Expr.AndAssign(ValueBool, Expr.Constant(false));
            var MyAndAssign2 = Expr.AndAssign(ValueInt, Expr.Constant(2));
            var MyOr = Expr.Or(Expr.Constant(true), Expr.Constant(false));
            var MyOr2 = Expr.Or(Expr.Constant(1), Expr.Constant(2));
            var MyOrAssign = Expr.OrAssign(ValueBool, Expr.Constant(false));
            var MyOrAssign2 = Expr.OrAssign(ValueInt, Expr.Constant(2));

            Expressions.Add(MyOrElse);
            Expressions.Add(MyGreaterThan);
            Expressions.Add(MyGreaterThanOrEqual);
            Expressions.Add(MyLessThan);
            Expressions.Add(MyLessThanOrEqual);
            Expressions.Add(MyAddAssign);
            Expressions.Add(MyAddAssignChecked);
            Expressions.Add(MySubtractAssign);
            Expressions.Add(MySubtractAssignChecked);
            Expressions.Add(MyMultiplyAssign);
            Expressions.Add(MyMultiplyAssignChecked);
            Expressions.Add(MyPower);
            Expressions.Add(MyPowerAssign);
            Expressions.Add(MyModuloAssign);
            Expressions.Add(MyModulo);
            Expressions.Add(MyRightShiftAssign);
            Expressions.Add(MyLeftShiftAssign);
            Expressions.Add(MyAnd);
            Expressions.Add(MyAnd2);
            Expressions.Add(MyAndAssign);
            Expressions.Add(MyAndAssign2);
            Expressions.Add(MyOr);
            Expressions.Add(MyOr2);
            Expressions.Add(MyOrAssign);
            Expressions.Add(MyOrAssign2);

            CallSiteBinder myBinder = new MyBinder();
            Expressions.Add(Expr.Dynamic(myBinder, typeof(string), Expr.Constant(1)));
            Expressions.Add(Expr.Dynamic(myBinder, typeof(string), Expr.Constant(1), Expr.Constant("Test")));
            Expressions.Add(Expr.Dynamic(myBinder, typeof(string), Expr.Constant(1), Expr.Constant("Test"), Expr.Constant(2.1)));
            Expressions.Add(Expr.Dynamic(myBinder, typeof(string), Expr.Constant(1), Expr.Constant("Test"), Expr.Constant(2.1), Expr.Constant(3)));

            foreach (var e in Expressions) {
                e.ToString();
            }

            return Expr.Empty();
        }
        #endregion
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
}
