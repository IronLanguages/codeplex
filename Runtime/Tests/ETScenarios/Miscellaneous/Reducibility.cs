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

    public class Reducibility {
        // on an instrinsic non reducible node.
        [ETUtils.TestAttribute(ETUtils.TestState.Disabled, "Reduce 1", new string[] { "positive", "constant", "miscellaneous", "Pri1" })]
        public static Expr Reduce1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ConstantExpression x = Expr.Constant(5);
            Expressions.Add(EU.GenAreEqual(x, x.Reduce()));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        public class MyExpression : Expression {
            public override bool CanReduce {
                get {
                    return true;
                }
            }

            public override Expression Reduce() {
                return Expression.Constant(42);
            }

            public sealed override ExpressionType NodeType {
                get { return ExpressionType.Constant; }
            }

            public sealed override Type Type {
                get { return typeof(int); }
            }
        }

        // on an user defined expression, check reduce is called when can reduce is true
        [ETUtils.TestAttribute(ETUtils.TestState.Disabled, "Reduce 2", new string[] { "positive", "constant", "miscellaneous", "Pri1" })]
        public static Expr Reduce2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expressions.Add(EU.GenAreEqual(Expr.Constant(42), new MyExpression()));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }


        public class MyExpression1 : Expression {
            public override bool CanReduce {
                get {
                    return true;
                }
            }

            public override Expression Reduce() {
                throw new CustomAttributeFormatException();
            }

            public sealed override ExpressionType NodeType {
                get { return ExpressionType.Constant; }
            }

            public sealed override Type Type {
                get { return typeof(int); }
            }
        }


        // return true on isreducible, throw exception when reduce is called
        [ETUtils.TestAttribute(ETUtils.TestState.Disabled, "Reduce 3", new string[] { "negative", "constant", "miscellaneous", "Pri1" }, Exception = typeof(CustomAttributeFormatException))]
        public static Expr Reduce3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expression x = new MyExpression1();
            V.ValidateException<CustomAttributeFormatException>(x, false);
            return EU.BlockVoid(x);
        }

        public class MyExpression2 : Expression {
            public override bool CanReduce {
                get {
                    return true;
                }
            }

            public bool Reduced = false;

            public override Expression Reduce() {
                return null;
            }

            public sealed override ExpressionType NodeType {
                get { return ExpressionType.Constant; }
            }

            public sealed override Type Type {
                get { return typeof(int); }
            }
        }

        // return true on isreducible, throw exception when reduce is called
        [ETUtils.TestAttribute(ETUtils.TestState.Disabled, "Reduce 4", new string[] { "negative", "constant", "miscellaneous", "Pri1" }, Exception = typeof(NullReferenceException))]
        public static Expr Reduce4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expression x = new MyExpression2();
            //var field = Expr.Field(x, typeof(MyExpression).GetField("Reduced"));
            //Expressions.Add(EU.GenAreEqual(Expr.Constant(true), field));
            V.ValidateException<NullReferenceException>(x, false);
            return EU.BlockVoid(x);
        }
    }
}
