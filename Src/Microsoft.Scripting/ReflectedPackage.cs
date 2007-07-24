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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Reflection;

using Microsoft.Scripting.Hosting;

namespace Microsoft.Scripting {
    public class AssemblyLoadedEventArgs : EventArgs {
        private Assembly _assembly;

        public AssemblyLoadedEventArgs(Assembly assembly) {
            _assembly = assembly;
        }

        public Assembly Assembly {
            get {
                return _assembly;
            }
        }
    }

    /// <summary>
    /// Represents the top reflected package which contains extra information such as
    /// all the assemblies loaded and the built-in modules.
    /// </summary>
    public class TopReflectedPackage : ReflectedPackage {
        private int _initialized;
        private bool _isolated;
        private int _lastDiscovery = 0;

        internal TopReflectedPackage()
            : base(null) {
            SetTopPackage(this);
        }

        /// <summary>
        /// Creates a top reflected package that is optionally isolated
        /// from all other packages in the system.
        /// </summary>
        public TopReflectedPackage(bool isolated)
            : this() {
            this._isolated = isolated;
        }

        public void Clear() {
            _initialized = 0;
            _lastDiscovery = 0;
            _dict = new SymbolDictionary();
            _packageAssemblies = new List<Assembly>();
            _typeNames = new Dictionary<Assembly, TypeNames>();
        }

        #region Public API Surface

        /// <summary>
        /// returns the package associated with the specified namespace and
        /// updates the associated module to mark the package as imported.
        /// </summary>
        public ScriptModule TryGetPackage(string name) {
            return TryGetPackage(SymbolTable.StringToId(name));
        }

        public ScriptModule TryGetPackage(SymbolId name) {
            ScriptModule pm = TryGetPackageAny(name) as ScriptModule;
            if (pm != null) {
                pm.PackageImported = true;
                return pm;
            }
            return null;
        }

        public object TryGetPackageAny(string name) {
            return TryGetPackageAny(SymbolTable.StringToId(name));
        }

        public object TryGetPackageAny(SymbolId name) {
            Initialize();
            object ret;
            if (TryGetMember(name, out ret)) {
                return ret;
            }
            return null;
        }

        public object TryGetPackageLazy(SymbolId name) {
            object ret;
            if (_dict.TryGetValue(name, out ret)) {
                return ret;
            }
            return null;
        }

        /// <summary>
        /// Ensures that the assembly is loaded
        /// </summary>
        /// <param name="assem"></param>
        /// <returns>true if the assembly was loaded for the first time. 
        /// false if the assembly had already been loaded before</returns>
        public bool LoadAssembly(Assembly assem) {
            lock (this) {
                if (_packageAssemblies.Contains(assem)) {
                    // The assembly is already loaded. There is nothing more to do
                    return false;
                }

                _packageAssemblies.Add(assem);
            }

            EventHandler<AssemblyLoadedEventArgs> assmLoaded = AssemblyLoaded;
            if (assmLoaded != null) {
                assmLoaded(this, new AssemblyLoadedEventArgs(assem));
            }
            return true;
        }


        public void Initialize() {
            if (_initialized != 0) return;
            if (System.Threading.Interlocked.Exchange(ref _initialized, 1) == 0) {

                // add mscorlib
                ClrModule.GetInstance().AddReference(typeof(string).Assembly);
                // add system.dll
                ClrModule.GetInstance().AddReference(typeof(System.Diagnostics.Debug).Assembly);
            }
        }

        #endregion

        protected override void LoadNamespaces() {
            lock (this) {
                for (int i = _lastDiscovery; i < PackageAssemblies.Count; i++) {
                    DiscoverAllTypes(PackageAssemblies[i]);
                }
                _lastDiscovery = PackageAssemblies.Count;
            }
        }

        public event EventHandler<AssemblyLoadedEventArgs> AssemblyLoaded;
    }

    /// <summary>
    /// ReflectedPackages represent a CLS namespace.  ReflectedPackages aren't
    /// exposed to the user.  Instead PythonModule holds a reference to the 
    /// ReflectedPackage and the two share the same dictionary for updating
    /// what is present within the namespace.
    /// </summary>
    public class ReflectedPackage : ICustomMembers {
        // _dict contains all the currently loaded entries. However, there may be pending types that have
        // not yet been loaded in _typeNames
        internal IAttributesCollection _dict = new SymbolDictionary();

        internal List<Assembly> _packageAssemblies = new List<Assembly>();
        internal Dictionary<Assembly, TypeNames> _typeNames = new Dictionary<Assembly, TypeNames>();

        private string _fullName; // null for the TopReflectedPackage
        private TopReflectedPackage _topPackage;

        #region Protected API Surface

        private ReflectedPackage() {
            _dict = new SymbolDictionary();
        }

