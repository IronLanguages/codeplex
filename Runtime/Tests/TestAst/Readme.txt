About TestAst
	- TestAst generates, compiles and runs expression trees.
	- General Architecture: 
		- TestAst creates a hosting environment and triggers a compilation event in the hosting environment.
		- This leads to user (TestAst's) code to be called back
			- For positive tests, this code supplies expression trees with the tests to the hosting environment, 
			    which compiles and run them.
			- For negative tests, a second portion of the code executes the tests separately and handles any 
			    exception thrown comparing it with the expected exception (obtained from the TestAttribute 
			    placed on the particular test running).
	- Notes
		- Some differences do happen when executed with the -X:Interpret mode. If you can't repro an issue from a run
			Try adding the switch to the command line.
		- The test accepts the following commands:
			"-pri:1;2;3" : runs the selected priority tests. For example, -pri:1 will only run the pri1 tests. This is the default.
			"-tests:TestName1;TestName2" : runs the selected tests.
			"-skip:TestName1;TestName2" : skips the selected tests. Overrides -tests, -pri.

		
	
	
Adding new tests
	- Create a new Expression tree in the ETScenarios project (see that project's readme for instructions)
	- On TestScenarios.Tests.cs add a stub to call into the ETScenarios method that creates the expression tree.
	-	Stub should have the TestAttribute applied to it, with no arguments if test is supposed to execute correctly, and 
	 	in case of an exception being expected with the exception property set to the expected exception.
	
	-	Search for Test_SimpleIfStatement1 for an example of a positive case, or Test_SimpleIfStatement13 for a negative one.
	-	Note that you can call the method in ETScenario directly, rather than through reflection.