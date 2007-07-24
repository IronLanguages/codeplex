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

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.IO;

using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Actions;

namespace Microsoft.Scripting.Ast {

    /// <summary>
    /// This captures a block of code that should correspond to a .NET method body.  It takes
    /// input through parameters and is expected to be fully bound.  This code can then be
    /// generated in a variety of ways.  The variables can be kept as .NET locals or in a
    /// 1st class environment object.  This is the primary unit used for passing around
    /// AST's in the DLR.
    /// 
    /// TODO - This should probably not be a Node but that will require some substantial walker changes.
    /// </summary>
    public partial class CodeBlock : Node {
        private static int _Counter = 0;

        private readonly Type _returnType;
        private string _name;
        private CodeBlock _parent;
        private Statement _body;

        private readonly List<Variable> _parameters = new List<Variable>();
        private readonly List<Variable> _variables = new List<Variable>();
        private IList<VariableReference> _references;

        private EnvironmentFactory _environmentFactory;

        private int _generatorTemps;

        private bool _isClosure;
        private bool _hasEnvironment;
        private bool _emitLocalDictionary;
        private bool _isGlobal;
        private bool _visibleScope = true;
        private bool _parameterArray;

        /// <summary>
        /// True, if the block is referenced by a declarative reference (CodeBlockExpression).
        /// </summary>
        private bool _declarativeReferenceExists;

        private Expression _explicitCodeContextExpression;

        internal CodeBlock(SourceSpan span, string name, Type returnType)
            : base(span) {
            Utils.Assert.NotNull(returnType);

            _name = name;
            _returnType = returnType;
        }

        internal CodeBlock(SourceSpan span, string name)
            : this(span, name, typeof(object)) {
        }

        public Type ReturnType {
            get { return _returnType; }
        }

        public List<Variable> Parameters {
            get { return _parameters; }
        }

        public string Name {
            get { return _name; }
            set { _name = value; }
        }

        public Expression ExplicitCodeContextExpression {
            get { return _explicitCodeContextExpression; }
            set { _explicitCodeContextExpression = value; }
        }

        /// <summary>
        /// The method refers to a variable in one of its parents lexical context and will need an environment
        /// flown into it.  A function which is a closure does not necessarily contain an Environment unless
        /// it contains additional closures or uses language features which require lifting all locals to
        /// an environment.
        /// </summary>
        internal bool IsClosure {
            get { return _isClosure; }
            set { _isClosure = value; }
        }

        /// <summary>
        /// Scopes with environments will have some locals stored within a dictionary (FunctionEnvironment).  If
        /// we are also a closure an environment is flown into the method and our environment will point to the
        /// parent environment.  Ultimately this will enable our children to get at our or our parents envs.
        /// 
        /// Upon entering a function with an environment a new CodeContext will be allocated with a new
        /// FunctionEnviroment as its locals.  In the case of a generator this new CodeContext and environment
        /// is allocated in the function called to create the Generator, not the function that implements the
        /// Generator body.
        /// 
        /// The environment is provided as the Locals of a CodeContext or in the case of a Generator 
        /// as the parentEnvironment field.
        /// </summary>
        internal bool HasEnvironment {
            get { return _hasEnvironment; }
            set { _hasEnvironment = value; }
        }

        /// <summary>
        /// True to force a function to have an environment and have all of its locals lifted
        /// into this environment.  This provides access to local variables via a dictionary but
        /// comes with the performance penality of not using the real stack for locals.
        /// </summary>
        public bool EmitLocalDictionary {
            get {
                // When custom frames are turned on, we emit dictionaries everywhere
                return ScriptDomainManager.Options.Frames || _emitLocalDictionary;
            }
            set {
                _emitLocalDictionary = value;
            }
        }

        public bool IsGlobal {
            get { return _isGlobal; }
            set { _isGlobal = value; }
        }

        public bool ParameterArray {
            get { return _parameterArray; }
            set { _parameterArray = value; }
        }

        public CodeBlock Parent {
            get { return _parent; }
            set { _parent = value; }
        }

        public bool IsVisible {
            get { return _visibleScope; }
            set { _visibleScope = value; }
        }
        public Statement Body {
            get { return _body; }
            set { _body = value; }
        }

        internal IList<VariableReference> References {
            get { return _references; }
            set { _references = value; }
        }

        public List<Variable> Variables {
            get { return _variables; }
        }

        public Type EnvironmentType {
            get {
                Debug.Assert(_environmentFactory != null);
                return _environmentFactory.EnvironmentType;
            }
        }

