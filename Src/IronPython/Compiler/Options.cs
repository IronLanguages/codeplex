/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Shared Source License
 * for IronPython. A copy of the license can be found in the License.html file
 * at the root of this distribution. If you can not locate the Shared Source License
 * for IronPython, please send an email to ironpy@microsoft.com.
 * By using this source code in any fashion, you are agreeing to be bound by
 * the terms of the Shared Source License for IronPython.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;

namespace IronPython.Compiler {
    /// <summary>
    /// Summary description for Options.
    /// </summary>
    public class Options {

        public enum DivisionOptions {
            Old,
            New,
            Warn,
            WarnAll
        };

        #region Static options
        // Will the command-line script be introspected after execution? Corresponds to the "-i" command-line argument
        public static bool Introspection;

        public static bool SkipFirstLine;

        public static bool TracebackSupport = (IntPtr.Size == 4);  // currently only enabled on 32-bit

        // Emit CheckInitialized calls
        public static bool CheckInitialized = true;

        public static bool TrackPerformance;

        // Should calls be optimized using ReflectOptimizer
        public static bool OptimizeReflectCalls = true;

        // Closures, generators and environments can use the optimized
        // generated environments FunctionEnvironment2 .. 32
        // If false, environments are stored in FunctionEnvironmentN only
        public static bool OptimizeEnvironments = true;

        // Experimenting with a HUGE (>100x) performance boost to simple evals
        // Its disabled for compatibility
        public static bool FastEval;

        // Generate functions using custom frames. Allocate the locals on frames.
        public static bool Frames;

        public static bool GenerateDynamicMethods = true;

        public static bool DoNotCacheConstants;

        // Explicitly call Ops.InvalidType() for cast operations that will fail
        // to give richer information of failing casts.
        public static bool GenerateSafeCasts = true;

        // Should the Reflection.Emit assembly be saved to disk and reloaded?
        // Its enabled to ease debugging
        public static bool SaveAndReloadBinaries;
        public static string BinariesDirectory = null;

        public static bool PrivateBinding;

        // true if we are emitting IL source for debugging generated code.
        public static bool ILDebug;

        // true to import modules as though a sequence of snippets
        public static bool GenerateModulesAsSnippets;

        // Should the console auto-indent the start of the suite statements of a compound statement?
        public static int AutoIndentSize = 4;

        public static int MaximumRecursion = Int32.MaxValue;

        public static bool UnbufferedStdOutAndError;

        // String containing python source to eval (used to implement the '-c' command
        // line switch).
        public static string Command;

        public static bool PrintVersionAndExit = false;

        // Whether to imply "import Site" on initialization.
        public static bool ImportSite = true;

        public static bool IgnoreEnvironmentVariables = false;

        // Whether to generate a warning or error if the tokenizer detects that indentation is
        // formatted inconsistently.
        public static bool WarningOnIndentationInconsistency = false;
        public static bool ErrorOnIndentationInconsistency = false;

        // Should we strip out all doc strings (the -OO command line option)?
        public static bool StripDocStrings = false;

        // List of -W (warning filter) options collected from the command line.
        public static List<string> WarningFilters;

        public static DivisionOptions Division = DivisionOptions.Old;

        // should the Python 2.5 features be enabled (-X:Python25 commandline option) ?
        public static bool Python25 = false;

        #endregion

        #region Instance options

        // Is this a debug mode? "__debug__" is defined, and Asserts are active
        public bool DebugMode = true;

        // Is the engine in debug mode? This is useful for debugging the engine itself
        public bool EngineDebug;

        // Display exception detail (callstack) when exception gets caught
        public bool ExceptionDetail;

        public bool ShowCLSExceptions;

        // corresponds to the "-v" command line parameter
        public bool Verbose;

        #endregion

        internal Options Clone() {
            return (Options)MemberwiseClone();
        }
    }
}
