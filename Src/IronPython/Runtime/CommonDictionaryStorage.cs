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

using System; using Microsoft;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;

using System.Runtime.Serialization;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime {
    /// <summary>
    /// General purpose storage used for most PythonDictionarys.
    /// 
    /// This dictionary storage is thread safe for multiple readers or writers.
    /// 
    /// Mutations to the dictionary involves a simple locking strategy of
    /// locking on the DictionaryStorage object to ensure that only one
    /// mutation happens at a time.
    /// 
    /// Reads against the dictionary happen lock free.  When the dictionary is mutated
    /// it is either adding or removing buckets in a thread-safe manner so that the readers
    /// will either see a consistent picture as if the read occured before or after the mutation.
    /// 
    /// When resizing the dictionary the buckets are replaced atomically so that the reader
    /// sees the new buckets or the old buckets.  When reading the reader first reads
    /// the buckets and then calls a static helper function to do the read from the bucket
    /// array to ensure that readers are not seeing multiple bucket arrays.
    /// </summary>
    [Serializable]
    internal sealed class CommonDictionaryStorage : DictionaryStorage
#if !SILVERLIGHT
        , ISerializable, IDeserializationCallback 
#endif
    {
        private Bucket[] _buckets;
        private int _count;
        private const int InitialBucketSize = 7;
        private const int ResizeMultiplier = 3;

        class HashSite {
            internal static CallSite<Func<CallSite, object, int>> _HashSite = CallSite<Func<CallSite, object, int>>.Create(
                new PythonOperationBinder(
                    DefaultContext.DefaultPythonContext.DefaultBinderState,
                    OperatorStrings.Hash
                )
            );
        }

        /// <summary>
        /// Creates a new dictionary storage with no buckets
        /// </summary>
        public CommonDictionaryStorage() {
        }

        /// <summary>
        /// Creates a new dictionary storage with no buckets
        /// </summary>
        public CommonDictionaryStorage(int count) {
            _buckets = new Bucket[count + 1];
        }

        /// <summary>
        /// Creates a new dictionary geting values/keys from the
        /// items arary
        /// </summary>
        public CommonDictionaryStorage(object[] items)
            : this(Math.Max(items.Length / 2, InitialBucketSize)) {
            for (int i = 0; i < items.Length / 2; i++) {
                AddNoLock(items[i * 2 + 1], items[i * 2]);
            }
        }

        /// <summary>
        /// Creates a new dictionary storage with the given set of buckets
        /// and size.  Used when cloning the dictionary storage.
        /// </summary>
        private CommonDictionaryStorage(Bucket[] buckets, int count) {
            _buckets = buckets;
            _count = count;
        }

#if !SILVERLIGHT
        private CommonDictionaryStorage(SerializationInfo info, StreamingContext context) {
            // remember the serialization info, we'll deserialize when we get the callback.  This
            // enables special types like DBNull.Value to successfully be deserialized inside of us.  We
            // store the serialization info in a special bucket so we don't have an extra field just for
            // serialization
            _buckets = new Bucket[] { new DeserializationBucket(info) };
        }
#endif

        /// <summary>
        /// Adds a new item to the dictionary, replacing an existing one if it already exists.
        /// </summary>
        public override void Add(object key, object value) {
            lock (this) {
                AddNoLock(key, value);
            }
        }

        public override void AddNoLock(object key, object value) {
            if (_buckets == null) {
                Initialize();
            }

            if (Add(_buckets, key, value)) {
                _count++;

                if (_count >= _buckets.Length) {
                    // grow the hash table
                    EnsureSize(_buckets.Length * ResizeMultiplier);
                }
            }
        }

        private void EnsureSize(int newSize) {
            if (_buckets.Length >= newSize) {
                return;
            }

            Bucket[] newBuckets = new Bucket[newSize];

            for (int i = 0; i < _buckets.Length; i++) {
                Bucket curBucket = _buckets[i];
                while (curBucket != null) {
                    Bucket next = curBucket.Next;

                    AddWorker(newBuckets, curBucket.Key, curBucket.Value, curBucket.HashCode);

                    curBucket = next;
                }
            }

            _buckets = newBuckets;
        }

        /// <summary>
        /// Initializes the buckets to their initial capacity, the caller
        /// must check if the buckets are empty first.
        /// </summary>
        private void Initialize() {
            _buckets = new Bucket[InitialBucketSize];
        }

        /// <summary>
        /// Static add helper that works over a single set of buckets.  Used for
        /// both the normal add case as well as the resize case.
        /// </summary>
        private static bool Add(Bucket[] buckets, object key, object value) {
            int hc = Hash(key);

            return AddWorker(buckets, key, value, hc);
        }

        private static bool AddWorker(Bucket[] buckets, object key, object value, int hc) {
            int index = hc % buckets.Length;
            Bucket prev = buckets[index];
            Bucket cur = prev;

            while (cur != null) {
                if (cur.HashCode == hc && PythonOps.EqualRetBool(key, cur.Key)) {
                    cur.Value = value;
                    return false;
                }

                prev = cur;
                cur = cur.Next;
            }

            if (prev != null) {
                Debug.Assert(prev.Next == null);
                prev.Next = new Bucket(hc, key, value, null);
            } else {
                buckets[index] = new Bucket(hc, key, value, null);
            }

            return true;
        }

        /// <summary>
        /// Removes an entry from the dictionary and returns true if the
        /// entry was removed or false.
        /// </summary>
        public override bool Remove(object key) {
            int hc = Hash(key);

            lock (this) {
                if (_buckets == null) return false;

                int index = hc % _buckets.Length;
                Bucket bucket = _buckets[index];
                Bucket prev = bucket;
                while (bucket != null) {
                    if (bucket.HashCode == hc && PythonOps.EqualRetBool(key, bucket.Key)) {
                        if (prev == bucket) {
                            _buckets[index] = bucket.Next;
                        } else {
                            prev.Next = bucket.Next;
                        }
                        _count--;
                        return true;
                    }
                    prev = bucket;
                    bucket = bucket.Next;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks to see if the key exists in the dictionary.
        /// </summary>
        public override bool Contains(object key) {
            return Contains(_buckets, key);
        }

        /// <summary>
        /// Static helper to see if the key exists in the provided bucket array.
        /// 
        /// Used so the contains check can run against a buckets while a writer
        /// replaces the buckets.
        /// </summary>
        private static bool Contains(Bucket[] buckets, object key) {
            if (buckets == null) return false;

            
            int hc = Hash(key);
            Bucket bucket = buckets[hc % buckets.Length];
            while (bucket != null) {
                if (bucket.HashCode == hc && PythonOps.EqualRetBool(key, bucket.Key)) {
                    return true;
                }
                bucket = bucket.Next;
            }
            return false;
        }

        /// <summary>
        /// Trys to get the value associated with the given key and returns true
        /// if it's found or false if it's not present.
        /// </summary>
        public override bool TryGetValue(object key, out object value) {
            return TryGetValue(_buckets, key, out value);
        }

        /// <summary>
        /// Static helper to try and get the value from the dictionary.
        /// 
        /// Used so the value lookup can run against a buckets while a writer
        /// replaces the buckets.
        /// </summary>
        private static bool TryGetValue(Bucket[] buckets, object key, out object value) {
            if (buckets != null) {
                int hc = Hash(key);
                Bucket bucket = buckets[hc % buckets.Length];
                while (bucket != null) {
                    if (bucket.HashCode == hc && PythonOps.EqualRetBool(key, bucket.Key)) {
                        value = bucket.Value;
                        return true;
                    }
                    bucket = bucket.Next;
                }
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Returns the number of key/value pairs currently in the dictionary.
        /// </summary>
        public override int Count {
            get { return _count; }
        }

        /// <summary>
        /// Clears the contents of the dictionary.
        /// </summary>
        public override void Clear() {
            lock (this) {
                if (_buckets != null) {
                    _buckets = new Bucket[8];
                    _count = 0;
                }
            }
        }

        public override List<KeyValuePair<object, object>> GetItems() {
            lock (this) {
                List<KeyValuePair<object, object>> res = new List<KeyValuePair<object, object>>(Count);
                if (_buckets != null) {
                    for (int i = 0; i < _buckets.Length; i++) {
                        Bucket curBucket = _buckets[i];
                        while (curBucket != null) {
                            res.Add(new KeyValuePair<object, object>(curBucket.Key, curBucket.Value));

                            curBucket = curBucket.Next;
                        }
                    }
                }
                return res;
            }
        }

        public override bool HasNonStringAttributes() {
            lock (this) {
                if (_buckets != null) {
                    for (int i = 0; i < _buckets.Length; i++) {
                        Bucket curBucket = _buckets[i];
                        while (curBucket != null) {
                            if (!(curBucket.Key is string)) {
                                return true;
                            }
                            
                            curBucket = curBucket.Next;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Clones the storage returning a new DictionaryStorage object.
        /// </summary>
        public override DictionaryStorage Clone() {
            lock (this) {
                if (_buckets == null) {
                    return new CommonDictionaryStorage();
                }

                Bucket[] resBuckets = new Bucket[_buckets.Length];
                for (int i = 0; i < _buckets.Length; i++) {
                    if (_buckets[i] != null) {
                        resBuckets[i] = _buckets[i].Clone();
                    }
                }

                return new CommonDictionaryStorage(resBuckets, Count);
            }
        }

        public override void CopyTo(DictionaryStorage/*!*/ into) {
            Debug.Assert(into != null);

            if (_buckets != null) {
                using (new OrderedLocker(this, into)) {
                    CommonDictionaryStorage commonInto = into as CommonDictionaryStorage;
                    if (commonInto != null) {
                        CommonCopyTo(commonInto);
                    } else {
                        UncommonCopyTo(into);
                    }
                }
            }
        }

        private void CommonCopyTo(CommonDictionaryStorage into) {
            if (into._buckets == null) {
                into._buckets = new Bucket[_buckets.Length];
            } else {
                int curSize = into._buckets.Length;
                while (curSize < _count + into._count) {
                    curSize *= ResizeMultiplier;
                }
                into.EnsureSize(curSize);
            }
            
            for (int i = 0; i < _buckets.Length; i++) {
                Bucket curBucket = _buckets[i];
                while (curBucket != null) {
                    if (AddWorker(into._buckets, curBucket.Key, curBucket.Value, curBucket.HashCode)) {
                        into._count++;
                    }
                    curBucket = curBucket.Next;
                }
            }            
        }

        private void UncommonCopyTo(DictionaryStorage into) {
            for (int i = 0; i < _buckets.Length; i++) {
                Bucket curBucket = _buckets[i];
                while (curBucket != null) {
                    into.AddNoLock(curBucket.Key, curBucket.Value);

                    curBucket = curBucket.Next;
                }
            }
        }

        /// <summary>
        /// Helper to hash the given key w/ support for null.
        /// </summary>
        private static int Hash(object key) {
            if (key is string) return key.GetHashCode() & 0x7fffffff;

            return GeneralHash(key);
        }

        private static int GeneralHash(object key) {
            return HashSite._HashSite.Target(HashSite._HashSite, key) & 0x7fffffff;
        }

        /// <summary>
        /// Used to store a single hashed key/value and a linked list of
        /// collisions.
        /// 
        /// Bucket is not serializable because it stores the computed hash
        /// code which could change between serialization and deserialization.
        /// </summary>
        private class Bucket {
            public object Key;          // the key to be hashed
            public object Value;        // the value associated with the key
            public Bucket Next;         // the next chained bucket when there's a collision
            public int HashCode;        // the hash code of the contained key.

            public Bucket() {
            }

            public Bucket(int hashCode, object key, object value, Bucket next) {
                HashCode = hashCode;
                Key = key;
                Value = value;
                Next = next;
            }

            public Bucket Clone() {
                return new Bucket(HashCode, Key, Value, CloneNext());
            }

            private Bucket CloneNext() {
                if (Next == null) return null;
                return Next.Clone();
            }
        }

#if !SILVERLIGHT

        /// <summary>
        /// Special marker bucket used during deserialization to not add
        /// an extra field to the dictionary storage type.
        /// </summary>
        private class DeserializationBucket : Bucket {            
            public readonly SerializationInfo/*!*/ SerializationInfo;

            public DeserializationBucket(SerializationInfo info) {
                SerializationInfo = info;
            }
        }

        private DeserializationBucket GetDeserializationBucket() {
            if (_buckets == null) {
                return null;
            }

            if (_buckets.Length != 1) {
                return null;
            }

            return _buckets[0] as DeserializationBucket;
        }

        #region ISerializable Members

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("buckets", GetItems());            
        }

        #endregion

        #region IDeserializationCallback Members

        void IDeserializationCallback.OnDeserialization(object sender) {
            DeserializationBucket bucket = GetDeserializationBucket();
            if (bucket == null) {
                // we've received multiple OnDeserialization callbacks, only 
                // deserialize after the 1st one
                return;
            }

            SerializationInfo info = bucket.SerializationInfo;
            _buckets = null;

            var buckets = (List<KeyValuePair<object, object>>)info.GetValue("buckets", typeof(List<KeyValuePair<object, object>>));

            foreach (KeyValuePair<object, object> kvp in buckets) {
                Add(kvp.Key, kvp.Value);
            }
        }

        #endregion
#endif
    }

}