        public override string ToString() {
            return base.ToString() + ":" + _fullName;
        }

        protected ReflectedPackage(string name)
            : this() {
            _fullName = name;
        }

        #endregion

        #region Internal API Surface

        internal ReflectedPackage GetOrMakeChildPackage(string childName, Assembly assem) {
            Debug.Assert(childName.IndexOf(Type.Delimiter) == -1); // This is the simple name, not the full name
            Debug.Assert(_packageAssemblies.Contains(assem)); // Parent namespace must contain all the assemblies of the child

            object ret;
            if (_dict.TryGetValue(SymbolTable.StringToId(childName), out ret)) {
                // if it's not a module, we'll wipe it out below, eg "def System(): pass" then 
                // "import System" will result in the namespace being visible.
                ScriptModule pm = ret as ScriptModule;
                ReflectedPackage res;
                do {
                    res = pm.InnerModule as ReflectedPackage;
                    if (res != null) {
                        if (!res._packageAssemblies.Contains(assem)) res._packageAssemblies.Add(assem);
                        return res;
                    }

                    pm = pm.InnerModule as ScriptModule;
                } while (pm != null);
            }

            return MakeChildPackage(childName, assem);
        }

        private ReflectedPackage MakeChildPackage(string childName, Assembly assem) {
            ReflectedPackage rp = new ReflectedPackage();
            rp.SetTopPackage(_topPackage);
            rp._packageAssemblies.Add(assem);

            ScriptModule smod;
            
            //if (/*isolated || !ScriptDomainManager.CurrentManager.TryGetScriptModule(GetFullChildName(childName), out smod)*/) {
                // no collisions (yet), create a new module for the package.
                smod = ScriptDomainManager.CurrentManager.CreateModule(childName);
                smod.PackageImported = true;
            //}
            // else there's already a module by this name.  We'll just
            // set the InnerModule but not make it visible until
            // the user does an import (and we set PackageImported).

            rp._fullName = GetFullChildName(childName);
            smod.InnerModule = rp;
            _dict[SymbolTable.StringToId(childName)] = smod;
            return rp;
        }

        string GetFullChildName(string childName) {
            Debug.Assert(childName.IndexOf(Type.Delimiter) == -1); // This is the simple name, not the full name
            if (_fullName == null) {
                return childName;
            }

            return _fullName + Type.Delimiter + childName;
        }

        static DynamicType LoadType(Assembly assem, string fullTypeName) {
            Type type = assem.GetType(fullTypeName);
            // We should ignore nested types. They will be loaded when the containing type is loaded
            Debug.Assert(!Utils.Reflection.IsNested(type));
            DynamicType dynamicType = DynamicHelpers.GetDynamicTypeFromType(type);
            return dynamicType;
        }

        internal void AddTypeName(string typeName, Assembly assem) {
            Debug.Assert(typeName.IndexOf(Type.Delimiter) == -1); // This is the simple name, not the full name

            if (!_typeNames.ContainsKey(assem)) {
                _typeNames[assem] = new TypeNames(assem, _fullName);
            }
            _typeNames[assem].AddTypeName(typeName);

            string normalizedTypeName = TypeCollision.GetNormalizedTypeName(typeName);
            if (_dict.ContainsObjectKey(normalizedTypeName)) {
                // A similarly named type already exists. We need to unify the types
                DynamicType newDynamicType = LoadType(assem, GetFullChildName(typeName));
                SymbolId normalizedTypeNameId = SymbolTable.StringToId(normalizedTypeName);

                IConstructorWithCodeContext existingTypeEntity = (IConstructorWithCodeContext)_dict[normalizedTypeNameId];
                _dict[normalizedTypeNameId] = TypeCollision.UpdateTypeEntity(existingTypeEntity, newDynamicType);
                return;
            }
        }

        /// <summary>
        /// Loads all the types from all assemblies that contribute to the current namespace (but not child namespaces)
        /// </summary>
        void LoadAllTypes() {
            foreach (TypeNames typeNameList in _typeNames.Values) {
                foreach (string typeName in typeNameList.GetNormalizedTypeNames()) {
                    object value;
                    if (!TryGetMember(SymbolTable.StringToId(typeName), out value)) {
                        Debug.Assert(false, "We should never get here as TryGetMember should raise a TypeLoadException");
                        throw new TypeLoadException(typeName);
                    }
                }
            }
        }

        #endregion

        protected void DiscoverAllTypes(Assembly assem) {
            ReflectedPackage previousPackage = null;
            string previousFullNamespace = String.Empty; // Note that String.Empty is not a valid namespace

            foreach (TypeName typeName in AssemblyTypeNames.GetTypeNames(assem)) {
                ReflectedPackage package;
                Debug.Assert(typeName.Namespace != String.Empty);
                if (typeName.Namespace == previousFullNamespace) {
                    // We have a cache hit. We dont need to call GetOrMakePackageHierarchy (which generates
                    // a fair amount of temporary substrings)
                    package = previousPackage;
                } else {
                    package = GetOrMakePackageHierarchy(assem, typeName.Namespace);
                    previousFullNamespace = typeName.Namespace;
                    previousPackage = package;
                }

                package.AddTypeName(typeName.Name, assem);
            }
        }

