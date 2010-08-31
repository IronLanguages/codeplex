using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CMultiply {
        //Multiply(Expression, Expression)
        public static void MultiplySample1() {
            //<Snippet1>
            //This expression multiplies the values of its two arguments.
            //Both arguments need to be of the same type.
            Expression MyMultiply = Expression.Multiply(
                Expression.Constant(2),
                Expression.Constant(3)
            );

            //The end result should be six:
            Console.WriteLine(Expression.Lambda<Func<int>>(MyMultiply).Compile().Invoke());
            //</Snippet1>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyMultiply).Compile().Invoke() != 6) throw new Exception();
        }

        //Multiply(Expression, Expression, MethodInfo)
        //<Snippet2>
        public static int Multiply(int arg1, Exception arg2) {
            return arg1 * Convert.ToInt32(arg2.Message);
        }
        //</Snippet2>


        public static void MultiplySample2() {
            //<Snippet2>
            //This expression represents the multiplication of two arguments using a user defined operator.
            //The parameters to the multiply should be reference convertible to the MethodInfo's arguments
            Expression MyMultiply = Expression.Multiply(
                Expression.Constant(6),
                Expression.Constant(new Exception("2")),
                ((Func<int, Exception, int>)Multiply).Method
            );

            //The end result should be twelve:
            Console.WriteLine(Expression.Lambda<Func<int>>(MyMultiply).Compile().Invoke());
            //</Snippet2>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyMultiply).Compile().Invoke() != 12) throw new Exception();
        }

        //MultiplyAssign(Expression, Expression)
        public static void MultiplyAssignSample1() {
            //<Snippet5>
            //MultiplyAssign requires an assignable expression to be used as the left argument.
            ParameterExpression Variable = Expression.Variable(typeof(int), "Variable");

            //Here we initialize the variable with 6, then use it in an MultiplyAssign expression.
            //both the MultiplyAssign expression and the variable will have the value 18 after the
            //tree is executed.
            Expression MyMultiplyAssign = Expression.Block(
                new ParameterExpression[] { Variable },
                Expression.Assign(Variable, Expression.Constant(6)),
                Expression.MultiplyAssign(
                    Variable,
                    Expression.Constant(3)
                )
            );

            //The result should be 18.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyMultiplyAssign).Compile().Invoke());
            //</Snippet5>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyMultiplyAssign).Compile().Invoke() != 18) throw new Exception();
        }

        //MultiplyAssign(Expression, Expression, MethodInfo)
        //<Snippet6>
        public static int MultiplyAssign(int arg1, double arg2) {
            return arg1 * (int)arg2;
        }
        //</Snippet6>

        public static void MultiplyAssignSample2() {
            //<Snippet6>
            //MultiplyAssign requires an assignable expression to be used as the left argument.
            ParameterExpression Variable = Expression.Variable(typeof(int), "Variable");

            //Here we initialize the variable with 6, then use it in an MultiplyAssign expression
            //with a user defined method.            
            Expression MyMultiplyAssign = Expression.Block(
                new ParameterExpression[] { Variable },
                Expression.Assign(Variable, Expression.Constant(6)),
                Expression.MultiplyAssign(
                    Variable,
                    Expression.Constant((double)3),
                    ((Func<int, double, int>)MultiplyAssign).Method
                )
            );

            //The result should be 18.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyMultiplyAssign).Compile().Invoke());
            //</Snippet6>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyMultiplyAssign).Compile().Invoke() != 18) throw new Exception();
        }

        //MultiplyAssign(Expression, Expression, MethodInfo, LambdaExpression)
        //<Snippet7>
        public static double MultiplyAssignDouble(int arg1, double arg2) {
            return (double)arg1 * arg2;
        }
        //</Snippet7>

        public static void MultiplyAssignSample3() {
            //<Snippet7>
            //MultiplyAssign requires an assignable expression to be used as the left argument.
            ParameterExpression Variable = Expression.Variable(typeof(int), "Variable");

            //This overload of MultiplyAssign also requires a conversion lambda. This is the 
            //Lambda's parameter
            ParameterExpression Param1 = Expression.Parameter(typeof(double), "Parameter 1");

            //Here we initialize the variable with 6, then use it in an MultiplyAssign expression
            //with a user defined method.
            //Since the return type of the method is double, we need to provide a conversion
            //to the variable's type.
            Expression MyMultiplyAssign = Expression.Block(
                new ParameterExpression[] { Variable },
                Expression.Assign(Variable, Expression.Constant(6)),
                Expression.MultiplyAssign(
                    Variable,
                    Expression.Constant((double)3),
                    ((Func<int, double, double>)MultiplyAssignDouble).Method,
                    Expression.Lambda<Func<double, int>>(
                        Expression.Convert(Param1, typeof(int)),
                        new ParameterExpression[] { Param1 }
                    )
                )
            );

            //The result should be 18.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyMultiplyAssign).Compile().Invoke());
            //</Snippet7>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyMultiplyAssign).Compile().Invoke() != 18) throw new Exception();
        }

        //MultiplyChecked(Expression, Expression)
        static public void MultiplyCheckedSample1() {
            //<Snippet8>
            //This expression multiplies the values of its two arguments.
            //Both arguments need to be of the same type.
            //If the result is larger than the type of the operation,
            //an OverflowException is thrown.
            Expression MyMultiply = Expression.MultiplyChecked(
                Expression.Constant(int.MaxValue),
                Expression.Constant(2)
            );

            //An exception should happen:
            try {
                Expression.Lambda<Func<int>>(MyMultiply).Compile().Invoke();
            } catch (OverflowException) {
                Console.WriteLine("Expected exception thrown");
            }
            //</Snippet8>

            //validate sample.
            try {
                Expression.Lambda<Func<int>>(MyMultiply).Compile().Invoke();
                throw new Exception("Expected Overflow Exception, no exception thrown.");
            } catch (OverflowException) {
            }
        }

        //MultiplyChecked(Expression, Expression, MethodInfo)
        //<Snippet9>
        public static Int64 MultiplyChecked(int arg1, Exception arg2) {
            Int64 res = (Int64)arg1 * Convert.ToInt64(arg2.Message);
            if (res > Int64.MaxValue || res < Int64.MinValue) throw new OverflowException();
            return res;
        }
        //</Snippet9>


        public static void MultiplyCheckedSample2() {
            //<Snippet9>
            //This expression represents the multiplication of two arguments using a user defined operator.
            //The parameters to the multiplication should be reference convertible to the MethodInfo's arguments
            Expression MyMultiply = Expression.MultiplyChecked(
                Expression.Constant(Int32.MaxValue),
                Expression.Constant(new Exception("2")),
                ((Func<int, Exception, Int64>)MultiplyChecked).Method
            );

            Console.WriteLine(Expression.Lambda<Func<Int64>>(MyMultiply).Compile().Invoke());
            //</Snippet9>

            //validate sample.
            if (Expression.Lambda<Func<Int64>>(MyMultiply).Compile().Invoke() != (Int32.MaxValue * (Int64)2)) throw new Exception();
        }

        //MultiplyAssignChecked(Expression, Expression)
        public static void MultiplyAssignCheckedSample1() {
            //<Snippet10>
            //MultiplyAssign requires an assignable expression to be used as the left argument.
            ParameterExpression Variable = Expression.Variable(typeof(int), "Variable");

            //Here we initialize the variable with Int32.MaxValue, then use it in an MultiplyAssign expression.
            Expression MyMultiplyAssignChecked = Expression.Block(
                new ParameterExpression[] { Variable },
                Expression.Assign(Variable, Expression.Constant(Int32.MaxValue)),
                Expression.MultiplyAssignChecked(
                    Variable,
                    Expression.Constant(2)
                )
            );

            //An exception should happen:
            try {
                Expression.Lambda<Func<int>>(MyMultiplyAssignChecked).Compile().Invoke();
            } catch (OverflowException) {
                Console.WriteLine("Expected exception thrown");
            }
            //</Snippet10>

            //validate sample.
            try {
                Expression.Lambda<Func<int>>(MyMultiplyAssignChecked).Compile().Invoke();
                throw new Exception("Expected Overflow Exception, no exception thrown.");
            } catch (OverflowException) {
            }
        }

        //MultiplyAssignChecked(Expression, Expression, MethodInfo)
        //<Snippet11>
        public static int MultiplyAssignChecked(int arg1, double arg2) {
            Int64 res = (Int64)arg1 * Convert.ToInt64(arg2);
            if (res > Int32.MaxValue || res < Int32.MinValue) throw new OverflowException();
            return (int)res;
        }
        //</Snippet11>

        public static void MultiplyAssignCheckedSample2() {
            //<Snippet11>
            //MultiplyAssign requires an assignable expression to be used as the left argument.
            ParameterExpression Variable = Expression.Variable(typeof(int), "Variable");

            //Here we initialize the variable with Int32.MaxValue, then use it in an MultiplyAssign expression
            //with a user defined method.            
            Expression MyMultiplyAssignChecked = Expression.Block(
                new ParameterExpression[] { Variable },
                Expression.Assign(Variable, Expression.Constant(Int32.MaxValue)),
                Expression.MultiplyAssignChecked(
                    Variable,
                    Expression.Constant((double)2.0),
                    ((Func<int, double, int>)MultiplyAssignChecked).Method
                )
            );

            //An exception should happen:
            try {
                Expression.Lambda<Func<int>>(MyMultiplyAssignChecked).Compile().Invoke();
            } catch (OverflowException) {
                Console.WriteLine("Expected exception thrown");
            }
            //</Snippet11>

            //validate sample.
            try {
                Expression.Lambda<Func<int>>(MyMultiplyAssignChecked).Compile().Invoke();
                throw new Exception("Expected Overflow Exception, no exception thrown.");
            } catch (OverflowException) {
            }
        }

        //MultiplyAssign(Expression, Expression, MethodInfo, LambdaExpression)
        //<Snippet12>
        public static double MultiplyAssignCheckedDouble(int arg1, double arg2) {
            Int64 res = (Int64)arg1 * Convert.ToInt64(arg2);
            if (res > Int32.MaxValue || res < Int32.MinValue) throw new OverflowException();
            return (double)res;
        }
        //</Snippet12>

        public static void MultiplyAssignCheckedSample3() {
            //<Snippet10>
            //MultiplyAssign requires an assignable expression to be used as the left argument.
            ParameterExpression Variable = Expression.Variable(typeof(int), "Variable");

            //This overload of MultiplyAssign also requires a conversion lambda. This is the 
            //Lambda's parameter
            ParameterExpression Param1 = Expression.Parameter(typeof(double), "Parameter 1");

            //Here we initialize the variable with Int32.MaxValue, then use it in an MultiplyAssign expression
            //with a user defined method.
            //Since the return type of the method is double, we need to provide a conversion
            //to the variable's type.
            Expression MyMultiplyAssignChecked = Expression.Block(
                new ParameterExpression[] { Variable },
                Expression.Assign(Variable, Expression.Constant(Int32.MaxValue)),
                Expression.MultiplyAssignChecked(
                    Variable,
                    Expression.Constant((double)2.0),
                    ((Func<int, double, double>)MultiplyAssignCheckedDouble).Method,
                    Expression.Lambda<Func<double, int>>(
                        Expression.Convert(Param1, typeof(int)),
                        new ParameterExpression[] { Param1 }
                    )
                )
            );

            //An exception should happen:
            try {
                Expression.Lambda<Func<int>>(MyMultiplyAssignChecked).Compile().Invoke();
            } catch (OverflowException) {
                Console.WriteLine("Expected exception thrown");
            }
            //</Snippet11>

            //validate sample.
            try {
                Expression.Lambda<Func<int>>(MyMultiplyAssignChecked).Compile().Invoke();
                throw new Exception("Expected Overflow Exception, no exception thrown.");
            } catch (OverflowException) {
            }
        }
    }
}
