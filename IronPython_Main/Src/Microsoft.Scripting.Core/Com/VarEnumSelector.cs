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

using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;

using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.ComInterop {
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
        private readonly VariantBuilder[] _variantBuilders;
        private readonly ReturnBuilder _returnBuilder;

        private static readonly Dictionary<VarEnum, Type> _ComToManagedPrimitiveTypes = CreateComToManagedPrimitiveTypes();
        private static readonly IList<IList<VarEnum>> _ComPrimitiveTypeFamilies = CreateComPrimitiveTypeFamilies();

        internal VarEnumSelector(Type returnType, Type[] explicitArgTypes) {
            _variantBuilders = new VariantBuilder[explicitArgTypes.Length];

            for (int i = 0; i < explicitArgTypes.Length; i++) {
                _variantBuilders[i] = GetVariantBuilder(explicitArgTypes[i]);
            }

            _returnBuilder = new ReturnBuilder(returnType);
        }

        internal ReturnBuilder ReturnBuilder {
            get {
                return _returnBuilder;
            }
        }

        internal VariantBuilder[] VariantBuilders {
            get {
                return _variantBuilders;
            }
        }

        /// <summary>
        /// Gets the managed type that an object needs to be coverted to in order for it to be able
        /// to be represented as a Variant.
        /// 
        /// In general, there is a many-to-many mapping between Type and VarEnum. However, this method
        /// returns a simple mapping that is needed for the current implementation. The reason for the 
        /// many-to-many relation is:
        /// 1. Int32 maps to VT_I4 as well as VT_ERROR, and Decimal maps to VT_DECIMAL and VT_CY. However,
        ///    this changes if you throw the wrapper types into the mix.
        /// 2. There is no Type to represent COM types. __ComObject is a private type, and Object is too
        ///    general.
        /// </summary>
        internal static Type GetManagedMarshalType(VarEnum varEnum) {
            Debug.Assert((varEnum & VarEnum.VT_BYREF) == 0);

            if (varEnum == VarEnum.VT_CY) {
                return typeof(CurrencyWrapper);
            }

            if (Variant.IsPrimitiveType(varEnum)) {
                return _ComToManagedPrimitiveTypes[varEnum];
            }

            switch (varEnum) {
                case VarEnum.VT_EMPTY:
                case VarEnum.VT_NULL:
                case VarEnum.VT_UNKNOWN:
                case VarEnum.VT_DISPATCH:
                case VarEnum.VT_VARIANT:
                    return typeof(Object);

                case VarEnum.VT_ERROR:
                    return typeof(ErrorWrapper);

                default:
                    throw Error.UnexpectedVarEnum(varEnum);
            }
        }

        private static Dictionary<VarEnum, Type> CreateComToManagedPrimitiveTypes() {
            Dictionary<VarEnum, Type> dict = new Dictionary<VarEnum, Type>();

            #region Generated ComToManagedPrimitiveTypes

            // *** BEGIN GENERATED CODE ***
            // generated by function: gen_ComToManagedPrimitiveTypes from: generate_comdispatch.py

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
            dict[VarEnum.VT_VARIANT] = typeof(Object);

            // *** END GENERATED CODE ***

            #endregion

            dict[VarEnum.VT_CY] = typeof(CurrencyWrapper);
            dict[VarEnum.VT_ERROR] = typeof(ErrorWrapper);
            dict[VarEnum.VT_DISPATCH] = typeof(DispatchWrapper);
            dict[VarEnum.VT_UNKNOWN] = typeof(UnknownWrapper);

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
                new VarEnum[] { VarEnum.VT_DATE },
                new VarEnum[] { VarEnum.VT_R8, VarEnum.VT_R4 },
                new VarEnum[] { VarEnum.VT_DECIMAL },
                new VarEnum[] { VarEnum.VT_BSTR },

                // wrappers
                new VarEnum[] { VarEnum.VT_CY },
                new VarEnum[] { VarEnum.VT_ERROR },
                new VarEnum[] { VarEnum.VT_DISPATCH },
                new VarEnum[] { VarEnum.VT_UNKNOWN },
            };

            return typeFamilies;
        }

        /// <summary>
        /// Get the (one representative type for each) primitive type families that the argument can be converted to
        /// </summary>
        private static List<VarEnum> GetConversionsToComPrimitiveTypeFamilies(Type argumentType) {
            List<VarEnum> compatibleComTypes = new List<VarEnum>();

            foreach (IList<VarEnum> typeFamily in _ComPrimitiveTypeFamilies) {
                foreach (VarEnum candidateType in typeFamily) {
                    Type candidateManagedType = _ComToManagedPrimitiveTypes[candidateType];
                    if (TypeUtils.IsImplicitlyConvertible(argumentType, candidateManagedType, true)) {
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


            throw Error.AmbiguousConversion(argumentType.Name, typeNames);
        }

        private static bool TryGetPrimitiveComType(Type argumentType, out VarEnum primitiveVarEnum) {
            // Look for an exact match with a COM primitive type

            foreach (KeyValuePair<VarEnum, Type> kvp in _ComToManagedPrimitiveTypes) {
                if (kvp.Value == argumentType) {
                    primitiveVarEnum = kvp.Key;
                    return true;
                }
            }

            primitiveVarEnum = VarEnum.VT_VOID; // error
            return false;
        }

        /// <summary>
        /// Is there a unique primitive type that has the best conversion for the argument
        /// </summary>
        private static bool TryGetPrimitiveComTypeViaConversion(Type argumentType, out VarEnum primitiveVarEnum) {
            // Look for a unique type family that the argument can be converted to.
            List<VarEnum> compatibleComTypes = GetConversionsToComPrimitiveTypeFamilies(argumentType);
            CheckForAmbiguousMatch(argumentType, compatibleComTypes);
            if (compatibleComTypes.Count == 1) {
                primitiveVarEnum = compatibleComTypes[0];
                return true;
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

        private VarEnum GetComType(ref Type argumentType) {
            if (argumentType == typeof(Missing)) {
                //TODO: consider specialcasing marshaling for Missing as VT_ERROR | E_PARAMNOTFOUND 
                return VarEnum.VT_VARIANT;
            }

            if (argumentType.IsArray) {
                //TODO: consider specialcasing marshaling for Arrays as VT_ARRAY
                return VarEnum.VT_VARIANT;
            }

            if (argumentType == typeof(UnknownWrapper)) {
                return VarEnum.VT_UNKNOWN;
            } else if (argumentType == typeof(DispatchWrapper)) {
                return VarEnum.VT_DISPATCH;
            } else if (argumentType == typeof(VariantWrapper)) {
                return VarEnum.VT_VARIANT;
            } else if (argumentType == typeof(BStrWrapper)) {
                return VarEnum.VT_BSTR;
            } else if (argumentType == typeof(ErrorWrapper)) {
                return VarEnum.VT_ERROR;
            } else if (argumentType == typeof(CurrencyWrapper)) {
                return VarEnum.VT_CY;
            }

            // Many languages require an explicit cast for an enum to be used as the underlying type.
            // However, we want to allow this conversion for COM without requiring an explicit cast
            // so that enums from interop assemblies can be used as arguments. 
            if (argumentType.IsEnum) {
                argumentType = Enum.GetUnderlyingType(argumentType);
                return GetComType(ref argumentType);
            }

            // COM cannot express valuetype nulls so we will convert to underlying type
            // it will throw if there is no value
            if (TypeUtils.IsNullableType(argumentType)) {
                argumentType = TypeUtils.GetNonNullableType(argumentType);
                return GetComType(ref argumentType);
            }

            if (argumentType.IsCOMObject) {
                return VT_DISPATCH_OR_UNKNOWN;
            }

            VarEnum primitiveVarEnum;
            if (TryGetPrimitiveComType(argumentType, out primitiveVarEnum)) {
                return primitiveVarEnum;
            }

            if (argumentType.IsValueType) {
                //TODO: consider specialcasing structs as VT_RECORD
                return VarEnum.VT_VARIANT;
            }

            // We could not find a way to marshal the type as a specific COM type
            // So we just indicate that it is an unknown value type (VT_RECORD) 
            // or unknown reference type (VT_DISPATCH_OR_UNKNOWN)
            // Note that callers may still find a less generic marshalling method if
            // the type implements IConvertible and if it is applicable

            //default marshal type
            return VT_DISPATCH_OR_UNKNOWN;
        }

        /// <summary>
        /// Get the COM Variant type that argument should be marshaled as for a call to COM
        /// </summary>
        private VariantBuilder GetVariantBuilder(Type argumentType) {
            if (argumentType == Null.Type) {
                return new VariantBuilder(VarEnum.VT_EMPTY, new NullArgBuilder());
            }

            if (argumentType == typeof(DBNull)) {
                return new VariantBuilder(VarEnum.VT_NULL, new NullArgBuilder());
            }

            ArgBuilder argBuilder;

            if (argumentType.IsByRef) {
                Type elementType = argumentType.GetElementType();

                VarEnum elementVarEnum;
                if (elementType == typeof(object)) {
                    //no type information known for a ref argument. 
                    //This parameter must accept any value including primitives so it should be a VT_VARIANT.
                    elementVarEnum = VarEnum.VT_VARIANT;
                } else {
                    elementVarEnum = GetComType(ref elementType);
                }

                argBuilder = GetSimpleArgBuilder(elementType, elementVarEnum);
                return new VariantBuilder(elementVarEnum | VarEnum.VT_BYREF, argBuilder);
            }

            Debug.Assert(!(argumentType.IsGenericType && argumentType.GetGenericTypeDefinition() == typeof(StrongBox<>)), "should not have StrongBox here");

            VarEnum varEnum = GetComType(ref argumentType);
            argBuilder = GetByValArgBuilder(argumentType, ref varEnum);

            return new VariantBuilder(varEnum, argBuilder);
        }


        // This helper is called when we are looking for a ByVal marhsalling
        // In a ByVal case we can take into account conversions or IConvertible if all other 
        // attempts to find marshalling type failed 
        private static ArgBuilder GetByValArgBuilder(Type elementType, ref VarEnum elementVarEnum) {
            // if VT indicates that marshalling type is unknown
            if (elementVarEnum == VarEnum.VT_VARIANT || elementVarEnum == VT_DISPATCH_OR_UNKNOWN) {
                //trying to find a conversion.
                VarEnum convertibleTo;
                if (TryGetPrimitiveComTypeViaConversion(elementType, out convertibleTo)) {
                    elementVarEnum = convertibleTo;
                    Type marshalType = GetManagedMarshalType(elementVarEnum);
                    return new ConversionArgBuilder(elementType, GetSimpleArgBuilder(marshalType, elementVarEnum));
                }

                //checking for IConvertible.
                if (typeof(IConvertible).IsAssignableFrom(elementType)) {
                    return new ConvertibleArgBuilder();
                }
            }
            return GetSimpleArgBuilder(elementType, elementVarEnum);
        }

        // This helper can produce a builder for types that are directly supported by Variant.
        private static SimpleArgBuilder GetSimpleArgBuilder(Type elementType, VarEnum elementVarEnum) {
            SimpleArgBuilder argBuilder;

            switch (elementVarEnum) {
                case VarEnum.VT_BSTR:
                    argBuilder = new StringArgBuilder(elementType);
                    break;
                case VarEnum.VT_BOOL:
                    argBuilder = new BoolArgBuilder(elementType);
                    break;
                case VarEnum.VT_DATE:
                    argBuilder = new DateTimeArgBuilder(elementType);
                    break;
                case VarEnum.VT_CY:
                    argBuilder = new CurrencyArgBuilder(elementType);
                    break;
                case VarEnum.VT_DISPATCH:
                    argBuilder = new DispatchArgBuilder(elementType);
                    break;
                case VarEnum.VT_UNKNOWN:
                    argBuilder = new UnknownArgBuilder(elementType);
                    break;
                case VarEnum.VT_VARIANT:
                    argBuilder = new VariantArgBuilder(elementType);
                    break;
                case VarEnum.VT_ERROR:
                    argBuilder = new ErrorArgBuilder(elementType);
                    break;
                default:
                    argBuilder = new SimpleArgBuilder(elementType);
                    break;
            }

            return argBuilder;
        }
    }
}

#endif
