/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

using Microsoft.Scripting.Ast;

namespace Microsoft.Scripting.Actions {
    using Ast = Microsoft.Scripting.Ast.Ast;
    using Microsoft.Scripting.Utils;

    public class MemberBinderHelper<T, TActionKind> 
        : BinderHelper<T, TActionKind> where TActionKind : MemberAction {
        private StandardRule<T> _rule;              // the rule being produced
        private Type _strongBoxType;                // null or the specific instantiated type of StrongBox
        private object[] _args;                     // the arguments we're creating a rule for
        private object _target;

        public MemberBinderHelper(CodeContext context, TActionKind action, object []args)
            : base(context, action) {
            Contract.RequiresNotNull(args, "args");
            if (args.Length == 0) throw new ArgumentException("args must have at least one member");

            _args = args;

            _target = args[0];
            if (IsStrongBox(_target)) {
                _strongBoxType = _target.GetType();
                _target = ((IStrongBox)_target).Value;
            }

            _rule = new StandardRule<T>();
        }

        protected object Target {
            get {
                return _target;
            }
        }

        /// <summary> Gets the Expression that represents the instance we're looking up </summary>
        protected Expression Instance {
            get {
                if (_strongBoxType == null) return _rule.Parameters[0];

                return Ast.ReadField(Ast.Cast(_rule.Parameters[0], _strongBoxType), _strongBoxType.GetField("Value"));
            }
        }

        protected Type StrongBoxType {
            get {
                return _strongBoxType;
            }
            set {
                _strongBoxType = value;
            }
        }

        protected StandardRule<T> Rule {
            get {
                return _rule;
            }
            set {
                _rule = value;
            }
        }

        /// <summary> helper to grab the name of the member we're looking up as a string </summary>
        protected string StringName {
            get { return SymbolTable.IdToString(Action.Name); }
        }

        protected MemberTypes GetMemberType(MemberInfo[] members, out Expression error) {
            error = null;
            MemberTypes memberType = MemberTypes.All;
            for (int i = 0; i < members.Length; i++) {
                MemberInfo mi = members[i];
                if (mi.MemberType != memberType) {
                    if (memberType != MemberTypes.All) {
                        error = MakeAmbigiousMatchError(members);
                        return MemberTypes.All;
                    }
                    memberType = mi.MemberType;
                }
            }
            return memberType;
        }

        protected Expression MakeGenericPropertyExpression() {
            return Ast.New(
                typeof(MemberAccessException).GetConstructor(new Type[] { typeof(string) }),
                Ast.Constant(StringName)
            );
        }

        protected Expression MakeIncorrectArgumentExpression(int provided, int expected) {
            return Ast.Call(null,
                    typeof(RuntimeHelpers).GetMethod("TypeErrorForIncorrectArgumentCount", new Type[] { typeof(string), typeof(int), typeof(int) }),
                    Ast.Constant(StringName),
                    Ast.Constant(provided),
                    Ast.Constant(expected)
                );
        }

        private static Expression MakeAmbigiousMatchError(MemberInfo[] members) {
            StringBuilder sb = new StringBuilder();
            foreach (MemberInfo mi in members) {
                if (sb.Length != 0) sb.Append(", ");
                sb.Append(mi.MemberType);
                sb.Append(" : ");
                sb.Append(mi.ToString());
            }

            return Ast.New(typeof(AmbiguousMatchException).GetConstructor(new Type[] { typeof(string) }),
                        Ast.Constant(sb.ToString()));
        }
    }
}
