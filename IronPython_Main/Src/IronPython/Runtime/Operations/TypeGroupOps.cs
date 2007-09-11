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
using Microsoft.Scripting.Actions;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

[assembly: PythonExtensionType(typeof(TypeGroup), typeof(TypeGroupOps))]
namespace IronPython.Runtime.Operations {
    public static class TypeGroupOps {
        [SpecialName, PythonName("__repr__")]
        public static string ToCodeRepresentation(TypeGroup self) {
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

        /// <summary>
        /// Indexer for generic parameter resolution.  We bind to one of the generic versions
        /// available in this type collision.  A user can also do someType[()] to force to
        /// bind to the non-generic version, but we will always present the non-generic version
        /// when no bindings are available.
        /// </summary>
        [SpecialName]
        public static DynamicType GetItem(TypeGroup self, params DynamicType[] types) {
            TypeTracker genType = self.GetTypeForArity(types.Length);
            if (genType == null) {
                throw new ArgumentException(String.Format("could not find compatible generic type for {0} type arguments", types.Length));
            }

            Type res = genType.Type;
            if (types.Length != 0) {
                res = res.MakeGenericType(DynamicTypeOps.ConvertToTypes(types));
            }

            return DynamicHelpers.GetDynamicTypeFromType(res);
        }
    }
}
