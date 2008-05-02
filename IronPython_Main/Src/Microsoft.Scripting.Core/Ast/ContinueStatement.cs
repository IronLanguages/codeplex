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
    /// Breaks to the beginning of the the target LabeledStatement
    /// </summary>
    public sealed class ContinueStatement : Expression {
        private readonly LabelTarget/*!*/ _target;

        internal ContinueStatement(Annotations annotations, LabelTarget target)
            : base(annotations, AstNodeType.ContinueStatement, typeof(void)) {
            _target = target;
        }

        public LabelTarget Target {
            get { return _target; }
        }
    }

    public partial class Expression {
        public static ContinueStatement Continue(LabelTarget target) {
            return Continue(SourceSpan.None, target);
        }
        public static ContinueStatement Continue(SourceSpan span, LabelTarget target) {
            return new ContinueStatement(Annotate(span), target);
        }
        public static ContinueStatement Continue(Annotations annotations, LabelTarget target) {
            ContractUtils.RequiresNotNull(target, "target");
            return new ContinueStatement(annotations, target);
        }
    }
}
