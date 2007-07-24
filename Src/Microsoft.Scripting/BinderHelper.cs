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
    using Microsoft.Scripting.Generation;

    public class BinderHelper<T, ActionType> where ActionType : Action {
        private CodeContext _context;
        private ActionType _action;
        
        public BinderHelper(CodeContext context, ActionType action) {
            if (context == null) throw new ArgumentNullException("context");
            if (action == null) throw new ArgumentNullException("action");

            _context = context;
            _action = action;
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

        protected StandardRule<T> MakeMethodRule(BuiltinFunction bf, Type targetType) {
            StandardRule<T> rule = new StandardRule<T>();
            rule.SetTarget(
                rule.MakeReturn(
                    Binder,
                    Ast.New(typeof(BoundBuiltinFunction).GetConstructor(new Type[] { typeof(BuiltinFunction), typeof(object) }),
                        Ast.Constant(new MemberGroupConstant(bf)),
                        rule.Parameters[0]
                    )
                )
            );

            rule.MakeTest(targetType);
            return rule;
        }
        
        /// <summary>
        /// Gets the expressions which correspond to each parameter on the calling method.
        /// </summary>
        public static Expression[] GetArgumentExpressions(MethodCandidate candidate, CallAction action, StandardRule<T> rule, object[] args) {
            List<Expression> res = new List<Expression>();
            object target = args[0];

            BoundBuiltinFunction bbf = target as BoundBuiltinFunction;
            if (bbf != null) {
                res.Add(GetBoundTarget(rule, bbf));
            }

            for (int i = 0; i < ArgumentCount(action, rule); i++) {
                switch (action.GetArgumentKind(i)) {
                    case ArgumentKind.Simple:
                        res.Add(rule.Parameters[i + 1]);
                        break;

                    case ArgumentKind.Named:
                        // until everyone supports kw args this may be null but not if our action specifies kw-args.
                        Debug.Assert(candidate != null);

                        // need to figure out which parameter we represent...
                        for (int j = 0; j < candidate.Parameters.Count; j++) {
                            if (candidate.Parameters[j].Name == action.GetArgumentName(i)) {
                                while (res.Count <= j) {
                                    res.Add(null);
                                }

                                res[j] = rule.Parameters[i + 1];
                                break;
                            }
                        }
                        break;

                    case ArgumentKind.List:
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
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }

            return res.ToArray();
        }

        /// <summary>
        /// Gets the instance parameter for the bound builtin function call.
        /// </summary>
        private static Expression GetBoundTarget(StandardRule<T> rule, BoundBuiltinFunction bbf) {
            Type declType = bbf.Target.ClrDeclaringType;
            Expression self = Ast.ReadProperty(
                Ast.Cast(rule.Parameters[0], typeof(BoundBuiltinFunction)),
                typeof(BoundBuiltinFunction).GetProperty("Self"));

            if (IsStrongBox(bbf.Self)) {                                
                self = Ast.ReadField(
                    Ast.Cast(self, bbf.Self.GetType()),
                    bbf.Self.GetType().GetField("Value")
                );
            }

            return self;
        }

        public static bool IsStrongBox(object target) {
            return target != null &&
                target.GetType().IsGenericType &&
                target.GetType().GetGenericTypeDefinition() == typeof(StrongBox<>);
        }

        public static int ArgumentCount(CallAction action, StandardRule<T> rule) {
            if (action.ArgumentInfos != null) {
                // non-simple call...
                return action.ArgumentInfos.Length;
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
                switch (action.GetArgumentKind(i - 1)) {
                    case ArgumentKind.Simple:
                    case ArgumentKind.Instance:
                    case ArgumentKind.Named:
                        res.Add(DynamicHelpers.GetDynamicType(args[i]));
                        continue;

                    case ArgumentKind.List:
                        Debug.Assert(i == args.Length - 1);

                        IList<object> list = args[i] as IList<object>;
                        if (list == null) return null;

                        for (int j = 0; j < list.Count; j++) {
                            res.Add(DynamicHelpers.GetDynamicType(list[j]));
                        }
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            return res.ToArray();
        }
    }
}
