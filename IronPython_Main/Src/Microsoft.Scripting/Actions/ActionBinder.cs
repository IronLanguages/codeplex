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

using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Ast;
using System.Diagnostics;

namespace Microsoft.Scripting.Actions {
    /// <summary>
    /// Provides binding semantics for a language.  This include conversions as well as support
    /// for producing rules for actions.  These optimized rules are used for calling methods, 
    /// performing operators, and getting members using the ActionBinder's conversion semantics.
    /// </summary>
    public abstract class ActionBinder {
        private readonly CodeContext _context;

        private readonly Dictionary<Action, BigRuleSet> _rules = new Dictionary<Action,BigRuleSet>();

        protected ActionBinder(CodeContext context) {
            if (context == null) throw new ArgumentNullException("context");

            this._context = context;
        }

        public CodeContext Context {
            get { return _context; }
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
                    ruleSet = new BigRuleSet(_context);
                    _rules[action] = ruleSet;
                }
            }

            StandardRule<T> rule = ruleSet.FindRule<T>(args);
            if (rule != null && rule.IsValid) {
                return rule;
            }

            IDynamicObject ndo = args[0] as IDynamicObject;
            if (ndo != null) {
                rule = ndo.GetRule<T>(action, _context, args);
            }

            rule = rule ?? MakeRule<T>(action, args);
            Debug.Assert(rule != null);
#if DEBUG
            AstWriter.DumpRule(rule);
#endif
            ruleSet.AddRule(rule);
            return rule;
        }

        /// <summary>
        /// Gets a rule for the provided action and arguments and executes it without compiling.
        /// </summary>
        public object Execute(CodeContext cc, Action action, object[] args) {
            return DynamicSiteHelpers.Execute(cc, this, action, args);
        }

        public virtual AbstractValue AbstractExecute(Action action, IList<AbstractValue> args) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Produces a rule for the specified Action for the given arguments.
        /// 
        /// The default implementation can produce rules for standard .NET types.  Languages should
        /// override this and provide any custom behavior they need and fallback to the default
        /// implementation if no custom behavior is required.
        /// </summary>
        /// <typeparam name="T">The type of the DynamicSite the rule is being produced for.</typeparam>
        /// <param name="action">The Action that is being performed.</param>
        /// <param name="args">The arguments to the action as provided from the call site at runtime.</param>
        /// <returns></returns>
        protected virtual StandardRule<T> MakeRule<T>(Action action, object[] args) {
            switch (action.Kind) {
                case ActionKind.Call:
                    return new CallBinderHelper<T>(_context, (CallAction)action).MakeRule(args);
                case ActionKind.GetMember:
                    return new GetMemberBinderHelper<T>(_context.LanguageContext.Binder, (GetMemberAction)action).MakeNewRule(args);
                case ActionKind.SetMember:
                    return new SetMemberBinderHelper<T>(_context.LanguageContext.Binder, (SetMemberAction)action).MakeNewRule(args);
                case ActionKind.CreateInstance:
                    return new CreateInstanceBinderHelper<T>(_context, (CreateInstanceAction)action).MakeRule(args);
                case ActionKind.DoOperation:
                default:
                    throw new NotImplementedException(action.ToString());
            }
        }

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

        /// <summary>
        /// Gets the return value when an object contains out / by-ref parameters.  
        /// </summary>
        /// <param name="args">The values of by-ref and out parameters that the called method produced.  This includes the normal return
        /// value if the method does not return void.</param>
        public virtual object GetByRefArray(object[] args) {
            return args;
        }
    }
}
