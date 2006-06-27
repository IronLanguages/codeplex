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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using IronPython.Runtime.Types;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime {
    /// <summary>
    /// Represents a dictionary that can be looked up by SymbolId (that corresponds to a string) 
    /// or arbitrary object key.  SymbolIds are handed out by the SymbolTable.
    /// </summary>
    [PythonType(typeof(Dict))]
    class FieldIdDict : IDictionary, IDictionary<object, object>, IAttributesDictionary,
                        IMapping, ICloneable, IRichEquality, IRichComparable {
        Dictionary<SymbolId, object> data = new Dictionary<SymbolId, object>();

        public FieldIdDict() {
        }

        /// <summary>
        /// Creates a new field-id dict from an existing string-based dict...
        /// </summary>
        public FieldIdDict(IDictionary<object, object> from) {
            // enumeration of a dictionary requires locking
            // the target dictionary.
            lock (from) {
                foreach (KeyValuePair<object, object> kvp in from) {
                    AsObjectKeyedDictionary()[kvp.Key] = kvp.Value;
                }
            }
        }

        public FieldIdDict(IAttributesDictionary from) {
            // enumeration of a dictionary requires locking
            // the target dictionary.
            lock (from) {
                foreach (KeyValuePair<object, object> kvp in from) {
                    AsObjectKeyedDictionary().Add(kvp.Key, kvp.Value);
                }
            }

        }

        /// <summary>
        /// Field dictionaries are usually indexed using literal strings, which is handled using the SymbolTable.
        /// However, Python does allow non-string keys too. We handle this case by lazily creating an object-keyed dictionary,
        /// and keeping it in the symbol-indexed dictionary. Such access is slower, which is acceptable.
        /// </summary>
        private Dictionary<object, object> GetObjectKeysDictionary() {
            Dictionary<object, object> objData = GetObjectKeysDictionaryIfExists();
            if (objData == null) {
                objData = new Dictionary<object, object>();
                data.Add(SymbolTable.ObjectKeys, objData);
            }
            return objData;
        }

        private Dictionary<object, object> GetObjectKeysDictionaryIfExists() {
            object objData;
            if (data.TryGetValue(SymbolTable.ObjectKeys, out objData))
                return (Dictionary<object, object>)objData;
            return null;
        }

        #region IDictionary<object, object> Members

        void IDictionary<object, object>.Add(object key, object value) {
            Debug.Assert(!(key is SymbolId));

            string strKey = key as string;
            lock (this) {
                if (strKey != null) {
                    data.Add(SymbolTable.StringToId(strKey), value);
                } else {
                    Dictionary<object, object> objData = GetObjectKeysDictionary();
                    objData[key] = value;
                }
            }
        }

        bool IDictionary<object, object>.ContainsKey(object key) {
            Debug.Assert(!(key is SymbolId));
            string strKey = key as string;
            lock (this) {
                if (strKey != null) {
                    return data.ContainsKey(SymbolTable.StringToId(strKey));
                } else {
                    Dictionary<object, object> objData = GetObjectKeysDictionaryIfExists();
                    if (objData == null) return false;
                    return objData.ContainsKey(key);
                }
            }
        }

        ICollection<object> IDictionary<object, object>.Keys {
            get {
                // data.Keys is typed as ICollection<SymbolId>. Hence, we cannot return as a ICollection<object>.
                // Instead, we need to copy the data to a List<object>
                List<object> res = new List<object>();

                lock (this) {
                    foreach (SymbolId x in data.Keys) {
                        if (x == SymbolTable.ObjectKeys) continue;
                        res.Add(SymbolTable.IdToString(x));
                    }

                    Dictionary<object, object> objData = GetObjectKeysDictionaryIfExists();
                    if (objData != null) res.AddRange(objData.Keys);
                }

                return res;
            }
        }

        bool IDictionary<object, object>.Remove(object key) {
            Debug.Assert(!(key is SymbolId));
            string strKey = key as string;
            lock (this) {
                if (strKey != null) {
                    return data.Remove(SymbolTable.StringToId(strKey));
                } else {
                    Dictionary<object, object> objData = GetObjectKeysDictionaryIfExists();
                    if (objData == null) return false;
                    return objData.Remove(key);
                }
            }
        }

        bool IDictionary<object, object>.TryGetValue(object key, out object value) {
            Debug.Assert(!(key is SymbolId));
            string strKey = key as string;
            lock (this) {
                if (strKey != null) {
                    return data.TryGetValue(SymbolTable.StringToId(strKey), out value);
                } else {
                    value = null;
                    Dictionary<object, object> objData = GetObjectKeysDictionaryIfExists();
                    if (objData == null) return false;
                    return objData.TryGetValue(key, out value);
                }
            }
        }

        ICollection<object> IDictionary<object, object>.Values {
            get {
                // Are there any object-keys? If not we can use a fast-path
                lock (this) {
                    Dictionary<object, object> objData = GetObjectKeysDictionaryIfExists();
                    if (objData == null)
                        return data.Values;

                    // There any object-keys. We need to flatten out all the values
                    List<object> res = new List<object>();

                    foreach (KeyValuePair<SymbolId, object> x in data) {
                        if (x.Key == SymbolTable.ObjectKeys) continue;
                        res.Add(x.Value);
                    }

                    foreach (object o in objData.Values) {
                        res.Add(o);
                    }

                    return res;
                }
            }
        }

        object IDictionary<object, object>.this[object key] {
            get {
                Debug.Assert(!(key is SymbolId));
                string strKey = key as string;
                lock (this) {
                    if (strKey != null) {
                        object value;
                        if (data.TryGetValue(SymbolTable.StringToId(strKey), out value))
                            return value;
                    } else {
                        Dictionary<object, object> objData = GetObjectKeysDictionaryIfExists();
                        if (objData != null)
                            return objData[key];
                    }
                }
                throw Ops.KeyError("'{0}'", key);
            }
            set {
                Debug.Assert(!(key is SymbolId));
                string strKey = key as string;
                lock (this) {
                    if (strKey != null) {
                        data[SymbolTable.StringToId(strKey)] = value;
                    } else {
                        Dictionary<object, object> objData = GetObjectKeysDictionary();
                        objData[key] = value;
                    }
                }
            }
        }

        #endregion

        #region ICollection<KeyValuePair<string,object>> Members

        public void Add(KeyValuePair<object, object> item) {
            string strKey = item.Key as string;
            lock (this) {
                if (strKey != null) {
                    data.Add(SymbolTable.StringToId(strKey), item.Value);
                } else {
                    Dictionary<object, object> objData = GetObjectKeysDictionary();
                    objData[item.Key] = item.Value;
                }
            }
        }

        [PythonName("clear")]
        public void Clear() {
            lock (this) data.Clear();
        }

        public bool Contains(KeyValuePair<object, object> item) {
            object value;
            if (AsObjectKeyedDictionary().TryGetValue(item.Key, out value) && value == item.Value) return true;
            return false;
        }

        public void CopyTo(KeyValuePair<object, object>[] array, int arrayIndex) {
            throw new Exception("The method or operation is not implemented.");
        }

        public int Count {
            get {
                lock (this) {
                    int count = data.Count;
                    Dictionary<object, object> objData = GetObjectKeysDictionaryIfExists();
                    if (objData != null) {
                        // -1 is because data contains objData
                        count += objData.Count - 1;
                    }
                    return count;
                }
            }
        }

        public bool IsReadOnly {
            get { return false; }
        }

        public bool Remove(KeyValuePair<object, object> item) {
            lock (this) {
                string strKey = item.Key as string;
                if (strKey != null) {
                    object value;
                    if (AsObjectKeyedDictionary().TryGetValue(strKey, out value) && value == item.Value) {
                        data.Remove(SymbolTable.StringToId(strKey));
                        return true;
                    }
                }
            }
            return false;
        }

        #endregion

        #region IEnumerable<KeyValuePair<object,object>> Members

        public IEnumerator<KeyValuePair<object, object>> GetEnumerator() {
            foreach (KeyValuePair<SymbolId, object> o in data) {
                if (o.Key == SymbolTable.ObjectKeys) continue;
                yield return new KeyValuePair<object, object>(SymbolTable.IdToString(o.Key), o.Value);
            }

            Dictionary<object, object> objData = GetObjectKeysDictionaryIfExists();
            if (objData != null) {
                foreach (KeyValuePair<object, object> o in objData) {
                    yield return o;
                }
            }
        }

        #endregion

        #region IEnumerable Members

        [PythonName("__iter__")]
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            foreach (KeyValuePair<SymbolId, object> o in data) {
                if (o.Key == SymbolTable.ObjectKeys) continue;
                yield return SymbolTable.IdToString(o.Key);
            }

            IDictionary<object, object> objData = GetObjectKeysDictionaryIfExists();
            if (objData != null) {
                foreach (object o in objData.Keys)
                    yield return o;
            }
        }

        #endregion

        #region IAttributesDictionary Members

        public void Add(SymbolId key, object value) {
            lock (this) data.Add(key, value);
        }

        public bool ContainsKey(SymbolId key) {
            lock (this) return data.ContainsKey(key);
        }

        public bool Remove(SymbolId key) {
            lock (this) return data.Remove(key);
        }

        public bool TryGetValue(SymbolId key, out object value) {
            lock (this) return data.TryGetValue(key, out value);
        }

        public object this[SymbolId key] {
            get {
                lock (this) return data[key];
            }
            set {
                lock (this) data[key] = value;
            }
        }

        public IDictionary<SymbolId, object> SymbolAttributes {
            get {
                lock (this) {
                    if (GetObjectKeysDictionaryIfExists() == null) return data;

                    Dictionary<SymbolId, object> d = new Dictionary<SymbolId, object>();
                    foreach (KeyValuePair<SymbolId, object> name in data) {
                        if (name.Key == SymbolTable.ObjectKeys) continue;
                        d.Add(name.Key, name.Value);
                    }
                    return d;
                }
            }
        }

        public void AddObjectKey(object key, object value) {
            AsObjectKeyedDictionary().Add(key, value);
        }

        public bool ContainsObjectKey(object key) {
            return AsObjectKeyedDictionary().ContainsKey(key);
        }

        public bool RemoveObjectKey(object key) {
            return AsObjectKeyedDictionary().Remove(key);
        }

        public bool TryGetObjectValue(object key, out object value) {
            return AsObjectKeyedDictionary().TryGetValue(key, out value);
        }

        public IDictionary<object, object> AsObjectKeyedDictionary() {
            return this;
        }

        public ICollection<object> Keys { get { return AsObjectKeyedDictionary().Keys; } }

        #endregion

        #region IDictionary Members

        void IDictionary.Add(object key, object value) {
            AsObjectKeyedDictionary().Add(key, value);
        }

        public bool Contains(object key) {
            lock (this) return AsObjectKeyedDictionary().ContainsKey(key);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator() {
            Dictionary<object, object> objData = GetObjectKeysDictionaryIfExists();
            if (objData == null) return new TransformDictEnum(data);

            List<IDictionaryEnumerator> enums = new List<IDictionaryEnumerator>();
            enums.Add(new TransformDictEnum(data));

            Dictionary<object, object>.Enumerator objDataEnumerator = objData.GetEnumerator();
            enums.Add(objDataEnumerator);

            return new DictionaryUnionEnumerator(enums);
        }

        public bool IsFixedSize {
            get { return false; }
        }

        ICollection IDictionary.Keys {
            get {
                // data.Keys is typed as ICollection<SymbolId>. Hence, we cannot return as a ICollection.
                // Instead, we need to copy the data to a List.
                List res = new List();

                lock (this) {
                    foreach (SymbolId x in data.Keys) {
                        if (x == SymbolTable.ObjectKeys) continue;
                        res.AddNoLock(SymbolTable.IdToString(x));
                    }

                    Dictionary<object, object> objData = GetObjectKeysDictionaryIfExists();
                    if (objData != null) res.AddRange(objData.Keys);
                }

                return res;
            }
        }

        void IDictionary.Remove(object key) {
            Debug.Assert(!(key is SymbolId));
            string strKey = key as string;
            lock (this) {
                if (strKey != null) {
                    data.Remove(SymbolTable.StringToId(strKey));
                } else {
                    Dictionary<object, object> objData = GetObjectKeysDictionaryIfExists();
                    if (objData != null)
                        objData.Remove(key);
                }
            }
        }

        ICollection IDictionary.Values {
            get {
                List res = new List();

                lock (this) {
                    foreach (KeyValuePair<SymbolId, object> x in data) {
                        if (x.Key == SymbolTable.ObjectKeys) continue;
                        res.AddNoLock(x.Value);
                    }

                    Dictionary<object, object> objData = GetObjectKeysDictionaryIfExists();
                    if (objData != null) res.AddRange(objData.Values);
                }

                return res;
            }
        }

        object IDictionary.this[object key] {
            get { return AsObjectKeyedDictionary()[key]; }
            set { AsObjectKeyedDictionary()[key] = value; }
        }

        #endregion

        #region ICollection Members

        public void CopyTo(Array array, int index) {
            throw new Exception("The method or operation is not implemented.");
        }

        public bool IsSynchronized {
            get { return false; }
        }

        public object SyncRoot {
            get { return null; }
        }

        #endregion

        #region IMapping Members

        [PythonName("get")]
        public object GetValue(object key) {
            Debug.Assert(!(key is SymbolId));
            return DictOps.GetIndex(this, key);
        }

        [PythonName("get")]
        public object GetValue(object key, object defaultValue) {
            Debug.Assert(!(key is SymbolId));
            return DictOps.GetIndex(this, key, defaultValue);
        }

        bool IMapping.TryGetValue(object key, out object value) {
            return ((IDictionary<object, object>)this).TryGetValue(key, out value);
        }

        object IMapping.this[object key] {
            get { return AsObjectKeyedDictionary()[key]; }
            set { AsObjectKeyedDictionary()[key] = value; }
        }

        [PythonName("__delitem__")]
        public void DeleteItem(object key) {
            DictOps.DelIndex(this, key);
        }

        #endregion

        [PythonName("has_key")]
        public object has_key(object key) {
            return DictOps.HasKey(this, key);
        }

        [PythonName("pop")]
        public object pop(object key) {
            return DictOps.Pop(this, key);
        }

        [PythonName("pop")]
        public object pop(object key, object defaultValue) {
            return DictOps.Pop(this, key, defaultValue);
        }

        [PythonName("setdefault")]
        public object setdefault(object key) {
            return DictOps.SetDefault(this, key);
        }

        [PythonName("setdefault")]
        public object setdefault(object key, object defaultValue) {
            return DictOps.SetDefault(this, key, defaultValue);
        }

        [PythonName("keys")]
        public List keys() {
            return DictOps.Keys(this);
        }

        [PythonName("values")]
        public List values() {
            return DictOps.Values(this);
        }

        [PythonName("items")]
        public List items() {
            return DictOps.Items(this);
        }

        [PythonName("iteritems")]
        public IEnumerator iteritems() {
            return DictOps.IterItems(this);
        }
        [PythonName("iterkeys")]
        public IEnumerator iterkeys() {
            return DictOps.IterKeys(this);
        }
        [PythonName("itervalues")]
        public IEnumerator itervalues() {
            return DictOps.IterValues(this);
        }

        [PythonName("__str__")]
        public override string ToString() {
            return DictOps.ToString(this);
        }

        [PythonName("update")]
        public void update(object b) {
            DictOps.Update(this, b);
        }

        [PythonName("popitem")]
        public Tuple popitem() {
            return DictOps.PopItem(this);
        }

        [PythonClassMethod("fromkeys")]
        public static object fromkeys(PythonType cls, object seq) {
            return Dict.FromKeys(cls, seq, null);
        }

        [PythonClassMethod("fromkeys")]
        public static object fromkeys(PythonType cls, object seq, object value) {
            return Dict.FromKeys(cls, seq, value);
        }


        #region IPythonContainer Members

        [PythonName("__len__")]
        public int GetLength() {
            return DictOps.Length(this);
        }

        [PythonName("__contains__")]
        public bool ContainsValue(object value) {
            return DictOps.Contains(this, value);
        }

        #endregion

        #region ICloneable Members

        [PythonName("copy")]
        public object Clone() {
            return new Dict(this);
        }

        #endregion

        #region IRichEquality Members

        [PythonName("__hash__")]
        public object RichGetHashCode() {
            throw Ops.TypeErrorForUnhashableType("dict");
        }

        [PythonName("__eq__")]
        public object RichEquals(object other) {
            IAttributesDictionary oth = other as IAttributesDictionary;
            IAttributesDictionary ths = this as IAttributesDictionary;
            if (oth == null) return Ops.FALSE;

            if (oth.Count != Count) return Ops.FALSE;

            foreach (KeyValuePair<object, object> o in ths) {
                object res;
                if (!oth.TryGetObjectValue(o.Key, out res) || !Ops.EqualRetBool(res, o.Value)) return Ops.FALSE;
            }
            return Ops.TRUE;
        }

        [PythonName("__ne__")]
        public object RichNotEquals(object other) {
            object res = RichEquals(other);
            if (res != Ops.NotImplemented) return Ops.Not(res);

            return Ops.NotImplemented;
        }

        #endregion

        #region IRichComparable Members

        [PythonName("__cmp__")]
        public object CompareTo(object obj) {
            IDictionary<object, object> other = obj as IDictionary<object, object>;
            // CompareTo is allowed to throw (string, int, etc... all do it if they don't get a matching type)
            if (other == null) return Ops.NotImplemented;

            return DictOps.CompareTo(this, other);
        }

        public object GreaterThan(object other) {
            object res = CompareTo(other);
            if (res == Ops.NotImplemented) return res;

            return ((int)res) > 0;
        }

        public object LessThan(object other) {
            object res = CompareTo(other);
            if (res == Ops.NotImplemented) return res;

            return ((int)res) < 0;
        }

        public object GreaterThanOrEqual(object other) {
            object res = CompareTo(other);
            if (res == Ops.NotImplemented) return res;

            return ((int)res) >= 0;
        }

        public object LessThanOrEqual(object other) {
            object res = CompareTo(other);
            if (res == Ops.NotImplemented) return res;

            return ((int)res) <= 0;
        }

        #endregion
    }
}
