/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/
using System; using Microsoft;


using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;

using IronPython.Runtime.Types;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime.Operations {
    public static class PythonAssemblyOps {
        private static readonly Dictionary<Assembly, TopNamespaceTracker> assemblyMap = new Dictionary<Assembly, TopNamespaceTracker>();

        [SpecialName]
        public static object GetBoundMember(CodeContext/*!*/ context, Assembly self, string name) {
            TopNamespaceTracker reflectedAssembly = GetReflectedAssembly(context, self);

            if (name == "__dict__") {
                return new PythonDictionary(new WrapperDictionaryStorage(reflectedAssembly));
            }
            MemberTracker mem = reflectedAssembly.TryGetPackageAny(name);
            if (mem != null) {
                if (mem.MemberType == TrackerTypes.Type) {
                    return DynamicHelpers.GetPythonTypeFromType(((TypeTracker)mem).Type);
                }
                // namespace or type collision
                return mem;
            }
            return OperationFailed.Value;
        }

        [SpecialName]
        public static List GetMemberNames(CodeContext/*!*/ context, Assembly self) {
            Debug.Assert(self != null);
            List ret = DynamicHelpers.GetPythonTypeFromType(self.GetType()).GetMemberNames(context);

            foreach (object o in GetReflectedAssembly(context, self).Keys) {
                if (o is string) {
                    ret.AddNoLock((string)o);
                }
            }

            return ret;
        }

        public static object __repr__(Assembly self) {
            Assembly asmSelf = self as Assembly;

            return "<Assembly " + asmSelf.FullName + ">";
        }

        private static TopNamespaceTracker GetReflectedAssembly(CodeContext/*!*/ context, Assembly assem) {
            Debug.Assert(assem != null);
            lock (assemblyMap) {
                TopNamespaceTracker reflectedAssembly;
                if (assemblyMap.TryGetValue(assem, out reflectedAssembly))
                    return reflectedAssembly;

                reflectedAssembly = new TopNamespaceTracker(context.LanguageContext.DomainManager);
                reflectedAssembly.LoadAssembly(assem);
                assemblyMap[assem] = reflectedAssembly;

                return reflectedAssembly;
            }
        }
    }   
}
