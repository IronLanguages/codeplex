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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Scripting;
using System.Scripting.Actions;
using Microsoft.Scripting.Actions;

namespace Microsoft.Scripting.Generation {
    /// <summary>
    /// ParameterWrapper represents the logical view of a parameter. For eg. the byref-reduced signature
    /// of a method with byref parameters will be represented using a ParameterWrapper of the underlying
    /// element type, since the logical view of the byref-reduced signature is that the argument will be
    /// passed by value (and the updated value is included in the return value).
    /// 
    /// Contrast this with ArgBuilder which represents the real physical argument passed to the method.
    /// </summary>
    public class ParameterWrapper {
        private readonly Type _type;
        private readonly bool _prohibitNull, _isParams, _isParamsDict;
        private readonly ActionBinder _binder;
        private readonly SymbolId _name;

        public ParameterWrapper(ActionBinder binder, Type type) {
            _type = type;
            _binder = binder;
        }

        public ParameterWrapper(ActionBinder binder, Type type, SymbolId name) 
            : this(binder, type) {
            _name = name;
        }

        public ParameterWrapper(ActionBinder binder, Type type, bool prohibitNull) {
            _type = type;
            _prohibitNull = prohibitNull;
            _binder = binder;
        }

        public ParameterWrapper(ActionBinder binder, Type type, bool prohibitNull, SymbolId name) 
            : this(binder, type, prohibitNull) {
            _name = name;
        }

        public ParameterWrapper(ActionBinder binder, ParameterInfo info)
            : this(binder, info.ParameterType) {            
            _prohibitNull = CompilerHelpers.ProhibitsNull(info);
            _isParams = CompilerHelpers.IsParamArray(info);
            _isParamsDict = BinderHelpers.IsParamDictionary(info);
            if (_isParams || _isParamsDict) {
                // params arrays & dictionaries don't allow assignment by keyword
                _name = SymbolTable.StringToId("<unknown>");
            } else {
                _name = SymbolTable.StringToId(info.Name ?? "<unknown>");
            }
        }

        public static int? CompareParameters(IList<ParameterWrapper> parameters1, IList<ParameterWrapper> parameters2, Type[] actualTypes) {
            Debug.Assert(parameters1.Count == parameters2.Count);
            Debug.Assert(parameters1.Count == actualTypes.Length);

            int? ret = 0;
            for (int i = 0; i < parameters1.Count; i++) {
                ParameterWrapper p1 = parameters1[i];
                ParameterWrapper p2 = parameters2[i];
                int? cmp = p1.CompareTo(p2, actualTypes[i]);
                
                switch (ret) {
                    case 0:
                        ret = cmp; break;
                    case +1:
                        if (cmp == -1) return null;
                        break;
                    case -1:
                        if (cmp == +1) return null;
                        break;
                    case null:
                        if (cmp != 0) ret = cmp;
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            return ret;
        }

        public static int? CompareParameters(IList<ParameterWrapper> parameters1, IList<ParameterWrapper> parameters2, MetaObject[] actualTypes) {
            Debug.Assert(parameters1.Count == parameters2.Count);
            Debug.Assert(parameters1.Count == actualTypes.Length);

            int? ret = 0;
            for (int i = 0; i < parameters1.Count; i++) {
                ParameterWrapper p1 = parameters1[i];
                ParameterWrapper p2 = parameters2[i];
                int? cmp = p1.CompareTo(p2, actualTypes[i]);

                switch (ret) {
                    case 0:
                        ret = cmp; break;
                    case +1:
                        if (cmp == -1) return null;
                        break;
                    case -1:
                        if (cmp == +1) return null;
                        break;
                    case null:
                        if (cmp != 0) ret = cmp;
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            return ret;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
        public Type Type {
            get { return _type; }
        }

        public bool HasConversionFrom(Type fromType, NarrowingLevel allowNarrowing) {
            if (fromType == Type || fromType == typeof(Dynamic)) return true;

            if (fromType == None.Type) {
                if (_prohibitNull) return false;

                if (Type.IsGenericType && Type.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                    return true;
                }
                return !Type.IsValueType || _binder.CanConvertFrom(fromType, Type, _prohibitNull, allowNarrowing);
            } else {
                return _binder.CanConvertFrom(fromType, Type, _prohibitNull, allowNarrowing);
            }
        }

        public int? CompareTo(ParameterWrapper other) {
            Type t1 = Type;
            Type t2 = other.Type;
            bool n1 = _prohibitNull;
            bool n2 = other._prohibitNull;

            if (t1 == t2 && n1 == n2) {
                return 0;
            }

            if (_binder.CanConvertFrom(t2, t1, n1, NarrowingLevel.None)) {
                if (_binder.CanConvertFrom(t1, t2, n2, NarrowingLevel.None)) {
                    return null;
                } else {
                    return -1;
                }
            }
            if (_binder.CanConvertFrom(t1, t2, n2, NarrowingLevel.None)) {
                return +1;
            }

            // Special additional rules to order numeric value types
            if (_binder.PreferConvert(t1, t2)) return -1;
            else if (_binder.PreferConvert(t2, t1)) return +1;

            return null;
        }

        private int? SelectBestConversionFor(Type actualType, Type candidateOne, bool oneNotNull, Type candidateTwo, bool twoNotNull, NarrowingLevel level) {
            return (int?)_binder.SelectBestConversionFor(actualType, candidateOne, oneNotNull, candidateTwo, twoNotNull, level);
        }

        private int? SelectBestConversionFor(MetaObject actualType, Type candidateOne, bool oneNotNull, Type candidateTwo, bool twoNotNull, NarrowingLevel level) {
            return (int?)_binder.SelectBestConversionFor(actualType.LimitType, candidateOne, oneNotNull, candidateTwo, twoNotNull, level);
        }

        public int? CompareTo(ParameterWrapper other, Type actualType) {
            //+1 if t1, -1 if t2, null if no resolution

            Type t1 = Type;
            Type t2 = other.Type;
            bool n1 = _prohibitNull;
            bool n2 = other._prohibitNull;
            if (t1 == t2 && n1 == n2) return 0;
            int? ret = null;

            for (NarrowingLevel curLevel = NarrowingLevel.None; curLevel <= NarrowingLevel.All; curLevel++) {
                ret = SelectBestConversionFor(actualType, t1, n1, t2, n2, curLevel);
                if (ret != null) {
                    return ret;
                }
            }
            
            return CompareTo(other);
        }

        public int? CompareTo(ParameterWrapper other, MetaObject actualType) {
            //+1 if t1, -1 if t2, null if no resolution

            Type t1 = Type;
            Type t2 = other.Type;
            bool n1 = _prohibitNull;
            bool n2 = other._prohibitNull;
            if (t1 == t2 && n1 == n2) return 0;
            int? ret = null;

            for (NarrowingLevel curLevel = NarrowingLevel.None; curLevel <= NarrowingLevel.All; curLevel++) {
                ret = SelectBestConversionFor(actualType, t1, n1, t2, n2, curLevel);
                if (ret != null) {
                    return ret;
                }
            }

            return CompareTo(other);
        }

        public SymbolId Name {
            get {
                return _name;
            }
        }

        public bool IsParamsArray {
            get {
                return _isParams;
            }
        }

        public bool IsParamsDict {
            get {
                return _isParamsDict;
            }
        }

        public string ToSignatureString() {
            return Type.Name;
        }
    }

}
