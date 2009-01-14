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


using Microsoft.Linq.Expressions;

namespace Microsoft.Scripting {
    /// <summary>
    /// Represents a dynamic object, that can have its operations bound at runtime.
    /// </summary>
    /// <remarks>
    /// Objects that want to participate in the binding process should implement an IDynamicObject interface,
    /// and implement <see cref="IDynamicObject.GetMetaObject" /> to return a <see cref="DynamicMetaObject" />.
    /// </remarks>
    public interface IDynamicObject {
        /// <summary>
        /// Returns the <see cref="DynamicMetaObject" /> responsible for binding operations performed on this object.
        /// </summary>
        /// <param name="parameter">The expression tree representation of the runtime value.</param>
        /// <returns>The <see cref="DynamicMetaObject" /> to bind this object.</returns>
        DynamicMetaObject GetMetaObject(Expression parameter);
    }
}
