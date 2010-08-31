using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class COnesComplement {
        //Expression.OnesComplement(Expression)
        static public void OnesComplementSample1() {
            //<Snippet1>
            //This Expression represents a OnesComplement operation.
            Expression MyOnesComplement = Expression.OnesComplement(
                                        Expression.Constant(5)
                                    );

            //Should print -6.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyOnesComplement).Compile().Invoke());

            //</Snippet1>

            //Validate sample
            if (Expression.Lambda<Func<int>>(MyOnesComplement).Compile().Invoke() != -6) throw new Exception("");
        }

        //<Snippet2>
        public static int OnesComplementMethod(int arg) {
            return ~(arg + 2);
        }
        //</Snippet2>

        //Expression.OnesComplement(Expression, MethodInfo)
        static public void OnesComplementSample2() {
            //<Snippet2>
            //This Expression represents a user defined OnesComplement operation; It will use the specified user defined operation.
            Expression MyOnesComplement = 
                Expression.OnesComplement(
                    Expression.Constant(5),
                    ((Func<int, int>)OnesComplementMethod).Method
                );

            //Should print -8
            Console.WriteLine(Expression.Lambda<Func<int>>(MyOnesComplement).Compile().Invoke());

            //</Snippet2>

            //Validate sample
            if (Expression.Lambda<Func<int>>(MyOnesComplement).Compile().Invoke() != -8) throw new Exception("");
        }
    }
}
