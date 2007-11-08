/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
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
using IronPython.Compiler;

namespace IronPython.Runtime.Types {
    class PythonTypeNameSlot : PythonTypeSlot {
        internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value) {
            PythonType dt;
            if (instance == null) {
                if (owner == TypeCache.None) {
                    dt = TypeCache.None;
                } else {
                    dt = DynamicHelpers.GetPythonType(owner);
                }
            } else {
                dt = ((PythonType)instance);
            }

            value = PythonTypeOps.GetName(dt);            
            return true;
        }

        internal override bool TrySetValue(CodeContext context, object instance, PythonType owner, object value) {
            return false;
        }

        internal override bool TryDeleteValue(CodeContext context, object instance, PythonType owner) {
            return false;
        }
    }
}
