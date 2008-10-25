
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
using System.Diagnostics;
using System.Reflection;
using Microsoft.Scripting.Utils;
using System.Text;
using System.Collections.Generic;

namespace Microsoft.Linq.Expressions {
    //CONFORMING
    public sealed class BinaryExpression : Expression {
        private readonly Expression _left;
        private readonly Expression _right;
        private readonly MethodInfo _method;
        private readonly LambdaExpression _conversion;
        private readonly ExpressionType _nodeType;
        private readonly Type _type;

        internal BinaryExpression(Annotations annotations, ExpressionType nodeType, Expression left, Expression right, Type type)
            : this(annotations, nodeType, left, right, type, null, null) {
        }

        internal BinaryExpression(Annotations annotations, ExpressionType nodeType, Expression left, Expression right, Type type, MethodInfo method)
            : this(annotations, nodeType, left, right, type, method, null) {
        }

        internal BinaryExpression(Annotations annotations,
                                  ExpressionType nodeType,
                                  Expression left,
                                  Expression right,
                                  Type type,
                                  MethodInfo method,
                                  LambdaExpression conversion)

            : base(annotations) {
            // Only Coalesce can have a conversion
            Debug.Assert(conversion == null || nodeType == ExpressionType.Coalesce);

            _nodeType = nodeType;
            _type = type;
            _left = left;
            _right = right;
            _method = method;
            _conversion = conversion;
        }

        protected override Type GetExpressionType() {
            return _type;
        }

        protected override ExpressionType GetNodeKind() {
            return _nodeType;
        }

        public override bool CanReduce {
            get {
                //Only OpAssignments are reducible.
                return IsOpAssignment(NodeType);
            }
        }

