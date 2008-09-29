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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;

namespace Microsoft.Linq.Expressions.Compiler {

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
        internal readonly CompilerScope Parent;
        
        // the expression node for this scope
        internal readonly Expression Expression;

        // Does this scope (or any inner scope) close over variables from any
        // parent scope?
        // TODO: We could simply logic across the system if we removed this,
        // and always add the Closure parameter to nested lambdas.
        internal readonly bool IsClosure;

        /// <summary>
        /// Scopes that were merged into this one, so we remember not to bind them
        /// </summary>
        internal readonly Queue<Expression> MergedScopes;

        // The scope's hoisted locals, if any.
        // Provides storage for variables that are referenced from nested lambdas
        private readonly HoistedLocals _hoistedLocals;

        /// <summary>
        /// The closed over hoisted locals
        /// </summary>
        private readonly HoistedLocals _closureHoistedLocals;

        // variables defined in this scope but not hoisted
        private readonly ReadOnlyCollection<Expression> _localVars;

        // Mutable dictionary that maps non-hoisted variables to either local
        // slots or argument slots
        private readonly Dictionary<Expression, Storage> _locals = new Dictionary<Expression, Storage>();

        /// <summary>
        /// Free list of unused generator temps, keyed off the type
        /// 
        /// These are virtual variables for storing hoisted temps that are
        /// needed by the compiler. They don't appear in the tree.
        /// </summary>
        private readonly KeyedQueue<Type, VariableExpression> _generatorTemps;

        /// <summary>
        /// Variables that should be cached into locals, if they're hoisted
        /// </summary>
        private readonly Set<Expression> _cachedVars;

