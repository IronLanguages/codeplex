using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CGreaterThan {
        //GreaterThan(Expression, Expression)
        static public void GreaterThan1() {
            //<Snippet1>
            // Add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //This expression compares the values of its two arguments.
            //Both arguments need to be of the same type.
            Expression MyGreaterThan = Expression.GreaterThan(
                Expression.Constant(42),
                Expression.Constant(45)
            );

            //The end result should be false, 42 is not greater than 45:
            Console.WriteLine(Expression.Lambda<Func<bool>>(MyGreaterThan).Compile().Invoke());
            //</Snippet1>

            //validate sample.
            if (Expression.Lambda<Func<bool>>(MyGreaterThan).Compile().Invoke() != false) throw new Exception();
        }

        //GreaterThan(Expression, Expression, Boolean, MethodInfo)
        //<Snippet2>
        public static bool GreaterThan(int arg1, Exception arg2) {
            return arg1 > Convert.ToInt32(arg2.Message);
        }
        //</Snippet2>


        public static void GreaterThan2() {
            //<Snippet2>
            // Add the following directive to your file
            // using Microsoft.Scripting.Ast;

            //This expression represents a GreaterThan comparison of two arguments using a user defined operator.
            //The parameters to the GreaterThan should be reference convertible to the MethodInfo's arguments
            Expression MyGreaterThan = Expression.GreaterThan(
                Expression.Constant(2),
                Expression.Constant(new Exception("2")),
                false,
                ((Func<int, Exception, bool>)GreaterThan).Method
            );

            //The end result should be false, 2 is not greater than 2:
            Console.WriteLine(Expression.Lambda<Func<bool>>(MyGreaterThan).Compile().Invoke());
            //</Snippet2>

            //validate sample.
            if (Expression.Lambda<Func<bool>>(MyGreaterThan).Compile().Invoke() != false) throw new Exception();
        }

    }
}
