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
    /// Boolean options map "TRUE" to true and other values to false.
    /// 
    /// These options are for internal debugging only, and should not be
    /// exposed through any public APIs.
    /// </summary>
    internal static class GlobalDlrOptions {
        private const string EnvironmentVariablePrefix = "DLR_";
        private const string CoreDlrEnvironmentVariablePrefix = "COREDLR_";

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static GlobalDlrOptions() {
            try {
                _frames = ReadOption("Frames");

                _preferComDispatch = ReadCoreOption("PreferComDispatch");
                _cachePointersInApartment = ReadCoreOption("CachePointersInApartment");

                if (_cachePointersInApartment) {
                    _preferComDispatch = true;
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

        private static bool ReadCoreOption(string name) {
#if SILVERLIGHT
            return false;
#else
            string envVar = Environment.GetEnvironmentVariable(CoreDlrEnvironmentVariablePrefix + name);
            return envVar != null && envVar == "TRUE";
#endif
        }

        private static bool _frames;
        private static bool _preferComDispatch;
        private static bool _cachePointersInApartment;

        /// <summary>
        /// Generate functions using custom frames. Allocate the locals on frames.
        /// When custom frames are turned on, we emit dictionaries everywhere
        /// </summary>
        internal static bool Frames {
            get { return _frames; }
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
        internal static bool PreferComDispatch {
            get { return _preferComDispatch; }
        }
    }
}