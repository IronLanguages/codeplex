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

using System.Reflection;
using System.Scripting.Utils;

namespace System.Linq.Expressions {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Op")]
    public class OpAssignmentExpression : Expression {
        private readonly Expression _left;
        private readonly Expression _right;
        private readonly MethodInfo _method;
        private readonly ExpressionType _op;

        internal OpAssignmentExpression(Annotations annotations, ExpressionType op, Expression left, Expression right, Type type, MethodInfo method)
            : base(type, true, annotations) {
            if (IsBound) {
                RequiresBound(left, "left");
                RequiresBound(right, "right");
            }
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

                case ExpressionType.ArrayIndex:
                    return ReduceArrayIndex();

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
                BinOp(_op, _method, _left, _right),
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
            Expression e2 = BinOp(_op, _method, GetMember(temp1, member.Member), _right);
            VariableExpression temp2 = Variable(e2.Type, "temp2");
            e2 = Expression.Assign(temp2, e2);

            // 3. temp1.b = temp2
            Expression e3 = SetMember(temp1, member.Member, temp2);

            // 3. temp2
            Expression e4 = temp2;

            return Expression.Scope(
                Expression.Comma(e1, e2, e3, e4),
                temp1, temp2
            );
        }

        private static Expression GetMember(Expression temp, MemberInfo member) {
            switch (member.MemberType) {
                case MemberTypes.Property:
                    return Expression.Property(temp, (PropertyInfo)member);
                case MemberTypes.Field:
                    return Expression.Field(temp, (FieldInfo)member);
                default:
                    throw Assert.Unreachable;
            }
        }

        private static Expression SetMember(Expression temp, MemberInfo member, Expression value) {
            switch (member.MemberType) {
                case MemberTypes.Property:
                    return Expression.AssignProperty(temp, (PropertyInfo)member, value);
                case MemberTypes.Field:
                    return Expression.AssignField(temp, (FieldInfo)member, value);
                default:
                    throw Assert.Unreachable;
            }
        }

        private Expression ReduceArrayIndex() {
            // left[b] (op)= r
            // ... is reduced into ...
            // temp1 = left
            // temp2 = b
            // temp3 = temp1[temp2] (op) r
            // temp1[temp2] = temp3
            // temp3

            BinaryExpression ai = (BinaryExpression)_left;

            VariableExpression temp1 = Expression.Variable(ai.Left.Type, "temp1");
            VariableExpression temp2 = Expression.Variable(ai.Right.Type, "temp2");

            // temp1 = left
            Expression e1 = Expression.Assign(temp1, ai.Left);

            // temp2 = b
            Expression e2 = Expression.Assign(temp2, ai.Right);

            // temp3 = temp1[temp2] (op) r
            Expression e3 = BinOp(_op, _method, Expression.ArrayIndex(temp1, temp2), _right);
            VariableExpression temp3 = Expression.Variable(e3.Type, "temp3");
            e3 = Expression.Assign(temp3, e3);

            // temp1[temp2] = temp3
            Expression e4 = Expression.AssignArrayIndex(temp1, temp2, temp3);

            // temp3
            Expression e5 = temp3;

            return Expression.Scope(
                Expression.Comma(e1, e2, e3, e4, e5),
                temp1, temp2, temp3
            );
        }

        private static Expression BinOp(ExpressionType op, MethodInfo method, Expression left, Expression right) {
            if (method != null) {
                return Expression.Call(method, left, right);
            }

            switch (op) {
                case ExpressionType.Add:
                    return Expression.Add(left, right);
                case ExpressionType.AddChecked:
                    return Expression.AddChecked(left, right);
                case ExpressionType.And:
                    return Expression.And(left, right);
                case ExpressionType.Coalesce:
                    return Expression.Coalesce(left, right);
                case ExpressionType.Divide:
                    return Expression.Divide(left, right);
                case ExpressionType.ExclusiveOr:
                    return Expression.ExclusiveOr(left, right);
                case ExpressionType.LeftShift:
                    return Expression.LeftShift(left, right);
                case ExpressionType.Modulo:
                    return Expression.Modulo(left, right);
                case ExpressionType.Multiply:
                    return Expression.Multiply(left, right);
                case ExpressionType.MultiplyChecked:
                    return Expression.MultiplyChecked(left, right);
                case ExpressionType.Or:
                    return Expression.Or(left, right);
                case ExpressionType.Power:
                    return Expression.Power(left, right);
                case ExpressionType.RightShift:
                    return Expression.RightShift(left, right);
                case ExpressionType.Subtract:
                    return Expression.Subtract(left, right);
                case ExpressionType.SubtractChecked:
                    return Expression.SubtractChecked(left, right);
                default:
                    throw Assert.Unreachable;
            }
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
