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

public static class Utils {

    internal static object CreateComObject(string name) {
        return System.Activator.CreateInstance(System.Type.GetTypeFromProgID(name));
    }

    internal static DynamicMetaObject Error(Type type, string message, BindingRestrictions restrictions) {
        return new DynamicMetaObject(
            Expression.Throw(
                Expression.New(
                    type.GetConstructor(new Type[] { typeof(string) }),
                    Expression.Constant(message)
                )
            ),
            restrictions
        );
    }

    internal static void Equal(object a, object b) {
        if (!object.Equals(a, b)) {
            throw new InvalidOperationException(string.Format("Test failed: expected '{0}' to equal '{1}'", a ?? "<null>", b ?? "<null>"));
        }
    }

    internal static void Throws<T>(Action test) where T : Exception {
        try {
            test();
        } catch (T) {
            return;
        }
        throw new InvalidOperationException(string.Format("Test failed: expected exception '{0}'", typeof(T).Name));
    }

    internal static LambdaExpression Lambda(string name, Expression body) {
        return Expression.Lambda(typeof(Func<Object>), body, name, new ParameterExpression[0]);
    }

};

