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
    using Ast = System.Linq.Expressions.Expression;

    /// <summary>
    /// Builds a parameter for a reference argument when a StrongBox has not been provided.  The
    /// updated return value is returned as one of the resulting return values.
    /// </summary>
    class ReturnReferenceArgBuilder : SimpleArgBuilder {
        VariableExpression _tmp;

        public ReturnReferenceArgBuilder(int index, Type type)
            : base(index, type) {
        }

        internal override Expression ToExpression(MethodBinderContext context, IList<Expression> parameters) {
            if (_tmp == null) {
                _tmp = context.GetTemporary(Type, "outParam");
            }

            return Ast.Comma(Ast.Assign(_tmp, base.ToExpression(context, parameters)), _tmp);
        }

        internal override Expression ToReturnExpression(MethodBinderContext context) {
            return _tmp;
        }

        public override int Priority {
            get {
                return 5;
            }
        }
    }
}
