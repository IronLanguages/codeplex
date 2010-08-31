using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CLeftShift {
        // LeftShift(Expression, Expression)
        public static void LeftShift1() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            // This expression represents left shifting the value of the first argument the number
            // of bits specified by the value of the second argument (i.e. 4 << 1).
            Expression MyShift = 
                Expression.Block(
                    Expression.LeftShift(
                        Expression.Constant(4),
                        Expression.Constant(1)
                    )
            );
            
            // The result should be eight:
            Console.WriteLine(Expression.Lambda<Func<int>>(MyShift).Compile().Invoke());
            //</Snippet1>

            // validate sample
            if (Expression.Lambda<Func<int>>(MyShift).Compile().Invoke() != 8)
                throw new Exception("LeftShiftSample1 failed");
        }

        // LeftShift(Expression, Expression, MethodInfo)
        //<Snippet2>
        public static int LShift(int arg1, Exception arg2) {
            return arg1 << Convert.ToInt32(arg2.Message);
        }
        //</Snippet2>
        
        public static void LeftShift2() {
            //<Snippet2>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            // This expression represents left shifting using a user-defined operator.
            // The parameters to the LeftShift should be reference convertible to the MethodInfo's arguments.
            Expression MyShift = Expression.LeftShift(
                Expression.Constant(4),
                Expression.Constant(new Exception("1")),
                ((Func<int, Exception, int>)LShift).Method
            );

            // The result should be eight:
            Console.WriteLine(Expression.Lambda<Func<int>>(MyShift).Compile().Invoke());
            //</Snippet2>

            // validate sample
            if (Expression.Lambda<Func<int>>(MyShift).Compile().Invoke() != 8)
                throw new Exception("LeftShiftSample2 failed");
        }

        //LeftShiftAssign(Expression, Expression)
        public static void LeftShiftAssign1() {
            //<Snippet3>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            // LeftShiftAssign requires an assignable expression to be used as the left argument.
            ParameterExpression Variable = Expression.Variable(typeof(int), "Variable");

            // Here we initialize the variable with 4, then use it in an LeftShiftAssign expression.
            // both the LeftShiftAssign expression and the variable will have the value 8 after the
            // tree is executed.
            Expression MyLeftShiftAssign = Expression.Block(
                new ParameterExpression[] { Variable },
                Expression.Assign(Variable, Expression.Constant(4)),
                Expression.LeftShiftAssign(
                    Variable,
                    Expression.Constant(1)
                )
            );

            // The result should be 8.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyLeftShiftAssign).Compile().Invoke());
            //</Snippet3>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyLeftShiftAssign).Compile().Invoke() != 8) throw new Exception();
        }

        //LeftShiftAssign(Expression, Expression, MethodInfo)
        //<Snippet6>
        public static int LShiftAssign(int arg1, double arg2) {
            return arg1 << (int)arg2;
        }
        //</Snippet6>

        public static void LeftShiftAssign2() {
            //<Snippet6>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            // LeftShiftAssign requires an assignable expression to be used as the left argument.
            ParameterExpression Variable = Expression.Variable(typeof(int), "Variable");

            // Here we initialize the variable with 2, then use it in an LeftShiftAssign expression
            // with a user defined method.            
            Expression MyLeftShiftAssign = Expression.Block(
                new ParameterExpression[] { Variable },
                Expression.Assign(Variable, Expression.Constant(2)),
                Expression.LeftShiftAssign(
                    Variable,
                    Expression.Constant((double)2),
                    ((Func<int, double, int>)LShiftAssign).Method
                )
            );

            // The result should be 8.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyLeftShiftAssign).Compile().Invoke());
            //</Snippet6>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyLeftShiftAssign).Compile().Invoke() != 8) throw new Exception();
        }

        //LeftShiftAssign(Expression, Expression, MethodInfo, LambdaExpression)
        //<Snippet7>
        public static double LShiftAssignDouble(int arg1, double arg2) {
            return (double)(arg1 << (int)arg2);
        }
        //</Snippet7>

        public static void LeftShiftAssign3() {
            //<Snippet7>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //LeftShiftAssign requires an assignable expression to be used as the left argument.
            ParameterExpression Variable = Expression.Variable(typeof(int), "Variable");

            //This overload of LeftShiftAssign also requires a conversion lambda. This is the 
            //Lambda's parameter
            ParameterExpression Param1 = Expression.Parameter(typeof(double), "Parameter 1");

            //Here we initialize the variable with 2, then use it in an LeftShiftAssign expression
            //with a user defined method.
            //Since the return type of the method is double, we need to provide a conversion
            //to the variable's type.
            Expression MyLeftShiftAssign = Expression.Block(
                new ParameterExpression[] { Variable },
                Expression.Assign(Variable, Expression.Constant(2)),
                Expression.LeftShiftAssign(
                    Variable,
                    Expression.Constant((double)2),
                    ((Func<int, double, double>)LShiftAssignDouble).Method,
                    Expression.Lambda<Func<double, int>>(
                        Expression.Convert(Param1, typeof(int)),
                        new ParameterExpression[] { Param1 }
                    )
                )
            );

            //The result should be 8.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyLeftShiftAssign).Compile().Invoke());
            //</Snippet7>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyLeftShiftAssign).Compile().Invoke() != 8) throw new Exception();
        }


    }
}
