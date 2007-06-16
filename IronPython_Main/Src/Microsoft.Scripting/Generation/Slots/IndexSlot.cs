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
using System.Reflection.Emit;

namespace Microsoft.Scripting.Generation {
    /// <summary>
    /// Slot that indexes into an array
    /// </summary>
    public class IndexSlot : Slot {
        private Slot _instance;
        private int _index;
        private Type _type;

        public IndexSlot(Slot instance, int index)
            : this(instance, index, typeof(object)) {
        }

        public IndexSlot(Slot instance, int index, Type type) {
            this._instance = instance;
            this._index = index;
            this._type = type;
        }

        public int Index {
            get {
                return _index;
            }
        }

        public override void EmitGet(CodeGen cg) {
            if (cg == null) throw new ArgumentNullException("cg");

            _instance.EmitGet(cg);
            cg.EmitInt(_index);
            if (Type == typeof(object)) cg.Emit(OpCodes.Ldelem_Ref);
            else cg.Emit(OpCodes.Ldelem, Type);
        }

        public override void EmitSet(CodeGen cg) {
            if (cg == null) throw new ArgumentNullException("cg");

            Slot val = cg.GetLocalTmp(Type);
            val.EmitSet(cg);
            EmitSet(cg, val);
            cg.FreeLocalTmp(val);
        }

        public override void EmitSet(CodeGen cg, Slot val) {
            if (cg == null) throw new ArgumentNullException("cg");
            if (val == null) throw new ArgumentNullException("val");

            _instance.EmitGet(cg);
            cg.EmitInt(_index);
            val.EmitGet(cg);
            cg.EmitStoreElement(Type);
        }

        public override void EmitGetAddr(CodeGen cg) {
            if (cg == null) throw new ArgumentNullException("cg");

            _instance.EmitGet(cg);
            cg.EmitInt(_index);
            cg.Emit(OpCodes.Ldelema, Type);
        }

        public override Type Type {
            get {
                return _type;
            }
        }

        public override string ToString() {
            return String.Format("IndexSlot From: ({0}) Index: {1} Type: {2}", _instance, _index, _type);
        }
    }
}
