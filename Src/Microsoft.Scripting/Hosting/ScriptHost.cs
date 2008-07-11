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
using System.IO;
using System.Scripting;
using System.Scripting.Runtime;
using System.Scripting.Utils;
using System.Text;

namespace Microsoft.Scripting.Hosting {

    /// <summary>
    /// ScriptHost is collocated with ScriptRuntime in the same app-domain. 
    /// The host can implement a derived class to consume some notifications and/or 
    /// customize operations like TryGetSourceUnit,ResolveSourceUnit, etc.
    ///
    /// The areguments to the the constructor of the derived class are specified in ScriptRuntimeSetup 
    /// instance that enters ScriptRuntime initialization.
    /// 
    /// If the host is remote with respect to DLR (i.e. also with respect to ScriptHost)
    /// and needs to access objects living in its app-domain it can pass MarshalByRefObject 
    /// as an argument to its ScriptHost subclass constructor.
    /// </summary>
    public class ScriptHost
#if !SILVERLIGHT
        : MarshalByRefObject 
#endif
    {
        /// <summary>
        /// The runtime the host is attached to.
        /// </summary>
        private ScriptRuntime _runtime;

        // TODO: remove (fix Nessie)
        private ScriptScope _defaultScope;
        
        public ScriptHost() {
        }

        // Called by ScriptRuntime when it is completely initialized. 
        // Notifies the host implementation that the runtime is available now.
        internal void SetRuntime(ScriptRuntime/*!*/ runtime) {
            Assert.NotNull(runtime);
            _runtime = runtime;
            _defaultScope = _runtime.CreateScope();

            RuntimeAttached();
        }

        public ScriptRuntime/*!*/ Runtime {
            get {
                if (_runtime == null) {
                    throw new InvalidOperationException("Host not initialized");
                }
                return _runtime;
            }
        }

        // TODO: remove (fix Nessie)
        public ScriptScope/*!*/ DefaultScope {
            get {
                if (_defaultScope == null) {
                    throw new InvalidOperationException("Host not initialized");
                }
                return _defaultScope;
            }
        }

        public virtual PlatformAdaptationLayer/*!*/ PlatformAdaptationLayer {
            get {
                return PlatformAdaptationLayer.Default;
            }
        }

        #region Source File Resolution and Creation

        public const string PathEnvironmentVariableName = "DLRPATH";
        
        /// <summary>
        /// Gets the default path used for searching for source units.
        /// Default implementation returns content of <see cref="PathEnvironmentVariableName"/> environment variable.
        /// </summary>
        protected virtual IList<string>/*!*/ SourceFileSearchPath {
            get {
#if SILVERLIGHT
                return new string[] { "." };
#else
                return (System.Environment.GetEnvironmentVariable(PathEnvironmentVariableName) ?? ".").Split(Path.PathSeparator);
#endif
            }
        }

        private string/*!*/[]/*!*/ GetFullSearchPaths() {
            IList<string> list;
            try {
                list = SourceFileSearchPath;
            } catch (Exception e) {
                throw new InvalidImplementationException(
                    String.Format("Invalid host implementation; unexpected exeption thrown: {0}", e.Message), e);
            }

            string[] result = new string[list.Count];
            for (int i = 0; i < list.Count; i++) {
                try {
                    result[i] = _runtime.Manager.Platform.GetFullPath(list[i]);
                } catch (Exception e) {
                    throw new InvalidImplementationException(String.Format("Invalid host implementation: {0}", e.Message), e);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets ScriptSource representing a source file on a specifed path. The format of the path is host defined.
        /// The resulting ScriptSource is associated with given language (engine). 
        /// </summary>
        /// <exception cref="ArgumentNullException">Engine or path is a <c>null</c> reference.</exception>
        public ScriptSource TryGetSourceFile(ScriptEngine/*!*/ engine, string/*!*/ path) {
            return TryGetSourceFile(engine, path, StringUtils.DefaultEncoding, SourceCodeKind.File);
        }

        /// <summary>
        /// Gets ScriptSource representing a source file on a specifed path and encoding. The format of the path is host defined.
        /// The resulting ScriptSource is associated with given language (engine). 
        /// </summary>
        /// <exception cref="ArgumentNullException">Engine, path or encoding is a <c>null</c> reference.</exception>
        public virtual ScriptSource TryGetSourceFile(ScriptEngine/*!*/ engine, string/*!*/ path, Encoding/*!*/ encoding, SourceCodeKind kind) {
            ContractUtils.RequiresNotNull(engine, "engine");
            ContractUtils.RequiresNotNull(path, "path");
            ContractUtils.RequiresNotNull(encoding, "encoding");

            if (PlatformAdaptationLayer.FileExists(path)) {                
                return engine.CreateScriptSourceFromFile(path, encoding, kind);
            }

            return null;
        }

        /// <summary>
        /// Resolves the given name to a source file path.
        /// Directories specified in SourceUnitResolutionPath are searched for files whose name is given and whose extension
        /// is one of the extensions registered with the runtime (e.g. ".py", ".rb", etc).
        /// </summary>
        /// <exception cref="FileNotFoundException">No file matches the specified name.</exception>
        /// <exception cref="AmbiguousFileNameException">Multiple matching files were found in a directory.</exception>
        /// <exception cref="ArgumentNullException">Name is a <c>null</c> reference.</exception>
        /// <exception cref="ArgumentException">Name contains invalid characters (see System.IO.Path.GetInvalidFileNameChars).</exception>
        /// <returns>Resolved file path.</returns>
        public virtual void ResolveSourceFileName(string/*!*/ name, out string path, out ScriptEngine engine) {
            ContractUtils.RequiresNotNull(name, "name");

            foreach (string directory in GetFullSearchPaths()) {

                string candidate = null;
                ScriptEngine candidateEngine = null;
                foreach (string extension in _runtime.GetRegisteredFileExtensions()) {
                    string fullPath;

                    try {
                        fullPath = Path.Combine(directory, name + extension);
                    } catch (ArgumentException) {
                        throw new ArgumentException("Invalid file name", "name");
                    }

                    if (PlatformAdaptationLayer.FileExists(fullPath)) {
                        try {
                            candidateEngine = _runtime.GetEngineByFileExtension(extension);
                        } catch (ArgumentException) {
                            // engine has been unregistered
                            continue;
                        }

                        if (candidate != null) {
                            throw new AmbiguousFileNameException(candidate, fullPath);
                        } 

                        candidate = fullPath;
                    }
                }

                if (candidate != null) {
                    path = candidate;
                    engine = candidateEngine;
                    return;
                }
            }

            throw new FileNotFoundException();
        }


        /// <summary>
        /// Resolves file name using ResolveSourceFileName. If successful returns ScriptSource pointing to the resolved file.
        /// System.Encoding.Default is used for as the encoding of the resulting ScriptSource.
        /// The kind of the ScriptSource is set to SourceCodeKind.File.
        /// </summary>
        /// <exception cref="FileNotFoundException">No file matches the specified name.</exception>
        /// <exception cref="AmbiguousFileNameException">Multiple matching files were found in a directory.</exception>
        /// <exception cref="ArgumentNullException">Name is a <c>null</c> reference.</exception>
        /// <exception cref="ArgumentException">Name contains invalid characters (see System.IO.Path.GetInvalidFileNameChars).</exception>
        public ScriptSource/*!*/ ResolveSourceFile(string/*!*/ name) {
            string path;
            ScriptEngine engine;
            ResolveSourceFileName(name, out path, out engine);
            return engine.CreateScriptSourceFromFile(path, StringUtils.DefaultEncoding, SourceCodeKind.File);
        }

        #endregion

        #region Notifications

        /// <summary>
        /// Invoked after the initialization of the associated Runtime is finished.
        /// The host can override this method to perform additional initialization of runtime (like loading assemblies etc.).
        /// </summary>
        protected virtual void RuntimeAttached() {
            // nop
        }

        /// <summary>
        /// Invoked after a new language is loaded into the Runtime.
        /// The host can override this method to perform additional initialization of language engines.
        /// </summary>
        internal protected virtual void EngineCreated(ScriptEngine/*!*/ engine) {
            // nop
        }

        #endregion
    }

}
