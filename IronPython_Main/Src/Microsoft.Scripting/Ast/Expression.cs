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
using System.Globalization;
using System.Collections.Generic;

using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    /// <summary>
    /// Summary description for Expr.
    /// </summary>
    public abstract class Expression : Node {
        private readonly Type /*!*/ _type;

        protected Expression(AstNodeType nodeType, Type type)
            : base(nodeType) {
            _type = type;
        }

        public virtual AbstractValue AbstractEvaluate(AbstractContext context) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Tests to see if this expression is a constant with the given value.
        /// </summary>
        /// <param name="value">The constant value to check for.</param>
        public virtual bool IsConstant(object value) {
            return false;
        }

        public Type Type {
            get { return _type; }
        }
    }
}
