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
using System.Diagnostics;
using System.Reflection;

using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Types;

namespace Microsoft.Scripting.Actions {
    /// <summary>
    /// Provides binding semantics for a language.  This include conversions as well as support
    /// for producing rules for actions.  These optimized rules are used for calling methods, 
    /// performing operators, and getting members using the ActionBinder's conversion semantics.
    /// </summary>
    public abstract class ActionBinder {
        private CodeContext _context;
        private readonly Dictionary<Action, BigRuleSet> _rules = new Dictionary<Action,BigRuleSet>();

        protected ActionBinder(CodeContext context) {
            _context = context;
        }

        /// <summary>
        /// Deprecated - only used by DelegateSignatureInfo.GenerateDelegateStub.  Use CodeContext
        /// passed in at rule creation time instead.
        /// </summary>
        internal CodeContext Context {
            get {
                return _context;
            }
        }

        // TODO: internal and friendly UnitTests
        public void ClearRuleCache() {
            _rules.Clear();
        }

        /// <summary>
        /// Gets a rule for the provided action and arguments.
        /// </summary>
        /// <typeparam name="T">The type of the DynamicSite the rule is being produced for.</typeparam>
        /// <param name="action">The Action the rule is being produced for.</param>
        /// <param name="args">The arguments to the rule as provided from the call site at runtime.</param>
        /// <param name="callerContext">The CodeContext that is requesting the rule and that should be used for conversions.</param>
        /// <returns>The new rule.</returns>
        public StandardRule<T> GetRule<T>(CodeContext callerContext, Action action, object[] args) {
            if (action == null) throw new ArgumentNullException("action");
            //Debug.Assert(action.Kind != ActionKind.GetMember || ((GetMemberAction)action).Name != SymbolTable.StringToId("x"));

            BigRuleSet ruleSet;
            lock (_rules) {
                if (!_rules.TryGetValue(action, out ruleSet)) {
                    ruleSet = new BigRuleSet(callerContext);
                    _rules[action] = ruleSet;
                }
            }

            StandardRule<T> rule = ruleSet.FindRule<T>(args);
            if (rule != null && rule.IsValid) {
                return rule;
            }

#if DEBUG
            string name = "";
            if (args[0] is DynamicType) {
                name = ((DynamicType)args[0]).Name;
            }
            PerfTrack.NoteEvent(PerfTrack.Categories.Rules, "MakeRule " + action.ToString() + " " + name);
#endif
             
            IDynamicObject ndo = args[0] as IDynamicObject;
            if (ndo != null) {
                rule = ndo.GetRule<T>(action, callerContext, args);
                Debug.Assert(rule == null || rule.Target != null && rule.Test != null);
            }

            rule = rule ?? MakeRule<T>(callerContext, action, args);
            Debug.Assert(rule != null && rule.Target != null && rule.Test != null);
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
        /// <param name="callerContext">The CodeContext that is requesting the rule and should be use</param>
        /// <returns></returns>
        protected virtual StandardRule<T> MakeRule<T>(CodeContext callerContext, Action action, object[] args) {
            switch (action.Kind) {
                case ActionKind.Call:
                    return new CallBinderHelper<T>(callerContext, (CallAction)action).MakeRule(args);
                case ActionKind.GetMember:
                    return new GetMemberBinderHelper<T>(callerContext, (GetMemberAction)action, args).MakeNewRule();
                case ActionKind.SetMember:
                    return new SetMemberBinderHelper<T>(callerContext, (SetMemberAction)action, args).MakeNewRule();
                case ActionKind.CreateInstance:
                    return new CreateInstanceBinderHelper<T>(callerContext, (CreateInstanceAction)action).MakeRule(args);
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

        /// <summary>
        /// Gets the members that are visible from the provided type of the specified name.
        /// 
        /// The default implemetnation first searches the type, then the flattened heirachy of the type, and then
        /// registered extension methods.
        /// </summary>
        public virtual MemberInfo[] GetMember(Type type, string name) {
            MemberInfo[] members = type.GetMember(name);
            if (members.Length == 0) {
                members = type.GetMember(name, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                if (members.Length == 0) {
                    members = GetExtensionMembers(name, type);
                }
            }
            return members;
        }

        /// <summary>
        /// Provides a way for the binder to provide a custom error message when lookup fails.  Just
        /// doing this for the time being until we get a more robust error return mechanism.
        /// </summary>
        public virtual Expression MakeMissingMemberError(Type type, string name) {
            return Ast.Ast.New(
                typeof(MissingMemberException).GetConstructor(new Type[] { typeof(string) }),
                Ast.Ast.Constant(name)
            );           
        }

        /// <summary>
        /// Provides a way for the binder to provide a custom error message when lookup fails.  Just
        /// doing this for the time being until we get a more robust error return mechanism.
        /// </summary>
        public virtual Expression MakeReadOnlyMemberError(Type type, string name) {
            return Ast.Ast.New(
                typeof(MissingMemberException).GetConstructor(new Type[] { typeof(string) }),
                Ast.Ast.Constant(name)
            );
        }

        private static MemberInfo[] GetExtensionMembers(string name, Type type) {
                Type curType = type;
                do {
                    Type[] extTypes = DynamicHelpers.GetExtensionTypes(curType);
                    List<MemberInfo> members = new List<MemberInfo>();

                    foreach (Type ext in extTypes) {
                        foreach (MemberInfo mi in ext.GetMember(name)) {
                            members.Add(mi);
                        }

                        foreach (MemberInfo mi in ext.GetMember("Get" + name)) {
                            if (!mi.IsDefined(typeof(PropertyMethodAttribute), false)) continue;
                            // TODO: ExtProperties
                        }

                        foreach (MemberInfo mi in ext.GetMember("Set" + name)) {
                            if (!mi.IsDefined(typeof(PropertyMethodAttribute), false)) continue;
                            // TODO: ExtProperties
                        }
                    }

                    if (members.Count != 0) {
                        return members.ToArray();
                    }

                    curType = curType.BaseType;
                } while (curType != null);

                return new MemberInfo[0];
            }   
    }
}
