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
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Diagnostics;
using IronPython.Runtime;

namespace IronPython.Compiler {
    public abstract class SlotFactory {
        public Slot MakeSlot(SymbolId name) {
            return MakeSlot(name, typeof(object));
        }
        public abstract Slot MakeSlot(SymbolId name, Type type);
    }

    public class LocalSlotFactory : SlotFactory {
        private CodeGen codeGen;

        public LocalSlotFactory(CodeGen codeGen) {
            this.codeGen = codeGen;
        }

        public override Slot MakeSlot(SymbolId name, Type type) {
            LocalBuilder b = codeGen.DeclareLocal(type);
            if (codeGen.EmitDebugInfo) b.SetLocalSymInfo(name.GetString());
            return new LocalSlot(b, codeGen);
        }
    }

    public class FieldSlotFactory : SlotFactory {
        private TypeGen typeGen;
        private Slot instance;

        public FieldSlotFactory(TypeGen typeGen, Slot instance) {
            this.typeGen = typeGen;
            this.instance = instance;
        }
        public override Slot MakeSlot(SymbolId name, Type type) {
            FieldBuilder fb = typeGen.myType.DefineField(name.GetString(), type, FieldAttributes.Public);
            return new FieldSlot(instance, fb);
        }
    }

    public class StaticFieldSlotFactory : SlotFactory {
        Dictionary<SymbolId, StaticFieldSlot> fields = new Dictionary<SymbolId,StaticFieldSlot>();
        private TypeGen typeGen;

        public StaticFieldSlotFactory(TypeGen typeGen) {
            this.typeGen = typeGen;
        }

        /// <summary>
        /// Makes a new static field slot.  Type is ignored.  If we ever change ignoring type then we
        /// need to make BuiltinWrapper generic and update StaticFieldBuiltinSlot to get the correct
        /// type.
        /// </summary>
        public override Slot MakeSlot(SymbolId name, Type type) {
            StaticFieldSlot fs;
            object builtin;
            if (!fields.TryGetValue(name, out fs)) {
                if (!TypeCache.Builtin.TryGetSlot(DefaultContext.Default, SymbolTable.StringToId(name.GetString()), out builtin)) {
                    // name does not collide w/ a built-in name, define a real field.

                    FieldBuilder fb = typeGen.myType.DefineField(name.GetString(), typeof(object), FieldAttributes.Public | FieldAttributes.Static);
                    fs = new StaticFieldSlot(fb);
                } else {
                    // name collides w/ a built-in name.  Our field becomes strongly typed to
                    // BuiltinWrapper.  We then return a 
                    // StaticFieldBuiltinSlot which checks the value to see if it's a built-in or
                    // not.

                    FieldBuilder fb = typeGen.myType.DefineField(name.GetString(), typeof(BuiltinWrapper), FieldAttributes.Public | FieldAttributes.Static);
                    fs = new StaticFieldBuiltinSlot(fb);                   
                }
                fields[name] = fs;
            }
            return fs;
        }
    }

    public class LocalFrameSlotFactory : SlotFactory {
        protected Slot frame;

        public LocalFrameSlotFactory(Slot frame) {
            this.frame = frame;
        }

        public override Slot MakeSlot(SymbolId name, Type type) {
            return new LocalNamedFrameSlot(frame, name);
        }
    }

    public class IndexSlotFactory : SlotFactory {
        private Slot instance;
        private int index;

        public int Index {
            get {
                return index;
            }
        }

        public IndexSlotFactory(Slot instance) {
            this.instance = instance;
        }

        public override Slot MakeSlot(SymbolId name, Type type) {
            return new IndexSlot(instance, index++, type);
        }
    }
}
