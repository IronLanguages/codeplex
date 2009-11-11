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
using System.Runtime.InteropServices;
using System.Text;

using Microsoft.Scripting;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

namespace IronPython.Runtime {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    [PythonType("tuple"), Serializable, DebuggerTypeProxy(typeof(CollectionDebugProxy)), DebuggerDisplay("tuple, {Count} items")]
    public class PythonTuple : ICollection, IEnumerable, IEnumerable<object>, IValueEquality, IList<object>, ICodeFormattable, IList {
        internal readonly object[] _data;
        
        internal static readonly PythonTuple EMPTY = new PythonTuple();

        public PythonTuple(object o) {
            this._data = MakeItems(o);
        }

        protected PythonTuple(object[] items) {
            this._data = items;
        }

        public PythonTuple() {
            this._data = ArrayUtils.EmptyObjects;
        }

        internal PythonTuple(PythonTuple other, object o) {
            this._data = other.Expand(o);
        }

        #region Python Constructors

        // Tuples are immutable so their initialization happens in __new__
        // They also explicitly implement __new__ so they can perform the
        // appropriate caching.  

        public static PythonTuple __new__(CodeContext context, PythonType cls) {
            if (cls == TypeCache.PythonTuple) {
                return EMPTY;
            } else {
                PythonTuple tupObj = cls.CreateInstance(context) as PythonTuple;
                if (tupObj == null) throw PythonOps.TypeError("{0} is not a subclass of tuple", cls);
                return tupObj;
            }
        }

