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


namespace Microsoft.Scripting {
    /// <summary>
    /// Represents the type of a null value.
    /// </summary>
    public sealed class DynamicNull {
        /// <summary>
        /// Represents the singleton type of a null value.
        /// </summary>
        public static readonly Type Type = typeof(DynamicNull);

        /// <summary>
        /// Private constructor is never called since 'null' is the only valid instance.
        /// </summary>
        private DynamicNull() { }
    }
}