        internal static bool IsOpAssignment(ExpressionType op) {
            switch (op) {
                case ExpressionType.AddAssign:
                case ExpressionType.SubtractAssign:
                case ExpressionType.MultiplyAssign:
                case ExpressionType.DivideAssign:
                case ExpressionType.ModuloAssign:
                case ExpressionType.PowerAssign:
                case ExpressionType.AndAssign:
                case ExpressionType.OrAssign:
                case ExpressionType.RightShiftAssign:
                case ExpressionType.LeftShiftAssign:
                case ExpressionType.ExclusiveOrAssign:
                    return true;
            }
            return false;
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

        public override Expression Reduce() {
            //Only reduce OpAssignment expressions.
            if (IsOpAssignment(NodeType)) {
                switch (_left.NodeType) {
                    case ExpressionType.MemberAccess:
                        return ReduceMember();

                    case ExpressionType.Index:
                        return ReduceIndex();

                    default:
                        return ReduceVariable();
                }
            }
            return this;
        }

        //Return the corresponding Op of an assignment op.
        private static ExpressionType GetBinaryOpFromAssignmentOp(ExpressionType op) {
            Debug.Assert(IsOpAssignment(op));
            switch (op) {
                case ExpressionType.AddAssign:
                    return ExpressionType.Add ;
                case ExpressionType.SubtractAssign:
                    return ExpressionType.Subtract;
                case ExpressionType.MultiplyAssign:
                    return ExpressionType.Multiply;
                case ExpressionType.DivideAssign:
                    return ExpressionType.Divide;
                case ExpressionType.ModuloAssign:
                    return ExpressionType.Modulo;
                case ExpressionType.PowerAssign:
                    return ExpressionType.Power;
                case ExpressionType.AndAssign:
                    return ExpressionType.And;
                case ExpressionType.OrAssign:
                    return ExpressionType.Or;
                case ExpressionType.RightShiftAssign:
                    return ExpressionType.RightShift;
                case ExpressionType.LeftShiftAssign:
                    return ExpressionType.LeftShift;
                case ExpressionType.ExclusiveOrAssign:
                    return ExpressionType.ExclusiveOr;
                default:
                    //must be an error
                    throw Error.InvalidOperation("op");
            }
            
        }

        private Expression ReduceVariable() {
            // v (op)= r
            // ... is reduced into ...
            // v = v (op) r
            var op = GetBinaryOpFromAssignmentOp(NodeType);
            return Expression.Assign(
                _left,
                Expression.MakeBinary(op, _left, _right, false, _method),
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
            ParameterExpression temp1 = Variable(member.Expression.Type, "temp1");

            // 1. temp1 = left
            Expression e1 = Expression.Assign(temp1, member.Expression);

            // 2. temp2 = temp1.b (op) r
            var op = GetBinaryOpFromAssignmentOp(NodeType);
            Expression e2 = Expression.MakeBinary(op, Expression.MakeMemberAccess(temp1, member.Member), _right, false, _method);
            ParameterExpression temp2 = Variable(e2.Type, "temp2");
            e2 = Expression.Assign(temp2, e2);

            // 3. temp1.b = temp2
            Expression e3 = Expression.Assign(Expression.MakeMemberAccess(temp1, member.Member), temp2);

            // 3. temp2
            Expression e4 = temp2;

            return Expression.Comma(
                new ParameterExpression[] { temp1, temp2 },
                e1, e2, e3, e4
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

            var vars = new List<ParameterExpression>(index.Arguments.Count + 2);
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
            var binaryOp = GetBinaryOpFromAssignmentOp(NodeType);
            var op = Expression.MakeBinary(binaryOp, tempIndex, _right, false, _method);
            var tempValue = Expression.Variable(op.Type, "tempValue");
            vars.Add(tempValue);
            exprs.Add(Expression.Assign(tempValue, op));

            // tempObj[tempArg0, ... tempArgN] = tempValue
            exprs.Add(Expression.Assign(tempIndex, tempValue));

            return Expression.Comma(vars, exprs);
        }

        public LambdaExpression Conversion {
            get { return _conversion; }
        }

        public bool IsLifted {
            get {
                if (this.NodeType == ExpressionType.Coalesce) {
                    return false;
                }
                bool leftIsNullable = TypeUtils.IsNullableType(_left.Type);
                if (_method != null) {
                    return leftIsNullable && _method.GetParametersCached()[0].ParameterType != _left.Type;
                }
                return leftIsNullable;
            }
        }

        public bool IsLiftedToNull {
            get {
                return this.IsLifted && TypeUtils.IsNullableType(this.Type);
            }
        }

        internal override void BuildString(StringBuilder builder) {
            ContractUtils.RequiresNotNull(builder, "builder");

            switch (NodeType) {
                case ExpressionType.ArrayIndex:
                    _left.BuildString(builder);
                    builder.Append("[");
                    _right.BuildString(builder);
                    builder.Append("]");
                    break;
                default:
                    string op = GetOperator();
                    if (op != null) {
                        builder.Append("(");
                        _left.BuildString(builder);
                        builder.Append(" ");
                        builder.Append(op);
                        builder.Append(" ");
                        _right.BuildString(builder);
                        builder.Append(")");
                    } else {
                        builder.Append(NodeType);
                        builder.Append("(");
                        _left.BuildString(builder);
                        builder.Append(", ");
                        _right.BuildString(builder);
                        builder.Append(")");
                    }
                    break;
            }
        }

        internal override Expression Accept(ExpressionTreeVisitor visitor) {
            return visitor.VisitBinary(this);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private string GetOperator() {
            switch (this.NodeType) {
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    return "+";
                case ExpressionType.AddAssign:
                    return "+=";
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return "-";
                case ExpressionType.SubtractAssign:
                    return "-=";
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    return "*";
                case ExpressionType.MultiplyAssign:
                    return "*=";
                case ExpressionType.Divide:
                    return "/";
                case ExpressionType.DivideAssign:
                    return "/=";
                case ExpressionType.Modulo:
                    return "%";
                case ExpressionType.ModuloAssign:
                    return "%=";
                case ExpressionType.Power:
                    return "**";
                case ExpressionType.PowerAssign:
                    return "**=";
                case ExpressionType.And:
                    if (this.Type == typeof(bool) || this.Type == typeof(bool?)) {
                        return "And";
                    }
                    return "&";
                case ExpressionType.AndAssign:
                    if (this.Type == typeof(bool) || this.Type == typeof(bool?)) {
                        return "&&=";
                    }
                    return "&=";
                case ExpressionType.AndAlso:
                    return "&&";
                case ExpressionType.Or:
                    if (this.Type == typeof(bool) || this.Type == typeof(bool?)) {
                        return "Or";
                    }
                    return "|";
                case ExpressionType.OrAssign:
                    if (this.Type == typeof(bool) || this.Type == typeof(bool?)) {
                        return "||=";
                    }
                    return "|=";
                case ExpressionType.OrElse:
                    return "||";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.Equal:
                    return "=";
                case ExpressionType.NotEqual:
                    return "!=";
                case ExpressionType.Coalesce:
                    return "??";
                case ExpressionType.RightShift:
                    return ">>";
                case ExpressionType.RightShiftAssign:
                    return ">>=";
                case ExpressionType.LeftShift:
                    return "<<";
                case ExpressionType.LeftShiftAssign:
                    return "<<=";
                case ExpressionType.ExclusiveOr:
                    return "^";
                case ExpressionType.ExclusiveOrAssign:
                    return "^=";
            }
            return null;
        }
    }

    public partial class Expression {

        //CONFORMING
        private static BinaryExpression GetUserDefinedBinaryOperator(ExpressionType binaryType, string name, Expression left, Expression right, bool liftToNull, Annotations annotations) {
            // try exact match first
            MethodInfo method = GetUserDefinedBinaryOperator(binaryType, left.Type, right.Type, name);
            if (method != null) {
                return new BinaryExpression(annotations, binaryType, left, right, method.ReturnType, method);
            }
            // try lifted call
            if (TypeUtils.IsNullableType(left.Type) && TypeUtils.IsNullableType(right.Type)) {
                Type nnLeftType = TypeUtils.GetNonNullableType(left.Type);
                Type nnRightType = TypeUtils.GetNonNullableType(right.Type);
                method = GetUserDefinedBinaryOperator(binaryType, nnLeftType, nnRightType, name);
                if (method != null && method.ReturnType.IsValueType && !TypeUtils.IsNullableType(method.ReturnType)) {
                    if (method.ReturnType != typeof(bool) || liftToNull) {
                        return new BinaryExpression(annotations, binaryType, left, right, TypeUtils.GetNullableType(method.ReturnType), method);
                    } else {
                        return new BinaryExpression(annotations, binaryType, left, right, typeof(bool), method);
                    }
                }
            }
            return null;
        }

        //CONFORMING
        private static BinaryExpression GetMethodBasedBinaryOperator(ExpressionType binaryType, Expression left, Expression right, MethodInfo method, bool liftToNull, Annotations annotations) {
            System.Diagnostics.Debug.Assert(method != null);
            ValidateOperator(method);
            ParameterInfo[] pms = method.GetParametersCached();
            if (pms.Length != 2)
                throw Error.IncorrectNumberOfMethodCallArguments(method);
            if (ParameterIsAssignable(pms[0], left.Type) && ParameterIsAssignable(pms[1], right.Type)) {
                ValidateParamswithOperandsOrThrow(pms[0].ParameterType, left.Type, binaryType, method.Name);
                ValidateParamswithOperandsOrThrow(pms[1].ParameterType, right.Type, binaryType, method.Name);
                return new BinaryExpression(annotations, binaryType, left, right, method.ReturnType, method);

            }
            // check for lifted call
            if (TypeUtils.IsNullableType(left.Type) && TypeUtils.IsNullableType(right.Type) &&
                ParameterIsAssignable(pms[0], TypeUtils.GetNonNullableType(left.Type)) &&
                ParameterIsAssignable(pms[1], TypeUtils.GetNonNullableType(right.Type)) &&
                method.ReturnType.IsValueType && !TypeUtils.IsNullableType(method.ReturnType)) {
                if (method.ReturnType != typeof(bool) || liftToNull) {
                    return new BinaryExpression(annotations, binaryType, left, right, TypeUtils.GetNullableType(method.ReturnType), method);
                } else {
                    return new BinaryExpression(annotations, binaryType, left, right, typeof(bool), method);
                }
            }
            throw Error.OperandTypesDoNotMatchParameters(binaryType, method.Name);
        }

        //CONFORMING
        private static BinaryExpression GetUserDefinedBinaryOperatorOrThrow(ExpressionType binaryType, string name, Expression left, Expression right, bool liftToNull, Annotations annotations) {
            BinaryExpression b = GetUserDefinedBinaryOperator(binaryType, name, left, right, liftToNull, annotations);
            if (b != null) {
                ParameterInfo[] pis = b.Method.GetParametersCached();
                ValidateParamswithOperandsOrThrow(pis[0].ParameterType, left.Type, binaryType, name);
                ValidateParamswithOperandsOrThrow(pis[1].ParameterType, right.Type, binaryType, name);
                return b;
            }
            throw Error.BinaryOperatorNotDefined(binaryType, left.Type, right.Type);
        }

        //CONFORMING
        private static MethodInfo GetUserDefinedBinaryOperator(ExpressionType binaryType, Type leftType, Type rightType, string name) {
            // UNDONE: This algorithm is wrong, we should be checking for uniqueness and erroring if
            // UNDONE: it is defined on both types.
            Type[] types = new Type[] { leftType, rightType };
            Type nnLeftType = TypeUtils.GetNonNullableType(leftType);
            Type nnRightType = TypeUtils.GetNonNullableType(rightType);
            BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            MethodInfo method = nnLeftType.GetMethod(name, flags, null, types, null);
            if (method == null && leftType != rightType) {
                method = nnRightType.GetMethod(name, flags, null, types, null);
            }

            if (IsLiftingConditionalLogicalOperator(leftType, rightType, method, binaryType)) {
                method = GetUserDefinedBinaryOperator(binaryType, nnLeftType, nnRightType, name);
            }
            return method;
        }

        //CONFORMING
        private static bool IsLiftingConditionalLogicalOperator(Type left, Type right, MethodInfo method, ExpressionType binaryType) {
            return TypeUtils.IsNullableType(right) &&
                    TypeUtils.IsNullableType(left) &&
                    method == null &&
                    (binaryType == ExpressionType.AndAlso || binaryType == ExpressionType.OrElse);
        }

        //CONFORMING
        private static bool ParameterIsAssignable(ParameterInfo pi, Type argType) {
            Type pType = pi.ParameterType;
            if (pType.IsByRef)
                pType = pType.GetElementType();
            return TypeUtils.AreReferenceAssignable(pType, argType);
        }

        //CONFORMING
        private static void ValidateParamswithOperandsOrThrow(Type paramType, Type operandType, ExpressionType exprType, string name) {
            if (TypeUtils.IsNullableType(paramType) && !TypeUtils.IsNullableType(operandType)) {
                throw Error.OperandTypesDoNotMatchParameters(exprType, name);
            }
        }

        //CONFORMING
        private static void ValidateOperator(MethodInfo method) {
            System.Diagnostics.Debug.Assert(method != null);
            ValidateMethodInfo(method);
            if (!method.IsStatic)
                throw Error.UserDefinedOperatorMustBeStatic(method);
            if (method.ReturnType == typeof(void))
                throw Error.UserDefinedOperatorMustNotBeVoid(method);
        }

        //TODO: consider moving to utils. It is used in many places.
        //CONFORMING
        private static void ValidateMethodInfo(MethodInfo method) {
            if (method.IsGenericMethodDefinition)
                throw Error.MethodIsGeneric(method);
            if (method.ContainsGenericParameters)
                throw Error.MethodContainsGenericParameters(method);
        }

        //CONFORMING
        private static bool IsNullComparison(Expression left, Expression right) {
            // If we have x==null, x!=null, null==x or null!=x where x is
            // nullable but not null, then this is treated as a call to x.HasValue
            // and is legal even if there is no equality operator defined on the
            // type of x.
            if (IsNullConstant(left) && !IsNullConstant(right) && TypeUtils.IsNullableType(right.Type)) {
                return true;
            }
            if (IsNullConstant(right) && !IsNullConstant(left) && TypeUtils.IsNullableType(left.Type)) {
                return true;
            }
            return false;
        }

        //CONFORMING
        // Note: this has different meaning than ConstantCheck.IsNull
        // That function attempts to determine if the result of a tree will be
        // null at runtime. This function is used at tree construction time and
        // only looks for a ConstantExpression with a null Value. It can't
        // become "smarter" or that would break tree construction.
        private static bool IsNullConstant(Expression e) {
            var c = e as ConstantExpression;
            return c != null && c.Value == null;
        }

        //CONFORMING
        private static void ValidateUserDefinedConditionalLogicOperator(ExpressionType nodeType, Type left, Type right, MethodInfo method) {
            ValidateOperator(method);
            ParameterInfo[] pms = method.GetParametersCached();
            if (pms.Length != 2)
                throw Error.IncorrectNumberOfMethodCallArguments(method);
            if (!ParameterIsAssignable(pms[0], left)) {
                if (!(TypeUtils.IsNullableType(left) && ParameterIsAssignable(pms[0], TypeUtils.GetNonNullableType(left))))
                    throw Error.OperandTypesDoNotMatchParameters(nodeType, method.Name);
            }
            if (!ParameterIsAssignable(pms[1], right)) {
                if (!(TypeUtils.IsNullableType(right) && ParameterIsAssignable(pms[1], TypeUtils.GetNonNullableType(right))))
                    throw Error.OperandTypesDoNotMatchParameters(nodeType, method.Name);
            }
            if (pms[0].ParameterType != pms[1].ParameterType)
                throw Error.LogicalOperatorMustHaveConsistentTypes(nodeType, method.Name);
            if (method.ReturnType != pms[0].ParameterType)
                throw Error.LogicalOperatorMustHaveConsistentTypes(nodeType, method.Name);
            if (IsValidLiftedConditionalLogicalOperator(left, right, pms)) {
                left = TypeUtils.GetNonNullableType(left);
                right = TypeUtils.GetNonNullableType(left);
            }
            MethodInfo opTrue = TypeUtils.GetBooleanOperator(method.DeclaringType, "op_True");
            MethodInfo opFalse = TypeUtils.GetBooleanOperator(method.DeclaringType, "op_False");
            if (opTrue == null || opTrue.ReturnType != typeof(bool) ||
                opFalse == null || opFalse.ReturnType != typeof(bool)) {
                throw Error.LogicalOperatorMustHaveBooleanOperators(nodeType, method.Name);
            }
        }

        //CONFORMING
        private static bool IsValidLiftedConditionalLogicalOperator(Type left, Type right, ParameterInfo[] pms) {
            return left == right && TypeUtils.IsNullableType(right) && pms[1].ParameterType == TypeUtils.GetNonNullableType(right);
        }

        //CONFORMING
        public static BinaryExpression MakeBinary(ExpressionType binaryType, Expression left, Expression right) {
            return MakeBinary(binaryType, left, right, false, null, null);
        }
        //CONFORMING
        public static BinaryExpression MakeBinary(ExpressionType binaryType, Expression left, Expression right, bool liftToNull, MethodInfo method) {
            return MakeBinary(binaryType, left, right, liftToNull, method, null, null);
        }
        //CONFORMING
        public static BinaryExpression MakeBinary(ExpressionType binaryType, Expression left, Expression right, bool liftToNull, MethodInfo method, LambdaExpression conversion) {
            return MakeBinary(binaryType, left, right, liftToNull, method, conversion, null);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public static BinaryExpression MakeBinary(ExpressionType binaryType, Expression left, Expression right, bool liftToNull, MethodInfo method, LambdaExpression conversion, Annotations annotations) {
            switch (binaryType) {
                case ExpressionType.Add:
                    return Expression.Add(left, right, method, annotations);
                case ExpressionType.AddChecked:
                    return Expression.AddChecked(left, right, method, annotations);
                case ExpressionType.Subtract:
                    return Expression.Subtract(left, right, method, annotations);
                case ExpressionType.SubtractChecked:
                    return Expression.SubtractChecked(left, right, method, annotations);
                case ExpressionType.Multiply:
                    return Expression.Multiply(left, right, method, annotations);
                case ExpressionType.MultiplyChecked:
                    return Expression.MultiplyChecked(left, right, method, annotations);
                case ExpressionType.Divide:
                    return Expression.Divide(left, right, method, annotations);
                case ExpressionType.Modulo:
                    return Expression.Modulo(left, right, method, annotations);
                case ExpressionType.Power:
                    return Expression.Power(left, right, method, annotations);
                case ExpressionType.And:
                    return Expression.And(left, right, method, annotations);
                case ExpressionType.AndAlso:
                    return Expression.AndAlso(left, right, method, annotations);
                case ExpressionType.Or:
                    return Expression.Or(left, right, method, annotations);
                case ExpressionType.OrElse:
                    return Expression.OrElse(left, right, method, annotations);
                case ExpressionType.LessThan:
                    return Expression.LessThan(left, right, liftToNull, method, annotations);
                case ExpressionType.LessThanOrEqual:
                    return Expression.LessThanOrEqual(left, right, liftToNull, method, annotations);
                case ExpressionType.GreaterThan:
                    return Expression.GreaterThan(left, right, liftToNull, method, annotations);
                case ExpressionType.GreaterThanOrEqual:
                    return Expression.GreaterThanOrEqual(left, right, liftToNull, method, annotations);
                case ExpressionType.Equal:
                    return Expression.Equal(left, right, liftToNull, method, annotations);
                case ExpressionType.NotEqual:
                    return Expression.NotEqual(left, right, liftToNull, method, annotations);
                case ExpressionType.ExclusiveOr:
                    return Expression.ExclusiveOr(left, right, method, annotations);
                case ExpressionType.Coalesce:
                    return Expression.Coalesce(left, right, conversion, annotations);
                case ExpressionType.ArrayIndex:
                    return Expression.ArrayIndex(left, right);
                case ExpressionType.RightShift:
                    return Expression.RightShift(left, right, method, annotations);
                case ExpressionType.LeftShift:
                    return Expression.LeftShift(left, right, method, annotations);
                default:
                    throw Error.UnhandledBinary(binaryType);
            }
        }


        #region Equality Operators

        //CONFORMING
        public static BinaryExpression Equal(Expression left, Expression right) {
            return Equal(left, right, false, null, null);
        }
        public static BinaryExpression Equal(Expression left, Expression right, Annotations annotations) {
            return Equal(left, right, false, null, annotations);
        }
        public static BinaryExpression Equal(Expression left, Expression right, bool liftToNull, MethodInfo method) {
            return Equal(left, right, liftToNull, method, null);
        }
        //CONFORMING
        public static BinaryExpression Equal(Expression left, Expression right, bool liftToNull, MethodInfo method, Annotations annotations) {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method == null) {
                return GetEqualityComparisonOperator(ExpressionType.Equal, "op_Equality", left, right, liftToNull, annotations);
            }
            return GetMethodBasedBinaryOperator(ExpressionType.Equal, left, right, method, liftToNull, annotations);
        }

        public static BinaryExpression NotEqual(Expression left, Expression right) {
            return NotEqual(left, right, null);
        }
        //CONFORMING
        public static BinaryExpression NotEqual(Expression left, Expression right, Annotations annotations) {
            return NotEqual(left, right, false, null, annotations);
        }
        public static BinaryExpression NotEqual(Expression left, Expression right, bool liftToNull, MethodInfo method) {
            return NotEqual(left, right, liftToNull, method, null);
        }
        //CONFORMING
        public static BinaryExpression NotEqual(Expression left, Expression right, bool liftToNull, MethodInfo method, Annotations annotations) {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method == null) {
                return GetEqualityComparisonOperator(ExpressionType.NotEqual, "op_Inequality", left, right, liftToNull, annotations);
            }
            return GetMethodBasedBinaryOperator(ExpressionType.NotEqual, left, right, method, liftToNull, annotations);
        }

        //CONFORMING
        private static BinaryExpression GetEqualityComparisonOperator(ExpressionType binaryType, string opName, Expression left, Expression right, bool liftToNull, Annotations annotations) {
            // known comparison - numeric types, bools, object, enums
            if (left.Type == right.Type && (TypeUtils.IsNumeric(left.Type) || 
                left.Type == typeof(object) || 
                TypeUtils.IsBool(left.Type) || 
                TypeUtils.GetNonNullableType(left.Type).IsEnum)) {
                if (TypeUtils.IsNullableType(left.Type) && liftToNull) {
                    return new BinaryExpression(annotations, binaryType, left, right, typeof(bool?));
                } else {
                    return new BinaryExpression(annotations, binaryType, left, right, typeof(bool));
                }
            }
            // look for user defined operator
            BinaryExpression b = GetUserDefinedBinaryOperator(binaryType, opName, left, right, liftToNull, annotations);
            if (b != null) {
                return b;
            }
            if (TypeUtils.HasBuiltInEqualityOperator(left.Type, right.Type) || IsNullComparison(left, right)) {
                if (TypeUtils.IsNullableType(left.Type) && liftToNull) {
                    return new BinaryExpression(annotations, binaryType, left, right, typeof(bool?));
                } else {
                    return new BinaryExpression(annotations, binaryType, left, right, typeof(bool));
                }
            }
            throw Error.BinaryOperatorNotDefined(binaryType, left.Type, right.Type);
        }

        #endregion

        #region Comparison Expressions

        //CONFORMING
        public static BinaryExpression GreaterThan(Expression left, Expression right) {
            return GreaterThan(left, right, false, null);
        }
        public static BinaryExpression GreaterThan(Expression left, Expression right, bool liftToNull, MethodInfo method) {
            return GreaterThan(left, right, liftToNull, method, null);
        }
        //CONFORMING
        public static BinaryExpression GreaterThan(Expression left, Expression right, bool liftToNull, MethodInfo method, Annotations annotations) {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method == null) {
                return GetComparisonOperator(ExpressionType.GreaterThan, "op_GreaterThan", left, right, liftToNull, annotations);
            }
            return GetMethodBasedBinaryOperator(ExpressionType.GreaterThan, left, right, method, liftToNull, annotations);
        }

        //CONFORMING
        public static BinaryExpression LessThan(Expression left, Expression right) {
            return LessThan(left, right, false, null);
        }
        public static BinaryExpression LessThan(Expression left, Expression right, bool liftToNull, MethodInfo method) {
            return LessThan(left, right, liftToNull, method, null);
        }
        //CONFORMING
        public static BinaryExpression LessThan(Expression left, Expression right, bool liftToNull, MethodInfo method, Annotations annotations) {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method == null) {
                return GetComparisonOperator(ExpressionType.LessThan, "op_LessThan", left, right, liftToNull, annotations);
            }
            return GetMethodBasedBinaryOperator(ExpressionType.LessThan, left, right, method, liftToNull, annotations);
        }

        //CONFORMING
        public static BinaryExpression GreaterThanOrEqual(Expression left, Expression right) {
            return GreaterThanOrEqual(left, right, false, null);
        }
        public static BinaryExpression GreaterThanOrEqual(Expression left, Expression right, bool liftToNull, MethodInfo method) {
            return GreaterThanOrEqual(left, right, liftToNull, method, null);
        }
        //CONFORMING
        public static BinaryExpression GreaterThanOrEqual(Expression left, Expression right, bool liftToNull, MethodInfo method, Annotations annotations) {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method == null) {
                return GetComparisonOperator(ExpressionType.GreaterThanOrEqual, "op_GreaterThanOrEqual", left, right, liftToNull, annotations);
            }
            return GetMethodBasedBinaryOperator(ExpressionType.GreaterThanOrEqual, left, right, method, liftToNull, annotations);
        }

