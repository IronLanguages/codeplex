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

using IronPython.Runtime.Types;

namespace IronPython.Runtime {
    /// <summary>
    /// Wrapper class around an IAttributesCollection to present a Python view of the type.
    /// </summary>
    [PythonType(typeof(PythonDictionary))]
    public class WrapperDictionary : BaseSymbolDictionary, IAttributesCollection, IDictionary<object, object> {
        private IAttributesCollection _wrap;

        public WrapperDictionary(IAttributesCollection wrap) {
            _wrap = wrap;
        }

        #region IAttributesCollection Members

        public void Add(SymbolId name, object value) {
            _wrap.Add(name, value);
        }

        public bool TryGetValue(SymbolId name, out object value) {
            return _wrap.TryGetValue(name, out value);
        }

        public bool Remove(SymbolId name) {
            return _wrap.Remove(name);
        }

        public bool ContainsKey(SymbolId name) {
            return _wrap.ContainsKey(name);
        }

        object IAttributesCollection.this[SymbolId name] {
            get {
                return _wrap[name];
            }
            set {
                _wrap[name] = value;
            }
        }

        public IDictionary<SymbolId, object> SymbolAttributes {
            get { return _wrap.SymbolAttributes; }
        }

        public void AddObjectKey(object name, object value) {
            _wrap.AddObjectKey(name, value);
        }

        public bool TryGetObjectValue(object name, out object value) {
            return _wrap.TryGetObjectValue(name, out value);
        }

        public bool RemoveObjectKey(object name) {
            return _wrap.RemoveObjectKey(name);
        }

        public bool ContainsObjectKey(object name) {
            return _wrap.ContainsObjectKey(name);
        }

        public override IDictionary<object, object> AsObjectKeyedDictionary() {
            return _wrap.AsObjectKeyedDictionary();
        }

        public int Count {
            get { return _wrap.Count; }
        }

        public ICollection<object> Keys {
            get { return _wrap.Keys; }
        }

        #endregion

        #region IEnumerable<KeyValuePair<object,object>> Members

        public IEnumerator<KeyValuePair<object, object>> GetEnumerator() {
            return _wrap.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return _wrap.GetEnumerator();
        }

        #endregion

        #region IDictionary<object,object> Members

        public void Add(object key, object value) {
            AddObjectKey(key, value);
        }

        public bool ContainsKey(object key) {
            return ContainsObjectKey(key);
        }

        public bool Remove(object key) {
            return RemoveObjectKey(key);
        }

        public bool TryGetValue(object key, out object value) {
            return TryGetObjectValue(key, out value);
        }

        public ICollection<object> Values {
            get { return this.AsObjectKeyedDictionary().Values; }
        }

        public object this[object key] {
            get {
                object value;
                if (TryGetObjectValue(key, out value)) return value;
                throw new KeyNotFoundException();
            }
            set {
                AddObjectKey(key, value);
            }
        }

        #endregion

        #region ICollection<KeyValuePair<object,object>> Members

        public void Add(KeyValuePair<object, object> item) {
            AddObjectKey(item.Key, item.Value);
        }

        public void Clear() {
        }

        public bool Contains(KeyValuePair<object, object> item) {
            object value;
            return TryGetObjectValue(item.Key, out value) && value == item.Value;
        }

        public void CopyTo(KeyValuePair<object, object>[] array, int arrayIndex) {
            throw new Exception("The method or operation is not implemented.");
        }

        public bool IsReadOnly {
            get { return false; }
        }

        public bool Remove(KeyValuePair<object, object> item) {
            if (Contains(item)) {
                return Remove(item.Key);
            }
            return false;
        }

        #endregion

        [PythonClassMethod("fromkeys")]
        public static object FromKeys(CodeContext context, PythonType cls, object seq) {
            return PythonDictionary.FromKeys(context, cls, seq, null);
        }

        [PythonClassMethod("fromkeys")]
        public static object FromKeys(CodeContext context, PythonType cls, object seq, object value) {
            return PythonDictionary.FromKeys(context, cls, seq, value);
        }

        public override string ToString() {
            return DictionaryOps.__str__(this);
        }
    }
}
