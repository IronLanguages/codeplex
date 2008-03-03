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
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using Microsoft.Scripting.Runtime;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

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
    public class CommonDictionaryStorage : DictionaryStorage {
        private Bucket[] _buckets;
        private int _count;

        /// <summary>
        /// Creates a new dictionary storage with no buckets
        /// </summary>
        public CommonDictionaryStorage() {
        }

        /// <summary>
        /// Creates a new dictionary storage with the given set of buckets
        /// and size.  Used when cloning the dictionary storage.
        /// </summary>
        private CommonDictionaryStorage(Bucket[] buckets, int count) {
            _buckets = buckets;
            _count = count;
        }

        /// <summary>
        /// Adds a new item to the dictionary, replacing an existing one if it already exists.
        /// </summary>
        public override void Add(object key, object value) {
            lock (this) {
                if (_buckets == null) {
                    Initialize();
                }

                if (Add(_buckets, key, value)) {
                    _count++;

                    if (_count >= _buckets.Length) {
                        // grow the hash table
                        Bucket[] newBuckets = new Bucket[_buckets.Length * 2];

                        for (int i = 0; i < _buckets.Length; i++) {
                            Bucket curBucket = _buckets[i];
                            while (curBucket != null) {
                                Add(newBuckets, curBucket.Key, curBucket.Value);

                                curBucket = curBucket.Next;
                            }
                        }

                        _buckets = newBuckets;
                    }
                }
            }
        }

        /// <summary>
        /// Initializes the buckets to their initial capacity, the caller
        /// must check if the buckets are empty first.
        /// </summary>
        private void Initialize() {
            _buckets = new Bucket[8];
        }

        /// <summary>
        /// Static add helper that works over a single set of buckets.  Used for
        /// both the normal add case as well as the resize case.
        /// </summary>
        private static bool Add(Bucket[] buckets, object key, object value) {
            int hc = Hash(key);

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
                //Console.WriteLine("{0} {1} {2} {3}", key, bucket.Key, hc, bucket.HashCode);
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

        /// <summary>
        /// Helper to hash the given key w/ support for null.
        /// </summary>
        private static int Hash(object key) {
            if (key == null) {
                return NoneTypeOps.NoneHashCode & 0x7fffffff;
            }
            return PythonOps.Hash(key) & 0x7fffffff;
        }

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
    }

}