        //CONFORMING
        public static BinaryExpression LessThanOrEqual(Expression left, Expression right) {
            return LessThanOrEqual(left, right, false, null);
        }
        public static BinaryExpression LessThanOrEqual(Expression left, Expression right, bool liftToNull, MethodInfo method) {
            return LessThanOrEqual(left, right, liftToNull, method, null);
        }
        //CONFORMING
        public static BinaryExpression LessThanOrEqual(Expression left, Expression right, bool liftToNull, MethodInfo method, Annotations annotations) {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method == null) {
                return GetComparisonOperator(ExpressionType.LessThanOrEqual, "op_LessThanOrEqual", left, right, liftToNull, annotations);
            }
            return GetMethodBasedBinaryOperator(ExpressionType.LessThanOrEqual, left, right, method, liftToNull, annotations);
        }

        //CONFORMING
        private static BinaryExpression GetComparisonOperator(ExpressionType binaryType, string opName, Expression left, Expression right, bool liftToNull, Annotations annotations) {
            if (left.Type == right.Type && TypeUtils.IsNumeric(left.Type)) {
                if (TypeUtils.IsNullableType(left.Type) && liftToNull) {
                    return new BinaryExpression(annotations, binaryType, left, right, typeof(bool?));
                } else {
                    return new BinaryExpression(annotations, binaryType, left, right, typeof(bool));
                }
            }
            return GetUserDefinedBinaryOperatorOrThrow(binaryType, opName, left, right, liftToNull, annotations);
        }

