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
using Microsoft.Scripting.Types;

using IronPython.Runtime.Operations;
using IronPython.Compiler;

namespace IronPython.Runtime.Types {
    class DynamicTypeNameSlot : DynamicTypeSlot {
        public override bool TryGetValue(CodeContext context, object instance, DynamicMixin owner, out object value) {
            DynamicType dt;
            if (instance == null) {
                if (owner == TypeCache.None) {
                    dt = TypeCache.None;
                } else {
                    dt = DynamicHelpers.GetDynamicType(owner);
                }
            } else {
                dt = ((DynamicType)instance);
            }

            value = DynamicTypeOps.GetName(dt);            
            return true;
        }

        public override bool TrySetValue(CodeContext context, object instance, DynamicMixin owner, object value) {
            return false;
        }

        public override bool TryDeleteValue(CodeContext context, object instance, DynamicMixin owner) {
            return false;
        }
    }
}
