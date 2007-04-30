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
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Diagnostics;

using Microsoft.Scripting.Internal.Generation;
using Microsoft.Scripting.Internal.Ast;

using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Internal;
using Microsoft.Scripting.Actions;



namespace Microsoft.Scripting {
    /// <summary>
    /// ScriptCode is an instance of compiled code that is bound to a specific LanguageContext
    /// but not a specific ScriptModule.  The code can be re-executed multiple times in different
    /// contexts. Hosting API counterpart for this class is <see cref="CompiledCode"/>.
    /// </summary>
    public class ScriptCode {
        public static readonly ScriptCode[] EmptyArray = new ScriptCode[0];

        private CodeBlock _code;
        private LanguageContext _languageContext;
        private CompilerContext _compilerContext;

        private CallTargetWithContext0 _simpleTarget;

        private CallTargetWithContext0 _optimizedTarget;
        private Scope _optimizedScope;

        public ScriptCode(CodeBlock code, LanguageContext languageContext, CompilerContext compilerContext) {
            Debug.Assert(code != null && languageContext != null && compilerContext != null);
            
            this._code = code;
            this._languageContext = languageContext;
            this._compilerContext = compilerContext;
        }

        public LanguageContext LanguageContext {
            get { return _languageContext; }
            internal set { _languageContext = value; }
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
                lock (this) {
                    if (_simpleTarget == null) {
                        _simpleTarget = _code.CreateDelegate<CallTargetWithContext0>(CompilerContext);
                    }
                }
            }
        }

        private object Run(CodeContext codeContext) {
            if (codeContext.Scope == _optimizedScope) {
                return _optimizedTarget(new CodeContext(_optimizedScope, LanguageContext));
            }

            EnsureCompiled();
            return _simpleTarget(codeContext);
        }

        public object Run(ScriptModule module) {
            return Run(new CodeContext(module.Scope, LanguageContext.GetLanguageContextForModule(module)));
        }

        public object Run(Scope scope) {
            return Run(new CodeContext(scope, LanguageContext));
        }

        public override string ToString() {
            return string.Format("ScriptCode {0} from {1}", 
                SourceUnit.Name, 
                LanguageContext.Engine.LanguageProvider.LanguageDisplayName);
        }

        public static ScriptCode FromCompiledCode(CompiledCode compiledCode) {
            if (compiledCode == null) throw new ArgumentNullException("compiledCode");
            return compiledCode.ScriptCode;
        }
    }
}
