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
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    // TODO: Remove! Use ActionExpression instead.
    public sealed class DynamicConversionExpression : Expression {
        private readonly Expression /*!*/ _expression;

        internal DynamicConversionExpression(Expression /*!*/ expression, Type /*!*/ conversion)
            : base(AstNodeType.DynamicConversionExpression, conversion) {
            _expression = expression;
        }

        public Expression Expression {
            get { return _expression; }
        }
    }

    public static partial class Ast {
        public static DynamicConversionExpression DynamicConvert(Expression expression, Type conversion) {
            Contract.RequiresNotNull(expression, "expression");
            Contract.RequiresNotNull(conversion, "conversion");

            return new DynamicConversionExpression(expression, conversion);
        }
    }
}
