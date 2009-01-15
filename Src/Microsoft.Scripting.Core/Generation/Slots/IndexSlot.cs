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
using System.Reflection.Emit;

using Microsoft.Scripting.Utils;
using System.Diagnostics.Contracts;
using Microsoft.Scripting.Ast;

namespace Microsoft.Scripting.Generation {
    /// <summary>
    /// Slot that indexes into an array
    /// </summary>
    class IndexSlot : Slot {
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

        public override void EmitGet(ILGen cg) {
            CodeContract.RequiresNotNull(cg, "cg");

            _instance.EmitGet(cg);
            cg.EmitInt(_index);
            if (Type == typeof(object)) cg.Emit(OpCodes.Ldelem_Ref);
            else cg.Emit(OpCodes.Ldelem, Type);
        }

        public override void EmitSet(ILGen cg) {
            CodeContract.RequiresNotNull(cg, "cg");

            Slot val = cg.GetLocalTmp(Type);
            val.EmitSet(cg);
            EmitSet(cg, val);
            cg.FreeLocalTmp(val);
        }

        public override void EmitSet(ILGen cg, Slot val) {
            CodeContract.RequiresNotNull(cg, "cg");
            CodeContract.RequiresNotNull(val, "val");

            _instance.EmitGet(cg);
            cg.EmitInt(_index);
            val.EmitGet(cg);
            cg.EmitStoreElement(Type);
        }

        public override void EmitGetAddr(ILGen cg) {
            CodeContract.RequiresNotNull(cg, "cg");

            _instance.EmitGet(cg);
            cg.EmitInt(_index);
            cg.Emit(OpCodes.Ldelema, Type);
        }

        public override Type Type {
            get {
                return _type;
            }
        }

        [Confined]
        public override string/*!*/ ToString() {
            return String.Format("IndexSlot From: ({0}) Index: {1} Type: {2}", _instance, _index, _type);
        }
    }
}
