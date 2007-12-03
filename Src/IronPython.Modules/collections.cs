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

using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

using Microsoft.Scripting;

[assembly: PythonModule("collections", typeof(IronPython.Modules.PythonCollections))]
namespace IronPython.Modules {
    [Documentation("High performance data structures\n")]
    [PythonType("collections")]
    public class PythonCollections {
        public static object deque = DynamicHelpers.GetPythonTypeFromType(typeof(PythonDequeCollection));

        [PythonType("deque")]
        public class PythonDequeCollection : IEnumerable, IComparable, ICodeFormattable, IValueEquality, ICollection {
            private object[] _data;
            private object _lockObj = new object();
            private int _head, _tail;
            private int _itemCnt, _version;

            public PythonDequeCollection() {
                Clear();
            }

            [PythonName("__init__")]
            public void Initialize() {
            }

            [PythonName("__init__")]
            public void Initialize(object iterable) {
                Extend(iterable);
            }

            #region core deque APIs
            [PythonName("append")]
            public void Append(object x) {
                lock (_lockObj) {
                    _version++;

                    if (_itemCnt == _data.Length) {
                        GrowArray();
                    }

                    _itemCnt++;
                    _data[_tail++] = x;
                    if (_tail == _data.Length) {
                        _tail = 0;
                    }
                }
            }

            [PythonName("appendleft")]
            public void AppendLeft(object x) {
                lock (_lockObj) {
                    _version++;

                    if (_itemCnt == _data.Length) {
                        GrowArray();
                    }

                    _itemCnt++;
                    --_head;
                    if (_head < 0) {
                        _head = _data.Length - 1;
                    }

                    _data[_head] = x;
                }
            }

            [PythonName("clear")]
            public void Clear() {
                lock (_lockObj) {
                    _version++;

                    _head = _tail = 0;
                    _itemCnt = 0;
                    _data = new object[8];
                }
            }

            [PythonName("extend")]
            public void Extend(object iterable) {
                IEnumerator e = PythonOps.GetEnumerator(iterable);
                while (e.MoveNext()) {
                    Append(e.Current);
                }
            }

            [PythonName("extendleft")]
            public void ExtendLeft(object iterable) {
                IEnumerator e = PythonOps.GetEnumerator(iterable);
                while (e.MoveNext()) {
                    AppendLeft(e.Current);
                }
            }

            [PythonName("pop")]
            public object Pop() {
                lock (_lockObj) {
                    if (_itemCnt == 0) throw PythonOps.IndexError("pop from an empty deque");

                    _version++;
                    if (_tail != 0) {
                        _tail--;
                    } else {
                        _tail = _data.Length - 1;
                    }
                    _itemCnt--;

                    object res = _data[_tail];
                    _data[_tail] = null;
                    return res;
                }
            }

            [PythonName("popleft")]
            public object PopLeft() {
                lock (_lockObj) {
                    if (_itemCnt == 0) throw PythonOps.IndexError("pop from an empty deque");

                    _version++;
                    object res = _data[_head];
                    _data[_head] = null;

                    if (_head != _data.Length - 1) {
                        _head++;
                    } else {
                        _head = 0;
                    }
                    _itemCnt--;
                    return res;
                }
            }

            [PythonName("remove")]
            public void Remove(object value) {
                lock (_lockObj) {
                    int found = -1;
                    WalkDeque(delegate(int index) {
                        if (PythonOps.Equals(_data[index], value)) {
                            found = index;
                            return false;
                        }
                        return true;
                    });

                    if (found == _head) {
                        PopLeft();
                    } else if (found == _tail - 1) {
                        Pop();
                    } else if (found == -1) {
                        throw PythonOps.TypeError("deque.remove(value): value not in deque");
                    } else {
                        // otherwise we're removing from the middle and need to slide the values over...
                        _version++;
                        
                        int start;
                        if (_head >= _tail) {
                            start = 0;
                        } else {
                            start = _head;
                        }

                        bool finished = false;
                        object copying = _tail != 0 ? _data[_tail - 1] : _data[_data.Length - 1];
                        for (int i = _tail - 2; i >= start; i--) {
                            object tmp = _data[i];
                            _data[i] = copying;
                            if (i == found) {
                                finished = true;
                                break;
                            }
                            copying = tmp;
                        }
                        if (_head >= _tail && !finished) {
                            for (int i = _data.Length - 1; i >= _head; i--) {
                                object tmp = _data[i];
                                _data[i] = copying;
                                if (i == found) break;
                                copying = tmp;
                            }
                        }

                        // we're one smaller now
                        _tail--;
                        _itemCnt--;
                        if (_tail < 0) {
                            // and tail just wrapped to the beginning
                            _tail = _data.Length - 1;
                        }
                    }                    
                }
            }

