#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.Reflection;

namespace ETScenarios.Miscellaneous {
    using EU = ETUtils.ExpressionUtils;
    using Expr = Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public class Block {
        // Pass no elements to the block
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Block 1", new string[] { "positive", "block", "miscellaneous", "Pri1" })]
        public static Expr Block1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.BlockVoid());

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass null to expressions
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Block 2", new string[] { "negative", "block", "miscellaneous", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr Block2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.ArgumentNullException>(() =>
            {
                Expr.Block((Expr[])null);
            }));

            return EU.BlockVoid(Expressions);
        }

        // Pass an array with a null element to expressions
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Block 3", new string[] { "negative", "block", "miscellaneous", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr Block3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.ArgumentNullException>(() =>
            {
                Expr.Block(new Expr[] { Expr.Empty(), null, Expr.Empty() });
            }));

            return EU.BlockVoid(Expressions);
        }

        // Pass null to expressions
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Block 4", new string[] { "negative", "block", "miscellaneous", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr Block4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.ArgumentNullException>(() =>
            {
                Expr.Block((IEnumerable<Expr>)null);
            }));

            return EU.BlockVoid(Expressions);
        }

        // Pass an empty ienumerable
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Block 5", new string[] { "negative", "block", "miscellaneous", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Block5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.Block((IEnumerable<Expr>)new Expr[] { });
            }));

            return Expr.Empty();
        }

        // Pass an ienumerable with a single element
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Block 7", new string[] { "positive", "block", "miscellaneous", "Pri2" })]
        public static Expr Block7(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(Expr.Block((IEnumerable<Expr>)new[] { Expr.Empty() }));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass an ienumerable with a null element
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Block 8", new string[] { "negative", "block", "miscellaneous", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr Block8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.Throws<System.ArgumentNullException>(() =>
            {
                Expr.Block((IEnumerable<Expr>)new[] { Expr.Empty(), null, Expr.Empty() });
            }));

            return EU.BlockVoid(Expressions);
        }

        // Multiple nested commas/blocks
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Block 9", new string[] { "positive", "block", "miscellaneous", "Pri2" })]
        public static Expr Block9(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression var = Expr.Parameter(typeof(int), "");
            Expr Nested = Expr.Assign(var, Expr.Constant(45));
            for (int i = 0; i < 31; i++) {
                Nested = Expr.Block(Nested);
            }

            Expressions.Add(Nested);

            Expressions.Add(EU.GenAreEqual(var, Expr.Constant(45)));

            var tree = Expr.Block(new[] { var }, Expressions);
            V.Validate(tree);
            return tree;
        }


        public static void RefMethod(ref int arg) {
            arg = arg + 1;
        }

        public static MethodInfo Refmi = typeof(Block).GetMethod("RefMethod");

        // Pass a comma to a ref argument
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Block 10", new string[] { "positive", "block", "miscellaneous", "Pri2" })]
        public static Expr Block10(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression var = Expr.Parameter(typeof(int), "");
            Expr Cm = Expr.Block(var);
            Expr Call = Expr.Call(null, Refmi, Cm);

            Expressions.Add(Call);

            Expressions.Add(EU.GenAreEqual(Expr.Constant(0), var));

            var tree = Expr.Block(new[] { var }, Expressions);
            V.Validate(tree);
            return tree;
        }

        public static int ThreeArgsMethod(int arg1, int arg2, int arg3) {
            return arg1 + arg2 + arg3;
        }

        public static MethodInfo Threemi = typeof(Block).GetMethod("ThreeArgsMethod");

        // Method with three arguments, pass variable, comma that modifies variable, variable again. check if variable is 
        //modified by third argument call.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Block 11", new string[] { "positive", "block", "miscellaneous", "Pri2" })]
        public static Expr Block11(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression var = Expr.Parameter(typeof(int), "");

            Expressions.Add(Expr.Assign(var, Expr.Constant(1)));


            Expr Call = Expr.Call(null, Threemi, var, Expr.Block(Expr.Assign(var, Expr.Constant(2))), var);

            Expressions.Add(EU.GenAreEqual(Expr.Constant(5), Call));

            var tree = Expr.Block(new[] { var }, Expressions);
            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Block 12", new string[] { "positive", "block", "miscellaneous", "Pri1" })]
        public static Expr Block12(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();


            var exp1 = Expr.Constant(1);
            var exp2 = Expr.Condition(Expr.Constant(true), Expr.Constant(1), Expr.Constant(2));
            var exp3 = Expression.Constant(2);

            BlockExpression bl = Expr.Block(
                exp1,
                exp2,
                exp3
            );

            Expressions.Add(EU.GenAreEqual(Expr.Constant(true), Expr.Constant(exp3 == bl.Result), "result 1"));

            BlockExpression bl2 = Expr.Block(
                exp2
            );

            Expressions.Add(EU.GenAreEqual(Expr.Constant(true), Expr.Constant(exp2 == bl2.Result), "result 2"));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }




        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Block 13", new string[] { "negative", "condition", "controlflow", "Pri1" }, Exception = typeof(ArgumentNullException))]
        public static Expr Block13(EU.IValidator V) {
            return EU.Throws<ArgumentException>(() =>
            {
                Expr.Block(
                    (Type)null,
                    Expr.Constant(true)
                    );
            });
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Block 14", new string[] { "negative", "condition", "controlflow", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Block14(EU.IValidator V) {
            Expr tree =
                EU.Throws<System.ArgumentException>(() =>
                {
                    Expr.Block(
                        typeof(long),
                        Expr.Constant(2)
                );
                });

            return Expr.Empty();
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Block 15", new string[] { "positive", "condition", "controlflow", "Pri1" })]
        public static Expr Block15(EU.IValidator V) {
            var tree = Expr.Block(
                EU.GenAreEqual(
                    Expr.Constant(2),
                    Expr.Block(
                        typeof(int),
                        Expr.Constant(2)
                    ),
                    "Condition 1"
                )
            );

            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Block 16", new string[] { "positive", "condition", "controlflow", "Pri1" })]
        public static Expr Block16(EU.IValidator V) {
            var tree = Expr.Block(
                EU.GenAreEqual(
                    Expr.Constant(typeof(ArgumentException), typeof(Type)),
                    Expr.Call(
                        Expr.Block(
                            typeof(Exception),
                            Expr.Constant(new ArgumentException())
                        ),
                        typeof(object).GetMethod("GetType")
                    ),
                    "Condition 1"
                )
            );

            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Block 17", new string[] { "positive", "condition", "controlflow", "Pri1" })]
        public static Expr Block17(EU.IValidator V) {
            var tree = Expr.Block(
                EU.GenAreEqual(
                    Expr.Constant(typeof(void), typeof(Type)),
                    Expr.Constant(
                        Expr.Block(
                            typeof(void),
                            Expr.Constant(1)
                        ).Type,
                        typeof(Type)
                    ),
                    "Condition 1"
                )
            );

            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Block 18", new string[] { "negative", "condition", "controlflow", "Pri1" }, Exception = typeof(ArgumentNullException))]
        public static Expr Block18(EU.IValidator V) {
            return EU.Throws<ArgumentException>(() =>
            {
                Expr.Block(
                    (Type)null,
                    (IEnumerable<Expression>)new Expression[] { Expr.Constant(true) }
                    );
            });
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Block 19", new string[] { "negative", "condition", "controlflow", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Block19(EU.IValidator V) {
            Expr tree =
                EU.Throws<System.ArgumentException>(() =>
                {
                    Expr.Block(
                        typeof(long),
                        (IEnumerable<Expression>)new Expression[] { Expr.Constant(2) }
                );
                });

            return Expr.Empty();
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Block 20", new string[] { "positive", "condition", "controlflow", "Pri1" })]
        public static Expr Block20(EU.IValidator V) {
            var tree = Expr.Block(
                EU.GenAreEqual(
                    Expr.Constant(2),
                    Expr.Block(
                        typeof(int),
                        Expr.Constant(2)
                    ),
                    "Condition 1"
                )
            );

            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Block 21", new string[] { "positive", "condition", "controlflow", "Pri1" })]
        public static Expr Block21(EU.IValidator V) {
            var tree = Expr.Block(
                EU.GenAreEqual(
                    Expr.Constant(typeof(ArgumentException), typeof(Type)),
                    Expr.Call(
                        Expr.Block(
                            typeof(Exception),
                            (IEnumerable<Expression>)new Expression[] { Expr.Constant(new ArgumentException()) }
                        ),
                        typeof(object).GetMethod("GetType")
                    ),
                    "Condition 1"
                )
            );
            
            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Block 22", new string[] { "positive", "condition", "controlflow", "Pri1" })]
        public static Expr Block22(EU.IValidator V) {
            var tree = Expr.Block(
                EU.GenAreEqual(
                    Expr.Constant(typeof(void), typeof(Type)),
                    Expr.Constant(
                        Expr.Block(
                            typeof(void),
                            (IEnumerable<Expression>)new Expression[] { Expr.Constant(1) }
                        ).Type,
                        typeof(Type)
                    ),
                    "Condition 1"
                )
            );
            
            V.Validate(tree);
            return tree;
        }





        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Block 23", new string[] { "negative", "condition", "controlflow", "Pri1" }, Exception = typeof(ArgumentNullException))]
        public static Expr Block23(EU.IValidator V) {
            return EU.Throws<ArgumentException>(() =>
            {
                Expr.Block(
                    (Type)null,
                    new ParameterExpression[] { },
                    Expr.Constant(true)
                    );
            });
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Block 24", new string[] { "negative", "condition", "controlflow", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Block24(EU.IValidator V) {
            Expr tree =
                EU.Throws<System.ArgumentException>(() =>
                {
                    Expr.Block(
                        typeof(long),
                        new ParameterExpression[] { },
                        Expr.Constant(2)
                );
                });

            return Expr.Empty();
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Block 25", new string[] { "positive", "condition", "controlflow", "Pri1" })]
        public static Expr Block25(EU.IValidator V) {
            var tree = Expr.Block(
                EU.GenAreEqual(
                    Expr.Constant(2),
                    Expr.Block(
                        typeof(int),
                        new ParameterExpression[] { },
                        Expr.Constant(2)
                    ),
                    "Condition 1"
                )
            );
            
            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Block 26", new string[] { "positive", "condition", "controlflow", "Pri1" })]
        public static Expr Block26(EU.IValidator V) {
            var tree = Expr.Block(
                EU.GenAreEqual(
                    Expr.Constant(typeof(ArgumentException), typeof(Type)),
                    Expr.Call(
                        Expr.Block(
                            typeof(Exception),
                            new ParameterExpression[] { },
                            Expr.Constant(new ArgumentException())
                        ),
                        typeof(object).GetMethod("GetType")
                    ),
                    "Condition 1"
                )
            );
            
            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Block 27", new string[] { "positive", "condition", "controlflow", "Pri1" })]
        public static Expr Block27(EU.IValidator V) {
            var tree = Expr.Block(
                EU.GenAreEqual(
                    Expr.Constant(typeof(void), typeof(Type)),
                    Expr.Constant(
                        Expr.Block(
                            typeof(void),
                            new ParameterExpression[] { },
                            Expr.Constant(1)
                        ).Type,
                        typeof(Type)
                    ),
                    "Condition 1"
                )
            );
            
            V.Validate(tree);
            return tree;
        }



        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Block 28", new string[] { "negative", "condition", "controlflow", "Pri1" }, Exception = typeof(ArgumentNullException))]
        public static Expr Block28(EU.IValidator V) {
            return EU.Throws<ArgumentException>(() =>
            {
                Expr.Block(
                    (Type)null,
                    new ParameterExpression[] { },
                    (IEnumerable<Expression>)new Expression[] { Expr.Constant(true) }
                    );
            });
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Block 29", new string[] { "negative", "condition", "controlflow", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Block29(EU.IValidator V) {
            Expr tree = EU.Throws<ArgumentException>(() =>
            {
                Expr.Block(
                    typeof(long),
                    new ParameterExpression[] { },
                    (IEnumerable<Expression>)new Expression[] { Expr.Constant(2) }
                    );
            });

            return Expr.Empty();
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Block 30", new string[] { "positive", "condition", "controlflow", "Pri1" })]
        public static Expr Block30(EU.IValidator V) {
            var tree = Expr.Block(
                EU.GenAreEqual(
                    Expr.Constant(2),
                    Expr.Block(
                        typeof(int),
                        new ParameterExpression[] { },
                        Expr.Constant(2)
                    ),
                    "Condition 1"
                )
            );
            
            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Block 31", new string[] { "positive", "condition", "controlflow", "Pri1" })]
        public static Expr Block31(EU.IValidator V) {
            var tree = Expr.Block(
                EU.GenAreEqual(
                    Expr.Constant(typeof(ArgumentException), typeof(Type)),
                    Expr.Call(
                        Expr.Block(
                            typeof(Exception),
                            new ParameterExpression[] { },
                            (IEnumerable<Expression>)new Expression[] { Expr.Constant(new ArgumentException()) }
                        ),
                        typeof(object).GetMethod("GetType")
                    ),
                    "Condition 1"
                )
            );
            
            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Block 32", new string[] { "positive", "condition", "controlflow", "Pri1" })]
        public static Expr Block32(EU.IValidator V) {
            var tree = Expr.Block(
                EU.GenAreEqual(
                    Expr.Constant(typeof(void), typeof(Type)),
                    Expr.Constant(
                        Expr.Block(
                            typeof(void),
                            new ParameterExpression[] { },
                            (IEnumerable<Expression>)new Expression[] { Expr.Constant(1) }
                        ).Type,
                        typeof(Type)
                    ),
                    "Condition 1"
                )
            );
            
            V.Validate(tree);
            return tree;
        }

    }
}
