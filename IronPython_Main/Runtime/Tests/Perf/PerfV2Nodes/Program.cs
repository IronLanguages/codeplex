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
            var testList = new[] {
                new TestInfo(Loop1,300),
                new TestInfo(Loop2,50),
                new TestInfo(Try1,500),
                new TestInfo(Try2,100000),
                new TestInfo(Try3,500),
                new TestInfo(Try4,100),
                new TestInfo(Try5,500),
                new TestInfo(Try6,100000),
                new TestInfo(Switch1,500),
                new TestInfo(Switch2,20000)
            };

            if (p_args.Length == 1 && p_args[0] == "RUN") {
                RunAll(testList);
            } else {
                RunInLab(p_args, testList);
            }
        }

        private static void RunAll(TestInfo[] testList) {
            //run each test once to deal with ngen thingies
            foreach (var test in testList) {
                test.Action(1);
            }


            Stopwatch sw = new Stopwatch();
            Stopwatch total = new Stopwatch();
            total.Start();
            foreach (var test in testList) {
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
            foreach (var test in testList) {
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

            foreach (var test in testList) {
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

        
#region Loop
        private static LabelTarget LoopLabel1 = Expression.Label();
        private static ParameterExpression LoopIndex1 = Expression.Parameter(typeof(int));
        private static Expression<Action> LoopSample1 = Expression.Lambda<Action>(
            RepeatExpression(
                10,
                Expression.Block(
                    new ParameterExpression[] { LoopIndex1 },
                    Expression.Assign(LoopIndex1,Expression.Constant(0)),
                    Expression.Loop(
                        Expression.Block(
                            Expression.IfThen(Expression.GreaterThan(LoopIndex1, Expression.Constant(10000000)), Expression.Break(LoopLabel1)),
                            Expression.PreIncrementAssign(LoopIndex1)
                        ),                    
                        LoopLabel1
                    )
                )
            )
        );

        
        public static void Loop1(long iters)
        {
            for (int i = 0; i < iters; i++)
            {
                Loop1_1();
            }
        }

        public static void Loop1_1()
        {
            LoopSample1.Compile();
            LoopSample1.Compile();
            LoopSample1.Compile();
            LoopSample1.Compile();
            LoopSample1.Compile();
            LoopSample1.Compile();
            LoopSample1.Compile();
            LoopSample1.Compile();
            LoopSample1.Compile();
            LoopSample1.Compile();
            LoopSample1.Compile();
            LoopSample1.Compile();
            LoopSample1.Compile();
            LoopSample1.Compile();
            LoopSample1.Compile();
            LoopSample1.Compile();
        }


        private static Action LoopCompiled1 = LoopSample1.Compile();
        public static void Loop2(long iters)
        {
            for (int i = 0; i < iters; i++)
            {
                LoopCompiled1.Invoke();
            }
        }
#endregion

#region TryCatch
        private static Expression<Action> TrySample1 = Expression.Lambda<Action>(
            RepeatExpression(
                10000,
                Expression.TryCatch(
                    Expression.Empty(),
                    Expression.Catch(typeof(Exception),Expression.Empty())
                )
            )
        );

        public static void Try1(long iters)
        {
            for (int i = 0; i < iters; i++)
            {
                Try1_1();
            }
        }

        public static void Try1_1()
        {
            TrySample1.Compile();
            TrySample1.Compile();
            TrySample1.Compile();
            TrySample1.Compile();
            TrySample1.Compile();
            TrySample1.Compile();
            TrySample1.Compile();
            TrySample1.Compile();
            TrySample1.Compile();
            TrySample1.Compile();
            TrySample1.Compile();
            TrySample1.Compile();
            TrySample1.Compile();
            TrySample1.Compile();
            TrySample1.Compile();
            TrySample1.Compile();
        }


        private static Action TryCompiled1 = TrySample1.Compile();
        public static void Try2(long iters)
        {
            for (int i = 0; i < iters; i++)
            {
                TryCompiled1.Invoke();
            }
        }



        private static Expression<Action> TrySample2 = Expression.Lambda<Action>(
            RepeatExpression(
                1000,
                Expression.TryCatch(
                    Expression.Throw(Expression.Constant(new Exception())),
                    Expression.Catch(typeof(Exception), Expression.Empty())
                )
            )
        );


        public static void Try3(long iters)
        {
            for (int i = 0; i < iters; i++)
            {
                Try3_1();
            }
        }

        public static void Try3_1()
        {
            TrySample2.Compile();
            TrySample2.Compile();
            TrySample2.Compile();
            TrySample2.Compile();
            TrySample2.Compile();
            TrySample2.Compile();
            TrySample2.Compile();
            TrySample2.Compile();
            TrySample2.Compile();
            TrySample2.Compile();
            TrySample2.Compile();
            TrySample2.Compile();
            TrySample2.Compile();
            TrySample2.Compile();
            TrySample2.Compile();
            TrySample2.Compile();
        }


        private static Action TryCompiled2 = TrySample2.Compile();
        public static void Try4(long iters)
        {
            for (int i = 0; i < iters; i++)
            {
                TryCompiled2.Invoke();
            }
        }



        private static Expression<Action> TrySample3 = Expression.Lambda<Action>(
            RepeatExpression(
                10000,
                Expression.TryCatchFinally(
                    Expression.Empty(),
                    Expression.Empty(),
                    Expression.Catch(typeof(Exception), Expression.Empty())
                )
            )
        );

        public static void Try5(long iters)
        {
            for (int i = 0; i < iters; i++)
            {
                Try5_1();
            }
        }

        public static void Try5_1()
        {
            TrySample3.Compile();
            TrySample3.Compile();
            TrySample3.Compile();
            TrySample3.Compile();
            TrySample3.Compile();
            TrySample3.Compile();
            TrySample3.Compile();
            TrySample3.Compile();
            TrySample3.Compile();
            TrySample3.Compile();
            TrySample3.Compile();
            TrySample3.Compile();
            TrySample3.Compile();
            TrySample3.Compile();
            TrySample3.Compile();
            TrySample3.Compile();
        }


        private static Action TryCompiled3 = TrySample3.Compile();
        public static void Try6(long iters)
        {
            for (int i = 0; i < iters; i++)
            {
                TryCompiled1.Invoke();
            }
        }



        #endregion


        #region Switch
        private static Expression<Action> SwitchSample1 = Expression.Lambda<Action>(
            RepeatExpression(
                100000,
                Expression.Switch(
                    Expression.Constant(1),
                    Expression.SwitchCase(Expression.Empty(),Expression.Constant(0)),
                    Expression.SwitchCase(Expression.Empty(), Expression.Constant(1)),
                    Expression.SwitchCase(Expression.Empty(),Expression.Constant(2))
                )
            )
        );

        public static void Switch1(long iters)
        {
            for (int i = 0; i < iters; i++)
            {
                Switch1_1();
            }
        }

        public static void Switch1_1()
        {
            SwitchSample1.Compile();
            SwitchSample1.Compile();
            SwitchSample1.Compile();
            SwitchSample1.Compile();
            SwitchSample1.Compile();
            SwitchSample1.Compile();
            SwitchSample1.Compile();
            SwitchSample1.Compile();
            SwitchSample1.Compile();
            SwitchSample1.Compile();
            SwitchSample1.Compile();
            SwitchSample1.Compile();
            SwitchSample1.Compile();
            SwitchSample1.Compile();
            SwitchSample1.Compile();
            SwitchSample1.Compile();
        }


        private static Action SwitchCompiled1 = SwitchSample1.Compile();
        public static void Switch2(long iters)
        {
            for (int i = 0; i < iters; i++)
            {
                SwitchCompiled1.Invoke();
            }
        }
        #endregion



        #region ETUtils

        /// <summary>
        /// This method wraps an expression on a loop.
        /// </summary>
        /// <param name="iters"></param>
        /// <param name="Body"></param>
        /// <returns></returns>
        public static Expression RepeatExpression(long iters, Expression Body) {
            return RepeatExpression(iters, Body,1);
        }

        /// <summary>
        /// This method wraps an expression on a loop.
        /// It attempts to duplicate the expression multiple times inside the loop to minimize the actual 
        /// loop time costs. It can therefore affect compilation time.
        /// This can also break code which contains labels or any other expression that should be unique.
        /// </summary>
        /// <param name="iters"></param>
        /// <param name="Body"></param>
        /// <returns></returns>
        public static Expression RepeatExpression(long iters, Expression Body, long DuplicationConstant) {
            long MainIterations;
            long LeftOvers;
                        
            MainIterations = iters/DuplicationConstant;
            LeftOvers = iters - MainIterations * DuplicationConstant ;

            ParameterExpression Counter = Expression.Parameter(typeof(long), "Counter");
            var exprs = new List<Expression>();

            var Test1 = Expression.LessThan(Counter, Expression.Constant(MainIterations));
            LabelTarget Exit1 = Expression.Label();
            BlockExpression Body1;
            
            var exprs1 = new List<Expression>();

            exprs1.Add(Expression.Condition(Test1, Expression.Empty(), Expression.Break(Exit1)));
            exprs1.Add(Expression.AddAssign(Counter, Expression.Constant((long)1)));
            for(int y = 0; y < DuplicationConstant ; y++){
                exprs1.Add(Body);
            }

            
            
            Body1 = Expression.Block(exprs1);

            exprs.Add(Expression.Assign(Counter, Expression.Constant((long)0)));
            exprs.Add(Expression.Loop(Body1,Exit1));

            var Test2 = Expression.LessThan(Counter, Expression.Constant(LeftOvers));
            LabelTarget Exit2 = Expression.Label();
            BlockExpression Body2;
            
            var exprs2 = new List<Expression>();

            exprs2.Add(Expression.Condition(Test2, Expression.Empty(), Expression.Break(Exit2)));
            exprs2.Add(Expression.AddAssign(Counter, Expression.Constant((long)1)));
            exprs2.Add(Body);
            


            Body2 = Expression.Block(exprs2);

            exprs.Add(Expression.Assign(Counter, Expression.Constant((long)0)));
            exprs.Add(Expression.Loop(Body2,Exit2));

            return Expression.Block(new []{Counter},exprs);
        }
        #endregion

        #endregion
    }
}
