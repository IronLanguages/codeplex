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

using System.Linq.Expressions;
using System.Scripting.Actions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;
using Ast = System.Linq.Expressions.Expression;

namespace IronPython.Runtime.Binding {

    class MetaBuiltinMethodDescriptor : MetaPythonObject, IPythonInvokable {
        public MetaBuiltinMethodDescriptor(Expression/*!*/ expression, Restrictions/*!*/ restrictions, BuiltinMethodDescriptor/*!*/ value)
            : base(expression, Restrictions.Empty, value) {
            Assert.NotNull(value);
        }

        #region IPythonInvokable Members

        public MetaObject/*!*/ Invoke(InvokeBinder/*!*/ pythonInvoke, Expression/*!*/ codeContext, MetaObject/*!*/[]/*!*/ args) {
            return InvokeWorker(pythonInvoke, codeContext, args);
        }

        #endregion

        #region MetaObject Overrides

        public override MetaObject/*!*/ Call(CallAction/*!*/ action, MetaObject/*!*/[]/*!*/ args) {
            return BindingHelpers.GenericCall(action, args);
        }

        public override MetaObject/*!*/ Invoke(InvokeAction/*!*/ call, params MetaObject/*!*/[]/*!*/ args) {
            // TODO: Context should come from BuiltinFunction
            return InvokeWorker(call, BinderState.GetCodeContext(call), args);
        }

        #endregion

        #region Invoke Implementation

        private MetaObject/*!*/ InvokeWorker(MetaAction/*!*/ call, Expression/*!*/ codeContext, MetaObject/*!*/[] args) {
            args = ArrayUtils.RemoveFirst(args);

            CallSignature signature = BindingHelpers.GetCallSignature(call);
            Restrictions selfRestrict = Restrictions.InstanceRestriction(Expression, Value).Merge(Restrictions);

            selfRestrict = selfRestrict.Merge(
                Restrictions.ExpressionRestriction(
                    Value.Template.MakeFunctionTest(
                        Ast.Call(
                            typeof(PythonOps).GetMethod("GetBuiltinMethodDescriptorTemplate"),
                            Ast.Convert(Expression, typeof(BuiltinMethodDescriptor))
                        )
                    )
                )
            );

            if (Value.Template.IsOnlyGeneric) {
                return BindingHelpers.TypeErrorGenericMethod(Value.DeclaringType, Value.Template.Name, selfRestrict);
            }

            if (Value.Template.IsReversedOperator) {
                ArrayUtils.SwapLastTwo(args);
            }

            BindingTarget target;

            BinderState state = BinderState.GetBinderState(call);

            MetaObject res = state.Binder.CallMethod(
                codeContext,
                Value.Template.Targets,
                args,
                signature,
                selfRestrict,
                NarrowingLevel.None,
                Value.Template.IsBinaryOperator ?
                    PythonNarrowing.BinaryOperator :
                    NarrowingLevel.All,
                Value.Template.Name,
                out target
            );


            if (target.Method != null && (target.Method.IsFamily || target.Method.IsFamilyOrAssembly)) {
                res = new MetaObject(
                    BindingHelpers.TypeErrorForProtectedMember(
                        target.Method.DeclaringType,
                        target.Method.Name
                    ),
                    res.Restrictions
                );
            } else if (Value.Template.IsBinaryOperator && args.Length == 2 && res.Expression.NodeType == ExpressionType.ThrowStatement) {
                // Binary Operators return NotImplemented on failure.
                res = new MetaObject(
                    Ast.Property(null, typeof(PythonOps), "NotImplemented"),
                    res.Restrictions
                );
            }

            WarningInfo info;
            if (target.Method != null && BindingWarnings.ShouldWarn(target.Method, out info)) {
                res = info.AddWarning(codeContext, res);
            }

            return res;
        }

        #endregion

        #region Helpers

        public new BuiltinMethodDescriptor/*!*/ Value {
            get {
                return (BuiltinMethodDescriptor)base.Value;
            }
        }

        #endregion
    }
}
