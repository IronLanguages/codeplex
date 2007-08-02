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
        [OperatorMethod]
        public static object __get__(ReflectedProperty self, object instance, object owner) {
            if (self.Getter == null)
                throw PythonOps.AttributeError("attribute '{0}' of '{1}' object is write-only",
                    self.Name,
                    DynamicTypeOps.GetName(DynamicHelpers.GetDynamicTypeFromType(self.DeclaringType)));

            object value;
            self.TryGetValue(DefaultContext.Default, instance, owner as DynamicType, out value);

            return value;
        }

        [OperatorMethod]
        public static void __set__(ReflectedProperty self, object instance, object value) {
            // TODO: Throw?  currently we have a test that verifies we never throw when this is called directly.
            self.TrySetValue(DefaultContext.Default, instance, DynamicHelpers.GetDynamicType(instance), value);
        }

        [OperatorMethod]
        public static void __delete__(ReflectedProperty self, object instance) {
            if (self.Setter != null)
                throw PythonOps.AttributeErrorForReadonlyAttribute(
                    DynamicHelpers.GetDynamicTypeFromType(self.DeclaringType).Name,
                    SymbolTable.StringToId(self.Name));
            else
                throw PythonOps.AttributeErrorForBuiltinAttributeDeletion(
                    DynamicHelpers.GetDynamicTypeFromType(self.DeclaringType).Name,
                    SymbolTable.StringToId(self.Name));
        }

        [OperatorMethod]
        public static string __str__(ReflectedProperty self) {
            return __repr__(self);
        }

        [PropertyMethod]
        public static string __doc__(ReflectedProperty info) {
                return DocBuilder.DocOneInfo(info.Info);
        }


        [OperatorMethod]
        public static string __repr__(ReflectedProperty self) {
            return string.Format("<property# {0} on {1}>", 
                self.Name,
                DynamicTypeOps.GetName(DynamicHelpers.GetDynamicTypeFromType(self.DeclaringType)));
        }
    }
}
