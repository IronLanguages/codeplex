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

        internal const int DISP_E_UNKNOWNINTERFACE = unchecked((int)0x80020001);
        internal const int DISP_E_MEMBERNOTFOUND = unchecked((int)0x80020003);
        internal const int DISP_E_PARAMNOTFOUND = unchecked((int)0x80020004);
        internal const int DISP_E_TYPEMISMATCH = unchecked((int)0x80020005);
        internal const int DISP_E_UNKNOWNNAME = unchecked((int)0x80020006); // GetIDsOfName
        internal const int DISP_E_NONAMEDARGS = unchecked((int)0x80020007);
        internal const int DISP_E_BADVARTYPE = unchecked((int)0x80020008);
        internal const int DISP_E_EXCEPTION = unchecked((int)0x80020009);
        internal const int DISP_E_OVERFLOW = unchecked((int)0x8002000A);
        internal const int DISP_E_BADINDEX = unchecked((int)0x8002000B); // GetTypeInfo
        internal const int DISP_E_UNKNOWNLCID = unchecked((int)0x8002000C);
        internal const int DISP_E_ARRAYISLOCKED = unchecked((int)0x8002000D); // VariantClear
        internal const int DISP_E_BADPARAMCOUNT = unchecked((int)0x8002000E);
        internal const int DISP_E_PARAMNOTOPTIONAL = unchecked((int)0x8002000F);

        public const int TYPE_E_LIBNOTREGISTERED = unchecked((int)0x8002801D);

        public static bool IsSuccess(int hresult) {
            return hresult >= 0;
        }
    }
}

#endif