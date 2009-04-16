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
using Microsoft.Linq.Expressions;
using Microsoft.Scripting.Utils;
using System.Threading;

namespace Microsoft.Scripting.Runtime {

    /// <summary>
    /// Represents a context of execution.  A context of execution has a set of variables
    /// associated with it (its dictionary) and a parent context.  
    /// 
    /// When looking up a name from a context first the local context is searched.  If the
    /// name is not found there the name lookup will be done against the parent context.
    /// 
    /// Scopes, like IAttrbibuteCollections, support both being indexed by SymbolId for fast
    /// access as well as being indexed by object.  The preferred access is via SymbolId and
    /// object access is provided for languages which require additional semantics.  All
    /// features supported for feature IDs are also supported for objects (e.g. context-sentsitivity
    /// and attributes) but the object API does not contain all the same sets of overloads provided
    /// for convenience.
    /// 
    /// TODO: Thread safety
    /// </summary>
    public class Scope {
        private ScopeExtension[] _extensions; // resizable
        private IAttributesCollection _dict;

        // TODO: remove
        private readonly Scope _parent;
        private bool _isVisible;

        /// <summary>
        /// Creates a new top-level scope with a new empty dictionary.  The scope
        /// is marked as being visible.
        /// </summary>
        public Scope()
            : this(null, null) {
        }

        /// <summary>
        /// Creates a new top-level Scope with the provided dictionary
        /// </summary>
        public Scope(IAttributesCollection dictionary)
            : this(null, dictionary) {
        }

        /// <summary>
        /// Creates a new Scope with the provided parent and dictionary.
        /// </summary>
        public Scope(Scope parent, IAttributesCollection dictionary)
            : this(parent, dictionary, true) {
        }

        /// <summary>
        /// Creates a new Scope with the provided parent, dictionary and visibility.
        /// </summary>
        public Scope(Scope parent, IAttributesCollection dictionary, bool isVisible) {
            _parent = parent;
            _dict = dictionary ?? new SymbolDictionary();
            _isVisible = isVisible;
            _extensions = ScopeExtension.EmptyArray;
        }

        public ScopeExtension GetExtension(ContextId languageContextId) {
            return (languageContextId.Id < _extensions.Length) ? _extensions[languageContextId.Id] : null;
        }

        public ScopeExtension SetExtension(ContextId languageContextId, ScopeExtension extension) {
            ContractUtils.RequiresNotNull(extension, "extension");

            if (languageContextId.Id >= _extensions.Length) {
                Array.Resize(ref _extensions, languageContextId.Id + 1);
            }

            ScopeExtension original = Interlocked.CompareExchange(ref _extensions[languageContextId.Id], extension, null);
            return original ?? extension;
        }

        /// <summary>
        /// Gets the parent of this Scope or null if the Scope has no parent.
        /// </summary>
        public Scope Parent {
            get {
                return _parent;
            }
        }

        /// <summary>
        /// Gets if the context is visible at this scope.  Visibility is a per-language feature that enables
        /// languages to include members in the Scope chain but hide them when directly exposed to the user.
        /// </summary>
        public bool IsVisible {
            get {
                return _isVisible;
            }
        }

        /// <summary>
        /// Returns the list of keys which are available to all languages.  Keys marked with the
        /// DontEnumerate flag will not be returned.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")] // TODO: fix
        public IEnumerable<SymbolId> Keys {
            get {
                foreach (object name in _dict.Keys) {
                    string strName = name as string;
                    if (strName == null) continue;

                    yield return SymbolTable.StringToId(strName);
                }
            }
        }

        /// <summary>
        /// Returns the list of Keys and Items which are available to all languages.  Keys marked
        /// with the DontEnumerate flag will not be returned.
        /// </summary>
        public IEnumerable<KeyValuePair<SymbolId, object>> Items {
            get {
                foreach (KeyValuePair<SymbolId, object> kvp in _dict.SymbolAttributes) {
                    yield return kvp;
                }
            }
        }

        /// <summary>
        /// Returns the list of Keys available to all languages in addition to those keys
        /// which are only available to the provided LanguageContext.
        /// 
        /// Keys marked with the DontEnumerate flag will not be returned.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")] // TODO: fix
        public IEnumerable<SymbolId> GetKeys(LanguageContext context) {
            foreach (SymbolId si in _dict.SymbolAttributes.Keys) {
                yield return si;
            }
        }

