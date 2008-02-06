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
using System.Collections.Generic;
using System.Text;

using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime {
    [PythonType(typeof(PythonDictionary))]
    class LocalsDictionary : ScopeDictionary {
        private LocalsDictionary(Scope scope) : base(scope) {
        }

        /// <summary>
        /// if the locals scope is composed of only a single dictionary, returns 
        /// it.  Otherwise returns the virtualized LocalsDictionary 
        /// </summary>
        internal static IAttributesCollection GetDictionaryFromScope(Scope scope) {
            Scope curScope = scope;
            int count = 0;
            while (ScopeVisible(curScope, scope)) {
                curScope = curScope.Parent;
                count++;
            }

            if (count == 1) {
                return scope.Dict;
            }

            return new LocalsDictionary(scope);
        }

        public override SymbolId[] GetExtraKeys() {
            List<SymbolId> keys = new List<SymbolId>();
            Scope curScope = Scope;

            while (ScopeVisible(curScope, Scope)) {
                keys.AddRange(curScope.Keys);
                curScope = curScope.Parent;
            }

            return keys.ToArray();
        }

        protected override bool TrySetExtraValue(SymbolId key, object value) {
            Scope.SetName(key, value);
            return true;
        }

        protected override bool TryGetExtraValue(SymbolId key, out object value) {
            Scope curScope = Scope;

            while (ScopeVisible(curScope, Scope)) {
                if (curScope.TryGetName(key, out value)) return true;
                curScope = curScope.Parent;
            }

            value = null;
            return false;
        }

        private static bool ScopeVisible(Scope curScope, Scope myScope) {
            if (myScope.Parent != null) {
                // non-leaf classes and globals (top most)
                return (curScope == myScope || curScope.IsVisible) && curScope.Parent != null;
            }
            // top-level locals, we'll just iterate once
            return curScope != null;
        }
    }
}
