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

using System.Linq.Expressions;
using System.Scripting.Actions;
using Microsoft.Contracts;

namespace System.Scripting.Com {
    /// <summary>
    /// This represents a bound dispmethod on a IDispatch object.
    /// </summary>
    public abstract partial class DispCallable : IDynamicObject {

        private readonly IDispatchObject _dispatch;
        private readonly ComMethodDesc _methodDesc;

        internal DispCallable(IDispatchObject dispatch, ComMethodDesc methodDesc) {
            _dispatch = dispatch;
            _methodDesc = methodDesc;
        }

        [Confined]
        public override string/*!*/ ToString() {
            return String.Format("<bound dispmethod {0}>", _methodDesc.Name);
        }

        public IDispatchObject DispatchObject { 
            get { return _dispatch; } 
        }

        public ComMethodDesc ComMethodDesc {
            get { return _methodDesc; }
        }

        #region IDynamicObject Members

        MetaObject IDynamicObject.GetMetaObject(Expression parameter) {
            return new DispCallableMetaObject(parameter, this);
        }

        #endregion
    }
}

#endif
