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

namespace Microsoft.Scripting.Actions.ComDispatch {
    /// <summary>
    /// ComEventSinksContainer is just a regular list with a finalizer.
    /// This list is usually attached as a custom data for RCW object and 
    /// is finalized whenever RCW is finalized.
    /// </summary>
    public class ComEventSinksContainer : List<ComEventSink>, IDisposable {
        private static readonly object _ComObjectEventSinksKey = (object)2;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        public static ComEventSinksContainer FromRCW(object rcw, bool createIfNotFound) {
            // !!! Marshal.Get/SetComObjectData has a LinkDemand for UnmanagedCode which will turn into
            // a full demand. We need to avoid this by making this method SecurityCritical
            object data = Marshal.GetComObjectData(rcw, _ComObjectEventSinksKey);
            if (data != null || createIfNotFound == false) {
                return (ComEventSinksContainer)data;
            }

            lock (_ComObjectEventSinksKey) {
                data = Marshal.GetComObjectData(rcw, _ComObjectEventSinksKey);
                if (data != null) {
                    return (ComEventSinksContainer)data;
                }

                ComEventSinksContainer comEventSinks = new ComEventSinksContainer();
                if (!Marshal.SetComObjectData(rcw, _ComObjectEventSinksKey, comEventSinks)) {
                    throw new COMException("Marshal.SetComObjectData failed");
                }

                return comEventSinks;
            }
        }

        #region IDisposable Members

        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "disposing")]
        private void Dispose(bool disposing) {
            foreach (ComEventSink sink in this) {
                sink.Dispose();
            }
        }

        ~ComEventSinksContainer() {
            this.Dispose(false);
        }
    }
}

#endif