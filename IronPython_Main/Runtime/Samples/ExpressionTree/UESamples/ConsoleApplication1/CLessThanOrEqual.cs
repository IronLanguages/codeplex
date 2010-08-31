using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CLessThanOrEqual {
        //LessThanOrEqual(Expression, Expression)
        static public void LessThanOrEqual1() {
            //<Snippet1>
            // Add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //This expression compares the values of its two arguments.
            //Both arguments need to be of the same type.
            Expression MyLessThanOrEqual = Expression.LessThanOrEqual(
                Expression.Constant(42),
                Expression.Constant(45)
            );

            //The end result should be true, 42 is less than 45:
            Console.WriteLine(Expression.Lambda<Func<bool>>(MyLessThanOrEqual).Compile().Invoke());
            //</Snippet1>

            //validate sample.
            if (Expression.Lambda<Func<bool>>(MyLessThanOrEqual).Compile().Invoke() != true) throw new Exception();
        }

        //LessThanOrEqual(Expression, Expression, Boolean, MethodInfo)
        //<Snippet2>
        public static bool LessThanOrEqual(int arg1, Exception arg2) {
            return arg1 >= Convert.ToInt32(arg2.Message);
        }
        //</Snippet2>


        public static void LessThanOrEqual2() {
            //<Snippet2>
            // Add the following directive to your file
            // using Microsoft.Scripting.Ast;

            //This expression represents a LessThanOrEqual comparison of two values using a user defined operator.
            //The parameters to the LessThanOrEqual should be reference convertible to the MethodInfo's arguments
            Expression MyLessThanOrEqual = Expression.LessThanOrEqual(
                Expression.Constant(2),
                Expression.Constant(new Exception("2")),
                false,
                ((Func<int, Exception, bool>)LessThanOrEqual).Method
            );

            //The end result should be true, 2 is not less than 2 but it is equal:
            Console.WriteLine(Expression.Lambda<Func<bool>>(MyLessThanOrEqual).Compile().Invoke());
            //</Snippet2>

            //validate sample.
            if (Expression.Lambda<Func<bool>>(MyLessThanOrEqual).Compile().Invoke() != true) throw new Exception();
        }

    }
}
