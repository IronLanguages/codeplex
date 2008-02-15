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
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Generation;
using Microsoft.Contracts;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting {
    /// <summary>
    /// ScriptCode is an instance of compiled code that is bound to a specific LanguageContext
    /// but not a specific ScriptScope.  The code can be re-executed multiple times in different
    /// contexts. Hosting API counterpart for this class is <see cref="CompiledCode"/>.
    /// </summary>
    public class ScriptCode {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly")]
        public static readonly ScriptCode[] EmptyArray = new ScriptCode[0];

        private readonly CodeBlock _code;
        private readonly LanguageContext _languageContext;
        private readonly CompilerContext _compilerContext;

        private CallTargetWithContext0 _simpleTarget;

        private CallTargetWithContext0 _optimizedTarget;
        private Scope _optimizedScope;

        internal ScriptCode(CodeBlock code, LanguageContext languageContext, CompilerContext compilerContext) {
            Assert.NotNull(code, languageContext, compilerContext);
            
            _code = code;
            _languageContext = languageContext;
            _compilerContext = compilerContext;
        }

        public LanguageContext LanguageContext {
            get { return _languageContext; }
        }

        public CompilerContext CompilerContext {
            get { return _compilerContext; }
        }

        public SourceUnit SourceUnit {
            get { return _compilerContext.SourceUnit; }
        }

        internal CodeBlock CodeBlock {
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

        internal CallTargetWithContext0 OptimizedTarget {
            set {
                _optimizedTarget = value;
            }
        }
        
        public void EnsureCompiled() {            
            if (_simpleTarget == null) {
                lock (this) { // TODO: mutex object
                    if (_simpleTarget == null) {
                        _simpleTarget = Compiler.CompileTopLevelCodeBlock(SourceUnit, _code);
                    }
                }
            }
        }

        private object Run(CodeContext codeContext, bool tryEvaluate) {
            
            codeContext.ModuleContext.CompilerContext =_compilerContext;

            // Python only: assigns TrueDivision from _compilerContext to codeContext.ModuleContext
            _languageContext.ModuleContextEntering(codeContext.ModuleContext);


            bool doEvaluation = tryEvaluate || _languageContext.Options.InterpretedMode;
            if (_simpleTarget == null && _optimizedTarget == null
                && doEvaluation
                && Ast.InterpretChecker.CanEvaluate(_code, _languageContext.Options.ProfileDrivenCompilation)) {
                return Interpreter.TopLevelExecute(_code, codeContext);
            }

            if (codeContext.Scope == _optimizedScope) { // flag on scope - "IsOptimized"?
                // TODO: why do we create a code context here?
                return _optimizedTarget(new CodeContext(_optimizedScope, _languageContext, codeContext.ModuleContext));
            }

            EnsureCompiled();
            return _simpleTarget(codeContext);
        }

        public object Run(Scope/*!*/ scope) {
            Contract.RequiresNotNull(scope, "scope");

            ScopeExtension scopeExtension = _languageContext.EnsureScopeExtension(scope);
            return Run(new CodeContext(scope, _languageContext, scopeExtension), false);
        }

        public object Run(Scope/*!*/ scope, ScopeExtension/*!*/ moduleContext) {
            return Run(scope, moduleContext, false);
        }

        public object Run(Scope/*!*/ scope, ScopeExtension/*!*/ moduleContext, bool tryEvaluate) {
            Contract.RequiresNotNull(scope, "scope");
            Contract.RequiresNotNull(moduleContext, "moduleContext");

            return Run(new CodeContext(scope, _languageContext, moduleContext), tryEvaluate);
        }

        [Confined]
        public override string/*!*/ ToString() {
            return string.Format("ScriptCode '{0}' from {1}", SourceUnit, _languageContext.DisplayName);
        }

        public static ScriptCode/*!*/ FromCompiledCode(CompiledCode/*!*/ compiledCode) {
            Contract.RequiresNotNull(compiledCode, "compiledCode");
            return compiledCode.ScriptCode;
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

            _languageContext.EnsureScopeExtension(scope);
            return scope;
        }
    }
}
