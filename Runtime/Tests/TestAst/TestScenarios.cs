/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if !CLR2
using System.Linq.Expressions;
#endif

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using EU = ETUtils.ExpressionUtils;

namespace TestAst {
    /// <summary>
    /// TestScenarios is where all the AST testing is done.  At the high level
    /// this class is used as such:
    /// 
    /// TestScenarios scen = TestScenarios();
    /// Statement stmt = scen.Generate();
    /// //stmt then contains the AST for all of our test cases and is returned to
    /// //TestCompiler.ParseFile() where it is packaged into a LambdaExpression and passed
    /// //back into the DLR for compilation and execution.
    /// 
    /// The final generated code is equivalent to something like this Python...
    /// 
    /// result = 0
    /// 
    /// try:
    ///     print description of test 'a'
    ///     logic for test 'a' here
    ///     print 'pass'
    /// except Exception, e:
    ///     result = result+1
    ///     print "Scenario '%s' failed!" % "a"
    ///     print e
    /// 
    /// try:
    ///     print description of test 'b'
    ///     logic for test 'b' here
    ///     print 'pass'
    /// except Exception, e:
    ///     result = result+1
    ///     print "Scenario '%s' failed!" % "b"
    ///     print e
    /// 
    /// if result==scenarioCount: #where scenarioCount = the total number of scenarios we expect to run
    ///     print "Pass"
    /// else:
    ///     raise Exception("Fail!")
    /// 
    /// Each scenario should throw an exception on failure.  And scenarios should not adversely
    /// affect global state since we're executing them all in the top level scope.
    /// 
    /// This partial class is split into three files:
    /// TestScenario.cs contains constructors and AST generation for the Main of the test
    /// TestScenario.Utils.cs contains helpers methods like Assert and methods to generate certain common AST nodes like print
    /// TestScenario.Tests.cs contains the actual test cases, all new tests should be added there
    /// </summary>
    public partial class TestScenarios {

        private readonly TestContext _tc;

        private static ParameterExpression result = null;
        private static ParameterExpression thrownException = null;

        internal TestScenarios(TestContext tc) {
            if (null == TestScope.Current) {
                throw new ApplicationException("A TestScope must be initialized before Test generation");
            }
            _tc = tc;
            result = TestScope.Current.HiddenVariable(typeof(int), "result");
            thrownException = TestScope.Current.HiddenVariable(typeof(Exception), "thrown_exception");
        }

        internal ActionBinder Binder {
            get {
                return _tc.Binder;
            }
        }

        internal Expression Generate() {
            return Generate(TestContext.TestTypeFlag.Negative | TestContext.TestTypeFlag.Positive);
        }

        internal Expression Generate(TestContext.TestTypeFlag Flags) {

            List<MethodInfo> tests = new List<MethodInfo>();
            string[] args = _tc.TestOptions.Arguments;
            if (args.Length == 0 && _tc.TestOptions.SkipTests.Length == 0 && _tc.TestOptions.RunTests.Length == 0) {
                tests.AddRange(GetAllTests());
            } else if (_tc.TestOptions.RunTests.Length > 0) {
                foreach (string testName in _tc.TestOptions.RunTests) {
                    tests.Add(GetTest(testName));
                }
                if (_tc.TestOptions.SkipTests.Length > 0) {
                    foreach (string testName in _tc.TestOptions.SkipTests) {
                        tests.Remove(GetTest(testName));
                    }
                }
            } else if (_tc.TestOptions.SkipTests.Length > 0) {
                tests.AddRange(GetAllTests());
                foreach (string testName in _tc.TestOptions.SkipTests) {
                    tests.Remove(GetTest(testName));
                }
            } else {
                foreach (string testName in args) {
                    tests.Add(GetTest(testName));
                }
            }

            int ScenarioCount = 0;
            List<Expression> stmts = new List<Expression>();
            stmts.Add(Expression.Assign(result, Expression.Constant(0)));


            List<Expression> MiniLambda = new List<Expression>();
            foreach (MethodInfo mi in tests) {
                int divres = 0;
                Math.DivRem(ScenarioCount, 50, out divres);
                if (divres == 0) {
                    // add previous minilambda to stmts as  a lambda invocation
                    if (MiniLambda.Count > 0) {
                        stmts.Add(MakeNonInlinedLambda(MiniLambda));
                        MiniLambda.Clear();
                    }
                }

                var attr = ETUtils.TestAttribute.GetAttribute(mi);
                if (attr == null)
                    continue;

                // for ignoring tests which need different permissions to run correctly in TestAst (they work in AstTest)
                bool requiresPartialTrust = false;
                foreach (string s in attr.KeyWords) {
                    if (s == "PartialTrustOnly")
                        requiresPartialTrust = true;
                }

                if (TestAttribute.IsEnabled(mi)) {
                    bool isNegative = (attr.KeyWords[0] == "negative") ? true : false;
                    if (((0 != (Flags & TestContext.TestTypeFlag.Negative)) && isNegative) || (0 != (Flags & TestContext.TestTypeFlag.Positive) && !isNegative && !requiresPartialTrust)) {
                        //if no priority is specfied, the default is pri1 (the default for the attribute also.)
                        if (_tc.TestOptions.PriorityOfTests.Length > 0) {
                            bool validPri = false;
                            foreach (int p in _tc.TestOptions.PriorityOfTests) {
                                //if (p == attr.Priority) {
                                if(p == TestAttribute.GetPriority(mi)) {
                                    validPri = true;
                                    break;
                                }
                            }
                            if (!validPri) continue;
                        }
                        
                        ScenarioCount++;
                        Test t = (Test)Delegate.CreateDelegate(typeof(Test), mi);
                        MiniLambda.Add(GenScenario(t));
                    }
                }
            }

            if (MiniLambda.Count > 0) {
                stmts.Add(MakeNonInlinedLambda(MiniLambda));
            }

            //if result==Cases.Length:
            //  print '[Done - PASS]'
            //else:
            //  raise Exception("[Done - FAIL]")
            IfStatementTest test = AstUtils.IfCondition(
                Expression.Equal(
                    Expression.Constant(ScenarioCount),
                    result
                ),
                GenPrint("\n\n[Done - PASS]")
            );

            Expression ifstate = Utils.If(new IfStatementTest[] { test }, GenThrow("[Done - FAIL - This exception only means the test failed, look above for why.]"));

            //stmts.Add(GenPrint("Result = ")); //@TODO - Comment out
            //stmts.Add(GenPrint(new BoundExpression(result))); //@TODO - Comment out
            stmts.Add(ifstate);
            return EU.BlockVoid(stmts);
        }

