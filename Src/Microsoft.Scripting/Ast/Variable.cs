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
using System.Diagnostics;
using Microsoft.Scripting;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Ast {
    /// <summary>
    /// Definition represents actual memory/dictionary location in the generated code.
    /// </summary>
    public class Variable {
        public enum VariableKind {
            Local,
            Parameter,
            Temporary,              // Temporary variable (name not important/published)

            Global,                 // Globals may need to go away and be handled on Python side only.

            /// <summary>
            /// Since we don't have the proper analysis at codegen time,
            /// we need this variable kind - a temporary used in the generators.
            /// To survive the yield, the temporary will be allocated in the environment.
            /// 
            /// When the analysis is added, these will become regular temps and will
            /// be allocated in the environment only if their lifetime spans a yield.
            /// </summary>
            GeneratorTemporary
        };

        private readonly SymbolId _name;

        // TODO: Maybe we don't need this!
        private readonly CodeBlock _block;

        private readonly VariableKind _kind;
        private readonly Type _type;
        private readonly Expression _defaultValue;

        private Type _knownType;

        private int _parameter;                     // parameter index
        private bool _parameterArray;               // should be part of parameter array
        private Storage _storage;                   // storage for the variable, used to create slots

        private bool _lift;             // Lift variable/parameter to closure
        private bool _unassigned;       // Variable ever referenced without being assigned
        private bool _uninitialized;    // Variable ever used either uninitialized or after deletion
        
        private Variable(SymbolId name, VariableKind kind, CodeBlock block, Type type, Expression defaultValue) 
            : this ( name, kind, block, type, defaultValue, true) {
        }

        private Variable(SymbolId name, VariableKind kind, CodeBlock block, Type type, Expression defaultValue, bool parameterArray) {
            _name = name;
            _kind = kind;
            _block = block;
            _type = type;
            _defaultValue = defaultValue;
            _parameterArray = parameterArray;
        }

        public SymbolId Name {
            get { return _name; }
        }

        public CodeBlock Block {
            get { return _block; }
        }

        public VariableKind Kind {
            get { return _kind; }
        }

        public Type Type {
            get { return _type; }
        }

        public Expression DefaultValue {
            get { return _defaultValue; }
        }

        public bool IsTemporary {
            get {
                return _kind == VariableKind.Temporary || _kind == VariableKind.GeneratorTemporary;
            }
        }

        public int ParameterIndex {
            get { return _parameter; }
            set { _parameter = value; }
        }

        public bool InParameterArray {
            get { return _parameterArray; }
        }

        public bool Lift {
            get { return _lift; }
        }

        public bool Unassigned {
            get { return _unassigned; }
        }

        public Type KnownType {
            get { return _knownType; }
            set { _knownType = value; }
        }

        public void UnassignedUse() {
            _unassigned = true;
        }

        public bool Uninitialized {
            get { return _uninitialized; }
        }

        public void UninitializedUse() {
            _uninitialized = true;
        }

        public void LiftToClosure() {
            switch(_kind) {
                case VariableKind.Local:
                case VariableKind.Parameter:
                    _lift = true;
                    break;
                default:
                    throw new InvalidOperationException(String.Format("Cannot lift variable of kind {0} to a closure ('{1}')", _kind, _name));
            }
        }

        public void Allocate(CodeGen cg) {
            Debug.Assert(cg.Allocator.ActiveScope == Block);

            switch (_kind) {
                case VariableKind.Local:
                    if (_block.IsGlobal) {
                        // Local on global level, simply allocate the storage
                        _storage = cg.Allocator.LocalAllocator.AllocateStorage(_name, _type);
                        if (_defaultValue != null) {
                            Slot slot = CreateSlotForVariable(cg);
                            _defaultValue.EmitAs(cg, slot.Type);
                            slot.EmitSet(cg);
                        } 
                    } else {
                        Slot slot;
                        // If lifting local into closure, allocate in the environment
                        if (_lift) {
                            // allocate space in the environment and set it to Uninitialized
                            slot = AllocInEnv(cg);
                        } else {
                            // Allocate the storage
                            _storage = cg.Allocator.LocalAllocator.AllocateStorage(_name, _type);
                            // No access slot for local variables, pass null.
                            slot = _storage.CreateSlot(null);
                            MarkLocal(slot);
                        }
                        if (_uninitialized || _lift) {
                            // Emit initialization
                            if (_defaultValue != null) {
                                _defaultValue.EmitAs(cg, slot.Type);
                                slot.EmitSet(cg);
                            } else {
                                slot.EmitSetUninitialized(cg);
                            }
                        }
                    }
                    break;
                case VariableKind.Parameter:
                    // Lifting parameter into closure, allocate in env and move.
                    if (_lift) {
                        Slot slot = AllocInEnv(cg);
                        Slot src = GetArgumentSlot(cg);
                        // Copy the value from the parameter (src) into the environment (slot)
                        slot.EmitSet(cg, src);
                    } else {
                        Debug.Assert(cg.Allocator.ActiveScope == Block);
                        // Nothing to do here
                    }
                    break;

                case VariableKind.Global:
                    _storage = cg.Allocator.GlobalAllocator.AllocateStorage(_name, _type);
                    break;
                case VariableKind.Temporary:
                    // Nothing to do here
                    break;
                case VariableKind.GeneratorTemporary:
                    // Do the work in CreateSlot
                    break;
            }
        }

        /// <summary>
        /// Will allocate the storage in the environment and return slot to access
        /// the variable in the current scope (so that it can be initialized)
        /// </summary>
        private Slot AllocInEnv(CodeGen cg) {
            Debug.Assert(_storage == null);
            Debug.Assert(_block.EnvironmentFactory != null, "Allocating in environment without environment factory.\nIs HasEnvironment set?");
            _storage = _block.EnvironmentFactory.MakeEnvironmentReference(_name, _type);
            return _storage.CreateSlot(cg.Allocator.GetScopeAccessSlot(_block));
        }

        private static Slot MarkLocal(Slot slot) {
            Debug.Assert(slot != null);
            slot.Local = true;
            return slot;
        }

        internal Slot CreateSlot(CodeGen cg) {
            switch (_kind) {
                case VariableKind.Local: {
                        return CreateSlotForVariable(cg);
                    }

                case VariableKind.Parameter:
                    if (_lift) {
                        return CreateSlotForVariable(cg);
                    } else {
                        //Debug.Assert(cg.Allocator.ActiveScope == _block);
                        return MarkLocal(GetArgumentSlot(cg));
                    }

                case VariableKind.Global:
                    // Globals are accessed via context slot
                    return _storage.CreateSlot(cg.ContextSlot);

                case VariableKind.Temporary:
                    return cg.GetNamedLocal(_type, SymbolTable.IdToString(_name));

                case VariableKind.GeneratorTemporary:
                    if (!cg.IsGenerator) {
                        goto case VariableKind.Temporary;
                    }

                    // Allocate in environment if emitting generator.
                    // This must be done here for now because the environment
                    // allocation, which is generally done in Allocate(),
                    // is done in the context of the outer generator codegen,
                    // which is not marked IsGenerator so the generator temps
                    // would go onto CLR stack rather than environment.
                    // TODO: Fix this once we have lifetime analysis in place.
                    _storage = _block.EnvironmentFactory.MakeEnvironmentReference(_name, _type);
                    return CreateSlotForVariable(cg);
            }

            Debug.Assert(false, "Unexpected variable kind: " + _kind.ToString());
            return null;
        }

        private Slot GetArgumentSlot(CodeGen cg) {
            Slot arg;
            if (_block != null && _block.ParameterArray) {
                // If not part of parameter array, get the normal parameter slot
                if (!_parameterArray) {
                    arg = cg.GetArgumentSlot(_parameter);
                } else {
                    Debug.Assert(cg.ParamsSlot != null);
                    Debug.Assert(cg.ParamsSlot.Type == typeof(object[]));
                    arg = new IndexSlot(cg.ParamsSlot, _parameter);
                    if (_type != typeof(object)) {
                        arg = new CastSlot(arg, _type);
                    }
                }
            } else {
                arg = cg.GetArgumentSlot(_parameter);
            }
            return arg;
        }

        private Slot CreateSlotForVariable(CodeGen cg) {
            Debug.Assert(_storage != null);
            Slot access = null;
            if (_storage.RequireAccessSlot) {
                access = cg.Allocator.GetScopeAccessSlot(_block);
            }
            Slot slot = _storage.CreateSlot(access);
            return MarkLocal(slot);
        }

        #region Factory methods

        public static Variable Parameter(CodeBlock block, SymbolId name, Type type) {
            return new Variable(name, VariableKind.Parameter, block, type, null);
        }

        public static Variable Parameter(CodeBlock block, SymbolId name, Type type, Expression defaultValue) {
            return new Variable(name, VariableKind.Parameter, block, type, defaultValue);
        }

        public static Variable Parameter(CodeBlock block, SymbolId name, Type type, Expression defaultValue, bool parameterArray) {
            return new Variable(name, VariableKind.Parameter, block, type, defaultValue, parameterArray);
        }

        internal static Variable Local(SymbolId name, CodeBlock block, Type type) {
            return new Variable(name,  VariableKind.Local, block, type, null);
        }

        internal static Variable Temporary(SymbolId name, CodeBlock block, Type type) {
            return new Variable(name, VariableKind.Temporary, block, type, null);
        }

        internal static Variable GeneratorTemp(SymbolId name, CodeBlock block, Type type) {
            return new Variable(name, VariableKind.GeneratorTemporary, block, type, null);
        }

        internal static Variable Create(SymbolId name, VariableKind kind, CodeBlock block, Type type) {
            return Create(name, kind, block, type, null);
        }

        internal static Variable Create(SymbolId name, VariableKind kind, CodeBlock block, Type type, Expression defaultValue) {
            // Cannot create parameters this way
            if (kind == VariableKind.Parameter) {
                throw new ArgumentException("kind");
            }
            return new Variable(name, kind, block, type, defaultValue);
        }

        #endregion
    }
}
