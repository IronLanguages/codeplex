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

using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Generation {
    internal sealed class GlobalNamedAllocator : StorageAllocator {
        private sealed class GlobalNamedStorage : Storage {
            private readonly SymbolId _name;

            internal GlobalNamedStorage(SymbolId name) {
                _name = name;
            }

            internal override Slot CreateSlot(Slot instance) {
                Debug.Assert(instance != null && typeof(CodeContext).IsAssignableFrom(instance.Type));

                return new NamedFrameSlot(instance, _name);
            }
        }

        internal override Storage AllocateStorage(Expression variable) {
            return new GlobalNamedStorage(VariableInfo.GetName(variable));
        }
    }

    internal abstract class GlobalFieldAllocator : StorageAllocator {
        private readonly Dictionary<SymbolId, Slot> _fields = new Dictionary<SymbolId, Slot>();

        internal GlobalFieldAllocator() {
        }

        /// <summary>
        /// Overriden by the base type.  Creates a new slot of the given name and type.  Only called once for each name.
        /// </summary>
        internal abstract Slot CreateSlot(SymbolId name, Type type);

        internal Dictionary<SymbolId, Slot> Fields {
            get { return _fields; }
        }

        internal override Storage AllocateStorage(Expression variable) {
            SymbolId name = VariableInfo.GetName(variable);

            Slot slot;
            if (_fields.TryGetValue(name, out slot)) {
                // Throw invalid operation - duplicate name on global level
                throw new InvalidOperationException("Duplicate global name");
            }

            _fields[name] = slot = new ModuleGlobalSlot(variable.Type, CreateSlot(name, typeof(ModuleGlobalWrapper)));
            return new SlotStorage(slot);
        }
    }
}
