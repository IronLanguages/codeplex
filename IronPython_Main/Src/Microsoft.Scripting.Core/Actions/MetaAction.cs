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

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Linq.Expressions;
using System.Scripting.Generation;
using System.Scripting.Utils;

#if !SILVERLIGHT
using ComMetaObject = System.Scripting.Com.ComMetaObject;
#endif

namespace System.Scripting.Actions {
    // TODO: Rename!!!
    public abstract class MetaAction : CallSiteBinder {
        public sealed override Rule<T> Bind<T>(object[] args) {
            if (!typeof(Delegate).IsAssignableFrom(typeof(T))) {
                throw Error.TypeParameterIsNotDelegate(typeof(T));
            }

            ParameterInfo[] pis = typeof(T).GetMethod("Invoke").GetParameters();

            if (pis.Length == 0 || pis[0].ParameterType != typeof(CallSite)) {
                throw Error.FirstArgumentMustBeCallSite();
            }

            ParameterExpression[] pes;
            ParameterExpression siteExpr = Expression.Parameter(typeof(CallSite), "callSite");
            Expression[] expressions = MakeParameters(pis, out pes);

            MetaObject[] mos = new MetaObject[args.Length];
            for (int i = 0; i < mos.Length; i++) {
                object arg = args[i];
                IDynamicObject ido = arg as IDynamicObject;
                if (ido != null) {
                    mos[i] = ido.GetMetaObject(expressions[i]);
#if !SILVERLIGHT
                } else if (ComMetaObject.IsGenericComObject(arg)) {
                    mos[i] = ComMetaObject.GetComMetaObject(expressions[i], arg);
#endif
                } else {
                    mos[i] = new ParameterMetaObject(expressions[i], arg);
                }
            }

            MetaObject binding = Bind(mos);
            if (binding == null) {
                throw Error.BindingCannotBeNull();
            }

            return new Rule<T>(
                Expression.Scope(
                    GetMetaObjectRule(binding, GetReturnType(typeof(T)), siteExpr),
                    "<rule>"
                ),
                null,
                new ReadOnlyCollection<ParameterExpression>(ArrayUtils.Insert(siteExpr, pes))
            );
        }

        private Expression AddReturn(Expression body, Type retType) {
            switch (body.NodeType) {
                case ExpressionType.Scope:
                    ScopeExpression se = (ScopeExpression)body;
                    return Expression.Scope(
                        AddReturn(se.Body, retType),
                        se.Variables
                    );
                case ExpressionType.Conditional:
                    ConditionalExpression conditional = (ConditionalExpression)body;
                    if (IsDeferExpression(conditional.IfTrue)) {
                        return Expression.Condition(
                            Expression.Not(conditional.Test),
                            Expression.Return(Expression.ConvertHelper(conditional.IfFalse, retType)),
                            Expression.Empty()
                        );
                    } else if (IsDeferExpression(conditional.IfFalse)) {
                        return Expression.Condition(
                            conditional.Test,
                            Expression.Return(Expression.ConvertHelper(conditional.IfTrue, retType)),
                            Expression.Empty()
                        );
                    }
                    return Expression.Condition(
                        conditional.Test,
                        AddReturn(conditional.IfTrue, retType),
                        AddReturn(conditional.IfFalse, retType)
                    );
                case ExpressionType.ThrowStatement:
                    return body;
                default:
                    return Expression.Return(Expression.ConvertHelper(body, retType));
            }
        }

        private bool IsDeferExpression(Expression e) {
            if (e.BindingInfo == this) {
                return true;
            }

            if (e.NodeType == ExpressionType.Convert) {
                return IsDeferExpression(((UnaryExpression)e).Operand);
            }

            return false;
        }

        private static Type GetReturnType(Type dlgType) {
            return dlgType.GetMethod("Invoke").ReturnType;
        }

        private Expression GetMetaObjectRule(MetaObject binding, Type retType, Expression siteExpr) {
            Debug.Assert(binding != null);

            ActionSelfRewriter rewriter = new ActionSelfRewriter(
                Expression.Property(
                    siteExpr,
                    typeof(CallSite).GetProperty("Binder")
                )
            );

            Expression body = rewriter.VisitNode(AddReturn(binding.Expression, retType));

            if (binding.Restrictions != Restrictions.Empty) {
                // add the test only if we have one
                body = Expression.Condition(
                    binding.Restrictions.CreateTest(),
                    body,
                    Expression.Empty()
                );
            }

            return body;
        }

        public abstract MetaObject Bind(MetaObject[] args);

        public MetaObject Defer(MetaObject[] args) {
            return new MetaObject(
                Expression.ActionExpression(
                    this,
                    typeof(object),   // !! what's the correct return type?
                    MetaObject.GetExpressions(args)
                ),
                Restrictions.Combine(args)
            );
        }

        private static Expression[] MakeParameters(ParameterInfo[] pis, out ParameterExpression[] parameters) {
            // First argument is the dynamic site
            const int FirstParameterIndex = 1;

            Expression[] all = new Expression[pis.Length - FirstParameterIndex];
            ParameterExpression[] vars = new ParameterExpression[pis.Length - FirstParameterIndex];

            for (int i = FirstParameterIndex; i < pis.Length; i++) {
                int index = i - FirstParameterIndex;
                all[index] = vars[index] = Expression.Parameter(pis[i].ParameterType, "$arg" + index);
            }

            parameters = vars;
            return all;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public static Expression GetSelfExpression() {
            return new ActionSelfExpression();
        }
    }
}
