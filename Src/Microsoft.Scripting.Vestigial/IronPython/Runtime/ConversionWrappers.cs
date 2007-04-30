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
using System.Collections;
using System.Text;

using IronPython.Runtime.Operations;
using Microsoft.Scripting;

namespace IronPython.Runtime {

    public class ListGenericWrapper<T> : IList<T> {
        private IList<object> _value;

        public ListGenericWrapper(IList<object> value) { this._value = value; }


        #region IList<T> Members

        public int IndexOf(T item) {
            return _value.IndexOf(item);
        }

        public void Insert(int index, T item) {
            _value.Insert(index, item);
        }

        public void RemoveAt(int index) {
            _value.RemoveAt(index);
        }

        public T this[int index] {
            get {
                return (T)_value[index];
            }
            set {
                this._value[index] = value;
            }
        }

        #endregion

        #region ICollection<T> Members

        public void Add(T item) {
            _value.Add(item);
        }

        public void Clear() {
            _value.Clear();
        }

        public bool Contains(T item) {
            return _value.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex) {
            throw new NotImplementedException();
        }

        public int Count {
            get { return _value.Count; }
        }

        public bool IsReadOnly {
            get { return _value.IsReadOnly; }
        }

        public bool Remove(T item) {
            return _value.Remove(item);
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator() {
            return new IEnumeratorOfTWrapper<T>(_value.GetEnumerator());
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator() {
            return _value.GetEnumerator();
        }

        #endregion
    }


    public class DictionaryGenericWrapper<K, V> : IDictionary<K, V> {
        private IDictionary<object, object> self;

        public DictionaryGenericWrapper(IDictionary<object, object> self) {
            this.self = self;
        }

        #region IDictionary<K,V> Members

        public void Add(K key, V value) {
            self.Add(key, value);
        }

        public bool ContainsKey(K key) {
            return self.ContainsKey(key);
        }

        public ICollection<K> Keys {
            get {
                List<K> res = new List<K>();
                foreach (object o in self.Keys) {
                    res.Add((K)o);
                }
                return res;
            }
        }

        public bool Remove(K key) {
            return self.Remove(key);
        }

        public bool TryGetValue(K key, out V value) {
            object outValue;
            if (self.TryGetValue(key, out outValue)) {
                value = (V)outValue;
                return true;
            }
            value = default(V);
            return false;
        }

        public ICollection<V> Values {
            get {
                List<V> res = new List<V>();
                foreach (object o in self.Values) {
                    res.Add((V)o);
                }
                return res;
            }
        }

        public V this[K key] {
            get {
                return (V)self[key];
            }
            set {
                self[key] = value;
            }
        }

        #endregion

        #region ICollection<KeyValuePair<K,V>> Members

        public void Add(KeyValuePair<K, V> item) {
            self.Add(new KeyValuePair<object, object>(item.Key, item.Value));
        }

        public void Clear() {
            self.Clear();
        }

        public bool Contains(KeyValuePair<K, V> item) {
            return self.Contains(new KeyValuePair<object, object>(item.Key, item.Value));
        }

        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex) {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        public int Count {
            get { return self.Count; }
        }

        public bool IsReadOnly {
            get { return self.IsReadOnly; }
        }

        public bool Remove(KeyValuePair<K, V> item) {
            return self.Remove(new KeyValuePair<object, object>(item.Key, item.Value));
        }

        #endregion

        #region IEnumerable<KeyValuePair<K,V>> Members

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator() {
            foreach (KeyValuePair<object, object> kv in self) {
                yield return new KeyValuePair<K, V>((K)kv.Key, (V)kv.Value);
            }
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator() {
            return self.GetEnumerator();
        }

        #endregion
    }



    /// <summary>
    /// Exposes a Python List as a C# IList
    /// </summary>
    public class ListWrapperForIList<T> : IList<T> {
        List list;

        public ListWrapperForIList(List wrappedList) {
            list = wrappedList;
        }


        #region IList<T> Members

        public int IndexOf(T item) {
            return list.IndexOf(item);
        }

        public void Insert(int index, T item) {
            list.Insert(index, item);
        }

        public void RemoveAt(int index) {
            list.RemoveAt(index);
        }

        public T this[int index] {
            get {
                // will throw InvalidCastException which Python code sees as TypeError.
                return (T)list[index];
            }
            set {
                list[index] = value;
            }
        }

        #endregion

        #region ICollection<T> Members

        public void Add(T item) {
            list.Add(item);
        }

        public void Clear() {
            list.Clear();
        }

        public bool Contains(T item) {
            return list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex) {
            list.CopyTo(array, arrayIndex);
        }

        public int Count {
            get { return list.Count; }
        }

        public bool IsReadOnly {
            get { return list.IsReadOnly; }
        }

        public bool Remove(T item) {
            if (list.Contains(item)) {
                list.Remove(item);
                return true;
            }
            return false;
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator() {
            for (int i = 0; i < Count; i++) {
                yield return this[i];
            }

        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return list.GetEnumerator();
        }

        #endregion

    }

    public class DictWrapperForIDict<TKey, TValue> : IDictionary<TKey, TValue> {
        PythonDictionary dict;

        public DictWrapperForIDict(PythonDictionary dictionary) {
            this.dict = dictionary;
        }

        #region IDictionary<TKey,TValue> Members

        public void Add(TKey key, TValue value) {
            dict.Add(key, value);
        }

        public bool ContainsKey(TKey key) {
            return dict.ContainsKey(key);
        }

        public ICollection<TKey> Keys {
            get {
                List<TKey> res = new List<TKey>();
                foreach (object o in dict.KeysAsList()) {
                    res.Add((TKey)o);
                }
                return res;
            }
        }

        public bool Remove(TKey key) {
            return dict.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value) {
            object retVal;
            if (dict.TryGetValue(key, out retVal)) {
                // will throw InvalidCastException (TypeError) if this fails.
                value = (TValue)retVal;
                return true;
            }
            value = default(TValue);
            return false;
        }

        public ICollection<TValue> Values {
            get {
                List<TValue> res = new List<TValue>();
                foreach (object o in dict.ValuesAsList()) {
                    res.Add((TValue)o);
                }
                return res;
            }
        }

        public TValue this[TKey key] {
            get {
                return (TValue)dict[key];
            }
            set {
                dict[key] = value;
            }
        }

        #endregion

        #region ICollection<KeyValuePair<TKey,TValue>> Members

        public void Add(KeyValuePair<TKey, TValue> item) {
            dict.Add(new KeyValuePair<object, object>(item.Key, item.Value));
        }

        public void Clear() {
            DictionaryOps.Clear(dict);
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) {
            return dict.Contains(new KeyValuePair<object, object>(item.Key, item.Value));
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
            foreach (KeyValuePair<object, object> kvp in dict) {
                array[arrayIndex++] = new KeyValuePair<TKey, TValue>((TKey)kvp.Key, (TValue)kvp.Value);
            }
        }

        public int Count {
            get { return dict.Count; }
        }

        public bool IsReadOnly {
            get { return false; }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item) {
            return dict.Remove(new KeyValuePair<object, object>(item.Key, item.Value));
        }

        #endregion

        #region IEnumerable<KeyValuePair<TKey,TValue>> Members

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
            foreach (KeyValuePair<object, object> kv in dict) {
                yield return new KeyValuePair<TKey, TValue>((TKey)kv.Key, (TValue)kv.Value);
            }
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator() {
            return dict.KeysAsList().GetEnumerator();
        }

        #endregion
    }


    public class IEnumeratorOfTWrapper<T> : IEnumerator<T> {
        IEnumerator enumerable;
        public IEnumeratorOfTWrapper(IEnumerator enumerable) {
            this.enumerable = enumerable;
        }

        #region IEnumerator<T> Members

        public T Current {
            get { return (T)enumerable.Current; }
        }

        #endregion

        #region IDisposable Members

        public void Dispose() {
        }

        #endregion

        #region IEnumerator Members

        object IEnumerator.Current {
            get { return enumerable.Current; }
        }

        public bool MoveNext() {
            return enumerable.MoveNext();
        }

        public void Reset() {
            enumerable.Reset();
        }

        #endregion
    }

    [PythonType("enumerable_wrapper")]
    public class IEnumerableOfTWrapper<T> : IEnumerable<T>, IEnumerable {
        IEnumerable enumerable;

        public IEnumerableOfTWrapper(IEnumerable enumerable) {
            this.enumerable = enumerable;
        }

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator() {
            return new IEnumeratorOfTWrapper<T>(enumerable.GetEnumerator());
        }

        #endregion

        #region IEnumerable Members

        [PythonName("__iter__")]
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        #endregion
    }
}
