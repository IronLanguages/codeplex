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
    internal static class GlobalDlrOptions {
        private const string EnvironmentVariablePrefix = "DLR_";

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static GlobalDlrOptions() {
            try {
                _frames = ReadOption("Frames");
                _lightweightScopes = ReadOption("LightweightScopes");
            } catch (SecurityException) {
                return;
            }
        }

        private static bool ReadOption(string name) {
#if SILVERLIGHT
            return false;
#else
            string envVar = Environment.GetEnvironmentVariable(EnvironmentVariablePrefix + name);
            return envVar != null && envVar.ToLowerInvariant() == "true";
#endif
        }

        private readonly static bool _frames;
        private readonly static bool _lightweightScopes;

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
    }
}