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

using IronPython.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Types;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;

[assembly: PythonModule("_weakref", typeof(IronPython.Modules.PythonWeakRef))]
namespace IronPython.Modules {
    [PythonType("weakref")]
    public static class PythonWeakRef {
        static PythonWeakRef() {
            Ops.SaveDynamicType(typeof(PythonWeakRefProxy),
                new ProxyDynamicType(typeof(PythonWeakRefProxy)));

            Ops.SaveDynamicType(typeof(PythonCallableWeakRefProxy),
                new ProxyDynamicType(typeof(PythonCallableWeakRefProxy)));

            ProxyType = Ops.GetDynamicTypeFromType(typeof(PythonWeakRefProxy));
            CallableProxyType = Ops.GetDynamicTypeFromType(typeof(PythonCallableWeakRefProxy));
        }

        internal static IWeakReferenceable ConvertToWeakReferenceable(object obj) {
            IWeakReferenceable iwr = obj as IWeakReferenceable;
            if (iwr != null) return iwr;

            throw Ops.TypeError("cannot create weak reference to '{0}' object", Ops.GetPythonTypeName(obj));
        }

        [PythonName("getweakrefcount")]
        public static int GetWeakRefCount(object @object) {
            return PythonWeakReference.GetWeakRefCount(@object);
        }

        [PythonName("getweakrefs")]
        public static List GetWeakRefs(object @object) {
            return PythonWeakReference.GetWeakRefs(@object);
        }

        public static object @ref = Ops.GetDynamicTypeFromType(typeof(PythonWeakReference));

        [PythonName("proxy")]
        public static object Proxy(object @object) {
            return Proxy(@object, null);
        }
        [PythonName("proxy")]
        public static object Proxy(object @object, object callback) {
            if (Ops.IsCallable(@object)) {
                return PythonCallableWeakRefProxy.MakeNew(@object, callback);
            } else {
                return PythonWeakRefProxy.MakeNew(@object, callback);
            }
        }

        public static object CallableProxyType;
        public static object ProxyType;
        public static object ReferenceType = Ops.GetDynamicTypeFromType(typeof(PythonWeakReference));
        public static object ReferenceError = ExceptionConverter.GetPythonException("ReferenceError");


        [PythonType("ref")]
        public class PythonWeakReference : ICallable {
            GCHandle target;
            int hashVal;
            bool fHasHash;

            #region Python Constructors
            [PythonName("__new__")]
            public static object MakeNew(DynamicType cls, object @object) {
                IWeakReferenceable iwr = ConvertToWeakReferenceable(@object);

                if (cls == Ops.GetDynamicTypeFromType(typeof(PythonWeakReference))) {
                    WeakRefTracker wrt = iwr.GetWeakRef();
                    if (wrt != null) {
                        for (int i = 0; i < wrt.HandlerCount; i++) {
                            if (wrt.GetHandlerCallback(i) == null && wrt.GetWeakRef(i) is PythonWeakReference) {
                                return wrt.GetWeakRef(i);
                            }
                        }
                    }

                    return new PythonWeakReference(@object);
                } else {
                    return cls.ctor.Call(cls, @object);
                }
            }

            [PythonName("__new__")]
            public static object MakeNew(DynamicType cls, object @object, object callback) {
                if (callback == null) return MakeNew(cls, @object);
                if (cls == Ops.GetDynamicTypeFromType(typeof(PythonWeakReference))) {
                    return new PythonWeakReference(@object, callback);
                } else {
                    return cls.ctor.Call(cls, @object, callback);
                }
            }
            #endregion

            #region Constructors
            public PythonWeakReference(object @object)
                : this(@object, null) {
            }

            public PythonWeakReference(object @object, object callback) {
                WeakRefHelpers.InitializeWeakRef(this, @object, callback);
                this.target = GCHandle.Alloc(@object, GCHandleType.Weak);
            }
            #endregion

            #region Finalizer
            ~PythonWeakReference() {
                // remove our self from the chain...
                try {
                    if (target.IsAllocated) {
                        IWeakReferenceable iwr = target.Target as IWeakReferenceable;
                        if (iwr != null) {
                            WeakRefTracker wrt = iwr.GetWeakRef();
                            if (wrt != null) {
                                // weak reference being finalized before target object,
                                // we don't want to run the callback when the object is
                                // finalized.
                                wrt.RemoveHandler(this);
                            }
                        }

                        target.Free();
                    }
                } catch (InvalidOperationException) {
                    // target was freed
                }
            }
            #endregion

