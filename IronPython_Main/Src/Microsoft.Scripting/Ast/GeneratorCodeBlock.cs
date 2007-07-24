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
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Threading;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Ast {
    /// <summary>
    /// Generator code block (code block with yield statements).
    /// 
    /// To create the generator, the AST node requires 2 types.
    /// First is the type of the generator object. The code generation will emit code to instantiate
    /// this type using constructor GeneratorType(CodeContext, DelegateType)
    /// 
    /// The GeneratorType must inherit from Generator
    /// The second type is the Delegate type in the above constructor call.
    /// 
    /// The inner function of the generator will have the signature:
    /// bool GetNext(GeneratorType, out object value);
    /// </summary>
    public class GeneratorCodeBlock : CodeBlock {
        /// <summary>
        /// The type of the generator instance.
        /// The CodeBlock will emit code to create a new instance of this type, using constructor:
        /// GeneratorType(CodeContext context, Delegate next);
        /// </summary>
        private readonly Type _generator;
        /// <summary>
        /// The type of the delegate to produce the next element.
        /// </summary>
        private readonly Type _next;

        private List<YieldTarget> _topTargets;

        private static int _Counter = 0;
        private static string[] _GeneratorSigNames = new string[] { "$gen", "$ret" };

        // FastEval: Cache for emitted delegate so that we only generate code once.
        //private WeakObject<Delegate> _delegate;
        private Delegate _delegate;

        public GeneratorCodeBlock(SourceSpan span, string name, Type generator, Type next)
            : base(span, name) {
            if (generator == null) throw new ArgumentNullException("generator");
            if (next == null) throw new ArgumentNullException("next");
            if (!typeof(Generator).IsAssignableFrom(generator)) throw new ArgumentException("The generator type must inherit from Generator");

            _generator = generator;
            _next = next;
        }

        // Return a delegate to an emitted codeblock, since we lack the ability to execute generator code blocks in interpreted mode.
        public override Delegate GetDelegateForInterpreter(CodeContext context, bool forceWrapperMethod) {
             lock (this) {
                if (_delegate == null) {
                    FlowChecker.Check(this);

                    bool createWrapperMethod = ParameterArray ? false : forceWrapperMethod || NeedsWrapperMethod(false);
                    
                    List<Type> paramTypes = new List<Type>();
                    if (createWrapperMethod) {
                        paramTypes.Add(typeof(object[]));
                    } else {
                        int paramIndex = 0;
                        foreach (Variable v in Parameters) {
                            paramTypes.Add(v.Type);
                            v.ParameterIndex = paramIndex++;
                        }
                    }

                    CodeGen cg = CompilerHelpers.CreateDynamicCodeGenerator(
                        "$generator" + Interlocked.Increment(ref _Counter),
                        typeof(object),
                        paramTypes.ToArray(),
                        new ConstantPool());
                    cg.FastEval = true;
                    // Use the constant pool to propagate our context into the CodeGen we are creating
                    cg.ContextSlot = cg.ConstantPool.AddData(context);
                    cg.Allocator = CompilerHelpers.CreateFrameAllocator(cg.ContextSlot);
                    cg.Binder = context.LanguageContext.Binder;
                    cg.EnvironmentSlot = new EnvironmentSlot(
                        new PropertySlot(
                            new PropertySlot(cg.ContextSlot,
                                typeof(CodeContext).GetProperty("Scope")),
                            typeof(Scope).GetProperty("Dict"))
                        );

                    EmitFunctionImplementation(cg);
                    cg.Finish();

                    Delegate d;
                    if (createWrapperMethod) {
                        throw new NotImplementedException("Wrapper methods not implemented for generators in FastEval mode");
                        /*
                        CallTargetN impl = (CallTargetN)cg.CreateDelegate(typeof(CallTargetN));
                        d = new CallTargetWithContextN(delegate(CodeContext targetContext, object[] args) {
                            // TODO: flow context?
                            return impl.Invoke(args);
                        });*/
                    } else if (ParameterArray) {
                        throw new NotImplementedException("ParameterArray not implemented for generators in FastEval mode");
                    } else {
                        d = cg.CreateDelegate(CallTargets.GetTargetType(false, Parameters.Count));
                    }
                    _delegate = d;
                }
            }
            return _delegate;
        }
        
        public override void EmitBody(CodeGen cg) {
            cg.Allocator.ActiveScope = this;
            CreateEnvironmentFactory(true);
            EmitGeneratorBody(cg);
            cg.EmitReturn();
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                Body.Walk(walker);
            }
            walker.PostWalk(this);
        }

        /// <summary>
        /// Defines the method with the correct signature and sets up the context slot appropriately.
        /// </summary>
        /// <returns></returns>
        private CodeGen CreateMethod(CodeGen _impl) {
            // Create the GenerateNext function
            CodeGen ncg = _impl.DefineMethod(
                GetGeneratorMethodName(),               // Method Name
                typeof(bool),                           // Return Type
                new Type[] {                            // signature
                    _generator,
                    typeof(object).MakeByRefType()
                },
                _GeneratorSigNames,                     // param names
                GetStaticDataForBody(_impl));

            Slot generator = ncg.GetArgumentSlot(0);
            ncg.ContextSlot = new PropertySlot(generator, typeof(Generator).GetProperty("Context"));

            // Namespace without er factory - all locals must exist ahead of time
            ncg.Allocator = new ScopeAllocator(_impl.Allocator, null);
            ncg.Allocator.ActiveScope = null;       // No scope is active at this point

            // We are emitting generator, mark the CodeGen
            ncg.IsGenerator = true;

            if (_impl.FastEval) {
                ncg.Binder = _impl.Binder;
                ncg.FastEval = true;
            }

            return ncg;
        }

        /// <summary>
        /// Emits the body of the function that creates a Generator object.  Also creates another
        /// CodeGen for the inner method which implements the user code defined in the generator.
        /// </summary>
        private void EmitGeneratorBody(CodeGen _impl) {
            CodeGen ncg = CreateMethod(_impl);
            ncg.EmitLineInfo = _impl.EmitLineInfo;

            ncg.Allocator.GlobalAllocator.PrepareForEmit(ncg);

            Slot flowedContext = _impl.ContextSlot;
            // If there are no locals in the generator than we don't need the environment
            if (HasEnvironment) {
                // Environment creation is emitted into outer function that returns the generator
                // function and then flowed into the generator method on each call via the Generator
                // instance.
                _impl.EnvironmentSlot = EmitEnvironmentAllocation(_impl);
                flowedContext = CreateEnvironmentContext(_impl);

                InitializeGeneratorEnvironment(_impl);

                // Promote env storage to local variable
                // envStorage = ((FunctionEnvironment)context.Locals).Tuple
                EnvironmentFactory.EmitGetStorageFromContext(ncg);

                ncg.EnvironmentSlot = EnvironmentFactory.CreateEnvironmentSlot(ncg);
                ncg.EnvironmentSlot.EmitSet(ncg);

                CreateGeneratorTemps(ncg);
            }

            CreateReferenceSlots(ncg);

            // Emit the generator body 
            EmitGenerator(ncg);

            flowedContext.EmitGet(_impl);
            _impl.EmitDelegateConstruction(ncg, _next);
            _impl.EmitNew(_generator, new Type[] { typeof(CodeContext), _next });
        }

        private string GetGeneratorMethodName() {
            return Name + "$g" + _Counter++;
        }

        private void CreateReferenceSlots(CodeGen cg) {
            CreateOuterScopeAccessSlots(cg);
            foreach (VariableReference r in References) {
                r.CreateSlot(cg);
                Debug.Assert(r.Slot != null);
            }
        }

        private void CreateGeneratorTemps(CodeGen cg) {
            for (int i = 0; i < GeneratorTemps; i++) {
                cg.Allocator.AddGeneratorTemp(EnvironmentFactory.MakeEnvironmentReference(SymbolTable.StringToId("temp$" + i)).CreateSlot(cg.EnvironmentSlot));
            }
        }

        // The slots for generators are created in 2 steps. In the outer function,
        // the slots are allocated, whereas in the actual generator they are CreateSlot'ed
        private void InitializeGeneratorEnvironment(CodeGen cg) {
            cg.Allocator.AddScopeAccessSlot(this, cg.EnvironmentSlot);
            foreach (Variable p in Parameters) {
                p.Allocate(cg);
            }
            foreach (Variable d in Variables) {
                d.Allocate(cg);
            }
        }

        private void EmitGenerator(CodeGen ncg) {
            Debug.Assert(_topTargets != null);

            Label[] jumpTable = new Label[_topTargets.Count];
            for (int i = 0; i < jumpTable.Length; i++) {
                jumpTable[i] = _topTargets[i].EnsureLabel(ncg);
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
            Body.Emit(ncg);
            // fall-through is almost always possible in generators, so this
            // is almost always needed
            ncg.EmitReturnInGenerator(null);
            ncg.Finish();

            ncg.GotoRouter = null;
            ncg.FreeLocalTmp(router);
        }

        internal int BuildYieldTargets() {
            Debug.Assert(_topTargets == null);
            int temps;
            YieldLabelBuilder.BuildYieldTargets(this, out _topTargets, out temps);
            return temps;
        }
    }
}
