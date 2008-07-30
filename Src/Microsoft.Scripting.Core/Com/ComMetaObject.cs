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

#if !SILVERLIGHT

using System.Linq.Expressions;
using System.Scripting.Actions;
using System.Scripting.Utils;

namespace System.Scripting.Com {
    internal class ComMetaObject : MetaObject {
        internal ComMetaObject(Expression expression, Restrictions restrictions, object arg)
            : base(expression, restrictions, arg) {
        }

        #region MetaObject

        public override MetaObject Call(CallAction action, MetaObject[] args) {
            return action.Defer(Wrap(args));
        }

        public override MetaObject Convert(ConvertAction action, MetaObject[] args) {
            return action.Defer(Wrap(args));
        }

        public override MetaObject Create(CreateAction action, MetaObject[] args) {
            return action.Defer(Wrap(args));
        }

        public override MetaObject DeleteMember(DeleteMemberAction action, MetaObject[] args) {
            return action.Defer(Wrap(args));
        }

        public override MetaObject GetMember(GetMemberAction action, MetaObject[] args) {
            return action.Defer(Wrap(args));
        }

        public override MetaObject Invoke(InvokeAction action, MetaObject[] args) {
            return action.Defer(Wrap(args));
        }

        public override MetaObject Operation(OperationAction action, MetaObject[] args) {
            return action.Defer(Wrap(args));
        }

        public override MetaObject SetMember(SetMemberAction action, MetaObject[] args) {
            return action.Defer(Wrap(args));
        }

        #endregion

        private MetaObject[] Wrap(MetaObject[] args) {
            MetaObject[] wrap = args.Copy();
            wrap[0] = new MetaObject(
                Expression.Call(
                    typeof(ComObject).GetMethod("ObjectToComObject"),
                    Expression.ConvertHelper(Expression, typeof(object))
                ),
                Restrictions.ExpressionRestriction(
                    Expression.AndAlso(
                        Expression.NotEqual(
                            Expression.ConvertHelper(Expression, typeof(object)),
                            Expression.Null()
                        ),
                        Expression.Call(
                            typeof(System.Runtime.InteropServices.Marshal).GetMethod("IsComObject"),
                            Expression.ConvertHelper(Expression, typeof(object))
                        )
                    )
                )
            );
            return wrap;
        }

        internal static MetaObject GetComMetaObject(Expression expression, object arg) {
            return new ComMetaObject(expression, Restrictions.Empty, arg);
        }

        internal static bool IsComObject(object obj) {
            return obj != null && System.Runtime.InteropServices.Marshal.IsComObject(obj);
        }
    }
}

#endif