        public static PythonTuple __new__(CodeContext context, PythonType cls, object sequence) {
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

        #region Python 2.6 Methods

        public int index(object obj, object start) {
            return index(obj, Converter.ConvertToIndex(start), _data.Length);
        }

        public int index(object obj, [DefaultParameterValue(0)]int start) {
            return index(obj, start, _data.Length);
        }

        public int index(object obj, object start, object end) {
            return index(obj, Converter.ConvertToIndex(start), Converter.ConvertToIndex(end));
        }

        public int index(object obj, int start, int end) {
            start = PythonOps.FixSliceIndex(start, _data.Length);
            end = PythonOps.FixSliceIndex(end, _data.Length);

            for (int i = start; i < end; i++) {
                if (PythonOps.EqualRetBool(obj, _data[i])) {
                    return i;
                }
            }

            throw PythonOps.ValueError("tuple.index(x): x not in list");
        }

        public int count(object obj) {
            int cnt = 0;
            foreach (object elem in _data) {
                if (PythonOps.EqualRetBool(obj, elem)) {
                    cnt++;
                }
            }
            return cnt;
        }

        #endregion

        internal static PythonTuple Make(object o) {
            if (o is PythonTuple) return (PythonTuple)o;
            return new PythonTuple(MakeItems(o));
        }

        internal static PythonTuple MakeTuple(params object[] items) {
            if (items.Length == 0) return EMPTY;
            return new PythonTuple(items);
        }

        private static object[] MakeItems(object o) {
            object[] arr;
            if (o is PythonTuple) {
                return ((PythonTuple)o)._data;
            } else if (o is string) {
                string s = (string)o;
                object[] res = new object[s.Length];
                for (int i = 0; i < res.Length; i++) {
                    res[i] = ScriptingRuntimeHelpers.CharToString(s[i]);
                }
                return res;
            } else if (o is List) {
                return ((List)o).GetObjectArray();
            } else if ((arr = o as object[])!=null) {
                return ArrayOps.CopyArray(arr, arr.Length);
            } else {
                PerfTrack.NoteEvent(PerfTrack.Categories.OverAllocate, "TupleOA: " + PythonTypeOps.GetName(o));

                List<object> l = new List<object>();
                IEnumerator i = PythonOps.GetEnumerator(o);
                while (i.MoveNext()) {
                    l.Add(i.Current);
                }

                return l.ToArray();
            }
        }
        
        /// <summary>
        /// Return a copy of this tuple's data array.
        /// </summary>
        internal object[] ToArray() {
            return ArrayOps.CopyArray(_data, _data.Length);
        }

        #region ISequence Members

        public virtual int __len__() {
            return _data.Length;
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

        public virtual object __getslice__(int start, int stop) {
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
        public static PythonTuple operator +([NotNull]PythonTuple x, [NotNull]PythonTuple y) {
            return MakeTuple(ArrayOps.Add(x._data, x._data.Length, y._data, y._data.Length));
        }

        private static PythonTuple MultiplyWorker(PythonTuple self, int count) {
            if (count <= 0) {
                return EMPTY;
            } else if (count == 1 && self.GetType() == typeof(PythonTuple)) {
                return self;
            }

            return MakeTuple(ArrayOps.Multiply(self._data, self._data.Length, count));
        }

        public static PythonTuple operator *(PythonTuple x, int n) {
            return MultiplyWorker(x, n);
        }

        public static PythonTuple operator *(int n, PythonTuple x) {
            return MultiplyWorker(x, n);
        }

        public static object operator *([NotNull]PythonTuple self, [NotNull]Index count) {
            return PythonOps.MultiplySequence<PythonTuple>(MultiplyWorker, self, count, true);
        }

        public static object operator *([NotNull]Index count, [NotNull]PythonTuple self) {
            return PythonOps.MultiplySequence<PythonTuple>(MultiplyWorker, self, count, false);
        }

        public static object operator *([NotNull]PythonTuple self, object count) {
            int index;
            if (Converter.TryConvertToIndex(count, out index)) {
                return self * index;
            }
            throw PythonOps.TypeErrorForUnIndexableObject(count);
        }

        public static object operator *(object count, [NotNull]PythonTuple self) {
            int index;
            if (Converter.TryConvertToIndex(count, out index)) {
                return index * self;
            }

            throw PythonOps.TypeErrorForUnIndexableObject(count);
        }

        #endregion

        #region ICollection Members

        bool ICollection.IsSynchronized {
            get { return false; }
        }

        public int Count {
            [PythonHidden]
            get { return __len__(); }
        }

        [PythonHidden]
        public void CopyTo(Array array, int index) {
            Array.Copy(_data, 0, array, index, _data.Length);
        }

        object ICollection.SyncRoot {
            get {
                return this;
            }
        }

        #endregion

        public virtual IEnumerator __iter__() {
            return new TupleEnumerator(this);
        }

        #region IEnumerable Members

        [PythonHidden]
        public IEnumerator GetEnumerator() {
            return __iter__();
        }

        #endregion

        private object[] Expand(object value) {
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

        public object __getnewargs__() {
            // Call "new Tuple()" to force result to be a Tuple (otherwise, it could possibly be a Tuple subclass)
            return PythonTuple.MakeTuple(new PythonTuple(this));
        }

        #region IEnumerable<object> Members

        IEnumerator<object> IEnumerable<object>.GetEnumerator() {
            return new TupleEnumerator(this);
        }

        #endregion

        #region IList<object> Members

        [PythonHidden]
        public int IndexOf(object item) {
            for (int i = 0; i < __len__(); i++) {
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

        [PythonHidden]
        public bool Contains(object item) {
            for (int i = 0; i < _data.Length; i++) {
                if (PythonOps.EqualRetBool(_data[i], item)) {
                    return true;
                }
            }

            return false;
        }

        [PythonHidden]
        public void CopyTo(object[] array, int arrayIndex) {
            for (int i = 0; i < __len__(); i++) {
                array[arrayIndex + i] = this[i];
            }
        }

        bool ICollection<object>.IsReadOnly {
            get { return true; }
        }

        bool ICollection<object>.Remove(object item) {
            throw new InvalidOperationException("Tuple is readonly");
        }

        #endregion

        #region Rich Comparison Members

        internal int CompareTo(PythonTuple other) {
            return PythonOps.CompareArrays(_data, _data.Length, other._data, other._data.Length);
        }

        public static bool operator >([NotNull]PythonTuple self, [NotNull]PythonTuple other) {
            return self.CompareTo(other) > 0;
        }

        public static bool operator <([NotNull]PythonTuple self, [NotNull]PythonTuple other) {
            return self.CompareTo(other) < 0;
        }

        public static bool operator >=([NotNull]PythonTuple self, [NotNull]PythonTuple other) {
            return self.CompareTo(other) >= 0;
        }

        public static bool operator <=([NotNull]PythonTuple self, [NotNull]PythonTuple other) {
            return self.CompareTo(other) <= 0;
        }

        #endregion

        public override bool Equals(object obj) {
            PythonTuple other = obj as PythonTuple;
            if (other == null) return false;
            if (other.__len__() != __len__()) return false;

            for (int i = 0; i < __len__(); i++) {
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
            int hash1 = 6551;
            int hash2 = hash1;

            for (int i = 0; i < _data.Length; i += 2) {
                hash1 = ((hash1 << 27) + ((hash2 + 1) << 1) + (hash1 >> 5)) ^ ((_data[i] == null) ? 0 : _data[i].GetHashCode());

                if (i == _data.Length - 1) {
                    break;
                }
                hash2 = ((hash2 << 5) + ((hash1 - 1) >> 1) + (hash2 >> 27)) ^ ((_data[i + 1] == null) ? 0 : _data[i + 1].GetHashCode());
            }

            return hash1 + (hash2 * 1566083941);
        }

        public override string ToString() {
            return __repr__(DefaultContext.Default);
        }

        #region IValueEquality Members

        private delegate int HashDelegate(object o, ref HashDelegate dlg);
        private static HashDelegate _strHasher = StringHasher, _intHasher = IntHasher, _initialHasher = InitialHasher, _doubleHasher = DoubleHasher;

        private static int InitialHasher(object o, ref HashDelegate dlg) {
            if (o == null) {
                return NoneTypeOps.NoneHashCode;
            }

            switch (Type.GetTypeCode(o.GetType())) {
                case TypeCode.String:
                    dlg = _strHasher;
                    return o.GetHashCode();
                case TypeCode.Int32:
                    dlg = _intHasher;
                    return (int)o;
                case TypeCode.Double:
                    dlg = _doubleHasher;
                    return DoubleOps.__hash__((double)o);
                default:
                    if (o is IPythonObject) {
                        dlg = new OptimizedUserHasher(((IPythonObject)o).PythonType).Hasher;
                    } else {
                        dlg = new OptimizedBuiltinHasher(o.GetType()).Hasher;
                    }

                    return dlg(o, ref dlg);                    
            }
        }

        class OptimizedUserHasher {
            private readonly PythonType _pt;

            public OptimizedUserHasher(PythonType pt) {
                _pt = pt;
            }

            public int Hasher(object o, ref HashDelegate dlg) {
                IPythonObject ipo = o as IPythonObject;
                if (ipo != null && ipo.PythonType == _pt) {
                    return _pt.Hash(o);
                }

                dlg = GenericHasher;
                return GenericHasher(o, ref dlg);
            }
        }
        
        class OptimizedBuiltinHasher {
            private readonly Type _type;
            private readonly PythonType _pt;
            
            public OptimizedBuiltinHasher(Type type) {
                _type = type;
                _pt = DynamicHelpers.GetPythonTypeFromType(type);
            }

            public int Hasher(object o, ref HashDelegate dlg) {
                if (o != null && o.GetType() == _type) {
                    return _pt.Hash(o);
                }

                dlg = GenericHasher;
                return GenericHasher(o, ref dlg);
            }
        }

        private static int GenericHasher(object o, ref HashDelegate dlg) {
            return PythonOps.Hash(DefaultContext.Default, o);
        }

        private static int IntHasher(object o, ref HashDelegate dlg) {
            if (o != null && o.GetType() == typeof(int)) {
                return o.GetHashCode();
            }

            dlg = GenericHasher;
            return GenericHasher(o, ref dlg);
        }

        private static int DoubleHasher(object o, ref HashDelegate dlg) {
            if (o != null && o.GetType() == typeof(double)) {
                return DoubleOps.__hash__((double)o);
            }

            dlg = GenericHasher;
            return GenericHasher(o, ref dlg);
        }

        private static int StringHasher(object o, ref HashDelegate dlg) {
            if (o != null && o.GetType() == typeof(string)) {
                return o.GetHashCode();
            }

            dlg = GenericHasher;
            return GenericHasher(o, ref dlg);
        }

        int IValueEquality.GetValueHashCode() {
            int hash1 = 6551;
            int hash2 = hash1;

            HashDelegate dlg = _initialHasher;
            for (int i = 0; i < _data.Length; i += 2) {
                hash1 = ((hash1 << 27) + ((hash2 + 1) << 1) + (hash1 >> 5)) ^ dlg(_data[i], ref dlg);

                if (i == _data.Length - 1) {
                    break;
                }
                hash2 = ((hash2 << 5) + ((hash1 - 1) >> 1) + (hash2 >> 27)) ^ dlg(_data[i + 1], ref dlg);
            }
            return hash1 + (hash2 * 1566083941);

        }

        bool IValueEquality.ValueEquals(object other) {
            if (!Object.ReferenceEquals(other, this)) {
                PythonTuple l = other as PythonTuple;
                if (l == null || _data.Length != l._data.Length) {
                    return false;
                }

                for (int i = 0; i < _data.Length; i++) {
                    object obj1 = _data[i], obj2 = l._data[i];

                    if (Object.ReferenceEquals(obj1, obj2)) {
                        continue;
                    } else if (!PythonOps.EqualRetBool(obj1, obj2)) {
                        return false;
                    }
                }
            }
            return true;
        }

        #endregion

        #region ICodeFormattable Members

        public virtual string/*!*/ __repr__(CodeContext/*!*/ context) {
            StringBuilder buf = new StringBuilder();
            buf.Append("(");
            for (int i = 0; i < _data.Length; i++) {
                if (i > 0) buf.Append(", ");
                buf.Append(PythonOps.Repr(context, _data[i]));
            }
            if (_data.Length == 1) buf.Append(",");
            buf.Append(")");
            return buf.ToString();
        }

        #endregion

        #region IList Members

        int IList.Add(object value) {
            throw new InvalidOperationException("Tuple is readonly");
        }

        void IList.Clear() {
            throw new InvalidOperationException("Tuple is readonly");
        }

        void IList.Insert(int index, object value) {
            throw new InvalidOperationException("Tuple is readonly");
        }

        bool IList.IsFixedSize {
            get { return true; }
        }

        bool IList.IsReadOnly {
            get { return true; }
        }

        void IList.Remove(object value) {
            throw new InvalidOperationException("Tuple is readonly");
        }

        void IList.RemoveAt(int index) {
            throw new InvalidOperationException("Tuple is readonly");
        }

        object IList.this[int index] {
            get {
                return this[index];
            }
            set {
                throw new InvalidOperationException("Tuple is readonly");
            }
        }

        #endregion
    }

    /// <summary>
    /// public class to get optimized
    /// </summary>
    [PythonType("tupleiterator")]
    public sealed class TupleEnumerator : IEnumerable, IEnumerator, IEnumerator<object> {
        private int _curIndex;
        private PythonTuple _tuple;

        public TupleEnumerator(PythonTuple t) {
            _tuple = t;
            _curIndex = -1;
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
                return _tuple._data[_curIndex];
            }
        }

        public bool MoveNext() {
            if ((_curIndex + 1) >= _tuple.__len__()) {
                return false;
            }
            _curIndex++;
            return true;
        }

        public void Reset() {
            _curIndex = -1;
        }

        #endregion

        #region IDisposable Members

        public void Dispose() {
            GC.SuppressFinalize(this);
        }

        #endregion

        #region IEnumerable Members

        public IEnumerator GetEnumerator() {
            return this;
        }

        #endregion
    }
}
