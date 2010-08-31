using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CIncrement {
        //Expression.Increment(Expression)
        static public void Increment1() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //This Expression represents a Increment operation, or adding 1 to a value. 
            Expression MyIncrement = Expression.Increment(
                                        Expression.Constant(5.5)
                                    );

            //Should print 6.5.
            Console.WriteLine(Expression.Lambda<Func<double>>(MyIncrement).Compile().Invoke());

            //</Snippet1>

            //Validate sample
            if (Expression.Lambda<Func<double>>(MyIncrement).Compile().Invoke() != 6.5) throw new Exception("");
        }

        //<Snippet2>
        public static double IncrementMethod(double arg) {
            return arg + 2;
        }
        //</Snippet2>

        //Expression.Increment(Expression, MethodInfo)
        static public void Increment2() {
            //<Snippet2>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //This Expression represents a user defined Increment operation; It will use the specified user defined operation.
            Expression MyIncrement = Expression.Increment(
                                        Expression.Constant(5.5),
                                        ((Func<double, double>) IncrementMethod).Method
                                    );

            //Should print 7.5
            Console.WriteLine(Expression.Lambda<Func<double>>(MyIncrement).Compile().Invoke());

            //</Snippet2>

            //Validate sample
            if (Expression.Lambda<Func<double>>(MyIncrement).Compile().Invoke() != 7.5) throw new Exception("");
        }

        //Expression.PreIncrementAssign(Expression)
        public static void PreIncrementAssignSample1() {
            //<Snippet3>
            // PreIncrementAssign requires an assignable expression to be used as the left argument.
            ParameterExpression MyVariable = Expression.Variable(typeof(int), "MyVariable");

            // This Expression represents a PreIncrementAssign operation. 
            // It increments the given value by one and assigns the result back to the expression.
            Expression MyPreIncrementAssign = 
                Expression.Block(
                    new ParameterExpression[] { MyVariable },
                    Expression.Assign(MyVariable, Expression.Constant(5)),
                    Expression.PreIncrementAssign(MyVariable)
                );

            //Should print 6.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyPreIncrementAssign).Compile().Invoke());

            //</Snippet3>

            //Validate sample
            if (Expression.Lambda<Func<int>>(MyPreIncrementAssign).Compile().Invoke() != 6) throw new Exception("");
        }

        //<Snippet4>
        public static double PreIncrementAssignMethod(double arg) {
            return arg + 2;
        }
        //</Snippet4>

        //Expression.PreIncrementAssign(Expression, MethodInfo)
        public static void PreIncrementAssignSample2() {
            //<Snippet4>
            // PreIncrementAssign requires an assignable expression to be used as the left argument.
            ParameterExpression MyVariable = Expression.Variable(typeof(double), "MyVariable");

            // This Expression represents a user defined PreIncrementAssign operation; It will use the specified user defined operation.
            Expression MyPreIncrementAssign = 
                Expression.Block(
                    new ParameterExpression[] { MyVariable },
                    Expression.Assign(MyVariable, Expression.Constant(5.5)),
                    Expression.PreIncrementAssign(
                        MyVariable,
                        ((Func<double, double>)PreIncrementAssignMethod).Method
                    )
                );

            // Should print 7.5
            Console.WriteLine(Expression.Lambda<Func<double>>(MyPreIncrementAssign).Compile().Invoke());
            //</Snippet4>

            //Validate sample
            if (Expression.Lambda<Func<double>>(MyPreIncrementAssign).Compile().Invoke() != 7.5) throw new Exception("");
        }

        //Expression.PostIncrementAssign(Expression)
        public static void PostIncrementAssignSample1() {
            //<Snippet5>
            // PostIncrementAssign requires an assignable expression to be used as the left argument.
            ParameterExpression MyVariable = Expression.Variable(typeof(int), "MyVariable");

            // This Expression represents a PostIncrementAssign operation. 
            // It assigns to the given Expression and then increments the original value by one.
            Expression MyPostIncrementAssign =
                Expression.Block(
                    new ParameterExpression[] { MyVariable },
                    Expression.Assign(MyVariable, Expression.Constant(5)),
                    Expression.PostIncrementAssign(MyVariable)
                );

            //Should print 5.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyPostIncrementAssign).Compile().Invoke());
            //</Snippet5>

            //Validate sample
            if (Expression.Lambda<Func<int>>(MyPostIncrementAssign).Compile().Invoke() != 5) throw new Exception("");
        }

        //<Snippet6>
        public static double PostIncrementAssignMethod(double arg) {
            return arg + 2;
        }
        //</Snippet6>

        //Expression.PostIncrementAssign(Expression, MethodInfo)
        public static void PostIncrementAssignSample2() {
            //<Snippet6>
            // PostIncrementAssign requires an assignable expression to be used as the left argument.
            ParameterExpression MyVariable = Expression.Variable(typeof(double), "MyVariable");

            //This Expression represents a user defined PostIncrementAssign operation; It will use the specified user defined operation.
            Expression MyPostIncrementAssign =
                Expression.Block(
                    new ParameterExpression[] { MyVariable },
                    Expression.Assign(MyVariable, Expression.Constant(5.5)),
                    Expression.PostIncrementAssign(
                        MyVariable,
                        ((Func<double, double>)PostIncrementAssignMethod).Method
                    )
                );

            //Should print 5.5 because MyVariable was assigned 5.5 before PostIncrementAssignMethod was invoked.
            Console.WriteLine(Expression.Lambda<Func<double>>(MyPostIncrementAssign).Compile().Invoke());
            //</Snippet6>

            //Validate sample
            if (Expression.Lambda<Func<double>>(MyPostIncrementAssign).Compile().Invoke() != 5.5) throw new Exception("");
        }
    }
}