            [PythonName("rotate")]
            public void Rotate() {
                Rotate(1);
            }

            [PythonName("rotate")]
            public void Rotate(object n) {
                lock (_lockObj) {
                    // rotation is easy if we have no items!
                    if (_itemCnt == 0) return;
                    int rot = Converter.ConvertToInt32(n) % _itemCnt;

                    // no need to rotate if we'll end back up where we started
                    if (rot != 0) {
                        _version++;

                        if (_itemCnt == _data.Length) {
                            // if all indexes are filled no moves are required
                            int newHead = (_head - rot) % _data.Length;
                            if (newHead < 0) newHead += _data.Length;
                            int newTail = (_tail - rot) % _data.Length;
                            if (newTail < 0) newTail += _data.Length;

                            _head = newHead;
                            _tail = newTail;
                        } else {
                            // too bad, we got gaps, looks like we'll be doing some real work.
                            object[] newData = new object[_itemCnt]; // we re-size to itemCnt so that future rotates don't require work
                            int curWriteIndex;
                            if (rot > 0) {
                                curWriteIndex = rot;
                            } else {
                                curWriteIndex = _itemCnt + rot;
                            }
                            WalkDeque(delegate(int curIndex) {
                                newData[curWriteIndex] = _data[curIndex];
                                curWriteIndex = (curWriteIndex + 1) % _itemCnt;
                                return true;
                            });
                            _data = newData;
                        }
                    }
                }
            }

            public object this[object index] {
                get {
                    lock (_lockObj) {
                        return _data[IndexToSlot(index)];
                    }
                }
                set {
                    lock (_lockObj) {
                        _version++;
                        _data[IndexToSlot(index)] = value;
                    }
                }
            }
            #endregion

            [PythonName("__copy__")]
            public object Copy() {
                if (GetType() == typeof(PythonDequeCollection)) {
                    PythonDequeCollection res = new PythonDequeCollection();
                    res.Extend(this.GetEnumerator());
                    return res;
                } else {
                    return PythonCalls.Call(DynamicHelpers.GetPythonType(this), GetEnumerator());
                }
            }

            [SpecialName, PythonName("__delitem__")]
            public void RemoveAt(object index) {
                lock (_lockObj) {
                    int realIndex = IndexToSlot(index);

                    _version++;
                    if (realIndex == _head) {
                        PopLeft();
                    } else if (realIndex == (_tail - 1) ||
                        (realIndex == (_data.Length - 1) && _tail == _data.Length)) {
                        Pop();
                    } else {
                        // we're removing an index from the middle, what a pain...
                        // we'll just recreate our data by walking the data once.
                        object[] newData = new object[_data.Length];
                        int writeIndex = 0;
                        WalkDeque(delegate(int curIndex) {
                            if (curIndex != realIndex) {
                                newData[writeIndex++] = _data[curIndex];
                            }

                            return true;
                        });

                        _head = 0;
                        _tail = writeIndex;
                        _data = newData;

                        _itemCnt--;
                    }
                }
            }

            [SpecialName, PythonName("__contains__")]
            public object Contains(object o) {
                lock (_lockObj) {
                    object res = RuntimeHelpers.False;
                    WalkDeque(delegate(int index) {
                        if (PythonOps.Equals(_data[index], o)) {
                            res = RuntimeHelpers.True;
                            return false;
                        }
                        return true;
                    });

                    return res;
                }
            }

