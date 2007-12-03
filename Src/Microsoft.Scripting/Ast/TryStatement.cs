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
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Scripting.Ast {
    public sealed class TryStatement : Statement {
        private readonly SourceLocation _header;
        private readonly Statement _body;
        private readonly ReadOnlyCollection<CatchBlock> _handlers;
        private readonly Statement _finally;

        private TargetLabel _target;    // The entry point into the try statement

        /// <summary>
        /// One or more of the catch blocks includes yield.
        /// </summary>
        private bool _yieldInCatch;

        /// <summary>
        /// Labels for the yields inside a try block.
        /// </summary>
        private List<YieldTarget> _tryYields;               // TODO: Move to Compiler !!!

        /// <summary>
        /// Labels for the yields inside the catch clause.
        /// This is only valid for the try statement with a 'finally' clause,
        /// in which case we need to enter the outer try, and then dispatch
        /// to the yield labels
        /// For try statement without finally, the yields contained within
        /// catch are hoisted outside of the try and as such don't need
        /// to be tracked
        /// </summary>
        private List<YieldTarget> _catchYields;

        /// <summary>
        /// Labels for the yields inside a finally block.
        /// </summary>
        private List<YieldTarget> _finallyYields;

        /// <summary>
        /// Called by <see cref="TryStatementBuilder"/>.
        /// Creates a try/catch/finally/else block.
        /// 
        /// The body is protected by the try block.
        /// The handlers consist of a set of language-dependent tests which call into the LanguageContext.
        /// The elseSuite runs if no exception is thrown.
        /// The finallySuite runs regardless of how control exits the body.
        /// </summary>
        internal TryStatement(SourceSpan span, SourceLocation header, Statement body, ReadOnlyCollection<CatchBlock> handlers, Statement @finally)
            : base(AstNodeType.TryStatement, span) {
            _body = body;
            _handlers = handlers;
            _finally = @finally;
            _header = header;
        }

        public SourceLocation Header {
            get { return _header; }
        }

        public Statement Body {
            get { return _body; }
        }

        public ReadOnlyCollection<CatchBlock> Handlers {
            get { return _handlers; }
        }

        public Statement FinallyStatement {
            get { return _finally; }
        }

        // TODO: Move to Compiler
        internal List<YieldTarget> TryYields {
            get { return _tryYields; }
        }

        // TODO: Move to Compiler
        internal bool YieldInCatch {
            get { return _yieldInCatch; }
        }

        // TODO: Move to Compiler
        internal List<YieldTarget> CatchYields {
            get { return _catchYields; }
        }

        // TODO: Move to Compiler
        internal List<YieldTarget> FinallyYields {
            get { return _finallyYields; }
        }

        // TODO: Move to Compiler
        internal TargetLabel Target {
            get { return _target; }
        }

        private TargetLabel EnsureTopTarget() {
            if (_target == null) {
                _target = new TargetLabel();
            }
            return _target;
        }

        internal static void AddYieldTarget(ref List<YieldTarget> list, TargetLabel target, int index) {
            if (list == null) {
                list = new List<YieldTarget>();
            }
            list.Add(new YieldTarget(index, target));
        }

        internal TargetLabel AddTryYieldTarget(TargetLabel label, int index) {
            // Yield inside try stays inside try block, so we need to
            // remember the target label.

            AddYieldTarget(ref _tryYields, label, index);
            return EnsureTopTarget();
        }

        internal TargetLabel AddCatchYieldTarget(TargetLabel label, int index, int handler) {
            // Yields inside catch blocks are hoisted out of the catch.
            // If the catch block has a finally, though, it will get wrapped in
            // another try block, in which case the direct jump is not possible
            // and code must route through the top target.

            Debug.Assert(_handlers != null && handler < _handlers.Count);
            CatchBlock cb = _handlers[handler];

            cb.Yield = true;
            _yieldInCatch = true;

            if (_finally != null) {
                AddYieldTarget(ref _catchYields, label, index);
                return EnsureTopTarget();
            } else {
                return label;
            }
        }

        internal TargetLabel AddFinallyYieldTarget(TargetLabel label, int index) {
            // Yields inside finally stay inside finally so we need to keep track
            // of them.
            AddYieldTarget(ref _finallyYields, label, index);
            return EnsureTopTarget();
        }

        internal int GetGeneratorTempCount() {
            return _finally != null ? 1 : 0;
        }
    }
}
