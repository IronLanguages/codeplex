#if !SILVERLIGHT // ComObject

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace IronPython.Runtime.Types.ComDispatch {

    /// <summary>
    /// ComEventSinksContainer is just a regular list with a finalizer.
    /// This list is usually attached as a custom data for RCW object and 
    /// is finalized whenever RCW is finalized.
    /// </summary>
    class ComEventSinksContainer : List<ComEventSink>, IDisposable {

        private static readonly object _ComObjectEventSinksKey = (object)2;

        internal static ComEventSinksContainer FromRCW(object rcw, bool createIfNotFound) {
            // !!! Marshal.Get/SetComObjectData has a LinkDemand for UnmanagedCode which will turn into
            // a full demand. We need to avoid this by making this method SecurityCritical
            object data = Marshal.GetComObjectData(rcw, _ComObjectEventSinksKey);
            if (data != null || createIfNotFound == false) {
                return (ComEventSinksContainer )data;
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

        private void Dispose(bool disposing) {
            foreach (ComEventSink sink in this)
                sink.Dispose();
        }

        ~ComEventSinksContainer() {
            this.Dispose(false);
        }
    }
}

#endif