        /// <summary>
        /// Trys to lookup the provided name in the current scope.  Search includes
        /// names that are only visible to the provided LanguageContext.
        /// </summary>
        public bool TryGetName(SymbolId name, out object value) {
            if (_dict.TryGetValue(name, out value)) return true;

            value = null;
            return false;
        }

        /// <summary>
        /// Attempts to lookup the provided name in this scope or any outer scope.
        /// </summary>
        public bool TryLookupName(SymbolId name, out object value) {
            Scope curScope = this;
            do {
                if (curScope == this || curScope.IsVisible) {
                    if (curScope.TryGetName(name, out value)) {
                        return true;
                    }
                }

                curScope = curScope.Parent;
            } while (curScope != null);

            value = null;
            return false;
        }

        /// <summary>
        /// Attempts to lookup the provided name in this scope or any outer scope.
        /// If the name is not defined the language defined MissingName exception is thrown.
        /// </summary>
        public object LookupName(LanguageContext context, SymbolId name) {
            object res;
            if (!TryLookupName(name, out res)) {
                throw context.MissingName(name);
            }

            return res;
        }

        /// <summary>
        /// Sets the name to the specified value for the current context.
        /// </summary>
        /// <exception cref="MemberAccessException">The name has already been published and marked as ReadOnly</exception>
        public void SetName(SymbolId name, object value) {
            _dict[name] = value;
        }

        /// <summary>
        /// Removes all members from the dictionary and any context-sensitive dictionaries.
        /// </summary>
        public void Clear() {
            List<object> ids = new List<object>(_dict.Keys);
            foreach (object name in ids) {
                _dict.RemoveObjectKey(name);
            }
        }

        /// <summary>
        /// Determines if this context or any outer scope contains the defined name that
        /// is available from the provided LanguageContext.
        /// </summary>
        public bool ContainsName(SymbolId name) {
            object tmp;
            return TryLookupName(name, out tmp);
        }

        /// <summary>
        /// Attemps to remove the provided name from this scope removing names visible
        /// to both the current context and all contexts.
        /// </summary>
        public bool TryRemoveName(SymbolId name) {
            bool fRemoved = false;

            // TODO: Ideally, we could do this without having to do two lookups.
            object removedObject;
            if (_dict.TryGetValue(name, out removedObject) && removedObject != Uninitialized.Instance) {
                fRemoved = _dict.Remove(name) || fRemoved;
            }

            return fRemoved;
        }

        // Emitted by TupleSlotFactory
        /// <summary>
        /// Gets the outer-most scope associated with this scope.  
        /// </summary>
        public Scope ModuleScope {
            get {
                Scope cur = this;
                while (cur.Parent != null) cur = cur.Parent;

                return cur;
            }
        }

        /// <summary>
        /// Default scope dictionary
        /// </summary>
        public IAttributesCollection Dict {
            get {
                return _dict;
            }
        }

        #region Object key access

        /// <summary>
        /// Attemps to remove the provided object name from this scope removing names visible
        /// to both the current context and all contexts.
        /// </summary>
        public bool TryRemoveObjectName(object name) {
            return _dict.RemoveObjectKey(name);
        }

        public bool TryGetObjectName(object name, out object value) {
            if (_dict.TryGetObjectValue(name, out value)) return true;

            value = null;
            return false;
        }

        /// <summary>
        /// Sets the name to the specified value for the current context.
        /// 
        /// The name is an arbitrary object.
        /// </summary>
        public void SetObjectName(object name, object value) {
            _dict.AddObjectKey(name, value);                
        }

        public IEnumerable<object> GetAllKeys() {
            foreach (object key in _dict.Keys) {
                yield return key;
            }
        }

        /// <summary>
        /// Returns the list of Keys and Values available to all languages in addition to those
        /// keys which are only available to the provided LanguageContext.
        /// 
        /// Keys marked with DontEnumerate flag will not be returned.
        /// </summary>
        public IEnumerable<KeyValuePair<object, object>> GetAllItems() {
            foreach (KeyValuePair<object, object> kvp in _dict) {
                yield return kvp;
            }
        }

        #endregion
    }
}
