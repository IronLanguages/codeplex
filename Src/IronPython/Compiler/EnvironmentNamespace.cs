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
using System.Reflection;
using System.Diagnostics;
using IronPython.Runtime;

namespace IronPython.Compiler {
    class EnvironmentNamespace {
        private EnvironmentFactory factory;
        private Dictionary<SymbolId, EnvironmentReference> references = new Dictionary<SymbolId, EnvironmentReference>();

        public EnvironmentNamespace(EnvironmentFactory factory) {
            this.factory = factory;
        }

        public EnvironmentReference GetReference(SymbolId name) {
            Debug.Assert(references.ContainsKey(name), "missing environment reference", name.GetString());
            return references[name];
        }

        public EnvironmentReference GetOrMakeReference(SymbolId name) {
            return GetOrMakeReference(name, typeof(object));
        }

        public EnvironmentReference GetOrMakeReference(SymbolId name, Type type) {
            EnvironmentReference er;
            if (!references.TryGetValue(name, out er)) {
                er = factory.MakeEnvironmentReference(name, type);
                references[name] = er;
            } else {
                Debug.Assert(er.ReferenceType.IsAssignableFrom(type));
            }
            return er;
        }
    }

    abstract class GlobalNamespace {
        public Slot GetOrMakeSlot(SymbolId name) {
            return GetOrMakeSlot(name, typeof(object));
        }
        public abstract Slot GetSlot(SymbolId name);
        public abstract Slot GetOrMakeSlot(SymbolId name, Type type);
        public abstract GlobalNamespace Relocate(Slot instance);
    }

    sealed class GlobalEnvironmentNamespace : GlobalNamespace {
        private EnvironmentNamespace en;
        private Slot instance;

        public GlobalEnvironmentNamespace(EnvironmentNamespace en, Slot instance) {
            this.en = en;
            this.instance = instance;
        }

        public override Slot GetSlot(SymbolId name) {
            EnvironmentReference es = en.GetReference(name);
            return es.CreateSlot(instance);
        }

        public override Slot GetOrMakeSlot(SymbolId name, Type type) {
            EnvironmentReference es = en.GetOrMakeReference(name, type);
            return es.CreateSlot(instance);
        }

        public override GlobalNamespace Relocate(Slot instance) {
            Debug.Assert(instance != null, "relocating environment namespace with null instance");
            Debug.Assert(typeof(IFrameEnvironment).IsAssignableFrom(instance.Type), "wrong instance type");
            return new GlobalEnvironmentNamespace(en, instance);
        }
    }

    sealed class GlobalFieldNamespace : GlobalNamespace {
        private StaticFieldSlotFactory sfsf;
        private Dictionary<SymbolId, Slot> slots = new Dictionary<SymbolId, Slot>();

        public GlobalFieldNamespace(StaticFieldSlotFactory sfsf) {
            this.sfsf = sfsf;
        }

        public override Slot GetSlot(SymbolId name) {
            Debug.Assert(slots.ContainsKey(name), "missing global field slot", name.GetString());
            return slots[name];
        }

        public override Slot GetOrMakeSlot(SymbolId name, Type type) {
            Slot slot;
            if (!slots.TryGetValue(name, out slot)) {
                slot = sfsf.MakeSlot(name, type);
                slots[name] = slot;
            } else {
                Debug.Assert(slot.Type.IsAssignableFrom(type));
            }
            return slot;
        }

        public override GlobalNamespace Relocate(Slot instance) {
            return this;
        }
    }
}