        public EnvironmentFactory EnvironmentFactory {
            get { return _environmentFactory; }
        }

        protected internal int GeneratorTemps {
            get { return _generatorTemps; }
        }

        internal void DeclarativeReferenceAdded() {
            if (_declarativeReferenceExists) throw new InvalidOperationException("Block cannot be declared twice");
            _declarativeReferenceExists = true;
        }

        public Variable CreateParameter(SymbolId name, Expression defaultValue) {
            Variable variable = Variable.Parameter(this, name, typeof(object), defaultValue);
            _parameters.Add(variable);
            return variable;
        }

        public Variable CreateParameter(SymbolId name, Expression defaultValue, bool inParameterArray) {
            Variable variable = Variable.Parameter(this, name, typeof(object), defaultValue, inParameterArray);
            _parameters.Add(variable);
            return variable;
        }

        public Variable CreateVariable(SymbolId name, Variable.VariableKind kind, Type type) {
            return CreateVariable(name, kind, type, null);
        }

        public Variable CreateVariable(SymbolId name, Variable.VariableKind kind, Type type, Expression defaultValue) {
            if (kind == Variable.VariableKind.Parameter) {
                throw new ArgumentException("kind");
            }
            Variable variable = Variable.Create(name, kind, this, type, defaultValue);
            _variables.Add(variable);
            return variable;
        }

        public Variable CreateLocalVariable(SymbolId name, Type type) {
            Variable variable = Variable.Local(name, this, type);
            _variables.Add(variable);
            return variable;
        }

        public Variable CreateTemporaryVariable(SymbolId name, Type type) {
            Variable variable = Variable.Temporary(name, this, type);
            _variables.Add(variable);
            return variable;
        }

        public Variable CreateGeneratorTempVariable(SymbolId name, Type type) {
            Variable variable = Variable.GeneratorTemp(name, this, type);
            _variables.Add(variable);
            return variable;
        }

        private void EmitEnvironmentIDs(CodeGen cg) {
            int size = 0;
            foreach (Variable prm in _parameters) {
                if (prm.Lift) size++;
            }
            foreach (Variable var in _variables) {
                if (var.Lift) size++;
            }

            if (!cg.IsDynamicMethod) {
                Debug.Assert(cg.TypeGen != null);

                CodeGen cctor = cg.TypeGen.TypeInitializer;
                EmitEnvironmentIdArray(cctor, size);
                Slot fields = cg.TypeGen.AddStaticField(typeof(SymbolId[]), "__symbolIds$" + _name + "$" + Interlocked.Increment(ref _Counter));
                fields.EmitSet(cctor);
                fields.EmitGet(cg);
            } else {
                EmitEnvironmentIdArray(cg, size);
            }
        }

        private void EmitEnvironmentIdArray(CodeGen cg, int size) {
            // Create the array for the names
            cg.EmitInt(size);
            cg.Emit(OpCodes.Newarr, typeof(SymbolId));

            int index = 0;
            cg.EmitDebugMarker("--- Environment IDs ---");

            foreach (Variable prm in _parameters) {
                if (prm.Lift) {
                    EmitSetVariableName(cg, index++, prm.Name);
                }
            }

            foreach (Variable var in _variables) {
                if (var.Lift) {
                    EmitSetVariableName(cg, index++, var.Name);
                }
            }
            cg.EmitDebugMarker("--- End Environment IDs ---");
        }

        private static void EmitSetVariableName(CodeGen cg, int index, SymbolId name) {
            cg.Emit(OpCodes.Dup);
            cg.EmitInt(index);
            cg.Emit(OpCodes.Ldelema, typeof(SymbolId));
            cg.EmitSymbolId(name);
            cg.Emit(OpCodes.Call, typeof(SymbolId).GetConstructor(new Type[] { typeof(SymbolId) }));
        }

        internal void CreateEnvironmentFactory(bool generator) {
            if (HasEnvironment) {
                // Get the environment size
                int size = 0;

                if (generator) {
                    size += _generatorTemps;

                    foreach (Variable var in _variables) {
                        if (var.Kind == Variable.VariableKind.GeneratorTemporary) {
                            size++;
                        }
                    }
                }

                foreach (Variable parm in _parameters) {
                    if (parm.Lift) size++;
                }
                foreach (Variable var in _variables) {
                    if (var.Lift) size++;
                }
                // Find the right environment factory for the size of elements to store
                _environmentFactory = CreateEnvironmentFactory(size);
            }
        }

