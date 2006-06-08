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
using ComTypes = System.Runtime.InteropServices.ComTypes;
using Microsoft.Win32;

using System.Diagnostics;
using System.Threading;

namespace IronPython.Runtime {
    [
    ComImport,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("00020400-0000-0000-C000-000000000046")
    ]
    interface IDispatch {
        void GetTypeInfoCount(out uint pctinfo);
        void GetTypeInfo(uint iTInfo, int lcid, out IntPtr info);
        void GetIDsOfNames(Guid iid, string[] names, uint cNames, int lcid, out int[] rgDispId);
        void Invoke(int dispIdMember, Guid riid, int lcid, ushort wFlags,
                    ComTypes.DISPPARAMS[] pDispParams, out object VarResult,
                    out ComTypes.EXCEPINFO pExcepInfo, out int puArgErr);
    }

    /// <summary>
    /// This is the ReflectedType used for all generic RCW COM objects.
    /// </summary>
    class ComType : OpsReflectedType {
        private static ComType comType;
        internal readonly static string comObjectTypeName = "System.__ComObject";
        internal readonly static Type comObjectType = Type.GetType(comObjectTypeName);

        private ComType()
            : base(comObjectTypeName, comObjectType, typeof(ComType), null) {
        }

        internal static ReflectedType MakeDynamicType() {
            if (comType != null) return comType;
            lock (typeof(ComType)) {
                if (comType != null) return comType;
                comType = new ComType();
                return comType;
            }
        }

        #region DynamicType overrides

        public override string Repr(object self) {
            ComObject com = ComObject.ObjectToComObject(self);
            return com.ToString();
        }

        public override bool TryGetAttr(ICallerContext context, object self, SymbolId name, out object ret) {
            if (base.TryGetAttr(context, self, name, out ret))
                return true;

            ComObject com = ComObject.ObjectToComObject(self);
            return com.TryGetAttr(context, name, out ret);           
        }

        public override object GetAttr(ICallerContext context, object self, SymbolId name) {
            object ret;
            if (TryGetAttr(context, self, name, out ret))
                return ret;
            throw Ops.AttributeErrorForMissingAttribute(ToString(), name);
        }

        public override void SetAttr(ICallerContext context, object self, SymbolId name, object value) {
            ComObject com = ComObject.ObjectToComObject(self);
            com.SetAttr(context, name, value);
        }

        public override List GetAttrNames(ICallerContext context, object self) {
            List ret = base.GetAttrNames(context, self);

            ComObject com = ComObject.ObjectToComObject(self);
            ret = ret.AddList(com.GetAttrNames(context));

            return ret;
        }

        public override Dict GetAttrDict(ICallerContext context, object self) {
            ComObject com = ComObject.ObjectToComObject(self);
            return new Dict(com.GetAttrDict(context));
        }

        #endregion
    }

    /// <summary>
    /// This is a helper class for runtime-callable-wrappers of COM instances. We create one instance of this type
    /// for every generic RCW instance.
    /// </summary>
    internal class ComObject {
        private readonly object obj; // the runtime-callable wrapper
        private ArrayList interfaces;
        private int initialized;

        private static Dictionary<Guid, Type> ComTypeCache = new Dictionary<Guid, Type>();
        private static WeakHash<object, ComObject> ComObjectHash = new WeakHash<object, ComObject>();

        public ComObject(object rcw) {
            Debug.Assert(ComObject.IsGenericRuntimeCallableWrapper(rcw));
            obj = rcw;
        }

        public object Obj {
            get {
                return obj;
            }
        }

        public static ComObject ObjectToComObject(object rcw)
        {
            Debug.Assert(ComObject.IsGenericRuntimeCallableWrapper(rcw));
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

                res = new ComObject(rcw);
                ComObjectHash[rcw] = res;
            }
            return res;
        }

        /// <summary>
        /// Is the object a Runtime-callable-wrapper for a COM object, and without a type library?
        /// If the CLR can locate a type library (if the COM object implements IProvideClassInfo),
        /// then the CLR marshaller wraps the COM object in a strongly typed wrapper,
        /// rather than using the generic __ComObject type.
        /// </summary>
        static internal bool IsGenericRuntimeCallableWrapper(object o) {
            return Is__ComObject(o.GetType());
        }

