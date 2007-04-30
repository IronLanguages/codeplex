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
using System.Reflection.Emit;
using Microsoft.Scripting.Internal.Generation;

namespace Microsoft.Scripting.Internal.Ast {
    public class BinaryExpression : Expression {
        private readonly Expression _left, _right;
        private readonly Operators _op;

        public BinaryExpression(Operators op, Expression left, Expression right, SourceSpan span)
            : base(span) {
            if (left == null) throw new ArgumentNullException("left");
            if (right == null) throw new ArgumentNullException("right");

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
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private bool EmitBranchTrue(CodeGen cg, Operators op, Label label) {
            switch (op) {
                case Operators.Equal:
                    if (_left.IsConstant(null)) {
                        _right.Emit(cg);
                        cg.Emit(OpCodes.Brfalse, label);
                    } else if (_right.IsConstant(null)) {
                        _left.Emit(cg);
                        cg.Emit(OpCodes.Brfalse, label);
                    } else {
                        _left.EmitAs(cg, GetEmitType());
                        _right.EmitAs(cg, GetEmitType());
                        cg.Emit(OpCodes.Beq, label);
                    }
                    return true;
                case Operators.NotEqual:
                    if (_left.IsConstant(null)) {
                        _right.Emit(cg);
                        cg.Emit(OpCodes.Brtrue, label);
                    } else if (_right.IsConstant(null)) {
                        _left.Emit(cg);
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
                    _left.EmitBranchFalse(cg, label);
                    _right.EmitBranchFalse(cg, label);
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

        public override void EmitAs(CodeGen cg, Type asType) {
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
                default:
                    throw new NotImplementedException();
            }
            cg.EmitConvert(ExpressionType, asType);
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
            //TODO these use Equals to handle boxed Value types correctly; however,
            // that is only a partially correct solution...
            switch (_op) {
                case Operators.Equal:
                    return RuntimeHelpers.BooleanToObject(TestEquals(l, r));
                case Operators.NotEqual:
                    return RuntimeHelpers.BooleanToObject(!TestEquals(l, r));
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

        public override void Emit(CodeGen cg) {
            EmitAs(cg, typeof(object));
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                _left.Walk(walker);
                _right.Walk(walker);
            }
            walker.PostWalk(this);
        }

        public static BinaryExpression Equal(Expression left, Expression right) {
            return new BinaryExpression(Operators.Equal, left, right, SourceSpan.None);
        }
        public static BinaryExpression NotEqual(Expression left, Expression right) {
            return new BinaryExpression(Operators.NotEqual, left, right, SourceSpan.None);
        }
        public static Expression AndAlso(Expression left, Expression right) {
            return new BinaryExpression(Operators.AndAlso, left, right, SourceSpan.None);
        }
    }
}
