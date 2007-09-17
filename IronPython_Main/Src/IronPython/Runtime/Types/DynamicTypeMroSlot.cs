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
using Microsoft.Scripting.Types;

using IronPython.Runtime.Calls;

namespace IronPython.Runtime.Types {

    class DynamicTypeMroSlot : DynamicTypeSlot {
        public override bool TryGetValue(CodeContext context, object instance, DynamicMixin owner, out object value) {
            if (instance != null) value = ToPython(((DynamicType)instance).ResolutionOrder);
            else value = ToPython(owner.ResolutionOrder);

            return true;
        }

        private static PythonTuple ToPython(IList<DynamicMixin> types) {
            List<object>res = new List<object>(types.Count);
            foreach (DynamicType dt in types) {
                if (dt.UnderlyingSystemType == typeof(ValueType)) continue; // hide value type

                DynamicTypeSlot dts;
                object val;
                if (dt != TypeCache.Object && dt.TryLookupSlot(DefaultContext.Default, Symbols.Class, out dts) &&
                    dts.TryGetValue(DefaultContext.Default, null, dt, out val)) {
                    res.Add(val);
                } else {
                    res.Add(dt);
                }
            }
            return PythonTuple.Make(res);
        }
    }
}
