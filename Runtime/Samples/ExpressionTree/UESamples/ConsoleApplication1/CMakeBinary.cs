using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CMakeBinary {
        //MakeBinary(ExpressionType, Expression, Expression)
        public static void MakeBinary1() {
            //<Snippet1>
            
            //This expression multiplies the values of its two arguments.
            //Both arguments need to be of the same type.
            Expression MyMakeBinary = Expression.MakeBinary(
                ExpressionType.Multiply,
                Expression.Constant(2),
                Expression.Constant(3)
            );

            //The end result should be six:
            Console.WriteLine(Expression.Lambda<Func<int>>(MyMakeBinary).Compile().Invoke());
            //</Snippet1>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyMakeBinary).Compile().Invoke() != 6) throw new Exception();
        }

        //MakeBinary(ExpressionType, Expression, Expression, Boolean, MethodInfo)
        //<Snippet2>
        public static int MakeBinary(int arg1, Exception arg2) {
            return arg1 * Convert.ToInt32(arg2.Message);
        }
        //</Snippet2>


        public static void MakeBinary2() {
            //<Snippet2>

            //This expression represents the multiplication of two arguments using a user defined operator.
            //The parameters to the MakeBinary should be reference convertible to the MethodInfo's arguments
            Expression MyMakeBinary = Expression.MakeBinary(
                ExpressionType.Multiply,
                Expression.Constant(6),
                Expression.Constant(new Exception("2")),
                false,
                ((Func<int, Exception, int>)MakeBinary).Method
            );

            //The end result should be twelve:
            Console.WriteLine(Expression.Lambda<Func<int>>(MyMakeBinary).Compile().Invoke());
            //</Snippet2>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyMakeBinary).Compile().Invoke() != 12) throw new Exception();
        }


        //MakeBinary(ExpressionType, Expression, Expression, boolean MethodInfo, LambdaExpression)
        //<Snippet3>
        public static double MakeBinaryAssignDouble(int arg1, double arg2) {
            return (double)arg1 * arg2;
        }
        //</Snippet3>

        public static void MakeBinary3() {
            //<Snippet3>

            //MakeBinary can take an assignable expression to be used as the left argument.
            ParameterExpression Variable = Expression.Variable(typeof(int), "Variable");

            //This overload of MakeBinary also requires a conversion lambda. This is the 
            //Lambda's parameter
            ParameterExpression Param1 = Expression.Parameter(typeof(double), "Parameter 1");

            //Here we initialize the variable with 6, then use it in an MakeBinary expression
            //with a user defined method.
            //Since the return type of the method is double, we need to provide a conversion
            //to the variable's type.
            Expression MyMakeBinaryAssign = Expression.Block(
                new ParameterExpression[] { Variable },
                Expression.Assign(Variable, Expression.Constant(6)),
                Expression.MakeBinary(
                    ExpressionType.MultiplyAssign,
                    Variable,
                    Expression.Constant((double)3),
                    false,
                    ((Func<int, double, double>)MakeBinaryAssignDouble).Method,
                    Expression.Lambda<Func<double, int>>(
                        Expression.Convert(Param1, typeof(int)),
                        new ParameterExpression[] { Param1 }
                    )
                )
            );

            //The result should be 18.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyMakeBinaryAssign).Compile().Invoke());
            //</Snippet3>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyMakeBinaryAssign).Compile().Invoke() != 18) throw new Exception();
        }
    }
}
