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
using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;

[assembly: PythonModule("collections", typeof(IronPython.Modules.PythonCollections))]
namespace IronPython.Modules {
    [Documentation("High performance data structures\n")]
    public class PythonCollections {
        [PythonSystemType]
        public class deque : IEnumerable, IComparable, ICodeFormattable, IValueEquality, ICollection {
            private object[] _data;
            private object _lockObj = new object();
            private int _head, _tail;
            private int _itemCnt, _version;

            public deque() {
                clear();
            }

            public void __init__() {
            }

            public void __init__(object iterable) {
                extend(iterable);
            }

            #region core deque APIs

            public void append(object x) {
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

            public void appendleft(object x) {
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

            public void clear() {
                lock (_lockObj) {
                    _version++;

                    _head = _tail = 0;
                    _itemCnt = 0;
                    _data = new object[8];
                }
            }

            public void extend(object iterable) {
                IEnumerator e = PythonOps.GetEnumerator(iterable);
                while (e.MoveNext()) {
                    append(e.Current);
                }
            }

            public void extendleft(object iterable) {
                IEnumerator e = PythonOps.GetEnumerator(iterable);
                while (e.MoveNext()) {
                    appendleft(e.Current);
                }
            }

            public object pop() {
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

            public object popleft() {
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

            public void remove(object value) {
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
                        popleft();
                    } else if (found == _tail - 1) {
                        pop();
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

            public void rotate() {
                rotate(1);
            }

            public void rotate(object n) {
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

            public object __copy__() {
                if (GetType() == typeof(deque)) {
                    deque res = new deque();
                    res.extend(((IEnumerable)this).GetEnumerator());
                    return res;
                } else {
                    return PythonCalls.Call(DynamicHelpers.GetPythonType(this), ((IEnumerable)this).GetEnumerator());
                }
            }

            public void __delitem__(object index) {
                lock (_lockObj) {
                    int realIndex = IndexToSlot(index);

                    _version++;
                    if (realIndex == _head) {
                        popleft();
                    } else if (realIndex == (_tail - 1) ||
                        (realIndex == (_data.Length - 1) && _tail == _data.Length)) {
                        pop();
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

            public object __contains__(object o) {
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

            public PythonTuple __reduce__() {
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

            public int __len__() {
                return _itemCnt;
            }

            #region Object Overrides

            public override string ToString() {
                return PythonOps.StringRepr(this);
            }

            #endregion

            #region IComparable Members

            int IComparable.CompareTo(object obj) {
                deque otherDeque = obj as deque;
                if (otherDeque == null) throw new ArgumentException("expected deque");

                return CompareToWorker(otherDeque);
            }

            private int CompareToWorker(deque otherDeque) {
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

            IEnumerator IEnumerable.GetEnumerator() {
                return new deque_iterator(this);
            }

            [PythonSystemType]
            private class deque_iterator : IEnumerator {
                deque deque;
                int curIndex, moveCnt, version;

                public deque_iterator(deque d) {
                    lock (d._lockObj) {
                        deque = d;
                        curIndex = d._head - 1;
                        version = d._version;
                    }
                }

                #region IEnumerator Members

                object IEnumerator.Current {
                    get {
                        return this.deque._data[curIndex];
                    }
                }

                bool IEnumerator.MoveNext() {
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

                void IEnumerator.Reset() {
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

            public virtual string/*!*/ __repr__(CodeContext/*!*/ context) {
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

            int IValueEquality.GetValueHashCode() {
                throw PythonOps.TypeError("deque objects are unhashable");
            }


            bool IValueEquality.ValueEquals(object other) {
                if (!(other is deque)) return false;

                return ((IComparable)this).CompareTo(other) == 0;
            }

            #endregion

            #region Rich Comparison Members

            [SpecialName]
            [return: MaybeNotImplemented]
            public static object operator >(deque self, object other) {
                deque otherDeque = other as deque;
                if (otherDeque == null) return PythonOps.NotImplemented;

                return RuntimeHelpers.BooleanToObject(self.CompareToWorker(otherDeque) > 0);
            }

            [SpecialName]
            [return: MaybeNotImplemented]
            public static object operator <(deque self, object other) {
                deque otherDeque = other as deque;
                if (otherDeque == null) return PythonOps.NotImplemented;

                return RuntimeHelpers.BooleanToObject(self.CompareToWorker(otherDeque) < 0);
            }

            [SpecialName]
            [return: MaybeNotImplemented]
            public static object operator >=(deque self, object other) {
                deque otherDeque = other as deque;
                if (otherDeque == null) return PythonOps.NotImplemented;

                return RuntimeHelpers.BooleanToObject(self.CompareToWorker(otherDeque) >= 0);
            }

            [SpecialName]
            [return: MaybeNotImplemented]
            public static object operator <=(deque self, object other) {
                deque otherDeque = other as deque;
                if (otherDeque == null) return PythonOps.NotImplemented;

                return RuntimeHelpers.BooleanToObject(self.CompareToWorker(otherDeque) <= 0);
            }

            #endregion

            #region ICollection Members

            void ICollection.CopyTo(Array array, int index) {
                throw new NotImplementedException();
            }

            int ICollection.Count {
                get { return this._itemCnt; }
            }

            bool ICollection.IsSynchronized {
                get { return false; }
            }

            object ICollection.SyncRoot {
                get { return this; }
            }

            #endregion
        }

        [PythonSystemType]
        public class defaultdict : PythonDictionary {
            private object _factory;
            private DynamicSite<object, object> _missingSite;

            public void __init__(object default_factory) {
                _factory = default_factory;
            }

            public void __init__(CodeContext/*!*/ context, object default_factory, params object[] args) {
                _factory = default_factory;
                foreach (object o in args) {
                    update(context, o);
                }
            }

            public void __init__(CodeContext/*!*/ context, object default_factory, [ParamDictionary]IAttributesCollection dict, params object[] args) {
                __init__(context, default_factory, args);

                foreach (KeyValuePair<SymbolId, object> kvp in dict.SymbolAttributes) {
                    this[SymbolTable.IdToString(kvp.Key)] = kvp.Value;
                }
            }

            public object default_factory {
                get {
                    return _factory;
                }
                set {
                    _factory = value;
                }
            }

            public object __missing__(CodeContext context, object key) {
                object factory = _factory;

                if (factory == null) throw PythonOps.KeyError(key);

                if (!_missingSite.IsInitialized) {
                    _missingSite.EnsureInitialized(CallAction.Make(DefaultContext.DefaultPythonBinder, 0));
                }

                // get the default value, store it in the dictionary and return it
                return this[key] = _missingSite.Invoke(context, factory);
            }

            public object __copy__(CodeContext/*!*/ context) {
                return copy(context);
            }

            public override PythonDictionary copy(CodeContext/*!*/ context) {
                defaultdict res = new defaultdict();
                res.default_factory = this.default_factory;
                res.update(context, this);
                return res;
            }


            public override string __repr__(CodeContext context) {
                return String.Format("defaultdict({0}, {1})", PythonOps.Repr(default_factory), base.__repr__(context));
            }

            public PythonTuple __reduce__() {
                return PythonTuple.MakeTuple(
                    DynamicHelpers.GetPythonType(this),
                    PythonTuple.MakeTuple(default_factory),
                    null,
                    null,
                    iteritems()
                );
            }
        }
    }
}
