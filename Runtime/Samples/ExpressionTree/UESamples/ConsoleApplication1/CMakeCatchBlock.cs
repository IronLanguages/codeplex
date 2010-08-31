using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples
{
    class CMakeCatchBlock
    {

        //MakeCatchBlock(Type, ParameterExpression, Expression, Expression)
        static public void MakeCatchBlock1()
        {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //We'll use a ParameterExpression to hold the exception being caught
            ParameterExpression MyException = Expression.Parameter(typeof(ArgumentException), "MyException");

            //This Expression represents a catch block of a try statement.
            //The block's body is simply a string that lets us know an exception was caught.
            //We also provide a filter - this exception will not be caught by this handler unless the filter evaluates to true
            CatchBlock MyMakeCatchBlock = Expression.MakeCatchBlock(
                typeof(ArgumentException),
                MyException,
                Expression.Constant("Exception was caught"),
                Expression.Equal(
                    Expression.Constant(1),
                    Expression.Constant(1)
                )
            );

            //We use the catch in a Try Expression.
            //This Try Expression has the type of string.
            Expression MyTry = Expression.TryCatch(
                Expression.Throw(Expression.New(typeof(ArgumentException).GetConstructor(new Type[] { })), typeof(string)),
                MyMakeCatchBlock
            );

            //Exception was caught?
            Console.WriteLine(Expression.Lambda<Func<string>>(MyTry).Compile().Invoke());

            //</Snippet1>

            //validate sample.
            if (String.Compare(Expression.Lambda<Func<string>>(MyTry).Compile().Invoke(), "Exception was caught") != 0) throw new Exception();
        }

    }
}
