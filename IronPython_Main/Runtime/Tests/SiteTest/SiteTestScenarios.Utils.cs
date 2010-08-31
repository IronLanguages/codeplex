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
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace SiteTest {
    partial class SiteTestScenarios {
        public delegate void Function();

        private static void AssertExceptionThrown<T>(Function f) where T : Exception {
            try {
                f();
            } catch (Exception e) {
                var t = e as T;
                if (t != null) {
                    return;
                }
                Assert.Fail("Expecting exception '{0}', {1} has been thrown instead: {2}", typeof(T), e.GetType(), e.Message);
            }
            Assert.Fail("Expecting exception '{0}', none thrown.", typeof(T));
        }

        private static void AssertExceptionThrown<T>(Function f, string expectedMessage) where T : Exception {
            try {
                f();
            } catch (Exception e) {
                var t = e as T;
                if (t != null) {
                    if (System.Threading.Thread.CurrentThread.CurrentUICulture.Name == "en-US") {
                        if (e.Message != expectedMessage)
                            Assert.Fail("Expected exception message '{0}', received '{1}' instead", expectedMessage, e.Message);
                    }

                    return; //Pass
                }
                Assert.Fail("Expecting exception '{0}', {1} has been thrown instead: {2}", typeof(T), e.GetType(), e.Message);
            }
            Assert.Fail("Expecting exception '{0}', none thrown.", typeof(T));
        }

        /// <summary>
        /// Generates a very basic dynamic method that
        /// can be invoked via a delegate of type T.
        /// </summary>
        /// <typeparam name="T">The delegate type to create a method for</typeparam>
        /// <param name="name">The name of the generated method</param>
        /// <returns></returns>
        private T CreateSimpleTarget<T>(string name) where T : class {
            //T should be a delegate type.  Pull out its Invoke method
            Type targetType = typeof(T);
            MethodInfo mi = targetType.GetMethod("Invoke");

            //Our new method should have the same signature as Invoke
            //So pull out all the argument types into an array suitable
            //for the DynamicMethod constructor.
            List<Type> argTypes = new List<Type>();
            foreach (ParameterInfo pi in mi.GetParameters())
                argTypes.Add(pi.ParameterType);

            //Create a new dynamic method
            DynamicMethod target = new DynamicMethod(name, mi.ReturnType, argTypes.ToArray(), typeof(SiteTest));
            ILGenerator il = target.GetILGenerator();
            //il.Emit(... @TODO - Ideally this would validate the arguments, return something interesting, and log an event in SiteLog

            //Finally generate the delegate to be used
            return target.CreateDelegate(typeof(T)) as T;
        }

        /// <summary>
        /// Writes the given text in the given colors.
        /// Helper method to beautify the output.
        /// </summary>
        /// <param name="foreground"></param>
        /// <param name="background"></param>
        /// <param name="format"></param>
        /// <param name="arg"></param>
        private void WriteLine(ConsoleColor foreground, ConsoleColor background, string format, params object[] arg) {
            lock (this) {
                try {
                    Console.ForegroundColor = foreground;
                    Console.BackgroundColor = background;
                    Console.WriteLine(format, arg);
                } finally {
                    Console.ResetColor();
                }
            }
        }

        /// <summary>
        /// Factory class for creating independently toggleable tests for rules
        /// </summary>
        static class RuleTestDispenser {
            private static int currentId=0;

            /// <summary>
            /// Dictionary of test ids to the desired result of invocation of the test 
            /// </summary>
            public static Dictionary<int, bool> Test = new Dictionary<int, bool>();

            /// <summary>
            /// Creates an expression usable as a RuleBuilder.Test.  This rule returns
            /// the contents of RuleTestDispenser.Rule[id], which defaults to True but is
            /// settable.  The rule also logs a TestInvocation event with the given
            /// eventDescr when invoked.
            /// </summary>
            /// <param name="scen"></param>
            /// <param name="eventDescr"></param>
            /// <param name="id"></param>
            /// <returns></returns>
            public static Expression Create(SiteTestScenarios scen, string eventDescr, out int id) {
                id = currentId++;
                Test.Add(id, true);
                Expression test = Expression.Block(scen._log.GenLog(SiteLog.EventType.TestInvocation, eventDescr),
                    Expression.Call(Expression.Constant(Test, typeof(Dictionary<int,bool>)),
                        typeof(Dictionary<int,bool>).GetMethod("get_Item"), Expression.Constant(id))
                );
                return test;
            }
        }

        /// <summary>
        /// Uses RuleTestDispenser to create a simple
        /// Rule whose test is controlled by the dispenser
        /// and whose target 
        /// </summary>
        /// <param name="testDescr"></param>
        /// <param name="targetDescr"></param>
        /// <returns></returns>
        internal RuleBuilder CreateLoggingRule(string testDescr, string targetDescr, out int id) {
            int ruleId;
            Expression test = RuleTestDispenser.Create(this, testDescr, out ruleId);
            RuleBuilder rule = (p, r) =>
                Expression.Condition(
                    Expression.Block(test, Expression.Constant(true)),
                    Expression.Return(
                        r,
                        Expression.Convert(
                            _log.GenLog(SiteLog.EventType.TargetInvocation, Expression.Constant(targetDescr)),
                            typeof(object)
                        )
                    ),
                    Expression.Empty()
                );

            id = ruleId;
            return rule;
        }

        private static void ClearRuleCache<T>(CallSite<T> site) where T : class {
            Type st = typeof(CallSite<T>);
            MethodInfo clear = st.GetMethod("ClearRuleCache", BindingFlags.Instance | BindingFlags.NonPublic);
            clear.Invoke(site, new object[0]);
        }
    }
}
