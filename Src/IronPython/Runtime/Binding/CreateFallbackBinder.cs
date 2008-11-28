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

using System; using Microsoft;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Linq.Expressions;
using Microsoft.Scripting;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronPython.Runtime.Binding {
    /// <summary>
    /// Fallback action for performing a new() on a foreign IDynamicObject.  used
    /// when call falls back.
    /// </summary>
    class CreateFallback : CreateInstanceBinder, IPythonSite {
        private readonly CompatibilityInvokeBinder/*!*/ _fallback;

        public CreateFallback(CompatibilityInvokeBinder/*!*/ realFallback, IEnumerable<ArgumentInfo/*!*/>/*!*/ arguments)
            : base(arguments) {
            _fallback = realFallback;
        }

        public override MetaObject/*!*/ FallbackCreateInstance(MetaObject/*!*/ target, MetaObject/*!*/[]/*!*/ args, MetaObject onBindingError) {
            return _fallback.InvokeFallback(target, args, BindingHelpers.GetCallSignature(this));
        }

        public override object CacheIdentity {
            get { return this; }
        }

        #region IPythonSite Members

        public BinderState Binder {
            get { return _fallback.Binder; }
        }

        #endregion
    }

}
