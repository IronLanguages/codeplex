/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using Microsoft.Scripting;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Internal;

namespace IronPython.Runtime {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix"), PythonType("xrange")]
    public class XRange : ISequence, IEnumerable, IEnumerable<int> {
        internal int _start, _stop, _step, _length;
        internal bool _overflow = false;

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
            this._start = start;
            this._stop = stop;
            this._step = step;
            this._length = GetLengthHelper();
            this._stop = start + step * this._length; // make stop precise
            if ((step > 0 && saved > this._stop) || (step < 0 && saved < this._stop)) {
                _overflow = true;
            }
        }

        #region ISequence Members
        [OperatorMethod, PythonName("__len__")]
        public int GetLength() {
            return _length;
        }

        private int GetLengthHelper() {
            long temp;
            if (_step > 0) {
                temp = (0L + _stop - _start + _step - 1) / _step;
            } else {
                temp = (0L + _stop - _start + _step + 1) / _step;
            }

            if (temp > Int32.MaxValue) {
                throw Ops.OverflowError("xrange() result has too many items");
            }
            return (int)temp;
        }

        public bool ContainsValue(object value) {
            throw new NotImplementedException();
        }

        public virtual object this[int index] {
            get {
                if (index < 0) index += _length;

                if (index >= _length || index < 0)
                    throw Ops.IndexError("xrange object index out of range");

                int ind = index * _step + _start;
                return RuntimeHelpers.Int32ToObject(ind);
            }
        }

        public object AddSequence(object other) {
            throw Ops.TypeErrorForBadInstance("unsupported operand type(s) for +: 'xrange' and {0}", other);
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
            return new XRangeIterator(new XRange(_stop - _step, _start - _step, -_step));
        }

        public IEnumerator GetEnumerator() {
            return new XRangeIterator(this);
        }

        public override string ToString() {
            if (_step == 1) {
                if (_start == 0) {
                    return string.Format("xrange({0})", _stop);
                } else {
                    return string.Format("xrange({0}, {1})", _start, _stop);
                }
            } else {
                return string.Format("xrange({0}, {1}, {2})", _start, _stop, _step);
            }
        }

        #region IEnumerable<int> Members

        IEnumerator<int> IEnumerable<int>.GetEnumerator() {
            return new XRangeIterator(this);
        }

        #endregion
    }
    public class XRangeIterator : IEnumerator, IEnumerator<int> {
        private XRange l;
        private int value;
        private int position;

        public XRangeIterator(XRange l) {
            this.l = l;
            this.value = l._start - l._step; // this could cause overflow, fine
            this.position = 0;
        }

        public object Current {
            get { return RuntimeHelpers.Int32ToObject(value); }
        }

        public bool MoveNext() {
            if (position >= l._length) return false;

            this.position++;
            this.value = this.value + l._step;
            return true;
        }

        public void Reset() {
            value = l._start - l._step;
            position = 0;
        }

        #region IEnumerator<int> Members

        int IEnumerator<int>.Current {
            get { return value; }
        }

        #endregion

        #region IDisposable Members

        public void Dispose() {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool notFinalizing) {
        }
        #endregion
    }
}
