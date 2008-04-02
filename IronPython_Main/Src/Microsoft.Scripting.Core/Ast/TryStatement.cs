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

using System.Collections.ObjectModel;

namespace Microsoft.Scripting.Ast {
    public sealed class TryStatement : Expression {
        private readonly Expression _body;
        private readonly ReadOnlyCollection<CatchBlock> _handlers;
        private readonly Expression _finally;

        /// <summary>
        /// Called by <see cref="TryStatementBuilder"/>.
        /// Creates a try/catch/finally/else block.
        /// 
        /// The body is protected by the try block.
        /// The handlers consist of a set of language-dependent tests which call into the LanguageContext.
        /// The elseSuite runs if no exception is thrown.
        /// The finallySuite runs regardless of how control exits the body.
        /// </summary>
        internal TryStatement(Annotations annotations, Expression body, ReadOnlyCollection<CatchBlock> handlers, Expression @finally)
            : base(annotations, AstNodeType.TryStatement, typeof(void)) {
            _body = body;
            _handlers = handlers;
            _finally = @finally;
        }

        internal SourceLocation Header {
            get { return Annotations.Get<SourceLocation>(); }
        }

        public Expression Body {
            get { return _body; }
        }

        public ReadOnlyCollection<CatchBlock> Handlers {
            get { return _handlers; }
        }

        public Expression FinallyStatement {
            get { return _finally; }
        }

        internal int GetGeneratorTempCount() {
            return _finally != null ? 1 : 0;
        }
    }
}
