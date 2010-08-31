using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CThrow {
        //Throw(Expression)
        public static void ThrowSample() {
            //<Snippet1>
            ParameterExpression Ex = Expression.Variable(typeof(string), "MyStringException");

            Expression MyTry =
                Expression.TryCatch(
                    Expression.Block(
                        // Defines a UnaryExpression that represents throwing an exception.
                        // Note it can throw non-Exception objects if that type is caught explicitly
                        Expression.Throw(Expression.Constant("AStringException")),
                        Expression.Constant("TryBody")
                    ),
                    Expression.Catch(
                        Ex,
                        Expression.Constant(Ex.Type.ToString()))
                );

            Console.WriteLine(Expression.Lambda<Func<string>>(MyTry).Compile().Invoke());
            //</Snippet1>

            // validate sample
            if (Expression.Lambda<Func<string>>(MyTry).Compile().Invoke() != "System.String")
                throw new Exception("ThrowSample failed");
        }

        //Throw(Expression, Type)
        public static void ThrowSample2() {
            //<Snippet2>
            Expression MyTry =
                Expression.TryCatch(
                    // Defines a UnaryExpression that represents throwing an exception.
                    // The type parameter defines the return type of the Throw.
                    // Without the second argument to Throw compiling this tree would throw an exception
                    // because the return types of the try body and catch would not match.
                    Expression.Throw(Expression.Constant(new DivideByZeroException()), typeof(string)),
                    Expression.Catch(typeof(DivideByZeroException), Expression.Constant("CatchBody"))
                );

            Console.WriteLine(Expression.Lambda<Func<string>>(MyTry).Compile().Invoke());
            //</Snippet2>

            // validate sample
            if (Expression.Lambda<Func<string>>(MyTry).Compile().Invoke() != "CatchBody")
                throw new Exception("ThrowSample2 failed");
        }

        //Rethrow()
        public static void RethrowSample1() {
            //<Snippet3>
            Expression MyTry =
                Expression.TryCatch(
                    Expression.Throw(Expression.Constant(new DivideByZeroException())),
                    Expression.Catch(
                        typeof(DivideByZeroException), 
                        // This expression represents rethrowing an exception.
                        Expression.Rethrow()    
                    )
                );

            // A DivideByZeroException should be thrown.
            try {
                Expression.Lambda<Action>(MyTry).Compile().Invoke();
            } catch (DivideByZeroException) {
                Console.WriteLine("Expected exception thrown");
            }
            //</Snippet3>

            // validate sample
            try {
                Expression.Lambda<Action>(MyTry).Compile().Invoke();
                throw new Exception("Expected DivideByZeroException, no exception thrown.");
            } catch (DivideByZeroException) {
            }
        }

        //Rethrow(Type)
        public static void RethrowSample2() {
            //<Snippet4>
            Expression MyTry =
                Expression.TryCatch(
                    Expression.Block(
                        Expression.Throw(Expression.Constant(new DivideByZeroException())),
                        Expression.Constant("ReturnAString")
                    ),
                    Expression.Catch(
                        typeof(DivideByZeroException),
                        // This expression represents rethrowing an exception.
                        // The type argument sets this expression's Type property to typeof(string).
                        // Without the type argument compiling this tree would throw an exception
                        // because the return types of the try body (string) and catch (void) would not match.
                        Expression.Rethrow(typeof(string))
                    )
                );

            // A DivideByZeroException should be thrown.
            try {
                Expression.Lambda<Action>(MyTry).Compile().Invoke();
            } catch (DivideByZeroException) {
                Console.WriteLine("Expected exception thrown");
            }
            //</Snippet4>

            // validate sample
            try {
                Expression.Lambda<Action>(MyTry).Compile().Invoke();
                throw new Exception("Expected DivideByZeroException, no exception thrown.");
            } catch (DivideByZeroException) {
            }
        }
    }
}