        /// <summary>
        /// Populates the tree with nodes for each part of the namespace
        /// </summary>
        /// <param name="assem"></param>
        /// <param name="fullNamespace">Full namespace name. It can be null (for top-level types)</param>
        /// <returns></returns>
        private ReflectedPackage GetOrMakePackageHierarchy(Assembly assem, string fullNamespace) {
            if (fullNamespace == null) {
                // null is the top-level namespace
                return this;
            }

            ReflectedPackage ret = this;
            string[] pieces = fullNamespace.Split(Type.Delimiter);
            for (int i = 0; i < pieces.Length; i++) {
                ret = ret.GetOrMakeChildPackage(pieces[i], assem);
            }

            return ret;
        }
        /// <summary>
        /// As a fallback, so if the type does exist in any assembly. This would happen if a new type was added
        /// that was not in the hardcoded list of types. 
        /// This code is not accurate because:
        /// 1. We dont deal with generic types (TypeCollision). 
        /// 2. Previous calls to GetCustomMemberNames (eg. "from foo import *" in Python) would not have included this type.
        /// 3. This does not deal with new namespaces added to the assembly
        /// </summary>
        IConstructorWithCodeContext CheckForUnlistedType(string nameString) {
            string fullTypeName = GetFullChildName(nameString);
            foreach (Assembly assem in _packageAssemblies) {
                Type type = assem.GetType(fullTypeName, false);
                if (type == null || Utils.Reflection.IsNested(type)) {
                    continue;
                }

                bool publishType = type.IsPublic || ScriptDomainManager.Options.PrivateBinding;
                if (!publishType) {
                    continue;
                }

                DynamicType dynamicType = DynamicHelpers.GetDynamicTypeFromType(type);
                // We dont use TypeCollision.UpdateTypeEntity here because we do not handle generic type names                    
                return dynamicType;
            }

            return null;
        }

        protected bool TryGetMember(SymbolId name, out object value) {
            LoadNamespaces();

            if (_dict.TryGetValue(name, out value)) {
                if (value == Uninitialized.Instance) return false;
                return true;
            }

            IConstructorWithCodeContext existingTypeEntity = null;
            string nameString = SymbolTable.IdToString(name);

            if (nameString.IndexOf(Type.Delimiter) != -1) {
                value = null;
                return false;
            }

            // Look up the type names and load the type if its name exists

            foreach (KeyValuePair<Assembly, TypeNames> kvp in _typeNames) {
                if (!kvp.Value.Contains(nameString)) {
                    continue;
                }

                existingTypeEntity = kvp.Value.UpdateTypeEntity(existingTypeEntity, nameString);
            }

            if (existingTypeEntity == null) {
                existingTypeEntity = CheckForUnlistedType(nameString);
            }

            if (existingTypeEntity != null) {
                _dict[name] = existingTypeEntity;
                value = existingTypeEntity;
                return true;
            }

            return false;
        }

        #region ICustomMembers Members

        public bool TryGetCustomMember(CodeContext context, SymbolId name, out object value) {
            if (TryGetMember(name, out value)) {
                return true;
            }

            return DynamicHelpers.GetDynamicTypeFromType(typeof(ReflectedPackage)).TryGetBoundMember(
                context,
                this,
                name,
                out value);
        }

        public bool TryGetBoundCustomMember(CodeContext context, SymbolId name, out object value) {
            return TryGetCustomMember(context, name, out value);
        }

        public void SetCustomMember(CodeContext context, SymbolId name, object value) {
            _dict[name] = value;
        }

        public bool DeleteCustomMember(CodeContext context, SymbolId name) {
            LoadNamespaces();

            if (!_dict.ContainsKey(name)) throw new MissingMemberException(String.Format("could't find {0}", SymbolTable.IdToString(name)));

            _dict[name] = Uninitialized.Instance;
            return true;
        }

        public IList<object> GetCustomMemberNames(CodeContext context) {
            LoadNamespaces();

            List<object> res = new List<object>(_dict.AsObjectKeyedDictionary().Keys);

            foreach (KeyValuePair<Assembly, TypeNames> kvp in _typeNames) {
                foreach (string typeName in kvp.Value.GetNormalizedTypeNames()) {
                    res.Add(typeName);
                }
            }

            res.Sort();
            return res;
        }

