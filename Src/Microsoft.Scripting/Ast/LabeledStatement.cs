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
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Ast {
    public class LabeledStatement : Statement {
        private readonly Statement _labeled;

        public LabeledStatement(Statement labeled)
            : base(SourceSpan.None) {
            this._labeled = labeled;
        }

        public Statement Statement {
            get { return _labeled; }
        }
                        
        public override void Emit(CodeGen cg) {
            Label label = cg.DefineLabel();
            cg.PushTargets(label, label, this);

            _labeled.Emit(cg);

            cg.MarkLabel(label);

            cg.PopTargets();
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                _labeled.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }
}
