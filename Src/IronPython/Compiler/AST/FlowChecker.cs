/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Shared Source License
 * for IronPython. A copy of the license can be found in the License.html file
 * at the root of this distribution. If you can not locate the Shared Source License
 * for IronPython, please send an email to ironpy@microsoft.com.
 * By using this source code in any fashion, you are agreeing to be bound by
 * the terms of the Shared Source License for IronPython.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using IronPython.Runtime;

/*
 * The data flow.
 * 
 * Each local name is represented as 2 bits:
 * One is for definitive assignment, the other is for uninitialized use detection.
 * The only difference between the two is behavior on delete.
 * On delete, the name is not assigned to meaningful value (we need to emit Ops.CheckInitialized() call),
 * but it is not uninitialized either (because delete statement will set it to Uninitialized).
 * This way, codegen doesn’t have to emit an explicit initialization for it.
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
 *     object a = new Uninitialized("a"); // explicit initialization because of the uninitialized use
 *     // print statement
 *     Ops.CheckUninitialized(a)
 *     Ops.Print(a);
 *     // a = 10
 *     a = 10
 * }
 * 
 * Whereas:
 * 
 * def f():
 *     a = 10
 *     del a        # explicit deletion which will set to Uninitialized
 *     print a
 * 
 * compiles into:
 * 
 * static void f$f0() {
 *     object a = 10                // a = 10
 *     a = new Uninitialized("a");  // del a
 *     Ops.CheckUninitialized(a)    // print a
 *     Ops.Print(a);                // also print a
 * }
 * 
 * The bit arrays in the flow checker hold the state and upon encountering NameExpr we figure
 * out whether the name has not yet been initialized at all (in which case we need to emit the
 * first explicit assignment to Uninitialized("name") and guard the use with CheckUninitialized()
 * or whether it is definitely assigned (we don't need to emit the CheckUninitialized() guard)
 * or whether it may be uninitialized, in which case we must only guard the use by CheckUninitialized()
 * 
 * More details on the bits.
 * 
 * First bit:
 *  1 .. value is definitely assigned (initialized and != Uninitialized())
 *  0 .. value may be uninitialized or set to Uninitialized()
 * Second bit:
 *  1 .. is definitely initialized  (including definitely initialized to Uninitialized() instance)
 *  0 .. may be uninitialized
 * 
 * In combination:
 *  11 .. initialized
 *  10 .. invalid
 *  01 .. deleted
 *  00 .. may be uninitialized
 */

namespace IronPython.Compiler.AST {
    class FlowDefiner : AstWalkerNonRecursive {
        private FlowChecker fc;
        public FlowDefiner(FlowChecker fc) {
            this.fc = fc;
        }

        public override bool Walk(NameExpr node) {
            fc.Define(node.Name);
            return false;
        }

        public override bool Walk(ParenExpr node) {
            return true;
        }

        public override bool Walk(TupleExpr node) {
            return true;
        }
    }

    class FlowDeleter : AstWalkerNonRecursive {
        private FlowChecker fc;
        public FlowDeleter(FlowChecker fc) {
            this.fc = fc;
        }

        public override bool Walk(NameExpr node) {
            fc.Delete(node.Name);
            return false;
        }
    }

    class FlowChecker : AstWalker {
        private BitArray bits;
        private Stack<BitArray> loops;
        private Dictionary<SymbolId, ScopeStatement.Binding> bindings;

        ScopeStatement scope;
        FlowDefiner fdef;
        FlowDeleter fdel;

        private FlowChecker(ScopeStatement scope) {
            bindings = scope.Bindings;
            bits = new BitArray(bindings.Count * 2);
            int index = 0;
            foreach (KeyValuePair<SymbolId, ScopeStatement.Binding> binding in bindings) {
                binding.Value.Index = index++;
            }
            this.scope = scope;
            this.fdef = new FlowDefiner(this);
            this.fdel = new FlowDeleter(this);
        }

