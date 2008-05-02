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
using System.Reflection;
using System.Diagnostics;

using Microsoft.Scripting.Utils;

// TODO: Remove dependency
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Ast {
    public sealed class BinaryExpression : Expression {
        private readonly Expression/*!*/ _left;
        private readonly Expression/*!*/ _right;
        private readonly MethodInfo _method;

        internal BinaryExpression(AstNodeType nodeType, Expression/*!*/ left, Expression/*!*/ right, Type type, MethodInfo method)
            : this(nodeType, Annotations.Empty, left, right, type, method, null) {
        }

        internal BinaryExpression(AstNodeType nodeType, Annotations annotations, Expression/*!*/ left, Expression/*!*/ right, Type type, MethodInfo method, DynamicAction bindingInfo)
            : base(annotations, nodeType, type, bindingInfo) {
            if (IsBound) {
                RequiresBound(left, "left");
                RequiresBound(right, "right");
            }
            _left = left;
            _right = right;
            _method = method;
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
    }

    public partial class Expression {
        public static BinaryExpression Equal(Expression left, Expression right) {
            return Equality(AstNodeType.Equal, "op_Equality", left, right);
        }

        public static BinaryExpression NotEqual(Expression left, Expression right) {
            return Equality(AstNodeType.NotEqual, "op_Inequality", left, right);
        }

        private static BinaryExpression Equality(AstNodeType nodeType, string opName, Expression left, Expression right) {
            Debug.Assert(nodeType == AstNodeType.Equal || nodeType == AstNodeType.NotEqual);

            ContractUtils.RequiresNotNull(left, "left");
            ContractUtils.RequiresNotNull(right, "right");

            // Numeric types and objects are easy
            if (left.Type == right.Type && (TypeUtils.IsNumeric(left.Type) || left.Type == typeof(object))) {
                return new BinaryExpression(nodeType, left, right, typeof(bool), null);
            }

            BinaryExpression be = UserDefinedBinaryOperator(nodeType, opName, typeof(bool), left, right);
            if (be != null) {
                return be;
            }

            if (TypeUtils.HasBuiltinEquality(left.Type, right.Type)) {
                return new BinaryExpression(nodeType, left, right, typeof(bool), null);
            }

            if (IsNullableComparison(left, right)) {
                return new BinaryExpression(nodeType, left, right, typeof(bool), null);
            }

            throw new ArgumentException(String.Format("Equality operation not defined for {0} and {1}", left.Type.Name, right.Type.Name));
        }

        private static BinaryExpression UserDefinedBinaryOperator(AstNodeType nodeType, string opName, Type result, Expression left, Expression right) {
            MethodInfo method = GetUserDefinedBinaryOperator(opName, left.Type, right.Type, result);
            if (method != null) {
                return new BinaryExpression(nodeType, left, right, method.ReturnType, method);
            }
            return null;
        }

        private static MethodInfo GetUserDefinedBinaryOperator(string opName, Type left, Type right, Type result) {
            MethodInfo method = GetUserDefinedBinaryOperator(opName, left, right);
            if (method != null && TypeUtils.CanAssign(method.ReturnType, result)) {
                return method;
            } else {
                return null;
            }
        }

        private static MethodInfo GetUserDefinedBinaryOperator(string opName, Type left, Type right) {
            BindingFlags bf = BindingFlags.Public | BindingFlags.Static;
            Type[] types = new Type[] { left, right };
            MethodInfo method = left.GetMethod(opName, bf, null, types, null);
            if (method == null) {
                method = right.GetMethod(opName, bf, null, types, null);
            }
            return method;
        }

        private static bool IsNullableComparison(Expression left, Expression right) {
            if (ConstantCheck.IsConstant(left, null) && !ConstantCheck.IsConstant(right, null) && TypeUtils.IsNullableType(right.Type)) {
                return true;
            }
            if (ConstantCheck.IsConstant(right, null) && !ConstantCheck.IsConstant(left, null) && TypeUtils.IsNullableType(left.Type)) {
                return true;
            }
            return false;
        }

        public static BinaryExpression GreaterThan(Expression left, Expression right) {
            return MakeBinaryComparisonExpression(AstNodeType.GreaterThan, left, right);
        }

        public static BinaryExpression LessThan(Expression left, Expression right) {
            return MakeBinaryComparisonExpression(AstNodeType.LessThan, left, right);
        }

        public static BinaryExpression GreaterThanEquals(Expression left, Expression right) {
            return MakeBinaryComparisonExpression(AstNodeType.GreaterThanOrEqual, left, right);
        }

        public static BinaryExpression LessThanEquals(Expression left, Expression right) {
            return MakeBinaryComparisonExpression(AstNodeType.LessThanOrEqual, left, right);
        }

        #region Boolean Expressions

        public static BinaryExpression AndAlso(Expression left, Expression right) {
            return LogicalBinary(AstNodeType.AndAlso, left, right);
        }

        public static BinaryExpression OrElse(Expression left, Expression right) {
            return LogicalBinary(AstNodeType.OrElse, left, right);
        }

        private static BinaryExpression LogicalBinary(AstNodeType nodeType, Expression left, Expression right) {
            ContractUtils.RequiresNotNull(left, "left");
            ContractUtils.RequiresNotNull(right, "right");
            ContractUtils.Requires(TypeUtils.IsBool(left.Type), "left");
            ContractUtils.Requires(TypeUtils.IsBool(right.Type), "right");
            ContractUtils.Requires(left.Type == right.Type);

            return new BinaryExpression(nodeType, left, right, typeof(bool), null);
        }

        #endregion

        #region Coalescing Expressions

        /// <summary>
        /// Null coalescing expression (LINQ).
        /// {result} ::= ((tmp = {_left}) == null) ? {right} : tmp
        /// '??' operator in C#.
        /// </summary>
        public static Expression Coalesce(Expression left, Expression right, out VariableExpression temp) {
            return CoalesceInternal(left, right, null, false, out temp);
        }

        /// <summary>
        /// True coalescing expression.
        /// {result} ::= IsTrue(tmp = {left}) ? {right} : tmp
        /// Generalized AND semantics.
        /// </summary>
        public static Expression CoalesceTrue(Expression left, Expression right, MethodInfo isTrue, out VariableExpression temp) {
            ContractUtils.RequiresNotNull(isTrue, "isTrue");
            return CoalesceInternal(left, right, isTrue, false, out temp);
        }

        /// <summary>
        /// False coalescing expression.
        /// {result} ::= IsTrue(tmp = {left}) ? tmp : {right}
        /// Generalized OR semantics.
        /// </summary>
        public static Expression CoalesceFalse(Expression left, Expression right, MethodInfo isTrue, out VariableExpression temp) {
            ContractUtils.RequiresNotNull(isTrue, "isTrue");
            return CoalesceInternal(left, right, isTrue, true, out temp);
        }

        private static Expression CoalesceInternal(Expression left, Expression right, MethodInfo isTrue, bool isReverse, out VariableExpression temp) {
            ContractUtils.RequiresNotNull(left, "left");
            ContractUtils.RequiresNotNull(right, "right");

            // A bit too strict, but on a safe side.
            ContractUtils.Requires(left.Type == right.Type, "Expression types must match");

            temp = Expression.Temporary(left.Type, "tmp_left");

            Expression condition;
            if (isTrue != null) {
                ContractUtils.Requires(isTrue.ReturnType == typeof(bool), "isTrue", "Predicate must return bool.");
                ParameterInfo[] parameters = isTrue.GetParameters();
                ContractUtils.Requires(parameters.Length == 1, "isTrue", "Predicate must take one parameter.");
                ContractUtils.Requires(isTrue.IsStatic && isTrue.IsPublic, "isTrue", "Predicate must be public and static.");

                Type pt = parameters[0].ParameterType;
                ContractUtils.Requires(TypeUtils.CanAssign(pt, left.Type), "left", "Incorrect left expression type");
                condition = Call(isTrue, Assign(temp, left));
            } else {
                ContractUtils.Requires(TypeUtils.CanCompareToNull(left.Type), "left", "Incorrect left expression type");
                condition = Equal(Assign(temp, left), Null(left.Type));
            }

            Expression t, f;
            if (isReverse) {
                t = Read(temp);
                f = right;
            } else {
                t = right;
                f = Read(temp);
            }

            return Condition(condition, t, f);
        }

        public static Expression Coalesce(LambdaBuilder builder, Expression left, Expression right) {
            VariableExpression temp;
            Expression result = Coalesce(left, right, out temp);
            builder.AddTemp(temp);
            return result;
        }

        /// <summary>
        /// True coalescing expression.
        /// {result} ::= IsTrue(tmp = {left}) ? {right} : tmp
        /// Generalized AND semantics.
        /// </summary>
        public static Expression CoalesceTrue(LambdaBuilder builder, Expression left, Expression right, MethodInfo isTrue) {
            ContractUtils.RequiresNotNull(isTrue, "isTrue");
            VariableExpression temp;
            Expression result = CoalesceTrue(left, right, isTrue, out temp);
            builder.AddTemp(temp);
            return result;
        }

        /// <summary>
        /// False coalescing expression.
        /// {result} ::= IsTrue(tmp = {left}) ? tmp : {right}
        /// Generalized OR semantics.
        /// </summary>
        public static Expression CoalesceFalse(LambdaBuilder builder, Expression left, Expression right, MethodInfo isTrue) {
            ContractUtils.RequiresNotNull(isTrue, "isTrue");
            VariableExpression temp;
            Expression result = CoalesceFalse(left, right, isTrue, out temp);
            builder.AddTemp(temp);
            return result;
        }

        #endregion

        #region Arithmetic Expressions

        /// <summary>
        /// Adds two arithmetic values of the same type.
        /// </summary>
        public static BinaryExpression Add(Expression left, Expression right) {
            return MakeBinaryArithmeticExpression(AstNodeType.Add, left, right);
        }

        /// <summary>
        /// Subtracts two arithmetic values of the same type.
        /// </summary>
        public static BinaryExpression Subtract(Expression left, Expression right) {
            return MakeBinaryArithmeticExpression(AstNodeType.Subtract, left, right);
        }

        /// <summary>
        /// Divides two arithmetic values of the same type.
        /// </summary>
        public static BinaryExpression Divide(Expression left, Expression right) {
            return MakeBinaryArithmeticExpression(AstNodeType.Divide, left, right);
        }

        /// <summary>
        /// Modulos two arithmetic values of the same type.
        /// </summary>
        public static BinaryExpression Modulo(Expression left, Expression right) {
            return MakeBinaryArithmeticExpression(AstNodeType.Modulo, left, right);
        }

        /// <summary>
        /// Multiples two arithmetic values of the same type.
        /// </summary>
        public static BinaryExpression Multiply(Expression left, Expression right) {
            return MakeBinaryArithmeticExpression(AstNodeType.Multiply, left, right);
        }

        /// <summary>
        /// Left shifts one arithmetic value by another aritmetic value of the same type.
        /// </summary>
        public static BinaryExpression LeftShift(Expression left, Expression right) {
            return MakeBinaryArithmeticExpression(AstNodeType.LeftShift, left, right);
        }

        /// <summary>
        /// Right shifts one arithmetic value by another aritmetic value of the same type.
        /// </summary>
        public static BinaryExpression RightShift(Expression left, Expression right) {
            return MakeBinaryArithmeticExpression(AstNodeType.RightShift, left, right);
        }

        /// <summary>
        /// Performs bitwise and of two values of the same type.
        /// </summary>
        public static BinaryExpression And(Expression left, Expression right) {
            return MakeBinaryArithmeticExpression(AstNodeType.And, left, right);
        }

        /// <summary>
        /// Performs bitwise or of two values of the same type.
        /// </summary>
        public static BinaryExpression Or(Expression left, Expression right) {
            return MakeBinaryArithmeticExpression(AstNodeType.Or, left, right);
        }

        /// <summary>
        /// Performs exclusive or of two values of the same type.
        /// </summary>
        public static BinaryExpression ExclusiveOr(Expression left, Expression right) {
            return MakeBinaryArithmeticExpression(AstNodeType.ExclusiveOr, left, right);
        }

        #endregion

        /// <summary>
        /// Creates a binary expression representing array indexing: array[index]
        /// </summary>
        public static BinaryExpression ArrayIndex(Expression array, Expression index) {
            ContractUtils.RequiresNotNull(array, "array");
            ContractUtils.RequiresNotNull(index, "index");
            ContractUtils.Requires(index.Type == typeof(int), "index", "Array index must be an int.");

            Type arrayType = array.Type;
            ContractUtils.Requires(arrayType.IsArray, "array", "Array argument must be array.");
            ContractUtils.Requires(arrayType.GetArrayRank() == 1, "index", "Incorrect number of indices.");

            return new BinaryExpression(AstNodeType.ArrayIndex, array, index, array.Type.GetElementType(), null);
        }

        private static BinaryExpression MakeBinaryArithmeticExpression(AstNodeType nodeType, Expression left, Expression right) {
            ContractUtils.RequiresNotNull(left, "left");
            ContractUtils.RequiresNotNull(left, "right");
            if (left.Type != right.Type || !TypeUtils.IsArithmetic(left.Type)) {
                throw new NotSupportedException(String.Format("{0} only supports identical arithmetic types, got {1} {2}", nodeType, left.Type.Name, right.Type.Name));
            }

            return new BinaryExpression(nodeType, left, right, left.Type, null);
        }

        private static BinaryExpression MakeBinaryComparisonExpression(AstNodeType nodeType, Expression left, Expression right) {
            ContractUtils.RequiresNotNull(left, "left");
            ContractUtils.RequiresNotNull(left, "right");

            if (left.Type != right.Type || !TypeUtils.IsNumeric(left.Type)) {
                throw new NotSupportedException(String.Format("{0} only supports identical numeric types, got {1} {2}", nodeType, left.Type.Name, right.Type.Name));
            }

            return new BinaryExpression(nodeType, left, right, typeof(bool), null);
        }

        #region dynamic operations

        private static BinaryExpression MakeDynamicBinaryExpression(AstNodeType nodeType, Annotations annotations, Expression left, Expression right, Type result, DoOperationAction bindingInfo, Operators expectedOp) {
            ContractUtils.RequiresNotNull(annotations, "annotations");
            ContractUtils.RequiresNotNull(left, "left");
            ContractUtils.RequiresNotNull(left, "right");
            ContractUtils.RequiresNotNull(bindingInfo, "bindingInfo");
            ContractUtils.Requires(bindingInfo.Operation == expectedOp, "bindingInfo", "operation kind must match node type");

            return new BinaryExpression(nodeType, annotations, left, right, result, null, bindingInfo);
        }

        public static BinaryExpression ArrayIndex(Annotations annotations, Expression array, Expression index, Type result, DoOperationAction bindingInfo) {
            return MakeDynamicBinaryExpression(AstNodeType.ArrayIndex, annotations, array, index, result, bindingInfo, Operators.GetItem);
        }

        public static BinaryExpression Equal(Annotations annotations, Expression left, Expression right, Type result, DoOperationAction bindingInfo) {
            return MakeDynamicBinaryExpression(AstNodeType.Equal, annotations, left, right, result, bindingInfo, Operators.Equals);
        }

        public static BinaryExpression NotEqual(Annotations annotations, Expression left, Expression right, Type result, DoOperationAction bindingInfo) {
            return MakeDynamicBinaryExpression(AstNodeType.NotEqual, annotations, left, right, result, bindingInfo, Operators.NotEquals);
        }

        public static BinaryExpression GreaterThan(Annotations annotations, Expression left, Expression right, Type result, DoOperationAction bindingInfo) {
            return MakeDynamicBinaryExpression(AstNodeType.GreaterThan, annotations, left, right, result, bindingInfo, Operators.GreaterThan);
        }

        public static BinaryExpression LessThan(Annotations annotations, Expression left, Expression right, Type result, DoOperationAction bindingInfo) {
            return MakeDynamicBinaryExpression(AstNodeType.LessThan, annotations, left, right, result, bindingInfo, Operators.LessThan);
        }

        public static BinaryExpression GreaterThanEquals(Annotations annotations, Expression left, Expression right, Type result, DoOperationAction bindingInfo) {
            return MakeDynamicBinaryExpression(AstNodeType.GreaterThanOrEqual, annotations, left, right, result, bindingInfo, Operators.GreaterThanOrEqual);
        }

        public static BinaryExpression LessThanEquals(Annotations annotations, Expression left, Expression right, Type result, DoOperationAction bindingInfo) {
            return MakeDynamicBinaryExpression(AstNodeType.LessThanOrEqual, annotations, left, right, result, bindingInfo, Operators.LessThanOrEqual);
        }

        /// <summary>
        /// Adds two arithmetic values of the same type.
        /// </summary>
        public static BinaryExpression Add(Annotations annotations, Expression left, Expression right, Type result, DoOperationAction bindingInfo) {
            return MakeDynamicBinaryExpression(AstNodeType.Add, annotations, left, right, result, bindingInfo, Operators.Add);
        }

        /// <summary>
        /// Subtracts two arithmetic values of the same type.
        /// </summary>
        public static BinaryExpression Subtract(Annotations annotations, Expression left, Expression right, Type result, DoOperationAction bindingInfo) {
            return MakeDynamicBinaryExpression(AstNodeType.Subtract, annotations, left, right, result, bindingInfo, Operators.Subtract);
        }

        /// <summary>
        /// Divides two arithmetic values of the same type.
        /// </summary>
        public static BinaryExpression Divide(Annotations annotations, Expression left, Expression right, Type result, DoOperationAction bindingInfo) {
            return MakeDynamicBinaryExpression(AstNodeType.Divide, annotations, left, right, result, bindingInfo, Operators.Divide);
        }

        /// <summary>
        /// Modulos two arithmetic values of the same type.
        /// </summary>
        public static BinaryExpression Modulo(Annotations annotations, Expression left, Expression right, Type result, DoOperationAction bindingInfo) {
            return MakeDynamicBinaryExpression(AstNodeType.Modulo, annotations, left, right, result, bindingInfo, Operators.Mod);
        }

        /// <summary>
        /// Multiples two arithmetic values of the same type.
        /// </summary>
        public static BinaryExpression Multiply(Annotations annotations, Expression left, Expression right, Type result, DoOperationAction bindingInfo) {
            return MakeDynamicBinaryExpression(AstNodeType.Multiply, annotations, left, right, result, bindingInfo, Operators.Multiply);
        }

        /// <summary>
        /// Left shifts one arithmetic value by another aritmetic value of the same type.
        /// </summary>
        public static BinaryExpression LeftShift(Annotations annotations, Expression left, Expression right, Type result, DoOperationAction bindingInfo) {
            return MakeDynamicBinaryExpression(AstNodeType.LeftShift, annotations, left, right, result, bindingInfo, Operators.LeftShift);
        }

        /// <summary>
        /// Right shifts one arithmetic value by another aritmetic value of the same type.
        /// </summary>
        public static BinaryExpression RightShift(Annotations annotations, Expression left, Expression right, Type result, DoOperationAction bindingInfo) {
            return MakeDynamicBinaryExpression(AstNodeType.RightShift, annotations, left, right, result, bindingInfo, Operators.RightShift);
        }

        /// <summary>
        /// Performs bitwise and of two values of the same type.
        /// </summary>
        public static BinaryExpression And(Annotations annotations, Expression left, Expression right, Type result, DoOperationAction bindingInfo) {
            return MakeDynamicBinaryExpression(AstNodeType.And, annotations, left, right, result, bindingInfo, Operators.BitwiseAnd);
        }

        /// <summary>
        /// Performs bitwise or of two values of the same type.
        /// </summary>
        public static BinaryExpression Or(Annotations annotations, Expression left, Expression right, Type result, DoOperationAction bindingInfo) {
            return MakeDynamicBinaryExpression(AstNodeType.Or, annotations, left, right, result, bindingInfo, Operators.BitwiseOr);
        }

        /// <summary>
        /// Performs exclusive or of two values of the same type.
        /// </summary>
        public static BinaryExpression ExclusiveOr(Annotations annotations, Expression left, Expression right, Type result, DoOperationAction bindingInfo) {
            return MakeDynamicBinaryExpression(AstNodeType.ExclusiveOr, annotations, left, right, result, bindingInfo, Operators.ExclusiveOr);
        }

        #endregion
    }
}
