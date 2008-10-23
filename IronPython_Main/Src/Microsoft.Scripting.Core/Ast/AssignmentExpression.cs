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
using System.Text;

namespace Microsoft.Linq.Expressions {
    /// <summary>
    /// Represents assignment: Expression = Value. The left had side
    /// must be a valid lvalue:
    ///   ParameterExpression
    ///   MemberExpression with writable property/field
    ///   IndexExpression
    ///   
    /// TODO: merge into BinaryExpression
    /// </summary>
    public sealed class AssignmentExpression : Expression {
        private readonly Expression _expression;
        private readonly Expression _value;

        internal AssignmentExpression(Annotations annotations, Expression expression, Expression value)
            : base(annotations) {
            _expression = expression;
            _value = value;
        }

        protected override Type GetExpressionType() {
            return Expression.Type;
        }

        protected override ExpressionType GetNodeKind() {
            return ExpressionType.Assign;
        }

        internal override Expression.NodeFlags GetFlags() {
            return NodeFlags.CanRead;
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

        internal override Expression Accept(ExpressionTreeVisitor visitor) {
            return visitor.VisitAssignment(this);
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
            RequiresCanWrite(left, "left");
            RequiresCanRead(right, "right");
            TypeUtils.ValidateType(left.Type);
            TypeUtils.ValidateType(right.Type);
            if (!TypeUtils.AreReferenceAssignable(left.Type, right.Type)) {
                throw Error.ExpressionTypeDoesNotMatchAssignment(right.Type, left.Type);
            }
            return new AssignmentExpression(annotations, left, right);
        }

        // TODO: remove?
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
            return Assign(Field(expression, field), value);
        }

        // TODO: remove?
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
            return Assign(Property(expression, property), value);
        }

        // TODO: remove or rename to 'AssignArrayAccess', allow multiple indexes
        public static AssignmentExpression AssignArrayIndex(Expression array, Expression index, Expression value) {
            return Assign(ArrayAccess(array, index), value);
        }
    }
}
