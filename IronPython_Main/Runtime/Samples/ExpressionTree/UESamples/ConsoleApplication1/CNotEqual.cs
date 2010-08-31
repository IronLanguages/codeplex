using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CNotEqual {
        //NotEqual(Expression, Expression)
        static public void NotEqual1() {
            //<Snippet1>
            // Add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //This expression compares whether the values of its two arguments are different.
            //Both arguments need to be of the same type.
            Expression MyNotEqual = Expression.NotEqual(
                Expression.Constant(42),
                Expression.Constant(45)
            );

            //The end result should be true:
            Console.WriteLine(Expression.Lambda<Func<bool>>(MyNotEqual).Compile().Invoke());
            //</Snippet1>

            //validate sample.
            if (Expression.Lambda<Func<bool>>(MyNotEqual).Compile().Invoke() != true) throw new Exception();
        }

        //NotEqual(Expression, Expression, Boolean, MethodInfo)
        //<Snippet2>
        public static bool NotEqual(int arg1, Exception arg2) {
            return arg1 != Convert.ToInt32(arg2.Message);
        }
        //</Snippet2>


        public static void NotEqual2() {
            //<Snippet2>
            // Add the following directive to your file
            // using Microsoft.Scripting.Ast;

            //This expression represents a comparison of whether the two arguments are different using a user defined operator.
            //The parameters to the NotEqual operation should be reference convertible to the MethodInfo's arguments
            Expression MyNotEqual = Expression.NotEqual(
                Expression.Constant(2),
                Expression.Constant(new Exception("2")),
                false,
                ((Func<int, Exception, bool>)NotEqual).Method
            );

            //The end result should be false:
            Console.WriteLine(Expression.Lambda<Func<bool>>(MyNotEqual).Compile().Invoke());
            //</Snippet2>

            //validate sample.
            if (Expression.Lambda<Func<bool>>(MyNotEqual).Compile().Invoke() != false) throw new Exception();
        }

    }
}
