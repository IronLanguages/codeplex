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
using System.Text;
using System.Diagnostics;

namespace Microsoft.Scripting.Ast {
    /// <summary>
    /// The base class for ClosureBinder and RuleBinder.
    /// </summary>
    abstract class VariableBinder : Walker {
        /// <summary>
        /// List to store all context statements for further processing - storage allocation
        /// </summary>
        private List<CodeBlock> _blocks;

        /// <summary>
        /// Stack to keep track of the code block nesting.
        /// </summary>
        private Stack<CodeBlockInfo> _stack;

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
            Push(node);
            return true;
        }

        protected internal override void PostWalk(CodeBlock node) {
            ProcessAndPop(node);
        }

        protected internal override bool Walk(GeneratorCodeBlock node) {
            Push(node);
            return true;
        }

        protected internal override void PostWalk(GeneratorCodeBlock node) {
            int temps = node.BuildYieldTargets();
            AddGeneratorTemps(temps);
            ProcessAndPop(node);
        }

        #endregion

        protected virtual void Reference(Variable variable) {
            Debug.Assert(variable != null);
            _stack.Peek().Reference(variable);
        }

        private void Push(CodeBlock block) {
            NonNullStack.Push(new CodeBlockInfo(block));
        }

        private void ProcessAndPop(CodeBlock block) {
            if (_blocks == null) {
                _blocks = new List<CodeBlock>();
            }

            _blocks.Add(block);
            CodeBlockInfo top = NonNullStack.Pop();
            Debug.Assert(top.CodeBlock == block);
            top.PublishReferences();
        }

        private void AddGeneratorTemps(int count) {
            Debug.Assert(_stack.Count > 0);
            _stack.Peek().AddGeneratorTemps(count);
        }

        #region Closure resolution

        protected void BindTheScopes() {
            if (_blocks != null) {
                for (int i = 0; i < _blocks.Count; i++) {
                    CodeBlock block = _blocks[i];
                    if (!block.IsGlobal) {
                        BindCodeBlock((CodeBlock)block);
                    }
                }
            }
        }

        private void BindCodeBlock(CodeBlock block) {
            // If the function is generator or needs custom frame,
            // lift locals to closure
            if (block is GeneratorCodeBlock || block.EmitLocalDictionary) {
                LiftLocalsToClosure(block);
            }
            ResolveClosure(block);
        }

        private static void LiftLocalsToClosure(CodeBlock block) {
            // Lift all parameters
            foreach (Variable p in block.Parameters) {
                p.LiftToClosure();
            }
            // Lift all locals
            foreach (Variable d in block.Variables) {
                if (d.Kind == Variable.VariableKind.Local) {
                    d.LiftToClosure();
                }
            }
            block.HasEnvironment = true;
        }

        private void ResolveClosure(CodeBlock block) {
            foreach (VariableReference r in block.References.Values) {
                Debug.Assert(r.Variable != null);

                if (r.Variable.Block == block) {
                    // local reference => no closure
                    continue;
                }

                // Global variables as local
                if (r.Variable.Kind == Variable.VariableKind.Global ||
                    (r.Variable.Kind == Variable.VariableKind.Local && r.Variable.Block.IsGlobal)) {
                    continue;
                }

                // Lift the variable into the closure
                r.Variable.LiftToClosure();

                // Mark all parent scopes between the use and the definition
                // as closures/environment
                CodeBlock current = block;
                do {
                    current.IsClosure = true;

                    CodeBlock parent = current.Parent;
                    if (parent == null) {
                        throw new ArgumentException(
                            String.Format(
                                "Cannot resolve variable '{0}' " +
                                "referenced from code block '{1}' " +
                                "and defined in code block {2}).\n" +
                                "Is CodeBlock.Parent set correctly?",
                                SymbolTable.IdToString(r.Variable.Name),
                                block.Name ?? "<unnamed>",
                                r.Variable.Block != null ? (r.Variable.Block.Name ?? "<unnamed>") : "<unknown>"
                            )
                        );
                    }

                    parent.HasEnvironment = true;
                    current = parent;
                } while (current != r.Variable.Block);
            }
        }

        #endregion
    }
}
