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

namespace SiteTest {
    public class SiteTest {
        static int Main(string[] args) {
            if (args.Length > 1) {
                Usage("");
                return -1;
            }
            try {
                SiteTestScenarios tests = new SiteTestScenarios();

                if (args.Length == 0)
                    return tests.RunAll();
                else {
                    if (args[0].ToLower() == "com")
                        return tests.RunCOM();
                    else
                        return tests.RunOne(args[0]);
                }
            } catch (Exception e) {
                Console.WriteLine("Catastrophic failure in SiteTest:");
                Console.WriteLine(e);
                return 1;
            }
        }

        /// <summary>
        /// Prints program usage to the console
        /// </summary>
        private static void Usage(string msg) {
            Console.WriteLine(msg);
            Console.WriteLine(@"
Usage: SiteTest.exe [scenario same]

Running SiteTest with no arguments will run all
available test scenarios.  You may optionally specify
the name of a single scenario to be run.");
        }
    }
}
