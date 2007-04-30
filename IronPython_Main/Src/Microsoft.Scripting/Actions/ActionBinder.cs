/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;

using Microsoft.Scripting.Internal.Generation;
using Microsoft.Scripting.Internal.Ast;

namespace Microsoft.Scripting.Actions {
    /// <summary>
    /// Provides binding semantics for a language.  This include conversions as well as support
    /// for producing rules for actions.  These optimized rules for are used for calling methods, 
    /// performing operators, and getting members using the ActionBinder's conversion semantics.
    /// </summary>
    public abstract class ActionBinder {
        private CodeContext _context;

        private Dictionary<Action, BigRuleSet> _rules = new Dictionary<Action,BigRuleSet>();

        protected ActionBinder(CodeContext context) {
            this._context = context;
        }

        public CodeContext Context {
            get { return _context; }
            set { _context = value; }
        }

        /// <summary>
        /// Gets a rule for the provided action and arguments.
        /// </summary>
        /// <typeparam name="T">The type of the DynamicSite the rule is being produced for.</typeparam>
        /// <param name="action">The Action the rule is being produced for.</param>
        /// <param name="args">The arguments to the rule as provided from the call site at runtime.</param>
        /// <returns>The new rule.</returns>
        public StandardRule<T> GetRule<T>(Action action, object[] args) {
            if (action == null) throw new ArgumentNullException("action");

            BigRuleSet ruleSet;
            lock (_rules) {
                if (!_rules.TryGetValue(action, out ruleSet)) {
                    ruleSet = new BigRuleSet(_context.LanguageContext);
                    _rules[action] = ruleSet;
                }
            }

            StandardRule<T> rule = ruleSet.FindRule<T>(args);
            if (rule != null && rule.IsValid) {
                return rule;
            }

            rule = MakeRule<T>(action, args);
            ruleSet.AddRule(rule);
            return rule;
        }

        /// <summary>
        /// Gets a rule for the provided action and arguments and executes it without compiling.
        /// </summary>
        public object Execute(Action action, object[] args) {
            throw new NotImplementedException(); //GetRule(action, args).
        }

        /// <summary>
        /// Produces a rule for the specified Action for the given arguments.
        /// </summary>
        /// <typeparam name="T">The type of the DynamicSite the rule is being produced for.</typeparam>
        /// <param name="action">The Action that is being performed.</param>
        /// <param name="args">The arguments to the action as provided from the call site at runtime.</param>
        /// <returns></returns>
        protected abstract StandardRule<T> MakeRule<T>(Action action, object[] args);

        /// <summary>
        /// Emits the code to convert an arbitrary object to the specified type.
        /// </summary>
        public abstract void EmitConvertFromObject(CodeGen cg, Type paramType);

        /// <summary>
        /// Converts an object at runtime into the specified type.
        /// </summary>
        public abstract object Convert(object obj, Type toType);

        /// <summary>
        /// Determines if a conversion exists from fromType to toType at the specified narrowing level.
        /// </summary>
        public abstract bool CanConvertFrom(Type fromType, Type toType, NarrowingLevel level);

        /// <summary>
        /// Provides ordering for two parameter types if there is no conversion between the two parameter types.
        /// 
        /// Returns true to select t1, false to select t2.
        /// </summary>
        public abstract bool PreferConvert(Type t1, Type t2);


        /// <summary>
        /// Converts the provided expression to the given type.  The expression is safe to evaluate multiple times.
        /// </summary>
        public abstract Expression ConvertExpression(Expression expr, Type toType);
    }
}
