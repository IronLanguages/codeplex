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
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Collections;
using System.IO;
using System.Threading;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Hosting {

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

        // friend: ScriptRuntime
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

        #region Source File Units Resolution and Creation

        public const string PathEnvironmentVariableName = "DLRPATH";
        
        /// <summary>
        /// Gets the default path used for searching for source units.
        /// </summary>
        public virtual IList<string/*!*/>/*!*/ SourceUnitResolutionPath {
            get {
#if SILVERLIGHT
                return new string[] { "" };
#else
                return (System.Environment.GetEnvironmentVariable(PathEnvironmentVariableName) ?? ".").Split(Path.PathSeparator);
#endif
            }
        }

        public virtual ScriptSource TryGetSourceFileUnit(ScriptEngine/*!*/ engine, string/*!*/ path, Encoding/*!*/ encoding, SourceCodeKind kind) {
            Contract.RequiresNotNull(engine, "engine");
            Contract.RequiresNotNull(path, "path");
            Contract.RequiresNotNull(encoding, "encoding");

            if (PlatformAdaptationLayer.FileExists(path)) {                
                return engine.CreateScriptSourceFromFile(PlatformAdaptationLayer.NormalizePath(path), encoding, kind);
            }

            return null;
        }

        /// <summary>
        /// Loads the module of the given name using the host provided semantics.
        /// 
        /// The default semantics are to search the host path for a file of the specified
        /// name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>A valid ScriptSource or null no module could be found.</returns>
        /// <exception cref="System.InvalidOperationException">An ambigious module match has occured</exception>
        public virtual ScriptSource ResolveSourceFileUnit(string/*!*/ name) {
            Contract.RequiresNotNull(name, "name");

            ScriptSource result = null;

            foreach (string directory in SourceUnitResolutionPath) {

                string finalPath = null;

                foreach (string extension in _runtime.GetRegisteredFileExtensions()) {
                    string fullPath = Path.Combine(directory, name + extension);

                    if (PlatformAdaptationLayer.FileExists(fullPath)) {
                        if (result != null) {
                            throw new InvalidOperationException(String.Format(Resources.AmbigiousModule, fullPath, finalPath));
                        }

                        try {
                            ScriptEngine engine = _runtime.GetEngineByFileExtension(extension);

                            result = TryGetSourceFileUnit(engine, fullPath, StringUtils.DefaultEncoding, SourceCodeKind.File);
                            finalPath = fullPath;
                        } catch (ArgumentException) {
                            // engine has been unregistered
                        }
                    }
                }
            }

            return result;
        }

        #endregion

        #region Notifications

        protected virtual void RuntimeAttached() {
            // nop
        }

        internal protected virtual void EngineCreated(ScriptEngine/*!*/ engine) {
            // nop
        }

        #endregion
    }

}
