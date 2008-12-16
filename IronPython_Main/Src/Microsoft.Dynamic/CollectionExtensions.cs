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
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Microsoft.Scripting {
    internal static class CollectionExtensions { 

        // Name needs to be different so it doesn't conflict with Enumerable.Select
        internal static U[] Map<T, U>(this ICollection<T> collection, Func<T, U> select) {
            int count = collection.Count;
            U[] result = new U[count];
            count = 0;
            foreach (T t in collection) {
                result[count++] = select(t);
            }
            return result;
        }

        internal static bool Any<T>(this IEnumerable<T> source, Func<T, bool> predicate) {
            foreach (T element in source) {
                if (predicate(element)) {
                    return true;
                }
            }
            return false;
        }

        internal static T[] RemoveFirst<T>(this T[] array) {
            T[] result = new T[array.Length - 1];
            Array.Copy(array, 1, result, 0, result.Length);
            return result;
        }

        internal static T[] AddFirst<T>(this IList<T> list, T item) {
            T[] res = new T[list.Count + 1];
            res[0] = item;
            list.CopyTo(res, 1);
            return res;
        }

        internal static T[] AddLast<T>(this IList<T> list, T item) {
            T[] res = new T[list.Count + 1];
            list.CopyTo(res, 0);
            res[list.Count] = item;
            return res;
        }
    }
}
