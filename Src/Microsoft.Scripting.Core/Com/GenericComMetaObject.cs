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
    internal class GenericComMetaObject : MetaObject {
        internal GenericComMetaObject(Expression expression, Restrictions restrictions, object arg)
            : base(expression, restrictions, arg) {
        }

        #region MetaObject

        public override MetaObject Convert(ConvertAction action, MetaObject[] args) {
            ContractUtils.RequiresNotNull(action, "action");
            if (action.ToType.IsInterface) {
                // Converting a COM object to any interface is always considered possible - it will result in 
                // a QueryInterface at runtime
                return new MetaObject(
                     Expression.Convert(
                         Expression.Property(
                             Expression.ConvertHelper(args[0].Expression, typeof(GenericComObject)),
                             typeof(ComObject).GetProperty("Obj")
                         ),
                         action.ToType
                     ),
                    args[0].Restrictions.Merge(Restrictions.TypeRestriction(args[0].Expression, args[0].LimitType))
                );
            }

            return base.Convert(action, args);
        }

        #endregion
    }
}

#endif
