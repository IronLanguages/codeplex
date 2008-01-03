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

namespace Microsoft.Scripting.Ast {

    /// <summary>
    /// Dynamic Language Runtime Compiler.
    /// This part compiles code blocks.
    /// </summary>
    public partial class Compiler {
        // Should this be in TypeGen directly?
        private static int _Counter = 0;
        private static int _GeneratorCounter = 0;

        private static string[] _GeneratorSigNames = new string[] { "$gen", "$ret" };

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

                Compiler cctor = TypeGen.TypeInitializer;
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

        private static void EmitSetVariableName(Compiler cg, int index, SymbolId name) {
            cg.Emit(OpCodes.Dup);
            cg.EmitInt(index);
            cg.Emit(OpCodes.Ldelema, typeof(SymbolId));
            cg.EmitSymbolId(name);
            cg.Emit(OpCodes.Call, typeof(SymbolId).GetConstructor(new Type[] { typeof(SymbolId) }));
        }

        private static void CreateEnvironmentFactory(CodeBlock block, bool generator) {
            if (block.HasEnvironment) {
                // Get the environment size
                int size = 0;

                if (generator) {
                    size += block.GeneratorTemps;

                    foreach (Variable var in block.Variables) {
                        if (var.Kind == Variable.VariableKind.GeneratorTemporary) {
                            size++;
                        }
                    }
                }

                foreach (Variable parm in block.Parameters) {
                    if (parm.Lift) size++;
                }
                foreach (Variable var in block.Variables) {
                    if (var.Lift) size++;
                }

                // Find the right environment factory for the size of elements to store
                block.EnvironmentFactory = CreateEnvironmentFactory(size);
            }
        }

        private static EnvironmentSlot EmitEnvironmentAllocation(Compiler cg, CodeBlock block) {
            Debug.Assert(block.EnvironmentFactory != null);

            cg.EmitDebugMarker("-- ENV ALLOC START --");

            block.EnvironmentFactory.EmitStorage(cg);
            cg.Emit(OpCodes.Dup);
            // Store the environment reference in the local
            EnvironmentSlot environmentSlot = block.EnvironmentFactory.CreateEnvironmentSlot(cg);
            environmentSlot.EmitSet(cg);

            // Emit the names array for the environment constructor
            cg.EmitEnvironmentIDs(block);
            // Emit code to generate the new instance of the environment

            block.EnvironmentFactory.EmitNewEnvironment(cg);

            cg.EmitDebugMarker("-- ENV ALLOC END --");

            return environmentSlot;
        }

        /// <summary>
        /// Creates a slot for context of type CodeContext from an environment slot.
        /// </summary>
        internal static Slot CreateEnvironmentContext(Compiler cg, CodeBlock block) {
            // update CodeContext so it contains the nested scope for the locals
            //  ctxSlot = new CodeContext(currentCodeContext, locals)
            Slot ctxSlot = cg.GetNamedLocal(typeof(CodeContext), "$frame");
            cg.EnvironmentSlot.EmitGetDictionary(cg);
            cg.EmitCodeContext();
            cg.EmitBoolean(block.IsVisible);
            cg.EmitCall(typeof(RuntimeHelpers), "CreateNestedCodeContext");
            ctxSlot.EmitSet(cg);
            return ctxSlot;
        }

        // Used by TreeCompiler
        internal static void CreateSlots(Compiler cg, CodeBlock block) {
            Contract.RequiresNotNull(cg, "cg");

            if (block.HasEnvironment) {
                // we're an environment slot, we need our own environment slot, and we're
                // going to update our Context slot to point to a CodeContext which has
                // its Locals pointing at our Environment.
                cg.EnvironmentSlot = EmitEnvironmentAllocation(cg, block);
                cg.ContextSlot = CreateEnvironmentContext(cg, block);
            }

            cg.Allocator.Block = block;

            CreateAccessSlots(cg, block);

            foreach (Variable prm in block.Parameters) {
                prm.Allocate(cg);
            }
            foreach (Variable var in block.Variables) {
                var.Allocate(cg);
            }
            foreach (VariableReference r in block.References) {
                r.CreateSlot(cg);
                Debug.Assert(r.Slot != null);
            }

            cg.Allocator.LocalAllocator.PrepareForEmit(cg);
            cg.Allocator.GlobalAllocator.PrepareForEmit(cg);
        }

