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
using System.Reflection;
using Microsoft.Scripting.Utils;

namespace Microsoft.Linq.Expressions {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Op")]
    public sealed class OpAssignmentExpression : Expression {
        private readonly Expression _left;
        private readonly Expression _right;
        private readonly MethodInfo _method;
        private readonly ExpressionType _op;

        internal OpAssignmentExpression(Annotations annotations, ExpressionType op, Expression left, Expression right, Type type, MethodInfo method)
            : base(type, true, annotations) {

            _left = left;
            _right = right;
            _method = method;
            _op = op;
        }

        public Expression Right {
            get { return _right; }
        }

        public Expression Left {
            get { return _left; }
        }

        public MethodInfo Method {
            get { return _method; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Op")]
        public ExpressionType Op {
            get {
                return _op;
            }
        }

        public override Expression Reduce() {
            switch (_left.NodeType) {
                case ExpressionType.MemberAccess:
                    return ReduceMember();

                case ExpressionType.Index:
                    return ReduceIndex();

                default:
                    return ReduceVariable();
            }
        }

        private Expression ReduceVariable() {
            // v (op)= r
            // ... is reduced into ...
            // v = v (op) r

            return Expression.Assign(
                _left,
                Expression.MakeBinary(_op, _left, _right, false, _method),
                Annotations
            );
        }

        private Expression ReduceMember() {
            // left.b (op)= r
            // ... is reduced into ...
            // temp1 = left
            // temp2 = temp1.b (op) r
            // temp1.b = temp2
            // temp2

            MemberExpression member = (MemberExpression)_left;
            VariableExpression temp1 = Variable(member.Expression.Type, "temp1");

            // 1. temp1 = left
            Expression e1 = Expression.Assign(temp1, member.Expression);

            // 2. temp2 = temp1.b (op) r
            Expression e2 = Expression.MakeBinary(_op, Expression.MakeMemberAccess(temp1, member.Member), _right, false, _method);
            VariableExpression temp2 = Variable(e2.Type, "temp2");
            e2 = Expression.Assign(temp2, e2);

            // 3. temp1.b = temp2
            Expression e3 = Expression.Assign(Expression.MakeMemberAccess(temp1, member.Member), temp2);

            // 3. temp2
            Expression e4 = temp2;

            return Expression.Scope(
                Expression.Comma(e1, e2, e3, e4),
                temp1, temp2
            );
        }

        private Expression ReduceIndex() {
            // left[a0, a1, ... aN] (op)= r
            //
            // ... is reduced into ...
            //
            // tempObj = left
            // tempArg0 = a0
            // ...
            // tempArgN = aN
            // tempValue = tempObj[tempArg0, ... tempArgN] (op) r
            // tempObj[tempArg0, ... tempArgN] = tempValue

            var index = (IndexExpression)_left;

            var vars = new List<VariableExpression>(index.Arguments.Count + 2);
            var exprs = new List<Expression>(index.Arguments.Count + 3);

            var tempObj = Expression.Variable(index.Object.Type, "tempObj");
            vars.Add(tempObj);
            exprs.Add(Expression.Assign(tempObj, index.Object));

            var tempArgs = new List<Expression>(index.Arguments.Count);
            foreach (var arg in index.Arguments) {
                var tempArg = Expression.Variable(arg.Type, "tempArg" + tempArgs.Count);
                vars.Add(tempArg);
                tempArgs.Add(tempArg);
                exprs.Add(Expression.Assign(tempArg, arg));
            }

            var tempIndex = Expression.MakeIndex(tempObj, index.Indexer, index.Annotations, tempArgs);

            // tempValue = tempObj[tempArg0, ... tempArgN] (op) r
            var op = Expression.MakeBinary(_op, tempIndex, _right, false, _method);
            var tempValue = Expression.Variable(op.Type, "tempValue");
            vars.Add(tempValue);
            exprs.Add(Expression.Assign(tempValue, op));

            // tempObj[tempArg0, ... tempArgN] = tempValue
            exprs.Add(Expression.Assign(tempIndex, tempValue));

            return Expression.Scope(Expression.Comma(exprs), vars);
        }

        protected internal override Expression VisitChildren(ExpressionTreeVisitor visitor) {
            Expression l = visitor.Visit(_left);
            Expression r = visitor.Visit(_right);
            if (l == _left && r == _right) {
                return this;
            }
            return Expression.OpAssign(_op, l, r);
        }
    }

    public partial class Expression {

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Op")]
        public static Expression OpAssign(ExpressionType op, Expression left, Expression right) {
            return OpAssign(op, left, right, null);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Op")]
        public static Expression OpAssign(ExpressionType op, Expression left, Expression right, MethodInfo method) {
            RequiresCanRead(left, "left");
            RequiresCanWrite(left, "left");
            RequiresCanRead(right, "right");
            ContractUtils.Requires(left.Type == right.Type, "right");

            switch (op) {
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.And:
                case ExpressionType.Coalesce:
                case ExpressionType.Divide:
                case ExpressionType.ExclusiveOr:
                case ExpressionType.LeftShift:
                case ExpressionType.Modulo:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Or:
                case ExpressionType.Power:
                case ExpressionType.RightShift:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:

                    break;

                default:
                    throw Error.InvalidOperation("op");
            }

            if (method != null) {
                ParameterInfo[] pis = method.GetParameters();
                ContractUtils.Requires(pis.Length == 2, "method");
                ContractUtils.Requires(TypeUtils.CanAssign(pis[0].ParameterType, left.Type));
            }

            return new OpAssignmentExpression(Annotations.Empty, op, left, right, left.Type, method);
        }
    }
}
