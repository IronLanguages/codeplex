using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CIsFalse {
        //Expression.IsFalse(MethodInfo, Expression[])
        static public void IsFalse1() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //This element defines an IsFalse expression. It searches for an IsFalse operator in the 
            //object supplied.
            Expression MyIsFalse = Expression.IsFalse(
                Expression.Constant(new System.Data.SqlTypes.SqlBoolean(true))
            );

            //The end result should be "False".
            Console.WriteLine(Expression.Lambda<Func<bool>>(MyIsFalse).Compile().Invoke());
            //</Snippet1>

            //Validate sample
            if(Expression.Lambda<Func<bool>>(MyIsFalse).Compile().Invoke()!= false) throw new Exception();

        }

        //Expression.IsFalse(MethodInfo, Expression[])
        //<Snippet2>
        public static bool MethodIsFalse2(int arg) {
            return arg == 0;
        }
        //</Snippet2>
        static public void IsFalse2() {
            //<Snippet2>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //This element defines an IsFalse expression through a user defined operator.
            Expression MyIsFalse = Expression.IsFalse(
                Expression.Constant(0),
                ((Func<int, bool>) MethodIsFalse2).Method
            );

            //The end result should be "True".
            Console.WriteLine(Expression.Lambda<Func<bool>>(MyIsFalse).Compile().Invoke());
            //</Snippet2>

            //Validate sample
            if (Expression.Lambda<Func<bool>>(MyIsFalse).Compile().Invoke() != true) throw new Exception();

        }


    }
}
