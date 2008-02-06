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
using System.Text;
using SpecialNameAttribute = System.Runtime.CompilerServices.SpecialNameAttribute;

using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime.Types;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Math;

namespace IronPython.Runtime {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix"), PythonType("tuple")]
    public class PythonTuple : ISequence, ICollection, IEnumerable, IEnumerable<object>, IValueEquality, IList<object>, ICodeFormattable, IParameterSequence {
        internal static PythonTuple EMPTY = new PythonTuple();

        #region Python Constructors

        // Tuples are immutable so their initialization happens in __new__
        // They also explicitly implement __new__ so they can perform the
        // appropriate caching.  

        [StaticExtensionMethod("__new__")]
        public static PythonTuple PythonNew(CodeContext context, PythonType cls) {
            if (cls == TypeCache.PythonTuple) {
                return EMPTY;
            } else {
                PythonTuple tupObj = cls.CreateInstance(context) as PythonTuple;
                if (tupObj == null) throw PythonOps.TypeError("{0} is not a subclass of tuple", cls);
                return tupObj;
            }
        }

        [StaticExtensionMethod("__new__")]
        public static PythonTuple PythonNew(CodeContext context, PythonType cls, object sequence) {
            if (sequence == null) throw PythonOps.TypeError("iteration over a non-sequence");

            if (cls == TypeCache.PythonTuple) {
                if (sequence.GetType() == typeof(PythonTuple)) return (PythonTuple)sequence;
                return new PythonTuple(MakeItems(sequence));
            } else {
                PythonTuple tupObj = cls.CreateInstance(context, sequence) as PythonTuple;
                if (tupObj == null) throw PythonOps.TypeError("{0} is not a subclass of tuple", cls);
                return tupObj;
            }
        }
        #endregion

        public static PythonTuple Make(object o) {
            if (o is PythonTuple) return (PythonTuple)o;
            return new PythonTuple(MakeItems(o));
        }

        public static PythonTuple MakeTuple(params object[] items) {
            if (items.Length == 0) return EMPTY;
            return new PythonTuple(items);
        }

        // TODO: Make internal
        public static PythonTuple MakeExpandableTuple(params object[] items) {
            if (items.Length == 0) return EMPTY;
            return new PythonTuple(true, items);
        }

        private static object[] MakeItems(object o) {
            object[] arr;
            if (o is PythonTuple) {
                return ((PythonTuple)o)._data;
            } else if (o is string) {
                string s = (string)o;
                object[] res = new object[s.Length];
                for (int i = 0; i < res.Length; i++) {
                    res[i] = RuntimeHelpers.CharToString(s[i]);
                }
                return res;
            } else if (o is List) {
                return ((List)o).GetObjectArray();
            } else if ((arr = o as object[])!=null) {
                object []res = new object[arr.Length];
                Array.Copy(arr, res, arr.Length);
                return res;
            } else {
                PerfTrack.NoteEvent(PerfTrack.Categories.OverAllocate, "TupleOA: " + DynamicHelpers.GetPythonType(o).Name);

                List<object> l = new List<object>();
                IEnumerator i = PythonOps.GetEnumerator(o);
                while (i.MoveNext()) {
                    l.Add(i.Current);
                }

                return l.ToArray();
            }
        }

        internal readonly object[] _data;
        private readonly bool expandable;

        public PythonTuple(object o) {
            this._data = MakeItems(o);
        }

        protected PythonTuple(object[] items) {
            this._data = items;
        }

        public PythonTuple() {
            this._data = ArrayUtils.EmptyObjects;
        }

        internal PythonTuple(bool expandable, object[] items) {
            this.expandable = expandable;
            this._data = items;
        }

        internal PythonTuple(IParameterSequence other, object o) {
            this._data = other.Expand(o);
        }

        bool IParameterSequence.IsExpandable {
            get {
                return expandable;
            }
        }

        /// <summary>
        /// Return a copy of this tuple's data array.
        /// </summary>
        internal object[] ToArray() {
            object[] copy = new object[_data.Length];
            Array.Copy(_data, copy, _data.Length);
            return copy;
        }

        #region ISequence Members
        [PythonName("__len__")]
        public virtual int GetLength() {
            return _data.Length;
        }

        public bool ContainsValueWrapper(object item) {
            return ContainsValue(item);
        }