        public IDictionary<object, object> GetCustomMemberDictionary(CodeContext context) {
            LoadNamespaces();
            LoadAllTypes();

            return _dict.AsObjectKeyedDictionary();
        }

        #endregion

        internal static void PublishType(Type t) {
            EventHandler<TypePublishedEventArgs> tp = TypePublished;
            if (tp != null) {
                tp(t.Assembly, new TypePublishedEventArgs(t));
            }
        }

        /// <summary>
        /// Provides notification of when a type is loaded so that a language can customize it appropriately.
        /// </summary>
        public static event EventHandler<TypePublishedEventArgs> TypePublished;

        public IList<Assembly> PackageAssemblies {
            get {
                return _packageAssemblies;
            }
        }

        protected virtual void LoadNamespaces() {
            if (_topPackage != null) {
                _topPackage.LoadNamespaces();
            }
        }

        protected void SetTopPackage(TopReflectedPackage pkg) {
            _topPackage = pkg;
        }

        #region Private Implementation Details

        #endregion

        /// <summary>
        /// This stores all the public non-nested type names in a single namespace and from a single assembly.
        /// This allows inspection of the namespace without eagerly loading all the types. Eagerly loading
        /// types slows down startup, increases working set, and is semantically incorrect as it can trigger
        /// TypeLoadExceptions sooner than required.
        /// </summary>
        internal class TypeNames {
            List<string> _simpleTypeNames = new List<string>();
            Dictionary<string, List<string>> _genericTypeNames = new Dictionary<string, List<string>>();

            Assembly _assembly;
            string _fullNamespace;

            internal TypeNames(Assembly assembly, string fullNamespace) {
                _assembly = assembly;
                _fullNamespace = fullNamespace;
            }

            internal bool Contains(string normalizedTypeName) {
                Debug.Assert(normalizedTypeName.IndexOf(Type.Delimiter) == -1); // This is the simple name, not the full name
                Debug.Assert(TypeCollision.GetNormalizedTypeName(normalizedTypeName) == normalizedTypeName);

                return _simpleTypeNames.Contains(normalizedTypeName) || _genericTypeNames.ContainsKey(normalizedTypeName);
            }

            internal IConstructorWithCodeContext UpdateTypeEntity(IConstructorWithCodeContext existingTypeEntity, string normalizedTypeName) {
                Debug.Assert(normalizedTypeName.IndexOf(Type.Delimiter) == -1); // This is the simple name, not the full name
                Debug.Assert(TypeCollision.GetNormalizedTypeName(normalizedTypeName) == normalizedTypeName);

                // Look for a non-generic type
                if (_simpleTypeNames.Contains(normalizedTypeName)) {
                    DynamicType newDynamicType = LoadType(_assembly, GetFullChildName(normalizedTypeName));
                    existingTypeEntity = TypeCollision.UpdateTypeEntity(existingTypeEntity, newDynamicType);
                }

                // Look for generic types
                if (_genericTypeNames.ContainsKey(normalizedTypeName)) {
                    List<string> actualNames = _genericTypeNames[normalizedTypeName];
                    foreach (string actualName in actualNames) {
                        DynamicType newDynamicType = LoadType(_assembly, GetFullChildName(actualName));
                        existingTypeEntity = TypeCollision.UpdateTypeEntity(existingTypeEntity, newDynamicType);
                    }
                }

                return existingTypeEntity;
            }

            internal void AddTypeName(string typeName) {
                Debug.Assert(typeName.IndexOf(Type.Delimiter) == -1); // This is the simple name, not the full name

                string normalizedName = TypeCollision.GetNormalizedTypeName(typeName);
                if (normalizedName == typeName) {
                    _simpleTypeNames.Add(typeName);
                } else {
                    List<string> actualNames;
                    if (_genericTypeNames.ContainsKey(normalizedName)) {
                        actualNames = _genericTypeNames[normalizedName];
                    } else {
                        actualNames = new List<string>();
                        _genericTypeNames[normalizedName] = actualNames;
                    }
                    actualNames.Add(typeName);
                }
            }

            string GetFullChildName(string childName) {
                Debug.Assert(childName.IndexOf(Type.Delimiter) == -1); // This is the simple name, not the full name
                if (_fullNamespace == null) {
                    return childName;
                }

                return _fullNamespace + Type.Delimiter + childName;
            }

            internal ICollection<string> GetNormalizedTypeNames() {
                List<string> normalizedTypeNames = new List<string>();

                normalizedTypeNames.AddRange(_simpleTypeNames);
                normalizedTypeNames.AddRange(_genericTypeNames.Keys);

                return normalizedTypeNames;
            }
        }
    }

    public class TypePublishedEventArgs : EventArgs {
        private Type _type;

        public TypePublishedEventArgs(Type type) {
            _type = type;
        }

        public Type Type {
            get {
                return _type;
            }
        }
    }
}
