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

namespace Microsoft.Scripting.Internal.Generation {
    /// <summary>
    /// This slot holds onto nothing and getting its value is a nop and setting
    /// its value is an error.  The purpose of this slot is to simplify some code
    /// gen paths by allowing void to be more easily treated like other types.
    /// </summary>
    public class VoidSlot : Slot {
        public VoidSlot() { }

        public override void EmitGet(CodeGen cg) {
            return;
        }

        public override void EmitGetAddr(CodeGen cg) {
            throw new NotImplementedException(Resources.NotImplemented);
        }

        public override void EmitSet(CodeGen cg) {
            throw new NotImplementedException(Resources.NotImplemented);
        }

        public override Type Type {
            get {
                return typeof(void);
            }
        }

        public override string ToString() {
            return "VoidSlot";
        }
    }
}
