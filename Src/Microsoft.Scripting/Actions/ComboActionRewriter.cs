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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Scripting.Actions;
using System.Scripting.Utils;

namespace Microsoft.Scripting.Actions {
    /// <summary>
    /// A tree rewriter which will find dynamic sites which consume dynamic sites and
    /// turn them into a single combo dynamic site.  The combo dynamic site will then run the
    /// individual meta binders and produce the resulting code in a single dynamic site.
    /// </summary>
    public class ComboActionRewriter : ExpressionTreeVisitor {        
        protected override Expression Visit(ActionExpression node) {
            Debug.Assert(node.IsDynamic);

            return RewriteSite(node, node.Arguments);
        }

        protected override Expression Visit(BinaryExpression node) {
            if (node.IsDynamic) {
                return RewriteSite(node, node.Left, node.Right);
            }

            return base.Visit(node);
        }

        protected override Expression Visit(UnaryExpression node) {
            if (node.IsDynamic) {
                return RewriteSite(node, node.Operand);
            }

            return base.Visit(node);
        }

        protected override Expression Visit(InvocationExpression node) {
            if (node.IsDynamic) {
                return RewriteSite(node, ArrayUtils.Insert(node.Expression, node.Arguments));
            }

            return base.Visit(node);
        }

        protected override Expression Visit(MethodCallExpression node) {
            if (node.IsDynamic) {
                return RewriteSite(node, node.Arguments);
            }

            return base.Visit(node);
        }

        protected override Expression Visit(NewExpression node) {
            if (node.IsDynamic) {
                return RewriteSite(node, node.Arguments);
            }

            return base.Visit(node);
        }

        protected override Expression Visit(DeleteExpression node) {
            if (node.IsDynamic) {
                return RewriteSite(node, node.Expression);
            }

            return base.Visit(node);
        }

        protected override Expression Visit(AssignmentExpression node) {
            if (node.IsDynamic) {
                switch (node.Expression.NodeType) {
                    case ExpressionType.ArrayIndex:
                        BinaryExpression arrayIndex = (BinaryExpression)node.Expression;
                        return RewriteSite(node, arrayIndex.Left, arrayIndex.Right, node.Value);
                    case ExpressionType.MemberAccess:
                        return RewriteSite(node, ((MemberExpression)node.Expression).Expression, node.Value);
                }
            }

            return base.Visit(node);
        }

        protected override Expression Visit(MemberExpression node) {
            if (node.IsDynamic) {
                return RewriteSite(node, node.Expression);
            }

            return base.Visit(node);
        }

        /// <summary>
        /// A reducable node which we use to generate the combo dynamic sites.  Each time we encounter
        /// a dynamic site we replace it with a ComboDynamicSiteExpression.  When a child of a dynamic site
        /// turns out to be a ComboDynamicSiteExpression we will then merge the child with the parent updating
        /// the binding mapping info.  If any of the inputs cause side effects then we'll stop the combination.
        /// </summary>
        class ComboDynamicSiteExpression : Expression {
            private readonly Expression/*!*/[]/*!*/ _inputs;
            private readonly List<BinderMappingInfo/*!*/>/*!*/ _binders;

            public ComboDynamicSiteExpression(Type type, List<BinderMappingInfo/*!*/>/*!*/ binders, Expression/*!*/[]/*!*/ inputs) :
                base(ExpressionType.Extension, type) {

                _binders = binders;
                _inputs = inputs;
            }

            public Expression/*!*/[]/*!*/ Inputs {
                get {
                    return _inputs;
                }
            }

            public List<BinderMappingInfo> Binders {
                get {
                    return _binders;
                }
            }

            public override bool IsReducible {
                get {
                    return true;
                }
            }

            public override Expression/*!*/ Reduce() {
                // we just reduce to a simple ActionExpression
                return Expression.ActionExpression(
                    new ComboBinder(_binders),
                    Type,
                    _inputs
                );
            }
        }

        private Expression RewriteSite(Expression node, params Expression[] args) {
            return RewriteSite(node, (IList<Expression>)args);
        }

