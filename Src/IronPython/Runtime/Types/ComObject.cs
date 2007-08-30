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
using Microsoft.Scripting.Types;
using Microsoft.Scripting;
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
                return PythonOps.NotImplemented;
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
                return;
            }

            ComObject com = ComObject.ObjectToComObject(self);
            com.SetAttr(DefaultContext.Default, SymbolTable.StringToId(name), value);
        }

        [SpecialName]
        public static IList<SymbolId> GetMemberNames([StaticThis]object self) {
            List<SymbolId> ret = new List<SymbolId>();

            if (!ComObject.Is__ComObject(self.GetType())) {
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
            if (!ComObject.Is__ComObject(self.GetType())) return self.ToString();  // subtype of ComObject

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
        private static DynamicType _comType;

        public ComTypeBuilder() {
        }

        public static DynamicType ComType {
            get {
                if (_comType == null) {
                    lock (typeof(ComTypeBuilder)) {
                        // TODO: ComOps needs to not use a type builder to be created, instead all of the op methods could be generated
                        // and it'd end this funny dance.
                        DynamicType newType = new ComTypeBuilder().DoBuild("System.__ComObject", ComObject.comObjectType, null, ContextId.Empty);
                        if (Interlocked.CompareExchange<DynamicType>(ref _comType, newType, null) == null) {
                            DynamicType.SetDynamicType(ComObject.comObjectType, newType);
                            DynamicHelpers.ExtendOneType(new PythonExtensionTypeAttribute(ComObject.comObjectType, typeof(ComOps)), newType, false);

                            return newType;
                        }
                    }
                }

                return _comType;
            }
        }

        protected override void AddOps() {
            foreach (Type ty in ComObject.comObjectType.GetInterfaces()) {
                InterfaceMapping mapping = ComObject.comObjectType.GetInterfaceMap(ty);
                for (int i = 0; i < mapping.TargetMethods.Length; i++) {
                    MethodInfo mi = mapping.TargetMethods[i];

                    if (mi == null) {
                        // COM objects can have interfaces that they don't appear
                        // to implement.  When that happens our target method is 
                        // null, but the interface method actually exists (we just need
                        // to QI for it).  For those we store the interfaces method
                        // directly into our dynamic type so the user can still call
                        // the appropriate method directly from the type.
                        Debug.Assert(ComObject.comObjectType.IsCOMObject);
                        
                        StoreReflectedMethod(mapping.InterfaceMethods[i].Name,
                            ContextId.Empty,
                            mapping.InterfaceMethods[i],
                            FunctionType.AlwaysVisible);
                        continue;
                    }
                }
            }

            Dictionary<SymbolId, OperatorMapping> ops = PythonExtensionTypeAttribute._pythonOperatorTable;

            DynamicType dt = Builder.UnfinishedType;
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
    internal abstract class ComObject {
        readonly object _comObject; // the runtime-callable wrapper
        private static WeakHash<object, ComObject> ComObjectHash = new WeakHash<object, ComObject>();
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

            ComObject res;
            if (ComObjectHash.TryGetValue(rcw, out res)) {
                Debug.Assert(rcw == res.Obj);
                return res;
            }

            lock (ComObjectHash) {
                if (ComObjectHash.TryGetValue(rcw, out res)) {
                    Debug.Assert(rcw == res.Obj);
                    return res;
                }

                res = CreateComObject(rcw);

                ComObjectHash[rcw] = res;
            }
            return res;
        }

        static ComObject CreateComObject(object rcw) {
            PythonEngineOptions engineOptions;
            engineOptions = PythonOps.GetLanguageContext().Engine.Options as PythonEngineOptions;
            if (engineOptions.PreferComDispatchOverTypeInfo && (rcw is IDispatch))
            {
                // We can do method invocations on IDispatch objects
                return new IDispatchObject(rcw);
            }
            ComObject comObject;
            
            // First check if we can associate metadata with the COM object
            comObject = ComObjectWithTypeInfo.CheckClassInfo(rcw);
            if (comObject != null) {
                return comObject;
            }

            if (rcw is IDispatch) {
                // We can do method invocations on IDispatch objects
                return new IDispatchObject(rcw);
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
    internal class GenericComObject : ComObject {
        internal GenericComObject(object rcw) : base(rcw) {}

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

    [
    ComImport,
    InterfaceType(ComInterfaceType.InterfaceIsIDispatch),
    Guid("00020400-0000-0000-C000-000000000046")
    ]
    interface IDispatchForReflection
    {
    }

    [
    ComImport,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("00020400-0000-0000-C000-000000000046")
    ]
    interface IDispatch {
        void GetTypeInfoCount(out uint pctinfo);

        void GetTypeInfo(uint iTInfo, int lcid, out IntPtr info);

        [PreserveSig]
        int GetIDsOfNames(
            ref Guid iid,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 2)]
            string[] names,
            uint cNames,
            int lcid,
            [Out]
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I4, SizeParamIndex = 2)]
            int[] rgDispId);

        [PreserveSig]
        int Invoke(
            int dispIdMember,
            ref Guid riid,
            int lcid,
            ushort wFlags,
            out ComTypes.DISPPARAMS pDispParams,
            out object VarResult,
            out ComTypes.EXCEPINFO pExcepInfo,
            out int puArgErr);
    }

    [
    ComImport,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("B196B283-BAB4-101A-B69C-00AA00341D07")
    ]
    interface IProvideClassInfo {
        void GetClassInfo(out IntPtr info);
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
        private DynamicType _comInterface; // The COM type of the COM object
        private static Dictionary<Guid, Type> ComTypeCache = new Dictionary<Guid, Type>();

        private ComObjectWithTypeInfo(object rcw, Type comInterface) : base(rcw) {

            Debug.Assert(comInterface.IsInterface && comInterface.IsImport);
            _comInterface = DynamicHelpers.GetDynamicTypeFromType(comInterface);
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

        const int TYPE_E_LIBNOTREGISTERED = unchecked((int)0x8002801D);

        private static ComObjectWithTypeInfo CheckIDispatchTypeInfo(object rcw) {
            IDispatch dispatch = rcw as IDispatch;

            if (dispatch == null) {
                return null;
            }

            uint typeCount;
            dispatch.GetTypeInfoCount(out typeCount);
            Debug.Assert(typeCount <= 1);
            if (typeCount == 0) {
                return null;
            }

            IntPtr typeInfoPtr = IntPtr.Zero;
            ComObjectWithTypeInfo ret;
            try {
                try {
                    dispatch.GetTypeInfo(0, 0, out typeInfoPtr);
                } catch (COMException e) {
                    // This must be a registration-free COM object
                    Debug.Assert(e.ErrorCode == TYPE_E_LIBNOTREGISTERED);
                    return null;
                }

                ret = CheckTypeInfo(rcw, typeInfoPtr);
            } finally {
                if (typeInfoPtr != IntPtr.Zero) Marshal.Release(typeInfoPtr);
            }

            return ret;
        }

        private static ComObjectWithTypeInfo CheckIProvideClassInfo(object rcw) {
            IProvideClassInfo provideClassInfo = rcw as IProvideClassInfo;

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
    /// 3. IronPython processes the signature and converts ref arguments into return values. 
    ///    However, since the signature of a DispMethod is not available beforehand, this conversion 
    ///    is not possible. There could be other signature conversions that may be affected. How does 
    ///    VB6 deal with ref arguments and IDispatch?
    /// </summary>
    class IDispatchObject : GenericComObject {

        private string _typeName;
        private Dictionary<SymbolId, ComDispatch.ComMethodDesc> _funcs;
        private static Dictionary<Guid, Dictionary<SymbolId, ComDispatch.ComMethodDesc>> _cacheComTypeInfo;

        internal IDispatchObject(object rcw)
            : base(rcw) {
            Debug.Assert(rcw is IDispatch);
        }

        public override string ToString() {
            
            EnsureScanDefinedFunctions();

            string typeName;
            if (String.IsNullOrEmpty(this._typeName))
                typeName = "IDispatch";
            else
                typeName = this._typeName;

            return String.Format("<System.__ComObject ({0})>", typeName);
        }

        IDispatch DispatchObject { get { return (IDispatch)Obj; } }

        #region HRESULT values returned by IDispatch::GetIDsOfNames and IDispatch::Invoke
        const int S_OK = 0;
        // The requested member does not exist, or the call to Invoke tried to set the value of a read-only property.
        const int DISP_E_MEMBERNOTFOUND = unchecked((int)0x80020003);
        // One or more of the names were not known
        const int DISP_E_UNKNOWNNAME = unchecked((int)0x80020006);
        // The number of elements provided in DISPPARAMS is different from the number of arguments accepted by the method or property.
        const int DISP_E_BADPARAMCOUNT = unchecked((int)0x8002000E);
        #endregion

        static int GetIDsOfNames(IDispatch dispatch, SymbolId name, out int dispId) {
            int[] dispIds = new int[1];
            Guid emtpyRiid = Guid.Empty;
            int hresult = dispatch.GetIDsOfNames(
                ref emtpyRiid,
                new string[] { name.ToString() },
                1,
                0,
                dispIds);

            dispId = dispIds[0];
            return hresult;
        }

        static int Invoke(IDispatch dispatch, int memberDispId, out object result) {
            Guid emtpyRiid = Guid.Empty;
            ComTypes.DISPPARAMS dispParams = new ComTypes.DISPPARAMS();
            ComTypes.EXCEPINFO excepInfo = new ComTypes.EXCEPINFO();
            int puArgErr;
            int hresult = dispatch.Invoke(
                memberDispId,
                ref emtpyRiid,
                0,
                (ushort)ComTypes.INVOKEKIND.INVOKE_PROPERTYGET,
                out dispParams,
                out result,
                out excepInfo,
                out puArgErr);

            return hresult;
        }

        #region ICustomMembers-like members

        override internal bool TryGetAttr(CodeContext context, SymbolId name, out object value) {
            // Check if the name exists

            EnsureScanDefinedFunctions();

            ComDispatch.ComMethodDesc methodDesc;

            // TODO: We have a thread-safety issue here right now
            // TODO: since we are mutating _funcs array
            // TODO: The workaround is to use Hashtable (which is thread-safe
            // TODO: on read operations) to fetch the value out.
            if (_funcs.TryGetValue(name, out methodDesc) == false) {
                int dispId;
                int hresult = GetIDsOfNames(DispatchObject, name, out dispId);
                if (hresult == DISP_E_UNKNOWNNAME) {
                    value = null;
                    return false;
                } else if (hresult != S_OK) {
                    throw PythonOps.AttributeError("Could not get DispId for {0} (error:0x{1:X})", name, hresult);
                }

                methodDesc = new ComDispatch.ComMethodDesc(name.ToString());
                _funcs.Add(name, methodDesc);
            }

            // There is a member with the given name.
            // It might be a property of a method.
            // 1. If this is a method we will return a callable object this defering the execution
            //    (notice that execution of parameterized properties is also defered)
            // 2. If this is a property - we will return the result of 
            //    invoking the property

            if (methodDesc != null && methodDesc.IsPropertyGet) {
                if (methodDesc.Parameters.Length == 0) {
                    value = new ComDispatch.DispMethod(DispatchObject, methodDesc).Call(context, ArrayUtils.EmptyObjects);
                } else {
                    value = new ComDispatch.DispIndexer(DispatchObject, methodDesc);
                }
            } else {
                value = new ComDispatch.DispMethod(DispatchObject, methodDesc);
            }

            return true;
        }

        override internal void SetAttr(CodeContext context, SymbolId name, object value) {
            int dispId;
            int hresult = GetIDsOfNames(DispatchObject, name, out dispId);
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

                typeof(IDispatchForReflection).InvokeMember(
                    SymbolTable.IdToString(name),
                    bindingFlags,
                    Type.DefaultBinder,
                    Obj,
                    new object[1] { value }
                    );
            } catch (Exception e) {
                if (e.InnerException != null) {
                    throw ExceptionHelpers.UpdateForRethrow(e.InnerException);
                }
                throw;
            }
        }

        override internal IList<SymbolId> GetAttrNames(CodeContext context) {

            EnsureScanDefinedFunctions();
            return new List<SymbolId>(_funcs.Keys);
        }

        override internal IDictionary<object, object> GetAttrDict(CodeContext context) {
            return new PythonDictionary(0);
        }

        #endregion

        internal static void GetTypeAttrForTypeInfo(ComTypes.ITypeInfo typeInfo, out ComTypes.TYPEATTR typeAttr) {
            IntPtr pAttrs = IntPtr.Zero;
            typeInfo.GetTypeAttr(out pAttrs);

            // GetTypeAttr should never return null, this is just to be safe
            if (pAttrs == IntPtr.Zero) {
                throw new COMException("ResolveComReference.CannotRetrieveTypeInformation");
            }

            try {
                typeAttr = (ComTypes.TYPEATTR)Marshal.PtrToStructure(pAttrs, typeof(ComTypes.TYPEATTR));
            } finally {
                typeInfo.ReleaseTypeAttr(pAttrs);
            }
        }

        internal static ComTypes.ITypeInfo GetITypeInfoFromIDispatch(IDispatch dispatch) {
            IntPtr pTypeInfo = IntPtr.Zero;
            dispatch.GetTypeInfo(0, 0, out pTypeInfo);

            ComTypes.ITypeInfo typeInfo = null;
            try {
                if (pTypeInfo != IntPtr.Zero)
                    typeInfo = Marshal.GetObjectForIUnknown(pTypeInfo) as ComTypes.ITypeInfo;
            } finally {
                if (pTypeInfo != IntPtr.Zero)
                    Marshal.Release(pTypeInfo);
                pTypeInfo = IntPtr.Zero;
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


        private void EnsureScanDefinedFunctions() {
            if (_funcs != null)
                return;

            ComTypes.ITypeInfo typeInfo = GetITypeInfoFromIDispatch(this.DispatchObject);

            ComTypes.TYPEATTR typeAttr;
            GetTypeAttrForTypeInfo(typeInfo, out typeAttr);

            // TODO: accessing _cacheComTypeInfo is currently not thread-safe
            // TODO: Suggested workaround is to use Hashtable
            if (_cacheComTypeInfo == null) {
                _cacheComTypeInfo = new Dictionary<Guid, Dictionary<SymbolId, IronPython.Runtime.Types.ComDispatch.ComMethodDesc> >();
            }  else if (_cacheComTypeInfo.TryGetValue(typeAttr.guid, out this._funcs) == true) {
                return;
            }

            string typeDocString;
            int dwHelpContext;
            string helpFile;
            typeInfo.GetDocumentation(-1, out this._typeName, out typeDocString, out dwHelpContext, out helpFile);

            Dictionary<SymbolId, ComDispatch.ComMethodDesc> funcs;
            funcs = new Dictionary<SymbolId, ComDispatch.ComMethodDesc>(typeAttr.cFuncs);

            for (int definedFuncIndex = 0; definedFuncIndex < typeAttr.cFuncs; definedFuncIndex++) {
                IntPtr funcDescHandleToRelease = IntPtr.Zero;

                try {
                    ComTypes.FUNCDESC funcDesc;
                    GetFuncDescForDescIndex(typeInfo, definedFuncIndex, out funcDesc, out funcDescHandleToRelease);

                    // we are not interested in hidden or restricted functions for now.
                    if ((funcDesc.wFuncFlags & (int)ComTypes.FUNCFLAGS.FUNCFLAG_FHIDDEN) != 0)
                        continue;
                    if ((funcDesc.wFuncFlags & (int)ComTypes.FUNCFLAGS.FUNCFLAG_FRESTRICTED) != 0)
                        continue;

                    ComDispatch.ComMethodDesc methodDesc;
                    SymbolId name;
                    // we do not need to store any info for property_put's as well - we will wait
                    // for corresponding property_get to come along.
                    if ((funcDesc.invkind & ComTypes.INVOKEKIND.INVOKE_PROPERTYPUT) != 0 ||
                        (funcDesc.invkind & ComTypes.INVOKEKIND.INVOKE_PROPERTYPUTREF) != 0)
                    {
                        if (funcDesc.memid == 0) //DISPID_VALUE
                        {
                            methodDesc = new ComDispatch.ComMethodDesc(typeInfo, funcDesc);
                            name = SymbolTable.StringToId("__setitem__");
                            funcs.Add(name, methodDesc);
                        }
                        continue;
                    }

                    methodDesc = new ComDispatch.ComMethodDesc(typeInfo, funcDesc);
                    name = SymbolTable.StringToId(methodDesc.Name);
                    funcs.Add(name, methodDesc);

                    if (funcDesc.memid == 0) //DISPID_VALUE
                    {
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

            _funcs = funcs;
            _cacheComTypeInfo.Add(typeAttr.guid, _funcs);
        }
    }
}

#endif
