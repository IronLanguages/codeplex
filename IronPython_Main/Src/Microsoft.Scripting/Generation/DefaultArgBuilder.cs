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
using System.Reflection;
using System.Linq.Expressions;

namespace Microsoft.Scripting.Generation {
    using Ast = System.Linq.Expressions.Expression;

    /// <summary>
    /// ArgBuilder which provides a default parameter value for a method call.
    /// </summary>
    class DefaultArgBuilder : ArgBuilder {
        private Type _argumentType;
        private object _defaultValue;

        public DefaultArgBuilder(Type argumentType, object defaultValue) {
            this._argumentType = argumentType;
            this._defaultValue = defaultValue;
        }

        public override int Priority {
            get { return 2; }
        }

        internal override Expression ToExpression(MethodBinderContext context, IList<Expression> parameters, bool[] hasBeenUsed) {
            object val = _defaultValue;
            if(val is Missing) {
                val = CompilerHelpers.GetMissingValue(_argumentType);
            }

            if (_argumentType.IsByRef) {
                VariableExpression tmp = context.GetTemporary(_argumentType.GetElementType(), "optRef");
                return Ast.Comma(
                    Ast.Assign(
                        tmp,
                        Ast.Convert(Ast.Constant(val), tmp.Type)
                    ),
                    tmp
                );
            }

            return context.ConvertExpression(Ast.Constant(val), _argumentType);            
        }
    }
}
