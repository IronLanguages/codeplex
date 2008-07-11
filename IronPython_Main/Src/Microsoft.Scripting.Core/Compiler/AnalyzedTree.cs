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

namespace System.Linq.Expressions {
    /// <summary>
    /// This class includes all information that LambdaBinder extracted
    /// from the tree, and which is used for code generation.
    /// </summary>
    class AnalyzedTree {
        /// <summary>
        /// The dictionary of all lambdas and their infos in the tree.
        /// This includes both instances of LambdaExpression and GeneratorLambdaExpression
        /// </summary>
        private readonly Dictionary<Expression, CompilerScope> _infos;

        /// <summary>
        /// The dictionary of all generators and their infos in the tree.
        /// </summary>
        private readonly Dictionary<LambdaExpression, GeneratorInfo> _generators;

        internal AnalyzedTree(Dictionary<Expression, CompilerScope> infos, Dictionary<LambdaExpression, GeneratorInfo> generators) {
            _infos = infos;
            _generators = generators;
        }

        internal CompilerScope GetCompilerScope(Expression scope) {
            Debug.Assert(_infos != null);
            CompilerScope result;
            _infos.TryGetValue(scope, out result);
            return result;
        }

        internal GeneratorInfo GetGeneratorInfo(LambdaExpression lambda) {
            Debug.Assert(_generators != null && _generators.ContainsKey(lambda));
            return _generators[lambda];
        }
    }
}
