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

using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace Microsoft.Scripting.ComInterop {
    /// <summary>
    /// The object used to cache information about an IDispatch RCW object. This will be associated with
    /// every IDispatch RCW that is handled by the DLR.
    /// 
    /// #TODO: The class can go away.
    /// </summary>
    public class IDispatchObject {

        private readonly IDispatch _dispatchObject;  // The RCW object

        internal IDispatchObject(IDispatch rcw) {
            _dispatchObject = rcw;
        }

        [CLSCompliant(false)]
        public IDispatch DispatchObject { get { return _dispatchObject; } }

        /// <summary>
        /// The caller should also call ReleaseDispatchPointer.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [Obsolete("Called from generated code only", true)]
        public IntPtr GetDispatchPointerInCurrentApartment() {
            return Marshal.GetIDispatchForObject(_dispatchObject);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [Obsolete("Called from generated code only", true)]
        public void ReleaseDispatchPointer(IntPtr dispatchPointer) {
            UnsafeMethods.IUnknownRelease(dispatchPointer);
        }

    }
}

#endif