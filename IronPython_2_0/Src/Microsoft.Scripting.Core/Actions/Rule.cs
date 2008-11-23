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


using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Microsoft.Linq.Expressions;
using Microsoft.Scripting.Utils;
using System.Globalization;

namespace Microsoft.Scripting.Actions {
    public class Rule<T> where T : class {
        /// <summary>
        /// The rule set that includes only this rule.
        /// </summary>
        private readonly SmallRuleSet<T> _mySet;

        /// <summary>
        /// The parameters to the rule
        /// </summary>
        private readonly ReadOnlyCollection<ParameterExpression> _parameters;

        /// <summary>
        /// The binding expression tree
        /// </summary>
        private readonly Expression _binding;

        /// <summary>
        /// The return label
        /// </summary>
        private readonly LabelTarget _returnLabel;

        /// <summary>
        /// Template data - null for methods which aren't templated.  Non-null for methods which
        /// have been templated.  The same template data is shared across all templated rules with
        /// the same target method.
        /// </summary>
        private readonly TemplateData<T> _template;

        public Rule(Expression binding, LabelTarget @return, params ParameterExpression[] parameters)
            : this(binding, @return, (IEnumerable<ParameterExpression>)parameters) {
        }

        public Rule(Expression binding, LabelTarget @return, IEnumerable<ParameterExpression> parameters) {
            var @params = parameters.ToReadOnly();
            ValidateRuleParameters(typeof(T), @return, @params);

            _binding = binding;
            _returnLabel = @return;
            _parameters = @params;
            _mySet = new SmallRuleSet<T>(new[] { this });
        }

        internal Rule(Expression binding, T target, TemplateData<T> template, LabelTarget @return, ReadOnlyCollection<ParameterExpression> parameters) {
            ValidateRuleParameters(typeof(T), @return, parameters);

            _binding = binding;
            _returnLabel = @return;
            _parameters = parameters;
            _mySet = new SmallRuleSet<T>(target, new Rule<T>[] { this });
            _template = template;
        }

        /// <summary>
        /// Each rule holds onto an immutable RuleSet that contains this rule only.
        /// This should heavily optimize monomorphic call sites.
        /// </summary>
        internal SmallRuleSet<T> RuleSet {
            get {
                return _mySet;
            }
        }

        /// <summary>
        /// Gets the logical parameters to the dynamic site in the form of Variables.
        /// </summary>
        public ReadOnlyCollection<ParameterExpression> Parameters {
            get { return _parameters; }
        }

        /// <summary>
        /// The expression representing the bound operation
        /// </summary>
        public Expression Binding {
            get { return _binding; }
        }

        /// <summary>
        /// The label used to return from the rule
        /// </summary>
        public LabelTarget ReturnLabel {
            get { return _returnLabel; }
        }

        internal TemplateData<T> Template {
            get {
                return _template;
            }
        }

        /// <summary>
        /// Gets or sets the method which is used for templating. If the rule is
        /// not templated then this is a nop (and returns null for the getter).
        /// 
        /// The method is tracked here independently from the delegate for the
        /// common case of the method being a DynamicMethod.  In order to re-bind
        /// the existing DynamicMethod to a new set of templated parameters we need
        /// to have the original method.
        /// </summary>
        internal MethodInfo TemplateMethod {
            get {
                if (_template != null) {
                    return _template.Method;
                }
                return null;
            }
            set {
                if (_template != null) {
                    _template.Method = value;
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        private string Dump {
            get {
                using (System.IO.StringWriter writer = new System.IO.StringWriter(CultureInfo.CurrentCulture)) {
                    ExpressionWriter.Dump(_binding, "Rule", writer);
                    return writer.ToString();
                }
            }
        }

        private static void ValidateRuleParameters(Type target, LabelTarget @return, ReadOnlyCollection<ParameterExpression> parameters) {
            ContractUtils.RequiresNotNull(parameters, "parameters");
            ContractUtils.Requires(typeof(Delegate).IsAssignableFrom(target), "target");
            MethodInfo invoke = target.GetMethod("Invoke");
            ContractUtils.RequiresNotNull(@return, "return");
            ContractUtils.Requires(@return.Type == invoke.GetReturnType());
            ParameterInfo[] pinfos = invoke.GetParametersCached();

            int count = pinfos.Length - 1;
            ContractUtils.Requires(parameters.Count == count, "parameters");

            for (int i = 0; i < count; i++) {
                ParameterExpression parameter = parameters[i];
                ContractUtils.RequiresNotNull(parameter, "parameters");
                Type type = pinfos[i + 1].ParameterType;
                if (parameter.IsByRef) {
                    type = type.GetElementType();
                }
                ContractUtils.Requires(type == parameter.Type, "parameters");
            }
        }
    }

    /// <summary>
    /// Data used for tracking templating information in a rule.
    /// 
    /// Currently we just track the method so we can retarget to
    /// new constant pools.
    /// </summary>
    internal class TemplateData<T> where T : class {
        internal MethodInfo Method;
    }
}