        static internal bool Is__ComObject(Type type) {
            return type == ComType.comObjectType;
        }

        private bool HaveInterfaces {
            get {
                Debug.Assert(initialized != 0);
                return interfaces != null && interfaces.Count > 0;
            }
        }

        public override string ToString() {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("<" + ComType.comObjectTypeName + " ");
            if (initialized == 0) {
                // interfaces are lazily initialized
                sb.Append(" uninitialized>");
            } else if (HaveInterfaces) {
                sb.Append(" with interfaces [");
                bool space = false;
                foreach (DynamicType dt in interfaces) {
                    if (space) sb.Append(" ");
                    sb.Append(dt.ToString());
                    space = true;
                }
                sb.Append("]>");
            } else {
                sb.Append(">");
            }
            return sb.ToString();
        }


        #region ICustomAttributes-like members

        internal bool TryGetAttr(ICallerContext context, SymbolId name, out object value) {
            Initialize(context);

            if (HaveInterfaces) {
                foreach (DynamicType type in interfaces) {
                    if (type.TryGetAttr(context, obj, name, out value)) {
                        return true;
                    }
                }
            } else {
                try {
                    object result = Obj.GetType().InvokeMember(
                        (string)SymbolTable.IdToString(name),
                        System.Reflection.BindingFlags.GetProperty |
                        System.Reflection.BindingFlags.GetField,
                        Type.DefaultBinder,
                        Obj,
                        new object[0]
                        );
                    value = Ops.ToPython(result);
                    return true;
                } catch (System.Reflection.TargetInvocationException e) {
                    COMException comex = e.InnerException as COMException;
                    if (comex != null) {
                        // The TargetInvocationException with a nested COMException indicates that the COM object does 
                        // have a method with this name. However, COM does not allow accessing it as a field.
                        // So we create a bound method object to represent the COM method.
                        value = new ComMethod(this, SymbolTable.IdToString(name));
                        return true;
                    }
                } catch {
                }
            }

            value = null;
            return false;
        }

        internal void SetAttr(ICallerContext context, SymbolId name, object value) {
            Initialize(context);

            if (HaveInterfaces) {
                foreach (DynamicType type in interfaces) {
                    try {
                        type.SetAttr(context, obj, name, value);
                        return;
                    } catch {
                    }
                }
                throw Ops.AttributeErrorForMissingAttribute(ComType.MakeDynamicType().__name__.ToString(), name);
            } else {
                try {
                    Obj.GetType().InvokeMember(
                        (string)SymbolTable.IdToString(name),
                        System.Reflection.BindingFlags.SetProperty |
                        System.Reflection.BindingFlags.SetField,
                        Type.DefaultBinder,
                        Obj,
                        new object[1] { value }
                        );
                } catch (Exception e) {
                    if (e.InnerException != null) {
                        throw ExceptionConverter.UpdateForRethrow(e.InnerException);
                    }
                    throw;
                }
            }
        }

        internal void DeleteAttr(ICallerContext context, SymbolId name) {
            Initialize(context);

            throw new NotSupportedException();
        }

        internal List GetAttrNames(ICallerContext context) {
            Initialize(context);

            List list = List.Make();
            if (HaveInterfaces) {
                foreach (DynamicType type in interfaces) {
                    List names = type.GetAttrNames(context, obj);
                    list.AppendListNoLockNoDups(names);
                }
            } else {
                //return GetDynamicType().GetAttrNames(context, this);
            }
            return list;
        }

        internal IDictionary<object, object> GetAttrDict(ICallerContext context) {
            if (HaveInterfaces) {
                Dict res = new Dict();
                foreach (DynamicType type in interfaces) {
                    Dict dict = type.GetAttrDict(context, obj);
                    foreach (KeyValuePair<object, object> val in dict) {
                        if (!res.ContainsKey(val.Key)) {
                            res.Add(val);
                        }
                    }
                }
                return res;
            } else {
                return new Dict(0);
            }
        }

        #endregion

