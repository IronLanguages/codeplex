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
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using Microsoft.Scripting;

using IronPython.Runtime.Types;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime {
    /* 
     * Enumeraters exposed to Python code directly
     * 
     */

    [PythonType("enumerate")]
    public class Enumerate : IEnumerator, IEnumerator<object> {
        private readonly IEnumerator iter;
        private int index = 0;
        public Enumerate(object iter) {
            this.iter = PythonOps.GetEnumerator(iter);
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

        #region IDisposable Members

        public void Dispose() {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool notFinalizing) {
        }

        #endregion
    }

    [PythonType("ReversedEnumerator")]
    public class ReversedEnumerator : IEnumerator, IEnumerator<object> {
        private readonly object _getItemMethod;
        private object _current;
        private int _index;
        private int _savedIndex;

        public ReversedEnumerator(int length, object getitem) {
            this._index = this._savedIndex = length;
            this._getItemMethod = getitem;
        }

        [SpecialName, PythonName("__len__")]
        public int Length() { return _index; }

        #region IEnumerator implementation

        public object Current {
            get {
                return _current;
            }
        }

        public bool MoveNext() {
            if (_index > 0) {
                _index--;
                _current = PythonCalls.Call(_getItemMethod, _index);
                return true;
            } else return false;
        }

        public void Reset() {
            _index = _savedIndex;
        }

        #endregion

        #region IDisposable Members

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
        }

        #endregion
    }

    [PythonType("SentinelIterator")]
    public sealed class SentinelIterator : IEnumerator, IEnumerator<object> {
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
        public object GetIterator() {
            return this;
        }

        [PythonName("next")]
        public object Next() {
            if (MoveNext()) {
                return Current;
            } else {
                throw PythonOps.StopIteration();
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

            current = PythonCalls.Call(target);

            bool hit = PythonOps.EqualRetBool(sentinel, current);
            if (hit) sinkState = true;

            return !hit;
        }

        public void Reset() {
            throw new NotImplementedException();
        }

        #endregion

        #region IDisposable Members

        public void Dispose() {
        }

        #endregion
    }

    /* 
     * Enumeraters exposed to .NET code
     * 
     */
    [PythonType("enumerator")]
    public class PythonEnumerator : IEnumerator {
        private readonly object _baseObject;
        private object _nextMethod;
        private object _current;

        public static bool TryCreate(object baseEnumerator, out IEnumerator enumerator) {
            object iter;

            if (PythonOps.TryGetBoundAttr(baseEnumerator, Symbols.Iterator, out iter)) {
                object iterator = PythonCalls.Call(iter);
                // don't re-wrap if we don't need to (common case is PythonGenerator).
                IEnumerable enumerale = iterator as IEnumerable;
                if (enumerale != null) {
                    enumerator = enumerale.GetEnumerator();
                } else {
                    enumerator = new PythonEnumerator(iterator);
                }
                return true;
            } else {
                enumerator = null;
                return false;
            }
        }

        public PythonEnumerator(object iter) {
            Debug.Assert(!(iter is PythonGenerator));

            this._baseObject = iter;
        }


        #region IEnumerator Members

        public void Reset() {
            throw new NotImplementedException();
        }

        public object Current {
            get {
                return _current;
            }
        }

        public bool MoveNext() {
            if (_nextMethod == null) {
                if (!PythonOps.TryGetBoundAttr(_baseObject, Symbols.GeneratorNext, out _nextMethod) || _nextMethod == null) {
                    throw PythonOps.TypeError("instance has no next() method");
                }
            }

            try {
                _current = PythonCalls.Call(_nextMethod);
                return true;
            } catch (StopIterationException) {
                return false;
            }
        }

        #endregion

        [PythonName("__iter__")]
        public object GetEnumerator() {
            return this;
        }
    }

    [PythonType("enumerable")]
    public class PythonEnumerable : IEnumerable {
        object iterator;

        public static bool TryCreate(object baseEnumerator, out PythonEnumerable enumerator) {
            object iter;

            if (PythonOps.TryGetBoundAttr(baseEnumerator, Symbols.Iterator, out iter)) {
                object iterator = PythonCalls.Call(iter);
                enumerator = new PythonEnumerable(iterator);
                return true;
            } else {
                enumerator = null;
                return false;
            }
        }

        private PythonEnumerable(object iterator) {
            this.iterator = iterator;
        }

        #region IEnumerable Members

        [PythonName("__iter__")]
        public IEnumerator GetEnumerator() {
            return new PythonEnumerator(iterator);
        }

        #endregion
    }

    [PythonType("item-enumerator")]
    internal class ItemEnumerator : IEnumerator {
        private readonly object getItemMethod;
        private object current = null;
        private int index = 0;

        public static bool TryCreate(object baseObject, out IEnumerator enumerator) {
            object getitem;

            if (PythonOps.TryGetBoundAttr(baseObject, Symbols.GetItem, out getitem)) {
                enumerator = new ItemEnumerator(getitem);
                return true;
            } else {
                enumerator = null;
                return false;
            }
        }

        internal ItemEnumerator(object getItemMethod) {
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
                current = PythonCalls.Call(getItemMethod, index);
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

    [PythonType("item-enumerable")]
    public class ItemEnumerable : IEnumerable {
        object getitem;

        public static bool TryCreate(object baseObject, out ItemEnumerable ie) {
            object getitem;

            if (PythonOps.TryGetBoundAttr(baseObject, Symbols.GetItem, out getitem)) {
                ie = new ItemEnumerable(getitem);
                return true;
            } else {
                ie = null;
                return false;
            }
        }

        private ItemEnumerable(object getitem) {
            this.getitem = getitem;
        }


        #region IEnumerable Members

        [PythonName("__iter__")]
        public IEnumerator GetEnumerator() {
            return new ItemEnumerator(getitem);
        }

        #endregion
    }

}
