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
    public sealed class ScopeStatement : Expression, ISpan {
        private readonly Expression _scope;
        private readonly Expression/*!*/ _body;

        private readonly SourceLocation _start;
        private readonly SourceLocation _end;

        internal ScopeStatement(SourceLocation start, SourceLocation end, Expression scope, Expression/*!*/ body)
            : base(AstNodeType.ScopeStatement, typeof(void)) {
            _start = start;
            _end = end;
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

        public SourceLocation Start {
            get { return _start; }
        }

        public SourceLocation End {
            get { return _end; }
        }
    }

    /// <summary>
    /// Factory methods.
    /// </summary>
    public static partial class Ast {
        public static ScopeStatement Scope(Expression/*!*/ scope, Expression/*!*/ body) {
            return Scope(SourceSpan.None, scope, body);
        }

        public static ScopeStatement Scope(SourceSpan span, Expression/*!*/ scope, Expression/*!*/ body) {
// TODO:            Contract.RequiresNotNull(scope, "scope");
            Contract.RequiresNotNull(body, "body");
// TODO:            Contract.Requires(TypeUtils.CanAssign(typeof(LocalScope), scope.Type), "scope", "Scope must of type LocalScope");

            return new ScopeStatement(span.Start, span.End, scope, body);
        }
    }
}
