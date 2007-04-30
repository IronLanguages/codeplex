/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Scripting.Hosting;
using System.Reflection;
using Microsoft.Scripting.Internal.Generation;

namespace Microsoft.Scripting.Hosting {
    public enum DivisionOption {
        Old,
        New,
        Warn,
        WarnAll
    }

    [Serializable]
    public sealed class ScriptDomainOptions {
        private bool debugMode = true;
        private bool engineDebug;
        private bool verbose;
        private bool traceBackSupport = (IntPtr.Size == 4);  // currently only enabled on 32-bit
        private bool checkInitialized = true;
        private bool debugCodeGen = false;
        private bool trackPerformance;
        private bool optimizeEnvironments = true;
        private bool frames;
        private AssemblyGenAttributes assemblyGenAttributes = AssemblyGenAttributes.None;
        private string binariesDirectory;
        private bool privateBinding;
        private bool generateSafeCasts = true;
        private bool doNotCacheConstants;
        private bool generateModulesAsSnippets;
        private bool bufferedStdOutAndError = true;
        private DivisionOption division = DivisionOption.Old;
        private bool python25;
        private bool fastOps = true;
        
        public ScriptDomainOptions() {
        }

        #region Public accessors

        /// <summary>
        /// True if fast-paths through ops are enabled, false other wise.
        /// </summary>
        public bool FastOps {
            get {
                return fastOps;
            }
            set {
                fastOps = value;
            }
        }

        /// <summary>
        ///  Is this a debug mode? "__debug__" is defined, and Asserts are active
        /// </summary>
        public bool DebugMode {
            get { return debugMode; }
            set { debugMode = value; }
        }

        /// <summary>
        /// corresponds to the "-v" command line parameter
        /// </summary>
        public bool Verbose {
            get { return verbose; }
            set { verbose = value; }
        }

        /// <summary>
        /// Is the engine in debug mode? This is useful for debugging the engine itself
        /// </summary>
        public bool EngineDebug {
            get { return engineDebug; }
            set { 
                engineDebug = value;
#if DEBUG
                if (value) assemblyGenAttributes |= AssemblyGenAttributes.VerifyAssemblies;
                else assemblyGenAttributes &= ~AssemblyGenAttributes.VerifyAssemblies;
#endif
            }
        }

        public bool DynamicStackTraceSupport {
            get { return traceBackSupport; }
            set { traceBackSupport = value; }
        }

        // Emit CheckInitialized calls
        public bool CheckInitialized {
            get { return checkInitialized; }
            set { checkInitialized = value; }
        }

        public bool TrackPerformance {
            get { return trackPerformance; }
            set { trackPerformance = value; }
        }

        /// <summary>
        /// Should optimized code gen be disabled.
        /// </summary>
        public bool DebugCodeGeneration {
            get { return debugCodeGen; }
            set { debugCodeGen = value; }
        }

        /// <summary>
        /// Closures, generators and environments can use the optimized
        /// generated environments FunctionEnvironment2 .. 32 
        /// If false, environments are stored in FunctionEnvironmentN only
        /// </summary>
        public bool OptimizeEnvironments {
            get { return optimizeEnvironments; }
            set { optimizeEnvironments = value; }
        }

        /// <summary>
        /// Generate functions using custom frames. Allocate the locals on frames.
        /// </summary>
        public bool Frames {
            get { return frames; }
            set { frames = value; }
        }

        /// <summary>
        /// Constants can be generated either by caching the boxed value in a static,
        /// or by boxing it every time its needed.
        /// </summary>
        public bool DoNotCacheConstants {
            get { return doNotCacheConstants; }
            set { doNotCacheConstants = value; }
        }

        /// <summary>
        /// Explicitly call Ops.InvalidType() for cast operations that will fail
        /// to give richer information of failing casts.
        /// </summary>
        public bool GenerateSafeCasts {
            get { return generateSafeCasts; }
            set { generateSafeCasts = value; }
        }

        public AssemblyGenAttributes AssemblyGenAttributes {
            get { return assemblyGenAttributes; }
            set { assemblyGenAttributes = value; }
        }

        public string BinariesDirectory {
            get { return binariesDirectory; }
            set { binariesDirectory = value; }
        }

        public bool PrivateBinding {
            get { return privateBinding; }
            set { privateBinding = value; }
        }

        /// <summary>
        /// true to import modules as though a sequence of snippets 
        /// </summary>
        public bool GenerateModulesAsSnippets {
            get { return generateModulesAsSnippets; }
            set { generateModulesAsSnippets = value; }
        }

        // obsolete:
        public bool BufferedStandardOutAndError {
            get { return bufferedStdOutAndError; }
            set { bufferedStdOutAndError = value; }
        }


        // Should we strip out all doc strings (the -OO command line option)?
        private bool stripDocStrings = false;

        public bool StripDocStrings {
            get { return stripDocStrings; }
            set { stripDocStrings = value; }
        }

        public DivisionOption Division {
            get { return division; }
            set { division = value; }
        }

        /// <summary>
        ///  should the Python 2.5 features be enabled (-X:Python25 commandline option) ?
        /// </summary>
        public bool Python25 {
            get { return python25; }
            set { python25 = value; }
        }

        /// <summary>
        ///  The runtime assembly was built in Debug/Release mode.
        /// </summary>
        public bool DebugAssembly {
            get {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        #endregion
    }
}