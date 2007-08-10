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
using Microsoft.Scripting;
using IronPython.Runtime.Calls;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime {
    /// <summary>
    /// Represents a thread safe symbol dictionary which is backed by a Scope.
    /// </summary>
    public abstract class ScopeDictionary : CustomSymbolDictionary, ICodeFormattable {
        private Scope _scope;

        protected ScopeDictionary(Scope scope) {
            _scope = scope;
        }

        public override void AddObjectKey(object name, object value) {
            _scope.SetObjectName(ContextId.Empty, name, value, ScopeMemberAttributes.None);
        }

        public override System.Collections.IDictionaryEnumerator GetObjectItems() {
            return new DictionaryEnumerator(_scope.GetAllItems(DefaultContext.Default.LanguageContext).GetEnumerator());
        }

        protected override int GetObjectKeyCount() {
            int count = 0;

            foreach (object o in _scope.GetAllKeys(DefaultContext.Default.LanguageContext)) {
                if (o is string) continue;

                count++;
            }
            return count;
        }

        protected override void GetObjectKeys(List<object> res) {
            foreach (object o in _scope.GetAllKeys(DefaultContext.Default.LanguageContext)) {
                if (o is string) continue;

                res.Add(o);
            }
        }

        protected override void GetObjectValues(List<object> res) {
            foreach (KeyValuePair<object, object> kvp in _scope.GetAllItems(DefaultContext.Default.LanguageContext)) {
                if (kvp.Key is string) continue;

                res.Add(kvp.Value);
            }
        }

        public override bool TryGetObjectValue(object name, out object value) {
            return _scope.TryLookupObjectName(DefaultContext.Default.LanguageContext, name, out value);
        }

        public override bool RemoveObjectKey(object name) {
            return _scope.TryRemoveObjectName(DefaultContext.Default.LanguageContext, name);
        }

        protected Scope Scope {
            get {
                return _scope;
            }
        }

        class DictionaryEnumerator : CheckedDictionaryEnumerator {
            IEnumerator<KeyValuePair<object, object>> enumerator;

            internal protected DictionaryEnumerator(IEnumerator<KeyValuePair<object, object>> e) {
                enumerator = e;
            }

            protected override object GetKey() {
                return enumerator.Current.Key;
            }

            protected override object GetValue() {
                return enumerator.Current.Value;
            }

            protected override bool DoMoveNext() {
                bool fRes;
                while(fRes = enumerator.MoveNext()) {
                    if(!(enumerator.Current.Key is string)) break;
                }

                return fRes;
            }

            protected override void DoReset() {
                enumerator.Reset();
            }
        }

        public override string ToString() {
            return DictionaryOps.__str__(this);
        }

        public string ToCodeString(CodeContext context) {
            return DictionaryOps.__repr__(this);
        }

    }
}
