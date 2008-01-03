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
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Runtime.Remoting;

using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting {

    // TODO: this class should be abstract
    public class ScopeExtension {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly")]
        public static readonly ScopeExtension[]/*!*/ EmptyArray = new ScopeExtension[0];

        private readonly Scope/*!*/ _scope;

        // TODO: is this meant to be on Scope or for invariant language (i.e. on InvariantScopeExtension)?
        private bool _showCls;

        // TODO: remove?
        private CompilerContext _compilerContext;

        public Scope/*!*/ Scope {
            get { return _scope; }
        }

        /// <summary>
        /// Returns the attributes associated with this LanguageContext's code.
        /// </summary>
        public virtual bool ShowCls {
            get {
                return _showCls;
            }
            set {
                _showCls = value;
            }
        }

        /// <summary>
        /// Returns the optional compiler context associated with this module.
        /// </summary>
        public CompilerContext CompilerContext {
            get {
                return _compilerContext;
            }
            set {
                _compilerContext = value;
            }
        }

        public ScopeExtension(Scope/*!*/ scope) {
            Contract.RequiresNotNull(scope, "scope");
            _scope = scope;
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        protected ScopeExtension(ScopeExtension/*!*/ extension) {
            Contract.RequiresNotNull(extension, "extension");
            _scope = extension._scope;
            _showCls = extension._showCls;
            _compilerContext = extension._compilerContext;
        }

        internal protected virtual void ModuleReloading() {
            _showCls = false;
        }

        internal protected virtual void ModuleReloaded() {
        }
    }
}
