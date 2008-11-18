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


using System.Collections.Generic;
using Microsoft.Linq.Expressions;
using System.Diagnostics;
using System.Reflection;

namespace Microsoft.Scripting.Actions.Calls {

    // TODO: Move to Pyhton

    /// <summary>
    /// ArgBuilder which provides the CodeContext parameter to a method.
    /// </summary>
    public sealed class ContextArgBuilder : ArgBuilder {
        public ContextArgBuilder(ParameterInfo info) 
            : base(info){
        }

        public override int Priority {
            get { return -1; }
        }

        internal protected override Expression ToExpression(ParameterBinder parameterBinder, IList<Expression> parameters, bool[] hasBeenUsed) {
            return ((ParameterBinderWithCodeContext)parameterBinder).ContextExpression;
        }
    }
}
