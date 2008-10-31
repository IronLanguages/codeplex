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
#if !SILVERLIGHT

using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Linq.Expressions;
using Microsoft.Linq.Expressions.Compiler;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;
using ComTypes = System.Runtime.InteropServices.ComTypes;

namespace Microsoft.Scripting.ComInterop {
    internal static class ComBinderHelpers {
        internal static bool IsStrongBoxArg(MetaObject o) {
            if (o.IsByRef) return false;

            Type t = o.LimitType;
            return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(StrongBox<>);
        }

        internal static MetaObject RewriteStrongBoxAsRef(CallSiteBinder action, MetaObject target, MetaObject[] args) {
            Debug.Assert(action != null && target != null && args != null);

            var restrictions = target.Restrictions.Merge(Restrictions.Combine(args));

            Expression[] argExpressions = new Expression[args.Length + 1];
            Type[] signatureTypes = new Type[args.Length + 1];

            //TODO: we are not restricting on target type here, but in theory we could. 
            //It is a tradeoff between rule reuse and higher polymorphism of the site. 
            argExpressions[0] = target.Expression;
            signatureTypes[0] = target.Expression.Type;

            for (int i = 0; i < args.Length; i++) {
                MetaObject currArgument = args[i];
                if (IsStrongBoxArg(currArgument)) {
                    restrictions = restrictions.Merge(Restrictions.GetTypeRestriction(currArgument.Expression, currArgument.LimitType));

                    // we have restricted this argument to LimitType so we can convert and conversion will be trivial cast.
                    Expression boxedValueAccessor = Expression.Field(
                        Helpers.Convert(
                            currArgument.Expression,
                            currArgument.LimitType
                        ),
                        currArgument.LimitType.GetField("Value")
                    );

                    argExpressions[i + 1] = boxedValueAccessor;
                    signatureTypes[i + 1] = boxedValueAccessor.Type.MakeByRefType();
                } else {
                    argExpressions[i + 1] = currArgument.Expression;
                    signatureTypes[i + 1] = currArgument.Expression.Type;
                }
            }

            // TODO: we should really be using the same delegate as the CallSite
            Type delegateType = DelegateHelpers.MakeDeferredSiteDelegate(signatureTypes, typeof(object));

            return new MetaObject(
                Expression.MakeDynamic(
                    delegateType,
                    action,
                    argExpressions
                ),
                restrictions
            );
        }
    }
}

#endif
