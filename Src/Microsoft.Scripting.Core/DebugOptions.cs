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
using System; using Microsoft;
using System.Globalization;
using System.Security;

namespace Microsoft.Scripting {

    /// <summary>
    /// This class holds onto internal debugging options of the compiler and
    /// dynamic sites. These options can be set via environment variables
    /// DLR_{option-name}. Boolean options map "true" to true and other values
    /// to false.
    /// 
    /// These options are for internal debugging only, and should not be
    /// exposed through any public APIs.
    /// 
    /// Note: all of these options are DEBUG only, except for
    /// PreferComInteropAssembly which only exists in the
    /// Microsoft.Scripting.Core build. It needs to be removed, or be
    /// controlled in some way other than an environment variable.
    /// </summary>
    internal static class DebugOptions {

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "name")]
        private static bool ReadScriptingOption(string name) {
#if MICROSOFT_SCRIPTING_CORE && !SILVERLIGHT
            try {
                string envVar = Environment.GetEnvironmentVariable("DLR_" + name);
                return envVar != null && envVar.ToUpperInvariant() == "TRUE";
            } catch (SecurityException) {
                return false;
            }
#else
            return false;
#endif
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "name")]
        private static bool ReadDebugOption(string name) {
#if DEBUG && !SILVERLIGHT
            string envVar = ReadDebugString(name);
            return envVar != null && envVar.ToUpperInvariant() == "TRUE";
#else
            return false;
#endif
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "name")]
        private static string ReadDebugString(string name) {
#if DEBUG && !SILVERLIGHT
            try {
                return Environment.GetEnvironmentVariable("DLR_" + name);
            } catch (SecurityException) {
                return null;
            }
#else
            return null;
#endif
        }

        private readonly static bool _showTrees = ReadDebugOption("ShowTrees");
        private readonly static bool _dumpTrees = ReadDebugOption("DumpTrees");
        private readonly static bool _showRules = ReadDebugOption("ShowRules");
        private readonly static bool _showIL = ReadDebugOption("ShowIL");
        private readonly static bool _dumpIL = ReadDebugOption("DumpIL");
        private readonly static bool _cachePointersInApartment = ReadDebugOption("CachePointersInApartment");
        private readonly static bool _preferComInteropAssembly = ReadScriptingOption("PreferComInteropAssembly");

        /// <summary>
        /// Print generated Abstract Syntax Trees to the console
        /// </summary>
        internal static bool ShowTrees {
            get { return _showTrees; }
        }

        /// <summary>
        /// Write out generated Abstract Syntax Trees as files in the current directory
        /// </summary>
        internal static bool DumpTrees {
            get { return _dumpTrees; }
        }

        /// <summary>
        /// Print generated action dispatch rules to the console
        /// </summary>
        internal static bool ShowRules {
            get { return _showRules; }
        }

        /// <summary>
        /// Write IL to a text file as it is generated.
        /// </summary>
        internal static bool DumpIL {
            get { return _dumpIL; }
        }

        /// <summary>
        /// Prints the IL to the console as it is generated.
        /// </summary>
        internal static bool ShowIL {
            get { return _showIL; }
        }

        /// <summary>
        /// An RCW object represents a COM object which could potentially be in another apartment. So access
        /// to the COM interface pointer needs to be done in an apartment-safe way. Marshal.GetIDispatchForObject
        /// gives out the the appropriate interface pointer (and doing marshalling of the COM object to the current
        /// aparment if necessary). However, this is expensive and we would like to cache the returned interface pointer.
        /// This is a prototype of the caching optimization. It is not ready for primte-time use. Currently, it will
        /// leak COM objects as it does not call Marshal.Release when it should.
        /// </summary>
        internal static bool CachePointersInApartment {
            get { return _cachePointersInApartment; }
        }

        /// <summary>
        /// Use pure IDispatch-based invocation when calling methods/properties
        /// on System.__ComObject
        /// </summary>
        internal static bool PreferComInteropAssembly {
            get { return _preferComInteropAssembly; }
        }

        // These options are read from the environment variables because they
        // can be mutated by tests
        // TODO: these should be debug-only flags

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