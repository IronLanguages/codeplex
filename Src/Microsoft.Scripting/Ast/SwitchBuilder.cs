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

using System.Collections.Generic;
using System.Linq.Expressions;
using System.Scripting.Utils;

namespace Microsoft.Scripting.Ast {

    // TODO: remove builders?
    public sealed class SwitchBuilder {
        private readonly Annotations _annotations;
        private Expression _value;
        private readonly List<SwitchCase> _cases = new List<SwitchCase>();
        private bool _default;
        private LabelTarget _label;

        internal SwitchBuilder(Expression value, LabelTarget label, Annotations annotations) {
            _value = value;
            _label = label;
            _annotations = annotations;
        }

        public SwitchBuilder Test(Expression value) {
            ContractUtils.RequiresNotNull(value, "value");
            _value = value;
            return this;
        }

        public SwitchBuilder Default(Expression body) {
            ContractUtils.Requires(_default == false, "body", "Already has default clause");
            _cases.Add(Expression.DefaultCase(body));
            _default = true;
            return this;
        }

        public SwitchBuilder Case(int value, Expression body) {
            _cases.Add(Expression.SwitchCase(value, body));
            return this;
        }

        public Expression ToExpression() {
            ContractUtils.Requires(_value != null);
            return Expression.Switch(_value, _label, _annotations, _cases);
        }

        public static implicit operator Expression(SwitchBuilder builder) {
            ContractUtils.RequiresNotNull(builder, "builder");
            return builder.ToExpression();
        }
    }

    // TODO: do we really need so many overloads?
    // let's use optional args instead
    public partial class Utils {
        public static SwitchBuilder Switch() {
            return Switch(null, null, Annotations.Empty);
        }

        public static SwitchBuilder Switch(Annotations annotations) {
            return Switch(null, null, annotations);
        }

        public static SwitchBuilder Switch(LabelTarget label) {
            return Switch(null, label, Annotations.Empty);
        }

        public static SwitchBuilder Switch(LabelTarget label, Annotations annotations) {
            return Switch(null, label, annotations);
        }

        public static SwitchBuilder Switch(Expression value) {
            return Switch(value, null, Annotations.Empty);
        }

        public static SwitchBuilder Switch(Expression value, Annotations annotations) {
            return Switch(value, null, annotations);
        }

        public static SwitchBuilder Switch(Expression value, LabelTarget label) {
            return Switch(value, label, Annotations.Empty);
        }
        
        public static SwitchBuilder Switch(Expression value, LabelTarget label, Annotations annotations) {
            return new SwitchBuilder(value, label, annotations);
        }
    }
}
