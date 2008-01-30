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
using System.Text;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Diagnostics.SymbolStore;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.CompilerServices;

using ComTypes = System.Runtime.InteropServices.ComTypes;
using System.Diagnostics;
using System.Threading;

using Microsoft.Win32;
using Microsoft.Scripting;
using ComDispatch = Microsoft.Scripting.Actions.ComDispatch;
using Microsoft.Scripting.Hosting;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Types {

    public static class ComOps {
        [SpecialName]
        public static object GetBoundMember([StaticThis]object self, string name) {
            if (!ComObject.Is__ComObject(self.GetType())) {
                Debug.Assert(self.GetType().IsCOMObject); // // Strongly-typed RCWs are subtypes of System.__ComObject
                return PythonOps.NotImplemented; // The caller should fall back to other means to resolve the member
            }

            ComObject com = ComObject.ObjectToComObject(self);
            object value;
            if (com.TryGetAttr(DefaultContext.DefaultCLS, SymbolTable.StringToId(name), out value)) {
                return value;
            }
            return OperationFailed.Value;
        }

        [SpecialName]
        public static void SetMember([StaticThis]object self, string name, object value) {
            if (!ComObject.Is__ComObject(self.GetType())) {
                Debug.Assert(self.GetType().IsCOMObject); // Strongly-typed RCWs are subtypes of System.__ComObject
                PythonType pyType = DynamicHelpers.GetPythonTypeFromType(self.GetType());
                pyType.SetMember(DefaultContext.Default, self, SymbolTable.StringToId(name), value);
                return;
            }

            ComObject com = ComObject.ObjectToComObject(self);
            com.SetAttr(DefaultContext.Default, SymbolTable.StringToId(name), value);
            return;
        }

        [SpecialName]
        public static IList<SymbolId> GetMemberNames([StaticThis]object self) {
            List<SymbolId> ret = new List<SymbolId>();

            if (!ComObject.Is__ComObject(self.GetType())) {
                Debug.Assert(self.GetType().IsCOMObject); // Strongly-typed RCWs are subtypes of System.__ComObject
                Microsoft.Scripting.Actions.TypeTracker type = ReflectionCache.GetTypeTracker(self.GetType());
                IList<object> members = type.GetMemberNames(DefaultContext.Default);
                foreach (string memberName in members) {
                    ret.Add(SymbolTable.StringToId(memberName));
                }
                return ret;
            }

            if (self != null) {
                ComObject com = ComObject.ObjectToComObject(self);
                IList<SymbolId> subAttrNames = com.GetAttrNames(DefaultContext.Default);
                foreach (SymbolId id in subAttrNames) {
                    if (id == Symbols.Doc || id == Symbols.Module || id == Symbols.Init) continue;

                    if (!ret.Contains(id)) ret.Add(id);
                }
            }

            return ret; 
        }

        [SpecialName, PythonName("__repr__")]
        public static string ComObjectToString(object self) {
            if (!ComObject.Is__ComObject(self.GetType())) {
                Debug.Assert(self.GetType().IsCOMObject); // Strongly-typed RCWs are subtypes of System.__ComObject
                return self.ToString();
            }

            ComObject com = ComObject.ObjectToComObject(self);

            return com.ToString();
        }

        /// <summary>
        /// Operators and COM are tricky, it's too easy for us to get into 
        /// a path where we need code context but we only had default context.  Given
        /// that COM can't override __nonzero__ this prevents us from hitting one
        /// of those common paths (we need the context if we generate a typelib).
        /// </summary>
        [SpecialName, PythonName("__nonzero__")]
        public static bool IsNonZero(object self) {
            return true;
        }
    }

    class ComTypeBuilder : ReflectedTypeBuilder {
        private static PythonType _comType;

        public ComTypeBuilder() {
        }

        public static PythonType ComType {
            get {
                if (_comType == null) {
                    lock (typeof(ComTypeBuilder)) {
                        // TODO: ComOps needs to not use a type builder to be created, instead all of the op methods could be generated
                        // and it'd end this funny dance.
                        PythonType newType = new ComTypeBuilder().DoBuild("System.__ComObject", ComObject.comObjectType, null, ContextId.Empty);
                        if (Interlocked.CompareExchange<PythonType>(ref _comType, newType, null) == null) {
                            PythonType.SetPythonType(ComObject.comObjectType, newType);
                            PythonOps.ExtendOneType(new PythonExtensionTypeAttribute(ComObject.comObjectType, typeof(ComOps)), newType);

                            return newType;
                        }
                    }
                }

                return _comType;
            }
        }

        protected override void AddOps() {
            Dictionary<SymbolId, OperatorMapping> ops = PythonExtensionTypeAttribute._pythonOperatorTable;

            PythonType dt = Builder.UnfinishedType;
            foreach (KeyValuePair<SymbolId, OperatorMapping> op in ops) {
                if (op.Key == Symbols.GetBoundAttr) continue;

                if (op.Value.Operator == Operators.SetItem) {
                    AddTupleExpansionSetItem();
                } else if (op.Value.Operator == Operators.GetItem) {
                    AddTupleExpansionGetOrDeleteItem(Symbols.GetItem);
                } else if (op.Value.Operator == Operators.DeleteItem) {
                    AddTupleExpansionGetOrDeleteItem(Symbols.DelItem);
                } else {
                    UserTypeBuilder.AddLookupOperator(Builder.UnfinishedType, op.Key);
                }
            }

            base.AddOps();
        }

        private void AddTupleExpansionGetOrDeleteItem(SymbolId op) {
            Builder.AddOperator(PythonExtensionTypeAttribute._pythonOperatorTable[op].Operator,
                delegate(CodeContext context, object self, object other, out object ret) {
                    object func;
                    if(!Builder.UnfinishedType.TryGetMember(context, self, op, out func)) {
                        ret = null;
                        return false;
                    }

                    IParameterSequence t = other as IParameterSequence;
                    if (t != null && t.IsExpandable) {
                        ret = PythonOps.CallWithContext(context,
                            func,
                            t.Expand(null));
                        return true;
                    }
                    ret = PythonOps.CallWithContext(context,
                        func,
                        other);
                    return true;
                });
        }

        private void AddTupleExpansionSetItem() {
            Builder.AddOperator(Operators.SetItem,
                delegate(CodeContext context, object self, object value1, object value2, out object ret) {
                    object func;
                    if (!Builder.UnfinishedType.TryGetMember(context, self, Symbols.SetItem, out func)) {
                        ret = null;
                        return false;
                    }

                    IParameterSequence t = value1 as IParameterSequence;
                    if (t != null && t.IsExpandable) {
                        value1 = t.Expand(null);
                        ret = PythonOps.CallWithContext(context,
                            func,
                            t.Expand(value2));
                        return true;
                    }
                    ret = PythonOps.CallWithContext(context,
                        func,
                        value1,
                        value2);
                    return true;
                });
        }

    }

    /// <summary>
    /// This is a helper class for runtime-callable-wrappers of COM instances. We create one instance of this type
    /// for every generic RCW instance.
    /// </summary>
    public abstract class ComObject {
        private readonly object _comObject; // the runtime-callable wrapper

        private static readonly object _ComObjectInfoKey = (object)1; // use an int as the key since hashing an Int32.GetHashCode is cheap
        internal const string comObjectTypeName = "System.__ComObject";
        internal readonly static Type comObjectType = Type.GetType(comObjectTypeName);

        protected ComObject(object rcw) {
            Debug.Assert(Is__ComObject(rcw.GetType()));
            _comObject = rcw;
        }

        protected object Obj {
            get {
                return _comObject;
            }
        }

        /// <summary>
        /// This is the factory method to get the ComObject corresponding to an RCW
        /// </summary>
        /// <returns></returns>
        internal static ComObject ObjectToComObject(object rcw) {
            Debug.Assert(Is__ComObject(rcw.GetType()));

            // Marshal.Get/SetComObjectData has a LinkDemand for UnmanagedCode which will turn into
            // a full demand. We could avoid this by making this method SecurityCritical
            object data = Marshal.GetComObjectData(rcw, _ComObjectInfoKey);
            if (data != null) {
                return (ComObject)data;
            }

            lock (_ComObjectInfoKey) {
                data = Marshal.GetComObjectData(rcw, _ComObjectInfoKey);
                if (data != null) {
                    return (ComObject)data;
                }

                ComObject comObjectInfo = CreateComObject(rcw);
                if (!Marshal.SetComObjectData(rcw, _ComObjectInfoKey, comObjectInfo)) {
                    throw new COMException("Marshal.SetComObjectData failed");
                }

                return comObjectInfo;
            }
        }

        static ComObject CreateComObject(object rcw) {
            PythonEngineOptions engineOptions;
            engineOptions = PythonOps.GetLanguageContext().Options as PythonEngineOptions;
            ComDispatch.IDispatch dispatchObject = rcw as ComDispatch.IDispatch;
            if (engineOptions.PreferComDispatchOverTypeInfo && (dispatchObject != null)) {
                // We can do method invocations on IDispatch objects
                return new IDispatchObject(dispatchObject);
            }
            ComObject comObject;
            
            // First check if we can associate metadata with the COM object
            comObject = ComObjectWithTypeInfo.CheckClassInfo(rcw);
            if (comObject != null) {
                return comObject;
            }

            if (rcw is ComDispatch.IDispatch) {
                // We can do method invocations on IDispatch objects
                return new IDispatchObject(dispatchObject);
            } 

            // There is not much we can do in this case
            return new GenericComObject(rcw);
        }

        static internal bool Is__ComObject(Type type) {
                return type == comObjectType;
        }

        public override string ToString() {
            return _comObject.ToString();
        }

        #region ICustomMembers-like members
        abstract internal bool TryGetAttr(CodeContext context, SymbolId name, out object value);
        abstract internal void SetAttr(CodeContext context, SymbolId name, object value);
        abstract internal IList<SymbolId> GetAttrNames(CodeContext context);
        abstract internal IDictionary<object, object> GetAttrDict(CodeContext context);
        #endregion

    }

    /// <summary>
    /// We have no additional information about this COM object.
    /// </summary>
    public class GenericComObject : ComObject {
        internal GenericComObject(object rcw) : base(rcw) { }

        public override string ToString() {
            return "<System.__ComObject>";
        }

        #region ICustomMembers-like members

        override internal bool TryGetAttr(CodeContext context, SymbolId name, out object value) {
            value = null;
            return false;
        }

        override internal void SetAttr(CodeContext context, SymbolId name, object value) {
            throw PythonOps.AttributeErrorForMissingAttribute(ComTypeBuilder.ComType.Name, name);
        }

        override internal IList<SymbolId> GetAttrNames(CodeContext context) {
            return new List<SymbolId>();
        }

        override internal IDictionary<object, object> GetAttrDict(CodeContext context) {
            return new PythonDictionary(0);
        }

        #endregion
    }

    /// <summary>
    /// A COM object can expose its type information in two ways:
    /// 1. IProvideClassInfo
    /// 2. IDispatch.GetTypeInfo
    /// If we can determine the COM type, we can cast the object to this COM type to perform operations.
    ///
    /// Note that the CLR tries to associate strong type information (metadata) with COM objects if
    /// possible. ComObjectWithTypeInfo tries to provide similar functionality. However, it can try even
    /// harder.
    /// </summary>
    class ComObjectWithTypeInfo : GenericComObject {
        private PythonType _comInterface; // The COM type of the COM object
        private static Dictionary<Guid, Type> ComTypeCache = new Dictionary<Guid, Type>();

        private ComObjectWithTypeInfo(object rcw, Type comInterface) : base(rcw) {

            Debug.Assert(comInterface.IsInterface && comInterface.IsImport);
            _comInterface = DynamicHelpers.GetPythonTypeFromType(comInterface);
        }

        internal static ComObjectWithTypeInfo CheckClassInfo(object rcw) {
            ComObjectWithTypeInfo comObjectWithTypeInfo = CheckIProvideClassInfo(rcw);
            if (comObjectWithTypeInfo != null) {
                return comObjectWithTypeInfo;
            }

            return CheckIDispatchTypeInfo(rcw);
        }

        public override string ToString() {
            return "<System.__ComObject (" + _comInterface.Name + ")>";
        }

        #region ICustomMembers-like members

        override internal bool TryGetAttr(CodeContext context, SymbolId name, out object value) {
            return _comInterface.TryGetBoundMember(context, Obj, name, out value);
        }

        override internal void SetAttr(CodeContext context, SymbolId name, object value) {
            _comInterface.SetMember(context, Obj, name, value);
        }

        override internal IList<SymbolId> GetAttrNames(CodeContext context) {
            return _comInterface.GetMemberNames(context, Obj);
        }

        override internal IDictionary<object, object> GetAttrDict(CodeContext context) {
            return new PythonDictionary(_comInterface.GetMemberDictionary(context, Obj));
        }

        #endregion

        private static ComTypes.TYPEATTR GetITypeInfoAttr(IntPtr typeInfoPtr, out ComTypes.TYPELIBATTR typeLibAttr) {
            ComTypes.ITypeInfo typeInfo = Marshal.GetTypedObjectForIUnknown(typeInfoPtr, typeof(ComTypes.ITypeInfo)) as ComTypes.ITypeInfo;

            // First get the TYPEATTR

            IntPtr typeAttrPtr;
            typeInfo.GetTypeAttr(out typeAttrPtr);

            ComTypes.TYPEATTR typeAttr;
            try {
                typeAttr = (ComTypes.TYPEATTR)Marshal.PtrToStructure(typeAttrPtr, typeof(ComTypes.TYPEATTR));
            } finally {
                typeInfo.ReleaseTypeAttr(typeAttrPtr);
            }

            // Then get the TYPELIBATTR

            ComTypes.ITypeLib typeLib;
            int typeIndex;
            typeInfo.GetContainingTypeLib(out typeLib, out typeIndex);
            IntPtr typeLibAttrPtr;
            typeLib.GetLibAttr(out typeLibAttrPtr);
            try {
                typeLibAttr = (ComTypes.TYPELIBATTR)Marshal.PtrToStructure(typeLibAttrPtr, typeof(ComTypes.TYPELIBATTR));
            } finally {
                typeLib.ReleaseTLibAttr(typeLibAttrPtr);
            }

            return typeAttr;
        }

        private static Type ConvertTypeLibToAssembly(IntPtr typeInfoPtr, Guid typeInfoGuid) {
            if (ScriptDomainManager.Options.Verbose)
                Console.WriteLine("Generating Interop assembly for type " + typeInfoGuid);

            // This can be very slow. If this is taking a long time, you need to add a reference
            // to the Primary Interop Assembly using clr.AddReference
            Type interfaceType = Marshal.GetTypeForITypeInfo(typeInfoPtr);

            if (ScriptDomainManager.Options.Verbose) {
                if (interfaceType == null)
                    Console.WriteLine("Could not find COM interface " + typeInfoGuid);
                else
                    Console.WriteLine("Resulting type is " + interfaceType.AssemblyQualifiedName);
            }

            if (interfaceType == null) {
                return null;
            }

            // Publish the generated Interop assembly. Note that we should not be doing this 
            // if the PIA is already loaded since some GUIDs will be mapped to the PIA, and some will
            // get mapped to the generated Interop assembly, and it will lead to type-identity problems.
            // We ensure this by publishing all COM types whenever an assembly is loaded.
            PublishComTypes(interfaceType.Assembly);

            Debug.Assert(ComTypeCache.ContainsKey(typeInfoGuid));
            if (!ComTypeCache.ContainsKey(typeInfoGuid))
                throw new COMException("TypeLib " + interfaceType.Assembly + " does not contain COM interface + " + typeInfoGuid);

            return ComTypeCache[typeInfoGuid];
        }

        private static Type GetInterfaceForTypeInfo(IntPtr typeInfoPtr) {
            Debug.Assert(typeInfoPtr != IntPtr.Zero);

            ComTypes.TYPELIBATTR typeLibAttr;
            ComTypes.TYPEATTR typeInfoAttr = GetITypeInfoAttr(typeInfoPtr, out typeLibAttr);
            Guid typeInfoGuid = typeInfoAttr.guid;

            // Have we seen the Type Guid before in a previously-loaded assembly?

            Type interfaceType = null;

            if (ComTypeCache.TryGetValue(typeInfoGuid, out interfaceType)) {
                return interfaceType;
            }

            // Try to find a registered Primary Interop Assembly (PIA)

            TypeLibConverter tlc = new TypeLibConverter();
            string asmName = null, asmCodeBase = null;
            if (tlc.GetPrimaryInteropAssembly(
                    typeLibAttr.guid,
                    typeLibAttr.wMajorVerNum,
                    typeLibAttr.wMinorVerNum,
                    0,
                    out asmName,
                    out asmCodeBase)) {
                try {
                    Assembly interopAssembly = Assembly.Load(asmName);
                    PublishComTypes(interopAssembly);

                    Debug.Assert(ComTypeCache.ContainsKey(typeInfoGuid));
                    if (!ComTypeCache.ContainsKey(typeInfoGuid))
                        throw new COMException("TypeLib " + asmName + " does not contain COM interface + " + typeInfoGuid);
                    return ComTypeCache[typeInfoGuid];
                } catch (FileNotFoundException) { }
            }

            // Try creating an Interop assembly on the fly
            return ConvertTypeLibToAssembly(typeInfoPtr, typeInfoGuid);
        }

        private static void PublishComTypes(Assembly interopAssembly) {
            foreach (Type t in interopAssembly.GetTypes()) {
                if (t.IsImport && t.IsInterface) {
                    Type existing;
                    if (ComTypeCache.TryGetValue(t.GUID, out existing)) {
                        if (!existing.IsDefined(typeof(CoClassAttribute), false)) {
                            // prefer the type w/ CoClassAttribute on it.  Example:
                            //    MS.Office.Interop.Excel.Worksheet 
                            //          vs
                            //    MS.Office.Interop.Excel._Worksheet
                            //  Worksheet defines all the interfaces that the type supports and has CoClassAttribute.
                            //  _Worksheet is just the interface for the worksheet.
                            //
                            // They both have the same GUID though.
                            ComTypeCache[t.GUID] = t;
                        }
                    } else {
                        ComTypeCache[t.GUID] = t;
                    }
                }
            }
        }

        private static ComObjectWithTypeInfo CheckIDispatchTypeInfo(object rcw) {
            ComDispatch.IDispatch dispatch = rcw as ComDispatch.IDispatch;

            if (dispatch == null) {
                return null;
            }

            ComTypes.ITypeInfo typeInfo = IDispatchObject.GetITypeInfoFromIDispatch(dispatch, false);
            if (typeInfo == null) {
                return null;
            }

            IntPtr typeInfoPtr = IntPtr.Zero;
            ComObjectWithTypeInfo ret;
            try {
                typeInfoPtr = Marshal.GetIUnknownForObject(typeInfo);
                ret = CheckTypeInfo(rcw, typeInfoPtr);
            } finally {
                if (typeInfoPtr != IntPtr.Zero) {
                    Marshal.Release(typeInfoPtr);
                }
            }

            return ret;
        }

        private static ComObjectWithTypeInfo CheckIProvideClassInfo(object rcw) {
            ComDispatch.IProvideClassInfo provideClassInfo = rcw as ComDispatch.IProvideClassInfo;

            if (provideClassInfo == null) {
                return null;
            }

            IntPtr typeInfoPtr = IntPtr.Zero;
            ComObjectWithTypeInfo ret;
            try {
                provideClassInfo.GetClassInfo(out typeInfoPtr);

                ret = CheckTypeInfo(rcw, typeInfoPtr);
            } finally {
                if (typeInfoPtr != IntPtr.Zero) Marshal.Release(typeInfoPtr);
            }

            return ret;
        }

        private static ComObjectWithTypeInfo CheckTypeInfo(object rcw, IntPtr typeInfoPtr) {
            Type comInterface = GetInterfaceForTypeInfo(typeInfoPtr);
            if (comInterface != null) {
                // We have successfully found metadata for the COM object!
                return new ComObjectWithTypeInfo(rcw, comInterface);
            }

            return null;
        }
    }

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
    /// 2. Attempt to find COM object’s coclass’s description
    ///    a. Query the object for IProvideClassInfo interface. Go to 3, if found
    ///    b. From object’s IDispatch retrieve primary interface description
    ///    c. Scan coclasses declared in object’s type library.
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
    /// to IDispatch.Invoke and apply  custom logic – in particular we will
    /// just find and invoke the multicast delegate corresponding to the invoked
    /// dispid.
    ///  </summary>

    public class IDispatchObject : GenericComObject {

        private readonly ComDispatch.IDispatchObject _dispatchObject;

        public ComDispatch.ComTypeDesc _comTypeDesc;
        private static Dictionary<Guid, ComDispatch.ComTypeDesc> _CacheComTypeDesc;

        internal IDispatchObject(ComDispatch.IDispatch rcw) : base(rcw) {
            _dispatchObject = new ComDispatch.IDispatchObject(rcw);
        }

        public override string ToString() {

            EnsureScanDefinedFunctions();

            string typeName = this._comTypeDesc.TypeName;
            if (String.IsNullOrEmpty(typeName))
                typeName = "IDispatch";

            return String.Format("<System.__ComObject ({0})>", typeName);
        }

        #region HRESULT values returned by IDispatch::GetIDsOfNames and IDispatch::Invoke
        const int S_OK = 0;
        // The requested member does not exist, or the call to Invoke tried to set the value of a read-only property.
        const int DISP_E_MEMBERNOTFOUND = unchecked((int)0x80020003);
        // One or more of the names were not known
        const int DISP_E_UNKNOWNNAME = unchecked((int)0x80020006);
        // The number of elements provided in DISPPARAMS is different from the number of arguments accepted by the method or property.
        const int DISP_E_BADPARAMCOUNT = unchecked((int)0x8002000E);

        const int DISPID_VALUE = 0;
        const int DISPID_NEWENUM = -4;
        #endregion

        static int GetIDsOfNames(ComDispatch.IDispatch dispatch, SymbolId name, out int dispId) {
            int[] dispIds = new int[1];
            Guid emtpyRiid = Guid.Empty;
            int hresult = dispatch.TryGetIDsOfNames(
                ref emtpyRiid,
                new string[] { name.ToString() },
                1,
                0,
                dispIds);

            dispId = dispIds[0];
            return hresult;
        }

        static int Invoke(ComDispatch.IDispatch dispatch, int memberDispId, out object result) {
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

        #region ICustomMembers-like members

        override internal bool TryGetAttr(CodeContext context, SymbolId name, out object value) {

            // Check if the name exists
            EnsureScanDefinedFunctions();
            ComDispatch.ComMethodDesc methodDesc = null;

            // TODO: We have a thread-safety issue here right now
            // TODO: since we are mutating _funcs array
            // TODO: The workaround is to use Hashtable (which is thread-safe
            // TODO: on read operations) to fetch the value out.
            if (_comTypeDesc.Funcs.TryGetValue(name, out methodDesc) == false) {

                EnsureScanDefinedEvents();
                ComDispatch.ComEventDesc eventDesc;
                if (_comTypeDesc.Events.TryGetValue(name, out eventDesc)) {
                    value = new ComDispatch.BoundDispEvent(this.Obj, eventDesc.sourceIID, eventDesc.dispid);
                    return true;
                }

                int dispId;
                int hresult = GetIDsOfNames(_dispatchObject.DispatchObject, name, out dispId);
                if (hresult == DISP_E_UNKNOWNNAME) {
                    value = null;
                    return false;
                } else if (hresult != S_OK) {
                    throw PythonOps.AttributeError("Could not get DispId for {0} (error:0x{1:X})", name, hresult);
                }

                methodDesc = new ComDispatch.ComMethodDesc(name.ToString(), dispId);
                _comTypeDesc.Funcs.Add(name, methodDesc);
            }

            // There is a member with the given name.
            // It might be a property of a method.
            // 1. If this is a method we will return a callable object this defering the execution
            //    (notice that execution of parameterized properties is also defered)
            // 2. If this is a property - we will return the result of 
            //    invoking the property
            if (methodDesc != null && methodDesc.IsPropertyGet) {
                if (methodDesc.Parameters.Length == 0) {
                    value = new ComDispatch.DispMethod(_dispatchObject, methodDesc).CallAsProperty(context);
                } else {
                    value = new ComDispatch.DispIndexer(_dispatchObject, methodDesc);
                }
            } else {
                value = new ComDispatch.DispMethod(_dispatchObject, methodDesc);
            }

            return true;
        }

        override internal void SetAttr(CodeContext context, SymbolId name, object value) {

            if (value is ComDispatch.BoundDispEvent) {
                // CONSIDER: 
                // SetAttr on BoundDispEvent is the last operator that is called
                // during += on an actual event handler.
                // In practice, we can do nothing here and just ingore this operation.
                // But is it syntactically correct?
                return;
            }

            int dispId;
            int hresult = GetIDsOfNames(_dispatchObject.DispatchObject, name, out dispId);
            if (hresult == DISP_E_UNKNOWNNAME) {
                throw PythonOps.AttributeErrorForMissingAttribute(ComTypeBuilder.ComType.Name, name);
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

                typeof(ComDispatch.IDispatchForReflection).InvokeMember(
                    SymbolTable.IdToString(name),
                    bindingFlags,
                    Type.DefaultBinder,
                    Obj,
                    new object[1] { value }
                    );
            } catch (TargetInvocationException e) {
                // Unwrap the real (inner) exception and raise it
                throw ExceptionHelpers.UpdateForRethrow(e.InnerException);
            }
        }

        override internal IList<SymbolId> GetAttrNames(CodeContext context) {

            EnsureScanDefinedFunctions();
            EnsureScanDefinedEvents();
            // TODO: Need to filter out ComTypes.FUNCFLAGS.FUNCFLAG_FHIDDEN and ComTypes.FUNCFLAGS.FUNCFLAG_FNONBROWSABLE
            List<SymbolId> list = new List<SymbolId>(_comTypeDesc.Funcs.Keys);

            if (_comTypeDesc.Events != null && _comTypeDesc.Events.Count > 0) {
                list.AddRange(_comTypeDesc.Events.Keys);
            }

            return list;
        }

        override internal IDictionary<object, object> GetAttrDict(CodeContext context) {
            return new PythonDictionary(0);
        }

        #endregion

        internal static ComTypes.TYPEATTR GetTypeAttrForTypeInfo(ComTypes.ITypeInfo typeInfo) {
            IntPtr pAttrs = IntPtr.Zero;
            typeInfo.GetTypeAttr(out pAttrs);

            // GetTypeAttr should never return null, this is just to be safe
            if (pAttrs == IntPtr.Zero) {
                throw new COMException("ResolveComReference.CannotRetrieveTypeInformation");
            }

            try {
                return (ComTypes.TYPEATTR)Marshal.PtrToStructure(pAttrs, typeof(ComTypes.TYPEATTR));
            } finally {
                typeInfo.ReleaseTypeAttr(pAttrs);
            }
        }

        /// <summary>
        /// This method should be called when typeinfo is not available for an object. The function
        /// will check if the typeinfo is expected to be missing. This can include error cases where
        /// the same error is guaranteed to happen all the time, on all machines, under all circumstances.
        /// In such cases, we just have to operate without the typeinfo.
        /// 
        /// However, if accessing the typeinfo is failing in a transient way, we might want to throw
        /// an exception so that we will eagerly predictably indicate the problem.
        /// </summary>
        private static void CheckIfMissingTypeInfoIsExpected(int hresult, bool throwIfMissingExpectedTypeInfo) {
            Debug.Assert(!ComDispatch.ComHresults.IsSuccess(hresult));

            // Word.Basic always returns this because of an incorrect implementation of IDispatch.GetTypeInfo
            // Any implementation that returns E_NOINTERFACE is likely to do so in all environments
            if (hresult == ComDispatch.ComHresults.E_NOINTERFACE) {
                return;
            }

            // This assert is potentially over-restrictive since COM components can behave in quite unexpected ways.
            // However, asserting the common expected cases ensures that we find out about the unexpected scenarios, and
            // can investigate the scenarios to ensure that there is no bug in our own code.
            Debug.Assert(hresult == ComDispatch.ComHresults.TYPE_E_LIBNOTREGISTERED);

            if (throwIfMissingExpectedTypeInfo) {
                Marshal.ThrowExceptionForHR(hresult);
            }
        }

        /// <summary>
        /// Look for typeinfo using IDispatch.GetTypeInfo
        /// </summary>
        /// <param name="dispatch"></param>
        /// <param name="throwIfMissingExpectedTypeInfo">
        /// Some COM objects just dont expose typeinfo. In these cases, this method will return null.
        /// Some COM objects do intend to expose typeinfo, but may not be able to do so if the type-library is not properly 
        /// registered. This will be considered as acceptable or as an error condition depending on throwIfMissingExpectedTypeInfo</param>
        /// <returns></returns>
        internal static ComTypes.ITypeInfo GetITypeInfoFromIDispatch(ComDispatch.IDispatch dispatch, bool throwIfMissingExpectedTypeInfo) {
            uint typeCount;
            int hresult = dispatch.TryGetTypeInfoCount(out typeCount);
            Marshal.ThrowExceptionForHR(hresult);
            Debug.Assert(typeCount <= 1);
            if (typeCount == 0) {
                return null;
            }

            IntPtr typeInfoPtr = IntPtr.Zero;

            hresult = dispatch.TryGetTypeInfo(0, 0, out typeInfoPtr);
            if (!ComDispatch.ComHresults.IsSuccess(hresult)) {
                CheckIfMissingTypeInfoIsExpected(hresult, throwIfMissingExpectedTypeInfo);
                return null;
            }
            if (typeInfoPtr == IntPtr.Zero) { // be defensive against components that return IntPtr.Zero
                if (throwIfMissingExpectedTypeInfo) {
                    Marshal.ThrowExceptionForHR(ComDispatch.ComHresults.E_FAIL);
                }
                return null;
            }

            ComTypes.ITypeInfo typeInfo = null;
            try {
                typeInfo = Marshal.GetObjectForIUnknown(typeInfoPtr) as ComTypes.ITypeInfo;
            } finally {
                Marshal.Release(typeInfoPtr);
            }

            return typeInfo;
        }

        internal static void GetFuncDescForDescIndex(ITypeInfo typeInfo, int funcIndex, out ComTypes.FUNCDESC funcDesc, out IntPtr funcDescHandle) {
            IntPtr pFuncDesc = IntPtr.Zero;
            typeInfo.GetFuncDesc(funcIndex, out pFuncDesc);

            // GetFuncDesc should never return null, this is just to be safe
            if (pFuncDesc == IntPtr.Zero) {
                throw new COMException("ResolveComReference.CannotRetrieveTypeInformation");
            }

            funcDesc = (ComTypes.FUNCDESC)Marshal.PtrToStructure(pFuncDesc, typeof(ComTypes.FUNCDESC));
            funcDescHandle = pFuncDesc;
        }

        internal static SymbolId GetNameOfMethod(ITypeInfo typeInfo, int memid, string prefix) {
            int cNames;
            string[] rgNames = new string[1];
            typeInfo.GetNames(memid, rgNames, 1, out cNames);
            return SymbolTable.StringToId(prefix + rgNames[0]);
        }

        private void EnsureScanDefinedEvents() {

            // _comTypeDesc.Events is null if we have not yet attempted
            // to scan the object for events.
            if (_comTypeDesc != null && _comTypeDesc.Events != null) {
                return;
            }

            // check type info in the type descriptions cache
            ComTypes.ITypeInfo typeInfo = GetITypeInfoFromIDispatch(_dispatchObject.DispatchObject, true);
            if (typeInfo == null) {
                _comTypeDesc = ComDispatch.ComTypeDesc.CreateEmptyTypeDesc();
                return;
            }

            ComTypes.TYPEATTR typeAttr = GetTypeAttrForTypeInfo(typeInfo);

            if (_comTypeDesc == null) {
                if (_CacheComTypeDesc != null &&
                    _CacheComTypeDesc.TryGetValue(typeAttr.guid, out _comTypeDesc) == true &&
                    _comTypeDesc.Events != null) {
                    return;
                }
            }

            ComDispatch.ComTypeDesc typeDesc = new ComDispatch.ComTypeDesc(typeInfo);

            ComTypes.ITypeInfo classTypeInfo = null;
            Dictionary<SymbolId, ComDispatch.ComEventDesc> events = null;

            ComTypes.IConnectionPointContainer cpc = Obj as ComTypes.IConnectionPointContainer;
            if (cpc == null) {
                // No ICPC - this object does not support events
                events = ComDispatch.ComTypeDesc.EmptyEvents;
            } else if ((classTypeInfo = GetCoClassTypeInfo(this.Obj, typeInfo)) == null) {
                // no class info found - this object may support events
                // but we could not discover those
                Debug.Assert(false, "object support IConnectionPoint but no class info found");
                events = ComDispatch.ComTypeDesc.EmptyEvents;
            } else {
                events = new Dictionary<SymbolId, ComDispatch.ComEventDesc>();

                ComTypes.TYPEATTR classTypeAttr = GetTypeAttrForTypeInfo(classTypeInfo);
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
                    events = ComDispatch.ComTypeDesc.EmptyEvents;
                }
            }

            EnsureComTypeDescCache();

            lock (_CacheComTypeDesc) {
                ComDispatch.ComTypeDesc cachedTypeDesc;
                if (_CacheComTypeDesc.TryGetValue(typeAttr.guid, out cachedTypeDesc)) {
                    _comTypeDesc = cachedTypeDesc;
                } else {
                    _comTypeDesc = typeDesc;
                    _CacheComTypeDesc.Add(typeAttr.guid, _comTypeDesc);
                }

                _comTypeDesc.Events = events;
            }

        }

        private static void ScanSourceInterface(ComTypes.ITypeInfo sourceTypeInfo, ref Dictionary<SymbolId, ComDispatch.ComEventDesc> events) {
            ComTypes.TYPEATTR sourceTypeAttribute = GetTypeAttrForTypeInfo(sourceTypeInfo);

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
                    SymbolId name = GetNameOfMethod(sourceTypeInfo, funcDesc.memid, "Event_");

                    // Sometimes coclass has multiple source interfaces. Usually this is caused by
                    // adding new events and putting them on new interfaces while keeping the
                    // old interfaces around. This may cause name collisioning which we are
                    // resolving by keeping only the first event with the same name.
                    if (events.ContainsKey(name) == false) {
                        ComDispatch.ComEventDesc eventDesc = new ComDispatch.ComEventDesc();
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

            ComDispatch.IProvideClassInfo provideClassInfo = rcw as ComDispatch.IProvideClassInfo;
            if (provideClassInfo != null) {
                IntPtr typeInfoPtr = IntPtr.Zero;
                try {
                    provideClassInfo.GetClassInfo(out typeInfoPtr);
                    if (typeInfoPtr != IntPtr.Zero) {
                        return Marshal.GetObjectForIUnknown(typeInfoPtr) as ITypeInfo;
                    }
                } finally {
                    if (typeInfoPtr != IntPtr.Zero) {
                        Marshal.Release(typeInfoPtr);
                    }
                }
            }

            // retrieving class information through IPCI has failed - 
            // we can try scanning the typelib to find the coclass

            // TODO: Why we scan typelib every time we need to find events
            // TODO: Instead we might just keep a dictionary mapping 
            // TODO: coclass's guids to implemented interfaces guids.
            ComTypes.ITypeLib typeLib;
            int typeInfoIndex;
            typeInfo.GetContainingTypeLib(out typeLib, out typeInfoIndex);

            string typeName = ComDispatch.ComTypeDesc.GetNameOfType(typeInfo);

            // TODO: check that there are no 2 coclasses implementing same interface

            int countTypes = typeLib.GetTypeInfoCount();
            for (int i = 0; i < countTypes; i++) {
                ComTypes.TYPEKIND typeKind;
                typeLib.GetTypeInfoType(i, out typeKind);
                if (typeKind != ComTypes.TYPEKIND.TKIND_COCLASS)
                    continue;

                ComTypes.ITypeInfo classTypeInfo;
                typeLib.GetTypeInfo(i, out classTypeInfo);

                ComTypes.TYPEATTR classTypeAttr = GetTypeAttrForTypeInfo(classTypeInfo);

                for (int j = 0; j < classTypeAttr.cImplTypes; j++) {
                    int hRefType;
                    classTypeInfo.GetRefTypeOfImplType(j, out hRefType);
                    ComTypes.ITypeInfo currentTypeInfo;
                    classTypeInfo.GetRefTypeInfo(hRefType, out currentTypeInfo);

                    string currentTypeName = ComDispatch.ComTypeDesc.GetNameOfType(currentTypeInfo);
                    if (currentTypeName == typeName)
                        return classTypeInfo;
                }
            }

            return null;
        }

        private void EnsureComTypeDescCache() {
            if (_CacheComTypeDesc != null)
                return;

            lock(this.GetType()) {

                if (_CacheComTypeDesc != null)
                    return;

                _CacheComTypeDesc = new Dictionary<Guid, ComDispatch.ComTypeDesc>();
            }
        }


        private void EnsureScanDefinedFunctions() {
            if (_comTypeDesc != null && _comTypeDesc.Funcs != null)
                return;

            ComTypes.ITypeInfo typeInfo = GetITypeInfoFromIDispatch(_dispatchObject.DispatchObject, true);
            if (typeInfo == null) {
                _comTypeDesc = ComDispatch.ComTypeDesc.CreateEmptyTypeDesc();
                return;
            }

            ComTypes.TYPEATTR typeAttr = GetTypeAttrForTypeInfo(typeInfo);

            if (_comTypeDesc == null) {
                if (_CacheComTypeDesc != null &&
                    _CacheComTypeDesc.TryGetValue(typeAttr.guid, out _comTypeDesc) == true &&
                    _comTypeDesc.Funcs != null) {
                    return;
                }
            }

            ComDispatch.ComTypeDesc typeDesc = new ComDispatch.ComTypeDesc(typeInfo);

            Dictionary<SymbolId, ComDispatch.ComMethodDesc> funcs;
            funcs = new Dictionary<SymbolId, ComDispatch.ComMethodDesc>(typeAttr.cFuncs);

            for (int definedFuncIndex = 0; definedFuncIndex < typeAttr.cFuncs; definedFuncIndex++) {
                IntPtr funcDescHandleToRelease = IntPtr.Zero;

                try {
                    ComTypes.FUNCDESC funcDesc;
                    GetFuncDescForDescIndex(typeInfo, definedFuncIndex, out funcDesc, out funcDescHandleToRelease);

                    if ((funcDesc.wFuncFlags & (int)ComTypes.FUNCFLAGS.FUNCFLAG_FRESTRICTED) != 0) {
                        // This function is not meant for the script user to use.
                        continue;
                    }

                    ComDispatch.ComMethodDesc methodDesc;
                    SymbolId name;

                    // we do not need to store any info for property_put's as well - we will wait
                    // for corresponding property_get to come along.
                    if ((funcDesc.invkind & ComTypes.INVOKEKIND.INVOKE_PROPERTYPUT) != 0 ||
                        (funcDesc.invkind & ComTypes.INVOKEKIND.INVOKE_PROPERTYPUTREF) != 0) {
                        if (funcDesc.memid == DISPID_VALUE) {
                            methodDesc = new ComDispatch.ComMethodDesc(typeInfo, funcDesc);
                            name = SymbolTable.StringToId("__setitem__");
                            funcs.Add(name, methodDesc);
                        }
                        continue;
                    }

                    if (funcDesc.memid == DISPID_NEWENUM) {
                        methodDesc = new ComDispatch.ComMethodDesc(typeInfo, funcDesc);
                        name = SymbolTable.StringToId("GetEnumerator");
                        funcs.Add(name, methodDesc);
                        continue;
                    }

                    methodDesc = new ComDispatch.ComMethodDesc(typeInfo, funcDesc);
                    name = SymbolTable.StringToId(methodDesc.Name);
                    funcs.Add(name, methodDesc);

                    if (funcDesc.memid == DISPID_VALUE) {
                        methodDesc = new ComDispatch.ComMethodDesc(typeInfo, funcDesc);
                        name = SymbolTable.StringToId("__getitem__");
                        funcs.Add(name, methodDesc);
                    }

                } finally {
                    if (funcDescHandleToRelease != IntPtr.Zero) {
                        typeInfo.ReleaseFuncDesc(funcDescHandleToRelease);
                    }
                }
            }

            EnsureComTypeDescCache();

            lock (_CacheComTypeDesc) {
                ComDispatch.ComTypeDesc cachedTypeDesc;
                if (_CacheComTypeDesc.TryGetValue(typeAttr.guid, out cachedTypeDesc)) {
                    _comTypeDesc = cachedTypeDesc;
                } else {
                    _comTypeDesc = typeDesc;
                    _CacheComTypeDesc.Add(typeAttr.guid, _comTypeDesc);
                }

                _comTypeDesc.Funcs = funcs;
            }
        }
    }
}

#endif
