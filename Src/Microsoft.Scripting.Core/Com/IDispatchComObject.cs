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

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Scripting.Actions;
using System.Linq.Expressions;
using ComTypes = System.Runtime.InteropServices.ComTypes;

namespace System.Scripting.Com {

    /// <summary>
    /// An object that implements IDispatch
    /// 
    /// This currently has the following issues:
    /// 1. If we prefer ComObjectWithTypeInfo over IDispatchObject, then we will often not
    ///    IDispatchObject since implementations of IDispatch often rely on a registered type library. 
    ///    If we prefer IDispatchObject over ComObjectWithTypeInfo, users get a non-ideal experience.
    /// 2. IDispatch cannot distinguish between properties and methods with 0 arguments (and non-0 
    ///    default arguments?). So obj.foo() is ambiguous as it could mean invoking method foo, 
    ///    or it could mean invoking the function pointer returned by property foo.
    ///    We are attempting to find whether we need to call a method or a property by examining
    ///    the ITypeInfo associated with the IDispatch. ITypeInfo tell's use what parameters the method
    ///    expects, is it a method or a property, what is the default property of the object, how to 
    ///    create an enumerator for collections etc.
    /// 3. IronPython processes the signature and converts ref arguments into return values. 
    ///    However, since the signature of a DispMethod is not available beforehand, this conversion 
    ///    is not possible. There could be other signature conversions that may be affected. How does 
    ///    VB6 deal with ref arguments and IDispatch?
    ///    
    /// We also support events for IDispatch objects:
    /// Background:
    /// COM objects support events through a mechanism known as Connect Points.
    /// Connection Points are separate objects created off the actual COM 
    /// object (this is to prevent circular references between event sink
    /// and event source). When clients want to sink events generated  by 
    /// COM object they would implement callback interfaces (aka source 
    /// interfaces) and hand it over (advise) to the Connection Point. 
    /// 
    /// Implementation details:
    /// When IDispatchObject.TryGetMember request is received we first check
    /// whether the requested member is a property or a method. If this check
    /// fails we will try to determine whether an event is requested. To do 
    /// so we will do the following set of steps:
    /// 1. Verify the COM object implements IConnectionPointContainer
    /// 2. Attempt to find COM object's coclass's description
    ///    a. Query the object for IProvideClassInfo interface. Go to 3, if found
    ///    b. From object's IDispatch retrieve primary interface description
    ///    c. Scan coclasses declared in object's type library.
    ///    d. Find coclass implementing this particular primary interface 
    /// 3. Scan coclass for all its source interfaces.
    /// 4. Check whether to any of the methods on the source interfaces matches 
    /// the request name
    /// 
    /// Once we determine that TryGetMember requests an event we will return
    /// an instance of BoundDispEvent class. This class has InPlaceAdd and
    /// InPlaceSubtract operators defined. Calling InPlaceAdd operator will:
    /// 1. An instance of ComEventSinksContainer class is created (unless 
    /// RCW already had one). This instance is hanged off the RCW in attempt
    /// to bind the lifetime of event sinks to the lifetime of the RCW itself,
    /// meaning event sink will be collected once the RCW is collected (this
    /// is the same way event sinks lifetime is controlled by PIAs).
    /// Notice: ComEventSinksContainer contains a Finalizer which will go and
    /// unadvise all event sinks.
    /// Notice: ComEventSinksContainer is a list of ComEventSink objects. 
    /// 2. Unless we have already created a ComEventSink for the required 
    /// source interface, we will create and advise a new ComEventSink. Each
    /// ComEventSink implements a single source interface that COM object 
    /// supports. 
    /// 3. ComEventSink contains a map between method DISPIDs to  the 
    /// multicast delegate that will be invoked when the event is raised.
    /// 4. ComEventSink implements IReflect interface which is exposed as
    /// custom IDispatch to COM consumers. This allows us to intercept calls
    /// to IDispatch.Invoke and apply custom logic - in particular we will
    /// just find and invoke the multicast delegate corresponding to the invoked
    /// dispid.
    ///  </summary>

