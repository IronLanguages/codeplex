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
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Ast {

    // TODO: Should move to Generation?

    /// <summary>
    /// LambdaCompiler is responsible for compiling individual lambda (LambdaExpression). The complete tree may
    /// contain multiple lambdas, the Compiler class is reponsible for compiling the whole tree, individual
    /// lambdas are then compiled by the LambdaCompiler.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")] // TODO: fix
    internal partial class LambdaCompiler : IDisposable {

        struct ReturnBlock {
            internal Slot returnValue;
            internal Label returnStart;
        }

        /// <summary>
        /// The compiler which contains the compilation-wide information
        /// such as other lambdas and their Compilers.
        /// 
        /// TODO: readonly once rules don't depend on mutating this
        /// </summary>
        private Compiler _compiler;

        /// <summary>
        /// The lambda information for the lambda being compiled.
        /// This may be null in the cases where Compiler is still being
        /// used outside of the purpose of compiling the ASTs (sometimes
        /// it is used to generate odd dynamic method here and there,
        /// those will go away eventually and this field will be required)
        /// 
        /// TODO: readonly once rules don't depend on mutating this
        /// </summary>
        private LambdaInfo _info;

        /// <summary>
        /// The generator information for the generator being compiled.
        /// This is null if the current lambda is not a generator
        /// </summary>
        private readonly GeneratorInfo _generatorInfo;

        private readonly ILGen _ilg;
        private CodeGenOptions _options;
        private readonly TypeGen _typeGen;

        private readonly MethodBase _method;

        private readonly ListStack<Targets> _targets = new ListStack<Targets>();

        private Nullable<ReturnBlock> _returnBlock;

        // Key slots
        private EnvironmentSlot _environmentSlot;   // reference to function's own environment
        private Slot _contextSlot;                  // code context

        /// <summary>
        /// Argument slots
        /// 
        /// This list contains _all_ arguments on the underlying method builder (except for the
        /// "this"). There are two views on the list. First provides the raw view (shows all
        /// arguments), the second view provides view of the arguments which are in the original
        /// lambda (so first argument, which may be closure argument, is skipped in that case)
        /// 
        /// The two views are provided by code:GetArgumentSlot (raw) and
        /// code:GetLambdaArgumentSlot (lambda arguments only)
        /// </summary>
        private readonly Slot[] _argumentSlots;
        private readonly int _firstLambdaArgument;

        /// <summary>
        /// Source unit the lambda is related to (for emitting debug info)
        /// </summary>
        private SourceUnit _source;
        private bool _emitDebugSymbols;

        private readonly ConstantPool _constantPool;

        // true if emitting generator body, false otherwise
        private bool _generatorBody;

        // Slot that stores the number of the label to go to.
        private Slot _gotoRouter;

        private const int FinallyExitsNormally = 0;
        private const int BranchForReturn = 1;
        private const int BranchForBreak = 2;
        private const int BranchForContinue = 3;

        private LambdaCompiler(Compiler compiler, LambdaExpression lambda,
            TypeGen typeGen, MethodBase/*!*/ mi, ILGenerator/*!*/ ilg, IList<Type>/*!*/ paramTypes,
            ConstantPool constantPool, bool closure) {

            ContractUtils.Requires(constantPool == null || mi.IsStatic, "constantPool");

            _typeGen = typeGen;
            _method = mi;

            // Create the ILGen instance, debug or not
#if !SILVERLIGHT
            if (Snippets.Shared.ILDebug) {
                _ilg = CreateDebugILGen(ilg, typeGen, mi, paramTypes);
            } else {
                _ilg = new ILGen(ilg, typeGen);
            }
#else
            _ilg = new ILGen(ilg);
#endif

            // Create the argument array
            int thisOffset = mi.IsStatic ? 0 : 1;
            _argumentSlots = new Slot[paramTypes.Count];
            for (int i = 0; i < _argumentSlots.Length; i++) {
                _argumentSlots[i] = new ArgSlot(i + thisOffset, paramTypes[i], _ilg);
            }

            // Create/initialize constant pool
            if ((_constantPool = constantPool) != null) {
                Debug.Assert(paramTypes.Count > 0);
                _constantPool.SetCodeGen(this, new FieldSlot(_argumentSlots[0], typeof(Closure).GetField("Constants")));
            } else {
                _constantPool = new ConstantPool();
                _constantPool.SetCodeGen(this, null);
            }

            // Adjust the lambda vs. raw view of the arguments
            _firstLambdaArgument = (constantPool != null || closure) ? 1 : 0;

            // Initialize _info and _compiler
            if (lambda != null) {
                if (compiler == null) {
                    // No compiler, analyze the tree to create one (possibly rewriting lambda)
                    compiler = new Compiler(AnalyzeLambda(ref lambda));
                }
                _info = compiler.GetLambdaInfo(lambda);

                GeneratorLambdaExpression gle = lambda as GeneratorLambdaExpression;
                if (gle != null) {
                    _generatorInfo = compiler.GetGeneratorInfo(gle);
                }
            }
            _compiler = compiler;

            NoteCompilerCreation(mi);
        }

        public override string ToString() {
            return _method.ToString();
        }

        #region Properties

        private Compiler Compiler {
            get { return _compiler; }
        }

        private LambdaInfo LambdaInfo {
            get { return _info; }
        }

        internal ILGen IL {
            get { return _ilg; }
        }

        private bool DynamicMethod {
            get {
                return (_options & CodeGenOptions.DynamicMethod) != 0;
            }
            set {
                if (value) {
                    _options |= CodeGenOptions.DynamicMethod;
                } else {
                    _options &= ~CodeGenOptions.DynamicMethod;
                }
            }
        }

        /// <summary>
        /// True if Compiler should store all constants in static fields and emit loads of those fields,
        /// false if constants should be emitted and boxed at runtime.
        /// </summary>
        private bool CacheConstants {
            get {
                return (_options & CodeGenOptions.CacheConstants) != 0;
            }
            set {
                if (value) {
                    _options |= CodeGenOptions.CacheConstants;
                } else {
                    _options &= ~CodeGenOptions.CacheConstants;
                }
            }
        }
        
        /// <summary>
        /// Gets the TypeGen object which this Compiler is emitting into.  TypeGen can be
        /// null if the method is a dynamic method.
        /// </summary>
        internal TypeGen TypeGen {
            get {
                return _typeGen;
            }
        }

        internal bool EmitDebugSymbols {
            get { return _emitDebugSymbols; }
        }

        internal MethodBase Method {
            get {
                return _method;
            }
        }

        internal bool IsDynamicMethod {
            get {
                return _method is DynamicMethod;
            }
        }

        /// <summary>
        /// Gets a list which can be used to inject references to objects from IL.  
        /// </summary>
        internal ConstantPool ConstantPool {
            get {
                return _constantPool;
            }
        }

        /// <summary>
        /// The slot used for the current frames environment.  If this method defines has one or more closure functions
        /// defined within it then the environment will contain all of the variables that the closure variables lifted
        /// into the environment.
        /// </summary>
        internal EnvironmentSlot EnvironmentSlot {
            get {
                Debug.Assert(_environmentSlot != null);
                return _environmentSlot;
            }
            set {
                _environmentSlot = value;
            }
        }

        internal Slot ContextSlot {
            get {
                return _contextSlot;
            }
            set {
                ContractUtils.RequiresNotNull(value, "value");
                if (!typeof(CodeContext).IsAssignableFrom(value.Type)) {
                    throw new ArgumentException("ContextSlot must be assignable from CodeContext", "value");
                }

                _contextSlot = value;
            }
        }

        internal void SetDebugSymbols(SourceUnit source) {
            SetDebugSymbols(source, _typeGen != null && _typeGen.AssemblyGen.IsDebuggable && source != null &&
                source.LanguageContext.DomainManager.GlobalOptions.DebugMode);
        }

        private void SetDebugSymbols(SourceUnit source, bool emitSymbols) {
            Debug.Assert(!emitSymbols || source != null,
                "Cannot emit symbols w/o source unit.");
            Debug.Assert(!emitSymbols || _typeGen != null && _typeGen.AssemblyGen.IsDebuggable,
                "Cannot emit symbols to a dynamic method or a non-debuggable module");

            _source = source;
            _emitDebugSymbols = emitSymbols;
        }

        private TargetBlockType BlockType {
            get {
                if (_targets.Count == 0) return TargetBlockType.Normal;
                Targets t = _targets.Peek();
                return t.BlockType;
            }
        }

        private Label? BlockContinueLabel {
            get {
                if (_targets.Count == 0) return Targets.NoLabel;
                Targets t = _targets.Peek();
                return t.ContinueLabel;
            }
        }

        internal bool IsGeneratorBody {
            get {
                return _generatorBody;
            }
            private set {
                _generatorBody = value;
            }
        }

        private const int GotoRouterNone = -1;
        private const int GotoRouterYielding = -2;

        private Slot GotoRouter {
            get {
                return _gotoRouter;
            }
            set {
                _gotoRouter = value;
            }
        }

        #endregion

        #region Compiler entry point

        /// <summary>
        /// Compiler entry point, used by ScriptCode.
        /// This is used for compiling the toplevel LambdaExpression object.
        /// </summary>
        internal static DlrMainCallTarget CompileTopLevelLambda(SourceUnit source, LambdaExpression lambda) {
            // 1. Create the lambda compiler
            LambdaCompiler cg = CreateDynamicLambdaCompiler(lambda, "Initialize", typeof(object), new Type[] { typeof(CodeContext) }, source);

            cg.ContextSlot = cg.GetLambdaArgumentSlot(0);
            cg.SetAllocators(new GlobalNamedAllocator(), new FrameStorageAllocator());

            // 2. Generate code
            cg.EmitBody();
            cg.Finish();

            return (DlrMainCallTarget)(object)cg.CreateDelegate(typeof(DlrMainCallTarget));
        }

        /// <summary>
        /// Compiler entry point, used by TreeCompiler
        /// </summary>
        /// <typeparam name="T">Type of the delegate to create</typeparam>
        /// <param name="lambda">LambdaExpression to compile.</param>
        /// <returns>The compiled delegate.</returns>
        internal static T CompileLambda<T>(LambdaExpression lambda) {
            // 1. Create signature
            List<Type> types;
            List<string> names;
            string name;
            ComputeSignature(lambda, out types, out names, out name);

            // 2. Create lambda compiler
            LambdaCompiler c = CreateDynamicLambdaCompiler(lambda, name, lambda.ReturnType, types, null);
            c.SetDefaultAllocators(null);

            // 3. Emit
            c.EmitBody();

            c.Finish();

            // 4. Return the delegate.
            return (T)(object)c.CreateDelegate(typeof(T));
        }

        #endregion

        private static AnalyzedTree AnalyzeLambda(ref LambdaExpression lambda) {
            DumpLambda(lambda);

            lambda = StackSpiller.AnalyzeLambda(lambda);
            AnalyzedTree at = LambdaBinder.Bind(lambda);

            DumpLambda(lambda);

            return at;
        }

        [Conditional("DEBUG")]
        private static void DumpLambda(LambdaExpression lambda) {
#if DEBUG
            AstWriter.Dump(lambda, lambda.Name);
#endif
        }

        private void PushExceptionBlock(TargetBlockType type, Slot returnFlag) {
            if (_targets.Count == 0) {
                _targets.Push(new Targets(Targets.NoLabel, Targets.NoLabel, type, returnFlag, null));
            } else {
                Targets t = _targets.Peek();
                _targets.Push(new Targets(t.BreakLabel, t.ContinueLabel, type, returnFlag ?? t.FinallyReturns, null));
            }
        }

        private void PushTargets(Label? breakTarget, Label? continueTarget, LabelTarget label) {
            if (_targets.Count == 0) {
                _targets.Push(new Targets(breakTarget, continueTarget, BlockType, null, label));
            } else {
                Targets t = _targets.Peek();
                TargetBlockType bt = t.BlockType;
                if (bt == TargetBlockType.Finally && label != null) {
                    bt = TargetBlockType.LoopInFinally;
                }
                _targets.Push(new Targets(breakTarget, continueTarget, bt, t.FinallyReturns, label));
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "type")]
        private void PopTargets(TargetBlockType type) {
            Targets t = _targets.Pop();
            Debug.Assert(t.BlockType == type);
        }

        private void PopTargets() {
            _targets.Pop();
        }

        // TODO: Cleanup, hacky!!!
        private void CheckAndPushTargets(LabelTarget label) {
            for (int i = _targets.Count - 1; i >= 0; i--) {
                if (_targets[i].Label == label) {
                    PushTargets(_targets[i].BreakLabel, _targets[i].ContinueLabel, null);
                    return;
                }
            }

            throw new InvalidOperationException("Statement not on the stack");
        }

        private void EmitBreak() {
            Targets t = _targets.Peek();
            int finallyIndex = -1;
            switch (t.BlockType) {
                default:
                case TargetBlockType.Normal:
                case TargetBlockType.LoopInFinally:
                    if (t.BreakLabel.HasValue)
                        _ilg.Emit(OpCodes.Br, t.BreakLabel.Value);
                    else
                        throw new InvalidOperationException();
                    break;
                case TargetBlockType.Try:
                case TargetBlockType.Else:
                case TargetBlockType.Catch:
                    for (int i = _targets.Count - 1; i >= 0; i--) {
                        if (_targets[i].BlockType == TargetBlockType.Finally) {
                            finallyIndex = i;
                            break;
                        }

                        if (_targets[i].BlockType == TargetBlockType.LoopInFinally ||
                            !_targets[i].BreakLabel.HasValue)
                            break;
                    }

                    if (finallyIndex == -1) {
                        if (t.BreakLabel.HasValue)
                            _ilg.Emit(OpCodes.Leave, t.BreakLabel.Value);
                        else
                            throw new InvalidOperationException();
                    } else {
                        if (!_targets[finallyIndex].LeaveLabel.HasValue)
                            _targets[finallyIndex].LeaveLabel = _ilg.DefineLabel();

                        _ilg.EmitInt(LambdaCompiler.BranchForBreak);
                        _targets[finallyIndex].FinallyReturns.EmitSet(_ilg);

                        _ilg.Emit(OpCodes.Leave, _targets[finallyIndex].LeaveLabel.Value);
                    }
                    break;
                case TargetBlockType.Finally:
                    _ilg.EmitInt(LambdaCompiler.BranchForBreak);
                    t.FinallyReturns.EmitSet(_ilg);
                    _ilg.Emit(OpCodes.Endfinally);
                    break;
            }
        }

        private void EmitContinue() {
            Targets t = _targets.Peek();
            switch (t.BlockType) {
                default:
                case TargetBlockType.Normal:
                case TargetBlockType.LoopInFinally:
                    if (t.ContinueLabel.HasValue)
                        _ilg.Emit(OpCodes.Br, t.ContinueLabel.Value);
                    else
                        throw new InvalidOperationException();
                    break;
                case TargetBlockType.Try:
                case TargetBlockType.Else:
                case TargetBlockType.Catch:
                    if (t.ContinueLabel.HasValue)
                        _ilg.Emit(OpCodes.Leave, t.ContinueLabel.Value);
                    else
                        throw new InvalidOperationException();
                    break;
                case TargetBlockType.Finally:
                    _ilg.EmitInt(LambdaCompiler.BranchForContinue);
                    t.FinallyReturns.EmitSet(_ilg);
                    _ilg.Emit(OpCodes.Endfinally);
                    break;
            }
        }

        private void EmitReturn() {
            int finallyIndex = -1;
            switch (BlockType) {
                default:
                case TargetBlockType.Normal:
                    _ilg.Emit(OpCodes.Ret);
                    break;
                case TargetBlockType.Catch:
                case TargetBlockType.Try:
                case TargetBlockType.Else:
                    // with has it's own finally block, so no need to search...
                    for (int i = _targets.Count - 1; i >= 0; i--) {
                        if (_targets[i].BlockType == TargetBlockType.Finally) {
                            finallyIndex = i;
                            break;
                        }
                    }

                    EnsureReturnBlock();
                    Debug.Assert(_returnBlock.HasValue);
                    if (CompilerHelpers.GetReturnType(_method) != typeof(void)) {
                        _returnBlock.Value.returnValue.EmitSet(_ilg);
                    }

                    if (finallyIndex == -1) {
                        // emit the real return
                        _ilg.Emit(OpCodes.Leave, _returnBlock.Value.returnStart);
                    } else {
                        // need to leave into the inner most finally block,
                        // the finally block will fall through and check
                        // the return value.
                        if (!_targets[finallyIndex].LeaveLabel.HasValue)
                            _targets[finallyIndex].LeaveLabel = _ilg.DefineLabel();

                        _ilg.EmitInt(LambdaCompiler.BranchForReturn);
                        _targets[finallyIndex].FinallyReturns.EmitSet(_ilg);

                        _ilg.Emit(OpCodes.Leave, _targets[finallyIndex].LeaveLabel.Value);
                    }
                    break;
                case TargetBlockType.LoopInFinally:
                case TargetBlockType.Finally: {
                        Targets t = _targets.Peek();
                        EnsureReturnBlock();
                        if (CompilerHelpers.GetReturnType(_method) != typeof(void)) {
                            _returnBlock.Value.returnValue.EmitSet(_ilg);
                        }
                        // Assert check ensures that those who pushed the block with finallyReturns as null 
                        // should not yield in their lambdas.
                        Debug.Assert(t.FinallyReturns != null);
                        _ilg.EmitInt(LambdaCompiler.BranchForReturn);
                        t.FinallyReturns.EmitSet(_ilg);
                        _ilg.Emit(OpCodes.Endfinally);
                        break;
                    }
            }
        }

        private void EmitImplicitCast(Type from, Type to) {
            if (!TryEmitImplicitCast(from, to)) {
                throw new ArgumentException(String.Format("Cannot cast from '{0}' to '{1}'", from, to));
            }
        }

        private void EmitCast(Type from, Type to) {
            ContractUtils.RequiresNotNull(from, "from");
            ContractUtils.RequiresNotNull(to, "to");

            if (!TryEmitExplicitCast(from, to)) {
                throw new ArgumentException(String.Format("Cannot cast from '{0}' to '{1}'", from, to));
            }
        }

        private bool TryEmitImplicitCast(Type from, Type to) {
            if (from.IsValueType && to == typeof(object)) {
                EmitBoxing(from);
                return true;
            } else {
                return _ilg.TryEmitImplicitCast(from, to);
            }
        }

        private bool TryEmitExplicitCast(Type from, Type to) {
            if (from.IsValueType && to == typeof(object)) {
                EmitBoxing(from);
                return true;
            } else {
                return _ilg.TryEmitExplicitCast(from, to);
            }
        }

        /// <summary>
        /// Boxes the value of the stack. No-op for reference types. Void is converted to a null reference. For almost all value types this method
        /// will box them in the standard way.  Int32 and Boolean are handled with optimized conversions
        /// that reuse the same object for small values.  For Int32 this is purely a performance
        /// optimization.  For Boolean this is use to ensure that True and False are always the same
        /// objects.
        /// </summary>
        /// <param name="type"></param>
        internal void EmitBoxing(Type type) {
            ContractUtils.RequiresNotNull(type, "type");
            Debug.Assert(typeof(void).IsValueType);

            if (type == typeof(int)) {
                _ilg.EmitCall(typeof(RuntimeHelpers), "Int32ToObject");
            } else if (type == typeof(bool)) {
                _ilg.EmitCall(typeof(RuntimeHelpers), "BooleanToObject");
            } else {
                _ilg.EmitBoxing(type);
            }
        }

        private void EmitReturnValue() {
            EnsureReturnBlock();
            if (CompilerHelpers.GetReturnType(_method) != typeof(void)) {
                _returnBlock.Value.returnValue.EmitGet(_ilg);
            }
        }

        private void EmitReturn(Expression expr) {
            if (IsGeneratorBody) {
                EmitReturnInGenerator(expr);
            } else {
                if (expr == null) {
                    Debug.Assert(CompilerHelpers.GetReturnType(_method) == typeof(void));
                } else {
                    Type result = CompilerHelpers.GetReturnType(_method);
                    Debug.Assert(result.IsAssignableFrom(expr.Type));
                    EmitExpression(expr);
                    if (!TypeUtils.CanAssign(result, expr.Type)) {
                        EmitImplicitCast(expr.Type, result);
                    }
                }
                EmitReturn();
            }
        }

        private void EmitReturnInGenerator(Expression expr) {
            EmitSetGeneratorReturnValue(expr);

            _ilg.EmitInt(0);
            EmitReturn();
        }

        private void EmitYield(Expression expr, YieldTarget target) {
            ContractUtils.RequiresNotNull(expr, "expr");

            EmitSetGeneratorReturnValue(expr);
            EmitUpdateGeneratorLocation(target.Index);

            // Mark that we are yielding, which will ensure we skip
            // all of the finally bodies that are on the way to exit

            _ilg.EmitInt(GotoRouterYielding);
            GotoRouter.EmitSet(_ilg);

            _ilg.EmitInt(1);
            EmitReturn();

            _ilg.MarkLabel(target.EnsureLabel(this));
            // Reached the routing destination, set router to GotoRouterNone
            _ilg.EmitInt(GotoRouterNone);
            GotoRouter.EmitSet(_ilg);
        }

        private void EmitSetGeneratorReturnValue(Expression expr) {
            GetLambdaArgumentSlot(1).EmitGet(_ilg);
            EmitExpressionAsObjectOrNull(expr);
            _ilg.Emit(OpCodes.Stind_Ref);
        }

        private void EmitUpdateGeneratorLocation(int index) {
            GetLambdaArgumentSlot(0).EmitGet(_ilg);
            _ilg.EmitInt(index);
            _ilg.EmitFieldSet(typeof(Generator).GetField("location"));
        }

        private void EmitGetGeneratorLocation() {
            GetLambdaArgumentSlot(0).EmitGet(_ilg);
            _ilg.EmitFieldGet(typeof(Generator), "location");
        }

        private void EmitPosition(SourceLocation start, SourceLocation end) {
            if (_emitDebugSymbols) {

                if (start == SourceLocation.Invalid || end == SourceLocation.Invalid) {
                    return;
                }

                if (start == SourceLocation.None || end == SourceLocation.None) {
                    EmitSequencePointNone();
                    return;
                }

                Debug.Assert(start.Line > 0 && end.Line > 0);

                MarkSequencePoint(
                    start.Line, start.Column,
                    end.Line, end.Column
                );

                _ilg.Emit(OpCodes.Nop);
            }
        }

        private void EmitSequencePointNone() {
            if (_emitDebugSymbols) {
                MarkSequencePoint(
                    SourceLocation.None.Line, SourceLocation.None.Column,
                    SourceLocation.None.Line, SourceLocation.None.Column
                );
                _ilg.Emit(OpCodes.Nop);
            }
        }

        internal Slot GetNamedLocal(Type type, string name) {
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.RequiresNotNull(name, "name");

            LocalBuilder lb = _ilg.DeclareLocal(type);
            if (_emitDebugSymbols) lb.SetLocalSymInfo(name);
            return new LocalSlot(lb, _ilg);
        }        

        private void EnsureReturnBlock() {
            if (!_returnBlock.HasValue) {
                ReturnBlock val = new ReturnBlock();

                if (CompilerHelpers.GetReturnType(_method) != typeof(void)) {
                    val.returnValue = GetNamedLocal(CompilerHelpers.GetReturnType(_method), "retval");
                }
                val.returnStart = _ilg.DefineLabel();

                _returnBlock = val;
            }
        }

        internal void Finish() {
            Debug.Assert(_targets.Count == 0);

            if (_returnBlock.HasValue) {
                _ilg.MarkLabel(_returnBlock.Value.returnStart);
                if (CompilerHelpers.GetReturnType(_method) != typeof(void)) {
                    _returnBlock.Value.returnValue.EmitGet(_ilg);
                }
                _ilg.Emit(OpCodes.Ret);
            }

            if (DynamicMethod) {
                this.CreateDelegateMethodInfo();
            }
        }

        internal void EmitTuple(Type tupleType, int count, EmitArrayHelper emit) {
            EmitTuple(tupleType, 0, count, emit);
        }

        private void EmitTuple(Type tupleType, int start, int end, EmitArrayHelper emit) {
            int size = end - start;

            if (size > Tuple.MaxSize) {
                int multiplier = 1;
                while (size > Tuple.MaxSize) {
                    size = (size + Tuple.MaxSize - 1) / Tuple.MaxSize;
                    multiplier *= Tuple.MaxSize;
                }
                for (int i = 0; i < size; i++) {
                    int newStart = start + (i * multiplier);
                    int newEnd = System.Math.Min(end, start + ((i + 1) * multiplier));

                    PropertyInfo pi = tupleType.GetProperty("Item" + String.Format("{0:D3}", i));
                    Debug.Assert(pi != null);
                    EmitTuple(pi.PropertyType, newStart, newEnd, emit);
                }
            } else {
                for (int i = start; i < end; i++) {
                    emit(i);
                }
            }

            // fill in emptys with null.
            Type[] genArgs = tupleType.GetGenericArguments();
            for (int i = size; i < genArgs.Length; i++) {
                _ilg.EmitNull();
            }

            EmitTupleNew(tupleType);
        }

        private void EmitTupleNew(Type tupleType) {
            ConstructorInfo[] cis = tupleType.GetConstructors();
            foreach (ConstructorInfo ci in cis) {
                if (ci.GetParameters().Length != 0) {
                    _ilg.EmitNew(ci);
                    break;
                }
            }
        }

        // Used only by OptimizedModuleGenerator
        internal void EmitArgGet(int index) {
            ContractUtils.Requires(index >= 0 && index < Int32.MaxValue, "index");

            if (_method == null || !_method.IsStatic) {
                // making room for this
                index++;
            }

            _ilg.EmitLoadArg(index);
        }

        private void EmitType(Type type) {
            ContractUtils.RequiresNotNull(type, "type");

            if (!(type is TypeBuilder) && !type.IsGenericParameter && !type.IsVisible) {
                // can't ldtoken on a non-visible type, refer to it via a runtime constant...
                EmitConstant(new RuntimeConstant(type));
            } else {
                _ilg.EmitType(type);
            }
        }

        /// <summary>
        /// Emits code which creates new instance of the delegateType delegate.
        /// 
        /// Since the delegate is getting closed over the "Closure" argument, this
        /// cannot be used with virtual/instance methods (delegateFunction must be static method)
        /// </summary>
        private void EmitDelegateConstruction(LambdaCompiler delegateFunction, Type delegateType, bool closure) {
            ContractUtils.RequiresNotNull(delegateFunction, "delegateFunction");
            ContractUtils.RequiresNotNull(delegateType, "delegateType");

            if (delegateFunction.Method is DynamicMethod || delegateFunction.ConstantPool.IsBound) {
                Slot method = ConstantPool.AddData(delegateFunction.CreateDelegateMethodInfo(), typeof(MethodInfo));
                Slot data = ConstantPool.AddData(delegateFunction.ConstantPool.Data);

                method.EmitGet(_ilg);                   // method
                _ilg.Emit(OpCodes.Ldtoken, delegateType);    // delegate (as RuntimeTypeHandler)
                EmitCodeContext();                      // CodeContext
                data.EmitGet(_ilg);                     // constants
                _ilg.EmitCall(typeof(RuntimeHelpers).GetMethod("CreateDynamicClosure"));
                _ilg.Emit(OpCodes.Castclass, delegateType);
            } else {
                if (closure) {
                    EmitCodeContext();      // CodeContext
                    _ilg.EmitNull();             // constant pool
                    _ilg.EmitNew(typeof(Closure).GetConstructor(new Type[] { typeof(CodeContext), typeof(object[]) }));
                } else {
                    _ilg.EmitNull();
                }
                _ilg.Emit(OpCodes.Ldftn, (MethodInfo)delegateFunction.Method);
                _ilg.Emit(OpCodes.Newobj, (ConstructorInfo)(delegateType.GetMember(".ctor")[0]));
            }
        }

        /// <summary>
        /// The main entry to the constant emitting.
        /// This will handle constant caching and compiler constants.
        /// Constants will be left on the execution stack as their direct type.
        /// </summary>
        /// <param name="value">Constant to be emitted</param>
        private void EmitConstant(object value) {
            CompilerConstant cc = value as CompilerConstant;

            if (cc != null) {
                if (CacheConstants) {
                    EmitCompilerConstantCache(cc);
                } else {
                    EmitCompilerConstantNoCache(cc);
                }
            } else {
                if (CacheConstants) {
                    EmitConstantCache(value);
                } else {
                    EmitConstantNoCache(value);
                }
            }
        }

        private void EmitConstantCache(object value) {
            Debug.Assert(!(value is CompilerConstant));

            string strVal;
            if (value == null) {
                _ilg.EmitNull();
            } else if ((strVal = value as string) != null) {
                _ilg.EmitString(strVal);
            } else {
                Slot s = _typeGen.GetOrMakeConstant(value);
                s.EmitGet(_ilg);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal void EmitConstantNoCache(object value) {
            Debug.Assert(!(value is CompilerConstant));

            Type type;
            if (value is SymbolId) {
                IL.EmitSymbolId((SymbolId)value);
            } else if ((type = value as Type) != null) {
                EmitType(type);
            } else {
                if (!IL.TryEmitConstant(value)) {
                    EmitConstant(new RuntimeConstant(value));
                }
            }
        }

        private void EmitCompilerConstantCache(CompilerConstant value) {
            Debug.Assert(value != null);
            Debug.Assert(_typeGen != null);
            _typeGen.GetOrMakeCompilerConstant(value).EmitGet(_ilg);
        }

        private void EmitCompilerConstantNoCache(CompilerConstant value) {
            Debug.Assert(value != null);
            if (ConstantPool.IsBound) {
                //TODO cache these so that we use the same slot for the same values
                _constantPool.AddData(value.Create(), value.Type).EmitGet(_ilg);
            } else {
                value.EmitCreation(IL);
            }
        }

        /// <summary>
        /// Returns index-th 'raw' argument on the lambda being compiled.
        /// This includes possible argument at index 0 which is the closure
        /// for the delegate being built.
        /// 
        /// For argument access which only takes into account the actual (lambda) arguments,
        /// use code:GetLambdaArgumentSlot
        /// </summary>
        private Slot GetArgumentSlot(int index) {
            return _argumentSlots[index];
        }

        /// <summary>
        /// Returns the index-th argument. This method provides access to the actual arguments
        /// defined on the lambda itself, and excludes the possible 0-th closure argument.
        /// </summary>
        internal Slot GetLambdaArgumentSlot(int index) {
            return GetArgumentSlot(index + _firstLambdaArgument);
        }

        internal int GetLambdaArgumentSlotCount() {
            return _argumentSlots.Length - _firstLambdaArgument;
        }

        internal MethodInfo CreateDelegateMethodInfo() {
            if (_method is DynamicMethod) {
                return (MethodInfo)_method;
            } else if (_method is MethodBuilder) {
                MethodBuilder mb = _method as MethodBuilder;
                Type methodType = _typeGen.FinishType();
                return methodType.GetMethod(mb.Name);
            } else {
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Creates instance of the delegate bound to the code context.
        /// </summary>
        private Delegate CreateDelegateWithContext(Type delegateType, CodeContext context) {
            ContractUtils.RequiresNotNull(delegateType, "delegateType");

            MethodInfo method = CreateDelegateMethodInfo();
            if (ConstantPool.IsBound) {
                return ReflectionUtils.CreateDelegate(method, delegateType, new Closure(context, _constantPool.Data));
            } else if (context != null) {
                return ReflectionUtils.CreateDelegate(method, delegateType, new Closure(context, null));
            } else {
                return ReflectionUtils.CreateDelegate(method, delegateType);
            }
        }

        internal Delegate CreateDelegate(Type delegateType) {
            return CreateDelegateWithContext(delegateType, null);
        }

        /// <summary>
        /// Creates a compiler that shares the same characteristic as "this". If compiling into
        /// DynamicMethod (both fake or real), it will create compiler backed by dynamic method
        /// (also fake or real), if compiling into a type, it will create compiler linked to
        /// a new (static) method on the same type.
        /// </summary>
        private LambdaCompiler CreateLambdaCompiler(LambdaExpression lambda, string name, Type retType, IList<Type> paramTypes, string[] paramNames,
            ConstantPool constantPool, bool closure) {
            ContractUtils.RequiresNotNullItems(paramTypes, "paramTypes");

            LambdaCompiler lc;
            if (DynamicMethod) {
                lc = CreateDynamicLambdaCompiler(_compiler, lambda, name, retType, paramTypes, paramNames, constantPool, closure, _source);
            } else {
                lc = CreateStaticLambdaCompiler(_compiler, lambda, _typeGen, name, retType, paramTypes, paramNames, constantPool, closure);
            }

            lc.SetDebugSymbols(_source, _emitDebugSymbols);

            return lc;
        }

        private void EndExceptionBlock() {
            if (_targets.Count > 0) {
                Targets t = _targets.Peek();
                Debug.Assert(t.BlockType != TargetBlockType.LoopInFinally);
                if (t.BlockType == TargetBlockType.Finally && t.LeaveLabel.HasValue) {
                    _ilg.MarkLabel(t.LeaveLabel.Value);
                }
            }

            _ilg.EndExceptionBlock();
        }

        private void MarkSequencePoint(int startLine, int startColumn, int endLine, int endColumn) {
            Debug.Assert(_source != null);

            string url = _source.GetSymbolDocument(startLine) ?? _source.Path;

            if (!String.IsNullOrEmpty(url)) {
                ISymbolDocumentWriter writer = _typeGen.AssemblyGen.GetSymbolWriter(url, _source.LanguageContext);

                startLine = _source.MapLine(startLine);
                endLine = _source.MapLine(endLine);

                _ilg.MarkSequencePoint(writer, startLine, startColumn, endLine, endColumn);
            }
        }


        #region IL Debugging Support

#if !SILVERLIGHT
        private static DebugILGen/*!*/ CreateDebugILGen(ILGenerator/*!*/ il, TypeGen tg, MethodBase/*!*/ method, IList<Type>/*!*/ paramTypes) {
            TextWriter txt = new StreamWriter(Snippets.Shared.GetMethodILDumpFile(method));
            DebugILGen dig = new DebugILGen(il, tg, txt);

            dig.WriteLine(String.Format("{0} {1} (", method.Name, method.Attributes));
            StringBuilder sb = new StringBuilder();
            foreach (Type type in paramTypes) {
                sb.Length = 0;  // Clear the builder
                sb.Append("\t");
                ReflectionUtils.FormatTypeName(sb, type);
                dig.WriteLine(sb.ToString());
            }
            dig.WriteLine(")");

            return dig;
        }
#endif

        #endregion

        internal Slot CreateDynamicSite(DynamicAction action, Type[] siteTypes) {
            Type siteType = DynamicSiteHelpers.MakeDynamicSiteType(siteTypes);
            object site = DynamicSiteHelpers.MakeSite(action, siteType);
            return ConstantPool.AddData(site);
        }

        private Slot GetTemporarySlot(Type type) {
            Slot temp;

            if (IsGeneratorBody) {
                temp = _generatorInfo.NextGeneratorTemp();
                if (type != typeof(object)) {
                    temp = new CastSlot(temp, type);
                }
            } else {
                temp = _ilg.GetLocalTmp(type);
            }
            return temp;
        }

        // TODO: remove so _compiler and _info can be readonly
        internal void InitializeRule(Compiler tc, LambdaInfo top) {
            Debug.Assert(tc != null);
            Debug.Assert(top != null);

            _compiler = tc;
            _info = top;
        }

        private LambdaInfo GetLambdaInfo(LambdaExpression lambda) {
            Debug.Assert(Compiler != null);
            return Compiler.GetLambdaInfo(lambda);
        }

        private TryStatementInfo GetTsi(TryStatement node) {
            if (_generatorInfo == null) {
                return null;
            }
            return _generatorInfo.TryGetTsi(node);
        }

        private YieldTarget GetYieldTarget(YieldStatement node) {
            if (_generatorInfo == null) {
                return null;
            }
            return _generatorInfo.TryGetYieldTarget(node);
        }

        internal void SetDefaultAllocators(LambdaCompiler outer) {
            _info.SetAllocators(
                outer != null ? outer.LambdaInfo.GlobalAllocator : null,
                new LocalStorageAllocator(new LocalSlotFactory(this))
            );
        }

        internal void SetAllocators(StorageAllocator global, StorageAllocator local) {
            Debug.Assert(_info != null);
            _info.SetAllocators(global, local);
        }

        #region IDisposable Members

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
        }

        #endregion

        #region Factory methods

        /// <summary>
        /// The internal LambdaCompiler factory to create dynamic method.
        /// 
        /// Dynamic methods created this way don't have named parameters, always get constant pool,
        /// are not closures and don't have debug info attached.
        /// </summary>
        internal static LambdaCompiler/*!*/ CreateDynamicLambdaCompiler(LambdaExpression lambda, string/*!*/ name, Type/*!*/ returnType,
            IList<Type/*!*/>/*!*/ parameterTypes, SourceUnit source) {
            return CreateDynamicLambdaCompiler(null, lambda, name, returnType, parameterTypes, null, new ConstantPool(), false, source);
        }

        /// <summary>
        /// Creates a compiler backed by dynamic method. Sometimes (when debugging is required) the dynamic
        /// method is actually a 'fake' dynamic method and is backed by static type created specifically for
        /// the one method
        /// </summary>
        private static LambdaCompiler/*!*/ CreateDynamicLambdaCompiler(Compiler compiler, LambdaExpression lambda, string/*!*/ methodName, Type/*!*/ returnType,
            IList<Type/*!*/>/*!*/ paramTypes, IList<string> paramNames, ConstantPool constantPool, bool closure, SourceUnit source) {

            Assert.NotEmpty(methodName);
            Assert.NotNull(returnType);
            Assert.NotNullItems(paramTypes);

            LambdaCompiler lc;
            bool debugMode = source != null && source.LanguageContext.DomainManager.GlobalOptions.DebugMode;
            bool emitSymbols = debugMode && source.HasPath;

            //
            // Generate a static method if either
            // 1) we want to dump all geneated IL to an assembly on disk (SaveSnippets on)
            // 2) the method is debuggable, i.e. DebugMode is on and a source unit is associated with the method
            //
            if (Snippets.Shared.SaveSnippets || emitSymbols) {
                TypeGen typeGen = Snippets.Shared.DefineType(methodName, typeof(object), false, source, false);
                lc = CreateStaticLambdaCompiler(compiler, lambda, typeGen, methodName, returnType, paramTypes, paramNames, constantPool, closure);

                // emit symbols iff we have a source unit (and are in debug mode, see GenerateStaticMethod):
                lc.SetDebugSymbols(source, emitSymbols);
            } else {
                Type[] parameterTypes = MakeParameterTypeArray(paramTypes, constantPool, closure);
                DynamicMethod target = Snippets.Shared.CreateDynamicMethod(methodName, returnType, parameterTypes);
                lc = new LambdaCompiler(compiler, lambda, null, target, target.GetILGenerator(), parameterTypes, constantPool, closure);

                // emits line number setting instructions if source unit available:
                if (debugMode) {
                    lc.SetDebugSymbols(source, false);
                }
            }

            // do not allocate constants to static fields:
            lc.CacheConstants = false;

            // this is a dynamic method:
            lc.DynamicMethod = true;

            return lc;
        }

        /// <summary>
        /// Creates a LambdaCompiler backed by a method on a static type (represented by tg).
        /// </summary>
        private static LambdaCompiler/*!*/ CreateStaticLambdaCompiler(Compiler compiler, LambdaExpression lambda, TypeGen tg, string/*!*/ name,
                                                                      Type/*!*/ retType, IList<Type/*!*/>/*!*/ paramTypes,
                                                                      IList<string> paramNames, ConstantPool constantPool, bool closure) {
            Assert.NotNull(name, retType);

            Type[] parameterTypes = MakeParameterTypeArray(paramTypes, constantPool, closure);

            MethodBuilder mb = tg.TypeBuilder.DefineMethod(name, CompilerHelpers.PublicStatic, retType, parameterTypes);
            LambdaCompiler lc = new LambdaCompiler(compiler, lambda, tg, mb, mb.GetILGenerator(), parameterTypes, constantPool, closure);
            if (tg.ContextField != null) {
                lc.ContextSlot = new StaticFieldSlot(tg.ContextField);
            }

            if (paramNames != null) {
                // parameters are index from 1, with constant pool we need to skip the first arg
                int startIndex = (constantPool != null || closure) ? 2 : 1;
                for (int i = 0; i < paramNames.Count; i++) {
                    mb.DefineParameter(i + startIndex, ParameterAttributes.None, paramNames[i]);
                }
            }
            return lc;
        }

        /// <summary>
        /// This creates compiler designed only for type gen.
        /// Once TypeGen removes its dependency on LambdaCompiler, this will go away.
        /// </summary>
        internal static LambdaCompiler/*!*/ CreateLambdaCompiler(Compiler compiler, LambdaExpression lambda, TypeGen/*!*/ tg, MethodBase/*!*/ mb, ILGenerator/*!*/ il, Type/*!*/[]/*!*/ parameterTypes) {
            LambdaCompiler lc = new LambdaCompiler(compiler, lambda, tg, mb, il, parameterTypes, null, false);
            if (tg.ContextField != null) {
                lc.ContextSlot = new StaticFieldSlot(tg.ContextField);
            }
            return lc;
        }

        #endregion

        #region Utilities

        [Conditional("DEBUG")]
        private static void NoteCompilerCreation(MethodBase mi) {
            string name = mi.Name;

            for (int i = 0; i < name.Length; i++) {
                if (!Char.IsLetter(name[i]) && name[i] != '.') {
                    name = name.Substring(0, i);
                    break;
                }
            }

            PerfTrack.NoteEvent(PerfTrack.Categories.Compiler, "Compiler " + name);
        }

        #endregion
    }
}
