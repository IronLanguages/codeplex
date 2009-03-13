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
    /// Represents the convert dynamic operation at the call site, providing the binding semantic and the details about the operation.
    /// </summary>
    public abstract class ConvertBinder : DynamicMetaObjectBinder {
        private readonly Type _type;
        private readonly bool _explicit;

        /// <summary>
        /// Initializes a new intsance of the <see cref="ConvertBinder" />.
        /// </summary>
        /// <param name="type">The type to convert to.</param>
        /// <param name="explicit">true if the conversion should consider explicit conversions; otherwise, false.</param>
        protected ConvertBinder(Type type, bool @explicit) {
            _type = type;
            _explicit = @explicit;
        }

        /// <summary>
        /// The type to convert to.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
        public Type Type {
            get {
                return _type;
            }
        }

        /// <summary>
        /// Gets the value indicating if the conversion should consider explicit conversions.
        /// </summary>
        public bool Explicit {
            get {
                return _explicit;
            }
        }

        /// <summary>
        /// Performs the binding of the dynamic convert operation if the target dynamic object cannot bind.
        /// </summary>
        /// <param name="target">The target of the dynamic convert operation.</param>
        /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        public DynamicMetaObject FallbackConvert(DynamicMetaObject target) {
            return FallbackConvert(target, null);
        }

        /// <summary>
        /// When overridden in the derived class, performs the binding of the dynamic convert operation if the target dynamic object cannot bind.
        /// </summary>
        /// <param name="target">The target of the dynamic convert operation.</param>
        /// <param name="errorSuggestion">The binding result to use if binding fails, or null.</param>
        /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        public abstract DynamicMetaObject FallbackConvert(DynamicMetaObject target, DynamicMetaObject errorSuggestion);

        /// <summary>
        /// Performs the binding of the dynamic convert operation.
        /// </summary>
        /// <param name="target">The target of the dynamic convert operation.</param>
        /// <param name="args">An array of arguments of the dynamic convert operation.</param>
        /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        public sealed override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args) {
            ContractUtils.RequiresNotNull(target, "target");
            ContractUtils.Requires(args.Length == 0);

            return target.BindConvert(this);
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
            ConvertBinder ca = obj as ConvertBinder;
            return ca != null && ca._type == _type && ca._explicit == _explicit;
        }
        
        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>An <see cref="Int32" /> containing the hash code for this instance.</returns>
        [Confined]
        public override int GetHashCode() {
            return ConvertBinderHash ^ _type.GetHashCode() ^ (_explicit ? 0x8000000 : 0);
        }
    }
}