        private static void CreateAccessSlots(Compiler cg, CodeBlock block) {
            CreateClosureAccessSlots(cg, block);
            CreateScopeAccessSlots(cg, block);
        }

        private static void CreateClosureAccessSlots(Compiler cg, CodeBlock block) {
            ScopeAllocator allocator = cg.Allocator;

            // Current context is accessed via environment slot, if any
            if (block.HasEnvironment) {
                allocator.AddClosureAccessSlot(block, cg.EnvironmentSlot);
            }

            if (block.IsClosure) {
                Slot scope = cg.GetLocalTmp(typeof(Scope));
                cg.EmitCodeContext();
                cg.EmitPropertyGet(typeof(CodeContext), "Scope");
                if (block.HasEnvironment) {
                    cg.EmitPropertyGet(typeof(Scope), "Parent");
                }
                scope.EmitSet(cg);

                CodeBlock current = block;
                do {
                    CodeBlock parent = current.Parent;
                    if (parent.EnvironmentFactory != null) {
                        scope.EmitGet(cg);

                        cg.EmitCall(typeof(RuntimeHelpers).GetMethod("GetTupleDictionaryData").MakeGenericMethod(parent.EnvironmentFactory.StorageType));

                        Slot storage = new LocalSlot(cg.DeclareLocal(parent.EnvironmentFactory.StorageType), cg);
                        storage.EmitSet(cg);
                        allocator.AddClosureAccessSlot(parent, storage);
                    }

                    scope.EmitGet(cg);
                    cg.EmitPropertyGet(typeof(Scope), "Parent");
                    scope.EmitSet(cg);

                    current = parent;
                } while (current != null && current.IsClosure);

                cg.FreeLocalTmp(scope);
            }
        }

