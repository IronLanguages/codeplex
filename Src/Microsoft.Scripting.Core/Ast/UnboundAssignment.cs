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
    public sealed class UnboundAssignment : Expression {
        private readonly SymbolId _name;
        private readonly Expression /*!*/ _value;

        internal UnboundAssignment(SymbolId name, Expression /*!*/ value)
            : base(AstNodeType.UnboundAssignment, typeof(object)) {
            _name = name;
            _value = value;
        }

        public SymbolId Name {
            get { return _name; }
        }

        public Expression Value {
            get { return _value; }
        }
    }

    /// <summary>
    /// Factory methods.
    /// </summary>
    public static partial class Ast {
        public static UnboundAssignment Assign(SymbolId name, Expression value) {
            Contract.Requires(!name.IsEmpty && !name.IsInvalid, "name", "Invalid or empty name is not allowed");
            Contract.RequiresNotNull(value, "value");
            return new UnboundAssignment(name, value);
        }
    }
}
