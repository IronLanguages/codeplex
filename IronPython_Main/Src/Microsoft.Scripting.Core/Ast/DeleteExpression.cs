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

using System.Scripting.Actions;
using System.Scripting.Utils;

namespace System.Linq.Expressions {
    /// <summary>
    /// AST node representing dynamic deletion.
    /// 
    /// Supported lvalue types:
    ///     MemberExpression
    ///     (future) BinaryExpression with NodeType == ArrayIndex
    ///     (future) IndexedPropertyExpression
    /// </summary>
    public sealed class DeleteExpression : Expression {
        private readonly Expression _expression;

        internal DeleteExpression(Annotations annotations, Expression expression, CallSiteBinder bindingInfo)
            : base(ExpressionType.Delete, typeof(object), annotations, bindingInfo) { // TODO: typeof(void) ?
            if (IsBound) {
                RequiresBound(expression, "expression");
            }
            _expression = expression;
        }

        public Expression Expression {
            get { return _expression; }
        }
    }

    public partial class Expression {
        public static DeleteExpression DeleteMember(Expression expression, CallSiteBinder binder, Annotations annotations) {
            RequiresCanRead(expression, "expression");
            ContractUtils.RequiresNotNull(binder, "binder");
            return new DeleteExpression(annotations, new MemberExpression(expression, null, null, expression.Type, true, false, binder), binder);
        }
    }
}
