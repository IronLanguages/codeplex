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
using System.Scripting.Actions;
using System.Scripting.Utils;
using System.Text;

namespace System.Linq.Expressions {
    /// <summary>
    /// Represents assignment: Expression = Value. The left had side
    /// must be a valid lvalue:
    ///   VariableExpression
    ///   ParameterExpression
    ///   MemberExpression with writable property/field
    ///   BinaryExpression with NodeType == ArrayIndex
    ///   (future) IndexedPropertyExpression
    /// </summary>
    public sealed class AssignmentExpression : Expression {
        private readonly Expression _expression;
        private readonly Expression _value;

        internal AssignmentExpression(Annotations annotations, Expression expression, Expression value)
            : this(annotations, expression, value, expression.Type, null) {
        }

        internal AssignmentExpression(Annotations annotations, Expression expression, Expression value, Type result, CallSiteBinder bindingInfo)
            : base(annotations, ExpressionType.Assign, result, bindingInfo) {
            if (IsBound) {
                RequiresBound(expression, "expression");
                RequiresBound(value, "value");
            }
            _expression = expression;
            _value = value;
        }

        public Expression Expression {
            get { return _expression; }
        }

        public Expression Value {
            get { return _value; }
        }

        internal override void BuildString(StringBuilder builder) {
            ContractUtils.RequiresNotNull(builder, "builder");

            _expression.BuildString(builder);
            builder.Append(" = ");
            _value.BuildString(builder);
        }
    }

    public partial class Expression {

        public static AssignmentExpression Assign(Expression left, Expression right) {
            return Assign(left, right, Annotations.Empty);
        }

        /// <summary>
        /// Performs an assignment variable = value
        /// </summary>
        public static AssignmentExpression Assign(Expression left, Expression right, Annotations annotations) {
            CheckLeftValue(left, right);
            return new AssignmentExpression(annotations, left, right);
        }

        /// <summary>
        /// Creates MemberExpression representing field access, instance or static.
        /// For static field, expression must be null and FieldInfo.IsStatic == true
        /// For instance field, expression must be non-null and FieldInfo.IsStatic == false.
        /// </summary>
        /// <param name="expression">Expression that evaluates to the instance for the field access.</param>
        /// <param name="field">Field represented by this Member expression.</param>
        /// <param name="value">Value to set this field to.</param>
        /// <returns>New instance of Member expression</returns>
        public static AssignmentExpression AssignField(Expression expression, FieldInfo field, Expression value) {
            CheckField(field, expression, value);
            return new AssignmentExpression(Annotations.Empty, new MemberExpression(Annotations.Empty, field, expression, field.FieldType, null), value);
        }

        /// <summary>
        /// Creates MemberExpression representing property access, instance or static.
        /// For static properties, expression must be null and property.IsStatic == true.
        /// For instance properties, expression must be non-null and property.IsStatic == false.
        /// </summary>
        /// <param name="expression">Expression that evaluates to the instance for instance property access.</param>
        /// <param name="property">PropertyInfo of the property to access</param>
        /// <param name="value">Value to set this property to.</param>
        /// <returns>New instance of the MemberExpression.</returns>
        public static AssignmentExpression AssignProperty(Expression expression, PropertyInfo property, Expression value) {
            CheckProperty(property, expression, value);
            return new AssignmentExpression(Annotations.Empty, new MemberExpression(Annotations.Empty, property, expression, property.PropertyType, null), value);
        }

        public static AssignmentExpression AssignArrayIndex(Expression array, Expression index, Expression value) {
            ContractUtils.RequiresNotNull(array, "array");
            ContractUtils.RequiresNotNull(index, "index");
            ContractUtils.RequiresNotNull(value, "value");
            ContractUtils.Requires(index.Type == typeof(int), "index", Strings.ArrayIndexMustBeInt);

            Type arrayType = array.Type;
            ContractUtils.Requires(arrayType.IsArray, "array", Strings.ArrayArgumentMustBeArray);
            ContractUtils.Requires(arrayType.GetArrayRank() == 1, "index", Strings.IncorrectIndicesCount);
            ContractUtils.Requires(value.Type == arrayType.GetElementType(), "value", Strings.ValueTypeMustMatchElementType);

            return new AssignmentExpression(Annotations.Empty, new BinaryExpression(Annotations.Empty, ExpressionType.ArrayIndex, array, index, array.Type.GetElementType()), value);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static AssignmentExpression AssignArrayIndex(Expression array, Expression index, Expression value, Type result, CallSiteBinder bindingInfo, Annotations annotations) {
            ContractUtils.RequiresNotNull(array, "array");
            ContractUtils.RequiresNotNull(index, "index");
            ContractUtils.RequiresNotNull(value, "value");
            ContractUtils.RequiresNotNull(bindingInfo, "bindingInfo");

            // TODO: don't pass bindinginfo to nested MemberExpression
            return new AssignmentExpression(
                annotations,
                new BinaryExpression(Annotations.Empty, ExpressionType.ArrayIndex, array, index, result, bindingInfo),
                value,
                result,
                bindingInfo
            );
        }

        public static AssignmentExpression SetMember(Expression expression, Type result, OldSetMemberAction bindingInfo, Expression value) {
            return SetMember(expression, result, bindingInfo, value, Annotations.Empty);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static AssignmentExpression SetMember(Expression expression, Type result, OldSetMemberAction bindingInfo, Expression value, Annotations annotations) {
            ContractUtils.RequiresNotNull(expression, "expression");
            ContractUtils.RequiresNotNull(bindingInfo, "bindingInfo");
            ContractUtils.RequiresNotNull(expression, "value");

            // TODO: don't pass bindinginfo to nested MemberExpression
            return new AssignmentExpression(annotations, new MemberExpression(Annotations.Empty, null, expression, expression.Type, bindingInfo), value, result, bindingInfo);
        }

        internal static void CheckLeftValue(Expression left, Expression right) {
            ContractUtils.RequiresNotNull(left, "left");
            ContractUtils.RequiresNotNull(right, "right");

            switch (left.NodeType) {
                case ExpressionType.Variable:
                case ExpressionType.Parameter:
                case ExpressionType.ArrayIndex:
                    ContractUtils.Requires(TypeUtils.CanAssign(left.Type, right), "right", Strings.RhsMustBeAssignableToLhs);
                    return;
                case ExpressionType.MemberAccess:
                    MemberExpression me = (MemberExpression)left;
                    MemberInfo mi = me.Member;
                    if (mi.MemberType == MemberTypes.Field) {
                        CheckField((FieldInfo)mi, me.Expression, right);
                        return;
                    } else if (mi.MemberType == MemberTypes.Property) {
                        CheckProperty((PropertyInfo)mi, me.Expression, right);
                        return;
                    }
                    break;
                case ExpressionType.IndexedProperty:
                    IndexedPropertyExpression ip = (IndexedPropertyExpression)left;
                    CheckPropertySet(ip.SetMethod, ip.Object, right);
                    return;
                case ExpressionType.Extension:
                    // Allow reduicble nodes to be lvalues
                    // TODO: can we do some check that it is in fact a valid lvalue?
                    return;
                default:
                    break;
            }

            throw Error.LhsUnassignable();
        }
    }
}
