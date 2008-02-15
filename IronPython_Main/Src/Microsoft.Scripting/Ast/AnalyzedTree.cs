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
        /// List of all blocks in the tree.
        /// This is sorted pre-order as a result of the analysis.
        /// </summary>
        private readonly List<CodeBlockInfo> _blocks;

        /// <summary>
        /// The dictionary of all code blocks and their infos in the tree.
        /// This includes both instances of CodeBlock and GeneratorCodeBlock
        /// </summary>
        private readonly Dictionary<CodeBlock, CodeBlockInfo> _infos;

        internal AnalyzedTree(List<CodeBlockInfo> blocks, Dictionary<CodeBlock, CodeBlockInfo> infos) {
            _blocks = blocks;
            _infos = infos;
        }

        internal List<CodeBlockInfo> Blocks {
            get {
                return _blocks;
            }
        }

        internal CodeBlockInfo GetCbi(CodeBlock cb) {
            Debug.Assert(_infos != null && _infos.ContainsKey(cb));
            return _infos[cb];
        }
    }

    /// <summary>
    /// This class includes all information that RuleBinder extracted
    /// from the tree, and which is used for code generation
    /// </summary>
    class AnalyzedRule : AnalyzedTree {
        /// <summary>
        /// The rule doesn't have top-level code block because the rule
        /// consists of two expressions. This CodeBlockInfo stores information
        /// about the top level expressions, but has no reference to code block.
        /// </summary>
        private readonly CodeBlockInfo _top;

        internal AnalyzedRule(CodeBlockInfo top, List<CodeBlockInfo> blocks, Dictionary<CodeBlock, CodeBlockInfo> infos)
            : base(blocks, infos) {
            _top = top;
        }

        internal CodeBlockInfo Top {
            get { return _top; }
        }
    }
}
