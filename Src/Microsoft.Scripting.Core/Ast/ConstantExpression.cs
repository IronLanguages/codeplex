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

using System.Scripting.Utils;
using System.Text;
using System.Scripting;

namespace System.Linq.Expressions {
    //CONFORMING
    public sealed class ConstantExpression : Expression {
        private readonly object _value;

        internal ConstantExpression(Annotations annotations, object value, Type type)
            : base(ExpressionType.Constant, type, annotations, null) {
            _value = value;
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
    }

    public partial class Expression {
        public static ConstantExpression True() {
            return new ConstantExpression(Annotations.Empty, true, typeof(bool));
        }

        public static ConstantExpression False() {
            return new ConstantExpression(Annotations.Empty, false, typeof(bool));
        }

        public static ConstantExpression Zero() {
            return new ConstantExpression(Annotations.Empty, 0, typeof(int));
        }

        public static ConstantExpression Null() {
            return new ConstantExpression(Annotations.Empty, null, typeof(object));
        }

        public static ConstantExpression Null(Type type) {
            ContractUtils.RequiresNotNull(type, "type");
            if (type.IsValueType && !TypeUtils.IsNullableType(type)) {
                throw Error.ArgumentTypesMustMatch();
            }
            return new ConstantExpression(Annotations.Empty, null, type);
        }
        
        //CONFORMING
        public static ConstantExpression Constant(object value) {
            return new ConstantExpression(Annotations.Empty, value, value == null ? typeof(object) : value.GetType());
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
            return new ConstantExpression(annotations, value, type);
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
