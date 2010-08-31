using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CDecrement {
        //Expression.Decrement(Expression)
        static public void Decrement1() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //This Expression represents a Decrement operation, or subtracting 1 from a value. 
            //expression to the type specified.
            Expression MyDecrement = Expression.Decrement(
                                        Expression.Constant(5.5)
                                    );

            //Should print 4.5.
            Console.WriteLine(Expression.Lambda<Func<double>>(MyDecrement).Compile().Invoke());

            //</Snippet1>

            //Validate sample
            if (Expression.Lambda<Func<double>>(MyDecrement).Compile().Invoke() != 4.5) throw new Exception("");
        }

        //<Snippet2>
        public static double DecrementMethod(double arg) {
            return arg - 2;
        }
        //</Snippet2>

        //Expression.Decrement(Expression, MethodInfo)
        static public void Decrement2() {
            //<Snippet2>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //This Expression represents a user defined Decrement operation; It will use the specified user defined operation.
            Expression MyDecrement = Expression.Decrement(
                                        Expression.Constant(5.5),
                                        ((Func<double, double>) DecrementMethod).Method
                                    );

            //Should print 3.5
            Console.WriteLine(Expression.Lambda<Func<double>>(MyDecrement).Compile().Invoke());

            //</Snippet2>

            //Validate sample
            if (Expression.Lambda<Func<double>>(MyDecrement).Compile().Invoke() != 3.5) throw new Exception("");
        }

        //Expression.PreDecrementAssign(Expression)
        public static void PreDecrementAssignSample1() {
            //<Snippet3>
            // PreDecrementAssign requires an assignable expression to be used as the left argument.
            ParameterExpression MyVariable = Expression.Variable(typeof(int), "MyVariable");

            // This Expression represents a PreDecrementAssign operation. 
            // It decrements the given value by one and assigns the result back to the expression.
            Expression MyPreDecrementAssign =
                Expression.Block(
                    new ParameterExpression[] { MyVariable },
                    Expression.Assign(MyVariable, Expression.Constant(5)),
                    Expression.PreDecrementAssign(MyVariable)
                );

            //Should print 4.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyPreDecrementAssign).Compile().Invoke());

            //</Snippet3>

            //Validate sample
            if (Expression.Lambda<Func<int>>(MyPreDecrementAssign).Compile().Invoke() != 4) throw new Exception("");
        }

        //<Snippet4>
        public static double PreDecrementAssignMethod(double arg) {
            return arg - 2;
        }
        //</Snippet4>

        //Expression.PreDecrementAssign(Expression, MethodInfo)
        public static void PreDecrementAssignSample2() {
            //<Snippet4>
            // PreDecrementAssign requires an assignable expression to be used as the left argument.
            ParameterExpression MyVariable = Expression.Variable(typeof(double), "MyVariable");

            //This Expression represents a user defined PreDecrementAssign operation; It will use the specified user defined operation.
            Expression MyPreDecrementAssign =
                Expression.Block(
                    new ParameterExpression[] { MyVariable },
                    Expression.Assign(MyVariable, Expression.Constant(5.5)),
                    Expression.PreDecrementAssign(
                        MyVariable,
                        ((Func<double, double>)PreDecrementAssignMethod).Method
                    ),
                    MyVariable
                );

            //Should print 3.5
            Console.WriteLine(Expression.Lambda<Func<double>>(MyPreDecrementAssign).Compile().Invoke());
            //</Snippet4>

            //Validate sample
            if (Expression.Lambda<Func<double>>(MyPreDecrementAssign).Compile().Invoke() != 3.5) throw new Exception("");
        }

        //Expression.PostDecrementAssign(Expression)
        public static void PostDecrementAssignSample1() {
            //<Snippet5>
            // PostDecrementAssign requires an assignable expression to be used as the left argument.
            ParameterExpression MyVariable = Expression.Variable(typeof(int), "MyVariable");

            // This Expression represents a PostDecrementAssign operation. 
            // It assigns to the given Expression and then decrements the original value by one.
            Expression MyPostDecrementAssign =
                Expression.Block(
                    new ParameterExpression[] { MyVariable },
                    Expression.Assign(MyVariable, Expression.Constant(5)),
                    Expression.PostDecrementAssign(MyVariable)
                );

            //Should print 5.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyPostDecrementAssign).Compile().Invoke());
            //</Snippet5>

            //Validate sample
            if (Expression.Lambda<Func<int>>(MyPostDecrementAssign).Compile().Invoke() != 5) throw new Exception("");
        }

        //<Snippet6>
        public static double PostDecrementAssignMethod(double arg) {
            return arg - 2;
        }
        //</Snippet6>

        //Expression.PostDecrementAssign(Expression, MethodInfo)
        public static void PostDecrementAssignSample2() {
            //<Snippet6>
            // PostDecrementAssign requires an assignable expression to be used as the left argument.
            ParameterExpression MyVariable = Expression.Variable(typeof(double), "MyVariable");

            //This Expression represents a user defined PostDecrementAssign operation; It will use the specified user defined operation.
            Expression MyPostDecrementAssign =
                Expression.Block(
                    new ParameterExpression[] { MyVariable },
                    Expression.Assign(MyVariable, Expression.Constant(5.5)),
                    Expression.PostDecrementAssign(
                        MyVariable,
                        ((Func<double, double>)PostDecrementAssignMethod).Method
                    )
                );

            //Should print 5.5 because MyVariable was assigned 5.5 before PostDecrementAssignMethod was invoked.
            Console.WriteLine(Expression.Lambda<Func<double>>(MyPostDecrementAssign).Compile().Invoke());
            //</Snippet6>

            //Validate sample
            if (Expression.Lambda<Func<double>>(MyPostDecrementAssign).Compile().Invoke() != 5.5) throw new Exception("");
        }
    }
}
