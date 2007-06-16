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
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

using Microsoft.Scripting.Ast;

namespace Microsoft.Scripting.Generation {
    /// <summary>
    /// Namespaces can be nested to get lexical scoping. Python looks up name as follows.
    /// 
    /// Assignments to a name :
    ///   For assignments, the name is assumed to be a local unless explicitly declared as global using
    ///   the "global var" statement.
    ///   GetSlotForSet() is the API which implements this.
    /// 
    /// References to a name
    ///   For names referenced in an expression or statement, Python uses the LEGB lookup rule, where the
    ///   order of lookup is:
    ///   1. Locals of the function
    ///   2. Enclosing local function scopes for nested functions and lambdas
    ///   3. Globals declared by the module
    ///   4. Built-in module which is always available
    ///   GetOrMakeSlotForGet() is the API which implements this
    /// 
    /// Note that for module-level code, globals and locals are the same.
    /// </summary>
    public class ScopeAllocator {
        private ScopeAllocator _parent;
        private StorageAllocator _allocator;

        private CodeBlock _block;

        // Slots to access outer scopes. For now this is dictionary, even though list would be better.
        // as soon as ScopeId goes away, make this list or something better.
        private Dictionary<CodeBlock, Slot> _scopeAccess = new Dictionary<CodeBlock, Slot>();

        private List<Slot> _generatorTemps;
        private int _generatorTempIndex;

        public ScopeAllocator(ScopeAllocator parent, StorageAllocator allocator) {
            _parent = parent;
            _allocator = allocator;
        }

        public StorageAllocator LocalAllocator {
            get { return _allocator; }
            set { _allocator = value; }
        }

        public StorageAllocator GlobalAllocator {
            get {
                ScopeAllocator global = this;
                while (global._parent != null) {
                    global = global._parent;
                }
                return global._allocator;
            }
        }

        public CodeBlock ActiveScope {
            get {
                Debug.Assert(_block != null);
                return _block;
            }
            set { _block = value; }
        }

        public Slot GetScopeAccessSlot(CodeBlock id) {
            Debug.Assert(_scopeAccess.ContainsKey(id));
            return _scopeAccess[id];
        }

        public void AddScopeAccessSlot(CodeBlock id, Slot slot) {
            Debug.Assert(!_scopeAccess.ContainsKey(id));
            _scopeAccess.Add(id, slot);
        }

        public void AddGeneratorTemp(Slot slot) {
            if (_generatorTemps == null) {
                _generatorTemps = new List<Slot>();
            }
            _generatorTemps.Add(slot);
        }

        public Slot GetGeneratorTemp() {
            Debug.Assert(_generatorTempIndex < _generatorTemps.Count);
            return _generatorTemps[_generatorTempIndex ++];
        }
    }
}
