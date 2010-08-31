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

using System;
using System.Collections.Generic;
using System.Dynamic;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.ComInterop;

namespace SiteTest.Actions {
    class TestGetMemberBinder : GetMemberBinder {
        internal TestGetMemberBinder(string name)
            : base(name, false) {
        }

        internal TestGetMemberBinder(string name, bool ignoreCase)
            : base(name, ignoreCase) {
        }

        private int _fakeId = 0;
        private int[] _fakeData;

        internal TestGetMemberBinder(string name, bool ignoreCase, int fakeId)
            : base(name, ignoreCase) {
            _fakeId = fakeId;
            // why don't we grab some RAM.
            _fakeData = new int[100000];
        }

        public override int GetHashCode() {
            return base.GetHashCode() ^ _fakeId;
        }

        public override bool Equals(object obj) {
            TestGetMemberBinder other = obj as TestGetMemberBinder;
            return base.Equals(other) && _fakeId == other._fakeId;
        }



        public override DynamicMetaObject FallbackGetMember(DynamicMetaObject self, DynamicMetaObject onBindingError) {
            DynamicMetaObject com;

            if (ComBinder.TryBindGetMember(this, self, out com, true))
                return com;

            //The language implementation to get a special member
            if (String.Compare(Name, "__code__", IgnoreCase) == 0) {
                return new DynamicMetaObject(
                    Expression.Constant("123"),
                    self.Restrictions.Merge(BindingRestrictions.GetTypeRestriction(self.Expression, self.LimitType))
                );
            }

            //The language implementation to get a special member
            if (String.Compare(Name, "pLong", IgnoreCase) == 0) {
                return new DynamicMetaObject(
                    Expression.Constant("777"),
                    self.Restrictions.Merge(BindingRestrictions.GetTypeRestriction(self.Expression, self.LimitType))
                );
            }

            return onBindingError ?? new DynamicMetaObject(
                Expression.Throw(Expression.New(typeof(BindingException)), typeof(object)),
                self.Restrictions.Merge(BindingRestrictions.GetTypeRestriction(self.Expression, self.LimitType))
            );
        }
    }

    public class BindingException : Exception { //@TODO - Pull this out and fill it in
        public BindingException()
            : base() {
        }

        public BindingException(string msg)
            : base(msg) {
        }
    }
}
