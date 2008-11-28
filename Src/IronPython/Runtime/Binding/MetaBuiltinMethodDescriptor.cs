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
using Microsoft.Linq.Expressions;
using Microsoft.Scripting;

using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

using Ast = Microsoft.Linq.Expressions.Expression;

namespace IronPython.Runtime.Binding {

    class MetaBuiltinMethodDescriptor : MetaPythonObject, IPythonInvokable {
        public MetaBuiltinMethodDescriptor(Expression/*!*/ expression, Restrictions/*!*/ restrictions, BuiltinMethodDescriptor/*!*/ value)
            : base(expression, Restrictions.Empty, value) {
            Assert.NotNull(value);
        }

        #region IPythonInvokable Members

        public MetaObject/*!*/ Invoke(PythonInvokeBinder/*!*/ pythonInvoke, Expression/*!*/ codeContext, MetaObject/*!*/ target, MetaObject/*!*/[]/*!*/ args) {
            return InvokeWorker(pythonInvoke, codeContext, args);
        }

        #endregion

        #region MetaObject Overrides

        public override MetaObject/*!*/ BindInvokeMember(InvokeMemberBinder/*!*/ action, MetaObject/*!*/[]/*!*/ args) {
            return BindingHelpers.GenericCall(action, this, args);
        }

        public override MetaObject/*!*/ BindInvoke(InvokeBinder/*!*/ call, params MetaObject/*!*/[]/*!*/ args) {
            // TODO: Context should come from BuiltinFunction
            return InvokeWorker(call, BinderState.GetCodeContext(call), args);
        }

        [Obsolete]
        public override MetaObject BindOperation(OperationBinder action, MetaObject[] args) {
            switch (action.Operation) {
                case StandardOperators.CallSignatures:
                    return PythonProtocol.MakeCallSignatureOperation(this, Value.Template.Targets);
            }

            return base.BindOperation(action, args);
        }

        #endregion

        #region Invoke Implementation

        private MetaObject/*!*/ InvokeWorker(MetaObjectBinder/*!*/ call, Expression/*!*/ codeContext, MetaObject/*!*/[] args) {
            CallSignature signature = BindingHelpers.GetCallSignature(call);
            Restrictions selfRestrict = Restrictions.GetInstanceRestriction(Expression, Value).Merge(Restrictions);

            selfRestrict = selfRestrict.Merge(
                Restrictions.GetExpressionRestriction(
                    MakeFunctionTest(
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
                new ParameterBinderWithCodeContext(state.Binder, codeContext),
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
            } else if (Value.Template.IsBinaryOperator && args.Length == 2 && res.Expression.NodeType == ExpressionType.Throw) {
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

        internal Expression MakeFunctionTest(Expression functionTarget) {
            return Ast.Equal(
                functionTarget,
                Ast.Constant(Value.Template)
            );
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
