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
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Ast {
    public sealed class BinaryExpression : Expression {
        private readonly Expression /*!*/ _left;
        private readonly Expression /*!*/ _right;
        private readonly MethodInfo _method;

        internal BinaryExpression(AstNodeType nodeType, Expression /*!*/ left, Expression /*!*/ right, Type /*!*/ type, MethodInfo method)
            : base(nodeType, type) {
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
            get {
                return _method;
            }
        }
    }

    public static partial class Ast {
        public static BinaryExpression Equal(Expression left, Expression right) {
            return Equality(AstNodeType.Equal, "op_Equality", left, right);
        }

        public static BinaryExpression NotEqual(Expression left, Expression right) {
            return Equality(AstNodeType.NotEqual, "op_Inequality", left, right);
        }

        private static BinaryExpression Equality(AstNodeType nodeType, string opName, Expression left, Expression right) {
            Debug.Assert(nodeType == AstNodeType.Equal || nodeType == AstNodeType.NotEqual);

            Contract.RequiresNotNull(left, "left");
            Contract.RequiresNotNull(right, "right");

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

        private static BinaryExpression UserDefinedBinaryOperator(AstNodeType nodeType, string opName, Expression left, Expression right) {
            MethodInfo method = GetUserDefinedBinaryOperator(opName, left.Type, right.Type);
            if (method != null) {
                return new BinaryExpression(nodeType, left, right, method.ReturnType, method);
            }
            return null;
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
            Contract.RequiresNotNull(left, "left");
            Contract.RequiresNotNull(right, "right");
            Contract.Requires(TypeUtils.IsBool(left.Type), "left");
            Contract.Requires(TypeUtils.IsBool(right.Type), "right");
            Contract.Requires(left.Type == right.Type);

            return new BinaryExpression(nodeType, left, right, typeof(bool), null);
        }

        #endregion

        #region Coalescing Expressions

        /// <summary>
        /// Null coalescing expression (LINQ).
        /// {result} ::= ((tmp = {_left}) == null) ? {right} : tmp
        /// '??' operator in C#.
        /// </summary>
        public static Expression Coalesce(CodeBlock currentBlock, Expression left, Expression right) {
            return CoalesceInternal(currentBlock, left, right, null, false);
        }

        /// <summary>
        /// True coalescing expression.
        /// {result} ::= IsTrue(tmp = {left}) ? {right} : tmp
        /// Generalized AND semantics.
        /// </summary>
        public static Expression CoalesceTrue(CodeBlock currentBlock, Expression left, Expression right, MethodInfo isTrue) {
            Contract.RequiresNotNull(isTrue, "isTrue");
            return CoalesceInternal(currentBlock, left, right, isTrue, false);
        }

        /// <summary>
        /// False coalescing expression.
        /// {result} ::= IsTrue(tmp = {left}) ? tmp : {right}
        /// Generalized OR semantics.
        /// </summary>
        public static Expression CoalesceFalse(CodeBlock currentBlock, Expression left, Expression right, MethodInfo isTrue) {
            Contract.RequiresNotNull(isTrue, "isTrue");
            return CoalesceInternal(currentBlock, left, right, isTrue, true);
        }

        private static Expression CoalesceInternal(CodeBlock currentBlock, Expression left, Expression right, MethodInfo isTrue, bool isReverse) {
            Contract.RequiresNotNull(currentBlock, "currentBlock");
            Contract.RequiresNotNull(left, "left");
            Contract.RequiresNotNull(right, "right");

            // A bit too strict, but on a safe side.
            Contract.Requires(left.Type == right.Type, "Expression types must match");

            Variable tmp = currentBlock.CreateTemporaryVariable(SymbolTable.StringToId("tmp_left"), left.Type);

            Expression condition;
            if (isTrue != null) {
                Contract.Requires(isTrue.ReturnType == typeof(bool), "isTrue", "Predicate must return bool.");
                ParameterInfo[] parameters = isTrue.GetParameters();
                Contract.Requires(parameters.Length == 1, "isTrue", "Predicate must take one parameter.");
                Contract.Requires(isTrue.IsStatic && isTrue.IsPublic, "isTrue", "Predicate must be public and static.");

                Type pt = parameters[0].ParameterType;
                Contract.Requires(TypeUtils.CanAssign(pt, left.Type), "left", "Incorrect left expression type");
                condition = Call(isTrue, Assign(tmp, left));
            } else {
                Contract.Requires(TypeUtils.CanCompareToNull(left.Type), "left", "Incorrect left expression type");
                condition = Equal(Assign(tmp, left), Null(left.Type));
            }

            Expression t, f;
            if (isReverse) {
                t = Read(tmp);
                f = right;
            } else {
                t = right;
                f = Read(tmp);
            }

            return Condition(condition, t, f);
        }

        #endregion

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

        /// <summary>
        /// Creates a binary expression representing array indexing: array[index]
        /// </summary>
        public static BinaryExpression ArrayIndex(Expression array, Expression index) {
            Contract.RequiresNotNull(array, "array");
            Contract.RequiresNotNull(index, "index");
            Contract.Requires(index.Type == typeof(int), "index", "Array index must be an int.");

            Type arrayType = array.Type;
            Contract.Requires(arrayType.IsArray, "array", "Array argument must be array.");
            Contract.Requires(arrayType.GetArrayRank() == 1, "index", "Incorrect number of indices.");

            return new BinaryExpression(AstNodeType.ArrayIndex, array, index, array.Type.GetElementType(), null);
        }

        private static BinaryExpression MakeBinaryArithmeticExpression(AstNodeType nodeType, Expression left, Expression right) {
            Contract.RequiresNotNull(left, "left");
            Contract.RequiresNotNull(left, "right");
            if (left.Type != right.Type || !TypeUtils.IsArithmetic(left.Type)) {
                throw new NotSupportedException(String.Format("{0} only supports identical arithmetic types, got {1} {2}", nodeType, left.Type.Name, right.Type.Name));
            }

            return new BinaryExpression(nodeType, left, right, left.Type, null);
        }

        private static BinaryExpression MakeBinaryComparisonExpression(AstNodeType nodeType, Expression left, Expression right) {
            Contract.RequiresNotNull(left, "left");
            Contract.RequiresNotNull(left, "right");

            if (left.Type != right.Type || !TypeUtils.IsNumeric(left.Type)) {
                throw new NotSupportedException(String.Format("{0} only supports identical numeric types, got {1} {2}", nodeType, left.Type.Name, right.Type.Name));
            }

            return new BinaryExpression(nodeType, left, right, typeof(bool), null);
        }
    }
}
