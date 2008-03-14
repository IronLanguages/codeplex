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
using System.Diagnostics;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Ast {

    /// <summary>
    /// VariableInfo stores read/write information needed by the compiler to emit a variable
    /// </summary>
    internal sealed class VariableInfo {
        private readonly VariableExpression/*!*/ _variable;

        private readonly LambdaExpression _lambda;  // hold onto the LambdaExpression for convenience

        private readonly int _parameter;            // parameter index
        private Storage _storage;                   // storage for the variable, used to create slots

        private bool _lift;             // Lift variable/parameter to closure

        internal VariableInfo(VariableExpression/*!*/ variable, LambdaExpression lambda)
            : this(variable, lambda, -1) {
        }

        internal VariableInfo(VariableExpression/*!*/ variable, LambdaExpression lambda, int parameterIndex) {
            Debug.Assert(variable != null);
            Debug.Assert(parameterIndex < 0 || variable.NodeType == AstNodeType.Parameter);
            _variable = variable;
            _lambda = lambda;
            _parameter = parameterIndex;
        }

        internal LambdaExpression Lambda {
            get { return _lambda; }
        }

        internal VariableExpression Variable {
            get { return _variable; }
        }

        internal bool Lift {
            get { return _lift; }
        }

        internal void LiftToClosure() {
            switch (_variable.NodeType) {
                case AstNodeType.LocalVariable:
                case AstNodeType.Parameter:
                    _lift = true;
                    break;
                default:
                    throw new InvalidOperationException(String.Format("Cannot lift variable of type {0} to a closure ('{1}')", _variable.NodeType, _variable.Name));
            }
        }

        internal void Allocate(LambdaCompiler cg, LambdaInfo li) {
            Debug.Assert(cg.Allocator.Lambda == _lambda);

            switch (_variable.NodeType) {
                case AstNodeType.LocalVariable:
                    if (_lambda.IsGlobal) {
                        // Local on global level, simply allocate the storage
                        _storage = cg.Allocator.LocalAllocator.AllocateStorage(_variable.Name, _variable.Type);
                    } else {
                        Slot slot;
                        // If lifting local into closure, allocate in the environment
                        if (_lift) {
                            // allocate space in the environment and set it to Uninitialized
                            slot = AllocInEnv(cg, li);
                        } else {
                            // Allocate the storage
                            _storage = cg.Allocator.LocalAllocator.AllocateStorage(_variable.Name, _variable.Type);
                            // No access slot for local variables, pass null.
                            slot = _storage.CreateSlot(_storage.RequireAccessSlot ? cg.Allocator.GetScopeAccessSlot(_lambda) : null);
                            MarkLocal(slot);
                        }
                    }
                    break;
                case AstNodeType.Parameter:
                    // Lifting parameter into closure, allocate in env and move.
                    if (_lift) {
                        Slot slot = AllocInEnv(cg, li);
                        Slot src = cg.GetLambdaArgumentSlot(_parameter);
                        // Copy the value from the parameter (src) into the environment (slot)
                        slot.EmitSet(cg, src);
                    } else {
                        Debug.Assert(cg.Allocator.Lambda == _lambda);
                        // Nothing to do here
                    }
                    break;

                case AstNodeType.GlobalVariable:
                    _storage = cg.Allocator.GlobalAllocator.AllocateStorage(_variable.Name, _variable.Type);
                    break;
                case AstNodeType.TemporaryVariable:
                    // Nothing to do here
                    break;
            }
        }

        /// <summary>
        /// Will allocate the storage in the environment and return slot to access
        /// the variable in the current scope (so that it can be initialized)
        /// </summary>
        private Slot AllocInEnv(LambdaCompiler cg, LambdaInfo li) {
            Debug.Assert(_storage == null);
            Debug.Assert(_lambda == li.Lambda);

            // TODO: We should verify this before coming here.
            Debug.Assert(li.EnvironmentFactory != null, "Allocating in environment without environment factory.\nIs HasEnvironment set?");

            _storage = li.EnvironmentFactory.MakeEnvironmentReference(_variable.Name, _variable.Type);
            return _storage.CreateSlot(cg.Allocator.GetClosureAccessSlot(_lambda));
        }

        private static Slot MarkLocal(Slot slot) {
            Debug.Assert(slot != null);
            slot.Local = true;
            return slot;
        }

        internal Slot CreateSlot(LambdaCompiler cg, LambdaInfo li) {
            switch (_variable.NodeType) {
                case AstNodeType.LocalVariable:
                    if (_storage == null) {
                        // Fall back on a runtime lookup if this variable does not have storage associated with it
                        // (e.g. if the variable belongs to a lambda in interpreted mode).
                        return new LocalNamedFrameSlot(cg.ContextSlot, _variable.Name);
                    } else {
                        return CreateSlotForVariable(cg);
                    }

                case AstNodeType.Parameter:
                    if (_lift) {
                        if (_storage == null) {
                            return new LocalNamedFrameSlot(cg.ContextSlot, _variable.Name);
                        } else {
                            return CreateSlotForVariable(cg);
                        }
                    } else {
                        return MarkLocal(cg.GetLambdaArgumentSlot(_parameter));
                    }

                case AstNodeType.GlobalVariable:
                    if (_storage == null) {
                        return new NamedFrameSlot(cg.ContextSlot, _variable.Name);
                    } else {
                        // Globals are accessed via context slot
                        return _storage.CreateSlot(cg.ContextSlot);
                    }

                case AstNodeType.TemporaryVariable:
                    if (cg.IsGenerator) {
                        // Allocate in environment if emitting generator.
                        // This must be done here for now because the environment
                        // allocation, which is generally done in Allocate(),
                        // is done in the context of the outer generator codegen,
                        // which is not marked IsGenerator so the generator temps
                        // would go onto CLR stack rather than environment.
                        // TODO: Fix this once we have lifetime analysis in place.
                        _storage = li.EnvironmentFactory.MakeEnvironmentReference(_variable.Name, _variable.Type);
                        return CreateSlotForVariable(cg);
                    } else {
                        return cg.GetNamedLocal(_variable.Type, SymbolTable.IdToString(_variable.Name));
                    }
            }

            throw new ArgumentException("Unexpected node type: " + _variable.NodeType.ToString());
        }

        private Slot CreateSlotForVariable(LambdaCompiler cg) {
            Debug.Assert(_storage != null);
            Slot access = null;
            if (_storage.RequireAccessSlot) {
                // TODO: May need to check that the lambda is a generator here
                access = _lift || _variable.NodeType == AstNodeType.TemporaryVariable ?
                    cg.Allocator.GetClosureAccessSlot(_lambda) :
                    cg.Allocator.GetScopeAccessSlot(_lambda);
            }
            Slot slot = _storage.CreateSlot(access);
            return MarkLocal(slot);
        }
      
    }
}
