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
using System.Text;
using Microsoft.Scripting;
using Microsoft.Linq.Expressions;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Runtime {
    public class RestrictedMetaObject : DynamicMetaObject, IRestrictedMetaObject {
        public RestrictedMetaObject(Expression expression, BindingRestrictions restriction, object value)  : base(expression, restriction, value) {
        }

        public RestrictedMetaObject(Expression expression, BindingRestrictions restriction)
            : base(expression, restriction) {
        }

        #region IRestrictedMetaObject Members

        public DynamicMetaObject Restrict(Type type) {
            if (type == LimitType) {
                return this;
            }

            if (HasValue) {
                return new RestrictedMetaObject(
                    AstUtils.Convert(Expression, type),
                    BindingRestrictionsHelpers.GetRuntimeTypeRestriction(Expression, type),
                    Value
                );
            }

            return new RestrictedMetaObject(
                AstUtils.Convert(Expression, type),
                BindingRestrictionsHelpers.GetRuntimeTypeRestriction(Expression, type)
            );
        }

        #endregion
    }
}
