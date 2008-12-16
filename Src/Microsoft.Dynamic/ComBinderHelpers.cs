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

using System.Diagnostics;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using Microsoft.Linq.Expressions;
using Microsoft.Linq.Expressions.Compiler;
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;

using System.Runtime.InteropServices;

namespace Microsoft.Scripting {
    internal static class ComBinderHelpers {

        internal static bool PreferPut(Type type) {
            Debug.Assert(type != null);

            if (type.IsValueType || type.IsArray) return true;

            if (type == typeof(String) ||
                type == typeof(DBNull) ||
                type == typeof(DynamicNull) ||
                type == typeof(System.Reflection.Missing) ||
                type == typeof(CurrencyWrapper)) {

                return true;
            } else {
                return false;
            }
        }

        internal static bool IsByRef(DynamicMetaObject mo) {
            ParameterExpression pe = mo.Expression as ParameterExpression;
            return pe != null && pe.IsByRef;
        }

        internal static bool IsStrongBoxArg(DynamicMetaObject o) {
            if (IsByRef(o)) {
                return false;
            }

            Type t = o.LimitType;
            return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(StrongBox<>);
        }

        internal static DynamicMetaObject RewriteStrongBoxAsRef(CallSiteBinder action, DynamicMetaObject target, DynamicMetaObject[] args, bool hasRhs) {
            Debug.Assert(action != null && target != null && args != null);

            var restrictions = target.Restrictions.Merge(BindingRestrictions.Combine(args));

            Expression[] argExpressions = new Expression[args.Length + 1];
            Type[] signatureTypes = new Type[args.Length + 3]; // args + CallSite, target, returnType

            signatureTypes[0] = typeof(CallSite);

            //we are not restricting on target type here.
            argExpressions[0] = target.Expression;
            signatureTypes[1] = target.Expression.Type;

            int argsToProcess = args.Length;

            if (hasRhs) {
                DynamicMetaObject rhsArgument = args[args.Length - 1];
                argExpressions[args.Length] = rhsArgument.Expression;
                signatureTypes[args.Length + 1] = rhsArgument.Expression.Type;
                argsToProcess--;
            }

            for (int i = 0; i < argsToProcess; i++) {
                DynamicMetaObject currArgument = args[i];
                if (IsStrongBoxArg(currArgument)) {
                    restrictions = restrictions.Merge(BindingRestrictions.GetTypeRestriction(currArgument.Expression, currArgument.LimitType));

                    // we have restricted this argument to LimitType so we can convert and conversion will be trivial cast.
                    Expression boxedValueAccessor = Expression.Field(
                        Helpers.Convert(
                            currArgument.Expression,
                            currArgument.LimitType
                        ),
                        currArgument.LimitType.GetField("Value")
                    );

                    argExpressions[i + 1] = boxedValueAccessor;
                    signatureTypes[i + 2] = boxedValueAccessor.Type.MakeByRefType();
                } else {
                    argExpressions[i + 1] = currArgument.Expression;
                    signatureTypes[i + 2] = currArgument.Expression.Type;
                }
            }
            
            // Last signatureType is the return value
            signatureTypes[signatureTypes.Length - 1] = typeof(object);

            return new DynamicMetaObject(
                Expression.MakeDynamic(
                    Expression.GetDelegateType(signatureTypes),
                    action,
                    argExpressions
                ),
                restrictions
            );
        }
    }
}

#endif
