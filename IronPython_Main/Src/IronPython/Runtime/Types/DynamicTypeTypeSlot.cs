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
    class DynamicTypeTypeSlot : DynamicTypeSlot {
        public override bool TryGetValue(CodeContext context, object instance, DynamicMixin owner, out object value) {
            if (instance == null) {
                if (owner == TypeCache.None) {
                    value = owner;
                } else {
                    value = DynamicHelpers.GetDynamicType(owner);
                }
            } else {
                value = DynamicHelpers.GetDynamicType(instance);
            }

            return true;
        }

        public override bool TrySetValue(CodeContext context, object instance, DynamicMixin owner, object value) {
            if (instance == null) return false;

            ISuperDynamicObject sdo = instance as ISuperDynamicObject;
            if (sdo == null) return false;

            DynamicType dt = value as DynamicType;
            if (dt == null) throw PythonOps.TypeError("__class__ must be set to new-style class, not '{0}' object", DynamicHelpers.GetDynamicType(value).Name);

            if(dt.UnderlyingSystemType != DynamicHelpers.GetDynamicType(instance).UnderlyingSystemType)
                throw PythonOps.TypeErrorForIncompatibleObjectLayout("__class__ assignment", DynamicHelpers.GetDynamicType(instance), dt.UnderlyingSystemType);

            sdo.SetDynamicType(dt);
            return true;
        }

        public override bool IsSetDescriptor(CodeContext context, DynamicMixin owner) {
            return true;
        }

        public override bool TryDeleteValue(CodeContext context, object instance, DynamicMixin owner) {
            throw PythonOps.AttributeErrorForReadonlyAttribute(DynamicTypeOps.GetName(instance), Symbols.Class);
        }
    }
}
