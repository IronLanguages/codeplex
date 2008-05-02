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

using System;
using System.Collections.Generic;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    public sealed class TryStatementBuilder {
        private Expression _try;
        private List<CatchBlock> _catchBlocks;
        private Expression _finally;
        private bool _skipNext;
        private SourceSpan _span;
        private SourceLocation _header;

        internal TryStatementBuilder(SourceSpan span, SourceLocation bodyLocation, Expression body) {
            ContractUtils.RequiresNotNull(body, "body");

            _try = body;
            _header = bodyLocation;
            _span = span;
        }

        public TryStatementBuilder Catch(Type type, Expression body) {
            return Catch(type, (VariableExpression)null, body);
        }

        public TryStatementBuilder Catch(Type type, params Expression[] body) {
            return Catch(type, null, body);
        }

        public TryStatementBuilder Catch(Type type, VariableExpression holder, params Expression[] body) {
            return Catch(type, holder, Expression.Block(body));
        }

        public TryStatementBuilder Catch(Type type, VariableExpression holder, Expression body) {
            if (_skipNext) {
                _skipNext = false;
                return this;
            }

            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.RequiresNotNull(body, "body");

            if (_finally != null) {
                throw new InvalidOperationException("Finally already defined");
            }

            if (_catchBlocks == null) {
                _catchBlocks = new List<CatchBlock>();
            }

            _catchBlocks.Add(Expression.Catch(type, holder, body));
            return this;
        }

        public TryStatementBuilder Filter(Type type, Expression condition, params Expression[] body) {
            return Filter(type, null, condition, body);
        }

        public TryStatementBuilder Filter(Type type, Expression condition, Expression body) {
            return Filter(type, null, condition, body);
        }

        public TryStatementBuilder Filter(Type type, VariableExpression holder, Expression condition, params Expression[] body) {
            return Filter(type, holder, condition, Expression.Block(body));
        }

        public TryStatementBuilder Filter(Type type, VariableExpression holder, Expression condition, Expression body) {
            if (_skipNext) {
                _skipNext = false;
                return this;
            }

            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.RequiresNotNull(condition, "condition");
            ContractUtils.RequiresNotNull(body, "body");

            if (_catchBlocks == null) {
                _catchBlocks = new List<CatchBlock>();
            }

            _catchBlocks.Add(Expression.Catch(type, holder, Expression.IfThenElse(condition, body, Expression.Rethrow())));
            return this;
        }

        public TryStatementBuilder Finally(params Expression[] body) {
            // we need to skip befor creating Expression.Block (body might be null):
            if (_skipNext) {
                _skipNext = false;
                return this;
            }

            return Finally(Expression.Block(body));
        }

        public TryStatementBuilder Finally(Expression body) {
            if (_skipNext) {
                _skipNext = false;
                return this;
            }

            ContractUtils.RequiresNotNull(body, "body");
            if (_finally != null) {
                throw new InvalidOperationException("Finally already defined");
            }

            _finally = body;
            return this;
        }

        public TryStatementBuilder SkipIf(bool condition) {
            _skipNext = condition;
            return this;
        }

        public static implicit operator TryStatement(TryStatementBuilder builder) {
            return ToStatement(builder);
        }

        public static TryStatement ToStatement(TryStatementBuilder builder) {
            ContractUtils.RequiresNotNull(builder, "builder");
            return new TryStatement(
                Expression.Annotate(builder._span, builder._header),
                builder._try,
                (builder._catchBlocks != null) ? CollectionUtils.ToReadOnlyCollection(builder._catchBlocks.ToArray()) : null,
                builder._finally
            );
        }
    }

    public partial class Expression {

        public static TryStatementBuilder Try(params Expression[] body) {
            return new TryStatementBuilder(SourceSpan.None, SourceLocation.None, Expression.Block(body));
        }

        public static TryStatementBuilder Try(Expression body) {
            return new TryStatementBuilder(SourceSpan.None, SourceLocation.None, body);
        }

        public static TryStatementBuilder Try(SourceSpan statementSpan, SourceLocation bodyLocation, Expression body) {
            return new TryStatementBuilder(statementSpan, bodyLocation, body);
        }

        public static TryStatementBuilder Try(SourceSpan span, SourceLocation location, params Expression[] body) {
            return new TryStatementBuilder(span, location, Expression.Block(body));
        }

        public static TryStatement TryCatch(SourceSpan span, SourceLocation header, Expression body, params CatchBlock[] handlers) {
            return TryCatchFinally(span, header, body, handlers, null);
        }

        public static TryStatement TryFinally(Expression body, Expression @finally) {
            return TryCatchFinally(SourceSpan.None, SourceLocation.None, body, null, @finally);
        }

        public static TryStatement TryFinally(SourceSpan span, SourceLocation header, Expression body, Expression @finally) {
            return TryCatchFinally(span, header, body, null, @finally);
        }

        public static TryStatement TryCatchFinally(Expression body, CatchBlock[] handlers, Expression @finally) {
            return new TryStatement(
                Annotate(SourceSpan.None, SourceLocation.None),
                body, CollectionUtils.ToReadOnlyCollection(handlers), @finally
            );
        }

        public static TryStatement TryCatchFinally(SourceSpan span, SourceLocation header, Expression body, CatchBlock[] handlers, Expression @finally) {
            return new TryStatement(
                Annotate(span, header),
                body, CollectionUtils.ToReadOnlyCollection(handlers), @finally
            );
        }
    }
}
