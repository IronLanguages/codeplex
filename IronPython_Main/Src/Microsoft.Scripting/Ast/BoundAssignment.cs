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
using Microsoft.Scripting.Actions;
using System.Reflection;

namespace Microsoft.Scripting.Ast {
    public class BoundAssignment : Expression {
        private readonly Variable _variable;
        private readonly Expression _value;
        private readonly Operators _op;
        private bool _defined;

        // implementation detail.
        private VariableReference _vr;

        internal BoundAssignment(SourceSpan span, Variable variable, Expression value, Operators op)
            : base(span) {
            if (variable == null) throw new ArgumentNullException("variable");
            if (value == null) throw new ArgumentNullException("value");
            _variable = variable;
            _value = value;
            _op = op;
        }

        public Variable Variable {
            get { return _variable; }
        }

        internal VariableReference Ref {
            get { return _vr; }
            set {
                Debug.Assert(value != null);
                Debug.Assert(value.Variable == _variable);
                Debug.Assert(_vr == null || (object)_vr == (object)value);
                _vr = value;
            }
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

        public override Type ExpressionType {
            get {
                return _variable.Type;
            }
        }

        public override void EmitAddress(CodeGen cg, Type asType) {
            EmitValue(cg);

            _vr.Slot.EmitSet(cg);

            _vr.Slot.EmitGetAddr(cg);
        }

        public override void Emit(CodeGen cg) {
            EmitValue(cg);

            cg.Emit(OpCodes.Dup);
            _vr.Slot.EmitSet(cg);
        }

        private void EmitValue(CodeGen cg) {
            if (_op == Operators.None) {
                _value.EmitAs(cg, _vr.Slot.Type);
            } else {
                cg.EmitInPlaceOperator(
                    _op,
                    _variable.Type,
                    delegate(CodeGen _cg, Type _as) {
                        _cg.EmitGet(_vr.Slot, _variable.Name, !_defined);
                        _cg.EmitConvert(_variable.Type, _as);
                    },
                    _value.ExpressionType,
                    _value.EmitAs
                );
                cg.EmitConvert(typeof(object), _vr.Slot.Type);
            }
        }

        protected override object DoEvaluate(CodeContext context) {
            object value;
            if (_op == Operators.None) { // Just an assignment
                value = _value.Evaluate(context);
            } else {
                //TODO: cache this action?
                ActionExpression action = Ast.Action.Operator(
                    _op, _variable.Type,
                    Ast.ReadDefined(_variable), _value);
                value = action.Evaluate(context);
            }
            // Do an explicit conversion to mirror the emit case
            value = context.LanguageContext.Binder.Convert(value, _variable.Type);

            EvaluateAssign(context, _variable, value);
            return value;
        }

        internal static void EvaluateAssign(CodeContext context, Variable var, object value) {
            switch (var.Kind) {
                case Variable.VariableKind.Temporary:
                case Variable.VariableKind.GeneratorTemporary:
                    context.Scope.TemporaryStorage[var] = value;
                    break;
                case Variable.VariableKind.Global:
                    RuntimeHelpers.SetGlobalName(context, var.Name, value);
                    break;
                default:
                    RuntimeHelpers.SetName(context, var.Name, value);
                    break;
            }
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                _value.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }

    public static partial class Ast {
        public static Statement Write(Variable rhsVariable, Expression lhsValue) {
            return Statement(Assign(rhsVariable, lhsValue));
        }

        public static Statement Write(Variable rhsVariable, FieldInfo rhsField, Expression lhsValue) {
            return Statement(AssignField(Read(rhsVariable), rhsField, lhsValue));
        }

        public static Statement Write(Variable rhsVariable, FieldInfo rhsField, Variable lhsVariable) {
            return Statement(AssignField(Read(rhsVariable), rhsField, Read(lhsVariable)));
        }

        public static Statement Write(Variable rhsVariable, Variable lhsVariable, FieldInfo lhsField) {
            return Statement(Assign(rhsVariable, ReadField(Read(lhsVariable), lhsField)));
        }

        public static Statement Write(Variable rhsVariable, FieldInfo rhsField, Variable lhsVariable, FieldInfo lhsField) {
            return Statement(AssignField(Read(rhsVariable), rhsField, ReadField(Read(lhsVariable), lhsField)));
        }

        public static BoundAssignment Assign(Variable variable, Expression value) {
            return Assign(SourceSpan.None, variable, value, Operators.None);
        }

        public static BoundAssignment Assign(Variable variable, Expression value, Operators op) {
            return Assign(SourceSpan.None, variable, value, op);
        }

        public static BoundAssignment Assign(SourceSpan span, Variable variable, Expression value) {
            return Assign(span, variable, value, Operators.None);
        }

        public static BoundAssignment Assign(SourceSpan span, Variable variable, Expression value, Operators op) {
            return new BoundAssignment(span, variable, value, op);
        }
    }
}
