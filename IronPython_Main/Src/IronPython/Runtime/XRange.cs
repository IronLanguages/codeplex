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
using Microsoft.Scripting.Runtime;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    [PythonSystemType("xrange")]
    public sealed class XRange : ISequence, ICollection, IEnumerable, IEnumerable<int>, ICodeFormattable {
        private int _start, _stop, _step, _length;

        public XRange(int stop) : this(0, stop, 1) { }
        public XRange(int start, int stop) : this(start, stop, 1) { }

        public XRange(int start, int stop, int step) {
            Initialize(start, stop, step);
        }

        private void Initialize(int start, int stop, int step) {
            if (step == 0) {
                throw PythonOps.ValueError("step must not be zero");
            } else if (step > 0) {
                if (start > stop) stop = start;
            } else {
                if (start < stop) stop = start;
            }

            _start = start;
            _stop = stop;
            _step = step;
            _length = GetLengthHelper();
            _stop = start + step * _length; // make stop precise
        }

        #region ISequence Members

        public int __len__() {
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
                throw PythonOps.OverflowError("xrange() result has too many items");
            }
            return (int)temp;
        }

        public object this[int index] {
            get {
                if (index < 0) index += _length;

                if (index >= _length || index < 0)
                    throw PythonOps.IndexError("xrange object index out of range");

                int ind = index * _step + _start;
                return RuntimeHelpers.Int32ToObject(ind);
            }
        }

        public object this[object index] {
            get {
                return this[Converter.ConvertToIndex(index)];
            }
        }

        public object __getslice__(int start, int stop) {
            throw PythonOps.TypeError("sequence index must be integer");
        }

        public object this[Slice slice] {
            get {
                throw PythonOps.TypeError("sequence index must be integer");
            }
        }

        #endregion

        public IEnumerator __reversed__() {
            return new XRangeIterator(new XRange(_stop - _step, _start - _step, -_step));
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return new XRangeIterator(this);
        }

        #region IEnumerable<int> Members

        IEnumerator<int> IEnumerable<int>.GetEnumerator() {
            return new XRangeIterator(this);
        }

        #endregion

        class XRangeIterator : IEnumerator, IEnumerator<int> {
            private XRange _xrange;
            private int _value;
            private int _position;

            public XRangeIterator(XRange xrange) {
                _xrange = xrange;
                _value = xrange._start - xrange._step; // this could cause overflow, fine
                _position = 0;
            }

            public object Current {
                get {
                    return RuntimeHelpers.Int32ToObject(_value);
                }
            }

            public bool MoveNext() {
                if (_position >= _xrange._length) {
                    return false;
                }

                _position++;
                _value = _value + _xrange._step;
                return true;
            }

            public void Reset() {
                _value = _xrange._start - _xrange._step;
                _position = 0;
            }

            #region IEnumerator<int> Members

            int IEnumerator<int>.Current {
                get { return _value; }
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

        #region ICodeFormattable Members

        public string/*!*/ __repr__(CodeContext/*!*/ context) {
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

        #endregion

        #region ICollection Members

        void ICollection.CopyTo(Array array, int index) {
            foreach (object o in this) {
                array.SetValue(o, index++);
            }
        }

        int ICollection.Count {
            get { return _length; }
        }

        bool ICollection.IsSynchronized {
            get { return false; }
        }

        object ICollection.SyncRoot {
            get { return null; }
        }

        #endregion
    }
}
