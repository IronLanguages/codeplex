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

using System.Diagnostics;

using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Generation;
using Microsoft.Contracts;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting {
    /// <summary>
    /// ScriptCode is an instance of compiled code that is bound to a specific LanguageContext
    /// but not a specific ScriptScope.  The code can be re-executed multiple times in different
    /// contexts. Hosting API counterpart for this class is <c>CompiledCode</c>.
    /// </summary>
    public class ScriptCode {
        private readonly LambdaExpression/*!*/ _code;
        private readonly LanguageContext/*!*/ _languageContext;
        private readonly CompilerContext/*!*/ _compilerContext;

        private DlrMainCallTarget _simpleTarget;

        private DlrMainCallTarget _optimizedTarget;
        private Scope _optimizedScope;

        internal ScriptCode(LambdaExpression/*!*/ code, LanguageContext/*!*/ languageContext, CompilerContext/*!*/ compilerContext) {
            Assert.NotNull(code, languageContext, compilerContext);
            
            _code = code;
            _languageContext = languageContext;
            _compilerContext = compilerContext;
        }

        public LanguageContext/*!*/ LanguageContext {
            get { return _languageContext; }
        }

        public CompilerContext/*!*/ CompilerContext {
            get { return _compilerContext; }
        }

        public SourceUnit/*!*/ SourceUnit {
            get { return _compilerContext.SourceUnit; }
        }

        internal LambdaExpression/*!*/ Lambda {
            get {
                return _code;
            }
        }

        internal Scope OptimizedScope {
            set {
                Debug.Assert(_optimizedScope == null);
                _optimizedScope = value;
            }
        }

        internal DlrMainCallTarget OptimizedTarget {
            set {
                _optimizedTarget = value;
            }
        }
        
        public void EnsureCompiled() {            
            if (_simpleTarget == null) {
                lock (this) { // TODO: mutex object
                    if (_simpleTarget == null) {
                        _simpleTarget = LambdaCompiler.CompileTopLevelLambda(SourceUnit, _code);
                    }
                }
            }
        }

        public object Run(Scope/*!*/ scope) {
            return Run(scope, false);
        }

        public object Run(Scope/*!*/ scope, bool tryEvaluate) {
            ContractUtils.RequiresNotNull(scope, "scope");

            ScopeExtension scopeExtension = _languageContext.EnsureScopeExtension(scope.ModuleScope);
            
            scope.ModuleScope.CompilerContext = _compilerContext;

            // Python only: assigns TrueDivision from _compilerContext to codeContext.ModuleContext
            _languageContext.ModuleContextEntering(scopeExtension);

            return Run(new CodeContext(scope, _languageContext), tryEvaluate);
        }

        public object Run(CodeContext/*!*/ context, bool tryEvaluate) {
            ContractUtils.RequiresNotNull(context, "context");

            bool doEvaluation = tryEvaluate || _languageContext.Options.InterpretedMode;
            if (_simpleTarget == null && _optimizedTarget == null
                && doEvaluation
                && Interpreter.InterpretChecker.CanEvaluate(_code)) {

                return Interpreter.Interpreter.TopLevelExecute(_code, context);
            }

            if (context.GlobalScope == _optimizedScope) {
                return _optimizedTarget(context);
            }

            EnsureCompiled();
            return _simpleTarget(context);
        }

        [Confined]
        public override string/*!*/ ToString() {
            return string.Format("ScriptCode '{0}' from {1}", SourceUnit, _languageContext.DisplayName);
        }

        public Scope/*!*/ MakeOptimizedScope() {
            Scope scope;

            // TODO:
            // This is a "double-bug". Both of the following issues need to be fixed:
            // 1) If the follwoing code is uncommented (a non-optimized scope is returned), interpretation tests will fail some assertions (bug #375352).
            // 2) Interpreted mode shouldn't totaly give up optimized scopes. Optimized storage should be available for interpreter.
            // But that would require decoupling optimized storage from compiled code.

            //if (_languageContext.Options.InterpretedMode) {
            //    scope = new Scope(_languageContext);
            //} else {
                scope = OptimizedModuleGenerator.Create(this).GenerateScope();
            //}

            return scope;
        }
    }
}
