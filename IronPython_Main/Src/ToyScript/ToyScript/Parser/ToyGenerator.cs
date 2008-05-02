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

using MSAst = Microsoft.Scripting.Ast;

using ToyScript.Parser.Ast;
using Microsoft.Scripting.Actions;

namespace ToyScript.Parser {
    class ToyGenerator {
        private readonly ToyLanguageContext _tlc;
        private ToyScope _scope;

        private ToyGenerator(ToyLanguageContext tlc, string name) {
            _tlc = tlc;
            PushNewScope(name);
        }

        internal ToyLanguageContext Tlc {
            get { return _tlc; }
        }

        internal ActionBinder Binder {
            get {
                return _tlc.Binder;
            }
        }

        internal ToyScope Scope {
            get {
                return _scope;
            }
        }

        internal ToyScope PushNewScope(string name) {
            return _scope = new ToyScope(name, _scope);
        }

        internal void PopScope() {
            _scope = _scope.Parent;
        }

        internal MSAst.Expression LookupName(string name) {
            return _scope.LookupName(name);
        }

        internal MSAst.Expression GetOrMakeLocal(string name) {
            return _scope.GetOrMakeLocal(name);
        }

        internal MSAst.Expression GetOrMakeGlobal(string name) {
            return _scope.TopScope.GetOrMakeLocal(name);
        }

        internal static MSAst.LambdaExpression Generate(ToyLanguageContext tlc, Statement statement, string name) {
            ToyGenerator tg = new ToyGenerator(tlc, name);

            MSAst.Expression body = statement.Generate(tg);

            return tg.Scope.FinishScope(body);
        }
    }
}
