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
    public class IndexAssignment : Expression {
        private readonly Expression _target;
        private readonly Expression _index;
        private readonly Expression _value;
        private readonly Operators _op;

        public IndexAssignment(Expression target, Expression index, Expression value, Operators op)
            : this(target, index, value, op, SourceSpan.None) {
        }

        public IndexAssignment(Expression target, Expression index, Expression value, Operators op, SourceSpan span)
            : base(span) {
            if (target == null) throw new ArgumentNullException("target");
            if (index == null) throw new ArgumentNullException("index");
            if (value == null) throw new ArgumentNullException("value");

            _target = target;
            _index = index;
            _value = value;
            _op = op;
        }

        public Expression Target {
            get { return _target; }
        }

        public Expression Index {
            get { return _index; }
        }
        public Expression Value {
            get { return _value; }
        }

        public Operators Op {
            get { return _op; }
        }

        public override void Emit(CodeGen cg) {
            EmitAs(cg, typeof(object));
        }

        public override void EmitAs(CodeGen cg, Type asType) {
            Slot targetTmp = null, indexTmp = null;
            if (_op == Operators.None) {
                _value.Emit(cg);
            } else {
                cg.EmitInPlaceOperator(
                    _op,
                    this.ExpressionType,
                    delegate(CodeGen _cg, Type _asType) {
                        _cg.EmitCodeContext();
                        _target.Emit(_cg);
                        targetTmp = _cg.CopyTopOfStack(typeof(object));
                        _index.Emit(_cg);
                        indexTmp = _cg.CopyTopOfStack(typeof(object));
                        _cg.EmitCall(typeof(RuntimeHelpers), "GetIndex");
                        _cg.EmitConvertFromObject(_asType);
                    },
                    _value.ExpressionType,
                    _value.EmitAs
                );
            }

            Slot result = cg.CopyTopOfStack(asType);
            if (targetTmp != null) {
                targetTmp.EmitGet(cg);
                cg.FreeLocalTmp(targetTmp);
                indexTmp.EmitGet(cg);
                cg.FreeLocalTmp(indexTmp);
            } else {
                _target.Emit(cg);
                _index.Emit(cg);
            }
            cg.EmitCodeContext();
            cg.EmitCall(typeof(RuntimeHelpers), "SetIndex");
            result.EmitGet(cg);
            cg.FreeLocalTmp(result);
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                _target.Walk(walker);
                _index.Walk(walker);
                _value.Walk(walker);
            }
            walker.PostWalk(this);
        }

        public static IndexAssignment SimpleAssign(Expression target, Expression index, Expression value) {
            return new IndexAssignment(target, index, value, Operators.None);
        }

        public static IndexAssignment SimpleAssign(SourceSpan span, Expression target, Expression index, Expression value) {
            return new IndexAssignment(target, index, value, Operators.None, SourceSpan.None);
        }

    }
}
