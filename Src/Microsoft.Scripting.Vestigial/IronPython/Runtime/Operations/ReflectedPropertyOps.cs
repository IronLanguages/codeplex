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

using IronPython.Runtime.Types;
using IronPython.Runtime.Operations;

using Microsoft.Scripting;
using Microsoft.Scripting.Internal;
using IronPython.Runtime.Calls;

[assembly: PythonExtensionType(typeof(ReflectedProperty), typeof(ReflectedPropertyOps))]
namespace IronPython.Runtime.Operations {
    public static class ReflectedPropertyOps {
        [PythonName("__get__")]
        public static object GetAttribute(ReflectedProperty self, object instance, object owner) {
            if (self.Getter == null)
                throw Ops.AttributeError("attribute '{0}' of '{1}' object is write-only",
                    self.Name,
                    DynamicTypeOps.GetName(Ops.GetDynamicTypeFromType(self.DeclaringType)));

            object value;
            self.TryGetValue(DefaultContext.Default, instance, owner as DynamicType, out value);

            return value;
        }

        [PythonName("__set__")]
        public static void SetAttribute(ReflectedProperty self, object instance, object value) {
            self.TrySetValue(DefaultContext.Default, instance, Ops.GetDynamicType(instance), value);
        }

        [PythonName("__delete__")]
        public static void DeleteAttribute(ReflectedProperty self, object instance) {
            self.TryDeleteValue(DefaultContext.Default, instance, Ops.GetDynamicType(instance));
        }

        [OperatorMethod, PythonName("__str__")]
        public static string ToString(ReflectedProperty self) {
            return ToCodeRepresentation(self);
        }

        [PropertyMethod, PythonName("__doc__")]
        public static string GetDocumentation(ReflectedProperty info)  {
                return DocBuilder.DocOneInfo(info.Info);
        }


        [OperatorMethod, PythonName("__repr__")]
        public static string ToCodeRepresentation(ReflectedProperty self) {
            return string.Format("<property# {0} on {1}>", 
                self.Name,
                Ops.GetDynamicTypeFromType(self.DeclaringType).Name);
        }
    }
}
