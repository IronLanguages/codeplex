/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System.Dynamic;
using Microsoft.Scripting.ComInterop;

namespace SiteTest.Actions {
    class TestSetMemberBinder : SetMemberBinder {
        internal TestSetMemberBinder(string name)
            : base(name, false) {
        }

        internal TestSetMemberBinder(string name, bool ignoreCase)
            : base(name, ignoreCase) {
        }

        public override DynamicMetaObject FallbackSetMember(DynamicMetaObject self, DynamicMetaObject value, DynamicMetaObject onBindingError) {
            DynamicMetaObject com;
            if (ComBinder.TryBindSetMember(this, self, value, out com))
                return com;

            return onBindingError ?? new DynamicMetaObject(
                Expression.Throw(Expression.New(typeof(BindingException)), typeof(object)),
                self.Restrictions.Merge(BindingRestrictions.GetTypeRestriction(self.Expression, self.LimitType))
                .Merge(value.Restrictions)
            );
        }
    }
}
