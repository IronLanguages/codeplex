using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CCondition {
        //Expression.Condition(Expression, Expression, Expression)
        static public void Condition1() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //This Expression represents a condition operation; it will evaluate the test (first expression)
            //execute the if true block (second argument) if true, or the if false block (third argument) 
            //if the test evaluates to false.
            Expression MyCondition = Expression.Condition(
                                        Expression.Constant(true),
                                        Expression.Constant("Oh yes!"),
                                        Expression.Constant("Oh no!")
                                    );

            //Should print "Oh yes!".
            Console.WriteLine(Expression.Lambda<Func<string>>(MyCondition).Compile().Invoke());

            //</Snippet1>

            //Validate sample
            if (string.Compare(Expression.Lambda<Func<string>>(MyCondition).Compile().Invoke(), "Oh yes!") != 0) throw new Exception("");
        }

        //Expression.Condition(Expression, Expression, Expression, Type)
        static public void Condition2() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //This Expression represents a condition operation; it will evaluate the test (first expression) then
            //execute the ifTrue block (second argument) if true, or the ifFalse block (third argument) 
            //if the test evaluates to false.
            //Type defines the return type of the condition expression, if void, the if and false blocks can have any type,
            //if a type is supplied, then both blocks have to be reference convertible to that type.
            Expression MyCondition = Expression.Condition(
                                        Expression.Constant(true),
                                        Expression.Constant("Oh yes!"),
                                        Expression.Constant("Oh no!"),
                                        typeof(object)
                                    );

            //Should print "Oh yes!".
            Console.WriteLine(Expression.Lambda<Func<object>>(MyCondition).Compile().Invoke());

            //</Snippet1>

            //Validate sample
            if (string.Compare((string)Expression.Lambda<Func<object>>(MyCondition).Compile().Invoke(), "Oh yes!") != 0) throw new Exception("");
        }
    }
}
