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
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Microsoft.Scripting.Actions.ComDispatch {
    public static class ComHresults {
        public const int E_NOINTERFACE = unchecked((int)0x80004002);
        public const int E_FAIL = unchecked((int)0x80004005);
        public const int DISP_E_EXCEPTION = unchecked((int)0x80020009);
        public const int DISP_E_TYPEMISMATCH = unchecked((int)0x80020005);
        public const int TYPE_E_LIBNOTREGISTERED = unchecked((int)0x8002801D);

        public static bool IsSuccess(int hresult) {
            return hresult >= 0;
        }
    }
}

#endif