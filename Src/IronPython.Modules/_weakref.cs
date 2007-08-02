/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
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
            DynamicType.SetDynamicType(typeof(PythonWeakRefProxy),
                ProxyDynamicTypeBuilder.Build(typeof(PythonWeakRefProxy)));

            DynamicType.SetDynamicType(typeof(PythonCallableWeakRefProxy),
                ProxyDynamicTypeBuilder.Build(typeof(PythonCallableWeakRefProxy)));

            ProxyType = DynamicHelpers.GetDynamicTypeFromType(typeof(PythonWeakRefProxy));
            CallableProxyType = DynamicHelpers.GetDynamicTypeFromType(typeof(PythonCallableWeakRefProxy));
        }

        internal static IWeakReferenceable ConvertToWeakReferenceable(object obj) {
            IWeakReferenceable iwr = obj as IWeakReferenceable;
            if (iwr != null) return iwr;

            // we don't own dynamic type, so we jump through an extra step
            // to track it's weak references
            DynamicType dt = obj as DynamicType;
            if (dt != null) {
                PythonTypeContext ctx = dt.GetContextTag(PythonContext.Id) as PythonTypeContext;
                if (ctx == null) {
                    ctx = new PythonTypeContext();
                    dt.SetContextTag(PythonContext.Id, ctx);
                }

                return (PythonTypeContext)ctx;
            }
            throw PythonOps.TypeError("cannot create weak reference to '{0}' object", PythonOps.GetPythonTypeName(obj));
        }

        [PythonName("getweakrefcount")]
        public static int GetWeakRefCount(object @object) {
            return PythonWeakReference.GetWeakRefCount(@object);
        }

        [PythonName("getweakrefs")]
        public static List GetWeakRefs(object @object) {
            return PythonWeakReference.GetWeakRefs(@object);
        }

        public static object @ref = DynamicHelpers.GetDynamicTypeFromType(typeof(PythonWeakReference));

        [PythonName("proxy")]
        public static object Proxy(object @object) {
            return Proxy(@object, null);
        }
        [PythonName("proxy")]
        public static object Proxy(object @object, object callback) {
            if (PythonOps.IsCallable(@object)) {
                return PythonCallableWeakRefProxy.MakeNew(@object, callback);
            } else {
                return PythonWeakRefProxy.MakeNew(@object, callback);
            }
        }

        public static object CallableProxyType;
        public static object ProxyType;
        public static object ReferenceType = DynamicHelpers.GetDynamicTypeFromType(typeof(PythonWeakReference));
        public static object ReferenceError = ExceptionConverter.GetPythonException("ReferenceError");


        [PythonType("ref")]
        public class PythonWeakReference : ICallableWithCodeContext, IValueEquality {
            Utils.WeakHandle target;
            int hashVal;
            bool fHasHash;

            #region Python Constructors
            [PythonName("__new__")]
            public static object MakeNew(CodeContext context, DynamicType cls, object @object) {
                IWeakReferenceable iwr = ConvertToWeakReferenceable(@object);

                if (cls == DynamicHelpers.GetDynamicTypeFromType(typeof(PythonWeakReference))) {
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
                    return cls.CreateInstance(context, @object);
                }
            }

            [PythonName("__new__")]
            public static object MakeNew(CodeContext context, DynamicType cls, object @object, object callback) {
                if (callback == null) return MakeNew(context, cls, @object);
                if (cls == DynamicHelpers.GetDynamicTypeFromType(typeof(PythonWeakReference))) {
                    return new PythonWeakReference(@object, callback);
                } else {
                    return cls.CreateInstance(context, @object, callback);
                }
            }
            #endregion

            #region Constructors
            public PythonWeakReference(object @object)
                : this(@object, null) {
            }

            public PythonWeakReference(object @object, object callback) {
                WeakRefHelpers.InitializeWeakRef(this, @object, callback);
                this.target = new Utils.WeakHandle(@object, false);
            }
            #endregion

            #region Finalizer
            ~PythonWeakReference() {
                // remove our self from the chain...
                try {
                    if (target.IsAlive) {
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

            #region ICallableWithCodeContext Members

            public object Call(CodeContext context, params object[] args) {
                if (args.Length > 0) throw PythonOps.TypeError("__call__() takes exactly 0 arguments ({0} given)", args.Length);

                if (!target.IsAlive) {
                    throw PythonOps.ReferenceError("weak object has gone away");
                }
                try {
                    object res = target.Target;
                    GC.KeepAlive(this);
                    return res;
                } catch (InvalidOperationException) {
                    throw PythonOps.ReferenceError("weak object has gone away");
                }
            }

            #endregion

            #region IValueEquality Members

            int IValueEquality.GetValueHashCode() {
                if (!fHasHash) {
                    object refObj = target.Target;
                    if (refObj == null) throw PythonOps.TypeError("weak object has gone away");
                    GC.KeepAlive(this);
                    hashVal = refObj.GetHashCode();
                    fHasHash = true;
                }
                return hashVal;
            }

            bool IValueEquality.ValueEquals(object other) {
                if ((object)this == other) return true;

                bool fResult = false;
                PythonWeakReference wr = other as PythonWeakReference;
                if (wr != null) {
                    object ourTarget = target.Target;
                    object itsTarget = wr.target.Target;

                    GC.KeepAlive(this);
                    GC.KeepAlive(wr);
                    if (ourTarget != null && itsTarget != null) {
                        fResult = RefEquals(DefaultContext.Default, ourTarget, itsTarget);
                    }
                }
                GC.KeepAlive(this);
                return fResult;
            }

            bool IValueEquality.ValueNotEquals(object other) {
                return !((IValueEquality)this).ValueEquals(other);
            }

            /// <summary>
            /// Special equals because none of the special cases in Ops.Equals
            /// are applicable here, and the reference equality check breaks some tests.
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns></returns>
            private static bool RefEquals(CodeContext context, object x, object y) {
                object ret;

                ret = DynamicHelpers.GetDynamicType(x).InvokeBinaryOperator(context, Operators.Equal, x, y);
                if (ret != PythonOps.NotImplemented) return (bool)ret;

                ret = DynamicHelpers.GetDynamicType(y).InvokeBinaryOperator(context, Operators.Equal, y, x);
                if (ret != PythonOps.NotImplemented) return (bool)ret;

                return x.Equals(y);
            }


            #endregion            
        }

        [PythonType("weakproxy")]
        public sealed class PythonWeakRefProxy : ISuperDynamicObject, ICodeFormattable, IProxyObject, IValueEquality, ICustomMembers {
            Utils.WeakHandle target;

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
                this.target = new Utils.WeakHandle(target, false);
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
                    throw PythonOps.ReferenceError("weakly referenced object no longer exists");
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

            IAttributesCollection ISuperDynamicObject.Dict {
                get {
                    ISuperDynamicObject sdo = GetObject() as ISuperDynamicObject;
                    if (sdo != null) {
                        return sdo.Dict;
                    }

                    return null;
                }
            }

            bool ISuperDynamicObject.HasDictionary {
                get {
                    return (GetObject() as ISuperDynamicObject).HasDictionary;
                }
            }

            IAttributesCollection ISuperDynamicObject.SetDict(IAttributesCollection dict) {
                return (GetObject() as ISuperDynamicObject).SetDict(dict);
            }

            bool ISuperDynamicObject.ReplaceDict(IAttributesCollection dict) {
                return (GetObject() as ISuperDynamicObject).ReplaceDict(dict);
            }

            void ISuperDynamicObject.SetDynamicType(DynamicType newType) {
                (GetObject() as ISuperDynamicObject).SetDynamicType(newType);
            }

            #endregion

            #region IDynamicObject Members

            DynamicType ISuperDynamicObject.DynamicType {
                get {
                    return DynamicHelpers.GetDynamicTypeFromType(typeof(PythonWeakRefProxy));
                }
            }

            #endregion

            #region object overloads
            public override string ToString() {
                return GetObject().ToString();
            }

            #endregion

            #region ICodeFormattable Members

            string ICodeFormattable.ToCodeString(CodeContext context) {
                object obj = target.Target;
                GC.KeepAlive(this);
                return String.Format("<weakproxy at {0} to {1} at {2}>",
                    IdDispenser.GetId(this),
                    PythonOps.GetPythonTypeName(obj),
                    IdDispenser.GetId(obj));
            }

            #endregion


            #region ICustomMembers Members

            public bool TryGetCustomMember(CodeContext context, SymbolId name, out object value) {
                object o = GetObject();
                return PythonOps.TryGetAttr(context, o, name, out value);
            }

            public bool TryGetBoundCustomMember(CodeContext context, SymbolId name, out object value) {
                object o = GetObject();
                return PythonOps.TryGetBoundAttr(context, o, name, out value);
            }

            public void SetCustomMember(CodeContext context, SymbolId name, object value) {
                object o = GetObject();
                PythonOps.SetAttr(context, o, name, value);
            }

            public bool DeleteCustomMember(CodeContext context, SymbolId name) {
                object o = GetObject();
                context.LanguageContext.DeleteMember(context, o, name);
                //TODO: Return the right value here (IP does not need it)
                return true;
            }

            public IList<object> GetCustomMemberNames(CodeContext context) {
                object o;
                if (!TryGetObject(out o)) {
                    // if we've been disconnected return an empty list
                    return new List();
                }

                return PythonOps.GetAttrNames(context, o);
            }

            public IDictionary<object, object> GetCustomMemberDictionary(CodeContext context) {
                object o = GetObject();
                return PythonOps.GetAttrDict(context, o);
            }

            #endregion

            #region IProxyObject Members

            object IProxyObject.Target {
                get { return GetObject(); }
            }

            #endregion

            #region IValueEquality Members
            public int GetValueHashCode() {
                throw PythonOps.TypeErrorForUnhashableType("weakproxy");
            }

            public bool ValueEquals(object other) {
                PythonWeakRefProxy wrp = other as PythonWeakRefProxy;
                if (wrp != null) return PythonOps.EqualRetBool(GetObject(), wrp.GetObject());

                return PythonOps.EqualRetBool(GetObject(), other);
            }

            public bool ValueNotEquals(object other) {
                PythonWeakRefProxy wrp = other as PythonWeakRefProxy;
                if (wrp != null) return !PythonOps.EqualRetBool(GetObject(), wrp.GetObject());

                return PythonSites.NotEqualRetBool(GetObject(), other);
            }
            #endregion

            [PythonName("__nonzero__")]
            public object IsNonZero() {
                return Converter.ConvertToBoolean(GetObject());
            }
        }

        [PythonType("weakcallableproxy")]
        public sealed class PythonCallableWeakRefProxy :
            ISuperDynamicObject,
            ICodeFormattable,
            ICallableWithCodeContext,
            IFancyCallable,
            IProxyObject,
            IValueEquality,
            ICustomMembers {

            Utils.WeakHandle target;

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
                this.target = new Utils.WeakHandle(target, false);
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
                    throw PythonOps.ReferenceError("weakly referenced object no longer exists");
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

            IAttributesCollection ISuperDynamicObject.Dict {
                get {
                    return (GetObject() as ISuperDynamicObject).Dict;
                }
            }

            bool ISuperDynamicObject.HasDictionary {
                get {
                    return (GetObject() as ISuperDynamicObject).HasDictionary;
                }
            }

            IAttributesCollection ISuperDynamicObject.SetDict(IAttributesCollection dict) {
                return (GetObject() as ISuperDynamicObject).SetDict(dict);
            }

            bool ISuperDynamicObject.ReplaceDict(IAttributesCollection dict) {
                return (GetObject() as ISuperDynamicObject).ReplaceDict(dict);
            }

            void ISuperDynamicObject.SetDynamicType(DynamicType newType) {
                (GetObject() as ISuperDynamicObject).SetDynamicType(newType);
            }

            #endregion

            #region IDynamicObject Members

            DynamicType ISuperDynamicObject.DynamicType {
                get {
                    return DynamicHelpers.GetDynamicTypeFromType(typeof(PythonCallableWeakRefProxy));
                }
            }

            #endregion

            #region object overloads
            public override string ToString() {
                return GetObject().ToString();
            }
            #endregion

            #region ICodeFormattable Members

            string ICodeFormattable.ToCodeString(CodeContext context) {
                object obj = target.Target;
                GC.KeepAlive(this);
                return String.Format("<weakproxy at {0} to {1} at {2}>",
                    IdDispenser.GetId(this),
                    PythonOps.GetPythonTypeName(obj),
                    IdDispenser.GetId(obj));
            }

            #endregion

            #region ICallableWithCodeContext Members

            object ICallableWithCodeContext.Call(CodeContext context, object[] args) {
                return PythonOps.CallWithContext(context, GetObject(), args);
            }

            #endregion

            #region IFancyCallable Members

            object IFancyCallable.Call(CodeContext context, object[] args, string[] names) {
                return PythonOps.CallWithKeywordArgs(context, GetObject(), args, names);
            }

            #endregion

            #region ICustomMembers Members

            public bool TryGetCustomMember(CodeContext context, SymbolId name, out object value) {
                object o = GetObject();
                return PythonOps.TryGetAttr(context, o, name, out value);
            }

            public bool TryGetBoundCustomMember(CodeContext context, SymbolId name, out object value) {
                object o = GetObject();
                return PythonOps.TryGetBoundAttr(context, o, name, out value);
            }

            public void SetCustomMember(CodeContext context, SymbolId name, object value) {
                object o = GetObject();
                PythonOps.SetAttr(context, o, name, value);
            }

            public bool DeleteCustomMember(CodeContext context, SymbolId name) {
                object o = GetObject();
                context.LanguageContext.DeleteMember(context, o, name);
                //TODO: Return the right value here (IP does not need it)
                return true;
            }

            public IList<object> GetCustomMemberNames(CodeContext context) {
                object o;
                if (!TryGetObject(out o)) {
                    // if we've been disconnected return an empty list
                    return new List();
                }

                return PythonOps.GetAttrNames(context, o);
            }

            public IDictionary<object, object> GetCustomMemberDictionary(CodeContext context) {
                object o = GetObject();
                return PythonOps.GetAttrDict(context, o);
            }

            #endregion

            #region IProxyObject Members

            object IProxyObject.Target {
                get { return GetObject(); }
            }

            #endregion

            #region IValueEquality Members

            public int GetValueHashCode() {
                throw PythonOps.TypeErrorForUnhashableType("weakcallableproxy");
            }

            public bool ValueEquals(object other) {
                PythonCallableWeakRefProxy wrp = other as PythonCallableWeakRefProxy;
                if (wrp != null) return GetObject().Equals(wrp.GetObject());

                return PythonOps.EqualRetBool(GetObject(), other);
            }

            public bool ValueNotEquals(object other) {
                PythonCallableWeakRefProxy wrp = other as PythonCallableWeakRefProxy;
                if (wrp != null) return !GetObject().Equals(wrp.GetObject());

                return PythonSites.NotEqualRetBool(GetObject(), other);
            }
            #endregion

            [PythonName("__nonzero__")]
            public object IsNonZero() {
                return Converter.ConvertToBoolean(GetObject());
            }
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

    [PythonType("slot-wrapper")]
    class SlotWrapper : DynamicTypeSlot, ICodeFormattable {
        SymbolId name;
        DynamicType type;

        public SlotWrapper(SymbolId slotName, DynamicType targetType) {
            name = slotName;
            this.type = targetType;
        }

        #region IDescriptor Members
        [PythonName("__get__")]
        public object GetAttribute(object instance, object owner) {
            if (instance == null) return this;

            IProxyObject proxy = instance as IProxyObject;

            if (proxy == null)
                throw PythonOps.TypeError("descriptor for {0} object doesn't apply to {1} object",
                    PythonOps.StringRepr(type.Name),
                    PythonOps.StringRepr(DynamicTypeOps.GetName(instance)));

            return new GenericMethodWrapper(name, proxy);
        }

        #endregion

        #region ICodeFormattable Members

        public string ToCodeString(CodeContext context) {
            return String.Format("<slot wrapper {0} of {1} objects>",
                PythonOps.StringRepr(SymbolTable.IdToString(name)),
                PythonOps.StringRepr(type.Name));
        }

        #endregion

        #region DynamicTypeSlot Overrides

        public override bool TryGetValue(CodeContext context, object instance, DynamicMixin owner, out object value) {
            if (instance == null) {
                value = this;
                return true;
            }

            IProxyObject proxy = instance as IProxyObject;

            if (proxy == null)
                throw PythonOps.TypeError("descriptor for {0} object doesn't apply to {1} object",
                    PythonOps.StringRepr(type.Name),
                    PythonOps.StringRepr(DynamicTypeOps.GetName(instance)));

            if (!DynamicHelpers.GetDynamicType(proxy.Target).TryGetBoundMember(context, proxy.Target, name, out value))
                return false;

            value = new GenericMethodWrapper(name, proxy);
            return true;
        }

        #endregion
    }

    [PythonType("method-wrapper")]
    public class GenericMethodWrapper : ICallableWithCodeContext, IFancyCallable {
        SymbolId name;
        IProxyObject target;

        public GenericMethodWrapper(SymbolId methodName, IProxyObject proxyTarget) {
            name = methodName;
            target = proxyTarget;
        }

        #region ICallableWithCodeContext Members

        [OperatorMethod]
        public object Call(CodeContext context, params object[] args) {
            return PythonOps.InvokeWithContext(context, target.Target, name, args);
        }

        #endregion

        #region IFancyCallable Members

        [OperatorMethod]
        public object Call(CodeContext context, object[] args, string[] names) {
            object targetMethod;
            if (!DynamicHelpers.GetDynamicType(target.Target).TryGetBoundMember(context, target.Target, name, out targetMethod))
                throw PythonOps.AttributeError("type {0} has no attribute {1}",
                    DynamicHelpers.GetDynamicType(target.Target),
                    SymbolTable.IdToString(name));

            return PythonOps.CallWithKeywordArgs(context, targetMethod, args, names);
        }

        #endregion
    }
}