        private static Guid GetITypeInfoGuid(IntPtr typeInfoPtr) {
            IntPtr typeAttrPtr;
            ComTypes.TYPEATTR typeAttr;
            ComTypes.ITypeInfo typeInfo;

            typeInfo = Marshal.GetTypedObjectForIUnknown(typeInfoPtr, typeof(ComTypes.ITypeInfo)) as ComTypes.ITypeInfo;

            typeInfo.GetTypeAttr(out typeAttrPtr);
            try {
                typeAttr = (ComTypes.TYPEATTR)Marshal.PtrToStructure(typeAttrPtr, typeof(ComTypes.TYPEATTR));
            } finally {
                typeInfo.ReleaseTypeAttr(typeAttrPtr);
            }

            return typeAttr.guid;
        }

        private static Dictionary<Type, IList<Type>> s_hiddenInterfaces = new Dictionary<Type, IList<Type>>();

        /// <summary>
        /// Gets the list of hidden interfaces.  Users of this object must lock
        /// on it before accessing for either reads or writes.
        /// </summary>
        internal static Dictionary<Type, IList<Type>> HiddenInterfaces { get { return s_hiddenInterfaces; } }

        private void Initialize(ICallerContext context) {
            int prevVal = Interlocked.CompareExchange(ref initialized, 1, 0);
            if (prevVal == 2) return;   // fully initialized, leave...

            if (prevVal != 0) {
                while (Thread.VolatileRead(ref initialized) != 2) {
                    Thread.SpinWait(10000);
                }
                return;
            }

            try {
                AddInterfaces(context);
                Interlocked.Exchange(ref initialized, 2);
            } catch {
                // initialization failed.
                Interlocked.Exchange(ref initialized, 0);
                throw;
            }
        }

        private static Type GetInterfaceForGuid(ICallerContext context, Guid typeInfoGuid, IntPtr typeInfoPtr) {
            Type interfaceType;
            if (ComTypeCache.TryGetValue(typeInfoGuid, out interfaceType) == false) {
                Assembly interopAssembly = SearchForInteropAssembly(typeInfoGuid);

                if (interopAssembly != null) {
                    context.SystemState.TopPackage.LoadAssembly(context.SystemState, interopAssembly, false);
                    ComTypeCache.TryGetValue(typeInfoGuid, out interfaceType);
                }

                if (interfaceType == null && typeInfoPtr != IntPtr.Zero) {
                    // This can be very slow. Hence we call SearchForInteropAssembly before we do this.
                    interfaceType = Marshal.GetTypeForITypeInfo(typeInfoPtr);

                    if (interfaceType != null) {
                        ComTypeCache[typeInfoGuid] = interfaceType;
                    }
                }
            }

            return interfaceType;
        }

        private void AddInterfaces(ICallerContext context, Guid typeInfoGuid, IntPtr typeInfoPtr)
        {
            Type interfaceType = ComObject.GetInterfaceForGuid(context, typeInfoGuid, typeInfoPtr);
            if (interfaceType == null)
                return;

            List<Type> rootInterfaces = new List<Type>();
            rootInterfaces.Add(interfaceType);

            lock (s_hiddenInterfaces) {
                if (s_hiddenInterfaces.ContainsKey(interfaceType))
                    rootInterfaces.AddRange(s_hiddenInterfaces[interfaceType]);
            }

            List<Type> interfaces = new List<Type>();
            foreach (Type rootInterface in rootInterfaces) {
                if (!rootInterface.IsInstanceOfType(Obj))
                    return;
                interfaces.Add(rootInterface);
                foreach (Type extendedInterface in rootInterface.GetInterfaces())
                    if (!interfaces.Contains(extendedInterface))
                        interfaces.Add(extendedInterface);
            }

            foreach (Type i in interfaces)
                AddInterface(i);
        }

        const int TYPE_E_LIBNOTREGISTERED = unchecked((int)0x8002801D);

