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
    public sealed class IntrinsicExpression : Expression {
        internal IntrinsicExpression(AstNodeType nodeType, Type type)
            : base(nodeType, type) {
        }
    }

    public static partial class Ast {
        public static Expression Environment(Type type) {
            Contract.RequiresNotNull(type, "type");
            return new IntrinsicExpression(AstNodeType.EnvironmentExpression, type);
        }

        public static Expression CodeContext() {
            return new IntrinsicExpression(AstNodeType.CodeContextExpression, typeof(CodeContext));
        }


        public static Expression Params() {
            return new IntrinsicExpression(AstNodeType.ParamsExpression, typeof(object[]));
        }
    }
}