            [PythonName("__reduce__")]
            public PythonTuple Reduce() {
                lock (_lockObj) {
                    object[] items = new object[_itemCnt];
                    int curItem = 0;
                    WalkDeque(delegate(int curIndex) {
                        items[curItem++] = _data[curIndex];
                        return true;
                    });

                    return PythonTuple.MakeTuple(
                        DynamicHelpers.GetPythonTypeFromType(GetType()),
                        PythonTuple.MakeTuple(new List(items)),
                        null
                    );
                }
            }

            [SpecialName, PythonName("__len__")]
            public int GetLength() {
                return _itemCnt;
            }

            #region Object Overrides

            public override string ToString() {
                return PythonOps.StringRepr(this);
            }
            #endregion

            #region IComparable Members

            public int CompareTo(object obj) {
                PythonDequeCollection otherDeque = obj as PythonDequeCollection;
                if (otherDeque == null) throw new ArgumentException("expected deque");

                return CompareToWorker(otherDeque);
            }

            private int CompareToWorker(PythonDequeCollection otherDeque) {
                if (otherDeque._itemCnt == 0 && _itemCnt == 0) {
                    // comparing two empty deques
                    return 0;
                }

                if (CompareUtil.Check(this)) return 0;

                CompareUtil.Push(this);
                try {
                    int otherIndex = otherDeque._head, ourIndex = _head;

                    for (; ; ) {
                        int result = PythonOps.Compare(_data[ourIndex], otherDeque._data[otherIndex]);
                        if (result != 0) {
                            return result;
                        }

                        // advance both indexes
                        otherIndex++;
                        if (otherIndex == otherDeque._data.Length) {
                            otherIndex = 0;
                        } else if (otherIndex == otherDeque._tail) {
                            break;
                        }

                        ourIndex++;
                        if (ourIndex == _data.Length) {
                            ourIndex = 0;
                        } else if (ourIndex == _tail) {
                            break;
                        }
                    }
                    // all items are equal, but # of items may be different.

                    if (otherDeque._itemCnt == _itemCnt) {
                        // same # of items, all items are equal
                        return 0;
                    }

                    return _itemCnt > otherDeque._itemCnt ? 1 : -1;
                } finally {
                    CompareUtil.Pop(this);
                }
            }
            #endregion

            #region IEnumerable Members
            [PythonName("__iter__")]
            public IEnumerator GetEnumerator() {
                return new PythonDequeEnumerator(this);
            }

            [PythonType("deque_iterator")]
            private class PythonDequeEnumerator : IEnumerator {
                PythonDequeCollection deque;
                int curIndex, moveCnt, version;

                public PythonDequeEnumerator(PythonDequeCollection d) {
                    lock (d._lockObj) {
                        deque = d;
                        curIndex = d._head - 1;
                        version = d._version;
                    }
                }

                #region IEnumerator Members

                public object Current {
                    get {
                        return this.deque._data[curIndex];
                    }
                }

                public bool MoveNext() {
                    lock (deque._lockObj) {
                        if (version != deque._version) throw PythonOps.RuntimeError("deque mutated during iteration");

                        if (moveCnt < deque._itemCnt) {
                            curIndex++;
                            moveCnt++;
                            if (curIndex == deque._data.Length) {
                                curIndex = 0;
                            }
                            return true;
                        }
                        return false;
                    }
                }

                public void Reset() {
                    moveCnt = 0;
                    curIndex = deque._head;
                }

                #endregion
            }

            #endregion

            #region private members
            private void GrowArray() {
                object[] newData = new object[_data.Length * 2];

                // make the array completely sequential again
                // by starting head back at 0.
                int cnt1, cnt2;
                if (_head >= _tail) {
                    cnt1 = _data.Length - _head;
                    cnt2 = _data.Length - cnt1;
                } else {
                    cnt1 = _tail - _head;
                    cnt2 = _data.Length - cnt1;
                }

                Array.Copy(_data, _head, newData, 0, cnt1);
                Array.Copy(_data, 0, newData, cnt1, cnt2);

                _head = 0;
                _tail = _data.Length;
                _data = newData;
            }