        private void AddInterfaces(ICallerContext context) {
            IDispatch dispatch = obj as IDispatch;

            if (dispatch == null) {
                // We have to treat it just as __ComObject
                AddInterfaces(context, obj.GetType().GUID, new IntPtr());
                return;
            }

            uint typeCount;
            dispatch.GetTypeInfoCount(out typeCount);
            if (typeCount > 0) {
                this.interfaces = new ArrayList();
                for (uint index = 0; index < typeCount; index++) {
                    IntPtr typeInfoPtr;
                    try {
                        dispatch.GetTypeInfo(index, 0, out typeInfoPtr);
                    } catch (COMException e) {
                        // This must be a registration-free COM object
                        Debug.Assert(e.ErrorCode == TYPE_E_LIBNOTREGISTERED);
                        continue;
                    }

                    try {
                        Guid guid = GetITypeInfoGuid(typeInfoPtr);
                        AddInterfaces(context, guid, typeInfoPtr);
                    } finally {
                        Marshal.Release(typeInfoPtr);
                    }
                }
            } else {
                // We have to treat it just as __ComObject
                AddInterfaces(context, obj.GetType().GUID, new IntPtr());
            }
        }

        /// <summary>
        /// Try to search the GAC for an assembly that seems to match the given type GUID
        /// </summary>
        private static Assembly SearchForInteropAssembly(Guid typeInfoGuid) {
            Assembly assembly = null;

            try {
                string typelibPath = @"Interface\{" + typeInfoGuid + @"}\TypeLib";
                RegistryKey typelibKey = Registry.ClassesRoot.OpenSubKey(typelibPath);
                if (typelibKey == null)
                    return null;

                // Read HKCR\Interface\{<typeInfoGuid>}\TypeLib\(Default)"
                Guid typelibGuid = new Guid(typelibKey.GetValue(null, null) as string);

                // Read HKCR\Interface\{<typeInfoGuid>}\TypeLib\Version"
                string typelibVersion = typelibKey.GetValue("Version") as String;
                if (typelibVersion == null)
                    return null;

                string[] versionSplit = typelibVersion.Split('.');
                int major = int.Parse(versionSplit[0]);
                int minor = int.Parse(versionSplit[1]);

                // Try using TypeLibConverter
                TypeLibConverter tlc = new TypeLibConverter();
                string asmName = null, asmCodeBase = null;
                tlc.GetPrimaryInteropAssembly(typelibGuid, major, minor, 0, out asmName, out asmCodeBase);

                if (asmName != null)
                    assembly = Assembly.Load(asmName);

                if (assembly != null)
                    return assembly;

                // Next, try looking up the GAC
                assembly = SearchForInteropAssemblyInGAC(typeInfoGuid, major, minor);

            } catch (Exception e) {
                if (IronPython.Hosting.PythonEngine.options.EngineDebug)
                    throw e;
            }

            return assembly;
        }

        /// <summary>
        /// Search assemblies in the GAC for a type with the same GUID as "typeInfoGuid"
        /// </summary>
        private static Assembly SearchForInteropAssemblyInGAC(Guid typeInfoGuid, int major, int minor) {
            string systemRoot = Environment.GetEnvironmentVariable("SystemRoot");
            string[] files = Directory.GetFiles(systemRoot + @"\assembly\gac", "*.dll", SearchOption.AllDirectories);
            string regex = systemRoot + @"\assembly\gac\[^\]*\{0}.{1}";
            regex = regex.Replace(@"\", @"\\");
            regex = string.Format(regex, major, minor);
            foreach (string file in files) {
                if (Regex.IsMatch(file, regex)) {
                    MetadataImport mdImport = new MetadataImport(file);
                    System.Collections.Generic.List<uint> types = mdImport.GetTypes();

                    foreach (uint tkType in types) {
                        if (mdImport.GetGuidCA(tkType) != typeInfoGuid)
                            continue;

                        AssemblyName name = new AssemblyName();
                        name.Name = Path.GetFileNameWithoutExtension(file);
                        string regexGac = regex + @"[^_]*__(?<publicKeyToken>[^\\]*)";
                        string match =
                            Regex.Match(file, regexGac).Groups["publicKeyToken"].Value;
                        byte[] publicKeyToken = new byte[match.Length / 2];
                        for (int i = 0; i < publicKeyToken.Length; i++) {
                            string b = "" + match[i * 2] + match[i * 2 + 1];
                            publicKeyToken[i] = byte.Parse(b, NumberStyles.HexNumber);
                        }
                        name.SetPublicKeyToken(publicKeyToken);
                        name.CultureInfo = CultureInfo.InvariantCulture;

                        return Assembly.Load(name);
                    }
                }

            }

            return null;
        }

        public void AddInterface(Type type) {
            interfaces.Add(Ops.GetDynamicTypeFromType(type));
        }

        internal static void AddType(Guid guid, Type type) {
            object[] attributes = type.GetCustomAttributes(typeof(ComImportAttribute), true);
            if (attributes.Length > 0) {
                if (!ComTypeCache.ContainsKey(guid)) {
                    ComTypeCache[guid] = type;
                }
            }
        }        
    }

