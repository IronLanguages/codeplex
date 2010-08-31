using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CModulo {
        //Modulo(Expression, Expression)
        public static void ModuloSample1() {
            //<Snippet1>
            //This expression modulos the values of its two arguments.
            //Both arguments need to be of the same type.
            Expression MyModulo = Expression.Modulo(
                Expression.Constant(5),
                Expression.Constant(2)
            );

            //The end result should be 1:
            Console.WriteLine(Expression.Lambda<Func<int>>(MyModulo).Compile().Invoke());
            //</Snippet1>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyModulo).Compile().Invoke() != 1) throw new Exception();
        }

        //Modulo(Expression, Expression, MethodInfo)
        //<Snippet2>
        public static int Modulo(int arg1, Exception arg2) {
            return arg1 % Convert.ToInt32(arg2.Message);
        }
        //</Snippet2>


        public static void ModuloSample2() {
            //<Snippet2>
            //This expression modulos the two arguments using a user defined operator.
            //The parameters to the expression should be reference convertible to the MethodInfo's arguments
            Expression MyModulo = Expression.Modulo(
                Expression.Constant(5),
                Expression.Constant(new Exception("2")),
                ((Func<int, Exception, int>)Modulo).Method
            );

            //The end result should be 1:
            Console.WriteLine(Expression.Lambda<Func<int>>(MyModulo).Compile().Invoke());
            //</Snippet2>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyModulo).Compile().Invoke() != 1) throw new Exception();
        }

        //ModuloAssign(Expression, Expression)
        public static void ModuloAssignSample1() {
            //<Snippet5>
            //ModuloAssign requires an assignable expression to be used as the left argument.
            ParameterExpression Variable = Expression.Variable(typeof(int), "Variable");

            //Here we initialize the variable with 5, then use it in an ModuloAssign expression.
            //both the ModuloAssign expression and the variable will have the value 1 after the
            //tree is executed.
            Expression MyModuloAssign = Expression.Block(
                new ParameterExpression[] { Variable },
                Expression.Assign(Variable, Expression.Constant(5)),
                Expression.ModuloAssign(
                    Variable,
                    Expression.Constant(2)
                )
            );

            //The result should be 1.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyModuloAssign).Compile().Invoke());
            //</Snippet5>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyModuloAssign).Compile().Invoke() != 1) throw new Exception();
        }

        //ModuloAssign(Expression, Expression, MethodInfo)
        //<Snippet6
        public static int ModuloAssign(int arg1, double arg2) {
            return arg1 % (int)arg2;
        }
        //</Snippet6>

        public static void ModuloAssignSample2() {
            //<Snippet6>
            //ModuloAssign requires an assignable expression to be used as the left argument.
            ParameterExpression Variable = Expression.Variable(typeof(int), "Variable");

            //Here we initialize the variable with 5, then use it in an ModuloAssign expression
            //with a user defined method.            
            Expression MyModuloAssign = Expression.Block(
                new ParameterExpression[] { Variable },
                Expression.Assign(Variable, Expression.Constant(5)),
                Expression.ModuloAssign(
                    Variable,
                    Expression.Constant((double)2),
                    ((Func<int, double, int>)ModuloAssign).Method
                )
            );

            //The result should be 1.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyModuloAssign).Compile().Invoke());
            //</Snippet6>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyModuloAssign).Compile().Invoke() != 1) throw new Exception();
        }

        //ModuloAssign(Expression, Expression, MethodInfo, LambdaExpression)
        //<Snippet7>
        public static double ModuloAssignDouble(int arg1, double arg2) {
            return (double)arg1 % arg2;
        }
        //</Snippet7>

        public static void ModuloAssignSample3() {
            //<Snippet7>
            //ModuloAssign requires an assignable expression to be used as the left argument.
            ParameterExpression Variable = Expression.Variable(typeof(int), "Variable");

            //This overload of ModuloAssign also requires a conversion lambda. This is the 
            //Lambda's parameter
            ParameterExpression Param1 = Expression.Parameter(typeof(double), "Parameter 1");

            //Here we initialize the variable with 5, then use it in an ModuloAssign expression
            //with a user defined method.
            //Since the return type of the method is double, we need to provide a conversion
            //to the variable's type.
            Expression MyModuloAssign = Expression.Block(
                new ParameterExpression[] { Variable },
                Expression.Assign(Variable, Expression.Constant(5)),
                Expression.ModuloAssign(
                    Variable,
                    Expression.Constant((double)2.0),
                    ((Func<int, double, double>)ModuloAssignDouble).Method,
                    Expression.Lambda<Func<double, int>>(
                        Expression.Convert(Param1, typeof(int)),
                        new ParameterExpression[] { Param1 }
                    )
                )
            );

            //The result should be 1.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyModuloAssign).Compile().Invoke());
            //</Snippet7>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyModuloAssign).Compile().Invoke() != 1) throw new Exception();
        }
    }
}
