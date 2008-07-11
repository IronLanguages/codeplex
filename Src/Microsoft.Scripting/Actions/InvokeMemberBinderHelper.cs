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

using System;
using System.Scripting;
using System.Scripting.Actions;
using System.Linq.Expressions; 
using System.Scripting.Runtime;
using System.Scripting.Utils;

namespace Microsoft.Scripting.Actions {
    using Ast = System.Linq.Expressions.Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public class InvokeMemberBinderHelper<T> : BinderHelper<T, OldInvokeMemberAction> where T : class {
        public InvokeMemberBinderHelper(CodeContext context, OldInvokeMemberAction action, object[] args)
            : base(context, action) {
            ContractUtils.RequiresNotNull(args, "args");
            if (args.Length < 1) throw new ArgumentException("Must receive at least one argument, the target to call", "args");
        }

        public virtual RuleBuilder<T> MakeRule() {
            OldCallAction callAction = OldCallAction.Make(Binder, Action.Signature);

            // TODO: First try to make a rule for get-member and see if we get back a constant method to call
            //GetMemberAction getAction = GetMemberAction.Make(Action.Name);
            //RuleBuilder<T> getRule = Binder.GetRule<T>(Context, getAction, new object[] { _args[0] });
            
            // otherwise, make a generic rule with embedded dynamic sites
            RuleBuilder<T> rule = new RuleBuilder<T>();
            rule.Test = Ast.True();
            Expression getExpr = AstUtils.GetMember(Binder, SymbolTable.IdToString(Action.Name), typeof(object), rule.Context, rule.Parameters[0]);

            Expression[] callArgs = new Expression[rule.ParameterCount + 1];
            callArgs[0] = rule.Context;
            callArgs[1] = getExpr;
            for (int i = 2; i < callArgs.Length; i++) {
                callArgs[i] = rule.Parameters[i - 1];
            }

            //TODO support non-object return types
            Expression callExpr = AstUtils.Call(callAction, typeof(object), callArgs);

            rule.Target = rule.MakeReturn(Binder, callExpr);

            return rule;
        }
    }
}
