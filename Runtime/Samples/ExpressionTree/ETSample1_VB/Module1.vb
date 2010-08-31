Imports Microsoft.Scripting.Ast
Imports System.Reflection

Module Module1

    Sub Main(ByVal args As String())
        'In the main method we get an expression tree from one of the samples, 
        'Use that expression as the body of a lambda,
        'Compile and execute the lambda.
        'You can select which sample to run by uncommenting it and commenting all others

        Dim LambdaBody As Expression

        'Samples:
        'LambdaBody = SimpleHelloWorld()
        LambdaBody = NotSoSimpleHelloWorld()

        'Create the lambda
        Dim Lambda As LambdaExpression = Expression.Lambda(LambdaBody)

        'Compile and execute the lambda
        Lambda.Compile().DynamicInvoke()

    End Sub

    ''' <summary>
    ''' This method demonstrates the typical "Hello world" sample using the DLR.
    ''' </summary>
    ''' <returns>An expression that if executed will print "Hello world!"</returns>
    Function SimpleHelloWorld() As Expression
        'First step, we get the methodinfo for Console.WriteLine. This is standard reflection.
        Dim WriteLine As MethodInfo = GetType(Console).GetMethod("WriteLine", New Type() {GetType(String)})
        'Second step, we create an expression that invokes that method with "Hello world!"
        Return Expression.Call(Nothing, WriteLine, Expression.Constant("Hello world!"))
    End Function

    ''' <summary>
    ''' This method is a variation on the hello world sample to demonstrate how to build expression trees to:
    '''     - Define and use variables
    '''     - Use operators
    '''     - Define conditions (in this case to define an if statement)
    '''     - Invoke CLR methods
    ''' </summary>
    Function NotSoSimpleHelloWorld() As Expression
        'Not so simple Hello world

        'This list will hold the expressions that we want to execute.
        Dim Instructions As List(Of Expression) = New List(Of Expression)()

        'Variable that will hold the star name for the current star system.
        'The type ParameterExpression is used both to define parameters and variables.
        'Note that the string passed to the name argument of the Variable method is only used for debugging purposes
        'and isn't validated. you can have multiple variables with the same name, or no name.
        Dim StarName As ParameterExpression = Expression.Parameter(GetType(String), "StarName")

        'Variable that will hold the planet number (from the sun)
        Dim PlanetNumber As ParameterExpression = Expression.Parameter(GetType(String), "PlanetNumber")

        'Prompt the user for the star's name and assign the value to the StarName variable.
        Instructions.Add( _
            Expression.Assign( _
                StarName, _
                Prompt("What is the name of the star you are currently orbiting?") _
                ) _
            )

        'Prompt the user for the planets location and assign the value to the PlanetNumber variable.
        Instructions.Add( _
            Expression.Assign( _
                PlanetNumber, _
                Prompt("What is the number of the planet, counting from the sun, you are currently at?") _
                ) _
            )

        'Are we in the general vicinity of the sun?
        Dim DoWeOrbitTheSun As Expression = Expression.Equal(StarName, Expression.Constant("sun"))

        'are there two other planets closer to the sun than we are?
        Dim AreWeTheThirdPlanet As Expression = Expression.Equal(PlanetNumber, Expression.Constant("3"))

        'Combining the two tests with an And
        Dim Test As Expression = Expression.And(DoWeOrbitTheSun, AreWeTheThirdPlanet)

        'create an if statement using the above defined test.
        'Note that a condition can actually have a value. This factory (Expression.Condition) can also be used
        'to create something equivalent to the ternary ?: operator.
        Dim [If] As Expression = Expression.Condition( _
                Test, _
                Print("Hello Earth!"), _
                Print("Hello random planet!") _
            )

        Instructions.Add([If])

        'And now it all comes together. we create a Block (an expression that holds a list of expressions)
        'and we wrap that in a Scope, which defines the scope of a set of variables.
        Return Expression.Block(New ParameterExpression() {StarName, PlanetNumber}, Expression.Block(Instructions))
    End Function

    Function Print(ByVal arg As String) As Expression
        'First step, we get the methodinfo for Console.WriteLine
        Dim WriteLine As MethodInfo = GetType(Console).GetMethod("WriteLine", New Type() {GetType(String)})
        'Second step, we create an expression that invokes that method with "Hello world!"
        Return Expression.Call(Nothing, WriteLine, Expression.Constant(arg))
    End Function

    ''' <summary>
    ''' Prompts the user, and returns an expression containing the user's reply (lower cased)
    ''' </summary>
    ''' <param name="PromptText">The text to prompt the user with</param>
    ''' <returns>An Expression containing the value the user typed</returns>
    Function Prompt(ByVal PromptText As String) As Expression
        'Prompt the user.
        Console.WriteLine(PromptText)
        'Return the user's input as an expression
        Return Expression.Constant(Console.ReadLine().ToLower())
    End Function

End Module
