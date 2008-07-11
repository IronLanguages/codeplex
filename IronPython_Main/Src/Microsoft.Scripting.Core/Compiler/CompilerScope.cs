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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Scripting.Generation;
using System.Scripting.Runtime;
using System.Scripting.Utils;
using CompilerServices = System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
    /// <summary>
    /// CompilerScope is the data structure which the Compiler keeps information
    /// related to compiling scopes. It stores the following information:
    ///   1. Parent relationship (for resolving variables)
    ///   2. Information about hoisted variables
    ///   3. Information for resolving closures
    /// 
    /// Instances are produced by VariableBinder, which does a tree walk
    /// looking for scope nodes: LambdaExpression, GeneratorLambdaExpression,
    /// and ScopeExpression.
    /// </summary>
    internal sealed partial class CompilerScope {
        // parent scope, if any
        private readonly CompilerScope _parent;
        
        // the expression node for this scope
        private readonly Expression _expression;

        // does this scope (or any inner scope) close over variables from any parent scope?
        private readonly bool _isClosure;

        // does this scope (or any inner scope in this generator) contain a yield?
        private readonly bool _scopeHasYield;

        // The scope's hoisted locals, if any.
        // Provides storage for variables that are referenced from nested lambdas
        private readonly HoistedLocals _hoistedLocals;

        // variables defined in this scope but not hoisted
        private readonly ReadOnlyCollection<Expression> _localVars;

        // Mutable dictionary that maps hosted local arrays to IL locals
        // Shared by all scopes within the same lambda
        private readonly Dictionary<HoistedLocals, Storage> _hoistedStorage;

        // Mutable dictionary that maps non-hoisted variables to either local
        // slots or argument slots
        private readonly Dictionary<Expression, Storage> _locals = new Dictionary<Expression, Storage>();

        /// <summary>
        /// The generator temps required to generate the lambda
        /// 
        /// These are not temporary variables visible in the tree, rather they
        /// are variables needed internally by code gen
        /// </summary>
        private readonly ReadOnlyCollection<VariableExpression> _generatorTemps;

        /// <summary>
        /// Mutable list of currently free generator temps, sorted by type
        /// </summary>
        private readonly KeyedQueue<Type, VariableExpression> _freeGeneratorTemps;

        internal CompilerScope(
            CompilerScope parent,
            Expression expression,
            bool isClosure,
            bool scopeHasYield,
            ReadOnlyCollection<Expression> hoistedVars,
            ReadOnlyCollection<Expression> localVars,
            ReadOnlyCollection<VariableExpression> generatorTemps
        ) {
            Assert.NotNull(expression, hoistedVars, localVars);

            _isClosure = isClosure;
            _scopeHasYield = scopeHasYield;
            _parent = parent;
            _expression = expression;
            _localVars = localVars;

            Debug.Assert(!_scopeHasYield || IsScopeExpression); // only scopes track yielding

            // only generators have two CompilerScopes pointing at the same expression
            // same_parent_expression implies is_generator
            Debug.Assert(_parent == null || _parent.Expression != _expression || _expression.NodeType == ExpressionType.Generator);

            if (hoistedVars.Count > 0) {
                _hoistedLocals = new HoistedLocals(ClosureHoistedLocals(), hoistedVars);
            }

            if (IsScopeExpression && parent != null) {
                // scopes share closure IL locals from their containing lambda
                _hoistedStorage = parent._hoistedStorage;
                _generatorTemps = parent._generatorTemps;
                _freeGeneratorTemps = parent._freeGeneratorTemps;
            } else {
                // lambdas get their own locals
                _hoistedStorage = new Dictionary<HoistedLocals, Storage>();
                _generatorTemps = generatorTemps;
                _freeGeneratorTemps = new KeyedQueue<Type, VariableExpression>();
            }

            DumpScope();
        }

        [Conditional("DEBUG")]
        private void DumpScope() {
            if (!CompilerDebugOptions.ShowScopes) {
                return;
            }

            System.IO.TextWriter output = Console.Out;
#if !SILVERLIGHT
            ConsoleColor color = Console.ForegroundColor;
            try {
                if (Console.BackgroundColor == ConsoleColor.White) {
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                } else {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                }
#endif
                output.WriteLine("scope {0}({1}):", GetName(_expression), (_parent != null) ? GetName(_parent.Expression) : "");
                if (_localVars.Count > 0) {
                    output.WriteLine("  locals {0}:", _localVars.Count);
                    foreach (Expression v in _localVars) {
                        output.WriteLine("    {0}", CompilerHelpers.GetVariableName(v));
                    }
                }
                if (_hoistedLocals != null) {
                    output.WriteLine("  hoisted {0}:", _hoistedLocals.Variables.Count);
                    foreach (Expression v in _hoistedLocals.Variables) {
                        output.WriteLine("    {0}", CompilerHelpers.GetVariableName(v));
                    }
                }
#if !SILVERLIGHT
            } finally {
                Console.ForegroundColor = color;
            }
#endif
        }

        /// <summary>
        /// The lambda or scope expression associated with this scope
        /// </summary>
        internal Expression Expression {
            get { return _expression; }
        }

        /// <summary>
        /// Indicates that this scope closes over variables in a parent scope
        /// </summary>
        internal bool IsClosure {
            get { return _isClosure; }
        }

        /// <summary>
        /// Indicates that this scope has hoisted variables
        /// 
        /// These are either closed over from a parent scope, or hoisted so we
        /// can provide access via the Expression.LocalVariables() intrinsic.
        /// </summary>
        internal bool HasHoistedLocals {
            get { return _hoistedLocals != null; }
        }

        /// <summary>
        /// If this scope represents an inner generator, this will return the outer
        /// generator scope. Otherwise, returns null
        /// </summary>
        internal CompilerScope GeneratorOuterScope {
            get {
                if (_expression.NodeType == ExpressionType.Generator && _parent != null && _parent.Expression == _expression) {
                    return _parent;
                }
                return null;
            }
        }

        /// <summary>
        /// Called when entering the scope. Performs all variable allocation
        /// needed, including creating hoisted locals and IL locals for accessing
        /// parent locals
        /// </summary>
        internal void Enter(LambdaCompiler lc) {
            Debug.Assert(_expression.NodeType != ExpressionType.Generator || lc.IsGeneratorBody == (GeneratorOuterScope != null));

            BuildGeneratorTempFreeList();
            EmitClosureAccess(lc);
            EmitNewHoistedLocals(lc);
            AllocateLocals(lc);
        }

        /// <summary>
        /// Called when exiting the scope. Clears out slots for IL locals.
        /// </summary>
        internal void Exit() {
            _locals.Clear();
            _freeGeneratorTemps.Clear();
            if (IsScopeExpression) {
                if (_hoistedLocals != null) {
                    _hoistedStorage.Remove(_hoistedLocals);
                }
            } else {
                _hoistedStorage.Clear();
            }
        }

        /// <summary>
        /// Emit the nearest hoisted locals, or null is there isn't one
        /// </summary>
        internal void EmitNearestHoistedLocals(ILGen gen) {
            HoistedLocals locals = NearestHoistedLocals();
            if (locals != null) {
                _hoistedStorage[locals].EmitLoad();
            } else {
                gen.EmitNull();
            }
        }

        #region LocalScopeExpression support

        internal void EmitVariableAccess(LambdaCompiler lc, ReadOnlyCollection<Expression> vars) {
            HoistedLocals nearestLocals = NearestHoistedLocals();
            if (nearestLocals != null) {
                // Find what array each variable is on & its index
                List<string> names = new List<string>(vars.Count);
                List<long> indexes = new List<long>(vars.Count);

                foreach (Expression variable in vars) {
                    // For each variable, find what array it's defined on
                    ulong parents = 0;
                    HoistedLocals locals = nearestLocals;
                    while (locals != null && !locals.Indexes.ContainsKey(variable)) {
                        parents++;
                        locals = locals.Parent;
                    }
                    
                    if (locals != null) {
                        // combine the number of parents we walked, with the
                        // real index of variable to get the index to emit.
                        ulong index = (parents << 32) | (uint)locals.Indexes[variable];

                        names.Add(CompilerHelpers.GetVariableName(variable));
                        indexes.Add((long)index);
                    }
                }

                if (names.Count > 0) {
                    _hoistedStorage[nearestLocals].EmitLoad();
                    lc.EmitConstantArray(names);
                    lc.EmitConstantArray(indexes);
                    lc.IL.EmitCall(typeof(RuntimeHelpers).GetMethod("CreateVariableAccess"));
                    return;
                }
            }

            // No visible variables
            lc.IL.EmitCall(typeof(RuntimeHelpers).GetMethod("CreateEmptyVariableAccess"));
            return;
        }

        #endregion

        #region Variable access

        /// <summary>
        /// Adds a new virtual variable corresponding to an IL local
        /// </summary>
        internal void AddLocal(ILGen gen, VariableExpression variable) {
            _locals.Add(variable, new LocalStorage(gen, variable.Type));
        }

        internal void EmitGet(Expression variable) {
            ResolveVariable(variable).EmitLoad();
        }

        internal void EmitSet(Expression variable) {
            ResolveVariable(variable).EmitStore();
        }

        internal void EmitAddressOf(Expression variable) {
            ResolveVariable(variable).EmitAddress();
        }

        /// <summary>
        /// Resolve a local variable in this scope or a closed over scope
        /// Throws if the variable is defined
        /// </summary>
        private Storage ResolveVariable(Expression variable) {
           
            // Search IL locals and arguments, but only in this lambda
            for (CompilerScope s = this; s != null; s = s._parent) {
                Storage storage;
                if (s._locals.TryGetValue(variable, out storage)) {
                    return storage;
                }

                // if this is a lambda or generator, we're done
                if (!s.IsScopeExpression) {
                    break;
                }
            }

            // search hoisted locals
            for (HoistedLocals h = NearestHoistedLocals(); h != null; h = h.Parent) {
                if (h.Indexes.ContainsKey(variable)) {
                    return new ElementStorage(h, _hoistedStorage[h], variable);
                }
            }

            throw new InvalidOperationException(
                string.Format(
                    "variable '{0}' of type '{1}' is not defined in scope '{2}'",
                    CompilerHelpers.GetVariableName(variable),
                    variable.Type,
                    GetName(_expression)
                )
            );
        }

        #endregion

        #region generator temps

        internal VariableExpression GetGeneratorTemp(ILGen gen, Type type) {
            if (_generatorTemps.Count > 0) {
                return _freeGeneratorTemps.Dequeue(type);
            } else {
                VariableExpression result = Expression.Variable(type, "$temp" + _locals.Count);
                _locals.Add(result, new LocalStorage(gen, type));
                return result;
            }
        }

        private void BuildGeneratorTempFreeList() {
            foreach (VariableExpression v in _generatorTemps) {
                _freeGeneratorTemps.Enqueue(v.Type, v);
            }
        }

        #endregion

        // private methods:

        private bool IsScopeExpression {
            get { return _expression.NodeType == ExpressionType.Scope; }
        }

        // Hoists IL arguments to the hoised locals array, if needed
        private void HoistArguments(LambdaCompiler lc, Storage hoistedStorage) {
            int i = 0;
            foreach (ParameterExpression p in lc.Parameters) {
                if (_hoistedLocals.Indexes.ContainsKey(p)) {
                    ElementStorage element = new ElementStorage(_hoistedLocals, hoistedStorage, p);
                    element.EmitStore(new ArgumentStorage(lc.IL, lc.GetLambdaArgument(i)));
                }
                i++;
            }
        }

        // Emits creation of the hoisted local storage
        private void EmitNewHoistedLocals(LambdaCompiler lc) {
            if (_hoistedLocals != null) {

                // create the array
                EmitNewLocalsArray(lc.IL);

                // store into local
                Storage storage = new LocalStorage(lc.IL, typeof(object[]));
                storage.EmitStore();

                // Copy the parent hoisted local pointer into this array
                if (_hoistedLocals.Parent != null) {
                    ElementStorage parent = new ElementStorage(_hoistedLocals, storage, _hoistedLocals.ParentVariable);
                    parent.EmitStore(_hoistedStorage[_hoistedLocals.Parent]);
                }

                // If this scope is inside a generator and contains a yield, we
                // need to hoist our locals array on the generator's locals array
                if (_scopeHasYield) {
                    Storage hoisted = ResolveVariable(GetGeneratorTemp(lc.IL, typeof(object[])));
                    hoisted.EmitStore(storage);
                    storage = hoisted;
                }

                _hoistedStorage.Add(_hoistedLocals, storage);

                if (!IsScopeExpression) {
                    HoistArguments(lc, storage);
                }
            }
        }

        private void EmitNewLocalsArray(ILGen gen) {
            // create the array
            gen.EmitInt(_hoistedLocals.Variables.Count);
            gen.Emit(OpCodes.Newarr, typeof(object));

            // initialize all elements
            int i = 0;
            foreach (Expression v in _hoistedLocals.Variables) {
                // array[i] = new StrongBox<T>();
                gen.Emit(OpCodes.Dup);
                gen.EmitInt(i++);
                gen.Emit(OpCodes.Newobj, typeof(CompilerServices.StrongBox<>).MakeGenericType(v.Type).GetConstructor(Type.EmptyTypes));
                gen.Emit(OpCodes.Stelem_Ref);
            }
        }

        // Creates IL locals for accessing closures
        private void EmitClosureAccess(LambdaCompiler lc) {
            if (!IsScopeExpression) {
                Debug.Assert(_hoistedStorage.Count == 0);

                HoistedLocals locals = ClosureHoistedLocals();
                if (locals != null) {

                    lc.EmitClosureArgument();
                    lc.IL.Emit(OpCodes.Ldfld, typeof(CompilerServices.Closure).GetField("Locals"));
                    LocalStorage storage = new LocalStorage(lc.IL, typeof(object[]));
                    storage.EmitStore();
                    _hoistedStorage.Add(locals, storage);

                    while (locals.Parent != null) {
                        LocalStorage parent = new LocalStorage(lc.IL, typeof(object[]));
                        parent.EmitStore(new ElementStorage(locals, storage, locals.ParentVariable));
                        locals = locals.Parent;
                        storage = parent;

                        _hoistedStorage.Add(locals, storage);
                    }
                }
            }
        }

        // Gets the current hoisted locals, or the closed over locals
        internal HoistedLocals NearestHoistedLocals() {
            return _hoistedLocals ?? ClosureHoistedLocals();
        }

        // Gets the closed over hoisted locals
        private HoistedLocals ClosureHoistedLocals() {
            HoistedLocals locals = null;
            CompilerScope s = this;
            while (s != null && s._isClosure && locals == null) {
                s = s._parent;
                locals = s._hoistedLocals;
            }
            return locals;
        }

        // Allocates slots for IL locals or IL arguments
        private void AllocateLocals(LambdaCompiler lc) {
            // allocate slots for non-hoisted variables
            foreach (Expression v in _localVars) {
                Storage s;
                if (v.NodeType == ExpressionType.Parameter) {
                    s = new ArgumentStorage(lc, (ParameterExpression)v);
                } else {
                    s = new LocalStorage(lc, (VariableExpression)v);
                }
                _locals.Add(v, s);
            }
        }

        #region static helper methods

        // Gets the name of the LambdaExpression or ScopeExpression
        internal static string GetName(Expression scope) {
            Assert.NotNull(scope);

            switch (scope.NodeType) {
                case ExpressionType.Scope:
                    return ((ScopeExpression)scope).Name;
                case ExpressionType.Lambda:
                case ExpressionType.Generator:
                    return ((LambdaExpression)scope).Name;
                default: throw Assert.Unreachable;
            }
        }

        #endregion

    }
}
