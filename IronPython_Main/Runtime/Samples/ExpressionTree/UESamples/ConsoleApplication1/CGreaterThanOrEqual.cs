using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CGreaterThanOrEqual {
        //GreaterThanOrEqual(Expression, Expression)
        static public void GreaterThanOrEqual1() {
            //<Snippet1>
            // Add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //This expression compares the values of its two arguments.
            //Both arguments need to be of the same type.
            Expression MyGreaterThanOrEqual = Expression.GreaterThanOrEqual(
                Expression.Constant(42),
                Expression.Constant(45)
            );

            //The end result should be false, 42 is not greater or equal to 45:
            Console.WriteLine(Expression.Lambda<Func<bool>>(MyGreaterThanOrEqual).Compile().Invoke());
            //</Snippet1>

            //validate sample.
            if (Expression.Lambda<Func<bool>>(MyGreaterThanOrEqual).Compile().Invoke() != false) throw new Exception();
        }

        //GreaterThanOrEqual(Expression, Expression, Boolean, MethodInfo)
        //<Snippet2>
        public static bool GreaterThanOrEqual(int arg1, Exception arg2) {
            return arg1 >= Convert.ToInt32(arg2.Message);
        }
        //</Snippet2>


        public static void GreaterThanOrEqual2() {
            //<Snippet2>
            // Add the following directive to your file
            // using Microsoft.Scripting.Ast;

            //This expression represents a GreaterThanOrEqual comparison of two values using a user defined operator.
            //The parameters to the GreaterThanOrEqual should be reference convertible to the MethodInfo's arguments
            Expression MyGreaterThanOrEqual = Expression.GreaterThanOrEqual(
                Expression.Constant(2),
                Expression.Constant(new Exception("2")),
                false,
                ((Func<int, Exception, bool>)GreaterThanOrEqual).Method
            );

            //The end result should be true, 2 is not greater than 2 but it is equal:
            Console.WriteLine(Expression.Lambda<Func<bool>>(MyGreaterThanOrEqual).Compile().Invoke());
            //</Snippet2>

            //validate sample.
            if (Expression.Lambda<Func<bool>>(MyGreaterThanOrEqual).Compile().Invoke() != true) throw new Exception();
        }

    }
}
