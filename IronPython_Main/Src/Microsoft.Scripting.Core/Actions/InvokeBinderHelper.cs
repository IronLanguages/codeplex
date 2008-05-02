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

using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Actions {
    using Ast = Microsoft.Scripting.Ast.Expression;
    
    public class InvokeMemberBinderHelper<T> : BinderHelper<T, InvokeMemberAction> {
        public InvokeMemberBinderHelper(CodeContext context, InvokeMemberAction action, object[] args)
            : base(context, action) {
            ContractUtils.RequiresNotNull(args, "args");
            if (args.Length < 1) throw new ArgumentException("Must receive at least one argument, the target to call", "args");
        }

        public virtual RuleBuilder<T> MakeRule() {
            CallAction callAction = CallAction.Make(Binder, Action.Signature);

            // TODO: First try to make a rule for get-member and see if we get back a constant method to call
            //GetMemberAction getAction = GetMemberAction.Make(Action.Name);
            //RuleBuilder<T> getRule = Binder.GetRule<T>(Context, getAction, new object[] { _args[0] });
            
            // otherwise, make a generic rule with embedded dynamic sites
            RuleBuilder<T> rule = new RuleBuilder<T>();
            rule.Test = Ast.True();
            Expression getExpr = Ast.Action.GetMember(Binder, Action.Name, typeof(object), rule.Parameters[0]);

            Expression[] callArgs = new Expression[rule.ParameterCount];
            callArgs[0] = getExpr;
            for (int i=1; i < callArgs.Length; i++) {
                callArgs[i] = rule.Parameters[i];
            }

            //TODO support non-object return types
            Expression callExpr = Ast.Action.Call(callAction, typeof(object), callArgs);

            rule.Target = rule.MakeReturn(Binder, callExpr);

            return rule;
        }
    }
}
