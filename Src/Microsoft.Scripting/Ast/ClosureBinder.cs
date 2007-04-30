/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/
using System;
using System.Diagnostics;
using System.Collections.Generic;

using Microsoft.Scripting.Internal.Generation;

namespace Microsoft.Scripting.Internal.Ast {
    /// <summary>
    /// Ths ClosureBinder takes as an input a bound AST tree in which each Reference is initialized with respective Definition.
    /// The ClosureBinder will then resolve Reference->Definition relationships which span multiple scopes and ensure that the
    /// functions are properly marked with IsClosure and HasEnvironment flags.
    /// </summary>
    class ClosureBinder : Walker {
        CodeBlock _global;

        /// <summary>
        /// List to store all context statements for further processing - storage allocation
        /// </summary>
        List<CodeBlock> _blocks = new List<CodeBlock>();
        Stack<CodeBlock> _stack = new Stack<CodeBlock>();

        public static void Bind(CodeBlock ast) {
            ClosureBinder db = new ClosureBinder((CodeBlock)ast);
            db.DoBind();
        }

        private ClosureBinder(CodeBlock global) {
            _global = global;
        }

        #region AstWalker overrides

        public override bool Walk(CodeBlock node) {
            Push(node);
            return true;
        }
        public override void PostWalk(CodeBlock node) {
            AddAndPop(node);
        }

        public override bool Walk(GeneratorCodeBlock node) {
            Push(node);
            return true;
        }
        public override void PostWalk(GeneratorCodeBlock node) {
            AddAndPop(node);
        }

        private void Push(CodeBlock block) {
            _stack.Push(block);
        }

        private void AddAndPop(CodeBlock block) {
            _blocks.Add(block);
            CodeBlock top = _stack.Pop();
            Debug.Assert(top == block);
        }

        // Temporary variable allocation
        public override void PostWalk(TryStatement node) {
            AddGeneratorTemps(TryStatement.GeneratorTemps);
        }

        private void AddGeneratorTemps(int count) {
            Debug.Assert(_stack.Count > 0);
            _stack.Peek().AddGeneratorTemps(count);
        }

        #endregion

        private void DoBind() {
            // Collect the context statements
            _global.Walk(this);

            BindTheScopes();
        }

        // TODO: Alternatively, this can be virtual method on ScopeStatement
        // or also implemented directly in the walker (PostWalk)
        private void BindTheScopes() {
            for (int i = 0; i < _blocks.Count; i++) {
                CodeBlock block = _blocks[i];
                if (!block.IsGlobal) BindCodeBlock((CodeBlock)block);
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

        private void LiftLocalsToClosure(CodeBlock block) {
            // Lift all parameters
            foreach (Parameter p in block.Parameters) {
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
            foreach (VariableReference r in block.References) {
                if (r.Variable == null) {
                    // Unbound variable dynamically resolved at runtime
                    continue;
                }

                if (r.Variable.Block == block) {
                    // local reference => no closure
                    continue;
                }

                // Global variables as local
                if (r.Variable.Kind == Variable.VariableKind.Global || 
                    (r.Variable.Kind == Variable.VariableKind.Local && r.Variable.Block.IsGlobal)) {
                    Debug.Assert(r.Variable.Block == _global);
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
                        throw new ArgumentException("Cannot resolve closure");
                    }

                    parent.HasEnvironment = true;
                    current = parent;
                } while (current != r.Variable.Block);
            }
        }
    }
}
