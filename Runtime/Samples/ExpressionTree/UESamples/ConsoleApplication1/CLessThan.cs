using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CLessThan {
        //LessThan(Expression, Expression)
        static public void LessThan1() {
            //<Snippet1>
            // Add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //This expression compares the values of its two arguments.
            //Both arguments need to be of the same type.
            Expression MyLessThan = Expression.LessThan(
                Expression.Constant(42),
                Expression.Constant(45)
            );

            //The end result should be true, 42 is less than 45:
            Console.WriteLine(Expression.Lambda<Func<bool>>(MyLessThan).Compile().Invoke());
            //</Snippet1>

            //validate sample.
            if (Expression.Lambda<Func<bool>>(MyLessThan).Compile().Invoke() != true) throw new Exception();
        }

        //LessThan(Expression, Expression, Boolean, MethodInfo)
        //<Snippet2>
        public static bool LessThan(int arg1, Exception arg2) {
            return arg1 < Convert.ToInt32(arg2.Message);
        }
        //</Snippet2>


        public static void LessThan2() {
            //<Snippet2>
            // Add the following directive to your file
            // using Microsoft.Scripting.Ast;

            //This expression represents a LessThan comparison of two arguments using a user defined operator.
            //The parameters to the LessThan should be reference convertible to the MethodInfo's arguments
            Expression MyLessThan = Expression.LessThan(
                Expression.Constant(2),
                Expression.Constant(new Exception("2")),
                false,
                ((Func<int, Exception, bool>)LessThan).Method
            );

            //The end result should be false, 2 is not less than 2:
            Console.WriteLine(Expression.Lambda<Func<bool>>(MyLessThan).Compile().Invoke());
            //</Snippet2>

            //validate sample.
            if (Expression.Lambda<Func<bool>>(MyLessThan).Compile().Invoke() != false) throw new Exception();
        }

    }
}
