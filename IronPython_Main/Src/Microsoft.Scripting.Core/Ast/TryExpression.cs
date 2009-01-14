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
using System.Collections.ObjectModel;
using Microsoft.Scripting.Utils;

namespace Microsoft.Linq.Expressions {
    /// <summary>
    /// Represents a try/catch/finally/fault block.
    /// 
    /// The body is protected by the try block.
    /// The handlers consist of a set of <see cref="CatchBlock"/>s that can either be catch or filters.
    /// The fault runs if an exception is thrown.
    /// The finally runs regardless of how control exits the body.
    /// Only one of fault or finally can be supplied.
    /// The return type of the try block must match the return type of any associated catch statements.
    /// </summary>
    public sealed class TryExpression : Expression {
        private readonly Expression _body;
        private readonly ReadOnlyCollection<CatchBlock> _handlers;
        private readonly Expression _finally;
        private readonly Expression _fault;

        internal TryExpression(Expression body, Expression @finally, Expression fault, ReadOnlyCollection<CatchBlock> handlers) {
            _body = body;
            _handlers = handlers;
            _finally = @finally;
            _fault = fault;
        }

        /// <summary>
        /// Gets the static type of the expression that this <see cref="Expression" /> represents. (Inherited from <see cref="Expression"/>.)
        /// </summary>
        /// <returns>The <see cref="Type"/> that represents the static type of the expression.</returns>
        protected override Type GetExpressionType() {
            if (_body == null) {
                return typeof(void);
            }
            return _body.Type;
        }

        /// <summary>
        /// Returns the node type of this <see cref="Expression" />. (Inherited from <see cref="Expression" />.)
        /// </summary>
        /// <returns>The <see cref="ExpressionType"/> that represents this expression.</returns>
        protected override ExpressionType GetNodeKind() {
            return ExpressionType.Try;
        }

        /// <summary>
        /// Gets the <see cref="Expression"/> representing the body of the try block.
        /// </summary>
        public Expression Body {
            get { return _body; }
        }

        /// <summary>
        /// Gets the collection of <see cref="CatchBlock"/>s associated with the try block.
        /// </summary>
        public ReadOnlyCollection<CatchBlock> Handlers {
            get { return _handlers; }
        }

        /// <summary>
        /// Gets the <see cref="Expression"/> representing the finally block.
        /// </summary>
        public Expression Finally {
            get { return _finally; }
        }

        /// <summary>
        /// Gets the <see cref="Expression"/> representing the fault block.
        /// </summary>
        public Expression Fault {
            get { return _fault; }
        }

        internal override Expression Accept(ExpressionVisitor visitor) {
            return visitor.VisitTry(this);
        }
    }

    public partial class Expression {

        /// <summary>
        /// Creates a <see cref="TryExpression"/> representing a try block with a fault block and no catch statements.
        /// </summary>
        /// <param name="body">The body of the try block.</param>
        /// <param name="fault">The body of the fault block.</param>
        /// <returns>The created <see cref="TryExpression"/>.</returns>
        public static TryExpression TryFault(Expression body, Expression fault) {
            return MakeTry(body, null, fault, null);
        }

        /// <summary>
        /// Creates a <see cref="TryExpression"/> representing a try block with a finally block and no catch statements.
        /// </summary>
        /// <param name="body">The body of the try block.</param>
        /// <param name="finally">The body of the finally block.</param>
        /// <returns>The created <see cref="TryExpression"/>.</returns>
        public static TryExpression TryFinally(Expression body, Expression @finally) {
            return MakeTry(body, @finally, null, null);
        }

        /// <summary>
        /// Creates a <see cref="TryExpression"/> representing a try block with any number of catch statements and neither a fault nor finally block.
        /// </summary>
        /// <param name="body">The body of the try block.</param>
        /// <param name="handlers">The array of zero or more <see cref="CatchBlock"/>s representing the catch statements to be associated with the try block.</param>
        /// <returns>The created <see cref="TryExpression"/>.</returns>
        public static TryExpression TryCatch(Expression body, params CatchBlock[] handlers) {
            return MakeTry(body, null, null, handlers);
        }

        /// <summary>
        /// Creates a <see cref="TryExpression"/> representing a try block with any number of catch statements and a finally block.
        /// </summary>
        /// <param name="body">The body of the try block.</param>
        /// <param name="finally">The body of the finally block.</param>
        /// <param name="handlers">The array of zero or more <see cref="CatchBlock"/>s representing the catch statements to be associated with the try block.</param>
        /// <returns>The created <see cref="TryExpression"/>.</returns>
        public static TryExpression TryCatchFinally(Expression body, Expression @finally, params CatchBlock[] handlers) {
            return MakeTry(body, @finally, null, handlers);
        }

        /// <summary>
        /// Creates a <see cref="TryExpression"/> representing a try block with the specified elements.
        /// </summary>
        /// <param name="body">The body of the try block.</param>
        /// <param name="finally">The body of the finally block. Pass null if the try block has no finally block associated with it.</param>
        /// <param name="fault">The body of the t block. Pass null if the try block has no fault block associated with it.</param>
        /// <param name="handlers">A collection of <see cref="CatchBlock"/>s representing the catch statements to be associated with the try block.</param>
        /// <returns>The created <see cref="TryExpression"/>.</returns>
        public static TryExpression MakeTry(Expression body, Expression @finally, Expression fault, IEnumerable<CatchBlock> handlers) {
            RequiresCanRead(body, "body");

            var @catch = handlers.ToReadOnly();
            ContractUtils.RequiresNotNullItems(@catch, "handlers");
            ValidateTryAndCatchHaveSameType(body, @catch);

            if (fault != null) {
                if (@finally != null || @catch.Count > 0) {
                    throw Error.FaultCannotHaveCatchOrFinally();
                }
                RequiresCanRead(fault, "fault");
            } else if (@finally != null) {
                RequiresCanRead(@finally, "finally");
            } else if (@catch.Count == 0) {
                throw Error.TryMustHaveCatchFinallyOrFault();
            }

            return new TryExpression(body, @finally, fault, @catch);
        }

        //Validate that the body of the try expression must have the same type as the body of every try block.
        private static void ValidateTryAndCatchHaveSameType(Expression tryBody, ReadOnlyCollection<CatchBlock> handlers) {
            if (tryBody == null || tryBody.Type == typeof(void)) {
                //The body of every try block must be null or have void type.
                foreach (CatchBlock cb in handlers) {
                    if (cb.Body != null && cb.Body.Type != typeof(void)) {
                        throw Error.BodyOfCatchMustHaveSameTypeAsBodyOfTry();
                    }
                }
            } else {
                //Body of every catch must have the same type of body of try.
                Type type = tryBody.Type;
                foreach (CatchBlock cb in handlers) {
                    if (cb.Body == null || cb.Body.Type != type) {
                        throw Error.BodyOfCatchMustHaveSameTypeAsBodyOfTry();
                    }
                }
            }
        }
    }

}
