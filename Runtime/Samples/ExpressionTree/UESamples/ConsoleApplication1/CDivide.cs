using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CDivide {
        //Divide(Expression, Expression)
        public static void Divide1() {
            //<Snippet1>
            // Add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //This expression Divides the values of its two arguments.
            //Both arguments need to be of the same type.
            Expression MyDivide = Expression.Divide(
                Expression.Constant(2),
                Expression.Constant(1)
            );

            //The end result should be two:
            Console.WriteLine(Expression.Lambda<Func<int>>(MyDivide).Compile().Invoke());
            //</Snippet1>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyDivide).Compile().Invoke() != 2) throw new Exception();
        }

        //Divide(Expression, Expression, MethodInfo)
        //<Snippet2>
        public static int Divide(int arg1, Exception arg2) {
            return arg1 / Convert.ToInt32(arg2.Message);
        }
        //</Snippet2>
        

        public static void Divide2() {
            //<Snippet2>
            // Add the following directive to your file
            // using Microsoft.Scripting.Ast;

            //This expression represents a division of two arguments using a user defined operator.
            //The parameters to the division should be reference convertible to the MethodInfo's arguments
            Expression MyDivide = Expression.Divide(
                Expression.Constant(6),
                Expression.Constant(new Exception("2")),
                ((Func<int, Exception, int>)Divide).Method
            );

            //The end result should be three:
            Console.WriteLine(Expression.Lambda<Func<int>>(MyDivide).Compile().Invoke());
            //</Snippet2>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyDivide).Compile().Invoke() != 3) throw new Exception();
        }

/*
        //DivideChecked(Expression, Expression)
        static public void DivideChecked1() {
            //<Snippet3>
            // Add the following directive to your file
            // using Microsoft.Scripting.Ast;

            //This expression divides the values of its two arguments.
            //Both arguments need to be of the same type.
            //If the result is larger than the type of the operation,
            //an OverflowException is thrown.
            Expression MyDivide = Expression.DivideChecked(
                Expression.Constant(int.MinValue),
                Expression.Constant(-1)
            );

            //An exception should happen:
            try {
                Expression.Lambda<Func<int>>(MyDivide).Compile().Invoke();
            } catch (OverflowException) {
                Console.WriteLine("Expected exception thrown");
            }
            //</Snippet3>

            //validate sample.
            try {
                Expression.Lambda<Func<int>>(MyDivide).Compile().Invoke();
                throw new Exception("Expected Overflow Exception, no exception thrown.");
            } catch (OverflowException) {
            }
        }

        //DivideChecked(Expression, Expression, MethodInfo)
        //<Snippet4>
        public static int DivideChecked(int arg1, Exception arg2) {
            Int64 res = (Int64)arg1 / Convert.ToInt64(arg2.Message);
            if (res < Int32.MinValue || res > Int32.MaxValue) throw new OverflowException();
            return (Int64)res;
        }
        //</Snippet4>


        public static void DivideChecked2() {
            //<Snippet4>
            // Add the following directive to your file
            // using Microsoft.Scripting.Ast;

            //This expression represents a division of two arguments using a user defined operator.
            //The parameters to the division should be reference convertible to the MethodInfo's arguments
            Expression MyDivide = Expression.DivideChecked(
                Expression.Constant(int.MinValue),
                Expression.Constant(new Exception("-1")),
                ((Func<int, Exception, int>)DivideChecked).Method
            );

            //An exception should happen:
            try {
                Expression.Lambda<Func<int>>(MyDivide).Compile().Invoke();
            } catch (OverflowException) {
                Console.WriteLine("Expected exception thrown");
            }
            //</Snippet4>

            //validate sample.
            try {
                Expression.Lambda<Func<int>>(MyDivide).Compile().Invoke();
                throw new Exception("Expected Overflow Exception, no exception thrown.");
            } catch (OverflowException) {
            }
        }
*/

        //DivideAssign(Expression, Expression)
        public static void DivideAssign1() {
            //<Snippet5>
            // Add the following directive to your file
            // using Microsoft.Scripting.Ast;

            //DivideAssign requires an assignable expression to be used as the left argument.
            ParameterExpression Variable = Expression.Variable(typeof(int), "Variable");

            //Here we initialize the variable with 6, then use it in an DivideAssign expression.
            //both the DivideAssign expression and the variable will have the value 2 after the
            //tree is executed.
            Expression MyDivideAssign = Expression.Block(
                new ParameterExpression[] { Variable },
                Expression.Assign(Variable, Expression.Constant(6)),
                Expression.DivideAssign(
                    Variable,
                    Expression.Constant(3)
                )
            );

            //The result should be two.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyDivideAssign).Compile().Invoke());
            //</Snippet5>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyDivideAssign).Compile().Invoke() != 2) throw new Exception();
        }

        //DivideAssign(Expression, Expression, MethodInfo)
        //<Snippet6
        public static int DivideAssign(int arg1, double arg2) {
            return arg1 / (int)arg2;
        }
        //</Snippet6>

        public static void DivideAssign2() {
            //<Snippet6>
            // Add the following directive to your file
            // using Microsoft.Scripting.Ast;

            //DivideAssign requires an assignable expression to be used as the left argument.
            ParameterExpression Variable = Expression.Variable(typeof(int), "Variable");

            //Here we initialize the variable with 6, then use it in an DivideAssign expression
            //with a user defined method.            
            Expression MyDivideAssign = Expression.Block(
                new ParameterExpression[] { Variable },
                Expression.Assign(Variable, Expression.Constant(6)),
                Expression.DivideAssign(
                    Variable,
                    Expression.Constant((double)3),
                    ((Func<int, double, int>)DivideAssign).Method
                )
            );

            //The result should be two.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyDivideAssign).Compile().Invoke());
            //</Snippet6>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyDivideAssign).Compile().Invoke() != 2) throw new Exception();
        }

        //DivideAssign(Expression, Expression, MethodInfo, LambdaExpression)
        //<Snippet7>
        public static double DivideAssignDouble(int arg1, double arg2) {
            return (double)arg1 / arg2;
        }
        //</Snippet7>

        public static void DivideAssign3() {
            //<Snippet7>
            // Add the following directive to your file
            // using Microsoft.Scripting.Ast;

            //DivideAssign requires an assignable expression to be used as the left argument.
            ParameterExpression Variable = Expression.Variable(typeof(int), "Variable");

            //This overload of DivideAssign also requires a conversion lambda. This is the 
            //Lambda's parameter
            ParameterExpression Param1 = Expression.Parameter(typeof(double), "Parameter 1");

            //Here we initialize the variable with 6, then use it in an DivideAssign expression
            //with a user defined method.
            //Since the return type of the method is double, we need to provide a conversion
            //to the variable's type.
            Expression MyDivideAssign = Expression.Block(
                new ParameterExpression[] { Variable },
                Expression.Assign(Variable, Expression.Constant(6)),
                Expression.DivideAssign(
                    Variable,
                    Expression.Constant((double)3),
                    ((Func<int, double, double>)DivideAssignDouble).Method,
                    Expression.Lambda<Func<double, int>>(
                        Expression.Convert(Param1, typeof(int)),
                        new ParameterExpression[] { Param1 }
                    )
                )
            );

            //The result should be two.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyDivideAssign).Compile().Invoke());
            //</Snippet7>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyDivideAssign).Compile().Invoke() != 2) throw new Exception();
        }




        /*

        //DivideAssignChecked(Expression, Expression)
        public static void DivideAssignChecked1() {
            //<Snippet8>
            // Add the following directive to your file
            // using Microsoft.Scripting.Ast;

            //DivideAssign requires an assignable expression to be used as the left argument.
            ParameterExpression Variable = Expression.Variable(typeof(int), "Variable");

            //Here we initialize the variable with 1, then use it in an DivideAssign expression.
            //both the DivideAssign expression and the variable will have the value 2 after the
            //tree is executed.
            Expression MyDivideAssignChecked = Expression.Block(
                new ParameterExpression[] { Variable },
                Expression.Assign(Variable, Expression.Constant(1)),
                Expression.DivideAssign(
                    Variable,
                    Expression.Constant(1)
                )
            );

            //The result should be two.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyDivideAssignChecked).Compile().Invoke());
            //</Snippet8>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyDivideAssignChecked).Compile().Invoke() != 2) throw new Exception();
        }

        //DivideAssignChecked(Expression, Expression, MethodInfo)
        //<Snippet9
        public static int DivideAssignChecked(int arg1, double arg2) {
            return arg1 + (int)arg2;
        }
        //</Snippet9>

        public static void DivideAssignChecked2() {
            //<Snippet9>
            // Add the following directive to your file
            // using Microsoft.Scripting.Ast;

            //DivideAssign requires an assignable expression to be used as the left argument.
            ParameterExpression Variable = Expression.Variable(typeof(int), "Variable");

            //Here we initialize the variable with 1, then use it in an DivideAssign expression
            //with a user defined method.            
            Expression MyDivideAssignChecked = Expression.Block(
                new ParameterExpression[] { Variable },
                Expression.Assign(Variable, Expression.Constant(1)),
                Expression.DivideAssignChecked(
                    Variable,
                    Expression.Constant((double)1),
                    ((Func<int, double, int>)DivideAssignChecked).Method
                )
            );

            //The result should be two.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyDivideAssignChecked).Compile().Invoke());
            //</Snippet9>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyDivideAssignChecked).Compile().Invoke() != 2) throw new Exception();
        }

        //DivideAssign(Expression, Expression, MethodInfo, LambdaExpression)
        //<Snippet10>
        public static double DivideAssignCheckedDouble(int arg1, double arg2) {
            return (double)arg1 + arg2;
        }
        //</Snippet10>

        public static void DivideAssignChecked3() {
            //<Snippet10>
            // Add the following directive to your file
            // using Microsoft.Scripting.Ast;

            //DivideAssign requires an assignable expression to be used as the left argument.
            ParameterExpression Variable = Expression.Variable(typeof(int), "Variable");

            //This overload of DivideAssign also requires a conversion lambda. This is the 
            //Lambda's parameter
            ParameterExpression Param1 = Expression.Parameter(typeof(double), "Parameter 1");

            //Here we initialize the variable with 1, then use it in an DivideAssign expression
            //with a user defined method.
            //Since the return type of the method is double, we need to provide a conversion
            //to the variable's type.
            Expression MyDivideAssignChecked = Expression.Block(
                new ParameterExpression[] { Variable },
                Expression.Assign(Variable, Expression.Constant(1)),
                Expression.DivideAssignChecked(
                    Variable,
                    Expression.Constant((double)1),
                    ((Func<int, double, double>)DivideAssignDouble).Method,
                    Expression.Lambda<Func<double, int>>(
                        Expression.Convert(Param1, typeof(int)),
                        new ParameterExpression[] { Param1 }
                    )
                )
            );

            //The result should be two.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyDivideAssignChecked).Compile().Invoke());
            //</Snippet10>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyDivideAssignChecked).Compile().Invoke() != 2) throw new Exception();
        }
        */
    }
}
