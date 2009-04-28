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
using System.Collections.Generic;
using Microsoft.Linq.Expressions;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    public sealed class TryStatementBuilder {
        private readonly List<CatchBlock> _catchBlocks = new List<CatchBlock>();
        private Expression _try;
        private Expression _finally, _fault;
        private Annotations _annotations;

        internal TryStatementBuilder(Annotations annotations, Expression body) {
            _try = body;
            _annotations = annotations;
        }

        public TryStatementBuilder Catch(Type type, Expression body) {
            return Catch(type, (ParameterExpression)null, body);
        }

        public TryStatementBuilder Catch(Type type, params Expression[] body) {
            return Catch(type, null, body);
        }

        public TryStatementBuilder Catch(Type type, ParameterExpression holder, params Expression[] body) {
            return Catch(type, holder, Expression.Block(body));
        }

        public TryStatementBuilder Catch(Type type, ParameterExpression holder, Expression body) {
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.RequiresNotNull(body, "body");

            if (_finally != null) {
                throw Error.FinallyAlreadyDefined();
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

        public TryStatementBuilder Filter(Type type, ParameterExpression holder, Expression condition, params Expression[] body) {
            return Filter(type, holder, condition, Expression.Block(body));
        }

        public TryStatementBuilder Filter(Type type, ParameterExpression holder, Expression condition, Expression body) {
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.RequiresNotNull(condition, "condition");
            ContractUtils.RequiresNotNull(body, "body");

            _catchBlocks.Add(Expression.Catch(type, holder, body, condition));
            return this;
        }

        public TryStatementBuilder Finally(params Expression[] body) {
            return Finally(Expression.Block(body));
        }

        public TryStatementBuilder Finally(Expression body) {
            ContractUtils.RequiresNotNull(body, "body");
            if (_finally != null) {
                throw Error.FinallyAlreadyDefined();
            } else if (_fault != null) {
                throw Error.CannotHaveFaultAndFinally();
            }

            _finally = body;
            return this;
        }

        public TryStatementBuilder Fault(params Expression[] body) {
            ContractUtils.RequiresNotNullItems(body, "body");

            if (_finally != null) {
                throw Error.CannotHaveFaultAndFinally();
            } else if (_fault != null) {
                throw Error.FaultAlreadyDefined();
            }

            if (body.Length == 1) {
                _fault = body[0];
            } else {
                _fault = Expression.Block(body);
            }

            return this;
        }

        public static implicit operator Expression(TryStatementBuilder builder) {
            return ToStatement(builder);
        }

        public static Expression ToStatement(TryStatementBuilder builder) {
            ContractUtils.RequiresNotNull(builder, "builder");
            var result = Expression.MakeTry(
                builder._try,
                builder._finally,
                builder._fault,
                builder._annotations,
                builder._catchBlocks
            );
            if (result.Finally != null || result.Fault != null) {
                return Utils.FinallyFlowControl(result);
            }
            return result;
        }
    }

    public partial class Utils {
        public static TryStatementBuilder Try(Annotations annotations, params Expression[] body) {
            ContractUtils.RequiresNotNull(body, "body");
            return new TryStatementBuilder(annotations, Expression.Block(body));
        }

        public static TryStatementBuilder Try(params Expression[] body) {
            return Try(Annotations.Empty, Expression.Block(body));
        }

        public static TryStatementBuilder Try(Expression body) {
            ContractUtils.RequiresNotNull(body, "body");
            return new TryStatementBuilder(Annotations.Empty, body);
        }
    }
}