/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Shared Source License
 * for IronPython. A copy of the license can be found in the License.html file
 * at the root of this distribution. If you can not locate the Shared Source License
 * for IronPython, please send an email to ironpy@microsoft.com.
 * By using this source code in any fashion, you are agreeing to be bound by
 * the terms of the Shared Source License for IronPython.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

using IronPython.Runtime.Operations;

namespace IronPython.Runtime {

    internal class ListGenericWrapper<T> : IList<T> {
        private IList<object> value;

        public ListGenericWrapper(IList<object> value) { this.value = value; }


        #region IList<T> Members

        public int IndexOf(T item) {
            return value.IndexOf(item);
        }

        public void Insert(int index, T item) {
            value.Insert(index, item);
        }

        public void RemoveAt(int index) {
            value.RemoveAt(index);
        }

        public T this[int index] {
            get {
                return (T)value[index];
            }
            set {
                this.value[index] = value;
            }
        }

        #endregion

        #region ICollection<T> Members

        public void Add(T item) {
            value.Add(item);
        }

        public void Clear() {
            value.Clear();
        }

        public bool Contains(T item) {
            return value.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex) {
            throw new NotImplementedException();
        }

        public int Count {
            get { return value.Count; }
        }

        public bool IsReadOnly {
            get { return value.IsReadOnly; }
        }

