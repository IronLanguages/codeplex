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
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Scripting;

using IronPython.Runtime;
using MSAst = Microsoft.Scripting.Internal.Ast;
using Microsoft.Scripting.Internal.Generation;

using VariableKind = Microsoft.Scripting.Internal.Ast.Variable.VariableKind;

/*
 * The name binding:
 *
 * The name binding happens in 2 passes.
 * In the first pass (full recursive walk of the AST) we resolve locals.
 * The second pass uses the "processed" list of all context statements (functions and class
 * bodies) and has each context statement resolve its free variables to determine whether
 * they are globals or references to lexically enclosing scopes.
 *
 * The second pass happens in post-order (the context statement is added into the "processed"
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
    class DefineBinder : PythonWalkerNonRecursive {
        private PythonNameBinder _binder;
        public DefineBinder(PythonNameBinder binder) {
            _binder = binder;
        }
        public override bool Walk(NameExpression node) {
            _binder.DefineName(node.Name);
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

    class ParameterBinder : PythonWalkerNonRecursive {
        private PythonNameBinder _binder;
        public ParameterBinder(PythonNameBinder binder) {
            _binder = binder;
        }
        public override bool Walk(NameExpression node) {
            // Called for the sublist parameters. The elements of the tuple become regular
            // local variables, therefore don't make the parameters (DefineParameter), but
            // regular locals (DefineName)
            _binder.DefineName(node.Name);
            node.Reference = _binder.Reference(node.Name);
            return false;
        }
        public override bool Walk(Parameter node) {
            _binder.DefineParameter(node.Name);
            node.Reference = _binder.Reference(node.Name);
            return false;
        }
        public override bool Walk(SublistParameter node) {
            _binder.DefineParameter(node.Name);
            node.Reference = _binder.Reference(node.Name);
            return true;
        }
        public override bool Walk(TupleExpression node) {
            return true;
        }
    }

    class DeleteBinder : PythonWalkerNonRecursive {
        private PythonNameBinder _binder;
        public DeleteBinder(PythonNameBinder binder) {
            _binder = binder;
        }
        public override bool Walk(NameExpression node) {
            _binder.DefineDeleted(node.Name);
            return false;
        }
    }

    [Flags]
    enum PythonScopeFlags {
        ContainsImportStar = 0x01,              // from module import *
        ContainsUnqualifiedExec = 0x02,         // exec "code"
        ContainsExec = 0x04,                    // exec in any form, both qualified or unqualified
        ContainsNestedFreeVariables = 0x08,     // nested function with free variable
        ContainsFreeVariables = 0x10,           // the function itself contains free variables
        NeedsLocalsDictionary = 0x20,           // The context needs locals dictionary
        // due to "exec" or call to dir, locals, eval, vars...
        IsClosure = 0x40,                       // the context is a closure (references outer context's locals)
    }

    class PythonNameBinder : PythonWalker {
        private PythonAst _globalScope;
        private ScopeStatement _currentScope;
        private List<ScopeStatement> _processed = new List<ScopeStatement>();

        private List<ClassDefinition> _classes;

        #region Recursive binders

        private DefineBinder _define;
        private DeleteBinder _delete;
        private ParameterBinder _parameter;

        #endregion

        private readonly CompilerContext _context;

        private PythonNameBinder(CompilerContext context) {
            _define = new DefineBinder(this);
            _delete = new DeleteBinder(this);
            _parameter = new ParameterBinder(this);
            _context = context;
        }

        #region Public surface

        internal static PythonAst Bind(Statement root, CompilerContext context, bool module) {
            PythonNameBinder binder = new PythonNameBinder(context);
            return binder.DoBind(root, module);
        }

        #endregion

        // TODO: The boolean flag is temporary until we have ModuleStatement or something of sorts
        private PythonAst DoBind(Statement root, bool module) {
            PythonAst gs = new PythonAst(root, module);
            _currentScope = _globalScope = gs;

            // Detect all local names
            gs.Walk(this);

            // Binding the free variables
            foreach (ScopeStatement scope in _processed) {
                BindNamesInScope(scope);
            }

            // Bind class members, creating name based lookup if necessary.
            BindClassMembers();

            // Bind and publish the global names.
            // This must be done after the class member binding because it can introduce new globals.
            BindNamesInScope(_globalScope);

            // Validate
            foreach (ScopeStatement scope in _processed) {
                if ((scope.Flags &
                    (PythonScopeFlags.ContainsFreeVariables |
                    PythonScopeFlags.ContainsImportStar |
                    PythonScopeFlags.ContainsUnqualifiedExec |
                    PythonScopeFlags.ContainsNestedFreeVariables)) == 0) {
                    continue;
                }

                FunctionDefinition func = scope as FunctionDefinition;
                if (func != null) {
                    if (scope.ContainsImportStar && scope.IsClosure) {
                        ReportSyntaxError(
                            String.Format(
                                System.Globalization.CultureInfo.InvariantCulture,
                                "import * is not allowed in function '{0}' because it is a nested function",
                                SymbolTable.IdToString(func.Name)),
                            func);
                    }
                    if (scope.ContainsImportStar && func.Parent is FunctionDefinition) {
                        ReportSyntaxError(
                            String.Format(
                                System.Globalization.CultureInfo.InvariantCulture,
                                "import * is not allowed in function '{0}' because it is a nested function",
                                SymbolTable.IdToString(func.Name)),
                            func);
                    }
                    if (scope.ContainsImportStar && scope.ContainsNestedFreeVariables) {
                        ReportSyntaxError(
                            String.Format(
                                System.Globalization.CultureInfo.InvariantCulture,
                                "import * is not allowed in function '{0}' because it contains a nested function with free variables",
                                SymbolTable.IdToString(func.Name)),
                            func);
                    }
                    if (scope.ContainsUnqualifiedExec && scope.ContainsNestedFreeVariables) {
                        ReportSyntaxError(
                            String.Format(
                                System.Globalization.CultureInfo.InvariantCulture,
                                "unqualified exec is not allowed in function '{0}' it contains a nested function with free variables",
                                SymbolTable.IdToString(func.Name)),
                            func);
                    }
                    if (scope.ContainsUnqualifiedExec && scope.IsClosure) {
                        ReportSyntaxError(
                            String.Format(
                                System.Globalization.CultureInfo.InvariantCulture,
                                "unqualified exec is not allowed in function '{0}' it is a nested function",
                                SymbolTable.IdToString(func.Name)),
                            func);
                    }
                }

                ClassDefinition cls = scope as ClassDefinition;
                if (cls != null) {
                    if (scope.ContainsImportStar) {
                        // warning
                    }
                }
            }

            return gs;
        }

        private void BindClassMembers() {
            // if there are no classes, bail
            if (_classes == null) {
                return;
            }

            // Determine when name based lookup is necessary for the class members.
            foreach (ClassDefinition c in _classes) {
                // No dataflow here, just go for it directly
                foreach (KeyValuePair<SymbolId, PythonReference> kv in c.References) {
                    PythonReference r = kv.Value;

                    if (r.Variable == null) {
                        // unbound variable is ok
                        continue;
                    }

                    if (r.Variable.Kind != VariableKind.Local ||
                        r.Variable.Scope != c) {
                        // only locals interest us, and since the closures haven't been
                        // resolved, the non-local references are still marked as Local
                        // (in the outer context) => check for locals in current context only
                        continue;
                    }

                    // Dissociate, lookup by name
                    r.Variable = null;
                }
            }
        }

        static bool TryBindInScope(ScopeStatement scope, SymbolId name, out PythonVariable variable) {
            // only names defined in the functions can be bound to,
            // names defined in the class can only be accessed via attribute access
            if (scope.IsFunctionScope) {
                return scope.TryGetDefinition(name, out variable);
            }
            variable = null;
            return false;
        }

        /// <summary>
        /// Binds names referenced in the context. Will look in the current and outer scopes to bind the name.
        /// </summary>
        /// <param name="context"></param>
        void BindNamesInScope(ScopeStatement scope) {
            if (scope.References != null) {
                foreach (KeyValuePair<SymbolId, PythonReference> kv in scope.References) {
                    PythonReference reference = kv.Value;

                    // Already bound
                    if (reference.Variable != null) {
                        continue;
                    }

                    SymbolId name = kv.Key;
                    PythonVariable variable;

                    // Try to bind in the current context
                    if (scope.TryGetDefinition(name, out variable)) {
                        reference.Variable = variable;
                        continue;
                    }

                    // Try to bind in outer context(s)
                    for (ScopeStatement parent = scope.Parent; parent != null; parent = parent.Parent) {
                        if (TryBindInScope(parent, name, out variable)) {
                            // Found it !
                            reference.Variable = variable;

                            // Cannot delete variables referenced from nested context
                            if (variable.Kind != VariableKind.Global) {
                                scope.IsClosure = true;

                                if ((variable.Flags & PythonVariable.PythonFlags.Deleted) != 0) {
                                    ReportSyntaxError(
                                        String.Format(
                                            System.Globalization.CultureInfo.InvariantCulture,
                                            "can not delete variable '{0}' referenced in nested context",
                                            SymbolTable.IdToString(name)
                                            ),
                                        scope);
                                }
                            }

                            // Cannot do break out of this loop and direct continue of the outer loop
                            // must resort to goto.
                            goto EndOfLoop;
                        }
                    }

                    // Variable unbound in the current (or outer) scopes - completely free
                    if (scope.IsGlobal) {
                        // At a global level, all unbound variables are locals
                        reference.Variable = DefineModuleLocal(name);
                    } else {
                        if (scope.ContainsUnqualifiedExec || scope.ContainsImportStar || scope.NeedsLocalsDictionary) {
                            // If the context contains unqualified exec, new locals can be introduced
                            // We introduce the locals for every free variable to optimize the name based
                            // lookup.
                            scope.DefineName(name);
                        } else {
                            // Create a global variable to bind to.
                            reference.Variable = EnsureGlobalVariableDefinition(name);
                        }
                    }

                EndOfLoop:
                    ;
                }
            }
        }

        private PythonVariable EnsureGlobalVariableDefinition(SymbolId name) {
            return _globalScope.EnsureGlobalVariable(name);
        }

        private void PushScope(ScopeStatement node) {
            node.Parent = _currentScope;
            _currentScope = node;
        }

        internal void DefineName(SymbolId name) {
            _currentScope.DefineName(name);
        }

        internal PythonVariable DefineModuleGlobal(SymbolId name) {
            PythonVariable variable = _globalScope.DefineName(name);
            variable.Kind = VariableKind.Global;
            return variable;
        }

        // Creates module level local variable. Even at module level,
        // there are globals and locals
        private PythonVariable DefineModuleLocal(SymbolId name) {
            return _globalScope.DefineModuleLocal(name);
        }

        internal void DefineParameter(SymbolId name) {
            _currentScope.DefineParameter(name);
        }

        internal void DefineDeleted(SymbolId name) {
            _currentScope.DefineDeleted(name);
        }

        internal void AddTemporaryReference(PythonReference reference) {
            _currentScope.AddTemporaryReference(reference);
        }

        internal PythonReference Reference(SymbolId name) {
            return _currentScope.EnsureReference(name);
        }

        private void ReportSyntaxWarning(string message, Node node) {
            ReportSyntaxError(message, node, Microsoft.Scripting.Hosting.Severity.Warning);
        }

        public void ReportSyntaxError(string message, Node node) {
            ReportSyntaxError(message, node, Microsoft.Scripting.Hosting.Severity.Error);
        }

        private void ReportSyntaxError(string message, Node node, Microsoft.Scripting.Hosting.Severity serverity) {
            // TODO: Change the error code (-1)
            _context.AddError(message, node.Start, node.End, serverity, -1);
        }

        private void StoreClass(ClassDefinition node) {
            if (_classes == null) {
                _classes = new List<ClassDefinition>();
            }
            _classes.Add(node);
        }

        #region AstBinder Overrides

        // NameExpression
        public override bool Walk(NameExpression node) {
            node.Reference = Reference(node.Name);
            return true;
        }

        // AssignmentStatement
        public override bool Walk(AssignmentStatement node) {
            foreach (Expression e in node.Left) {
                e.Walk(_define);
            }
            return true;
        }

        public override bool Walk(AugmentedAssignStatement node) {
            node.Left.Walk(_define);
            return true;
        }

        public override void PostWalk(CallExpression node) {
            if (node.NeedsLocalsDictionary()) {
                _currentScope.NeedsLocalsDictionary = true;
            }
        }

        // ClassDefinition
        public override bool Walk(ClassDefinition node) {
            DefineName(node.Name);
            node.Reference = Reference(node.Name);

            // Base references are in the outer context
            foreach (Expression b in node.Bases) b.Walk(this);

            PushScope(node);

            // get a reference to __name__ in globals
            node.NameReference = new PythonReference(Symbols.Name);
            node.NameReference.Variable = EnsureGlobalVariableDefinition(Symbols.Name);
            AddTemporaryReference(node.NameReference);

            // define the __doc__ and the __module__
            DefineName(Symbols.Doc);
            node.DocReference = Reference(Symbols.Doc);
            DefineName(Symbols.Module);
            node.ModReference = Reference(Symbols.Module);

            // Walk the body
            node.Body.Walk(this);
            return false;
        }

        // ClassDefinition
        public override void PostWalk(ClassDefinition node) {
            Debug.Assert(node == _currentScope);
            _processed.Add(_currentScope);
            _currentScope = _currentScope.Parent;

            StoreClass(node);
        }

        // DelStatement
        public override bool Walk(DelStatement node) {
            foreach (Expression e in node.Expressions) {
                e.Walk(_delete);
            }
            return true;
        }

        // ExecStatement
        public override bool Walk(ExecStatement node) {
            if (node.Locals == null && node.Globals == null) {
                Debug.Assert(_currentScope != null);
                _currentScope.ContainsUnqualifiedExec = true;
            }
            _currentScope.ContainsExec = true;
            return true;
        }

        public override void PostWalk(ExecStatement node) {
            if (node.NeedsLocalsDictionary()) {
                _currentScope.NeedsLocalsDictionary = true;
            }
        }

        // ForEachStatement
        public override bool Walk(ForStatement node) {
            node.Left.Walk(_define);
            // Add locals
            return true;
        }

        // WithStatement
        public override bool Walk(WithStatement node) {
            if (node.Variable != null) {
                node.Variable.Walk(_define);
            }
            return true;
        }

        // FromImportStatement
        public override bool Walk(FromImportStatement node) {
            if (node.Names != FromImportStatement.Star) {
                PythonReference[] references = new PythonReference[node.Names.Count];
                for (int i = 0; i < node.Names.Count; i++) {
                    SymbolId name = node.AsNames[i] != SymbolId.Empty ? node.AsNames[i] : node.Names[i];
                    DefineName(name);
                    references[i] = Reference(name);
                }
                node.References = references;
            } else {
                Debug.Assert(_currentScope != null);
                _currentScope.ContainsImportStar = true;
                _currentScope.NeedsLocalsDictionary = true;
            }
            return true;
        }

        // FunctionDefinition
        public override bool Walk(FunctionDefinition node) {
            // Name is defined in the enclosing context
            DefineName(node.Name);
            node.Reference = Reference(node.Name);

            // process the default arg values in the outer context
            foreach (Parameter p in node.Parameters) {
                if (p.DefaultValue != null) {
                    p.DefaultValue.Walk(this);
                }
            }
            // process the decorators in the outer context
            if (node.Decorators != null) {
                foreach (Expression dec in node.Decorators) {
                    dec.Walk(this);
                }
            }

            PushScope(node);

            foreach (Parameter p in node.Parameters) {
                p.Walk(_parameter);
            }

            node.Body.Walk(this);
            return false;
        }

        // FunctionDefinition
        public override void PostWalk(FunctionDefinition node) {
            Debug.Assert(_currentScope == node);
            _processed.Add(_currentScope);
            _currentScope = _currentScope.Parent;
        }

        // GlobalStatement
        public override bool Walk(GlobalStatement node) {
            foreach (SymbolId n in node.Names) {
                // Create the variable in the global context and mark it as global
                PythonVariable variable = _globalScope.DefineName(n);
                variable.Kind = VariableKind.Global;

                // Check current context for conflicting variable
                PythonVariable conflict;
                if (_currentScope.TryGetDefinition(n, out conflict)) {
                    if (conflict.Kind == VariableKind.Global) {
                        // Python allows global statement to list the same name multiple times
                        // or to list the same name in the multiple different global statements

                        // OK
                    } else if (conflict.Kind == VariableKind.Parameter) {
                        ReportSyntaxError(
                            String.Format(
                                System.Globalization.CultureInfo.InvariantCulture,
                                "name '{0}' is a function parameter and declared global",
                                SymbolTable.IdToString(n)),
                            node);
                    } else {
                        ReportSyntaxWarning(
                            String.Format(
                                System.Globalization.CultureInfo.InvariantCulture,
                                "Variable {0} assigned before global declaration",
                                SymbolTable.IdToString(n)),
                            node);
                    }
                } else {
                    // no previously definied variables, add it to the current context
                    _currentScope.AddGlobalDefinition(variable);
                }

                // Check for the name being referenced previously. If it has been, issue warning.
                PythonReference cref;
                if (_currentScope.TryGetReference(n, out cref)) {
                    ReportSyntaxWarning(
                        String.Format(
                        System.Globalization.CultureInfo.InvariantCulture,
                        "Variable {0} used before global declaration",
                        SymbolTable.IdToString(n)),
                    node);
                }
            }
            return true;
        }

        // GlobalSuite
        public override bool Walk(PythonAst node) {
            // If binding full fledged module, create references for the
            // __doc__ and __name__ variables
            if (node.Module) {
                node.DocReference = Reference(Symbols.Doc);
                node.NameReference = Reference(Symbols.Name);
            }
            return true;
        }

        // GlobalSuite
        public override void PostWalk(PythonAst node) {
            // Do not add the global suite to the list of processed nodes,
            // the publishing must be done after the class local binding.
            Debug.Assert(_currentScope == node);
            _currentScope = _currentScope.Parent;
        }
        // ImportStatement
        public override bool Walk(ImportStatement node) {
            PythonReference[] references = new PythonReference[node.Names.Count];
            for (int i = 0; i < node.Names.Count; i++) {
                SymbolId name = node.AsNames[i] != SymbolId.Empty ? node.AsNames[i] : node.Names[i].Names[0];
                DefineName(name);
                references[i] = Reference(name);
            }
            node.References = references;
            return true;
        }

        // TryStatement
        public override bool Walk(TryStatement node) {
            if (node.Handlers != null) {
                foreach (TryStatementHandler tsh in node.Handlers) {
                    if (tsh.Target != null) {
                        tsh.Target.Walk(_define);
                    }
                }
            }

            return true;
        }

        // ListComprehensionFor
        public override bool Walk(ListComprehensionFor node) {
            node.Left.Walk(_define);
            return true;
        }

        #endregion
    }
}
