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
using System.Reflection;

namespace SiteTest {
    /// <summary>
    /// SiteTestScenarios contains all the Dynamic Sites test cases.
    /// 
    /// It is split into three files.  This file contains the initialization
    /// logic as well as public APIs to run one or all test cases.
    /// 
    /// SiteTestScenarios.Tests.cs contains the actual test scenarios
    /// themselves.
    /// 
    /// SiteTestScenarios.Utils.cs contains some private helper functions to be
    /// used by the test scenarios.
    /// </summary>
    partial class SiteTestScenarios {
        #region Public helper methods
        public SiteTestScenarios() {
            _sitebinder = new SiteBinder();
            _log = SiteBinder.Log;
        }
        
        /// <summary>
        /// Runs one test scenario
        /// </summary>
        /// <param name="mi">MethodInfo of the test to be run</param>
        /// <returns></returns>
        public int RunOne(MethodInfo mi) {
            if (!TestAttribute.IsTest(mi))
                throw new ArgumentException("Specified method is not a test.");

            //Reset our global state first
            _log.Reset();
            _sitebinder.SetRules();

            //Now launch the test with some nicely formatted diagnostic output
            try {
                WriteLine(ConsoleColor.Cyan, ConsoleColor.Black, "\n[Testing - {0}]", mi.Name);
                mi.Invoke(this, null);
                if(_log.Length>0)
                    Console.WriteLine(Environment.NewLine + _log.ToPrintableString());
                WriteLine(ConsoleColor.Green, ConsoleColor.Black, "[Pass - {0}]", mi.Name);
                return 0;
            } catch (Exception e) {
                if (e is TargetInvocationException)
                    Console.WriteLine(e.InnerException);
                else
                    Console.WriteLine(e);

                //if(_log.Length > 0)
                //    Console.WriteLine(Environment.NewLine + _log.ToPrintableString());
                WriteLine(ConsoleColor.Black, ConsoleColor.Red, "\n[FAIL - '{0}'] <<<<<<<<<<<<<<<<<<<<<<<<<<<", mi.Name);
                return 1;
            }
        }

        /// <summary>
        /// Runs one test scenario
        /// </summary>
        /// <param name="scenario">Name of the method containing the scenario</param>
        /// <returns></returns>
        public int RunOne(string scenario) {
            MethodInfo minfo = typeof(SiteTestScenarios).GetMethod(scenario, BindingFlags.Instance | BindingFlags.NonPublic);
            if (minfo == null)
                throw new ArgumentException(String.Format("Unknown scenario '{0}'", scenario));
            return RunOne(minfo);
        }

        /// <summary>
        /// Runs all defined and enabled test scenarios
        /// </summary>
        /// <returns></returns>
        public int RunAll() {
            int retVal = 0;
            string sep = "\n-------------------------------------------------------------------------------";
            WriteLine(ConsoleColor.Blue, ConsoleColor.DarkGray, sep);

            MethodInfo[] minfos = typeof(SiteTestScenarios).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (MethodInfo mi in minfos) {
                if (TestAttribute.IsEnabled(mi)) {
                    retVal += RunOne(mi);
                    WriteLine(ConsoleColor.Blue, ConsoleColor.DarkGray, sep);
                }
            }

            if (0 == retVal)
                WriteLine(ConsoleColor.Green, ConsoleColor.Black, "\n\n[Done - PASS]");
            else
                WriteLine(ConsoleColor.Black, ConsoleColor.Red, "\n\n[Done - FAIL]");
            return retVal;
        }

        public int RunCOM() {
            int retVal = 0;
            string sep = "\n-------------------------------------------------------------------------------";
            WriteLine(ConsoleColor.Blue, ConsoleColor.DarkGray, sep);

            MethodInfo[] minfos = typeof(SiteTestScenarios).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (MethodInfo mi in minfos) {
                if (TestAttribute.IsCOM(mi)) {
                    retVal += RunOne(mi);
                    WriteLine(ConsoleColor.Blue, ConsoleColor.DarkGray, sep);
                }
            }

            if (0 == retVal)
                WriteLine(ConsoleColor.Green, ConsoleColor.Black, "\n\n[Done - PASS]");
            else
                WriteLine(ConsoleColor.Black, ConsoleColor.Red, "\n\n[Done - FAIL]");
            return retVal;
        }
        #endregion

        #region Private fields
        private SiteBinder _sitebinder;
        private SiteLog _log;
        #endregion
    }
}
