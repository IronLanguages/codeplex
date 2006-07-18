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
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;
using System.Threading;

using IronPython.Compiler;
using IronPython.Runtime.Types;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime {

    [PythonType("list"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public class List : IMutableSequence, IList, IComparable, ICodeFormattable, IRichEquality, IDynamicObject, IList<object> {
        private int size;
        private object[] data;

        [PythonName("__init__")]
        public void Initialize() {
            data = new object[8];
            size = 0;
        }

        [PythonName("__init__")]
        public void Initialize(object sequence) {
            ICollection items = sequence as ICollection;
            object len;

            if (items != null) {
                if (this == items) {
                    // list.__init__(l, l) resets l
                    size = 0;
                    return;
                }
                data = new object[items.Count];
                int i = 0;
                foreach (object item in items) {
                    data[i++] = item;
                }
                size = i;
            } else if (Ops.TryInvokeSpecialMethod(sequence, SymbolTable.Length, out len)) {
                int ilen = Converter.ConvertToInt32(len);
                data = new object[ilen];
                size = 0;
                Extend(sequence);
            } else {
                data = new object[20];
                size = 0;
                Extend(sequence);
            }
        }

        public static List Make() {
            return new List();
        }
        public static List Make(object sequence) {
            return new List(sequence);
        }

        private List(IEnumerator e)
            : this(10) {
            while (e.MoveNext()) AddNoLock(e.Current);
        }

        public static List MakeList(params object[] items) {
            return new List(items);
        }

        public static List MakeEmptyList(int capacity) {
            return new List(capacity);
        }

        private List(int capacity) { data = new object[capacity]; size = 0; }
        private List(params object[] items) { data = items; size = data.Length; } //!!! do we need to copy
        public List() : this(20) { }  //!!! figure out the right default size
        internal List(object sequence) {
            ICollection items = sequence as ICollection;
            object len;

            if (items != null) {
                data = new object[items.Count];
                int i = 0;
                foreach (object item in items) {
                    data[i++] = item;
                }
                size = i;
            } else if (Ops.TryInvokeSpecialMethod(sequence, SymbolTable.Length, out len)) {
                int ilen = Converter.ConvertToInt32(len);
                data = new object[ilen];
                Extend(sequence);
            } else {
                data = new object[20];
                Extend(sequence);
            }
        }

        private List(ICollection items)
            : this(items.Count) {
            int i = 0;
            foreach (object item in items) {
                data[i++] = item;
            }
            size = i;
        }

        public static List operator *(List l, int count) {
            return List.MultiplySequenceWorker(l, count);
        }

        public static List operator *(int count, List l) {
            return List.MultiplySequenceWorker(l, count);
        }

        public object[] GetObjectArray() {
            lock (this) {
                object[] ret = new object[size];
                Array.Copy(data, 0, ret, 0, size);
                return ret;
            }
        }

        #region IPythonContainer Members

        [PythonName("__len__")]
        public int GetLength() {
            return size;
        }

        [PythonName("__contains__")]
        public object ContainsValueWrapper(object value) {
            return Ops.Bool2Object(ContainsValue(value));
        }

        public bool ContainsValue(object value) {
            lock (this) {
                for (int i = 0; i < size; i++) {
                    object thisIndex = data[i];

                    // release the lock while we may call user code...
                    Monitor.Exit(this);
                    try {
                        if (Ops.EqualRetBool(thisIndex, value))
                            return true;
                    } finally {
                        Monitor.Enter(this);
                    }
                }
            }
            return false;
        }
        #endregion

        #region ISequence Members

        [PythonName("__add__")]
        public virtual object AddSequence(object other) {
            List l = other as List;
            if (l == null) throw Ops.TypeErrorForBadInstance("can only concatenate list (not \"{0}\") to list", other);

            return AddList(l);
        }

        internal List AddList(List l) {
            // for thread safety we require a temporary copy of their data.
            object[] them = l.GetObjectArray();
            lock (this) {
                object[] ret = new object[size + them.Length];

                Array.Copy(data, 0, ret, 0, size);
                Array.Copy(them, 0, ret, size, them.Length);

                return new List(ret);
            }
        }

        internal void AddRange<T>(ICollection<T> otherList) {
            foreach (object o in otherList) Add(o);
        }

        [PythonName("__iadd__")]
        public virtual object InPlaceAdd(object other) {
            IEnumerator e = Ops.GetEnumerator(other);
            while (e.MoveNext()) {
                Append(e.Current);
            }

            return this;
        }

        [PythonName("__delitem__")]
        public virtual void DeleteItem(int index) {
            lock (this) RawDelete(Ops.FixIndex(index, size));
        }

        [PythonName("__mul__")]
        public virtual object MultiplySequence(object count) {
            return Ops.MultiplySequence<List>(List.MultiplySequenceWorker, this, count);
        }

        private static List MultiplySequenceWorker(List self, int count) {
            if (count <= 0) return MakeEmptyList(0);

            int n, newCount;
            object[] ret;
            lock (self) {
                n = self.size;
                //??? is this useful optimization
                //???if (n == 1) return new List(Array.ArrayList.Repeat(this[0], count));

                newCount = n * count;
                ret = new object[newCount];

                Array.Copy(self.data, 0, ret, 0, n);
            }

            // this should be extremely fast for large count as it uses the same algoithim as efficient integer powers
            // ??? need to test to see how large count and n need to be for this to be fastest approach
            int block = n;
            int pos = n;
            while (pos < newCount) {
                Array.Copy(ret, 0, ret, pos, Math.Min(block, newCount - pos));
                pos += block;
                block *= 2;
            }
            return new List(ret);
        }

        [PythonName("__imul__")]
        public object InPlaceMultiply(int count) {
            lock (this) {
                int n = this.size;
                int newCount = n * count;
                EnsureSize(newCount);

                int block = n;
                int pos = n;
                while (pos < newCount) {
                    Array.Copy(data, 0, data, pos, Math.Min(block, newCount - pos));
                    pos += block;
                    block *= 2;
                }
                this.size = newCount;
            }
            return this;
        }

        [PythonName("__getslice__")]
        public virtual object GetSlice(int start, int stop) {
            if (start < 0) start = 0;
            if (stop > GetLength()) stop = GetLength();
            if (start > stop) return List.MakeEmptyList(0);

            int n = Math.Max(0, stop - start);
            object[] ret = new object[n];
            lock (this) Array.Copy(data, start, ret, 0, n);
            return new List(ret);
        }

        [PythonName("__setslice__")]
        public virtual void SetSlice(int start, int stop, object value) {
            if (start < 0) start = 0;
            if (stop > GetLength()) stop = GetLength();
            if (start > stop) return;

            SliceNoStep(start, stop, value);
        }

        [PythonName("__delslice__")]
        public virtual void DeleteSlice(int start, int stop) {
            if (start < 0) start = 0;
            if (stop > GetLength()) stop = GetLength();
            if (start > stop) return;

            int i = start;
            for (int j = stop; j < size; j++, i++) {
                data[i] = data[j];
            }
            size -= stop - start;
            return;
        }

        public object this[Slice slice] {
            get {
                if (slice == null) throw Ops.TypeError("list indicies must be integer or slice, not None");

                int start, stop, step;
                slice.indices(size, out start, out stop, out step);

                if ((step > 0 && start >= stop) || (step < 0 && start <= stop)) return new List();

                if (step == 1) {
                    int n = Math.Max(0, stop - start);
                    object[] ret = new object[n];
                    lock (this) Array.Copy(data, start, ret, 0, n);
                    return new List(ret);
                } else {
                    // start/stop/step could be near Int32.MaxValue, and simply addition could cause overflow
                    int n = (int)(step > 0 ? (0L + stop - start + step - 1) / step : (0L + stop - start + step + 1) / step);
                    object[] ret = new object[n];
                    lock (this) {
                        int ri = 0;
                        for (int i = 0, index = start; i < n; i++, index += step) {
                            ret[ri++] = data[index];
                        }
                    }
                    return new List(ret);

                }
            }
            set {
                if (slice == null) throw Ops.TypeError("list indicies must be integer or slice, not None");

                if (slice.step != null) {
                    // try to assign back to self: make a copy first
                    if (this == value) value = new List(value);

                    slice.DoSliceAssign(this.SliceAssign, size, value);
                } else {
                    int start, stop, step;
                    slice.indices(size, out start, out stop, out step);
                    if (start > stop) return;

                    SliceNoStep(start, stop, value);
                }
            }
        }

        private void SliceNoStep(int start, int stop, object value) {
            IEnumerator enumerator = Ops.GetEnumerator(value);

            // save a ref to myData incase other calls cause us 
            // to re-size.

            List newList = new List(data.Length); // race is tolerable...

            lock (this) {
                for (int i = 0; i < start; i++) {
                    newList.AddNoLock(data[i]);
                }
            }

            // calling user code, get rid of the lock...
            while (enumerator.MoveNext()) {
                newList.AddNoLock(enumerator.Current);
            }

            lock (this) {
                for (int i = stop; i < size; i++) {
                    newList.AddNoLock(data[i]);
                }

                if (newList.data.Length < data.Length) {
                    // shrinking our array may result in IndexOutOfRange in
                    // this[...] where we read w/o a lock.
                    Array.Copy(newList.data, data, newList.data.Length);
                } else {
                    this.data = newList.data;
                }

                this.size = newList.size;
            }
        }


        private void SliceAssign(int index, object value) {
            this[index] = value;
        }

        [PythonName("__delitem__")]
        public void DeleteItem(Slice slice) {
            lock (this) {
                int start, stop, step;
                // slice is sealed, indicies can't be user code...
                slice.indices(size, out start, out stop, out step);

                if (step > 0 && (start >= stop)) return;
                if (step < 0 && (start <= stop)) return;

                if (step == 1) {
                    int i = start;
                    for (int j = stop; j < size; j++, i++) {
                        data[i] = data[j];
                    }
                    size -= stop - start;
                    return;
                }
                if (step == -1) {
                    int i = stop + 1;
                    for (int j = start + 1; j < size; j++, i++) {
                        data[i] = data[j];
                    }
                    size -= start - stop;
                    return;
                }

                if (step < 0) {
                    // find "start" we will skip in the 1,2,3,... order
                    int i = start;
                    while (i > stop) {
                        i += step;
                    }
                    i -= step;

                    // swap start/stop, make step positive
                    stop = start + 1;
                    start = i;
                    step = -step;
                }

                int curr, skip, move;
                // skip: the next position we should skip
                // curr: the next position we should fill in data
                // move: the next position we will check
                curr = skip = move = start;

                while (curr < stop && move < stop) {
                    if (move != skip) {
                        data[curr++] = data[move];
                    } else
                        skip += step;
                    move++;
                }
                while (stop < size) {
                    data[curr++] = data[stop++];
                }
                size = curr;
            }
        }

        #endregion

        private void RawDelete(int index) {
            int len = size - 1;
            size = len;
            object[] tempData = data;
            for (int i = index; i < len; i++) {
                tempData[i] = tempData[i + 1];
            }
            tempData[len] = null;
        }


        public override string ToString() {
            return Ops.StringRepr(this);
        }

        private void EnsureSize(int needed) {
            if (data.Length >= needed) return;

            int newSize = Math.Max(size * 2, 4);
            while (newSize < needed) newSize *= 2;
            object[] newData = new object[newSize];
            data.CopyTo(newData, 0);
            data = newData;
        }

        [PythonName("append")]
        public void Append(object item) {
            lock (this) {
                AddNoLock(item);
            }
        }

        /// <summary>
        /// Non-thread safe adder, should only be used by internal callers that
        /// haven't yet exposed their list.
        /// </summary>
        internal void AddNoLock(object item) {
            EnsureSize(size + 1);
            data[size] = item;
            size += 1;
        }

        internal void AddNoLockNoDups(object item) {
            for (int i = 0; i < size; i++) {
                if (Ops.EqualRetBool(data[i], item)) {
                    return;
                }
            }

            AddNoLock(item);
        }

        internal void AppendListNoLockNoDups(List list) {
            if (list != null) {
                foreach (object item in list) {
                    AddNoLockNoDups(item);
                }
            }
        }

        [PythonName("count")]
        public int count(object item) {
            lock (this) {
                int cnt = 0;
                for (int i = 0, len = size; i < len; i++) {
                    object val = data[i];

                    Monitor.Exit(this);
                    try {
                        if (Ops.EqualRetBool(val, item)) cnt++;
                    } finally {
                        Monitor.Enter(this);
                    }
                }
                return cnt;
            }
        }

        [PythonName("extend")]
        public void Extend(object seq) {
            //!!! optimize case of easy sequence (List or Tuple)

            IEnumerator i = Ops.GetEnumerator(seq);
            if (seq == (object)this) {
                List other = new List(i);
                i = other.GetEnumerator();
            }
            while (i.MoveNext()) Append(i.Current);
        }

        [PythonName("index")]
        public int Index(object item) {
            return Index(item, 0, size);
        }

        [PythonName("index")]
        public int Index(object item, int start) {
            return Index(item, start, size);
        }

        [PythonName("index")]
        public int Index(object item, int start, int stop) {
            // CPython behavior for index is to only look at the 
            // original items.  If new items are added they
            // are ignored, but if items are removed they
            // aren't iterated.  We therefore get a stable view
            // of our data, and then go with the minimum between
            // our starting size and ending size.

            object[] locData;
            int locSize;
            lock (this) {
                // get a stable view on size / data...
                locData = data;
                locSize = size;
            }

            start = Ops.FixSliceIndex(start, locSize);
            stop = Ops.FixSliceIndex(stop, locSize);

            for (int i = start; i < Math.Min(stop, Math.Min(locSize, size)); i++) {
                if (Ops.EqualRetBool(locData[i], item)) return i;
            }

            throw Ops.ValueError("list.index(item): item not in list");
        }

        [PythonName("index")]
        public int Index(object item, object start) {
            return Index(item, Converter.ConvertToSliceIndex(start), size);
        }

        [PythonName("index")]
        public int Index(object item, object start, object stop) {
            return Index(item, Converter.ConvertToSliceIndex(start), Converter.ConvertToSliceIndex(stop));
        }

        [PythonName("insert")]
        public void insert(int index, object item) {
            if (index >= size) {
                Append(item);
                return;
            }

            lock (this) {
                index = Ops.FixSliceIndex(index, size);

                EnsureSize(size + 1);
                size += 1;
                for (int i = size - 1; i > index; i--) {
                    data[i] = data[i - 1];
                }
                data[index] = item;
            }
        }

        [PythonName("pop")]
        public object Pop() {
            if (this.size == 0) throw Ops.IndexError("pop off of empty list");

            lock (this) {
                this.size -= 1;
                return data[this.size];
            }
        }

        [PythonName("pop")]
        public object Pop(int index) {
            lock (this) {
                index = Ops.FixIndex(index, size);
                if (size == 0) throw Ops.IndexError("pop off of empty list");

                object ret = data[index];
                size -= 1;
                for (int i = index; i < size; i++) {
                    data[i] = data[i + 1];
                }
                return ret;
            }
        }

        [PythonName("remove")]
        public void remove(object item) {
            lock (this) RawDelete(Index(item));
        }

        [PythonName("reverse")]
        public void Reverse() {
            lock (this) Array.Reverse(data, 0, size);
        }

        internal void Reverse(int index, int count) {
            lock (this) Array.Reverse(data, index, count);
        }

        private class DefaultPythonComparer : IComparer {
            public static DefaultPythonComparer Instance = new DefaultPythonComparer();
            public DefaultPythonComparer() { }

            public int Compare(object x, object y) {
                //??? Putting this optimization here is awfully special case, but comes close to halving sort time for int lists
                //				if (x is int && y is int) {
                //					int xi = (int)x;
                //					int yi = (int)y;
                //					return xi == yi ? 0 : (xi < yi ? -1 : +1);
                //				}

                return Ops.Compare(x, y);
            }
        }

        private class FunctionComparer : IComparer {
            //??? optimized version when we know we have a Function
            private object cmpfunc;
            public FunctionComparer(object cmpfunc) { this.cmpfunc = cmpfunc; }

            public int Compare(object o1, object o2) {
                object res = Ops.Call(cmpfunc, o1, o2);
                if (res is int) {
                    return (int)res;
                }

                return Converter.ConvertToInt32(res);
            }
        }

        [PythonName("sort")]
        public void Sort() {
            Sort(null, null, false);
        }

        [PythonName("sort")]
        public void Sort(object cmp) {
            Sort(cmp, null, false);
        }

        [PythonName("sort")]
        public void Sort(object cmp, object key) {
            Sort(cmp, key, false);
        }

        [PythonName("sort")]
        public void Sort([DefaultParameterValueAttribute(null)] object cmp,
                         [DefaultParameterValueAttribute(null)] object key,
                         [DefaultParameterValueAttribute(false)] bool reverse) {
            IComparer comparer = (cmp == null) ?
                (IComparer)DefaultPythonComparer.Instance :
                (IComparer)new FunctionComparer(cmp);

            DoSort(comparer, key, reverse, 0, size);
        }

        internal void DoSort(IComparer cmp, object key, bool reverse, int index, int count) {
            lock (this) {
                object[] sortData = data;
                int sortSize = size;

                try {
                    // make the list appear empty for the duration of the sort...
                    data = Ops.EMPTY;
                    size = 0;

                    if (key != null) {
                        object[] keys = new object[sortSize];
                        for (int i = 0; i < sortSize; i++) {
                            Debug.Assert(data.Length == 0);
                            keys[i] = Ops.Call(key, sortData[i]);
                            if (data.Length != 0) throw Ops.ValueError("list mutated while determing keys");
                        }

                        sortData = ListMergeSort(sortData, keys, cmp, index, count, reverse);
                    } else {
                        sortData = ListMergeSort(sortData, cmp, index, count, reverse);
                    }
                } finally {
                    // restore the list to it's old data & size (which is now supported appropriately)
                    data = sortData;
                    size = sortSize;
                }

            }
        }

        internal object[] ListMergeSort(object[] sortData, IComparer cmp, int index, int count, bool reverse) {
            return ListMergeSort(sortData, null, cmp, index, count, reverse);
        }

        internal object[] ListMergeSort(object[] sortData, object[] keys, IComparer cmp, int index, int count, bool reverse) {
            if (count - index < 2) return sortData;  // 1 or less items, we're sorted, quit now...

            if (keys == null) keys = sortData;
            // list merge sort - stable sort w/ a minimum # of comparisons.

            int len = count - index;
            // prepare the two lists.
            int[] lists = new int[len + 2];    //0 and count + 1 are auxillary fields

            lists[0] = 1;
            lists[len + 1] = 2;
            for (int i = 1; i <= len - 2; i++) {
                lists[i] = -(i + 2);
            }

            lists[len - 1] = lists[len] = 0;

            // new pass
            for (; ; ) {
                // p & q  traverse the lists during each pass.  
                //  s is usually the most most recently processed record of the current sublist
                //  t points to the end of the previously output sublist
                int s = 0;
                int t = len + 1;
                int p = lists[s];
                int q = lists[t];

                if (q == 0) break;  // we're done
                for (; ; ) {
                    // Indexes into the array here are 1 based.  0 is a 
                    // virtual element and so is (len - 1) - they only exist in
                    // the length array.

                    if ((p < 1) || (q <= len && DoCompare(keys, cmp, p + index - 1, q + index - 1, reverse))) {
                        // advance p
                        if (lists[s] < 0) lists[s] = Math.Abs(p) * -1;
                        else lists[s] = Math.Abs(p);

                        s = p;
                        p = lists[p];

                        if (p > 0) continue;

                        // complete the sublist
                        lists[s] = q;
                        s = t;
                        do {
                            t = q;
                            q = lists[q];
                        } while (q > 0);
                    } else {
                        // advance q
                        if (lists[s] < 0) lists[s] = Math.Abs(q) * -1;
                        else lists[s] = Math.Abs(q);

                        s = q;
                        q = lists[q];

                        if (q > 0) continue;

                        // Complete the sublist
                        lists[s] = p;
                        s = t;

                        do {
                            t = p;
                            p = lists[p];
                        } while (p > 0);
                    }

                    Debug.Assert(p <= 0);
                    Debug.Assert(q <= 0);
                    p *= -1;
                    q *= -1;

                    if (q == 0) {
                        if (lists[s] < 0) lists[s] = Math.Abs(p) * -1;
                        else lists[s] = Math.Abs(p);
                        lists[t] = 0;
                        // go back to new pass
                        break;
                    } // else keep going
                }
            }

            // use the resulting indicies to
            // extract the order.
            object[] newData = new object[len];
            int start = lists[0];
            int outIndex = 0;
            while (start != 0) {
                newData[outIndex++] = sortData[start + index - 1];
                start = lists[start];
            }

            if (sortData.Length != count || index != 0) {
                for (int j = 0; j < count; j++) {
                    sortData[j + index] = newData[j];
                }
            } else {
                sortData = newData;
            }

            return sortData;
        }

        /// <summary>
        /// Compares the two specified keys
        /// </summary>
        private bool DoCompare(object[] keys, IComparer cmp, int p, int q, bool reverse) {
            Debug.Assert(data.Length == 0);

            int result = cmp.Compare(keys[p], keys[q]);
            bool ret = reverse ? (result >= 0) : (result <= 0);

            if (data.Length != 0) throw Ops.ValueError("list mutated during sort");
            return ret;
        }

        internal int BinarySearch(int index, int count, object value, IComparer comparer) {
            lock (this) return Array.BinarySearch(data, index, count, value, comparer);
        }

        public int CompareTo(object obj) {
            List l = obj as List;
            if (l == null) throw new ArgumentException("expected list");

            CompareUtil.Push(this, obj);
            try {
                return CompareToWorker(l);
            } finally {
                CompareUtil.Pop(this, obj);
            }
        }

        private int CompareToWorker(List l) {
            // we need to lock both objects (or copy all of one's data w/ it's lock held, and
            // then compare, which is bad).  Therefore we have a strong order for locking on 
            // the lists
            int result;
            if (this.GetHashCode() < l.GetHashCode()) {
                lock (this) lock (l) result = Ops.CompareArrays(data, size, l.data, l.size);
            } else if (this.GetHashCode() != l.GetHashCode()) {
                lock (l) lock (this) result = Ops.CompareArrays(data, size, l.data, l.size);
            } else {
                // rare, but possible.  We need a second opinion
                if (IdDispenser.GetId(this) < IdDispenser.GetId(l))
                    lock (this) lock (l) result = Ops.CompareArrays(data, size, l.data, l.size);
                else
                    lock (l) lock (this) result = Ops.CompareArrays(data, size, l.data, l.size);
            }
            return result;
        }

        #region IList Members

        public bool IsReadOnly {
            get { return false; }
        }

        public virtual object this[int index] {
            get {
                // no locks works here, we either return an
                // old item (as if we were called first) or return
                // a current item...
                return data[Ops.FixIndex(index, size)];
            }
            set {
                // but we need a lock here incase we're assigning
                // while re-sizing.
                lock (this) data[Ops.FixIndex(index, size)] = value;
            }
        }

        public void RemoveAt(int index) {
            lock (this) RawDelete(index);
        }

        public void Insert(int index, object value) {
            insert(index, value);
        }

        public void Remove(object value) {
            remove(value);
        }

        public bool Contains(object value) {
            return ContainsValue(value);
        }

        public void Clear() {
            lock (this) size = 0;
        }

        public int IndexOf(object value) {
            // we get a stable view of the list, and if user code
            // clears it then we'll stop iterating.
            object[] locData;
            int locSize;
            lock (this) {
                locData = data;
                locSize = size;
            }

            for (int i = 0; i < Math.Min(locSize, size); i++) {
                if (Ops.EqualRetBool(locData[i], value)) return i;
            }
            return -1;
        }

        public int Add(object value) {
            lock (this) {
                AddNoLock(value);
                return size - 1;
            }
        }

        public bool IsFixedSize {
            get { return false; }
        }

        #endregion

        #region ICollection Members

        public bool IsSynchronized {
            get { return false; }
        }

        public int Count {
            get { return size; }
        }

        public void CopyTo(Array array, int index) {
            Array.Copy(data, 0, array, index, size);
        }

        public void CopyTo(Array array, int index, int arrayIndex, int count) {
            Array.Copy(data, index, array, arrayIndex, count);
        }

        public object SyncRoot {
            get {
                return this;
            }
        }

        #endregion

        #region IEnumerable Members

        [PythonName("__iter__")]
        public virtual IEnumerator GetEnumerator() {
            return new ListEnumeratorCollection(this);
        }

        #endregion

        public class ListEnumeratorCollection : IEnumerator, IEnumerable {
            private int index = -1;
            private List l;
            private bool sinkState = false;

            public ListEnumeratorCollection(List l) { this.l = l; }

            #region IEnumerator Members

            public void Reset() {
                index = -1;
            }

            public object Current {
                get {
                    return l.data[index];
                }
            }

            public bool MoveNext() {
                if (sinkState) return false;

                index++;
                bool hit = index > l.size - 1;
                if (hit) sinkState = true;
                return !hit;
            }

            #endregion

            #region IEnumerable Members

            [PythonName("__iter__")]
            public IEnumerator GetEnumerator() {
                return this;
            }

            #endregion
        }

        #region ICodeFormattable Members

        public string ToCodeString() {
            StringBuilder buf = new StringBuilder();
            buf.Append("[");
            for (int i = 0; i < size; i++) {
                if (i > 0) buf.Append(", ");
                buf.Append(Ops.StringRepr(data[i]));
            }
            buf.Append("]");
            return buf.ToString();
        }

        #endregion

        #region IRichEquality Members

        [PythonName("__hash__")]
        public virtual object RichGetHashCode() {
            throw Ops.TypeError("list object is unhashable");
        }

        [PythonName("__eq__")]
        public virtual object RichEquals(object other) {
            List l = other as List;
            if (l == null) return Ops.NotImplemented;

            if (l.Count != this.Count) return Ops.FALSE;
            return Ops.Bool2Object(CompareTo(l) == 0);
        }

        [PythonName("__ne__")]
        public virtual object RichNotEquals(object other) {
            object res = RichEquals(other);
            if (res != Ops.NotImplemented) return Ops.Not(res);

            return Ops.NotImplemented;
        }

        #endregion

        #region IDynamicObject Members

        public DynamicType GetDynamicType() {
            return TypeCache.List;
        }

        #endregion

        #region IList<object> Members

        int IList<object>.IndexOf(object item) {
            return this.IndexOf(item);
        }

        void IList<object>.Insert(int index, object item) {
            this.Insert(index, item);
        }

        void IList<object>.RemoveAt(int index) {
            this.RemoveAt(index);
        }

        object IList<object>.this[int index] {
            get {
                return this[index];
            }
            set {
                this[index] = value;
            }
        }

        #endregion

        #region ICollection<object> Members

        void ICollection<object>.Add(object item) {
            this.Add(item);
        }

        void ICollection<object>.Clear() {
            this.Clear();
        }

        bool ICollection<object>.Contains(object item) {
            return this.Contains(item);
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
            get { return this.IsReadOnly; }
        }

        bool ICollection<object>.Remove(object item) {
            if (this.Contains(item)) {
                this.Remove(item);
                return true;
            }
            return false;
        }

        #endregion

        #region IEnumerable<object> Members

        IEnumerator<object> IEnumerable<object>.GetEnumerator() {
            return new IEnumeratorOfTWrapper<object>(GetEnumerator());
        }

        #endregion
    }
}
