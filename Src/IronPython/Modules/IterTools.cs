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
using System.Collections.Generic;
using System.Text;
using IronPython.Runtime;

[assembly: PythonModule("itertools", typeof(IronPython.Modules.IterTools))]
namespace IronPython.Modules {
    public static class IterTools {
        [PythonName("tee")]
        public static object Tee(object iterable) {
            return Tee(iterable, 2);
        }
        [PythonName("tee")]
        public static object Tee(object iterable, int n) {
            if (n < 0) throw Ops.SystemError("bad argument to internal function");

            object[] res = new object[n];
            if (!(iterable is TeeIterator)) {
                IEnumerator iter = Ops.GetEnumerator(iterable);
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
                    res[1] = new TeeIterator(ti.iter, ti.data);
                }
            }

            return new Tuple(false, res);
        }

        /// <summary>
        /// Base class used for iterator wrappers.
        /// </summary>
        public class IterBase : IEnumerator {
            protected IEnumerator inner;

            #region IEnumerator Members

            public object Current {
                get { return inner.Current; }
            }

            public bool MoveNext() {
                return inner.MoveNext();
            }

            public void Reset() {
                inner.Reset();
            }

            #endregion
        }

        [PythonType("chain")]
        public class Chain : IterBase {
            IEnumerator[] iterables;
            public Chain(params object[] iterables) {
                this.iterables = new IEnumerator[iterables.Length];
                for (int i = 0; i < iterables.Length; i++) {
                    this.iterables[i] = Ops.GetEnumerator(iterables[i]);
                }
                inner = Yielder();
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
            int cur, start;

            public Count() {
                cur = start = -1;
            }
            public Count(int n) {
                cur = start = (n - 1);
            }
            #region IEnumerator Members

            public object Current {
                get { return cur; }
            }

            public bool MoveNext() {
                cur++;
                return true;
            }

            public void Reset() {
                cur = start;
            }

            #endregion

            #region Object overrides
            public override string ToString() {
                return String.Format("{0}({1})", Ops.GetDynamicType(this).__name__, cur + 1);
            }
            #endregion
        }

