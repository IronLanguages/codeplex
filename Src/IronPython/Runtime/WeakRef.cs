/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

using Microsoft.Scripting;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Calls;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Actions;

namespace IronPython.Runtime {
    /// <summary>
    /// single finalizable instance used to track and deliver all the 
    /// callbacks for a single object that has been weakly referenced by
    /// one or more references and proxies.  The reference to this object
    /// is held in objects that implement IWeakReferenceable.
    /// </summary>
    public class WeakRefTracker {
        struct CallbackInfo {
            object callback;
            
            WeakHandle _longRef, _shortRef;
            
            public CallbackInfo(object callback, object weakRef) {
                this.callback = callback;
                // we need a short ref & a long ref to deal with the case
                // when what we're finalizing is cyclic trash.  (see test_weakref
                // test_callbacks_on_callback).  If the short ref is dead, but the
                // long ref still lives then it means we'the weakref is in the
                // finalization queue and we shouldn't run it's callback - we're
                // just unlucky and are getting ran first.
                this._longRef = new WeakHandle(weakRef, true);
                this._shortRef = new WeakHandle(weakRef, false);
            }

            public object Callback {
                get {
                    return callback;
                }
            }

            public WeakHandle LongRef { 
                get { 
                    return _longRef; 
                } 
            }

            public bool IsFinalizing {
                get {
                    return _longRef.IsAlive && !_shortRef.IsAlive || 
                        _longRef.Target != _shortRef.Target;
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
            return callbacks[index].LongRef.Target;
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
                            ci.LongRef.Target is InstanceFinalizer)) {

                            PythonCalls.Call(ci.Callback, ci.LongRef.Target);
                        }
                    } catch (Exception) {
                    }

                    callbacks[i].LongRef.Free();
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
    /// only reference to this object is the instance so when that goes out of context
    /// this does as well and this will get finalized.  
    /// </summary>
    internal sealed class InstanceFinalizer : ICallableWithCodeContext {
        private object _instance;
        private DynamicSite<object, object> _site = DynamicSite<object, object>.Create(CallAction.Simple);

        public InstanceFinalizer(object inst) {
            Debug.Assert(inst != null);

            _instance = inst;
        }

        #region ICallableWithCodeContext Members

        public object Call(CodeContext context, params object[] args) {
            object o;

            IronPython.Runtime.Types.OldInstance oi = _instance as IronPython.Runtime.Types.OldInstance;
            if (oi != null) {
                if (oi.TryGetCustomMember(context, Symbols.Unassign, out o)) {
                    return _site.Invoke(context, o);
                }
            } else {
                DynamicHelpers.GetDynamicType(_instance).TryInvokeUnaryOperator(context, Operators.Unassign, _instance, out o);
            }

            return null;
        }

        #endregion
    }
}
