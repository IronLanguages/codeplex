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


using System.Reflection;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Actions {
    /// <summary>
    /// Represents the top reflected package which contains extra information such as
    /// all the assemblies loaded and the built-in modules.
    /// </summary>
    public class TopNamespaceTracker : NamespaceTracker {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")] // TODO: fix
        private int _lastDiscovery = 0;
        private readonly ScriptDomainManager _manager;

        public TopNamespaceTracker(ScriptDomainManager manager)
            : base(null) {
            ContractUtils.RequiresNotNull(manager, "manager");
            SetTopPackage(this);

            _manager = manager;
        }

        #region Public API Surface

        /// <summary>
        /// returns the package associated with the specified namespace and
        /// updates the associated module to mark the package as imported.
        /// </summary>
        public NamespaceTracker TryGetPackage(string name) {
            return TryGetPackage(SymbolTable.StringToId(name));
        }

        public NamespaceTracker TryGetPackage(SymbolId name) {
            NamespaceTracker pm = TryGetPackageAny(name) as NamespaceTracker;
            if (pm != null) {
                return pm;
            }
            return null;
        }

        public MemberTracker TryGetPackageAny(string name) {
            return TryGetPackageAny(SymbolTable.StringToId(name));
        }

        public MemberTracker TryGetPackageAny(SymbolId name) {
            MemberTracker ret;
            if (TryGetValue(name, out ret)) {
                return ret;
            }
            return null;
        }

        public MemberTracker TryGetPackageLazy(SymbolId name) {
            lock (this) {
                MemberTracker ret;
                if (_dict.TryGetValue(SymbolTable.IdToString(name), out ret)) {
                    return ret;
                }
                return null;
            }
        }

        /// <summary>
        /// Ensures that the assembly is loaded
        /// </summary>
        /// <param name="assem"></param>
        /// <returns>true if the assembly was loaded for the first time. 
        /// false if the assembly had already been loaded before</returns>
        public bool LoadAssembly(Assembly assem) {
            ContractUtils.RequiresNotNull(assem, "assem");

            lock (this) {
                if (_packageAssemblies.Contains(assem)) {
                    // The assembly is already loaded. There is nothing more to do
                    return false;
                }

                _packageAssemblies.Add(assem);
                UpdateId();
#if !SILVERLIGHT // ComObject
                Microsoft.Scripting.Com.ComObjectWithTypeInfo.PublishComTypes(assem);
#endif
            }

            return true;
        }

        #endregion

        protected override void LoadNamespaces() {
            lock (this) {
                for (int i = _lastDiscovery; i < _packageAssemblies.Count; i++) {
                    DiscoverAllTypes(_packageAssemblies[i]);
                }
                _lastDiscovery = _packageAssemblies.Count;
            }
        }

        public ScriptDomainManager DomainManager {
            get {
                return _manager;
            }
        }
    }
}
