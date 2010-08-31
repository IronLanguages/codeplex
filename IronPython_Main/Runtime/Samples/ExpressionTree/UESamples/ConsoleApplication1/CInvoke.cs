using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CInvoke {
        //Invoke(Expression, Expression[])
        static public void Invoke1() {
            //<Snippet1>
            // Add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //We'll use a parameter in the example.
            var Arg = Expression.Parameter(typeof(int));

            //This expression represents an invoke of a delegate or a lambda.
            Expression MyInvoke = Expression.Invoke(
                Expression.Lambda<Func<int,int>>(
                    Expression.Add(Arg, Expression.Constant(1)),
                    Arg
                ),
                Expression.Constant(41)
            );

            //The end result should be the 41 + 1
            Console.WriteLine(Expression.Lambda<Func<int>>(MyInvoke).Compile().Invoke());
            //</Snippet1>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyInvoke).Compile().Invoke() != 42) throw new Exception();
        }

        //Invoke(Expression, IEnumerable<Expression>)
        static public void Invoke2() {
            //<Snippet1>
            // Add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //We'll use a parameter in the example.
            var Arg1 = Expression.Parameter(typeof(int));
            var Arg2 = Expression.Parameter(typeof(int));
            var ArgList = new List<ParameterExpression>() { Arg1, Arg2 };

            //This expression represents an invoke of a delegate or a lambda.
            Expression MyInvoke = Expression.Invoke(
                Expression.Lambda<Func<int, int, int>>(
                    Expression.Add(
                        Arg1, 
                        Expression.Add(
                            Arg2, 
                            Expression.Constant(1)
                        )
                    ),
                    ArgList
                ),
                Expression.Constant(20),
                Expression.Constant(21)
            );

            //The end result should be the 42
            Console.WriteLine(Expression.Lambda<Func<int>>(MyInvoke).Compile().Invoke());
            //</Snippet1>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyInvoke).Compile().Invoke() != 42) throw new Exception();
        }

    }
}

