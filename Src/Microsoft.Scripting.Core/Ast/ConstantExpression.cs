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
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Ast {
    public sealed class ConstantExpression : Expression {
        private readonly object _value;

        internal ConstantExpression(object value, Type /*!*/ type)
            : base(AstNodeType.Constant, type) {
            _value = value;
        }

        public object Value {
            get { return _value; }
        }
    }

    public static partial class Ast {
        public static ConstantExpression True() {
            return new ConstantExpression(true, typeof(bool));
        }

        public static ConstantExpression False() {
            return new ConstantExpression(false, typeof(bool));
        }

        public static ConstantExpression Zero() {
            return new ConstantExpression(0, typeof(int));
        }

        public static ConstantExpression Null() {
            return new ConstantExpression(null, typeof(object));
        }

        public static ConstantExpression Null(Type type) {
            Contract.Requires(!type.IsValueType, "type");
            return new ConstantExpression(null, type);
        }

        public static ConstantExpression Constant(object value) {
            return new ConstantExpression(value, value == null ? typeof(object) : value.GetType());
        }

        public static ConstantExpression Constant(object value, Type type) {
            Contract.RequiresNotNull(type, "type");
            if (value == null) {
                Contract.Requires(!type.IsValueType, "type");
            } else {
                Contract.Requires(TypeUtils.CanAssign(type, value.GetType()), "type");
            }
            return new ConstantExpression(value, type);
        }

        public static ConstantExpression RuntimeConstant(object value) {
            return new ConstantExpression(new RuntimeConstant(value), value == null ? typeof(object) : value.GetType());
        }

        /// <summary>
        /// Wraps the given value in a WeakReference and returns a tree that will retrieve
        /// the value from the WeakReference.
        /// </summary>
        public static MemberExpression WeakConstant(object value) {
            System.Diagnostics.Debug.Assert(!(value is Expression));
            return Ast.ReadProperty(
                Ast.RuntimeConstant(new WeakReference(value)),
                typeof(WeakReference).GetProperty("Target")
            );
        }
    }
}
