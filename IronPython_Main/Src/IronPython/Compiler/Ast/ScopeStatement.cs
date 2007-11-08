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

using Microsoft.Scripting;
using MSAst = Microsoft.Scripting.Ast;
using VariableKind = Microsoft.Scripting.Ast.Variable.VariableKind;

namespace IronPython.Compiler.Ast {
    public abstract class ScopeStatement : Statement {
        private ScopeStatement _parent;

        private bool _importStar;                   // from module import *
        private bool _unqualifiedExec;              // exec "code"
        private bool _nestedFreeVariables;          // nested function with free variable
        private bool _locals;                       // The scope needs locals dictionary
                                                    // due to "exec" or call to dir, locals, eval, vars...
        private bool _closure;                      // the scope is a closure (its locals are referenced by nested scopes)


        private Dictionary<SymbolId, PythonVariable> _variables;
        private Dictionary<SymbolId, PythonReference> _references;

        public ScopeStatement Parent {
            get { return _parent; }
            set { _parent = value; }
        }

        protected abstract MSAst.CodeBlock Block { get; }

        internal void SetParent(MSAst.CodeBlock target) {
            if (_parent != null) {
                target.Parent = _parent.Block;
            }
        }

        internal bool ContainsImportStar {
            get { return _importStar; }
            set { _importStar = value; }
        }
        internal bool ContainsUnqualifiedExec {
            get { return _unqualifiedExec; }
            set { _unqualifiedExec = value; }
        }
        internal bool ContainsNestedFreeVariables {
            get { return _nestedFreeVariables; }
            set { _nestedFreeVariables = value; }
        }
        internal bool NeedsLocalsDictionary {
            get { return _locals; }
            set { _locals = value; }
        }
        internal bool IsClosure {
            get { return _closure; }
            set { _closure = value; }
        }

        internal Dictionary<SymbolId, PythonVariable> Variables {
            get { return _variables; }
        }

        protected virtual void CreateVariables(MSAst.CodeBlock block) {
            if (_variables != null) {
                foreach (KeyValuePair<SymbolId, PythonVariable> kv in _variables) {
                    // Publish variables for this context only (there may be references to the global variables
                    // in the dictionary also that were used for name binding lookups
                    // Do not publish parameters, they will get created separately.
                    if (kv.Value.Scope == this && kv.Value.Kind  != VariableKind.Parameter) {
                        kv.Value.Transform(block);
                    }
                }
            }

            // Require the context emits local dictionary
            if (NeedsLocalsDictionary) {
                block.EmitLocalDictionary = true;
            }
        }

        private bool TryGetAnyVariable(SymbolId name, out PythonVariable variable) {
            if (_variables != null) {
                return _variables.TryGetValue(name, out variable);
            } else {
                variable = null;
                return false;
            }
        }

        internal bool TryGetVariable(SymbolId name, out PythonVariable variable) {
            if (TryGetAnyVariable(name, out variable) && variable.Visible) {
                return true;
            } else {
                variable = null;
                return false;
            }
        }

        internal virtual bool TryBindOuter(SymbolId name, out PythonVariable variable) {
            // Hide scope contents by default (only functions expose their locals)
            variable = null;
            return false;
        }

        internal abstract PythonVariable BindName(SymbolId name);

        internal virtual void Bind(PythonNameBinder binder) {
            if (_references != null) {
                foreach (KeyValuePair<SymbolId, PythonReference> kv in _references) {
                    PythonVariable variable;
                    kv.Value.PythonVariable = variable = BindName(kv.Key);

                    // Accessing outer scope variable which is being deleted?
                    if (variable != null &&
                        variable.Deleted &&
                        variable.Kind != VariableKind.Global &&
                        (object)variable.Scope != (object)this) {

                        // report syntax error
                        binder.ReportSyntaxError(
                            String.Format(
                                System.Globalization.CultureInfo.InvariantCulture,
                                "can not delete variable '{0}' referenced in nested context",
                                SymbolTable.IdToString(kv.Key)
                                ),
                            this);
                    }
                }
            }
        }

        private void EnsureVariables() {
            if (_variables == null) {
                _variables = new Dictionary<SymbolId, PythonVariable>();
            }
        }

        internal void AddGlobalVariable(PythonVariable variable) {
            EnsureVariables();
            _variables[variable.Name] = variable;
        }

        internal PythonReference Reference(SymbolId name) {
            if (_references == null) {
                _references = new Dictionary<SymbolId, PythonReference>();
            }
            PythonReference reference;
            if (!_references.TryGetValue(name, out reference)) {
                _references[name] = reference = new PythonReference(name);
            }
            return reference;
        }

        internal bool IsReferenced(SymbolId name) {
            PythonReference reference;
            return _references != null && _references.TryGetValue(name, out reference);
        }

        internal PythonVariable CreateVariable(SymbolId name, VariableKind kind) {
            EnsureVariables();
            Debug.Assert(!_variables.ContainsKey(name));
            PythonVariable variable;
            _variables[name] = variable = new PythonVariable(name, kind, this);
            return variable;
        }

        internal PythonVariable EnsureVariable(SymbolId name) {
            PythonVariable variable;
            if (!TryGetVariable(name, out variable)) {
                return CreateVariable(name, VariableKind.Local);
            }
            return variable;
        }

        internal PythonVariable EnsureHiddenVariable(SymbolId name) {
            PythonVariable variable;
            if (!TryGetAnyVariable(name, out variable)) {
                variable = CreateVariable(name, VariableKind.Local);
                variable.Hide();
            }
            return variable;
        }

        internal PythonVariable DefineParameter(SymbolId name) {
            return CreateVariable(name, VariableKind.Parameter);
        }

        protected internal PythonAst GetGlobalScope() {
            ScopeStatement global = this;
            while (global.Parent != null) {
                global = global.Parent;
            }
            Debug.Assert(global is PythonAst);
            return global as PythonAst;
        }
    }
}
