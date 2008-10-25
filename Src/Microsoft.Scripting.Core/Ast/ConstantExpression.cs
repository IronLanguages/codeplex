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
using Microsoft.Scripting.Utils;
using System.Text;
using Microsoft.Scripting;

namespace Microsoft.Linq.Expressions {
    //CONFORMING
    public class ConstantExpression : Expression {
        internal static readonly ConstantExpression TrueLiteral = ConstantExpression.Make(null, true, typeof(bool));
        internal static readonly ConstantExpression FalseLiteral = ConstantExpression.Make(null, false, typeof(bool));
        internal static readonly ConstantExpression ZeroLiteral = ConstantExpression.Make(null, 0, typeof(int));
        internal static readonly ConstantExpression NullLiteral = ConstantExpression.Make(null, null, typeof(object));
        internal static readonly ConstantExpression EmptyStringLiteral = ConstantExpression.Make(null, String.Empty, typeof(string));
        internal static readonly ConstantExpression[] IntCache = new ConstantExpression[100];

        private readonly object _value;

        internal ConstantExpression(Annotations annotations, object value)
            : base(annotations) {
            _value = value;
        }

        internal static ConstantExpression Make(Annotations annotations, object value, Type type) {
            if ((value == null && type == typeof(object)) || (value != null && value.GetType() == type)) {
                return new ConstantExpression(annotations, value);
            } else {
                return new TypedConstantExpression(annotations, value, type);
            }
        }

        protected override Type GetExpressionType() {
            if(_value == null) {
                return typeof(object);
            }
            return _value.GetType();
        }

        protected override ExpressionType GetNodeKind() {
            return ExpressionType.Constant;
        }

        public object Value {
            get { return _value; }
        }

        internal override void BuildString(StringBuilder builder) {
            ContractUtils.RequiresNotNull(builder, "builder");

            if (_value != null) {
                if (_value is string) {
                    builder.Append("\"");
                    builder.Append(_value);
                    builder.Append("\"");
                } else if (_value.ToString() == _value.GetType().ToString()) {
                    builder.Append("value(");
                    builder.Append(_value);
                    builder.Append(")");
                } else {
                    builder.Append(_value);
                }
            } else {
                builder.Append("null");
            }
        }

        internal override Expression Accept(ExpressionTreeVisitor visitor) {
            return visitor.VisitConstant(this);
        }
    }

    internal class TypedConstantExpression : ConstantExpression {
        private readonly Type _type;

        internal TypedConstantExpression(Annotations annotations, object value, Type type)
            : base(annotations, value) {
            _type = type;
        }

        protected override Type GetExpressionType() {
            return _type;
        }
    }

    public partial class Expression {
        public static ConstantExpression True() {
            return ConstantExpression.TrueLiteral;
        }

        public static ConstantExpression False() {
            return ConstantExpression.FalseLiteral;
        }

        public static ConstantExpression Zero() {
            return ConstantExpression.ZeroLiteral;
        }

        public static ConstantExpression Null() {
            return ConstantExpression.NullLiteral;
        }

        public static ConstantExpression Null(Type type) {
            ContractUtils.RequiresNotNull(type, "type");
            if (type.IsValueType && !TypeUtils.IsNullableType(type)) {
                throw Error.ArgumentTypesMustMatch();
            }
            return ConstantExpression.Make(Annotations.Empty, null, type);
        }
        
        //CONFORMING
        public static ConstantExpression Constant(object value) {
            if (value == null) {
                return Null();
            }

            Type t = value.GetType();
            if (!t.IsEnum) {
                switch (Type.GetTypeCode(t)) {
                    case TypeCode.Boolean:
                        if ((bool)value == true) {
                            return True();
                        }
                        return False();
                    case TypeCode.Int32:
                        int x = (int)value;
                        int cacheIndex = x + 2;
                        if (cacheIndex >= 0 && cacheIndex < ConstantExpression.IntCache.Length) {
                            ConstantExpression res;
                            if ((res = ConstantExpression.IntCache[cacheIndex]) == null) {
                                ConstantExpression.IntCache[cacheIndex] = res = ConstantExpression.Make(null, x, typeof(int));
                            }
                            return res;
                        }
                        break;
                    case TypeCode.String:
                        if (String.IsNullOrEmpty((string)value)) {
                            return ConstantExpression.EmptyStringLiteral;
                        }
                        break;
                }
            }

            return ConstantExpression.Make(Annotations.Empty, value, value == null ? typeof(object) : value.GetType());
        }

        public static ConstantExpression Constant(object value, Type type) {
            return Constant(value, type, Annotations.Empty);
        }

        //CONFORMING
        public static ConstantExpression Constant(object value, Type type, Annotations annotations) {
            ContractUtils.RequiresNotNull(type, "type");
            if (value == null && type.IsValueType && !TypeUtils.IsNullableType(type)) {
                throw Error.ArgumentTypesMustMatch();
            }
            if (value != null && !TypeUtils.AreAssignable(type, value.GetType())) {
                throw Error.ArgumentTypesMustMatch();
            }
            return ConstantExpression.Make(annotations, value, type);
        }

        [Obsolete("use Expression.Constant instead")]
        public static ConstantExpression RuntimeConstant(object value) {
            return Constant(value);
        }

        [Obsolete("use Expression.Constant instead")]
        public static ConstantExpression RuntimeConstant(object value, Type type) {
            return Constant(value, type);
        }

        /// <summary>
        /// Wraps the given value in a WeakReference and returns a tree that will retrieve
        /// the value from the WeakReference.
        /// </summary>
        [Obsolete("use Utils.WeakConstant in Microsoft.Scripting instead")]
        public static MemberExpression WeakConstant(object value) {
            System.Diagnostics.Debug.Assert(!(value is Expression));
            return Expression.Property(
                Expression.Constant(new WeakReference(value)),
                typeof(WeakReference).GetProperty("Target")
            );
        }
    }
}