            #region Static helpers
            public static int GetWeakRefCount(object o) {
                IWeakReferenceable iwr = o as IWeakReferenceable;
                if (iwr != null) {
                    WeakRefTracker wrt = iwr.GetWeakRef();
                    if (wrt != null) return wrt.HandlerCount;
                }

                return 0;
            }

            public static List GetWeakRefs(object o) {
                List l = new List();
                IWeakReferenceable iwr = o as IWeakReferenceable;
                if (iwr != null) {
                    WeakRefTracker wrt = iwr.GetWeakRef();
                    if (wrt != null) {
                        for (int i = 0; i < wrt.HandlerCount; i++) {
                            l.AddNoLock(wrt.GetWeakRef(i));
                        }
                    }
                }
                return l;
            }

            #endregion

            #region ICallable Members

            public object Call(params object[] args) {
                if (args.Length > 0) throw Ops.TypeError("__call__() takes exactly 0 arguments ({0} given)", args.Length);

                if (!target.IsAllocated) {
                    throw Ops.ReferenceError("weak object has gone away");
                }
                try {
                    object res = target.Target;
                    GC.KeepAlive(this);
                    return res;
                } catch (InvalidOperationException) {
                    throw Ops.ReferenceError("weak object has gone away");
                }
            }

            #endregion

            #region object overrides
            public override int GetHashCode() {
                if (!fHasHash) {
                    object refObj = target.Target;
                    if (refObj == null) throw Ops.TypeError("weak object has gone away");
                    GC.KeepAlive(this);
                    hashVal = refObj.GetHashCode();
                    fHasHash = true;
                }
                return hashVal;
            }

            public override bool Equals(object obj) {
                if ((object)this == obj) return true;

                bool fResult = false;
                PythonWeakReference wr = obj as PythonWeakReference;
                if (wr != null) {
                    object ourTarget = target.Target;
                    object itsTarget = wr.target.Target;

                    GC.KeepAlive(this);
                    GC.KeepAlive(wr);
                    if (ourTarget != null && itsTarget != null) {
                        fResult = Ops.IsTrue(RefEquals(ourTarget, itsTarget));
                    }
                }
                GC.KeepAlive(this);
                return fResult;
            }

            /// <summary>
            /// Special equals because none of the special cases in Ops.Equals
            /// are applicable here, and the reference equality check breaks some tests.
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns></returns>
            static object RefEquals(object x, object y) {
                object ret = Ops.NotImplemented;

                ret = Ops.GetDynamicType(x).Equal(x, y);
                if (ret != Ops.NotImplemented) return ret;

                ret = Ops.GetDynamicType(y).Equal(y, x);
                if (ret != Ops.NotImplemented) return ret;

                return Ops.Bool2Object(x.Equals(y));
            }

            #endregion
        }

        [PythonType("weakproxy")]
        public sealed class PythonWeakRefProxy : ISuperDynamicObject, ICodeFormattable, IProxyObject, IRichEquality, ICustomAttributes {
            GCHandle target;

            #region Python Constructors
            internal static object MakeNew(object @object, object callback) {
                IWeakReferenceable iwr = ConvertToWeakReferenceable(@object);

                if (callback == null) {
                    WeakRefTracker wrt = iwr.GetWeakRef();
                    if (wrt != null) {
                        for (int i = 0; i < wrt.HandlerCount; i++) {
                            if (wrt.GetHandlerCallback(i) == null && wrt.GetWeakRef(i) is PythonWeakRefProxy) {
                                return wrt.GetWeakRef(i);
                            }
                        }
                    }
                }

                return new PythonWeakRefProxy(@object, callback);
            }
            #endregion

            #region Constructors

            private PythonWeakRefProxy(object target, object callback) {
                WeakRefHelpers.InitializeWeakRef(this, target, callback);
                this.target = GCHandle.Alloc(target, GCHandleType.Weak);
            }
            #endregion

