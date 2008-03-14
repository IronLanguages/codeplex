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
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;
using System.Threading;

using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Runtime;
using System.Collections.ObjectModel;

namespace Microsoft.Scripting.Ast {

    /// <summary>
    /// Dynamic Language Runtime Compiler.
    /// This part compiles lambdas.
    /// </summary>
    partial class LambdaCompiler {
        // Should this be in TypeGen directly?
        [MultiRuntimeAware]
        private static int _Counter = 0;
        [MultiRuntimeAware]
        private static int _GeneratorCounter = 0;

        private static readonly string[] _GeneratorSigNames = new string[] { "$gen", "$ret" };

        private void EmitEnvironmentIDs(LambdaExpression lambda) {
            int size = 0;
            foreach (Variable prm in lambda.Parameters) {
                if (prm.Lift) size++;
            }
            foreach (Variable var in lambda.Variables) {
                if (var.Lift) size++;
            }

            if (!IsDynamicMethod) {
                Debug.Assert(TypeGen != null);

                LambdaCompiler cctor = TypeGen.TypeInitializer;
                cctor.EmitEnvironmentIdArray(lambda, size);
                Slot fields = TypeGen.AddStaticField(typeof(SymbolId[]), "__symbolIds$" + lambda.Name + "$" + Interlocked.Increment(ref _Counter));
                fields.EmitSet(cctor);
                fields.EmitGet(this);
            } else {
                EmitEnvironmentIdArray(lambda, size);
            }
        }

        private void EmitEnvironmentIdArray(LambdaExpression lambda, int size) {
            // Create the array for the names
            EmitInt(size);
            Emit(OpCodes.Newarr, typeof(SymbolId));

            int index = 0;
            EmitDebugMarker("--- Environment IDs ---");

            foreach (Variable prm in lambda.Parameters) {
                if (prm.Lift) {
                    EmitSetVariableName(this, index++, prm.Name);
                }
            }

            foreach (Variable var in lambda.Variables) {
                if (var.Lift) {
                    EmitSetVariableName(this, index++, var.Name);
                }
            }
            EmitDebugMarker("--- End Environment IDs ---");
        }

        private static void EmitSetVariableName(LambdaCompiler cg, int index, SymbolId name) {
            cg.Emit(OpCodes.Dup);
            cg.EmitInt(index);
            cg.Emit(OpCodes.Ldelema, typeof(SymbolId));
            cg.EmitSymbolId(name);
            cg.Emit(OpCodes.Call, typeof(SymbolId).GetConstructor(new Type[] { typeof(SymbolId) }));
        }

        private static void CreateEnvironmentFactory(LambdaInfo li, bool generator) {
            if (li.HasEnvironment) {
                LambdaExpression lambda = li.Lambda;

                // Get the environment size
                int size = 0;

                if (generator) {
                    size += li.GeneratorTemps;

                    foreach (Variable var in lambda.Variables) {
                        if (var.IsTemporary) {
                            size++;
                        }
                    }
                }

                foreach (Variable parm in lambda.Parameters) {
                    if (parm.Lift) size++;
                }
                foreach (Variable var in lambda.Variables) {
                    if (var.Lift) size++;
                }

                // Find the right environment factory for the size of elements to store
                li.EnvironmentFactory = CreateEnvironmentFactory(size);
            }
        }

        private static EnvironmentSlot EmitEnvironmentAllocation(LambdaCompiler cg, LambdaInfo li) {
            Debug.Assert(li.EnvironmentFactory != null);

            cg.EmitDebugMarker("-- ENV ALLOC START --");

            li.EnvironmentFactory.EmitStorage(cg);
            cg.Emit(OpCodes.Dup);
            // Store the environment reference in the local
            EnvironmentSlot environmentSlot = li.EnvironmentFactory.CreateEnvironmentSlot(cg);
            environmentSlot.EmitSet(cg);

            // Emit the names array for the environment constructor
            cg.EmitEnvironmentIDs(li.Lambda);
            // Emit code to generate the new instance of the environment

            li.EnvironmentFactory.EmitNewEnvironment(cg);

            cg.EmitDebugMarker("-- ENV ALLOC END --");

            return environmentSlot;
        }

