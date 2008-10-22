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

namespace Microsoft.Scripting.Actions {
    /// <summary>
    /// A meta object who's exact type is known and additional calls to Restrict
    /// do nothing.
    /// </summary>
    public class RestrictedMetaObject : MetaObject {
        public RestrictedMetaObject(Expression expr, Restrictions rest)
            : base(expr, rest) {
        }

        public RestrictedMetaObject(Expression expr, Restrictions rest, object value)
            : base(expr, rest, value) {
        }

        public override MetaObject Restrict(Type type) {
            return this;
        }
    }
}
