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

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Linq.Expressions {

    // Utility extension methods
    internal static class Extensions {
        /// <summary>
        /// Wraps the provided enumerable into a ReadOnlyCollection{T}
        /// 
        /// Copies all of the data into a new array, so the data can't be
        /// changed after creation. The exception is if the enumerable is
        /// already a ReadOnlyCollection{T}, in which case we just return it.
        /// </summary>
        internal static ReadOnlyCollection<T> ToReadOnly<T>(this IEnumerable<T> enumerable) {
            if (enumerable == null) {
                return DefaultReadOnlyCollection<T>.Empty;
            }

            var roc = enumerable as ReadOnlyCollection<T>;
            if (roc != null) {
                return roc;
            }

            var collection = enumerable as ICollection<T>;
            if (collection != null) {
                int count = collection.Count;
                if (count == 0) {
                    return DefaultReadOnlyCollection<T>.Empty;
                }

                T[] array = new T[count];
                collection.CopyTo(array, 0);
                return new ReadOnlyCollection<T>(array);
            }

            // ToArray trims the excess space and speeds up access
            return new ReadOnlyCollection<T>(new List<T>(enumerable).ToArray());
        }
    }

    internal static class DefaultReadOnlyCollection<T> {
        internal static ReadOnlyCollection<T> Empty = new ReadOnlyCollection<T>(new T[0]);
    }
}
