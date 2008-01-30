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
using System.Collections.Generic;

using Microsoft.Scripting.Ast;

namespace Microsoft.Scripting.Generation {
    /// <summary>
    /// ArgBuilder provides an argument value used by the MethodBinder.  One ArgBuilder exists for each
    /// physical parameter defined on a method.  
    /// 
    /// Contrast this with ParameterWrapper which represents the logical argument passed to the method.
    /// </summary>
    abstract class ArgBuilder {
        public abstract int Priority {
            get;
        }

        public virtual AbstractValue AbstractBuild(AbstractContext context, IList<AbstractValue> parameters) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Provides the Expression which provides the value to be passed to the argument.
        /// </summary>
        internal abstract Expression ToExpression(MethodBinderContext context, IList<Expression> parameters);

        /// <summary>
        /// Returns the type required for the argument or null if the ArgBuilder
        /// does not consume a type.
        /// </summary>
        public virtual Type Type {
            get {
                return null;
            }
        }

        /// <summary>
        /// Provides an Expression which will update the provided value after a call to the method.  May
        /// return null if no update is required.
        /// </summary>
        internal virtual Expression UpdateFromReturn(MethodBinderContext context, IList<Expression> parameters) {
            return null;
        }

        /// <summary>
        /// If the argument produces a return value (e.g. a ref or out value) this provides
        /// the additional value to be returned.
        /// </summary>
        internal virtual Expression ToReturnExpression(MethodBinderContext context) {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Builds the value of the argument to be passed for a call via reflection.
        /// </summary>
        public abstract object Build(CodeContext context, object[] args);

        /// <summary>
        /// If the argument produces a return value (e.g. a ref or out value) this
        /// provides the additional value to be returned.
        /// This will not be called if the method call throws an exception, and so it should not
        /// be used for cleanup that is required to be done.
        /// </summary>
        /// <param name="callArg">The (potentially updated) value of the byref argument</param>
        /// <param name="args">The original argument list. One element of the list may get updated if it is
        /// being passed as a byref parameter that needs to follow copy-in copy-out semantics</param>
        public virtual void UpdateFromReturn(object callArg, object[] args) { }
    }
}
