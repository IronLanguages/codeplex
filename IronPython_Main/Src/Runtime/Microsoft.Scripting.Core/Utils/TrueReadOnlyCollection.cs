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
using Microsoft.Scripting.Utils;

namespace Microsoft.Runtime.CompilerServices {
    sealed class TrueReadOnlyCollection<T> : ReadOnlyCollection<T> {
        /// <summary>
        /// Creates instnace of TrueReadOnlyCollection, wrapping passed in array.
        /// !!! DOES NOT COPY THE ARRAY !!!
        /// </summary>
        internal TrueReadOnlyCollection(T[] list)
            : base(list) {
        }
    }
}