    public partial class IDispatchComObject : GenericComObject {

        private readonly IDispatchObject _dispatchObject;
        private ComTypeDesc _comTypeDesc;
        private static Dictionary<Guid, ComTypeDesc> _CacheComTypeDesc = new Dictionary<Guid,ComTypeDesc>();

        internal IDispatchComObject(IDispatch rcw)
            : base(rcw) {
            _dispatchObject = new IDispatchObject(rcw);
        }

        public override string ToString() {

            EnsureScanDefinedFunctions();

            string typeName = this._comTypeDesc.TypeName;
            if (String.IsNullOrEmpty(typeName))
                typeName = "IDispatch";

            return String.Format("{0} ({1})", Obj.ToString(), typeName);
        }

        public ComTypeDesc ComTypeDesc {
            get {
                
                EnsureScanDefinedFunctions();
             
                return _comTypeDesc;
            }
        }

        private static int GetIDsOfNames(IDispatch dispatch, string name, out int dispId) {
            int[] dispIds = new int[1];
            Guid emtpyRiid = Guid.Empty;
            int hresult = dispatch.TryGetIDsOfNames(
                ref emtpyRiid,
                new string[] { name },
                1,
                0,
                dispIds);

            dispId = dispIds[0];
            return hresult;
        }

        private static int GetIDsOfNames(IDispatch dispatch, SymbolId name, out int dispId) {
            return GetIDsOfNames(dispatch, SymbolTable.IdToString(name), out dispId);
        }

        static int Invoke(IDispatch dispatch, int memberDispId, out object result) {
            Guid emtpyRiid = Guid.Empty;
            ComTypes.DISPPARAMS dispParams = new ComTypes.DISPPARAMS();
            ComTypes.EXCEPINFO excepInfo = new ComTypes.EXCEPINFO();
            uint argErr;
            int hresult = dispatch.TryInvoke(
                memberDispId,
                ref emtpyRiid,
                0,
                ComTypes.INVOKEKIND.INVOKE_PROPERTYGET,
                ref dispParams,
                out result,
                out excepInfo,
                out argErr);

            return hresult;
        }