        #endregion

        #region Boolean Expressions

        //CONFORMING
        public static BinaryExpression AndAlso(Expression left, Expression right) {
            return AndAlso(left, right, null, null);
        }
        //CONFORMING
        public static BinaryExpression AndAlso(Expression left, Expression right, MethodInfo method) {
            return AndAlso(left, right, method, null);
        }
        public static BinaryExpression AndAlso(Expression left, Expression right, MethodInfo method, Annotations annotations) {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            Type returnType;
            if (method == null) {
                if (left.Type == right.Type && TypeUtils.IsBool(left.Type)) {
                    return new BinaryExpression(annotations, ExpressionType.AndAlso, left, right, left.Type);
                }
                method = GetUserDefinedBinaryOperator(ExpressionType.AndAlso, left.Type, right.Type, "op_BitwiseAnd");
                if (method != null) {
                    ValidateUserDefinedConditionalLogicOperator(ExpressionType.AndAlso, left.Type, right.Type, method);
                    returnType = (TypeUtils.IsNullableType(left.Type) && method.ReturnType == TypeUtils.GetNonNullableType(left.Type)) ? left.Type : method.ReturnType;
                    return new BinaryExpression(annotations, ExpressionType.AndAlso, left, right, returnType, method);
                }
                throw Error.BinaryOperatorNotDefined(ExpressionType.AndAlso, left.Type, right.Type);
            }
            ValidateUserDefinedConditionalLogicOperator(ExpressionType.AndAlso, left.Type, right.Type, method);
            returnType = (TypeUtils.IsNullableType(left.Type) && method.ReturnType == TypeUtils.GetNonNullableType(left.Type)) ? left.Type : method.ReturnType;
            return new BinaryExpression(annotations, ExpressionType.AndAlso, left, right, returnType, method);
        }