        public virtual bool ContainsValue(object value) {
            return ArrayOps.Contains(_data, _data.Length, value);
        }

        public virtual object this[int index] {
            get {
                return _data[PythonOps.FixIndex(index, _data.Length)];
            }
        }
        
        public virtual object this[object index] {
            get {
                return this[Converter.ConvertToIndex(index)];
            }
        }

        public virtual object this[BigInteger index] {
            get {
                return this[index.ToInt32()];
            }
        }

        [PythonName("__getslice__")]
        public virtual object GetSlice(int start, int stop) {
            Slice.FixSliceArguments(_data.Length, ref start, ref stop);

            return MakeTuple(ArrayOps.GetSlice(_data, start, stop));
        }

        public virtual object this[Slice slice] {
            get {
                return MakeTuple(ArrayOps.GetSlice(_data, slice));
            }
        }

        #endregion

        #region binary operators
        public static PythonTuple operator +(PythonTuple x, PythonTuple y) {
            return MakeTuple(ArrayOps.Add(x._data, x._data.Length, y._data, y._data.Length));
        }

        private static PythonTuple MultiplyWorker(PythonTuple self, int count) {
            if (count <= 0) return EMPTY;
            if (count == 1) return self;
            return MakeTuple(ArrayOps.Multiply(self._data, self._data.Length, count));
        }

        public static PythonTuple operator *(PythonTuple x, int n) {
            return MultiplyWorker(x, n);
        }

        public static PythonTuple operator *(int n, PythonTuple x) {
            return MultiplyWorker(x, n);
        }

        public static object operator *(PythonTuple self, object count) {
            return PythonOps.MultiplySequence<PythonTuple>(MultiplyWorker, self, count, true);
        }

        public static object operator *(object count, PythonTuple self) {
            return PythonOps.MultiplySequence<PythonTuple>(MultiplyWorker, self, count, false);
        }

        #endregion

        #region ICollection Members

        public bool IsSynchronized {
            get { return false; }
        }

        public int Count {
            get { return GetLength(); }
        }

        public void CopyTo(Array array, int index) {
            Array.Copy(_data, 0, array, index, _data.Length);
        }

        public object SyncRoot {
            get {
                return this;
            }
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// public class to get optimized
        /// </summary>
        public class TupleEnumerator : IEnumerator, IEnumerator<object> {
            int curIndex;
            PythonTuple tuple;

            public TupleEnumerator(PythonTuple t) {
                tuple = t;
                curIndex = -1;
            }

            #region IEnumerator Members

            public object Current {
                get { 
                    // access _data directly because this is what CPython does:
                    // class T(tuple):
                    //     def __getitem__(self): return None
                    // 
                    // for x in T((1,2)): print x
                    // prints 1 and 2
                    return tuple._data[curIndex]; 
                }
            }

            public bool MoveNext() {
                if ((curIndex + 1) >= tuple.Count) {
                    return false;
                }
                curIndex++;
                return true;
            }

            public void Reset() {
                curIndex = -1;
            }

            #endregion

            #region IDisposable Members

            public void Dispose() {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing) {

            }

            #endregion
        }

        public IEnumerator GetEnumerator() {
            return new TupleEnumerator(this);
        }

        #endregion

        public override string ToString() {
            StringBuilder buf = new StringBuilder();
            buf.Append("(");
            for (int i = 0; i < _data.Length; i++) {
                if (i > 0) buf.Append(", ");
                buf.Append(PythonOps.StringRepr(_data[i]));
            }
            if (_data.Length == 1) buf.Append(",");
            buf.Append(")");
            return buf.ToString();
        }

        object[] IParameterSequence.Expand(object value) {
            object[] args;
            int length = _data.Length;
            if (value == null)
                args = new object[length];
            else
                args = new object[length + 1];

            for (int i = 0; i < length; i++) {
                args[i] = _data[i];
            }

            if (value != null) {
                args[length] = value;
            }

            return args;
        }

        [PythonName("__getnewargs__")]
        public object GetNewArgs() {
            // Call "new Tuple()" to force result to be a Tuple (otherwise, it could possibly be a Tuple subclass)
            return PythonTuple.MakeTuple(new PythonTuple(this));
        }

        #region IEnumerable<object> Members

        IEnumerator<object> IEnumerable<object>.GetEnumerator() {
            return new TupleEnumerator(this);
        }

        #endregion

        #region IList<object> Members

