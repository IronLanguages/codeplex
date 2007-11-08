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

namespace IronPython.Runtime {
    [PythonType(typeof(PythonDictionary))]
    class LocalsDictionary : ScopeDictionary {
        public LocalsDictionary(Scope scope) : base(scope) {
        }

        public override SymbolId[] GetExtraKeys() {
            List<SymbolId> keys = new List<SymbolId>();
            Scope curScope = Scope;

            while (ScopeVisible(curScope)) {
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

            while (ScopeVisible(curScope)) {
                if (curScope.TryGetName(key, out value)) return true;
                curScope = curScope.Parent;
            }

            value = null;
            return false;
        }

        private bool ScopeVisible(Scope curScope) {
            if (Scope.Parent != null) {
                // non-leaf classes and globals (top most)
                return (curScope == Scope || curScope.IsVisible) && curScope.Parent != null;
            }
            // top-level locals, we'll just iterate once
            return curScope != null;
        }
    }
}
