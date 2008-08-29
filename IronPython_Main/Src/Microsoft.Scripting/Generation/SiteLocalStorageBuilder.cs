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
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Generation {
    public sealed class SiteLocalStorageBuilder : ArgBuilder {
        private readonly Type _type;

        public override int Priority {
            get { return -1; }
        }
                
        public SiteLocalStorageBuilder(Type type) {
            ContractUtils.RequiresNotNull(type, "type");
            _type = type;
        }

        internal override Expression ToExpression(MethodBinderContext context, IList<Expression> parameters, bool[] hasBeenUsed) {
            return Expression.Constant(Activator.CreateInstance(_type));
        }
    }
}
