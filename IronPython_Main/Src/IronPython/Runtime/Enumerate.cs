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
using System.Diagnostics;
using System.Runtime.CompilerServices;

using Microsoft.Scripting.Runtime;

using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

namespace IronPython.Runtime {
    /* 
     * Enumeraters exposed to Python code directly
     * 
     */

    [PythonType("enumerate")]
    [Documentation("enumerate(iterable) -> iterator for index, value of iterable")]
    [DontMapIDisposableToContextManagerAttribute, DontMapIEnumerableToContains]
    public class Enumerate : IEnumerator, IEnumerator<object> {
        private readonly IEnumerator _iter;
        private int _index = -1;

        public Enumerate(object iter) {
            _iter = PythonOps.GetEnumerator(iter);
        }

        public object __iter__() {
            return this;
        }

        #region IEnumerator Members

        void IEnumerator.Reset() {
            throw new NotImplementedException();
        }

        object IEnumerator.Current {
            get {
                return PythonTuple.MakeTuple(_index, _iter.Current);
            }
        }

        object IEnumerator<object>.Current {
            get {
                return ((IEnumerator)this).Current;
            }
        }

        bool IEnumerator.MoveNext() {
            _index++;
            return _iter.MoveNext();
        }

        #endregion

        #region IDisposable Members

        void IDisposable.Dispose() {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        [PythonHidden]
        protected virtual void Dispose(bool notFinalizing) {
        }

        #endregion
    }

    [PythonType("SentinelIterator")]
    public sealed class SentinelIterator : IEnumerator, IEnumerator<object> {
        private readonly object _target;
        private readonly object _sentinel;
        private readonly CodeContext/*!*/ _context;
        private object _current;
        private bool _sinkState;

        public SentinelIterator(CodeContext/*!*/ context, object target, object sentinel) {
            _target = target;
            _sentinel = sentinel;
            _context = context;
        }

        public object __iter__() {
            return this;
        }

        public object next() {
            if (((IEnumerator)this).MoveNext()) {
                return ((IEnumerator)this).Current;
            } else {
                throw PythonOps.StopIteration();
            }
        }

        #region IEnumerator implementation

        object IEnumerator.Current {
            get {
                return _current;
            }   
        }

        object IEnumerator<object>.Current {
            get {
                return _current;
            }
        }

        bool IEnumerator.MoveNext() {
            if (_sinkState) return false;

            _current = PythonCalls.Call(_target);

            bool hit = PythonOps.EqualRetBool(_context, _sentinel, _current);
            if (hit) _sinkState = true;

            return !hit;
        }

        void IEnumerator.Reset() {
            throw new NotImplementedException();
        }

        #endregion

        #region IDisposable Members

        void IDisposable.Dispose() {
        }

        #endregion
    }

    /* 
     * Enumerators exposed to .NET code
     */

    [PythonType("enumerator")]
    public class PythonEnumerator : IEnumerator {
        private readonly object _baseObject;
        private object _nextMethod;
        private object _current;

        public static bool TryCastIEnumer(object baseObject, out IEnumerator enumerator) {
            if (baseObject is IEnumerator) {
                enumerator = (IEnumerator)baseObject;
                return true;
            }

            if (baseObject is IEnumerable) {
                enumerator = ((IEnumerable)baseObject).GetEnumerator();
                return true;
            }

            enumerator = null;
            return false;
        }

        public static bool TryCreate(object baseObject, out IEnumerator enumerator) {
            if (TryCastIEnumer(baseObject, out enumerator)) {
                return true;
            }

            object iter;
            if (PythonOps.TryGetBoundAttr(baseObject, Symbols.Iterator, out iter)) {
                object iterator = PythonCalls.Call(iter);
                // don't re-wrap if we don't need to (common case is PythonGenerator).
                if (TryCastIEnumer(iterator, out enumerator)) {
                    return true;
                }
                enumerator = new PythonEnumerator(iterator);
                return true;
            } else {
                enumerator = null;
                return false;
            }
        }

        public static IEnumerator Create(object baseObject) {
            IEnumerator res;
            if (!TryCreate(baseObject, out res)) {
                throw PythonOps.TypeError("cannot convert {0} to IEnumerator", PythonTypeOps.GetName(baseObject));
            }
            return res;
        }

        internal PythonEnumerator(object iter) {
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

        public object __iter__() {
            return this;
        }
    }

    [PythonType("enumerable")]
    public class PythonEnumerable : IEnumerable {
        private object _iterator;

        public static bool TryCreate(object baseEnumerator, out IEnumerable enumerator) {
            Debug.Assert(!(baseEnumerator is IEnumerable) || baseEnumerator is IPythonObject);   // we shouldn't re-wrap things that don't need it
            object iter;

            if (PythonOps.TryGetBoundAttr(baseEnumerator, Symbols.Iterator, out iter)) {
                object iterator = PythonCalls.Call(iter);
                if (iterator is IEnumerable) {
                    enumerator = (IEnumerable)iterator;
                } else {
                    enumerator = new PythonEnumerable(iterator);
                }
                return true;
            } else {
                enumerator = null;
                return false;
            }
        }

        public static IEnumerable Create(object baseObject) {
            IEnumerable res;
            if (!TryCreate(baseObject, out res)) {
                throw PythonOps.TypeError("cannot convert {0} to IEnumerable", PythonTypeOps.GetName(baseObject));
            }
            return res;
        }

        private PythonEnumerable(object iterator) {
            this._iterator = iterator;
        }

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator() {
            return _iterator as IEnumerator ?? new PythonEnumerator(_iterator);
        }

        #endregion
    }

    [PythonType("iterator")]
    public class ItemEnumerator : IEnumerator {
        private readonly object _getItemMethod;
        private readonly CallSite<Func<CallSite, CodeContext, object, int, object>> _site;
        private object _current;
        private int _index;

        internal ItemEnumerator(object getItemMethod, CallSite<Func<CallSite, CodeContext, object, int, object>> site) {
            _getItemMethod = getItemMethod;
            _site = site;
        }

        #region IEnumerator members

        object IEnumerator.Current {
            get {
                return _current;
            }
        }

        bool IEnumerator.MoveNext() {
            if (_index < 0) {
                return false;
            }

            try {
                _current = _site.Target(_site, DefaultContext.Default,_getItemMethod,  _index);
                _index++;
                return true;
            } catch (IndexOutOfRangeException) {
                _current = null;
                _index = -1;     // this is the end
                return false;
            } catch (StopIterationException) {
                _current = null;
                _index = -1;     // this is the end
                return false;
            }
        }

        void IEnumerator.Reset() {
            _index = 0;
            _current = null;
        }

        #endregion
    }

    [PythonType("iterable")]
    public class ItemEnumerable : IEnumerable {
        private readonly object _getitem;
        private readonly CallSite<Func<CallSite, CodeContext, object, int, object>> _site;

        internal ItemEnumerable(object getitem, CallSite<Func<CallSite, CodeContext, object, int, object>> site) {
            _getitem = getitem;
            _site = site;
        }

        public IEnumerator __iter__() {
            return ((IEnumerable)this).GetEnumerator();
        }

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator() {
            return new ItemEnumerator(_getitem, _site);
        }

        #endregion
    }

}
