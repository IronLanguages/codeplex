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
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using ComTypes = System.Runtime.InteropServices.ComTypes;

using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Ast;

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

            public static IntPtr ConvertByrefToPtr(out Variant variant) {
                IntPtr ptr = _ConvertByrefToPtr(out variant);
#if DEBUG
                // Ensure that "variant" is a local variable in some caller's frame. So converting
                // the byref to an IntPtr is a safe operation. Alternatively, we could also allow 
                // allowed "variant"  to be a pinned object.
                Variant dummy;
                IntPtr ptrToLocal = _ConvertByrefToPtr(out dummy);
                Debug.Assert(ptrToLocal.ToInt64() < ptr.ToInt64());
                Debug.Assert((ptr.ToInt64() - ptrToLocal.ToInt64()) < (16 * 1024));
#endif
                return ptr;
            }

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

            private delegate IntPtr ConvertByrefToPtrDelegate(out Variant variant);
            private static ConvertByrefToPtrDelegate _ConvertByrefToPtr = Create_ConvertByrefToPtr();

            private static ConvertByrefToPtrDelegate Create_ConvertByrefToPtr() {
                // We dont use AssemblyGen.DefineMethod since that can create a anonymously-hosted DynamicMethod which cannot contain unverifiable code.
                TypeGen type = _UnverifiableAssembly.DefinePublicType("Type$ConvertByrefToPtr", typeof(object));
                Type[] paramTypes = new Type[] { typeof(Variant).MakeByRefType() };
                Compiler method = type.DefineMethod("ConvertByrefToPtr", typeof(IntPtr), paramTypes, null, null);

                method.Emit(OpCodes.Ldarg_0);
                method.Emit(OpCodes.Conv_I);
                method.EmitReturn();

                return (ConvertByrefToPtrDelegate)method.CreateDelegate(typeof(ConvertByrefToPtrDelegate));
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
