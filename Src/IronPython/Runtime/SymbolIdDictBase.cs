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

    [PythonType(typeof(Dict))]
    public abstract class SymbolIdDictBase : IMapping, ICloneable, IRichEquality, IRichComparable, ICodeFormattable {
        private static object DefaultGetItem;
        #region Abstract Members
        abstract internal IDictionary<object, object> AsObjectKeyedDictionary();
        [PythonName("clear")] // The CustomAttribute needs to be copied down to the override
        public abstract void Clear();
        [PythonName("__iter__")] // The CustomAttribute needs to be copied down to the override
        public abstract System.Collections.IEnumerator GetEnumerator();
        #endregion

        #region IMapping Members

        [PythonName("get")]
        public object GetValue(object key) {
            Debug.Assert(!(key is SymbolId));
            return DictOps.GetIndex(AsObjectKeyedDictionary(), key);
        }

        [PythonName("get")]
        public object GetValue(object key, object defaultValue) {
            Debug.Assert(!(key is SymbolId));
            return DictOps.GetIndex(AsObjectKeyedDictionary(), key, defaultValue);
        }

        bool IMapping.TryGetValue(object key, out object value) {
            if (DictOps.TryGetValueVirtual(this, key, ref DefaultGetItem, out value)) {
                return true;
            }

            return AsObjectKeyedDictionary().TryGetValue(key, out value);
        }

        object IMapping.this[object key] {
            get { return AsObjectKeyedDictionary()[key]; }
            set { AsObjectKeyedDictionary()[key] = value; }
        }

        [PythonName("__delitem__")]
        public void DeleteItem(object key) {
            DictOps.DelIndex(AsObjectKeyedDictionary(), key);
        }

        #endregion

        #region Dict Members

        [PythonName("has_key")]
        public object has_key(object key) {
            return DictOps.HasKey(AsObjectKeyedDictionary(), key);
        }

        [PythonName("pop")]
        public object pop(object key) {
            return DictOps.Pop(AsObjectKeyedDictionary(), key);
        }

        [PythonName("pop")]
        public object pop(object key, object defaultValue) {
            return DictOps.Pop(AsObjectKeyedDictionary(), key, defaultValue);
        }

        [PythonName("setdefault")]
        public object setdefault(object key) {
            return DictOps.SetDefault(AsObjectKeyedDictionary(), key);
        }

        [PythonName("setdefault")]
        public object setdefault(object key, object defaultValue) {
            return DictOps.SetDefault(AsObjectKeyedDictionary(), key, defaultValue);
        }

        [PythonName("keys")]
        public List keys() {
            return DictOps.Keys(AsObjectKeyedDictionary());
        }

        [PythonName("values")]
        public List values() {
            return DictOps.Values(AsObjectKeyedDictionary());
        }

        [PythonName("items")]
        public List items() {
            return DictOps.Items(AsObjectKeyedDictionary());
        }

        [PythonName("iteritems")]
        public IEnumerator iteritems() {
            return DictOps.IterItems(AsObjectKeyedDictionary());
        }
        [PythonName("iterkeys")]
        public IEnumerator iterkeys() {
            return DictOps.IterKeys(AsObjectKeyedDictionary());
        }
        [PythonName("itervalues")]
        public IEnumerator itervalues() {
            return DictOps.IterValues(AsObjectKeyedDictionary());
        }

        [PythonName("__str__")]
        public override string ToString() {
            return DictOps.ToString(AsObjectKeyedDictionary());
        }

        [PythonName("update")]
        public void update(object b) {
            DictOps.Update(AsObjectKeyedDictionary(), b);
        }

        [PythonName("popitem")]
        public Tuple popitem() {
            return DictOps.PopItem(AsObjectKeyedDictionary());
        }

        [PythonClassMethod("fromkeys")]
        public static object fromkeys(DynamicType cls, object seq) {
            return Dict.FromKeys(cls, seq, null);
        }

        [PythonClassMethod("fromkeys")]
        public static object fromkeys(DynamicType cls, object seq, object value) {
            return Dict.FromKeys(cls, seq, value);
        }

        #endregion

        #region ICollection Members

        public void CopyTo(Array array, int index) {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        public bool IsSynchronized {
            get { return false; }
        }

        public object SyncRoot {
            get { return null; }
        }

        #endregion

        #region IPythonContainer Members

        [PythonName("__len__")]
        public int GetLength() {
            return DictOps.Length(AsObjectKeyedDictionary());
        }

        [PythonName("__contains__")]
        public bool ContainsValue(object value) {
            return DictOps.Contains(AsObjectKeyedDictionary(), value);
        }

        #endregion

        #region IRichEquality Members

        [PythonName("__hash__")]
        public object RichGetHashCode() {
            throw Ops.TypeErrorForUnhashableType("dict");
        }

        [PythonName("__eq__")]
        public virtual object RichEquals(object other) {
            IAttributesDictionary oth = other as IAttributesDictionary;
            IAttributesDictionary ths = this as IAttributesDictionary;
            if (oth == null) return Ops.FALSE;

            if (oth.Count != ths.Count) return Ops.FALSE;

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

        #region ICloneable Members

        [PythonName("copy")]
        public object Clone() {
            return new Dict(this);
        }

        #endregion

        #region IRichComparable Members

        [PythonName("__cmp__")]
        public object CompareTo(object obj) {
            IDictionary<object, object> other = obj as IDictionary<object, object>;
            // CompareTo is allowed to throw (string, int, etc... all do it if they don't get a matching type)
            if (other == null) return Ops.NotImplemented;

            return DictOps.CompareTo(AsObjectKeyedDictionary(), other);
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

        #region ICodeFormattable Members

        public string ToCodeString() {
            return ToString();
        }

        #endregion
    }
}