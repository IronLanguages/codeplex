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
using System.Collections.ObjectModel;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;
using System.Threading;

using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Ast {

    /// <summary>
    /// Dynamic Language Runtime Compiler.
    /// This part compiles lambdas.
    /// </summary>
    partial class LambdaCompiler {
        // Should this be in TypeGen directly?
        [MultiRuntimeAware]
        private static int _Counter;
        [MultiRuntimeAware]
        private static int _GeneratorCounter;

        private static readonly string[] _GeneratorSigNames = new string[] { "$gen", "$ret" };

        private void EmitEnvironmentIDs() {
            List<SymbolId> liftedVarIds = new List<SymbolId>();
            foreach (VariableInfo vi in _info.Variables.Values) {
                if (vi.Lift) {
                    liftedVarIds.Add(vi.Name);
                }
            }

            if (!IsDynamicMethod) {
                Debug.Assert(TypeGen != null);

                ILGen cctor = TypeGen.TypeInitializer;
                EmitEnvironmentIdArray(cctor, liftedVarIds);
                Slot fields = TypeGen.AddStaticField(typeof(SymbolId[]), "__symbolIds$" + _info.Lambda.Name + "$" + Interlocked.Increment(ref _Counter));
                fields.EmitSet(cctor);
                fields.EmitGet(_ilg);
            } else {
                EmitEnvironmentIdArray(_ilg, liftedVarIds);
            }
        }

        private static void EmitEnvironmentIdArray(ILGen init, List<SymbolId> liftedVarIds) {
            init.EmitDebugWriteLine("--- Environment IDs ---");
            init.EmitArray(typeof(SymbolId), liftedVarIds.Count, delegate(int i) { init.EmitSymbolId(liftedVarIds[i]); });
            init.EmitDebugWriteLine("--- End Environment IDs ---");
        }

        private void CreateEnvironmentFactory() {
            if (_info.HasEnvironment) {
                // Get the environment size
                int size = 0;

                if (_generatorInfo != null) {
                    size += _generatorInfo.TempCount;

                    foreach (VariableInfo vi in _info.Variables.Values) {
                        if (vi.Variable.NodeType == AstNodeType.TemporaryVariable) {
                            size++;
                        }
                    }
                }

                foreach (VariableInfo vi in _info.Variables.Values) {
                    if (vi.Lift) size++;
                }

                // Find the right environment factory for the size of elements to store
                _info.EnvironmentFactory = CreateEnvironmentFactory(size);
            }
        }

        private Slot/*!*/ EmitCreateLocalScope(bool visible) {
            Debug.Assert(_info.EnvironmentFactory != null);

            _ilg.EmitDebugWriteLine("-- ENV ALLOC START --");

            // Create tuple and store it in the local:
            // Tuple<...> tuple = new Tuple<...>() + initialize
            EnvironmentSlot environmentSlot = _info.EnvironmentFactory.CreateEnvironmentSlot(this);
            _info.EnvironmentFactory.EmitStorage(_ilg);
            environmentSlot.EmitSet(_ilg);

            // $frame = RuntimeHelpers.CreateLocalScope<TTuple>(tuple, names, parent, visible):
            Slot ctxSlot = GetNamedLocal(typeof(CodeContext), "$frame");
            environmentSlot.EmitGet(_ilg);
            EmitEnvironmentIDs();
            EmitCodeContext();
            _ilg.EmitBoolean(visible);

            MethodInfo scopeFactory = _info.Lambda.ScopeFactory ?? typeof(RuntimeHelpers).GetMethod("CreateLocalScope");
            _ilg.EmitCall(scopeFactory.MakeGenericMethod(_info.EnvironmentFactory.StorageType));

            ctxSlot.EmitSet(_ilg);

            _ilg.EmitDebugWriteLine("-- ENV ALLOC END --");

            EnvironmentSlot = environmentSlot;
            return ctxSlot;           
        }

        private void CreateSlots() {
            Debug.Assert(_info != null);
            LambdaExpression lambda = _info.Lambda;

            if (_info.HasEnvironment) {
                // we're an environment slot, we need our own environment slot, and we're
                // going to update our Context slot to point to a CodeContext which has
                // its Locals pointing at our Environment.
                ContextSlot = EmitCreateLocalScope(lambda.IsVisible);                
            }

            CreateClosureAccessSlots();
            CreateScopeAccessSlots();

            foreach (VariableInfo vi in _info.Variables.Values) {
                Allocate(vi);
            }

            CreateReferenceSlots();

            _info.LocalAllocator.PrepareForEmit(this);
            _info.GlobalAllocator.PrepareForEmit(this);
        }

        private void CreateClosureAccessSlots() {
            // Current context is accessed via environment slot, if any
            if (_info.HasEnvironment) {
                _info.ClosureAccess[_info.Lambda] = EnvironmentSlot;
            }

            if (_info.IsClosure) {
                Slot scope = _ilg.GetLocalTmp(typeof(CodeContext));
                EmitCodeContext();
                if (_info.HasEnvironment) {
                    _ilg.EmitCall(typeof(RuntimeHelpers), "GetStorageParent");
                }
                scope.EmitSet(_ilg);

                LambdaInfo current = _info;
                while (true) {
                    LambdaInfo parent = current.Parent;

                    if (parent.EnvironmentFactory != null) {
                        scope.EmitGet(_ilg);

                        _ilg.EmitCall(typeof(RuntimeHelpers).GetMethod("GetScopeStorage").MakeGenericMethod(parent.EnvironmentFactory.StorageType));

                        Slot storage = new LocalSlot(_ilg.DeclareLocal(parent.EnvironmentFactory.StorageType), _ilg);
                        storage.EmitSet(_ilg);
                        _info.ClosureAccess[parent.Lambda] = storage;
                    }

                    if (parent == null || !parent.IsClosure) {
                        break;
                    }

                    scope.EmitGet(_ilg);
                    _ilg.EmitCall(typeof(RuntimeHelpers), "GetStorageParent");
                    scope.EmitSet(_ilg);

                    current = parent;
                }

                _ilg.FreeLocalTmp(scope);
            }
        }

        private void CreateScopeAccessSlots() {
            LambdaInfo info = _info;
            while (info.IsClosure) {
                info = info.Parent;
            }

            while (info != null && info.LocalAllocator != null) {
                foreach (Expression variable in _info.ReferenceSlots.Keys) {
                    VariableInfo vi = _info.GetVariableInfo(variable);
                    if (!vi.Lift && vi.LambdaInfo == info) {
                        Slot accessSlot = info.LocalAllocator.GetAccessSlot(this);
                        // Returns:
                        //   this.ContextSlot if (allocator.LocalAllocator is FrameStorageAllocator)
                        //   null otherwise
                        if (accessSlot != null) {
                            _info.ScopeAccess.Add(info.Lambda, accessSlot);
                        }
                        break;
                    }
                }

                info = info.Parent;
            }
        }

        // internal because it's used by Rule.Emit
        internal void CreateReferenceSlots() {
            foreach (Expression variable in new List<Expression>(_info.ReferenceSlots.Keys)) {
                _info.ReferenceSlots[variable] = CreateSlot(_info.GetVariableInfo(variable));
            }
        }

        /// <summary>
        /// Emits a delegate to the method generated for the LambdaExpression.
        /// May end up creating a wrapper to match the requested delegate type.
        /// </summary>
        /// <param name="lambda">Lambda for which to generate a delegate</param>
        /// <param name="delegateType">Type of the delegate.</param>
        private void EmitDelegateConstruction(LambdaExpression lambda, Type delegateType) {
            LambdaInfo li = GetLambdaInfo(lambda);

            bool needsClosure = li.IsClosure || !(ContextSlot is StaticFieldSlot);

            //
            // Emit the lambda itself
            //
            LambdaCompiler lc = Compiler.ProvideLambdaImplementation(this, lambda, needsClosure);

            lc = CreateWrapperIfNeeded(lc, lambda, delegateType);

            EmitSequencePointNone();
            EmitDelegateConstruction(lc, delegateType, needsClosure);
        }

        /// <summary>
        /// If lambda and delegate signatures match, no need to create wrapper, otherwise
        /// we create wrapper and generate code to pass arguments through to the underlying method.
        /// </summary>
        private static LambdaCompiler CreateWrapperIfNeeded(LambdaCompiler lc, LambdaExpression lambda, Type type) {
            Debug.Assert(typeof(Delegate).IsAssignableFrom(type) && type != typeof(Delegate));

            Type[] blockSig = Expression.GetLambdaSignature(lambda);
            MethodInfo invoke = type.GetMethod("Invoke");
            ParameterInfo[] parameters = invoke.GetParameters();

            if (SignaturesMatch(blockSig, parameters)) {
                //
                // No wrapper needed if signatures match
                //
                return lc;
            } else {
                //
                // No match, must create wrapper
                //
                return lc.CreateWrapper(lambda, blockSig, invoke, parameters);
            }
        }

        /// <summary>
        /// Signatures match if the length is the same and they have identical types.
        /// </summary>
        private static bool SignaturesMatch(Type/*!*/[]/*!*/ blockSig, ParameterInfo/*!*/[]/*!*/ parameters) {
            if (blockSig.Length == parameters.Length) {
                for (int i = 0; i < parameters.Length; i++) {
                    if (blockSig[i] != parameters[i].ParameterType) {
                        // different types
                        return false;
                    }
                }
                // same
                return true;
            } else {
                // The delegate must have fewer parameters than lambda (not counting the params array)
                // and must be parameter array
                if (parameters.Length == 0 ||
                    !CompilerHelpers.IsParamArray(parameters[parameters.Length - 1]) ||
                    parameters.Length - 1 > blockSig.Length) {

                    // TODO: Validate this in the factory
                    throw new InvalidOperationException("Wrong delegate type");
                }

                return false;
            }
        }

        /// <summary>
        /// Creates the wrapper for the lambda to match delegate signature.
        /// The wrapper can have parameter array, and so can the lambda.
        /// 
        /// The wrapper will propagate parameters from the wrapper into the method
        /// backing the lambda, extracting them from the wrapper's parameter array
        /// if necessary, and also (if lambda itself takes parameter array) shift the
        /// wrapper's parameter array to pass it down to the lambda's method.
        /// </summary>
        /// <param name="lambda">LambdaExpression compiled by lc that we are building wrapper for.</param>
        /// <param name="lambdaSig">Signature of the lambda (parameters only)</param>
        /// <param name="wrapInvoke">"Invoke" method of the wrapper delegate type.</param>
        /// <param name="wrapSig">The wrapper delegate signature (parameters)</param>
        /// <returns>LambdaCompiler for the wrapper.</returns>
        private LambdaCompiler CreateWrapper(LambdaExpression/*!*/ lambda, Type/*!*/[]/*!*/ lambdaSig,
            MethodInfo/*!*/ wrapInvoke, ParameterInfo/*!*/[]/*!*/ wrapSig) {

            // Must be called on the compiler that originally compiled the lambda.
            Debug.Assert(_info != null && _info.Lambda == lambda);
            // Lambda must have at least as many parameters as the delegate, not counting the
            // params array
            Debug.Assert(lambdaSig.Length >= wrapSig.Length ||
                         (wrapSig.Length > 0 && CompilerHelpers.IsParamArray(wrapSig[wrapSig.Length - 1]) &&
                          lambdaSig.Length >= wrapSig.Length - 1));

            ConstantPool constants = ConstantPool.IsBound ? ConstantPool.CopyData() : null;

            // Create wrapper compiler
            LambdaCompiler wlc = CreateLambdaCompiler(
                null, // LambdaExpression
                Method.Name,
                wrapInvoke.ReturnType,
                ReflectionUtils.GetParameterTypes(wrapSig),
                null,                           // parameter names
                constants,
                _firstLambdaArgument > 0        // closure?
            );

            Debug.Assert(_firstLambdaArgument == wlc._firstLambdaArgument);

            // Pass the closure through, if any
            if (_firstLambdaArgument > 0) {
                wlc.GetArgumentSlot(0).EmitGet(wlc.IL);
            }

            bool paramArray = wrapSig.Length > 0 && CompilerHelpers.IsParamArray(wrapSig[wrapSig.Length - 1]);

            // How many parameters can we simply copy forward?
            int copy = wrapSig.Length;
            if (paramArray) copy--;                // do not copy the last parameter, it is a params array

            int current = 0;                        // the parameter being currently emitted

            //
            // First step is easy, simply copy the parameters that we can copy
            //
            while (current < copy) {
                Slot slot = wlc.GetLambdaArgumentSlot(current);
                slot.EmitGet(wlc.IL);

                // Cast to match the lambda signature, if needed
                wlc.EmitCast(slot.Type, lambdaSig[current]);

                current++;
            }

            //
            // If the wrapper has a parameter array, we need to extract the parameters from it
            //
            if (paramArray) {
                Slot wrapperArray = wlc.GetLambdaArgumentSlot(wrapSig.Length - 1);
                Type elementType = wrapSig[wrapSig.Length - 1].ParameterType.GetElementType();

                int extract = lambdaSig.Length - current;    // we already copied "current" number of parameters
                if (lambda.ParameterArray) extract--;       // do not extract last if the lambda has parameter array too

                int extracting = 0;                         // index into the the wrapper's parameter array

                //
                // Extract the parameters that get passed to underlying method one by one
                // to pass them as actual arguments
                //
                while (extracting < extract) {
                    wrapperArray.EmitGet(wlc.IL);
                    wlc.IL.EmitInt(extracting);
                    wlc.IL.EmitLoadElement(elementType);

                    wlc.EmitCast(elementType, lambdaSig[current]);

                    extracting++; current++;
                }

                //
                // Extract the rest of the array in bulk
                //
                if (lambda.ParameterArray) {
                    wrapperArray.EmitGet(wlc.IL);
                    wlc.IL.EmitInt(extract);               // this is how many we already extracted

                    // Call the helper
                    MethodInfo shifter = typeof(RuntimeHelpers).GetMethod("ShiftParamsArray");
                    shifter = shifter.MakeGenericMethod(elementType);
                    wlc.IL.EmitCall(shifter);

                    // just copied the last parameter
                    current++;
                }
            } else {
                // With no parameter array, we must have dealt with all wrapper's arguments
                Debug.Assert(current == wrapSig.Length);
            }

            // We must be done with all parameters to the lambdas method now
            Debug.Assert(current == lambdaSig.Length);

            MethodInfo method = (MethodInfo)Method;
            wlc.IL.EmitCall(method);
            wlc.EmitCast(method.ReturnType, wrapInvoke.ReturnType);
            wlc.IL.Emit(OpCodes.Ret);
            return wlc;
        }

        /// <summary>
        /// Creates the signature for the actual CLR method to create. The base types come from the
        /// lambda/LambdaExpression (or its wrapper method), this method may pre-pend an argument to hold
        /// closure information (for closing over constant pool or the lexical closure)
        /// </summary>
        private static Type/*!*/[]/*!*/ MakeParameterTypeArray(IList<Type/*!*/>/*!*/ baseTypes, ConstantPool cp, bool closure) {
            Assert.NotNullItems(baseTypes);

            Type[] signature;

            if (cp != null || closure) {
                signature = new Type[baseTypes.Count + 1];
                baseTypes.CopyTo(signature, 1);

                // Add the closure argument
                signature[0] = typeof(Closure);
            } else {
                signature = new Type[baseTypes.Count];
                baseTypes.CopyTo(signature, 0);
            }

            return signature;
        }

        /// <summary>
        /// Creates the signature for the lambda as list of types and list of names separately
        /// </summary>
        private static void ComputeSignature(LambdaExpression lambda, out List<Type> paramTypes, out List<string> paramNames, out string implName) {
            paramTypes = new List<Type>();
            paramNames = new List<string>();

            foreach (ParameterExpression p in lambda.Parameters) {
                paramTypes.Add(p.Type);
                paramNames.Add(p.Name);
            }

            implName = GetGeneratedName(lambda.Name);
        }

        private static string GetGeneratedName(string prefix) {
            return prefix + "$" + Interlocked.Increment(ref _Counter);
        }

        /// <summary>
        /// Defines the method with the correct signature and sets up the context slot appropriately.
        /// </summary>
        internal static LambdaCompiler CreateLambdaCompiler(LambdaCompiler outer, LambdaExpression lambda, bool closure) {
            List<Type> paramTypes;
            List<string> paramNames;
            LambdaCompiler impl;
            string implName;

            // Create the constant pool, if needed
            ConstantPool cp = outer.DynamicMethod ? new ConstantPool() : null;

            // Create the signature
            ComputeSignature(lambda, out paramTypes, out paramNames, out implName);

            // Create the new method & setup its locals
            impl = outer.CreateLambdaCompiler(lambda, implName, lambda.ReturnType, paramTypes, paramNames.ToArray(), cp, closure);

            // TODO: Can this go, if so, closure can be handled completely inside!!!
            if (closure) {
                impl.CreateClosureContextSlot();
            } else {
                impl.ContextSlot = outer.ContextSlot;
            }

            impl.SetDefaultAllocators(outer);

            return impl;
        }

        internal void CreateClosureContextSlot() {
            Debug.Assert(_firstLambdaArgument > 0);
            ContextSlot = new FieldSlot(GetArgumentSlot(0), typeof(Closure).GetField("Context"));
        }

        // Used by Compiler, TreeCompiler
        internal void EmitBody() {
            if (_generatorInfo != null) {
                EmitGeneratorLambdaBody();
            } else {
                EmitLambdaBody();
            }
        }

        private void EmitLambdaBody() {
            LambdaExpression lambda = _info.Lambda;

            Debug.Assert(lambda.GetType() == typeof(LambdaExpression));

            CreateEnvironmentFactory();
            CreateSlots();

            EmitBlockStartPosition(lambda);
            EmitExpressionAndPop(lambda.Body);
            EmitBlockEndPosition(lambda);

            // TODO: Skip if Body is guaranteed to return
            if (lambda.ReturnType != typeof(void)) {
                if (TypeUtils.CanAssign(typeof(object), lambda.ReturnType)) {
                    _ilg.EmitNull();
                } else {
                    _ilg.EmitMissingValue(lambda.ReturnType);
                }
            }
            EmitReturn();
        }

        private void EmitBlockStartPosition(LambdaExpression lambda) {
            // ensure a break point exists at the top
            // of the file if there isn't a statement
            // flush with the start of the file.
            if (!lambda.Start.IsValid) {
                return;
            }

            SourceSpan span = lambda.Body.Annotations.Get<SourceSpan>();
            if (span.Start.IsValid) {
                if (span.Start != lambda.Start) {
                    EmitPosition(lambda.Start, lambda.Start);
                }
            } else {
                Block body = lambda.Body as Block;
                if (body != null) {
                    for (int i = 0; i < body.Expressions.Count; i++) {
                        span = body.Expressions[i].Annotations.Get<SourceSpan>();
                        if (span.Start.IsValid) {
                            if (span.Start != lambda.Start) {
                                EmitPosition(lambda.Start, lambda.Start);
                            }
                            break;
                        }
                    }
                }
            }
        }

        private void EmitBlockEndPosition(LambdaExpression lambda) {
            // ensure we emit a sequence point at the end
            // so the user can inspect any info before exiting
            // the function.  Also make sure additional code
            // isn't associated with this function.
            EmitPosition(lambda.End, lambda.End);
            EmitSequencePointNone();
        }

        private static EnvironmentFactory CreateEnvironmentFactory(int size) {
            Type[] argTypes = CompilerHelpers.MakeRepeatedArray(typeof(object), size);
            Type tupleType = Tuple.MakeTupleType(argTypes);
            
            return new PropertyEnvironmentFactory(tupleType);
        }

        #region GeneratorLambdaExpression

        private void EmitGeneratorLambdaBody() {
            Debug.Assert(_info.Lambda.GetType() == typeof(GeneratorLambdaExpression));

            // pull out the rewritten generator
            GeneratorLambdaExpression gle = (GeneratorLambdaExpression)_info.Lambda;

            CreateEnvironmentFactory();
            EmitGeneratorBody(gle);
            EmitReturn();
        }

        /// <summary>
        /// Defines the method with the correct signature and sets up the context slot appropriately.
        /// </summary>
        /// <returns></returns>
        private LambdaCompiler CreateGeneratorLambdaCompiler(GeneratorLambdaExpression block) {
            // Create the GenerateNext function
            LambdaCompiler ncg = CreateLambdaCompiler(
                block,
                GetGeneratorMethodName(block.Name),     // Method Name
                typeof(bool),                           // Return Type
                new Type[] {                            // signature
                    block.GeneratorType,
                    typeof(object).MakeByRefType()
                },
                _GeneratorSigNames,                     // param names
                DynamicMethod ? new ConstantPool() : null,
                false                                   // no closure
            );

            Slot generator = ncg.GetLambdaArgumentSlot(0);
            ncg.ContextSlot = new PropertySlot(generator, typeof(Generator).GetProperty("Context"));

            // Namespace without factory - all locals must exist ahead of time
            ncg.SetAllocators(_info.GlobalAllocator, null);

            // We are emitting generator, mark the Compiler
            ncg.IsGeneratorBody = true;

            return ncg;
        }

        /// <summary>
        /// Emits the body of the function that creates a Generator object.  Also creates another
        /// Compiler for the inner method which implements the user code defined in the generator.
        /// </summary>
        private void EmitGeneratorBody(GeneratorLambdaExpression block) {
            LambdaCompiler ncg = CreateGeneratorLambdaCompiler(block);
            ncg.LambdaInfo.GlobalAllocator.PrepareForEmit(ncg);

            Slot flowedContext = ContextSlot;
            // If there are no locals in the generator than we don't need the environment
            if (_info.HasEnvironment) {
                // Environment creation is emitted into outer function that returns the generator
                // function and then flowed into the generator method on each call via the Generator
                // instance.
                flowedContext = EmitCreateLocalScope(block.IsVisible);

                InitializeGeneratorEnvironment(block);

                // Promote env storage to local variable
                // envStorage = ((FunctionEnvironment)context.Locals).Tuple
                _info.EnvironmentFactory.EmitGetStorageFromContext(ncg);

                ncg.EnvironmentSlot = _info.EnvironmentFactory.CreateEnvironmentSlot(ncg);
                ncg.EnvironmentSlot.EmitSet(ncg.IL);

                ncg.CreateGeneratorTemps();
            }

            ncg.CreateClosureAccessSlots();
            ncg.CreateScopeAccessSlots();
            ncg.CreateReferenceSlots();

            // Emit the generator body 
            ncg.EmitGenerator(block);

            flowedContext.EmitGet(_ilg);
            EmitDelegateConstruction(ncg, block.DelegateType, false);
            _ilg.EmitNew(block.GeneratorType, new Type[] { typeof(CodeContext), block.DelegateType });
        }

        private static string GetGeneratorMethodName(string name) {
            return name + "$g" + _GeneratorCounter++;
        }

        private void CreateGeneratorTemps() {
            for (int i = 0; i < _generatorInfo.Temps.Count; i++) {
                _generatorInfo.Temps[i] = _info.EnvironmentFactory.MakeEnvironmentReference(
                    SymbolTable.StringToId("temp$" + i)
                ).CreateSlot(EnvironmentSlot);
            }
        }

        // The slots for generators are created in 2 steps. In the outer function,
        // the slots are allocated, whereas in the actual generator they are CreateSlot'ed
        private void InitializeGeneratorEnvironment(GeneratorLambdaExpression block) {

            // Set this now so we can allocate variables in the outer function
            // later it will be set to the inner function's environment slot
            _info.ClosureAccess[block] = EnvironmentSlot;

            foreach (VariableInfo vi in _info.Variables.Values) {
                Allocate(vi);
            }
        }

        private void EmitGenerator(GeneratorLambdaExpression block) {
            IList<YieldTarget> topTargets = _generatorInfo.TopTargets;
            Debug.Assert(topTargets != null);

            Label[] jumpTable = new Label[topTargets.Count];
            for (int i = 0; i < jumpTable.Length; i++) {
                jumpTable[i] = topTargets[i].EnsureLabel(this);
            }

            Slot router = _ilg.GetLocalTmp(typeof(int));
            EmitGetGeneratorLocation();
            router.EmitSet(_ilg);
            GotoRouter = router;
            router.EmitGet(_ilg);
            _ilg.Emit(OpCodes.Switch, jumpTable);

            // fall-through on first pass
            // yield statements will insert the needed labels after their returns

            EmitExpression(block.Body);

            // fall-through is almost always possible in generators, so this
            // is almost always needed
            EmitReturnInGenerator(null);
            Finish();

            GotoRouter = null;
            _ilg.FreeLocalTmp(router);
        }

        #endregion

        #region variables storage/slot allocation

        private void Allocate(VariableInfo vi) {
            Debug.Assert(vi.LambdaInfo == _info);

            switch (vi.Variable.NodeType) {
                case AstNodeType.LocalVariable:
                    if (_info.Lambda.IsGlobal) {
                        // Local on global level, simply allocate the storage
                        vi.Storage = _info.LocalAllocator.AllocateStorage(vi.Name, vi.Variable.Type);
                    } else {
                        Slot slot;
                        // If lifting local into closure, allocate in the environment
                        if (vi.Lift) {
                            // allocate space in the environment and set it to Uninitialized
                            slot = AllocateInEnvironment(vi);
                        } else {
                            // Allocate the storage
                            vi.Storage = _info.LocalAllocator.AllocateStorage(vi.Name, vi.Variable.Type);
                            // No access slot for local variables, pass null.
                            slot = vi.Storage.CreateSlot(vi.Storage.RequireAccessSlot ? _info.ScopeAccess[_info.Lambda] : null);
                            slot.Local = true;
                        }
                    }
                    break;
                case AstNodeType.Parameter:
                    // Lifting parameter into closure, allocate in env and move.
                    if (vi.Lift) {
                        Slot slot = AllocateInEnvironment(vi);
                        Slot src = GetLambdaArgumentSlot(vi.ParameterIndex);
                        // Copy the value from the parameter (src) into the environment (slot)
                        slot.EmitSet(IL, src);
                    } else {
                        // Nothing to do here
                    }
                    break;

                case AstNodeType.GlobalVariable:
                    vi.Storage = _info.GlobalAllocator.AllocateStorage(vi.Name, vi.Variable.Type);
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
        private Slot AllocateInEnvironment(VariableInfo vi) {
            Debug.Assert(vi.Storage == null);
            Debug.Assert(vi.LambdaInfo == _info);

            // TODO: We should verify this before coming here.
            Debug.Assert(_info.EnvironmentFactory != null, "Allocating in environment without environment factory.\nIs HasEnvironment set?");

            vi.Storage = _info.EnvironmentFactory.MakeEnvironmentReference(vi.Name, vi.Variable.Type);
            return vi.Storage.CreateSlot(_info.ClosureAccess[_info.Lambda]);
        }

        private Slot CreateSlot(VariableInfo vi) {
            switch (vi.Variable.NodeType) {
                case AstNodeType.LocalVariable:
                    if (vi.Storage == null) {
                        // Fall back on a runtime lookup if this variable does not have storage associated with it
                        // (e.g. if the variable belongs to a lambda in interpreted mode).
                        return new LocalNamedFrameSlot(ContextSlot, vi.Name);
                    } else {
                        return CreateSlotForVariable(vi);
                    }

                case AstNodeType.Parameter:
                    if (vi.Lift) {
                        if (vi.Storage == null) {
                            return new LocalNamedFrameSlot(ContextSlot, vi.Name);
                        } else {
                            return CreateSlotForVariable(vi);
                        }
                    } else {
                        Slot slot = GetLambdaArgumentSlot(vi.ParameterIndex);
                        slot.Local = true;
                        return slot;
                    }

                case AstNodeType.GlobalVariable:
                    if (vi.Storage == null) {
                        return new NamedFrameSlot(ContextSlot, vi.Name);
                    } else {
                        // Globals are accessed via context slot
                        return vi.Storage.CreateSlot(ContextSlot);
                    }

                case AstNodeType.TemporaryVariable:
                    if (IsGeneratorBody) {
                        // Allocate in environment if emitting generator.
                        // This must be done here for now because the environment
                        // allocation, which is generally done in Allocate(),
                        // is done in the context of the outer generator codegen,
                        // which is not marked IsGenerator so the generator temps
                        // would go onto CLR stack rather than environment.
                        // TODO: Fix this once we have lifetime analysis in place.
                        vi.Storage = _info.EnvironmentFactory.MakeEnvironmentReference(vi.Name, vi.Variable.Type);
                        return CreateSlotForVariable(vi);
                    } else {
                        return GetNamedLocal(vi.Variable.Type, SymbolTable.IdToString(vi.Name));
                    }
            }

            throw new ArgumentException("Unexpected node type: " + vi.Variable.NodeType.ToString());
        }

        private Slot CreateSlotForVariable(VariableInfo vi) {
            Debug.Assert(vi.Storage != null);
            Slot access = null;
            if (vi.Storage.RequireAccessSlot) {
                // TODO: May need to check that the lambda is a generator here
                if (vi.Lift || vi.Variable.NodeType == AstNodeType.TemporaryVariable) {
                    access = _info.ClosureAccess[vi.LambdaInfo.Lambda];
                } else {
                    access = _info.ScopeAccess[vi.LambdaInfo.Lambda];
                }
            }
            Slot slot = vi.Storage.CreateSlot(access);
            slot.Local = true;
            return slot;
        }

        #endregion
    }
}
