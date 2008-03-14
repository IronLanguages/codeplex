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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

using Microsoft.Scripting.Runtime;

using ComTypes = System.Runtime.InteropServices.ComTypes;

namespace Microsoft.Scripting.Actions.ComDispatch {

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
        private Type _comType;
        private List<SymbolId> _comTypeMemberNames;
        private static Dictionary<Guid, Type> ComTypeCache = new Dictionary<Guid, Type>();

        private ComObjectWithTypeInfo(object rcw, Type comInterface)
            : base(rcw) {
            Debug.Assert(comInterface.IsInterface && comInterface.IsImport);
            _comType = comInterface;
        }

        internal static ComObjectWithTypeInfo CheckClassInfo(object rcw) {
            ComObjectWithTypeInfo comObjectWithTypeInfo = CheckIProvideClassInfo(rcw);
            if (comObjectWithTypeInfo != null) {
                return comObjectWithTypeInfo;
            }

            return CheckIDispatchTypeInfo(rcw);
        }

        public override string ToString() {
            return String.Format("{0} ({1})", Obj.ToString(), _comType.Name);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public override bool Equals(object obj) {
            return Obj.Equals(obj);
        }

        public override string Documentation {
            get { return _comType.FullName; }
        }

        public static string PropertyPutDefault {
            get { return "set__Default"; }
        }

        public static string PropertyGetDefault {
            get { return "get__Default"; }
        }

        #region IMembersList

        public override IList<SymbolId> GetMemberNames(CodeContext context) {
            if (_comTypeMemberNames == null) {
                InitializeMemberNames();
            }

            return _comTypeMemberNames;
        }

        private void InitializeMemberNames() {
            _comTypeMemberNames = new List<SymbolId>();

            InitializeMemberNames(_comType);
            RemoveDuplicates(_comTypeMemberNames);
        }

        private void RemoveDuplicates(List<SymbolId> list) {
            list.Sort();
            for (int i = list.Count - 2; i >= 0; --i) {
                if (list[i] == list[i + 1]) {
                    list.RemoveAt(i);
                    ++i;
                }
            }
        }

        private void InitializeMemberNames(Type type) {
            foreach (MemberInfo member in type.GetMembers()) {
                _comTypeMemberNames.Add(SymbolTable.StringToId(member.Name));
            }

            foreach (Type typeInterface in type.GetInterfaces()) {
                InitializeMemberNames(typeInterface);
            }
        }

        #endregion

        #region IDynamicObject

        public override StandardRule<T> GetRule<T>(DynamicAction action, CodeContext context, object[] args) {
            switch (action.Kind) {
                case DynamicActionKind.DoOperation:
                    return (new ComObjectWithTypeInfoDoOperationBinderHelper<T>(context, (DoOperationAction)action)).MakeNewRule();

                case DynamicActionKind.GetMember:
                    return (new ComObjectWithTypeInfoGetMemberBinderHelper<T>(context, _comType, (GetMemberAction)action, args)).MakeNewRule();

                case DynamicActionKind.SetMember:
                    return (new ComObjectWithTypeInfoSetMemberBinderHelper<T>(context, _comType, (SetMemberAction)action, args)).MakeNewRule();
            }

            return base.GetRule<T>(action, context, args);
        }

        #endregion

        internal static MemberInfo[] WalkType(Type type, string name) {
            MemberInfo[] foundMembers = type.GetMember(name);
            if (foundMembers != null && foundMembers.Length > 0) {
                return foundMembers;
            }

            foreach (Type subtype in type.GetInterfaces()) {
                MemberInfo[] foundSubMembers = WalkType(subtype, name);
                if (foundSubMembers != null && foundSubMembers.Length > 0) {
                    return foundSubMembers;
                }
            }

            return new MemberInfo[0];
        }

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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        private static Type ConvertTypeLibToAssembly(IntPtr typeInfoPtr, Guid typeInfoGuid) {
            if (ScriptDomainManager.Options.Verbose) {
                Console.WriteLine("Generating Interop assembly for type " + typeInfoGuid);
            }

            // This can be very slow. If this is taking a long time, you need to add a reference
            // to the Primary Interop Assembly using clr.AddReference
            Type interfaceType = Marshal.GetTypeForITypeInfo(typeInfoPtr);

            if (ScriptDomainManager.Options.Verbose) {
                if (interfaceType == null) {
                    Console.WriteLine("Could not find COM interface " + typeInfoGuid);
                } else {
                    Console.WriteLine("Resulting type is " + interfaceType.AssemblyQualifiedName);
                }
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
            if (!ComTypeCache.ContainsKey(typeInfoGuid)) {
                throw new COMException("TypeLib " + interfaceType.Assembly + " does not contain COM interface + " + typeInfoGuid);
            }

            return ComTypeCache[typeInfoGuid];
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
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
                    if (!ComTypeCache.ContainsKey(typeInfoGuid)) {
                        throw new COMException("TypeLib " + asmName + " does not contain COM interface + " + typeInfoGuid);
                    }
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

            ComTypes.ITypeInfo typeInfo = IDispatchComObject.GetITypeInfoFromIDispatch(dispatch, false);
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
}

#endif