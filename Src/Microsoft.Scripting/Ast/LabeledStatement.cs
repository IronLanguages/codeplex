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
        private readonly Statement _statement;

        internal LabeledStatement(SourceSpan span, Statement statement)
            : base(span) {
            _statement = statement;
        }

        public Statement Statement {
            get { return _statement; }
        }
                        
        public override void Emit(CodeGen cg) {
            Label label = cg.DefineLabel();
            cg.PushTargets(label, label, this);

            _statement.Emit(cg);

            cg.MarkLabel(label);

            cg.PopTargets();
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                _statement.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }

    public static partial class Ast {
        public static LabeledStatement Labeled(Statement statement) {
            return Labeled(SourceSpan.None, statement);
        }
        public static LabeledStatement Labeled(SourceSpan span, Statement statement) {
            return new LabeledStatement(span, statement);
        }
    }
}