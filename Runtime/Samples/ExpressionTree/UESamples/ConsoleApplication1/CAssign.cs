using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CAssign {
        //Expression.Assign(Expression,Expression)
        static public void Assign1() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //A variable we will assign to
            ParameterExpression MyVariable = Expression.Variable(typeof(String), "MyVar");
            
            //This Expression represents the assignment of a value.
            //It will copy the value for value types, and
            //Copy a reference for reference types.
            Expression MyAssign = Expression.Assign(MyVariable, Expression.Constant("Hello World!"));

            //We need to wrap the assignment on a block so we can define the variable:
            Expression ABlock = Expression.Block(new ParameterExpression[]{MyVariable},MyAssign);

            //And to check that the variable value should be "Hello World!" we'll wrap the whole thing in
            //a lambda and invoke it.
            Console.WriteLine(Expression.Lambda<Func<string>>(ABlock).Compile().Invoke());

            //</Snippet1>

            //validate sample.
            if (Expression.Lambda<Func<string>>(ABlock).Compile().Invoke().CompareTo("Hello World!")!=0) throw new Exception();
        }
    }
}
