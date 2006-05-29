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
using System.Diagnostics;

using System.Threading;

namespace IronPython.Runtime {
    /// <summary>
    /// Thread safe dictionary that allows lazy-creation where readers will block for
    /// the creation of the lazily created value.  Call GetOrCreateValue w/ a key
    /// and a callback function.  If the value exists it is returned, if not the create
    /// callback is called (w/o any locks held).  The create call back will only be called
    /// once for each key.  
    /// </summary>
    class Publisher<TKey, TValue> {
        Dictionary<TKey, PublishInfo<TValue>> data = new Dictionary<TKey,PublishInfo<TValue>>();

        public delegate TValue CreateValue();

        public TValue GetOrCreateValue(TKey key, CreateValue create) {
            lock (data) {
                PublishInfo<TValue> pubValue;
                if (data.TryGetValue(key, out pubValue)) {
                    if (pubValue.Value == null && pubValue.Exception == null) {
                        pubValue.PrepareForWait();
                        Monitor.Exit(data);

                        try {
                            pubValue.WaitForPublish();
                        } finally {
                            Monitor.Enter(data);
                            pubValue.FinishWait();
                        }
                    }

                    if (pubValue.Exception != null) throw new Exception("Error", pubValue.Exception);

                    return pubValue.Value;
                }

                TValue ret;
                // publish the empty PublishInfo
                data[key] = pubValue = new PublishInfo<TValue>();
                // release our lock while we create the new value
                // then re-acquire the lock and publish the info.
                Monitor.Exit(data);
                try{
                    try {
                        ret = create();
                        Debug.Assert(ret != null, "Can't publish a null value");
                    } finally {
                        Monitor.Enter(data);
                    }
                } catch (Exception e) {
                    pubValue.PublishError(e);
                    throw;
                }

                pubValue.PublishValue(ret);
                return ret;
            }
        }

        /// <summary>
        /// Helper class which stores the published value
        /// </summary>
        class PublishInfo<T> {
            public PublishInfo() {
            }

            public T Value;
            public Exception Exception;
            private ManualResetEvent waitEvent;
            private int waiters;

            public void PublishValue(T value) {
                Value = value;
                if (waitEvent != null) waitEvent.Set();
            }

            public void PublishError(Exception e) {
                Exception = e;
            }

            public void PrepareForWait() {
                if (waitEvent == null) {
                    ManualResetEvent mre = new ManualResetEvent(false);
                    if (Interlocked.CompareExchange<ManualResetEvent>(ref waitEvent, mre, null) != null) {
                        mre.Close();
                    }
                }
                waiters++;
            }

            public void WaitForPublish() {
                waitEvent.WaitOne();
            }

            public void FinishWait() {
                waiters--;
                if (waiters == 0) waitEvent.Close();
            }
        }
    }


}
