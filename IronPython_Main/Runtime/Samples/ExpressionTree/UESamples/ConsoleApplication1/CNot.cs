using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CNot {
        //Expression.Not(Expression)
        static public void NotSample1() {
            //<Snippet1>
            //This Expression represents a Not operation.
            Expression MyNot = Expression.Not(Expression.Constant(false));

            //Should print true.
            Console.WriteLine(Expression.Lambda<Func<bool>>(MyNot).Compile().Invoke());

            //</Snippet1>

            //Validate sample
            if (Expression.Lambda<Func<bool>>(MyNot).Compile().Invoke() != true) throw new Exception("");
        }

        //<Snippet2>
        public static bool NotMethod(int arg) {
            return (arg > 0) ? true : false;
        }
        //</Snippet2>

        //Expression.Not(Expression, MethodInfo)
        static public void NotSample2() {
            //<Snippet2>
            //This Expression represents a user defined Not operation; It will use the specified user defined operation.
            Expression MyNot =
                Expression.Not(
                    Expression.Constant(-1),
                    ((Func<int, bool>)NotMethod).Method
                );

            //Should print false because the argument was < 0.
            Console.WriteLine(Expression.Lambda<Func<bool>>(MyNot).Compile().Invoke());

            //</Snippet2>

            //Validate sample
            if (Expression.Lambda<Func<bool>>(MyNot).Compile().Invoke() != false) throw new Exception("");
        }
    }
}
