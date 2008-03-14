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
    class ScopeAllocator {
        private readonly ScopeAllocator _parent;
        private readonly StorageAllocator _allocator;

        private LambdaExpression _lambda;

        // Slots to access outer scopes. For now this is dictionary, even though list would be better.
        // as soon as ScopeId goes away, make this list or something better.
        private Dictionary<LambdaExpression, Slot> _closureAccess;

        // Slots to access scopes, including local. The closure access slots are closure specific,
        // and some variables in local scope may need access slot which is different than that
        // for closures
        private Dictionary<LambdaExpression, Slot> _scopeAccess;

        private List<Slot> _generatorTemps;
        private int _generatorTempIndex;

        public ScopeAllocator(ScopeAllocator parent, StorageAllocator allocator) {
            _parent = parent;
            _allocator = allocator;
        }

        public StorageAllocator LocalAllocator {
            get { return _allocator; }
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

        public LambdaExpression Lambda {
            get { return _lambda; }
            set { _lambda = value; }
        }

        public ScopeAllocator Parent {
            get { return _parent; }
        }

        public Slot GetClosureAccessSlot(LambdaExpression lambda) {
            return GetAccessSlot(lambda, _closureAccess);
        }

        public void AddClosureAccessSlot(LambdaExpression lambda, Slot slot) {
            AddAccessSlot(lambda, ref _closureAccess, slot);
        }

        public Slot GetScopeAccessSlot(LambdaExpression lambda) {
            return GetAccessSlot(lambda, _scopeAccess);
        }

        public void AddScopeAccessSlot(LambdaExpression lambda, Slot slot) {
            AddAccessSlot(lambda, ref _scopeAccess, slot);
        }

        public void AddGeneratorTemp(Slot slot) {
            if (_generatorTemps == null) {
                _generatorTemps = new List<Slot>();
            }
            _generatorTemps.Add(slot);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public Slot GetGeneratorTemp() {
            Debug.Assert(_generatorTempIndex < _generatorTemps.Count);
            return _generatorTemps[_generatorTempIndex ++];
        }

        private Slot GetAccessSlot(LambdaExpression lambda, Dictionary<LambdaExpression, Slot> slots) {
            Debug.Assert(slots != null);
            Debug.Assert(slots.ContainsKey(lambda));
            return slots[lambda];
        }

        private void AddAccessSlot(LambdaExpression lambda, ref Dictionary<LambdaExpression, Slot> slots, Slot slot) {
            if (slots == null) {
                slots = new Dictionary<LambdaExpression, Slot>();
            }
            Debug.Assert(!slots.ContainsKey(lambda) || slots[lambda] == slot);
            slots[lambda] = slot;
        }
    }
}