        public bool Remove(T item) {
            return value.Remove(item);
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator() {
            return new IEnumeratorOfTWrapper<T>(value.GetEnumerator());
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator() {
            return value.GetEnumerator();
        }

        #endregion
    }


    internal class DictionaryGenericWrapper<K, V> : IDictionary<K, V> {
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
            get { return self.Count;  }
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
    internal class ListWrapperForIList<T> : IList<T> {
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

    internal class DictWrapperForIDict<TKey, TValue> : IDictionary<TKey, TValue> {
        Dict dict;

        public DictWrapperForIDict(Dict dictionary) {
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
                foreach (object o in dict.keys()) {
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
                foreach (object o in dict.values()) {
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
            DictOps.Clear(dict);
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
            return dict.keys().GetEnumerator();
        }

        #endregion
    }

    internal class DictWrapperForHashtableDictionary : Hashtable {
        Dict dict;

        public DictWrapperForHashtableDictionary(Dict dictionary) {
            this.dict = dictionary;
        }

        public override void Add(object key, object value) {
            dict.Add(key, value);
        }

        public override void Clear() {
            dict.Clear();
        }

        public override bool Contains(object key) {
            return dict.ContainsKey(key);
        }

        public override bool ContainsKey(object key) {
            return dict.ContainsKey(key);
        }

        public override bool ContainsValue(object value) {
            return dict.Values.Contains(value);
        }

        public override void CopyTo(Array array, int arrayIndex) {
            base.CopyTo(array, arrayIndex);
        }

        public override int Count {
            get {
                return dict.Count;
            }
        }

        class DictionaryEnumerator : IDictionaryEnumerator {
            IEnumerator<KeyValuePair<object, object>> inner;
            public DictionaryEnumerator(IEnumerator<KeyValuePair<object, object>> innerEnum) {
                this.inner = innerEnum;
            }

            #region IDictionaryEnumerator Members

            public DictionaryEntry Entry {
                get { return new DictionaryEntry(inner.Current.Key, inner.Current.Value); }
            }

            public object Key {
                get { return inner.Current.Key; }
            }

            public object Value {
                get { return inner.Current.Value; }
            }

            #endregion

            #region IEnumerator Members

            public object Current {
                get { return inner.Current; }
            }

            public bool MoveNext() {
                return inner.MoveNext();
            }

            public void Reset() {
                inner.Reset();
            }

            #endregion
        }
        public override IDictionaryEnumerator GetEnumerator() {
            return new DictionaryEnumerator(dict.GetEnumerator());
        }

        public override bool IsFixedSize {
            get {
                return false;
            }
        }

        public override bool IsReadOnly {
            get {
                return false;
            }
        }

        public override bool IsSynchronized {
            get {
                return false;
            }
        }

        public override ICollection Keys {
            get {
                return dict.keys();
            }
        }


        public override ICollection Values {
            get {
                return (ICollection)dict.Values;
            }
        }

        public override object this[object key] {
            get {
                return dict[key];
            }
            set {
                dict[key] = value;
            }
        }

        protected override bool KeyEquals(object item, object key) {
            return Ops.EqualRetBool(item, key);
        }

        protected override int GetHash(object key) {
            return Ops.Hash(key);
        }

        public override void Remove(object key) {
            dict.Remove(key);
        }

        public override object Clone() {
            return new DictWrapperForHashtableDictionary(dict.Clone() as Dict);
        }
    }

    /// <summary>
    /// Exposes a Python List as a C# ArrayList
    /// </summary>
    internal class ListWrapperForArrayListCollection : ArrayList {
        List list;

        public ListWrapperForArrayListCollection(List wrappedList) {
            list = wrappedList;
        }

        #region ArrayList overrides
        public override int Add(object value) {
            return list.Add(value);
        }

        public override void AddRange(ICollection c) {
            list.Extend(c);
        }

        public override int BinarySearch(int index, int count, object value, IComparer comparer) {
            return list.BinarySearch(index, count, value, comparer);
        }

        public override int BinarySearch(object value) {
            return list.BinarySearch(0, Count, value, null);
        }

        public override int BinarySearch(object value, IComparer comparer) {
            return list.BinarySearch(0, Count, value, comparer);
        }

        public override void Clear() {
            list.Clear();
        }

        public override bool Contains(object value) {
            return list.Contains(value);
        }

        public override object Clone() {
            return new ListWrapperForArrayListCollection(new List(list));
        }

        public override void CopyTo(int index, Array array, int arrayIndex, int count) {
            list.CopyTo(array, index, arrayIndex, count);
        }

        public override void CopyTo(Array array) {
            list.CopyTo(array, 0);
        }

        public override void CopyTo(Array array, int index) {
            list.CopyTo(array, index);
        }

        public override int Count {
            get {
                return list.Count;
            }
        }

        public override int Capacity {
            get {
                return list.Count;
            }
            set {
                if (value < 0) throw new ArgumentOutOfRangeException("value must be greater than or equal to zero");

                while (list.Count > value) {
                    list.RemoveAt(list.Count - 1);
                }
            }
        }

        public override IEnumerator GetEnumerator() {
            return list.GetEnumerator();
        }

        public override IEnumerator GetEnumerator(int index, int count) {
            return GetRange(index, count).GetEnumerator();
        }

        public override ArrayList GetRange(int index, int count) {
            ArrayList list = new ArrayList(count);
            for (int i = 0; i < count; i++) {
                list.Add(this[index + i]);
            }
            return list;
        }

        public override int IndexOf(object value) {
            return list.IndexOf(value);
        }

        public override int IndexOf(object value, int startIndex) {
            return IndexOf(value, startIndex, Count - startIndex);
        }

        public override int IndexOf(object value, int startIndex, int count) {
            for (int i = startIndex; i < Math.Min(count + startIndex, Count); i++) {
                if (Ops.EqualRetBool(this[i], value)) return i;
            }
            return -1;
        }

        public override void Insert(int index, object value) {
            list.Insert(index, value);
        }

        public override void InsertRange(int index, ICollection c) {
            foreach (object o in c) {
                list.Insert(index++, o);
            }
        }

        public override bool IsFixedSize {
            get {
                return false;
            }
        }

        public override bool IsReadOnly {
            get {
                return false;
            }
        }

        public override int LastIndexOf(object value) {
            return LastIndexOf(value, Count - 1);
        }

        public override int LastIndexOf(object value, int startIndex) {
            return LastIndexOf(value, startIndex, Count);
        }

        public override int LastIndexOf(object value, int startIndex, int count) {
            for (int i = startIndex; i > Math.Max(startIndex - count, -1); i--) {
                if (Ops.EqualRetBool(this[i], value)) return i;
            }
            return -1;
        }

        public override void Remove(object value) {
            list.Remove(value);
        }

        public override void RemoveAt(int index) {
            list.RemoveAt(index);
        }

        public override void RemoveRange(int index, int count) {
            if (count < 0) throw Ops.ValueError("count can not be negative");

            for (int i = index + count - 1; i >= index; i--) {
                list.RemoveAt(i);
            }
        }

        public override void Reverse() {
            list.Reverse();
        }

        public override void Reverse(int index, int count) {
            list.Reverse(index, count);
        }

        public override void SetRange(int index, ICollection c) {
            foreach (object o in c) {
                list[index++] = o;
            }
        }

        public override void Sort() {
            list.Sort();
        }

        public override void Sort(int index, int count, IComparer comparer) {
            list.DoSort(comparer, null, false, index, count);
        }

        public override void Sort(IComparer comparer) {
            list.DoSort(comparer, null, false, 0, Count);
        }

        public override object[] ToArray() {
            object[] array = new object[list.Count];
            list.CopyTo(array, 0);
            return array;
        }

        public override Array ToArray(Type type) {
            Array res = Array.CreateInstance(type, list.Count);

            list.CopyTo(res, 0);
            return res;
        }

        public override void TrimToSize() {
        }

        public override object this[int index] {
            get {
                return list[index];
            }
            set {
                list[index] = value;
            }
        }

        public override bool IsSynchronized {
            get {
                return false;
            }
        }

        public override object SyncRoot {
            get {
                return false;
            }
        }
        #endregion
    }

    internal class IEnumeratorOfTWrapper<T> : IEnumerator<T> {
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

    internal class IEnumerableOfTWrapper<T> : IEnumerable<T>, IEnumerable {
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
