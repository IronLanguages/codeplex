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

namespace Microsoft.Scripting.Utils {

    internal class SimplePublisher<K, V> {
        private class Holder {
            internal V item;
        }

        private readonly Dictionary<K, Holder> dict = new Dictionary<K, Holder>();

        internal V GetOrCreateValue(K key, Function<V> creator) {
            Holder holder;
            lock (dict) {
                bool hasIt = dict.TryGetValue(key, out holder);
                if (!hasIt) {
                    dict[key] = holder = new Holder();
                }
            }
            lock (holder) {
                if (holder.item == null) {
                    holder.item = creator();
                }
                return holder.item;
            }
        }
    }
}

