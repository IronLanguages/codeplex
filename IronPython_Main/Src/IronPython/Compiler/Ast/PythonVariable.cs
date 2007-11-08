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

namespace IronPython.Compiler.Ast {
    class PythonVariable {
        private readonly SymbolId _name;
        private readonly Type _type;
        private readonly ScopeStatement _scope;
        private bool _visible = true;       // variable visible to the nested scopes - the default
        private bool _deleted;              // del x

        private MSAst.Variable.VariableKind _kind;
        private MSAst.Variable _variable;

        public PythonVariable(SymbolId name, MSAst.Variable.VariableKind kind, ScopeStatement scope)
            : this(name, typeof(object), kind, scope) {
        }

        public PythonVariable(SymbolId name, Type type, MSAst.Variable.VariableKind kind, ScopeStatement scope) {
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

        public MSAst.Variable.VariableKind Kind {
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
        }
        internal void MarkDeleted() {
            _deleted = true;
        }

        public MSAst.Variable Variable {
            get {
                Debug.Assert(_variable != null);
                return _variable;
            }
        }

        internal void SetParameter(MSAst.Variable parameter) {
            Debug.Assert(_variable == null);
            _variable = parameter;
        }

        internal MSAst.Variable Transform(MSAst.CodeBlock block) {
            Debug.Assert(_kind != MSAst.Variable.VariableKind.Parameter);
            return _variable = block.CreateVariable(_name, _kind, _type);
        }
    }
}
