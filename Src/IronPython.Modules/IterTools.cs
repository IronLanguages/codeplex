/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;

using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Calls;

[assembly: PythonModule("itertools", typeof(IronPython.Modules.PythonIterTools))]
namespace IronPython.Modules {
    [PythonType("itertools")]
    public static class PythonIterTools {
        
        [PythonName("tee")]
        public static object Tee(object iterable) {
            return Tee(iterable, 2);
        }
        [PythonName("tee")]
        public static object Tee(object iterable, int n) {
            if (n < 0) throw PythonOps.SystemError("bad argument to internal function");

            object[] res = new object[n];
            if (!(iterable is TeeIterator)) {
                IEnumerator iter = PythonOps.GetEnumerator(iterable);
                List dataList = new List();

                for (int i = 0; i < n; i++) {
                    res[i] = new TeeIterator(iter, dataList);
                }

            } else if (n != 0) {
                // if you pass in a tee you get back the original tee
                // and other iterators that share the same data.
                TeeIterator ti = iterable as TeeIterator;
                res[0] = ti;
                for (int i = 1; i < n; i++) {
                    res[1] = new TeeIterator(ti._iter, ti._data);
                }
            }

            return new PythonTuple(false, res);
        }

        /// <summary>
        /// Base class used for iterator wrappers.
        /// </summary>
        public class IterBase : IEnumerator {
            private IEnumerator _inner;

            protected IEnumerator InnerEnumerator {
                get { return _inner; }
                set { _inner = value; }
            }

            #region IEnumerator Members

            public object Current {
                get { return _inner.Current; }
            }

            public bool MoveNext() {
                return _inner.MoveNext();
            }

            public void Reset() {
                _inner.Reset();
            }

            #endregion
        }

        [PythonType("chain")]
        public class Chain : IterBase {
            IEnumerator[] iterables;
            public Chain(params object[] iterables) {
                this.iterables = new IEnumerator[iterables.Length];
                for (int i = 0; i < iterables.Length; i++) {
                    this.iterables[i] = PythonOps.GetEnumerator(iterables[i]);
                }
                InnerEnumerator = Yielder();
            }

            IEnumerator<object> Yielder() {
                for (int i = 0; i < iterables.Length; i++) {
                    while (MoveNextHelper(iterables[i])) {
                        yield return iterables[i].Current;
                    }
                }
            }
        }

        [PythonType("count")]
        public class Count : IEnumerator {
            private int _cur, _start;

            public Count() {
                _cur = _start = -1;
            }
            public Count(int n) {
                _cur = _start = (n - 1);
            }
            #region IEnumerator Members

            public object Current {
                get { return _cur; }
            }

            public bool MoveNext() {
                _cur++;
                return true;
            }

            public void Reset() {
                _cur = _start;
            }

            #endregion

            #region Object overrides
            public override string ToString() {
                return String.Format("{0}({1})", PythonOps.GetPythonTypeName(this), _cur + 1);
            }
            #endregion
        }

        [PythonType("cycle")]
        public class Cycle : IterBase {
            public Cycle(object iterable) {
                InnerEnumerator = Yielder(PythonOps.GetEnumerator(iterable));
            }

            IEnumerator<object> Yielder(IEnumerator iter) {
                List result = new List();
                while (MoveNextHelper(iter)) {
                    result.AddNoLock(iter.Current);
                    yield return iter.Current;
                }
                if (result.Count != 0) {
                    for (; ; ) {
                        for (int i = 0; i < result.Count; i++) {
                            yield return result[i];
                        }
                    }
                }
            }
        }

        [PythonType("dropwhile")]
        public class DropWhile : IterBase {
            private FastDynamicSite<object, object, bool> _callSite = RuntimeHelpers.CreateSimpleCallSite<object, object, bool>(DefaultContext.Default);

            public DropWhile(object predicate, object iterable) {
                InnerEnumerator = Yielder(predicate, PythonOps.GetEnumerator(iterable));
            }

