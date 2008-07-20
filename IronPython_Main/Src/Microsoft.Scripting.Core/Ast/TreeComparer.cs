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
using System.Scripting;
using System.Scripting.Actions;
using System.Scripting.Utils;

namespace System.Linq.Expressions {

    static class TreeComparer {
        #region Tree Walker

        /// <summary>
        /// Walks all of the nodes of a tree and puts all of the expressions into
        /// a list.
        /// </summary>
        class FlatTreeWalker : ExpressionTreeVisitor {
            public List<Expression> Expressions = new List<Expression>();
            public Dictionary<ConstantExpression, ConstantExpression> _templated;

            internal bool IsTemplatedConstant(ConstantExpression constantExpr) {
                if (_templated == null) {
                    return false;
                }

                return _templated.ContainsKey(constantExpr);
            }

            protected override Expression Visit(ActionExpression node) {
                Expressions.Add(node);
                return base.Visit(node);
            }

            protected override Expression Visit(AssignmentExpression node) {
                Expressions.Add(node);
                return base.Visit(node);
            }

            protected override Expression Visit(BinaryExpression node) {
                Expressions.Add(node);
                return base.Visit(node);
            }

            protected override Expression Visit(Block node) {
                Expressions.Add(node);
                return base.Visit(node);
            }

            protected override Expression Visit(BreakStatement node) {
                Expressions.Add(node);
                return base.Visit(node);
            }

            protected override Expression Visit(ConditionalExpression node) {
                Expressions.Add(node);
                return base.Visit(node);
            }

            protected override Expression Visit(ConstantExpression node) {
                // if we've promoted a value to a templated constant turn it
                // back into a normal constant for the purpose of comparing the trees
                ITemplatedValue tempVal = node.Value as ITemplatedValue;
                if (tempVal != null) {
                    if (_templated == null) {
                        _templated = new Dictionary<ConstantExpression, ConstantExpression>();
                    }

                    // if we have templated constants we need to make sure to remember their
                    // templated to see if the resulting rules will be compatible.
                    ConstantExpression newConstant = Expression.Constant(tempVal.ObjectValue, tempVal.GetType().GetGenericArguments()[0]);

                    _templated[newConstant] = newConstant;
                    Expressions.Add(newConstant);
                } else {
                    Expressions.Add(node);
                }
                return base.Visit(node);
            }

            protected override Expression Visit(ContinueStatement node) {
                Expressions.Add(node);
                return base.Visit(node);
            }

            protected override Expression Visit(DoStatement node) {
                Expressions.Add(node);
                return base.Visit(node);
            }

            protected override Expression Visit(EmptyStatement node) {
                Expressions.Add(node);
                return base.Visit(node);
            }

            protected override Expression Visit(InvocationExpression node) {
                Expressions.Add(node);
                return base.Visit(node);
            }

            protected override Expression Visit(LabeledStatement node) {
                Expressions.Add(node);
                return base.Visit(node);
            }

            protected override Expression Visit(LambdaExpression node) {
                Expressions.Add(node);
                return base.Visit(node);
            }

            protected override Expression Visit(LoopStatement node) {
                Expressions.Add(node);
                return base.Visit(node);
            }

            protected override Expression Visit(MemberExpression node) {
                // ignore the templated constants but add normal member expressions
                // We could look for an annotation on TemplatedValue<> incase the user
                // creates one of these but they shouldn't be doing that.
                Expression target = node.Expression;
                if (target == null ||
                    !target.Type.IsGenericType ||
                    target.Type.GetGenericTypeDefinition() != typeof(TemplatedValue<>)) {
                    Expressions.Add(node);
                }

                return base.Visit(node);
            }

            protected override Expression Visit(MethodCallExpression node) {
                Expressions.Add(node);
                return base.Visit(node);
            }

            protected override Expression Visit(NewArrayExpression node) {
                Expressions.Add(node);
                return base.Visit(node);
            }

            protected override Expression Visit(NewExpression node) {
                Expressions.Add(node);
                return base.Visit(node);
            }

            protected override Expression Visit(ParameterExpression node) {
                Expressions.Add(node);
                return base.Visit(node);
            }

            protected override Expression Visit(ReturnStatement node) {
                Expressions.Add(node);
                return base.Visit(node);
            }

            protected override Expression Visit(ScopeExpression node) {
                Expressions.Add(node);
                return base.Visit(node);
            }

            protected override Expression Visit(SwitchStatement node) {
                Expressions.Add(node);
                return base.Visit(node);
            }

            protected override Expression Visit(ThrowStatement node) {
                Expressions.Add(node);
                return base.Visit(node);
            }

