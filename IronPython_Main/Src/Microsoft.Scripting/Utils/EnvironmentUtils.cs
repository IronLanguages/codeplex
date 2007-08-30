/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Microsoft.Scripting.Utils {
    public static class EnvironmentUtils {

#if SILVERLIGHT
        private static UnhandledExceptionEventHandler unhandledExceptionEventHandler;
        private static string[] commandLineArgs;

        public class ExitProcessException : Exception {

            public int ExitCode { get { return exitCode; } }
            int exitCode;

            public ExitProcessException(int exitCode) {
                this.exitCode = exitCode;
            }
        }

        public static void ExitProcess(int exitCode) {
            throw new ExitProcessException(exitCode);
        }
#else
        public static bool IsOrcas {
            get {
                Type t = typeof(object).Assembly.GetType("System.DateTimeOffset", false);
                return t != null;
            }
        }
#endif

        public delegate int MainRoutine();

        public static int RunMain(MainRoutine main, string[] args) {
            Debug.Assert(main != null && args != null);
#if SILVERLIGHT
            commandLineArgs = args;
            try {
                return main();
            } catch (ExitProcessException e) {
                // Environment.Exit:
                return e.ExitCode;
            } catch (Exception e) {

                // unhandled exceptions:
                if (unhandledExceptionEventHandler != null)
                    unhandledExceptionEventHandler(null, new UnhandledExceptionEventArgs(e, true));
                else
                    throw;

                return 1;
            }
#else
            return main();
#endif
        }

        public static string[] GetCommandLineArgs() {
#if SILVERLIGHT
            return (string[])commandLineArgs.Clone();
#else
            return System.Environment.GetCommandLineArgs();
#endif
        }

        public static void AddUnhandledExceptionHandler(UnhandledExceptionEventHandler handler) {
#if SILVERLIGHT
            unhandledExceptionEventHandler += handler;
#else
            AppDomain.CurrentDomain.UnhandledException += handler;
#endif
        }
    }
}
