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
using System.Diagnostics;

namespace Microsoft.Scripting.Generation {
    /// <summary>
    /// A slot that can be used to wrap other slots and perform a cast before
    /// getting or setting the value.
    /// </summary>
    public class CastSlot : Slot {
        private Slot _instance;
        private Type _type;

        public CastSlot(Slot instance, Type type) {
            if (instance == null) throw new ArgumentNullException("instance");
            if (type == null) throw new ArgumentNullException("type");

            this._instance = instance;
            this._type = type;
        }

        public override void EmitGet(CodeGen cg) {
            if (cg == null) throw new ArgumentNullException("cg");

            _instance.EmitGet(cg);
            if (!_type.IsAssignableFrom(_instance.Type)) {
                if (_type.IsValueType) {
                    Debug.Assert(_instance.Type == typeof(object));
                    cg.Emit(OpCodes.Unbox_Any, _type);
                } else {
                    cg.Emit(OpCodes.Castclass, _type);
                }
            }
        }

        public override void EmitGetAddr(CodeGen cg) {
            throw new NotImplementedException(Resources.NotImplemented);
        }

        public override void EmitSet(CodeGen cg) {
            if (cg == null) throw new ArgumentNullException("cg");

            if (_instance.Type.IsAssignableFrom(_type)) {
                if (_type.IsValueType) {
                    Debug.Assert(_instance.Type == typeof(object));
                    cg.Emit(OpCodes.Box, _type);
                } else {
                    cg.Emit(OpCodes.Castclass, _instance.Type);
                }
            }
            _instance.EmitSet(cg);
        }

        public override void EmitSetUninitialized(CodeGen cg) {
            // Cannot initialize non-object to "Uninitialized"
            if (_type == typeof(object)) {
                base.EmitSetUninitialized(cg);
            }
        }

        public override Type Type {
            get {
                return _type;
            }
        }

        public override string ToString() {
            return String.Format("CastSlot From: ({0}) To: {1}", _instance, _type);
        }
    }
}
