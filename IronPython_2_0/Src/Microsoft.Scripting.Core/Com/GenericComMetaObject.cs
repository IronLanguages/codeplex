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
using System; using Microsoft;
#if !SILVERLIGHT

using Microsoft.Linq.Expressions;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Com {
    internal class GenericComMetaObject : MetaObject {
        internal GenericComMetaObject(Expression expression, Restrictions restrictions, object arg)
            : base(expression, restrictions, arg) {
        }

        #region MetaObject

        public override MetaObject Convert(ConvertAction action) {
            ContractUtils.RequiresNotNull(action, "action");
            if (action.ToType.IsInterface) {
                // Converting a COM object to any interface is always considered possible - it will result in 
                // a QueryInterface at runtime
                return new MetaObject(
                     Expression.Convert(
                         Expression.Property(
                             Expression.ConvertHelper(Expression, typeof(GenericComObject)),
                             typeof(ComObject).GetProperty("Obj")
                         ),
                         action.ToType
                     ),
                    Restrictions.Merge(Restrictions.TypeRestriction(Expression, LimitType))
                );
            }

            return base.Convert(action);
        }

        #endregion
    }
}

#endif
