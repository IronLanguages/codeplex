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

using IronPython.Runtime.Calls;
using IronPython.Runtime.Types;
using IronPython.Runtime.Operations;


[assembly: PythonExtensionType(typeof(ReflectedProperty), typeof(ReflectedPropertyOps))]
namespace IronPython.Runtime.Operations {
    public static class ReflectedPropertyOps {
        [OperatorMethod, PythonName("__get__")]
        public static object GetAttribute(ReflectedProperty self, object instance, object owner) {
            if (self.Getter == null)
                throw PythonOps.AttributeError("attribute '{0}' of '{1}' object is write-only",
                    self.Name,
                    DynamicTypeOps.GetName(DynamicHelpers.GetDynamicTypeFromType(self.DeclaringType)));

            object value;
            self.TryGetValue(DefaultContext.Default, instance, owner as DynamicType, out value);

            return value;
        }

        [OperatorMethod, PythonName("__set__")]
        public static void SetAttribute(ReflectedProperty self, object instance, object value) {
            // TODO: Throw?  currently we have a test that verifies we never throw when this is called directly.
            self.TrySetValue(DefaultContext.Default, instance, DynamicHelpers.GetDynamicType(instance), value);
        }

        [OperatorMethod, PythonName("__delete__")]
        public static void DeleteAttribute(ReflectedProperty self, object instance) {
            if (self.Setter != null)
                throw PythonOps.AttributeErrorForReadonlyAttribute(
                    DynamicHelpers.GetDynamicTypeFromType(self.DeclaringType).Name,
                    SymbolTable.StringToId(self.Name));
            else
                throw PythonOps.AttributeErrorForBuiltinAttributeDeletion(
                    DynamicHelpers.GetDynamicTypeFromType(self.DeclaringType).Name,
                    SymbolTable.StringToId(self.Name));
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
                DynamicTypeOps.GetName(DynamicHelpers.GetDynamicTypeFromType(self.DeclaringType)));
        }
    }
}
