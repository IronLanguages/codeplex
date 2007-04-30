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

using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.Scripting;
using MSAst = Microsoft.Scripting.Internal.Ast;
using VariableKind = Microsoft.Scripting.Internal.Ast.Variable.VariableKind;

namespace IronPython.Compiler.Ast {
    public abstract class ScopeStatement : Statement {
        private ScopeStatement _parent;
        private PythonScopeFlags _flags;

        private Dictionary<SymbolId, PythonVariable> _variables;
        private Dictionary<SymbolId, PythonReference> _references;
        private List<PythonReference> _temprefs;

        public ScopeStatement Parent {
            get { return _parent; }
            set { _parent = value; }
        }

        internal PythonScopeFlags Flags {
            get { return _flags; }
        }

        protected abstract MSAst.CodeBlock Block { get; }

        internal void SetParent(MSAst.CodeBlock target) {
            if (_parent != null) {
                target.Parent = _parent.Block;
            }
        }

        internal bool ContainsImportStar {
            get { return GetFlag(PythonScopeFlags.ContainsImportStar); }
            set { SetFlag(value, PythonScopeFlags.ContainsImportStar); }
        }
        internal bool ContainsUnqualifiedExec {
            get { return GetFlag(PythonScopeFlags.ContainsUnqualifiedExec); }
            set { SetFlag(value, PythonScopeFlags.ContainsUnqualifiedExec); }
        }
        internal bool ContainsExec {
            get { return (_flags & PythonScopeFlags.ContainsExec) != 0; }
            set { SetFlag(value, PythonScopeFlags.ContainsExec); }
        }
        internal bool ContainsNestedFreeVariables {
            get { return GetFlag(PythonScopeFlags.ContainsNestedFreeVariables); }
            set { SetFlag(value, PythonScopeFlags.ContainsNestedFreeVariables); }
        }
        internal bool NeedsLocalsDictionary {
            get { return GetFlag(PythonScopeFlags.NeedsLocalsDictionary); }
            set { SetFlag(value, PythonScopeFlags.NeedsLocalsDictionary); }
        }
        internal bool IsClosure {
            get { return GetFlag(PythonScopeFlags.IsClosure); }
            set { SetFlag(value, PythonScopeFlags.IsClosure); }
        }

        private bool GetFlag(PythonScopeFlags flag) {
            return (_flags & flag) != 0;
        }
        private void SetFlag(bool value, PythonScopeFlags flag) {
            if (value) {
                _flags |= flag;
            } else {
                _flags &= ~flag;
            }
        }

        // Scope has fixed locals if no locals can be introduced at runtime
        // This is true if the context contains neither exec (in any form, either qualified or non-qualified) nor import *
        // Note that even qualified exec may introduce locals: "exec 'x = 1' in globals(), locals()"
        internal bool HasFixedLocals {
            get {
                return (_flags & (PythonScopeFlags.ContainsExec | PythonScopeFlags.ContainsImportStar)) == 0;
            }
        }

        //[Obsolete("Get rid of this")]
        internal bool IsGlobal {
            get {
                Debug.Assert(this is PythonAst || _parent != null);
                return _parent == null;
            }
        }

        //[Obsolete("Remove")]
        public bool IsFunctionScope {
            get { return GetType() == typeof(FunctionDefinition); }
        }

        internal Dictionary<SymbolId, PythonVariable> Definitions {
            get { return _variables; }
        }
        internal Dictionary<SymbolId, PythonReference> References {
            get { return _references; }
        }

        protected virtual void CreateVariables(MSAst.CodeBlock block) {
            if (_variables != null) {
                foreach (KeyValuePair<SymbolId, PythonVariable> kv in _variables) {
                    // Publish variables for this context only (there may be references to the global variables
                    // in the dictionary also that were used for name binding lookups
                    // Do not publish parameters, they will get created separately.
                    if (kv.Value.Scope == this && kv.Value.Kind  != VariableKind.Parameter) {
                        block.AddVariable(kv.Value.Transform(block));
                    }
                }
            }
            if (_references != null) {
                foreach (KeyValuePair<SymbolId, PythonReference> kv in _references) {
                    block.AddReference(kv.Value.Transform());
                }
            }
            if (_temprefs != null) {
                foreach (PythonReference r in _temprefs) {
                    block.AddReference(r.Transform());
                }
            }

            // Require the context emits local dictionary
            if (NeedsLocalsDictionary) {
                block.EmitLocalDictionary = true;
            }
        }

        private PythonVariable EnsureDefinition(SymbolId name) {
            PythonVariable variable;
            EnsureDefinitions();
            if (!_variables.TryGetValue(name, out variable)) {
                variable = new PythonVariable(name, this);
                _variables[name] = variable;
            }
            return variable;
        }

        private void EnsureDefinitions() {
            if (_variables == null) {
                _variables = new Dictionary<SymbolId, PythonVariable>();
            }
        }

        internal bool TryGetDefinition(SymbolId name, out PythonVariable variable) {
            if (_variables == null) {
                variable = null;
                return false;
            }
            return _variables.TryGetValue(name, out variable);
        }

        public PythonReference EnsureReference(SymbolId name) {
            PythonReference reference;
            EnsureReferences();
            if (!_references.TryGetValue(name, out reference)) {
                reference = new PythonReference(name);
                _references[name] = reference;
            }
            return reference;
        }

        private void EnsureReferences() {
            if (_references == null) {
                _references = new Dictionary<SymbolId, PythonReference>();
            }
        }

        internal bool TryGetReference(SymbolId name, out PythonReference reference) {
            if (_references == null) {
                reference = null;
                return false;
            }
            return _references.TryGetValue(name, out reference);
        }

        internal void AddTemporaryReference(PythonReference reference) {
            if (_temprefs == null) {
                _temprefs = new List<PythonReference>();
            }
            _temprefs.Add(reference);
        }

        internal PythonVariable DefineName(SymbolId name) {
            PythonVariable variable = EnsureDefinition(name);
            variable.Flags |= PythonVariable.PythonFlags.Assigned;
            return variable;
        }

        internal void DefineParameter(SymbolId name) {
            PythonVariable variable = EnsureDefinition(name);
            Debug.Assert(variable.Kind == VariableKind.Local);
            variable.Kind = VariableKind.Parameter;
        }

        internal void DefineDeleted(SymbolId name) {
            PythonVariable variable = EnsureDefinition(name);
            variable.Flags |= PythonVariable.PythonFlags.Deleted;
        }

        internal void AddGlobalDefinition(PythonVariable variable) {
            EnsureDefinitions();
            _variables[variable.Name] = variable;
        }
    }
}
