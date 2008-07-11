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
using System.Collections.ObjectModel;
using System.Scripting.Utils;

namespace System.Linq.Expressions {
    /// <summary>
    /// Represents a try/catch/finally/fault block.
    /// 
    /// The body is protected by the try block.
    /// The handlers consist of a set of CatchBlocks that can either be catch or filters.
    /// The fault runs if an exception is thrown.
    /// The finally runs regardless of how control exits the body.
    /// Only fault or finally can be supplied
    /// </summary>
    public sealed class TryStatement : Expression {
        private readonly Expression _body;
        private readonly ReadOnlyCollection<CatchBlock> _handlers;
        private readonly Expression _finally;
        private readonly Expression _fault;

        internal TryStatement(Annotations annotations, Expression body, ReadOnlyCollection<CatchBlock> handlers, Expression @finally, Expression fault)
            : base(annotations, ExpressionType.TryStatement, typeof(void)) {
            _body = body;
            _handlers = handlers;
            _finally = @finally;
            _fault = fault;
        }

        public Expression Body {
            get { return _body; }
        }

        public ReadOnlyCollection<CatchBlock> Handlers {
            get { return _handlers; }
        }

        public Expression Finally {
            get { return _finally; }
        }

        public Expression Fault {
            get { return _fault; }
        }
    }

    // TODO: CatchBlock handlers come last because they're params--is this
    // confusing? The alternative is to put them after the body but remove
    // params. Fortunately, they're strongly typed and not Expressions which
    // mitigates this concern somewhat.
    public partial class Expression {

        // TryFault
        public static TryStatement TryFault(Expression body, Expression fault) {
            return MakeTry(body, null, fault, Annotations.Empty, null);
        }
        public static TryStatement TryFault(Expression body, Expression fault, Annotations annotations) {
            return MakeTry(body, null, fault, annotations, null);
        }

        // TryFinally
        public static TryStatement TryFinally(Expression body, Expression @finally) {
            return MakeTry(body, @finally, null, Annotations.Empty, null);
        }
        public static TryStatement TryFinally(Expression body, Expression @finally, Annotations annotations) {
            return MakeTry(body, @finally, null, annotations, null);
        }

        // TryCatch
        public static TryStatement TryCatch(Expression body, params CatchBlock[] handlers) {
            return MakeTry(body, null, null, Annotations.Empty, handlers);
        }
        public static TryStatement TryCatch(Expression body, Annotations annotations, params CatchBlock[] handlers) {
            return MakeTry(body, null, null, annotations, handlers);
        }

        // TryCatchFinally
        public static TryStatement TryCatchFinally(Expression body, Expression @finally, params CatchBlock[] handlers) {
            return MakeTry(body, @finally, null, Annotations.Empty, handlers);
        }
        public static TryStatement TryCatchFinally(Expression body, Expression @finally, Annotations annotations, params CatchBlock[] handlers) {
            return MakeTry(body, @finally, null, annotations, handlers);
        }

        // TryCatchFault
        public static TryStatement TryCatchFault(Expression body, Expression fault, params CatchBlock[] handlers) {
            return MakeTry(body, null, fault, Annotations.Empty, handlers);
        }
        public static TryStatement TryCatchFault(Expression body, Expression fault, Annotations annotations, params CatchBlock[] handlers) {
            return MakeTry(body, null, fault, annotations, handlers);
        }

        // MakeTry: the one factory that creates TryStatement
        public static TryStatement MakeTry(Expression body, Expression @finally, Expression fault, Annotations annotations, IEnumerable<CatchBlock> handlers) {
            ContractUtils.RequiresNotNull(body, "body");
            ContractUtils.Requires(@finally == null || fault == null, "cannot have finally and fault");
            ContractUtils.RequiresNotNull(annotations, "annotations");

            var @catch = handlers.ToReadOnly();
            ContractUtils.RequiresNotNullItems(@catch, "handlers");

            ContractUtils.Requires(
                @catch.Count > 0 || @finally != null || fault != null,
                "try must have at least one catch, finally, or fault clause"
            );

            return new TryStatement(
                annotations,
                body,
                @catch,
                @finally,
                fault
            );
        }
    }

}
