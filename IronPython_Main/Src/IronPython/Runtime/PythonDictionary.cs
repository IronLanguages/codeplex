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
using System.Diagnostics;
using System.Scripting;
using System.Scripting.Actions;
using System.Scripting.Runtime;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

namespace IronPython.Runtime {

    [PythonSystemType("dict")]
    public class PythonDictionary : IDictionary<object, object>, IValueEquality,
        IDictionary, ICodeFormattable, IAttributesCollection
    {
        [MultiRuntimeAware]
        private static object DefaultGetItem;   // our cached __getitem__ method
        internal DictionaryStorage _storage;
        
        internal static object MakeDict(CodeContext/*!*/ context, PythonType cls) {
            if (cls == TypeCache.Dict) {
                return new PythonDictionary();
            }
            return PythonCalls.Call(context, cls);
        }

        #region Constructors

        public PythonDictionary() {
            _storage = new CommonDictionaryStorage();
        }

        internal PythonDictionary(DictionaryStorage storage) {
            _storage = storage;
        }

        internal PythonDictionary(IDictionary dict)
            : this() {
            foreach (DictionaryEntry de in dict) {
                this[de.Key] = de.Value;
            }
        }

        internal PythonDictionary(CodeContext/*!*/ context, object o)
            : this() {
            update(context, o);
        }

        internal PythonDictionary(int size) {
            _storage = new CommonDictionaryStorage();
        }

        internal static PythonDictionary MakeSymbolDictionary() {
            return new PythonDictionary(new SymbolIdDictionaryStorage());
        }

        internal static PythonDictionary MakeSymbolDictionary(int count) {
            return new PythonDictionary(new SymbolIdDictionaryStorage(count));
        }

        public void __init__(CodeContext/*!*/ context, object o, [ParamDictionary] IAttributesCollection kwArgs) {
            update(context, o);
            update(context, kwArgs);
        }

        public void __init__(CodeContext/*!*/ context, [ParamDictionary] IAttributesCollection kwArgs) {
            update(context, kwArgs);
        }

        public void __init__(CodeContext/*!*/ context, object o) {
            update(context, o);
        }

        public void __init__() {
        }

        #endregion

        #region IDictionary<object,object> Members

        void IDictionary<object,object>.Add(object key, object value) {
            _storage.Add(key, value);
        }

        bool IDictionary<object,object>.ContainsKey(object key) {
            return _storage.Contains(key);
        }

        ICollection<object> IDictionary<object,object>.Keys {
            get { return keys(); }
        }

        bool IDictionary<object,object>.Remove(object key) {
            try {
                __delitem__(key);
                return true;
            } catch (KeyNotFoundException) {
                return false;
            }
        }

        bool IDictionary<object,object>.TryGetValue(object key, out object value) {
            return _storage.TryGetValue(key, out value);
        }

        ICollection<object> IDictionary<object,object>.Values {
            get { return values(); }
        }

        #endregion

        #region ICollection<KeyValuePair<object,object>> Members

        void ICollection<KeyValuePair<object,object>>.Add(KeyValuePair<object, object> item) {
            _storage.Add(item.Key, item.Value);
        }

        void ICollection<KeyValuePair<object, object>>.Clear() {
            _storage.Clear();
        }

        bool ICollection<KeyValuePair<object,object>>.Contains(KeyValuePair<object, object> item) {
            return _storage.Contains(item.Key);
        }

        void ICollection<KeyValuePair<object,object>>.CopyTo(KeyValuePair<object, object>[] array, int arrayIndex) {
            _storage.GetItems().CopyTo(array, arrayIndex);
        }

        int ICollection<KeyValuePair<object,object>>.Count {
            get { return __len__(); }
        }

        bool ICollection<KeyValuePair<object,object>>.IsReadOnly {
            get { return false; }
        }

        bool ICollection<KeyValuePair<object,object>>.Remove(KeyValuePair<object, object> item) {
            return _storage.Remove(item.Key);
        }

        #endregion

        #region IEnumerable<KeyValuePair<object,object>> Members

        IEnumerator<KeyValuePair<object, object>> IEnumerable<KeyValuePair<object, object>>.GetEnumerator() {
            foreach (KeyValuePair<object, object> kvp in _storage.GetItems()) {
                yield return kvp;
            }
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator() {
            return Converter.ConvertToIEnumerator(__iter__());
        }

        public virtual object __iter__() {
            return new DictionaryKeyEnumerator(_storage);
        }

        #endregion

        #region IMapping Members

        public object get(object key) {
            return DictionaryOps.get(this, key);
        }

        public object get(object key, object defaultValue) {
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
            get {
                Debug.Assert(!(key is SymbolId));

                object ret;
                if (_storage.TryGetValue(key, out ret)) return ret;

                // we need to manually look up a slot to get the correct behavior when
                // the __missing__ function is declared on a sub-type which is an old-class
                if (PythonTypeOps.TryInvokeBinaryOperator(DefaultContext.Default,
                    this,
                    key,
                    Symbols.Missing,
                    out ret)) {
                    return ret;
                }

                throw PythonOps.KeyError(key);
            }
            set {
                Debug.Assert(!(key is SymbolId));

                _storage.Add(key, value);
            }
        }


        public virtual void __delitem__(object key) {
            if (!_storage.Remove(key)) {
                throw PythonOps.KeyError(key);
            }
        }

        public virtual void __delitem__(params object[] key) {
            if (key == null) {
                __delitem__((object)null);
            } else if (key.Length > 0) {
                __delitem__(PythonTuple.MakeTuple(key));
            } else {
                throw PythonOps.TypeError("__delitem__() takes exactly one argument (0 given)");
            }
        }

        #endregion

        #region IPythonContainer Members

        public virtual int __len__() {
            return _storage.Count;
        }

        #endregion

        #region Python dict implementation

        public void clear() {
            _storage.Clear();
        }

        public bool has_key(object key) {
            return DictionaryOps.has_key(this, key);
        }

        public object pop(object key) {
            return DictionaryOps.pop(this, key);
        }

        public object pop(object key, object defaultValue) {
            return DictionaryOps.pop(this, key, defaultValue);
        }

        public PythonTuple popitem() {
            return DictionaryOps.popitem(this);
        }

        public object setdefault(object key) {
            return DictionaryOps.setdefault(this, key);
        }

        public object setdefault(object key, object defaultValue) {
            return DictionaryOps.setdefault(this, key, defaultValue);
        }

        public virtual List keys() {
            List res = new List();
            foreach (KeyValuePair<object, object> kvp in _storage.GetItems()) {
                res.append(kvp.Key);
            }
            return res;
        }

        public virtual List values() {
            List res = new List();
            foreach (KeyValuePair<object, object> kvp in _storage.GetItems()) {
                res.append(kvp.Value);
            }
            return res;
        }

        public virtual List items() {
            List res = new List();
            foreach (KeyValuePair<object, object> kvp in _storage.GetItems()) {
                res.append(PythonTuple.MakeTuple(kvp.Key, kvp.Value));
            }
            return res;
        }

        public IEnumerator iteritems() {
            return new DictionaryItemEnumerator(_storage);
        }

        public IEnumerator iterkeys() {
            return new DictionaryKeyEnumerator(_storage);
        }

        public IEnumerator itervalues() {
            return new DictionaryValueEnumerator(_storage);
        }

        public void update() {
        }

        public void update(CodeContext/*!*/ context, [ParamDictionary]IAttributesCollection b) {
            DictionaryOps.update(context, this, b);
        }

        public void update(CodeContext/*!*/ context, object b) {
            DictionaryOps.update(context, this, b);
        }

        public void update(CodeContext/*!*/ context, object b, [ParamDictionary]IAttributesCollection f) {
            DictionaryOps.update(context, this, b);
            DictionaryOps.update(context, this, f);
        }

        private static object fromkeysAny(CodeContext/*!*/ context, PythonType cls, object o, object value) {
            PythonDictionary pyDict;
            object dict;

            if (cls == TypeCache.Dict) {
                string str;
                ICollection ic = o as ICollection;

                // creating our own dict, try and get the ideal size and add w/o locks
                if (ic != null) {
                    pyDict = new PythonDictionary(new CommonDictionaryStorage(ic.Count));
                } else if ((str = o as string) != null) {
                    pyDict = PythonDictionary.MakeSymbolDictionary(str.Length);
                } else {
                    pyDict = new PythonDictionary();
                }

                IEnumerator i = PythonOps.GetEnumerator(o);
                while (i.MoveNext()) {
                    pyDict._storage.AddNoLock(i.Current, value);
                }

                return pyDict;
            } else {
                // call the user type constructor
                dict = MakeDict(context, cls);
                pyDict = dict as PythonDictionary;
            }

            if (pyDict != null) {
                // then store all the keys with their associated value
                IEnumerator i = PythonOps.GetEnumerator(o);
                while (i.MoveNext()) {
                    pyDict[i.Current] = value;
                }
            } else {
                // slow path, cls.__new__ returned a user defined dictionary instead of a PythonDictionary.
                IEnumerator i = PythonOps.GetEnumerator(o);
                while (i.MoveNext()) {
                    PythonOps.SetIndex(dict, i.Current, value);
                }
            }

            return dict;
        }

        [PythonClassMethod]
        public static object fromkeys(CodeContext context, PythonType cls, object seq) {
            return fromkeys(context, cls, seq, null);
        }

        [PythonClassMethod]
        public static object fromkeys(CodeContext context, PythonType cls, object seq, object value) {            
            XRange xr = seq as XRange;
            if (xr != null) {
                int n = xr.__len__();
                object ret = PythonContext.GetContext(context).Call(cls);
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
            return fromkeysAny(context, cls, seq, value);
        }

        public virtual PythonDictionary copy(CodeContext/*!*/ context) {
            return new PythonDictionary(_storage.Clone());
        }

        public virtual bool __contains__(object key) {
            return _storage.Contains(key);
        }

        // Dictionary has an odd not-implemented check to support custom dictionaries and therefore
        // needs a custom __eq__ / __ne__ implementation.

        [return: MaybeNotImplemented]
        public object __eq__(object other) {
            if (!(other is PythonDictionary || other is IDictionary<object, object>))
                return NotImplementedType.Value;

            return RuntimeHelpers.BooleanToObject(((IValueEquality)this).ValueEquals(other));
        }

        [return: MaybeNotImplemented]
        public object __ne__(object other) {
            object res = __eq__(other);
            if (res != NotImplementedType.Value) return PythonOps.Not(res);

            return res;
        }

        [return: MaybeNotImplemented]
        public object __cmp__(CodeContext context, object other) {
            IDictionary<object, object> oth = other as IDictionary<object, object>;
            // CompareTo is allowed to throw (string, int, etc... all do it if they don't get a matching type)
            if (oth == null) {
                object len, iteritems;
                if (!PythonOps.TryGetBoundAttr(context, other, Symbols.Length, out len) ||
                    !PythonOps.TryGetBoundAttr(context, other, SymbolTable.StringToId("iteritems"), out iteritems)) {
                    return NotImplementedType.Value;
                }

                // user-defined dictionary...
                int lcnt = __len__();
                int rcnt = PythonContext.GetContext(context).ConvertToInt32(PythonOps.CallWithContext(context, len));

                if (lcnt != rcnt) return lcnt > rcnt ? 1 : -1;

                return DictionaryOps.CompareToWorker(context, this, new List(PythonOps.CallWithContext(context, iteritems)));
            }

            CompareUtil.Push(this, oth);
            try {
                return DictionaryOps.CompareTo(context, this, oth);
            } finally {
                CompareUtil.Pop(this, oth);
            }
        }

        #endregion
        
        #region IValueEquality Members

        int IValueEquality.GetValueHashCode() {
            throw PythonOps.TypeErrorForUnhashableType("dict");
        }

        bool IValueEquality.ValueEquals(object other) {
            if (Object.ReferenceEquals(this, other)) return true;

            IDictionary<object, object> oth = other as IDictionary<object, object>;
            if (oth == null) return false;
            if (oth.Count != __len__()) return false;

            // we cannot call Compare here and compare against zero because Python defines
            // value equality as working even if the keys/values are unordered.
            List myKeys = keys();

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

        #endregion

        #region IDictionary Members

        void IDictionary.Add(object key, object value) {
            this[key] = value;
        }

        void IDictionary.Clear() {
            DictionaryOps.clear(this);
        }

        bool IDictionary.Contains(object key) {
            return __contains__(key);
        }

        internal class DictEnumerator : IDictionaryEnumerator {
            private IEnumerator<KeyValuePair<object, object>> _enumerator;
            private bool _moved;

            public DictEnumerator(IEnumerator<KeyValuePair<object, object>> enumerator) {
                _enumerator = enumerator;
            }

            #region IDictionaryEnumerator Members

            public DictionaryEntry Entry {
                get {
                    // List<T> enumerator doesn't throw, so we need to.
                    if (!_moved) throw new InvalidOperationException();

                    return new DictionaryEntry(_enumerator.Current.Key, _enumerator.Current.Value);  
                }
            }

            public object Key {
                get { return Entry.Key; }
            }

            public object Value {
                get { return Entry.Value; }
            }

            #endregion

            #region IEnumerator Members

            public object Current {
                get { return Entry; }
            }

            public bool MoveNext() {
                if (_enumerator.MoveNext()) {
                    _moved = true;
                    return true;
                }

                _moved = false;
                return false;
            }

            public void Reset() {
                _enumerator.Reset();
                _moved = false;
            }

            #endregion
        }

        IDictionaryEnumerator IDictionary.GetEnumerator() {
            return new DictEnumerator(_storage.GetItems().GetEnumerator());
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
            ((IDictionary<object, object>)this).Remove(key);
        }

        ICollection IDictionary.Values {
            get { return values(); }
        }

        object IDictionary.this[object key] {
            get {
                object res;
                if (!_storage.TryGetValue(key, out res)) {
                    throw PythonOps.KeyError(key);
                }
                return res;
            }
            set {
                _storage.Add(key, value);
            }
        }

        #endregion

        #region ICollection Members

        void ICollection.CopyTo(Array array, int index) {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        int ICollection.Count {
            get { return __len__(); }
        }

        bool ICollection.IsSynchronized {
            get { return false; }
        }

        object ICollection.SyncRoot {
            get { return null; }
        }

        #endregion

        #region ICodeFormattable Members

        public virtual string/*!*/ __repr__(CodeContext/*!*/ context) {
            return DictionaryOps.__repr__(this);
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
            this[SymbolTable.IdToString(name)] = value;
        }

        bool IAttributesCollection.ContainsKey(SymbolId name) {
            return __contains__(SymbolTable.IdToString(name));
        }

        bool IAttributesCollection.Remove(SymbolId name) {
            return ((IDictionary<object, object>)this).Remove(SymbolTable.IdToString(name));
        }

        ICollection<object> IAttributesCollection.Keys {
            get { return keys(); }
        }

        int IAttributesCollection.Count {
            get {
                return __len__();
            }
        }

        bool IAttributesCollection.TryGetValue(SymbolId name, out object value) {
            if (GetType() != typeof(PythonDictionary) &&
                DictionaryOps.TryGetValueVirtual(DefaultContext.Default, this, SymbolTable.IdToString(name), ref DefaultGetItem, out value)) {
                return true;
            }

            // call Dict.TryGetValue to get the real value.
            return _storage.TryGetValue(name, out value);
        }

        object IAttributesCollection.this[SymbolId name] {
            get {
                return this[SymbolTable.IdToString(name)];
            }
            set {
                if (GetType() == typeof(PythonDictionary)) {
                    // no need to call virtual version
                    _storage.Add(name, value);
                } else {
                    this[SymbolTable.IdToString(name)] = value;
                }
            }
        }

        IDictionary<SymbolId, object> IAttributesCollection.SymbolAttributes {
            get {
                Dictionary<SymbolId, object> d = new Dictionary<SymbolId, object>();
                foreach (KeyValuePair<object, object> name in _storage.GetItems()) {
                    string stringKey = name.Key as string;
                    if (stringKey == null) continue;
                    d.Add(SymbolTable.StringToId(stringKey), name.Value);
                }
                return d;
            }
        }

        void IAttributesCollection.AddObjectKey(object name, object value) { this[name] =  value; }
        bool IAttributesCollection.TryGetObjectValue(object name, out object value) { return ((IDictionary<object, object>)this).TryGetValue(name, out value); }
        bool IAttributesCollection.RemoveObjectKey(object name) { return ((IDictionary<object, object>)this).Remove(name); }
        bool IAttributesCollection.ContainsObjectKey(object name) { return __contains__(name); }
        IDictionary<object, object> IAttributesCollection.AsObjectKeyedDictionary() { return this; }

        #endregion

        #endregion       
    }

#if !SILVERLIGHT // environment variables not available
    internal class EnvironmentDictionaryStorage : CommonDictionaryStorage {
        public EnvironmentDictionaryStorage() {
            foreach(DictionaryEntry de in Environment.GetEnvironmentVariables()) {
                Add(de.Key, de.Value);
            }
        }

        public override void Add(object key, object value) {
            base.Add(key, value);

            string s1 = key as string;
            string s2 = value as string;
            if (s1 != null && s2 != null) {
                Environment.SetEnvironmentVariable(s1, s2);
            }
        }

        public override bool Remove(object key) {
            bool res = base.Remove(key);

            string s = key as string;
            if (s != null) {
                Environment.SetEnvironmentVariable(s, string.Empty);
            }

            return res;
        }       
    }
#endif
   
    /// <summary>
    /// Note: 
    ///   IEnumerator innerEnum = Dictionary&lt;K,V&gt;.KeysCollections.GetEnumerator();
    ///   innerEnum.MoveNext() will throw InvalidOperation even if the values get changed,
    ///   which is supported in python
    /// </summary>
    [PythonSystemType("dictionary-keyiterator")]
    public sealed class DictionaryKeyEnumerator : IEnumerator, IEnumerator<object> {
        private readonly int _size;
        DictionaryStorage _dict;
        private readonly object[] _keys;
        private int _pos;

        internal DictionaryKeyEnumerator(DictionaryStorage dict) {
            _dict = dict;
            _size = dict.Count;
            _keys = new object[_size];
            int i = 0;
            foreach (KeyValuePair<object, object> kvp in dict.GetItems()) {
                _keys[i++] = kvp.Key;
            }
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
                return _keys[_pos];
            }
        }

        public void Dispose() {
        }

        public object __iter__() {
            return this;
        }

        public int __len__() {
            return _size - _pos - 1;
        }
    }

    /// <summary>
    /// Note: 
    ///   IEnumerator innerEnum = Dictionary&lt;K,V&gt;.KeysCollections.GetEnumerator();
    ///   innerEnum.MoveNext() will throw InvalidOperation even if the values get changed,
    ///   which is supported in python
    /// </summary>
    [PythonSystemType("dictionary-valueiterator")]
    public sealed class DictionaryValueEnumerator : IEnumerator, IEnumerator<object> {
        private readonly int _size;
        DictionaryStorage _dict;
        private readonly object[] _values;
        private int _pos;

        internal DictionaryValueEnumerator(DictionaryStorage dict) {
            _dict = dict;
            _size = dict.Count;
            _values = new object[_size];
            int i = 0;
            foreach (KeyValuePair<object, object> kvp in dict.GetItems()) {
                _values[i++] = kvp.Value;
            }
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
                return _values[_pos];
            }
        }

        public void Dispose() {
        }

        public object __iter__() {
            return this;
        }

        public int __len__() {
            return _size - _pos - 1;
        }
    }

    /// <summary>
    /// Note: 
    ///   IEnumerator innerEnum = Dictionary&lt;K,V&gt;.KeysCollections.GetEnumerator();
    ///   innerEnum.MoveNext() will throw InvalidOperation even if the values get changed,
    ///   which is supported in python
    /// </summary>
    [PythonSystemType("dictionary-itemiterator")]
    public sealed class DictionaryItemEnumerator : IEnumerator, IEnumerator<object> {
        private readonly int _size;
        DictionaryStorage _dict;
        private readonly object[] _keys;
        private readonly object[] _values;
        private int _pos;

        internal DictionaryItemEnumerator(DictionaryStorage dict) {
            _dict = dict;
            _size = dict.Count;
            _keys = new object[_size];
            _values = new object[_size];
            int i = 0;
            foreach (KeyValuePair<object, object> kvp in dict.GetItems()) {
                _keys[i] = kvp.Key;
                _values[i++] = kvp.Value;
            }
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
                return PythonOps.MakeTuple(_keys[_pos], _values[_pos]);
            }
        }

        public void Dispose() {
        }

        public object __iter__() {
            return this;
        }

        public int __len__() {
            return _size - _pos - 1;
        }
    }

}
