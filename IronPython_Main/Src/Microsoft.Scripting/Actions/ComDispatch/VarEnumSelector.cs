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
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using ComTypes = System.Runtime.InteropServices.ComTypes;

using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Actions.ComDispatch {
    /// <summary>
    /// If a managed user type (as opposed to a primitive type or a COM object) is passed as an argument to a COM call, we need
    /// to determine the VarEnum type we will marshal it as. We have the following options:
    /// 1.	Raise an exception. Languages with their own version of primitive types would not be able to call
    ///     COM methods using the language's types (for eg. strings in IronRuby are not System.String). An explicit
    ///     cast would be needed.
    /// 2.	We could marshal it as VT_DISPATCH. Then COM code will be able to access all the APIs in a late-bound manner,
    ///     but old COM components will probably malfunction if they expect a primitive type.
    /// 3.	We could guess which primitive type is the closest match. This will make COM components be as easily 
    ///     accessible as .NET methods.
    /// 4.	We could use the type library to check what the expected type is. However, the type library may not be available.
    /// 
    /// VarEnumSelector implements option # 3
    /// </summary>
    internal class VarEnumSelector {

        /// <summary>
        /// Describes how the user arguments should be represented as Variants for the late-bound COM call.
        /// </summary>
        internal struct DispatchArgumentInfo {
            private VarEnum _marshalType;

            internal VarEnum VariantType {
                get {
                    return _marshalType;
                }
                set {
                    _marshalType = value;
                }
            }
        }

        private object[] _userArguments; // excluding the IDispatch instance
        ActionBinder _binder;

        private DispatchArgumentInfo[] _dispatchArguments;
        private bool _isSupportedByFastPath = true;

        private static readonly Dictionary<VarEnum, Type> _ComToManagedPrimitiveTypes = CreateComToManagedPrimitiveTypes();
        private static readonly IList<IList<VarEnum>> _ComPrimitiveTypeFamilies = CreateComPrimitiveTypeFamilies();

        /// <summary>
        /// This constructor infers the COM types to marshal the arguments to based on
        /// the conversions supported for the given argument type.
        /// </summary>
        internal VarEnumSelector(ActionBinder binder, object[] args) {
            _userArguments = args;
            _binder = binder;

            _dispatchArguments = new DispatchArgumentInfo[args.Length];

            for (int i = 0; i < args.Length; i++) {
                _dispatchArguments[i].VariantType = GetComType(args[i]);
            }
        }

        /// <summary>
        /// Can IDispatchCallBinderHelper generate an optimized DynamicSite, or should we fall back
        /// to the slow DispCallable.UpoptimizedInvoke
        /// </summary>
        internal bool IsSupportedByFastPath {
            get {
                return _isSupportedByFastPath;
            }
        }

        internal DispatchArgumentInfo[] DispatchArguments {
            get {
                return _dispatchArguments;
            }
        }

        /// <summary>
        /// Gets the arguments ready for marshaling to COM
        /// </summary>
        internal object[] ConvertArguments() {
            object[] convertedArguments = new object[_userArguments.Length];

            for (int i = 0; i < _userArguments.Length; i++) {
                object userArgument = _userArguments[i];
                VarEnum targetComType = _dispatchArguments[i].VariantType;
                Type targetManagedType = GetManagedMarshalType(targetComType);
                convertedArguments[i] = _binder.Convert(userArgument, targetManagedType);
            }

            return convertedArguments;
        }

        /// <summary>
        /// Gets the managed type that an object needs to be coverted to in order for it to be able
        /// to be represented as a Variant
        /// </summary>
        internal static Type GetManagedMarshalType(VarEnum varEnum) {
            if (Variant.IsPrimitiveType(varEnum)) {
                return _ComToManagedPrimitiveTypes[varEnum];
            }

            switch(varEnum) {
                case VarEnum.VT_EMPTY:
                case VarEnum.VT_NULL:
                case VarEnum.VT_UNKNOWN:
                case VarEnum.VT_DISPATCH:
                    return typeof(Object);
                default:
                    throw new NotImplementedException("Unexpected VarEnum " + varEnum);
            }
        }

        private static Dictionary<VarEnum, Type> CreateComToManagedPrimitiveTypes() {
            Dictionary<VarEnum, Type> dict = new Dictionary<VarEnum, Type>();

            #region Generated ComToManagedPrimitiveTypes

            // *** BEGIN GENERATED CODE ***

            dict[VarEnum.VT_I1] = typeof(SByte);
            dict[VarEnum.VT_I2] = typeof(Int16);
            dict[VarEnum.VT_I4] = typeof(Int32);
            dict[VarEnum.VT_I8] = typeof(Int64);
            dict[VarEnum.VT_UI1] = typeof(Byte);
            dict[VarEnum.VT_UI2] = typeof(UInt16);
            dict[VarEnum.VT_UI4] = typeof(UInt32);
            dict[VarEnum.VT_UI8] = typeof(UInt64);
            dict[VarEnum.VT_INT] = typeof(IntPtr);
            dict[VarEnum.VT_UINT] = typeof(UIntPtr);
            dict[VarEnum.VT_BOOL] = typeof(bool);
            dict[VarEnum.VT_R4] = typeof(Single);
            dict[VarEnum.VT_R8] = typeof(Double);
            dict[VarEnum.VT_DECIMAL] = typeof(Decimal);
            dict[VarEnum.VT_DATE] = typeof(DateTime);
            dict[VarEnum.VT_BSTR] = typeof(String);

            // *** END GENERATED CODE ***

            #endregion

            return dict;
        }

        #region Primitive COM types

        /// <summary>
        /// Creates a family of COM types such that within each family, there is a completely non-lossy
        /// conversion from a type to an earlier type in the family.
        /// </summary>
        private static IList<IList<VarEnum>> CreateComPrimitiveTypeFamilies() {
            VarEnum[][] typeFamilies = new VarEnum[][] {
                new VarEnum[] { VarEnum.VT_I8, VarEnum.VT_I4, VarEnum.VT_I2, VarEnum.VT_I1 },
                new VarEnum[] { VarEnum.VT_UI8, VarEnum.VT_UI4, VarEnum.VT_UI2, VarEnum.VT_UI1 },
                new VarEnum[] { VarEnum.VT_INT },
                new VarEnum[] { VarEnum.VT_UINT },
                new VarEnum[] { VarEnum.VT_BOOL },
                new VarEnum[] { VarEnum.VT_DATE, VarEnum.VT_R8, VarEnum.VT_R4 },
                new VarEnum[] { VarEnum.VT_DECIMAL },
                new VarEnum[] { VarEnum.VT_BSTR }
            };

            return typeFamilies;
        }

        /// <summary>
        /// Get the (one representative type for each) primitive type families that the argument can be converted to
        /// at the given NarrowingLevel.
        /// </summary>
        private List<VarEnum> GetConversionsToComPrimitiveTypeFamilies(Type argumentType, NarrowingLevel narrowingLevel) {
            List<VarEnum> compatibleComTypes = new List<VarEnum>();

            foreach (IList<VarEnum> typeFamily in _ComPrimitiveTypeFamilies) {
                foreach (VarEnum candidateType in typeFamily) {
                    Type candidateManagedType = _ComToManagedPrimitiveTypes[candidateType];
                    if (_binder.CanConvertFrom(argumentType, candidateManagedType, narrowingLevel)) {
                        compatibleComTypes.Add(candidateType);
                        // Move on to the next type family. We need atmost one type from each family
                        break;
                    }
                }
            }
            return compatibleComTypes;
        }

        /// <summary>
        /// If there is more than one type family that the argument can be converted to, we will throw a
        /// AmbiguousMatchException instead of randomly picking a winner.
        /// </summary>
        private static void CheckForAmbiguousMatch(Type argumentType, List<VarEnum> compatibleComTypes) {
            if (compatibleComTypes.Count <= 1) {
                return;
            }

            String typeNames = "";
            for (int i = 0; i < compatibleComTypes.Count; i++) {
                string typeName = _ComToManagedPrimitiveTypes[compatibleComTypes[i]].Name;
                if (i == (compatibleComTypes.Count - 1)) {
                    typeNames += " and ";
                } else if (i != 0) {
                    typeNames += ", ";
                }
                typeNames += typeName;
            }

            string message = String.Format("There are valid conversions from {0} to {1}", argumentType.Name, typeNames);
            throw new AmbiguousMatchException(message);
        }

        /// <summary>
        /// Is there a unique primitive type that has the best conversion for the argument
        /// </summary>
        private bool TryGetPrimitiveComType(Type argumentType, out VarEnum primitiveVarEnum) {
            // Look for an exact match with a COM primitive type

            foreach (KeyValuePair<VarEnum,Type> kvp in _ComToManagedPrimitiveTypes) {
                if (kvp.Value == argumentType) {
                    primitiveVarEnum = kvp.Key;
                    return true;
                }
            }

            // Look for a unique type family that the argument can be converted to.

            foreach (NarrowingLevel narrowingLevel in Enum.GetValues(typeof(NarrowingLevel))) {
                List<VarEnum> compatibleComTypes = GetConversionsToComPrimitiveTypeFamilies(argumentType, narrowingLevel);
                CheckForAmbiguousMatch(argumentType, compatibleComTypes);
                if (compatibleComTypes.Count == 1) {
                    primitiveVarEnum = compatibleComTypes[0];
                    return true;
                }
            }

            primitiveVarEnum = VarEnum.VT_VOID; // error
            return false;
        }

        #endregion

        // Type.InvokeMember tries to marshal objects as VT_DISPATCH, and falls back to VT_UNKNOWN if IDispatch
        // cannot be supported. For now, we will just support VT_DISPATCH. We might want to move to VT_UNKNOWN
        // as doing a QueryInterface for IID_IDispatch could be expensive, and it should be upto the callee
        // to do the QueryInterface if needed
        const VarEnum VT_DISPATCH_OR_UNKNOWN = VarEnum.VT_DISPATCH;

        private VarEnum GetComTypeNonArray(Type argumentType) {
            Debug.Assert(!argumentType.IsArray);

            if (argumentType == typeof(UnknownWrapper)) {
                _isSupportedByFastPath = false;
                return VarEnum.VT_UNKNOWN;
            } else if (argumentType == typeof(DispatchWrapper)) {
                _isSupportedByFastPath = false;
                return VarEnum.VT_DISPATCH;
            } else if (argumentType == typeof(BStrWrapper)) {
                _isSupportedByFastPath = false;
                return VarEnum.VT_BSTR;
            } else if (argumentType == typeof(ErrorWrapper)) {
                _isSupportedByFastPath = false;
                return VarEnum.VT_ERROR;
            } else if (argumentType == typeof(CurrencyWrapper)) {
                _isSupportedByFastPath = false;
                return VarEnum.VT_CY;
            }

            if (argumentType.IsEnum) {
                Type underlyingType = Enum.GetUnderlyingType(argumentType);
                return GetComTypeNonArray(underlyingType);
            }

            if (argumentType.IsCOMObject) {
                return VT_DISPATCH_OR_UNKNOWN;
            }

            VarEnum primitiveVarEnum;
            if (TryGetPrimitiveComType(argumentType, out primitiveVarEnum)) {
                return primitiveVarEnum;
            }

            if (argumentType.IsValueType) {
                _isSupportedByFastPath = false;
                return VarEnum.VT_RECORD;
            }

            // Marshal as an RCW (Runtime-callable wrapper)
            return VT_DISPATCH_OR_UNKNOWN;
        }

        private VarEnum GetComType(Type argumentType) {
            if (argumentType.IsArray) {
                Type elementType = argumentType.GetElementType();

                // Arrays of arrays will get marshaled as VT_ARRAY|VT_DISPATCH. We will not support marshaling
                // of the inner arrays as VT_ARRAY.
                VarEnum elementComType = GetComTypeNonArray(elementType);

                _isSupportedByFastPath = false;
                return (VarEnum.VT_ARRAY | elementComType);
            }

            if (argumentType.IsGenericType && argumentType.GetGenericTypeDefinition() == typeof(StrongBox<>)) {
                _isSupportedByFastPath = false;
                Type boxedType = argumentType.GetGenericArguments()[0];
                return GetComType(boxedType);
            }

            return GetComTypeNonArray(argumentType);
        }

        /// <summary>
        /// Get the COM Variant type that argument should be marshaled as for a call to COM
        /// </summary>
        private VarEnum GetComType(object argument) {
            if (argument == null) {
                return VarEnum.VT_EMPTY;
            }

            if (argument == DBNull.Value) {
                return VarEnum.VT_NULL;
            }

            if (argument == Type.Missing) {
                _isSupportedByFastPath = false;
                return VarEnum.VT_ERROR;
            }

            return GetComType(argument.GetType());
        }
    }
}

#endif