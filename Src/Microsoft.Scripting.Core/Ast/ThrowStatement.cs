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

namespace System.Linq.Expressions {
    public sealed class ThrowStatement : Expression {
        private readonly Expression _val;

        internal ThrowStatement(Annotations annotations, Expression value)
            : base(annotations, ExpressionType.ThrowStatement, typeof(void)) {
            _val = value;
        }

        public Expression Value {
            get {
                return _val;
            }
        }

        public Expression Exception {
            get {
                return _val;
            }
        }
    }

    public partial class Expression {
        public static ThrowStatement Rethrow() {
            return Throw(null);
        }

        public static ThrowStatement Throw(Expression value) {
            return Throw(value, Annotations.Empty);
        }

        public static ThrowStatement Throw(Expression value, Annotations annotations) {
            if (value != null) {
                ContractUtils.Requires(TypeUtils.CanAssign(typeof(Exception), value.Type));
            }
            return new ThrowStatement(annotations, value);
        }
    }
}
