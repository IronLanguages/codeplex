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

using System.Runtime.Remoting;

namespace Microsoft.Scripting.Hosting {
    internal sealed class RemoteCompiledCode : RemoteWrapper, ICompiledCode {
        private readonly CompiledCode _compiledCode;

        public RemoteCompiledCode(CompiledCode compiledCode) {
            _compiledCode = compiledCode;
        }

        public override ILocalObject LocalObject {
            get { return _compiledCode; }
        }

        #region ICompiledCode Members

        public IScriptScope MakeOptimizedScope() {
            return RemoteWrapper.WrapRemotable<IScriptScope>(_compiledCode.MakeOptimizedScope());
        }

        public void Execute(IScriptScope module) {
            _compiledCode.Execute(module);
        }

        // throws SerializationException
        public object Evaluate(IScriptScope module) {
            return _compiledCode.Evaluate(module);
        }

        public ObjectHandle EvaluateAndWrap(IScriptScope module) {
            return _compiledCode.EvaluateAndWrap(module);
        }

        #endregion
    }
}

#endif
