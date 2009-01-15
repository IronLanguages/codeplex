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
                if (vi.Lift && vi.Variable.NodeType != AstNodeType.TemporaryVariable) {
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
            init.EmitArray(typeof(SymbolId), liftedVarIds.Count, delegate(int i) { init.EmitSymbolId(liftedVarIds[i]); });
        }

        private void CreateEnvironmentAllocator() {
            if (_info.HasEnvironment) {
                // Get the types of the elements in the environment Tuple
                // Order is important here--we need to add types in the same
                // order we will allocate them

                // 1. Collect locals and parameters. These go first because
                //    they're visible to languages via Expression.LocalScope()
                List<Type> types = new List<Type>();
                foreach (VariableInfo vi in _info.Variables.Values) {
                    if (vi.Lift && vi.Variable.NodeType != AstNodeType.TemporaryVariable) {
                        types.Add(vi.Variable.Type);
                    }
                }

                int locals = types.Count;

                // 2. Collect parent environment pointer, if any
                if (_info.IsClosure) {
                    types.Add(_info.Parent.EnvironmentAllocator.StorageType);
                }

                // 3. Collect normal temporary variables
                foreach (VariableInfo vi in _info.Variables.Values) {
                    if (vi.Lift && vi.Variable.NodeType == AstNodeType.TemporaryVariable) {
                        types.Add(vi.Variable.Type);
                    }
                }

                // Find the right environment factory for the size of elements to store
                _info.EnvironmentAllocator = new EnvironmentAllocator(Tuple.MakeTupleType(types.ToArray()), locals);
            }
        }

        private Slot/*!*/ EmitCreateEnvironment(bool visible) {
            Debug.Assert(_info.EnvironmentAllocator != null);
            
            // Create tuple and store it in the local:
            // $environment = new Tuple<...>();
            Type tupleType = _info.EnvironmentAllocator.StorageType;
            EnvironmentSlot = GetNamedLocal(tupleType, "$environment");
            _info.EnvironmentAllocator.EmitStorage(_ilg);
            EnvironmentSlot.EmitSet(_ilg);

            // TODO: remove all of this:

            // $frame = RuntimeHelpers.CreateLocalScope<TTuple>(tuple, names, parent, visible):
            Slot ctxSlot = GetNamedLocal(typeof(CodeContext), "$frame");
            EnvironmentSlot.EmitGet(_ilg);
            EmitEnvironmentIDs();
            EmitCodeContext();
            _ilg.EmitBoolean(visible);
            MethodInfo scopeFactory = _info.Lambda.ScopeFactory ?? typeof(RuntimeHelpers).GetMethod("CreateLocalScope");
            _ilg.EmitCall(scopeFactory.MakeGenericMethod(tupleType));
            ctxSlot.EmitSet(_ilg);
            return ctxSlot;           
        }

        private void CreateSlots() {
            Debug.Assert(_info != null);
            LambdaExpression lambda = _info.Lambda;

            if (_info.HasEnvironment) {
                // we're an environment slot, we need our own environment slot, and we're
                // going to update our Context slot to point to a CodeContext which has
                // its Locals pointing at our Environment.
                ContextSlot = EmitCreateEnvironment(lambda.IsVisible);
            }

            Slot parentEnv = EmitParentEnvironment();
            StoreParentEnvironment(parentEnv);
            CreateClosureAccessSlots(parentEnv);
            CreateVariableStorage();
            CreateReferenceSlots();

            _info.LocalAllocator.PrepareForEmit(this);
            _info.GlobalAllocator.PrepareForEmit(this);
        }

        private void StoreParentEnvironment(Slot parentEnv) {
            if (parentEnv != null && _info.HasEnvironment) {
                // Save a pointer to the parent environment into our environment tuple
                // $environment.Item??? = (TTuple)closure.Environment;
                Storage storage = _info.EnvironmentAllocator.AllocateStorage(Expression.Temporary(_info.Parent.EnvironmentAllocator.StorageType, "parentEnv"));
                _info.ParentEnvironmentStorage = storage;
                storage.CreateSlot(EnvironmentSlot).EmitSet(_ilg, parentEnv);
            }
        }

        private Slot EmitParentEnvironment() {
            if (_info.IsClosure) {
                Debug.Assert(_closureSlot != null && _info.Parent != null);
                Type tupleType = _info.Parent.EnvironmentAllocator.StorageType;
                _closureSlot.EmitGet(_ilg);
                _ilg.EmitFieldGet(typeof(Closure).GetField("Environment"));
                _ilg.Emit(OpCodes.Castclass, tupleType);
                Slot slot = new LocalSlot(_ilg.DeclareLocal(tupleType), _ilg);
                slot.EmitSet(_ilg);
                return slot;
            }
            return null;
        }

        private void EmitGeneratorEnvironment() {
            Debug.Assert(IsGeneratorBody && _closureSlot != null && _info.HasEnvironment);

            Type tupleType = _info.EnvironmentAllocator.StorageType;
            _closureSlot.EmitGet(_ilg);
            _ilg.EmitFieldGet(typeof(Closure).GetField("Environment"));
            _ilg.Emit(OpCodes.Castclass, tupleType);
            Slot slot = new LocalSlot(_ilg.DeclareLocal(tupleType), _ilg);
            slot.EmitSet(_ilg);
            EnvironmentSlot = slot;
        }

        private void CreateClosureAccessSlots(Slot parentEnv) {
            // Current context is accessed via environment slot, if any
            if (_info.HasEnvironment) {
                _info.ClosureAccess[_info.Lambda] = EnvironmentSlot;
            }

            if (_info.IsClosure) {
                Debug.Assert(_info.Parent != null && parentEnv != null);

                _info.ClosureAccess[_info.Parent.Lambda] = parentEnv;

                for (LambdaInfo li = _info.Parent; li.IsClosure; li = li.Parent) {
                    li.ParentEnvironmentStorage.CreateSlot(parentEnv).EmitGet(_ilg);
                    parentEnv = new LocalSlot(_ilg.DeclareLocal(li.Parent.EnvironmentAllocator.StorageType), _ilg);
                    parentEnv.EmitSet(_ilg);
                    _info.ClosureAccess[li.Parent.Lambda] = parentEnv;
                }
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
            _closureSlot = GetArgumentSlot(0);
            ContextSlot = new FieldSlot(_closureSlot, typeof(Closure).GetField("Context"));
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

            CreateEnvironmentAllocator();
            CreateSlots();

            EmitlambdaStart(lambda);
            EmitExpressionAsVoid(lambda.Body);
            EmitLambdaEnd(lambda);

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

        #region DebugMarkers

        private void EmitlambdaStart(LambdaExpression lambda) {
            if (!_emitDebugSymbols) {
                return;
            }

            // ensure a break point exists at the top
            // of the file if there isn't a statement
            // flush with the start of the file.
            SourceSpan lambdaSpan;
            if (!lambda.Annotations.TryGet<SourceSpan>(out lambdaSpan)) {
                return;
            }

            SourceSpan bodySpan = lambda.Body.Annotations.Get<SourceSpan>();
            if (bodySpan.Start.IsValid) {
                if (bodySpan.Start != lambdaSpan.Start) {
                    EmitPosition(lambdaSpan.Start, lambdaSpan.Start);
                }
            } else {
                Block body = lambda.Body as Block;
                if (body != null) {
                    for (int i = 0; i < body.Expressions.Count; i++) {
                        bodySpan = body.Expressions[i].Annotations.Get<SourceSpan>();
                        if (bodySpan.Start.IsValid) {
                            if (bodySpan.Start != lambdaSpan.Start) {
                                EmitPosition(lambdaSpan.Start, lambdaSpan.Start);
                            }
                            break;
                        }
                    }
                }
            }
        }

        private void EmitLambdaEnd(LambdaExpression lambda) {
            if (!_emitDebugSymbols) {
                return;
            }

            // ensure we emit a sequence point at the end
            // so the user can inspect any info before exiting
            // the function.  Also make sure additional code
            // isn't associated with this function.           
            SourceSpan span;
            if (lambda.Annotations.TryGet<SourceSpan>(out span)) {
                EmitPosition(span.End, span.End);
            }
            EmitSequencePointNone();
        }

        #endregion


        #region GeneratorLambdaExpression

        private void EmitGeneratorLambdaBody() {
            Debug.Assert(_info.Lambda.GetType() == typeof(GeneratorLambdaExpression));

            // pull out the rewritten generator
            GeneratorLambdaExpression gle = (GeneratorLambdaExpression)_info.Lambda;

            CreateEnvironmentAllocator();
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
                _info.HasEnvironment || _info.IsClosure
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
                flowedContext = EmitCreateEnvironment(block.IsVisible);

                // Store the parent environment in our environment
                StoreParentEnvironment(EmitParentEnvironment());

                // Set this now so we can allocate variables in the outer function
                // later it will be set to the inner function's environment slot
                _info.ClosureAccess[block] = EnvironmentSlot;

                // The slots for generators are created in 2 steps. In the outer function,
                // the slots are allocated, whereas in the actual generator they are CreateSlot'ed
                CreateVariableStorage();

                // Promote env storage to local variable
                ncg.EmitGeneratorEnvironment();
            }

            Slot parentEnv = null;
            if (_info.IsClosure) {
                // If the generator has its own environment, get the parent,
                // otherwise use the one stored in the closure.
                if (_info.HasEnvironment) {
                    parentEnv = _info.ParentEnvironmentStorage.CreateSlot(ncg.EnvironmentSlot);
                } else {
                    parentEnv = ncg.EmitParentEnvironment();
                }
            }

            ncg.CreateClosureAccessSlots(parentEnv);
            ncg.CreateReferenceSlots();

            // Emit the generator body 
            ncg.EmitGenerator(block);

            flowedContext.EmitGet(_ilg);
            EmitDelegateConstruction(ncg, block.DelegateType, _info.IsClosure || _info.HasEnvironment);
            _ilg.EmitNew(block.GeneratorType, new Type[] { typeof(CodeContext), block.DelegateType });
        }

        private static string GetGeneratorMethodName(string name) {
            return name + "$g" + _GeneratorCounter++;
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

        /// <summary>
        /// Allocate variables in the current lambda
        /// </summary>
        private void CreateVariableStorage() {
            foreach (VariableInfo vi in _info.Variables.Values) {
                vi.Storage = Allocate(vi);
            }
        }

        /// <summary>
        /// Allocates a variable in the current lambda
        /// 
        /// This is needed mainly for variables lifted into the environment; we
        /// need to have the VariabelInfo remember which index it was in the
        /// environment tuple.
        /// </summary>
        /// <param name="vi"></param>
        private Storage Allocate(VariableInfo vi) {
            Debug.Assert(vi.LambdaInfo == _info);
            Debug.Assert(vi.Storage == null);

            Storage storage;
            switch (vi.Variable.NodeType) {
                case AstNodeType.LocalVariable:
                    // TODO: don't special case top level lambda
                    if (vi.Lift && !_info.Lambda.IsGlobal) {
                        storage = _info.EnvironmentAllocator.AllocateStorage(vi.Variable);
                    } else {
                        // Local on global level, simply allocate the storage
                        storage = _info.LocalAllocator.AllocateStorage(vi.Variable);
                    }
                    break;
                case AstNodeType.Parameter:
                    // Lifting parameter into closure, allocate in env and move.
                    Slot arg = GetLambdaArgumentSlot(vi.ParameterIndex);
                    if (vi.Lift) {
                        storage = _info.EnvironmentAllocator.AllocateStorage(vi.Variable);
                        // Copy the value from the parameter into the environment
                        storage.CreateSlot(EnvironmentSlot).EmitSet(_ilg, arg);
                    } else {
                        storage = new SlotStorage(arg);
                    }
                    break;

                case AstNodeType.GlobalVariable:
                    storage = _info.GlobalAllocator.AllocateStorage(vi.Variable);
                    break;
                case AstNodeType.TemporaryVariable:
                    if (vi.Lift) {
                        storage = _info.EnvironmentAllocator.AllocateStorage(vi.Variable);
                    } else {
                        storage = new SlotStorage(GetNamedLocal(vi.Variable.Type, SymbolTable.IdToString(vi.Name)));
                    }
                    break;
                default: throw Assert.Unreachable;
            }

            return storage;
        }

        // TODO: internal because it's used by Rule.Emit
        internal void CreateReferenceSlots() {
            foreach (Expression v in new List<Expression>(_info.ReferenceSlots.Keys)) {
                _info.ReferenceSlots[v] = CreateSlot(_info.GetVariableInfo(v));
            }
        }

        private Slot CreateSlot(VariableInfo vi) {
            Storage storage = vi.Storage;

            if (storage == null) {
                // Rules don't do a seperate allocate pass, so we can get here
                // but not have storage allocated.
                //
                // Also, we can't save the storage on the VariableInfo because
                // the rule can emit itself under different LambdaCompilers.
                // Yuck!
                //
                // TODO: fix this
                Debug.Assert(vi.LambdaInfo.Lambda == null);
                storage = Allocate(vi);
            }

            // TODO: This is hacky, sometimes we pass the environment tuple, sometimes we pass CodeContext
            // Sometimes the Storage doesn't even care what we pass.
            return storage.CreateSlot(vi.Lift ? _info.ClosureAccess[vi.LambdaInfo.Lambda] : ContextSlot);
        }

        #endregion
    }
}
