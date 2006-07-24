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
using IronPython.Runtime;

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

namespace IronPython.Compiler.Ast {
    class DefineBinder : AstWalkerNonRecursive {
        protected Binder binder;
        public DefineBinder(Binder binder) {
            this.binder = binder;
        }

        public override bool Walk(NameExpression node) {
            binder.Define(node.Name);
            return false;
        }

        public override bool Walk(ParenthesisExpression node) {
            return true;
        }

        public override bool Walk(TupleExpression node) {
            return true;
        }

        public override bool Walk(ListExpression node) {
            return true;
        }
    }

    class ParameterBinder : AstWalkerNonRecursive {
        private Binder binder;
        public ParameterBinder(Binder binder) {
            this.binder = binder;
        }

        public override bool Walk(NameExpression node) {
            binder.DefineParameter(node.Name);
            return false;
        }

        public override bool Walk(ParenthesisExpression node) {
            return true;
        }

        public override bool Walk(TupleExpression node) {
            return true;
        }
    }

    class DeleteBinder : AstWalkerNonRecursive {
        private Binder binder;
        public DeleteBinder(Binder binder) {
            this.binder = binder;
        }

        public override bool Walk(NameExpression node) {
            binder.Deleted(node.Name);
            return false;
        }
    }

    class Binder : AstWalker {
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

        public static GlobalSuite Bind(Statement root, CompilerContext context) {
            Binder binder = new Binder(context);
            return binder.DoBind(root);
        }

        private GlobalSuite DoBind(Statement root) {
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
                    (ScopeStatement.ScopeAttributes.ContainsFreeVariables |
                    ScopeStatement.ScopeAttributes.ContainsImportStar |
                    ScopeStatement.ScopeAttributes.ContainsUnqualifiedExec |
                    ScopeStatement.ScopeAttributes.ContainsNestedFreeVariables)) == 0) {
                    continue;
                }

                FunctionDefinition func;
                if ((func = scope as FunctionDefinition) != null) {
                    if (func.ContainsImportStar && func.IsClosure) {
                        ReportSyntaxError(String.Format("import * is not allowed in function '{0}' because it is a nested function", func.Name.GetString()), func);
                    }
                    if (func.ContainsImportStar && func.Parent is FunctionDefinition) {
                        ReportSyntaxError(String.Format("import * is not allowed in function '{0}' because it is a nested function", func.Name.GetString()), func);
                    }
                    if (func.ContainsImportStar && func.ContainsNestedFreeVariables) {
                        ReportSyntaxError(String.Format("import * is not allowed in function '{0}' because it contains a nested function with free variables", func.Name.GetString()), func);
                    }
                    if (func.ContainsUnqualifiedExec && func.ContainsNestedFreeVariables) {
                        ReportSyntaxError(String.Format("unqualified exec is not allowed in function '{0}' it contains a nested function with free variables", func.Name.GetString()), func);
                    }
                    if (func.ContainsUnqualifiedExec && func.IsClosure) {
                        ReportSyntaxError(String.Format("unqualified exec is not allowed in function '{0}' it is a nested function", func.Name.GetString()), func);
                    }
                }

