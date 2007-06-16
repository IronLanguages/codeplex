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

namespace Microsoft.Scripting.Generation {
    /// <summary>
    /// A stack implemented as a list.  Allows both Push/Pop access and indexing into any
    /// member of the list.
    /// </summary>
    class ListStack<T> : List<T> {
        public void Push(T val) {
            Add(val);
        }

        public T Peek() {
            if (Count == 0)
                throw new InvalidOperationException();

            return this[Count - 1];
        }

        public T Pop() {
            if (Count == 0)
                throw new InvalidOperationException();

            T res = this[Count - 1];
            RemoveAt(Count - 1);
            return res;
        }
    }

}
