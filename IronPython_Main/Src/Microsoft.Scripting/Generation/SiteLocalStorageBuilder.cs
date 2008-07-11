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
using System.Linq.Expressions;

namespace Microsoft.Scripting.Generation {
    class SiteLocalStorageBuilder : ArgBuilder {
        private Type _type;

        public SiteLocalStorageBuilder(Type type) {
            _type = type;
        }

        internal override Expression ToExpression(MethodBinderContext context, IList<Expression> parameters) {
            return Expression.Constant(Activator.CreateInstance(_type));
        }

        public override int Priority {
            get { return -1; }
        }
    }
}