        /// <summary>
        /// Creates a slot for context of type CodeContext from an environment slot.
        /// </summary>
        private static Slot CreateEnvironmentContext(LambdaCompiler cg, bool visible) {
            // update CodeContext so it contains the nested scope for the locals
            //  ctxSlot = new CodeContext(currentCodeContext, locals)
            Slot ctxSlot = cg.GetNamedLocal(typeof(CodeContext), "$frame");
            cg.EnvironmentSlot.EmitGetDictionary(cg);
            cg.EmitCodeContext();
            cg.EmitBoolean(visible);
            cg.EmitCall(typeof(RuntimeHelpers), "CreateNestedCodeContext");
            ctxSlot.EmitSet(cg);
            return ctxSlot;
        }

        private static void CreateSlots(LambdaCompiler cg, LambdaInfo li) {
            Debug.Assert(cg != null);
            Debug.Assert(li != null);

            LambdaExpression lambda = li.Lambda;

            if (li.HasEnvironment) {
                // we're an environment slot, we need our own environment slot, and we're
                // going to update our Context slot to point to a CodeContext which has
                // its Locals pointing at our Environment.
                cg.EnvironmentSlot = EmitEnvironmentAllocation(cg, li);
                cg.ContextSlot = CreateEnvironmentContext(cg, lambda.IsVisible);
            }

            cg.Allocator.Lambda = lambda;

            CreateAccessSlots(cg, li);

            foreach (Variable prm in lambda.Parameters) {
                prm.Allocate(cg, li);
            }
            foreach (Variable var in lambda.Variables) {
                var.Allocate(cg, li);
            }
            foreach (VariableReference r in li.References.Values) {
                r.CreateSlot(cg, li);
                Debug.Assert(r.Slot != null);
            }

            cg.Allocator.LocalAllocator.PrepareForEmit(cg);
            cg.Allocator.GlobalAllocator.PrepareForEmit(cg);
        }

        private static void CreateAccessSlots(LambdaCompiler cg, LambdaInfo li) {
            CreateClosureAccessSlots(cg, li);
            CreateScopeAccessSlots(cg, li);
        }

        private static void CreateClosureAccessSlots(LambdaCompiler cg, LambdaInfo li) {
            ScopeAllocator allocator = cg.Allocator;
            LambdaExpression lambda = li.Lambda;

            // Current context is accessed via environment slot, if any
            if (li.HasEnvironment) {
                allocator.AddClosureAccessSlot(lambda, cg.EnvironmentSlot);
            }

            if (li.IsClosure) {
                Slot scope = cg.GetLocalTmp(typeof(Scope));
                cg.EmitCodeContext();
                cg.EmitPropertyGet(typeof(CodeContext), "Scope");
                if (li.HasEnvironment) {
                    cg.EmitPropertyGet(typeof(Scope), "Parent");
                }
                scope.EmitSet(cg);

                LambdaInfo currentInfo = li;
                LambdaExpression currentLambda = lambda;
                do {
                    LambdaInfo parentInfo = currentInfo.Parent;
                    LambdaExpression parent = parentInfo.Lambda;

                    if (parentInfo.EnvironmentFactory != null) {
                        scope.EmitGet(cg);

                        cg.EmitCall(typeof(RuntimeHelpers).GetMethod("GetTupleDictionaryData").MakeGenericMethod(parentInfo.EnvironmentFactory.StorageType));

                        Slot storage = new LocalSlot(cg.DeclareLocal(parentInfo.EnvironmentFactory.StorageType), cg);
                        storage.EmitSet(cg);
                        allocator.AddClosureAccessSlot(parent, storage);
                    }

                    scope.EmitGet(cg);
                    cg.EmitPropertyGet(typeof(Scope), "Parent");
                    scope.EmitSet(cg);

                    currentLambda = parent;
                    currentInfo = parentInfo;
                } while (currentInfo != null && currentInfo.IsClosure);

                cg.FreeLocalTmp(scope);
            }
        }

