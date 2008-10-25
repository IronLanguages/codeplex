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
using Microsoft.Linq.Expressions.Compiler;

#if !SILVERLIGHT
using ComMetaObject = Microsoft.Scripting.ComInterop.ComMetaObject;
#endif

namespace Microsoft.Scripting.Actions {
    public abstract class MetaObjectBinder : CallSiteBinder {

        #region Public APIs

        public sealed override Expression Bind(object[] args, ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel) {
            if (args.Length == 0) {
                throw new InvalidOperationException();
            }

            MetaObject[] mos;
            if (args.Length != 1) {
                mos = new MetaObject[args.Length - 1];
                for (int i = 1; i < args.Length; i++) {
                    mos[i - 1] = MetaObject.ObjectToMetaObject(args[i], parameters[i]);
                }
            } else {
                mos = MetaObject.EmptyMetaObjects;
            }

            MetaObject binding = Bind(
                MetaObject.ObjectToMetaObject(args[0], parameters[0]),
                mos
            );

            if (binding == null) {
                throw Error.BindingCannotBeNull();
            }

            return GetMetaObjectRule(binding, returnLabel);
        }

        public abstract MetaObject Bind(MetaObject target, MetaObject[] args);

        public MetaObject Defer(MetaObject target, params MetaObject[] args) {
            ContractUtils.RequiresNotNull(target, "target");

            if (args == null) {
                return MakeDeferred(
                        target.Restrictions,
                        target
                );
            } else {
                return MakeDeferred(
                        target.Restrictions.Merge(Restrictions.Combine(args)),
                        args.AddFirst(target)
                );
            }
        }

        public MetaObject Defer(params MetaObject[] args) {
            return MakeDeferred(
                Restrictions.Combine(args),
                args
            );
        }

        private MetaObject MakeDeferred(Restrictions rs, params MetaObject[] args) {
            var exprs = MetaObject.GetExpressions(args);

            // TODO: we should really be using the same delegate as the CallSite
            Type delegateType = DelegateHelpers.MakeDeferredSiteDelegate(args, typeof(object));

            return new MetaObject(
                Expression.MakeDynamic(
                    delegateType,
                    this,
                    exprs
                ),
                rs
            );
        }

        #endregion

        private Expression AddReturn(Expression body, LabelTarget @return) {
            switch (body.NodeType) {
                case ExpressionType.Conditional:
                    ConditionalExpression conditional = (ConditionalExpression)body;
                    if (IsDeferExpression(conditional.IfTrue)) {
                        return Expression.Condition(
                            Expression.Not(conditional.Test),
                            Expression.Return(@return, Expression.ConvertHelper(conditional.IfFalse, @return.Type)),
                            Expression.Empty()
                        );
                    } else if (IsDeferExpression(conditional.IfFalse)) {
                        return Expression.Condition(
                            conditional.Test,
                            Expression.Return(@return, Expression.ConvertHelper(conditional.IfTrue, @return.Type)),
                            Expression.Empty()
                        );
                    }
                    return Expression.Condition(
                        conditional.Test,
                        AddReturn(conditional.IfTrue, @return),
                        AddReturn(conditional.IfFalse, @return)
                    );
                case ExpressionType.Throw:
                    return body;
                case ExpressionType.Block:
                    // block could have a throw which we need to run through to avoid 
                    // trying to convert it
                    BlockExpression block = (BlockExpression)body;
                    if (block.Expressions.Count > 0) {
                        Expression[] nodes = new Expression[block.Expressions.Count];
                        for (int i = 0; i < nodes.Length - 1; i++) {
                            nodes[i] = block.Expressions[i];
                        }
                        nodes[nodes.Length - 1] = AddReturn(block.Expressions[block.Expressions.Count - 1], @return);

                        if (block.Type == typeof(void)) {
                            return Expression.Block(block.Variables, nodes);
                        } else {
                            return Expression.Comma(block.Variables, nodes);
                        }
                    }

                    goto default;
                default:
                    return Expression.Return(@return, Expression.ConvertHelper(body, @return.Type));
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

        private Expression GetMetaObjectRule(MetaObject binding, LabelTarget @return) {
            Debug.Assert(binding != null);

            Expression body = AddReturn(binding.Expression, @return);

            if (binding.Restrictions != Restrictions.Empty) {
                // add the test only if we have one
                body = Expression.Condition(
                    binding.Restrictions.ToExpression(),
                    body,
                    Expression.Empty()
                );
            }

            return body;
        }
    }
}
