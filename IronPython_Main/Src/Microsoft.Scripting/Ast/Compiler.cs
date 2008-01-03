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
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Generation;
using Microsoft.Contracts;

namespace Microsoft.Scripting.Ast {

    // TODO: Should move to Generation?

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")] // TODO: fix
    public partial class Compiler : IDisposable {

        struct ReturnBlock {
            internal Slot returnValue;
            internal Label returnStart;
        }

        private readonly ILGen _ilg;

        private CodeGenOptions _options;
        private readonly TypeGen _typeGen;
        private readonly ISymbolDocumentWriter _debugSymbolWriter;
        private ScopeAllocator _allocator;

        private readonly MethodBase _method;
        private MethodInfo _methodToOverride;

        private readonly ListStack<Targets> _targets = new ListStack<Targets>();
        private readonly List<Slot> _freeSlots = new List<Slot>();

        private IList<Label> _yieldLabels;
        private Nullable<ReturnBlock> _returnBlock;

        // TODO: Where does this belong?
        private Dictionary<CodeBlock, Compiler> _codeBlockImplementations;

        // Key slots
        private EnvironmentSlot _environmentSlot;   // reference to function's own environment
        private Slot _contextSlot;                  // code context
        private Slot _paramsSlot;                   // slot for the parameter array, if any

        // Runtime line # tracking
        private Slot _currentLineSlot;              // used to track the current line # at runtime
        private int _currentLine;                   // last line number emitted to avoid dupes

        private Slot[] _argumentSlots;
        private CompilerContext _context;
        private ActionBinder _binder;

        private readonly ConstantPool _constantPool;

        private bool _generator;                    // true if emitting generator, false otherwise
        private Slot _gotoRouter;                   // Slot that stores the number of the label to go to.

        internal const int FinallyExitsNormally = 0;
        internal const int BranchForReturn = 1;
        internal const int BranchForBreak = 2;
        internal const int BranchForContinue = 3;

        // This is true if we are emitting code while in an interpreted context.
        // This flag should always be flowed through to other Compiler objects created from this one.
        private bool _interpretedMode = false;

        internal Compiler(TypeGen typeGen, AssemblyGen assemblyGen, MethodBase mi, ILGenerator ilg, IList<Type> paramTypes, ConstantPool constantPool) {
            Contract.Requires(typeGen == null || typeGen.AssemblyGen == assemblyGen, "assemblyGen");
            Contract.Requires(constantPool == null || mi.IsStatic, "constantPool");

            _typeGen = typeGen;
            _method = mi;
            _constantPool = constantPool;

            if (_typeGen == null) {
                DynamicMethod = true;
            } else {
                _debugSymbolWriter = typeGen.AssemblyGen.SymbolWriter;
            }

            ILDebug = assemblyGen.ILDebug;
#if !SILVERLIGHT
            if (ILDebug) {
                _ilg = CreateDebugILGen(ilg, typeGen, mi, paramTypes, _debugSymbolWriter != null);
            } else {
                _ilg = new ILGen(ilg);
            }
#else
            _ilg = new ILGen(ilg);
#endif

            int firstArg;
            if (constantPool != null) {
                Debug.Assert(paramTypes.Count > 0);
                constantPool.SetCodeGen(this, new ArgSlot(0, constantPool.SlotType, this));
                firstArg = 1;
            } else {
                firstArg = 0;
                _constantPool = new ConstantPool();
                _constantPool.SetCodeGen(this, null);
            }

            int thisOffset = !mi.IsStatic ? 1 : 0;

            _argumentSlots = new Slot[paramTypes.Count - firstArg];
            for (int i = 0; i < _argumentSlots.Length; i++) {
                _argumentSlots[i] = new ArgSlot(i + firstArg + thisOffset, paramTypes[i + firstArg], this);
            }

            EmitLineInfo = ScriptDomainManager.Options.DynamicStackTraceSupport;

            _codeBlockImplementations = new Dictionary<CodeBlock, Compiler>();

            NoteCompilerCreation(mi);
        }

