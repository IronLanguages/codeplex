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

#if !SILVERLIGHT3
#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System;
using System.Collections;
#if SILVERLIGHT
using System.Collections.Generic;
#endif
using EU = ETUtils.ExpressionUtils;

namespace AstTest {
    public static partial class Scenarios {
        public static void Helper_Positive_TryTypeUnification(int n) {
            switch (n) {
                case 0:
                    throw new DivideByZeroException();
                case 1:
                    throw new OverflowException();
                case 2:
                    throw new IndexOutOfRangeException();
                case 3:
                    throw new ArgumentNullException();
                case 4:
                    throw new InvalidOperationException();
                default:
                    break;
            }
        }

        public static void Positive_TryTypeUnification(EU.IValidator V) {
#if SILVERLIGHT
            var ht = new Dictionary<String, int>();
#else
            var ht = new Hashtable();
#endif
            ht["Hi"] = 10;
            ht["Hello"] = 20;

            ParameterExpression n = Expression.Parameter(typeof(int), "n");
            var lambda = Expression.Lambda<Func<int, IEnumerable>>(
                Expression.MakeTry(
                    typeof(IEnumerable),
                    Expression.Block(
                        Expression.Call(typeof(Scenarios).GetMethod("Helper_Positive_TryTypeUnification"), n),
#if SILVERLIGHT
                        Expression.Constant(new Queue<Object>(new object[] { 10, "Hi", 3.5 }))
#else
                        Expression.Constant(new Queue(new object[] { 10, "Hi", 3.5 }))
#endif
                    ),
                    null,
                    null,
                    new[] {
                        Expression.Catch(
                            typeof(DivideByZeroException),
                            Expression.Constant("Hello World")
                        ),
                        Expression.Catch(
                            typeof(OverflowException),
                            Expression.Constant(new[] { 10, 20, 30, 40 })
                        ),
                        Expression.Catch(
                            typeof(IndexOutOfRangeException),
#if SILVERLIGHT
                            Expression.Constant(new List<string>(new [] { "Hello", "World" }))
#else
                            Expression.Constant(new ArrayList(new [] { "Hello", "World" }))
#endif
                        ),
                        Expression.Catch(
                            typeof(ArgumentNullException),
                            Expression.Constant(ht)
                        ),
                        Expression.Catch(
                            typeof(InvalidOperationException),
                            Expression.Constant(new BitArray(new[] { true, false, true, true, false, true, false, false, false, true }))
                        )
                    }
                ),
                n
            );

            V.Validate(lambda, d =>
            {
                for (int i = 0; i < 5; i++) {
                    foreach (var o in d(i)) {
                        EU.Equal(o != null, true);
                    }
                }
            });
        }
    }
}
#endif