        internal EnvironmentSlot EmitEnvironmentAllocation(CodeGen cg) {
            Debug.Assert(_environmentFactory != null);

            cg.EmitDebugMarker("-- ENV ALLOC START --");

            _environmentFactory.EmitStorage(cg);
            cg.Emit(OpCodes.Dup);
            // Store the environment reference in the local
            EnvironmentSlot environmentSlot = _environmentFactory.CreateEnvironmentSlot(cg);
            environmentSlot.EmitSet(cg);

            // Emit the names array for the environment constructor
            EmitEnvironmentIDs(cg);
            // Emit code to generate the new instance of the environment

            _environmentFactory.EmitNewEnvironment(cg);

            cg.EmitDebugMarker("-- ENV ALLOC END --");

            return environmentSlot;
        }

        /// <summary>
        /// Creates a slot for context of type CodeContext from an environment slot.
        /// </summary>
        internal Slot CreateEnvironmentContext(CodeGen cg) {
            // update CodeContext so it contains the nested scope for the locals
            //  ctxSlot = new CodeContext(currentCodeContext, locals)
            Slot ctxSlot = cg.GetNamedLocal(typeof(CodeContext), "$frame");
            cg.EmitCodeContext();
            cg.EnvironmentSlot.EmitGetDictionary(cg);
            cg.EmitInt(_visibleScope ? 1 : 0);
            cg.EmitCall(typeof(RuntimeHelpers), "CreateNestedCodeContext");
            ctxSlot.EmitSet(cg);
            return ctxSlot;
        }

        public void CreateSlots(CodeGen cg) {
            if (cg == null) throw new ArgumentNullException("cg");

            if (HasEnvironment) {
                // we're an environment slot, we need our own environment slot, and we're
                // going to update our Context slot to point to a CodeContext which has
                // its Locals pointing at our Environment.
                cg.EnvironmentSlot = EmitEnvironmentAllocation(cg);
                cg.ContextSlot = CreateEnvironmentContext(cg);
            }

            CreateOuterScopeAccessSlots(cg);

            foreach (Variable prm in _parameters) {
                prm.Allocate(cg);
            }
            foreach (Variable var in _variables) {
                var.Allocate(cg);
            }
            foreach (VariableReference r in _references) {
                r.CreateSlot(cg);
                Debug.Assert(r.Slot != null);
            }

            cg.Allocator.LocalAllocator.PrepareForEmit(cg);
            cg.Allocator.GlobalAllocator.PrepareForEmit(cg);
        }

        public void CreateOuterScopeAccessSlots(CodeGen cg) {
            ScopeAllocator allocator = cg.Allocator;

            // Current context is accessed via environment slot, if any
            if (HasEnvironment) {
                allocator.AddScopeAccessSlot(this, cg.EnvironmentSlot);
            }

            if (IsClosure) {
                Slot scope = cg.GetLocalTmp(typeof(Scope));
                cg.EmitCodeContext();
                cg.EmitPropertyGet(typeof(CodeContext), "Scope");
                if (HasEnvironment) {
                    cg.EmitPropertyGet(typeof(Scope), "Parent");
                }
                scope.EmitSet(cg);

                CodeBlock current = this;
                do {
                    CodeBlock parent = current._parent;
                    if (parent._environmentFactory != null) {
                        scope.EmitGet(cg);

                        cg.EmitCall(typeof(RuntimeHelpers).GetMethod("GetTupleDictionaryData").MakeGenericMethod(parent._environmentFactory.StorageType));

                        Slot storage = new LocalSlot(cg.DeclareLocal(parent._environmentFactory.StorageType), cg);
                        storage.EmitSet(cg);
                        allocator.AddScopeAccessSlot(parent, storage);
                    }

                    scope.EmitGet(cg);
                    cg.EmitPropertyGet(typeof(Scope), "Parent");
                    scope.EmitSet(cg);

                    current = parent;
                } while(current != null && current.IsClosure);

                cg.FreeLocalTmp(scope);
            }

            // TODO: Create access slot for globals
        }

        public void AddGeneratorTemps(int count) {
            _generatorTemps += count;
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                Body.Walk(walker);
            }
            walker.PostWalk(this);
        }


