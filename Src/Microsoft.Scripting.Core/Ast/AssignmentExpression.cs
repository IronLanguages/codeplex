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
using System.Diagnostics;
using System.Reflection;

using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    /// <summary>
    /// Represents assignment: Expression = Value. The left had side
    /// must be a valid lvalue:
    ///   MemberExpression with writable property/field
    ///   BinaryExpression with NodeType == ArrayIndex
    ///   VariableExpression
    ///   ParameterExpression
    /// </summary>
    public sealed class AssignmentExpression : Expression {
        private readonly Expression/*!*/ _expression;
        private readonly Expression/*!*/ _value;

        internal AssignmentExpression(Annotations annotations, Expression/*!*/ expression, Expression/*!*/ value)
            : this(annotations, expression, value, expression.Type, null) {
        }

        internal AssignmentExpression(Annotations annotations, Expression/*!*/ expression, Expression/*!*/ value, Type result, DynamicAction bindingInfo)
            : base(annotations, AstNodeType.Assign, result, bindingInfo) {
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
    }

    public partial class Expression {
        /// <summary>
        /// Performs an assignment variable = value
        /// TODO: remove in favor of Assign
        /// </summary>
        public static Expression Write(Expression variable, Expression value) {
            return Assign(variable, value);
        }

        /// <summary>
        /// Performs an assignment variable.field = value
        /// </summary>
        public static Expression Write(Expression variable, FieldInfo field, Expression value) {
            return AssignField(variable, field, value);
        }

        /// <summary>
        /// Performs an assignment variable = right.field
        /// </summary>
        public static Expression Write(Expression variable, Expression right, FieldInfo field) {
            return Assign(variable, ReadField(right, field));
        }

        /// <summary>
        /// Performs an assignment variable.leftField = right.rightField
        /// </summary>
        public static Expression Write(Expression variable, FieldInfo leftField, Expression right, FieldInfo rightField) {
            return AssignField(variable, leftField, ReadField(right, rightField));
        }

        public static AssignmentExpression Assign(Expression variable, Expression value) {
            return Assign(Annotations.Empty, variable, value);
        }

        public static AssignmentExpression Assign(SourceSpan span, Expression variable, Expression value) {
            return Assign(Annotate(span), variable, value);
        }

        /// <summary>
        /// Performs an assignment variable = value
        /// </summary>
        public static AssignmentExpression Assign(Annotations annotations, Expression variable, Expression value) {
            ContractUtils.RequiresNotNull(variable, "variable");
            ContractUtils.RequiresNotNull(value, "value");
            ContractUtils.Requires(TypeUtils.CanAssign(variable.Type, value));
            ContractUtils.Requires(variable is VariableExpression || variable is ParameterExpression, "variable", "variable must be VariableExpression or ParameterExpression");
            return new AssignmentExpression(annotations, variable, value);
        }

        public static AssignmentExpression AssignField(Expression expression, Type type, string field, Expression value) {
            return AssignField(expression, GetFieldChecked(type, field, expression, value), value);
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
            return new AssignmentExpression(Annotations.Empty, new MemberExpression(field, expression, field.FieldType, null), value);
        }

        public static AssignmentExpression AssignProperty(Expression expression, Type type, string property, Expression value) {
            return AssignProperty(expression, GetPropertyChecked(type, property, expression, value), value);
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
            return new AssignmentExpression(Annotations.Empty, new MemberExpression(property, expression, property.PropertyType, null), value);
        }

        public static AssignmentExpression AssignArrayIndex(Expression array, Expression index, Expression value) {
            ContractUtils.RequiresNotNull(array, "array");
            ContractUtils.RequiresNotNull(index, "index");
            ContractUtils.RequiresNotNull(value, "value");
            ContractUtils.Requires(index.Type == typeof(int), "index", "Array index must be an int.");

            Type arrayType = array.Type;
            ContractUtils.Requires(arrayType.IsArray, "array", "Array argument must be array.");
            ContractUtils.Requires(arrayType.GetArrayRank() == 1, "index", "Incorrect number of indices.");
            ContractUtils.Requires(value.Type == arrayType.GetElementType(), "value", "Value type must match the array element type.");

            return new AssignmentExpression(Annotations.Empty, new BinaryExpression(AstNodeType.ArrayIndex, array, index, array.Type.GetElementType(), null), value);
        }

        public static AssignmentExpression SetMember(Expression expression, Type result, SetMemberAction bindingInfo, Expression value) {
            return SetMember(Annotations.Empty, expression, result, bindingInfo, value);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static AssignmentExpression SetMember(Annotations annotations, Expression expression, Type result, SetMemberAction bindingInfo, Expression value) {
            ContractUtils.RequiresNotNull(expression, "expression");
            ContractUtils.RequiresNotNull(bindingInfo, "bindingInfo");
            ContractUtils.RequiresNotNull(expression, "value");

            // TODO: don't pass bindinginfo to nested MemberExpression
            return new AssignmentExpression(annotations, new MemberExpression(null, expression, expression.Type, bindingInfo), value, result, bindingInfo);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static AssignmentExpression AssignArrayIndex(Annotations annotations, Expression array, Expression index, Expression value, Type result, DoOperationAction bindingInfo) {
            ContractUtils.RequiresNotNull(annotations, "annotations");
            ContractUtils.RequiresNotNull(array, "array");
            ContractUtils.RequiresNotNull(index, "index");
            ContractUtils.RequiresNotNull(value, "value");
            ContractUtils.RequiresNotNull(bindingInfo, "bindingInfo");

            // TODO: don't pass bindinginfo to nested MemberExpression
            return new AssignmentExpression(
                annotations,
                new BinaryExpression(AstNodeType.ArrayIndex, Annotations.Empty, array, index, result, null, bindingInfo),
                value,
                result,
                bindingInfo
            );
        }
    }
}
