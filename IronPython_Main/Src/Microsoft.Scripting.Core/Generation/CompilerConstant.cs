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
using Microsoft.Scripting.Ast;

namespace Microsoft.Scripting.Generation {
    /// <summary>
    /// Represents a constant value created during compilation.  
    /// </summary>
    public abstract class CompilerConstant {
        protected CompilerConstant() {
        }

        /// <summary>
        /// Gets the type of the constant.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
        public abstract Type Type { get; }

        /// <summary>
        /// Emits creation of the constant value into the Compiler object provided.
        /// </summary>
        /// <param name="il"></param>
        public abstract void EmitCreation(ILGen il);

        /// <summary>
        /// Creates a new instance of the constant and returns it.
        /// </summary>
        public abstract object Create();

        /// <summary>
        /// Gets a friendly name of the constant.  
        /// </summary>
        public virtual string Name {
            get {
                return GetType().Name;
            }
        }
    }
}
