/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Diagnostics;

using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting {
    class ParameterWrapper {
        private Type _type;
        private bool _prohibitNull, _isParams, _isParamsDict;
        private ActionBinder _binder;
        private SymbolId _name;

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
            _name = SymbolTable.StringToId(info.Name ?? "<unknown>");
            _prohibitNull = info.IsDefined(typeof(NotNullAttribute), false);
            _isParams = info.IsDefined(typeof(ParamArrayAttribute), false);
            _isParamsDict = info.IsDefined(typeof(ParamDictionaryAttribute), false);
        }

        public static int? CompareParameters(IList<ParameterWrapper> parameters1, IList<ParameterWrapper> parameters2) {
            Debug.Assert(parameters1.Count == parameters2.Count);
            int? ret = 0;
            for (int i = 0; i < parameters1.Count; i++) {
                ParameterWrapper p1 = parameters1[i];
                ParameterWrapper p2 = parameters2[i];
                int? cmp = p1.CompareTo(p2);
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

        public Type Type {
            get { return _type; }
        }

        public bool HasConversionFrom(Type ty, NarrowingLevel allowNarrowing) {
            if (ty == Type) return true;

            if (ty == None.Type) {
                if (_prohibitNull) return false;

                if (Type.IsGenericType && Type.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                    return true;
                }
                return !Type.IsValueType;
            } else {
                return _binder.CanConvertFrom(ty, Type, allowNarrowing);
            }
        }

        public int? CompareTo(ParameterWrapper other) {
            Type t1 = Type;
            Type t2 = other.Type;
            if (t1 == t2) return 0;
            if (_binder.CanConvertFrom(t2, t1, NarrowingLevel.None)) {
                if (_binder.CanConvertFrom(t1, t2, NarrowingLevel.None)) {
                    return null;
                } else {
                    return -1;
                }
            }
            if (_binder.CanConvertFrom(t1, t2, NarrowingLevel.None)) {
                return +1;
            }

            // Special additional rules to order numeric value types
            if (_binder.PreferConvert(t1, t2)) return -1;
            else if (_binder.PreferConvert(t2, t1)) return +1;

            return null;
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
            return DynamicHelpers.GetDynamicTypeFromType(Type).Name;
        }
    }

}
