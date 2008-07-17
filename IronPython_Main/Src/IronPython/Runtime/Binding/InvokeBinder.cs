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
using System.Linq.Expressions;
using System.Scripting.Actions;

using IronPython.Runtime.Binding;

namespace IronPython.Runtime.Binding {
    using Ast = System.Linq.Expressions.Expression;
    using IronPython.Runtime.Operations;

    class InvokeBinder : InvokeAction, IPythonSite, IExpressionSerializable {
        private readonly BinderState/*!*/ _state;
        private readonly CallSignature _signature;

        public InvokeBinder(BinderState/*!*/ binder, CallSignature signature)
            : base(BindingHelpers.GetArguments(signature)) {
            _state = binder;
            _signature = signature;
        }

        public override MetaObject/*!*/ Fallback(MetaObject/*!*/[]/*!*/ args) {
            if (args[0].NeedsDeferral) {
                return Defer(args);
            }

            if (args[0].IsDynamicObject) {
                // try creating an instance...
                return args[0].Create(
                    new CreateFallback(this, Signature),
                    args
                );                    
            }

            return InvokeFallback(args);
        }

        private MetaObject InvokeFallback(MetaObject/*!*/[] args) {
            return PythonProtocol.Call(this, args) ??
                    Binder.Binder.Create(Signature, BindingHelpers.GetSiteCodeContext(), args) ??
                   Binder.Binder.Call(Signature, BindingHelpers.GetSiteCodeContext(), args);
        }

        public CallSignature Signature {
            get {
                return _signature;
            }
        }

        public BinderState Binder {
            get {
                return _state;
            }
        }

        public override object HashCookie {
            get { return this; }
        }

        public override int GetHashCode() {
            return base.GetHashCode() ^ _state.Binder.GetHashCode();
        }

        public override bool Equals(object obj) {
            InvokeBinder ob = obj as InvokeBinder;
            if (ob == null) {
                return false;
            }

            return ob._state.Binder == _state.Binder && base.Equals(obj) ;
        }

        public override string ToString() {
            return "Python Invoke " + Signature.ToString();
        }

        class CreateFallback : CreateAction {
            private readonly InvokeBinder/*!*/ _fallback;
            public CreateFallback(InvokeBinder realFallback, CallSignature signature)
                : base(BindingHelpers.GetArguments(signature)) {
                _fallback = realFallback;
            }

            public override MetaObject Fallback(MetaObject[] args) {                
                return _fallback.InvokeFallback(args);
            }

            public override object HashCookie {
                get { return this; }
            }
        }

        #region IExpressionSerializable Members

        public virtual Expression CreateExpression() {
            return Ast.Call(
                typeof(PythonOps).GetMethod("MakeInvokeAction"),
                BindingHelpers.CreateBinderStateExpression(),
                Signature.CreateExpression()
            );
        }

        #endregion
    }
}
