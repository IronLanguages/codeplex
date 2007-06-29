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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.Scripting.Generation;

/*
 * The data flow.
 * 
 * Each local name is represented as 2 bits:
 * One is for definitive assignment, the other is for uninitialized use detection.
 * The only difference between the two is behavior on delete.
 * On delete, the name is not assigned to meaningful value (we need to check at runtime if it's initialized),
 * but it is not uninitialized either (because delete statement will set it to Uninitialized.instance).
 * This way, codegen doesn?t have to emit an explicit initialization for it.
 * 
 * Consider:
 * 
 * def f():
 *     print a  # uninitialized use
 *     a = 10
 * 
 * We compile this into:
 * 
 * static void f$f0() {
 *     object a = Uninitialized.instance; // explicit initialization because of the uninitialized use
 *     // print statement
 *     if(a == Uninitialized.instance)
 *       throw ThrowUnboundLocalError("a");
 *     else
 *       Ops.Print(a);
 *     // a = 10
 *     a = 10
 * }
 * 
 * Whereas:
 * 
 * def f():
 *     a = 10
 *     del a        # explicit deletion which will set to Uninitialized.instance
 *     print a
 * 
 * compiles into:
 * 
 * static void f$f0() {
 *     object a = 10;                        // a = 10
 *     a = Uninitialized.instance;           // del a
 *     if(a == Uninitialized.instance)       // print a
 *       throw ThrowUnboundLocalError("a");
 *     else
 *       Ops.Print(a);
 * }
 * 
 * The bit arrays in the flow checker hold the state and upon encountering NameExpr we figure
 * out whether the name has not yet been initialized at all (in which case we need to emit the
 * first explicit assignment to Uninitialized.instance and guard the use with an inlined check
 * or whether it is definitely assigned (we don't need to inline the check)
 * or whether it may be uninitialized, in which case we must only guard the use by inlining the Uninitialized check
 * 
 * More details on the bits.
 * 
 * First bit:
 *  1 .. value is definitely assigned (initialized and != Uninitialized.instance)
 *  0 .. value may be uninitialized or set to Uninitialized.instance
 * Second bit:
 *  1 .. is definitely initialized  (including definitely initialized to Uninitialized.instance)
 *  0 .. may be uninitialized
 * 
 * In combination:
 *  11 .. initialized
 *  10 .. invalid
 *  01 .. deleted
 *  00 .. may be uninitialized
 */

namespace Microsoft.Scripting.Ast {
    class FlowChecker : Walker {
        private BitArray _bits;
        private Stack<BitArray> _loops;

        CodeBlock _block;

        Dictionary<Variable, int> _indices = new Dictionary<Variable,int>();

        private FlowChecker(CodeBlock block) {
            List<Variable> variables = block.Variables;
            List<Variable> parameters = block.Parameters;

            _bits = new BitArray((variables.Count + parameters.Count) * 2);
            int index = 0;
            foreach (Variable variable in variables) {
                _indices[variable] = index++;
            }
            foreach (Variable parameter in parameters) {
                _indices[parameter] = index++;
            }
            _block = block;
        }

        private bool TryGetIndex(Variable variable, out int index) {
            if (variable != null) {
                if (_indices.TryGetValue(variable, out index)) {
                    index *= 2;
                    return true;
                } else {
                    // locals and parameters must have be tracked, except for global scope
                    Debug.Assert(
                        variable.Kind != Variable.VariableKind.Local &&
                        variable.Kind != Variable.VariableKind.Parameter ||
                        variable.Lift ||
                        variable.Block.IsGlobal,
                        "Untracked local/parameter " + variable.Name.ToString()
                    );
                }
            }

            index = -1;
            return false;
        }

        private bool TryCheckVariable(Variable variable, out bool defined) {
            int index;
            if (TryGetIndex(variable, out index)) {
                Debug.Assert(index < _bits.Count);
                defined = _bits.Get(index);
                if (!defined) {
                    variable.UninitializedUse();
                }
                if (!_bits.Get(index + 1)) {
                    // Found an unbound use of the name => need to initialize to Uninitialized.instance
                    variable.UnassignedUse();
                }
                return true;
            } else {
                // TODO: Report unbound name - error
                defined = false;
                return false;
            }
        }

        #region Public API

        public static void Check(CodeBlock block) {
            FlowChecker fc = new FlowChecker(block);
            block.Walk(fc);
        }

        #endregion

        public void Define(Variable variable) {
            int index;
            if (TryGetIndex(variable, out index)) {
                _bits.Set(index, true);      // is assigned
                _bits.Set(index + 1, true);  // cannot be unassigned
            }
        }

        public void Delete(Variable variable) {
            int index;
            if (TryGetIndex(variable, out index)) {
                _bits.Set(index, false);     // is not initialized
                _bits.Set(index + 1, true);  // is assigned (to Uninitialized.instance)
            }
        }

        private void PushLoop(BitArray ba) {
            if (_loops == null) {
                _loops = new Stack<BitArray>();
            }
            _loops.Push(ba);
        }

        private BitArray PeekLoop() {
            return _loops != null ? _loops.Peek() : null;
        }

        private void PopLoop() {
            if (_loops != null) _loops.Pop();
        }

        #region AstWalker Methods

        // BoundExpression
        public override bool Walk(BoundExpression node) {
            bool defined;
            if (TryCheckVariable(node.Variable, out defined)) {
                node.IsDefined = defined;
            }
            return true;
        }

