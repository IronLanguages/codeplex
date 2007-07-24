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
using System.Diagnostics;
using System.Reflection.Emit;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Ast {
    public class BinaryExpression : Expression {
        private readonly Expression _left, _right;
        private readonly Operators _op;

        internal BinaryExpression(SourceSpan span, Operators op, Expression left, Expression right)
            : base(span) {
            Debug.Assert(left != null);
            Debug.Assert(right != null);

            _left = left;
            _right = right;
            _op = op;
        }

        public Expression Right {
            get { return _right; }
        }

        public Expression Left {
            get { return _left; }
        }

        public override Type ExpressionType {
            get {
                switch (_op) {
                    case Operators.Equal:
                    case Operators.NotEqual:
                        return typeof(bool);
                    case Operators.AndAlso:
                        return typeof(bool);
                    case Operators.Add:
                    case Operators.Multiply:
                        return typeof(int);
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public Operators Op {
            get { return _op; }
        }

        private bool EmitBranchTrue(CodeGen cg, Operators op, Label label) {
            switch (op) {
                case Operators.Equal:
                    if (_left.IsConstant(null)) {
                        _right.EmitAsObject(cg);
                        cg.Emit(OpCodes.Brfalse, label);
                    } else if (_right.IsConstant(null)) {
                        _left.EmitAsObject(cg);
                        cg.Emit(OpCodes.Brfalse, label);
                    } else {
                        _left.EmitAs(cg, GetEmitType());
                        _right.EmitAs(cg, GetEmitType());
                        cg.Emit(OpCodes.Beq, label);
                    }
                    return true;
                case Operators.NotEqual:
                    if (_left.IsConstant(null)) {
                        _right.EmitAsObject(cg);
                        cg.Emit(OpCodes.Brtrue, label);
                    } else if (_right.IsConstant(null)) {
                        _left.EmitAsObject(cg);
                        cg.Emit(OpCodes.Brtrue, label);
                    } else {
                        _left.EmitAs(cg, GetEmitType());
                        _right.EmitAs(cg, GetEmitType());
                        cg.Emit(OpCodes.Ceq);
                        cg.Emit(OpCodes.Brfalse, label);
                    }
                    return true;
                case Operators.AndAlso:
                    Label falseBranch = cg.DefineLabel();
                    _left.EmitBranchFalse(cg, falseBranch);
                    _right.EmitBranchTrue(cg, label);
                    cg.MarkLabel(falseBranch);
                    return true;
                default:
                    return false;
            }
        }

        private Type GetEmitType() {
            if (_op == Operators.Multiply) return typeof(int);

            return _left.ExpressionType == _right.ExpressionType ? _left.ExpressionType : typeof(object);
        }

        public override void EmitBranchFalse(CodeGen cg, Label label) {
            switch (_op) {
                case Operators.Equal:
                    EmitBranchTrue(cg, Operators.NotEqual, label);
                    break;
                case Operators.NotEqual:
                    EmitBranchTrue(cg, Operators.Equal, label);
                    break;
                case Operators.AndAlso:
                    if (_left.IsConstant(false)) {
                        cg.Emit(OpCodes.Br, label);
                    } else {
                        if (!_left.IsConstant(true)) _left.EmitBranchFalse(cg, label);

                        if (_right.IsConstant(false)) {
                            cg.Emit(OpCodes.Br, label);
                        } else if (!_right.IsConstant(true)) {
                            _right.EmitBranchFalse(cg, label);
                        }
                    } 
                    break;
                default:
                    base.EmitBranchFalse(cg, label);
                    break;
            }
        }

        public override void EmitBranchTrue(CodeGen cg, Label label) {
            if (!EmitBranchTrue(cg, _op, label)) {
                base.EmitBranchTrue(cg, label);
            }
        }

        public override void Emit(CodeGen cg) {
            if (_op == Operators.AndAlso) {
                Label falseBranch = cg.DefineLabel();
                _left.EmitBranchFalse(cg, falseBranch);

                //TODO code gen will be suboptimal for chained AndAlsos and AndAlso inside If
                _right.EmitAs(cg, typeof(bool));
                Label skipFalseBranch = cg.DefineLabel();
                cg.Emit(OpCodes.Br, skipFalseBranch);
                cg.MarkLabel(falseBranch);
                cg.EmitInt(0);
                cg.MarkLabel(skipFalseBranch);
                return;
            }

            _left.EmitAs(cg, GetEmitType());
            _right.EmitAs(cg, GetEmitType());

            switch (_op) {
                case Operators.Equal:
                    cg.Emit(OpCodes.Ceq);
                    break;
                case Operators.NotEqual:
                    cg.Emit(OpCodes.Ceq);
                    cg.EmitInt(0);
                    cg.Emit(OpCodes.Ceq);
                    break;
                case Operators.Multiply:
                    cg.Emit(OpCodes.Mul);
                    break;
                case Operators.Add:
                    cg.Emit(OpCodes.Add);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public override object Evaluate(CodeContext context) {
            if (_op == Operators.AndAlso) {
                object ret = _left.Evaluate(context);
                if ((bool)ret) {
                    return _right.Evaluate(context);
                } else {
                    return ret;
                }
            }

            object l = _left.Evaluate(context);
            object r = _right.Evaluate(context);
            switch (_op) {
                case Operators.Equal:
                    return RuntimeHelpers.BooleanToObject(TestEquals(l, r));
                case Operators.NotEqual:
                    return RuntimeHelpers.BooleanToObject(!TestEquals(l, r));
                case Operators.Multiply:
                    return (int)context.LanguageContext.Binder.Convert(l, typeof(int)) *
                            (int)context.LanguageContext.Binder.Convert(r, typeof(int));
                case Operators.Add:
                    return (int)context.LanguageContext.Binder.Convert(l, typeof(int)) +
                            (int)context.LanguageContext.Binder.Convert(r, typeof(int));
                default:
                    throw new NotImplementedException();
            }
        }

        private bool TestEquals(object l, object r) {
            // This code needs to mimic the code we generate above in Emit()

            // TODO: The null checks _should_ be unnecessary!
            // TODO: enums are not IsPrimitive
            if (GetEmitType().IsPrimitive && l != null && r != null) {
                return l.Equals(r);
            }
            return l == r;
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                _left.Walk(walker);
                _right.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }

    public static partial class Ast {
        public static BinaryExpression Equal(Expression left, Expression right) {
            return Binary(SourceSpan.None, Operators.Equal, left, right);
        }
        public static BinaryExpression NotEqual(Expression left, Expression right) {
            return Binary(SourceSpan.None, Operators.NotEqual, left, right);
        }
        public static Expression AndAlso(Expression left, Expression right) {
            return Binary(SourceSpan.None, Operators.AndAlso, left, right);
        }

        /// <summary>
        /// Multiples two Int32 values.
        /// </summary>
        public static BinaryExpression Multiply(Expression left, Expression right) {
            if (left.ExpressionType != typeof(int) || right.ExpressionType != typeof(int)) {
                throw new NotSupportedException(String.Format("multiply only supports ints, got {0} {1}", left.ExpressionType.Name, right.ExpressionType.Name));
            }

            return Binary(SourceSpan.None, Operators.Multiply, left, right);
        }

        /// <summary>
        /// Adds two Int32 values.
        /// </summary>
        public static Expression Add(Expression left, Expression right) {
            if (left.ExpressionType != typeof(int) || right.ExpressionType != typeof(int)) {
                throw new NotSupportedException(String.Format("add only supports ints, got {0} {1}", left.ExpressionType.Name, right.ExpressionType.Name));
            }

            return Binary(SourceSpan.None, Operators.Add, left, right);
        }

        public static BinaryExpression Binary(SourceSpan span, Operators op, Expression left, Expression right) {
            if (left == null) {
                throw new ArgumentNullException("left");
            }
            if (right == null) {
                throw new ArgumentNullException("right");
            }
            return new BinaryExpression(span, op, left, right);
        }
    }
}
