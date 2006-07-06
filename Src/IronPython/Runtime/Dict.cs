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
using System.Text;
using System.Diagnostics;

using IronPython.Compiler;
using IronPython.Runtime.Types;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime {

    [PythonType("dict")]
    public class Dict : IMapping, IDictionary<object, object>, IComparable, ICloneable, IRichComparable, 
                        IDictionary, ICodeFormattable, IAttributesDictionary {
        internal static readonly IEqualityComparer<object> Comparer = new PythonObjectComparer();

        internal Dictionary<object, object> data;

        internal static object MakeDict(DynamicType cls) {            
            if(cls == TypeCache.Dict) return new Dict();            
            return cls.Call();
        }

        #region Constructors
        public Dict() {
            this.data = new Dictionary<object, object>(Comparer);
        }

        internal Dict(object o)
            : this() {
            Update(o);
        }

        internal Dict(int size) { data = new Dictionary<object, object>(size, Comparer); }

        [PythonName("__init__")]
        public void Initialize(object o, [ParamDict] Dict kwArgs) {
            Update(o);
            Update(kwArgs);
        }

        [PythonName("__init__")]
        public void Initialize([ParamDict] Dict kwArgs) {
            Update(kwArgs);
        }

        [PythonName("__init__")]
        public void Initialize(object o) {
            Update(o);
        }

        [PythonName("__init__")]
        public void Initialize() {
        }

        #endregion

        #region Object overrides
        [PythonName("__str__")]
        public override string ToString() {
            return DictOps.ToString(this);
        }

        #endregion

        #region IDictionary<object,object> Members

        public void Add(object key, object value) {
            lock (this) data.Add(DictOps.NullToObj(key), value);
        }

        public bool ContainsKey(object key) {
            lock (this) return data.ContainsKey(DictOps.NullToObj(key));
        }

        public ICollection<object> Keys {
            get { lock (this) return new DictionaryKeyCollection(this, data.Keys); }
        }

        public bool Remove(object key) {
            lock (this) return data.Remove(DictOps.NullToObj(key));
        }

        public bool TryGetValue(object key, out object value) {
            lock (this) return data.TryGetValue(DictOps.NullToObj(key), out value);
        }

        public ICollection<object> Values {
            get { lock (this) return data.Values; }
        }

        #endregion

        #region ICollection<KeyValuePair<object,object>> Members

        public void Add(KeyValuePair<object, object> item) {
            lock (this) data[DictOps.NullToObj(item.Key)] = item.Value;
        }

        [PythonName("clear")]
        public void Clear() {
            lock (this) data.Clear();
        }

        public bool Contains(KeyValuePair<object, object> item) {
            lock (this) return data.ContainsKey(DictOps.NullToObj(item.Key));
        }

        public void CopyTo(KeyValuePair<object, object>[] array, int arrayIndex) {
            lock (this) {
                Dictionary<object, object>.Enumerator ie = data.GetEnumerator();
                while (ie.MoveNext()) {
                    array[arrayIndex++] = new KeyValuePair<object, object>(DictOps.ObjToNull(ie.Current.Key), ie.Current.Value);
                }
            }
        }

        public int Count {
            get { return data.Count; }
        }

        public bool IsReadOnly {
            get { return ((ICollection<KeyValuePair<object, object>>)data).IsReadOnly; }
        }

        public bool Remove(KeyValuePair<object, object> item) {
            lock (this) return data.Remove(DictOps.NullToObj(item.Key));
        }

        #endregion

        #region IEnumerable<KeyValuePair<object,object>> Members

        [PythonName("__iter__")]
        public IEnumerator<KeyValuePair<object, object>> GetEnumerator() {            
            return new DictKeyValueEnumerator(data);
        }

        #endregion

        #region IEnumerable Members

        [PythonName("__iter__")]
        IEnumerator IEnumerable.GetEnumerator() {
            return new DictionaryKeyEnumerator(data);
        }

        #endregion

        #region IMapping Members

        [PythonName("get")]
        public object GetValue(object key) {
            return DictOps.GetIndex(this, key);
        }

        [PythonName("get")]
        public object GetValue(object key, object defaultValue) {
            return DictOps.GetIndex(this, key, defaultValue);
        }

        public virtual object this[object key] {
            get {
                object realKey = DictOps.NullToObj(key);
                object ret;
                lock (this) if (TryGetValue(realKey, out ret)) return ret;
                throw Ops.KeyError("'{0}'", key);
            }
            set {
                lock (this) data[DictOps.NullToObj(key)] = value;
            }
        }

        [PythonName("__delitem__")]
        public virtual void DeleteItem(object key) {
            DictOps.DelIndex(this, key);
        }

        #endregion

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

        #region Python dict implementation

        [PythonName("has_key")]
        public object HasKey(object key) {
            return DictOps.HasKey(this, key);
        }

        [PythonName("pop")]
        public object Pop(object key) {
            return DictOps.Pop(this, key);
        }

        [PythonName("pop")]
        public object Pop(object key, object defaultValue) {
            return DictOps.Pop(this, key, defaultValue);
        }

        [PythonName("popitem")]
        public Tuple PopItem() {
            return DictOps.PopItem(this);
        }

        [PythonName("setdefault")]
        public object SetDefault(object key) {
            return DictOps.SetDefault(this, key);
        }

        [PythonName("setdefault")]
        public object SetDefault(object key, object defaultValue) {
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
        public List Items() {
            return DictOps.Items(this);
        }

        [PythonName("iteritems")]
        public IEnumerator IterItems() {
            return DictOps.IterItems(this);
        }

        [PythonName("iterkeys")]
        public IEnumerator IterKeys() {
            return DictOps.IterKeys(this);
        }

        [PythonName("itervalues")]
        public IEnumerator IterValues() {
            return DictOps.IterValues(this);
        }

        [PythonName("update")]
        public void Update() {
        }

        [PythonName("update")]
        public void Update(object b) {
            DictOps.Update(this, b);
        }

        [PythonName("fromkeys")]
        internal static object FromKeys(object[] keys) {
            Dict ret = new Dict();
            for (int i = 0; i < keys.Length; i++) {
                ret.Add(keys[i], null);
            }
            return ret;
        }

        [PythonName("fromkeysany")]
        private static object fromkeysAny(DynamicType cls, object o, object value) {
            object ret = MakeDict(cls);
            if (ret.GetType() == typeof(Dict)) {
                Dict dr = ret as Dict;
                IEnumerator i = Ops.GetEnumerator(o);
                while (i.MoveNext()) {
                    dr[i.Current] = value;
                }
            } else {
                // slow path - user defined dictionary.
                IEnumerator i = Ops.GetEnumerator(o);
                while (i.MoveNext()) {
                    Ops.SetIndex(ret, i.Current, value);
                }
            }
            return ret;
        }

        [PythonClassMethod("fromkeys")]
        public static object FromKeys(DynamicType cls, object seq) {
            return FromKeys(cls, seq, null);
        }

        [PythonClassMethod("fromkeys")]
        public static object FromKeys(DynamicType cls, object seq, object value) {
            XRange xr = seq as XRange;
            if (xr != null) {
                int n = xr.GetLength();
                object ret = cls.Call();
                if (ret.GetType() == typeof(Dict)) {
                    Dict dr = ret as Dict;
                    for (int i = 0; i < n; i++) {
                        dr[xr[i]] = value;
                    }
                } else {
                    // slow path, user defined dict
                    for (int i = 0; i < n; i++) {
                        Ops.SetIndex(ret, xr[i], value);
                    }
                }
                return ret;
            }
            return fromkeysAny(cls, seq, value);
        }

        #endregion

        #region IComparable Members

        int IComparable.CompareTo(object obj) {
            IDictionary<object, object> other = obj as IDictionary<object, object>;
            // CompareTo is allowed to throw (string, int, etc... all do it if they don't get a matching type)
            if (other == null) throw Ops.TypeError("CompareTo argument must be a Dictionary");

            return DictOps.CompareTo(this, other);
        }

        #endregion

        #region ICloneable Members

        [PythonName("copy")]
        public object Clone() {
            return new Dict(new Dictionary<object, object>(data));
        }

        #endregion

        #region IRichEquality Members

        [PythonName("__hash__")]
        public object RichGetHashCode() {
            throw Ops.TypeErrorForUnhashableType("dict");
        }

        [PythonName("__eq__")]
        public object RichEquals(object other) {
            IDictionary<object, object> oth = null;
            if (!(other is Dict || other is CustomSymbolDict || other is FieldIdDict))
                return Ops.NotImplemented;

            oth = (IDictionary<object, object>)other;
            Debug.Assert(oth != null);
            Debug.Assert(oth is IRichEquality);

            if (oth.Count != Count) return Ops.FALSE;

            List myKeys;
            lock (this) myKeys = this.keys();

            foreach (object o in myKeys) {
                object res;
                if (!oth.TryGetValue(o, out res) || !Ops.EqualRetBool(res, this[o])) return Ops.FALSE;
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

        #region IDictionary Members

        void IDictionary.Add(object key, object value) {
            this[key] = value;
        }

        void IDictionary.Clear() {
            DictOps.Clear(this);
        }

        bool IDictionary.Contains(object key) {
            return ContainsKey(key);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator() {
            return data.GetEnumerator();
        }

        bool IDictionary.IsFixedSize {
            get { return false; }
        }

        bool IDictionary.IsReadOnly {
            get { return false; }
        }

        ICollection IDictionary.Keys {
            get { return this.keys(); }
        }

        void IDictionary.Remove(object key) {
            data.Remove(key);
        }

        ICollection IDictionary.Values {
            get { return values(); }
        }

        object IDictionary.this[object key] {
            get {
                return data[key];
            }
            set {
                data[key] = value;
            }
        }

        #endregion

        #region ICollection Members

        void ICollection.CopyTo(Array array, int index) {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        int ICollection.Count {
            get { return data.Count; }
        }

        bool ICollection.IsSynchronized {
            get { return false; }
        }

        object ICollection.SyncRoot {
            get { return null; }
        }

        #endregion

        #region ICodeFormattable Members

        [PythonName("__repr__")]
        public virtual string ToCodeString() {
            return ToString();
        }

        #endregion

        #region Fast Attribute Access Support
        /* IAttributesDictionary is implemented on our built-in
         * dictionaries to allow users to assign dictionaries into
         * classes.  These dictionaries will resolve their key via
         * the field table, but only get used when the user does
         * explicit dictionary assignment.
         *
         */

        #region IAttributesDictionary Members

        void IAttributesDictionary.Add(SymbolId key, object value) {
            Add(SymbolTable.IdToString(key), value);
        }

        bool IAttributesDictionary.ContainsKey(SymbolId key) {
            return ContainsKey(SymbolTable.IdToString(key));
        }

        bool IAttributesDictionary.Remove(SymbolId key) {
            return Remove(SymbolTable.IdToString(key));
        }

        bool IAttributesDictionary.TryGetValue(SymbolId key, out object value) {
            return TryGetValue(SymbolTable.IdToString(key), out value);
        }

        object IAttributesDictionary.this[SymbolId key] {
            get {
                return this[SymbolTable.IdToString(key)];
            }
            set {
                this[SymbolTable.IdToString(key)] = value;
            }
        }

        public IDictionary<SymbolId, object> SymbolAttributes {
            get {
                Dictionary<SymbolId, object> d = new Dictionary<SymbolId, object>();
                foreach (KeyValuePair<object, object> name in data) {
                    string stringKey = name.Key as string;
                    if (stringKey == null) continue;
                    d.Add(SymbolTable.StringToId(stringKey), name.Value);
                }
                return d;
            }
        }

        public void AddObjectKey(object name, object value) { Add(name, value); }
        public bool TryGetObjectValue(object name, out object value) { return TryGetValue(name, out value); }
        public bool RemoveObjectKey(object name) { return Remove(name); }
        public bool ContainsObjectKey(object name) { return ContainsKey(name); }
        public IDictionary<object, object> AsObjectKeyedDictionary() { return this; }

        #endregion

        #endregion

        #region IRichComparable Members

        [PythonName("__cmp__")]
        public object CompareTo(object other) {
            IDictionary<object, object> oth = other as IDictionary<object, object>;
            // CompareTo is allowed to throw (string, int, etc... all do it if they don't get a matching type)
            if (oth == null) {
                object len, iteritems;
                if(!Ops.TryGetAttr(other, SymbolTable.Length, out len) ||
                    !Ops.TryGetAttr(other, SymbolTable.StringToId("iteritems"), out iteritems)) {
                    return Ops.NotImplemented;
                }

                // user-defined dictionary...
                int lcnt = this.Count;
                int rcnt = Converter.ConvertToInt32(Ops.Call(len));

                if (lcnt != rcnt) return lcnt > rcnt ? 1 : -1;


                return DictOps.CompareToWorker(this, rcnt, new List(Ops.Call(iteritems)));
            }

            return DictOps.CompareTo(this, oth);
        }

        public object GreaterThan(object other) {
            object res = CompareTo(other);
            if (res == Ops.NotImplemented) return res;
            return (int)res > 0 ? Ops.TRUE : Ops.FALSE;
        }

        public object LessThan(object other) {
            object res = CompareTo(other);
            if (res == Ops.NotImplemented) return res;
            return (int)res < 0 ? Ops.TRUE : Ops.FALSE;
        }

        public object GreaterThanOrEqual(object other) {
            object res = CompareTo(other);
            if (res == Ops.NotImplemented) return res;
            return (int)res >= 0 ? Ops.TRUE : Ops.FALSE;
        }

        public object LessThanOrEqual(object other) {
            object res = CompareTo(other);
            if (res == Ops.NotImplemented) return res;
            return (int)res <= 0 ? Ops.TRUE : Ops.FALSE;
        }

        #endregion
    }

    [PythonType(typeof(Dict))]
    internal class EnvironmentDictionary : Dict {
        public EnvironmentDictionary() : base(Environment.GetEnvironmentVariables()) { }

        public override object this[object key] {
            set {
                data[DictOps.NullToObj(key)] = value;

                string s1 = key as string;
                string s2 = value as string;
                if (s1 != null && s2 != null)
                    Environment.SetEnvironmentVariable(s1, s2);
            }
        }

        [PythonName("__delitem__")]
        public override void DeleteItem(object key) {
            base.DeleteItem(key);

            string s = key as string;
            if (s != null)
                Environment.SetEnvironmentVariable(s, string.Empty);
        }
    }

    internal static class DictOps {
        static object nullObject = new object();

        public static object GetIndex(IDictionary<object, object> self, object key) {
            return GetIndex(self, key, null);
        }

        public static object GetIndex(IDictionary<object, object> self, object key, object defaultValue) {
            object ret;
            if (self.TryGetValue(key, out ret)) return ret;
            return defaultValue;
        }

        public static object SetDefault(IDictionary<object, object> self, object key) {
            return SetDefault(self, key, null);
        }

        public static object SetDefault(IDictionary<object, object> self, object key, object defaultValue) {
            object ret;
            if (self.TryGetValue(key, out ret)) return ret;
            self[key] = defaultValue;
            return defaultValue;
        }


        public static void DelIndex(IDictionary<object, object> self, object key) {
            if (!self.Remove(key)) {
                throw Ops.KeyError("'{0}'", key);
            }
        }

        public static int Length(IDictionary<object, object> self) {
            return self.Count;
        }

        public static bool Contains(IDictionary<object, object> self, object value) {
            return self.ContainsKey(value);
        }

        public static void Clear(IDictionary<object, object> self) {
            self.Clear();
        }

        public static object HasKey(IDictionary<object, object> self, object key) {
            return Ops.Bool2Object(self.ContainsKey(key));
        }

        public static object Pop(IDictionary<object, object> self, object key) {
            //??? perf won't match expected Python perf
            object ret;
            if (self.TryGetValue(key, out ret)) {
                self.Remove(key);
                return ret;
            } else {
                throw Ops.KeyError("'{0}'", key);
            }
        }

        public static object Pop(IDictionary<object, object> self, object key, object defaultValue) {
            //??? perf won't match expected Python perf
            object ret;
            if (self.TryGetValue(key, out ret)) {
                self.Remove(key);
                return ret;
            } else {
                return defaultValue;
            }
        }

        public static Tuple PopItem(IDictionary<object, object> self) {
            IEnumerator<KeyValuePair<object, object>> ie = self.GetEnumerator();
            if (ie.MoveNext()) {
                object key = ie.Current.Key;
                object val = ie.Current.Value;
                self.Remove(key);
                return Tuple.MakeTuple(key, val);
            }
            throw Ops.KeyError("dictionary is empty");
        }

        public static List Items(IDictionary<object, object> self) {
            List ret = List.MakeEmptyList(self.Count);
            foreach (KeyValuePair<object, object> kv in self) {
                ret.AddNoLock(Tuple.MakeTuple(kv.Key, kv.Value));
            }
            return ret;
        }

        public static List Keys(IDictionary<object, object> self) {
            List l = List.Make(self.Keys);
            for (int i = 0; i < l.Count; i++) {
                if (l[i] == nullObject) {
                    l[i] = DictOps.ObjToNull(l[i]);
                    break;
                }
            }
            return l;
        }

        public static List Values(IDictionary<object, object> self) {
            return List.Make(self.Values);
        }

        public static bool AddKeyValue(IDictionary<object, object> self, object o) {
            IEnumerator i = Ops.GetEnumerator(o); //c.GetEnumerator();
            if (i.MoveNext()) {
                object key = i.Current;
                if (i.MoveNext()) {
                    object value = i.Current;
                    self[key] = value;

                    return !i.MoveNext();
                }
            }
            return false;
        }


        public static void Update(IDictionary<object, object> self, object b) {
            object keysFunc;
            IDictionary dict = b as IDictionary;
            if (dict != null) {
                IDictionaryEnumerator e = dict.GetEnumerator();
                while (e.MoveNext()) {
                    self[DictOps.NullToObj(e.Key)] = e.Value;
                }
            } else if (Ops.TryGetAttr(b, SymbolTable.Keys, out keysFunc)) {
                // user defined dictionary
                IEnumerator i = Ops.GetEnumerator(Ops.Call(keysFunc));
                while (i.MoveNext()) {
                    self[DictOps.NullToObj(i.Current)] = Ops.GetIndex(b, i.Current);
                }
            } else {
                // list of lists (key/value pairs), list of tuples,
                // tuple of tuples, etc...
                IEnumerator i = Ops.GetEnumerator(b);
                int index = 0;
                while (i.MoveNext()) {
                    if (!AddKeyValue(self, i.Current)) {
                        throw Ops.ValueError("dictionary update sequence element #{0} has bad length; 2 is required", index);
                    }
                    index++;
                }
            }
        }

        public static IEnumerator IterItems(IDictionary<object, object> self) {
            return Items(self).GetEnumerator();
        }

        public static IEnumerator IterKeys(IDictionary<object, object> self) {
            return Keys(self).GetEnumerator();
        }

        public static IEnumerator IterValues(IDictionary<object, object> self) {
            return Values(self).GetEnumerator();
        }

        public static string ToString(IDictionary<object, object> self) {
            StringBuilder buf = new StringBuilder();
            buf.Append("{");
            bool first = true;
            foreach (KeyValuePair<object, object> kv in self) {
                if (first) first = false;
                else buf.Append(", ");

                if (kv.Key == nullObject)
                    buf.Append("None");
                else
                    buf.Append(Ops.StringRepr(kv.Key));
                buf.Append(": ");
                buf.Append(Ops.StringRepr(kv.Value));
            }
            buf.Append("}");
            return buf.ToString();
        }

        public static object NullToObj(object o) {
            if (o == null) return nullObject;
            return o;
        }

        public static object ObjToNull(object o) {
            if (o == nullObject) return null;
            return o;
        }

        public static int CompareTo(IDictionary<object, object> left, IDictionary<object, object> right) {
            int lcnt = left.Count;
            int rcnt = right.Count;

            if (lcnt != rcnt) return lcnt > rcnt ? 1 : -1;

            List ritems = DictOps.Items(right);
            return CompareToWorker(left, rcnt, ritems);
        }

        public static int CompareToWorker(IDictionary<object, object> left, int rightLen, List ritems) {
            List litems = DictOps.Items(left);

            litems.Sort();
            ritems.Sort();

            return litems.CompareTo(ritems);
        }
    }

    public class DictKeyValueEnumerator : IEnumerator, IEnumerator<KeyValuePair<object, object>> {
        IEnumerator<KeyValuePair<object, object>> innerEnum;
        readonly int size;
        int pos;

        public DictKeyValueEnumerator(IDictionary<object, object> dict) {
            innerEnum = dict.GetEnumerator();
            size = dict.Count;
            pos = -1;
        }

        public bool MoveNext() {
            try {
                bool ret = innerEnum.MoveNext();
                if (ret) pos++;
                return ret;
            } catch (InvalidOperationException) {
                pos = size - 1;
                throw Ops.RuntimeError("dictionary changed size during iteration");
            }
        }

        public void Reset() {
            innerEnum.Reset();
            pos = -1;
        }

        object IEnumerator.Current {
            get {
                return new KeyValuePair<object, object>(DictOps.ObjToNull(innerEnum.Current.Key), innerEnum.Current.Value);
            }
        }
        public KeyValuePair<object, object> Current {
            get {
                return new KeyValuePair<object, object>(DictOps.ObjToNull(innerEnum.Current.Key), innerEnum.Current.Value);
            }
        }

        public void Dispose() {
            innerEnum.Dispose();
        }

        [PythonName("__iter__")]
        public object GetEnumerator() {
            return this;
        }

        [PythonName("__len__")]
        public int GetLength() {
            return size - pos - 1;
        }
    }

    public class DictionaryKeyCollection : ICollection<object> {
        private ICollection<object> items;
        private Dict dict;

        public DictionaryKeyCollection(Dict dictionary, ICollection<object> collection) {
            items = collection;
            dict = dictionary;
        }

        #region ICollection<object> Members

        public void Add(object item) {
            items.Add(DictOps.NullToObj(item));
        }

        public void Clear() {
            items.Clear();
        }

        public bool Contains(object item) {
            return items.Contains(DictOps.NullToObj(item));
        }

        public void CopyTo(object[] array, int arrayIndex) {
            int i = 0;
            foreach(object o in items) {
                array[i + arrayIndex] = DictOps.ObjToNull(o);
                i++;
            }
        }

        public int Count {
            get { return items.Count; }
        }

        public bool IsReadOnly {
            get { return items.IsReadOnly; }
        }

        public bool Remove(object item) {
            return items.Remove(DictOps.NullToObj(item));
        }

        #endregion

        #region IEnumerable<object> Members

        public IEnumerator<object> GetEnumerator() {
            return new DictionaryKeyEnumerator(dict.data);
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator() {
            return new DictionaryKeyEnumerator(dict.data);
        }

        #endregion
    }
    /// <summary>
    /// Note: 
    ///   IEnumerator innerEnum = Dictionary<K,V>.KeysCollections.GetEnumerator();
    ///   innerEnum.MoveNext() will throw InvalidOperation even if the values get changed,
    ///   which is supported in python
    /// </summary>
    public class DictionaryKeyEnumerator : IEnumerator, IEnumerator<object> {
        readonly int size;
        readonly Dictionary<object, object> dict;
        readonly object[] keys;
        int pos;

        public DictionaryKeyEnumerator(Dictionary<object, object> dict) {
            this.dict = dict;
            this.size = dict.Count;
            keys = new object[size];
            this.dict.Keys.CopyTo(keys, 0);
            this.pos = -1;
        }

        public bool MoveNext() {
            if (size != dict.Count) {
                pos = size - 1; // make the length 0
                throw Ops.RuntimeError("dictionary changed size during iteration");
            }
            if (pos + 1 < size) {
                pos++;
                return true;
            } else {
                return false;
            }
        }

        public void Reset() {
            pos = -1;
        }

        public object Current {
            get {
                return DictOps.ObjToNull(keys[pos]);
            }
        }

        public void Dispose() {
        }

        [PythonName("__iter__")]
        public object GetEnumerator() {
            return this;
        }

        [PythonName("__len__")]
        public int GetLength() {
            return size - pos - 1;
        }
    }

}