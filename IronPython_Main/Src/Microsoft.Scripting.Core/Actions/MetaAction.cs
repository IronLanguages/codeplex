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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Linq.Expressions;
using Microsoft.Scripting.Utils;

#if !SILVERLIGHT
using ComMetaObject = Microsoft.Scripting.Com.ComMetaObject;
#endif

namespace Microsoft.Scripting.Actions {
    // TODO: Rename!!!
    public abstract class MetaAction : CallSiteBinder {

        #region Public APIs

        public sealed override Rule<T> Bind<T>(object[] args) {
            if (!typeof(Delegate).IsAssignableFrom(typeof(T))) {
                throw Error.TypeParameterIsNotDelegate(typeof(T));
            }

            ParameterInfo[] pis = typeof(T).GetMethod("Invoke").GetParameters();

            if (pis.Length == 0 || pis[0].ParameterType != typeof(CallSite)) {
                throw Error.FirstArgumentMustBeCallSite();
            }

            ParameterExpression[] pes;
            Expression[] expressions = MakeParameters(pis, out pes);


            if (args.Length == 0) {
                throw new InvalidOperationException();
            }

            MetaObject[] mos;
            if (args.Length != 1) {
                mos = new MetaObject[args.Length - 1];
                for (int i = 1; i < args.Length; i++) {
                    mos[i - 1] = MetaObject.ObjectToMetaObject(args[i], expressions[i]);
                }
            } else {
                mos = MetaObject.EmptyMetaObjects;
            }

            MetaObject binding = Bind(
                MetaObject.ObjectToMetaObject(args[0], expressions[0]), 
                mos
            );
            
            if (binding == null) {
                throw Error.BindingCannotBeNull();
            }

            return new Rule<T>(
                Expression.Scope(
                    GetMetaObjectRule(binding, GetReturnType(typeof(T))),
                    "<rule>"
                ),
                new ReadOnlyCollection<ParameterExpression>(pes)
            );
        }

        public abstract MetaObject Bind(MetaObject target, MetaObject[] args);

        public MetaObject Defer(params MetaObject[] args) {
            var exprs = MetaObject.GetExpressions(args);

            // TODO: we should really be using the same delegate as the CallSite
            return new MetaObject(
                Expression.Dynamic(
                    this,
                    typeof(object),   // !! what's the correct return type?
                    exprs
                ),
                Restrictions.Combine(args)
            );
        }

        #endregion

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
                case ExpressionType.Block:
                    // block could have a throw which we need to run through to avoid 
                    // trying to convert it
                    Block block = (Block)body;
                    if (block.Expressions.Count > 0) {
                        Expression[] nodes = new Expression[block.Expressions.Count];
                        for (int i = 0; i < nodes.Length - 1; i++) {
                            nodes[i] = block.Expressions[i];
                        }
                        nodes[nodes.Length - 1] = AddReturn(block.Expressions[block.Expressions.Count - 1], retType);

                        if (block.Type == typeof(void)) {
                            return Expression.Block(nodes);
                        } else {
                            return Expression.Comma(nodes);
                        }
                    }

                    goto default;
                default:
                    return Expression.Return(Expression.ConvertHelper(body, retType));
            }
        }

        private bool IsDeferExpression(Expression e) {
            if (e.NodeType == ExpressionType.Dynamic) {
                return ((DynamicExpression)e).Binder == this;
            }

            if (e.NodeType == ExpressionType.Convert) {
                return IsDeferExpression(((UnaryExpression)e).Operand);
            }

            return false;
        }

        private static Type GetReturnType(Type dlgType) {
            return dlgType.GetMethod("Invoke").ReturnType;
        }

        private Expression GetMetaObjectRule(MetaObject binding, Type retType) {
            Debug.Assert(binding != null);

            Expression body = AddReturn(binding.Expression, retType);

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
    }
}
