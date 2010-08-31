using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples
{
    class CCoalesce
    {
        //Expression.Coalesce(Expression, Expression)
        static public void Coalesce1() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //This Expression represents a coalesce operation; it will return the first argument
            //if it is non null, or the second argument if the first one is null.
            Expression MyCoalesce = Expression.Coalesce(
                                        Expression.Constant(1, typeof(int?)),
                                        Expression.Constant(2, typeof(int?))
                                    );

            //Should print 1.
            Console.WriteLine(Expression.Lambda<Func<int?>>(MyCoalesce).Compile().Invoke());

            //</Snippet1>

            //Validate sample
            if (Expression.Lambda<Func<int?>>(MyCoalesce).Compile().Invoke() != 1) throw new Exception("");
        }

        //Expression.Coalesce(Expression, Expression, LambdaExpression)
        static public void Coalesce2() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;

            ParameterExpression X = Expression.Parameter(typeof(int?));

            //This Expression represents a coalesce operation; it will return the first argument
            //if it is non null, or the second argument if the first one is null.
            //We also provide a lambda expression that will convert the first argument to the type of the second
            //so coalesce has a consistent return type.
            Expression MyCoalesce = Expression.Coalesce(
                                        Expression.Constant(null, typeof(int?)),
                                        Expression.Constant(2.0, typeof(double)),
                                        Expression.Lambda<Func<int?,double>>(
                                            Expression.Convert(X,typeof(double)),
                                            X
                                        )
                                    );

            //Should print 2.
            Console.WriteLine(Expression.Lambda<Func<double>>(MyCoalesce).Compile().Invoke());

            //</Snippet1>

            //Validate sample
            if (Expression.Lambda<Func<double>>(MyCoalesce).Compile().Invoke() != 2) throw new Exception("");
        }
    }
}