            #region Finalizer
            ~PythonWeakRefProxy() {
                // remove our self from the chain...
                try {
                    IWeakReferenceable iwr = target.Target as IWeakReferenceable;
                    if (iwr != null) {
                        WeakRefTracker wrt = iwr.GetWeakRef();
                        wrt.RemoveHandler(this);
                    }

                    target.Free();
                } catch (InvalidOperationException) {
                    // target was freed
                }
            }
            #endregion

            #region private members
            /// <summary>
            /// gets the object or throws a reference exception
            /// </summary>
            object GetObject() {
                object res;
                if (!TryGetObject(out res)) {
                    throw Ops.ReferenceError("weakly referenced object no longer exists");
                }
                return res;
            }

            bool TryGetObject(out object result) {
                try {
                    result = target.Target;
                    if (result == null) return false;
                    GC.KeepAlive(this);
                    return true;
                } catch (InvalidOperationException) {
                    result = null;
                    return false;
                }
            }
            #endregion

            #region ISuperDynamicObject Members

            IAttributesDictionary ISuperDynamicObject.GetDict() {
                return (GetObject() as ISuperDynamicObject).GetDict();
            }

            bool ISuperDynamicObject.SetDict(IAttributesDictionary dict) {
                return (GetObject() as ISuperDynamicObject).SetDict(dict);
            }

            void ISuperDynamicObject.SetDynamicType(UserType newType) {
                (GetObject() as ISuperDynamicObject).SetDynamicType(newType);
            }

            #endregion

            #region IDynamicObject Members

            DynamicType IDynamicObject.GetDynamicType() {
                return Ops.GetDynamicTypeFromType(typeof(PythonWeakRefProxy));
            }

            #endregion

            #region object overloads
            public override string ToString() {
                return GetObject().ToString();
            }

            #endregion

            #region ICodeFormattable Members

            string ICodeFormattable.ToCodeString() {
                object obj = target.Target;
                GC.KeepAlive(this);
                return String.Format("<weakproxy at {0} to {1} at {2}>", 
                    IdDispenser.GetId(this),
                    Ops.GetPythonTypeName(obj),
                    IdDispenser.GetId(obj));
            }

            #endregion

            #region ICustomAttributes Members

            public bool TryGetAttr(ICallerContext context, SymbolId name, out object value) {
                object o = GetObject();
                return Ops.TryGetAttr(context, o, name, out value);
            }

            public void SetAttr(ICallerContext context, SymbolId name, object value) {
                object o = GetObject();
                Ops.SetAttr(context, o, name, value);
            }

            public void DeleteAttr(ICallerContext context, SymbolId name) {
                object o = GetObject();
                Ops.DelAttr(context, o, name);
            }

            public List GetAttrNames(ICallerContext context) {
                object o;
                if (!TryGetObject(out o)) {
                    // if we've been disconnected return an empty list
                    return new List();
                }
                
                return Ops.GetAttrNames(context, o);
            }

            public IDictionary<object, object> GetAttrDict(ICallerContext context) {
                object o = GetObject();
                return Ops.GetAttrDict(context, o);
            }

            #endregion
            
            #region IProxyObject Members

            object IProxyObject.Target {
                get { return GetObject(); }
            }

            #endregion

            #region IRichEquality Members
            public object RichGetHashCode() {
                throw Ops.TypeErrorForUnhashableType("weakproxy");
            }

            public object RichEquals(object other) {
                PythonWeakRefProxy wrp = other as PythonWeakRefProxy;
                if (wrp != null) return Ops.Bool2Object(Ops.EqualRetBool(GetObject(), wrp.GetObject()));

                return Ops.Equal(GetObject(), other);
            }

            public object RichNotEquals(object other) {
                PythonWeakRefProxy wrp = other as PythonWeakRefProxy;
                if (wrp != null) return Ops.Bool2Object(!Ops.EqualRetBool(GetObject(), wrp.GetObject()));

                return Ops.NotEqual(GetObject(), other);
            }
            #endregion            
        }

        [PythonType("weakcallableproxy")]
        public sealed class PythonCallableWeakRefProxy : ISuperDynamicObject, ICodeFormattable, ICallable, IFancyCallable, IProxyObject, IRichEquality, ICustomAttributes {
            GCHandle target;

            #region Python Constructors
            internal static object MakeNew(object @object, object callback) {
                IWeakReferenceable iwr = ConvertToWeakReferenceable(@object);

