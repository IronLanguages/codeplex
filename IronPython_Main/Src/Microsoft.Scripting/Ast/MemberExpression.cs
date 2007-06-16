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
    /// property or field access, both static and instance.
    /// For instance property/field, Expression must be != null.
    /// </summary>
    public class MemberExpression : Expression {
        private readonly MemberInfo _member;
        private readonly Expression _expression;

        public MemberInfo Member {
            get { return _member; }
        }

        public Expression Expression {
            get { return _expression; }
        }

        public override Type ExpressionType {
            get {
                switch (_member.MemberType) {
                    case MemberTypes.Field:
                        return ((FieldInfo)_member).FieldType;
                    case MemberTypes.Property:
                        return ((PropertyInfo)_member).PropertyType;
                    default:
                        Debug.Assert(false, "Invalid member type");
                        return null;
                }
            }
        }

        private MemberExpression(MemberInfo member, Expression expression)
            : base(SourceSpan.None) {
            _member = member;
            _expression = expression;
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
                    cg.EmitFieldGet(field);
                    break;                    
                case MemberTypes.Property:
                    PropertyInfo property = (PropertyInfo)_member;
                    cg.EmitPropertyGet(property);
                    break;
                default:
                    Debug.Assert(false, "Invalid member type");
                    break;
            }
        }

        public override object Evaluate(CodeContext context) {
            object self = _expression != null ? _expression.Evaluate(context) : null;
            switch (_member.MemberType) {
                case MemberTypes.Field:
                    FieldInfo field = (FieldInfo)_member;
                    return field.GetValue(self);
                case MemberTypes.Property:                    
                    PropertyInfo property = (PropertyInfo)_member;
                    return property.GetValue(self, new object[0]);
                default:
                    Debug.Assert(false, "Invalid member type");
                    break;
            }

            throw new InvalidOperationException();
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                if (_expression != null) {
                    _expression.Walk(walker);
                }
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
        /// <returns>New instance of Member expression</returns>
        public static MemberExpression Field(Expression expression, FieldInfo field) {
            if (field == null) {
                throw new ArgumentNullException("field");
            }
            if (expression == null ^ field.IsStatic) {
                throw new ArgumentException("field");
            }

            return new MemberExpression(field, expression);
        }

        /// <summary>
        /// Creates MemberExpression representing property access, instance or static.
        /// For static properties, expression must be null and property.IsStatic == true.
        /// For instance properties, expression must be non-null and property.IsStatic == false.
        /// </summary>
        /// <param name="expression">Expression that evaluates to the instance for instance property access.</param>
        /// <param name="property">PropertyInfo of the property to access</param>
        /// <returns>New instance of the MemberExpression.</returns>
        public static MemberExpression Property(Expression expression, PropertyInfo property) {
            if (property == null) {
                throw new ArgumentNullException("property");
            }

            MethodInfo getter = property.GetGetMethod();
            if (getter == null) {
                throw new ArgumentNullException("property");
            }

            if (expression == null ^ getter.IsStatic) {
                throw new ArgumentException("property");
            }

            return new MemberExpression(property, expression);
        }

        /// <summary>
        /// Creates MemberExpression representing member access.
        /// </summary>
        /// <param name="expression">Expression representing instance (for instance access).</param>
        /// <param name="member">Member info for the desired member.</param>
        /// <returns>New instance of MemberExpression representing member access.</returns>
        public static MemberExpression MakeMemberAccess(Expression expression, MemberInfo member) {
            FieldInfo field = member as FieldInfo;
            if (field != null) {
                return Field(expression, field);
            }

            PropertyInfo property = member as PropertyInfo;
            if (property != null) {
                return Property(expression, property);
            }

            throw new ArgumentException("member");
        }

        #endregion
    }
}
