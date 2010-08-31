using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    public class CAnd {
        //Expression.And(Expression, Expression)
        static public void And1() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //This expression ands the values of its two arguments.
            //Both arguments need to be of the same type, either an 
            //integer type or boolean.
            Expression MyAnd = Expression.And(
                Expression.Constant(6),
                Expression.Constant(2)
            );

            //The end result should be two:
            Console.WriteLine(Expression.Lambda<Func<int>>(MyAnd).Compile().Invoke());
            //</Snippet1>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyAnd).Compile().Invoke() != 2) throw new Exception();
        }

        //Expression.And(Expression, Expression, MethodInfo)
        //<Snippet2>
        public static int And(int arg1, Exception arg2) {
            return arg1 & Convert.ToInt32(arg2.Message);
        }
        //</Snippet2>


        public static void And2() {
            //<Snippet2>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;

            //This expression represents a boolean or bitwise and of two arguments using a user defined operator.
            //The parameters to the and should be reference convertible to the MethodInfo's arguments
            Expression MyAnd = Expression.And(
                Expression.Constant(1),
                Expression.Constant(new Exception("2")),
                ((Func<int, Exception, int>)And).Method
            );

            //The end result should be zero:
            Console.WriteLine(Expression.Lambda<Func<int>>(MyAnd).Compile().Invoke());
            //</Snippet2>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyAnd).Compile().Invoke() != 0) throw new Exception();
        }

        //Expression.AndAlso(Expression, Expression)
        static public void AndAlso1() {
            //<Snippet3>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //This expression ands the values of its two arguments, 
            //but will not evaluate the second if the first is false.
            //Both arguments need to be of boolean type
            Expression MyAnd = Expression.AndAlso(
                Expression.Constant(false),
                Expression.Constant(true)
            );

            //The end result should be false:
            Console.WriteLine(Expression.Lambda<Func<bool>>(MyAnd).Compile().Invoke());
            //</Snippet3>

            //validate sample.
            if (Expression.Lambda<Func<bool>>(MyAnd).Compile().Invoke() != false) throw new Exception();
        }
        //Expression.AndAlso(Expression, Expression, MethodInfo)
        //<Snippet4>
        public class AndAlsoTest {
            public AndAlsoTest(bool Value) {
                this.Value = Value;
            }
            public bool Value;
            public static AndAlsoTest AndAlso(AndAlsoTest arg1, AndAlsoTest arg2) {
                return new AndAlsoTest(arg1.Value && arg2.Value);
            }
            public static bool operator true(AndAlsoTest arg1) {
                return arg1.Value;
            }
            public static bool operator false(AndAlsoTest arg1) {
                return !arg1.Value;
            }
        }
        
        //</Snippet4>
        
        public static void AndAlso2() {
            //<Snippet4>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;

            //This expression represents a boolean or bitwise and of two arguments using a user defined operator.
            //The parameters to the and should be reference convertible to the MethodInfo's arguments
            Expression MyAnd = Expression.AndAlso(
                Expression.Constant(new AndAlsoTest(true)),
                Expression.Constant(new AndAlsoTest(false)),
                ((Func<AndAlsoTest, AndAlsoTest, AndAlsoTest>)AndAlsoTest.AndAlso).Method
            );

            //The end result should be false:
            Console.WriteLine(Expression.Lambda<Func<bool>>(Expression.Field(MyAnd,"Value")).Compile().Invoke());
            //</Snippet4>

            //validate sample.
            if (Expression.Lambda<Func<bool>>(Expression.Field(MyAnd, "Value")).Compile().Invoke() != false) throw new Exception();
        }

        //Expression.AndAssign(Expression, Expression)
        public static void AndAssign1() {
            //<Snippet5>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;

            //AndAssign requires an assignable expression to be used as the left argument.
            ParameterExpression Variable = Expression.Variable(typeof(bool), "Variable");

            //Here we initialize the variable with true, then use it in an AndAssign expression.
            //both the AndAssign expression and the variable will have the value false after the
            //tree is executed.
            Expression MyAndAssign = Expression.Block(
                new ParameterExpression[] { Variable },
                Expression.Assign(Variable, Expression.Constant(false)),
                Expression.AndAssign(
                    Variable,
                    Expression.Constant(false)
                )
            );

            //The result should be false.
            Console.WriteLine(Expression.Lambda<Func<bool>>(MyAndAssign).Compile().Invoke());
            //</Snippet5>

            //validate sample.
            if (Expression.Lambda<Func<bool>>(MyAndAssign).Compile().Invoke() != false) throw new Exception();
        }

        //Expression.AndAssign(Expression, Expression, MethodInfo)
        //<Snippet6
        public static bool AndAssign(bool arg1, bool arg2) {
            return arg1 && arg2;
        }
        //</Snippet6>

        public static void AndAssign2() {
            //<Snippet6>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;

            //AndAssign requires an assignable expression to be used as the left argument.
            ParameterExpression Variable = Expression.Variable(typeof(bool), "Variable");

            //Here we initialize the variable with true, then use it in an AndAssign expression
            //with a user defined method.
            Expression MyAndAssign = Expression.Block(
                new ParameterExpression[] { Variable },
                Expression.Assign(Variable, Expression.Constant(true)),
                Expression.AndAssign(
                    Variable,
                    Expression.Constant(true),
                    ((Func<bool, bool, bool>)AndAssign).Method
                )
            );

            //The result value should be true.
            Console.WriteLine(Expression.Lambda<Func<bool>>(MyAndAssign).Compile().Invoke());
            //</Snippet6>

            //validate sample.
            if (Expression.Lambda<Func<bool>>(MyAndAssign).Compile().Invoke() != true) throw new Exception();
        }

        //Expression.AndAssign(Expression, Expression, MethodInfo, LambdaExpression)
        //<Snippet7>
        public static int AndAssignDouble(double arg1, double arg2) {
            return (int)(arg1 + arg2);
        }
        //</Snippet7>

        public static void AndAssign3() {
            //<Snippet7>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;

            //AndAssign requires an assignable expression to be used as the left argument.
            ParameterExpression Variable = Expression.Variable(typeof(double), "Variable");

            //This overload of AndAssign also requires a conversion lambda. This is the 
            //Lambda's parameter
            ParameterExpression Param1 = Expression.Parameter(typeof(int), "Parameter 1");

            //Here we initialize the double variable with 1, then use it in an AndAssign expression
            //with a user defined method.
            //Since the return type of the method is int, we need to provide a conversion
            //to the variable's type.
            Expression MyAndAssign = Expression.Block(
                new ParameterExpression[] { Variable },
                Expression.Assign(Variable, Expression.Constant((double)1)),
                Expression.AndAssign(
                    Variable,
                    Expression.Constant((double)1),
                    ((Func<double, double, int>)AndAssignDouble).Method,
                    Expression.Lambda<Func<int, double>>(
                        Expression.Convert(Param1,typeof(double)),
                        new ParameterExpression[] { Param1 }
                    )
                )
            );

            //The result should be two.
            Console.WriteLine(Expression.Lambda<Func<double>>(MyAndAssign).Compile().Invoke());
            //</Snippet7>

            //validate sample.
            if (Expression.Lambda<Func<double>>(MyAndAssign).Compile().Invoke() != 2) throw new Exception();
        }


    }
}

