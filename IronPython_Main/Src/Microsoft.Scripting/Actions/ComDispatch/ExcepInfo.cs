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
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Diagnostics;
using ComTypes = System.Runtime.InteropServices.ComTypes;

namespace Microsoft.Scripting.Actions.ComDispatch {
    /// <summary>
    /// This is similar to ComTypes.EXCEPINFO, but lets us do our own custom marshaling
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
    [StructLayout(LayoutKind.Sequential)]
    public struct ExcepInfo {
        private IntPtr bstrDescription;
        private IntPtr bstrHelpFile;
        private IntPtr bstrSource;
        private int dwHelpContext;
        private IntPtr pfnDeferredFillIn;
        private IntPtr pvReserved;
        private int scode;
        private short wCode;
        private short wReserved;

#if DEBUG
        static ExcepInfo() {
            Debug.Assert(Marshal.SizeOf(typeof(ExcepInfo)) == Marshal.SizeOf(typeof(ComTypes.EXCEPINFO)));
        }
#endif
        private static string ConvertAndFreeBstr(ref IntPtr bstr) {
            if (bstr == IntPtr.Zero) {
                return null;
            }

            string result = Marshal.PtrToStringBSTR(bstr);
            Marshal.FreeBSTR(bstr);
            bstr = IntPtr.Zero;
            return result;
        }

        internal void Dummy() {
            bstrDescription = IntPtr.Zero;
            bstrHelpFile = IntPtr.Zero;
            bstrSource = IntPtr.Zero;
            dwHelpContext = 0;
            pfnDeferredFillIn = IntPtr.Zero;
            pvReserved = IntPtr.Zero;
            scode = 0;
            wCode = 0;
            wReserved = 0; wReserved++;
            
            throw new InvalidOperationException("This method exists only to keep the compiler happy");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        internal COMException GetException() {
            Debug.Assert(pfnDeferredFillIn == IntPtr.Zero); // TODO
#if DEBUG
            System.Diagnostics.Debug.Assert(wReserved != -1);
            wReserved = -1; // to ensure that the method gets called only once
#endif

            int errorCode = (scode != 0) ? scode : wCode;
            string message = ConvertAndFreeBstr(ref bstrDescription);
            string source = ConvertAndFreeBstr(ref bstrSource);
            // TODO: Why do we need to include source here?
            COMException exception = new COMException(message ?? source, errorCode);
            exception.Source = source;

            string helpLink = ConvertAndFreeBstr(ref bstrHelpFile);
            if (dwHelpContext != 0) {
                helpLink += "#" + dwHelpContext;
            }
            exception.HelpLink = helpLink;

            return exception;
        }
    }
}

#endif