                if (callback == null) {
                    WeakRefTracker wrt = iwr.GetWeakRef();
                    if (wrt != null) {
                        for (int i = 0; i < wrt.HandlerCount; i++) {

                            if (wrt.GetHandlerCallback(i) == null &&
                                wrt.GetWeakRef(i) is PythonCallableWeakRefProxy) {
                                return wrt.GetWeakRef(i);
                            }
                        }
                    }
                }

                return new PythonCallableWeakRefProxy(@object, callback);
            }
            #endregion

            #region Constructors

            private PythonCallableWeakRefProxy(object target, object callback) {
                WeakRefHelpers.InitializeWeakRef(this, target, callback);
                this.target = GCHandle.Alloc(target, GCHandleType.Weak);
            }
            #endregion

            #region Finalizer
            ~PythonCallableWeakRefProxy() {
                // remove our self from the chain...
                try {
                    IWeakReferenceable iwr = target.Target as IWeakReferenceable;
                    if (iwr != null) {
                        WeakRefTracker wrt = iwr.GetWeakRef();
                        wrt.RemoveHandler(this);
                    }
                    target.Free();
                } catch (InvalidOperationException) {
                    // target was freed
                }
            }
            #endregion

            #region private members
            /// <summary>
            /// gets the object or throws a reference exception
            /// </summary>
            object GetObject() {
                object res;
                if (!TryGetObject(out res)) {
                    throw Ops.ReferenceError("weakly referenced object no longer exists");
                }
                return res;
            }

            bool TryGetObject(out object result) {
                try {
                    result = target.Target;
                    if (result == null) return false;
                    GC.KeepAlive(this);
                    return true;
                } catch (InvalidOperationException) {
                    result = null;
                    return false;
                }
            }
            #endregion

            #region ISuperDynamicObject Members

            IAttributesDictionary ISuperDynamicObject.GetDict() {
                return (GetObject() as ISuperDynamicObject).GetDict();
            }

            bool ISuperDynamicObject.SetDict(IAttributesDictionary dict) {
                return (GetObject() as ISuperDynamicObject).SetDict(dict);
            }

            void ISuperDynamicObject.SetDynamicType(UserType newType) {
                (GetObject() as ISuperDynamicObject).SetDynamicType(newType);
            }

            #endregion

            #region IDynamicObject Members

            DynamicType IDynamicObject.GetDynamicType() {
                return Ops.GetDynamicTypeFromType(typeof(PythonCallableWeakRefProxy));
            }

            #endregion

            #region object overloads
            public override string ToString() {
                return GetObject().ToString();
            }
            #endregion

            #region ICodeFormattable Members

            string ICodeFormattable.ToCodeString() {
                object obj = target.Target;
                GC.KeepAlive(this);
                return String.Format("<weakproxy at {0} to {1} at {2}>",
                    IdDispenser.GetId(this),
                    Ops.GetPythonTypeName(obj),
                    IdDispenser.GetId(obj));
            }

            #endregion

            #region ICallable Members

            object ICallable.Call(params object[] args) {
                return Ops.Call(GetObject(), args);
            }

            #endregion

            #region IFancyCallable Members

            object IFancyCallable.Call(ICallerContext context, object[] args, string[] names) {
                return Ops.Call(context, GetObject(), args, names);
            }

            #endregion

            #region ICustomAttributes Members

            public bool TryGetAttr(ICallerContext context, SymbolId name, out object value) {
                object o = GetObject();
                return Ops.TryGetAttr(context, o, name, out value);
            }

            public void SetAttr(ICallerContext context, SymbolId name, object value) {
                object o = GetObject();
                Ops.SetAttr(context, o, name, value);
            }

            public void DeleteAttr(ICallerContext context, SymbolId name) {
                object o = GetObject();
                Ops.DelAttr(context, o, name);
            }

            public List GetAttrNames(ICallerContext context) {
                object o;
                if (!TryGetObject(out o)) {
                    // if we've been disconnected return an empty list
                    return new List();
                }

                return Ops.GetAttrNames(context, o);
            }

            public IDictionary<object, object> GetAttrDict(ICallerContext context) {
                object o = GetObject();
                return Ops.GetAttrDict(context, o);
            }

