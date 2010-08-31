using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CIsTrue {
        //Expression.IsTrue(MethodInfo, Expression[])
        static public void IsTrue1() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //This element defines an IsTrue expression. It searches for an IsTrue operator in the 
            //object supplied.
            Expression MyIsTrue = Expression.IsTrue(
                Expression.Constant(new System.Data.SqlTypes.SqlBoolean(true))
            );

            //The end result should be "True".
            Console.WriteLine(Expression.Lambda<Func<bool>>(MyIsTrue).Compile().Invoke());
            //</Snippet1>

            //Validate sample
            if(Expression.Lambda<Func<bool>>(MyIsTrue).Compile().Invoke()!= true) throw new Exception();

        }

        //Expression.IsTrue(MethodInfo, Expression[])
        //<Snippet2>
        public static bool MethodIsTrue2(int arg) {
            return arg != 0;
        }
        //</Snippet2>
        static public void IsTrue2() {
            //<Snippet2>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //This element defines an IsTrue expression through a user defined operator.
            Expression MyIsTrue = Expression.IsTrue(
                Expression.Constant(0),
                ((Func<int, bool>) MethodIsTrue2).Method
            );

            //The end result should be "False".
            Console.WriteLine(Expression.Lambda<Func<bool>>(MyIsTrue).Compile().Invoke());
            //</Snippet2>

            //Validate sample
            if (Expression.Lambda<Func<bool>>(MyIsTrue).Compile().Invoke() != false) throw new Exception();

        }


    }
}
