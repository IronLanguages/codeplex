/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;

using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Runtime {
    [Serializable]
    public sealed class ScriptDomainOptions {
        private bool _debugMode;
        private bool _privateBinding;

        private bool _traceBackSupport = true;
        private bool _trackPerformance;
        private bool _optimizeEnvironments = true;
        private bool _frames;

        private bool _verbose;
        private bool _showASTs;
        private bool _dumpASTs;
        private bool _showRules;
        private bool _cachePointersInApartment;
        private bool _tupleBasedOptimizedScopes;

        /// <summary>
        /// Whether the application is in debug mode.
        /// This means:
        /// 
        /// 1) Symbols are emitted for debuggable methods (methods associated with SourceUnit).
        /// 2) Debuggable methods are emitted to non-collectable types (this is due to CLR limitations on dynamic method debugging).
        /// 3) Dynamic stack frames of debuggable methods are recorded at run-time 
        /// 4) JIT optimization is disabled for all methods
        /// 5) Languages may disable optimizations based on this value.
        /// 
        /// TODO: host visible, move to ScriptRuntime
        /// </summary>
        public bool DebugMode {
            get { return _debugMode; }
            set { _debugMode = value; }
        }

        /// <summary>
        /// Ignore CLR visibility checks.
        /// TODO: host visible, move to ScriptRuntime
        /// </summary>
        public bool PrivateBinding {
            get { return _privateBinding; }
            set { _privateBinding = value; }
        }

        // TODO: review
        public bool DynamicStackTraceSupport {
            get { return _traceBackSupport; }
            set { _traceBackSupport = value; }
        }

        // TODO: review
        public bool TrackPerformance {
            get { return _trackPerformance; }
            set { _trackPerformance = value; }
        }

        // TODO: review
        /// <summary>
        /// Closures, generators and environments can use the optimized
        /// generated environments FunctionEnvironment2 .. 32 
        /// If false, environments are stored in FunctionEnvironmentN only
        /// </summary>
        public bool OptimizeEnvironments {
            get { return _optimizeEnvironments; }
            set { _optimizeEnvironments = value; }
        }

        // TODO: review
        /// <summary>
        /// Generate functions using custom frames. Allocate the locals on frames.
        /// </summary>
        public bool Frames {
            get { return _frames; }
            set { _frames = value; }
        }

        #region Internal Debugging Options

        /// <summary>
        /// Print generated Abstract Syntax Trees to the console
        /// </summary>
        public bool ShowASTs {
            get { return _showASTs; }
            set { _showASTs = value; }
        }

        /// <summary>
        /// Write out generated Abstract Syntax Trees as files in the current directory
        /// </summary>
        public bool DumpASTs {
            get { return _dumpASTs; }
            set { _dumpASTs = value; }
        }

        /// <summary>
        /// Print generated action dispatch rules to the console
        /// </summary>
        public bool ShowRules {
            get { return _showRules; }
            set { _showRules = value; }
        }

        /// <summary>
        /// Use a tuple for optimized scope storage (experimental).
        /// </summary>
        public bool TupleBasedOptimizedScopes {
            get { return _tupleBasedOptimizedScopes; }
            set { _tupleBasedOptimizedScopes = value; }
        }

        /// <summary>
        /// An RCW object represents a COM object which could potentially be in another apartment. So access
        /// to the COM interface pointer needs to be done in an apartment-safe way. Marshal.GetIDispatchForObject
        /// gives out the the appropriate interface pointer (and doing marshalling of the COM object to the current
        /// aparment if necessary). However, this is expensive and we would like to cache the returned interface pointer.
        /// This is a prototype of the caching optimization. It is not ready for primte-time use. Currently, it will
        /// leak COM objects as it does not call Marshal.Release when it should.
        /// </summary>
        public bool CachePointersInApartment {
            get { return _cachePointersInApartment; }
            set {
                _cachePointersInApartment = value;
            }
        }

        // TODO: review
        public bool Verbose {
            get { return _verbose; }
            set { _verbose = value; }
        }

        // TODO: internal (test only)
        public bool EmitsUncollectableCode {
            get {
                return Snippets.Shared.SaveSnippets || _debugMode;
            }
        }

        #endregion
    }
}