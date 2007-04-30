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
#if !SILVERLIGHT

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace Microsoft.Scripting.Hosting {
    
    /// <summary>
    /// Forwards to associated host wrapping objects if it resides in remote domain.
    /// Override those methods that shouldn't forward to remote host.
    /// </summary>
    public class LocalScriptHost : IScriptHost, ILocalObject {
        
        // implementors shouldn't have access to the host; they should call the base implementation for forwarding
        private RemoteScriptHost _remoteHost;
        private ScriptModule _defaultModule;

        public LocalScriptHost() {
        }

        // the host is not set in ctor in order to prevent manipulation with it while constructing the base class:
        internal void SetRemoteHost(RemoteScriptHost remoteHost) {
            Debug.Assert(remoteHost != null);
            _remoteHost = remoteHost;
        }

        public virtual string NormalizePath(string path) {
            Debug.Assert(_remoteHost != null);
            return _remoteHost.NormalizePath(path);
        }

        public virtual bool SourceFileExists(string path) {
            Debug.Assert(_remoteHost != null);
            return _remoteHost.SourceFileExists(path);
        }

        public virtual bool SourceDirectoryExists(string path) {
            Debug.Assert(_remoteHost != null);
            return _remoteHost.SourceDirectoryExists(path);
        }

        public virtual string[] GetSourceFileNames(string mask, string searchPattern) {
            Debug.Assert(_remoteHost != null);
            return _remoteHost.GetSourceFileNames(mask, searchPattern);
        }

        public virtual SourceFileUnit GetSourceFileUnit(IScriptEngine engine, string path, string name) {
            Debug.Assert(_remoteHost != null);
            return _remoteHost.GetSourceFileUnit(RemoteWrapper.WrapRemotable<IScriptEngine>(engine), path, name);
        }

        public virtual SourceFileUnit ResolveSourceFileUnit(string name) {
            Debug.Assert(_remoteHost != null);
            return _remoteHost.ResolveSourceFileUnit(name);
        }

        /// <summary>
        /// TODO: Called under a lock. Should work with the engine via the argument only.
        /// </summary>
        public virtual void EngineCreated(IScriptEngine engine) {
            if (engine == null) throw new ArgumentNullException("engine");
            Debug.Assert(_remoteHost != null);
            _remoteHost.EngineCreated(RemoteWrapper.WrapRemotable<IScriptEngine>(engine));
        }

        public virtual void ModuleCreated(IScriptModule module) {
            if (module == null) throw new ArgumentNullException("module");
            Debug.Assert(_remoteHost != null);
            _remoteHost.ModuleCreated(RemoteWrapper.WrapRemotable<IScriptModule>(module));
        }

        // throws SerializationException 
        public virtual bool TrySetVariable(IScriptEngine engine, SymbolId name, object value) {
            Debug.Assert(_remoteHost != null);
            return _remoteHost.TrySetVariable(RemoteWrapper.WrapRemotable<IScriptEngine>(engine), name, value);
        }

        // throws SerializationException 
        public virtual bool TryGetVariable(IScriptEngine engine, SymbolId name, out object value) {
            Debug.Assert(_remoteHost != null);
            return _remoteHost.TryGetVariable(RemoteWrapper.WrapRemotable<IScriptEngine>(engine), name, out value);
        }

        public virtual IScriptModule DefaultModule {
            get {
                if (_defaultModule == null) {
                    ScriptHost.CreateDefaultModule(ref _defaultModule);
                }
                return _defaultModule;
            }
        }

        #region ILocalObject Members

#if !SILVERLIGHT
        RemoteWrapper ILocalObject.Wrap() {
            return new RemoteScriptHost(this);
        }
#endif

        #endregion
    }
}

#endif