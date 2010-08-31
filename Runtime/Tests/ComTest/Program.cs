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

namespace ComTest {

    //
    // Quick note on the meaning of "Positive" "Negative" and "Slow" test
    // prefixes:
    //
    //   Positive: this test throws no exceptions while running. We want these
    //     to run first
    //
    //   Negative: this test intentionally throws an exception while running.
    //     These are run after positive tests so we don't see intentional
    //     exceptions when debugging positive test failures.
    //
    //   Slow: this test takes a while to run because it compiles lots of
    //     trees. For this reason, we want it to run last.
    //
    //   Disabled: the test is disabled. It can still be run manually
    //

    public static partial class Scenarios {
        private static Exception RunTest(MethodInfo test) {
            Console.WriteLine("====== Testing: " + test.Name);
            Exception error = null;
            try {
                Expression expr = (Expression)(test.Invoke(null, new object[0]));
                if (expr != null) {
                    error = ValidateExpression(expr);
                }
            } catch (TargetInvocationException e) {
                error = e.InnerException;
            }

            if (error == null) {
                ColorWriteLine("PASS", ConsoleColor.Green);
            } else {
                ColorWriteLine("FAIL", ConsoleColor.Red);
            }
            return error;
        }


        private static void ColorWriteLine(string msg, ConsoleColor color) {
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            try {
                Console.WriteLine(msg);
            } finally {
                Console.ForegroundColor = oldColor;
            }
        }

         private static Exception ValidateExpression(Expression expr) {
            //verifies that Expression.ToString() does not blow up.
            try {
                expr.ToString();
            } catch (Exception e) {
                return e;
            }
            return null;
        }

        private static int Main(string[] args) {
            if (args.Length == 0) {
                args = new[] { "Positive", "Negative", "Slow" };
            }

            var methods = typeof(Scenarios).GetMethods(BindingFlags.Public | BindingFlags.Static);
            var errors = new List<KeyValuePair<string, Exception>>();

            foreach (string prefix in args) {
                foreach (var test in methods) {
                    if (test.Name.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase)) {
                        var error = RunTest(test);
                        if (error != null) {
                            errors.Add(new KeyValuePair<string, Exception>(test.Name, error));
                        }
                    }
                }
            }

            foreach (var error in errors) {
                Console.WriteLine();
                Console.WriteLine("Test " + error.Key + " threw:");
                Console.WriteLine(error.Value);
            }

            return errors.Count;
        }
    }
}