        //CONFORMING
        public static BinaryExpression OrElse(Expression left, Expression right) {
            return OrElse(left, right, null, null);
        }
        //CONFORMING
        public static BinaryExpression OrElse(Expression left, Expression right, MethodInfo method) {
            return OrElse(left, right, method, null);
        }
        public static BinaryExpression OrElse(Expression left, Expression right, MethodInfo method, Annotations annotations) {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            Type returnType;
            if (method == null) {
                if (left.Type == right.Type && TypeUtils.IsBool(left.Type)) {
                    return new BinaryExpression(annotations, ExpressionType.OrElse, left, right, left.Type);
                }
                method = GetUserDefinedBinaryOperator(ExpressionType.OrElse, left.Type, right.Type, "op_BitwiseOr");
                if (method != null) {
                    ValidateUserDefinedConditionalLogicOperator(ExpressionType.OrElse, left.Type, right.Type, method);
                    returnType = (TypeUtils.IsNullableType(left.Type) && method.ReturnType == TypeUtils.GetNonNullableType(left.Type)) ? left.Type : method.ReturnType;
                    return new BinaryExpression(annotations, ExpressionType.OrElse, left, right, returnType, method);
                }
                throw Error.BinaryOperatorNotDefined(ExpressionType.OrElse, left.Type, right.Type);
            }
            ValidateUserDefinedConditionalLogicOperator(ExpressionType.OrElse, left.Type, right.Type, method);
            returnType = (TypeUtils.IsNullableType(left.Type) && method.ReturnType == TypeUtils.GetNonNullableType(left.Type)) ? left.Type : method.ReturnType;
            return new BinaryExpression(annotations, ExpressionType.OrElse, left, right, returnType, method);
        }

        #endregion

        #region Coalescing Expressions

        //CONFORMING
        public static BinaryExpression Coalesce(Expression left, Expression right) {
            return Coalesce(left, right, null, null);
        }

        //CONFORMING
        public static BinaryExpression Coalesce(Expression left, Expression right, LambdaExpression conversion) {
            return Coalesce(left, right, conversion, null);
        }

        public static BinaryExpression Coalesce(Expression left, Expression right, LambdaExpression conversion, Annotations annotations) {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");

            if (conversion == null) {
                Type resultType = ValidateCoalesceArgTypes(left.Type, right.Type);
                return new BinaryExpression(annotations, ExpressionType.Coalesce, left, right, resultType);
            }

            if (left.Type.IsValueType && !TypeUtils.IsNullableType(left.Type)) {
                throw Error.CoalesceUsedOnNonNullType();
            }

            Type delegateType = conversion.Type;
            Debug.Assert(TypeUtils.AreAssignable(typeof(System.Delegate), delegateType) && delegateType != typeof(System.Delegate));
            MethodInfo method = delegateType.GetMethod("Invoke");
            if (method.ReturnType == typeof(void))
                throw Error.UserDefinedOperatorMustNotBeVoid(conversion);
            ParameterInfo[] pms = method.GetParametersCached();
            Debug.Assert(pms.Length == conversion.Parameters.Count);
            if (pms.Length != 1)
                throw Error.IncorrectNumberOfMethodCallArguments(conversion);
            // The return type must match exactly.
            // CONSIDER: We could weaken this restriction and
            // CONSIDER: say that the return type must be assignable to from
            // CONSIDER: the return type of the lambda.
            if (method.ReturnType != right.Type) {
                throw Error.OperandTypesDoNotMatchParameters(ExpressionType.Coalesce, conversion.ToString());
            }
            // The parameter of the conversion lambda must either be assignable
            // from the erased or unerased type of the left hand side.
            if (!ParameterIsAssignable(pms[0], TypeUtils.GetNonNullableType(left.Type)) &&
                !ParameterIsAssignable(pms[0], left.Type)) {
                throw Error.OperandTypesDoNotMatchParameters(ExpressionType.Coalesce, conversion.ToString());
            }
            return new BinaryExpression(annotations, ExpressionType.Coalesce, left, right, right.Type, null, conversion);
        }

        //CONFORMING
        private static Type ValidateCoalesceArgTypes(Type left, Type right) {
            Type leftStripped = TypeUtils.GetNonNullableType(left);
            if (left.IsValueType && !TypeUtils.IsNullableType(left)) {
                throw Error.CoalesceUsedOnNonNullType();
            } else if (TypeUtils.IsNullableType(left) && TypeUtils.IsImplicitlyConvertible(right, leftStripped)) {
                return leftStripped;
            } else if (TypeUtils.IsImplicitlyConvertible(right, left)) {
                return left;
            } else if (TypeUtils.IsImplicitlyConvertible(leftStripped, right)) {
                return right;
            } else {
                throw Error.ArgumentTypesMustMatch();
            }
        }



        #endregion

        #region Arithmetic Expressions

