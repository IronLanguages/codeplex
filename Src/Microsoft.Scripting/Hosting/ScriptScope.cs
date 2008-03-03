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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting;
using System.Threading;

using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Runtime;
using System.ComponentModel;

namespace Microsoft.Scripting.Hosting {
    /// <summary>
    /// A ScriptScope is a unit of execution for code.  It consists of a global Scope which
    /// all code executes in.  A ScriptScope can have an arbitrary initializer and arbitrary
    /// reloader. 
    /// 
    /// ScriptScope is not thread safe. Host should either lock when multiple threads could 
    /// access the same module or should make a copy for each thread.
    /// </summary>
    public sealed class ScriptScope 
#if !SILVERLIGHT
        : MarshalByRefObject
#endif
    {
        private readonly Scope/*!*/ _scope;
        private readonly ScriptEngine/*!*/ _engine;

        internal ScriptScope(ScriptEngine/*!*/ engine, Scope/*!*/ scope) {
            Assert.NotNull(engine);
            Assert.NotNull(scope);
            
            _scope = scope;
            _engine = engine;
        }

        internal Scope/*!*/ Scope {
            get {
                return _scope;
            }
        }

#if !SILVERLIGHT
        public ObjectHandle LookupVariableAndWrap(string name) {
            return new ObjectHandle(LookupVariable(name));
        }
#endif

        public T GetVariable<T>(string/*!*/ name) {
            return _engine.Operations.ConvertTo<T>(_engine.GetVariable(this, name));
        }

        /// <summary>
        /// Trys to lookup the provided name in the current scope.
        /// </summary>
        public bool TryGetVariable(string name, out object value) {
            return _scope.TryGetName(SymbolTable.StringToId(name), out value);
        }

        /// <summary>
        /// Attempts to lookup the provided name in this scope or any outer scope.   
        /// </summary>
        public bool TryLookupVariable(string name, out object value) {
            return _scope.TryLookupName(SymbolTable.StringToId(name), out value);
        }

        /// <summary>
        /// Attempts to lookup the provided name in this scope or any outer scope.   If the
        /// name is not defined MissingMemberException is thrown.
        /// </summary>
        public object LookupVariable(string name) {
            return _scope.LookupName(SymbolTable.StringToId(name));
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
            return _scope.ContainsName(SymbolTable.StringToId(name));
        }

        /// <summary>
        /// Attemps to remove the provided name from this scope
        /// </summary> 
        public bool RemoveVariable(string name) {
            return _scope.TryRemoveName(SymbolTable.StringToId(name));
        }

        /// <summary>
        /// Removes all members from the dictionary and any context-sensitive dictionaries.
        /// </summary>
        public void ClearVariables() {
            _scope.Clear();
        }

        public IEnumerable<KeyValuePair<string, object>> Items {
            get {
                foreach (KeyValuePair<SymbolId, object> kvp in _scope.Items) {
                    yield return new KeyValuePair<string, object>(SymbolTable.IdToString(kvp.Key), kvp.Value);
                }
            }
        }

        // dynamic behavior of the scope:

        [SpecialName, EditorBrowsable(EditorBrowsableState.Never)]
        public object GetCustomMember(CodeContext context, string name) {
            return _scope.GetCustomMember(context, name);
        }

        [SpecialName, EditorBrowsable(EditorBrowsableState.Never)]
        public void SetMemberAfter(string name, object value) {
            _scope.SetMemberAfter(name, value);
        }

        [SpecialName, EditorBrowsable(EditorBrowsableState.Never)]
        public bool DeleteMember(CodeContext context, string name) {
            return _scope.DeleteMember(context, name);
        }
    }
}
