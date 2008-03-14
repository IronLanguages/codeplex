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
using System.Diagnostics;

using Microsoft.Scripting;
using MSAst = Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;

namespace IronPython.Compiler.Ast {
    class PythonVariable {
        private readonly SymbolId _name;
        private readonly Type _type;
        private readonly ScopeStatement _scope;
        private bool _visible = true;       // variable visible to the nested scopes - the default
        private bool _deleted;              // del x
        private bool _unassigned;           // Variable ever referenced without being assigned
        private bool _uninitialized;        // Variable ever used either uninitialized or after deletion

        private bool _fallback;             // If uninitialized, lookup in builtins

        private int _index;                 // Index for flow checker

        private VariableKind _kind;
        private MSAst.VariableExpression _variable;

        public PythonVariable(SymbolId name, VariableKind kind, ScopeStatement scope)
            : this(name, typeof(object), kind, scope) {
        }

        public PythonVariable(SymbolId name, Type type, VariableKind kind, ScopeStatement scope) {
            _name = name;
            _type = type;
            _kind = kind;
            _scope = scope;
        }

        public SymbolId Name {
            get { return _name; }
        }

        public Type Type {
            get { return _type; }
        }

        public ScopeStatement Scope {
            get { return _scope; }
        }

        public VariableKind Kind {
            get { return _kind; }
            set { _kind = value; }
        }

        internal bool Visible {
            get { return _visible; }
        }
        internal void Hide() {
            _visible = false;
        }

        internal bool Deleted {
            get { return _deleted; }
            set { _deleted = value; }
        }

        internal int Index {
            get { return _index; }
            set { _index = value; }
        }

        public bool Unassigned {
            get { return _unassigned; }
            set { _unassigned = value; }
        }

        public bool Uninitialized {
            get { return _uninitialized; }
            set { _uninitialized = value; }
        }

        internal bool Fallback {
            get { return _fallback; }
            set { _fallback = value; }
        }

        public MSAst.VariableExpression Variable {
            get {
                Debug.Assert(_variable != null);
                return _variable;
            }
        }

        internal void SetParameter(MSAst.VariableExpression parameter) {
            Debug.Assert(_variable == null);
            _variable = parameter;
        }

        internal MSAst.VariableExpression Transform(AstGenerator ag) {
            Debug.Assert(_kind != VariableKind.Parameter);
            switch (_kind) {
                case VariableKind.Global:
                    return _variable = ag.Block.CreateGlobalVariable(_name, _type);
                case VariableKind.Local:
                    return _variable = ag.Block.CreateLocalVariable(_name, _type);
                case VariableKind.Temporary:
                    return _variable = ag.Block.CreateTemporaryVariable(_name, _type);
                default: throw Assert.Unreachable;
            }
        }
    }
}
