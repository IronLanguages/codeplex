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


using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Linq.Expressions;

namespace Microsoft.Scripting.Utils {

    // TODO: Update with the newest version of the managed contracts stuff
    internal static class ContractUtils {

        internal static void Requires(bool precondition) {
            if (!precondition) {
                throw new ArgumentException(Strings.MethodPreconditionViolated);
            }
        }

        internal static void Requires(bool precondition, string paramName) {
            Assert.NotEmpty(paramName);

            if (!precondition) {
                throw new ArgumentException(Strings.InvalidArgumentValue, paramName);
            }
        }

        internal static void Requires(bool precondition, string paramName, string message) {
            Assert.NotEmpty(paramName);

            if (!precondition) {
                throw new ArgumentException(message, paramName);
            }
        }

        internal static void RequiresNotNull(object value, string paramName) {
            Assert.NotEmpty(paramName);

            if (value == null) {
                throw new ArgumentNullException(paramName);
            }
        }

        internal static void RequiresNotEmpty(string str, string paramName) {
            RequiresNotNull(str, paramName);
            if (str.Length == 0) {
                throw new ArgumentException(Strings.NonEmptyStringRequired, paramName);
            }
        }

        internal static void RequiresNotEmpty<T>(ICollection<T> collection, string paramName) {
            RequiresNotNull(collection, paramName);
            if (collection.Count == 0) {
                throw new ArgumentException(Strings.NonEmptyCollectionRequired, paramName);
            }
        }

        /// <summary>
        /// Requires the specified index to point inside the array.
        /// </summary>
        /// <exception cref="ArgumentNullException">Array is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Index is outside the array.</exception>
        internal static void RequiresArrayIndex<T>(IList<T> array, int index, string indexName) {
            Assert.NotEmpty(indexName);
            Assert.NotNull(array);

            if (index < 0 || index >= array.Count) throw new ArgumentOutOfRangeException(indexName);
        }

        /// <summary>
        /// Requires the specified index to point inside the array or at the end
        /// </summary>
        /// <exception cref="ArgumentNullException">Array is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Index is outside the array.</exception>
        internal static void RequiresArrayInsertIndex<T>(IList<T> array, int index, string indexName) {
            Assert.NotEmpty(indexName);
            Assert.NotNull(array);

            if (index < 0 || index > array.Count) throw new ArgumentOutOfRangeException(indexName);
        }

        /// <summary>
        /// Requires the range [offset, offset + count] to be a subset of [0, array.Count].
        /// </summary>
        /// <exception cref="ArgumentNullException">Array is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Offset or count are out of range.</exception>
        internal static void RequiresArrayRange<T>(IList<T> array, int offset, int count, string offsetName, string countName) {
            Assert.NotEmpty(offsetName);
            Assert.NotEmpty(countName);
            Assert.NotNull(array);

            if (count < 0) throw new ArgumentOutOfRangeException(countName);
            if (offset < 0 || array.Count - offset < count) throw new ArgumentOutOfRangeException(offsetName);
        }

        /// <summary>
        /// Requires the range [offset, offset + count] to be a subset of [0, array.Count].
        /// </summary>
        /// <exception cref="ArgumentNullException">Array is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Offset or count are out of range.</exception>
        internal static void RequiresListRange(IList array, int offset, int count, string offsetName, string countName) {
            Assert.NotEmpty(offsetName);
            Assert.NotEmpty(countName);
            Assert.NotNull(array);

            if (count < 0) throw new ArgumentOutOfRangeException(countName);
            if (offset < 0 || array.Count - offset < count) throw new ArgumentOutOfRangeException(offsetName);
        }

        /// <summary>
        /// Requires the range [offset, offset + count] to be a subset of [0, array.Count].
        /// </summary>
        /// <exception cref="ArgumentNullException">String is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Offset or count are out of range.</exception>
        internal static void RequiresArrayRange(string str, int offset, int count, string offsetName, string countName) {
            Assert.NotEmpty(offsetName);
            Assert.NotEmpty(countName);
            Assert.NotNull(str);

            if (count < 0) throw new ArgumentOutOfRangeException(countName);
            if (offset < 0 || str.Length - offset < count) throw new ArgumentOutOfRangeException(offsetName);
        }

        /// <summary>
        /// Requires the array and all its items to be non-null.
        /// </summary>
        internal static void RequiresNotNullItems<T>(IList<T> array, string arrayName) {
            Assert.NotNull(arrayName);
            RequiresNotNull(array, arrayName);

            for (int i = 0; i < array.Count; i++) {
                if (array[i] == null) {
                    throw new ArgumentNullException(string.Format(System.Globalization.CultureInfo.CurrentCulture, "{0}[{1}]", arrayName, i));
                }
            }
        }
    }
}
