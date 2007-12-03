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

using System.Diagnostics;
using System.Runtime.Remoting;

using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Hosting {

    public interface ICompiledCode : IRemotable {
        IScriptScope MakeModule(string name);
        
        void Execute();
        void Execute(IScriptScope module);

        object Evaluate();
        object Evaluate(IScriptScope module);
     
#if !SILVERLIGHT
        ObjectHandle EvaluateAndWrap();
        ObjectHandle EvaluateAndWrap(IScriptScope module);
#endif
    }

    /// <summary>
    /// Hosting API counterpart for <see cref="ScriptCode"/>.
    /// </summary>
    public sealed class CompiledCode : ICompiledCode, ILocalObject {
        private readonly ScriptCode _code;

        // should be called only from ScriptCode.FromCompiledCode:
        internal ScriptCode ScriptCode { get { return _code; } }

        internal CompiledCode(ScriptCode code) {
            Debug.Assert(code != null);
            _code = code;
        }

        public IScriptScope MakeModule(string name) {
            Contract.RequiresNotNull(name, "name");
            return ScriptDomainManager.CurrentManager.CreateModule(name, _code);
        }

        /// <summary>
        /// Execute code within default module context.
        /// </summary>
        public void Execute() {
            Evaluate(null);
        }

        /// <summary>
        /// Execute code within a given module context. 
        /// The module must be local with respect to the compiled code object.
        /// </summary>
        public void Execute(IScriptScope module) {
            Evaluate(module);
        }

        /// <summary>
        /// Execute code within default module context and returns the result.
        /// </summary>
        public object Evaluate() {
            return Evaluate(null);
        }

        /// <summary>
        /// Execute code within a given module context and returns the result.
        /// The module must be local with respect to the compiled code object.
        /// </summary>
        public object Evaluate(IScriptScope module) {
            ScriptScope localModule;

            if (module == null) {
                localModule = RemoteWrapper.TryGetLocal<ScriptScope>(ScriptDomainManager.CurrentManager.Host.DefaultModule);
            } else {
                localModule = RemoteWrapper.GetLocalArgument<ScriptScope>(module, "module");
            }

            return _code.Run(localModule);
        }

#if !SILVERLIGHT

        public ObjectHandle EvaluateAndWrap() {
            return new ObjectHandle(Evaluate());
        }

        public ObjectHandle EvaluateAndWrap(IScriptScope module) {
            return new ObjectHandle(Evaluate(module));
        }

#endif

        #region ILocalObject Members

#if !SILVERLIGHT
        RemoteWrapper ILocalObject.Wrap() {
            return new RemoteCompiledCode(this);
        }
#endif

        #endregion

    }
}
