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
    /// The derived type can maintain an extra set of keys. If user code adds an entry into the
    /// Dict, the value is either associated with an extra key if one exists, or it is
    /// simply added to the simple dictionary "data". The extra keys are usually maintained
    /// in a highly optimized data-structure, and are hence not maintained in "data".
    /// </summary>
    [PythonType(typeof(Dict))]
    public abstract class CustomSymbolDict : SymbolIdDictBase, IDictionary, IDictionary<object, object>, IAttributesDictionary {
        Dictionary<SymbolId, object> data;

        protected CustomSymbolDict() {
        }

        //??? add and object[] ExtraValues???
        protected SymbolId[] ExtraKeys { get { return GetExtraKeys(); } }

        //!!! shouldn't need to be public
        public abstract SymbolId[] GetExtraKeys();
        // These return true if the key exits in ExtraKeys. 
        // Note that they do return true if the value is Uninitialized.
        public abstract bool TrySetExtraValue(SymbolId key, object value);
        public abstract bool TryGetExtraValue(SymbolId key, out object value);

        private void InitializeData() {
            Debug.Assert(data == null);

            data = new Dictionary<SymbolId, object>();
        }

        /// <summary>
        /// Field dictionaries are usually indexed using literal strings, which is handled using the SymbolTable.
        /// However, Python does allow non-string keys too. We handle this case by lazily creating an object-keyed dictionary,
        /// and keeping it in the symbol-indexed dictionary. Such access is slower, which is acceptable.
        /// </summary>
        private Dictionary<object, object> GetObjectKeysDictionary() {
            Dictionary<object, object> objData = GetObjectKeysDictionaryIfExists();
            if (objData == null) {
                if (data == null) InitializeData();
                objData = new Dictionary<object, object>();
                data.Add(SymbolTable.ObjectKeys, objData);
            }
            return objData;
        }

        private Dictionary<object, object> GetObjectKeysDictionaryIfExists() {
            if (data == null) return null;

            object objData;
            if (data.TryGetValue(SymbolTable.ObjectKeys, out objData))
                return (Dictionary<object, object>)objData;
            return null;
        }

        #region IDictionary<object, object> Members

        void IDictionary<object, object>.Add(object key, object value) {
            Debug.Assert(!(key is SymbolId));
            lock (this) {
                if (data == null) InitializeData();
                string strKey = key as string;
                if (strKey != null) {
                    SymbolId keyId = SymbolTable.StringToId(strKey);
                    if (TrySetExtraValue(keyId, value))
                        return;
                    data.Add(keyId, value);
                } else {
                    Dictionary<object, object> objData = GetObjectKeysDictionary();
                    objData[key] = value;
                }
            }
        }

        bool IDictionary<object, object>.ContainsKey(object key) {
            Debug.Assert(!(key is SymbolId));
            lock (this) {
                object dummy;
                return AsObjectKeyedDictionary().TryGetValue(key, out dummy);
            }
        }

        ICollection<object> IDictionary<object, object>.Keys {
            get {
                List<object> res = new List<object>();
                lock (this) if (data != null) {
                        foreach (SymbolId x in data.Keys) {
                            if (x == SymbolTable.ObjectKeys) continue;
                            res.Add(SymbolTable.IdToString(x));
                        }
                    }

                foreach (SymbolId key in ExtraKeys) {
                    if (key.Id < 0) break;

                    object dummy;
                    if (TryGetExtraValue(key, out dummy) && dummy != Uninitialized.instance) {
                        res.Add(SymbolTable.IdToString(key));
                    }
                }

                Dictionary<object, object> objData = GetObjectKeysDictionaryIfExists();
                if (objData != null) res.AddRange(objData.Keys);

                return res;
            }
        }

        bool IDictionary<object, object>.Remove(object key) {
            Debug.Assert(!(key is SymbolId));

            string strKey = key as string;
            lock (this) {
                if (strKey != null) {
                    SymbolId fieldId = SymbolTable.StringToId(strKey);
                    if (TrySetExtraValue(fieldId, Uninitialized.instance)) return true;

                    if (data == null) return false;
                    return data.Remove(fieldId);
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
                    SymbolId fieldId = SymbolTable.StringToId(strKey);

                    if (TryGetExtraValue(fieldId, out value) && value != Uninitialized.instance) return true;

                    if (data == null) return false;
                    return data.TryGetValue(fieldId, out value);
                } else {
                    Dictionary<object, object> objData = GetObjectKeysDictionaryIfExists();
                    if (objData != null)
                        return objData.TryGetValue(key, out value);
                }
            }
            value = null;
            return false;
        }

        ICollection<object> IDictionary<object, object>.Values {
            get {
                List<object> res = new List<object>();
                lock (this) {
                    if (data != null) {
                        foreach (SymbolId x in data.Keys) {
                            if (x == SymbolTable.ObjectKeys) continue;
                            res.Add(data[x]);
                        }
                    }
                }

                foreach (SymbolId key in ExtraKeys) {
                    if (key.Id < 0) break;

                    object value;
                    if (TryGetExtraValue(key, out value) && value != Uninitialized.instance) {
                        res.Add(value);
                    }
                }

                Dictionary<object, object> objData = GetObjectKeysDictionaryIfExists();
                if (objData != null) res.AddRange(objData.Values);

                return res;
            }
        }

        object IDictionary<object, object>.this[object key] {
            get {
                Debug.Assert(!(key is SymbolId));

                lock (this) {
                    string strKey = key as string;
                    if (strKey != null) {
                        SymbolId id = SymbolTable.StringToId(strKey);
                        object res;
                        if (TryGetExtraValue(id, out res) && !(res is Uninitialized)) return res;

                        if (data == null) throw Ops.KeyError("'{0}'", key);
                        return data[id];
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

                lock (this) {
                    string strKey = key as string;
                    if (strKey != null) {
                        SymbolId id = SymbolTable.StringToId(strKey);
                        if (TrySetExtraValue(id, value)) return;

                        if (data == null) InitializeData();
                        data[id] = value;
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
            throw new NotImplementedException();
        }

        [PythonName("clear")]
        public override void Clear() {
            lock (this) {
                foreach (SymbolId key in ExtraKeys) {
                    if (key.Id < 0) break;

                    TrySetExtraValue(key, Uninitialized.instance);
                }
                data = null;
            }
        }

        public bool Contains(KeyValuePair<object, object> item) {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<object, object>[] array, int arrayIndex) {
            throw new NotImplementedException();
        }

        public int Count {
            get {
                int count = 0;

                lock (this) {
                    if (data != null) {
                        count = data.Count;

                        Dictionary<object, object> objData = GetObjectKeysDictionaryIfExists();
                        if (objData != null) {
                            // -1 is because data contains objData
                            count += objData.Count - 1;
                        }
                    }

                    foreach (SymbolId key in ExtraKeys) {
                        if (key.Id < 0) break;

                        object dummy;
                        if (TryGetExtraValue(key, out dummy) && dummy != Uninitialized.instance) count++;
                    }
                }

                return count;
            }
        }

        public bool IsReadOnly {
            get { return false; }
        }

        public bool Remove(KeyValuePair<object, object> item) {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable<KeyValuePair<object,object>> Members

        IEnumerator<KeyValuePair<object, object>> IEnumerable<KeyValuePair<object,object>>.GetEnumerator() {
            if (data != null) {
                foreach (KeyValuePair<SymbolId, object> o in data) {
                    if (o.Key == SymbolTable.Invalid) break;
                    if (o.Key == SymbolTable.ObjectKeys) continue;
                    yield return new KeyValuePair<object, object>(SymbolTable.IdToString(o.Key), o.Value);
                }
            }

            foreach (SymbolId o in ExtraKeys) {
                if (o.Id < 0) break;

                object val;
                if (TryGetExtraValue(o, out val) && val != Uninitialized.instance) {
                    yield return new KeyValuePair<object, object>(SymbolTable.IdToString(o), val);
                }
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
        public override System.Collections.IEnumerator GetEnumerator() {
            return keys().GetEnumerator();
        }

        #endregion

        #region IAttributesDictionary Members

        public void Add(SymbolId key, object value) {
            lock (this) {
                if (TrySetExtraValue(key, value)) return;

                if (data == null) InitializeData();
                data.Add(key, value);
            }
        }

        public bool ContainsKey(SymbolId key) {
            object value;
            if (TryGetExtraValue(key, out value) && value != Uninitialized.instance) return true;
            if (data == null) return false;

            lock (this) return data.ContainsKey(key);
        }

        public bool Remove(SymbolId key) {
            if (TrySetExtraValue(key, Uninitialized.instance)) return true;

            if (data == null) return false;

            lock (this) return data.Remove(key);
        }

        public bool TryGetValue(SymbolId key, out object value) {
            if (TryGetExtraValue(key, out value) && value != Uninitialized.instance) return true;

            if (data == null) return false;

            lock (this) return data.TryGetValue(key, out value);
        }

        public virtual object this[SymbolId key] {
            get {
                object res;
                if (TryGetExtraValue(key, out res) && res != Uninitialized.instance) return res;

                lock (this) {
                    if (data == null) throw Ops.KeyError("'{0}'", key);
                    return data[key];
                }
            }
            set {
                if (TrySetExtraValue(key, value)) return;

                lock (this) {
                    if (data == null) InitializeData();
                    data[key] = value;
                }
            }
        }

        public IDictionary<SymbolId, object> SymbolAttributes {
            get {
                Dictionary<SymbolId, object> d = new Dictionary<SymbolId, object>();
                lock (this) {
                    foreach (KeyValuePair<SymbolId, object> name in data) {
                        if (name.Key == SymbolTable.ObjectKeys) continue;
                        d.Add(name.Key, name.Value);
                    }
                    foreach(SymbolId extraKey in ExtraKeys) {
                        object value;
                        if (TryGetExtraValue(extraKey, out value) && !(value is Uninitialized))
                            d.Add(extraKey, value);
                    }
                }
                return d;
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

        public ICollection<object> Keys { get { return AsObjectKeyedDictionary().Keys; } }

        IDictionary<object, object> IAttributesDictionary.AsObjectKeyedDictionary() {
            return AsObjectKeyedDictionary();
        }

        internal override IDictionary<object, object> AsObjectKeyedDictionary() {
            return this;
        }

        #endregion

        #region IDictionary Members

        void IDictionary.Add(object key, object value) {
            AsObjectKeyedDictionary().Add(key, value);
        }

        public bool Contains(object key) {
            return AsObjectKeyedDictionary().ContainsKey(key);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator() {
            List<IDictionaryEnumerator> enums = new List<IDictionaryEnumerator>();

            enums.Add(new ExtraKeyEnumerator(this));

            if (data != null) enums.Add(new TransformDictEnum(data));

            Dictionary<object, object> objData = GetObjectKeysDictionaryIfExists();
            if (objData != null) {
                Dictionary<object, object>.Enumerator objDataEnumerator = objData.GetEnumerator();
                if (objDataEnumerator.MoveNext())
                    if (objData != null) enums.Add(objDataEnumerator);
            }

            return new DictionaryUnionEnumerator(enums);
        }

        public bool IsFixedSize {
            get { return false; }
        }

        ICollection IDictionary.Keys {
            get { return new List(AsObjectKeyedDictionary().Keys); }
        }

        void IDictionary.Remove(object key) {
            Debug.Assert(!(key is SymbolId));
            string strKey = key as string;
            if (strKey != null) {
                SymbolId id = SymbolTable.StringToId(strKey);
                if (TrySetExtraValue(id, Uninitialized.instance)) return;

                lock (this) if (data != null) data.Remove(id);
            } else lock (this) {
                    Dictionary<object, object> objData = GetObjectKeysDictionaryIfExists();
                    if (objData != null)
                        objData.Remove(key);
                }
        }

        ICollection IDictionary.Values {
            get {
                return new List(AsObjectKeyedDictionary().Values);
            }
        }

        object IDictionary.this[object key] {
            get { return AsObjectKeyedDictionary()[key]; }
            set { AsObjectKeyedDictionary()[key] = value; }
        }

        #endregion

        #region IRichEquality Members

        [PythonName("__eq__")]
        public override object RichEquals(object obj) {
            IDictionary<object, object> other = obj as IDictionary<object, object>;
            if (other == null) throw Ops.TypeError("CompareTo argument must be a Dictionary");

            if (this.Count != other.Count) return Ops.FALSE;

            //!!! too expensive
            List thisItems = DictOps.Items(this);
            List otherItems = DictOps.Items(other);
            thisItems.Sort();
            otherItems.Sort();

            return thisItems.RichEquals(otherItems);
        }

        #endregion
    }

    /// <summary>
    /// Not all .NET enumerators throw exceptions if accessed in an invalid state. This type
    /// can be used to throw exceptions from enumerators implemented in IronPython.
    /// </summary>
    internal abstract class CheckedDictionaryEnumerator : IDictionaryEnumerator, IEnumerator<KeyValuePair<object, object>> {
        enum EnumeratorState {
            NotStarted,
            Started,
            Ended
        }

        EnumeratorState enumeratorState = EnumeratorState.NotStarted;

        void CheckEnumeratorState() {
            if (enumeratorState == EnumeratorState.NotStarted)
                throw new InvalidOperationException("Enumeration has not started. Call MoveNext.");
            else if (enumeratorState == EnumeratorState.Ended)
                throw new InvalidOperationException("Enumeration already finished.");
        }

        #region IDictionaryEnumerator Members
        public DictionaryEntry Entry {
            get {
                CheckEnumeratorState(); 
                return new DictionaryEntry(Key, Value); 
            }
        }

        public object Key {
            get {
                CheckEnumeratorState();
                return GetKey();
            }
        }

        public object Value {
            get {
                CheckEnumeratorState();
                return GetValue();
            }
        }
        #endregion

        #region IEnumerator Members
        public bool MoveNext() {
            if (enumeratorState == EnumeratorState.Ended)
                throw new InvalidOperationException("Enumeration already finished.");

            bool result = DoMoveNext();
            if (result)
                enumeratorState = EnumeratorState.Started;
            else
                enumeratorState = EnumeratorState.Ended;
            return result;
        }

        public object Current { get { return Entry; } }

        public void Reset() {
            DoReset();
            enumeratorState = EnumeratorState.NotStarted;
        }
        #endregion

        #region IEnumerator<KeyValuePair<object,object>> Members

        KeyValuePair<object, object> IEnumerator<KeyValuePair<object, object>>.Current {
            get { return new KeyValuePair<object,object>(Key, Value); }
        }

        #endregion

        #region IDisposable Members

        public void Dispose() {}

        #endregion

        #region Methods that a sub-type needs to implement
        protected abstract object GetKey();
        protected abstract object GetValue();
        protected abstract bool DoMoveNext();
        protected abstract void DoReset();
        #endregion

    }

    class ExtraKeyEnumerator : CheckedDictionaryEnumerator {
        CustomSymbolDict idDict;
        int curIndex = -1;

        public ExtraKeyEnumerator(CustomSymbolDict idDict) {
            this.idDict = idDict;
        }

        protected override object GetKey() {
            return SymbolTable.IdToString(idDict.GetExtraKeys()[curIndex]);
        }

        protected override object GetValue() {
            object val;
            bool hasExtraValue = idDict.TryGetExtraValue(idDict.GetExtraKeys()[curIndex], out val);
            Debug.Assert(hasExtraValue && !(val is Uninitialized));
            return val;
        }

        protected override bool DoMoveNext() {
            if (idDict.GetExtraKeys().Length == 0)
                return false;

            while (curIndex < (idDict.GetExtraKeys().Length - 1)) {
                curIndex++;
                if (idDict.GetExtraKeys()[curIndex].Id < 0) break;

                object val;
                if (idDict.TryGetExtraValue(idDict.GetExtraKeys()[curIndex], out val) && val != Uninitialized.instance) {
                    return true;
                }
            }
            return false;
        }

        protected override void DoReset() {
            curIndex = -1;
        }
    }
}
