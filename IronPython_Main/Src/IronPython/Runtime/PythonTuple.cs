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
using SpecialNameAttribute = System.Runtime.CompilerServices.SpecialNameAttribute;

using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Types;

using IronPython.Runtime.Types;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix"), PythonType("tuple")]
    public class PythonTuple : ISequence, ICollection, IEnumerable, IEnumerable<object>, IValueEquality, IList<object>, ICodeFormattable, IParameterSequence {
        private static PythonTuple EMPTY = new PythonTuple();

        #region Python Constructors

        // Tuples are immutable so their initialization happens in __new__
        // They also explicitly implement __new__ so they can perform the
        // appropriate caching.  

        [StaticExtensionMethod("__new__")]
        public static PythonTuple PythonNew(CodeContext context, DynamicType cls) {
            if (cls == TypeCache.PythonTuple) {
                return EMPTY;
            } else {
                PythonTuple tupObj = cls.CreateInstance(context) as PythonTuple;
                if (tupObj == null) throw PythonOps.TypeError("{0} is not a subclass of tuple", cls);
                return tupObj;
            }
        }

        [StaticExtensionMethod("__new__")]
        public static PythonTuple PythonNew(CodeContext context, DynamicType cls, object sequence) {
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
                return ((PythonTuple)o).data;
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
                PerfTrack.NoteEvent(PerfTrack.Categories.OverAllocate, "TupleOA: " + DynamicHelpers.GetDynamicType(o).Name);

                List<object> l = new List<object>();
                IEnumerator i = PythonOps.GetEnumerator(o);
                while (i.MoveNext()) {
                    l.Add(i.Current);
                }

                return l.ToArray();
            }
        }

        internal readonly object[] data;
        private readonly bool expandable;

        public PythonTuple(object o) {
            this.data = MakeItems(o);
        }

        protected PythonTuple(object[] items) {
            this.data = items;
        }

        public PythonTuple() {
            this.data = ArrayUtils.EmptyObjects;
        }

        internal PythonTuple(bool expandable, object[] items) {
            this.expandable = expandable;
            this.data = items;
        }

        internal PythonTuple(IParameterSequence other, object o) {
            this.data = other.Expand(o);
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
            object[] copy = new object[data.Length];
            Array.Copy(data, copy, data.Length);
            return copy;
        }

        #region ISequence Members
        [SpecialName, PythonName("__len__")]
        public int GetLength() {
            return data.Length;
        }

        [SpecialName, PythonName("__contains__")]
        public bool ContainsValueWrapper(object item) {
            return ContainsValue(item);
        }

        public virtual bool ContainsValue(object value) {
            return ArrayOps.Contains(data, data.Length, value);
        }

        public virtual object this[int index] {
            get {
                return data[PythonOps.FixIndex(index, data.Length)];
            }
        }

        [PythonName("__getslice__")]
        public virtual object GetSlice(int start, int stop) {
            if (start < 0) start = 0;
            if (stop > GetLength()) stop = GetLength();

            return MakeTuple(ArrayOps.GetSlice(data, start, stop));
        }

        public object this[Slice slice] {
            get {
                return MakeTuple(ArrayOps.GetSlice(data, slice));
            }
        }

        #endregion

        #region binary operators
        public static PythonTuple operator +(PythonTuple x, PythonTuple y) {
            return MakeTuple(ArrayOps.Add(x.data, x.data.Length, y.data, y.data.Length));
        }

        private static PythonTuple MultiplyWorker(PythonTuple self, int count) {
            if (count <= 0) return EMPTY;
            if (count == 1) return self;
            return MakeTuple(ArrayOps.Multiply(self.data, self.data.Length, count));
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
            get { return data.Length; }
        }

        public void CopyTo(Array array, int index) {
            Array.Copy(data, 0, array, index, data.Length);
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
                get { return tuple[curIndex]; }
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
            for (int i = 0; i < data.Length; i++) {
                if (i > 0) buf.Append(", ");
                buf.Append(PythonOps.StringRepr(data[i]));
            }
            if (data.Length == 1) buf.Append(",");
            buf.Append(")");
            return buf.ToString();
        }

        object[] IParameterSequence.Expand(object value) {
            object[] args;
            int length = data.Length;
            if (value == null)
                args = new object[length];
            else
                args = new object[length + 1];

            for (int i = 0; i < length; i++) {
                args[i] = data[i];
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

        #region IDynamicObject Members

        public virtual DynamicType DynamicType {
            get {
                return TypeCache.PythonTuple;
            }
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

        bool ICollection<object>.Contains(object item) {
            return this.ContainsValue(item);
        }

        void ICollection<object>.CopyTo(object[] array, int arrayIndex) {
            for (int i = 0; i < Count; i++) {
                array[arrayIndex + i] = this[i];
            }
        }

        int ICollection<object>.Count {
            get { return this.Count; }
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
            return PythonOps.CompareArrays(data, data.Length, other.data, other.data.Length);
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
            foreach (object o in data) {
                ret = (ret << 5) ^ (ret >> 26) ^ (o == null ? 0 : o.GetHashCode());
            }
            return ret;
        }

        #region IValueEquality Members

        int IValueEquality.GetValueHashCode() {
            int ret = 6551;
            foreach (object o in data) {
                ret = (ret << 5) ^ (ret >> 26) ^ PythonOps.Hash(o);
            }
            return ret;
        }

        bool IValueEquality.ValueEquals(object other) {
            if (Object.ReferenceEquals(other, this)) return true;

            PythonTuple l = other as PythonTuple;
            if (l == null) return false;

            if (data.Length != l.data.Length) return false;
            for (int i = 0; i < data.Length; i++) {
                if (!PythonOps.EqualRetBool(data[i], l.data[i])) return false;
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
