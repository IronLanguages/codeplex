using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CSubtract {
        //Subtract(Expression, Expression)
        static public void SubtractSample1() {
            //<Snippet1>
            //This expression subtracts the values of its two arguments.
            //Both arguments need to be of the same type.
            Expression MySubtract = Expression.Subtract(
                Expression.Constant(2),
                Expression.Constant(1)
            );

            //The end result should be one:
            Console.WriteLine(Expression.Lambda<Func<int>>(MySubtract).Compile().Invoke());
            //</Snippet1>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MySubtract).Compile().Invoke() != 1) throw new Exception();
        }

        //Subtract(Expression, Expression, MethodInfo)
        //<Snippet2>
        public static int Subtract(int arg1, Exception arg2) {
            return arg1 - Convert.ToInt32(arg2.Message);
        }
        //</Snippet2>

        public static void SubtractSample2() {
            //<Snippet2>
            //This expression represents an subtractition of two arguments using a user defined operator.
            //The parameters should be reference convertible to the MethodInfo's arguments
            Expression MySubtract = Expression.Subtract(
                Expression.Constant(2),
                Expression.Constant(new Exception("1")),
                ((Func<int, Exception, int>)Subtract).Method
            );

            //The end result should be one:
            Console.WriteLine(Expression.Lambda<Func<int>>(MySubtract).Compile().Invoke());
            //</Snippet2>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MySubtract).Compile().Invoke() != 1) throw new Exception();
        }

        //SubtractChecked(Expression, Expression)
        static public void SubtractCheckedSample1() {
            //<Snippet3>
            //This expression subtracts the values of its two arguments.
            //Both arguments need to be of the same type.
            //If the result is smaller than the type of the operation,
            //An OverflowException is thrown.
            Expression MySubtract = Expression.SubtractChecked(
                Expression.Constant(int.MinValue),
                Expression.Constant(1)
            );

            //An exception should happen:
            try {
                Expression.Lambda<Func<int>>(MySubtract).Compile().Invoke();
            } catch (OverflowException) {
                Console.WriteLine("Expected exception thrown");
            }
            //</Snippet3>

            //validate sample.
            try {
                Expression.Lambda<Func<int>>(MySubtract).Compile().Invoke();
                throw new Exception("Expected Overflow Exception, no exception thrown.");
            } catch (OverflowException) {
            }
        }

        //SubtractChecked(Expression, Expression, MethodInfo)
        //<Snippet4>
        public static int SubtractChecked(int arg1, Exception arg2) {
            int res = arg1 - Convert.ToInt32(arg2.Message);
            if (res > arg1 || res > Convert.ToInt32(arg2.Message)) throw new OverflowException();
            return res;
        }
        //</Snippet4>


        public static void SubtractCheckedSample2() {
            //<Snippet4>
            //This expression represents an subtraction of two arguments using a user defined operator.
            //The parameters to the subtractition should be reference convertible to the MethodInfo's arguments
            Expression MySubtract = Expression.SubtractChecked(
                Expression.Constant(int.MinValue),
                Expression.Constant(new Exception("2")),
                ((Func<int, Exception, int>)SubtractChecked).Method
            );

            //An exception should happen:
            try {
                Expression.Lambda<Func<int>>(MySubtract).Compile().Invoke();
            } catch (OverflowException) {
                Console.WriteLine("Expected exception thrown");
            }
            //</Snippet4>

            //validate sample.
            try {
                Expression.Lambda<Func<int>>(MySubtract).Compile().Invoke();
                throw new Exception("Expected Overflow Exception, no exception thrown.");
            } catch (OverflowException) {
            }
        }

        //SubtractAssign(Expression, Expression)
        public static void SubtractAssignSample1() {
            //<Snippet5>
            //SubtractAssign requires an assignable expression to be used as the left argument.
            ParameterExpression Variable = Expression.Variable(typeof(int), "Variable");

            //Here we initialize the variable with 1, then use it in an SubtractAssign expression.
            //both the SubtractAssign expression and the variable will have the value 2 after the
            //tree is executed.
            Expression MySubtractAssign = Expression.Block(
                new ParameterExpression[] { Variable },
                Expression.Assign(Variable, Expression.Constant(1)),
                Expression.SubtractAssign(
                    Variable,
                    Expression.Constant(1)
                )
            );

            //The result should be zero.
            Console.WriteLine(Expression.Lambda<Func<int>>(MySubtractAssign).Compile().Invoke());
            //</Snippet5>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MySubtractAssign).Compile().Invoke() != 0) throw new Exception();
        }

        //SubtractAssign(Expression, Expression, MethodInfo)
        //<Snippet6
        public static int SubtractAssign(int arg1, double arg2) {
            return arg1 - (int)arg2;
        }
        //</Snippet6>

        public static void SubtractAssignSample2() {
            //<Snippet6>
            //SubtractAssign requires an assignable expression to be used as the left argument.
            ParameterExpression Variable = Expression.Variable(typeof(int), "Variable");

            //Here we initialize the variable with 1, then use it in an SubtractAssign expression
            //with a user defined method.            
            Expression MySubtractAssign = Expression.Block(
                new ParameterExpression[] { Variable },
                Expression.Assign(Variable, Expression.Constant(1)),
                Expression.SubtractAssign(
                    Variable,
                    Expression.Constant((double)1),
                    ((Func<int, double, int>)SubtractAssign).Method
                )
            );

            //The result should be zero.
            Console.WriteLine(Expression.Lambda<Func<int>>(MySubtractAssign).Compile().Invoke());
            //</Snippet6>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MySubtractAssign).Compile().Invoke() != 0) throw new Exception();
        }

        //SubtractAssign(Expression, Expression, MethodInfo, LambdaExpression)
        //<Snippet7>
        public static double SubtractAssignDouble(int arg1, double arg2) {
            return (double)arg1 - arg2;
        }
        //</Snippet7>

        public static void SubtractAssignSample3() {
            //<Snippet7>
            //SubtractAssign requires an assignable expression to be used as the left argument.
            ParameterExpression Variable = Expression.Variable(typeof(int), "Variable");

            //This overload of SubtractAssign also requires a conversion lambda. This is the 
            //Lambda's parameter
            ParameterExpression Param1 = Expression.Parameter(typeof(double), "Parameter 1");

            //Here we initialize the variable with 1, then use it in an SubtractAssign expression
            //with a user defined method.
            //Since the return type of the method is double, we need to provide a conversion
            //to the variable's type.
            Expression MySubtractAssign = Expression.Block(
                new ParameterExpression[] { Variable },
                Expression.Assign(Variable, Expression.Constant(1)),
                Expression.SubtractAssign(
                    Variable,
                    Expression.Constant((double)1),
                    ((Func<int, double, double>)SubtractAssignDouble).Method,
                    Expression.Lambda<Func<double, int>>(
                        Expression.Convert(Param1, typeof(int)),
                        new ParameterExpression[] { Param1 }
                    )
                )
            );

            //The result should be zero.
            Console.WriteLine(Expression.Lambda<Func<int>>(MySubtractAssign).Compile().Invoke());
            //</Snippet7>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MySubtractAssign).Compile().Invoke() != 0) throw new Exception();
        }

        //SubtractAssignChecked(Expression, Expression)
        public static void SubtractAssignCheckedSample1() {
            //<Snippet8>
           //SubtractAssign requires an assignable expression to be used as the left argument.
            ParameterExpression Variable = Expression.Variable(typeof(int), "Variable");

            //Here we initialize the variable with 1, then use it in an SubtractAssign expression.
            //both the SubtractAssign expression and the variable will have the value 0 after the
            //tree is executed.
            Expression MySubtractAssignChecked = Expression.Block(
                new ParameterExpression[] { Variable },
                Expression.Assign(Variable, Expression.Constant(1)),
                Expression.SubtractAssign(
                    Variable,
                    Expression.Constant(1)
                )
            );

            //The result should be two.
            Console.WriteLine(Expression.Lambda<Func<int>>(MySubtractAssignChecked).Compile().Invoke());
            //</Snippet8>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MySubtractAssignChecked).Compile().Invoke() != 0) throw new Exception();
        }

        //SubtractAssignChecked(Expression, Expression, MethodInfo)
        //<Snippet9>
        public static int SubtractAssignChecked(int arg1, double arg2) {
            return arg1 - (int)arg2;
        }
        //</Snippet9>

        public static void SubtractAssignChecked2() {
            //<Snippet9>
            //SubtractAssign requires an assignable expression to be used as the left argument.
            ParameterExpression Variable = Expression.Variable(typeof(int), "Variable");

            //Here we initialize the variable with 1, then use it in an SubtractAssign expression
            //with a user defined method.            
            Expression MySubtractAssignChecked = Expression.Block(
                new ParameterExpression[] { Variable },
                Expression.Assign(Variable, Expression.Constant(1)),
                Expression.SubtractAssignChecked(
                    Variable,
                    Expression.Constant((double)1),
                    ((Func<int, double, int>)SubtractAssignChecked).Method
                )
            );

            //The result should be zero.
            Console.WriteLine(Expression.Lambda<Func<int>>(MySubtractAssignChecked).Compile().Invoke());
            //</Snippet9>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MySubtractAssignChecked).Compile().Invoke() != 0) throw new Exception();
        }

        //SubtractAssign(Expression, Expression, MethodInfo, LambdaExpression)
        //<Snippet10>
        public static double SubtractAssignCheckedDouble(int arg1, double arg2) {
            return (double)arg1 + arg2;
        }
        //</Snippet10>

        public static void SubtractAssignCheckedSample3() {
            //<Snippet10>
            //SubtractAssign requires an assignable expression to be used as the left argument.
            ParameterExpression Variable = Expression.Variable(typeof(int), "Variable");

            //This overload of SubtractAssign also requires a conversion lambda. This is the 
            //Lambda's parameter
            ParameterExpression Param1 = Expression.Parameter(typeof(double), "Parameter 1");

            //Here we initialize the variable with 1, then use it in an SubtractAssign expression
            //with a user defined method.
            //Since the return type of the method is double, we need to provide a conversion
            //to the variable's type.
            Expression MySubtractAssignChecked = Expression.Block(
                new ParameterExpression[] { Variable },
                Expression.Assign(Variable, Expression.Constant(1)),
                Expression.SubtractAssignChecked(
                    Variable,
                    Expression.Constant((double)1),
                    ((Func<int, double, double>)SubtractAssignDouble).Method,
                    Expression.Lambda<Func<double, int>>(
                        Expression.Convert(Param1, typeof(int)),
                        new ParameterExpression[] { Param1 }
                    )
                )
            );

            //The result should be zero.
            Console.WriteLine(Expression.Lambda<Func<int>>(MySubtractAssignChecked).Compile().Invoke());
            //</Snippet10>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MySubtractAssignChecked).Compile().Invoke() != 0) throw new Exception();
        }

    }
}
