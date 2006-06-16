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
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Text;

using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

[assembly: PythonModule("collections", typeof(IronPython.Modules.PythonCollections))]
namespace IronPython.Modules {
    public class PythonCollections {
        public static object deque = Ops.GetDynamicTypeFromType(typeof(PythonDequeCollection));
        public static string __doc__ = "High performance data structures\n";

        [PythonType("deque")]
        public class PythonDequeCollection : IEnumerable, IComparable, ICodeFormattable, IRichEquality {
            object[] data;
            object lockObj = new object();
            int head, tail;
            int itemCnt, version;

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
                lock (lockObj) {
                    version++;

                    if (itemCnt == data.Length) {
                        GrowArray();
                    }

                    itemCnt++;
                    data[tail++] = x;
                    if (tail == data.Length) {
                        tail = 0;
                    }
                }
            }

            [PythonName("appendleft")]
            public void AppendLeft(object x) {
                lock (lockObj) {
                    version++;

                    if (itemCnt == data.Length) {
                        GrowArray();
                    }

                    itemCnt++;
                    --head;
                    if (head < 0) {
                        head = data.Length - 1;
                    }

                    data[head] = x;
                }
            }

            [PythonName("clear")]
            public void Clear() {
                lock (lockObj) {
                    version++;

                    head = tail = 0;
                    itemCnt = 0;
                    data = new object[8];
                }
            }

            [PythonName("extend")]
            public void Extend(object iterable) {
                IEnumerator e = Ops.GetEnumerator(iterable);
                while (e.MoveNext()) {
                    Append(e.Current);
                }
            }

            [PythonName("extendleft")]
            public void ExtendLeft(object iterable) {
                IEnumerator e = Ops.GetEnumerator(iterable);
                while (e.MoveNext()) {
                    AppendLeft(e.Current);
                }
            }

            [PythonName("pop")]
            public object Pop() {
                lock (lockObj) {
                    if (itemCnt == 0) throw Ops.IndexError("pop from an empty deque");

                    version++;
                    if (tail != 0) {
                        tail--;
                    } else {
                        tail = data.Length - 1;
                    }
                    itemCnt--;

                    object res = data[tail];
                    data[tail] = null;
                    return res;
                }
            }

            [PythonName("popleft")]
            public object PopLeft() {
                lock (lockObj) {
                    if (itemCnt == 0) throw Ops.IndexError("pop from an empty deque");

                    version++;
                    object res = data[head];
                    data[head] = null;

                    if (head != data.Length - 1) {
                        head++;
                    } else {
                        head = 0;
                    }
                    itemCnt--;
                    return res;
                }
            }

            [PythonName("rotate")]
            public void Rotate() {
                Rotate(1);
            }

            [PythonName("rotate")]
            public void Rotate(object n) {
                lock (lockObj) {
                    // rotation is easy if we have no items!
                    if (itemCnt == 0) return;
                    int rot = Converter.ConvertToInt32(n) % itemCnt;

                    // no need to rotate if we'll end back up where we started
                    if (rot != 0) {
                        version++;

                        if (itemCnt == data.Length) {
                            // if all indexes are filled no moves are required
                            int newHead = (head - rot) % data.Length;
                            if (newHead < 0) newHead += data.Length;
                            int newTail = (tail - rot) % data.Length;
                            if (newTail < 0) newTail += data.Length;

                            head = newHead;
                            tail = newTail;
                        } else {
                            // too bad, we got gaps, looks like we'll be doing some real work.
                            object[] newData = new object[itemCnt]; // we re-size to itemCnt so that future rotates don't require work
                            int curWriteIndex;
                            if (rot > 0) {
                                curWriteIndex = rot;
                            } else {
                                curWriteIndex = itemCnt + rot;
                            }
                            WalkDeque(delegate(int curIndex) {
                                newData[curWriteIndex] = data[curIndex];
                                curWriteIndex = (curWriteIndex + 1) % itemCnt;
                                return true;
                            });
                            data = newData;
                        }
                    }
                }
            }

