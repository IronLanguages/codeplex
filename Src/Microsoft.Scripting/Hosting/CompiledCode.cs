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
using System;

namespace Microsoft.Scripting.Hosting {

    public interface ICompiledCode : IRemotable {
        IScriptScope MakeOptimizedScope();
        
        void Execute(IScriptScope module);
        object Evaluate(IScriptScope module);
     
#if !SILVERLIGHT
        ObjectHandle EvaluateAndWrap(IScriptScope module);
#endif
    }

    /// <summary>
    /// Hosting API counterpart for <see cref="ScriptCode"/>.
    /// </summary>
    public sealed class CompiledCode : ICompiledCode, ILocalObject {
        private readonly ScriptEngine/*!*/ _engine;
        private readonly ScriptCode/*!*/ _code;

        // should be called only from ScriptCode.FromCompiledCode:
        internal ScriptCode/*!*/ ScriptCode { get { return _code; } }

        internal CompiledCode(ScriptEngine/*!*/ engine, ScriptCode/*!*/ code) {
            Assert.NotNull(engine);
            Assert.NotNull(code);

            _engine = engine;
            _code = code;
        }

        public IScriptScope/*!*/ MakeOptimizedScope() {
            return new ScriptScope(_engine, _code.MakeOptimizedScope());
        }

        /// <summary>
        /// Execute code within a given module context. 
        /// The module must be local with respect to the compiled code object.
        /// </summary>
        public void Execute(IScriptScope/*!*/ scope) {
            Evaluate(scope);
        }

        /// <summary>
        /// Execute code within a given module context and returns the result.
        /// The module must be local with respect to the compiled code object.
        /// </summary>
        public object Evaluate(IScriptScope/*!*/ scope) {
            Contract.RequiresNotNull(scope, "scope");

            ScriptScope localModule = RemoteWrapper.GetLocalArgument<ScriptScope>(scope, "module");
            return _code.Run(localModule.Scope);
        }

#if !SILVERLIGHT

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
