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


#if !SILVERLIGHT // ComObject

using System.Diagnostics;
using Microsoft.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using ComTypes = System.Runtime.InteropServices.ComTypes;

namespace Microsoft.Scripting.Com {
    /// <summary>
    /// This is similar to ComTypes.EXCEPINFO, but lets us do our own custom marshaling
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
    [StructLayout(LayoutKind.Sequential)]
    public struct ExcepInfo {
        private short wCode;
        private short wReserved;
        private IntPtr bstrSource;
        private IntPtr bstrDescription;
        private IntPtr bstrHelpFile;
        private int dwHelpContext;
        private IntPtr pvReserved;
        private IntPtr pfnDeferredFillIn;
        private int scode;

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
            wCode = 0;
            wReserved = 0; wReserved++;
            bstrSource = IntPtr.Zero;
            bstrDescription = IntPtr.Zero;
            bstrHelpFile = IntPtr.Zero;
            dwHelpContext = 0;
            pfnDeferredFillIn = IntPtr.Zero;
            pvReserved = IntPtr.Zero;
            scode = 0;

            throw Error.MethodShouldNotBeCalled();
        }
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        internal Exception GetException() {
            Debug.Assert(pfnDeferredFillIn == IntPtr.Zero); // TODO
#if DEBUG
            System.Diagnostics.Debug.Assert(wReserved != -1);
            wReserved = -1; // to ensure that the method gets called only once
#endif

            int errorCode = (scode != 0) ? scode : wCode;
            Exception exception = Marshal.GetExceptionForHR(errorCode);

            string message = ConvertAndFreeBstr(ref bstrDescription);
            if (message != null) {
                // If we have a custom message, create a new Exception object with the message set correctly.
                // We need to create a new object because "exception.Message" is a read-only property.
                if (exception is COMException) {
                    exception = new COMException(message, errorCode);
                } else {
                    Type exceptionType = exception.GetType();
                    ConstructorInfo ctor = exceptionType.GetConstructor(new Type[] { typeof(string) });
                    if (ctor != null) {
                        exception = (Exception)ctor.Invoke(new object[] { message });
                    }
                }
            }

            exception.Source = ConvertAndFreeBstr(ref bstrSource);

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