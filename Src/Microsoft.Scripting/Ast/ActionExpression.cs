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
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Internal.Generation;

namespace Microsoft.Scripting.Internal.Ast {
    public class ActionExpression : Expression {
        private readonly IList<Expression> _arguments;
        private readonly Action _action;

        public ActionExpression(Action action, IList<Expression> arguments, SourceSpan span)
            : base(span) {
            if (action == null) throw new ArgumentNullException("action");
            if (arguments == null) throw new ArgumentNullException("arguments");

            _action = action;
            _arguments = arguments;
        }

        public Action Action {
            get { return _action; }
        }

        public IList<Expression> Arguments {
            get { return _arguments; }
        }

        public override object Evaluate(CodeContext context) {
            return context.LanguageContext.Binder.Execute(_action, Evaluate(_arguments, context));
        }

        private Type[] GetSiteTypes(Type returnType) {
            Type[] ret = new Type[_arguments.Count + 1];
            for (int i = 0; i < _arguments.Count; i++) {
                ret[i] = _arguments[i].ExpressionType;
            }
            ret[_arguments.Count] = returnType == typeof(void) ? typeof(object) : returnType;
            return ret;
        }

        public override void Emit(CodeGen cg) {
            EmitAs(cg, typeof(object));
        }

        public override void EmitAs(CodeGen cg, Type asType) {
            bool fast;
            Slot site = cg.CreateDynamicSite(_action, GetSiteTypes(asType), out fast);
            MethodInfo method = site.Type.GetMethod("Invoke");

            Debug.Assert(!method.IsStatic);

            // Emit "this" - the site
            site.EmitGet(cg);
            ParameterInfo[] parameters = method.GetParameters();

            int first = 0;

            // Emit code context for unoptimized sites only
            if (!fast) {
                Debug.Assert(parameters.Length == _arguments.Count + 1);
                Debug.Assert(parameters[0].ParameterType == typeof(CodeContext));

                cg.EmitCodeContext();

                // skip the CodeContext parameter
                first = 1;
            }

            // Emit the arguments
            for (int arg = 0; arg < _arguments.Count; arg ++) {
                Expression ex = _arguments[arg];
                ex.EmitAs(cg, parameters[arg + first].ParameterType);
            }

            // Emit the site invoke
            cg.EmitCall(site.Type, "Invoke");

            // Convert result into expected type
            cg.EmitConvert(method.ReturnType, asType);
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                foreach (Expression ex in _arguments) {
                    ex.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }

        #region Factories
        
        public static ActionExpression Operator(SourceSpan span, Operators op, params Expression[] arguments) {
            return new ActionExpression(DoOperationAction.Make(op), arguments, span);
        }

        public static ActionExpression Operator(Operators op, params Expression[] arguments) {
            return new ActionExpression(DoOperationAction.Make(op), arguments, SourceSpan.None);
        }

        public static ActionExpression GetMember(SymbolId name, params Expression[] arguments) {
            return new ActionExpression(GetMemberAction.Make(name), arguments, SourceSpan.None);
        }

        public static ActionExpression SetMember(SymbolId name, params Expression[] arguments) {
            return new ActionExpression(SetMemberAction.Make(name), arguments, SourceSpan.None);
        }
        
        #endregion
    }
}
