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
using System.Diagnostics;
using System.Collections.Generic;

using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Actions;

namespace Microsoft.Scripting.Actions {
    using Ast = Microsoft.Scripting.Ast.Ast;

    public class BinderHelper<T, ActionType> where ActionType : Action {
        private CodeContext _context;
        private ActionType _action;
        
        public BinderHelper(CodeContext context, ActionType action) {
            _context = context;
            _action = action;
        }
        
        protected static bool IsParamsCallWorker(CallAction action) {
            if (action.IsSimple) return false;

            foreach (ArgumentKind ak in action.ArgumentKinds) {
                if (ak.Name != SymbolId.Empty ||
                    ak.ExpandDictionary ||
                    ak.IsThis) {
                    return false;
                }
            }
            return action.ArgumentKinds[action.ArgumentKinds.Length - 1].ExpandList;
        }

        protected static BuiltinFunction TryConvertToBuiltinFunction(object o) {
            BuiltinMethodDescriptor md = o as BuiltinMethodDescriptor;

            if (md != null) {
                return md.Template;
            }

            BoundBuiltinFunction bbf = o as BoundBuiltinFunction;
            if (bbf != null) {
                return bbf.Target;
            }

            return o as BuiltinFunction;
        }


        protected CodeContext Context {
            get {
                return _context;
            }
        }

        protected ActionType Action {
            get {
                return _action;
            }
        }

        protected ActionBinder Binder {
            get {
                return _context.LanguageContext.Binder;
            }
        }

        /// <summary>
        /// Gets the expressions which correspond to each parameter on the calling method.
        /// </summary>
        public static Expression[] GetArgumentExpressions(MethodCandidate candidate, CallAction action, StandardRule<T> rule, object[] args) {
            List<Expression> res = new List<Expression>();
            object target = args[0];

            if (target is BoundBuiltinFunction) {
                // bound method call, the argument expressions include the instance from the bound function too
                res.Add(Ast.ReadProperty(
                    Ast.Cast(rule.Parameters[0], typeof(BoundBuiltinFunction)),
                    typeof(BoundBuiltinFunction).GetProperty("Self")));
            }

            for (int i = 0; i < ArgumentCount(action, rule); i++) {
                if (action.ArgumentKinds == null || action.ArgumentKinds[i].IsSimple) {
                    res.Add(rule.Parameters[i + 1]);
                } else if (action.ArgumentKinds[i].Name != SymbolId.Empty) {
                    // until everyone supports kw args this may be null but not if our action specifies kw-args.
                    Debug.Assert(candidate != null);

                    // need to figure out which parameter we represent...
                    for (int j = 0; j < candidate.Parameters.Count; j++) {
                        if (candidate.Parameters[j].Name == action.ArgumentKinds[i].Name) {
                            while (res.Count <= j) {
                                res.Add(null);
                            }

                            res[j] = rule.Parameters[i + 1];
                            break;
                        }
                    }
                } else {
                    // we only support lists right now
                    Debug.Assert(action.ArgumentKinds[i].ExpandList);
                    Debug.Assert(i == ArgumentCount(action, rule) - 1);

                    for (int j = 0; j < ((IList<object>)args[args.Length - 1]).Count; j++) {
                        res.Add(
                            Ast.Call(
                                Ast.Cast(
                                    rule.Parameters[i + 1],
                                    typeof(IList<object>)),
                                typeof(IList<object>).GetMethod("get_Item"),
                                Ast.Constant(j)
                            )
                        );
                    }
                }                
            }

            return res.ToArray();
        }

        private static int ArgumentCount(CallAction action, StandardRule<T> rule) {
            if (action.ArgumentKinds != null) {
                // non-simple call...
                return action.ArgumentKinds.Length;
            }

            return rule.ParameterCount - 1;
        }

        public static UnaryExpression GetParamsList(StandardRule<T> rule) {
            return Ast.Cast(
                rule.Parameters[rule.ParameterCount - 1],
                typeof(IList<object>)
            );
        }

        public static Expression MakeParamsTest(StandardRule<T> rule, object[] args) {
            return Ast.Equal(
                Ast.ReadProperty(
                    GetParamsList(rule),
                    typeof(ICollection<object>).GetProperty("Count")
                ),
                Ast.Constant(((IList<object>)args[args.Length - 1]).Count)
            );
        }

        public static DynamicType[] GetArgumentTypes(CallAction action, object[] args) {
            List<DynamicType> res = new List<DynamicType>();
            for (int i = 1; i < args.Length; i++) {
                if (action.ArgumentKinds == null ||
                    (!action.ArgumentKinds[i - 1].ExpandDictionary &&
                    !action.ArgumentKinds[i - 1].ExpandList)) {
                    res.Add(DynamicHelpers.GetDynamicType(args[i]));
                    continue;
                }

                // we don't support optimizing anything else yet...
                Debug.Assert(action.ArgumentKinds[i - 1].ExpandList);
                Debug.Assert(i == args.Length - 1);

                IList<object> list = args[i] as IList<object>;
                if (list == null) return null;

                for (int j = 0; j < list.Count; j++) {
                    res.Add(DynamicHelpers.GetDynamicType(list[j]));
                }
            }
            return res.ToArray();
        }

    }
}
