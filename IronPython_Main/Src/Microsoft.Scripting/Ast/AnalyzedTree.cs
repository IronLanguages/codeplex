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
using System.Diagnostics;

namespace Microsoft.Scripting.Ast {
    /// <summary>
    /// This class includes all information that ClosureBinder extracted
    /// from the tree, and which is used for code generation.
    /// </summary>
    class AnalyzedTree {
        /// <summary>
        /// List of all lambdas in the tree.
        /// This is sorted pre-order as a result of the analysis.
        /// </summary>
        private readonly List<LambdaInfo> _lambdas;

        /// <summary>
        /// The dictionary of all lambdas and their infos in the tree.
        /// This includes both instances of LambdaExpression and GeneratorLambdaExpression
        /// </summary>
        private readonly Dictionary<LambdaExpression, LambdaInfo> _infos;

        internal AnalyzedTree(List<LambdaInfo> lambdas, Dictionary<LambdaExpression, LambdaInfo> infos) {
            _lambdas = lambdas;
            _infos = infos;
        }

        internal List<LambdaInfo> Lambdas {
            get {
                return _lambdas;
            }
        }

        internal LambdaInfo GetLambdaInfo(LambdaExpression lambda) {
            Debug.Assert(_infos != null && _infos.ContainsKey(lambda));
            return _infos[lambda];
        }
    }

    /// <summary>
    /// This class includes all information that RuleBinder extracted
    /// from the tree, and which is used for code generation
    /// </summary>
    class AnalyzedRule : AnalyzedTree {
        /// <summary>
        /// The rule doesn't have top-level lambda because the rule
        /// consists of two expressions. This LambdaInfo stores information
        /// about the top level expressions, but has no reference to lambda.
        /// </summary>
        private readonly LambdaInfo _top;

        internal AnalyzedRule(LambdaInfo top, List<LambdaInfo> lambdas, Dictionary<LambdaExpression, LambdaInfo> infos)
            : base(lambdas, infos) {
            _top = top;
        }

        internal LambdaInfo Top {
            get { return _top; }
        }
    }
}
