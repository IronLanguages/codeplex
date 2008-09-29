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
#if !SILVERLIGHT

using System.Collections.Generic;
using Microsoft.Linq.Expressions;

namespace Microsoft.Scripting.Com {
    /// <summary>
    /// ArgBuilder provides an argument value used by the MethodBinder.  One ArgBuilder exists for each
    /// physical parameter defined on a method.  
    /// 
    /// Contrast this with ParameterWrapper which represents the logical argument passed to the method.
    /// </summary>
    internal abstract class ArgBuilder {

        internal virtual VariableExpression[] TemporaryVariables {
            get {
                return new VariableExpression[0];
            }
        }

        /// <summary>
        /// Provides the Expression which provides the value to be passed to the argument.
        /// </summary>
        internal abstract Expression Build(Expression parameter);

        /// <summary>
        /// Builds the value of the argument to be passed for a call via reflection.
        /// </summary>
        internal abstract object Build(object arg);

        /// <summary>
        /// Provides an Expression which will update the provided value after a call to the method.  May
        /// return null if no update is required.
        /// </summary>
        internal virtual Expression UpdateFromReturn(Expression parameter) {
            return null;
        }

        /// <summary>
        /// If the argument produces a return value (e.g. a ref or out value) this
        /// provides the additional value to be returned.
        /// This will not be called if the method call throws an exception, and so it should not
        /// be used for cleanup that is required to be done.
        /// </summary>
        internal virtual void UpdateFromReturn(object originalArg, object updatedArg) { }
    }
}

#endif
