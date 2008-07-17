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

#if !SILVERLIGHT // ComObject

using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Scripting;
using System.Scripting.Actions;
using System.Scripting.Runtime;
using System.Text;

namespace Microsoft.Scripting.Actions.ComDispatch {

    internal abstract class ComMemberBinderHelper<T, TAction> : ComBinderHelper<T, TAction>
        where T : class
        where TAction : OldMemberAction {

        internal ComMemberBinderHelper(CodeContext context, TAction action)
            : base(context, action) {
        }

        /// <summary> helper to grab the name of the member we're looking up as a string </summary>
        protected string StringName {
            get { return SymbolTable.IdToString(Action.Name); }
        }

        protected static TrackerTypes GetMemberType(MemberGroup members, out Expression error) {
            error = null;
            TrackerTypes memberType = TrackerTypes.All;
            for (int i = 0; i < members.Count; i++) {
                MemberTracker mi = members[i];
                if (mi.MemberType != memberType) {
                    if (memberType != TrackerTypes.All) {
                        error = MakeAmbigiousMatchError(members);
                        return TrackerTypes.All;
                    }
                    memberType = mi.MemberType;
                }
            }
            return memberType;
        }

        private static Expression MakeAmbigiousMatchError(MemberGroup members) {
            StringBuilder sb = new StringBuilder();
            foreach (MethodTracker mi in members) {
                if (sb.Length != 0) sb.Append(", ");
                sb.Append(mi.MemberType);
                sb.Append(" : ");
                sb.Append(mi.ToString());
            }

            return Expression.New(
                typeof(AmbiguousMatchException).GetConstructor(new Type[] { typeof(string) }),
                Expression.Constant(sb.ToString())
            );
        }

        protected void MakeMissingMemberError(Type type) {
            AddToBody(Binder.MakeMissingMemberError(type, StringName).MakeErrorForRule(Rule, Binder));
        }

        protected void MakeReadOnlyMemberError(Type type) {
            AddToBody(Binder.MakeReadOnlyMemberError(Rule, type, StringName));
        }


    }
}

#endif
