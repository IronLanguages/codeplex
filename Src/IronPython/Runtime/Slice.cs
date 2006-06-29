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
using System.Text;
using System.Collections;

using IronPython.Runtime.Operations;

namespace IronPython.Runtime {
    [PythonType("slice")]
    public sealed class Slice : IComparable, IRichEquality {
        public readonly object start, stop, step;

        public Slice(object stop) : this(null, stop, null) { }
        public Slice(object start, object stop) : this(start, stop, null) { }
        public Slice(object start, object stop, object step) {
            this.start = start; this.stop = stop; this.step = step;
        }

        [PythonName("indices")]
        public Tuple indices(int len) {
            int istart, istop, istep;
            indices(len, out istart, out istop, out istep);
            return Tuple.MakeTuple(istart, istop, istep);
        }

        [PythonName("indices")]
        public void indices(int len, out int ostart, out int ostop, out int ostep) {
            int count;
            Ops.FixSlice(len, start, stop, step, out ostart, out ostop, out ostep, out count);
        }

        /// <summary>
        /// Gets the indicies for the deprecated __getslice__, __setslice__, __delslice__ functions
        /// 
        /// This form is deprecated in favor of using __getitem__ w/ a slice object as an index.  This
        /// form also has subtly different mechanisms for fixing the slice index before calling the function.
        /// 
        /// If an index is negative and __len__ is not defined on the object than an AttributeError
        /// is raised.
        /// </summary>
        internal void DeprecatedFixed(object self, out int newStart, out int newStop) {
            bool calcedLength = false;  // only call __len__ once, even if we need it twice
            int length = 0;

            if (start != null) {
                newStart = Converter.ConvertToSliceIndex(start);
                if (newStart < 0) {
                    calcedLength = true;
                    length = Converter.ConvertToInt32(Ops.Invoke(self, SymbolTable.Length)); ;

                    newStart += length;
                }
            } else {
                newStart = 0;
            }

            if (stop != null) {
                newStop = Converter.ConvertToSliceIndex(stop);
                if (newStop < 0) {
                    if (!calcedLength) length = Converter.ConvertToInt32(Ops.Invoke(self, SymbolTable.Length)); ;

                    newStop += length;
                }
            } else {
                newStop = Int32.MaxValue;
            }

        }        

        #region Object overrides

        [PythonName("__str__")]
        public override string ToString() {
            return string.Format("slice({0}, {1}, {2})", Ops.StringRepr(start), Ops.StringRepr(stop), Ops.StringRepr(step));
        }

        public override bool Equals(object obj) {
            return Ops.IsTrue(RichEquals(obj));
        }

        public override int GetHashCode() {
            int hash = 0;
            if (start != null) hash ^= start.GetHashCode();
            if (stop !=null) hash ^= stop.GetHashCode();
            if (step != null) hash ^= step.GetHashCode();
            return hash;
        }

        #endregion

        [PythonName("__cmp__")]
        public int CompareTo(object other) {
            Slice s = other as Slice;
            if (s == null) throw new ArgumentException("expected slice");
            return Ops.CompareArrays(new object[] { start, stop, step }, 3,
                new object[] { s.start, s.stop, s.step }, 3);
        }

        #region IRichEquality Members

        [PythonName("__hash__")]
        public object RichGetHashCode() {
            throw Ops.TypeErrorForUnhashableType("slice");
        }

        public object RichEquals(object other) {
            Slice s = other as Slice;
            if (s == null) return Ops.FALSE;
            return Ops.Bool2Object((Ops.Compare(start, s.start) == 0) &&
                (Ops.Compare(stop, s.stop) == 0) &&
                (Ops.Compare(step, s.step) == 0));
        }

        public object RichNotEquals(object other) {
            object res = RichEquals(other);
            if (res != Ops.NotImplemented) return Ops.Not(res);

            return Ops.NotImplemented;
        }

        #endregion

        internal delegate void SliceAssign(int index, object value);

        internal void DoSliceAssign(SliceAssign assign, int size, object value) {
            int ostart, ostop, ostep;
            indices(size, out ostart, out ostop, out ostep);

            if (this.step == null) throw Ops.ValueError("cannot do slice assignment w/ no step");

            DoSliceAssign(assign, ostart, ostop, ostep, value);            
        }

        private static void DoSliceAssign(SliceAssign assign, int start, int stop, int step, object value) {
            int n = (step > 0 ? (stop - start + step - 1) : (stop - start + step + 1)) / step;
            // fast paths, if we know the size then we can
            // do this quickly.
            if (value is IList) {
                ListSliceAssign(assign, start, n, step, value as IList);
            } else if (value is ISequence) {
                SequenceSliceAssign(assign, start, n, step, value as ISequence);
            } else {
                OtherSliceAssign(assign, start, stop, step, value);
            }
        }

        private static void ListSliceAssign(SliceAssign assign, int start, int n, int step, IList lst) {
            if (lst.Count < n) throw Ops.ValueError("too few items in the enumerator. need {0} have {1}", n,lst.Count);
            else if (lst.Count != n) throw Ops.ValueError("too many items in the enumerator need {0} have {1}", n, lst.Count);

            for (int i = 0, index = start; i < n; i++, index += step) {
                assign(index, lst[i]);
            }
        }

        private static void SequenceSliceAssign(SliceAssign assign, int start, int n, int step, ISequence lst) {
            if (lst.GetLength() < n) throw Ops.ValueError("too few items in the enumerator. need {0}", n);
            else if (lst.GetLength() != n) throw Ops.ValueError("too many items in the enumerator need {0}", n);

            for (int i = 0, index = start; i < n; i++, index += step) {
                assign(index, lst[i]);
            }

        }

        private static void OtherSliceAssign(SliceAssign assign, int start, int stop, int step, object value) {
            // get enumerable data into a list, and then
            // do the slice.
            IEnumerator enumerator = Ops.GetEnumerator(value);
            List sliceData = new List();
            while (enumerator.MoveNext()) sliceData.AddNoLock(enumerator.Current);

            DoSliceAssign(assign, start, stop, step, sliceData);
        }
    }
}