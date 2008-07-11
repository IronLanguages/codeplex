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

using System.Collections.Generic;
using System.Scripting.Utils;

namespace System.Linq.Expressions {
    public sealed class TryStatementBuilder {
        private readonly List<CatchBlock> _catchBlocks = new List<CatchBlock>();
        private Expression _try;
        private Expression _finally, _fault;
        private bool _skipNext;
        private Annotations _annotations;

        internal TryStatementBuilder(Annotations annotations, Expression body) {
            ContractUtils.RequiresNotNull(body, "body");

            _try = body;
            _annotations = annotations;
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

            _catchBlocks.Add(Expression.Catch(type, holder, body, condition));
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
            } else if (_fault != null) {
                throw new InvalidOperationException("can't have fault and finally");
            }

            _finally = body;
            return this;
        }

        public TryStatementBuilder Fault(params Expression/*!*/[]/*!*/ body) {
            ContractUtils.RequiresNotNullItems(body, "body");

            if (_finally != null) {
                throw new InvalidOperationException("can't have fault and finally");
            } else if (_fault != null) {
                throw new InvalidOperationException("Fault already defined");
            }

            if (body.Length == 1) {
                _fault = body[0];
            } else {
                _fault = Expression.Block(body);
            }

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
                builder._annotations,
                builder._try,
                CollectionUtils.ToReadOnlyCollection(builder._catchBlocks.ToArray()),
                builder._finally,
                builder._fault
            );
        }
    }

    public partial class Expression {
        public static TryStatementBuilder Try(Annotations annotations, params Expression[] body) {
            return new TryStatementBuilder(annotations, Expression.Block(body));
        }

        public static TryStatementBuilder Try(params Expression[] body) {
            return Try(Annotations.Empty, Expression.Block(body));
        }

        public static TryStatementBuilder Try(Expression body) {
            return new TryStatementBuilder(Annotations.Empty, body);
        }

        public static TryStatement TryFault(Expression body, Expression fault) {
            return TryFault(body, fault, Annotations.Empty);
        }

        public static TryStatement TryFault(Expression body, Expression fault, Annotations annotations) {
            return new TryStatement(
                annotations,
                body,
                DefaultReadOnlyCollection<CatchBlock>.Empty,
                null, // finally
                fault
            );
        }

        public static TryStatement TryFinally(Expression body, Expression @finally) {
            return TryCatchFinally(body, null, @finally, null);
        }
        
        public static TryStatement TryCatchFinally(Expression body, CatchBlock[] handlers, Expression @finally) {
                return TryCatchFinally(body, handlers, @finally, null);
        }

        public static TryStatement TryCatchFinally(Expression body, CatchBlock[] handlers, Expression @finally, Expression fault) {
            return TryCatchFinally(body, handlers, @finally, fault, Annotations.Empty);
        }

        public static TryStatement TryCatchFinally(Expression body, CatchBlock[] handlers, Expression @finally, Expression fault, Annotations annotations) {
            return new TryStatement(
                annotations,
                body,
                CollectionUtils.ToReadOnlyCollection(handlers),
                @finally,
                fault
            );
        }

    }
}
