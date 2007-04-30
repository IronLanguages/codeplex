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

using Microsoft.Scripting.Internal.Generation;

namespace Microsoft.Scripting.Internal.Ast {
    /// <summary>
    /// AST node representing deletion of the expression.
    /// </summary>
    public class DeleteIndexExpression : Expression {
        private readonly Expression _target;
        private readonly Expression _index;

        public DeleteIndexExpression(Expression target, Expression index)
            : this(target, index, SourceSpan.None) {
        }

        public DeleteIndexExpression(Expression target, Expression index, SourceSpan span) 
            : base(span) {
            _target = target;
            _index = index;
        }

        public Expression Target {
            get { return _target; }
        }

        public Expression Index {
            get { return _index; }
        }

        public override void Emit(CodeGen cg) {
            cg.EmitPosition(Start, End);

            cg.EmitLanguageContext();
            cg.EmitCodeContext();
            _target.Emit(cg);
            _index.Emit(cg);
            cg.EmitCall(typeof(LanguageContext), "DeleteIndex");
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                _target.Walk(walker);
                _index.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }
}
