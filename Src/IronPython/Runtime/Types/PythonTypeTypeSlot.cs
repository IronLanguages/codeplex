/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
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
    class PythonTypeTypeSlot : PythonTypeSlot {
        internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value) {
            if (instance == null) {
                if (owner == TypeCache.None) {
                    value = owner;
                } else {
                    value = DynamicHelpers.GetPythonType(owner);
                }
            } else {
                value = DynamicHelpers.GetPythonType(instance);
            }

            return true;
        }

        internal override bool TrySetValue(CodeContext context, object instance, PythonType owner, object value) {
            if (instance == null) return false;

            IPythonObject sdo = instance as IPythonObject;
            if (sdo == null) return false;

            PythonType dt = value as PythonType;
            if (dt == null) throw PythonOps.TypeError("__class__ must be set to new-style class, not '{0}' object", DynamicHelpers.GetPythonType(value).Name);

            if(dt.UnderlyingSystemType != DynamicHelpers.GetPythonType(instance).UnderlyingSystemType)
                throw PythonOps.TypeErrorForIncompatibleObjectLayout("__class__ assignment", DynamicHelpers.GetPythonType(instance), dt.UnderlyingSystemType);

            sdo.SetPythonType(dt);
            return true;
        }

        internal override bool IsSetDescriptor(CodeContext context, PythonType owner) {
            return true;
        }

        internal override bool TryDeleteValue(CodeContext context, object instance, PythonType owner) {
            throw PythonOps.AttributeErrorForReadonlyAttribute(PythonTypeOps.GetName(instance), Symbols.Class);
        }
    }
}
