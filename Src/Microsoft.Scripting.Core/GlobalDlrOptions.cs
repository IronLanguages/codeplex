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

using System.Globalization;
using System.Security;

namespace System.Scripting {

    /// <summary>
    /// This class holds onto internal debugging options of the compiler and
    /// dynamic sites. These options can be set via environment variables COREDLR_{option-name}.
    /// Boolean options map "TRUE" to true and other values to false.
    /// 
    /// These options are for internal debugging only, and should not be
    /// exposed through any public APIs.
    /// 
    /// TODO: Should these be debug only? If so, should this whole class, and
    /// all associated types (ExpressionWriter, DebugILGen) become debug only?
    /// 
    /// TODO: can we enable this in Silverlight builds somehow? Possibly
    /// something like late binding to a known type name that holds onto
    /// options?
    /// </summary>
    internal static class GlobalDlrOptions {

        private const string EnvironmentVariablePrefix = "COREDLR_";

        // Add to global suppression?
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static GlobalDlrOptions() {
            try {
                _showTrees = ReadOption("ShowTrees");
                _dumpTrees = ReadOption("DumpTrees");
                _showRules = ReadOption("ShowRules");
                _showScopes = ReadOption("ShowScopes");
                _showIL = ReadOption("ShowIL");
                _dumpIL = ReadOption("DumpIL");
                _lightweightScopes = ReadOption("LightweightScopes");
                _trackPerformance = ReadOption("TrackPerformance");

                _preferComInteropAssembly = ReadOption("PreferComInteropAssembly");
                _cachePointersInApartment = ReadOption("CachePointersInApartment");
                
                if (_cachePointersInApartment) {
                    _preferComInteropAssembly = true;
                }
            } catch (SecurityException) {
                return;
            }
        }

        private static bool ReadOption(string name) {
#if SILVERLIGHT
            return false;
#else
            string envVar = Environment.GetEnvironmentVariable(EnvironmentVariablePrefix + name);
            return envVar != null && envVar == "TRUE";
#endif
        }

        // These fields are actually readonly but FxCop gets confused if
        // they're marked that way
        private static bool _showTrees;
        private static bool _dumpTrees;
        private static bool _showRules;
        private static bool _showScopes;
        private static bool _showIL;
        private static bool _dumpIL;
        private static bool _lightweightScopes;
        private static bool _cachePointersInApartment;
        private static bool _preferComInteropAssembly;
        private static bool _trackPerformance;

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
        /// Print the scopes and closures that get generated by the compiler
        /// </summary>
        internal static bool ShowScopes {
            get { return _showScopes; }
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
        /// Generate optimized scopes that can be garbage collected
        /// (globals are stored in an array instead of static fields on a
        /// generated type)
        /// </summary>
        internal static bool LightweightScopes {
            get { return _lightweightScopes; }
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

        internal static bool TrackPerformance {
            get { return _trackPerformance; }
        }
    }
}