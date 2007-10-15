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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using SpecialNameAttribute = System.Runtime.CompilerServices.SpecialNameAttribute;

using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Types;

using IronPython.Compiler;
using IronPython.Runtime.Types;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Actions;

namespace IronPython.Runtime {

    [PythonType("dict")]
    public class PythonDictionary : IMapping, IDictionary<object, object>, IValueEquality,
        IDictionary, ICodeFormattable, IAttributesCollection
#if !SILVERLIGHT
        , ICloneable
#endif
    {
        internal static readonly IEqualityComparer<object> Comparer = new PythonObjectComparer();
        private static object DefaultGetItem;   // our cached __getitem__ method

        internal Dictionary<object, object> data;

        internal static object MakeDict(DynamicType cls) {
            if (cls == TypeCache.Dict) return new PythonDictionary();
            return PythonCalls.Call(cls);
        }

        #region Constructors
        public PythonDictionary() {
            this.data = new Dictionary<object, object>(Comparer);
        }

        internal PythonDictionary(object o)
            : this() {
            Update(o);
        }

        internal PythonDictionary(int size) { data = new Dictionary<object, object>(size, Comparer); }

        [PythonName("__init__")]
        public void Initialize(object o, [ParamDictionary] IAttributesCollection kwArgs) {
            Update(o);
            Update(kwArgs);
        }

        [PythonName("__init__")]
        public void Initialize([ParamDictionary] IAttributesCollection kwArgs) {
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
        public override string ToString() {
            return DictionaryOps.__str__(this);
        }

        public override bool Equals(object obj) {
            return base.Equals(obj);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        #endregion

        #region IDictionary<object,object> Members

        public void Add(object key, object value) {
            lock (this) data.Add(BaseSymbolDictionary.NullToObj(key), value);
        }

        public virtual bool ContainsKey(object key) {
            lock (this) return data.ContainsKey(BaseSymbolDictionary.NullToObj(key));
        }

        public ICollection<object> Keys {
            get { lock (this) return new DictionaryKeyCollection(this, data.Keys); }
        }

        public bool Remove(object key) {
            lock (this) return data.Remove(BaseSymbolDictionary.NullToObj(key));
        }

        public bool TryGetValue(object key, out object value) {
            lock (this) return data.TryGetValue(BaseSymbolDictionary.NullToObj(key), out value);
        }

        bool IMapping.TryGetValue(object key, out object value) {
            if (DictionaryOps.TryGetValueVirtual(DefaultContext.Default, this, key, ref DefaultGetItem, out value)) {
                return true;
            }

            // call Dict.TryGetValue to get the real value.
            return this.TryGetValue(key, out value);
        }

        public ICollection<object> Values {
            get { lock (this) return data.Values; }
        }

        #endregion

        #region ICollection<KeyValuePair<object,object>> Members

        public void Add(KeyValuePair<object, object> item) {
            lock (this) data[BaseSymbolDictionary.NullToObj(item.Key)] = item.Value;
        }

        [PythonName("clear")]
        public void Clear() {
            lock (this) data.Clear();
        }

        public bool Contains(KeyValuePair<object, object> item) {
            lock (this) return data.ContainsKey(BaseSymbolDictionary.NullToObj(item.Key));
        }

        public void CopyTo(KeyValuePair<object, object>[] array, int arrayIndex) {
            lock (this) {
                Dictionary<object, object>.Enumerator ie = data.GetEnumerator();
                while (ie.MoveNext()) {
                    array[arrayIndex++] = new KeyValuePair<object, object>(BaseSymbolDictionary.ObjToNull(ie.Current.Key), ie.Current.Value);
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
            lock (this) return data.Remove(BaseSymbolDictionary.NullToObj(item.Key));
        }

        #endregion

        #region IEnumerable<KeyValuePair<object,object>> Members

        IEnumerator<KeyValuePair<object, object>> IEnumerable<KeyValuePair<object, object>>.GetEnumerator() {
            return new DictKeyValueEnumerator(data);
        }

        #endregion

        #region IEnumerable Members

        [PythonName("__iter__")]
        public IEnumerator GetEnumerator() {
            return new DictionaryKeyEnumerator(data);
        }

        #endregion

        #region IMapping Members

        [PythonName("get")]
        public object GetValue(object key) {
            return DictionaryOps.get(this, key);
        }

        [PythonName("get")]
        public object GetValue(object key, object defaultValue) {
            return DictionaryOps.get(this, key, defaultValue);
        }

        public virtual object this[params object[] key] {
            get {
                if (key == null) return this[(object)null];

                if (key.Length == 0) {
                    throw PythonOps.TypeError("__getitem__() takes exactly one argument (0 given)");
                }                

                return this[PythonTuple.MakeTuple(key)];
            }
            set {
                if (key == null) {
                    this[(object)null] = value;
                    return;
                }

                if (key.Length == 0) {
                    throw PythonOps.TypeError("__setitem__() takes exactly two argument (1 given)");
                }

                this[PythonTuple.MakeTuple(key)] = value;
            }
        }

        public virtual object this[object key] {
            [PythonName("__getitem__")]
            get {
                object realKey = BaseSymbolDictionary.NullToObj(key);
                object ret;
                lock (this) if (TryGetValue(realKey, out ret)) return ret;

                // we need to manually look up a slot to get the correct behavior when
                // the __missing__ function is declared on a sub-type which is an old-class
                if (DynamicHelpers.GetDynamicType(this).TryInvokeBinaryOperator(DefaultContext.Default,
                    Operators.Missing,
                    this,
                    key,
                    out ret)) {
                    return ret;
                }

                throw PythonOps.KeyError(key);
            }
            [PythonName("__setitem__")]
            set {
                lock (this) data[BaseSymbolDictionary.NullToObj(key)] = value;
            }
        }

        [PythonName("__delitem__")]
        public virtual bool DeleteItem(object key) {
            DictionaryOps.__delitem__(this, key);
            return true;
        }

        #endregion

        #region IPythonContainer Members

        [PythonName("__len__")]
        public virtual int GetLength() {
            return DictionaryOps.__len__(this);
        }

        public bool ContainsValue(object value) {
            return DictionaryOps.__contains__(this, value);
        }

        #endregion

        #region Python dict implementation

        [PythonName("has_key")]
        public bool HasKey(object key) {
            return DictionaryOps.has_key(this, key);
        }

        [PythonName("pop")]
        public object Pop(object key) {
            return DictionaryOps.pop(this, key);
        }

        [PythonName("pop")]
        public object Pop(object key, object defaultValue) {
            return DictionaryOps.pop(this, key, defaultValue);
        }

        [PythonName("popitem")]
        public PythonTuple PopItem() {
            return DictionaryOps.popitem(this);
        }

        [PythonName("setdefault")]
        public object SetDefault(object key) {
            return DictionaryOps.setdefault(this, key);
        }

        [PythonName("setdefault")]
        public object SetDefault(object key, object defaultValue) {
            return DictionaryOps.setdefault(this, key, defaultValue);
        }

        [PythonName("keys")]
        public List KeysAsList() {
            return DictionaryOps.keys(this);
        }

        [PythonName("values")]
        public List ValuesAsList() {
            return DictionaryOps.values(this);
        }

        [PythonName("items")]
        public List Items() {
            return DictionaryOps.items(this);
        }

        [PythonName("iteritems")]
        public IEnumerator IterItems() {
            return DictionaryOps.iteritems(this);
        }

        [PythonName("iterkeys")]
        public IEnumerator IterKeys() {
            return DictionaryOps.iterkeys(this);
        }

        [PythonName("itervalues")]
        public IEnumerator IterValues() {
            return DictionaryOps.itervalues(this);
        }

        [PythonName("update")]
        public void Update() {
        }

        [PythonName("update")]
        public void Update([ParamDictionary]IAttributesCollection b) {
            DictionaryOps.update(this, b);
        }

        [PythonName("update")]
        public void Update(object b) {
            DictionaryOps.update(this, b);
        }

        internal static object FromKeys(object[] keys) {
            PythonDictionary ret = new PythonDictionary();
            for (int i = 0; i < keys.Length; i++) {
                ret.Add(keys[i], null);
            }
            return ret;
        }

        [PythonName("fromkeysany")]
        private static object fromkeysAny(DynamicType cls, object o, object value) {
            object ret = MakeDict(cls);
            if (ret.GetType() == typeof(PythonDictionary)) {
                PythonDictionary dr = ret as PythonDictionary;
                IEnumerator i = PythonOps.GetEnumerator(o);
                while (i.MoveNext()) {
                    dr[i.Current] = value;
                }
            } else {
                // slow path - user defined dictionary.
                IEnumerator i = PythonOps.GetEnumerator(o);
                while (i.MoveNext()) {
                    PythonOps.SetIndex(ret, i.Current, value);
                }
            }
            return ret;
        }

        private static FastDynamicSite<DynamicType, object> _fromkeysSite;
        [PythonClassMethod("fromkeys")]
        public static object FromKeys(CodeContext context, DynamicType cls, object seq) {
            return FromKeys(context, cls, seq, null);
        }

        [PythonClassMethod("fromkeys")]
        public static object FromKeys(CodeContext context, DynamicType cls, object seq, object value) {            
            XRange xr = seq as XRange;
            if (xr != null) {
                if (_fromkeysSite == null) {
                    _fromkeysSite = RuntimeHelpers.CreateSimpleCallSite<DynamicType, object>(DefaultContext.Default);
                }

                int n = xr.GetLength();
                object ret = _fromkeysSite.Invoke(cls);
                if (ret.GetType() == typeof(PythonDictionary)) {
                    PythonDictionary dr = ret as PythonDictionary;
                    for (int i = 0; i < n; i++) {
                        dr[xr[i]] = value;
                    }
                } else {
                    // slow path, user defined dict
                    for (int i = 0; i < n; i++) {
                        PythonOps.SetIndex(ret, xr[i], value);
                    }
                }
                return ret;
            }
            return fromkeysAny(cls, seq, value);
        }

        #endregion

        #region ICloneable Members

        [PythonName("copy")]
        public object Clone() {
            return new PythonDictionary(new Dictionary<object, object>(data));
        }

        #endregion

        #region Rich Equality Helpers
        
        // Dictionary has an odd not-implemented check to support custom dictionaries and therefore
        // needs a custom __eq__ / __ne__ implementation.

        [return: MaybeNotImplemented]
        [SpecialName, PythonName("__eq__")]
        public object RichEquals(object other) {
            if (!(other is PythonDictionary || other is CustomSymbolDictionary || other is SymbolDictionary))
                return PythonOps.NotImplemented;

            return RuntimeHelpers.BooleanToObject(ValueEquals(other));
        }

        [return: MaybeNotImplemented]
        [SpecialName, PythonName("__ne__")]
        public object RichNotEquals(object other) {
            object res = RichEquals(other);
            if (res != PythonOps.NotImplemented) return PythonOps.Not(res);

            return res;
        }

        #endregion

        #region IValueEquality Members

        public int GetValueHashCode() {
            throw PythonOps.TypeErrorForUnhashableType("dict");
        }

        public bool ValueEquals(object other) {
            IDictionary<object, object> oth = other as IDictionary<object, object>;
            if (oth == null) return false;
            if (oth.Count != Count) return false;

            // we cannot call Compare here and compare against zero because Python defines
            // value equality as working even if the keys/values are unordered.
            List myKeys;
            lock (this) myKeys = this.KeysAsList();

            foreach (object o in myKeys) {
                object res;
                if (!oth.TryGetValue(o, out res)) return false;

                CompareUtil.Push(res);
                try {
                    if (!PythonOps.EqualRetBool(res, this[o])) return false;
                } finally {
                    CompareUtil.Pop(res);
                }
            }
            return true;
        }

        public bool ValueNotEquals(object other) {
            return !ValueEquals(other);
        }

        #endregion

        #region IDictionary Members

        void IDictionary.Add(object key, object value) {
            this[key] = value;
        }

        void IDictionary.Clear() {
            DictionaryOps.clear(this);
        }

        [PythonName("__contains__")]
        public virtual bool Contains(object key) {
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
            get { return this.KeysAsList(); }
        }

        void IDictionary.Remove(object key) {
            data.Remove(key);
        }

        ICollection IDictionary.Values {
            get { return ValuesAsList(); }
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

        [SpecialName, PythonName("__repr__")]
        public virtual string ToCodeString(CodeContext context) {
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

        void IAttributesCollection.Add(SymbolId name, object value) {
            Add(SymbolTable.IdToString(name), value);
        }

        bool IAttributesCollection.ContainsKey(SymbolId name) {
            return ContainsKey(SymbolTable.IdToString(name));
        }

        bool IAttributesCollection.Remove(SymbolId name) {
            return Remove(SymbolTable.IdToString(name));
        }

        bool IAttributesCollection.TryGetValue(SymbolId name, out object value) {
            return ((IMapping)this).TryGetValue(SymbolTable.IdToString(name), out value);
        }

        object IAttributesCollection.this[SymbolId name] {
            get {
                return this[SymbolTable.IdToString(name)];
            }
            set {
                this[SymbolTable.IdToString(name)] = value;
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

        [SpecialName, PythonName("__cmp__")]
        [return: MaybeNotImplemented]
        public object CompareTo(CodeContext context, object other) {
            IDictionary<object, object> oth = other as IDictionary<object, object>;
            // CompareTo is allowed to throw (string, int, etc... all do it if they don't get a matching type)
            if (oth == null) {
                object len, iteritems;
                if (!PythonOps.TryGetBoundAttr(context, other, Symbols.Length, out len) ||
                    !PythonOps.TryGetBoundAttr(context, other, SymbolTable.StringToId("iteritems"), out iteritems)) {
                    return PythonOps.NotImplemented;
                }

                // user-defined dictionary...
                int lcnt = this.Count;
                int rcnt = Converter.ConvertToInt32(PythonOps.CallWithContext(context, len));

                if (lcnt != rcnt) return lcnt > rcnt ? 1 : -1;

                return DictionaryOps.CompareToWorker(this, new List(PythonOps.CallWithContext(context, iteritems)));
            }

            CompareUtil.Push(this, oth);
            try {
                return DictionaryOps.CompareTo(this, oth);
            } finally {
                CompareUtil.Pop(this, oth);
            }
        }

    }

#if !SILVERLIGHT // environment variables not available
    [PythonType(typeof(PythonDictionary))]
    internal class EnvironmentDictionary : PythonDictionary {
        public EnvironmentDictionary() : base(Environment.GetEnvironmentVariables()) { }

        public override object this[object key] {
            set {
                data[BaseSymbolDictionary.NullToObj(key)] = value;

                string s1 = key as string;
                string s2 = value as string;
                if (s1 != null && s2 != null)
                    Environment.SetEnvironmentVariable(s1, s2);
            }
        }

        [PythonName("__delitem__")]
        public override bool DeleteItem(object key) {
            bool isDeleted = base.DeleteItem(key);

            string s = key as string;
            if (s != null)
                Environment.SetEnvironmentVariable(s, string.Empty);

            return isDeleted;
        }
    }
#endif
   
    [PythonType("dictionary-itemiterator")]
    public sealed class DictKeyValueEnumerator : IEnumerator, IEnumerator<KeyValuePair<object, object>> {
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
                throw PythonOps.RuntimeError("dictionary changed size during iteration");
            }
        }

        public void Reset() {
            innerEnum.Reset();
            pos = -1;
        }

        object IEnumerator.Current {
            get {
                return new KeyValuePair<object, object>(BaseSymbolDictionary.ObjToNull(innerEnum.Current.Key), innerEnum.Current.Value);
            }
        }
        public KeyValuePair<object, object> Current {
            get {
                return new KeyValuePair<object, object>(BaseSymbolDictionary.ObjToNull(innerEnum.Current.Key), innerEnum.Current.Value);
            }
        }

        public void Dispose() {
            innerEnum.Dispose();
        }

        [PythonName("__iter__")]
        public object GetEnumerator() {
            return this;
        }

        [SpecialName, PythonName("__len__")]
        public int GetLength() {
            return size - pos - 1;
        }
    }

    public class DictionaryKeyCollection : ICollection<object> {
        private ICollection<object> _items;
        private PythonDictionary _dict;

        public DictionaryKeyCollection(PythonDictionary dictionary, ICollection<object> collection) {
            _items = collection;
            _dict = dictionary;
        }

        #region ICollection<object> Members

        public void Add(object item) {
            _items.Add(BaseSymbolDictionary.NullToObj(item));
        }

        public void Clear() {
            _items.Clear();
        }

        public bool Contains(object item) {
            return _items.Contains(BaseSymbolDictionary.NullToObj(item));
        }

        public void CopyTo(object[] array, int arrayIndex) {
            int i = 0;
            foreach (object o in _items) {
                array[i + arrayIndex] = BaseSymbolDictionary.ObjToNull(o);
                i++;
            }
        }

        public int Count {
            get { return _items.Count; }
        }

        public bool IsReadOnly {
            get { return _items.IsReadOnly; }
        }

        public bool Remove(object item) {
            return _items.Remove(BaseSymbolDictionary.NullToObj(item));
        }

        #endregion

        #region IEnumerable<object> Members

        public IEnumerator<object> GetEnumerator() {
            return new DictionaryKeyEnumerator(_dict.data);
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator() {
            return new DictionaryKeyEnumerator(_dict.data);
        }

        #endregion
    }
    /// <summary>
    /// Note: 
    ///   IEnumerator innerEnum = Dictionary&lt;K,V&gt;.KeysCollections.GetEnumerator();
    ///   innerEnum.MoveNext() will throw InvalidOperation even if the values get changed,
    ///   which is supported in python
    /// </summary>
    [PythonType("dictionary-keyiterator")]
    public sealed class DictionaryKeyEnumerator : IEnumerator, IEnumerator<object> {
        private readonly int _size;
        private readonly IDictionary<object, object> _dict;
        private readonly object[] _keys;
        private int _pos;

        public DictionaryKeyEnumerator(IDictionary<object, object> dict) {
            _dict = dict;
            _size = dict.Count;
            _keys = new object[_size];
            _dict.Keys.CopyTo(_keys, 0);
            _pos = -1;
        }

        public bool MoveNext() {
            if (_size != _dict.Count) {
                _pos = _size - 1; // make the length 0
                throw PythonOps.RuntimeError("dictionary changed size during iteration");
            }
            if (_pos + 1 < _size) {
                _pos++;
                return true;
            } else {
                return false;
            }
        }

        public void Reset() {
            _pos = -1;
        }

        public object Current {
            get {
                return BaseSymbolDictionary.ObjToNull(_keys[_pos]);
            }
        }

        public void Dispose() {
        }

        [PythonName("__iter__")]
        public object GetEnumerator() {
            return this;
        }

        [SpecialName, PythonName("__len__")]
        public int GetLength() {
            return _size - _pos - 1;
        }
    }

}