        internal CompilerScope(
            CompilerScope parent,
            Expression expression,
            bool isClosure,
            VariableExpression hoistedSelfVar,
            ReadOnlyCollection<Expression> hoistedVars,
            ReadOnlyCollection<Expression> localVars,
            Queue<Expression> mergedScopes,
            KeyedQueue<Type, VariableExpression> generatorTemps,
            Set<Expression> cachedVars
        ) {
            Assert.NotNull(expression, hoistedVars, localVars);

            IsClosure = isClosure;
            Parent = parent;
            Expression = expression;
            MergedScopes = mergedScopes;
            _localVars = localVars;
            _generatorTemps = generatorTemps;
            _cachedVars = cachedVars;

            // No two scopes share the same parent
            Debug.Assert(Parent == null || Parent.Expression != Expression);

            // Generators always have non-null temps (but it might be empty)
            Debug.Assert((generatorTemps != null) == (expression.NodeType == ExpressionType.Generator));

            HoistedLocals locals = null;
            CompilerScope s = this;
            while (s.IsClosure && locals == null) {
                s = s.Parent;
                Debug.Assert(s != null);
                locals = s._hoistedLocals;
            }
            _closureHoistedLocals = locals;

            if (hoistedVars.Count > 0) {
                _hoistedLocals = new HoistedLocals(_closureHoistedLocals, hoistedSelfVar, hoistedVars);
            }

            DumpScope();
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
        /// This scope's hoisted locals, or the closed over locals, if any
        /// Equivalent to: _hoistedLocals ?? _closureHoistedLocals
        /// </summary>
        internal HoistedLocals NearestHoistedLocals {
            get { return _hoistedLocals ?? _closureHoistedLocals; }
        }

        internal LambdaExpression Lambda {
            get {
                CompilerScope cs = this;
                while (cs.IsScopeExpression) {
                    cs = cs.Parent;
                }
                return (LambdaExpression)cs.Expression;
            }
        }

        /// <summary>
        /// Called when entering a lambda. Performs all variable allocation
        /// needed, including creating hoisted locals and IL locals for accessing
        /// parent locals
        /// </summary>
        internal void EnterLambda(LambdaCompiler lc) {
            // generators need to call the EnterGenerator methods
            Debug.Assert(Expression.NodeType == ExpressionType.Lambda);

            AllocateLocals(lc);

            if (_closureHoistedLocals != null) {
                EmitClosureAccess(lc, _closureHoistedLocals);
            }

            EmitNewHoistedLocals(lc);
            EmitCachedVariables();
        }

        /// <summary>
        /// Called when entering a scope. Performs all variable allocation
        /// needed, including creating hoisted locals and IL locals for accessing
        /// parent locals
        /// </summary>
        internal void EnterScope(LambdaCompiler lc) {
            Debug.Assert(Expression.NodeType == ExpressionType.Scope);

            AllocateLocals(lc);
            EmitNewHoistedLocals(lc);
        }

        /// <summary>
        /// Called when entering the outer generator. Must be called after the
        /// inner generator has been emitted
        /// </summary>
        internal void EnterGeneratorOuter(LambdaCompiler lc) {
            Debug.Assert(Expression.NodeType == ExpressionType.Generator && !lc.IsGeneratorBody, "must be outer generator compiler");

            // Clear out locals so the outer generator ones don't conflict
            // with the inner generator ones.
            _locals.Clear();

            // Initialize hoisted locals in outer generator
            if (_closureHoistedLocals != null) {
                EmitClosureToVariable(lc, _closureHoistedLocals);
            }
            if (_hoistedLocals != null) {
                AddLocal(lc, _hoistedLocals.SelfVariable);
                EmitNewHoistedLocals(lc);
            }
        }

        /// <summary>
        /// Called when entering the inner generator. Must be called before the
        /// outer generator has been emitted
        /// </summary>
        internal void EnterGeneratorInner(LambdaCompiler lc) {
            Debug.Assert(Expression.NodeType == ExpressionType.Generator && lc.IsGeneratorBody, "must be inner generator compiler");
            Debug.Assert(_locals.Count == 0, "scope should not have been entered yet");

            // Allocate IL locals and emit closure access in the inner generator
            AllocateLocals(lc);
            EmitClosureAccess(lc, NearestHoistedLocals);

            // TODO: is this worth it:
            // We'd have to cache all variables every time we enter the generator
            //EmitCachedVariables();
        }

        #region LocalScopeExpression support

        internal void EmitVariableAccess(LambdaCompiler lc, ReadOnlyCollection<Expression> vars) {
            if (NearestHoistedLocals != null) {
                // Find what array each variable is on & its index
                List<string> names = new List<string>(vars.Count);
                List<long> indexes = new List<long>(vars.Count);

                foreach (Expression variable in vars) {
                    // For each variable, find what array it's defined on
                    ulong parents = 0;
                    HoistedLocals locals = NearestHoistedLocals;
                    while (!locals.Indexes.ContainsKey(variable)) {
                        parents++;
                        locals = locals.Parent;
                        Debug.Assert(locals != null);
                    }
                    
                    // combine the number of parents we walked, with the
                    // real index of variable to get the index to emit.
                    ulong index = (parents << 32) | (uint)locals.Indexes[variable];

                    names.Add(CompilerScope.GetName(variable));
                    indexes.Add((long)index);
                }

                if (names.Count > 0) {
                    EmitGet(NearestHoistedLocals.SelfVariable);
                    lc.EmitConstantArray(names.ToArray());
                    lc.EmitConstantArray(indexes.ToArray());
                    lc.IL.EmitCall(typeof(RuntimeOps).GetMethod("CreateVariableAccess"));
                    return;
                }
            }

            // No visible variables
            lc.IL.EmitCall(typeof(RuntimeOps).GetMethod("CreateEmptyVariableAccess"));
            return;
        }

        #endregion

        #region Variable access

        /// <summary>
        /// Adds a new virtual variable corresponding to an IL local
        /// </summary>
        internal void AddLocal(LambdaCompiler gen, VariableExpression variable) {
            if (!_locals.ContainsKey(variable)) {
                _locals.Add(variable, new LocalStorage(gen, variable));
            }
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

        private Storage ResolveVariable(Expression variable) {
            return ResolveVariable(variable, NearestHoistedLocals);
        }

        /// <summary>
        /// Resolve a local variable in this scope or a closed over scope
        /// Throws if the variable is defined
        /// </summary>
        private Storage ResolveVariable(Expression variable, HoistedLocals hoistedLocals) {
            // Search IL locals and arguments, but only in this lambda
            for (CompilerScope s = this; s != null; s = s.Parent) {
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
            for (HoistedLocals h = hoistedLocals; h != null; h = h.Parent) {
                if (h.Indexes.ContainsKey(variable)) {
                    return new ElementStorage(h, ResolveVariable(h.SelfVariable, hoistedLocals), variable);
                }
            }

            // If this is a genuine unbound variable, the error should be
            // thrown in VariableBinder
            Debug.Assert(
                false, 
                Strings.UndefinedVariable(
                    CompilerScope.GetName(variable),
                    variable.Type,
                    GetName(Expression)
                )
            );

            throw Error.UndefinedVariable(
                CompilerScope.GetName(variable),
                variable.Type,
                GetName(Expression)
            );

        }

        #endregion

        #region generator temps

        internal VariableExpression GetGeneratorTemp(Type type) {
            CompilerScope cs = this;
            while (cs.IsScopeExpression) {
                cs = cs.Parent;
            }
            Debug.Assert(cs._generatorTemps != null && cs._generatorTemps.GetCount(type) > 0);
            return cs._generatorTemps.Dequeue(type);
        }

        #endregion

        // private methods:

        private bool IsScopeExpression {
            get { return Expression.NodeType == ExpressionType.Scope; }
        }

        // Emits creation of the hoisted local storage
        private void EmitNewHoistedLocals(LambdaCompiler lc) {
            if (_hoistedLocals == null) {
                return;
            }

            // create the array
            lc.IL.EmitInt(_hoistedLocals.Variables.Count);
            lc.IL.Emit(OpCodes.Newarr, typeof(object));

            // initialize all elements
            int i = 0;
            foreach (Expression v in _hoistedLocals.Variables) {
                // array[i] = new StrongBox<T>(...);
                lc.IL.Emit(OpCodes.Dup);
                lc.IL.EmitInt(i++);
                Type boxType = typeof(StrongBox<>).MakeGenericType(v.Type);

                if (v.NodeType == ExpressionType.Parameter) {
                    // array[i] = new StrongBox<T>(argument);
                    int index = lc.Parameters.IndexOf((ParameterExpression)v);
                    Debug.Assert(index >= 0);
                    lc.EmitLambdaArgument(index);
                    lc.IL.Emit(OpCodes.Newobj, boxType.GetConstructor(new Type[] { v.Type }));
                } else if (v == _hoistedLocals.ParentVariable) {
                    // array[i] = new StrongBox<T>(closure.Locals);
                    ResolveVariable(v, _closureHoistedLocals).EmitLoad();
                    lc.IL.Emit(OpCodes.Newobj, boxType.GetConstructor(new Type[] { v.Type }));
                } else {
                    // array[i] = new StrongBox<T>();
                    lc.IL.Emit(OpCodes.Newobj, boxType.GetConstructor(Type.EmptyTypes));
                }
                // if we want to cache this into a local, do it now
                if (_cachedVars.Contains(v) && !_locals.ContainsKey(v) && !lc.IsGeneratorBody) {
                    lc.IL.Emit(OpCodes.Dup);
                    CacheBoxToLocal(lc, v);
                }
                lc.IL.Emit(OpCodes.Stelem_Ref);
            }

            // store it
            EmitSet(_hoistedLocals.SelfVariable);
        }


        // If hoisted variables are referenced "enough", we cache the
        // StrongBox<T> in an IL local, which saves an array index and a cast
        // when we go to look it up later
        private void EmitCachedVariables() {
            foreach (var v in _cachedVars) {
                if (!_locals.ContainsKey(v)) {
                    var storage = ResolveVariable(v) as ElementStorage;
                    if (storage != null) {
                        storage.EmitLoadBox();
                        CacheBoxToLocal(storage.Compiler, v);
                    }
                }
            }
        }

        private void CacheBoxToLocal(LambdaCompiler lc, Expression v) {
            Debug.Assert(_cachedVars.Contains(v) && !_locals.ContainsKey(v));
            var local = new LocalBoxStorage(lc, v);
            local.EmitStoreBox();
            _locals.Add(v, local);
        }

        // Creates IL locals for accessing closures
        private void EmitClosureAccess(LambdaCompiler lc, HoistedLocals locals) {
            if (locals == null) {
                return;
            }

            EmitClosureToVariable(lc, locals);

            while ((locals = locals.Parent) != null) {
                var v =  locals.SelfVariable;
                var local = new LocalStorage(lc, v);
                local.EmitStore(ResolveVariable(v));
                _locals.Add(v, local);
            }
        }

        private void EmitClosureToVariable(LambdaCompiler lc, HoistedLocals locals) {
            lc.EmitClosureArgument();
            lc.IL.Emit(OpCodes.Ldfld, typeof(Closure).GetField("Locals"));
            AddLocal(lc, locals.SelfVariable);
            EmitSet(locals.SelfVariable);
        }

        // Allocates slots for IL locals or IL arguments
        private void AllocateLocals(LambdaCompiler lc) {
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

        #region helper methods

        // Gets the name of the lambda, scope, or variable
        internal static string GetName(Expression e) {
            switch (e.NodeType) {
                case ExpressionType.Parameter:
                    return ((ParameterExpression)e).Name;
                case ExpressionType.Variable:
                    return ((VariableExpression)e).Name;
                case ExpressionType.Lambda:
                case ExpressionType.Generator:
                    return ((LambdaExpression)e).Name;
                case ExpressionType.Scope:
                    return ((ScopeExpression)e).Name;
                default: throw Assert.Unreachable;
            }
        }


        [Conditional("DEBUG")]
        private void DumpScope() {
            if (!DebugOptions.ShowScopes) {
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
                output.WriteLine("scope {0}({1}):", GetName(Expression), (Parent != null) ? GetName(Parent.Expression) : "");
                if (_localVars.Count > 0) {
                    output.WriteLine("  locals {0}:", _localVars.Count);
                    foreach (Expression v in _localVars) {
                        output.WriteLine("    {0}", CompilerScope.GetName(v));
                    }
                }
                if (_hoistedLocals != null) {
                    output.WriteLine("  hoisted {0}:", _hoistedLocals.Variables.Count);
                    foreach (Expression v in _hoistedLocals.Variables) {
                        output.WriteLine("    {0}", CompilerScope.GetName(v));
                    }
                }
#if !SILVERLIGHT
            } finally {
                Console.ForegroundColor = color;
            }
#endif
        }

        #endregion

    }
}
