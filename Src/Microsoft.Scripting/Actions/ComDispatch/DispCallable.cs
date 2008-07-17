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

#if !SILVERLIGHT // ComObject

using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Scripting.Actions;
using System.Scripting.Generation;
using System.Scripting.Runtime;
using System.Scripting.Utils;
using Microsoft.Contracts;

namespace Microsoft.Scripting.Actions.ComDispatch {
    using Ast = System.Linq.Expressions.Expression;

    /// <summary>
    /// This represents a bound dispmethod on a IDispatch object.
    /// </summary>
    public abstract partial class DispCallable : IOldDynamicObject {

        private readonly IDispatchObject _dispatch;
        private readonly ComMethodDesc _methodDesc;

        internal DispCallable(IDispatchObject dispatch, ComMethodDesc methodDesc) {
            _dispatch = dispatch;
            _methodDesc = methodDesc;
        }

        [Confined]
        public override string ToString() {
            return String.Format("<bound dispmethod {0}>", _methodDesc.Name);
        }

        public IDispatchObject DispatchObject {
            get { return _dispatch; }
        }

        public ComMethodDesc ComMethodDesc {
            get { return _methodDesc; }
        }

        private void UpdateByrefArguments(object[] explicitArgs, object[] argsForCall, VarEnumSelector varEnumSelector) {
            VariantBuilder[] variantBuilders = varEnumSelector.VariantBuilders;
            object[] allArgs = ArrayUtils.Insert<object>(this, explicitArgs);
            for (int i = 0; i < variantBuilders.Length; i++) {
                variantBuilders[i].ArgBuilder.UpdateFromReturn(argsForCall[i], allArgs);
            }
        }

        public object UnoptimizedInvoke(CodeContext context, string[] keywordArgNames, params object[] explicitArgs) {
            try {
                VarEnumSelector varEnumSelector = new VarEnumSelector(context.LanguageContext.Binder, typeof(object), explicitArgs);
                object[] allArgs = ArrayUtils.Insert<object>(this, explicitArgs);
                ParameterModifier parameterModifiers;
                object[] argsForCall = varEnumSelector.BuildArguments(context, allArgs, out parameterModifiers);

                BindingFlags bindingFlags = BindingFlags.Instance;
                if (_methodDesc.IsPropertyGet) {
                    bindingFlags |= BindingFlags.GetProperty;
                } else if (_methodDesc.IsPropertyPut) {
                    bindingFlags |= BindingFlags.SetProperty;
                } else {
                    bindingFlags |= BindingFlags.InvokeMethod;
                }

                string memberName = _methodDesc.DispIdString; // Use the "[DISPID=N]" format to avoid a call to GetIDsOfNames
                string[] namedParams = null;
                if (keywordArgNames.Length > 0) {
                    // InvokeMember does not allow the method name to be in "[DISPID=N]" format if there are namedParams
                    memberName = _methodDesc.Name;

                    namedParams = keywordArgNames;
                    argsForCall = ArrayUtils.RotateRight(argsForCall, namedParams.Length);
                }

                // We use Type.InvokeMember instead of IDispatch.Invoke so that we do not
                // have to worry about marshalling the arguments. Type.InvokeMember will use
                // IDispatch.Invoke under the hood.
                object retVal = _dispatch.DispatchObject.GetType().InvokeMember(
                    memberName,
                    bindingFlags,
                    Type.DefaultBinder,
                    _dispatch.DispatchObject,
                    argsForCall,
                    new ParameterModifier[] { parameterModifiers },
                    null,
                    namedParams
                    );

                UpdateByrefArguments(explicitArgs, argsForCall, varEnumSelector);

                return retVal;
            } catch (TargetInvocationException e) {
                COMException comException = e.InnerException as COMException;
                if (comException != null) {
                    int errorCode = comException.ErrorCode;
                    if (errorCode > ComHresults.DISP_E_UNKNOWNINTERFACE && errorCode < ComHresults.DISP_E_PARAMNOTOPTIONAL) {
                        // If the current exception was caused because of a DISP_E_* errorcode, call
                        // ComRuntimeHelpers.CheckThrowException which handles these errorcodes specially. This ensures
                        // that we preserve identical behavior in both cases.
                        ExcepInfo excepInfo = new ExcepInfo();
                        ComRuntimeHelpers.CheckThrowException(comException.ErrorCode, ref excepInfo, UInt32.MaxValue, ComMethodDesc.Name);
                    }
                }

                // Unwrap the real (inner) exception and raise it
                throw ExceptionHelpers.UpdateForRethrow(e.InnerException);
            }
        }

        #region IOldDynamicObject Members

        RuleBuilder<T> IOldDynamicObject.GetRule<T>(OldDynamicAction action, CodeContext context, object[] args) {
            switch (action.Kind) {
                case DynamicActionKind.Call:
                    return MakeCallRule<T>((OldCallAction)action, context, args);

                case DynamicActionKind.DoOperation:
                    return MakeDoOperationRule<T>((OldDoOperationAction)action, context);
            }
            return null;
        }

        private RuleBuilder<T> MakeDoOperationRule<T>(OldDoOperationAction doOperationAction, CodeContext context) where T : class {
            switch (doOperationAction.Operation) {
                case Operators.CallSignatures:
                case Operators.Documentation:
                    return MakeDocumentationRule<T>(context);

                case Operators.IsCallable:
                    return MakeIsCallableRule<T>(context, true);
            }
            return null;
        }

        // This can produce a IsCallable rule that returns the immutable constant isCallable.
        // Beware that objects can have a mutable callable property.
        private RuleBuilder<T> MakeIsCallableRule<T>(CodeContext context, bool isCallable) where T : class {
            RuleBuilder<T> rule = new RuleBuilder<T>();
            rule.MakeTest(this.GetType());
            rule.Target =
                rule.MakeReturn(
                    context.LanguageContext.Binder,
                    Expression.Constant(isCallable)
                );

            return rule;
        }

        private RuleBuilder<T> MakeDocumentationRule<T>(CodeContext context) where T : class {
            RuleBuilder<T> rule = new RuleBuilder<T>();
            rule.MakeTest(CompilerHelpers.GetType(this));
            // return this.ComMethodDesc.Signature
            rule.Target =
                rule.MakeReturn(
                    context.LanguageContext.Binder,
                    Ast.Property(
                        Ast.Property(
                            Ast.ConvertHelper(rule.Parameters[0], typeof(DispCallable)),
                            typeof(DispCallable).GetProperty("ComMethodDesc")),
                        typeof(ComMethodDesc).GetProperty("Signature")));
            return rule;
        }

        private RuleBuilder<T> MakeCallRule<T>(OldCallAction action, CodeContext context, object[] args) where T : class {
            RuleBuilder<T> builder = new RuleBuilder<T>();
            Expression test, body;

            new CallBinder<T>(context, action, args, _methodDesc, builder.Context, builder.Parameters).Bind(out test, out body);

            builder.Test = test;
            builder.Target = body;
            return builder;
        }

        #endregion
    }
}

#endif
