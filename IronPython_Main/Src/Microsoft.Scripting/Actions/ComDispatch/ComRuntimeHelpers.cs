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
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Actions.ComDispatch {

    public static class ComRuntimeHelpers {

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "1#"), CLSCompliant(false)]
        public static void CheckThrowException(int hresult, ref ExcepInfo excepInfo, uint argErr, DispCallable dispCallable) {
            if (ComHresults.IsSuccess(hresult)) {
                return;
            }

            if (hresult == ComHresults.DISP_E_EXCEPTION) {
                throw excepInfo.GetException();
            } else if (hresult == ComHresults.DISP_E_TYPEMISMATCH) {
                string message = String.Format("Could not convert argument {0} for call to function {1}", argErr, dispCallable.ComMethodDesc.Name);
                throw new COMException("DISP_E_TYPEMISMATCH", new InvalidCastException(message));
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

            private static AssemblyGen _UnverifiableAssembly = GetUnverifiableAssembly();

            private static AssemblyGen GetUnverifiableAssembly() {
                AssemblyGen snippetsAssembly = ScriptDomainManager.CurrentManager.Snippets.Assembly;
                if (!snippetsAssembly.VerifyAssemblies) {
                    return snippetsAssembly;
                }

                AssemblyGen asm = new AssemblyGen("UnverifiableAssembly", null, null, AssemblyGenAttributes.None);
                return asm;
            }

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

            private static MethodInfo _ConvertByrefToPtr = Create_ConvertByrefToPtr();
            private delegate IntPtr ConvertByrefToPtrDelegate<T>(ref T value);

            #region Generated ConvertByrefToPtrDelegates

            // *** BEGIN GENERATED CODE ***

            private static ConvertByrefToPtrDelegate<SByte> _ConvertSByteByrefToPtr = (ConvertByrefToPtrDelegate<SByte>)Delegate.CreateDelegate(typeof(ConvertByrefToPtrDelegate<SByte>), _ConvertByrefToPtr.MakeGenericMethod(typeof(SByte)));
            private static ConvertByrefToPtrDelegate<Int16> _ConvertInt16ByrefToPtr = (ConvertByrefToPtrDelegate<Int16>)Delegate.CreateDelegate(typeof(ConvertByrefToPtrDelegate<Int16>), _ConvertByrefToPtr.MakeGenericMethod(typeof(Int16)));
            private static ConvertByrefToPtrDelegate<Int32> _ConvertInt32ByrefToPtr = (ConvertByrefToPtrDelegate<Int32>)Delegate.CreateDelegate(typeof(ConvertByrefToPtrDelegate<Int32>), _ConvertByrefToPtr.MakeGenericMethod(typeof(Int32)));
            private static ConvertByrefToPtrDelegate<Int64> _ConvertInt64ByrefToPtr = (ConvertByrefToPtrDelegate<Int64>)Delegate.CreateDelegate(typeof(ConvertByrefToPtrDelegate<Int64>), _ConvertByrefToPtr.MakeGenericMethod(typeof(Int64)));
            private static ConvertByrefToPtrDelegate<Byte> _ConvertByteByrefToPtr = (ConvertByrefToPtrDelegate<Byte>)Delegate.CreateDelegate(typeof(ConvertByrefToPtrDelegate<Byte>), _ConvertByrefToPtr.MakeGenericMethod(typeof(Byte)));
            private static ConvertByrefToPtrDelegate<UInt16> _ConvertUInt16ByrefToPtr = (ConvertByrefToPtrDelegate<UInt16>)Delegate.CreateDelegate(typeof(ConvertByrefToPtrDelegate<UInt16>), _ConvertByrefToPtr.MakeGenericMethod(typeof(UInt16)));
            private static ConvertByrefToPtrDelegate<UInt32> _ConvertUInt32ByrefToPtr = (ConvertByrefToPtrDelegate<UInt32>)Delegate.CreateDelegate(typeof(ConvertByrefToPtrDelegate<UInt32>), _ConvertByrefToPtr.MakeGenericMethod(typeof(UInt32)));
            private static ConvertByrefToPtrDelegate<UInt64> _ConvertUInt64ByrefToPtr = (ConvertByrefToPtrDelegate<UInt64>)Delegate.CreateDelegate(typeof(ConvertByrefToPtrDelegate<UInt64>), _ConvertByrefToPtr.MakeGenericMethod(typeof(UInt64)));
            private static ConvertByrefToPtrDelegate<IntPtr> _ConvertIntPtrByrefToPtr = (ConvertByrefToPtrDelegate<IntPtr>)Delegate.CreateDelegate(typeof(ConvertByrefToPtrDelegate<IntPtr>), _ConvertByrefToPtr.MakeGenericMethod(typeof(IntPtr)));
            private static ConvertByrefToPtrDelegate<UIntPtr> _ConvertUIntPtrByrefToPtr = (ConvertByrefToPtrDelegate<UIntPtr>)Delegate.CreateDelegate(typeof(ConvertByrefToPtrDelegate<UIntPtr>), _ConvertByrefToPtr.MakeGenericMethod(typeof(UIntPtr)));
            private static ConvertByrefToPtrDelegate<Single> _ConvertSingleByrefToPtr = (ConvertByrefToPtrDelegate<Single>)Delegate.CreateDelegate(typeof(ConvertByrefToPtrDelegate<Single>), _ConvertByrefToPtr.MakeGenericMethod(typeof(Single)));
            private static ConvertByrefToPtrDelegate<Double> _ConvertDoubleByrefToPtr = (ConvertByrefToPtrDelegate<Double>)Delegate.CreateDelegate(typeof(ConvertByrefToPtrDelegate<Double>), _ConvertByrefToPtr.MakeGenericMethod(typeof(Double)));
            private static ConvertByrefToPtrDelegate<Decimal> _ConvertDecimalByrefToPtr = (ConvertByrefToPtrDelegate<Decimal>)Delegate.CreateDelegate(typeof(ConvertByrefToPtrDelegate<Decimal>), _ConvertByrefToPtr.MakeGenericMethod(typeof(Decimal)));

            // *** END GENERATED CODE ***

            #endregion
            
            private static ConvertByrefToPtrDelegate<Variant> _ConvertVariantByrefToPtr = (ConvertByrefToPtrDelegate<Variant>)Delegate.CreateDelegate(typeof(ConvertByrefToPtrDelegate<Variant>), _ConvertByrefToPtr.MakeGenericMethod(typeof(Variant)));

            private static MethodInfo Create_ConvertByrefToPtr() {
                // We dont use AssemblyGen.DefineMethod since that can create a anonymously-hosted DynamicMethod which cannot contain unverifiable code.
                TypeGen type = _UnverifiableAssembly.DefinePublicType("Type$ConvertByrefToPtr", typeof(object));
                Type[] paramTypes = new Type[] { typeof(Variant).MakeByRefType() };
                Compiler method = type.DefineMethod("ConvertByrefToPtr", typeof(IntPtr), paramTypes, null, null);
                GenericTypeParameterBuilder[] typeParams = ((MethodBuilder)method.Method).DefineGenericParameters("T");
                typeParams[0].SetGenericParameterAttributes(GenericParameterAttributes.NotNullableValueTypeConstraint);
                ((MethodBuilder)method.Method).SetSignature(typeof(IntPtr), null, null, new Type[] { typeParams[0].MakeByRefType() }, null, null);

                method.Emit(OpCodes.Ldarg_0);
                method.Emit(OpCodes.Conv_I);
#if DEBUG
                method.Emit(OpCodes.Dup);
                method.Emit(OpCodes.Call, typeof(ComRuntimeHelpers.UnsafeMethods).GetMethod("AssertByrefPointsToStack"));
#endif
                method.EmitReturn();

                return type.TypeBuilder.CreateType().GetMethod("ConvertByrefToPtr");
            }

            /// <summary>
            /// We will emit an indirect call to an unmanaged function pointer from the vtable of the given interface pointer. 
            /// This approach can take only ~300 instructions on x86 compared with ~900 for Marshal.Release. We are relying on 
            /// the JIT-compiler to do pinvoke-stub-inlining and calling the pinvoke target directly.
            /// </summary>
            private delegate int IUnknownReleaseDelegate(IntPtr interfacePointer);
            private static IUnknownReleaseDelegate _IUnknownRelease = Create_IUnknownRelease();

            private static IUnknownReleaseDelegate Create_IUnknownRelease() {
                // We dont use AssemblyGen.DefineMethod since that can create a anonymously-hosted DynamicMethod which cannot contain unverifiable code.
                TypeGen type = _UnverifiableAssembly.DefinePublicType("Type$IUnknownRelease", typeof(object));
                Compiler method = type.DefineMethod("IUnknownRelease", typeof(int), new Type[] { typeof(IntPtr) }, null, null);

                LocalBuilder functionPtr = method.DeclareLocal(typeof(IntPtr));

                // functionPtr = *(IntPtr*)(*(interfacePointer) + VTABLE_OFFSET)

                int iunknownReleaseOffset = ((int)IDispatchMethodIndices.IUnknown_Release) * Marshal.SizeOf(typeof(IntPtr));
                method.EmitArgGet(0);
                method.Emit(OpCodes.Ldind_I);
                method.Emit(OpCodes.Ldc_I4, iunknownReleaseOffset);
                method.Emit(OpCodes.Add);
                method.Emit(OpCodes.Ldind_I);
                method.Emit(OpCodes.Stloc, functionPtr);

                // return functionPtr(...)

                method.EmitArgGet(0);
                method.Emit(OpCodes.Ldloc, functionPtr);
                SignatureHelper signature = SignatureHelper.GetMethodSigHelper(CallingConvention.Winapi, typeof(int));
                signature.AddArgument(typeof(IntPtr));
                method.Emit(OpCodes.Calli, signature);

                method.EmitReturn();

                type.FinishType();
                return (IUnknownReleaseDelegate)method.CreateDelegate(typeof(IUnknownReleaseDelegate));
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

            private static IDispatchInvokeDelegate _IDispatchInvoke = Create_IDispatchInvoke();

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
                TypeGen type = _UnverifiableAssembly.DefinePublicType("Type$IDispatchInvoke", typeof(object));
                Compiler method = type.DefineMethod("IDispatchInvoke", typeof(int), paramTypes, null, null);

                LocalBuilder functionPtr = method.DeclareLocal(typeof(IntPtr));

                // functionPtr = *(IntPtr*)(*(dispatchPointer) + VTABLE_OFFSET)

                int idispatchInvokeOffset = ((int)IDispatchMethodIndices.IDispatch_Invoke) * Marshal.SizeOf(typeof(IntPtr));
                method.EmitArgGet(dispatchPointerIndex);
                method.Emit(OpCodes.Ldind_I);
                method.Emit(OpCodes.Ldc_I4, idispatchInvokeOffset);
                method.Emit(OpCodes.Add);
                method.Emit(OpCodes.Ldind_I);
                method.Emit(OpCodes.Stloc, functionPtr);

                // return functionPtr(...)

                method.EmitArgGet(dispatchPointerIndex);
                method.EmitArgGet(memberDispIdIndex);
                method.Emit(OpCodes.Ldsfld, typeof(ComRuntimeHelpers.UnsafeMethods).GetField("IID_NULL")); // riid
                method.Emit(OpCodes.Ldc_I4_0); // lcid
                method.EmitArgGet(flagsIndex);
                method.EmitArgGet(dispParamsIndex);
                method.EmitArgGet(resultIndex);
                method.EmitArgGet(exceptInfoIndex);
                method.EmitArgGet(argErrIndex);
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

                method.EmitReturn();

                type.FinishType();
                return (IDispatchInvokeDelegate)method.CreateDelegate(typeof(IDispatchInvokeDelegate));
            }

            #endregion
        }
    }
}

#endif
