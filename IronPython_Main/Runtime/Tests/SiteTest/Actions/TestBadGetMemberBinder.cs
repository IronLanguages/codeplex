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
    class TestBadGetMemberBinder : GetMemberBinder {
        internal TestBadGetMemberBinder(string name)
            : base(name, false) {
        }

        internal TestBadGetMemberBinder(string name, bool ignoreCase)
            : base(name, ignoreCase) {
        }

        public override DynamicMetaObject FallbackGetMember(DynamicMetaObject self, DynamicMetaObject onBindingError) {
            DynamicMetaObject com;

            if (ComBinder.TryBindGetMember(this, self, out com, true))
                return com;

            //This is what makes this binder "bad".  It supplies insufficient restrictions.
            return onBindingError ?? new DynamicMetaObject(
                Expression.Throw(
                    Expression.New(typeof(BindingException)), typeof(object)
                ),
                BindingRestrictions.Empty
            );
        }
    }
}
