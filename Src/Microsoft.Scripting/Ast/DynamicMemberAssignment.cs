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
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Ast {
    public class DynamicMemberAssignment : Expression {
        private readonly Expression _target;
        private readonly SymbolId _name;
        private readonly MemberBinding _binding;
        private readonly Expression _value;
        private readonly Operators _op;

        internal DynamicMemberAssignment(SourceSpan span, Expression target, SymbolId name,
                                         MemberBinding binding, Expression value, Operators op)
            : base(span) {
            Debug.Assert(target != null);
            Debug.Assert(value != null);

            _target = target;
            _name = name;
            _binding = binding;
            _value = value;
            _op = op;
        }

        public Expression Target {
            get { return _target; }
        }

        public SymbolId Name {
            get { return _name; }
        }

        public MemberBinding Binding {
            get { return _binding; }
        }

        public Expression Value {
            get { return _value; }
        }

        public Operators Op {
            get { return _op; }
        }

        public override void Emit(CodeGen cg) {
            Slot targetTmp = null;
            if (_op == Operators.None) {
                _value.EmitAsObject(cg);
            } else {
                cg.EmitInPlaceOperator(
                    _op,
                    this.ExpressionType,
                    delegate(CodeGen _cg, Type _asType) {
                        _cg.EmitCodeContext();
                        _target.EmitAsObject(_cg);
                        targetTmp = _cg.DupAndStoreInTemp(typeof(object));
                        _cg.EmitSymbolId(_name);
                        _cg.EmitCall(typeof(RuntimeHelpers), _binding == MemberBinding.Unbound ? "GetMember" : "GetBoundMember");
                        _cg.EmitConvertFromObject(_asType);
                    },
                    _value.ExpressionType,
                    _value.EmitAs
                );
            }

            if (targetTmp != null) {
                targetTmp.EmitGet(cg);
                cg.FreeLocalTmp(targetTmp);
            } else {
                _target.Emit(cg);
            }

            cg.EmitSymbolId(_name);
            cg.EmitCodeContext();
            cg.EmitCall(typeof(RuntimeHelpers), "SetMember");
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                _target.Walk(walker);
                _value.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }

    public static partial class Ast {
        public static DynamicMemberAssignment DynamicAssignMember(Expression target, SymbolId name, MemberBinding binding, Expression value, Operators op) {
            return DynamicAssignMember(SourceSpan.None, target, name, binding, value, op);
        }

        public static DynamicMemberAssignment DynamicAssignMember(SourceSpan span, Expression target, SymbolId name, MemberBinding binding, Expression value, Operators op) {
            if (target == null) {
                throw new ArgumentNullException("target");
            }
            if (value == null) {
                throw new ArgumentNullException("value");
            }
            if (name.IsInvalid || name.IsEmpty) {
                throw new ArgumentException("Invalid or empty name is not allowed");
            }

            return new DynamicMemberAssignment(span, target, name, binding, value, op);
        }
    }
}
