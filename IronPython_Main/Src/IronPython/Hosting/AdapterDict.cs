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
using System.Collections;
using System.Collections.Generic;

using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime;
using IronPython.Runtime.Types;

namespace IronPython.Hosting {

    /// <summary>
    /// Users of the Hosting APIs deal with IDictionary&lt;string, object&gt;. However, the engine talks
    /// in terms of IAttributesDictionary.
    /// </summary>
    [PythonType(typeof(PythonDictionary))]
    public class StringDictionaryAdapterDict : BaseSymbolDictionary,
        IDictionary, IDictionary<object, object>, IAttributesCollection {

        IDictionary<string, object> dict; // the underlying dictionary

        public StringDictionaryAdapterDict(IDictionary<string, object> d) {
            Contract.RequiresNotNull(d, "d");

            dict = d;
        }

        public IDictionary<string, object> UnderlyingDictionary { get { return dict; } }

        static SymbolId GetSymbolIdKey(object key) {
            Contract.RequiresNotNull(key, "key");

            if (!(key is string))
                throw new NotSupportedException("Cannot add or delete non-string attribute");

            return SymbolTable.StringToId((string)key);
        }

        [PythonClassMethod("fromkeys")]
        public static object fromkeys(CodeContext context, PythonType cls, object seq) {
            return PythonDictionary.FromKeys(context, cls, seq, null);
        }

        [PythonClassMethod("fromkeys")]
        public static object fromkeys(CodeContext context, PythonType cls, object seq, object value) {
            return PythonDictionary.FromKeys(context, cls, seq, value);
        }

        #region SymbolIdDictBase abstract methods

        public override IDictionary<object, object> AsObjectKeyedDictionary() {
            return this;
        }

        [PythonName("clear")]
        public void Clear() {
            dict.Clear();
        }

        [PythonName("__iter__")]
        public IEnumerator GetEnumerator() {
            return dict.GetEnumerator();
        }
        #endregion

        #region IAttributesDictionary Members

        public void Add(SymbolId name, object value) {
            dict[SymbolTable.IdToString(name)] = value;
        }

        public bool TryGetValue(SymbolId name, out object value) {
            return dict.TryGetValue(SymbolTable.IdToString(name), out value);
        }

        public bool Remove(SymbolId name) {
            return dict.Remove(SymbolTable.IdToString(name));
        }

        public bool ContainsKey(SymbolId name) {
            return dict.ContainsKey(SymbolTable.IdToString(name));
        }

        public virtual object this[SymbolId name] {
            get {
                return dict[SymbolTable.IdToString(name)];
            }
            set {
                dict[SymbolTable.IdToString(name)] = value;
            }
        }

        public IDictionary<SymbolId, object> SymbolAttributes {
            get {
                Dictionary<SymbolId, object> symbolIdDict = new Dictionary<SymbolId, object>();
                lock (this) {
                    foreach (KeyValuePair<string, object> kvp in dict) {
                        symbolIdDict[SymbolTable.StringToId(kvp.Key)] = kvp.Value;
                    }
                }
                return symbolIdDict;
            }
        }

        public void AddObjectKey(object name, object value) {
            Add(GetSymbolIdKey(name), value);
        }

        public bool TryGetObjectValue(object name, out object value) {
            if (name is string)
                return TryGetValue(GetSymbolIdKey(name), out value);

            value = null;
            return false;
        }

        public bool RemoveObjectKey(object name) {
            if (name is string)
                return Remove(GetSymbolIdKey(name));
            return false;
        }

        public bool ContainsObjectKey(object name) {
            if (name is string)
                return ContainsKey(GetSymbolIdKey(name));
            return false;
        }

        public int Count {
            get { return dict.Count; }
        }

        public ICollection<object> Keys {
            get {
                List<object> result = new List<object>();
                lock (this) {
                    foreach (KeyValuePair<string, object> kvp in dict) {
                        result.Add(kvp.Key);
                    }
                }
                return result;
            }
        }

        #endregion

        #region IEnumerable<KeyValuePair<object,object>> Members

        class DictionaryEnumerator : CheckedDictionaryEnumerator {
            IEnumerator<KeyValuePair<string, object>> enumerator;

            internal protected DictionaryEnumerator(IEnumerator<KeyValuePair<string, object>> e) {
                enumerator = e;
            }

            protected override object GetKey() {
                return enumerator.Current.Key;
            }

            protected override object GetValue() {
                return enumerator.Current.Value;
            }

            protected override bool DoMoveNext() {
                return enumerator.MoveNext();
            }

            protected override void DoReset() {
                enumerator.Reset();
            }
        }

        IEnumerator<KeyValuePair<object, object>> IEnumerable<KeyValuePair<object, object>>.GetEnumerator() {
            return new DictionaryEnumerator(dict.GetEnumerator());
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        #endregion

        #region IDictionary Members

        void IDictionary.Add(object key, object value) {
            AsObjectKeyedDictionary().Add(key, value);
        }

        public bool Contains(object key) {
            return ContainsObjectKey(key);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator() {
            return new DictionaryEnumerator(dict.GetEnumerator());
        }

        public bool IsFixedSize {
            get { return false; }
        }

        ICollection IDictionary.Keys {
            get {
                // data.Keys is typed as ICollection<string>. Hence, we cannot return as a ICollection.
                // Instead, we need to copy the data to a List.
                List res = new List();

                lock (this) {
                    foreach (string x in dict.Keys) {
                        res.AddNoLock(x);
                    }
                }

                return res;
            }
        }

        void IDictionary.Remove(object key) {
            RemoveObjectKey(key);
        }

        ICollection IDictionary.Values {
            get {
                // data.Keys is typed as ICollection<object>. Hence, we cannot return as a ICollection.
                // Instead, we need to copy the data to a List.
                List res = new List();

                lock (this) {
                    foreach (KeyValuePair<string, object> x in dict) {
                        res.AddNoLock(x.Value);
                    }
                }

                return res;
            }
        }

        object IDictionary.this[object key] {
            get { return AsObjectKeyedDictionary()[key]; }
            set { AsObjectKeyedDictionary()[key] = value; }
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
            get { return dict.Values; }
        }

        public object this[object key] {
            get {
                return this[GetSymbolIdKey(key)];
            }
            set {
                this[GetSymbolIdKey(key)] = value;
            }
        }

        #endregion

        #region ICollection<KeyValuePair<object,object>> Members

        public void Add(KeyValuePair<object, object> item) {
            AddObjectKey(item.Key, item.Value);
        }

        void ICollection<KeyValuePair<object, object>>.Clear() {
            Clear();
        }

        public bool Contains(KeyValuePair<object, object> item) {
            object value;
            if (TryGetObjectValue(item.Key, out value) && value == item.Value)
                return true;
            return false;
        }

        public void CopyTo(Array/*!*/ array, int index) {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        public bool IsSynchronized {
            get {
                return false;
            }
        }

        public object/*!*/ SyncRoot {
            get {
                return this;
            }
        }

        public void CopyTo(KeyValuePair<object, object>[] array, int arrayIndex) {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        public bool IsReadOnly {
            get { return false; }
        }

        public bool Remove(KeyValuePair<object, object> item) {
            object value;
            if (TryGetObjectValue(item.Key, out value) && value == item.Value) {
                return RemoveObjectKey(item.Key);
            }
            return false;
        }

        #endregion
    }   
}