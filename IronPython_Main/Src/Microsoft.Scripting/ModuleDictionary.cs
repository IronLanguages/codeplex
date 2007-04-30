/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Scripting {
#if FALSE
    public abstract class ModuleDictionary : IAttributesCollection {
        public abstract bool TryGetValue(ScriptScope scope, SymbolId name, out object value);
        public abstract void SetValue(ScriptScope scope, SymbolId name, object value);
        public abstract bool Remove(ScriptScope scope, SymbolId name);
        public abstract IEnumerator<KeyValuePair<object, object>> GetEnumerator(ScriptScope scope);
        public abstract IEnumerator<SymbolId> GetKeys(ScriptScope scope);
        public bool ContainsObjectKey(ScriptScope scope, object o) {
            string so = o as string;
            if (so != null) {
                SymbolId name = SymbolTable.StringToId(so);

                return TryGetValue(scope, name, out o);
            }

            return false;
        }
    }

    public class ModuleDictionaryAdapter : ModuleDictionary {
        private IAttributesCollection _data;

        public ModuleDictionaryAdapter(IAttributesCollection data) {
            _data = data;
        }

        public override bool TryGetValue(ScriptScope scope, SymbolId name, out object value) {
            return _data.TryGetValue(name, out value);
        }

        public override void SetValue(ScriptScope scope, SymbolId name, object value) {
            _data[name] = value;
        }

        public override bool Remove(ScriptScope scope, SymbolId name) {
            return _data.Remove(name);
        }

        public override IEnumerator<KeyValuePair<object, object>> GetEnumerator(ScriptScope scope) {
            return _data.GetEnumerator();
        }

        public override IEnumerator<SymbolId> GetKeys(ScriptScope scope) {
            foreach (KeyValuePair<SymbolId, object> kvp in _data.SymbolAttributes) {
                yield return kvp.Key;
            }
        }
    }
#endif
}
