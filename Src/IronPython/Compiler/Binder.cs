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
using System.Diagnostics;
using System.Collections.Generic;

/*
 * The name binding:
 *
 * The name binding happens in 2 passes.
 * In the first pass (full recursive walk of the AST) we resolve locals.
 * The second pass uses the "processed" list of all scope statements (functions and class
 * bodies) and has each scope statement resolve its free variables to determine whether
 * they are globals or references to lexically enclosing scopes.
 *
 * The second pass happens in post-order (the scope statement is added into the "processed"
 * list after processing its nested functions/statements). This way, when the function is
 * processing its free variables, it also knows already which of its locals are being lifted
 * to the closure and can report error if such closure variable is being deleted.
 *
 * This is illegal in Python:
 *
 * def f():
 *     x = 10
 *     if (cond): del x        # illegal because x is a closure variable
 *     def g():
 *         print x
 */

namespace IronPython.Compiler {
    public class DefineBinder : AstWalkerNonRecursive {
        protected Binder binder;
        public DefineBinder(Binder binder) {
            this.binder = binder;
        }

        public override bool Walk(NameExpr node) {
            binder.Define(node.name);
            return false;
        }

        public override bool Walk(ParenExpr node) {
            return true;
        }

        public override bool Walk(TupleExpr node) {
            return true;
        }
    }

    public class ParameterBinder : DefineBinder {
        public ParameterBinder(Binder binder)
            : base(binder) {
        }

        public override bool Walk(NameExpr node) {
            binder.DefineParameter(node.name);
            return false;
        }
    }

    public class DeleteBinder : AstWalkerNonRecursive {
        private Binder binder;
        public DeleteBinder(Binder binder) {
            this.binder = binder;
        }

        public override bool Walk(NameExpr node) {
            binder.Deleted(node.name);
            return false;
        }
    }

    public class Binder : AstWalker {
        private ScopeStatement current;
        private List<ScopeStatement> processed = new List<ScopeStatement>();

        #region Recursive binders

        DefineBinder define;
        DeleteBinder delete;
        ParameterBinder parameter;

        #endregion

        private readonly CompilerContext context;

        private Binder(CompilerContext context) {
            this.define = new DefineBinder(this);
            this.delete = new DeleteBinder(this);
            this.parameter = new ParameterBinder(this);
            this.context = context;
        }

        public static GlobalSuite Bind(Stmt root, CompilerContext context) {
            Binder binder = new Binder(context);
            return binder.DoBind(root);
        }

        private GlobalSuite DoBind(Stmt root) {
            GlobalSuite global = new GlobalSuite(root);
            current = global;

            // Detect all local names
            root.Walk(this);

            // Binding the free variables
            foreach (ScopeStatement scope in processed) {
                scope.BindNames(global, this);
            }

            // Validate
            foreach (ScopeStatement scope in processed) {
                if ((scope.ScopeInfo &
                    (ScopeStatement.ScopeFlags.ContainsFreeVariables |
                    ScopeStatement.ScopeFlags.ContainsImportStar |
                    ScopeStatement.ScopeFlags.ContainsUnqualifiedExec |
                    ScopeStatement.ScopeFlags.ContainsNestedFreeVariables)) == 0) {
                    continue;
                }

                FuncDef func;
                if ((func = scope as FuncDef) != null) {
                    if (func.ContainsImportStar && func.IsClosure) {
                        ReportSyntaxError(String.Format("import * is not allowed in function '{0}' because it is a nested function", func.name.GetString()), func);
                    }
                    if (func.ContainsImportStar && func.parent is FuncDef) {
                        ReportSyntaxError(String.Format("import * is not allowed in function '{0}' because it is a nested function", func.name.GetString()), func);
                    }
                    if (func.ContainsImportStar && func.ContainsNestedFreeVariables) {
                        ReportSyntaxError(String.Format("import * is not allowed in function '{0}' because it contains a nested function with free variables", func.name.GetString()), func);
                    }
                    if (func.ContainsUnqualifiedExec && func.ContainsNestedFreeVariables) {
                        ReportSyntaxError(String.Format("unqualified exec is not allowed in function '{0}' it contains a nested function with free variables", func.name.GetString()), func);
                    }
                }

                ClassDef cls;
                if ((cls = scope as ClassDef) != null) {
                    if (cls.ContainsImportStar) {
                        // warning
                    }

                }
            }
            return global;
        }

        private void ReportSyntaxWarning(string message, Node node) {
            ReportSyntaxError(message, node, IronPython.Hosting.Severity.Warning);
        }

        public void ReportSyntaxError(string message, Node node) {
            ReportSyntaxError(message, node, IronPython.Hosting.Severity.Error);
        }

        public void ReportSyntaxError(string message, Node node, IronPython.Hosting.Severity serverity) {
            this.context.AddError(message, node, serverity);
        }

