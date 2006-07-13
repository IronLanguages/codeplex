/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Shared Source License
 * for IronPython. A copy of the license can be found in the License.html file
 * at the root of this distribution. If you can not locate the Shared Source License
 * for IronPython, please send an email to ironpy@microsoft.com.
 * By using this source code in any fashion, you are agreeing to be bound by
 * the terms of the Shared Source License for IronPython.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

using IronPython.Runtime;

namespace IronPython.Compiler.Generation {
    // Namespaces can be nested to get lexical scoping. Python looks up name as follows.
    //
    // Assignments to a name :
    //   For assignments, the name is assumed to be a local unless explicitly declared as global using
    //   the "global var" statement.
    //   GetSlotForSet() is the API which implements this.
    //
    // References to a name
    //   For names referenced in an expression or statement, Python uses the LEGB lookup rule, where the
    //   order of lookup is:
    //   1. Locals of the function
    //   2. Enclosing local function scopes for nested functions and lambdas
    //   3. Globals declared by the module
    //   4. Built-in module which is always available
    //   GetOrMakeSlotForGet() is the API which implements this
    //
    // Note that for module-level code, globals and locals are the same.

    class Namespace {
        private Dictionary<SymbolId, Slot> slots = new Dictionary<SymbolId, Slot>();

        private SlotFactory locals;
        private GlobalNamespace globals;

        // temporary slots
        private List<Slot> temps = new List<Slot>();

        public Namespace(SlotFactory factory) {
            this.locals = factory;
        }

        internal GlobalNamespace Globals {
            get {
                Debug.Assert(globals != null, "No globals");
                return globals;
            }
            set { globals = value; }
        }

        public Slot this[SymbolId name] {
            get {
                Debug.Assert(slots.ContainsKey(name), "undefined slot: " + name.GetString());
                return slots[name];
            }
            set {
                Debug.Assert(!slots.ContainsKey(name), "slot override: " + name.GetString());
                slots[name] = value;
            }
        }

        public Dictionary<SymbolId, Slot> Slots {
            get {
                return slots;
            }
        }

        internal Slot CreateGlobalSlot(SymbolId name) {
            Slot slot = Globals.GetOrMakeSlot(name);
            slots[name] = slot;
            return slot;
        }

        internal Slot EnsureLocalSlot(SymbolId name) {
            Slot slot;
            if (!slots.TryGetValue(name, out slot)) {
                slot = locals.MakeSlot(name);
                slots[name] = slot;
            }
            return slot;
        }

        public void AddTempSlot(Slot slot) {
            temps.Add(slot);
        }

        public void SetSlot(SymbolId name, Slot slot) {
            slots[name] = slot;
        }

        private int tempCounter = 0;
        public Slot GetTempSlot(string prefix, Type type) {
            if (locals != null) {
                return locals.MakeSlot(SymbolTable.StringToId(prefix + "$" + tempCounter++), type);
            } else {
                Debug.Assert(tempCounter < temps.Count);
                return new CastSlot(temps[tempCounter++], type);
            }
        }
    }
}
