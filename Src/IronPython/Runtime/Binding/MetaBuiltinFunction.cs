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
using System.Diagnostics;
using System.Scripting.Actions;
using System.Linq.Expressions;
using System.Scripting.Generation;
using System.Scripting.Utils;

using Microsoft.Scripting.Generation;

using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

namespace IronPython.Runtime.Binding {
    using Ast = System.Linq.Expressions.Expression;

    class MetaBuiltinFunction : MetaPythonObject, IPythonInvokable {
        public MetaBuiltinFunction(Expression/*!*/ expression, Restrictions/*!*/ restrictions, BuiltinFunction/*!*/ value)
            : base(expression, Restrictions.Empty, value) {
            Assert.NotNull(value);
        }

        #region MetaObject Overrides

        public override MetaObject/*!*/ Invoke(InvokeAction/*!*/ call, params MetaObject/*!*/[]/*!*/ args) {
            // TODO: Context should come from BuiltinFunction
            return InvokeWorker(call, BinderState.GetCodeContext(call), args);
        }

        public override MetaObject Convert(ConvertAction/*!*/ conversion, MetaObject/*!*/[]/*!*/ args) {
            if (conversion.ToType.IsSubclassOf(typeof(Delegate))) {
                return MakeDelegateTarget(conversion, conversion.ToType, Restrict(typeof(BuiltinFunction)));
            }
            return conversion.Fallback(args);
        }

        #endregion

        #region IPythonInvokable Members

        public MetaObject/*!*/ Invoke(InvokeBinder/*!*/ pythonInvoke, Expression/*!*/ codeContext, MetaObject/*!*/[]/*!*/ args) {
            return InvokeWorker(pythonInvoke, codeContext, args);
        }

        #endregion

        #region Invoke Implementation

        private MetaObject/*!*/ InvokeWorker(MetaAction/*!*/ call, Expression/*!*/ codeContext, MetaObject/*!*/[]/*!*/ args) {
            for (int i = 0; i < args.Length; i++) {
                if (args[i].NeedsDeferral) {
                    return call.Defer(args);
                }
            }

            args = ArrayUtils.RemoveFirst(args);

            Restrictions selfRestrict = Restrictions.InstanceRestriction(Expression, Value).Merge(Restrictions);

            if (Value.IsReversedOperator) {
                ArrayUtils.SwapLastTwo(args);
            }

            BindingTarget target;
            MetaObject res = BinderState.GetBinderState(call).Binder.CallMethod(
                codeContext,
                Value.Targets,
                args,
                BindingHelpers.GetCallSignature(call),
                selfRestrict,
                PythonNarrowing.None,
                Value.IsBinaryOperator ?
                        PythonNarrowing.BinaryOperator :
                        NarrowingLevel.All,
                Value.Name,
                out target
            );

            if (Value.IsBinaryOperator && args.Length == 2 && res.Expression.NodeType == ExpressionType.ThrowStatement) {
                // Binary Operators return NotImplemented on failure.
                res = new MetaObject(
                    Ast.Property(null, typeof(PythonOps), "NotImplemented"),
                    res.Restrictions
                );
            }

            return res;
        }

        #endregion

        #region Helpers

        public new BuiltinFunction/*!*/ Value {
            get {
                return (BuiltinFunction)base.Value;
            }
        }

        #endregion
    }
}
