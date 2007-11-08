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
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting;
using System.Reflection;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Types.ComDispatch {
        /// <summary>
        /// This represents a bound dispmethod on a IDispatch object.
        /// </summary>
        public class DispCallable {
            private readonly IDispatch _dispatch;
            private readonly ComDispatch.ComMethodDesc _methodDesc;

            internal DispCallable(IDispatch dispatch, ComDispatch.ComMethodDesc methodDesc) {
                _dispatch = dispatch;
                _methodDesc = methodDesc;
            }

            public override string ToString() {
                return String.Format("<bound dispmethod {0}>", _methodDesc.Name);
            }

            private object[] GetArgsForCall(object[] originalArgs, out ParameterModifier parameterModifiers) {

                int countOfArgs = originalArgs.Length;

                ComDispatch.ComParamDesc [] paramsDesc = _methodDesc.Parameters;

                if (paramsDesc != null) {
                    for (int i = countOfArgs; i < paramsDesc.Length; i++) {
                        if (paramsDesc[i].IsOptional == false) {
                            countOfArgs = i + 1;
                        }
                    }
                }

                if (countOfArgs == 0) {
                    parameterModifiers = new ParameterModifier();
                    return originalArgs;
                }

                object[] argsForCall = new object[countOfArgs];

                parameterModifiers = new ParameterModifier(countOfArgs);
                for (int i = 0; i < countOfArgs; i++) {
                    if (i < originalArgs.Length) {
                        object arg = originalArgs[i];
                        if (arg is IStrongBox) {
                            argsForCall[i] = (arg as IStrongBox).Value;
                            parameterModifiers[i] = true;
                        } 
                        else {
                            argsForCall[i] = arg;
                            if (paramsDesc != null && paramsDesc[i].ByReference) {
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
                            argsForCall[i] = Activator.CreateInstance(paramsDesc[i].GetParamType());
                            parameterModifiers[i] = true;
                        }
                    }
                }

                return argsForCall;
            }

            static void UpdateByrefArguments(object[] originalArgs, object[] argsForCall, ParameterModifier parameterModifiers) {

                for (int i = 0; i < originalArgs.Length; i++) {
                    if (parameterModifiers[i]) {
                        IStrongBox strongBox = originalArgs[i] as IStrongBox;
                        if (strongBox != null) {
                            strongBox.Value = argsForCall[i];
                        }
                    }
                }
            }

            protected object Call(object[] args) {
                try {
                    ParameterModifier parameterModifiers;

                    object[] argsForCall = GetArgsForCall(args, out parameterModifiers);


                    BindingFlags bindingFlags = BindingFlags.Instance;
                    if (_methodDesc.IsPropertyGet)
                        bindingFlags |= BindingFlags.GetProperty;
                    else if (_methodDesc.IsPropertyPut)
                        bindingFlags |= BindingFlags.SetProperty;
                    else
                        bindingFlags |= BindingFlags.InvokeMethod;

                    // We use Type.InvokeMember instead of IDispatch.Invoke so that we do not
                    // have to worry about marshalling the arguments. Type.InvokeMember will use
                    // IDispatch.Invoke under the hood.
                    object retVal = _dispatch.GetType().InvokeMember(
                        _methodDesc.DispIdOrName,
                        bindingFlags,
                        Type.DefaultBinder,
                        _dispatch,
                        argsForCall,
                        new ParameterModifier[] { parameterModifiers },
                        null,
                        null
                        );

                    UpdateByrefArguments(args, argsForCall, parameterModifiers);

                    return retVal;
                } catch (Exception e) {
                    if (e.InnerException != null) {
                        throw ExceptionHelpers.UpdateForRethrow(e.InnerException);
                    }
                    throw;
                }
            }
        }
}
#endif
