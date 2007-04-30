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
    public class IndexExpression : Expression {
        private readonly Expression _target;
        private readonly Expression _index;

        public IndexExpression(Expression target, Expression index, SourceSpan span)
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

        public override object Evaluate(CodeContext context) {
            object t = _target.Evaluate(context);
            object i = _index.Evaluate(context);
            return RuntimeHelpers.GetIndex(context, t, i);
        }

        public override void Emit(CodeGen cg) {
            cg.EmitCodeContext();
            _target.Emit(cg);
            _index.Emit(cg);
            cg.EmitCall(typeof(RuntimeHelpers), "GetIndex");
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                _target.Walk(walker);
                _index.Walk(walker);
            }
            walker.PostWalk(this);
        }

        #region Factories
        public static IndexExpression Indexer(SourceSpan span, Expression target, Expression index) {
            return new IndexExpression(target, index, span);
        }

        public static IndexExpression Indexer(Expression target, Expression index) {
            return new IndexExpression(target, index, SourceSpan.None);
        }
        #endregion
    }
}
