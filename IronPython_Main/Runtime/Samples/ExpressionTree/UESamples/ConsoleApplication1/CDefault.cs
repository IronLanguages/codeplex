using System;
using System.Collections.Generic;
using Microsoft.Scripting.Ast;
using System.Text;

namespace Samples {
    class CDefault {

        //Expression.Default(Type)
        static public void Default1() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //This Expression represents the default value of a type - for example, 0 for integer, null for a string, etc.
            Expression MyDefault = Expression.Default(
                                        typeof(byte)
                                    );

            //Should print 0.
            Console.WriteLine(Expression.Lambda<Func<byte>>(MyDefault).Compile().Invoke());

            //</Snippet1>

            //Validate sample
            if (Expression.Lambda<Func<byte>>(MyDefault).Compile().Invoke() != 0) throw new Exception("");
        }
    }
}
