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

using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Ast {
    public class DeleteUnboundExpression : Expression {
        private readonly SymbolId _name;

        internal DeleteUnboundExpression(Annotations annotations, SymbolId name)
            : base(annotations, AstNodeType.Extension, typeof(object)) {
            _name = name;
        }

        public SymbolId Name {
            get { return _name; }
        }

        public override bool IsReducible {
            get { return true; }
        }

        public override Expression Reduce() {
            return Expression.Call(
                Annotations,
                null,
                typeof(RuntimeHelpers).GetMethod("RemoveName"),
                Expression.CodeContext(), 
                Expression.Constant(_name)
            );
        }
    }

    public static partial class Utils {
        public static DeleteUnboundExpression Delete(SymbolId name) {
            return Delete(SourceSpan.None, name);
        }
        public static DeleteUnboundExpression Delete(SourceSpan span, SymbolId name) {
            ContractUtils.Requires(!name.IsInvalid && !name.IsEmpty, "name");
            return new DeleteUnboundExpression(Expression.Annotate(span), name);
        }
    }
}