        [PythonType("cycle")]
        public class Cycle : IterBase {
            public Cycle(object iterable) {
                inner = Yielder(Ops.GetEnumerator(iterable));
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
            public DropWhile(object predicate, object iterable) {
                inner = Yielder(predicate, Ops.GetEnumerator(iterable));
            }

            IEnumerator<object> Yielder(object predicate, IEnumerator iter) {
                while (MoveNextHelper(iter)) {
                    if (!Ops.IsTrue(Ops.Call(predicate, iter.Current))) {
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
            static object starterKey = new object();
            bool fFinished = false;

            public GroupBy(object iterable) {
                inner = Yielder(Ops.GetEnumerator(iterable), null);
            }

            public GroupBy(object iterable, object key) {
                inner = Yielder(Ops.GetEnumerator(iterable), key);
            }

            IEnumerator<object> Yielder(IEnumerator iter, object keyFunc) {
                object curKey = starterKey;
                if (MoveNextHelper(iter)) {
                    while (!fFinished) {
                        while (Ops.EqualRetBool(GetKey(iter.Current, keyFunc), curKey)) {
                            if (!MoveNextHelper(iter)) { fFinished = true; yield break; }
                        }
                        curKey = GetKey(iter.Current, keyFunc);
                        yield return Tuple.MakeTuple(curKey, Grouper(iter, curKey, keyFunc));
                    }
                }
            }

            IEnumerator<object> Grouper(IEnumerator iter, object curKey, object keyFunc) {
                while (Ops.EqualRetBool(GetKey(iter.Current, keyFunc), curKey)) {
                    yield return iter.Current;
                    if (!MoveNextHelper(iter)) { fFinished = true; yield break; }
                }
            }

            static object GetKey(object val, object keyFunc) {
                if (keyFunc == null) return val;
                return Ops.Call(keyFunc, val);
            }
        }

        [PythonType("ifilter")]
        public class IteratorFilter : IterBase {
            public IteratorFilter(object predicate, object iterable) {
                inner = Yielder(predicate, Ops.GetEnumerator(iterable));
            }

            IEnumerator<object> Yielder(object predicate, IEnumerator iter) {
                while (MoveNextHelper(iter)) {
                    if (ShouldYield(predicate, iter.Current)) {
                        yield return iter.Current;
                    }
                }
            }

            static bool ShouldYield(object predicate, object current) {
                if (predicate == null) return Ops.IsTrue(current);
                return Ops.IsTrue(Ops.Call(predicate, current));
            }
        }

        [PythonType("ifilterfalse")]
        public class IteratorFilterFalse : IterBase {
            public IteratorFilterFalse(object predicate, object iterable) {
                inner = Yielder(predicate, Ops.GetEnumerator(iterable));
            }

            IEnumerator<object> Yielder(object predicate, IEnumerator iter) {
                while (MoveNextHelper(iter)) {
                    if (ShouldYield(predicate, iter.Current)) {
                        yield return iter.Current;
                    }
                }
            }

            static bool ShouldYield(object predicate, object current) {
                if (predicate == null) return !Ops.IsTrue(current);
                return !Ops.IsTrue(Ops.Call(predicate, current));
            }
        }

        [PythonType("imap")]
        public class IteratorMap : IEnumerator {
            object function;
            IEnumerator[] iterables;

            public IteratorMap(object function, params object[] iterables) {
                if (iterables.Length < 1) throw Ops.TypeError("imap() must have at least two arguments");

                this.function = function;
                this.iterables = new IEnumerator[iterables.Length];
                for (int i = 0; i < iterables.Length; i++) {
                    this.iterables[i] = Ops.GetEnumerator(iterables[i]);
                }
            }

            #region IEnumerator Members

            public object Current {
                get {
                    object[] args = new object[iterables.Length];
                    for (int i = 0; i < args.Length; i++) {
                        args[i] = iterables[i].Current;
                    }
                    if (function == null) {
                        return new Tuple(false, args);
                    } else {
                        return Ops.Call(function, args);
                    }
                }
            }

            public bool MoveNext() {
                for (int i = 0; i < iterables.Length; i++) {
                    if (!MoveNextHelper(iterables[i])) return false;
                }
                return true;
            }

            public void Reset() {
                for (int i = 0; i < iterables.Length; i++) {
                    iterables[i].Reset();
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
                Conversion conv;

                object res = Converter.TryConvertToInt32(start, out conv);
                if (conv == Conversion.None || ((int)res) < 0) throw Ops.ValueError("start argument must be non-negative integer");
                startInt = (int)res;

                if (stop != null) {
                    res = Converter.TryConvertToInt32(stop, out conv);
                    if (conv == Conversion.None || ((int)res) < 0) throw Ops.ValueError("stop argument must be non-negative integer ({0})", stop);
                    stopInt = (int)res;
                }

                if (step <= 0) throw Ops.ValueError("step must be 1 or greater for islice");

                inner = Yielder(Ops.GetEnumerator(iterable), startInt, stopInt, step);
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
                    iters[i] = Ops.GetEnumerator(iterables[i]);
                }
            }

            #region IEnumerator Members

            public object Current {
                get {
                    object[] res = new object[iters.Length];
                    for (int i = 0; i < res.Length; i++) {
                        res[i] = iters[i].Current;
                    }
                    return new Tuple(false, res);
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
                throw new Exception("The method or operation is not implemented.");
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
                inner = Yielder();
                fInfinite = true;
            }
            public Repeat(object @object, int times) {
                obj = @object;
                inner = Yielder();
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
                    return String.Format("{0}({1})", Ops.GetDynamicType(this).__name__, Ops.Repr(obj));
                }
                return String.Format("{0}({1}, {2})", Ops.GetDynamicType(this).__name__, Ops.Repr(obj), remaining);

            }
            #endregion

            [PythonName("__len__")]
            public int GetLength() {
                if (fInfinite) throw Ops.TypeError("len of unsized object");
                return Math.Max(remaining, 0);
            }
        }

        [PythonType("starmap")]
        public class StarMap : IterBase {
            public StarMap(object function, object iterable) {
                inner = Yielder(function, Ops.GetEnumerator(iterable));
            }

            IEnumerator<object> Yielder(object function, IEnumerator iter) {
                while (MoveNextHelper(iter)) {
                    Tuple args = iter.Current as Tuple;
                    if (args == null) throw Ops.TypeError("iterator must be a tuple");

                    object[] objargs = new object[args.Count];
                    for (int i = 0; i < objargs.Length; i++) {
                        objargs[i] = args[i];
                    }
                    yield return Ops.Call(function, objargs);
                }
            }
        }

        [PythonType("takewhile")]
        public class TakeWhile : IterBase {
            public TakeWhile(object predicate, object iterable) {
                inner = Yielder(predicate, Ops.GetEnumerator(iterable));
            }

            IEnumerator<object> Yielder(object predicate, IEnumerator iter) {
                while (MoveNextHelper(iter)) {
                    if (!Ops.IsTrue(Ops.Call(predicate, iter.Current))) break;
                    yield return iter.Current;
                }
            }
        }

        public class TeeIterator : IEnumerator, IWeakReferenceable {
            internal IEnumerator iter;
            internal List data;
            int curIndex = -1;
            WeakRefTracker weakRef;

            public TeeIterator(object iterable) {
                TeeIterator other = iterable as TeeIterator;
                if (other != null) {
                    this.iter = other.iter;
                    this.data = other.data;
                } else {
                    this.iter = Ops.GetEnumerator(iterable);
                    data = new List();
                }
            }

            public TeeIterator(IEnumerator iter, List dataList) {
                this.iter = iter;
                this.data = dataList;
            }

            #region IEnumerator Members

            public object Current {
                get {
                    return data[curIndex];
                }
            }

            public bool MoveNext() {
                lock (data) {
                    curIndex++;
                    if (curIndex >= data.Count && MoveNextHelper(iter)) {
                        data.Add(iter.Current);
                    }
                    return curIndex < data.Count;
                }
            }

            public void Reset() {
                throw new Exception("The method or operation is not implemented.");
            }

            #endregion

            #region IWeakReferenceable Members

            public WeakRefTracker GetWeakRef() {
                return (weakRef);
            }

            public bool SetWeakRef(WeakRefTracker value) {
                weakRef = value;
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
