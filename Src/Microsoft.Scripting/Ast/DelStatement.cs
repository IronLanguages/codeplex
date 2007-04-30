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

using System.Collections.Generic;
using Microsoft.Scripting.Internal.Generation;

namespace Microsoft.Scripting.Internal.Ast {
    /// <summary>
    /// AST node representing deletion of the expression.
    /// </summary>
    public class DelStatement : Statement {
        private readonly VariableReference _vr;
        private bool _defined;

        public DelStatement(VariableReference vr)
            : this(vr, SourceSpan.None) {
        }

        public DelStatement(VariableReference vr, SourceSpan span) 
            : base(span) {
            _vr = vr;
        }

        internal bool IsDefined {
            get { return _defined; }
            set { _defined = value; }
        }

        public VariableReference Reference {
            get { return _vr; }
        }

        public override void Emit(CodeGen cg) {
            cg.EmitPosition(Start, End);
            cg.EmitDel(_vr.Slot, _vr.Name, !_defined);
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
            }
            walker.PostWalk(this);
        }
    }
}
