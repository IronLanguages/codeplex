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
using System.Text;
using System.Collections;

using IronPython.Runtime.Operations;

using Microsoft.Scripting;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime {
    [PythonType("slice")]
    public sealed class Slice : IComparable, IValueEquality, ISlice {
        private readonly object start, stop, step;

        public Slice(object stop) : this(null, stop, null) { }
        public Slice(object start, object stop) : this(start, stop, null) { }
        public Slice(object start, object stop, object step) {
            this.start = start; this.stop = stop; this.step = step;
        }

        #region ISlice Members

        public object Start {
            [PythonName("start")]
            get { return start; }
        }

        public object Stop {
            [PythonName("stop")]
            get { return stop; }
        }

        public object Step {
            [PythonName("step")]
            get { return step; }
        }

        #endregion

        [PythonName("indices")]
        public void Indices(int len, out int ostart, out int ostop, out int ostep) {
            int count;
            PythonOps.FixSlice(len, start, stop, step, out ostart, out ostop, out ostep, out count);
        }

        [PythonName("indices")]
        public void Indices(object len, out int ostart, out int ostop, out int ostep) {
            int count;
            PythonOps.FixSlice(Converter.ConvertToIndex(len), start, stop, step, out ostart, out ostop, out ostep, out count);
        }

        internal static void FixSliceArguments(int size, ref int start, ref int stop) {
            start = start < 0 ? 0 : start > size ? size : start;
            stop  = stop  < 0 ? 0 : stop  > size ? size : stop;
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
                newStart = Converter.ConvertToIndex(start);
                if (newStart < 0) {
                    calcedLength = true;
                    length = PythonOps.Length(self);

                    newStart += length;
                }
            } else {
                newStart = 0;
            }

            if (stop != null) {
                newStop = Converter.ConvertToIndex(stop);
                if (newStop < 0) {
                    if (!calcedLength) length = PythonOps.Length(self);

                    newStop += length;
                }
            } else {
                newStop = Int32.MaxValue;
            }

        }

        #region Object overrides

        public override string ToString() {
            return string.Format("slice({0}, {1}, {2})", PythonOps.StringRepr(start), PythonOps.StringRepr(stop), PythonOps.StringRepr(step));
        }

        public override bool Equals(object obj) {
            Slice s = obj as Slice;
            if (s == null) return false;

            return (PythonOps.Compare(start, s.start) == 0) &&
                (PythonOps.Compare(stop, s.stop) == 0) &&
                (PythonOps.Compare(step, s.step) == 0);
        }

        public override int GetHashCode() {
            int hash = 0;
            if (start != null) hash ^= start.GetHashCode();
            if (stop != null) hash ^= stop.GetHashCode();
            if (step != null) hash ^= step.GetHashCode();
            return hash;
        }

        #endregion

        [PythonName("__cmp__")]
        public int CompareTo(object obj) {
            Slice s = obj as Slice;
            if (s == null) throw new ArgumentException("expected slice");
            return PythonOps.CompareArrays(new object[] { start, stop, step }, 3,
                new object[] { s.start, s.stop, s.step }, 3);
        }

        #region IValueEquality Members

        public int GetValueHashCode() {
            throw PythonOps.TypeErrorForUnhashableType("slice");
        }
        
        // slice is sealed so equality doesn't need to be virtual and can be the IValueEquality
        // interface implementation
        [PythonName("__eq__")]  
        public bool ValueEquals(object other) {
            return Equals(other);
        }

        [PythonName("__ne__")]
        public bool ValueNotEquals(object other) {
            return !Equals(other);
        }

        #endregion

        internal delegate void SliceAssign(int index, object value);

        internal void DoSliceAssign(SliceAssign assign, int size, object value) {
            int ostart, ostop, ostep;
            Indices(size, out ostart, out ostop, out ostep);

            if (this.step == null) throw PythonOps.ValueError("cannot do slice assignment w/ no step");

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
            if (lst.Count < n) throw PythonOps.ValueError("too few items in the enumerator. need {0} have {1}", n, lst.Count);
            else if (lst.Count != n) throw PythonOps.ValueError("too many items in the enumerator need {0} have {1}", n, lst.Count);

            for (int i = 0, index = start; i < n; i++, index += step) {
                assign(index, lst[i]);
            }
        }

        private static void SequenceSliceAssign(SliceAssign assign, int start, int n, int step, ISequence lst) {
            if (lst.GetLength() < n) throw PythonOps.ValueError("too few items in the enumerator. need {0}", n);
            else if (lst.GetLength() != n) throw PythonOps.ValueError("too many items in the enumerator need {0}", n);

            for (int i = 0, index = start; i < n; i++, index += step) {
                assign(index, lst[i]);
            }

        }

        private static void OtherSliceAssign(SliceAssign assign, int start, int stop, int step, object value) {
            // get enumerable data into a list, and then
            // do the slice.
            IEnumerator enumerator = PythonOps.GetEnumerator(value);
            List sliceData = new List();
            while (enumerator.MoveNext()) sliceData.AddNoLock(enumerator.Current);

            DoSliceAssign(assign, start, stop, step, sliceData);
        }

        //public static bool operator ==(Slice self, Slice other) {
        //    return EqualsWorker(self, other);
        //}

        private static bool EqualsWorker(Slice self, Slice other) {
            if (Object.ReferenceEquals(self, other)) return true;

            if (Object.ReferenceEquals(self, null) || object.ReferenceEquals(other, null)) return false;

            return PythonOps.EqualRetBool(self.start, other.start) &&
                PythonOps.EqualRetBool(self.stop, other.stop) &&
                PythonOps.EqualRetBool(self.step, other.step);
        }

        //public static bool operator !=(Slice self, Slice other) {
        //    return !EqualsWorker(self, other);
        //}

        public static bool operator >(Slice self, Slice other) {
            return self.CompareTo(other) > 0;
        }

        public static bool operator <(Slice self, Slice other) {
            return self.CompareTo(other) < 0;
        }
    }
}