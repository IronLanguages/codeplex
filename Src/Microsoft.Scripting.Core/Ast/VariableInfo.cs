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
    /// VariableInfo stores read/write information needed by the compiler to emit a variable/parameter
    /// </summary>
    internal sealed class VariableInfo {
        private readonly Expression/*!*/ _variable;

        private readonly LambdaExpression _lambda;  // hold onto the LambdaExpression for convenience

        private readonly int _parameterIndex;            // parameter index
        private Storage _storage;                   // storage for the variable, used to create slots

        private bool _lift;             // Lift variable/parameter to closure

        internal VariableInfo(VariableExpression/*!*/ variable, LambdaExpression lambda) {
            Debug.Assert(variable != null);
            _variable = variable;
            _lambda = lambda;
            _parameterIndex = -1;
        }

        internal VariableInfo(ParameterExpression/*!*/ parameter, LambdaExpression lambda, int parameterIndex) {
            Debug.Assert(parameter != null);
            Debug.Assert(parameterIndex >= 0);
            _variable = parameter;
            _lambda = lambda;
            _parameterIndex = parameterIndex;
        }

        internal LambdaExpression Lambda {
            get { return _lambda; }
        }

        internal Expression Variable {
            get { return _variable; }
        }

        internal SymbolId Name {
            get { return GetName(_variable); }
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
                    throw new InvalidOperationException(String.Format("Cannot lift variable of type {0} to a closure ('{1}')", _variable.NodeType, Name));
            }
        }

        // Gets the name of a VariableExpression or ParameterExpression
        internal static SymbolId GetName(Expression variable) {
            Debug.Assert(variable is VariableExpression || variable is ParameterExpression);
            VariableExpression v = variable as VariableExpression;
            if (v != null) {
                return v.Name;
            }
            return GetName((ParameterExpression)variable);
        }

        internal static SymbolId GetName(ParameterExpression p) {
            return p.Name != null ? SymbolTable.StringToId(p.Name) : SymbolId.Empty;
        }

        internal void Allocate(LambdaCompiler cg, LambdaInfo li) {
            Debug.Assert(cg.Allocator.Lambda == _lambda);

            switch (_variable.NodeType) {
                case AstNodeType.LocalVariable:
                    if (_lambda.IsGlobal) {
                        // Local on global level, simply allocate the storage
                        _storage = cg.Allocator.LocalAllocator.AllocateStorage(Name, _variable.Type);
                    } else {
                        Slot slot;
                        // If lifting local into closure, allocate in the environment
                        if (_lift) {
                            // allocate space in the environment and set it to Uninitialized
                            slot = AllocInEnv(cg, li);
                        } else {
                            // Allocate the storage
                            _storage = cg.Allocator.LocalAllocator.AllocateStorage(Name, _variable.Type);
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
                        Slot src = cg.GetLambdaArgumentSlot(_parameterIndex);
                        // Copy the value from the parameter (src) into the environment (slot)
                        slot.EmitSet(cg, src);
                    } else {
                        Debug.Assert(cg.Allocator.Lambda == _lambda);
                        // Nothing to do here
                    }
                    break;

                case AstNodeType.GlobalVariable:
                    _storage = cg.Allocator.GlobalAllocator.AllocateStorage(Name, _variable.Type);
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

            _storage = li.EnvironmentFactory.MakeEnvironmentReference(Name, _variable.Type);
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
                        return new LocalNamedFrameSlot(cg.ContextSlot, Name);
                    } else {
                        return CreateSlotForVariable(cg);
                    }

                case AstNodeType.Parameter:
                    if (_lift) {
                        if (_storage == null) {
                            return new LocalNamedFrameSlot(cg.ContextSlot, Name);
                        } else {
                            return CreateSlotForVariable(cg);
                        }
                    } else {
                        return MarkLocal(cg.GetLambdaArgumentSlot(_parameterIndex));
                    }

                case AstNodeType.GlobalVariable:
                    if (_storage == null) {
                        return new NamedFrameSlot(cg.ContextSlot, Name);
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
                        _storage = li.EnvironmentFactory.MakeEnvironmentReference(Name, _variable.Type);
                        return CreateSlotForVariable(cg);
                    } else {
                        return cg.GetNamedLocal(_variable.Type, SymbolTable.IdToString(Name));
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
