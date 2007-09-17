/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting;
using IronPython.Runtime;
using IronPython.Runtime.Calls;

namespace IronPython.Runtime {
    [PythonType(typeof(PythonDictionary))]
    public class GlobalsDictionary : ScopeDictionary {
        public GlobalsDictionary(Scope scope) : base(scope.ModuleScope) {
        }

        public override SymbolId[] GetExtraKeys() {
            return new List<SymbolId>(Scope.GetKeys(DefaultContext.Default.LanguageContext)).ToArray();
        }

        protected override bool TrySetExtraValue(SymbolId key, object value) {
            Scope.SetName(key, value);
            return true;
        }

        protected override bool TryGetExtraValue(SymbolId key, out object value) {
            return Scope.TryGetName(DefaultContext.Default.LanguageContext, key, out value);            
        }

    }
}