        [Conditional("DEBUG")]
        public void Dump(BitArray bits) {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendFormat("FlowChecker ({0})", scope is FuncDef ? ((FuncDef)scope).Name.GetString() :
                                                 scope is ClassDef ? ((ClassDef)scope).Name.GetString() : "");
            sb.Append('{');
            bool comma = false;
            foreach (KeyValuePair<SymbolId, ScopeStatement.Binding> binding in bindings) {
                if (comma) sb.Append(", ");
                else comma = true;
                int index = 2 * binding.Value.Index;
                sb.AppendFormat("{0}:{1}{2}",
                    binding.Key.GetString(),
                    bits.Get(index) ? "*" : "-",
                    bits.Get(index + 1) ? "-" : "*");
                if (binding.Value.Unassigned)
                    sb.Append("#");
            }
            sb.Append('}');
            Debug.Print(sb.ToString());
        }

        public static void Check(ScopeStatement scope) {
            FlowChecker fc = new FlowChecker(scope);
            scope.Walk(fc);
        }

        public void Define(SymbolId name) {
            ScopeStatement.Binding binding;
            if (bindings.TryGetValue(name, out binding)) {
                int index = binding.Index * 2;
                bits.Set(index, true);      // is assigned
                bits.Set(index + 1, true);  // cannot be unassigned
            }
        }

        public void Delete(SymbolId name) {
            ScopeStatement.Binding binding;
            if (bindings.TryGetValue(name, out binding)) {
                int index = binding.Index * 2;
                bits.Set(index, false);     // is not initialized
                bits.Set(index + 1, true);  // is assigned (to Uninitialized())
            }
        }

        private void PushLoop(BitArray ba) {
            if (loops == null) {
                loops = new Stack<BitArray>();
            }
            loops.Push(ba);
        }

        private BitArray PeekLoop() {
            return loops != null ? loops.Peek() : null;
        }

        private void PopLoop() {
            if (loops != null) loops.Pop();
        }

        #region AstWalker Methods

        // LambdaExpr
        public override bool Walk(LambdaExpr node) { return false; }

        // ListComp
        public override bool Walk(ListComp node) {
            BitArray save = bits;
            bits = new BitArray(bits);

            foreach (ListCompIter iter in node.Iterators) iter.Walk(this);
            node.Item.Walk(this);

            bits = save;
            return false;
        }

        // NameExpr
        public override bool Walk(NameExpr node) {
            ScopeStatement.Binding binding;
            if (bindings.TryGetValue(node.Name, out binding)) {
                int index = binding.Index * 2;
                node.IsDefined = bits.Get(index);
                if (!node.IsDefined) {
                    binding.UninitializedUse();
                }
                if (!bits.Get(index + 1)) {
                    // Found an unbound use of the name => need to initialize to Uninitialized
                    binding.UnassignedUse();
                }
            }
            return true;
        }
        public override void PostWalk(NameExpr node) { }

        // AssignStmt
        public override bool Walk(AssignStmt node) {
            node.Right.Walk(this);
            foreach (Expr e in node.Left) {
                e.Walk(fdef);
            }
            return false;
        }
        public override void PostWalk(AssignStmt node) { }

        // AugAssignStmt
        public override bool Walk(AugAssignStmt node) { return true; }
        public override void PostWalk(AugAssignStmt node) {
            node.Left.Walk(fdef);
        }

        // BreakStmt
        public override bool Walk(BreakStmt node) {
            BitArray exit = PeekLoop();
            if (exit != null) { // break outside loop
                exit.And(bits);
            }
            return true;
        }

        // ClassDef
        public override bool Walk(ClassDef node) {
            if (scope == node) {
                return true;
            } else {
                Define(node.Name);
                return false;
            }
        }

        // ContinueStmt
        public override bool Walk(ContinueStmt node) { return true; }

        // DelStmt
        public override void PostWalk(DelStmt node) {
            foreach (Expr e in node.Expressions) {
                e.Walk(fdel);
            }
        }

