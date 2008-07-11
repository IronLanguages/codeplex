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

#if !SILVERLIGHT // ComObject

using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Microsoft.Scripting.Actions.ComDispatch {

    internal sealed class ReturnBuilder {
        private readonly Type/*!*/ _returnType;

        /// <summary>
        /// Creates a ReturnBuilder
        /// </summary>
        /// <param name="returnType">the type the ReturnBuilder will leave on the stack</param>
        internal ReturnBuilder(Type/*!*/ returnType) {
            Debug.Assert(returnType != null);

            _returnType = returnType; 
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        internal Expression ToExpression(Expression ret) {
            return ret;
        }

        internal Type/*!*/ ReturnType {
            get {
                return _returnType;
            }
        }
    }
}

#endif
