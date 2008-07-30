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

using System.Linq.Expressions;
using System.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Ast {
    public class UnboundAssignment : Expression {
        private readonly SymbolId _name;
        private readonly Expression _value;

        internal UnboundAssignment(Annotations annotations, SymbolId name, Expression value)
            : base(typeof(object), true, annotations) {
            _name = name;
            _value = value;
        }

        public SymbolId Name {
            get { return _name; }
        }

        public Expression Value {
            get { return _value; }
        }

        public override Expression Reduce() {
            return Expression.Call(
                null,
                typeof(RuntimeHelpers).GetMethod("SetName"),
                Annotations,
                new Expression[] {
                    Utils.CodeContext(), 
                    AstUtils.Constant(_name),
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
