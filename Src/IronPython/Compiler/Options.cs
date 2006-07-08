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
    public enum DivisionOption {
        Old,
        New,
        Warn,
        WarnAll
    };

    /// <summary>
    /// Summary description for Options.
    /// </summary>
    public class Options {
        private static bool debugMode = true;
        private static bool engineDebug;
        private static bool verbose;
        private static bool traceBackSupport = (IntPtr.Size == 4);  // currently only enabled on 32-bit
        private static bool checkInitialized = true;
        private static bool optimizeReflectCalls = true;
        private static bool trackPerformance;
        private static bool optimizeEnvironments = true;
        private static bool fastEval;
        private static bool frames;
        private static bool generateDynamicMethods = true;
        private static string binariesDirectory;
        private static bool privateBinding;
        private static bool ilDebug;
        private static bool generateSafeCasts = true;
        private static bool doNotCacheConstants;
        private static bool saveAndReloadBinaries;
        private static bool generateModulesAsSnippets;
        private static int maximumRecursion = Int32.MaxValue;
        private static bool bufferedStdOutAndError = true;
        private static bool warningOnIndentationInconsistency ;
        private static bool errorOnIndentationInconsistency;
        private static List<string> warningFilters;
        private static DivisionOption division = DivisionOption.Old;
        private static bool python25;

        #region Public accessors

        /// <summary>
        ///  Is this a debug mode? "__debug__" is defined, and Asserts are active
        /// </summary>
        public static bool DebugMode {
            get { return Options.debugMode; }
            set { Options.debugMode = value; }
        }

        /// <summary>
        /// corresponds to the "-v" command line parameter
        /// </summary>
        public static bool Verbose {
            get { return Options.verbose; }
            set { Options.verbose = value; }
        }

        /// <summary>
        /// Is the engine in debug mode? This is useful for debugging the engine itself
        /// </summary>
        public static bool EngineDebug {
            get { return Options.engineDebug; }
            set { Options.engineDebug = value; }
        }

        public static bool TraceBackSupport {
            get { return Options.traceBackSupport; }
            set { Options.traceBackSupport = value; }
        }

        // Emit CheckInitialized calls
        public static bool CheckInitialized {
            get { return Options.checkInitialized; }
            set { Options.checkInitialized = value; }
        }

        public static bool TrackPerformance {
            get { return Options.trackPerformance; }
            set { Options.trackPerformance = value; }
        }

        /// <summary>
        /// Should calls be optimized using ReflectOptimizer
        /// </summary>
        public static bool OptimizeReflectCalls {
            get { return Options.optimizeReflectCalls; }
            set { Options.optimizeReflectCalls = value; }
        }

        /// <summary>
        /// Closures, generators and environments can use the optimized
        /// generated environments FunctionEnvironment2 .. 32 
        /// If false, environments are stored in FunctionEnvironmentN only
        /// </summary>
        public static bool OptimizeEnvironments {
            get { return Options.optimizeEnvironments; }
            set { Options.optimizeEnvironments = value; }
        }

        /// <summary>
        /// Should we interpret the eval expression instead of compiling it?
        /// This yields a HUGE (>100x) performance boost to simple evals.
        /// Its disabled for compatibility
        /// </summary>
        public static bool FastEvaluation {
            get { return Options.fastEval; }
            set { Options.fastEval = value; }
        }

        /// <summary>
        /// Generate functions using custom frames. Allocate the locals on frames.
        /// </summary>
        public static bool Frames {
            get { return Options.frames; }
            set { Options.frames = value; }
        }

        public static bool GenerateDynamicMethods {
            get { return Options.generateDynamicMethods; }
            set { Options.generateDynamicMethods = value; }
        }

        /// <summary>
        /// Constants can be generated either by caching the boxed value in a static,
        /// or by boxing it every time its needed.
        /// </summary>
        public static bool DoNotCacheConstants {
            get { return Options.doNotCacheConstants; }
            set { Options.doNotCacheConstants = value; }
        }

        /// <summary>
        /// Explicitly call Ops.InvalidType() for cast operations that will fail
        /// to give richer information of failing casts.
        /// </summary>
        public static bool GenerateSafeCasts {
            get { return Options.generateSafeCasts; }
            set { Options.generateSafeCasts = value; }
        }

        /// <summary>
        /// Should the Reflection.Emit assembly be saved to disk and reloaded?
        /// Its enabled to ease debugging 
        /// </summary>
        public static bool SaveAndReloadBinaries {
            get { return Options.saveAndReloadBinaries; }
            set { Options.saveAndReloadBinaries = value; }
        }

        public static string BinariesDirectory {
            get { return Options.binariesDirectory; }
            set { Options.binariesDirectory = value; }
        }

        public static bool PrivateBinding {
            get { return Options.privateBinding; }
            set { Options.privateBinding = value; }
        }

        /// <summary>
        /// true if we are emitting IL source for debugging generated code.
        /// </summary>
        public static bool ILDebug {
            get { return ilDebug; }
            set { ilDebug = value; if (ilDebug) generateDynamicMethods = false; }
        }

        /// <summary>
        /// true to import modules as though a sequence of snippets 
        /// </summary>
        public static bool GenerateModulesAsSnippets {
            get { return Options.generateModulesAsSnippets; }
            set { Options.generateModulesAsSnippets = value; }
        }

        public static int MaximumRecursion {
            get { return Options.maximumRecursion; }
            set { Options.maximumRecursion = value; }
        }

        public static bool BufferedStandardOutAndError {
            get { return Options.bufferedStdOutAndError; }
            set { Options.bufferedStdOutAndError = value; }
        }


        /// <summary> 
        /// Whether to generate a warning if the tokenizer detects that indentation is
        /// formatted inconsistently.
        /// </summary>
        public static bool WarningOnIndentationInconsistency {
            get { return Options.warningOnIndentationInconsistency; }
            set { Options.warningOnIndentationInconsistency = value; }
        }

        /// <summary> 
        /// Whether to generate an error if the tokenizer detects that indentation is
        /// formatted inconsistently.
        /// </summary>
        public static bool ErrorOnIndentationInconsistency {
            get { return Options.errorOnIndentationInconsistency; }
            set { Options.errorOnIndentationInconsistency = value; }
        }

        // Should we strip out all doc strings (the -OO command line option)?
        private static bool stripDocStrings = false;

        public static bool StripDocStrings {
            get { return Options.stripDocStrings; }
            set { Options.stripDocStrings = value; }
        }

        /// <summary>
        ///  List of -W (warning filter) options collected from the command line.
        /// </summary>
        public static IList<string> WarningFilters {
            get { return Options.warningFilters; }
            set { Options.warningFilters = new List<string>(value); }
        }

        public static DivisionOption Division {
            get { return Options.division; }
            set { Options.division = value; }
        }

        /// <summary>
        ///  should the Python 2.5 features be enabled (-X:Python25 commandline option) ?
        /// </summary>
        public static bool Python25 {
            get { return Options.python25; }
            set { Options.python25 = value; }
        }

        #endregion
    }
}
