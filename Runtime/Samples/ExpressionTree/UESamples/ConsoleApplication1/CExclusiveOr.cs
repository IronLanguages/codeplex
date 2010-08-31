using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    public class CExclusiveOr {
        //Expression.ExclusiveOr(Expression, Expression)
        static public void ExclusiveOr1() {
            //<Snippet1>
            // Add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //This expression represents an exclusive or of the values of its two arguments.
            //Both arguments need to be of the same type, either an integer type or boolean.
            Expression MyExclusiveOr = Expression.ExclusiveOr(
                Expression.Constant(5),
                Expression.Constant(3)
            );

            //The end result should be six (101 xor 011 = 110:
            Console.WriteLine(Expression.Lambda<Func<int>>(MyExclusiveOr).Compile().Invoke());
            //</Snippet1>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyExclusiveOr).Compile().Invoke() != 6) throw new Exception();
        }

        //Expression.ExclusiveOr(Expression, Expression, MethodInfo)
        //<Snippet2>
        public static int ExclusiveOr(int arg1, Exception arg2) {
            return arg1 ^ Convert.ToInt32(arg2.Message);
        }
        //</Snippet2>


        public static void ExclusiveOr2() {
            //<Snippet2>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;

            //This expression represents a boolean or bitwise exclusive or of two arguments using a user defined operator.
            //The parameters to the ExclusiveOr should be reference convertible to the MethodInfo's arguments
            Expression MyExclusiveOr = Expression.ExclusiveOr(
                Expression.Constant(5),
                Expression.Constant(new Exception("3")),
                ((Func<int, Exception, int>)ExclusiveOr).Method
            );

            //The end result should be six:
            Console.WriteLine(Expression.Lambda<Func<int>>(MyExclusiveOr).Compile().Invoke());
            //</Snippet2>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyExclusiveOr).Compile().Invoke() != 6) throw new Exception();
        }

  
        //Expression.ExclusiveOrAssign(Expression, Expression)
        public static void ExclusiveOrAssign1() {
            //<Snippet5>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;

            //ExclusiveOrAssign requires an assignable expression to be used as the left argument.
            ParameterExpression Variable = Expression.Variable(typeof(bool), "Variable");

            //Here we initialize the variable with true, then use it in an ExclusiveOrAssign expression.
            //both the ExclusiveOrAssign expression and the variable will have the value false after the
            //tree is executed.
            Expression MyExclusiveOrAssign = Expression.Block(
                new ParameterExpression[] { Variable },
                Expression.Assign(Variable, Expression.Constant(false)),
                Expression.ExclusiveOrAssign(
                    Variable,
                    Expression.Constant(false)
                )
            );

            //The result should be false.
            Console.WriteLine(Expression.Lambda<Func<bool>>(MyExclusiveOrAssign).Compile().Invoke());
            //</Snippet5>

            //validate sample.
            if (Expression.Lambda<Func<bool>>(MyExclusiveOrAssign).Compile().Invoke() != false) throw new Exception();
        }

        //Expression.ExclusiveOrAssign(Expression, Expression, MethodInfo)
        //<Snippet6
        public static bool ExclusiveOrAssign(bool arg1, bool arg2) {
            return arg1 ^ arg2;
        }
        //</Snippet6>

        public static void ExclusiveOrAssign2() {
            //<Snippet6>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;

            //ExclusiveOrAssign requires an assignable expression to be used as the left argument.
            ParameterExpression Variable = Expression.Variable(typeof(bool), "Variable");

            //Here we initialize the variable with true, then use it in an ExclusiveOrAssign expression
            //with a user defined method.
            Expression MyExclusiveOrAssign = Expression.Block(
                new ParameterExpression[] { Variable },
                Expression.Assign(Variable, Expression.Constant(false)),
                Expression.ExclusiveOrAssign(
                    Variable,
                    Expression.Constant(true),
                    ((Func<bool, bool, bool>)ExclusiveOrAssign).Method
                )
            );

            //The result value should be true.
            Console.WriteLine(Expression.Lambda<Func<bool>>(MyExclusiveOrAssign).Compile().Invoke());
            //</Snippet6>

            //validate sample.
            if (Expression.Lambda<Func<bool>>(MyExclusiveOrAssign).Compile().Invoke() != true) throw new Exception();
        }

        //Expression.ExclusiveOrAssign(Expression, Expression, MethodInfo, LambdaExpression)
        //<Snippet7>
        public static int ExclusiveOrAssignDouble(long arg1, long arg2) {
            return (int)(arg1 ^ arg2);
        }
        //</Snippet7>

        public static void ExclusiveOrAssign3() {
            //<Snippet7>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;

            //ExclusiveOrAssign requires an assignable expression to be used as the left argument.
            ParameterExpression Variable = Expression.Variable(typeof(long), "Variable");

            //This overload of ExclusiveOrAssign also requires a conversion lambda. This is the 
            //Lambda's parameter
            ParameterExpression Param1 = Expression.Parameter(typeof(int), "Parameter 1");

            //Here we initialize the double variable with 1, then use it in an ExclusiveOrAssign expression
            //with a user defined method.
            //Since the return type of the method is int, we need to provide a conversion
            //to the variable's type.
            Expression MyExclusiveOrAssign = Expression.Block(
                new ParameterExpression[] { Variable },
                Expression.Assign(Variable, Expression.Constant((long)1)),
                Expression.ExclusiveOrAssign(
                    Variable,
                    Expression.Constant((long)3),
                    ((Func<long, long, int>)ExclusiveOrAssignDouble).Method,
                    Expression.Lambda<Func<int, long>>(
                        Expression.Convert(Param1,typeof(long)),
                        new ParameterExpression[] { Param1 }
                    )
                )
            );

            //The result should be two.
            Console.WriteLine(Expression.Lambda<Func<long>>(MyExclusiveOrAssign).Compile().Invoke());
            //</Snippet7>

            //validate sample.
            if (Expression.Lambda<Func<long>>(MyExclusiveOrAssign).Compile().Invoke() != 2) throw new Exception();
        }


    }
}

