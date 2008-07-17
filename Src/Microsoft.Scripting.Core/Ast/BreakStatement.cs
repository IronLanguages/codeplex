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

using System.Scripting.Utils;

namespace System.Linq.Expressions {
    /// <summary>
    /// Breaks to the end of the the target LabeledStatement
    /// </summary>
    public sealed class BreakStatement : Expression {
        private readonly LabelTarget _target;

        internal BreakStatement(Annotations annotations, LabelTarget target)
            : base(annotations, ExpressionType.BreakStatement, typeof(void)) {
            _target = target;
        }

        public LabelTarget Target {
            get { return _target; }
        }
    }

    public partial class Expression {
        public static BreakStatement Break(LabelTarget target) {
            return Break(target, Annotations.Empty);
        }
        public static BreakStatement Break(LabelTarget target, Annotations annotations) {
            ContractUtils.RequiresNotNull(target, "target");
            return new BreakStatement(annotations, target);
        }
    }
}