            #endregion

            #region IProxyObject Members

            object IProxyObject.Target {
                get { return GetObject(); }
            }

            #endregion

            #region IRichEquality Members

            [PythonName("__hash__")]
            public object RichGetHashCode() {
                throw Ops.TypeErrorForUnhashableType("weakcallableproxy");
            }

            [PythonName("__eq__")]
            public object RichEquals(object other) {
                PythonCallableWeakRefProxy wrp = other as PythonCallableWeakRefProxy;
                if (wrp != null) return Ops.Bool2Object(GetObject().Equals(wrp.GetObject()));

                return Ops.Equal(GetObject(), other);
            }

            public object RichNotEquals(object other) {
                PythonCallableWeakRefProxy wrp = other as PythonCallableWeakRefProxy;
                if (wrp != null) return Ops.Bool2Object(!GetObject().Equals(wrp.GetObject()));

                return Ops.NotEqual(GetObject(), other);
            }
            #endregion            
        }

        static class WeakRefHelpers {
            public static void InitializeWeakRef(object self, object target, object callback) {
                IWeakReferenceable iwr = ConvertToWeakReferenceable(target);

                WeakRefTracker wrt = iwr.GetWeakRef();
                if (wrt == null) {
                    iwr.SetWeakRef(new WeakRefTracker(callback, self));
                } else {
                    wrt.ChainCallback(callback, self);
                }
            }
        }
    }

    /// <summary>
    /// Custom ReflectedType for proxy's that looks slots in the proxied objects
    /// type instead of our own type
    /// </summary>
    [PythonType(typeof(DynamicType))]
    public partial class ProxyDynamicType : ReflectedType {

        public ProxyDynamicType(Type type)
            : base(type) {
        }

        internal override bool TryLookupBoundSlot(ICallerContext context, object inst, SymbolId name, out object ret) {            
            IProxyObject po = inst as IProxyObject;
            Debug.Assert(po != null);

            object target = po.Target;
            return Ops.GetDynamicType(target).TryLookupBoundSlot(context, target, name, out ret);
        }        
    }
    
    class SlotWrapper : IDescriptor, ICodeFormattable {
        SymbolId name;
        ProxyDynamicType type;

        public SlotWrapper(SymbolId slotName, ProxyDynamicType targetType) {
            name = slotName;
            this.type = targetType;
        }

        #region IDescriptor Members

        public object GetAttribute(object instance, object owner) {
            if (instance == null) return this;

            IProxyObject proxy = instance as IProxyObject;

            if (proxy == null)
                throw Ops.TypeError("descriptor for {0} object doesn't apply to {1} object",
                    Ops.StringRepr(type.Name),
                    Ops.StringRepr(Ops.GetDynamicType(instance).Name));

            return new GenericMethodWrapper(name, proxy);            
        }

        #endregion

        #region ICodeFormattable Members

        public string ToCodeString() {
            return String.Format("<slot wrapper {0} of {1} objects>",
                Ops.StringRepr(name.ToString()),
                Ops.StringRepr(type.Name));
        }

        #endregion
    }

    [PythonType("method-wrapper")]
    public class GenericMethodWrapper : ICallable, IFancyCallable {
        SymbolId name;
        IProxyObject target;

        public GenericMethodWrapper(SymbolId methodName, IProxyObject proxyTarget) {
            name = methodName;
            target = proxyTarget;
        }

        #region ICallable Members

        [PythonName("__call__")]
        public object Call(params object[] args) {
            object ret;
            if(!Ops.TryInvokeSpecialMethod(target.Target, name, out ret, args))
                throw Ops.AttributeError("type {0} has no attribute {1}", 
                    Ops.GetDynamicType(target.Target), 
                    name.ToString());

            return ret;
        }

        #endregion

        #region IFancyCallable Members

        [PythonName("__call__")]
        public object Call(ICallerContext context, object[] args, string[] names) {
            object targetMethod;
            if(!Ops.GetDynamicType(target).TryLookupBoundSlot(context, target, name, out targetMethod))
                throw Ops.AttributeError("type {0} has no attribute {1}",
                    Ops.GetDynamicType(target.Target),
                    name.ToString());

            return Ops.Call(context, targetMethod, args, names);
        }

        #endregion
    }
}