            private int IndexToSlot(object index) {
                if (_itemCnt == 0) throw PythonOps.IndexError("deque index out of range");

                int intIndex = Converter.ConvertToInt32(index);
                if (intIndex >= 0) {
                    if (intIndex >= _itemCnt) throw PythonOps.IndexError("deque index out of range");

                    int realIndex = _head + intIndex;
                    if (realIndex >= _data.Length) {
                        realIndex -= _data.Length;
                    }

                    return realIndex;
                } else {
                    if ((intIndex * -1) > _itemCnt) throw PythonOps.IndexError("deque index out of range");

                    int realIndex = _tail + intIndex;
                    if (realIndex < 0) {
                        realIndex += _data.Length;
                    }

                    return realIndex;
                }
            }

            private delegate bool DequeWalker(int curIndex);

            /// <summary>
            /// Walks the queue calling back to the specified delegate for
            /// each populated index in the queue.
            /// </summary>
            private void WalkDeque(DequeWalker walker) {
                if (_itemCnt != 0) {
                    int end;
                    if (_head >= _tail) {
                        end = _data.Length;
                    } else {
                        end = _tail;
                    }

                    for (int i = _head; i < end; i++) {
                        if (!walker(i)) {
                            return;
                        }
                    }
                    if (_head >= _tail) {
                        for (int i = 0; i < _tail; i++) {
                            if (!walker(i)) {
                                return;
                            }
                        }
                    }
                }
            }
            #endregion

            #region ICodeFormattable Members

            [SpecialName, PythonName("__repr__")]
            public string ToCodeString(CodeContext context) {
                StringBuilder sb = new StringBuilder();
                sb.Append("deque([");
                string comma = "";

                lock (_lockObj) {
                    WalkDeque(delegate(int index) {
                        sb.Append(comma);
                        sb.Append(PythonOps.StringRepr(_data[index]));
                        comma = ", ";
                        return true;
                    });
                }

                sb.Append("])");

                return sb.ToString();
            }

            #endregion

            #region IValueEquality Members

            public int GetValueHashCode() {
                throw PythonOps.TypeError("deque objects are unhashable");
            }


            public bool ValueEquals(object other) {
                if (!(other is PythonDequeCollection)) return false;

                return CompareTo(other) == 0;
            }

            public bool ValueNotEquals(object other) {
                if (!(other is PythonDequeCollection)) return true;

                return CompareTo(other) != 0;
            }

            #endregion

            #region Rich Comparison Members

            [SpecialName]
            [return: MaybeNotImplemented]
            public object GreaterThan(CodeContext context, object other) {
                PythonDequeCollection otherDeque = other as PythonDequeCollection;
                if (otherDeque == null) return PythonOps.NotImplemented;

                return RuntimeHelpers.BooleanToObject(CompareToWorker(otherDeque) > 0);
            }

            [SpecialName]
            [return: MaybeNotImplemented]
            public object LessThan(CodeContext context, object other) {
                PythonDequeCollection otherDeque = other as PythonDequeCollection;
                if (otherDeque == null) return PythonOps.NotImplemented;

                return RuntimeHelpers.BooleanToObject(CompareToWorker(otherDeque) < 0);
            }

            [SpecialName]
            [return: MaybeNotImplemented]
            public object GreaterThanOrEqual(CodeContext context, object other) {
                PythonDequeCollection otherDeque = other as PythonDequeCollection;
                if (otherDeque == null) return PythonOps.NotImplemented;

                return RuntimeHelpers.BooleanToObject(CompareToWorker(otherDeque) >= 0);
            }

            [SpecialName]
            [return: MaybeNotImplemented]
            public object LessThanOrEqual(CodeContext context, object other) {
                PythonDequeCollection otherDeque = other as PythonDequeCollection;
                if (otherDeque == null) return PythonOps.NotImplemented;

                return RuntimeHelpers.BooleanToObject(CompareToWorker(otherDeque) <= 0);
            }

            #endregion

            #region ICollection Members

            void ICollection.CopyTo(Array array, int index) {
                throw new NotImplementedException();
            }

            int ICollection.Count {
                get { return this._itemCnt;  }
            }

            bool ICollection.IsSynchronized {
                get { return false; }
            }

            object ICollection.SyncRoot {
                get { return this; }
            }

            #endregion           
        }

    }
}
