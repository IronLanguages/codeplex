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
    /// The object responsible for compiling the whole tree. The tree may contain multiple lambdas
    /// Each of the individual lambdas is compiled by the LambdaCompiler.
    /// The LambdaCompilers used to compile the tree are cached inside _compilers dictionary.
    /// </summary>
    class Compiler {
        /// <summary>
        /// The dictionary of all compilers that are compiling individual lambdas.
        /// </summary>
        private readonly Dictionary<LambdaExpression, LambdaCompiler> _compilers = new Dictionary<LambdaExpression, LambdaCompiler>();

        /// <summary>
        /// Analyzed tree - the binding information etc.
        /// </summary>
        private readonly AnalyzedTree _at;

        internal Compiler(AnalyzedTree at) {
            Debug.Assert(at != null);
            _at = at;
        }

        /// <summary>
        /// Returns the Compiler implementing the lambda.
        /// Emits the lambda implementation if it hasn't been emitted yet.
        /// </summary>
        internal LambdaCompiler ProvideLambdaImplementation(LambdaCompiler outer, LambdaExpression lambda, bool closure) {
            Debug.Assert(lambda != null);
            LambdaCompiler impl;

            // Emit the lambda body if it hasn't been emitted yet
            if (!_compilers.TryGetValue(lambda, out impl)) {
                impl = LambdaCompiler.CreateLambdaCompiler(outer, lambda, closure);
                impl.InitializeCompilerAndLambda(this, lambda);
                impl.EmitFunctionImplementation(GetLambdaInfo(lambda));
                impl.Finish();

                _compilers.Add(lambda, impl);
            }

            return impl;
        }

        /// <summary>
        /// Finds the LambdaInfo for the given lambda in the
        /// AnalyzedTree. The lambda must be there since the _at
        /// came out of the analysis of the ast being compiled.
        /// </summary>
        internal LambdaInfo GetLambdaInfo(LambdaExpression lambda) {
            return _at.GetLambdaInfo(lambda);
        }
    }
}