        private static void CreateScopeAccessSlots(LambdaCompiler cg, LambdaInfo li) {
            ScopeAllocator allocator = cg.Allocator;
            for (; ; ) {
                if (allocator.Lambda != null) {
                    LambdaInfo abLi = cg.GetLambdaInfo(allocator.Lambda);
                    if (!abLi.IsClosure) {
                        break;
                    }
                }
                allocator = allocator.Parent;
            }

            while (allocator != null) {
                if (allocator.Lambda != null) {
                    foreach (VariableReference reference in li.References.Values) {
                        if (!reference.Variable.Lift && reference.Variable.Lambda == allocator.Lambda) {
                            Slot accessSlot = allocator.LocalAllocator.GetAccessSlot(cg, allocator.Lambda);
                            if (accessSlot != null) {
                                cg.Allocator.AddScopeAccessSlot(allocator.Lambda, accessSlot);
                            }
                            break;
                        }
                    }
                }
                allocator = allocator.Parent;
            }
        }

        /// <summary>
        /// Emits a delegate to the method generated for the LambdaExpression.
        /// May end up creating a wrapper to match the requested delegate type.
        /// </summary>
        /// <param name="lambda">Lambda for which to generate a delegate</param>
        /// <param name="delegateType">Type of the delegate.</param>
        private void EmitDelegateConstruction(LambdaExpression lambda, Type delegateType) {
            FlowChecker.Check(lambda);

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
        private LambdaCompiler CreateWrapperIfNeeded(LambdaCompiler lc, LambdaExpression lambda, Type type) {
            Debug.Assert(typeof(Delegate).IsAssignableFrom(type) && type != typeof(Delegate));

            Type[] blockSig = Ast.GetLambdaSignature(lambda);
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
                wlc.GetArgumentSlot(0).EmitGet(wlc);
            }

            bool paramArray = wrapSig.Length > 0 && CompilerHelpers.IsParamArray(wrapSig[wrapSig.Length - 1]);

            // How many parameters can we simply copy forward?
            int copy = wrapSig.Length;
            if (paramArray) copy --;                // do not copy the last parameter, it is a params array

            int current = 0;                        // the parameter being currently emitted

            //
            // First step is easy, simply copy the parameters that we can copy
            //
            while (current < copy) {
                Slot slot = wlc.GetLambdaArgumentSlot(current);
                slot.EmitGet(wlc);

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
                if (lambda.ParameterArray) extract --;       // do not extract last if the lambda has parameter array too

                int extracting = 0;                         // index into the the wrapper's parameter array

                //
                // Extract the parameters that get passed to underlying method one by one
                // to pass them as actual arguments
                //
                while (extracting < extract) {
                    wrapperArray.EmitGet(wlc);
                    wlc.EmitInt(extracting);
                    wlc.EmitLoadElement(elementType);

                    wlc.EmitCast(elementType, lambdaSig[current]);

                    extracting++; current++;
                }

                //
                // Extract the rest of the array in bulk
                //
                if (lambda.ParameterArray) {
                    wrapperArray.EmitGet(wlc);
                    wlc.EmitInt(extract);               // this is how many we already extracted

                    // Call the helper
                    MethodInfo shifter = typeof(RuntimeHelpers).GetMethod("ShiftParamsArray");
                    shifter = shifter.MakeGenericMethod(elementType);
                    wlc.EmitCall(shifter);

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
            wlc.EmitCall(method);
            wlc.EmitCast(method.ReturnType, wrapInvoke.ReturnType);
            wlc.Emit(OpCodes.Ret);
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
                Debug.Assert(cp == null || cp.SlotType == typeof(object[]), "Closure requires slot type to be object[]");

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
        private static void ComputeSignature(LambdaExpression lambda, out List<Type> paramTypes, out List<SymbolId> paramNames, out string implName) {
            paramTypes = new List<Type>();
            paramNames = new List<SymbolId>();

            int index = 0;

            foreach (Variable p in lambda.Parameters) {
                paramTypes.Add(p.Type);
                paramNames.Add(p.Name);
                p.ParameterIndex = index++;
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
            List<SymbolId> paramNames;
            LambdaCompiler impl;
            string implName;

            // Create the constant pool, if needed
            ConstantPool cp = outer.DynamicMethod ? new ConstantPool() : null;

            // Create the signature
            ComputeSignature(lambda, out paramTypes, out paramNames, out implName);

            // Create the new method & setup its locals
            impl = outer.CreateLambdaCompiler(implName, lambda.ReturnType, paramTypes, SymbolTable.IdsToStrings(paramNames), cp, closure);

            impl.InitializeCompilerAndLambda(outer.Compiler, lambda);

            // TODO: Can this go, if so, closure can be handled completely inside!!!
            if (closure) {
                impl.CreateClosureContextSlot();
            } else {
                impl.ContextSlot = outer.ContextSlot;
            }

            impl.Allocator = CompilerHelpers.CreateLocalStorageAllocator(outer, impl);

            return impl;
        }

        internal void CreateClosureContextSlot() {
            Debug.Assert(_firstLambdaArgument > 0);
            ContextSlot = new FieldSlot(GetArgumentSlot(0), typeof(Closure).GetField("Context"));
        }

        // Called by Compiler
        internal void EmitFunctionImplementation(LambdaInfo li) {
            LambdaExpression lambda = li.Lambda;

            EmitStackTraceTryBlockStart();

            // emit the actual body
            EmitBody(this, li);

            string displayName;

            if (_source != null) {
                displayName = _source.GetSymbolDocument(lambda.Start.Line) ?? lambda.Name;
            } else {
                displayName = lambda.Name;
            }

            EmitStackTraceFaultBlock(lambda.Name, displayName);
        }

        private void EmitStackTraceTryBlockStart() {
            if (ScriptDomainManager.Options.DebugMode && ScriptDomainManager.Options.DynamicStackTraceSupport) {
                // push a try for traceback support
                PushTryBlock();
                BeginExceptionBlock();
            }
        }

        private void EmitStackTraceFaultBlock(string name, string displayName) {
            if (ScriptDomainManager.Options.DebugMode && ScriptDomainManager.Options.DynamicStackTraceSupport) {
                // push a fault block (runs only if there's an exception, doesn't handle the exception)
                PopTargets();
                if (IsDynamicMethod) {
                    BeginCatchBlock(typeof(Exception));
                } else {
                    BeginFaultBlock();
                }

                EmitCodeContext();
                if (IsDynamicMethod) {
                    ConstantPool.AddData(Method).EmitGet(this);
                } else {
                    Emit(OpCodes.Ldtoken, (MethodInfo)Method);
                    EmitCall(typeof(MethodBase), "GetMethodFromHandle", new Type[] { typeof(RuntimeMethodHandle) });
                }
                EmitString(name);
                EmitString(displayName);
                EmitGetCurrentLine();
                EmitCall(typeof(ExceptionHelpers), "UpdateStackTrace");

                // end the exception block
                if (IsDynamicMethod) {
                    Emit(OpCodes.Rethrow);
                }
                EndExceptionBlock();
            }
        }

        // Used by TreeCompiler
        private static void EmitBody(LambdaCompiler cg, LambdaInfo li) {
            GeneratorCodeBlock gcb = li.Lambda as GeneratorCodeBlock;
            if (gcb != null) {
                EmitGeneratorCodeBlockBody(cg, li);
            } else {
                EmitLambdaBody(cg, li);
            }
        }

        private static void EmitLambdaBody(LambdaCompiler cg, LambdaInfo li) {
            LambdaExpression lambda = li.Lambda;

            Debug.Assert(lambda.GetType() == typeof(LambdaExpression));

            CreateEnvironmentFactory(li, false);
            CreateSlots(cg, li);

            cg.EmitBlockStartPosition(lambda);
            cg.EmitExpressionAndPop(lambda.Body);
            cg.EmitBlockEndPosition(lambda);

            // TODO: Skip if Body is guaranteed to return
            if (lambda.ReturnType != typeof(void)) {
                if (TypeUtils.CanAssign(typeof(object), lambda.ReturnType)) {
                    cg.EmitNull();
                } else {
                    cg.EmitMissingValue(lambda.ReturnType);
                }
            }
            cg.EmitReturn();
        }

        private void EmitBlockStartPosition(LambdaExpression lambda) {
            // ensure a break point exists at the top
            // of the file if there isn't a statement
            // flush with the start of the file.
            if (!lambda.Start.IsValid) {
                return;
            }

            ISpan span = lambda.Body as ISpan;
            if (span != null && span.Start.IsValid) {
                if (span.Start != lambda.Start) {
                    EmitPosition(lambda.Start, lambda.Start);
                }
            } else {
                Block body = lambda.Body as Block;
                if (body != null) {
                    for (int i = 0; i < body.Expressions.Count; i++) {
                        span = body.Expressions[i] as ISpan;
                        if (span != null && span.Start.IsValid) {
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
            size++; // +1 for the FunctionEnvironmentDictionary 

            Type[] argTypes = CompilerHelpers.MakeRepeatedArray(typeof(object), size);
            argTypes[0] = typeof(IAttributesCollection);

            Type tupleType = Tuple.MakeTupleType(argTypes);
            Type envType = typeof(FunctionEnvironmentDictionary<>).MakeGenericType(tupleType);

            return new PropertyEnvironmentFactory(tupleType, envType);
        }

        #region GeneratorCodeBlock

        private static void EmitGeneratorCodeBlockBody(LambdaCompiler cg, LambdaInfo li) {
            Debug.Assert(li.Lambda.GetType() == typeof(GeneratorCodeBlock));

            GeneratorCodeBlock gcb = (GeneratorCodeBlock)li.Lambda;

            cg.Allocator.Lambda = gcb;
            CreateEnvironmentFactory(li, true);
            EmitGeneratorBody(cg, li, gcb);
            cg.EmitReturn();
        }

        /// <summary>
        /// Defines the method with the correct signature and sets up the context slot appropriately.
        /// </summary>
        /// <returns></returns>
        private static LambdaCompiler CreateGeneratorLambdaCompiler(LambdaCompiler impl, GeneratorCodeBlock block) {
            // Create the GenerateNext function
            LambdaCompiler ncg = impl.CreateLambdaCompiler(
                GetGeneratorMethodName(block.Name),     // Method Name
                typeof(bool),                           // Return Type
                new Type[] {                            // signature
                    block.GeneratorType,
                    typeof(object).MakeByRefType()
                },
                _GeneratorSigNames,                     // param names
                impl.DynamicMethod ? new ConstantPool() : null,
                false                                   // no closure
            );

            Slot generator = ncg.GetLambdaArgumentSlot(0);
            ncg.ContextSlot = new PropertySlot(generator, typeof(Generator).GetProperty("Context"));

            // Namespace without er factory - all locals must exist ahead of time
            ncg.Allocator = new ScopeAllocator(impl.Allocator, null);
            ncg.Allocator.Lambda = null;       // No scope is active at this point

            // We are emitting generator, mark the Compiler
            ncg.IsGenerator = true;

            // Inherit the compiler
            ncg.InitializeCompilerAndLambda(impl.Compiler, block);

            return ncg;
        }

        /// <summary>
        /// Emits the body of the function that creates a Generator object.  Also creates another
        /// Compiler for the inner method which implements the user code defined in the generator.
        /// </summary>
        private static void EmitGeneratorBody(LambdaCompiler impl, LambdaInfo li, GeneratorCodeBlock block) {
            LambdaCompiler ncg = CreateGeneratorLambdaCompiler(impl, block);
            ncg.EmitLineInfo = impl.EmitLineInfo;
            ncg.Allocator.GlobalAllocator.PrepareForEmit(ncg);

            Slot flowedContext = impl.ContextSlot;
            // If there are no locals in the generator than we don't need the environment
            if (li.HasEnvironment) {
                // Environment creation is emitted into outer function that returns the generator
                // function and then flowed into the generator method on each call via the Generator
                // instance.
                impl.EnvironmentSlot = EmitEnvironmentAllocation(impl, li);
                flowedContext = CreateEnvironmentContext(impl, block.IsVisible);

                InitializeGeneratorEnvironment(impl, li, block);

                // Promote env storage to local variable
                // envStorage = ((FunctionEnvironment)context.Locals).Tuple
                li.EnvironmentFactory.EmitGetStorageFromContext(ncg);

                ncg.EnvironmentSlot = li.EnvironmentFactory.CreateEnvironmentSlot(ncg);
                ncg.EnvironmentSlot.EmitSet(ncg);

                CreateGeneratorTemps(ncg, li);
            }

            CreateReferenceSlots(ncg, li);

            // Emit the generator body 
            EmitGenerator(ncg, li, block);

            flowedContext.EmitGet(impl);
            impl.EmitDelegateConstruction(ncg, block.DelegateType, false);
            impl.EmitNew(block.GeneratorType, new Type[] { typeof(CodeContext), block.DelegateType });
        }

        private static string GetGeneratorMethodName(string name) {
            return name + "$g" + _GeneratorCounter++;
        }

        private static void CreateReferenceSlots(LambdaCompiler cg, LambdaInfo li) {
            CreateAccessSlots(cg, li);
            foreach (VariableReference r in li.References.Values) {
                r.CreateSlot(cg, li);
                Debug.Assert(r.Slot != null);
            }
        }

        private static void CreateGeneratorTemps(LambdaCompiler cg, LambdaInfo li) {
            for (int i = 0; i < li.GeneratorTemps; i++) {
                cg.Allocator.AddGeneratorTemp(
                    li.EnvironmentFactory.MakeEnvironmentReference(
                        SymbolTable.StringToId("temp$" + i)
                    ).CreateSlot(cg.EnvironmentSlot)
                );
            }
        }

        // The slots for generators are created in 2 steps. In the outer function,
        // the slots are allocated, whereas in the actual generator they are CreateSlot'ed
        private static void InitializeGeneratorEnvironment(LambdaCompiler cg, LambdaInfo li, GeneratorCodeBlock block) {
            cg.Allocator.AddClosureAccessSlot(block, cg.EnvironmentSlot);
            foreach (Variable p in block.Parameters) {
                p.Allocate(cg, li);
            }
            foreach (Variable d in block.Variables) {
                d.Allocate(cg, li);
            }
        }

        private static void EmitGenerator(LambdaCompiler ncg, LambdaInfo li, GeneratorCodeBlock block) {
            IList<YieldTarget> topTargets = li.TopTargets;
            Debug.Assert(topTargets != null);

            Label[] jumpTable = new Label[topTargets.Count];
            for (int i = 0; i < jumpTable.Length; i++) {
                jumpTable[i] = topTargets[i].EnsureLabel(ncg);
            }

            Slot router = ncg.GetLocalTmp(typeof(int));
            ncg.EmitGetGeneratorLocation();
            router.EmitSet(ncg);
            ncg.GotoRouter = router;
            router.EmitGet(ncg);
            ncg.Emit(OpCodes.Switch, jumpTable);

            // fall-through on first pass
            // yield statements will insert the needed labels after their returns

            ncg.EmitExpression(block.Body);

            // fall-through is almost always possible in generators, so this
            // is almost always needed
            ncg.EmitReturnInGenerator(null);
            ncg.Finish();

            ncg.GotoRouter = null;
            ncg.FreeLocalTmp(router);
        }

        #endregion

    }
}