        public object Execute(CodeContext context) {
            object ret;
            try {
                ret = Body.Execute(context);
            } catch {
                // TODO: MethodBase.GetCurrentMethod() instead of null?
                RuntimeHelpers.UpdateStackTrace(context, null, _name, context.ModuleContext.Module.FileName, 0);
                throw;
            }
            if (ret == Statement.NextStatement) {
                return null;
            } else {
                Debug.Assert(!(ret is ControlFlow));
                return ret;
            }
        }

        protected bool NeedsWrapperMethod(bool stronglyTyped) {
            return _parameters.Count > (stronglyTyped ? Utils.Reflection.MaxSignatureSize - 1 : CallTargets.MaximumCallArgs);
        }

        protected ConstantPool GetStaticDataForBody(CodeGen cg) {
            if (cg.DynamicMethod) return new ConstantPool();
            else return null;
        }

        public void BindClosures() {
            ClosureBinder.Bind(this);
            FlowChecker.Check(this);
        }

        public object ExecuteWithChildContext(CodeContext parent, params object[] args) {
            CodeContext child = RuntimeHelpers.CreateNestedCodeContext(parent, new SymbolDictionary(), IsVisible);
            for (int i = 0; i < _parameters.Count; i++) {
                RuntimeHelpers.SetName(child, _parameters[i].Name, args[i]);
            }
            return Execute(child);
        }

        // Return a delegate to execute this block in interpreted mode.
        public virtual Delegate GetDelegateForInterpreter(CodeContext context, bool forceWrapperMethod) {
            FlowChecker.Check(this);

            //TODO: return a better match than CallTargetWithContextN.
            // This will fix the incorrect func_code.co_flags (which is computed based on the type of delegate)
            return new CallTargetWithContextN(ExecuteWithChildContext);
        }

        
        public void EmitDelegateConstruction(CodeGen cg, bool forceWrapperMethod, bool stronglyTyped, Type delegateType) {
            FlowChecker.Check(this);

            // TODO: explicit delegate type may be wrapped...
            bool createWrapperMethod = _parameterArray ? false : forceWrapperMethod || NeedsWrapperMethod(stronglyTyped);

            bool hasContextParameter = _explicitCodeContextExpression == null && 
                (createWrapperMethod ||
                IsClosure ||
                !(cg.ContextSlot is StaticFieldSlot) ||
                _parameterArray);

            bool hasThis = false;

            for (int index = 0; index < _parameters.Count; index++) {
                if (!_parameters[index].InParameterArray) {
                    // Currently only one parameter can be out of parameter array
                    // TODO: Any number of parameters to be taken out of parameter array
                    Debug.Assert(hasThis == false);
                    Debug.Assert(index == 0);
                    hasThis = true;
                }
            }

            cg.EmitSequencePointNone();

            // TODO: storing implementations on code gen doesn't allow blocks being referenced from different methods
            // the implementations should be stored on some kind of Module when available
            CodeGen impl = cg.ProvideCodeBlockImplementation(this, hasContextParameter, hasThis);
            
            // if the method has more than our maximum # of args wrap
            // it in a method that takes an object[] instead.
            if (createWrapperMethod) {
                CodeGen wrapper = MakeWrapperMethodN(cg, impl, hasThis);
                wrapper.Finish();
                
                if (delegateType == null) {
                    delegateType = hasThis ? typeof(CallTargetWithContextAndThisN) : typeof(CallTargetWithContextN);
                }

                cg.EmitDelegateConstruction(wrapper, delegateType);
            } else if (_parameterArray) {
                if (delegateType == null) {
                    delegateType = hasThis ? typeof(CallTargetWithContextAndThisN) : typeof(CallTargetWithContextN);
                }
                cg.EmitDelegateConstruction(impl, delegateType);
            } else {
                if (delegateType == null) {
                    if (stronglyTyped) {
                        delegateType = Utils.Reflection.GetDelegateType(GetParameterTypes(hasContextParameter), _returnType);
                    } else {
                        delegateType = CallTargets.GetTargetType(hasContextParameter, _parameters.Count);
                    }
                }
                cg.EmitDelegateConstruction(impl, delegateType);
            }
        }

        private Type[] GetParameterTypes(bool hasContextParameter) {
            Type[] result = new Type[_parameters.Count + (hasContextParameter ? 1 : 0)];
            int j = 0;
            if (hasContextParameter) {
                result[j++] = typeof(CodeContext);
            }

            for (int i = 0; i < _parameters.Count; i++) {
                result[j++] = _parameters[i].Type;
            }
            return result;
        }

