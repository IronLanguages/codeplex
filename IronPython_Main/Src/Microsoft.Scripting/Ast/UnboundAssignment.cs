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
    public class UnboundAssignment : Expression {
        private readonly SymbolId _name;
        private readonly Expression _value;
        private readonly Operators _op;

        public UnboundAssignment(SymbolId name, Expression value, Operators op)
            : this(name, value, op, SourceSpan.None) {
        }

        public UnboundAssignment(SymbolId name, Expression value, Operators op, SourceSpan span)
            : base(span) {
            if (value == null) throw new ArgumentNullException("value");
            _name = name;
            _value = value;
            _op = op;
        }

        public override Type ExpressionType {
            get {
                return typeof(object);
            }
        }

        public override object Evaluate(CodeContext context) {
            object value = _value.Evaluate(context);
            RuntimeHelpers.SetName(context, _name, value);
            return value;
        }

        public override void Emit(CodeGen cg) {
            if (_op == Operators.None) {
                _value.EmitAsObject(cg);
            } else {
                cg.EmitInPlaceOperator(
                    _op,
                    typeof(object),
                    delegate(CodeGen _cg, Type _as) {
                        _cg.EmitCodeContext();
                        _cg.EmitSymbolId(_name);
                        _cg.EmitCall(typeof(RuntimeHelpers), "LookupName");
                        _cg.EmitConvert(typeof(object), _as);
                    },
                    _value.ExpressionType,
                    _value.EmitAs
                );
            }

            cg.EmitCodeContext();
            cg.EmitSymbolId(_name);
            cg.EmitCall(typeof(RuntimeHelpers), "SetNameReorder");
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                _value.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }
}
