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
    /// Breaks to the beginning of the the target LabeledStatement
    /// </summary>
    public sealed class ContinueStatement : Expression {
        private readonly LabelTarget _target;

        internal ContinueStatement(Annotations annotations, LabelTarget target)
            : base(ExpressionType.ContinueStatement, typeof(void), annotations) {
            _target = target;
        }

        public LabelTarget Target {
            get { return _target; }
        }
    }

    public partial class Expression {
        public static ContinueStatement Continue(LabelTarget target) {
            return Continue(target, Annotations.Empty);
        }
        public static ContinueStatement Continue(LabelTarget target, Annotations annotations) {
            ContractUtils.RequiresNotNull(target, "target");
            return new ContinueStatement(annotations, target);
        }
    }
}
