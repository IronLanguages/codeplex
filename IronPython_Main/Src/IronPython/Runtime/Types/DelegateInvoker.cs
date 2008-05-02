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

using System.Runtime.CompilerServices;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime.Types {
    /// <summary>
    /// Wrapper to invoke a delegate from Python. Created from getting a PythonTypeDelegateCallSlot. 
    /// </summary>
    public class DelegateInvoker {
        private BuiltinFunction _invoker;
        private object _instance;
        private DynamicSite<object, object[], object> _site;

        public DelegateInvoker(object instance, BuiltinFunction invoker) {
            _instance = instance;
            _invoker = invoker;
        }

        [SpecialName]
        public object Call(CodeContext context, params object[] args) {
            if (!_site.IsInitialized) {
                _site.EnsureInitialized(CallAction.Make(PythonContext.GetContext(context).Binder, new CallSignature(new ArgumentInfo(ArgumentKind.List))));
            }

            return _site.Invoke(context, _invoker, args);
        }
    }
}