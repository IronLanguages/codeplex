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

using IronPython.Runtime.Types;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime {
    [PythonType("tuple")]
    public class Tuple : ISequence, ICollection, IEnumerable, IEnumerable<object>, IComparable, IDynamicObject, IList<object> {
        private static Tuple EMPTY = new Tuple();

        #region Python Constructors

        // Tuples are immutable so their initialization happens in __new__
        // They also explicitly implement __new__ so they can perform the
        // appropriate caching.  

        [PythonName("__new__")]
        public static Tuple PythonNew(DynamicType cls) {
            if (cls == TypeCache.Tuple) {
                return EMPTY;
            } else {
                Tuple tupObj = cls.ctor.Call(cls) as Tuple;
                if (tupObj == null) throw Ops.TypeError("{0} is not a subclass of tuple", cls);
                return tupObj;
            }
        }

        [PythonName("__new__")]
        public static Tuple PythonNew(DynamicType cls, object sequence) {
            if (cls == TypeCache.Tuple) {
                if(sequence.GetType() == typeof(Tuple)) return (Tuple)sequence;
                return new Tuple(MakeItems(sequence));
            } else {
                Tuple tupObj = cls.ctor.Call(cls, sequence) as Tuple;
                if (tupObj == null) throw Ops.TypeError("{0} is not a subclass of tuple", cls);
                return tupObj;
            }
        }
        #endregion

        public static Tuple Make(object o) {
            if (o is Tuple) return (Tuple)o;
            return new Tuple(MakeItems(o));
        }

        public static Tuple MakeTuple(params object[] items) {
            if (items.Length == 0) return EMPTY;
            return new Tuple(items);
        }


        internal static Tuple MakeExpandableTuple(params object[] items) {
            if (items.Length == 0) return EMPTY;
            return new Tuple(true, items);
        }

        private static object[] MakeItems(object o) {
            if (o is Tuple) {
                return ((Tuple)o).data;
            } else if (o is string) {
                string s = (string)o;
                object[] res = new object[s.Length];
                for (int i = 0; i < res.Length; i++) {
                    res[i] = Ops.Char2String(s[i]);
                }
                return res;
            } else if (o is List) {
                return ((List)o).GetObjectArray();
            } else {
                ArrayList l = new ArrayList();
                IEnumerator i = Ops.GetEnumerator(o);
                while (i.MoveNext()) {
                    l.Add(i.Current);
                }

                return l.ToArray();
            }
        }

        private readonly object[] data;
        private readonly bool expandable;

        public Tuple(object o) {
            this.data = MakeItems(o); 
        }

        private Tuple(params object[] items) {
            this.data = items;
        }

        public Tuple() {
            this.data = Ops.EMPTY;
        }

        internal Tuple(bool expandable, object[] items) {
            this.expandable = expandable;
            this.data = items;
        }

        internal Tuple(Tuple other, object o) {
            this.data = other.Expand(o);
        }

        internal bool IsExpandable {
            get {
                return expandable;
            }
        }

        #region ISequence Members
        [PythonName("__len__")]
        public int GetLength() {
            return data.Length;
        }

        [PythonName("__contains__")]
        public object ContainsValueWrapper(object item) {
            return Ops.Bool2Object(ContainsValue(item));
        }

        public virtual bool ContainsValue(object item) {
            return ArrayOps.Contains(data, data.Length, item);
        }
        
        public virtual object this[int index]{
            get {                
                return data[Ops.FixIndex(index, data.Length)];
            }
        }

        [PythonName("__add__")]
        public virtual object AddSequence(object other) {
            Tuple o = other as Tuple;
            if (o == null) throw Ops.TypeErrorForBadInstance("can only concatenate tuple (not \"{0}\") to tuple", other);

            return MakeTuple(ArrayOps.Add(data, data.Length, o.data, o.data.Length));
        }

        [PythonName("__iadd__")]
        public virtual object __iadd__(object other) {
            return AddSequence(other);
        }

        [PythonName("__mul__")]
        public virtual object MultiplySequence(object count) {
            return Ops.MultiplySequence<Tuple>(MultiplySequenceWorker, this, count);
        }

        private static Tuple MultiplySequenceWorker(Tuple self, int count) {
            return MakeTuple(ArrayOps.Multiply(self.data, self.data.Length, count));
        }

        [PythonName("__imul__")]
        public virtual object __imul__(object count) {
            return MultiplySequence(count);
        }

        [PythonName("__getslice__")]
        public virtual object GetSlice(int start, int stop) {
            if (start < 0) start = 0;
            if (stop > GetLength()) stop = GetLength();

            return Make(ArrayOps.GetSlice(data, data.Length, new Slice(start, stop)));
        }

        public object this[Slice slice] {
            get {
                return Make(ArrayOps.GetSlice(data, data.Length, slice));
            }
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
            Tuple tuple;

            public TupleEnumerator(Tuple t) {
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
            }

            #endregion
        }

        public IEnumerator GetEnumerator() {
            return new TupleEnumerator(this);
        }

        #endregion


        [PythonName("__hash__")]
        public override int GetHashCode() {
            int ret = 6551;
            foreach (object o in data) {
                ret = (ret << 5) ^ (ret >> 26) ^ Ops.Hash(o);
            }
            return ret;
        }

        [PythonName("__eq__")]
        public override bool Equals(object obj) {
            Tuple l = obj as Tuple;
            if (l == null) return false;

            if (data.Length != l.data.Length) return false;
            for (int i = 0; i < data.Length; i++) {
                if (!Ops.EqualRetBool(data[i], l.data[i])) return false;
            }
            return true;
        }


        public int CompareTo(object other) {
            Tuple l = other as Tuple;
            if (l == null) throw new ArgumentException("expected list");

            return Ops.CompareArrays(data, data.Length, l.data, l.data.Length);
        }

        [PythonName("__str__")]
        public override string ToString() {
            StringBuilder buf = new StringBuilder();
            buf.Append("(");
            for (int i = 0; i < data.Length; i++) {
                if (i > 0) buf.Append(", ");
                buf.Append(Ops.StringRepr(data[i]));
            }
            if (data.Length == 1) buf.Append(",");
            buf.Append(")");
            return buf.ToString();
        }

        internal object[] Expand(object value) {
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
            return Tuple.MakeTuple(new Tuple(this));
        }

        #region IEnumerable<object> Members

        IEnumerator<object> IEnumerable<object>.GetEnumerator() {
            return new TupleEnumerator(this);
        }

        #endregion

        #region IDynamicObject Members

        public DynamicType GetDynamicType() {
            return TypeCache.Tuple;
        }

        #endregion

        #region IList<object> Members

        int IList<object>.IndexOf(object item) {
            for (int i = 0; i < Count; i++) {
                if (Ops.EqualRetBool(this[i], item)) return i;
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

    }
}