        int IList<object>.IndexOf(object item) {
            for (int i = 0; i < Count; i++) {
                if (PythonOps.EqualRetBool(this[i], item)) return i;
            }
            return -1;
        }

        void IList<object>.Insert(int index, object item) {
            throw new InvalidOperationException("Tuple is readonly");
        }

        void IList<object>.RemoveAt(int index) {
            throw new InvalidOperationException("Tuple is readonly");
        }

        object IList<object>.this[int index] {
            get {
                return this[index];
            }
            set {
                throw new InvalidOperationException("Tuple is readonly");
            }
        }

        #endregion

        #region ICollection<object> Members

        void ICollection<object>.Add(object item) {
            throw new InvalidOperationException("Tuple is readonly");
        }

        void ICollection<object>.Clear() {
            throw new InvalidOperationException("Tuple is readonly");
        }

        [PythonName("__contains__")]
        public virtual bool Contains(object item) {
            return this.ContainsValue(item);
        }

        void ICollection<object>.CopyTo(object[] array, int arrayIndex) {
            for (int i = 0; i < Count; i++) {
                array[arrayIndex + i] = this[i];
            }
        }

        int ICollection<object>.Count {
            get { return GetLength(); }
        }

        bool ICollection<object>.IsReadOnly {
            get { return true; }
        }

        bool ICollection<object>.Remove(object item) {
            throw new InvalidOperationException("Tuple is readonly");
        }

        #endregion

        #region Rich Comparison Members

        private int CompareTo(PythonTuple other) {
            return PythonOps.CompareArrays(_data, _data.Length, other._data, other._data.Length);
        }

        [return: MaybeNotImplemented]
        public static object operator >(PythonTuple self, object other) {
            PythonTuple t = other as PythonTuple;
            if (t == null) return PythonOps.NotImplemented;

            return RuntimeHelpers.BooleanToObject(self.CompareTo(t) > 0);
        }

        [return: MaybeNotImplemented]
        public static object operator <(PythonTuple self, object other) {
            PythonTuple t = other as PythonTuple;
            if (t == null) return PythonOps.NotImplemented;

            return RuntimeHelpers.BooleanToObject(self.CompareTo(t) < 0);
        }

        [return: MaybeNotImplemented]
        public static object operator >=(PythonTuple self, object other) {
            PythonTuple t = other as PythonTuple;
            if (t == null) return PythonOps.NotImplemented;

            return RuntimeHelpers.BooleanToObject(self.CompareTo(t) >= 0);
        }

        [return: MaybeNotImplemented]
        public static object operator <=(PythonTuple self, object other) {
            PythonTuple t = other as PythonTuple;
            if (t == null) return PythonOps.NotImplemented;

            return RuntimeHelpers.BooleanToObject(self.CompareTo(t) <= 0);
        }

        #endregion

        public override bool Equals(object obj) {
            PythonTuple other = obj as PythonTuple;
            if (other == null) return false;
            if (other.Count != Count) return false;

            for (int i = 0; i < Count; i++) {
                if (this[i] != null) {
                    if (!this[i].Equals(other[i])) {
                        return false;
                    }
                } else if (other[i] != null) {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode() {
            int ret = 6551;
            foreach (object o in _data) {
                ret = (ret << 5) ^ (ret >> 26) ^ (o == null ? 0 : o.GetHashCode());
            }
            return ret;
        }

        #region IValueEquality Members

        int IValueEquality.GetValueHashCode() {
            int ret = 6551;
            foreach (object o in _data) {
                ret = (ret << 5) ^ (ret >> 26) ^ PythonOps.Hash(o);
            }
            return ret;
        }

        bool IValueEquality.ValueEquals(object other) {
            if (Object.ReferenceEquals(other, this)) return true;

            PythonTuple l = other as PythonTuple;
            if (l == null) return false;

            if (_data.Length != l._data.Length) return false;
            for (int i = 0; i < _data.Length; i++) {
                if (!PythonOps.EqualRetBool(_data[i], l._data[i])) return false;
            }
            return true;
        }

        bool IValueEquality.ValueNotEquals(object other) {
            return !((IValueEquality)this).ValueEquals(other);
        }

        #endregion

        #region ICodeFormattable Members

        [SpecialName, PythonName("__repr__")]
        public string ToCodeString(CodeContext context) {
            return ToString();
        }

        #endregion
    }
}
