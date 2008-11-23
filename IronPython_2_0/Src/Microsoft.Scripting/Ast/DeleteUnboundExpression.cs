/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/
using System; using Microsoft;


using Microsoft.Linq.Expressions;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Ast {
    public class DeleteUnboundExpression : Expression {
        private readonly SymbolId _name;

        internal DeleteUnboundExpression(Annotations annotations, SymbolId name)
            : base(typeof(object), true, annotations) {
            _name = name;
        }

        public SymbolId Name {
            get { return _name; }
        }

        public override Expression Reduce() {
            return Expression.Call(
                typeof(ExpressionHelpers).GetMethod("RemoveName"),
                Annotations,
                new [] {
                    Utils.CodeContext(),
                    AstUtils.Constant(_name)
                }
            );
        }

        protected override Expression VisitChildren(ExpressionTreeVisitor visitor) {
            return this;
        }
    }

    public static partial class Utils {
        public static DeleteUnboundExpression Delete(SymbolId name) {
            return Delete(name, SourceSpan.None);
        }
        public static DeleteUnboundExpression Delete(SymbolId name, SourceSpan span) {
            ContractUtils.Requires(!name.IsInvalid && !name.IsEmpty, "name");
            return new DeleteUnboundExpression(Expression.Annotate(span), name);
        }
    }

    public static partial class ExpressionHelpers {
        /// <summary>
        /// Called from generated code, helper to remove a name
        /// </summary>
        public static object RemoveName(CodeContext context, SymbolId name) {
            return context.LanguageContext.RemoveName(context, name);
        }
    }
}
