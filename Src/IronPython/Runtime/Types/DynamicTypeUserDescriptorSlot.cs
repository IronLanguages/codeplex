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
using IronPython.Runtime.Operations;    

namespace IronPython.Runtime.Types {
    sealed class DynamicTypeUserDescriptorSlot : DynamicTypeSlot {
        private object _value;

        public DynamicTypeUserDescriptorSlot(object value) {
            _value = value;
        }

        public override bool TryGetValue(CodeContext context, object instance, DynamicMixin owner, out object value) {
            value = PythonOps.GetUserDescriptor(Value, instance, owner);
            return true;
        }

        public override bool TryDeleteValue(CodeContext context, object instance, DynamicMixin owner) {
            return PythonOps.TryDeleteUserDescriptor(Value, instance);
        }

        public override bool TrySetValue(CodeContext context, object instance, DynamicMixin owner, object value) {
            return PythonOps.TrySetUserDescriptor(Value, instance, value);
        }

        public override bool IsSetDescriptor(CodeContext context, DynamicMixin owner) {
            object dummy;
            return PythonOps.TryGetBoundAttr(context, Value, Symbols.SetDescriptor, out dummy);
        }
        public object Value {
            get {
                return _value;
            }
        }

    }
}