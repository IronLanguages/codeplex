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
using System; using Microsoft;
#if !SILVERLIGHT // ComObject

using System.Diagnostics;
using Microsoft.Linq.Expressions;
using Microsoft.Linq.Expressions.Compiler;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using Microsoft.Scripting.Utils;
using ComTypes = System.Runtime.InteropServices.ComTypes;

namespace Microsoft.Scripting.Com {

    public static class ComRuntimeHelpers {

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "1#")]
        [CLSCompliant(false)]
        public static void CheckThrowException(int hresult, ref ExcepInfo excepInfo, uint argErr, string message) {
            if (ComHresults.IsSuccess(hresult)) {
                return;
            }

            switch (hresult) {
                case ComHresults.DISP_E_BADPARAMCOUNT:
                    // The number of elements provided to DISPPARAMS is different from the number of arguments 
                    // accepted by the method or property.
                    throw Error.DispBadParamCount(message);

                case ComHresults.DISP_E_BADVARTYPE:
                    //One of the arguments in rgvarg is not a valid variant type.
                    break;

                case ComHresults.DISP_E_EXCEPTION:
                    // The application needs to raise an exception. In this case, the structure passed in pExcepInfo 
                    // should be filled in.
                    throw excepInfo.GetException();

                case ComHresults.DISP_E_MEMBERNOTFOUND:
                    // The requested member does not exist, or the call to Invoke tried to set the value of a 
                    // read-only property.
                    throw Error.DispMemberNotFound(message);

                case ComHresults.DISP_E_NONAMEDARGS:
                    // This implementation of IDispatch does not support named arguments.
                    throw Error.DispNoNamedArgs(message);

                case ComHresults.DISP_E_OVERFLOW:
                    // One of the arguments in rgvarg could not be coerced to the specified type.
                    throw Error.DispOverflow(message);

                case ComHresults.DISP_E_PARAMNOTFOUND:
                    // One of the parameter DISPIDs does not correspond to a parameter on the method. In this case, 
                    // puArgErr should be set to the first argument that contains the error. 
                    break;

                case ComHresults.DISP_E_TYPEMISMATCH:
                    // One or more of the arguments could not be coerced. The index within rgvarg of the first 
                    // parameter with the incorrect type is returned in the puArgErr parameter.
                    throw Error.DispTypeMismatch(argErr, message);

                case ComHresults.DISP_E_UNKNOWNINTERFACE:
                    // The interface identifier passed in riid is not IID_NULL.
                    break;

                case ComHresults.DISP_E_UNKNOWNLCID:
                    // The member being invoked interprets string arguments according to the LCID, and the 
                    // LCID is not recognized.
                    break;

                case ComHresults.DISP_E_PARAMNOTOPTIONAL:
                    // A required parameter was omitted.
                    throw Error.DispParamNotOptional(message);
            }

            Marshal.ThrowExceptionForHR(hresult);
        }

        internal static void GetInfoFromType(ComTypes.ITypeInfo typeInfo, out string name, out string documentation) {
            int dwHelpContext;
            string strHelpFile;

            typeInfo.GetDocumentation(-1, out name, out documentation, out dwHelpContext, out strHelpFile);
        }

        internal static string GetNameOfMethod(ComTypes.ITypeInfo typeInfo, int memid) {
            int cNames;
            string[] rgNames = new string[1];
            typeInfo.GetNames(memid, rgNames, 1, out cNames);
            return rgNames[0];
        }

        internal static string GetNameOfLib(ComTypes.ITypeLib typeLib) {
            string name;
            string strDocString;
            int dwHelpContext;
            string strHelpFile;

            typeLib.GetDocumentation(-1, out name, out strDocString, out dwHelpContext, out strHelpFile);
            return name;
        }

        internal static string GetNameOfType(ComTypes.ITypeInfo typeInfo) {
            string name;
            string documentation;
            GetInfoFromType(typeInfo, out name, out documentation);

            return name;
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
        internal static ComTypes.ITypeInfo GetITypeInfoFromIDispatch(IDispatch dispatch, bool throwIfMissingExpectedTypeInfo) {
            uint typeCount;
            int hresult = dispatch.TryGetTypeInfoCount(out typeCount);
            Marshal.ThrowExceptionForHR(hresult);
            Debug.Assert(typeCount <= 1);
            if (typeCount == 0) {
                return null;
            }

            IntPtr typeInfoPtr = IntPtr.Zero;

            hresult = dispatch.TryGetTypeInfo(0, 0, out typeInfoPtr);
            if (!ComHresults.IsSuccess(hresult)) {
                CheckIfMissingTypeInfoIsExpected(hresult, throwIfMissingExpectedTypeInfo);
                return null;
            }
            if (typeInfoPtr == IntPtr.Zero) { // be defensive against components that return IntPtr.Zero
                if (throwIfMissingExpectedTypeInfo) {
                    Marshal.ThrowExceptionForHR(ComHresults.E_FAIL);
                }
                return null;
            }

            ComTypes.ITypeInfo typeInfo = null;
            try {
                typeInfo = Marshal.GetObjectForIUnknown(typeInfoPtr) as ComTypes.ITypeInfo;
            }
            finally {
                Marshal.Release(typeInfoPtr);
            }

            return typeInfo;
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
            Debug.Assert(!ComHresults.IsSuccess(hresult));

            // Word.Basic always returns this because of an incorrect implementation of IDispatch.GetTypeInfo
            // Any implementation that returns E_NOINTERFACE is likely to do so in all environments
            if (hresult == ComHresults.E_NOINTERFACE) {
                return;
            }

            // This assert is potentially over-restrictive since COM components can behave in quite unexpected ways.
            // However, asserting the common expected cases ensures that we find out about the unexpected scenarios, and
            // can investigate the scenarios to ensure that there is no bug in our own code.
            Debug.Assert(hresult == ComHresults.TYPE_E_LIBNOTREGISTERED);

            if (throwIfMissingExpectedTypeInfo) {
                Marshal.ThrowExceptionForHR(hresult);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        internal static ComTypes.TYPEATTR GetTypeAttrForTypeInfo(ComTypes.ITypeInfo typeInfo) {
            IntPtr pAttrs = IntPtr.Zero;
            typeInfo.GetTypeAttr(out pAttrs);

            // GetTypeAttr should never return null, this is just to be safe
            if (pAttrs == IntPtr.Zero) {
                throw Error.CannotRetrieveTypeInformation();
            }

            try {
                return (ComTypes.TYPEATTR)Marshal.PtrToStructure(pAttrs, typeof(ComTypes.TYPEATTR));
            }
            finally {
                typeInfo.ReleaseTypeAttr(pAttrs);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        internal static ComTypes.TYPELIBATTR GetTypeAttrForTypeLib(ComTypes.ITypeLib typeLib) {
            IntPtr pAttrs = IntPtr.Zero;
            typeLib.GetLibAttr(out pAttrs);

            // GetTypeAttr should never return null, this is just to be safe
            if (pAttrs == IntPtr.Zero) {
                throw Error.CannotRetrieveTypeInformation();
            }

            try {
                return (ComTypes.TYPELIBATTR)Marshal.PtrToStructure(pAttrs, typeof(ComTypes.TYPELIBATTR));
            }
            finally {
                typeLib.ReleaseTLibAttr(pAttrs);
            }
        }

        private static void UpdateByrefArguments(object[] explicitArgs, object[] argsForCall, VarEnumSelector varEnumSelector) {
            VariantBuilder[] variantBuilders = varEnumSelector.VariantBuilders;
            for (int i = 0; i < variantBuilders.Length; i++) {
                variantBuilders[i].ArgBuilder.UpdateFromReturn(explicitArgs[i], argsForCall[i]);
            }
        }

        public static object UnoptimizedInvoke(ComMethodDesc method, IDispatchObject dispatch, string[] keywordArgNames, object[] explicitArgs) {
            try {
                VarEnumSelector varEnumSelector = new VarEnumSelector(typeof(object), explicitArgs);
                ParameterModifier parameterModifiers;
                object[] argsForCall = varEnumSelector.BuildArguments(explicitArgs, out parameterModifiers);

                BindingFlags bindingFlags = BindingFlags.Instance;
                if (method.IsPropertyGet) {
                    bindingFlags |= BindingFlags.GetProperty;
                } else if (method.IsPropertyPut) {
                    bindingFlags |= BindingFlags.SetProperty;
                } else {
                    bindingFlags |= BindingFlags.InvokeMethod;
                }

                string memberName = method.DispIdString; // Use the "[DISPID=N]" format to avoid a call to GetIDsOfNames
                string[] namedParams = null;
                if (keywordArgNames.Length > 0) {
                    // InvokeMember does not allow the method name to be in "[DISPID=N]" format if there are namedParams
                    memberName = method.Name;

                    namedParams = keywordArgNames;
                    argsForCall = argsForCall.RotateRight(namedParams.Length);
                }

                // We use Type.InvokeMember instead of IDispatch.Invoke so that we do not
                // have to worry about marshalling the arguments. Type.InvokeMember will use
                // IDispatch.Invoke under the hood.
                object retVal = dispatch.DispatchObject.GetType().InvokeMember(
                    memberName,
                    bindingFlags,
                    Type.DefaultBinder,
                    dispatch.DispatchObject,
                    argsForCall,
                    new ParameterModifier[] { parameterModifiers },
                    null,
                    namedParams
                    );

                UpdateByrefArguments(explicitArgs, argsForCall, varEnumSelector);

                return retVal;
            } catch (TargetInvocationException e) {
                COMException comException = e.InnerException as COMException;
                if (comException != null) {
                    int errorCode = comException.ErrorCode;
                    if (errorCode > ComHresults.DISP_E_UNKNOWNINTERFACE && errorCode < ComHresults.DISP_E_PARAMNOTOPTIONAL) {
                        // If the current exception was caused because of a DISP_E_* errorcode, call
                        // ComRuntimeHelpers.CheckThrowException which handles these errorcodes specially. This ensures
                        // that we preserve identical behavior in both cases.
                        ExcepInfo excepInfo = new ExcepInfo();
                        ComRuntimeHelpers.CheckThrowException(comException.ErrorCode, ref excepInfo, UInt32.MaxValue, method.Name);
                    }
                }

                // Unwrap the real (inner) exception and raise it
                throw e.InnerException;
            }
        }

        // TODO: Can be internal?
        [Obsolete("Called only from generated code", true)]
        public static BoundDispEvent CreateComEvent(object rcw, Guid sourceIid, int dispid) {
            return new BoundDispEvent(rcw, sourceIid, dispid);
        }

        // TODO: Can be internal?
        [Obsolete("Called only from generated code", true)]
        public static DispPropertyGet CreatePropertyGet(IDispatchObject dispatch, ComMethodDesc method) {
            return new DispPropertyGet(dispatch, method);
        }

        // TODO: Can be internal?
        [Obsolete("Called only from generated code", true)]
        public static DispPropertyPut CreatePropertyPut(IDispatchObject dispatch, ComMethodDesc method) {
            return new DispPropertyPut(dispatch, method);
        }

        // TODO: Can be internal?
        [Obsolete("Called only from generated code", true)]
        public static DispMethod CreateMethod(IDispatchObject dispatch, ComMethodDesc method) {
            return new DispMethod(dispatch, method);
        }
    }

    /// <summary>
    /// This class contains methods that either cannot be expressed in C#, or which require writing unsafe code.
    /// Callers of these methods need to use them extremely carefully as incorrect use could cause GC-holes
    /// and other problems.
    /// </summary>
    public static class UnsafeMethods {

        private static void EmitLoadArg(ILGenerator il, int index) {
            ContractUtils.Requires(index >= 0, "index");

            switch (index) {
                case 0:
                    il.Emit(OpCodes.Ldarg_0);
                    break;
                case 1:
                    il.Emit(OpCodes.Ldarg_1);
                    break;
                case 2:
                    il.Emit(OpCodes.Ldarg_2);
                    break;
                case 3:
                    il.Emit(OpCodes.Ldarg_3);
                    break;
                default:
                    if (index <= Byte.MaxValue) {
                        il.Emit(OpCodes.Ldarg_S, (byte)index);
                    } else {
                        il.Emit(OpCodes.Ldarg, index);
                    }
                    break;
            }
        }

        #region public members

        #region Generated ConvertByrefToPtr

        // *** BEGIN GENERATED CODE ***
        // generated by function: gen_ConvertByrefToPtr from: generate_comdispatch.py

        [CLSCompliant(false)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
        public static IntPtr ConvertSByteByrefToPtr(ref SByte value) { return _ConvertSByteByrefToPtr(ref value); }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
        public static IntPtr ConvertInt16ByrefToPtr(ref Int16 value) { return _ConvertInt16ByrefToPtr(ref value); }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
        public static IntPtr ConvertInt32ByrefToPtr(ref Int32 value) { return _ConvertInt32ByrefToPtr(ref value); }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
        public static IntPtr ConvertInt64ByrefToPtr(ref Int64 value) { return _ConvertInt64ByrefToPtr(ref value); }
        [CLSCompliant(false)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
        public static IntPtr ConvertByteByrefToPtr(ref Byte value) { return _ConvertByteByrefToPtr(ref value); }
        [CLSCompliant(false)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
        public static IntPtr ConvertUInt16ByrefToPtr(ref UInt16 value) { return _ConvertUInt16ByrefToPtr(ref value); }
        [CLSCompliant(false)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
        public static IntPtr ConvertUInt32ByrefToPtr(ref UInt32 value) { return _ConvertUInt32ByrefToPtr(ref value); }
        [CLSCompliant(false)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
        public static IntPtr ConvertUInt64ByrefToPtr(ref UInt64 value) { return _ConvertUInt64ByrefToPtr(ref value); }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
        public static IntPtr ConvertIntPtrByrefToPtr(ref IntPtr value) { return _ConvertIntPtrByrefToPtr(ref value); }
        [CLSCompliant(false)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
        public static IntPtr ConvertUIntPtrByrefToPtr(ref UIntPtr value) { return _ConvertUIntPtrByrefToPtr(ref value); }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
        public static IntPtr ConvertSingleByrefToPtr(ref Single value) { return _ConvertSingleByrefToPtr(ref value); }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
        public static IntPtr ConvertDoubleByrefToPtr(ref Double value) { return _ConvertDoubleByrefToPtr(ref value); }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
        public static IntPtr ConvertDecimalByrefToPtr(ref Decimal value) { return _ConvertDecimalByrefToPtr(ref value); }

        // *** END GENERATED CODE ***

        #endregion

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")] // TODO: fix
        public static IntPtr ConvertVariantByrefToPtr(ref Variant value) { return _ConvertVariantByrefToPtr(ref value); }

        public static int IUnknownRelease(IntPtr interfacePointer) {
            return _IUnknownRelease(interfacePointer);
        }

        public static readonly IntPtr NullInterfaceId = GetNullInterfaceId();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")] // TODO: fix
        [CLSCompliant(false)]
        public static int IDispatchInvoke(
            IntPtr dispatchPointer,
            int memberDispId,
            ComTypes.INVOKEKIND flags,
            ref ComTypes.DISPPARAMS dispParams,
            out Variant result,
            out ExcepInfo excepInfo,
            out uint argErr) {

            int hresult = _IDispatchInvoke(
                dispatchPointer,
                memberDispId,
                flags,
                ref dispParams,
                out result,
                out excepInfo,
                out argErr);

            if (hresult == ComHresults.DISP_E_MEMBERNOTFOUND
                && (flags & ComTypes.INVOKEKIND.INVOKE_FUNC) != 0
                && (flags & (ComTypes.INVOKEKIND.INVOKE_PROPERTYPUT | ComTypes.INVOKEKIND.INVOKE_PROPERTYPUTREF)) == 0) {

                // Re-invoke with no result argument to accomodate Word
                hresult = _IDispatchInvokeNoResult(
                    dispatchPointer,
                    memberDispId,
                    ComTypes.INVOKEKIND.INVOKE_FUNC,
                    ref dispParams,
                    out result,
                    out excepInfo,
                    out argErr);
            }
            return hresult;
        }

        [CLSCompliant(false)]
        public static IntPtr GetIdsOfNamedParameters(IDispatchObject dispatch, string[] names, int methodDispId, out GCHandle pinningHandle) {
            pinningHandle = GCHandle.Alloc(null, GCHandleType.Pinned);
            int[] dispIds = new int[names.Length];
            Guid empty = Guid.Empty;
            int hresult = dispatch.DispatchObject.TryGetIDsOfNames(ref empty, names, (uint)names.Length, 0, dispIds);
            if (hresult < 0) {
                Marshal.ThrowExceptionForHR(hresult);
            }

            if (methodDispId != dispIds[0]) {
                throw Error.GetIDsOfNamesInvalid(names[0]);
            }

            int[] keywordArgDispIds = dispIds.RemoveFirst(); // Remove the dispId of the method name

            pinningHandle.Target = keywordArgDispIds;
            return Marshal.UnsafeAddrOfPinnedArrayElement(keywordArgDispIds, 0);
        }

        #endregion

        #region non-public members

        private const int _dummyMarker = 0x10101010;

        /// <summary>
        /// TODO: Used only in DEBUG build. Remove?
        /// 
        /// Ensure that "value" is a local variable in some caller's frame. So converting
        /// the byref to an IntPtr is a safe operation. Alternatively, we could also allow 
        /// allowed "value"  to be a pinned object.
        /// </summary>
        public static void AssertByrefPointsToStack(IntPtr ptr) {
            if (Marshal.ReadInt32(ptr) == _dummyMarker) {
                // Prevent recursion
                return;
            }
            int dummy = _dummyMarker;
            IntPtr ptrToLocal = ConvertInt32ByrefToPtr(ref dummy);
            Debug.Assert(ptrToLocal.ToInt64() < ptr.ToInt64());
            Debug.Assert((ptr.ToInt64() - ptrToLocal.ToInt64()) < (16 * 1024));
        }

        private static readonly MethodInfo _ConvertByrefToPtr = Create_ConvertByrefToPtr();
        private delegate IntPtr ConvertByrefToPtrDelegate<T>(ref T value);

        #region Generated ConvertByrefToPtrDelegates

        // *** BEGIN GENERATED CODE ***
        // generated by function: gen_ConvertByrefToPtrDelegates from: generate_comdispatch.py

        private static readonly ConvertByrefToPtrDelegate<SByte> _ConvertSByteByrefToPtr = (ConvertByrefToPtrDelegate<SByte>)Delegate.CreateDelegate(typeof(ConvertByrefToPtrDelegate<SByte>), _ConvertByrefToPtr.MakeGenericMethod(typeof(SByte)));
        private static readonly ConvertByrefToPtrDelegate<Int16> _ConvertInt16ByrefToPtr = (ConvertByrefToPtrDelegate<Int16>)Delegate.CreateDelegate(typeof(ConvertByrefToPtrDelegate<Int16>), _ConvertByrefToPtr.MakeGenericMethod(typeof(Int16)));
        private static readonly ConvertByrefToPtrDelegate<Int32> _ConvertInt32ByrefToPtr = (ConvertByrefToPtrDelegate<Int32>)Delegate.CreateDelegate(typeof(ConvertByrefToPtrDelegate<Int32>), _ConvertByrefToPtr.MakeGenericMethod(typeof(Int32)));
        private static readonly ConvertByrefToPtrDelegate<Int64> _ConvertInt64ByrefToPtr = (ConvertByrefToPtrDelegate<Int64>)Delegate.CreateDelegate(typeof(ConvertByrefToPtrDelegate<Int64>), _ConvertByrefToPtr.MakeGenericMethod(typeof(Int64)));
        private static readonly ConvertByrefToPtrDelegate<Byte> _ConvertByteByrefToPtr = (ConvertByrefToPtrDelegate<Byte>)Delegate.CreateDelegate(typeof(ConvertByrefToPtrDelegate<Byte>), _ConvertByrefToPtr.MakeGenericMethod(typeof(Byte)));
        private static readonly ConvertByrefToPtrDelegate<UInt16> _ConvertUInt16ByrefToPtr = (ConvertByrefToPtrDelegate<UInt16>)Delegate.CreateDelegate(typeof(ConvertByrefToPtrDelegate<UInt16>), _ConvertByrefToPtr.MakeGenericMethod(typeof(UInt16)));
        private static readonly ConvertByrefToPtrDelegate<UInt32> _ConvertUInt32ByrefToPtr = (ConvertByrefToPtrDelegate<UInt32>)Delegate.CreateDelegate(typeof(ConvertByrefToPtrDelegate<UInt32>), _ConvertByrefToPtr.MakeGenericMethod(typeof(UInt32)));
        private static readonly ConvertByrefToPtrDelegate<UInt64> _ConvertUInt64ByrefToPtr = (ConvertByrefToPtrDelegate<UInt64>)Delegate.CreateDelegate(typeof(ConvertByrefToPtrDelegate<UInt64>), _ConvertByrefToPtr.MakeGenericMethod(typeof(UInt64)));
        private static readonly ConvertByrefToPtrDelegate<IntPtr> _ConvertIntPtrByrefToPtr = (ConvertByrefToPtrDelegate<IntPtr>)Delegate.CreateDelegate(typeof(ConvertByrefToPtrDelegate<IntPtr>), _ConvertByrefToPtr.MakeGenericMethod(typeof(IntPtr)));
        private static readonly ConvertByrefToPtrDelegate<UIntPtr> _ConvertUIntPtrByrefToPtr = (ConvertByrefToPtrDelegate<UIntPtr>)Delegate.CreateDelegate(typeof(ConvertByrefToPtrDelegate<UIntPtr>), _ConvertByrefToPtr.MakeGenericMethod(typeof(UIntPtr)));
        private static readonly ConvertByrefToPtrDelegate<Single> _ConvertSingleByrefToPtr = (ConvertByrefToPtrDelegate<Single>)Delegate.CreateDelegate(typeof(ConvertByrefToPtrDelegate<Single>), _ConvertByrefToPtr.MakeGenericMethod(typeof(Single)));
        private static readonly ConvertByrefToPtrDelegate<Double> _ConvertDoubleByrefToPtr = (ConvertByrefToPtrDelegate<Double>)Delegate.CreateDelegate(typeof(ConvertByrefToPtrDelegate<Double>), _ConvertByrefToPtr.MakeGenericMethod(typeof(Double)));
        private static readonly ConvertByrefToPtrDelegate<Decimal> _ConvertDecimalByrefToPtr = (ConvertByrefToPtrDelegate<Decimal>)Delegate.CreateDelegate(typeof(ConvertByrefToPtrDelegate<Decimal>), _ConvertByrefToPtr.MakeGenericMethod(typeof(Decimal)));

        // *** END GENERATED CODE ***

        #endregion

        private static readonly ConvertByrefToPtrDelegate<Variant> _ConvertVariantByrefToPtr = (ConvertByrefToPtrDelegate<Variant>)Delegate.CreateDelegate(typeof(ConvertByrefToPtrDelegate<Variant>), _ConvertByrefToPtr.MakeGenericMethod(typeof(Variant)));

        private static MethodInfo Create_ConvertByrefToPtr() {
            // We dont use AssemblyGen.DefineMethod since that can create a anonymously-hosted DynamicMethod which cannot contain unverifiable code.
            TypeBuilder type = Snippets.Shared.DefineUnsafeType("Type$ConvertByrefToPtr", typeof(object));

            Type[] paramTypes = new Type[] { typeof(Variant).MakeByRefType() };
            MethodBuilder mb = type.DefineMethod("ConvertByrefToPtr", MethodAttributes.Public | MethodAttributes.Static, typeof(IntPtr), paramTypes);
            GenericTypeParameterBuilder[] typeParams = mb.DefineGenericParameters("T");
            typeParams[0].SetGenericParameterAttributes(GenericParameterAttributes.NotNullableValueTypeConstraint);
            mb.SetSignature(typeof(IntPtr), null, null, new Type[] { typeParams[0].MakeByRefType() }, null, null);

            ILGenerator method = mb.GetILGenerator();

            method.Emit(OpCodes.Ldarg_0);
            method.Emit(OpCodes.Conv_I);
#if DEBUG
                method.Emit(OpCodes.Dup);
                method.Emit(OpCodes.Call, typeof(UnsafeMethods).GetMethod("AssertByrefPointsToStack"));
#endif
            method.Emit(OpCodes.Ret);

            return type.CreateType().GetMethod("ConvertByrefToPtr");
        }

        /// <summary>
        /// We will emit an indirect call to an unmanaged function pointer from the vtable of the given interface pointer. 
        /// This approach can take only ~300 instructions on x86 compared with ~900 for Marshal.Release. We are relying on 
        /// the JIT-compiler to do pinvoke-stub-inlining and calling the pinvoke target directly.
        /// </summary>
        private delegate int IUnknownReleaseDelegate(IntPtr interfacePointer);
        private static readonly IUnknownReleaseDelegate _IUnknownRelease = Create_IUnknownRelease();

        private static IUnknownReleaseDelegate Create_IUnknownRelease() {
            // We dont use AssemblyGen.DefineMethod since that can create a anonymously-hosted DynamicMethod which cannot contain unverifiable code.
            TypeBuilder type = Snippets.Shared.DefineUnsafeType("Type$IUnknownRelease", typeof(object));
            MethodBuilder mb = type.DefineMethod("IUnknownRelease", MethodAttributes.Public | MethodAttributes.Static, typeof(int), new Type[] { typeof(IntPtr) });

            ILGenerator method = mb.GetILGenerator();

            LocalBuilder functionPtr = method.DeclareLocal(typeof(IntPtr));

            // functionPtr = *(IntPtr*)(*(interfacePointer) + VTABLE_OFFSET)

            int iunknownReleaseOffset = ((int)IDispatchMethodIndices.IUnknown_Release) * Marshal.SizeOf(typeof(IntPtr));
            method.Emit(OpCodes.Ldarg_0);
            method.Emit(OpCodes.Ldind_I);
            method.Emit(OpCodes.Ldc_I4, iunknownReleaseOffset);
            method.Emit(OpCodes.Add);
            method.Emit(OpCodes.Ldind_I);
            method.Emit(OpCodes.Stloc, functionPtr);

            // return functionPtr(...)

            method.Emit(OpCodes.Ldarg_0);
            method.Emit(OpCodes.Ldloc, functionPtr);
            SignatureHelper signature = SignatureHelper.GetMethodSigHelper(CallingConvention.Winapi, typeof(int));
            signature.AddArgument(typeof(IntPtr));
            method.Emit(OpCodes.Calli, signature);

            method.Emit(OpCodes.Ret);

            Type newType = type.CreateType();
            return (IUnknownReleaseDelegate)Delegate.CreateDelegate(typeof(IUnknownReleaseDelegate), newType.GetMethod("IUnknownRelease"));
        }

        private static IntPtr GetNullInterfaceId() {
            int size = Marshal.SizeOf(Guid.Empty);
            IntPtr ptr = Marshal.AllocHGlobal(size);
            for (int i = 0; i < size; i++) {
                Marshal.WriteByte(ptr, i, 0);
            }
            return ptr;
        }

        /// <summary>
        /// We will emit an indirect call to an unmanaged function pointer from the vtable of the given IDispatch interface pointer. 
        /// It is not possible to express this in C#. Using an indirect pinvoke call allows us to do our own marshalling. 
        /// We can allocate the Variant arguments cheaply on the stack. We are relying on the JIT-compiler to do 
        /// pinvoke-stub-inlining and calling the pinvoke target directly.
        /// The alternative of calling via a managed interface declaration of IDispatch would have a performance
        /// penalty of going through a CLR stub that would have to re-push the arguments on the stack, etc.
        /// Marshal.GetDelegateForFunctionPointer could be used here, but its too expensive (~2000 instructions on x86).
        /// </summary>
        private delegate int IDispatchInvokeDelegate(
            IntPtr dispatchPointer,
            int memberDispId,
            ComTypes.INVOKEKIND flags,
            ref ComTypes.DISPPARAMS dispParams,
            out Variant result,
            out ExcepInfo excepInfo,
            out uint argErr);

        private static readonly IDispatchInvokeDelegate _IDispatchInvoke = Create_IDispatchInvoke(true);
        private static IDispatchInvokeDelegate _IDispatchInvokeNoResultImpl;

        private static IDispatchInvokeDelegate _IDispatchInvokeNoResult {
            get {
                if (_IDispatchInvokeNoResultImpl == null) {
                    lock (_IDispatchInvoke) {
                        if (_IDispatchInvokeNoResultImpl == null) {
                            _IDispatchInvokeNoResultImpl = Create_IDispatchInvoke(false);
                        }
                    }
                }
                return _IDispatchInvokeNoResultImpl;
            }
        }

        private static IDispatchInvokeDelegate Create_IDispatchInvoke(bool returnResult) {
            const int dispatchPointerIndex = 0;
            const int memberDispIdIndex = 1;
            const int flagsIndex = 2;
            const int dispParamsIndex = 3;
            const int resultIndex = 4;
            const int exceptInfoIndex = 5;
            const int argErrIndex = 6;
            Debug.Assert(argErrIndex + 1 == typeof(IDispatchInvokeDelegate).GetMethod("Invoke").GetParameters().Length);

            Type[] paramTypes = new Type[argErrIndex + 1];
            paramTypes[dispatchPointerIndex] = typeof(IntPtr);
            paramTypes[memberDispIdIndex] = typeof(int);
            paramTypes[flagsIndex] = typeof(ComTypes.INVOKEKIND);
            paramTypes[dispParamsIndex] = typeof(ComTypes.DISPPARAMS).MakeByRefType();
            paramTypes[resultIndex] = typeof(Variant).MakeByRefType();
            paramTypes[exceptInfoIndex] = typeof(ExcepInfo).MakeByRefType();
            paramTypes[argErrIndex] = typeof(uint).MakeByRefType();

            // We dont use AssemblyGen.DefineMethod since that can create a anonymously-hosted DynamicMethod which cannot contain unverifiable code.
            TypeBuilder type = Snippets.Shared.DefineUnsafeType("Type$IDispatchInvoke", typeof(object));
            MethodBuilder mb = type.DefineMethod("IDispatchInvoke", MethodAttributes.Public | MethodAttributes.Static, typeof(int), paramTypes);

            ILGenerator method = mb.GetILGenerator();

            LocalBuilder functionPtr = method.DeclareLocal(typeof(IntPtr));

            // functionPtr = *(IntPtr*)(*(dispatchPointer) + VTABLE_OFFSET)

            int idispatchInvokeOffset = ((int)IDispatchMethodIndices.IDispatch_Invoke) * Marshal.SizeOf(typeof(IntPtr));
            EmitLoadArg(method, dispatchPointerIndex);
            method.Emit(OpCodes.Ldind_I);
            method.Emit(OpCodes.Ldc_I4, idispatchInvokeOffset);
            method.Emit(OpCodes.Add);
            method.Emit(OpCodes.Ldind_I);
            method.Emit(OpCodes.Stloc, functionPtr);

            // return functionPtr(...)

            EmitLoadArg(method, dispatchPointerIndex);
            EmitLoadArg(method, memberDispIdIndex);
            method.Emit(OpCodes.Ldsfld, typeof(UnsafeMethods).GetField("NullInterfaceId")); // riid
            method.Emit(OpCodes.Ldc_I4_0); // lcid
            EmitLoadArg(method, flagsIndex);
            EmitLoadArg(method, dispParamsIndex);
            if (returnResult) {
                EmitLoadArg(method, resultIndex);
            } else {
                method.Emit(OpCodes.Ldsfld, typeof(IntPtr).GetField("Zero"));
            }
            EmitLoadArg(method, exceptInfoIndex);
            EmitLoadArg(method, argErrIndex);
            method.Emit(OpCodes.Ldloc, functionPtr);
            SignatureHelper signature = SignatureHelper.GetMethodSigHelper(CallingConvention.Winapi, typeof(int));
            Type[] invokeParamTypes = new Type[] { 
                    typeof(IntPtr), // dispatchPointer
                    typeof(int), // memberDispId
                    typeof(IntPtr), // riid
                    typeof(int), // lcid
                    typeof(ushort), // flags
                    typeof(IntPtr), // dispParams
                    typeof(IntPtr), // result
                    typeof(IntPtr), // excepInfo
                    typeof(IntPtr), // argErr
                };
            signature.AddArguments(invokeParamTypes, null, null);
            method.Emit(OpCodes.Calli, signature);

            method.Emit(OpCodes.Ret);

            Type newType = type.CreateType();
            return (IDispatchInvokeDelegate)Delegate.CreateDelegate(typeof(IDispatchInvokeDelegate), newType.GetMethod("IDispatchInvoke"));
        }

        #endregion
    }

    public static class UnsafeNativeMethods {
        [System.Runtime.Versioning.ResourceExposure(System.Runtime.Versioning.ResourceScope.None)]
        [System.Runtime.Versioning.ResourceConsumption(System.Runtime.Versioning.ResourceScope.Process, System.Runtime.Versioning.ResourceScope.Process)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")] // TODO: fix
        [DllImport("oleaut32.dll", PreserveSig = false)]
        internal static extern void VariantClear(IntPtr variant);

        [System.Runtime.Versioning.ResourceExposure(System.Runtime.Versioning.ResourceScope.Machine)]
        [System.Runtime.Versioning.ResourceConsumption(System.Runtime.Versioning.ResourceScope.Machine, System.Runtime.Versioning.ResourceScope.Machine)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")] // TODO: fix
        [DllImport("oleaut32.dll", PreserveSig = false)]
        internal static extern ComTypes.ITypeLib LoadRegTypeLib(ref Guid clsid, short majorVersion, short minorVersion, int lcid);
    }
}

#endif
