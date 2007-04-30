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

using Microsoft.Scripting;
using Microsoft.Scripting.Internal;
using Microsoft.Scripting.Hosting;

#if !SILVERLIGHT
using ComObjectWithTypeInfo = IronPython.Runtime.Types.ComObjectWithTypeInfo;
#endif

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
        private Dictionary<Assembly, bool> _loadedAssemblies = new Dictionary<Assembly, bool>();
        private int _initialized;
        private bool _isolated;

        internal TopReflectedPackage()
            : base(String.Empty) {
        }

        /// <summary>
        /// Creates a top reflected package that is optionally isolated
        /// from all other packages in the system.
        /// </summary>
        internal TopReflectedPackage(bool isolated)
            : base(String.Empty) {
            this._isolated = isolated;
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
            if (_dict.TryGetValue(name, out ret)) {
                return ret;
            }
            return null;
        }

        public bool LoadAssembly(Assembly assem) {
            bool loaded;
            if (_loadedAssemblies.TryGetValue(assem, out loaded)) {
                return false;
            }
           
            Type[] types = LoadTypesFromAssembly(assem);

            foreach (Type type in types) {
                //  Skip nested types. They get loaded during parent type initalization.
                //  (do not use Type.IsNested property as it is not available in Silverlight)
                if (type.DeclaringType != null) {
                    continue;
                }
                if ((!type.IsPublic && !ScriptDomainManager.Options.PrivateBinding)) {
                    continue;
                }

                // save all the namespaces, types will be lazily initialized
                // on demand in GetBoundAttr
                ReflectedPackage pkg = GetOrMakeTopPackage(assem, type.Namespace);

                if (!loaded) {
                    // We dont save all types since it requires us to hold on to the Type object unnecessarily.
                    // We do load all the types, so its not clear if this optimizations is really useful.

#if !SILVERLIGHT // ComObject
                    // Publish all COM interfaces so that ComObject can access it when needed
                    // for generic Runtime-callable-wrappers
                    ComObjectWithTypeInfo.PublishComInterface(type);
#endif
                    // We need to save top-level types immediately
                    if (String.IsNullOrEmpty(type.Namespace)) {
                        pkg.SaveType(type);
                    }
                } else {
                    // doing a non-lazy reload...  force all types to get loaded
                    pkg.LoadAllTypes();
                }
            }

            // Assembly was loaded
            _loadedAssemblies[assem] = true;

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
                ClrModule.GetInstance().AddReferenceByName(typeof(string).Assembly.FullName);
                // add system.dll
                ClrModule.GetInstance().AddReferenceByName(typeof(System.Diagnostics.Debug).Assembly.FullName);
            }
        }

        public Dictionary<Assembly, bool> LoadedAssemblies {
            get {
                return _loadedAssemblies;
            }
        }

        #endregion

        #region Private Implementation Details

        private ReflectedPackage GetOrMakeTopPackage(Assembly assm, string ns) {
            ReflectedPackage ret = this;
            if (ns != null) {

                string[] pieces = ns.Split(Type.Delimiter);
                for (int i = 0; i < pieces.Length; i++) {
                    if (!ret._packageAssemblies.Contains(assm)) ret._packageAssemblies.Add(assm);
                    ret = ret.GetOrMakePackage(String.Join(".", pieces, 0, i + 1), pieces[i], _isolated);
                }
            }

            if (!ret._packageAssemblies.Contains(assm)) ret._packageAssemblies.Add(assm);
            return ret;
        }

       
        #endregion

        public event EventHandler<AssemblyLoadedEventArgs> AssemblyLoaded;
    }

    /// <summary>
    /// ReflectedPackages represent a CLS namespace.  ReflectedPackages aren't
    /// exposed to the user.  Instead PythonModule holds a reference to the 
    /// ReflectedPackage and the two share the same dictionary for updating
    /// what is present within the namespace.
    /// </summary>
    public class ReflectedPackage : ICustomMembers {
        internal IAttributesCollection _dict;
        internal List<Assembly> _packageAssemblies = new List<Assembly>();        

        private IDictionary<SymbolId, int> _loadLevels;
        private string _fullName;
        private int _assemblyTypeLoadIndex = -1;
        
        #region Protected API Surface

        private ReflectedPackage() {
            _dict = new SymbolDictionary();
            _loadLevels = new Dictionary<SymbolId, int>();
        }

        protected ReflectedPackage(string name)
            : this() {
            _fullName = name;
        }

        #endregion

        #region Internal API Surface

        internal object SaveType(Type type) {
            string name = GetCoreTypeName(type);
            object existingType;

            // if there's no collisions we just save the type
            if (!_dict.TryGetValue(SymbolTable.StringToId(name), out existingType)) {
                DynamicType ret;
                _dict[SymbolTable.StringToId(name)] = ret = DynamicHelpers.GetDynamicTypeFromType(type);
                return ret;
            }

            // two types w/ the same name.  Good examples are:
            //      System.Nullable and System.Nullable<T>
            //      System.IComparable vs System.IComparable<T>
            //    In this case we need to allow the user to disambiguate the two.
            // 
            // Or we could have a recompile & reload cycle (or a really bad
            // collision).  In those cases the new type wins.

            TypeCollision tc = existingType as TypeCollision;
            if (tc != null) {
                // we've collided before...
                if (!type.ContainsGenericParameters) {
                    // we're replacing the existing non generic type 
                    // or moving some random generic type from the "base"
                    // reflected type into the list of generics.
                    _dict[SymbolTable.StringToId(name)] = tc = tc.CloneWithNewBase(type);
                } else {
                    // we're a generic type.  we just need to add 
                    // ourselves to the list or replace an existing type 
                    // of the same arity.
                    tc.UpdateType(type);
                }
            } else {
                // first time collision on this name, provide
                // the type collision to disambiguate.  The non-generic
                // is exposed by default, and the generic gets added
                // to the list to disambiguate.
                DynamicType rt = existingType as DynamicType;

                if (rt.UnderlyingSystemType.ContainsGenericParameters) {
                    // existing type has generics, append it.
                    tc = new TypeCollision(type);
                    _dict[SymbolTable.StringToId(name)] = tc;
                    tc.UpdateType(rt.UnderlyingSystemType);
                } else if (type.ContainsGenericParameters) {
                    // new type has generics, append it.
                    tc = new TypeCollision(rt.UnderlyingSystemType);
                    _dict[SymbolTable.StringToId(name)] = tc;
                    tc.UpdateType(type);
                } else {
                    // neither type has generics, replace the old
                    // non-generic type w/ the new non-generic type.
                    _dict[SymbolTable.StringToId(name)] = DynamicHelpers.GetDynamicTypeFromType(type);
                }
            }
            return tc;
        }

        internal ReflectedPackage GetOrMakePackage(string fullName, string name, bool isolated) {
            object ret;
            if (_dict.TryGetValue(SymbolTable.StringToId(name), out ret)) {
                // if it's not a module we'll wipe it out below, eg def System(): pass then 
                // import System will result in the namespace being visible.
                ScriptModule pm = ret as ScriptModule;
                ReflectedPackage res;
                do {
                    res = pm.InnerModule as ReflectedPackage;
                    if (res != null) return res;

                    pm = pm.InnerModule as ScriptModule;
                } while (pm != null);
            }

            return MakePackage(fullName, name, isolated);
        }

        private ReflectedPackage MakePackage(string fullName, string name, bool isolated) {
            ReflectedPackage rp = new ReflectedPackage();
            ScriptModule smod;
            
            //if (/*isolated || !ScriptDomainManager.CurrentManager.TryGetScriptModule(name, out smod)*/) {
                // no collisions (yet), create a new module for the package.
                smod = ScriptDomainManager.CurrentManager.CreateModule(name);
                smod.PackageImported = true;
            //}
            // else there's already a module by this name.  We'll just
            // set the InnerModule but not make it visible until
            // the user does an import (and we set PackageImported).

            rp._fullName = fullName;
            smod.InnerModule = rp;
            _dict[SymbolTable.StringToId(name)] = smod;
            return rp;
        }

        internal void LoadAllTypes() {
            // if new assemblies are loaded we need to re-load their types...
            if (_assemblyTypeLoadIndex == _packageAssemblies.Count) return;

            for (int i = _assemblyTypeLoadIndex + 1; i < _packageAssemblies.Count; i++) {
                Type[] types = LoadTypesFromAssembly(_packageAssemblies[i]);
                for (int j = 0; j < types.Length; j++) {
                    if (types[j].DeclaringType != null) {
                        continue;
                    }

#if !SILVERLIGHT // ComObjectWithTypeInfo
                    ComObjectWithTypeInfo.PublishComInterface(types[j]);
#endif
                    // only load types that are in our namespace
                    // but not in a child namespace
                    if (_fullName.Length < types[j].FullName.Length &&
                        String.Compare(_fullName, 0, types[j].FullName, 0, _fullName.Length) == 0 &&
                        types[j].FullName.IndexOf(Type.Delimiter, _fullName.Length + 1) == -1) {
                        SaveType(types[j]);
                    }
                }

                _assemblyTypeLoadIndex = i;
            }
        }

        static Type[] GetAllTypesFromAssembly(Assembly asm) {
#if SILVERLIGHT // ReflectionTypeLoadException
            try {
                return asm.GetTypes();
            } catch (Exception) {
                return new Type[0];
            }
#else
            try {
                return asm.GetTypes();
            } catch (ReflectionTypeLoadException rtlException) {
                return rtlException.Types;
            }
#endif
        }

        internal static Type[] LoadTypesFromAssembly(Assembly asm) {
            if (ScriptDomainManager.Options.PrivateBinding) {
                return GetAllTypesFromAssembly(asm);
            }

            try {
                return asm.GetExportedTypes();
            } catch (NotSupportedException) {
                // GetExportedTypes does not work with dynamic assemblies
            } catch (Exception) {
                // Some type loads may cause exceptions. Unfortunately, there is no way to ask GetExportedTypes
                // for just the list of types that we successfully loaded.
            }

            Type[] allTypes = GetAllTypesFromAssembly(asm);

            Predicate<Type> isPublicTypeDelegate = delegate(Type t) { return t != null && t.IsPublic; };
            return Utils.Array.FindAll(allTypes, isPublicTypeDelegate);
        }

        #endregion

        #region ICustomMembers Members

        public bool TryGetCustomMember(CodeContext context, SymbolId name, out object value) {
            // We lazily load types on demand.  We try to avoid having to iterate
            // all the types in the assembly on each re-load.
            // If a type is in our dictionary we check a 2ndary dictionary which contains
            //      the number of assemblies we knew about when we reflected over it.  If 
            //      this number is less than our current number of assemblies the type is good
            //      as it is to hand out.  If the # of assemblies has changed we need to check
            //      all of our loaded assemblies, and hand out all available types (in case there
            //      are collisions).
            // If we fail to find a name in our dictionary then we will only iterate all of the types
            // if we don't have the full assembly loaded.  This is controllved via our assemblyTypeLoadIndex.

            if (_assemblyTypeLoadIndex != -1 && _assemblyTypeLoadIndex < _packageAssemblies.Count)
                LoadAllTypes();

            lock (_loadLevels) {
                if (_dict.TryGetValue(name, out value)) {
                    if (value == Uninitialized.Instance) return false;

                    int level;
                    if (!_loadLevels.TryGetValue(name, out level) || level >= _packageAssemblies.Count) {
                        return true;
                    }
                }

                if (_assemblyTypeLoadIndex != _packageAssemblies.Count) {
                    value = null;
                    string typeName = _fullName + "." + SymbolTable.IdToString(name);

                    bool fRemovedOld = false;

                    // try and find the type name...
                    for (int i = 0; i < _packageAssemblies.Count; i++) {
                        string arityName = typeName + Utils.Reflection.GenericArityDelimiter;
                        Type[] allTypes = LoadTypesFromAssembly(_packageAssemblies[i]);

                        for (int j = 0; j < allTypes.Length; j++) {
                            Type t = allTypes[j];
                            int nested = t.FullName.IndexOf('+');
                            if (nested != -1) continue;

                            if (t.FullName == typeName ||
                                String.Compare(t.FullName, 0, arityName, 0, arityName.Length) == 0) {

                                if (!fRemovedOld) {
                                    // remove the old entry, we replace it w/ the values
                                    // we get from the full iteration now.
                                    if (_dict.ContainsKey(name)) _dict.Remove(name);

                                    fRemovedOld = true;
                                }
#if !SILVERLIGHT // ComObjectWithTypeInfo
                                ComObjectWithTypeInfo.PublishComInterface(t);
#endif
                                value = SaveType(t);
                            }
                        }
                        _loadLevels[name] = _packageAssemblies.Count;
                    }

                    if (value != null) return true;
                }

                // could have been a namespace, try the dictionary one last time...
                if (_dict.TryGetValue(name, out value)) return true;
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
            if (!_dict.ContainsKey(name)) throw new MissingMemberException(String.Format("could't find {0}", SymbolTable.IdToString(name)));

            _dict[name] = Uninitialized.Instance;
            return true;
        }

        public IList<object> GetCustomMemberNames(CodeContext context) {
            LoadAllTypes();

            List<object> res = new List<object>(((IDictionary<object, object>)_dict).Keys);
            res.Sort();
            return res;
        }

        public IDictionary<object, object> GetCustomMemberDictionary(CodeContext context) {
            LoadAllTypes();

            return (IDictionary<object, object>)_dict;
        }

        #endregion

        #region Private Implementation Details

        private static string GetCoreTypeName(Type type) {
            string name = type.Name;
            if (type.IsGenericType) {
                int backtick = name.IndexOf(Utils.Reflection.GenericArityDelimiter);
                if (backtick != -1) return name.Substring(0, backtick);
            }
            return name;
        }

        #endregion
    }
}
