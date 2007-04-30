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

namespace Microsoft.Scripting.Hosting {

    /// <summary>
    /// Common wrapper for LocalScriptHost and ScriptHost.
    /// </summary>
    internal sealed class RemoteScriptHost : RemoteWrapper, IScriptHost {
        // can be both LocalScriptHost and ScriptHost
        private IScriptHost _host;

        public override ILocalObject LocalObject {
            get { return (ILocalObject)_host; }
        }

        #region Construction

        internal RemoteScriptHost() {
            // to be provided later:
            _host = null;
        }

        public RemoteScriptHost(IScriptHost host) {
            if (host == null) throw new ArgumentNullException("host");
            if (!(host is ILocalObject)) throw new ArgumentException("host");
            _host = host;
        }

        internal void SetLocalHost(IScriptHost host) {
            Debug.Assert(host != null && host is ILocalObject);
            _host = host;
        }

        public override object InitializeLifetimeService() {
            return null;
        }

        #endregion

        #region Virtual File System

        public string NormalizePath(string path) {
            return _host.NormalizePath(path);
        }

        public bool SourceFileExists(string path) {
            return _host.SourceFileExists(path);
        }

        public bool SourceDirectoryExists(string path) {
            return _host.SourceDirectoryExists(path);
        }

        public string[] GetSourceFileNames(string mask, string searchPattern) {
            return _host.GetSourceFileNames(mask, searchPattern);
        }

        #endregion

        #region Source Units

        public SourceFileUnit GetSourceFileUnit(IScriptEngine engine, string path, string name) {
            return _host.GetSourceFileUnit(engine, path, name);
        }

        public SourceFileUnit ResolveSourceFileUnit(string name) {
            return _host.ResolveSourceFileUnit(name);
        }

        #endregion

        #region Notifications

        public void EngineCreated(IScriptEngine engine) {
            _host.EngineCreated(engine);
        }

        public void ModuleCreated(IScriptModule module) {
            _host.ModuleCreated(module);
        }

        #endregion

        #region Environment Variables

        public bool TrySetVariable(IScriptEngine engine, SymbolId name, object value) {
            return _host.TrySetVariable(engine, name, value);
        }

        public bool TryGetVariable(IScriptEngine engine, SymbolId name, out object value) {
            return _host.TryGetVariable(engine, name, out value);
        }

        #endregion

        #region Modules

        public IScriptModule DefaultModule {
            get {
                return RemoteWrapper.WrapRemotable<IScriptModule>(_host.DefaultModule);
            }
        }

        #endregion
    }

}

#endif