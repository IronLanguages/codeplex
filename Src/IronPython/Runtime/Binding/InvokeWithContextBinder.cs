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
using System.Scripting.Actions;
using System.Scripting.Runtime;
using System.Scripting.Utils;
using IronPython.Compiler.Ast;

namespace IronPython.Runtime.Binding {
    using Ast = System.Linq.Expressions.Expression;
    using IronPython.Runtime.Operations;

    /// <summary>
    /// Call node but includes CodeContext as the first argument.  Because CodeContext isn't callable
    /// we'll always immediately land back at Fallback.  We then remove theCodeContext and if it's our
    /// own BuiltinFunction we'll call and flow through the CodeContext.  If it's another IDO we'll
    /// call Call on it w/o the CodeContext.  Finally if it's some other object we'll call the base
    /// implementation (again, w/o the CodeContext) which handles normal .NET binding.
    /// </summary>
    class InvokeWithContextBinder : InvokeBinder, IPythonSite {
        public InvokeWithContextBinder(BinderState/*!*/ binder, CallSignature signature)
            : base(binder, signature) {
        }

        public override MetaObject/*!*/ Fallback(MetaObject/*!*/[]/*!*/ args) {
            const int codeContext = 0, callTarget = 1;

            // we don't have CodeContext if an IDO falls back to us when we ask them to produce the Call
            MetaObject cc = args[codeContext];
            if (cc.HasValue && cc.Value is CodeContext) {
                IInvokableWithContext icc = args[callTarget] as IInvokableWithContext;

                // ask the IDO to provide the call, it might callback to Fallback
                MetaObject[] callargs = ArrayUtils.RemoveFirst(args);

                if (icc != null) {
                    // call it and provide the context
                    return icc.InvokeWithContext(
                        this,
                        cc.Expression,
                        callargs
                    );
                } else if (args[callTarget].IsDynamicObject) {
                    return args[callTarget].Invoke(this, callargs);
                }

                // fallback w/o code context
                args = ArrayUtils.RemoveFirst(args);
            }

            return base.Fallback(args);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public override bool Equals(object obj) {
            InvokeWithContextBinder ob = obj as InvokeWithContextBinder;
            if (ob == null) {
                return false;
            }

            return base.Equals(obj);
        }

        public override string ToString() {
            return "Python InvokeWithContext " + Signature.ToString();
        }

        #region IExpressionSerializable Members

        public override System.Linq.Expressions.Expression CreateExpression() {
            return Ast.Call(
                typeof(PythonOps).GetMethod("MakeInvokeWithContextAction"),
                BindingHelpers.CreateBinderStateExpression(),
                Signature.CreateExpression()
            );
        }

        #endregion
    }
}