    /// <summary>
    /// This represents a bound method on a COM object.
    /// </summary>
    class ComMethod : ICallable {
        private readonly object obj;
        private readonly string name;

        public ComMethod(ComObject o, string n) {
            obj = o.Obj;
            name = n;
        }

        public override string ToString() {
            return String.Format("<com_method {0} on {1}", name, obj.ToString());
        }

        public object Call(params object[] args) {
            try {
                object ret = obj.GetType().InvokeMember(
                    name,
                    System.Reflection.BindingFlags.InvokeMethod,
                    Type.DefaultBinder,
                    obj,
                    args
                    );
                return Ops.ToPython(ret);
            } catch (Exception e) {
                if (e.InnerException != null) {
                    throw ExceptionConverter.UpdateForRethrow(e.InnerException);
                }
                throw;
            }
        }
    }
}

namespace IronPython.Runtime {
    [ComImport, GuidAttribute("31BCFCE2-DAFB-11D2-9F81-00C04F79A0A3"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMetaDataDispenserEx {
        uint DefineScope(ref Guid rclsid, uint dwCreateFlags, ref Guid riid, [MarshalAs(UnmanagedType.Interface)]out object ppIUnk);
        uint OpenScope([MarshalAs(UnmanagedType.LPWStr)]string szScope, uint dwOpenFlags, ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out object ppIUnk);
        uint OpenScopeOnMemory(IntPtr pData, uint cbData, uint dwOpenFlags, ref Guid riid, [MarshalAs(UnmanagedType.Interface)]out object ppIUnk);
        uint SetOption(ref Guid optionid, [MarshalAs(UnmanagedType.Struct)]object value);
        uint GetOption(ref Guid optionid, [MarshalAs(UnmanagedType.Struct)]out object pvalue);
        uint OpenScopeOnITypeInfo([MarshalAs(UnmanagedType.Interface)]ITypeInfo pITI, uint dwOpenFlags, ref Guid riid, [MarshalAs(UnmanagedType.Interface)]out object ppIUnk);
        uint GetCORSystemDirectory([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)]char[] szBuffer, uint cchBuffer, out uint pchBuffer);
        uint FindAssembly([MarshalAs(UnmanagedType.LPWStr)]string szAppBase, [MarshalAs(UnmanagedType.LPWStr)]string szPrivateBin, [MarshalAs(UnmanagedType.LPWStr)]string szGlobalBin, [MarshalAs(UnmanagedType.LPWStr)]string szAssemblyName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)]char[] szName, uint cchName, out uint pcName);
        uint FindAssemblyModule([MarshalAs(UnmanagedType.LPWStr)]string szAppBase, [MarshalAs(UnmanagedType.LPWStr)]string szPrivateBin, [MarshalAs(UnmanagedType.LPWStr)]string szGlobalBin, [MarshalAs(UnmanagedType.LPWStr)]string szAssemblyName, [MarshalAs(UnmanagedType.LPWStr)]string szModuleName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 5)]char[] szName, uint cchName, out uint pcName);
    }

    [ComImport, GuidAttribute("E5CB7A31-7512-11D2-89CE-0080C792E5D8")]
    public class CorMetaDataDispenserExClass { }

    [ComImport, GuidAttribute("31BCFCE2-DAFB-11D2-9F81-00C04F79A0A3"), CoClass(typeof(CorMetaDataDispenserExClass))]
    public interface MetaDataDispenserEx : IMetaDataDispenserEx { }

    [ComImport, GuidAttribute("7DAC8207-D3AE-4c75-9B67-92801A497D44"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMetaDataImport {
        void CloseEnum(uint hEnum);
        uint CountEnum(uint hEnum, out uint count);
        uint ResetEnum(uint hEnum, uint ulPos);
        uint EnumTypeDefs(ref uint phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]uint[] rTypeDefs, uint cMax, out uint pcTypeDefs);
        uint EnumInterfaceImpls(ref uint phEnum, uint td, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)]uint[] rImpls, uint cMax, out uint pcImpls);
        uint EnumTypeRefs(ref uint phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]uint[] rTypeDefs, uint cMax, out uint pcTypeRefs);
        uint FindTypeDefByName([MarshalAs(UnmanagedType.LPWStr)]string szTypeDef, uint tkEnclosingClass, out uint ptd);
        uint GetScopeProps([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)]char[] szName, uint cchName, out uint pchName, ref Guid pmvid);
        uint GetModuleFromScope(out uint pmd);
        uint GetTypeDefProps(uint td, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]char[] szTypeDef, uint cchTypeDef, out uint pchTypeDef, out uint pdwTypeDefFlags, out uint ptkExtends);
        uint GetInterfaceImplProps(uint iiImpl, out uint pClass, out uint ptkIface);
        uint GetTypeRefProps(uint tr, out uint ptkResolutionScope, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)]char[] szName, uint cchName, out uint pchName);
        uint ResolveTypeRef(uint tr, ref Guid riid, [MarshalAs(UnmanagedType.Interface)]out object ppIScope, out uint ptd);
        uint EnumMembers(ref uint phEnum, uint cl, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)]uint[] rMembers, uint cMax, out uint pcTokens);
        uint EnumMembersWithName(ref uint phEnum, uint cl, [MarshalAs(UnmanagedType.LPWStr)]string szName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)]uint[] rMembers, uint cMax, out uint pcTokens);
        uint EnumMethods(ref uint phEnum, uint cl, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)]uint[] rMethods, uint cMax, out uint pcTokens);
        uint EnumMethodsWithName(ref uint phEnum, uint cl, [MarshalAs(UnmanagedType.LPWStr)]string szName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)]uint[] rMethods, uint cMax, out uint pcTokens);
        uint EnumFields(ref uint phEnum, uint cl, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)]uint[] rFields, uint cMax, out uint pcTokens);
        uint EnumFieldsWithName(ref uint phEnum, uint cl, [MarshalAs(UnmanagedType.LPWStr)]string szName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)]uint[] rFields, uint cMax, out uint pcTokens);
        uint EnumParams(ref uint phEnum, uint mb, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)]uint[] rParams, uint cMax, out uint pcTokens);
        uint EnumMemberRefs(ref uint phEnum, uint tkParent, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)]uint[] rMemberRefs, uint cMax, out uint pcTokens);
        uint EnumMethodImpls(ref uint phEnum, uint td, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]uint[] rMethodBody, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)]uint[] rMethodDecl, uint cMax, out uint pcTokens);
        uint EnumPermissionSets(ref uint phEnum, uint tk, uint dwActions, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)]uint[] rPermission, uint cMax, out uint pcTokens);
        uint FindMember(uint td, [MarshalAs(UnmanagedType.LPWStr)]string szName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]byte[] pvSigBlob, uint cbSigBlob, out uint pmb);
        uint FindMethod(uint td, [MarshalAs(UnmanagedType.LPWStr)]string szName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]byte[] pvSigBlob, uint cbSigBlob, out uint pmb);
        uint FindField(uint td, [MarshalAs(UnmanagedType.LPWStr)]string szName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]byte[] pvSigBlob, uint cbSigBlob, out uint pmb);
        uint FindMemberRef(uint td, [MarshalAs(UnmanagedType.LPWStr)]string szName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]byte[] pvSigBlob, int cbSigBlob, out uint pmr);
        uint GetMethodProps(uint mb, out uint pClass, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]char[] szMethod, uint cchMethod, out uint pchMethod, out uint pdwAttr, out IntPtr ppvSigBlob, out uint pcbSigBlob, out uint pulCodeRVA, out uint pdwImplFlags);
        uint GetMemberRefProps(uint mr, out uint ptk, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]char[] szMember, uint cchMember, out uint pchMember, out IntPtr ppvSigBlob, out uint pbSigBlob);
        uint EnumProperties(ref uint phEnum, uint td, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]uint[] rProperties, uint cMax, out uint pcProperties);
        uint EnumEvents(ref uint phEnum, uint td, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]uint[] rEvents, uint cMax, out uint pcEvents);
        uint GetEventProps(uint ev, out uint pClass, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]char[] szEvent, uint cchEvent, out uint pchEvent, out uint pdwEventFlags, out uint ptkEventType, out uint pmdAddOn, out uint pmdRemoveOn, out uint pmdFire, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 10)]uint[] rmdOtherMethod, uint cMax, out uint pcOtherMethod);
        uint EnumMethodSemantics(ref uint phEnum, uint mb, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]uint[] rEventProp, uint cMax, out uint pcEventProp);
        uint GetMethodSemantics(uint mb, uint tkEventProp, out uint pdwSemanticsFlags);
        uint GetClassLayout(uint td, out uint pdwPackSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]long[] rFieldOffset, uint cMax, out uint pcFieldOffset, out uint pulClassSize);
        uint GetFieldMarshal(uint tk, out IntPtr ppvNativeType, out uint pcbNativeType);
        uint GetRVA(uint tk, out uint pulCodeRVA, out uint pdwImplFlags);
        uint GetPermissionSetProps(uint pm, out uint pdwAction, out IntPtr ppvPermission, out uint pcbPermission);
        uint GetSigFromToken(uint mdSig, out IntPtr ppvSig, out uint pcbSig);
        uint GetModuleRefProps(uint mur, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]char[] szName, uint cchName, out uint pchName);
        uint EnumModuleRefs(ref uint phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]uint[] rModuleRefs, uint cmax, out uint pcModuleRefs);
        uint GetTypeSpecFromToken(uint typespec, out IntPtr ppvSig, out uint pcbSig);
        uint GetNameFromToken(uint tk, out IntPtr pszUtf8NamePtr);
        uint EnumUnresolvedMethods(ref uint phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]uint[] rMethods, uint cMax, out uint pcTokens);
        uint GetUserString(uint stk, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] char[] szString, uint cchString, out uint pchString);
        uint GetPinvokeMap(uint tk, out uint pdwMappingFlags, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]char[] szImportName, uint cchImportName, out uint pchImportName, out uint pmrImportDLL);
        uint EnumSignatures(ref uint phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]uint[] rSignatures, uint cmax, out uint pcSignatures);
        uint EnumTypeSpecs(ref uint phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]uint[] rTypeSpecs, uint cmax, out uint pcTypeSpecs);
        uint EnumUserStrings(ref uint phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]uint[] rStrings, uint cmax, out uint pcStrings);
        uint GetParamForMethodIndex(uint md, uint ulParamSeq, out uint ppd);
        uint EnumCustomAttributes(ref uint phEnum, uint tk, uint tkType, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)]uint[] rCustomAttributes, uint cMax, out uint pcCustomAttributes);
        uint GetCustomAttributeProps(uint cv, out uint ptkObj, out uint ptkType, out IntPtr ppBlob, out uint pcbSize);
        uint FindTypeRef(uint tkResolutionScope, [MarshalAs(UnmanagedType.LPWStr)]string szName, out uint ptr);
        uint GetMemberProps(uint mb, out uint pClass, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]char[] szMember, uint cchMember, out uint pchMember, out uint pdwAttr, out IntPtr ppvSigBlob, out uint pcbSigBlob, out uint pulCodeRVA, out uint pdwImplFlags, out uint pdwCPlusTypeFlag, out IntPtr ppValue, out uint pcchValue);
        uint GetFieldProps(uint mb, out uint pClass, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]char[] szField, uint cchField, out uint pchField, out uint pdwAttr, out IntPtr ppvSigBlob, out uint pcbSigBlob, out uint pdwCPlusTypeFlag, out IntPtr ppValue, out uint pcchValue);
        uint GetPropertyProps(uint prop, out uint pClass, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]char[] szProperty, uint cchProperty, out uint pchProperty, out uint pdwPropFlags, out IntPtr ppvSig, out uint pbSig, out uint pdwCPlusTypeFlag, out IntPtr ppDefaultValue, out uint pcchDefaultValue, out uint pmdSetter, out uint pmdGetter, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 13)]uint[] rmdOtherMethod, uint cMax, out uint pcOtherMethod);
        uint GetParamProps(uint tk, out uint pmd, out uint pulSequence, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)]char[] szName, uint cchName, out uint pchName, out uint pdwAttr, out uint pdwCPlusTypeFlag, out IntPtr ppValue, out uint pcchValue);
        uint GetCustomAttributeByName(uint tkObj, [MarshalAs(UnmanagedType.LPWStr)]string szName, out IntPtr ppData, out uint pcbData);
        bool IsValidToken(uint tk);
        uint GetNestedClassProps(uint tdNestedClass, out uint ptdEnclosingClass);
        uint GetNativeCallConvFromSig(IntPtr pvSig, uint cbSig, out uint pCallConv);
        uint IsGlobal(uint pd, out uint pbGlobal);
    }

    [ComImport, GuidAttribute("EE62470B-E94B-424e-9B7C-2F00C9249F93"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMetaDataAssemblyImport {
    }

    public class MetadataImport {
        IMetaDataImport m_import;
        IMetaDataDispenserEx m_dispenser;

        public MetadataImport(string assemblyPath) {
            m_dispenser = new MetaDataDispenserEx();
            object rawScope = null;

            Guid metaDataImportGuid = new Guid("7DAC8207-D3AE-4c75-9B67-92801A497D44");
            m_dispenser.OpenScope(assemblyPath, 0, ref metaDataImportGuid, out rawScope);
            m_import = (IMetaDataImport)rawScope;
        }

        public List<uint> GetTypes() {
            List<uint> result = new List<uint>();

            uint typeEnum = 0;
            uint[] types = new uint[256];
            uint typeCount = 0;

            while ((m_import.EnumTypeDefs(ref typeEnum, types, 256, out typeCount) == 0) && (typeCount > 0)) {
                for (int i = 0; i < typeCount; i++) {
                    result.Add(types[i]);
                }
            }

            return result;
        }

        public Guid GetGuidCA(uint tkTypeDef) {
            IntPtr data;
            uint cbData = 0;
            try {
                if (m_import.GetCustomAttributeByName(tkTypeDef, "System.Runtime.InteropServices.GuidAttribute", out data, out cbData) == 0 && (cbData > 0)) {
                    data = (IntPtr)(data.ToInt64() + 3);
                    string guid = Marshal.PtrToStringAnsi(data);
                    return new Guid(guid);
                }
            } catch { }

            return new Guid();
        }

        public void GetMethodImpls(uint typeDefTok, out List<uint> bodyToks, out List<uint> declToks) {
            bodyToks = new List<uint>();
            declToks = new List<uint>();

            uint handle = 0;
            uint[] bodys = new uint[10];
            uint[] decls = new uint[10];
            uint count = 0;

            while ((m_import.EnumMethodImpls(ref handle, typeDefTok, bodys, decls, 10, out count) != 0) && (count > 0)) {
                for (int i = 0; i < count; i++) {
                    bodyToks.Add(bodys[i]);
                    declToks.Add(decls[i]);
                }
                m_import.EnumMethodImpls(ref handle, typeDefTok, bodys, decls, 10, out count);
            }
            m_import.CloseEnum(handle);
        }

        public void GetModuleRefs(out List<uint> moduleToks) {
            uint handle = 0;
            uint[] tokens = new uint[10];
            uint count = 0;

            moduleToks = new List<uint>();
            while ((m_import.EnumModuleRefs(ref handle, tokens, 10, out count) != 0) && (count > 0)) {
                for (int i = 0; i < count; i++) {
                    moduleToks.Add(tokens[i]);
                }
            }
            m_import.CloseEnum(handle);
        }

        public IList<string> GetModuleNames(bool includeModuleDef) {
            List<string> names = new List<string>();
            List<uint> moduleToks;
            GetModuleRefs(out moduleToks);

            char[] szName = new char[256];
            uint actual;
            foreach (uint token in moduleToks) {
                m_import.GetModuleRefProps(token, szName, 256, out actual);
                names.Add(new string(szName, 0, (int)actual));
            }

            if (includeModuleDef) {
                Guid mvid = new Guid();
                m_import.GetScopeProps(szName, 256, out actual, ref mvid);
                names.Add(new string(szName, 0, (int)actual));
            }
            return names;
        }
    }
}