        public override string Documentation {
            get {
                EnsureScanDefinedFunctions();

                return _comTypeDesc.Documentation;
            }
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public override bool Equals(object obj) {
            return Obj.Equals(obj);
        }

        #region ICustomMembers-like members

        public bool TryGetGetItem(out DispCallable value) {

            EnsureScanDefinedFunctions();

            ComMethodDesc methodDesc = _comTypeDesc.GetItem;

            // The following attempts to get a method corresponding to "[PROPERTYGET, DISPID(0)] HRESULT Item(...)".
            // However, without type information, we really don't know whether or not we have a property getter.
            // All we can do is verify that the found dispId is DISPID_VALUE.  So, if we find a dispId of DISPID_VALUE,
            // we happily package it up as a property getter; otherwise, it's a no go...
            if (methodDesc == null) {
                int dispId;
                string name = "Item";
                int hresult = GetIDsOfNames(_dispatchObject.DispatchObject, name, out dispId);
                if (hresult == ComHresults.DISP_E_UNKNOWNNAME) {
                    value = null;
                    return false;
                } else if (hresult != ComHresults.S_OK) {
                    throw new MissingMemberException(string.Format("Could not get DispId for {0} (error:0x{1:X})", name, hresult));
                }

                methodDesc = new ComMethodDesc(name, dispId, ComTypes.INVOKEKIND.INVOKE_PROPERTYGET);
                _comTypeDesc.GetItem = methodDesc;
            }

            value = new DispPropertyGet(_dispatchObject, methodDesc);

            return true;
        }

        public bool TryGetSetItem(out DispCallable value) {

            EnsureScanDefinedFunctions();
            
            ComMethodDesc methodDesc = _comTypeDesc.SetItem;

            // The following attempts to get a method corresponding to "[PROPERTYPUT, DISPID(0)] HRESULT Item(...)".
            // However, without type information, we really don't know whether or not we have a property setter.
            // All we can do is verify that the found dispId is DISPID_VALUE.  So, if we find a dispId of DISPID_VALUE,
            // we happily package it up as a property setter; otherwise, it's a no go...
            if (methodDesc == null) {
                int dispId;
                string name = "Item";
                int hresult = GetIDsOfNames(_dispatchObject.DispatchObject, name, out dispId);
                if (hresult == ComHresults.DISP_E_UNKNOWNNAME) {
                    value = null;
                    return false;
                } else if (hresult != ComHresults.S_OK) {
                    throw new MissingMemberException(string.Format("Could not get DispId for {0} (error:0x{1:X})", name, hresult));
                } else if (dispId != ComDispIds.DISPID_VALUE) {
                    value = null;
                    return false;
                }

                methodDesc = new ComMethodDesc(name, dispId, ComTypes.INVOKEKIND.INVOKE_PROPERTYPUT);
                _comTypeDesc.SetItem = methodDesc;
            }

            value = new DispPropertyPut(_dispatchObject, methodDesc);
            
            return true;
        }

        public bool TryGetAttr(SymbolId name, out object value) {

            // Check if the name exists
            EnsureScanDefinedFunctions();
            ComMethodDesc methodDesc = null;

            // TODO: We have a thread-safety issue here right now
            // TODO: since we are mutating _funcs array
            // TODO: The workaround is to use Hashtable (which is thread-safe
            // TODO: on read operations) to fetch the value out.
            if (_comTypeDesc.Funcs.TryGetValue(name, out methodDesc) == false) {

                EnsureScanDefinedEvents();
                ComEventDesc eventDesc;
                if (_comTypeDesc.Events.TryGetValue(name, out eventDesc)) {
                    value = new BoundDispEvent(this.Obj, eventDesc.sourceIID, eventDesc.dispid);
                    return true;
                }

                int dispId;
                int hresult = GetIDsOfNames(_dispatchObject.DispatchObject, name, out dispId);
                if (hresult == ComHresults.DISP_E_UNKNOWNNAME) {
                    value = null;
                    return false;
                } else if (hresult != ComHresults.S_OK) {
                    throw new MissingMemberException(string.Format("Could not get DispId for {0} (error:0x{1:X})", name, hresult));
                }

                methodDesc = new ComMethodDesc(name.ToString(), dispId);
                _comTypeDesc.Funcs.Add(name, methodDesc);
            }

            // There is a member with the given name.
            // It might be a property of a method.
            // 1. If this is a method we will return a callable object this defering the execution
            //    (notice that execution of parameterized properties is also defered)
            // 2. If this is a property - we will return the result of 
            //    invoking the property
            if (methodDesc != null && methodDesc.IsPropertyGet) {
                value = new DispPropertyGet(_dispatchObject, methodDesc);
                if (methodDesc.Parameters.Length == 0) {
                    value = ((DispPropertyGet)value)[new object[0]];
                }
            } else if (methodDesc != null && methodDesc.IsPropertyPut) {
                value = new DispPropertyPut(_dispatchObject, methodDesc);
            } else {
                value = new DispMethod(_dispatchObject, methodDesc);
            }

            return true;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId="context")]
        public bool TrySetAttr(SymbolId name, object value, out Exception exception) {

            exception = null;

            if (value is BoundDispEvent) {
                // CONSIDER: 
                // SetAttr on BoundDispEvent is the last operator that is called
                // during += on an actual event handler.
                // In practice, we can do nothing here and just ingore this operation.
                // But is it syntactically correct?
                return true;
            }

            int dispId;
            int hresult = GetIDsOfNames(_dispatchObject.DispatchObject, name, out dispId);
            if (hresult == ComHresults.DISP_E_UNKNOWNNAME) {
                exception = System.Runtime.InteropServices.Marshal.GetExceptionForHR(hresult);
                return false;
            }

            try {
                // We use Type.InvokeMember instead of IDispatch.Invoke so that we do not
                // have to worry about marshalling the arguments. Type.InvokeMember will use
                // IDispatch.Invoke under the hood.
                // This technique also only works on types declared with 
                // InterfaceType(ComInterfaceType.InterfaceIsIDispatch) attribute. This is
                // why IDispatchForReflection type is used instead of IDispatch.

                BindingFlags bindingFlags = 0;
                bindingFlags |= System.Reflection.BindingFlags.SetProperty;
                bindingFlags |= System.Reflection.BindingFlags.Instance;

                typeof(IDispatchForReflection).InvokeMember(
                    SymbolTable.IdToString(name),
                    bindingFlags,
                    Type.DefaultBinder,
                    Obj,
                    new object[1] { value },
                    CultureInfo.InvariantCulture
                    );
            } catch (TargetInvocationException e) {
                exception = e;
                return false;
            }

            return true;
        }

