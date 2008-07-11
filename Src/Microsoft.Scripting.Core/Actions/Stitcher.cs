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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Linq.Expressions;

namespace System.Scripting.Actions {

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
                lp = AddCallSiteParameter(parameters, rule.Parameters);
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
            return VisitNode(expression);
        }

        private static ParameterExpression[] AddCallSiteParameter(ParameterInfo[] pis, ReadOnlyCollection<ParameterExpression> expressions) {
            Debug.Assert(pis.Length > 0 && pis.Length - 1 == expressions.Count);

            ParameterExpression[] vars = new ParameterExpression[pis.Length];
            vars[0] = Expression.Parameter(pis[0].ParameterType, "$arg0");

            for (int i = 1; i < vars.Length; i++) {
                Debug.Assert(pis[i].ParameterType.IsByRef
                    ? pis[i].ParameterType.GetElementType() == expressions[i - 1].Type
                    : pis[i].ParameterType == expressions[i - 1].Type);

                vars[i] = expressions[i - 1];
            }

            return vars;
        }

        private static ParameterExpression[] MakeParameters(ParameterInfo[] pis) {
            ParameterExpression[] vars = new ParameterExpression[pis.Length];

            for (int i = 0; i < pis.Length; i++) {
                Type type = pis[i].ParameterType;
                vars[i] = type.IsByRef
                    ? Expression.ByRefParameter(type, "$arg" + i)
                    : Expression.Parameter(type, "$arg" + i);
            }

            return vars;
        }

        protected override Expression Visit(ParameterExpression node) {
            return _map[node];
        }
    }
}