        /// <summary>
        /// Defines the method with the correct signature and sets up the context slot appropriately.
        /// </summary>
        /// <returns></returns>
        internal CodeGen CreateMethod(CodeGen outer, bool hasContextParameter, bool hasThis) {
            List<Type> paramTypes = new List<Type>();
            List<SymbolId> paramNames = new List<SymbolId>();
            CodeGen impl;
            int parameterIndex = 0;

            if (hasContextParameter) {
                paramTypes.Add(typeof(CodeContext));
                paramNames.Add(SymbolTable.StringToId("$context"));
                parameterIndex = 1;
            }

            // Parameters
            if (_parameterArray) {
                int startIndex = 0;
                if (hasThis) {
                    paramTypes.Add(typeof(Object));
                    paramNames.Add(SymbolTable.StringToId("$this"));
                    _parameters[0].ParameterIndex = parameterIndex++;
                    startIndex = 1;
                }

                paramTypes.Add(typeof(object[]));
                paramNames.Add(SymbolTable.StringToId("$params"));

                for (int index = startIndex; index < _parameters.Count; index++) {
                    _parameters[index].ParameterIndex = index - startIndex;
                }
            } else {
                foreach (Variable p in _parameters) {
                    paramTypes.Add(p.Type);
                    paramNames.Add(p.Name);
                    p.ParameterIndex = parameterIndex++;
                }
            }

            string implName = _name + "$" + Interlocked.Increment(ref _Counter);

            // create the new method & setup its locals
            impl = outer.DefineMethod(implName, _returnType,
                paramTypes, SymbolTable.IdsToStrings(paramNames), GetStaticDataForBody(outer));

            if (_explicitCodeContextExpression != null) {
                Slot localContextSlot = impl.GetLocalTmp(typeof(CodeContext));
                
                // cannot access code context slot during emit:
                _explicitCodeContextExpression.Emit(impl);

                localContextSlot.EmitSet(impl);
                impl.ContextSlot = localContextSlot;

            } else {
                impl.ContextSlot = hasContextParameter ? impl.GetArgumentSlot(0) : outer.ContextSlot;
            }
            
            if (_parameterArray) {
                impl.ParamsSlot = impl.GetArgumentSlot(parameterIndex);
            }

            // create the new method & setup its locals
            impl.Allocator = CompilerHelpers.CreateLocalStorageAllocator(outer, impl);

            if (outer.FastEval) {
                impl.Binder = outer.Binder;
                impl.FastEval = true;
            }

            return impl;
        }

