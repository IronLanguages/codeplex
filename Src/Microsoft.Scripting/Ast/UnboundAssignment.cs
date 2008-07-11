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

using System.Scripting;
using System.Linq.Expressions;
using System.Scripting.Runtime;
using System.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    public class UnboundAssignment : Expression {
        private readonly SymbolId _name;
        private readonly Expression _value;

        internal UnboundAssignment(Annotations annotations, SymbolId name, Expression value)
            : base(annotations, ExpressionType.Extension, typeof(object)) {
            _name = name;
            _value = value;
        }

        public SymbolId Name {
            get { return _name; }
        }

        public Expression Value {
            get { return _value; }
        }

        public override bool IsReducible {
            get { return true; }
        }

        public override Expression Reduce() {
            return Expression.Call(
                null,
                typeof(RuntimeHelpers).GetMethod("SetName"),
                Annotations,
                new Expression[] {
                    Expression.CodeContext(), 
                    Expression.Constant(_name),
                    Expression.Convert(_value, typeof(object))
                }
            );
        }
    }

    /// <summary>
    /// Factory methods.
    /// </summary>
    public static partial class Utils {
        public static UnboundAssignment Assign(SymbolId name, Expression value) {
            return Assign(name, value, SourceSpan.None);
        }
        public static UnboundAssignment Assign(SymbolId name, Expression value, SourceSpan span) {
            ContractUtils.Requires(!name.IsEmpty && !name.IsInvalid, "name", "Invalid or empty name is not allowed");
            ContractUtils.RequiresNotNull(value, "value");
            return new UnboundAssignment(Expression.Annotate(span), name, value);
        }
    }
}
