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
using System.Diagnostics;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using ComTypes = System.Runtime.InteropServices.ComTypes;
using System.Threading;

using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Contracts;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Actions.ComDispatch {
    using Ast = Microsoft.Scripting.Ast.Ast;

    /// <summary>
    /// This represents a bound dispmethod on a IDispatch object.
    /// </summary>
    public class DispCallable : IDynamicObject {
        private readonly IDispatchObject _dispatch;
        private readonly ComDispatch.ComMethodDesc _methodDesc;

        [CLSCompliant(false)]
        public DispCallable(IDispatchObject dispatch, ComMethodDesc methodDesc) {
            _dispatch = dispatch;
            _methodDesc = methodDesc;
        }

        [Confined]
        public override string/*!*/ ToString() {
            return String.Format("<bound dispmethod {0}>", _methodDesc.Name);
        }

        public IDispatchObject DispatchObject { get { return _dispatch; } }
        public ComMethodDesc ComMethodDesc { get { return _methodDesc; } }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private object[] GetArgsForCall(CodeContext context, object[] originalArgs, out ParameterModifier parameterModifiers) {

            int countOfArgsForCall = originalArgs.Length;

            VarEnumSelector varEnumSelector = new VarEnumSelector(context.LanguageContext.Binder, originalArgs);
            originalArgs = varEnumSelector.ConvertArguments();

            ComDispatch.ComParamDesc[] paramsDesc = _methodDesc.Parameters;

            // TODO: Redo this code
            if (paramsDesc != null) {
                for (int i = countOfArgsForCall; i < paramsDesc.Length; i++) {
                    // Find number of required user arguments (some of which may be out params)
                    if (paramsDesc[i].IsOptional == false) {
                        countOfArgsForCall = i + 1;
                    }
                }
            }

            if (countOfArgsForCall == 0) {
                parameterModifiers = new ParameterModifier();
                return originalArgs;
            }

            object[] argsForCall = new object[countOfArgsForCall];

            parameterModifiers = new ParameterModifier(countOfArgsForCall);

            // TODO: we should throw exception when number of expected params
            // TODO: is smaller than the number of parameters passed by user to
            // TODO: the call.

            for (int i = 0; i < countOfArgsForCall; i++) {
                if (i < originalArgs.Length) {
                    object arg = originalArgs[i];
                    if (arg is IStrongBox) {
                        argsForCall[i] = (arg as IStrongBox).Value;
                        parameterModifiers[i] = true;
                    } else {
                        argsForCall[i] = arg;
                        if (paramsDesc != null && i < paramsDesc.Length && paramsDesc[i].ByReference) {
                            parameterModifiers[i] = true;
                        }
                    }
                } else {
                    // we need to fill in some params that were not 
                    // provided by the user.
                    // We will fill in parameters according to what
                    // we know from its ITypeInfo description.
                    if (paramsDesc[i].IsOptional) {
                        argsForCall[i] = System.Type.Missing;
                    } else if (paramsDesc[i].IsOut) {
                        argsForCall[i] = Activator.CreateInstance(paramsDesc[i].ParameterType);
                        parameterModifiers[i] = true;
                    }
                }
            }

            return argsForCall;
        }

        public static void UpdateByrefArguments(object[] originalArgs, object[] argsForCall, ParameterModifier parameterModifiers) {

            for (int i = 0; i < originalArgs.Length; i++) {
                if (parameterModifiers[i]) {
                    IStrongBox strongBox = originalArgs[i] as IStrongBox;
                    if (strongBox != null) {
                        strongBox.Value = argsForCall[i];
                    }
                }
            }
        }

        public object UnoptimizedInvoke(CodeContext context, params object[] args) {
            try {
                ParameterModifier parameterModifiers;

                object[] argsForCall = GetArgsForCall(context, args, out parameterModifiers);

                BindingFlags bindingFlags = BindingFlags.Instance;
                if (_methodDesc.IsPropertyGet) {
                    bindingFlags |= BindingFlags.GetProperty;
                } else if (_methodDesc.IsPropertyPut) {
                    bindingFlags |= BindingFlags.SetProperty;
                } else {
                    bindingFlags |= BindingFlags.InvokeMethod;
                }

                // We use Type.InvokeMember instead of IDispatch.Invoke so that we do not
                // have to worry about marshalling the arguments. Type.InvokeMember will use
                // IDispatch.Invoke under the hood.
                object retVal = _dispatch.DispatchObject.GetType().InvokeMember(
                    _methodDesc.DispIdOrName,
                    bindingFlags,
                    Type.DefaultBinder,
                    _dispatch.DispatchObject,
                    argsForCall,
                    new ParameterModifier[] { parameterModifiers },
                    null,
                    null
                    );

                UpdateByrefArguments(args, argsForCall, parameterModifiers);

                return retVal;
            } catch (TargetInvocationException e) {
                // Unwrap the real (inner) exception and raise it
                throw ExceptionHelpers.UpdateForRethrow(e.InnerException);
            }
        }

        #region IDynamicObject Members

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")] // TODO: fix
        public LanguageContext LanguageContext {
            get { throw new NotImplementedException(); }
        }

        StandardRule<T> IDynamicObject.GetRule<T>(DynamicAction action, CodeContext context, object[] args) {
            switch (action.Kind) {
                case DynamicActionKind.Call: return MakeCallRule<T>((CallAction)action, context, args);
                case DynamicActionKind.DoOperation: return MakeDoOperationRule<T>((DoOperationAction)action, context, args);
            }
            return null;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "context")] // TODO: fix
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "args")] // TODO: fix
        private StandardRule<T> MakeDoOperationRule<T>(DoOperationAction doOperationAction, CodeContext context, object[] args) {
            switch (doOperationAction.Operation) {
                case Operators.CallSignatures:
                case Operators.Documentation:
                    return MakeDocumentationRule<T>(context);

                case Operators.IsCallable:
                    return BinderHelper.MakeIsCallableRule<T>(context, this, true);
            }
            return null;
        }

        private StandardRule<T> MakeDocumentationRule<T>(CodeContext context) {
            StandardRule<T> rule = new StandardRule<T>();
            rule.MakeTest(CompilerHelpers.GetType(this));
            // return this.ComMethodDesc.Signature
            rule.SetTarget(
                rule.MakeReturn(
                    context.LanguageContext.Binder,
                    Ast.ReadProperty(
                        Ast.ReadProperty(
                            Ast.ConvertHelper(rule.Parameters[0], typeof(DispCallable)),
                            typeof(DispCallable).GetProperty("ComMethodDesc")),
                        typeof(ComMethodDesc).GetProperty("Signature"))));
            return rule;
        }

        private StandardRule<T> MakeCallRule<T>(CallAction action, CodeContext context, object[] currentInstanceAndArgs) {
            IDispatchCallBinderHelper<T> helper = new IDispatchCallBinderHelper<T>(context, action, currentInstanceAndArgs);
            return helper.MakeRule();
        }

        #endregion
    }
}

#endif