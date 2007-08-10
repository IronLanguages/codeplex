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

using IronPython.Runtime.Types;
using IronPython.Runtime.Operations;

[assembly: PythonExtensionType(typeof(ReflectedField), typeof(ReflectedFieldOps))]
namespace IronPython.Runtime.Operations {
    public static class ReflectedFieldOps {
        [SpecialName, PythonName("__str__")]
        public static string ToString(ReflectedField field) {
            return CodeRepresentation(field);
        }

        [SpecialName, PythonName("__repr__")]
        public static string CodeRepresentation(ReflectedField field) {
            return string.Format("<field# {0} on {1}>", field.info.Name, field.info.DeclaringType.Name);
        }

        [SpecialName, PythonName("__get__")]
        public static object GetDescriptor(ReflectedField self, object instance, object typeContext) {
            PerfTrack.NoteEvent(PerfTrack.Categories.Fields, self);
            if (instance == null) {
                if (self.info.IsStatic) {
                    return self.info.GetValue(null);
                } else {
                    return self;
                }
            } else {
                return self.info.GetValue(Converter.Convert(instance, self.info.DeclaringType));
            }            
        }

        [SpecialName, PythonName("__set__")]
        public static void SetDescriptor(ReflectedField self, object instance, object value) {
            if (instance == null && self.info.IsStatic) {
                DoSet(self, null, value);
            } else if (!self.info.IsStatic) {
                DoSet(self, instance, value);
            } else {
                throw PythonOps.AttributeError("cannot set", self.info.Name);
            }
        }

        [SpecialName, PythonName("__delete__")]
        public static void DeleteDescriptor(ReflectedField self, object instance) {
            throw PythonOps.AttributeErrorForBuiltinAttributeDeletion(self.info.DeclaringType.Name, SymbolTable.StringToId(self.info.Name));
        }

        private static void DoSet(ReflectedField self, object instance, object val) {
            PerfTrack.NoteEvent(PerfTrack.Categories.Fields, self);
            if (instance != null && instance.GetType().IsValueType)
                throw PythonOps.ValueError("Attempt to update field '{0}' on value type '{1}'; value type fields cannot be directly modified", self.info.Name, self.info.DeclaringType.Name);
            if (self.info.IsInitOnly)
                throw PythonOps.AttributeErrorForReadonlyAttribute(self.info.DeclaringType.Name, SymbolTable.StringToId(self.info.Name));

            self.info.SetValue(instance, Converter.Convert(val, self.info.FieldType));
        }        
    }
}
