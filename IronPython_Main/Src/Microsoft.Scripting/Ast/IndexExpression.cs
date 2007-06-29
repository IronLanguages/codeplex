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
    public class IndexExpression : Expression {
        private readonly Expression _target;
        private readonly Expression _index;

        internal IndexExpression(SourceSpan span, Expression target, Expression index)
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
            _target.EmitAsObject(cg);
            _index.EmitAsObject(cg);
            cg.EmitCall(typeof(RuntimeHelpers), "GetIndex");
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
        public static IndexExpression DynamicReadItem(Expression target, Expression index) {
            return DynamicReadItem(SourceSpan.None, target, index);
        }
        public static IndexExpression DynamicReadItem(SourceSpan span, Expression target, Expression index) {
            return new IndexExpression(span, target, index);
        }
    }
}
