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
    /// (CodeBlocks). Each of the individual lambdas is compiled by the LambdaCompiler.
    /// The LambdaCompilers used to compile the tree are cached inside _compilers dictionary.
    /// </summary>
    class Compiler {
        /// <summary>
        /// The dictionary of all compilers that are compiling individual code blocks/lambdas.
        /// </summary>
        private readonly Dictionary<CodeBlock, LambdaCompiler> _compilers = new Dictionary<CodeBlock, LambdaCompiler>();

        /// <summary>
        /// Analyzed tree - the binding information etc.
        /// </summary>
        private readonly AnalyzedTree _at;

        internal Compiler(AnalyzedTree at) {
            Debug.Assert(at != null);
            _at = at;
        }

        /// <summary>
        /// Returns the Compiler implementing the code block.
        /// Emits the code block implementation if it hasn't been emitted yet.
        /// </summary>
        internal LambdaCompiler ProvideCodeBlockImplementation(LambdaCompiler outer, CodeBlock block, bool closure, bool hasThis) {
            Debug.Assert(block != null);
            LambdaCompiler impl;

            // Emit the code block body if it hasn't been emitted yet
            if (!_compilers.TryGetValue(block, out impl)) {
                impl = LambdaCompiler.CreateCodeBlockCompiler(outer, block, closure, hasThis);
                impl.InitializeCompilerAndBlock(this, block);
                impl.EmitFunctionImplementation(GetCbi(block));
                impl.Finish();

                _compilers.Add(block, impl);
            }

            return impl;
        }

        /// <summary>
        /// Finds the CodeBlockInfo for the given code block in the
        /// AnalyzedTree. The code block must be there since the _at
        /// came out of the analysis of the ast being compiled.
        /// </summary>
        internal CodeBlockInfo GetCbi(CodeBlock cb) {
            return _at.GetCbi(cb);
        }
    }
}
