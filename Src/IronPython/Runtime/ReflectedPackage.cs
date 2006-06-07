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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

using System.Reflection;

using IronPython.Modules;

namespace IronPython.Runtime {
    /// <summary>
    /// Represents the top reflected package which contains extra information such as
    /// all the assemblies loaded and the built-in modules.
    /// </summary>
    internal class TopReflectedPackage : ReflectedPackage {
        private Dictionary<Assembly, bool> loadedAssemblies = new Dictionary<Assembly, bool>();
        private Dictionary<string, Type> builtins = new Dictionary<string, Type>();
        private int initialized;

        #region Public API Surface

        /// <summary>
        /// returns the package associated with the specified namespace and
        /// updates the associated module to mark the package as imported.
        /// </summary>
        public PythonModule TryGetPackage(SystemState state, string name) {
            PythonModule pm = TryGetPackageAny(state, name) as PythonModule;
            if (pm != null) {
                pm.PackageImported = true;
                return pm;
            }
            return null;
        }

        public object TryGetPackageAny(SystemState state, string name) {
            Initialize(state);
            object ret;            
            if (__dict__.TryGetValue(SymbolTable.StringToId(name), out ret)) {
                return ret;
            }
            return null;
        }

        public bool LoadAssembly(SystemState state, Assembly assem) {
            return LoadAssembly(state, assem, true);
        }

        public bool LoadAssembly(SystemState state, Assembly assem, bool lazy) {
            bool loaded;
            if (loadedAssemblies.TryGetValue(assem, out loaded) && lazy) {
                return false;
            }

            if (!loaded) {
                foreach (PythonModuleAttribute pma in assem.GetCustomAttributes(typeof(PythonModuleAttribute), false)) {
                    builtins.Add(pma.name, pma.type);
                }
            }

            foreach (Type type in assem.GetExportedTypes()) {
                //  Skip nested types. They get loaded during parent type initalization.
                //
                if (type.IsNested) {
                    continue;
                }

                // save all the namespaces, types will be lazily initialized
                // on demand in GetAttr
                ReflectedPackage pkg = GetOrMakeTopPackage(state, assem, type.Namespace);

                if (!loaded) {
                    if (String.IsNullOrEmpty(type.Namespace)) {
                        // but we need to save top-level types immediately
                        ComObject.AddType(type.GUID, type);

                        pkg.SaveType(type);
                    }
                } else {
                    // doing a non-lazy reload...  force all types to get loaded
                    pkg.LoadAllTypes();
                }
            }

            // Assembly was loaded
            loadedAssemblies[assem] = true;

            return true;
        }

        public void Initialize(SystemState state) {
            if (initialized != 0) return;
            if (System.Threading.Interlocked.Exchange(ref initialized, 1) == 0) {

                // add mscorlib
                state.ClrModule.AddReferenceByName(typeof(string).Assembly.FullName);
                // add system.dll
                state.ClrModule.AddReferenceByName(typeof(System.Diagnostics.Debug).Assembly.FullName);

                InitializeBuiltins(state);
            }
        }

        public Dictionary<string, Type> Builtins {
            get {
                return builtins;
            }
        }

        public Dictionary<Assembly, bool> LoadedAssemblies {
            get {
                return loadedAssemblies;
            }
        }

        #endregion

        #region Private Implementation Details

        private ReflectedPackage GetOrMakeTopPackage(SystemState state, Assembly assm, string ns) {
            ReflectedPackage ret = this;
            if (ns != null) {

                string[] pieces = ns.Split('.');
                for (int i = 0; i < pieces.Length; i++) {
                    if (!ret.packageAssemblies.Contains(assm)) ret.packageAssemblies.Add(assm);
                    ret = ret.GetOrMakePackage(state, String.Join(".", pieces, 0, i + 1), pieces[i]);
                }
            }

            if (!ret.packageAssemblies.Contains(assm)) ret.packageAssemblies.Add(assm);
            return ret;
        }

