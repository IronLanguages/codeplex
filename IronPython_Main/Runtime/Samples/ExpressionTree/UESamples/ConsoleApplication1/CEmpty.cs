using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CEmpty {
        //Expression.Empty(MethodInfo, Expression[])
        static public void Empty1() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //This element defines an empty expression.
            DefaultExpression MyEmpty = Expression.Empty();

            //It can be used where an expression is expected, but no action is desired.
            var MyEmptyBlock = Expression.Block(MyEmpty);
                
            //</Snippet1>

        }
    }
}
