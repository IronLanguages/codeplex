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

#if !SILVERLIGHT

using System.Diagnostics;
using System.Runtime.Remoting;

using Microsoft.Scripting.Hosting;

namespace Microsoft.Scripting {

    internal sealed class RemoteScriptModule : RemoteWrapper, IScriptScope {
        private readonly ScriptScope _module;

        public override ILocalObject LocalObject {
            get { return _module; }
        }

        #region Construction

        internal RemoteScriptModule(ScriptScope module) {
            Debug.Assert(module != null);
            _module = module;
        }
        
        public override object InitializeLifetimeService() {
            return null;
        }

        #endregion


        // throws SerializationException 
        public bool TryLookupVariable(string name, out object value) {
            return _module.TryLookupVariable(name, out value);
        }

        // throws SerializationException 
        public bool TryGetVariable(string name, out object value) {
            return _module.TryGetVariable(name, out value);
        }

        // throws SerializationException 
        public object LookupVariable(string name) {
            return _module.LookupVariable(name);
        }

        // throws SerializationException 
        public void SetVariable(string name, object value) {
            _module.SetVariable(name, value);
        }
        
        public ObjectHandle LookupVariableAndWrap(string name) {
            return _module.LookupVariableAndWrap(name);
        }

        public bool VariableExists(string name) {
            return _module.VariableExists(name);
        }

        public bool RemoveVariable(string name) {
            return _module.RemoveVariable(name);
        }

        public void ClearVariables() {
            _module.ClearVariables();
        }

        public T GetVariable<T>(string/*!*/ name) {
            return _module.GetVariable<T>(name);
        }
    }
}

#endif
