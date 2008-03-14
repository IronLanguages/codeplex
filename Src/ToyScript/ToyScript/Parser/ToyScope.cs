/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
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
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Math;
using MSAst = Microsoft.Scripting.Ast;

namespace ToyScript.Parser {
    class ToyScope {
        private ToyScope _parent;
        private MSAst.LambdaBuilder _block;
        private Dictionary<string, MSAst.Variable> _variables = new Dictionary<string, MSAst.Variable>();

        public ToyScope(string name, ToyScope parent) {
            _block = MSAst.Ast.Lambda(name ?? "<toyblock>", typeof(object));
            _parent = parent;
        }

        public ToyScope Parent {
            get {
                return _parent;
            }
        }

        public ToyScope TopScope {
            get {
                if (_parent == null) {
                    return this;
                } else {
                    return _parent.TopScope;
                }
            }
        }

        public MSAst.Variable CreateParameter(string name) {
            MSAst.Variable variable = _block.CreateParameter(SymbolTable.StringToId(name), typeof(object));
            _variables[name] = variable;
            return variable;
        }

        public MSAst.Variable GetOrMakeLocal(string name) {
            return GetOrMakeLocal(name, typeof(object));
        }

        public MSAst.Variable GetOrMakeLocal(string name, Type type) {
            MSAst.Variable variable;
            if (_variables.TryGetValue(name, out variable)) {
                return variable;
            }
            variable = _block.CreateLocalVariable(SymbolTable.StringToId(name), type);
            _variables[name] = variable;
            return variable;
        }

        public MSAst.Variable LookupName(string name) {
            MSAst.Variable var;
            if (_variables.TryGetValue(name, out var)) {
                return var;
            }

            if (_parent != null) {
                return _parent.LookupName(name);
            } else {
                return null;
            }
        }

        public MSAst.Variable CreateTemporaryVariable(string name, Type type) {
            return _block.CreateTemporaryVariable(SymbolTable.StringToId(name), type);
        }

        public MSAst.LambdaExpression FinishScope(MSAst.Expression body) {
            _block.Body = body;
            return _block.MakeLambda();
        }
    }
}