        // BoundAssignment
        public override bool Walk(BoundAssignment node) {
            if (node.Operator != Operators.None) {
                bool defined;
                if (TryCheckVariable(node.Variable, out defined)) {
                    node.IsDefined = defined;
                }
            }
            node.Value.Walk(this);
            Define(node.Variable);
            return false;
        }


        // BreakStatement
        public override bool Walk(BreakStatement node) {
            BitArray exit = PeekLoop();
            if (exit != null) { // break outside loop
                exit.And(_bits);
            }
            return true;
        }

        // ContinueStatement
        public override bool Walk(ContinueStatement node) { return true; }

        // DelStatement
        public override bool Walk(DeleteStatement node) {
            bool defined;
            if (TryCheckVariable(node.Variable, out defined)) {
                node.IsDefined = defined;
            }
            return true;
        }

        public override void PostWalk(DeleteStatement node) {
            Delete(node.Variable);
        }

        // CodeBlockExpression interrupt flow analysis
        public override bool Walk(CodeBlockExpression node) {
            return false;
        }

        // CodeBlock
        public override bool Walk(CodeBlock node) {
            foreach (Variable p in node.Parameters) {
                // Define the parameters
                Define(p);
            }
            return true;
        }

        // GeneratorCodeBlock
        public override bool Walk(GeneratorCodeBlock node) {
            return Walk((CodeBlock)node);
        }

        // DoStatement
        public override bool Walk(DoStatement node) {
            BitArray loop = new BitArray(_bits); // State at the loop entry with which the loop runs
            BitArray save = _bits;               // Save the state at loop entry

            // Prepare loop exit state
            BitArray exit = new BitArray(_bits.Length, true);
            PushLoop(exit);

            // Loop will be flown starting from the current state
            _bits = loop;

            // Walk the loop
            node.Body.Walk(this);
            // Walk the test in the context of the loop
            node.Test.Walk(this);

            // Handle the loop exit
            PopLoop();
            _bits.And(exit);

            // Restore the state after walking the loop
            _bits = save;
            _bits.And(loop);

            return false;
        }

        // IfStatement
        public override bool Walk(IfStatement node) {
            BitArray result = new BitArray(_bits.Length, true);
            BitArray save = _bits;

            _bits = new BitArray(_bits.Length);

            foreach (IfStatementTest ist in node.Tests) {
                // Set the initial branch value to bits
                _bits.SetAll(false);
                _bits.Or(save);

                // Flow the test first
                ist.Test.Walk(this);
                // Flow the body
                ist.Body.Walk(this);
                // Intersect
                result.And(_bits);
            }

            // Set the initial branch value to bits
            _bits.SetAll(false);
            _bits.Or(save);

            if (node.ElseStatement != null) {
                // Flow the else_
                node.ElseStatement.Walk(this);
            }

            // Intersect
            result.And(_bits);

            _bits = save;

            // Remember the result
            _bits.SetAll(false);
            _bits.Or(result);
            return false;
        }

        // DynamicTryStatement
        public override bool Walk(DynamicTryStatement node) {
            BitArray save = _bits;
            _bits = new BitArray(_bits);

            // Flow the body
            node.Body.Walk(this);

            if (node.ElseStatement != null) {
                // Else is flown only after completion of Try with same bits
                node.ElseStatement.Walk(this);
            }


            if (node.Handlers != null) {
                foreach (DynamicTryStatementHandler tsh in node.Handlers) {
                    // Restore to saved state
                    _bits.SetAll(false);
                    _bits.Or(save);

                    // Flow the test
                    if (tsh.Test != null) {
                        tsh.Test.Walk(this);
                    }

                    // Define the target
                    if (tsh.Variable != null) {
                        Define(tsh.Variable);
                    }

                    // Flow the body
                    tsh.Body.Walk(this);
                }
            }

            _bits = save;

            if (node.FinallyStatement != null) {
                // Flow finally - this executes no matter what
                node.FinallyStatement.Walk(this);
            }

            return false;
        }

        // LoopStatement
        public override bool Walk(LoopStatement node) {
            // Expression is executed always at least once
            if (node.Test != null) {
                node.Test.Walk(this);
            }

            // Beyond this point, either body will be executed (test succeeded),
            // or else is getting executed (test failed). There is no guarantee
            // that both will be executed, though.
            BitArray opte = new BitArray(_bits);
            BitArray exit = new BitArray(_bits.Length, true);

            PushLoop(exit);
            node.Body.Walk(this);
            if (node.Increment != null) {
                node.Increment.Walk(this);
            }
            PopLoop();

            _bits.And(exit);

            if (node.ElseStatement != null) {
                // Flow the else
                BitArray save = _bits;
                _bits = opte;
                node.ElseStatement.Walk(this);
                // Restore the bits
                _bits = save;
            }

            // Intersect
            _bits.And(opte);

            return false;
        }

        #endregion

        [Conditional("DEBUG")]
        public void Dump(BitArray bits) {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendFormat("FlowChecker ({0})", _block is CodeBlock ? ((CodeBlock)_block).Name : "");
            sb.Append('{');
            bool comma = false;

            foreach (KeyValuePair<Variable, int> kv in _indices) {
                Variable variable = kv.Key;
                int index = kv.Value * 2;

                if (comma) sb.Append(", ");
                else comma = true;
                sb.AppendFormat("{0}:{1}{2}",
                    SymbolTable.IdToString(variable.Name),
                    bits.Get(index) ? "*" : "-",
                    bits.Get(index + 1) ? "-" : "*");
                if (variable.Unassigned)
                    sb.Append("#");
            }
            sb.Append('}');
            Debug.WriteLine(sb.ToString());
        }
    }
}
