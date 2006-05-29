/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Shared Source License
 * for IronPython. A copy of the license can be found in the License.html file
 * at the root of this distribution. If you can not locate the Shared Source License
 * for IronPython, please send an email to ironpy@microsoft.com.
 * By using this source code in any fashion, you are agreeing to be bound by
 * the terms of the Shared Source License for IronPython.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace IronPython.Runtime {

    public interface IFastCallable : ICallable {
        #region Generated FastCallable methods

        // *** BEGIN GENERATED CODE ***

        object Call();
        object Call(object arg0);
        object Call(object arg0, object arg1);
        object Call(object arg0, object arg1, object arg2);
        object Call(object arg0, object arg1, object arg2, object arg3);
        object Call(object arg0, object arg1, object arg2, object arg3, object arg4);

        // *** END GENERATED CODE ***

        #endregion
    }
}
