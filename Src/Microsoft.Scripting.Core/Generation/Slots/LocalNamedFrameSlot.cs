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
using System.Reflection.Emit;
using Microsoft.Scripting.Ast;
using Microsoft.Contracts;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Generation {
    class LocalNamedFrameSlot : Slot {
        private readonly Slot _frame;
        private readonly SymbolId _name;

        public LocalNamedFrameSlot(Slot frame, SymbolId name) {
            this._frame = frame;
            this._name = name;
        }

        public override void EmitGet(ILGen cg) {
            // RuntimeHelpers.LookupName(context, name)
            _frame.EmitGet(cg);
            cg.EmitSymbolId(_name);
            cg.EmitCall(typeof(RuntimeHelpers), "LookupName");
        }

        public override void EmitGetAddr(ILGen cg) {
            //???how bad is it that we can't do this???
            throw new NotImplementedException("address of local frame slot");
        }

        public override void EmitSet(ILGen cg, Slot val) {
            // Emit the following:
            //    RuntimeHelpers.SetName(codeContext, name, value)
            _frame.EmitGet(cg);
            cg.EmitSymbolId(_name);
            val.EmitGet(cg);
            cg.EmitCall(typeof(RuntimeHelpers), "SetName");
        }

        public override void EmitSetUninitialized(ILGen cg) {
        }

        public override void EmitDelete(ILGen cg, SymbolId name) {
            // Emit the following:
            //    RuntimeHelpers.RemoveName(context, symbol_id)
            _frame.EmitGet(cg);
            cg.EmitSymbolId(name);
            cg.EmitCall(typeof(RuntimeHelpers), "RemoveName");
            cg.Emit(OpCodes.Pop);
        }

        public override Type Type {
            get {
                return typeof(object);
            }
        }

        public Slot Frame {
            get { return _frame; }
        }

        public SymbolId Name {
            get { return _name; }
        }

        [Confined]
        public override string/*!*/ ToString() {
            return String.Format("LocalNamedFromSlot Name: ({0}) From: {1}", SymbolTable.IdToString(_name), _frame);
        }
    }
}
