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
using System.Collections.Generic;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Generation {
    /// <summary>
    /// Environment factory which constructs storage into a tuple-backed FunctionEnvironment.
    /// 
    /// Used for environments with less than 128 members.
    /// </summary>
    class PropertyEnvironmentFactory : EnvironmentFactory {
        private Type _type;
        private Type _envType;
        private int _index;

        /// <summary>
        /// Creates a new PropertyEnvironmentFactory backed by the specified type of tuple and
        /// FunctionEnvironment.
        /// </summary>
        public PropertyEnvironmentFactory(Type tupleType, Type envType) {
            ValidateTupleType(tupleType);

            _type = tupleType;
            _envType = envType;

            // 1st entry always points back to our dictionary            
            _index = 1;
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
            return new PropertyEnvironmentReference(_type.GetProperty("Item" + (_index++).ToString("D3")), type);
        }

        protected int Index {
            get { return _index; }
            set { _index = value; }
        }

        public override void EmitStorage(CodeGen cg) {
            cg.EmitNew(StorageType.GetConstructor(ArrayUtils.EmptyTypes));
            cg.Emit(OpCodes.Dup);
            cg.EmitCall(typeof(RuntimeHelpers), "UninitializeEnvironmentTuple");
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