        // ForStmt
        public override bool Walk(ForStmt node) {
            // Walk the expression
            node.List.Walk(this);

            BitArray opte = new BitArray(bits);
            BitArray exit = new BitArray(bits.Length, true);
            PushLoop(exit);

            // Define the lhs
            node.Left.Walk(fdef);
            // Walk the body
            node.Body.Walk(this);

            PopLoop();

            bits.And(exit);

            if (node.ElseStatement != null) {
                // Flow the else
                BitArray save = bits;
                bits = opte;
                node.ElseStatement.Walk(this);
                // Restore the bits
                bits = save;
            }

            // Intersect
            bits.And(opte);

            return false;
        }

        // FromImportStmt
        public override bool Walk(FromImportStmt node) {
            if (node.Names != FromImportStmt.Star) {
                for (int i = 0; i < node.Names.Count; i++) {
                    Define(node.AsNames[i] != SymbolTable.Empty ? node.AsNames[i] : node.Names[i]);
                }
            }
            return true;
        }

        // FuncDef
        public override bool Walk(FuncDef node) {
            if (node == scope) {
                foreach (Expr e in node.Parameters) {
                    e.Walk(fdef);
                }
                return true;
            } else {
                Define(node.Name);
                return false;
            }
        }

        // IfStmt
        public override bool Walk(IfStmt node) {
            BitArray result = new BitArray(bits.Length, true);
            BitArray save = bits;

            bits = new BitArray(bits.Length);

            foreach (IfStmtTest ist in node.Tests) {
                // Set the initial branch value to bits
                bits.SetAll(false);
                bits.Or(save);

                // Flow the test first
                ist.Test.Walk(this);
                // Flow the body
                ist.Body.Walk(this);
                // Intersect
                result.And(bits);
            }

            // Set the initial branch value to bits
            bits.SetAll(false);
            bits.Or(save);

            if (node.ElseStatement != null) {
                // Flow the else_
                node.ElseStatement.Walk(this);
            }

            // Intersect
            result.And(bits);

            bits = save;

            // Remember the result
            bits.SetAll(false);
            bits.Or(result);
            return false;
        }

        // ImportStmt
        public override bool Walk(ImportStmt node) {
            for (int i = 0; i < node.Names.Count; i++) {
                Define(node.AsNames[i] != SymbolTable.Empty ? node.AsNames[i] : node.Names[i].Names[0]);
            }
            return true;
        }

        public override void PostWalk(ReturnStmt node) { }

        // TryFinallyStmt
        public override bool Walk(TryFinallyStmt node) {
            BitArray save = bits;
            bits = new BitArray(bits);
            // Flow the body
            node.Body.Walk(this);
            bits = save;
            // Flow finally - this executes no matter what
            node.FinallyStmt.Walk(this);
            return false;
        }

        // TryStmt
        public override bool Walk(TryStmt node) {
            BitArray result = new BitArray(bits.Length, true);
            BitArray save = bits;
            bits = new BitArray(bits);

            // Flow the body
            node.Body.Walk(this);
            result.And(bits);

            foreach (TryStmtHandler tsh in node.Handlers) {
                // Restore to saved state
                bits.SetAll(false);
                bits.Or(save);

                // Flow the test
                if (tsh.Test != null) {
                    tsh.Test.Walk(this);
                }

                // Define the target
                if (tsh.Target != null) {
                    tsh.Target.Walk(fdef);
                }

                // Flow the body
                tsh.Body.Walk(this);
                result.And(bits);
            }

            if (node.ElseStatement != null) {
                bits.SetAll(false);
                bits.Or(save);

                node.ElseStatement.Walk(this);
                result.And(bits);
            }

            bits = save;
            bits.SetAll(false);
            bits.Or(result);
            return false;
        }

        // WhileStmt
        public override bool Walk(WhileStmt node) {
            // Walk the expression
            node.Test.Walk(this);

            BitArray opte = node.ElseStatement != null ? new BitArray(bits) : null;
            BitArray exit = new BitArray(bits.Length, true);

            PushLoop(exit);
            node.Body.Walk(this);
            PopLoop();

            bits.And(exit);

            if (node.ElseStatement != null) {
                // Flow the else
                BitArray save = bits;
                bits = opte;
                node.ElseStatement.Walk(this);
                // Restore the bits
                bits = save;

                // Intersect
                bits.And(opte);
            }

            return false;
        }

        #endregion
    }
}
