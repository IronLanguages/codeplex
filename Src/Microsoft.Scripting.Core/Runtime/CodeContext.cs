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
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;

using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Runtime {
    /// <summary>
    /// TODO
    /// </summary>    
    public sealed class CodeContext {
        public const string ContextFieldName = "__global_context";

        /// <summary> can be any dictionary or IMapping. </summary>        
        private readonly Scope _scope;
        private readonly LanguageContext _languageContext;
        private readonly ScopeExtension _moduleContext; // internally mutable, optional (shouldn't be used when not set)
        private LocalScope _localScope;
        private readonly CodeContext _parent; // TODO: Ruby hack, move to local scope

        public LocalScope LocalScope {
            get { return _localScope; }
            set { _localScope = value; }
        }

        public CodeContext Parent {
            get { return _parent; }
        }

        // emitted (OptimizedModuleGenerator, Compiler, Interpreter, PropertyEnvironmentFactory, TupleSlotFactory)
        public Scope Scope {
            get {
                return _scope;
            }
        }

        public LanguageContext LanguageContext {
            get {
                return _languageContext;
            }
        }

        public ScopeExtension ModuleContext {
            get {
                return _moduleContext ?? _languageContext.EnsureScopeExtension(_scope);
            }
        }

        public CodeContext(CodeContext parent)
            : this(parent.Scope, parent.LanguageContext, parent.ModuleContext) {
            _parent = parent;
        }

        public CodeContext(Scope scope, LanguageContext languageContext)
            : this(scope, languageContext, null) {
        }

        internal CodeContext(Scope scope, LanguageContext languageContext, ScopeExtension moduleContext) 
            : this(scope, languageContext, moduleContext, null) {
        }

        internal CodeContext(Scope scope, LanguageContext languageContext, ScopeExtension moduleContext, CodeContext parent) {
            Assert.NotNull(scope, languageContext);

            _languageContext = languageContext;
            _moduleContext = moduleContext;
            _scope = scope;
            _parent = parent;
        }
        
    }   
}
