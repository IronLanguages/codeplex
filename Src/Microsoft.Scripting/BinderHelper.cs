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
using System.Text;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using System.Diagnostics;

namespace Microsoft.Scripting {
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
        public static Expression[] GetArgumentExpressions(CallAction action, StandardRule<T> rule, object[] args) {
            List<Expression> res = new List<Expression>();
            object target = args[0];

            if (target is BoundBuiltinFunction) {
                // bound method call, the argument expressions include the instance from the bound function too
                res.Add(MemberExpression.Property(
                    StaticUnaryExpression.Convert(rule.GetParameterExpression(0), typeof(BoundBuiltinFunction)),
                    typeof(BoundBuiltinFunction).GetProperty("Self")));
            }

            for (int i = 0; i < ArgumentCount(action, rule); i++) {
                if (action.ArgumentKinds == null ||                // simple call...
                    (!action.ArgumentKinds[i].ExpandDictionary &&
                    !action.ArgumentKinds[i].ExpandList &&
                    action.ArgumentKinds[i].Name == SymbolId.Empty)) {
                    res.Add(rule.GetParameterExpression(i + 1));
                    continue;
                }

                // we only support lists right now
                Debug.Assert(action.ArgumentKinds[i].ExpandList);
                Debug.Assert(i == ArgumentCount(action, rule) - 1);

                for (int j = 0; j < ((IList<object>)args[args.Length - 1]).Count; j++) {
                    res.Add(
                        MethodCallExpression.Call(
                            StaticUnaryExpression.Convert(
                                rule.GetParameterExpression(i + 1),
                                typeof(IList<object>)),
                            typeof(IList<object>).GetMethod("get_Item"),
                            new ConstantExpression(j)
                        )
                    );
                }
            }

            return res.ToArray();
        }

        private static int ArgumentCount(CallAction action, StandardRule<T> rule) {
            if (action.ArgumentKinds != null) {
                // non-simple call...
                return action.ArgumentKinds.Length;
            }

            return rule.Parameters.Length - 1;
        }

        public static StaticUnaryExpression GetParamsList(StandardRule<T> rule) {
            return StaticUnaryExpression.Convert(
                rule.GetParameterExpression(rule.Parameters.Length - 1),
                typeof(IList<object>)
            );
        }

        public static Expression MakeParamsTest(StandardRule<T> rule, object[] args) {
            return BinaryExpression.Equal(
                MemberExpression.Property(
                    GetParamsList(rule),
                    typeof(ICollection<object>).GetProperty("Count")
                ),
                new ConstantExpression(((IList<object>)args[args.Length - 1]).Count)
            );

        }

        public static DynamicType[] GetArgumentTypes(CallAction action, object[] args) {
            List<DynamicType> res = new List<DynamicType>();
            for (int i = 1; i < args.Length; i++) {
                if (action.ArgumentKinds == null ||
                    (!action.ArgumentKinds[i - 1].ExpandDictionary &&
                    !action.ArgumentKinds[i - 1].ExpandList &&
                    action.ArgumentKinds[i - 1].Name == SymbolId.Empty)) {
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
