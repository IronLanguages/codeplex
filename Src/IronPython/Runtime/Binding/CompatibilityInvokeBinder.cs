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
using System.Diagnostics;
using System.Linq.Expressions;
using System.Scripting.Actions;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime.Operations;

using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronPython.Runtime.Binding {
    /// <summary>
    /// Fallback action for performing an invoke from Python.  We translate the
    /// CallSignature which supports splatting position and keyword args into
    /// their expanded form.
    /// </summary>
    class CompatibilityInvokeBinder : InvokeAction, IPythonSite {
        private readonly BinderState/*!*/ _state;

        public CompatibilityInvokeBinder(BinderState/*!*/ state, params Argument/*!*/[]/*!*/ args)
            : base(args) {
            _state = state;
        }

        public override MetaObject/*!*/ Fallback(MetaObject/*!*/[]/*!*/ args, MetaObject onBindingError) {
            if (args[0].IsDynamicObject) {
                // try creating an instance...
                return args[0].Create(
                    new CreateFallback(this, Arguments),
                    args
                );
            }

            return InvokeFallback(args, BindingHelpers.ArgumentArrayToSignature(Arguments));
        }

        internal MetaObject/*!*/ InvokeFallback(MetaObject/*!*/[]/*!*/ args, CallSignature sig) {
            return PythonProtocol.Call(this, args) ??
               Binder.Binder.Create(sig, Expression.Constant(_state.Context), args) ??
               Binder.Binder.Call(sig, Expression.Constant(_state.Context), args);
        }

        public override int GetHashCode() {
            return base.GetHashCode() ^ _state.Binder.GetHashCode();
        }

        public override bool Equals(object obj) {
            CompatibilityInvokeBinder ob = obj as CompatibilityInvokeBinder;
            if (ob == null) {
                return false;
            }

            return ob._state.Binder == _state.Binder && base.Equals(obj);
        }

        public override object/*!*/ HashCookie {
            get { return this; }
        }

        #region IPythonSite Members

        public BinderState/*!*/ Binder {
            get { return _state; }
        }

        #endregion
    }
}
