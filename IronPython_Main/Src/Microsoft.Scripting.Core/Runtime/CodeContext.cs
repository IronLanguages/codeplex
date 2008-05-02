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
    /// TODO: Rename to LocalScope
    /// </summary>    
    public class CodeContext {
        public const string ContextFieldName = "__global_context";

        // TODO: move to subclasses
        private readonly Scope _scope;

        // TODO: move to subclasses
        private readonly LanguageContext _languageContext;

        private readonly CodeContext _parent;

        public CodeContext Parent {
            get { return _parent; }
        }

        // TODO: remove
        public Scope Scope {
            get {
                return _scope;
            }
        }

        public virtual Scope GlobalScope {
            get {
                Debug.Assert(_scope != null, "Global scope not available");
                return _scope.ModuleScope;
            }
        }

        public LanguageContext/*!*/ LanguageContext {
            get {
                return _languageContext;
            }
        }

        // TODO: remove: DLR should use an internal tuple chain instead of asking code context:
        internal protected virtual TTuple/*!*/ GetStorage<TTuple>() where TTuple : Tuple {
            return ((TupleDictionary<TTuple>)_scope.Dict).TupleData;
        }

        // TODO: remove: DLR should use an internal tuple chain instead of asking code context:
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        internal protected virtual CodeContext GetStorageParent() {
            return _parent;
        }

        public CodeContext(Scope scope, LanguageContext/*!*/ languageContext)
            : this(scope, languageContext, null) {
        }

        public CodeContext(Scope scope, LanguageContext/*!*/ languageContext, CodeContext parent) {
            Assert.NotNull(languageContext);

            _languageContext = languageContext;
            _scope = scope;
            _parent = parent;
        }
    }   
}
