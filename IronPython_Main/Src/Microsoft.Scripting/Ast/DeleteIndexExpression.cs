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

using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Ast {
    /// <summary>
    /// AST node representing deletion of the expression.
    /// </summary>
    public class DeleteIndexExpression : Expression {
        private readonly Expression _target;
        private readonly Expression _index;

        internal DeleteIndexExpression(SourceSpan span, Expression target, Expression index) 
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
            _target.EmitAsObject(cg);
            _index.EmitAsObject(cg);
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

    public static partial class Ast {
        /// <summary>
        /// Deletes index from the object:  del target[index]
        /// </summary>
        public static DeleteIndexExpression Delete(Expression target, Expression index) {
            return new DeleteIndexExpression(SourceSpan.None, target, index);
        }

        public static DeleteIndexExpression Delete(SourceSpan span, Expression target, Expression index) {
            return new DeleteIndexExpression(span, target, index);
        }
    }
}
