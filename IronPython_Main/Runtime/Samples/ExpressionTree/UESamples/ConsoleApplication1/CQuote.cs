using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CQuote {
        //Quote(LabelTarget)
        static public void Quote1() {
            //<Snippet1>
            // Add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //This expression represents an expression that has a value of type expression
            Expression MyQuote = Expression.Quote(
                Expression.Lambda(
                    Expression.Constant(1)
                )
            );

            

            //The end result should be 1:
            Console.WriteLine(Expression.Lambda<Func<LambdaExpression>>(MyQuote).Compile().Invoke().Compile().DynamicInvoke());
            //</Snippet1>

            //validate sample.
            if ((int)Expression.Lambda<Func<LambdaExpression>>(MyQuote).Compile().Invoke().Compile().DynamicInvoke() != 1) throw new Exception();
        }
    }
}
