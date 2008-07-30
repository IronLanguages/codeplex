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
using System.Runtime.Remoting;
using System.Runtime.Serialization;
using System.Scripting;
using System.Scripting.Runtime;
using System.Scripting.Utils;

namespace Microsoft.Scripting.Hosting {
    /// <summary>
    /// A ScriptScope is a unit of execution for code.  It consists of a global Scope which
    /// all code executes in.  A ScriptScope can have an arbitrary initializer and arbitrary
    /// reloader. 
    /// 
    /// ScriptScope is not thread safe. Host should either lock when multiple threads could 
    /// access the same module or should make a copy for each thread.
    ///
    /// Hosting API counterpart for <see cref="Scope"/>.
    /// </summary>
#if SILVERLIGHT
    public sealed class ScriptScope {
#else
    [DebuggerTypeProxy(typeof(ScriptScope.DebugView))]
    public sealed class ScriptScope : MarshalByRefObject {
#endif
        private readonly Scope _scope;
        private readonly ScriptEngine _engine;

        internal ScriptScope(ScriptEngine engine, Scope scope) {
            Assert.NotNull(engine);
            Assert.NotNull(scope);

            _scope = scope;
            _engine = engine;
        }

        internal Scope Scope {
            get { return _scope; }
        }

        internal bool CanExecuteCode {
            get { return _engine.LanguageContext.CanCreateSourceCode; }
        }

        /// <summary>
        /// Gets an engine for the langauge associated with this scope.
        /// Returns invariant engine if the scope is language agnostic.
        /// </summary>
        public ScriptEngine Engine {
            get { return _engine; }
        }

        #region Code Execution (for convenience)

        /// <summary>
        /// Executes specified code against the scope using the engine of the language associated with the scope.
        /// </summary>
        /// <exception cref="NotSupportedException">No language is associated with the scope.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="code"/> is a <c>null</c> reference.</exception>
        public object Execute(string code) {
            ContractUtils.RequiresNotNull(code, "code");
            if (!CanExecuteCode) throw new NotSupportedException("Cannot execute code on language agnostic scope");
            return _engine.LanguageContext.CreateSnippet(code).Execute(_scope);
        }

        /// <summary>
        /// Executes specified code against the scope using the engine of the language associated with the scope.
        /// Converts the result to the specified type using the conversion that the language associated with the scope defines.
        /// </summary>
        /// <exception cref="NotSupportedException">No language is associated with the scope.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="code"/> is a <c>null</c> reference.</exception>
        public T Execute<T>(string code) {
            object result = Execute(code);
            return _engine.Operations.ConvertTo<T>(result);
        }

        /// <summary>
        /// Executes content of the specified physical file against the scope using the engine of the language associated with the scope.
        /// Converts the result to the specified type using the conversion that the language associated with the scope defines.
        /// </summary>
        /// <exception cref="NotSupportedException">No language is associated with the scope.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is a <c>null</c> reference.</exception>
        public void IncludeFile(string path) {
            ContractUtils.RequiresNotNull(path, "path");
            if (!CanExecuteCode) throw new NotSupportedException("Cannot execute code on language agnostic scope");

            _engine.LanguageContext.CreateFileUnit(path).Execute(_scope);
        }

        #endregion

        /// <summary>
        /// Gets a value stored in the scope under the given name.
        /// </summary>
        /// <exception cref="MissingMemberException">The specified name is not defined in the scope.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is a <c>null</c> reference.</exception>
        public object GetVariable(string name) {
            return _scope.LookupName(_engine.LanguageContext, SymbolTable.StringToId(name));
        }

        /// <summary>
        /// Gets a value stored in the scope under the given name.
        /// Converts the result to the specified type using the conversion that the language associated with the scope defines.
        /// If no language is associated with the scope, the default CLR conversion is attempted.
        /// </summary>
        /// <exception cref="MissingMemberException">The specified name is not defined in the scope.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is a <c>null</c> reference.</exception>
        public T GetVariable<T>(string name) {
            return _engine.Operations.ConvertTo<T>(_engine.GetVariable(this, name));
        }

