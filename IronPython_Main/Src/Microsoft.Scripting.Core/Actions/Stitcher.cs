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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Linq.Expressions;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Actions {

    /// <summary>
    /// Stitcher, a.k.a Rule inliner. Takes list of rules and produces LambdaExpression with the
    /// rules inlined, adding the "update" call at the very end.
    /// </summary>
    internal sealed class Stitcher : ExpressionTreeVisitor {
        /// <summary>
        /// LambdaExpression parameters
        /// </summary>
        private readonly ParameterExpression[] _lp;

        /// <summary>
        /// Mapping of the ParameterExpression (rule) -> ParameterExpression (lambda)
        /// </summary>
        private readonly Dictionary<ParameterExpression, ParameterExpression> _map = new Dictionary<ParameterExpression, ParameterExpression>();

        private Stitcher(ParameterExpression[] lp) {
            _lp = lp;
        }

        internal static Expression<T> Stitch<T>(Rule<T>[] rules) where T : class {
            Type targetType = typeof(T);
            Type siteType = typeof(CallSite<T>);

            MethodInfo invoke = targetType.GetMethod("Invoke");
            ParameterInfo[] parameters = invoke.GetParameters();

            int length = rules.Length;

            ParameterExpression[] lp;
            Expression[] body = new Expression[length + 1];

            if (length == 1) {
                // only one rule, use its parameters and don't stitch..
                Rule<T> rule = rules[0];
                lp = rule.Parameters.AddFirst(Expression.Parameter(typeof(CallSite), "site"));
                body[0] = rule.Binding;
            } else {
                // Many rules, stitch them together on a new set of parameters
                lp = MakeParameters(parameters);
                Stitcher stitch = new Stitcher(lp);

                for (int i = 0; i < rules.Length; i++) {
                    Rule<T> rule = rules[i];
                    body[i] = stitch.StitchRule(rule.Parameters, rule.Binding);
                }
            }

            body[rules.Length] = Expression.Return(
                Expression.Call(
                    Expression.Field(
                        Expression.Convert(lp[0], siteType),
                        siteType.GetField("Update")
                    ),
                    invoke,
                    lp
                )
            );

            return new Expression<T>(
                Annotations.Empty,
                ExpressionType.Lambda,
                "_stub_",
                Expression.Block(body),
                new ReadOnlyCollection<ParameterExpression>(lp)
            );
        }

        private Expression StitchRule(ReadOnlyCollection<ParameterExpression> parameters, Expression expression) {
            _map.Clear();
            for (int i = 0; i < parameters.Count; i++) {
                Debug.Assert(parameters[i].Type == _lp[i + 1].Type);
                _map[parameters[i]] = _lp[i + 1];
            }
            return Visit(expression);
        }

        private static ParameterExpression[] MakeParameters(ParameterInfo[] pis) {
            ParameterExpression[] vars = new ParameterExpression[pis.Length];

            for (int i = 0; i < pis.Length; i++) {
                vars[i] = Expression.Parameter(pis[i].ParameterType, "$arg" + i);
            }

            return vars;
        }

        protected internal override Expression VisitParameter(ParameterExpression node) {
            return _map[node];
        }
    }
}