        public override string ToString() {
            return _method.ToString();
        }

        #region Properties

        internal ILGen IL {
            get { return _ilg; }
        }

        internal bool DynamicMethod {
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
        /// True if Compiler should output a text file containing the generated IL, false otherwise.
        /// </summary>
        internal bool ILDebug {
            get {
                return (_options & CodeGenOptions.ILDebug) != 0;
            }
            set {
                if (value) {
                    _options |= CodeGenOptions.ILDebug;
                } else {
                    _options &= ~CodeGenOptions.ILDebug;
                }
            }
        }

        /// <summary>
        /// True if Compiler should store all constants in static fields and emit loads of those fields,
        /// false if constants should be emitted and boxed at runtime.
        /// </summary>
        internal bool CacheConstants {
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

        internal bool EmitDebugInfo {
            get { return _debugSymbolWriter != null; }
        }

        public MethodBase Method {
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

        // TODO: fix
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        internal IList<Label> YieldLabels {
            get {
                return _yieldLabels;
            }
            set {
                _yieldLabels = value;
            }
        }

        public IList<Slot> ArgumentSlots {
            get {
                return _argumentSlots;
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

        public Slot ContextSlot {
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

        internal Slot ParamsSlot {
            get {
                return _paramsSlot;
            }
            set {
                _paramsSlot = value;
            }
        }

        internal CompilerContext Context {
            get {
                Debug.Assert(_context != null);
                return _context;
            }
            set {
                _context = value;
                this.Binder = _context.SourceUnit.LanguageContext.Binder;
            }
        }

        internal bool HasContext {
            get { return _context != null; }
        }

        internal ActionBinder Binder {
            get {
                if (_binder == null) {
                    throw new InvalidOperationException("no Binder has been set");
                }
                return _binder;
            }
            set {
                _binder = value;
            }
        }

        internal TargetBlockType BlockType {
            get {
                if (_targets.Count == 0) return TargetBlockType.Normal;
                Targets t = _targets.Peek();
                return t.BlockType;
            }
        }

        internal Nullable<Label> BlockContinueLabel {
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
            set {
                _generator = value;
            }
        }

        internal const int GotoRouterNone = -1;
        internal const int GotoRouterYielding = -2;

        internal Slot GotoRouter {
            get {
                return _gotoRouter;
            }
            set {
                _gotoRouter = value;
            }
        }

        /// <summary>
        /// Returns true if we are attempting to generate code while in interpreted mode;
        /// this changes some variable scoping behavior.
        /// 
        /// TODO: Can this be removed?
        /// </summary>
        internal bool InterpretedMode {
            get {
                return _interpretedMode;
            }
            set {
                _interpretedMode = value;
            }
        }

        #endregion

        internal void PushExceptionBlock(TargetBlockType type, Slot returnFlag) {
            if (_targets.Count == 0) {
                _targets.Push(new Targets(Targets.NoLabel, Targets.NoLabel, type, returnFlag, null));
            } else {
                Targets t = _targets.Peek();
                _targets.Push(new Targets(t.breakLabel, t.continueLabel, type, returnFlag ?? t.finallyReturns, null));
            }
        }

        internal void PushTryBlock() {
            PushExceptionBlock(TargetBlockType.Try, null);
        }

        internal void PushTargets(Nullable<Label> breakTarget, Nullable<Label> continueTarget, Statement statement) {
            if (_targets.Count == 0) {
                _targets.Push(new Targets(breakTarget, continueTarget, BlockType, null, statement));
            } else {
                Targets t = _targets.Peek();
                TargetBlockType bt = t.BlockType;
                if (bt == TargetBlockType.Finally) {
                    bt = TargetBlockType.LoopInFinally;
                }
                _targets.Push(new Targets(breakTarget, continueTarget, bt, t.finallyReturns, statement));
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "type")]
        internal void PopTargets(TargetBlockType type) {
            Targets t = _targets.Pop();
            Debug.Assert(t.BlockType == type);
        }

        internal void PopTargets() {
            _targets.Pop();
        }

        // TODO: Cleanup, hacky!!!
        internal void CheckAndPushTargets(Statement statement) {
            for (int i = _targets.Count - 1; i >= 0; i--) {
                if (_targets[i].statement == statement) {
                    PushTargets(_targets[i].breakLabel, _targets[i].continueLabel, null);
                    return;
                }
            }

            throw new InvalidOperationException("Statement not on the stack");
        }

        internal void EmitBreak() {
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

                        EmitInt(Compiler.BranchForBreak);
                        _targets[finallyIndex].finallyReturns.EmitSet(this);

                        Emit(OpCodes.Leave, _targets[finallyIndex].leaveLabel.Value);
                    }
                    break;
                case TargetBlockType.Finally:
                    EmitInt(Compiler.BranchForBreak);
                    t.finallyReturns.EmitSet(this);
                    Emit(OpCodes.Endfinally);
                    break;
            }
        }

        internal void EmitContinue() {
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
                    EmitInt(Compiler.BranchForContinue);
                    t.finallyReturns.EmitSet(this);
                    Emit(OpCodes.Endfinally);
                    break;
            }
        }

        public void EmitReturn() {
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

                        EmitInt(Compiler.BranchForReturn);
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
                        // should not yield in their blocks.
                        Debug.Assert(t.finallyReturns != null);
                        EmitInt(Compiler.BranchForReturn);
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

        public void EmitCast(Type from, Type to) {
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
        public void EmitBoxing(Type type) {
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

        internal void EmitReturnValue() {
            EnsureReturnBlock();
            if (CompilerHelpers.GetReturnType(_method) != typeof(void)) {
                _returnBlock.Value.returnValue.EmitGet(this);
            }
        }

        internal void EmitReturn(Expression expr) {
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

        internal void EmitReturnInGenerator(Expression expr) {
            EmitSetGeneratorReturnValue(expr);

            EmitInt(0);
            EmitReturn();
        }

        internal void EmitYield(Expression expr, YieldTarget target) {
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
            ArgumentSlots[1].EmitGet(this);
            EmitExpressionAsObjectOrNull(expr);
            Emit(OpCodes.Stind_Ref);
        }

        internal void EmitUpdateGeneratorLocation(int index) {
            ArgumentSlots[0].EmitGet(this);
            EmitInt(index);
            EmitFieldSet(typeof(Generator).GetField("location"));
        }

        internal void EmitGetGeneratorLocation() {
            ArgumentSlots[0].EmitGet(this);
            EmitFieldGet(typeof(Generator), "location");
        }

        internal void EmitUninitialized() {
            EmitFieldGet(typeof(Uninitialized), "Instance");
        }

        internal void EmitPosition(SourceLocation start, SourceLocation end) {
            if (EmitDebugInfo) {

                Debug.Assert(start != SourceLocation.Invalid);
                Debug.Assert(end != SourceLocation.Invalid);

                if (start == SourceLocation.None || end == SourceLocation.None) {
                    return;
                }
                Debug.Assert(start.Line > 0 && end.Line > 0);

                MarkSequencePoint(
                    _debugSymbolWriter,
                    start.Line, start.Column,
                    end.Line, end.Column
                    );

                Emit(OpCodes.Nop);
            }

            EmitCurrentLine(start.Line);
        }

        internal void EmitSequencePointNone() {
            if (EmitDebugInfo) {
                MarkSequencePoint(
                    _debugSymbolWriter,
                    SourceLocation.None.Line, SourceLocation.None.Column,
                    SourceLocation.None.Line, SourceLocation.None.Column
                );
            }
        }

        public Slot GetLocalTmp(Type type) {
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
            if (EmitDebugInfo) lb.SetLocalSymInfo(name);
            return new LocalSlot(lb, this);
        }

        internal void FreeLocalTmp(Slot slot) {
            if (slot != null) {
                Debug.Assert(!_freeSlots.Contains(slot));
                _freeSlots.Add(slot);
            }
        }

        internal Slot DupAndStoreInTemp(Type type) {
            Debug.Assert(type != typeof(void));
            this.Emit(OpCodes.Dup);
            Slot ret = GetLocalTmp(type);
            ret.EmitSet(this);
            return ret;
        }

        internal void SetCustomAttribute(CustomAttributeBuilder cab) {
            MethodBuilder builder = _method as MethodBuilder;
            if (builder != null) {
                builder.SetCustomAttribute(cab);
            }
        }

        internal ParameterBuilder DefineParameter(int position, ParameterAttributes attributes, string strParamName) {
            MethodBuilder builder = _method as MethodBuilder;
            if (builder != null) {
                return builder.DefineParameter(position, attributes, strParamName);
            }
            DynamicMethod dm = _method as DynamicMethod;
            if (dm != null) {
                return dm.DefineParameter(position, attributes, strParamName);
            }

            throw new InvalidOperationException(Resources.InvalidOperation_DefineParameterBakedMethod);
        }

        internal void EmitGet(Slot slot, SymbolId name, bool check) {
            Contract.RequiresNotNull(slot, "slot");

            slot.EmitGet(this);
            if (check) {
                slot.EmitCheck(this, name);
            }
        }

        internal void EmitGetCurrentLine() {
            if (_currentLineSlot != null) {
                _currentLineSlot.EmitGet(this);
            } else {
                EmitInt(0);
            }
        }

        private void EmitCurrentLine(int line) {
            if (!EmitLineInfo || !HasContext) return;

            line = _context.SourceUnit.MapLine(line);
            if (line != _currentLine && line != SourceLocation.None.Line) {
                if (_currentLineSlot == null) {
                    _currentLineSlot = GetNamedLocal(typeof(int), "$line");
                }

                EmitInt(_currentLine = line);
                _currentLineSlot.EmitSet(this);
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

        public void Finish() {
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

        internal void EmitEnvironmentOrNull() {
            if (_environmentSlot != null) {
                _environmentSlot.EmitGet(this);
            } else {
                EmitNull();
            }
        }

        public void EmitThis() {
            if (_method.IsStatic) throw new InvalidOperationException(Resources.InvalidOperation_ThisInStaticMethod);
            //!!! want to confirm this doesn't have a constant pool too
            Emit(OpCodes.Ldarg_0);
        }

        /// <summary>
        /// Emits an array of constant values provided in the given list.  The array
        /// is strongly typed.
        /// </summary>
        public void EmitArray<T>(IList<T> items) {
            Contract.RequiresNotNull(items, "items");

            EmitInt(items.Count);
            Emit(OpCodes.Newarr, typeof(T));
            for (int i = 0; i < items.Count; i++) {
                Emit(OpCodes.Dup);
                EmitInt(i);
                EmitConstant(items[i]);
                EmitStoreElement(typeof(T));
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

        public void EmitArgGet(int index) {
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
        public void EmitSymbolId(SymbolId id) {
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

        public void EmitType(Type type) {
            Contract.RequiresNotNull(type, "type");

            if (!(type is TypeBuilder) && !type.IsGenericParameter && !type.IsVisible) {
                // can't ldtoken on a non-visible type, refer to it via a runtime constant...
                EmitConstant(new RuntimeConstant(type));
            } else {
                _ilg.EmitType(type);
            }
        }

        // Not to be used with virtual methods
        internal void EmitDelegateConstruction(Compiler delegateFunction, Type delegateType) {
            Contract.RequiresNotNull(delegateFunction, "delegateFunction");
            Contract.RequiresNotNull(delegateType, "delegateType");

            if (delegateFunction.Method is DynamicMethod || delegateFunction.ConstantPool.IsBound) {
                Delegate d = delegateFunction.CreateDelegate(delegateType);
                this.ConstantPool.AddData(d).EmitGet(this);
            } else {
                EmitNull();
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
        internal void EmitConstant(object value) {
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
                value.EmitCreation(this);
            }
        }

        internal void EmitTypeError(string format, params object[] args) {
            EmitString(String.Format(format, args));
            EmitCall(typeof(RuntimeHelpers), "SimpleTypeError");
            Emit(OpCodes.Throw);
        }

        internal Slot GetArgumentSlot(int index) {
            return _argumentSlots[index];
        }

        public MethodInfo CreateDelegateMethodInfo() {
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

        public Delegate CreateDelegate(Type delegateType) {
            Contract.RequiresNotNull(delegateType, "delegateType");

            if (ConstantPool.IsBound) {
                return ReflectionUtils.CreateDelegate(CreateDelegateMethodInfo(), delegateType, _constantPool.Data);
            } else {
                return ReflectionUtils.CreateDelegate(CreateDelegateMethodInfo(), delegateType);
            }
        }

        internal Delegate CreateDelegate(Type delegateType, object target) {
            Contract.RequiresNotNull(delegateType, "delegateType");
            Debug.Assert(!ConstantPool.IsBound);

            return ReflectionUtils.CreateDelegate(CreateDelegateMethodInfo(), delegateType, target);
        }

        public Compiler DefineMethod(string name, Type returnType, IList<Type> parameterTypes, string[] parameterNames) {
            return DefineMethod(name, returnType, parameterTypes, parameterNames, null);
        }

        internal Compiler DefineMethod(string name, Type retType, IList<Type> paramTypes, string[] paramNames, ConstantPool constantPool) {
            Contract.RequiresNotNullItems(paramTypes, "paramTypes");
            //Contract.RequiresNotNull(paramNames, "paramNames");

            Compiler res;
            if (!DynamicMethod) {
                res = _typeGen.DefineMethod(name, retType, paramTypes, paramNames, constantPool);
            } else {
                if (CompilerHelpers.NeedDebuggableDynamicCodeGenerator(_context)) {
                    res = CompilerHelpers.CreateDebuggableDynamicCodeGenerator(_context, name, retType, paramTypes, paramNames, constantPool);
                } else {
                    res = CompilerHelpers.CreateDynamicCodeGenerator(name, retType, paramTypes, constantPool);
                }
            }

            if (_context != null) res.Context = _context;
            res.InterpretedMode = _interpretedMode;
            return res;
        }

        internal void EndExceptionBlock() {
            if (_targets.Count > 0) {
                Targets t = _targets.Peek();
                Debug.Assert(t.BlockType != TargetBlockType.LoopInFinally);
                if (t.BlockType == TargetBlockType.Finally && t.leaveLabel.HasValue) {
                    MarkLabel(t.leaveLabel.Value);
                }
            }

            _ilg.EndExceptionBlock();
        }

        internal void MarkSequencePoint(ISymbolDocumentWriter document, int startLine, int startColumn, int endLine, int endColumn) {
            if (_context != null) {
                startLine = _context.SourceUnit.MapLine(startLine);
                endLine = _context.SourceUnit.MapLine(endLine);
            }
            _ilg.MarkSequencePoint(document, startLine, startColumn, endLine, endColumn);
        }

        #region ILGen forwards - will go away

        internal void BeginCatchBlock(Type exceptionType) {
            _ilg.BeginCatchBlock(exceptionType);
        }

        internal Label BeginExceptionBlock() {
            return _ilg.BeginExceptionBlock();
        }

        internal void BeginFaultBlock() {
            _ilg.BeginFaultBlock();
        }

        internal void BeginFinallyBlock() {
            _ilg.BeginFinallyBlock();
        }

        internal LocalBuilder DeclareLocal(Type localType) {
            return _ilg.DeclareLocal(localType);
        }

        public Label DefineLabel() {
            return _ilg.DefineLabel();
        }

        public void Emit(OpCode opcode) {
            _ilg.Emit(opcode);
        }

        internal void Emit(OpCode opcode, byte arg) {
            _ilg.Emit(opcode, arg);
        }

        public void Emit(OpCode opcode, ConstructorInfo con) {
            _ilg.Emit(opcode, con);
        }

        internal void Emit(OpCode opcode, double arg) {
            _ilg.Emit(opcode, arg);
        }

        internal void Emit(OpCode opcode, FieldInfo field) {
            _ilg.Emit(opcode, field);
        }

        internal void Emit(OpCode opcode, float arg) {
            _ilg.Emit(opcode, arg);
        }

        internal void Emit(OpCode opcode, int arg) {
            _ilg.Emit(opcode, arg);
        }

        public void Emit(OpCode opcode, Label label) {
            _ilg.Emit(opcode, label);
        }

        internal void Emit(OpCode opcode, Label[] labels) {
            _ilg.Emit(opcode, labels);
        }

        internal void Emit(OpCode opcode, LocalBuilder local) {
            _ilg.Emit(opcode, local);
        }

        internal void Emit(OpCode opcode, long arg) {
            _ilg.Emit(opcode, arg);
        }

        public void Emit(OpCode opcode, MethodInfo meth) {
            _ilg.Emit(opcode, meth);
        }

        internal void Emit(OpCode opcode, sbyte arg) {
            _ilg.Emit(opcode, arg);
        }

        internal void Emit(OpCode opcode, short arg) {
            _ilg.Emit(opcode, arg);
        }

#if !SILVERLIGHT
        internal void Emit(OpCode opcode, SignatureHelper signature) {
            _ilg.Emit(opcode, signature);
        }
#endif

        internal void Emit(OpCode opcode, string str) {
            _ilg.Emit(opcode, str);
        }

        public void Emit(OpCode opcode, Type cls) {
            _ilg.Emit(opcode, cls);
        }

        public void EmitCall(OpCode opcode, MethodInfo methodInfo, Type[] optionalParameterTypes) {
            _ilg.EmitCall(opcode, methodInfo, optionalParameterTypes);
        }

        public void MarkLabel(Label loc) {
            _ilg.MarkLabel(loc);
        }

        [Conditional("DEBUG")]
        private void EmitDebugMarker(string marker) {
            _ilg.EmitDebugWriteLine(marker);
        }

        [Conditional("DEBUG")]
        internal void EmitAssertNotNull() {
            _ilg.EmitAssertNotNull();
        }

        /// <summary>
        /// asserts the value at the top of the stack is not null
        /// </summary>
        [Conditional("DEBUG")]
        internal void EmitAssertNotNull(string message) {
            _ilg.EmitAssertNotNull(message);
        }

        internal void EmitTrueArgGet(int index) {
            _ilg.EmitLoadArg(index);
        }

        internal void EmitArgAddr(int index) {
            _ilg.EmitLoadArgAddress(index);
        }

        public void EmitPropertyGet(Type type, string name) {
            _ilg.EmitPropertyGet(type, name);
        }

        public void EmitPropertyGet(PropertyInfo pi) {
            _ilg.EmitPropertyGet(pi);
        }

        public void EmitPropertySet(Type type, string name) {
            _ilg.EmitPropertySet(type, name);
        }

        public void EmitPropertySet(PropertyInfo pi) {
            _ilg.EmitPropertySet(pi);
        }

        internal void EmitFieldAddress(FieldInfo fi) {
            _ilg.EmitFieldAddress(fi);
        }

        public void EmitFieldGet(Type type, String name) {
            _ilg.EmitFieldGet(type, name);
        }

        public void EmitFieldGet(FieldInfo fi) {
            _ilg.EmitFieldGet(fi);
        }

        public void EmitFieldSet(FieldInfo fi) {
            _ilg.EmitFieldSet(fi);
        }

        public void EmitNew(ConstructorInfo ci) {
            _ilg.EmitNew(ci);
        }

        public void EmitNew(Type type, Type[] paramTypes) {
            _ilg.EmitNew(type, paramTypes);
        }

        public void EmitCall(MethodInfo mi) {
            _ilg.EmitCall(mi);
        }

        public void EmitCall(Type type, String name) {
            _ilg.EmitCall(type, name);
        }

        public void EmitCall(Type type, String name, Type[] paramTypes) {
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
        internal void EmitLoadElement(Type type) {
            _ilg.EmitLoadElement(type);
        }

        /// <summary>
        /// Emits a Stelem* instruction for the appropriate type.
        /// </summary>
        internal void EmitStoreElement(Type type) {
            _ilg.EmitStoreElement(type);
        }

        public void EmitNull() {
            _ilg.EmitNull();
        }

        public void EmitString(string value) {
            _ilg.EmitString(value);
        }

        public void EmitBoolean(bool value) {
            _ilg.EmitBoolean(value);
        }

        internal void EmitChar(char value) {
            _ilg.EmitChar(value);
        }

        internal void EmitByte(byte value) {
            _ilg.EmitByte(value);
        }

        private void EmitSByte(sbyte value) {
            _ilg.EmitSByte(value);
        }

        private void EmitShort(short value) {
            _ilg.EmitShort(value);
        }

        private void EmitUShort(ushort value) {
            _ilg.EmitUShort(value);
        }

        public void EmitInt(int value) {
            _ilg.EmitInt(value);
        }

        internal void EmitUInt(uint value) {
            _ilg.EmitUInt(value);
        }

        public void EmitUnbox(Type type) {
            _ilg.EmitUnbox(type);
        }

        internal void EmitMissingValue(Type type) {
            _ilg.EmitMissingValue(type);
        }

        /// <summary>
        /// Emits an array of values of count size.  The items are emitted via the callback
        /// which is provided with the current item index to emit.
        /// </summary>
        public void EmitArray(Type elementType, int count, EmitArrayHelper emit) {
            Contract.RequiresNotNull(elementType, "elementType");
            Contract.RequiresNotNull(emit, "emit");
            Contract.Requires(count >= 0, "count", "Count must be non-negative.");

            _ilg.EmitArray(elementType, count, emit);
        }

        #endregion

        #region IL Debugging Support

#if !SILVERLIGHT
        static int count;

        private static DebugILGen CreateDebugILGen(ILGenerator il, TypeGen typeGen, MethodBase method, IList<Type> paramTypes, bool debug) {
            // This ensures that it is not a DynamicMethod
            Debug.Assert(typeGen != null);
            string full_method_name = ((method.DeclaringType != null) ? method.DeclaringType.Name + "." : "") + method.Name;

            string filename = String.Format("gen_{0}_{1}.il", IOUtils.ToValidFileName(full_method_name),
                System.Threading.Interlocked.Increment(ref count));

            string tempFolder = Path.Combine(Path.GetTempPath(), "IronPython");
            Directory.CreateDirectory(tempFolder);
            filename = Path.Combine(tempFolder, filename);

            ISymbolDocumentWriter doc = null;

            if (debug) {
                doc = typeGen.AssemblyGen.ModuleBuilder.DefineDocument(filename,
                    SymbolGuids.LanguageType_ILAssembly,
                    SymbolGuids.LanguageVendor_Microsoft,
                    SymbolGuids.DocumentType_Text);
            }

            TextWriter txt = new StreamWriter(ScriptDomainManager.CurrentManager.PAL.OpenOutputFileStream(filename));
            DebugILGen dig = new DebugILGen(il, txt, doc);

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

        internal Slot GetTemporarySlot(Type type) {
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

        internal void FreeTemporarySlot(Slot temp) {
            if (!IsGenerator) {
                FreeLocalTmp(temp);
            }
        }

        /// <summary>
        /// Returns the Compiler implementing the code block.
        /// Emits the code block implementation if it hasn't been emitted yet.
        /// </summary>
        internal Compiler ProvideCodeBlockImplementation(CodeBlock block, bool hasContextParameter, bool hasThis) {
            Assert.NotNull(block);
            Compiler impl;

            // emit the code block method if it has:
            if (!_codeBlockImplementations.TryGetValue(block, out impl)) {
                impl = CreateMethod(this, block, hasContextParameter, hasThis);
                impl.Binder = _binder;
                impl.EmitFunctionImplementation(block);
                impl.Finish();

                _codeBlockImplementations.Add(block, impl);
            }

            return impl;
        }

        #region IDisposable Members

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
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
