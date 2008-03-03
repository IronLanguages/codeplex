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
    /// This part compiles code blocks.
    /// </summary>
    partial class LambdaCompiler {
        // Should this be in TypeGen directly?
        [MultiRuntimeAware]
        private static int _Counter = 0;
        [MultiRuntimeAware]
        private static int _GeneratorCounter = 0;

        private static readonly string[] _GeneratorSigNames = new string[] { "$gen", "$ret" };

        private void EmitEnvironmentIDs(CodeBlock block) {
            int size = 0;
            foreach (Variable prm in block.Parameters) {
                if (prm.Lift) size++;
            }
            foreach (Variable var in block.Variables) {
                if (var.Lift) size++;
            }

            if (!IsDynamicMethod) {
                Debug.Assert(TypeGen != null);

                LambdaCompiler cctor = TypeGen.TypeInitializer;
                cctor.EmitEnvironmentIdArray(block, size);
                Slot fields = TypeGen.AddStaticField(typeof(SymbolId[]), "__symbolIds$" + block.Name + "$" + Interlocked.Increment(ref _Counter));
                fields.EmitSet(cctor);
                fields.EmitGet(this);
            } else {
                EmitEnvironmentIdArray(block, size);
            }
        }

        private void EmitEnvironmentIdArray(CodeBlock block, int size) {
            // Create the array for the names
            EmitInt(size);
            Emit(OpCodes.Newarr, typeof(SymbolId));

            int index = 0;
            EmitDebugMarker("--- Environment IDs ---");

            foreach (Variable prm in block.Parameters) {
                if (prm.Lift) {
                    EmitSetVariableName(this, index++, prm.Name);
                }
            }

            foreach (Variable var in block.Variables) {
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

        private static void CreateEnvironmentFactory(CodeBlockInfo cbi, bool generator) {
            if (cbi.HasEnvironment) {
                CodeBlock cb = cbi.CodeBlock;

                // Get the environment size
                int size = 0;

                if (generator) {
                    size += cbi.GeneratorTemps;

                    foreach (Variable var in cb.Variables) {
                        if (var.IsTemporary) {
                            size++;
                        }
                    }
                }

                foreach (Variable parm in cb.Parameters) {
                    if (parm.Lift) size++;
                }
                foreach (Variable var in cb.Variables) {
                    if (var.Lift) size++;
                }

                // Find the right environment factory for the size of elements to store
                cbi.EnvironmentFactory = CreateEnvironmentFactory(size);
            }
        }

        private static EnvironmentSlot EmitEnvironmentAllocation(LambdaCompiler cg, CodeBlockInfo cbi) {
            Debug.Assert(cbi.EnvironmentFactory != null);

            cg.EmitDebugMarker("-- ENV ALLOC START --");

            cbi.EnvironmentFactory.EmitStorage(cg);
            cg.Emit(OpCodes.Dup);
            // Store the environment reference in the local
            EnvironmentSlot environmentSlot = cbi.EnvironmentFactory.CreateEnvironmentSlot(cg);
            environmentSlot.EmitSet(cg);

            // Emit the names array for the environment constructor
            cg.EmitEnvironmentIDs(cbi.CodeBlock);
            // Emit code to generate the new instance of the environment

            cbi.EnvironmentFactory.EmitNewEnvironment(cg);

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

        private static void CreateSlots(LambdaCompiler cg, CodeBlockInfo cbi) {
            Debug.Assert(cg != null);
            Debug.Assert(cbi != null);

            CodeBlock cb = cbi.CodeBlock;

            if (cbi.HasEnvironment) {
                // we're an environment slot, we need our own environment slot, and we're
                // going to update our Context slot to point to a CodeContext which has
                // its Locals pointing at our Environment.
                cg.EnvironmentSlot = EmitEnvironmentAllocation(cg, cbi);
                cg.ContextSlot = CreateEnvironmentContext(cg, cb.IsVisible);
            }

            cg.Allocator.Block = cb;

            CreateAccessSlots(cg, cbi);

            foreach (Variable prm in cb.Parameters) {
                prm.Allocate(cg, cbi);
            }
            foreach (Variable var in cb.Variables) {
                var.Allocate(cg, cbi);
            }
            foreach (VariableReference r in cbi.References.Values) {
                r.CreateSlot(cg, cbi);
                Debug.Assert(r.Slot != null);
            }

            cg.Allocator.LocalAllocator.PrepareForEmit(cg);
            cg.Allocator.GlobalAllocator.PrepareForEmit(cg);
        }

        private static void CreateAccessSlots(LambdaCompiler cg, CodeBlockInfo cbi) {
            CreateClosureAccessSlots(cg, cbi);
            CreateScopeAccessSlots(cg, cbi);
        }

        private static void CreateClosureAccessSlots(LambdaCompiler cg, CodeBlockInfo cbi) {
            ScopeAllocator allocator = cg.Allocator;
            CodeBlock cb = cbi.CodeBlock;

            // Current context is accessed via environment slot, if any
            if (cbi.HasEnvironment) {
                allocator.AddClosureAccessSlot(cb, cg.EnvironmentSlot);
            }

            if (cbi.IsClosure) {
                Slot scope = cg.GetLocalTmp(typeof(Scope));
                cg.EmitCodeContext();
                cg.EmitPropertyGet(typeof(CodeContext), "Scope");
                if (cbi.HasEnvironment) {
                    cg.EmitPropertyGet(typeof(Scope), "Parent");
                }
                scope.EmitSet(cg);

                CodeBlockInfo currentInfo = cbi;
                CodeBlock current = cb;
                do {
                    CodeBlockInfo parentInfo = currentInfo.Parent;
                    CodeBlock parent = parentInfo.CodeBlock;

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

                    current = parent;
                    currentInfo = parentInfo;
                } while (currentInfo != null && currentInfo.IsClosure);

                cg.FreeLocalTmp(scope);
            }
        }

        private static void CreateScopeAccessSlots(LambdaCompiler cg, CodeBlockInfo cbi) {
            ScopeAllocator allocator = cg.Allocator;
            for (; ; ) {
                if (allocator == null) {
                    // TODO: interpreted mode anomaly
                    break;
                }
                if (allocator.Block != null) {
                    CodeBlockInfo abCbi = cg.GetCbi(allocator.Block);
                    if (!abCbi.IsClosure) {
                        break;
                    }
                }
                allocator = allocator.Parent;
            }

            while (allocator != null) {
                if (allocator.Block != null) {
                    foreach (VariableReference reference in cbi.References.Values) {
                        if (!reference.Variable.Lift && reference.Variable.Block == allocator.Block) {
                            Slot accessSlot = allocator.LocalAllocator.GetAccessSlot(cg, allocator.Block);
                            if (accessSlot != null) {
                                cg.Allocator.AddScopeAccessSlot(allocator.Block, accessSlot);
                            }
                            break;
                        }
                    }
                }
                allocator = allocator.Parent;
            }
        }

        // Used by Interpreter. Move to CompilerHelpers.
        internal static bool NeedsWrapperMethod(CodeBlock block, bool hasContextParameter, bool hasThis, bool stronglyTyped) {
            if (stronglyTyped) {
                // strongly typed delegate signature includes the context explicitly, block parameters don't:
                return block.Parameters.Count + (hasContextParameter ? 1 : 0) > ReflectionUtils.MaxSignatureSize;
            } else {
                // call-target signature includes both the context and 'this' parameter implicitly, block parameters don't include context but they include 'this' parameter:
                return block.Parameters.Count - (hasThis ? 1 : 0) > CallTargets.MaximumCallArgs;
            }
        }

        private static void EmitDelegateConstruction(LambdaCompiler cg, CodeBlock block, bool forceWrapperMethod, bool stronglyTyped, Type delegateType) {
            FlowChecker.Check(block);

            CodeBlockInfo cbi = cg.GetCbi(block);

            bool needsClosure = cbi.IsClosure || !(cg.ContextSlot is StaticFieldSlot);
            bool hasThis = block.HasThis();
            bool needsWrapper = block.ParameterArray ? false : (forceWrapperMethod || NeedsWrapperMethod(block, needsClosure, hasThis, stronglyTyped));

            LambdaCompiler impl = cg.Compiler.ProvideCodeBlockImplementation(cg, block, needsClosure, hasThis);

            // if the method has more than our maximum # of args wrap
            // it in a method that takes an object[] instead.
            if (needsWrapper) {
                impl = impl.MakeWrapperMethodN(hasThis);
                impl.Finish();

                if (delegateType == null) {
                    delegateType = GetWrapperDelegateType(hasThis);
                }
            } else if (block.ParameterArray) {
                if (delegateType == null) {
                    delegateType = GetWrapperDelegateType(hasThis);
                }
            } else {
                if (delegateType == null) {
                    if (stronglyTyped) {
                        delegateType = ReflectionUtils.GetDelegateType(GetParameterTypes(block), block.ReturnType);
                    } else {
                        delegateType = CallTargets.GetTargetType(block.Parameters.Count - (hasThis ? 1 : 0), hasThis);
                    }
                }
            }

            cg.EmitSequencePointNone();
            cg.EmitDelegateConstruction(impl, delegateType, needsClosure);
        }

        private static Type GetWrapperDelegateType(bool hasThis) {
            return hasThis ? typeof(CallTargetWithThisN) : typeof(CallTargetN);
        }

        private static Type[] GetParameterTypes(CodeBlock block) {
            ReadOnlyCollection<Variable> parameters = block.Parameters;
            Type[] result = new Type[parameters.Count];
            for (int i = 0; i < parameters.Count; i++) {
                result[i] = parameters[i].Type;
            }
            return result;
        }

        /// <summary>
        /// Creates the signature for the actual CLR method to create. The base types come from the
        /// lambda/CodeBlock (or its wrapper method), this method may pre-pend an argument to hold
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

        // Used by Interpreter and TreeCompiler
        internal static void ComputeSignature(CodeBlock block, bool hasThis, out List<Type> paramTypes, out List<SymbolId> paramNames, out string implName) {
            paramTypes = new List<Type>();
            paramNames = new List<SymbolId>();

            int parameterIndex = 0;

            if (block.ParameterArray) {
                int startIndex = 0;
                if (hasThis) {
                    paramTypes.Add(typeof(Object));
                    paramNames.Add(SymbolTable.StringToId("$this"));
                    block.Parameters[0].ParameterIndex = parameterIndex++;
                    startIndex = 1;
                }

                paramTypes.Add(typeof(object[]));
                paramNames.Add(SymbolTable.StringToId("$params"));

                for (int index = startIndex; index < block.Parameters.Count; index++) {
                    block.Parameters[index].ParameterIndex = index - startIndex;
                }
            } else {
                foreach (Variable p in block.Parameters) {
                    paramTypes.Add(p.Type);
                    paramNames.Add(p.Name);
                    p.ParameterIndex = parameterIndex++;
                }
            }

            implName = GetGeneratedName(block.Name);
        }

        // Used by Interpreter. Doesn't do much in a way of compilation.
        internal static void ComputeDelegateSignature(CodeBlock block, Type delegateType, out List<Type> paramTypes, out List<SymbolId> paramNames, out string implName) {
            implName = GetGeneratedName(block.Name);

            MethodInfo invoke = delegateType.GetMethod("Invoke");
            ParameterInfo[] pis = invoke.GetParameters();
            paramNames = new List<SymbolId>();
            paramTypes = new List<Type>();

            foreach (ParameterInfo pi in pis) {
                paramTypes.Add(pi.ParameterType);
                paramNames.Add(SymbolTable.StringToId(pi.Name));
            }
        }

        private static string GetGeneratedName(string prefix) {
            return prefix + "$" + Interlocked.Increment(ref _Counter);
        }

        /// <summary>
        /// Defines the method with the correct signature and sets up the context slot appropriately.
        /// </summary>
        internal static LambdaCompiler CreateCodeBlockCompiler(LambdaCompiler outer, CodeBlock block, bool closure, bool hasThis) {
            List<Type> paramTypes;
            List<SymbolId> paramNames;
            LambdaCompiler impl;
            string implName;

            // Create the constant pool, if needed
            ConstantPool cp = outer.DynamicMethod ? new ConstantPool() : null;

            // Create the signature
            ComputeSignature(block, hasThis, out paramTypes, out paramNames, out implName);

            // Create the new method & setup its locals
            impl = outer.CreateLambdaCompiler(implName, block.ReturnType, paramTypes, SymbolTable.IdsToStrings(paramNames), cp, closure);

            impl.InitializeCompilerAndBlock(outer.Compiler, block);
            if (closure) {
                impl.CreateClosureContextSlot();
            } else {
                impl.ContextSlot = outer.ContextSlot;
            }

            if (block.ParameterArray) {
                impl.ParamsSlot = impl.GetLambdaArgumentSlot(paramTypes.Count - 1);
            }

            impl.Allocator = CompilerHelpers.CreateLocalStorageAllocator(outer, impl);

            return impl;
        }

        internal void CreateClosureContextSlot() {
            Debug.Assert(_firstLambdaArgument > 0);
            ContextSlot = new FieldSlot(GetArgumentSlot(0), typeof(Closure).GetField("Context"));
        }

        /// <summary>
        /// Creates a wrapper method for the user-defined function. This allows us to use the
        /// CallTargetN delegate against the function when we don't have a CallTarget# which is
        /// large enough.
        /// 
        /// Used by Interpreter.
        /// </summary>
        internal LambdaCompiler MakeWrapperMethodN(bool needsThis) {
            LambdaCompiler wrapper;
            Slot argSlot;
            ConstantPool staticData = ConstantPool.IsBound ? ConstantPool.CopyData() : null;

            //
            // Calculate size
            //
            int count = 1;                  // typeof(object[]) is always present
            if (needsThis) count++;         // this

            Type[] parameters = new Type[count];

            //
            // Populate the array
            //
            int index = 0;
            if (needsThis) parameters[index++] = typeof(object);
            parameters[index] = typeof(object[]);

            //
            // Create the LambdaCompiler
            //
            wrapper = CreateLambdaCompiler(Method.Name, typeof(object), parameters, null, staticData, _firstLambdaArgument > 0);

            Debug.Assert(_firstLambdaArgument == wrapper._firstLambdaArgument);

            int keep = count + _firstLambdaArgument - 1;
            argSlot = wrapper.GetArgumentSlot(keep);

            int arg = 0;
            while (arg < keep) {
                wrapper.GetArgumentSlot(arg++).EmitGet(wrapper);
            }

            int arrayIndex = 0;
            while (arg < GetArgumentSlotCount()) {
                argSlot.EmitGet(wrapper);
                wrapper.EmitInt(arrayIndex++);
                wrapper.Emit(OpCodes.Ldelem_Ref);

                Slot argumentSlot = GetArgumentSlot(arg++);
                if (argumentSlot.Type != typeof(object)) {
                    wrapper.EmitCast(typeof(object), argumentSlot.Type);
                }
            }

            wrapper.EmitCall((MethodInfo)Method);
            wrapper.Emit(OpCodes.Ret);
            return wrapper;
        }

        // Called by Compiler
        internal void EmitFunctionImplementation(CodeBlockInfo block) {
            CodeBlock cb = block.CodeBlock;

            EmitStackTraceTryBlockStart();

            // emit the actual body
            EmitBody(this, block);

            string displayName;

            if (_source != null) {
                displayName = _source.GetSymbolDocument(cb.Start.Line) ?? cb.Name;
            } else {
                displayName = cb.Name;
            }

            EmitStackTraceFaultBlock(cb.Name, displayName);
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
        private static void EmitBody(LambdaCompiler cg, CodeBlockInfo cbi) {
            GeneratorCodeBlock gcb = cbi.CodeBlock as GeneratorCodeBlock;
            if (gcb != null) {
                EmitGeneratorCodeBlockBody(cg, cbi);
            } else {
                EmitCodeBlockBody(cg, cbi);
            }
        }

        private static void EmitCodeBlockBody(LambdaCompiler cg, CodeBlockInfo cbi) {
            CodeBlock cb = cbi.CodeBlock;

            Debug.Assert(cb.GetType() == typeof(CodeBlock));

            CreateEnvironmentFactory(cbi, false);
            CreateSlots(cg, cbi);
            if (cg.InterpretedMode) {
                foreach (VariableReference vr in cbi.References.Values) {
                    if (vr.Variable.Kind == VariableKind.Local && vr.Variable.Block == cb) {
                        vr.Slot.EmitSetUninitialized(cg);
                    }
                }
            }

            cg.EmitBlockStartPosition(cb);

            cg.EmitExpressionAndPop(cb.Body);

            cg.EmitBlockEndPosition(cb);

            // TODO: Skip if Body is guaranteed to return
            if (cb.ReturnType != typeof(void)) {
                if (TypeUtils.CanAssign(typeof(object), cb.ReturnType)) {
                    cg.EmitNull();
                } else {
                    cg.EmitMissingValue(cb.ReturnType);
                }
            }
            cg.EmitReturn();
        }

        private void EmitBlockStartPosition(CodeBlock block) {
            // ensure a break point exists at the top
            // of the file if there isn't a statement
            // flush with the start of the file.
            if (!block.Start.IsValid) {
                return;
            }

            ISpan span = block.Body as ISpan;
            if (span != null && span.Start.IsValid) {
                if (span.Start != block.Start) {
                    EmitPosition(block.Start, block.Start);
                }
            } else {
                Block body = block.Body as Block;
                if (body != null) {
                    for (int i = 0; i < body.Expressions.Count; i++) {
                        span = body.Expressions[i] as ISpan;
                        if (span != null && span.Start.IsValid) {
                            if (span.Start != block.Start) {
                                EmitPosition(block.Start, block.Start);
                            }
                            break;
                        }
                    }
                }
            }
        }

        private void EmitBlockEndPosition(CodeBlock block) {
            // ensure we emit a sequence point at the end
            // so the user can inspect any info before exiting
            // the function.  Also make sure additional code
            // isn't associated with this function.
            EmitPosition(block.End, block.End);
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

        private static void EmitGeneratorCodeBlockBody(LambdaCompiler cg, CodeBlockInfo cbi) {
            Debug.Assert(cbi.CodeBlock.GetType() == typeof(GeneratorCodeBlock));

            GeneratorCodeBlock gcb = (GeneratorCodeBlock)cbi.CodeBlock;

            if (!cg.HasAllocator) {
                // In the interpreted case, we do not have an allocator yet
                Debug.Assert(cg.InterpretedMode);
                cg.Allocator = CompilerHelpers.CreateFrameAllocator();
            }

            cg.Allocator.Block = gcb;
            CreateEnvironmentFactory(cbi, true);
            EmitGeneratorBody(cg, cbi, gcb);
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
            ncg.Allocator.Block = null;       // No scope is active at this point

            // We are emitting generator, mark the Compiler
            ncg.IsGenerator = true;

            // Inherit the compiler
            ncg.InitializeCompilerAndBlock(impl.Compiler, block);

            return ncg;
        }

        /// <summary>
        /// Emits the body of the function that creates a Generator object.  Also creates another
        /// Compiler for the inner method which implements the user code defined in the generator.
        /// </summary>
        private static void EmitGeneratorBody(LambdaCompiler impl, CodeBlockInfo cbi, GeneratorCodeBlock block) {
            LambdaCompiler ncg = CreateGeneratorLambdaCompiler(impl, block);
            ncg.EmitLineInfo = impl.EmitLineInfo;
            ncg.Allocator.GlobalAllocator.PrepareForEmit(ncg);

            Slot flowedContext = impl.ContextSlot;
            // If there are no locals in the generator than we don't need the environment
            if (cbi.HasEnvironment) {
                // Environment creation is emitted into outer function that returns the generator
                // function and then flowed into the generator method on each call via the Generator
                // instance.
                impl.EnvironmentSlot = EmitEnvironmentAllocation(impl, cbi);
                flowedContext = CreateEnvironmentContext(impl, block.IsVisible);

                InitializeGeneratorEnvironment(impl, cbi, block);

                // Promote env storage to local variable
                // envStorage = ((FunctionEnvironment)context.Locals).Tuple
                cbi.EnvironmentFactory.EmitGetStorageFromContext(ncg);

                ncg.EnvironmentSlot = cbi.EnvironmentFactory.CreateEnvironmentSlot(ncg);
                ncg.EnvironmentSlot.EmitSet(ncg);

                CreateGeneratorTemps(ncg, cbi);
            }

            CreateReferenceSlots(ncg, cbi);

            // Emit the generator body 
            EmitGenerator(ncg, cbi, block);

            flowedContext.EmitGet(impl);
            impl.EmitDelegateConstruction(ncg, block.DelegateType, false);
            impl.EmitNew(block.GeneratorType, new Type[] { typeof(CodeContext), block.DelegateType });
        }

        private static string GetGeneratorMethodName(string name) {
            return name + "$g" + _GeneratorCounter++;
        }

        private static void CreateReferenceSlots(LambdaCompiler cg, CodeBlockInfo cbi) {
            CreateAccessSlots(cg, cbi);
            foreach (VariableReference r in cbi.References.Values) {
                r.CreateSlot(cg, cbi);
                Debug.Assert(r.Slot != null);
            }
        }

        private static void CreateGeneratorTemps(LambdaCompiler cg, CodeBlockInfo block) {
            for (int i = 0; i < block.GeneratorTemps; i++) {
                cg.Allocator.AddGeneratorTemp(
                    block.EnvironmentFactory.MakeEnvironmentReference(
                        SymbolTable.StringToId("temp$" + i)
                    ).CreateSlot(cg.EnvironmentSlot)
                );
            }
        }

        // The slots for generators are created in 2 steps. In the outer function,
        // the slots are allocated, whereas in the actual generator they are CreateSlot'ed
        private static void InitializeGeneratorEnvironment(LambdaCompiler cg, CodeBlockInfo cbi, GeneratorCodeBlock block) {
            cg.Allocator.AddClosureAccessSlot(block, cg.EnvironmentSlot);
            foreach (Variable p in block.Parameters) {
                p.Allocate(cg, cbi);
            }
            foreach (Variable d in block.Variables) {
                d.Allocate(cg, cbi);
            }
        }

        private static void EmitGenerator(LambdaCompiler ncg, CodeBlockInfo cbi, GeneratorCodeBlock block) {
            IList<YieldTarget> topTargets = cbi.TopTargets;
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
