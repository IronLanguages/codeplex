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
using System.Runtime.Remoting;
using System.Security.Permissions;

using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Hosting {

    /// <summary>
    /// Hosting API counterpart for <see cref="ScriptCode"/>.
    /// </summary>
    public sealed class CompiledCode 
#if !SILVERLIGHT 
        : MarshalByRefObject 
#endif
    {
        private readonly ScriptEngine/*!*/ _engine;
        private readonly ScriptCode/*!*/ _code;

        internal CompiledCode(ScriptEngine/*!*/ engine, ScriptCode/*!*/ code) {
            Assert.NotNull(engine);
            Assert.NotNull(code);

            _engine = engine;
            _code = code;
        }

        public ScriptScope/*!*/ MakeOptimizedScope() {
            return new ScriptScope(_engine, _code.MakeOptimizedScope());
        }

        /// <summary>
        /// Execute code within a given module context. 
        /// The module must be local with respect to the compiled code object.
        /// </summary>
        public void Execute(ScriptScope/*!*/ scope) {
            Evaluate(scope);
        }

        /// <summary>
        /// Execute code within a given module context and returns the result.
        /// The module must be local with respect to the compiled code object.
        /// </summary>
        public object Evaluate(ScriptScope/*!*/ scope) {
            ContractUtils.RequiresNotNull(scope, "scope");
            return _code.Run(scope.Scope);
        }

#if !SILVERLIGHT
        public ObjectHandle EvaluateAndWrap(ScriptScope module) {
            return new ObjectHandle(Evaluate(module));
        }

        // TODO: Figure out what is the right lifetime
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService() {
            return null;
        }
#endif
    }
}
