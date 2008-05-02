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
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;

using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Ast;

namespace Microsoft.Scripting.Generation {
    /// <summary>
    /// Environment factory which constructs storage into a tuple-backed FunctionEnvironment.
    /// 
    /// Used for environments with less than 128 members.
    /// </summary>
    internal class PropertyEnvironmentFactory : EnvironmentFactory {
        private readonly Type/*!*/ _storageType;
        private int _index;

        /// <summary>
        /// Creates a new PropertyEnvironmentFactory backed by the specified type of tuple and
        /// FunctionEnvironment.
        /// </summary>
        public PropertyEnvironmentFactory(Type/*!*/ storageType) {
            Assert.NotNull(storageType);
            ValidateTupleType(storageType);

            _storageType = storageType;
        }

        public override Type/*!*/ StorageType {
            get {
                return _storageType;
            }
        }

        public override Storage/*!*/ MakeEnvironmentReference(SymbolId name, Type/*!*/ variableType) {
            Assert.NotNull(variableType);
            return new PropertyEnvironmentReference(_storageType, _index++, variableType);
        }

        protected int Index {
            get { return _index; }
            
        }

        public override void EmitStorage(ILGen cg) {
            cg.EmitNew(StorageType.GetConstructor(Type.EmptyTypes));

            if (Tuple.GetSize(StorageType) > Tuple.MaxSize) {
                cg.Emit(OpCodes.Dup);
                EmitNestedTupleInit(cg, StorageType);
            }
        }

        private static void EmitNestedTupleInit(ILGen cg, Type storageType) {
            Slot tmp = cg.GetLocalTmp(storageType);
            tmp.EmitSet(cg);

            Type[] nestedTuples = storageType.GetGenericArguments();
            for (int i = 0; i < nestedTuples.Length; i++) {
                Type t = nestedTuples[i];
                if (t.IsSubclassOf(typeof(Tuple))) {
                    tmp.EmitGet(cg);

                    cg.EmitNew(t.GetConstructor(Type.EmptyTypes));
                    cg.EmitPropertySet(storageType, String.Format("Item{0:D3}", i));

                    if (Tuple.GetSize(t) > Tuple.MaxSize) {
                        tmp.EmitGet(cg);
                        cg.EmitPropertyGet(storageType, String.Format("Item{0:D3}", i));

                        EmitNestedTupleInit(cg, t);
                    }
                }
            }

            cg.FreeLocalTmp(tmp);
        }

        public override void EmitGetStorageFromContext(LambdaCompiler cg) {
            cg.EmitCodeContext();
            cg.IL.EmitCall(typeof(RuntimeHelpers).GetMethod("GetScopeStorage").MakeGenericMethod(StorageType));
        }

        public override EnvironmentSlot CreateEnvironmentSlot(LambdaCompiler cg) {
            return new FunctionEnvironmentSlot(cg.GetNamedLocal(StorageType, "$environment"), StorageType);
        }

        [Conditional("DEBUG")]
        private static void ValidateTupleType(Type type) {
            Type curType = type.BaseType;
            while (curType != typeof(Tuple)) {
                Debug.Assert(curType != typeof(object));
                curType = curType.BaseType;
            }
        }

    }
}
