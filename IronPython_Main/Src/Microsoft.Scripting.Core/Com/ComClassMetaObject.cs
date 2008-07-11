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

using System.Scripting.Actions;
using System.Linq.Expressions;

namespace System.Scripting.Com {
    internal class ComClassMetaObject : MetaObject {
        internal ComClassMetaObject(Expression expression, ComTypeClassDesc cls)
            : base(expression, Restrictions.Empty, cls) {
        }

        public override MetaObject Create(CreateAction action, MetaObject[] args) {
            return new MetaObject(
                Expression.Call(
                    Expression.ConvertHelper(Expression, typeof(ComTypeClassDesc)),
                    typeof(ComTypeClassDesc).GetMethod("CreateInstance")
                ),
                Restrictions.Combine(args).Merge(
                    Restrictions.TypeRestriction(Expression, typeof(ComTypeClassDesc))
                )
            );
        }
    }
}

#endif
