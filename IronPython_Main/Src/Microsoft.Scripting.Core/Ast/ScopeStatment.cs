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

using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    public sealed class ScopeStatement : Expression {
        private readonly Expression _scope;
        private readonly Expression/*!*/ _body;

        internal ScopeStatement(Annotations annotations, Expression scope, Expression/*!*/ body)
            : base(annotations, AstNodeType.ScopeStatement, typeof(void)) {
            _scope = scope;
            _body = body;
        }

        public Expression Scope {
            get {
                return _scope;
            }
        }

        public Expression/*!*/ Body {
            get {
                return _body;
            }
        }
    }

    /// <summary>
    /// Factory methods.
    /// </summary>
    public static partial class Ast {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames")] // TODO: fix
        public static ScopeStatement Scope(Expression/*!*/ scope, Expression/*!*/ body) {
            return Scope(SourceSpan.None, scope, body);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames")] // TODO: fix
        public static ScopeStatement Scope(SourceSpan span, Expression/*!*/ scope, Expression/*!*/ body) {
            // TODO:            Contract.RequiresNotNull(scope, "scope");
            Contract.RequiresNotNull(body, "body");
            // TODO:            Contract.Requires(TypeUtils.CanAssign(typeof(LocalScope), scope.Type), "scope", "Scope must of type LocalScope");

            return new ScopeStatement(Annotations(span), scope, body);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames")] // TODO: fix
        public static ScopeStatement Scope(Annotations annotations, Expression/*!*/ scope, Expression/*!*/ body) {
            // TODO:            Contract.RequiresNotNull(scope, "scope");
            Contract.RequiresNotNull(body, "body");
            // TODO:            Contract.Requires(TypeUtils.CanAssign(typeof(LocalScope), scope.Type), "scope", "Scope must of type LocalScope");

            return new ScopeStatement(annotations, scope, body);
        }

    }
}
