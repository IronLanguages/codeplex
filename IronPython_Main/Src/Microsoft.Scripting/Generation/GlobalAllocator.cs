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
using System.Diagnostics;

namespace Microsoft.Scripting.Internal.Generation {
    sealed class GlobalNamedStorage : Storage {
        private readonly SymbolId _name;
        private readonly Type _type;

        internal GlobalNamedStorage(SymbolId name, Type type) {
            _name = name;
            _type = type;
        }

        public override bool RequireAccessSlot {
            get { return true; }
        }

        public override Slot CreateSlot(Slot instance) {
            Debug.Assert(typeof(CodeContext).IsAssignableFrom(instance.Type), "wrong instance type");
            return new NamedFrameSlot(instance, _name);
        }
    }

    sealed class GlobalNamedAllocator : StorageAllocator {
        public override Storage AllocateStorage(SymbolId name, Type type) {
            return new GlobalNamedStorage(name, type);
        }
    }

    sealed class GlobalFieldStorage : Storage {
        // Storing slot directly as there is no relocation involved
        private readonly Slot _slot;

        internal GlobalFieldStorage(Slot slot) {
            _slot = slot;
        }

        public override bool RequireAccessSlot {
            get { return false; }
        }

        public override Slot CreateSlot(Slot instance) {
            return _slot;
        }
    }

    sealed class GlobalFieldAllocator : StorageAllocator {
        private readonly SlotFactory _slotFactory;

        public GlobalFieldAllocator(SlotFactory sfsf) {
            _slotFactory = sfsf;
        }

        public SlotFactory SlotFactory {
            get { return _slotFactory; }
        }

        public override void PrepareForEmit(CodeGen cg) {
            _slotFactory.PrepareForEmit(cg);
        }

        public override Storage AllocateStorage(SymbolId name, Type type) {
            return new GlobalFieldStorage(_slotFactory.MakeSlot(name, type));
        }
    }
}
