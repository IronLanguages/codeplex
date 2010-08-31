#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Utils;
#endif

using System;
using System.Collections.Generic;
using Microsoft.Scripting.Ast;

namespace ETScenarios.ControlFlow {
    using EU = ETUtils.ExpressionUtils;
    using Expr = Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public class IfStatement {
        //Call AstUtils.If(). Use resulting ifstatementbuilder to construct a simple if statement with a test statement that evaluates to true, false.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "If simple case 1", new string[] { "positive", "if", "controlflow", "Pri2" })]
        public static Expr If1(EU.IValidator V) {
            List<Expr> Expressions = new List<Expr>();
            //if statement, sets a variable, checks for value at the end.
            ParameterExpression VarTrue = Expr.Variable(typeof(Boolean), "TrueRan");
            ParameterExpression VarFalse = Expr.Variable(typeof(Boolean), "FalseRan");

            IfStatementBuilder _if = AstUtils.If();
            _if = _if.ElseIf(Expr.Constant(true), Expr.Assign(VarTrue, Expr.Constant(true)));

            Expressions.Add(_if.Else(Expr.Assign(VarFalse, Expr.Constant(true))));

            //Assert issues
            Expressions.Add(ETUtils.ExpressionUtils.GenAreEqual(Expr.Constant(true), VarTrue));
            Expressions.Add(ETUtils.ExpressionUtils.GenAreEqual(Expr.Constant(false), VarFalse));
            var tree = EU.BlockVoid(new[] { VarTrue, VarFalse }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //Call AstUtils.If(). Use resulting ifstatementbuilder to construct a simple if statement with a test statement that evaluates to true, false.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "If simple case 2", new string[] { "positive", "if", "controlflow", "Pri2" })]
        public static Expr If2(EU.IValidator V) {
            List<Expr> Expressions = new List<Expr>();
            //if statement, sets a variable, checks for value at the end.
            ParameterExpression VarTrue = Expr.Variable(typeof(Boolean), "TrueRan");
            ParameterExpression VarFalse = Expr.Variable(typeof(Boolean), "FalseRan");

            IfStatementBuilder _if = AstUtils.If();
            _if = _if.ElseIf(Expr.Constant(false), Expr.Assign(VarTrue, Expr.Constant(true)));

            Expressions.Add(_if.Else(Expr.Assign(VarFalse, Expr.Constant(true))));

            //Assert issues
            Expressions.Add(ETUtils.ExpressionUtils.GenAreEqual(Expr.Constant(false), VarTrue, "Ran the if even thought it was false?"));
            Expressions.Add(ETUtils.ExpressionUtils.GenAreEqual(Expr.Constant(true), VarFalse, "Didn't run the else?"));
            var tree = EU.BlockVoid(new[] { VarTrue, VarFalse }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //Call AstUtils.If(). Use resulting ifstatementbuilder to construct a simple if statement with a test statement that evaluates to true, false.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "If simple case 3", new string[] { "positive", "if", "controlflow", "Pri3" })]
        public static Expr If3(EU.IValidator V) {
            List<Expr> Expressions = new List<Expr>();
            //if statement, sets a variable, checks for value at the end.
            ParameterExpression VarTrue = Expr.Variable(typeof(Boolean), "TrueRan");

            IfStatementBuilder _if = AstUtils.If();
            _if = _if.ElseIf(Expr.Constant(true), Expr.Assign(VarTrue, Expr.Constant(true)));

            Expressions.Add(_if.ToStatement());

            //Assert issues
            Expressions.Add(ETUtils.ExpressionUtils.GenAreEqual(Expr.Constant(true), VarTrue));
            var tree = EU.BlockVoid(new[] { VarTrue }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //Call AstUtils.If(). Use resulting ifstatementbuilder to construct a simple if statement with a test statement that evaluates to true, false.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "If simple case 4", new string[] { "positive", "if", "controlflow", "Pri3" })]
        public static Expr If4(EU.IValidator V) {
            List<Expr> Expressions = new List<Expr>();
            //if statement, sets a variable, checks for value at the end.
            ParameterExpression VarTrue = Expr.Variable(typeof(Boolean), "TrueRan");

            IfStatementBuilder _if = AstUtils.If();
            _if = _if.ElseIf(Expr.Constant(false), Expr.Assign(VarTrue, Expr.Constant(true)));

            Expressions.Add(_if.ToStatement());

            //Assert issues
            Expressions.Add(ETUtils.ExpressionUtils.GenAreEqual(Expr.Constant(false), VarTrue));
            var tree = EU.BlockVoid(new[] { VarTrue }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //Use an Ifstatemenet builder from if() to construct an if with an elseif clause, exercise both paths
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "If() 5", new string[] { "positive", "if", "controlflow", "Pri2" })]
        public static Expr If5(EU.IValidator V) {
            List<Expr> Expressions = new List<Expr>();
            //if statement, sets a variable, checks for value at the end.
            ParameterExpression VarIf = Expr.Variable(typeof(Boolean), "TrueRan");
            ParameterExpression VarElseIf = Expr.Variable(typeof(Boolean), "FalseRan");

            IfStatementBuilder _if = AstUtils.If();
            _if = _if.ElseIf(Expr.Constant(false), Expr.Assign(VarIf, Expr.Constant(true)));
            _if = _if.ElseIf(Expr.Constant(false), Expr.Assign(VarElseIf, Expr.Constant(true)));
            Expressions.Add(_if.ToStatement());

            //Assert issues
            Expressions.Add(ETUtils.ExpressionUtils.GenAreEqual(Expr.Constant(false), VarIf, "Shouldn't have run the if?"));
            Expressions.Add(ETUtils.ExpressionUtils.GenAreEqual(Expr.Constant(false), VarElseIf, "shouldn't have run the elseif?"));
            var tree = EU.BlockVoid(new[] { VarIf, VarElseIf }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //Use an Ifstatemenet builder from if() to construct an if with an elseif clause, exercise both paths
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "If() 6", new string[] { "positive", "if", "controlflow", "Pri2" })]
        public static Expr If6(EU.IValidator V) {
            List<Expr> Expressions = new List<Expr>();
            //if statement, sets a variable, checks for value at the end.
            ParameterExpression VarIf = Expr.Variable(typeof(Boolean), "TrueRan");
            ParameterExpression VarElseIf = Expr.Variable(typeof(Boolean), "FalseRan");

            IfStatementBuilder _if = AstUtils.If();
            _if = _if.ElseIf(Expr.Constant(true), Expr.Assign(VarIf, Expr.Constant(true)));
            _if = _if.ElseIf(Expr.Constant(false), Expr.Assign(VarElseIf, Expr.Constant(true)));
            Expressions.Add(_if.ToStatement());

            //Assert issues
            Expressions.Add(ETUtils.ExpressionUtils.GenAreEqual(Expr.Constant(true), VarIf, "Didn't run the if?"));
            Expressions.Add(ETUtils.ExpressionUtils.GenAreEqual(Expr.Constant(false), VarElseIf, "shouldn't have run the elseif?"));
            var tree = EU.BlockVoid(new[] { VarIf, VarElseIf }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //Use an Ifstatemenet builder from if() to construct an if with an elseif clause, exercise both paths
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "If() 7", new string[] { "positive", "if", "controlflow", "Pri2" })]
        public static Expr If7(EU.IValidator V) {
            List<Expr> Expressions = new List<Expr>();
            //if statement, sets a variable, checks for value at the end.
            ParameterExpression VarIf = Expr.Variable(typeof(Boolean), "TrueRan");
            ParameterExpression VarElseIf = Expr.Variable(typeof(Boolean), "FalseRan");

            IfStatementBuilder _if = AstUtils.If();
            _if = _if.ElseIf(Expr.Constant(true), Expr.Assign(VarIf, Expr.Constant(true)));
            _if = _if.ElseIf(Expr.Constant(true), Expr.Assign(VarElseIf, Expr.Constant(true)));
            Expressions.Add(_if.ToStatement());

            //Assert issues
            Expressions.Add(ETUtils.ExpressionUtils.GenAreEqual(Expr.Constant(true), VarIf, "Didn't run the if?"));
            Expressions.Add(ETUtils.ExpressionUtils.GenAreEqual(Expr.Constant(false), VarElseIf, "shouldn't have run the elseif"));
            var tree = EU.BlockVoid(new[] { VarIf, VarElseIf }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //Use an Ifstatemenet builder from if() to construct an if with an elseif clause, exercise both paths
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "If() 8", new string[] { "positive", "if", "controlflow", "Pri2" })]
        public static Expr If8(EU.IValidator V) {
            List<Expr> Expressions = new List<Expr>();
            //if statement, sets a variable, checks for value at the end.
            ParameterExpression VarIf = Expr.Variable(typeof(Boolean), "TrueRan");
            ParameterExpression VarElseIf = Expr.Variable(typeof(Boolean), "FalseRan");

            IfStatementBuilder _if = AstUtils.If();
            _if = _if.ElseIf(Expr.Constant(false), Expr.Assign(VarIf, Expr.Constant(true)));
            _if = _if.ElseIf(Expr.Constant(true), Expr.Assign(VarElseIf, Expr.Constant(true)));
            Expressions.Add(_if.ToStatement());

            //Assert issues
            Expressions.Add(ETUtils.ExpressionUtils.GenAreEqual(Expr.Constant(false), VarIf, "Shouldn't have run the if"));
            Expressions.Add(ETUtils.ExpressionUtils.GenAreEqual(Expr.Constant(true), VarElseIf, "Didn't run the elseif?"));
            var tree = EU.BlockVoid(new[] { VarIf, VarElseIf }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //Use an ifstatementbuilder from if() to construct an if with an elseif clause and an else clause, exercise all three paths.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "If() 9", new string[] { "positive", "if", "controlflow", "Pri1" })]
        public static Expr If9(EU.IValidator V) {
            List<Expr> Expressions = new List<Expr>();
            //if statement, sets a variable, checks for value at the end.
            ParameterExpression VarIf = Expr.Variable(typeof(Boolean), "IfRan");
            ParameterExpression VarElseIf = Expr.Variable(typeof(Boolean), "ElseIfRan");
            ParameterExpression VarElse = Expr.Variable(typeof(Boolean), "ElseRan");

            IfStatementBuilder _if = AstUtils.If();
            _if = _if.ElseIf(Expr.Constant(true), Expr.Assign(VarIf, Expr.Constant(true)));
            _if = _if.ElseIf(Expr.Constant(true), Expr.Assign(VarElseIf, Expr.Constant(true)));

            Expressions.Add(_if.Else(Expr.Assign(VarElse, Expr.Constant(true))));

            //Assert issues
            Expressions.Add(ETUtils.ExpressionUtils.GenAreEqual(Expr.Constant(true), VarIf, "if ran"));
            Expressions.Add(ETUtils.ExpressionUtils.GenAreEqual(Expr.Constant(false), VarElseIf, "elseif ran"));
            Expressions.Add(ETUtils.ExpressionUtils.GenAreEqual(Expr.Constant(false), VarElse, "else ran"));

            var tree = EU.BlockVoid(new[] { VarIf, VarElseIf, VarElse }, Expressions);
            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "If() 10", new string[] { "positive", "if", "controlflow", "Pri1" })]
        public static Expr If10(EU.IValidator V) {
            List<Expr> Expressions = new List<Expr>();
            //if statement, sets a variable, checks for value at the end.
            ParameterExpression VarIf = Expr.Variable(typeof(Boolean), "IfRan");
            ParameterExpression VarElseIf = Expr.Variable(typeof(Boolean), "ElseIfRan");
            ParameterExpression VarElse = Expr.Variable(typeof(Boolean), "ElseRan");

            IfStatementBuilder _if = AstUtils.If();
            _if = _if.ElseIf(Expr.Constant(true), Expr.Assign(VarIf, Expr.Constant(true)));
            _if = _if.ElseIf(Expr.Constant(false), Expr.Assign(VarElseIf, Expr.Constant(true)));

            Expressions.Add(_if.Else(Expr.Assign(VarElse, Expr.Constant(true))));

            //Assert issues
            Expressions.Add(ETUtils.ExpressionUtils.GenAreEqual(Expr.Constant(true), VarIf, "if ran"));
            Expressions.Add(ETUtils.ExpressionUtils.GenAreEqual(Expr.Constant(false), VarElseIf, "elseif ran"));
            Expressions.Add(ETUtils.ExpressionUtils.GenAreEqual(Expr.Constant(false), VarElse, "else ran"));

            var tree = EU.BlockVoid(new[] { VarIf, VarElseIf, VarElse }, Expressions);
            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "If() 11", new string[] { "positive", "if", "controlflow", "Pri1" })]
        public static Expr If11(EU.IValidator V) {
            List<Expr> Expressions = new List<Expr>();
            //if statement, sets a variable, checks for value at the end.
            ParameterExpression VarIf = Expr.Variable(typeof(Boolean), "IfRan");
            ParameterExpression VarElseIf = Expr.Variable(typeof(Boolean), "ElseIfRan");
            ParameterExpression VarElse = Expr.Variable(typeof(Boolean), "ElseRan");

            IfStatementBuilder _if = AstUtils.If();
            _if = _if.ElseIf(Expr.Constant(false), Expr.Assign(VarIf, Expr.Constant(true)));
            _if = _if.ElseIf(Expr.Constant(true), Expr.Assign(VarElseIf, Expr.Constant(true)));

            Expressions.Add(_if.Else(Expr.Assign(VarElse, Expr.Constant(true))));

            //Assert issues
            Expressions.Add(ETUtils.ExpressionUtils.GenAreEqual(Expr.Constant(false), VarIf, "if ran"));
            Expressions.Add(ETUtils.ExpressionUtils.GenAreEqual(Expr.Constant(true), VarElseIf, "elseif ran"));
            Expressions.Add(ETUtils.ExpressionUtils.GenAreEqual(Expr.Constant(false), VarElse, "else ran"));

            var tree = EU.BlockVoid(new[] { VarIf, VarElseIf, VarElse }, Expressions);
            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "If() 12", new string[] { "positive", "if", "controlflow", "Pri1" })]
        public static Expr If12(EU.IValidator V) {
            List<Expr> Expressions = new List<Expr>();
            //if statement, sets a variable, checks for value at the end.
            ParameterExpression VarIf = Expr.Variable(typeof(Boolean), "IfRan");
            ParameterExpression VarElseIf = Expr.Variable(typeof(Boolean), "ElseIfRan");
            ParameterExpression VarElse = Expr.Variable(typeof(Boolean), "ElseRan");

            IfStatementBuilder _if = AstUtils.If();
            _if = _if.ElseIf(Expr.Constant(false), Expr.Assign(VarIf, Expr.Constant(true)));
            _if = _if.ElseIf(Expr.Constant(false), Expr.Assign(VarElseIf, Expr.Constant(true)));

            Expressions.Add(_if.Else(Expr.Assign(VarElse, Expr.Constant(true))));

            //Assert issues
            Expressions.Add(ETUtils.ExpressionUtils.GenAreEqual(Expr.Constant(false), VarIf, "if ran"));
            Expressions.Add(ETUtils.ExpressionUtils.GenAreEqual(Expr.Constant(false), VarElseIf, "elseif ran"));
            Expressions.Add(ETUtils.ExpressionUtils.GenAreEqual(Expr.Constant(true), VarElse, "else ran"));

            var tree = EU.BlockVoid(new[] { VarIf, VarElseIf, VarElse }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //Pass an array with null element to the body argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "If() 13", new string[] { "negative", "if", "controlflow", "Pri1" }, Exception = typeof(ArgumentNullException))]
        public static Expr If13(EU.IValidator V) {
            List<Expr> Expressions = new List<Expr>();

            ConstantExpression test = Expr.Constant(true);

            Expressions.Add(EU.Throws<System.ArgumentNullException>(() =>
            {
                AstUtils.If(test, new Expr[] { null }).ToStatement();
            }));

            return EU.BlockVoid(Expressions);
        }

        //Verify Order of execution - two elseifs, and an else
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "If() 14", new string[] { "positive", "if", "controlflow", "Pri1" })]
        public static Expr If14(EU.IValidator V) {
            List<Expr> Expressions = new List<Expr>();

            ParameterExpression Result = Expr.Variable(typeof(String), "Result");

            InvocationExpression IfTest = Expr.Invoke(Expr.Lambda<Func<Boolean>>(Expr.Block(EU.ConcatEquals(Result, "_If_"), Expr.Constant(false))));
            InvocationExpression ElseIf1Test = Expr.Invoke(Expr.Lambda<Func<Boolean>>(Expr.Block(EU.ConcatEquals(Result, "_ElseIf1_"), Expr.Constant(false))));
            InvocationExpression ElseIf2Test = Expr.Invoke(Expr.Lambda<Func<Boolean>>(Expr.Block(EU.ConcatEquals(Result, "_ElseIf2_"), Expr.Constant(false))));

            IfStatementBuilder MyIf = AstUtils.If(IfTest, Expr.Assign(Result, Expr.Constant("")));
            MyIf = MyIf.ElseIf(ElseIf1Test, Expr.Assign(Result, Expr.Constant("")));
            MyIf = MyIf.ElseIf(ElseIf2Test, Expr.Assign(Result, Expr.Constant("")));



            Expressions.Add(MyIf.Else(EU.ConcatEquals(Result, "_Else_")));

            Expressions.Add(ETUtils.ExpressionUtils.GenAreEqual(Result, Expr.Constant("_If__ElseIf1__ElseIf2__Else_")));

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //Define a scope on the main block, on an elseif block, and on an else block
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "If() 15", new string[] { "positive", "if", "scope", "controlflow", "Pri1" })]
        public static Expr If15(EU.IValidator V) {
            List<Expr> Expressions = new List<Expr>();

            ParameterExpression VarIf = Expr.Variable(typeof(Boolean), "VarIf");
            ParameterExpression VarElseIf = Expr.Variable(typeof(Boolean), "VarElseIf");
            ParameterExpression VarElse = Expr.Variable(typeof(Boolean), "VarElse");

            ParameterExpression VarLocalIf = Expr.Variable(typeof(Boolean), "VarLocalIf");
            ParameterExpression VarLocalElseIf = Expr.Variable(typeof(Boolean), "VarLocalElseIf");
            ParameterExpression VarLocalElse = Expr.Variable(typeof(Boolean), "VarLocalElse");

            ConstantExpression IfTest = Expr.Constant(true);
            ConstantExpression ElseIf1Test = Expr.Constant(false);

            Expressions.Add(Expr.Assign(VarIf, Expr.Constant(false)));
            Expressions.Add(Expr.Assign(VarElseIf, Expr.Constant(false)));
            Expressions.Add(Expr.Assign(VarElse, Expr.Constant(false)));

            IfStatementBuilder MyIf = AstUtils.If(
                                        IfTest,
                                        EU.BlockVoid(
                                            new[] { VarLocalIf },
                                            Expr.Assign(
                                                VarLocalIf,
                                                Expr.Constant(true)
                                            ),
                                            Expr.Assign(VarIf, VarLocalIf)
                                        )
                                    );

            MyIf = MyIf.ElseIf(
                                    ElseIf1Test,
                                    EU.BlockVoid(
                                        new[] { VarLocalElseIf },
                                        Expr.Assign(VarLocalElseIf, Expr.Constant(false)),
                                        Expr.Assign(
                                            VarLocalElseIf,
                                            Expr.Constant(true)
                                        ),
                                        Expr.Assign(VarElseIf, VarLocalElseIf)
                                    )
                              );

            Expressions.Add(
                                MyIf.Else(
                                    EU.BlockVoid(
                                        new[] { VarLocalElse },
                                        Expr.Assign(VarLocalElse, Expr.Constant(false)),
                                        Expr.Assign(
                                            VarLocalElse,
                                            Expr.Constant(true)
                                        ),
                                        Expr.Assign(VarElse, VarLocalElse)
                                    )
                                )
                            );

            Expressions.Add(ETUtils.ExpressionUtils.GenAreEqual(VarIf, Expr.Constant(true), "If 1"));
            Expressions.Add(ETUtils.ExpressionUtils.GenAreEqual(VarElseIf, Expr.Constant(false), "ElseIf 1"));
            Expressions.Add(ETUtils.ExpressionUtils.GenAreEqual(VarElse, Expr.Constant(false), "Else 1"));

            //second case.
            IfTest = Expr.Constant(true);
            ElseIf1Test = Expr.Constant(true);

            Expressions.Add(Expr.Assign(VarIf, Expr.Constant(false)));
            Expressions.Add(Expr.Assign(VarElseIf, Expr.Constant(false)));
            Expressions.Add(Expr.Assign(VarElse, Expr.Constant(false)));

            MyIf = AstUtils.If(
                                        IfTest,
                                        EU.BlockVoid(
                                            new[] { VarLocalIf },
                                            Expr.Assign(
                                                VarLocalIf,
                                                Expr.Constant(true)
                                            ),
                                            Expr.Assign(VarIf, VarLocalIf)
                                        )
                                 );

            MyIf = MyIf.ElseIf(
                                    ElseIf1Test,
                                    EU.BlockVoid(
                                        new[] { VarLocalElseIf },
                                        Expr.Assign(
                                            VarLocalElseIf,
                                            Expr.Constant(true)
                                        ),
                                        Expr.Assign(VarElseIf, VarLocalElseIf)
                                    )
                              );

            Expressions.Add(
                                MyIf.Else(
                                    EU.BlockVoid(
                                        new[] { VarLocalElse },
                                        Expr.Assign(
                                            VarLocalElse,
                                            Expr.Constant(true)
                                        ),
                                        Expr.Assign(VarElse, VarLocalElse)
                                    )
                                )
                            );

            Expressions.Add(ETUtils.ExpressionUtils.GenAreEqual(VarIf, Expr.Constant(true), "If 2"));
            Expressions.Add(ETUtils.ExpressionUtils.GenAreEqual(VarElseIf, Expr.Constant(false), "ElseIf 2"));
            Expressions.Add(ETUtils.ExpressionUtils.GenAreEqual(VarElse, Expr.Constant(false), "Else 2"));

            //third case
            IfTest = Expr.Constant(false);
            ElseIf1Test = Expr.Constant(true);

            Expressions.Add(Expr.Assign(VarIf, Expr.Constant(false)));
            Expressions.Add(Expr.Assign(VarElseIf, Expr.Constant(false)));
            Expressions.Add(Expr.Assign(VarElse, Expr.Constant(false)));

            MyIf = AstUtils.If(
                                        IfTest,
                                        EU.BlockVoid(
                                            new[] { VarLocalIf },
                                            Expr.Assign(
                                                VarLocalIf,
                                                Expr.Constant(true)
                                            ),
                                            Expr.Assign(VarIf, VarLocalIf)
                                        )
                                 );

            MyIf = MyIf.ElseIf(
                                    ElseIf1Test,
                                    EU.BlockVoid(
                                        new[] { VarLocalElseIf },
                                        Expr.Assign(
                                            VarLocalElseIf,
                                            Expr.Constant(true)
                                        ),
                                        Expr.Assign(VarElseIf, VarLocalElseIf)
                                    )
                              );

            Expressions.Add(
                                MyIf.Else(
                                    EU.BlockVoid(
                                        new[] { VarLocalElse },
                                        Expr.Assign(
                                            VarLocalElse,
                                            Expr.Constant(true)
                                        ),
                                        Expr.Assign(VarElse, VarLocalElse)
                                    )
                                )
                            );

            Expressions.Add(ETUtils.ExpressionUtils.GenAreEqual(VarIf, Expr.Constant(false), "If 3"));
            Expressions.Add(ETUtils.ExpressionUtils.GenAreEqual(VarElseIf, Expr.Constant(true), "ElseIf 3"));
            Expressions.Add(ETUtils.ExpressionUtils.GenAreEqual(VarElse, Expr.Constant(false), "Else 3"));

            //fourth case
            IfTest = Expr.Constant(false);
            ElseIf1Test = Expr.Constant(false);

            Expressions.Add(Expr.Assign(VarIf, Expr.Constant(false)));
            Expressions.Add(Expr.Assign(VarElseIf, Expr.Constant(false)));
            Expressions.Add(Expr.Assign(VarElse, Expr.Constant(false)));

            MyIf = AstUtils.If(
                                        IfTest,
                                        EU.BlockVoid(
                                            new[] { VarLocalIf },
                                            Expr.Assign(
                                                VarLocalIf,
                                                Expr.Constant(true)
                                            ),
                                            Expr.Assign(VarIf, VarLocalIf)
                                        )
                                 );

            MyIf = MyIf.ElseIf(
                                    ElseIf1Test,
                                    EU.BlockVoid(
                                        new[] { VarLocalElseIf },
                                        Expr.Assign(
                                            VarLocalElseIf,
                                            Expr.Constant(true)
                                        ),
                                        Expr.Assign(VarElseIf, VarLocalElseIf)
                                    )
                              );

            Expressions.Add(
                                MyIf.Else(
                                    EU.BlockVoid(
                                        new[] { VarLocalElse },
                                        Expr.Assign(
                                            VarLocalElse,
                                            Expr.Constant(true)
                                        ),
                                        Expr.Assign(VarElse, VarLocalElse)
                                    )
                                )
                            );

            Expressions.Add(ETUtils.ExpressionUtils.GenAreEqual(VarIf, Expr.Constant(false), "If 4"));
            Expressions.Add(ETUtils.ExpressionUtils.GenAreEqual(VarElseIf, Expr.Constant(false), "ElseIf 4"));
            Expressions.Add(ETUtils.ExpressionUtils.GenAreEqual(VarElse, Expr.Constant(true), "Else 4"));



            var tree = EU.BlockVoid(new[] { VarIf, VarElseIf, VarElse }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //Define a scope on the main block, on an elseif block, and on an else block
        //if referred to outside the if, an exception occurs.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "If() 16", new string[] { "negative", "if", "scope", "controlflow", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr If16(EU.IValidator V) {
            List<Expr> Expressions = new List<Expr>();

            ParameterExpression VarLocalIf = Expr.Variable(typeof(Boolean), "VarLocalIf");
            ParameterExpression VarLocalElseIf = Expr.Variable(typeof(Boolean), "VarLocalElseIf");
            ParameterExpression VarLocalElse = Expr.Variable(typeof(Boolean), "VarLocalElse");

            ConstantExpression IfTest = Expr.Constant(true);
            ConstantExpression ElseIf1Test = Expr.Constant(false);

            IfStatementBuilder MyIf = AstUtils.If(
                                        IfTest,
                                        EU.BlockVoid(
                                            new[] { VarLocalIf },
                                            Expr.Assign(
                                                VarLocalIf,
                                                Expr.Constant(true)
                                            )
                                        )
                                 );

            MyIf = MyIf.ElseIf(
                                    ElseIf1Test,
                                    EU.BlockVoid(
                                        new[] { VarLocalElseIf },
                                        Expr.Assign(
                                            VarLocalElseIf,
                                            Expr.Constant(true)
                                        )
                                    )
                              );

            Expressions.Add(
                                MyIf.Else(
                                    EU.BlockVoid(
                                        new[] { VarLocalElse },
                                        Expr.Assign(
                                            VarLocalElse,
                                            Expr.Constant(true)
                                        )
                                    )
                                )
                            );


            Expressions.Add(Expr.Assign(VarLocalIf, Expr.Constant(false)));
            var tree = EU.BlockVoid(Expressions);
            V.ValidateException<InvalidOperationException>(tree, true);
            return tree;
        }

        //Define a scope on the main block, on an elseif block, and on an else block
        //if referred to outside the if, an exception occurs.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "If() 17", new string[] { "negative", "if", "scope", "controlflow", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr If17(EU.IValidator V) {
            List<Expr> Expressions = new List<Expr>();

            ParameterExpression VarLocalIf = Expr.Variable(typeof(Boolean), "VarLocalIf");
            ParameterExpression VarLocalElseIf = Expr.Variable(typeof(Boolean), "VarLocalElseIf");
            ParameterExpression VarLocalElse = Expr.Variable(typeof(Boolean), "VarLocalElse");

            ConstantExpression IfTest = Expr.Constant(true);
            ConstantExpression ElseIf1Test = Expr.Constant(false);

            IfStatementBuilder MyIf = AstUtils.If(
                                        IfTest,
                                        EU.BlockVoid(
                                            new[] { VarLocalIf },
                                            Expr.Assign(
                                                VarLocalIf,
                                                Expr.Constant(true)
                                            )
                                        )
                                 );

            MyIf = MyIf.ElseIf(
                                    ElseIf1Test,
                                    EU.BlockVoid(
                                        new[] { VarLocalElseIf },
                                        Expr.Assign(
                                            VarLocalElseIf,
                                            Expr.Constant(true)
                                        )
                                    )
                              );

            Expressions.Add(
                                MyIf.Else(
                                    EU.BlockVoid(
                                        new[] { VarLocalElse },
                                        Expr.Assign(
                                            VarLocalElse,
                                            Expr.Constant(true)
                                        )
                                    )
                                )
                            );


            Expressions.Add(Expr.Assign(VarLocalElseIf, Expr.Constant(false)));

            var tree = EU.BlockVoid(Expressions);
            V.ValidateException<InvalidOperationException>(tree, true);
            return tree;
        }

        //Define a scope on the main block, on an elseif block, and on an else block
        //if referred to outside the if, an exception occurs.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "If() 18", new string[] { "negative", "if", "scope", "controlflow", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr If18(EU.IValidator V) {
            List<Expr> Expressions = new List<Expr>();

            ParameterExpression VarLocalIf = Expr.Variable(typeof(Boolean), "VarLocalIf");
            ParameterExpression VarLocalElseIf = Expr.Variable(typeof(Boolean), "VarLocalElseIf");
            ParameterExpression VarLocalElse = Expr.Variable(typeof(Boolean), "VarLocalElse");

            ConstantExpression IfTest = Expr.Constant(true);
            ConstantExpression ElseIf1Test = Expr.Constant(false);

            IfStatementBuilder MyIf = AstUtils.If(
                                        IfTest,
                                        EU.BlockVoid(
                                            new[] { VarLocalIf },
                                            Expr.Assign(
                                                VarLocalIf,
                                                Expr.Constant(true)
                                            )
                                        )
                                 );

            MyIf = MyIf.ElseIf(
                                    ElseIf1Test,
                                    EU.BlockVoid(
                                        new[] { VarLocalElseIf },
                                        Expr.Assign(
                                            VarLocalElseIf,
                                            Expr.Constant(true)
                                        )
                                    )
                              );

            Expressions.Add(
                                MyIf.Else(
                                    EU.BlockVoid(
                                        new[] { VarLocalElse },
                                        Expr.Assign(
                                            VarLocalElse,
                                            Expr.Constant(true)
                                        )
                                    )
                                )
                            );


            Expressions.Add(Expr.Assign(VarLocalElse, Expr.Constant(false)));

            var tree = EU.BlockVoid(Expressions);
            V.ValidateException<InvalidOperationException>(tree, true);
            return tree;
        }

        //Use if(expression, expression[]) to build a simple if state4ment
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "If() 19", new string[] { "positive", "if", "scope", "controlflow", "Pri2" })]
        public static Expr If19(EU.IValidator V) {
            List<Expr> Expressions = new List<Expr>();

            ParameterExpression VarIf = Expr.Variable(typeof(Boolean), "");
            ParameterExpression VarElse = Expr.Variable(typeof(Boolean), "");

            IfStatementBuilder MyIf = AstUtils.If(Expr.Constant(true), Expr.Assign(VarIf, Expr.Constant(true)));
            Expressions.Add(MyIf.Else(Expr.Assign(VarElse, Expr.Constant(true))));

            Expressions.Add(EU.GenAreEqual(VarIf, Expr.Constant(true), "If should have run 1"));
            Expressions.Add(EU.GenAreEqual(VarElse, Expr.Constant(false), "Else shouldn't have run 1"));

            //second case:
            Expressions.Add(Expr.Assign(VarIf, Expr.Constant(false)));
            Expressions.Add(Expr.Assign(VarElse, Expr.Constant(false)));


            MyIf = AstUtils.If(Expr.Constant(false), Expr.Assign(VarIf, Expr.Constant(true)));
            Expressions.Add(MyIf.Else(Expr.Assign(VarElse, Expr.Constant(true))));

            Expressions.Add(EU.GenAreEqual(VarIf, Expr.Constant(false), "If should not have run 2"));
            Expressions.Add(EU.GenAreEqual(VarElse, Expr.Constant(true), "Else should have run 2"));

            var tree = EU.BlockVoid(new[] { VarIf, VarElse }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //pass null to the test argument ( if(expression, expression[])) 
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "If() 20", new string[] { "negative", "if", "scope", "controlflow", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr If20(EU.IValidator V) {
            return EU.Throws<System.ArgumentNullException>(() => { AstUtils.If(null, new Expr[] { Expr.Constant(true) }); });
        }

        //pass null to the body argument ( if(expression, expression[])) 
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "If() 21", new string[] { "negative", "if", "scope", "controlflow", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr If21(EU.IValidator V) {
            return EU.Throws<System.ArgumentNullException>(() => { AstUtils.If(Expr.Constant(true), (Expr[])null); });
        }

        //pass empty array to the body argument ( if(expression, expression[])) 
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "If() 22", new string[] { "positive", "if", "scope", "controlflow", "Pri2" })]
        public static Expr If22(EU.IValidator V) {
            IfStatementBuilder MyIf = AstUtils.If(Expr.Constant(true), new Expr[] { });
            var tree = EU.BlockVoid(MyIf.Else(Expr.Throw(Expr.New(typeof(Exception)))));
            V.Validate(tree);
            return tree;
        }

        //Pass a scope to test (if(expression, expression[]))
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "If() 23", new string[] { "positive", "if", "scope", "controlflow", "Pri2" })]
        public static Expr If23(EU.IValidator V) {
            List<Expr> Expressions = new List<Expr>();

            ParameterExpression VarIf = Expr.Variable(typeof(Boolean), "");
            ParameterExpression VarElse = Expr.Variable(typeof(Boolean), "");
            ParameterExpression VTest = Expr.Variable(typeof(Boolean), "");

            BlockExpression Test = Expr.Block(Expr.Assign(VTest, Expr.Constant(true)), VTest);

            IfStatementBuilder MyIf = AstUtils.If(Expr.Block(new[] { VTest }, Test), Expr.Assign(VarIf, Expr.Constant(true)));
            Expressions.Add(MyIf.Else(Expr.Assign(VarElse, Expr.Constant(true))));

            Expressions.Add(EU.GenAreEqual(VarIf, Expr.Constant(true), "If should have run 1"));
            Expressions.Add(EU.GenAreEqual(VarElse, Expr.Constant(false), "Else shouldn't have run 1"));

            //second case:
            Expressions.Add(Expr.Assign(VarIf, Expr.Constant(false)));
            Expressions.Add(Expr.Assign(VarElse, Expr.Constant(false)));


            Test = Expr.Block(Expr.Assign(VTest, Expr.Constant(false)), VTest);

            MyIf = AstUtils.If(Expr.Block(new[] { VTest }, Test), Expr.Assign(VarIf, Expr.Constant(true)));
            Expressions.Add(MyIf.Else(Expr.Assign(VarElse, Expr.Constant(true))));

            Expressions.Add(EU.GenAreEqual(VarIf, Expr.Constant(false), "If should not have run 2"));
            Expressions.Add(EU.GenAreEqual(VarElse, Expr.Constant(true), "Else should have run 2"));

            var tree = EU.BlockVoid(new[] { VarIf, VarElse }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //If(IfStatementTest[] tests, expression else) - Use if(...) to build an if statement with two else ifs and an else
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "If() 24", new string[] { "positive", "if", "scope", "controlflow", "Pri2" })]
        public static Expr If24(EU.IValidator V) {
            List<Expr> Expressions = new List<Expr>();

            ParameterExpression Result = Expr.Variable(typeof(String), "");
            BlockExpression TestIf = Expr.Block(EU.ConcatEquals(Result, "_IfTest_"), Expr.Constant(false));
            BlockExpression TestElseIf1 = Expr.Block(EU.ConcatEquals(Result, "_ElseIfTest1_"), Expr.Constant(false));
            BlockExpression TestElseIf2 = Expr.Block(EU.ConcatEquals(Result, "_ElseIfTest2_"), Expr.Constant(false));


            IfStatementTest IfBlock = AstUtils.IfCondition(TestIf, EU.ConcatEquals(Result, "_If_"));
            IfStatementTest IfElse1 = AstUtils.IfCondition(TestElseIf1, EU.ConcatEquals(Result, "_ElseIf1_"));
            IfStatementTest IfElse2 = AstUtils.IfCondition(TestElseIf2, EU.ConcatEquals(Result, "_ElseIf2_"));
            Expression ElseBlock = EU.ConcatEquals(Result, "_Else_");

            Expression MyIf = AstUtils.If(new IfStatementTest[] { IfBlock, IfElse1, IfElse2 }, ElseBlock);
            Expressions.Add(MyIf);

            Expressions.Add(EU.GenAreEqual(Result, Expr.Constant("_IfTest__ElseIfTest1__ElseIfTest2__Else_"), "If Execution was unexpected 1"));

            //second case:
            Expressions.Add(Expr.Assign(Result, Expr.Constant("")));
            TestIf = Expr.Block(EU.ConcatEquals(Result, "_IfTest_"), Expr.Constant(true));
            TestElseIf1 = Expr.Block(EU.ConcatEquals(Result, "_ElseIfTest1_"), Expr.Constant(false));
            TestElseIf2 = Expr.Block(EU.ConcatEquals(Result, "_ElseIfTest2_"), Expr.Constant(false));


            IfBlock = AstUtils.IfCondition(TestIf, EU.ConcatEquals(Result, "_If_"));
            IfElse1 = AstUtils.IfCondition(TestElseIf1, EU.ConcatEquals(Result, "_ElseIf1_"));
            IfElse2 = AstUtils.IfCondition(TestElseIf2, EU.ConcatEquals(Result, "_ElseIf2_"));
            ElseBlock = EU.ConcatEquals(Result, "_Else_");

            MyIf = AstUtils.If(new IfStatementTest[] { IfBlock, IfElse1, IfElse2 }, ElseBlock);
            Expressions.Add(MyIf);

            Expressions.Add(EU.GenAreEqual(Result, Expr.Constant("_IfTest__If_"), "If Execution was unexpected 1"));

            //Third case
            Expressions.Add(Expr.Assign(Result, Expr.Constant("")));
            TestIf = Expr.Block(EU.ConcatEquals(Result, "_IfTest_"), Expr.Constant(false));
            TestElseIf1 = Expr.Block(EU.ConcatEquals(Result, "_ElseIfTest1_"), Expr.Constant(true));
            TestElseIf2 = Expr.Block(EU.ConcatEquals(Result, "_ElseIfTest2_"), Expr.Constant(true));

            IfBlock = AstUtils.IfCondition(TestIf, EU.ConcatEquals(Result, "_If_"));
            IfElse1 = AstUtils.IfCondition(TestElseIf1, EU.ConcatEquals(Result, "_ElseIf1_"));
            IfElse2 = AstUtils.IfCondition(TestElseIf2, EU.ConcatEquals(Result, "_ElseIf2_"));
            ElseBlock = EU.ConcatEquals(Result, "_Else_");

            MyIf = AstUtils.If(new IfStatementTest[] { IfBlock, IfElse1, IfElse2 }, ElseBlock);
            Expressions.Add(MyIf);

            Expressions.Add(EU.GenAreEqual(Result, Expr.Constant("_IfTest__ElseIfTest1__ElseIf1_"), "If Execution was unexpected 1"));

            //Fourth Case
            Expressions.Add(Expr.Assign(Result, Expr.Constant("")));
            TestIf = Expr.Block(EU.ConcatEquals(Result, "_IfTest_"), Expr.Constant(false));
            TestElseIf1 = Expr.Block(EU.ConcatEquals(Result, "_ElseIfTest1_"), Expr.Constant(false));
            TestElseIf2 = Expr.Block(EU.ConcatEquals(Result, "_ElseIfTest2_"), Expr.Constant(true));

            IfBlock = AstUtils.IfCondition(TestIf, EU.ConcatEquals(Result, "_If_"));
            IfElse1 = AstUtils.IfCondition(TestElseIf1, EU.ConcatEquals(Result, "_ElseIf1_"));
            IfElse2 = AstUtils.IfCondition(TestElseIf2, EU.ConcatEquals(Result, "_ElseIf2_"));
            ElseBlock = EU.ConcatEquals(Result, "_Else_");

            MyIf = AstUtils.If(new IfStatementTest[] { IfBlock, IfElse1, IfElse2 }, ElseBlock);
            Expressions.Add(MyIf);

            Expressions.Add(EU.GenAreEqual(Result, Expr.Constant("_IfTest__ElseIfTest1__ElseIfTest2__ElseIf2_"), "If Execution was unexpected 1"));

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //If(IfStatementTest[] tests, expression else) - Pass scope wrapped tests; pass a scope wrapped expression to else.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "If() 25", new string[] { "positive", "if", "scope", "controlflow", "Pri2" })]
        public static Expr If25(EU.IValidator V) {
            List<Expr> Expressions = new List<Expr>();

            ParameterExpression Result = Expr.Variable(typeof(String), "");
            ParameterExpression Test1 = Expr.Variable(typeof(String), "");
            ParameterExpression Test2 = Expr.Variable(typeof(String), "");
            ParameterExpression Test3 = Expr.Variable(typeof(String), "");
            ParameterExpression ElseText = Expr.Variable(typeof(String), "");

            BlockExpression TestIf = Expr.Block(new ParameterExpression[] { Test1 }, Expr.Assign(Test1, Expr.Constant("_IfTest_")), EU.ConcatEquals(Result, Test1), Expr.Constant(false));
            BlockExpression TestElseIf1 = Expr.Block(new ParameterExpression[] { Test2 }, Expr.Assign(Test2, Expr.Constant("_ElseIfTest1_")), EU.ConcatEquals(Result, Test2), Expr.Constant(false));
            BlockExpression TestElseIf2 = Expr.Block(new ParameterExpression[] { Test3 }, Expr.Assign(Test3, Expr.Constant("_ElseIfTest2_")), EU.ConcatEquals(Result, Test3), Expr.Constant(false));


            ParameterExpression VarElseIf = Expr.Variable(typeof(Exception), "");
            IfStatementTest IfBlock = AstUtils.IfCondition(TestIf, EU.ConcatEquals(Result, "_If_"));
            IfStatementTest IfElse1 = AstUtils.IfCondition(TestElseIf1, EU.ConcatEquals(Result, "_ElseIf1_"));
            IfStatementTest IfElse2 = AstUtils.IfCondition(TestElseIf2, Expr.Block(new[] { VarElseIf }, EU.ConcatEquals(Result, "_ElseIf2_")));
            BlockExpression ElseBlock = EU.BlockVoid(new ParameterExpression[] { ElseText }, Expr.Assign(ElseText, Expr.Constant("_Else_")), EU.ConcatEquals(Result, ElseText));

            Expression MyIf = AstUtils.If(new IfStatementTest[] { IfBlock, IfElse1, IfElse2 }, ElseBlock);
            Expressions.Add(MyIf);

            Expressions.Add(EU.GenAreEqual(Result, Expr.Constant("_IfTest__ElseIfTest1__ElseIfTest2__Else_"), "If Execution was unexpected 1"));

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //If(IfStatementTest[] tests, expression else) - Pass null to the conditions argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "If() 26", new string[] { "negative", "if", "scope", "controlflow", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr If26(EU.IValidator V) {
            return EU.Throws<System.ArgumentNullException>(() => { AstUtils.If((IfStatementTest[])null, Expr.Constant("Hello")); });
        }
        //If(IfStatementTest[] tests, expression else) - Pass an empty array to the conditions argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "If() 27", new string[] { "positive", "if", "scope", "controlflow", "Pri2" })]
        public static Expr If27(EU.IValidator V) {
            List<Expr> Expressions = new List<Expr>();

            ParameterExpression Result = Expr.Variable(typeof(Boolean), "");
            Expressions.Add(AstUtils.If(new IfStatementTest[] { }, Expr.Assign(Result, Expr.Constant(true))));
            Expressions.Add(EU.GenAreEqual(Result, Expr.Constant(true)));

            var tree = EU.BlockVoid(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //If(IfStatementTest[] tests, expression else) - Pass an array with a null element to the conditions argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "If() 28", new string[] { "negative", "if", "scope", "controlflow", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr If28(EU.IValidator V) {
            IfStatementTest test1 = AstUtils.IfCondition(Expr.Constant(true), Expr.Constant("Hello!"));
            IfStatementTest test2 = AstUtils.IfCondition(Expr.Constant(true), Expr.Constant("Hello..."));
            var tree = EU.Throws<System.ArgumentNullException>(() => { AstUtils.If(new IfStatementTest[] { test1, null, test2 }, Expr.Constant("Hello")); });
            return tree;
        }


        //If(IfStatementTest[] tests, expression else) - //pass null to else.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "If() 29", new string[] { "positive", "if", "scope", "controlflow", "Pri2" })]
        public static Expr If29(EU.IValidator V) {
            List<Expr> Expressions = new List<Expr>();

            Expressions.Add(AstUtils.If(new IfStatementTest[] { }, null));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        //If(Expression test, expression body) - Pass null to the test argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "If() 30", new string[] { "negative", "if", "scope", "controlflow", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr If30(EU.IValidator V) {
            var tree = EU.Throws<System.ArgumentNullException>(() => { AstUtils.If((Expr)null, Expr.Constant("Hello")); });
            return tree;
        }

        //If(Expression test, expression body) - Pass null to the body argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "If() 31", new string[] { "negative", "if", "scope", "controlflow", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr If31(EU.IValidator V) {
            var tree = EU.Throws<System.ArgumentNullException>(() => { AstUtils.If(Expr.Constant(true), (Expr)null); });
            return tree;
        }

        //General If scenarios - Pass a nullable boolean
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "If() 32", new string[] { "negative", "if", "scope", "controlflow", "Pri2" }, Exception = typeof(System.ArgumentException))]
        public static Expr If32(EU.IValidator V) {
            List<Expr> Expressions = new List<Expr>();

            System.Reflection.ConstructorInfo Constructor = typeof(Nullable<Boolean>).GetConstructor(new Type[] { typeof(Boolean) });
            ParameterExpression Test = Expr.Variable(typeof(Nullable<Boolean>), "");
            ParameterExpression VarIf = Expr.Variable(typeof(Boolean), "");

            Expressions.Add(Expr.Assign(Test, Expr.New(Constructor, Expr.Constant(true))));
            Expression MyIf = EU.Throws<System.ArgumentException>(() => { AstUtils.If(new IfStatementTest[] { AstUtils.IfCondition(Test, Expr.Assign(VarIf, Expr.Constant(true))) }, null); });

            return Expr.Empty();
        }

        // If(Expression, Expression)
        // Scenario/Test : Use If(Expression, expression) to build a simple if statement
        // Expected      : Executed main block or not, according to test
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "If() 33", new string[] { "positive", "IfStatement", "ControlFlow", "Pri3" })]
        public static Expr If33(EU.IValidator V) {
            List<Expr> Expressions = new List<Expr>();

            ParameterExpression VarIf = Expr.Variable(typeof(Boolean), "");

            Expression MyIf = AstUtils.If(new IfStatementTest[] { AstUtils.IfCondition(Expr.Constant(true), Expr.Assign(VarIf, Expr.Constant(true))) }, null);

            Expressions.Add(MyIf);
            Expressions.Add(EU.GenAreEqual(VarIf, Expr.Constant(true)));

            var tree = EU.BlockVoid(new[] { VarIf }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // If(Expression, Expression)
        // Scenario/Test : Pass a block wrapped expression to test
        // Expected      : Should work
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "If() 34", new string[] { "positive", "IfStatement", "ControlFlow", "Pri3" })]
        public static Expr If34(EU.IValidator V) {
            List<Expr> Expressions = new List<Expr>();

            Expression Test = Expr.Block(new Expression[] { Expr.Constant(true) });

            ParameterExpression VarIf = Expr.Variable(typeof(Boolean), "");

            Expression MyIf = AstUtils.If(new IfStatementTest[] { AstUtils.IfCondition(Test, Expr.Assign(VarIf, Expr.Constant(true))) }, null);

            Expressions.Add(MyIf);
            Expressions.Add(EU.GenAreEqual(VarIf, Expr.Constant(true)));

            var tree = EU.BlockVoid(new[] { VarIf }, Expressions);
            V.Validate(tree);
            return tree;

        }

        // If(Expression, Expression)
        // Scenario/Test : Pass a block wrapped expression to body
        // Expected      : Should work
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "If() 35", new string[] { "positive", "IfStatement", "ControlFlow", "Pri3" })]
        public static Expr If35(EU.IValidator V) {
            List<Expr> Expressions = new List<Expr>();

            ParameterExpression VarIf = Expr.Variable(typeof(Boolean), "");

            Expression Body = Expr.Block(new Expression[] { Expr.Assign(VarIf, Expr.Constant(true)) });

            Expression MyIf = AstUtils.If(new IfStatementTest[] { AstUtils.IfCondition(Expr.Constant(true), Body) }, null);

            Expressions.Add(MyIf);
            Expressions.Add(EU.GenAreEqual(VarIf, Expr.Constant(true)));

            var tree = EU.BlockVoid(new[] { VarIf }, Expressions);
            V.Validate(tree);
            return tree;

        }

        // IfStatementBuilder
        // Scenario/Test : Use an ifstatementbuilder to construct an if with multiple elseif clauses 
        //                 and an else clause, exercise both all paths
        // Expected      : Check order of execution of elseif statements. Check that only the first 
        //                 tests are executed, and after one passes no test is executed.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "If() 36", new string[] { "positive", "IfStatement", "ControlFlow", "Pri3" })]
        public static Expr If36(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            Expressions.Add(If36Helper0());
            Expressions.Add(If36Helper1());
            Expressions.Add(If36Helper2());
            Expressions.Add(If36Helper3());
            Expressions.Add(If36Helper4());
            Expressions.Add(If36Helper5());

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static Expr If36Helper0() {
            List<Expr> Expressions = new List<Expr>();

            ParameterExpression VarLocalIf = Expr.Variable(typeof(Boolean), "VarLocalIf");
            ParameterExpression VarLocalElseIf = Expr.Variable(typeof(Boolean), "VarLocalElseIf");
            ParameterExpression VarLocal2ElseIf = Expr.Variable(typeof(Boolean), "VarLocal2ElseIf");
            ParameterExpression VarLocalElse = Expr.Variable(typeof(Boolean), "VarLocalElse");


            ConstantExpression IfTest1 = Expr.Constant(true);
            ConstantExpression IfTest2 = Expr.Constant(false);
            ConstantExpression IfTest3 = Expr.Constant(false);

            IfStatementBuilder MyIf = AstUtils.If(
                                        IfTest1,
                                        EU.BlockVoid(
                                            new ParameterExpression[] { },
                                            Expr.Assign(
                                                VarLocalIf,
                                                Expr.Constant(true)
                                            )
                                        )
                                 );

            MyIf = MyIf.ElseIf(
                                    IfTest2,
                                    EU.BlockVoid(
                                        new ParameterExpression[] { },
                                        Expr.Assign(
                                            VarLocalElseIf,
                                            Expr.Constant(true)
                                        )
                                    )
                              );


            MyIf = MyIf.ElseIf(
                                    IfTest3,
                                    EU.BlockVoid(
                                        new ParameterExpression[] { },
                                        Expr.Assign(
                                            VarLocal2ElseIf,
                                            Expr.Constant(true)
                                        )
                                    )
                              );

            Expression IfTree1 = MyIf.Else(
                                    EU.BlockVoid(
                                        new ParameterExpression[] { },
                                        Expr.Assign(
                                            VarLocalElse,
                                            Expr.Constant(true)
                                        )
                                    )
                                );


            Expressions.Add(IfTree1);
            Expressions.Add(EU.GenAreEqual(Expr.Constant(true), VarLocalIf, "Test Inside If36Helper0"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(false), VarLocalElseIf, "Test Inside If36Helper0"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(false), VarLocal2ElseIf, "Test Inside If36Helper0"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(false), VarLocalElse, "Test Inside If36Helper0"));

            return EU.BlockVoid(new[] { VarLocalIf, VarLocalElseIf, VarLocal2ElseIf, VarLocalElse }, Expressions);
        }

        private static Expr If36Helper1() {
            List<Expr> Expressions = new List<Expr>();

            ParameterExpression VarLocalIf = Expr.Variable(typeof(Boolean), "VarLocalIf");
            ParameterExpression VarLocalElseIf = Expr.Variable(typeof(Boolean), "VarLocalElseIf");
            ParameterExpression VarLocal2ElseIf = Expr.Variable(typeof(Boolean), "VarLocal2ElseIf");
            ParameterExpression VarLocalElse = Expr.Variable(typeof(Boolean), "VarLocalElse");


            ConstantExpression IfTest1 = Expr.Constant(false);
            ConstantExpression IfTest2 = Expr.Constant(true);
            ConstantExpression IfTest3 = Expr.Constant(false);

            IfStatementBuilder MyIf = AstUtils.If(
                                        IfTest1,
                                        EU.BlockVoid(
                                            new ParameterExpression[] { },
                                            Expr.Assign(
                                                VarLocalIf,
                                                Expr.Constant(true)
                                            )
                                        )
                                 );

            MyIf = MyIf.ElseIf(
                                    IfTest2,
                                    EU.BlockVoid(
                                        new ParameterExpression[] { },
                                        Expr.Assign(
                                            VarLocalElseIf,
                                            Expr.Constant(true)
                                        )
                                    )
                              );


            MyIf = MyIf.ElseIf(
                                    IfTest3,
                                    EU.BlockVoid(
                                        new ParameterExpression[] { },
                                        Expr.Assign(
                                            VarLocal2ElseIf,
                                            Expr.Constant(true)
                                        )
                                    )
                              );

            Expression IfTree1 = MyIf.Else(
                                    EU.BlockVoid(
                                        new ParameterExpression[] { },
                                        Expr.Assign(
                                            VarLocalElse,
                                            Expr.Constant(true)
                                        )
                                    )
                                );


            Expressions.Add(IfTree1);
            Expressions.Add(EU.GenAreEqual(Expr.Constant(false), VarLocalIf, "Test Inside If36Helper1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(true), VarLocalElseIf, "Test Inside If36Helper1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(false), VarLocal2ElseIf, "Test Inside If36Helper1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(false), VarLocalElse, "Test Inside If36Helper1"));

            return EU.BlockVoid(new[] { VarLocalIf, VarLocalElseIf, VarLocal2ElseIf, VarLocalElse }, Expressions);
        }

        private static Expr If36Helper2() {
            List<Expr> Expressions = new List<Expr>();

            ParameterExpression VarLocalIf = Expr.Variable(typeof(Boolean), "VarLocalIf");
            ParameterExpression VarLocalElseIf = Expr.Variable(typeof(Boolean), "VarLocalElseIf");
            ParameterExpression VarLocal2ElseIf = Expr.Variable(typeof(Boolean), "VarLocal2ElseIf");
            ParameterExpression VarLocalElse = Expr.Variable(typeof(Boolean), "VarLocalElse");


            ConstantExpression IfTest1 = Expr.Constant(false);
            ConstantExpression IfTest2 = Expr.Constant(false);
            ConstantExpression IfTest3 = Expr.Constant(true);

            IfStatementBuilder MyIf = AstUtils.If(
                                        IfTest1,
                                        EU.BlockVoid(
                                            new ParameterExpression[] { },
                                            Expr.Assign(
                                                VarLocalIf,
                                                Expr.Constant(true)
                                            )
                                        )
                                 );

            MyIf = MyIf.ElseIf(
                                    IfTest2,
                                    EU.BlockVoid(
                                        new ParameterExpression[] { },
                                        Expr.Assign(
                                            VarLocalElseIf,
                                            Expr.Constant(true)
                                        )
                                    )
                              );


            MyIf = MyIf.ElseIf(
                                    IfTest3,
                                    EU.BlockVoid(
                                        new ParameterExpression[] { },
                                        Expr.Assign(
                                            VarLocal2ElseIf,
                                            Expr.Constant(true)
                                        )
                                    )
                              );

            Expression IfTree1 = MyIf.Else(
                                    EU.BlockVoid(
                                        new ParameterExpression[] { },
                                        Expr.Assign(
                                            VarLocalElse,
                                            Expr.Constant(true)
                                        )
                                    )
                                );


            Expressions.Add(IfTree1);
            Expressions.Add(EU.GenAreEqual(Expr.Constant(false), VarLocalIf, "Test Inside If36Helper2"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(false), VarLocalElseIf, "Test Inside If36Helper2"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(true), VarLocal2ElseIf, "Test Inside If36Helper2"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(false), VarLocalElse, "Test Inside If36Helper2"));

            return EU.BlockVoid(new[] { VarLocalIf, VarLocalElseIf, VarLocal2ElseIf, VarLocalElse }, Expressions);
        }

        private static Expr If36Helper3() {
            List<Expr> Expressions = new List<Expr>();

            ParameterExpression VarLocalIf = Expr.Variable(typeof(Boolean), "VarLocalIf");
            ParameterExpression VarLocalElseIf = Expr.Variable(typeof(Boolean), "VarLocalElseIf");
            ParameterExpression VarLocal2ElseIf = Expr.Variable(typeof(Boolean), "VarLocal2ElseIf");
            ParameterExpression VarLocalElse = Expr.Variable(typeof(Boolean), "VarLocalElse");


            ConstantExpression IfTest1 = Expr.Constant(false);
            ConstantExpression IfTest2 = Expr.Constant(false);
            ConstantExpression IfTest3 = Expr.Constant(false);

            IfStatementBuilder MyIf = AstUtils.If(
                                        IfTest1,
                                        EU.BlockVoid(
                                            new ParameterExpression[] { },
                                            Expr.Assign(
                                                VarLocalIf,
                                                Expr.Constant(true)
                                            )
                                        )
                                 );

            MyIf = MyIf.ElseIf(
                                    IfTest2,
                                    EU.BlockVoid(
                                        new ParameterExpression[] { },
                                        Expr.Assign(
                                            VarLocalElseIf,
                                            Expr.Constant(true)
                                        )
                                    )
                              );


            MyIf = MyIf.ElseIf(
                                    IfTest3,
                                    EU.BlockVoid(
                                        new ParameterExpression[] { },
                                        Expr.Assign(
                                            VarLocal2ElseIf,
                                            Expr.Constant(true)
                                        )
                                    )
                              );

            Expression IfTree1 = MyIf.Else(
                                    EU.BlockVoid(
                                        new ParameterExpression[] { },
                                        Expr.Assign(
                                            VarLocalElse,
                                            Expr.Constant(true)
                                        )
                                    )
                                );


            Expressions.Add(IfTree1);
            Expressions.Add(EU.GenAreEqual(Expr.Constant(false), VarLocalIf, "Test Inside If36Helper3"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(false), VarLocalElseIf, "Test Inside If36Helper3"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(false), VarLocal2ElseIf, "Test Inside If36Helper3"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(true), VarLocalElse, "Test Inside If36Helper3"));

            return EU.BlockVoid(new[] { VarLocalIf, VarLocalElseIf, VarLocal2ElseIf, VarLocalElse }, Expressions);
        }

        private static Expr If36Helper4() {
            List<Expr> Expressions = new List<Expr>();

            ParameterExpression VarLocalIf = Expr.Variable(typeof(Boolean), "VarLocalIf");
            ParameterExpression VarLocalElseIf = Expr.Variable(typeof(Boolean), "VarLocalElseIf");
            ParameterExpression VarLocal2ElseIf = Expr.Variable(typeof(Boolean), "VarLocal2ElseIf");
            ParameterExpression VarLocalElse = Expr.Variable(typeof(Boolean), "VarLocalElse");


            ConstantExpression IfTest1 = Expr.Constant(true);
            ConstantExpression IfTest2 = Expr.Constant(true);
            ConstantExpression IfTest3 = Expr.Constant(true);

            IfStatementBuilder MyIf = AstUtils.If(
                                        IfTest1,
                                        EU.BlockVoid(
                                            new ParameterExpression[] { },
                                            Expr.Assign(
                                                VarLocalIf,
                                                Expr.Constant(true)
                                            )
                                        )
                                 );

            MyIf = MyIf.ElseIf(
                                    IfTest2,
                                    EU.BlockVoid(
                                        new ParameterExpression[] { },
                                        Expr.Assign(
                                            VarLocalElseIf,
                                            Expr.Constant(true)
                                        )
                                    )
                              );


            MyIf = MyIf.ElseIf(
                                    IfTest3,
                                    EU.BlockVoid(
                                        new ParameterExpression[] { },
                                        Expr.Assign(
                                            VarLocal2ElseIf,
                                            Expr.Constant(true)
                                        )
                                    )
                              );

            Expression IfTree1 = MyIf.Else(
                                    EU.BlockVoid(
                                        new ParameterExpression[] { },
                                        Expr.Assign(
                                            VarLocalElse,
                                            Expr.Constant(true)
                                        )
                                    )
                                );


            Expressions.Add(IfTree1);
            Expressions.Add(EU.GenAreEqual(Expr.Constant(true), VarLocalIf, "Test Inside If36Helper4"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(false), VarLocalElseIf, "Test Inside If36Helper4"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(false), VarLocal2ElseIf, "Test Inside If36Helper4"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(false), VarLocalElse, "Test Inside If36Helper4"));

            return EU.BlockVoid(new[] { VarLocalIf, VarLocalElseIf, VarLocal2ElseIf, VarLocalElse }, Expressions);
        }

        private static Expr If36Helper5() {
            List<Expr> Expressions = new List<Expr>();

            ParameterExpression VarLocalIf = Expr.Variable(typeof(Boolean), "VarLocalIf");
            ParameterExpression VarLocalElseIf = Expr.Variable(typeof(Boolean), "VarLocalElseIf");
            ParameterExpression VarLocal2ElseIf = Expr.Variable(typeof(Boolean), "VarLocal2ElseIf");
            ParameterExpression VarLocalElse = Expr.Variable(typeof(Boolean), "VarLocalElse");


            ConstantExpression IfTest1 = Expr.Constant(false);
            ConstantExpression IfTest2 = Expr.Constant(true);
            ConstantExpression IfTest3 = Expr.Constant(true);

            IfStatementBuilder MyIf = AstUtils.If(
                                        IfTest1,
                                        EU.BlockVoid(
                                            new ParameterExpression[] { },
                                            Expr.Assign(
                                                VarLocalIf,
                                                Expr.Constant(true)
                                            )
                                        )
                                 );

            MyIf = MyIf.ElseIf(
                                    IfTest2,
                                    EU.BlockVoid(
                                        new ParameterExpression[] { },
                                        Expr.Assign(
                                            VarLocalElseIf,
                                            Expr.Constant(true)
                                        )
                                    )
                              );


            MyIf = MyIf.ElseIf(
                                    IfTest3,
                                    EU.BlockVoid(
                                        new ParameterExpression[] { },
                                        Expr.Assign(
                                            VarLocal2ElseIf,
                                            Expr.Constant(true)
                                        )
                                    )
                              );

            Expression IfTree1 = MyIf.Else(
                                    EU.BlockVoid(
                                        new ParameterExpression[] { },
                                        Expr.Assign(
                                            VarLocalElse,
                                            Expr.Constant(true)
                                        )
                                    )
                                );


            Expressions.Add(IfTree1);
            Expressions.Add(EU.GenAreEqual(Expr.Constant(false), VarLocalIf, "Test Inside If36Helper5"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(true), VarLocalElseIf, "Test Inside If36Helper5"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(false), VarLocal2ElseIf, "Test Inside If36Helper5"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(false), VarLocalElse, "Test Inside If36Helper5"));

            return EU.BlockVoid(new[] { VarLocalIf, VarLocalElseIf, VarLocal2ElseIf, VarLocalElse }, Expressions);
        }

        // bool arg is just to avoid an unreachable code warning
        private static Expr If37Test(bool AlwaysTrue) {
            if (AlwaysTrue) {
                throw new ArgumentException("If37Test Test ArgumentException");
            }
            return Expr.Constant(true);
        }
        // General If Scenarios
        // Scenario/Test : Throw an exception on the test element
        // Expected      : Exception should be thrown, handleable. No if block should be executed
        // Note          : After exception is thrown body if block will not be excuted
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "If() 37", new string[] { "negative", "IfStatement", "ControlFlow", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr If37(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression TestVar = Expr.Variable(typeof(Boolean), "TestVar");

            var ET = EU.Throws<System.ArgumentException>(() =>
            {
                AstUtils.If(If37Test(true), new Expression[] { Expr.Assign(TestVar, Expr.Constant(true)) });
            });

            Expressions.Add(ET);
            return Expr.Block(Expressions);
        }

        // General If Scenarios
        // Scenario/Test : Pass an expression that doesn't converts to Boolean
        // Expected      : Exception
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "If() 39", new string[] { "negative", "IfStatement", "ControlFlow", "Pri2" }, Exception = typeof(InvalidOperationException))]
        public static Expr If39(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var ET = EU.Throws<System.InvalidOperationException>(() =>
            {
                AstUtils.If(Expr.Convert(Expr.Empty(), typeof(Boolean)), new Expression[] { Expr.Empty() });
            });

            Expressions.Add(ET);
            var tree = Expr.Block(Expressions);

            return tree;
        }


        public class UserDefinedConversion {
            public int _value;
            public UserDefinedConversion() { }
            public UserDefinedConversion(int value) {
                _value = value;
            }
            public static explicit operator ConstantExpression(UserDefinedConversion udc) {
                return Expr.Constant(Convert.ToBoolean(udc._value), typeof(Boolean));
            }
        }

        // General If Scenarios
        // Scenario/Test : Pass a type that has a user defined conversion to Boolean
        // Expected      : Should work?
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "If() 40", new string[] { "positive", "IfStatement", "ControlFlow", "Pri2" })]
        public static Expr If40(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            UserDefinedConversion udc = new UserDefinedConversion(1);

            ParameterExpression TestVar = Expr.Variable(typeof(Boolean), "TestVar");

            Expression ET = AstUtils.If((ConstantExpression)udc, new Expression[] { Expr.Assign(TestVar, Expr.Constant(true)) });

            Expressions.Add(ET);
            Expressions.Add(EU.GenAreEqual(Expr.Constant(true), TestVar));
            var tree = Expr.Block(new ParameterExpression[] { TestVar }, Expressions);
            V.Validate(tree);
            return tree;

        }

        // General If Scenarios
        // Scenario/Test : Pass a comma to the test â€“ have most execution being unrelated to the test result. 
        // Expected      : Legal
        // Note          : Comma has been removed. Using Expr.Block(...) instead.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "If() 41", new string[] { "positive", "IfStatement", "ControlFlow", "Pri2" })]
        public static Expr If41(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression TestVar = Expr.Variable(typeof(Boolean), "TestVar");

            Expression ET = AstUtils.If(Expr.Block(new Expression[] { Expr.Empty(), Expr.Empty(), Expr.Constant(true) }),
                                         new Expression[] { Expr.Assign(TestVar, Expr.Constant(true)) }
                                       );

            Expressions.Add(ET);
            Expressions.Add(EU.GenAreEqual(Expr.Constant(true), TestVar));
            var tree = Expr.Block(new ParameterExpression[] { TestVar }, Expressions);
            V.Validate(tree);
            return tree;

        }


        // General If Scenarios
        // Scenario/Test : Define nested ifs
        // Expected      : Validate execution flow
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "If() 43", new string[] { "positive", "IfStatement", "ControlFlow", "Pri2" })]
        public static Expr If43(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression TestVar = Expr.Variable(typeof(Boolean), "TestVar");
            Expression ConsExpr = Expr.Convert(Expr.Constant(1), typeof(sbyte));

            Expression ET = AstUtils.If(Expr.Constant(true),
                                         new Expression[] {AstUtils.If(Expr.Constant(true), 
                                                                       new Expression[]{ Expr.Assign(TestVar, Expr.Constant(true))}
                                                                      )
                                                           }
                                       );

            Expressions.Add(ET);
            Expressions.Add(EU.GenAreEqual(Expr.Constant(true), TestVar));
            var tree = Expr.Block(new ParameterExpression[] { TestVar }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // General If Scenarios
        // Scenario/Test : Pass a nullable expression that is convertible to Boolean
        // Expected      : ? (Works)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "If() 44", new string[] { "positive", "IfStatement", "ControlFlow", "Pri2" })]
        public static Expr If44(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            // Nullable BoolValue passed to If Test

            Boolean? TestBool = true;
            ParameterExpression TestVar = Expr.Variable(typeof(Boolean), "TestVar");
            Expression ET = AstUtils.If(Expr.Constant(TestBool),
                                        new Expression[] { Expr.Assign(TestVar, Expr.Constant(true)) }

                                       );

            Expressions.Add(ET);
            Expressions.Add(EU.GenAreEqual(Expr.Constant(true), TestVar));
            var tree = Expr.Block(new ParameterExpression[] { TestVar }, Expressions);
            V.Validate(tree);
            return tree;

        }

        public class IsTrueFalseTest {
            // The three possible DBBool values.
            public static readonly IsTrueFalseTest Null = new IsTrueFalseTest(0);
            public static readonly IsTrueFalseTest False = new IsTrueFalseTest(-1);
            public static readonly IsTrueFalseTest True = new IsTrueFalseTest(1);

            //public int _value;
            sbyte value;
            public IsTrueFalseTest() { }
            public IsTrueFalseTest(int v) {
                value = (sbyte)v;
            }


            public static implicit operator IsTrueFalseTest(bool x) {
                return x ? True : False;
            }

            public static implicit operator ConstantExpression(IsTrueFalseTest udc)
            {
                return Expr.Constant(System.Convert.ToBoolean(udc.value), typeof(Boolean));

            }
            public static Boolean operator true(IsTrueFalseTest oitf) {
                return true;
            }
            public static Boolean operator false(IsTrueFalseTest oitf) {
                return true;
            }
        }

        // General If Scenarios
        // Scenario/Test : Pass an expression that converts to Boolean
        // Expected      : ? (Worked!)
        // NOte          : better example of earlier test 
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "If() 45", new string[] { "positive", "IfStatement", "ControlFlow", "Pri2" })]
        public static Expr If45(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            // Nullable BoolValue passed to If Test

            IsTrueFalseTest TestBool = new IsTrueFalseTest(1);
            ParameterExpression TestVar = Expr.Variable(typeof(Boolean), "TestVar");
            Expression ET = AstUtils.If(TestBool,
                                        new Expression[] { Expr.Assign(TestVar, Expr.Constant(true)) }

                                       );

            Expressions.Add(ET);
            Expressions.Add(EU.GenAreEqual(Expr.Constant(true), TestVar));
            var tree = Expr.Block(new ParameterExpression[] { TestVar }, Expressions);
            V.Validate(tree);
            return tree;

        }


        // IfCondition(Expression, Expression)
        // Scenario/Test : Pass a null value to each of the arguments
        // Expected      : Exception
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "If() 46", new string[] { "negative", "IfStatement", "ControlFlow", "Pri3" }, Exception = typeof(ArgumentException))]
        public static Expr If46(EU.IValidator V) {
            var Test2 =
            EU.Throws<System.ArgumentException>(() =>
            {
                AstUtils.IfCondition(Expr.Constant(null), Expr.Constant(null));
            });

            return Expr.Empty();
        }
    }
}