        // We need to split the tests up into seperate methods
        // Otherwise the size of Initialize causes JIT to run really slow and
        // breaks PEVerify
        //
        // Now that we have inlining, we have to work around it.
        private Expression MakeNonInlinedLambda(List<Expression> block) {
            var lambda = Expression.Parameter(typeof(Action), "lambda");
            return Expression.Block(
                new[] { lambda },
                Expression.Assign(lambda, Expression.Lambda<Action>(Expression.Block(block))),
                Expression.Invoke(lambda)
            );
        }

        private static MethodInfo GetTest(string testName) {
            MethodInfo mi = typeof(TestScenarios).GetMethod(testName, BindingFlags.Static | BindingFlags.NonPublic);

            ContractUtils.Requires(mi != null, "test", "Could not find test: " + testName);
            ContractUtils.Requires(ETUtils.TestAttribute.IsTest(mi), "test", "Method is not a test: " + testName);

            return mi;
        }

        private static MethodInfo[] GetAllTests() {
            return typeof(TestScenarios).GetMethods(BindingFlags.Static | BindingFlags.NonPublic);
        }

        /// <summary>
        /// Generates the AST for one particular test scenario, handling all the try/catch
        /// logic and various sundry so that the scenario code doesn't have to.
        /// </summary>
        /// <param name="name">delegate to the scenario method</param>
        /// <returns></returns>
        private BlockExpression GenScenario(Test testCase) {
            List<Expression> stmts = new List<Expression>();
            try {
                stmts.Add(GenPrint(String.Format("\n[Testing '{0}']", testCase.Method.Name)));
                //@TODO - switch to individual methods per scenario if not too difficult

                //  try {
                //      t()
                //
                //      result ++;
                //      Console.WriteLine("[Pass - 't']");
                //
                //  } catch (Exception thrownException) {
                //      Console.WriteLine(thrownException);
                //      Console.WriteLine("[FAIL - 't'] <<<<");
                //

                Expression testBody = testCase(this);

                stmts.Add(
                    Utils.Try(
                    // test case itself
                        testBody,

                        // result = result+1;
                        GenIncrement(result),
                        GenPrint(String.Format("[Pass - '{0}']", testCase.Method.Name))
                    ).Catch(thrownException,
                        GenPrint(thrownException),
                        GenPrint(String.Format("[FAIL - '{0}'] <<<<<<<<<<<<<<<<<<<<<<<<<<<", testCase.Method.Name))
                    )
                );

                //stmts.Add(GenPrint(new BoundExpression(result))); //remove this
                return EU.BlockVoid(stmts.ToArray());
            } catch (Exception ex) {
                Console.WriteLine("Fatal error generating AST for '{0}'", testCase.Method.Name);
                Console.WriteLine(ex.ToString());
                throw ex;
            }
        }

        private BlockExpression GenScenario(MethodInfo testCase) {
            List<Expression> stmts = new List<Expression>();
            try {
                stmts.Add(GenPrint(String.Format("\n[Testing '{0}']", testCase.Name)));
                //@TODO - switch to individual methods per scenario if not too difficult

                //  try {
                //      t()
                //
                //      result ++;
                //      Console.WriteLine("[Pass - 't']");
                //
                //  } catch (Exception thrownException) {
                //      Console.WriteLine(thrownException);
                //      Console.WriteLine("[FAIL - 't'] <<<<");
                //
                Expression testBody = (Expression)testCase.Invoke(null, new object[] {});

                stmts.Add(
                    Expression.TryCatch(
                        Expression.Block(
                            testBody,
                            GenIncrement(result),
                            GenPrint(String.Format("[Pass - '{0}']", testCase.Name))
                        ),
                        Expression.Catch(
                            thrownException,
                            Expression.Block(
                                GenPrint(thrownException),
                                GenPrint(String.Format("[FAIL - '{0}'] <<<<<<<<<<<<<<<<<<<<<<<<<<<", testCase.Name))
                            )
                        )
                    )
                );
                //stmts.Add(GenPrint(new BoundExpression(result))); //remove this
                return EU.BlockVoid(stmts.ToArray());
            } catch (Exception ex) {
                Console.WriteLine("Fatal error generating AST for '{0}'", testCase.Name);
                Console.WriteLine(ex.ToString());
                throw ex;
            }
        }
    }
}
