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
    public class DynamicLookupSlot : Slot {
        private readonly SymbolId _name;

        public DynamicLookupSlot(SymbolId name) {
            _name = name;
        }

        public override Type Type {
            get {
                return typeof(object);
            }
        }

        public SymbolId Name {
            get { return _name; }
        }

        public override void EmitGet(CodeGen cg) {
            // RuntimeHelpers.LookupName(CodeContext, name)
            cg.EmitCodeContext();
            cg.EmitSymbolId(_name);
            cg.EmitCall(typeof(RuntimeHelpers), "LookupName");
        }

        public override void EmitGetAddr(CodeGen cg) {
            throw new NotImplementedException("Address of the dynamic lookup slot.");
        }

        public override void EmitSet(CodeGen cg, Slot val) {
            // RuntimeHelpers.SetName(CodeContext, name, value)
            cg.EmitCodeContext();
            cg.EmitSymbolId(_name);
            val.EmitGet(cg);
            cg.EmitCall(typeof(RuntimeHelpers), "SetName");
        }

        public override void EmitSetUninitialized(CodeGen cg) {
        }

        public override void EmitDelete(CodeGen cg, SymbolId name, bool check) {
            // RuntimeHelpers.RemoveName(CodeContext, name, )
            Debug.Assert(_name == name);
            cg.EmitCodeContext();
            cg.EmitSymbolId(name);
            cg.EmitCall(typeof(RuntimeHelpers), "RemoveName");
        }

        public override string ToString() {
            return String.Format("DynamicLookupSlot Name: ({0})", SymbolTable.IdToString(_name));
        }
    }
}
