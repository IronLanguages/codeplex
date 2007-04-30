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
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;
using Microsoft.Scripting;
using System.Collections.Generic;

using Microsoft.Scripting.Internal;
using Microsoft.Scripting.Internal.Generation;

namespace Microsoft.Scripting.Generation {
    /// <summary>
    /// Environment factory which constructs storage into a tuple-backed FunctionEnvironment.
    /// 
    /// Used for environments with less than 128 members.
    /// </summary>
    class PropertyEnvironmentFactory : EnvironmentFactory {
        private Type _type;
        private Type _envType;
        private PropertyInfo[] _properties;
        private int _index;
        private static Dictionary<Type, PropertyInfo[]> _cachedProps;

        /// <summary>
        /// Creates a new PropertyEnvironmentFactory backed by the specified type of tuple and
        /// FunctionEnvironment.
        /// </summary>
        public PropertyEnvironmentFactory(Type tupleType, Type envType) {
            ValidateTupleType(tupleType);

            _type = tupleType;
            _envType = envType;
            _properties = GetProperties(tupleType);

            // 1st entry always points back to our dictionary
            MakeEnvironmentReference(SymbolTable.StringToId("$env"), typeof(IAttributesCollection));
        }

        public override Type EnvironmentType {
            get { return _envType; }
        }

        public override Type StorageType {
            get {
                return _type;
            }
        }

        public override Storage MakeEnvironmentReference(SymbolId name, Type type) {
            Debug.Assert(_index < _properties.Length);
            return new PropertyEnvironmentReference(_properties[_index++], type);
        }

        protected PropertyInfo[] Properties {
            get {
                return _properties;
            }
        }

        protected int Index {
            get { return _index; }
            set { _index = value; }
        }

        public override void EmitStorage(CodeGen cg) {
            cg.EmitNew(StorageType.GetConstructor(new Type[0]));
        }

        public override void EmitNewEnvironment(CodeGen cg) {
            ConstructorInfo ctor = EnvironmentType.GetConstructor(
                new Type[] {
                    StorageType,
                    typeof(SymbolId[]),
                });

            // emit: dict.Tuple.Item0 = dict, and then leave dict on the stack

            cg.EmitNew(ctor);

            cg.Emit(OpCodes.Dup);

            Slot tmp = cg.GetLocalTmp(EnvironmentType);
            tmp.EmitSet(cg);

            cg.EmitPropertyGet(EnvironmentType, "Tuple");
            tmp.EmitGet(cg);
            cg.EmitPropertySet(StorageType, "Item000");

            cg.FreeLocalTmp(tmp);
        }

        public override void EmitGetStorageFromContext(CodeGen cg) {
            cg.EmitCodeContext();
            cg.EmitPropertyGet(typeof(CodeContext), "Scope");
            cg.EmitCall(typeof(RuntimeHelpers).GetMethod("GetTupleDictionaryData").MakeGenericMethod(StorageType));
        }

        public override EnvironmentSlot CreateEnvironmentSlot(CodeGen cg) {
            return new FunctionEnvironmentSlot(cg.GetNamedLocal(StorageType, "$environment"), StorageType);
        }

        /// <summary>
        /// Private helper to return the list of properties for our underlying tuple storage type.
        /// </summary>
        private static PropertyInfo[] GetProperties(Type type) {
            PropertyInfo[] props;

            if (_cachedProps == null || !_cachedProps.TryGetValue(type, out props)) {
                props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                // need to sort the names as reflection can pull them out in any order
                Array.Sort(props, delegate(PropertyInfo x, PropertyInfo y) {
                    return string.Compare(x.Name, y.Name);
                });

                if (_cachedProps == null) _cachedProps = new Dictionary<Type, PropertyInfo[]>();
                _cachedProps[type] = props;
            }
            return props;
        }

        [Conditional("DEBUG")]
        private static void ValidateTupleType(Type type) {
            Type curType = type.BaseType;
            while (curType != typeof(NewTuple)) {
                Debug.Assert(curType != typeof(object));
                curType = curType.BaseType;
            }
        }

    }
}