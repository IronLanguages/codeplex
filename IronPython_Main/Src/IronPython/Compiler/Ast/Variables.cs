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
using System.Diagnostics;

using Microsoft.Scripting;

using MSAst = Microsoft.Scripting.Internal.Ast;

namespace IronPython.Compiler.Ast {
    class PythonVariable {
        [Flags]
        public enum PythonFlags {
            // Python specific flags
            Deleted = 0x01,             // del x
            Assigned = 0x02,            // x = ...
            Parameter = 0x04,           // def f(x) or def f(a, (x, b)) ..
        }

        private readonly SymbolId _name;
        private readonly Type _type;
        private readonly ScopeStatement _scope;

        private MSAst.Variable.VariableKind _kind;
        private PythonFlags _flags;

        private MSAst.Variable _variable;

        public PythonVariable(SymbolId name, ScopeStatement scope)
            : this(name, typeof(object), scope) {
        }

        public PythonVariable(SymbolId name, Type type, ScopeStatement scope) {
            _name = name;
            _type = type;
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

        public PythonFlags Flags {
            get { return _flags; }
            set { _flags = value; }
        }

        public MSAst.Variable Variable {
            get {
                Debug.Assert(_variable != null);
                return _variable;
            }
        }

        internal void SetParameter(MSAst.Parameter parameter) {
            Debug.Assert(_variable == null);
            _variable = parameter;
        }

        internal MSAst.Variable Transform(MSAst.CodeBlock block) {
            Debug.Assert(_variable == null);
            if (_variable == null) {
                _variable = MSAst.Variable.Create(_name, _kind, block, _type);
            }
            Debug.Assert(block == _variable.Block);
            return _variable;
        }
    }

    public class PythonReference {
        private SymbolId _name;
        private PythonVariable _variable;

        private MSAst.VariableReference _reference;

        public PythonReference(SymbolId name) {
            _name = name;
        }

        public SymbolId Name {
            get { return _name; }
        }

        internal PythonVariable Variable {
            get { return _variable; }
            set { _variable = value; }
        }

        public MSAst.VariableReference Reference {
            get {
                Debug.Assert(_reference != null);
                return _reference;
            }
        }

        internal MSAst.VariableReference Transform() {
            if (_reference == null) {
                _reference = new MSAst.VariableReference(_name);
                if (_variable != null) {
                    _reference.Variable = _variable.Variable;
                }
            }
            return _reference;
        }
    }
}
