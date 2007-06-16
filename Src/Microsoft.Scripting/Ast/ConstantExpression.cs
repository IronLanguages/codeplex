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
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Ast {
    public class ConstantExpression : Expression {
        private readonly object _value;

        public ConstantExpression(object value)
            : this(value, SourceSpan.None) {
        }

        public ConstantExpression(object value, SourceSpan span)
            : base(span) {
            _value = value;
        }

        public object Value {
            get { return _value; }
        }

        public override Type ExpressionType {
            get {
                if (_value == null) {
                    return typeof(object);
                }
                CompilerConstant cc = _value as CompilerConstant;
                if (cc != null) {
                    return cc.Type;
                }
                return _value.GetType();
            }
        }

        public override object Evaluate(CodeContext context) {
            CompilerConstant cc = _value as CompilerConstant;
            if (cc != null) return cc.Create(); // TODO: Only create once?

            return _value;
        }

        public override void Emit(CodeGen cg) {
            cg.EmitConstant(_value);
        }

        public override bool IsConstant(object value) {
            if (value == null) {
                return _value == null;
            } else {
                return value.Equals(_value);
            }
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                ;
            }
            walker.PostWalk(this);
        }

        public static ConstantExpression Constant(object value) {
            return new ConstantExpression(value);
        }
    }
}
