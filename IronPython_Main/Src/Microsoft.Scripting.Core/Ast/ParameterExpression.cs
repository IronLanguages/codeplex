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
    public sealed class ParameterExpression : Expression {
        private readonly string _name;

        internal ParameterExpression(Type type, string name)
            : base(AstNodeType.Parameter, type) {
            _name = name;
        }

        public string Name {
            get { return _name; }
        }
    }

    public static partial class Ast {
        public static ParameterExpression Parameter(Type type, string name) {
            Contract.Requires(type != typeof(void));
            return new ParameterExpression(type, name);
        }
    }
}