            public object this[object index] {
                get {
                    lock (lockObj) {
                        return data[IndexToSlot(index)];
                    }
                }
                set {
                    lock (lockObj) {
                        version++;
                        data[IndexToSlot(index)] = value;
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
                    UserType ut = Ops.GetDynamicType(this) as UserType;
                    System.Diagnostics.Debug.Assert(ut != null);

                    return ut.Call(new object[] { GetEnumerator() });
                }
            }

            [PythonName("__delitem__")]
            public void RemoveAt(object index) {
                lock (lockObj) {
                    int realIndex = IndexToSlot(index);

                    version++;
                    if (realIndex == head) {
                        PopLeft();
                    } else if (realIndex == (tail - 1) ||
                        (realIndex == (data.Length - 1) && tail == data.Length)) {
                        Pop();
                    } else {
                        // we're removing an index from the middle, what a pain...
                        // we'll just recreate our data by walking the data once.
                        object[] newData = new object[data.Length];
                        int writeIndex = 0;
                        WalkDeque(delegate(int curIndex) {
                            if (curIndex != realIndex) {
                                newData[writeIndex++] = data[curIndex];
                            }

                            return true;
                        });

                        head = 0;
                        tail = writeIndex;
                        data = newData;

                        itemCnt--;
                    }
                }
            }

            [PythonName("__contains__")]
            public object Contains(object o) {
                lock (lockObj) {
                    object res = Ops.FALSE;
                    WalkDeque(delegate(int index) {
                        if (Ops.Equals(data[index], o)) {
                            res = Ops.TRUE;
                            return false;
                        }
                        return true;
                    });

                    return res;
                }
            }

            [PythonName("__reduce__")]
            public Tuple Reduce() {
                lock (lockObj) {
                    object[] items = new object[itemCnt];
                    int curItem = 0;
                    WalkDeque(delegate(int curIndex) {
                        items[curItem++] = data[curIndex];
                        return true;
                    });

                    return Tuple.MakeTuple(new object[] { 
                    Ops.GetDynamicTypeFromType(GetType()), 
                    Tuple.MakeTuple(new List(items)), 
                    null });
                }
            }

            [PythonName("__reduce_ex__")]
            public Tuple ReduceExt(object proto) {
                return Reduce();
            }

            [PythonName("__len__")]
            public int GetLength() {
                return itemCnt;
            }

            #region Object Overrides

            [PythonName("__str__")]
            public override string ToString() {
                return Ops.StringRepr(this);
            }
            #endregion

            #region IComparable Members

            public int CompareTo(object obj) {
                PythonDequeCollection otherDeque = obj as PythonDequeCollection;
                if (otherDeque == null) throw new ArgumentException("expected deque");

                if (otherDeque.itemCnt == 0 && itemCnt == 0) {
                    // comparing two empty deques
                    return 0;
                }

                ArrayList infinite = Ops.GetCmpInfinite();
                if (infinite.Contains(this)) return 0;

                int index = infinite.Add(this);
                try {
                    int otherIndex = otherDeque.head, ourIndex = head;

                    for (; ; ) {
                        int result = Ops.Compare(data[ourIndex], otherDeque.data[otherIndex]);
                        if (result != 0) {
                            return result;
                        }

                        // advance both indexes
                        otherIndex++;
                        if (otherIndex == otherDeque.data.Length) {
                            otherIndex = 0;
                        } else if (otherIndex == otherDeque.tail) {
                            break;
                        }

                        ourIndex++;
                        if (ourIndex == data.Length) {
                            ourIndex = 0;
                        } else if (ourIndex == tail) {
                            break;
                        }
                    }
                    // all items are equal, but # of items may be different.

                    if (otherDeque.itemCnt == itemCnt) {
                        // same # of items, all items are equal
                        return 0;
                    }

                    return itemCnt > otherDeque.itemCnt ? 1 : -1;
                } finally {
                    System.Diagnostics.Debug.Assert(infinite.Count == index + 1);
                    infinite.RemoveAt(index);
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
                    lock (d.lockObj) {
                        deque = d;
                        curIndex = d.head - 1;
                        version = d.version;
                    }
                }

                #region IEnumerator Members

                public object Current {
                    get {
                        return this.deque.data[curIndex];
                    }
                }

                public bool MoveNext() {
                    lock (deque.lockObj) {
                        if (version != deque.version) throw Ops.RuntimeError("deque mutated during iteration");

                        if (moveCnt < deque.itemCnt) {
                            curIndex++;
                            moveCnt++;
                            if (curIndex == deque.data.Length) {
                                curIndex = 0;
                            }
                            return true;
                        }
                        return false;
                    }
                }

                public void Reset() {
                    moveCnt = 0;
                    curIndex = deque.head;
                }

                #endregion
            }

