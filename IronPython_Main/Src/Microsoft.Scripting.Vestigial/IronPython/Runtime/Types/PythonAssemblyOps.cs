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
using System.Reflection;
using System.Diagnostics;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Internal;

[assembly: PythonExtensionType(typeof(Assembly), typeof(PythonAssemblyOps))]
namespace IronPython.Runtime.Types {
    public static class PythonAssemblyOps {
        private static Dictionary<Assembly, TopReflectedPackage> assemblyMap = new Dictionary<Assembly, TopReflectedPackage>();

        [OperatorMethod]
        public static object GetBoundMember(Assembly self, string name) {
            TopReflectedPackage reflectedAssembly = GetReflectedAssembly(self);

            if (name == "__dict__") {
                return new PythonDictionary(reflectedAssembly.GetCustomMemberDictionary(DefaultContext.Default));
            }
            object value;
            if (reflectedAssembly.TryGetBoundCustomMember(DefaultContext.Default, SymbolTable.StringToId(name), out value)) {
                return value;
            }
            return DBNull.Value;
        }

        [OperatorMethod]
        public static IList<SymbolId> GetMemberNames(Assembly self) {
            TopReflectedPackage reflectedAssembly = GetReflectedAssembly(self);

            IList<object> res = reflectedAssembly.GetCustomMemberNames(DefaultContext.Default);
            List<SymbolId> ret = new List<SymbolId>();
            foreach (object o in res) {
                if (o is string) {
                    ret.Add(SymbolTable.StringToId((string)o));
                }
            }

            return ret;
        }

        private static TopReflectedPackage GetReflectedAssembly(Assembly assem) {
            Debug.Assert(assem != null);
            lock (assemblyMap) {
                TopReflectedPackage reflectedAssembly;
                if (assemblyMap.TryGetValue(assem, out reflectedAssembly))
                    return reflectedAssembly;

                reflectedAssembly = new TopReflectedPackage(true);
                reflectedAssembly.LoadAssembly(assem);
                assemblyMap[assem] = reflectedAssembly;

                return reflectedAssembly;
            }
        }
    }   
}