        private static void CreateScopeAccessSlots(Compiler cg, CodeBlock block) {
            ScopeAllocator allocator = cg.Allocator;
            for (; ; ) {
                if (allocator == null) {
                    // TODO: interpreted mode anomaly
                    break;
                }
                if (allocator.Block != null && !allocator.Block.IsClosure) {
                    break;
                }
                allocator = allocator.Parent;
            }

            while (allocator != null) {
                if (allocator.Block != null) {
                    foreach (VariableReference reference in block.References) {
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
        internal static bool NeedsWrapperMethod(CodeBlock block, bool hasContextParameter, bool stronglyTyped) {
            return block.Parameters.Count + (hasContextParameter ? 1 : 0) >
                (stronglyTyped ? ReflectionUtils.MaxSignatureSize - 1 : CallTargets.MaximumCallArgs);
        }

        private static ConstantPool GetStaticDataForBody(Compiler cg) {
            if (cg.DynamicMethod) {
                return new ConstantPool();
            } else {
                return null;
            }
        }

        private static void EmitDelegateConstruction(Compiler cg, CodeBlock block, bool forceWrapperMethod, bool stronglyTyped, Type delegateType) {
            FlowChecker.Check(block);

            bool hasContextParameter = block.ExplicitCodeContextExpression == null &&
                (block.IsClosure ||
                !(cg.ContextSlot is StaticFieldSlot));

            bool createWrapperMethod = block.ParameterArray ? false : (forceWrapperMethod || NeedsWrapperMethod(block, hasContextParameter, stronglyTyped));

            bool hasThis = block.HasThis();

            cg.EmitSequencePointNone();

            // TODO: storing implementations on code gen doesn't allow blocks being referenced from different methods
            // the implementations should be stored on some kind of Module when available
            Compiler impl = cg.ProvideCodeBlockImplementation(block, hasContextParameter, hasThis);

            // if the method has more than our maximum # of args wrap
            // it in a method that takes an object[] instead.
            if (createWrapperMethod) {
                Compiler wrapper = MakeWrapperMethodN(cg, impl, block, hasThis);
                wrapper.Finish();

                if (delegateType == null) {
                    delegateType = GetWrapperDelegateType(hasThis, hasContextParameter);
                }

                cg.EmitDelegateConstruction(wrapper, delegateType);
            } else if (block.ParameterArray) {
                if (delegateType == null) {
                    delegateType = GetWrapperDelegateType(hasThis, hasContextParameter);
                }
                cg.EmitDelegateConstruction(impl, delegateType);
            } else {
                if (delegateType == null) {
                    if (stronglyTyped) {
                        delegateType = ReflectionUtils.GetDelegateType(GetParameterTypes(block, hasContextParameter), block.ReturnType);
                    } else {
                        delegateType = CallTargets.GetTargetType(hasContextParameter, block.Parameters.Count - (hasThis ? 1 : 0), hasThis);
                    }
                }
                cg.EmitDelegateConstruction(impl, delegateType);
            }
        }

        private static Type GetWrapperDelegateType(bool hasThis, bool hasContextParameter) {
            if (hasContextParameter) {
                return hasThis ? typeof(CallTargetWithContextAndThisN) : typeof(CallTargetWithContextN);
            } else {
                return hasThis ? typeof(CallTargetWithThisN) : typeof(CallTargetN);
            }
        }

        private static Type[] GetParameterTypes(CodeBlock block, bool hasContextParameter) {
            List<Variable> parameters = block.Parameters;
            Type[] result = new Type[parameters.Count + (hasContextParameter ? 1 : 0)];
            int j = 0;
            if (hasContextParameter) {
                result[j++] = typeof(CodeContext);
            }

            for (int i = 0; i < parameters.Count; i++) {
                result[j++] = parameters[i].Type;
            }
            return result;
        }

        // Used by Interpreter and TreeCompiler
        internal static int ComputeSignature(CodeBlock block, bool hasContextParameter, bool hasThis, out List<Type> paramTypes, out List<SymbolId> paramNames, out string implName) {
            paramTypes = new List<Type>();
            paramNames = new List<SymbolId>();

            int parameterIndex = 0;

            if (hasContextParameter) {
                paramTypes.Add(typeof(CodeContext));
                paramNames.Add(SymbolTable.StringToId("$context"));
                parameterIndex = 1;
            }

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

            return parameterIndex;
        }

        // Used by Interpreter
        internal static int ComputeDelegateSignature(CodeBlock block, Type delegateType, out List<Type> paramTypes, out List<SymbolId> paramNames, out string implName) {
            implName = GetGeneratedName(block.Name);

            MethodInfo invoke = delegateType.GetMethod("Invoke");
            ParameterInfo[] pis = invoke.GetParameters();
            paramNames = new List<SymbolId>();
            paramTypes = new List<Type>();

            foreach (ParameterInfo pi in pis) {
                paramTypes.Add(pi.ParameterType);
                paramNames.Add(SymbolTable.StringToId(pi.Name));
            }

            return -1;
        }

        private static string GetGeneratedName(string prefix) {
            return prefix + "$" + Interlocked.Increment(ref _Counter);
        }

        // TODO: Rename To CreateCodeBlockCompiler or something such
        /// <summary>
        /// Defines the method with the correct signature and sets up the context slot appropriately.
        /// </summary>
        /// <returns></returns>
        private static Compiler CreateMethod(Compiler outer, CodeBlock block, bool hasContextParameter, bool hasThis) {
            List<Type> paramTypes = new List<Type>();
            List<SymbolId> paramNames = new List<SymbolId>();
            Compiler impl;
            string implName;

            int lastParamIndex = ComputeSignature(block, hasContextParameter, hasThis, out paramTypes, out paramNames, out implName);

            // create the new method & setup its locals
            impl = outer.DefineMethod(implName, block.ReturnType,
                paramTypes, SymbolTable.IdsToStrings(paramNames), GetStaticDataForBody(outer));

            if (block.ExplicitCodeContextExpression != null) {
                Slot localContextSlot = impl.GetLocalTmp(typeof(CodeContext));

                // cannot access code context slot during emit:
                impl.EmitExpression(block.ExplicitCodeContextExpression);

                localContextSlot.EmitSet(impl);
                impl.ContextSlot = localContextSlot;

            } else {
                impl.ContextSlot = hasContextParameter ? impl.GetArgumentSlot(0) : outer.ContextSlot;
            }

            if (block.ParameterArray) {
                impl.ParamsSlot = impl.GetArgumentSlot(lastParamIndex);
            }

            impl.Allocator = CompilerHelpers.CreateLocalStorageAllocator(outer, impl);

            return impl;
        }

        private static Compiler CreateWrapperCodeGen(Compiler outer, string implName, bool hasContextParameter, bool hasThisParameter, ConstantPool staticData) {
            Compiler result;

            List<Type> paramTypes = new List<Type>(3);
            if (hasContextParameter) paramTypes.Add(typeof(CodeContext));
            if (hasThisParameter) paramTypes.Add(typeof(object));
            paramTypes.Add(typeof(object[]));

            if (outer == null) {
                result = CompilerHelpers.CreateDynamicCodeGenerator(implName, typeof(object), paramTypes, staticData);
            } else {
                result = outer.DefineMethod(implName, typeof(object), paramTypes.ToArray(), null, staticData);
            }

            // DynamicMethod doesn't allow to define params-attribute on the last param. We need to check delegate type in call site.

            return result;
        }

        /// <summary>
        /// Creates a wrapper method for the user-defined function.  This allows us to use the CallTargetN
        /// delegate against the function when we don't have a CallTarget# which is large enough.
        /// Used by Interpreter.
        /// </summary>
        internal static Compiler MakeWrapperMethodN(Compiler outer, Compiler impl, CodeBlock block, bool hasThis) {
            Compiler wrapper;
            Slot contextSlot = null;
            Slot argSlot;
            Slot thisSlot = null;
            ConstantPool staticData = null;

            bool hasContextParameter = impl.ArgumentSlots.Count > 0
                && impl.ArgumentSlots[0].Type == typeof(CodeContext);

            if (impl.ConstantPool.IsBound) {
                staticData = impl.ConstantPool.CopyData();
            }

            string implName = impl.Method.Name;

            wrapper = CreateWrapperCodeGen(outer, implName, hasContextParameter, hasThis, staticData);

            if (hasContextParameter) {
                if (hasThis) {
                    contextSlot = wrapper.GetArgumentSlot(0);
                    thisSlot = wrapper.GetArgumentSlot(1);
                    argSlot = wrapper.GetArgumentSlot(2);
                } else {
                    contextSlot = wrapper.GetArgumentSlot(0);
                    argSlot = wrapper.GetArgumentSlot(1);
                }
            } else {
                // Context weirdness: DynamicMethods need to flow their context, and if we don't
                // have a TypeGen we'll create a DynamicMethod but we won't flow context w/ it.
                Debug.Assert(outer == null || outer.TypeGen != null);
                if (hasThis) {
                    thisSlot = wrapper.GetArgumentSlot(0);
                    argSlot = wrapper.GetArgumentSlot(1);
                } else {
                    argSlot = wrapper.GetArgumentSlot(0);
                }
            }

            if (wrapper.ConstantPool.IsBound) {
                wrapper.ConstantPool.Slot.EmitGet(wrapper);
            }

            if (contextSlot != null) {
                contextSlot.EmitGet(wrapper);
            }

            int startIndex = 0;
            if (thisSlot != null) {
                thisSlot.EmitGet(wrapper);
                startIndex = 1;
            }

            for (int pi = startIndex; pi < block.Parameters.Count; pi++) {
                argSlot.EmitGet(wrapper);
                wrapper.EmitInt(pi - startIndex);
                wrapper.Emit(OpCodes.Ldelem_Ref);
                wrapper.EmitCast(typeof(object), block.Parameters[pi].Type);
            }
            wrapper.EmitCall((MethodInfo)impl.Method);
            wrapper.Emit(OpCodes.Ret);
            return wrapper;
        }

        // Used by OptimizedModuleGenerator and Interpreter
        internal void EmitFunctionImplementation(CodeBlock block) {
            CompilerHelpers.EmitStackTraceTryBlockStart(this);

            // emit the actual body
            EmitBody(this, block);

            string displayName;

            if (this.HasContext) {
                displayName = Context.SourceUnit.GetSymbolDocument(block.Start.Line) ?? block.Name;
            } else {
                displayName = block.Name;
            }

            CompilerHelpers.EmitStackTraceFaultBlock(this, block.Name, displayName);
        }

        // Used by TreeCompiler
        internal static void EmitBody(Compiler cg, CodeBlock block) {
            switch (block.NodeType) {
                case AstNodeType.CodeBlock:
                    EmitCodeBlockBody(cg, block);
                    break;
                case AstNodeType.GeneratorCodeBlock:
                    EmitGeneratorCodeBlockBody(cg, (GeneratorCodeBlock)block);
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        private static void EmitCodeBlockBody(Compiler cg, CodeBlock block) {
            Debug.Assert(block.NodeType == AstNodeType.CodeBlock);

            CreateEnvironmentFactory(block, false);
            CreateSlots(cg, block);
            if (cg.InterpretedMode) {
                foreach (VariableReference vr in block.References) {
                    if (vr.Variable.Kind == Variable.VariableKind.Local && vr.Variable.Block == block) {
                        vr.Slot.EmitSetUninitialized(cg);
                    }
                }
            }

            cg.EmitBlockStartPosition(block);

            cg.EmitStatement(block.Body);

            cg.EmitBlockEndPosition(block);

            //TODO skip if Body is guaranteed to return
            if (block.ReturnType != typeof(void)) {
                if (TypeUtils.CanAssign(typeof(object), block.ReturnType)) {
                    cg.EmitNull();
                } else {
                    cg.EmitMissingValue(block.ReturnType);
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

            if (block.Body.Start.IsValid) {
                if (block.Body.Start != block.Start) {
                    EmitPosition(block.Start, block.Start);
                }
            } else {
                BlockStatement body = block.Body as BlockStatement;
                if (block != null) {
                    for (int i = 0; i < body.Statements.Count; i++) {
                        if (body.Statements[i].Start.IsValid) {
                            if (body.Statements[i].Start != block.Start) {
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

        // Compiler entry point, used by ScriptCode.
        // This is used for compiling the toplevel CodeBlock object.
        internal static T CreateDelegate<T>(CompilerContext context, CodeBlock block)
            where T : class {
            Compiler cg = CompilerHelpers.CreateDynamicCodeGenerator(context);
            cg.Allocator = CompilerHelpers.CreateFrameAllocator();

            cg.EnvironmentSlot = new EnvironmentSlot(
                new PropertySlot(
                    new PropertySlot(cg.ContextSlot,
                        typeof(CodeContext).GetProperty("Scope")),
                    typeof(Scope).GetProperty("Dict"))
                );

            cg.EmitFunctionImplementation(block);

            cg.Finish();

            return (T)(object)cg.CreateDelegate(typeof(T));
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

        private static void EmitGeneratorCodeBlockBody(Compiler cg, GeneratorCodeBlock block) {
            if (!cg.HasAllocator) {
                // In the interpreted case, we do not have an allocator yet
                Debug.Assert(cg.InterpretedMode);
                cg.Allocator = CompilerHelpers.CreateFrameAllocator();
            }

            cg.Allocator.Block = block;
            CreateEnvironmentFactory(block, true);
            EmitGeneratorBody(cg, block);
            cg.EmitReturn();
        }

        /// <summary>
        /// Defines the method with the correct signature and sets up the context slot appropriately.
        /// </summary>
        /// <returns></returns>
        private static Compiler CreateMethod(Compiler _impl, GeneratorCodeBlock block) {
            // Create the GenerateNext function
            Compiler ncg = _impl.DefineMethod(
                GetGeneratorMethodName(block.Name),     // Method Name
                typeof(bool),                           // Return Type
                new Type[] {                            // signature
                    block.GeneratorType,
                    typeof(object).MakeByRefType()
                },
                _GeneratorSigNames,                     // param names
                GetStaticDataForBody(_impl));

            Slot generator = ncg.GetArgumentSlot(0);
            ncg.ContextSlot = new PropertySlot(generator, typeof(Generator).GetProperty("Context"));

            // Namespace without er factory - all locals must exist ahead of time
            ncg.Allocator = new ScopeAllocator(_impl.Allocator, null);
            ncg.Allocator.Block = null;       // No scope is active at this point

            // We are emitting generator, mark the Compiler
            ncg.IsGenerator = true;

            return ncg;
        }

        /// <summary>
        /// Emits the body of the function that creates a Generator object.  Also creates another
        /// Compiler for the inner method which implements the user code defined in the generator.
        /// </summary>
        private static void EmitGeneratorBody(Compiler _impl, GeneratorCodeBlock block) {
            Compiler ncg = CreateMethod(_impl, block);
            ncg.EmitLineInfo = _impl.EmitLineInfo;

            ncg.Allocator.GlobalAllocator.PrepareForEmit(ncg);

            Slot flowedContext = _impl.ContextSlot;
            // If there are no locals in the generator than we don't need the environment
            if (block.HasEnvironment) {
                // Environment creation is emitted into outer function that returns the generator
                // function and then flowed into the generator method on each call via the Generator
                // instance.
                _impl.EnvironmentSlot = EmitEnvironmentAllocation(_impl, block);
                flowedContext = CreateEnvironmentContext(_impl, block);

                InitializeGeneratorEnvironment(_impl, block);

                // Promote env storage to local variable
                // envStorage = ((FunctionEnvironment)context.Locals).Tuple
                block.EnvironmentFactory.EmitGetStorageFromContext(ncg);

                ncg.EnvironmentSlot = block.EnvironmentFactory.CreateEnvironmentSlot(ncg);
                ncg.EnvironmentSlot.EmitSet(ncg);

                CreateGeneratorTemps(ncg, block);
            }

            CreateReferenceSlots(ncg, block);

            // Emit the generator body 
            EmitGenerator(ncg, block);

            flowedContext.EmitGet(_impl);
            _impl.EmitDelegateConstruction(ncg, block.DelegateType);
            _impl.EmitNew(block.GeneratorType, new Type[] { typeof(CodeContext), block.DelegateType });
        }

        private static string GetGeneratorMethodName(string name) {
            return name + "$g" + _GeneratorCounter++;
        }

        private static void CreateReferenceSlots(Compiler cg, GeneratorCodeBlock block) {
            CreateAccessSlots(cg, block);
            foreach (VariableReference r in block.References) {
                r.CreateSlot(cg);
                Debug.Assert(r.Slot != null);
            }
        }

        private static void CreateGeneratorTemps(Compiler cg, GeneratorCodeBlock block) {
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
        private static void InitializeGeneratorEnvironment(Compiler cg, GeneratorCodeBlock block) {
            cg.Allocator.AddClosureAccessSlot(block, cg.EnvironmentSlot);
            foreach (Variable p in block.Parameters) {
                p.Allocate(cg);
            }
            foreach (Variable d in block.Variables) {
                d.Allocate(cg);
            }
        }

        private static void EmitGenerator(Compiler ncg, GeneratorCodeBlock block) {
            IList<YieldTarget> topTargets = block.TopTargets;
            Debug.Assert(topTargets != null);

            Label[] jumpTable = new Label[topTargets.Count];
            for (int i = 0; i < jumpTable.Length; i++) {
                jumpTable[i] = topTargets[i].EnsureLabel(ncg);
            }
            ncg.YieldLabels = jumpTable;

            Slot router = ncg.GetLocalTmp(typeof(int));
            ncg.EmitGetGeneratorLocation();
            router.EmitSet(ncg);
            ncg.GotoRouter = router;
            router.EmitGet(ncg);
            ncg.Emit(OpCodes.Switch, jumpTable);

            // fall-through on first pass
            // yield statements will insert the needed labels after their returns

            ncg.EmitStatement(block.Body);

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