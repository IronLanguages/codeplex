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
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting {
    /// <summary>
    /// CodeContext represents an environment for execution of script code.  
    /// 
    /// The execution environment consists of both a set of locals and globals.  It also consists
    /// with various engine state such as the currently executing module, any flags that might
    /// affect that engine, etc...
    /// 
    /// A CodeContext is logically associated with a Module ?
    /// </summary>    
    public sealed class CodeContext {
        // The name that is used for static fields that hold a CodeContext to be shared
        public const string ContextFieldName = "__global_context";

        /// <summary> can be any dictionary or IMapping. </summary>        
        private readonly Scope _scope;
        private readonly LanguageContext _languageContext;
        private ScopeExtension _moduleContext; // internally mutable, optional (shouldn't be used when not set)
        private LocalScope _localScope;
        private CodeContext _parent; // TODO: Ruby hack, move to local scope

        public CodeContext(CodeContext parent)
            : this(parent.Scope, parent.LanguageContext, parent.ModuleContext) {
            _parent = parent;
        }

        public CodeContext(Scope scope, LanguageContext languageContext)
            : this(scope, languageContext, null) {
        }

        public CodeContext(Scope scope, LanguageContext languageContext, ScopeExtension moduleContext) {
            Assert.NotNull(scope, languageContext);

            _languageContext = languageContext;
            _moduleContext = moduleContext;
            _scope = scope;
        }

        #region Public API Surface

        public LocalScope LocalScope {
            get { return _localScope; }
            set { _localScope = value; }
        }

        public CodeContext Parent {
            get { return _parent; }
        }

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

        #endregion
    }   
}