            IEnumerator<object> Yielder(object predicate, IEnumerator iter) {
                while (MoveNextHelper(iter)) {
                    if (!_callSite.Invoke(predicate, iter.Current)) {
                        yield return iter.Current;
                        break;
                    }
                }

                while (MoveNextHelper(iter)) {
                    yield return iter.Current;
                }
            }
        }

        [PythonType("groupby")]
        public class GroupBy : IterBase {
            private static object _starterKey = new object();
            private bool _fFinished = false;
            private object _key;
            private FastDynamicSite<object, object, object> _callSite;
            private FastDynamicSite<object, object, bool> _eqSite = FastDynamicSite<object,object,bool>.Create(DefaultContext.Default, DoOperationAction.Make(Operators.Equals));

            public GroupBy(object iterable) {
                InnerEnumerator = Yielder(PythonOps.GetEnumerator(iterable));
            }

            public GroupBy(object iterable, object key) {
                InnerEnumerator = Yielder(PythonOps.GetEnumerator(iterable));
                if (key != null) {
                    _key = key;
                    _callSite = RuntimeHelpers.CreateSimpleCallSite<object, object, object>(DefaultContext.Default);
                }
            }

            private IEnumerator<object> Yielder(IEnumerator iter) {
                object curKey = _starterKey;
                if (MoveNextHelper(iter)) {
                    while (!_fFinished) {
                        while (_eqSite.Invoke(GetKey(iter.Current), curKey)) {
                            if (!MoveNextHelper(iter)) { _fFinished = true; yield break; }
                        }
                        curKey = GetKey(iter.Current);
                        yield return PythonTuple.MakeTuple(curKey, Grouper(iter, curKey));
                    }
                }
            }

            IEnumerator<object> Grouper(IEnumerator iter, object curKey) {
                while (_eqSite.Invoke(GetKey(iter.Current), curKey)) {
                    yield return iter.Current;
                    if (!MoveNextHelper(iter)) { _fFinished = true; yield break; }
                }
            }

            object GetKey(object val) {
                if (_key == null) return val;
                return _callSite.Invoke(_key, val);
            }
        }

        [PythonType("ifilter")]
        public class IteratorFilter : IterBase {
            private FastDynamicSite<object, object, bool> _callSite;

            public IteratorFilter(object predicate, object iterable) {
                InnerEnumerator = Yielder(predicate, PythonOps.GetEnumerator(iterable));
                if (predicate != null) {
                    _callSite = RuntimeHelpers.CreateSimpleCallSite<object, object, bool>(DefaultContext.Default);
                }
            }

            private IEnumerator<object> Yielder(object predicate, IEnumerator iter) {
                while (MoveNextHelper(iter)) {
                    if (ShouldYield(predicate, iter.Current)) {
                        yield return iter.Current;
                    }
                }
            }

            private bool ShouldYield(object predicate, object current) {
                if (predicate == null) return PythonOps.IsTrue(current);

                return _callSite.Invoke(predicate, current);
            }
        }

        [PythonType("ifilterfalse")]
        public class IteratorFilterFalse : IterBase {
            private FastDynamicSite<object, object, bool> _callSite;

            public IteratorFilterFalse(object predicate, object iterable) {
                InnerEnumerator = Yielder(predicate, PythonOps.GetEnumerator(iterable));
                if (predicate != null) {
                    _callSite = RuntimeHelpers.CreateSimpleCallSite<object, object, bool>(DefaultContext.Default);
                }
            }

            private IEnumerator<object> Yielder(object predicate, IEnumerator iter) {
                while (MoveNextHelper(iter)) {
                    if (ShouldYield(predicate, iter.Current)) {
                        yield return iter.Current;
                    }
                }
            }

            private bool ShouldYield(object predicate, object current) {
                if (predicate == null) return !PythonOps.IsTrue(current);
                return !_callSite.Invoke(predicate, current);
            }
        }

        [PythonType("imap")]
        public class IteratorMap : IEnumerator {
            private object _function;
            private IEnumerator[] _iterables;
            private FastDynamicSite<object, object[], object> _callSite;

