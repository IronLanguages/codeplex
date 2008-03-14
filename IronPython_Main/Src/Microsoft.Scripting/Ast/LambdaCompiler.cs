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
        /// </summary>
        private Compiler _compiler;

        /// <summary>
        /// The lambda information for the lambda being compiled.
        /// This may be null in the cases where Compiler is still being
        /// used outside of the purpose of compiling the ASTs (sometimes
        /// it is used to generate odd dynamic method here and there,
        /// those will go away eventually and this field will be required)
        /// </summary>
        private LambdaInfo _info;

        private readonly ILGen _ilg;
        private CodeGenOptions _options;
        private readonly TypeGen _typeGen;
        private ScopeAllocator _allocator;

        private readonly MethodBase _method;
        private MethodInfo _methodToOverride;

        private readonly ListStack<Targets> _targets = new ListStack<Targets>();
        private readonly List<Slot> _freeSlots = new List<Slot>();

        private Nullable<ReturnBlock> _returnBlock;

        // Key slots
        private EnvironmentSlot _environmentSlot;   // reference to function's own environment
        private Slot _contextSlot;                  // code context

        // Runtime line # tracking
        private Slot _currentLineSlot;              // used to track the current line # at runtime
        private int _currentLine;                   // last line number emitted to avoid dupes

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

        private bool _generator;                    // true if emitting generator, false otherwise
        private Slot _gotoRouter;                   // Slot that stores the number of the label to go to.

        private const int FinallyExitsNormally = 0;
        private const int BranchForReturn = 1;
        private const int BranchForBreak = 2;
        private const int BranchForContinue = 3;

        private LambdaCompiler(TypeGen typeGen, MethodBase/*!*/ mi, ILGenerator/*!*/ ilg, IList<Type>/*!*/ paramTypes,
            ConstantPool constantPool, bool closure) {

            Contract.Requires(constantPool == null || mi.IsStatic, "constantPool");

            _typeGen = typeGen;
            _method = mi;

            // Create the ILGen instance, debug or not
#if !SILVERLIGHT
            if (Snippets.Shared.ILDebug) {
                _ilg = CreateDebugILGen(ilg, mi, paramTypes);
            } else {
                _ilg = new ILGen(ilg);
            }
#else
            _ilg = new ILGen(ilg);
#endif

            // Create the argument array
            int thisOffset = mi.IsStatic ? 0 : 1;
            _argumentSlots = new Slot[paramTypes.Count];
            for (int i = 0; i < _argumentSlots.Length; i++) {
                _argumentSlots[i] = new ArgSlot(i + thisOffset, paramTypes[i], this);
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

            EmitLineInfo = ScriptDomainManager.Options.DynamicStackTraceSupport;
            NoteCompilerCreation(mi);
        }

        public override string ToString() {
            return _method.ToString();
        }

        #region Properties

        private Compiler Compiler {
            get {
                return _compiler;
            }
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
        /// True if line information should be tracked during code execution to provide
        /// runtime line-information in non-debug builds, false otherwise.
        /// </summary>
        internal bool EmitLineInfo {
            get {
                return (_options & CodeGenOptions.EmitLineInfo) != 0;
            }
            set {
                if (value) {
                    _options |= CodeGenOptions.EmitLineInfo;
                } else {
                    _options &= ~CodeGenOptions.EmitLineInfo;
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

        // TODO: Remove !!!
        internal MethodInfo MethodToOverride {
            get {
                return _methodToOverride;
            }
            set {
                _methodToOverride = value;
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

        internal ScopeAllocator Allocator {
            get {
                Debug.Assert(_allocator != null);
                return _allocator;
            }
            set { _allocator = value; }
        }

        internal bool HasAllocator {
            get {
                return _allocator != null;
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
                Contract.RequiresNotNull(value, "value");
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

        private Nullable<Label> BlockContinueLabel {
            get {
                if (_targets.Count == 0) return Targets.NoLabel;
                Targets t = _targets.Peek();
                return t.continueLabel;
            }
        }

        internal bool IsGenerator {
            get {
                return _generator;
            }
            private set {
                _generator = value;
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
            // 1. Analyze
            AnalyzedTree at = AnalyzeLambda(lambda);

            // 2. Create the Compiler
            Compiler tc = new Compiler(at);

            // 3. Create the lambda compiler
            LambdaCompiler cg = CreateDynamicLambdaCompiler("Initialize", typeof(object), new Type[] { typeof(CodeContext) }, source);

            cg.ContextSlot = cg.GetLambdaArgumentSlot(0);
            cg.Allocator = CompilerHelpers.CreateFrameAllocator();
            cg.InitializeCompilerAndLambda(tc, lambda);

            // 4. Generate code
            cg.EnvironmentSlot = new EnvironmentSlot(
                new PropertySlot(
                    new PropertySlot(cg.ContextSlot,
                        typeof(CodeContext).GetProperty("Scope")),
                    typeof(Scope).GetProperty("Dict"))
                );

            LambdaInfo li = at.GetLambdaInfo(lambda);
            cg.EmitFunctionImplementation(li);

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
            // 1. Analyze
            AnalyzedTree at = AnalyzeLambda(lambda);

            // 2. Create The Compiler
            Compiler tc = new Compiler(at);

            // 3. Create signature
            List<Type> types;
            List<SymbolId> names;
            string name;
            ComputeSignature(lambda, out types, out names, out name);

            // 4. Create lambda compiler
            LambdaCompiler c = CreateDynamicLambdaCompiler(name, lambda.ReturnType, types, null);
            c.Allocator = CompilerHelpers.CreateLocalStorageAllocator(null, c);

            // 5. Initialize the compiler
            c.InitializeCompilerAndLambda(tc, lambda);

            // 6. Emit
            EmitBody(c, at.GetLambdaInfo(lambda));

            c.Finish();

            // 7. Return the delegate.
            return (T)(object)c.CreateDelegate(typeof(T));
        }

        /// <summary>
        /// Compiler entry point, used by OptimizedModuleGenerator
        /// </summary>
        internal void GenerateLambda(LambdaExpression lambda) {
            if (_source == null) {
                throw new InvalidOperationException("Must have source unit.");
            }

            // 1. Analyze
            AnalyzedTree at = AnalyzeLambda(lambda);

            // 2. Finish initialization
            InitializeCompilerAndLambda(new Compiler(at), lambda);

            // 3. Generate the code.
            EmitStackTraceTryBlockStart();

            // 4. Emit the actual body
            EmitBody(this, at.GetLambdaInfo(lambda));

            string displayName = _source.GetSymbolDocument(lambda.Start.Line) ?? lambda.Name;

            EmitStackTraceFaultBlock(lambda.Name, displayName);
        }

        #endregion

        private static AnalyzedTree AnalyzeLambda(LambdaExpression lambda) {
            DumpBlock(lambda);

            ForestRewriter.Rewrite(lambda);
            AnalyzedTree at = ClosureBinder.Bind(lambda);
            FlowChecker.Check(lambda);

            DumpBlock(lambda);

            return at;
        }

        [Conditional("DEBUG")]
        private static void DumpBlock(LambdaExpression lambda) {
#if DEBUG
            AstWriter.Dump(lambda);
#endif
        }

        private void PushExceptionBlock(TargetBlockType type, Slot returnFlag) {
            if (_targets.Count == 0) {
                _targets.Push(new Targets(Targets.NoLabel, Targets.NoLabel, type, returnFlag, null));
            } else {
                Targets t = _targets.Peek();
                _targets.Push(new Targets(t.breakLabel, t.continueLabel, type, returnFlag ?? t.finallyReturns, null));
            }
        }

        private void PushTryBlock() {
            PushExceptionBlock(TargetBlockType.Try, null);
        }

        private void PushTargets(Nullable<Label> breakTarget, Nullable<Label> continueTarget, Expression expression) {
            if (_targets.Count == 0) {
                _targets.Push(new Targets(breakTarget, continueTarget, BlockType, null, expression));
            } else {
                Targets t = _targets.Peek();
                TargetBlockType bt = t.BlockType;
                if (bt == TargetBlockType.Finally) {
                    bt = TargetBlockType.LoopInFinally;
                }
                _targets.Push(new Targets(breakTarget, continueTarget, bt, t.finallyReturns, expression));
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
        private void CheckAndPushTargets(Expression expression) {
            for (int i = _targets.Count - 1; i >= 0; i--) {
                if (_targets[i].expression == expression) {
                    PushTargets(_targets[i].breakLabel, _targets[i].continueLabel, null);
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
                    if (t.breakLabel.HasValue)
                        Emit(OpCodes.Br, t.breakLabel.Value);
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

                        if (_targets[i].BlockType == TargetBlockType.LoopInFinally)
                            break;
                    }

                    if (finallyIndex == -1) {
                        if (t.breakLabel.HasValue)
                            Emit(OpCodes.Leave, t.breakLabel.Value);
                        else
                            throw new InvalidOperationException();
                    } else {
                        if (!_targets[finallyIndex].leaveLabel.HasValue)
                            _targets[finallyIndex].leaveLabel = DefineLabel();

                        EmitInt(LambdaCompiler.BranchForBreak);
                        _targets[finallyIndex].finallyReturns.EmitSet(this);

                        Emit(OpCodes.Leave, _targets[finallyIndex].leaveLabel.Value);
                    }
                    break;
                case TargetBlockType.Finally:
                    EmitInt(LambdaCompiler.BranchForBreak);
                    t.finallyReturns.EmitSet(this);
                    Emit(OpCodes.Endfinally);
                    break;
            }
        }

        private void EmitContinue() {
            Targets t = _targets.Peek();
            switch (t.BlockType) {
                default:
                case TargetBlockType.Normal:
                case TargetBlockType.LoopInFinally:
                    if (t.continueLabel.HasValue)
                        Emit(OpCodes.Br, t.continueLabel.Value);
                    else
                        throw new InvalidOperationException();
                    break;
                case TargetBlockType.Try:
                case TargetBlockType.Else:
                case TargetBlockType.Catch:
                    if (t.continueLabel.HasValue)
                        Emit(OpCodes.Leave, t.continueLabel.Value);
                    else
                        throw new InvalidOperationException();
                    break;
                case TargetBlockType.Finally:
                    EmitInt(LambdaCompiler.BranchForContinue);
                    t.finallyReturns.EmitSet(this);
                    Emit(OpCodes.Endfinally);
                    break;
            }
        }

        private void EmitReturn() {
            int finallyIndex = -1;
            switch (BlockType) {
                default:
                case TargetBlockType.Normal:
                    Emit(OpCodes.Ret);
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
                        _returnBlock.Value.returnValue.EmitSet(this);
                    }

                    if (finallyIndex == -1) {
                        // emit the real return
                        Emit(OpCodes.Leave, _returnBlock.Value.returnStart);
                    } else {
                        // need to leave into the inner most finally block,
                        // the finally block will fall through and check
                        // the return value.
                        if (!_targets[finallyIndex].leaveLabel.HasValue)
                            _targets[finallyIndex].leaveLabel = DefineLabel();

                        EmitInt(LambdaCompiler.BranchForReturn);
                        _targets[finallyIndex].finallyReturns.EmitSet(this);

                        Emit(OpCodes.Leave, _targets[finallyIndex].leaveLabel.Value);
                    }
                    break;
                case TargetBlockType.LoopInFinally:
                case TargetBlockType.Finally: {
                        Targets t = _targets.Peek();
                        EnsureReturnBlock();
                        if (CompilerHelpers.GetReturnType(_method) != typeof(void)) {
                            _returnBlock.Value.returnValue.EmitSet(this);
                        }
                        // Assert check ensures that those who pushed the block with finallyReturns as null 
                        // should not yield in their lambdas.
                        Debug.Assert(t.finallyReturns != null);
                        EmitInt(LambdaCompiler.BranchForReturn);
                        t.finallyReturns.EmitSet(this);
                        Emit(OpCodes.Endfinally);
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
            Contract.RequiresNotNull(from, "from");
            Contract.RequiresNotNull(to, "to");

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
            Contract.RequiresNotNull(type, "type");
            Debug.Assert(typeof(void).IsValueType);

            if (type == typeof(int)) {
                EmitCall(typeof(RuntimeHelpers), "Int32ToObject");
            } else if (type == typeof(bool)) {
                EmitCall(typeof(RuntimeHelpers), "BooleanToObject");
            } else {
                _ilg.EmitBoxing(type);
            }
        }

        private void EmitReturnValue() {
            EnsureReturnBlock();
            if (CompilerHelpers.GetReturnType(_method) != typeof(void)) {
                _returnBlock.Value.returnValue.EmitGet(this);
            }
        }

        private void EmitReturn(Expression expr) {
            if (_generator) {
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

            EmitInt(0);
            EmitReturn();
        }

        private void EmitYield(Expression expr, YieldTarget target) {
            Contract.RequiresNotNull(expr, "expr");

            EmitSetGeneratorReturnValue(expr);
            EmitUpdateGeneratorLocation(target.Index);

            // Mark that we are yielding, which will ensure we skip
            // all of the finally bodies that are on the way to exit

            EmitInt(GotoRouterYielding);
            GotoRouter.EmitSet(this);

            EmitInt(1);
            EmitReturn();

            MarkLabel(target.EnsureLabel(this));
            // Reached the routing destination, set router to GotoRouterNone
            EmitInt(GotoRouterNone);
            GotoRouter.EmitSet(this);
        }

        private void EmitSetGeneratorReturnValue(Expression expr) {
            GetLambdaArgumentSlot(1).EmitGet(this);
            EmitExpressionAsObjectOrNull(expr);
            Emit(OpCodes.Stind_Ref);
        }

        private void EmitUpdateGeneratorLocation(int index) {
            GetLambdaArgumentSlot(0).EmitGet(this);
            EmitInt(index);
            EmitFieldSet(typeof(Generator).GetField("location"));
        }

        private void EmitGetGeneratorLocation() {
            GetLambdaArgumentSlot(0).EmitGet(this);
            EmitFieldGet(typeof(Generator), "location");
        }

        internal void EmitUninitialized() {
            EmitFieldGet(typeof(Uninitialized), "Instance");
        }

        private void EmitPosition(SourceLocation start, SourceLocation end) {
            if (_emitDebugSymbols) {

                Debug.Assert(start != SourceLocation.Invalid);
                Debug.Assert(end != SourceLocation.Invalid);

                if (start == SourceLocation.None || end == SourceLocation.None) {
                    return;
                }
                Debug.Assert(start.Line > 0 && end.Line > 0);

                MarkSequencePoint(
                    start.Line, start.Column,
                    end.Line, end.Column
                );

                Emit(OpCodes.Nop);
            }

            EmitCurrentLine(start.Line);
        }

        private void EmitSequencePointNone() {
            if (_emitDebugSymbols) {
                MarkSequencePoint(
                    SourceLocation.None.Line, SourceLocation.None.Column,
                    SourceLocation.None.Line, SourceLocation.None.Column
                );
            }
        }

        internal Slot GetLocalTmp(Type type) {
            Contract.RequiresNotNull(type, "type");

            for (int i = 0; i < _freeSlots.Count; i++) {
                Slot slot = _freeSlots[i];
                if (slot.Type == type) {
                    _freeSlots.RemoveAt(i);
                    return slot;
                }
            }

            return new LocalSlot(DeclareLocal(type), this);
        }

        internal Slot GetNamedLocal(Type type, string name) {
            Contract.RequiresNotNull(type, "type");
            Contract.RequiresNotNull(name, "name");

            LocalBuilder lb = DeclareLocal(type);
            if (_emitDebugSymbols) lb.SetLocalSymInfo(name);
            return new LocalSlot(lb, this);
        }

        internal void FreeLocalTmp(Slot slot) {
            if (slot != null) {
                Debug.Assert(!_freeSlots.Contains(slot));
                _freeSlots.Add(slot);
            }
        }

        private void EmitGet(Slot slot, SymbolId name, bool check) {
            Debug.Assert(slot != null, "slot");

            slot.EmitGet(this);
            if (check) {
                slot.EmitCheck(this, name);
            }
        }

        private void EmitGetCurrentLine() {
            if (_currentLineSlot != null) {
                _currentLineSlot.EmitGet(this);
            } else {
                EmitInt(0);
            }
        }

        private void EmitCurrentLine(int line) {
            if (!EmitLineInfo || _source == null) {
                return;
            }

            line = _source.MapLine(line);
            if (line != _currentLine && line != SourceLocation.None.Line) {
                EnsureCurrentLineSlot();

                EmitInt(_currentLine = line);
                _currentLineSlot.EmitSet(this);
            }
        }

        private void EmitCurrentLine(Slot line) {
            if (!EmitLineInfo || _source == null) {
                return;
            }

            EnsureCurrentLineSlot();
            _currentLineSlot.EmitSet(this, line);
        }

        private void EnsureCurrentLineSlot() {
            if (_currentLineSlot == null) {
                _currentLineSlot = GetNamedLocal(typeof(int), "$line");
            }
        }

        private void EnsureReturnBlock() {
            if (!_returnBlock.HasValue) {
                ReturnBlock val = new ReturnBlock();

                if (CompilerHelpers.GetReturnType(_method) != typeof(void)) {
                    val.returnValue = GetNamedLocal(CompilerHelpers.GetReturnType(_method), "retval");
                }
                val.returnStart = DefineLabel();

                _returnBlock = val;
            }
        }

        internal void Finish() {
            Debug.Assert(_targets.Count == 0);

            if (_returnBlock.HasValue) {
                MarkLabel(_returnBlock.Value.returnStart);
                if (CompilerHelpers.GetReturnType(_method) != typeof(void)) {
                    _returnBlock.Value.returnValue.EmitGet(this);
                }
                Emit(OpCodes.Ret);
            }

            if (_methodToOverride != null) {
                _typeGen.TypeBuilder.DefineMethodOverride((MethodInfo)_method, _methodToOverride);
            }

            if (DynamicMethod) {
                this.CreateDelegateMethodInfo();
            }
        }

        private void EmitEnvironmentOrNull() {
            if (_environmentSlot != null) {
                _environmentSlot.EmitGet(this);
            } else {
                EmitNull();
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
                EmitNull();
            }

            EmitTupleNew(tupleType);
        }

        private void EmitTupleNew(Type tupleType) {
            ConstructorInfo[] cis = tupleType.GetConstructors();
            foreach (ConstructorInfo ci in cis) {
                if (ci.GetParameters().Length != 0) {
                    EmitNew(ci);
                    break;
                }
            }
        }

        // Used only by OptimizedModuleGenerator
        internal void EmitArgGet(int index) {
            Contract.Requires(index >= 0 && index < Int32.MaxValue, "index");

            if (_method == null || !_method.IsStatic) {
                // making room for this
                index++;
            }

            EmitTrueArgGet(index);
        }

        /// <summary>
        /// Emits a symbol id.  
        /// </summary>
        internal void EmitSymbolId(SymbolId id) {
            if (DynamicMethod) {
                EmitInt(id.Id);
                EmitNew(typeof(SymbolId), new Type[] { typeof(int) });
            } else {
                //TODO - This code is Python-centric, investigate perf issues
                //around removing it and consider re-adding a generic version
                //FieldInfo fi = Symbols.GetFieldInfo(id);
                //if (fi != null) {
                //    Emit(OpCodes.Ldsfld, fi);
                //} else {
                _typeGen.EmitIndirectedSymbol(this, id);
            }
        }

        internal void EmitSymbolIdId(SymbolId id) {
            if (DynamicMethod) {
                EmitInt(id.Id);
            } else {
                EmitSymbolId(id);
                Slot slot = GetLocalTmp(typeof(SymbolId));
                slot.EmitSet(this);
                slot.EmitGetAddr(this);
                EmitPropertyGet(typeof(SymbolId), "Id");
                FreeLocalTmp(slot);
            }
        }

        private void EmitType(Type type) {
            Contract.RequiresNotNull(type, "type");

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
            Contract.RequiresNotNull(delegateFunction, "delegateFunction");
            Contract.RequiresNotNull(delegateType, "delegateType");

            if (delegateFunction.Method is DynamicMethod || delegateFunction.ConstantPool.IsBound) {
                Slot method = ConstantPool.AddData(delegateFunction.CreateDelegateMethodInfo(), typeof(MethodInfo));
                Slot data = ConstantPool.AddData(delegateFunction.ConstantPool.Data);

                method.EmitGet(this);                   // method
                Emit(OpCodes.Ldtoken, delegateType);    // delegate (as RuntimeTypeHandler)
                EmitCodeContext();                      // CodeContext
                data.EmitGet(this);                     // constants
                EmitCall(typeof(RuntimeHelpers).GetMethod("CreateDynamicClosure"));
                Emit(OpCodes.Castclass, delegateType);
            } else {
                if (closure) {
                    EmitCodeContext();      // CodeContext
                    EmitNull();             // constant pool
                    EmitNew(typeof(Closure).GetConstructor(new Type[] { typeof(CodeContext), typeof(object[]) }));
                } else {
                    EmitNull();
                }
                Emit(OpCodes.Ldftn, (MethodInfo)delegateFunction.Method);
                Emit(OpCodes.Newobj, (ConstructorInfo)(delegateType.GetMember(".ctor")[0]));
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
                EmitNull();
            } else if ((strVal = value as string) != null) {
                EmitString(strVal);
            } else {
                Slot s = _typeGen.GetOrMakeConstant(value);
                s.EmitGet(this);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal void EmitConstantNoCache(object value) {
            Debug.Assert(!(value is CompilerConstant));

            Type type;
            if (value is SymbolId) {
                EmitSymbolId((SymbolId)value);
            } else if ((type = value as Type) != null) {
                EmitType(type);
            } else {
                if (!_ilg.TryEmitConstant(value)) {
                    EmitConstant(new RuntimeConstant(value));
                }
            }
        }

        private void EmitCompilerConstantCache(CompilerConstant value) {
            Debug.Assert(value != null);
            Debug.Assert(_typeGen != null);
            _typeGen.GetOrMakeCompilerConstant(value).EmitGet(this);
        }

        private void EmitCompilerConstantNoCache(CompilerConstant value) {
            Debug.Assert(value != null);
            if (ConstantPool.IsBound) {
                //TODO cache these so that we use the same slot for the same values
                _constantPool.AddData(value.Create(), value.Type).EmitGet(this);
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

        private int GetArgumentSlotCount() {
            return _argumentSlots.Length;
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
            Contract.RequiresNotNull(delegateType, "delegateType");

            if (ConstantPool.IsBound) {
                return ReflectionUtils.CreateDelegate(
                    CreateDelegateMethodInfo(),
                    delegateType,
                    new Closure(context, _constantPool.Data));
            } else {
                MethodInfo method = CreateDelegateMethodInfo();
                if (context != null) {
                    return ReflectionUtils.CreateDelegate(method, delegateType, new Closure(context, null));
                } else {
                    return ReflectionUtils.CreateDelegate(method, delegateType);
                }
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
        private LambdaCompiler CreateLambdaCompiler(string name, Type retType, IList<Type> paramTypes, string[] paramNames,
            ConstantPool constantPool, bool closure) {
            Contract.RequiresNotNullItems(paramTypes, "paramTypes");

            LambdaCompiler lc;
            if (DynamicMethod) {
                lc = CreateDynamicLambdaCompiler(name, retType, paramTypes, paramNames, constantPool, closure, _source);
            } else {
                lc = CreateStaticLambdaCompiler(_typeGen, name, retType, paramTypes, paramNames, constantPool, closure);
            }

            lc.SetDebugSymbols(_source, _emitDebugSymbols);

            return lc;
        }

        private void EndExceptionBlock() {
            if (_targets.Count > 0) {
                Targets t = _targets.Peek();
                Debug.Assert(t.BlockType != TargetBlockType.LoopInFinally);
                if (t.BlockType == TargetBlockType.Finally && t.leaveLabel.HasValue) {
                    MarkLabel(t.leaveLabel.Value);
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

        #region ILGen forwards - will go away

        private void BeginCatchBlock(Type exceptionType) {
            _ilg.BeginCatchBlock(exceptionType);
        }

        private Label BeginExceptionBlock() {
            return _ilg.BeginExceptionBlock();
        }

        private void BeginFaultBlock() {
            _ilg.BeginFaultBlock();
        }

        private void BeginFinallyBlock() {
            _ilg.BeginFinallyBlock();
        }

        internal LocalBuilder DeclareLocal(Type localType) {
            return _ilg.DeclareLocal(localType);
        }

        internal Label DefineLabel() {
            return _ilg.DefineLabel();
        }

        internal void Emit(OpCode opcode) {
            _ilg.Emit(opcode);
        }

        internal void Emit(OpCode opcode, ConstructorInfo con) {
            _ilg.Emit(opcode, con);
        }

        internal void Emit(OpCode opcode, FieldInfo field) {
            _ilg.Emit(opcode, field);
        }

        internal void Emit(OpCode opcode, int arg) {
            _ilg.Emit(opcode, arg);
        }

        internal void Emit(OpCode opcode, Label label) {
            _ilg.Emit(opcode, label);
        }

        private void Emit(OpCode opcode, Label[] labels) {
            _ilg.Emit(opcode, labels);
        }

        internal void Emit(OpCode opcode, LocalBuilder local) {
            _ilg.Emit(opcode, local);
        }

        internal void Emit(OpCode opcode, MethodInfo meth) {
            _ilg.Emit(opcode, meth);
        }

#if !SILVERLIGHT
        internal void Emit(OpCode opcode, SignatureHelper signature) {
            _ilg.Emit(opcode, signature);
        }
#endif

        internal void Emit(OpCode opcode, Type cls) {
            _ilg.Emit(opcode, cls);
        }

        internal void MarkLabel(Label loc) {
            _ilg.MarkLabel(loc);
        }

        [Conditional("DEBUG")]
        private void EmitDebugMarker(string marker) {
            _ilg.EmitDebugWriteLine(marker);
        }

        internal void EmitTrueArgGet(int index) {
            _ilg.EmitLoadArg(index);
        }

        internal void EmitArgAddr(int index) {
            _ilg.EmitLoadArgAddress(index);
        }

        internal void EmitPropertyGet(Type type, string name) {
            _ilg.EmitPropertyGet(type, name);
        }

        internal void EmitPropertyGet(PropertyInfo pi) {
            _ilg.EmitPropertyGet(pi);
        }

        internal void EmitPropertySet(Type type, string name) {
            _ilg.EmitPropertySet(type, name);
        }

        internal void EmitPropertySet(PropertyInfo pi) {
            _ilg.EmitPropertySet(pi);
        }

        internal void EmitFieldAddress(FieldInfo fi) {
            _ilg.EmitFieldAddress(fi);
        }

        private void EmitFieldGet(Type type, String name) {
            _ilg.EmitFieldGet(type, name);
        }

        internal void EmitFieldGet(FieldInfo fi) {
            _ilg.EmitFieldGet(fi);
        }

        internal void EmitFieldSet(FieldInfo fi) {
            _ilg.EmitFieldSet(fi);
        }

        internal void EmitNew(ConstructorInfo ci) {
            _ilg.EmitNew(ci);
        }

        internal void EmitNew(Type type, Type[] paramTypes) {
            _ilg.EmitNew(type, paramTypes);
        }

        internal void EmitCall(MethodInfo mi) {
            _ilg.EmitCall(mi);
        }

        internal void EmitCall(Type type, String name) {
            _ilg.EmitCall(type, name);
        }

        private void EmitCall(Type type, String name, Type[] paramTypes) {
            _ilg.EmitCall(type, name, paramTypes);
        }

        /// <summary>
        /// Emits a Ldind* instruction for the appropriate type
        /// </summary>
        internal void EmitLoadValueIndirect(Type type) {
            _ilg.EmitLoadValueIndirect(type);
        }

        /// <summary>
        /// Emits a Stind* instruction for the appropriate type.
        /// </summary>
        internal void EmitStoreValueIndirect(Type type) {
            _ilg.EmitStoreValueIndirect(type);
        }

        /// <summary>
        /// Emits the Ldelem* instruction for the appropriate type
        /// </summary>
        /// <param name="type"></param>
        private void EmitLoadElement(Type type) {
            _ilg.EmitLoadElement(type);
        }

        /// <summary>
        /// Emits a Stelem* instruction for the appropriate type.
        /// </summary>
        internal void EmitStoreElement(Type type) {
            _ilg.EmitStoreElement(type);
        }

        private void EmitNull() {
            _ilg.EmitNull();
        }

        internal void EmitString(string value) {
            _ilg.EmitString(value);
        }

        private void EmitBoolean(bool value) {
            _ilg.EmitBoolean(value);
        }

        internal void EmitInt(int value) {
            _ilg.EmitInt(value);
        }

        private void EmitMissingValue(Type type) {
            _ilg.EmitMissingValue(type);
        }

        /// <summary>
        /// Emits an array of values of count size.  The items are emitted via the callback
        /// which is provided with the current item index to emit.
        /// </summary>
        private void EmitArray(Type elementType, int count, EmitArrayHelper emit) {
            Contract.RequiresNotNull(elementType, "elementType");
            Contract.RequiresNotNull(emit, "emit");
            Contract.Requires(count >= 0, "count", "Count must be non-negative.");

            _ilg.EmitArray(elementType, count, emit);
        }

        private void EmitArray(Type arrayType) {
            Contract.RequiresNotNull(arrayType, "arrayType");

            _ilg.EmitArray(arrayType);
        }

        #endregion

        #region IL Debugging Support

#if !SILVERLIGHT
        private static DebugILGen/*!*/ CreateDebugILGen(ILGenerator/*!*/ il, MethodBase/*!*/ method, IList<Type>/*!*/ paramTypes) {
            TextWriter txt = new StreamWriter(Snippets.Shared.GetMethodILDumpFile(method));
            DebugILGen dig = new DebugILGen(il, txt);

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

        private bool CanUseFastSite() {
            // TypeGen is required for fast sites.
            if (_typeGen == null) {
                return false;
            }

            // Fast sites are disabled for dynamic methods
            if (DynamicMethod) {
                return false;
            }

            // Fast sites only possible with global constext
            if (!(this.ContextSlot is StaticFieldSlot)) {
                return false;
            }

            return true;
        }

        internal Slot CreateDynamicSite(DynamicAction action, Type[] siteTypes, out bool fast) {
            object site;
            if (fast = CanUseFastSite()) {
                // Use fast dynamic site (with cached CodeContext)
                Type fastSite = DynamicSiteHelpers.MakeFastDynamicSiteType(siteTypes);
                site = DynamicSiteHelpers.MakeFastSite(null, action, fastSite);
            } else {
                Type siteType = DynamicSiteHelpers.MakeDynamicSiteType(siteTypes);
                site = DynamicSiteHelpers.MakeSite(action, siteType);
            }

            return ConstantPool.AddData(site);
        }

        private Slot GetTemporarySlot(Type type) {
            Slot temp;

            if (IsGenerator) {
                temp = _allocator.GetGeneratorTemp();
                if (type != typeof(object)) {
                    temp = new CastSlot(temp, type);
                }
            } else {
                temp = GetLocalTmp(type);
            }
            return temp;
        }

        private Slot GetVariableSlot(Variable variable) {
            return _info.References[variable].Slot;
        }

        internal void InitializeCompilerAndLambda(Compiler tc, LambdaExpression lambda) {
            Debug.Assert(tc != null);
            Debug.Assert(lambda != null);

            _compiler = tc;
            _info = GetLambdaInfo(lambda);
        }

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
            Debug.Assert(_info != null);
            return _info.TryGetTsi(node);
        }

        private YieldTarget GetYieldTarget(YieldStatement node) {
            Debug.Assert(_info != null);
            return _info.TryGetYieldTarget(node);
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
        internal static LambdaCompiler/*!*/ CreateDynamicLambdaCompiler(string/*!*/ name, Type/*!*/ returnType,
            IList<Type/*!*/>/*!*/ parameterTypes, SourceUnit source) {
            return CreateDynamicLambdaCompiler(name, returnType, parameterTypes, null, new ConstantPool(), false, source);
        }

        /// <summary>
        /// Creates a compiler backed by dynamic method. Sometimes (when debugging is required) the dynamic
        /// method is actually a 'fake' dynamic method and is backed by static type created specifically for
        /// the one method
        /// </summary>
        private static LambdaCompiler/*!*/ CreateDynamicLambdaCompiler(string/*!*/ methodName, Type/*!*/ returnType,
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
                lc = CreateStaticLambdaCompiler(typeGen, methodName, returnType, paramTypes, paramNames, constantPool, closure);

                // emit symbols iff we have a source unit (and are in debug mode, see GenerateStaticMethod):
                lc.SetDebugSymbols(source, emitSymbols);
            } else {
                Type[] parameterTypes = MakeParameterTypeArray(paramTypes, constantPool, closure);
                DynamicMethod target = Snippets.Shared.CreateDynamicMethod(methodName, returnType, parameterTypes);
                lc = new LambdaCompiler(null, target, target.GetILGenerator(), parameterTypes, constantPool, closure);

                // emits line number setting instructions if source unit available:
                if (debugMode) {
                    lc.SetDebugSymbols(source, false);
                    lc.EmitLineInfo = true; // TODO: ??
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
        private static LambdaCompiler/*!*/ CreateStaticLambdaCompiler(TypeGen tg, string/*!*/ name, Type/*!*/ retType, IList<Type/*!*/>/*!*/ paramTypes,
                                                                      IList<string> paramNames, ConstantPool constantPool, bool closure) {
            Assert.NotNull(name, retType);

            Type[] parameterTypes = MakeParameterTypeArray(paramTypes, constantPool, closure);

            MethodBuilder mb = tg.TypeBuilder.DefineMethod(name, CompilerHelpers.PublicStatic, retType, parameterTypes);
            LambdaCompiler lc = new LambdaCompiler(tg, mb, mb.GetILGenerator(), parameterTypes, constantPool, closure);
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
        internal static LambdaCompiler/*!*/ CreateLambdaCompiler(TypeGen/*!*/ tg, MethodBase/*!*/ mb, ILGenerator/*!*/ il, Type/*!*/[]/*!*/ parameterTypes) {
            LambdaCompiler lc = new LambdaCompiler(tg, mb, il, parameterTypes, null, false);
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
