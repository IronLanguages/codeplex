/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
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
using Microsoft.Scripting.Utils;

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

        public override Type Type {
            get {
                return typeof(void);
            }
        }

        internal MemberAssignment(SourceSpan span, MemberInfo member, Expression expression, Expression value)
            : base(span) {
            _member = member;
            _expression = expression;
            _value = value;
        }

        protected override object DoEvaluate(CodeContext context) {
            object target = _expression != null ? _expression.Evaluate(context) : null;
            object value = _value.Evaluate(context);
            
            switch (_member.MemberType) {
                case MemberTypes.Field:
                    FieldInfo field = (FieldInfo)_member;
                    field.SetValue(target, value);
                    break;
                case MemberTypes.Property:
                    PropertyInfo property = (PropertyInfo)_member;
                    property.SetValue(target, value, null);
                    break;
                default:
                    Debug.Assert(false, "Invalid member type");
                    break;
            }
            return null;
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
    }

    /// <summary>
    /// Factory methods.
    /// </summary>
    public static partial class Ast {
        public static MemberAssignment AssignField(Expression expression, Type type, string field, Expression value) {
            return AssignField(SourceSpan.None, expression, type, field, value);
        }

        public static MemberAssignment AssignField(SourceSpan span, Expression expression, Type type, string field, Expression value) {
            Contract.RequiresNotNull(type, "type");
            Contract.RequiresNotNull(field, "field");
            Contract.RequiresNotNull(value, "value");

            FieldInfo fi = type.GetField(field);
            if (fi == null) {
                throw new ArgumentException(String.Format("Type {0} doesn't have field {1}", type, field));
            }
            if (expression == null ^ fi.IsStatic) {
                throw new ArgumentNullException("Static field requires null expression, non-static field requires non-null expression.");
            }
            return new MemberAssignment(span, fi, expression, value);
        }

        public static MemberAssignment AssignField(Expression expression, FieldInfo field, Expression value) {
            return AssignField(SourceSpan.None, expression, field, value);
        }
        /// <summary>
        /// Creates MemberExpression representing field access, instance or static.
        /// For static field, expression must be null and FieldInfo.IsStatic == true
        /// For instance field, expression must be non-null and FieldInfo.IsStatic == false.
        /// </summary>
        /// <param name="span">The source code span of the expression.</param>
        /// <param name="expression">Expression that evaluates to the instance for the field access.</param>
        /// <param name="field">Field represented by this Member expression.</param>
        /// <param name="value">Value to set this field to.</param>
        /// <returns>New instance of Member expression</returns>
        public static MemberAssignment AssignField(SourceSpan span, Expression expression, FieldInfo field, Expression value) {
            Contract.RequiresNotNull(field, "field");
            Contract.RequiresNotNull(value, "value");

            if (expression == null ^ field.IsStatic) {
                throw new ArgumentException("field");
            }

            return new MemberAssignment(span, field, expression, value);
        }

        public static MemberAssignment AssignProperty(Expression expression, Type type, string property, Expression value) {
            return AssignProperty(SourceSpan.None, expression, type, property, value);
        }

        public static MemberAssignment AssignProperty(SourceSpan span, Expression expression, Type type, string property, Expression value) {
            Contract.RequiresNotNull(type, "type");
            Contract.RequiresNotNull(property, "property");
            Contract.RequiresNotNull(value, "value");

            PropertyInfo pi = type.GetProperty(property);
            if (pi == null) {
                throw new ArgumentException(String.Format("Type {0} doesn't have property {1}", type, property));
            }
            if (!pi.CanWrite) {
                throw new ArgumentException(String.Format("Cannot assign property {0}.{1}", pi.DeclaringType, pi.Name));
            }
            if (expression == null ^ pi.GetSetMethod().IsStatic) {
                throw new ArgumentNullException("Static property requires null target, non-static property requires non-null target.");
            }
            return new MemberAssignment(span, pi, expression, value);
        }

        public static MemberAssignment AssignProperty(Expression expression, PropertyInfo property, Expression value) {
            return AssignProperty(SourceSpan.None, expression, property, value);
        }

        /// <summary>
        /// Creates MemberExpression representing property access, instance or static.
        /// For static properties, expression must be null and property.IsStatic == true.
        /// For instance properties, expression must be non-null and property.IsStatic == false.
        /// </summary>
        /// <param name="span">The source code span of the expression.</param>
        /// <param name="expression">Expression that evaluates to the instance for instance property access.</param>
        /// <param name="property">PropertyInfo of the property to access</param>
        /// <param name="value">Value to set this property to.</param>
        /// <returns>New instance of the MemberExpression.</returns>
        public static MemberAssignment AssignProperty(SourceSpan span, Expression expression, PropertyInfo property, Expression value) {
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

            return new MemberAssignment(SourceSpan.None, property, expression, value);
        }
    }
}
