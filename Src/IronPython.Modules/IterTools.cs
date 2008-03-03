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
using System.Threading;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;

using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Calls;
using Microsoft.Scripting.Runtime;

[assembly: PythonModule("itertools", typeof(IronPython.Modules.PythonIterTools))]
namespace IronPython.Modules {
    public static class PythonIterTools {
        
        public static object tee(object iterable) {
            return tee(iterable, 2);
        }

        public static object tee(object iterable, int n) {
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

            object IEnumerator.Current {
                get { return _inner.Current; }
            }

            bool IEnumerator.MoveNext() {
                return _inner.MoveNext();
            }

            void IEnumerator.Reset() {
                _inner.Reset();
            }

            #endregion
        }

        public class chain : IterBase {
            IEnumerator[] _iterables;
            
            public chain(params object[] iterables) {
                _iterables = new IEnumerator[iterables.Length];
                for (int i = 0; i < iterables.Length; i++) {
                    _iterables[i] = PythonOps.GetEnumerator(iterables[i]);
                }
                InnerEnumerator = Yielder();
            }

            private IEnumerator<object> Yielder() {
                for (int i = 0; i < _iterables.Length; i++) {
                    while (MoveNextHelper(_iterables[i])) {
                        yield return _iterables[i].Current;
                    }
                }
            }
        }

        [PythonSystemType]
        public class count : IEnumerator {
            private int _cur, _start;

            public count() {
                _cur = _start = -1;
            }

            public count(int n) {
                _cur = _start = (n - 1);
            }

            #region IEnumerator Members

            object IEnumerator.Current {
                get { return _cur; }
            }

            bool IEnumerator.MoveNext() {
                _cur++;
                return true;
            }

            void IEnumerator.Reset() {
                _cur = _start;
            }

            #endregion

            #region Object overrides

            public override string ToString() {
                return String.Format("{0}({1})", PythonOps.GetPythonTypeName(this), _cur + 1);
            }

            #endregion
        }

        [PythonSystemType]
        public class cycle : IterBase {
            public cycle(object iterable) {
                InnerEnumerator = Yielder(PythonOps.GetEnumerator(iterable));
            }

            private IEnumerator<object> Yielder(IEnumerator iter) {
                List result = new List();
                while (MoveNextHelper(iter)) {
                    result.AddNoLock(iter.Current);
                    yield return iter.Current;
                }
                if (result.__len__() != 0) {
                    for (; ; ) {
                        for (int i = 0; i < result.__len__(); i++) {
                            yield return result[i];
                        }
                    }
                }
            }
        }

        [PythonSystemType]
        public class dropwhile : IterBase {
            private FastDynamicSite<object, object, bool> _callSite = RuntimeHelpers.CreateSimpleCallSite<object, object, bool>(DefaultContext.Default);

            public dropwhile(object predicate, object iterable) {
                InnerEnumerator = Yielder(predicate, PythonOps.GetEnumerator(iterable));
            }

