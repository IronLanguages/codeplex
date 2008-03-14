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
using Microsoft.Scripting;
using MSAst = Microsoft.Scripting.Ast;

using ToyScript.Runtime;
using ToyScript.Parser.Ast;

namespace ToyScript.Parser {
    class ToyGenerator {
        private ToyScope _scope;

        private ToyGenerator(string name) {
            PushNewScope(name);
        }

        public ToyScope Scope {
            get {
                return _scope;
            }
        }

        public ToyScope PushNewScope(string name) {
            return _scope = new ToyScope(name, _scope);
        }

        public void PopScope() {
            _scope = _scope.Parent;
        }

        internal MSAst.Variable LookupName(string name) {
            return _scope.LookupName(name);
        }

        internal MSAst.Variable GetOrMakeLocal(string name) {
            return _scope.GetOrMakeLocal(name);
        }

        internal MSAst.Variable GetOrMakeGlobal(string name) {
            return _scope.TopScope.GetOrMakeLocal(name);
        }

        internal static MSAst.LambdaExpression Generate(Statement statement, string name) {
            ToyGenerator tg = new ToyGenerator(name);

            MSAst.Expression body = statement.Generate(tg);

            return tg.Scope.FinishScope(body);
        }
    }
}
