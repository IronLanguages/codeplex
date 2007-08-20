/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Scripting.Utils {
    public static class Contract {

        public static void Requires(bool precondition) {
            if (!precondition) {
                throw new ArgumentException();
            }
        }

        public static void Requires(bool precondition, string paramName, string message) {
            if (!precondition) {
                throw new ArgumentException(message, paramName);
            }
        }

        public static void RequiresNotNull(object value, string paramName) {
            if (value == null) {
                throw new ArgumentNullException(paramName);
            }
        }

        public static void RequiresInRange<T>(IList<T> array, int offset, int count, string arrayName, string offsetName, string countName) {
            RequiresNotNull(arrayName, "arrayName");
            RequiresNotNull(offsetName, "offsetName");
            RequiresNotNull(countName, "countName");

            RequiresNotNull(array, arrayName);
            if (offset < 0 || offset > array.Count) throw new ArgumentOutOfRangeException(offsetName);
            if (count < 0 || count > array.Count - offset) throw new ArgumentOutOfRangeException(countName);
        }

        public static void RequiresNonNullItems<T>(IList<T> array, string arrayName) {
            RequiresNotNull(arrayName, "arrayName");
            RequiresNotNull(array, arrayName);

            for (int i = 0; i < array.Count; i++) {
                if (array[i] == null) {
                    throw ExceptionUtils.MakeArgumentItemNullException(i, arrayName);
                }
            }
        }
    }
}
