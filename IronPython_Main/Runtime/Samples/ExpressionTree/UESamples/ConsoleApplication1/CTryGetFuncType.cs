using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CTryGetFuncType {
        //Func(Expression, Expression)
        static public void Func1() {
            //<Snippet1>
            // Add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            
            Type MyFunc;

            //This method sets the second argument to the type of a Func delegate with the supplied number and type of arguments.
            //It returns whether it succeeded or not.
            var Result= Expression.TryGetFuncType(
                new Type[]{
                    typeof(int),
                    typeof(string)
                },
                out MyFunc
            );

            //We should obtain a delegate type with string return type, and an int argument.
            Console.WriteLine(MyFunc.ToString());
            //</Snippet1>

            //validate sample.
            if (MyFunc != typeof(Func<int,string>)) throw new Exception();
        }
    }
}
