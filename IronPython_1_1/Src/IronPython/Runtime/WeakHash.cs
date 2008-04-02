/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public
 * License. A  copy of the license can be found in the License.html file at the
 * root of this distribution. If  you cannot locate the  Microsoft Public
 * License, please send an email to  dlr@microsoft.com. By using this source
 * code in any fashion, you are agreeing to be bound by the terms of the 
 * Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

using IronPython.Runtime.Operations;

namespace IronPython.Runtime {
    class WeakHash<TKey, TValue> : IDictionary<TKey, TValue> {
        // The one and only comparer instance.
        static readonly IEqualityComparer<object> comparer = new WeakComparer<object>();

        IDictionary<object, TValue> dict = new Dictionary<object, TValue>();
        int version, cleanupVersion, cleanupGC;

        public WeakHash() {
        }

        #region IDictionary<TKey,TValue> Members

        public void Add(TKey key, TValue value) {
            CheckCleanup();
            dict.Add(new WeakObject<TKey>(key), value);
        }

        public bool ContainsKey(TKey key) {
            return dict.ContainsKey(key);
        }

        public ICollection<TKey> Keys {
            get { throw new NotImplementedException(); }
        }

        public bool Remove(TKey key) {
            return dict.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value) {
            return dict.TryGetValue(key, out value);
        }

        public ICollection<TValue> Values {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public TValue this[TKey key] {
            get {
                return dict[key];
            }
            set {
                dict[new WeakObject<TKey>(key)] = value;
            }
        }

        void CheckCleanup() {
            version++;

            long change = version - cleanupVersion;


            // Cleanup the table if it is a while since we have done it last time.
            // Take the size of the table into account.
            if (change > 1234 + dict.Count / 2) {
                // It makes sense to do the cleanup only if a GC has happened in the meantime.
                // WeakReferences can become zero only during the GC.
                int currentGC = GC.CollectionCount(0);
                if (currentGC != cleanupGC) {
                    Cleanup();

                    cleanupVersion = version;
                    cleanupGC = currentGC;
                } else {
                    cleanupVersion += 1234;
                }
            }
        }
        void Cleanup() {

            int liveCount = 0;
            int emptyCount = 0;

            foreach (WeakObject<TKey> w in dict.Keys) {
                TKey tmp;
                if(w.TryGetTarget(out tmp)) 
                    liveCount++;
                else
                    emptyCount++;
            }

            // Rehash the table if there is a significant number of empty slots
            if (emptyCount > liveCount / 4) {
                Dictionary<object, TValue> newtable = new Dictionary<object, TValue>(liveCount + liveCount / 4, comparer);

                foreach (WeakObject<TKey> w in dict.Keys) {
                    TKey target;
                    if (w.TryGetTarget(out target)) {
                        newtable[w] = dict[w];
                    }
                    GC.KeepAlive(target);
                }

                dict = newtable;
            }
        }
        #endregion

        #region ICollection<KeyValuePair<TKey,TValue>> Members

        public void Add(KeyValuePair<TKey, TValue> item) {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Clear() {
            throw new Exception("The method or operation is not implemented.");
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) {
            throw new Exception("The method or operation is not implemented.");
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
            throw new Exception("The method or operation is not implemented.");
        }

        public int Count {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public bool IsReadOnly {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item) {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        #region IEnumerable<KeyValuePair<TKey,TValue>> Members

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }

    internal class WeakObject<T> {
        WeakReference weakReference;
        int hashCode;

        public WeakObject(T obj) {
            weakReference = new WeakReference(obj, true);
            hashCode = RuntimeHelpers.GetHashCode(obj);
        }

        public bool TryGetTarget(out T target) {
            object res = weakReference.Target;
            if (res != null) {
                target = (T)res;
                return true;
            }            
            target = default(T);
            return false;
        }

        public override int GetHashCode() {
            return hashCode;
        }

        public override bool Equals(object obj) {
            T target;
            if (TryGetTarget(out target)) {
                return target.Equals(obj);
            }
            return false;
        }
    }

    // WeakComparer treats WeakObject as transparent envelope
    sealed class WeakComparer<T> : IEqualityComparer<T> {
        bool IEqualityComparer<T>.Equals(T x, T y) {
            WeakObject<T> wx = x as WeakObject<T>;
            if (wx != null) {
                if (!wx.TryGetTarget(out x)) {
                    return false;
                }
            }

            WeakObject<T> wy = y as WeakObject<T>;
            if (wy != null) {
                if (!wy.TryGetTarget(out y)) {
                    return false;
                }
            }

            return Object.Equals(x, y);
        }

        int IEqualityComparer<T>.GetHashCode(T obj) {
            WeakObject<T> wobj = obj as WeakObject<T>;
            if (wobj != null)
                return wobj.GetHashCode();

            return RuntimeHelpers.GetHashCode(obj);
        }
    }

    sealed class HybridMapping<T> {
        Dictionary<int, object> dict = new Dictionary<int, object>();
        readonly Object synchObject = new Object();
        readonly int SIZE = 4096;
        int current = 0;

        public int WeakAdd(T value) {
            lock (synchObject) {
                int saved = current;
                while (dict.ContainsKey(current)) {
                    current = (current + 1) % SIZE;
                    if (current == saved)
                        throw Ops.SystemError("HybridMapping is full");
                }
                dict.Add(current, new WeakObject<T>(value));
                return current;
            }
        }

        public int StrongAdd(T value) {
            lock (synchObject) {
                int saved = current;
                while (dict.ContainsKey(current)) {
                    current = (current + 1) % SIZE;
                    if (current == saved)
                        throw Ops.SystemError("HybridMapping is full");
                }
                dict.Add(current, value);
                return current;
            }
        }

        public T GetObjectFromId(int id) {
            object ret;
            if (dict.TryGetValue(id, out ret)) {
                WeakObject<T> wt = ret as WeakObject<T>;
                if(wt != null) {
                    T tres;
                    if (wt.TryGetTarget(out tres)) {
                        return tres;
                    }
                    return default(T);
                }
                if (ret is T) {
                    return (T)ret;
                }

                throw Ops.SystemError("Unexpected dictionary content: type {0}", ret.GetType());
            } else
                return default(T);
        }

        public int GetIdFromObject(T value) {
            lock (synchObject) {
                foreach (KeyValuePair<int, object> kv in dict) {
                    WeakObject<T> val = kv.Value as WeakObject<T>;
                    if (val != null) {
                        T target;
                        if (val.TryGetTarget(out target) && target.Equals(value)) {
                            return kv.Key;
                        }
                    } else if (kv.Value is T) {
                        object target = (T)(kv.Value);
                        if (target.Equals(value))
                            return kv.Key;
                    }
                }
            }
            return -1;
        }

        public void RemoveOnId(int id) {
            lock (synchObject) {
                dict.Remove(id);
            }
        }

        public void RemoveOnObject(T value) {
            try {
                int id = GetIdFromObject(value);
                RemoveOnId(id);
            } catch { }
        }
    }
}