            #endregion

            #region private members
            private void GrowArray() {
                object[] newData = new object[data.Length * 2];

                // make the array completely sequential again
                // by starting head back at 0.
                int cnt1, cnt2;
                if (head >= tail) {
                    cnt1 = data.Length - head;
                    cnt2 = data.Length - cnt1;
                } else {
                    cnt1 = tail - head;
                    cnt2 = data.Length - cnt1;
                }

                Array.Copy(data, head, newData, 0, cnt1);
                Array.Copy(data, 0, newData, cnt1, cnt2);

                head = 0;
                tail = data.Length;
                data = newData;
            }


            private int IndexToSlot(object index) {
                if (itemCnt == 0) throw Ops.IndexError("deque index out of range");

                int intIndex = Converter.ConvertToInt32(index);
                if (intIndex >= 0) {
                    if (intIndex >= itemCnt) throw Ops.IndexError("deque index out of range");

                    int realIndex = head + intIndex;
                    if (realIndex >= data.Length) {
                        realIndex -= data.Length;
                    }

                    return realIndex;
                } else {
                    if ((intIndex * -1) > itemCnt) throw Ops.IndexError("deque index out of range");

                    int realIndex = tail + intIndex;
                    if (realIndex < 0) {
                        realIndex += data.Length;
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
                if (itemCnt != 0) {
                    int end;
                    if (head > tail) {
                        end = data.Length;
                    } else if (head == tail) {
                        end = data.Length;
                    } else {
                        end = tail;
                    }

                    for (int i = head; i < end; i++) {
                        if (!walker(i)) {
                            return;
                        }
                    }
                    if (head >= tail) {
                        for (int i = 0; i < tail; i++) {
                            if (!walker(i)) {
                                return;
                            }
                        }
                    }
                }
            }
            #endregion

            #region ICodeFormattable Members

            [PythonName("__repr__")]
            public string ToCodeString() {
                StringBuilder sb = new StringBuilder();
                sb.Append("deque([");
                string comma = "";

                lock (lockObj) {
                    WalkDeque(delegate(int index) {
                        sb.Append(comma);
                        sb.Append(Ops.StringRepr(data[index]));
                        comma = ", ";
                        return true;
                    });
                }

                sb.Append("])");

                return sb.ToString();
            }

            #endregion

            #region IRichEquality Members
            [PythonName("__hash__")]
            public object RichGetHashCode() {
                throw Ops.TypeError("deque objects are unhashable");
            }

            [PythonName("__eq__")]
            public object RichEquals(object other) {
                if (!(other is PythonDequeCollection)) return Ops.FALSE;

                return Ops.Bool2Object(CompareTo(other) == 0);
            }

            public object RichNotEquals(object other) {
                if (!(other is PythonDequeCollection)) return Ops.TRUE;

                return Ops.Bool2Object(CompareTo(other) != 0);
            }

            #endregion
        }

    }
}
