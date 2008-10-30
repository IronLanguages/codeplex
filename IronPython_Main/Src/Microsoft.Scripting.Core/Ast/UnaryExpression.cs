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
        private readonly ExpressionType _nodeType;
        private readonly Type _type;

        internal UnaryExpression(Annotations annotations, ExpressionType nodeType, Expression expression, Type type, MethodInfo method)
            : base(annotations) {

            _operand = expression;
            _method = method;
            _nodeType = nodeType;
            _type = type;
        }

        protected override Type GetExpressionType() {
            return _type;
        }

        protected override ExpressionType GetNodeKind() {
            return _nodeType;
        }

        public Expression Operand {
            get { return _operand; }
        }

        public MethodInfo Method {
            get { return _method; }
        }

        public bool IsLifted {
            get {
                if (NodeType == ExpressionType.TypeAs || NodeType == ExpressionType.Quote || NodeType == ExpressionType.Throw) {
                    return false;
                }
                bool operandIsNullable = TypeUtils.IsNullableType(_operand.Type);
                bool resultIsNullable = TypeUtils.IsNullableType(this.Type);
                if (_method != null) {
                    return (operandIsNullable && _method.GetParametersCached()[0].ParameterType != _operand.Type) ||
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

        internal override Expression Accept(ExpressionTreeVisitor visitor) {
            return visitor.VisitUnary(this);
        }
    }

    /// <summary>
    /// Factory methods.
    /// </summary>
    public partial class Expression {
        //CONFORMING
        public static UnaryExpression MakeUnary(ExpressionType unaryType, Expression operand, Type type) {
            return MakeUnary(unaryType, operand, type, null, null);
        }
        //CONFORMING
        public static UnaryExpression MakeUnary(ExpressionType unaryType, Expression operand, Type type, MethodInfo method) {
            return MakeUnary(unaryType, operand, type, method, null);
        }

        public static UnaryExpression MakeUnary(ExpressionType unaryType, Expression operand, Type type, MethodInfo method, Annotations annotations) {
            switch (unaryType) {
                case ExpressionType.Negate:
                    return Expression.Negate(operand, method, annotations);
                case ExpressionType.NegateChecked:
                    return Expression.NegateChecked(operand, method, annotations);
                case ExpressionType.Not:
                    return Expression.Not(operand, method, annotations);
                case ExpressionType.ArrayLength:
                    return Expression.ArrayLength(operand, annotations);
                case ExpressionType.Convert:
                    return Expression.Convert(operand, type, method, annotations);
                case ExpressionType.ConvertChecked:
                    return Expression.ConvertChecked(operand, type, method, annotations);
                case ExpressionType.Throw:
                    return Expression.Throw(operand, type, annotations);
                case ExpressionType.TypeAs:
                    return Expression.TypeAs(operand, type, annotations);
                case ExpressionType.Quote:
                    return Expression.Quote(operand, annotations);
                case ExpressionType.UnaryPlus:
                    return Expression.UnaryPlus(operand, method, annotations);
                case ExpressionType.Unbox:
                    return Expression.Unbox(operand, type, annotations);
                default:
                    throw Error.UnhandledUnary(unaryType);
            }
        }

        //CONFORMING
        private static UnaryExpression GetUserDefinedUnaryOperatorOrThrow(ExpressionType unaryType, string name, Expression operand, Annotations annotations) {
            UnaryExpression u = GetUserDefinedUnaryOperator(unaryType, name, operand, annotations);
            if (u != null) {
                ValidateParamswithOperandsOrThrow(u.Method.GetParametersCached()[0].ParameterType, operand.Type, unaryType, name);
                return u;
            }
            throw Error.UnaryOperatorNotDefined(unaryType, operand.Type);
        }
        //CONFORMING
        private static UnaryExpression GetUserDefinedUnaryOperator(ExpressionType unaryType, string name, Expression operand, Annotations annotations) {
            Type operandType = operand.Type;
            Type[] types = new Type[] { operandType };
            Type nnOperandType = TypeUtils.GetNonNullableType(operandType);
            MethodInfo method = nnOperandType.GetMethod(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, types, null);
            if (method != null) {
                return new UnaryExpression(annotations, unaryType, operand, method.ReturnType, method);
            }
            // try lifted call
            if (TypeUtils.IsNullableType(operandType)) {
                types[0] = nnOperandType;
                method = nnOperandType.GetMethod(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, types, null);
                if (method != null && method.ReturnType.IsValueType && !TypeUtils.IsNullableType(method.ReturnType)) {
                    return new UnaryExpression(annotations, unaryType, operand, TypeUtils.GetNullableType(method.ReturnType), method);
                }
            }
            return null;
        }
        //CONFORMING
        private static UnaryExpression GetMethodBasedUnaryOperator(ExpressionType unaryType, Expression operand, MethodInfo method, Annotations annotations) {
            System.Diagnostics.Debug.Assert(method != null);
            ValidateOperator(method);
            ParameterInfo[] pms = method.GetParametersCached();
            if (pms.Length != 1)
                throw Error.IncorrectNumberOfMethodCallArguments(method);
            if (ParameterIsAssignable(pms[0], operand.Type)) {
                ValidateParamswithOperandsOrThrow(pms[0].ParameterType, operand.Type, unaryType, method.Name);
                return new UnaryExpression(annotations, unaryType, operand, method.ReturnType, method);
            }
            // check for lifted call
            if (TypeUtils.IsNullableType(operand.Type) &&
                ParameterIsAssignable(pms[0], TypeUtils.GetNonNullableType(operand.Type)) &&
                method.ReturnType.IsValueType && !TypeUtils.IsNullableType(method.ReturnType)) {
                return new UnaryExpression(annotations, unaryType, operand, TypeUtils.GetNullableType(method.ReturnType), method);
            }

            throw Error.OperandTypesDoNotMatchParameters(unaryType, method.Name);
        }

        //CONFORMING
        private static UnaryExpression GetUserDefinedCoercionOrThrow(ExpressionType coercionType, Expression expression, Type convertToType, Annotations annotations) {
            UnaryExpression u = GetUserDefinedCoercion(coercionType, expression, convertToType, annotations);
            if (u != null) {
                return u;
            }
            throw Error.CoercionOperatorNotDefined(expression.Type, convertToType);
        }

        //CONFORMING
        private static UnaryExpression GetUserDefinedCoercion(ExpressionType coercionType, Expression expression, Type convertToType, Annotations annotations) {
            MethodInfo method = TypeUtils.GetUserDefinedCoercionMethod(expression.Type, convertToType, false);
            if (method != null) {
                return new UnaryExpression(annotations, coercionType, expression, convertToType, method);
            } else {
                return null;
            }
        }

        //CONFORMING
        private static UnaryExpression GetMethodBasedCoercionOperator(ExpressionType unaryType, Expression operand, Type convertToType, MethodInfo method, Annotations annotations) {
            System.Diagnostics.Debug.Assert(method != null);
            ValidateOperator(method);
            ParameterInfo[] pms = method.GetParametersCached();
            if (pms.Length != 1)
                throw Error.IncorrectNumberOfMethodCallArguments(method);
            if (ParameterIsAssignable(pms[0], operand.Type) && method.ReturnType == convertToType) {
                return new UnaryExpression(annotations, unaryType, operand, method.ReturnType, method);
            }
            // check for lifted call
            if ((TypeUtils.IsNullableType(operand.Type) || TypeUtils.IsNullableType(convertToType)) &&
                ParameterIsAssignable(pms[0], TypeUtils.GetNonNullableType(operand.Type)) &&
                method.ReturnType == TypeUtils.GetNonNullableType(convertToType)) {
                return new UnaryExpression(annotations, unaryType, operand, convertToType, method);
            }
            throw Error.OperandTypesDoNotMatchParameters(unaryType, method.Name);
        }

        //CONFORMING
        public static UnaryExpression Negate(Expression expression) {
            return Negate(expression, null, null);
        }
        //CONFORMING
        public static UnaryExpression Negate(Expression expression, MethodInfo method) {
            return Negate(expression, method, null);
        }
        public static UnaryExpression Negate(Expression expression, MethodInfo method, Annotations annotations) {
            RequiresCanRead(expression, "expression");
            if (method == null) {
                if (TypeUtils.IsArithmetic(expression.Type) && !TypeUtils.IsUnsignedInt(expression.Type)) {
                    return new UnaryExpression(annotations, ExpressionType.Negate, expression, expression.Type, null);
                }
                return GetUserDefinedUnaryOperatorOrThrow(ExpressionType.Negate, "op_UnaryNegation", expression, annotations);
            }
            return GetMethodBasedUnaryOperator(ExpressionType.Negate, expression, method, annotations);
        }

        //CONFORMING
        public static UnaryExpression UnaryPlus(Expression expression) {
            return UnaryPlus(expression, null, null);
        }
        //CONFORMING
        public static UnaryExpression UnaryPlus(Expression expression, MethodInfo method) {
            return UnaryPlus(expression, method, null);
        }
        public static UnaryExpression UnaryPlus(Expression expression, MethodInfo method, Annotations annotations) {
            RequiresCanRead(expression, "expression");
            if (method == null) {
                if (TypeUtils.IsArithmetic(expression.Type)) {
                    return new UnaryExpression(annotations, ExpressionType.UnaryPlus, expression, expression.Type, null);
                }
                return GetUserDefinedUnaryOperatorOrThrow(ExpressionType.UnaryPlus, "op_UnaryPlus", expression, annotations);
            }
            return GetMethodBasedUnaryOperator(ExpressionType.UnaryPlus, expression, method, annotations);
        }

        //CONFORMING
        public static UnaryExpression NegateChecked(Expression expression) {
            return NegateChecked(expression, null, null);
        }
        //CONFORMING
        public static UnaryExpression NegateChecked(Expression expression, MethodInfo method) {
            return NegateChecked(expression, method, null);
        }
        public static UnaryExpression NegateChecked(Expression expression, MethodInfo method, Annotations annotations) {
            RequiresCanRead(expression, "expression");
            if (method == null) {
                if (TypeUtils.IsArithmetic(expression.Type) && !TypeUtils.IsUnsignedInt(expression.Type)) {
                    return new UnaryExpression(annotations, ExpressionType.NegateChecked, expression, expression.Type, null);
                }
                return GetUserDefinedUnaryOperatorOrThrow(ExpressionType.NegateChecked, "op_UnaryNegation", expression, annotations);
            }
            return GetMethodBasedUnaryOperator(ExpressionType.NegateChecked, expression, method, annotations);
        }

        //CONFORMING
        public static UnaryExpression Not(Expression expression) {
            return Not(expression, null, null);
        }
        //CONFORMING
        public static UnaryExpression Not(Expression expression, MethodInfo method) {
            return Not(expression, method, null);
        }
        public static UnaryExpression Not(Expression expression, MethodInfo method, Annotations annotations) {
            RequiresCanRead(expression, "expression");
            if (method == null) {
                if (TypeUtils.IsIntegerOrBool(expression.Type)) {
                    return new UnaryExpression(annotations, ExpressionType.Not, expression, expression.Type, null);
                }
                UnaryExpression u = GetUserDefinedUnaryOperator(ExpressionType.Not, "op_LogicalNot", expression, annotations);
                if (u != null) {
                    return u;
                }
                return GetUserDefinedUnaryOperatorOrThrow(ExpressionType.Not, "op_OnesComplement", expression, annotations);
            }
            return GetMethodBasedUnaryOperator(ExpressionType.Not, expression, method, annotations);
        }

        //CONFORMING
        public static UnaryExpression TypeAs(Expression expression, Type type) {
            return TypeAs(expression, type, null);
        }
        public static UnaryExpression TypeAs(Expression expression, Type type, Annotations annotations) {
            RequiresCanRead(expression, "expression");
            ContractUtils.RequiresNotNull(type, "type");
            if (type.IsValueType && !TypeUtils.IsNullableType(type)) {
                throw Error.IncorrectTypeForTypeAs(type);
            }
            return new UnaryExpression(annotations, ExpressionType.TypeAs, expression, type, null);
        }

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
            return Convert(expression, type, null, null);
        }
        //CONFORMING
        public static UnaryExpression Convert(Expression expression, Type type, MethodInfo method) {
            return Convert(expression, type, method, null);
        }
        public static UnaryExpression Convert(Expression expression, Type type, MethodInfo method, Annotations annotations) {
            RequiresCanRead(expression, "expression");
            if (method == null) {
                ContractUtils.RequiresNotNull(type, "type");
                if (TypeUtils.HasIdentityPrimitiveOrNullableConversion(expression.Type, type) ||
                    TypeUtils.HasReferenceConversion(expression.Type, type)) {
                    return new UnaryExpression(annotations, ExpressionType.Convert, expression, type, null);
                }
                return GetUserDefinedCoercionOrThrow(ExpressionType.Convert, expression, type, annotations);
            }
            return GetMethodBasedCoercionOperator(ExpressionType.Convert, expression, type, method, annotations);
        }

        //CONFORMING
        public static UnaryExpression ConvertChecked(Expression expression, Type type) {
            return ConvertChecked(expression, type, null, null);
        }
        //CONFORMING
        public static UnaryExpression ConvertChecked(Expression expression, Type type, MethodInfo method) {
            return ConvertChecked(expression, type, method, null);
        }
        public static UnaryExpression ConvertChecked(Expression expression, Type type, MethodInfo method, Annotations annotations) {
            RequiresCanRead(expression, "expression");
            if (method == null) {
                ContractUtils.RequiresNotNull(type, "type");
                if (TypeUtils.HasIdentityPrimitiveOrNullableConversion(expression.Type, type)) {
                    return new UnaryExpression(annotations, ExpressionType.ConvertChecked, expression, type, null);
                }
                if (TypeUtils.HasReferenceConversion(expression.Type, type)) {
                    return new UnaryExpression(annotations, ExpressionType.Convert, expression, type, null);
                }
                return GetUserDefinedCoercionOrThrow(ExpressionType.ConvertChecked, expression, type, annotations);
            }
            return GetMethodBasedCoercionOperator(ExpressionType.ConvertChecked, expression, type, method, annotations);
        }

        //CONFORMING
        public static UnaryExpression ArrayLength(Expression array) {
            return ArrayLength(array, null);
        }
        public static UnaryExpression ArrayLength(Expression array, Annotations annotations) {
            ContractUtils.RequiresNotNull(array, "array");
            if (!array.Type.IsArray || !TypeUtils.AreAssignable(typeof(Array), array.Type)) {
                throw Error.ArgumentMustBeArray();
            }
            if (array.Type.GetArrayRank() != 1) {
                throw Error.ArgumentMustBeSingleDimensionalArrayType();
            }
            return new UnaryExpression(annotations, ExpressionType.ArrayLength, array, typeof(int), null);
        }

        //CONFORMING
        public static UnaryExpression Quote(Expression expression) {
            return Quote(expression, null);
        }
        public static UnaryExpression Quote(Expression expression, Annotations annotations) {
            RequiresCanRead(expression, "expression");
            return new UnaryExpression(annotations, ExpressionType.Quote, expression, expression.GetType(), null);
        }

        [Obsolete("use Expression.Convert or Utils.Convert instead")]
        public static Expression ConvertHelper(Expression expression, Type type) {
            RequiresCanRead(expression, "expression");
            ContractUtils.RequiresNotNull(type, "type");

            if (expression.Type != type) {
                expression = Convert(expression, type);
            }
            return expression;
        }

        // TODO: should we just always wrap it in a convert?
        // Do we need this factory at all?
        public static Expression Void(Expression expression) {
            RequiresCanRead(expression, "expression");
            if (expression.Type == typeof(void)) {
                return expression;
            }
            return Expression.Convert(expression, typeof(void));
        }

        public static UnaryExpression Rethrow() {
            return Throw(null);
        }

        public static UnaryExpression Throw(Expression value) {
            return Throw(value, typeof(void), Annotations.Empty);
        }

        public static UnaryExpression Throw(Expression value, Type type) {
            return Throw(value, type, Annotations.Empty);
        }

        public static UnaryExpression Throw(Expression value, Type type, Annotations annotations) {
            ContractUtils.RequiresNotNull(type, "type");

            if (value != null) {
                RequiresCanRead(value, "value");
                ContractUtils.Requires(
                    TypeUtils.AreReferenceAssignable(typeof(Exception), value.Type),
                    "value",
                    Strings.ArgumentMustBeException
                );
            }
            return new UnaryExpression(annotations, ExpressionType.Throw, value, type, null);
        }
    }
}
