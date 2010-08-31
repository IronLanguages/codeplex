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

using System;
using System.Collections.ObjectModel;
using System.Dynamic.Binders;
using System.Linq.Expressions;
using System.Reflection;

namespace AstTest {
    public static partial class Scenarios {
        
        private class RefCallBinder : CallAction {
            public RefCallBinder()
                : base("CallByRef", false) {
            }

            public override MetaObject Fallback(MetaObject target, MetaObject[] args, MetaObject onBindingError) {
                return new MetaObject(
                    Expression.Call(
                        typeof(Scenarios).GetMethod(Name),
                        target.Expression,
                        args[0].Expression,
                        args[1].Expression
                    ),
                    Restrictions.Combine(args)
                );
            }

            public override MetaObject FallbackInvoke(MetaObject[] args, MetaObject onBindingError) {
                return new MetaObject(
                    Expression.Throw(
                        Expression.New(
                            typeof(InvalidOperationException).GetConstructor(new Type[] { typeof(string) }),
                            Expression.Constant("Cannot invoke.")
                        )
                    ),
                    Restrictions.Combine(args)
                );
            }

            public override object HashCookie {
                get { return this; }
            }
        }

        public static void Positive_BinderWithRefSite() {
            var site = CallSite<CallByRefDelegate>.Create(new RefCallBinder());
            int a = 0;
            string b = null;
            double c = 0.0;

            int result = site.Target(site, ref a, ref b, out c);

            Utils.Equal(a, 17);
            Utils.Equal(b, "Called");
            Utils.Equal(c, Math.PI);
            Utils.Equal(result, 7);
        }
    }
}
