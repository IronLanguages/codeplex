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
using System.Diagnostics;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Ast {
    /// <summary>
    /// GeneratorInfo is a data structure in which Compiler keeps information related co compiling
    /// GeneartorLambdaExpression. It's created by YieldLabelBuilder which is invoked by VariableBinder
    /// </summary>
    class GeneratorInfo {
        /// <summary>
        /// Try statements in this generator
        /// </summary>
        private readonly Dictionary<TryStatement, TryStatementInfo> _tryInfos;

        /// <summary>
        /// Yield statements in this generator
        /// </summary>
        private readonly Dictionary<YieldStatement, YieldTarget> _yieldTargets;

        /// <summary>
        /// The top targets for the generator dispatch.
        /// </summary>
        private readonly IList<YieldTarget> _topTargets;

        /// <summary>
        /// The generator temps required to generate the lambda
        /// </summary>
        private readonly Slot[] _temps;

        /// <summary>
        /// The index of the next temp to hand out
        /// </summary>
        private int _nextTemp;

        internal GeneratorInfo(Dictionary<TryStatement, TryStatementInfo> tryInfos,
                               Dictionary<YieldStatement, YieldTarget> yieldTargets,
                               List<YieldTarget> topTargets,
                               int tempCount) {
            _tryInfos = tryInfos;
            _yieldTargets = yieldTargets;
            _topTargets = topTargets;
            _temps = new Slot[tempCount];
        }

        internal int TempCount {
            get { return _temps.Length; }
        }

        internal IList<YieldTarget> TopTargets {
            get { return _topTargets; }
        }

        internal IList<Slot> Temps {
            get { return _temps; }
        }

        internal TryStatementInfo TryGetTsi(TryStatement ts) {
            TryStatementInfo tsi;
            if (_tryInfos != null && _tryInfos.TryGetValue(ts, out tsi)) {
                return tsi;
            } else {
                return null;
            }
        }

        internal YieldTarget TryGetYieldTarget(YieldStatement ys) {
            YieldTarget yt;
            if (_yieldTargets != null && _yieldTargets.TryGetValue(ys, out yt)) {
                return yt;
            } else {
                return null;
            }
        }

        internal Slot NextGeneratorTemp() {
            Debug.Assert(_nextTemp < _temps.Length);
            return _temps[_nextTemp++];
        }

    }
}