        #region AstBinder Overrides

        // NameExpr
        public override bool Walk(NameExpr node) {
            Reference(node.name);
            return true;
        }

        // AssignStmt
        public override bool Walk(AssignStmt node) {
            foreach (Expr e in node.lhs) {
                e.Walk(define);
            }
            return true;
        }

        // AugAssignStmt
        public override bool Walk(AugAssignStmt node) {
            node.lhs.Walk(define);
            return true;
        }

        // ClassDef
        public override bool Walk(ClassDef node) {
            Define(node.name);

            // Base references are in the outer scope
            foreach (Expr b in node.bases) b.Walk(this);

            // And so is the __name__ reference
            Reference(Name.Make("__name__"));

            node.parent = current;
            current = node;

            // define the __doc__ and the __module__
            Define(Name.Make("__doc__"));
            Define(Name.Make("__module__"));

            // Walk the body
            node.body.Walk(this);
            processed.Add(node);
            return false;
        }
        public override void PostWalk(ClassDef node) {
            Debug.Assert(node == current);
            current = current.parent;
        }

        // DelStmt
        public override bool Walk(DelStmt node) {
            foreach (Expr e in node.exprs) {
                e.Walk(delete);
            }
            return true;
        }

        // ExecStmt
        public override bool Walk(ExecStmt node) {
            if (node.locals == null) {
                Debug.Assert(current != null);
                current.ContainsUnqualifiedExec = true;
            }
            return true;
        }

        // ForStmt
        public override bool Walk(ForStmt node) {
            node.lhs.Walk(define);
            // Add locals
            Debug.Assert(current != null);
            current.tempsCount += node.LocalSlots;
            return true;
        }

        // FromImportStmt
        public override bool Walk(FromImportStmt node) {
            if (node.names != FromImportStmt.Star) {
                for (int i = 0; i < node.names.Length; i++) {
                    Define(node.asNames[i] != null ? node.asNames[i] : node.names[i]);
                }
            } else {
                Debug.Assert(current != null);
                current.ContainsImportStar = true;
            }
            return true;
        }

        // FuncDef
        public override bool Walk(FuncDef node) {
            // Name is defined in the enclosing scope
            Define(node.name);

            // process the default arg values in the outer scope
            foreach (Expr e in node.defaults) {
                e.Walk(this);
            }
            // process the decorators in the outer scope
            if (node.decorators != null) {
                node.decorators.Walk(this);
            }

            node.parent = current;
            current = node;
            foreach (Expr e in node.parameters) {
                e.Walk(parameter);
            }

            node.body.Walk(this);
            processed.Add(node);

            return false;
        }
        public override void PostWalk(FuncDef node) {
            Debug.Assert(current == node);
            current = current.parent;
        }

        // GlobalStmt
        public override bool Walk(GlobalStmt node) {
            foreach (Name n in node.names) {
                if (current != null) {
                    ScopeStatement.Binding binding;
                    if (current.Bindings.TryGetValue(n, out binding)) {
                        if (binding.IsParameter) {
                            ReportSyntaxError(String.Format("name '{0}' is a function parameter and declared global", n.GetString()), node);
                        } else if (binding.IsAssigned) {
                            ReportSyntaxWarning(String.Format("Variable {0} assigned before global declaration", n.GetString()), node);
                        } else if (binding.IsFree) {
                            ReportSyntaxWarning(String.Format("Variable {0} used before global declaration", n.GetString()), node);
                        }
                    }
                    current.BindGlobal(n);
                }
            }
            return true;
        }

        // GlobalSuite
        public override void PostWalk(GlobalSuite node) {
            current = current.parent;
        }

        // ImportStmt
        public override bool Walk(ImportStmt node) {
            for (int i = 0; i < node.names.Length; i++) {
                Define(node.asNames[i] != null ? node.asNames[i] : node.names[i].names[0]);
            }
            return true;
        }

        // TryStmt
        public override bool Walk(TryStmt node) {
            foreach (TryStmtHandler tsh in node.handlers) {
                if (tsh.target != null) {
                    tsh.target.Walk(define);
                }
            }
            return true;
        }

        // DottedName
        public override bool Walk(DottedName node) {
            Reference(node.names[0]);
            return true;
        }

        // ListCompFor
        public override bool Walk(ListCompFor node) {
            node.lhs.Walk(define);
            return true;
        }

        #endregion

        public void Define(Name name) {
            Debug.Assert(current != null);
            current.Bind(name);
        }

        public void DefineParameter(Name name) {
            Debug.Assert(current != null);
            current.BindParameter(name);
        }

        public void Reference(Name name) {
            Debug.Assert(current != null);
            current.Reference(name);
        }

        public void Deleted(Name name) {
            Debug.Assert(current != null);
            current.BindDeleted(name);
        }
    }
}
