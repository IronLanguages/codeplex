using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CGetDelegateType {
        //Delegate(Expression, Expression)
        static public void Delegate1() {
            //<Snippet1>
            // Add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //This method returns the type of a delegate with the supplied number and type of arguments
            var MyDelegate = Expression.GetDelegateType(
                typeof(int),
                typeof(string)
            );

            //We should obtain a delegate type with string return type, and an int argument.
            Console.WriteLine(MyDelegate.ToString());
            //</Snippet1>

            //validate sample.
            if (MyDelegate != typeof(Func<int,string>)) throw new Exception();
        }
    }
}
