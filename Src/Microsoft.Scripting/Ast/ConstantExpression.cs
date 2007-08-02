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

        internal ConstantExpression(SourceSpan span, object value)
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

        public override AbstractValue AbstractEvaluate(AbstractContext context) {
            return AbstractValue.Constant(_value, this);
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
    }

    public static partial class Ast {
        public static ConstantExpression True() {
            return new ConstantExpression(SourceSpan.None, true);
        }

        public static ConstantExpression False() {
            return new ConstantExpression(SourceSpan.None, false);
        }

        public static ConstantExpression Zero() {
            return new ConstantExpression(SourceSpan.None, 0);
        }

        public static ConstantExpression Null() {
            return new ConstantExpression(SourceSpan.None, null);
        }

        public static ConstantExpression Constant(object value) {
            return Constant(SourceSpan.None, value);
        }

        public static ConstantExpression Constant(SourceSpan span, object value) {
            return new ConstantExpression(span, value);
        }

        public static ConstantExpression RuntimeConstant(object value) {
            return new ConstantExpression(SourceSpan.None, new RuntimeConstant(value));
        }

        /// <summary>
        /// Wraps the given value in a WeakReference and returns a tree that will retrieve
        /// the value from the WeakReference.
        /// </summary>
        public static MemberExpression WeakConstant(object value) {
            return Ast.ReadProperty(
                Ast.RuntimeConstant(new WeakReference(value)),
                typeof(WeakReference).GetProperty("Target")
            );
        }
    }
}
