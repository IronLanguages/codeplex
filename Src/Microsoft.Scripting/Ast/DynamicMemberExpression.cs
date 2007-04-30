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

    public class DynamicMemberExpression : Expression {
        private readonly Expression _target;
        private readonly SymbolId _name;
        private readonly MemberBinding _binding;

        public DynamicMemberExpression(Expression target, SymbolId name, MemberBinding binding)
            : this(target, name, binding, SourceSpan.None) {
        }

        public DynamicMemberExpression(Expression target, SymbolId name, MemberBinding binding, SourceSpan span)
            : base(span) {
            _target = target;
            _name = name;
            _binding = binding;
        }

        public override string ToString() {
            return base.ToString() + ":" + SymbolTable.IdToString(_name);
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

        public override object Evaluate(CodeContext context) {
            object t = _target.Evaluate(context);
            if (_binding == MemberBinding.Bound) {
                return RuntimeHelpers.GetBoundMember(context, t, _name);
            } else {
                return RuntimeHelpers.GetMember(context, t, _name);
            }
        }

        public override void Emit(CodeGen cg) {
            cg.EmitCodeContext();
            _target.Emit(cg);
            cg.EmitSymbolId(_name);
            cg.EmitCall(typeof(RuntimeHelpers), _binding == MemberBinding.Unbound ? "GetMember" : "GetBoundMember");
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                _target.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }
}
