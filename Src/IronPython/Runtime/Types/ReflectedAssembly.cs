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
using System.Diagnostics;
using System.Threading;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Calls;

namespace IronPython.Runtime.Types {
    public class ReflectedAssemblyType : OpsReflectedType {
        static ReflectedAssemblyType AssemblyType;
        Dictionary<Assembly, ReflectedAssembly> assemblyMap = new Dictionary<Assembly, ReflectedAssembly>();

        internal static ReflectedType MakeDynamicType() {
            if (AssemblyType == null) {
                ReflectedAssemblyType rat = new ReflectedAssemblyType();
                if(Interlocked.CompareExchange<ReflectedAssemblyType>(ref AssemblyType, rat, null)==null)
                    return rat;
            }
            return AssemblyType;
        }

        public ReflectedAssemblyType()
            : base("Assembly", typeof(Assembly), typeof(ReflectedAssemblyType), null) {
        }

        public override bool TryGetAttr(ICallerContext context, object self, SymbolId name, out object ret) {
            Assembly asm = self as Assembly;
            ReflectedAssembly reflectedAssembly = GetReflectedAssembly(asm);

            if (name == SymbolTable.Dict) {
                ret = reflectedAssembly.GetTypeMap();
                return true;
            }

            if (base.TryGetAttr(context, self, name, out ret)) {
                return true;
            }

            if (!reflectedAssembly.TryGetValue(name, out ret))
                throw Ops.AttributeError("assembly {0} has no type {1}", asm.GetName().Name, name);

            return true;
        }

        public ReflectedAssembly GetReflectedAssembly(Assembly assem) {
            Debug.Assert(assem != null);
            lock (this) {
                ReflectedAssembly reflectedAssembly;
                if (assemblyMap.TryGetValue(assem, out reflectedAssembly))
                    return reflectedAssembly;

                reflectedAssembly = new ReflectedAssembly(assem);
                assemblyMap[assem] = reflectedAssembly;

                return reflectedAssembly;
            }
        }

        public override List GetAttrNames(ICallerContext context, object self) {
            Assembly asm = self as Assembly;
            ReflectedAssembly reflectedAssembly = GetReflectedAssembly(asm);

            List ret = base.GetAttrNames(context, self);

            ret.AddRange(reflectedAssembly.GetNamesInScope());
            return ret;
        }

        public override Dict GetAttrDict(ICallerContext context, object self) {
            List attrs = GetAttrNames(context, self);

            Dict res = new Dict();
            foreach (string o in attrs) {
                res[o] = GetAttr(context, self, SymbolTable.StringToId(o));
            }

            return res;
        }

    }

    /// <summary>
    /// ReflectedScope represents an assembly or a namespace in a CLI assembly which can contain types, and other nested namespaces.
    /// </summary>
    public abstract class ReflectedScope {
        // This is a {string -> (ReflectedNamespace|System.Type)} mapping
        internal IDictionary<string, object> rawTypeMap = new Dictionary<string, object>();
        // This is a {SymbolId -> (ReflectedNamespace|ReflectedType)} mapping
        internal FieldIdDict typeMap;

        /// <summary>
        /// This maps strings to System.Type. The types may not have a corresponding ReflectedTypes, and hence
        /// should not be exposed to the user. Use GetTypeMap() if the types will be exposed to the user.
        /// Using this method also prevents publishing all the type names into the SymbolTable unnecessarily.
        /// </summary>
        internal ICollection<object> GetNamesInScope() {
            // Make a copy of the value in case another thread sets rawTypeMap to null
            IDictionary<string, object> rawTypeMapValue = rawTypeMap;

            if (rawTypeMapValue == null) {
                return typeMap.AsObjectKeyedDictionary().Keys;
            } else {
                List<object> ret = new List<object>();
                foreach (object o in rawTypeMapValue.Keys) ret.Add(o);
                return ret;
            }
        }

        /// <summary>
        /// All the types are guaranteed to have a ReflectedTypes, and so can be exposed to the user.
        /// Use GetNamesInScope() if the types need not have to be exposed to the user, as that is more performant.
        /// </summary>
        internal FieldIdDict GetTypeMap() {
            // Have we already promoted the Types to ReflectedTypes?
            if (typeMap != null) return typeMap;

            // We lazily convert the Types to ReflectedTypes
            FieldIdDict newTypeMap = new FieldIdDict();
            foreach (KeyValuePair<string, object> entry in rawTypeMap) {
                object value = entry.Value;
                if (value is Type) {
                    value = Ops.GetDynamicTypeFromType((Type)value);
                }
                newTypeMap[SymbolTable.StringToId(entry.Key)] = value;
            }

            typeMap = newTypeMap;
            rawTypeMap = null;
            return typeMap;
        }

        internal bool TryGetValue(SymbolId key, out object value) {
            // Make a copy of the value in case another thread sets rawTypeMap to null
            IDictionary<string, object> rawTypeMapValue = rawTypeMap;

            if (rawTypeMapValue == null) {
                if (!typeMap.TryGetValue(key, out value)) 
                    return false;
            } else {
                if (!rawTypeMapValue.TryGetValue(key.ToString(), out value)) 
                    return false;
            }

            if (value is Type) {
                // Lazily convert the Type to a ReflectedType
                value = Ops.GetDynamicTypeFromType((Type)value);
            }
            return true;
        }
    }