            private IEnumerator<object> Yielder(object predicate, IEnumerator iter) {
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

        [PythonSystemType]
        public class groupby : IterBase {
            private static readonly object _starterKey = new object();
            private bool _fFinished = false;
            private object _key;
            private FastDynamicSite<object, object, object> _callSite;
            private FastDynamicSite<object, object, bool> _eqSite = FastDynamicSite<object,object,bool>.Create(DefaultContext.Default, DoOperationAction.Make(Operators.Equals));

            public groupby(object iterable) {
                InnerEnumerator = Yielder(PythonOps.GetEnumerator(iterable));
            }

            public groupby(object iterable, object key) {
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

            private IEnumerator<object> Grouper(IEnumerator iter, object curKey) {
                while (_eqSite.Invoke(GetKey(iter.Current), curKey)) {
                    yield return iter.Current;
                    if (!MoveNextHelper(iter)) { _fFinished = true; yield break; }
                }
            }

            private object GetKey(object val) {
                if (_key == null) return val;
                return _callSite.Invoke(_key, val);
            }
        }

        [PythonSystemType]
        public class ifilter : IterBase {
            private FastDynamicSite<object, object, bool> _callSite;

            public ifilter(object predicate, object iterable) {
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

        [PythonSystemType]
        public class ifilterfalse : IterBase {
            private FastDynamicSite<object, object, bool> _callSite;

            public ifilterfalse(object predicate, object iterable) {
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

        [PythonSystemType]
        public class imap : IEnumerator {
            private object _function;
            private IEnumerator[] _iterables;
            private FastDynamicSite<object, object[], object> _callSite;

            public imap(object function, params object[] iterables) {
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

            object IEnumerator.Current {
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

            bool IEnumerator.MoveNext() {
                for (int i = 0; i < _iterables.Length; i++) {
                    if (!MoveNextHelper(_iterables[i])) return false;
                }
                return true;
            }

            void IEnumerator.Reset() {
                for (int i = 0; i < _iterables.Length; i++) {
                    _iterables[i].Reset();
                }
            }

            #endregion
        }

        [PythonSystemType]
        public class islice : IterBase {
            public islice(object iterable, object stop)
                : this(iterable, 0, stop, 1) {
            }

            public islice(object iterable, object start, object stop)
                : this(iterable, start, stop, 1) {
            }

            public islice(object iterable, object start, object stop, int step) {
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

            private IEnumerator<object> Yielder(IEnumerator iter, int start, int stop, int step) {
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

        [PythonSystemType]
        public class izip : IEnumerator {
            private readonly IEnumerator[]/*!*/ _iters;
            private readonly object[]/*!*/ _currentValues;

            public izip(params object[] iterables) {
                _iters = new IEnumerator[iterables.Length];
                _currentValues = new object[iterables.Length];

                for (int i = 0; i < iterables.Length; i++) {
                    _iters[i] = PythonOps.GetEnumerator(iterables[i]);
                }
            }

            #region IEnumerator Members

            object IEnumerator.Current {
                get {
                    return PythonOps.MakeTuple((object[])_currentValues.Clone());
                }
            }

            bool IEnumerator.MoveNext() {
                if (_iters.Length == 0) return false;

                for (int i = 0; i < _iters.Length; i++) {
                    if (!MoveNextHelper(_iters[i])) return false;

                    // values need to be extraced and saved as we move incase
                    // the user passed the same iterable multiple times.
                    _currentValues[i] = _iters[i].Current;
                }
                return true;
            }

            void IEnumerator.Reset() {
                throw new NotImplementedException("The method or operation is not implemented.");
            }

            #endregion
        }

        [PythonSystemType]
        public class repeat : IterBase, ICodeFormattable, ICollection {
            private int _remaining;
            private bool _fInfinite;
            private object _obj;

            public repeat(object @object) {
                _obj = @object;
                InnerEnumerator = Yielder();
                _fInfinite = true;
            }

            public repeat(object @object, int times) {
                _obj = @object;
                InnerEnumerator = Yielder();
                _remaining = times;
            }

            private IEnumerator<object> Yielder() {
                while (_fInfinite || _remaining > 0) {
                    _remaining--;
                    yield return _obj;
                }
            }

            public int __len__() {
                if (_fInfinite) throw PythonOps.TypeError("len of unsized object");
                return Math.Max(_remaining, 0);
            }

            #region ICodeFormattable Members

            public virtual string/*!*/ __repr__(CodeContext/*!*/ context) {
                if (_fInfinite) {
                    return String.Format("{0}({1})", PythonOps.GetPythonTypeName(this), PythonOps.Repr(_obj));
                }
                return String.Format("{0}({1}, {2})", PythonOps.GetPythonTypeName(this), PythonOps.Repr(_obj), _remaining);
            }

            #endregion

            #region ICollection Members

            public void CopyTo(Array array, int index) {
                if (_fInfinite) throw new InvalidOperationException();
                if (_remaining > array.Length - index) {
                    throw new IndexOutOfRangeException();
                }
                for (int i = 0; i < _remaining; i++) {
                    array.SetValue(_obj, index + i);
                }
                _remaining = 0;
            }

            public int Count {
                get { return __len__(); }
            }

            public bool IsSynchronized {
                get { return false; }
            }

            public object SyncRoot {
                get { throw new NotImplementedException(); }
            }

            #endregion

            #region IEnumerable Members

            public IEnumerator GetEnumerator() {
                while (_fInfinite || _remaining > 0) {
                    _remaining--;
                    yield return _obj;
                }
            }

            #endregion
        }
        
        [PythonSystemType]
        public class starmap : IterBase {
            private FastDynamicSite<object, object[], object> _callSite = FastDynamicSite<object, object[], object>.Create(DefaultContext.Default, 
                CallAction.Make(new CallSignature(ArgumentKind.List)));

            public starmap(CodeContext context, object function, object iterable) {
                InnerEnumerator = Yielder(context, function, PythonOps.GetEnumerator(iterable));
            }

            private IEnumerator<object> Yielder(CodeContext context, object function, IEnumerator iter) {
                while (MoveNextHelper(iter)) {
                    PythonTuple args = iter.Current as PythonTuple;
                    if (args == null) throw PythonOps.TypeError("iterator must be a tuple");

                    object[] objargs = new object[args.__len__()];
                    for (int i = 0; i < objargs.Length; i++) {
                        objargs[i] = args[i];
                    }
                    yield return _callSite.Invoke(function, objargs);
                }
            }
        }

        [PythonSystemType]
        public class takewhile : IterBase {
            private FastDynamicSite<object, object, bool> _callSite = 
                RuntimeHelpers.CreateSimpleCallSite<object, object, bool>(DefaultContext.Default);

            public takewhile(object predicate, object iterable) {
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

            object IEnumerator.Current {
                get {
                    return _data[_curIndex];
                }
            }

            bool IEnumerator.MoveNext() {
                lock (_data) {
                    _curIndex++;
                    if (_curIndex >= _data.__len__() && MoveNextHelper(_iter)) {
                        _data.append(_iter.Current);
                    }
                    return _curIndex < _data.__len__();
                }
            }

            void IEnumerator.Reset() {
                throw new NotImplementedException("The method or operation is not implemented.");
            }

            #endregion

            #region IWeakReferenceable Members

            WeakRefTracker IWeakReferenceable.GetWeakRef() {
                return (_weakRef);
            }

            bool IWeakReferenceable.SetWeakRef(WeakRefTracker value) {
                _weakRef = value;
                return true;
            }

            void IWeakReferenceable.SetFinalizer(WeakRefTracker value) {
                _weakRef = value;
            }

            #endregion
        }

        private static bool MoveNextHelper(IEnumerator move) {
            try { return move.MoveNext(); } catch (IndexOutOfRangeException) { return false; } catch (StopIterationException) { return false; }
        }
    }
}