        private Expression RewriteSite(Expression node, IList<Expression> args) {
            MetaAction metaBinder = node.BindingInfo as MetaAction;
            if (metaBinder == null) {
                // don't rewrite non meta-binder nodes, we can't compose them
                return node;
            }

            // gather the real arguments for the new dynamic site node
            bool foundSideEffectingArgs = false;
            List<Expression> inputs = new List<Expression>();

            // parameter mapping is 1 List<ComboParameterMappingInfo> for each meta binder, the inner list
            // contains the mapping info for each particular binder

            List<BinderMappingInfo> binders = new List<BinderMappingInfo>();
            List<ParameterMappingInfo> myInfo = new List<ParameterMappingInfo>();

            int actionCount = 0;
            for (int i = 0; i < args.Count; i++) {
                Expression e = args[i];

                if (!foundSideEffectingArgs) {
                    // attempt to combine the arguments...
                    Expression rewritten = VisitNode(e);

                    ComboDynamicSiteExpression combo = rewritten as ComboDynamicSiteExpression;
                    ConstantExpression ce;
                    if (combo != null) {
                        // an action expression we can combine with our own expression
                        
                        // remember how many actions we have so far - if any of our children consume
                        // actions their offset is bumped up
                        int baseActionCount = actionCount;

                        foreach (BinderMappingInfo comboInfo in combo.Binders) {
                            List<ParameterMappingInfo> newInfo = new List<ParameterMappingInfo>();

                            foreach (ParameterMappingInfo info in comboInfo.MappingInfo) {
                                if (info.IsParameter) {
                                    // all of the inputs from the child now become ours
                                    newInfo.Add(ParameterMappingInfo.Parameter(inputs.Count));
                                    inputs.Add(combo.Inputs[info.ParameterIndex]);
                                } else if (info.IsAction) {
                                    newInfo.Add(ParameterMappingInfo.Action(info.ActionIndex + baseActionCount));
                                    actionCount++;
                                } else {
                                    Debug.Assert(info.Constant != null);

                                    // constants can just flow through
                                    newInfo.Add(info);
                                }
                            }

                            binders.Add(new BinderMappingInfo(comboInfo.Binder, newInfo));
                        }

                        myInfo.Add(ParameterMappingInfo.Action(actionCount++));
                    } else if ((ce = rewritten as ConstantExpression) != null) {
                        // we can hoist the constant into the combo
                        myInfo.Add(ParameterMappingInfo.Fixed(ce));
                    } else if (IsSideEffectFree(rewritten)) {
                        // we can treat this as an input parameter
                        myInfo.Add(ParameterMappingInfo.Parameter(inputs.Count));
                        inputs.Add(rewritten);
                    } else {
                        // this argument is doing something we don't understand - we have to leave
                        // it as is (an input we consume) and all the remaining arguments need to be 
                        // evaluated normally as this could have side effects on them.
                        foundSideEffectingArgs = true;
                        myInfo.Add(ParameterMappingInfo.Parameter(inputs.Count));
                        inputs.Add(e);
                    }
                } else {
                    // we've already seen an argument which may have side effects, don't do
                    // any more combinations.
                    myInfo.Add(ParameterMappingInfo.Parameter(inputs.Count));
                    inputs.Add(e);
                }
            }
            binders.Add(new BinderMappingInfo(metaBinder, myInfo));
            
            // TODO: Remove any duplicate inputs (e.g. locals being fed in multiple times)
            return new ComboDynamicSiteExpression(node.Type, binders, inputs.ToArray());
        }

        private bool IsSideEffectFree(Expression rewritten) {
            if (rewritten is ParameterExpression ||
                rewritten is VariableExpression) {
                return true;
            }

            if (rewritten.NodeType == ExpressionType.TypeIs) {
                return IsSideEffectFree(((UnaryExpression)rewritten).Operand);
            }

            BinaryExpression be = rewritten as BinaryExpression;
            if (be != null) {
                if (be.Method == null && !be.IsDynamic && IsSideEffectFree(be.Left) && IsSideEffectFree(be.Right)) {
                    return true;
                }
            }

            return false;
        }
    }
}
