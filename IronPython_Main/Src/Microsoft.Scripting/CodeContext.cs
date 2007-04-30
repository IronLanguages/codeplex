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
using System.Diagnostics;
using System.Reflection;
using System.Globalization;

using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Internal.Generation;
using Microsoft.Scripting.Hosting;
using System.Threading;

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
        private Scope _scope;
        private LanguageContext _context;

        public CodeContext(CodeContext parent, IAttributesCollection locals) :
            this(new Scope(parent.Scope, locals), parent.LanguageContext) {            
        }

        public CodeContext(Scope scope, LanguageContext context) {
            _context = context;
            _scope = scope;
        }

        #region Public API Surface

        public Scope Scope {
            get {
                return _scope;
            }
        }

        public LanguageContext LanguageContext {
            get {
                return _context;
            }
        }

        /// <summary>
        /// TODO: Remove me
        /// </summary>
        public IAttributesCollection Locals {
            get { return Scope.Dict; }
        }        
        
        #endregion
    }   
}
