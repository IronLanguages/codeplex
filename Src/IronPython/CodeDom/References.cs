/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Shared Source License
 * for IronPython. A copy of the license can be found in the License.html file
 * at the root of this distribution. If you can not locate the Shared Source License
 * for IronPython, please send an email to ironpy@microsoft.com.
 * By using this source code in any fashion, you are agreeing to be bound by
 * the terms of the Shared Source License for IronPython.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

using System.Reflection;

namespace IronPython.CodeDom {
    /// <summary>
    /// Provides a local cacheing service for references.  This serves as the proxy
    /// to the remote references where we have all of the assemblies loaded.
    /// </summary>
    class LocalReferences {
        RemoteReferences refs;
        Dictionary<string, TypeReference> cachedTypes = new Dictionary<string,TypeReference>();

        /// <summary> Creates a new LocalReferences that gets it's data from a RemoteReferences </summary>
        public LocalReferences(RemoteReferences backing) {
            refs = backing;
        }

        public TypeReference GetType(string name) {
            TypeReference res;
            if (cachedTypes.TryGetValue(name, out res)) {
                return res;
            }

            res = refs.GetType(name);
            if(res != null) cachedTypes[name] = res;

            return res;
        }
    }

    /// <summary>
    /// Provides a peek into a remtoe domain w/ the references that exist there.
    /// </summary>
    class RemoteReferences : MarshalByRefObject {
        internal static RemoteReferences Instance;

        List<Assembly> refs;
        List<string> asmNames;

        public RemoteReferences() {
        }

        public void Initialize(List<string> references) {
            asmNames = references;
            refs = new List<Assembly>();
            for (int i = 0; i < references.Count; i++) {
                refs.Add(LoadAssembly(references[i]));
            }
            Instance = this;
        }

       public TypeReference GetType(string name) {
            for (int i = 0; i < refs.Count; i++) {
                // try the raw type name first
                Type t = refs[i].GetType(name);
                if (t != null) {
                    return new TypeReference(t, asmNames[i]);
                }
            }
            return null;
        }

        public TypeReference GetMemberType(TypeReference typeRef, string name, out MemberTypes memberType) {
            MemberInfo[] mi = RefTypeToType(typeRef).GetMember(name);

            for (int i = 0; i < mi.Length; i++) {
                if (mi[i] is PropertyInfo) {
                    memberType = MemberTypes.Property;
                    return new TypeReference(((PropertyInfo)mi[i]).PropertyType);
                } else if (mi[i] is FieldInfo) {
                    memberType = MemberTypes.Field;
                    return new TypeReference(((FieldInfo)mi[i]).FieldType);
                } else if (mi[i] is MethodInfo) {
                    memberType = MemberTypes.Method;
                    return new TypeReference(((MethodInfo)mi[i]).ReturnType);
                } else if (mi[i] is EventInfo) {
                    memberType = MemberTypes.Event;
                    return new TypeReference(((EventInfo)mi[i]).EventHandlerType);
                }
                // other types?
            }
            memberType = MemberTypes.All;
            return null;
        }

        private Type RefTypeToType(TypeReference refType) {
            for (int i = 0; i < asmNames.Count; i++) {
                if (asmNames[i] == refType.Assembly) {
                    return refs[i].GetType(refType.FullName);
                }
            }

            return Assembly.Load(refType.Assembly).GetType(refType.FullName);
        }

        private static Assembly LoadAssembly(string name) {                        
            try {
                return Assembly.Load(name);
            } catch {
                try {
                    return Assembly.LoadFrom(name);
                } catch {
                    try {
#pragma warning disable
                        return Assembly.LoadWithPartialName(name);
#pragma warning enable
                    } catch {                        
                    }
                }
            }
            return null;
        }

    }

    [Serializable]
    class TypeReference {
        public string FullName;
        public string Assembly;
        private Dictionary<string, MemberInfo> Members;
        private RemoteReferences Parent = RemoteReferences.Instance;

        public TypeReference(Type t, string asm) {
            FullName = t.FullName;
            Assembly = asm;
        }

        public TypeReference(Type t) {
            FullName = t.FullName;
            Assembly = t.Assembly.FullName;
        }

        public TypeReference GetMemberType(string name, out MemberTypes memType) {
            MemberInfo mi;
            memType = MemberTypes.All;
            TypeReference res = null;
            if (Members == null || !Members.TryGetValue(name, out mi)) {
                // calcluate member type.

                res = Parent.GetMemberType(this, name, out memType);
                if (res != null) {
                    if (Members == null) Members = new Dictionary<string, MemberInfo>();
                    Members[name] = new MemberInfo(memType, res);
                }
            } else {
                memType = mi.MemberType;
                res = mi.Type;
            }

            return res;
        }

        [Serializable]
        class MemberInfo {
            public MemberInfo(MemberTypes mt, TypeReference tr) {
                MemberType = mt;
                Type = tr;
            }

            public MemberTypes MemberType;
            public TypeReference Type;
        }
    }
    
}
