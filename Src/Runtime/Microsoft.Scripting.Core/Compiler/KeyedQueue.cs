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
using Microsoft.Linq.Expressions;

#if SILVERLIGHT
using System.Core;
#endif

namespace Microsoft.Linq.Expressions.Compiler {
    
    /// <summary>
    /// A simple dictionary of queues, keyed off a particular type
    /// This is useful for storing free lists of variables
    /// </summary>
    internal sealed class KeyedQueue<K, V> {
        private readonly Dictionary<K, Queue<V>> _data;

        internal KeyedQueue() {
            _data = new Dictionary<K, Queue<V>>();
        }

        internal void Enqueue(K key, V value) {
            Queue<V> queue;
            if (!_data.TryGetValue(key, out queue)) {
                _data.Add(key, queue = new Queue<V>());
            }
            queue.Enqueue(value);
        }

        internal V Dequeue(K key) {
            Queue<V> queue;
            if (!_data.TryGetValue(key, out queue)) {
                throw Error.QueueEmpty();
            }
            V result = queue.Dequeue();
            if (queue.Count == 0) {
                _data.Remove(key);
            }
            return result;
        }

        internal bool TryDequeue(K key, out V value) {
            Queue<V> queue;
            if (_data.TryGetValue(key, out queue) && queue.Count > 0) {
                value = queue.Dequeue();
                if (queue.Count == 0) {
                    _data.Remove(key);
                }
                return true;
            }
            value = default(V);
            return false;
        }

        internal V Peek(K key) {
            Queue<V> queue;
            if (!_data.TryGetValue(key, out queue)) {
                throw Error.QueueEmpty();
            }
            return queue.Peek();
        }

        internal int GetCount(K key) {
            Queue<V> queue;
            if (!_data.TryGetValue(key, out queue)) {
                return 0;
            }
            return queue.Count;
        }

        internal void Clear() {
            _data.Clear();
        }
    }
}
