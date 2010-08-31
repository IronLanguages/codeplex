using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CEqual {
        //Equal(Expression, Expression)
        static public void Equal1() {
            //<Snippet1>
            // Add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //This expression compares the values of its two arguments.
            //Both arguments need to be of the same type.
            Expression MyEqual = Expression.Equal(
                Expression.Constant(42),
                Expression.Constant(45)
            );

            //The end result should be false, 45 is not the answer:
            Console.WriteLine(Expression.Lambda<Func<bool>>(MyEqual).Compile().Invoke());
            //</Snippet1>

            //validate sample.
            if (Expression.Lambda<Func<bool>>(MyEqual).Compile().Invoke() != false) throw new Exception();
        }

        //Equal(Expression, Expression, Boolean, MethodInfo)
        //<Snippet2>
        public static bool Equal(int arg1, Exception arg2) {
            return arg1 == Convert.ToInt32(arg2.Message);
        }
        //</Snippet2>


        public static void Equal2() {
            //<Snippet2>
            // Add the following directive to your file
            // using Microsoft.Scripting.Ast;

            //This expression represents an Equality of two arguments using a user defined operator.
            //The parameters to the Equality should be reference convertible to the MethodInfo's arguments
            Expression MyEqual = Expression.Equal(
                Expression.Constant(2),
                Expression.Constant(new Exception("2")),
                false,
                ((Func<int, Exception, bool>)Equal).Method
            );

            //The end result should be true:
            Console.WriteLine(Expression.Lambda<Func<bool>>(MyEqual).Compile().Invoke());
            //</Snippet2>

            //validate sample.
            if (Expression.Lambda<Func<bool>>(MyEqual).Compile().Invoke() != true) throw new Exception();
        }

    }
}
