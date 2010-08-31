using System;
using System.Collections.Generic;
using Microsoft.Scripting.Ast;
using System.Text;

namespace Samples {
    class CIfThenElse {
        //Expression.IfThenElse(MethodInfo, Expression[])
        static public void IfThenElse1() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //We'll define a helper lambda to print out some information
            ParameterExpression Text = Expression.Parameter(typeof(string));
            var MyPrint = Expression.Lambda<Action<string>>(
                Expression.Call(
                    null,
                    typeof(Console).GetMethod("WriteLine",new Type[]{typeof(string)}),
                    Text
                ),
                Text
            );

            //This element defines an IfThenElse expression.
            Expression MyIfThenElse1 = Expression.IfThenElse(
                Expression.Constant(true),
                Expression.Invoke(
                    MyPrint,
                    Expression.Constant("Ran 1")
                ),
                Expression.Invoke(
                    MyPrint,
                    Expression.Constant("Ran 2")
                )
            );

            Expression MyIfThenElse2 = Expression.IfThenElse(
                Expression.Constant(false),
                Expression.Invoke(
                    MyPrint,
                    Expression.Constant("Ran 3")
                ),
                Expression.Invoke(
                    MyPrint,
                    Expression.Constant("Ran 4")
                )
            );

            var MyBlock = Expression.Block(
                MyIfThenElse1,
                MyIfThenElse2
            );

            //The end result should be "Ran 1" \n "Ran 4".
            Expression.Lambda<Action>(MyBlock).Compile().Invoke();
            //</Snippet1>
        }


    }
}
