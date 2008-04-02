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

namespace Microsoft.Scripting.Ast {
    /// <summary>
    /// Represents a labeled statement
    /// break and continue statements will jump to the end of body
    /// </summary>
    public sealed class LabeledStatement : Expression {
        private readonly Expression/*!*/ _expression;
        private readonly LabelTarget/*!*/ _label;

        internal LabeledStatement(Annotations annotations, LabelTarget label, Expression expression)
            : base(annotations, AstNodeType.LabeledStatement, typeof(void)) {
            _label = label;
            _expression = expression;
        }

        public LabelTarget Label {
            get { return _label; }
        }

        public Expression Statement {
            get { return _expression; }
        }

    }

    public static partial class Ast {
        public static LabeledStatement Labeled(LabelTarget label, Expression body) {
            return Labeled(SourceSpan.None, label, body);
        }

        public static LabeledStatement Labeled(SourceSpan span, LabelTarget label, Expression body) {
            return Labeled(Annotations(span, span), label, body);
        }

        public static LabeledStatement Labeled(Annotations annotations, LabelTarget label, Expression body) {
            Contract.RequiresNotNull(label, "label");
            Contract.RequiresNotNull(body, "body");
            return new LabeledStatement(annotations, label, body);
        }
    }
}
