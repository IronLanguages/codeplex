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
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using ComTypes = System.Runtime.InteropServices.ComTypes;

using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Actions.ComDispatch {

    public static class ComRuntimeHelpers {

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "1#"), CLSCompliant(false)]
        public static void CheckThrowException(int hresult, ref ExcepInfo excepInfo, uint argErr, DispCallable dispCallable) {
            if (ComHresults.IsSuccess(hresult)) {
                return;
            }

            string genericMessage = "Error while invoking " + dispCallable.ComMethodDesc.Name;

            switch (hresult) {
                case ComHresults.DISP_E_BADPARAMCOUNT:
                    // The number of elements provided to DISPPARAMS is different from the number of arguments 
                    // accepted by the method or property.
                    throw new TargetParameterCountException(genericMessage);

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
                    throw new MissingMemberException(genericMessage);

                case ComHresults.DISP_E_NONAMEDARGS:
                    // This implementation of IDispatch does not support named arguments.
                    throw new ArgumentException(genericMessage + ". Named arguments are not supported");

                case ComHresults.DISP_E_OVERFLOW:
                    // One of the arguments in rgvarg could not be coerced to the specified type.
                    throw new OverflowException(genericMessage);

                case ComHresults.DISP_E_PARAMNOTFOUND:
                    // One of the parameter DISPIDs does not correspond to a parameter on the method. In this case, 
                    // puArgErr should be set to the first argument that contains the error. 
                    break;

                case ComHresults.DISP_E_TYPEMISMATCH:
                    // One or more of the arguments could not be coerced. The index within rgvarg of the first 
                    // parameter with the incorrect type is returned in the puArgErr parameter.
                    string message = String.Format("Could not convert argument {0} for call to {1}", argErr, dispCallable.ComMethodDesc.Name);
                    throw new ArgumentTypeException(message);

                case ComHresults.DISP_E_UNKNOWNINTERFACE:
                    // The interface identifier passed in riid is not IID_NULL.
                    break;

                case ComHresults.DISP_E_UNKNOWNLCID:
                    // The member being invoked interprets string arguments according to the LCID, and the 
                    // LCID is not recognized.
                    break;

                case ComHresults.DISP_E_PARAMNOTOPTIONAL:
                    // A required parameter was omitted.
                    throw new ArgumentException(genericMessage + ". A required parameter was omitted.");
            }

            Marshal.ThrowExceptionForHR(hresult);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")] // TODO: fix
        public static class UnsafeNativeMethods {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible")] // TODO: fix
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")] // TODO: fix
            [DllImport("oleaut32.dll", PreserveSig = false)]
            public static extern void VariantClear(IntPtr variant);

        }

        /// <summary>
        /// This class contains methods that either cannot be expressed in C#, or which require writing unsafe code.
        /// Callers of these methods need to use them extremely carefully as incorrect use could cause GC-holes
        /// and other problems.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")] // TODO: fix
        public static class UnsafeMethods {

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

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
            public static IntPtr ConvertVariantByrefToPtr(ref Variant value) { return _ConvertVariantByrefToPtr(ref value); }

            public static int IUnknownRelease(IntPtr interfacePointer) {
                return _IUnknownRelease(interfacePointer);
            }

            public static readonly IntPtr IID_NULL = GetIID_NULL();

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

                return _IDispatchInvoke(
                    dispatchPointer,
                    memberDispId,
                    flags,
                    ref dispParams,
                    out result,
                    out excepInfo,
                    out argErr);
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
                    throw new InvalidImplementationException(String.Format("IDispatch::GetIDsOfNames behaved unexpectedly for {0}", names[0]));
                }

                int[] keywordArgDispIds = ArrayUtils.RemoveFirst(dispIds); // Remove the dispId of the method name

                pinningHandle.Target = keywordArgDispIds;
                return Marshal.UnsafeAddrOfPinnedArrayElement(keywordArgDispIds, 0);
            }

            #endregion

            #region non-public members

