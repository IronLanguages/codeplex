using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CPower {
        //Power(Expression, Expression)
        public static void PowerSample1() {
            //<Snippet1>
            //This expression raises the value of first argument to the power of the value of the second argument.
            Expression MyPower = Expression.Power(
                Expression.Constant(2.0, typeof(double)),
                Expression.Constant(3.0, typeof(double))
            );

            //The end result should be eight:
            Console.WriteLine(Expression.Lambda<Func<double>>(MyPower).Compile().Invoke());
            //</Snippet1>

            //validate sample.
            if (Expression.Lambda<Func<double>>(MyPower).Compile().Invoke() != 8) throw new Exception();
        }

        //Power(Expression, Expression, MethodInfo)
        //<Snippet2>
        public static int Power(int arg1, Exception arg2) {
            return (int)Math.Pow(arg1,Convert.ToInt32(arg2.Message));
        }
        //</Snippet2>


        public static void PowerSample2() {
            //<Snippet2>
            //This expression represents a power operation using a user defined operator.
            //The parameters should be reference convertible to the MethodInfo's arguments
            Expression MyPower = Expression.Power(
                Expression.Constant(2),
                Expression.Constant(new Exception("3")),
                ((Func<int, Exception, int>)Power).Method
            );

            //The end result should be eight:
            Console.WriteLine(Expression.Lambda<Func<int>>(MyPower).Compile().Invoke());
            //</Snippet2>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyPower).Compile().Invoke() != 8) throw new Exception();
        }

        //PowerAssign(Expression, Expression)
        public static void PowerAssignSample1() {
            //<Snippet5>
            //PowerAssign requires an assignable expression to be used as the left argument.
            ParameterExpression Variable = Expression.Variable(typeof(double), "Variable");

            //Here we initialize the variable with 2, then use it in an PowerAssign expression.
            //both the PowerAssign expression and the variable will have the value 8 after the
            //tree is executed.
            Expression MyPowerAssign = Expression.Block(
                new ParameterExpression[] { Variable },
                Expression.Assign(Variable, Expression.Constant(2.0, typeof(double))),
                Expression.PowerAssign(
                    Variable,
                    Expression.Constant(3.0, typeof(double))
                )
            );

            //The result should be eight.
            Console.WriteLine(Expression.Lambda<Func<double>>(MyPowerAssign).Compile().Invoke());
            //</Snippet5>

            //validate sample.
            if (Expression.Lambda<Func<double>>(MyPowerAssign).Compile().Invoke() != 8) throw new Exception();
        }

        //PowerAssign(Expression, Expression, MethodInfo)
        //<Snippet6>
        public static int PowerAssign(int arg1, string arg2) {
            return (int)Math.Pow(arg1, Convert.ToInt32(arg2));
        }
        //</Snippet6>

        public static void PowerAssignSample2() {
            //<Snippet6>
            //PowerAssign requires an assignable expression to be used as the left argument.
            ParameterExpression Variable = Expression.Variable(typeof(int), "Variable");

            //Here we initialize the variable with 2, then use it in an PowerAssign expression
            //with a user defined method.            
            Expression MyPowerAssign = Expression.Block(
                new ParameterExpression[] { Variable },
                Expression.Assign(Variable, Expression.Constant(2)),
                Expression.PowerAssign(
                    Variable,
                    Expression.Constant("3"),
                    ((Func<int, string, int>)PowerAssign).Method
                )
            );

            //The result should be eight.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyPowerAssign).Compile().Invoke());
            //</Snippet6>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyPowerAssign).Compile().Invoke() != 8) throw new Exception();
        }

        //PowerAssign(Expression, Expression, MethodInfo, LambdaExpression)
        //<Snippet7>
        public static double PowerAssignDouble(int arg1, string arg2) {
            return Math.Pow(arg1, Convert.ToInt32(arg2));
        }
        //</Snippet7>

        public static void PowerAssignSample3() {
            //<Snippet7>
            //PowerAssign requires an assignable expression to be used as the left argument.
            ParameterExpression Variable = Expression.Variable(typeof(int), "Variable");

            //This overload of PowerAssign also requires a conversion lambda. This is the 
            //Lambda's parameter
            ParameterExpression Param1 = Expression.Parameter(typeof(double), "Parameter 1");

            //Here we initialize the variable with 2, then use it in an PowerAssign expression
            //with a user defined method.
            //Since the return type of the method is double, we need to provide a conversion
            //to the variable's type.
            Expression MyPowerAssign = Expression.Block(
                new ParameterExpression[] { Variable },
                Expression.Assign(Variable, Expression.Constant(2)),
                Expression.PowerAssign(
                    Variable,
                    Expression.Constant("3"),
                    ((Func<int, string, double>)PowerAssignDouble).Method,
                    Expression.Lambda<Func<double, int>>(
                        Expression.Convert(Param1, typeof(int)),
                        new ParameterExpression[] { Param1 }
                    )
                )
            );

            //The result should be 8.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyPowerAssign).Compile().Invoke());
            //</Snippet7>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyPowerAssign).Compile().Invoke() != 8) throw new Exception();
        }
    }
}
