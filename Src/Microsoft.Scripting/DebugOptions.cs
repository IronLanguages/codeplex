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

using System.Security;
using System;

namespace Microsoft.Scripting {

    /// <summary>
    /// This class holds onto internal debugging options used in this assembly. 
    /// These options can be set via environment variables DLR_{option-name}.
    /// Boolean options map "true" to true and other values to false.
    /// 
    /// These options are for internal debugging only, and should not be
    /// exposed through any public APIs.
    /// </summary>
    internal static class DebugOptions {

        private static bool ReadOption(string name) {
#if SILVERLIGHT
            return false;
#else
            string envVar = ReadString(name);
            return envVar != null && envVar.ToLowerInvariant() == "true";
#endif
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "name")]
        private static bool ReadDebugOption(string name) {
#if DEBUG
            return ReadOption(name);
#else
            return false;
#endif
        }

        private static string ReadString(string name) {
#if SILVERLIGHT
            return null;
#else
            try {
                return Environment.GetEnvironmentVariable("DLR_" + name);
            } catch (SecurityException) {
                return null;
            }
#endif
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "name")]
        private static string ReadDebugString(string name) {
#if DEBUG
            return ReadString(name);
#else
            return null;
#endif
        }

        // TODO: this is a Python specific option (for optimized modules)
        // Can it be debug-only, or moved to Python?
        private readonly static bool _lightweightScopes = ReadOption("LightweightScopes");

        private readonly static bool _frames = ReadDebugOption("Frames");
        private readonly static bool _trackPerformance = ReadDebugOption("TrackPerformance");

        /// <summary>
        /// Generate functions using custom frames. Allocate the locals on frames.
        /// When custom frames are turned on, we emit dictionaries everywhere
        /// </summary>
        internal static bool Frames {
            get { return _frames; }
        }

        /// <summary>
        /// Generate optimized scopes that can be garbage collected
        /// (globals are stored in an array instead of static fields on a
        /// generated type)
        /// </summary>
        internal static bool LightweightScopes {
            get { return _lightweightScopes; }
        }

        internal static bool TrackPerformance {
            get { return _trackPerformance; }
        }


        // These options are read from the environment variables because they
        // can be mutated by tests

        /// <summary>
        /// Directory where snippet assembly will be saved if SaveSnippets is set.
        /// </summary>
        internal static string SnippetsDirectory {
            get { return ReadDebugString("AssembliesDir"); }
        }

        /// <summary>
        /// Name of the snippet assembly (w/o extension).
        /// </summary>
        internal static string SnippetsFileName {
            get { return ReadDebugString("AssembliesFileName"); }
        }

        /// <summary>
        /// Save snippets to an assembly (see also SnippetsDirectory, SnippetsFileName).
        /// </summary>
        internal static bool SaveSnippets {
            get { return ReadDebugOption("SaveAssemblies"); }
        }
    }
}