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
using Microsoft.Scripting.Internal;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

[assembly: PythonExtensionType(typeof(TypeCollision), typeof(TypeCollisionOps))]
namespace IronPython.Runtime.Operations {
    public static class TypeCollisionOps {
        [OperatorMethod, PythonName("__repr__")]
        public static string ToCodeRepresentation(TypeCollision self) {
            StringBuilder sb = new StringBuilder("<types ");
            sb.Append(Ops.StringRepr(DynamicTypeOps.GetName(Ops.GetDynamicTypeFromType(self.DefaultType))));
            for (int i = 0; i < self.OtherTypes.Count; i++) {
                sb.Append(", ");
                sb.Append(Ops.StringRepr(DynamicTypeOps.GetName(self.OtherTypes[i])));
            }
            sb.Append(">");

            return sb.ToString();
        }

    }
}
