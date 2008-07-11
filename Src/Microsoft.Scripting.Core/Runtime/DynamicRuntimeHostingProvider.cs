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

using System.IO;
using System.Text;

namespace System.Scripting.Runtime {
    /// <summary>
    /// DLR requires any Hosting API provider to implement this class and provide its instance upon Runtime initialization.
    /// DLR calls on it to perform basic host/system dependent operations.
    /// </summary>
    [Serializable]
    public abstract class DynamicRuntimeHostingProvider {
        /// <summary>
        /// Abstracts system operations that are used by DLR and could potentially be platform specific.
        /// </summary>
        public abstract PlatformAdaptationLayer/*!*/ PlatformAdaptationLayer { get; }

        /// <summary>
        /// Gets SourceUnit corresponding to a source file on a specifed path.
        /// The result is associated with the given language (engine) and encoding.
        /// The format of the path is host defined.
        /// </summary>
        /// <exception cref="ArgumentNullException">Engine, path or encoding is a <c>null</c> reference.</exception>
        /// <returns>Null, if the source file doesn't exist.</returns>
        public abstract SourceUnit TryGetSourceFileUnit(LanguageContext/*!*/ langauge, string/*!*/ path, Encoding/*!*/ encoding, SourceCodeKind kind);

        // TODO: fix when this moves up:
        //<exception cref="AmbiguousFileNameException">Multiple matching files were found.</exception>
        /// <summary>
        /// Resolves the given name to a source unit.
        /// </summary>
        /// <exception cref="FileNotFoundException">No file matches the specified name.</exception>
        /// <exception cref="ArgumentNullException">Name is a <c>null</c> reference.</exception>
        /// <exception cref="ArgumentException">Name is not valid.</exception>
        public abstract SourceUnit/*!*/ ResolveSourceFileUnit(string/*!*/ name);
    }
}
