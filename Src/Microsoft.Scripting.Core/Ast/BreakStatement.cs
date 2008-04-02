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
    /// Breaks to the end of the the target LabeledStatement
    /// </summary>
    public sealed class BreakStatement : Expression {
        private readonly LabelTarget/*!*/ _target;

        internal BreakStatement(Annotations annotations, LabelTarget target)
            : base(annotations, AstNodeType.BreakStatement, typeof(void)) {
            _target = target;
        }

        public LabelTarget Target {
            get { return _target; }
        }
    }

    public static partial class Ast {
        public static BreakStatement Break(LabelTarget target) {
            return Break(SourceSpan.None, target);
        }
        public static BreakStatement Break(SourceSpan span, LabelTarget target) {
            return new BreakStatement(Annotations(span), target);
        }
        public static BreakStatement Break(Annotations annotations, LabelTarget target) {
            Contract.RequiresNotNull(target, "target");
            return new BreakStatement(annotations, target);
        }
    }
}
