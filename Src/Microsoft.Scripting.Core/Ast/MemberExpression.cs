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
    //CONFORMING
    /// <summary>
    /// Member expression (statically typed) which represents 
    /// property or field access, both static and instance.
    /// For instance property/field, Expression must be != null.
    /// </summary>
    public sealed class MemberExpression : Expression {
        private readonly MemberInfo _member;
        private readonly Expression _expression;

        public MemberInfo Member {
            get { return _member; }
        }

        public Expression Expression {
            get { return _expression; }
        }

        // param order: factories args in order, then other args
        internal MemberExpression(Expression expression, MemberInfo member, Annotations annotations, Type type, bool canRead, bool canWrite)
            : base(ExpressionType.MemberAccess, type, false, annotations, canRead, canWrite) {

            _member = member;
            _expression = expression;
        }

        internal override void BuildString(StringBuilder builder) {
            ContractUtils.RequiresNotNull(builder, "builder");

            if (_expression != null) {
                _expression.BuildString(builder);
            } else {
                builder.Append(_member.DeclaringType.Name);
            }
            builder.Append(".");
            builder.Append(_member.Name);
        }
    }

    /// <summary>
    /// Factory methods.
    /// </summary>
    public partial class Expression {

        #region Field

        //CONFORMING
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames")]
        public static MemberExpression Field(Expression expression, FieldInfo field) {
            return Field(expression, field, Annotations.Empty);
        }

        //CONFORMING
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames")]
        public static MemberExpression Field(Expression expression, FieldInfo field, Annotations annotations) {
            ContractUtils.RequiresNotNull(field, "field");
            ContractUtils.RequiresNotNull(annotations, "annotations");

            if (field.IsStatic) {
                ContractUtils.Requires(expression == null, "expression", Strings.OnlyStaticFieldsHaveNullExpr);
            } else {
                RequiresCanRead(expression, "expression");
                if (!TypeUtils.AreReferenceAssignable(field.DeclaringType, expression.Type)) {
                    throw Error.FieldNotDefinedForType(field, expression.Type);
                }
            }
            return new MemberExpression(expression, field, annotations, field.FieldType, true, !(field.IsLiteral | field.IsInitOnly));
        }

        //CONFORMING
        public static MemberExpression Field(Expression expression, string fieldName) {
            return Field(expression, fieldName, Annotations.Empty);
        }

        //CONFORMING
        public static MemberExpression Field(Expression expression, string fieldName, Annotations annotations) {
            RequiresCanRead(expression, "expression");
            ContractUtils.RequiresNotNull(annotations, "annotations");

            // bind to public names first
            FieldInfo fi = expression.Type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.FlattenHierarchy);
            if (fi == null) {
                fi = expression.Type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.IgnoreCase | BindingFlags.FlattenHierarchy);
            }
            if (fi == null) {
                throw Error.FieldNotDefinedForType(fieldName, expression.Type);
            }
            return Expression.Field(expression, fi, annotations);
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames")]
        public static MemberExpression Field(Expression expression, Type type, string fieldName) {
            ContractUtils.RequiresNotNull(type, "type");

            // bind to public names first
            FieldInfo fi = type.GetField(fieldName, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.FlattenHierarchy);
            if (fi == null) {
                fi = type.GetField(fieldName, BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.IgnoreCase | BindingFlags.FlattenHierarchy);
            }

            if (fi == null) {
                throw Error.FieldNotDefinedForType(fieldName, type);
            }
            return Expression.Field(expression, fi);
        }
        #endregion

        #region Property

        //CONFORMING
        public static MemberExpression Property(Expression expression, string propertyName) {
            RequiresCanRead(expression, "expression");
            // bind to public names first
            PropertyInfo pi = expression.Type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.FlattenHierarchy);
            if (pi == null) {
                pi = expression.Type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.IgnoreCase | BindingFlags.FlattenHierarchy);
            }
            if (pi == null) {
                throw Error.PropertyNotDefinedForType(propertyName, expression.Type);
            }
            return Property(expression, pi);
        }

        public static MemberExpression Property(Expression expression, Type type, string propertyName) {
            ContractUtils.RequiresNotNull(type, "type");
            // bind to public names first
            PropertyInfo pi = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.FlattenHierarchy);
            if (pi == null) {
                pi = type.GetProperty(propertyName, BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.IgnoreCase | BindingFlags.FlattenHierarchy);
            }
            if (pi == null) {
                throw Error.PropertyNotDefinedForType(propertyName, type);
            }
            return Property(expression, pi);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames")]
        public static MemberExpression Property(Expression expression, PropertyInfo property) {
            return Property(expression, property, Annotations.Empty);
        }

        //CONFORMING
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames")]
        public static MemberExpression Property(Expression expression, PropertyInfo property, Annotations annotations) {
            ContractUtils.RequiresNotNull(property, "property");

            if (property.GetAccessors(true)[0].IsStatic) {
                ContractUtils.Requires(expression == null, "expression", Strings.OnlyStaticPropertiesHaveNullExpr); 
            } else {
                RequiresCanRead(expression, "expression");
                if (!TypeUtils.IsValidInstanceType(property, expression.Type)) {
                    throw Error.PropertyNotDefinedForType(property, expression.Type);
                }
            }
            return new MemberExpression(expression, property, annotations, property.PropertyType, property.CanRead, property.CanWrite);
        }
        //CONFORMING
        public static MemberExpression Property(Expression expression, MethodInfo propertyAccessor) {
            ContractUtils.RequiresNotNull(propertyAccessor, "propertyAccessor");
            ValidateMethodInfo(propertyAccessor);
            return Property(expression, GetProperty(propertyAccessor));
        }

        //CONFORMING
        private static PropertyInfo GetProperty(MethodInfo mi) {
            Type type = mi.DeclaringType;
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic;
            flags |= (mi.IsStatic) ? BindingFlags.Static : BindingFlags.Instance;
            PropertyInfo[] props = type.GetProperties(flags);
            foreach (PropertyInfo pi in props) {
                if (pi.CanRead && CheckMethod(mi, pi.GetGetMethod(true))) {
                    return pi;
                }
                if (pi.CanWrite && CheckMethod(mi, pi.GetSetMethod(true))) {
                    return pi;
                }
            }
            throw Error.MethodNotPropertyAccessor(mi.DeclaringType, mi.Name);
        }

        //CONFORMING
        private static bool CheckMethod(MethodInfo method, MethodInfo propertyMethod) {
            if (method == propertyMethod) {
                return true;
            }
            // If the type is an interface then the handle for the method got by the compiler will not be the 
            // same as that returned by reflection.
            // Check for this condition and try and get the method from reflection.
            Type type = method.DeclaringType;
            if (type.IsInterface && method.Name == propertyMethod.Name && type.GetMethod(method.Name) == propertyMethod) {
                return true;
            }
            return false;
        }

        #endregion

        //CONFORMING
        public static MemberExpression PropertyOrField(Expression expression, string propertyOrFieldName) {
            RequiresCanRead(expression, "expression");
            // bind to public names first
            PropertyInfo pi = expression.Type.GetProperty(propertyOrFieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.FlattenHierarchy);
            if (pi != null)
                return Property(expression, pi);
            FieldInfo fi = expression.Type.GetField(propertyOrFieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.FlattenHierarchy);
            if (fi != null)
                return Field(expression, fi);
            pi = expression.Type.GetProperty(propertyOrFieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.IgnoreCase | BindingFlags.FlattenHierarchy);
            if (pi != null)
                return Property(expression, pi);
            fi = expression.Type.GetField(propertyOrFieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.IgnoreCase | BindingFlags.FlattenHierarchy);
            if (fi != null)
                return Field(expression, fi);

            throw Error.NotAMemberOfType(propertyOrFieldName, expression.Type);
        }

        //CONFORMING
        public static MemberExpression MakeMemberAccess(Expression expression, MemberInfo member) {
            ContractUtils.RequiresNotNull(member, "member");

            FieldInfo fi = member as FieldInfo;
            if (fi != null) {
                return Expression.Field(expression, fi);
            }
            PropertyInfo pi = member as PropertyInfo;
            if (pi != null) {
                return Expression.Property(expression, pi);
            }
            throw Error.MemberNotFieldOrProperty(member);
        }
    }
}
