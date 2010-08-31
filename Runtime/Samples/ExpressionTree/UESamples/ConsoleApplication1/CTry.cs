using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CTry {
        //TryFinally(Expression, Expression)
        public static void TryFinallySample() {
            //<Snippet7>
            //Add the following directive to your file
            //using Microsoft.Scripting.Ast;

            // Defines a TryExpression with a finally block and no catch statements
            Expression MyTry =
                Expression.TryFinally(
                    Expression.Constant("TryBody"),
                    Expression.Call(typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }), Expression.Constant("Finally"))
                );

            // The returned value is the last expression in the try body
            Console.WriteLine(Expression.Lambda<Func<string>>(MyTry).Compile().Invoke());
            //</Snippet7>

            // validate sample
            if (Expression.Lambda<Func<string>>(MyTry).Compile().Invoke() != "TryBody")
                throw new Exception("TryFinallySample failed");
        }

        // TODO: Bug using Fault in dynamic methods?
        public static void TryFault/*Sample*/() {
            //<Snippet8>
            //Add the following directive to your file
            //using Microsoft.Scripting.Ast;

            // Defines a TryExpression with a fault block and no catch statements
            // Fault blocks are only executed if an Exception is thrown
            Expression MyTry =
                Expression.TryFault(
                    Expression.Constant("TryBody"),
                    Expression.Call(typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }), Expression.Constant("Fault"))
                );

            // The returned value is the last expression in the try body if no exception is thrown.
            Console.WriteLine(Expression.Lambda<Func<string>>(MyTry).Compile().Invoke());
            //</Snippet8>

            // validate sample
            if (Expression.Lambda<Func<string>>(MyTry).Compile().Invoke() != "TryBody")
                throw new Exception("TryFaultSample failed");
        }

        //TryCatchFinally(Expression, Expression, CatchBlock[])
        public static void TryCatchFinallySample() {
            //<Snippet9>
            //Add the following directive to your file
            //using Microsoft.Scripting.Ast;

            // Defines a TryExpression with a finally block and one or more catch statements.
            // The return type of the try and catch blocks (i.e., the last expression in each) must match.
            Expression MyTry =
                Expression.TryCatchFinally(
                    Expression.Block(
                        Expression.Throw(Expression.Constant(new ArgumentException())),
                        Expression.Constant("TryBody")
                    ),
                    Expression.Call(typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }), Expression.Constant("Finally")),
                    new CatchBlock[] {
                        Expression.Catch(typeof(DivideByZeroException), Expression.Constant("CatchBody1")),
                        Expression.Catch(typeof(Exception), Expression.Constant("CatchBody2"))
                    }
                );

            // If a catch statement is entered then the return value is the last expression in that catch statement.
            // Otherwise the return value is the last expression in the try block.
            Console.WriteLine(Expression.Lambda<Func<string>>(MyTry).Compile().Invoke());
            //</Snippet9>

            // validate sample
            if (Expression.Lambda<Func<string>>(MyTry).Compile().Invoke() != "CatchBody2")
                throw new Exception("TryCatchFinallySample failed");
        }

        // TryCatch(Expression, CatchBlock[])
        public static void TryCatchSample() {
            //<Snippet10>
            //Add the following directive to your file
            //using Microsoft.Scripting.Ast;

            // Defines a TryExpression with a finally block and one or more catch statements.
            // The return type of the try and catch blocks (i.e., the last expression in each) must match.
            Expression MyTry =
                Expression.TryCatch(
                    Expression.Block(
                        Expression.Throw(Expression.Constant(new DivideByZeroException())),
                        Expression.Constant("TryBody")
                    ),
                    Expression.Catch(typeof(DivideByZeroException), Expression.Constant("CatchBody"))
                );

            // If a catch statement is entered then the return value is the last expression in that catch statement.
            // Otherwise the return value is the last expression in the try block.
            Console.WriteLine(Expression.Lambda<Func<string>>(MyTry).Compile().Invoke());
            //</Snippet10>

            // validate sample
            if (Expression.Lambda<Func<string>>(MyTry).Compile().Invoke() != "CatchBody")
                throw new Exception("TryCatchSample failed");
        }
    }
}