                ClassDefinition cls;
                if ((cls = scope as ClassDefinition) != null) {
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
        public override bool Walk(NameExpression node) {
            Reference(node.Name);
            return true;
        }

        // AssignStmt
        public override bool Walk(AssignStatement node) {
            foreach (Expression e in node.Left) {
                e.Walk(define);
            }
            return true;
        }

        // AugAssignStmt
        public override bool Walk(AugAssignStatement node) {
            node.Left.Walk(define);
            return true;
        }

        // ClassDef
        public override bool Walk(ClassDefinition node) {
            Define(node.Name);

            // Base references are in the outer scope
            foreach (Expression b in node.Bases) b.Walk(this);

            // And so is the __name__ reference
            Reference(SymbolTable.Name);

            node.Parent = current;
            current = node;

            // define the __doc__ and the __module__
            Define(SymbolTable.Doc);
            Define(SymbolTable.Module);

            // Walk the body
            node.Body.Walk(this);
            processed.Add(node);
            return false;
        }
        public override void PostWalk(ClassDefinition node) {
            Debug.Assert(node == current);
            current = current.Parent;
        }

        // DelStmt
        public override bool Walk(DelStatement node) {
            foreach (Expression e in node.Expressions) {
                e.Walk(delete);
            }
            return true;
        }

        // ExecStmt
        public override bool Walk(ExecStatement node) {
            if (node.Locals == null && node.Globals == null) {
                Debug.Assert(current != null);
                current.ContainsUnqualifiedExec = true;
            }
            return true;
        }

        // ForStmt
        public override bool Walk(ForStatement node) {
            node.Left.Walk(define);
            // Add locals
            Debug.Assert(current != null);
            current.TempsCount += ForStatement.LocalSlots;
            return true;
        }

        // WithStmt
        public override bool Walk(WithStatement node) {
            if (node.Variable != null) {
                node.Variable.Walk(define);
            }

            // Add locals
            Debug.Assert(current != null);
            current.TempsCount += WithStatement.LocalSlots;
            return true;
        }

        // FromImportStmt
        public override bool Walk(FromImportStatement node) {
            if (node.Names != FromImportStatement.Star) {
                for (int i = 0; i < node.Names.Count; i++) {
                    Define(node.AsNames[i] != SymbolTable.Empty ? node.AsNames[i] : node.Names[i]);
                }
            } else {
                Debug.Assert(current != null);
                current.ContainsImportStar = true;
            }
            return true;
        }

        // FuncDef
        public override bool Walk(FunctionDefinition node) {
            // Name is defined in the enclosing scope
            Define(node.Name);

            // process the default arg values in the outer scope
            foreach (Expression e in node.Defaults) {
                e.Walk(this);
            }
            // process the decorators in the outer scope
            if (node.Decorators != null) {
                node.Decorators.Walk(this);
            }

            node.Parent = current;
            current = node;
            foreach (Expression e in node.Parameters) {
                e.Walk(parameter);
            }

            node.Body.Walk(this);
            processed.Add(node);

            return false;
        }
        public override void PostWalk(FunctionDefinition node) {
            Debug.Assert(current == node);
            current = current.Parent;
        }

        // GlobalStmt
        public override bool Walk(GlobalStatement node) {
            foreach (SymbolId n in node.Names) {
                if (current != null) {
                    Binding binding;
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
            current = current.Parent;
        }

        // ImportStmt
        public override bool Walk(ImportStatement node) {
            for (int i = 0; i < node.Names.Count; i++) {
                Define(node.AsNames[i] != SymbolTable.Empty ? node.AsNames[i] : node.Names[i].Names[0]);
            }
            return true;
        }

        // TryStmt
        public override bool Walk(TryStatement node) {
            foreach (TryStatementHandler tsh in node.Handlers) {
                if (tsh.Target != null) {
                    tsh.Target.Walk(define);
                }
            }
            return true;
        }

        // DottedName
        public override bool Walk(DottedName node) {
            Reference(node.Names[0]);
            return true;
        }

        // ListCompFor
        public override bool Walk(ListComprehensionFor node) {
            node.Left.Walk(define);
            return true;
        }

        #endregion

        public void Define(SymbolId name) {
            Debug.Assert(current != null);
            current.Bind(name);
        }

        public void DefineParameter(SymbolId name) {
            Debug.Assert(current != null);
            current.BindParameter(name);
        }

        public void Reference(SymbolId name) {
            Debug.Assert(current != null);
            current.Reference(name);
        }

        public void Deleted(SymbolId name) {
            Debug.Assert(current != null);
            current.BindDeleted(name);
        }
    }
}
