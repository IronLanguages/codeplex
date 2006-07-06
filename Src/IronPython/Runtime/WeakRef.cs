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
using System.Runtime.InteropServices;
using System.Diagnostics;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Calls;

namespace IronPython.Runtime {
    /// <summary>
    /// single finalizable instance used to track & deliver all the 
    /// callbacks for a single object that has been weakly referenced by
    /// one or more references & proxies.  The reference to this object
    /// is held in objects that implement IWeakReferenceable.
    /// </summary>
    public class WeakRefTracker {
        struct CallbackInfo {
            object callback;
            GCHandle weakref;
            GCHandle shortRef;

            public CallbackInfo(object callback, object weakRef) {
                this.callback = callback;
                // we need a short ref & a long ref to deal with the case
                // when what we're finalizing is cyclic trash.  (see test_weakref
                // test_callbacks_on_callback).  If the short ref is dead, but the
                // long ref still lives then it means we'the weakref is in the
                // finalization queue and we shouldn't run it's callback - we're
                // just unlucky and are getting ran first.
                this.weakref = GCHandle.Alloc(weakRef, GCHandleType.WeakTrackResurrection);
                this.shortRef = GCHandle.Alloc(weakRef, GCHandleType.Weak);
            }

            public object Callback {
                get {
                    return callback;
                }
            }
            public GCHandle WeakRef {
                get {
                    return weakref;
                }
            }

            public bool IsFinalizing {
                get {
                    return (weakref.IsAllocated != shortRef.IsAllocated) ||
                        (weakref.Target != shortRef.Target);

                }
            }
        }
        List<CallbackInfo> callbacks;

        public WeakRefTracker(object callback, object weakRef) {
            callbacks = new List<CallbackInfo>(1);
            ChainCallback(callback, weakRef);
        }

        public void ChainCallback(object callback, object weakRef) {
            callbacks.Add(new CallbackInfo(callback, weakRef));
        }

        public int HandlerCount {
            get {
                return callbacks.Count;
            }
        }

        public void RemoveHandlerAt(int index) {
            callbacks.RemoveAt(index);
        }

        public void RemoveHandler(object o) {
            for (int i = 0; i < HandlerCount; i++) {
                if (GetWeakRef(i) == o) {
                    RemoveHandlerAt(i);
                    break;
                }
            }
        }

        public object GetHandlerCallback(int index) {
            return callbacks[index].Callback;
        }

        public object GetWeakRef(int index) {
            return callbacks[index].WeakRef.Target;
        }

        ~WeakRefTracker() {
            // callbacks are delivered last registered to first registered.
            for (int i = callbacks.Count - 1; i >= 0; i--) {

                CallbackInfo ci = callbacks[i];
                try {
                    try {
                        // a little ugly - we only run callbacks that aren't a part
                        // of cyclic trash.  but classes use a single field for
                        // finalization & GC - and that's always cyclic, so we need to special case it.
                        if (ci.Callback != null &&
                            (!ci.IsFinalizing ||
                            ci.WeakRef.Target is InstanceFinalizer)) {

                            Ops.Call(ci.Callback, ci.WeakRef.Target);
                        }
                    } catch (Exception) {
                    }

                    callbacks[i].WeakRef.Free();
                } catch (InvalidOperationException) {
                    // target was freed
                }
            }
        }
    }


    /// <summary>
    /// Finalizable object used to hook up finalization calls for OldInstances.
    /// 
    /// We create one of these each time an object w/ a finalizer gets created.  The
    /// only reference to this object is the instance so when that goes out of scope
    /// this does as well and this will get finalized.  
    /// </summary>
    internal sealed class InstanceFinalizer : ICallable {
        object instance;

        public InstanceFinalizer(object inst) {
            Debug.Assert(inst != null);

            instance = inst;
        }

        #region ICallable Members

        public object Call(params object[] args) {
            object o;
            Ops.TryInvokeSpecialMethod(instance, SymbolTable.Unassign, out o, new object[0]);

            return null;
        }

        #endregion
    }   
}
