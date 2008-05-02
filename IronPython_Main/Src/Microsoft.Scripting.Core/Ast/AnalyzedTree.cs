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
    /// This class includes all information that LambdaBinder extracted
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

        /// <summary>
        /// The dictionary of all generators and their infos in the tree.
        /// </summary>
        private readonly Dictionary<GeneratorLambdaExpression, GeneratorInfo> _generators;

        internal AnalyzedTree(List<LambdaInfo> lambdas, Dictionary<LambdaExpression, LambdaInfo> infos, Dictionary<GeneratorLambdaExpression, GeneratorInfo> generators) {
            _lambdas = lambdas;
            _infos = infos;
            _generators = generators;
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

        internal GeneratorInfo GetGeneratorInfo(GeneratorLambdaExpression lambda) {
            Debug.Assert(_generators != null && _generators.ContainsKey(lambda));
            return _generators[lambda];
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

        internal AnalyzedRule(LambdaInfo top, List<LambdaInfo> lambdas, Dictionary<LambdaExpression, LambdaInfo> infos, Dictionary<GeneratorLambdaExpression, GeneratorInfo> generators)
            : base(lambdas, infos, generators) {
            _top = top;
        }

        internal LambdaInfo Top {
            get { return _top; }
        }
    }
}
