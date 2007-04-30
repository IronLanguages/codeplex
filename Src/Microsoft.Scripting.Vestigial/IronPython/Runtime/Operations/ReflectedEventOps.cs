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

using IronPython.Runtime.Types;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Calls;

[assembly: PythonExtensionType(typeof(ReflectedEvent), typeof(ReflectedEventOps))]
namespace IronPython.Runtime.Operations {
    public static class ReflectedEventOps {
        [OperatorMethod, PythonName("__str__")]
        public static string ToString(ReflectedEvent self) {
            return string.Format("<event# {0} on {1}>", self.Info.Name, self.Info.DeclaringType.Name);
        }

        [PythonName("__set__")]
        public static void SetAttribute(ReflectedEvent self, object instance, object value) {
            self.TrySetValue(DefaultContext.Default, instance, Ops.GetDynamicType(instance), value);
        }

        [PythonName("__delete__")]
        public static void DeleteAttribute(ReflectedEvent self, object instance) {
            self.TryDeleteValue(DefaultContext.Default, instance, Ops.GetDynamicType(instance));
        }
    }
}
