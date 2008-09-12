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
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {

    /// <summary>
    /// Evaluates to the CodeContext that's currently in scope
    /// 
    /// TODO: this should go away as an intrinsic in favor of languages
    ///       tracking their own scope chain explicitly
    /// </summary>
    public sealed class CodeContextExpression : Expression {
        internal CodeContextExpression(Annotations annotations)
            : base(typeof(CodeContext), false, annotations) {
        }
    }

    /// <summary>
    /// Creates a new scope where the specified CodeContext will be valid
    /// 
    /// TODO: this should go away as an intrinsic in favor of languages
    ///       tracking their own scope chain explicitly
    /// </summary>
    public sealed class CodeContextScopeExpression : Expression {
        private readonly Expression _newContext;
        private readonly Expression _body;

        internal CodeContextScopeExpression(Annotations annotations, Expression body, Expression newContext)
            : base(body.Type, false, annotations) {
            _body = body;
            _newContext = newContext;
        }

        /// <summary>
        /// The body where the new CodeContext can be used
        /// </summary>
        public Expression Body {
            get { return _body; }
        }

        /// <summary>
        /// The expression that initializes the new CodeContext for this scope
        /// </summary>
        public Expression NewContext {
            get { return _newContext; }
        }
    }

    public partial class Utils {
        public static Expression CodeContext() {
            return CodeContext(Annotations.Empty);
        }
        public static Expression CodeContext(Annotations annotations) {
            return new CodeContextExpression(annotations);
        }
        public static CodeContextScopeExpression CodeContextScope(Expression body, Expression newContext) {
            return CodeContextScope(body, newContext, Annotations.Empty);
        }
        public static CodeContextScopeExpression CodeContextScope(Expression body, Expression newContext, Annotations annotations) {
            ContractUtils.RequiresNotNull(body, "body");
            ContractUtils.RequiresNotNull(newContext, "newContext");
            ContractUtils.Requires(TypeUtils.AreAssignable(typeof(CodeContext), newContext.Type), "newContext");
            ContractUtils.Requires(newContext.CanRead, "newContext");
            return new CodeContextScopeExpression(annotations, body, newContext);
        }
    }
}
