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

namespace Microsoft.Scripting.Com {
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
        /// Narrowing conversions are conversions that cannot be proved to always succeed, conversions that are 
        /// known to possibly lose information, and conversions across domains of types sufficiently different 
        /// to merit narrowing notation like casts. 
        /// 
        /// Its upto every language to define the levels for conversions. The narrowling levels can be used by
        /// for method overload resolution, where the overload is based on the parameter types (and not the number 
        /// of parameters).
        /// </summary>
        private enum NarrowingLevel {
            /// <summary>
            /// Conversions at this level do not do any narrowing. Typically, this will include
            /// implicit numeric conversions, Type.IsAssignableFrom, StringBuilder to string, etc.
            /// </summary>
            None,
            /// <summary>
            /// Language defined prefered narrowing conversion.  First level that introduces narrowing
            /// conversions.
            /// </summary>
            One,
            /// <summary>
            /// Language defined preferred narrowing conversion.  Second level that introduces narrowing
            /// conversions and should have more conversions than One.
            /// </summary>
            Two,
            /// <summary>
            /// Language defined preferred narrowing conversion.  Third level that introduces narrowing
            /// conversions and should have more conversions that Two.
            /// </summary>
            Three,
            /// <summary>
            /// A somewhat meaningful conversion is possible, but it will quite likely be lossy.
            /// For eg. BigInteger to an Int32, Boolean to Int32, one-char string to a char,
            /// larger number type to a smaller numeric type (where there is no overflow), etc
            /// </summary>
            All
        }

        private VariantBuilder[] _variantBuilders;
        private ReturnBuilder _returnBuilder;
        private bool _isSupportedByFastPath = true;
        private static readonly Dictionary<VarEnum, Type> _ComToManagedPrimitiveTypes = CreateComToManagedPrimitiveTypes();
        private static readonly IList<IList<VarEnum>> _ComPrimitiveTypeFamilies = CreateComPrimitiveTypeFamilies();

        /// <summary>
        /// This constructor infers the COM types to marshal the arguments to based on
        /// the conversions supported for the given argument type.
        /// </summary>
        internal VarEnumSelector(Type returnType, object[] explicitArgs)
            : this(returnType, TypeUtils.GetTypesForBinding(explicitArgs)) {
        }

