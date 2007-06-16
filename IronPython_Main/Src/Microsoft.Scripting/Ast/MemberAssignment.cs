/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Ast {
    /// <summary>
    /// Member expression (statically typed) which represents 
    /// property or field set, both static and instance.
    /// For instance property/field, Expression must be != null.
    /// </summary>
    public class MemberAssignment : Expression {
        private readonly MemberInfo _member;
        private readonly Expression _expression;
        private readonly Expression _value;

        public MemberInfo Member {
            get { return _member; }
        }

        public Expression Expression {
            get { return _expression; }
        }

        public Expression Value {
            get { return _value; }
        }

        public override Type ExpressionType {
            get {
                return typeof(void);
            }
        }

        private MemberAssignment(MemberInfo member, Expression expression, Expression value)
            : base(SourceSpan.None) {
            _member = member;
            _expression = expression;
            _value = value;
        }

        public override void Emit(CodeGen cg) {
            // emit "this", if any
            if (_expression != null) {
                if (_member.DeclaringType.IsValueType) {
                    _expression.EmitAddress(cg, _member.DeclaringType);
                } else {
                    _expression.EmitAs(cg, _member.DeclaringType);
                }
            }

            switch (_member.MemberType) {
                case MemberTypes.Field:
                    FieldInfo field = (FieldInfo)_member;
                    _value.EmitAs(cg, field.FieldType);
                    cg.EmitFieldSet(field);
                    break;                    
                case MemberTypes.Property:
                    PropertyInfo property = (PropertyInfo)_member;
                    _value.EmitAs(cg, property.PropertyType);
                    cg.EmitPropertySet(property);
                    break;
                default:
                    Debug.Assert(false, "Invalid member type");
                    break;
            }
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                if (_expression != null) {
                    _expression.Walk(walker);
                }
                _value.Walk(walker);
            }
            walker.PostWalk(this);
        }

        #region Factory methods

        /// <summary>
        /// Creates MemberExpression representing field access, instance or static.
        /// For static field, expression must be null and FieldInfo.IsStatic == true
        /// For instance field, expression must be non-null and FieldInfo.IsStatic == false.
        /// </summary>
        /// <param name="expression">Expression that evaluates to the instance for the field access.</param>
        /// <param name="field">Field represented by this Member expression.</param>
        /// <param name="value">Value to set this field to.</param>
        /// <returns>New instance of Member expression</returns>
        public static MemberAssignment Field(Expression expression, FieldInfo field, Expression value) {
            if (field == null) {
                throw new ArgumentNullException("field");
            }
            if (value == null) {
                throw new ArgumentNullException("value");
            }

            if (expression == null ^ field.IsStatic) {
                throw new ArgumentException("field");
            }

            return new MemberAssignment(field, expression, value);
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
        public static MemberAssignment Property(Expression expression, PropertyInfo property, Expression value) {
            if (property == null) {
                throw new ArgumentNullException("property");
            }

            if (value == null) {
                throw new ArgumentNullException("value");
            }

            MethodInfo setter = property.GetSetMethod();
            if (setter == null) {
                throw new ArgumentNullException("property");
            }

            if (expression == null ^ setter.IsStatic) {
                throw new ArgumentException("property");
            }

            return new MemberAssignment(property, expression, value);
        }

        #endregion
    }
}
