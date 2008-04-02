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
using System.Diagnostics;

namespace Microsoft.Scripting.Ast {
    class TryStatementInfo {
        private readonly TryStatement _ts;

        /// <summary>
        /// The entry point into the try statement
        /// </summary>
        private TargetLabel _target;

        /// <summary>
        /// One or more of the catch blocks includes yield.
        /// </summary>
        private bool _yieldInCatch;

        /// <summary>
        /// Labels for the yields inside a try block.
        /// </summary>
        private List<YieldTarget> _tryYields;

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
        /// For each catch block we have flag whether this block yields.
        /// This is a parallel array to the TryStatement.Handlers
        /// </summary>
        private bool[] _catchYieldsFlags;

        internal TryStatementInfo(TryStatement ts) {
            _ts = ts;
        }

        internal List<YieldTarget> TryYields {
            get { return _tryYields; }
        }

        internal bool YieldInCatch {
            get { return _yieldInCatch; }
        }

        internal List<YieldTarget> CatchYields {
            get { return _catchYields; }
        }

        internal List<YieldTarget> FinallyYields {
            get { return _finallyYields; }
        }

        internal TargetLabel Target {
            get { return _target; }
        }

        private TargetLabel EnsureTopTarget() {
            if (_target == null) {
                _target = new TargetLabel();
            }
            return _target;
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

            ReadOnlyCollection<CatchBlock> handlers = _ts.Handlers;

            Debug.Assert(handlers != null && handler < handlers.Count);
            CatchBlock cb = handlers[handler];

            _catchYieldsFlags[handler] = true;
            _yieldInCatch = true;

            if (_ts.FinallyStatement != null) {
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

        internal static void AddYieldTarget(ref List<YieldTarget> list, TargetLabel target, int index) {
            if (list == null) {
                list = new List<YieldTarget>();
            }
            list.Add(new YieldTarget(index, target));
        }

        internal void CreateCatchFlags(int count) {
            _catchYieldsFlags = new bool[count];
        }

        internal bool CatchBlockYields(int index) {
            Debug.Assert(_catchYieldsFlags != null && index < _catchYieldsFlags.Length);
            return _catchYieldsFlags[index];
        }
    }
}
