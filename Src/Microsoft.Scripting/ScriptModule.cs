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
using System.Threading;
using System.Diagnostics;

using Microsoft.Scripting.Internal.Generation;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Hosting;
using System.Runtime.Remoting;

namespace Microsoft.Scripting {
    [Flags]
    public enum CodeContextAttributes {
        None = 0,
        ShowCls = 0x01,
    }

    public interface IScriptModule : IRemotable {
        string ModuleName { get; }
        string FileName { get; set; } // TODO: setter?

        // code execution:
        void Execute();
        void Reload();

        // module variables:
        bool TryGetVariable(string name, out object value);
        void SetVariable(string name, object value);
        bool TryLookupVariable(string name, out object value);
        object LookupVariable(string name); // TODO: rename to GetVariable
        bool VariableExists(string name);
        bool RemoveVariable(string name);
        void ClearVariables();

        // compiler options:
        CompilerOptions GetCompilerOptions(IScriptEngine engine);

#if !SILVERLIGHT
        IObjectHandle LookupVariableAndWrap(string name);
        // TODO: void SetVariable(string name, IObjectHandle value);
#endif
    }

    /// <summary>
    /// A ScriptModule is a unit of execution for code.  It consists of a global Scope which
    /// all code executes in.  A ScriptModule can have an arbitrary initializer and arbitrary
    /// reloader.
    /// </summary>
    public sealed class ScriptModule : IScriptModule, ICustomMembers, ILocalObject {
        private Scope _scope;
        private ICustomMembers _innerMod;
        private ScriptCode[] _codeBlocks;
        private string _name, _fileName;
        private bool _packageImported;
        private Dictionary<object, object> _langData;

        private static DynamicType ModuleType;

        /// <summary>
        /// Creates a ScriptModule consisting of multiple ScriptCode blocks (possibly with each
        /// ScriptCode block belonging to a different language). 
        /// Can ONLY be called from ScriptDomainManager.CreateModule factory (due to host notification).
        /// </summary>
        internal ScriptModule(string name, Scope scope, ScriptCode[] codeBlocks) {
            Utils.Assert.NotNull(name, scope, codeBlocks);
            Utils.Assert.NotNull(codeBlocks);

            _codeBlocks = Utils.Array.Copy(codeBlocks);
            _name = name;
            _scope = scope;
        }

        /// <summary>
        /// Perform one-time initialization on the module.
        /// </summary>
        public void Execute() {
            Debug.Assert(Scope != null);

            for (int i = 0; i < _codeBlocks.Length; i++) {
                _codeBlocks[i].Run(_scope);
            }
        }

        /// <summary>
        /// Reloads a module from disk and executes the new module body.
        /// </summary>
        public void Reload() {
            if (_codeBlocks.Length > 0) {
                ScriptCode[] newCode = new ScriptCode[_codeBlocks.Length];

                // get the new ScriptCode's...
                for (int i = 0; i < _codeBlocks.Length; i++) {
                    newCode[i] = _codeBlocks[i].LanguageContext.Reload(_codeBlocks[i], this);
                }

                // run the new code in the existing scope
                // we don't clear the scope before doing this
                _codeBlocks = newCode;

                Execute();
            }
        }

        #region Properties

        /// <summary>
        /// Event fired when a module changes.
        /// </summary>
        public event EventHandler<ModuleChangeEventArgs> ModuleChanged;

        /// <summary>
        /// Gets the context in which this module executes.
        /// </summary>
        public Scope Scope {
            get {
                return _scope;
            }
        }

