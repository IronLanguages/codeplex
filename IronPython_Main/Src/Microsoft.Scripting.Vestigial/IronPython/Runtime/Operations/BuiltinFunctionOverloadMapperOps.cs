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
using System.Reflection;

using Microsoft.Scripting;
using Microsoft.Scripting.Internal;

using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

[assembly: PythonExtensionType(typeof(BuiltinFunctionOverloadMapper), typeof(BuiltinFunctionOverloadMapperOps))]
namespace IronPython.Runtime.Operations {
    public static class BuiltinFunctionOverloadMapperOps {
        [OperatorMethod, PythonName("__str__")]
        public static string ToString(BuiltinFunctionOverloadMapper self) {
            PythonDictionary overloadList = new PythonDictionary();

            foreach (MethodBase mb in self.Targets) {
                string key = DocBuilder.CreateAutoDoc(mb);
                overloadList[key] = self.Function;
            }
            return overloadList.ToString();
        }

        [OperatorMethod, PythonName("__repr__")]
        public static string ToCodeRepresentation(BuiltinFunctionOverloadMapper self) {
            return ToString(self);
        }
    }
}