    /// <summary>
    /// This is utility class for managing namespaces in a CLI assembly. Note that the user will never get an
    /// instance of this type. It is only used by ReflectedAssemblyType to store informations associated with an assembly.
    /// </summary>
    public class ReflectedAssembly : ReflectedScope {
        Assembly assembly;

        internal ReflectedAssembly(Assembly assem) {
            assembly = assem;
            Dictionary<string, ReflectedNamespace> namespaceFullNameMap = new Dictionary<string, ReflectedNamespace>();

            Type[] types = assem.GetTypes();

            // Walk all the types to discover all the namespaces
            foreach (Type type in types) {
                string typeFullName = type.FullName;
                // Nested types are named as "outerType+innerType". However, '+' cannot be used in a name.
                typeFullName = typeFullName.Replace('+', '_');

                string parentScopeName;
                string name = GetNameParts(typeFullName, out parentScopeName);

                if (parentScopeName == null) {
                    // This is global type. Add it to the assembly itself
                    rawTypeMap[typeFullName] = type;
                } else {
                    // Add the type to the containing namespace object
                    ReflectedNamespace ns = GetReflectedNamespace(parentScopeName, namespaceFullNameMap);
                    ns.rawTypeMap[name] = type;
                }
            }
        }

        /// <summary>
        /// Given "a.b.c", sets parentScopeName to "a.b" and returns "c".
        /// Given "a", sets parentScopeName to null and returns "a"
        /// </summary>
        static string GetNameParts(string fullName, out string parentScopeName) {
            int parentNameEnd = fullName.LastIndexOf(Type.Delimiter);
            if (parentNameEnd == -1) {
                // This is a name at the outermost scope
                parentScopeName = null;
                return fullName;
            }

            parentScopeName = fullName.Substring(0, parentNameEnd);
            return fullName.Substring(parentNameEnd + 1);
        }

        ReflectedNamespace GetReflectedNamespace(string namespaceFullName, Dictionary<string, ReflectedNamespace> namespaceFullNameMap) {
            ReflectedNamespace ns;
            if (namespaceFullNameMap.TryGetValue(namespaceFullName, out ns)) return ns;

            string parentScopeName;
            string name = GetNameParts(namespaceFullName, out parentScopeName);
            ReflectedNamespace parent = null;
            if (parentScopeName != null) {
                // This can recursively create namespace objects for the entire chain as reqired
                parent = GetReflectedNamespace(parentScopeName, namespaceFullNameMap);
            }

            // Create a new namespace, and update the parent to point to it
            ns = new ReflectedNamespace(parent, assembly, name);
            if (parent != null) parent.rawTypeMap[name] = ns;
            else rawTypeMap[name] = ns;

            namespaceFullNameMap[namespaceFullName] = ns;

            return ns;
        }

        public override string ToString() {
            return assembly.ToString();
        }
    }

    /// <summary>
    /// This represents a namespace in a CLI assembly. They form a tree, rooted at a ReflectedAssembly.
    /// </summary>
    public class ReflectedNamespace : ReflectedScope, ICustomAttributes {
        ReflectedNamespace parent; // null for top-level namespaces
        string myNamespace; // For the namespace "a.b.c", this will be just "c"
        Assembly assembly;

        public ReflectedNamespace(ReflectedNamespace parentNamespace, Assembly fromAssembly, string nameSpace) {
            myNamespace = nameSpace;
            assembly = fromAssembly;
            parent = parentNamespace;
        }

        [PythonName("__str__")]
        public override string ToString() {
            return String.Format("Namespace {0} in assembly {1}", FullName, assembly.FullName);
        }

        public string FullName {
            [PythonName("full_name")]
            get {
                StringBuilder res = new StringBuilder();

                GetName(res, this);
                return res.ToString();
            }
        }        

        #region Private helper functions
        void GetName(StringBuilder sb, ReflectedNamespace ns) {
            if (ns.parent != null) {
                GetName(sb, ns.parent);
                sb.Append(Type.Delimiter);
            }

            sb.Append(ns.myNamespace);
        }

        #endregion

        #region ICustomAttributes Members

        public bool TryGetAttr(ICallerContext context, SymbolId name, out object value) {
            if (TryGetValue(name, out value)) return true;

            if (name == SymbolTable.Name) { value = FullName; return true; }
            if (name == SymbolTable.File) { value = assembly.FullName; return true; }
            if (name == SymbolTable.Dict) { value = GetTypeMap(); return true; }

            return false;
        }

        public void SetAttr(ICallerContext context, SymbolId name, object value) {
            GetTypeMap()[name] = value;
        }

        public void DeleteAttr(ICallerContext context, SymbolId name) {
            if (!GetTypeMap().Remove(name)) throw Ops.AttributeError("namespace {0} has no type/nested namespace {1}", myNamespace, name);
        }

        public List GetAttrNames(ICallerContext context) {
            List ret = new List();
            ret.AddRange(GetNamesInScope());
            ret.AddNoLock(SymbolTable.Name.ToString());
            ret.AddNoLock(SymbolTable.File.ToString());
            return ret;
        }

        public IDictionary<object, object> GetAttrDict(ICallerContext context) {
            return GetTypeMap();
        }

        #endregion
    }
}