#if DEBUG
            private const int _dummyMarker = 0x10101010;

            /// <summary>
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
#endif

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
                TypeGen type = Snippets.Shared.DefineUnsafeType("Type$ConvertByrefToPtr", typeof(object));

                Type[] paramTypes = new Type[] { typeof(Variant).MakeByRefType() };
                MethodBuilder mb = type.TypeBuilder.DefineMethod("ConvertByrefToPtr", MethodAttributes.Public | MethodAttributes.Static, typeof(IntPtr), paramTypes);
                GenericTypeParameterBuilder[] typeParams = mb.DefineGenericParameters("T");
                typeParams[0].SetGenericParameterAttributes(GenericParameterAttributes.NotNullableValueTypeConstraint);
                mb.SetSignature(typeof(IntPtr), null, null, new Type[] { typeParams[0].MakeByRefType() }, null, null);

                ILGenerator method = mb.GetILGenerator();

                method.Emit(OpCodes.Ldarg_0);
                method.Emit(OpCodes.Conv_I);
#if DEBUG
                method.Emit(OpCodes.Dup);
                method.Emit(OpCodes.Call, typeof(ComRuntimeHelpers.UnsafeMethods).GetMethod("AssertByrefPointsToStack"));
#endif
                method.Emit(OpCodes.Ret);

                return type.TypeBuilder.CreateType().GetMethod("ConvertByrefToPtr");
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
                TypeGen type = Snippets.Shared.DefineUnsafeType("Type$IUnknownRelease", typeof(object));
                MethodBuilder mb = type.TypeBuilder.DefineMethod("IUnknownRelease", MethodAttributes.Public | MethodAttributes.Static, typeof(int), new Type[] { typeof(IntPtr) });

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

                Type newType = type.FinishType();
                return (IUnknownReleaseDelegate)Delegate.CreateDelegate(typeof(IUnknownReleaseDelegate), newType.GetMethod("IUnknownRelease"));
            }

            private static IntPtr GetIID_NULL() {
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

            private static readonly IDispatchInvokeDelegate _IDispatchInvoke = Create_IDispatchInvoke();

            private static IDispatchInvokeDelegate Create_IDispatchInvoke() {
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
                TypeGen type = Snippets.Shared.DefineUnsafeType("Type$IDispatchInvoke", typeof(object));
                MethodBuilder mb = type.TypeBuilder.DefineMethod("IDispatchInvoke", MethodAttributes.Public | MethodAttributes.Static, typeof(int), paramTypes);

                ILGen method = new ILGen(mb.GetILGenerator());

                LocalBuilder functionPtr = method.DeclareLocal(typeof(IntPtr));

                // functionPtr = *(IntPtr*)(*(dispatchPointer) + VTABLE_OFFSET)

                int idispatchInvokeOffset = ((int)IDispatchMethodIndices.IDispatch_Invoke) * Marshal.SizeOf(typeof(IntPtr));
                method.EmitLoadArg(dispatchPointerIndex);
                method.Emit(OpCodes.Ldind_I);
                method.Emit(OpCodes.Ldc_I4, idispatchInvokeOffset);
                method.Emit(OpCodes.Add);
                method.Emit(OpCodes.Ldind_I);
                method.Emit(OpCodes.Stloc, functionPtr);

                // return functionPtr(...)

                method.EmitLoadArg(dispatchPointerIndex);
                method.EmitLoadArg(memberDispIdIndex);
                method.Emit(OpCodes.Ldsfld, typeof(ComRuntimeHelpers.UnsafeMethods).GetField("IID_NULL")); // riid
                method.Emit(OpCodes.Ldc_I4_0); // lcid
                method.EmitLoadArg(flagsIndex);
                method.EmitLoadArg(dispParamsIndex);
                method.EmitLoadArg(resultIndex);
                method.EmitLoadArg(exceptInfoIndex);
                method.EmitLoadArg(argErrIndex);
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

                Type newType = type.FinishType();
                return (IDispatchInvokeDelegate)Delegate.CreateDelegate(typeof(IDispatchInvokeDelegate), newType.GetMethod("IDispatchInvoke"));
            }

            #endregion
        }
    }
}

#endif
