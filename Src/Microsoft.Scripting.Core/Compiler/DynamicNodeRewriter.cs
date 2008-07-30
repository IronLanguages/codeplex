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
using System.Diagnostics;
using System.Reflection;
using System.Scripting.Actions;
using System.Scripting.Utils;

namespace System.Linq.Expressions.Compiler {

    /// <summary>
    /// Walks the DLR tree and reduces dynamic AST nodes
    /// Also reduces extension nodes
    /// 
    /// TODO: copy into Microsoft.Scripting, make internal
    /// </summary>
    public class DynamicNodeRewriter : ExpressionTreeVisitor {

        protected override Expression VisitExtension(Expression node) {
            return VisitNode(node.ReduceToKnown());
        }

        protected override Expression Visit(ActionExpression node) {
            Debug.Assert(node.IsDynamic);

            return RewriteSite(node.BindingInfo, node.Type, VisitNodes(node.Arguments));
        }

        protected override Expression Visit(BinaryExpression node) {
            if (node.IsDynamic) {
                return RewriteSite(node.BindingInfo, node.Type, new Expression[] {
                    VisitNode(node.Left), VisitNode(node.Right)
                });
            }

            return base.Visit(node);
        }

        protected override Expression Visit(UnaryExpression node) {
            if (node.IsDynamic) {
                return RewriteSite(node.BindingInfo, node.Type, new Expression[] { VisitNode(node.Operand) });
            }

            // Do nothing for quoted sub-expressions
            // We can reduce dynamic sites later if it gets compiled
            if (node.NodeType == ExpressionType.Quote) {
                return node;
            }

            return base.Visit(node);
        }

        protected override Expression Visit(InvocationExpression node) {
            if (node.IsDynamic) {
                var e = VisitNode(node.Expression);
                return RewriteSite(node.BindingInfo, node.Type, VisitNodes(node.Arguments).AddFirst(e));
            }

            return base.Visit(node);
        }

        protected override Expression Visit(MethodCallExpression node) {
            if (node.IsDynamic) {
                return RewriteSite(node.BindingInfo, node.Type, VisitNodes(node.Arguments));
            }

            return base.Visit(node);
        }

        protected override Expression Visit(NewExpression node) {
            if (node.IsDynamic) {
                return RewriteSite(node.BindingInfo, node.Type, VisitNodes(node.Arguments));
            }

            return base.Visit(node);
        }

        protected override Expression Visit(DeleteExpression node) {
            Debug.Assert(node.IsDynamic);
            return RewriteSite(node.BindingInfo, node.Type, new Expression[] { VisitNode(node.Expression) });
        }

        protected override Expression Visit(AssignmentExpression node) {
            if (node.IsDynamic) {
                switch (node.Expression.NodeType) {
                    case ExpressionType.ArrayIndex:
                        BinaryExpression arrayIndex = (BinaryExpression)node.Expression;
                        return RewriteSite(node.BindingInfo, node.Type, new Expression[] {
                            VisitNode(arrayIndex.Left), VisitNode(arrayIndex.Right), VisitNode(node.Value)
                        });
                    case ExpressionType.MemberAccess:
                        return RewriteSite(node.BindingInfo, node.Type, new Expression[] {
                            VisitNode(((MemberExpression)node.Expression).Expression), VisitNode(node.Value)
                        });
                }
            }

            return base.Visit(node);
        }

        protected override Expression Visit(MemberExpression node) {
            if (node.IsDynamic) {
                return RewriteSite(node.BindingInfo, node.Type, new Expression[] { VisitNode(node.Expression) });
            }

            return base.Visit(node);
        }

        // args must be visited before calling this
        // (We do this to save stack space)
        private Expression RewriteSite(CallSiteBinder bindingInfo, Type retType, IList<Expression> args) {
            Type siteType = DynamicSiteHelpers.MakeDynamicSiteType(args.Map(a => a.Type).AddLast(retType));

            // Rewrite the site as a constant
            Expression siteExpr = Expression.Constant(DynamicSiteHelpers.MakeSite(bindingInfo, siteType));
            siteExpr = VisitNode(siteExpr);

            // Rewrite all of the arguments first
            Expression[] siteArgs = new Expression[args.Count + 1];
            siteArgs[0] = siteExpr;
            for (int i = 0; i < args.Count; i++) {
                siteArgs[i + 1] = args[i];
            }

            FieldInfo target = siteType.GetField("Target");

            // TODO: this expands the site's expression twice, which is only
            // correct when it's a constant
            //
            // site.Target.Invoke(site, *args)
            return Expression.Call(
                Expression.Field(siteExpr, target),
                target.FieldType.GetMethod("Invoke"),
                siteArgs
            );
        }
    }
}