        #endregion

        #region IMembersList

        public override IList<SymbolId> GetMemberNames() {
            EnsureScanDefinedFunctions();
            EnsureScanDefinedEvents();
            IList<SymbolId> list = new List<SymbolId>();

            foreach (SymbolId symbol in _comTypeDesc.Funcs.Keys) {
                list.Add(symbol);
            }

            if (_comTypeDesc.Events != null && _comTypeDesc.Events.Count > 0) {
                foreach (SymbolId symbol in _comTypeDesc.Events.Keys) {
                    list.Add(symbol);
                }
            }

            return list;
        }

        #endregion

        public override MetaObject GetMetaObject(Expression parameter) {
            EnsureScanDefinedFunctions();
            return new IDispatchMetaObject(parameter, _comTypeDesc);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        internal static void GetFuncDescForDescIndex(ComTypes.ITypeInfo typeInfo, int funcIndex, out ComTypes.FUNCDESC funcDesc, out IntPtr funcDescHandle) {
            IntPtr pFuncDesc = IntPtr.Zero;
            typeInfo.GetFuncDesc(funcIndex, out pFuncDesc);

            // GetFuncDesc should never return null, this is just to be safe
            if (pFuncDesc == IntPtr.Zero) {
                throw new COMException("ResolveComReference.CannotRetrieveTypeInformation");
            }

            funcDesc = (ComTypes.FUNCDESC)Marshal.PtrToStructure(pFuncDesc, typeof(ComTypes.FUNCDESC));
            funcDescHandle = pFuncDesc;
        }

        private void EnsureScanDefinedEvents() {

            // _comTypeDesc.Events is null if we have not yet attempted
            // to scan the object for events.
            if (_comTypeDesc != null && _comTypeDesc.Events != null) {
                return;
            }

            // check type info in the type descriptions cache
            ComTypes.ITypeInfo typeInfo = ComRuntimeHelpers.GetITypeInfoFromIDispatch(_dispatchObject.DispatchObject, true);
            if (typeInfo == null) {
                _comTypeDesc = ComTypeDesc.CreateEmptyTypeDesc();
                return;
            }

            ComTypes.TYPEATTR typeAttr = ComRuntimeHelpers.GetTypeAttrForTypeInfo(typeInfo);

            if (_comTypeDesc == null) {
                lock (_CacheComTypeDesc) {
                    if (_CacheComTypeDesc.TryGetValue(typeAttr.guid, out _comTypeDesc) == true &&
                        _comTypeDesc.Events != null) {
                        return;
                    }
                }
            }

            ComTypeDesc typeDesc = ComTypeDesc.FromITypeInfo(typeInfo, null);

            ComTypes.ITypeInfo classTypeInfo = null;
            Dictionary<SymbolId, ComEventDesc> events = null;

            ComTypes.IConnectionPointContainer cpc = Obj as ComTypes.IConnectionPointContainer;
            if (cpc == null) {
                // No ICPC - this object does not support events
                events = ComTypeDesc.EmptyEvents;
            } else if ((classTypeInfo = GetCoClassTypeInfo(this.Obj, typeInfo)) == null) {
                // no class info found - this object may support events
                // but we could not discover those
                Debug.Assert(false, "object support IConnectionPoint but no class info found");
                events = ComTypeDesc.EmptyEvents;
            } else {
                events = new Dictionary<SymbolId, ComEventDesc>();

                ComTypes.TYPEATTR classTypeAttr = ComRuntimeHelpers.GetTypeAttrForTypeInfo(classTypeInfo);
                for (int i = 0; i < classTypeAttr.cImplTypes; i++) {
                    int hRefType;
                    classTypeInfo.GetRefTypeOfImplType(i, out hRefType);

                    ComTypes.ITypeInfo interfaceTypeInfo;
                    classTypeInfo.GetRefTypeInfo(hRefType, out interfaceTypeInfo);

                    ComTypes.IMPLTYPEFLAGS flags;
                    classTypeInfo.GetImplTypeFlags(i, out flags);
                    if ((flags & ComTypes.IMPLTYPEFLAGS.IMPLTYPEFLAG_FSOURCE) != 0) {
                        ScanSourceInterface(interfaceTypeInfo, ref events);
                    }
                }

                if (events.Count == 0) {
                    events = ComTypeDesc.EmptyEvents;
                }
            }

            lock (_CacheComTypeDesc) {
                ComTypeDesc cachedTypeDesc;
                if (_CacheComTypeDesc.TryGetValue(typeAttr.guid, out cachedTypeDesc)) {
                    _comTypeDesc = cachedTypeDesc;
                } else {
                    _comTypeDesc = typeDesc;
                    _CacheComTypeDesc.Add(typeAttr.guid, _comTypeDesc);
                }
                _comTypeDesc.Events = events;
            }

        }

        private static void ScanSourceInterface(ComTypes.ITypeInfo sourceTypeInfo, ref Dictionary<SymbolId, ComEventDesc> events) {
            ComTypes.TYPEATTR sourceTypeAttribute = ComRuntimeHelpers.GetTypeAttrForTypeInfo(sourceTypeInfo);

            for (int index = 0; index < sourceTypeAttribute.cFuncs; index++) {
                IntPtr funcDescHandleToRelease = IntPtr.Zero;

                try {
                    ComTypes.FUNCDESC funcDesc;
                    GetFuncDescForDescIndex(sourceTypeInfo, index, out funcDesc, out funcDescHandleToRelease);

                    // we are not interested in hidden or restricted functions for now.
                    if ((funcDesc.wFuncFlags & (int)ComTypes.FUNCFLAGS.FUNCFLAG_FHIDDEN) != 0) {
                        continue;
                    }
                    if ((funcDesc.wFuncFlags & (int)ComTypes.FUNCFLAGS.FUNCFLAG_FRESTRICTED) != 0) {
                        continue;
                    }

                    // TODO: prefixing events is a temporary workaround to allow dsitinguising 
                    // between methods and events in IntelliSense.
                    // Ideally, we should solve this problem by passing out flags.
                    SymbolId name = ComRuntimeHelpers.GetSymbolIdOfMethod(sourceTypeInfo, funcDesc.memid, "Event_");

                    // Sometimes coclass has multiple source interfaces. Usually this is caused by
                    // adding new events and putting them on new interfaces while keeping the
                    // old interfaces around. This may cause name collisioning which we are
                    // resolving by keeping only the first event with the same name.
                    if (events.ContainsKey(name) == false) {
                        ComEventDesc eventDesc = new ComEventDesc();
                        eventDesc.dispid = funcDesc.memid;
                        eventDesc.sourceIID = sourceTypeAttribute.guid;
                        events.Add(name, eventDesc);
                    }
                } finally {
                    if (funcDescHandleToRelease != IntPtr.Zero) {
                        sourceTypeInfo.ReleaseFuncDesc(funcDescHandleToRelease);
                    }
                }
            }
        }

        private static ComTypes.ITypeInfo GetCoClassTypeInfo(object rcw, ComTypes.ITypeInfo typeInfo) {
            Debug.Assert(typeInfo != null);

            IProvideClassInfo provideClassInfo = rcw as IProvideClassInfo;
            if (provideClassInfo != null) {
                IntPtr typeInfoPtr = IntPtr.Zero;
                try {
                    provideClassInfo.GetClassInfo(out typeInfoPtr);
                    if (typeInfoPtr != IntPtr.Zero) {
                        return Marshal.GetObjectForIUnknown(typeInfoPtr) as ComTypes.ITypeInfo;
                    }
                } finally {
                    if (typeInfoPtr != IntPtr.Zero) {
                        Marshal.Release(typeInfoPtr);
                    }
                }
            }

            // retrieving class information through IPCI has failed - 
            // we can try scanning the typelib to find the coclass

            ComTypes.ITypeLib typeLib;
            int typeInfoIndex;
            typeInfo.GetContainingTypeLib(out typeLib, out typeInfoIndex);
            string typeName = ComRuntimeHelpers.GetNameOfType(typeInfo);

            ComTypeLibDesc typeLibDesc = ComTypeLibDesc.GetFromTypeLib(typeLib);
            ComTypeClassDesc coclassDesc = typeLibDesc.GetCoClassForInterface(typeName);
            if (coclassDesc == null) {
                return null;
            }

            ComTypes.ITypeInfo typeInfoCoClass;
            Guid coclassGuid = coclassDesc.Guid;
            typeLib.GetTypeInfoOfGuid(ref coclassGuid, out typeInfoCoClass);
            return typeInfoCoClass;
        }

        private void EnsureScanDefinedFunctions() {
            if (_comTypeDesc != null && _comTypeDesc.Funcs != null)
                return;

            ComTypes.ITypeInfo typeInfo = ComRuntimeHelpers.GetITypeInfoFromIDispatch(_dispatchObject.DispatchObject, true);
            if (typeInfo == null) {
                _comTypeDesc = ComTypeDesc.CreateEmptyTypeDesc();
                return;
            }

            ComTypes.TYPEATTR typeAttr = ComRuntimeHelpers.GetTypeAttrForTypeInfo(typeInfo);

            if (_comTypeDesc == null) {
                lock (_CacheComTypeDesc) {
                    if (_CacheComTypeDesc.TryGetValue(typeAttr.guid, out _comTypeDesc) == true &&
                        _comTypeDesc.Funcs != null) {
                        return;
                    }
                }
            }

            ComTypeDesc typeDesc = ComTypeDesc.FromITypeInfo(typeInfo, null);

            Dictionary<SymbolId, ComMethodDesc> funcs;
            ComMethodDesc getItem = null;
            ComMethodDesc setItem = null;
            funcs = new Dictionary<SymbolId, ComMethodDesc>(typeAttr.cFuncs);
            Queue<KeyValuePair<IntPtr, ComTypes.FUNCDESC>> writeOnlyCandidates = new Queue<KeyValuePair<IntPtr, ComTypes.FUNCDESC>>();
            List<int> usedDispIds = new List<int>();

            try {
                for (int definedFuncIndex = 0; definedFuncIndex < typeAttr.cFuncs; definedFuncIndex++) {
                    IntPtr funcDescHandleToRelease = IntPtr.Zero;

                    try {
                        ComTypes.FUNCDESC funcDesc;
                        GetFuncDescForDescIndex(typeInfo, definedFuncIndex, out funcDesc, out funcDescHandleToRelease);

                        if ((funcDesc.wFuncFlags & (int)ComTypes.FUNCFLAGS.FUNCFLAG_FRESTRICTED) != 0) {
                            // This function is not meant for the script user to use.
                            continue;
                        }

                        // since we need to store only on function description per dispId, we might
                        // not need to store any info for a property_put as it typically shares its
                        // dispId with a property_get - as such, we will wait for a corresponding 
                        // property_get to come along.  But if it's a write only property, we'll 
                        // have to capture it later...
                        if ((funcDesc.invkind & ComTypes.INVOKEKIND.INVOKE_PROPERTYPUT) != 0 ||
                            (funcDesc.invkind & ComTypes.INVOKEKIND.INVOKE_PROPERTYPUTREF) != 0) {
                            // exception to the rule: for the special dispId == 0, we need to store
                            // the method descriptor for the Do(SetItem) action. 
                            if (funcDesc.memid == ComDispIds.DISPID_VALUE) {
                                setItem = new ComMethodDesc(typeInfo, funcDesc);
                            }
                            writeOnlyCandidates.Enqueue(new KeyValuePair<IntPtr, ComTypes.FUNCDESC>(funcDescHandleToRelease, funcDesc));
                            funcDescHandleToRelease = IntPtr.Zero;
                            continue;
                        }

                        ComMethodDesc methodDesc;
                        SymbolId name;

                        usedDispIds.Add(funcDesc.memid);

                        if (funcDesc.memid == ComDispIds.DISPID_NEWENUM) {
                            methodDesc = new ComMethodDesc(typeInfo, funcDesc);
                            name = SymbolTable.StringToId("GetEnumerator");
                            funcs.Add(name, methodDesc);
                            continue;
                        }

                        methodDesc = new ComMethodDesc(typeInfo, funcDesc);
                        name = SymbolTable.StringToId(methodDesc.Name);
                        funcs.Add(name, methodDesc);

                        // for the special dispId == 0, we need to store the method descriptor 
                        // for the Do(GetItem) action. 
                        if (funcDesc.memid == ComDispIds.DISPID_VALUE) {
                            getItem = new ComMethodDesc(typeInfo, funcDesc);
                        }
                    } finally {
                        if (funcDescHandleToRelease != IntPtr.Zero) {
                            typeInfo.ReleaseFuncDesc(funcDescHandleToRelease);
                        }
                    }
                }

                while (writeOnlyCandidates.Count > 0) {
                    KeyValuePair<IntPtr, ComTypes.FUNCDESC> woc = writeOnlyCandidates.Dequeue();

                    try {
                        if (!usedDispIds.Contains(woc.Value.memid)) {
                            ComMethodDesc methodDesc = new ComMethodDesc(typeInfo, woc.Value);
                            funcs.Add(SymbolTable.StringToId(methodDesc.Name), methodDesc);
                            usedDispIds.Add(woc.Value.memid);
                        }
                    } finally {
                        Debug.Assert(woc.Key != IntPtr.Zero);
                        typeInfo.ReleaseFuncDesc(woc.Key);
                    }
                }
            } finally {
                foreach (KeyValuePair<IntPtr, ComTypes.FUNCDESC> woc in writeOnlyCandidates) {
                    typeInfo.ReleaseFuncDesc(woc.Key);
                }
            }

            lock (_CacheComTypeDesc) {
                ComTypeDesc cachedTypeDesc;
                if (_CacheComTypeDesc.TryGetValue(typeAttr.guid, out cachedTypeDesc)) {
                    _comTypeDesc = cachedTypeDesc;
                } else {
                    _comTypeDesc = typeDesc;
                    _CacheComTypeDesc.Add(typeAttr.guid, _comTypeDesc);
                }
                _comTypeDesc.Funcs = funcs;
                _comTypeDesc.GetItem = getItem;
                _comTypeDesc.SetItem = setItem;
            }
        }
    }
}

#endif
