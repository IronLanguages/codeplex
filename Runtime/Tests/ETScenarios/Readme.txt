About ETScenarios
	- ETScenarios is a container for expression tree tests. Tests Are marked with the TestAttribute (defined in ETUtils).
	
To Create a Test
	- If test is part of the Expression Trees testplan, location should be under a folder for the particular testplan 
		it was defined in, and in a source file for the type of factory that it relates the most to. For example,
		tests for the Add factory are found under the folder Operators, in a file named Add.cs
	- If test is not part of the Expression Trees tesplan, use your judgement. If it applies to a factory, find the 
		appropriate file it would fall into if it belonged to the testplan. If not, place it under Miscellaneous, 
		potentially under a sub folder.
	- The test:
		- Tests are basically methods that return the expression tree to be tested, or throw trying. (the only way of 
			failure is to throw an exception, but the exception can happen after the method returns while the expression
			tree is compiled or executed).
		- Tests need to be marked with the TestAttribute from the ETUtils project.
			- Each test needs to have a unique description, which acts as its ID.
			- Optionally a test's attribute can mark the test as enabled or disabled.
			- If a test is negative, the attribute's Exception property should be set to the type of the expected exception.
			- The attribute should have a list of tags that identify the test's focus areas. For example, a test focusing 
				on the Add factory should have an "add" tag.
			- Two standard tags should be added to the list regardless: "negative"/"positive" and "Pri1"/2/3.
			- Optionally, the priority property on the attribute can be set. Default is 1.
		- Tests should return an expression to be tested.
			- Note that if the test is being invoked through TestAst, it will be inserted in a lambda, compiled and executed
				unless an exception happens. In that case, the exception will be compared to the exception defined in the 
				TestAst Stub that obtains the ETScenarios method, not the exception defined in TestAst. This is due the 
				architecture of TestAst. Other test handlers may use the exception defined in ETScenarios directly (AstTest 
				will in the future), so keeping the exceptions updated is essencial.
			- It's a good practice to ensure that if an exception is expected during the creation of the tree, that
				it cannot be masked by the same exception being thrown at compile or execution time. For that purpose,
				return Expression.Empty() when your test should throw before returning the expression.
			- ETUtils contains several helper functions that should simplify the test writing work:
				- GenAreEqual returns an expression that compares two values, throws an exception if they are different.
				- GenThrow returns an expression that throws an exception.
				- GenConcat returns an expression that concatenates strings or a string to a var
				- ConcatEquals returns an expression that concatenates a string to a variable and assigns the result to 
					the variable. It's quite handy to track execution paths.
				- GetExprType returns an expression that obtains the System.Type of an expression's value.
			- Tricks of the business
				- Commas: Commas allow you to do an expression and still have a value resulting from the expression.
					They are frequently used to track execution path: the first expression in a comma is a ConcatEquals 
					expression, and the second one is the value one passes to an operator. The concatenations happen as 
					the arguments to the operator are evaluated, so the resulting string can be compared with a string 
					representing the expected sequence. See "If14" for an example.
					While this could be achieved with lambdas, it's more practical with the commas.
				

			
	