            public IteratorMap(object function, params object[] iterables) {
                if (iterables.Length < 1) throw PythonOps.TypeError("imap() must have at least two arguments");

                this._function = function;
                if (function != null) {
                    _callSite = FastDynamicSite<object, object[], object>.Create(DefaultContext.Default, CallAction.Make(new CallSignature(ArgumentKind.List)));
                }

                this._iterables = new IEnumerator[iterables.Length];
                for (int i = 0; i < iterables.Length; i++) {
                    this._iterables[i] = PythonOps.GetEnumerator(iterables[i]);
                }
            }

            #region IEnumerator Members

            public object Current {
                get {
                    object[] args = new object[_iterables.Length];
                    for (int i = 0; i < args.Length; i++) {
                        args[i] = _iterables[i].Current;
                    }
                    if (_function == null) {
                        return new PythonTuple(false, args);
                    } else {
                        return _callSite.Invoke(_function, args);
                    }
                }
            }

            public bool MoveNext() {
                for (int i = 0; i < _iterables.Length; i++) {
                    if (!MoveNextHelper(_iterables[i])) return false;
                }
                return true;
            }

            public void Reset() {
                for (int i = 0; i < _iterables.Length; i++) {
                    _iterables[i].Reset();
                }
            }

            #endregion
        }

        [PythonType("islice")]
        public class IteratorSlice : IterBase {
            public IteratorSlice(object iterable, object stop)
                : this(iterable, 0, stop, 1) {
            }

            public IteratorSlice(object iterable, object start, object stop)
                : this(iterable, start, stop, 1) {
            }

            public IteratorSlice(object iterable, object start, object stop, int step) {
                int startInt, stopInt = -1;

                if (!Converter.TryConvertToInt32(start, out startInt) || startInt < 0)
                    throw PythonOps.ValueError("start argument must be non-negative integer, ({0})", start);

                if (stop != null) {
                    if (!Converter.TryConvertToInt32(stop, out stopInt) || stopInt < 0)
                        throw PythonOps.ValueError("stop argument must be non-negative integer ({0})", stop);
                }

                if (step <= 0) throw PythonOps.ValueError("step must be 1 or greater for islice");

                InnerEnumerator = Yielder(PythonOps.GetEnumerator(iterable), startInt, stopInt, step);
            }

            IEnumerator<object> Yielder(IEnumerator iter, int start, int stop, int step) {
                if (!MoveNextHelper(iter)) yield break;

                int cur = 0;
                while (cur < start) {
                    if (!MoveNextHelper(iter)) yield break;
                    cur++;
                }

                while (cur < stop || stop == -1) {
                    yield return iter.Current;
                    if ((cur + step) < 0) yield break;  // early out if we'll overflow.                    

                    for (int i = 0; i < step; i++, cur++) {
                        if (!MoveNextHelper(iter)) yield break;
                    }
                }
            }

        }

        [PythonType("izip")]
        public class IteratorZip : IEnumerator {
            IEnumerator[] iters;
            public IteratorZip(params object[] iterables) {
                iters = new IEnumerator[iterables.Length];
                for (int i = 0; i < iterables.Length; i++) {
                    iters[i] = PythonOps.GetEnumerator(iterables[i]);
                }
            }

            #region IEnumerator Members

            public object Current {
                get {
                    object[] res = new object[iters.Length];
                    for (int i = 0; i < res.Length; i++) {
                        res[i] = iters[i].Current;
                    }
                    return new PythonTuple(false, res);
                }
            }

            public bool MoveNext() {
                if (iters.Length == 0) return false;

                for (int i = 0; i < iters.Length; i++) {
                    if (!MoveNextHelper(iters[i])) return false;
                }
                return true;
            }

            public void Reset() {
                throw new NotImplementedException("The method or operation is not implemented.");
            }

            #endregion
        }

        [PythonType("repeat")]
        public class Repeat : IterBase {
            int remaining;
            bool fInfinite;
            object obj;

            public Repeat(object @object) {
                obj = @object;
                InnerEnumerator = Yielder();
                fInfinite = true;
            }
            public Repeat(object @object, int times) {
                obj = @object;
                InnerEnumerator = Yielder();
                remaining = times;
            }

