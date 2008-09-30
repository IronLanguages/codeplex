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
using System.Reflection;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.Linq.Expressions {
    //CONFORMING
    public sealed class UnaryExpression : Expression {
        private readonly Expression _operand;
        private readonly MethodInfo _method;

        internal UnaryExpression(Annotations annotations, ExpressionType nodeType, Expression expression, Type type, MethodInfo method)
            : base(nodeType, type, annotations) {

            _operand = expression;
            _method = method;
        }

        public Expression Operand {
            get { return _operand; }
        }

        public MethodInfo Method {
            get { return _method; }
        }

        public bool IsLifted {
            get {
                if (NodeType == ExpressionType.TypeAs || NodeType == ExpressionType.Quote) {
                    return false;
                }
                bool operandIsNullable = TypeUtils.IsNullableType(_operand.Type);
                bool resultIsNullable = TypeUtils.IsNullableType(this.Type);
                if (_method != null) {
                    return (operandIsNullable && _method.GetParameters()[0].ParameterType != _operand.Type) ||
                           (resultIsNullable && _method.ReturnType != this.Type);
                }
                return operandIsNullable || resultIsNullable;
            }
        }

        public bool IsLiftedToNull {
            get {
                return IsLifted && TypeUtils.IsNullableType(this.Type);
            }
        }

        internal override void BuildString(StringBuilder builder) {
            ContractUtils.RequiresNotNull(builder, "builder");

            switch (this.NodeType) {
                case ExpressionType.TypeAs:
                    builder.Append("(");
                    _operand.BuildString(builder);
                    builder.Append(" As ");
                    builder.Append(this.Type.Name);
                    builder.Append(")");
                    break;
                case ExpressionType.Not:
                    builder.Append("Not");
                    builder.Append("(");
                    _operand.BuildString(builder);
                    builder.Append(")");
                    break;
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                    builder.Append("-");
                    _operand.BuildString(builder);
                    break;
                case ExpressionType.UnaryPlus:
                    builder.Append("+");
                    _operand.BuildString(builder);
                    break;
                case ExpressionType.Quote:
                    _operand.BuildString(builder);
                    break;
                default:
                    builder.Append(this.NodeType);
                    builder.Append("(");
                    _operand.BuildString(builder);
                    builder.Append(")");
                    break;
            }
        }
    }

    /// <summary>
    /// Factory methods.
    /// </summary>
    public partial class Expression {
        //CONFORMING
        public static UnaryExpression MakeUnary(ExpressionType unaryType, Expression operand, Type type) {
            return MakeUnary(unaryType, operand, type, null);
        }
        //CONFORMING
        public static UnaryExpression MakeUnary(ExpressionType unaryType, Expression operand, Type type, MethodInfo method) {
            switch (unaryType) {
                case ExpressionType.Negate:
                    return Expression.Negate(operand, method);
                case ExpressionType.NegateChecked:
                    return Expression.NegateChecked(operand, method);
                case ExpressionType.Not:
                    return Expression.Not(operand, method);
                case ExpressionType.ArrayLength:
                    return Expression.ArrayLength(operand);
                case ExpressionType.Convert:
                    return Expression.Convert(operand, type, method);
                case ExpressionType.ConvertChecked:
                    return Expression.ConvertChecked(operand, type, method);
                case ExpressionType.TypeAs:
                    return Expression.TypeAs(operand, type);
                case ExpressionType.Quote:
                    return Expression.Quote(operand);
                case ExpressionType.UnaryPlus:
                    return Expression.UnaryPlus(operand, method);
                default:
                    throw Error.UnhandledUnary(unaryType);
            }
        }

        //CONFORMING
        private static UnaryExpression GetUserDefinedUnaryOperatorOrThrow(ExpressionType unaryType, string name, Expression operand) {
            UnaryExpression u = GetUserDefinedUnaryOperator(unaryType, name, operand);
            if (u != null) {
                ValidateParamswithOperandsOrThrow(u.Method.GetParameters()[0].ParameterType, operand.Type, unaryType, name);
                return u;
            }
            throw Error.UnaryOperatorNotDefined(unaryType, operand.Type);
        }
        //CONFORMING
        private static UnaryExpression GetUserDefinedUnaryOperator(ExpressionType unaryType, string name, Expression operand) {
            Type operandType = operand.Type;
            Type[] types = new Type[] { operandType };
            Type nnOperandType = TypeUtils.GetNonNullableType(operandType);
            MethodInfo method = nnOperandType.GetMethod(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, types, null);
            if (method != null) {
                return new UnaryExpression(Annotations.Empty, unaryType, operand, method.ReturnType, method);
            }
            // try lifted call
            if (TypeUtils.IsNullableType(operandType)) {
                types[0] = nnOperandType;
                method = nnOperandType.GetMethod(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, types, null);
                if (method != null && method.ReturnType.IsValueType && !TypeUtils.IsNullableType(method.ReturnType)) {
                    return new UnaryExpression(Annotations.Empty, unaryType, operand, TypeUtils.GetNullableType(method.ReturnType), method);
                }
            }
            return null;
        }
        //CONFORMING
        private static UnaryExpression GetMethodBasedUnaryOperator(ExpressionType unaryType, Expression operand, MethodInfo method) {
            System.Diagnostics.Debug.Assert(method != null);
            ValidateOperator(method);
            ParameterInfo[] pms = method.GetParameters();
            if (pms.Length != 1)
                throw Error.IncorrectNumberOfMethodCallArguments(method);
            if (ParameterIsAssignable(pms[0], operand.Type)) {
                ValidateParamswithOperandsOrThrow(pms[0].ParameterType, operand.Type, unaryType, method.Name);
                return new UnaryExpression(Annotations.Empty, unaryType, operand, method.ReturnType, method);
            }
            // check for lifted call
            if (TypeUtils.IsNullableType(operand.Type) &&
                ParameterIsAssignable(pms[0], TypeUtils.GetNonNullableType(operand.Type)) &&
                method.ReturnType.IsValueType && !TypeUtils.IsNullableType(method.ReturnType)) {
                return new UnaryExpression(Annotations.Empty, unaryType, operand, TypeUtils.GetNullableType(method.ReturnType), method);
            }

            throw Error.OperandTypesDoNotMatchParameters(unaryType, method.Name);
        }

        //CONFORMING
        private static UnaryExpression GetUserDefinedCoercionOrThrow(ExpressionType coercionType, Expression expression, Type convertToType) {
            UnaryExpression u = GetUserDefinedCoercion(coercionType, expression, convertToType);
            if (u != null) {
                return u;
            }
            throw Error.CoercionOperatorNotDefined(expression.Type, convertToType);
        }
        //CONFORMING
        private static UnaryExpression GetUserDefinedCoercion(ExpressionType coercionType, Expression expression, Type convertToType) {
            // check for implicit coercions first
            Type nnExprType = TypeUtils.GetNonNullableType(expression.Type);
            Type nnConvType = TypeUtils.GetNonNullableType(convertToType);
            // try exact match on types
            MethodInfo[] eMethods = nnExprType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            MethodInfo method = FindConversionOperator(eMethods, expression.Type, convertToType);
            if (method != null) {
                return new UnaryExpression(Annotations.Empty, coercionType, expression, convertToType, method);
            }
            MethodInfo[] cMethods = nnConvType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            method = FindConversionOperator(cMethods, expression.Type, convertToType);
            if (method != null) {
                return new UnaryExpression(Annotations.Empty, coercionType, expression, convertToType, method);
            }
            // try lifted conversion
            if (nnExprType != expression.Type || nnConvType != convertToType) {
                method = FindConversionOperator(eMethods, nnExprType, nnConvType);
                if (method == null) {
                    method = FindConversionOperator(cMethods, nnExprType, nnConvType);
                }
                if (method != null) {
                    return new UnaryExpression(Annotations.Empty, coercionType, expression, convertToType, method);
                }
            }
            return null;
        }
        //CONFORMING
        private static MethodInfo FindConversionOperator(MethodInfo[] methods, Type typeFrom, Type typeTo) {
            foreach (MethodInfo mi in methods) {
                if (mi.Name != "op_Implicit" && mi.Name != "op_Explicit")
                    continue;
                if (mi.ReturnType != typeTo)
                    continue;
                ParameterInfo[] pis = mi.GetParameters();
                if (pis[0].ParameterType != typeFrom)
                    continue;
                return mi;
            }
            return null;
        }
        //CONFORMING
        private static UnaryExpression GetMethodBasedCoercionOperator(ExpressionType unaryType, Expression operand, Type convertToType, MethodInfo method) {
            System.Diagnostics.Debug.Assert(method != null);
            ValidateOperator(method);
            ParameterInfo[] pms = method.GetParameters();
            if (pms.Length != 1)
                throw Error.IncorrectNumberOfMethodCallArguments(method);
            if (ParameterIsAssignable(pms[0], operand.Type) && method.ReturnType == convertToType) {
                return new UnaryExpression(Annotations.Empty, unaryType, operand, method.ReturnType, method);
            }
            // check for lifted call
            if ((TypeUtils.IsNullableType(operand.Type) || TypeUtils.IsNullableType(convertToType)) &&
                ParameterIsAssignable(pms[0], TypeUtils.GetNonNullableType(operand.Type)) &&
                method.ReturnType == TypeUtils.GetNonNullableType(convertToType)) {
                return new UnaryExpression(Annotations.Empty, unaryType, operand, convertToType, method);
            }
            throw Error.OperandTypesDoNotMatchParameters(unaryType, method.Name);
        }

        //CONFORMING
        public static UnaryExpression Negate(Expression expression) {
            RequiresCanRead(expression, "expression");
            if (TypeUtils.IsArithmetic(expression.Type) && !TypeUtils.IsUnsignedInt(expression.Type)) {
                return new UnaryExpression(Annotations.Empty, ExpressionType.Negate, expression, expression.Type, null);
            }
            return GetUserDefinedUnaryOperatorOrThrow(ExpressionType.Negate, "op_UnaryNegation", expression);
        }
        //CONFORMING
        public static UnaryExpression Negate(Expression expression, MethodInfo method) {
            RequiresCanRead(expression, "expression");
            if (method == null) {
                return Negate(expression);
            }
            return GetMethodBasedUnaryOperator(ExpressionType.Negate, expression, method);
        }

        //CONFORMING
        public static UnaryExpression UnaryPlus(Expression expression) {
            RequiresCanRead(expression, "expression");
            if (TypeUtils.IsArithmetic(expression.Type)) {
                return new UnaryExpression(Annotations.Empty, ExpressionType.UnaryPlus, expression, expression.Type, null);
            }
            return GetUserDefinedUnaryOperatorOrThrow(ExpressionType.UnaryPlus, "op_UnaryPlus", expression);
        }
        //CONFORMING
        public static UnaryExpression UnaryPlus(Expression expression, MethodInfo method) {
            RequiresCanRead(expression, "expression");
            if (method == null) {
                return UnaryPlus(expression);
            }
            return GetMethodBasedUnaryOperator(ExpressionType.UnaryPlus, expression, method);
        }

        //CONFORMING
        public static UnaryExpression NegateChecked(Expression expression) {
            RequiresCanRead(expression, "expression");
            if (TypeUtils.IsArithmetic(expression.Type) && !TypeUtils.IsUnsignedInt(expression.Type)) {
                return new UnaryExpression(Annotations.Empty, ExpressionType.NegateChecked, expression, expression.Type, null);
            }
            return GetUserDefinedUnaryOperatorOrThrow(ExpressionType.NegateChecked, "op_UnaryNegation", expression);
        }
        //CONFORMING
        public static UnaryExpression NegateChecked(Expression expression, MethodInfo method) {
            RequiresCanRead(expression, "expression");
            if (method == null) {
                return NegateChecked(expression);
            }
            return GetMethodBasedUnaryOperator(ExpressionType.NegateChecked, expression, method);
        }

        //CONFORMING
        public static UnaryExpression Not(Expression expression) {
            RequiresCanRead(expression, "expression");
            if (TypeUtils.IsIntegerOrBool(expression.Type)) {
                return new UnaryExpression(Annotations.Empty, ExpressionType.Not, expression, expression.Type, null);
            }
            UnaryExpression u = GetUserDefinedUnaryOperator(ExpressionType.Not, "op_LogicalNot", expression);
            if (u != null)
                return u;
            return GetUserDefinedUnaryOperatorOrThrow(ExpressionType.Not, "op_OnesComplement", expression);
        }
        //CONFORMING
        public static UnaryExpression Not(Expression expression, MethodInfo method) {
            RequiresCanRead(expression, "expression");
            if (method == null) {
                return Not(expression);
            }
            return GetMethodBasedUnaryOperator(ExpressionType.Not, expression, method);
        }

        //CONFORMING
        public static UnaryExpression TypeAs(Expression expression, Type type) {
            RequiresCanRead(expression, "expression");
            ContractUtils.RequiresNotNull(type, "type");
            if (type.IsValueType && !TypeUtils.IsNullableType(type)) {
                throw Error.IncorrectTypeForTypeAs(type);
            }
            return new UnaryExpression(Annotations.Empty, ExpressionType.TypeAs, expression, type, null);
        }

        // TODO: optional annotations and remove this overload?
        public static UnaryExpression Unbox(Expression expression, Type type) {
            return Unbox(expression, type, null);
        }

        public static UnaryExpression Unbox(Expression expression, Type type, Annotations annotations) {
            RequiresCanRead(expression, "expression");
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.Requires(
                expression.Type.IsInterface || expression.Type == typeof(object),
                "expression", Strings.InvalidUnboxType
            );
            ContractUtils.Requires(type.IsValueType, "type", Strings.InvalidUnboxType);
            return new UnaryExpression(annotations, ExpressionType.Unbox, expression, type, null);
        }

        //CONFORMING
        public static UnaryExpression Convert(Expression expression, Type type) {
            RequiresCanRead(expression, "expression");
            ContractUtils.RequiresNotNull(type, "type");
            if (TypeUtils.HasIdentityPrimitiveOrNullableConversion(expression.Type, type) ||
                TypeUtils.HasReferenceConversion(expression.Type, type)) {
                return new UnaryExpression(Annotations.Empty, ExpressionType.Convert, expression, type, null);
            }
            return GetUserDefinedCoercionOrThrow(ExpressionType.Convert, expression, type);
        }

        //CONFORMING
        public static UnaryExpression Convert(Expression expression, Type type, MethodInfo method) {
            RequiresCanRead(expression, "expression");
            if (method == null) {
                return Convert(expression, type);
            }
            return GetMethodBasedCoercionOperator(ExpressionType.Convert, expression, type, method);
        }

        //CONFORMING
        public static UnaryExpression ConvertChecked(Expression expression, Type type) {
            RequiresCanRead(expression, "expression");
            ContractUtils.RequiresNotNull(type, "type");
            if (TypeUtils.HasIdentityPrimitiveOrNullableConversion(expression.Type, type)) {
                return new UnaryExpression(Annotations.Empty, ExpressionType.ConvertChecked, expression, type, null);
            }
            if (TypeUtils.HasReferenceConversion(expression.Type, type)) {
                return new UnaryExpression(Annotations.Empty, ExpressionType.Convert, expression, type, null);
            }
            return GetUserDefinedCoercionOrThrow(ExpressionType.ConvertChecked, expression, type);
        }

        //CONFORMING
        public static UnaryExpression ConvertChecked(Expression expression, Type type, MethodInfo method) {
            RequiresCanRead(expression, "expression");
            if (method == null) {
                return ConvertChecked(expression, type);
            }
            return GetMethodBasedCoercionOperator(ExpressionType.ConvertChecked, expression, type, method);
        }

        //CONFORMING
        public static UnaryExpression ArrayLength(Expression array) {
            ContractUtils.RequiresNotNull(array, "array");
            if (!array.Type.IsArray || !TypeUtils.AreAssignable(typeof(Array), array.Type)) {
                throw Error.ArgumentMustBeArray();
            }
            if (array.Type.GetArrayRank() != 1) {
                throw Error.ArgumentMustBeSingleDimensionalArrayType();
            }
            return new UnaryExpression(Annotations.Empty, ExpressionType.ArrayLength, array, typeof(int), null);
        }

        //CONFORMING
        public static UnaryExpression Quote(Expression expression) {
            RequiresCanRead(expression, "expression");
            return new UnaryExpression(Annotations.Empty, ExpressionType.Quote, expression, expression.GetType(), null);
        }

        public static Expression ConvertHelper(Expression expression, Type type) {
            RequiresCanRead(expression, "expression");
            ContractUtils.RequiresNotNull(type, "type");

            if (expression.Type != type) {
                expression = Convert(expression, type);
            }
            return expression;
        }

        public static Expression Void(Expression expression) {
            RequiresCanRead(expression, "expression");
            return ConvertHelper(expression, typeof(void));
        }
    }
}
