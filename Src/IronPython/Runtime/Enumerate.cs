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

namespace IronPython.Runtime {
    [PythonType("enumerate")]
    public class Enumerate : IEnumerator, IDynamicObject {
        private readonly IEnumerator iter;
        private int index = 0;
        public Enumerate(object iter) {
            this.iter = Ops.GetEnumerator(iter);
        }

        public static string Documentation {
            [PythonName("__doc__")]
            get {
                return "enumerate(iterable) -> iterator for index, value of iterable";
            }
        }

        #region IEnumerator Members

        public void Reset() {
            throw new NotImplementedException();
        }

        public object Current {
            get {
                return Tuple.MakeTuple(index++, iter.Current);
            }
        }

        public bool MoveNext() {
            return iter.MoveNext();
        }

        #endregion

        #region IDynamicObject Members

        public DynamicType GetDynamicType() {
            return TypeCache.Enumerate;
        }

        #endregion
    }

    public class PythonEnumerator : IEnumerator, IEnumerable {
        private readonly object baseObject;
        private object nextMethod;
        private object current = null;

        public static bool Create(object baseEnumerator, out IEnumerator enumerator) {
            object iter;

            if (Ops.TryGetAttr(baseEnumerator, SymbolTable.Iterator, out iter)) {
                object iterator = Ops.Call(iter);
                enumerator = new PythonEnumerator(iterator);
                return true;
            } else {
                enumerator = null;
                return false;
            }
        }

        public PythonEnumerator(object iter) {
            this.baseObject = iter;
        }


        #region IEnumerator Members

        public void Reset() {
            throw new NotImplementedException();
        }

        public object Current {
            get {
                return current;
            }
        }

        public bool MoveNext() {
            if (nextMethod == null) {
                if (!Ops.TryGetAttr(baseObject, SymbolTable.GeneratorNext, out nextMethod) || nextMethod == null) {
                    throw Ops.TypeError("instance has no next() method");
                }
            }

            try {
                current = Ops.Call(nextMethod);
                return true;
            } catch (StopIterationException) {
                return false;
            }
        }

        #endregion

        #region IEnumerable Members

        [PythonName("__iter__")]
        public IEnumerator GetEnumerator() {
            return this;
        }

        #endregion
    }

    public class ItemEnumerator : IEnumerator {
        private readonly object getItemMethod;
        private object current = null;
        private int index = 0;

        public static bool Create(object baseObject, out IEnumerator enumerator) {
            object getitem;

            if (Ops.TryGetAttr(baseObject, SymbolTable.GetItem, out getitem)) {
                enumerator = new ItemEnumerator(getitem);
                return true;
            } else {
                enumerator = null;
                return false;
            }
        }

        public ItemEnumerator(object getItemMethod) {
            this.getItemMethod = getItemMethod;
        }

        #region IEnumerator members

        public object Current {
            get {
                return current;
            }
        }

        public bool MoveNext() {
            if (index < 0) {
                return false;
            }

            try {
                current = Ops.Call(getItemMethod, index);
                index++;
                return true;
            } catch (IndexOutOfRangeException) {
                current = null;
                index = -1;     // this is the end
                return false;
            } catch (StopIterationException) {
                current = null;
                index = -1;     // this is the end
                return false;
            }
        }

        public void Reset() {
            index = 0;
            current = null;
        }

        #endregion

    }

    [PythonType("ReversedEnumerator")]
    public class ReversedEnumerator : IEnumerator {
        private readonly object getItemMethod;
        private object current;
        private int index;
        private int savedIndex;

        public ReversedEnumerator(int length, object getitem) {
            this.index = this.savedIndex = length;
            this.getItemMethod = getitem;
        }

        [PythonName("__len__")]
        public int __len__() { return index; }

        #region IEnumerator implementation

        public object Current {
            get {
                return current;
            }
        }

        public bool MoveNext() {
            if (index > 0) {
                index--;
                current = Ops.Call(getItemMethod, index);
                return true;
            } else return false;
        }

        public void Reset() {
            index = savedIndex;
        }

        #endregion
    }

    [PythonType("SentinelIterator")]
    public class SentinelIterator : IEnumerator {
        private readonly object target;
        private readonly object sentinel;
        private object current;
        private bool sinkState;

        public SentinelIterator(object target, object sentinel) {
            this.target = target;
            this.sentinel = sentinel;
            this.current = null;
            this.sinkState = false;
        }

        [PythonName("__iter__")]
        public object __iter__() {
            return this;
        }

        [PythonName("__next__")]
        public object next() {
            if (MoveNext()) {
                return Current;
            } else {
                throw Ops.StopIteration();
            }
        }

        #region IEnumerator implementation

        public object Current {
            get {
                return current;
            }
        }

        public bool MoveNext() {
            if (sinkState) return false;

            current = Ops.Call(target);

            bool hit = Ops.EqualRetBool(sentinel, current);
            if (hit) sinkState = true;

            return !hit;
        }

        public void Reset() {
            throw new NotImplementedException();
        }

        #endregion
    }
}