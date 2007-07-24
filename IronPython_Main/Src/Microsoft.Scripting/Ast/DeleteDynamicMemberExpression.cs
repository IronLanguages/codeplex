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
    public class DeleteDynamicMemberExpression : Expression {
        private readonly Expression _target;
        private SymbolId _name;

        internal DeleteDynamicMemberExpression(SourceSpan span, Expression target, SymbolId name) 
            : base(span) {
            _target = target;
            _name = name;
        }

        public Expression Target {
            get { return _target; }
        }

        public SymbolId Name {
            get { return _name; }
        }

        public override object Evaluate(CodeContext context) {
            return context.LanguageContext.DeleteMember(context, _target.Evaluate(context), _name);
        }

        public override void Emit(CodeGen cg) {
            cg.EmitPosition(Start, End);

            cg.EmitLanguageContext();
            cg.EmitCodeContext();
            _target.EmitAsObject(cg);
            cg.EmitSymbolId(_name);
            cg.EmitCall(typeof(LanguageContext), "DeleteMember");
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                _target.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }

    public static partial class Ast {
        public static DeleteDynamicMemberExpression Delete(Expression target, SymbolId name) {
            return Delete(SourceSpan.None, target, name);
        }
        public static DeleteDynamicMemberExpression Delete(SourceSpan span, Expression target, SymbolId name) {
            return new DeleteDynamicMemberExpression(span, target, name);
        }
    }
}
