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
using Microsoft.Linq.Expressions;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Runtime;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Ast {
    public class UnboundExpression : Expression {
        private readonly SymbolId _name;

        internal UnboundExpression(Annotations annotations, SymbolId name)
            : base(typeof(object), true, annotations) {
            _name = name;
        }

        public SymbolId Name {
            get { return _name; }
        }

        public override Expression Reduce() {
            return Expression.Call(
                typeof(RuntimeHelpers).GetMethod("LookupName"),
                Utils.CodeContext(),
                AstUtils.Constant(_name)
            );
        }

        protected override Expression VisitChildren(ExpressionTreeVisitor visitor) {
            return this;
        }
    }

    /// <summary>
    /// Factory methods
    /// </summary>
    public static partial class Utils {
        public static UnboundExpression Read(SymbolId name) {
            return Read(name, Annotations.Empty);
        }
        public static UnboundExpression Read(SymbolId name, Annotations annotations) {
            ContractUtils.Requires(!name.IsInvalid && !name.IsEmpty, "name", "Invalid or empty name is not allowed");
            return new UnboundExpression(annotations, name);
        }
    }
}
