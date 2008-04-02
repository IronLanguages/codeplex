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

namespace Microsoft.Scripting.Ast {
    /// <summary>
    /// Member expression (statically typed) which represents 
    /// property or field set, both static and instance.
    /// For instance property/field, Expression must be != null.
    /// </summary>
    public sealed class MemberAssignment : Expression {
        private readonly MemberInfo /*!*/ _member;
        private readonly Expression _expression;
        private readonly Expression /*!*/ _value;

        public MemberInfo Member {
            get { return _member; }
        }

        public Expression Expression {
            get { return _expression; }
        }

        public Expression Value {
            get { return _value; }
        }

        internal MemberAssignment(MemberInfo /*!*/ member, Expression expression, Expression /*!*/ value)
            : base(AstNodeType.MemberAssignment, typeof(void)) {        // TODO: Not a void type !!!
            _member = member;
            _expression = expression;
            _value = value;
        }
    }

    /// <summary>
    /// Factory methods.
    /// </summary>
    public static partial class Ast {
        public static MemberAssignment AssignField(Expression expression, Type type, string field, Expression value) {
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
        public static MemberAssignment AssignField(Expression expression, FieldInfo field, Expression value) {
            CheckField(field, expression, value);
            return new MemberAssignment(field, expression, value);
        }

        public static MemberAssignment AssignProperty(Expression expression, Type type, string property, Expression value) {
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
        public static MemberAssignment AssignProperty(Expression expression, PropertyInfo property, Expression value) {
            CheckProperty(property, expression, value);
            return new MemberAssignment(property, expression, value);
        }
    }
}
