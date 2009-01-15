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
using System.Diagnostics;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Generation {
    internal class FrameStorageAllocator : StorageAllocator {
        private class FrameStorage : Storage {
            private readonly SymbolId _name;
            private readonly Type _type;

            internal FrameStorage(SymbolId name, Type type) {
                _name = name;
                _type = type;
            }

            internal override Slot CreateSlot(Slot instance) {
                Debug.Assert(instance != null && typeof(CodeContext).IsAssignableFrom(instance.Type));

                Slot slot = new LocalNamedFrameSlot(instance, _name);
                if (_type != slot.Type) {
                    slot = new CastSlot(slot, _type);
                }
                return slot;
            }
        }
        internal override Storage AllocateStorage(Expression variable) {
            return new FrameStorage(VariableInfo.GetName(variable), variable.Type);
        }
    }
}
