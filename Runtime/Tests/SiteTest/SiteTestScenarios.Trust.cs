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
using Microsoft.Scripting.Utils;
#endif

using System;
using System.Dynamic;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;

namespace SiteTest {
    partial class SiteTestScenarios {
        [Test("Using ExpandoObject in partial trust")]
        private void Scenario_ExpandoInPartialTrust() {
            var setup = new AppDomainSetup();
            setup.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;
            var permissions = new PermissionSet(PermissionState.None);
            permissions.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
            var domain = AppDomain.CreateDomain("Test", null, setup, permissions);
            domain.DoCallBack(new ExpandoTest().ExpandoPartialTrustTest);
        }
    }

    public class ExpandoTest : MarshalByRefObject {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void ExpandoPartialTrustTest() {
            ExpandoPartialTrustTestWorker();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ExpandoPartialTrustTestWorker() {
            var set = CallSite<Func<CallSite, object, object, object>>.Create(new SetExpandoMemberBinder("Hello"));
            var get = CallSite<Func<CallSite, object, object>>.Create(new GetExpandoMemberBinder("Hello"));
            var exp = new ExpandoObject();

            set.Target(set, exp, "Value");
            var res = (string)get.Target(get, exp);

            if (res != "Value") {
                throw new InvalidOperationException("Calling expando in partial trust failed");
            }
        }

        class SetExpandoMemberBinder : SetMemberBinder {
            public SetExpandoMemberBinder(string name)
                : base(name, false) {
            }

            public override DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value, DynamicMetaObject errorSuggestion) {
                return errorSuggestion ?? new DynamicMetaObject(
                    Expression.Throw(Expression.Constant(new Exception("Fallback binding failed")), typeof(object)),
                    BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType)
                );
            }
        }

        class GetExpandoMemberBinder : GetMemberBinder {
            public GetExpandoMemberBinder(string name)
                : base(name, false) {
            }

            public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion) {
                return errorSuggestion ?? new DynamicMetaObject(
                    Expression.Throw(Expression.Constant(new Exception("Fallback binding failed")), typeof(object)),
                    BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType)
                );
            }
        }
    }
}
