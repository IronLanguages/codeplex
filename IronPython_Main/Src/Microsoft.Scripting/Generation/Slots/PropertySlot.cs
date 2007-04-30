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
using System.Diagnostics;

namespace Microsoft.Scripting.Internal.Generation {
    /// <summary>
    /// Slot that accesses a property off an object
    /// </summary>
    public class PropertySlot : Slot {
        private Slot _instance;
        private PropertyInfo _property;

        public PropertySlot(Slot instance, PropertyInfo property) {
            Debug.Assert(property != null);

            this._instance = instance;
            this._property = property;
        }

        public override void EmitSet(CodeGen cg, Slot val) {
            if (cg == null) throw new ArgumentNullException("cg");
            if (val == null) throw new ArgumentNullException("val");

            MethodInfo method = _property.GetSetMethod();
            Debug.Assert(method != null, "Cannot set property");
            Debug.Assert(method.GetParameters().Length == 1, "Wrong number of parameters on the property setter");

            //  Emit instance
            if (!method.IsStatic) {
                Debug.Assert(_instance != null, "need instance slot for instance property");
                _instance.EmitGet(cg);
            }

            //  Emit value
            val.EmitGet(cg);

            //  Emit call
            cg.EmitCall(method);
        }

        public override void EmitGet(CodeGen cg) {
            if (cg == null) throw new ArgumentNullException("cg");

            MethodInfo method = _property.GetGetMethod();
            Debug.Assert(method != null, "Cannot set property");
            Debug.Assert(method.GetParameters().Length == 0, "Wrong number of parameters on the property getter");

            // Emit instance
            if (!method.IsStatic) {
                Debug.Assert(_instance != null, "need instance slot for instance property");
                _instance.EmitGet(cg);
            }

            // Emit call
            cg.EmitCall(method);
        }

        public override void EmitGetAddr(CodeGen cg) {
            if (cg == null) throw new ArgumentNullException("cg");

            throw new NotImplementedException(Resources.NotImplemented);
        }

        public override Type Type {
            get {
                return _property.PropertyType;
            }
        }

        public override string ToString() {
            return String.Format("PropertySlot From: ({0}) On: {1} Property: {2}", _instance, _property.DeclaringType, _property.Name);
        }
    }
}
