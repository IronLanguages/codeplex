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

#if !SILVERLIGHT

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;

namespace Microsoft.Scripting.Actions.ComDispatch {
    /// <summary>
    /// The object used to cache information about an IDispatch RCW object. This will be associated with
    /// every IDispatch RCW that is handled by the DLR.
    /// 
    /// 
    /// TODO: This should inherit from ComObject once ComObject is moved to the DLR
    /// 
    /// 
    /// </summary>
    public class IDispatchObject {
        private readonly IDispatch _dispatchObject; // The RCW object

        // This is valid only if ScriptDomainManager.Options.CachePointersInApartment==true
        private Dictionary<Thread, IntPtr> _dispatchPointersByApartment;

        [CLSCompliant(false)]
        public IDispatchObject(IDispatch rcw) {
            _dispatchObject = rcw;

            if (ScriptDomainManager.Options.CachePointersInApartment) {
                _dispatchPointersByApartment = new Dictionary<Thread,IntPtr>();
            }
        }

        [CLSCompliant(false)]
        public IDispatch DispatchObject { get { return _dispatchObject; } }

        /// <summary>
        /// The caller should also call ReleaseDispatchPointer.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public IntPtr GetDispatchPointerInCurrentApartment() {
            if (!ScriptDomainManager.Options.CachePointersInApartment) {
                return Marshal.GetIDispatchForObject(_dispatchObject);
            }

            lock (this) {
                Thread currentThread = Thread.CurrentThread;
                IntPtr dispatchPointerInApartment;
                if (_dispatchPointersByApartment.TryGetValue(currentThread, out dispatchPointerInApartment)) {
                    return dispatchPointerInApartment;
                }

                // We will hold onto the dispatch pointer until the RCW stays alive, and release it in Finalize
                dispatchPointerInApartment = Marshal.GetIDispatchForObject(_dispatchObject);
                _dispatchPointersByApartment[currentThread] = dispatchPointerInApartment;
                return dispatchPointerInApartment;
            }
        }

        public void ReleaseDispatchPointer(IntPtr dispatchPointer) {
            if (ScriptDomainManager.Options.CachePointersInApartment) {
                // Nothing to do here. This will leak the COM object.
                Debug.Assert(_dispatchPointersByApartment[Thread.CurrentThread] == dispatchPointer);
                return;
            } else {
                ComRuntimeHelpers.UnsafeMethods.IUnknownRelease(dispatchPointer);
            }
        }

    }
}

#endif