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

using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Interpreter {
    public static class InterpreterHelpers {
        /// <summary>
        /// Used by interpreter to invoke the dynamic site via the action binder.
        /// </summary>
        public static object ExecuteRule<T>(ActionBinder ab, CodeContext cc, DynamicAction action, object[] args) {
            T target = default(T);
            RuleSet<T> rules = null;
            return ab.UpdateSiteAndExecute<T>(cc, action, args, null, ref target, ref rules);
        }
    }
}
