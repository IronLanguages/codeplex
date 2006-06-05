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

namespace IronPython.Runtime {
    [PythonType("xrange")]
    public class XRange : ISequence, IEnumerable {
        internal int start, stop, step, length;
        internal bool overflow = false;

        public XRange(int stop) : this(0, stop, 1) { }
        public XRange(int start, int stop) : this(start, stop, 1) { }

        public XRange(int start, int stop, int step) {
            Initialize(start, stop, step);
        }

        private void Initialize(int start, int stop, int step) {
            if (step == 0) {
                throw Ops.ValueError("step must not be zero");
            } else if (step > 0) {
                if (start > stop) stop = start;
            } else {
                if (start < stop) stop = start;
            }

            int saved = stop;
            this.start = start;
            this.stop = stop;
            this.step = step;
            this.length = GetLengthHelper();
            this.stop = start + step * this.length; // make stop precise
            if ((step > 0 && saved > this.stop) || (step < 0 && saved < this.stop)) {
                overflow = true;
            }
        }

        public XRange(object stop) : this(0, stop, 1) { }
        public XRange(object start, object stop) : this(start, stop, 1) { }
        public XRange(object start, object stop, object step) {
            int start1 = Converter.ConvertToXRangeIndex(start);
            int stop1 = Converter.ConvertToXRangeIndex(stop);
            int step1 = Converter.ConvertToXRangeIndex(step);
            Initialize(start1, stop1, step1);
        }

        #region ISequence Members
        [PythonName("__len__")]
        public int GetLength() {
            return length;
        }

        private int GetLengthHelper() {
            long temp;
            if (step > 0) {
                temp = (0L + stop - start + step - 1) / step;
            } else {
                temp = (0L + stop - start + step + 1) / step;
            }

            if (temp > Int32.MaxValue) {
                throw Ops.OverflowError("xrange() result has too many items");
            }
            return (int)temp;
        }

        public bool ContainsValue(object item) {
            throw new NotImplementedException();
        }

        public virtual object this[int index] {
            get {
                if (index < 0) index += length;

                if (index >= length || index < 0)
                    throw Ops.IndexError("xrange object index out of range");

                int ind = index * step + start;
                return Ops.Int2Object(ind);
            }
        }

        public object AddSequence(object other) {
            throw Ops.TypeError("unsupported operand type(s) for +: 'xrange' and '{0}'", Ops.GetDynamicType(other).__name__);
        }

        public object MultiplySequence(object count) {
            throw Ops.TypeError("unsupported operand type(s) for *: 'xrange' and 'int'");
        }

        [PythonName("__getslice__")]
        public virtual object GetSlice(int start, int stop) {
            throw Ops.TypeError("sequence index must be integer");
        }

        public object this[Slice slice] {
            get {
                throw Ops.TypeError("sequence index must be integer");
            }
        }

        #endregion

        [PythonName("__reversed__")]
        public object Reversed() {
            return new XRangeIterator(new XRange(stop - step, start - step, -step));
        }

        public IEnumerator GetEnumerator() {
            return new XRangeIterator(this);
        }

        public override string ToString() {
            if (step == 1) {
                if (start == 0) {
                    return string.Format("xrange({0})", stop);
                } else {
                    return string.Format("xrange({0}, {1})", start, stop);
                }
            } else {
                return string.Format("xrange({0}, {1}, {2})", start, stop, step);
            }
        }
    }
    public class XRangeIterator : IEnumerator {
        private XRange l;
        private int value;
        private int position;

        public XRangeIterator(XRange l) {
            this.l = l;
            this.value = l.start - l.step; // this could cause overflow, fine
            this.position = 0;
        }

        public object Current {
            get { return Ops.Int2Object(value); }
        }

        public bool MoveNext() {
            if (position >= l.length) return false;

            this.position++;
            this.value = this.value + l.step;
            return true;
        }

        public void Reset() {
            value = l.start - l.step;
            position = 0;
        }
    }
}