        internal VarEnumSelector(Type returnType, Type[] explicitArgTypes) {
            _variantBuilders = new VariantBuilder[explicitArgTypes.Length];

            for (int i = 0; i < explicitArgTypes.Length; i++) {
                _variantBuilders[i] = GetVariantBuilder(i, explicitArgTypes[i]);
            }

            _returnBuilder = new ReturnBuilder(returnType);
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

        internal ArgBuilder[] GetArgBuilders() {
            ArgBuilder[] argBuilders = new ArgBuilder[_variantBuilders.Length];
            for (int i = 0; i < _variantBuilders.Length; i++) {
                argBuilders[i] = _variantBuilders[i].ArgBuilder;
            }
            return argBuilders;
        }

        /// <summary>
        /// Gets the arguments ready for marshaling to COM
        /// </summary>
        internal object[] BuildArguments(object[] args, out ParameterModifier parameterModifiers) {
            object[] convertedArguments = new object[_variantBuilders.Length];
            if (_variantBuilders.Length != 0) {
                parameterModifiers = new ParameterModifier(_variantBuilders.Length);
            } else {
                parameterModifiers = new ParameterModifier();
            }

            for (int i = 0; i < _variantBuilders.Length; i++) {
                convertedArguments[i] = _variantBuilders[i].Build(args);
                parameterModifiers[i] = _variantBuilders[i].IsByRef;
            }

            return convertedArguments;
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
                case VarEnum.VT_CY:
                    return typeof(CurrencyWrapper);

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
        private static List<VarEnum> GetConversionsToComPrimitiveTypeFamilies(Type argumentType) {
            List<VarEnum> compatibleComTypes = new List<VarEnum>();

            foreach (IList<VarEnum> typeFamily in _ComPrimitiveTypeFamilies) {
                foreach (VarEnum candidateType in typeFamily) {
                    Type candidateManagedType = _ComToManagedPrimitiveTypes[candidateType];
                    if (candidateManagedType.IsAssignableFrom(argumentType)) {
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

        // We do not use NarrowingLevel.All as it can potentially return degenerate conversions.
        static private readonly NarrowingLevel[] _ComNarrowingLevels = new NarrowingLevel[] { 
            NarrowingLevel.None, 
            NarrowingLevel.One
        };

        /// <summary>
        /// Is there a unique primitive type that has the best conversion for the argument
        /// </summary>
        private static bool TryGetPrimitiveComType(Type argumentType, out VarEnum primitiveVarEnum) {
            // Look for an exact match with a COM primitive type

            foreach (KeyValuePair<VarEnum, Type> kvp in _ComToManagedPrimitiveTypes) {
                if (kvp.Value == argumentType) {
                    primitiveVarEnum = kvp.Key;
                    return true;
                }
            }

            // Look for a unique type family that the argument can be converted to.

            // Assert the unused values. If the enum is changed, the logic below should be checked to ensure
            // that it is using the approriate enum values.
            Debug.Assert(((int)NarrowingLevel.Two) == 2 && ((int)NarrowingLevel.Three) == 3 && ((int)NarrowingLevel.All) == 4);

            foreach (NarrowingLevel narrowingLevel in _ComNarrowingLevels) {
                List<VarEnum> compatibleComTypes = GetConversionsToComPrimitiveTypeFamilies(argumentType);
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

            // Many languages require an explicit cast for an enum to be used as the underlying type.
            // However, we want to allow this conversion for COM without requiring an explicit cast
            // so that enums from interop assemblies can be used as arguments. 
            // TODO: This will not be needed once we enable loading type libraries and using the enum
            // definition from the type library. We need to revisit this and decide if we want to allow enums to be used as the underlying type even if the language does not support the conversion
            if (argumentType.IsEnum) {
                // The slow path automatically supports such a conversion since Type.InvokeMember supports it.
                _isSupportedByFastPath = false;

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
            if (argumentType == typeof(Missing)) {
                _isSupportedByFastPath = false;
                return VarEnum.VT_ERROR;
            }

            if (argumentType.IsArray) {
                Type elementType = argumentType.GetElementType();

                // Arrays of arrays will get marshaled as VT_ARRAY|VT_DISPATCH. We will not support marshaling
                // of the inner arrays as VT_ARRAY.
                VarEnum elementComType = GetComTypeNonArray(elementType);

                _isSupportedByFastPath = false;
                return (VarEnum.VT_ARRAY | elementComType);
            }

            return GetComTypeNonArray(argumentType);
        }

        /// <summary>
        /// Get the COM Variant type that argument should be marshaled as for a call to COM
        /// </summary>
        private VarEnum GetComType(int argIndex, Type argumentType, out ArgBuilder argBuilder) {
            if (argumentType == None.Type) {
                argBuilder = new NullArgBuilder();
                return VarEnum.VT_EMPTY;
            }

            if (argumentType == typeof(DBNull)) {
                argBuilder = new NullArgBuilder();
                return VarEnum.VT_NULL;
            }

            if (argumentType.IsGenericType && argumentType.GetGenericTypeDefinition() == typeof(StrongBox<>)) {
                Type elementType = argumentType.GetGenericArguments()[0];
                VarEnum elementVarEnum = GetComType(elementType);
                if (elementType == typeof(string)) {
                    argBuilder = new ComReferenceArgBuilder(argIndex, argumentType);
                } else {
                    if (!Variant.HasCommonLayout(elementVarEnum)) {
                        _isSupportedByFastPath = false;
                    }
                    argBuilder = new ReferenceArgBuilder(argIndex, argumentType);
                }
                return (elementVarEnum | VarEnum.VT_BYREF);
            }

            VarEnum varEnum = GetComType(argumentType);
            argBuilder = new SimpleArgBuilder(argIndex, GetManagedMarshalType(varEnum));
            return varEnum;
        }

        private VariantBuilder GetVariantBuilder(int explicitArgIndex, Type argumentType) {
            ArgBuilder argBuilder;
            VarEnum varEnum = GetComType(explicitArgIndex + 1, argumentType, out argBuilder); // + 1 for the IDispatch instance argument
            VariantBuilder variantBuilder = new VariantBuilder(varEnum, argBuilder);
            return variantBuilder;
        }
    }
}

#endif