            protected override Expression Visit(TryStatement node) {
                Expressions.Add(node);
                return base.Visit(node);
            }

            protected override Expression Visit(TypeBinaryExpression node) {
                Expressions.Add(node);
                return base.Visit(node);
            }

            protected override Expression Visit(UnaryExpression node) {
                Expressions.Add(node);
                return base.Visit(node);
            }

            protected override Expression Visit(VariableExpression node) {
                Expressions.Add(node);
                return base.Visit(node);
            }

            protected override Expression Visit(YieldStatement node) {
                Expressions.Add(node);
                return base.Visit(node);
            }

            protected override Expression Visit(DeleteExpression node) {
                Expressions.Add(node);
                return base.Visit(node);
            }

            protected override Expression VisitExtension(Expression node) {
                if (!node.IsReducible) {
                    Expressions.Add(node);
                } else {
                    return VisitNode(Expression.ReduceToKnown(node));
                }
                return node;
            }
        }

        #endregion

        class VariableInfo {
            private Dictionary<VariableExpression, int> _left = new Dictionary<VariableExpression, int>();
            private Dictionary<VariableExpression, int> _right = new Dictionary<VariableExpression, int>();
            private int _curLeft, _curRight;

            public int GetLeftVariable(VariableExpression ve) {
                if (ve == null) {
                    return -1;
                }

                int res;
                if (!_left.TryGetValue(ve, out res)) {
                    _left[ve] = res = _curLeft++;
                }

                return res;
            }

            public int GetRightVariable(VariableExpression ve) {
                if (ve == null) {
                    return -1;
                }

                int res;
                if (!_right.TryGetValue(ve, out res)) {
                    _right[ve] = res = _curRight++;
                }

                return res;
            }
        }

