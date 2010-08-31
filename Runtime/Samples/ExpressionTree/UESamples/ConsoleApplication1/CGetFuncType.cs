using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CGetFuncType {
        //Func(Expression, Expression)
        static public void Func1() {
            //<Snippet1>
            // Add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //This method returns the type of a Func delegate with the supplied number and type of arguments
            var MyFunc = Expression.GetFuncType(
                typeof(int),
                typeof(string)
            );

            //We should obtain a delegate type with string return type, and an int argument.
            Console.WriteLine(MyFunc.ToString());
            //</Snippet1>

            //validate sample.
            if (MyFunc != typeof(Func<int,string>)) throw new Exception();
        }
    }
}
