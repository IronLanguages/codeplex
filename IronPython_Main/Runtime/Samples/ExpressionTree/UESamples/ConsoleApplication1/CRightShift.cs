using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CRightShift {
        // RightShift(Expression, Expression)
        public static void RightShiftSample1() {
            //<Snippet1>
            // This expression represents shifting the value of the first argument right the number
            // of bits specified by the value of the second argument (i.e. 4 >> 1).
            Expression MyShift = 
                Expression.Block(
                    Expression.RightShift(
                        Expression.Constant(4),
                        Expression.Constant(1)
                    )
            );
            
            // The result should be two:
            Console.WriteLine(Expression.Lambda<Func<int>>(MyShift).Compile().Invoke());
            //</Snippet1>

            // validate sample
            if (Expression.Lambda<Func<int>>(MyShift).Compile().Invoke() != 2)
                throw new Exception("RightShiftSample1 failed");
        }

        // RightShift(Expression, Expression, MethodInfo)
        //<Snippet2>
        public static int RShift(int arg1, Exception arg2) {
            return arg1 >> Convert.ToInt32(arg2.Message);
        }
        //</Snippet2>
        
        public static void RightShiftSample2() {
            //<Snippet2>
            // This expression represents right shifting using a user-defined operator.
            // The parameters to the RightShift should be reference convertible to the MethodInfo's arguments.
            Expression MyShift = Expression.RightShift(
                Expression.Constant(4),
                Expression.Constant(new Exception("1")),
                ((Func<int, Exception, int>)RShift).Method
            );

            // The result should be two:
            Console.WriteLine(Expression.Lambda<Func<int>>(MyShift).Compile().Invoke());
            //</Snippet2>

            // validate sample
            if (Expression.Lambda<Func<int>>(MyShift).Compile().Invoke() != 2)
                throw new Exception("RightShiftSample2 failed");
        }

        //RightShiftAssign(Expression, Expression)
        public static void RightShiftAssignSample1() {
            //<Snippet3>
            // RightShiftAssign requires an assignable expression to be used as the left argument.
            ParameterExpression Variable = Expression.Variable(typeof(int), "Variable");

            // Here we initialize the variable with 8, then use it in an RightShiftAssign expression.
            // both the RightShiftAssign expression and the variable will have the value 4 after the
            // tree is executed.
            Expression MyRightShiftAssign = Expression.Block(
                new ParameterExpression[] { Variable },
                Expression.Assign(Variable, Expression.Constant(8)),
                Expression.RightShiftAssign(
                    Variable,
                    Expression.Constant(1)
                )
            );

            // The result should be 4.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyRightShiftAssign).Compile().Invoke());
            //</Snippet3>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyRightShiftAssign).Compile().Invoke() != 4) throw new Exception();
        }

        //RightShiftAssign(Expression, Expression, MethodInfo)
        //<Snippet6>
        public static int RShiftAssign(int arg1, double arg2) {
            return arg1 >> (int)arg2;
        }
        //</Snippet6>

        public static void RightShiftAssignSample2() {
            //<Snippet6>
            // RightShiftAssign requires an assignable expression to be used as the left argument.
            ParameterExpression Variable = Expression.Variable(typeof(int), "Variable");

            // Here we initialize the variable with 8, then use it in an RightShiftAssign expression
            // with a user defined method.            
            Expression MyRightShiftAssign = Expression.Block(
                new ParameterExpression[] { Variable },
                Expression.Assign(Variable, Expression.Constant(8)),
                Expression.RightShiftAssign(
                    Variable,
                    Expression.Constant((double)2),
                    ((Func<int, double, int>)RShiftAssign).Method
                )
            );

            // The result should be 2.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyRightShiftAssign).Compile().Invoke());
            //</Snippet6>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyRightShiftAssign).Compile().Invoke() != 2) throw new Exception();
        }

        //RightShiftAssign(Expression, Expression, MethodInfo, LambdaExpression)
        //<Snippet7>
        public static double RShiftAssignDouble(int arg1, double arg2) {
            return (double)(arg1 >> (int)arg2);
        }
        //</Snippet7>

        public static void RightShiftAssignSample3() {
            //<Snippet7>
            //RightShiftAssign requires an assignable expression to be used as the left argument.
            ParameterExpression Variable = Expression.Variable(typeof(int), "Variable");

            //This overload of RightShiftAssign also requires a conversion lambda. This is the 
            //Lambda's parameter
            ParameterExpression Param1 = Expression.Parameter(typeof(double), "Parameter 1");

            //Here we initialize the variable with 8, then use it in an RightShiftAssign expression
            //with a user defined method.
            //Since the return type of the method is double, we need to provide a conversion
            //to the variable's type.
            Expression MyRightShiftAssign = Expression.Block(
                new ParameterExpression[] { Variable },
                Expression.Assign(Variable, Expression.Constant(8)),
                Expression.RightShiftAssign(
                    Variable,
                    Expression.Constant((double)2),
                    ((Func<int, double, double>)RShiftAssignDouble).Method,
                    Expression.Lambda<Func<double, int>>(
                        Expression.Convert(Param1, typeof(int)),
                        new ParameterExpression[] { Param1 }
                    )
                )
            );

            //The result should be 2.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyRightShiftAssign).Compile().Invoke());
            //</Snippet7>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyRightShiftAssign).Compile().Invoke() != 2) throw new Exception();
        }


    }
}
