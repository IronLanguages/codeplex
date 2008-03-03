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

namespace Microsoft.Scripting.Ast {
    /// <summary>
    /// The base class for ClosureBinder and RuleBinder.
    /// </summary>
    abstract class VariableBinder : Walker {
        /// <summary>
        /// List to store all context statements for further processing - storage allocation
        /// </summary>
        private List<CodeBlockInfo> _blocks;

        /// <summary>
        /// The dictionary of all code blocks and their infos in the tree.
        /// </summary>
        private Dictionary<CodeBlock, CodeBlockInfo> _infos;

        /// <summary>
        /// Stack to keep track of the code block nesting.
        /// </summary>
        private Stack<CodeBlockInfo> _stack;

        protected List<CodeBlockInfo> Blocks {
            get { return _blocks; }
        }

        protected Dictionary<CodeBlock, CodeBlockInfo> Infos {
            get { return _infos; }
        }

        protected Stack<CodeBlockInfo> Stack {
            get { return _stack; }
        }

        private Stack<CodeBlockInfo> NonNullStack {
            get {
                if (_stack == null) {
                    _stack = new Stack<CodeBlockInfo>();
                }
                return _stack;
            }
        }

        protected VariableBinder() {
        }

        #region Walker overrides

        protected internal override bool Walk(BoundAssignment node) {
            Reference(node.Variable);
            return true;
        }

        protected internal override bool Walk(BoundExpression node) {
            Reference(node.Variable);
            return true;
        }

        protected internal override bool Walk(DeleteStatement node) {
            Reference(node.Variable);
            return true;
        }

        protected internal override bool Walk(CatchBlock node) {
            // CatchBlock is not required to have target variable
            if (node.Variable != null) {
                Reference(node.Variable);
            }
            return true;
        }

        protected internal override bool Walk(CodeBlock node) {
            return Push(node);
        }

        protected internal override void PostWalk(CodeBlock node) {
            CodeBlockInfo cbi = Pop();
            Debug.Assert(cbi.CodeBlock == node);
        }

        protected internal override bool Walk(GeneratorCodeBlock node) {
            return Push(node);
        }

        protected internal override void PostWalk(GeneratorCodeBlock node) {
            CodeBlockInfo cbi = Pop();
            Debug.Assert(cbi.CodeBlock == node);
            Debug.Assert(cbi.TopTargets == null);

            // Build the yield targets and store them in the cbi
            YieldLabelBuilder.BuildYieldTargets(node, cbi);
        }

        #endregion

        protected virtual void Reference(Variable variable) {
            Debug.Assert(variable != null);
            _stack.Peek().Reference(variable);
        }

        private bool Push(CodeBlock block) {
            if (_infos == null) {
                _infos = new Dictionary<CodeBlock, CodeBlockInfo>();
                _blocks = new List<CodeBlockInfo>();
            }

            // We've seen this block already
            // (referenced from multiple CodeBlockExpressions)
            if (_infos.ContainsKey(block)) {
                return false;
            }

            Stack<CodeBlockInfo> stack = NonNullStack;

            // The parent of the code block is the block currently
            // on top of the stack, or a null if at top level.
            CodeBlockInfo parent = stack.Count > 0 ? stack.Peek() : null;
            CodeBlockInfo cbi = new CodeBlockInfo(block, parent);

            // Add the block to the list.
            // The blocks are added in prefix order so they
            // will have to be reversed for name binding
            _blocks.Add(cbi);

            // Remember we saw the block already
            _infos[block] = cbi;

            // And push it on the stack.
            stack.Push(cbi);

            return true;
        }

        private CodeBlockInfo Pop() {
            return NonNullStack.Pop();
        }

        private void AddGeneratorTemps(int count) {
            Debug.Assert(_stack.Count > 0);
            _stack.Peek().AddGeneratorTemps(count);
        }

        #region Closure resolution

        protected void BindTheScopes() {
            if (_blocks != null) {
                // Process the blocks in post-order so that
                // all children are processed before parent
                int i = _blocks.Count;
                while (i-- > 0) {
                    CodeBlockInfo cbi = _blocks[i];
                    if (!cbi.CodeBlock.IsGlobal) {
                        BindCodeBlock(cbi);
                    }
                }
            }
        }

        private void BindCodeBlock(CodeBlockInfo block) {
            // If the function is generator or needs custom frame,
            // lift locals to closure
            CodeBlock cb = block.CodeBlock;
            if (cb is GeneratorCodeBlock || cb.EmitLocalDictionary) {
                LiftLocalsToClosure(block);
            }
            ResolveClosure(block);
        }

        private static void LiftLocalsToClosure(CodeBlockInfo block) {
            // Lift all parameters
            foreach (Variable p in block.CodeBlock.Parameters) {
                p.LiftToClosure();
            }
            // Lift all locals
            foreach (Variable d in block.CodeBlock.Variables) {
                if (d.Kind == VariableKind.Local) {
                    d.LiftToClosure();
                }
            }
            block.HasEnvironment = true;
        }

        private void ResolveClosure(CodeBlockInfo block) {
            CodeBlock cb = block.CodeBlock;

            foreach (VariableReference r in block.References.Values) {
                Debug.Assert(r.Variable != null);

                if (r.Variable.Block == cb) {
                    // local reference => no closure
                    continue;
                }

                // Global variables as local
                if (r.Variable.Kind == VariableKind.Global ||
                    (r.Variable.Kind == VariableKind.Local && r.Variable.Block.IsGlobal)) {
                    continue;
                }

                // Lift the variable into the closure
                r.Variable.LiftToClosure();

                // Mark all parent scopes between the use and the definition
                // as closures/environment
                CodeBlockInfo current = block;
                do {
                    current.IsClosure = true;

                    CodeBlockInfo parent = current.Parent;

                    if (parent == null) {
                        throw new ArgumentException(
                            String.Format(
                                "Cannot resolve variable '{0}' " +
                                "referenced from code block '{1}' " +
                                "and defined in code block {2}).\n" +
                                "Is CodeBlock.Parent set correctly?",
                                SymbolTable.IdToString(r.Variable.Name),
                                block.CodeBlock.Name ?? "<unnamed>",
                                r.Variable.Block != null ? (r.Variable.Block.Name ?? "<unnamed>") : "<unknown>"
                            )
                        );
                    }

                    parent.HasEnvironment = true;
                    current = parent;
                } while (current.CodeBlock != r.Variable.Block);
            }
        }

        private CodeBlockInfo GetCodeBlockInfo(CodeBlock block) {
            return _infos[block];
        }

        #endregion
    }
}
