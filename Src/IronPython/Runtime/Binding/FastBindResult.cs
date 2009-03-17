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
using Microsoft.Scripting;
using Microsoft.Linq.Expressions;

using Microsoft.Scripting.Actions;

using Ast = Microsoft.Linq.Expressions.Expression;
using Microsoft.Scripting.Runtime;
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;


namespace IronPython.Runtime.Binding {
    struct FastBindResult<T> where T : class {
        public readonly T Target;
        public readonly bool ShouldCache;

        public FastBindResult(T target, bool shouldCache) {
            Target = target;
            ShouldCache = shouldCache;
        }

        public FastBindResult(T target) {
            Target = target;
            ShouldCache = false;
        }
    }
}