        private void InitializeBuiltins(SystemState state) {
            LoadAssembly(state, typeof(Builtin).Assembly);
            if (Environment.OSVersion.Platform == PlatformID.Unix) {
                // we make our nt package show up as a posix package
                // on unix platforms.  Because we build on top of the 
                // CLI for all file operations we should be good from
                // there, but modules that check for the presence of
                // names (e.g. os) will do the right thing.
                builtins.Add("posix", typeof(PythonNT));
                builtins.Remove("nt");
            }

            state.modules["sys"] = state;
            state.modules["__builtin__"] = Importer.MakePythonModule(state, "__builtin__", TypeCache.Builtin);

            state.builtin_module_names = Tuple.Make(builtins.Keys);            
        }

        #endregion

    }

    /// <summary>
    /// ReflectedPackages represent a CLS namespace.  ReflectedPackages aren't
    /// exposed to the user.  Instead PythonModule holds a reference to the 
    /// ReflectedPackage and the two share the same dictionary for updating
    /// what is present within the namespace.
    /// </summary>
    internal class ReflectedPackage : ICustomAttributes {
        internal IAttributesDictionary __dict__;
        internal List<Assembly> packageAssemblies = new List<Assembly>();

        private IDictionary<SymbolId, int> loadLevels;
        private string fullName;
        private int assemblyTypeLoadIndex = -1;

        #region Protected API Surface

        protected ReflectedPackage() {
            __dict__ = new FieldIdDict();
            loadLevels = new Dictionary<SymbolId, int>();
        }

        #endregion

        #region Internal API Surface

        internal DynamicType SaveType(Type type) {
            string name = GetCoreTypeName(type);
            object existingType;

            // if there's no collisions we just save the type
            if (!__dict__.TryGetValue(SymbolTable.StringToId(name), out existingType)) {
                DynamicType ret;
                __dict__[SymbolTable.StringToId(name)] = ret = Ops.GetDynamicTypeFromType(type);
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
                    __dict__[SymbolTable.StringToId(name)] = tc = tc.CloneWithNewBase(type);
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
                ReflectedType rt = existingType as ReflectedType;
                Debug.Assert(rt != null);

                if (rt.type.ContainsGenericParameters) {
                    // existing type has generics, append it.
                    tc = new TypeCollision(type);
                    __dict__[SymbolTable.StringToId(name)] = tc;
                    tc.UpdateType(rt.type);
                } else if (type.ContainsGenericParameters) {
                    // new type has generics, append it.
                    tc = new TypeCollision(rt.type);
                    __dict__[SymbolTable.StringToId(name)] = tc;
                    tc.UpdateType(type);
                } else {
                    // neither type has generics, replace the old
                    // non-generic type w/ the new non-generic type.
                    __dict__[SymbolTable.StringToId(name)] = Ops.GetDynamicTypeFromType(type);
                }
            }
            return tc;
        }

        internal ReflectedPackage GetOrMakePackage(SystemState state, string fullName, string name) {
            object ret;
            if (__dict__.TryGetValue(SymbolTable.StringToId(name), out ret)) {
                // if it's not a module we'll wipe it out below, eg def System(): pass then 
                // import System will result in the namespace being visible.
                PythonModule pm = ret as PythonModule;
                ReflectedPackage res;
                do{
                    res = pm.InnerModule as ReflectedPackage;
                    if (res != null) return res;

                    pm = pm.InnerModule as PythonModule;
                } while (pm != null);
            }

            return MakePackage(state, fullName, name);
        }

        internal ReflectedPackage MakePackage(SystemState state, string fullName, string name) {
            ReflectedPackage rp = new ReflectedPackage();
            object mod;
            PythonModule pmod;

            if (!Importer.TryGetExistingModule(state, name, out mod)) {
                // no collisions (yet), create a new module for the package.
                pmod = new PythonModule(name, new Dict(), state);
                pmod.PackageImported = true;
            } else {
                // there's already a module by this name.  We'll just
                // set the InnerModule but not make it visible until
                // the user does an import (and we set PackageImported).
                pmod = mod as PythonModule;
                System.Diagnostics.Debug.Assert(pmod != null);
            }

            rp.fullName = fullName;
            pmod.InnerModule = rp;
            __dict__[SymbolTable.StringToId(name)] = pmod;
            return rp;
        }