        /// <summary>
        /// Tries to get a value stored in the scope under the given name.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is a <c>null</c> reference.</exception>
        public bool TryGetVariable(string name, out object value) {
            return _scope.TryGetName(_engine.LanguageContext, SymbolTable.StringToId(name), out value);
        }

        /// <summary>
        /// Sets the name to the specified value.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is a <c>null</c> reference.</exception>
        public void SetVariable(string name, object value) {
            _scope.SetName(SymbolTable.StringToId(name), value);
        }

#if !SILVERLIGHT
        /// <summary>
        /// Gets a handle for a value stored in the scope under the given name.
        /// </summary>
        /// <exception cref="MissingMemberException">The specified name is not defined in the scope.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is a <c>null</c> reference.</exception>
        public ObjectHandle GetVariableHandle(string name) {
            return new ObjectHandle(GetVariable(name));
        }

        /// <summary>
        /// Tries to get a handle for a value stored in the scope under the given name.
        /// Returns <c>true</c> if there is such name, <c>false</c> otherwise. 
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is a <c>null</c> reference.</exception>
        public bool TryGetVariableHandle(string name, out ObjectHandle handle) {
            object value;
            if (TryGetVariable(name, out value)) {
                handle = new ObjectHandle(value);
                return true;
            } else {
                handle = null;
                return false;
            }
        }

        /// <summary>
        /// Sets the name to the specified value.
        /// </summary>
        /// <exception cref="SerializationException">
        /// The value held by the handle isn't from the scope's app-domain and isn't serializable or MarshalByRefObject.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> or <paramref name="handle"/> is a <c>null</c> reference.</exception>
        public void SetVariable(string name, ObjectHandle handle) {
            ContractUtils.RequiresNotNull(handle, "handle");
            SetVariable(name, handle.Unwrap());
        }
#endif

        /// <summary>
        /// Determines if this context or any outer scope contains the defined name.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is a <c>null</c> reference.</exception>
        public bool ContainsVariable(string name) {
            return _scope.ContainsName(_engine.LanguageContext, SymbolTable.StringToId(name));
        }

        /// <summary>
        /// Removes the variable of the given name from this scope.
        /// </summary> 
        /// <returns><c>true</c> if the value existed in the scope before it has been removed.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is a <c>null</c> reference.</exception>
        public bool RemoveVariable(string name) {
            return _scope.TryRemoveName(_engine.LanguageContext, SymbolTable.StringToId(name));
        }

        /// <summary>
        /// Removes all values from the scope.
        /// </summary>
        public void ClearVariables() {
            _scope.Clear();
        }

        /// <summary>
        /// Gets enumeration of variable names stored in the scope.
        /// </summary>
        public IEnumerable<string> VariableNames {
            get {
                foreach (KeyValuePair<SymbolId, object> kvp in _scope.Items) {
                    yield return SymbolTable.IdToString(kvp.Key);
                }
            }
        }

        /// <summary>
        /// Gets enumeration of variable names and their values stored in the scope.
        /// </summary>
        public IEnumerable<KeyValuePair<string, object>> Items {
            get {
                foreach (KeyValuePair<SymbolId, object> kvp in _scope.Items) {
                    yield return new KeyValuePair<string, object>(SymbolTable.IdToString(kvp.Key), kvp.Value);
                }
            }
        }

        #region DebugView
#if !SILVERLIGHT
        internal sealed class DebugView {
            private readonly ScriptScope _scope;

            public DebugView(ScriptScope scope) {
                Assert.NotNull(scope);
                _scope = scope;
            }

            public ScriptEngine Language {
                get { return _scope._engine; }
            }

            public System.Collections.Hashtable Variables {
                get {
                    System.Collections.Hashtable result = new System.Collections.Hashtable();
                    foreach (KeyValuePair<string, object> variable in _scope.Items) {
                        result[variable.Key] = variable.Value;
                    }
                    return result;
                }
            }
        }
#endif
        #endregion
    }
}
