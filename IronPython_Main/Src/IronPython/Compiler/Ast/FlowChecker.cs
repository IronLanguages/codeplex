/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public
 * License. A  copy of the license can be found in the License.html file at the
 * root of this distribution. If  you cannot locate the  Microsoft Public
 * License, please send an email to  dlr@microsoft.com. By using this source
 * code in any fashion, you are agreeing to be bound by the terms of the 
 * Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using IronPython.Runtime;
using Microsoft.Scripting;

/*
 * The data flow.
 * 
 * Each local name is represented as 2 bits:
 * One is for definitive assignment, the other is for uninitialized use detection.
 * The only difference between the two is behavior on delete.
 * On delete, the name is not assigned to meaningful value (we need to check at runtime if it's initialized),
 * but it is not uninitialized either (because delete statement will set it to Uninitialized.instance).
 * This way, codegen doesn�t have to emit an explicit initialization for it.
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

namespace IronPython.Compiler.Ast {
    class FlowDefiner : PythonWalkerNonRecursive {
        private readonly FlowChecker _fc;

        public FlowDefiner(FlowChecker fc) {
            _fc = fc;
        }

        public override bool Walk(NameExpression node) {
            _fc.Define(node.Name);
            return false;
        }

        public override bool Walk(ParenthesisExpression node) {
            return true;
        }

        public override bool Walk(TupleExpression node) {
            return true;
        }

        public override bool Walk(Parameter node) {
            _fc.Define(node.Name);
            return true;
        }

        public override bool Walk(SublistParameter node) {
            return true;
        }
    }

    class FlowDeleter : PythonWalkerNonRecursive {
        private readonly FlowChecker _fc;

        public FlowDeleter(FlowChecker fc) {
            _fc = fc;
        }

        public override bool Walk(NameExpression node) {
            _fc.Delete(node.Name);
            return false;
        }
    }

    class FlowChecker : PythonWalker {
        private BitArray _bits;
        private Stack<BitArray> _loops;
        private Dictionary<SymbolId, PythonVariable> _variables;

        private readonly ScopeStatement _scope;
        private readonly FlowDefiner _fdef;
        private readonly FlowDeleter _fdel;

        private FlowChecker(ScopeStatement scope) {
            _variables = scope.Variables;
            _bits = new BitArray(_variables.Count * 2);
            int index = 0;
            foreach (KeyValuePair<SymbolId, PythonVariable> binding in _variables) {
                binding.Value.Index = index++;
            }
            _scope = scope;
            _fdef = new FlowDefiner(this);
            _fdel = new FlowDeleter(this);
        }

        [Conditional("DEBUG")]
        public void Dump(BitArray bits) {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendFormat("FlowChecker ({0})", _scope is FunctionDefinition ? SymbolTable.IdToString(((FunctionDefinition)_scope).Name) :
                                                 _scope is ClassDefinition ? SymbolTable.IdToString(((ClassDefinition)_scope).Name) : "");
            sb.Append('{');
            bool comma = false;
            foreach (KeyValuePair<SymbolId, PythonVariable> binding in _variables) {
                if (comma) sb.Append(", ");
                else comma = true;
                int index = 2 * binding.Value.Index;
                sb.AppendFormat("{0}:{1}{2}",
                    SymbolTable.IdToString(binding.Key),
                    bits.Get(index) ? "*" : "-",
                    bits.Get(index + 1) ? "-" : "*");
                if (binding.Value.Unassigned)
                    sb.Append("#");
            }
            sb.Append('}');
            Debug.WriteLine(sb.ToString());
        }

        public static void Check(ScopeStatement scope) {
            if (scope.Variables != null) {
                FlowChecker fc = new FlowChecker(scope);
                scope.Walk(fc);
            }
        }

        public void Define(SymbolId name) {
            PythonVariable binding;
            if (_variables.TryGetValue(name, out binding)) {
                int index = binding.Index * 2;
                _bits.Set(index, true);      // is assigned
                _bits.Set(index + 1, true);  // cannot be unassigned
            }
        }

        public void Delete(SymbolId name) {
            PythonVariable binding;
            if (_variables.TryGetValue(name, out binding)) {
                int index = binding.Index * 2;
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

        // LambdaExpr
        public override bool Walk(LambdaExpression node) { return false; }

        // ListComp
        public override bool Walk(ListComprehension node) {
            BitArray save = _bits;
            _bits = new BitArray(_bits);

            foreach (ListComprehensionIterator iter in node.Iterators) iter.Walk(this);
            node.Item.Walk(this);

            _bits = save;
            return false;
        }

        // NameExpr
        public override bool Walk(NameExpression node) {
            PythonVariable binding;
            if (_variables.TryGetValue(node.Name, out binding)) {
                int index = binding.Index * 2;
                bool defined = _bits.Get(index);
                node.Assigned = defined;
                if (!defined) {
                    binding.Uninitialized = true;
                }
                if (!_bits.Get(index + 1)) {
                    // Found an unbound use of the name => need to initialize to Uninitialized.instance
                    binding.Unassigned = true;
                }
            }
            return true;
        }
        public override void PostWalk(NameExpression node) { }

        // AssignStmt
        public override bool Walk(AssignmentStatement node) {
            node.Right.Walk(this);
            foreach (Expression e in node.Left) {
                e.Walk(_fdef);
            }
            return false;
        }
        public override void PostWalk(AssignmentStatement node) { }

        // AugAssignStmt
        public override bool Walk(AugmentedAssignStatement node) { return true; }
        public override void PostWalk(AugmentedAssignStatement node) {
            node.Left.Walk(_fdef);
        }

        // BreakStmt
        public override bool Walk(BreakStatement node) {
            BitArray exit = PeekLoop();
            if (exit != null) { // break outside loop
                exit.And(_bits);
            }
            return true;
        }

        // ClassDef
        public override bool Walk(ClassDefinition node) {
            if (_scope == node) {
                return true;
            } else {
                Define(node.Name);
                return false;
            }
        }

        // ContinueStmt
        public override bool Walk(ContinueStatement node) { return true; }

        // DelStmt
        public override void PostWalk(DelStatement node) {
            foreach (Expression e in node.Expressions) {
                e.Walk(_fdel);
            }
        }

        // ForStmt
        public override bool Walk(ForStatement node) {
            // Walk the expression
            node.List.Walk(this);

            BitArray opte = new BitArray(_bits);
            BitArray exit = new BitArray(_bits.Length, true);
            PushLoop(exit);

            // Define the lhs
            node.Left.Walk(_fdef);
            // Walk the body
            node.Body.Walk(this);

            PopLoop();

            _bits.And(exit);

            if (node.Else != null) {
                // Flow the else
                BitArray save = _bits;
                _bits = opte;
                node.Else.Walk(this);
                // Restore the bits
                _bits = save;
            }

            // Intersect
            _bits.And(opte);

            return false;
        }

        // FromImportStmt
        public override bool Walk(FromImportStatement node) {
            if (node.Names != FromImportStatement.Star) {
                for (int i = 0; i < node.Names.Count; i++) {
                    Define(node.AsNames[i] != SymbolId.Empty ? node.AsNames[i] : node.Names[i]);
                }
            }
            return true;
        }

        // FuncDef
        public override bool Walk(FunctionDefinition node) {
            if (node == _scope) {
                foreach (Parameter e in node.Parameters) {
                    e.Walk(_fdef);
                }
                return true;
            } else {
                Define(node.Name);
                return false;
            }
        }

        // IfStmt
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

        // ImportStmt
        public override bool Walk(ImportStatement node) {
            for (int i = 0; i < node.Names.Count; i++) {
                Define(node.AsNames[i] != SymbolId.Empty ? node.AsNames[i] : node.Names[i].Names[0]);
            }
            return true;
        }

        public override void PostWalk(ReturnStatement node) { }

        // WithStmt
        public override bool Walk(WithStatement node) {
            // Walk the expression
            node.ContextManager.Walk(this);
            BitArray save = _bits;
            _bits = new BitArray(_bits);

            // Define the Rhs
            if (node.Variable != null)
                node.Variable.Walk(_fdef);

            // Flow the body
            node.Body.Walk(this);

            _bits = save;
            return false;
        }

        // TryStmt
        public override bool Walk(TryStatement node) {
            BitArray save = _bits;
            _bits = new BitArray(_bits);

            // Flow the body
            node.Body.Walk(this);

            if (node.Else != null) {
                // Else is flown only after completion of Try with same bits
                node.Else.Walk(this);
            }


            if (node.Handlers != null) {
                foreach (TryStatementHandler tsh in node.Handlers) {
                    // Restore to saved state
                    _bits.SetAll(false);
                    _bits.Or(save);

                    // Flow the test
                    if (tsh.Test != null) {
                        tsh.Test.Walk(this);
                    }

                    // Define the target
                    if (tsh.Target != null) {
                        tsh.Target.Walk(_fdef);
                    }

                    // Flow the body
                    tsh.Body.Walk(this);
                }
            }

            _bits = save;

            if (node.Finally != null) {
                // Flow finally - this executes no matter what
                node.Finally.Walk(this);
            }

            return false;
        }

        // WhileStmt
        public override bool Walk(WhileStatement node) {
            // Walk the expression
            node.Test.Walk(this);

            BitArray opte = node.ElseStatement != null ? new BitArray(_bits) : null;
            BitArray exit = new BitArray(_bits.Length, true);

            PushLoop(exit);
            node.Body.Walk(this);
            PopLoop();

            _bits.And(exit);

            if (node.ElseStatement != null) {
                // Flow the else
                BitArray save = _bits;
                _bits = opte;
                node.ElseStatement.Walk(this);
                // Restore the bits
                _bits = save;

                // Intersect
                _bits.And(opte);
            }

            return false;
        }

        #endregion
    }
}
