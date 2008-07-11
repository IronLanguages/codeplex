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

using System.Collections.Generic;
using System.Scripting.Actions;
using System.Linq.Expressions;

namespace System.Scripting.Com {

    /// <summary>
    /// We have no additional information about this COM object.
    /// </summary>
    public class GenericComObject : ComObject {

        internal GenericComObject(object rcw) : base(rcw) { }

        public override string ToString() {
            return Obj.ToString();
        }

        public override string Documentation {
            get { return string.Empty; }
        }

        #region IMembersList Members

        public override IList<SymbolId> GetMemberNames() {
            return new List<SymbolId>();
        }

        #endregion

        public override MetaObject GetMetaObject(Expression parameter) {
            // Fallback to the language on everything
            return new MetaObject(parameter, Restrictions.Empty);
        }
    }
}

#endif
