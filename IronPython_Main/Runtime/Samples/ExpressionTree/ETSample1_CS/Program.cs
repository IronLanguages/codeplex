using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Microsoft.Scripting.Ast;

namespace ETSample1_CS {
    class Program {
        static void Main(string[] args) {
            //In the main method we get an expression tree from one of the samples, 
            //Use that expression as the body of a lambda,
            //Compile and execute the lambda.
            //You can select which sample to run by uncommenting it and commenting all others

            Expression LambdaBody;

            //Samples:
            //LambdaBody = SimpleHelloWorld();
            LambdaBody = NotSoSimpleHelloWorld();
            
            //Create the lambda
            LambdaExpression Lambda = Expression.Lambda(LambdaBody);

            //Compile and execute the lambda
            Lambda.Compile().DynamicInvoke();

        }

        /// <summary>
        /// This method demonstrates the typical "Hello world" sample using the DLR.
        /// </summary>
        /// <returns>An expression that if executed will print "Hello world!"</returns>
        static Expression SimpleHelloWorld() {
            //First step, we get the methodinfo for Console.WriteLine. This is standard reflection.
            MethodInfo WriteLine = typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) });
            //Second step, we create an expression that invokes that method with "Hello world!"
            return Expression.Call(null, WriteLine, Expression.Constant("Hello world!"));
        }

        /// <summary>
        /// This method is a variation on the hello world sample to demonstrate how to build expression trees to:
        ///     - Define and use variables
        ///     - Use operators
        ///     - Define conditions (in this case to define an if statement)
        ///     - Invoke CLR methods
        /// </summary>
        static Expression NotSoSimpleHelloWorld() {
            //Not so simple Hello world

            //This list will hold the expressions that we want to execute.
            List<Expression> Instructions = new List<Expression>();

            //Variable that will hold the star name for the current star system.
            //The type ParameterExpression is used both to define parameters and variables.
            //Note that the string passed to the name argument of the Variable method is only used for debugging purposes
            //and isn't validated. you can have multiple variables with the same name, or no name.
            ParameterExpression StarName = Expression.Parameter(typeof(String), "StarName");

            //Variable that will hold the planet number (from the sun)
            ParameterExpression PlanetNumber = Expression.Parameter(typeof(String), "PlanetNumber");

            //Prompt the user for the star's name and assign the value to the StarName variable.
            Instructions.Add(
                Expression.Assign(
                    StarName,
                    Prompt("What is the name of the star you are currently orbiting?")
                    )
                );

            //Prompt the user for the planets location and assign the value to the PlanetNumber variable.
            Instructions.Add(
                Expression.Assign(
                    PlanetNumber,
                    Prompt("What is the numer of the planet, counting from the sun, you are currently at?")
                    )
                );

            //Are we in the general vicinity of the sun?
            Expression DoWeOrbitTheSun = Expression.Equal(StarName, Expression.Constant("sun"));

            //are there two other planets closer to the sun than we are?
            Expression AreWeTheThirdPlanet = Expression.Equal(PlanetNumber, Expression.Constant("3"));

            //Combining the two tests with an And
            Expression Test = Expression.And(DoWeOrbitTheSun, AreWeTheThirdPlanet);

            //create an if statement using the above defined test.
            //Note that a condition can actually have a value. This factory (Expression.Condition) can also be used
            //to create something equivalent to the ternary ?: operator.
            Expression If = Expression.Condition(
                    Test,
                    Print("Hello Earth!"),
                    Print("Hello random planet!")
                );

            Instructions.Add(If);

            //And now it all comes together. we create a Block (an expression that holds a list of expressions)
            //and we wrap that in a Scope, which defines the scope of a set of variables.
            return Expression.Block(new ParameterExpression[] { StarName, PlanetNumber }, Expression.Block(Instructions));
        }

        static Expression Print(String arg) {
            //First step, we get the methodinfo for Console.WriteLine
            MethodInfo WriteLine = typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) });
            //Second step, we create an expression that invokes that method with "Hello world!"
            return Expression.Call(null, WriteLine, Expression.Constant(arg));
        }

        /// <summary>
        /// Prompts the user, and returns an expression containing the user's reply (lower cased)
        /// </summary>
        /// <param name="PromptText">The text to prompt the user with</param>
        /// <returns>An Expression containing the value the user typed</returns>
        static Expression Prompt(String PromptText) {
            //Prompt the user.
            Console.WriteLine(PromptText);
            //Return the user's input as an expression
            return Expression.Constant(Console.ReadLine().ToLower());
        }

    }
}
