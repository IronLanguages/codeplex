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
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Ast;

namespace Microsoft.Scripting.Generation {
    /// <summary>
    /// Environment factory which constructs storage into a tuple-backed lambda environment.
    /// </summary>
    internal class EnvironmentAllocator : StorageAllocator {
        private class EnvironmentStorage : Storage {
            private readonly IEnumerable<PropertyInfo> _accessPath;

            internal EnvironmentStorage(IEnumerable<PropertyInfo> accessPath) {
                _accessPath = accessPath;
            }

            internal override Slot CreateSlot(Slot instance) {
                Debug.Assert(instance != null);

                Slot slot = instance;
                foreach (PropertyInfo pi in _accessPath) {
                    Debug.Assert(pi.DeclaringType.IsAssignableFrom(slot.Type));

                    slot = new PropertySlot(slot, pi);
                }
                return slot;
            }
        }

        private readonly Type/*!*/ _storageType;
        private int _localIndex, _tempIndex;

        /// <summary>
        /// Creates a new EnvironmentAllocator backed by the specified type of tuple
        /// </summary>
        internal EnvironmentAllocator(Type/*!*/ storageType, int localVarCount) {
            Assert.NotNull(storageType);
            ValidateTupleType(storageType);

            _storageType = storageType;

            // Temporaries go after locals/params in the tuple
            _tempIndex = localVarCount;
        }

        internal override Storage/*!*/ AllocateStorage(Expression variable) {
            Assert.NotNull(variable);

            int index;
            if (variable.NodeType == AstNodeType.TemporaryVariable) {
                index = _tempIndex++;
            } else {
                index = _localIndex++;
            }

            IEnumerable<PropertyInfo> accessPath = Tuple.GetAccessPath(_storageType, index);

            ValidateElementType(accessPath, variable.Type);

            return new EnvironmentStorage(accessPath);
        }

        internal Type StorageType {
            get { return _storageType; }
        }

        internal void EmitStorage(ILGen cg) {
            cg.EmitNew(_storageType.GetConstructor(Type.EmptyTypes));

            if (Tuple.GetSize(_storageType) > Tuple.MaxSize) {
                cg.Emit(OpCodes.Dup);
                EmitNestedTupleInit(cg, _storageType);
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

        [Conditional("DEBUG")]
        private static void ValidateTupleType(Type type) {
            Type curType = type.BaseType;
            while (curType != typeof(Tuple)) {
                Debug.Assert(curType != typeof(object));
                curType = curType.BaseType;
            }
        }

        [Conditional("DEBUG")]
        private static void ValidateElementType(IEnumerable<PropertyInfo> accessPath, Type elementType) {
            Type last = null;
            foreach (PropertyInfo pi in accessPath) {
                last = pi.PropertyType;
            }
            Debug.Assert(last == elementType);
        }

    }
}
