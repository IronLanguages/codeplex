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

using System.Reflection;
using System.Runtime.InteropServices;
using System.Scripting.Actions;
using System.Linq.Expressions;
using System.Scripting.Runtime;
using System.Scripting.Utils;
using Microsoft.Contracts;

namespace System.Scripting.Com {
    /// <summary>
    /// This represents a bound dispmethod on a IDispatch object.
    /// </summary>
    public abstract partial class DispCallable : IDynamicObject {

        private readonly IDispatchObject _dispatch;
        private readonly ComMethodDesc _methodDesc;

        [CLSCompliant(false)]
        protected DispCallable(IDispatchObject dispatch, ComMethodDesc methodDesc) {
            _dispatch = dispatch;
            _methodDesc = methodDesc;
        }

        [Confined]
        public override string/*!*/ ToString() {
            return String.Format("<bound dispmethod {0}>", _methodDesc.Name);
        }

        public IDispatchObject DispatchObject { 
            get { return _dispatch; } 
        }

        public ComMethodDesc ComMethodDesc {
            get { return _methodDesc; }
        }

        internal void UpdateByrefArguments(object[] explicitArgs, object[] argsForCall, VarEnumSelector varEnumSelector) {
            VariantBuilder[] variantBuilders = varEnumSelector.VariantBuilders;
            object[] allArgs = ArrayUtils.Insert<object>(this, explicitArgs);
            for (int i = 0; i < variantBuilders.Length; i++) {
                variantBuilders[i].ArgBuilder.UpdateFromReturn(argsForCall[i], allArgs);
            }
        }

        public object UnoptimizedInvoke(SymbolId[] keywordArgNames, params object[] explicitArgs) {
            try {
                VarEnumSelector varEnumSelector = new VarEnumSelector(typeof(object), explicitArgs);
                object[] allArgs = ArrayUtils.Insert<object>(this, explicitArgs);
                ParameterModifier parameterModifiers;
                object[] argsForCall = varEnumSelector.BuildArguments(allArgs, out parameterModifiers);

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

                    namedParams = SymbolTable.IdsToStrings(keywordArgNames);
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
                        ComRuntimeHelpers.CheckThrowException(comException.ErrorCode, ref excepInfo, UInt32.MaxValue, this);
                    }
                }

                // Unwrap the real (inner) exception and raise it
                throw ExceptionHelpers.UpdateForRethrow(e.InnerException);
            }
        }

        #region IDynamicObject Members

        MetaObject IDynamicObject.GetMetaObject(Expression parameter) {
            return new DispCallableMetaObject(parameter, this);
        }

        #endregion
    }
}

#endif