        /// <summary>
        /// Compares two trees.  If the trees differ only by constants then the list of constants which differ
        /// is provided as a list via an out-param.  The constants collected are the constants in the left
        /// side of the tree and only include constants which differ in value.
        /// </summary>
        public static bool Compare(Expression left, Expression right, out List<ConstantExpression> replacementNodes, out bool tooSpecific) {
            replacementNodes = null;
            tooSpecific = false;

            FlatTreeWalker walkLeft = new FlatTreeWalker();
            FlatTreeWalker walkRight = new FlatTreeWalker();
            walkLeft.VisitNode(left);

            Debug.Assert(walkLeft._templated == null);

            walkRight.VisitNode(right);

            // check the length first to see if the trees are obviously different            
            if (walkLeft.Expressions.Count != walkRight.Expressions.Count) {
                return false;
            }

            // then see if they differ by just constants which we could replace
            List<ConstantExpression> needsReplacement = new List<ConstantExpression>();

            VariableInfo varInfo = new VariableInfo();
            for (int i = 0; i < walkLeft.Expressions.Count; i++) {
                Expression currentLeft = walkLeft.Expressions[i], currentRight = walkRight.Expressions[i];

                // ReductionRewriter should have removed these

                if (currentLeft.NodeType != currentRight.NodeType) {
                    // different node types, they can't possibly be equal
                    return false;
                } else if (currentLeft.Type != currentRight.Type) {
                    // they can't possibly be a match
                    PerfTrack.NoteEvent(PerfTrack.Categories.RuleEvaluation, "TreesDifferByType");
                    return false;
                } else if (currentLeft.BindingInfo != null) {
                    if (currentRight.BindingInfo == null ||
                        !currentRight.BindingInfo.HashCookie.Equals(currentLeft.BindingInfo.HashCookie)) {
                        PerfTrack.NoteEvent(PerfTrack.Categories.RuleEvaluation, "TreesDifferByBindingInfo");
                        return false;
                    }
                } else if (currentRight.BindingInfo != null) {
                    PerfTrack.NoteEvent(PerfTrack.Categories.RuleEvaluation, "TreesDifferByBindingInfo");
                    return false;
                }

                if (!CompareTwoNodes(walkRight, needsReplacement, varInfo, currentLeft, currentRight, ref tooSpecific)) {
                    return false;
                }
            }

            replacementNodes = needsReplacement;
            return true;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private static bool CompareTwoNodes(FlatTreeWalker walkRight, List<ConstantExpression> needsReplacement, VariableInfo varInfo, Expression currentLeft, Expression currentRight, ref bool tooSpecific) {
            switch (currentLeft.NodeType) {
                case ExpressionType.Constant:
                    // check constant value                        
                    ConstantExpression ceLeft = (ConstantExpression)currentLeft;
                    ConstantExpression ceRight = (ConstantExpression)currentRight;

                    object leftValue = ceLeft.Value;
                    object rightValue = ceRight.Value;

                    if (leftValue == null && rightValue == null) {
                        // both are null, no need to template this param.
                        break;
                    }

                    // See if they're both sites
                    CallSite leftSite = ceLeft.Value as CallSite;
                    CallSite rightSite = ceRight.Value as CallSite;
                    if (leftSite != null) {
                        if (rightSite == null) {
                            PerfTrack.NoteEvent(PerfTrack.Categories.RuleEvaluation, "TreesDifferBySiteAndConstant (R)");
                            return false;
                        }

                        if (!leftSite.Binder.HashCookie.Equals(rightSite.Binder.HashCookie)) {
                            PerfTrack.NoteEvent(PerfTrack.Categories.RuleEvaluation, "TreesDifferBySiteBinder");
                            return false;
                        }

                        return true;
                    } else if (rightSite != null) {
                        PerfTrack.NoteEvent(PerfTrack.Categories.RuleEvaluation, "TreesDifferBySiteAndConstant (L)");
                        return false;
                    }

                    // add if left is null and right's something else or
                    // left and right aren't equal.  We'll also add it if
                    // the existing rule has hoisted this value into a template
                    // parameter.
                    if (leftValue == null ||
                        !leftValue.Equals(rightValue) ||
                        walkRight.IsTemplatedConstant(ceRight)) {

                        if (walkRight._templated != null && !walkRight.IsTemplatedConstant(ceRight)) {
                            // if we have template args on the right hand side and this isn't
                            // one of them we need to re-compile a more general rule.
                            tooSpecific = true;
                        }

                        needsReplacement.Add(ceLeft);
                    }
                    break;
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                    if (!CompareEquality((BinaryExpression)currentLeft, (BinaryExpression)currentRight)) {
                        PerfTrack.NoteEvent(PerfTrack.Categories.RuleEvaluation, "TreesDifferByBinary");
                        return false;
                    }
                    break;
                case ExpressionType.Add:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.ArrayIndex:
                case ExpressionType.Divide:
                case ExpressionType.ExclusiveOr:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LeftShift:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.Modulo:
                case ExpressionType.Multiply:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.RightShift:
                case ExpressionType.Subtract:
                    if (!Compare((BinaryExpression)currentLeft, (BinaryExpression)currentRight)) {
                        PerfTrack.NoteEvent(PerfTrack.Categories.RuleEvaluation, "TreesDifferByBinary");
                        return false;
                    }
                    break;
                case ExpressionType.Call:
                    if (!Compare((MethodCallExpression)currentLeft, (MethodCallExpression)currentRight)) {
                        PerfTrack.NoteEvent(PerfTrack.Categories.RuleEvaluation, "TreesDifferByCall");
                        return false;
                    }
                    break;
                case ExpressionType.New:
                    // chcek ConstructorInfo and BindingInfo
                    if (!Compare((NewExpression)currentLeft, (NewExpression)currentRight)) {
                        PerfTrack.NoteEvent(PerfTrack.Categories.RuleEvaluation, "TreesDifferByNew");
                        return false;
                    }
                    break;
                case ExpressionType.TypeIs:
                    // check type
                    if (!Compare((TypeBinaryExpression)currentLeft, (TypeBinaryExpression)currentRight)) {
                        PerfTrack.NoteEvent(PerfTrack.Categories.RuleEvaluation, "TreesDifferByTypeIs");
                        return false;
                    }
                    break;
                case ExpressionType.Scope:
                    // compare factory method
                    if (!Compare(varInfo, (ScopeExpression)currentLeft, (ScopeExpression)currentRight)) {
                        PerfTrack.NoteEvent(PerfTrack.Categories.RuleEvaluation, "TreesDifferByScope");
                        return false;
                    }
                    break;
                case ExpressionType.MemberAccess:
                    // compare member
                    if (!Compare((MemberExpression)currentLeft, (MemberExpression)currentRight)) {
                        PerfTrack.NoteEvent(PerfTrack.Categories.RuleEvaluation, "TreesDifferByMember");
                        return false;
                    }
                    break;
                case ExpressionType.TryStatement:
                    // compare catch finally blocks and their handler types
                    if (!Compare(varInfo, (TryStatement)currentLeft, (TryStatement)currentRight)) {
                        PerfTrack.NoteEvent(PerfTrack.Categories.RuleEvaluation, "TreesDifferByTry");
                        return false;
                    }
                    break;
                case ExpressionType.Variable:
                    if (!Compare(varInfo, (VariableExpression)currentLeft, (VariableExpression)currentRight)) {
                        PerfTrack.NoteEvent(PerfTrack.Categories.RuleEvaluation, "TreesDifferByVariable");
                        return false;
                    }
                    break;
                case ExpressionType.Lambda:
                case ExpressionType.YieldStatement:
                case ExpressionType.ReturnStatement:
                case ExpressionType.Block:
                case ExpressionType.Assign:
                case ExpressionType.BreakStatement:
                case ExpressionType.ThrowStatement:
                case ExpressionType.LoopStatement:
                case ExpressionType.ContinueStatement:
                case ExpressionType.EmptyStatement:
                case ExpressionType.DoStatement:
                case ExpressionType.Convert:
                case ExpressionType.Unbox:
                case ExpressionType.Negate:
                case ExpressionType.Not:
                case ExpressionType.OnesComplement:
                case ExpressionType.Conditional:
                case ExpressionType.ActionExpression:
                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                case ExpressionType.Invoke:
                case ExpressionType.Parameter:
                    // these nodes children and types completely
                    // define the node
                    break;
                case ExpressionType.Generator:
                // TODO: compare the lamdas
                case ExpressionType.LabeledStatement:
                // TODO: cache and compare labels
                case ExpressionType.SwitchStatement:
                // TODO: compare case values
                case ExpressionType.Extension:

                    PerfTrack.NoteEvent(PerfTrack.Categories.RuleEvaluation, "TreesDifferByUnknown");
                    // we should have been reduced, but error on the side of being different.
                    return false;
                default:
                    throw Assert.Unreachable;
            }
            return true;
        }

        private static bool CompareEquality(BinaryExpression left, BinaryExpression right) {
            if (left.Left.Type == typeof(object) && left.Right.Type == typeof(object)) {
                // could be comparing object to runtime constant w/ identity semantics.
                return CompareBinaryForEquality(GetConstantExpression(left.Left), GetConstantExpression(right.Left)) &&
                       CompareBinaryForEquality(GetConstantExpression(left.Right), GetConstantExpression(right.Right));
            }

            return true;
        }

        private static ConstantExpression GetConstantExpression(Expression expression) {
            if (expression.NodeType == ExpressionType.Convert) {
                return GetConstantExpression(((UnaryExpression)expression).Operand);
            }

            return expression as ConstantExpression;
        }

        private static bool CompareBinaryForEquality(ConstantExpression left, ConstantExpression right) {
            if (left == null || right == null) {
                return true;
            }

            return left.Value == right.Value;
        }

        private static bool Compare(BinaryExpression left, BinaryExpression right) {
            if (left.Method != right.Method) {
                return false;
            }

            return true;
        }

        private static bool Compare(MethodCallExpression left, MethodCallExpression right) {
            if (left.Method != right.Method) {
                return false;
            }

            return true;
        }

        private static bool Compare(NewExpression left, NewExpression right) {
            if (left.Constructor != right.Constructor) {
                return false;
            }

            return true;
        }


        private static bool Compare(TypeBinaryExpression left, TypeBinaryExpression right) {
            if (left.TypeOperand != right.TypeOperand) {
                return false;
            }

            return true;
        }

        private static bool Compare(VariableInfo varInfo, ScopeExpression left, ScopeExpression right) {
            if (left.Variables.Count != right.Variables.Count) {
                return false;
            }

            for (int i = 0; i < left.Variables.Count; i++) {
                Compare(varInfo, left.Variables[i], right.Variables[i]);
            }
            return true;
        }

        private static bool Compare(MemberExpression left, MemberExpression right) {
            if (left.Member != right.Member) {
                return false;
            }

            return true;
        }

        private static bool Compare(VariableInfo varInfo, TryStatement left, TryStatement right) {
            if ((left.Finally == null && right.Finally != null) ||
                (left.Finally != null && right.Finally == null)) {
                return false;
            }

            if (left.Handlers.Count != right.Handlers.Count) {
                return false;
            }

            for (int i = 0; i < left.Handlers.Count; i++) {
                if (left.Handlers[i].Test != right.Handlers[i].Test) {
                    return false;
                }

                if (varInfo.GetLeftVariable(left.Handlers[i].Variable) != varInfo.GetRightVariable(right.Handlers[i].Variable)) {
                    return false;
                }
            }

            return true;
        }

        private static bool Compare(VariableInfo varInfo, VariableExpression left, VariableExpression right) {
            if (varInfo.GetLeftVariable(left) != varInfo.GetRightVariable(right)) {
                return false;
            }

            return true;
        }
    }
}
