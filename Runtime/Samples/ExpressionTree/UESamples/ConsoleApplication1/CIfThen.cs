using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CIfThen {
        //Expression.IfThen(MethodInfo, Expression[])
        static public void IfThen1() {
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

            //This element defines an IfThen expression.
            Expression MyIfThen1 = Expression.IfThen(
                Expression.Constant(true),
                Expression.Invoke(
                    MyPrint,
                    Expression.Constant("Ran 1")
                )
            );

            Expression MyIfThen2 = Expression.IfThen(
                Expression.Constant(false),
                Expression.Invoke(
                    MyPrint,
                    Expression.Constant("Ran 2")
                )
            );

            var MyBlock = Expression.Block(
                MyIfThen1,
                MyIfThen2
            );

            //The end result should be "Ran 1".
            Expression.Lambda<Action>(MyBlock).Compile().Invoke();
            //</Snippet1>
        }


    }
}