            IEnumerator<object> Yielder() {
                while (fInfinite || remaining > 0) {
                    remaining--;
                    yield return obj;
                }
            }

            #region Object overrides
            public override string ToString() {
                if (fInfinite) {
                    return String.Format("{0}({1})", PythonOps.GetPythonTypeName(this), PythonOps.Repr(obj));
                }
                return String.Format("{0}({1}, {2})", PythonOps.GetPythonTypeName(this), PythonOps.Repr(obj), remaining);

            }
            #endregion

            [System.Runtime.CompilerServices.SpecialName, PythonName("__len__")]
            public int GetLength() {
                if (fInfinite) throw PythonOps.TypeError("len of unsized object");
                return Math.Max(remaining, 0);
            }
        }

        [PythonType("starmap")]
        public class StarMap : IterBase {
            private FastDynamicSite<object, object[], object> _callSite = FastDynamicSite<object, object[], object>.Create(DefaultContext.Default, 
                CallAction.Make(new CallSignature(ArgumentKind.List)));

            public StarMap(CodeContext context, object function, object iterable) {
                InnerEnumerator = Yielder(context, function, PythonOps.GetEnumerator(iterable));
            }

            private IEnumerator<object> Yielder(CodeContext context, object function, IEnumerator iter) {
                while (MoveNextHelper(iter)) {
                    PythonTuple args = iter.Current as PythonTuple;
                    if (args == null) throw PythonOps.TypeError("iterator must be a tuple");

                    object[] objargs = new object[args.Count];
                    for (int i = 0; i < objargs.Length; i++) {
                        objargs[i] = args[i];
                    }
                    yield return _callSite.Invoke(function, objargs);
                }
            }
        }

        [PythonType("takewhile")]
        public class TakeWhile : IterBase {
            private FastDynamicSite<object, object, bool> _callSite = 
                RuntimeHelpers.CreateSimpleCallSite<object, object, bool>(DefaultContext.Default);

            public TakeWhile(object predicate, object iterable) {
                InnerEnumerator = Yielder(predicate, PythonOps.GetEnumerator(iterable));
            }

            private IEnumerator<object> Yielder(object predicate, IEnumerator iter) {
                while (MoveNextHelper(iter)) {
                    if (!_callSite.Invoke(predicate, iter.Current)) break;
                    yield return iter.Current;
                }
            }
        }

        public class TeeIterator : IEnumerator, IWeakReferenceable {
            internal IEnumerator _iter;
            internal List _data;
            private int _curIndex = -1;
            private WeakRefTracker _weakRef;

            public TeeIterator(object iterable) {
                TeeIterator other = iterable as TeeIterator;
                if (other != null) {
                    this._iter = other._iter;
                    this._data = other._data;
                } else {
                    this._iter = PythonOps.GetEnumerator(iterable);
                    _data = new List();
                }
            }

            public TeeIterator(IEnumerator iter, List dataList) {
                this._iter = iter;
                this._data = dataList;
            }

            #region IEnumerator Members

            public object Current {
                get {
                    return _data[_curIndex];
                }
            }

            public bool MoveNext() {
                lock (_data) {
                    _curIndex++;
                    if (_curIndex >= _data.Count && MoveNextHelper(_iter)) {
                        _data.Add(_iter.Current);
                    }
                    return _curIndex < _data.Count;
                }
            }

            public void Reset() {
                throw new NotImplementedException("The method or operation is not implemented.");
            }

            #endregion

            #region IWeakReferenceable Members

            public WeakRefTracker GetWeakRef() {
                return (_weakRef);
            }

            public bool SetWeakRef(WeakRefTracker value) {
                _weakRef = value;
                return true;
            }

            public void SetFinalizer(WeakRefTracker value) {
                SetWeakRef(value);
            }

            #endregion
        }

        private static bool MoveNextHelper(IEnumerator move) {
            try { return move.MoveNext(); } catch (IndexOutOfRangeException) { return false; } catch (StopIterationException) { return false; }
        }

    }
}
