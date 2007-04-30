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
using Microsoft.Scripting.Internal.Generation;

namespace Microsoft.Scripting.Internal.Ast {
    public class BoundAssignment : Expression {
        private readonly VariableReference _vr;
        private readonly Expression _value;
        private readonly Operators _op;
        private bool _defined;

        public BoundAssignment(VariableReference vr, Expression value, Operators op)
            : this(vr, value, op, SourceSpan.None) {
        }

        public BoundAssignment(VariableReference vr, Expression value, Operators op, SourceSpan span)
            : base(span) {
            if (vr == null) throw new ArgumentNullException("vr");
            if (value == null) throw new ArgumentNullException("value");
            _vr = vr;
            _value = value;
            _op = op;
        }

        public VariableReference Reference {
            get { return _vr; }
        }

        public Expression Value {
            get { return _value; }
        }

        public Operators Operator {
            get { return _op; }
        }

        public bool IsDefined {
            get { return _defined; }
            set { _defined = value; }
        }

        public override void Emit(CodeGen cg) {
            EmitAs(cg, typeof(object));
        }

        public override void EmitAs(CodeGen cg, Type asType) {
            if (_op == Operators.None) {
                _value.EmitAs(cg, _value.ExpressionType);
                cg.EmitConvert(_value.ExpressionType, _vr.Slot.Type);
            } else {
                cg.EmitInPlaceOperator(
                    _op,
                    _vr.Type,
                    delegate (CodeGen _cg, Type _as) {
                        _cg.EmitGet(_vr.Slot, _vr.Name, !_defined);
                        _cg.EmitConvert(_vr.Type, _as);
                    },
                    _value.ExpressionType,
                    _value.EmitAs
                );
                cg.EmitConvert(typeof(object), _vr.Slot.Type);
            }

            if (asType == typeof(void)) {
                _vr.Slot.EmitSet(cg);
            } else if (_vr.Slot is LocalSlot || _vr.Slot is ArgSlot) {
                _vr.Slot.EmitSet(cg);
                _vr.Slot.EmitGet(cg);
                cg.EmitConvert(_vr.Slot.Type, asType);
            } else {
                Slot tmp = cg.GetLocalTmp(_vr.Slot.Type);
                tmp.EmitSet(cg);
                tmp.EmitGet(cg);
                _vr.Slot.EmitSet(cg);
                tmp.EmitGet(cg);
                cg.EmitConvert(tmp.Type, asType);
                cg.FreeLocalTmp(tmp);
            }
        }

        public override object Evaluate(CodeContext context) {
            object result = _value.Evaluate(context);
            RuntimeHelpers.SetName(context, _vr.Name, result);
            return result;
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                _value.Walk(walker);
            }
            walker.PostWalk(this);
        }

        #region Factory methods
        public static BoundAssignment Assign(VariableReference vr, Expression value) {
            return Assign(vr, value, SourceSpan.None);
        }

        public static BoundAssignment Assign(VariableReference vr, Expression value, SourceSpan span) {
            return OpAssign(vr, value, Operators.None, span);
        }

        public static BoundAssignment OpAssign(VariableReference vr, Expression value, Operators op, SourceSpan span) {
            return new BoundAssignment(vr, value, op, span);
        }

        #endregion
    }
}
