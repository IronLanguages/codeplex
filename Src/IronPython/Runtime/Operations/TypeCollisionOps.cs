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
using System.Runtime.CompilerServices;

using Microsoft.Scripting;
using Microsoft.Scripting.Types;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

[assembly: PythonExtensionType(typeof(TypeCollision), typeof(TypeCollisionOps))]
namespace IronPython.Runtime.Operations {
    public static class TypeCollisionOps {
        [SpecialName, PythonName("__repr__")]
        public static string ToCodeRepresentation(TypeCollision self) {
            StringBuilder sb = new StringBuilder("<types ");
            bool pastFirstType = false;
            foreach(Type type in self.Types) {
                if (pastFirstType) { 
                    sb.Append(", ");
                }
                DynamicType dt = DynamicHelpers.GetDynamicTypeFromType(type);
                sb.Append(PythonOps.StringRepr(DynamicTypeOps.GetName(dt)));
                pastFirstType = true;
            }
            sb.Append(">");

            return sb.ToString();
        }

    }
}
