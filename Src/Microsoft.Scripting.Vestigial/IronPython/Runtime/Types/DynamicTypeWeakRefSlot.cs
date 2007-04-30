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
using System.Collections.Generic;
using System.Text;

using Microsoft.Scripting;
using IronPython.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Types {
    [PythonType("getset_descriptor")]
    public sealed class DynamicTypeWeakRefSlot : DynamicTypeSlot, ICodeFormattable {
        DynamicType _type;

        public DynamicTypeWeakRefSlot(DynamicType parent) {
            this._type = parent;
        }

        public override bool TryGetValue(CodeContext context, object instance, DynamicMixin owner, out object value) {
            if (instance == null) {
                value = this;
                return true;
            }

            IWeakReferenceable reference = instance as IWeakReferenceable;
            if (reference != null) {
                WeakRefTracker tracker = reference.GetWeakRef();
                if (tracker == null || tracker.HandlerCount == 0) {
                    value = null;
                } else {
                    value = tracker.GetHandlerCallback(0);
                }
                return true;
            }

            value = null;
            return false;
        }

        public override bool TrySetValue(CodeContext context, object instance, DynamicMixin owner, object value) {
            IWeakReferenceable reference = instance as IWeakReferenceable;
            if (reference != null) {
                return reference.SetWeakRef(new WeakRefTracker(value, instance));
            }
            return false;
        }

        public override bool TryDeleteValue(CodeContext context, object instance, DynamicMixin owner) {
            throw Ops.TypeError("__weakref__ attribute cannot be deleted");
        }
       
        public override string ToString() {
            return String.Format("<attribute '__weakref__' of '{0}' objects>", _type.Name);
        }

        #region ICodeFormattable Members

        public string ToCodeString(CodeContext context) {
            return String.Format("<attribute '__weakref__' of {0} objects",
                Ops.StringRepr(_type));
        }

        #endregion
    }
}
