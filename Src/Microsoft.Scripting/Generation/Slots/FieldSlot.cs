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
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;

namespace Microsoft.Scripting.Generation {
    /// <summary>
    /// FieldSlot is an access of an attribute of an object 
    /// </summary>
    public class FieldSlot : Slot {
        private readonly Slot _instance;
        private readonly FieldInfo _field;

        public FieldSlot(Slot instance, FieldInfo field) {
            if (instance == null) throw new ArgumentNullException("instance");
            if (field == null) throw new ArgumentNullException("field");

            this._instance = instance;
            this._field = field;
        }
        public override void EmitGet(CodeGen cg) {
            if (cg == null) throw new ArgumentNullException("cg");

            _instance.EmitGet(cg);
            cg.Emit(OpCodes.Ldfld, _field);
        }
        public override void EmitGetAddr(CodeGen cg) {
            if (cg == null) throw new ArgumentNullException("cg");

            _instance.EmitGet(cg);
            cg.Emit(OpCodes.Ldflda, _field);
        }

        public override void EmitSet(CodeGen cg, Slot val) {
            if (cg == null) throw new ArgumentNullException("cg");
            if (val == null) throw new ArgumentNullException("val");

            _instance.EmitGet(cg);
            val.EmitGet(cg);
            cg.Emit(OpCodes.Stfld, _field);
        }

        public override void EmitSet(CodeGen cg) {
            if (cg == null) throw new ArgumentNullException("cg");

            Slot val = cg.GetLocalTmp(_field.FieldType);
            val.EmitSet(cg);
            EmitSet(cg, val);
            cg.FreeLocalTmp(val);
        }

        public override Type Type {
            get {
                return _field.FieldType;
            }
        }

        /// <summary>
        /// Gets the slot that is used for the instance of the field
        /// </summary>
        public Slot Instance {
            get { return _instance; }
        }

        /// <summary>
        /// Gets the FieldInfo for which this slot loads its value
        /// </summary>
        public FieldInfo Field {
            get { return _field; }
        }

        public override string ToString() {
            return String.Format("FieldSlot From: ({0}) On {1} Field {2}", _instance, _field.DeclaringType, _field.Name);
        }
    }
}
