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
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting;

namespace IronPython.Runtime.Types.ComDispatch {
    public class DispIndexer : DispCallable, ICallableWithCodeContext {
        internal DispIndexer(IDispatch dispatch, ComDispatch.ComMethodDesc methodDesc)
            : base(dispatch, methodDesc) {
        }

        public object this[params object[] args] {
            get { return base.Call(args); }
        }

        #region ICallableWithCodeContext Members

        public object Call(CodeContext context, object[] args) {
            return base.Call(args);
        }

        #endregion
    }
}
#endif