        //CONFORMING
        public static BinaryExpression Add(Expression left, Expression right) {
            return Add(left, right, null, null);
        }
        //CONFORMING
        public static BinaryExpression Add(Expression left, Expression right, MethodInfo method) {
            return Add(left, right, method, null);
        }        
        public static BinaryExpression Add(Expression left, Expression right, MethodInfo method, Annotations annotations) {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method == null) {
                if (left.Type == right.Type && TypeUtils.IsArithmetic(left.Type)) {
                    return new BinaryExpression(annotations, ExpressionType.Add, left, right, left.Type);
                }
                return GetUserDefinedBinaryOperatorOrThrow(ExpressionType.Add, "op_Addition", left, right, true, annotations);
            }
            return GetMethodBasedBinaryOperator(ExpressionType.Add, left, right, method, true, annotations);
        }

        //CONFORMING
        public static BinaryExpression AddAssign(Expression left, Expression right) {
            return AddAssign(left, right, null, null);
        }
        //CONFORMING
        public static BinaryExpression AddAssign(Expression left, Expression right, MethodInfo method) {
            return AddAssign(left, right, method, null);
        }
        public static BinaryExpression AddAssign(Expression left, Expression right, MethodInfo method, Annotations annotations) {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method == null) {
                if (left.Type == right.Type && TypeUtils.IsArithmetic(left.Type)) {
                    return new BinaryExpression(annotations, ExpressionType.AddAssign, left, right, left.Type);
                }
                return GetUserDefinedBinaryOperatorOrThrow(ExpressionType.AddAssign, "op_AdditionAssignment", left, right, true, annotations);
            }
            return GetMethodBasedBinaryOperator(ExpressionType.AddAssign, left, right, method, true, annotations);
        }

        //CONFORMING
        public static BinaryExpression AddChecked(Expression left, Expression right) {
            return AddChecked(left, right, null, null);
        }
        //CONFORMING
        public static BinaryExpression AddChecked(Expression left, Expression right, MethodInfo method) {
            return AddChecked(left, right, method, null);
        }
        public static BinaryExpression AddChecked(Expression left, Expression right, MethodInfo method, Annotations annotations) {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method == null) {
                if (left.Type == right.Type && TypeUtils.IsArithmetic(left.Type)) {
                    return new BinaryExpression(annotations, ExpressionType.AddChecked, left, right, left.Type);
                }
                return GetUserDefinedBinaryOperatorOrThrow(ExpressionType.AddChecked, "op_Addition", left, right, false, annotations);
            }
            return GetMethodBasedBinaryOperator(ExpressionType.AddChecked, left, right, method, true, annotations);
        }

        //CONFORMING
        public static BinaryExpression Subtract(Expression left, Expression right) {
            return Subtract(left, right, null, null);
        }
        //CONFORMING
        public static BinaryExpression Subtract(Expression left, Expression right, MethodInfo method) {
            return Subtract(left, right, method, null);
        }
        public static BinaryExpression Subtract(Expression left, Expression right, MethodInfo method, Annotations annotations) {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method == null) {
                if (left.Type == right.Type && TypeUtils.IsArithmetic(left.Type)) {
                    return new BinaryExpression(annotations, ExpressionType.Subtract, left, right, left.Type);
                }
                return GetUserDefinedBinaryOperatorOrThrow(ExpressionType.Subtract, "op_Subtraction", left, right, true, annotations);
            }
            return GetMethodBasedBinaryOperator(ExpressionType.Subtract, left, right, method, true, annotations);
        }

        //CONFORMING
        public static BinaryExpression SubtractAssign(Expression left, Expression right) {
            return SubtractAssign(left, right, null, null);
        }
        //CONFORMING
        public static BinaryExpression SubtractAssign(Expression left, Expression right, MethodInfo method) {
            return SubtractAssign(left, right, method, null);
        }
        public static BinaryExpression SubtractAssign(Expression left, Expression right, MethodInfo method, Annotations annotations) {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method == null) {
                if (left.Type == right.Type && TypeUtils.IsArithmetic(left.Type)) {
                    return new BinaryExpression(annotations, ExpressionType.SubtractAssign, left, right, left.Type);
                }
                return GetUserDefinedBinaryOperatorOrThrow(ExpressionType.SubtractAssign, "op_SubtractionAssignment", left, right, true, annotations);
            }
            return GetMethodBasedBinaryOperator(ExpressionType.SubtractAssign, left, right, method, true, annotations);
        }

        //CONFORMING
        public static BinaryExpression SubtractChecked(Expression left, Expression right) {
            return SubtractChecked(left, right, null, null);
        }
        //CONFORMING
        public static BinaryExpression SubtractChecked(Expression left, Expression right, MethodInfo method) {
            return SubtractChecked(left, right, method, null);
        }
        public static BinaryExpression SubtractChecked(Expression left, Expression right, MethodInfo method, Annotations annotations) {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method == null) {
                if (left.Type == right.Type && TypeUtils.IsArithmetic(left.Type)) {
                    return new BinaryExpression(annotations, ExpressionType.SubtractChecked, left, right, left.Type);
                }
                return GetUserDefinedBinaryOperatorOrThrow(ExpressionType.SubtractChecked, "op_Subtraction", left, right, true, annotations);
            }
            return GetMethodBasedBinaryOperator(ExpressionType.SubtractChecked, left, right, method, true, annotations);
        }

        //CONFORMING
        public static BinaryExpression Divide(Expression left, Expression right) {
            return Divide(left, right, null, null);
        }
        //CONFORMING
        public static BinaryExpression Divide(Expression left, Expression right, MethodInfo method) {
            return Divide(left, right, method, null);
        }
        public static BinaryExpression Divide(Expression left, Expression right, MethodInfo method, Annotations annotations) {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method == null) {
                if (left.Type == right.Type && TypeUtils.IsArithmetic(left.Type)) {
                    return new BinaryExpression(annotations, ExpressionType.Divide, left, right, left.Type);
                }
                return GetUserDefinedBinaryOperatorOrThrow(ExpressionType.Divide, "op_Division", left, right, true, annotations);
            }
            return GetMethodBasedBinaryOperator(ExpressionType.Divide, left, right, method, true, annotations);
        }

        //CONFORMING
        public static BinaryExpression DivideAssign(Expression left, Expression right) {
            return DivideAssign(left, right, null, null);
        }
        //CONFORMING
        public static BinaryExpression DivideAssign(Expression left, Expression right, MethodInfo method) {
            return DivideAssign(left, right, method, null);
        }
        public static BinaryExpression DivideAssign(Expression left, Expression right, MethodInfo method, Annotations annotations) {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method == null) {
                if (left.Type == right.Type && TypeUtils.IsArithmetic(left.Type)) {
                    return new BinaryExpression(annotations, ExpressionType.DivideAssign, left, right, left.Type);
                }
                return GetUserDefinedBinaryOperatorOrThrow(ExpressionType.DivideAssign, "op_DivisionAssignment", left, right, true, annotations);
            }
            return GetMethodBasedBinaryOperator(ExpressionType.DivideAssign, left, right, method, true, annotations);
        }

        //CONFORMING
        public static BinaryExpression Modulo(Expression left, Expression right) {
            return Modulo(left, right, null, null);
        }
        //CONFORMING
        public static BinaryExpression Modulo(Expression left, Expression right, MethodInfo method) {
            return Modulo(left, right, method, null);
        }
        public static BinaryExpression Modulo(Expression left, Expression right, MethodInfo method, Annotations annotations) {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method == null) {
                if (left.Type == right.Type && TypeUtils.IsArithmetic(left.Type)) {
                    return new BinaryExpression(annotations, ExpressionType.Modulo, left, right, left.Type);
                }
                return GetUserDefinedBinaryOperatorOrThrow(ExpressionType.Modulo, "op_Modulus", left, right, true, annotations);
            }
            return GetMethodBasedBinaryOperator(ExpressionType.Modulo, left, right, method, true, annotations);
        }

        //CONFORMING
        public static BinaryExpression ModuloAssign(Expression left, Expression right) {
            return ModuloAssign(left, right, null, null);
        }
        //CONFORMING
        public static BinaryExpression ModuloAssign(Expression left, Expression right, MethodInfo method) {
            return ModuloAssign(left, right, method, null);
        }
        public static BinaryExpression ModuloAssign(Expression left, Expression right, MethodInfo method, Annotations annotations) {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method == null) {
                if (left.Type == right.Type && TypeUtils.IsArithmetic(left.Type)) {
                    return new BinaryExpression(annotations, ExpressionType.ModuloAssign, left, right, left.Type);
                }
                return GetUserDefinedBinaryOperatorOrThrow(ExpressionType.ModuloAssign, "op_ModulusAssignment", left, right, true, annotations);
            }
            return GetMethodBasedBinaryOperator(ExpressionType.ModuloAssign, left, right, method, true, annotations);
        }

        //CONFORMING
        public static BinaryExpression Multiply(Expression left, Expression right) {
            return Multiply(left, right, null, null);
        }
        //CONFORMING
        public static BinaryExpression Multiply(Expression left, Expression right, MethodInfo method) {
            return Multiply(left, right, method, null);
        }
        public static BinaryExpression Multiply(Expression left, Expression right, MethodInfo method, Annotations annotations) {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method == null) {
                if (left.Type == right.Type && TypeUtils.IsArithmetic(left.Type)) {
                    return new BinaryExpression(annotations, ExpressionType.Multiply, left, right, left.Type);
                }
                return GetUserDefinedBinaryOperatorOrThrow(ExpressionType.Multiply, "op_Multiply", left, right, true, annotations);
            }
            return GetMethodBasedBinaryOperator(ExpressionType.Multiply, left, right, method, true, annotations);
        }

        //CONFORMING
        public static BinaryExpression MultiplyAssign(Expression left, Expression right) {
            return MultiplyAssign(left, right, null, null);
        }
        //CONFORMING
        public static BinaryExpression MultiplyAssign(Expression left, Expression right, MethodInfo method) {
            return MultiplyAssign(left, right, method, null);
        }
        public static BinaryExpression MultiplyAssign(Expression left, Expression right, MethodInfo method, Annotations annotations) {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method == null) {
                if (left.Type == right.Type && TypeUtils.IsArithmetic(left.Type)) {
                    return new BinaryExpression(annotations, ExpressionType.MultiplyAssign, left, right, left.Type);
                }
                return GetUserDefinedBinaryOperatorOrThrow(ExpressionType.MultiplyAssign, "op_MultiplicationAssignment", left, right, true, annotations);
            }
            return GetMethodBasedBinaryOperator(ExpressionType.MultiplyAssign, left, right, method, true, annotations);
        }

        //CONFORMING
        public static BinaryExpression MultiplyChecked(Expression left, Expression right) {
            return MultiplyChecked(left, right, null, null);
        }
        //CONFORMING
        public static BinaryExpression MultiplyChecked(Expression left, Expression right, MethodInfo method) {
            return MultiplyChecked(left, right, method, null);
        }
        public static BinaryExpression MultiplyChecked(Expression left, Expression right, MethodInfo method, Annotations annotations) {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method == null) {
                if (left.Type == right.Type && TypeUtils.IsArithmetic(left.Type)) {
                    return new BinaryExpression(annotations, ExpressionType.MultiplyChecked, left, right, left.Type);
                }
                return GetUserDefinedBinaryOperatorOrThrow(ExpressionType.MultiplyChecked, "op_Multiply", left, right, true, annotations);
            }
            return GetMethodBasedBinaryOperator(ExpressionType.MultiplyChecked, left, right, method, true, annotations);
        }

        //CONFORMING
        public static BinaryExpression LeftShift(Expression left, Expression right) {
            return LeftShift(left, right, null, null);
        }
        //CONFORMING
        public static BinaryExpression LeftShift(Expression left, Expression right, MethodInfo method) {
            return LeftShift(left, right, method, null);
        }
        public static BinaryExpression LeftShift(Expression left, Expression right, MethodInfo method, Annotations annotations) {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method == null) {
                if (TypeUtils.IsInteger(left.Type) && TypeUtils.GetNonNullableType(right.Type) == typeof(int)) {
                    return new BinaryExpression(annotations, ExpressionType.LeftShift, left, right, left.Type);
                }
                return GetUserDefinedBinaryOperatorOrThrow(ExpressionType.LeftShift, "op_LeftShift", left, right, true, annotations);
            }
            return GetMethodBasedBinaryOperator(ExpressionType.LeftShift, left, right, method, true, annotations);
        }

        //CONFORMING
        public static BinaryExpression LeftShiftAssign(Expression left, Expression right) {
            return LeftShiftAssign(left, right, null, null);
        }
        //CONFORMING
        public static BinaryExpression LeftShiftAssign(Expression left, Expression right, MethodInfo method) {
            return LeftShiftAssign(left, right, method, null);
        }
        public static BinaryExpression LeftShiftAssign(Expression left, Expression right, MethodInfo method, Annotations annotations) {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method == null) {
                if (TypeUtils.IsInteger(left.Type) && TypeUtils.GetNonNullableType(right.Type) == typeof(int)) {
                    return new BinaryExpression(annotations, ExpressionType.LeftShiftAssign, left, right, left.Type);
                }
                return GetUserDefinedBinaryOperatorOrThrow(ExpressionType.LeftShiftAssign, "op_LeftShiftAssignment", left, right, true, annotations);
            }
            return GetMethodBasedBinaryOperator(ExpressionType.LeftShiftAssign, left, right, method, true, annotations);
        }

        //CONFORMING
        public static BinaryExpression RightShift(Expression left, Expression right) {
            return RightShift(left, right, null, null);
        }
        //CONFORMING
        public static BinaryExpression RightShift(Expression left, Expression right, MethodInfo method) {
            return RightShift(left, right, method, null);
        }
        public static BinaryExpression RightShift(Expression left, Expression right, MethodInfo method, Annotations annotations) {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method == null) {
                if (TypeUtils.IsInteger(left.Type) && TypeUtils.GetNonNullableType(right.Type) == typeof(int)) {
                    return new BinaryExpression(annotations, ExpressionType.RightShift, left, right, left.Type);
                }
                return GetUserDefinedBinaryOperatorOrThrow(ExpressionType.RightShift, "op_RightShift", left, right, true, annotations);
            }
            return GetMethodBasedBinaryOperator(ExpressionType.RightShift, left, right, method, true, annotations);
        }

        //CONFORMING
        public static BinaryExpression RightShiftAssign(Expression left, Expression right) {
            return RightShiftAssign(left, right, null, null);
        }
        //CONFORMING
        public static BinaryExpression RightShiftAssign(Expression left, Expression right, MethodInfo method) {
            return RightShiftAssign(left, right, method, null);
        }
        public static BinaryExpression RightShiftAssign(Expression left, Expression right, MethodInfo method, Annotations annotations) {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method == null) {
                if (TypeUtils.IsInteger(left.Type) && TypeUtils.GetNonNullableType(right.Type) == typeof(int)) {
                    return new BinaryExpression(annotations, ExpressionType.RightShiftAssign, left, right, left.Type);
                }
                return GetUserDefinedBinaryOperatorOrThrow(ExpressionType.RightShiftAssign, "op_RightShiftAssignment", left, right, true, annotations);
            }
            return GetMethodBasedBinaryOperator(ExpressionType.RightShiftAssign, left, right, method, true, annotations);
        }

        //CONFORMING
        public static BinaryExpression And(Expression left, Expression right) {
            return And(left, right, null, null);
        }
        //CONFORMING
        public static BinaryExpression And(Expression left, Expression right, MethodInfo method) {
            return And(left, right, method, null);
        }
        public static BinaryExpression And(Expression left, Expression right, MethodInfo method, Annotations annotations) {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method == null) {
                if (left.Type == right.Type && TypeUtils.IsIntegerOrBool(left.Type)) {
                    return new BinaryExpression(annotations, ExpressionType.And, left, right, left.Type);
                }
                return GetUserDefinedBinaryOperatorOrThrow(ExpressionType.And, "op_BitwiseAnd", left, right, true, annotations);
            }
            return GetMethodBasedBinaryOperator(ExpressionType.And, left, right, method, true, annotations);
        }

        //CONFORMING
        public static BinaryExpression AndAssign(Expression left, Expression right) {
            return AndAssign(left, right, null, null);
        }
        //CONFORMING
        public static BinaryExpression AndAssign(Expression left, Expression right, MethodInfo method) {
            return AndAssign(left, right, method, null);
        }
        public static BinaryExpression AndAssign(Expression left, Expression right, MethodInfo method, Annotations annotations) {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method == null) {
                if (left.Type == right.Type && TypeUtils.IsIntegerOrBool(left.Type)) {
                    return new BinaryExpression(annotations, ExpressionType.AndAssign, left, right, left.Type);
                }
                return GetUserDefinedBinaryOperatorOrThrow(ExpressionType.AndAssign, "op_BitwiseAndAssignment", left, right, true, annotations);
            }
            return GetMethodBasedBinaryOperator(ExpressionType.AndAssign, left, right, method, true, annotations);
        }

        //CONFORMING
        public static BinaryExpression Or(Expression left, Expression right) {
            return Or(left, right, null, null);
        }
        //CONFORMING
        public static BinaryExpression Or(Expression left, Expression right, MethodInfo method) {
            return Or(left, right, method, null);
        }
        public static BinaryExpression Or(Expression left, Expression right, MethodInfo method, Annotations annotations) {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method == null) {
                if (left.Type == right.Type && TypeUtils.IsIntegerOrBool(left.Type)) {
                    return new BinaryExpression(annotations, ExpressionType.Or, left, right, left.Type);
                }
                return GetUserDefinedBinaryOperatorOrThrow(ExpressionType.Or, "op_BitwiseOr", left, right, true, annotations);
            }
            return GetMethodBasedBinaryOperator(ExpressionType.Or, left, right, method, true, annotations);
        }

        //CONFORMING
        public static BinaryExpression OrAssign(Expression left, Expression right) {
            return OrAssign(left, right, null, null);
        }
        //CONFORMING
        public static BinaryExpression OrAssign(Expression left, Expression right, MethodInfo method) {
            return OrAssign(left, right, method, null);
        }
        public static BinaryExpression OrAssign(Expression left, Expression right, MethodInfo method, Annotations annotations) {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method == null) {
                if (left.Type == right.Type && TypeUtils.IsIntegerOrBool(left.Type)) {
                    return new BinaryExpression(annotations, ExpressionType.OrAssign, left, right, left.Type);
                }
                return GetUserDefinedBinaryOperatorOrThrow(ExpressionType.OrAssign, "op_BitwiseOrAssignment", left, right, true, annotations);
            }
            return GetMethodBasedBinaryOperator(ExpressionType.OrAssign, left, right, method, true, annotations);
        }

        //CONFORMING
        public static BinaryExpression ExclusiveOr(Expression left, Expression right) {
            return ExclusiveOr(left, right, null, null);
        }
        //CONFORMING
        public static BinaryExpression ExclusiveOr(Expression left, Expression right, MethodInfo method) {
            return ExclusiveOr(left, right, method, null);
        }
        public static BinaryExpression ExclusiveOr(Expression left, Expression right, MethodInfo method, Annotations annotations) {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method == null) {
                if (left.Type == right.Type && TypeUtils.IsIntegerOrBool(left.Type)) {
                    return new BinaryExpression(annotations, ExpressionType.ExclusiveOr, left, right, left.Type);
                }
                return GetUserDefinedBinaryOperatorOrThrow(ExpressionType.ExclusiveOr, "op_ExclusiveOr", left, right, true, annotations);
            }
            return GetMethodBasedBinaryOperator(ExpressionType.ExclusiveOr, left, right, method, true, annotations);
        }

        //CONFORMING
        public static BinaryExpression ExclusiveOrAssign(Expression left, Expression right) {
            return ExclusiveOrAssign(left, right, null, null);
        }
        //CONFORMING
        public static BinaryExpression ExclusiveOrAssign(Expression left, Expression right, MethodInfo method) {
            return ExclusiveOrAssign(left, right, method, null);
        }
        public static BinaryExpression ExclusiveOrAssign(Expression left, Expression right, MethodInfo method, Annotations annotations) {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method == null) {
                if (left.Type == right.Type && TypeUtils.IsIntegerOrBool(left.Type)) {
                    return new BinaryExpression(annotations, ExpressionType.ExclusiveOrAssign, left, right, left.Type);
                }
                return GetUserDefinedBinaryOperatorOrThrow(ExpressionType.ExclusiveOrAssign, "op_ExclusiveOrAssignment", left, right, true, annotations);
            }
            return GetMethodBasedBinaryOperator(ExpressionType.ExclusiveOrAssign, left, right, method, true, annotations);
        }

        //CONFORMING
        public static BinaryExpression Power(Expression left, Expression right) {
            return Power(left, right, null, null);
        }
        //CONFORMING
        public static BinaryExpression Power(Expression left, Expression right, MethodInfo method) {
            return Power(left, right, method, null);
        }
        public static BinaryExpression Power(Expression left, Expression right, MethodInfo method, Annotations annotations) {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method == null) {
                Type mathType = typeof(System.Math);
                method = mathType.GetMethod("Pow", BindingFlags.Static | BindingFlags.Public);
                if (method == null) {
                    throw Error.BinaryOperatorNotDefined(ExpressionType.Power, left.Type, right.Type);
                }
            }
            return GetMethodBasedBinaryOperator(ExpressionType.Power, left, right, method, true, annotations);
        }

        //CONFORMING
        public static BinaryExpression PowerAssign(Expression left, Expression right) {
            return PowerAssign(left, right, null, null);
        }
        //CONFORMING
        public static BinaryExpression PowerAssign(Expression left, Expression right, MethodInfo method) {
            return PowerAssign(left, right, method, null);
        }
        public static BinaryExpression PowerAssign(Expression left, Expression right, MethodInfo method, Annotations annotations) {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method == null) {
                Type mathType = typeof(System.Math);
                method = mathType.GetMethod("Pow", BindingFlags.Static | BindingFlags.Public);
                if (method == null) {
                    throw Error.BinaryOperatorNotDefined(ExpressionType.PowerAssign, left.Type, right.Type);
                }
            }
            return GetMethodBasedBinaryOperator(ExpressionType.PowerAssign, left, right, method, true, annotations);
        }

        #endregion

        #region ArrayIndex Expression

        //CONFORMING
        // Note: it's okay to not include Annotations here. This node is
        // deprecated in favor of ArrayAccess
        public static BinaryExpression ArrayIndex(Expression array, Expression index) {
            RequiresCanRead(array, "array");
            RequiresCanRead(index, "index");
            if (index.Type != typeof(int))
                throw Error.ArgumentMustBeArrayIndexType();

            Type arrayType = array.Type;
            if (!arrayType.IsArray)
                throw Error.ArgumentMustBeArray();
            if (arrayType.GetArrayRank() != 1)
                throw Error.IncorrectNumberOfIndexes();

            return new BinaryExpression(null, ExpressionType.ArrayIndex, array, index, arrayType.GetElementType());
        }

        #endregion        
    }
}