        /// <summary>
        /// Creates a wrapper method for the user-defined function.  This allows us to use the CallTargetN
        /// delegate against the function when we don't have a CallTarget# which is large enough.
        /// </summary>
        private CodeGen MakeWrapperMethodN(CodeGen outer, CodeGen impl, bool hasThis) {
            CodeGen wrapper;
            Slot contextSlot = null;
            Slot argSlot;
            Slot thisSlot = null;
            ConstantPool staticData = null;

            bool hasContextParameter = impl.ArgumentSlots.Count > 0
                && impl.ArgumentSlots[0].Type == typeof(CodeContext);

            if (impl.ConstantPool.IsBound) {
                staticData = impl.ConstantPool.CopyData();
            }

            string implName = impl.MethodBase.Name;

            if (hasContextParameter) {
                if (hasThis) {
                    wrapper = outer.DefineMethod(implName,
                        _returnType,
                        new Type[] { typeof(CodeContext), typeof(object), typeof(object[]) },
                        null, staticData);
                    contextSlot = wrapper.GetArgumentSlot(0);
                    thisSlot = wrapper.GetArgumentSlot(1);
                    argSlot = wrapper.GetArgumentSlot(2);
                } else {
                    wrapper = outer.DefineMethod(implName,
                        _returnType,
                        new Type[] { typeof(CodeContext), typeof(object[]) },
                        null, staticData);
                    contextSlot = wrapper.GetArgumentSlot(0);
                    argSlot = wrapper.GetArgumentSlot(1);
                }
            } else {
                // Context weirdness: DynamicMethods need to flow their context, and if we don't
                // have a TypeGen we'll create a DynamicMethod but we won't flow context w/ it.
                Debug.Assert(outer.TypeGen != null);
                if (hasThis) {
                    wrapper = outer.DefineMethod(implName, _returnType, new Type[] { typeof(object), typeof(object[]) },
                        null, staticData);
                    thisSlot = wrapper.GetArgumentSlot(0);
                    argSlot = wrapper.GetArgumentSlot(1);
                } else {
                    wrapper = outer.DefineMethod(implName, _returnType, new Type[] { typeof(object[]) },
                        null, staticData);
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

            for (int pi = startIndex; pi < _parameters.Count; pi++) {
                argSlot.EmitGet(wrapper);
                wrapper.EmitInt(pi - startIndex);
                wrapper.Emit(OpCodes.Ldelem_Ref);
                wrapper.EmitCast(typeof(object), _parameters[pi].Type);
            }
            wrapper.EmitCall(impl.MethodInfo);
            wrapper.Emit(OpCodes.Ret);
            return wrapper;
        }

        public void EmitFunctionImplementation(CodeGen impl) {
            CompilerHelpers.EmitStackTraceTryBlockStart(impl);

            // emit the actual body
            EmitBody(impl);

            CompilerHelpers.EmitStackTraceFaultBlock(impl, _name, impl.HasContext ? impl.Context.SourceUnit.DisplayName : _name);
        }

        public virtual void EmitBody(CodeGen cg) {
            cg.Allocator.ActiveScope = this;
            CreateEnvironmentFactory(false);
            CreateSlots(cg);

            EmitStartPosition(cg);

            Body.Emit(cg);

            EmitEndPosition(cg);

            cg.EmitReturn(null); //TODO skip if Body is guaranteed to return
        }

        private void EmitStartPosition(CodeGen cg) {
            // ensure a break point exists at the top
            // of the file if there isn't a statement
            // flush with the start of the file.
            if (!Start.IsValid) return;

            if (Body.Start.IsValid) {
                if (Body.Start != Start) {
                    cg.EmitPosition(Start, Start);
                }
            } else {
                BlockStatement block = Body as BlockStatement;
                if (block != null) {
                    for (int i = 0; i < block.Statements.Count; i++) {
                        if (block.Statements[i].Start.IsValid) {
                            if (block.Statements[i].Start != Start) {
                                cg.EmitPosition(Start, Start);
                            }
                            break;
                        }
                    }
                }
            }
        }

        private void EmitEndPosition(CodeGen cg) {
            // ensure we emit a sequence point at the end
            // so the user can inspect any info before exiting
            // the function.  Also make sure additional code
            // isn't associated with this function.
            cg.EmitPosition(End, End);
            cg.EmitSequencePointNone();
        }

        // This is used for compiling the toplevel CodeBlock object.
        public T CreateDelegate<T>(CompilerContext context) 
            where T : class {
            CodeGen cg = CompilerHelpers.CreateDynamicCodeGenerator(context);
            cg.Allocator = CompilerHelpers.CreateFrameAllocator(cg.ContextSlot);
            
            cg.EnvironmentSlot = new EnvironmentSlot(                
                new PropertySlot(
                    new PropertySlot(cg.ContextSlot, 
                        typeof(CodeContext).GetProperty("Scope")),
                    typeof(Scope).GetProperty("Dict"))
                );

            EmitFunctionImplementation(cg);
            cg.Finish();

            return (T)(object)cg.CreateDelegate(typeof(T));
        }
    }

    public static partial class Ast {
        public static CodeBlock CodeBlock(string name) {
            return CodeBlock(SourceSpan.None, name);
        }

        public static CodeBlock CodeBlock(SourceSpan span, string name) {
            return new CodeBlock(span, name, typeof(object));
        }

        public static CodeBlock CodeBlock(string name, Type returnType) {
            return new CodeBlock(SourceSpan.None, name, returnType);
        }

        public static CodeBlock CodeBlock(SymbolId name) {
            return CodeBlock(SourceSpan.None, SymbolTable.IdToString(name));
        }

        public static CodeBlock CodeBlock(SourceSpan span, SymbolId name) {
            return CodeBlock(span, SymbolTable.IdToString(name));
        }

        public static CodeBlock EventHandlerBlock(string name, EventInfo eventInfo) {
            if (name == null) throw new ArgumentNullException("name");
            if (eventInfo == null) throw new ArgumentNullException("eventInfo");

            ParameterInfo returnInfo;
            ParameterInfo[] parameterInfos;

            Utils.Reflection.GetDelegateSignature(eventInfo.EventHandlerType, out parameterInfos, out returnInfo);

            CodeBlock result = Ast.CodeBlock(name, returnInfo.ParameterType);
            for (int i = 0; i < parameterInfos.Length; i++) {
                result.Parameters.Add(Variable.Parameter(result, SymbolTable.StringToId("$" + i), parameterInfos[i].ParameterType));
            }

            return result;
        }
    }
}
