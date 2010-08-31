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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace PerfHost {
    public class Program {
        public static int RebindVersion;
        private static TestUtil m_util = new TestUtil();

        class TestInfo {
            public Action<long> Action;
            public long Iterations;

            public TestInfo(Action<long> action, long iterations) {
                Action = action;
                Iterations = iterations;
            }
        }

        static void Main(string[] p_args) {
            RunTests(p_args);
        }

        private static void RunTests(string[] p_args) {
            TestInfo[] testList = new TestInfo[] {
                new TestInfo(Convert1, 1000000),
                new TestInfo(Convert2, 240000000),

                new TestInfo(Lambda1, 1000),
                new TestInfo(Lambda2, 25000000),
                new TestInfo(Lambda3, 10000),
                new TestInfo(Lambda4, 80000),
                new TestInfo(Lambda5, 100000),
                new TestInfo(Lambda6, 100000),
                new TestInfo(Lambda7, 30000000),

                new TestInfo(UnaryPlus1, 250000000),
                 
                new TestInfo(MethodCall1, 100000),
                new TestInfo(MethodCall2, 5000),
                new TestInfo(MethodCall3, 300),
                new TestInfo(MethodCall4, 100000),

                new TestInfo(FieldAccess1, 50000),
                new TestInfo(FieldAccess2, 15000),
                new TestInfo(FieldAccess3, 10000),
                new TestInfo(FieldAccess4, 40000),

                new TestInfo(PropertyAccess1, 40000),
                new TestInfo(PropertyAccess2, 15000),
                new TestInfo(PropertyAccess3, 3000),
                new TestInfo(PropertyAccess4, 20000),
                
                new TestInfo(Misc1, 50000),
                new TestInfo(Misc2, 600),
                new TestInfo(Misc3, 7000000),
            };

            if (p_args.Length == 1 && p_args[0] == "RUN") {
                RunAll(testList);
            } else {
                RunInLab(p_args, testList);
            }
        }

        private static void RunAll(TestInfo[] testList) {
            //run each test once to deal with ngen thingies
            foreach (TestInfo test in testList) {
                test.Action(1);
            }


            Stopwatch sw = new Stopwatch();
            Stopwatch total = new Stopwatch();
            total.Start();
            foreach (TestInfo test in testList) {
                // then run w/ the proper number of iterations
                sw.Start();

                test.Action(test.Iterations);

                sw.Stop();

                Console.WriteLine("{0,-40} {1,-5}ms, {2,5} ms/iter", test.Action.Method.Name, sw.ElapsedMilliseconds, (double)sw.ElapsedMilliseconds / (double)test.Iterations);
                sw.Reset();
            }
            total.Stop();
            Console.WriteLine();
            Console.WriteLine("Total time: {0}ms", total.ElapsedMilliseconds);
        }

        private static void RunInLab(string[] p_args, TestInfo[] testList) {
            foreach (TestInfo test in testList) {
                m_util.AddTest(test.Action.Method.Name);
            }

            string[] args = m_util.ParseArgs(p_args);	// parses out extra args

            long iterations = m_util.GetIterations();	// gets -iters arg
            string testName = m_util.GetTestName();	    // gets –test arg

            if (iterations == 0) {
                Console.WriteLine("To run all tests pass RUN as the only argument.");

                m_util.DumpTestNames();
                return;
            }

            foreach (TestInfo test in testList) {
                if (test.Action.Method.Name == testName) {
                    // run any prep code in the test once...
                    test.Action(0);

                    // then run w/ the proper number of iterations
                    m_util.StartTimer(testName);

                    test.Action(iterations);

                    m_util.StopTimer();

                    m_util.DumpResult();

                    break;
                }
            }
        }

        #region ET scenarios

        #region Conversions

        public static class Delegates {
            public static Func<object> Del1 = Expression.Lambda<Func<object>>(
                    Expression.Condition(
                        Expression.Convert(
                            Expression.GreaterThan(
                                Expression.Add(
                                    Expression.Convert(
                                        Expression.Convert(
                                            Expression.Convert(
                                                Expression.Convert(
                                                    Expression.Convert(
                                                        Expression.Convert(
                                                            Expression.Convert(
                                                                Expression.Convert(
                                                                    Expression.Convert(
                                                                        Expression.Convert(
                                                                            Expression.Convert(
                                                                                Expression.Convert(
                                                                                    Expression.Convert(
                                                                                        Expression.Convert(
                                                                                            Expression.Convert(
                                                                                                Expression.Convert(
                                                                                                    Expression.Convert(
                                                                                                        Expression.Convert(
                                                                                                            Expression.Convert(
                                                                                                                Expression.Convert(
                                                                                                                    Expression.Convert(
                                                                                                                        Expression.Convert(
                                                                                                                            Expression.Convert(
                                                                                                                                Expression.Convert(
                                                                                                                                    Expression.Constant(5),
                                                                                                                                    typeof(object)
                                                                                                                                ),
                                                                                                                                typeof(int)
                                                                                                                            ),
                                                                                                                            typeof(object)
                                                                                                                        ),
                                                                                                                        typeof(int)
                                                                                                                    ),
                                                                                                                    typeof(object)
                                                                                                                ),
                                                                                                                typeof(int)
                                                                                                            ),
                                                                                                            typeof(object)
                                                                                                        ),
                                                                                                        typeof(int)
                                                                                                    ),
                                                                                                    typeof(object)
                                                                                                ),
                                                                                                typeof(int)
                                                                                            ),
                                                                                            typeof(object)
                                                                                        ),
                                                                                        typeof(int)
                                                                                    ),
                                                                                    typeof(object)
                                                                                ),
                                                                                typeof(int)
                                                                            ),
                                                                            typeof(object)
                                                                        ),
                                                                        typeof(int)
                                                                    ),
                                                                    typeof(object)
                                                                ),
                                                                typeof(int)
                                                            ),
                                                            typeof(object)
                                                        ),
                                                        typeof(int)
                                                    ),
                                                    typeof(object)
                                                ),
                                                typeof(int)
                                            ),
                                            typeof(object)
                                        ),
                                        typeof(int)
                                    ),
                                    Expression.Convert(
                                        Expression.Convert(
                                            Expression.Constant(-10),
                                            typeof(object)
                                        ),
                                        typeof(int)
                                    )
                                ),
                                Expression.Constant(0)
                            ),
                            typeof(bool)
                        ),
                        Expression.Convert(Expression.Constant("yes"), typeof(object)),
                        Expression.Convert(Expression.Constant("no"), typeof(object))
                    )
                ).Compile();
        }

        public static void Convert1(long iters) {
            for (long i = 0; i < iters; i++) {
                Delegates.Del1();
                Delegates.Del1();
                Delegates.Del1();
                Delegates.Del1();
                Delegates.Del1();
                Delegates.Del1();
                Delegates.Del1();
                Delegates.Del1();
                Delegates.Del1();
                Delegates.Del1();
            }
        }

        private static Func<int> Convert2Var1 = (Func<int>)Expression.Lambda(
            Expression.Convert(
                Expression.Convert(
                    Expression.Convert(
                        Expression.Convert(
                            Expression.Convert(
                                Expression.Convert(
                                    Expression.Convert(
                                        Expression.Convert(
                                            Expression.Convert(
                                                Expression.Convert(
                                                    Expression.Convert(
                                                        Expression.Convert(
                                                            Expression.Convert(
                                                                Expression.Convert(
                                                                    Expression.Convert(
                                                                        Expression.Convert(
                                                                            Expression.Convert(
                                                                                Expression.Convert(
                                                                                    Expression.Convert(
                                                                                        Expression.Convert(
                                                                                            Expression.Convert(
                                                                                                Expression.Convert(
                                                                                                    Expression.Convert(
                                                                                                        Expression.Convert(
                                                                                                            Expression.Convert(
                                                                                                                Expression.Convert(
                                                                                                                    Expression.Convert(
                                                                                                                        Expression.Convert(
                                                                                                                            Expression.Convert(
                                                                                                                                Expression.Convert(
                                                                                                                                    Expression.Convert(
                                                                                                                                        Expression.Convert(
                                                                                                                                            Expression.Convert(
                                                                                                                                                Expression.Convert(
                                                                                                                                                    Expression.Convert(
                                                                                                                                                        Expression.Convert(
                                                                                                                                                            Expression.Constant(1),
                                                                                                                                                            typeof(int)
                                                                                                                                                        ),
                                                                                                                                                        typeof(int)
                                                                                                                                                    ),
                                                                                                                                                    typeof(int)
                                                                                                                                                ),
                                                                                                                                                typeof(int)
                                                                                                                                            ),
                                                                                                                                            typeof(int)
                                                                                                                                        ),
                                                                                                                                        typeof(int)
                                                                                                                                    ),
                                                                                                                                    typeof(int)
                                                                                                                                ),
                                                                                                                                typeof(int)
                                                                                                                            ),
                                                                                                                            typeof(int)
                                                                                                                        ),
                                                                                                                        typeof(int)
                                                                                                                    ),
                                                                                                                    typeof(int)
                                                                                                                ),
                                                                                                                typeof(int)
                                                                                                            ),
                                                                                                            typeof(int)
                                                                                                        ),
                                                                                                        typeof(int)
                                                                                                    ),
                                                                                                    typeof(int)
                                                                                                ),
                                                                                                typeof(int)
                                                                                            ),
                                                                                            typeof(int)
                                                                                        ),
                                                                                        typeof(int)
                                                                                    ),
                                                                                    typeof(int)
                                                                                ),
                                                                                typeof(int)
                                                                            ),
                                                                            typeof(int)
                                                                        ),
                                                                        typeof(int)
                                                                    ),
                                                                    typeof(int)
                                                                ),
                                                                typeof(int)
                                                            ),
                                                            typeof(int)
                                                        ),
                                                        typeof(int)
                                                    ),
                                                    typeof(int)
                                                ),
                                                typeof(int)
                                            ),
                                            typeof(int)
                                        ),
                                        typeof(int)
                                    ),
                                    typeof(int)
                                ),
                                typeof(int)
                            ),
                            typeof(int)
                        ),
                        typeof(int)
                    ),
                    typeof(int)
                ),
                typeof(int)
            )
        ).Compile();
        public static void Convert2(long iters) {
            for (long i = 0; i < iters; i++) {
                Convert2Var1.Invoke();
            }
        }

        #endregion

        #region lambdas
        //Test empty lambda compilation
        private static LambdaExpression Lambda1Var1 = Expression.Lambda(Expression.Constant(null));
        public static void Lambda1(long iters) {
            for (long i = 0; i < iters; i++) {
                Lambda1Var1.Compile();
                Lambda1Var1.Compile();
                Lambda1Var1.Compile();
                Lambda1Var1.Compile();
                Lambda1Var1.Compile();
                Lambda1Var1.Compile();
                Lambda1Var1.Compile();
                Lambda1Var1.Compile();
                Lambda1Var1.Compile();
                Lambda1Var1.Compile();
            }
        }


        //Lambda invocation
        private static Action Lambda2Var1 = (Action)Expression.Lambda<Action>(Expression.Constant(null)).Compile();
        public static void Lambda2(long iters) {
            for (long i = 0; i < iters; i++) {
                Lambda2Var1.Invoke();
                Lambda2Var1.Invoke();
                Lambda2Var1.Invoke();
                Lambda2Var1.Invoke();
                Lambda2Var1.Invoke();
                Lambda2Var1.Invoke();
                Lambda2Var1.Invoke();
                Lambda2Var1.Invoke();
                Lambda2Var1.Invoke();
                Lambda2Var1.Invoke();
                Lambda2Var1.Invoke();
                Lambda2Var1.Invoke();
            }
        }

        //Lambda Factories
        public static void Lambda3(long iters) {
            Expression Empty = Expression.Constant(null);
            for (long i = 0; i < iters; i++) {
                Expression.Lambda(Empty);
                Expression.Lambda(Empty);
                Expression.Lambda(Empty);
                Expression.Lambda(Empty);
                Expression.Lambda(Empty);
                Expression.Lambda(Empty);
                Expression.Lambda(Empty);
                Expression.Lambda(Empty);
                Expression.Lambda(Empty);
                Expression.Lambda(Empty);
            }
        }

        public static void Lambda4(long iters) {
            Expression Empty = Expression.Constant(null);
            ParameterExpression Param1 = Expression.Parameter(typeof(int), "");
            ParameterExpression Param2 = Expression.Parameter(typeof(int), "");
            for (long i = 0; i < iters; i++) {
                Expression.Lambda(Empty, Param1, Param2);
            }
        }

        public static void Lambda5(long iters) {
            Expression Empty = Expression.Constant(null);
            ParameterExpression Param1 = Expression.Parameter(typeof(int), "");
            ParameterExpression Param2 = Expression.Parameter(typeof(int), "");
            for (long i = 0; i < iters; i++) {
                Expression.Lambda(typeof(Action<int,int>), Empty, new ParameterExpression[] { Param1, Param2 });
                Expression.Lambda(typeof(Action<int, int>), Empty, new ParameterExpression[] { Param1, Param2 });
            }
        }

        public static void Lambda6(long iters) {
            Expression Empty = Expression.Constant(null);
            ParameterExpression Param1 = Expression.Parameter(typeof(int), "");
            ParameterExpression Param2 = Expression.Parameter(typeof(int), "");
            for (long i = 0; i < iters; i++) {
                Expression.Lambda<Action<int, int>>(Empty, Param1, Param2);
                Expression.Lambda<Action<int, int>>(Empty, Param1, Param2);
            }
        }

        public static ParameterExpression Lambda7Parm1 = Expression.Parameter(typeof(int), "");
        public static ParameterExpression Lambda7Parm2 = Expression.Parameter(typeof(int), "");
        public static LambdaExpression Lambda7Var1 = Expression.Lambda<Func<int>>(
            Expression.Invoke(
                Expression.Lambda(Expression.Constant(1), Lambda7Parm1, Lambda7Parm2),
                Expression.Constant(1),
                Expression.Constant(2)
            )
        );
        public static Func<int> Lambda7Var2 = (Func<int>)Lambda7Var1.Compile();

        public static void Lambda7(long iters) {
            for (long i = 0; i < iters; i++) {
                Lambda7Var2.Invoke();
                Lambda7Var2.Invoke();
                Lambda7Var2.Invoke();
                Lambda7Var2.Invoke();
                Lambda7Var2.Invoke();
                Lambda7Var2.Invoke();
                Lambda7Var2.Invoke();
                Lambda7Var2.Invoke();
                Lambda7Var2.Invoke();
                Lambda7Var2.Invoke();
            }
        }


        #endregion

        #region Operators

        private static Func<int> UnaryPlus1Var1 = (Func<int>)Expression.Lambda(
            Expression.UnaryPlus(
                Expression.UnaryPlus(
                    Expression.UnaryPlus(
                        Expression.UnaryPlus(
                            Expression.UnaryPlus(
                                Expression.UnaryPlus(
                                    Expression.UnaryPlus(
                                        Expression.UnaryPlus(
                                            Expression.UnaryPlus(
                                                Expression.UnaryPlus(
                                                    Expression.UnaryPlus(
                                                        Expression.UnaryPlus(
                                                            Expression.UnaryPlus(
                                                                Expression.UnaryPlus(
                                                                    Expression.UnaryPlus(
                                                                        Expression.UnaryPlus(
                                                                            Expression.UnaryPlus(
                                                                                Expression.UnaryPlus(
                                                                                    Expression.UnaryPlus(
                                                                                        Expression.UnaryPlus(
                                                                                            Expression.UnaryPlus(
                                                                                                Expression.UnaryPlus(
                                                                                                    Expression.UnaryPlus(
                                                                                                        Expression.UnaryPlus(
                                                                                                            Expression.UnaryPlus(
                                                                                                                Expression.UnaryPlus(
                                                                                                                    Expression.UnaryPlus(
                                                                                                                        Expression.UnaryPlus(
                                                                                                                            Expression.UnaryPlus(
                                                                                                                                Expression.UnaryPlus(
                                                                                                                                    Expression.UnaryPlus(
                                                                                                                                        Expression.UnaryPlus(
                                                                                                                                            Expression.UnaryPlus(
                                                                                                                                                Expression.UnaryPlus(
                                                                                                                                                    Expression.UnaryPlus(
                                                                                                                                                        Expression.UnaryPlus(
                                                                                                                                                            Expression.UnaryPlus(
                                                                                                                                                                Expression.UnaryPlus(
                                                                                                                                                                    Expression.UnaryPlus(
                                                                                                                                                                        Expression.UnaryPlus(
                                                                                                                                                                            Expression.UnaryPlus(
                                                                                                                                                                                Expression.UnaryPlus(
                                                                                                                                                                                    Expression.UnaryPlus(
                                                                                                                                                                                        Expression.UnaryPlus(
                                                                                                                                                                                            Expression.UnaryPlus(
                                                                                                                                                                                                Expression.UnaryPlus(
                                                                                                                                                                                                    Expression.UnaryPlus(
                                                                                                                                                                                                        Expression.UnaryPlus(
                                                                                                                                                                                                            Expression.Constant(5)
                                                                                                                                                                                                        )
                                                                                                                                                                                                    )
                                                                                                                                                                                                )
                                                                                                                                                                                            )
                                                                                                                                                                                        )
                                                                                                                                                                                    )
                                                                                                                                                                                )
                                                                                                                                                                            )
                                                                                                                                                                        )
                                                                                                                                                                    )
                                                                                                                                                                )
                                                                                                                                                            )
                                                                                                                                                        )
                                                                                                                                                    )
                                                                                                                                                )
                                                                                                                                            )
                                                                                                                                        )
                                                                                                                                    )
                                                                                                                                )
                                                                                                                            )
                                                                                                                        )
                                                                                                                    )
                                                                                                                )
                                                                                                            )
                                                                                                        )
                                                                                                    )
                                                                                                )
                                                                                            )
                                                                                        )
                                                                                    )
                                                                                )
                                                                            )
                                                                        )
                                                                    )
                                                                )
                                                            )
                                                        )
                                                    )
                                                )
                                            )
                                        )
                                    )
                                )
                            )
                        )
                    )
                )
            )
        ).Compile();
        public static void UnaryPlus1(long iters) {
            for (long i = 0; i < iters; i++) {
                UnaryPlus1Var1.Invoke();
            }
        }
        #endregion

        #region Misc
        public static void Misc1(long iters) {
            for (long i = 0; i < iters; i++) {
                Expression.Lambda(
                    Expression.Condition(
                        Expression.NotEqual(
                            Expression.Constant(1.1),
                            Expression.Constant(1.2)
                        ),
                        Expression.Constant(5),
                        Expression.Constant(6)
                    ),
                    Expression.Parameter(typeof(int), ""),
                    Expression.Parameter(typeof(int), "")
                );
            }
        }

        private static ParameterExpression Misc2Param1 = Expression.Parameter(typeof(int), "");
        private static ParameterExpression Misc2Param2 = Expression.Parameter(typeof(int), "");
        private static LambdaExpression Misc2Var1 = Expression.Lambda(
                    Expression.Condition(
                        Expression.NotEqual(
                            Misc2Param1,
                            Misc2Param2
                        ),
                        Expression.Constant(5),
                        Expression.Constant(6)
                    ),
                    Misc2Param1,
                    Misc2Param2
                );
        public static void Misc2(long iters) {
            for (long i = 0; i < iters; i++) {
                Misc2Var1.Compile();
                Misc2Var1.Compile();
                Misc2Var1.Compile();
                Misc2Var1.Compile();
                Misc2Var1.Compile();
                Misc2Var1.Compile();
                Misc2Var1.Compile();
                Misc2Var1.Compile();
                Misc2Var1.Compile();
                Misc2Var1.Compile();
            }
        }

        private static Func<int, int, int> Misc3Var1 = (Func<int, int, int>)Misc2Var1.Compile();
        public static void Misc3(long iters) {
            for (long i = 0; i < iters; i++) {
                Misc3Var1.Invoke(1, 2);
                Misc3Var1.Invoke(1, 1);
                Misc3Var1.Invoke(1, 2);
                Misc3Var1.Invoke(1, 1);
                Misc3Var1.Invoke(1, 2);
                Misc3Var1.Invoke(1, 1);
                Misc3Var1.Invoke(1, 2);
                Misc3Var1.Invoke(1, 1);
                Misc3Var1.Invoke(1, 2);
                Misc3Var1.Invoke(1, 1);
                Misc3Var1.Invoke(1, 2);
                Misc3Var1.Invoke(1, 1);
                Misc3Var1.Invoke(1, 2);
                Misc3Var1.Invoke(1, 1);
                Misc3Var1.Invoke(1, 2);
                Misc3Var1.Invoke(1, 1);
                Misc3Var1.Invoke(1, 2);
                Misc3Var1.Invoke(1, 1);
                Misc3Var1.Invoke(1, 2);
                Misc3Var1.Invoke(1, 1);
                Misc3Var1.Invoke(1, 2);
                Misc3Var1.Invoke(1, 1);
                Misc3Var1.Invoke(1, 2);
                Misc3Var1.Invoke(1, 1);
                Misc3Var1.Invoke(1, 2);
                Misc3Var1.Invoke(1, 1);
                Misc3Var1.Invoke(1, 2);
                Misc3Var1.Invoke(1, 1);
                Misc3Var1.Invoke(1, 2);
                Misc3Var1.Invoke(1, 1);
                Misc3Var1.Invoke(1, 2);
                Misc3Var1.Invoke(1, 1);
                Misc3Var1.Invoke(1, 2);
                Misc3Var1.Invoke(1, 1);
                Misc3Var1.Invoke(1, 2);
                Misc3Var1.Invoke(1, 1);
                Misc3Var1.Invoke(1, 2);
                Misc3Var1.Invoke(1, 1);
                Misc3Var1.Invoke(1, 2);
                Misc3Var1.Invoke(1, 1);
            }
        }
        #endregion

        #region CLR access
        //method call
        public static MethodInfo MethodCall1Var1 = typeof(int).GetMethod("GetType");
        //Factory 1
        public static void MethodCall1(long iters) {
            ConstantExpression Const1 = Expression.Constant(1);
            for (long i = 0; i < iters; i++) {
                Expression.Call(Const1, MethodCall1Var1);
                Expression.Call(Const1, MethodCall1Var1);
                Expression.Call(Const1, MethodCall1Var1);
                Expression.Call(Const1, MethodCall1Var1);
                Expression.Call(Const1, MethodCall1Var1);
                Expression.Call(Const1, MethodCall1Var1);
                Expression.Call(Const1, MethodCall1Var1);
                Expression.Call(Const1, MethodCall1Var1);
                Expression.Call(Const1, MethodCall1Var1);
                Expression.Call(Const1, MethodCall1Var1);
            }
        }

        //Factory 2
        public static void MethodCall2(long iters) {
            ConstantExpression Const1 = Expression.Constant(1);
            for (long i = 0; i < iters; i++) {
                Expression.Call(Const1, "GetType", new Type[] { });
                Expression.Call(Const1, "GetType", new Type[] { });
                Expression.Call(Const1, "GetType", new Type[] { });
                Expression.Call(Const1, "GetType", new Type[] { });
                Expression.Call(Const1, "GetType", new Type[] { });
                Expression.Call(Const1, "GetType", new Type[] { });
                Expression.Call(Const1, "GetType", new Type[] { });
                Expression.Call(Const1, "GetType", new Type[] { });
                Expression.Call(Const1, "GetType", new Type[] { });
                Expression.Call(Const1, "GetType", new Type[] { });
            }
        }

        //Compilation
        public static void MethodCall3(long iters) {
            ConstantExpression Const1 = Expression.Constant(1);
            LambdaExpression expr = Expression.Lambda(Expression.Call(Const1, "GetType", new Type[] { }));
            for (long i = 0; i < iters; i++) {
                expr.Compile();
                expr.Compile();
                expr.Compile();
                expr.Compile();
                expr.Compile();
                expr.Compile();
                expr.Compile();
                expr.Compile();
                expr.Compile();
                expr.Compile();
            }
        }

        //Execution
        public static Action MethodCall4Var1 = Expression.Lambda<Action>(Expression.Call(Expression.Constant(1), "GetType", new Type[] { })).Compile();
        public static void MethodCall4(long iters) {
            for (long i = 0; i < iters; i++) {
                for (int j = 0; j < 300; j++) {
                    MethodCall4Var1.Invoke();
                }
            }
        }

        //field access
        //factories
        public static void FieldAccess1(long iters) {
            FieldInfo Field = typeof(int).GetField("MaxValue");
            for (long i = 0; i < iters; i++) {
                for (int j = 0; j < 500; j++) {
                    Expression.Field(null, Field);
                }
            }
        }

        public class c_FA2 {
            public int x;
        }
        public static ConstantExpression FieldAccess2Var1 = Expression.Constant(new c_FA2());
        public static void FieldAccess2(long iters) {
            for (long i = 0; i < iters; i++) {
                for (int j = 0; j < 100; j++) {
                    Expression.Field(FieldAccess2Var1, "x");
                }
            }
        }

        //compilation
        public static LambdaExpression FieldAccess3Var1 = Expression.Lambda<Action>(
            Expression.Field(null, typeof(int).GetField("MaxValue"))
        );
        public static void FieldAccess3(long iters) {
            for (long i = 0; i < iters; i++) {
                FieldAccess3Var1.Compile();
            }
        }

        //execution
        public static Action FieldAccess4Var1 = (Action)FieldAccess3Var1.Compile();
        public static void FieldAccess4(long iters) {
            for (long i = 0; i < iters; i++) {
                for (int j = 0; j < 10000; j++) {
                    FieldAccess4Var1.Invoke();
                }
            }
        }

        //property access
        //factories
        public static void PropertyAccess1(long iters) {
            PropertyInfo Property = typeof(DateTime).GetProperty("Now");
            for (long i = 0; i < iters; i++) {
                for (int j = 0; j < 500; j++) {
                    Expression.Property(null, Property);
                }
            }
        }

        public class c_PA2 {
            public int x {
                get {
                    return 1;
                }
                set {
                }
            }
        }
        public static ConstantExpression PropertyAccess2Var1 = Expression.Constant(new c_PA2());
        public static void PropertyAccess2(long iters) {
            for (long i = 0; i < iters; i++) {
                for (int j = 0; j < 100; j++) {
                    Expression.Property(PropertyAccess2Var1, "x");
                }
            }
        }

        //compilation
        public static LambdaExpression PropertyAccess3Var1 = Expression.Lambda<Action>(
            Expression.Property(Expression.Constant(new c_PA2()), typeof(c_PA2).GetProperty("x"))
        );
        public static void PropertyAccess3(long iters) {
            for (long i = 0; i < iters; i++) {
                PropertyAccess3Var1.Compile();
            }
        }

        //execution
        public static Action PropertyAccess4Var1 = (Action)PropertyAccess3Var1.Compile();
        public static void PropertyAccess4(long iters) {
            for (long i = 0; i < iters; i++) {
                for (int j = 0; j < 10000; j++) {
                    PropertyAccess4Var1.Invoke();
                }
            }
        }

        #endregion


        #endregion
    }
}