        internal void LoadAllTypes() {
            // if new assemblies are loaded we need to re-load their types...
            if (assemblyTypeLoadIndex == packageAssemblies.Count) return;

            for (int i = assemblyTypeLoadIndex + 1; i < packageAssemblies.Count; i++) {
                Type[] types = packageAssemblies[i].GetExportedTypes();
                for (int j = 0; j < types.Length; j++) {
                    if (types[j].IsNested) {
                        continue;
                    }

                    ComObject.AddType(types[j].GUID, types[j]);

                    // only load types that are in our namespace
                    // but not in a child namespace
                    if (fullName.Length < types[j].FullName.Length &&
                        String.Compare(fullName, 0, types[j].FullName, 0, fullName.Length) == 0 &&
                        types[j].FullName.IndexOf('.', fullName.Length + 1) == -1) {
                        SaveType(types[j]);
                    }
                }

                assemblyTypeLoadIndex = i;
            }
        }

        #endregion        

        #region ICustomAttributes Members

        public bool TryGetAttr(ICallerContext context, SymbolId name, out object value) {
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

            if (assemblyTypeLoadIndex != -1 && assemblyTypeLoadIndex < packageAssemblies.Count)
                LoadAllTypes();

            if (__dict__.TryGetValue(name, out value)) {
                int level;
                if (!loadLevels.TryGetValue(name, out level) || level >= packageAssemblies.Count) {
                    return true;
                }
            }

            if (assemblyTypeLoadIndex != packageAssemblies.Count) {
                value = null;
                string typeName = fullName + "." + SymbolTable.IdToString(name);

                bool fRemovedOld = false;

                // try and find the type name...
                for (int i = 0; i < packageAssemblies.Count; i++) {
                    string arityName = typeName + '`';  
                    Type[] allTypes = packageAssemblies[i].GetTypes();

                    for (int j = 0; j < allTypes.Length; j++) {
                        Type t = allTypes[j];
                        int nested = t.FullName.IndexOf('+');
                        if (nested != -1) continue;

                        object [] attrs = t.GetCustomAttributes(typeof(PythonTypeAttribute), false);
                        if ((attrs.Length>0 && ((PythonTypeAttribute)attrs[0]).name == typeName) ||
                            t.FullName == typeName ||
                            String.Compare(t.FullName, 0, arityName, 0, arityName.Length) == 0) {

                            if (!fRemovedOld) {
                                // remove the old entry, we replace it w/ the values
                                // we get from the full iteration now.
                                if (__dict__.ContainsKey(name)) __dict__.Remove(name);

                                fRemovedOld = true;
                            }
                            ComObject.AddType(t.GUID, t);

                            value = SaveType(t);
                        }
                    }
                    loadLevels[name] = packageAssemblies.Count;
                }

                if (value != null) return true;
            }

            // could have been a namespace, try the dictionary one last time...
            if (__dict__.TryGetValue(name, out value)) return true;

            value = null;
            return false;
        }

        public void SetAttr(ICallerContext context, SymbolId name, object value) {
            __dict__[name] = value;
        }

        public void DeleteAttr(ICallerContext context, SymbolId name) {
            __dict__.Remove(name);
        }

        public List GetAttrNames(ICallerContext context) {
            LoadAllTypes();

            return new List(((IDictionary<object, object>)__dict__).Keys);
        }

        public IDictionary<object, object> GetAttrDict(ICallerContext context) {
            LoadAllTypes();

            return (IDictionary<object, object>)__dict__;
        }

        #endregion

        #region Private Implementation Details

        private static string GetCoreTypeName(Type type) {
            string name = type.Name;
            if (type.IsGenericType) {
                int backtick = name.IndexOf('`');
                if (backtick != -1) return name.Substring(0, backtick);
            }
            return name;
        }

        #endregion
    }    
}
