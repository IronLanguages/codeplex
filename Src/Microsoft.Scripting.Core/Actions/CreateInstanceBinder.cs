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


using Microsoft.Scripting.Utils;
using Microsoft.Contracts;

namespace Microsoft.Scripting {
    /// <summary>
    /// Represents the create dynamic operation at the call site, providing the binding semantic and the details about the operation.
    /// </summary>
    public abstract class CreateInstanceBinder : DynamicMetaObjectBinder {
        private readonly CallInfo _callInfo;

        /// <summary>
        /// Initializes a new intsance of the <see cref="CreateInstanceBinder" />.
        /// </summary>
        /// <param name="callInfo">The signature of the arguments at the call site.</param>
        protected CreateInstanceBinder(CallInfo callInfo) {
            ContractUtils.RequiresNotNull(callInfo, "callInfo");
            _callInfo = callInfo;
        }

        /// <summary>
        /// Gets the signature of the arguments at the call site.
        /// </summary>
        public CallInfo CallInfo {
            get { return _callInfo; }
        }

        /// <summary>
        /// Performs the binding of the dynamic create operation if the target dynamic object cannot bind.
        /// </summary>
        /// <param name="target">The target of the dynamic create operation.</param>
        /// <param name="args">The arguments of the dynamic create operation.</param>
        /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        public DynamicMetaObject FallbackCreateInstance(DynamicMetaObject target, DynamicMetaObject[] args) {
            return FallbackCreateInstance(target, args, null);
        }

        /// <summary>
        /// When overridden in the derived class, performs the binding of the dynamic create operation if the target dynamic object cannot bind.
        /// </summary>
        /// <param name="target">The target of the dynamic create operation.</param>
        /// <param name="args">The arguments of the dynamic create operation.</param>
        /// <param name="errorSuggestion">The binding result to use if binding fails, or null.</param>
        /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        public abstract DynamicMetaObject FallbackCreateInstance(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion);

        /// <summary>
        /// Performs the binding of the dynamic create operation.
        /// </summary>
        /// <param name="target">The target of the dynamic create operation.</param>
        /// <param name="args">An array of arguments of the dynamic create operation.</param>
        /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        public sealed override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args) {
            ContractUtils.RequiresNotNull(target, "target");
            ContractUtils.RequiresNotNullItems(args, "args");

            return target.BindCreateInstance(this, args);
        }

        // this is a standard DynamicMetaObjectBinder
        internal override sealed bool IsStandardBinder {
            get {
                return true;
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="Object" /> is equal to the current object.
        /// </summary>
        /// <param name="obj">The <see cref="Object" /> to compare with the current object.</param>
        /// <returns>true if the specified System.Object is equal to the current object; otherwise false.</returns>
        [Confined]
        public override bool Equals(object obj) {
            CreateInstanceBinder ca = obj as CreateInstanceBinder;
            return ca != null && ca._callInfo.Equals(_callInfo);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>An <see cref="Int32" /> containing the hash code for this instance.</returns>
        [Confined]
        public override int GetHashCode() {
            return CreateInstanceBinderHash ^ _callInfo.GetHashCode();
        }
    }
}
