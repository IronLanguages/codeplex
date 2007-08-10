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
using IronPython.Runtime.Calls;

[assembly: PythonExtensionType(typeof(ReflectedIndexer), typeof(ReflectedIndexerOps))]
namespace IronPython.Runtime.Operations {
    public static class ReflectedIndexerOps {
        [PythonName("__get__")]
        public static object GetAttribute(ReflectedIndexer self, object instance, object owner) {
            object val;
            self.TryGetValue(DefaultContext.Default, instance, owner as DynamicType, out val);
            return val;
        }

        [SpecialName]
        public static object GetItem(ReflectedIndexer self, object key) {
            IParameterSequence tupleKey = key as IParameterSequence;
            if (tupleKey != null && tupleKey.IsExpandable) {
                return self.GetValue(DefaultContext.Default, tupleKey.Expand(null));
            }

            return self.GetValue(DefaultContext.Default, new object[] { key });
        }

        [SpecialName]
        public static void SetItem(ReflectedIndexer self, object key, object value) {
            IParameterSequence tupleKey = key as IParameterSequence;
            if (tupleKey != null && tupleKey.IsExpandable) {
                if (!self.SetValue(DefaultContext.Default, tupleKey.Expand(null), value)) {
                    throw PythonOps.AttributeErrorForReadonlyAttribute(self.DeclaringType.Name, SymbolTable.StringToId(self.Name));
                }
                return;
            }

            if (!self.SetValue(DefaultContext.Default, new object[] { key }, value)) {
                throw PythonOps.AttributeErrorForReadonlyAttribute(self.DeclaringType.Name, SymbolTable.StringToId(self.Name));
            }
        }

        [SpecialName]
        public static void DeleteItem(ReflectedIndexer self, object key) {
            if (self.Setter != null)
                throw PythonOps.AttributeErrorForReadonlyAttribute(
                    DynamicHelpers.GetDynamicTypeFromType(self.DeclaringType).Name,
                    SymbolTable.StringToId(self.Name));
            else
                throw PythonOps.AttributeErrorForBuiltinAttributeDeletion(
                    DynamicHelpers.GetDynamicTypeFromType(self.DeclaringType).Name,
                    SymbolTable.StringToId(self.Name));
        }
    }
}
