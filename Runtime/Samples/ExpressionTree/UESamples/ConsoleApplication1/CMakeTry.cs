using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CMakeTry {
        //MakeTry(Type, Expression, Expression, Expression, IEnumerable<CatchBlock>)
        public static void MakeTry1() {
            //<Snippet1>
            //Add the following directive to your file
            //using Microsoft.Scripting.Ast;

            // This expression defines a try statement with a finally block and one or more catch statements.
            // The return type is also defined.
            // One of the finally block or the fault block should be specified.
            Expression MyMakeTry =
                Expression.MakeTry(
                    typeof(string),
                    Expression.Block(
                        Expression.Throw(Expression.Constant(new ArgumentException())),
                        Expression.Constant("MakeTryBody")
                    ),
                    Expression.Call(typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }), Expression.Constant("Finally")),
                    null,
                    new CatchBlock[] {
                        Expression.Catch(typeof(DivideByZeroException), Expression.Constant("CatchBody1")),
                        Expression.Catch(typeof(Exception), Expression.Constant("CatchBody2"))
                    }
                );

            // If a catch statement is entered then the return value is the last expression in that catch statement.
            // Otherwise the return value is the last expression in the MakeTry block.
            Console.WriteLine(Expression.Lambda<Func<string>>(MyMakeTry).Compile().Invoke());
            //</Snippet1>

            // validate sample
            if (Expression.Lambda<Func<string>>(MyMakeTry).Compile().Invoke() != "CatchBody2")
                throw new Exception("MakeTryCatchFinallySample failed");
        }

    }
}
