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
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HostingTest;
using System.Reflection;

namespace HostingTestRunner {
    class NoExceptionException : Exception {
    }

    /// <summary>
    /// Provides a simple runner for the hosting tests.  Can be run stand alone and will run all
    /// of the tests in the HostingTest project.  On the command line you can pass a list of
    /// tests to be run and all other tests will be skipped.  The test name is the type name w/o
    /// namespace + . + the method name. For example ScriptScopeTest.GetItems_RemoteAD.
    /// </summary>
    class Program {
        static void Main(string[] args) {
            Type[] testTypes = typeof(ScriptHostTest).Assembly.GetExportedTypes();
            int failCount = 0;
            foreach (Type t in testTypes) {
                if(t.IsDefined(typeof(TestClassAttribute), false)) {
                    if (t.GetConstructor(Type.EmptyTypes) != null) {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine(t.FullName);
                        object o = Activator.CreateInstance(t);

                        foreach (MethodInfo mi in t.GetMethods()) {
                            if (mi.IsDefined(typeof(TestMethodAttribute), false) && !mi.IsDefined(typeof(IgnoreAttribute), false)) {
                                if (ExecuteOneTest(args, o, mi)) {
                                    failCount++;
                                }
                            }
                        }
                    }
                }
            }

            if (failCount == 0) {
                Console.ForegroundColor = ConsoleColor.Green;
            } else {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            Console.WriteLine();
            Console.WriteLine("{0} failures", failCount);
        }

        private static bool ExecuteOneTest(string[] args, object o, MethodInfo mi) {
            bool failed = false;
            Console.ForegroundColor = ConsoleColor.Gray;
            string testName = mi.DeclaringType.Name + "." + mi.Name;
            Console.Write("Executing {0,-80}: ", testName);

            if (args.Length > 0 && !args.Contains(testName)) {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("SKIPPING");
                return false;
            }

            ExpectedExceptionAttribute[] expected = (ExpectedExceptionAttribute[])mi.GetCustomAttributes(typeof(ExpectedExceptionAttribute), true);
            try {
                mi.Invoke(o, Type.EmptyTypes);
                if (expected.Length != 0) {
                    throw new NoExceptionException();
                }
            } catch (TargetInvocationException e) {
                bool found = false;
                foreach (ExpectedExceptionAttribute expectedEx in expected) {
                    if (expectedEx.ExceptionType.IsInstanceOfType(e.InnerException)) {
                        found = true;
                    }
                }

                if (!found) {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Unexpected exception: {0}", e.InnerException.Message);
                    Console.WriteLine("Stack Trace: {0}", e.InnerException.StackTrace);
                    failed = true;
                }
            } catch (NoExceptionException) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Expected exception but didn't get one");
                failed = true;
            }

            if (!failed) {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("PASS");
            }
            return failed;
        }
    }
}
