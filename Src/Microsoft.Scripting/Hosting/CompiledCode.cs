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
using System.Scripting;
using System.Scripting.Utils;
using System.Security.Permissions;

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

        internal ScriptCode/*!*/ ScriptCode { get { return _code; } }
        
        internal CompiledCode(ScriptEngine/*!*/ engine, ScriptCode/*!*/ code) {
            Assert.NotNull(engine);
            Assert.NotNull(code);

            _engine = engine;
            _code = code;
        }

        /// <summary>
        /// Engine that compiled this code.
        /// </summary>
        public ScriptEngine/*!*/ Engine { 
            get { return _engine; } 
        }

        /// <summary>
        /// TODO: Executes code in an optimized scope.
        /// </summary>
        public object Execute() {
            return _code.Run();
        }

        /// <summary>
        /// Execute code within a given scope and returns the result.
        /// </summary>
        public object Execute(ScriptScope/*!*/ scope) {
            ContractUtils.RequiresNotNull(scope, "scope");
            return _code.Run(scope.Scope);
        }

#if !SILVERLIGHT
        public ObjectHandle/*!*/ ExecuteAndWrap(ScriptScope/*!*/ scope) {
            return new ObjectHandle(Execute(scope));
        }

        // TODO: Figure out what is the right lifetime
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService() {
            return null;
        }
#endif
    }
}