        /// <summary>
        /// Gets or sets the name of the module.
        /// </summary>
        public string ModuleName {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Gets or sets the filename of the module.
        /// </summary>
        public string FileName {
            get { return _fileName; }
            set { _fileName = value; }
        }

        public ICustomMembers InnerModule {
            get {
                return _innerMod;
            }
            set {
                _innerMod = value;
            }
        }

        /// <summary>
        /// True if the package has been imported into this module, otherwise false.
        /// </summary>
        public bool PackageImported {
            get {
                return _packageImported;
            }
            set {
                _packageImported = value;
            }
        }

        /// <summary>
        /// Called by the base class to fire the module change event when the
        /// module has been modified.
        /// </summary>
        private void OnModuleChange(ModuleChangeEventArgs e) {
            EventHandler<ModuleChangeEventArgs> handler = ModuleChanged;
            if (handler != null) {
                handler(this, e);
            }
        }

        #endregion

        #region ICustomMembers Members

        private static void EnsureModType() {
            // Temporary work around until we build types in Microsoft.Scripting.
            if (ModuleType == null) ModuleType = DynamicType.GetDynamicType(typeof(ScriptModule));
        }

        public bool TryGetCustomMember(CodeContext context, SymbolId name, out object value) {
            return TryGetBoundCustomMember(context, name, out value);
        }

        public bool TryGetBoundCustomMember(CodeContext context, SymbolId name, out object value) {
            if (Scope.TryGetName(context.LanguageContext, name, out value)) {
                if (value == Uninitialized.Instance) return false;

                IContextAwareMember icaa = value as IContextAwareMember;
                if (icaa == null || icaa.IsVisible(context, null)) { /* TODO: owner type*/
                    return true;
                }
                value = null;
            }

            if (PackageImported && InnerModule.TryGetBoundCustomMember(context, name, out value)) {
                return true;
            }

            EnsureModType();
            return ModuleType.TryGetBoundMember(context, this, name, out value);
        }

        public void SetCustomMember(CodeContext context, SymbolId name, object value) {
            DynamicTypeSlot dts;
            EnsureModType();
            if (ModuleType.TryLookupSlot(context, name, out dts)) {
                if (!ModuleType.TrySetMember(context, this, name, value)) {
                    throw new ArgumentTypeException(String.Format("cannot set {0}", SymbolTable.IdToString(name)));
                }
            } else {
                OnModuleChange(new ModuleChangeEventArgs(name, ModuleChangeType.Set, value));

                Scope.SetName(name, value);
            }
        }

        public bool DeleteCustomMember(CodeContext context, SymbolId name) {
            bool isDeleted = true;
            if (Scope.TryRemoveName(context.LanguageContext, name)) {
                object dummy;
                if (PackageImported && InnerModule.TryGetBoundCustomMember(context, name, out dummy))
                    isDeleted = InnerModule.DeleteCustomMember(context, name);

                OnModuleChange(new ModuleChangeEventArgs(name, ModuleChangeType.Delete));

                return isDeleted;
            } else if (PackageImported) {
                isDeleted = InnerModule.DeleteCustomMember(context, name);
            }

            EnsureModType();
            if (!ModuleType.TryDeleteMember(context, this, name)) {
                throw new ArgumentTypeException(String.Format("cannot delete {0}", SymbolTable.IdToString(name)));
            }
            return isDeleted;
        }

        public IList<object> GetCustomMemberNames(CodeContext context) {
            List<object> ret;
            if (!context.LanguageContext.ShowCls) {
                ret = new List<object>();
                foreach (KeyValuePair<object, object> kvp in Scope.GetAllItems(context.LanguageContext)) {
                    IContextAwareMember icaa = kvp.Value as IContextAwareMember;
                    if (icaa == null || icaa.IsVisible(context, null)) {  /* TODO: Owner type */
                        if (kvp.Key is SymbolId) {
                            ret.Add(SymbolTable.IdToString((SymbolId)kvp.Key));
                        } else {
                            ret.Add(kvp.Key);
                        }
                    }
                }
            } else {
                ret = new List<object>(Scope.GetAllKeys(context.LanguageContext));
            }

            if (PackageImported) {
                foreach (object o in InnerModule.GetCustomMemberNames(context)) {
                    string strName = o as string;
                    if (strName == null) continue;

                    if (!Scope.ContainsName(context.LanguageContext, SymbolTable.StringToId(strName))) ret.Add(o);
                }
            }

            return ret;
        }

        public IDictionary<object, object> GetCustomMemberDictionary(CodeContext context) {
            return (IDictionary<object, object>)Scope.Dict;
        }

        #endregion

        #region IScriptModule Members

#if !SILVERLIGHT
        RemoteWrapper ILocalObject.Wrap() {
            return new RemoteScriptModule(this);
        }

        public IObjectHandle LookupVariableAndWrap(string name) {
            return new ObjectHandle(LookupVariable(name));
        }
#endif

        public CompilerOptions GetCompilerOptions(IScriptEngine engine) {
            if (engine == null) throw new ArgumentNullException("engine");
            return engine.GetModuleCompilerOptions(this);
        }

        /// <summary>
        /// Trys to lookup the provided name in the current scope.
        /// </summary>
        public bool TryGetVariable(string name, out object value) {
            return _scope.TryGetName(InvariantContext.Instance, SymbolTable.StringToId(name), out value);
        }

        /// <summary>
        /// Attempts to lookup the provided name in this scope or any outer scope.   
        /// </summary>
        public bool TryLookupVariable(string name, out object value) {
            return _scope.TryLookupName(InvariantContext.Instance, SymbolTable.StringToId(name), out value);
        }

        /// <summary>
        /// Attempts to lookup the provided name in this scope or any outer scope.   If the
        /// name is not defined MissingMemberException is thrown.
        /// </summary>
        public object LookupVariable(string name) {
            return _scope.LookupName(InvariantContext.Instance, SymbolTable.StringToId(name));
        }

        /// <summary>
        /// Sets the name to the specified value for the current context.
        /// </summary>
        public void SetVariable(string name, object value) {
            _scope.SetName(SymbolTable.StringToId(name), value);
        }

        /// <summary>
        /// Determines if this context or any outer scope contains the defined name.
        /// </summary>
        public bool VariableExists(string name) {
            return _scope.ContainsName(InvariantContext.Instance, SymbolTable.StringToId(name));
        }

        /// <summary>
        /// Attemps to remove the provided name from this scope
        /// </summary> 
        public bool RemoveVariable(string name) {
            return _scope.TryRemoveName(InvariantContext.Instance, SymbolTable.StringToId(name));
        }

        /// <summary>
        /// Removes all members from the dictionary and any context-sensitive dictionaries.
        /// </summary>
        public void ClearVariables() {
            _scope.Clear();
        }

        #endregion      

        /// <summary>
        /// Provides access to get per-language data needed to track state about the ScriptModule.
        /// 
        /// Callers should use unique objects to ensure that their keys do not collide with
        /// other languages.
        /// </summary>
        /// <param name="key">The key for the language specific data.  Callers should use unique objects to ensure that their keys do not collide with other languages.</param>
        /// <param name="value">The value stored with the key if the return value is true</param>
        /// <exception cref="System.ArgumentNullException">The key parameter was null</exception>
        public bool TryGetLanguageData(object key, out object value) {
            if (key == null) throw new ArgumentNullException("key");

            if (_langData != null) {
                lock (_langData) {
                    return _langData.TryGetValue(key, out value);
                }
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Provides access to set per-language data needed to track state about the ScriptModule.
        /// </summary>
        /// <param name="key">The key for the language specific data.  Callers should use unique objects to ensure that their keys do not collide with other languages.</param>
        /// <param name="value">The value to be associated with the key</param>
        /// <exception cref="System.ArgumentNullException">The key parameter was null</exception>
        public void SetLanguageData(object key, object value) {
            if (key == null) throw new ArgumentNullException("key");

            if (_langData == null) {
                Interlocked.CompareExchange<Dictionary<object, object>>(ref _langData, new Dictionary<object, object>(), null);
            }

            lock (_langData) {
                _langData[key] = value;
            }
        }

        /// <summary>
        /// Provides the ability to remove per-language data needed to track state about the ScriptModule.
        /// </summary>
        /// <param name="key">The key for the language specific data.  Callers should use unique objects to ensure that their keys do not collide with other languages.</param>
        /// <returns>true if the key was removed, false otherwise.</returns>
        public bool RemoveLanguageData(object key) {
            if (_langData == null) return false;

            lock (_langData) {
                return _langData.Remove(key);
            }
        }
    }
}
