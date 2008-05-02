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

using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Generation {
    internal sealed class GlobalNamedStorage : Storage {
        private readonly SymbolId _name;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")] // TODO: fix
        private readonly Type _type;

        internal GlobalNamedStorage(SymbolId name, Type type) {
            _name = name;
            _type = type;
        }

        internal override bool RequireAccessSlot {
            get { return true; }
        }

        internal override Slot CreateSlot(Slot instance) {
            Debug.Assert(typeof(CodeContext).IsAssignableFrom(instance.Type), "wrong instance type");
            return new NamedFrameSlot(instance, _name);
        }
    }

    internal sealed class GlobalNamedAllocator : StorageAllocator {
        internal override Storage AllocateStorage(SymbolId name, Type type) {
            return new GlobalNamedStorage(name, type);
        }
    }

    internal sealed class GlobalFieldStorage : Storage {
        // Storing slot directly as there is no relocation involved
        private readonly Slot _slot;

        internal GlobalFieldStorage(Slot slot) {
            _slot = slot;
        }

        internal override bool RequireAccessSlot {
            get { return false; }
        }

        internal override Slot CreateSlot(Slot instance) {
            return _slot;
        }
    }

    internal sealed class GlobalFieldAllocator : StorageAllocator {
        private readonly SlotFactory _slotFactory;
        private readonly Dictionary<SymbolId, Slot> _fields = new Dictionary<SymbolId, Slot>();

        internal GlobalFieldAllocator(SlotFactory sfsf) {
            _slotFactory = sfsf;
        }

        internal SlotFactory SlotFactory {
            get { return _slotFactory; }
        }

        internal Dictionary<SymbolId, Slot> Fields {
            get {
                return _fields;
            }
        }

        internal override void PrepareForEmit(LambdaCompiler cg) {
            _slotFactory.PrepareForEmit(cg);
        }

        internal override Storage AllocateStorage(SymbolId name, Type type) {
            Slot slot;
            if (_fields.TryGetValue(name, out slot)) {
                // Throw invalid operation - duplicate name on global level
                throw new InvalidOperationException("Duplicate global name");
            }

            _fields[name] = slot = _slotFactory.CreateSlot(name, type);
            return new GlobalFieldStorage(slot);
        }
    }
}
